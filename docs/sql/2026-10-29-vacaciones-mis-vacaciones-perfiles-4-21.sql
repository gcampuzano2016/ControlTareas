/* ============================================================================
   Vacaciones - "Mis vacaciones" para los perfiles 4 y 21
   ReporTarea  |  2026-10-29
   Los perfiles 4 ("falta" en dbo.Perfiles) y 21 (no existe en dbo.Perfiles)
   no veian el grupo 5 (Vacaciones & Permisos) y por eso no les llego la hoja
   MisVacaciones.aspx (2026-10-29-vacaciones-mis-vacaciones-menu.sql).
   Usuarios en esos perfiles a la fecha: 4 -> 2 activos (y 1 inactivo);
   21 -> 1 activo (y 2 inactivos).
   Hace visible para 4 y 21:
     1. el grupo 5, y
     2. la hoja MisVacaciones.aspx.
   No toca ninguna otra hoja del grupo 5: el grupo solo muestra las hojas que
   tienen su propia fila visible para el perfil, y 4/21 no tienen ninguna.
   Repetible. Reversa al final, comentada.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

DECLARE @IdGrupo INT = 5;
DECLARE @IdHoja  INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'MisVacaciones.aspx');

IF @IdHoja IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('No esta la hoja MisVacaciones.aspx. Correr antes 2026-10-29-vacaciones-mis-vacaciones-menu.sql.', 16, 1);
    RETURN;
END

DECLARE @Vis TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Vis (IdPerfil) VALUES (4), (21);

DECLARE @Menus TABLE (IdMenu INT PRIMARY KEY);
INSERT INTO @Menus (IdMenu) VALUES (@IdGrupo), (@IdHoja);

UPDATE pm SET Estado = '0'
  FROM dbo.PerfilMenu AS pm
  JOIN @Vis   AS v ON v.IdPerfil = pm.IdPerfil
  JOIN @Menus AS m ON m.IdMenu   = pm.id_Menu
 WHERE pm.Estado <> '0';

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT m.IdMenu, v.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM @Vis AS v CROSS JOIN @Menus AS m
 WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu AS pm WHERE pm.id_Menu = m.IdMenu AND pm.IdPerfil = v.IdPerfil);

COMMIT TRANSACTION;

/* Lo que ven 4 y 21 dentro del grupo 5: debe ser solo "Mis vacaciones". */
SELECT pm.IdPerfil, m.Id_Menu, m.Titulo, m.Href, m.Id_MenuPadre, pm.Estado
  FROM dbo.PerfilMenu pm
  JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
 WHERE pm.IdPerfil IN (4, 21) AND pm.Estado = '0'
   AND (m.Id_Menu = 5 OR m.Id_MenuPadre = 5)
 ORDER BY pm.IdPerfil, m.Id_MenuPadre, m.Id_Menu;
PRINT 'Listo.';
GO

/* ------------------------------------------------------------ REVERSA ------
DELETE pm
  FROM dbo.PerfilMenu pm
  JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
 WHERE pm.IdPerfil IN (4, 21)
   AND (m.Id_Menu = 5 OR LTRIM(RTRIM(m.Href)) = 'MisVacaciones.aspx');
---------------------------------------------------------------------------- */
