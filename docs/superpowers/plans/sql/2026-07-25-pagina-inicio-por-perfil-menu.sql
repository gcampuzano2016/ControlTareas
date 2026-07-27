/* ============================================================================
   Registro en menu de la pantalla "Parametrizacion de pagina de inicio por perfil"
   Base: ReporTarea  — Padre: 20042 (Manejo de Perfiles)  — Perfiles: 1,2,18,19
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

DECLARE @Titulo   VARCHAR(200) = 'Pagina de Inicio por Perfil';
DECLARE @Href     VARCHAR(200) = 'ParametrizacionPerfilInicio.aspx';
DECLARE @Icono    VARCHAR(100) = 'fa fa-sign-in';
DECLARE @Padre    INT = 20042;
DECLARE @IdMenu   INT;

/* Insertar en MenuDos solo si no existe (por Href) */
SELECT @IdMenu = Id_Menu FROM dbo.MenuDos WHERE Href = @Href;

IF @IdMenu IS NULL
BEGIN
    INSERT INTO dbo.MenuDos (Titulo, Href, Class_Icon, Id_MenuPadre)
    VALUES (@Titulo, @Href, @Icono, @Padre);
    SET @IdMenu = SCOPE_IDENTITY();
END

/* Habilitar el menu (y su padre) para los perfiles admin, sin duplicar */
DECLARE @Perfiles TABLE (IdPerfil INT);
INSERT INTO @Perfiles (IdPerfil) VALUES (1),(2),(18),(19);

INSERT INTO dbo.PerfilMenu (IdPerfil, id_Menu, Estado)
SELECT p.IdPerfil, m.id_Menu, 0
FROM @Perfiles p
CROSS JOIN (SELECT @IdMenu AS id_Menu UNION SELECT @Padre) m
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PerfilMenu pm
    WHERE pm.IdPerfil = p.IdPerfil AND pm.id_Menu = m.id_Menu
);

/* Asegurar Estado=0 (activo) del item y su padre para esos perfiles */
UPDATE pm SET pm.Estado = 0
FROM dbo.PerfilMenu pm
JOIN @Perfiles p ON p.IdPerfil = pm.IdPerfil
WHERE pm.id_Menu IN (@IdMenu, @Padre);

SELECT Id_Menu = @IdMenu, Href = @Href;
