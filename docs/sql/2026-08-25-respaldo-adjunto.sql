/* ============================================================================
   Cierre de un hueco de la auditoria: el campo "Respaldo adjunto".

   1. Vacaciones.RespaldoAdjunto    : SI / NO / NO_APLICA
   2. Sp_RTA_GuardarDetallePermiso  : lo recibe
   3. Sp_RTA_ObtenerDetallePermiso  : lo devuelve

   ORDEN: despues de 2026-08-25-saldo-mensual-en-el-pdf.sql.
   Se puede ejecutar varias veces sin efecto adicional. No modifica ni una fila.
   Base: ReporTarea
   ============================================================================

   El requisito
   ------------
   "Respaldo adjunto · nuevo: Si / No / No aplica. Solo cuando es 'Si' se muestra
   el campo de carga de archivo — con 'No' o 'No aplica' el formulario no lo
   pide."

   Estaba en la especificacion y no se habia implementado. La pantalla ya decide
   con el si se pide archivo; faltaba guardar la respuesta, porque el dato dice
   algo que despues no se puede deducir: que un permiso no tenga adjuntos no
   distingue entre "no hacia falta" y "se olvidaron de subirlo".

   Medico y Teletrabajo no lo usan
   -------------------------------
   En Medico el adjunto es obligatorio, asi que preguntar si hay respaldo no
   tiene sentido. En Teletrabajo la especificacion dice que no usa el campo
   generico de adjuntos. En los dos casos la pantalla no muestra el selector y
   aca llega vacio.
   ============================================================================ */

SET NOCOUNT ON;
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Vacaciones' AND COLUMN_NAME = 'RespaldoAdjunto')
BEGIN
    ALTER TABLE dbo.Vacaciones ADD RespaldoAdjunto VARCHAR(20) NULL;
    PRINT 'Vacaciones.RespaldoAdjunto creada. NULL en lo historico: el campo no existia.';
END
ELSE
    PRINT 'Vacaciones.RespaldoAdjunto ya existia.';
GO

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
    @ConfirmaConectividad BIT           = 0,
    @UsaPermisoMensual    BIT           = 0,
    @SaldoMensualTexto    VARCHAR(200)  = '',
    @RespaldoAdjunto      VARCHAR(20)   = ''
AS
BEGIN
    SET NOCOUNT ON;

    SET @TipoPermiso          = UPPER(LTRIM(RTRIM(ISNULL(@TipoPermiso,''))));
    SET @TratamientoExcedente = UPPER(LTRIM(RTRIM(ISNULL(@TratamientoExcedente,''))));
    SET @Modalidad            = UPPER(LTRIM(RTRIM(ISNULL(@Modalidad,''))));
    SET @RespaldoAdjunto      = UPPER(LTRIM(RTRIM(ISNULL(@RespaldoAdjunto,''))));

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

    IF @RespaldoAdjunto <> '' AND @RespaldoAdjunto NOT IN ('SI','NO','NO_APLICA')
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El respaldo adjunto no es valido.'; RETURN;
    END

    IF @TipoPermiso = 'TELETRABAJO' AND @Modalidad NOT IN ('COMPLETA','HORAS')
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Debe indicar la modalidad del teletrabajo.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Vacaciones
        SET TipoPermiso          = NULLIF(@TipoPermiso,''),
            TratamientoExcedente = NULLIF(@TratamientoExcedente,''),
            UsaPermisoMensual    = @UsaPermisoMensual,
            RespaldoAdjunto      = NULLIF(@RespaldoAdjunto,''),
            /* Solo se guarda cuando de verdad usa la bolsa: si no, la constancia
               no viene al caso y el PDF no debe mostrarla. */
            SaldoMensualTexto    = CASE WHEN @UsaPermisoMensual = 1
                                        THEN NULLIF(LTRIM(RTRIM(@SaldoMensualTexto)),'')
                                        ELSE NULL END
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
PRINT 'Sp_RTA_GuardarDetallePermiso actualizado con RespaldoAdjunto.';
GO

IF OBJECT_ID('dbo.Sp_RTA_ObtenerDetallePermiso') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ObtenerDetallePermiso;
GO

CREATE PROCEDURE dbo.Sp_RTA_ObtenerDetallePermiso
    @IdVacaciones BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.IdVacaciones,
        TipoPermiso          = ISNULL(v.TipoPermiso,''),
        TratamientoExcedente = ISNULL(v.TratamientoExcedente,''),
        Actividad            = ISNULL(v.Actividad,''),
        UsaPermisoMensual    = ISNULL(v.UsaPermisoMensual, CAST(0 AS BIT)),
        SaldoMensualTexto    = ISNULL(v.SaldoMensualTexto,''),
        RespaldoAdjunto      = ISNULL(v.RespaldoAdjunto,''),
        EsTeletrabajo        = CASE WHEN t.IdVacaciones IS NULL THEN 0 ELSE 1 END,
        Modalidad            = ISNULL(t.Modalidad,''),
        HoraDesde            = ISNULL(t.HoraDesde,''),
        HoraHasta            = ISNULL(t.HoraHasta,''),
        Lugar                = ISNULL(t.Lugar,''),
        MediosContacto       = ISNULL(t.MediosContacto,''),
        MotivoGeneral        = ISNULL(t.MotivoGeneral,''),
        Actividades          = ISNULL(t.Actividades,''),
        Entregables          = ISNULL(t.Entregables,''),
        ConfirmaConectividad = ISNULL(t.ConfirmaConectividad,CAST(0 AS BIT)),
        TieneRecuperacion    = CASE WHEN r.IdVacaciones IS NULL THEN 0 ELSE 1 END,
        RecFechaPropuesta    = r.FechaPropuesta,
        RecHorario           = ISNULL(r.HorarioPropuesto,''),
        RecActividades       = ISNULL(r.Actividades,''),
        RecEntregables       = ISNULL(r.Entregables,''),
        RecFechaMaxima       = r.FechaMaximaCierre,
        RecConfirmado        = r.Confirmado,
        RecObservacion       = ISNULL(r.ObservacionCierre,'')
    FROM dbo.Vacaciones v
    LEFT JOIN dbo.PermisoTeletrabajo t ON t.IdVacaciones = v.IdVacaciones
    LEFT JOIN dbo.PermisoRecuperacion r ON r.IdVacaciones = v.IdVacaciones
    WHERE v.IdVacaciones = @IdVacaciones;
END
GO
PRINT 'Sp_RTA_ObtenerDetallePermiso actualizado con RespaldoAdjunto.';
GO
