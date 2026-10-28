/* ============================================================================
   Vacaciones - motor de saldos: esquema
   ReporTarea  |  2026-10-28
   Plan: docs/superpowers/plans/2026-10-28-vacaciones-motor-de-saldos.md (F1)
   Crea:
     1. Vac_Parametro        los parametros de la hoja PARAMETROS del Excel de GTH
     2. Vac_Colaborador      el maestro (hoja COLABORADORES): ingreso, salida, corte
     3. Vac_Movimiento       lo que descuenta (hoja REGISTRO)
     4. Vac_FeriadoHistorico los feriados de 2005 a 2025, que dbo.Feriado no tiene
   El motor va en 2026-10-28-vacaciones-motor.sql y la carga la arma
   generar-carga-vacaciones.py.
   Repetible: correrlo dos veces no duplica nada. No toca tablas existentes.
   Sin claves foraneas, como el resto de la base.
   Sin tildes a proposito: sqlcmd lee el archivo con la pagina de codigos del
   equipo. Por eso los estados se guardan sin tilde (HISTORICO).

   Por que una tabla aparte de feriados: dbo.Feriado la usan tambien horas
   extras (FN_RTA_ClasificarTramosHorario). Cargarle 2005-2025 podria cambiar
   como se clasifican horas ya pagadas. El motor une las dos; desde 2026 manda
   dbo.Feriado.

   El sueldo no se guarda aqui: la nomina y la liquidacion lo tomaran de
   HE_Salario, que ya tiene el acceso restringido.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------------- 1. Vac_Parametro ------- */
