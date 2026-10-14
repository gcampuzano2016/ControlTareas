# Permisos desde el portal — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** reemplazar las 13 listas de perfiles quemadas por permisos de pantalla (menú) y de acción que se administran desde «Menú por perfil» y «Menú por usuario».

**Architecture:**
- **Cuatro piezas:**
  - un procedimiento de solo lectura, `Sp_RTA_PermisosUsuario`, que devuelve las pantallas y acciones de una persona (perfil ∪ adicionales);
  - una regla pura en `CapaNegocio/NegPermisos`, con caché de 2 minutos decidida también ahí;
  - una fachada web, `ReporteTareas/clases/Permisos`, que es lo único que llaman las pantallas y los handlers;
  - tres tablas nuevas para las acciones.
- **El perfil 18 puede todo** y no consulta la base.
- **Los procedimientos de menú actuales no se tocan:** los siguen usando los binarios viejos mientras dura la publicación.

**Tech Stack:**
- ASP.NET WebForms en .NET 4.6.1 con C# 7.3. Las capas se compilan como `.exe`.
- MSTest (`CapaPruebas`), SQL Server 2008+, y jQuery 1.9.1 con ES5.

**Spec:** `docs/superpowers/specs/2026-10-14-permisos-desde-el-portal-design.md`

## Ruling sobre la spec (leer antes de la Tarea 6)

**Qué dice la spec:** en «Menú por perfil», el perfil 18 «no se edita».

**Por qué no puede quedar así:** el menú lateral del 18 sale de `PerfilMenu` (`Sp_RTA_ConsultarMenuPerfilUsuario`). Si se bloquea, nadie puede agregarle al 18 una pantalla nueva en su menú.

**Decisión:**
- Para el 18, las **pantallas** se editan como en cualquier otro perfil, porque deciden qué ve en el menú.
- Las **acciones** salen todas marcadas y deshabilitadas, con la nota «Super Admin: tiene todas las acciones; las pantallas que desmarque solo dejan de verse en su menú».
- `Sp_RTA_GuardarPermisosPerfil` ignora las acciones cuando el perfil es 18.

**Costo si esto es un error:** cambiar una línea de JS para bloquear también las pantallas del 18.

## Global Constraints

- **Compilar:**
  - solo con el MSBuild de VS2019, desde **PowerShell**: `& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo`;
  - se cuentan los errores, no los warnings.
- **Pruebas:** `vstest.console.exe` (está bajo `C:\Program Files (x86)\Microsoft Visual Studio\2019`) con `CapaPruebas\bin\Debug\CapaPruebas.dll /Platform:x86`. Hoy son 604, todas en verde.
- **C# 7.3:** sin `using var`, sin switch expressions, sin `??=`.
- **JS en ES5:** sin `let`, `const`, flechas ni template strings.
- **Proyectos:** son `.csproj` de estilo viejo. Cada archivo `.cs` nuevo necesita su `<Compile Include="..."/>` en el `.csproj` que corresponda, y cada `.aspx`, `.js` o `.ashx` nuevo su `<Content Include>`. Esta tarea no crea `.aspx` ni `.ashx` nuevos.
- **Codificación:**
  - los `.cs`, `.aspx` y `.js` **nuevos** van en UTF-8 **con BOM**;
  - al editar un archivo existente se conservan su BOM y sus finales de línea;
  - los `.sql` van en UTF-8 **sin BOM** y **sin tildes ni ñ**, también en los comentarios.
- **Repositorio público:** nada de nombres de personas, sueldos ni montos en código, comentarios o docs.
- **Producción:** **ningún subagente ejecuta SQL contra la base.** Los scripts se escriben y se revisan. Correrlos en `ReporTarea` lo hace el controlador **con autorización explícita del usuario**. `docs/sql/run-sql.sh` apunta a producción.
- **Texto de los handlers:** los mensajes al usuario en JSON llevan tildes y el handler fija `context.Response.ContentEncoding = Encoding.UTF8`.
- **Comparación de pantallas:** todo `Href` se compara **sin distinguir mayúsculas** y recortado.
- **Estados:** `PerfilMenu.Estado` usa `0` = activo (se compara `= 0`, igual que `Sp_RTA_ConsultarMenuPerfilUsuario`). `R_UsuarioMenu`, `RTA_PerfilAccion` y `RTA_UsuarioAccion` usan `'A'`/`'I'`.
- **Commits:** cada uno termina con `Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>`. Nunca se commitean `ReporteTareas/Web.config` ni `ReporteTareas/obj/Release/Package/PackageTmp/Web.config`.

## Review Focus

1. **Sesión reiniciada por `Login.aspx`.** `UserLogin` y `Cod_Usuario` quedan en `""` y `Id_Perfil` queda con el valor anterior. `Permisos` tiene que negar todo, **incluso para un `Id_Perfil` 18 que quedó colgado**: primero se revisa `Autenticado`, después el 18. Lo cubre la Tarea 3.
2. **El perfil 18 con la base caída.** Tiene que entrar igual, sin consultar. Lo cubre `PuedeEntrar_SuperAdmin_NoNecesitaPermisos` en la Tarea 2.
3. **Cambio de perfil con la sesión abierta.** Si a alguien le cambian el perfil y su sesión tiene otro `Id_Perfil`, la caché se recarga porque la identidad cambió. Lo cubre `DebeRecargar_CambiaElPerfil_Recarga` en la Tarea 2.
4. **Acción suelta.** Una fila de acción activa para un perfil que no tiene la pantalla no vale nada. Lo cubre `PuedeHacer_AccionSinSuPantalla_SeNiega` en la Tarea 2.
5. **JS viejo en la caché del navegador.** Si llama a la acción vieja `GuardarMenuUsuario`, tiene que seguir funcionando, pero solo para quien puede entrar a la pantalla. Lo cubre la Tarea 6: la acción vieja se conserva, detrás de la guarda nueva.

---

### Task 1: Esquema de permisos (SQL)

**Files:**
- Create: `docs/sql/2026-10-14-permisos-esquema.sql`

**Interfaces:**
- Produces (procedimientos que llaman las Tareas 2 y 6):
  - `Sp_RTA_PermisosUsuario @IdPerfil INT, @CodUsuario VARCHAR(50)` → resultado 1 `Href`; resultado 2 `Clave, Href`.
  - `Sp_RTA_ListarAccionesPerfil @IdPerfil INT` → `Id_Accion, Clave, Id_Menu, Titulo, Descripcion, Activo`.
  - `Sp_RTA_ListarAccionesUsuario @CodUsuario VARCHAR(50)` → `Id_Accion, Clave, Id_Menu, Titulo, Descripcion, ActivoPerfil, ActivoUsuario`.
  - `Sp_RTA_GuardarPermisosPerfil @IdPerfil INT, @MenusCsv VARCHAR(MAX), @AccionesCsv VARCHAR(MAX), @UsuarioRegistro VARCHAR(50)` → `Respuestas, Mensaje`.
  - `Sp_RTA_GuardarPermisosUsuario @CodUsuario VARCHAR(50), @ExtrasCsv VARCHAR(MAX), @AccionesCsv VARCHAR(MAX), @UsuarioRegistro VARCHAR(50)` → `Respuestas, Mensaje`.
  - Claves del catálogo: `HorasExtras.Reabrir`, `HorasExtras.EditarMaestro`, `Usuarios.CambiarPerfil`.

- [ ] **Step 1: Escribir el script completo**

