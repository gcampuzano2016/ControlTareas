/* ============================================================================
   Catálogo de horarios: crear, editar y horario propio por persona
   ReporTarea  |  2026-09-07

   PENDIENTE DE EJECUTAR. Correr primero en PRUEBAS.

   ----------------------------------------------------------------------------
   Qué cambia y por qué

   Hasta hoy los perfiles de horario (R_HorarioLaboral) solo se podían crear o
   corregir por SQL a mano: la pantalla ParametrizacionHorarioUsuario.aspx
   únicamente asigna los cuatro que dejó cargados el script consolidado. Este
   script agrega lo que necesita la pantalla nueva ParametrizacionHorario.aspx
   para administrar ese catálogo, y la opción de que una persona tenga un
   horario que es solo suyo.

   El horario propio NO es un modelo nuevo: es un perfil que pertenece a alguien.
   Por eso alcanza con una columna, Id_ResponsableDueno:

       NULL          el perfil es compartido y sale en el combo de asignación
       Cod_Usuario   el perfil es de esa persona y no aparece en el catálogo

   Con eso FN_RTA_ClasificarTramosHorario sigue resolviendo igual que siempre
   (persona -> asignación -> perfil -> detalle del día) y no se toca.

   ----------------------------------------------------------------------------
   Objetos

   Columna nueva   R_HorarioLaboral.Id_ResponsableDueno
   SPs nuevos      Sp_RTA_ListarHorarios
                   Sp_RTA_ObtenerHorario
                   Sp_RTA_GuardarHorario
                   Sp_RTA_CambiarEstadoHorario
                   Sp_RTA_GuardarHorarioPropio
   SPs recreados   Sp_RTA_ListarPerfilesHorario     (excluye los horarios propios)
                   Sp_RTA_ListarUsuariosConHorario  (agrega la columna EsPropio)

   Idempotente: se puede ejecutar varias veces.
   ============================================================================ */

USE [ReporTarea];
GO

PRINT '== Catálogo de horarios: inicio ==';
GO

/* ============================================================================
   1. Columna Id_ResponsableDueno
   ============================================================================ */

IF COL_LENGTH('dbo.R_HorarioLaboral', 'Id_ResponsableDueno') IS NULL
BEGIN
    ALTER TABLE dbo.R_HorarioLaboral
    ADD Id_ResponsableDueno VARCHAR(20) NULL;

    PRINT '1: Columna Id_ResponsableDueno agregada.';
END
ELSE
    PRINT '1: La columna Id_ResponsableDueno ya existe. Sin cambios.';
GO

/* Un solo horario propio por persona. El índice es filtrado: los perfiles
   compartidos (NULL) no entran, así que no chocan entre ellos. */
IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UQ_R_HorarioLaboral_Dueno'
      AND object_id = OBJECT_ID(N'dbo.R_HorarioLaboral')
)
BEGIN
    CREATE UNIQUE INDEX UQ_R_HorarioLaboral_Dueno
    ON dbo.R_HorarioLaboral (Id_ResponsableDueno)
    WHERE Id_ResponsableDueno IS NOT NULL;

    PRINT '1.1: Índice UQ_R_HorarioLaboral_Dueno creado.';
END
ELSE
    PRINT '1.1: El índice UQ_R_HorarioLaboral_Dueno ya existe. Sin cambios.';
GO

/* ============================================================================
   2. Sp_RTA_ListarPerfilesHorario  (recreado)

   Mismo contrato de siempre (Id / Valor). El único cambio es que los horarios
   propios de una persona no salen en el combo de asignación: no son perfiles
   que se le puedan poner a cualquiera.
   ============================================================================ */

IF OBJECT_ID(N'dbo.Sp_RTA_ListarPerfilesHorario', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPerfilesHorario;
GO

CREATE PROCEDURE dbo.Sp_RTA_ListarPerfilesHorario
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CONVERT(VARCHAR(10), IdHorarioLaboral) AS Id,
        Nombre                                 AS Valor
    FROM dbo.R_HorarioLaboral
    WHERE Activo = 1
      AND Id_ResponsableDueno IS NULL
    ORDER BY EsPredeterminado DESC, Nombre;
END;
GO

/* ============================================================================
   3. Sp_RTA_ListarUsuariosConHorario  (recreado)

   Idéntico al anterior salvo la columna EsPropio, que la pantalla de asignación
   necesita para saber si abrir el editor de horario propio en vez del combo.
   ============================================================================ */

IF OBJECT_ID(N'dbo.Sp_RTA_ListarUsuariosConHorario', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarUsuariosConHorario;
GO

CREATE PROCEDURE dbo.Sp_RTA_ListarUsuariosConHorario
    @Filtro VARCHAR(100) = ''
AS
BEGIN
    SET NOCOUNT ON;

    SET @Filtro = LTRIM(RTRIM(ISNULL(@Filtro, '')));

    SELECT
        U.Cod_Usuario,
        U.Nom_Usuario,
        ISNULL(U.Cedula, '')        AS Cedula,
        ISNULL(U.Departamento, '')  AS Departamento,
        ISNULL(U.Empresa, '')       AS Empresa,
        COALESCE(HA.IdHorarioLaboral, P.IdHorarioLaboral, 0) AS IdHorarioLaboral,
        COALESCE(HA.Codigo, P.Codigo, '')                    AS CodigoHorario,
        COALESCE(HA.Nombre, P.Nombre, 'Sin perfil')          AS NombreHorario,
        CASE WHEN A.IdHorarioLaboral IS NULL THEN 1 ELSE 0 END AS EsPredeterminado,
        ISNULL(CONVERT(VARCHAR(10), A.FechaDesde, 103), '')  AS FechaDesde,
        CASE WHEN HA.Id_ResponsableDueno IS NULL THEN 0 ELSE 1 END AS EsPropio
    FROM dbo.R_Usuarios AS U
    OUTER APPLY
    (
        SELECT TOP (1) X.IdHorarioLaboral, X.FechaDesde
        FROM dbo.R_UsuarioHorarioLaboral AS X
        WHERE X.Id_Responsable = U.Cod_Usuario
          AND X.Activo = 1
          AND X.FechaHasta IS NULL
        ORDER BY X.FechaDesde DESC
    ) AS A
    LEFT JOIN dbo.R_HorarioLaboral AS HA
        ON HA.IdHorarioLaboral = A.IdHorarioLaboral
    OUTER APPLY
    (
        SELECT TOP (1) Pre.IdHorarioLaboral, Pre.Codigo, Pre.Nombre
        FROM dbo.R_HorarioLaboral AS Pre
        WHERE Pre.EsPredeterminado = 1
          AND Pre.Activo = 1
        ORDER BY Pre.IdHorarioLaboral
    ) AS P
    WHERE U.Usuario_Estado = 'A'
      AND
      (
          @Filtro = ''
          OR U.Nom_Usuario LIKE '%' + @Filtro + '%'
          OR U.Cod_Usuario LIKE '%' + @Filtro + '%'
          OR ISNULL(U.Cedula, '') LIKE '%' + @Filtro + '%'
      )
    ORDER BY U.Nom_Usuario;
END;
GO

/* ============================================================================
   4. Sp_RTA_ListarHorarios

   La grilla del catálogo. El resumen agrupa los días que comparten franja, así
   que "LUN,MAR,MIE,JUE,VIE 08:30-17:30" sale en una línea en vez de en cinco.

   @IncluirInactivos = 1  muestra también los perfiles apagados
   @IncluirPropios   = 1  muestra también los horarios propios de cada persona
   ============================================================================ */

IF OBJECT_ID(N'dbo.Sp_RTA_ListarHorarios', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarHorarios;
GO

CREATE PROCEDURE dbo.Sp_RTA_ListarHorarios
    @Filtro           VARCHAR(100) = '',
    @IncluirInactivos BIT = 0,
    @IncluirPropios   BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SET @Filtro = LTRIM(RTRIM(ISNULL(@Filtro, '')));

    WITH Det AS
    (
        SELECT
            D.IdHorarioLaboral,
            D.DiaSemana,
            LEFT(D.NombreDia, 3) AS Abrev,
            CONVERT(VARCHAR(5), D.HoraInicio, 108) + '-' +
            CONVERT(VARCHAR(5), D.HoraFin, 108)    AS Franja
        FROM dbo.R_HorarioLaboralDetalle AS D
        WHERE D.Activo = 1
          AND D.EsLaborable = 1
          AND D.HoraInicio IS NOT NULL
          AND D.HoraFin IS NOT NULL
    ),
    Grupo AS
    (
        SELECT IdHorarioLaboral, Franja, MIN(DiaSemana) AS PrimerDia
        FROM Det
        GROUP BY IdHorarioLaboral, Franja
    ),
    GrupoTexto AS
    (
        SELECT
            G.IdHorarioLaboral,
            G.PrimerDia,
            STUFF
            ((
                SELECT ',' + D.Abrev
                FROM Det AS D
                WHERE D.IdHorarioLaboral = G.IdHorarioLaboral
                  AND D.Franja = G.Franja
                ORDER BY D.DiaSemana
                FOR XML PATH(''), TYPE
            ).value('.', 'VARCHAR(200)'), 1, 1, '') + ' ' + G.Franja AS Texto
        FROM Grupo AS G
    )
    SELECT
        H.IdHorarioLaboral,
        H.Codigo,
        H.Nombre,
        CONVERT(INT, H.EsPredeterminado) AS EsPredeterminado,
        CONVERT(INT, H.Activo)           AS Activo,
        CASE WHEN H.Id_ResponsableDueno IS NULL THEN 0 ELSE 1 END AS EsPropio,
        ISNULL(H.Id_ResponsableDueno, '') AS CodigoDueno,
        ISNULL(Dueno.Nom_Usuario, '')     AS NombreDueno,
        ISNULL
        (
            STUFF
            ((
                SELECT '; ' + GT.Texto
                FROM GrupoTexto AS GT
                WHERE GT.IdHorarioLaboral = H.IdHorarioLaboral
                ORDER BY GT.PrimerDia
                FOR XML PATH(''), TYPE
            ).value('.', 'VARCHAR(500)'), 1, 2, ''),
            'Sin días laborables'
        ) AS Resumen,
        (
            SELECT COUNT(*)
            FROM dbo.R_UsuarioHorarioLaboral AS A
            WHERE A.IdHorarioLaboral = H.IdHorarioLaboral
              AND A.Activo = 1
              AND A.FechaHasta IS NULL
        ) AS UsuariosAsignados
    FROM dbo.R_HorarioLaboral AS H
    LEFT JOIN dbo.R_Usuarios AS Dueno
        ON Dueno.Cod_Usuario = H.Id_ResponsableDueno
    WHERE (@IncluirInactivos = 1 OR H.Activo = 1)
      AND (@IncluirPropios = 1 OR H.Id_ResponsableDueno IS NULL)
      AND
      (
          @Filtro = ''
          OR H.Codigo LIKE '%' + @Filtro + '%'
          OR H.Nombre LIKE '%' + @Filtro + '%'
          OR ISNULL(Dueno.Nom_Usuario, '') LIKE '%' + @Filtro + '%'
      )
    ORDER BY H.EsPredeterminado DESC, H.Activo DESC, H.Nombre;
END;
GO

/* ============================================================================
   5. Sp_RTA_ObtenerHorario

   Los siete días de un perfil, para llenar el editor. Devuelve los siete
   siempre, aunque en la tabla falte alguno: la pantalla no tiene por qué
   adivinar huecos. Las horas salen como texto 'HH:mm' porque eso es lo que
   come un input type=time.
   ============================================================================ */

IF OBJECT_ID(N'dbo.Sp_RTA_ObtenerHorario', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ObtenerHorario;
GO

CREATE PROCEDURE dbo.Sp_RTA_ObtenerHorario
    @IdHorarioLaboral INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Semana TABLE
    (
        DiaSemana TINYINT PRIMARY KEY,
        NombreDia VARCHAR(15)
    );

    INSERT INTO @Semana (DiaSemana, NombreDia)
    VALUES (1, 'LUNES'), (2, 'MARTES'), (3, 'MIERCOLES'), (4, 'JUEVES'),
           (5, 'VIERNES'), (6, 'SABADO'), (7, 'DOMINGO');

    SELECT
        S.DiaSemana,
        S.NombreDia,
        CONVERT(INT, ISNULL(D.EsLaborable, 0)) AS EsLaborable,
        ISNULL(CONVERT(VARCHAR(5), D.HoraInicio, 108), '') AS HoraInicio,
        ISNULL(CONVERT(VARCHAR(5), D.HoraFin, 108), '')    AS HoraFin
    FROM @Semana AS S
    LEFT JOIN dbo.R_HorarioLaboralDetalle AS D
        ON D.IdHorarioLaboral = @IdHorarioLaboral
       AND D.DiaSemana = S.DiaSemana
       AND D.Activo = 1
    ORDER BY S.DiaSemana;
END;
GO

/* ============================================================================
   6. Sp_RTA_GuardarHorario

   Crea o actualiza un perfil compartido junto con sus siete días.

   @Detalle llega como XML porque son siete filas y no vale la pena inventar
   veintiún parámetros:

       <dias>
         <dia ds="1" lab="1" ini="08:30" fin="17:30" />
         <dia ds="6" lab="0" ini="" fin="" />
         ...
       </dias>

   @ConfirmaSobrescribir existe por una razón concreta: editar las horas de un
   perfil cambia el cálculo de horas extras de TODO lo que ya se registró con
   él. Si el perfil tiene gente asignada y las horas cambian, el procedimiento
   se planta con -5 y devuelve cuántos son. La pantalla pregunta y, si el
   usuario insiste, vuelve a llamar con el flag en 1.

   Respuestas: > 0  IdHorarioLaboral guardado
               -1  código o nombre vacío
               -2  ya existe otro perfil con ese código
               -3  el perfil que se quiere editar no existe
               -4  el detalle de los días no es válido
               -5  cambian las horas y hay gente asignada (falta confirmar)
               -6  no se puede quitar el predeterminado sin poner otro
               -7  no se puede inactivar un perfil con gente asignada
               -99 error no controlado
   ============================================================================ */

IF OBJECT_ID(N'dbo.Sp_RTA_GuardarHorario', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarHorario;
GO

CREATE PROCEDURE dbo.Sp_RTA_GuardarHorario
    @IdHorarioLaboral     INT = 0,          -- 0 = perfil nuevo
    @Codigo               VARCHAR(30),
    @Nombre               VARCHAR(120),
    @EsPredeterminado     BIT = 0,
    @Activo               BIT = 1,
    @Detalle              XML,
    @UsuarioRegistro      VARCHAR(100) = '',
    @ConfirmaSobrescribir BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Dias TABLE
    (
        DiaSemana   TINYINT PRIMARY KEY,
        NombreDia   VARCHAR(15),
        EsLaborable BIT,
        HoraInicio  TIME(0) NULL,
        HoraFin     TIME(0) NULL
    );

    DECLARE @Asignados        INT = 0;
    DECLARE @EraPredeterminado BIT = 0;

    SET @Codigo = UPPER(LTRIM(RTRIM(ISNULL(@Codigo, ''))));
    SET @Nombre = LTRIM(RTRIM(ISNULL(@Nombre, '')));
    SET @IdHorarioLaboral = ISNULL(@IdHorarioLaboral, 0);

    IF @Codigo = '' OR @Nombre = ''
    BEGIN
        SELECT -1 AS Respuestas, N'El código y el nombre del horario son obligatorios.' AS Mensaje;
        RETURN;
    END;

    /* ---------------------------------------------------------------- detalle */
    INSERT INTO @Dias (DiaSemana, NombreDia, EsLaborable, HoraInicio, HoraFin)
    SELECT
        T.c.value('@ds', 'TINYINT'),
        CASE T.c.value('@ds', 'TINYINT')
            WHEN 1 THEN 'LUNES'   WHEN 2 THEN 'MARTES'  WHEN 3 THEN 'MIERCOLES'
            WHEN 4 THEN 'JUEVES'  WHEN 5 THEN 'VIERNES' WHEN 6 THEN 'SABADO'
            ELSE 'DOMINGO'
        END,
        T.c.value('@lab', 'BIT'),
        CASE WHEN T.c.value('@lab', 'BIT') = 1
             THEN TRY_CONVERT(TIME(0), NULLIF(LTRIM(RTRIM(T.c.value('@ini', 'VARCHAR(8)'))), ''))
        END,
        CASE WHEN T.c.value('@lab', 'BIT') = 1
             THEN TRY_CONVERT(TIME(0), NULLIF(LTRIM(RTRIM(T.c.value('@fin', 'VARCHAR(8)'))), ''))
        END
    FROM @Detalle.nodes('/dias/dia') AS T(c)
    WHERE T.c.value('@ds', 'TINYINT') BETWEEN 1 AND 7;

    IF (SELECT COUNT(*) FROM @Dias) <> 7
    BEGIN
        SELECT -4 AS Respuestas, N'Deben enviarse los siete días de la semana.' AS Mensaje;
        RETURN;
    END;

    IF EXISTS
    (
        SELECT 1 FROM @Dias
        WHERE EsLaborable = 1
          AND (HoraInicio IS NULL OR HoraFin IS NULL OR HoraInicio >= HoraFin)
    )
    BEGIN
        SELECT -4 AS Respuestas,
               N'En los días laborables la hora de inicio y la de fin son obligatorias, y el inicio debe ser anterior al fin.' AS Mensaje;
        RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM @Dias WHERE EsLaborable = 1)
    BEGIN
        SELECT -4 AS Respuestas, N'El horario debe tener al menos un día laborable.' AS Mensaje;
        RETURN;
    END;

    /* ---------------------------------------------------------------- cabecera */
    IF EXISTS
    (
        SELECT 1 FROM dbo.R_HorarioLaboral
        WHERE Codigo = @Codigo
          AND IdHorarioLaboral <> @IdHorarioLaboral
    )
    BEGIN
        SELECT -2 AS Respuestas, N'Ya existe otro horario con el código ' + @Codigo + N'.' AS Mensaje;
        RETURN;
    END;

    /* Un predeterminado inactivo deja sin horario a todo el que no tenga
       asignación: FN_RTA_ClasificarTramosHorario lo busca con Activo = 1. */
    IF @EsPredeterminado = 1 AND @Activo = 0
    BEGIN
        SELECT -6 AS Respuestas,
               N'El horario predeterminado no puede quedar inactivo.' AS Mensaje;
        RETURN;
    END;

    IF @IdHorarioLaboral > 0
    BEGIN
        SELECT @EraPredeterminado = EsPredeterminado
        FROM dbo.R_HorarioLaboral
        WHERE IdHorarioLaboral = @IdHorarioLaboral
          AND Id_ResponsableDueno IS NULL;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT -3 AS Respuestas,
                   N'El horario que intenta editar no existe, o es el horario propio de una persona y se edita desde la pantalla de asignación.' AS Mensaje;
            RETURN;
        END;

        SELECT @Asignados = COUNT(*)
        FROM dbo.R_UsuarioHorarioLaboral
        WHERE IdHorarioLaboral = @IdHorarioLaboral
          AND Activo = 1
          AND FechaHasta IS NULL;

        /* Quitarle el predeterminado al único que lo tiene deja sin horario a
           todos los que no tienen asignación explícita. */
        IF @EraPredeterminado = 1 AND @EsPredeterminado = 0
        BEGIN
            SELECT -6 AS Respuestas,
                   N'Este es el horario predeterminado. Para quitarle esa marca, primero marque otro horario como predeterminado.' AS Mensaje;
            RETURN;
        END;

        IF @Activo = 0 AND @Asignados > 0
        BEGIN
            SELECT -7 AS Respuestas,
                   N'No se puede inactivar: hay ' + CONVERT(NVARCHAR(10), @Asignados)
                   + N' usuario(s) con este horario vigente. Cámbieles el horario primero.' AS Mensaje;
            RETURN;
        END;

        /* Editar las horas reescribe el pasado de quien ya tiene este horario */
        IF @Asignados > 0 AND @ConfirmaSobrescribir = 0
        BEGIN
            /* El detalle actual se filtra ANTES de unir: en un FULL OUTER JOIN
               las condiciones del ON no descartan filas, y los días de los
               otros horarios entrarían como diferencias inexistentes. */
            IF EXISTS
            (
                SELECT 1
                FROM @Dias AS N
                FULL OUTER JOIN
                (
                    SELECT DiaSemana, EsLaborable, HoraInicio, HoraFin
                    FROM dbo.R_HorarioLaboralDetalle
                    WHERE IdHorarioLaboral = @IdHorarioLaboral
                      AND Activo = 1
                ) AS D
                    ON D.DiaSemana = N.DiaSemana
                WHERE D.DiaSemana IS NULL
                   OR N.DiaSemana IS NULL
                   OR D.EsLaborable <> N.EsLaborable
                   OR ISNULL(CONVERT(VARCHAR(8), D.HoraInicio, 108), '') <> ISNULL(CONVERT(VARCHAR(8), N.HoraInicio, 108), '')
                   OR ISNULL(CONVERT(VARCHAR(8), D.HoraFin, 108), '')    <> ISNULL(CONVERT(VARCHAR(8), N.HoraFin, 108), '')
            )
            BEGIN
                SELECT -5 AS Respuestas,
                       N'Está cambiando los días u horas de un horario que tienen asignado '
                       + CONVERT(NVARCHAR(10), @Asignados)
                       + N' usuario(s). Eso recalcula las horas suplementarias y extraordinarias que ya se registraron con este horario.' AS Mensaje;
                RETURN;
            END;
        END;
    END;

    /* ---------------------------------------------------------------- guardar */
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @IdHorarioLaboral > 0
        BEGIN
            UPDATE dbo.R_HorarioLaboral
            SET Codigo           = @Codigo,
                Nombre           = @Nombre,
                EsPredeterminado = @EsPredeterminado,
                Activo           = @Activo
            WHERE IdHorarioLaboral = @IdHorarioLaboral;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.R_HorarioLaboral (Codigo, Nombre, EsPredeterminado, Activo, Id_ResponsableDueno)
            VALUES (@Codigo, @Nombre, @EsPredeterminado, @Activo, NULL);

            SET @IdHorarioLaboral = CONVERT(INT, SCOPE_IDENTITY());
        END;

        /* Predeterminado hay uno solo */
        IF @EsPredeterminado = 1
        BEGIN
            UPDATE dbo.R_HorarioLaboral
            SET EsPredeterminado = 0
            WHERE EsPredeterminado = 1
              AND IdHorarioLaboral <> @IdHorarioLaboral;
        END;

        MERGE dbo.R_HorarioLaboralDetalle AS Destino
        USING
        (
            SELECT DiaSemana, NombreDia, EsLaborable, HoraInicio, HoraFin
            FROM @Dias
        ) AS Origen
        ON  Destino.IdHorarioLaboral = @IdHorarioLaboral
        AND Destino.DiaSemana = Origen.DiaSemana

        WHEN MATCHED THEN
            UPDATE SET
                Destino.NombreDia   = Origen.NombreDia,
                Destino.EsLaborable = Origen.EsLaborable,
                Destino.HoraInicio  = Origen.HoraInicio,
                Destino.HoraFin     = Origen.HoraFin,
                Destino.Activo      = 1

        WHEN NOT MATCHED BY TARGET THEN
            INSERT (IdHorarioLaboral, DiaSemana, NombreDia, EsLaborable, HoraInicio, HoraFin, Activo)
            VALUES (@IdHorarioLaboral, Origen.DiaSemana, Origen.NombreDia,
                    Origen.EsLaborable, Origen.HoraInicio, Origen.HoraFin, 1);

        COMMIT TRANSACTION;

        SELECT @IdHorarioLaboral AS Respuestas, N'Horario guardado correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SELECT -99 AS Respuestas,
               N'No se pudo guardar el horario. Detalle: ' + ERROR_MESSAGE() AS Mensaje;
    END CATCH;
END;
GO

/* ============================================================================
   7. Sp_RTA_CambiarEstadoHorario

   Prende o apaga un perfil desde la grilla, con las mismas dos protecciones:
   el predeterminado no se apaga, y uno con gente asignada tampoco.

   Respuestas:  1  listo
               -3  el perfil no existe
               -6  es el predeterminado
               -7  tiene usuarios con ese horario vigente
   ============================================================================ */

IF OBJECT_ID(N'dbo.Sp_RTA_CambiarEstadoHorario', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_CambiarEstadoHorario;
GO

CREATE PROCEDURE dbo.Sp_RTA_CambiarEstadoHorario
    @IdHorarioLaboral INT,
    @Activo           BIT,
    @UsuarioRegistro  VARCHAR(100) = ''
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EsPredeterminado BIT;
    DECLARE @Asignados INT = 0;

    SELECT @EsPredeterminado = EsPredeterminado
    FROM dbo.R_HorarioLaboral
    WHERE IdHorarioLaboral = @IdHorarioLaboral;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT -3 AS Respuestas, N'El horario indicado no existe.' AS Mensaje;
        RETURN;
    END;

    IF @Activo = 0
    BEGIN
        IF @EsPredeterminado = 1
        BEGIN
            SELECT -6 AS Respuestas,
                   N'No se puede inactivar el horario predeterminado. Marque otro como predeterminado primero.' AS Mensaje;
            RETURN;
        END;

        SELECT @Asignados = COUNT(*)
        FROM dbo.R_UsuarioHorarioLaboral
        WHERE IdHorarioLaboral = @IdHorarioLaboral
          AND Activo = 1
          AND FechaHasta IS NULL;

        IF @Asignados > 0
        BEGIN
            SELECT -7 AS Respuestas,
                   N'No se puede inactivar: hay ' + CONVERT(NVARCHAR(10), @Asignados)
                   + N' usuario(s) con este horario vigente.' AS Mensaje;
            RETURN;
        END;
    END;

    UPDATE dbo.R_HorarioLaboral
    SET Activo = @Activo
    WHERE IdHorarioLaboral = @IdHorarioLaboral;

    SELECT 1 AS Respuestas,
           CASE WHEN @Activo = 1 THEN N'Horario activado.' ELSE N'Horario inactivado.' END AS Mensaje;
END;
GO

/* ============================================================================
   8. Sp_RTA_GuardarHorarioPropio

   El horario que es de una sola persona. Guarda los días y deja la asignación
   hecha en la misma transacción, para que no quede un perfil huérfano si algo
   falla a mitad de camino.

   El perfil se reusa: si la persona ya tiene el suyo, se le reescriben los días
   en vez de crear uno nuevo cada vez. Por eso el índice UQ_..._Dueno.

   La asignación repite el criterio de Sp_RTA_AsignarHorarioUsuario: cierra la
   anterior, desactiva las que quedaron por delante de la nueva fecha, e inserta.
   Si ya está vigente este mismo perfil desde esta misma fecha, no toca nada.

   Respuestas: > 0  IdHorarioLaboral del horario propio
               -2  el usuario no existe
               -4  el detalle de los días no es válido
               -6  la fecha desde no es válida
               -99 error no controlado
   ============================================================================ */

IF OBJECT_ID(N'dbo.Sp_RTA_GuardarHorarioPropio', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarHorarioPropio;
GO

CREATE PROCEDURE dbo.Sp_RTA_GuardarHorarioPropio
    @Id_Responsable  VARCHAR(20),
    @Detalle         XML,
    @FechaDesde      VARCHAR(10) = NULL,   -- 'yyyy-MM-dd' (NULL = hoy)
    @UsuarioRegistro VARCHAR(100) = ''
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Dias TABLE
    (
        DiaSemana   TINYINT PRIMARY KEY,
        NombreDia   VARCHAR(15),
        EsLaborable BIT,
        HoraInicio  TIME(0) NULL,
        HoraFin     TIME(0) NULL
    );

    DECLARE @Fecha       DATE;
    DECLARE @NomUsuario  VARCHAR(200);
    DECLARE @IdHorario   INT;
    DECLARE @Codigo      VARCHAR(30);
    DECLARE @Sufijo      INT;
    DECLARE @YaVigente   BIT = 0;

    SET @Id_Responsable = LTRIM(RTRIM(ISNULL(@Id_Responsable, '')));

    SELECT @NomUsuario = Nom_Usuario
    FROM dbo.R_Usuarios
    WHERE Cod_Usuario = @Id_Responsable;

    IF @NomUsuario IS NULL
    BEGIN
        SELECT -2 AS Respuestas, N'El usuario indicado no existe.' AS Mensaje;
        RETURN;
    END;

    IF @FechaDesde IS NULL OR LTRIM(RTRIM(@FechaDesde)) = ''
        SET @Fecha = CONVERT(DATE, GETDATE());
    ELSE
        SET @Fecha = TRY_CONVERT(DATE, @FechaDesde);

    IF @Fecha IS NULL
    BEGIN
        SELECT -6 AS Respuestas, N'La fecha desde no tiene un formato válido (use yyyy-MM-dd).' AS Mensaje;
        RETURN;
    END;

    /* ---------------------------------------------------------------- detalle */
    INSERT INTO @Dias (DiaSemana, NombreDia, EsLaborable, HoraInicio, HoraFin)
    SELECT
        T.c.value('@ds', 'TINYINT'),
        CASE T.c.value('@ds', 'TINYINT')
            WHEN 1 THEN 'LUNES'   WHEN 2 THEN 'MARTES'  WHEN 3 THEN 'MIERCOLES'
            WHEN 4 THEN 'JUEVES'  WHEN 5 THEN 'VIERNES' WHEN 6 THEN 'SABADO'
            ELSE 'DOMINGO'
        END,
        T.c.value('@lab', 'BIT'),
        CASE WHEN T.c.value('@lab', 'BIT') = 1
             THEN TRY_CONVERT(TIME(0), NULLIF(LTRIM(RTRIM(T.c.value('@ini', 'VARCHAR(8)'))), ''))
        END,
        CASE WHEN T.c.value('@lab', 'BIT') = 1
             THEN TRY_CONVERT(TIME(0), NULLIF(LTRIM(RTRIM(T.c.value('@fin', 'VARCHAR(8)'))), ''))
        END
    FROM @Detalle.nodes('/dias/dia') AS T(c)
    WHERE T.c.value('@ds', 'TINYINT') BETWEEN 1 AND 7;

    IF (SELECT COUNT(*) FROM @Dias) <> 7
    BEGIN
        SELECT -4 AS Respuestas, N'Deben enviarse los siete días de la semana.' AS Mensaje;
        RETURN;
    END;

    IF EXISTS
    (
        SELECT 1 FROM @Dias
        WHERE EsLaborable = 1
          AND (HoraInicio IS NULL OR HoraFin IS NULL OR HoraInicio >= HoraFin)
    )
    BEGIN
        SELECT -4 AS Respuestas,
               N'En los días laborables la hora de inicio y la de fin son obligatorias, y el inicio debe ser anterior al fin.' AS Mensaje;
        RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM @Dias WHERE EsLaborable = 1)
    BEGIN
        SELECT -4 AS Respuestas, N'El horario debe tener al menos un día laborable.' AS Mensaje;
        RETURN;
    END;

    /* ---------------------------------------------------------------- guardar */
    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @IdHorario = IdHorarioLaboral
        FROM dbo.R_HorarioLaboral
        WHERE Id_ResponsableDueno = @Id_Responsable;

        IF @IdHorario IS NULL
        BEGIN
            SET @Codigo = LEFT('IND_' + @Id_Responsable, 30);
            SET @Sufijo = 1;

            WHILE EXISTS (SELECT 1 FROM dbo.R_HorarioLaboral WHERE Codigo = @Codigo) AND @Sufijo < 100
            BEGIN
                SET @Codigo = LEFT('IND_' + @Id_Responsable, 26) + '_' + CONVERT(VARCHAR(3), @Sufijo);
                SET @Sufijo = @Sufijo + 1;
            END;

            INSERT INTO dbo.R_HorarioLaboral (Codigo, Nombre, EsPredeterminado, Activo, Id_ResponsableDueno)
            VALUES (@Codigo,
                    LEFT('Horario propio - ' + @NomUsuario, 120),
                    0, 1, @Id_Responsable);

            SET @IdHorario = CONVERT(INT, SCOPE_IDENTITY());
        END
        ELSE
        BEGIN
            UPDATE dbo.R_HorarioLaboral
            SET Nombre = LEFT('Horario propio - ' + @NomUsuario, 120),
                Activo = 1,
                EsPredeterminado = 0
            WHERE IdHorarioLaboral = @IdHorario;
        END;

        MERGE dbo.R_HorarioLaboralDetalle AS Destino
        USING
        (
            SELECT DiaSemana, NombreDia, EsLaborable, HoraInicio, HoraFin
            FROM @Dias
        ) AS Origen
        ON  Destino.IdHorarioLaboral = @IdHorario
        AND Destino.DiaSemana = Origen.DiaSemana

        WHEN MATCHED THEN
            UPDATE SET
                Destino.NombreDia   = Origen.NombreDia,
                Destino.EsLaborable = Origen.EsLaborable,
                Destino.HoraInicio  = Origen.HoraInicio,
                Destino.HoraFin     = Origen.HoraFin,
                Destino.Activo      = 1

        WHEN NOT MATCHED BY TARGET THEN
            INSERT (IdHorarioLaboral, DiaSemana, NombreDia, EsLaborable, HoraInicio, HoraFin, Activo)
            VALUES (@IdHorario, Origen.DiaSemana, Origen.NombreDia,
                    Origen.EsLaborable, Origen.HoraInicio, Origen.HoraFin, 1);

        /* ------------------------------------------------------- asignación */
        IF EXISTS
        (
            SELECT 1
            FROM dbo.R_UsuarioHorarioLaboral
            WHERE Id_Responsable = @Id_Responsable
              AND IdHorarioLaboral = @IdHorario
              AND Activo = 1
              AND FechaHasta IS NULL
              AND FechaDesde = @Fecha
        )
            SET @YaVigente = 1;

        IF @YaVigente = 0
        BEGIN
            UPDATE dbo.R_UsuarioHorarioLaboral
            SET FechaHasta = DATEADD(DAY, -1, @Fecha)
            WHERE Id_Responsable = @Id_Responsable
              AND Activo = 1
              AND FechaHasta IS NULL
              AND FechaDesde < @Fecha;

            UPDATE dbo.R_UsuarioHorarioLaboral
            SET Activo = 0
            WHERE Id_Responsable = @Id_Responsable
              AND Activo = 1
              AND FechaHasta IS NULL
              AND FechaDesde >= @Fecha;

            INSERT INTO dbo.R_UsuarioHorarioLaboral
            (
                Id_Responsable, IdHorarioLaboral, FechaDesde, FechaHasta, Activo, UsuarioRegistro
            )
            VALUES
            (
                @Id_Responsable, @IdHorario, @Fecha, NULL, 1,
                NULLIF(LTRIM(RTRIM(@UsuarioRegistro)), '')
            );
        END;

        COMMIT TRANSACTION;

        SELECT @IdHorario AS Respuestas, N'Horario propio guardado y asignado correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SELECT -99 AS Respuestas,
               N'No se pudo guardar el horario propio. Detalle: ' + ERROR_MESSAGE() AS Mensaje;
    END CATCH;
END;
GO

PRINT '== Catálogo de horarios: procedimientos creados ==';
GO

/* ============================================================================
   9. La pantalla en el menú, colgando de "Manejo de Perfiles" (Id_MenuPadre 20042)

   Mismo patrón que Script_Menu_ParametrizacionHorario.sql. En PerfilMenu,
   Estado = '0' MUESTRA la opción y Estado = '1' la oculta.
   ============================================================================ */

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @IdMenuPadre BIGINT = 20042;   -- Manejo de Perfiles

DECLARE @Href        VARCHAR(512) = 'ParametrizacionHorario.aspx';
DECLARE @Titulo      VARCHAR(128) = 'Catálogo de Horarios';
DECLARE @Descripcion VARCHAR(512) = 'Crear y editar los horarios laborales que luego se asignan a los usuarios';
DECLARE @Icono       VARCHAR(128) = 'fa fa-clock-o';
DECLARE @UsuarioCrea VARCHAR(16)  = 'admin';
DECLARE @IpCrea      VARCHAR(32)  = '127.0.0.1';

DECLARE @IdMenu BIGINT;
DECLARE @Orden  INT;

SELECT @IdMenu = Id_Menu
FROM dbo.MenuDos
WHERE Href = @Href
  AND Estado_Logico_Registro = 1;

IF @IdMenu IS NULL
BEGIN
    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1
    FROM dbo.MenuDos
    WHERE Id_MenuPadre = @IdMenuPadre;

    INSERT INTO dbo.MenuDos
    (
        Id_MenuPadre, Es_Opcion_de_Menu, Href, Class_Opcion, Class_Icon,
        Titulo, Descripcion, Es_Opcion_Publica, Orden_Opcion, Estado_Registro,
        Fecha_Creacion, Id_Usuario_Creacion, Ip_Creacion, Estado_Logico_Registro
    )
    VALUES
    (
        @IdMenuPadre, 0, @Href, NULL, @Icono,
        @Titulo, @Descripcion, 0, @Orden, 1,
        GETDATE(), @UsuarioCrea, @IpCrea, 1
    );

    SET @IdMenu = CONVERT(BIGINT, SCOPE_IDENTITY());

    PRINT 'MenuDos: opción creada con Id_Menu = ' + CONVERT(VARCHAR(20), @IdMenu) + '.';
END
ELSE
    PRINT 'MenuDos: la opción ya existía (Id_Menu = ' + CONVERT(VARCHAR(20), @IdMenu) + '). Sin duplicar.';

DECLARE @Perfiles TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Perfiles (IdPerfil) VALUES (1), (2), (18), (19);

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT CONVERT(INT, @IdMenu), P.IdPerfil, CONVERT(DATE, GETDATE()), '0'
FROM @Perfiles AS P
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.PerfilMenu AS PM
    WHERE PM.id_Menu = CONVERT(INT, @IdMenu)
      AND PM.IdPerfil = P.IdPerfil
);

PRINT 'PerfilMenu: enlaces de perfil asegurados (1, 2, 18, 19) con Estado = 0 (visible).';

COMMIT TRANSACTION;
GO

/* ----------------------------------------------------------------------------
   Verificación
   ---------------------------------------------------------------------------- */
SELECT M.Id_Menu, M.Id_MenuPadre, M.Titulo, M.Href, M.Orden_Opcion, PM.IdPerfil, PM.Estado
FROM dbo.MenuDos AS M
LEFT JOIN dbo.PerfilMenu AS PM ON PM.id_Menu = M.Id_Menu
WHERE M.Href = 'ParametrizacionHorario.aspx'
ORDER BY PM.IdPerfil;
GO

EXEC dbo.Sp_RTA_ListarHorarios @Filtro = '', @IncluirInactivos = 1, @IncluirPropios = 1;
GO

PRINT '== Catálogo de horarios: fin ==';
GO
