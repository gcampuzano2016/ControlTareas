/* ============================================================================
   Bonos de empleados - menu
   ReporTarea  |  2026-10-22
   Se corre AL DESPLEGAR, junto con los binarios: antes, las opciones de menu
   llevarian a paginas que todavia no existen en el servidor.
   Crea o actualiza:
     1. Grupo de menu "Bonos" (clon del grupo 5, Vacaciones & Permisos)
     2. Hojas, en este orden:
          BonosProponer.aspx   "Proponer bonos"
          BonosRevisar.aspx    "Revisar y aprobar"
          BonosCatalogo.aspx   "Catalogo de bonos" (con tilde, via NCHAR)
          BonosReporte.aspx    "Reporte mensual"
     3. Visibilidad (PerfilMenu, Estado '0' = visible):
          grupo y BonosProponer.aspx -> todo perfil con usuarios internos activos, y el 18
          BonosRevisar, BonosCatalogo y BonosReporte -> 14 y 18
   Repetible. Reversa al final, comentada.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID('tempdb..#ClonarMenu') IS NOT NULL DROP PROCEDURE #ClonarMenu;
GO
/* MenuDos tiene 18 columnas, varias NOT NULL con default: se clona una fila
   modelo en vez de enumerarlas. Mismo bloque que 2026-10-15-agenda-menu.sql. */
CREATE PROCEDURE #ClonarMenu
    @IdModelo INT,
    @IdNuevo  INT OUTPUT
AS
BEGIN
    DECLARE @cols NVARCHAR(MAX) = STUFF
    ((
        SELECT ',' + QUOTENAME(c.name)
          FROM sys.columns AS c
         WHERE c.object_id = OBJECT_ID('dbo.MenuDos')
           AND c.is_identity = 0 AND c.is_computed = 0 AND c.name <> 'Id_Menu'
         ORDER BY c.column_id
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 1, '');
    DECLARE @sql NVARCHAR(MAX);

    IF EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.MenuDos') AND name = 'Id_Menu' AND is_identity = 1)
    BEGIN
        SET @sql = N'INSERT INTO dbo.MenuDos (' + @cols + N') SELECT ' + @cols +
                   N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo; SET @IdOut = CONVERT(INT, SCOPE_IDENTITY());';
        EXEC sys.sp_executesql @sql, N'@Modelo INT, @IdOut INT OUTPUT', @Modelo = @IdModelo, @IdOut = @IdNuevo OUTPUT;
    END
    ELSE
    BEGIN
        SELECT @IdNuevo = ISNULL(MAX(Id_Menu), 0) + 1 FROM dbo.MenuDos WITH (TABLOCKX, HOLDLOCK);
        SET @sql = N'INSERT INTO dbo.MenuDos ([Id_Menu],' + @cols + N') SELECT @Id, ' + @cols +
                   N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo;';
        EXEC sys.sp_executesql @sql, N'@Modelo INT, @Id INT', @Modelo = @IdModelo, @Id = @IdNuevo;
    END
END
GO

BEGIN TRANSACTION;

DECLARE @TituloGrupo    VARCHAR(128) = 'Bonos';
DECLARE @TituloCatalogo VARCHAR(128) = CONVERT(VARCHAR(128), N'Cat' + NCHAR(225) + N'logo de bonos');
DECLARE @IdModeloGrupo  INT = 5;
DECLARE @IdModeloHoja   INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'TableroCostos.aspx');
DECLARE @IdGrupo INT, @Orden INT;

IF NOT EXISTS (SELECT 1 FROM dbo.MenuDos WHERE Id_Menu = @IdModeloGrupo AND Es_Opcion_de_Menu = 1)
   OR @IdModeloHoja IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('No estan las filas modelo (grupo 5 o TableroCostos.aspx). Revisar el menu antes de seguir.', 16, 1);
    RETURN;
END

/* 1. grupo */
SELECT @IdGrupo = MIN(Id_Menu) FROM dbo.MenuDos WHERE Es_Opcion_de_Menu = 1 AND Titulo = @TituloGrupo;
IF @IdGrupo IS NULL
BEGIN
    EXEC #ClonarMenu @IdModeloGrupo, @IdGrupo OUTPUT;
    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1 FROM dbo.MenuDos WHERE Es_Opcion_de_Menu = 1 AND Id_Menu <> @IdGrupo;
    UPDATE dbo.MenuDos
       SET Titulo = @TituloGrupo, Descripcion = 'Bonos mensuales de empleados', Orden_Opcion = @Orden
     WHERE Id_Menu = @IdGrupo;
    PRINT 'Grupo creado: ' + CONVERT(VARCHAR(10), @IdGrupo);
END

