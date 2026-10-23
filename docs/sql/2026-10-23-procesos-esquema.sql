/* ============================================================================
   Procesos entre areas (Entrega 2 de la agenda) - esquema
   ReporTarea  |  2026-10-23
   Spec: docs/superpowers/specs/2026-10-23-procesos-entrega2-design.md
   Crea o actualiza:
     1. Tablas AG_Plantilla, AG_PlantillaPaso, AG_Proceso, AG_ProcesoPaso
     2. AG_Actividad.IdProcesoPaso (FK + indice unico filtrado: a lo sumo una
        actividad por paso) y AG_CorreoBitacora (IdActividad admite NULL,
        IdProceso nuevo, CHECK de al menos uno)
     3. Procedimientos nuevos Sp_RTA_Agenda* de plantillas y procesos
     4. Procedimientos de la Entrega 1 que cambian: Actividades, Actividad,
        GuardarActividad, MarcarEntregable, MarcarCompleta, RegistrarCorreo.
        Para una actividad SIN IdProcesoPaso hacen exactamente lo mismo que
        antes; MarcarEntregable y MarcarCompleta devuelven ademas una fila
        (Respuestas = 0, IdProcesoPasoActivado y IdProcesoTerminado NULL) que
        el DAO actual (ExecuteNonQuery) no lee.
        El cuerpo anterior de cada uno -el que corre en produccion el
        2026-10-23- esta comentado al final, para la reversa.
     5. La marca de version AG_VersionEsquema = 2 (propiedad extendida de
        dbo.AG_Actividad).
   No toca ninguna tabla fuera de AG_*. No precarga plantillas.

   GUARD DE VERSION: si la base ya tiene una version de esquema MAYOR que 2
   (una entrega posterior cambio estos procedimientos), el script entero se
   salta con NOEXEC: volver a correrlo devolveria los procedimientos a esta
   version. Con version 2 o sin marca se ejecuta, y es repetible: correrlo dos
   veces no duplica nada.
   2026-10-15-agenda-esquema.sql tiene el guard simetrico: no corre si esta
   entrega ya esta aplicada (si corriera, borraria el avance de procesos de
   los procedimientos de marcado).

   Sin tildes a proposito: sqlcmd lee el archivo con la pagina de codigos del
   equipo. Los textos con tilde se arman con NCHAR.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO

/* ----------------------------------------------------------- GUARD -------- */
/* Requiere la Entrega 1: este script ALTERa sus procedimientos. */
IF OBJECT_ID('dbo.AG_Actividad', 'U') IS NULL OR OBJECT_ID('dbo.Sp_RTA_AgendaMarcarEntregable', 'P') IS NULL
BEGIN
    PRINT 'Falta la Entrega 1 (2026-10-15-agenda-esquema.sql). Este script NO se ejecuta.';
    SET NOEXEC ON;
END
GO

IF EXISTS (SELECT 1 FROM sys.extended_properties
            WHERE class = 1 AND minor_id = 0
              AND major_id = OBJECT_ID('dbo.AG_Actividad')
              AND name = 'AG_VersionEsquema'
              AND TRY_CONVERT(INT, value) > 2)
BEGIN
    PRINT 'La base tiene una version de esquema de agenda posterior a la 2.';
    PRINT 'Este script NO se ejecuta: devolveria los procedimientos a la version 2.';
    SET NOEXEC ON;
END
GO

