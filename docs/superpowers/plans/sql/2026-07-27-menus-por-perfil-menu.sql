/* ============================================================================
   Registro en menu de "Parametrizacion de menus por perfil"
   Base: ReporTarea — Padre: 20042 (Manejo de Perfiles) — Perfiles: 2,18,19

   NOTA: se excluye el perfil 1 (Tecnico Especialista) a proposito. Hoy tiene el
   menu padre 20042 en Estado=1 (oculto), igual que las otras pantallas nuevas
   (Ventana de Aprobacion, Pagina de Inicio); incluirlo cambiaria lo que ven los
   tecnicos en produccion. Si se lo quiere habilitar, hacerlo desde esta misma
   pantalla una vez desplegada.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

DECLARE @Titulo VARCHAR(200) = 'Menus por Perfil';
DECLARE @Href   VARCHAR(200) = 'ParametrizacionMenuPerfil.aspx';
DECLARE @Icono  VARCHAR(100) = 'fa fa-sitemap';
DECLARE @Padre  INT = 20042;
DECLARE @IdMenu INT;

SELECT @IdMenu = Id_Menu FROM dbo.MenuDos WHERE Href = @Href;

IF @IdMenu IS NULL
BEGIN
    INSERT INTO dbo.MenuDos (Titulo, Href, Class_Icon, Id_MenuPadre)
    VALUES (@Titulo, @Href, @Icono, @Padre);
    SET @IdMenu = SCOPE_IDENTITY();
END

DECLARE @Perfiles TABLE (IdPerfil INT);
INSERT INTO @Perfiles (IdPerfil) VALUES (2),(18),(19);

INSERT INTO dbo.PerfilMenu (IdPerfil, id_Menu, Estado)
SELECT p.IdPerfil, m.id_Menu, 0
FROM @Perfiles p
CROSS JOIN (SELECT @IdMenu AS id_Menu UNION SELECT @Padre) m
WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
                  WHERE pm.IdPerfil = p.IdPerfil AND pm.id_Menu = m.id_Menu);

UPDATE pm SET pm.Estado = 0
FROM dbo.PerfilMenu pm
JOIN @Perfiles p ON p.IdPerfil = pm.IdPerfil
WHERE pm.id_Menu IN (@IdMenu, @Padre);

SELECT Id_Menu = @IdMenu, Href = @Href;
