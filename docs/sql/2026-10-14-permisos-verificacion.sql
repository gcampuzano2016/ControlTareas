/* Verificacion de permisos (solo lectura). Matriz esperada despues de los dos
   scripts del 2026-10-14. El 18 se exige en el menu de todas para que las
   vea; su acceso no depende de esto. */
SET NOCOUNT ON;
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
    SELECT DISTINCT m.Href, pm.IdPerfil FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
     WHERE pm.Estado = 0 AND m.Href IN (SELECT Href FROM @E))
SELECT Diferencia = CASE WHEN r.Href IS NULL THEN 'FALTA en el menu' ELSE 'SOBRA en el menu' END,
       Href = ISNULL(e.Href, r.Href), IdPerfil = ISNULL(e.IdPerfil, r.IdPerfil)
  INTO #Dif
  FROM @E e FULL JOIN R r ON r.Href = e.Href AND r.IdPerfil = e.IdPerfil
 WHERE e.Href IS NULL OR r.Href IS NULL;

SELECT * FROM #Dif ORDER BY Href, IdPerfil;

SELECT Accion = a.Clave, pa.IdPerfil, pa.Estado FROM dbo.RTA_PerfilAccion pa JOIN dbo.RTA_Accion a ON a.Id_Accion = pa.Id_Accion ORDER BY a.Clave, pa.IdPerfil;

IF (SELECT COUNT(*) FROM #Dif) = 0
    PRINT 'VEREDICTO: OK - el menu coincide con la matriz acordada.';
ELSE
    RAISERROR('VEREDICTO: FALLO - el menu difiere de la matriz acordada (ver filas).', 16, 1);
DROP TABLE #Dif;