/* ------------------------------------------------------- 1. tablas -------- */
IF OBJECT_ID('dbo.AG_Plantilla', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_Plantilla
    (
        IdPlantilla          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_Plantilla PRIMARY KEY,
        Nombre               NVARCHAR(150) NOT NULL CONSTRAINT UQ_AG_Plantilla_Nombre UNIQUE,
        Descripcion          NVARCHAR(500) NULL,
        Activa               BIT           NOT NULL CONSTRAINT DF_AG_Plantilla_Activa DEFAULT 1,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_AG_Plantilla_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    PRINT 'AG_Plantilla creada.';
END
GO

IF OBJECT_ID('dbo.AG_PlantillaPaso', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_PlantillaPaso
    (
        IdPlantillaPaso      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_PlantillaPaso PRIMARY KEY,
        IdPlantilla          INT            NOT NULL CONSTRAINT FK_AG_PlantillaPaso_Plantilla REFERENCES dbo.AG_Plantilla (IdPlantilla),
        Orden                INT            NOT NULL,
        IdArea               INT            NOT NULL CONSTRAINT FK_AG_PlantillaPaso_Area REFERENCES dbo.AG_Area (IdArea),
        Titulo               NVARCHAR(200)  NOT NULL,
        PlazoDias            INT            NOT NULL CONSTRAINT CK_AG_PlantillaPaso_Plazo CHECK (PlazoDias BETWEEN 1 AND 60),
        Entregables          NVARCHAR(2000) NULL,
        Usuario_Registro     VARCHAR(50)    NOT NULL,
        Fecha_Registro       DATETIME       NOT NULL CONSTRAINT DF_AG_PlantillaPaso_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)    NULL,
        Fecha_Modificacion   DATETIME       NULL,
        CONSTRAINT UQ_AG_PlantillaPaso_Orden UNIQUE (IdPlantilla, Orden)
    );
    PRINT 'AG_PlantillaPaso creada.';
END
GO

IF OBJECT_ID('dbo.AG_Proceso', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_Proceso
    (
        IdProceso            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_Proceso PRIMARY KEY,
        Nombre               NVARCHAR(200) NOT NULL,
        IdPlantilla          INT           NOT NULL CONSTRAINT FK_AG_Proceso_Plantilla REFERENCES dbo.AG_Plantilla (IdPlantilla),
        Cod_IniciadoPor      VARCHAR(20)   NOT NULL,
        Estado               VARCHAR(20)   NOT NULL CONSTRAINT CK_AG_Proceso_Estado CHECK (Estado IN ('EnCurso', 'Terminado', 'Cancelado')),
        FechaInicio          DATETIME      NOT NULL CONSTRAINT DF_AG_Proceso_FechaInicio DEFAULT GETDATE(),
        FechaFin             DATETIME      NULL,
        MotivoCancelacion    NVARCHAR(400) NULL,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_AG_Proceso_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    CREATE INDEX IX_AG_Proceso_Plantilla ON dbo.AG_Proceso (IdPlantilla);
    PRINT 'AG_Proceso creada.';
END
GO

IF OBJECT_ID('dbo.AG_ProcesoPaso', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_ProcesoPaso
    (
        IdProcesoPaso        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_ProcesoPaso PRIMARY KEY,
        IdProceso            INT            NOT NULL CONSTRAINT FK_AG_ProcesoPaso_Proceso REFERENCES dbo.AG_Proceso (IdProceso),
        Orden                INT            NOT NULL,
        IdArea               INT            NOT NULL CONSTRAINT FK_AG_ProcesoPaso_Area REFERENCES dbo.AG_Area (IdArea),
        Titulo               NVARCHAR(200)  NOT NULL,
        PlazoDias            INT            NOT NULL CONSTRAINT CK_AG_ProcesoPaso_Plazo CHECK (PlazoDias BETWEEN 1 AND 60),
        Entregables          NVARCHAR(2000) NULL,
        Cod_Responsable      VARCHAR(20)    NOT NULL,
        Estado               VARCHAR(20)    NOT NULL CONSTRAINT CK_AG_ProcesoPaso_Estado CHECK (Estado IN ('Pendiente', 'EnCurso', 'Completo', 'Cancelado')),
        FechaActivacion      DATETIME       NULL,
        FechaCompletado      DATETIME       NULL,
        Usuario_Registro     VARCHAR(50)    NOT NULL,
        Fecha_Registro       DATETIME       NOT NULL CONSTRAINT DF_AG_ProcesoPaso_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)    NULL,
        Fecha_Modificacion   DATETIME       NULL,
        CONSTRAINT UQ_AG_ProcesoPaso_Orden UNIQUE (IdProceso, Orden)
    );
    PRINT 'AG_ProcesoPaso creada.';
END
GO

/* --------------------------------------- 2. columnas en tablas de la E1 --- */
IF COL_LENGTH('dbo.AG_Actividad', 'IdProcesoPaso') IS NULL
BEGIN
    ALTER TABLE dbo.AG_Actividad ADD IdProcesoPaso INT NULL
        CONSTRAINT FK_AG_Actividad_ProcesoPaso REFERENCES dbo.AG_ProcesoPaso (IdProcesoPaso);
    PRINT 'AG_Actividad.IdProcesoPaso agregada.';
END
GO

/* Unico y filtrado: a lo sumo UNA actividad por paso, aunque dos marcas
   simultaneas lograran saltarse el bloqueo. Tambien es el indice de la FK. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.AG_Actividad') AND name = 'UX_AG_Actividad_ProcesoPaso')
BEGIN
    CREATE UNIQUE INDEX UX_AG_Actividad_ProcesoPaso ON dbo.AG_Actividad (IdProcesoPaso)
        WHERE IdProcesoPaso IS NOT NULL;
    PRINT 'UX_AG_Actividad_ProcesoPaso creado.';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns
            WHERE object_id = OBJECT_ID('dbo.AG_CorreoBitacora') AND name = 'IdActividad' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.AG_CorreoBitacora ALTER COLUMN IdActividad INT NULL;
    PRINT 'AG_CorreoBitacora.IdActividad admite NULL.';
END
GO

IF COL_LENGTH('dbo.AG_CorreoBitacora', 'IdProceso') IS NULL
BEGIN
    ALTER TABLE dbo.AG_CorreoBitacora ADD IdProceso INT NULL
        CONSTRAINT FK_AG_CorreoBitacora_Proceso REFERENCES dbo.AG_Proceso (IdProceso);
    PRINT 'AG_CorreoBitacora.IdProceso agregada.';
END
GO

IF OBJECT_ID('dbo.CK_AG_CorreoBitacora_Origen', 'C') IS NULL
BEGIN
    ALTER TABLE dbo.AG_CorreoBitacora ADD CONSTRAINT CK_AG_CorreoBitacora_Origen
        CHECK (IdActividad IS NOT NULL OR IdProceso IS NOT NULL);
    PRINT 'CK_AG_CorreoBitacora_Origen creado.';
END
GO

/* ------------------------------------- 3. procedimientos internos --------- */
/* Los tres siguientes NO los llama C#: los usan los procedimientos de abajo,
   dentro de su transaccion.

   ORDEN DE BLOQUEO (siempre el mismo, para no tener abrazos mortales):
   fila de AG_Proceso (UPDLOCK, HOLDLOCK) -> filas de AG_ProcesoPaso del
   proceso (UPDLOCK, HOLDLOCK) -> AG_Actividad / AG_Entregable. */
IF OBJECT_ID('dbo.Sp_RTA_AgendaPasoBloquear', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaPasoBloquear AS RETURN 0;');
GO
/* Bloquea el proceso del paso y todos sus pasos hasta el fin de la
   transaccion del que llama, y devuelve el estado (ya bloqueado) del paso y
   del proceso. Exige una transaccion abierta. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaPasoBloquear
    @IdProcesoPaso INT,
    @EstadoPaso    VARCHAR(20) OUTPUT,
    @EstadoProceso VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @@TRANCOUNT = 0
        THROW 50001, 'Sp_RTA_AgendaPasoBloquear necesita una transaccion abierta.', 1;

    DECLARE @IdProceso INT, @Pasos INT;
    SET @EstadoPaso = NULL;
    SET @EstadoProceso = NULL;

    SELECT @IdProceso = IdProceso FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @IdProcesoPaso;
    IF @IdProceso IS NULL RETURN;

    SELECT @EstadoProceso = Estado
      FROM dbo.AG_Proceso WITH (UPDLOCK, HOLDLOCK)
     WHERE IdProceso = @IdProceso;

    SELECT @Pasos = COUNT(*)
      FROM dbo.AG_ProcesoPaso WITH (UPDLOCK, HOLDLOCK)
     WHERE IdProceso = @IdProceso;

    SELECT @EstadoPaso = Estado FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @IdProcesoPaso;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaActivarPaso', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaActivarPaso AS RETURN 0;');
GO
/* Activa un paso Pendiente: estado EnCurso, FechaActivacion, y su actividad
   (titulo y responsable del paso, Cod_AsignadoPor = quien inicio el proceso,
   Usuario_Registro = quien disparo la activacion) con sus entregables, uno por
   linea del campo Entregables, en orden. Si el paso ya tiene actividad no
   crea otra. El que llama ya tiene el proceso bloqueado. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaActivarPaso
    @IdProcesoPaso INT,
    @FechaEntrega  DATE,
    @Usuario       VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @@TRANCOUNT = 0
        THROW 50002, 'Sp_RTA_AgendaActivarPaso necesita una transaccion abierta.', 1;
    IF @FechaEntrega IS NULL
        THROW 50003, 'Sp_RTA_AgendaActivarPaso: falta la fecha de entrega.', 1;

    DECLARE @Titulo NVARCHAR(200), @Responsable VARCHAR(20), @Entregables NVARCHAR(2000),
            @Iniciador VARCHAR(20), @IdActividad INT;

    SELECT @Titulo = pp.Titulo, @Responsable = pp.Cod_Responsable, @Entregables = pp.Entregables,
           @Iniciador = p.Cod_IniciadoPor
      FROM dbo.AG_ProcesoPaso AS pp
      JOIN dbo.AG_Proceso AS p ON p.IdProceso = pp.IdProceso
     WHERE pp.IdProcesoPaso = @IdProcesoPaso;

    UPDATE dbo.AG_ProcesoPaso
       SET Estado = 'EnCurso', FechaActivacion = GETDATE(),
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdProcesoPaso = @IdProcesoPaso AND Estado = 'Pendiente';
    IF @@ROWCOUNT = 0
        THROW 50004, 'Sp_RTA_AgendaActivarPaso: el paso no existe o no estaba Pendiente.', 1;

    IF EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdProcesoPaso = @IdProcesoPaso)
        RETURN;

    INSERT INTO dbo.AG_Actividad (Titulo, Cod_Responsable, Cod_AsignadoPor, FechaEntrega, Usuario_Registro, IdProcesoPaso)
    VALUES (@Titulo, @Responsable, @Iniciador, @FechaEntrega, @Usuario, @IdProcesoPaso);
    SET @IdActividad = CONVERT(INT, SCOPE_IDENTITY());

    /* Una linea por entregable; se ignoran las vacias. */
    DECLARE @T NVARCHAR(MAX) = REPLACE(ISNULL(@Entregables, N''), NCHAR(13), N'') + NCHAR(10);
    DECLARE @L TABLE (N INT NOT NULL PRIMARY KEY, Texto NVARCHAR(2000) NOT NULL);
    ;WITH S AS
    (
        SELECT N = 1, Ini = CONVERT(BIGINT, 1), Fin = CONVERT(BIGINT, CHARINDEX(NCHAR(10), @T))
        UNION ALL
        SELECT N + 1, Fin + 1, CONVERT(BIGINT, CHARINDEX(NCHAR(10), @T, Fin + 1))
          FROM S
         WHERE CHARINDEX(NCHAR(10), @T, Fin + 1) > 0
    )
    INSERT INTO @L (N, Texto)
    SELECT N, LTRIM(RTRIM(REPLACE(SUBSTRING(@T, Ini, Fin - Ini), NCHAR(9), N' ')))
      FROM S
    OPTION (MAXRECURSION 0);

    INSERT INTO dbo.AG_Entregable (IdActividad, Descripcion, Orden, Usuario_Registro)
    SELECT @IdActividad, LEFT(Texto, 200), ROW_NUMBER() OVER (ORDER BY N), @Usuario
      FROM @L
     WHERE Texto <> N''
     ORDER BY N;

    EXEC dbo.Sp_RTA_AgendaRecalcular @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaPasoAvanzar', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaPasoAvanzar AS RETURN 0;');
GO
/* Despues de recalcular la actividad de un paso: si el paso esta EnCurso y su
   actividad (no anulada) quedo completa, el paso pasa a Completo y se activa
   el siguiente por Orden, o el proceso queda Terminado. Si no, no hace nada.
   El que llama ya tiene el proceso bloqueado y ya comprobo que, si hay
   siguiente, trae fecha; si no la trae, esto es un error de programa y se
   lanza (XACT_ABORT deshace todo). */
ALTER PROCEDURE dbo.Sp_RTA_AgendaPasoAvanzar
    @IdProcesoPaso         INT,
    @FechaEntregaSiguiente DATE,
    @Usuario               VARCHAR(50),
    @IdProcesoPasoActivado INT OUTPUT,
    @IdProcesoTerminado    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @IdProcesoPasoActivado = NULL;
    SET @IdProcesoTerminado = NULL;
    IF @@TRANCOUNT = 0
        THROW 50005, 'Sp_RTA_AgendaPasoAvanzar necesita una transaccion abierta.', 1;

    DECLARE @IdProceso INT, @Orden INT, @Estado VARCHAR(20), @Siguiente INT, @EstadoSiguiente VARCHAR(20);
    SELECT @IdProceso = IdProceso, @Orden = Orden, @Estado = Estado
      FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @IdProcesoPaso;
    IF @Estado IS NULL OR @Estado <> 'EnCurso' RETURN;

    IF NOT EXISTS (SELECT 1 FROM dbo.AG_Actividad
                    WHERE IdProcesoPaso = @IdProcesoPaso AND Anulada = 0 AND FechaCompletada IS NOT NULL)
        RETURN;

    SELECT TOP 1 @Siguiente = IdProcesoPaso, @EstadoSiguiente = Estado
      FROM dbo.AG_ProcesoPaso
     WHERE IdProceso = @IdProceso AND Orden > @Orden
     ORDER BY Orden;

    IF @Siguiente IS NOT NULL AND @EstadoSiguiente <> 'Pendiente'
        THROW 50006, 'Sp_RTA_AgendaPasoAvanzar: el paso siguiente no esta Pendiente.', 1;
    IF @Siguiente IS NOT NULL AND @FechaEntregaSiguiente IS NULL
        THROW 50008, 'Sp_RTA_AgendaPasoAvanzar: falta @FechaEntregaSiguiente.', 1;

    UPDATE dbo.AG_ProcesoPaso
       SET Estado = 'Completo', FechaCompletado = GETDATE(),
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdProcesoPaso = @IdProcesoPaso;

    IF @Siguiente IS NOT NULL
    BEGIN
        EXEC dbo.Sp_RTA_AgendaActivarPaso @Siguiente, @FechaEntregaSiguiente, @Usuario;
        SET @IdProcesoPasoActivado = @Siguiente;
    END
    ELSE
    BEGIN
        UPDATE dbo.AG_Proceso
           SET Estado = 'Terminado', FechaFin = GETDATE(),
               Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
         WHERE IdProceso = @IdProceso;
        SET @IdProcesoTerminado = @IdProceso;
    END
END
GO

/* ------------------------------------- 4. procedimientos nuevos ----------- */
IF OBJECT_ID('dbo.Sp_RTA_AgendaFeriados', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaFeriados AS RETURN 0;');
GO
/* Feriados activos desde hoy - 30 dias (para sumar dias habiles en C#). */
ALTER PROCEDURE dbo.Sp_RTA_AgendaFeriados
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Fecha = f.Fecha
      FROM dbo.Feriado AS f
     WHERE f.Activo = 1
       AND f.Fecha >= DATEADD(DAY, -30, CONVERT(DATE, GETDATE()))
     ORDER BY f.Fecha;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaPlantillas', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaPlantillas AS RETURN 0;');
GO
/* Dos resultados: plantillas (activas e inactivas, con TieneProcesos) y sus
   pasos con el nombre del area. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaPlantillas
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.IdPlantilla, p.Nombre, Descripcion = ISNULL(p.Descripcion, N''), p.Activa,
           TieneProcesos = CONVERT(BIT, CASE WHEN EXISTS (SELECT 1 FROM dbo.AG_Proceso AS x
                                                           WHERE x.IdPlantilla = p.IdPlantilla)
                                             THEN 1 ELSE 0 END)
      FROM dbo.AG_Plantilla AS p
     ORDER BY p.Nombre;

    SELECT s.IdPlantillaPaso, s.IdPlantilla, s.Orden, s.IdArea, Area = a.Nombre, s.Titulo, s.PlazoDias,
           Entregables = ISNULL(s.Entregables, N'')
      FROM dbo.AG_PlantillaPaso AS s
      JOIN dbo.AG_Area AS a ON a.IdArea = s.IdArea
     ORDER BY s.IdPlantilla, s.Orden;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaGuardarPlantilla', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaGuardarPlantilla AS RETURN 0;');
GO
/* @IdPlantilla = 0 crea. @Pasos es la lista COMPLETA y reemplaza la anterior
   (los procesos ya iniciados tienen su copia: no se ven afectados):
   <ps><p o="1" a="3" t="titulo" d="5" e="uno&#xA;dos"/>...</ps>
   o = orden (1..n consecutivo), a = IdArea (activa, no "Sin area"),
   t = titulo, d = plazo en dias habiles (1 a 60), e = entregables, uno por
   linea (XAttribute de C# escribe el salto como &#xA;: se conserva).
   Devuelve Resultado = IdPlantilla, o -1 nombre repetido, -2 datos invalidos
   (respaldo de NegAgendaProceso.ValidarPlantilla), -3 la plantilla no existe. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaGuardarPlantilla
    @IdPlantilla INT,
    @Nombre      NVARCHAR(150),
    @Descripcion NVARCHAR(500),
    @Activa      BIT,
    @Pasos       XML,
    @Usuario     VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @Nombre = LTRIM(RTRIM(ISNULL(@Nombre, N'')));
    SET @IdPlantilla = ISNULL(@IdPlantilla, 0);

    DECLARE @P TABLE (Orden INT NULL, IdArea INT NULL, Titulo NVARCHAR(200) NULL, PlazoDias INT NULL,
                      Entregables NVARCHAR(2000) NULL);
    INSERT INTO @P (Orden, IdArea, Titulo, PlazoDias, Entregables)
    SELECT x.value('@o', 'INT'), x.value('@a', 'INT'),
           LEFT(LTRIM(RTRIM(ISNULL(x.value('@t', 'NVARCHAR(400)'), N''))), 200),
           x.value('@d', 'INT'),
           NULLIF(LEFT(LTRIM(RTRIM(ISNULL(x.value('@e', 'NVARCHAR(4000)'), N''))), 2000), N'')
      FROM @Pasos.nodes('//p') AS t(x);

    DECLARE @N INT = (SELECT COUNT(*) FROM @P);
    IF @Nombre = N'' OR @N = 0
       OR EXISTS (SELECT 1 FROM @P WHERE Orden IS NULL OR IdArea IS NULL OR PlazoDias IS NULL
                                       OR PlazoDias NOT BETWEEN 1 AND 60 OR Titulo = N'')
       OR (SELECT COUNT(DISTINCT Orden) FROM @P) <> @N
       OR (SELECT MIN(Orden) FROM @P) <> 1
       OR (SELECT MAX(Orden) FROM @P) <> @N
       OR EXISTS (SELECT 1 FROM @P AS x
                   WHERE NOT EXISTS (SELECT 1 FROM dbo.AG_Area AS a
                                      WHERE a.IdArea = x.IdArea AND a.Activa = 1 AND a.EsSinArea = 0))
    BEGIN
        SELECT Resultado = -2;
        RETURN;
    END

    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.AG_Plantilla WITH (UPDLOCK, HOLDLOCK)
                WHERE Nombre = @Nombre AND IdPlantilla <> @IdPlantilla)
    BEGIN
        COMMIT TRANSACTION;
        SELECT Resultado = -1;
        RETURN;
    END

    IF @IdPlantilla = 0
    BEGIN
        INSERT INTO dbo.AG_Plantilla (Nombre, Descripcion, Activa, Usuario_Registro)
        VALUES (@Nombre, NULLIF(LTRIM(RTRIM(@Descripcion)), N''), ISNULL(@Activa, 1), @Usuario);
        SET @IdPlantilla = CONVERT(INT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.AG_Plantilla
           SET Nombre = @Nombre, Descripcion = NULLIF(LTRIM(RTRIM(@Descripcion)), N''),
               Activa = ISNULL(@Activa, Activa),
               Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
         WHERE IdPlantilla = @IdPlantilla;
        IF @@ROWCOUNT = 0
        BEGIN
            COMMIT TRANSACTION;
            SELECT Resultado = -3;
            RETURN;
        END
        DELETE FROM dbo.AG_PlantillaPaso WHERE IdPlantilla = @IdPlantilla;
    END

    INSERT INTO dbo.AG_PlantillaPaso (IdPlantilla, Orden, IdArea, Titulo, PlazoDias, Entregables, Usuario_Registro)
    SELECT @IdPlantilla, Orden, IdArea, Titulo, PlazoDias, Entregables, @Usuario
      FROM @P
     ORDER BY Orden;

    COMMIT TRANSACTION;
    SELECT Resultado = @IdPlantilla;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaProcesos', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaProcesos AS RETURN 0;');
GO
/* Todos los procesos, de cualquier estado: quien ve cual lo decide C#
   (NegAgendaProceso.PuedeVer). Dos resultados: procesos y pasos, cada paso
   con su area, color, y la FechaEntrega / ActividadCompleta de su actividad
   (NULL / 0 si todavia no tiene). */
ALTER PROCEDURE dbo.Sp_RTA_AgendaProcesos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.IdProceso, p.Nombre, p.IdPlantilla, Plantilla = pl.Nombre,
           CodIniciadoPor = p.Cod_IniciadoPor, p.Estado, p.FechaInicio, p.FechaFin,
           MotivoCancelacion = ISNULL(p.MotivoCancelacion, N'')
      FROM dbo.AG_Proceso AS p
      JOIN dbo.AG_Plantilla AS pl ON pl.IdPlantilla = p.IdPlantilla
     ORDER BY p.FechaInicio DESC, p.IdProceso DESC;

    SELECT pp.IdProcesoPaso, pp.IdProceso, pp.Orden, pp.IdArea, Area = ar.Nombre, ar.Color,
           pp.Titulo, pp.PlazoDias, Entregables = ISNULL(pp.Entregables, N''),
           CodResponsable = pp.Cod_Responsable, pp.Estado, pp.FechaActivacion, pp.FechaCompletado,
           a.IdActividad, a.FechaEntrega,
           ActividadCompleta = CONVERT(BIT, CASE WHEN a.FechaCompletada IS NOT NULL THEN 1 ELSE 0 END)
      FROM dbo.AG_ProcesoPaso AS pp
      JOIN dbo.AG_Area AS ar ON ar.IdArea = pp.IdArea
      LEFT JOIN dbo.AG_Actividad AS a ON a.IdProcesoPaso = pp.IdProcesoPaso
     ORDER BY pp.IdProceso, pp.Orden;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaIniciarProceso', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaIniciarProceso AS RETURN 0;');
GO
/* Crea el proceso (Cod_IniciadoPor = @Usuario, que es el Cod_Usuario de quien
   inicia), copia los pasos como Pendiente y activa el 1.o con
   @FechaEntregaPrimero (la calcula C# con dias habiles).
   @Pasos: <ps><p o="1" a="3" t="titulo" d="5" e="uno&#xA;dos" r="COD"/>...</ps>
   (r = Cod_Usuario del responsable; el resto como en GuardarPlantilla).
   Devuelve IdProceso e IdProcesoPasoActivado; si no inicia, IdProceso es
   -2 datos invalidos (respaldo de NegAgendaProceso; incluye un area
   inexistente, inactiva o "Sin area", como en GuardarPlantilla) o -3 la
   plantilla no existe o esta inactiva, e IdProcesoPasoActivado NULL. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaIniciarProceso
    @Nombre              NVARCHAR(200),
    @IdPlantilla         INT,
    @Pasos               XML,
    @FechaEntregaPrimero DATE,
    @Usuario             VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @Nombre = LTRIM(RTRIM(ISNULL(@Nombre, N'')));

    DECLARE @P TABLE (Orden INT NULL, IdArea INT NULL, Titulo NVARCHAR(200) NULL, PlazoDias INT NULL,
                      Entregables NVARCHAR(2000) NULL, Responsable VARCHAR(20) NULL);
    INSERT INTO @P (Orden, IdArea, Titulo, PlazoDias, Entregables, Responsable)
    SELECT x.value('@o', 'INT'), x.value('@a', 'INT'),
           LEFT(LTRIM(RTRIM(ISNULL(x.value('@t', 'NVARCHAR(400)'), N''))), 200),
           x.value('@d', 'INT'),
           NULLIF(LEFT(LTRIM(RTRIM(ISNULL(x.value('@e', 'NVARCHAR(4000)'), N''))), 2000), N''),
           LEFT(LTRIM(RTRIM(ISNULL(x.value('@r', 'VARCHAR(50)'), ''))), 20)
      FROM @Pasos.nodes('//p') AS t(x);

    DECLARE @N INT = (SELECT COUNT(*) FROM @P);
    IF @Nombre = N'' OR @N = 0 OR @FechaEntregaPrimero IS NULL OR LTRIM(RTRIM(ISNULL(@Usuario, ''))) = ''
       OR EXISTS (SELECT 1 FROM @P WHERE Orden IS NULL OR IdArea IS NULL OR PlazoDias IS NULL
                                       OR PlazoDias NOT BETWEEN 1 AND 60 OR Titulo = N'' OR Responsable = '')
       OR (SELECT COUNT(DISTINCT Orden) FROM @P) <> @N
       OR (SELECT MIN(Orden) FROM @P) <> 1
       OR (SELECT MAX(Orden) FROM @P) <> @N
       OR EXISTS (SELECT 1 FROM @P AS x
                   WHERE NOT EXISTS (SELECT 1 FROM dbo.AG_Area AS a
                                      WHERE a.IdArea = x.IdArea AND a.Activa = 1 AND a.EsSinArea = 0))
    BEGIN
        SELECT IdProceso = -2, IdProcesoPasoActivado = CONVERT(INT, NULL);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.AG_Plantilla WHERE IdPlantilla = @IdPlantilla AND Activa = 1)
    BEGIN
        SELECT IdProceso = -3, IdProcesoPasoActivado = CONVERT(INT, NULL);
        RETURN;
    END

    DECLARE @IdProceso INT, @Primero INT;

    BEGIN TRANSACTION;

    INSERT INTO dbo.AG_Proceso (Nombre, IdPlantilla, Cod_IniciadoPor, Estado, Usuario_Registro)
    VALUES (@Nombre, @IdPlantilla, LEFT(LTRIM(RTRIM(@Usuario)), 20), 'EnCurso', @Usuario);
    SET @IdProceso = CONVERT(INT, SCOPE_IDENTITY());

    INSERT INTO dbo.AG_ProcesoPaso (IdProceso, Orden, IdArea, Titulo, PlazoDias, Entregables, Cod_Responsable,
                                    Estado, Usuario_Registro)
    SELECT @IdProceso, Orden, IdArea, Titulo, PlazoDias, Entregables, Responsable, 'Pendiente', @Usuario
      FROM @P
     ORDER BY Orden;

    SELECT @Primero = IdProcesoPaso FROM dbo.AG_ProcesoPaso WHERE IdProceso = @IdProceso AND Orden = 1;
    EXEC dbo.Sp_RTA_AgendaActivarPaso @Primero, @FechaEntregaPrimero, @Usuario;

    COMMIT TRANSACTION;
    SELECT IdProceso = @IdProceso, IdProcesoPasoActivado = @Primero;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaReasignarPaso', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaReasignarPaso AS RETURN 0;');
GO
/* Cambia el responsable de un paso Pendiente o EnCurso de un proceso EnCurso.
   @EstadoEsperado es el estado que vio la pantalla: si ya no es ese, nada.
   EnCurso: su actividad pasa al nuevo responsable con sus entregables tal
   cual (hechos incluidos); Cod_AsignadoPor no cambia (sigue siendo quien
   inicio el proceso) y quien reasigno queda en Usuario_Modificacion.
   Devuelve Resultado = 0, -1 el proceso o el paso cambio, -2 sin responsable. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaReasignarPaso
    @IdProcesoPaso  INT,
    @EstadoEsperado VARCHAR(20),
    @CodResponsable VARCHAR(20),
    @Usuario        VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @CodResponsable = LTRIM(RTRIM(ISNULL(@CodResponsable, '')));
    IF @CodResponsable = ''
    BEGIN
        SELECT Resultado = -2;
        RETURN;
    END

    DECLARE @EstadoPaso VARCHAR(20), @EstadoProceso VARCHAR(20);
    BEGIN TRANSACTION;
    EXEC dbo.Sp_RTA_AgendaPasoBloquear @IdProcesoPaso, @EstadoPaso OUTPUT, @EstadoProceso OUTPUT;

    IF @EstadoProceso IS NULL OR @EstadoProceso <> 'EnCurso'
       OR @EstadoPaso IS NULL OR @EstadoPaso <> ISNULL(@EstadoEsperado, '')
       OR @EstadoPaso NOT IN ('Pendiente', 'EnCurso')
    BEGIN
        COMMIT TRANSACTION;
        SELECT Resultado = -1;
        RETURN;
    END

    UPDATE dbo.AG_ProcesoPaso
       SET Cod_Responsable = @CodResponsable, Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdProcesoPaso = @IdProcesoPaso;

    IF @EstadoPaso = 'EnCurso'
        UPDATE dbo.AG_Actividad
           SET Cod_Responsable = @CodResponsable, Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
         WHERE IdProcesoPaso = @IdProcesoPaso;

    COMMIT TRANSACTION;
    SELECT Resultado = 0;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaCambiarPasoPendiente', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaCambiarPasoPendiente AS RETURN 0;');
GO
/* Titulo, plazo y entregables (uno por linea) de un paso Pendiente de un
   proceso EnCurso. Devuelve Resultado = 0, -1 el paso ya no esta Pendiente,
   -2 datos invalidos (titulo vacio o plazo fuera de 1..60). */
ALTER PROCEDURE dbo.Sp_RTA_AgendaCambiarPasoPendiente
    @IdProcesoPaso INT,
    @Titulo        NVARCHAR(200),
    @PlazoDias     INT,
    @Entregables   NVARCHAR(2000),
    @Usuario       VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @Titulo = LTRIM(RTRIM(ISNULL(@Titulo, N'')));
    IF @Titulo = N'' OR @PlazoDias IS NULL OR @PlazoDias NOT BETWEEN 1 AND 60
    BEGIN
        SELECT Resultado = -2;
        RETURN;
    END

    DECLARE @EstadoPaso VARCHAR(20), @EstadoProceso VARCHAR(20);
    BEGIN TRANSACTION;
    EXEC dbo.Sp_RTA_AgendaPasoBloquear @IdProcesoPaso, @EstadoPaso OUTPUT, @EstadoProceso OUTPUT;

    IF @EstadoProceso IS NULL OR @EstadoProceso <> 'EnCurso' OR @EstadoPaso IS NULL OR @EstadoPaso <> 'Pendiente'
    BEGIN
        COMMIT TRANSACTION;
        SELECT Resultado = -1;
        RETURN;
    END

    UPDATE dbo.AG_ProcesoPaso
       SET Titulo = @Titulo, PlazoDias = @PlazoDias,
           Entregables = NULLIF(LTRIM(RTRIM(ISNULL(@Entregables, N''))), N''),
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdProcesoPaso = @IdProcesoPaso;

    COMMIT TRANSACTION;
    SELECT Resultado = 0;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaCambiarFechaPaso', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaCambiarFechaPaso AS RETURN 0;');
GO
/* FechaEntrega de la actividad de un paso EnCurso. Que no sea sabado ni
   domingo lo valida C#, como en la Entrega 1. Devuelve Resultado = 0, -1 el
   paso ya no esta EnCurso, -2 sin fecha. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaCambiarFechaPaso
    @IdProcesoPaso INT,
    @Fecha         DATE,
    @Usuario       VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @Fecha IS NULL
    BEGIN
        SELECT Resultado = -2;
        RETURN;
    END

    DECLARE @EstadoPaso VARCHAR(20), @EstadoProceso VARCHAR(20);
    BEGIN TRANSACTION;
    EXEC dbo.Sp_RTA_AgendaPasoBloquear @IdProcesoPaso, @EstadoPaso OUTPUT, @EstadoProceso OUTPUT;

    IF @EstadoProceso IS NULL OR @EstadoProceso <> 'EnCurso' OR @EstadoPaso IS NULL OR @EstadoPaso <> 'EnCurso'
    BEGIN
        COMMIT TRANSACTION;
        SELECT Resultado = -1;
        RETURN;
    END

    UPDATE dbo.AG_Actividad
       SET FechaEntrega = @Fecha, Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdProcesoPaso = @IdProcesoPaso;

    COMMIT TRANSACTION;
    SELECT Resultado = 0;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaCancelarProceso', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaCancelarProceso AS RETURN 0;');
GO
/* Proceso EnCurso -> Cancelado con motivo; sus pasos no completos ->
   Cancelado; la actividad del paso en curso se anula (sus entregables
   marcados quedan como historia). Sin correo. Devuelve Resultado = 0, o -1
   si el proceso no esta EnCurso. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaCancelarProceso
    @IdProceso INT,
    @Motivo    NVARCHAR(400),
    @Usuario   VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @EstadoProceso VARCHAR(20), @Pasos INT;

    BEGIN TRANSACTION;
    SELECT @EstadoProceso = Estado
      FROM dbo.AG_Proceso WITH (UPDLOCK, HOLDLOCK)
     WHERE IdProceso = @IdProceso;
    SELECT @Pasos = COUNT(*)
      FROM dbo.AG_ProcesoPaso WITH (UPDLOCK, HOLDLOCK)
     WHERE IdProceso = @IdProceso;

    IF @EstadoProceso IS NULL OR @EstadoProceso <> 'EnCurso'
    BEGIN
        COMMIT TRANSACTION;
        SELECT Resultado = -1;
        RETURN;
    END

    UPDATE dbo.AG_Proceso
       SET Estado = 'Cancelado', FechaFin = GETDATE(),
           MotivoCancelacion = NULLIF(LTRIM(RTRIM(ISNULL(@Motivo, N''))), N''),
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdProceso = @IdProceso;

    UPDATE a
       SET Anulada = 1, Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
      FROM dbo.AG_Actividad AS a
      JOIN dbo.AG_ProcesoPaso AS pp ON pp.IdProcesoPaso = a.IdProcesoPaso
     WHERE pp.IdProceso = @IdProceso AND pp.Estado = 'EnCurso' AND a.Anulada = 0;

    UPDATE dbo.AG_ProcesoPaso
       SET Estado = 'Cancelado', Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdProceso = @IdProceso AND Estado IN ('Pendiente', 'EnCurso');

    COMMIT TRANSACTION;
    SELECT Resultado = 0;
END
GO

/* ------------------------- 5. procedimientos de la Entrega 1 que cambian -- */
/* Las no anuladas que vencen en el rango, mas las atrasadas de cualquier
   fecha (sin completar y vencidas antes de @Hoy). Dos resultados:
   actividades y entregables. Quien ve que lo decide CapaNegocio.
   Entrega 2: agrega IdProcesoPaso, IdProceso, NombreProceso y EstadoPaso
   (NULL en las actividades que no son de un proceso). Las filas son las
   mismas que antes. */
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
           a.CompletadaManual, a.FechaCompletada, a.Anulada,
           a.IdProcesoPaso, pp.IdProceso, NombreProceso = p.Nombre, EstadoPaso = pp.Estado
      FROM dbo.AG_Actividad AS a
      JOIN #Ids AS i ON i.IdActividad = a.IdActividad
      LEFT JOIN dbo.AG_ProcesoPaso AS pp ON pp.IdProcesoPaso = a.IdProcesoPaso
      LEFT JOIN dbo.AG_Proceso AS p ON p.IdProceso = pp.IdProceso
     ORDER BY a.FechaEntrega, a.IdActividad;

    SELECT e.IdEntregable, e.IdActividad, e.Descripcion, e.Orden, e.Hecho
      FROM dbo.AG_Entregable AS e
      JOIN #Ids AS i ON i.IdActividad = e.IdActividad
     ORDER BY e.IdActividad, e.Orden, e.IdEntregable;
END
GO

/* Una sola, aunque este anulada: el handler decide con ella si se puede tocar.
   Entrega 2: agrega IdProcesoPaso, IdProceso, NombreProceso y EstadoPaso. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaActividad
    @IdActividad INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.IdActividad, a.Titulo, a.Cod_Responsable, a.Cod_AsignadoPor, a.FechaEntrega,
           a.CompletadaManual, a.FechaCompletada, a.Anulada,
           a.IdProcesoPaso, pp.IdProceso, NombreProceso = p.Nombre, EstadoPaso = pp.Estado
      FROM dbo.AG_Actividad AS a
      LEFT JOIN dbo.AG_ProcesoPaso AS pp ON pp.IdProcesoPaso = a.IdProcesoPaso
      LEFT JOIN dbo.AG_Proceso AS p ON p.IdProceso = pp.IdProceso
     WHERE a.IdActividad = @IdActividad;

    SELECT e.IdEntregable, e.IdActividad, e.Descripcion, e.Orden, e.Hecho
      FROM dbo.AG_Entregable AS e
     WHERE e.IdActividad = @IdActividad
     ORDER BY e.Orden, e.IdEntregable;
END
GO

/* @IdActividad = 0 crea; si no, actualiza titulo y fecha (responsable y quien
   asigno no cambian). @Entregables es la lista COMPLETA:
   <es><e id="5" o="1">texto</e><e id="0" o="2">nuevo</e></es>
   Los que no vienen se borran, los que tienen id se actualizan (conservan
   Hecho) y los de id 0 se crean. Devuelve el IdActividad.
   Entrega 2: si la actividad existente es el paso de un proceso devuelve -9
   y no toca nada (titulo y entregables vienen del paso; quitar entregables
   no puede completar un paso por la puerta de atras). Sin proceso, igual que
   antes. */
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

    IF ISNULL(@IdActividad, 0) <> 0
       AND EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdActividad = @IdActividad AND IdProcesoPaso IS NOT NULL)
    BEGIN
        SELECT IdActividad = -9;
        RETURN;
    END

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

/* Entrega 2. Sin proceso: el mismo UPDATE y el mismo recalculo de antes, y
   una fila Respuestas = 0 con NULLs.
   Actividad de un paso: bloquea el proceso (Sp_RTA_AgendaPasoBloquear) y
     -9  el paso esta Completo o Cancelado: no se toca nada;
     -8  la marca completaria el paso, hay paso siguiente y
         @FechaEntregaSiguiente es NULL: no se toca nada;
      0  se marca; si la actividad quedo completa el paso pasa a Completo y se
         activa el siguiente (IdProcesoPasoActivado) o el proceso termina
         (IdProcesoTerminado). Todo en una transaccion: dos marcas a la vez
         se serializan en el bloqueo del proceso y la segunda ve el paso
         Completo (-9). */
ALTER PROCEDURE dbo.Sp_RTA_AgendaMarcarEntregable
    @IdEntregable          INT,
    @Hecho                 BIT,
    @Usuario               VARCHAR(50),
    @FechaEntregaSiguiente DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @IdActividad INT, @IdProcesoPaso INT, @EstadoPaso VARCHAR(20), @EstadoProceso VARCHAR(20),
            @Respuestas INT = 0, @IdProcesoPasoActivado INT = NULL, @IdProcesoTerminado INT = NULL;

    SELECT @IdActividad = e.IdActividad, @IdProcesoPaso = a.IdProcesoPaso
      FROM dbo.AG_Entregable AS e
      JOIN dbo.AG_Actividad AS a ON a.IdActividad = e.IdActividad
     WHERE e.IdEntregable = @IdEntregable;

    BEGIN TRANSACTION;

    IF @IdProcesoPaso IS NOT NULL
    BEGIN
        EXEC dbo.Sp_RTA_AgendaPasoBloquear @IdProcesoPaso, @EstadoPaso OUTPUT, @EstadoProceso OUTPUT;
        IF @EstadoPaso IN ('Completo', 'Cancelado')
            SET @Respuestas = -9;
        ELSE IF @EstadoPaso = 'EnCurso'
            AND @FechaEntregaSiguiente IS NULL
            AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso AS s
                          JOIN dbo.AG_ProcesoPaso AS c ON c.IdProceso = s.IdProceso AND s.Orden > c.Orden
                         WHERE c.IdProcesoPaso = @IdProcesoPaso)
            AND EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdActividad = @IdActividad AND Anulada = 0)
            AND NOT EXISTS (SELECT 1 FROM dbo.AG_Entregable
                             WHERE IdActividad = @IdActividad
                               AND CASE WHEN IdEntregable = @IdEntregable THEN @Hecho ELSE Hecho END = 0)
            SET @Respuestas = -8;
    END

    IF @Respuestas = 0
    BEGIN
        UPDATE dbo.AG_Entregable
           SET Hecho       = @Hecho,
               Cod_Hecho   = CASE WHEN @Hecho = 1 THEN @Usuario END,
               Fecha_Hecho = CASE WHEN @Hecho = 1 THEN GETDATE() END,
               Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
         WHERE IdEntregable = @IdEntregable;

        IF @IdActividad IS NOT NULL EXEC dbo.Sp_RTA_AgendaRecalcular @IdActividad;

        IF @EstadoPaso = 'EnCurso'
            EXEC dbo.Sp_RTA_AgendaPasoAvanzar @IdProcesoPaso, @FechaEntregaSiguiente, @Usuario,
                 @IdProcesoPasoActivado OUTPUT, @IdProcesoTerminado OUTPUT;
    END

    COMMIT TRANSACTION;
    SELECT Respuestas = @Respuestas, IdProcesoPasoActivado = @IdProcesoPasoActivado,
           IdProcesoTerminado = @IdProcesoTerminado;
END
GO

/* Solo para actividades sin entregables: con entregables, se completa
   marcandolos. El handler ya lo comprueba; esto es el respaldo.
   Entrega 2: mismos codigos y avance que Sp_RTA_AgendaMarcarEntregable. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaMarcarCompleta
    @IdActividad           INT,
    @Completa              BIT,
    @Usuario               VARCHAR(50),
    @FechaEntregaSiguiente DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @IdProcesoPaso INT, @EstadoPaso VARCHAR(20), @EstadoProceso VARCHAR(20),
            @Respuestas INT = 0, @IdProcesoPasoActivado INT = NULL, @IdProcesoTerminado INT = NULL;

    SELECT @IdProcesoPaso = IdProcesoPaso FROM dbo.AG_Actividad WHERE IdActividad = @IdActividad;

    BEGIN TRANSACTION;

    IF @IdProcesoPaso IS NOT NULL
    BEGIN
        EXEC dbo.Sp_RTA_AgendaPasoBloquear @IdProcesoPaso, @EstadoPaso OUTPUT, @EstadoProceso OUTPUT;
        IF @EstadoPaso IN ('Completo', 'Cancelado')
            SET @Respuestas = -9;
        ELSE IF @EstadoPaso = 'EnCurso'
            AND @FechaEntregaSiguiente IS NULL
            AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso AS s
                          JOIN dbo.AG_ProcesoPaso AS c ON c.IdProceso = s.IdProceso AND s.Orden > c.Orden
                         WHERE c.IdProcesoPaso = @IdProcesoPaso)
            AND EXISTS (SELECT 1 FROM dbo.AG_Actividad AS a
                         WHERE a.IdActividad = @IdActividad AND a.Anulada = 0
                           AND CASE WHEN EXISTS (SELECT 1 FROM dbo.AG_Entregable e WHERE e.IdActividad = a.IdActividad)
                                    THEN CASE WHEN NOT EXISTS (SELECT 1 FROM dbo.AG_Entregable e
                                                                WHERE e.IdActividad = a.IdActividad AND e.Hecho = 0)
                                              THEN 1 ELSE 0 END
                                    ELSE CONVERT(INT, @Completa) END = 1)
            SET @Respuestas = -8;
    END

    IF @Respuestas = 0
    BEGIN
        UPDATE a
           SET CompletadaManual = @Completa,
               Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
          FROM dbo.AG_Actividad AS a
         WHERE a.IdActividad = @IdActividad
           AND a.Anulada = 0
           AND NOT EXISTS (SELECT 1 FROM dbo.AG_Entregable e WHERE e.IdActividad = a.IdActividad);
        EXEC dbo.Sp_RTA_AgendaRecalcular @IdActividad;

        IF @EstadoPaso = 'EnCurso'
            EXEC dbo.Sp_RTA_AgendaPasoAvanzar @IdProcesoPaso, @FechaEntregaSiguiente, @Usuario,
                 @IdProcesoPasoActivado OUTPUT, @IdProcesoTerminado OUTPUT;
    END

    COMMIT TRANSACTION;
    SELECT Respuestas = @Respuestas, IdProcesoPasoActivado = @IdProcesoPasoActivado,
           IdProcesoTerminado = @IdProcesoTerminado;
END
GO

/* Se llama DESPUES de saber si el correo salio.
   Entrega 2: @IdProceso nuevo (correo "proceso terminado", sin actividad) e
   @IdActividad admite NULL; el CHECK de la tabla exige al menos uno. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaRegistrarCorreo
    @IdActividad  INT,
    @Destinatario VARCHAR(200),
    @Asunto       NVARCHAR(300),
    @Enviado      BIT,
    @Error        NVARCHAR(1000),
    @IdProceso    INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AG_CorreoBitacora (IdActividad, Destinatario, Asunto, Enviado, Error, IdProceso)
    VALUES (@IdActividad, @Destinatario, ISNULL(@Asunto, N''), @Enviado, LEFT(@Error, 1000), @IdProceso);
END
GO

/* ------------------------------------------- 6. marca de version --------- */
IF NOT EXISTS (SELECT 1 FROM sys.extended_properties
                WHERE class = 1 AND minor_id = 0 AND major_id = OBJECT_ID('dbo.AG_Actividad')
                  AND name = 'AG_VersionEsquema')
    EXEC sys.sp_addextendedproperty @name = N'AG_VersionEsquema', @value = 2,
         @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AG_Actividad';
