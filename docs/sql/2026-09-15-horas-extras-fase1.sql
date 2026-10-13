/* ============================================================================
   Modulo de Horas Extras - FASE 1: estructura y parametros

   Crea las seis tablas y carga los 7 parametros de calculo.

   NO CARGA PERSONAS. Los 64 colaboradores, sus 69 sueldos y su elegibilidad
   viajan en un script aparte que se genera desde la plantilla Excel y que NO
   se versiona: este repositorio es publico y esos datos son nombres, cedulas
   y sueldos de gente real.

   Las tablas cuelgan de Empleados por IdEmpleado y no por cedula. La cedula
   se usa una sola vez, en la carga, y nunca mas para identificar a nadie: hay
   una que en R_Usuarios comparten seis usuarios activos distintos.

   Idempotente: se puede correr dos veces sin dano.

   Estado CHAR(1) aparece en varias tablas de este script (HE_ColaboradorParametro,
   HE_Salario) pero NINGUN codigo de la fase 1 lo lee ni lo filtra: ni el
   generador de la carga, ni el verificador, ni NegHorasExtras. Existe para
   un borrado logico que todavia no tiene dueno. El DAO de la fase 2 tiene
   OBLIGACION de filtrar Estado = '1' en cada SELECT que toque estas tablas;
   si no lo hace, una fila dada de baja logica sigue participando del
   calculo como si estuviera activa.
   ============================================================================ */

SET NOCOUNT ON;
GO
/* Explicitos y dentro del script, no como parametro de sqlcmd: DESPLIEGUE.md
   dice que esto lo corre una persona a mano y sqlcmd los deja apagados por
   omision. En el modulo de perfil eso aborto un script a media ejecucion. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------- 1. HE_Parametro --------------- */

