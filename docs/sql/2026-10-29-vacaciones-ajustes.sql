/* ============================================================================
   Vacaciones - mantenimiento de saldo de corte y de compra de vacaciones
   ReporTarea  |  2026-10-29
   Plan: docs/superpowers/plans/2026-10-28-vacaciones-motor-de-saldos.md (F5)
   Crea:
     1. Vac_Bitacora                 una fila por cada cambio de corte o de
                                     compra: antes, despues, motivo, quien.
     2. Sp_RTA_VacCorteGuardar       pone, cambia o quita el saldo de corte.
     3. Sp_RTA_VacCorteSimular       el saldo como quedaria con otro corte, sin
                                     guardar nada (cambia y deshace).
     4. Sp_RTA_VacCompraRegistrar    registra una compra (Vac_Movimiento,
                                     Origen SISTEMA): el motor ya la descuenta.
     5. Sp_RTA_VacCompraAnular       la pasa a DADA BAJA; no se borra nada.
     6. Sp_RTA_VacAjustesHistorial   la bitacora de una persona y sus folios.
   El cambio y su fila de bitacora van en la misma transaccion: o los dos o
   ninguno. Los de corte comprueban que el corte siga siendo el que vio la
   pantalla (dos personas de GTH editando a la vez).
   Devuelven Resultado: OK, NO EXISTE, CAMBIADO, SIN CAMBIOS, NO ES COMPRA,
   YA ANULADA. Los textos para el usuario los pone CapaNegocio.
   Repetible. No toca tablas existentes. Sin claves foraneas, como el resto.
   Sin tildes a proposito (ver 2026-10-28-vacaciones-esquema.sql).
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* --------------------------------------------------- 1. Vac_Bitacora ------- */
IF OBJECT_ID('dbo.Vac_Bitacora', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vac_Bitacora
    (
        IdBitacora        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vac_Bitacora PRIMARY KEY,
        IdColaborador     INT           NOT NULL,
        Tipo              VARCHAR(10)   NOT NULL
                          CONSTRAINT CK_Vac_Bitacora_Tipo CHECK (Tipo IN ('CORTE', 'COMPRA')),
        Accion            VARCHAR(10)   NOT NULL
                          CONSTRAINT CK_Vac_Bitacora_Accion CHECK (Accion IN ('REGISTRA', 'CAMBIA', 'QUITA', 'ANULA')),
        IdMovimiento      INT           NULL,            -- la compra
        FechaAntes        DATE          NULL,            -- corte: fecha de corte; compra: fecha de la compra
        DiasAntes         DECIMAL(9,2)  NULL,            -- corte: saldo de corte; compra: dias comprados
        FechaDespues      DATE          NULL,
        DiasDespues       DECIMAL(9,2)  NULL,
        Motivo            NVARCHAR(1000) NOT NULL,
        Usuario_Registro  VARCHAR(50)   NOT NULL,        -- Cod_Usuario de la sesion
        Fecha_Registro    DATETIME      NOT NULL CONSTRAINT DF_Vac_Bitacora_Fecha DEFAULT GETDATE(),
        Ip                VARCHAR(45)   NULL
    );
    CREATE INDEX IX_Vac_Bitacora_Colaborador ON dbo.Vac_Bitacora (IdColaborador, Fecha_Registro);
    PRINT 'Vac_Bitacora creada.';
END
GO

/* ------------------------------------------ 2. Sp_RTA_VacCorteGuardar ------- */
IF OBJECT_ID('dbo.Sp_RTA_VacCorteGuardar', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacCorteGuardar AS RETURN 0');
GO
ALTER PROCEDURE dbo.Sp_RTA_VacCorteGuardar
    @IdColaborador  INT,
    @FechaCorte     DATE,              -- NULL con @SaldoCorte NULL = quitar el corte
    @SaldoCorte     DECIMAL(9,2),
    @FechaAnterior  DATE,              -- el corte que mostraba la pantalla
    @SaldoAnterior  DECIMAL(9,2),
    @Motivo         NVARCHAR(1000),
    @Usuario        VARCHAR(50),
    @Ip             VARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF (@FechaCorte IS NULL AND @SaldoCorte IS NOT NULL) OR (@FechaCorte IS NOT NULL AND @SaldoCorte IS NULL)
    BEGIN
        RAISERROR('La fecha y el saldo de corte van juntos.', 16, 1);
        RETURN;
    END

    DECLARE @F DATE, @S DECIMAL(9,2);
    BEGIN TRANSACTION;

    SELECT @F = FechaCorte, @S = SaldoCorte
    FROM dbo.Vac_Colaborador WITH (UPDLOCK, HOLDLOCK)
    WHERE IdColaborador = @IdColaborador;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 'NO EXISTE' AS Resultado;
        RETURN;
    END

    IF NOT (ISNULL(@F, '19000101') = ISNULL(@FechaAnterior, '19000101') AND ISNULL(@S, -99999) = ISNULL(@SaldoAnterior, -99999))
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 'CAMBIADO' AS Resultado;
        RETURN;
    END

    IF ISNULL(@F, '19000101') = ISNULL(@FechaCorte, '19000101') AND ISNULL(@S, -99999) = ISNULL(@SaldoCorte, -99999)
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 'SIN CAMBIOS' AS Resultado;
        RETURN;
    END

    UPDATE dbo.Vac_Colaborador
    SET FechaCorte = @FechaCorte, SaldoCorte = @SaldoCorte,
        Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
    WHERE IdColaborador = @IdColaborador;

    INSERT INTO dbo.Vac_Bitacora
        (IdColaborador, Tipo, Accion, IdMovimiento, FechaAntes, DiasAntes, FechaDespues, DiasDespues, Motivo, Usuario_Registro, Ip)
    VALUES
        (@IdColaborador, 'CORTE',
         CASE WHEN @F IS NULL THEN 'REGISTRA' WHEN @FechaCorte IS NULL THEN 'QUITA' ELSE 'CAMBIA' END,
         NULL, @F, @S, @FechaCorte, @SaldoCorte, @Motivo, @Usuario, @Ip);

    COMMIT TRANSACTION;
    SELECT 'OK' AS Resultado;
END
GO

/* ----------------------------------------- 3. Sp_RTA_VacCorteSimular ------- */
IF OBJECT_ID('dbo.Sp_RTA_VacCorteSimular', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacCorteSimular AS RETURN 0');
GO
/* Devuelve lo mismo que Sp_RTA_VacCalcular @Salida = 'SALDOS' para una
   persona, como si tuviera ese corte. Cambia el corte, calcula y deshace:
   asi el resultado sale del mismo motor y no de una copia de sus reglas. */
ALTER PROCEDURE dbo.Sp_RTA_VacCorteSimular
    @IdColaborador  INT,
    @FechaCorte     DATE,
    @SaldoCorte     DECIMAL(9,2),
    @FechaCalculo   DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF (@FechaCorte IS NULL AND @SaldoCorte IS NOT NULL) OR (@FechaCorte IS NOT NULL AND @SaldoCorte IS NULL)
    BEGIN
        RAISERROR('La fecha y el saldo de corte van juntos.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    UPDATE dbo.Vac_Colaborador SET FechaCorte = @FechaCorte, SaldoCorte = @SaldoCorte
    WHERE IdColaborador = @IdColaborador;
    EXEC dbo.Sp_RTA_VacCalcular @FechaCalculo = @FechaCalculo, @IdColaborador = @IdColaborador, @Salida = 'SALDOS';
    ROLLBACK TRANSACTION;
END
GO

/* -------------------------------------- 4. Sp_RTA_VacCompraRegistrar ------- */
IF OBJECT_ID('dbo.Sp_RTA_VacCompraRegistrar', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacCompraRegistrar AS RETURN 0');
GO
ALTER PROCEDURE dbo.Sp_RTA_VacCompraRegistrar
    @IdColaborador  INT,
    @Fecha          DATE,
    @Dias           DECIMAL(9,2),
    @Motivo         NVARCHAR(1000),
    @Usuario        VARCHAR(50),
    @Ip             VARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Vac_Colaborador WHERE IdColaborador = @IdColaborador)
    BEGIN
        SELECT 'NO EXISTE' AS Resultado, CAST(NULL AS INT) AS IdMovimiento;
        RETURN;
    END

    DECLARE @Id INT;
    BEGIN TRANSACTION;

    /* Orden: desempate entre movimientos del mismo dia. Los del Excel usan la
       fila; los del sistema van despues de todo lo existente. */
    INSERT INTO dbo.Vac_Movimiento
        (IdColaborador, Tipo, Estado, FechaDesde, FechaHasta, Dias, Horas, Minutos, BeneficioDos, Observacion, Origen, Orden, Usuario_Registro)
    SELECT @IdColaborador, 'COMPRA DE VACACIONES', 'APROBADO', @Fecha, @Fecha, @Dias, NULL, NULL, 0, @Motivo, 'SISTEMA',
           ISNULL(MAX(Orden), 0) + 1, @Usuario
    FROM dbo.Vac_Movimiento WITH (UPDLOCK, HOLDLOCK);
    SET @Id = CONVERT(INT, SCOPE_IDENTITY());

    INSERT INTO dbo.Vac_Bitacora
        (IdColaborador, Tipo, Accion, IdMovimiento, FechaAntes, DiasAntes, FechaDespues, DiasDespues, Motivo, Usuario_Registro, Ip)
    VALUES
        (@IdColaborador, 'COMPRA', 'REGISTRA', @Id, NULL, NULL, @Fecha, @Dias, @Motivo, @Usuario, @Ip);

    COMMIT TRANSACTION;
    SELECT 'OK' AS Resultado, @Id AS IdMovimiento;
END
GO

/* ----------------------------------------- 5. Sp_RTA_VacCompraAnular ------- */
IF OBJECT_ID('dbo.Sp_RTA_VacCompraAnular', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacCompraAnular AS RETURN 0');
GO
ALTER PROCEDURE dbo.Sp_RTA_VacCompraAnular
    @IdColaborador  INT,
    @IdMovimiento   INT,
    @Motivo         NVARCHAR(1000),
    @Usuario        VARCHAR(50),
    @Ip             VARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Tipo VARCHAR(25), @Estado VARCHAR(15), @Fecha DATE, @Dias DECIMAL(9,2);
    BEGIN TRANSACTION;

    SELECT @Tipo = Tipo, @Estado = Estado, @Fecha = FechaDesde, @Dias = Dias
    FROM dbo.Vac_Movimiento WITH (UPDLOCK, HOLDLOCK)
    WHERE IdMovimiento = @IdMovimiento AND IdColaborador = @IdColaborador;

    IF @Tipo IS NULL                         BEGIN ROLLBACK TRANSACTION; SELECT 'NO EXISTE' AS Resultado;    RETURN; END
    IF @Tipo <> 'COMPRA DE VACACIONES'       BEGIN ROLLBACK TRANSACTION; SELECT 'NO ES COMPRA' AS Resultado; RETURN; END
    IF @Estado = 'DADA BAJA'                 BEGIN ROLLBACK TRANSACTION; SELECT 'YA ANULADA' AS Resultado;   RETURN; END

    UPDATE dbo.Vac_Movimiento
    SET Estado = 'DADA BAJA', Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
    WHERE IdMovimiento = @IdMovimiento;

    INSERT INTO dbo.Vac_Bitacora
        (IdColaborador, Tipo, Accion, IdMovimiento, FechaAntes, DiasAntes, FechaDespues, DiasDespues, Motivo, Usuario_Registro, Ip)
    VALUES
        (@IdColaborador, 'COMPRA', 'ANULA', @IdMovimiento, @Fecha, @Dias, NULL, NULL, @Motivo, @Usuario, @Ip);

    COMMIT TRANSACTION;
    SELECT 'OK' AS Resultado;
END
GO

/* -------------------------------------- 6. Sp_RTA_VacAjustesHistorial ------- */
IF OBJECT_ID('dbo.Sp_RTA_VacAjustesHistorial', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacAjustesHistorial AS RETURN 0');
GO
/* Dos resultados: la bitacora (lo ultimo primero) y los folios emitidos, que
   quedan desactualizados si cambia el corte o una compra. */
ALTER PROCEDURE dbo.Sp_RTA_VacAjustesHistorial
    @IdColaborador INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdBitacora, Tipo, Accion, IdMovimiento, FechaAntes, DiasAntes, FechaDespues, DiasDespues,
           Motivo, Usuario_Registro, Fecha_Registro
    FROM dbo.Vac_Bitacora
    WHERE IdColaborador = @IdColaborador
    ORDER BY Fecha_Registro DESC, IdBitacora DESC;

    SELECT COUNT(*) AS Emitidos, MAX(Fecha_Registro) AS UltimoEmitido
    FROM dbo.Vac_EstadoEmitido
    WHERE IdColaborador = @IdColaborador;
END
GO

PRINT 'Vac_Bitacora y procedimientos de ajustes listos.';
GO
