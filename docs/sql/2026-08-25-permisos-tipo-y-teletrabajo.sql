/* ============================================================================
   Etapa 4a de CB-GAP-POL-01: Tipo de Permiso y la rama de Teletrabajo.

   1. Vacaciones.TipoPermiso           : Personal/Medico/Familiar/...
   2. Vacaciones.TratamientoExcedente  : reemplaza a "Cargo a vacaciones"
   3. PermisoTeletrabajo               : los campos propios de esa rama
   4. Sp_RTA_GuardarDetallePermiso     : guarda ambas cosas
   5. Sp_RTA_ObtenerDetallePermiso     : las devuelve para la pantalla y el PDF

   Se puede ejecutar varias veces sin efecto adicional. No modifica ni una fila
   existente.
   Base: ReporTarea
   ============================================================================

   Por que las columnas quedan NULL en lo historico
   ------------------------------------------------
   Los 1437 permisos ya registrados no se migran. Se reviso el dato: Actividad
   es texto libre con 491 valores distintos, de los cuales 415 aparecen una sola
   vez. Clasificar eso automaticamente meteria unos 600 permisos en "Otro", que
   no dice nada, y clasificarlo a mano es trabajo de Talento Humano sobre 491
   cadenas sueltas.

   Asi que "Tipo de Permiso" aplica solo a solicitudes nuevas y lo viejo conserva
   su texto. La pantalla muestra el que cada solicitud tenga. Durante un tiempo
   los reportes van a mezclar dos formas, y esa es la verdad: esos datos nunca
   estuvieron clasificados.

   Y por que TratamientoExcedente tampoco se rellena
   -------------------------------------------------
   CargoVacaciones tiene 655 unos y 782 ceros. El uno significaba "cargar a
   vacaciones" y se podria mapear, pero el cero solo dice "no cargar": no dice
   que correspondiera descuento en nomina. Mapear esos 782 a "sin remuneracion"
   seria afirmar algo que no consta sobre permisos de gente real. Se dejan sin
   valor.

   Por que el detalle va en un procedimiento aparte
   ------------------------------------------------
   Sp_RTAInsertaNuevaSolicitud hace el alta, la actualizacion, la aprobacion, el
   rechazo y el paso de planificacion a vacaciones, todo en un solo cuerpo. Meter
   ahi seis parametros mas de teletrabajo es tocar el camino por el que pasa cada
   solicitud del sistema. El detalle se guarda despues, en su propio
   procedimiento, igual que se hizo con las firmas.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* --------------------------------------------------- 1 y 2. columnas nuevas */
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Vacaciones' AND COLUMN_NAME = 'TipoPermiso')
BEGIN
    ALTER TABLE dbo.Vacaciones ADD TipoPermiso VARCHAR(20) NULL;
    PRINT 'Vacaciones.TipoPermiso creada. Queda NULL en lo historico, a proposito.';
END
ELSE
    PRINT 'Vacaciones.TipoPermiso ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Vacaciones' AND COLUMN_NAME = 'TratamientoExcedente')
BEGIN
    ALTER TABLE dbo.Vacaciones ADD TratamientoExcedente VARCHAR(20) NULL;
    PRINT 'Vacaciones.TratamientoExcedente creada. Queda NULL en lo historico.';
END
ELSE
    PRINT 'Vacaciones.TratamientoExcedente ya existia.';
GO