IF OBJECT_ID('dbo.HE_Parametro','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_Parametro
    (
        IdParametro        INT IDENTITY(1,1) NOT NULL,
        Clave              VARCHAR(60)   NOT NULL,
        Valor              DECIMAL(18,6) NOT NULL,
        FechaVigenciaDesde DATE          NOT NULL,
        FechaVigenciaHasta DATE          NULL,

        Fec_Modificacion   DATETIME2(0)  NOT NULL
            CONSTRAINT DF_HeParametro_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion   VARCHAR(50)   NULL,
        Ip_Modificacion    VARCHAR(64)   NULL,

        CONSTRAINT PK_HE_Parametro PRIMARY KEY (IdParametro),

        /* NegHorasExtras redondea con Math.Round(v, p.DecimalesMonto, ...),
           pero HE_Detalle.Total50, Total100 y TotalHE son DECIMAL(18,2) FIJOS.
           Si este parametro pidiera mas de 2 decimales -exactamente lo que
           el permiso de administrar parametros va a exponer- C# calcularia
           75.0375 y la columna guardaria 75.04 SIN ERROR, rompiendo en
           silencio el invariante que la fase escribio una prueba para
           proteger. El limite va aqui, no en las columnas: en nomina el
           dinero va a dos decimales: lo que sobra es la configurabilidad,
           no la precision. */
        CONSTRAINT CK_HE_Parametro_DecimalesMonto
            CHECK (Clave <> 'DecimalesMonto' OR Valor <= 2)
    );
    CREATE UNIQUE INDEX UX_HE_Parametro_Clave ON dbo.HE_Parametro (Clave, FechaVigenciaDesde DESC);
    PRINT 'HE_Parametro creada.';
END
ELSE PRINT 'HE_Parametro ya existia.';
GO

/* --------------------------------- 2. HE_ColaboradorParametro -------------- */

/* Lo que Empleados no tiene y este modulo necesita. Tabla satelite y no
   columnas nuevas en Empleados, que la comparte RRHHEmpleados.aspx: es el
   mismo criterio que uso el modulo de perfil.

   Empresa se guarda aqui y no se toma de Empleados.Sociedad a proposito: son
   dos clasificaciones distintas. La plantilla usa cuatro valores -separa
   "servicios profesionales"- y Sociedad tiene tres que no los distinguen. La
   que importa para pagar es la de la plantilla. */
IF OBJECT_ID('dbo.HE_ColaboradorParametro','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_ColaboradorParametro
    (
        IdColaboradorParametro INT IDENTITY(1,1) NOT NULL,
        IdEmpleado             BIGINT       NOT NULL,
        JornadaHorasDia        INT          NOT NULL,
        DivisorManual          INT          NULL,
        AplicaHE               BIT          NOT NULL,
        MotivoNoAplica         VARCHAR(40)  NULL,
        Empresa                VARCHAR(120) NULL,

        Estado                 CHAR(1)      NOT NULL
            CONSTRAINT DF_HeColabPar_Estado DEFAULT ('1'),
        Fec_Modificacion       DATETIME2(0) NOT NULL
            CONSTRAINT DF_HeColabPar_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion       VARCHAR(50)  NULL,
        Ip_Modificacion        VARCHAR(64)  NULL,

        CONSTRAINT PK_HE_ColaboradorParametro PRIMARY KEY (IdColaboradorParametro)
    );
    CREATE UNIQUE INDEX UX_HE_ColaboradorParametro_Empleado
        ON dbo.HE_ColaboradorParametro (IdEmpleado);
    PRINT 'HE_ColaboradorParametro creada.';
END
ELSE PRINT 'HE_ColaboradorParametro ya existia.';
GO

/* --------------------------------------------- 3. HE_Salario --------------- */

/* El primer lugar del sistema donde vive un sueldo. Hoy no hay ninguno en
   ninguna tabla de esta base. */
IF OBJECT_ID('dbo.HE_Salario','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_Salario
    (
        IdSalario          INT IDENTITY(1,1) NOT NULL,
        IdEmpleado         BIGINT        NOT NULL,
        Monto              DECIMAL(18,2) NOT NULL,
        FechaVigenciaDesde DATE          NOT NULL,
        Origen             VARCHAR(20)   NOT NULL,   -- Rol | Ajuste
        Observacion        VARCHAR(400)  NULL,

        Estado             CHAR(1)       NOT NULL
            CONSTRAINT DF_HeSalario_Estado DEFAULT ('1'),
        Fec_Modificacion   DATETIME2(0)  NOT NULL
            CONSTRAINT DF_HeSalario_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion   VARCHAR(50)   NULL,
        Ip_Modificacion    VARCHAR(64)   NULL,

        CONSTRAINT PK_HE_Salario PRIMARY KEY (IdSalario),

        /* Origen ya no es decorativo: NegHorasExtras.SalarioVigente desempata
           por esta columna cuando dos filas comparten fecha de vigencia
           (Ajuste gana a Rol, documento funcional 2.2). El comentario de
           arriba dice cuales son los dos valores permitidos; este CHECK hace
           que sea imposible cargar un tercero. Si RRHH escribe "Ajuste
           salarial" en una celda, la carga generada fallaria aqui en vez de
           tratarse como Rol en silencio. */
        CONSTRAINT CK_HE_Salario_Origen CHECK (Origen IN ('Rol','Ajuste'))
    );
    CREATE INDEX IX_HE_Salario_Vigencia
        ON dbo.HE_Salario (IdEmpleado, FechaVigenciaDesde DESC);
    PRINT 'HE_Salario creada.';
END
ELSE PRINT 'HE_Salario ya existia.';
GO

/* --------------------------------------------- 4. HE_Periodo --------------- */

IF OBJECT_ID('dbo.HE_Periodo','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_Periodo
    (
        IdPeriodo        INT IDENTITY(1,1) NOT NULL,
        Anio             INT          NOT NULL,
        Mes              INT          NOT NULL,
        Descripcion      VARCHAR(120) NULL,
        EstadoPeriodo    VARCHAR(10)  NOT NULL,   -- Abierto | Cerrado | Anulado

        FechaCreacion    DATETIME2(0) NOT NULL
            CONSTRAINT DF_HePeriodo_FecCrea DEFAULT (SYSDATETIME()),
        UsuarioCreacion  VARCHAR(50)  NULL,
        FechaCierre      DATETIME2(0) NULL,
        UsuarioCierre    VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_HE_Periodo PRIMARY KEY (IdPeriodo)
    );
    CREATE UNIQUE INDEX UX_HE_Periodo_AnioMes ON dbo.HE_Periodo (Anio, Mes);
    PRINT 'HE_Periodo creada.';
END
ELSE PRINT 'HE_Periodo ya existia.';
GO

/* --------------------------------------------- 5. HE_Detalle --------------- */

/* Una fila por periodo y colaborador, con el SNAPSHOT congelado.

   El snapshot es lo que impide que un periodo cerrado cambie porque alguien
   edito el maestro despues. En el Excel la cedula estaba escrita a mano
   mientras el resto venia por formula: reordenar la hoja desalineaba los datos
   sin aviso. Aqui el detalle guarda su propia copia de lo que uso para
   calcular, y la relacion es por IdEmpleado. */
IF OBJECT_ID('dbo.HE_Detalle','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_Detalle
    (
        IdDetalle              INT IDENTITY(1,1) NOT NULL,
        IdPeriodo              INT           NOT NULL,
        IdEmpleado             BIGINT        NOT NULL,

        /* snapshot */
        CedulaSnapshot         VARCHAR(20)   NULL,
        NombreSnapshot         VARCHAR(400)  NULL,
        EmpresaSnapshot        VARCHAR(120)  NULL,
        CargoSnapshot          VARCHAR(200)  NULL,
        JornadaHorasDiaSnapshot INT          NOT NULL,
        SalarioBaseSnapshot    DECIMAL(18,2) NOT NULL,
        AplicaHESnapshot       BIT           NOT NULL,

        /* calculo */
        Divisor                INT           NOT NULL,
        ValorHoraOrdinaria     DECIMAL(18,6) NOT NULL,
        ValorHora50            DECIMAL(18,6) NOT NULL,
        ValorHora100           DECIMAL(18,6) NOT NULL,
        Horas50                DECIMAL(9,2)  NOT NULL CONSTRAINT DF_HeDetalle_H50  DEFAULT (0),
        Horas100               DECIMAL(9,2)  NOT NULL CONSTRAINT DF_HeDetalle_H100 DEFAULT (0),
        Total50                DECIMAL(18,2) NOT NULL CONSTRAINT DF_HeDetalle_T50  DEFAULT (0),
        Total100               DECIMAL(18,2) NOT NULL CONSTRAINT DF_HeDetalle_T100 DEFAULT (0),
        TotalHoras             DECIMAL(9,2)  NOT NULL CONSTRAINT DF_HeDetalle_TH   DEFAULT (0),
        TotalHE                DECIMAL(18,2) NOT NULL CONSTRAINT DF_HeDetalle_THE  DEFAULT (0),

        Observacion            VARCHAR(400)  NULL,
        Fec_Modificacion       DATETIME2(0)  NOT NULL
            CONSTRAINT DF_HeDetalle_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion       VARCHAR(50)   NULL,
        Ip_Modificacion        VARCHAR(64)   NULL,

        CONSTRAINT PK_HE_Detalle PRIMARY KEY (IdDetalle)
    );
    CREATE UNIQUE INDEX UX_HE_Detalle_PeriodoEmpleado
        ON dbo.HE_Detalle (IdPeriodo, IdEmpleado);
    CREATE INDEX IX_HE_Detalle_Periodo ON dbo.HE_Detalle (IdPeriodo);
    PRINT 'HE_Detalle creada.';
END
ELSE PRINT 'HE_Detalle ya existia.';
GO

/* ------------------------------------ 6. HE_DetalleAuditoria --------------- */

/* Solo sobre las dos columnas editables. Auditar las derivadas seria auditar
   una formula: se recalculan solas y su rastro es el de las horas. */
IF OBJECT_ID('dbo.HE_DetalleAuditoria','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_DetalleAuditoria
    (
        IdAuditoria      INT IDENTITY(1,1) NOT NULL,
        IdDetalle        INT           NOT NULL,
        Campo            VARCHAR(20)   NOT NULL,   -- Horas50 | Horas100 | Observacion
        ValorAnterior    DECIMAL(9,2)  NULL,
        ValorNuevo       DECIMAL(9,2)  NULL,
        Fecha            DATETIME2(0)  NOT NULL
            CONSTRAINT DF_HeDetAud_Fec DEFAULT (SYSDATETIME()),
        Usuario          VARCHAR(50)   NULL,
        Ip               VARCHAR(64)   NULL,

        CONSTRAINT PK_HE_DetalleAuditoria PRIMARY KEY (IdAuditoria)
    );
    CREATE INDEX IX_HE_DetalleAuditoria_Detalle ON dbo.HE_DetalleAuditoria (IdDetalle);
    PRINT 'HE_DetalleAuditoria creada.';
END
ELSE PRINT 'HE_DetalleAuditoria ya existia.';
GO

/* ------------------------------------- 7. parametros iniciales ------------- */

/* Los 7 valores del documento funcional, con vigencia desde el arranque del
   modulo. No son datos personales: son configuracion.

   Se insertan solo si no existe ya una vigencia para esa clave, para que
   correr el script dos veces no duplique ni pise un valor que alguien ajusto. */
-- Misma fecha que VIGENCIA_ROL en docs/sql/generar-carga-horas-extras.py.
-- No se unifican -son lenguajes distintos-, pero si cambia una hay que
-- revisar la otra: son la vigencia inicial de dos cosas relacionadas
-- (los parametros de calculo aqui, los sueldos de rol alla).
DECLARE @Desde DATE = '2026-09-01';

INSERT INTO dbo.HE_Parametro (Clave, Valor, FechaVigenciaDesde, Usu_Modificacion)
SELECT v.Clave, v.Valor, @Desde, 'carga-inicial'
  FROM (VALUES
        ('DiasMes',                  30),
        ('HorasMesJornadaCompleta', 240),
        ('Factor50',                1.5),
        ('Factor100',                 2),
        ('TopeDiario50',              4),
        ('TopeSemanal50',            12),
        ('DecimalesMonto',            2)
       ) v (Clave, Valor)
 WHERE NOT EXISTS (SELECT 1 FROM dbo.HE_Parametro p
                    WHERE p.Clave = v.Clave AND p.FechaVigenciaDesde = @Desde);

PRINT 'Parametros iniciales revisados.';
GO

/* ------------------------------------------------- 8. aserciones ----------- */

DECLARE @Fallos INT = 0;

IF OBJECT_ID('dbo.HE_Parametro','U') IS NULL
BEGIN
    RAISERROR('FALLO: HE_Parametro no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.HE_ColaboradorParametro','U') IS NULL
BEGIN
    RAISERROR('FALLO: HE_ColaboradorParametro no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.HE_Salario','U') IS NULL
BEGIN
    RAISERROR('FALLO: HE_Salario no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.HE_Periodo','U') IS NULL
BEGIN
    RAISERROR('FALLO: HE_Periodo no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.HE_Detalle','U') IS NULL
BEGIN
    RAISERROR('FALLO: HE_Detalle no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.HE_DetalleAuditoria','U') IS NULL
BEGIN
    RAISERROR('FALLO: HE_DetalleAuditoria no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF (SELECT COUNT(*) FROM dbo.HE_Parametro WHERE FechaVigenciaDesde = '2026-09-01') <> 7
BEGIN
    RAISERROR('FALLO: no quedaron los 7 parametros con la vigencia inicial.', 16, 1);
    SET @Fallos += 1;
END

/* Un colaborador no puede tener dos filas de parametros: el calculo tomaria
   una al azar. El indice unico lo impide, y esta asercion comprueba que el
   indice existe y no que alguien lo creo sin UNIQUE. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.HE_ColaboradorParametro')
                  AND name = 'UX_HE_ColaboradorParametro_Empleado' AND is_unique = 1)
BEGIN
    RAISERROR('FALLO: falta el indice UNICO por empleado en HE_ColaboradorParametro.', 16, 1);
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.HE_Detalle')
                  AND name = 'UX_HE_Detalle_PeriodoEmpleado' AND is_unique = 1)
BEGIN
    RAISERROR('FALLO: falta el indice UNICO por periodo y empleado en HE_Detalle.', 16, 1);
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.HE_Parametro')
                  AND name = 'UX_HE_Parametro_Clave' AND is_unique = 1)
BEGIN
    RAISERROR('FALLO: falta el indice UNICO por clave y fecha vigencia en HE_Parametro.', 16, 1);
    SET @Fallos += 1;
END

/* Este script no puede haber cargado personas. Si lo hizo, algo se colo. */
IF (SELECT COUNT(*) FROM dbo.HE_ColaboradorParametro) > 0
   AND NOT EXISTS (SELECT 1 FROM dbo.HE_ColaboradorParametro WHERE Usu_Modificacion <> 'carga-inicial')
    PRINT 'AVISO: HE_ColaboradorParametro ya tiene filas. Vienen de la carga generada, no de este script.';

IF @Fallos > 0
    RAISERROR('FALLO: %d verificaciones no pasaron. La estructura NO quedo lista.', 16, 1, @Fallos);
ELSE
    PRINT 'Horas Extras fase 1: estructura y parametros listos.';
GO
