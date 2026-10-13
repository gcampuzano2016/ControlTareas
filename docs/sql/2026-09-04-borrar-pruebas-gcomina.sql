/* ============================================================================
   Borra las solicitudes de PRUEBA de Gabriela Comina.

   Base: ReporTarea
   ============================================================================

   LEER ANTES DE CORRER
   --------------------
   El script viene con ROLLBACK puesto. Asi como esta, no borra nada: corre
   todo, muestra los conteos y deshace. Se revisa el resultado y recien
   entonces se cambia la ultima linea por COMMIT y se vuelve a correr.

   Que borra
   ---------
   Las solicitudes donde ella es la colaboradora, registradas desde el 25 de
   agosto de 2026, que es cuando empezaron las pruebas del flujo de tres
   firmas. Con todo lo que cuelga de cada una: firmas, detalle de teletrabajo,
   plan de recuperacion y sus notificaciones.

   Que NO borra, y por que
   -----------------------
   1. Su historial real anterior al 25 de agosto. Son seis solicitudes de
      verdad -vacaciones de diciembre de 2025, permisos de mayo y junio,
      vacaciones del 3 de agosto- y no tienen nada que ver con las pruebas.

   2. El usuario. Gabriela es la persona de Talento Humano: su correo es el
      valor del parametro CORREORH y es quien pone la tercera firma. Borrarla
      de R_Usuarios dejaria al sistema sin destinatario para el aviso de
      validacion y sin nadie que pueda cerrar un tramite.

   3. Sus firmas como GTH sobre solicitudes de OTRAS personas. Al 4 de
      septiembre de 2026 son cuatro: 22876, 22888, 22891 y 22893, tres de ellas
      ya PROCESADAS. Son registros de otra gente y su firma es parte de ellos.
      El borrado va acotado por IdVacaciones de las solicitudes de ella, asi
      que esas quedan intactas sin que haya que hacer nada especial; se dice
      aca para que quede constancia de que se penso.

   Los archivos en disco
   ---------------------
   El adjunto de una solicitud vive en la propia fila de Vacaciones
   -Ruta_Archivo y Descripcion_Archivo-, no en una tabla aparte. Al borrar la
   fila se pierde la referencia, pero el archivo sigue en el disco del
   servidor, en descargas\ y en repositorio_archivos\. No estorba: nadie llega
   a el sin la referencia. Si se quiere limpiar tambien, es a mano.

   Un efecto lateral, deseable
   ---------------------------
   El saldo del permiso mensual se calcula sumando las horas de los permisos no
   rechazados del mes. Al borrar las pruebas, la bolsa de Gabriela de agosto y
   septiembre vuelve a quedar libre, que es lo correcto: esas horas no fueron
   ausencias reales.
   ============================================================================ */

SET NOCOUNT ON;
GO

DECLARE @Cedula VARCHAR(32)  = '1750304964';   /* Gabriela Comina */
DECLARE @Desde  DATE         = '2026-08-25';   /* inicio de las pruebas */

/* ---------------------------------------------------- que se va a borrar */
DECLARE @ids TABLE (IdVacaciones BIGINT PRIMARY KEY);

INSERT INTO @ids (IdVacaciones)
SELECT v.IdVacaciones
FROM dbo.Vacaciones v
WHERE v.Cedula = @Cedula
  AND v.FechaRegistro >= @Desde;

PRINT '--- solicitudes que se van a borrar ---';
SELECT v.IdVacaciones,
       Tipo   = CASE WHEN v.IdTipoSolicitud = 1 THEN 'PERMISO' ELSE 'VACACIONES' END,
       v.EstadoSolicitud,
       Fecha  = CAST(v.FechaRegistro AS DATE),
       v.Horas,
       v.TotalDias,
       Firmas = (SELECT COUNT(1) FROM dbo.VacacionesFirma f WHERE f.IdVacaciones = v.IdVacaciones)
FROM dbo.Vacaciones v
JOIN @ids i ON i.IdVacaciones = v.IdVacaciones
ORDER BY v.IdVacaciones;

PRINT '--- lo que se conserva de ella (historial real) ---';
SELECT v.IdVacaciones,
       Tipo  = CASE WHEN v.IdTipoSolicitud = 1 THEN 'PERMISO' ELSE 'VACACIONES' END,
       v.EstadoSolicitud,
       Fecha = CAST(v.FechaRegistro AS DATE)
FROM dbo.Vacaciones v
WHERE v.Cedula = @Cedula AND v.FechaRegistro < @Desde
ORDER BY v.IdVacaciones;

PRINT '--- sus firmas como GTH en solicitudes de otros (NO se tocan) ---';
SELECT f.IdVacaciones, f.Rol, f.Decision, v.Colaborador, v.EstadoSolicitud
FROM dbo.VacacionesFirma f
JOIN dbo.Vacaciones v ON v.IdVacaciones = f.IdVacaciones
WHERE f.Cod_Usuario = '2869799'
  AND NOT EXISTS (SELECT 1 FROM @ids i WHERE i.IdVacaciones = f.IdVacaciones)
ORDER BY f.IdVacaciones;

/* --------------------------------------------------------- salvaguardas */
/* Si el filtro llegara a atrapar una solicitud de otra persona, no se sigue.
   Es barato y evita el peor error posible de este script. */
IF EXISTS (SELECT 1 FROM dbo.Vacaciones v JOIN @ids i ON i.IdVacaciones = v.IdVacaciones
           WHERE v.Cedula <> @Cedula)
BEGIN
    RAISERROR('El filtro alcanzo solicitudes de otra persona. No se borro nada.', 16, 1);
    RETURN;
END

/* Un rango demasiado grande casi siempre significa una fecha mal escrita. */
DECLARE @Cuantas INT = (SELECT COUNT(1) FROM @ids);
IF @Cuantas > 60
BEGIN
    RAISERROR('Son mas de 60 solicitudes. Revise @Desde antes de continuar.', 16, 1);
    RETURN;
END

PRINT '--- total de solicitudes alcanzadas ---';
SELECT Solicitudes = @Cuantas;

/* ------------------------------------------------------------- el borrado */
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @nNotif INT, @nRecup INT, @nTele INT, @nFirmas INT, @nLog INT, @nSol INT;

    /* De la hoja hacia la raiz. El esquema no tiene claves foraneas, asi que el
       orden no lo impone el motor: lo impone quien escribe, y si se invierte
       quedan filas huerfanas apuntando a una solicitud que ya no existe. */
    DELETE n FROM dbo.NotificacionRecuperacion n JOIN @ids i ON i.IdVacaciones = n.IdVacaciones;
    SET @nNotif = @@ROWCOUNT;

    DELETE r FROM dbo.PermisoRecuperacion r JOIN @ids i ON i.IdVacaciones = r.IdVacaciones;
    SET @nRecup = @@ROWCOUNT;

    DELETE p FROM dbo.PermisoTeletrabajo p JOIN @ids i ON i.IdVacaciones = p.IdVacaciones;
    SET @nTele = @@ROWCOUNT;

    DELETE f FROM dbo.VacacionesFirma f JOIN @ids i ON i.IdVacaciones = f.IdVacaciones;
    SET @nFirmas = @@ROWCOUNT;

    DELETE l FROM dbo.VacacionesLog l JOIN @ids i ON i.IdVacaciones = l.IdVacaciones;
    SET @nLog = @@ROWCOUNT;

    DELETE v FROM dbo.Vacaciones v JOIN @ids i ON i.IdVacaciones = v.IdVacaciones;
    SET @nSol = @@ROWCOUNT;

    PRINT '--- filas borradas ---';
    SELECT NotificacionRecuperacion = @nNotif,
           PermisoRecuperacion      = @nRecup,
           PermisoTeletrabajo       = @nTele,
           VacacionesFirma          = @nFirmas,
           VacacionesLog            = @nLog,
           Vacaciones               = @nSol;

    /* ====================================================================
       ROLLBACK mientras se revisa. Cuando los numeros de arriba sean los
       esperados, cambiar esta linea por COMMIT TRANSACTION y volver a correr.
       ==================================================================== */
    ROLLBACK TRANSACTION;
    PRINT 'ROLLBACK: no se borro nada. Cambie la linea por COMMIT para aplicarlo.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'Error, no se borro nada: ' + ERROR_MESSAGE();
END CATCH
GO

/* ------------------------------------------------------------ despues de
   Para comprobar que quedo como se esperaba, una vez aplicado el COMMIT:

-- Debe devolver solo las seis anteriores al 25 de agosto.
SELECT IdVacaciones, EstadoSolicitud, CAST(FechaRegistro AS DATE) AS Fecha
FROM dbo.Vacaciones WHERE Cedula = '1750304964' ORDER BY IdVacaciones;

-- Sus firmas como GTH sobre solicitudes de otros deben seguir estando: 4 filas.
SELECT f.IdVacaciones, f.Rol, v.Colaborador
FROM dbo.VacacionesFirma f JOIN dbo.Vacaciones v ON v.IdVacaciones = f.IdVacaciones
WHERE f.Cod_Usuario = '2869799';

-- Su bolsa mensual de septiembre debe volver a estar entera.
EXEC dbo.Sp_RTA_SaldoPermisoMensual @Cod_Usuario = '2869799', @Fecha = '2026-09-04';

-- Y no deben quedar hijos apuntando a solicitudes que ya no existen.
SELECT huerfanas = COUNT(1) FROM dbo.VacacionesFirma f
WHERE NOT EXISTS (SELECT 1 FROM dbo.Vacaciones v WHERE v.IdVacaciones = f.IdVacaciones);
   ------------------------------------------------------------------------- */
