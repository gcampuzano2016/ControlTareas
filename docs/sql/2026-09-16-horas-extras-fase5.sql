/* ============================================================================
   Horas Extras fase 5: los parametros se editan desde una pantalla
   ReporTarea  |  2026-09-16

   PENDIENTE DE EJECUTAR. El usuario lo corre en produccion, en este orden:
     1) las fases 1 a 4 (ya aplicadas)
     2) los binarios de la Task 1                (un periodo calcula con los
                                                  parametros que regian en el)
     3) ESTE SCRIPT                              (los dos procedimientos y el
                                                  registro en el menu)
     4) los binarios de las Tasks 3 y 4          (la pantalla)

   El paso 2 va ANTES que el 3 a proposito y no es preferencia. Hasta hoy
   HE_Parametro nunca tuvo mas de una version de una misma clave, y eso es lo
   unico que ha mantenido oculto que el calculo leia "el parametro vigente HOY"
   en vez de "el que regia en el periodo". Este script es justo el que permite
   que haya dos versiones. Si llegara antes que los binarios de la Task 1, el
   primer cambio de un parametro recalcularia periodos viejos con valores
   nuevos, en silencio.

   ----------------------------------------------------------------------------
   Que hace

     1. Sp_RTA_HeParametrosListar  - todo el historial, tambien el cerrado.
     2. Sp_RTA_HeParametroGuardar  - NO actualiza la fila vigente: la cierra e
                                     inserta una nueva. Ese es el punto entero.
     3. ParametrizacionHorasExtras.aspx entra al menu, colgada del mismo grupo
        que HorasExtras.aspx y visible para los perfiles 14 y 18.
     4. Un bloque de verificacion con el contador @Fallos.

   ----------------------------------------------------------------------------
   Lo que este script NO hace, y por que

   NO anade IdParametro a HE_Parametro. Una version anterior del plan decia que
   la columna no existia; es falso y la medicion que lo dijo estaba truncada. La
   tabla YA tiene IdParametro INT IDENTITY(1,1) como clave primaria, poblada del
   1 al 7. Cualquier ALTER para crearla fallaria y abortaria el script. Lo que si
   se hace es comprobar que sigue ahi, en el bloque de verificacion: los dos
   procedimientos la devuelven y la pantalla la necesita para saber que fila
   edita.

   NO toca la visibilidad del grupo padre (Id_Menu 20081, "Nomina"). Ya se la dio
   2026-09-16-horas-extras-fase2-menu.sql a los perfiles 14 y 18, y una hoja solo
   se ve si su grupo tambien esta visible para ese perfil. Si alguien apagara el
   grupo, la pantalla nueva desapareceria sin que este script se entere.

   Re-correrlo es inofensivo: el guard de abajo lo salta entero.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ----------------------------------------------------------- GUARD -------- */
/* Si la fase 5 ya esta aplicada, este script NO debe volver a correr.

   No es solo ruido. El bloque del menu es idempotente por su cuenta, pero los
   dos DROP/CREATE de mas abajo no saben nada de lo que venga despues: si una
   fase 6 le anade un parametro a Sp_RTA_HeParametroGuardar, una segunda pasada
   de ESTE script lo devolveria a cinco y cada guardado de la pantalla empezaria
   a fallar con "too many arguments", sin ninguna pista de la causa. Es el mismo
   accidente que el guard de la fase 2 existe para impedir.

   No hay forma de saltarse solo una seccion -CREATE PROCEDURE tiene que ser la
   primera instruccion de su lote y no se puede envolver en un IF-, asi que se
   salta el script entero con NOEXEC.

   La senal es Sp_RTA_HeParametroGuardar: es lo ultimo que esta fase crea que no
   existia antes de ella. */
IF OBJECT_ID('dbo.Sp_RTA_HeParametroGuardar','P') IS NOT NULL
BEGIN
    PRINT 'La fase 5 ya esta aplicada. No se hace nada.';
    SET NOEXEC ON;
END
GO

/* ------------------------------------------- 1. listar el historial -------- */
IF OBJECT_ID('dbo.Sp_RTA_HeParametrosListar','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeParametrosListar;
GO
/* TODAS las filas, tambien las de vigencia cerrada. No es un descuido: ver el
   historial es la mitad del valor de la pantalla. Quien mire un rol viejo tiene
   que poder responder "con que factor se pago esto" sin abrir la base, y esa
   respuesta esta justo en las filas que un "solo lo vigente" esconderia.

   El orden agrupa por clave y pone la version mas nueva primero, que es como la
   grilla las quiere pintar: una clave, y debajo de ella su pasado.

   Sin parametros a proposito. Son 7 claves y unas pocas versiones por clave;
   filtrar aqui seria complicar un SELECT que cabe entero en pantalla.

   El contrato posicional es
       IdParametro, Clave, Valor, FechaVigenciaDesde, FechaVigenciaHasta,
       Usu_Modificacion, Fec_Modificacion
   y se extiende solo por el final.

   OJO: este NO es el procedimiento que usa el calculo. El calculo lee por
   DaoHorasExtras.LeerHistorialParametros, que filtra por fecha de corte y no
   trae IdParametro. Son dos lectores distintos con necesidades distintas, a
   proposito. */
CREATE PROCEDURE dbo.Sp_RTA_HeParametrosListar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdParametro,
           Clave,
           Valor,
           FechaVigenciaDesde,
           FechaVigenciaHasta,
           Usu_Modificacion,
           Fec_Modificacion
      FROM dbo.HE_Parametro
     ORDER BY Clave, FechaVigenciaDesde DESC;
END
GO

/* ------------------------------------------- 2. guardar un parametro ------- */
IF OBJECT_ID('dbo.Sp_RTA_HeParametroGuardar','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeParametroGuardar;
GO
/* Cierra la version vigente e inserta una nueva. NO actualiza la fila que ya
   estaba, y eso es deliberado: un UPDATE en sitio cambiaria retroactivamente lo
   que se pago en periodos ya cerrados, sin dejar rastro de que el valor fue
   otro. Con dos filas, cada periodo sigue calculando con lo suyo -que es lo que
   la Task 1 dejo listo del lado de C#- y ademas queda escrito quien cambio que
   y desde cuando.

   Respuestas:
      0  guardado
     -1  la clave no es una de las siete conocidas
     -2  el valor no es mayor que cero
     -3  ya hay una version de esa clave que empieza ese mismo dia
     -4  la fecha no es posterior a la de la version que reemplaza
     -5  DecimalesMonto no admite mas de 2

   Sobre el -1: una clave inventada no rompe nada hoy -nadie la leeria- pero
   queda en la tabla para siempre, y dentro de un ano nadie sabra si algun
   calculo depende de ella. La lista blanca esta duplicada aqui y en
   NegHeParametroPantalla a proposito: la de negocio da un mensaje util a quien
   usa la pantalla, y esta protege la tabla de cualquier OTRO llamador.

   Sobre el -2: las siete son divisores, factores o contadores. Ninguna admite
   cero ni negativo, y un DiasMes en cero es una division por cero en el
   calculo del valor hora.

   Sobre el -3: la base ya lo impide por su cuenta -UX_HE_Parametro_Clave es
   UNICO sobre (Clave, FechaVigenciaDesde)-, pero sin esta validacion el usuario
   recibiria en la cara el mensaje crudo del indice unico en vez de un codigo
   que la pantalla sabe traducir.

   Sobre el -5: CK_HE_Parametro_DecimalesMonto exige
   "Clave <> 'DecimalesMonto' OR Valor <= 2", porque HE_Detalle.Total50,
   Total100 y TotalHE son DECIMAL(18,2) FIJOS: con 4 decimales C# calcularia
   75.0375 y la columna guardaria 75.04 sin error. Igual que el -3, la
   restriccion ya existe; el codigo esta para que el mensaje sea util.

   Devuelve una sola columna, Respuestas. El IdParametro nuevo no se devuelve
   porque nadie lo necesita: la pantalla recarga la grilla con
   Sp_RTA_HeParametrosListar y ahi lo lee. Si algun dia hiciera falta, entra
   como segunda columna al final. */
CREATE PROCEDURE dbo.Sp_RTA_HeParametroGuardar
    @Clave              VARCHAR(60),
    @Valor              DECIMAL(18,6),
    @FechaVigenciaDesde DATE,
    @Usuario            VARCHAR(50),
    @Ip                 VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    /* Si algo revienta dentro de la transaccion -por ejemplo el indice unico,
       que es la unica forma de perder una carrera contra otra sesion-, la
       transaccion se deshace sola y no queda abierta colgando de la conexion
       del sitio web. Un SET dentro de un procedimiento se restaura al salir,
       asi que esto no afecta a quien llama. */
    SET XACT_ABORT ON;

    /* ---- las validaciones baratas, antes de abrir la transaccion ---- */

    /* La lista blanca tambien normaliza la escritura. La comparacion de
       VARCHAR es insensible a mayusculas con la intercalacion de esta base, asi
       que 'factor50' pasaria el filtro y se guardaria con esa grafia: distinta
       de la que muestra la pantalla y distinta de la de las otras versiones de
       la misma clave. Quedarse con la grafia canonica cuesta una linea. */
    DECLARE @Claves TABLE (Clave VARCHAR(60) PRIMARY KEY);
    INSERT INTO @Claves (Clave) VALUES
        ('DecimalesMonto'),
        ('DiasMes'),
        ('Factor50'),
        ('Factor100'),
        ('HorasMesJornadaCompleta'),
        ('TopeDiario50'),
        ('TopeSemanal50');

    DECLARE @ClaveCanonica VARCHAR(60);

    SELECT @ClaveCanonica = C.Clave
      FROM @Claves AS C
     WHERE C.Clave = LTRIM(RTRIM(ISNULL(@Clave, '')));

    IF @ClaveCanonica IS NULL
    BEGIN
        SELECT Respuestas = -1;
        RETURN;
    END

    IF @Valor IS NULL OR @Valor <= 0
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    IF @ClaveCanonica = 'DecimalesMonto' AND @Valor > 2
    BEGIN
        SELECT Respuestas = -5;
        RETURN;
    END

    /* El NULL de la fecha entra por aqui y no mas abajo: con NULL las
       comparaciones de fecha dan UNKNOWN, no coincidirian con nada, y el
       INSERT reventaria contra el NOT NULL de la columna en vez de devolver un
       codigo. Sale por el -4 porque el mensaje de ese codigo habla justamente
       de la fecha de vigencia. */
    IF @FechaVigenciaDesde IS NULL
    BEGIN
        SELECT Respuestas = -4;
        RETURN;
    END

    /* ---- las que leen la tabla, y la escritura, en una sola transaccion ---- */

    /* Cerrar e insertar son dos escrituras que solo tienen sentido juntas: a
       medias, la clave se quedaria sin ninguna version abierta y el calculo
       caeria al valor por omision sin avisar.

       Las dos lecturas van con UPDLOCK, HOLDLOCK dentro de la misma
       transaccion. Sin eso, dos personas guardando la misma clave a la vez
       pueden pasar las dos validaciones y cerrar la misma fila dos veces,
       dejando dos versiones abiertas con fechas distintas. El indice unico NO
       lo atrapa: solo impide la fecha repetida. */
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1
                 FROM dbo.HE_Parametro WITH (UPDLOCK, HOLDLOCK)
                WHERE Clave = @ClaveCanonica
                  AND FechaVigenciaDesde = @FechaVigenciaDesde)
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT Respuestas = -3;
        RETURN;
    END

    /* La fecha de la ultima version que existe de esa clave, este abierta o
       cerrada. Se mira la ultima de TODAS y no solo la abierta a proposito: si
       alguien cerrara la vigente a mano sin abrir otra, permitir una version
       nueva por debajo de la ultima fecha dejaria el historial desordenado y
       la eleccion de version pasaria a depender de como se lean las filas. */
    DECLARE @DesdeUltima DATE;

    SELECT @DesdeUltima = MAX(FechaVigenciaDesde)
      FROM dbo.HE_Parametro WITH (UPDLOCK, HOLDLOCK)
     WHERE Clave = @ClaveCanonica;

    /* @DesdeUltima NULL significa que la clave todavia no tiene ninguna
       version. Hoy no puede pasar -las siete estan cargadas desde el
       2026-09-01- pero si pasara, la primera version puede empezar cuando
       quiera y no hay nada que cerrar. */
    IF @DesdeUltima IS NOT NULL AND @FechaVigenciaDesde <= @DesdeUltima
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT Respuestas = -4;
        RETURN;
    END

    /* Se cierra el dia ANTERIOR al que empieza la nueva, de modo que las dos
       vigencias no comparten ni un dia. FechaVigenciaHasta es inclusiva -el dia
       del cierre todavia rige-, que es como la lee ValorAlCorte en C#.

       El UPDATE va por conjunto y no por IdParametro: si por lo que sea hubiera
       mas de una version abierta de la clave, las cierra todas en vez de dejar
       una suelta.

       Fec_Modificacion, Usu_Modificacion e Ip_Modificacion de la fila que se
       cierra NO se tocan. Esas columnas dicen quien puso ESE valor, y la
       pantalla las muestra por fila; pisarlas al cerrar atribuiria el valor
       viejo a quien lo reemplazo, que es exactamente lo contrario de lo que el
       historial existe para contar. Quien cerro se deduce de la fila nueva. */
    UPDATE dbo.HE_Parametro
       SET FechaVigenciaHasta = DATEADD(DAY, -1, @FechaVigenciaDesde)
     WHERE Clave = @ClaveCanonica
       AND FechaVigenciaHasta IS NULL;

    INSERT INTO dbo.HE_Parametro
        (Clave, Valor, FechaVigenciaDesde, FechaVigenciaHasta,
         Fec_Modificacion, Usu_Modificacion, Ip_Modificacion)
    VALUES
        (@ClaveCanonica, @Valor, @FechaVigenciaDesde, NULL,
         SYSDATETIME(), @Usuario, @Ip);

    COMMIT TRANSACTION;

    SELECT Respuestas = 0;
END
GO

/* ------------------------------------------- 3. la pantalla entra al menu -- */

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

/* La hoja nueva se CLONA de HorasExtras.aspx, que es su hermana y cuelga del
   grupo al que tiene que colgar ella. dbo.MenuDos tiene 18 columnas, varias
   NOT NULL con default y de casing inconsistente; enumerarlas a mano es la
   forma segura de olvidarse de una. El clon copia por metadata TODAS las
   columnas menos la clave, asi que arrastra hasta las que no conocemos, y
   despues se corrigen las cinco que distinguen a una pantalla de la otra.

   El padre se toma del modelo y no se escribe a pelo: "el mismo grupo que
   Horas Extras" es el requisito, y 20081 es solo el valor que ese grupo tiene
   hoy. El bloque de verificacion comprueba las dos cosas por separado, para
   distinguir "el script hizo algo raro" de "el mundo cambio". */
DECLARE @HrefModelo VARCHAR(512) = 'HorasExtras.aspx';
DECLARE @HrefHoja   VARCHAR(512) = 'ParametrizacionHorasExtras.aspx';
DECLARE @TituloHoja VARCHAR(128) = 'Parametros de Horas Extras';
DECLARE @DescHoja   VARCHAR(512) = 'Parametros de calculo de horas extras';

DECLARE @IdModelo INT;
DECLARE @IdPadre  INT;
DECLARE @IdHoja   INT;
DECLARE @Orden    INT;

SELECT @IdModelo = Id_Menu, @IdPadre = Id_MenuPadre
  FROM dbo.MenuDos
 WHERE Href = @HrefModelo;

IF @IdModelo IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('No existe la fila de menu de HorasExtras.aspx: falta correr 2026-09-16-horas-extras-fase2-menu.sql antes que este script.', 16, 1);
    RETURN;
END

/* Las columnas no-identidad de MenuDos, menos Id_Menu, que se trata aparte. */
DECLARE @cols NVARCHAR(MAX) = STUFF
((
    SELECT ',' + QUOTENAME(c.name)
    FROM sys.columns AS c
    WHERE c.object_id = OBJECT_ID('dbo.MenuDos')
      AND c.is_identity = 0
      AND c.is_computed = 0
      AND c.name <> 'Id_Menu'
    ORDER BY c.column_id
    FOR XML PATH(''), TYPE
).value('.', 'NVARCHAR(MAX)'), 1, 1, '');

/* Si Id_Menu es IDENTITY, el valor lo pone la base; si no lo es, hay que
   calcularlo. Las dos ramas existen porque las fuentes se contradicen: el
   script de la pantalla hermana y el del catalogo de horarios dan por hecho
   que es IDENTITY -los dos usan SCOPE_IDENTITY() y los dos corrieron-, y la
   medicion del 2026-09-16 dice que no lo es. Una de las dos esta equivocada y
   no se puede comprobar desde aqui sin tocar la base, asi que el script
   funciona igual en los dos mundos en vez de apostar por uno. */
DECLARE @EsIdentidad BIT =
    CASE WHEN EXISTS (SELECT 1 FROM sys.columns
                       WHERE object_id = OBJECT_ID('dbo.MenuDos')
                         AND name = 'Id_Menu'
                         AND is_identity = 1)
         THEN 1 ELSE 0 END;

SELECT @IdHoja = Id_Menu
  FROM dbo.MenuDos
 WHERE Href = @HrefHoja;

IF @IdHoja IS NULL
BEGIN
    DECLARE @sql NVARCHAR(MAX);

    IF @EsIdentidad = 1
    BEGIN
        SET @sql =
            N'INSERT INTO dbo.MenuDos (' + @cols + N') ' +
            N'SELECT ' + @cols + N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo; ' +
            N'SET @IdOut = CONVERT(INT, SCOPE_IDENTITY());';

        EXEC sys.sp_executesql @sql,
             N'@Modelo INT, @IdOut INT OUTPUT',
             @Modelo = @IdModelo,
             @IdOut  = @IdHoja OUTPUT;
    END
    ELSE
    BEGIN
        /* MAX + 1 y no el 20083 escrito a pelo: si alguien anadio un menu entre
           la medicion y la ejecucion, el valor fijo chocaria contra la clave
           primaria. TABLOCKX porque dos sesiones calculando MAX a la vez
           sacarian el mismo numero. */
        SELECT @IdHoja = ISNULL(MAX(Id_Menu), 0) + 1
          FROM dbo.MenuDos WITH (TABLOCKX, HOLDLOCK);

        SET @sql =
            N'INSERT INTO dbo.MenuDos (' + QUOTENAME('Id_Menu') + N',' + @cols + N') ' +
            N'SELECT @IdNuevo, ' + @cols + N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo;';

        EXEC sys.sp_executesql @sql,
             N'@Modelo INT, @IdNuevo INT',
             @Modelo  = @IdModelo,
             @IdNuevo = @IdHoja;
    END

    /* El orden dentro del grupo. En el estado medido el 2026-09-16 el grupo
       tiene un solo hijo con Orden_Opcion = 1, asi que esto da 2, que es lo que
       pide el plan. Se calcula en vez de escribirse por la misma razon que el
       Id_Menu: si entre medias apareciera otra pantalla de nomina, un 2 fijo
       empataria con ella y el orden pasaria a decidirlo la base. */
    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1
      FROM dbo.MenuDos
     WHERE Id_MenuPadre = @IdPadre
       AND Id_Menu <> @IdHoja;

    UPDATE dbo.MenuDos
       SET Href              = @HrefHoja,
           Titulo            = @TituloHoja,
           Descripcion       = @DescHoja,
           Id_MenuPadre      = @IdPadre,
           Es_Opcion_de_Menu = 0,
           Orden_Opcion      = @Orden
     WHERE Id_Menu = @IdHoja;

    PRINT 'MenuDos: hoja ParametrizacionHorasExtras.aspx creada con Id_Menu = '
        + CONVERT(VARCHAR(20), @IdHoja) + ', bajo el grupo '
        + CONVERT(VARCHAR(20), @IdPadre) + '.';
END
ELSE
BEGIN
    /* Ya existia: puede ser una corrida anterior de este mismo script, o una
       insercion a mano colgada de otro sitio. Se re-engancha al grupo correcto
       en vez de dejarla suelta, pero no se duplica. */
    UPDATE dbo.MenuDos
       SET Id_MenuPadre = @IdPadre
     WHERE Id_Menu = @IdHoja
       AND Id_MenuPadre <> @IdPadre;

    PRINT 'MenuDos: la hoja ParametrizacionHorasExtras.aspx ya existia (Id_Menu = '
        + CONVERT(VARCHAR(20), @IdHoja) + '). Sin duplicar.';
END

/* Visibilidad para los perfiles 14 (Talento Humano) y 18 (Super Admin).

   Estado = '0' MUESTRA y '1' OCULTA: la semantica esta invertida respecto a lo
   que el nombre sugiere, porque Sp_RTA_ConsultarMenuPerfilUsuario filtra
   WHERE P.Estado = 0. Contarlo al reves lleva a la conclusion opuesta sobre
   quien ve la pantalla.

   La pantalla muestra y cambia los factores con los que se paga la nomina de 64
   personas: la lista de perfiles es corta a proposito. */
DECLARE @Perfiles TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Perfiles (IdPerfil) VALUES (14), (18);

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT @IdHoja, P.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM @Perfiles AS P
 WHERE NOT EXISTS
 (
     SELECT 1 FROM dbo.PerfilMenu AS PM
      WHERE PM.id_Menu = @IdHoja
        AND PM.IdPerfil = P.IdPerfil
 );

/* Si alguna de esas dos filas ya existia pero apagada, se enciende: el objetivo
   es que los perfiles 14 y 18 vean la pantalla, no solo que la fila exista. */
UPDATE PM
   SET PM.Estado = '0'
  FROM dbo.PerfilMenu AS PM
  JOIN @Perfiles AS P ON P.IdPerfil = PM.IdPerfil
 WHERE PM.id_Menu = @IdHoja
   AND PM.Estado <> '0';

PRINT 'PerfilMenu: visibilidad asegurada para los perfiles 14 y 18 (Estado = 0).';

COMMIT TRANSACTION;
GO

/* Se devuelve como estaba: XACT_ABORT vive en la conexion, y dejarlo encendido
   cambiaria el comportamiento de todo lo que quien corre el script ejecute
   despues en la misma ventana. */
SET XACT_ABORT OFF;
GO

/* ------------------------------------------- 4. aserciones ----------------- */

/* Todo este bloque va en un solo batch, sin GO en medio: un GO entre el DECLARE
   y su ultimo uso hace fallar el script entero con "must declare the scalar
   variable". Por eso va entero al final y no intercalado entre cada CREATE.

   RAISERROR con severidad 16 no aborta el lote, asi que se cuentan todos los
   fallos y el veredicto va al final. */
DECLARE @Fallos INT = 0;

/* --- 1. los dos procedimientos --- */
IF OBJECT_ID('dbo.Sp_RTA_HeParametrosListar','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeParametrosListar no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeParametroGuardar','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeParametroGuardar no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

/* La firma completa. Si falta un parametro, el DAO de la Task 3 falla en cada
   guardado con un mensaje que no menciona cual. */
IF OBJECT_ID('dbo.Sp_RTA_HeParametroGuardar','P') IS NOT NULL
   AND (SELECT COUNT(*) FROM sys.parameters
         WHERE object_id = OBJECT_ID('dbo.Sp_RTA_HeParametroGuardar')
           AND name IN ('@Clave','@Valor','@FechaVigenciaDesde','@Usuario','@Ip')) <> 5
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeParametroGuardar no tiene los cinco parametros esperados.', 16, 1);
    SET @Fallos += 1;
END

/* --- 2. IdParametro sigue ahi y sigue siendo identidad --- */
/* Este script NO la crea; la comprueba. Si algun dia faltara, los dos
   procedimientos de arriba se habrian creado igual -la resolucion de nombres de
   columnas es diferida- y solo reventarian al ser llamados. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_Parametro')
                  AND name = 'IdParametro')
BEGIN
    RAISERROR('FALLO: HE_Parametro no tiene la columna IdParametro.', 16, 1);
    SET @Fallos += 1;
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns
                     WHERE object_id = OBJECT_ID('dbo.HE_Parametro')
                       AND name = 'IdParametro'
                       AND is_identity = 1)
BEGIN
    RAISERROR('FALLO: HE_Parametro.IdParametro existe pero no es IDENTITY: el INSERT de Sp_RTA_HeParametroGuardar no le daria valor.', 16, 1);
    SET @Fallos += 1;
END

/* --- 3. el listado devuelve por lo menos las siete claves --- */
/* INSERT ... EXEC para poder contar el result set. No se imprime ninguna fila;
   aqui no hay datos personales, pero el repositorio es publico y la regla de la
   fase es no volcar filas nunca. */
CREATE TABLE #HeParametros
(
    IdParametro        INT,
    Clave              VARCHAR(60),
    Valor              DECIMAL(18,6),
    FechaVigenciaDesde DATE,
    FechaVigenciaHasta DATE,
    Usu_Modificacion   VARCHAR(50),
    Fec_Modificacion   DATETIME2(0)
);

IF OBJECT_ID('dbo.Sp_RTA_HeParametrosListar','P') IS NOT NULL
BEGIN
    INSERT INTO #HeParametros
        (IdParametro, Clave, Valor, FechaVigenciaDesde, FechaVigenciaHasta,
         Usu_Modificacion, Fec_Modificacion)
    EXEC dbo.Sp_RTA_HeParametrosListar;

    /* Se cuentan claves DISTINTAS y no filas: en cuanto alguien use la pantalla
       habra mas de una fila por clave, y una asercion sobre el numero de filas
       empezaria a fallar sin que nada este roto. */
    IF (SELECT COUNT(DISTINCT Clave) FROM #HeParametros
         WHERE Clave IN ('DecimalesMonto','DiasMes','Factor50','Factor100',
                         'HorasMesJornadaCompleta','TopeDiario50','TopeSemanal50')) <> 7
    BEGIN
        RAISERROR('FALLO: Sp_RTA_HeParametrosListar no devuelve las 7 claves conocidas.', 16, 1);
        SET @Fallos += 1;
    END

    /* Ninguna fila puede salir sin IdParametro: es la clave con la que la
       pantalla dice que version esta editando. */
    IF EXISTS (SELECT 1 FROM #HeParametros WHERE IdParametro IS NULL OR IdParametro = 0)
    BEGIN
        RAISERROR('FALLO: Sp_RTA_HeParametrosListar devuelve filas sin IdParametro.', 16, 1);
        SET @Fallos += 1;
    END
END

DROP TABLE #HeParametros;

/* --- 4. los seis codigos de respuesta estan en el cuerpo del procedimiento --- */
/* Esto NO prueba el comportamiento, solo que ningun camino se quedo sin
   escribir. Probarlo de verdad exigiria guardar parametros reales en
   produccion, y una version de prueba de un parametro de nomina no se puede
   borrar sin dejar el historial mentiroso: quedaria una vigencia que nunca
   existio y los periodos que caigan en ella calcularian con ella.

   La prueba de comportamiento la hace el controlador a mano, llamando al
   procedimiento con una clave inventada, un valor cero, una fecha repetida, una
   fecha anterior y un DecimalesMonto de 4, y mirando que devuelva -1, -2, -3,
   -4 y -5 SIN escribir nada. Ese juego completo se puede correr sin ensuciar la
   tabla: los cinco casos son rechazos. */
DECLARE @Cuerpo NVARCHAR(MAX) = ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.Sp_RTA_HeParametroGuardar')), '');

IF CHARINDEX('Respuestas = -1', @Cuerpo) = 0
   OR CHARINDEX('Respuestas = -2', @Cuerpo) = 0
   OR CHARINDEX('Respuestas = -3', @Cuerpo) = 0
   OR CHARINDEX('Respuestas = -4', @Cuerpo) = 0
   OR CHARINDEX('Respuestas = -5', @Cuerpo) = 0
   OR CHARINDEX('Respuestas = 0',  @Cuerpo) = 0
BEGIN
    RAISERROR('FALLO: a Sp_RTA_HeParametroGuardar le falta alguno de los codigos 0, -1, -2, -3, -4 o -5.', 16, 1);
    SET @Fallos += 1;
END

/* Que cierre la vigente en vez de actualizarla es LO que distingue esta fase de
   un UPDATE a mano. Si el cuerpo no menciona el cierre, el historial no existe
   y la fase no sirve para nada. */
IF CHARINDEX('FechaVigenciaHasta = DATEADD(DAY, -1, @FechaVigenciaDesde)', @Cuerpo) = 0
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeParametroGuardar no cierra la version vigente.', 16, 1);
    SET @Fallos += 1;
END

/* --- 5. la fila del menu --- */
DECLARE @IdHojaV  INT;
DECLARE @IdPadreV INT;
DECLARE @Cuantas  INT;

SELECT @Cuantas = COUNT(*) FROM dbo.MenuDos WHERE Href = 'ParametrizacionHorasExtras.aspx';

IF @Cuantas <> 1
BEGIN
    RAISERROR('FALLO: ParametrizacionHorasExtras.aspx aparece %d veces en MenuDos; se esperaba 1.', 16, 1, @Cuantas);
    SET @Fallos += 1;
END
ELSE
BEGIN
    SELECT @IdHojaV = Id_Menu, @IdPadreV = Id_MenuPadre
      FROM dbo.MenuDos WHERE Href = 'ParametrizacionHorasExtras.aspx';

    /* Cuelga del mismo grupo que su hermana. Este es el requisito de verdad. */
    IF @IdPadreV IS NULL
       OR @IdPadreV <> (SELECT Id_MenuPadre FROM dbo.MenuDos WHERE Href = 'HorasExtras.aspx')
    BEGIN
        RAISERROR('FALLO: la hoja nueva no cuelga del mismo grupo que HorasExtras.aspx.', 16, 1);
        SET @Fallos += 1;
    END

    /* Y ese grupo es el 20081, que es el valor medido el 2026-09-16. Si esta
       falla y la de arriba no, no esta roto el script: cambio el mundo. */
    IF @IdPadreV <> 20081
    BEGIN
        RAISERROR('FALLO: la hoja nueva cuelga del mismo grupo que su hermana, pero ese grupo ya no es el 20081 que se midio el 2026-09-16. El script no esta roto; hay que revisar que cambio en el menu antes de desplegar.', 16, 1);
        SET @Fallos += 1;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.MenuDos
                    WHERE Id_Menu = @IdHojaV AND Es_Opcion_de_Menu = 0)
    BEGIN
        RAISERROR('FALLO: la hoja nueva no quedo marcada como pagina (Es_Opcion_de_Menu = 0).', 16, 1);
        SET @Fallos += 1;
    END

    /* --- 6. quien la ve --- */
    /* Exactamente dos filas, las dos encendidas, y las de los perfiles 14 y 18.
       Las tres condiciones en una sola cuenta: si sobrara un perfil, si
       cualquiera de los dos estuviera apagado o si faltara uno, el numero deja
       de ser 2. Recordar que Estado = '0' MUESTRA. */
    SELECT @Cuantas = COUNT(*) FROM dbo.PerfilMenu WHERE id_Menu = @IdHojaV;

    IF @Cuantas <> 2
    BEGIN
        RAISERROR('FALLO: PerfilMenu tiene %d filas para la hoja nueva; se esperaban 2.', 16, 1, @Cuantas);
        SET @Fallos += 1;
    END

    SELECT @Cuantas = COUNT(*)
      FROM dbo.PerfilMenu
     WHERE id_Menu = @IdHojaV
       AND IdPerfil IN (14, 18)
       AND Estado = '0';

    IF @Cuantas <> 2
    BEGIN
        RAISERROR('FALLO: los perfiles 14 y 18 no tienen las dos filas visibles (Estado = 0) de la hoja nueva.', 16, 1);
        SET @Fallos += 1;
    END
END

/* Lo que NO se comprueba aqui, y donde se comprueba:

   - Que la hoja se VEA en el menu. Depende de que el grupo padre 20081 tambien
     tenga Estado = '0' para los perfiles 14 y 18, cosa que hizo el script de la
     fase 2 y que este no toca. Se mira entrando a la aplicacion con un usuario
     de cada perfil.
   - Que los codigos -1 a -5 se devuelvan de verdad. Ver el comentario del punto
     4: son cinco llamadas de rechazo que el controlador hace a mano.
   - Que un guardado correcto cierre la vigente e inserte la nueva. Esa si
     escribe, y escribe en la tabla que decide cuanto se paga. Hacerla en
     produccion significa dejar una version nueva puesta; conviene hacerla con
     una clave cuyo valor no cambie -repetir el valor actual con fecha de
     manana- y mirar que queden dos filas, la vieja cerrada ayer. */

IF @Fallos > 0
    RAISERROR('FALLO: %d verificaciones no pasaron.', 16, 1, @Fallos);
ELSE
    PRINT 'Horas Extras fase 5: los parametros se guardan con historial y la pantalla esta en el menu.';
GO

/* Deshace el guard de la cabecera: sin esto, la sesion de quien corriera el
   script se quedaria en NOEXEC y todo lo que ejecutara despues -en la misma
   ventana- se analizaria sin ejecutarse, en silencio. */
SET NOEXEC OFF;
GO
