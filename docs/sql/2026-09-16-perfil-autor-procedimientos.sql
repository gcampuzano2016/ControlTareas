/* ============================================================================
   Perfil del colaborador: Usu_Modificacion pasa a guardar al AUTOR
   ReporTarea  |  2026-09-16

   YA APLICADO EN PRODUCCION (verificado el 2026-09-21). Paso 2 de la
   entrega 1. Requiere haber corrido antes
   docs/sql/2026-09-16-perfil-autor-columnas.sql.

   ----------------------------------------------------------------------------
   Que hace y por que

   Los 14 procedimientos de escritura del modulo guardaban
   Usu_Modificacion = @Cod_Usuario, es decir, el DUENO del perfil. Era cierto
   por construccion: solo el dueno podia escribir. Cuando Talento Humano pueda
   corregir el perfil de otro, esa columna diria que el cambio lo hizo el
   empleado. Pasa a guardar @Usu_Accion: quien lo hizo de verdad.

   @Usu_Accion es OPCIONAL a proposito. El SQL se despliega antes que los
   binarios (DESPLIEGUE.md). Si fuera obligatorio, entre un paso y el otro la
   aplicacion viva llamaria a procedimientos que exigen un parametro que
   todavia no manda, y el modulo entero dejaria de guardar. Con el valor por
   omision, los binarios viejos se comportan igual que hoy.

   Durante esa ventana -SQL puesto, binarios viejos- Empleados.Usu_ModificacionCod
   y Emp_CargaFamiliar.Usu_ModificacionCod se llenan con el codigo del DUENO del
   perfil, indistinguible de un autor real: los primeros valores de esas dos
   columnas no prueban que nadie de Talento Humano haya tocado nada.

   EL SENTIDO INVERSO NO ES SEGURO. Con SQL viejo y binarios nuevos -el que se
   saltea este paso, y tambien el que revierte el SQL con los binarios ya
   puestos- DaoPerfil manda @Usu_Accion por nombre a procedimientos que no lo
   declaran: el modulo entero deja de guardar y el usuario ve en pantalla el
   texto crudo de SQL Server.

   Sp_RTA_PerfilEliminarFoto NO esta aca: hace un DELETE fisico y, borrada la
   fila, no hay columna donde anotar al autor.

   Los cuatro de lectura (PerfilColaborador, PerfilDocumentoArchivo,
   PerfilEquipo, PerfilEquipoLista) tampoco: no escriben.

   Idempotente: cada procedimiento se hace DROP y CREATE.

   ----------------------------------------------------------------------------
   El cuerpo de cada procedimiento es el que ya esta en produccion, copiado de
   su script de origen, con tres cambios y ninguno mas:

     (a) @Usu_Accion VARCHAR(50) = NULL como ULTIMO parametro.
     (b) la linea de respaldo, inmediatamente despues de SET NOCOUNT ON;.
     (c) cada escritura de Usu_Modificacion pasa de @Cod_Usuario a @Usu_Accion.
         La columna Cod_Usuario sigue recibiendo @Cod_Usuario, siempre: el
         dueno del dato no cambia porque lo corrija otro.

   Y en tres de ellos, ademas, (d) una escritura NUEVA en las columnas que creo
   2026-09-16-perfil-autor-columnas.sql, porque hasta ahora no habia donde:

     Sp_RTA_PerfilGuardarContacto        Empleados.Usu_ModificacionCod
     Sp_RTA_PerfilGuardarCargaFamiliar   Emp_CargaFamiliar.Usu_ModificacionCod
     Sp_RTA_PerfilEliminarCargaFamiliar  Emp_CargaFamiliar.Usu_ModificacionCod

   De donde salen los cuerpos:
     docs/sql/2026-09-14-perfil-colaborador.sql        (1, 2, 3)
     docs/sql/2026-09-14-perfil-colaborador-fase2.sql  (4 a 11)
     docs/sql/2026-09-15-perfil-colaborador-fase3a.sql (12, 13, 14)
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* Las dos columnas de autor tienen que existir antes -tres de estos
   procedimientos escriben en ellas y el CREATE fallaria al compilarse- y tienen
   que ser del tipo correcto. Lo segundo no es paranoia: el comentario del punto
   3 de Sp_RTA_PerfilGuardarCargaFamiliar (fase 2) afirma que
   Emp_CargaFamiliar.Usu_Modificacion "es numeric" y el punto 1 del mismo
   comentario dice que esa columna no existe. Se consulto la base: el punto 3
   tiene razon, la columna existe y es numeric(5). Por eso el autor no se guarda
   ahi sino en Usu_ModificacionCod, una columna nueva que crea
   2026-09-16-perfil-autor-columnas.sql, y por eso esta comprobacion mira el
   tipo y no solo el nombre: una comprobacion por nombre a secas daria por buena
   la numeric vieja, los 14 procedimientos se crearian sin una queja y el error
   saldria recien al primer guardado de una carga familiar, en produccion, al
   convertir un Cod_Usuario a numero. */
DECLARE @TipoCargaFam  VARCHAR(128), @LargoCargaFam  INT,
        @TipoEmpleados VARCHAR(128), @LargoEmpleados INT;

SELECT @TipoCargaFam  = t.name,
       @LargoCargaFam = c.max_length
  FROM sys.columns c
  JOIN sys.types   t ON t.user_type_id = c.user_type_id
 WHERE c.object_id = OBJECT_ID('dbo.Emp_CargaFamiliar')
   AND c.name      = 'Usu_ModificacionCod';

SELECT @TipoEmpleados  = t.name,
       @LargoEmpleados = c.max_length
  FROM sys.columns c
  JOIN sys.types   t ON t.user_type_id = c.user_type_id
 WHERE c.object_id = OBJECT_ID('dbo.Empleados')
   AND c.name      = 'Usu_ModificacionCod';

/* Las dos ramas encienden NOEXEC y no hacen RETURN: RETURN fuera de un
   procedimiento sale del LOTE, no del script. Despues del GO la ejecucion
   seguiria y los 14 CREATE PROCEDURE se intentarian igual, fallando uno por uno
   con errores de columna en vez de con este mensaje. NOEXEC hace que los lotes
   siguientes se analicen pero no se ejecuten. Se apaga al final del archivo.

   max_length = -1 es varchar(MAX): mas ancha que 50, sirve igual. */
IF @TipoCargaFam IS NULL OR @TipoEmpleados IS NULL
BEGIN
    /* "o falta la tabla" no es una formula de cortesia: si dbo.Emp_CargaFamiliar
       o dbo.Empleados no existieran, el tipo daria NULL igual, y en ese caso
       2026-09-16-perfil-autor-columnas.sql no habria fallado -habria dicho "no
       existe. Omitido."-, asi que volver a correrlo no arreglaria nada. */
    RAISERROR('No estan las dos columnas de autor: falta correr 2026-09-16-perfil-autor-columnas.sql, o falta alguna de las dos tablas (dbo.Emp_CargaFamiliar, dbo.Empleados). Script detenido.', 16, 1);
    SET NOEXEC ON;
END
ELSE IF NOT (@TipoCargaFam  = 'varchar' AND (@LargoCargaFam  >= 50 OR @LargoCargaFam  = -1))
     OR NOT (@TipoEmpleados = 'varchar' AND (@LargoEmpleados >= 50 OR @LargoEmpleados = -1))
BEGIN
    RAISERROR('Las columnas de autor existen pero alguna no es del tipo esperado: Emp_CargaFamiliar.Usu_ModificacionCod es %s de largo %d y Empleados.Usu_ModificacionCod es %s de largo %d; en las dos se esperaba varchar de 50 o mas, porque van a recibir un Cod_Usuario. Revisar 2026-09-16-perfil-autor-columnas.sql y la columna que sobra antes de seguir. Script detenido.', 16, 1, @TipoCargaFam, @LargoCargaFam, @TipoEmpleados, @LargoEmpleados);
    SET NOEXEC ON;
END
GO

PRINT '== Perfil: el autor en los 14 procedimientos - inicio ==';
GO

/* ======================================================== 1. contacto ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarContacto','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarContacto;
GO

/* Escribe en dos sitios porque el dato vive en dos sitios: lo nuevo va a
   Perfil_ContactoPersonal y el estado civil se queda en Empleados, que es
   donde RRHH ya lo mantiene y lo lee su propia pantalla.

   El estado civil solo se actualiza si la persona tiene ficha enlazada. Para
   los 119 sin ficha no hay donde escribirlo; se ignora en silencio en vez de
   fallar, porque el resto del guardado si tiene sentido para ellos.

   @CodigoRepetido reproduce EXACTAMENTE el mismo calculo y el mismo criterio
   -solo usuarios activos- que usa Sp_RTA_PerfilColaborador para bloquear sus
   siete SELECT. La lectura ya se negaba a adivinar de quien es el dato
   cuando dos personas activas comparten Cod_Usuario; sin este mismo bloqueo
   aca, el guardado quedaba como la puerta abierta: una de las dos sesiones
   pisaria en el MERGE la unica fila de Perfil_ContactoPersonal de ese
   codigo -la de la otra persona- y, como la lectura si esta cerrada,
   ninguna de las dos se enteraria nunca. Devuelve -2 (no colisiona con los
   valores que ya existen) para que la capa de datos distinga este caso de
   un guardado correcto y muestre un mensaje explicito en vez de fingir
   exito. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarContacto
    @Cod_Usuario      VARCHAR(50),
    @CorreoPersonal   VARCHAR(150),
    @TelefonoPersonal VARCHAR(50),
    @Direccion        VARCHAR(400),
    @EstadoCivil      VARCHAR(100),
    @Ip               VARCHAR(64),
    @Usu_Accion       VARCHAR(50) = NULL      -- (a) siempre el ultimo
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    MERGE dbo.Perfil_ContactoPersonal AS destino
    USING (SELECT @Cod_Usuario AS Cod_Usuario) AS origen
       ON destino.Cod_Usuario = origen.Cod_Usuario
    WHEN MATCHED THEN
        UPDATE SET CorreoPersonal   = @CorreoPersonal,
                   TelefonoPersonal = @TelefonoPersonal,
                   Direccion        = @Direccion,
                   Fec_Modificacion = SYSDATETIME(),
                   Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
                   Ip_Modificacion  = @Ip
    WHEN NOT MATCHED THEN
        /* El primer @Cod_Usuario del VALUES es la columna Cod_Usuario -el
           dueno- y no se toca. El que cambia es el quinto, el de
           Usu_Modificacion. */
        INSERT (Cod_Usuario, CorreoPersonal, TelefonoPersonal, Direccion,
                Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @CorreoPersonal, @TelefonoPersonal, @Direccion,
                @Usu_Accion, @Ip);                   -- (c) era @Cod_Usuario

    IF LTRIM(RTRIM(ISNULL(@EstadoCivil,''))) <> ''
    BEGIN
        /* LEFT(@Ip, 32) no es cosmetica: Empleados.Ip_Modificacion es varchar(32),
           mas angosta que las columnas equivalentes de las tablas nuevas, que son
           varchar(64). Con una IPv6 este UPDATE no truncaria en silencio -fallaria
           con "String or binary data would be truncated" y se caeria el guardado
           entero, incluido lo que si cabia-. Se recorta aqui a proposito. */

        /* Este UPDATE sigue sin escribir Empleados.Usu_Modificacion, y no es un
           descuido: esa columna es numeric(5), pensada para un correlativo
           interno de RRHH, y no admite un Cod_Usuario (varchar). Por eso
           2026-09-16-perfil-autor-columnas.sql agrego Usu_ModificacionCod
           varchar(50) al lado -el sufijo dice de que esta hecha- y es esa la que
           recibe aqui al autor del cambio. Hasta esa columna, un cambio de
           estado civil no dejaba mas rastro que Fec_Modificacion e
           Ip_Modificacion. */
        UPDATE dbo.Empleados
           SET EstadoCivil         = @EstadoCivil,
               Fec_Modificacion    = GETDATE(),
               Usu_ModificacionCod = @Usu_Accion,   -- (d) escritura nueva
               Ip_Modificacion     = LEFT(@Ip, 32)
         WHERE Cod_Usuario = @Cod_Usuario;
    END

    SELECT Respuestas = 0;
END
GO
PRINT 'Sp_RTA_PerfilGuardarContacto actualizado.';
GO

/* ====================================================== 2. emergencia ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarEmergencia','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarEmergencia;
GO

/* IdContacto = 0 significa alta; cualquier otro, edicion.

   El WHERE de la edicion lleva Cod_Usuario ademas del IdContacto a proposito:
   sin eso, mandar el IdContacto de otra persona editaria su contacto. El
   IdContacto viene del cliente y no se puede confiar en el. El Cod_Usuario
   tambien puede venir del cliente -es asi desde que Talento Humano puede
   corregir el perfil de otro-; lo que lo hace confiable no es su origen sino
   que NegPerfilAcceso ya decidio que este autor puede escribir ESE perfil.

   @CodigoRepetido reproduce EXACTAMENTE el mismo calculo y el mismo criterio
   -solo usuarios activos- que Sp_RTA_PerfilColaborador y
   Sp_RTA_PerfilGuardarContacto. Hay 4 usuarios activos con el Cod_Usuario
   repetido, y en un caso ('0000') son dos personas distintas con su propio
   login y su propia sesion, ambas cargando el mismo valor. Sin este bloqueo,
   cualquiera de las dos veria y podria borrar los contactos de emergencia de
   la otra -exactamente el dato cuyo proposito es que alguien reciba una
   llamada cuando hay una urgencia-. Devuelve -2, igual que
   Sp_RTA_PerfilGuardarContacto, para que la capa de datos lo distinga de un
   guardado normal y muestre un mensaje explicito en vez de fingir exito.
   El RETURN es necesario: un SELECT de retorno no interrumpe la ejecucion en
   T-SQL, y sin el RETURN el procedimiento informaria el bloqueo y de todos
   modos escribiria. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarEmergencia
    @Cod_Usuario VARCHAR(50),
    @IdContacto  INT,
    @Nombre      VARCHAR(150),
    @Parentesco  VARCHAR(50),
    @Telefono    VARCHAR(50),
    @Ip          VARCHAR(64),
    @Usu_Accion  VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdContacto = @IdContacto;
        RETURN;
    END

    IF @IdContacto = 0
    BEGIN
        INSERT INTO dbo.Perfil_ContactoEmergencia
            (Cod_Usuario, Nombre, Parentesco, Telefono, Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nombre, @Parentesco, @Telefono, @Usu_Accion, @Ip);

        SELECT Respuestas = 0, IdContacto = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Perfil_ContactoEmergencia
           SET Nombre           = @Nombre,
               Parentesco       = @Parentesco,
               Telefono         = @Telefono,
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
               Ip_Modificacion  = @Ip
         WHERE IdContacto  = @IdContacto
           AND Cod_Usuario = @Cod_Usuario;

        SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdContacto = @IdContacto;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarEmergencia actualizado.';
GO

/* ============================================= 3. eliminar emergencia ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarEmergencia','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarEmergencia;
GO

/* Borrado logico, y con Cod_Usuario en el WHERE por la misma razon de arriba.

   Lleva el mismo @CodigoRepetido y el mismo RETURN que
   Sp_RTA_PerfilGuardarEmergencia y por la misma razon: sin el, una de las dos
   personas que comparten Cod_Usuario podria borrar el contacto de emergencia
   de la otra con solo mandar su IdContacto. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarEmergencia
    @Cod_Usuario VARCHAR(50),
    @IdContacto  INT,
    @Ip          VARCHAR(64),
    @Usu_Accion  VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.Perfil_ContactoEmergencia
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
           Ip_Modificacion  = @Ip
     WHERE IdContacto  = @IdContacto
       AND Cod_Usuario = @Cod_Usuario;       -- el dueno sigue siendo el dueno

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarEmergencia actualizado.';
GO

/* ======================================================== 4. estudios ===== */

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
    @Ip             VARCHAR(64),
    @Usu_Accion     VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
                @Usu_Accion, @Ip);

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
               Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
               Ip_Modificacion  = @Ip
         WHERE IdEstudio  = @IdEstudio
           AND Cod_Usuario = @Cod_Usuario;

        SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdEstudio  = @IdEstudio;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarEstudio actualizado.';
