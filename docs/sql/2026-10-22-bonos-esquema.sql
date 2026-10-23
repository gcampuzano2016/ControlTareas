/* ============================================================================
   Bonos de empleados - esquema
   ReporTarea  |  2026-10-22
   Spec: docs/superpowers/specs/2026-10-20-bonos-design.md (1 Datos, 2 Reglas)
   Crea o actualiza:
     1. Tablas Bono_Tipo, Bono_TipoConcepto, Bono_Periodo, Bono_Detalle,
        Bono_DetalleSemana, Bono_Bitacora
     2. Procedimientos Sp_RTA_Bono*
   La migracion, el menu y la verificacion van en sus propios scripts.
   Repetible: correrlo dos veces no duplica nada. No toca tablas existentes.
   Sin claves foraneas, como el resto de la base.
   Sin tildes a proposito: sqlcmd lee el archivo con la pagina de codigos del
   equipo. Los textos con tilde se arman con NCHAR.

   Codigos que devuelven los procedimientos que escriben (columna Respuestas):
     -1  el bono (o el periodo) cambio de estado mientras tanto / ya existe
     -2  el periodo no esta abierto (o falta un concepto SAP, al cerrar)
     -3  doble pago: otro bono no rechazado de la misma persona y tipo ya
         tiene alguno de esos lunes
   Un rechazo por regla no escribe nada: la transaccion se confirma vacia
   (COMMIT, no ROLLBACK) para no romper una transaccion externa ni un
   INSERT ... EXEC.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------------------- 1. tablas -------- */
