/* ============================================================================
   Menus por perfil — stored procedures
   Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- 1) Listar perfiles (combo) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarPerfiles','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPerfiles;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarPerfiles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id_Perfil, Nombre
    FROM dbo.R_Perfil
    WHERE ISNULL(Estado_Logico_Registro,1) = 1
    ORDER BY Nombre;
END
GO

/* ---------- 2) Listar menus con estado para el perfil ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarMenuPerfil','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarMenuPerfil;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarMenuPerfil
    @IdPerfil INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        m.Id_Menu,
        Id_MenuPadre = ISNULL(m.Id_MenuPadre,0),
        m.Titulo,
        Class_Icon   = ISNULL(m.Class_Icon,''),
        Activo       = CASE WHEN pm.id_Menu IS NOT NULL THEN 1 ELSE 0 END
    FROM dbo.MenuDos m
    LEFT JOIN dbo.PerfilMenu pm
        ON pm.id_Menu = m.Id_Menu AND pm.IdPerfil = @IdPerfil AND pm.Estado = 0
    ORDER BY ISNULL(m.Id_MenuPadre,0), m.Titulo;
END
GO

/* ---------- 3) Guardar (atomico; agrega los padres de los hijos activos) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_GuardarMenuPerfil','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarMenuPerfil;
GO
CREATE PROCEDURE dbo.Sp_RTA_GuardarMenuPerfil
    @IdPerfil   INT,
    @ActivosCsv VARCHAR(MAX) = ''
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    IF ISNULL(@IdPerfil,0) = 0 OR NOT EXISTS (SELECT 1 FROM dbo.R_Perfil WHERE Id_Perfil = @IdPerfil)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El perfil indicado no existe.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Activos TABLE (IdMenu INT PRIMARY KEY);

        /* parsear el CSV de enteros via XML-nodes (compatible SQL 2008+) */
        IF ISNULL(LTRIM(RTRIM(@ActivosCsv)),'') <> ''
        BEGIN
            DECLARE @xml XML = CAST('<i>' + REPLACE(@ActivosCsv, ',', '</i><i>') + '</i>' AS XML);
            INSERT INTO @Activos (IdMenu)
            SELECT DISTINCT T.c.value('.', 'INT')
            FROM @xml.nodes('/i') AS T(c)
            WHERE ISNULL(T.c.value('.', 'VARCHAR(20)'),'') <> '';
        END

        /* invariante: agregar el padre de todo hijo activo que no este ya en el set */
        INSERT INTO @Activos (IdMenu)
        SELECT DISTINCT m.Id_MenuPadre
        FROM @Activos a
        JOIN dbo.MenuDos m ON m.Id_Menu = a.IdMenu
        WHERE ISNULL(m.Id_MenuPadre,0) <> 0
          AND m.Id_MenuPadre NOT IN (SELECT IdMenu FROM @Activos);

        /* upsert: existentes -> Estado 0/1 segun set; faltantes activos -> insertar Estado 0 */
        MERGE dbo.PerfilMenu AS tgt
        USING (
            SELECT m.Id_Menu,
                   EsActivo = CASE WHEN a.IdMenu IS NOT NULL THEN 1 ELSE 0 END
            FROM dbo.MenuDos m
            LEFT JOIN @Activos a ON a.IdMenu = m.Id_Menu
        ) AS src
        ON tgt.IdPerfil = @IdPerfil AND tgt.id_Menu = src.Id_Menu
        WHEN MATCHED THEN
            UPDATE SET tgt.Estado = CASE WHEN src.EsActivo = 1 THEN 0 ELSE 1 END
        WHEN NOT MATCHED BY TARGET AND src.EsActivo = 1 THEN
            INSERT (IdPerfil, id_Menu, Estado) VALUES (@IdPerfil, src.Id_Menu, 0);

        SET @Respuestas = 1;
        SET @Mensaje = 'Menus del perfil guardados correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar los menus del perfil: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO
