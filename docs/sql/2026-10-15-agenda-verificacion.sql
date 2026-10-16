/* ============================================================================
   Equipo y agenda (Entrega 1) - verificacion (solo lectura)
   Correr despues de los tres scripts anteriores, y otra vez despues de
   publicar los binarios. Termina en VEREDICTO: OK o VEREDICTO: FALLA.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF OBJECT_ID('tempdb..#C2') IS NOT NULL DROP TABLE #C2;
IF OBJECT_ID('tempdb..#Ciclos') IS NOT NULL DROP TABLE #Ciclos;
IF OBJECT_ID('tempdb..#Dif') IS NOT NULL DROP TABLE #Dif;

DECLARE @Fallos INT = 0;

/* 1. una sola "Sin area", activa */
DECLARE @SinArea INT = (SELECT COUNT(*) FROM dbo.AG_Area WHERE EsSinArea = 1 AND Activa = 1);
SELECT Chequeo = '1. filas Sin area activas (esperado 1)', Valor = @SinArea;
IF @SinArea <> 1 SET @Fallos += 1;

/* 2. personas activas con Departamento pero sin area */
;WITH Activos AS
(
    SELECT Cod = LTRIM(RTRIM(u.Cod_Usuario)), Depto = LTRIM(RTRIM(ISNULL(u.Departamento, '')))
      FROM dbo.R_Usuarios AS u
     WHERE u.Usuario_Estado = 'A' AND u.EstadoUsuario IS NULL AND ISNULL(u.IdCliente, 0) = 0
       AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
)
SELECT Chequeo = '2. con Departamento y sin area (esperado 0)', Valor = COUNT(*)
  INTO #C2
  FROM Activos AS x
 WHERE x.Depto <> '' AND NOT EXISTS (SELECT 1 FROM dbo.AG_PersonaArea AS pa WHERE pa.Cod_Usuario = x.Cod);
SELECT * FROM #C2;
IF (SELECT Valor FROM #C2) <> 0 SET @Fallos += 1;

/* 3. ciclos de jefes entre los activos */
;WITH A AS
(
    SELECT Cod = LTRIM(RTRIM(Cod_Usuario)), Jefe = LTRIM(RTRIM(ISNULL(Cod_Jefe_Inm, '')))
      FROM dbo.R_Usuarios
     WHERE Usuario_Estado = 'A' AND EstadoUsuario IS NULL AND ISNULL(IdCliente, 0) = 0
),
R AS
(
    SELECT Origen = Cod, Actual = Jefe, Nivel = 1 FROM A WHERE Jefe <> '' AND Jefe <> Cod
    UNION ALL
    SELECT R.Origen, A.Jefe, R.Nivel + 1
      FROM R JOIN A ON A.Cod = R.Actual
     WHERE A.Jefe <> '' AND A.Jefe <> A.Cod AND R.Actual <> R.Origen AND R.Nivel < 50
)
SELECT DISTINCT Origen INTO #Ciclos FROM R WHERE Actual = Origen OPTION (MAXRECURSION 60);
SELECT Chequeo = '3. personas en un ciclo de jefes (esperado 0)', Valor = COUNT(*) FROM #Ciclos;
SELECT EnCiclo = Origen FROM #Ciclos;
IF EXISTS (SELECT 1 FROM #Ciclos) SET @Fallos += 1;

/* 4. menu */
DECLARE @IdAgenda INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'Agenda.aspx');
DECLARE @IdAreas  INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'ParametrizacionAreasAgenda.aspx');
DECLARE @IdGrupo  INT = (SELECT Id_MenuPadre FROM dbo.MenuDos WHERE Id_Menu = @IdAgenda);
SELECT Chequeo = '4. ids de menu', IdGrupo = @IdGrupo, IdAgenda = @IdAgenda, IdAreas = @IdAreas;
IF @IdAgenda IS NULL OR @IdAreas IS NULL OR @IdGrupo IS NULL SET @Fallos += 1;

