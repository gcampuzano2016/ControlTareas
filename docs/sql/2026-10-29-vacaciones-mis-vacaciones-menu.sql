/* ============================================================================
   Vacaciones - "Mis vacaciones": menu
   ReporTarea  |  2026-10-29
   Se corre AL DESPLEGAR, despues de los binarios.
   Crea o actualiza:
     1. Hoja MisVacaciones.aspx "Mis vacaciones" dentro del grupo 5
        (Vacaciones & Permisos), clonada de TableroCostos.aspx.
     2. Visibilidad (PerfilMenu, Estado '0' = visible): todo perfil que hoy ve
        el grupo 5. Quien ve "Vacaciones & Permisos" ve su saldo.
   Cada persona ve solo lo suyo: lo decide AdministrarMisVacaciones.ashx con
   el usuario de la sesion, no el menu.
   Repetible. Reversa al final, comentada.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID('tempdb..#ClonarMenu') IS NOT NULL DROP PROCEDURE #ClonarMenu;
GO
/* Mismo bloque que 2026-10-22-bonos-menu.sql. */
CREATE PROCEDURE #ClonarMenu
    @IdModelo INT,
    @IdNuevo  INT OUTPUT
AS
BEGIN
    DECLARE @cols NVARCHAR(MAX) = STUFF
    ((
        SELECT ',' + QUOTENAME(c.name)
          FROM sys.columns AS c
         WHERE c.object_id = OBJECT_ID('dbo.MenuDos')
           AND c.is_identity = 0 AND c.is_computed = 0 AND c.name <> 'Id_Menu'
         ORDER BY c.column_id
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 1, '');
    DECLARE @sql NVARCHAR(MAX);

    IF EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.MenuDos') AND name = 'Id_Menu' AND is_identity = 1)
    BEGIN
        SET @sql = N'INSERT INTO dbo.MenuDos (' + @cols + N') SELECT ' + @cols +
                   N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo; SET @IdOut = CONVERT(INT, SCOPE_IDENTITY());';
        EXEC sys.sp_executesql @sql, N'@Modelo INT, @IdOut INT OUTPUT', @Modelo = @IdModelo, @IdOut = @IdNuevo OUTPUT;
    END
    ELSE
    BEGIN
        SELECT @IdNuevo = ISNULL(MAX(Id_Menu), 0) + 1 FROM dbo.MenuDos WITH (TABLOCKX, HOLDLOCK);
        SET @sql = N'INSERT INTO dbo.MenuDos ([Id_Menu],' + @cols + N') SELECT @Id, ' + @cols +
                   N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo;';
        EXEC sys.sp_executesql @sql, N'@Modelo INT, @Id INT', @Modelo = @IdModelo, @Id = @IdNuevo;
    END
END
GO

BEGIN TRANSACTION;

DECLARE @Href         VARCHAR(128) = 'MisVacaciones.aspx';
DECLARE @IdGrupo      INT = 5;
DECLARE @IdModeloHoja INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'TableroCostos.aspx');
DECLARE @IdHoja INT, @Orden INT;

IF NOT EXISTS (SELECT 1 FROM dbo.MenuDos WHERE Id_Menu = @IdGrupo AND Es_Opcion_de_Menu = 1)
   OR @IdModeloHoja IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('No estan las filas modelo (grupo 5 o TableroCostos.aspx). Revisar el menu antes de seguir.', 16, 1);
    RETURN;
END

/* 1. hoja, al final del grupo */
SET @IdHoja = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = @Href);
IF @IdHoja IS NULL
BEGIN
    EXEC #ClonarMenu @IdModeloHoja, @IdHoja OUTPUT;
    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1 FROM dbo.MenuDos WHERE Id_MenuPadre = @IdGrupo AND Id_Menu <> @IdHoja;
    UPDATE dbo.MenuDos
       SET Href = @Href, Titulo = 'Mis vacaciones', Descripcion = 'Saldo, periodos y solicitudes de vacaciones propios',
           Id_MenuPadre = @IdGrupo, Es_Opcion_de_Menu = 0, Orden_Opcion = @Orden
     WHERE Id_Menu = @IdHoja;
    PRINT 'Hoja creada: ' + @Href + ' = ' + CONVERT(VARCHAR(10), @IdHoja);
END

/* 2. visibilidad: los mismos perfiles que ven el grupo 5 */
DECLARE @Vis TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Vis (IdPerfil)
SELECT DISTINCT IdPerfil FROM dbo.PerfilMenu WHERE id_Menu = @IdGrupo AND Estado = '0';

UPDATE pm SET Estado = '0'
  FROM dbo.PerfilMenu AS pm
  JOIN @Vis AS v ON v.IdPerfil = pm.IdPerfil
 WHERE pm.id_Menu = @IdHoja AND pm.Estado <> '0';

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT @IdHoja, v.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM @Vis AS v
 WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu AS pm WHERE pm.id_Menu = @IdHoja AND pm.IdPerfil = v.IdPerfil);

COMMIT TRANSACTION;

SELECT m.Id_Menu, m.Titulo, pm.IdPerfil, pm.Estado
  FROM dbo.MenuDos m
  JOIN dbo.PerfilMenu pm ON pm.id_Menu = m.Id_Menu
 WHERE LTRIM(RTRIM(m.Href)) = 'MisVacaciones.aspx'
 ORDER BY pm.IdPerfil;
PRINT 'Menu listo.';
GO

/* ------------------------------------------------------------ REVERSA ------
BEGIN TRANSACTION;
DECLARE @M TABLE (Id INT);
INSERT INTO @M SELECT Id_Menu FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'MisVacaciones.aspx';
DELETE FROM dbo.PerfilMenu WHERE id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.R_UsuarioMenu WHERE Id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.MenuDos WHERE Id_Menu IN (SELECT Id FROM @M);
COMMIT TRANSACTION;
---------------------------------------------------------------------------- */
