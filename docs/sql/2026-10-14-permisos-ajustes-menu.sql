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
SELECT @Hoja = m.Id_Menu, @Padre = m.Id_MenuPadre FROM dbo.MenuDos m WHERE LTRIM(RTRIM(m.Href)) = 'ParametrizacionHorasExtras.aspx' AND m.Id_Menu = (SELECT MIN(x.Id_Menu) FROM dbo.MenuDos x WHERE LTRIM(RTRIM(x.Href)) = 'ParametrizacionHorasExtras.aspx');

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
  JOIN @Quitar q ON q.IdPerfil = pm.IdPerfil AND q.Href = LTRIM(RTRIM(m.Href))
 WHERE pm.Estado = 0;

/* grupos que quedaron vacios para esos perfiles */
UPDATE pm SET pm.Estado = 1
  FROM dbo.PerfilMenu pm
  JOIN dbo.MenuDos g ON g.Id_Menu = pm.id_Menu
 WHERE pm.IdPerfil IN (14, 19) AND pm.Estado = 0
   AND ISNULL(LTRIM(RTRIM(g.Href)), '') = ''
   AND g.Id_Menu IN (SELECT m.Id_MenuPadre FROM dbo.MenuDos m JOIN @Quitar q ON q.Href = LTRIM(RTRIM(m.Href)))
   AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu h JOIN dbo.MenuDos mh ON mh.Id_Menu = h.id_Menu
                    WHERE h.IdPerfil = pm.IdPerfil AND h.Estado = 0 AND mh.Id_MenuPadre = g.Id_Menu);
GO

/* aserciones */
DECLARE @Fallos INT = 0;

IF NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
                WHERE LTRIM(RTRIM(m.Href)) = 'ParametrizacionHorasExtras.aspx' AND pm.IdPerfil = 14 AND pm.Estado = 0)
BEGIN RAISERROR('FALLO: el 14 no tiene ParametrizacionHorasExtras.aspx.', 16, 1); SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
            WHERE pm.Estado = 0 AND pm.IdPerfil <> 18
              AND LTRIM(RTRIM(m.Href)) IN ('ParametrizacionMenuPerfil.aspx', 'ParametrizacionMenuUsuario.aspx'))
BEGIN RAISERROR('AVISO/FALLO: un perfil distinto del 18 administra permisos; revisar a mano.', 16, 1); SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM dbo.R_UsuarioMenu um JOIN dbo.MenuDos m ON m.Id_Menu = um.Id_Menu
            WHERE um.Estado = 'A' AND LTRIM(RTRIM(m.Href)) IN ('ParametrizacionMenuPerfil.aspx', 'ParametrizacionMenuUsuario.aspx'))
BEGIN RAISERROR('AVISO/FALLO: hay usuarios con Menu por perfil o por usuario como adicional; revisar a mano.', 16, 1); SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
            WHERE LTRIM(RTRIM(m.Href)) = 'ParametrizacionPerfiles.aspx' AND pm.IdPerfil = 14 AND pm.Estado = 0)
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
 WHERE pm.IdPerfil = 14 AND LTRIM(RTRIM(m.Href)) = 'ParametrizacionHorasExtras.aspx';
-- y reactivar a mano los grupos que se hayan ocultado (mirar Id_MenuPadre de esas hojas).
*/
