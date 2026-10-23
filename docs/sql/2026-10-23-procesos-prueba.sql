/* ============================================================================
   Procesos entre areas (Entrega 2) - prueba del esquema (se deshace sola)
   ReporTarea  |  2026-10-23
   Correr UNA vez, despues de 2026-10-23-procesos-esquema.sql.
   Todo va dentro de BEGIN TRAN ... ROLLBACK: no deja filas. Lo unico que no
   vuelve atras son los contadores IDENTITY de las tablas AG_* (el proximo
   Id puede saltar algunos numeros); no afecta a nada.
   Imprime PASS / FAIL por caso y termina en VEREDICTO: OK o FALLA. Si un
   procedimiento falla con error, XACT_ABORT deshace todo y el lote se corta:
   el ultimo lote lo detecta y tambien es FALLA.
   Usa areas, personas, plantillas y procesos ficticios (ZZ...). De los datos
   reales solo cuenta filas, para comprobar al final que no quedo nada.

   Dos partes:
     R  Regresion de la Entrega 1: una actividad SIN proceso -guardar, editar,
        marcar, desmarcar, completar, anular, correo- deja exactamente las
        filas escritas abajo como "esperado", que son las que deja la version
        anterior de los procedimientos (comprobado corriendo esta misma parte
        contra la version anterior en una base local). Las lineas marcadas
        "solo E2" comprueban lo unico que se agrega: la fila de respuesta de
        Marcar* (0 / NULL / NULL) y las columnas nuevas en NULL.
     P  Procesos: plantilla de 3 pasos, iniciar, avanzar, -8, -9, reasignar,
        cambiar, cancelar, terminar.
   Sin tildes a proposito.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* Foto de cada actividad, en texto: A = la actividad, E = sus entregables.
   Las fechas de auditoria salen como F (tiene) o - (NULL). Procedimiento
   temporal: vive en tempdb y desaparece al cerrar la sesion. */
IF OBJECT_ID('tempdb..#ZZFoto') IS NOT NULL DROP PROCEDURE #ZZFoto;
GO
CREATE PROCEDURE #ZZFoto @IdActividad INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT V = CONCAT('A|', a.Titulo, '|', a.Cod_Responsable, '|', a.Cod_AsignadoPor, '|',
                      CONVERT(CHAR(8), a.FechaEntrega, 112), '|', CONVERT(INT, a.CompletadaManual), '|',
                      IIF(a.FechaCompletada IS NULL, '-', 'F'), '|', CONVERT(INT, a.Anulada), '|',
                      a.Usuario_Registro, '|', ISNULL(a.Usuario_Modificacion, '-'), '|',
                      IIF(a.Fecha_Modificacion IS NULL, '-', 'F'))
      FROM dbo.AG_Actividad AS a WHERE a.IdActividad = @IdActividad
    UNION ALL
    SELECT CONCAT('E|', e.Orden, '|', e.Descripcion, '|', CONVERT(INT, e.Hecho), '|', ISNULL(e.Cod_Hecho, '-'), '|',
                  IIF(e.Fecha_Hecho IS NULL, '-', 'F'), '|', e.Usuario_Registro, '|',
                  ISNULL(e.Usuario_Modificacion, '-'), '|', IIF(e.Fecha_Modificacion IS NULL, '-', 'F'))
      FROM dbo.AG_Entregable AS e WHERE e.IdActividad = @IdActividad;
END
GO

/* Conteos antes (fuera de la transaccion, en tempdb), para comprobar al final
   que no quedo nada; y la marca de que el lote principal llego al final. */
IF OBJECT_ID('tempdb..#ZZAntes') IS NOT NULL DROP TABLE #ZZAntes;
IF OBJECT_ID('tempdb..#ZZFin') IS NOT NULL DROP TABLE #ZZFin;
CREATE TABLE #ZZAntes (Tabla SYSNAME PRIMARY KEY, Filas INT NOT NULL);
CREATE TABLE #ZZFin (Veredicto VARCHAR(40) NOT NULL);
INSERT INTO #ZZAntes (Tabla, Filas) SELECT 'AG_Area', COUNT(*) FROM dbo.AG_Area;
INSERT INTO #ZZAntes (Tabla, Filas) SELECT 'AG_PersonaArea', COUNT(*) FROM dbo.AG_PersonaArea;
INSERT INTO #ZZAntes (Tabla, Filas) SELECT 'AG_Actividad', COUNT(*) FROM dbo.AG_Actividad;
INSERT INTO #ZZAntes (Tabla, Filas) SELECT 'AG_Entregable', COUNT(*) FROM dbo.AG_Entregable;
INSERT INTO #ZZAntes (Tabla, Filas) SELECT 'AG_CorreoBitacora', COUNT(*) FROM dbo.AG_CorreoBitacora;
INSERT INTO #ZZAntes (Tabla, Filas) SELECT 'AG_Plantilla', COUNT(*) FROM dbo.AG_Plantilla; -- solo E2
INSERT INTO #ZZAntes (Tabla, Filas) SELECT 'AG_PlantillaPaso', COUNT(*) FROM dbo.AG_PlantillaPaso; -- solo E2
INSERT INTO #ZZAntes (Tabla, Filas) SELECT 'AG_Proceso', COUNT(*) FROM dbo.AG_Proceso; -- solo E2
INSERT INTO #ZZAntes (Tabla, Filas) SELECT 'AG_ProcesoPaso', COUNT(*) FROM dbo.AG_ProcesoPaso; -- solo E2
GO

/* ============================================================ lote principal */
DECLARE @R TABLE (N INT IDENTITY(1,1) PRIMARY KEY, Caso VARCHAR(120) NOT NULL, Ok BIT NOT NULL);
DECLARE @Diag TABLE (N INT IDENTITY(1,1) PRIMARY KEY, Caso VARCHAR(120), Lado VARCHAR(10), V NVARCHAR(600));
DECLARE @S TABLE (V NVARCHAR(600));          -- foto real
DECLARE @X TABLE (V NVARCHAR(600));          -- foto esperada
DECLARE @M TABLE (Respuestas INT, IdProcesoPasoActivado INT, IdProcesoTerminado INT);
DECLARE @G TABLE (Id INT);
DECLARE @Ok BIT, @Caso VARCHAR(120), @Id INT, @Err INT, @Estado INT;
DECLARE @A1 INT, @A2 INT, @E1 INT, @E2 INT, @E3 INT, @Xml XML;

BEGIN TRANSACTION;

/* ------------------------------------------------ R. regresion Entrega 1 */
--<REGRESION>
/* R1 guardar nueva con 2 entregables */
DELETE FROM @G;
INSERT INTO @G EXEC dbo.Sp_RTA_AgendaGuardarActividad @IdActividad = 0, @Titulo = N'  Reg titulo ', @CodResponsable = 'ZZREG',
     @CodAsignadoPor = 'ZZREGJ', @FechaEntrega = '20301104',
     @Entregables = N'<es><e id="0" o="1">r1</e><e id="0" o="2">r2</e></es>', @Usuario = 'ZZREGJ';
