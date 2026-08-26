/* ============================================================================
   Etapa 4c de CB-GAP-POL-01: el plan de recuperacion y su cierre.

   1. PermisoRecuperacion            : el plan y la confirmacion del jefe
   2. Sp_RTA_GuardarPlanRecuperacion : lo registra al pedir el permiso
   3. Sp_RTA_ConfirmarRecuperacion   : la confirmacion posterior del jefe
   4. Sp_RTA_ListarRecuperacionesPendientes : las que ya se pueden confirmar

   ORDEN: despues de 2026-08-25-permisos-tipo-y-teletrabajo.sql.
   Se puede ejecutar varias veces sin efecto adicional. No modifica ni una fila.
   Base: ReporTarea
   ============================================================================

   Que queda fuera, y por que
   --------------------------
   La especificacion dice que si el jefe responde "no se recupero", o si nadie
   confirma antes del plazo, el sistema carga esas horas al saldo de vacaciones
   del colaborador. Eso no esta aca.

   El saldo de vacaciones lo espeja SaldoVacaciones desde SAP, y la aplicacion
   nunca le escribe a SAP: DaoVacaciones lee [dcp].PA2006 con un solo SELECT y
   eso es todo. Cargar horas al saldo es el mismo problema de conciliacion que
   dejo bloqueado el adelanto de dias, y sigue sin definirse con Talento Humano
   que cierra ese cargo cuando SAP ya refleje el movimiento.

   Construirlo antes de esa definicion significaria migrar datos reales cuando se
   decida. Asi que 4c registra el plan, permite confirmarlo y deja constancia de
   que no se recupero. El cargo automatico entra junto con el adelanto, cuando
   haya regla.

   La fecha maxima de cierre
   -------------------------
   La spec dice "calculada: mismo mes o hasta 30 dias", que admite dos lecturas
   distintas y con consecuencias: de esta fecha depende que alguien pierda dias
   de vacaciones.

   Se tomo la lectura inequivoca —30 dias desde la fecha del permiso— y se la
   dejo en UN solo lugar, marcado abajo, para que Talento Humano la pueda
   cambiar por una linea si queria decir otra cosa.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* --------------------------------------------------------- 1. la tabla */
IF OBJECT_ID('dbo.PermisoRecuperacion') IS NULL
BEGIN
    CREATE TABLE dbo.PermisoRecuperacion
    (
        IdVacaciones        BIGINT        NOT NULL,

        /* El plan, tal como lo propone el colaborador. */
        FechaPropuesta      DATE          NOT NULL,
        HorarioPropuesto    VARCHAR(100)  NULL,
        Actividades         VARCHAR(1000) NULL,
        Entregables         VARCHAR(1000) NULL,

        /* Calculada al guardar. Ver el encabezado. */
        FechaMaximaCierre   DATE          NOT NULL,

        /* NULL mientras nadie confirma. 1 = se recupero, 0 = no se recupero.
           El cero es un dato con consecuencias, por eso se guarda explicito en
           vez de deducirlo de que la fecha paso. */
        Confirmado          BIT           NULL,
        ObservacionCierre   VARCHAR(500)  NULL,
        FechaConfirmacion   DATETIME2(0)  NULL,
        UsuarioConfirmacion VARCHAR(64)   NULL,

        FechaRegistro       DATETIME2(0)  NOT NULL
            CONSTRAINT DF_PermisoRecuperacion_Fecha DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_PermisoRecuperacion PRIMARY KEY (IdVacaciones)
    );

    /* El panel del jefe busca por fecha de cierre entre las no confirmadas. */
    CREATE INDEX IX_PermisoRecuperacion_Cierre
        ON dbo.PermisoRecuperacion (FechaMaximaCierre) INCLUDE (Confirmado);

    PRINT 'PermisoRecuperacion creada.';
END
ELSE
    PRINT 'PermisoRecuperacion ya existia.';
GO

/* --------------------------------------------------- 2. guardar el plan */
IF OBJECT_ID('dbo.Sp_RTA_GuardarPlanRecuperacion') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarPlanRecuperacion;
GO

