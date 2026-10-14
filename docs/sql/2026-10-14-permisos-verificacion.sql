/* Verificacion de permisos (solo lectura). Matriz esperada despues de los dos
   scripts del 2026-10-14. El 18 se exige en el menu de todas para que las
   vea; su acceso no depende de esto.

   Corre ANTES de publicar los binarios (despues del esquema y los ajustes de
   menu) y otra vez despues. Tambien revisa los extras por usuario
   (R_UsuarioMenu) y que no haya acciones por usuario, porque desde este cambio
   un extra da acceso a la pantalla protegida. */
SET NOCOUNT ON;
DECLARE @Fallos INT = 0;
IF OBJECT_ID('tempdb..#Dif') IS NOT NULL DROP TABLE #Dif;
DECLARE @E TABLE (Href VARCHAR(100), IdPerfil INT);
INSERT @E VALUES
 ('TableroCostos.aspx',14),('TableroCostos.aspx',18),
 ('HorasExtras.aspx',2),('HorasExtras.aspx',14),('HorasExtras.aspx',18),
 ('ParametrizacionHorasExtras.aspx',14),('ParametrizacionHorasExtras.aspx',18),
 ('CurvaColaboradores.aspx',14),('CurvaColaboradores.aspx',18),
 ('PerfilesPersonal.aspx',14),('PerfilesPersonal.aspx',18),
 ('ParametrizacionPerfiles.aspx',18),('ParametrizacionPerfiles.aspx',19),
 ('ParametrizacionUsuarios.aspx',14),('ParametrizacionUsuarios.aspx',18),('ParametrizacionUsuarios.aspx',19),
 ('ParametrizacionMenuPerfil.aspx',18),('ParametrizacionMenuUsuario.aspx',18);

;WITH R AS (
    SELECT DISTINCT Href = LTRIM(RTRIM(m.Href)), pm.IdPerfil FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
     WHERE pm.Estado = 0 AND LTRIM(RTRIM(m.Href)) IN (SELECT Href FROM @E))
SELECT Diferencia = CASE WHEN r.Href IS NULL THEN 'FALTA en el menu' ELSE 'SOBRA en el menu' END,
       Href = ISNULL(e.Href, r.Href), IdPerfil = ISNULL(e.IdPerfil, r.IdPerfil)
  INTO #Dif
  FROM @E e FULL JOIN R r ON r.Href = e.Href AND r.IdPerfil = e.IdPerfil
 WHERE e.Href IS NULL OR r.Href IS NULL;

SELECT * FROM #Dif ORDER BY Href, IdPerfil;

SELECT Accion = a.Clave, pa.IdPerfil, pa.Estado FROM dbo.RTA_PerfilAccion pa JOIN dbo.RTA_Accion a ON a.Id_Accion = pa.Id_Accion ORDER BY a.Clave, pa.IdPerfil;

IF (SELECT COUNT(*) FROM #Dif) > 0 SET @Fallos += 1;

/* ---- extras por usuario sobre las 9 pantallas protegidas (sin filtro de estado de usuario) ---- */
DECLARE @P TABLE (Href VARCHAR(100));
INSERT @P VALUES ('TableroCostos.aspx'),('HorasExtras.aspx'),('ParametrizacionHorasExtras.aspx'),
 ('CurvaColaboradores.aspx'),('PerfilesPersonal.aspx'),('ParametrizacionPerfiles.aspx'),
 ('ParametrizacionUsuarios.aspx'),('ParametrizacionMenuPerfil.aspx'),('ParametrizacionMenuUsuario.aspx');

SELECT Extra = LTRIM(RTRIM(m.Href)), um.Cod_Usuario,
       Id_Perfil = (SELECT MAX(u.Id_Perfil) FROM dbo.R_Usuarios u WHERE u.Cod_Usuario = um.Cod_Usuario),
       Usuario_Estado = (SELECT MAX(CAST(u.Usuario_Estado AS VARCHAR(10))) FROM dbo.R_Usuarios u WHERE u.Cod_Usuario = um.Cod_Usuario),
       EstadoUsuario = (SELECT MAX(CAST(u.EstadoUsuario AS VARCHAR(10))) FROM dbo.R_Usuarios u WHERE u.Cod_Usuario = um.Cod_Usuario)
  INTO #Ext
  FROM dbo.R_UsuarioMenu um JOIN dbo.MenuDos m ON m.Id_Menu = um.Id_Menu
 WHERE um.Estado = 'A' AND LTRIM(RTRIM(m.Href)) IN (SELECT Href FROM @P);

SELECT * FROM #Ext ORDER BY Cod_Usuario, Extra;

/* esperado: un solo usuario, de perfil 2, solo con HorasExtras.aspx y TableroCostos.aspx */
IF EXISTS (SELECT 1 FROM #Ext WHERE Extra NOT IN ('HorasExtras.aspx','TableroCostos.aspx'))
BEGIN RAISERROR('FALLO: hay extras por usuario sobre una pantalla protegida distinta de HorasExtras/TableroCostos.', 16, 1); SET @Fallos += 1; END
IF (SELECT COUNT(DISTINCT Cod_Usuario) FROM #Ext) > 1
BEGIN RAISERROR('FALLO: mas de un usuario tiene extras sobre pantallas protegidas.', 16, 1); SET @Fallos += 1; END
IF EXISTS (SELECT 1 FROM #Ext WHERE ISNULL(Id_Perfil, -1) <> 2)
BEGIN RAISERROR('FALLO: el usuario con extras sobre pantallas protegidas no es de perfil 2.', 16, 1); SET @Fallos += 1; END
DROP TABLE #Ext;

/* ---- acciones por usuario: se espera ninguna ---- */
SELECT Accion = a.Clave, ua.Cod_Usuario
  INTO #UA
  FROM dbo.RTA_UsuarioAccion ua JOIN dbo.RTA_Accion a ON a.Id_Accion = ua.Id_Accion
 WHERE ua.Estado = 'A';
SELECT * FROM #UA ORDER BY Accion, Cod_Usuario;
IF EXISTS (SELECT 1 FROM #UA)
BEGIN RAISERROR('FALLO: hay acciones activas por usuario (se esperaba ninguna).', 16, 1); SET @Fallos += 1; END
DROP TABLE #UA;

IF @Fallos = 0
    PRINT 'VEREDICTO: OK - el menu coincide con la matriz y no hay extras ni acciones inesperados.';
ELSE
    RAISERROR('VEREDICTO: FALLO - %d comprobaciones fallaron (ver filas y mensajes).', 16, 1, @Fallos);
DROP TABLE #Dif;
