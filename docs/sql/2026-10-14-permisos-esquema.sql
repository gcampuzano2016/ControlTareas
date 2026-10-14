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
  JOIN (SELECT Href = LTRIM(RTRIM(Href)), Id_Menu = MIN(Id_Menu) FROM dbo.MenuDos WHERE ISNULL(LTRIM(RTRIM(Href)),'') <> '' GROUP BY LTRIM(RTRIM(Href))) m ON m.Href = c.Href
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
