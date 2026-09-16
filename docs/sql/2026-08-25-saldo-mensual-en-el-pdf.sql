/* ============================================================================
   Cierre de un hueco de la etapa 4b: el saldo mensual tiene que quedar en el PDF.

   1. Vacaciones.SaldoMensualTexto     : el saldo congelado al pedir el permiso
   2. Sp_RTA_SaldoPermisoMensual       : la fecha pasa a formato largo
   3. Sp_RTA_GuardarDetallePermiso     : recibe y guarda el texto
   4. Sp_RTA_ObtenerDetallePermiso     : lo devuelve para el PDF

   ORDEN: despues de 2026-08-25-permiso-mensual-guardado.sql.
   Se puede ejecutar varias veces sin efecto adicional. No modifica ni una fila.
   Base: ReporTarea
   ============================================================================

   Que faltaba
   -----------
   La especificacion pide, textual: "Ese mismo texto se repite en el PDF
   descargado, para que quede constancia de cuanto quedaba en ese momento". El
   ejemplo CB-GAP-REG-03 muestra dos lineas en el documento. Se implemento el
   saldo en pantalla y no la constancia en el PDF.

   Por que se congela y no se recalcula
   ------------------------------------
   Lo resuelve la propia frase de la spec: "cuanto quedaba EN ESE MOMENTO". Si el
   PDF recalculara, un documento abierto en noviembre mostraria el saldo de
   noviembre para un permiso de septiembre, y la constancia diria algo falso.

   Es el mismo criterio con el que se congela la identidad del firmante en
   VacacionesFirma: un documento tiene que seguir diciendo lo que decia cuando se
   firmo.

   Por que el nombre del mes va con CASE y no con DATENAME
   ------------------------------------------------------
   DATENAME(MONTH, ...) devuelve el nombre en el idioma de la sesion, que depende
   de quien llama: la aplicacion, un job, o alguien desde Management Studio. El
   mismo permiso podria imprimir "septiembre" o "September" segun el caso. Con
   CASE el resultado es el mismo siempre.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ------------------------------------------------- 1. el texto congelado */
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Vacaciones' AND COLUMN_NAME = 'SaldoMensualTexto')
BEGIN
    ALTER TABLE dbo.Vacaciones ADD SaldoMensualTexto VARCHAR(200) NULL;
    PRINT 'Vacaciones.SaldoMensualTexto creada. NULL en lo historico: no habia saldo mensual.';
END
ELSE
    PRINT 'Vacaciones.SaldoMensualTexto ya existia.';
GO

/* --------------------------------------- 2. el saldo, con la fecha larga */
IF OBJECT_ID('dbo.Sp_RTA_SaldoPermisoMensual') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_SaldoPermisoMensual;
GO