GO

/* =============================================== 5. eliminar estudio ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarEstudio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarEstudio;
GO

/* Borrado logico. Con Cod_Usuario en el WHERE por la misma razon de arriba:
   sin el, cualquiera borraria el estudio de otra persona mandando su id. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarEstudio
    @Cod_Usuario VARCHAR(50),
    @IdEstudio   INT,
    @Ip          VARCHAR(64),
    @Usu_Accion  VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
           Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
           Ip_Modificacion  = @Ip
     WHERE IdEstudio   = @IdEstudio
       AND Cod_Usuario = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarEstudio actualizado.';
GO

/* ================================================= 6. certificaciones ===== */

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
    @Ip              VARCHAR(64),
    @Usu_Accion      VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
        VALUES (@Cod_Usuario, @Nombre, @Entidad, @Fecha, @Usu_Accion, @Ip);

        SELECT Respuestas = 0, IdCertificacion = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Perfil_Certificacion
           SET Nombre           = @Nombre,
               Entidad          = @Entidad,
               FechaObtencion   = @Fecha,
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
               Ip_Modificacion  = @Ip
         WHERE IdCertificacion = @IdCertificacion
           AND Cod_Usuario     = @Cod_Usuario;

        SELECT Respuestas      = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdCertificacion = @IdCertificacion;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarCertificacion actualizado.';
GO

/* ========================================= 7. eliminar certificacion ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCertificacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarCertificacion;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarCertificacion
    @Cod_Usuario     VARCHAR(50),
    @IdCertificacion INT,
    @Ip              VARCHAR(64),
    @Usu_Accion      VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
           Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
           Ip_Modificacion  = @Ip
     WHERE IdCertificacion = @IdCertificacion
       AND Cod_Usuario     = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarCertificacion actualizado.';
GO

/* ===================================================== 8. experiencia ===== */

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
    @Ip            VARCHAR(64),
    @Usu_Accion    VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
                @Funciones, @Usu_Accion, @Ip);

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
               Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
               Ip_Modificacion  = @Ip
         WHERE IdExperiencia = @IdExperiencia
           AND Cod_Usuario   = @Cod_Usuario;

        SELECT Respuestas    = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdExperiencia = @IdExperiencia;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarExperiencia actualizado.';
GO

/* ============================================ 9. eliminar experiencia ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarExperiencia','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarExperiencia;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarExperiencia
    @Cod_Usuario   VARCHAR(50),
    @IdExperiencia INT,
    @Ip            VARCHAR(64),
    @Usu_Accion    VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
           Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
           Ip_Modificacion  = @Ip
     WHERE IdExperiencia = @IdExperiencia
       AND Cod_Usuario   = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarExperiencia actualizado.';
GO

/* =============================================== 10. carga familiar ====== */

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
      habia borrado. Desde este script si queda registro de quien la habia
      tocado desde el perfil, pero no de quien apreto ese boton.

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

   3. Usu_ModificacionCod es la columna NUEVA que creo
      2026-09-16-perfil-autor-columnas.sql: varchar(50) y nullable. Esta tabla
      ya tenia Usu_Modificacion, pero es numeric(5) -verificado contra la base-
      y no admite un Cod_Usuario, asi que no se toca: sigue donde estaba, con
      lo que haya escrito el resto del sistema. Empleados esta en el mismo
      caso y recibio una columna con el mismo nombre por la misma razon. Lo
      que se escribia desde el perfil quedaba hasta ahora sin registro de
      quien lo hizo.

   IdEmpleado se deja nulo a proposito. La columna es nullable y 119 de 231
   usuarios no tienen ficha enlazada; sus cargas cuelgan solo de Cod_Usuario. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarCargaFamiliar
    @Cod_Usuario     VARCHAR(50),
    @IdCargaFam      INT,
    @Nombre          VARCHAR(150),
    @Parentesco      VARCHAR(50),
    @FechaNacimiento VARCHAR(10),
    @Ip              VARCHAR(64),
    @Usu_Accion      VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
             Estado, Fec_Modificacion, Usu_ModificacionCod, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nombre, @Parentesco, @Nacimiento,
                '1', GETDATE(), @Usu_Accion, LEFT(@Ip, 32));   -- (d) escritura nueva

        SELECT Respuestas = 0, IdCargaFam = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Emp_CargaFamiliar
           SET Nombre           = @Nombre,
               Parentesco       = @Parentesco,
               Fecha_nacimiento = @Nacimiento,
               Fec_Modificacion = GETDATE(),
               Usu_ModificacionCod = @Usu_Accion,   -- (d) escritura nueva
               Ip_Modificacion  = LEFT(@Ip, 32)
         WHERE IdCargaFam  = @IdCargaFam
           AND Cod_Usuario = @Cod_Usuario;

        SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdCargaFam = @IdCargaFam;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarCargaFamiliar actualizado.';
