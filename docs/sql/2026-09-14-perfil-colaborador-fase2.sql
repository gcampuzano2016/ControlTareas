/* ============================================================================
   Modulo: Perfil del colaborador — FASE 2 (hoja de vida)
   Spec  : docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md
   Previo: docs/sql/2026-09-14-perfil-colaborador.sql (fase 1, ya aplicado)

   1. Estudios        : guardar / eliminar
   2. Certificaciones : guardar / eliminar
   3. Experiencia     : guardar / eliminar
   4. Cargas familiares: guardar / eliminar
   5. Sp_RTA_PerfilColaborador con el octavo result set (cargas familiares)
   6. Aserciones

   Las tablas ya existen: las creo la fase 1. Aqui solo van procedimientos.

   Se puede ejecutar varias veces sin efecto adicional.
   Base: ReporTarea
   ============================================================================ */

SET NOCOUNT ON;
GO

/* Igual que en el script de la fase 1. No se heredan entre archivos: cada uno
   tiene que encenderlos por su cuenta o un indice filtrado -o cualquier objeto
   que los exija- falla con el error 1934. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------------------------------------------------------- 1. estudios --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarEstudio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarEstudio;
GO

/* IdEstudio = 0 es alta; cualquier otro, edicion.

   La guarda @CodigoRepetido es la misma de todos los procedimientos del modulo:
   R_Usuarios.Cod_Usuario NO es unico -la clave primaria real es Id_Usuario- y
   hay dos personas distintas compartiendo un codigo. Cuando eso pasa no se
   entrega ni se escribe nada, antes que adivinar de quien es el dato. Mismo
   criterio que CapaDato/DaoFirmaUsuario.cs.

   El RETURN despues del SELECT no es opcional: un SELECT de retorno NO
   interrumpe la ejecucion en T-SQL, asi que sin el se informaria el bloqueo y
   se escribiria igual.

   El WHERE de la edicion lleva Cod_Usuario ademas del IdEstudio: el id viene
   del cliente y no es de fiar, el Cod_Usuario viene de la sesion y si. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarEstudio
    @Cod_Usuario    VARCHAR(50),
    @IdEstudio      INT,
    @Nivel          VARCHAR(60),
    @Institucion    VARCHAR(200),
    @Titulo         VARCHAR(200),
    @AnioGraduacion SMALLINT,
    @Ip             VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdEstudio = @IdEstudio;
        RETURN;
    END

    IF @IdEstudio = 0
    BEGIN
        INSERT INTO dbo.Perfil_Estudio
            (Cod_Usuario, Nivel, Institucion, Titulo, AnioGraduacion,
             Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nivel, @Institucion, @Titulo, NULLIF(@AnioGraduacion, 0),
                @Cod_Usuario, @Ip);

        SELECT Respuestas = 0, IdEstudio = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Perfil_Estudio
           SET Nivel            = @Nivel,
               Institucion      = @Institucion,
               Titulo           = @Titulo,
               AnioGraduacion   = NULLIF(@AnioGraduacion, 0),
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Cod_Usuario,
               Ip_Modificacion  = @Ip
         WHERE IdEstudio  = @IdEstudio
           AND Cod_Usuario = @Cod_Usuario;

        SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdEstudio  = @IdEstudio;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarEstudio creado.';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarEstudio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarEstudio;
GO

/* Borrado logico. Con Cod_Usuario en el WHERE por la misma razon de arriba:
   sin el, cualquiera borraria el estudio de otra persona mandando su id. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarEstudio
    @Cod_Usuario VARCHAR(50),
    @IdEstudio   INT,
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

    UPDATE dbo.Perfil_Estudio
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Cod_Usuario,
           Ip_Modificacion  = @Ip
     WHERE IdEstudio   = @IdEstudio
       AND Cod_Usuario = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarEstudio creado.';
GO

/* --------------------------------------------------- 2. certificaciones --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarCertificacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarCertificacion;
GO

/* @FechaObtencion llega como texto "yyyy-MM" desde un input type="month". Se
   convierte aqui con estilo 126 (ISO) y agregando el dia 01, que es lo que
   TRY_CONVERT necesita. Si no convierte queda NULL: la validacion de C# ya
   rechazo los formatos raros, esto es la red por si alguien llama al
   procedimiento por fuera. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarCertificacion
    @Cod_Usuario     VARCHAR(50),
    @IdCertificacion INT,
    @Nombre          VARCHAR(200),
    @Entidad         VARCHAR(200),
    @FechaObtencion  VARCHAR(10),
    @Ip              VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdCertificacion = @IdCertificacion;
        RETURN;
    END

    DECLARE @Fecha DATE = TRY_CONVERT(date, NULLIF(LTRIM(RTRIM(@FechaObtencion)),'') + '-01', 126);

    IF @IdCertificacion = 0
    BEGIN
        INSERT INTO dbo.Perfil_Certificacion
            (Cod_Usuario, Nombre, Entidad, FechaObtencion, Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nombre, @Entidad, @Fecha, @Cod_Usuario, @Ip);

        SELECT Respuestas = 0, IdCertificacion = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Perfil_Certificacion
           SET Nombre           = @Nombre,
               Entidad          = @Entidad,
               FechaObtencion   = @Fecha,
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Cod_Usuario,
               Ip_Modificacion  = @Ip
         WHERE IdCertificacion = @IdCertificacion
           AND Cod_Usuario     = @Cod_Usuario;

        SELECT Respuestas      = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdCertificacion = @IdCertificacion;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarCertificacion creado.';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCertificacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarCertificacion;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarCertificacion
    @Cod_Usuario     VARCHAR(50),
    @IdCertificacion INT,
    @Ip              VARCHAR(64)
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

    UPDATE dbo.Perfil_Certificacion
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Cod_Usuario,
           Ip_Modificacion  = @Ip
     WHERE IdCertificacion = @IdCertificacion
       AND Cod_Usuario     = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarCertificacion creado.';
GO

/* ------------------------------------------------------ 3. experiencia --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarExperiencia','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarExperiencia;
GO

/* @AnioHasta = 0 significa "sigue ahi" y se guarda como NULL. No es lo mismo
   que "no se sabe": la lectura ordena poniendo los nulos primero, porque el
   empleo actual va arriba en un CV. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarExperiencia
    @Cod_Usuario   VARCHAR(50),
    @IdExperiencia INT,
    @Empresa       VARCHAR(200),
    @Cargo         VARCHAR(200),
    @AnioDesde     SMALLINT,
    @AnioHasta     SMALLINT,
    @Funciones     VARCHAR(MAX),
    @Ip            VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdExperiencia = @IdExperiencia;
        RETURN;
    END

    IF @IdExperiencia = 0
    BEGIN
        INSERT INTO dbo.Perfil_Experiencia
            (Cod_Usuario, Empresa, Cargo, AnioDesde, AnioHasta, Funciones,
             Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Empresa, @Cargo, NULLIF(@AnioDesde,0), NULLIF(@AnioHasta,0),
                @Funciones, @Cod_Usuario, @Ip);

        SELECT Respuestas = 0, IdExperiencia = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Perfil_Experiencia
           SET Empresa          = @Empresa,
               Cargo            = @Cargo,
               AnioDesde        = NULLIF(@AnioDesde,0),
               AnioHasta        = NULLIF(@AnioHasta,0),
               Funciones        = @Funciones,
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Cod_Usuario,
               Ip_Modificacion  = @Ip
         WHERE IdExperiencia = @IdExperiencia
           AND Cod_Usuario   = @Cod_Usuario;

        SELECT Respuestas    = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdExperiencia = @IdExperiencia;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarExperiencia creado.';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarExperiencia','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarExperiencia;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarExperiencia
    @Cod_Usuario   VARCHAR(50),
    @IdExperiencia INT,
    @Ip            VARCHAR(64)
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

    UPDATE dbo.Perfil_Experiencia
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Cod_Usuario,
           Ip_Modificacion  = @Ip
     WHERE IdExperiencia = @IdExperiencia
       AND Cod_Usuario   = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarExperiencia creado.';
GO

/* ------------------------------------------------- 4. cargas familiares --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarCargaFamiliar','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarCargaFamiliar;
GO

/* Emp_CargaFamiliar NO es una tabla de este modulo: la comparte
   RRHHEmpleados.aspx. De ahi varias diferencias con los otros procedimientos:

   1. Estado usa '1'/'0', igual que las demas tablas del modulo, y NO
      'Activo'/'Inactivo' como se penso en una version anterior de este script.

      RRHHEmpleados.aspx borra con Sp_RTACambiarEstadoCargaFam, que hace esto:

          UPDATE Emp_CargaFamiliar
             SET Estado = CASE WHEN Estado='Activo' THEN 'Inactivo'
                                WHEN Estado='Inactivo' THEN 'Activo'
                                ELSE Estado END
           WHERE Nombre = @Nombre;

      Es un TOGGLE, POR NOMBRE, sobre TODA la tabla -sin filtrar por
      IdEmpleado ni por ningun otro dueno-. Hoy no hace nada porque ninguna
      fila tiene Estado escrito y todas caen en el ELSE. Esta fase es lo que
      lo despierta: en cuanto una fila del perfil escribiera 'Activo', ese
      boton empezaria a alcanzarla, desactivando la carga de una persona y
      -si dos personas comparten nombre- REACTIVANDO la que la otra ya
      habia borrado. Sin Usu_Modificacion en esta tabla, no queda registro de
      quien lo hizo.

      Con '1' en Estado, el CASE de arriba cae siempre en el ELSE (ni
      'Activo' ni 'Inactivo' hacen match) y estas filas quedan intocables
      para ese boton. No hace falta -ni se debe- tocar una sola linea de
      Sp_RTACambiarEstadoCargaFam: alcanza con no hablar su mismo vocabulario.

      La version anterior de este comentario decia que el filtro de lectura
      del octavo result set era "<> 'Inactivo'" y no "= 'Activo'" porque
      "RRHHEmpleados.aspx inserta sin escribir Estado". Ese razonamiento
      resulto no sostenerse: las filas que crea RRHHEmpleados.aspx tienen
      Cod_Usuario NULO -esa pantalla no lo conoce- y por lo tanto NUNCA
      pueden salir del SELECT del perfil, que siempre filtra por
      Cod_Usuario = @Cod_Usuario, sea cual sea el filtro de Estado que use.
      Las dos pantallas son ciegas entre si al leer (una filtra por
      IdEmpleado, la otra por Cod_Usuario), asi que compartir vocabulario en
      Estado no aportaba nada y si abria la puerta al toggle de arriba.

   2. Ip_Modificacion es varchar(32), mas angosta que el varchar(64) de las
      tablas nuevas. Con una IPv6 este INSERT no truncaria: fallaria con
      "String or binary data would be truncated" y tumbaria el guardado entero.
      Por eso LEFT(@Ip, 32).

   3. Usu_Modificacion es numeric y no admite un Cod_Usuario, asi que no se
      escribe. No es un descuido: no cabe. Queda sin registro de quien lo
      cambio, igual que en Empleados.

   IdEmpleado se deja nulo a proposito. La columna es nullable y 119 de 231
   usuarios no tienen ficha enlazada; sus cargas cuelgan solo de Cod_Usuario. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarCargaFamiliar
    @Cod_Usuario     VARCHAR(50),
    @IdCargaFam      INT,
    @Nombre          VARCHAR(150),
    @Parentesco      VARCHAR(50),
    @FechaNacimiento VARCHAR(10),
    @Ip              VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdCargaFam = @IdCargaFam;
        RETURN;
    END

    DECLARE @Nacimiento DATETIME = TRY_CONVERT(datetime, NULLIF(LTRIM(RTRIM(@FechaNacimiento)),''), 126);

    IF @IdCargaFam = 0
    BEGIN
        INSERT INTO dbo.Emp_CargaFamiliar
            (Cod_Usuario, Nombre, Parentesco, Fecha_nacimiento,
             Estado, Fec_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nombre, @Parentesco, @Nacimiento,
                '1', GETDATE(), LEFT(@Ip, 32));

        SELECT Respuestas = 0, IdCargaFam = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Emp_CargaFamiliar
           SET Nombre           = @Nombre,
               Parentesco       = @Parentesco,
               Fecha_nacimiento = @Nacimiento,
               Fec_Modificacion = GETDATE(),
               Ip_Modificacion  = LEFT(@Ip, 32)
         WHERE IdCargaFam  = @IdCargaFam
           AND Cod_Usuario = @Cod_Usuario;

        SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdCargaFam = @IdCargaFam;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarCargaFamiliar creado.';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCargaFamiliar','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarCargaFamiliar;
GO

/* Escribe '0', igual que el borrado logico de las demas tablas del modulo.
   Ver el comentario de Sp_RTA_PerfilGuardarCargaFamiliar: con '0' el toggle
   de Sp_RTACambiarEstadoCargaFam (que solo entiende 'Activo'/'Inactivo') cae
   en su ELSE y no alcanza esta fila. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarCargaFamiliar
    @Cod_Usuario VARCHAR(50),
    @IdCargaFam  INT,
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

    UPDATE dbo.Emp_CargaFamiliar
       SET Estado           = '0',
           Fec_Modificacion = GETDATE(),
           Ip_Modificacion  = LEFT(@Ip, 32)
     WHERE IdCargaFam  = @IdCargaFam
       AND Cod_Usuario = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarCargaFamiliar creado.';
GO

/* -------------------------------- 5. Sp_RTA_PerfilColaborador (recreado) --- */

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
END
GO
PRINT 'Sp_RTA_PerfilColaborador recreado con el octavo result set.';
GO

