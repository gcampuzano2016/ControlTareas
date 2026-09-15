/* ============================================================================
   Perfil del colaborador - FASE 3b
   La vista de jefatura: una jefatura consulta -sin editar- lo indispensable
   de las personas que le reportan.

   Contenido:
     1. Sp_RTA_PerfilEquipoLista
     2. Sp_RTA_PerfilEquipo
     3. Aserciones

   Esta es la unica parte del modulo donde alguien lee datos de otra persona.
   La guarda vive aqui y no en C#: los dos procedimientos exigen que el
   subordinado le reporte al jefe, y devuelven cero filas si no.

   No crea ni altera ninguna tabla.
   Idempotente: se puede correr dos veces sin dano.
   ============================================================================ */

SET NOCOUNT ON;
GO
/* QUOTED_IDENTIFIER y ANSI_NULLS explicitos y dentro del script: DESPLIEGUE.md
   dice que esto lo corre una persona a mano y sqlcmd los deja apagados por
   omision. En la fase 1 eso aborto el script a media ejecucion (error 1934). */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ----------------------------------------- 1. Sp_RTA_PerfilEquipoLista --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEquipoLista','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEquipoLista;
GO

/* Quien le reporta directamente a esta persona. Sin recursion: un jefe de jefe
   no ve a los nietos, que es lo que dice el diseno y lo que permite el dato
   disponible -Cod_Jefe_Inm es el jefe inmediato y nada mas-.

   El equipo mas grande de la empresa es de 49 personas, asi que no hay
   paginacion: sobra con filtrar por texto en el servidor. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEquipoLista
    @Cod_Jefe VARCHAR(50),
    @Filtro   VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Jefe AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    DECLARE @F VARCHAR(100) = LTRIM(RTRIM(ISNULL(@Filtro, '')));

    /* LTRIM(RTRIM(...)) en los DOS lados de la comparacion, siempre. Los
       valores de Cod_Jefe_Inm traen relleno y una comparacion cruda devuelve
       equipos vacios sin dar ningun error. Es la misma forma que usa el
       calculo de EsJefe que ya esta en produccion. */
    SELECT  CodUsuario     = LTRIM(RTRIM(u.Cod_Usuario)),
            NombreCompleto = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario),
            Cargo          = ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo),
            Area           = ISNULL(NULLIF(LTRIM(RTRIM(e.AreaTrabajo)), ''), u.Departamento),
            Ciudad         = e.Ciudad
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados e ON e.Cod_Usuario = u.Cod_Usuario
     WHERE  LTRIM(RTRIM(u.Cod_Jefe_Inm)) = LTRIM(RTRIM(@Cod_Jefe))
       AND  ISNULL(u.EstadoUsuario, 0) = 0
       AND  @CodigoRepetido = 0
       AND  (@F = ''
             OR ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario) LIKE '%' + @F + '%'
             OR LTRIM(RTRIM(u.Cod_Usuario)) LIKE '%' + @F + '%'
             OR ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo) LIKE '%' + @F + '%')
     ORDER BY NombreCompleto;
END
GO
PRINT 'Sp_RTA_PerfilEquipoLista creado.';
GO

/* ------------------------------------------- 2. Fn_RTA_EsSubordinado --- */

IF OBJECT_ID('dbo.Fn_RTA_EsSubordinado','FN') IS NOT NULL
    DROP FUNCTION dbo.Fn_RTA_EsSubordinado;
GO

/* La guarda, en una funcion y no escrita a mano dentro del procedimiento.

   El motivo es que se pueda PROBAR. La verificacion numero 3 del diseno -"que
   un codUsuario que no es subordinado devuelva vacio"- no se puede cubrir con
   una prueba unitaria, y contra la base tampoco se puede capturar la salida de
   Sp_RTA_PerfilEquipo con INSERT ... EXEC, porque devuelve seis result sets de
   formas distintas y ese INSERT intenta meterlos todos en la misma tabla.

   Sacando la condicion aqui, la demostracion llama exactamente a la MISMA
   funcion que usa el procedimiento, en vez de a una copia de su logica que
   podria divergir sin que nadie se entere. Una prueba que verifica un duplicado
   del codigo no verifica el codigo.

   LTRIM(RTRIM(...)) en los dos lados: Cod_Jefe_Inm trae relleno. */