IF OBJECT_ID('dbo.Bono_Tipo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bono_Tipo
    (
        IdTipo               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bono_Tipo PRIMARY KEY,
        Nombre               NVARCHAR(150) NOT NULL CONSTRAINT UQ_Bono_Tipo_Nombre UNIQUE,
        Familia              VARCHAR(20)   NOT NULL
                             CONSTRAINT CK_Bono_Tipo_Familia CHECK (Familia IN ('Standby', 'Contrato', 'Otro')),
        ValorSugerido        DECIMAL(9,2)  NULL,
        PorSemana            BIT           NOT NULL CONSTRAINT DF_Bono_Tipo_PorSemana DEFAULT 0,
        Activo               BIT           NOT NULL CONSTRAINT DF_Bono_Tipo_Activo DEFAULT 1,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_Bono_Tipo_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    PRINT 'Bono_Tipo creada.';
END
GO

IF OBJECT_ID('dbo.Bono_TipoConcepto', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bono_TipoConcepto
    (
        IdTipo               INT          NOT NULL,
        Empresa              VARCHAR(10)  NOT NULL
                             CONSTRAINT CK_Bono_TipoConcepto_Empresa CHECK (Empresa IN ('DOS', 'AGILITY')),
        CodigoConcepto       VARCHAR(10)  NOT NULL,
        Usuario_Registro     VARCHAR(50)  NOT NULL,
        Fecha_Registro       DATETIME     NOT NULL CONSTRAINT DF_Bono_TipoConcepto_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)  NULL,
        Fecha_Modificacion   DATETIME     NULL,
        CONSTRAINT PK_Bono_TipoConcepto PRIMARY KEY (IdTipo, Empresa)
    );
    PRINT 'Bono_TipoConcepto creada.';
END
GO

IF OBJECT_ID('dbo.Bono_Periodo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bono_Periodo
    (
        IdPeriodo            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bono_Periodo PRIMARY KEY,
        Anio                 INT           NOT NULL,
        Mes                  INT           NOT NULL CONSTRAINT CK_Bono_Periodo_Mes CHECK (Mes BETWEEN 1 AND 12),
        Estado               VARCHAR(10)   NOT NULL
                             CONSTRAINT CK_Bono_Periodo_Estado CHECK (Estado IN ('Abierto', 'Cerrado', 'Historico')),
        FechaApertura        DATETIME      NULL,
        UsuarioApertura      VARCHAR(50)   NULL,
        FechaCierre          DATETIME      NULL,
        UsuarioCierre        VARCHAR(50)   NULL,
        TotalHistorico       DECIMAL(11,2) NULL,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_Bono_Periodo_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL,
        CONSTRAINT UQ_Bono_Periodo_AnioMes UNIQUE (Anio, Mes)
    );
    PRINT 'Bono_Periodo creada.';
END
GO

IF OBJECT_ID('dbo.Bono_Detalle', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bono_Detalle
    (
        IdBono               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bono_Detalle PRIMARY KEY,
        IdPeriodo            INT           NULL,          -- NULL = pendiente sin periodo
        CodUsuario           VARCHAR(50)   NOT NULL,
        Empresa              VARCHAR(10)   NULL,          -- copia al enviar / registrar
        Nombre               NVARCHAR(150) NULL,          -- copia al enviar / registrar
        Identificacion       VARCHAR(32)   NULL,          -- copia al enviar / registrar
        Lugar                NVARCHAR(100) NULL,
        IdTipo               INT           NOT NULL,
        Semanas              INT           NOT NULL CONSTRAINT DF_Bono_Detalle_Semanas DEFAULT 0,
        ValorSugerido        DECIMAL(9,2)  NULL,
        Valor                DECIMAL(9,2)  NOT NULL CONSTRAINT CK_Bono_Detalle_Valor CHECK (Valor > 0),
        MotivoAjuste         NVARCHAR(500) NULL,
        Observacion          NVARCHAR(500) NULL,
        Estado               VARCHAR(10)   NOT NULL
                             CONSTRAINT CK_Bono_Detalle_Estado CHECK (Estado IN ('Borrador', 'Enviado', 'Aprobado', 'Rechazado')),
        Origen               VARCHAR(15)   NOT NULL
                             CONSTRAINT CK_Bono_Detalle_Origen CHECK (Origen IN ('Jefe', 'TalentoHumano', 'Migracion')),
        CodJefe              VARCHAR(50)   NULL,
        UsuarioRevisa        VARCHAR(50)   NULL,
        FechaRevisa          DATETIME      NULL,
        MotivoRechazo        NVARCHAR(500) NULL,
        NombreTipo           NVARCHAR(150) NULL,          -- copia al cerrar
        CodigoConcepto       VARCHAR(10)   NULL,          -- copia al cerrar
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_Bono_Detalle_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    CREATE INDEX IX_Bono_Detalle_Periodo ON dbo.Bono_Detalle (IdPeriodo) INCLUDE (Estado);
    CREATE INDEX IX_Bono_Detalle_PersonaTipo ON dbo.Bono_Detalle (CodUsuario, IdTipo) INCLUDE (Estado);
    PRINT 'Bono_Detalle creada.';
END
GO

IF OBJECT_ID('dbo.Bono_DetalleSemana', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bono_DetalleSemana
    (
        IdBono           INT         NOT NULL,
        Lunes            DATE        NOT NULL
                         CONSTRAINT CK_Bono_DetalleSemana_Lunes CHECK (DATEDIFF(DAY, '19000101', Lunes) % 7 = 0),
        Usuario_Registro VARCHAR(50) NOT NULL,
        Fecha_Registro   DATETIME    NOT NULL CONSTRAINT DF_Bono_DetalleSemana_Fecha DEFAULT GETDATE(),
        CONSTRAINT PK_Bono_DetalleSemana PRIMARY KEY (IdBono, Lunes)
    );
    CREATE INDEX IX_Bono_DetalleSemana_Lunes ON dbo.Bono_DetalleSemana (Lunes, IdBono);
    PRINT 'Bono_DetalleSemana creada.';
END
GO

IF OBJECT_ID('dbo.Bono_Bitacora', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bono_Bitacora
    (
        IdBitacora    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bono_Bitacora PRIMARY KEY,
        IdBono        INT            NULL,
        IdPeriodo     INT            NULL,
        Accion        VARCHAR(20)    NOT NULL,
        ValorAnterior NVARCHAR(200)  NULL,
        ValorNuevo    NVARCHAR(200)  NULL,
        Detalle       NVARCHAR(2000) NULL,
        Usuario       VARCHAR(50)    NOT NULL,
        Fecha         DATETIME       NOT NULL CONSTRAINT DF_Bono_Bitacora_Fecha DEFAULT GETDATE()
    );
    CREATE INDEX IX_Bono_Bitacora_Bono ON dbo.Bono_Bitacora (IdBono);
    CREATE INDEX IX_Bono_Bitacora_Periodo ON dbo.Bono_Bitacora (IdPeriodo);
    PRINT 'Bono_Bitacora creada.';
END
GO

/* ------------------------------------------------- 2. procedimientos ------ */
IF OBJECT_ID('dbo.Sp_RTA_BonoPersonasActivas', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoPersonasActivas AS RETURN 0;');
GO
/* El UNICO lugar que dice quien es "activo" en bonos: el mismo criterio de
   Sp_RTA_AgendaPersonasActivas (Usuario_Estado = 'A', EstadoUsuario NULL,
   solo internos). Si un Cod_Usuario esta repetido, la cuenta mas reciente.
   Empresa sale tal cual (sin normalizar): la normaliza NegBonoReglas. */
ALTER PROCEDURE dbo.Sp_RTA_BonoPersonasActivas
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH U AS
    (
        SELECT CodUsuario = LTRIM(RTRIM(u.Cod_Usuario)),
               Nombre     = LTRIM(RTRIM(ISNULL(u.Nom_Usuario, ''))),
               Correo     = LTRIM(RTRIM(ISNULL(u.E_Mail, ''))),
               CodJefe    = LTRIM(RTRIM(ISNULL(u.Cod_Jefe_Inm, ''))),
               Empresa    = LTRIM(RTRIM(ISNULL(u.Empresa, ''))),
               CodSap     = LTRIM(RTRIM(ISNULL(u.Cod_Sap, ''))),
               Cedula     = LTRIM(RTRIM(ISNULL(u.Cedula, ''))),
               IdPerfil   = CONVERT(INT, u.Id_Perfil),
               Orden      = ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(u.Cod_Usuario)) ORDER BY u.Id_Usuario DESC)
          FROM dbo.R_Usuarios AS u
         WHERE u.Usuario_Estado = 'A'
           AND u.EstadoUsuario IS NULL
           AND ISNULL(u.IdCliente, 0) = 0
           AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
    )
    SELECT CodUsuario, Nombre, Correo, CodJefe, Empresa, CodSap, Cedula, IdPerfil
      FROM U
     WHERE Orden = 1
     ORDER BY Nombre;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoTipos', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoTipos AS RETURN 0;');
GO
/* Dos resultados: los tipos (activos e inactivos) y sus conceptos SAP. */
ALTER PROCEDURE dbo.Sp_RTA_BonoTipos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdTipo, Nombre, Familia, ValorSugerido, PorSemana, Activo
      FROM dbo.Bono_Tipo
     ORDER BY Nombre;

    SELECT IdTipo, Empresa, CodigoConcepto
      FROM dbo.Bono_TipoConcepto
     ORDER BY IdTipo, Empresa;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoGuardarTipo', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoGuardarTipo AS RETURN 0;');
GO
/* @IdTipo = 0 crea. Devuelve Respuestas = IdTipo, -1 nombre repetido o
   -2 el tipo no existe. Un concepto vacio o NULL borra el de esa empresa. */
ALTER PROCEDURE dbo.Sp_RTA_BonoGuardarTipo
    @IdTipo          INT,
    @Nombre          NVARCHAR(150),
    @Familia         VARCHAR(20),
    @ValorSugerido   DECIMAL(9,2),
    @PorSemana       BIT,
    @Activo          BIT,
    @ConceptoDos     VARCHAR(10),
    @ConceptoAgility VARCHAR(10),
    @Usuario         VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @IdTipo  = ISNULL(@IdTipo, 0);
    SET @Nombre  = LTRIM(RTRIM(ISNULL(@Nombre, N'')));
    SET @Familia = LTRIM(RTRIM(ISNULL(@Familia, '')));

    DECLARE @C TABLE (Empresa VARCHAR(10) NOT NULL PRIMARY KEY, CodigoConcepto VARCHAR(10) NULL);
    INSERT INTO @C (Empresa, CodigoConcepto)
    VALUES ('DOS',     NULLIF(UPPER(LTRIM(RTRIM(ISNULL(@ConceptoDos, '')))), '')),
           ('AGILITY', NULLIF(UPPER(LTRIM(RTRIM(ISNULL(@ConceptoAgility, '')))), ''));

    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.Bono_Tipo WITH (UPDLOCK, HOLDLOCK)
                WHERE Nombre = @Nombre AND IdTipo <> @IdTipo)
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -1;
        RETURN;
    END

    DECLARE @Accion NVARCHAR(20);
    IF @IdTipo = 0
    BEGIN
        INSERT INTO dbo.Bono_Tipo (Nombre, Familia, ValorSugerido, PorSemana, Activo, Usuario_Registro)
        VALUES (@Nombre, @Familia, @ValorSugerido, ISNULL(@PorSemana, 0), ISNULL(@Activo, 1), @Usuario);
        SET @IdTipo = CONVERT(INT, SCOPE_IDENTITY());
        SET @Accion = N'Crear';
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Bono_Tipo WITH (UPDLOCK, HOLDLOCK) WHERE IdTipo = @IdTipo)
        BEGIN
            COMMIT TRANSACTION;
            SELECT Respuestas = -2;
            RETURN;
        END
        UPDATE dbo.Bono_Tipo
           SET Nombre = @Nombre, Familia = @Familia, ValorSugerido = @ValorSugerido,
               PorSemana = ISNULL(@PorSemana, 0), Activo = ISNULL(@Activo, 1),
               Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
         WHERE IdTipo = @IdTipo;
        SET @Accion = N'Editar';
    END

    DELETE tc
      FROM dbo.Bono_TipoConcepto AS tc
      JOIN @C AS c ON c.Empresa = tc.Empresa
     WHERE tc.IdTipo = @IdTipo AND c.CodigoConcepto IS NULL;

    UPDATE tc
       SET CodigoConcepto = c.CodigoConcepto,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
      FROM dbo.Bono_TipoConcepto AS tc
      JOIN @C AS c ON c.Empresa = tc.Empresa
     WHERE tc.IdTipo = @IdTipo AND c.CodigoConcepto IS NOT NULL
       AND tc.CodigoConcepto <> c.CodigoConcepto;

    INSERT INTO dbo.Bono_TipoConcepto (IdTipo, Empresa, CodigoConcepto, Usuario_Registro)
    SELECT @IdTipo, c.Empresa, c.CodigoConcepto, @Usuario
      FROM @C AS c
     WHERE c.CodigoConcepto IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Bono_TipoConcepto tc
                        WHERE tc.IdTipo = @IdTipo AND tc.Empresa = c.Empresa);

    INSERT INTO dbo.Bono_Bitacora (IdBono, IdPeriodo, Accion, ValorAnterior, ValorNuevo, Detalle, Usuario)
    VALUES (NULL, NULL, 'Catalogo', NULL, LEFT(@Nombre, 200),
            LEFT(@Accion + N' tipo ' + CONVERT(NVARCHAR(12), @IdTipo)
                 + N'; familia ' + @Familia
                 + N'; sugerido ' + ISNULL(CONVERT(NVARCHAR(20), @ValorSugerido), N'-')
                 + N'; por semana ' + CONVERT(NVARCHAR(1), ISNULL(@PorSemana, 0))
                 + N'; activo ' + CONVERT(NVARCHAR(1), ISNULL(@Activo, 1))
                 + N'; DOS ' + ISNULL((SELECT CodigoConcepto FROM @C WHERE Empresa = 'DOS'), '-')
                 + N'; AGILITY ' + ISNULL((SELECT CodigoConcepto FROM @C WHERE Empresa = 'AGILITY'), '-'), 2000),
            @Usuario);

    COMMIT TRANSACTION;
    SELECT Respuestas = @IdTipo;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoPeriodos', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoPeriodos AS RETURN 0;');
