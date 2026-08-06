/* ============================================================================
   Administracion de usuarios — SPs de escritura
   Base: ReporTarea
   Los parametros se declaran mas largos que las columnas a proposito: asi un
   valor excedido se rechaza con mensaje en vez de truncarse en silencio.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- 1) Actualizar datos del usuario ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ActualizarUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ActualizarUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_ActualizarUsuario
    @Id_Usuario      NUMERIC(5),
    @Nom_Usuario     VARCHAR(300) = '',
    @E_Mail          VARCHAR(300) = '',
    @Cedula          VARCHAR(300) = '',
    @Departamento    VARCHAR(300) = '',
    @Empresa         VARCHAR(300) = '',
    @Cod_Sap         VARCHAR(300) = '',
    @Cod_Jefe_Inm    VARCHAR(300) = '',
    @MailCodJefeInm  VARCHAR(300) = '',
    @UsuarioRegistro VARCHAR(50)  = 'SISTEMA'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    /* normalizar entradas */
    SET @Nom_Usuario    = LTRIM(RTRIM(ISNULL(@Nom_Usuario,'')));
    SET @E_Mail         = LTRIM(RTRIM(ISNULL(@E_Mail,'')));
    SET @Cedula         = LTRIM(RTRIM(ISNULL(@Cedula,'')));
    SET @Departamento   = LTRIM(RTRIM(ISNULL(@Departamento,'')));
    SET @Empresa        = LTRIM(RTRIM(ISNULL(@Empresa,'')));
    SET @Cod_Sap        = LTRIM(RTRIM(ISNULL(@Cod_Sap,'')));
    SET @Cod_Jefe_Inm   = LTRIM(RTRIM(ISNULL(@Cod_Jefe_Inm,'')));
    SET @MailCodJefeInm = LTRIM(RTRIM(ISNULL(@MailCodJefeInm,'')));

    IF NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios WHERE Id_Usuario = @Id_Usuario)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El usuario indicado no existe.'; RETURN;
    END

    IF @Nom_Usuario = ''
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El nombre del usuario es obligatorio.'; RETURN;
    END

    /* longitudes reales de las columnas */
    IF LEN(@Nom_Usuario) > 100 OR LEN(@E_Mail) > 100 OR LEN(@Cedula) > 32
       OR LEN(@Departamento) > 128 OR LEN(@Empresa) > 50 OR LEN(@Cod_Sap) > 50
       OR LEN(@Cod_Jefe_Inm) > 100 OR LEN(@MailCodJefeInm) > 100
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Alguno de los datos excede el largo permitido.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @oNom VARCHAR(100), @oMail VARCHAR(100), @oCed VARCHAR(32),
                @oDep VARCHAR(128), @oEmp VARCHAR(50), @oSap VARCHAR(50),
                @oJefe VARCHAR(100), @oMailJefe VARCHAR(100);

        SELECT @oNom      = ISNULL(Nom_Usuario,''),
               @oMail     = ISNULL(E_Mail,''),
               @oCed      = ISNULL(Cedula,''),
               @oDep      = ISNULL(Departamento,''),
               @oEmp      = ISNULL(Empresa,''),
               @oSap      = ISNULL(Cod_Sap,''),
               @oJefe     = ISNULL(Cod_Jefe_Inm,''),
               @oMailJefe = ISNULL(MailCodJefeInm,'')
        FROM dbo.R_Usuarios WHERE Id_Usuario = @Id_Usuario;

        /* La collation de la base (SQL_Latin1_General_CP1_CI_AS) es case-insensitive
           y no distingue acentos. Se fuerza collation binaria en ambos lados de cada
           comparacion para que un cambio de mayusculas/minusculas o de acentos (p.ej.
           en Cod_Sap o Cod_Jefe_Inm, que son codigos de sistemas externos) SI se
           detecte como cambio real. No quitar esta collation. */
        DECLARE @Detalle VARCHAR(2000) = '';
        IF @oNom      COLLATE Latin1_General_BIN <> @Nom_Usuario    COLLATE Latin1_General_BIN SET @Detalle = @Detalle + 'Nombre: [' + @oNom + '] -> [' + @Nom_Usuario + ']; ';
        IF @oMail     COLLATE Latin1_General_BIN <> @E_Mail         COLLATE Latin1_General_BIN SET @Detalle = @Detalle + 'Correo: [' + @oMail + '] -> [' + @E_Mail + ']; ';
        IF @oCed      COLLATE Latin1_General_BIN <> @Cedula         COLLATE Latin1_General_BIN SET @Detalle = @Detalle + 'Cedula: [' + @oCed + '] -> [' + @Cedula + ']; ';
        IF @oDep      COLLATE Latin1_General_BIN <> @Departamento   COLLATE Latin1_General_BIN SET @Detalle = @Detalle + 'Departamento: [' + @oDep + '] -> [' + @Departamento + ']; ';
        IF @oEmp      COLLATE Latin1_General_BIN <> @Empresa        COLLATE Latin1_General_BIN SET @Detalle = @Detalle + 'Empresa: [' + @oEmp + '] -> [' + @Empresa + ']; ';
        IF @oSap      COLLATE Latin1_General_BIN <> @Cod_Sap        COLLATE Latin1_General_BIN SET @Detalle = @Detalle + 'Cod SAP: [' + @oSap + '] -> [' + @Cod_Sap + ']; ';
        IF @oJefe     COLLATE Latin1_General_BIN <> @Cod_Jefe_Inm   COLLATE Latin1_General_BIN SET @Detalle = @Detalle + 'Jefe inmediato: [' + @oJefe + '] -> [' + @Cod_Jefe_Inm + ']; ';
        IF @oMailJefe COLLATE Latin1_General_BIN <> @MailCodJefeInm COLLATE Latin1_General_BIN SET @Detalle = @Detalle + 'Correo jefe: [' + @oMailJefe + '] -> [' + @MailCodJefeInm + ']; ';

        IF @Detalle = ''
        BEGIN
            COMMIT TRANSACTION;
            SELECT Respuestas = 1, Mensaje = 'No hubo cambios que guardar.'; RETURN;
        END

        UPDATE dbo.R_Usuarios
        SET Nom_Usuario    = @Nom_Usuario,
            E_Mail         = NULLIF(@E_Mail,''),
            Cedula         = NULLIF(@Cedula,''),
            Departamento   = NULLIF(@Departamento,''),
            Empresa        = NULLIF(@Empresa,''),
            Cod_Sap        = NULLIF(@Cod_Sap,''),
            Cod_Jefe_Inm   = NULLIF(@Cod_Jefe_Inm,''),
            MailCodJefeInm = NULLIF(@MailCodJefeInm,'')
        WHERE Id_Usuario = @Id_Usuario;

        INSERT INTO dbo.R_UsuarioBitacora (Id_Usuario, Accion, Detalle, Usuario_Registro)
        VALUES (@Id_Usuario, 'EDICION', LEFT(@Detalle,2000), @UsuarioRegistro);

        SET @Respuestas = 1;
        SET @Mensaje = 'Datos del usuario guardados correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar los datos del usuario: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO

/* ---------- 2) Restablecer contrasena ---------- */
IF OBJECT_ID('dbo.Sp_RTA_RestablecerPassword','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_RestablecerPassword;
GO
CREATE PROCEDURE dbo.Sp_RTA_RestablecerPassword
    @Id_Usuario      NUMERIC(5),
    @HashMd5         VARCHAR(50) = '',
    @UsuarioRegistro VARCHAR(50) = 'SISTEMA'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    SET @HashMd5 = LTRIM(RTRIM(ISNULL(@HashMd5,'')));

    IF NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios WHERE Id_Usuario = @Id_Usuario)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El usuario indicado no existe.'; RETURN;
    END

    /* Defensa en profundidad: el login compara el hash tal cual. Si aqui llegara
       una contrasena en claro, el usuario quedaria sin poder entrar. */
    IF LEN(@HashMd5) <> 32 OR @HashMd5 LIKE '%[^0-9a-fA-F]%'
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'La clave recibida no tiene el formato esperado.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.R_Usuarios SET Pass_Usuario = @HashMd5 WHERE Id_Usuario = @Id_Usuario;

        /* Nunca se registra la contrasena ni el hash. */
        INSERT INTO dbo.R_UsuarioBitacora (Id_Usuario, Accion, Detalle, Usuario_Registro)
        VALUES (@Id_Usuario, 'RESET_PASSWORD', 'Contrasena restablecida', @UsuarioRegistro);

        SET @Respuestas = 1;
        SET @Mensaje = 'Contrasena restablecida correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo restablecer la contrasena: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO
