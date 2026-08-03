/* ============================================================================
   Sidebar: el menu ahora es (menus del perfil) UNION (extras del usuario).
   Con @CodUsuario NULL el resultado es identico al de antes de este cambio.
   Se elimina el IF EXISTS original por redundante: si el perfil no tiene filas
   en PerfilMenu el SELECT no devuelve nada igual, y estorbaba para que un
   usuario sin menus de perfil pudiera ver sus extras.
   Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_ConsultarMenuPerfilUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ConsultarMenuPerfilUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_ConsultarMenuPerfilUsuario
    @tipoPerfil AS INT = 0,
    @CodUsuario AS VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* Las seis columnas y su orden son contrato: DaoMenuDos.cs las lee por nombre. */
    SELECT P.id_Menu, M.Titulo, P.Estado, M.Id_MenuPadre, M.Class_Icon, M.Href
    FROM dbo.PerfilMenu P
    INNER JOIN dbo.MenuDos M ON P.id_Menu = M.Id_Menu
    WHERE P.IdPerfil = @tipoPerfil AND P.Estado = 0

    UNION

    SELECT UM.Id_Menu, M.Titulo, 0 AS Estado, M.Id_MenuPadre, M.Class_Icon, M.Href
    FROM dbo.R_UsuarioMenu UM
    INNER JOIN dbo.MenuDos M ON UM.Id_Menu = M.Id_Menu
    WHERE @CodUsuario IS NOT NULL
      AND UM.Cod_Usuario = @CodUsuario
      AND UM.Estado = 'A';

    SET NOCOUNT OFF;
END
GO
