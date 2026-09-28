/* ============================================================================
   Horas Extras: tiempo registrado fuera de horario
   ReporTarea  |  2026-09-28

   Crea Sp_RTA_HeFueraDeHorario. SOLO LECTURA: no escribe en ninguna tabla.
   Nadie lo llama hasta que se publiquen los binarios, asi que puede correrse
   antes que ellos sin efecto en la pantalla.

   Que devuelve, para el rango del periodo (por Det_Fch_RegDetalleIni, la fecha
   del TRABAJO, igual que Sp_RTA_HeHorasAprobadas):

     1. Los segmentos fuera de horario de los tramos que NO estan pagados. La
        clasificacion la hace FN_RTA_ClasificarTramosHorario, la misma que usa
        el registro, con el horario que la persona tenia ese dia.
     2. Los intervalos de los tramos pagados (estado 2 y tipo 1 o 2), para que
        CapaNegocio descuente lo que ya cubren.

   Restar y unir intervalos NO se hace aqui a proposito: vive en
   NegHeFueraDeHorario, que tiene pruebas. En T-SQL no las tendria.

   El puente de identidad es el de Sp_RTA_HeHorasAprobadas, en el mismo
   sentido: tarea -> usuario -> cedula -> Empleados -> maestro. Al reves, una
   cedula con seis cuentas sumaria seis veces.

   Re-correrlo es inofensivo: el procedimiento es nuevo y no lo recrea ninguna
   otra fase. Si una fase posterior lo recrea con otra firma, este script tiene
   que llevar el guard SET NOEXEC de las fases 2, 4 y 5.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_HeFueraDeHorario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeFueraDeHorario;
GO

CREATE PROCEDURE dbo.Sp_RTA_HeFueraDeHorario
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    /* 1. Segmentos fuera de horario de los tramos no pagados.
          TipoHora 0 (horas normales) se descarta en el WHERE. Los tramos que
          cruzan la medianoche o caen en un dia sin configuracion no devuelven
          nada de la funcion: el registro ya los rechaza, y aqui no se inventa
          una clasificacion para ellos. */
    SELECT c.IdEmpleado,
           CAST(d.Id_RegDetTareas AS BIGINT)                   AS IdRegDetTareas,
           d.Det_Fch_RegDetalleIni                             AS TramoInicio,
           d.Det_Fch_RegDetalleFin                             AS TramoFin,
           s.TramoInicio                                       AS SegmentoInicio,
           s.TramoFin                                          AS SegmentoFin,
           CAST(s.TipoHora AS INT)                             AS TipoHora,
           CAST(d.Det_Horas_Extras_Estado AS INT)              AS EstadoTramo,
           CAST(ISNULL(d.Det_Horas_Extras_Tipo, 0) AS INT)     AS TipoRegistrado,
           LEFT(ISNULL(d.Det_Det_Tarea, ''), 200)              AS Actividad
      FROM dbo.R_DetTareasAranda d
      JOIN dbo.R_Usuarios u ON u.Cod_Usuario = d.Id_Responsable
      JOIN dbo.Empleados  e ON LTRIM(RTRIM(e.Cedula)) = LTRIM(RTRIM(u.Cedula))
      JOIN dbo.HE_ColaboradorParametro c ON c.IdEmpleado = e.IdEmpleado AND c.Estado = '1'
     CROSS APPLY dbo.FN_RTA_ClasificarTramosHorario(d.Id_Responsable,
                                                    d.Det_Fch_RegDetalleIni,
                                                    d.Det_Fch_RegDetalleFin) s
     WHERE d.Det_Fch_RegDetalleIni >= @FechaInicio
       AND d.Det_Fch_RegDetalleIni <  DATEADD(DAY, 1, @FechaFin)
       AND d.Det_Fch_RegDetalleFin IS NOT NULL
       AND s.TipoHora IN (1, 2)
       AND d.Det_Horas_Extras_Estado IN (0, 1, 2, 3)
       AND NOT (d.Det_Horas_Extras_Estado = 2 AND ISNULL(d.Det_Horas_Extras_Tipo, 0) IN (1, 2))
     ORDER BY c.IdEmpleado, s.TramoInicio;

    /* 2. Lo ya pagado: el intervalo COMPLETO del tramo, no solo su parte fuera
          de horario. Un minuto pagado no aparece en ninguna otra columna. */
    SELECT c.IdEmpleado,
           d.Det_Fch_RegDetalleIni AS Inicio,
           d.Det_Fch_RegDetalleFin AS Fin
      FROM dbo.R_DetTareasAranda d
      JOIN dbo.R_Usuarios u ON u.Cod_Usuario = d.Id_Responsable
      JOIN dbo.Empleados  e ON LTRIM(RTRIM(e.Cedula)) = LTRIM(RTRIM(u.Cedula))
      JOIN dbo.HE_ColaboradorParametro c ON c.IdEmpleado = e.IdEmpleado AND c.Estado = '1'
     WHERE d.Det_Fch_RegDetalleIni >= @FechaInicio
       AND d.Det_Fch_RegDetalleIni <  DATEADD(DAY, 1, @FechaFin)
       AND d.Det_Fch_RegDetalleFin IS NOT NULL
       AND d.Det_Horas_Extras_Estado = 2
       AND ISNULL(d.Det_Horas_Extras_Tipo, 0) IN (1, 2)
     ORDER BY c.IdEmpleado, d.Det_Fch_RegDetalleIni;
END
GO

/* ------------------------------------------------------ VERIFICACION ------ */
/* Sin GO entre el DECLARE y el veredicto: @Fallos no sobreviviria al lote. */
SET NOCOUNT ON;
DECLARE @Fallos INT = 0;

IF OBJECT_ID('dbo.Sp_RTA_HeFueraDeHorario','P') IS NULL
BEGIN
    PRINT 'FALLO: Sp_RTA_HeFueraDeHorario no existe.';
    SET @Fallos += 1;
END

IF (SELECT COUNT(*) FROM sys.parameters
     WHERE object_id = OBJECT_ID('dbo.Sp_RTA_HeFueraDeHorario')) <> 2
BEGIN
    PRINT 'FALLO: Sp_RTA_HeFueraDeHorario deberia tener exactamente 2 parametros.';
    SET @Fallos += 1;
END

/* Que corra sobre un periodo real sin error. Devuelve las dos grillas: son
   solo lectura y se pueden ignorar. */
DECLARE @Ini DATE, @Fin DATE;
SELECT TOP (1) @Ini = FechaInicio, @Fin = FechaFin FROM dbo.HE_Periodo ORDER BY FechaInicio DESC;

IF @Ini IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC dbo.Sp_RTA_HeFueraDeHorario @FechaInicio = @Ini, @FechaFin = @Fin;
    END TRY
    BEGIN CATCH
        PRINT 'FALLO: Sp_RTA_HeFueraDeHorario fallo al ejecutarse: ' + ERROR_MESSAGE();
        SET @Fallos += 1;
    END CATCH
END

IF @Fallos = 0
    PRINT 'OK: Sp_RTA_HeFueraDeHorario listo.';
ELSE
    PRINT 'VEREDICTO: FALLO (' + CAST(@Fallos AS VARCHAR(10)) + ' comprobaciones). Revisar arriba.';
GO