```sql
/* ============================================================================
   Permisos desde el portal: esquema
   ReporTarea  |  2026-10-14
   Spec: docs/superpowers/specs/2026-10-14-permisos-desde-el-portal-design.md

   PENDIENTE DE EJECUTAR.

   Crea o actualiza:
     1. RTA_Accion, RTA_PerfilAccion, RTA_UsuarioAccion
     2. Catalogo inicial (3 acciones) y su siembra por perfil
     3. Sp_RTA_PermisosUsuario          (solo lectura: lo que puede una persona)
     4. Sp_RTA_ListarAccionesPerfil / Sp_RTA_ListarAccionesUsuario
     5. Sp_RTA_GuardarPermisosPerfil / Sp_RTA_GuardarPermisosUsuario

   NO modifica ningun procedimiento existente: Sp_RTA_GuardarMenuPerfil y
   Sp_RTA_GuardarMenuUsuario siguen sirviendo a los binarios viejos. Por eso se
   puede correr antes de publicar.

   Repetible. El perfil 18 (Super Admin) no lleva filas: el codigo le da todo.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------------------------------------------------------- 1. tablas ---- */
IF OBJECT_ID('dbo.RTA_Accion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RTA_Accion
    (
        Id_Accion   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RTA_Accion PRIMARY KEY,
        Clave       VARCHAR(60)  NOT NULL CONSTRAINT UQ_RTA_Accion_Clave UNIQUE,
        Id_Menu     INT          NOT NULL,
        Titulo      VARCHAR(100) NOT NULL,
        Descripcion VARCHAR(300) NULL,
        Estado      CHAR(1)      NOT NULL CONSTRAINT DF_RTA_Accion_Estado DEFAULT 'A'
    );
    PRINT 'RTA_Accion creada.';
END
GO

IF OBJECT_ID('dbo.RTA_PerfilAccion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RTA_PerfilAccion
    (
        IdPerfil         INT         NOT NULL,
        Id_Accion        INT         NOT NULL,
        Estado           CHAR(1)     NOT NULL,
        Usuario_Registro VARCHAR(50) NOT NULL,
        Fecha_Registro   DATETIME    NOT NULL CONSTRAINT DF_RTA_PerfilAccion_Fecha DEFAULT GETDATE(),
        CONSTRAINT UQ_RTA_PerfilAccion UNIQUE (IdPerfil, Id_Accion)
    );
    PRINT 'RTA_PerfilAccion creada.';
END
GO

IF OBJECT_ID('dbo.RTA_UsuarioAccion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RTA_UsuarioAccion
    (
        Cod_Usuario      VARCHAR(50) NOT NULL,
        Id_Accion        INT         NOT NULL,
        Estado           CHAR(1)     NOT NULL,
        Usuario_Registro VARCHAR(50) NOT NULL,
        Fecha_Registro   DATETIME    NOT NULL CONSTRAINT DF_RTA_UsuarioAccion_Fecha DEFAULT GETDATE(),
        CONSTRAINT UQ_RTA_UsuarioAccion UNIQUE (Cod_Usuario, Id_Accion)
    );
    PRINT 'RTA_UsuarioAccion creada.';
END
GO

/* -------------------------------------------- 2. catalogo y siembra ---- */
/* La pantalla de cada accion se resuelve por Href: el Id_Menu cambia entre
   ambientes. Si la hoja no existe, la accion no se crea y las aserciones
   del final lo reportan. */
DECLARE @Cat TABLE (Clave VARCHAR(60), Href VARCHAR(100), Titulo VARCHAR(100), Descripcion VARCHAR(300));
INSERT @Cat VALUES
 ('HorasExtras.Reabrir',       'HorasExtras.aspx',            'Reabrir periodo cerrado',            'Deshacer el cierre formal de un periodo de horas extras.'),
 ('HorasExtras.EditarMaestro', 'HorasExtras.aspx',            'Completar datos y ajustes de sueldo', 'Agregar personas al modulo con su sueldo y registrar ajustes de sueldo.'),
 ('Usuarios.CambiarPerfil',    'ParametrizacionUsuarios.aspx','Cambiar el perfil de un usuario',     'Asignar otro perfil a un usuario. El perfil decide casi todos sus accesos.');

INSERT INTO dbo.RTA_Accion (Clave, Id_Menu, Titulo, Descripcion, Estado)
SELECT c.Clave, m.Id_Menu, c.Titulo, c.Descripcion, 'A'
  FROM @Cat c
  JOIN dbo.MenuDos m ON m.Href = c.Href
 WHERE NOT EXISTS (SELECT 1 FROM dbo.RTA_Accion a WHERE a.Clave = c.Clave);

/* Siembra = las listas que hoy estan en el codigo, sin el 18:
   HorasExtras.EditarMaestro {14,18} -> perfil 14. Las otras dos eran {18}. */
INSERT INTO dbo.RTA_PerfilAccion (IdPerfil, Id_Accion, Estado, Usuario_Registro, Fecha_Registro)
SELECT 14, a.Id_Accion, 'A', 'MIGRACION', GETDATE()
  FROM dbo.RTA_Accion a
 WHERE a.Clave = 'HorasExtras.EditarMaestro'
   AND NOT EXISTS (SELECT 1 FROM dbo.RTA_PerfilAccion pa WHERE pa.IdPerfil = 14 AND pa.Id_Accion = a.Id_Accion);
GO

/* ------------------------------------------ 3. Sp_RTA_PermisosUsuario ---- */
IF OBJECT_ID('dbo.Sp_RTA_PermisosUsuario', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_PermisosUsuario AS RETURN 0;');
GO
/* Dos resultados, en este orden: pantallas (Href) y acciones (Clave, Href de
   su pantalla). Cod_Usuario se compara igual que en
   Sp_RTA_ConsultarMenuPerfilUsuario: lo que dibuja el menu y lo que autoriza
   el codigo tienen que coincidir. La regla (accion exige su pantalla, el 18
   puede todo) la aplica CapaNegocio.NegPermisos, no este procedimiento. */
ALTER PROCEDURE dbo.Sp_RTA_PermisosUsuario
    @IdPerfil   INT,
    @CodUsuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT Href = LTRIM(RTRIM(m.Href))
      FROM dbo.MenuDos m
     WHERE ISNULL(LTRIM(RTRIM(m.Href)), '') <> ''
       AND (EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
                     WHERE pm.id_Menu = m.Id_Menu AND pm.IdPerfil = @IdPerfil AND pm.Estado = 0)
            OR (@CodUsuario IS NOT NULL
                AND EXISTS (SELECT 1 FROM dbo.R_UsuarioMenu um
                             WHERE um.Id_Menu = m.Id_Menu AND um.Cod_Usuario = @CodUsuario AND um.Estado = 'A')));

    SELECT a.Clave, Href = LTRIM(RTRIM(m.Href))
      FROM dbo.RTA_Accion a
      JOIN dbo.MenuDos m ON m.Id_Menu = a.Id_Menu
     WHERE a.Estado = 'A'
       AND (EXISTS (SELECT 1 FROM dbo.RTA_PerfilAccion pa
                     WHERE pa.Id_Accion = a.Id_Accion AND pa.IdPerfil = @IdPerfil AND pa.Estado = 'A')
            OR (@CodUsuario IS NOT NULL
                AND EXISTS (SELECT 1 FROM dbo.RTA_UsuarioAccion ua
                             WHERE ua.Id_Accion = a.Id_Accion AND ua.Cod_Usuario = @CodUsuario AND ua.Estado = 'A')));
END
GO

/* ------------------------------------ 4. listas para la administracion ---- */
IF OBJECT_ID('dbo.Sp_RTA_ListarAccionesPerfil', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_ListarAccionesPerfil AS RETURN 0;');
GO
ALTER PROCEDURE dbo.Sp_RTA_ListarAccionesPerfil
    @IdPerfil INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.Id_Accion, a.Clave, a.Id_Menu, a.Titulo, Descripcion = ISNULL(a.Descripcion, ''),
           Activo = CASE WHEN pa.Id_Accion IS NOT NULL THEN 1 ELSE 0 END
      FROM dbo.RTA_Accion a
      LEFT JOIN dbo.RTA_PerfilAccion pa
        ON pa.Id_Accion = a.Id_Accion AND pa.IdPerfil = @IdPerfil AND pa.Estado = 'A'
     WHERE a.Estado = 'A'
     ORDER BY a.Id_Menu, a.Titulo;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_ListarAccionesUsuario', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_ListarAccionesUsuario AS RETURN 0;');
GO
/* Mismo criterio de perfil que Sp_RTA_ListarMenuUsuario: Cod_Usuario no es
   unico en R_Usuarios, se toma el mayor Id_Perfil. */
ALTER PROCEDURE dbo.Sp_RTA_ListarAccionesUsuario
    @CodUsuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @IdPerfil BIGINT;
    SELECT TOP 1 @IdPerfil = Id_Perfil FROM dbo.R_Usuarios WHERE Cod_Usuario = @CodUsuario ORDER BY Id_Perfil DESC;

    SELECT a.Id_Accion, a.Clave, a.Id_Menu, a.Titulo, Descripcion = ISNULL(a.Descripcion, ''),
           ActivoPerfil  = CASE WHEN pa.Id_Accion IS NOT NULL THEN 1 ELSE 0 END,
           ActivoUsuario = CASE WHEN ua.Id_Accion IS NOT NULL THEN 1 ELSE 0 END
      FROM dbo.RTA_Accion a
      LEFT JOIN dbo.RTA_PerfilAccion pa
        ON pa.Id_Accion = a.Id_Accion AND pa.IdPerfil = @IdPerfil AND pa.Estado = 'A'
      LEFT JOIN dbo.RTA_UsuarioAccion ua
        ON ua.Id_Accion = a.Id_Accion AND ua.Cod_Usuario = @CodUsuario AND ua.Estado = 'A'
     WHERE a.Estado = 'A'
     ORDER BY a.Id_Menu, a.Titulo;
END
GO

/* ----------------------------------------- 5a. Sp_RTA_GuardarPermisosPerfil */
IF OBJECT_ID('dbo.Sp_RTA_GuardarPermisosPerfil', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_GuardarPermisosPerfil AS RETURN 0;');
GO
/* La parte de menu es la de Sp_RTA_GuardarMenuPerfil (2026-08-11) sin cambios.
   Lo nuevo: las acciones, la pantalla de cada accion entra sola al set de
   menus, y todo va en UNA transaccion. Para el perfil 18 las acciones se
   ignoran: el codigo ya se las da todas. */
ALTER PROCEDURE dbo.Sp_RTA_GuardarPermisosPerfil
    @IdPerfil        INT,
    @MenusCsv        VARCHAR(MAX) = '',
    @AccionesCsv     VARCHAR(MAX) = '',
    @UsuarioRegistro VARCHAR(50)  = 'SISTEMA'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    IF ISNULL(@IdPerfil,0) = 0 OR NOT EXISTS (SELECT 1 FROM dbo.Perfiles WHERE IdPerfiles = @IdPerfil)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El perfil indicado no existe.'; RETURN;
    END

    DECLARE @Activos TABLE (IdMenu INT PRIMARY KEY);
    DECLARE @Acc TABLE (IdAccion INT PRIMARY KEY);
    DECLARE @xml XML;

    IF ISNULL(LTRIM(RTRIM(@MenusCsv)),'') <> ''
    BEGIN
        SET @xml = CAST('<i>' + REPLACE(@MenusCsv, ',', '</i><i>') + '</i>' AS XML);
        INSERT INTO @Activos (IdMenu)
        SELECT DISTINCT T.c.value('.', 'INT') FROM @xml.nodes('/i') AS T(c)
         WHERE ISNULL(T.c.value('.', 'VARCHAR(20)'),'') <> '';
    END

    IF @IdPerfil <> 18 AND ISNULL(LTRIM(RTRIM(@AccionesCsv)),'') <> ''
    BEGIN
        SET @xml = CAST('<i>' + REPLACE(@AccionesCsv, ',', '</i><i>') + '</i>' AS XML);
        INSERT INTO @Acc (IdAccion)
        SELECT DISTINCT T.c.value('.', 'INT') FROM @xml.nodes('/i') AS T(c)
         WHERE ISNULL(T.c.value('.', 'VARCHAR(20)'),'') <> '';
    END

    IF EXISTS (SELECT 1 FROM @Acc x WHERE NOT EXISTS (SELECT 1 FROM dbo.RTA_Accion a WHERE a.Id_Accion = x.IdAccion AND a.Estado = 'A'))
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Hay una accion que no existe en el catalogo. No se guardo nada.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        /* invariante accion => pantalla */
        INSERT INTO @Activos (IdMenu)
        SELECT DISTINCT a.Id_Menu FROM @Acc x JOIN dbo.RTA_Accion a ON a.Id_Accion = x.IdAccion
         WHERE a.Id_Menu NOT IN (SELECT IdMenu FROM @Activos);

        /* invariante hijo => padre (igual que Sp_RTA_GuardarMenuPerfil) */
        INSERT INTO @Activos (IdMenu)
        SELECT DISTINCT m.Id_MenuPadre FROM @Activos a JOIN dbo.MenuDos m ON m.Id_Menu = a.IdMenu
         WHERE ISNULL(m.Id_MenuPadre,0) <> 0 AND m.Id_MenuPadre NOT IN (SELECT IdMenu FROM @Activos);

        MERGE dbo.PerfilMenu AS tgt
        USING (
            SELECT m.Id_Menu, EsActivo = CASE WHEN a.IdMenu IS NOT NULL THEN 1 ELSE 0 END
              FROM dbo.MenuDos m LEFT JOIN @Activos a ON a.IdMenu = m.Id_Menu
        ) AS src
        ON tgt.IdPerfil = @IdPerfil AND tgt.id_Menu = src.Id_Menu
        WHEN MATCHED THEN
            UPDATE SET tgt.Estado = CASE WHEN src.EsActivo = 1 THEN 0 ELSE 1 END
        WHEN NOT MATCHED BY TARGET AND src.EsActivo = 1 THEN
            INSERT (IdPerfil, id_Menu, Estado) VALUES (@IdPerfil, src.Id_Menu, 0);

        IF @IdPerfil <> 18
        BEGIN
            MERGE dbo.RTA_PerfilAccion AS tgt
            USING (
                SELECT a.Id_Accion, EsActivo = CASE WHEN x.IdAccion IS NOT NULL THEN 1 ELSE 0 END
                  FROM dbo.RTA_Accion a LEFT JOIN @Acc x ON x.IdAccion = a.Id_Accion
            ) AS src
            ON tgt.IdPerfil = @IdPerfil AND tgt.Id_Accion = src.Id_Accion
            WHEN MATCHED AND tgt.Estado <> CASE WHEN src.EsActivo = 1 THEN 'A' ELSE 'I' END THEN
                UPDATE SET tgt.Estado = CASE WHEN src.EsActivo = 1 THEN 'A' ELSE 'I' END,
                           tgt.Usuario_Registro = @UsuarioRegistro, tgt.Fecha_Registro = GETDATE()
            WHEN NOT MATCHED BY TARGET AND src.EsActivo = 1 THEN
                INSERT (IdPerfil, Id_Accion, Estado, Usuario_Registro, Fecha_Registro)
                VALUES (@IdPerfil, src.Id_Accion, 'A', @UsuarioRegistro, GETDATE());
        END

        SET @Respuestas = 1;
        SET @Mensaje = 'Permisos del perfil guardados correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar los permisos del perfil: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO

/* ---------------------------------------- 5b. Sp_RTA_GuardarPermisosUsuario */
IF OBJECT_ID('dbo.Sp_RTA_GuardarPermisosUsuario', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_GuardarPermisosUsuario AS RETURN 0;');
GO
/* La parte de menu es la de Sp_RTA_GuardarMenuUsuario (2026-08-03) sin
   cambios. Lo nuevo: las acciones extra (las que ya da el perfil se
   descartan), la pantalla de cada accion entra sola a los extras salvo que la
   de el perfil, y todo va en UNA transaccion. */
ALTER PROCEDURE dbo.Sp_RTA_GuardarPermisosUsuario
    @CodUsuario      VARCHAR(50),
    @ExtrasCsv       VARCHAR(MAX) = '',
    @AccionesCsv     VARCHAR(MAX) = '',
    @UsuarioRegistro VARCHAR(50)  = 'SISTEMA'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    IF ISNULL(LTRIM(RTRIM(@CodUsuario)),'') = ''
       OR NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios WHERE Cod_Usuario = @CodUsuario)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El usuario indicado no existe.'; RETURN;
    END

    DECLARE @IdPerfil BIGINT;
    SELECT TOP 1 @IdPerfil = Id_Perfil FROM dbo.R_Usuarios WHERE Cod_Usuario = @CodUsuario ORDER BY Id_Perfil DESC;

    DECLARE @Extras TABLE (IdMenu INT PRIMARY KEY);
    DECLARE @Acc TABLE (IdAccion INT PRIMARY KEY);
    DECLARE @xml XML;

    IF ISNULL(LTRIM(RTRIM(@ExtrasCsv)),'') <> ''
    BEGIN
        SET @xml = CAST('<i>' + REPLACE(@ExtrasCsv, ',', '</i><i>') + '</i>' AS XML);
        INSERT INTO @Extras (IdMenu)
        SELECT DISTINCT T.c.value('.', 'INT') FROM @xml.nodes('/i') AS T(c)
         WHERE ISNULL(T.c.value('.', 'VARCHAR(20)'),'') <> '';
    END

    IF ISNULL(LTRIM(RTRIM(@AccionesCsv)),'') <> ''
    BEGIN
        SET @xml = CAST('<i>' + REPLACE(@AccionesCsv, ',', '</i><i>') + '</i>' AS XML);
        INSERT INTO @Acc (IdAccion)
        SELECT DISTINCT T.c.value('.', 'INT') FROM @xml.nodes('/i') AS T(c)
         WHERE ISNULL(T.c.value('.', 'VARCHAR(20)'),'') <> '';
    END

    IF EXISTS (SELECT 1 FROM @Acc x WHERE NOT EXISTS (SELECT 1 FROM dbo.RTA_Accion a WHERE a.Id_Accion = x.IdAccion AND a.Estado = 'A'))
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Hay una accion que no existe en el catalogo. No se guardo nada.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM @Extras WHERE IdMenu NOT IN (SELECT Id_Menu FROM dbo.MenuDos);

        /* solo excepciones reales: lo que ya da el perfil no se guarda como extra */
        DELETE x FROM @Acc x
         WHERE EXISTS (SELECT 1 FROM dbo.RTA_PerfilAccion pa
                        WHERE pa.IdPerfil = @IdPerfil AND pa.Id_Accion = x.IdAccion AND pa.Estado = 'A');

        /* invariante accion => pantalla, salvo que la pantalla ya la de el perfil */
        INSERT INTO @Extras (IdMenu)
        SELECT DISTINCT a.Id_Menu FROM @Acc x JOIN dbo.RTA_Accion a ON a.Id_Accion = x.IdAccion
         WHERE a.Id_Menu NOT IN (SELECT IdMenu FROM @Extras)
           AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
                            WHERE pm.IdPerfil = @IdPerfil AND pm.id_Menu = a.Id_Menu AND pm.Estado = 0);

        /* invariante hijo => padre (igual que Sp_RTA_GuardarMenuUsuario) */
        INSERT INTO @Extras (IdMenu)
        SELECT DISTINCT m.Id_MenuPadre FROM @Extras a JOIN dbo.MenuDos m ON m.Id_Menu = a.IdMenu
         WHERE ISNULL(m.Id_MenuPadre,0) <> 0
           AND m.Id_MenuPadre NOT IN (SELECT IdMenu FROM @Extras)
           AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
                            WHERE pm.IdPerfil = @IdPerfil AND pm.id_Menu = m.Id_MenuPadre AND pm.Estado = 0);

        MERGE dbo.R_UsuarioMenu AS tgt
        USING (
            SELECT IdMenu, EsActivo = 1 FROM @Extras
            UNION
            SELECT um.Id_Menu, EsActivo = 0 FROM dbo.R_UsuarioMenu um
             WHERE um.Cod_Usuario = @CodUsuario AND um.Id_Menu NOT IN (SELECT IdMenu FROM @Extras)
        ) AS src
        ON tgt.Cod_Usuario = @CodUsuario AND tgt.Id_Menu = src.IdMenu
        WHEN MATCHED AND tgt.Estado <> CASE WHEN src.EsActivo = 1 THEN 'A' ELSE 'I' END THEN
            UPDATE SET tgt.Estado = CASE WHEN src.EsActivo = 1 THEN 'A' ELSE 'I' END,
                       tgt.Usuario_Registro = @UsuarioRegistro, tgt.Fecha_Registro = GETDATE()
        WHEN NOT MATCHED BY TARGET AND src.EsActivo = 1 THEN
            INSERT (Cod_Usuario, Id_Menu, Estado, Usuario_Registro, Fecha_Registro)
            VALUES (@CodUsuario, src.IdMenu, 'A', @UsuarioRegistro, GETDATE());

        MERGE dbo.RTA_UsuarioAccion AS tgt
        USING (
            SELECT IdAccion, EsActivo = 1 FROM @Acc
            UNION
            SELECT ua.Id_Accion, EsActivo = 0 FROM dbo.RTA_UsuarioAccion ua
             WHERE ua.Cod_Usuario = @CodUsuario AND ua.Id_Accion NOT IN (SELECT IdAccion FROM @Acc)
        ) AS src
        ON tgt.Cod_Usuario = @CodUsuario AND tgt.Id_Accion = src.IdAccion
        WHEN MATCHED AND tgt.Estado <> CASE WHEN src.EsActivo = 1 THEN 'A' ELSE 'I' END THEN
            UPDATE SET tgt.Estado = CASE WHEN src.EsActivo = 1 THEN 'A' ELSE 'I' END,
                       tgt.Usuario_Registro = @UsuarioRegistro, tgt.Fecha_Registro = GETDATE()
        WHEN NOT MATCHED BY TARGET AND src.EsActivo = 1 THEN
            INSERT (Cod_Usuario, Id_Accion, Estado, Usuario_Registro, Fecha_Registro)
            VALUES (@CodUsuario, src.IdAccion, 'A', @UsuarioRegistro, GETDATE());

        SET @Respuestas = 1;
        SET @Mensaje = 'Permisos del usuario guardados correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar los permisos del usuario: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO

/* ------------------------------------------------------ 6. aserciones ---- */
DECLARE @Fallos INT = 0;

IF OBJECT_ID('dbo.RTA_Accion','U') IS NULL OR OBJECT_ID('dbo.RTA_PerfilAccion','U') IS NULL OR OBJECT_ID('dbo.RTA_UsuarioAccion','U') IS NULL
BEGIN RAISERROR('FALLO: falta una de las tres tablas de acciones.', 16, 1); SET @Fallos += 1; END

IF (SELECT COUNT(*) FROM dbo.RTA_Accion WHERE Clave IN ('HorasExtras.Reabrir','HorasExtras.EditarMaestro','Usuarios.CambiarPerfil') AND Estado = 'A') <> 3
BEGIN RAISERROR('FALLO: el catalogo no tiene las 3 acciones (falta una hoja de menu?).', 16, 1); SET @Fallos += 1; END

IF NOT EXISTS (SELECT 1 FROM dbo.RTA_PerfilAccion pa JOIN dbo.RTA_Accion a ON a.Id_Accion = pa.Id_Accion
                WHERE a.Clave = 'HorasExtras.EditarMaestro' AND pa.IdPerfil = 14 AND pa.Estado = 'A')
BEGIN RAISERROR('FALLO: el perfil 14 no tiene HorasExtras.EditarMaestro.', 16, 1); SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM dbo.RTA_PerfilAccion WHERE IdPerfil = 18)
BEGIN RAISERROR('FALLO: el perfil 18 no debe tener filas de accion.', 16, 1); SET @Fallos += 1; END

IF OBJECT_ID('dbo.Sp_RTA_PermisosUsuario','P') IS NULL OR OBJECT_ID('dbo.Sp_RTA_ListarAccionesPerfil','P') IS NULL
   OR OBJECT_ID('dbo.Sp_RTA_ListarAccionesUsuario','P') IS NULL OR OBJECT_ID('dbo.Sp_RTA_GuardarPermisosPerfil','P') IS NULL
   OR OBJECT_ID('dbo.Sp_RTA_GuardarPermisosUsuario','P') IS NULL
BEGIN RAISERROR('FALLO: falta uno de los cinco procedimientos.', 16, 1); SET @Fallos += 1; END

IF @Fallos = 0
    PRINT 'VEREDICTO: OK - esquema de permisos listo en la base.';
ELSE
    RAISERROR('VEREDICTO: FALLO - %d comprobaciones fallaron.', 16, 1, @Fallos);
GO
```

