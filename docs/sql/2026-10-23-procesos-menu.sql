/* ============================================================================
   Procesos entre areas (Entrega 2 de la agenda) - menu
   ReporTarea  |  2026-10-23
   Se corre AL DESPLEGAR, justo despues de los binarios y las paginas: antes, la
   opcion de menu llevaria a paginas que todavia no existen en el servidor.
   Agrega dos hojas al grupo "Equipo y agenda" que ya existe (Entrega 1):
     1. Procesos.aspx "Procesos" (orden 3 dentro del grupo)
        -> todo perfil con usuarios internos activos, y el 18
     2. ParametrizacionPlantillasProcesos.aspx "Plantillas de procesos" (orden 4)
        -> perfiles 10 (Gerencia) y 18
   El grupo y las hojas de la Entrega 1 no se tocan. Los perfiles 10 y 18 se
   aseguran tambien en el grupo (sin el, la hoja no se ve).
   La llave de "gestionar cualquier proceso" es la accion Agenda.VerTodos, que ya
   existe: aqui no se crea ninguna accion.
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

DECLARE @TituloGrupo VARCHAR(128) = 'Equipo y agenda';
DECLARE @TituloProc  VARCHAR(128) = 'Procesos';
DECLARE @TituloPlan  VARCHAR(128) = 'Plantillas de procesos';
DECLARE @IdModeloHoja INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'ParametrizacionAreasAgenda.aspx');
DECLARE @IdGrupo INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE Es_Opcion_de_Menu = 1 AND Titulo = @TituloGrupo);
DECLARE @IdProc INT, @IdPlan INT;

IF @IdGrupo IS NULL OR @IdModeloHoja IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('Falta el grupo "Equipo y agenda" o la hoja modelo (Entrega 1: 2026-10-15-agenda-menu.sql).', 16, 1);
    RETURN;
END

/* 1. hoja Procesos.aspx */
SELECT @IdProc = MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'Procesos.aspx';
IF @IdProc IS NULL
BEGIN
    EXEC #ClonarMenu @IdModeloHoja, @IdProc OUTPUT;
    UPDATE dbo.MenuDos
       SET Href = 'Procesos.aspx', Titulo = @TituloProc, Descripcion = 'Procesos de pasos entre areas',
           Id_MenuPadre = @IdGrupo, Es_Opcion_de_Menu = 0, Orden_Opcion = 3
     WHERE Id_Menu = @IdProc;
    PRINT 'Hoja Procesos.aspx creada: ' + CONVERT(VARCHAR(10), @IdProc);
END

/* 2. hoja ParametrizacionPlantillasProcesos.aspx */
SELECT @IdPlan = MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'ParametrizacionPlantillasProcesos.aspx';
IF @IdPlan IS NULL
BEGIN
    EXEC #ClonarMenu @IdModeloHoja, @IdPlan OUTPUT;
    UPDATE dbo.MenuDos
       SET Href = 'ParametrizacionPlantillasProcesos.aspx', Titulo = @TituloPlan,
           Descripcion = 'Plantillas de procesos entre areas',
           Id_MenuPadre = @IdGrupo, Es_Opcion_de_Menu = 0, Orden_Opcion = 4
     WHERE Id_Menu = @IdPlan;
    PRINT 'Hoja ParametrizacionPlantillasProcesos.aspx creada: ' + CONVERT(VARCHAR(10), @IdPlan);
END

/* 3. visibilidad. Una hoja solo se ve si su grupo tambien es visible para el perfil. */
DECLARE @Todos TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Todos (IdPerfil)
SELECT DISTINCT u.Id_Perfil
  FROM dbo.R_Usuarios AS u
 WHERE u.Usuario_Estado = 'A' AND u.EstadoUsuario IS NULL AND ISNULL(u.IdCliente, 0) = 0
   AND u.Id_Perfil IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM @Todos WHERE IdPerfil = 18) INSERT INTO @Todos (IdPerfil) VALUES (18);

DECLARE @Vis TABLE (IdMenu INT, IdPerfil INT, PRIMARY KEY (IdMenu, IdPerfil));
INSERT INTO @Vis (IdMenu, IdPerfil) SELECT @IdProc, IdPerfil FROM @Todos;
INSERT INTO @Vis (IdMenu, IdPerfil) VALUES (@IdPlan, 10), (@IdPlan, 18), (@IdGrupo, 10), (@IdGrupo, 18);
/* el grupo debe verse para todo perfil que vera Procesos.aspx */
INSERT INTO @Vis (IdMenu, IdPerfil) SELECT @IdGrupo, IdPerfil FROM @Todos WHERE IdPerfil NOT IN (10, 18);
/* los perfiles de @Todos, 10 y 18 quedan con el grupo visible (re-ejecutable: el UPDATE/INSERT siguientes son idempotentes) */

UPDATE pm SET Estado = '0'
  FROM dbo.PerfilMenu AS pm
  JOIN @Vis AS v ON v.IdMenu = pm.id_Menu AND v.IdPerfil = pm.IdPerfil
 WHERE pm.Estado <> '0';

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT v.IdMenu, v.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM @Vis AS v
 WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu AS pm WHERE pm.id_Menu = v.IdMenu AND pm.IdPerfil = v.IdPerfil);

COMMIT TRANSACTION;
PRINT 'Menu listo. Correr 2026-10-23-procesos-verificacion.sql.';
GO

/* ------------------------------------------------------------ REVERSA ------
Quita solo las dos hojas nuevas; el grupo y las hojas de la Entrega 1 se quedan.
BEGIN TRANSACTION;
DECLARE @M TABLE (Id INT);
INSERT INTO @M SELECT Id_Menu FROM dbo.MenuDos
 WHERE LTRIM(RTRIM(Href)) IN ('Procesos.aspx', 'ParametrizacionPlantillasProcesos.aspx');
DELETE FROM dbo.PerfilMenu WHERE id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.R_UsuarioMenu WHERE Id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.MenuDos WHERE Id_Menu IN (SELECT Id FROM @M);
COMMIT TRANSACTION;
---------------------------------------------------------------------------- */
