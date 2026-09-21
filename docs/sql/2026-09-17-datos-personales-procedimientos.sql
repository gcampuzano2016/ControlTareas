/* ============================================================================
   Datos personales editables: el guardado
   ReporTarea  |  2026-09-17

   YA APLICADO EN PRODUCCION (verificado el 2026-09-21). Corre DESPUES de
   docs/sql/2026-09-17-datos-personales-columnas.sql.

   ----------------------------------------------------------------------------
   Escribe los ocho campos en las DOS tablas. No es duplicacion: la cabecera de
   Sp_RTA_PerfilColaborador lee con respaldo y la precedencia no es la misma
   para todos los campos -Nombre y Cargo los manda Empleados, Cedula la manda
   R_Usuarios-, asi que escribir en un solo lado deja el otro viejo y la
   pantalla muestra el valor anterior justo despues de guardar.

   Idempotente: DROP y CREATE.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* Las columnas de autor tienen que existir antes: estos procedimientos escriben
   en ellas y el CREATE fallaria al compilarse.

   NOEXEC y no RETURN: RETURN fuera de un procedimiento sale del LOTE, no del
   script, y despues del GO los CREATE se intentarian igual, fallando uno por
   uno con errores de columna en vez de con este mensaje. Se apaga al final del
   archivo, sin condicion. */
IF  NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.R_Usuarios') AND name='Usu_ModificacionCod')
 OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.R_Usuarios') AND name='Fec_Modificacion')
 OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.R_Usuarios') AND name='Ip_Modificacion')
BEGIN
    RAISERROR('Faltan columnas de auditoria en R_Usuarios: correr primero docs/sql/2026-09-17-datos-personales-columnas.sql. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Empleados') AND name='Usu_ModificacionCod')