- [ ] **Step 2: Revisar codificación**

Run (bash): `head -c3 docs/sql/2026-10-14-permisos-esquema.sql | od -c | head -1; LC_ALL=C grep -n '[^ -~	]' docs/sql/2026-10-14-permisos-esquema.sql`

Expected:
- la primera línea **no** empieza con `357 273 277` (sin BOM);
- el `grep` no imprime nada (sin caracteres fuera de ASCII).

- [ ] **Step 3: Commit**

```bash
git add docs/sql/2026-10-14-permisos-esquema.sql
git commit -m "feat(permisos): esquema de acciones y procedimientos de permisos" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

> **Controlador:** este script **no** se corre en esta tarea. Se corre en la Tarea 9, con autorización del usuario.

---

### Task 2: Regla pura y acceso a datos (`NegPermisos`, `DaoPermisos`)

**Files:**
- Create: `CapaEntidad/EntPermisos.cs` (+ `<Compile Include="EntPermisos.cs" />` en `CapaEntidad/CapaEntidad.csproj`)
- Create: `CapaDato/DaoPermisos.cs` (+ Compile en `CapaDato/CapaDato.csproj`)
- Create: `CapaNegocio/NegPermisos.cs` y `CapaNegocio/NegPermisos.Datos.cs` (+ Compile en `CapaNegocio/CapaNegocio.csproj`)
- Test: `CapaPruebas/NegPermisosTests.cs` (+ Compile en `CapaPruebas/CapaPruebas.csproj`)

**Interfaces:**
- Consumes: `Sp_RTA_PermisosUsuario` (Tarea 1).
- Produces:
  - `CapaEntidad.EntPermisos`, con `HashSet<string> Pantallas` y `Dictionary<string,string> Acciones` (clave → href), los dos sin distinguir mayúsculas.
  - `CapaEntidad.EntPermisosCache`, con `int? IdPerfil`, `string CodUsuario`, `DateTime Cargado` y `EntPermisos Permisos`.
  - `NegPermisos.PerfilSuperAdmin` (`const int = 18`) y `NegPermisos.Vigencia` (`TimeSpan`, 2 min).
  - `bool NegPermisos.PuedeEntrar(int? idPerfil, EntPermisos permisos, string href)`.
  - `bool NegPermisos.PuedeHacer(int? idPerfil, EntPermisos permisos, string clave)`.
  - `bool NegPermisos.DebeRecargar(EntPermisosCache cache, int? idPerfil, string codUsuario, DateTime ahora)`.
  - `EntPermisos NegPermisos.Cargar(int? idPerfil, string codUsuario)`, que llama a `DaoPermisos.Cargar(int idPerfil, string codUsuario)`.

- [ ] **Step 1: Escribir las pruebas que fallan**

`CapaPruebas/NegPermisosTests.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CapaPruebas
{
    [TestClass]
    public class NegPermisosTests
    {
        private static EntPermisos Con(string[] pantallas, params string[] accionesYPantalla)
        {
            EntPermisos p = new EntPermisos();
            foreach (string h in pantallas) { p.Pantallas.Add(h); }
            for (int i = 0; i + 1 < accionesYPantalla.Length; i += 2)
            {
                p.Acciones[accionesYPantalla[i]] = accionesYPantalla[i + 1];
            }
            return p;
        }

        [TestMethod]
        public void PuedeEntrar_SuperAdmin_NoNecesitaPermisos()
        {
            Assert.IsTrue(NegPermisos.PuedeEntrar(18, null, "TableroCostos.aspx"));
        }

        [TestMethod]
        public void PuedeEntrar_PantallaEnSusPermisos_Entra()
        {
            Assert.IsTrue(NegPermisos.PuedeEntrar(2, Con(new[] { "HorasExtras.aspx" }), "HorasExtras.aspx"));
        }

        [TestMethod]
        public void PuedeEntrar_PantallaQueNoTiene_NoEntra()
        {
            Assert.IsFalse(NegPermisos.PuedeEntrar(2, Con(new[] { "HorasExtras.aspx" }), "TableroCostos.aspx"));
        }

        [TestMethod]
        public void PuedeEntrar_MayusculasYEspacios_NoImportan()
        {
            Assert.IsTrue(NegPermisos.PuedeEntrar(2, Con(new[] { "HorasExtras.aspx" }), "  horasextras.ASPX "));
        }

        [TestMethod]
        public void PuedeEntrar_SinPermisosCargados_NoEntra()
        {
            Assert.IsFalse(NegPermisos.PuedeEntrar(2, null, "HorasExtras.aspx"));
            Assert.IsFalse(NegPermisos.PuedeEntrar(null, null, "HorasExtras.aspx"));
        }

        [TestMethod]
        public void PuedeEntrar_HrefVacio_NoEntra()
        {
            Assert.IsFalse(NegPermisos.PuedeEntrar(2, Con(new[] { "HorasExtras.aspx" }), ""));
            Assert.IsFalse(NegPermisos.PuedeEntrar(2, Con(new[] { "HorasExtras.aspx" }), null));
        }

        [TestMethod]
        public void PuedeHacer_SuperAdmin_NoNecesitaPermisos()
        {
            Assert.IsTrue(NegPermisos.PuedeHacer(18, null, "HorasExtras.Reabrir"));
        }

        [TestMethod]
        public void PuedeHacer_AccionConSuPantalla_Puede()
        {
            EntPermisos p = Con(new[] { "HorasExtras.aspx" }, "HorasExtras.EditarMaestro", "HorasExtras.aspx");
            Assert.IsTrue(NegPermisos.PuedeHacer(14, p, "HorasExtras.EditarMaestro"));
        }

        [TestMethod]
        public void PuedeHacer_AccionSinSuPantalla_SeNiega()
        {
            EntPermisos p = Con(new string[0], "HorasExtras.EditarMaestro", "HorasExtras.aspx");
            Assert.IsFalse(NegPermisos.PuedeHacer(14, p, "HorasExtras.EditarMaestro"));
        }

        [TestMethod]
        public void PuedeHacer_AccionQueNoTiene_SeNiega()
        {
            EntPermisos p = Con(new[] { "HorasExtras.aspx" }, "HorasExtras.EditarMaestro", "HorasExtras.aspx");
            Assert.IsFalse(NegPermisos.PuedeHacer(14, p, "HorasExtras.Reabrir"));
        }

        [TestMethod]
        public void PuedeHacer_ClaveDesconocidaOVacia_SeNiega()
        {
            EntPermisos p = Con(new[] { "HorasExtras.aspx" }, "HorasExtras.EditarMaestro", "HorasExtras.aspx");
            Assert.IsFalse(NegPermisos.PuedeHacer(14, p, "No.Existe"));
            Assert.IsFalse(NegPermisos.PuedeHacer(14, p, ""));
            Assert.IsFalse(NegPermisos.PuedeHacer(14, p, null));
            Assert.IsFalse(NegPermisos.PuedeHacer(14, null, "HorasExtras.EditarMaestro"));
        }

        [TestMethod]
        public void DebeRecargar_SinCache_Recarga()
        {
            Assert.IsTrue(NegPermisos.DebeRecargar(null, 2, "U1", DateTime.Now));
        }

        [TestMethod]
        public void DebeRecargar_MismaIdentidadDentroDeLaVigencia_NoRecarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = new EntPermisos() };
            Assert.IsFalse(NegPermisos.DebeRecargar(c, 2, "U1", t.AddSeconds(119)));
        }

        [TestMethod]
        public void DebeRecargar_PasaronDosMinutos_Recarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = new EntPermisos() };
            Assert.IsTrue(NegPermisos.DebeRecargar(c, 2, "U1", t.AddMinutes(2)));
        }

        [TestMethod]
        public void DebeRecargar_CambiaElPerfil_Recarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = new EntPermisos() };
            Assert.IsTrue(NegPermisos.DebeRecargar(c, 14, "U1", t.AddSeconds(5)));
        }

        [TestMethod]
        public void DebeRecargar_CambiaElUsuario_Recarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = new EntPermisos() };
            Assert.IsTrue(NegPermisos.DebeRecargar(c, 2, "U2", t.AddSeconds(5)));
        }

        [TestMethod]
        public void DebeRecargar_RelojHaciaAtras_Recarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = new EntPermisos() };
            Assert.IsTrue(NegPermisos.DebeRecargar(c, 2, "U1", t.AddMinutes(-1)));
        }

        [TestMethod]
        public void DebeRecargar_CacheSinPermisos_Recarga()
        {
            DateTime t = new DateTime(2026, 10, 14, 10, 0, 0);
            EntPermisosCache c = new EntPermisosCache { IdPerfil = 2, CodUsuario = "U1", Cargado = t, Permisos = null };
            Assert.IsTrue(NegPermisos.DebeRecargar(c, 2, "U1", t.AddSeconds(5)));
        }
    }
}
```

- [ ] **Step 2: Correr y verificar que no compila**

Compilar (comando de Global Constraints). Expected: errores CS0246 / CS0103 por `EntPermisos` y `NegPermisos` inexistentes.

- [ ] **Step 3: Implementar**

`CapaEntidad/EntPermisos.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Lo que puede una persona: pantallas (Href) y acciones (Clave -> Href de
    /// su pantalla). Sale de Sp_RTA_PermisosUsuario. Ninguno de los dos
    /// distingue mayusculas: MenuDos.Href no tiene un formato unico.
    /// [Serializable] porque vive en Session.
    /// </summary>
    [Serializable]
    public class EntPermisos
    {
        public HashSet<string> Pantallas { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> Acciones { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Los permisos de la sesion, con la identidad y la hora con que se cargaron.</summary>
    [Serializable]
    public class EntPermisosCache
    {
        public int? IdPerfil { get; set; }
        public string CodUsuario { get; set; } = "";
        public DateTime Cargado { get; set; }
        public EntPermisos Permisos { get; set; }
    }
}
```

`CapaDato/DaoPermisos.cs`:

```csharp
using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace CapaDato
{
    /// <summary>
    /// Sp_RTA_PermisosUsuario devuelve dos resultados: pantallas y acciones. Si
    /// falta el segundo se lanza una excepcion: un "sin acciones" silencioso
    /// le quitaria a alguien un permiso sin ningun error visible.
    /// </summary>
    public class DaoPermisos
    {
        public static EntPermisos Cargar(int idPerfil, string codUsuario)
        {
            EntPermisos p = new EntPermisos();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PermisosUsuario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = idPerfil;
                cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 50).Value = (object)codUsuario ?? DBNull.Value;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string href = Convert.ToString(dr["Href"], CultureInfo.InvariantCulture).Trim();
                        if (href != "") { p.Pantallas.Add(href); }
                    }
                    if (!dr.NextResult())
                    {
                        throw new InvalidOperationException("Sp_RTA_PermisosUsuario no devolvió las acciones.");
                    }
                    while (dr.Read())
                    {
                        string clave = Convert.ToString(dr["Clave"], CultureInfo.InvariantCulture).Trim();
                        string href = Convert.ToString(dr["Href"], CultureInfo.InvariantCulture).Trim();
                        if (clave != "") { p.Acciones[clave] = href; }
                    }
                }
            }
            return p;
        }
    }
}
```

`CapaNegocio/NegPermisos.cs`:

```csharp
using CapaEntidad;
using System;

