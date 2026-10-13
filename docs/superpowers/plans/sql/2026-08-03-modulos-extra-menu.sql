/* ============================================================================
   Registro en menu de "Modulos por Usuario"
   Base: ReporTarea — Padre: 20042 (Manejo de Perfiles) — Perfiles: 2,18,19
   Se excluye el perfil 1 a proposito, igual que en el registro de
   ParametrizacionMenuPerfil.aspx.
   Idempotente.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @Titulo VARCHAR(200) = 'Modulos por Usuario';
DECLARE @Href   VARCHAR(200) = 'ParametrizacionMenuUsuario.aspx';
DECLARE @Icono  VARCHAR(200) = 'fa fa-user-plus';
DECLARE @Padre  INT          = 20042;
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
GO
