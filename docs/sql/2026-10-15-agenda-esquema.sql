/* ============================================================================
   Equipo y agenda (Entrega 1) - esquema
   ReporTarea  |  2026-10-15
   Spec: docs/superpowers/specs/2026-10-15-agenda-equipo-entrega1-design.md
   Crea o actualiza:
     1. Tablas AG_Area, AG_PersonaArea, AG_Actividad, AG_Entregable, AG_CorreoBitacora
     2. La fila "Sin area" de AG_Area
     3. Procedimientos Sp_RTA_Agenda*
   La accion Agenda.VerTodos y el menu van en 2026-10-15-agenda-menu.sql.
   Repetible: correrlo dos veces no duplica nada. No toca tablas existentes.
   Sin tildes a proposito: sqlcmd lee el archivo con la pagina de codigos del
   equipo. Los textos con tilde se arman con NCHAR.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ----------------------------------------------------------- GUARD -------- */
/* (Agregado el 2026-10-23.) Si la Entrega 2 (procesos,
   2026-10-23-procesos-esquema.sql) ya esta aplicada, este script NO debe
   volver a correr: sus ALTER devolverian Sp_RTA_AgendaMarcarEntregable,
   MarcarCompleta, GuardarActividad y las consultas a la version sin procesos,
   y marcar la actividad de un paso dejaria de avanzar el proceso, sin aviso.
   Todo lo que este script crea ya existe en ese caso. */
IF COL_LENGTH('dbo.AG_Actividad', 'IdProcesoPaso') IS NOT NULL
   OR EXISTS (SELECT 1 FROM sys.extended_properties
               WHERE class = 1 AND minor_id = 0 AND major_id = OBJECT_ID('dbo.AG_Actividad')
                 AND name = 'AG_VersionEsquema' AND TRY_CONVERT(INT, value) >= 2)
BEGIN
    PRINT 'La Entrega 2 de la agenda ya esta aplicada. Este script NO se ejecuta.';
    SET NOEXEC ON;
END
GO

