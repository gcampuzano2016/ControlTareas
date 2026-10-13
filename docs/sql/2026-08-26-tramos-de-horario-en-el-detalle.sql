/* ============================================================================
   Registro de actividades partido por tramos de horario
   ReporTarea  |  2026-08-26

   APLICADO MANUALMENTE POR EL USUARIO. Este archivo queda como el registro de
   lo que se ejecutó, no como algo que corra un despliegue automático.

   ----------------------------------------------------------------------------
   Qué cambia y por qué

   Hoy, si alguien con jornada 08:30-17:30 registra una actividad de 07:30 a
   18:30, el procedimiento la rechaza con el código -7 y le pide hacer tres
   registros a mano:

       07:30 a 08:30  = HORAS SUPLEMENTARIAS 50%
       08:30 a 17:30  = HORAS NORMALES
       17:30 a 18:30  = HORAS SUPLEMENTARIAS 50%

   Los tramos ya los calcula FN_RTA_ClasificarTramosHorario. El procedimiento
   los tenía, los mostraba en el mensaje de error, y los tiraba. Ahora inserta
   una fila por tramo, cada una con su tipo, su descripción y su propia
   duración.

   ----------------------------------------------------------------------------
   Tres cosas más que se corrigen de paso

   1. Det_Horas_Extras_Estado ya no se escribe con el tipo de hora.

      La versión anterior hacía SET @Det_Horas_Extras_Estado = @TipoHoraCalculado,
      pero esa columna no es el tipo: es el estado del flujo de autorización.
      Verificado en los datos y en el código:

          0  sin solicitar (default de la columna)
          1  solicitud enviada   AdministrarTarea.ashx.cs
          2  APROBADO            RespuestaAprobacion.aspx.cs
          3  rechazado           RespuestaAprobacion.aspx.cs

      Con lo anterior, una hora al 100% nacía marcada como aprobada. El handler
      normalmente la corregía a 1 después de enviar el correo, pero si el correo
      fallaba no lo hacía, y quedaba una hora extra aprobada que nadie autorizó.
      Ahora se inserta 0 y el flujo la mueve.

   2. Ya no se valida que el tipo enviado coincida con el calculado (-9).

      El combo de la pantalla deja de decidir: el sistema clasifica según el
      horario registrado. @Det_Horas_Extras_Tipo se mantiene en la firma porque
      la capa de datos lo envía, pero no se usa. Con eso también desaparece el
      -10, que validaba un parámetro que ya nadie lee.

   3. El -7 queda con un solo significado: el rango cruza la medianoche.

      Antes servía para eso y además para "tipos de horas mezclados", que es
      justamente el caso que ahora se resuelve partiendo.

   ----------------------------------------------------------------------------
   Lo que NO cambia acá

   @Id_Responsable sigue declarado VARCHAR(20) mientras
   R_DetTareasAranda.Id_Responsable es varchar(10). El comentario CAMBIO 2 de la
   versión anterior amplió el parámetro y la firma de la función, pero no la
   columna, así que un valor de más de 10 caracteres reventaría igual y saldría
   como -99. Hoy los Cod_Usuario son cortos y no se nota. Ampliar la columna es
   una migración aparte: hay que revisar antes las tablas que la referencian.
   ============================================================================ */

USE ReporTarea;
GO

/* ============================================================================
   1. Índice para la validación de cruce de horarios

   R_DetTareasAranda tiene ~85.660 filas y no hay ningún índice que empiece por
   Id_Responsable, así que la validación de cruce la recorre entera. La segunda
   pasada corre con UPDLOCK, HOLDLOCK, y sobre un escaneo eso toma rangos de
   bloqueo amplios: bajo uso concurrente serializa el registro de actividades.

   Con el partido en tramos pasa de conveniente a necesario, porque ahora son
   hasta tres inserciones dentro de la misma transacción.
   ============================================================================ */

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.R_DetTareasAranda')
      AND name = 'IX_R_DetTareasAranda_Responsable_Fechas'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_R_DetTareasAranda_Responsable_Fechas
        ON dbo.R_DetTareasAranda
        (
            Id_Responsable,
            Det_Fch_RegDetalleIni,
            Det_Fch_RegDetalleFin
        );

    PRINT 'Indice IX_R_DetTareasAranda_Responsable_Fechas creado.';
END
ELSE
BEGIN
    PRINT 'Indice IX_R_DetTareasAranda_Responsable_Fechas ya existia.';
