/* ============================================================================
   Etapa 2 de CB-GAP-POL-01: las piezas compartidas por Permisos y Vacaciones.

   1. R_Usuarios.Cargo                : el cargo del firmante, que no existia
   2. VacacionesFirma                 : una fila por solicitud y rol firmante
   3. Sp_RTA_GuardarFirmaSolicitud     : registra una firma
   4. Sp_RTA_ListarFirmasSolicitud     : las firmas de una solicitud, para el PDF
   5. Sp_RTA_ListarUsuariosAdmin       : devuelve el Cargo
   6. Sp_RTA_ActualizarUsuario         : guarda y audita el Cargo

   Las secciones estan en el orden en que deben ejecutarse.
   Se puede ejecutar varias veces sin efecto adicional.
   Base: ReporTarea
   ============================================================================

   Por que una tabla aparte y no columnas en Vacaciones
   ----------------------------------------------------
   Vacaciones tiene UN solo par UsuarioAprobo/FechaAprobacion. La especificacion
   pide tres firmas —colaborador, jefe inmediato y Talento Humano— y en Permisos
   con recuperacion el jefe firma dos veces. No hay forma de meter eso en un par
   de columnas sin inventar UsuarioAprobo2, UsuarioAprobo3 y demas. Una tabla
   hija con una fila por firma lo resuelve y no toca nada de lo que ya funciona.

   Por que la identidad se congela y no se resuelve por JOIN
   ---------------------------------------------------------
   Nombre, cargo y cedula se copian a la fila de firma en el momento de firmar,
   en vez de leerse de R_Usuarios cada vez que se abre el PDF. Si manana esa
   persona cambia de cargo o le corrigen el nombre, el documento que ya firmo
   tiene que seguir diciendo lo que decia cuando lo firmo. Un JOIN reescribiria
   la historia en silencio, y de eso se trata justamente tener una firma.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ------------------------------------------------------- 1. cargo del usuario */
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'R_Usuarios' AND COLUMN_NAME = 'Cargo')
BEGIN
    ALTER TABLE dbo.R_Usuarios ADD Cargo VARCHAR(128) NULL;
    PRINT 'R_Usuarios.Cargo creada. Queda NULL hasta que Talento Humano la llene.';
END
ELSE
    PRINT 'R_Usuarios.Cargo ya existia.';
GO

/* ----------------------------------------------------------- 2. tabla de firmas */
IF OBJECT_ID('dbo.VacacionesFirma') IS NULL
BEGIN
    CREATE TABLE dbo.VacacionesFirma
    (
        IdFirma      INT IDENTITY(1,1) NOT NULL,

        /* La solicitud. Sin clave foranea, como el resto del esquema: este modelo
           no tiene ninguna y agregar una sola aqui daria una falsa sensacion de
           integridad referencial. */
        IdVacaciones INT           NOT NULL,

        /* COLABORADOR | JEFE | GTH. Secuencia es 1 salvo la reconfirmacion del
           jefe cuando hubo recuperacion, que es la segunda firma del mismo rol
           (Permisos, etapa 4). */
        Rol          VARCHAR(20)   NOT NULL,
        Secuencia    INT           NOT NULL CONSTRAINT DF_VacacionesFirma_Secuencia DEFAULT (1),

        /* Declaracion de conformidad: la casilla obligatoria de la especificacion. */
        Decision     VARCHAR(10)   NOT NULL,
        Comentario   VARCHAR(500)  NULL,

        /* El trazo dibujado o la imagen subida, tal cual, para incrustarla en el
           PDF sin regenerarla. Va en la tabla y no en disco porque es parte del
           registro legal: si se separa, un respaldo de la base queda con firmas
           que apuntan a archivos que ya no estan. */
        Trazo        VARBINARY(MAX) NULL,
        TrazoTipo    VARCHAR(30)    NULL,

        /* Identidad congelada al momento de firmar. Ver el encabezado. */
        Cod_Usuario  VARCHAR(64)   NOT NULL,
        Nombre       VARCHAR(128)  NOT NULL,
        Cargo        VARCHAR(128)  NULL,
        Cedula       VARCHAR(32)   NULL,

        /* Trazabilidad. */
        FechaFirma   DATETIME2(0)  NOT NULL CONSTRAINT DF_VacacionesFirma_Fecha DEFAULT (SYSDATETIME()),
        Ip           VARCHAR(45)   NULL,
        Dispositivo  VARCHAR(300)  NULL,

        CONSTRAINT PK_VacacionesFirma PRIMARY KEY (IdFirma),

        /* Un rol no puede firmar dos veces el mismo paso. Es la unica salvaguarda
           que evita firmas duplicadas por un doble clic o un reenvio. */
        CONSTRAINT UQ_VacacionesFirma_Paso UNIQUE (IdVacaciones, Rol, Secuencia)
    );

    CREATE INDEX IX_VacacionesFirma_Solicitud ON dbo.VacacionesFirma (IdVacaciones);

    PRINT 'VacacionesFirma creada.';