namespace CapaNegocio
{
    /// <summary>
    /// La regla de permisos del portal. Clase pura en este archivo: no toca
    /// base, sesion ni HttpContext. El unico archivo que llama a CapaDato es
    /// NegPermisos.Datos.cs.
    ///
    /// - Entrar a una pantalla: perfil 18, o el Href esta en sus permisos
    ///   (PerfilMenu de su perfil mas R_UsuarioMenu).
    /// - Hacer una accion: perfil 18, o puede entrar a la pantalla de la accion
    ///   Y tiene la accion (RTA_PerfilAccion mas RTA_UsuarioAccion).
    /// - Sin permisos cargados, se niega.
    ///
    /// El 18 es la unica regla fija del codigo, a proposito: es el respaldo para
    /// que nadie quede sin poder administrar permisos.
    /// </summary>
    public static partial class NegPermisos
    {
        public const int PerfilSuperAdmin = 18;

        /// <summary>Cuanto vale la copia de la sesion antes de volver a la base.</summary>
        public static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(2);

        public static bool PuedeEntrar(int? idPerfil, EntPermisos permisos, string href)
        {
            if (idPerfil == PerfilSuperAdmin) { return true; }
            string h = (href ?? "").Trim();
            return h != "" && permisos != null && permisos.Pantallas.Contains(h);
        }

        public static bool PuedeHacer(int? idPerfil, EntPermisos permisos, string clave)
        {
            if (idPerfil == PerfilSuperAdmin) { return true; }
            string c = (clave ?? "").Trim();
            if (c == "" || permisos == null) { return false; }
            string pantalla;
            return permisos.Acciones.TryGetValue(c, out pantalla) && PuedeEntrar(idPerfil, permisos, pantalla);
        }

        /// <summary>
        /// Si hay que volver a la base: no hay copia, la copia no tiene permisos,
        /// cambio la identidad, vencio, o el reloj retrocedio.
        /// </summary>
        public static bool DebeRecargar(EntPermisosCache cache, int? idPerfil, string codUsuario, DateTime ahora)
        {
            if (cache == null || cache.Permisos == null) { return true; }
            if (cache.IdPerfil != idPerfil) { return true; }
            if (!string.Equals((cache.CodUsuario ?? "").Trim(), (codUsuario ?? "").Trim(), StringComparison.OrdinalIgnoreCase)) { return true; }
            if (ahora < cache.Cargado) { return true; }
            return ahora - cache.Cargado >= Vigencia;
        }
    }
}
```

`CapaNegocio/NegPermisos.Datos.cs`:

```csharp
using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    /// <summary>Unico archivo de NegPermisos que toca la base.</summary>
    public static partial class NegPermisos
    {
        /// <summary>Lanza si la base falla: quien llama decide negar.</summary>
        public static EntPermisos Cargar(int? idPerfil, string codUsuario)
        {
            return DaoPermisos.Cargar(idPerfil ?? 0, codUsuario);
        }
    }
}
```

Agregar los cuatro `<Compile Include>` (y el de la prueba) junto a las entradas vecinas de cada `.csproj`.

- [ ] **Step 4: Compilar y correr las pruebas**

Compilar y luego: `vstest.console.exe CapaPruebas\bin\Debug\CapaPruebas.dll /Platform:x86 /Tests:NegPermisosTests`

Expected: las 18 pasan, y la suite completa queda en 622 en verde.

- [ ] **Step 5: Commit**

```bash
git add CapaEntidad/EntPermisos.cs CapaEntidad/CapaEntidad.csproj CapaDato/DaoPermisos.cs CapaDato/CapaDato.csproj CapaNegocio/NegPermisos.cs CapaNegocio/NegPermisos.Datos.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegPermisosTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(permisos): regla de pantallas y acciones, con cache de dos minutos" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 3: Fachada web `Permisos` y nombres (`Pantallas`, `Acciones`)

