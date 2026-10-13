/* ============================================================================
   Corrige el respaldo de los permisos medicos ya registrados.

   Base: ReporTarea
   ============================================================================

   Que paso
   --------
   En un permiso medico la cita es obligatoria: el formulario no deja enviar sin
   adjuntarla. Por eso el selector "Respaldo adjunto" no se muestra en ese tipo,
   no hay nada que decidir.

   Pero el selector escondido seguia mandando su valor por defecto, NO_APLICA, y
   eso es lo que se guardo. En el documento ese valor se imprime como
   "Respaldo adjunto: No aplica", que se lee como "no hay respaldo" -justo lo
   contrario de lo que ocurrio, porque el archivo se subio y sin el la solicitud
   no se habria podido enviar-.

   La solicitud 22897 es la que lo destapo.

   El codigo ya no lo repite: desde el 4 de septiembre de 2026 el servidor graba
   SI en los medicos, sin depender de lo que mande la pantalla. Este script
   arregla lo que quedo guardado antes.

   Alcance
   -------
   Solo TipoPermiso = 'MEDICO' con RespaldoAdjunto = 'NO_APLICA'. Al 4 de
   septiembre de 2026 son 5 filas.

   No toca los otros tipos. En PERSONAL, por ejemplo, hay 7 con NO_APLICA y son
   correctos: ahi el selector se ve y la persona eligio esa opcion. Y TELETRABAJO
   se queda como esta, que tambien es cierto: no hay respaldo que adjuntar.
   ============================================================================ */

SET NOCOUNT ON;
GO

PRINT '--- antes ---';
SELECT IdVacaciones, TipoPermiso, RespaldoAdjunto, Colaborador,
       Fecha = CAST(FechaRegistro AS DATE)
FROM dbo.Vacaciones
WHERE IdTipoSolicitud = 1
  AND TipoPermiso = 'MEDICO'
  AND ISNULL(RespaldoAdjunto,'') = 'NO_APLICA'
ORDER BY IdVacaciones;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE dbo.Vacaciones
    SET RespaldoAdjunto = 'SI'
    WHERE IdTipoSolicitud = 1
      AND TipoPermiso = 'MEDICO'
      AND ISNULL(RespaldoAdjunto,'') = 'NO_APLICA';

    DECLARE @n INT = @@ROWCOUNT;
    PRINT '--- filas corregidas ---';
    SELECT Corregidas = @n;

    /* Un numero muy distinto al esperado significa que el filtro alcanzo algo
       que no debia. Cinco al 4 de septiembre de 2026; crece si se registran
       medicos nuevos antes de desplegar los binarios. */
    IF @n > 50
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Se alcanzaron mas de 50 filas. No se aplico nada, revise el filtro.', 16, 1);
        RETURN;
    END

    COMMIT TRANSACTION;
    PRINT 'Aplicado.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'Error, no se aplico nada: ' + ERROR_MESSAGE();
END CATCH
GO

PRINT '--- despues: no debe quedar ningun medico en NO_APLICA ---';
SELECT TipoPermiso, RespaldoAdjunto = ISNULL(RespaldoAdjunto,'(nulo)'), Cuantos = COUNT(1)
FROM dbo.Vacaciones
WHERE IdTipoSolicitud = 1 AND ISNULL(TipoPermiso,'') <> ''
GROUP BY TipoPermiso, ISNULL(RespaldoAdjunto,'(nulo)')
ORDER BY TipoPermiso, 2;
GO