GO
/* Todos los periodos, del mas reciente al mas viejo, con la suma de lo
   Aprobado (0 si no hay; en los Historico vale TotalHistorico). */
ALTER PROCEDURE dbo.Sp_RTA_BonoPeriodos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.IdPeriodo, p.Anio, p.Mes, p.Estado, p.TotalHistorico,
           p.FechaApertura, p.FechaCierre,
           UsuarioApertura = ISNULL(p.UsuarioApertura, ''),
           UsuarioCierre   = ISNULL(p.UsuarioCierre, ''),
           TotalAprobado   = ISNULL((SELECT SUM(d.Valor) FROM dbo.Bono_Detalle d
                                      WHERE d.IdPeriodo = p.IdPeriodo AND d.Estado = 'Aprobado'), 0)
      FROM dbo.Bono_Periodo AS p
     ORDER BY p.Anio DESC, p.Mes DESC;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoAbrirPeriodo', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoAbrirPeriodo AS RETURN 0;');
GO
/* Abre el mes siguiente al ultimo existente (Anio, Mes); si no hay ninguno,
   el mes actual. Engancha todos los pendientes (IdPeriodo NULL).
   Respuestas = IdPeriodo nuevo, o -1 si ya hay uno abierto. */
ALTER PROCEDURE dbo.Sp_RTA_BonoAbrirPeriodo
    @Usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    /* El bloqueo sobre toda la tabla de periodos serializa dos aperturas. */
    DECLARE @Anio INT, @Mes INT, @Hay INT;
    SELECT @Hay = COUNT(*) FROM dbo.Bono_Periodo WITH (UPDLOCK, HOLDLOCK, TABLOCK) WHERE Estado = 'Abierto';
    IF @Hay > 0
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -1;
        RETURN;
    END

    SELECT TOP (1) @Anio = Anio, @Mes = Mes
      FROM dbo.Bono_Periodo
     ORDER BY Anio DESC, Mes DESC;

    IF @Anio IS NULL
        SELECT @Anio = YEAR(GETDATE()), @Mes = MONTH(GETDATE());
    ELSE IF @Mes = 12
        SELECT @Anio = @Anio + 1, @Mes = 1;
    ELSE
        SET @Mes = @Mes + 1;

    INSERT INTO dbo.Bono_Periodo (Anio, Mes, Estado, FechaApertura, UsuarioApertura, Usuario_Registro)
    VALUES (@Anio, @Mes, 'Abierto', GETDATE(), @Usuario, @Usuario);
    DECLARE @IdPeriodo INT = CONVERT(INT, SCOPE_IDENTITY());

    UPDATE dbo.Bono_Detalle
       SET IdPeriodo = @IdPeriodo, Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdPeriodo IS NULL;
    DECLARE @Enganchados INT = @@ROWCOUNT;

    INSERT INTO dbo.Bono_Bitacora (IdBono, IdPeriodo, Accion, ValorAnterior, ValorNuevo, Detalle, Usuario)
    VALUES (NULL, @IdPeriodo, 'Abrir', NULL, N'Abierto',
            N'Periodo ' + CONVERT(NVARCHAR(4), @Anio) + N'-' + RIGHT(N'0' + CONVERT(NVARCHAR(2), @Mes), 2)
            + N'; pendientes enganchados: ' + CONVERT(NVARCHAR(12), @Enganchados),
            @Usuario);

    COMMIT TRANSACTION;
    SELECT Respuestas = @IdPeriodo;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoCerrarPeriodo', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoCerrarPeriodo AS RETURN 0;');
GO
/* Respuestas = 0 cerrado, -1 el periodo no esta abierto, -2 algun Aprobado
   no tiene concepto SAP para su (IdTipo, Empresa). Al cerrar: los Aprobados
   reciben la copia de NombreTipo y CodigoConcepto (el Excel de un mes cerrado
   sale de esas copias); todo lo demas queda pendiente (IdPeriodo NULL)
   conservando su estado. */
ALTER PROCEDURE dbo.Sp_RTA_BonoCerrarPeriodo
    @IdPeriodo INT,
    @Usuario   VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM dbo.Bono_Periodo WITH (UPDLOCK, HOLDLOCK)
                    WHERE IdPeriodo = @IdPeriodo AND Estado = 'Abierto')
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -1;
        RETURN;
    END

    IF EXISTS (SELECT 1
                 FROM dbo.Bono_Detalle AS d WITH (UPDLOCK, HOLDLOCK)
                WHERE d.IdPeriodo = @IdPeriodo AND d.Estado = 'Aprobado'
                  AND NOT EXISTS (SELECT 1 FROM dbo.Bono_TipoConcepto AS c
                                   WHERE c.IdTipo = d.IdTipo
                                     AND c.Empresa = UPPER(LTRIM(RTRIM(ISNULL(d.Empresa, ''))))))
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE d
       SET NombreTipo = t.Nombre, CodigoConcepto = c.CodigoConcepto,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
      FROM dbo.Bono_Detalle AS d
      JOIN dbo.Bono_Tipo AS t ON t.IdTipo = d.IdTipo
      JOIN dbo.Bono_TipoConcepto AS c
        ON c.IdTipo = d.IdTipo AND c.Empresa = UPPER(LTRIM(RTRIM(ISNULL(d.Empresa, ''))))
     WHERE d.IdPeriodo = @IdPeriodo AND d.Estado = 'Aprobado';
    DECLARE @Aprobados INT = @@ROWCOUNT;

    UPDATE dbo.Bono_Detalle
       SET IdPeriodo = NULL, Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdPeriodo = @IdPeriodo AND Estado <> 'Aprobado';
    DECLARE @Fuera INT = @@ROWCOUNT;

    UPDATE dbo.Bono_Periodo
       SET Estado = 'Cerrado', FechaCierre = GETDATE(), UsuarioCierre = @Usuario,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdPeriodo = @IdPeriodo;

    INSERT INTO dbo.Bono_Bitacora (IdBono, IdPeriodo, Accion, ValorAnterior, ValorNuevo, Detalle, Usuario)
    VALUES (NULL, @IdPeriodo, 'Cerrar', N'Abierto', N'Cerrado',
            N'Aprobados: ' + CONVERT(NVARCHAR(12), @Aprobados)
            + N'; total: ' + CONVERT(NVARCHAR(20), ISNULL((SELECT SUM(Valor) FROM dbo.Bono_Detalle
                                                           WHERE IdPeriodo = @IdPeriodo AND Estado = 'Aprobado'), 0))
            + N'; pasan a pendientes: ' + CONVERT(NVARCHAR(12), @Fuera),
            @Usuario);

    COMMIT TRANSACTION;
    SELECT Respuestas = 0;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoDetalles', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoDetalles AS RETURN 0;');
GO
/* @IdPeriodo NULL = el periodo abierto (si hay) mas los pendientes sin
   periodo. Dos resultados: bonos (columnas = propiedades de EntBono) y sus
   lunes (IdBono, Lunes). NombreTipo y CodigoConcepto: la copia guardada al
   cerrar; mientras no la hay, el valor vigente del catalogo. */
ALTER PROCEDURE dbo.Sp_RTA_BonoDetalles
    @IdPeriodo INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    CREATE TABLE #Ids (IdBono INT PRIMARY KEY);

    IF @IdPeriodo IS NULL
        INSERT INTO #Ids (IdBono)
        SELECT d.IdBono
          FROM dbo.Bono_Detalle AS d
         WHERE d.IdPeriodo IS NULL
            OR d.IdPeriodo IN (SELECT p.IdPeriodo FROM dbo.Bono_Periodo p WHERE p.Estado = 'Abierto');
    ELSE
        INSERT INTO #Ids (IdBono)
        SELECT d.IdBono FROM dbo.Bono_Detalle AS d WHERE d.IdPeriodo = @IdPeriodo;

    SELECT d.IdBono, d.IdPeriodo, d.CodUsuario,
           Empresa        = ISNULL(d.Empresa, ''),
           Nombre         = ISNULL(d.Nombre, N''),
           Identificacion = ISNULL(d.Identificacion, ''),
           Lugar          = ISNULL(d.Lugar, N''),
           d.IdTipo, d.Semanas, d.ValorSugerido, d.Valor,
           MotivoAjuste   = ISNULL(d.MotivoAjuste, N''),
           Observacion    = ISNULL(d.Observacion, N''),
           d.Estado, d.Origen,
           CodJefe        = ISNULL(d.CodJefe, ''),
           MotivoRechazo  = ISNULL(d.MotivoRechazo, N''),
           NombreTipo     = ISNULL(d.NombreTipo, ISNULL(t.Nombre, N'')),
           CodigoConcepto = ISNULL(d.CodigoConcepto, ISNULL(c.CodigoConcepto, ''))
      FROM dbo.Bono_Detalle AS d
      JOIN #Ids AS i ON i.IdBono = d.IdBono
      LEFT JOIN dbo.Bono_Tipo AS t ON t.IdTipo = d.IdTipo
      LEFT JOIN dbo.Bono_TipoConcepto AS c
        ON c.IdTipo = d.IdTipo AND c.Empresa = UPPER(LTRIM(RTRIM(ISNULL(d.Empresa, ''))))
     ORDER BY d.IdBono;

    SELECT s.IdBono, s.Lunes
      FROM dbo.Bono_DetalleSemana AS s
      JOIN #Ids AS i ON i.IdBono = s.IdBono
     ORDER BY s.IdBono, s.Lunes;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoDePersona', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoDePersona AS RETURN 0;');
GO
/* Todos los bonos NO rechazados de la persona, de cualquier periodo, con sus
   lunes: para el doble pago y las semanas tachadas del calendario. Mismas
   columnas y mismos dos resultados que Sp_RTA_BonoDetalles. */