**Files:**
- Create: `ReporteTareas/clases/Permisos.cs` (+ `<Compile Include="clases\Permisos.cs" />` en `ReporteTareas/ReporteTareas.csproj`, junto a `clases\PerfilIdentidad.cs`)

**Interfaces:**
- Consumes: `NegPermisos` y `EntPermisosCache` (Tarea 2).
- Produces (namespace `ReporteTareas.clases`, todo `internal`):
  - `Permisos.Autenticado(HttpContext)`, `Permisos.PuedeEntrar(HttpContext, string href)` y `Permisos.PuedeHacer(HttpContext, string clave)`.
  - Constantes en `Pantallas`: `TableroCostos`, `HorasExtras`, `ParametrizacionHorasExtras`, `CurvaColaboradores`, `PerfilesPersonal`, `ParametrizacionPerfiles`, `ParametrizacionUsuarios`, `ParametrizacionMenuPerfil`, `ParametrizacionMenuUsuario`.
  - Constantes en `Acciones`: `HorasExtrasReabrir`, `HorasExtrasEditarMaestro`, `UsuariosCambiarPerfil`.

- [ ] **Step 1: Escribir el archivo**

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Globalization;
using System.Web;

namespace ReporteTareas.clases
{
    /// <summary>
    /// Lo unico que llaman pantallas y handlers para decidir acceso. No decide
    /// nada: saca la identidad de la sesion, mantiene la copia de permisos y le
    /// pregunta a NegPermisos, que es donde estan la regla y las pruebas.
    /// Si aparece un numero de perfil en este archivo, esta en el lugar
    /// equivocado.
    ///
    /// Orden de las comprobaciones (Review Focus 1): primero la sesion
    /// completa. Login.aspx deja UserLogin y Cod_Usuario en "" sin tocar
    /// Id_Perfil, asi que un 18 colgado de una sesion cerrada no debe pasar.
    ///
    /// Ante cualquier error al cargar: log y se niega.
    /// </summary>
    internal static class Permisos
    {
        private const string ClaveSesion = "Permisos.Cache";

        internal static bool Autenticado(HttpContext context)
        {
            return context != null && context.Session != null
                && !string.IsNullOrWhiteSpace(Convert.ToString(context.Session["UserLogin"], CultureInfo.InvariantCulture))
                && !string.IsNullOrWhiteSpace(Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture));
        }

        internal static bool PuedeEntrar(HttpContext context, string href)
        {
            if (!Autenticado(context)) { return false; }
            int? perfil = Perfil(context);
            if (perfil == NegPermisos.PerfilSuperAdmin) { return true; }
            return NegPermisos.PuedeEntrar(perfil, Obtener(context, perfil), href);
        }

        internal static bool PuedeHacer(HttpContext context, string clave)
        {
            if (!Autenticado(context)) { return false; }
            int? perfil = Perfil(context);
            if (perfil == NegPermisos.PerfilSuperAdmin) { return true; }
            return NegPermisos.PuedeHacer(perfil, Obtener(context, perfil), clave);
        }

        private static int? Perfil(HttpContext context)
        {
            int id;
            return int.TryParse(Convert.ToString(context.Session["Id_Perfil"], CultureInfo.InvariantCulture), out id) ? id : (int?)null;
        }

        private static EntPermisos Obtener(HttpContext context, int? perfil)
        {
            string cod = Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture);
            try
            {
                EntPermisosCache cache = context.Session[ClaveSesion] as EntPermisosCache;
                DateTime ahora = DateTime.Now;
                if (NegPermisos.DebeRecargar(cache, perfil, cod, ahora))
                {
                    cache = new EntPermisosCache
                    {
                        IdPerfil = perfil,
                        CodUsuario = cod,
                        Cargado = ahora,
                        Permisos = NegPermisos.Cargar(perfil, cod)
                    };
                    context.Session[ClaveSesion] = cache;
                }
                return cache.Permisos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Permisos: no se pudieron cargar. " + ex);
                context.Session.Remove(ClaveSesion);
                return null;
            }
        }
    }

    /// <summary>Href de las pantallas protegidas, tal como estan en MenuDos.</summary>
    internal static class Pantallas
    {
        internal const string TableroCostos = "TableroCostos.aspx";
        internal const string HorasExtras = "HorasExtras.aspx";
        internal const string ParametrizacionHorasExtras = "ParametrizacionHorasExtras.aspx";
        /// <summary>Da acceso a TODO el modulo Curva (handler y sus 6 pantallas).</summary>
        internal const string CurvaColaboradores = "CurvaColaboradores.aspx";
        internal const string PerfilesPersonal = "PerfilesPersonal.aspx";
        internal const string ParametrizacionPerfiles = "ParametrizacionPerfiles.aspx";
        internal const string ParametrizacionUsuarios = "ParametrizacionUsuarios.aspx";
        internal const string ParametrizacionMenuPerfil = "ParametrizacionMenuPerfil.aspx";
        internal const string ParametrizacionMenuUsuario = "ParametrizacionMenuUsuario.aspx";
    }

    /// <summary>Claves de RTA_Accion (docs/sql/2026-10-14-permisos-esquema.sql).</summary>
    internal static class Acciones
    {
        internal const string HorasExtrasReabrir = "HorasExtras.Reabrir";
        internal const string HorasExtrasEditarMaestro = "HorasExtras.EditarMaestro";
        internal const string UsuariosCambiarPerfil = "Usuarios.CambiarPerfil";
    }
}
```

- [ ] **Step 2: Compilar**

Expected: EXIT 0. Todavía nadie usa la clase.

- [ ] **Step 3: Commit**

```bash
git add ReporteTareas/clases/Permisos.cs ReporteTareas/ReporteTareas.csproj
git commit -m "feat(permisos): fachada web Permisos y nombres de pantallas y acciones" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 4: Migrar Tablero de Costos, Horas Extras, Parámetros de Horas Extras y Curva

**Files:**
- Modify: `ReporteTareas/Formulario/AdministrarTableroCostos.ashx.cs` (`Autorizado`)
- Modify: `CapaNegocio/NegTableroCostos.cs` (quitar `PerfilesAutorizados` y `PuedeVer(int?, Func<bool>)` con su comentario)
- Modify: `CapaNegocio/NegTableroCostos.Datos.cs` (quitar `PuedeVer(int?, string)`)
- Modify: `CapaDato/DaoTableroCostos.cs` (quitar `AccesoPorUsuario`)
- Modify: `CapaPruebas/NegTableroCostosTests.cs` (quitar el bloque `// ---- acceso` y sus 6 pruebas `PuedeVer_*`, con `NoDebeConsultar`)
- Modify: `ReporteTareas/Formulario/TableroCostos.aspx.cs` (solo el comentario, si nombra a `PuedeVer`)
- Modify: `ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs`, `DescargarHorasExtras.ashx.cs`, `HorasExtras.aspx.cs`
- Modify: `ReporteTareas/Formulario/AdministrarParametrosHE.ashx.cs`, `ParametrizacionHorasExtras.aspx.cs`
- Modify: `ReporteTareas/Formulario/AdministrarCurvaAprendizaje.ashx.cs`, `CurvaAcceso.cs` (solo el comentario)

**Interfaces:**
- Consumes: `Permisos`, `Pantallas` y `Acciones` (Tarea 3). Agregar `using ReporteTareas.clases;` en cada archivo que los use.

- [ ] **Step 1: Tablero**

En `AdministrarTableroCostos.ashx.cs`, el cuerpo de `Autorizado` queda así:

```csharp
        internal static bool Autorizado(HttpContext context)
        {
            return Autenticado(context) && Permisos.PuedeEntrar(context, Pantallas.TableroCostos);
        }
```

- Actualizar el comentario de la clase: la regla está en `ReporteTareas.clases.Permisos` (pantalla `TableroCostos.aspx`, perfil o adicional).
- Borrar lo que quedó sin uso en `NegTableroCostos`, `DaoTableroCostos` y las pruebas, según la lista de archivos.
- Mantener `Autenticado`, porque sigue decidiendo el 401.

- [ ] **Step 2: Horas Extras**

En `AdministrarHorasExtras.ashx.cs`:
- La guarda de `ProcessRequest`, `else if (!EstaEn(context, PerfilesAutorizados))`, pasa a `else if (!Permisos.PuedeEntrar(context, Pantallas.HorasExtras))`.
- La línea 320, `if (!EstaEn(context, PerfilesQueReabren))`, pasa a `if (!Permisos.PuedeHacer(context, Acciones.HorasExtrasReabrir))`.
- Las líneas 345, 373 y 409, `if (!EstaEn(context, PerfilesQueEditanMaestro))`, pasan a `if (!Permisos.PuedeHacer(context, Acciones.HorasExtrasEditarMaestro))`.
- Borrar `PerfilesAutorizados`, `PerfilesQueReabren`, `PerfilesQueEditanMaestro` y `EstaEn`, con sus comentarios.
- Reescribir el comentario de la clase (líneas ~40-58): las barreras siguen **aquí**, pero quién tiene cada permiso se administra en el portal.

En `DescargarHorasExtras.ashx.cs`, el cuerpo de `TienePermiso` pasa a `return Permisos.PuedeEntrar(context, Pantallas.HorasExtras);`. Actualizar su comentario.

En `HorasExtras.aspx.cs`:

```csharp
            if (!Permisos.PuedeEntrar(Context, Pantallas.HorasExtras))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
```

- `PuedeReabrir` pasa a `get { return Permisos.PuedeHacer(Context, Acciones.HorasExtrasReabrir); }`.
- `PuedeEditarMaestro` pasa a `get { return Permisos.PuedeHacer(Context, Acciones.HorasExtrasEditarMaestro); }`.
- Reemplazar el comentario que hablaba de la lista única: ahora la regla vive en `Permisos`.

- [ ] **Step 3: Parámetros de Horas Extras**

En `AdministrarParametrosHE.ashx.cs`:
- la guarda pasa a `else if (!Permisos.PuedeEntrar(context, Pantallas.ParametrizacionHorasExtras))`;
- borrar `PerfilesAutorizados` y `EstaEn` (si no queda otro uso) con su comentario.

En `ParametrizacionHorasExtras.aspx.cs`, la redirección pasa a `if (!Permisos.PuedeEntrar(Context, Pantallas.ParametrizacionHorasExtras))`.

- [ ] **Step 4: Curva**

En `AdministrarCurvaAprendizaje.ashx.cs`:

```csharp
        /// <summary>
        /// El modulo entero (handler, descarga y sus pantallas) depende de la
        /// pantalla CurvaColaboradores.aspx: quien la tiene en su menu, por
        /// perfil o como adicional, usa todo el modulo. Las otras dos opciones
        /// del menu de Curva solas no dan acceso.
        /// </summary>
        internal static bool Autorizado(HttpContext context)
        {
            return Permisos.PuedeEntrar(context, Pantallas.CurvaColaboradores);
        }
```

- Borrar `PerfilesAutorizados`.
- `Permisos.PuedeEntrar` ya exige sesión completa, más estricta que el `UserLogin == null` anterior. Esto es correcto: el `ProcessRequest` responde 401 antes por su propia rama de sesión, si la tiene.
- Verificar que la rama de sesión de `ProcessRequest` (alrededor de la línea 60) siga antes de `Autorizado`.
- Actualizar el comentario de `CurvaAcceso.cs`.

