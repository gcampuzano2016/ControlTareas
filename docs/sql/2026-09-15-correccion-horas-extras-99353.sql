/* ============================================================================
   Corrección puntual: horas extras del sábado 05-09-2026
   ReporTarea  |  2026-09-15

   PARA EJECUTAR A MANO. Revise el paso 1, ejecute el paso 2, verifique el 3.

   ----------------------------------------------------------------------------
   Qué se corrige

   ALARCON CALLE JIMMY SEBASTIAN (Cod_Usuario 1871753) registró dos actividades
   el sábado 05-09-2026:

       99191   06:00 a 06:40   Tipo 2 = 100%     correcto
       99353   16:40 a 18:00   Tipo 0 = ninguna  INCORRECTO

   El sábado no es laborable en ninguno de los cinco perfiles de horario, así
   que las dos debían quedar al 100%.

   ----------------------------------------------------------------------------
   Por qué pasó

   Entre el 01 y el 07 de septiembre la clasificación automática estuvo
   desactivada en producción. El SQL estaba aplicado desde el 26 de agosto, pero
   la línea de CapaDato que llama a Sp_RTAInsertaDetalleTarea_V2 se revirtió el
   01-09 (commit cf677f6) y volvió el 07-09 (commit 66d1cc1). Con el
   procedimiento anterior el tipo de hora no se calcula: se guarda lo que la
   persona elija en el combo "¿Hora extra?" de la pantalla, que viene en blanco
   (value 0) por defecto.

   Eso explica que dos registros del mismo sábado tengan tipos distintos:
   en el de la madrugada se seleccionó 100% a mano, en el de la tarde no.

   ----------------------------------------------------------------------------
   IMPORTANTE: este script NO envía el correo de autorización

   El correo con los botones Aprobar / Rechazar lo arma el C#
   (AdministrarTarea.ashx.cs), no el procedimiento. Al corregir por SQL el
   registro queda en estado 1 (solicitud enviada) pero la jefatura no recibe
   nada.

   La jefatura debe aprobarlo desde la pantalla de horas extras por autorizar,
   filtrando por estado 1 (Sp_RTAConsultaHorasExtrasPorAutorizar recibe el
   estado como parámetro, así que el registro sí aparece en esa lista).

   Avísele a la jefatura de Jimmy que tiene una solicitud esperando.

   ----------------------------------------------------------------------------
   Estados de Det_Horas_Extras_Estado

       0  sin solicitar
       1  solicitud enviada   <- queda así
       2  aprobado
       3  rechazado
   ============================================================================ */

USE [ReporTarea];
GO

SET NOCOUNT ON;
GO

/* ============================================================
   PASO 1. ANTES. Revise que el registro sea el esperado.
   ============================================================
   Debe devolver UNA fila, de ALARCON CALLE JIMMY SEBASTIAN,
   del 2026-09-05 16:40 a 18:00, con Det_Horas_Extras_Tipo = 0.
   Si no cuadra, NO siga. */

PRINT '--- PASO 1: estado ANTES de la correccion ---';

SELECT
    D.Id_RegDetTareas,
    U.Nom_Usuario,
    D.Det_Fch_RegDetalleIni,
    D.Det_Fch_RegDetalleFin,
    D.Det_Tiempo,
    D.Det_Horas_Extras_Tipo,
    D.Det_Horas_Extras_Estado,
    D.Det_Horas_Extras_Descripcion,
    D.Det_Det_Tarea
FROM dbo.R_DetTareasAranda AS D
INNER JOIN dbo.R_Usuarios AS U
    ON U.Cod_Usuario = D.Id_Responsable
WHERE D.Id_RegDetTareas = 99353;
GO

/* ============================================================
   PASO 2. LA CORRECCION.
   ============================================================
   Va dentro de una transaccion con guardas. Si algo no cuadra
   -el registro no existe, no es de Jimmy, no es del 5 de
   septiembre, o ya no esta en tipo 0- hace ROLLBACK y no toca
   nada. */

BEGIN TRANSACTION;

BEGIN TRY

    DECLARE @Afectadas INT;

    UPDATE dbo.R_DetTareasAranda
    SET
        Det_Horas_Extras_Tipo        = 2,        /* 100% */
        Det_Horas_Extras_Descripcion = '100%',   /* igual que el registro 99191 */
        Det_Horas_Extras_Estado      = 1         /* solicitud enviada */
    WHERE Id_RegDetTareas = 99353
      AND Id_Responsable  = '1871753'
      AND CONVERT(DATE, Det_Fch_RegDetalleIni) = '2026-09-05'
      AND ISNULL(Det_Horas_Extras_Tipo, 0) = 0;

    SET @Afectadas = @@ROWCOUNT;

    IF @Afectadas = 1
    BEGIN
        COMMIT TRANSACTION;
        PRINT '--- PASO 2: OK. 1 fila corregida y confirmada. ---';
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT '--- PASO 2: SIN CAMBIOS. Se esperaba 1 fila y se afectaron '
              + CAST(@Afectadas AS VARCHAR(10))
              + '. Se deshizo todo. Revise el PASO 1. ---';
    END;

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '--- PASO 2: ERROR. Se deshizo todo. ---';
    PRINT ERROR_MESSAGE();

END CATCH;
GO

/* ============================================================
   PASO 3. DESPUES. Las dos filas del sabado deben quedar
   iguales: Tipo 2.
   ============================================================
   99191 debe seguir en Tipo 2 / Estado 2 (ya aprobada).
   99353 debe quedar en Tipo 2 / Estado 1 (esperando firma). */

PRINT '--- PASO 3: estado DESPUES de la correccion ---';

SELECT
    D.Id_RegDetTareas,
    D.Det_Fch_RegDetalleIni,
    D.Det_Fch_RegDetalleFin,
    D.Det_Tiempo,
    D.Det_Horas_Extras_Tipo,
    D.Det_Horas_Extras_Estado,
    D.Det_Horas_Extras_Descripcion
FROM dbo.R_DetTareasAranda AS D
WHERE D.Id_Responsable = '1871753'
  AND CONVERT(DATE, D.Det_Fch_RegDetalleIni) = '2026-09-05'
ORDER BY D.Det_Fch_RegDetalleIni;
GO

/* ============================================================
   ROLLBACK, si hiciera falta volver atras
   ============================================================
   Deja el registro como estaba antes de este script:

       UPDATE dbo.R_DetTareasAranda
       SET Det_Horas_Extras_Tipo        = 0,
           Det_Horas_Extras_Descripcion = '',
           Det_Horas_Extras_Estado      = 2
       WHERE Id_RegDetTareas = 99353;
   ============================================================ */