BEGIN
    RAISERROR('Falta Empleados.Usu_ModificacionCod: correr primero la entrega 1 (docs/sql/2026-09-16-perfil-autor-columnas.sql). Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

PRINT '== Datos personales: procedimientos - inicio ==';
GO

/* ====================================================== 1. el guardado ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarDatosPersonales','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarDatosPersonales;
GO

/* Respuestas:
      0  guardado, sin nada que avisar
      1  guardado, pero el correo ya lo tiene otro usuario activo
      2  guardado, pero la cedula ya la tiene otro usuario activo
      3  guardado, y las dos cosas
     -2  Cod_Usuario repetido entre usuarios activos. Mismo caso que bloquean
         Sp_RTA_PerfilColaborador y las otras escrituras del modulo: con dos
         personas compartiendo codigo, no hay forma de saber a cual escribirle.
     -3  el jefe elegido no existe, no esta activo, o es la persona misma. Un
         Cod_Jefe_Inm que no apunta a nadie deja las solicitudes de vacaciones de
         esa persona sin aprobador, y nadie se entera hasta que alguien pide
         vacaciones.

   Los positivos son todos "se guardo": el 1, el 2 y el 3 son avisos, no
   errores. Los negativos son "no se guardo nada".

   Los anchos de los parametros son los de la columna MAS ANGOSTA de las dos
   tablas, los mismos que valida NegPerfilCampos. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarDatosPersonales
    @Cod_Usuario  VARCHAR(50),
    @Nombre       VARCHAR(100),
    @Cedula       VARCHAR(32),
    @FechaNac     VARCHAR(10),
    @Cargo        VARCHAR(128),
    @Area         VARCHAR(128),
    @Ciudad       VARCHAR(150),
    @CodJefeInm   VARCHAR(100),
    @Correo       VARCHAR(100),
    @Ip           VARCHAR(64),
    @Usu_Accion   VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* El autor del cambio, no el dueno del perfil. Si no viene -una llamada
       vieja, o un binario sin desplegar- se cae al dueno, que es exactamente lo
       que hacia el modulo antes de la entrega 1. */
    DECLARE @Autor VARCHAR(50);
    SET @Autor = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

    DECLARE @Jefe VARCHAR(100);
    SET @Jefe = LTRIM(RTRIM(ISNULL(@CodJefeInm, '')));

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario, 0) = 0) <> 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    IF @Jefe <> ''
    BEGIN
        /* Que no sea uno mismo tambien lo comprueba NegPerfilCampos, con
           pruebas. Se repite aca porque este procedimiento es llamable desde
           SSMS y desde cualquier codigo futuro, y porque el dano -una persona
           que se aprueba sus propias vacaciones- no se nota hasta que ya paso. */
        IF @Jefe = LTRIM(RTRIM(@Cod_Usuario))
        BEGIN
            SELECT Respuestas = -3;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios
                        WHERE LTRIM(RTRIM(Cod_Usuario)) = @Jefe
                          AND ISNULL(EstadoUsuario, 0) = 0)
        BEGIN
            SELECT Respuestas = -3;
            RETURN;
        END
    END

    BEGIN TRANSACTION;

    /* --- R_Usuarios ------------------------------------------------------
       Nom_Usuario es NOT NULL: por eso el nombre es el unico campo obligatorio
       de los ocho y NegPerfilCampos lo rechaza vacio antes de llegar aca.

       Cod_Jefe_Inm va a NULL cuando viene vacio y no a cadena vacia: el LEFT
       JOIN de la cabecera y la consulta de equipo comparan contra Cod_Usuario, y
       una cadena vacia no empareja con nadie pero tampoco se lee como "sin
       jefe". */
    UPDATE dbo.R_Usuarios
       SET Nom_Usuario         = @Nombre,
           Cedula              = @Cedula,
           Cargo               = @Cargo,
           Departamento        = @Area,
           Cod_Jefe_Inm        = NULLIF(@Jefe, ''),
           E_Mail              = @Correo,
           Usu_ModificacionCod = @Autor,
           Fec_Modificacion    = GETDATE(),
           Ip_Modificacion     = LEFT(@Ip, 64)
     WHERE Cod_Usuario = @Cod_Usuario;

    /* --- Empleados: primero adoptar, despues crear ------------------------ */
    DECLARE @IdEmpleado BIGINT;

    /* TOP 1 con ORDER BY, igual que la adopcion de mas abajo. Hoy ningun
       Cod_Usuario tiene dos fichas -verificado el 2026-09-17- pero no hay indice
       unico que lo impida, y sin ORDER BY una segunda ficha se elegiria al azar
       y el guardado iria a parar a una u otra sin que nadie se entere. */
    SELECT TOP 1 @IdEmpleado = IdEmpleado
      FROM dbo.Empleados
     WHERE Cod_Usuario = @Cod_Usuario
     ORDER BY IdEmpleado;

    /* La adopcion. Empleados tiene 21 filas sin Cod_Usuario -fichas de gente
       real que nadie enlazo nunca- y 4 de ellas son de personas que esta
       pantalla da por "sin ficha". Sin este SELECT, esas 4 terminan con dos
       fichas en RRHHEmpleados.aspx y ningun mensaje lo advierte.

       Por cedula exacta y sin LIKE: emparejar de mas es peor que no emparejar,
       porque mezcla las fichas de dos personas distintas. TOP 1 con ORDER BY
       para que sea determinista; hoy ninguna cedula huerfana esta repetida. */
    IF @IdEmpleado IS NULL AND LTRIM(RTRIM(ISNULL(@Cedula, ''))) <> ''
    BEGIN
        SELECT TOP 1 @IdEmpleado = IdEmpleado
          FROM dbo.Empleados
         WHERE LTRIM(RTRIM(ISNULL(Cod_Usuario, ''))) = ''
           AND LTRIM(RTRIM(ISNULL(Cedula, '')))      = LTRIM(RTRIM(@Cedula))
         ORDER BY IdEmpleado;
    END

    IF @IdEmpleado IS NULL
    BEGIN
        /* IdEmpleado es IDENTITY y la unica columna NOT NULL de la tabla, asi
           que el INSERT no necesita nada mas. Estado = 'Activo' es el valor que
           usan las 128 fichas vivas; sin el, la ficha nueva no aparece en
           RRHHEmpleados.aspx y el efecto seria justo el contrario del buscado.

           Ip_Modificacion de Empleados es varchar(32), mas angosta que las 64 de
           R_Usuarios: con una IPv6 esto no truncaria en silencio, fallaria con
           "String or binary data would be truncated" y se caeria el guardado
           entero. Por eso LEFT(@Ip, 32) aqui y LEFT(@Ip, 64) arriba. */
        INSERT INTO dbo.Empleados
              (Cod_Usuario, Nombre, Cedula, Fecha_nacimiento, PuestoTrabajo,
               AreaTrabajo, Ciudad, Estado, Fec_Modificacion,
               Usu_ModificacionCod, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nombre, @Cedula, @FechaNac, @Cargo,
               @Area, @Ciudad, 'Activo', GETDATE(),
               @Autor, LEFT(@Ip, 32));
    END
    ELSE
    BEGIN
        /* Cod_Usuario se escribe tambien en el UPDATE: es lo que completa la
           adopcion de una ficha huerfana. Para una ficha que ya era de esta
           persona, es escribir el mismo valor. */
        UPDATE dbo.Empleados
           SET Cod_Usuario         = @Cod_Usuario,
               Nombre              = @Nombre,
               Cedula              = @Cedula,
               Fecha_nacimiento    = @FechaNac,
               PuestoTrabajo       = @Cargo,
               AreaTrabajo         = @Area,
               Ciudad              = @Ciudad,
               Fec_Modificacion    = GETDATE(),
               Usu_ModificacionCod = @Autor,
               Ip_Modificacion     = LEFT(@Ip, 32)
         WHERE IdEmpleado = @IdEmpleado;
    END

    COMMIT TRANSACTION;

    /* --- Los avisos, despues de guardar ----------------------------------
       Un correo repetido NO impide guardar: el sistema ya convive con correos
       repetidos, y bloquear impediria corregir justamente esos casos. Pero
       tampoco puede pasar en silencio: RTA_CodigoUsuarioPorCorreo busca por
       correo y, ante dos usuarios con el mismo, se queda con el de Id_Usuario
       mas alto. Duplicar un correo no da error, cambia a quien se le atribuye
       una firma.

       Lo mismo con la cedula, que es el puente de identidad entre R_Usuarios y
       Empleados y por donde el modulo de horas extras enlaza a la gente.

       Van DESPUES del COMMIT a proposito: son dos lecturas que no tienen por que
       mantener abierta una transaccion que ya termino su trabajo.

       Se suman como banderas para no necesitar dos viajes: 1 correo, 2 cedula,
       3 los dos. */
    DECLARE @Aviso INT;
    SET @Aviso = 0;

    IF LTRIM(RTRIM(ISNULL(@Correo, ''))) <> ''
       AND EXISTS (SELECT 1 FROM dbo.R_Usuarios
                    WHERE LTRIM(RTRIM(ISNULL(E_Mail, ''))) = LTRIM(RTRIM(@Correo))
                      AND Cod_Usuario <> @Cod_Usuario
                      AND ISNULL(EstadoUsuario, 0) = 0)
        SET @Aviso = @Aviso + 1;

    IF LTRIM(RTRIM(ISNULL(@Cedula, ''))) <> ''
       AND EXISTS (SELECT 1 FROM dbo.R_Usuarios
                    WHERE LTRIM(RTRIM(ISNULL(Cedula, ''))) = LTRIM(RTRIM(@Cedula))
                      AND Cod_Usuario <> @Cod_Usuario
                      AND ISNULL(EstadoUsuario, 0) = 0)
        SET @Aviso = @Aviso + 2;

    SELECT Respuestas = @Aviso;
END
GO

PRINT 'Sp_RTA_PerfilGuardarDatosPersonales actualizado.';
GO

/* ================================================ 2. la lista de jefes ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilJefesLista','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilJefesLista;
GO

/* Los candidatos a jefe inmediato para el combo de la pantalla.

   Mismo filtro que Sp_RTA_PerfilPersonalLista: ISNULL(EstadoUsuario,0) = 0 y sin
   Cod_Usuario repetido. Los repetidos no se ofrecen porque elegir uno dejaria la
   cadena de aprobaciones apuntando a dos personas.

   Se excluye a la persona misma: que nadie sea su propio jefe tambien lo
   comprueban NegPerfilCampos y el guardado, pero no ofrecerlo evita que aparezca
   como una opcion legitima. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilJefesLista
    @Cod_Usuario VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  CodUsuario = LTRIM(RTRIM(u.Cod_Usuario)),
            Nombre     = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario)
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados e ON e.Cod_Usuario = u.Cod_Usuario
     WHERE  ISNULL(u.EstadoUsuario, 0) = 0
       AND  (SELECT COUNT(*) FROM dbo.R_Usuarios r
              WHERE r.Cod_Usuario = u.Cod_Usuario
                AND ISNULL(r.EstadoUsuario, 0) = 0) = 1
       AND  LTRIM(RTRIM(u.Cod_Usuario)) <> LTRIM(RTRIM(ISNULL(@Cod_Usuario, '')))
     ORDER BY Nombre;
END
GO

PRINT 'Sp_RTA_PerfilJefesLista actualizado.';
GO

/* ======================================== 3. la cabecera, con el codigo ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilColaborador','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilColaborador;
GO

/* Identico al que esta en produccion salvo UNA linea de datos:
   CodJefeInmediato en el primer SELECT. Es aditivo -el Dao lee por nombre de
   columna, no por posicion- asi que ni "Mi perfil" ni "Perfiles del personal"
   se enteran del cambio.

   Se vuelve a crear ENTERO y no se parchea: es un procedimiento de nueve
   conjuntos de resultados que leen las dos pantallas, y un ALTER parcial de algo
   asi es justo el tipo de cambio que se rompe sin avisar.

   El cuerpo se copio de docs/sql/2026-09-15-perfil-colaborador-fase3a.sql, que
   es el mas reciente de los tres scripts del repositorio que lo crean y el unico
   que coincide EXACTAMENTE con lo que estaba corriendo en produccion el
   2026-09-17 (los de la fase 1 y la fase 2 difieren en 39 y 18 lineas: son
   versiones viejas, y copiar de ahi habria revertido las fases posteriores). */
CREATE PROCEDURE dbo.Sp_RTA_PerfilColaborador
    @Cod_Usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    /* R_Usuarios no tiene indice unico sobre Cod_Usuario -su llave real es
       Id_Usuario- y hay codigos repetidos entre usuarios activos (el caso
       '0000' son dos personas distintas). Si se entregara CUALQUIERA de los
       nueve result sets en ese caso, el contacto personal, los contactos de
       emergencia y todo lo demas que cuelga de Cod_Usuario vendria mezclado
       o seria de la otra persona -domicilio, telefono personal, a
       quien llamar en una emergencia-, sin forma de saber de quien es cada
       dato. Y aunque la pantalla oculte las pestanas cuando no hay perfil, el
       JSON ya viajo al navegador: cerrar la puerta de la cabecera y dejar
       las demas abiertas serviria de poco.

       Por eso @CodigoRepetido se aplica en el WHERE de los nueve SELECT, no
       solo en la cabecera: el criterio es "no se entrega nada", no "no se
       entrega la cabecera". No hay un RETURN anticipado a proposito -el Dao
       recorre los result sets por posicion y espera que los nueve siempre
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

            /* El codigo, ademas del nombre. JefeInmediato sirve para mostrar;
               este sirve para preseleccionar el combo de la edicion de datos
               personales y para comparar al validar.

               Sale de u.Cod_Jefe_Inm y NO de j: si el jefe guardado esta
               inactivo no empareja en el LEFT JOIN, j.Nom_Usuario queda NULL y
               leer el codigo desde ahi lo daria por "sin jefe". Al guardar
               cualquier otro campo, ese jefe se perderia y la persona quedaria
               sin aprobador. */
            CodJefeInmediato = LTRIM(RTRIM(ISNULL(u.Cod_Jefe_Inm, ''))),

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

PRINT 'Sp_RTA_PerfilColaborador actualizado.';
GO

SET NOEXEC OFF;
GO

PRINT '== Datos personales: procedimientos - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   Esperado: 2 filas.

SELECT nombre = name, creado = CONVERT(VARCHAR(20), modify_date, 120)
  FROM sys.procedures
 WHERE name IN ('Sp_RTA_PerfilGuardarDatosPersonales','Sp_RTA_PerfilJefesLista');

   ============================================================================ */
