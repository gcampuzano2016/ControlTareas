/* ============================================================================
   Horas Extras: la pantalla entra al menu
   ReporTarea  |  2026-09-16

   YA APLICADO EN PRODUCCION (verificado el 2026-09-21).

   Se corrio en produccion en este orden:
     1) 2026-09-16-horas-extras-cargo.sql               (crea la columna Cargo)
     2) carga-generada/carga-horas-extras.sql (regenerada) (la llena para los 64)
     3) 2026-09-16-horas-extras-fase2.sql                (los cinco procedimientos)
     4) ESTE SCRIPT                                       (la opcion de menu)
     5) los binarios (los scripts SQL van siempre antes)

   ----------------------------------------------------------------------------
   Que hace y por que

   HorasExtras.aspx muestra el sueldo de 64 personas reales. Solo la deben ver
   los perfiles 14 (Talento Humano) y 18 (Super Admin), y ningun otro. Ninguno
   de los grupos que hoy ve el perfil 14 (20042 Manejo de Perfiles, 20057
   Departamento Medico, 20060 Departamento Medico GTH, 5 Vacaciones & Permisos)
   es un hogar natural para una pantalla de nomina, asi que este script crea un
   grupo nuevo "Nomina" y cuelga la hoja de el.

   IMPORTANTE: en dbo.PerfilMenu la semantica de Estado esta INVERTIDA:
   '0' MUESTRA la opcion y '1' la OCULTA (Sp_RTA_ConsultarMenuPerfilUsuario
   filtra WHERE P.Estado = 0). Ademas, una hoja solo se ve si su grupo padre
   TAMBIEN tiene Estado='0' para ese perfil; por eso se da permiso en las dos
   filas (grupo y hoja) para los dos perfiles.

   dbo.MenuDos tiene 18 columnas, varias NOT NULL con default y de casing
   inconsistente. En vez de inventarlas, las dos filas nuevas se clonan de
   filas modelo que ya funcionan en produccion, copiando por metadata TODAS
   las columnas no-identidad (asi arrastra cualquier columna que no
   conozcamos):

       Grupo "Nomina"          clonado de Id_Menu = 20060 (Departamento Medico GTH)
       Hoja  HorasExtras.aspx  clonado de Id_Menu = 20061 (RRHHEmpleados.aspx)

   El clon depende de que Id_Menu sea IDENTITY (se verifica antes de tocar
   nada). Titulo, Href, Id_MenuPadre, Descripcion y Orden_Opcion se corrigen
   despues del clon; el resto de columnas queda como en la fila modelo.

   Idempotente: se puede ejecutar varias veces sin duplicar el grupo, la hoja
   ni los permisos.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '== Horas Extras: registro en el menu - inicio ==';
GO

BEGIN TRANSACTION;

IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.MenuDos')
      AND name = 'Id_Menu'
      AND is_identity = 1
)
BEGIN
    ROLLBACK;
    RAISERROR('dbo.MenuDos.Id_Menu no es IDENTITY: el clon automatico no es seguro. Revisar el script a mano.', 16, 1);
    RETURN;
END

DECLARE @ModeloGrupo INT = 20060;   -- Departamento Medico GTH
DECLARE @ModeloHoja  INT = 20061;   -- RRHHEmpleados.aspx

DECLARE @TituloGrupo      VARCHAR(128)  = 'Nomina';
DECLARE @DescripcionGrupo VARCHAR(512)  = 'Modulo de nomina';
DECLARE @TituloHoja       VARCHAR(128)  = 'Horas Extras';
DECLARE @DescripcionHoja  VARCHAR(512)  = 'Registro de horas extras del personal';
DECLARE @HrefHoja         VARCHAR(512)  = 'HorasExtras.aspx';

DECLARE @IdGrupo INT;
DECLARE @IdHoja  INT;
DECLARE @Orden   INT;

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
   1. Grupo "Nomina"  (cabecera: Es_Opcion_de_Menu = 1, Id_MenuPadre = 0)
   ============================================================================ */
SELECT @IdGrupo = Id_Menu
FROM dbo.MenuDos
WHERE Titulo = @TituloGrupo
  AND Es_Opcion_de_Menu = 1
  AND Id_MenuPadre = 0;

IF @IdGrupo IS NULL
BEGIN
    DECLARE @sqlGrupo NVARCHAR(MAX) =
        N'INSERT INTO dbo.MenuDos (' + @cols + N') ' +
        N'SELECT ' + @cols + N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo; ' +
        N'SET @IdOut = CONVERT(INT, SCOPE_IDENTITY());';

    EXEC sys.sp_executesql @sqlGrupo,
         N'@Modelo INT, @IdOut INT OUTPUT',
         @Modelo = @ModeloGrupo,
         @IdOut  = @IdGrupo OUTPUT;

    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1
    FROM dbo.MenuDos
    WHERE Id_MenuPadre = 0;

    UPDATE dbo.MenuDos
    SET Titulo       = @TituloGrupo,
        Descripcion  = @DescripcionGrupo,
        Orden_Opcion = @Orden
    WHERE Id_Menu = @IdGrupo;

    PRINT 'MenuDos: grupo Nomina creado con Id_Menu = ' + CONVERT(VARCHAR(20), @IdGrupo) + '.';
END
ELSE
    PRINT 'MenuDos: el grupo Nomina ya existia (Id_Menu = ' + CONVERT(VARCHAR(20), @IdGrupo) + '). Sin duplicar.';

/* ============================================================================
   2. Hoja HorasExtras.aspx, colgada del grupo Nomina
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

    PRINT 'MenuDos: hoja HorasExtras.aspx creada con Id_Menu = ' + CONVERT(VARCHAR(20), @IdHoja) + ', bajo el grupo ' + CONVERT(VARCHAR(20), @IdGrupo) + '.';
END
ELSE
BEGIN
    /* Si ya existia colgada de otro padre (por ejemplo una corrida manual
       anterior), la re-enganchamos al grupo Nomina para no dejarla suelta. */
    UPDATE dbo.MenuDos
    SET Id_MenuPadre = @IdGrupo
    WHERE Id_Menu = @IdHoja
      AND Id_MenuPadre <> @IdGrupo;

    PRINT 'MenuDos: la hoja HorasExtras.aspx ya existia (Id_Menu = ' + CONVERT(VARCHAR(20), @IdHoja) + '). Sin duplicar.';
END

/* ============================================================================
   3. Visibilidad para los perfiles 14 (Talento Humano) y 18 (Super Admin),
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

/* ----------------------------------------------------------------------------
   Verificacion: como quedo el grupo Nomina y su hoja, por perfil.
   ---------------------------------------------------------------------------- */
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
WHERE M.Titulo = 'Nomina'
   OR M.Href = 'HorasExtras.aspx'
ORDER BY M.Id_MenuPadre, M.Id_Menu, PM.IdPerfil;
GO

PRINT '== Horas Extras: registro en el menu - fin ==';
GO
