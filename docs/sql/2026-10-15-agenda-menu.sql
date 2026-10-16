/* ============================================================================
   Equipo y agenda (Entrega 1) - menu y accion Agenda.VerTodos
   ReporTarea  |  2026-10-15
   Se corre AL DESPLEGAR, junto con los binarios: antes, la opcion de menu
   llevaria a una pagina que todavia no existe en el servidor.
   Crea o actualiza:
     1. Grupo de menu "Equipo y agenda" (clon del grupo 5, Vacaciones & Permisos)
     2. Hoja Agenda.aspx "Equipo y agenda"
     3. Hoja ParametrizacionAreasAgenda.aspx "Areas de la agenda" (con tilde, via NCHAR)
     4. Visibilidad (PerfilMenu, Estado '0' = visible):
        grupo y Agenda.aspx -> todo perfil con usuarios internos activos, y el 18
        ParametrizacionAreasAgenda.aspx -> 14 y 18
     5. Accion Agenda.VerTodos en RTA_Accion, ligada a Agenda.aspx, y dada al
        perfil 10 (Gerencia). El 18 la tiene siempre por codigo.
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
   modelo en vez de enumerarlas. Mismo bloque que 2026-10-12-tablero-costos.sql. */
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

DECLARE @TituloGrupo  VARCHAR(128) = 'Equipo y agenda';
DECLARE @TituloAreas  VARCHAR(128) = CONVERT(VARCHAR(128), NCHAR(193) + N'reas de la agenda');
DECLARE @IdModeloGrupo INT = 5;
DECLARE @IdModeloHoja  INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'TableroCostos.aspx');
DECLARE @IdGrupo INT, @IdAgenda INT, @IdAreas INT, @Orden INT;

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
       SET Titulo = @TituloGrupo, Descripcion = 'Agenda semanal del equipo', Orden_Opcion = @Orden
     WHERE Id_Menu = @IdGrupo;
    PRINT 'Grupo creado: ' + CONVERT(VARCHAR(10), @IdGrupo);
END

/* 2. hoja Agenda.aspx */
SELECT @IdAgenda = MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'Agenda.aspx';
IF @IdAgenda IS NULL
BEGIN
    EXEC #ClonarMenu @IdModeloHoja, @IdAgenda OUTPUT;
    UPDATE dbo.MenuDos
       SET Href = 'Agenda.aspx', Titulo = 'Equipo y agenda', Descripcion = 'Equipo por area y agenda semanal',
           Id_MenuPadre = @IdGrupo, Es_Opcion_de_Menu = 0, Orden_Opcion = 1
     WHERE Id_Menu = @IdAgenda;
    PRINT 'Hoja Agenda.aspx creada: ' + CONVERT(VARCHAR(10), @IdAgenda);
END

/* 3. hoja ParametrizacionAreasAgenda.aspx */
SELECT @IdAreas = MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'ParametrizacionAreasAgenda.aspx';
IF @IdAreas IS NULL
BEGIN
    EXEC #ClonarMenu @IdModeloHoja, @IdAreas OUTPUT;
    UPDATE dbo.MenuDos
       SET Href = 'ParametrizacionAreasAgenda.aspx', Titulo = @TituloAreas, Descripcion = 'Catalogo de areas de la agenda',
           Id_MenuPadre = @IdGrupo, Es_Opcion_de_Menu = 0, Orden_Opcion = 2
     WHERE Id_Menu = @IdAreas;
    PRINT 'Hoja ParametrizacionAreasAgenda.aspx creada: ' + CONVERT(VARCHAR(10), @IdAreas);
END

/* 4. visibilidad. Una hoja solo se ve si su grupo tambien es visible para el perfil. */
DECLARE @Todos TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Todos (IdPerfil)
SELECT DISTINCT u.Id_Perfil
  FROM dbo.R_Usuarios AS u
 WHERE u.Usuario_Estado = 'A' AND u.EstadoUsuario IS NULL AND ISNULL(u.IdCliente, 0) = 0
   AND u.Id_Perfil IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM @Todos WHERE IdPerfil = 18) INSERT INTO @Todos (IdPerfil) VALUES (18);

DECLARE @Vis TABLE (IdMenu INT, IdPerfil INT, PRIMARY KEY (IdMenu, IdPerfil));
INSERT INTO @Vis (IdMenu, IdPerfil) SELECT @IdGrupo, IdPerfil FROM @Todos;
INSERT INTO @Vis (IdMenu, IdPerfil) SELECT @IdAgenda, IdPerfil FROM @Todos;
INSERT INTO @Vis (IdMenu, IdPerfil) VALUES (@IdAreas, 14), (@IdAreas, 18);

UPDATE pm SET Estado = '0'
  FROM dbo.PerfilMenu AS pm
  JOIN @Vis AS v ON v.IdMenu = pm.id_Menu AND v.IdPerfil = pm.IdPerfil
 WHERE pm.Estado <> '0';

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT v.IdMenu, v.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM @Vis AS v
 WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu AS pm WHERE pm.id_Menu = v.IdMenu AND pm.IdPerfil = v.IdPerfil);

/* 5. accion */
IF NOT EXISTS (SELECT 1 FROM dbo.RTA_Accion WHERE Clave = 'Agenda.VerTodos')
BEGIN
    INSERT INTO dbo.RTA_Accion (Clave, Id_Menu, Titulo, Descripcion, Estado)
    VALUES ('Agenda.VerTodos', @IdAgenda, 'Ver la agenda de toda la empresa',
            'Ver a todas las personas activas y asignarles actividades, no solo a su cadena de jefatura.', 'A');
END

INSERT INTO dbo.RTA_PerfilAccion (IdPerfil, Id_Accion, Estado, Usuario_Registro, Fecha_Registro)
SELECT 10, a.Id_Accion, 'A', 'MIGRACION', GETDATE()
  FROM dbo.RTA_Accion AS a
 WHERE a.Clave = 'Agenda.VerTodos'
   AND NOT EXISTS (SELECT 1 FROM dbo.RTA_PerfilAccion AS pa WHERE pa.IdPerfil = 10 AND pa.Id_Accion = a.Id_Accion);

COMMIT TRANSACTION;
PRINT 'Menu y accion listos. Correr 2026-10-15-agenda-verificacion.sql.';
GO

/* ------------------------------------------------------------ REVERSA ------
BEGIN TRANSACTION;
DECLARE @A INT = (SELECT Id_Accion FROM dbo.RTA_Accion WHERE Clave = 'Agenda.VerTodos');
DELETE FROM dbo.RTA_PerfilAccion WHERE Id_Accion = @A;
DELETE FROM dbo.RTA_UsuarioAccion WHERE Id_Accion = @A;
DELETE FROM dbo.RTA_Accion WHERE Id_Accion = @A;
DECLARE @M TABLE (Id INT);
INSERT INTO @M SELECT Id_Menu FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) IN ('Agenda.aspx', 'ParametrizacionAreasAgenda.aspx');
INSERT INTO @M SELECT Id_Menu FROM dbo.MenuDos WHERE Es_Opcion_de_Menu = 1 AND Titulo = 'Equipo y agenda';
DELETE FROM dbo.PerfilMenu WHERE id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.R_UsuarioMenu WHERE Id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.MenuDos WHERE Id_Menu IN (SELECT Id FROM @M);
COMMIT TRANSACTION;
---------------------------------------------------------------------------- */
