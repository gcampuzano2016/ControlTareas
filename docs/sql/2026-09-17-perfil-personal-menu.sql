/* ============================================================================
   Perfil del personal: la pantalla entra al menu
   ReporTarea  |  2026-09-17

   PENDIENTE DE EJECUTAR. Independiente de docs/sql/2026-09-17-perfil-personal-
   lista.sql (el procedimiento del listado): los dos scripts no comparten nada
   y se pueden correr en cualquier orden entre si. Este es el unico script de
   esta entrega que toca el menu.

   ----------------------------------------------------------------------------
   Que hace y por que

   PerfilesPersonal.aspx le permite a Talento Humano consultar y corregir el
   perfil de cualquier colaborador. Solo la deben ver los perfiles 14 (Talento
   Humano) y 18 (Super Admin).

   A diferencia del script equivalente de Horas Extras
   (docs/sql/2026-09-16-horas-extras-fase2-menu.sql), este NO crea un grupo de
   menu nuevo: la pantalla cuelga del mismo grupo donde ya vive
   RRHHEmpleados.aspx, que es la pantalla con la que Talento Humano ya
   administra datos de empleados y es su vecina natural.

   Nadie de los que trabajamos este script puede consultar la base para saber
   el Id_Menu de ese grupo, asi que el script lo resuelve solo: busca la fila
   de RRHHEmpleados.aspx por Href y toma su Id_MenuPadre. La MISMA fila sirve
   ademas de modelo para clonar la hoja nueva: dbo.MenuDos tiene 18 columnas,
   varias NOT NULL con default y de casing inconsistente, y clonar por
   metadata evita inventarlas (asi arrastra tambien cualquier columna que hoy
   no conocemos). Href, Titulo, Descripcion, Id_MenuPadre y Orden_Opcion se
   corrigen despues del clon; el resto de columnas queda como en la fila
   modelo.

   Esa busqueda por Href LIKE puede dar cero, una o mas de una fila, y las
   tres se tratan distinto: cero o mas de una detienen el script -nunca se
   elige una fila a ciegas con un TOP 1 sin ORDER BY; un grupo colgado de una
   eleccion arbitraria seria un defecto silencioso, y ese principio ya esta
   escrito en este modulo (Sp_RTA_PerfilColaborador se niega a devolver nada
   cuando un Cod_Usuario esta repetido, en vez de elegir una de las dos
   personas). Solo el caso de exactamente una fila sigue adelante.

   IMPORTANTE: en dbo.PerfilMenu la semantica de Estado esta INVERTIDA: '0'
   MUESTRA la opcion y '1' la OCULTA (Sp_RTA_ConsultarMenuPerfilUsuario filtra
   WHERE P.Estado = 0). Ademas, una hoja solo se ve si su grupo padre TAMBIEN
   tiene Estado='0' para ese perfil; por eso se da permiso en las dos filas
   (grupo y hoja) para los dos perfiles.

   Guarda al principio, en su propio lote, con SET NOEXEC ON / OFF y no con
   RETURN: un RETURN fuera de un procedimiento sale del lote, no del script, y
   el resto seguiria ejecutandose despues del GO -es el defecto que costo dos
   rondas de arreglo en la entrega 1-. La guarda comprueba tres cosas, cada
   una con su propio RAISERROR: que exista exactamente una fila de
   RRHHEmpleados.aspx con Id_MenuPadre distinto de cero (ni cero ni varias),
   y que dbo.MenuDos.Id_Menu siga siendo IDENTITY (de eso depende que el clon
   con SCOPE_IDENTITY() sea seguro). Si alguna falla, el script se detiene
   entero y no crea nada. El COUNT(*) de la guarda y el SELECT TOP 1 del lote
   principal comparten literalmente el mismo WHERE, para que la guarda
   proteja exactamente lo mismo que el lote principal termina usando.

   Idempotente: se puede ejecutar varias veces sin duplicar la hoja ni las
   filas de PerfilMenu.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------------------------ 0. guarda --- */

/* Tres condiciones independientes, cada una con su propio RAISERROR y su
   propio SET NOEXEC ON. Ninguna usa una funcion como argumento de
   sustitucion del RAISERROR -solo literales-: RAISERROR admite unicamente
   literales o variables locales ahi, una llamada a funcion no compila y se
   lleva puesto el lote entero, guarda incluida. Por eso no se usa ISNULL()
   ni nada parecido dentro del texto de ningun mensaje.

   Se cuenta primero y se decide despues, en vez de un TOP 1 sin ORDER BY
   que elegiria una fila a ciegas si hubiera mas de una: determinista y
   equivocado es peor que detenido, porque nadie lo descubre. Este COUNT(*)
   y el SELECT TOP 1 del lote principal (mas abajo, despues del GO) usan
   EXACTAMENTE el mismo WHERE a proposito: si contaran sobre un conjunto de
   filas y despues eligieran sobre otro, esta guarda protegeria algo
   distinto de lo que el lote principal termina usando.

   Va en su propio lote, ANTES de cualquier PRINT de inicio, para que una
   corrida detenida no imprima "inicio" como si hubiera sido normal. */
DECLARE @NGrupo INT;

SELECT @NGrupo = COUNT(*)
  FROM dbo.MenuDos
 WHERE Href LIKE '%RRHHEmpleados.aspx%'
   AND ISNULL(Id_MenuPadre, 0) <> 0;

IF @NGrupo = 0
BEGIN
    RAISERROR('No se encontro ninguna fila de RRHHEmpleados.aspx con Id_MenuPadre distinto de cero. Revisar a mano de que grupo debe colgar PerfilesPersonal.aspx. Script detenido.', 16, 1);
    SET NOEXEC ON;
END

IF @NGrupo > 1
BEGIN
    RAISERROR('Se encontraron varias filas de RRHHEmpleados.aspx con Id_MenuPadre distinto de cero. No se elige ninguna a ciegas: revisar a mano de cual grupo debe colgar PerfilesPersonal.aspx. Script detenido.', 16, 1);
    SET NOEXEC ON;
END

IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.MenuDos')
      AND name = 'Id_Menu'
      AND is_identity = 1
)
BEGIN
    RAISERROR('dbo.MenuDos.Id_Menu no es IDENTITY: el clon automatico no es seguro. Revisar el script a mano. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

PRINT '== Perfil del personal: registro en el menu - inicio ==';
GO

BEGIN TRANSACTION;

/* Se recalculan aca @IdGrupo y @ModeloHoja: las variables de la guarda de
   arriba no cruzan el GO -cada lote tiene las suyas propias-. Si la guarda
   no encontro exactamente una fila, ya encendio NOEXEC ON y este lote ni se
   ejecuta; si se ejecuta es porque la guarda ya confirmo que hay una sola
   fila que cumple este WHERE, asi que el TOP 1 de aca no elige entre varias
   -ya no queda mas que una para elegir-. El WHERE es literalmente el mismo
   que el del COUNT(*) de la guarda: mismo conjunto de filas en los dos
   lugares. */
DECLARE @IdGrupo    INT;
DECLARE @ModeloHoja INT;

SELECT TOP 1 @IdGrupo = Id_MenuPadre, @ModeloHoja = Id_Menu
  FROM dbo.MenuDos
 WHERE Href LIKE '%RRHHEmpleados.aspx%'
   AND ISNULL(Id_MenuPadre, 0) <> 0;

DECLARE @TituloHoja      VARCHAR(128) = 'Perfiles del personal';
DECLARE @DescripcionHoja VARCHAR(512) = 'Consulta y correccion del perfil de cualquier colaborador';
DECLARE @HrefHoja        VARCHAR(512) = 'PerfilesPersonal.aspx';

DECLARE @IdHoja INT;
DECLARE @Orden  INT;

/* Columnas no-identidad de MenuDos, calculadas por metadata: si la tabla
   tiene columnas que hoy no conocemos, el clon las arrastra igual. */
DECLARE @cols NVARCHAR(MAX) = STUFF
((
    SELECT ',' + QUOTENAME(c.name)
    FROM sys.columns AS c
    WHERE c.object_id = OBJECT_ID('dbo.MenuDos')
      AND c.is_identity = 0
    ORDER BY c.column_id
    FOR XML PATH(''), TYPE
).value('.', 'NVARCHAR(MAX)'), 1, 1, '');

/* ============================================================================
   1. Hoja PerfilesPersonal.aspx, colgada del grupo de RRHHEmpleados.aspx
   ============================================================================ */
SELECT @IdHoja = Id_Menu
FROM dbo.MenuDos
WHERE Href = @HrefHoja;

IF @IdHoja IS NULL
BEGIN
    DECLARE @sqlHoja NVARCHAR(MAX) =
        N'INSERT INTO dbo.MenuDos (' + @cols + N') ' +
        N'SELECT ' + @cols + N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo; ' +
        N'SET @IdOut = CONVERT(INT, SCOPE_IDENTITY());';

    EXEC sys.sp_executesql @sqlHoja,
         N'@Modelo INT, @IdOut INT OUTPUT',
         @Modelo = @ModeloHoja,
         @IdOut  = @IdHoja OUTPUT;

    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1
    FROM dbo.MenuDos
    WHERE Id_MenuPadre = @IdGrupo;

    UPDATE dbo.MenuDos
    SET Href         = @HrefHoja,
        Titulo       = @TituloHoja,
        Descripcion  = @DescripcionHoja,
        Id_MenuPadre = @IdGrupo,
        Orden_Opcion = @Orden
    WHERE Id_Menu = @IdHoja;

    PRINT 'MenuDos: hoja PerfilesPersonal.aspx creada con Id_Menu = ' + CONVERT(VARCHAR(20), @IdHoja) + ', bajo el grupo ' + CONVERT(VARCHAR(20), @IdGrupo) + '.';
END
ELSE
BEGIN
    /* Si ya existia colgada de otro padre (por ejemplo una corrida manual
       anterior), la re-enganchamos al grupo de RRHHEmpleados.aspx para no
       dejarla suelta. */
    UPDATE dbo.MenuDos
    SET Id_MenuPadre = @IdGrupo
    WHERE Id_Menu = @IdHoja
      AND Id_MenuPadre <> @IdGrupo;

    PRINT 'MenuDos: la hoja PerfilesPersonal.aspx ya existia (Id_Menu = ' + CONVERT(VARCHAR(20), @IdHoja) + '). Sin duplicar.';
END

/* ============================================================================
   2. Visibilidad para los perfiles 14 (Talento Humano) y 18 (Super Admin),
      tanto en el grupo como en la hoja. Estado = '0' = visible.
   ============================================================================ */
DECLARE @Perfiles TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Perfiles (IdPerfil) VALUES (14), (18);

DECLARE @Menus TABLE (id_Menu INT PRIMARY KEY);
INSERT INTO @Menus (id_Menu) VALUES (@IdGrupo), (@IdHoja);

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT M.id_Menu, P.IdPerfil, CONVERT(DATE, GETDATE()), '0'
FROM @Menus AS M
CROSS JOIN @Perfiles AS P
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.PerfilMenu AS PM
    WHERE PM.id_Menu = M.id_Menu
      AND PM.IdPerfil = P.IdPerfil
);

