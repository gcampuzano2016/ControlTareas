/* ============================================================================
   Pantalla: Administracion de usuarios (ParametrizacionUsuarios.aspx)

   1. Columna TelefonosEmergencia en R_Usuarios
   2. Sp_RTA_ListarDepartamentos : alimenta el combo de departamentos
   3. Sp_RTA_ListarUsuariosAdmin : devuelve el campo nuevo
   4. Sp_RTA_ActualizarUsuario   : guarda y audita el campo nuevo

   Las secciones estan en el orden en que deben ejecutarse.

   Se puede ejecutar varias veces sin efecto adicional.
   Base: ReporTarea
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ---------------------------------------------------------------- 1. columna */
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'R_Usuarios' AND COLUMN_NAME = 'TelefonosEmergencia')
BEGIN
    ALTER TABLE dbo.R_Usuarios ADD TelefonosEmergencia VARCHAR(100) NULL;
    PRINT 'R_Usuarios.TelefonosEmergencia creada.';
END
ELSE
    PRINT 'R_Usuarios.TelefonosEmergencia ya existia.';
GO

/* ------------------------------------------------- 2. combo de departamentos */
IF OBJECT_ID('dbo.Sp_RTA_ListarDepartamentos') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarDepartamentos;
GO

/* Los departamentos que existen hoy en los usuarios. No hay tabla catalogo:
   la lista sale de los propios datos, sin vacios y sin repetir.

   Limitacion aceptada: esto NO es un catalogo. Un departamento que ningun
   usuario tiene todavia no aparece en el combo, y no hay forma de crearlo
   desde la pantalla. Para dar de alta un area nueva hay que asignarla por SQL
   a un usuario primero; desde ahi ya sale en la lista para todos los demas. */
CREATE PROCEDURE dbo.Sp_RTA_ListarDepartamentos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT Departamento = LTRIM(RTRIM(Departamento))
    FROM dbo.R_Usuarios
    WHERE ISNULL(LTRIM(RTRIM(Departamento)), '') <> ''
    ORDER BY 1;
END
GO
PRINT 'Sp_RTA_ListarDepartamentos creado.';
GO

/* --------------------------------------------------- 3. listado de usuarios */
IF OBJECT_ID('dbo.Sp_RTA_ListarUsuariosAdmin') IS NOT NULL
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
        E_Mail              = ISNULL(u.E_Mail,''),
        Cedula              = ISNULL(u.Cedula,''),
        Departamento        = ISNULL(u.Departamento,''),
        Empresa             = ISNULL(u.Empresa,''),
        Cod_Sap             = ISNULL(u.Cod_Sap,''),
        Cod_Jefe_Inm        = ISNULL(u.Cod_Jefe_Inm,''),
        MailCodJefeInm      = ISNULL(u.MailCodJefeInm,''),
        TelefonosEmergencia = ISNULL(u.TelefonosEmergencia,''),
        u.Id_Perfil,
        NombrePerfil        = ISNULL(p.NombrePerfil,'Sin perfil'),
        u.Usuario_Estado,
        /* cadena vacia = activo (NULL en la tabla); cualquier otro valor = inactivo */
        EstadoUsuario       = ISNULL(CAST(u.EstadoUsuario AS VARCHAR(10)),'')
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
PRINT 'Sp_RTA_ListarUsuariosAdmin actualizado.';
GO

/* ------------------------------------------------ 4. actualizacion + bitacora */
IF OBJECT_ID('dbo.Sp_RTA_ActualizarUsuario') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ActualizarUsuario;
GO

