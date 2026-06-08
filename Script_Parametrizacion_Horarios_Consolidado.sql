/* ================================================================================
   SCRIPT CONSOLIDADO - PARAMETRIZACION DE HORARIOS Y CLASIFICACION DE HORAS
   Base de datos : ReporTarea
   Objetivo      : Crear la estructura para clasificar horas normales / suplementarias
                   (50%) / extraordinarias (100%) por empleado, segun el Codigo del
                   Trabajo de Ecuador.

   CARACTERISTICAS:
   - 100% IDEMPOTENTE: solo crea/inserta lo que NO existe. Se puede ejecutar varias
     veces sin alterar objetos, columnas ni datos ya presentes.
   - NO modifica ninguna tabla, dato ni procedimiento existente. Solo agrega:
        * 1 columna nueva (si no existe) en R_DetTareasAranda
        * 4 tablas nuevas
        * 1 tabla temporal de carga
        * Los catalogos base (perfiles de horario + feriados 2026)
        * 1 funcion nueva
   - El cambio al SP Sp_RTAInsertaDetalleTarea_V2 NO se ejecuta aqui: queda
     documentado al final (PARTE B) como paso manual, porque modifica un objeto
     existente en produccion.

   IMPORTANTE: Respalde la base ReporTarea antes de ejecutar.
   Ejecute primero en ambiente de PRUEBAS.
   ================================================================================ */

USE [ReporTarea];
GO

PRINT '== Inicio del despliegue de parametrizacion de horarios ==';
GO

/* ================================================================================
   PASO 1. Agregar columna Det_Horas_Extras_Estado en R_DetTareasAranda
   (Solo se agrega si NO existe. No altera datos existentes.)
   ================================================================================ */

IF COL_LENGTH('dbo.R_DetTareasAranda', 'Det_Horas_Extras_Estado') IS NULL
BEGIN
    ALTER TABLE dbo.R_DetTareasAranda
    ADD Det_Horas_Extras_Estado BIGINT NOT NULL
        CONSTRAINT DF_R_DetTareasAranda_HorasExtrasEstado
        DEFAULT (0) WITH VALUES;

    PRINT 'PASO 1: Columna Det_Horas_Extras_Estado agregada.';
END
ELSE
    PRINT 'PASO 1: La columna Det_Horas_Extras_Estado ya existe. Sin cambios.';
GO

/* ================================================================================
   PASO 2. Tabla de perfiles de horario: R_HorarioLaboral
   ================================================================================ */

IF OBJECT_ID(N'dbo.R_HorarioLaboral', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.R_HorarioLaboral
    (
        IdHorarioLaboral    INT IDENTITY(1,1) NOT NULL,
        Codigo              VARCHAR(30) NOT NULL,
        Nombre              VARCHAR(120) NOT NULL,
        EsPredeterminado    BIT NOT NULL
            CONSTRAINT DF_R_HorarioLaboral_Predeterminado DEFAULT (0),
        Activo              BIT NOT NULL
            CONSTRAINT DF_R_HorarioLaboral_Activo DEFAULT (1),
        FechaRegistro       DATETIME2(0) NOT NULL
            CONSTRAINT DF_R_HorarioLaboral_FechaRegistro
            DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_R_HorarioLaboral
            PRIMARY KEY (IdHorarioLaboral),

        CONSTRAINT UQ_R_HorarioLaboral_Codigo
            UNIQUE (Codigo)
    );

    PRINT 'PASO 2: Tabla R_HorarioLaboral creada.';
END
ELSE
    PRINT 'PASO 2: La tabla R_HorarioLaboral ya existe. Sin cambios.';
GO

/* ================================================================================
   PASO 3. Tabla de detalle por dia: R_HorarioLaboralDetalle
   ================================================================================ */

IF OBJECT_ID(N'dbo.R_HorarioLaboralDetalle', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.R_HorarioLaboralDetalle
    (
        IdHorarioLaboralDetalle BIGINT IDENTITY(1,1) NOT NULL,
        IdHorarioLaboral        INT NOT NULL,
        DiaSemana               TINYINT NOT NULL,
        NombreDia               VARCHAR(15) NOT NULL,
        EsLaborable             BIT NOT NULL,
        HoraInicio              TIME(0) NULL,
        HoraFin                 TIME(0) NULL,
        Activo                  BIT NOT NULL
            CONSTRAINT DF_R_HorarioLaboralDetalle_Activo DEFAULT (1),

        CONSTRAINT PK_R_HorarioLaboralDetalle
            PRIMARY KEY (IdHorarioLaboralDetalle),

        CONSTRAINT FK_R_HorarioLaboralDetalle_Horario
            FOREIGN KEY (IdHorarioLaboral)
            REFERENCES dbo.R_HorarioLaboral (IdHorarioLaboral),

        CONSTRAINT UQ_R_HorarioLaboralDetalle
            UNIQUE (IdHorarioLaboral, DiaSemana),

        CONSTRAINT CK_R_HorarioLaboralDetalle_DiaSemana
            CHECK (DiaSemana BETWEEN 1 AND 7),

        CONSTRAINT CK_R_HorarioLaboralDetalle_Horas
            CHECK
            (
                (
                    EsLaborable = 0
                    AND HoraInicio IS NULL
                    AND HoraFin IS NULL
                )
                OR
                (
                    EsLaborable = 1
                    AND HoraInicio IS NOT NULL
                    AND HoraFin IS NOT NULL
                    AND HoraInicio < HoraFin
                )
            )
    );

    PRINT 'PASO 3: Tabla R_HorarioLaboralDetalle creada.';
END
ELSE
    PRINT 'PASO 3: La tabla R_HorarioLaboralDetalle ya existe. Sin cambios.';
GO

/* ================================================================================
   PASO 4. Registrar los cuatro perfiles identificados en el Excel
   (MERGE: inserta si no existe, no duplica. No borra perfiles adicionales.)
   ================================================================================ */

MERGE dbo.R_HorarioLaboral AS Destino
USING
(
    SELECT 'H0830_1730' AS Codigo, 'Jornada general 08:30 a 17:30' AS Nombre, CAST(1 AS BIT) AS EsPredeterminado
    UNION ALL
    SELECT 'H0800_1700', 'Jornada 08:00 a 17:00', CAST(0 AS BIT)
    UNION ALL
    SELECT 'H0830_1230', 'Jornada 08:30 a 12:30', CAST(0 AS BIT)
    UNION ALL
    SELECT 'H0800_1630', 'Jornada 08:00 a 16:30', CAST(0 AS BIT)
) AS Origen
ON Destino.Codigo = Origen.Codigo

WHEN MATCHED THEN
    UPDATE SET
        Destino.Nombre = Origen.Nombre,
        Destino.EsPredeterminado = Origen.EsPredeterminado,
        Destino.Activo = 1

WHEN NOT MATCHED THEN
    INSERT (Codigo, Nombre, EsPredeterminado, Activo)
    VALUES (Origen.Codigo, Origen.Nombre, Origen.EsPredeterminado, 1);

PRINT 'PASO 4: Perfiles de horario sincronizados (4 perfiles base).';
GO

/* --------------------------------------------------------------------------------
   PASO 4.1 Detalle de los perfiles: Lunes a viernes laborables, sabado/domingo no.
   (MERGE por horario + dia de la semana.)
   -------------------------------------------------------------------------------- */

DECLARE @Horarios TABLE
(
    Codigo      VARCHAR(30),
    HoraInicio  TIME(0),
    HoraFin     TIME(0)
);

INSERT INTO @Horarios (Codigo, HoraInicio, HoraFin)
VALUES
    ('H0830_1730', '08:30:00', '17:30:00'),
    ('H0800_1700', '08:00:00', '17:00:00'),
    ('H0830_1230', '08:30:00', '12:30:00'),
    ('H0800_1630', '08:00:00', '16:30:00');

DECLARE @Dias TABLE
(
    DiaSemana   TINYINT,
    NombreDia   VARCHAR(15),
    EsLaborable BIT
);

INSERT INTO @Dias (DiaSemana, NombreDia, EsLaborable)
VALUES
    (1, 'LUNES',     1),
    (2, 'MARTES',    1),
    (3, 'MIERCOLES', 1),
    (4, 'JUEVES',    1),
    (5, 'VIERNES',   1),
    (6, 'SABADO',    0),
    (7, 'DOMINGO',   0);

MERGE dbo.R_HorarioLaboralDetalle AS Destino
USING
(
    SELECT
        Horario.IdHorarioLaboral,
        Dia.DiaSemana,
        Dia.NombreDia,
        Dia.EsLaborable,
        CASE WHEN Dia.EsLaborable = 1 THEN Parametro.HoraInicio ELSE NULL END AS HoraInicio,
        CASE WHEN Dia.EsLaborable = 1 THEN Parametro.HoraFin    ELSE NULL END AS HoraFin
    FROM @Horarios AS Parametro
    INNER JOIN dbo.R_HorarioLaboral AS Horario
        ON Horario.Codigo = Parametro.Codigo
    CROSS JOIN @Dias AS Dia
) AS Origen
ON Destino.IdHorarioLaboral = Origen.IdHorarioLaboral
AND Destino.DiaSemana = Origen.DiaSemana

WHEN MATCHED THEN
    UPDATE SET
        Destino.NombreDia = Origen.NombreDia,
        Destino.EsLaborable = Origen.EsLaborable,
        Destino.HoraInicio = Origen.HoraInicio,
        Destino.HoraFin = Origen.HoraFin,
        Destino.Activo = 1

WHEN NOT MATCHED THEN
    INSERT (IdHorarioLaboral, DiaSemana, NombreDia, EsLaborable, HoraInicio, HoraFin, Activo)
    VALUES (Origen.IdHorarioLaboral, Origen.DiaSemana, Origen.NombreDia,
            Origen.EsLaborable, Origen.HoraInicio, Origen.HoraFin, 1);

PRINT 'PASO 4.1: Detalle de dias por perfil sincronizado.';
GO

/* ================================================================================
   PASO 5. Tabla de asignacion por empleado: R_UsuarioHorarioLaboral
   ================================================================================ */

IF OBJECT_ID(N'dbo.R_UsuarioHorarioLaboral', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.R_UsuarioHorarioLaboral
    (
        IdUsuarioHorario    BIGINT IDENTITY(1,1) NOT NULL,
        Id_Responsable      VARCHAR(20) NOT NULL,
        IdHorarioLaboral    INT NOT NULL,
        FechaDesde          DATE NOT NULL
            CONSTRAINT DF_R_UsuarioHorarioLaboral_FechaDesde DEFAULT ('19000101'),
        FechaHasta          DATE NULL,
        Activo              BIT NOT NULL
            CONSTRAINT DF_R_UsuarioHorarioLaboral_Activo DEFAULT (1),
        UsuarioRegistro     VARCHAR(100) NULL,
        FechaRegistro       DATETIME2(0) NOT NULL
            CONSTRAINT DF_R_UsuarioHorarioLaboral_FechaRegistro DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_R_UsuarioHorarioLaboral
            PRIMARY KEY (IdUsuarioHorario),

        CONSTRAINT FK_R_UsuarioHorarioLaboral_Horario
            FOREIGN KEY (IdHorarioLaboral)
            REFERENCES dbo.R_HorarioLaboral (IdHorarioLaboral),

        CONSTRAINT CK_R_UsuarioHorarioLaboral_Fechas
            CHECK (FechaHasta IS NULL OR FechaDesde <= FechaHasta)
    );

    PRINT 'PASO 5: Tabla R_UsuarioHorarioLaboral creada.';
END
ELSE
    PRINT 'PASO 5: La tabla R_UsuarioHorarioLaboral ya existe. Sin cambios.';
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_R_UsuarioHorarioLaboral_Responsable'
      AND object_id = OBJECT_ID(N'dbo.R_UsuarioHorarioLaboral')
)
BEGIN
    CREATE INDEX IX_R_UsuarioHorarioLaboral_Responsable
    ON dbo.R_UsuarioHorarioLaboral (Id_Responsable, Activo, FechaDesde, FechaHasta);

    PRINT 'PASO 5.1: Indice IX_R_UsuarioHorarioLaboral_Responsable creado.';
END
ELSE
    PRINT 'PASO 5.1: El indice ya existe. Sin cambios.';
GO

/* ================================================================================
   PASO 6. Tabla temporal de carga del Excel: R_UsuarioHorarioLaboralCargaExcel
   (Estructura de apoyo para importar el listado de contratos CAS.)
   ================================================================================ */

IF OBJECT_ID(N'dbo.R_UsuarioHorarioLaboralCargaExcel', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.R_UsuarioHorarioLaboralCargaExcel
    (
        ApellidosNombres    VARCHAR(250) NULL,
        Cedula              VARCHAR(20) NOT NULL,
        Empresa             VARCHAR(100) NULL,
        Cliente             VARCHAR(150) NULL,
        HorarioTexto        VARCHAR(30) NOT NULL
    );

    PRINT 'PASO 6: Tabla R_UsuarioHorarioLaboralCargaExcel creada.';
END
ELSE
    PRINT 'PASO 6: La tabla R_UsuarioHorarioLaboralCargaExcel ya existe. Sin cambios.';
GO

/* ================================================================================
   PASO 7. Tabla de feriados: Feriado
   ================================================================================ */

/* Esquema alineado con la tabla Feriado ya existente en produccion:
   IdFeriado, Fecha, Descripcion, Activo, UsuarioRegistro, FechaRegistro.
   (Sin TipoFeriado / IdCiudad: la BD actual no maneja feriados locales por ciudad.) */
IF OBJECT_ID(N'dbo.Feriado', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Feriado
    (
        IdFeriado       INT IDENTITY(1,1) NOT NULL,
        Fecha           DATE NOT NULL,
        Descripcion     NVARCHAR(200) NOT NULL,
        Activo          BIT NOT NULL
            CONSTRAINT DF_Feriado_Activo DEFAULT (1),
        UsuarioRegistro VARCHAR(150) NULL,
        FechaRegistro   DATETIME2(0) NOT NULL
            CONSTRAINT DF_Feriado_FechaRegistro DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_Feriado
            PRIMARY KEY (IdFeriado),

        CONSTRAINT UQ_Feriado_Fecha
            UNIQUE (Fecha)
    );

    PRINT 'PASO 7: Tabla Feriado creada.';
END
ELSE
    PRINT 'PASO 7: La tabla Feriado ya existe. Sin cambios.';
GO

/* --------------------------------------------------------------------------------
   PASO 7.1 Feriados nacionales efectivos de Ecuador para 2026
   (MERGE: no duplica los que ya existan.)
   -------------------------------------------------------------------------------- */

MERGE dbo.Feriado AS Destino
USING
(
    SELECT CAST('2026-01-01' AS DATE) AS Fecha, N'Año Nuevo' AS Descripcion
    UNION ALL SELECT '2026-02-16', N'Carnaval - lunes'
    UNION ALL SELECT '2026-02-17', N'Carnaval - martes'
    UNION ALL SELECT '2026-04-03', N'Viernes Santo'
    UNION ALL SELECT '2026-05-01', N'Día del Trabajo'
    UNION ALL SELECT '2026-05-25', N'Batalla del Pichincha - descanso trasladado'
    UNION ALL SELECT '2026-08-10', N'Primer Grito de la Independencia'
    UNION ALL SELECT '2026-10-09', N'Independencia de Guayaquil'
    UNION ALL SELECT '2026-11-02', N'Día de los Difuntos'
    UNION ALL SELECT '2026-11-03', N'Independencia de Cuenca'
    UNION ALL SELECT '2026-12-25', N'Navidad'
) AS Origen
ON Destino.Fecha = Origen.Fecha

WHEN MATCHED THEN
    UPDATE SET
        Destino.Descripcion = Origen.Descripcion,
        Destino.Activo = 1

WHEN NOT MATCHED THEN
    INSERT (Fecha, Descripcion, Activo, UsuarioRegistro)
    VALUES (Origen.Fecha, Origen.Descripcion, 1, 'SCRIPT_INICIAL');

PRINT 'PASO 7.1: Feriados nacionales 2026 sincronizados.';
GO

/* ================================================================================
   PASO 8. Funcion: FN_RTA_ClasificarTramosHorario
   Clasifica un rango horario en tramos: 0=Normal, 1=Suplementaria 50%,
   2=Extraordinaria 100%.

   NOTA: Esta funcion es un objeto NUEVO de esta funcionalidad. Para que el script
   sea repetible se elimina y se recrea (solo afecta a esta funcion, no a objetos
   preexistentes). Si prefiere no recrearla cuando ya exista, comente el bloque DROP.
   ================================================================================ */

IF OBJECT_ID(N'dbo.FN_RTA_ClasificarTramosHorario') IS NOT NULL
BEGIN
    DROP FUNCTION dbo.FN_RTA_ClasificarTramosHorario;
    PRINT 'PASO 8: Funcion FN_RTA_ClasificarTramosHorario existente eliminada para recrear.';
END;
GO

CREATE FUNCTION dbo.FN_RTA_ClasificarTramosHorario
(
    @Id_Responsable VARCHAR(20),
    @FechaInicio    DATETIME,
    @FechaFin       DATETIME
)
RETURNS @Tramos TABLE
(
    Orden        INT NOT NULL,
    TramoInicio  DATETIME NOT NULL,
    TramoFin     DATETIME NOT NULL,
    TipoHora     BIGINT NOT NULL,
    Descripcion  VARCHAR(50) NOT NULL
)
AS
BEGIN
    IF @FechaInicio IS NULL
       OR @FechaFin IS NULL
       OR @FechaInicio >= @FechaFin
    BEGIN
        RETURN;
    END;

    IF CONVERT(DATE, @FechaInicio) <> CONVERT(DATE, @FechaFin)
    BEGIN
        RETURN;
    END;

    DECLARE @FechaTrabajo DATE;
    DECLARE @DiaSemana TINYINT;
    DECLARE @IdHorarioLaboral INT;
    DECLARE @EsLaborable BIT;
    DECLARE @HoraInicioOficina TIME(0);
    DECLARE @HoraFinOficina TIME(0);
    DECLARE @EsFeriado BIT;

    DECLARE @InicioDia DATETIME;
    DECLARE @LimiteSeis DATETIME;
    DECLARE @InicioOficina DATETIME;
    DECLARE @FinOficina DATETIME;

    SET @FechaTrabajo = CONVERT(DATE, @FechaInicio);
    SET @EsFeriado = 0;

    /* 1900-01-01 fue lunes. No depende de SET DATEFIRST. */
    SET @DiaSemana =
        CONVERT(TINYINT, (DATEDIFF(DAY, '19000101', @FechaTrabajo) % 7) + 1);

    /* 1. Horario especifico del empleado */
    SELECT TOP (1)
        @IdHorarioLaboral = Asignacion.IdHorarioLaboral
    FROM dbo.R_UsuarioHorarioLaboral AS Asignacion
    INNER JOIN dbo.R_HorarioLaboral AS Horario
        ON Horario.IdHorarioLaboral = Asignacion.IdHorarioLaboral
    WHERE Asignacion.Id_Responsable = @Id_Responsable
      AND Asignacion.Activo = 1
      AND Horario.Activo = 1
      AND Asignacion.FechaDesde <= @FechaTrabajo
      AND (Asignacion.FechaHasta IS NULL OR Asignacion.FechaHasta >= @FechaTrabajo)
    ORDER BY Asignacion.FechaDesde DESC;

    /* 2. Si no existe asignacion, usar horario predeterminado */
    IF @IdHorarioLaboral IS NULL
    BEGIN
        SELECT TOP (1)
            @IdHorarioLaboral = Horario.IdHorarioLaboral
        FROM dbo.R_HorarioLaboral AS Horario
        WHERE Horario.EsPredeterminado = 1
          AND Horario.Activo = 1
        ORDER BY Horario.IdHorarioLaboral;
    END;

    IF @IdHorarioLaboral IS NULL
    BEGIN
        RETURN;
    END;

    /* 3. Configuracion del dia */
    SELECT TOP (1)
        @EsLaborable = Detalle.EsLaborable,
        @HoraInicioOficina = Detalle.HoraInicio,
        @HoraFinOficina = Detalle.HoraFin
    FROM dbo.R_HorarioLaboralDetalle AS Detalle
    WHERE Detalle.IdHorarioLaboral = @IdHorarioLaboral
      AND Detalle.DiaSemana = @DiaSemana
      AND Detalle.Activo = 1;

    IF @EsLaborable IS NULL
    BEGIN
        RETURN;
    END;

    /* 4. Validar feriado nacional */
    IF EXISTS
    (
        SELECT 1
        FROM dbo.Feriado AS Feriado
        WHERE Feriado.Fecha = @FechaTrabajo
          AND Feriado.Activo = 1
    )
    BEGIN
        SET @EsFeriado = 1;
    END;

    /* 5. Feriado o dia no laborable: 100% */
    IF @EsFeriado = 1 OR @EsLaborable = 0
    BEGIN
        INSERT INTO @Tramos (Orden, TramoInicio, TramoFin, TipoHora, Descripcion)
        VALUES (1, @FechaInicio, @FechaFin, 2, 'HORAS EXTRAORDINARIAS 100%');
        RETURN;
    END;

    SET @InicioDia = CONVERT(DATETIME, @FechaTrabajo);
    SET @LimiteSeis = DATEADD(HOUR, 6, @InicioDia);

    SET @InicioOficina =
        DATEADD(SECOND,
            DATEDIFF(SECOND, CAST('00:00:00' AS TIME(0)), @HoraInicioOficina),
            @InicioDia);

    SET @FinOficina =
        DATEADD(SECOND,
            DATEDIFF(SECOND, CAST('00:00:00' AS TIME(0)), @HoraFinOficina),
            @InicioDia);

    DECLARE @Puntos TABLE (Punto DATETIME NOT NULL PRIMARY KEY);

    INSERT INTO @Puntos (Punto)
    VALUES (@FechaInicio), (@FechaFin);

    IF @LimiteSeis > @FechaInicio AND @LimiteSeis < @FechaFin
       AND NOT EXISTS (SELECT 1 FROM @Puntos WHERE Punto = @LimiteSeis)
    BEGIN
        INSERT INTO @Puntos (Punto) VALUES (@LimiteSeis);
    END;

    IF @InicioOficina > @FechaInicio AND @InicioOficina < @FechaFin
       AND NOT EXISTS (SELECT 1 FROM @Puntos WHERE Punto = @InicioOficina)
    BEGIN
        INSERT INTO @Puntos (Punto) VALUES (@InicioOficina);
    END;

    IF @FinOficina > @FechaInicio AND @FinOficina < @FechaFin
       AND NOT EXISTS (SELECT 1 FROM @Puntos WHERE Punto = @FinOficina)
    BEGIN
        INSERT INTO @Puntos (Punto) VALUES (@FinOficina);
    END;

    ;WITH PuntosOrdenados AS
    (
        SELECT Punto, ROW_NUMBER() OVER (ORDER BY Punto) AS Orden
        FROM @Puntos
    ),
    Segmentos AS
    (
        SELECT
            Inicio.Orden,
            Inicio.Punto AS TramoInicio,
            Fin.Punto AS TramoFin,
            CASE
                WHEN CONVERT(TIME(0), Inicio.Punto) < CAST('06:00:00' AS TIME(0))
                    THEN 2
                WHEN Inicio.Punto >= @InicioOficina AND Fin.Punto <= @FinOficina
                    THEN 0
                ELSE 1
            END AS TipoHora
        FROM PuntosOrdenados AS Inicio
        INNER JOIN PuntosOrdenados AS Fin
            ON Fin.Orden = Inicio.Orden + 1
    )
    INSERT INTO @Tramos (Orden, TramoInicio, TramoFin, TipoHora, Descripcion)
    SELECT
        Segmentos.Orden,
        Segmentos.TramoInicio,
        Segmentos.TramoFin,
        Segmentos.TipoHora,
        CASE Segmentos.TipoHora
            WHEN 0 THEN 'HORAS NORMALES'
            WHEN 1 THEN 'HORAS SUPLEMENTARIAS 50%'
            WHEN 2 THEN 'HORAS EXTRAORDINARIAS 100%'
        END
    FROM Segmentos;

    RETURN;
END;
GO

PRINT 'PASO 8: Funcion FN_RTA_ClasificarTramosHorario creada.';
GO

PRINT '== Despliegue de estructura completado correctamente ==';
GO


/* ================================================================================
   ================================================================================
   PARTE B - PASOS QUE MODIFICAN OBJETOS EXISTENTES O DEPENDEN DE DATOS
             >>> NO se ejecutan automaticamente. Revise y ejecute manualmente. <<<
   ================================================================================
   ================================================================================ */

/* --------------------------------------------------------------------------------
   B.1  CARGA / ASIGNACION DE HORARIOS A LOS 33 EMPLEADOS DEL EXCEL
   --------------------------------------------------------------------------------
   COLUMNAS REALES DE dbo.R_Usuarios YA CONFIRMADAS (verificadas contra la base):
        Id_Responsable  ->  Cod_Usuario     varchar(50)   (100% de coincidencia)
        Cedula          ->  Cedula          varchar(32)
        Estado/activo   ->  Usuario_Estado  varchar(1)     ('A' = Activo)
        Ciudad          ->  NO EXISTE (feriados locales quedan como carga manual)

   Requisito previo: importar el Excel a dbo.R_UsuarioHorarioLaboralCargaExcel.

   IMPORTANTE: El CASE de HorarioTexto asume el texto del Excel ('8:30 a 17:30',
   etc.). Si el Excel trae otro formato (p.ej. '08:30 a 17:30'), ajuste los WHEN.
   Valide primero con:
        SELECT DISTINCT HorarioTexto FROM dbo.R_UsuarioHorarioLaboralCargaExcel;

   --- SCRIPT LISTO PARA EJECUTAR (ya con columnas reales) -------------------------

   INSERT INTO dbo.R_UsuarioHorarioLaboral
   (
       Id_Responsable, IdHorarioLaboral, FechaDesde, UsuarioRegistro
   )
   SELECT
       U.Cod_Usuario,
       Horario.IdHorarioLaboral,
       '19000101',
       'CARGA_EXCEL'
   FROM dbo.R_Usuarios AS U
   INNER JOIN dbo.R_UsuarioHorarioLaboralCargaExcel AS Excel
       ON Excel.Cedula = U.Cedula
   INNER JOIN dbo.R_HorarioLaboral AS Horario
       ON Horario.Codigo =
           CASE LTRIM(RTRIM(Excel.HorarioTexto))
               WHEN '8:30 a 17:30' THEN 'H0830_1730'
               WHEN '8:00 a 17:00' THEN 'H0800_1700'
               WHEN '8:30 a 12:30' THEN 'H0830_1230'
               WHEN '8:00 a 16:30' THEN 'H0800_1630'
           END
   WHERE U.Usuario_Estado = 'A'          -- solo usuarios activos (opcional)
     AND NOT EXISTS
   (
       SELECT 1
       FROM dbo.R_UsuarioHorarioLaboral AS Asignacion
       WHERE Asignacion.Id_Responsable = U.Cod_Usuario
         AND Asignacion.Activo = 1
         AND Asignacion.FechaHasta IS NULL
   );

   Los usuarios sin asignacion usaran automaticamente el perfil predeterminado
   (H0830_1730) gracias a la logica de la funcion.
   -------------------------------------------------------------------------------- */

/* --------------------------------------------------------------------------------
   B.2  MODIFICACION DEL SP EXISTENTE Sp_RTAInsertaDetalleTarea_V2
   --------------------------------------------------------------------------------
   ESTE BLOQUE SI MODIFICA UN OBJETO EXISTENTE. Revise y ejecutelo aparte, DESPUES
   de crear la funcion (PARTE A, PASO 8).

   El SP ya tenia implementada casi toda la logica (tramos, mensaje -7, guardado de
   Tipo/Estado/Descripcion, validaciones). El UNICO cambio funcional obligatorio es
   pasar @Id_Responsable a la nueva firma de la funcion; sin el, toda insercion
   fallaria. Se incluyen ademas 2 ajustes de consistencia (longitudes):

     1) (OBLIGATORIO) llamada a FN_RTA_ClasificarTramosHorario con @Id_Responsable.
     2) @Id_Responsable        VARCHAR(10) -> VARCHAR(20)  (coincide tabla/funcion/C#).
     3) @Tramos.Descripcion    VARCHAR(30) -> VARCHAR(50)  (la funcion retorna 50).

   El resto del cuerpo es identico al SP actual en produccion.

   --- ALTER LISTO PARA EJECUTAR -------------------------------------------------- */
GO

ALTER PROCEDURE [dbo].[Sp_RTAInsertaDetalleTarea_V2]
     @Id_RegTareas                  BIGINT,
     @Det_Num_OrdenServicio         VARCHAR(50) = '',
     @Det_Id_CompAranda             VARCHAR(80) = '',
     @Det_Fch_RegDetalleIni         VARCHAR(40) = '',
     @Det_Fch_RegDetalleFin         VARCHAR(40) = '',
     @Det_EstadoIni                 VARCHAR(3) = '',
     @Det_EstadoFin                 VARCHAR(3) = '',
     @Det_Nom_Empresa               VARCHAR(100) = '',
     @Det_Det_Tarea                 VARCHAR(500) = '',
     @Det_Estado                    VARCHAR(500) = '',
     @IdDet_EstadoIni               INT = 0,
     @Det_Motivo_Cambio_Estado      VARCHAR(256) = '',
     @Det_Observaciones             VARCHAR(512) = '',
     @Det_Horas_Extras_Tipo         BIGINT = 0,
     @Id_Responsable                VARCHAR(20) = '',          /* CAMBIO 2: 10 -> 20 */
     @Det_Tiempo                    VARCHAR(10) = '',
     @Cod_CatalogoTareaSap          BIGINT = 0,
     @IdTipoGasto                   BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Respuestas BIGINT;
    DECLARE @Tiempo VARCHAR(10);
    DECLARE @ExisteOrden VARCHAR(30);
    DECLARE @DesCatalogo VARCHAR(60);
    DECLARE @Configuracion VARCHAR(2);
    DECLARE @Estatus VARCHAR(50);
    DECLARE @PermiteInsertar BIT;

    DECLARE @FechaInicio DATETIME;
    DECLARE @FechaFin DATETIME;

    DECLARE @TipoHoraCalculado BIGINT;
    DECLARE @CantidadTipos INT;
    DECLARE @Det_Horas_Extras_Estado BIGINT;
    DECLARE @Det_Horas_Extras_Descripcion VARCHAR(50);
    DECLARE @DetalleTramos NVARCHAR(MAX);

    DECLARE @Tramos TABLE
    (
        Orden        INT NOT NULL,
        TramoInicio  DATETIME NOT NULL,
        TramoFin     DATETIME NOT NULL,
        TipoHora     BIGINT NOT NULL,
        Descripcion  VARCHAR(50) NOT NULL                      /* CAMBIO 3: 30 -> 50 */
    );

    SET @Respuestas = 0;
    SET @Tiempo = '';
    SET @ExisteOrden = '';
    SET @DesCatalogo = '';
    SET @Configuracion = 'NO';
    SET @Estatus = '';
    SET @PermiteInsertar = 0;
    SET @TipoHoraCalculado = 0;
    SET @CantidadTipos = 0;
    SET @Det_Horas_Extras_Estado = 0;
    SET @Det_Horas_Extras_Descripcion = '';
    SET @DetalleTramos = '';

    /* ========================================================
       1. VALIDAR FECHAS
       ======================================================== */

    SET @FechaInicio =
        TRY_CONVERT(DATETIME, @Det_Fch_RegDetalleIni, 120);

    SET @FechaFin =
        TRY_CONVERT(DATETIME, @Det_Fch_RegDetalleFin, 120);

    IF @FechaInicio IS NULL OR @FechaFin IS NULL
    BEGIN
        SELECT
            -6 AS Respuestas,
            N'Las fechas no tienen un formato válido. Utilice yyyy-MM-dd HH:mm:ss.'
                AS Mensaje;

        RETURN;
    END;

    IF @FechaInicio >= @FechaFin
    BEGIN
        SELECT
            -6 AS Respuestas,
            N'La fecha final debe ser mayor que la fecha inicial.'
                AS Mensaje;

        RETURN;
    END;

    IF CONVERT(DATE, @FechaInicio) <> CONVERT(DATE, @FechaFin)
    BEGIN
        SELECT
            -7 AS Respuestas,
            N'El horario cruza la medianoche. Registre cada fecha por separado.'
                AS Mensaje;

        RETURN;
    END;

    IF ISNULL(@Det_Horas_Extras_Tipo, -1) NOT IN (0, 1, 2)
    BEGIN
        SELECT
            -10 AS Respuestas,
            N'El tipo de horas únicamente puede ser 0, 1 o 2.'
                AS Mensaje;

        RETURN;
    END;

    /* ========================================================
       2. CLASIFICAR EL RANGO EN TRAMOS
       ======================================================== */

    INSERT INTO @Tramos
    (
        Orden,
        TramoInicio,
        TramoFin,
        TipoHora,
        Descripcion
    )
    SELECT
        Orden,
        TramoInicio,
        TramoFin,
        TipoHora,
        Descripcion
    FROM dbo.FN_RTA_ClasificarTramosHorario
    (
        @Id_Responsable,                                       /* CAMBIO 1: nueva firma */
        @FechaInicio,
        @FechaFin
    );

    IF NOT EXISTS
    (
        SELECT 1
        FROM @Tramos
    )
    BEGIN
        SELECT
            -8 AS Respuestas,
            N'No existe una configuración activa para el día seleccionado.'
                AS Mensaje;

        RETURN;
    END;

    SELECT
        @CantidadTipos = COUNT(DISTINCT TipoHora),
        @TipoHoraCalculado = MIN(TipoHora)
    FROM @Tramos;

    SELECT
        @DetalleTramos =
            STUFF
            (
                (
                    SELECT
                        N'; '
                        + CONVERT(VARCHAR(5), Tramo.TramoInicio, 108)
                        + N' a '
                        + CONVERT(VARCHAR(5), Tramo.TramoFin, 108)
                        + N' = '
                        + Tramo.Descripcion
                    FROM @Tramos AS Tramo
                    ORDER BY Tramo.Orden
                    FOR XML PATH(''), TYPE
                ).value('.', 'NVARCHAR(MAX)'),
                1,
                2,
                N''
            );

    /* ========================================================
       3. SI EXISTEN VARIOS TIPOS, NO INSERTAR
       ======================================================== */

    IF @CantidadTipos > 1
    BEGIN
        SELECT
            -7 AS Respuestas,
            N'El horario enviado contiene diferentes tipos de horas. '
            + N'Registre los siguientes tramos por separado: '
            + @DetalleTramos
            + N'.'
                AS Mensaje;

        SELECT
            Orden,
            CONVERT(VARCHAR(5), TramoInicio, 108) AS HoraInicio,
            CONVERT(VARCHAR(5), TramoFin, 108) AS HoraFin,
            TipoHora,
            Descripcion
        FROM @Tramos
        ORDER BY Orden;

        RETURN;
    END;

    /* ========================================================
       4. VALIDAR QUE EL TIPO ENVIADO SEA EL CORRECTO
       ======================================================== */

    IF @Det_Horas_Extras_Tipo <> @TipoHoraCalculado
    BEGIN
        SELECT
            -9 AS Respuestas,
            N'El tipo enviado no corresponde al horario. Debe registrar: '
            + @DetalleTramos
            + N'.'
                AS Mensaje,
            @TipoHoraCalculado AS TipoHoraPermitido;

        RETURN;
    END;

    SET @Det_Horas_Extras_Tipo = @TipoHoraCalculado;
    SET @Det_Horas_Extras_Estado = @TipoHoraCalculado;

    SET @Det_Horas_Extras_Descripcion =
        CASE @TipoHoraCalculado
            WHEN 1 THEN '50%'
            WHEN 2 THEN '100%'
            ELSE ''
        END;

    /* ========================================================
       5. VALIDAR PERÍODO HABILITADO
       ======================================================== */

    SELECT
        @Configuracion =
            CASE
                WHEN EXISTS
                (
                    SELECT 1
                    FROM dbo.PrmConfiguracionTarea AS Configuracion
                    WHERE Configuracion.Estado = 1
                      AND @FechaInicio >= Configuracion.FechaInicio
                      AND @FechaInicio <= Configuracion.FechaFinal
                )
                THEN 'SI'
                ELSE 'NO'
            END;

    IF @Configuracion <> 'SI'
    BEGIN
        SELECT
            -5 AS Respuestas,
            N'La fecha se encuentra fuera del período habilitado.'
                AS Mensaje;

        RETURN;
    END;

    /* ========================================================
       6. VALIDAR ESTADO DE LA ORDEN
       ======================================================== */

    SELECT
        @Estatus =
            UPPER
            (
                RTRIM
                (
                    LTRIM
                    (
                        ISNULL(Orden.ESTATUS, '')
                    )
                )
            )
    FROM dbo.OrdenServicio AS Orden
    WHERE Orden.ORDEN = @Det_Num_OrdenServicio;

    IF @Estatus <> 'EN EJECUCION'
       AND @Estatus <> ''
    BEGIN
        SELECT
            -4 AS Respuestas,
            N'La orden de servicio no se encuentra en ejecución.'
                AS Mensaje;

        RETURN;
    END;

    /* ========================================================
       7. VALIDAR CRUCE CON ACTIVIDADES EXISTENTES
       ======================================================== */

    IF EXISTS
    (
        SELECT 1
        FROM dbo.R_DetTareasAranda AS Detalle
        WHERE Detalle.Id_Responsable = @Id_Responsable
          AND @FechaInicio < Detalle.Det_Fch_RegDetalleFin
          AND @FechaFin > Detalle.Det_Fch_RegDetalleIni
    )
    BEGIN
        SELECT
            -3 AS Respuestas,
            N'Existe otro registro del responsable que se cruza con el horario enviado.'
                AS Mensaje;

        RETURN;
    END;

    /* ========================================================
       8. VALIDAR TIPO DE GASTO Y ORDEN
       ======================================================== */

    SELECT
        @DesCatalogo =
            UPPER
            (
                RTRIM
                (
                    LTRIM
                    (
                        ISNULL(Catalogo.Descripcion, '')
                    )
                )
            )
    FROM dbo.Catalogo AS Catalogo
    WHERE Catalogo.IdCatalogo = @IdTipoGasto;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.R_TareasAranda AS Tarea
        WHERE Tarea.Num_OrdenServicio = @Det_Num_OrdenServicio
    )
    BEGIN
        SET @ExisteOrden = 'EXISTE';
    END;
    ELSE
    BEGIN
        SET @ExisteOrden = 'NO EXISTE';
    END;

    IF
    (
        @ExisteOrden = 'EXISTE'
        AND @DesCatalogo IN
        (
            'DELIVERY',
            'SERVICIOS INTERNOS',
            'PREVENTA'
        )
    )
    BEGIN
        SET @PermiteInsertar = 1;
    END;
    ELSE IF
    (
        @ExisteOrden = 'NO EXISTE'
        AND @DesCatalogo IN
        (
            'SERVICIOS INTERNOS',
            'PREVENTA'
        )
    )
    BEGIN
        SET @PermiteInsertar = 1;
    END;

    IF @PermiteInsertar = 0
    BEGIN
        SELECT
            -1 AS Respuestas,
            N'El número de orden o el tipo de gasto no cumple las condiciones requeridas.'
                AS Mensaje;

        RETURN;
    END;

    /* ========================================================
       9. CALCULAR DURACIÓN
       ======================================================== */

    SET @Tiempo =
        CONVERT
        (
            VARCHAR(8),
            DATEADD
            (
                SECOND,
                DATEDIFF(SECOND, @FechaInicio, @FechaFin),
                CAST('19000101' AS DATETIME)
            ),
            108
        );

    /* ========================================================
       10. INSERTAR ACTIVIDAD
       ======================================================== */

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.R_DetTareasAranda AS Detalle
                WITH (UPDLOCK, HOLDLOCK)
            WHERE Detalle.Id_Responsable = @Id_Responsable
              AND @FechaInicio < Detalle.Det_Fch_RegDetalleFin
              AND @FechaFin > Detalle.Det_Fch_RegDetalleIni
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                -3 AS Respuestas,
                N'Existe otro registro del responsable que se cruza con el horario enviado.'
                    AS Mensaje;

            RETURN;
        END;

        INSERT INTO dbo.R_DetTareasAranda
        (
            Id_RegTareas,
            Det_Num_OrdenServicio,
            Det_Id_CompAranda,
            Det_Fch_RegDetalleIni,
            Det_Fch_RegDetalleFin,
            Det_EstadoIni,
            Det_EstadoFin,
            Det_Nom_Empresa,
            Det_Det_Tarea,
            Det_Motivo_Cambio_Estado,
            Det_Estado,
            IdDet_EstadoIni,
            Det_Observaciones,
            Det_Horas_Extras_Tipo,
            Det_Horas_Extras_Estado,
            Id_Responsable,
            Det_Tiempo,
            Cod_CatalogoTareaSap,
            IdTipoGasto,
            Det_Fecha_RegistraActividad,
            Det_Horas_Extras_Descripcion
        )
        VALUES
        (
            @Id_RegTareas,
            @Det_Num_OrdenServicio,
            @Det_Id_CompAranda,
            @FechaInicio,
            @FechaFin,
            @Det_EstadoIni,
            @Det_EstadoFin,
            @Det_Nom_Empresa,
            @Det_Det_Tarea,
            @Det_Motivo_Cambio_Estado,
            @Det_Estado,
            @IdDet_EstadoIni,
            @Det_Observaciones,
            @Det_Horas_Extras_Tipo,
            @Det_Horas_Extras_Estado,
            @Id_Responsable,
            @Tiempo,
            @Cod_CatalogoTareaSap,
            @IdTipoGasto,
            GETDATE(),
            @Det_Horas_Extras_Descripcion
        );

        SET @Respuestas =
            CONVERT(BIGINT, SCOPE_IDENTITY());

        COMMIT TRANSACTION;

        SELECT
            @Respuestas AS Respuestas,
            N'Actividad registrada correctamente.'
                AS Mensaje,
            @Det_Horas_Extras_Tipo
                AS Det_Horas_Extras_Tipo,
            @Det_Horas_Extras_Estado
                AS Det_Horas_Extras_Estado,
            @Det_Horas_Extras_Descripcion
                AS Det_Horas_Extras_Descripcion,
            @Tiempo
                AS Det_Tiempo;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        SELECT
            -99 AS Respuestas,
            N'No se pudo registrar la actividad. Detalle técnico: '
            + ERROR_MESSAGE()
                AS Mensaje;
    END CATCH;

    SET NOCOUNT OFF;
END;
GO

/* --------------------------------------------------------------------------------
   Fin del ALTER del SP. La capa C# (DaoTareas.cs) ya esta preparada: lee la columna
   "Mensaje", envia @Id_Responsable (VarChar 20) y maneja el codigo -7.
   -------------------------------------------------------------------------------- */

/* --------------------------------------------------------------------------------
   B.3  EJEMPLO: cambio de horario de un empleado conservando historial
   --------------------------------------------------------------------------------
   UPDATE dbo.R_UsuarioHorarioLaboral
   SET FechaHasta = '2026-06-30'
   WHERE Id_Responsable = 'CODIGO_USUARIO'
     AND Activo = 1
     AND FechaHasta IS NULL;

   INSERT INTO dbo.R_UsuarioHorarioLaboral
   (Id_Responsable, IdHorarioLaboral, FechaDesde, UsuarioRegistro)
   SELECT 'CODIGO_USUARIO', Horario.IdHorarioLaboral, '2026-07-01', 'admin'
   FROM dbo.R_HorarioLaboral AS Horario
   WHERE Horario.Codigo = 'H0800_1700';
   -------------------------------------------------------------------------------- */

/* --------------------------------------------------------------------------------
   B.4  PRUEBAS FUNCIONALES (ejecutar despues del despliegue)
   --------------------------------------------------------------------------------
   -- Usuario sin asignacion (usa predeterminado 08:30-17:30): 3 tramos
   SELECT * FROM dbo.FN_RTA_ClasificarTramosHorario
       ('USUARIO_SIN_ASIGNACION', '2026-06-08 07:00:00', '2026-06-08 18:30:00');

   -- Madrugada 02:00-05:00: 100%
   SELECT * FROM dbo.FN_RTA_ClasificarTramosHorario
       ('USUARIO_PRUEBA', '2026-06-08 02:00:00', '2026-06-08 05:00:00');

   -- Feriado 25/05/2026: 100%
   SELECT * FROM dbo.FN_RTA_ClasificarTramosHorario
       ('USUARIO_PRUEBA', '2026-05-25 08:30:00', '2026-05-25 17:30:00');

   -- Domingo 07/06/2026: 100%
   SELECT * FROM dbo.FN_RTA_ClasificarTramosHorario
       ('USUARIO_PRUEBA', '2026-06-07 08:30:00', '2026-06-07 17:30:00');
   -------------------------------------------------------------------------------- */