CREATE FUNCTION dbo.Fn_RTA_EsSubordinado
(
    @Cod_Jefe    VARCHAR(50),
    @Cod_Usuario VARCHAR(50)
)
RETURNS BIT
AS
BEGIN
    DECLARE @EsSubordinado BIT = 0;

    IF EXISTS (SELECT 1 FROM dbo.R_Usuarios s
                WHERE LTRIM(RTRIM(s.Cod_Usuario))  = LTRIM(RTRIM(@Cod_Usuario))
                  AND LTRIM(RTRIM(s.Cod_Jefe_Inm)) = LTRIM(RTRIM(@Cod_Jefe))
                  AND ISNULL(s.EstadoUsuario, 0) = 0)
        SET @EsSubordinado = 1;

    RETURN @EsSubordinado;
END
GO
PRINT 'Fn_RTA_EsSubordinado creada.';
GO

/* ---------------------------------------------- 3. Sp_RTA_PerfilEquipo --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEquipo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEquipo;
GO

/* Lo que una jefatura puede ver de alguien de su equipo. SEIS result sets, en
   este orden: cabecera recortada, contactos de emergencia, estudios,
   certificaciones, experiencia, foto. El Dao los recorre POR POSICION, igual
   que en Sp_RTA_PerfilColaborador: si algun dia hay que anadir algo, va al
   final y no en medio.

   Lo que NO devuelve, a proposito, porque la matriz de permisos del diseno no
   se lo concede a la jefatura: cedula, fecha de nacimiento, edad, domicilio,
   correo y telefono personales, estado civil, cargas familiares y documentos
   de respaldo.

   Los documentos merecen su propia frase: la matriz concede certificaciones
   pero calla sobre sus respaldos, y el respaldo de una certificacion suele ser
   un escaneo con la cedula impresa -justo el campo que la matriz niega-. Un
   adjunto seria la puerta trasera de la restriccion.

   LA GUARDA. @EsSubordinado decide todo: si esa persona no le reporta a este
   jefe, los seis SELECT devuelven cero filas. No hay RETURN anticipado a
   proposito, porque el Dao recorre por posicion y espera que los seis
   conjuntos vengan siempre, aunque vacios; un RETURN rompe ese contrato. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEquipo
    @Cod_Jefe    VARCHAR(50),
    @Cod_Usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    /* La guarda y el SELECT de la cabecera tienen que comparar exactamente
       igual -recortado, y filtrando el mismo EstadoUsuario-. Si esta cuenta
       vigilara un conjunto mas chico que el que el SELECT entrega, un gemelo
       inactivo o un codigo con relleno a la izquierda pasaria la guarda sin
       contarse aqui y el SELECT devolveria dos filas de dos personas
       distintas, que el Dao mezclaria en una sola lectura. */
    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE LTRIM(RTRIM(Cod_Usuario)) = LTRIM(RTRIM(@Cod_Jefe))
           AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    /* El codigo del subordinado tambien puede estar repetido: en ese caso no se
       sabe de quien serian los datos y tampoco se entrega nada. */
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE LTRIM(RTRIM(Cod_Usuario)) = LTRIM(RTRIM(@Cod_Usuario))
           AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    /* La condicion vive en Fn_RTA_EsSubordinado y no aqui, para que la
       demostracion en SQL pueda llamar a la misma funcion en vez de a una copia
       de su logica. */
    DECLARE @EsSubordinado BIT = 0;

    IF @CodigoRepetido = 0
        SET @EsSubordinado = dbo.Fn_RTA_EsSubordinado(@Cod_Jefe, @Cod_Usuario);

    /* 1. cabecera recortada */
    SELECT  CodUsuario     = LTRIM(RTRIM(u.Cod_Usuario)),
            NombreCompleto = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario),
            Cargo          = ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo),
            Area           = ISNULL(NULLIF(LTRIM(RTRIM(e.AreaTrabajo)), ''), u.Departamento),
            Ciudad         = e.Ciudad,
            CorreoNotificacion = u.E_Mail,
            JefeInmediato  = j.Nom_Usuario,

            /* Subconsulta y no LEFT JOIN, por la misma razon que en
               Sp_RTA_PerfilColaborador: hay 5 usuarios con mas de una
               asignacion activa y un join los duplicaria, cuando este SELECT
               tiene que devolver exactamente una fila. */
            Horario = (SELECT TOP 1 h.Nombre
                         FROM dbo.R_UsuarioHorarioLaboral uh
                         JOIN dbo.R_HorarioLaboral h
                              ON h.IdHorarioLaboral = uh.IdHorarioLaboral
                        WHERE uh.Id_Responsable = u.Cod_Usuario
                          AND uh.Activo = 1
                        ORDER BY uh.FechaDesde DESC, uh.IdUsuarioHorario DESC)
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados  e ON e.Cod_Usuario = u.Cod_Usuario
      LEFT JOIN dbo.R_Usuarios j ON LTRIM(RTRIM(j.Cod_Usuario)) = LTRIM(RTRIM(u.Cod_Jefe_Inm))
     WHERE  LTRIM(RTRIM(u.Cod_Usuario)) = LTRIM(RTRIM(@Cod_Usuario))
       AND  ISNULL(u.EstadoUsuario, 0) = 0
       AND  @EsSubordinado = 1;

    /* 2. contactos de emergencia */
    SELECT IdContacto, Nombre, Parentesco, Telefono
      FROM dbo.Perfil_ContactoEmergencia
     WHERE LTRIM(RTRIM(Cod_Usuario)) = LTRIM(RTRIM(@Cod_Usuario))
       AND Estado = '1'
       AND @EsSubordinado = 1
     ORDER BY IdContacto;

    /* 3. estudios */
    SELECT IdEstudio, Nivel, Institucion, Titulo, AnioGraduacion
      FROM dbo.Perfil_Estudio
     WHERE LTRIM(RTRIM(Cod_Usuario)) = LTRIM(RTRIM(@Cod_Usuario))
       AND Estado = '1'
       AND @EsSubordinado = 1
     ORDER BY ISNULL(AnioGraduacion, 0) DESC, IdEstudio;

    /* 4. certificaciones */
    SELECT IdCertificacion, Nombre, Entidad, FechaObtencion
      FROM dbo.Perfil_Certificacion
     WHERE LTRIM(RTRIM(Cod_Usuario)) = LTRIM(RTRIM(@Cod_Usuario))
       AND Estado = '1'
       AND @EsSubordinado = 1
     ORDER BY FechaObtencion DESC, IdCertificacion;

    /* 5. experiencia */
    SELECT IdExperiencia, Empresa, Cargo, AnioDesde, AnioHasta, Funciones
      FROM dbo.Perfil_Experiencia
     WHERE LTRIM(RTRIM(Cod_Usuario)) = LTRIM(RTRIM(@Cod_Usuario))
       AND Estado = '1'
       AND @EsSubordinado = 1
     ORDER BY ISNULL(AnioHasta, 9999) DESC, AnioDesde DESC;

    /* 6. foto */
    SELECT FotoBase64, FotoTipo
      FROM dbo.Perfil_Foto
     WHERE LTRIM(RTRIM(Cod_Usuario)) = LTRIM(RTRIM(@Cod_Usuario))
       AND @EsSubordinado = 1;