END
ELSE
    PRINT 'VacacionesFirma ya existia.';
GO

/* ------------------------------------------------------------ 3. guardar firma */
IF OBJECT_ID('dbo.Sp_RTA_GuardarFirmaSolicitud') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarFirmaSolicitud;
GO

CREATE PROCEDURE dbo.Sp_RTA_GuardarFirmaSolicitud
    @IdVacaciones INT,
    @Rol          VARCHAR(20),
    @Secuencia    INT            = 1,
    @Decision     VARCHAR(10)    = 'APROBADO',
    @Comentario   VARCHAR(500)   = '',
    @Trazo        VARBINARY(MAX) = NULL,
    @TrazoTipo    VARCHAR(30)    = 'image/png',
    @Cod_Usuario  VARCHAR(64)    = '',
    @Ip           VARCHAR(45)    = '',
    @Dispositivo  VARCHAR(300)   = ''
AS
BEGIN
    SET NOCOUNT ON;

    SET @Rol      = UPPER(LTRIM(RTRIM(ISNULL(@Rol,''))));
    SET @Decision = UPPER(LTRIM(RTRIM(ISNULL(@Decision,''))));

    IF @Rol NOT IN ('COLABORADOR','JEFE','GTH')
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El rol de la firma no es valido.'; RETURN;
    END

    IF @Decision NOT IN ('APROBADO','RECHAZADO')
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Debe indicar si aprueba o rechaza.'; RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Vacaciones WHERE IdVacaciones = @IdVacaciones)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'La solicitud indicada no existe.'; RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.VacacionesFirma
               WHERE IdVacaciones = @IdVacaciones AND Rol = @Rol AND Secuencia = @Secuencia)
    BEGIN
        /* No es un error del usuario: pasa con un doble clic. Se responde que ya
           esta firmado y la pantalla sigue su curso. */
        SELECT Respuestas = 1, Mensaje = 'Ese paso ya estaba firmado.'; RETURN;
    END

    /* La identidad se toma de R_Usuarios ahora y se copia. Ver el encabezado. */
    DECLARE @Nombre VARCHAR(128), @Cargo VARCHAR(128), @Cedula VARCHAR(32);

    SELECT @Nombre = ISNULL(Nom_Usuario,''),
           @Cargo  = Cargo,
           @Cedula = Cedula
    FROM dbo.R_Usuarios WHERE Cod_Usuario = @Cod_Usuario;

    IF @Nombre IS NULL
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'No se encontro el usuario que esta firmando.'; RETURN;
    END

    BEGIN TRY
        INSERT INTO dbo.VacacionesFirma
            (IdVacaciones, Rol, Secuencia, Decision, Comentario, Trazo, TrazoTipo,
             Cod_Usuario, Nombre, Cargo, Cedula, Ip, Dispositivo)
        VALUES
            (@IdVacaciones, @Rol, @Secuencia, @Decision, NULLIF(LTRIM(RTRIM(@Comentario)),''),
             @Trazo, @TrazoTipo, @Cod_Usuario, @Nombre, @Cargo, @Cedula,
             NULLIF(@Ip,''), NULLIF(@Dispositivo,''));

        SELECT Respuestas = 1, Mensaje = 'Firma registrada.';
    END TRY
    BEGIN CATCH
        SELECT Respuestas = 0, Mensaje = 'No se pudo registrar la firma: ' + ERROR_MESSAGE();
    END CATCH
END
GO
PRINT 'Sp_RTA_GuardarFirmaSolicitud creado.';
GO

/* ----------------------------------------------------------- 4. listar firmas */
IF OBJECT_ID('dbo.Sp_RTA_ListarFirmasSolicitud') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarFirmasSolicitud;
GO

/* Las firmas de una solicitud, en el orden del flujo. El trazo va en base64
   porque de aqui sale directo al <img> del HTML que se convierte a PDF. */