ELSE
    EXEC sys.sp_updateextendedproperty @name = N'AG_VersionEsquema', @value = 2,
         @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AG_Actividad';
GO

/* Deshace el guard de la cabecera (si salto, la sesion quedaria en NOEXEC). */
SET NOEXEC OFF;
GO

/* ------------------------------------------------- veredicto -------------- */
/* Solo lectura. Sin GO entre el DECLARE de @Fallos y su ultimo uso. */
SET NOCOUNT ON;
DECLARE @Fallos INT = 0;

DECLARE @Faltan INT =
    (SELECT COUNT(*) FROM (VALUES
        ('dbo.AG_Plantilla', 'U'), ('dbo.AG_PlantillaPaso', 'U'), ('dbo.AG_Proceso', 'U'),
        ('dbo.AG_ProcesoPaso', 'U'),
        ('dbo.Sp_RTA_AgendaPasoBloquear', 'P'), ('dbo.Sp_RTA_AgendaActivarPaso', 'P'),
        ('dbo.Sp_RTA_AgendaPasoAvanzar', 'P'), ('dbo.Sp_RTA_AgendaFeriados', 'P'),
        ('dbo.Sp_RTA_AgendaPlantillas', 'P'), ('dbo.Sp_RTA_AgendaGuardarPlantilla', 'P'),
        ('dbo.Sp_RTA_AgendaProcesos', 'P'), ('dbo.Sp_RTA_AgendaIniciarProceso', 'P'),
        ('dbo.Sp_RTA_AgendaReasignarPaso', 'P'), ('dbo.Sp_RTA_AgendaCambiarPasoPendiente', 'P'),
        ('dbo.Sp_RTA_AgendaCambiarFechaPaso', 'P'), ('dbo.Sp_RTA_AgendaCancelarProceso', 'P'),
        ('dbo.FK_AG_Actividad_ProcesoPaso', 'F'), ('dbo.FK_AG_CorreoBitacora_Proceso', 'F'),
        ('dbo.CK_AG_CorreoBitacora_Origen', 'C'), ('dbo.CK_AG_Proceso_Estado', 'C'),
        ('dbo.CK_AG_ProcesoPaso_Estado', 'C'), ('dbo.CK_AG_ProcesoPaso_Plazo', 'C'),
        ('dbo.CK_AG_PlantillaPaso_Plazo', 'C'), ('dbo.UQ_AG_Plantilla_Nombre', 'UQ'),
        ('dbo.UQ_AG_PlantillaPaso_Orden', 'UQ'), ('dbo.UQ_AG_ProcesoPaso_Orden', 'UQ')
     ) AS o (Nombre, Tipo)
     WHERE OBJECT_ID(o.Nombre, o.Tipo) IS NULL);
