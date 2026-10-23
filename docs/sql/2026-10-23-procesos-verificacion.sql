/* ============================================================================
   Procesos entre areas (Entrega 2 de la agenda) - verificacion (solo lectura)
   Correr despues del esquema, y otra vez despues de publicar y de correr el menu.
   Imprime una linea por chequeo (OK, FALLA o PENDIENTE) y termina en
   VEREDICTO: OK o VEREDICTO: FALLA. Todo lo que no sea OK cuenta como falla:
   antes de correr el menu, el chequeo 6 da PENDIENTE a proposito.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DECLARE @Fallos INT = 0;
DECLARE @R TABLE (N INT IDENTITY(1,1), Chequeo VARCHAR(120), Resultado VARCHAR(10), Detalle VARCHAR(300));

/* 1. tablas */
DECLARE @Faltan INT = (SELECT COUNT(*) FROM (VALUES ('dbo.AG_Plantilla'), ('dbo.AG_PlantillaPaso'),
    ('dbo.AG_Proceso'), ('dbo.AG_ProcesoPaso')) AS t (Nombre) WHERE OBJECT_ID(t.Nombre, 'U') IS NULL);
INSERT INTO @R (Chequeo, Resultado, Detalle)
VALUES ('1. tablas AG_Plantilla, AG_PlantillaPaso, AG_Proceso, AG_ProcesoPaso',
        CASE WHEN @Faltan = 0 THEN 'OK' ELSE 'FALLA' END, 'faltan: ' + CONVERT(VARCHAR(10), @Faltan));
IF @Faltan <> 0 SET @Fallos += 1;

/* 2. columnas */
DECLARE @ColFaltan INT = (SELECT COUNT(*) FROM (VALUES
    ('AG_Plantilla', 'Nombre', 0), ('AG_Plantilla', 'Activa', 0),
    ('AG_PlantillaPaso', 'IdArea', 0), ('AG_PlantillaPaso', 'PlazoDias', 0), ('AG_PlantillaPaso', 'Entregables', 1),
    ('AG_Proceso', 'Estado', 0), ('AG_Proceso', 'Cod_IniciadoPor', 0), ('AG_Proceso', 'MotivoCancelacion', 1),
    ('AG_ProcesoPaso', 'Cod_Responsable', 0), ('AG_ProcesoPaso', 'Estado', 0), ('AG_ProcesoPaso', 'FechaActivacion', 1),
    ('AG_Actividad', 'IdProcesoPaso', 1),
    ('AG_CorreoBitacora', 'IdActividad', 1), ('AG_CorreoBitacora', 'IdProceso', 1)
  ) AS c (Tabla, Columna, Nulable)
  WHERE NOT EXISTS (SELECT 1 FROM sys.columns AS k
                     WHERE k.object_id = OBJECT_ID('dbo.' + c.Tabla) AND k.name = c.Columna
                       AND k.is_nullable = c.Nulable));
INSERT INTO @R (Chequeo, Resultado, Detalle)
VALUES ('2. columnas nuevas (y su nulabilidad)', CASE WHEN @ColFaltan = 0 THEN 'OK' ELSE 'FALLA' END,
        'ausentes o con otra nulabilidad: ' + CONVERT(VARCHAR(10), @ColFaltan));
IF @ColFaltan <> 0 SET @Fallos += 1;

/* 3. procedimientos (los 12 nuevos y los 6 de la Entrega 1 que cambian) */
DECLARE @SpFaltan INT = (SELECT COUNT(*) FROM (VALUES
    ('Sp_RTA_AgendaPasoBloquear'), ('Sp_RTA_AgendaActivarPaso'), ('Sp_RTA_AgendaPasoAvanzar'),
    ('Sp_RTA_AgendaFeriados'), ('Sp_RTA_AgendaPlantillas'), ('Sp_RTA_AgendaGuardarPlantilla'),
    ('Sp_RTA_AgendaProcesos'), ('Sp_RTA_AgendaIniciarProceso'), ('Sp_RTA_AgendaReasignarPaso'),
    ('Sp_RTA_AgendaCambiarPasoPendiente'), ('Sp_RTA_AgendaCambiarFechaPaso'), ('Sp_RTA_AgendaCancelarProceso'),
    ('Sp_RTA_AgendaActividades'), ('Sp_RTA_AgendaActividad'), ('Sp_RTA_AgendaGuardarActividad'),
    ('Sp_RTA_AgendaMarcarEntregable'), ('Sp_RTA_AgendaMarcarCompleta'), ('Sp_RTA_AgendaRegistrarCorreo')
  ) AS p (Nombre) WHERE OBJECT_ID('dbo.' + p.Nombre, 'P') IS NULL);
