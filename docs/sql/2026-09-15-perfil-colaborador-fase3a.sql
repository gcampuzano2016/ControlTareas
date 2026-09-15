/* ============================================================================
   Perfil del colaborador - FASE 3a
   Foto de perfil, documentos de respaldo y lo que el CV necesita leer.

   Contenido:
     1. Sp_RTA_PerfilGuardarFoto
     2. Sp_RTA_PerfilEliminarFoto
     3. Sp_RTA_PerfilGuardarDocumento
     4. Sp_RTA_PerfilEliminarDocumento
     5. Sp_RTA_PerfilDocumentoArchivo
     6. Sp_RTA_PerfilColaborador recreado con el NOVENO result set (foto)
     7. Aserciones

   Las tablas Perfil_Foto y Perfil_Documento ya existen: las creo la fase 1.
   Este script no crea ni altera ninguna tabla.

   Idempotente: se puede correr dos veces sin dano.
   ============================================================================ */

SET NOCOUNT ON;
GO
/* QUOTED_IDENTIFIER y ANSI_NULLS explicitos y dentro del script, no como
   parametro de sqlcmd: DESPLIEGUE.md dice que esto lo corre una persona a
   mano, y sqlcmd los deja apagados por omision. En la fase 1 eso aborto el
   script a media ejecucion (error 1934). */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------ 1. Sp_RTA_PerfilGuardarFoto --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarFoto','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarFoto;
GO

/* Una fila por persona: si ya tiene foto se reemplaza. No hay historial de
   fotos y no hace falta. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarFoto
    @Cod_Usuario VARCHAR(50),
    @FotoBase64  VARCHAR(MAX),
    @FotoTipo    VARCHAR(50),
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    /* Cod_Usuario no es unico en R_Usuarios: la PK real es Id_Usuario y hay
       codigos repetidos entre usuarios activos, en un caso entre dos personas
       distintas. Cuando pasa, este procedimiento no escribe: no hay forma de
       saber de quien seria la foto. Mismo criterio que DaoFirmaUsuario. */
    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        /* El RETURN es de verdad necesario: en T-SQL un SELECT no interrumpe
           la ejecucion, y sin el se seguiria escribiendo despues de haber
           devuelto -2. */
        SELECT Respuestas = -2;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Perfil_Foto WHERE Cod_Usuario = @Cod_Usuario)
        UPDATE dbo.Perfil_Foto
           SET FotoBase64       = @FotoBase64,
               FotoTipo         = @FotoTipo,
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Cod_Usuario,
               Ip_Modificacion  = @Ip
         WHERE Cod_Usuario = @Cod_Usuario;
    ELSE
        INSERT INTO dbo.Perfil_Foto
              (Cod_Usuario, FotoBase64, FotoTipo, Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @FotoBase64, @FotoTipo, @Cod_Usuario, @Ip);

    SELECT Respuestas = 0;
END
GO
PRINT 'Sp_RTA_PerfilGuardarFoto creado.';
GO

/* ----------------------------------------- 2. Sp_RTA_PerfilEliminarFoto --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarFoto','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarFoto;
GO

/* Borrado fisico y no logico, a diferencia del resto del modulo: Perfil_Foto
   no tiene columna Estado -es una fila por persona, no una lista- y guardar
   una foto marcada como borrada no le sirve a nadie. @Ip se recibe por
   simetria con los demas procedimientos aunque una fila que desaparece no
   tenga donde anotarla. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarFoto
    @Cod_Usuario VARCHAR(50),
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    DELETE FROM dbo.Perfil_Foto WHERE Cod_Usuario = @Cod_Usuario;

    IF @@ROWCOUNT = 0
        SELECT Respuestas = -1;
    ELSE
        SELECT Respuestas = 0;
END
GO
PRINT 'Sp_RTA_PerfilEliminarFoto creado.';
GO

/* ------------------------------------- 3. Sp_RTA_PerfilGuardarDocumento --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarDocumento','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarDocumento;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarDocumento
    @Cod_Usuario         VARCHAR(50),
    @Origen              VARCHAR(20),
    @IdOrigen            INT,
    @NombreArchivo       VARCHAR(260),
    @NombreArchivoCodigo VARCHAR(260),
    @Ruta                VARCHAR(400),
    @Ip                  VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdDocumento = 0;
        RETURN;
    END

    /* Un documento cuelga de algo, y ese algo tiene que ser suyo. Sin esta
       comprobacion un POST directo colgaria un archivo de la certificacion de
       otra persona: el handler valida que el IdOrigen sea un numero, pero no
       puede saber de quien es. Aqui si. */
    DECLARE @EsSuyo BIT = 0;

    IF @Origen = 'CERTIFICACION'
       AND EXISTS (SELECT 1 FROM dbo.Perfil_Certificacion
                    WHERE IdCertificacion = @IdOrigen
                      AND Cod_Usuario     = @Cod_Usuario
                      AND Estado          = '1')
        SET @EsSuyo = 1;

    /* Emp_CargaFamiliar la comparte RRHHEmpleados.aspx, que crea filas con
       Cod_Usuario NULL. Exigir Cod_Usuario = @Cod_Usuario deja fuera esas
       filas, que es exactamente lo que se quiere: nadie adjunta un documento
       a una carga que no registro desde su perfil. */
    IF @Origen = 'CARGAFAMILIAR'
       AND EXISTS (SELECT 1 FROM dbo.Emp_CargaFamiliar
                    WHERE IdCargaFam  = @IdOrigen
                      AND Cod_Usuario = @Cod_Usuario
                      AND Estado      = '1')
        SET @EsSuyo = 1;

    IF @EsSuyo = 0
    BEGIN
        SELECT Respuestas = -1, IdDocumento = 0;
        RETURN;
    END

    INSERT INTO dbo.Perfil_Documento
          (Cod_Usuario, Origen, IdOrigen, NombreArchivo, NombreArchivoCodigo,
           Ruta, Usu_Modificacion, Ip_Modificacion)
    VALUES (@Cod_Usuario, @Origen, @IdOrigen, @NombreArchivo, @NombreArchivoCodigo,
            @Ruta, @Cod_Usuario, @Ip);

    SELECT Respuestas = 0, IdDocumento = CONVERT(INT, SCOPE_IDENTITY());
END
GO
PRINT 'Sp_RTA_PerfilGuardarDocumento creado.';
GO