ALTER PROCEDURE dbo.Sp_RTA_BonoDePersona
    @CodUsuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET @CodUsuario = LTRIM(RTRIM(ISNULL(@CodUsuario, '')));
    CREATE TABLE #Ids (IdBono INT PRIMARY KEY);

    INSERT INTO #Ids (IdBono)
    SELECT d.IdBono
      FROM dbo.Bono_Detalle AS d
     WHERE d.CodUsuario = @CodUsuario AND d.Estado <> 'Rechazado';

    SELECT d.IdBono, d.IdPeriodo, d.CodUsuario,
           Empresa        = ISNULL(d.Empresa, ''),
           Nombre         = ISNULL(d.Nombre, N''),
           Identificacion = ISNULL(d.Identificacion, ''),
           Lugar          = ISNULL(d.Lugar, N''),
           d.IdTipo, d.Semanas, d.ValorSugerido, d.Valor,
           MotivoAjuste   = ISNULL(d.MotivoAjuste, N''),
           Observacion    = ISNULL(d.Observacion, N''),
           d.Estado, d.Origen,
           CodJefe        = ISNULL(d.CodJefe, ''),
           MotivoRechazo  = ISNULL(d.MotivoRechazo, N''),
           NombreTipo     = ISNULL(d.NombreTipo, ISNULL(t.Nombre, N'')),
           CodigoConcepto = ISNULL(d.CodigoConcepto, ISNULL(c.CodigoConcepto, ''))
      FROM dbo.Bono_Detalle AS d
      JOIN #Ids AS i ON i.IdBono = d.IdBono
      LEFT JOIN dbo.Bono_Tipo AS t ON t.IdTipo = d.IdTipo
      LEFT JOIN dbo.Bono_TipoConcepto AS c
        ON c.IdTipo = d.IdTipo AND c.Empresa = UPPER(LTRIM(RTRIM(ISNULL(d.Empresa, ''))))
     ORDER BY d.IdBono;

    SELECT s.IdBono, s.Lunes
      FROM dbo.Bono_DetalleSemana AS s
      JOIN #Ids AS i ON i.IdBono = s.IdBono
     ORDER BY s.IdBono, s.Lunes;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoGuardar', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoGuardar AS RETURN 0;');
GO
/* Crea (@IdBono = 0) o edita un bono y reemplaza sus lunes.
   @Lunes: 'yyyy-MM-dd' separados por coma ('' o NULL = ninguno); una fecha
   mal escrita o que no sea lunes es un error de programacion (RAISERROR, no escribe nada).
   @EstadoEsperado: el estado que el usuario tenia en pantalla (se ignora al
   crear). @EstadoNuevo: el que queda (Borrador del jefe, Aprobado del
   registro directo de Talento Humano...). El bono queda en el periodo
   abierto. MotivoRechazo, NombreTipo y CodigoConcepto no se tocan aqui.
   Respuestas = IdBono, -1 cambio de estado (o ya no existe), -2 no hay
   periodo abierto o el bono es de un periodo que no esta abierto, -3 doble
   pago. */