DECLARE @SpViejos INT = (SELECT COUNT(*) FROM (VALUES
    ('Sp_RTA_AgendaActividades', 'IdProcesoPaso'), ('Sp_RTA_AgendaActividad', 'IdProcesoPaso'),
    ('Sp_RTA_AgendaGuardarActividad', 'IdProcesoPaso'),
    ('Sp_RTA_AgendaMarcarEntregable', '@FechaEntregaSiguiente'),
    ('Sp_RTA_AgendaMarcarCompleta', '@FechaEntregaSiguiente'),
    ('Sp_RTA_AgendaRegistrarCorreo', '@IdProceso')
  ) AS v (Nombre, Marca)
  WHERE CHARINDEX(v.Marca, ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.' + v.Nombre)), '')) = 0);
INSERT INTO @R (Chequeo, Resultado, Detalle)
VALUES ('3. procedimientos (18) y versiones nuevas de los 6 de la Entrega 1',
        CASE WHEN @SpFaltan = 0 AND @SpViejos = 0 THEN 'OK' ELSE 'FALLA' END,
        'faltan: ' + CONVERT(VARCHAR(10), @SpFaltan) + ', en version anterior: ' + CONVERT(VARCHAR(10), @SpViejos));
IF @SpFaltan <> 0 OR @SpViejos <> 0 SET @Fallos += 1;

/* 4. AG_CorreoBitacora: el CHECK existe, esta habilitado y confiable, y ninguna fila lo viola */
DECLARE @Ck INT = (SELECT COUNT(*) FROM sys.check_constraints
                    WHERE name = 'CK_AG_CorreoBitacora_Origen' AND parent_object_id = OBJECT_ID('dbo.AG_CorreoBitacora')
                      AND is_disabled = 0 AND is_not_trusted = 0
                      AND CHARINDEX('IdProceso', definition) > 0 AND CHARINDEX('IdActividad', definition) > 0);
DECLARE @Viola INT = (SELECT COUNT(*) FROM dbo.AG_CorreoBitacora WHERE IdActividad IS NULL AND IdProceso IS NULL);
INSERT INTO @R (Chequeo, Resultado, Detalle)
VALUES ('4. AG_CorreoBitacora: CHECK de origen', CASE WHEN @Ck = 1 AND @Viola = 0 THEN 'OK' ELSE 'FALLA' END,
        'check habilitado y confiable: ' + CONVERT(VARCHAR(10), @Ck) + ', filas sin origen: ' + CONVERT(VARCHAR(10), @Viola));
IF NOT (@Ck = 1 AND @Viola = 0) SET @Fallos += 1;

/* 5. integridad de los procesos */
DECLARE @Huerf INT = (SELECT COUNT(*) FROM dbo.AG_Actividad AS a
                       WHERE a.IdProcesoPaso IS NOT NULL
                         AND NOT EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso AS pp WHERE pp.IdProcesoPaso = a.IdProcesoPaso));
DECLARE @SinAct INT = (SELECT COUNT(*) FROM dbo.AG_ProcesoPaso AS pp
                        WHERE pp.Estado = 'EnCurso'
                          AND NOT EXISTS (SELECT 1 FROM dbo.AG_Actividad AS a WHERE a.IdProcesoPaso = pp.IdProcesoPaso));
DECLARE @DosAct INT = (SELECT COUNT(*) FROM (SELECT IdProcesoPaso FROM dbo.AG_Actividad
                        WHERE IdProcesoPaso IS NOT NULL GROUP BY IdProcesoPaso HAVING COUNT(*) > 1) AS x);
DECLARE @DosEnCurso INT = (SELECT COUNT(*) FROM (SELECT IdProceso FROM dbo.AG_ProcesoPaso
                        WHERE Estado = 'EnCurso' GROUP BY IdProceso HAVING COUNT(*) > 1) AS x);
INSERT INTO @R (Chequeo, Resultado, Detalle)
VALUES ('5. actividades de proceso huerfanas, pasos EnCurso sin actividad o con dos',
        CASE WHEN @Huerf + @SinAct + @DosAct + @DosEnCurso = 0 THEN 'OK' ELSE 'FALLA' END,
        'actividades con paso inexistente: ' + CONVERT(VARCHAR(10), @Huerf)
        + ', pasos EnCurso sin actividad: ' + CONVERT(VARCHAR(10), @SinAct)
        + ', pasos con dos actividades: ' + CONVERT(VARCHAR(10), @DosAct)
        + ', procesos con dos pasos EnCurso: ' + CONVERT(VARCHAR(10), @DosEnCurso));
IF @Huerf + @SinAct + @DosAct + @DosEnCurso <> 0 SET @Fallos += 1;

/* 6. menu esperado. PENDIENTE (cuenta como falla) mientras falte alguna hoja. */
DECLARE @IdAgenda INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'Agenda.aspx');
DECLARE @IdGrupo  INT = (SELECT Id_MenuPadre FROM dbo.MenuDos WHERE Id_Menu = @IdAgenda);
DECLARE @IdProc   INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'Procesos.aspx');
DECLARE @IdPlan   INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'ParametrizacionPlantillasProcesos.aspx');
DECLARE @Estado6 VARCHAR(10), @Detalle6 VARCHAR(300);
DECLARE @Dif TABLE (Diferencia VARCHAR(30), IdMenu INT, IdPerfil INT);
DECLARE @Todos TABLE (IdPerfil INT PRIMARY KEY);
DECLARE @E TABLE (IdMenu INT, IdPerfil INT);