- [ ] **Step 5: Comprobar que no quedan listas**

Run: `grep -rnE "PerfilesAutorizados|PerfilesQue|EstaEn\(" --include=*.cs ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs ReporteTareas/Formulario/DescargarHorasExtras.ashx.cs ReporteTareas/Formulario/HorasExtras.aspx.cs ReporteTareas/Formulario/AdministrarParametrosHE.ashx.cs ReporteTareas/Formulario/ParametrizacionHorasExtras.aspx.cs ReporteTareas/Formulario/AdministrarCurvaAprendizaje.ashx.cs CapaNegocio/NegTableroCostos.cs`

Expected: ninguna línea de código (puede quedar texto en comentarios que expliquen la historia; revisarlo).

- [ ] **Step 6: Compilar y correr la suite**

Expected:
- EXIT 0;
- la suite pasa de 622 a 616: se quitan las 6 `PuedeVer_*` del Tablero;
- todas en verde.

- [ ] **Step 7: Commit**

```bash
git add -u CapaNegocio CapaDato CapaPruebas ReporteTareas/Formulario
git commit -m "refactor(permisos): Tablero, Horas Extras, sus parametros y Curva preguntan a Permisos" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 5: Migrar Perfil del colaborador, Administración de perfiles y de usuarios

**Files:**
- Modify: `CapaNegocio/NegPerfilAcceso.cs`
- Modify: `CapaPruebas/NegPerfilAccesoTests.cs`
- Modify: `ReporteTareas/clases/PerfilIdentidad.cs`
- Modify: `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` (línea ~1000, `PerfilIdentidad.EsRRHH`)
- Modify: `ReporteTareas/Formulario/PerfilesPersonal.aspx.cs`
- Modify: `ReporteTareas/Formulario/AdministrarPerfiles.ashx.cs`, `ParametrizacionPerfiles.aspx.cs`
- Modify: `ReporteTareas/Formulario/AdministrarUsuarios.ashx.cs`, `ParametrizacionUsuarios.aspx.cs`

**Interfaces:**
- Produces: `NegPerfilAcceso.Objetivo(string codSesion, bool veAjenos, string codPedido)` y `PerfilIdentidad.VeAjenos(HttpContext)`.
- Se eliminan `NegPerfilAcceso.PerfilesRRHH`, `NegPerfilAcceso.EsRRHH` y `PerfilIdentidad.EsRRHH`.

- [ ] **Step 1: Cambiar las pruebas primero**

En `CapaPruebas/NegPerfilAccesoTests.cs`:
- Borrar las constantes `Rrhh`, `Admin` y `Comun` (líneas 21-23).
- En cada llamada, `Objetivo(Yo, Comun, …)` pasa a `Objetivo(Yo, false, …)` y `Objetivo(Yo, Rrhh, …)` pasa a `Objetivo(Yo, true, …)`.
- Borrar la prueba `Objetivo_PerfilSuperAdmin_PuedePedirUnoAjeno`, que queda igual a la de Talento Humano.
- Borrar el bloque `EsRRHH` (líneas ~150-161).
- Renombrar la prueba de la línea ~53, si su nombre dice «Rrhh», a `Objetivo_QuienVeAjenos_PuedePedirUnoAjeno`.

Compilar. Expected: error CS1503, no se puede convertir `bool` a `string`.

- [ ] **Step 2: `NegPerfilAcceso`**

- Borrar `PerfilesRRHH` y `EsRRHH`.
- Cambiar la firma a `public static EntPerfilObjetivo Objetivo(string codSesion, bool veAjenos, string codPedido)`.
- La rama `if (EsRRHH(idPerfilSesion))` pasa a `if (veAjenos)`.
- En el comentario de la clase, explicar que quién ve perfiles ajenos se decide afuera, con el permiso de la pantalla `PerfilesPersonal.aspx` (portal), y llega como `veAjenos`.

- [ ] **Step 3: `PerfilIdentidad`**

```csharp
        internal static EntPerfilObjetivo Objetivo(HttpContext context, string codPedido)
        {
            return NegPerfilAcceso.Objetivo(Autor(context), VeAjenos(context), codPedido);
        }

        /// <summary>
        /// Si la sesion puede ver y corregir perfiles ajenos: quien tiene la
        /// pantalla PerfilesPersonal.aspx (por perfil o como adicional).
        /// </summary>
        internal static bool VeAjenos(HttpContext context)
        {
            return Permisos.PuedeEntrar(context, Pantallas.PerfilesPersonal);
        }
```

- Borrar `EsRRHH`.
- En `AdministrarPerfil.ashx.cs`, `PerfilIdentidad.EsRRHH(context)` pasa a `PerfilIdentidad.VeAjenos(context)`.
- En `PerfilesPersonal.aspx.cs`, la redirección pasa a `if (!Permisos.PuedeEntrar(Context, Pantallas.PerfilesPersonal))`. Agregar los `using` necesarios.

- [ ] **Step 4: Administración de perfiles**

En `AdministrarPerfiles.ashx.cs`:
- el cuerpo de `TienePermisoDeAdministracion` pasa a `return Permisos.PuedeEntrar(context, Pantallas.ParametrizacionPerfiles);`;
- borrar `PerfilesAutorizados`.

En `ParametrizacionPerfiles.aspx.cs`, después de `GenLogin.RedireccionarALogin(this);`:

```csharp
            if (!Permisos.PuedeEntrar(Context, Pantallas.ParametrizacionPerfiles))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
```

- [ ] **Step 5: Administración de usuarios**

En `AdministrarUsuarios.ashx.cs`:
- La guarda pasa a `else if (!Permisos.PuedeEntrar(context, Pantallas.ParametrizacionUsuarios))`.
- `EsSuperAdmin(context)` se reemplaza en sus dos usos (líneas ~309 y ~334) por `Permisos.PuedeHacer(context, Acciones.UsuariosCambiarPerfil)`.
- Borrar `PerfilesQueAdministranUsuarios`, `PerfilesQueCambianPerfil`, `EstaEn` y `EsSuperAdmin`.
- Reescribir el comentario «OJO con el acoplamiento»: ya no hay dos fuentes, el menú decide.

En `ParametrizacionUsuarios.aspx.cs`:
- agregar la misma redirección del Step 4 con `Pantallas.ParametrizacionUsuarios`;
- `PuedeCambiarPerfil` pasa a `get { return Permisos.PuedeHacer(Context, Acciones.UsuariosCambiarPerfil); }`.

- [ ] **Step 6: Comprobar, compilar y probar**

Run: `grep -rnE "PerfilesRRHH|EsRRHH|PerfilesQue|PerfilesAutorizados|idPerfil == 18" --include=*.cs ReporteTareas CapaNegocio | grep -v "/obj/\|/bin/"`

Expected: nada.

Compilar y correr la suite. Expected:
- EXIT 0;
- 614 en verde: 616 menos `Objetivo_PerfilSuperAdmin_PuedePedirUnoAjeno` y menos `EsRRHH_CatorceYDieciocho_SonLosDosUnicos`.

- [ ] **Step 7: Commit**

```bash
git add -u CapaNegocio CapaPruebas ReporteTareas
git commit -m "refactor(permisos): perfiles ajenos, administracion de perfiles y de usuarios preguntan a Permisos" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 6: Proteger y extender los handlers de administración del menú

**Files:**
- Create: `CapaEntidad/EntAccionPermiso.cs` (+ Compile)
- Modify: `CapaDato/DaoMenuPerfil.cs`, `CapaDato/DaoMenuUsuario.cs`
- Modify: `CapaNegocio/NegMenuPerfil.cs`, `CapaNegocio/NegMenuUsuario.cs`
- Modify: `ReporteTareas/Formulario/AdministrarMenuPerfil.ashx.cs`, `AdministrarMenuUsuario.ashx.cs`
- Modify: `ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx.cs`, `ParametrizacionMenuUsuario.aspx.cs`

**Interfaces:**
- Consumes: `Sp_RTA_ListarAccionesPerfil`, `Sp_RTA_ListarAccionesUsuario`, `Sp_RTA_GuardarPermisosPerfil` y `Sp_RTA_GuardarPermisosUsuario` (Tarea 1); `Permisos` y `Pantallas` (Tarea 3).
- Produces:
  - **Acciones JSON nuevas** que usa la Tarea 7:
    - Perfil: `ListaAccionesPerfil {idPerfil}` → `[EntAccionPermiso]`, y `GuardarPermisosPerfil {idPerfil, activos:[int], acciones:[int]}` → `EntRespuesta`.
    - Usuario: `ListaAccionesUsuario {codUsuario}` → `[EntAccionPermiso]`, y `GuardarPermisosUsuario {codUsuario, extras:[int], acciones:[int]}` → `EntRespuesta`.
  - `EntAccionPermiso` tiene `int Id_Accion`, `string Clave`, `int Id_Menu`, `string Titulo`, `string Descripcion`, `int Activo`, `int ActivoPerfil` y `int ActivoUsuario`. En la lista por perfil, `ActivoPerfil` y `ActivoUsuario` valen 0. En la lista por usuario, `Activo` vale 0.

- [ ] **Step 1: Entidad y acceso a datos**

`CapaEntidad/EntAccionPermiso.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>Una accion del catalogo RTA_Accion con su estado para un perfil o un usuario.</summary>
    public class EntAccionPermiso
    {
        public int Id_Accion { get; set; }
        public string Clave { get; set; } = "";
        public int Id_Menu { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int Activo { get; set; }
        public int ActivoPerfil { get; set; }
        public int ActivoUsuario { get; set; }
    }
}
```

En `DaoMenuPerfil`, agregar:
- `public static List<EntAccionPermiso> ListarAccionesPerfil(int idPerfil)`, que lee `Id_Accion, Clave, Id_Menu, Titulo, Descripcion, Activo` de `Sp_RTA_ListarAccionesPerfil`;
- `public static EntRespuesta GuardarPermisosPerfil(int idPerfil, string menusCsv, string accionesCsv, string usuarioRegistro)`, que llama a `Sp_RTA_GuardarPermisosPerfil` (`@IdPerfil INT`, `@MenusCsv VARCHAR(-1)`, `@AccionesCsv VARCHAR(-1)`, `@UsuarioRegistro VARCHAR(50)`).

En `GuardarPermisosPerfil` se copia **exactamente** el patrón de lectura de `Respuestas`/`Mensaje` y el `catch` de `GuardarMenuPerfil` del mismo archivo, con el mensaje «Ocurrió un error al guardar los permisos del perfil. Detalle: ».

En `DaoMenuUsuario`, lo mismo:
- `ListarAccionesUsuario(string codUsuario)`, que lee `ActivoPerfil` y `ActivoUsuario`;
- `GuardarPermisosUsuario(string codUsuario, string extrasCsv, string accionesCsv, string usuarioRegistro)`.

En `NegMenuPerfil` y `NegMenuUsuario`, métodos de paso con el mismo nombre.

- [ ] **Step 2: Guardas de los dos handlers**

En `AdministrarMenuPerfil.ashx.cs`:
- La clase pasa a `public class AdministrarMenuPerfil : IHttpHandler, System.Web.SessionState.IRequiresSessionState`. Agregar `using ReporteTareas.clases;`.
- Al comienzo de `ProcessRequest`, antes del `if (ContentType…)`, y convirtiendo la cadena en `if / else if / else if`:

```csharp
            if (!Permisos.Autenticado(context))
            {
                responseAction.Append(responseMessage("0", "Su sesión ha expirado. Vuelva a iniciar sesión.", "danger"));
            }
            else if (!Permisos.PuedeEntrar(context, Pantallas.ParametrizacionMenuPerfil))
            {
                responseAction.Append(responseMessage("0", "No tiene permisos para esta pantalla.", "danger"));
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
```