CREATE PROCEDURE dbo.Sp_RTA_GuardarPlanRecuperacion
    @IdVacaciones     BIGINT,
    @FechaPropuesta   DATE,
    @HorarioPropuesto VARCHAR(100)  = '',
    @Actividades      VARCHAR(1000) = '',
    @Entregables      VARCHAR(1000) = ''
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacaciones WHERE IdVacaciones = @IdVacaciones)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'La solicitud indicada no existe.'; RETURN;
    END

    IF @FechaPropuesta IS NULL
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Debe indicar la fecha propuesta de recuperacion.'; RETURN;
    END

    /* La fecha del permiso, desde la que se cuenta el plazo. */
    DECLARE @FechaPermiso DATE;
    SELECT @FechaPermiso = CAST(ISNULL(FechaDesde, FechaRegistro) AS DATE)
    FROM dbo.Vacaciones WHERE IdVacaciones = @IdVacaciones;

    IF @FechaPropuesta < @FechaPermiso
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'La recuperacion no puede ser anterior al permiso.'; RETURN;
    END

    /* >>> LA REGLA DEL PLAZO. Un solo lugar, a proposito. <<<
       30 dias desde el permiso. Si Talento Humano queria decir "hasta fin del
       mismo mes", se cambia aca y en ningun otro lado. */
    DECLARE @FechaMaxima DATE = DATEADD(DAY, 30, @FechaPermiso);

    IF @FechaPropuesta > @FechaMaxima
    BEGIN
        SELECT Respuestas = 0,
               Mensaje = 'La fecha propuesta supera el plazo maximo, que vence el '
                         + CONVERT(VARCHAR(10), @FechaMaxima, 105) + '.';
        RETURN;
    END

    BEGIN TRY
        /* Se rehace: el plan acompana a la solicitud y la solicitud es una sola.
           Ojo, se conserva la confirmacion si ya existia: reproponer fechas no
           puede borrar el hecho de que el jefe ya cerro el caso. */
        IF EXISTS (SELECT 1 FROM dbo.PermisoRecuperacion WHERE IdVacaciones = @IdVacaciones)
        BEGIN
            UPDATE dbo.PermisoRecuperacion
            SET FechaPropuesta    = @FechaPropuesta,
                HorarioPropuesto  = NULLIF(LTRIM(RTRIM(@HorarioPropuesto)),''),
                Actividades       = NULLIF(LTRIM(RTRIM(@Actividades)),''),
                Entregables       = NULLIF(LTRIM(RTRIM(@Entregables)),''),
                FechaMaximaCierre = @FechaMaxima
            WHERE IdVacaciones = @IdVacaciones;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.PermisoRecuperacion
                (IdVacaciones, FechaPropuesta, HorarioPropuesto, Actividades,
                 Entregables, FechaMaximaCierre)
            VALUES
                (@IdVacaciones, @FechaPropuesta,
                 NULLIF(LTRIM(RTRIM(@HorarioPropuesto)),''),
                 NULLIF(LTRIM(RTRIM(@Actividades)),''),
                 NULLIF(LTRIM(RTRIM(@Entregables)),''),
                 @FechaMaxima);
        END

        SELECT Respuestas = 1,
               Mensaje = 'Plan de recuperacion guardado. Plazo maximo: '
                         + CONVERT(VARCHAR(10), @FechaMaxima, 105) + '.';
    END TRY
    BEGIN CATCH
        SELECT Respuestas = 0, Mensaje = 'No se pudo guardar el plan: ' + ERROR_MESSAGE();
    END CATCH
END
GO
PRINT 'Sp_RTA_GuardarPlanRecuperacion creado.';
GO

/* ----------------------------------------- 3. confirmacion del jefe */
IF OBJECT_ID('dbo.Sp_RTA_ConfirmarRecuperacion') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ConfirmarRecuperacion;
GO