SELECT @A1 = Id FROM @G;
SELECT @E1 = IdEntregable FROM dbo.AG_Entregable WHERE IdActividad = @A1 AND Orden = 1;
SELECT @E2 = IdEntregable FROM dbo.AG_Entregable WHERE IdActividad = @A1 AND Orden = 2;
INSERT INTO @R (Caso, Ok) VALUES ('R1 guardar nueva devuelve el id', IIF(@A1 > 0 AND (SELECT COUNT(*) FROM @G) = 1, 1, 0));
SET @Caso = 'R1 guardar nueva: filas';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Reg titulo|ZZREG|ZZREGJ|20301104|0|-|0|ZZREGJ|-|-'),
    (N'E|1|r1|0|-|-|ZZREGJ|-|-'),
    (N'E|2|r2|0|-|-|ZZREGJ|-|-');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A1;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;
INSERT INTO @R (Caso, Ok) VALUES ('R1 IdProcesoPaso NULL', IIF((SELECT IdProcesoPaso FROM dbo.AG_Actividad WHERE IdActividad = @A1) IS NULL, 1, 0)); -- solo E2

/* R2 editar: titulo, fecha; conserva r1 con texto nuevo, quita r2, agrega r3 */
SET @Xml = CONVERT(XML, N'<es><e id="' + CONVERT(NVARCHAR(10), @E1) + N'" o="1">r1b</e><e id="0" o="2">r3</e></es>');
DELETE FROM @G;
INSERT INTO @G EXEC dbo.Sp_RTA_AgendaGuardarActividad @IdActividad = @A1, @Titulo = N' Reg titulo 2 ', @CodResponsable = 'IGNORADO',
     @CodAsignadoPor = 'IGNORADO', @FechaEntrega = '20301105', @Entregables = @Xml, @Usuario = 'ZZREGJ2';
SELECT @E3 = IdEntregable FROM dbo.AG_Entregable WHERE IdActividad = @A1 AND Descripcion = N'r3';
INSERT INTO @R (Caso, Ok) VALUES ('R2 editar devuelve el mismo id', IIF((SELECT Id FROM @G) = @A1, 1, 0));
SET @Caso = 'R2 editar: filas';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Reg titulo 2|ZZREG|ZZREGJ|20301105|0|-|0|ZZREGJ|ZZREGJ2|F'),
    (N'E|1|r1b|0|-|-|ZZREGJ|ZZREGJ2|F'),
    (N'E|2|r3|0|-|-|ZZREGJ2|-|-');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A1;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;
INSERT INTO @R (Caso, Ok) VALUES ('R2 r2 borrado', IIF(NOT EXISTS (SELECT 1 FROM dbo.AG_Entregable WHERE IdEntregable = @E2), 1, 0));

/* R3 marcar r1b */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @E1, @Hecho = 1, @Usuario = 'ZZREG';
INSERT INTO @R (Caso, Ok) VALUES ('R3 fila de respuesta 0/NULL/NULL', IIF((SELECT COUNT(*) FROM @M) = 1 AND EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL), 1, 0)); -- solo E2
SET @Caso = 'R3 marcar un entregable: filas';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Reg titulo 2|ZZREG|ZZREGJ|20301105|0|-|0|ZZREGJ|ZZREGJ2|F'),
    (N'E|1|r1b|1|ZZREG|F|ZZREGJ|ZZREG|F'),
    (N'E|2|r3|0|-|-|ZZREGJ2|-|-');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A1;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* R4 marcar r3: la actividad queda completa */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @E3, @Hecho = 1, @Usuario = 'ZZREG';
INSERT INTO @R (Caso, Ok) VALUES ('R4 fila de respuesta 0/NULL/NULL', IIF((SELECT COUNT(*) FROM @M) = 1 AND EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL), 1, 0)); -- solo E2
SET @Caso = 'R4 marcar el ultimo: completa';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Reg titulo 2|ZZREG|ZZREGJ|20301105|0|F|0|ZZREGJ|ZZREGJ2|F'),
    (N'E|1|r1b|1|ZZREG|F|ZZREGJ|ZZREG|F'),
    (N'E|2|r3|1|ZZREG|F|ZZREGJ2|ZZREG|F');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A1;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* R5 desmarcar r3: deja de estar completa */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @E3, @Hecho = 0, @Usuario = 'ZZREG';
INSERT INTO @R (Caso, Ok) VALUES ('R5 fila de respuesta 0/NULL/NULL', IIF((SELECT COUNT(*) FROM @M) = 1 AND EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL), 1, 0)); -- solo E2
SET @Caso = 'R5 desmarcar: filas';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Reg titulo 2|ZZREG|ZZREGJ|20301105|0|-|0|ZZREGJ|ZZREGJ2|F'),
    (N'E|1|r1b|1|ZZREG|F|ZZREGJ|ZZREG|F'),
    (N'E|2|r3|0|-|-|ZZREGJ2|ZZREG|F');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A1;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* R6 "completa" sobre una actividad con entregables: no cambia nada */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarCompleta @IdActividad = @A1, @Completa = 1, @Usuario = 'ZZREG';
INSERT INTO @R (Caso, Ok) VALUES ('R6 fila de respuesta 0/NULL/NULL', IIF((SELECT COUNT(*) FROM @M) = 1 AND EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL), 1, 0)); -- solo E2
SET @Caso = 'R6 completa con entregables: sin cambios';
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A1;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* R7 actividad sin entregables: completa y descompleta */
DELETE FROM @G;
INSERT INTO @G EXEC dbo.Sp_RTA_AgendaGuardarActividad @IdActividad = 0, @Titulo = N'Reg sin entregables', @CodResponsable = 'ZZREG',
     @CodAsignadoPor = 'ZZREG', @FechaEntrega = '20301106', @Entregables = N'<es/>', @Usuario = 'ZZREG';