/* ------------------------------------------------- 3. detalle de teletrabajo */
IF OBJECT_ID('dbo.PermisoTeletrabajo') IS NULL
BEGIN
    /* Tabla hija y no columnas en Vacaciones: el teletrabajo fue 44 de 1437
       permisos historicos. Nueve columnas casi siempre vacias en la tabla
       principal ensucian cada consulta del modulo para servir a una minoria. */
    CREATE TABLE dbo.PermisoTeletrabajo
    (
        IdVacaciones         BIGINT        NOT NULL,

        /* COMPLETA | HORAS. Con HORAS valen HoraDesde y HoraHasta, y despues de
           esa hora la persona se presenta de forma presencial. */
        Modalidad            VARCHAR(20)   NOT NULL,
        HoraDesde            VARCHAR(10)   NULL,
        HoraHasta            VARCHAR(10)   NULL,

        Lugar                VARCHAR(200)  NULL,
        MediosContacto       VARCHAR(200)  NULL,
        MotivoGeneral        VARCHAR(500)  NULL,
        Actividades          VARCHAR(1000) NULL,
        Entregables          VARCHAR(1000) NULL,
        ConfirmaConectividad BIT           NOT NULL CONSTRAINT DF_PermisoTeletrabajo_Conect DEFAULT (0),

        FechaRegistro        DATETIME2(0)  NOT NULL CONSTRAINT DF_PermisoTeletrabajo_Fecha DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_PermisoTeletrabajo PRIMARY KEY (IdVacaciones)
    );

    PRINT 'PermisoTeletrabajo creada.';
END
ELSE
    PRINT 'PermisoTeletrabajo ya existia.';
GO

/* -------------------------------------------------------- 4. guardar detalle */
IF OBJECT_ID('dbo.Sp_RTA_GuardarDetallePermiso') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarDetallePermiso;
GO

CREATE PROCEDURE dbo.Sp_RTA_GuardarDetallePermiso
    @IdVacaciones         BIGINT,
    @TipoPermiso          VARCHAR(20)   = '',
    @TratamientoExcedente VARCHAR(20)   = '',
    @Modalidad            VARCHAR(20)   = '',
    @HoraDesde            VARCHAR(10)   = '',
    @HoraHasta            VARCHAR(10)   = '',
    @Lugar                VARCHAR(200)  = '',
    @MediosContacto       VARCHAR(200)  = '',
    @MotivoGeneral        VARCHAR(500)  = '',
    @Actividades          VARCHAR(1000) = '',
    @Entregables          VARCHAR(1000) = '',
    @ConfirmaConectividad BIT           = 0
AS
BEGIN
    SET NOCOUNT ON;

    SET @TipoPermiso          = UPPER(LTRIM(RTRIM(ISNULL(@TipoPermiso,''))));
    SET @TratamientoExcedente = UPPER(LTRIM(RTRIM(ISNULL(@TratamientoExcedente,''))));
    SET @Modalidad            = UPPER(LTRIM(RTRIM(ISNULL(@Modalidad,''))));

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacaciones WHERE IdVacaciones = @IdVacaciones)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'La solicitud indicada no existe.'; RETURN;
    END

    IF @TipoPermiso <> '' AND @TipoPermiso NOT IN
       ('PERSONAL','MEDICO','FAMILIAR','CALAMIDAD','TELETRABAJO','OTRO')
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El tipo de permiso no es valido.'; RETURN;
    END

    IF @TratamientoExcedente <> '' AND @TratamientoExcedente NOT IN
       ('VACACIONES','RECUPERACION','SIN_REMUNERACION')
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El tratamiento del excedente no es valido.'; RETURN;
    END

    IF @TipoPermiso = 'TELETRABAJO' AND @Modalidad NOT IN ('COMPLETA','HORAS')
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Debe indicar la modalidad del teletrabajo.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Vacaciones
        SET TipoPermiso          = NULLIF(@TipoPermiso,''),
            TratamientoExcedente = NULLIF(@TratamientoExcedente,'')
        WHERE IdVacaciones = @IdVacaciones;

        IF @TipoPermiso = 'TELETRABAJO'
        BEGIN
            /* Se rehace la fila en vez de acumular versiones: el detalle acompana
               a la solicitud, y la solicitud es una sola. */
            DELETE FROM dbo.PermisoTeletrabajo WHERE IdVacaciones = @IdVacaciones;

            INSERT INTO dbo.PermisoTeletrabajo
                (IdVacaciones, Modalidad, HoraDesde, HoraHasta, Lugar, MediosContacto,
                 MotivoGeneral, Actividades, Entregables, ConfirmaConectividad)
            VALUES
                (@IdVacaciones, @Modalidad,
                 NULLIF(LTRIM(RTRIM(@HoraDesde)),''), NULLIF(LTRIM(RTRIM(@HoraHasta)),''),
                 NULLIF(LTRIM(RTRIM(@Lugar)),''), NULLIF(LTRIM(RTRIM(@MediosContacto)),''),
                 NULLIF(LTRIM(RTRIM(@MotivoGeneral)),''), NULLIF(LTRIM(RTRIM(@Actividades)),''),
                 NULLIF(LTRIM(RTRIM(@Entregables)),''), @ConfirmaConectividad);
        END
        ELSE
        BEGIN
            /* Si dejo de ser teletrabajo, el detalle viejo no debe quedar colgando
               y aparecer despues en el PDF. */
            DELETE FROM dbo.PermisoTeletrabajo WHERE IdVacaciones = @IdVacaciones;
        END

        COMMIT TRANSACTION;
        SELECT Respuestas = 1, Mensaje = 'Detalle del permiso guardado.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT Respuestas = 0, Mensaje = 'No se pudo guardar el detalle: ' + ERROR_MESSAGE();
    END CATCH