En `AdministrarMenuUsuario.ashx.cs`:
- la rama de sesión existente pasa a `if (!Permisos.Autenticado(context))`;
- se agrega la misma rama `else if (!Permisos.PuedeEntrar(context, Pantallas.ParametrizacionMenuUsuario))`.

Las acciones viejas (`GuardarMenuPerfil` y `GuardarMenuUsuario`) **se conservan**, ya detrás de la guarda: el JS viejo en caché las sigue usando (Review Focus 5).

En las dos `.aspx.cs`, después de `GenLogin.RedireccionarALogin(this);`, agregar la redirección a Principal con `Permisos.PuedeEntrar(Context, Pantallas.ParametrizacionMenuPerfil)` y `Pantallas.ParametrizacionMenuUsuario` respectivamente.

- [ ] **Step 3: Acciones nuevas**

En `AdministrarMenuPerfil.ashx.cs`, registrar en el despacho:

```csharp
                if (Action == "ListaAccionesPerfil")
                {
                    existAction = true;
                    responseAction.Append(ListaAccionesPerfil(parameters));
                }

                if (Action == "GuardarPermisosPerfil")
                {
                    existAction = true;
                    responseAction.Append(GuardarPermisosPerfil(context, parameters));
                }
```

Métodos:
- **`ListaAccionesPerfil`:** valida `idPerfil` igual que `ListaMenuPerfil` y devuelve `ToJson(NegMenuPerfil.ListarAccionesPerfil(idPerfil))`.
- **`GuardarPermisosPerfil(HttpContext context, dynamic campos)`:**
  - valida `idPerfil` igual que `GuardarMenuPerfil`;
  - arma el CSV de `activos` con la **misma validación estricta** que `AdministrarMenuUsuario.GuardarMenuUsuario`: cualquier valor no entero o ≤ 0 → `responseMessage("0", "El listado de módulos contiene un valor inválido.", "warning")`;
  - hace lo mismo con `acciones`, con el mensaje «El listado de acciones contiene un valor inválido.»;
  - toma `usuarioRegistro` de `context.Session["Cod_Usuario"]`;
  - llama a `NegMenuPerfil.GuardarPermisosPerfil(...)`.
- **Una lista que no llega** (`campos` no contiene la clave) se trata como vacía.

En `AdministrarMenuUsuario.ashx.cs`, lo mismo con `ListaAccionesUsuario {codUsuario}` y `GuardarPermisosUsuario {codUsuario, extras, acciones}`. La validación de `codUsuario` se copia de `GuardarMenuUsuario` del mismo archivo.

- [ ] **Step 4: Compilar y correr la suite**

Expected: EXIT 0 y 614 en verde.

- [ ] **Step 5: Commit**

```bash
git add CapaEntidad/EntAccionPermiso.cs CapaEntidad/CapaEntidad.csproj CapaDato/DaoMenuPerfil.cs CapaDato/DaoMenuUsuario.cs CapaNegocio/NegMenuPerfil.cs CapaNegocio/NegMenuUsuario.cs ReporteTareas/Formulario/AdministrarMenuPerfil.ashx.cs ReporteTareas/Formulario/AdministrarMenuUsuario.ashx.cs ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx.cs ReporteTareas/Formulario/ParametrizacionMenuUsuario.aspx.cs
git commit -m "feat(permisos): las pantallas de menu exigen permiso y administran acciones" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 7: Árbol con acciones en las dos pantallas

**Files:**
- Modify: `ReporteTareas/js/parametrizacionMenuPerfil.js`, `ReporteTareas/js/parametrizacionMenuUsuario.js`
- Modify: `ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx` (`?v=1` → `?v=2`), `ReporteTareas/Formulario/ParametrizacionMenuUsuario.aspx` (subir su `?v=` en 1)

**Interfaces:**
- Consumes: las acciones JSON de la Tarea 6.

- [ ] **Step 1: `parametrizacionMenuPerfil.js`**

- **Estado:** agregar `var _acciones = [];`.
- **`BuscarMenus`:** después de recibir `_menus`, pide `ListaAccionesPerfil` con el mismo `idPerfil`, guarda `_acciones` y recién entonces llama a `RenderArbol(_menus)`. Si la segunda respuesta trae `estado`, muestra el mensaje y pinta el árbol sin acciones.
- **`RenderArbol`:** después de la `<label>` de cada padre y de cada hijo, agrega `info += PintarAcciones(item.Id_Menu);`.
- **Perfil 18:** si `$("#cmbPerfil").val() === "18"`, antes del árbol pinta `<div class='alert alert-info'>Super Admin: tiene todas las acciones; las pantallas que desmarque solo dejan de verse en su menú.</div>`.

```javascript
/* Acciones de una pantalla, debajo de ella. Son casillas aparte (chkAccion):
   nunca viajan como menus. */
function PintarAcciones(idMenu) {
    var html = "";
    var esSuper = ($("#cmbPerfil").val() === "18");
    $.each(_acciones, function (i, a) {
        if (a.Id_Menu != idMenu) { return; }
        var marcado = (esSuper || a.Activo == 1) ? "checked" : "";
        var bloqueado = esSuper ? "disabled" : "";
        html += "<div style='margin-left:56px'>";
        html += "<label style='font-weight:normal' title='" + EscaparAttr(a.Descripcion) + "'>";
        html += "<input type='checkbox' class='chkAccion' data-accion='" + EscaparAttr(String(a.Id_Accion)) + "'";
        html += " data-menu='" + EscaparAttr(String(a.Id_Menu)) + "' " + marcado + " " + bloqueado;
        html += " onchange='OnAccionChange(" + a.Id_Menu + ", this)' /> ";
        html += "<i class='fa fa-cog' style='color:#777'></i> " + Escapar(a.Titulo);
        html += "</label></div>";
    });
    return html;
}

/* Marcar una accion marca su pantalla (y por OnHijoChange, el padre). */
function OnAccionChange(idMenu, chk) {
    if (!$(chk).is(":checked")) { return; }
    var $pantalla = $(".chkMenu[data-menu='" + idMenu + "']");
    $pantalla.prop("checked", true);
    var padre = $pantalla.attr("data-padre");
    if (padre) { OnHijoChange(padre); }
}

/* Desmarcar una pantalla desmarca sus acciones editables y avisa una vez. */
function DesmarcarAccionesDe(idMenu) {
    var $acc = $(".chkAccion[data-menu='" + idMenu + "']:checked").not(":disabled");
    if ($acc.length > 0) {
        $acc.prop("checked", false);
        MostrarMensaje("Se quitaron también las acciones de esa pantalla.", "warning");
    }
}
```

**Eventos:**
- Los checkbox de hijo pasan de `onchange='OnHijoChange(idPadre)'` a `onchange='OnHijoChange(idPadre); if(!this.checked){DesmarcarAccionesDe(idMenu);}'`, donde `idMenu` es el `hijo.Id_Menu` en el string.
- `OnPadreChange`, cuando desmarca un padre, también llama a `DesmarcarAccionesDe` para el padre y para cada hijo que desmarca.

**`GuardarMenus`:**
- arma `acciones` con `$(".chkAccion:checked").not(":disabled")` → `parseInt(data-accion)`;
- pasa a llamar a `PostMenu("GuardarPermisosPerfil", { "idPerfil": idPerfil, "activos": activos, "acciones": acciones }, …)`;
- el selector de `activos` sigue siendo `.chkMenu:checked`, que **no** incluye `.chkAccion`.

- [ ] **Step 2: `parametrizacionMenuUsuario.js`**

- **Estado:** agregar `var _acciones = [];`.
- **`SeleccionarUsuario`:** después de `ListaMenuUsuario`, pide `ListaAccionesUsuario {codUsuario}` y después pinta el árbol.
- **`RenderArbol`:** después de cada `Etiqueta(padre)` y `Etiqueta(hijo)`, agrega `PintarAcciones(item.Id_Menu)`.
- **`PintarAcciones` en esta pantalla:**
  - heredada del perfil (`ActivoPerfil == 1`): marcada, `disabled`, con la etiqueta `<span class='label label-default'>Perfil</span>`;
  - adicional (`ActivoUsuario == 1`): marcada y editable, con la etiqueta `<span class='label label-success'>Extra</span>`;
  - las casillas llevan la clase `chkAccion` y **no** `chkModulo`.
- **`OnAccionChange`:** marcar una acción marca `.chkModulo[data-menu=idMenu]` si no está `disabled`, y luego llama a `OnHijoChange(padre)`.
- **`DesmarcarAccionesDe`:** igual que en perfil, con `.not(":disabled")`.
- **`SincronizarRama`:** sin cambios. Cuenta `.chkHijo`, que no incluye acciones.
- **`GuardarModulos`:** envía `GuardarPermisosUsuario { codUsuario, extras, acciones }`. `acciones` sale de `$(".chkAccion:checked").not(":disabled")`.

- [ ] **Step 3: Subir versiones y revisar la codificación**

- Subir el `?v=` de los dos `<script>` en las `.aspx`.
- Revisar que los dos `.js` conserven el BOM: `head -c3 ReporteTareas/js/parametrizacionMenuPerfil.js | od -c | head -1` debe empezar con `357 273 277`.

- [ ] **Step 4: Compilar**

Expected: EXIT 0. El JS no se compila: se revisa que no tenga `let`, `const`, `=>` ni `` ` ``, con `grep -nE "\blet\b|\bconst\b|=>|\`" ReporteTareas/js/parametrizacionMenu*.js`, que no debe imprimir nada.

- [ ] **Step 5: Commit**

```bash
git add ReporteTareas/js/parametrizacionMenuPerfil.js ReporteTareas/js/parametrizacionMenuUsuario.js ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx ReporteTareas/Formulario/ParametrizacionMenuUsuario.aspx
git commit -m "feat(permisos): el arbol de menus muestra y guarda las acciones" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 8: Ajustes de menú y verificación (SQL)

**Files:**
- Create: `docs/sql/2026-10-14-permisos-ajustes-menu.sql`
- Create: `docs/sql/2026-10-14-permisos-verificacion.sql`

- [ ] **Step 1: Escribir el script de ajustes**

```sql
/* ============================================================================
   Permisos desde el portal: ajustes de menu (decisiones del 2026-10-14)
   ReporTarea  |  2026-10-14

   PENDIENTE DE EJECUTAR. Va ANTES de los binarios; con los binarios viejos
   solo cambia lo que se ve en el menu.

   Desde este cambio el menu decide el acceso. Cruce codigo <-> menu y
   decisiones del usuario:
     1. ParametrizacionHorasExtras.aspx: el 14 la tenia por codigo y no en el
        menu -> se AGREGA al menu del 14.
     2. ParametrizacionPerfiles.aspx: el 2 la tenia por codigo y no en el menu
        -> gana el menu (nada que hacer aqui; el codigo deja de darsela).
     3. ParametrizacionPerfiles.aspx: el 14 la tenia en el menu y el codigo la
        rechazaba -> se QUITA del menu del 14.
     4. ParametrizacionMenuPerfil.aspx y ParametrizacionMenuUsuario.aspx: solo
        el 18 -> se QUITAN del menu del 14 y del 19.

   Se desactiva (Estado = '1'), no se borra. Un grupo que quede sin ninguna
   hoja visible para ese perfil tambien se oculta, para no dejar un titulo
   vacio en el menu. Repetible.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @Hoja INT, @Padre INT;

/* 1. agregar ParametrizacionHorasExtras.aspx al 14, con su grupo */
SELECT @Hoja = Id_Menu, @Padre = Id_MenuPadre FROM dbo.MenuDos WHERE Href = 'ParametrizacionHorasExtras.aspx';