IF @Faltan <> 0
BEGIN
    PRINT 'FALLO: faltan ' + CONVERT(VARCHAR(10), @Faltan) + ' tablas, procedimientos o restricciones.';
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.AG_Actividad') AND name = 'UX_AG_Actividad_ProcesoPaso'
                  AND is_unique = 1 AND has_filter = 1)
BEGIN
    PRINT 'FALLO: falta el indice unico filtrado UX_AG_Actividad_ProcesoPaso.';
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.AG_Actividad') AND name = 'IdProcesoPaso' AND is_nullable = 1)
   OR NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.AG_CorreoBitacora') AND name = 'IdActividad' AND is_nullable = 1)
   OR NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.AG_CorreoBitacora') AND name = 'IdProceso' AND is_nullable = 1)
BEGIN
    PRINT 'FALLO: columnas nuevas de AG_Actividad / AG_CorreoBitacora ausentes o NOT NULL.';
    SET @Fallos += 1;
END

/* Los procedimientos de la Entrega 1 tienen la version nueva. */
DECLARE @Viejos INT =
    (SELECT COUNT(*) FROM (VALUES
        ('Sp_RTA_AgendaActividades', 'IdProcesoPaso'), ('Sp_RTA_AgendaActividad', 'IdProcesoPaso'),
        ('Sp_RTA_AgendaGuardarActividad', 'IdProcesoPaso'),
        ('Sp_RTA_AgendaMarcarEntregable', '@FechaEntregaSiguiente'),
        ('Sp_RTA_AgendaMarcarCompleta', '@FechaEntregaSiguiente'),
        ('Sp_RTA_AgendaRegistrarCorreo', '@IdProceso')
     ) AS v (Nombre, Marca)
     WHERE CHARINDEX(v.Marca, ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.' + v.Nombre)), '')) = 0);
IF @Viejos <> 0
BEGIN
    PRINT 'FALLO: ' + CONVERT(VARCHAR(10), @Viejos) + ' procedimientos de la Entrega 1 siguen en la version anterior.';
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.parameters
                WHERE object_id = OBJECT_ID('dbo.Sp_RTA_AgendaMarcarEntregable') AND name = '@FechaEntregaSiguiente')
   OR NOT EXISTS (SELECT 1 FROM sys.parameters
                   WHERE object_id = OBJECT_ID('dbo.Sp_RTA_AgendaMarcarCompleta') AND name = '@FechaEntregaSiguiente')
   OR NOT EXISTS (SELECT 1 FROM sys.parameters
                   WHERE object_id = OBJECT_ID('dbo.Sp_RTA_AgendaRegistrarCorreo') AND name = '@IdProceso')
BEGIN
    PRINT 'FALLO: faltan parametros nuevos en Marcar* o RegistrarCorreo.';
    SET @Fallos += 1;
END

/* Un procedimiento de la agenda compilado con QUOTED_IDENTIFIER OFF no puede
   escribir en AG_Actividad (indice filtrado): error 1934 en produccion. */
IF EXISTS (SELECT 1 FROM sys.sql_modules
            WHERE OBJECT_NAME(object_id) LIKE 'Sp[_]RTA[_]Agenda%' AND uses_quoted_identifier = 0)
BEGIN
    PRINT 'FALLO: hay procedimientos Sp_RTA_Agenda* compilados con QUOTED_IDENTIFIER OFF.';
    SET @Fallos += 1;
END

DECLARE @Version INT =
    (SELECT TRY_CONVERT(INT, value) FROM sys.extended_properties
      WHERE class = 1 AND minor_id = 0 AND major_id = OBJECT_ID('dbo.AG_Actividad') AND name = 'AG_VersionEsquema');
IF ISNULL(@Version, 0) < 2
BEGIN
    PRINT 'FALLO: la marca AG_VersionEsquema no es 2 o mayor.';
    SET @Fallos += 1;
END

/* Integridad: ningun paso con dos actividades, ningun paso EnCurso sin
   actividad, ningun proceso EnCurso con dos pasos EnCurso. */
IF EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso AS pp
            WHERE pp.Estado = 'EnCurso'
              AND NOT EXISTS (SELECT 1 FROM dbo.AG_Actividad AS a WHERE a.IdProcesoPaso = pp.IdProcesoPaso))
   OR EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE Estado = 'EnCurso' GROUP BY IdProceso HAVING COUNT(*) > 1)