END
GO
PRINT 'Sp_RTA_PerfilEquipo creado.';
GO

/* ------------------------------------------------------- 4. aserciones --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEquipoLista','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEquipoLista no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEquipo','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEquipo no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Fn_RTA_EsSubordinado','FN') IS NULL
    RAISERROR('FALLO: Fn_RTA_EsSubordinado no quedo creada.', 16, 1);

/* La guarda tiene que estar en el texto del procedimiento. Se apunta a que
   LLAME a la funcion Y a que su resultado filtre los SELECT: si alguien
   quitara la condicion de los SELECT dejando la llamada, un LIKE contra el
   nombre de la funcion solo seguiria pasando. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilEquipo'
                 AND m.definition LIKE '%Fn_RTA_EsSubordinado%'
                 AND m.definition LIKE '%AND @EsSubordinado = 1%')
    RAISERROR('FALLO: Sp_RTA_PerfilEquipo no tiene la guarda de jefatura.', 16, 1);

/* La guarda tiene que negar de verdad, no solo existir. Dos casos construidos
   con datos reales: alguien contra su propio jefe da 1, y contra un jefe que
   no es el suyo da 0. Si estas dos no se cumplen, nada de lo de arriba importa. */
DECLARE @J VARCHAR(50), @S VARCHAR(50);

SELECT TOP 1 @J = LTRIM(RTRIM(s.Cod_Jefe_Inm)), @S = LTRIM(RTRIM(s.Cod_Usuario))
  FROM dbo.R_Usuarios s
  JOIN dbo.R_Usuarios u ON LTRIM(RTRIM(u.Cod_Usuario)) = LTRIM(RTRIM(s.Cod_Jefe_Inm))
                       AND ISNULL(u.EstadoUsuario,0) = 0
 WHERE ISNULL(s.EstadoUsuario,0) = 0
 ORDER BY s.Cod_Usuario;