END;
GO

/* ============================================================================
   2. El procedimiento

   Se redefine completo. Los scripts ya aplicados no se editan.
   ============================================================================ */

IF OBJECT_ID('dbo.Sp_RTAInsertaDetalleTarea_V2', 'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE dbo.Sp_RTAInsertaDetalleTarea_V2 AS SET NOCOUNT ON;');
END;
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
     @Det_Horas_Extras_Tipo         BIGINT = 0,   /* se recibe y se ignora: el
                                                     horario decide, no el combo */
     @Id_Responsable                VARCHAR(20) = '',
     @Det_Tiempo                    VARCHAR(10) = '',
     @Cod_CatalogoTareaSap          BIGINT = 0,
     @IdTipoGasto                   BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ExisteOrden VARCHAR(30);
    DECLARE @DesCatalogo VARCHAR(60);
    DECLARE @Configuracion VARCHAR(2);
    DECLARE @Estatus VARCHAR(50);
    DECLARE @PermiteInsertar BIT;

    DECLARE @FechaInicio DATETIME;
    DECLARE @FechaFin DATETIME;

    DECLARE @DetalleTramos NVARCHAR(MAX);
    DECLARE @IdsHorasExtras NVARCHAR(MAX);
    DECLARE @TramosInsertados INT;
    DECLARE @PrimerId BIGINT;

    DECLARE @Tramos TABLE
    (
        Orden        INT NOT NULL,
        TramoInicio  DATETIME NOT NULL,
        TramoFin     DATETIME NOT NULL,
        TipoHora     BIGINT NOT NULL,
        Descripcion  VARCHAR(50) NOT NULL
    );

    /* Los identificadores que salen del INSERT, con su tipo al lado: el tipo es
       lo que decide cuáles necesitan autorización del jefe. */
    DECLARE @Insertados TABLE
    (
        Id_RegDetTareas BIGINT NOT NULL,
        TipoHora        BIGINT NOT NULL
    );

    SET @ExisteOrden = '';
    SET @DesCatalogo = '';
    SET @Configuracion = 'NO';
    SET @Estatus = '';
    SET @PermiteInsertar = 0;
    SET @DetalleTramos = '';
    SET @IdsHorasExtras = '';
    SET @TramosInsertados = 0;

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

    /* Sigue rechazado: la bolsa de 00:00 a 06:00 es del día siguiente y el
       cálculo de tramos trabaja sobre un solo día. */
    IF CONVERT(DATE, @FechaInicio) <> CONVERT(DATE, @FechaFin)
    BEGIN
        SELECT
            -7 AS Respuestas,
            N'El horario cruza la medianoche. Registre cada fecha por separado.'
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
        @Id_Responsable,
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

    /* El detalle en texto, para contarle al usuario cómo quedó repartido. */
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
       3. VALIDAR PERÍODO HABILITADO
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
       4. VALIDAR ESTADO DE LA ORDEN
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
       5. VALIDAR CRUCE CON ACTIVIDADES EXISTENTES
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
       6. VALIDAR TIPO DE GASTO Y ORDEN
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
       7. INSERTAR UNA FILA POR TRAMO
       ======================================================== */

    BEGIN TRY
        BEGIN TRANSACTION;

        /* Se revalida el cruce con el candado tomado: sin esto, dos registros
           simultáneos del mismo responsable pasan los dos la validación de
           arriba y quedan las dos filas superpuestas. */
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

        /* Un solo INSERT desde @Tramos. El OUTPUT devuelve los identificadores
           con su tipo al lado, que es lo que después decide a cuáles hay que
           pedirles autorización.

           OUTPUT ... INTO es seguro acá: la tabla no tiene triggers. */
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
        OUTPUT
            inserted.Id_RegDetTareas,
            inserted.Det_Horas_Extras_Tipo
        INTO @Insertados (Id_RegDetTareas, TipoHora)
        SELECT
            @Id_RegTareas,
            @Det_Num_OrdenServicio,
            @Det_Id_CompAranda,
            Tramo.TramoInicio,
            Tramo.TramoFin,
            @Det_EstadoIni,
            @Det_EstadoFin,
            @Det_Nom_Empresa,
            @Det_Det_Tarea,
            @Det_Motivo_Cambio_Estado,
            @Det_Estado,
            @IdDet_EstadoIni,
            @Det_Observaciones,
            Tramo.TipoHora,
            /* El estado del flujo de autorización arranca en cero. No es el tipo
               de hora: 2 significa APROBADO. */
            0,
            @Id_Responsable,
            /* La duración de cada tramo, no la del rango completo. */
            CONVERT
            (
                VARCHAR(8),
                DATEADD
                (
                    SECOND,
                    DATEDIFF(SECOND, Tramo.TramoInicio, Tramo.TramoFin),
                    CAST('19000101' AS DATETIME)
                ),
                108
            ),
            @Cod_CatalogoTareaSap,
            @IdTipoGasto,
            GETDATE(),
            CASE Tramo.TipoHora
                WHEN 1 THEN '50%'
                WHEN 2 THEN '100%'
                ELSE ''
            END
        FROM @Tramos AS Tramo;

        COMMIT TRANSACTION;

        /* Los identificadores que necesitan autorización, separados por coma.
           Van en la misma fila de respuesta y no en un segundo result set,
           porque la capa de datos lee una sola fila. */
        SELECT
            @IdsHorasExtras =
                ISNULL
                (
                    STUFF
                    (
                        (
                            SELECT
                                N',' + CONVERT(NVARCHAR(20), Fila.Id_RegDetTareas)
                            FROM @Insertados AS Fila
                            WHERE Fila.TipoHora IN (1, 2)
                            ORDER BY Fila.Id_RegDetTareas
                            FOR XML PATH(''), TYPE
                        ).value('.', 'NVARCHAR(MAX)'),
                        1,
                        1,
                        N''
                    ),
                    N''
                );

        SELECT
            @TramosInsertados = COUNT(*),
            @PrimerId = MIN(Id_RegDetTareas)
        FROM @Insertados;

        SELECT
            @PrimerId AS Respuestas,
            CASE
                WHEN @TramosInsertados = 1
                    THEN N'Actividad registrada correctamente.'
                ELSE N'La actividad se registró en '
                     + CONVERT(NVARCHAR(5), @TramosInsertados)
                     + N' tramos según su horario: '
                     + @DetalleTramos
                     + N'.'
            END AS Mensaje,
            @IdsHorasExtras AS IdsHorasExtras,
            @TramosInsertados AS TramosInsertados;
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