BEGIN
    PRINT 'FALLO: hay pasos EnCurso sin actividad o procesos con dos pasos EnCurso.';
    SET @Fallos += 1;
END

SELECT ObjetosFaltantes = @Faltan, ProcedimientosViejos = @Viejos, VersionEsquema = @Version, Fallos = @Fallos;
IF @Fallos = 0
    PRINT 'VEREDICTO: OK';
ELSE
    PRINT 'VEREDICTO: FALLA (' + CONVERT(VARCHAR(10), @Fallos) + ' comprobaciones). Revisar arriba.';
SELECT Veredicto = CASE WHEN @Fallos = 0 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA' END;
GO

/* ============================================================ REVERSA ======
   Devuelve los 6 procedimientos de la Entrega 1 a su version anterior. Es el
   cuerpo VIVO en produccion el 2026-10-23 (OBJECT_DEFINITION, identico al
   de 2026-10-15-agenda-esquema.sql salvo que la base lo guarda como CREATE).
   Antes de usarla: los binarios de la Entrega 1 tienen que estar publicados
   (el DAO nuevo lee la fila de Marcar*). Las tablas y columnas nuevas quedan:
   son NULL-ables y la version anterior las ignora. Ojo: tras la reversa,
   marcar la actividad de un paso ya no avanza el proceso.
   Despues de la reversa NO se quita nada mas: la marca AG_VersionEsquema y
   la columna AG_Actividad.IdProcesoPaso quedan. Por la columna, el guard de
   2026-10-15-agenda-esquema.sql seguira negandose a correr (aunque se
   borrara la marca), y esta bien: todo lo que ese script crea ya existe.
   Si hiciera falta volver a poner la version de la Entrega 1 de estos 6
   procedimientos, se usan los cuerpos de esta reversa, no aquel script. Para
   volver a la Entrega 2, se corre otra vez este script (con la marca en 2,
   el guard de la cabecera lo deja correr).
   Cada ALTER PROCEDURE tiene que ir en su propio lote: copiar uno por vez, o
   poner una linea GO entre ellos. Y ANTES del primero, en la misma sesion:
       SET QUOTED_IDENTIFIER ON;
       SET ANSI_NULLS ON;
       GO
   sqlcmd arranca con QUOTED_IDENTIFIER OFF; un procedimiento creado asi no
   puede escribir en AG_Actividad (por el indice filtrado de este script:
   error 1934) ni usar metodos XML. Probado en LocalDB: sin esas dos lineas
   la reversa deja GuardarActividad roto.

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
============================================================================ */