CREATE PROCEDURE dbo.Sp_RTA_ConfirmarRecuperacion
    @IdVacaciones      BIGINT,
    @SeRecupero        BIT,
    @Observacion       VARCHAR(500) = '',
    @Cod_Usuario       VARCHAR(64)  = ''
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FechaMaxima DATE, @YaConfirmado BIT;

    SELECT @FechaMaxima = FechaMaximaCierre, @YaConfirmado = Confirmado
    FROM dbo.PermisoRecuperacion WHERE IdVacaciones = @IdVacaciones;

    IF @FechaMaxima IS NULL
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Esa solicitud no tiene plan de recuperacion.'; RETURN;
    END

    IF @YaConfirmado IS NOT NULL
    BEGIN
        /* No es un error: pasa con un doble clic. */
        SELECT Respuestas = 1, Mensaje = 'La recuperacion ya estaba confirmada.'; RETURN;
    END

    /* El paso no existe antes de que venza el plazo. La especificacion lo pide
       asi, y tiene sentido: confirmar antes seria opinar sobre algo que todavia
       puede pasar. */
    IF CAST(GETDATE() AS DATE) < @FechaMaxima
    BEGIN
        SELECT Respuestas = 0,
               Mensaje = 'Todavia no vence el plazo. Se podra confirmar desde el '
                         + CONVERT(VARCHAR(10), @FechaMaxima, 105) + '.';
        RETURN;
    END

    IF @SeRecupero = 0 AND LTRIM(RTRIM(ISNULL(@Observacion,''))) = ''
    BEGIN
        /* Decir que no se recupero tiene consecuencias para el colaborador, asi
           que no puede quedar sin explicacion. */
        SELECT Respuestas = 0, Mensaje = 'Debe indicar por que no se recupero el tiempo.'; RETURN;
    END

    BEGIN TRY
        UPDATE dbo.PermisoRecuperacion
        SET Confirmado          = @SeRecupero,
            ObservacionCierre   = NULLIF(LTRIM(RTRIM(@Observacion)),''),
            FechaConfirmacion   = SYSDATETIME(),
            UsuarioConfirmacion = NULLIF(@Cod_Usuario,'')
        WHERE IdVacaciones = @IdVacaciones;

        SELECT Respuestas = 1,
               Mensaje = CASE WHEN @SeRecupero = 1
                              THEN 'Recuperacion confirmada.'
                              /* El cargo a vacaciones no se hace todavia: falta
                                 definir con GTH la conciliacion con SAP. Se deja
                                 constancia para que se resuelva a mano. */
                              ELSE 'Registrado que no se recupero el tiempo. Talento Humano debe resolver el cargo.'
                         END;
    END TRY
    BEGIN CATCH
        SELECT Respuestas = 0, Mensaje = 'No se pudo confirmar: ' + ERROR_MESSAGE();
    END CATCH
END
GO
PRINT 'Sp_RTA_ConfirmarRecuperacion creado.';
GO

/* -------------------------------- 4. las que el jefe ya puede confirmar */
IF OBJECT_ID('dbo.Sp_RTA_ListarRecuperacionesPendientes') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarRecuperacionesPendientes;
GO

/* Sin filtro por jefe: la pantalla ya trabaja sobre las solicitudes que ese
   usuario ve. Filtrar dos veces por criterios distintos es como se termina con
   listas que no coinciden entre si. */
CREATE PROCEDURE dbo.Sp_RTA_ListarRecuperacionesPendientes
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.IdVacaciones,
        v.Colaborador,
        v.Cedula,
        v.JefeInmediato,
        FechaPermiso      = CAST(v.FechaDesde AS DATE),
        Horas             = ISNULL(v.Horas,''),
        r.FechaPropuesta,
        HorarioPropuesto  = ISNULL(r.HorarioPropuesto,''),
        Actividades       = ISNULL(r.Actividades,''),
        Entregables       = ISNULL(r.Entregables,''),
        r.FechaMaximaCierre,
        DiasVencido       = DATEDIFF(DAY, r.FechaMaximaCierre, CAST(GETDATE() AS DATE))
    FROM dbo.PermisoRecuperacion r
    INNER JOIN dbo.Vacaciones v ON v.IdVacaciones = r.IdVacaciones
    WHERE r.Confirmado IS NULL
      AND CAST(GETDATE() AS DATE) >= r.FechaMaximaCierre
    ORDER BY r.FechaMaximaCierre;
END
GO
PRINT 'Sp_RTA_ListarRecuperacionesPendientes creado.';
GO