/* ============================================================================
   3. Verificación

   Los tramos, sin insertar nada. Con el usuario 4089, que no tiene horario
   asignado y cae al predeterminado 08:30-17:30, un miércoles:

       SELECT *
       FROM dbo.FN_RTA_ClasificarTramosHorario
            ('4089', '2026-08-26 07:30:00', '2026-08-26 18:30:00');

   Debe devolver tres filas: 50%, normales, 50%.

   El procedimiento, revirtiendo todo. Reemplace la orden y el tipo de gasto por
   valores válidos de su ambiente:

       BEGIN TRANSACTION;

       EXEC dbo.Sp_RTAInsertaDetalleTarea_V2
            @Id_RegTareas          = <id de una tarea existente>,
            @Det_Num_OrdenServicio = '<orden EN EJECUCION>',
            @Det_Fch_RegDetalleIni = '2026-08-26 07:30:00',
            @Det_Fch_RegDetalleFin = '2026-08-26 18:30:00',
            @Det_Horas_Extras_Tipo = 0,
            @Id_Responsable        = '4089',
            @IdTipoGasto           = <IdCatalogo de SERVICIOS INTERNOS>;

       SELECT Id_RegDetTareas, Det_Fch_RegDetalleIni, Det_Fch_RegDetalleFin,
              Det_Tiempo, Det_Horas_Extras_Tipo, Det_Horas_Extras_Estado,
              Det_Horas_Extras_Descripcion
       FROM dbo.R_DetTareasAranda
       WHERE Id_Responsable = '4089'
         AND Det_Fch_RegDetalleIni >= '2026-08-26'
       ORDER BY Det_Fch_RegDetalleIni;

       ROLLBACK TRANSACTION;

   Se espera: TramosInsertados = 3, IdsHorasExtras con dos identificadores, los
   tres Det_Horas_Extras_Estado en 0, y las duraciones 01:00:00 / 09:00:00 /
   01:00:00.

   El caso que no debe cambiar: un rango 09:00-12:00, todo dentro de la jornada,
   tiene que insertar UNA fila, con IdsHorasExtras vacío y el mensaje de siempre.
   ============================================================================ */