CREATE PROCEDURE dbo.Sp_RTA_SaldoPermisoMensual
    @Cod_Usuario  VARCHAR(64),
    @Fecha        DATE = NULL,
    /* Para no contar la propia solicitud cuando se esta editando una que ya
       existe: sin esto, abrir un permiso guardado y volver a guardarlo veria
       sus horas como ya consumidas. */
    @IdVacaciones BIGINT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @Fecha IS NULL SET @Fecha = CAST(GETDATE() AS DATE);

    DECLARE @Asignados INT = 180;   /* tres horas, en minutos */
    DECLARE @CodSap VARCHAR(50);

    /* Cod_Usuario NO es unico en R_Usuarios: de 246 usuarios hay 3 codigos
       repetidos, y '1052' aparece 12 veces con dos Cod_Sap y dos nombres
       distintos. Con la forma SELECT @var = ... eso no falla, simplemente toma
       una fila cualquiera, asi que el saldo saldria distinto entre llamadas sin
       que nada avise.

       Se elige de forma determinista: usuario activo primero, y entre esos el de
       Id_Usuario mas alto. No arregla el dato —eso es un problema aparte que ya
       afecta a Sp_RTAConsultarSaldoVacaciones— pero al menos la respuesta es
       siempre la misma. */
    SELECT TOP 1 @CodSap = Cod_Sap
    FROM dbo.R_Usuarios
    WHERE Cod_Usuario = @Cod_Usuario
      AND ISNULL(Cod_Sap,'') <> ''
    ORDER BY CASE WHEN Usuario_Estado = 'A' THEN 0 ELSE 1 END, Id_Usuario DESC;

    DECLARE @Hasta DATE = EOMONTH(@Fecha);

    /* Formato largo, como lo escribio Talento Humano: "30 de septiembre de 2026".
       Con CASE y no DATENAME, por lo del idioma de la sesion. */
    DECLARE @HastaTexto VARCHAR(40) =
        CAST(DAY(@Hasta) AS VARCHAR(2)) + ' de ' +
        CASE MONTH(@Hasta)
            WHEN  1 THEN 'enero'   WHEN  2 THEN 'febrero'  WHEN  3 THEN 'marzo'
            WHEN  4 THEN 'abril'   WHEN  5 THEN 'mayo'     WHEN  6 THEN 'junio'
            WHEN  7 THEN 'julio'   WHEN  8 THEN 'agosto'   WHEN  9 THEN 'septiembre'
            WHEN 10 THEN 'octubre' WHEN 11 THEN 'noviembre' ELSE 'diciembre'
        END + ' de ' + CAST(YEAR(@Hasta) AS VARCHAR(4));

    IF @CodSap IS NULL
    BEGIN
        /* Se responde una fila igual, con la bolsa entera. La pantalla llama a
           esto al abrir el formulario, y un usuario sin Cod_Sap no deberia
           quedarse sin poder pedir permisos por esto. */
        SELECT MinutosAsignados = @Asignados,
               MinutosUsados = 0,
               MinutosDisponibles = @Asignados,
               VigenteHasta = @Hasta,
               Mensaje = 'No se pudo verificar el saldo del mes.';
        RETURN;
    END

    DECLARE @Usados INT =
    (
        SELECT ISNULL(SUM(
            CASE WHEN v.Horas LIKE '%:%' AND TRY_CAST(v.Horas AS TIME) IS NOT NULL
                 THEN DATEDIFF(MINUTE, CAST('00:00' AS TIME), TRY_CAST(v.Horas AS TIME))
                 ELSE 0 END), 0)
        FROM dbo.Vacaciones v
        WHERE v.IdTipoSolicitud = 1
          AND v.CodSap = @CodSap
          AND ISNULL(v.UsaPermisoMensual, 0) = 1
          AND v.IdVacaciones <> @IdVacaciones
          AND v.EstadoSolicitud NOT IN ('RECHAZADO', 'RECHAZADO GTH', 'ANULAR')
          AND YEAR(v.FechaDesde) = YEAR(@Fecha)
          AND MONTH(v.FechaDesde) = MONTH(@Fecha)
    );

    DECLARE @Disponibles INT = @Asignados - @Usados;
    IF @Disponibles < 0 SET @Disponibles = 0;

    SELECT MinutosAsignados  = @Asignados,
           MinutosUsados     = @Usados,
           MinutosDisponibles= @Disponibles,
           VigenteHasta      = @Hasta,
           /* El texto se arma aca para que pantalla y PDF digan lo mismo sin
              tener que repetir la redaccion en dos lenguajes distintos. */
           Mensaje = CASE
               WHEN @Disponibles = 0
                    THEN 'Agotado. Vuelve a estar disponible el 1 del mes siguiente.'
               ELSE 'Le queda ' + CAST(@Disponibles / 60 AS VARCHAR(3)) + 'h'
                    + RIGHT('0' + CAST(@Disponibles % 60 AS VARCHAR(2)), 2)
                    + ' del permiso mensual, disponible hasta el ' + @HastaTexto + '.'
           END;
END
GO
PRINT 'Sp_RTA_SaldoPermisoMensual actualizado con la fecha en formato largo.';
GO

/* ------------------------------------- 3. guardar el texto con el detalle */
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
    /* El saldo tal como se le mostro al colaborador al pedir el permiso. Con
       valor por defecto, para que una pantalla vieja en cache siga guardando. */
    @SaldoMensualTexto    VARCHAR(200)  = ''
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
            UsaPermisoMensual    = @UsaPermisoMensual,
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
PRINT 'Sp_RTA_GuardarDetallePermiso actualizado con SaldoMensualTexto.';
GO

/* --------------------------------------- 4. devolverlo para pantalla y PDF */
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
        UsaPermisoMensual    = ISNULL(v.UsaPermisoMensual, CAST(0 AS BIT)),
        SaldoMensualTexto    = ISNULL(v.SaldoMensualTexto,''),
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
        /* El plan de recuperacion, para que el PDF lo pueda mostrar sin una
           segunda consulta. */
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
PRINT 'Sp_RTA_ObtenerDetallePermiso actualizado con saldo mensual y recuperacion.';
GO
