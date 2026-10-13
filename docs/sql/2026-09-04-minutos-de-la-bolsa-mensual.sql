/* ============================================================================
   Los minutos que cubrio la bolsa mensual, para poder decir cuanto se recupera.

   1. Vacaciones.SaldoMensualMinutos   : cuanto tenia la bolsa al pedir el permiso
   2. Sp_RTA_GuardarDetallePermiso     : lo recibe y lo guarda
   3. Sp_RTA_ObtenerDetallePermiso     : lo devuelve

   ORDEN: despues de 2026-08-25-respaldo-adjunto.sql, que es la ultima version
   de los dos procedimientos que este script rehace.

   Se puede ejecutar varias veces sin efecto adicional. No modifica ni una fila
   existente.
   Base: ReporTarea
   ============================================================================

   El problema que resuelve
   ------------------------
   Quien pide 4 horas de permiso y usa las 3 de la bolsa mensual tiene que
   recuperar 1 hora. El documento no lo decia: omitia el renglon "Horas a
   recuperar" cada vez que se usaba la bolsa.

   Y lo omitia por una buena razon. Lo unico que se guardaba de la bolsa era
   SaldoMensualTexto, la frase que se le mostro a la persona -"Le queda 3h00 del
   permiso mensual..."-, y sacar un numero de ahi con SUBSTRING para restarlo es
   la clase de cosa que funciona hasta que alguien cambia una palabra del texto.
   Antes que inventar una cifra en un documento firmado, se omitia el renglon.

   Con la columna el numero esta, y la resta es una resta.

   Por que se guarda el saldo y no las horas a recuperar
   -----------------------------------------------------
   El saldo es el hecho: cuanto habia en la bolsa en ese momento. Las horas a
   recuperar son una consecuencia -permiso menos saldo- y dependen de las horas
   del permiso, que ya viven en Vacaciones.Horas y pueden corregirse. Guardar la
   consecuencia obligaria a recalcularla en cada correccion; guardando el hecho,
   el documento la deriva cuando la necesita.

   Por que NULL y no cero en lo historico
   ---------------------------------------
   Los permisos anteriores a hoy no tienen el dato, y NULL dice exactamente eso.
   Un cero diria "la bolsa estaba vacia", que es una afirmacion distinta y falsa:
   significaria que hay que recuperar el permiso entero. El documento distingue
   los dos casos: con NULL omite el renglon, como hasta ahora.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ------------------------------------------------------ 1. la columna nueva */
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Vacaciones' AND COLUMN_NAME = 'SaldoMensualMinutos')
BEGIN
    ALTER TABLE dbo.Vacaciones ADD SaldoMensualMinutos INT NULL;
    PRINT 'Vacaciones.SaldoMensualMinutos creada. Queda NULL en lo historico, a proposito.';
END
ELSE
    PRINT 'Vacaciones.SaldoMensualMinutos ya existia.';
GO

/* -------------------------------------------- 2. el detalle recibe el numero */
IF OBJECT_ID('dbo.Sp_RTA_GuardarDetallePermiso') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarDetallePermiso;
GO

/* @SaldoMensualMinutos va con valor por defecto para que el codigo anterior lo
   siga llamando sin enterarse, que es lo que permite correr este script antes
   de subir los binarios. */
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
    @RespaldoAdjunto      VARCHAR(20)   = '',
    @SaldoMensualMinutos  INT           = NULL
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

    /* Un saldo negativo no existe: la bolsa se agota en cero. */
    IF @SaldoMensualMinutos < 0 SET @SaldoMensualMinutos = 0;

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
                                        ELSE NULL END,
            /* El numero, por lo mismo y para lo mismo. Va junto al texto: los dos
               son la constancia de lo que habia en la bolsa en ese momento. */
            SaldoMensualMinutos  = CASE WHEN @UsaPermisoMensual = 1
                                        THEN @SaldoMensualMinutos
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
PRINT 'Sp_RTA_GuardarDetallePermiso actualizado.';
GO

/* ------------------------------------------- 3. el detalle devuelve el numero */
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
        /* Sin ISNULL a proposito: NULL significa "no se registro" y el documento
           lo distingue de un cero, que significaria bolsa vacia. */
        SaldoMensualMinutos  = v.SaldoMensualMinutos,
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
PRINT 'Sp_RTA_ObtenerDetallePermiso actualizado.';
GO

/* ------------------------------------------------------------------ pruebas
   Para correr a mano despues. No forman parte del despliegue.

-- Un permiso historico: SaldoMensualMinutos debe venir NULL y el resto igual.
EXEC dbo.Sp_RTA_ObtenerDetallePermiso @IdVacaciones = 22883;

-- Guardar sin el parametro nuevo, como lo llama el codigo anterior: no debe fallar.
EXEC dbo.Sp_RTA_GuardarDetallePermiso @IdVacaciones = 22883, @TipoPermiso = 'MEDICO';
   ------------------------------------------------------------------------- */
