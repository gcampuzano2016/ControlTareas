/* ============================================================================
   Vacaciones - liquidacion por periodo para salidas: menu
   ReporTarea  |  2026-10-29
   Se corre AL DESPLEGAR, junto con los binarios: antes, la opcion llevaria a
   una pagina que todavia no existe en el servidor.
   Crea o actualiza:
     1. Hoja VacacionesLiquidacion.aspx "Liquidacion de vacaciones" (con tilde) dentro del grupo 5
        (Vacaciones & Permisos), clonada de TableroCostos.aspx.
     2. Visibilidad (PerfilMenu, Estado '0' = visible): 14 (Talento Humano) y
        18 (super admin, NegPermisos.PerfilSuperAdmin). El grupo 5 ya es visible para los dos; si no lo fuera,
        el script lo avisa y no lo toca, porque cambiaria el menu de otros.
   Muestra sueldos: quien mas la necesite se le da como extra desde Permisos,
   no aqui, y con cuidado.
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

DECLARE @Href         VARCHAR(128) = 'VacacionesLiquidacion.aspx';
/* "Liquidacion" con tilde via NCHAR(243): el archivo va sin tildes (ver 2026-10-22-bonos-menu.sql). */
DECLARE @Titulo       VARCHAR(128) = CONVERT(VARCHAR(128), N'Liquidaci' + NCHAR(243) + N'n de vacaciones');
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
       SET Href = @Href, Titulo = @Titulo, Descripcion = 'Liquidacion de vacaciones por periodo para salidas (GTH)',
           Id_MenuPadre = @IdGrupo, Es_Opcion_de_Menu = 0, Orden_Opcion = @Orden
     WHERE Id_Menu = @IdHoja;
    PRINT 'Hoja creada: ' + @Href + ' = ' + CONVERT(VARCHAR(10), @IdHoja);
END

/* Si la hoja ya existia con otro titulo, se corrige. */
UPDATE dbo.MenuDos SET Titulo = @Titulo
 WHERE Id_Menu = @IdHoja AND Titulo COLLATE Latin1_General_BIN <> @Titulo COLLATE Latin1_General_BIN;   -- binaria: con _AI, 'Nomina' = 'Nomina con tilde'
IF @@ROWCOUNT > 0 PRINT 'Titulo corregido.';

/* 2. visibilidad */
DECLARE @Vis TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Vis (IdPerfil) VALUES (14), (18);

UPDATE pm SET Estado = '0'
  FROM dbo.PerfilMenu AS pm
  JOIN @Vis AS v ON v.IdPerfil = pm.IdPerfil
 WHERE pm.id_Menu = @IdHoja AND pm.Estado <> '0';

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT @IdHoja, v.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM @Vis AS v
 WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu AS pm WHERE pm.id_Menu = @IdHoja AND pm.IdPerfil = v.IdPerfil);

COMMIT TRANSACTION;

/* El grupo tiene que verse para que se vea la hoja. */
IF EXISTS (SELECT 1 FROM @Vis v
            WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
                               WHERE pm.id_Menu = 5 AND pm.IdPerfil = v.IdPerfil AND pm.Estado = '0'))
    PRINT 'AVISO: el grupo 5 no es visible para el perfil 14 o el 18; la hoja no se vera hasta corregirlo.';

SELECT m.Id_Menu, m.Titulo, m.Href, m.Id_MenuPadre, pm.IdPerfil, pm.Estado
  FROM dbo.MenuDos m
  JOIN dbo.PerfilMenu pm ON pm.id_Menu = m.Id_Menu
 WHERE LTRIM(RTRIM(m.Href)) = 'VacacionesLiquidacion.aspx'
 ORDER BY pm.IdPerfil;
PRINT 'Menu listo.';
GO

/* ------------------------------------------------------------ REVERSA ------
BEGIN TRANSACTION;
DECLARE @M TABLE (Id INT);
INSERT INTO @M SELECT Id_Menu FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'VacacionesLiquidacion.aspx';
DELETE FROM dbo.PerfilMenu WHERE id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.R_UsuarioMenu WHERE Id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.MenuDos WHERE Id_Menu IN (SELECT Id FROM @M);
COMMIT TRANSACTION;
---------------------------------------------------------------------------- */
