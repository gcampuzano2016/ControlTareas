/* ============================================================================
   Vacaciones - pantalla de saldos para GTH: lecturas de apoyo
   ReporTarea  |  2026-10-28
   Plan: docs/superpowers/plans/2026-10-28-vacaciones-motor-de-saldos.md (F5)
   Crea:
     1. Sp_RTA_VacColaboradores      el maestro, para poner nombre y cedula a
                                     lo que devuelve Sp_RTA_VacCalcular
     2. Sp_RTA_VacMovimientosOrigen  lo que se cargo de cada movimiento (dias,
                                     horas, observacion), para la ficha
   Los saldos los calcula Sp_RTA_VacCalcular; aqui no se calcula nada.
   Solo leen. Repetible. Sin tildes a proposito (ver el esquema).
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------- 1. Sp_RTA_VacColaboradores --- */
IF OBJECT_ID('dbo.Sp_RTA_VacColaboradores', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacColaboradores AS RETURN 0');
GO
ALTER PROCEDURE dbo.Sp_RTA_VacColaboradores
    @IdColaborador INT = NULL      -- NULL = todos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdColaborador, Cedula, Nombre, Empresa, Area, Cargo, FechaIngreso, Estado,
           FechaSalida, FechaCorte, SaldoCorte, Observacion
    FROM dbo.Vac_Colaborador
    WHERE @IdColaborador IS NULL OR IdColaborador = @IdColaborador
    ORDER BY Nombre;
END
GO

/* --------------------------------------- 2. Sp_RTA_VacMovimientosOrigen --- */
IF OBJECT_ID('dbo.Sp_RTA_VacMovimientosOrigen', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacMovimientosOrigen AS RETURN 0');
GO
ALTER PROCEDURE dbo.Sp_RTA_VacMovimientosOrigen
    @IdColaborador INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdMovimiento, Dias, Horas, Minutos, BeneficioDos, Observacion, Origen
    FROM dbo.Vac_Movimiento
    WHERE IdColaborador = @IdColaborador;
END
GO

PRINT 'Sp_RTA_VacColaboradores y Sp_RTA_VacMovimientosOrigen listos.';
GO
