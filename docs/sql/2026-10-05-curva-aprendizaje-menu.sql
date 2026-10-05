/* ============================================================================
   Curva de Aprendizaje: las pantallas entran al menu
   ReporTarea  |  2026-10-05

   Tres hojas nuevas, colgadas del mismo grupo que RRHHEmpleados.aspx y
   PerfilesPersonal.aspx (hoy el 20060), visibles para los perfiles 14
   (Talento Humano) y 18 (Super Admin):

       CurvaColaboradores.aspx     "Curva de aprendizaje"
       CurvaColaboradorNuevo.aspx  "Curva: nuevo colaborador"
       CurvaDashboard.aspx         "Curva: dashboard"

   La ficha, la evaluacion y la pagina remota NO van al menu: se llega a ellas
   desde la lista.

   Mismo metodo que 2026-09-17-perfil-personal-menu.sql: el grupo se resuelve
   por el Href de RRHHEmpleados.aspx (cero o varias filas detienen el script),
   la hoja se clona de esa fila por metadata y luego se corrigen Href, Titulo,
   Descripcion, Id_MenuPadre y Orden_Opcion.

   PerfilMenu.Estado esta INVERTIDO: '0' MUESTRA, '1' OCULTA. Y una hoja solo
   se ve si su grupo tambien esta en '0' para ese perfil.

   El MISMO juego de perfiles esta en AdministrarCurvaAprendizaje.ashx.cs
   (PerfilesAutorizados). Si se cambia uno, cambiar el otro: con solo el menu,
   la opcion redirige a Principal.aspx; con solo el arreglo, la pantalla
   queda alcanzable por URL pero no aparece.

   Idempotente.
   ============================================================================ */

USE [ReporTarea];
GO
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------------------------ 0. guarda --- */
DECLARE @NGrupo INT;
SELECT @NGrupo = COUNT(*)
  FROM dbo.MenuDos
 WHERE Href LIKE '%RRHHEmpleados.aspx%'
   AND ISNULL(Id_MenuPadre, 0) <> 0;

IF @NGrupo <> 1
BEGIN
    RAISERROR('No hay exactamente una fila de RRHHEmpleados.aspx con Id_MenuPadre distinto de cero. Revisar a mano de que grupo deben colgar las pantallas. Script detenido.', 16, 1);
    SET NOEXEC ON;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.MenuDos') AND name = 'Id_Menu' AND is_identity = 1)