IF @J IS NOT NULL
BEGIN
    IF dbo.Fn_RTA_EsSubordinado(@J, @S) <> 1
        RAISERROR('FALLO: la guarda no reconoce a un subordinado real.', 16, 1);

    /* Un codigo que no existe, y no la relacion invertida: invertirla dependeria
       de que no haya ciclos de dos en el arbol de jefaturas, y eso no esta
       comprobado. Esto si es determinista. La demostracion completa
       -prueba-guarda-jefatura.sql- hace la version fuerte, con una persona real
       que no es del equipo. */
    IF dbo.Fn_RTA_EsSubordinado(@J, 'ZZ-NO-EXISTE-ZZ') <> 0
        RAISERROR('FALLO: la guarda acepta a alguien que no existe.', 16, 1);
END

/* Los seis conjuntos: si faltara uno, el Dao -que recorre por posicion- leeria
   la lista equivocada sin quejarse. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilEquipo'
                 AND m.definition LIKE '%FROM dbo.Perfil_ContactoEmergencia%'
                 AND m.definition LIKE '%FROM dbo.Perfil_Estudio%'
                 AND m.definition LIKE '%FROM dbo.Perfil_Certificacion%'
                 AND m.definition LIKE '%FROM dbo.Perfil_Experiencia%'
                 AND m.definition LIKE '%FROM dbo.Perfil_Foto%')
    RAISERROR('FALLO: a Sp_RTA_PerfilEquipo le falta alguno de los seis conjuntos.', 16, 1);

/* Lo que NO puede aparecer. La matriz de permisos niega estos campos a la
   jefatura, y esta asercion es lo que impide que vuelvan por descuido en una
   edicion futura del procedimiento. */
IF EXISTS (SELECT 1 FROM sys.sql_modules m
            JOIN sys.procedures p ON p.object_id = m.object_id
           WHERE p.name = 'Sp_RTA_PerfilEquipo'
             AND (m.definition LIKE '%Perfil_ContactoPersonal%'
                  OR m.definition LIKE '%Emp_CargaFamiliar%'
                  OR m.definition LIKE '%Perfil_Documento%'
                  OR m.definition LIKE '%Fecha_nacimiento%'
                  /* Empleados ya esta unida para traer nombre, cargo, area y
                     ciudad, asi que anadir una de estas tres columnas al SELECT
                     seria trivial y una lista de tablas no lo veria. Se busca la
                     columna CON el alias y nada mas: en este archivo toda columna
                     de Empleados se lee como e.Algo, incluso en la forma
                     "Alias = e.Algo". Un patron mas ancho -la palabra suelta-
                     casaria contra el comentario de este mismo procedimiento, que
                     nombra la cedula y el domicilio justo para decir que NO se
                     entregan, y la asercion fallaria en cada ejecucion. Una
                     asercion que grita en falso acaba desactivada. */
                  OR m.definition LIKE '%e.Cedula%'
                  OR m.definition LIKE '%e.EstadoCivil%'
                  OR m.definition LIKE '%e.Direccion%'))
    RAISERROR('FALLO: Sp_RTA_PerfilEquipo toca datos que la matriz de permisos le niega a la jefatura.', 16, 1);

/* La misma prohibicion, pero preguntando por las COLUMNAS que el procedimiento
   devuelve en vez de por su texto. Esto no se puede burlar escribiendo la
   columna de otra forma: mira lo que la jefatura recibe.

   Solo describe el PRIMER result set, que es la cabecera -y es donde
   aterrizarian estos campos si alguien los anadiera-. Los otros cinco salen de
   tablas Perfil_* que la asercion de arriba ya vigila por nombre de tabla. */
IF EXISTS (SELECT 1
             FROM sys.dm_exec_describe_first_result_set(
                      N'EXEC dbo.Sp_RTA_PerfilEquipo @Cod_Jefe = NULL, @Cod_Usuario = NULL', NULL, 0)
            WHERE name IN ('Cedula', 'FechaNacTexto', 'Fecha_nacimiento', 'Edad',
                           'Direccion', 'Domicilio', 'CorreoPersonal', 'TelefonoPersonal',
                           'EstadoCivil'))
    RAISERROR('FALLO: la cabecera de Sp_RTA_PerfilEquipo devuelve una columna que la matriz de permisos le niega a la jefatura.', 16, 1);

PRINT 'Fase 3b: script terminado.';
GO