IF @IdProc IS NULL OR @IdPlan IS NULL
BEGIN
    SET @Estado6 = 'PENDIENTE';
    SET @Detalle6 = 'falta correr 2026-10-23-procesos-menu.sql (hojas sin crear)';
END
ELSE
BEGIN
    INSERT INTO @Todos SELECT DISTINCT Id_Perfil FROM dbo.R_Usuarios
     WHERE Usuario_Estado = 'A' AND EstadoUsuario IS NULL AND ISNULL(IdCliente, 0) = 0 AND Id_Perfil IS NOT NULL;
    IF NOT EXISTS (SELECT 1 FROM @Todos WHERE IdPerfil = 18) INSERT INTO @Todos VALUES (18);

    INSERT INTO @E SELECT @IdProc, IdPerfil FROM @Todos;
    INSERT INTO @E VALUES (@IdPlan, 10), (@IdPlan, 18);

    ;WITH R AS (SELECT pm.id_Menu, pm.IdPerfil FROM dbo.PerfilMenu AS pm
                 WHERE pm.Estado = '0' AND pm.id_Menu IN (@IdProc, @IdPlan))
    INSERT INTO @Dif (Diferencia, IdMenu, IdPerfil)
    SELECT CASE WHEN r.id_Menu IS NULL THEN 'FALTA en el menu' ELSE 'SOBRA en el menu' END,
           ISNULL(e.IdMenu, r.id_Menu), ISNULL(e.IdPerfil, r.IdPerfil)
      FROM @E AS e FULL JOIN R AS r ON r.id_Menu = e.IdMenu AND r.IdPerfil = e.IdPerfil
     WHERE e.IdMenu IS NULL OR r.id_Menu IS NULL;

    /* el grupo tiene que ser visible para cada perfil que ve una hoja */
    INSERT INTO @Dif (Diferencia, IdMenu, IdPerfil)
    SELECT DISTINCT 'GRUPO no visible', @IdGrupo, e.IdPerfil
      FROM @E AS e
     WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu AS pm
                        WHERE pm.id_Menu = @IdGrupo AND pm.IdPerfil = e.IdPerfil AND pm.Estado = '0');

    /* las dos hojas cuelgan del grupo "Equipo y agenda" */
    IF (SELECT COUNT(*) FROM dbo.MenuDos WHERE Id_Menu IN (@IdProc, @IdPlan) AND Id_MenuPadre = @IdGrupo) <> 2
        INSERT INTO @Dif VALUES ('hoja fuera del grupo', @IdProc, NULL);

    /* SOBRA en el menu es informativo: un perfil que perdio su ultimo usuario activo conserva legitimamente su fila */
    SET @Estado6 = CASE WHEN EXISTS (SELECT 1 FROM @Dif WHERE Diferencia <> 'SOBRA en el menu') THEN 'FALLA' ELSE 'OK' END;
    SET @Detalle6 = 'diferencias: ' + CONVERT(VARCHAR(10), (SELECT COUNT(*) FROM @Dif WHERE Diferencia <> 'SOBRA en el menu'))
                  + '; informativas (SOBRA): ' + CONVERT(VARCHAR(10), (SELECT COUNT(*) FROM @Dif WHERE Diferencia = 'SOBRA en el menu'));
END
INSERT INTO @R (Chequeo, Resultado, Detalle)
VALUES ('6. menu: Procesos (todo perfil activo y 18), Plantillas (10 y 18)', @Estado6, @Detalle6);
IF @Estado6 <> 'OK' SET @Fallos += 1;

SELECT Chequeo, Resultado, Detalle FROM @R ORDER BY N;
SELECT Diferencia, IdMenu, IdPerfil FROM @Dif ORDER BY IdMenu, IdPerfil;

SELECT Veredicto = CASE WHEN @Fallos = 0 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA' END, Fallos = @Fallos;