/* Si alguna de esas filas ya existia pero apagada (Estado='1'), la
   encendemos: el objetivo es que los perfiles 14 y 18 vean la pantalla. */
UPDATE PM
SET PM.Estado = '0'
FROM dbo.PerfilMenu AS PM
JOIN @Menus AS M ON M.id_Menu = PM.id_Menu
JOIN @Perfiles AS P ON P.IdPerfil = PM.IdPerfil
WHERE PM.Estado <> '0';

PRINT 'PerfilMenu: visibilidad asegurada para los perfiles 14 y 18, en el grupo y en la hoja (Estado = 0).';

COMMIT TRANSACTION;
GO

/* Incondicional: si la guarda de arriba encendio NOEXEC, apagarlo aca es lo
   que evita que el resto de la sesion de SSMS quede sin ejecutar nada y
   parezca que los scripts siguientes "no hacen nada". */
SET NOEXEC OFF;
GO

PRINT '== Perfil del personal: registro en el menu - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   Esperado: la hoja PerfilesPersonal.aspx colgada del mismo Id_MenuPadre que
   RRHHEmpleados.aspx, y 4 filas de PerfilMenu con Estado = 0 (perfiles 14 y
   18, una para el grupo y otra para la hoja, cada uno).

SELECT
    M.Id_Menu,
    M.Id_MenuPadre,
    M.Titulo,
    M.Href,
    M.Es_Opcion_de_Menu,
    M.Orden_Opcion,
    PM.IdPerfil,
    PM.Estado
FROM dbo.MenuDos AS M
LEFT JOIN dbo.PerfilMenu AS PM ON PM.id_Menu = M.Id_Menu
WHERE M.Href = 'PerfilesPersonal.aspx'
   OR M.Id_Menu = (SELECT Id_MenuPadre FROM dbo.MenuDos
                     WHERE Href LIKE '%RRHHEmpleados.aspx%'
                       AND ISNULL(Id_MenuPadre, 0) <> 0)
ORDER BY M.Id_MenuPadre, M.Id_Menu, PM.IdPerfil;

   ============================================================================ */