SELECT @A2 = Id FROM @G;
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarCompleta @IdActividad = @A2, @Completa = 1, @Usuario = 'ZZREGJ';
INSERT INTO @R (Caso, Ok) VALUES ('R7 fila de respuesta 0/NULL/NULL (completa)', IIF((SELECT COUNT(*) FROM @M) = 1 AND EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL), 1, 0)); -- solo E2
SET @Caso = 'R7 completa sin entregables';
DELETE FROM @X; INSERT INTO @X VALUES (N'A|Reg sin entregables|ZZREG|ZZREG|20301106|1|F|0|ZZREG|ZZREGJ|F');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A2;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarCompleta @IdActividad = @A2, @Completa = 0, @Usuario = 'ZZREG';
INSERT INTO @R (Caso, Ok) VALUES ('R7 fila de respuesta 0/NULL/NULL (descompleta)', IIF((SELECT COUNT(*) FROM @M) = 1 AND EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL), 1, 0)); -- solo E2
SET @Caso = 'R7 descompleta sin entregables';
DELETE FROM @X; INSERT INTO @X VALUES (N'A|Reg sin entregables|ZZREG|ZZREG|20301106|0|-|0|ZZREG|ZZREG|F');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A2;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* R8 anular */
EXEC dbo.Sp_RTA_AgendaAnular @IdActividad = @A1, @Usuario = 'ZZREGJ';
SET @Caso = 'R8 anular: filas';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Reg titulo 2|ZZREG|ZZREGJ|20301105|0|-|1|ZZREGJ|ZZREGJ|F'),
    (N'E|1|r1b|1|ZZREG|F|ZZREGJ|ZZREG|F'),
    (N'E|2|r3|0|-|-|ZZREGJ2|ZZREG|F');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A1;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* R9 guardar una anulada: el titulo no cambia pero los entregables si (y el
   recalculo la marca completa): es el comportamiento de la Entrega 1 y se
   conserva tal cual. */
SET @Xml = CONVERT(XML, N'<es><e id="' + CONVERT(NVARCHAR(10), @E1) + N'" o="1">r1c</e></es>');
DELETE FROM @G;
INSERT INTO @G EXEC dbo.Sp_RTA_AgendaGuardarActividad @IdActividad = @A1, @Titulo = N'Otro', @CodResponsable = 'ZZREG',
     @CodAsignadoPor = 'ZZREGJ', @FechaEntrega = '20301110', @Entregables = @Xml, @Usuario = 'ZZREGJ3';
INSERT INTO @R (Caso, Ok) VALUES ('R9 guardar anulada devuelve el id', IIF((SELECT Id FROM @G) = @A1, 1, 0));
SET @Caso = 'R9 guardar anulada: filas';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Reg titulo 2|ZZREG|ZZREGJ|20301105|0|F|1|ZZREGJ|ZZREGJ|F'),
    (N'E|1|r1c|1|ZZREG|F|ZZREGJ|ZZREGJ3|F');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A1;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* R10 marcar en una anulada: la Entrega 1 lo deja pasar; se conserva */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @E1, @Hecho = 0, @Usuario = 'ZZREG';
INSERT INTO @R (Caso, Ok) VALUES ('R10 fila de respuesta 0/NULL/NULL', IIF((SELECT COUNT(*) FROM @M) = 1 AND EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL), 1, 0)); -- solo E2
SET @Caso = 'R10 desmarcar en anulada: filas';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Reg titulo 2|ZZREG|ZZREGJ|20301105|0|-|1|ZZREGJ|ZZREGJ|F'),
    (N'E|1|r1c|0|-|-|ZZREGJ|ZZREG|F');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @A1;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* R11 correo con la firma de la Entrega 1 (5 parametros) */
