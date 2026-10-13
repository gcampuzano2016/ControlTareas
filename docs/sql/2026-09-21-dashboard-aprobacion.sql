/* ============================================================================
   Dashboard de aprobacion para jefatura: los cinco conjuntos
   ReporTarea  |  2026-09-21

   REEMPLAZADO por docs/sql/2026-09-23-dashboard-horas-por-cliente.sql, que es
   la definicion vigente del procedimiento (agrega MinutosAprobados por empresa
   y el conjunto 6 de persona x cliente). NO VOLVER A CORRER ESTE SCRIPT: si se
   corre, el procedimiento vuelve a devolver cinco conjuntos y la tabla nueva
   de la pantalla (TablaClientes, la que cruza persona y cliente) queda vacia
   SIN NINGUN ERROR, porque el DAO tolera la ausencia del sexto conjunto.

   YA APLICADO EN PRODUCCION (verificado el 2026-09-21, despues de la ronda
   de correcciones). Se corrio la version corregida y se comprobo contra la
   base: cinco conjuntos, PersonasDiaTotal presente y DiasPromedio decimal
   -13.200000 en la muestra- con DiasMaximo todavia entero.

   ---------------------------------------------------------------------------

   Devuelve TODO ya agregado. El navegador dibuja, no suma: traer 5.665 filas para
   que el JavaScript las recorra seria lento y, peor, pondria el calculo de horas
   en un segundo lugar distinto del que usa la tabla de la misma pantalla.

   El parseo de Det_Tiempo es el de Sp_RTAListaHorasRecursosPorJefatura, copiado:
   SUBSTRING(...,1,2)*60 + SUBSTRING(...,4,2). Descarta los segundos -5.616 filas
   los tienen distintos de cero- y se deja asi A PROPOSITO. Si el grafico sumara
   los segundos y la tabla no, dirian cifras distintas sobre el mismo dato.

   Idempotente: DROP y CREATE.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.R_DetTareasAranda','U') IS NULL
BEGIN
    RAISERROR('No existe dbo.R_DetTareasAranda. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

PRINT '== Dashboard de aprobacion - inicio ==';
GO

IF OBJECT_ID('dbo.Sp_RTA_DashboardAprobacionJefatura','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_DashboardAprobacionJefatura;
GO

CREATE PROCEDURE dbo.Sp_RTA_DashboardAprobacionJefatura
    @IdUsuarioJefe VARCHAR(10),
    @FechaInicio   VARCHAR(20),
    @FechaFin      VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Desde DATE, @Hasta DATE;
    SET @Desde = CONVERT(DATE, @FechaInicio, 105);
    SET @Hasta = CONVERT(DATE, @FechaFin, 105);

    /* Los cuatro codigos escritos a mano son los mismos de
       Sp_RTAListaHorasRecursosPorJefatura: ven a todo el mundo en vez de a su
       equipo. Se replica la regla, verruga incluida, para que el dashboard cubra
       EXACTAMENTE a la misma gente que la tabla de la misma pantalla. Si un dia
       se arregla, hay que arreglarlo en los dos lugares a la vez. */
    DECLARE @VeTodo BIT;
    SET @VeTodo = CASE WHEN @IdUsuarioJefe IN ('1314','1171','222692','1655906')
                       THEN 1 ELSE 0 END;

    /* Tabla temporal y no CTE: los cinco conjuntos la recorren, y con un CTE se
       recalcularia cinco veces. */
    CREATE TABLE #Base
    (
        Id_Responsable VARCHAR(50),
        Nombre         VARCHAR(100),
        Fecha          DATE,
        Estado         BIGINT,
        Empresa        VARCHAR(200),
        Minutos        INT,
        FechaAprob     DATETIME
    );

    /* El LIKE sobre Det_Tiempo protege al CONVERT: hoy las 86.496 filas tienen
       formato HH:MM:SS, pero una sola fila mal formada tumbaria el dashboard
       entero con un error de conversion, y un tablero que no abre es peor que uno
       al que le falta una fila. */
    INSERT INTO #Base (Id_Responsable, Nombre, Fecha, Estado, Empresa, Minutos, FechaAprob)
      SELECT  a.Id_Responsable,
              u.Nom_Usuario,
              CONVERT(DATE, a.Det_Fch_RegDetalleIni),
              a.Det_Aprobacion_Tarea_Estado,
              LTRIM(RTRIM(ISNULL(a.Det_Nom_Empresa, '(sin empresa)'))),
              CONVERT(INT, SUBSTRING(a.Det_Tiempo, 1, 2)) * 60
            + CONVERT(INT, SUBSTRING(a.Det_Tiempo, 4, 2)),
              a.Det_Fecha_Aprobacion_Tarea
        FROM  dbo.R_DetTareasAranda a
        INNER JOIN dbo.R_Usuarios u ON a.Id_Responsable = u.Cod_Usuario
       WHERE  CONVERT(DATE, a.Det_Fch_RegDetalleIni) BETWEEN @Desde AND @Hasta
         AND  a.Det_Tiempo LIKE '[0-9][0-9]:[0-9][0-9]:[0-9][0-9]'
         AND  (@VeTodo = 1
               OR a.Id_Responsable = LTRIM(RTRIM(@IdUsuarioJefe))
               OR u.Cod_Jefe_Inm   = LTRIM(RTRIM(@IdUsuarioJefe)));

    /* --- 1. totales ------------------------------------------------------
       Personas-dia con COUNT(DISTINCT responsable + fecha): una persona que
       cargo seis tareas el martes es UNA persona-dia, no seis.

       PersonasDiaTotal es el mismo COUNT(DISTINCT ...) pero SIN filtrar por
       estado. Los dos parciales de abajo se solapan -un dia con tareas
       aprobadas y pendientes cae en los dos- asi que sumarlos cuenta de mas;
       el total hay que contarlo aparte y es esta columna.

       Los estados 5 y 7 van juntos en "Otros". Son 592 filas que hoy no aparecen
       en ninguna pantalla y no estan en el catalogo; no se les inventa un nombre,
       pero esconderlas haria que el total de las tarjetas no cuadre con el rango,
       y un tablero cuyos numeros no suman no lo cree nadie. */
    SELECT
        MinutosAprobados   = ISNULL(SUM(CASE WHEN Estado = 2 THEN Minutos END), 0),
        MinutosPendientes  = ISNULL(SUM(CASE WHEN Estado = 1 THEN Minutos END), 0),
        MinutosOtros       = ISNULL(SUM(CASE WHEN Estado NOT IN (1,2) THEN Minutos END), 0),
        PersonasDiaTotal   = ISNULL(COUNT(DISTINCT Id_Responsable + '|' + CONVERT(VARCHAR(10), Fecha, 112)), 0),
        PersonasDiaAprob   = ISNULL(COUNT(DISTINCT CASE WHEN Estado = 2 THEN Id_Responsable + '|' + CONVERT(VARCHAR(10), Fecha, 112) END), 0),
        PersonasDiaPend    = ISNULL(COUNT(DISTINCT CASE WHEN Estado = 1 THEN Id_Responsable + '|' + CONVERT(VARCHAR(10), Fecha, 112) END), 0),
        Responsables       = ISNULL(COUNT(DISTINCT Id_Responsable), 0)
      FROM #Base;

    /* --- 2. por semana ---------------------------------------------------
       DATEADD/DATEDIFF por WEEK da el lunes de cada semana, que es la etiqueta
       del eje. Se devuelve como fecha y no como "semana 38": el numero de semana
       obliga a quien mira a traducirlo, y cruza mal el cambio de anio. */
    SELECT
        Semana            = DATEADD(WEEK, DATEDIFF(WEEK, 0, Fecha), 0),
        MinutosAprobados  = ISNULL(SUM(CASE WHEN Estado = 2 THEN Minutos END), 0),
        MinutosPendientes = ISNULL(SUM(CASE WHEN Estado = 1 THEN Minutos END), 0)
      FROM #Base
     GROUP BY DATEADD(WEEK, DATEDIFF(WEEK, 0, Fecha), 0)
     ORDER BY 1;

    /* --- 3. por responsable ----------------------------------------------
       DiasBajoJornada usa el mismo umbral de 8 horas que CumpleJornada en
       Sp_RTAListaHorasRecursosPorJefatura. Se cuenta sobre el total del dia, sin
       importar el estado: un dia incompleto lo es aunque ya este aprobado. */
    SELECT
        Nombre            = MAX(b.Nombre),
        MinutosAprobados  = ISNULL(SUM(CASE WHEN b.Estado = 2 THEN b.Minutos END), 0),
        MinutosPendientes = ISNULL(SUM(CASE WHEN b.Estado = 1 THEN b.Minutos END), 0),
        PersonasDia       = COUNT(DISTINCT CONVERT(VARCHAR(10), b.Fecha, 112)),
        DiasBajoJornada   = ISNULL((SELECT COUNT(1)
                                      FROM (SELECT d.Fecha
                                              FROM #Base d
                                             WHERE d.Id_Responsable = b.Id_Responsable
                                             GROUP BY d.Fecha
                                            HAVING SUM(d.Minutos) < 480) x), 0)
      FROM #Base b
     GROUP BY b.Id_Responsable
     ORDER BY 2 DESC;

    /* --- 4. demora en aprobar --------------------------------------------
       Solo sobre lo aprobado y con fecha de aprobacion: el 97,7% de las filas la
       tiene. DiasMasViejoPendiente mira lo que sigue en estado 1 y responde la
       pregunta que un promedio no puede: hace cuanto que lo mas viejo espera. */
    SELECT
        /* DECIMAL y no INT: AVG sobre INT en T-SQL TRUNCA, no redondea, y una
           demora media real de 1,9 dias se reportaba como 1. El sesgo era
           sistematico y siempre a la baja: el tablero decia que se tarda menos
           de lo que se tarda. Un decimal alcanza; el segundo es ruido.
           DiasMaximo sigue siendo entero a proposito: es un DATEDIFF suelto,
           no un promedio, y no tiene nada que redondear. */
        DiasPromedio = ISNULL(ROUND(AVG(CAST(CASE WHEN Estado = 2 AND FechaAprob IS NOT NULL
                                                  THEN DATEDIFF(DAY, Fecha, CONVERT(DATE, FechaAprob)) END AS DECIMAL(9,2))), 1), 0),
        DiasMaximo   = ISNULL(MAX(CASE WHEN Estado = 2 AND FechaAprob IS NOT NULL
                                       THEN DATEDIFF(DAY, Fecha, CONVERT(DATE, FechaAprob)) END), 0),
        AprobadasConFecha = ISNULL(SUM(CASE WHEN Estado = 2 AND FechaAprob IS NOT NULL THEN 1 ELSE 0 END), 0),
        AprobadasSinFecha = ISNULL(SUM(CASE WHEN Estado = 2 AND FechaAprob IS NULL THEN 1 ELSE 0 END), 0),
        DiasMasViejoPendiente = ISNULL(DATEDIFF(DAY, MIN(CASE WHEN Estado = 1 THEN Fecha END), CONVERT(DATE, GETDATE())), 0)
      FROM #Base;

    /* --- 5. por empresa ---------------------------------------------------
       Se devuelven TODAS. El top 10 y la porcion "Otras" los arma
       NegDashboardAprobacion.TopConOtras, que esta probado; hacerlo aqui dejaria
       esa regla sin prueba y repartida en dos lenguajes. */
    SELECT
        Empresa = Empresa,
        Minutos = ISNULL(SUM(Minutos), 0)
      FROM #Base
     GROUP BY Empresa
     ORDER BY 2 DESC;

    DROP TABLE #Base;
END
GO

PRINT 'Sp_RTA_DashboardAprobacionJefatura actualizado.';
GO

SET NOEXEC OFF;
GO

PRINT '== Dashboard de aprobacion - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   1) Que exista:

SELECT nombre = name, creado = CONVERT(VARCHAR(20), modify_date, 120)
  FROM sys.procedures WHERE name = 'Sp_RTA_DashboardAprobacionJefatura';

   2) Que devuelva cinco conjuntos y que los totales cuadren con la pantalla.
      Usar un jefe y un rango reales, los mismos que se elijan en la pantalla:

EXEC dbo.Sp_RTA_DashboardAprobacionJefatura '1171', '01-03-2026', '31-03-2026';

   ============================================================================ */