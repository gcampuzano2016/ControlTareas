/* ============================================================================
   Vacaciones - nomina y provision (dias y valor de vacaciones pendientes)
   ReporTarea  |  2026-10-29
   Plan: docs/superpowers/plans/2026-10-28-vacaciones-motor-de-saldos.md (F5)
   Crea:
     1. Vac_Sueldo        sueldo mensual para vacaciones, POR CEDULA (la carga
                          del Excel recrea Vac_Colaborador con Id nuevos).
                          Solo para quien no esta en HE_Salario: si la persona
                          tiene historial ahi, manda HE_Salario.
     2. Sp_RTA_VacSueldos tres resultados: el historial de HE_Salario de cada
                          IdColaborador (por la cedula de Empleados), sus filas
                          de Vac_Sueldo y el parametro DIAS_MES. El sueldo
                          vigente lo elige C# (NegHorasExtras.SalarioVigente,
                          la misma regla que horas extras y el certificado).
   Por que una tabla aparte y no cargar HE_Salario: HE_Salario lo leen tambien
   el Tablero de Costos (valoriza horas con el) y horas extras. Al 2026-10-29
   tiene el sueldo de 61 de los 95 activos, todos iguales al Excel; los otros
   34 solo estan en el Excel. Decidido el 2026-10-29.
   Los sueldos se cargan con carga-generada/vacaciones-sueldos.sql (generar-
   carga-vacaciones.py): son datos personales y no van al repositorio.
   Repetible. No toca tablas existentes. Sin tildes a proposito.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* --------------------------------------------------- 1. Vac_Sueldo ------- */
IF OBJECT_ID('dbo.Vac_Sueldo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vac_Sueldo
    (
        IdSueldo           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vac_Sueldo PRIMARY KEY,
        Cedula             VARCHAR(13)   NOT NULL,
        Monto              DECIMAL(12,2) NOT NULL CONSTRAINT CK_Vac_Sueldo_Monto CHECK (Monto > 0),
        FechaVigenciaDesde DATE          NOT NULL,        -- 1900-01-01 = sin fecha (el del Excel)
        Origen             VARCHAR(10)   NOT NULL
                           CONSTRAINT CK_Vac_Sueldo_Origen CHECK (Origen IN ('EXCEL', 'SISTEMA')),
        Usuario_Registro   VARCHAR(50)   NOT NULL,
        Fecha_Registro     DATETIME      NOT NULL CONSTRAINT DF_Vac_Sueldo_Fecha DEFAULT GETDATE(),
        CONSTRAINT UQ_Vac_Sueldo_Cedula_Fecha UNIQUE (Cedula, FechaVigenciaDesde)
    );
    PRINT 'Vac_Sueldo creada.';
END
GO

/* ---------------------------------------------- 2. Sp_RTA_VacSueldos ------- */
IF OBJECT_ID('dbo.Sp_RTA_VacSueldos', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacSueldos AS RETURN 0');
GO
ALTER PROCEDURE dbo.Sp_RTA_VacSueldos
AS
BEGIN
    SET NOCOUNT ON;

    /* 1. HE_Salario. DISTINCT: una cedula con dos filas en Empleados (mismo
          IdEmpleado) no debe duplicar el historial. */
    SELECT DISTINCT v.IdColaborador, s.Monto, s.FechaVigenciaDesde, Origen = ISNULL(s.Origen, '')
    FROM dbo.Vac_Colaborador v
    JOIN dbo.Empleados e ON LTRIM(RTRIM(e.Cedula)) = v.Cedula
    JOIN dbo.HE_Salario s ON s.IdEmpleado = e.IdEmpleado
    WHERE s.Estado = '1'
    ORDER BY v.IdColaborador, s.FechaVigenciaDesde;

    /* 2. Vac_Sueldo */
    SELECT v.IdColaborador, s.Monto, s.FechaVigenciaDesde, s.Origen
    FROM dbo.Vac_Colaborador v
    JOIN dbo.Vac_Sueldo s ON s.Cedula = v.Cedula
    ORDER BY v.IdColaborador, s.FechaVigenciaDesde;

    /* 3. Dias del mes para el valor diario (sueldo / DIAS_MES) */
    SELECT Valor FROM dbo.Vac_Parametro WHERE Clave = 'DIAS_MES';
END
GO

PRINT 'Vac_Sueldo y Sp_RTA_VacSueldos listos.';
GO
