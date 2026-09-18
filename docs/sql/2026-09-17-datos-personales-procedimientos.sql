/* ============================================================================
   Datos personales editables: el guardado
   ReporTarea  |  2026-09-17

   PENDIENTE DE EJECUTAR. Corre DESPUES de
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
