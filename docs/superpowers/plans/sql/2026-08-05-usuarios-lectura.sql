/* ============================================================================
   Administracion de usuarios — bitacora y SPs de lectura
   Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- 1) Bitacora de cambios ---------- */
IF OBJECT_ID('dbo.R_UsuarioBitacora','U') IS NULL
BEGIN
    CREATE TABLE dbo.R_UsuarioBitacora
    (
        Id_Bitacora      INT IDENTITY(1,1) NOT NULL,
        Id_Usuario       NUMERIC(5)    NOT NULL,
        Accion           VARCHAR(30)   NOT NULL,
        Detalle          VARCHAR(2000) NOT NULL,
        Usuario_Registro VARCHAR(50)   NOT NULL,
        Fecha_Registro   DATETIME      NOT NULL CONSTRAINT DF_R_UsuarioBitacora_Fecha DEFAULT (GETDATE()),
        CONSTRAINT PK_R_UsuarioBitacora PRIMARY KEY (Id_Bitacora)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_R_UsuarioBitacora_Usuario' AND object_id = OBJECT_ID('dbo.R_UsuarioBitacora'))
    CREATE INDEX IX_R_UsuarioBitacora_Usuario ON dbo.R_UsuarioBitacora (Id_Usuario, Fecha_Registro DESC);
GO

/* ---------- 2) Listado de usuarios (activos e inactivos) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarUsuariosAdmin','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarUsuariosAdmin;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarUsuariosAdmin
    @Filtro VARCHAR(100) = ''
AS
BEGIN
    SET NOCOUNT ON;
    SET @Filtro = LTRIM(RTRIM(ISNULL(@Filtro,'')));

    SELECT
        u.Id_Usuario,
        u.Cod_Usuario,
        u.Nom_Usuario,
        u.Log_Usuario,
        E_Mail         = ISNULL(u.E_Mail,''),
        Cedula         = ISNULL(u.Cedula,''),
        Departamento   = ISNULL(u.Departamento,''),
        Empresa        = ISNULL(u.Empresa,''),
        Cod_Sap        = ISNULL(u.Cod_Sap,''),
        Cod_Jefe_Inm   = ISNULL(u.Cod_Jefe_Inm,''),
        MailCodJefeInm = ISNULL(u.MailCodJefeInm,''),
        u.Id_Perfil,
        NombrePerfil   = ISNULL(p.NombrePerfil,'Sin perfil'),
        u.Usuario_Estado
    FROM dbo.R_Usuarios u
    LEFT JOIN dbo.Perfiles p ON p.IdPerfiles = u.Id_Perfil
    WHERE
    (
        @Filtro = ''
        OR u.Nom_Usuario LIKE '%' + @Filtro + '%'
        OR u.Cod_Usuario LIKE '%' + @Filtro + '%'
        OR ISNULL(u.Cedula,'') LIKE '%' + @Filtro + '%'
    )
    ORDER BY u.Nom_Usuario;
END
GO

/* ---------- 3) Historial de un usuario ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarBitacoraUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarBitacoraUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarBitacoraUsuario
    @Id_Usuario NUMERIC(5)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (50)
        b.Accion,
        b.Detalle,
        b.Usuario_Registro,
        Fecha_Registro = CONVERT(VARCHAR(19), b.Fecha_Registro, 120)
    FROM dbo.R_UsuarioBitacora b
    WHERE b.Id_Usuario = @Id_Usuario
    ORDER BY b.Fecha_Registro DESC;
END
GO
