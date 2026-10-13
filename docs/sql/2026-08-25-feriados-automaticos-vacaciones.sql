/* ============================================================================
   Pantalla: Solicitar Vacaciones y Permisos (SolicitarVacacionesPermiso.aspx)
   Etapa 1 de CB-GAP-POL-01: los feriados dejan de digitarse a mano.

   1. Sp_RTA_ContarFeriadosRango : cuantos feriados caen en el rango pedido

   Se puede ejecutar varias veces sin efecto adicional. No toca datos.
   Base: ReporTarea
   ============================================================================

   Por que existe este procedimiento
   ---------------------------------
   Hasta ahora el colaborador escribia a mano cuantos feriados caian dentro de
   sus vacaciones, en el campo "Feriados". Ese numero no era decorativo: se
   restaba de los dias que se cobran al saldo, porque un feriado dentro del
   rango no consume vacaciones. De 1207 solicitudes de vacaciones, 65 lo usaron.

   La especificacion pide quitar el campo. Quitarlo sin mas le cobraria a la
   gente los dias completos, asi que en vez de eliminar el concepto se elimina
   el trabajo manual: la tabla Feriado ya tiene los feriados del pais y ya la
   usan FN_RTA_ClasificarTramosHorario y SP_RegistrarHorasEmpleado. Este
   procedimiento la consulta para el rango de la solicitud.

   AniosSinCargar es la parte importante
   -------------------------------------
   La tabla Feriado se carga a mano y hoy solo tiene 2026: no hay pantalla que
   la mantenga. Un rango de 2027 devolveria 0 feriados, y el colaborador
   perderia dias sin que nadie se entere. Por eso el procedimiento devuelve
   tambien los anios del rango de los que no hay ni un feriado cargado, para que
   la pantalla lo pueda advertir en vez de callarse.
   ============================================================================ */

SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_ContarFeriadosRango') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ContarFeriadosRango;
GO

CREATE PROCEDURE dbo.Sp_RTA_ContarFeriadosRango
    @FechaDesde DATE = NULL,
    @FechaHasta DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* Rango invalido o incompleto: se responde 0 y sin advertencia. La pantalla
       llama a esto en cada cambio de fecha, incluso a medio llenar. */
    IF @FechaDesde IS NULL OR @FechaHasta IS NULL OR @FechaHasta < @FechaDesde
    BEGIN
        SELECT Feriados = 0, AniosSinCargar = '';
        RETURN;
    END

    DECLARE @Feriados INT =
    (
        SELECT COUNT(*)
        FROM dbo.Feriado
        WHERE Activo = 1
          AND Fecha BETWEEN @FechaDesde AND @FechaHasta
    );

    /* Los anios que toca el rango. Son uno o dos salvo vacaciones absurdamente
       largas, asi que el ciclo no es un problema de rendimiento. */
    DECLARE @Anios TABLE (Anio INT PRIMARY KEY);
    DECLARE @Anio INT = YEAR(@FechaDesde);

    WHILE @Anio <= YEAR(@FechaHasta)
    BEGIN
        INSERT INTO @Anios (Anio) VALUES (@Anio);
        SET @Anio = @Anio + 1;
    END

    /* De esos anios, los que no tienen ningun feriado cargado. La concatenacion
       sin ORDER BY alcanza porque son a lo sumo dos filas. */
    DECLARE @AniosSinCargar VARCHAR(100) = '';

    SELECT @AniosSinCargar = @AniosSinCargar
                           + CASE WHEN @AniosSinCargar = '' THEN '' ELSE ', ' END
                           + CAST(a.Anio AS VARCHAR(4))
    FROM @Anios a
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Feriado f
                      WHERE f.Activo = 1 AND YEAR(f.Fecha) = a.Anio);

    SELECT Feriados = @Feriados, AniosSinCargar = @AniosSinCargar;
END
GO
PRINT 'Sp_RTA_ContarFeriadosRango creado.';
GO

/* ------------------------------------------------------------------ pruebas
   Para correr a mano despues de crear el procedimiento. No forman parte del
   despliegue.

-- Semana Santa 2026: el 03/04 es Viernes Santo, debe dar 1 feriado.
EXEC dbo.Sp_RTA_ContarFeriadosRango '2026-03-30', '2026-04-05';

-- Carnaval 2026: 16 y 17 de febrero, debe dar 2.
EXEC dbo.Sp_RTA_ContarFeriadosRango '2026-02-14', '2026-02-20';

-- Un rango sin feriados: debe dar 0 y sin advertencia.
EXEC dbo.Sp_RTA_ContarFeriadosRango '2026-06-01', '2026-06-05';

-- Un anio no cargado: debe dar 0 feriados y advertir '2027'.
EXEC dbo.Sp_RTA_ContarFeriadosRango '2027-01-01', '2027-01-10';

-- Rango invertido: debe dar 0 y no reventar.
EXEC dbo.Sp_RTA_ContarFeriadosRango '2026-05-10', '2026-05-01';
   ------------------------------------------------------------------------- */
