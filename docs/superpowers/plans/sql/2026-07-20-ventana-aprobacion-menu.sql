/* ============================================================================
   Ventana de aprobación por jefe inmediato — Task 6
   Registro de la pantalla ParametrizacionVentanaAprobacion.aspx en el menú
   para los perfiles 1, 2, 18 y 19.

   BD: ReporTarea.  Ejecutar en SSMS (el archivo tiene acentos; los literales
   usan N'' para evitar problemas de codificación).

   Semántica invertida de PerfilMenu:  Estado = '0' MUESTRA,  '1' OCULTA.
   Una opción hija solo se ve si su grupo padre (Id_MenuPadre = 0) también
   tiene Estado = '0' para ese perfil.

   MODO DE USO
   -----------
   1) Ejecutar tal cual (@Aplicar = 0). No cambia nada: imprime el diagnóstico
      y hace ROLLBACK. Revisar sobre todo el bloque "IMPACTO" (opciones que
      quedarían visibles al abrir el grupo padre para los perfiles 2 y 19).
   2) Si el diagnóstico es correcto, poner @Aplicar = 1 y volver a ejecutar.
   ============================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Aplicar BIT = 0;   -- <<<<<< 0 = simulación (rollback) | 1 = aplicar cambios

DECLARE @HrefNuevo   NVARCHAR(200) = N'ParametrizacionVentanaAprobacion.aspx';
DECLARE @HrefModelo  NVARCHAR(200) = N'ParametrizacionHorarioUsuario.aspx';
DECLARE @TituloNuevo NVARCHAR(200) = N'Parametrización de Ventana de Aprobación';

/* ---------------------------------------------------------------------------
   0) Validaciones previas de esquema — fallar ruidosamente, nunca en silencio
   --------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.MenuDos', 'Titulo') IS NULL
    OR COL_LENGTH('dbo.MenuDos', 'Href') IS NULL
    OR COL_LENGTH('dbo.MenuDos', 'Id_MenuPadre') IS NULL
BEGIN
    RAISERROR('dbo.MenuDos no tiene las columnas esperadas (Titulo/Href/Id_MenuPadre). Revisar el esquema antes de continuar.', 16, 1);
    RETURN;
END

IF COL_LENGTH('dbo.PerfilMenu', 'Id_Perfil') IS NULL
    OR COL_LENGTH('dbo.PerfilMenu', 'Id_Menu') IS NULL
    OR COL_LENGTH('dbo.PerfilMenu', 'Estado') IS NULL
BEGIN
    -- Si falla, ejecutar esto y ajustar el nombre de la columna de perfil en el script:
    --   SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID('dbo.PerfilMenu') ORDER BY c.column_id;
    RAISERROR('dbo.PerfilMenu no tiene las columnas esperadas (Id_Perfil/Id_Menu/Estado). Revisar el esquema antes de continuar.', 16, 1);
    RETURN;
END

-- Los INSERT sobre PerfilMenu solo informan Id_Menu, Id_Perfil y Estado. Si la
-- tabla tuviera otra columna obligatoria sin DEFAULT, el MERGE fallaría con un
-- error poco claro; se avisa aquí antes de empezar.
IF EXISTS (SELECT 1
           FROM   sys.columns c
           WHERE  c.object_id = OBJECT_ID('dbo.PerfilMenu')
             AND  c.is_nullable = 0
             AND  c.is_identity = 0
             AND  c.default_object_id = 0
             AND  c.name NOT IN ('Id_Menu', 'Id_Perfil', 'Estado'))
BEGIN
    SELECT 'Columna obligatoria no contemplada' AS Problema, c.name
    FROM   sys.columns c
    WHERE  c.object_id = OBJECT_ID('dbo.PerfilMenu')
      AND  c.is_nullable = 0 AND c.is_identity = 0 AND c.default_object_id = 0
      AND  c.name NOT IN ('Id_Menu', 'Id_Perfil', 'Estado');
    RAISERROR('dbo.PerfilMenu tiene columnas NOT NULL sin DEFAULT que este script no informa (ver resultado). Añadirlas a los INSERT del MERGE antes de aplicar.', 16, 1);
    RETURN;
END

DECLARE @idModelo INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE Href = @HrefModelo);
IF @idModelo IS NULL
BEGIN
    RAISERROR('No se encontró la opción de menú modelo (ParametrizacionHorarioUsuario.aspx). No se puede clonar.', 16, 1);
    RETURN;
END

DECLARE @idPadre INT = (SELECT Id_MenuPadre FROM dbo.MenuDos WHERE Id_Menu = @idModelo);

PRINT '--- Modelo -------------------------------------------------';
PRINT 'Id_Menu modelo (Horario) = ' + CAST(@idModelo AS VARCHAR(20));
PRINT 'Id_MenuPadre (grupo)     = ' + CAST(@idPadre  AS VARCHAR(20));

/* ---------------------------------------------------------------------------
   IMPACTO: al abrir el grupo padre para los perfiles 2 y 19, ¿qué OTRAS
   opciones (que ya tienen Estado='0' propio) pasarían a ser visibles?
   Revisar esta lista antes de aplicar.
   --------------------------------------------------------------------------- */
SELECT  'IMPACTO: se volverá visible' AS Advertencia,
        pm.Id_Perfil,
        m.Id_Menu,
        m.Titulo,
        m.Href
FROM    dbo.PerfilMenu pm
JOIN    dbo.MenuDos    m ON m.Id_Menu = pm.Id_Menu
WHERE   m.Id_MenuPadre = @idPadre
  AND   pm.Id_Perfil IN (2, 19)
  AND   pm.Estado = '0'
  AND   NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu p
                    WHERE p.Id_Menu = @idPadre AND p.Id_Perfil = pm.Id_Perfil AND p.Estado = '0')