ALTER PROCEDURE dbo.Sp_RTA_BonoGuardar
    @IdBono         INT,
    @CodUsuario     VARCHAR(50),
    @Empresa        VARCHAR(10),
    @Nombre         NVARCHAR(150),
    @Identificacion VARCHAR(32),
    @Lugar          NVARCHAR(100),
    @IdTipo         INT,
    @Semanas        INT,
    @ValorSugerido  DECIMAL(9,2),
    @Valor          DECIMAL(9,2),
    @MotivoAjuste   NVARCHAR(500),
    @Observacion    NVARCHAR(500),
    @Origen         VARCHAR(15),
    @CodJefe        VARCHAR(50),
    @Lunes          VARCHAR(MAX),
    @EstadoEsperado VARCHAR(10),
    @EstadoNuevo    VARCHAR(10),
    @Usuario        VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @IdBono     = ISNULL(@IdBono, 0);
    SET @CodUsuario = LTRIM(RTRIM(ISNULL(@CodUsuario, '')));
    SET @CodJefe    = NULLIF(LTRIM(RTRIM(ISNULL(@CodJefe, ''))), '');

    DECLARE @L TABLE (Lunes DATE NOT NULL PRIMARY KEY);
    DECLARE @Partes TABLE (Texto VARCHAR(30) NOT NULL, Fecha DATE NULL);
    INSERT INTO @Partes (Texto, Fecha)
    SELECT LTRIM(RTRIM(value)), TRY_CONVERT(DATE, LTRIM(RTRIM(value)), 23)
      FROM STRING_SPLIT(ISNULL(@Lunes, ''), ',')
     WHERE LTRIM(RTRIM(value)) <> '';

    IF EXISTS (SELECT 1 FROM @Partes
                WHERE Fecha IS NULL OR DATEDIFF(DAY, '19000101', Fecha) % 7 <> 0)
    BEGIN
        RAISERROR('Sp_RTA_BonoGuardar: @Lunes trae un valor que no es un lunes yyyy-MM-dd.', 16, 1);
        RETURN;
    END

    INSERT INTO @L (Lunes) SELECT DISTINCT Fecha FROM @Partes;

    BEGIN TRANSACTION;

    /* Primero el periodo y despues el bono, siempre en ese orden (como
       CambiarEstado, Borrar y CerrarPeriodo): las escrituras se serializan
       sobre la fila del periodo abierto. */
    DECLARE @IdAbierto INT;
    SELECT @IdAbierto = IdPeriodo FROM dbo.Bono_Periodo WITH (UPDLOCK, HOLDLOCK) WHERE Estado = 'Abierto';

    DECLARE @Existe BIT = 0, @EstadoActual VARCHAR(10), @PeriodoActual INT, @ValorAnt DECIMAL(9,2);
    IF @IdBono <> 0
    BEGIN
        SELECT @Existe = 1, @EstadoActual = Estado, @PeriodoActual = IdPeriodo, @ValorAnt = Valor
          FROM dbo.Bono_Detalle WITH (UPDLOCK, HOLDLOCK)
         WHERE IdBono = @IdBono;
        IF @Existe = 0 OR @EstadoActual <> ISNULL(@EstadoEsperado, '')
        BEGIN
            COMMIT TRANSACTION;
            SELECT Respuestas = -1;
            RETURN;
        END
    END

    IF @IdAbierto IS NULL OR (@PeriodoActual IS NOT NULL AND @PeriodoActual <> @IdAbierto)
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -2;
        RETURN;
    END

    IF ISNULL(@EstadoNuevo, '') <> 'Rechazado'
       AND EXISTS (SELECT 1
                     FROM dbo.Bono_DetalleSemana AS s WITH (UPDLOCK, HOLDLOCK)
                     JOIN @L AS l ON l.Lunes = s.Lunes
                     JOIN dbo.Bono_Detalle AS d WITH (UPDLOCK, HOLDLOCK) ON d.IdBono = s.IdBono
                    WHERE d.CodUsuario = @CodUsuario
                      AND d.IdTipo = @IdTipo
                      AND d.Estado <> 'Rechazado'
                      AND d.IdBono <> @IdBono)
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -3;
        RETURN;
    END

    IF @IdBono = 0
    BEGIN
        INSERT INTO dbo.Bono_Detalle
            (IdPeriodo, CodUsuario, Empresa, Nombre, Identificacion, Lugar, IdTipo, Semanas,
             ValorSugerido, Valor, MotivoAjuste, Observacion, Estado, Origen, CodJefe,
             UsuarioRevisa, FechaRevisa, Usuario_Registro)
        VALUES
            (@IdAbierto, @CodUsuario,
             NULLIF(UPPER(LTRIM(RTRIM(ISNULL(@Empresa, '')))), ''),
             NULLIF(LTRIM(RTRIM(ISNULL(@Nombre, N''))), N''),
             NULLIF(LTRIM(RTRIM(ISNULL(@Identificacion, ''))), ''),
             NULLIF(LTRIM(RTRIM(ISNULL(@Lugar, N''))), N''),
             @IdTipo, ISNULL(@Semanas, 0), @ValorSugerido, @Valor,
             NULLIF(LTRIM(RTRIM(ISNULL(@MotivoAjuste, N''))), N''),
             NULLIF(LTRIM(RTRIM(ISNULL(@Observacion, N''))), N''),
             @EstadoNuevo, @Origen, @CodJefe,
             CASE WHEN @EstadoNuevo = 'Aprobado' THEN @Usuario END,
             CASE WHEN @EstadoNuevo = 'Aprobado' THEN GETDATE() END,
             @Usuario);
        SET @IdBono = CONVERT(INT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.Bono_Detalle
           SET IdPeriodo      = @IdAbierto,
               CodUsuario     = @CodUsuario,
               Empresa        = NULLIF(UPPER(LTRIM(RTRIM(ISNULL(@Empresa, '')))), ''),
               Nombre         = NULLIF(LTRIM(RTRIM(ISNULL(@Nombre, N''))), N''),
               Identificacion = NULLIF(LTRIM(RTRIM(ISNULL(@Identificacion, ''))), ''),
               Lugar          = NULLIF(LTRIM(RTRIM(ISNULL(@Lugar, N''))), N''),
               IdTipo         = @IdTipo,
               Semanas        = ISNULL(@Semanas, 0),
               ValorSugerido  = @ValorSugerido,
               Valor          = @Valor,
               MotivoAjuste   = NULLIF(LTRIM(RTRIM(ISNULL(@MotivoAjuste, N''))), N''),
               Observacion    = NULLIF(LTRIM(RTRIM(ISNULL(@Observacion, N''))), N''),
               Estado         = @EstadoNuevo,
               Origen         = @Origen,
               CodJefe        = @CodJefe,
               UsuarioRevisa  = CASE WHEN @EstadoNuevo = 'Aprobado' THEN @Usuario ELSE UsuarioRevisa END,
               FechaRevisa    = CASE WHEN @EstadoNuevo = 'Aprobado' THEN GETDATE() ELSE FechaRevisa END,
               Usuario_Modificacion = @Usuario,
               Fecha_Modificacion   = GETDATE()
         WHERE IdBono = @IdBono;
    END

    DELETE s
      FROM dbo.Bono_DetalleSemana AS s
     WHERE s.IdBono = @IdBono
       AND NOT EXISTS (SELECT 1 FROM @L AS l WHERE l.Lunes = s.Lunes);

    INSERT INTO dbo.Bono_DetalleSemana (IdBono, Lunes, Usuario_Registro)
    SELECT @IdBono, l.Lunes, @Usuario
      FROM @L AS l
     WHERE NOT EXISTS (SELECT 1 FROM dbo.Bono_DetalleSemana s
                        WHERE s.IdBono = @IdBono AND s.Lunes = l.Lunes);

    INSERT INTO dbo.Bono_Bitacora (IdBono, IdPeriodo, Accion, ValorAnterior, ValorNuevo, Detalle, Usuario)
    VALUES (@IdBono, @IdAbierto,
            CASE WHEN @Existe = 1 THEN 'Editar' ELSE 'Crear' END,
            CASE WHEN @Existe = 1 THEN @EstadoActual + N' ' + CONVERT(NVARCHAR(20), @ValorAnt) END,
            @EstadoNuevo + N' ' + CONVERT(NVARCHAR(20), @Valor),
            LEFT(N'Persona ' + @CodUsuario + N'; tipo ' + CONVERT(NVARCHAR(12), @IdTipo)
                 + N'; lunes: ' + ISNULL(@Lunes, ''), 2000),
            @Usuario);

    COMMIT TRANSACTION;
    SELECT Respuestas = @IdBono;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoCambiarEstado', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoCambiarEstado AS RETURN 0;');
GO
/* Enviar, aprobar, ajustar o rechazar un bono.
   @Valor NULL = no cambia. @Motivo: va a MotivoRechazo si @EstadoNuevo es
   Rechazado; si no, a MotivoAjuste cuando viene @Valor (NULL = no cambia).
   @Snapshot* (NULL = no cambia): la copia de empresa, nombre e
   identificacion que se toma al enviar o al aprobar.
   Respuestas = 0, -1 cambio de estado (o ya no existe), -2 su periodo no
   esta abierto, -3 doble pago (se comprueba siempre que el estado nuevo no
   sea Rechazado: al aprobar y tambien al enviar). */