CREATE PROCEDURE dbo.Sp_RTA_ActualizarUsuario
    @Id_Usuario          NUMERIC(5),
    @Nom_Usuario         VARCHAR(300) = '',
    @E_Mail              VARCHAR(300) = '',
    @Cedula              VARCHAR(300) = '',
    @Departamento        VARCHAR(300) = '',
    @Empresa             VARCHAR(300) = '',
    @Cod_Sap             VARCHAR(300) = '',
    @Cod_Jefe_Inm        VARCHAR(300) = '',
    @MailCodJefeInm      VARCHAR(300) = '',
    @TelefonosEmergencia VARCHAR(300) = '',
    @UsuarioRegistro     VARCHAR(50)  = 'SISTEMA'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    /* normalizar entradas */
    SET @Nom_Usuario         = LTRIM(RTRIM(ISNULL(@Nom_Usuario,'')));
    SET @E_Mail              = LTRIM(RTRIM(ISNULL(@E_Mail,'')));
    SET @Cedula              = LTRIM(RTRIM(ISNULL(@Cedula,'')));
    SET @Departamento        = LTRIM(RTRIM(ISNULL(@Departamento,'')));
    SET @Empresa             = LTRIM(RTRIM(ISNULL(@Empresa,'')));
    SET @Cod_Sap             = LTRIM(RTRIM(ISNULL(@Cod_Sap,'')));
    SET @Cod_Jefe_Inm        = LTRIM(RTRIM(ISNULL(@Cod_Jefe_Inm,'')));
    SET @MailCodJefeInm      = LTRIM(RTRIM(ISNULL(@MailCodJefeInm,'')));
    SET @TelefonosEmergencia = LTRIM(RTRIM(ISNULL(@TelefonosEmergencia,'')));

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
       OR LEN(@TelefonosEmergencia) > 100
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Alguno de los datos excede el largo permitido.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @oNom VARCHAR(100), @oMail VARCHAR(100), @oCed VARCHAR(32),
                @oDep VARCHAR(128), @oEmp VARCHAR(50), @oSap VARCHAR(50),
                @oJefe VARCHAR(100), @oMailJefe VARCHAR(100), @oTelEmer VARCHAR(100);

        SELECT @oNom      = ISNULL(Nom_Usuario,''),
               @oMail     = ISNULL(E_Mail,''),
               @oCed      = ISNULL(Cedula,''),
               @oDep      = ISNULL(Departamento,''),
               @oEmp      = ISNULL(Empresa,''),
               @oSap      = ISNULL(Cod_Sap,''),
               @oJefe     = ISNULL(Cod_Jefe_Inm,''),
               @oMailJefe = ISNULL(MailCodJefeInm,''),
               @oTelEmer  = ISNULL(TelefonosEmergencia,'')
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
        IF @oTelEmer  COLLATE Latin1_General_BIN <> @TelefonosEmergencia COLLATE Latin1_General_BIN SET @Detalle = @Detalle + 'Telefonos de emergencia: [' + @oTelEmer + '] -> [' + @TelefonosEmergencia + ']; ';

        IF @Detalle = ''
        BEGIN
            COMMIT TRANSACTION;
            SELECT Respuestas = 1, Mensaje = 'No hubo cambios que guardar.'; RETURN;
        END

        UPDATE dbo.R_Usuarios
        SET Nom_Usuario         = @Nom_Usuario,
            E_Mail              = NULLIF(@E_Mail,''),
            Cedula              = NULLIF(@Cedula,''),
            Departamento        = NULLIF(@Departamento,''),
            Empresa             = NULLIF(@Empresa,''),
            Cod_Sap             = NULLIF(@Cod_Sap,''),
            Cod_Jefe_Inm        = NULLIF(@Cod_Jefe_Inm,''),
            MailCodJefeInm      = NULLIF(@MailCodJefeInm,''),
            TelefonosEmergencia = NULLIF(@TelefonosEmergencia,'')
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
PRINT 'Sp_RTA_ActualizarUsuario actualizado.';
GO

/* ------------------------------------------------------------------ opcional
   Normalizar el departamento escrito distinto, para que el combo no ofrezca
   dos veces el mismo. Descomentar y ejecutar cuando se quiera aplicar.

   Los datos se revisaron el 2026-08-24 con
   2026-08-24-departamentos-conteo.sql. De las variantes que parecian
   duplicadas, solo una lo era.

   GTH- -> GTH: es un typo, afecta a una fila. La evidencia es que el jefe
   inmediato de esa persona (3296891) esta en GTH.
*/

-- UPDATE dbo.R_Usuarios SET Departamento = 'GTH' WHERE LTRIM(RTRIM(Departamento)) = 'GTH-';

/* ADMINISTRACION y ADMINISTRATIVO NO se unifican, aunque el prefijo comun
   invite a hacerlo. Son un usuario cada uno, de empresas distintas y con
   jefes distintos:

     ADMINISTRACION   MORA MOREJON DEVORA ESTELA   DOS       jefe 2184695
     ADMINISTRATIVO   Karina Ruiz Palacios         AGILITY   jefe 1075

   No hay nada que indique que sean la misma area, y elegir un destino seria
   inventarlo: unificarlas le cambia el area a una persona real por una
   coincidencia de las primeras seis letras. Si alguien confirma con GTH que
   son la misma, ahi se decide el destino y se agrega el UPDATE.

   AREA OPERACIONES tampoco se toca. Es el unico nombre con prefijo
   redundante, pero no colisiona con ningun OPERACIONES: cambiarlo seria
   cosmetico.

   Aparte: 45 usuarios no tienen departamento (43 activos, pero solo 6
   visibles en selectores). El combo les sale en blanco. No es un duplicado,
   es un dato que nunca se lleno.
   ------------------------------------------------------------------------- */
