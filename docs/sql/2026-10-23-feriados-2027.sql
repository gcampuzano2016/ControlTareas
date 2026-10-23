/* ============================================================================
   Feriados nacionales 2027 en dbo.Feriado (dia de descanso efectivo, como 2026)
   ReporTarea  |  2026-10-23
   Fuente: calendario 2026-2030 anunciado por el Viceministerio de Turismo el
   2025-09-01 (publicado por Diario Expreso). Confirmar contra el decreto o el
   Ministerio del Trabajo; los traslados de noviembre difieren entre fuentes.
   Repetible: no inserta una fecha que ya exista. Lo usan los plazos en dias
   habiles de Procesos (Entrega 2) y FN_RTA_ClasificarTramosHorario.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @F TABLE (Fecha DATE PRIMARY KEY, Descripcion NVARCHAR(200));
INSERT INTO @F (Fecha, Descripcion) VALUES
 ('2027-01-01', N'A' + NCHAR(241) + N'o Nuevo'),
 ('2027-02-08', N'Carnaval - lunes'),
 ('2027-02-09', N'Carnaval - martes'),
 ('2027-03-26', N'Viernes Santo'),
 ('2027-04-30', N'D' + NCHAR(237) + N'a del Trabajo - descanso trasladado'),
 ('2027-05-24', N'Batalla del Pichincha'),
 ('2027-08-09', N'Primer Grito de la Independencia - descanso trasladado'),
 ('2027-10-08', N'Independencia de Guayaquil - descanso trasladado'),
 ('2027-11-01', N'D' + NCHAR(237) + N'a de los Difuntos - descanso trasladado'),
 ('2027-11-02', N'Independencia de Cuenca - descanso trasladado'),
 ('2027-12-25', N'Navidad');

BEGIN TRANSACTION;
INSERT INTO dbo.Feriado (Fecha, Descripcion, Activo, UsuarioRegistro, FechaRegistro)
SELECT f.Fecha, f.Descripcion, 1, 'SCRIPT_2027', SYSDATETIME()
  FROM @F AS f
 WHERE NOT EXISTS (SELECT 1 FROM dbo.Feriado AS x WHERE x.Fecha = f.Fecha);
PRINT 'Feriados insertados: ' + CONVERT(VARCHAR(10), @@ROWCOUNT);
COMMIT TRANSACTION;

DECLARE @n INT = (SELECT COUNT(*) FROM dbo.Feriado WHERE Activo = 1 AND YEAR(Fecha) = 2027);
SELECT CONVERT(VARCHAR(10), Fecha, 120) AS Fecha, Descripcion, Activo
  FROM dbo.Feriado WHERE YEAR(Fecha) = 2027 ORDER BY Fecha;
PRINT CASE WHEN @n = 11 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA (' + CONVERT(VARCHAR(10), @n) + ' feriados activos en 2027)' END;