EXEC dbo.Sp_RTA_AgendaRegistrarCorreo @IdActividad = @A2, @Destinatario = 'zz@prueba', @Asunto = N'Asunto ZZ', @Enviado = 0, @Error = N'error ZZ';
INSERT INTO @R (Caso, Ok) VALUES ('R11 correo registrado como antes', IIF((SELECT COUNT(*) FROM dbo.AG_CorreoBitacora WHERE IdActividad = @A2 AND Destinatario = 'zz@prueba' AND Asunto = N'Asunto ZZ' AND Enviado = 0 AND Error = N'error ZZ') = 1, 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('R11 correo con IdProceso NULL', IIF(EXISTS (SELECT 1 FROM dbo.AG_CorreoBitacora WHERE IdActividad = @A2 AND IdProceso IS NULL), 1, 0)); -- solo E2

/* R12 marcar un entregable que no existe: nada */
SELECT @Id = COUNT(*) FROM dbo.AG_Entregable;
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = -12345, @Hecho = 1, @Usuario = 'ZZREG';
INSERT INTO @R (Caso, Ok) VALUES ('R12 entregable inexistente: nada cambia', IIF((SELECT COUNT(*) FROM dbo.AG_Entregable WHERE Usuario_Modificacion = 'ZZREG' AND IdEntregable = -12345) = 0 AND (SELECT COUNT(*) FROM dbo.AG_Entregable) = @Id, 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('R12 fila de respuesta 0/NULL/NULL', IIF((SELECT COUNT(*) FROM @M) = 1 AND EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL), 1, 0)); -- solo E2
--</REGRESION>

/* ------------------------------------------------------------ P. procesos */
--<PROCESOS>
DECLARE @Ar1 INT, @Ar2 INT, @Ar3 INT, @Pl INT, @Pr INT, @Pr2 INT, @Ps1 INT, @Ps2 INT, @Ps3 INT,
        @Ac1 INT, @Ac2 INT, @Ac3 INT, @EUno INT, @EDos INT, @EA INT, @EB INT, @Q1 INT, @QAc INT, @QUno INT, @QDos INT;
DECLARE @Rs TABLE (Resultado INT);
DECLARE @I TABLE (IdProceso INT, IdProcesoPasoActivado INT);
DECLARE @F TABLE (Fecha DATE);

INSERT INTO dbo.AG_Area (Nombre, Color, Activa, EsSinArea, Usuario_Registro) VALUES (N'ZZ Prueba procesos A1', '#101010', 1, 0, 'ZZ');
SET @Ar1 = CONVERT(INT, SCOPE_IDENTITY());
INSERT INTO dbo.AG_Area (Nombre, Color, Activa, EsSinArea, Usuario_Registro) VALUES (N'ZZ Prueba procesos A2', '#202020', 1, 0, 'ZZ');
SET @Ar2 = CONVERT(INT, SCOPE_IDENTITY());
INSERT INTO dbo.AG_Area (Nombre, Color, Activa, EsSinArea, Usuario_Registro) VALUES (N'ZZ Prueba procesos A3', '#303030', 1, 0, 'ZZ');
SET @Ar3 = CONVERT(INT, SCOPE_IDENTITY());

/* P1 plantilla de 3 pasos */
SET @Xml = CONVERT(XML,
      N'<ps><p o="1" a="' + CONVERT(NVARCHAR(10), @Ar1) + N'" t="Paso uno" d="3" e="Uno&#xA;Dos"/>'
    + N'<p o="2" a="' + CONVERT(NVARCHAR(10), @Ar2) + N'" t="Paso dos" d="5" e="A&#xD;&#xA;  B  &#xA;&#xA;"/>'
    + N'<p o="3" a="' + CONVERT(NVARCHAR(10), @Ar3) + N'" t="Paso tres" d="2"/></ps>');
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaGuardarPlantilla @IdPlantilla = 0, @Nombre = N' ZZ Plantilla prueba ', @Descripcion = N'desc',
     @Activa = 1, @Pasos = @Xml, @Usuario = 'ZZADM';
SELECT @Pl = Resultado FROM @Rs;
INSERT INTO @R (Caso, Ok) VALUES ('P1 guardar plantilla nueva devuelve el id', IIF(@Pl > 0, 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P1 plantilla con 3 pasos en orden', IIF((SELECT COUNT(*) FROM dbo.AG_PlantillaPaso WHERE IdPlantilla = @Pl) = 3 AND EXISTS (SELECT 1 FROM dbo.AG_Plantilla WHERE IdPlantilla = @Pl AND Nombre = N'ZZ Plantilla prueba' AND Activa = 1) AND EXISTS (SELECT 1 FROM dbo.AG_PlantillaPaso WHERE IdPlantilla = @Pl AND Orden = 3 AND Titulo = N'Paso tres' AND PlazoDias = 2 AND Entregables IS NULL AND IdArea = @Ar3), 1, 0));

DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaGuardarPlantilla @IdPlantilla = 0, @Nombre = N'ZZ Plantilla prueba', @Descripcion = NULL,
     @Activa = 1, @Pasos = @Xml, @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P1 nombre repetido -1', IIF((SELECT Resultado FROM @Rs) = -1, 1, 0));

DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaGuardarPlantilla @IdPlantilla = 0, @Nombre = N'ZZ Plantilla mala', @Descripcion = NULL, @Activa = 1,
     @Pasos = N'<ps><p o="1" a="1" t="x" d="3"/><p o="3" a="1" t="y" d="3"/></ps>', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P1 orden no consecutivo -2', IIF((SELECT Resultado FROM @Rs) = -2, 1, 0));

DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaGuardarPlantilla @IdPlantilla = 0, @Nombre = N'ZZ Plantilla mala', @Descripcion = NULL, @Activa = 1,
     @Pasos = N'<ps><p o="1" a="1" t="x" d="61"/></ps>', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P1 plazo 61 -2', IIF((SELECT Resultado FROM @Rs) = -2, 1, 0));

/* P2 iniciar: paso 1 EnCurso con actividad; 2 y 3 Pendiente sin actividad */
SET @Xml = CONVERT(XML,
      N'<ps><p o="1" a="' + CONVERT(NVARCHAR(10), @Ar1) + N'" t="Paso uno ajustado" d="3" e="Uno&#xA;Dos" r="ZZR1"/>'
    + N'<p o="2" a="' + CONVERT(NVARCHAR(10), @Ar2) + N'" t="Paso dos" d="5" e="A&#xD;&#xA;  B  &#xA;&#xA;" r="ZZR2"/>'
    + N'<p o="3" a="' + CONVERT(NVARCHAR(10), @Ar3) + N'" t="Paso tres" d="2" r="ZZR3"/></ps>');
DELETE FROM @I;
INSERT INTO @I EXEC dbo.Sp_RTA_AgendaIniciarProceso @Nombre = N'ZZ Proceso uno', @IdPlantilla = @Pl, @Pasos = @Xml,
     @FechaEntregaPrimero = '20301111', @Usuario = 'ZZINI';
SELECT @Pr = IdProceso FROM @I;
SELECT @Ps1 = IdProcesoPaso FROM dbo.AG_ProcesoPaso WHERE IdProceso = @Pr AND Orden = 1;
SELECT @Ps2 = IdProcesoPaso FROM dbo.AG_ProcesoPaso WHERE IdProceso = @Pr AND Orden = 2;
SELECT @Ps3 = IdProcesoPaso FROM dbo.AG_ProcesoPaso WHERE IdProceso = @Pr AND Orden = 3;
SELECT @Ac1 = IdActividad FROM dbo.AG_Actividad WHERE IdProcesoPaso = @Ps1;
SELECT @EUno = IdEntregable FROM dbo.AG_Entregable WHERE IdActividad = @Ac1 AND Orden = 1;
SELECT @EDos = IdEntregable FROM dbo.AG_Entregable WHERE IdActividad = @Ac1 AND Orden = 2;
INSERT INTO @R (Caso, Ok) VALUES ('P2 iniciar devuelve proceso y paso 1', IIF(@Pr > 0 AND (SELECT IdProcesoPasoActivado FROM @I) = @Ps1, 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P2 proceso EnCurso, iniciado por ZZINI', IIF(EXISTS (SELECT 1 FROM dbo.AG_Proceso WHERE IdProceso = @Pr AND Estado = 'EnCurso' AND Cod_IniciadoPor = 'ZZINI' AND IdPlantilla = @Pl AND FechaFin IS NULL), 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P2 paso 1 EnCurso con FechaActivacion', IIF(EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps1 AND Estado = 'EnCurso' AND FechaActivacion IS NOT NULL AND Titulo = N'Paso uno ajustado' AND Cod_Responsable = 'ZZR1'), 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P2 pasos 2 y 3 Pendiente sin actividad', IIF((SELECT COUNT(*) FROM dbo.AG_ProcesoPaso WHERE IdProceso = @Pr AND Orden IN (2, 3) AND Estado = 'Pendiente' AND FechaActivacion IS NULL) = 2 AND NOT EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdProcesoPaso IN (@Ps2, @Ps3)), 1, 0));
SET @Caso = 'P2 actividad del paso 1: filas';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Paso uno ajustado|ZZR1|ZZINI|20301111|0|-|0|ZZINI|-|-'),
    (N'E|1|Uno|0|-|-|ZZINI|-|-'),
    (N'E|2|Dos|0|-|-|ZZINI|-|-');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @Ac1;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* P3 editar la plantilla no toca el proceso ya iniciado */
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaGuardarPlantilla @IdPlantilla = @Pl, @Nombre = N'ZZ Plantilla prueba', @Descripcion = NULL, @Activa = 1,
     @Pasos = @Xml, @Usuario = 'ZZADM';
DELETE FROM @Rs;
SET @Xml = CONVERT(XML, N'<ps><p o="1" a="' + CONVERT(NVARCHAR(10), @Ar1) + N'" t="Solo uno" d="9"/></ps>');
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaGuardarPlantilla @IdPlantilla = @Pl, @Nombre = N'ZZ Plantilla prueba', @Descripcion = NULL, @Activa = 1,
     @Pasos = @Xml, @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P3 editar plantilla reemplaza sus pasos', IIF((SELECT Resultado FROM @Rs) = @Pl AND (SELECT COUNT(*) FROM dbo.AG_PlantillaPaso WHERE IdPlantilla = @Pl) = 1, 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P3 el proceso conserva sus 3 pasos', IIF((SELECT COUNT(*) FROM dbo.AG_ProcesoPaso WHERE IdProceso = @Pr) = 3 AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps2 AND Titulo = N'Paso dos'), 1, 0));
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaGuardarPlantilla @IdPlantilla = -77, @Nombre = N'ZZ Plantilla inexistente', @Descripcion = NULL, @Activa = 1,
     @Pasos = @Xml, @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P3 plantilla inexistente -3', IIF((SELECT Resultado FROM @Rs) = -3, 1, 0));

/* P4 marcar "Uno" con fecha: no completa, no avanza */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @EUno, @Hecho = 1, @Usuario = 'ZZR1', @FechaEntregaSiguiente = '20301118';
INSERT INTO @R (Caso, Ok) VALUES ('P4 marcar sin completar: 0 sin avance', IIF(EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL) AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps1 AND Estado = 'EnCurso'), 1, 0));

/* P5 marcar el ultimo ("Dos") sin fecha siguiente: -8, nada cambia */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @EDos, @Hecho = 1, @Usuario = 'ZZR1';
INSERT INTO @R (Caso, Ok) VALUES ('P5 completar sin fecha siguiente: -8', IIF(EXISTS (SELECT 1 FROM @M WHERE Respuestas = -8 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL), 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P5 -8 no marco ni avanzo', IIF(EXISTS (SELECT 1 FROM dbo.AG_Entregable WHERE IdEntregable = @EDos AND Hecho = 0 AND Cod_Hecho IS NULL AND Usuario_Modificacion IS NULL) AND EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdActividad = @Ac1 AND FechaCompletada IS NULL) AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps1 AND Estado = 'EnCurso') AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps2 AND Estado = 'Pendiente') AND NOT EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdProcesoPaso = @Ps2), 1, 0));

/* P6 marcar el ultimo con fecha: paso 1 Completo, paso 2 EnCurso con su fecha */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @EDos, @Hecho = 1, @Usuario = 'ZZR1', @FechaEntregaSiguiente = '20301118';
SELECT @Ac2 = IdActividad FROM dbo.AG_Actividad WHERE IdProcesoPaso = @Ps2;
SELECT @EA = IdEntregable FROM dbo.AG_Entregable WHERE IdActividad = @Ac2 AND Orden = 1;
SELECT @EB = IdEntregable FROM dbo.AG_Entregable WHERE IdActividad = @Ac2 AND Orden = 2;
INSERT INTO @R (Caso, Ok) VALUES ('P6 devuelve 0 y el paso 2 activado', IIF(EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado = @Ps2 AND IdProcesoTerminado IS NULL), 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P6 paso 1 Completo con FechaCompletado', IIF(EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps1 AND Estado = 'Completo' AND FechaCompletado IS NOT NULL), 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P6 paso 2 EnCurso, paso 3 Pendiente', IIF(EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps2 AND Estado = 'EnCurso' AND FechaActivacion IS NOT NULL) AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps3 AND Estado = 'Pendiente') AND (SELECT COUNT(*) FROM dbo.AG_Actividad WHERE IdProcesoPaso = @Ps2) = 1, 1, 0));
SET @Caso = 'P6 actividad del paso 2: fecha, asignado por el iniciador, 2 entregables';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Paso dos|ZZR2|ZZINI|20301118|0|-|0|ZZR1|-|-'),
    (N'E|1|A|0|-|-|ZZR1|-|-'),
    (N'E|2|B|0|-|-|ZZR1|-|-');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @Ac2;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* P7 paso completo: desmarcar -9, completa -9, sin cambios */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @EDos, @Hecho = 0, @Usuario = 'ZZR1', @FechaEntregaSiguiente = '20301118';
INSERT INTO @R (Caso, Ok) VALUES ('P7 desmarcar en paso Completo: -9', IIF(EXISTS (SELECT 1 FROM @M WHERE Respuestas = -9 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado IS NULL), 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P7 -9 no desmarco', IIF(EXISTS (SELECT 1 FROM dbo.AG_Entregable WHERE IdEntregable = @EDos AND Hecho = 1) AND EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdActividad = @Ac1 AND FechaCompletada IS NOT NULL), 1, 0));
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarCompleta @IdActividad = @Ac1, @Completa = 0, @Usuario = 'ZZR1';
INSERT INTO @R (Caso, Ok) VALUES ('P7 completa en paso Completo: -9', IIF(EXISTS (SELECT 1 FROM @M WHERE Respuestas = -9), 1, 0));

/* P8 guardar la actividad de un paso: -9, sin cambios */
DELETE FROM @G;
INSERT INTO @G EXEC dbo.Sp_RTA_AgendaGuardarActividad @IdActividad = @Ac2, @Titulo = N'Cambiado', @CodResponsable = 'ZZR2',
     @CodAsignadoPor = 'ZZINI', @FechaEntrega = '20301130', @Entregables = N'<es/>', @Usuario = 'ZZR2';
INSERT INTO @R (Caso, Ok) VALUES ('P8 guardar actividad de paso: -9', IIF((SELECT Id FROM @G) = -9, 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P8 -9 no toco nada', IIF(EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdActividad = @Ac2 AND Titulo = N'Paso dos' AND FechaEntrega = '20301118' AND Usuario_Modificacion IS NULL) AND (SELECT COUNT(*) FROM dbo.AG_Entregable WHERE IdActividad = @Ac2) = 2, 1, 0));

/* P9 marcar "A" del paso 2 (no completa) */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @EA, @Hecho = 1, @Usuario = 'ZZR2', @FechaEntregaSiguiente = '20301125';
INSERT INTO @R (Caso, Ok) VALUES ('P9 marcar A: 0 sin avance', IIF(EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL), 1, 0));

/* P10 reasignar */
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaReasignarPaso @IdProcesoPaso = @Ps2, @EstadoEsperado = 'Pendiente', @CodResponsable = 'ZZR2B', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P10 reasignar con estado viejo: -1', IIF((SELECT Resultado FROM @Rs) = -1 AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps2 AND Cod_Responsable = 'ZZR2'), 1, 0));
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaReasignarPaso @IdProcesoPaso = @Ps1, @EstadoEsperado = 'Completo', @CodResponsable = 'ZZR2B', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P10 reasignar un paso Completo: -1', IIF((SELECT Resultado FROM @Rs) = -1, 1, 0));
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaReasignarPaso @IdProcesoPaso = @Ps2, @EstadoEsperado = 'EnCurso', @CodResponsable = ' ZZR2B ', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P10 reasignar en curso: 0', IIF((SELECT Resultado FROM @Rs) = 0 AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps2 AND Cod_Responsable = 'ZZR2B' AND Estado = 'EnCurso'), 1, 0));
SET @Caso = 'P10 la actividad cambia de responsable y conserva sus entregables';
DELETE FROM @X; INSERT INTO @X VALUES
    (N'A|Paso dos|ZZR2B|ZZINI|20301118|0|-|0|ZZR1|ZZADM|F'),
    (N'E|1|A|1|ZZR2|F|ZZR1|ZZR2|F'),
    (N'E|2|B|0|-|-|ZZR1|-|-');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @Ac2;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaReasignarPaso @IdProcesoPaso = @Ps3, @EstadoEsperado = 'Pendiente', @CodResponsable = 'ZZR3B', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P10 reasignar Pendiente: solo el responsable', IIF((SELECT Resultado FROM @Rs) = 0 AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps3 AND Cod_Responsable = 'ZZR3B' AND Estado = 'Pendiente') AND NOT EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdProcesoPaso = @Ps3), 1, 0));

/* P11 cambiar un paso Pendiente y la fecha del paso en curso */
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaCambiarPasoPendiente @IdProcesoPaso = @Ps3, @Titulo = N' Paso tres nuevo ', @PlazoDias = 7, @Entregables = N'', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P11 cambiar paso Pendiente: 0', IIF((SELECT Resultado FROM @Rs) = 0 AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps3 AND Titulo = N'Paso tres nuevo' AND PlazoDias = 7 AND Entregables IS NULL), 1, 0));
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaCambiarPasoPendiente @IdProcesoPaso = @Ps2, @Titulo = N'No', @PlazoDias = 7, @Entregables = NULL, @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P11 cambiar paso EnCurso como Pendiente: -1', IIF((SELECT Resultado FROM @Rs) = -1 AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps2 AND Titulo = N'Paso dos'), 1, 0));
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaCambiarPasoPendiente @IdProcesoPaso = @Ps3, @Titulo = N'X', @PlazoDias = 0, @Entregables = NULL, @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P11 plazo 0: -2', IIF((SELECT Resultado FROM @Rs) = -2, 1, 0));
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaCambiarFechaPaso @IdProcesoPaso = @Ps2, @Fecha = '20301120', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P11 cambiar fecha del paso EnCurso: 0', IIF((SELECT Resultado FROM @Rs) = 0 AND EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdActividad = @Ac2 AND FechaEntrega = '20301120'), 1, 0));
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaCambiarFechaPaso @IdProcesoPaso = @Ps3, @Fecha = '20301120', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P11 cambiar fecha de un Pendiente: -1', IIF((SELECT Resultado FROM @Rs) = -1, 1, 0));

/* P12 indice unico: una segunda actividad para el mismo paso se rechaza */
SET @Err = 0;
SET XACT_ABORT OFF;
BEGIN TRY
    INSERT INTO dbo.AG_Actividad (Titulo, Cod_Responsable, Cod_AsignadoPor, FechaEntrega, Usuario_Registro, IdProcesoPaso)
    VALUES (N'Duplicada', 'ZZR2', 'ZZINI', '20301120', 'ZZ', @Ps2);
END TRY
BEGIN CATCH
    SET @Err = ERROR_NUMBER();
END CATCH
SET @Estado = XACT_STATE();
SET XACT_ABORT ON;
INSERT INTO @R (Caso, Ok) VALUES ('P12 segunda actividad del mismo paso rechazada (2601)', IIF(@Err = 2601 AND @Estado = 1, 1, 0));

/* P13 completar el paso 2 (con fecha) activa el 3 con el titulo y responsable ajustados */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @EB, @Hecho = 1, @Usuario = 'ZZR2B', @FechaEntregaSiguiente = '20301125';
SELECT @Ac3 = IdActividad FROM dbo.AG_Actividad WHERE IdProcesoPaso = @Ps3;
INSERT INTO @R (Caso, Ok) VALUES ('P13 paso 3 activado', IIF(EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado = @Ps3 AND IdProcesoTerminado IS NULL) AND EXISTS (SELECT 1 FROM dbo.AG_ProcesoPaso WHERE IdProcesoPaso = @Ps2 AND Estado = 'Completo'), 1, 0));
SET @Caso = 'P13 actividad del paso 3 sin entregables';
DELETE FROM @X; INSERT INTO @X VALUES (N'A|Paso tres nuevo|ZZR3B|ZZINI|20301125|0|-|0|ZZR2B|-|-');
DELETE FROM @S; INSERT INTO @S EXEC #ZZFoto @Ac3;
SET @Ok = IIF(NOT EXISTS (SELECT V FROM @S EXCEPT SELECT V FROM @X) AND NOT EXISTS (SELECT V FROM @X EXCEPT SELECT V FROM @S) AND (SELECT COUNT(*) FROM @S) = (SELECT COUNT(*) FROM @X), 1, 0);
INSERT INTO @R (Caso, Ok) VALUES (@Caso, @Ok);
IF @Ok = 0 INSERT INTO @Diag (Caso, Lado, V) SELECT @Caso, 'real', V FROM @S UNION ALL SELECT @Caso, 'esperado', V FROM @X;

/* P14 terminar: "completa" del ultimo paso, sin fecha siguiente (no hace falta) */
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarCompleta @IdActividad = @Ac3, @Completa = 1, @Usuario = 'ZZR3B';
INSERT INTO @R (Caso, Ok) VALUES ('P14 devuelve IdProcesoTerminado', IIF(EXISTS (SELECT 1 FROM @M WHERE Respuestas = 0 AND IdProcesoPasoActivado IS NULL AND IdProcesoTerminado = @Pr), 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P14 proceso Terminado, 3 pasos Completo', IIF(EXISTS (SELECT 1 FROM dbo.AG_Proceso WHERE IdProceso = @Pr AND Estado = 'Terminado' AND FechaFin IS NOT NULL) AND (SELECT COUNT(*) FROM dbo.AG_ProcesoPaso WHERE IdProceso = @Pr AND Estado = 'Completo' AND FechaCompletado IS NOT NULL) = 3, 1, 0));
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarCompleta @IdActividad = @Ac3, @Completa = 0, @Usuario = 'ZZR3B';
INSERT INTO @R (Caso, Ok) VALUES ('P14 descompletar el ultimo paso: -9', IIF(EXISTS (SELECT 1 FROM @M WHERE Respuestas = -9) AND EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdActividad = @Ac3 AND CompletadaManual = 1 AND FechaCompletada IS NOT NULL), 1, 0));
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaCancelarProceso @IdProceso = @Pr, @Motivo = N'tarde', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P14 cancelar un proceso Terminado: -1', IIF((SELECT Resultado FROM @Rs) = -1 AND EXISTS (SELECT 1 FROM dbo.AG_Proceso WHERE IdProceso = @Pr AND Estado = 'Terminado'), 1, 0));

/* P15 correo de proceso terminado: sin actividad */
EXEC dbo.Sp_RTA_AgendaRegistrarCorreo @IdActividad = NULL, @Destinatario = 'zzini@prueba', @Asunto = N'Proceso terminado ZZ', @Enviado = 1, @Error = NULL, @IdProceso = @Pr;
INSERT INTO @R (Caso, Ok) VALUES ('P15 correo con IdProceso y sin actividad', IIF(EXISTS (SELECT 1 FROM dbo.AG_CorreoBitacora WHERE IdProceso = @Pr AND IdActividad IS NULL AND Asunto = N'Proceso terminado ZZ'), 1, 0));
SET @Err = 0;
SET XACT_ABORT OFF;
BEGIN TRY
    EXEC dbo.Sp_RTA_AgendaRegistrarCorreo @IdActividad = NULL, @Destinatario = 'zz@prueba', @Asunto = N'Sin origen', @Enviado = 1, @Error = NULL;
END TRY
BEGIN CATCH
    SET @Err = ERROR_NUMBER();
END CATCH
SET @Estado = XACT_STATE();
SET XACT_ABORT ON;
INSERT INTO @R (Caso, Ok) VALUES ('P15 correo sin actividad ni proceso rechazado (547)', IIF(@Err = 547 AND @Estado = 1 AND NOT EXISTS (SELECT 1 FROM dbo.AG_CorreoBitacora WHERE Asunto = N'Sin origen'), 1, 0));

/* P16 cancelar a mitad: actividad anulada, entregables como historia */
SET @Xml = CONVERT(XML,
      N'<ps><p o="1" a="' + CONVERT(NVARCHAR(10), @Ar1) + N'" t="C uno" d="3" e="Uno&#xA;Dos" r="ZZR1"/>'
    + N'<p o="2" a="' + CONVERT(NVARCHAR(10), @Ar2) + N'" t="C dos" d="5" r="ZZR2"/></ps>');
DELETE FROM @I;
INSERT INTO @I EXEC dbo.Sp_RTA_AgendaIniciarProceso @Nombre = N'ZZ Proceso dos', @IdPlantilla = @Pl, @Pasos = @Xml,
     @FechaEntregaPrimero = '20301111', @Usuario = 'ZZINI2';
SELECT @Pr2 = IdProceso, @Q1 = IdProcesoPasoActivado FROM @I;
SELECT @QAc = IdActividad FROM dbo.AG_Actividad WHERE IdProcesoPaso = @Q1;
SELECT @QUno = IdEntregable FROM dbo.AG_Entregable WHERE IdActividad = @QAc AND Orden = 1;
SELECT @QDos = IdEntregable FROM dbo.AG_Entregable WHERE IdActividad = @QAc AND Orden = 2;
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @QUno, @Hecho = 1, @Usuario = 'ZZR1', @FechaEntregaSiguiente = '20301118';
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaCancelarProceso @IdProceso = @Pr2, @Motivo = N' Prueba de cancelacion ', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P16 cancelar: 0 y proceso Cancelado con motivo', IIF((SELECT Resultado FROM @Rs) = 0 AND EXISTS (SELECT 1 FROM dbo.AG_Proceso WHERE IdProceso = @Pr2 AND Estado = 'Cancelado' AND FechaFin IS NOT NULL AND MotivoCancelacion = N'Prueba de cancelacion'), 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P16 todos los pasos Cancelado', IIF((SELECT COUNT(*) FROM dbo.AG_ProcesoPaso WHERE IdProceso = @Pr2 AND Estado = 'Cancelado') = 2, 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P16 actividad anulada, entregable hecho conservado', IIF(EXISTS (SELECT 1 FROM dbo.AG_Actividad WHERE IdActividad = @QAc AND Anulada = 1 AND Usuario_Modificacion = 'ZZADM') AND EXISTS (SELECT 1 FROM dbo.AG_Entregable WHERE IdEntregable = @QUno AND Hecho = 1 AND Cod_Hecho = 'ZZR1'), 1, 0));
DELETE FROM @M;
INSERT INTO @M EXEC dbo.Sp_RTA_AgendaMarcarEntregable @IdEntregable = @QDos, @Hecho = 1, @Usuario = 'ZZR1', @FechaEntregaSiguiente = '20301118';
INSERT INTO @R (Caso, Ok) VALUES ('P16 marcar en paso Cancelado: -9', IIF(EXISTS (SELECT 1 FROM @M WHERE Respuestas = -9) AND EXISTS (SELECT 1 FROM dbo.AG_Entregable WHERE IdEntregable = @QDos AND Hecho = 0), 1, 0));
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaCancelarProceso @IdProceso = @Pr2, @Motivo = N'otra vez', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P16 cancelar dos veces: -1', IIF((SELECT Resultado FROM @Rs) = -1, 1, 0));
DELETE FROM @Rs;
INSERT INTO @Rs EXEC dbo.Sp_RTA_AgendaReasignarPaso @IdProcesoPaso = @Q1, @EstadoEsperado = 'Cancelado', @CodResponsable = 'ZZR9', @Usuario = 'ZZADM';
INSERT INTO @R (Caso, Ok) VALUES ('P16 reasignar en proceso Cancelado: -1', IIF((SELECT Resultado FROM @Rs) = -1, 1, 0));

/* P17 iniciar con datos invalidos o plantilla inactiva: no crea nada */
SELECT @Id = COUNT(*) FROM dbo.AG_Proceso;
DELETE FROM @I;
INSERT INTO @I EXEC dbo.Sp_RTA_AgendaIniciarProceso @Nombre = N'ZZ Malo', @IdPlantilla = @Pl,
     @Pasos = N'<ps><p o="1" a="1" t="x" d="3" r="ZZR1"/><p o="1" a="1" t="y" d="3" r="ZZR1"/></ps>',
     @FechaEntregaPrimero = '20301111', @Usuario = 'ZZINI';
INSERT INTO @R (Caso, Ok) VALUES ('P17 orden repetido: -2', IIF((SELECT IdProceso FROM @I) = -2, 1, 0));
DELETE FROM @I;
SET @Xml = CONVERT(XML, N'<ps><p o="1" a="' + CONVERT(NVARCHAR(10), @Ar1) + N'" t="x" d="3"/></ps>');
INSERT INTO @I EXEC dbo.Sp_RTA_AgendaIniciarProceso @Nombre = N'ZZ Malo', @IdPlantilla = @Pl, @Pasos = @Xml,
     @FechaEntregaPrimero = '20301111', @Usuario = 'ZZINI';
INSERT INTO @R (Caso, Ok) VALUES ('P17 sin responsable: -2', IIF((SELECT IdProceso FROM @I) = -2, 1, 0));
UPDATE dbo.AG_Plantilla SET Activa = 0 WHERE IdPlantilla = @Pl;
SET @Xml = CONVERT(XML, N'<ps><p o="1" a="' + CONVERT(NVARCHAR(10), @Ar1) + N'" t="x" d="3" r="ZZR1"/></ps>');
DELETE FROM @I;
INSERT INTO @I EXEC dbo.Sp_RTA_AgendaIniciarProceso @Nombre = N'ZZ Malo', @IdPlantilla = @Pl, @Pasos = @Xml,
     @FechaEntregaPrimero = '20301111', @Usuario = 'ZZINI';
INSERT INTO @R (Caso, Ok) VALUES ('P17 plantilla inactiva: -3', IIF((SELECT IdProceso FROM @I) = -3, 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P17 no se creo ningun proceso', IIF((SELECT COUNT(*) FROM dbo.AG_Proceso) = @Id, 1, 0));

/* P18 feriados: solo activos y desde hoy - 30 */
DELETE FROM @F;
INSERT INTO @F EXEC dbo.Sp_RTA_AgendaFeriados;
INSERT INTO @R (Caso, Ok) VALUES ('P18 feriados activos desde hoy - 30', IIF((SELECT COUNT(*) FROM @F) = (SELECT COUNT(*) FROM dbo.Feriado WHERE Activo = 1 AND Fecha >= DATEADD(DAY, -30, CONVERT(DATE, GETDATE()))) AND NOT EXISTS (SELECT 1 FROM @F WHERE Fecha < DATEADD(DAY, -30, CONVERT(DATE, GETDATE()))), 1, 0));

/* P19 la consulta de una actividad agrega las columnas de proceso (las 8
   de antes, en el mismo orden, y despues las nuevas) */
INSERT INTO @R (Caso, Ok) VALUES ('P19 columnas de Sp_RTA_AgendaActividad',
    IIF((SELECT STUFF((SELECT ',' + name FROM sys.dm_exec_describe_first_result_set_for_object(OBJECT_ID('dbo.Sp_RTA_AgendaActividad'), 0)
                        ORDER BY column_ordinal FOR XML PATH('')), 1, 1, ''))
        = 'IdActividad,Titulo,Cod_Responsable,Cod_AsignadoPor,FechaEntrega,CompletadaManual,FechaCompletada,Anulada,IdProcesoPaso,IdProceso,NombreProceso,EstadoPaso', 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P19 columnas de Sp_RTA_AgendaProcesos',
    IIF((SELECT STUFF((SELECT ',' + name FROM sys.dm_exec_describe_first_result_set_for_object(OBJECT_ID('dbo.Sp_RTA_AgendaProcesos'), 0)
                        ORDER BY column_ordinal FOR XML PATH('')), 1, 1, ''))
        = 'IdProceso,Nombre,IdPlantilla,Plantilla,CodIniciadoPor,Estado,FechaInicio,FechaFin,MotivoCancelacion', 1, 0));
INSERT INTO @R (Caso, Ok) VALUES ('P19 columnas de Marcar*',
    IIF((SELECT STUFF((SELECT ',' + name FROM sys.dm_exec_describe_first_result_set_for_object(OBJECT_ID('dbo.Sp_RTA_AgendaMarcarEntregable'), 0)
                        ORDER BY column_ordinal FOR XML PATH('')), 1, 1, '')) = 'Respuestas,IdProcesoPasoActivado,IdProcesoTerminado'
        AND (SELECT STUFF((SELECT ',' + name FROM sys.dm_exec_describe_first_result_set_for_object(OBJECT_ID('dbo.Sp_RTA_AgendaMarcarCompleta'), 0)
                        ORDER BY column_ordinal FOR XML PATH('')), 1, 1, '')) = 'Respuestas,IdProcesoPasoActivado,IdProcesoTerminado', 1, 0));
--</PROCESOS>

/* Invariantes al final, dentro de la transaccion */
INSERT INTO @R (Caso, Ok) VALUES ('Z ningun paso con dos actividades', IIF(NOT EXISTS (SELECT IdProcesoPaso FROM dbo.AG_Actividad WHERE IdProcesoPaso IS NOT NULL GROUP BY IdProcesoPaso HAVING COUNT(*) > 1), 1, 0)); -- solo E2
INSERT INTO @R (Caso, Ok) VALUES ('Z transaccion abierta y sana', IIF(@@TRANCOUNT = 1 AND XACT_STATE() = 1, 1, 0));

ROLLBACK TRANSACTION;

/* ------------------------------------------------------------ resultados */
DECLARE @N INT = 1, @Max INT = (SELECT MAX(N) FROM @R), @Linea NVARCHAR(800);
WHILE @N <= @Max
BEGIN
    SELECT @Linea = IIF(Ok = 1, 'PASS  ', 'FAIL  ') + Caso FROM @R WHERE N = @N;
    PRINT @Linea;
    SET @N += 1;
END
SET @N = 1; SET @Max = ISNULL((SELECT MAX(N) FROM @Diag), 0);
WHILE @N <= @Max
BEGIN
    SELECT @Linea = '      ' + Caso + ' [' + Lado + '] ' + V FROM @Diag WHERE N = @N;
    PRINT @Linea;
    SET @N += 1;
END

DECLARE @Quedaron INT =
    (SELECT COUNT(*) FROM #ZZAntes AS a
      WHERE a.Filas <> CASE a.Tabla
                         WHEN 'AG_Area' THEN (SELECT COUNT(*) FROM dbo.AG_Area)
                         WHEN 'AG_PersonaArea' THEN (SELECT COUNT(*) FROM dbo.AG_PersonaArea)
                         WHEN 'AG_Actividad' THEN (SELECT COUNT(*) FROM dbo.AG_Actividad)
                         WHEN 'AG_Entregable' THEN (SELECT COUNT(*) FROM dbo.AG_Entregable)
                         WHEN 'AG_CorreoBitacora' THEN (SELECT COUNT(*) FROM dbo.AG_CorreoBitacora)
                         WHEN 'AG_Plantilla' THEN (SELECT COUNT(*) FROM dbo.AG_Plantilla) -- solo E2
                         WHEN 'AG_PlantillaPaso' THEN (SELECT COUNT(*) FROM dbo.AG_PlantillaPaso) -- solo E2
                         WHEN 'AG_Proceso' THEN (SELECT COUNT(*) FROM dbo.AG_Proceso) -- solo E2
                         WHEN 'AG_ProcesoPaso' THEN (SELECT COUNT(*) FROM dbo.AG_ProcesoPaso) -- solo E2
                       END);
PRINT IIF(@Quedaron = 0 AND @@TRANCOUNT = 0, 'PASS  conteos de filas iguales que antes (no quedo nada)',
          'FAIL  conteos de filas distintos o transaccion abierta: ' + CONVERT(VARCHAR(10), @Quedaron));

DECLARE @Fallos INT = (SELECT COUNT(*) FROM @R WHERE Ok = 0) + IIF(@Quedaron = 0 AND @@TRANCOUNT = 0, 0, 1);
SELECT Casos = (SELECT COUNT(*) FROM @R) + 1, Fallos = @Fallos;
INSERT INTO #ZZFin (Veredicto) VALUES (IIF(@Fallos = 0, 'VEREDICTO: OK', 'VEREDICTO: FALLA'));
IF @Fallos = 0
    PRINT 'VEREDICTO: OK';
ELSE
    PRINT 'VEREDICTO: FALLA (' + CONVERT(VARCHAR(10), @Fallos) + ' casos). Revisar arriba.';
GO

/* Si el lote principal se corto por un error, no llego a escribir #ZZFin. */
IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM #ZZFin)
    PRINT 'VEREDICTO: FALLA (el lote principal se corto con un error; XACT_ABORT deshizo todo).';
SELECT Veredicto = ISNULL((SELECT TOP 1 Veredicto FROM #ZZFin), 'VEREDICTO: FALLA');
DROP TABLE #ZZFin;
DROP TABLE #ZZAntes;
DROP PROCEDURE #ZZFoto;
GO