UPDATE dbo.PerfilMenu SET Estado = 0 WHERE IdPerfil = 14 AND id_Menu IN (@Hoja, @Padre) AND Estado <> 0;
INSERT INTO dbo.PerfilMenu (IdPerfil, id_Menu, Estado)
SELECT 14, x.Id, 0 FROM (SELECT Id = @Hoja UNION SELECT @Padre) x
 WHERE x.Id IS NOT NULL AND ISNULL(x.Id, 0) <> 0
   AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm WHERE pm.IdPerfil = 14 AND pm.id_Menu = x.Id);

/* 3 y 4. quitar hojas */
DECLARE @Quitar TABLE (IdPerfil INT, Href VARCHAR(100));
INSERT @Quitar VALUES
 (14, 'ParametrizacionPerfiles.aspx'),
 (14, 'ParametrizacionMenuPerfil.aspx'), (14, 'ParametrizacionMenuUsuario.aspx'),
 (19, 'ParametrizacionMenuPerfil.aspx'), (19, 'ParametrizacionMenuUsuario.aspx');

UPDATE pm SET pm.Estado = 1
  FROM dbo.PerfilMenu pm
  JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
  JOIN @Quitar q ON q.IdPerfil = pm.IdPerfil AND q.Href = m.Href
 WHERE pm.Estado = 0;

/* grupos que quedaron vacios para esos perfiles */
UPDATE pm SET pm.Estado = 1
  FROM dbo.PerfilMenu pm
  JOIN dbo.MenuDos g ON g.Id_Menu = pm.id_Menu
 WHERE pm.IdPerfil IN (14, 19) AND pm.Estado = 0
   AND ISNULL(LTRIM(RTRIM(g.Href)), '') = ''
   AND g.Id_Menu IN (SELECT m.Id_MenuPadre FROM dbo.MenuDos m JOIN @Quitar q ON q.Href = m.Href)
   AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu h JOIN dbo.MenuDos mh ON mh.Id_Menu = h.id_Menu
                    WHERE h.IdPerfil = pm.IdPerfil AND h.Estado = 0 AND mh.Id_MenuPadre = g.Id_Menu);
GO

/* aserciones */
DECLARE @Fallos INT = 0;

IF NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
                WHERE m.Href = 'ParametrizacionHorasExtras.aspx' AND pm.IdPerfil = 14 AND pm.Estado = 0)
BEGIN RAISERROR('FALLO: el 14 no tiene ParametrizacionHorasExtras.aspx.', 16, 1); SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
            WHERE pm.Estado = 0 AND pm.IdPerfil <> 18
              AND m.Href IN ('ParametrizacionMenuPerfil.aspx', 'ParametrizacionMenuUsuario.aspx'))
BEGIN RAISERROR('FALLO: un perfil distinto del 18 administra permisos.', 16, 1); SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM dbo.R_UsuarioMenu um JOIN dbo.MenuDos m ON m.Id_Menu = um.Id_Menu
            WHERE um.Estado = 'A' AND m.Href IN ('ParametrizacionMenuPerfil.aspx', 'ParametrizacionMenuUsuario.aspx'))
BEGIN RAISERROR('AVISO/FALLO: hay usuarios con Menu por perfil o por usuario como adicional; revisar a mano.', 16, 1); SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
            WHERE m.Href = 'ParametrizacionPerfiles.aspx' AND pm.IdPerfil = 14 AND pm.Estado = 0)
BEGIN RAISERROR('FALLO: el 14 sigue viendo ParametrizacionPerfiles.aspx.', 16, 1); SET @Fallos += 1; END

IF @Fallos = 0
    PRINT 'VEREDICTO: OK - menu ajustado a las decisiones del 2026-10-14.';
ELSE
    RAISERROR('VEREDICTO: FALLO - %d comprobaciones fallaron.', 16, 1, @Fallos);
GO

/* REVERSA (no se ejecuta; copiar a mano si hiciera falta):
UPDATE pm SET Estado = 0 FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
 WHERE (pm.IdPerfil = 14 AND m.Href IN ('ParametrizacionPerfiles.aspx','ParametrizacionMenuPerfil.aspx','ParametrizacionMenuUsuario.aspx'))
    OR (pm.IdPerfil = 19 AND m.Href IN ('ParametrizacionMenuPerfil.aspx','ParametrizacionMenuUsuario.aspx'));
UPDATE pm SET Estado = 1 FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
 WHERE pm.IdPerfil = 14 AND m.Href = 'ParametrizacionHorasExtras.aspx';
-- y reactivar a mano los grupos que se hayan ocultado (mirar Id_MenuPadre de esas hojas).
*/
```

- [ ] **Step 2: Escribir el script de verificación, de solo lectura**

`docs/sql/2026-10-14-permisos-verificacion.sql` compara, para cada pantalla protegida, los perfiles que la tienen activa en `PerfilMenu` contra lo esperado. Imprime cada diferencia y termina con `VEREDICTO`.

```sql
/* Verificacion de permisos (solo lectura). Matriz esperada despues de los dos
   scripts del 2026-10-14. El 18 se exige en el menu de todas para que las
   vea; su acceso no depende de esto. */
SET NOCOUNT ON;
DECLARE @E TABLE (Href VARCHAR(100), IdPerfil INT);
INSERT @E VALUES
 ('TableroCostos.aspx',14),('TableroCostos.aspx',18),
 ('HorasExtras.aspx',2),('HorasExtras.aspx',14),('HorasExtras.aspx',18),
 ('ParametrizacionHorasExtras.aspx',14),('ParametrizacionHorasExtras.aspx',18),
 ('CurvaColaboradores.aspx',14),('CurvaColaboradores.aspx',18),
 ('PerfilesPersonal.aspx',14),('PerfilesPersonal.aspx',18),
 ('ParametrizacionPerfiles.aspx',18),('ParametrizacionPerfiles.aspx',19),
 ('ParametrizacionUsuarios.aspx',14),('ParametrizacionUsuarios.aspx',18),('ParametrizacionUsuarios.aspx',19),
 ('ParametrizacionMenuPerfil.aspx',18),('ParametrizacionMenuUsuario.aspx',18);

;WITH R AS (
    SELECT DISTINCT m.Href, pm.IdPerfil FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
     WHERE pm.Estado = 0 AND m.Href IN (SELECT Href FROM @E))
SELECT Diferencia = CASE WHEN r.Href IS NULL THEN 'FALTA en el menu' ELSE 'SOBRA en el menu' END,
       Href = ISNULL(e.Href, r.Href), IdPerfil = ISNULL(e.IdPerfil, r.IdPerfil)
  INTO #Dif
  FROM @E e FULL JOIN R r ON r.Href = e.Href AND r.IdPerfil = e.IdPerfil
 WHERE e.Href IS NULL OR r.Href IS NULL;

SELECT * FROM #Dif ORDER BY Href, IdPerfil;

SELECT Accion = a.Clave, pa.IdPerfil, pa.Estado FROM dbo.RTA_PerfilAccion pa JOIN dbo.RTA_Accion a ON a.Id_Accion = pa.Id_Accion ORDER BY a.Clave, pa.IdPerfil;

IF (SELECT COUNT(*) FROM #Dif) = 0
    PRINT 'VEREDICTO: OK - el menu coincide con la matriz acordada.';
ELSE
    RAISERROR('VEREDICTO: FALLO - el menu difiere de la matriz acordada (ver filas).', 16, 1);
DROP TABLE #Dif;
```

- [ ] **Step 3: Revisar la codificación** de los dos archivos (sin BOM y solo ASCII, igual que en la Tarea 1, Step 2).

- [ ] **Step 4: Commit**

```bash
git add docs/sql/2026-10-14-permisos-ajustes-menu.sql docs/sql/2026-10-14-permisos-verificacion.sql
git commit -m "feat(permisos): ajustes de menu acordados y verificacion de la matriz" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 9: Documentación, paquete y aplicación en la base (controlador)

**Files:**
- Modify: `DESPLIEGUE.md`. Agregar la §17 «Permisos desde el portal» y actualizar el recuadro de la §16.1 y la §16.5: el Tablero ya no tiene lista en el código, y para agregar un perfil basta con `PerfilMenu`.
- Regenerar el paquete Release.

- [ ] **Step 1: DESPLIEGUE §17**

Contenido mínimo, en español con tildes:
- **Qué es:** el menú decide el acceso a las 10 pantallas protegidas; hay acciones por perfil y por usuario; el 18 puede todo.
- **Orden:**
  1. `2026-10-14-permisos-esquema.sql`;
  2. `2026-10-14-permisos-ajustes-menu.sql`;
  3. binarios **juntos** (`ReporteTareas.dll` y los tres `.exe` de capa), más `ParametrizacionMenuPerfil.aspx`, `ParametrizacionMenuUsuario.aspx` y sus dos `.js`;
  4. `2026-10-14-permisos-verificacion.sql`.
- **Cómo dar una pantalla o una acción:** marcarla en Menú por perfil o en Menú por usuario. Tarda hasta 2 minutos en aplicarse para quien ya tiene la sesión abierta.
- **Agregar una acción nueva:** una fila en `RTA_Accion` y una línea `Permisos.PuedeHacer` en el código.
- **Proteger una pantalla más:** una línea `Permisos.PuedeEntrar` en su `.aspx.cs` y en su handler.
- **Reversa:** reponer los binarios anteriores, juntos. Las tablas pueden quedar. El script de ajustes trae su reversa comentada.
- **Lista para probar en producción:**
  - **con alguien del perfil 14:** entra a Tablero, Horas Extras, Parámetros de Horas Extras, Curva, Perfiles del personal y Usuarios; **no** entra a Perfiles, Menú por perfil ni Menú por usuario; ve «Completar datos» y **no** ve «Reabrir»;
  - **con la persona de perfil 2 que tiene el Tablero como adicional:** entra a Tablero y a Horas Extras, sin «Completar datos»;
  - **con cualquiera de los dos:** escribir la dirección de una pantalla que no tiene lo manda a Principal.

- [ ] **Step 2: Paquete**

PowerShell: `& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas\ReporteTareas.csproj" -t:Package -p:Configuration=Release -v:minimal -nologo`

Expected: EXIT 0.

Hacer commit de `DESPLIEGUE.md` y, en un commit aparte (`build(permisos): paquete …`), del paquete con `git add -u` sobre las carpetas `bin` y `obj`, **excluyendo los dos `Web.config`**.

- [ ] **Step 3: Base de datos — SOLO con autorización explícita del usuario**

Preguntar al usuario y, con su sí:
1. `bash docs/sql/run-sql.sh docs/sql/2026-10-14-permisos-esquema.sql` → `VEREDICTO: OK`.
2. `bash docs/sql/run-sql.sh docs/sql/2026-10-14-permisos-ajustes-menu.sql` → `VEREDICTO: OK`.
3. `bash docs/sql/run-sql.sh docs/sql/2026-10-14-permisos-verificacion.sql` → `VEREDICTO: OK`.

Si algo falla, parar y reportar la salida exacta.

- [ ] **Step 4: Prueba en navegador (perfil 18, IIS Express de 32 bits)**

Levantar IIS Express desde PowerShell, puerto 51037, como en sesiones anteriores. Con el login del usuario:
1. «Menú por perfil», perfil 14: debajo de *Horas Extras* aparecen ⚙ «Reabrir periodo cerrado» (desmarcada) y ⚙ «Completar datos…» (marcada).
2. Marcar «Reabrir» para el 14 y guardar. Recargar y confirmar que quedó marcada. Desmarcarla y guardar, para que quede como estaba.
3. Perfil 18: las acciones salen marcadas y bloqueadas, con la nota visible.
4. «Menú por usuario», la persona de perfil 2: el Tablero aparece como *Extra* y Horas Extras como *Perfil*.

El controlador no puede iniciar sesión con otros perfiles (el login va por AD): las pruebas de 14 y 2 quedan en la lista de la §17 para el usuario.