ALTER PROCEDURE dbo.Sp_RTA_BonoCambiarEstado
    @IdBono                 INT,
    @EstadoEsperado         VARCHAR(10),
    @EstadoNuevo            VARCHAR(10),
    @Valor                  DECIMAL(9,2)  = NULL,
    @Motivo                 NVARCHAR(500) = NULL,
    @SnapshotEmpresa        VARCHAR(10)   = NULL,
    @SnapshotNombre         NVARCHAR(150) = NULL,
    @SnapshotIdentificacion VARCHAR(32)   = NULL,
    @Usuario                VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    DECLARE @IdAbierto INT;
    SELECT @IdAbierto = IdPeriodo FROM dbo.Bono_Periodo WITH (UPDLOCK, HOLDLOCK) WHERE Estado = 'Abierto';

    DECLARE @Existe BIT = 0, @EstadoActual VARCHAR(10), @PeriodoActual INT, @ValorAnt DECIMAL(9,2),
            @CodUsuario VARCHAR(50), @IdTipo INT;
    SELECT @Existe = 1, @EstadoActual = Estado, @PeriodoActual = IdPeriodo, @ValorAnt = Valor,
           @CodUsuario = CodUsuario, @IdTipo = IdTipo
      FROM dbo.Bono_Detalle WITH (UPDLOCK, HOLDLOCK)
     WHERE IdBono = @IdBono;

    IF @Existe = 0 OR @EstadoActual <> ISNULL(@EstadoEsperado, '')
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -1;
        RETURN;
    END

    IF @IdAbierto IS NULL OR @PeriodoActual IS NULL OR @PeriodoActual <> @IdAbierto
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -2;
        RETURN;
    END

    IF ISNULL(@EstadoNuevo, '') <> 'Rechazado'
       AND EXISTS (SELECT 1
                     FROM dbo.Bono_DetalleSemana AS mia
                     JOIN dbo.Bono_DetalleSemana AS s WITH (UPDLOCK, HOLDLOCK) ON s.Lunes = mia.Lunes
                     JOIN dbo.Bono_Detalle AS d WITH (UPDLOCK, HOLDLOCK) ON d.IdBono = s.IdBono
                    WHERE mia.IdBono = @IdBono
                      AND d.CodUsuario = @CodUsuario
                      AND d.IdTipo = @IdTipo
                      AND d.Estado <> 'Rechazado'
                      AND d.IdBono <> @IdBono)
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -3;
        RETURN;
    END

    DECLARE @Motivo2 NVARCHAR(500) = NULLIF(LTRIM(RTRIM(ISNULL(@Motivo, N''))), N'');

    UPDATE dbo.Bono_Detalle
       SET Estado         = @EstadoNuevo,
           Valor          = ISNULL(@Valor, Valor),
           MotivoAjuste   = CASE WHEN @EstadoNuevo <> 'Rechazado' AND @Valor IS NOT NULL AND @Motivo IS NOT NULL
                                 THEN @Motivo2 ELSE MotivoAjuste END,
           MotivoRechazo  = CASE WHEN @EstadoNuevo = 'Rechazado' AND @Motivo IS NOT NULL
                                 THEN @Motivo2 ELSE MotivoRechazo END,
           Empresa        = CASE WHEN @SnapshotEmpresa IS NOT NULL
                                 THEN NULLIF(UPPER(LTRIM(RTRIM(@SnapshotEmpresa))), '') ELSE Empresa END,
           Nombre         = CASE WHEN @SnapshotNombre IS NOT NULL
                                 THEN NULLIF(LTRIM(RTRIM(@SnapshotNombre)), N'') ELSE Nombre END,
           Identificacion = CASE WHEN @SnapshotIdentificacion IS NOT NULL
                                 THEN NULLIF(LTRIM(RTRIM(@SnapshotIdentificacion)), '') ELSE Identificacion END,
           UsuarioRevisa  = CASE WHEN @EstadoNuevo IN ('Aprobado', 'Rechazado') THEN @Usuario ELSE UsuarioRevisa END,
           FechaRevisa    = CASE WHEN @EstadoNuevo IN ('Aprobado', 'Rechazado') THEN GETDATE() ELSE FechaRevisa END,
           Usuario_Modificacion = @Usuario,
           Fecha_Modificacion   = GETDATE()
     WHERE IdBono = @IdBono;

    INSERT INTO dbo.Bono_Bitacora (IdBono, IdPeriodo, Accion, ValorAnterior, ValorNuevo, Detalle, Usuario)
    VALUES (@IdBono, @PeriodoActual,
            CASE WHEN @EstadoNuevo = 'Enviado'   THEN 'Enviar'
                 WHEN @EstadoNuevo = 'Rechazado' THEN 'Rechazar'
                 WHEN @EstadoNuevo = 'Aprobado' AND @Valor IS NOT NULL AND @Valor <> @ValorAnt THEN 'Ajustar'
                 WHEN @EstadoNuevo = 'Aprobado'  THEN 'Aprobar'
                 ELSE 'Estado' END,
            @EstadoActual + N' ' + CONVERT(NVARCHAR(20), @ValorAnt),
            @EstadoNuevo + N' ' + CONVERT(NVARCHAR(20), ISNULL(@Valor, @ValorAnt)),
            LEFT(@Motivo2, 2000),
            @Usuario);

    COMMIT TRANSACTION;
    SELECT Respuestas = 0;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoBorrar', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoBorrar AS RETURN 0;');
GO
/* Borra el bono y sus lunes. Respuestas = 0, -1 cambio de estado (o ya no
   existe), -2 es de un periodo que no esta abierto (un pendiente sin
   periodo si se puede borrar). Quien puede borrar que lo decide
   NegBonoReglas; la bitacora conserva lo borrado. */
ALTER PROCEDURE dbo.Sp_RTA_BonoBorrar
    @IdBono         INT,
    @EstadoEsperado VARCHAR(10),
    @Usuario        VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    DECLARE @IdAbierto INT;
    SELECT @IdAbierto = IdPeriodo FROM dbo.Bono_Periodo WITH (UPDLOCK, HOLDLOCK) WHERE Estado = 'Abierto';

    DECLARE @Existe BIT = 0, @EstadoActual VARCHAR(10), @PeriodoActual INT, @Detalle NVARCHAR(2000);
    SELECT @Existe = 1, @EstadoActual = Estado, @PeriodoActual = IdPeriodo,
           @Detalle = LEFT(N'Persona ' + CodUsuario + N'; tipo ' + CONVERT(NVARCHAR(12), IdTipo)
                           + N'; valor ' + CONVERT(NVARCHAR(20), Valor)
                           + N'; semanas ' + CONVERT(NVARCHAR(12), Semanas)
                           + N'; origen ' + Origen, 2000)
      FROM dbo.Bono_Detalle WITH (UPDLOCK, HOLDLOCK)
     WHERE IdBono = @IdBono;

    IF @Existe = 0 OR @EstadoActual <> ISNULL(@EstadoEsperado, '')
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -1;
        RETURN;
    END

    IF @PeriodoActual IS NOT NULL AND (@IdAbierto IS NULL OR @PeriodoActual <> @IdAbierto)
    BEGIN
        COMMIT TRANSACTION;
        SELECT Respuestas = -2;
        RETURN;
    END

    DELETE FROM dbo.Bono_DetalleSemana WHERE IdBono = @IdBono;
    DELETE FROM dbo.Bono_Detalle WHERE IdBono = @IdBono;

    INSERT INTO dbo.Bono_Bitacora (IdBono, IdPeriodo, Accion, ValorAnterior, ValorNuevo, Detalle, Usuario)
    VALUES (@IdBono, @PeriodoActual, 'Borrar', @EstadoActual, NULL, @Detalle, @Usuario);

    COMMIT TRANSACTION;
    SELECT Respuestas = 0;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_BonoBitacora', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_BonoBitacora AS RETURN 0;');