/* ------------------------------------------------------- 1. tablas -------- */
IF OBJECT_ID('dbo.AG_Area', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_Area
    (
        IdArea               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_Area PRIMARY KEY,
        Nombre               NVARCHAR(100) NOT NULL CONSTRAINT UQ_AG_Area_Nombre UNIQUE,
        Color                CHAR(7)       NOT NULL,
        Activa               BIT           NOT NULL CONSTRAINT DF_AG_Area_Activa DEFAULT 1,
        EsSinArea            BIT           NOT NULL CONSTRAINT DF_AG_Area_EsSinArea DEFAULT 0,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_AG_Area_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    PRINT 'AG_Area creada.';
END
GO

IF OBJECT_ID('dbo.AG_PersonaArea', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_PersonaArea
    (
        Cod_Usuario          VARCHAR(20) NOT NULL CONSTRAINT PK_AG_PersonaArea PRIMARY KEY,
        IdArea               INT         NOT NULL CONSTRAINT FK_AG_PersonaArea_Area REFERENCES dbo.AG_Area (IdArea),
        Usuario_Registro     VARCHAR(50) NOT NULL,
        Fecha_Registro       DATETIME    NOT NULL CONSTRAINT DF_AG_PersonaArea_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50) NULL,
        Fecha_Modificacion   DATETIME    NULL
    );
    PRINT 'AG_PersonaArea creada.';
END
GO

IF OBJECT_ID('dbo.AG_Actividad', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_Actividad
    (
        IdActividad          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_Actividad PRIMARY KEY,
        Titulo               NVARCHAR(200) NOT NULL,
        Cod_Responsable      VARCHAR(20)   NOT NULL,
        Cod_AsignadoPor      VARCHAR(20)   NOT NULL,
        FechaEntrega         DATE          NOT NULL,
        CompletadaManual     BIT           NOT NULL CONSTRAINT DF_AG_Actividad_Manual DEFAULT 0,
        FechaCompletada      DATETIME      NULL,
        Anulada              BIT           NOT NULL CONSTRAINT DF_AG_Actividad_Anulada DEFAULT 0,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_AG_Actividad_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    CREATE INDEX IX_AG_Actividad_Fecha ON dbo.AG_Actividad (FechaEntrega)
        INCLUDE (Cod_Responsable, Anulada, FechaCompletada);
    PRINT 'AG_Actividad creada.';
END
GO

IF OBJECT_ID('dbo.AG_Entregable', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_Entregable
    (
        IdEntregable         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_Entregable PRIMARY KEY,
        IdActividad          INT           NOT NULL CONSTRAINT FK_AG_Entregable_Actividad REFERENCES dbo.AG_Actividad (IdActividad),
        Descripcion          NVARCHAR(200) NOT NULL,
        Orden                INT           NOT NULL,
        Hecho                BIT           NOT NULL CONSTRAINT DF_AG_Entregable_Hecho DEFAULT 0,
        Cod_Hecho            VARCHAR(20)   NULL,
        Fecha_Hecho          DATETIME      NULL,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_AG_Entregable_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    CREATE INDEX IX_AG_Entregable_Actividad ON dbo.AG_Entregable (IdActividad);
    PRINT 'AG_Entregable creada.';
END
GO

IF OBJECT_ID('dbo.AG_CorreoBitacora', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_CorreoBitacora
    (
        IdCorreo     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_CorreoBitacora PRIMARY KEY,
        IdActividad  INT            NOT NULL CONSTRAINT FK_AG_CorreoBitacora_Actividad REFERENCES dbo.AG_Actividad (IdActividad),
        Destinatario VARCHAR(200)   NULL,
        Asunto       NVARCHAR(300)  NOT NULL,
        Enviado      BIT            NOT NULL,
        Error        NVARCHAR(1000) NULL,
        Fecha        DATETIME       NOT NULL CONSTRAINT DF_AG_CorreoBitacora_Fecha DEFAULT GETDATE()
    );
    PRINT 'AG_CorreoBitacora creada.';
END
GO

/* ------------------------------------------------- 2. "Sin area" ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.AG_Area WHERE EsSinArea = 1)
BEGIN
    INSERT INTO dbo.AG_Area (Nombre, Color, Activa, EsSinArea, Usuario_Registro)
    VALUES (N'Sin ' + NCHAR(225) + N'rea', '#8A8886', 1, 1, 'MIGRACION');
    PRINT 'Fila Sin area creada.';
END
GO

/* ------------------------------------------------- 3. procedimientos ------ */
IF OBJECT_ID('dbo.Sp_RTA_AgendaPersonasActivas', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaPersonasActivas AS RETURN 0;');
GO
/* El UNICO lugar que dice quien es "activo" en la agenda: el criterio de los
   selectores de jefe del sistema (Usuario_Estado = 'A' y EstadoUsuario NULL),
   solo usuarios internos. Si un Cod_Usuario esta repetido, la cuenta mas
   reciente. Sin area, o con su area inactiva: la fila "Sin area". */
ALTER PROCEDURE dbo.Sp_RTA_AgendaPersonasActivas
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @IdSin INT, @NomSin NVARCHAR(100), @ColSin CHAR(7);
    SELECT TOP 1 @IdSin = IdArea, @NomSin = Nombre, @ColSin = Color
      FROM dbo.AG_Area WHERE EsSinArea = 1;

    ;WITH U AS
    (
        SELECT Cod_Usuario = LTRIM(RTRIM(u.Cod_Usuario)),
               Nombre      = LTRIM(RTRIM(ISNULL(u.Nom_Usuario, ''))),
               Correo      = LTRIM(RTRIM(ISNULL(u.E_Mail, ''))),
               CodJefe     = LTRIM(RTRIM(ISNULL(u.Cod_Jefe_Inm, ''))),
               Orden       = ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(u.Cod_Usuario)) ORDER BY u.Id_Usuario DESC)
          FROM dbo.R_Usuarios AS u
         WHERE u.Usuario_Estado = 'A'
           AND u.EstadoUsuario IS NULL
           AND ISNULL(u.IdCliente, 0) = 0
           AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
    )
    SELECT U.Cod_Usuario, U.Nombre, U.Correo, U.CodJefe,
           IdArea    = ISNULL(a.IdArea, @IdSin),
           Area      = ISNULL(a.Nombre, @NomSin),
           Color     = ISNULL(a.Color, @ColSin),
           EsSinArea = CONVERT(BIT, CASE WHEN a.IdArea IS NULL THEN 1 ELSE 0 END)
      FROM U
      LEFT JOIN dbo.AG_PersonaArea AS pa ON pa.Cod_Usuario = U.Cod_Usuario
      LEFT JOIN dbo.AG_Area AS a ON a.IdArea = pa.IdArea AND a.Activa = 1 AND a.EsSinArea = 0
     WHERE U.Orden = 1
     ORDER BY U.Nombre;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaAreas', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaAreas AS RETURN 0;');
GO
ALTER PROCEDURE dbo.Sp_RTA_AgendaAreas
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdArea, Nombre, Color, Activa, EsSinArea
      FROM dbo.AG_Area
     ORDER BY EsSinArea, Nombre;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaGuardarArea', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaGuardarArea AS RETURN 0;');
GO
/* Devuelve el IdArea, o un codigo que CapaNegocio (NegAgendaAreas) traduce:
   -1 nombre repetido, -2 se intento renombrar o desactivar "Sin area",
   -3 el area no existe. @IdArea = 0 crea. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaGuardarArea
    @IdArea  INT,
    @Nombre  NVARCHAR(100),
    @Color   CHAR(7),
    @Activa  BIT,
    @Usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET @Nombre = LTRIM(RTRIM(@Nombre));
    SET @IdArea = ISNULL(@IdArea, 0);

    IF EXISTS (SELECT 1 FROM dbo.AG_Area WHERE Nombre = @Nombre AND IdArea <> @IdArea)
    BEGIN
        SELECT Resultado = -1;
        RETURN;
    END

    IF @IdArea = 0
    BEGIN
        INSERT INTO dbo.AG_Area (Nombre, Color, Activa, EsSinArea, Usuario_Registro)
        VALUES (@Nombre, @Color, @Activa, 0, @Usuario);
        SELECT Resultado = CONVERT(INT, SCOPE_IDENTITY());
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.AG_Area WHERE IdArea = @IdArea)
    BEGIN
        SELECT Resultado = -3;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.AG_Area
                WHERE IdArea = @IdArea AND EsSinArea = 1
                  AND (Nombre <> @Nombre OR @Activa = 0))
    BEGIN
        SELECT Resultado = -2;
        RETURN;
    END

    UPDATE dbo.AG_Area
       SET Nombre = @Nombre, Color = @Color, Activa = @Activa,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdArea = @IdArea;
    SELECT Resultado = @IdArea;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaAsignarArea', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaAsignarArea AS RETURN 0;');
GO
/* Asignar "Sin area" borra la fila: sin fila, la persona cae en "Sin area". */
ALTER PROCEDURE dbo.Sp_RTA_AgendaAsignarArea
    @CodUsuario VARCHAR(20),
    @IdArea     INT,
    @Usuario    VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET @CodUsuario = LTRIM(RTRIM(@CodUsuario));

    IF EXISTS (SELECT 1 FROM dbo.AG_Area WHERE IdArea = @IdArea AND EsSinArea = 1)
    BEGIN
        DELETE FROM dbo.AG_PersonaArea WHERE Cod_Usuario = @CodUsuario;
        RETURN;
    END

    UPDATE dbo.AG_PersonaArea
       SET IdArea = @IdArea, Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE Cod_Usuario = @CodUsuario;
    IF @@ROWCOUNT = 0
        INSERT INTO dbo.AG_PersonaArea (Cod_Usuario, IdArea, Usuario_Registro)
        VALUES (@CodUsuario, @IdArea, @Usuario);
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaRecalcular', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaRecalcular AS RETURN 0;');
GO
/* Mantiene FechaCompletada: se llena la primera vez que la actividad queda
   completa y se vacia si deja de estarlo. La regla es la de
   NegAgendaActividad.Completa: con entregables, todos hechos; sin
   entregables, la marca manual. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaRecalcular
    @IdActividad INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE a
       SET FechaCompletada =
           CASE WHEN (CASE WHEN EXISTS (SELECT 1 FROM dbo.AG_Entregable e WHERE e.IdActividad = a.IdActividad)
                           THEN CASE WHEN NOT EXISTS (SELECT 1 FROM dbo.AG_Entregable e
                                                       WHERE e.IdActividad = a.IdActividad AND e.Hecho = 0)
                                     THEN 1 ELSE 0 END
                           ELSE CONVERT(INT, a.CompletadaManual) END) = 1
                THEN ISNULL(a.FechaCompletada, GETDATE())
                ELSE NULL END
      FROM dbo.AG_Actividad AS a
     WHERE a.IdActividad = @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaActividades', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaActividades AS RETURN 0;');
GO
/* Las no anuladas que vencen en el rango, mas las atrasadas de cualquier
   fecha (sin completar y vencidas antes de @Hoy). Dos resultados:
   actividades y entregables. Quien ve que lo decide CapaNegocio. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaActividades
    @Desde DATE,
    @Hasta DATE,
    @Hoy   DATE
AS
BEGIN
    SET NOCOUNT ON;
    CREATE TABLE #Ids (IdActividad INT PRIMARY KEY);
    INSERT INTO #Ids (IdActividad)
    SELECT a.IdActividad
      FROM dbo.AG_Actividad AS a
     WHERE a.Anulada = 0
       AND (   a.FechaEntrega BETWEEN @Desde AND @Hasta
            OR (a.FechaEntrega < @Hoy AND a.FechaCompletada IS NULL));

    SELECT a.IdActividad, a.Titulo, a.Cod_Responsable, a.Cod_AsignadoPor, a.FechaEntrega,
           a.CompletadaManual, a.FechaCompletada, a.Anulada
      FROM dbo.AG_Actividad AS a
      JOIN #Ids AS i ON i.IdActividad = a.IdActividad
     ORDER BY a.FechaEntrega, a.IdActividad;

    SELECT e.IdEntregable, e.IdActividad, e.Descripcion, e.Orden, e.Hecho
      FROM dbo.AG_Entregable AS e
      JOIN #Ids AS i ON i.IdActividad = e.IdActividad
     ORDER BY e.IdActividad, e.Orden, e.IdEntregable;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaActividad', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaActividad AS RETURN 0;');
GO
/* Una sola, aunque este anulada: el handler decide con ella si se puede tocar. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaActividad
    @IdActividad INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.IdActividad, a.Titulo, a.Cod_Responsable, a.Cod_AsignadoPor, a.FechaEntrega,
           a.CompletadaManual, a.FechaCompletada, a.Anulada
      FROM dbo.AG_Actividad AS a
     WHERE a.IdActividad = @IdActividad;

    SELECT e.IdEntregable, e.IdActividad, e.Descripcion, e.Orden, e.Hecho
      FROM dbo.AG_Entregable AS e
     WHERE e.IdActividad = @IdActividad
     ORDER BY e.Orden, e.IdEntregable;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaGuardarActividad', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaGuardarActividad AS RETURN 0;');
GO
/* @IdActividad = 0 crea; si no, actualiza titulo y fecha (responsable y quien
   asigno no cambian). @Entregables es la lista COMPLETA:
   <es><e id="5" o="1">texto</e><e id="0" o="2">nuevo</e></es>
   Los que no vienen se borran, los que tienen id se actualizan (conservan
   Hecho) y los de id 0 se crean. Devuelve el IdActividad. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaGuardarActividad
    @IdActividad    INT,
    @Titulo         NVARCHAR(200),
    @CodResponsable VARCHAR(20),
    @CodAsignadoPor VARCHAR(20),
    @FechaEntrega   DATE,
    @Entregables    XML,
    @Usuario        VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @E TABLE (IdEntregable INT NOT NULL, Orden INT NOT NULL, Descripcion NVARCHAR(200) NOT NULL);
    INSERT INTO @E (IdEntregable, Orden, Descripcion)
    SELECT ISNULL(x.value('@id', 'INT'), 0),
           ISNULL(x.value('@o', 'INT'), 0),
           LEFT(LTRIM(RTRIM(x.value('.', 'NVARCHAR(400)'))), 200)
      FROM @Entregables.nodes('/es/e') AS t(x);

    BEGIN TRANSACTION;

    IF ISNULL(@IdActividad, 0) = 0
    BEGIN
        INSERT INTO dbo.AG_Actividad (Titulo, Cod_Responsable, Cod_AsignadoPor, FechaEntrega, Usuario_Registro)
        VALUES (LTRIM(RTRIM(@Titulo)), LTRIM(RTRIM(@CodResponsable)), LTRIM(RTRIM(@CodAsignadoPor)), @FechaEntrega, @Usuario);
        SET @IdActividad = CONVERT(INT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.AG_Actividad
           SET Titulo = LTRIM(RTRIM(@Titulo)), FechaEntrega = @FechaEntrega,
               Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
         WHERE IdActividad = @IdActividad AND Anulada = 0;
    END

    DELETE e
      FROM dbo.AG_Entregable AS e
     WHERE e.IdActividad = @IdActividad
       AND NOT EXISTS (SELECT 1 FROM @E AS x WHERE x.IdEntregable = e.IdEntregable);

    UPDATE e
       SET Descripcion = x.Descripcion, Orden = x.Orden,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
      FROM dbo.AG_Entregable AS e
      JOIN @E AS x ON x.IdEntregable = e.IdEntregable
     WHERE e.IdActividad = @IdActividad;

    INSERT INTO dbo.AG_Entregable (IdActividad, Descripcion, Orden, Usuario_Registro)
    SELECT @IdActividad, Descripcion, Orden, @Usuario
      FROM @E
     WHERE IdEntregable = 0;

    EXEC dbo.Sp_RTA_AgendaRecalcular @IdActividad;

    COMMIT TRANSACTION;
    SELECT IdActividad = @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaMarcarEntregable', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaMarcarEntregable AS RETURN 0;');
GO
ALTER PROCEDURE dbo.Sp_RTA_AgendaMarcarEntregable
    @IdEntregable INT,
    @Hecho        BIT,
    @Usuario      VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @IdActividad INT;
    SELECT @IdActividad = IdActividad FROM dbo.AG_Entregable WHERE IdEntregable = @IdEntregable;

    UPDATE dbo.AG_Entregable
       SET Hecho       = @Hecho,
           Cod_Hecho   = CASE WHEN @Hecho = 1 THEN @Usuario END,
           Fecha_Hecho = CASE WHEN @Hecho = 1 THEN GETDATE() END,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdEntregable = @IdEntregable;

    IF @IdActividad IS NOT NULL EXEC dbo.Sp_RTA_AgendaRecalcular @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaMarcarCompleta', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaMarcarCompleta AS RETURN 0;');
GO
/* Solo para actividades sin entregables: con entregables, se completa
   marcandolos. El handler ya lo comprueba; esto es el respaldo. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaMarcarCompleta
    @IdActividad INT,
    @Completa    BIT,
    @Usuario     VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE a
       SET CompletadaManual = @Completa,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
      FROM dbo.AG_Actividad AS a
     WHERE a.IdActividad = @IdActividad
       AND a.Anulada = 0
       AND NOT EXISTS (SELECT 1 FROM dbo.AG_Entregable e WHERE e.IdActividad = a.IdActividad);
    EXEC dbo.Sp_RTA_AgendaRecalcular @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaAnular', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaAnular AS RETURN 0;');
GO
ALTER PROCEDURE dbo.Sp_RTA_AgendaAnular
    @IdActividad INT,
    @Usuario     VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AG_Actividad
       SET Anulada = 1, Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdActividad = @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaRegistrarCorreo', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaRegistrarCorreo AS RETURN 0;');
GO
/* Se llama DESPUES de saber si el correo salio. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaRegistrarCorreo
    @IdActividad  INT,
    @Destinatario VARCHAR(200),
    @Asunto       NVARCHAR(300),
    @Enviado      BIT,
    @Error        NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AG_CorreoBitacora (IdActividad, Destinatario, Asunto, Enviado, Error)
    VALUES (@IdActividad, @Destinatario, ISNULL(@Asunto, N''), @Enviado, LEFT(@Error, 1000));
END
GO

/* ------------------------------------------------- veredicto -------------- */
DECLARE @Faltan INT =
    (SELECT COUNT(*) FROM (VALUES
        ('dbo.AG_Area', 'U'), ('dbo.AG_PersonaArea', 'U'), ('dbo.AG_Actividad', 'U'),
        ('dbo.AG_Entregable', 'U'), ('dbo.AG_CorreoBitacora', 'U'),
        ('dbo.Sp_RTA_AgendaPersonasActivas', 'P'), ('dbo.Sp_RTA_AgendaAreas', 'P'),
        ('dbo.Sp_RTA_AgendaGuardarArea', 'P'), ('dbo.Sp_RTA_AgendaAsignarArea', 'P'),
        ('dbo.Sp_RTA_AgendaRecalcular', 'P'), ('dbo.Sp_RTA_AgendaActividades', 'P'),
        ('dbo.Sp_RTA_AgendaActividad', 'P'), ('dbo.Sp_RTA_AgendaGuardarActividad', 'P'),
        ('dbo.Sp_RTA_AgendaMarcarEntregable', 'P'), ('dbo.Sp_RTA_AgendaMarcarCompleta', 'P'),
        ('dbo.Sp_RTA_AgendaAnular', 'P'), ('dbo.Sp_RTA_AgendaRegistrarCorreo', 'P')
     ) AS o (Nombre, Tipo)
     WHERE OBJECT_ID(o.Nombre, o.Tipo) IS NULL);
DECLARE @SinArea INT = (SELECT COUNT(*) FROM dbo.AG_Area WHERE EsSinArea = 1 AND Activa = 1);
SELECT ObjetosFaltantes = @Faltan, FilasSinArea = @SinArea;
SELECT Veredicto = CASE WHEN @Faltan = 0 AND @SinArea = 1 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA' END;
GO

/* Deshace el guard de la cabecera. */
SET NOEXEC OFF;
GO
