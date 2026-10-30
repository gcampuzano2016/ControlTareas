/* ============================================================================
   Vacaciones - las solicitudes del sistema descuentan del saldo
   ReporTarea  |  2026-10-30
   Plan: docs/superpowers/plans/2026-10-28-vacaciones-motor-de-saldos.md (F3)

   Hasta el 28-10 GTH pasaba a mano al Excel las solicitudes del sistema, y el
   Excel se cargaba en Vac_Movimiento. GTH decidio (30-10) no cargar mas el
   Excel: desde ahora las solicitudes del sistema entran solas.

   Crea:
     1. Vac_SolicitudIncluida   solicitudes anteriores al corte que GTH decide
                                que tambien descuenten (de la lista de
                                revision). Empieza vacia.
     2. Vw_RTA_VacMovimiento    Vac_Movimiento + las solicitudes del sistema,
                                con la forma de Vac_Movimiento. La leen el
                                motor y Sp_RTA_VacMovimientosOrigen.
     3. Sp_RTA_VacSolicitudesARevisar  la lista para GTH.

   Que solicitud del sistema entra (decisiones de GTH del 30-10):
     - Solo PROCESADO: la procesa GTH. Lo pendiente, rechazado o anulado no
       descuenta; si una procesada se anula, deja de descontar al instante
       (es una vista, no una copia).
     - Registrada DESPUES de la ultima carga del Excel (el corte es la
       Fecha_Registro mas reciente de Origen EXCEL), o incluida a mano en
       Vac_SolicitudIncluida. Lo anterior ya lo paso GTH al Excel.
     - Vacaciones (tipo 2): TotalDias, que ya viene sin los feriados
       (decision 1: el feriado no descuenta).
     - Permiso (tipo 1): solo con TratamientoExcedente = VACACIONES, y solo
       el excedente sobre la bolsa mensual (decision 3). Horas "HH:mm" como
       HtmlSolicitud.Minutos. Si uso la bolsa y no se guardo cuanto tenia
       (SaldoMensualMinutos NULL) no se inventa: no entra y sale en la lista.
     - Planificacion (tipo 3): nunca.
     - La persona se busca por cedula en Vac_Colaborador.
   IdMovimiento = -IdVacaciones, para no chocar con Vac_Movimiento.

   Despues de este script hay que volver a correr el motor
   (2026-10-28-vacaciones-motor.sql) y la pantalla de saldos
   (2026-10-28-vacaciones-saldos-pantalla.sql): ahora leen la vista.
   Repetible. Sin tildes a proposito (ver el esquema).
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------------------------------------- 1. Vac_SolicitudIncluida ------- */
IF OBJECT_ID('dbo.Vac_SolicitudIncluida', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vac_SolicitudIncluida
    (
        IdVacaciones     BIGINT        NOT NULL CONSTRAINT PK_Vac_SolicitudIncluida PRIMARY KEY,
        Motivo           NVARCHAR(500) NOT NULL,
        Usuario_Registro VARCHAR(50)   NOT NULL,
        Fecha_Registro   DATETIME      NOT NULL CONSTRAINT DF_Vac_SolicitudIncluida_Fecha DEFAULT GETDATE()
    );
    PRINT 'Vac_SolicitudIncluida creada.';
END
GO

/* ----------------------------------------- 2. Vw_RTA_VacMovimiento ------- */
IF OBJECT_ID('dbo.Vw_RTA_VacMovimiento', 'V') IS NULL
    EXEC ('CREATE VIEW dbo.Vw_RTA_VacMovimiento AS SELECT 1 AS x');
GO
ALTER VIEW dbo.Vw_RTA_VacMovimiento
AS
SELECT m.IdMovimiento, m.IdColaborador, m.Tipo, m.Estado, m.FechaDesde, m.FechaHasta,
       m.Dias, m.Horas, m.Minutos, m.BeneficioDos, m.Observacion, m.Origen, m.Orden
FROM dbo.Vac_Movimiento m
UNION ALL
SELECT CONVERT(INT, -s.IdVacaciones), c.IdColaborador,
       CASE s.IdTipoSolicitud WHEN 2 THEN 'VACACIONES' ELSE 'PERMISO POR HORAS' END,
       'PROCESADO',
       CAST(s.FechaDesde AS DATE), CAST(s.FechaHasta AS DATE),
       CASE WHEN s.IdTipoSolicitud = 2 THEN s.TotalDias END,
       CASE WHEN s.IdTipoSolicitud = 1 THEN CAST(0 AS DECIMAL(9,2)) END,
       CASE WHEN s.IdTipoSolicitud = 1 THEN CAST(x.Excedente AS DECIMAL(9,2)) END,
       CAST(0 AS BIT),
       CONVERT(NVARCHAR(1000), N'Solicitud ' + CONVERT(NVARCHAR(20), s.IdVacaciones) + N' del sistema'
               /* La observacion viene con tabulaciones y saltos de linea del formulario. */
               + ISNULL(N' - ' + NULLIF(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(s.Observacion, CHAR(9), ' '), CHAR(13), ' '), CHAR(10), ' '))), ''), N'')),
       'SISTEMA',
       CONVERT(INT, 1000000 + s.IdVacaciones)
FROM dbo.Vacaciones s
JOIN dbo.Vac_Colaborador c ON c.Cedula = LTRIM(RTRIM(s.Cedula))
CROSS APPLY (SELECT MAX(e.Fecha_Registro) AS Corte FROM dbo.Vac_Movimiento e WHERE e.Origen = 'EXCEL') k
CROSS APPLY (SELECT CASE WHEN s.Horas LIKE '%[0-9]:[0-9]%' AND s.Horas NOT LIKE '%:%:%'
                         THEN TRY_CONVERT(INT, LEFT(s.Horas, CHARINDEX(':', s.Horas) - 1)) * 60
                              + TRY_CONVERT(INT, SUBSTRING(s.Horas, CHARINDEX(':', s.Horas) + 1, 10))
                    END AS Permiso) h
CROSS APPLY (SELECT CASE WHEN ISNULL(s.UsaPermisoMensual, 0) = 1 THEN h.Permiso - s.SaldoMensualMinutos
                         ELSE h.Permiso END AS Excedente) x
WHERE s.EstadoSolicitud = 'PROCESADO'
  AND s.FechaDesde IS NOT NULL
  AND (s.FechaRegistro > k.Corte
       OR EXISTS (SELECT 1 FROM dbo.Vac_SolicitudIncluida i WHERE i.IdVacaciones = s.IdVacaciones))
  AND (   (s.IdTipoSolicitud = 2 AND s.TotalDias > 0)
       OR (s.IdTipoSolicitud = 1 AND s.TratamientoExcedente = 'VACACIONES' AND x.Excedente > 0));
GO
PRINT 'Vw_RTA_VacMovimiento lista.';
GO

/* ------------------------------ 3. Sp_RTA_VacSolicitudesARevisar ------- */
IF OBJECT_ID('dbo.Sp_RTA_VacSolicitudesARevisar', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacSolicitudesARevisar AS RETURN 0');
GO
/* Vacaciones y permisos con cargo a vacaciones de @Desde en adelante, que
   pueden descontar (no rechazados ni anulados) y que el saldo NO esta
   contando: ni entran por la vista, ni el Excel tiene algo vigente de esa
   persona en fechas que se crucen. Se compara por fechas y no por tipo: GTH
   pasaba al Excel los permisos de dia completo como VACACIONES de 1 dia.
   Para que GTH decida una por una. */
ALTER PROCEDURE dbo.Sp_RTA_VacSolicitudesARevisar
    @Desde DATE = '20260101'
AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.IdVacaciones,
           CASE s.IdTipoSolicitud WHEN 2 THEN 'VACACIONES' ELSE 'PERMISO POR HORAS' END AS Tipo,
           s.EstadoSolicitud, s.Colaborador, c.IdColaborador,
           CAST(s.FechaRegistro AS DATE) AS Registrada,
           CAST(s.FechaDesde AS DATE) AS Desde, CAST(s.FechaHasta AS DATE) AS Hasta,
           s.TotalDias, s.Horas,
           CASE WHEN c.IdColaborador IS NULL                         THEN 'Su cedula no esta en Vac_Colaborador'
                WHEN s.EstadoSolicitud <> 'PROCESADO'                THEN 'Pendiente: descontara si se procesa y se incluye'
                WHEN s.IdTipoSolicitud = 1 AND ISNULL(s.UsaPermisoMensual, 0) = 1 AND s.SaldoMensualMinutos IS NULL
                                                                     THEN 'Uso la bolsa mensual pero no se guardo cuanto tenia'
                ELSE 'Procesada antes del corte y no esta en el Excel' END AS Razon
    FROM dbo.Vacaciones s
    LEFT JOIN dbo.Vac_Colaborador c ON c.Cedula = LTRIM(RTRIM(s.Cedula))
    WHERE s.FechaDesde >= @Desde
      AND s.EstadoSolicitud IN ('POR APROBAR', 'APROBADO', 'PROCESADO')
      AND (s.IdTipoSolicitud = 2 OR (s.IdTipoSolicitud = 1 AND s.TratamientoExcedente = 'VACACIONES'))
      AND NOT EXISTS (SELECT 1 FROM dbo.Vw_RTA_VacMovimiento v WHERE v.IdMovimiento = CONVERT(INT, -s.IdVacaciones))
      AND NOT EXISTS (SELECT 1 FROM dbo.Vac_Movimiento e
                       WHERE e.IdColaborador = c.IdColaborador AND e.Origen = 'EXCEL' AND e.Estado <> 'DADA BAJA'
                         AND e.FechaDesde <= CAST(ISNULL(s.FechaHasta, s.FechaDesde) AS DATE)
                         AND ISNULL(e.FechaHasta, e.FechaDesde) >= CAST(s.FechaDesde AS DATE))
      /* Lo registrado despues del corte que todavia no se proceso entra solo al procesarse: no es para revisar. */
      AND NOT (s.FechaRegistro > (SELECT MAX(e.Fecha_Registro) FROM dbo.Vac_Movimiento e WHERE e.Origen = 'EXCEL')
               AND s.EstadoSolicitud <> 'PROCESADO')
    ORDER BY s.Colaborador, s.FechaDesde;
END
GO
PRINT 'Sp_RTA_VacSolicitudesARevisar listo.';
GO

/* ------------------------------------------------------------ REVERSA ------
   Volver a correr las versiones anteriores del motor y de la pantalla de
   saldos (git), y luego:
DROP PROCEDURE dbo.Sp_RTA_VacSolicitudesARevisar;
DROP VIEW dbo.Vw_RTA_VacMovimiento;
DROP TABLE dbo.Vac_SolicitudIncluida;
---------------------------------------------------------------------------- */
