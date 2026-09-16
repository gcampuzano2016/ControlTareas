/* ============================================================================
   Fix: el combo de perfiles leia de R_Perfil (solo 5 perfiles, nombres que no
   corresponden). El catalogo que mapea con R_Usuarios.Id_Perfil es dbo.Perfiles.
   Se conservan los nombres de columna Id_Perfil / Nombre para no tocar C# ni JS.
   Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_ListarPerfiles','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPerfiles;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarPerfiles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        Id_Perfil = IdPerfiles,
        Nombre    = NombrePerfil
    FROM dbo.Perfiles
    WHERE ISNULL(Estado,1) = 1
    ORDER BY NombrePerfil;
END
GO

/* verificacion */
EXEC dbo.Sp_RTA_ListarPerfiles;
GO