/* ------------------------------------------------------- 6. aserciones --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarEstudio','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarEstudio no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarEstudio','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarEstudio no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarCertificacion','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarCertificacion no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCertificacion','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarCertificacion no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarExperiencia','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarExperiencia no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarExperiencia','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarExperiencia no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarCargaFamiliar','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarCargaFamiliar no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCargaFamiliar','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarCargaFamiliar no quedo creado.', 16, 1);

/* El octavo result set tiene que estar: el Dao lo lee por posicion y si
   faltara leeria nulos sin quejarse. Se apunta al texto de la consulta -no a
   un comentario- porque es la unica verificacion automatizada del contrato
   posicional, que es el riesgo alrededor del cual se diseno esta fase: un
   LIKE contra un comentario pasaria aunque alguien borrara el SELECT y
   dejara el texto explicativo, y fallaria si alguien solo reformula el
   comentario sin tocar nada. Exigiendo el FROM y una columna propia de la
   tabla, al menos falla si el SELECT desaparece. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilColaborador'
                 AND m.definition LIKE '%FROM dbo.Emp_CargaFamiliar%'
                 AND m.definition LIKE '%IdCargaFam%')
    RAISERROR('FALLO: Sp_RTA_PerfilColaborador no tiene el octavo result set.', 16, 1);

PRINT 'Aserciones de la fase 2 OK.';
GO

PRINT 'Script 2026-09-14-perfil-colaborador-fase2 completado.';
GO