ORDER BY pm.Id_Perfil, m.Id_Menu;

/* ---------------------------------------------------------------------------
   Cambios
   --------------------------------------------------------------------------- */
BEGIN TRAN;

DECLARE @idNuevo INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE Href = @HrefNuevo);

IF @idNuevo IS NOT NULL
BEGIN
    PRINT 'La opción de menú ya existe (Id_Menu = ' + CAST(@idNuevo AS VARCHAR(20)) + '); solo se revisan los permisos.';
END
ELSE
BEGIN
    /* 1) Clonar la fila de MenuDos del modelo copiando TODAS las columnas
          no-identidad (así arrastra Id_MenuPadre, Class_Icon, Es_Opcion_de_Menu
          y cualquier columna que no conozcamos). El clon depende de que
          Id_Menu sea IDENTITY: si no lo fuera, el INSERT duplicaría la PK. */
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.MenuDos')
                     AND name = 'Id_Menu' AND is_identity = 1)
    BEGIN
        ROLLBACK;
        RAISERROR('dbo.MenuDos.Id_Menu no es IDENTITY: el clon automático no es seguro. Insertar la fila manualmente asignando un Id_Menu libre.', 16, 1);
        RETURN;
    END

    DECLARE @cols NVARCHAR(MAX) = STUFF((
        SELECT ',' + QUOTENAME(c.name)
        FROM   sys.columns c
        WHERE  c.object_id = OBJECT_ID('dbo.MenuDos') AND c.is_identity = 0
        ORDER  BY c.column_id
        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '');

    DECLARE @sql NVARCHAR(MAX) =
        N'INSERT INTO dbo.MenuDos (' + @cols + N') ' +
        N'SELECT ' + @cols + N' FROM dbo.MenuDos WHERE Id_Menu = @idModelo; ' +
        N'SET @idOut = CONVERT(INT, SCOPE_IDENTITY());';

    EXEC sys.sp_executesql @sql,
         N'@idModelo INT, @idOut INT OUTPUT',
         @idModelo = @idModelo,
         @idOut    = @idNuevo OUTPUT;

    /* 2) Ajustar Href y título del clon */
    UPDATE dbo.MenuDos
       SET Href   = @HrefNuevo,
           Titulo = @TituloNuevo
     WHERE Id_Menu = @idNuevo;

    PRINT 'Opción creada. Nuevo Id_Menu = ' + CAST(@idNuevo AS VARCHAR(20));
END

/* 3) Visibilidad de la NUEVA opción para los perfiles 1, 2, 18, 19 */
MERGE dbo.PerfilMenu AS t
USING (VALUES (1), (2), (18), (19)) AS p(Id_Perfil)
   ON  t.Id_Menu = @idNuevo AND t.Id_Perfil = p.Id_Perfil
WHEN MATCHED THEN
    UPDATE SET t.Estado = '0'
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id_Menu, Id_Perfil, Estado) VALUES (@idNuevo, p.Id_Perfil, '0');

PRINT 'Permisos de la opción asegurados para perfiles 1, 2, 18, 19.';

/* 4) Visibilidad del GRUPO PADRE para los mismos perfiles (sin esto, los
      perfiles 2 y 19 no verían la opción aunque tengan permiso propio). */
MERGE dbo.PerfilMenu AS t
USING (VALUES (1), (2), (18), (19)) AS p(Id_Perfil)
   ON  t.Id_Menu = @idPadre AND t.Id_Perfil = p.Id_Perfil
WHEN MATCHED THEN
    UPDATE SET t.Estado = '0'
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id_Menu, Id_Perfil, Estado) VALUES (@idPadre, p.Id_Perfil, '0');

PRINT 'Permisos del grupo padre asegurados para perfiles 1, 2, 18, 19.';

/* ---------------------------------------------------------------------------
   Resultado
   --------------------------------------------------------------------------- */
SELECT  'RESULTADO' AS Seccion, m.Id_Menu, m.Id_MenuPadre, m.Titulo, m.Href
FROM    dbo.MenuDos m
WHERE   m.Id_Menu IN (@idNuevo, @idPadre);

SELECT  'PERMISOS' AS Seccion, pm.Id_Menu, pm.Id_Perfil, pm.Estado,
        CASE WHEN pm.Id_Menu = @idPadre THEN 'grupo padre' ELSE 'opción' END AS Nivel
FROM    dbo.PerfilMenu pm
WHERE   pm.Id_Menu IN (@idNuevo, @idPadre)
  AND   pm.Id_Perfil IN (1, 2, 18, 19)
ORDER BY Nivel DESC, pm.Id_Perfil;

IF @Aplicar = 1
BEGIN
    COMMIT;
    PRINT '>>> CAMBIOS APLICADOS (COMMIT).';
END
ELSE
BEGIN
    ROLLBACK;
    PRINT '>>> SIMULACIÓN: no se aplicó ningún cambio (ROLLBACK). Poner @Aplicar = 1 para confirmar.';
END
GO

/* ============================================================================
   VERIFICACIÓN (ejecutar después de aplicar, en una ventana aparte)
   Cada EXEC debe incluir una fila con Href = 'ParametrizacionVentanaAprobacion.aspx'
   ============================================================================ */
-- EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 1;
-- EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 2;
-- EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 18;
-- EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 19;

/* ============================================================================
   ROLLBACK MANUAL (si hubiera que deshacer el registro del menú)
   Ajustar el Id_Menu impreso por el script.
   ============================================================================ */
-- DECLARE @id INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE Href = 'ParametrizacionVentanaAprobacion.aspx');
-- DELETE FROM dbo.PerfilMenu WHERE Id_Menu = @id;
-- DELETE FROM dbo.MenuDos    WHERE Id_Menu = @id;