IF OBJECT_ID('dbo.Vac_Parametro', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vac_Parametro
    (
        Clave                VARCHAR(20)   NOT NULL CONSTRAINT PK_Vac_Parametro PRIMARY KEY,
        Valor                DECIMAL(9,2)  NOT NULL,
        Descripcion          NVARCHAR(300) NOT NULL,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_Vac_Parametro_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    PRINT 'Vac_Parametro creada.';
END
GO

/* Valores del Excel de GTH al 2026-10-28. Los SI/NO van como 1/0.
   Solo inserta las claves que faltan: no pisa lo que GTH haya cambiado. */
INSERT INTO dbo.Vac_Parametro (Clave, Valor, Descripcion, Usuario_Registro)
SELECT v.Clave, v.Valor, v.Descripcion, 'SCRIPT_VAC'
FROM (VALUES
    ('DIAS_BASE',    15, N'Dias de vacaciones por anio cumplido (Art. 69 CT)'),
    ('ANIOS_ADIC',    5, N'Anios a partir de los cuales se suma un dia por anio'),
    ('TOPE_ADIC',    15, N'Tope de dias adicionales (maximo 30 por anio)'),
    ('PER_ACUM',      3, N'Periodos acumulables: el periodo n vence en el aniversario n+3 (Art. 75 CT)'),
    ('ALERTA',       90, N'Dias antes del vencimiento en que el periodo sale POR VENCER'),
    ('DIAS_MES',     30, N'Dias del mes para el valor diario (sueldo / 30)'),
    ('HORAS_DIA',     8, N'Horas por jornada para convertir permisos por horas en dias'),
    ('SUELTOS_CADA',  5, N'Dias sueltos entre semana, dentro del mismo anio de servicio, que cargan un fin de semana'),
    ('DIAS_FINDE',    2, N'Dias que se cargan por cada fin de semana'),
    ('MAX_FINDES',    2, N'Maximo de fines de semana cargados por periodo'),
    ('FINDE_HORAS',   0, N'1 = los permisos por horas cuentan como dias sueltos'),
    ('FUTURAS',       1, N'1 = las solicitudes con fecha posterior a la de calculo ya descuentan')
) AS v (Clave, Valor, Descripcion)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Vac_Parametro p WHERE p.Clave = v.Clave);
GO

/* ----------------------------------------------- 2. Vac_Colaborador ------- */
IF OBJECT_ID('dbo.Vac_Colaborador', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vac_Colaborador
    (
        IdColaborador        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vac_Colaborador PRIMARY KEY,
        Cedula               VARCHAR(13)   NOT NULL CONSTRAINT UQ_Vac_Colaborador_Cedula UNIQUE,
        Nombre               NVARCHAR(200) NOT NULL,
        Empresa              VARCHAR(10)   NULL,
        Area                 NVARCHAR(60)  NULL,
        Cargo                NVARCHAR(150) NULL,
        FechaIngreso         DATE          NOT NULL,           -- la vigente: si reingreso, la del reingreso
        Estado               VARCHAR(10)   NOT NULL
                             CONSTRAINT CK_Vac_Colaborador_Estado CHECK (Estado IN ('ACTIVO', 'INACTIVO')),
        FechaSalida          DATE          NULL,               -- si tiene, el saldo se calcula a esa fecha
        FechaCorte           DATE          NULL,               -- saldo real conciliado y firmado
        SaldoCorte           DECIMAL(9,2)  NULL,
        Observacion          NVARCHAR(1000) NULL,
        FilaExcel            INT           NULL,               -- N de la hoja COLABORADORES, para la prueba de oro
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_Vac_Colaborador_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL,
        CONSTRAINT CK_Vac_Colaborador_Corte CHECK ((FechaCorte IS NULL AND SaldoCorte IS NULL)
                                                OR (FechaCorte IS NOT NULL AND SaldoCorte IS NOT NULL))
    );
    PRINT 'Vac_Colaborador creada.';
END
GO

/* ------------------------------------------------ 3. Vac_Movimiento ------- */
IF OBJECT_ID('dbo.Vac_Movimiento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vac_Movimiento
    (
        IdMovimiento         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vac_Movimiento PRIMARY KEY,
        IdColaborador        INT           NOT NULL,
        Tipo                 VARCHAR(25)   NOT NULL
                             CONSTRAINT CK_Vac_Movimiento_Tipo CHECK (Tipo IN ('VACACIONES', 'PERMISO POR HORAS', 'COMPRA DE VACACIONES')),
        Estado               VARCHAR(15)   NOT NULL
                             CONSTRAINT CK_Vac_Movimiento_Estado CHECK (Estado IN ('POR APROBAR', 'APROBADO', 'PROCESADO', 'HISTORICO', 'REVISAR', 'DADA BAJA')),
        FechaDesde           DATE          NULL,               -- NULL = sin fecha: no descuenta
        FechaHasta           DATE          NULL,
        Dias                 DECIMAL(9,2)  NULL,               -- vacaciones y compra
        Horas                DECIMAL(9,2)  NULL,               -- permisos por horas
        Minutos              DECIMAL(9,2)  NULL,
        BeneficioDos         BIT           NOT NULL CONSTRAINT DF_Vac_Movimiento_Beneficio DEFAULT 0,  -- sin cargo de fin de semana
        Observacion          NVARCHAR(1000) NULL,
        Origen               VARCHAR(10)   NOT NULL
                             CONSTRAINT CK_Vac_Movimiento_Origen CHECK (Origen IN ('EXCEL', 'SISTEMA')),
        Orden                INT           NOT NULL,           -- desempate entre solicitudes del mismo dia (fila del Excel)
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_Vac_Movimiento_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    CREATE INDEX IX_Vac_Movimiento_Colaborador ON dbo.Vac_Movimiento (IdColaborador, FechaDesde, Orden);
    PRINT 'Vac_Movimiento creada.';
END
GO

/* ------------------------------------------ 4. Vac_FeriadoHistorico ------- */
IF OBJECT_ID('dbo.Vac_FeriadoHistorico', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vac_FeriadoHistorico
    (
        Fecha                DATE          NOT NULL CONSTRAINT PK_Vac_FeriadoHistorico PRIMARY KEY,
        Descripcion          NVARCHAR(200) NOT NULL,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_Vac_FeriadoHistorico_Fecha DEFAULT GETDATE(),
        CONSTRAINT CK_Vac_FeriadoHistorico_Anio CHECK (Fecha < '20260101')   -- desde 2026 manda dbo.Feriado
    );
    PRINT 'Vac_FeriadoHistorico creada.';
END
GO