DECLARE @Todos TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Todos SELECT DISTINCT Id_Perfil FROM dbo.R_Usuarios
 WHERE Usuario_Estado = 'A' AND EstadoUsuario IS NULL AND ISNULL(IdCliente, 0) = 0 AND Id_Perfil IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM @Todos WHERE IdPerfil = 18) INSERT INTO @Todos VALUES (18);

DECLARE @E TABLE (IdMenu INT, IdPerfil INT);
INSERT INTO @E SELECT @IdGrupo, IdPerfil FROM @Todos;
INSERT INTO @E SELECT @IdAgenda, IdPerfil FROM @Todos;
INSERT INTO @E VALUES (@IdAreas, 14), (@IdAreas, 18);

;WITH R AS (SELECT pm.id_Menu, pm.IdPerfil FROM dbo.PerfilMenu AS pm
             WHERE pm.Estado = '0' AND pm.id_Menu IN (@IdGrupo, @IdAgenda, @IdAreas))
SELECT Diferencia = CASE WHEN r.id_Menu IS NULL THEN 'FALTA en el menu' ELSE 'SOBRA en el menu' END,
       IdMenu = ISNULL(e.IdMenu, r.id_Menu), IdPerfil = ISNULL(e.IdPerfil, r.IdPerfil)
  INTO #Dif
  FROM @E AS e FULL JOIN R AS r ON r.id_Menu = e.IdMenu AND r.IdPerfil = e.IdPerfil
 WHERE e.IdMenu IS NULL OR r.id_Menu IS NULL;
SELECT * FROM #Dif ORDER BY IdMenu, IdPerfil;
IF EXISTS (SELECT 1 FROM #Dif) SET @Fallos += 1;

/* 5. accion Agenda.VerTodos: ligada a Agenda.aspx y dada solo al perfil 10 */
SELECT Chequeo = '5. accion', a.Clave, a.Id_Menu, a.Estado, pa.IdPerfil, PerfilEstado = pa.Estado
  FROM dbo.RTA_Accion AS a
  LEFT JOIN dbo.RTA_PerfilAccion AS pa ON pa.Id_Accion = a.Id_Accion
 WHERE a.Clave = 'Agenda.VerTodos';
IF NOT EXISTS (SELECT 1 FROM dbo.RTA_Accion AS a JOIN dbo.RTA_PerfilAccion AS pa ON pa.Id_Accion = a.Id_Accion
                WHERE a.Clave = 'Agenda.VerTodos' AND a.Id_Menu = @IdAgenda AND a.Estado = 'A'
                  AND pa.IdPerfil = 10 AND pa.Estado = 'A')
   OR EXISTS (SELECT 1 FROM dbo.RTA_Accion AS a JOIN dbo.RTA_PerfilAccion AS pa ON pa.Id_Accion = a.Id_Accion
               WHERE a.Clave = 'Agenda.VerTodos' AND pa.IdPerfil <> 10 AND pa.Estado = 'A')
    SET @Fallos += 1;

/* 6. informativo: personas cuyo jefe no esta entre los activos (solo las ve VerTodos) */
;WITH A AS
(
    SELECT Cod = LTRIM(RTRIM(Cod_Usuario)), Nombre = Nom_Usuario, Jefe = LTRIM(RTRIM(ISNULL(Cod_Jefe_Inm, '')))
      FROM dbo.R_Usuarios
     WHERE Usuario_Estado = 'A' AND EstadoUsuario IS NULL AND ISNULL(IdCliente, 0) = 0
)
SELECT Informativo = '6. jefe fuera de los activos', a.Cod, a.Nombre, a.Jefe
  FROM A AS a
 WHERE a.Jefe NOT IN ('', '0') AND NOT EXISTS (SELECT 1 FROM A AS j WHERE j.Cod = a.Jefe);

SELECT Veredicto = CASE WHEN @Fallos = 0 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA' END, Fallos = @Fallos;
