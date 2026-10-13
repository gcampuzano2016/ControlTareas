/* ============================================================================
   Etapa 4b, segunda parte: guardar la casilla del permiso mensual.

   Redefine Sp_RTA_GuardarDetallePermiso para que reciba tambien
   UsaPermisoMensual.

   ORDEN: va despues de 2026-08-25-permisos-tipo-y-teletrabajo.sql, que es donde
   se crea este procedimiento por primera vez, y despues de
   2026-08-25-permiso-mensual-3-horas.sql, que crea la columna.

   Se hace en un archivo aparte y no editando los anteriores porque esos ya
   corrieron. Un script aplicado no se toca: si se lo edita, un servidor nuevo
   reproduce un camino distinto al que siguio produccion, y ahi es donde
   aparecen las diferencias que nadie sabe explicar.

   Se puede ejecutar varias veces sin efecto adicional. No modifica ni una fila.
   Base: ReporTarea
   ============================================================================ */

SET NOCOUNT ON;
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
    /* Nuevo en la etapa 4b. Con valor por defecto para que una pantalla vieja en
       cache, que no lo manda, siga guardando sin error. */
    @UsaPermisoMensual    BIT           = 0
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
            TratamientoExcedente = NULLIF(@TratamientoExcedente,''),
            UsaPermisoMensual    = @UsaPermisoMensual
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
PRINT 'Sp_RTA_GuardarDetallePermiso actualizado con UsaPermisoMensual.';
GO
