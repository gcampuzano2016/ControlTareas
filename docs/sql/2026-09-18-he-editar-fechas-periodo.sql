/* ============================================================================
   Horas extras: corregir las fechas de un periodo ya creado
   ReporTarea  |  2026-09-18

   PENDIENTE DE EJECUTAR.

   ----------------------------------------------------------------------------
   Que problema resuelve

   Hasta hoy las fechas de un periodo no se podian corregir. Sp_RTA_HeCrearPeriodo
   busca el periodo por RANGO EXACTO, asi que al cambiar una fecha ya no lo
   encuentra e intenta crear uno nuevo; ahi choca con la validacion de
   solapamiento y devuelve -5, comparandose contra el mismo periodo que se queria
   corregir. El usuario ve "El rango se cruza con otro periodo ya abierto" sin
   forma de salir de ahi salvo crear un periodo nuevo y abandonar el anterior.

   Este procedimiento cambia las fechas de un periodo que YA EXISTE, por su
   IdPeriodo. Quien recalcula despues es NegHorasExtrasPantalla, llamando al
   AbrirPeriodo que ya existe: eso relee las horas aprobadas del rango nuevo,
   recalcula los sueldos al nuevo corte y respeta las filas corregidas a mano.
   Este procedimiento NO toca HE_Detalle.

   ----------------------------------------------------------------------------
   Respuestas:
      0  fechas actualizadas
     -1  el rango esta al reves o viene vacio
     -4  el periodo no existe
     -5  el rango nuevo se solapa con OTRO periodo
     -6  el periodo esta cerrado

   El -6 no es una formalidad. Un periodo cerrado ya se pago: moverle las fechas
   cambiaria el rango de un rol firmado, y el recalculo posterior reescribiria
   sueldos y totales que alguien ya cobro. Para corregir uno cerrado hay que
   reabrirlo primero, que es una accion con su propia auditoria y su propio
   permiso.

   Idempotente: DROP y CREATE. Poner las mismas fechas que ya tiene devuelve 0 y
   no cambia nada.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HE_Periodo')
BEGIN
    RAISERROR('No existe dbo.HE_Periodo: este script pertenece al modulo de horas extras. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

PRINT '== Horas extras: editar fechas del periodo - inicio ==';
GO

IF OBJECT_ID('dbo.Sp_RTA_HeEditarFechasPeriodo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeEditarFechasPeriodo;
GO

CREATE PROCEDURE dbo.Sp_RTA_HeEditarFechasPeriodo
    @IdPeriodo   INT,
    @FechaInicio DATE,
    @FechaFin    DATE,
    @Usuario     VARCHAR(50),
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    /* El NULL entra por aqui y no mas abajo: con NULL las comparaciones de rango
       dan UNKNOWN, no coinciden con nada, y el UPDATE escribiria NULL contra un
       NOT NULL en vez de devolver un codigo. Es el mismo orden que usa
       Sp_RTA_HeCrearPeriodo. */
    IF @FechaInicio IS NULL OR @FechaFin IS NULL OR @FechaInicio > @FechaFin
    BEGIN
        SELECT Respuestas = -1;
        RETURN;
    END

    DECLARE @Estado VARCHAR(20);

    SELECT @Estado = EstadoPeriodo
      FROM dbo.HE_Periodo
     WHERE IdPeriodo = @IdPeriodo;

    IF @Estado IS NULL
    BEGIN
        SELECT Respuestas = -4;
        RETURN;
    END

    IF @Estado <> 'Abierto'
    BEGIN
        SELECT Respuestas = -6;
        RETURN;
    END

    /* La misma formula de solapamiento que Sp_RTA_HeCrearPeriodo -dos rangos se
       cruzan si cada uno empieza antes de que el otro termine-, con la unica
       diferencia que es el corazon de este cambio: EXCLUIRSE A SI MISMO.

       Sin el IdPeriodo <> @IdPeriodo, todo periodo se solapa consigo mismo y
       ninguna edicion pasaria jamas. Eso es exactamente lo que ocurre hoy al
       intentar corregir las fechas por la via de crear. */
    IF EXISTS (SELECT 1 FROM dbo.HE_Periodo
                WHERE IdPeriodo <> @IdPeriodo
                  AND @FechaInicio <= FechaFin
                  AND FechaInicio  <= @FechaFin)
    BEGIN
        SELECT Respuestas = -5;
        RETURN;
    END

    /* La Descripcion se rearma del rango, igual que al crear: si no, queda
       diciendo las fechas viejas y el desplegable de la pantalla -que muestra la
       descripcion- mentiria sobre el periodo que acaba de corregirse.

       Fec_Modificacion y UsuarioCreacion no se tocan: la tabla no tiene columnas
       de "ultima modificacion" del periodo y UsuarioCreacion significa quien lo
       creo, no quien lo edito. Ip_Modificacion si existe y se actualiza. */
    UPDATE dbo.HE_Periodo
       SET FechaInicio     = @FechaInicio,
           FechaFin        = @FechaFin,
           Descripcion     = CONVERT(VARCHAR(10), @FechaInicio, 103) + ' - '
                           + CONVERT(VARCHAR(10), @FechaFin, 103),
           Ip_Modificacion = @Ip
     WHERE IdPeriodo = @IdPeriodo;

    SELECT Respuestas = 0;
END
GO

PRINT 'Sp_RTA_HeEditarFechasPeriodo actualizado.';
GO

SET NOEXEC OFF;
GO

PRINT '== Horas extras: editar fechas del periodo - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   Esperado: 1 fila.

SELECT nombre = name, creado = CONVERT(VARCHAR(20), modify_date, 120)
  FROM sys.procedures
 WHERE name = 'Sp_RTA_HeEditarFechasPeriodo';

   ============================================================================ */