GO
/* Fila suelta de bitacora: correos (destinatario y error) y lo que la capa
   de negocio quiera dejar anotado. Los procedimientos que escriben ya dejan
   la suya. */
ALTER PROCEDURE dbo.Sp_RTA_BonoBitacora
    @IdBono        INT,
    @IdPeriodo     INT,
    @Accion        VARCHAR(20),
    @ValorAnterior NVARCHAR(200),
    @ValorNuevo    NVARCHAR(200),
    @Detalle       NVARCHAR(MAX),
    @Usuario       VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Bono_Bitacora (IdBono, IdPeriodo, Accion, ValorAnterior, ValorNuevo, Detalle, Usuario)
    VALUES (NULLIF(@IdBono, 0), NULLIF(@IdPeriodo, 0), ISNULL(@Accion, ''),
            LEFT(@ValorAnterior, 200), LEFT(@ValorNuevo, 200), LEFT(@Detalle, 2000),
            ISNULL(@Usuario, ''));
END
GO

/* ------------------------------------------------- veredicto -------------- */
/* Solo lectura. Sin GO entre el DECLARE de @Fallos y su ultimo uso. */
SET NOCOUNT ON;
DECLARE @Fallos INT = 0;

DECLARE @Faltan INT =
    (SELECT COUNT(*) FROM (VALUES
        ('dbo.Bono_Tipo', 'U'), ('dbo.Bono_TipoConcepto', 'U'), ('dbo.Bono_Periodo', 'U'),
        ('dbo.Bono_Detalle', 'U'), ('dbo.Bono_DetalleSemana', 'U'), ('dbo.Bono_Bitacora', 'U'),
        ('dbo.Sp_RTA_BonoPersonasActivas', 'P'), ('dbo.Sp_RTA_BonoTipos', 'P'),
        ('dbo.Sp_RTA_BonoGuardarTipo', 'P'), ('dbo.Sp_RTA_BonoPeriodos', 'P'),
        ('dbo.Sp_RTA_BonoAbrirPeriodo', 'P'), ('dbo.Sp_RTA_BonoCerrarPeriodo', 'P'),
        ('dbo.Sp_RTA_BonoDetalles', 'P'), ('dbo.Sp_RTA_BonoDePersona', 'P'),
        ('dbo.Sp_RTA_BonoGuardar', 'P'), ('dbo.Sp_RTA_BonoCambiarEstado', 'P'),
        ('dbo.Sp_RTA_BonoBorrar', 'P'), ('dbo.Sp_RTA_BonoBitacora', 'P')
     ) AS o (Nombre, Tipo)
     WHERE OBJECT_ID(o.Nombre, o.Tipo) IS NULL);
IF @Faltan <> 0
BEGIN
    PRINT 'FALLO: faltan ' + CONVERT(VARCHAR(10), @Faltan) + ' tablas o procedimientos Bono.';
    SET @Fallos += 1;
END

DECLARE @Restricciones INT =
    (SELECT COUNT(*) FROM (VALUES
        ('CK_Bono_DetalleSemana_Lunes'), ('PK_Bono_DetalleSemana'), ('UQ_Bono_Tipo_Nombre'),
        ('PK_Bono_TipoConcepto'), ('UQ_Bono_Periodo_AnioMes'), ('CK_Bono_Detalle_Estado'),
        ('CK_Bono_Detalle_Origen'), ('CK_Bono_Periodo_Estado'), ('CK_Bono_Tipo_Familia'),
        ('CK_Bono_TipoConcepto_Empresa'), ('CK_Bono_Detalle_Valor')
     ) AS r (Nombre)
     WHERE OBJECT_ID('dbo.' + r.Nombre) IS NULL);
IF @Restricciones <> 0
BEGIN
    PRINT 'FALLO: faltan ' + CONVERT(VARCHAR(10), @Restricciones) + ' restricciones (CHECK / PK / UNIQUE).';
    SET @Fallos += 1;
END

IF COL_LENGTH('dbo.Bono_Detalle', 'NombreTipo') IS NULL OR COL_LENGTH('dbo.Bono_Detalle', 'CodigoConcepto') IS NULL
BEGIN
    PRINT 'FALLO: Bono_Detalle no tiene NombreTipo o CodigoConcepto.';
    SET @Fallos += 1;
END

/* El CHECK de lunes acepta un lunes y rechaza un martes (sin escribir). */
IF NOT (DATEDIFF(DAY, '19000101', CONVERT(DATE, '20260803')) % 7 = 0
        AND DATEDIFF(DAY, '19000101', CONVERT(DATE, '20260804')) % 7 <> 0)
BEGIN
    PRINT 'FALLO: la expresion del CHECK de lunes no reconoce el 2026-08-03 como lunes.';
    SET @Fallos += 1;
END

IF (SELECT COUNT(*) FROM dbo.Bono_Periodo WHERE Estado = 'Abierto') > 1
BEGIN
    PRINT 'FALLO: hay mas de un periodo abierto.';
    SET @Fallos += 1;
END

SELECT ObjetosFaltantes = @Faltan, RestriccionesFaltantes = @Restricciones, Fallos = @Fallos;
IF @Fallos = 0
    PRINT 'VEREDICTO: OK';
ELSE
    PRINT 'VEREDICTO: FALLA (' + CONVERT(VARCHAR(10), @Fallos) + ' comprobaciones). Revisar arriba.';
SELECT Veredicto = CASE WHEN @Fallos = 0 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA' END;
GO
