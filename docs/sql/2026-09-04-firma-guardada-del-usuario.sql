/* ============================================================================
   La firma de cada persona queda guardada, para no volver a subirla cada vez.

   1. UsuarioFirma                     : una fila por persona, su ultimo trazo
   2. Sp_RTA_GuardarFirmaSolicitud     : al firmar una solicitud, la recuerda
   3. Sp_RTA_FirmaGuardadaUsuario      : la devuelve para precargar el pad

   ORDEN: despues de 2026-08-25-firmas-y-cargo.sql, que es donde nacen
   VacacionesFirma y el procedimiento que este script rehace.

   Se puede ejecutar varias veces sin efecto adicional.
   Base: ReporTarea
   ============================================================================

   Que resuelve
   ------------
   Hoy el pad recuerda la firma en sessionStorage del navegador, asi que sirve
   entre Vacaciones y Permisos en el mismo rato y se pierde al cerrar la pestana.
   Al dia siguiente hay que volver a dibujarla o subirla.

   Guardarla contra la persona la hace viajar con ella en vez de quedarse en la
   maquina. Se descarto localStorage por lo contrario: en una maquina compartida
   dejaria la firma de uno disponible para el siguiente que se siente ahi. Aca
   para verla hay que iniciar sesion como esa persona, que es otra cosa.

   Por que se guarda sola al firmar
   ---------------------------------
   No hay boton de "guardar mi firma" ni pantalla de administracion. Cada vez que
   alguien firma una solicitud, esa firma queda como la suya. La ultima gana, asi
   que cambiarla es firmar distinto una vez. Un paso menos que mantener y una
   pantalla menos que explicar.

   Lo que se reutiliza es el trazo, no la aprobacion: cada fila de VacacionesFirma
   sigue guardando su propia fecha, IP, dispositivo y decision.

   Por que los codigos repetidos quedan afuera
   --------------------------------------------
   Cod_Usuario NO es unico en R_Usuarios. Al 4 de septiembre de 2026 hay tres
   codigos repetidos y el peor es '1052': 12 filas y DOS personas distintas. Si la
   firma se guardara por codigo a secas, esas dos personas compartirian firma.

   En cualquier otro campo seria un error feo; en una firma es inaceptable. Asi
   que ni se guarda ni se ofrece cuando el codigo esta repetido: ahi el pad se
   comporta como siempre, dibujar o subir cada vez. Son 16 filas de 247 y ninguna
   ha firmado nunca, asi que nadie pierde algo que hoy tenga. Cuando Talento
   Humano limpie esos codigos, la funcion aparece sola.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ------------------------------------------------- 1. la firma de la persona */
IF OBJECT_ID('dbo.UsuarioFirma') IS NULL
BEGIN
    CREATE TABLE dbo.UsuarioFirma
    (
        /* La clave es el codigo con el que la aplicacion identifica a quien tiene
           la sesion abierta. Ver el encabezado sobre los codigos repetidos. */
        Cod_Usuario   VARCHAR(64)    NOT NULL,

        Trazo         VARBINARY(MAX) NOT NULL,
        TrazoTipo     VARCHAR(30)    NULL,

        /* Cuando la actualizo por ultima vez y desde donde. No es la trazabilidad
           de ninguna firma en particular -esa vive en VacacionesFirma- sino de
           esta imagen. */
        FechaRegistro DATETIME2(0)   NOT NULL CONSTRAINT DF_UsuarioFirma_Fecha DEFAULT (SYSDATETIME()),
        Ip            VARCHAR(45)    NULL,
        Dispositivo   VARCHAR(300)   NULL,

        CONSTRAINT PK_UsuarioFirma PRIMARY KEY (Cod_Usuario)
    );

    PRINT 'UsuarioFirma creada.';
END
ELSE
    PRINT 'UsuarioFirma ya existia.';
GO

/* --------------------------------- 2. al firmar una solicitud, se la recuerda */
IF OBJECT_ID('dbo.Sp_RTA_GuardarFirmaSolicitud') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarFirmaSolicitud;
GO