/* ------------------------------------ 4. Sp_RTA_PerfilEliminarDocumento --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarDocumento','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarDocumento;
GO

/* Borrado logico. El archivo se queda en el disco a proposito: desde que
   Estado pasa a '0' deja de ser alcanzable -Sp_RTA_PerfilDocumentoArchivo
   exige '1'-, y borrarlo agregaria un modo de fallo (la base dice borrado, el
   disco fallo) a cambio de espacio que no es un problema. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarDocumento
    @Cod_Usuario VARCHAR(50),
    @IdDocumento INT,
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.Perfil_Documento
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Cod_Usuario,
           Ip_Modificacion  = @Ip
     WHERE IdDocumento = @IdDocumento
       AND Cod_Usuario = @Cod_Usuario
       AND Estado      = '1';

    IF @@ROWCOUNT = 0
        SELECT Respuestas = -1;
    ELSE
        SELECT Respuestas = 0;
END
GO
PRINT 'Sp_RTA_PerfilEliminarDocumento creado.';
GO

/* ------------------------------------- 5. Sp_RTA_PerfilDocumentoArchivo --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilDocumentoArchivo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilDocumentoArchivo;
GO

/* La guarda de la descarga. Devuelve una fila solo si ese documento es de esa
   persona y sigue activo; cero filas en cualquier otro caso, incluido el del
   codigo repetido. El handler no decide nada: si no hay fila, no hay archivo.

   Que no tenga parametro @Ip no es un olvido: es una lectura. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilDocumentoArchivo
    @Cod_Usuario VARCHAR(50),
    @IdDocumento INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    SELECT NombreArchivo, NombreArchivoCodigo, Ruta
      FROM dbo.Perfil_Documento
     WHERE IdDocumento = @IdDocumento
       AND Cod_Usuario = @Cod_Usuario
       AND Estado      = '1'
       AND @CodigoRepetido = 0;
END
GO
PRINT 'Sp_RTA_PerfilDocumentoArchivo creado.';
GO

/* ------------------------- 6. Sp_RTA_PerfilColaborador (recreado) --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilColaborador','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilColaborador;
GO

/* Todo el perfil de una persona en una sola ida: ocho result sets.

   La cabecera es un LEFT JOIN contra Empleados a proposito. 119 de 231
   usuarios activos no tienen ficha enlazada; con INNER JOIN entrarian y verian
   una pantalla en blanco. Con LEFT ven su nombre, su area y todo lo que si se
   sabe de ellos, y las secciones nuevas -que cuelgan de Cod_Usuario- les
   funcionan completas.

   Fecha_nacimiento sale como TEXTO, sin convertir. Es nvarchar(50) con formato
   dd/MM/yyyy y convertirla aca obliga a acertarle al estilo (103) o la edad se
   rompe para 80 de 133 personas sin dar error. La conversion la hace
   NegPerfilCampos.EdadDesdeTexto, que tiene pruebas.

   Los result sets 4 a 8 se llenan a partir de la fase 2 (cargas familiares
   llega en el 8). El procedimiento ya los declaraba desde la fase 1 para que
   las fases siguientes no tuvieran que volver a tocar la base. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilColaborador
    @Cod_Usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    /* R_Usuarios no tiene indice unico sobre Cod_Usuario -su llave real es
       Id_Usuario- y hay codigos repetidos entre usuarios activos (el caso
       '0000' son dos personas distintas). Si se entregara CUALQUIERA de los
       ocho result sets en ese caso, el contacto personal, los contactos de
       emergencia y todo lo demas que cuelga de Cod_Usuario vendria mezclado
       o seria de la otra persona -domicilio, telefono personal, a
       quien llamar en una emergencia-, sin forma de saber de quien es cada
       dato. Y aunque la pantalla oculte las pestanas cuando no hay perfil, el
       JSON ya viajo al navegador: cerrar la puerta de la cabecera y dejar
       las demas abiertas serviria de poco.

       Por eso @CodigoRepetido se aplica en el WHERE de los ocho SELECT, no
       solo en la cabecera: el criterio es "no se entrega nada", no "no se
       entrega la cabecera". No hay un RETURN anticipado a proposito -el Dao
       recorre los result sets por posicion y espera que los ocho siempre
       vengan, aunque vacios; un RETURN rompe ese contrato.

       Mismo criterio que CapaDato/DaoFirmaUsuario.cs (comentario del metodo
       Obtener): ante un codigo repetido, se devuelve vacio en vez de
       adivinar cual de las dos personas es -"la fila seria de dos personas
       distintas"-. No se usa TOP 1 con ORDER BY: eso elegiria una fila fija,
       y para '0000' esa fila fija seria siempre la de la persona equivocada
       para la otra. Determinista y equivocado es peor que vacio, porque
       nadie lo descubre. */
    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    /* 1. cabecera */
    SELECT  u.Cod_Usuario,
            NombreCompleto  = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario),
            Cedula          = ISNULL(NULLIF(LTRIM(RTRIM(u.Cedula)), ''), e.Cedula),
            FechaNacTexto   = LTRIM(RTRIM(ISNULL(e.Fecha_nacimiento, ''))),
            Cargo           = ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo),
            Area            = ISNULL(NULLIF(LTRIM(RTRIM(e.AreaTrabajo)), ''), u.Departamento),
            Ciudad          = e.Ciudad,
            CorreoNotificacion = u.E_Mail,
            JefeInmediato   = j.Nom_Usuario,

            /* El horario va como subconsulta y NO como LEFT JOIN: hay 5 usuarios
               con mas de una asignacion activa a la vez, y un join los duplicaria.
               La cabecera tiene que devolver exactamente una fila siempre, porque
               el Dao hace un solo Read(): con un join, esas 5 personas verian un
               horario elegido al azar y nadie se enteraria.

               R_UsuarioHorarioLaboral.Id_Responsable guarda un Cod_Usuario, pese
               al nombre. Solo 88 de 231 tienen horario asignado; el resto recibe
               NULL y la pantalla muestra un guion. */
            Horario = (SELECT TOP 1 h.Nombre
                         FROM dbo.R_UsuarioHorarioLaboral uh
                         JOIN dbo.R_HorarioLaboral h
                              ON h.IdHorarioLaboral = uh.IdHorarioLaboral
                        WHERE uh.Id_Responsable = u.Cod_Usuario
                          AND uh.Activo = 1
                        ORDER BY uh.FechaDesde DESC, uh.IdUsuarioHorario DESC),

            TieneFicha      = CASE WHEN e.IdEmpleado IS NULL THEN 0 ELSE 1 END,
            EsJefe          = CASE WHEN EXISTS (SELECT 1 FROM dbo.R_Usuarios s
                                                 WHERE LTRIM(RTRIM(s.Cod_Jefe_Inm)) = LTRIM(RTRIM(u.Cod_Usuario))
                                                   AND ISNULL(s.EstadoUsuario, 0) = 0)
                                   THEN 1 ELSE 0 END
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados  e ON e.Cod_Usuario = u.Cod_Usuario
      LEFT JOIN dbo.R_Usuarios j ON LTRIM(RTRIM(j.Cod_Usuario)) = LTRIM(RTRIM(u.Cod_Jefe_Inm))
     WHERE  u.Cod_Usuario = @Cod_Usuario
       AND  @CodigoRepetido = 0;

    /* 2. contacto personal (editable) */
    SELECT  CorreoPersonal   = ISNULL(p.CorreoPersonal, ''),
            TelefonoPersonal = ISNULL(p.TelefonoPersonal, ''),
            Direccion        = ISNULL(p.Direccion, CAST(e.Direccion AS VARCHAR(400))),
            EstadoCivil      = ISNULL(e.EstadoCivil, '')
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Perfil_ContactoPersonal p ON p.Cod_Usuario = u.Cod_Usuario
      LEFT JOIN dbo.Empleados e ON e.Cod_Usuario = u.Cod_Usuario
     WHERE  u.Cod_Usuario = @Cod_Usuario
       AND  @CodigoRepetido = 0;

    /* 3. contactos de emergencia */
    SELECT IdContacto, Nombre, Parentesco, Telefono
      FROM dbo.Perfil_ContactoEmergencia
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
       AND @CodigoRepetido = 0
     ORDER BY IdContacto;

    /* 4. estudios (fase 2) */
    SELECT IdEstudio, Nivel, Institucion, Titulo, AnioGraduacion
      FROM dbo.Perfil_Estudio
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
       AND @CodigoRepetido = 0
     ORDER BY AnioGraduacion DESC, IdEstudio;

    /* 5. certificaciones (fase 2) */
    SELECT IdCertificacion, Nombre, Entidad, FechaObtencion
      FROM dbo.Perfil_Certificacion
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
       AND @CodigoRepetido = 0
     ORDER BY FechaObtencion DESC, IdCertificacion;

    /* 6. experiencia (fase 2) */
    SELECT IdExperiencia, Empresa, Cargo, AnioDesde, AnioHasta, Funciones
      FROM dbo.Perfil_Experiencia
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
       AND @CodigoRepetido = 0
     ORDER BY ISNULL(AnioHasta, 9999) DESC, AnioDesde DESC;

    /* 7. documentos de respaldo (fase 3) */
    SELECT IdDocumento, Origen, IdOrigen, NombreArchivo, NombreArchivoCodigo, Ruta
      FROM dbo.Perfil_Documento
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
       AND @CodigoRepetido = 0
     ORDER BY IdDocumento;

    /* 8. cargas familiares

       Va al final y no junto a los otros datos personales a proposito: el Dao
       recorre los result sets POR POSICION, asi que un contrato posicional se
       amplia por el final. Insertarlo en medio desplazaria los conjuntos 4 a 7
       y sus datos aterrizarian en la propiedad equivocada, sin ningun error.

       El filtro es Estado = '1', igual que en las otras cinco tablas del
       modulo, y no el vocabulario 'Activo'/'Inactivo' de RRHHEmpleados.aspx.
       Da lo mismo para efectos de este SELECT: las filas que crea esa
       pantalla tienen Cod_Usuario NULO -no lo conoce- y jamas pasan el
       WHERE Cod_Usuario = @Cod_Usuario de aqui, sea cual sea el filtro de
       Estado. Ver el comentario de Sp_RTA_PerfilGuardarCargaFamiliar para el
       porque completo de usar '1'/'0' en esta tabla compartida. */
    SELECT IdCargaFam,
           Nombre,
           Parentesco,
           FechaNacTexto = CONVERT(VARCHAR(10), Fecha_nacimiento, 23)
      FROM dbo.Emp_CargaFamiliar
     WHERE Cod_Usuario = @Cod_Usuario
       AND Estado = '1'
       AND @CodigoRepetido = 0
     ORDER BY Fecha_nacimiento DESC, IdCargaFam;

    /* 9. foto de perfil

       Va al final, como fue el 8 en su momento: el Dao recorre los result sets
       POR POSICION y un contrato posicional se amplia por el final. Meterla en
       la cabecera habria obligado a reescribir el SELECT del conjunto 1, que es
       el que lleva el LEFT JOIN de los 119 sin ficha y el TRY_CONVERT con
       estilo 103 del que depende la edad. Este SELECT deja aquel intacto.

       Devuelve cero filas si la persona no subio foto: es el caso normal el
       primer dia y el Dao lo trata como "sin foto", no como error. */
    SELECT FotoBase64, FotoTipo
      FROM dbo.Perfil_Foto
     WHERE Cod_Usuario = @Cod_Usuario
       AND @CodigoRepetido = 0;
