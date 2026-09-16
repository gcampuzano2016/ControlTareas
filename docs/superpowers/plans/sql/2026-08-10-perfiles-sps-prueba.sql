SET QUOTED_IDENTIFIER ON;
GO

PRINT '=== CASO A: perfil sin usuarios, con fila en PerfilMenu -> debe borrarse ===';
-- OJO: dbo.Perfiles no tiene FK entrante, asi que R_Usuarios puede tener
-- Id_Perfil/Cod_Perfil "huerfanos" que no corresponden a ningun perfil
-- vigente (se detectaron 4 en produccion: -1, 20, 21 y 41 al momento de
-- esta prueba). Si el proximo IDENTITY coincide con uno de esos valores
-- huerfanos, el perfil de prueba "sin usuarios" aparece con usuarios y
-- Sp_RTA_EliminarPerfil lo rechaza correctamente (eso ya paso una vez
-- al ejecutar esta prueba: caso A choco con el Id_Perfil huerfano 20 de
-- un usuario real). Por eso se verifica el conteo ANTES de asumir que
-- el caso es realmente "sin usuarios".
DECLARE @idA bigint;

INSERT INTO dbo.Perfiles (Codigo, NombrePerfil, Estado, Fecha)
VALUES (0, 'ZZ_PRUEBA_CasoA', 1, GETDATE());
SET @idA = SCOPE_IDENTITY();
UPDATE dbo.Perfiles SET Codigo = @idA WHERE IdPerfiles = @idA;

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
VALUES (1, @idA, GETDATE(), '1');

PRINT 'Id del perfil de prueba A:';
SELECT @idA AS IdPerfilA;

PRINT 'Usuarios reales que matchean este id (si no es 0, este id choco con un Id_Perfil/Cod_Perfil huerfano preexistente):';
SELECT COUNT(*) AS UsuariosReales FROM dbo.R_Usuarios WHERE Id_Perfil = @idA OR Cod_Perfil = @idA;

PRINT 'Filas en PerfilMenu ANTES de borrar:';
SELECT * FROM dbo.PerfilMenu WHERE IdPerfil = @idA;

EXEC dbo.Sp_RTA_EliminarPerfil @IdPerfiles = @idA;

PRINT 'Perfiles restantes con ese id (debe ser 0 filas):';
SELECT * FROM dbo.Perfiles WHERE IdPerfiles = @idA;

PRINT 'PerfilMenu restantes con ese id (debe ser 0 filas):';
SELECT * FROM dbo.PerfilMenu WHERE IdPerfil = @idA;
GO

PRINT '=== CASO B: perfil real con usuarios -> debe rechazarse ===';
-- Perfil 1 "Tecnico" tiene usuarios (verificado antes de esta prueba).
EXEC dbo.Sp_RTA_EliminarPerfil @IdPerfiles = 1;

PRINT 'El perfil 1 debe seguir existiendo:';
SELECT IdPerfiles, Codigo, NombrePerfil FROM dbo.Perfiles WHERE IdPerfiles = 1;
GO

PRINT '=== CASO C: id inexistente (999999) -> debe rechazarse sin error ===';
EXEC dbo.Sp_RTA_EliminarPerfil @IdPerfiles = 999999;
GO

PRINT '=== LISTADO: sin filtro ===';
EXEC dbo.Sp_RTA_ListarPerfilesAdmin @filtro = '';

PRINT 'Suma de columna Usuarios (sin filtro):';
CREATE TABLE #listado (IdPerfiles bigint, NombrePerfil varchar(150), Estado int, Usuarios int);
INSERT INTO #listado EXEC dbo.Sp_RTA_ListarPerfilesAdmin @filtro = '';
SELECT SUM(Usuarios) AS TotalUsuariosEnListado FROM #listado;

PRINT 'Perfiles con usuarios reales (fuera del SP) que aparecen con 0 en el listado (debe ser 0 filas):';
SELECT l.IdPerfiles, l.NombrePerfil, l.Usuarios AS UsuariosListado,
       (SELECT COUNT(*) FROM dbo.R_Usuarios u WHERE u.Id_Perfil = p.IdPerfiles OR u.Cod_Perfil = p.Codigo) AS UsuariosReales