GO

/* ======================================= 11. eliminar carga familiar ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCargaFamiliar','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarCargaFamiliar;
GO

/* Escribe '0', igual que el borrado logico de las demas tablas del modulo.
   Ver el comentario de Sp_RTA_PerfilGuardarCargaFamiliar: con '0' el toggle
   de Sp_RTACambiarEstadoCargaFam (que solo entiende 'Activo'/'Inactivo') cae
   en su ELSE y no alcanza esta fila. No cambiar sin entender esto.

   Usu_ModificacionCod es la columna nueva de 2026-09-16-perfil-autor-columnas:
   la Usu_Modificacion que ya existia es numeric(5) y no se toca. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarCargaFamiliar
    @Cod_Usuario VARCHAR(50),
    @IdCargaFam  INT,
    @Ip          VARCHAR(64),
    @Usu_Accion  VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
           Usu_ModificacionCod = @Usu_Accion,   -- (d) escritura nueva
           Ip_Modificacion  = LEFT(@Ip, 32)
     WHERE IdCargaFam  = @IdCargaFam
       AND Cod_Usuario = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarCargaFamiliar actualizado.';
GO

/* ============================================================ 12. foto ==== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarFoto','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarFoto;
GO

/* Una fila por persona: si ya tiene foto se reemplaza. No hay historial de
   fotos y no hace falta. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarFoto
    @Cod_Usuario VARCHAR(50),
    @FotoBase64  VARCHAR(MAX),
    @FotoTipo    VARCHAR(50),
    @Ip          VARCHAR(64),
    @Usu_Accion  VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
               Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
               Ip_Modificacion  = @Ip
         WHERE Cod_Usuario = @Cod_Usuario;
    ELSE
        INSERT INTO dbo.Perfil_Foto
              (Cod_Usuario, FotoBase64, FotoTipo, Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @FotoBase64, @FotoTipo, @Usu_Accion, @Ip);

    SELECT Respuestas = 0;
END
GO
PRINT 'Sp_RTA_PerfilGuardarFoto actualizado.';
GO

/* ======================================================= 13. documento ==== */

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
    @Ip                  VARCHAR(64),
    @Usu_Accion          VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
       puede saber de quien es. Aqui si.

       La comprobacion sigue siendo contra @Cod_Usuario -el dueno- y no contra
       @Usu_Accion: el documento cuelga de la certificacion o de la carga
       familiar del dueno del perfil, sea quien sea el que lo sube. */
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
            @Ruta, @Usu_Accion, @Ip);   -- (c) era @Cod_Usuario

    SELECT Respuestas = 0, IdDocumento = CONVERT(INT, SCOPE_IDENTITY());
END
GO
PRINT 'Sp_RTA_PerfilGuardarDocumento actualizado.';
GO

/* ============================================== 14. eliminar documento ==== */

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
    @Ip          VARCHAR(64),
    @Usu_Accion  VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

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
           Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
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
PRINT 'Sp_RTA_PerfilEliminarDocumento actualizado.';
GO

/* Incondicional: si la guarda de arriba encendio NOEXEC, apagarlo aca es lo que
   evita que el resto de la sesion de SSMS quede sin ejecutar nada y parezca que
   los scripts siguientes "no hacen nada". */
SET NOEXEC OFF;
GO

PRINT '== Perfil: el autor en los 14 procedimientos - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano; las dos consultas, una ANTES y otra DESPUES)

   ----------------------------------------------------------------------------
   1. ANTES DE CORRER ESTE SCRIPT: los permisos a nivel de objeto

   Cada procedimiento se hace DROP y CREATE, y el DROP se lleva puestos los
   permisos que hubiera sobre el objeto: un GRANT EXECUTE a un usuario o a un
   rol no vuelve solo, y el CREATE no lo repone. Ningun script de docs/sql/
   tiene un GRANT, asi que si se perdiera no habria de donde recuperarlo y el
   modulo de perfil empezaria a fallar con "EXECUTE permission denied" para
   todos. Los tres scripts de origen ya crearon estos mismos 14 con DROP y
   CREATE y el modulo funciona, o sea que lo esperable es que esto devuelva
   CERO filas.

   Si devuelve filas: anotarlas, correr el script, y volver a otorgar esos
   mismos permisos despues. No alcanza con mirar: el DROP ya los descarto.

   Esta consulta es toda la mitigacion del DROP+CREATE, asi que esta escrita
   para ser de fiar y no para ser corta:

     - class = 1 (OBJECT_OR_COLUMN) es obligatorio. sys.database_permissions
       guarda permisos de muchas clases y major_id significa una cosa distinta
       en cada una -en class 0 es la base, en class 4 un principal, en class 3
       un esquema-, asi que sin el filtro el IN puede emparejar por numero un
       permiso que no tiene nada que ver con estos procedimientos y hacer creer
       que hay algo que reponer donde no lo hay.

     - La lista es la de los 14 que ESTE script dropea, nombrados uno por uno, y
       no 'Sp_RTA_Perfil%', que son 19: los otros 5 -Sp_RTA_PerfilEliminarFoto y
       los cuatro de lectura- no se tocan aca y sus permisos no corren peligro.
       Mezclarlos solo agrega filas que confunden.

SELECT  procedimiento = OBJECT_NAME(dp.major_id),
        beneficiario  = USER_NAME(dp.grantee_principal_id),
        dp.permission_name,
        dp.state_desc
  FROM  sys.database_permissions dp
 WHERE  dp.class = 1
   AND  OBJECT_NAME(dp.major_id) IN
        ('Sp_RTA_PerfilGuardarContacto',       'Sp_RTA_PerfilGuardarEmergencia',
         'Sp_RTA_PerfilEliminarEmergencia',    'Sp_RTA_PerfilGuardarEstudio',
         'Sp_RTA_PerfilEliminarEstudio',       'Sp_RTA_PerfilGuardarCertificacion',
         'Sp_RTA_PerfilEliminarCertificacion', 'Sp_RTA_PerfilGuardarExperiencia',
         'Sp_RTA_PerfilEliminarExperiencia',   'Sp_RTA_PerfilGuardarCargaFamiliar',
         'Sp_RTA_PerfilEliminarCargaFamiliar', 'Sp_RTA_PerfilGuardarFoto',
         'Sp_RTA_PerfilGuardarDocumento',      'Sp_RTA_PerfilEliminarDocumento')
 ORDER BY procedimiento, beneficiario;

   ----------------------------------------------------------------------------
   2. DESPUES DE CORRER ESTE SCRIPT: que los 14 tengan el parametro

   Esperado: 19 filas. 14 con tieneUsuAccion = 1 -los de este script- y 5 con
   0: Sp_RTA_PerfilEliminarFoto (DELETE fisico, no hay donde anotar) y los
   cuatro de lectura (Sp_RTA_PerfilColaborador, Sp_RTA_PerfilDocumentoArchivo,
   Sp_RTA_PerfilEquipo, Sp_RTA_PerfilEquipoLista).

SELECT  procedimiento = OBJECT_NAME(p.object_id),
        tieneUsuAccion = MAX(CASE WHEN pa.name = '@Usu_Accion' THEN 1 ELSE 0 END)
  FROM  sys.procedures p
  LEFT JOIN sys.parameters pa ON pa.object_id = p.object_id
 WHERE  OBJECT_NAME(p.object_id) LIKE 'Sp_RTA_Perfil%'
 GROUP BY OBJECT_NAME(p.object_id)
 ORDER BY tieneUsuAccion DESC, procedimiento;

   ============================================================================ */
