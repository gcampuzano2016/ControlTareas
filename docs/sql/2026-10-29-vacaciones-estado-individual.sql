/* ============================================================================
   Vacaciones - estado individual en PDF para firmar
   ReporTarea  |  2026-10-29
   Plan: docs/superpowers/plans/2026-10-28-vacaciones-motor-de-saldos.md (F5)
   Crea:
     1. Vac_EstadoEmitido           una fila por PDF generado: el folio que va
                                    impreso y los numeros que mostraba, para que
                                    el papel firmado se pueda contrastar despues
                                    con el sistema aunque el saldo cambie.
     2. Sp_RTA_VacEstadoRegistrar   inserta y devuelve el folio.
   Repetible. No toca tablas existentes. Sin claves foraneas, como el resto.
   Sin tildes a proposito (ver 2026-10-28-vacaciones-esquema.sql).
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* --------------------------------------------- 1. Vac_EstadoEmitido ------- */
IF OBJECT_ID('dbo.Vac_EstadoEmitido', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vac_EstadoEmitido
    (
        IdEstado          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vac_EstadoEmitido PRIMARY KEY,   -- el folio
        IdColaborador     INT           NOT NULL,
        FechaCalculo      DATE          NOT NULL,        -- la del saldo (la de salida, si es inactivo)
        Disponible        DECIMAL(12,4) NOT NULL,
        Proporcional      DECIMAL(12,4) NOT NULL,
        UsadoEnCurso      DECIMAL(12,4) NOT NULL,
        Saldo             DECIMAL(12,2) NOT NULL,
        PorVencer         DECIMAL(12,4) NOT NULL,
        EstadoSaldo       VARCHAR(12)   NOT NULL,
        Usuario_Registro  VARCHAR(50)   NOT NULL,        -- quien lo genero (Cod_Usuario de la sesion)
        Fecha_Registro    DATETIME      NOT NULL CONSTRAINT DF_Vac_EstadoEmitido_Fecha DEFAULT GETDATE(),
        Ip                VARCHAR(45)   NULL
    );
    CREATE INDEX IX_Vac_EstadoEmitido_Colaborador ON dbo.Vac_EstadoEmitido (IdColaborador, Fecha_Registro);
    PRINT 'Vac_EstadoEmitido creada.';
END
GO

/* -------------------------------------- 2. Sp_RTA_VacEstadoRegistrar ------- */
IF OBJECT_ID('dbo.Sp_RTA_VacEstadoRegistrar', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacEstadoRegistrar AS RETURN 0');
GO
ALTER PROCEDURE dbo.Sp_RTA_VacEstadoRegistrar
    @IdColaborador INT,
    @FechaCalculo  DATE,
    @Disponible    DECIMAL(12,4),
    @Proporcional  DECIMAL(12,4),
    @UsadoEnCurso  DECIMAL(12,4),
    @Saldo         DECIMAL(12,2),
    @PorVencer     DECIMAL(12,4),
    @EstadoSaldo   VARCHAR(12),
    @Usuario       VARCHAR(50),
    @Ip            VARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Vac_EstadoEmitido
        (IdColaborador, FechaCalculo, Disponible, Proporcional, UsadoEnCurso, Saldo, PorVencer, EstadoSaldo, Usuario_Registro, Ip)
    VALUES
        (@IdColaborador, @FechaCalculo, @Disponible, @Proporcional, @UsadoEnCurso, @Saldo, @PorVencer, @EstadoSaldo, @Usuario, @Ip);
    SELECT CONVERT(INT, SCOPE_IDENTITY()) AS IdEstado;
END
GO

PRINT 'Vac_EstadoEmitido y Sp_RTA_VacEstadoRegistrar listos.';
GO
