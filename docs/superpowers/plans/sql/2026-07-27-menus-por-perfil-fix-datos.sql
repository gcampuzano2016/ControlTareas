/* ============================================================================
   Arreglo one-off: activar el padre de todo hijo activo (invariante).
   Idempotente. Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

/* Conteo ANTES (submenus activos cuyo padre no esta activo para el perfil) */
SELECT CasosAntes = COUNT(*)
FROM dbo.PerfilMenu pm
JOIN dbo.MenuDos child ON child.Id_Menu = pm.id_Menu
WHERE pm.Estado = 0 AND ISNULL(child.Id_MenuPadre,0) <> 0
  AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu p2
                  WHERE p2.IdPerfil = pm.IdPerfil AND p2.id_Menu = child.Id_MenuPadre AND p2.Estado = 0);

/* 1) Insertar la fila del padre (Estado=0) cuando no existe */
INSERT INTO dbo.PerfilMenu (IdPerfil, id_Menu, Estado)
SELECT DISTINCT pm.IdPerfil, child.Id_MenuPadre, 0
FROM dbo.PerfilMenu pm
JOIN dbo.MenuDos child ON child.Id_Menu = pm.id_Menu
WHERE pm.Estado = 0 AND ISNULL(child.Id_MenuPadre,0) <> 0
  AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu p2
                  WHERE p2.IdPerfil = pm.IdPerfil AND p2.id_Menu = child.Id_MenuPadre);

/* 2) Reactivar la fila del padre cuando existe pero esta inactiva */
UPDATE p2 SET p2.Estado = 0
FROM dbo.PerfilMenu p2
WHERE p2.Estado <> 0
  AND EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
              JOIN dbo.MenuDos child ON child.Id_Menu = pm.id_Menu
              WHERE pm.Estado = 0 AND pm.IdPerfil = p2.IdPerfil AND child.Id_MenuPadre = p2.id_Menu);

/* Conteo DESPUES (debe ser 0) */
SELECT CasosDespues = COUNT(*)
FROM dbo.PerfilMenu pm
JOIN dbo.MenuDos child ON child.Id_Menu = pm.id_Menu
WHERE pm.Estado = 0 AND ISNULL(child.Id_MenuPadre,0) <> 0
  AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu p2
                  WHERE p2.IdPerfil = pm.IdPerfil AND p2.id_Menu = child.Id_MenuPadre AND p2.Estado = 0);
