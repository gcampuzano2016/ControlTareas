/* ============================================================================
   Modulos extra por usuario — tabla y stored procedures
   Regla: el usuario ve los menus de su perfil MAS estos extras. Nunca menos.
   Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- 1) Tabla de excepciones aditivas ---------- */
IF OBJECT_ID('dbo.R_UsuarioMenu','U') IS NULL
BEGIN
    CREATE TABLE dbo.R_UsuarioMenu
    (
        Id_UsuarioMenu   INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        Id_Menu          INT          NOT NULL,
        Estado           CHAR(1)      NOT NULL CONSTRAINT DF_R_UsuarioMenu_Estado DEFAULT ('A'),
        Usuario_Registro VARCHAR(50)  NOT NULL,
        Fecha_Registro   DATETIME     NOT NULL CONSTRAINT DF_R_UsuarioMenu_Fecha DEFAULT (GETDATE()),
        CONSTRAINT PK_R_UsuarioMenu PRIMARY KEY (Id_UsuarioMenu),
        CONSTRAINT UQ_R_UsuarioMenu UNIQUE (Cod_Usuario, Id_Menu)
    );
END
GO

/* ---------- 2) Buscar usuarios con su perfil ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarUsuariosMenu','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarUsuariosMenu;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarUsuariosMenu
    @Filtro VARCHAR(100) = ''
AS
BEGIN
    SET NOCOUNT ON;
    SET @Filtro = LTRIM(RTRIM(ISNULL(@Filtro,'')));

    /* Cod_Usuario NO es unico en R_Usuarios: hay usuarios activos con varias filas
       (distintos perfiles). Se agrupa por Cod_Usuario para devolver una sola fila
       por usuario. Id_Perfil toma el mayor de los perfiles del usuario (mismo
       criterio deterministico que Sp_RTA_ListarMenuUsuario y
       Sp_RTA_GuardarMenuUsuario), y NombrePerfil se resuelve para ESE Id_Perfil,
       no con un MAX() independiente que podria mezclar perfil de una fila con
       nombre de otra. */
    SELECT
        g.Cod_Usuario,
        g.Nom_Usuario,
        g.Cedula,
        g.Id_Perfil,
        NombrePerfil = ISNULL(p.NombrePerfil,'Sin perfil'),
        TotalExtras  = ISNULL(x.Total,0)
    FROM
    (
        SELECT
            u.Cod_Usuario,
            Nom_Usuario = MAX(u.Nom_Usuario),
            Cedula      = MAX(ISNULL(u.Cedula,'')),
            Id_Perfil   = MAX(u.Id_Perfil)
        FROM dbo.R_Usuarios u
        WHERE u.Usuario_Estado = 'A'
          AND
          (
              @Filtro = ''
              OR u.Nom_Usuario LIKE '%' + @Filtro + '%'
              OR u.Cod_Usuario LIKE '%' + @Filtro + '%'
              OR ISNULL(u.Cedula,'') LIKE '%' + @Filtro + '%'
          )
        GROUP BY u.Cod_Usuario
    ) g
    LEFT JOIN dbo.Perfiles p
        ON p.IdPerfiles = g.Id_Perfil
    OUTER APPLY
    (
        SELECT Total = COUNT(*)
        FROM dbo.R_UsuarioMenu um
        WHERE um.Cod_Usuario = g.Cod_Usuario AND um.Estado = 'A'
    ) x
    ORDER BY g.Nom_Usuario;
END
GO

/* ---------- 3) Arbol de menus con las dos banderas ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarMenuUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarMenuUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarMenuUsuario
    @CodUsuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    /* Cod_Usuario no es unico en R_Usuarios (hay usuarios activos con varias filas
       y perfiles distintos): se toma el mayor Id_Perfil de forma deterministica,
       mismo criterio que Sp_RTA_ListarUsuariosMenu y Sp_RTA_GuardarMenuUsuario. */
    DECLARE @IdPerfil BIGINT;
    SELECT TOP 1 @IdPerfil = Id_Perfil
    FROM dbo.R_Usuarios
    WHERE Cod_Usuario = @CodUsuario
    ORDER BY Id_Perfil DESC;

    SELECT
        m.Id_Menu,
        Id_MenuPadre  = ISNULL(m.Id_MenuPadre,0),
        m.Titulo,
        Class_Icon    = ISNULL(m.Class_Icon,''),
        ActivoPerfil  = CASE WHEN pm.id_Menu IS NOT NULL THEN 1 ELSE 0 END,
        ActivoUsuario = CASE WHEN um.Id_Menu IS NOT NULL THEN 1 ELSE 0 END
    FROM dbo.MenuDos m
    LEFT JOIN dbo.PerfilMenu pm
        ON pm.id_Menu = m.Id_Menu AND pm.IdPerfil = @IdPerfil AND pm.Estado = 0
    LEFT JOIN dbo.R_UsuarioMenu um
        ON um.Id_Menu = m.Id_Menu AND um.Cod_Usuario = @CodUsuario AND um.Estado = 'A'
    ORDER BY ISNULL(m.Id_MenuPadre,0), m.Titulo;
END
GO

/* ---------- 4) Guardar extras (atomico) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_GuardarMenuUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarMenuUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_GuardarMenuUsuario
    @CodUsuario      VARCHAR(50),
    @ExtrasCsv       VARCHAR(MAX) = '',
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

    BEGIN TRY
        BEGIN TRANSACTION;

        /* Cod_Usuario no es unico en R_Usuarios (hay usuarios activos con varias
           filas y perfiles distintos): se toma el mayor Id_Perfil de forma
           deterministica, mismo criterio que Sp_RTA_ListarUsuariosMenu y
           Sp_RTA_ListarMenuUsuario. */
        DECLARE @IdPerfil BIGINT;
        SELECT TOP 1 @IdPerfil = Id_Perfil
        FROM dbo.R_Usuarios
        WHERE Cod_Usuario = @CodUsuario
        ORDER BY Id_Perfil DESC;

        DECLARE @Extras TABLE (IdMenu INT PRIMARY KEY);

        /* parsear el CSV de enteros via XML-nodes (compatible SQL 2008+) */
        IF ISNULL(LTRIM(RTRIM(@ExtrasCsv)),'') <> ''
        BEGIN
            DECLARE @xml XML = CAST('<i>' + REPLACE(@ExtrasCsv, ',', '</i><i>') + '</i>' AS XML);
            INSERT INTO @Extras (IdMenu)
            SELECT DISTINCT T.c.value('.', 'INT')
            FROM @xml.nodes('/i') AS T(c)
            WHERE ISNULL(T.c.value('.', 'VARCHAR(20)'),'') <> '';
        END

        /* solo ids que existan en MenuDos */
        DELETE FROM @Extras
        WHERE IdMenu NOT IN (SELECT Id_Menu FROM dbo.MenuDos);

        /* invariante hijo=>padre: si el padre no lo da el perfil ni esta en el set, agregarlo.
           Master.Master.cs no dibuja un hijo cuyo padre no exista en el resultado. */
        INSERT INTO @Extras (IdMenu)
        SELECT DISTINCT m.Id_MenuPadre
        FROM @Extras a
        JOIN dbo.MenuDos m ON m.Id_Menu = a.IdMenu
        WHERE ISNULL(m.Id_MenuPadre,0) <> 0
          AND m.Id_MenuPadre NOT IN (SELECT IdMenu FROM @Extras)
          AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
                          WHERE pm.IdPerfil = @IdPerfil AND pm.id_Menu = m.Id_MenuPadre AND pm.Estado = 0);

        /* upsert: activar los del set, desactivar el resto de sus filas */
        MERGE dbo.R_UsuarioMenu AS tgt
        USING (
            SELECT IdMenu, EsActivo = 1 FROM @Extras
            UNION
            SELECT um.Id_Menu, EsActivo = 0
            FROM dbo.R_UsuarioMenu um
            WHERE um.Cod_Usuario = @CodUsuario
              AND um.Id_Menu NOT IN (SELECT IdMenu FROM @Extras)
        ) AS src
        ON tgt.Cod_Usuario = @CodUsuario AND tgt.Id_Menu = src.IdMenu
        WHEN MATCHED AND tgt.Estado <> CASE WHEN src.EsActivo = 1 THEN 'A' ELSE 'I' END THEN
            UPDATE SET tgt.Estado           = CASE WHEN src.EsActivo = 1 THEN 'A' ELSE 'I' END,
                       tgt.Usuario_Registro = @UsuarioRegistro,
                       tgt.Fecha_Registro   = GETDATE()
        WHEN NOT MATCHED BY TARGET AND src.EsActivo = 1 THEN
            INSERT (Cod_Usuario, Id_Menu, Estado, Usuario_Registro, Fecha_Registro)
            VALUES (@CodUsuario, src.IdMenu, 'A', @UsuarioRegistro, GETDATE());

        SET @Respuestas = 1;
        SET @Mensaje = 'Modulos del usuario guardados correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar los modulos del usuario: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO
