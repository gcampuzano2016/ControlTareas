/* ============================================================================
   Activacion de "Administracion de Perfiles" (ParametrizacionPerfiles.aspx)
   Base: ReporTarea

   Ejecutar SOLO despues de publicar ParametrizacionPerfiles.aspx en el
   servidor web. Pone en Estado = '0' (VISIBLE — semantica invertida de
   PerfilMenu) las filas de los perfiles 2, 18 y 19 que
   2026-08-10-perfiles-menu.sql dejo en Estado = '1' (oculta).

   Identifica la opcion por su Href, no por un Id_Menu fijo.
   Seguro de ejecutar mas de una vez (UPDATE idempotente).
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @Href VARCHAR(200) = 'ParametrizacionPerfiles.aspx';
DECLARE @IdMenu INT;

SELECT @IdMenu = Id_Menu FROM dbo.MenuDos WHERE Href = @Href;

IF @IdMenu IS NULL
BEGIN
    RAISERROR('No existe una fila de MenuDos con Href = %s. No se actualizo nada.', 16, 1, @Href);
END
ELSE
BEGIN
    UPDATE dbo.PerfilMenu
    SET Estado = '0'
    WHERE id_Menu = @IdMenu
      AND IdPerfil IN (2, 18, 19);

    SELECT id_Menu, IdPerfil, Estado
    FROM dbo.PerfilMenu
    WHERE id_Menu = @IdMenu
    ORDER BY IdPerfil;
END
GO