CREATE PROCEDURE dbo.Sp_RTA_GuardarFirmaSolicitud
    @IdVacaciones BIGINT,
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

    /* La identidad se toma de R_Usuarios ahora y se copia. Ver el encabezado de
       2026-08-25-firmas-y-cargo.sql. */
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

        /* Y queda como SU firma, para no tener que volver a subirla.

           Solo si el codigo identifica a una sola persona: con un codigo repetido
           esta fila seria de dos, y una firma compartida no es una firma.

           Fuera del TRY de la firma no: si esto fallara, la firma de la solicitud
           -que es lo que importa- ya quedo insertada y el CATCH de abajo devolveria
           un error que no corresponde. Por eso va con su propio TRY. */
        IF @Trazo IS NOT NULL
           AND (SELECT COUNT(1) FROM dbo.R_Usuarios WHERE Cod_Usuario = @Cod_Usuario) = 1
        BEGIN
            BEGIN TRY
                UPDATE dbo.UsuarioFirma
                SET Trazo         = @Trazo,
                    TrazoTipo     = @TrazoTipo,
                    FechaRegistro = SYSDATETIME(),
                    Ip            = NULLIF(@Ip,''),
                    Dispositivo   = NULLIF(@Dispositivo,'')
                WHERE Cod_Usuario = @Cod_Usuario;

                IF @@ROWCOUNT = 0
                    INSERT INTO dbo.UsuarioFirma
                        (Cod_Usuario, Trazo, TrazoTipo, Ip, Dispositivo)
                    VALUES
                        (@Cod_Usuario, @Trazo, @TrazoTipo, NULLIF(@Ip,''), NULLIF(@Dispositivo,''));
            END TRY
            BEGIN CATCH
                /* Que no se pueda recordar la firma no invalida la que se acaba de
                   registrar. Se sigue en silencio: lo peor que pasa es que la
                   proxima vez la tenga que volver a dibujar. */
                SET @Nombre = @Nombre;
            END CATCH
        END

        SELECT Respuestas = 1, Mensaje = 'Firma registrada.';
    END TRY
    BEGIN CATCH
        SELECT Respuestas = 0, Mensaje = 'No se pudo registrar la firma: ' + ERROR_MESSAGE();
    END CATCH
END
GO
PRINT 'Sp_RTA_GuardarFirmaSolicitud actualizado.';
GO

/* ------------------------------------------ 3. la firma guardada, para el pad */
IF OBJECT_ID('dbo.Sp_RTA_FirmaGuardadaUsuario') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_FirmaGuardadaUsuario;
GO

/* Devuelve siempre una fila. Sin firma guardada, con el trazo vacio: asi quien
   llama no tiene que distinguir "no hay" de "no se pudo". */
CREATE PROCEDURE dbo.Sp_RTA_FirmaGuardadaUsuario
    @Cod_Usuario VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    /* Codigo repetido: no se ofrece. Ver el encabezado. */
    IF (SELECT COUNT(1) FROM dbo.R_Usuarios WHERE Cod_Usuario = @Cod_Usuario) <> 1
    BEGIN
        SELECT TrazoBase64 = '', TrazoTipo = '', FechaRegistro = CAST(NULL AS DATETIME2(0));
        RETURN;
    END

    SELECT
        /* En base64 porque de aqui sale directo al src de una imagen. */
        TrazoBase64 = CASE WHEN f.Trazo IS NULL THEN ''
                           ELSE CAST(N'' AS XML).value('xs:base64Binary(sql:column("f.Trazo"))', 'VARCHAR(MAX)')
                      END,
        TrazoTipo   = ISNULL(f.TrazoTipo,'image/png'),
        f.FechaRegistro
    FROM dbo.UsuarioFirma f
    WHERE f.Cod_Usuario = @Cod_Usuario;

    /* Sin fila, se responde vacio igual. */
    IF @@ROWCOUNT = 0
        SELECT TrazoBase64 = '', TrazoTipo = '', FechaRegistro = CAST(NULL AS DATETIME2(0));
END
GO
PRINT 'Sp_RTA_FirmaGuardadaUsuario creado.';
GO

/* ------------------------------------------------------------------ pruebas
   Para correr a mano. No forman parte del despliegue.

-- Sin firma guardada todavia: una fila con el trazo vacio.
EXEC dbo.Sp_RTA_FirmaGuardadaUsuario @Cod_Usuario = '2869799';

-- Un codigo repetido: tambien vacio, y a proposito.
EXEC dbo.Sp_RTA_FirmaGuardadaUsuario @Cod_Usuario = '1052';

-- Quienes ya tienen firma guardada.
SELECT u.Cod_Usuario, r.Nom_Usuario, u.FechaRegistro, Bytes = DATALENGTH(u.Trazo)
FROM dbo.UsuarioFirma u
LEFT JOIN dbo.R_Usuarios r ON r.Cod_Usuario = u.Cod_Usuario
ORDER BY u.FechaRegistro DESC;
   ------------------------------------------------------------------------- */
