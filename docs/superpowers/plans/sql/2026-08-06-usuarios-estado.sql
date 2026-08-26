/* ============================================================================
   Administracion de usuarios — inactivar / activar por EstadoUsuario
   Base: ReporTarea

   Semantica de EstadoUsuario (la que ya usan RTA_ConsultaLike,
   Sp_RTAConsultaUsuarioPorJefe, Sp_RTAConsultaUsuarioPorJefeSap y
   Sp_RTAListaSolicitud):
       EstadoUsuario IS NULL  -> el usuario aparece en selectores de jefe,
                                 autocompletado y listas de solicitudes
       EstadoUsuario NOT NULL -> no aparece
   Por eso "activar" escribe NULL y "inactivar" escribe 0. Los 6 usuarios que
   hoy tienen el valor huerfano 1 se normalizan a 0 al inactivarlos.

   OJO: esta columna NO controla el inicio de sesion. El login autentica contra
   Active Directory (Login.aspx.cs:109) y no consulta ninguna columna de estado.

   Este script REEMPLAZA Sp_RTA_ListarUsuariosAdmin de
   2026-08-05-usuarios-lectura.sql (le agrega la columna EstadoUsuario).
   Aplicarlo despues de aquel.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- 1) Listado: ahora tambien devuelve EstadoUsuario ---------- */
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
        u.Usuario_Estado,
        /* cadena vacia = activo (NULL en la tabla); cualquier otro valor = inactivo */
        EstadoUsuario  = ISNULL(CAST(u.EstadoUsuario AS VARCHAR(10)),'')
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

/* ---------- 2) Inactivar / activar ---------- */
IF OBJECT_ID('dbo.Sp_RTA_CambiarEstadoUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_CambiarEstadoUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_CambiarEstadoUsuario
    @Id_Usuario      NUMERIC(5),
    @Inactivar       BIT,
    @UsuarioRegistro VARCHAR(50) = 'SISTEMA'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    IF NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios WHERE Id_Usuario = @Id_Usuario)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El usuario indicado no existe.'; RETURN;
    END

    IF @Inactivar IS NULL
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'No se indico si se debe activar o inactivar.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Actual INT, @Nuevo INT;
        SELECT @Actual = EstadoUsuario FROM dbo.R_Usuarios WHERE Id_Usuario = @Id_Usuario;
        SET @Nuevo = CASE WHEN @Inactivar = 1 THEN 0 ELSE NULL END;

        /* Sin cambios: ya estaba activo y se pide activar, o ya tenia
           exactamente el mismo valor de inactivo. */
        IF (@Actual IS NULL AND @Nuevo IS NULL) OR (@Actual = @Nuevo)
        BEGIN
            COMMIT TRANSACTION;
            SELECT Respuestas = 1,
                   Mensaje = CASE WHEN @Inactivar = 1
                                  THEN 'El usuario ya estaba inactivo.'
                                  ELSE 'El usuario ya estaba activo.' END;
            RETURN;
        END

        UPDATE dbo.R_Usuarios SET EstadoUsuario = @Nuevo WHERE Id_Usuario = @Id_Usuario;

        DECLARE @Detalle VARCHAR(2000) =
            'Estado en selectores: [' + ISNULL(CAST(@Actual AS VARCHAR(10)),'activo') + '] -> ['
                                      + ISNULL(CAST(@Nuevo  AS VARCHAR(10)),'activo') + ']';

        INSERT INTO dbo.R_UsuarioBitacora (Id_Usuario, Accion, Detalle, Usuario_Registro)
        VALUES (@Id_Usuario,
                CASE WHEN @Inactivar = 1 THEN 'INACTIVACION' ELSE 'ACTIVACION' END,
                @Detalle,
                @UsuarioRegistro);

        SET @Respuestas = 1;
        SET @Mensaje = CASE WHEN @Inactivar = 1
                            THEN 'Usuario inactivado correctamente.'
                            ELSE 'Usuario activado correctamente.' END;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo cambiar el estado del usuario: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO
