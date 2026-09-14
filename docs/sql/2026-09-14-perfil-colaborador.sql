/* ============================================================================
   Modulo: Perfil del colaborador (MiPerfil.aspx)
   Spec  : docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md

   1. Siete tablas nuevas, todas con eje Cod_Usuario
   2. Cod_Usuario en Empleados y en Emp_CargaFamiliar
   3. Poblado del enlace por cedula
   4. Sp_RTA_PerfilColaborador
   6-7. Las agregan tareas posteriores (guardar contacto y demas pantallas),
        antes de la seccion 8, para que las aserciones sigan corriendo al final.
   8. Aserciones y reporte de excepciones para RRHH

   Las secciones estan en el orden en que deben ejecutarse.
   Se puede ejecutar varias veces sin efecto adicional.
   Base: ReporTarea
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ------------------------------------------------------------ 1. tablas --- */

/* Una fila por persona: aca no hay borrado logico porque no significa nada.
   Nace esta tabla, y no se reusa Empleados.Correo, porque ese campo tiene 108
   direcciones corporativas y 23 externas: no es un correo personal, es un
   correo a secas. Ver la seccion "Ninguno de los dos correos..." del spec. */
IF OBJECT_ID('dbo.Perfil_ContactoPersonal','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_ContactoPersonal
    (
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        CorreoPersonal   VARCHAR(150) NULL,
        TelefonoPersonal VARCHAR(50)  NULL,
        Direccion        VARCHAR(400) NULL,

        Fec_Modificacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilContactoPersonal_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_ContactoPersonal PRIMARY KEY (Cod_Usuario)
    );
    PRINT 'Perfil_ContactoPersonal creada.';
END
ELSE PRINT 'Perfil_ContactoPersonal ya existia.';
GO

IF OBJECT_ID('dbo.Perfil_ContactoEmergencia','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_ContactoEmergencia
    (
        IdContacto       INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        Nombre           VARCHAR(150) NOT NULL,
        Parentesco       VARCHAR(50)  NULL,
        Telefono         VARCHAR(50)  NULL,

        Estado           CHAR(1)      NOT NULL
            CONSTRAINT DF_PerfilContactoEmergencia_Estado DEFAULT ('1'),
        Fec_Modificacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilContactoEmergencia_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_ContactoEmergencia PRIMARY KEY (IdContacto)
    );
    CREATE INDEX IX_Perfil_ContactoEmergencia_Usuario
        ON dbo.Perfil_ContactoEmergencia (Cod_Usuario, Estado);
    PRINT 'Perfil_ContactoEmergencia creada.';
END
ELSE PRINT 'Perfil_ContactoEmergencia ya existia.';
GO

IF OBJECT_ID('dbo.Perfil_Estudio','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_Estudio
    (
        IdEstudio        INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        Nivel            VARCHAR(60)  NULL,
        Institucion      VARCHAR(200) NULL,
        Titulo           VARCHAR(200) NULL,
        AnioGraduacion   SMALLINT     NULL,

        Estado           CHAR(1)      NOT NULL
            CONSTRAINT DF_PerfilEstudio_Estado DEFAULT ('1'),
        Fec_Modificacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilEstudio_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_Estudio PRIMARY KEY (IdEstudio)
    );
    CREATE INDEX IX_Perfil_Estudio_Usuario ON dbo.Perfil_Estudio (Cod_Usuario, Estado);
    PRINT 'Perfil_Estudio creada.';
END
ELSE PRINT 'Perfil_Estudio ya existia.';
GO

IF OBJECT_ID('dbo.Perfil_Certificacion','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_Certificacion
    (
        IdCertificacion  INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        Nombre           VARCHAR(200) NULL,
        Entidad          VARCHAR(200) NULL,
        FechaObtencion   DATE         NULL,

        Estado           CHAR(1)      NOT NULL
            CONSTRAINT DF_PerfilCertificacion_Estado DEFAULT ('1'),
        Fec_Modificacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilCertificacion_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_Certificacion PRIMARY KEY (IdCertificacion)
    );
    CREATE INDEX IX_Perfil_Certificacion_Usuario
        ON dbo.Perfil_Certificacion (Cod_Usuario, Estado);
    PRINT 'Perfil_Certificacion creada.';
END
ELSE PRINT 'Perfil_Certificacion ya existia.';
GO

/* AnioDesde/AnioHasta separados y numericos, y no el "2017 - 2019" de texto
   libre de la maqueta, porque el CV ordena la experiencia por fecha y con
   texto no se puede. AnioHasta NULL significa "hasta hoy". */
IF OBJECT_ID('dbo.Perfil_Experiencia','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_Experiencia
    (
        IdExperiencia    INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        Empresa          VARCHAR(200) NULL,
        Cargo            VARCHAR(200) NULL,
        AnioDesde        SMALLINT     NULL,
        AnioHasta        SMALLINT     NULL,
        Funciones        VARCHAR(MAX) NULL,

        Estado           CHAR(1)      NOT NULL
            CONSTRAINT DF_PerfilExperiencia_Estado DEFAULT ('1'),
        Fec_Modificacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilExperiencia_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_Experiencia PRIMARY KEY (IdExperiencia)
    );
    CREATE INDEX IX_Perfil_Experiencia_Usuario
        ON dbo.Perfil_Experiencia (Cod_Usuario, Estado);
    PRINT 'Perfil_Experiencia creada.';
END
ELSE PRINT 'Perfil_Experiencia ya existia.';
GO

/* Una sola tabla para todos los respaldos. Origen dice de que cuelga cada uno
   e IdOrigen a cual. Asi el documento de una certificacion y el de una carga
   familiar se suben, listan y borran con el mismo codigo. */
IF OBJECT_ID('dbo.Perfil_Documento','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_Documento
    (
        IdDocumento         INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario         VARCHAR(50)  NOT NULL,
        Origen              VARCHAR(20)  NOT NULL,   -- CERTIFICACION | CARGAFAMILIAR
        IdOrigen            INT          NOT NULL,
        NombreArchivo       VARCHAR(260) NULL,
        NombreArchivoCodigo VARCHAR(260) NULL,
        Ruta                VARCHAR(400) NULL,

        Estado              CHAR(1)      NOT NULL
            CONSTRAINT DF_PerfilDocumento_Estado DEFAULT ('1'),
        Fec_Modificacion    DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilDocumento_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion    VARCHAR(50)  NULL,
        Ip_Modificacion     VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_Documento PRIMARY KEY (IdDocumento)
    );
    CREATE INDEX IX_Perfil_Documento_Origen
        ON dbo.Perfil_Documento (Cod_Usuario, Origen, IdOrigen, Estado);
    PRINT 'Perfil_Documento creada.';
END
ELSE PRINT 'Perfil_Documento ya existia.';
GO

/* Tabla aparte y no una columna de R_Usuarios: un VARCHAR(MAX) en la tabla que
   se lee en cada request del menu se paga en todas las pantallas. Mismo patron
   que la firma guardada (DaoFirmaUsuario): base64 + tipo, y el data URI se arma
   al leer. */
IF OBJECT_ID('dbo.Perfil_Foto','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_Foto
    (
        Cod_Usuario      VARCHAR(50)   NOT NULL,
        FotoBase64       VARCHAR(MAX)  NULL,
        FotoTipo         VARCHAR(50)   NULL,

        Fec_Modificacion DATETIME2(0)  NOT NULL
            CONSTRAINT DF_PerfilFoto_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)   NULL,
        Ip_Modificacion  VARCHAR(64)   NULL,

        CONSTRAINT PK_Perfil_Foto PRIMARY KEY (Cod_Usuario)
    );
    PRINT 'Perfil_Foto creada.';
END
ELSE PRINT 'Perfil_Foto ya existia.';
GO

/* ------------------------------------------- 2. Cod_Usuario en lo existente */

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Empleados' AND COLUMN_NAME = 'Cod_Usuario')
BEGIN
    ALTER TABLE dbo.Empleados ADD Cod_Usuario VARCHAR(50) NULL;
    PRINT 'Empleados.Cod_Usuario creada.';
END
ELSE PRINT 'Empleados.Cod_Usuario ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Empleados_Cod_Usuario')
BEGIN
    CREATE INDEX IX_Empleados_Cod_Usuario ON dbo.Empleados (Cod_Usuario);
    PRINT 'IX_Empleados_Cod_Usuario creado.';
END
GO

/* Emp_CargaFamiliar tiene 0 filas: nunca se uso. Recibe Cod_Usuario para que
   los 113 usuarios sin ficha de empleado tambien puedan registrar cargas. */
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Emp_CargaFamiliar' AND COLUMN_NAME = 'Cod_Usuario')
BEGIN
    ALTER TABLE dbo.Emp_CargaFamiliar ADD Cod_Usuario VARCHAR(50) NULL;
    PRINT 'Emp_CargaFamiliar.Cod_Usuario creada.';
END
ELSE PRINT 'Emp_CargaFamiliar.Cod_Usuario ya existia.';
GO

/* ------------------------------------------------- 3. poblado del enlace --- */

/* Se enlaza por cedula, que es la unica llave que resiste el dato real:
   Cod_Sap no sirve -63 de 133 empleados lo tienen en 0 o nulo-.

   Se excluyen a proposito las cedulas repetidas en R_Usuarios (6 usuarios
   activos): dos logins con la misma cedula apuntarian a la misma ficha y no
   hay forma de saber cual es cual. Salen en el reporte de la seccion 8 para
   que RRHH los resuelva. Es el mismo criterio de DaoFirmaUsuario cuando el
   codigo de usuario esta repetido: antes que adivinar, no responder.

   Solo se escribe donde esta vacio, asi que correrlo de nuevo no pisa un
   enlace corregido a mano. */
UPDATE e
   SET e.Cod_Usuario = u.Cod_Usuario
  FROM dbo.Empleados e
  JOIN (
        SELECT LTRIM(RTRIM(Cedula)) AS Cedula, MIN(Cod_Usuario) AS Cod_Usuario
          FROM dbo.R_Usuarios
         WHERE ISNULL(EstadoUsuario, 0) = 0
           AND ISNULL(LTRIM(RTRIM(Cedula)), '') <> ''
         GROUP BY LTRIM(RTRIM(Cedula))
        HAVING COUNT(*) = 1          -- cedula repetida: no se enlaza
       ) u
    ON LTRIM(RTRIM(e.Cedula)) = u.Cedula
 WHERE ISNULL(e.Cod_Usuario, '') = '';

PRINT 'Enlace poblado: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' fichas enlazadas en esta corrida.';
GO

/* --------------------------------------------------- 4. lectura del perfil */

IF OBJECT_ID('dbo.Sp_RTA_PerfilColaborador','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilColaborador;
GO

/* Todo el perfil de una persona en una sola ida: siete result sets.

   La cabecera es un LEFT JOIN contra Empleados a proposito. 113 de 231
   usuarios activos no tienen ficha enlazada; con INNER JOIN entrarian y verian
   una pantalla en blanco. Con LEFT ven su nombre, su area y todo lo que si se
   sabe de ellos, y las secciones nuevas -que cuelgan de Cod_Usuario- les
   funcionan completas.

   Fecha_nacimiento sale como TEXTO, sin convertir. Es nvarchar(50) con formato
   dd/MM/yyyy y convertirla aca obliga a acertarle al estilo (103) o la edad se
   rompe para 80 de 133 personas sin dar error. La conversion la hace
   NegPerfilCampos.EdadDesdeTexto, que tiene pruebas.

   Los result sets 3 a 7 devuelven vacio en la fase 1 porque todavia no hay
   pantalla que los llene. El procedimiento ya los declara para que las fases 2
   y 3 no tengan que volver a tocar la base. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilColaborador
    @Cod_Usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

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
     WHERE  u.Cod_Usuario = @Cod_Usuario;

    /* 2. contacto personal (editable) */
    SELECT  CorreoPersonal   = ISNULL(p.CorreoPersonal, ''),
            TelefonoPersonal = ISNULL(p.TelefonoPersonal, ''),
            Direccion        = ISNULL(p.Direccion, CAST(e.Direccion AS VARCHAR(400))),
            EstadoCivil      = ISNULL(e.EstadoCivil, '')
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Perfil_ContactoPersonal p ON p.Cod_Usuario = u.Cod_Usuario
      LEFT JOIN dbo.Empleados e ON e.Cod_Usuario = u.Cod_Usuario
     WHERE  u.Cod_Usuario = @Cod_Usuario;

    /* 3. contactos de emergencia */
    SELECT IdContacto, Nombre, Parentesco, Telefono
      FROM dbo.Perfil_ContactoEmergencia
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
     ORDER BY IdContacto;

    /* 4. estudios (fase 2) */
    SELECT IdEstudio, Nivel, Institucion, Titulo, AnioGraduacion
      FROM dbo.Perfil_Estudio
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
     ORDER BY AnioGraduacion DESC, IdEstudio;

    /* 5. certificaciones (fase 2) */
    SELECT IdCertificacion, Nombre, Entidad, FechaObtencion
      FROM dbo.Perfil_Certificacion
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
     ORDER BY FechaObtencion DESC, IdCertificacion;

    /* 6. experiencia (fase 2) */
    SELECT IdExperiencia, Empresa, Cargo, AnioDesde, AnioHasta, Funciones
      FROM dbo.Perfil_Experiencia
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
     ORDER BY ISNULL(AnioHasta, 9999) DESC, AnioDesde DESC;

    /* 7. documentos de respaldo (fase 3) */
    SELECT IdDocumento, Origen, IdOrigen, NombreArchivo, NombreArchivoCodigo, Ruta
      FROM dbo.Perfil_Documento
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
     ORDER BY IdDocumento;
END
GO
PRINT 'Sp_RTA_PerfilColaborador creado.';
GO

/* ------------------------------------------ 8. aserciones y excepciones --- */
/* Las secciones 6 y 7 (guardar contacto y las demas pantallas del modulo)
   las agregan tareas posteriores, insertadas antes de este bloque, para que
   las aserciones y el reporte de RRHH sigan corriendo al final del script. */

/* Aserciones: si algo de esto falla, el script no dejo la base como se espera. */
IF OBJECT_ID('dbo.Sp_RTA_PerfilColaborador','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilColaborador no quedo creado.', 16, 1);

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME='Empleados' AND COLUMN_NAME='Cod_Usuario')
    RAISERROR('FALLO: Empleados.Cod_Usuario no existe.', 16, 1);

/* Ninguna ficha puede quedar enlazada a dos usuarios distintos. */
IF EXISTS (SELECT 1 FROM dbo.Empleados
            WHERE ISNULL(Cod_Usuario,'') <> ''
            GROUP BY Cod_Usuario HAVING COUNT(*) > 1)
    RAISERROR('FALLO: hay un Cod_Usuario enlazado a mas de una ficha.', 16, 1);

PRINT 'Aserciones OK.';
GO

/* Reporte para RRHH. No cambia nada: solo lista lo que hay que corregir para
   que el modulo rinda completo. Exportar el resultado y enviarlo. */
PRINT '--- Excepciones para RRHH ---';

SELECT Caso = 'Usuario activo sin cedula (no se puede enlazar)',
       u.Cod_Usuario, u.Nom_Usuario, u.E_Mail, u.Departamento
  FROM dbo.R_Usuarios u
 WHERE ISNULL(u.EstadoUsuario,0) = 0
   AND ISNULL(LTRIM(RTRIM(u.Cedula)),'') = ''
UNION ALL
SELECT 'Usuario activo con cedula pero sin ficha de empleado',
       u.Cod_Usuario, u.Nom_Usuario, u.E_Mail, u.Departamento
  FROM dbo.R_Usuarios u
 WHERE ISNULL(u.EstadoUsuario,0) = 0
   AND ISNULL(LTRIM(RTRIM(u.Cedula)),'') <> ''
   AND NOT EXISTS (SELECT 1 FROM dbo.Empleados e
                    WHERE LTRIM(RTRIM(e.Cedula)) = LTRIM(RTRIM(u.Cedula)))
UNION ALL
SELECT 'Cedula repetida entre usuarios activos (no se enlazo ninguno)',
       u.Cod_Usuario, u.Nom_Usuario, u.E_Mail, u.Departamento
  FROM dbo.R_Usuarios u
 WHERE ISNULL(u.EstadoUsuario,0) = 0
   AND LTRIM(RTRIM(u.Cedula)) IN (
        SELECT LTRIM(RTRIM(Cedula)) FROM dbo.R_Usuarios
         WHERE ISNULL(EstadoUsuario,0) = 0 AND ISNULL(LTRIM(RTRIM(Cedula)),'') <> ''
         GROUP BY LTRIM(RTRIM(Cedula)) HAVING COUNT(*) > 1)
UNION ALL
SELECT 'Jefe inmediato que no corresponde a ningun usuario',
       u.Cod_Usuario, u.Nom_Usuario, u.E_Mail, u.Departamento
  FROM dbo.R_Usuarios u
 WHERE ISNULL(u.EstadoUsuario,0) = 0
   AND ISNULL(LTRIM(RTRIM(u.Cod_Jefe_Inm)),'') <> ''
   AND NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios j
                    WHERE LTRIM(RTRIM(j.Cod_Usuario)) = LTRIM(RTRIM(u.Cod_Jefe_Inm)))
 ORDER BY 1, 3;
GO

/* Fechas de nacimiento fuera de rango razonable: erratas de carga. */
SELECT Caso = 'Fecha de nacimiento fuera de rango', e.IdEmpleado, e.Cedula, e.Nombre,
       e.Fecha_nacimiento
  FROM dbo.Empleados e
 WHERE TRY_CONVERT(date, e.Fecha_nacimiento, 103) IS NOT NULL
   AND (TRY_CONVERT(date, e.Fecha_nacimiento, 103) > DATEADD(year, -15, GETDATE())
     OR TRY_CONVERT(date, e.Fecha_nacimiento, 103) < '1940-01-01');
GO

PRINT 'Script 2026-09-14-perfil-colaborador completado.';
GO