END
GO
PRINT 'Sp_RTA_PerfilColaborador recreado con el noveno result set.';
GO

/* ------------------------------------------------------- 7. aserciones --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarFoto','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarFoto no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarFoto','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarFoto no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarDocumento','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarDocumento no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarDocumento','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarDocumento no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilDocumentoArchivo','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilDocumentoArchivo no quedo creado.', 16, 1);

/* El noveno result set tiene que estar: el Dao lo lee por posicion y si
   faltara leeria nulos sin quejarse. Se apunta al texto de la consulta -FROM
   mas una columna propia de la tabla- y no a un comentario, por lo mismo que
   la asercion equivalente de la fase 2: un LIKE contra un comentario pasaria
   aunque alguien borrara el SELECT. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilColaborador'
                 AND m.definition LIKE '%FROM dbo.Perfil_Foto%'
                 AND m.definition LIKE '%FotoBase64%')
    RAISERROR('FALLO: Sp_RTA_PerfilColaborador no tiene el noveno result set.', 16, 1);

/* El octavo tampoco se puede haber perdido en la recreacion. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilColaborador'
                 AND m.definition LIKE '%FROM dbo.Emp_CargaFamiliar%')
    RAISERROR('FALLO: Sp_RTA_PerfilColaborador perdio el octavo result set.', 16, 1);

/* Y el septimo, que es el que esta fase empieza a leer. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilColaborador'
                 AND m.definition LIKE '%FROM dbo.Perfil_Documento%')
    RAISERROR('FALLO: Sp_RTA_PerfilColaborador perdio el septimo result set.', 16, 1);

PRINT 'Fase 3a: script terminado.';
GO