/* 2. hojas */
DECLARE @Hojas TABLE (Orden INT PRIMARY KEY, Href VARCHAR(128), Titulo VARCHAR(128), Descripcion VARCHAR(256), IdMenu INT NULL);
INSERT INTO @Hojas (Orden, Href, Titulo, Descripcion) VALUES
    (1, 'BonosProponer.aspx', 'Proponer bonos',    'Proponer los bonos del mes para su gente'),
    (2, 'BonosRevisar.aspx',  'Revisar y aprobar', 'Revisar, aprobar y cerrar los bonos del mes'),
    (3, 'BonosCatalogo.aspx', @TituloCatalogo,     'Tipos de bono y conceptos SAP'),
    (4, 'BonosReporte.aspx',  'Reporte mensual',   'Totales por mes y Excel para Nomina');

DECLARE @Ord INT, @Href VARCHAR(128), @Tit VARCHAR(128), @Des VARCHAR(256), @IdHoja INT;
DECLARE hojas CURSOR LOCAL STATIC FORWARD_ONLY FOR SELECT Orden, Href, Titulo, Descripcion FROM @Hojas ORDER BY Orden;
OPEN hojas;
FETCH NEXT FROM hojas INTO @Ord, @Href, @Tit, @Des;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @IdHoja = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = @Href);
    IF @IdHoja IS NULL
    BEGIN
        EXEC #ClonarMenu @IdModeloHoja, @IdHoja OUTPUT;
        UPDATE dbo.MenuDos
           SET Href = @Href, Titulo = @Tit, Descripcion = @Des,
               Id_MenuPadre = @IdGrupo, Es_Opcion_de_Menu = 0, Orden_Opcion = @Ord
         WHERE Id_Menu = @IdHoja;
        PRINT 'Hoja creada: ' + @Href + ' = ' + CONVERT(VARCHAR(10), @IdHoja);
    END
    UPDATE @Hojas SET IdMenu = @IdHoja WHERE Orden = @Ord;
    FETCH NEXT FROM hojas INTO @Ord, @Href, @Tit, @Des;
END
CLOSE hojas;
DEALLOCATE hojas;

/* 3. visibilidad. Una hoja solo se ve si su grupo tambien es visible para el perfil. */
DECLARE @Todos TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Todos (IdPerfil)
SELECT DISTINCT u.Id_Perfil
  FROM dbo.R_Usuarios AS u
 WHERE u.Usuario_Estado = 'A' AND u.EstadoUsuario IS NULL AND ISNULL(u.IdCliente, 0) = 0
   AND u.Id_Perfil IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM @Todos WHERE IdPerfil = 18) INSERT INTO @Todos (IdPerfil) VALUES (18);

DECLARE @Vis TABLE (IdMenu INT, IdPerfil INT, PRIMARY KEY (IdMenu, IdPerfil));
INSERT INTO @Vis (IdMenu, IdPerfil) SELECT @IdGrupo, IdPerfil FROM @Todos;
INSERT INTO @Vis (IdMenu, IdPerfil) SELECT h.IdMenu, t.IdPerfil FROM @Hojas AS h CROSS JOIN @Todos AS t WHERE h.Href = 'BonosProponer.aspx';
INSERT INTO @Vis (IdMenu, IdPerfil)
SELECT h.IdMenu, p.IdPerfil FROM @Hojas AS h CROSS JOIN (VALUES (14), (18)) AS p (IdPerfil)
 WHERE h.Href IN ('BonosRevisar.aspx', 'BonosCatalogo.aspx', 'BonosReporte.aspx');

UPDATE pm SET Estado = '0'
  FROM dbo.PerfilMenu AS pm
  JOIN @Vis AS v ON v.IdMenu = pm.id_Menu AND v.IdPerfil = pm.IdPerfil
 WHERE pm.Estado <> '0';

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT v.IdMenu, v.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM @Vis AS v
 WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu AS pm WHERE pm.id_Menu = v.IdMenu AND pm.IdPerfil = v.IdPerfil);

COMMIT TRANSACTION;
PRINT 'Menu listo. Correr 2026-10-22-bonos-verificacion.sql.';
GO

/* ------------------------------------------------------------ REVERSA ------
BEGIN TRANSACTION;
DECLARE @M TABLE (Id INT);
INSERT INTO @M SELECT Id_Menu FROM dbo.MenuDos
 WHERE LTRIM(RTRIM(Href)) IN ('BonosProponer.aspx', 'BonosRevisar.aspx', 'BonosCatalogo.aspx', 'BonosReporte.aspx');
INSERT INTO @M SELECT Id_Menu FROM dbo.MenuDos WHERE Es_Opcion_de_Menu = 1 AND Titulo = 'Bonos';
DELETE FROM dbo.PerfilMenu WHERE id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.R_UsuarioMenu WHERE Id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.MenuDos WHERE Id_Menu IN (SELECT Id FROM @M);
COMMIT TRANSACTION;
---------------------------------------------------------------------------- */