END
GO
PRINT 'Sp_RTA_GuardarDetallePermiso creado.';
GO

/* -------------------------------------------------------- 5. leer el detalle */
IF OBJECT_ID('dbo.Sp_RTA_ObtenerDetallePermiso') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ObtenerDetallePermiso;
GO

/* Devuelve una fila siempre, aunque no haya teletrabajo ni tipo cargado: la
   pantalla y el PDF necesitan poder preguntar sin tener que distinguir entre
   "no hay detalle" y "no existe la solicitud". */
CREATE PROCEDURE dbo.Sp_RTA_ObtenerDetallePermiso
    @IdVacaciones BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.IdVacaciones,
        TipoPermiso          = ISNULL(v.TipoPermiso,''),
        TratamientoExcedente = ISNULL(v.TratamientoExcedente,''),
        /* Lo historico no tiene TipoPermiso: ahi la pantalla muestra la actividad
           en texto libre, que es lo unico que se registro en su momento. */
        Actividad            = ISNULL(v.Actividad,''),
        EsTeletrabajo        = CASE WHEN t.IdVacaciones IS NULL THEN 0 ELSE 1 END,
        Modalidad            = ISNULL(t.Modalidad,''),
        HoraDesde            = ISNULL(t.HoraDesde,''),
        HoraHasta            = ISNULL(t.HoraHasta,''),
        Lugar                = ISNULL(t.Lugar,''),
        MediosContacto       = ISNULL(t.MediosContacto,''),
        MotivoGeneral        = ISNULL(t.MotivoGeneral,''),
        Actividades          = ISNULL(t.Actividades,''),
        Entregables          = ISNULL(t.Entregables,''),
        ConfirmaConectividad = ISNULL(t.ConfirmaConectividad,CAST(0 AS BIT))
    FROM dbo.Vacaciones v
    LEFT JOIN dbo.PermisoTeletrabajo t ON t.IdVacaciones = v.IdVacaciones
    WHERE v.IdVacaciones = @IdVacaciones;
END
GO
PRINT 'Sp_RTA_ObtenerDetallePermiso creado.';
GO

/* ------------------------------------------------------------------ pruebas
   Para correr a mano. No forman parte del despliegue.

-- Debe rechazar un tipo que no existe.
EXEC dbo.Sp_RTA_GuardarDetallePermiso @IdVacaciones = 1, @TipoPermiso = 'VACACIONES_LARGAS';

-- Teletrabajo sin modalidad: debe rechazar.
EXEC dbo.Sp_RTA_GuardarDetallePermiso @IdVacaciones = 1, @TipoPermiso = 'TELETRABAJO';

-- Una solicitud que no existe.
EXEC dbo.Sp_RTA_GuardarDetallePermiso @IdVacaciones = 999999999, @TipoPermiso = 'PERSONAL';

-- Una solicitud historica: debe devolver una fila con TipoPermiso vacio y la
-- actividad en texto libre.
EXEC dbo.Sp_RTA_ObtenerDetallePermiso @IdVacaciones = 1;
   ------------------------------------------------------------------------- */