FROM #listado l
JOIN dbo.Perfiles p ON p.IdPerfiles = l.IdPerfiles
WHERE l.Usuarios = 0
  AND (SELECT COUNT(*) FROM dbo.R_Usuarios u WHERE u.Id_Perfil = p.IdPerfiles OR u.Cod_Perfil = p.Codigo) > 0;

DROP TABLE #listado;
GO

PRINT '=== LISTADO: con filtro que coincide con "Tecnico" ===';
EXEC dbo.Sp_RTA_ListarPerfilesAdmin @filtro = 'Tecnico';
GO

PRINT '=== ALTA: correcta ===';
DECLARE @respAlta int;
DECLARE @tabla TABLE (Respuestas int);
INSERT INTO @tabla
EXEC dbo.Sp_RTAInsertaNuevoPerfil @Codigo = 0, @NombrePerfil = 'ZZ_PRUEBA_AltaOk', @Estado = 1, @Fecha = NULL;
SELECT @respAlta = Respuestas FROM @tabla;
PRINT 'Respuestas devuelto (debe ser > 0):';
SELECT @respAlta AS Respuestas;

PRINT 'Fila insertada, Codigo debe quedar igual a IdPerfiles:';
SELECT IdPerfiles, Codigo, NombrePerfil, Estado, Fecha FROM dbo.Perfiles WHERE IdPerfiles = @respAlta;
GO

PRINT '=== ALTA: nombre repetido (ZZ_PRUEBA_AltaOk) -> debe devolver -1 y no insertar ===';
DECLARE @countAntes int, @countDespues int;
SELECT @countAntes = COUNT(*) FROM dbo.Perfiles WHERE NombrePerfil = 'ZZ_PRUEBA_AltaOk';

DECLARE @tabla2 TABLE (Respuestas int);
INSERT INTO @tabla2
EXEC dbo.Sp_RTAInsertaNuevoPerfil @Codigo = 0, @NombrePerfil = 'ZZ_PRUEBA_AltaOk', @Estado = 1, @Fecha = NULL;
SELECT Respuestas AS RespuestasNombreRepetido FROM @tabla2;

SELECT @countDespues = COUNT(*) FROM dbo.Perfiles WHERE NombrePerfil = 'ZZ_PRUEBA_AltaOk';
PRINT 'Cantidad de filas con ese nombre antes y despues (deben ser iguales, ambas 1):';
SELECT @countAntes AS Antes, @countDespues AS Despues;
GO

PRINT '=== ALTA: nombre vacio -> debe devolver -1 y no insertar ===';
DECLARE @countPrev int;
SELECT @countPrev = COUNT(*) FROM dbo.Perfiles WHERE NombrePerfil LIKE 'ZZ_PRUEBA_%';

DECLARE @tabla3 TABLE (Respuestas int);
INSERT INTO @tabla3
EXEC dbo.Sp_RTAInsertaNuevoPerfil @Codigo = 0, @NombrePerfil = '', @Estado = 1, @Fecha = NULL;
SELECT Respuestas AS RespuestasNombreVacio FROM @tabla3;

DECLARE @countPost int;
SELECT @countPost = COUNT(*) FROM dbo.Perfiles WHERE NombrePerfil LIKE 'ZZ_PRUEBA_%';
PRINT 'Cantidad de filas ZZ_PRUEBA_% antes y despues del intento con nombre vacio (deben ser iguales):';
SELECT @countPrev AS Antes, @countPost AS Despues;
GO

PRINT '=== LIMPIEZA: borrar todos los perfiles de prueba ===';
DELETE FROM dbo.PerfilMenu WHERE IdPerfil IN (SELECT IdPerfiles FROM dbo.Perfiles WHERE NombrePerfil LIKE 'ZZ_PRUEBA_%');
DELETE FROM dbo.RTA_PerfilInicio WHERE IdPerfil IN (SELECT IdPerfiles FROM dbo.Perfiles WHERE NombrePerfil LIKE 'ZZ_PRUEBA_%');
DELETE FROM dbo.Perfiles WHERE NombrePerfil LIKE 'ZZ_PRUEBA_%';

PRINT 'Verificacion final: no debe quedar ninguna fila ZZ_PRUEBA_% en Perfiles:';
SELECT * FROM dbo.Perfiles WHERE NombrePerfil LIKE 'ZZ_PRUEBA_%';
GO