BEGIN
    RAISERROR('dbo.MenuDos.Id_Menu no es IDENTITY: el clon automatico no es seguro. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

PRINT '== Curva de Aprendizaje: registro en el menu - inicio ==';
GO

BEGIN TRANSACTION;

DECLARE @IdGrupo INT, @ModeloHoja INT;
SELECT TOP 1 @IdGrupo = Id_MenuPadre, @ModeloHoja = Id_Menu
  FROM dbo.MenuDos
 WHERE Href LIKE '%RRHHEmpleados.aspx%'
   AND ISNULL(Id_MenuPadre, 0) <> 0;

DECLARE @cols NVARCHAR(MAX) = STUFF
((
    SELECT ',' + QUOTENAME(c.name)
    FROM sys.columns AS c
    WHERE c.object_id = OBJECT_ID('dbo.MenuDos') AND c.is_identity = 0
    ORDER BY c.column_id
    FOR XML PATH(''), TYPE
).value('.', 'NVARCHAR(MAX)'), 1, 1, '');

DECLARE @Hojas TABLE (n INT PRIMARY KEY, Href VARCHAR(512), Titulo VARCHAR(128), Descripcion VARCHAR(512), IdMenu INT NULL);
INSERT INTO @Hojas (n, Href, Titulo, Descripcion) VALUES
 (1, 'CurvaColaboradores.aspx',    'Curva de aprendizaje',     'Seguimiento de onboarding y curva de aprendizaje del primer anio'),
 (2, 'CurvaColaboradorNuevo.aspx', 'Curva: nuevo colaborador', 'Registrar un colaborador e iniciar su curva de aprendizaje'),
 (3, 'CurvaDashboard.aspx',        'Curva: dashboard',         'Vista agregada del programa de curva de aprendizaje');

DECLARE @i INT = 1, @Href VARCHAR(512), @Titulo VARCHAR(128), @Desc VARCHAR(512), @IdHoja INT, @Orden INT;
DECLARE @sqlHoja NVARCHAR(MAX) =
    N'INSERT INTO dbo.MenuDos (' + @cols + N') SELECT ' + @cols + N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo; ' +
    N'SET @IdOut = CONVERT(INT, SCOPE_IDENTITY());';

WHILE @i <= 3
BEGIN
    SELECT @Href = Href, @Titulo = Titulo, @Desc = Descripcion FROM @Hojas WHERE n = @i;
    SET @IdHoja = NULL;
    SELECT @IdHoja = Id_Menu FROM dbo.MenuDos WHERE Href = @Href;

    IF @IdHoja IS NULL
    BEGIN
        EXEC sys.sp_executesql @sqlHoja, N'@Modelo INT, @IdOut INT OUTPUT', @Modelo = @ModeloHoja, @IdOut = @IdHoja OUTPUT;
        SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1 FROM dbo.MenuDos WHERE Id_MenuPadre = @IdGrupo;
        UPDATE dbo.MenuDos
           SET Href = @Href, Titulo = @Titulo, Descripcion = @Desc, Id_MenuPadre = @IdGrupo, Orden_Opcion = @Orden
         WHERE Id_Menu = @IdHoja;
        PRINT 'MenuDos: hoja ' + @Href + ' creada con Id_Menu = ' + CONVERT(VARCHAR(20), @IdHoja) + '.';
    END
    ELSE
    BEGIN
        UPDATE dbo.MenuDos SET Id_MenuPadre = @IdGrupo WHERE Id_Menu = @IdHoja AND Id_MenuPadre <> @IdGrupo;
        PRINT 'MenuDos: la hoja ' + @Href + ' ya existia (Id_Menu = ' + CONVERT(VARCHAR(20), @IdHoja) + '). Sin duplicar.';
    END

    UPDATE @Hojas SET IdMenu = @IdHoja WHERE n = @i;
    SET @i += 1;
END

DECLARE @Perfiles TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Perfiles (IdPerfil) VALUES (14), (18);

DECLARE @Menus TABLE (id_Menu INT PRIMARY KEY);
INSERT INTO @Menus (id_Menu) SELECT IdMenu FROM @Hojas;
INSERT INTO @Menus (id_Menu) SELECT @IdGrupo WHERE NOT EXISTS (SELECT 1 FROM @Menus WHERE id_Menu = @IdGrupo);

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT M.id_Menu, P.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM @Menus AS M CROSS JOIN @Perfiles AS P
 WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu AS PM WHERE PM.id_Menu = M.id_Menu AND PM.IdPerfil = P.IdPerfil);

UPDATE PM SET PM.Estado = '0'
  FROM dbo.PerfilMenu AS PM
  JOIN @Menus AS M ON M.id_Menu = PM.id_Menu
  JOIN @Perfiles AS P ON P.IdPerfil = PM.IdPerfil
 WHERE ISNULL(PM.Estado, '') <> '0';

PRINT 'PerfilMenu: visibilidad asegurada para los perfiles 14 y 18 (grupo y tres hojas).';

COMMIT TRANSACTION;
GO

SET NOEXEC OFF;
GO

/* ------------------------------------------------------ verificacion --- */
SET NOCOUNT ON;
DECLARE @Fallos INT = 0;
DECLARE @Grupo INT = (SELECT Id_MenuPadre FROM dbo.MenuDos WHERE Href LIKE '%RRHHEmpleados.aspx%' AND ISNULL(Id_MenuPadre,0) <> 0);
IF (SELECT COUNT(*) FROM dbo.MenuDos WHERE Href IN ('CurvaColaboradores.aspx','CurvaColaboradorNuevo.aspx','CurvaDashboard.aspx') AND Id_MenuPadre = @Grupo) <> 3
BEGIN PRINT 'FALLO: no estan las tres hojas bajo el grupo de RRHHEmpleados.aspx.'; SET @Fallos += 1; END
IF (SELECT COUNT(*) FROM dbo.PerfilMenu PM JOIN dbo.MenuDos M ON M.Id_Menu = PM.id_Menu
     WHERE M.Href IN ('CurvaColaboradores.aspx','CurvaColaboradorNuevo.aspx','CurvaDashboard.aspx')
       AND PM.IdPerfil IN (14,18) AND PM.Estado = '0') <> 6
BEGIN PRINT 'FALLO: se esperaban 6 filas visibles en PerfilMenu (3 hojas x perfiles 14 y 18).'; SET @Fallos += 1; END
IF @Fallos = 0 PRINT 'OK: menu de Curva de Aprendizaje listo.'
ELSE PRINT 'VEREDICTO: FALLO (' + CAST(@Fallos AS VARCHAR(10)) + ' comprobaciones). Revisar arriba.';
GO
