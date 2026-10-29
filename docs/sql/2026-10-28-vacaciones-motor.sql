/* ============================================================================
   Vacaciones - motor de saldos
   ReporTarea  |  2026-10-28
   Plan: docs/superpowers/plans/2026-10-28-vacaciones-motor-de-saldos.md (F2)
   Requiere 2026-10-28-vacaciones-esquema.sql.

   dbo.Sp_RTA_VacCalcular @FechaCalculo, @IdColaborador, @Salida, @Liquidacion
     @FechaCalculo   NULL = hoy. Si la persona tiene FechaSalida, se usa esa
                     (salvo con @Liquidacion = 1).
     @IdColaborador  NULL = todos.
     @Salida         'SALDOS' (una fila por persona), 'PERIODOS' (una por
                     periodo de aniversario) o 'MOVIMIENTOS' (una por
                     solicitud, con lo que descuenta).
     @Liquidacion    1 = hoja LIQUIDACION del Excel: se calcula EXACTAMENTE a
                     @FechaCalculo (aunque haya FechaSalida) y solo cuenta lo
                     tomado hasta esa fecha (como FUTURAS = 0). Agregado el
                     2026-10-29; con 0, todo sigue igual.
   Solo lee: no escribe en ninguna tabla.

   Replica las formulas del Excel de GTH (SISTEMA_VACACIONES_DOS), hoja por
   hoja; los nombres entre corchetes son sus columnas:
     MOVIMIENTOS = REGISTRO          [K] equivalentes, [T] base, [W] fin de
                                     semana, [L] total que descuenta
     PERIODOS    = CALCULO_PERIODOS  consumo del periodo mas antiguo primero
     SALDOS      = SALDOS
   Reglas, en el orden en que se aplican:
     1. Un movimiento no descuenta si no tiene fecha, esta DADA BAJA, es
        anterior al ingreso vigente (reingreso) o anterior al saldo de corte.
     2. Permisos por horas: (horas + minutos / 60) / HORAS_DIA.
     3. Fin de semana (solo VACACIONES): se carga uno si la solicitud termina
        en viernes (salvo BeneficioDos), y otro cada SUELTOS_CADA dias sueltos
        (sin fin de semana ni feriado en el rango) del mismo anio de servicio.
        Tope: MAX_FINDES por anio de servicio, contando los domingos que ya
        caen dentro de las fechas pedidas.
     4. Periodo n: DIAS_BASE + min(max(n - ANIOS_ADIC, 0), TOPE_ADIC) dias.
        Vence en el aniversario n + PER_ACUM. Consume lo tomado antes de su
        vencimiento que no absorbieron los periodos anteriores.
     5. El saldo de corte entra como un consumo inicial: lo ganado hasta el
        corte menos el saldo firmado.
   Sin tildes a proposito (ver el esquema).
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_VacCalcular', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacCalcular AS RETURN 0;');
GO

ALTER PROCEDURE dbo.Sp_RTA_VacCalcular
    @FechaCalculo  DATE        = NULL,
    @IdColaborador INT         = NULL,
    @Salida        VARCHAR(12) = 'SALDOS',
    @Liquidacion   BIT         = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Hoy DATE = ISNULL(@FechaCalculo, CAST(GETDATE() AS DATE));

    /* Calculo en FLOAT, como Excel. Los ROUND a 2 decimales pasan antes por
       DECIMAL(18,6): en FLOAT 23.815 queda 23.81499..., y Excel lo sube a 23.82. */
    DECLARE @DiasBase FLOAT, @AniosAdic INT, @TopeAdic INT, @PerAcum INT, @Alerta INT,
            @HorasDia FLOAT, @SueltosCada FLOAT, @DiasFinde FLOAT, @MaxFindes FLOAT,
            @FindeHoras BIT, @Futuras BIT;
    SELECT @DiasBase    = MAX(CASE WHEN Clave = 'DIAS_BASE'    THEN Valor END),
           @AniosAdic   = MAX(CASE WHEN Clave = 'ANIOS_ADIC'   THEN Valor END),
           @TopeAdic    = MAX(CASE WHEN Clave = 'TOPE_ADIC'    THEN Valor END),
           @PerAcum     = MAX(CASE WHEN Clave = 'PER_ACUM'     THEN Valor END),
           @Alerta      = MAX(CASE WHEN Clave = 'ALERTA'       THEN Valor END),
           @HorasDia    = MAX(CASE WHEN Clave = 'HORAS_DIA'    THEN Valor END),
           @SueltosCada = MAX(CASE WHEN Clave = 'SUELTOS_CADA' THEN Valor END),
           @DiasFinde   = MAX(CASE WHEN Clave = 'DIAS_FINDE'   THEN Valor END),
           @MaxFindes   = MAX(CASE WHEN Clave = 'MAX_FINDES'   THEN Valor END),
           @FindeHoras  = MAX(CASE WHEN Clave = 'FINDE_HORAS'  THEN Valor END),
           @Futuras     = MAX(CASE WHEN Clave = 'FUTURAS'      THEN Valor END)
    FROM dbo.Vac_Parametro;
    IF @Liquidacion = 1 SET @Futuras = 0;

    /* Dias de cada anio de servicio y lo acumulado hasta el (PARAMETROS A20:C59). */
    CREATE TABLE #Anio (N INT PRIMARY KEY, Dias FLOAT NOT NULL, Acumulado FLOAT NOT NULL);
    ;WITH n AS (SELECT TOP (40) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N FROM sys.all_objects)
    INSERT INTO #Anio (N, Dias, Acumulado)
    SELECT n.N, d.Dias, SUM(d.Dias) OVER (ORDER BY n.N ROWS UNBOUNDED PRECEDING)
    FROM n
    CROSS APPLY (SELECT @DiasBase + CASE WHEN n.N - @AniosAdic < 0 THEN 0
                                         WHEN n.N - @AniosAdic > @TopeAdic THEN @TopeAdic
                                         ELSE n.N - @AniosAdic END AS Dias) d;

    /* Feriados: los historicos hasta 2025 y dbo.Feriado desde 2026. */
    CREATE TABLE #Feriado (Fecha DATE PRIMARY KEY);
    INSERT INTO #Feriado (Fecha)
    SELECT Fecha FROM dbo.Vac_FeriadoHistorico
    UNION
    SELECT Fecha FROM dbo.Feriado WHERE Activo = 1 AND Fecha >= '20260101';

    /* ------------------------------------------------ colaboradores ------- */
    /* Anios cumplidos = DATEDIF(a, b, "y"). */
    CREATE TABLE #Col
    (
        IdColaborador INT PRIMARY KEY,
        Ingreso       DATE  NOT NULL,
        Calculo       DATE  NOT NULL,     -- [G] salida o fecha de calculo
        Corte         DATE  NULL,
        Estado        VARCHAR(10) NOT NULL,
        Cumplidos     INT   NOT NULL,     -- [I]
        Ajuste        FLOAT NOT NULL      -- [M] consumo inicial por el corte
    );
    INSERT INTO #Col (IdColaborador, Ingreso, Calculo, Corte, Estado, Cumplidos, Ajuste)
    SELECT c.IdColaborador, c.FechaIngreso, g.Calculo, c.FechaCorte, c.Estado,
           CASE WHEN g.Calculo < c.FechaIngreso THEN 0
                ELSE DATEDIFF(YEAR, c.FechaIngreso, g.Calculo)
                     - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, c.FechaIngreso, g.Calculo), c.FechaIngreso) > g.Calculo THEN 1 ELSE 0 END
           END,
           CASE WHEN c.FechaCorte IS NULL THEN 0
                ELSE ISNULL((SELECT a.Acumulado FROM #Anio a WHERE a.N =
                         DATEDIFF(YEAR, c.FechaIngreso, c.FechaCorte)
                         - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, c.FechaIngreso, c.FechaCorte), c.FechaIngreso) > c.FechaCorte THEN 1 ELSE 0 END), 0)
                     - c.SaldoCorte
           END
    FROM dbo.Vac_Colaborador c
    CROSS APPLY (SELECT CASE WHEN @Liquidacion = 1 THEN @Hoy ELSE ISNULL(c.FechaSalida, @Hoy) END AS Calculo) g
    WHERE @IdColaborador IS NULL OR c.IdColaborador = @IdColaborador;

    /* -------------------------------------------------- movimientos ------- */
    CREATE TABLE #Mov
    (
        IdMovimiento  INT PRIMARY KEY,
        IdColaborador INT NOT NULL,
        Tipo          VARCHAR(25) NOT NULL,
        Estado        VARCHAR(15) NOT NULL,
        Desde         DATE NULL,
        Hasta         DATE NULL,
        Orden         INT  NOT NULL,
        Situacion     VARCHAR(20) NOT NULL,
        Equivalentes  FLOAT NOT NULL,     -- [K]
        Base          FLOAT NOT NULL,     -- [T]
        AnioServicio  INT   NULL,         -- [Y]
        NoLaborables  INT   NOT NULL,     -- [S] fines de semana + feriados en el rango
        TerminaViernes INT  NOT NULL,     -- [AB]
        DomingosDentro INT  NOT NULL,     -- [AA]
        Sueltos       FLOAT NOT NULL,     -- [U]
        SueltosAntes  FLOAT NULL,         -- [V]
        FindesAntes   FLOAT NULL,         -- [AC]
        Findes        FLOAT NOT NULL DEFAULT 0,  -- [AD]
        Paso          INT  NULL           -- orden dentro de (persona, anio de servicio)
    );

    INSERT INTO #Mov (IdMovimiento, IdColaborador, Tipo, Estado, Desde, Hasta, Orden, Situacion,
                      Equivalentes, Base, AnioServicio, NoLaborables, TerminaViernes, DomingosDentro, Sueltos)
    SELECT m.IdMovimiento, m.IdColaborador, m.Tipo, m.Estado, m.FechaDesde, m.FechaHasta, m.Orden,
           s.Situacion, k.Equivalentes, b.Base, y.AnioServicio, nl.NoLaborables, v.TerminaViernes, dd.DomingosDentro,
           CASE WHEN b.Base = 0 THEN 0
                WHEN m.Tipo = 'VACACIONES' THEN CASE WHEN nl.NoLaborables = 0 AND v.TerminaViernes = 0 THEN b.Base ELSE 0 END
                WHEN m.Tipo = 'PERMISO POR HORAS' THEN CASE WHEN @FindeHoras = 1 THEN b.Base ELSE 0 END
                ELSE 0 END
    FROM dbo.Vac_Movimiento m
    JOIN #Col c ON c.IdColaborador = m.IdColaborador
    CROSS APPLY (SELECT CASE WHEN ISNULL(m.Horas, 0) + ISNULL(m.Minutos, 0) > 0
                             THEN (ISNULL(CAST(m.Horas AS FLOAT), 0) + ISNULL(CAST(m.Minutos AS FLOAT), 0) / 60) / @HorasDia
                             ELSE ISNULL(CAST(m.Dias AS FLOAT), 0) END AS Equivalentes) k
    CROSS APPLY (SELECT CASE WHEN m.FechaDesde IS NULL                              THEN 'SIN FECHA'
                             WHEN m.Estado = 'DADA BAJA'                            THEN 'ANULADO'
                             WHEN m.FechaDesde < c.Ingreso                          THEN 'RELACION ANTERIOR'
                             WHEN c.Corte IS NOT NULL AND m.FechaDesde < c.Corte    THEN 'ANTES DEL CORTE'
                             WHEN @Futuras = 0 AND m.FechaDesde > c.Calculo         THEN 'POSTERIOR'
                             ELSE 'DESCUENTA' END AS Situacion) s
    CROSS APPLY (SELECT CASE WHEN s.Situacion = 'DESCUENTA' THEN k.Equivalentes ELSE 0 END AS Base) b
    CROSS APPLY (SELECT CASE WHEN m.FechaDesde IS NULL OR m.FechaDesde < c.Ingreso THEN NULL
                             ELSE DATEDIFF(YEAR, c.Ingreso, m.FechaDesde) + 1
                                  - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, c.Ingreso, m.FechaDesde), c.Ingreso) > m.FechaDesde THEN 1 ELSE 0 END
                        END AS AnioServicio) y
    /* Dia de la semana sin depender de DATEFIRST: 1900-01-01 fue lunes, 0 = lunes ... 4 = viernes, 6 = domingo. */
    CROSS APPLY (SELECT CASE WHEN m.FechaDesde IS NULL OR m.FechaHasta IS NULL OR m.FechaHasta < m.FechaDesde THEN 0
                             ELSE (SELECT COUNT(*)
                                   FROM (SELECT TOP (DATEDIFF(DAY, m.FechaDesde, m.FechaHasta) + 1)
                                                DATEADD(DAY, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1, m.FechaDesde) AS Dia
                                         FROM sys.all_objects a1 CROSS JOIN sys.all_objects a2) r
                                   WHERE DATEDIFF(DAY, '19000101', r.Dia) % 7 >= 5
                                      OR r.Dia IN (SELECT f.Fecha FROM #Feriado f)) END AS NoLaborables) nl
    CROSS APPLY (SELECT CASE WHEN b.Base = 0 OR m.Tipo <> 'VACACIONES' OR m.FechaHasta IS NULL OR m.BeneficioDos = 1 THEN 0
                             WHEN DATEDIFF(DAY, '19000101', m.FechaHasta) % 7 = 4 THEN 1
                             ELSE 0 END AS TerminaViernes) v
    /* Domingos entre Desde+1 y Hasta (NETWORKDAYS.INTL con "1111110"). 1899-12-31 fue domingo. */
    CROSS APPLY (SELECT CASE WHEN b.Base = 0 OR m.Tipo <> 'VACACIONES' OR m.FechaHasta IS NULL OR m.FechaHasta <= m.FechaDesde THEN 0
                             ELSE DATEDIFF(DAY, '18991231', m.FechaHasta) / 7 - DATEDIFF(DAY, '18991231', m.FechaDesde) / 7
                        END AS DomingosDentro) dd;

    /* Fines de semana cargados. Cada solicitud depende de lo cargado en las
       anteriores del mismo anio de servicio, asi que va paso a paso: en el
       paso k se calculan, a la vez para todas las personas, sus k-esimas. */
    UPDATE m SET Paso = r.Paso
    FROM #Mov m
    JOIN (SELECT IdMovimiento, ROW_NUMBER() OVER (PARTITION BY IdColaborador, AnioServicio ORDER BY Desde, Orden) AS Paso
          FROM #Mov WHERE Base <> 0) r ON r.IdMovimiento = m.IdMovimiento;

    DECLARE @Paso INT = 1, @Pasos INT = (SELECT ISNULL(MAX(Paso), 0) FROM #Mov);
    WHILE @Paso <= @Pasos
    BEGIN
        UPDATE m
        SET SueltosAntes = a.Sueltos, FindesAntes = a.Findes
        FROM #Mov m
        CROSS APPLY (SELECT ISNULL(SUM(q.Sueltos), 0) AS Sueltos, ISNULL(SUM(q.DomingosDentro + q.Findes), 0) AS Findes
                     FROM #Mov q
                     WHERE q.IdColaborador = m.IdColaborador AND q.AnioServicio = m.AnioServicio AND q.Paso < m.Paso) a
        WHERE m.Paso = @Paso;

        UPDATE m
        SET Findes = CASE WHEN p.PorRegla < p.Cupo THEN p.PorRegla ELSE p.Cupo END
        FROM #Mov m
        CROSS APPLY (SELECT m.TerminaViernes
                            + CASE WHEN m.Sueltos = 0 THEN 0
                                   ELSE FLOOR(ROUND((m.SueltosAntes + m.Sueltos) / @SueltosCada, 6))
                                        - FLOOR(ROUND(m.SueltosAntes / @SueltosCada, 6)) END AS PorRegla,
                            CASE WHEN @MaxFindes - m.FindesAntes - m.DomingosDentro > 0
                                 THEN @MaxFindes - m.FindesAntes - m.DomingosDentro ELSE 0 END AS Cupo) p
        WHERE m.Paso = @Paso;

        SET @Paso += 1;
    END

    IF @Salida = 'MOVIMIENTOS'
    BEGIN
        SELECT m.IdMovimiento, m.IdColaborador, m.Tipo, m.Estado, m.Desde, m.Hasta, m.Situacion,
               m.AnioServicio,
               /* 8 decimales, como DiasTomados: el estado individual las acumula. */
               CAST(ROUND(m.Equivalentes, 8) AS DECIMAL(18,8))                    AS DiasEquivalentes,
               CAST(ROUND(m.Base, 8) AS DECIMAL(18,8))                            AS DiasBase,
               CAST(@DiasFinde * m.Findes AS DECIMAL(12,4))                       AS DiasFinDeSemana,
               CAST(ROUND(m.Base + @DiasFinde * m.Findes, 8) AS DECIMAL(18,8))    AS DiasDescuenta,
               /* [Z] otras solicitudes vigentes de la misma persona que se cruzan en fechas */
               CASE WHEN m.Desde IS NULL OR m.Hasta IS NULL OR m.Estado = 'DADA BAJA' THEN 0
                    ELSE (SELECT COUNT(*) FROM #Mov o
                          WHERE o.IdColaborador = m.IdColaborador AND o.IdMovimiento <> m.IdMovimiento
                            AND o.Estado <> 'DADA BAJA' AND o.Desde <= m.Hasta AND o.Hasta >= m.Desde) END AS Cruces
        FROM #Mov m
        ORDER BY m.IdColaborador, m.Desde, m.Orden;
        RETURN;
    END

    /* ----------------------------------------------------- periodos ------- */
    /* Del 1 al anio en curso + 1 (el Excel abre hasta cumplidos + 2), maximo 40. */
    CREATE TABLE #Per
    (
        IdColaborador INT NOT NULL,
        N             INT NOT NULL,
        Inicio        DATE NOT NULL,
        Fin           DATE NOT NULL,
        Dias          FLOAT NOT NULL,     -- [H]
        Vence         DATE NOT NULL,      -- [I]
        TomadoHastaVencer FLOAT NOT NULL, -- [J]
        Consumido     FLOAT NULL,         -- [L]
        Acumulado     FLOAT NULL,         -- [M]
        PRIMARY KEY (IdColaborador, N)
    );
    INSERT INTO #Per (IdColaborador, N, Inicio, Fin, Dias, Vence, TomadoHastaVencer)
    SELECT c.IdColaborador, a.N,
           DATEADD(YEAR, a.N - 1, c.Ingreso),
           DATEADD(DAY, -1, DATEADD(YEAR, a.N, c.Ingreso)),
           a.Dias,
           DATEADD(YEAR, a.N + @PerAcum, c.Ingreso),
           c.Ajuste + ISNULL((SELECT SUM(m.Base + @DiasFinde * m.Findes) FROM #Mov m
                              WHERE m.IdColaborador = c.IdColaborador
                                AND m.Desde < DATEADD(YEAR, a.N + @PerAcum, c.Ingreso)), 0)
    FROM #Col c
    JOIN #Anio a ON a.N <= c.Cumplidos + 2;

    /* El periodo n consume lo que queda despues de los anteriores, hasta sus dias. */
    DECLARE @N INT = 1;
    WHILE @N <= 40
    BEGIN
        UPDATE p
        SET Consumido = x.Consumido, Acumulado = ISNULL(ant.Acumulado, 0) + x.Consumido
        FROM #Per p
        LEFT JOIN #Per ant ON ant.IdColaborador = p.IdColaborador AND ant.N = p.N - 1
        CROSS APPLY (SELECT p.TomadoHastaVencer - ISNULL(ant.Acumulado, 0) AS Libre) l
        CROSS APPLY (SELECT CASE WHEN l.Libre <= 0 THEN 0 WHEN l.Libre > p.Dias THEN p.Dias ELSE l.Libre END AS Consumido) x
        WHERE p.N = @N;
        SET @N += 1;
    END

    SELECT p.IdColaborador, p.N, p.Inicio, p.Fin, p.Dias, p.Vence, p.Consumido, c.Calculo,
           CASE WHEN c.Corte IS NOT NULL AND p.Vence <= c.Corte THEN 'CERRADO AL CORTE'
                WHEN c.Calculo < p.Inicio THEN 'FUTURO'
                WHEN c.Calculo <= p.Fin THEN 'EN CURSO'
                WHEN p.Dias - p.Consumido < 0.005 THEN 'GOZADO'
                WHEN c.Calculo >= p.Vence THEN 'VENCIDO'
                ELSE 'VIGENTE' END AS Estado
    INTO #PerEstado
    FROM #Per p
    JOIN #Col c ON c.IdColaborador = p.IdColaborador;

    IF @Salida = 'PERIODOS'
    BEGIN
        SELECT IdColaborador, N,
               CONVERT(VARCHAR(4), YEAR(Inicio)) + '-' + CONVERT(VARCHAR(4), YEAR(Inicio) + 1) AS Periodo,
               Inicio, Fin,
               CAST(Dias AS DECIMAL(12,4))                                                   AS DiasGanados,
               /* 8 decimales: el estado individual suma estos valores para saber a que periodo
                  se carga cada solicitud, y con 4 un limite exacto (530.13125) quedaba corrido. */
               CAST(ROUND(Consumido, 8) AS DECIMAL(18,8))                                    AS DiasTomados,
               CAST(ROUND(CASE WHEN Estado = 'VENCIDO' THEN Dias - Consumido ELSE 0 END, 4) AS DECIMAL(12,4)) AS DiasVencidos,
               CAST(ROUND(CASE WHEN Estado = 'VIGENTE' THEN Dias - Consumido ELSE 0 END, 4) AS DECIMAL(12,4)) AS Saldo,
               Vence, Estado
        FROM #PerEstado
        ORDER BY IdColaborador, N;
        RETURN;
    END

    /* ------------------------------------------------------- saldos ------- */
    SELECT c.IdColaborador, c.Estado AS EstadoColaborador, c.Calculo AS FechaCalculo, c.Cumplidos,
           CAST(ac.Dias AS DECIMAL(12,4))                                            AS DiasPorAnio,
           pc.InicioPeriodo, pc.ProximoAniversario,
           CAST(ROUND(c.Ajuste, 4) AS DECIMAL(12,4))                                 AS AjusteCorte,
           CAST(ROUND(t.Disponible, 4) AS DECIMAL(12,4))                             AS Disponible,
           CAST(ROUND(t.Usado, 4) AS DECIMAL(12,4))                                  AS UsadoEnCurso,
           CAST(pr.Proporcional AS DECIMAL(12,4))                                    AS Proporcional,
           CAST(ROUND(CAST(t.Disponible + pr.Proporcional - t.Usado AS DECIMAL(18,6)), 2) AS DECIMAL(12,2)) AS Saldo,
           CAST(ROUND(t.PorVencer, 4) AS DECIMAL(12,4))                              AS PorVencer,
           t.ProximoVencimiento,
           CAST(ROUND(t.Vencidos, 4) AS DECIMAL(12,4))                               AS Vencidos,
           CAST(ROUND(ISNULL(g.Gozado, 0), 4) AS DECIMAL(12,4))                      AS TotalGozado,
           CASE WHEN ROUND(CAST(t.Disponible + pr.Proporcional - t.Usado AS DECIMAL(18,6)), 2) < 0 THEN 'ANTICIPO'
                WHEN t.PorVencer > 0 THEN 'POR VENCER'
                WHEN t.Disponible > ac.Dias THEN 'ACUMULANDO'
                ELSE 'AL DIA' END                                                    AS Estado
    FROM #Col c
    JOIN #Anio ac ON ac.N = c.Cumplidos + 1
    CROSS APPLY (SELECT DATEADD(YEAR, c.Cumplidos, c.Ingreso) AS InicioPeriodo,
                        DATEADD(YEAR, c.Cumplidos + 1, c.Ingreso) AS ProximoAniversario) pc
    CROSS APPLY (SELECT CASE WHEN x.P > 0 THEN x.P ELSE 0 END AS Proporcional
                 FROM (SELECT CAST(ROUND(CAST(ac.Dias * DATEDIFF(DAY, pc.InicioPeriodo, c.Calculo)
                                    / DATEDIFF(DAY, pc.InicioPeriodo, pc.ProximoAniversario) AS DECIMAL(18,6)), 2) AS FLOAT) AS P) x) pr
    CROSS APPLY (SELECT ISNULL(SUM(CASE WHEN e.Estado = 'VIGENTE' THEN e.Dias - e.Consumido ELSE 0 END), 0) AS Disponible,
                        ISNULL(SUM(CASE WHEN e.Estado IN ('EN CURSO', 'FUTURO') THEN e.Consumido ELSE 0 END), 0) AS Usado,
                        ISNULL(SUM(CASE WHEN e.Estado = 'VIGENTE' AND DATEDIFF(DAY, e.Calculo, e.Vence) <= @Alerta
                                        THEN e.Dias - e.Consumido ELSE 0 END), 0) AS PorVencer,
                        ISNULL(SUM(CASE WHEN e.Estado = 'VENCIDO' THEN e.Dias - e.Consumido ELSE 0 END), 0) AS Vencidos,
                        MIN(CASE WHEN e.Estado = 'VIGENTE' AND e.Dias - e.Consumido > 0 THEN e.Vence END) AS ProximoVencimiento
                 FROM #PerEstado e WHERE e.IdColaborador = c.IdColaborador) t
    CROSS APPLY (SELECT SUM(m.Base + @DiasFinde * m.Findes) AS Gozado FROM #Mov m WHERE m.IdColaborador = c.IdColaborador) g
    ORDER BY c.IdColaborador;
END
GO
