/* ============================================================================
   Etapa 4b de CB-GAP-POL-01: el permiso mensual de 3 horas.

   1. Vacaciones.UsaPermisoMensual : la casilla nueva del formulario
   2. Sp_RTA_SaldoPermisoMensual   : cuanto le queda a alguien en un mes

   Se puede ejecutar varias veces sin efecto adicional. No modifica ni una fila.
   Base: ReporTarea
   ============================================================================

   La regla
   --------
   Tres horas por colaborador y por mes calendario, del 1 al ultimo dia. No se
   acumula: lo que no se usa en septiembre no pasa a octubre. Al llegar a cero,
   la opcion queda deshabilitada hasta el dia 1 siguiente.

   El mes lo define FechaDesde, no FechaRegistro: la bolsa es del mes en que la
   persona se ausenta, no del mes en que lo pidio. Alguien que el 29 de agosto
   pide un permiso para el 2 de septiembre esta usando la bolsa de septiembre.

   Que cuenta como consumido, y por que no solo lo aprobado
   -------------------------------------------------------
   La especificacion dice "cada vez que se aprueba un permiso se descuenta". Si
   se contara solo lo aprobado, el tope no topa: alguien pide dos permisos de
   dos horas el mismo mes, ninguno esta aprobado todavia, los dos pasan el
   control porque el saldo se ve intacto, y al aprobarlos quedan cuatro horas
   consumidas de una bolsa de tres.

   Asi que cuenta todo lo que no esta rechazado ni anulado, incluido lo que esta
   POR APROBAR. Una solicitud pendiente reserva sus horas. Si la rechazan, las
   horas vuelven solas: dejan de cumplir el filtro. El efecto visible es que
   mientras un permiso espera aprobacion su tiempo ya no esta disponible, que es
   lo correcto: esta comprometido.

   El formato de Horas
   -------------------
   Se guarda como HH:MM en 1403 de 1437 permisos; los otros 34 tienen el texto
   '0'. Por eso la conversion pasa por TRY_CAST y lo que no se entiende suma
   cero, en vez de reventar la consulta del saldo por un dato viejo mal escrito.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ------------------------------------------------------------- 1. la casilla */
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Vacaciones' AND COLUMN_NAME = 'UsaPermisoMensual')
BEGIN
    ALTER TABLE dbo.Vacaciones ADD UsaPermisoMensual BIT NULL;
    PRINT 'Vacaciones.UsaPermisoMensual creada. NULL en lo historico: nadie la uso antes.';
END
ELSE
    PRINT 'Vacaciones.UsaPermisoMensual ya existia.';
GO

/* --------------------------------------------------------- 2. saldo del mes */
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

    IF @CodSap IS NULL
    BEGIN
        /* Se responde una fila igual, con la bolsa entera. La pantalla llama a
           esto al abrir el formulario, y un usuario sin Cod_Sap no deberia
           quedarse sin poder pedir permisos por esto. */
        SELECT MinutosAsignados = @Asignados,
               MinutosUsados = 0,
               MinutosDisponibles = @Asignados,
               VigenteHasta = EOMONTH(@Fecha),
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

    DECLARE @Hasta DATE = EOMONTH(@Fecha);

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
                    + ' del permiso mensual, disponible hasta el '
                    + CONVERT(VARCHAR(10), @Hasta, 105) + '.'
           END;
END
GO
PRINT 'Sp_RTA_SaldoPermisoMensual creado.';
GO

/* ------------------------------------------------------------------ pruebas
   Para correr a mano. No forman parte del despliegue.

-- Alguien sin permisos mensuales usados: debe dar 180 disponibles.
EXEC dbo.Sp_RTA_SaldoPermisoMensual @Cod_Usuario = '0000', @Fecha = '2026-09-15';

-- Un usuario que no existe: debe responder igual, con la bolsa entera.
EXEC dbo.Sp_RTA_SaldoPermisoMensual @Cod_Usuario = 'NO_EXISTE', @Fecha = '2026-09-15';

-- Sin fecha: toma el mes en curso.
EXEC dbo.Sp_RTA_SaldoPermisoMensual @Cod_Usuario = '0000';
   ------------------------------------------------------------------------- */