CREATE PROCEDURE dbo.Sp_RTA_ListarFirmasSolicitud
    @IdVacaciones INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        f.IdFirma,
        f.IdVacaciones,
        f.Rol,
        f.Secuencia,
        f.Decision,
        Comentario  = ISNULL(f.Comentario,''),
        TrazoBase64 = CASE WHEN f.Trazo IS NULL THEN ''
                           ELSE CAST(N'' AS XML).value('xs:base64Binary(sql:column("f.Trazo"))', 'VARCHAR(MAX)')
                      END,
        TrazoTipo   = ISNULL(f.TrazoTipo,'image/png'),
        f.Cod_Usuario,
        f.Nombre,
        Cargo       = ISNULL(f.Cargo,''),
        Cedula      = ISNULL(f.Cedula,''),
        f.FechaFirma,
        Ip          = ISNULL(f.Ip,''),
        Dispositivo = ISNULL(f.Dispositivo,'')
    FROM dbo.VacacionesFirma f
    WHERE f.IdVacaciones = @IdVacaciones
    ORDER BY CASE f.Rol WHEN 'COLABORADOR' THEN 1 WHEN 'JEFE' THEN 2 ELSE 3 END,
             f.Secuencia;
END
GO
PRINT 'Sp_RTA_ListarFirmasSolicitud creado.';
GO

/* --------------------------------------- 5. el listado de usuarios trae Cargo */
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
        Cargo               = ISNULL(u.Cargo,''),
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

/* -------------------------------------- 6. actualizacion y bitacora del Cargo */
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
    @Cargo               VARCHAR(300) = '',
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
    SET @Cargo               = LTRIM(RTRIM(ISNULL(@Cargo,'')));
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
       OR LEN(@Departamento) > 128 OR LEN(@Empresa) > 50 OR LEN(@Cargo) > 128
       OR LEN(@Cod_Sap) > 50
       OR LEN(@Cod_Jefe_Inm) > 100 OR LEN(@MailCodJefeInm) > 100
       OR LEN(@TelefonosEmergencia) > 100
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Alguno de los datos excede el largo permitido.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @oNom VARCHAR(100), @oMail VARCHAR(100), @oCed VARCHAR(32),
                @oDep VARCHAR(128), @oEmp VARCHAR(50), @oCargo VARCHAR(128),
                @oSap VARCHAR(50),
                @oJefe VARCHAR(100), @oMailJefe VARCHAR(100), @oTelEmer VARCHAR(100);

        SELECT @oNom      = ISNULL(Nom_Usuario,''),
               @oMail     = ISNULL(E_Mail,''),
               @oCed      = ISNULL(Cedula,''),
               @oDep      = ISNULL(Departamento,''),
               @oEmp      = ISNULL(Empresa,''),
               @oCargo    = ISNULL(Cargo,''),
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
        IF @oCargo    COLLATE Latin1_General_BIN <> @Cargo          COLLATE Latin1_General_BIN SET @Detalle = @Detalle + 'Cargo: [' + @oCargo + '] -> [' + @Cargo + ']; ';
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
            Cargo               = NULLIF(@Cargo,''),
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

/* ------------------------------------------------------------------ a revisar
   IdVacaciones se declaro INT en VacacionesFirma para calzar con el Int32 que
   usa la capa de datos. Sp_RTAInsertaNuevaSolicitud lo declara BIGINT, asi que
   el modelo ya venia inconsistente. Conviene confirmar el tipo real de
   Vacaciones.IdVacaciones cuando la base este disponible:

SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Vacaciones' AND COLUMN_NAME = 'IdVacaciones';

   Si resultara BIGINT y llegara a superar el rango de INT, esta tabla habria que
   ampliarla. Con 2801 solicitudes en cuatro anios, eso no pasa en esta decada.
   ------------------------------------------------------------------------- */

/* ------------------------------------------------------------------ pruebas
   Para correr a mano despues de crear todo. No forman parte del despliegue.

-- Debe rechazar un rol invalido.
EXEC dbo.Sp_RTA_GuardarFirmaSolicitud @IdVacaciones = 1, @Rol = 'GERENTE', @Cod_Usuario = '1';

-- Debe rechazar una solicitud que no existe.
EXEC dbo.Sp_RTA_GuardarFirmaSolicitud @IdVacaciones = 999999999, @Rol = 'JEFE', @Cod_Usuario = '1';

-- Sin firmas, debe devolver cero filas y no fallar.
EXEC dbo.Sp_RTA_ListarFirmasSolicitud @IdVacaciones = 1;
   ------------------------------------------------------------------------- */
