/* ============================================================================
   Perfil 41 (Medico Laboral): sin Equipo y agenda, Procesos ni Bonos
   ReporTarea  |  2026-10-30
   La cuenta medicolaboral es del medico externo. Los grupos "Equipo y agenda"
   y "Bonos" le llegaron porque 2026-10-15-agenda-menu.sql y
   2026-10-22-bonos-menu.sql los dieron a todo perfil con usuarios activos.
   Se ocultan (Estado '1') todas las filas del perfil 41 en esos dos grupos y
   sus hojas. No se borran: la reversa es volver a '0'.
   No toca el Departamento Medico, que va por R_UsuarioMenu.
   Repetible. Reversa al final, comentada.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

DECLARE @Grupos TABLE (IdMenu INT PRIMARY KEY);
INSERT INTO @Grupos (IdMenu)
SELECT Id_Menu FROM dbo.MenuDos
 WHERE Id_MenuPadre = 0
   AND Id_Menu IN (SELECT Id_MenuPadre FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) IN ('Agenda.aspx', 'BonosProponer.aspx'));

UPDATE pm SET Estado = '1'
  FROM dbo.PerfilMenu AS pm
  JOIN dbo.MenuDos    AS m ON m.Id_Menu = pm.id_Menu
 WHERE pm.IdPerfil = 41 AND pm.Estado = '0'
   AND (m.Id_Menu IN (SELECT IdMenu FROM @Grupos) OR m.Id_MenuPadre IN (SELECT IdMenu FROM @Grupos));
PRINT 'Filas ocultadas: ' + CONVERT(VARCHAR(10), @@ROWCOUNT);

COMMIT TRANSACTION;

/* Lo que le queda visible al perfil 41 (debe salir vacio) y por usuario (Departamento Medico). */
SELECT pm.id_Menu, m.Titulo, m.Href, pm.Estado
  FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
 WHERE pm.IdPerfil = 41 AND pm.Estado = '0';
SELECT um.Id_Menu, m.Titulo, m.Href, um.Estado
  FROM dbo.R_UsuarioMenu um JOIN dbo.MenuDos m ON m.Id_Menu = um.Id_Menu
 WHERE LTRIM(RTRIM(CONVERT(VARCHAR(50), um.Cod_Usuario))) = '1080' AND um.Estado = 'A';
GO

/* ------------------------------------------------------------ REVERSA ------
UPDATE pm SET Estado = '0'
  FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
 WHERE pm.IdPerfil = 41 AND pm.Estado = '1'
   AND (m.Id_Menu IN (20091, 20094) OR m.Id_MenuPadre IN (20091, 20094));
---------------------------------------------------------------------------- */
