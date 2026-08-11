/* ============================================================================
   Registro en menu de "Administracion de Perfiles" (ParametrizacionPerfiles.aspx)
   Base: ReporTarea — Padre: 20042 (Manejo de Perfiles)
   Clonado de la fila modelo Id_Menu = 20077 (ParametrizacionUsuarios.aspx),
   que ya vive en el mismo grupo con Es_Opcion_de_Menu = 0.

   IMPORTANTE: la pantalla ParametrizacionPerfiles.aspx todavia NO esta
   publicada en el servidor web. Las filas de PerfilMenu para los perfiles
   2, 18 y 19 se insertan con Estado = '1' (OCULTA — la semantica de
   PerfilMenu esta invertida: '0' = visible, '1' = oculta). Se activaran
   despues del despliegue con 2026-08-10-perfiles-menu-activar.sql.

   Idempotente: si ya existe una fila de MenuDos con este Href, no vuelve
   a clonar ni a insertar filas de PerfilMenu duplicadas.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @Titulo VARCHAR(200) = 'Administracion de Perfiles';
DECLARE @Href   VARCHAR(200) = 'ParametrizacionPerfiles.aspx';
DECLARE @Modelo INT          = 20077;
DECLARE @IdMenu INT;

SELECT @IdMenu = Id_Menu FROM dbo.MenuDos WHERE Href = @Href;

IF @IdMenu IS NULL
BEGIN
    INSERT INTO dbo.MenuDos
        (Id_MenuPadre, Es_Opcion_de_Menu, Href, Class_Opcion, Class_Icon, Titulo,
         Descripcion, Es_Opcion_Publica, Orden_Opcion, Estado_Registro, Fecha_Creacion,
         Id_Usuario_Creacion, Ip_Creacion, Fecha_Modificacion, Id_Usuario_Modificacion,
         Ip_Modificacion, Estado_Logico_Registro)
    SELECT
         Id_MenuPadre, Es_Opcion_de_Menu, Href, Class_Opcion, Class_Icon, Titulo,
         Descripcion, Es_Opcion_Publica, Orden_Opcion, Estado_Registro, Fecha_Creacion,
         Id_Usuario_Creacion, Ip_Creacion, Fecha_Modificacion, Id_Usuario_Modificacion,
         Ip_Modificacion, Estado_Logico_Registro
    FROM dbo.MenuDos
    WHERE Id_Menu = @Modelo;

    SET @IdMenu = SCOPE_IDENTITY();

    UPDATE dbo.MenuDos
    SET Href = @Href, Titulo = @Titulo
    WHERE Id_Menu = @IdMenu;
END

DECLARE @Perfiles TABLE (IdPerfil INT);
INSERT INTO @Perfiles (IdPerfil) VALUES (2),(18),(19);

INSERT INTO dbo.PerfilMenu (IdPerfil, id_Menu, Estado)
SELECT p.IdPerfil, @IdMenu, '1'
FROM @Perfiles p
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PerfilMenu pm
    WHERE pm.IdPerfil = p.IdPerfil AND pm.id_Menu = @IdMenu
);

SELECT Id_Menu = @IdMenu, Href = @Href, Titulo = @Titulo;
GO
