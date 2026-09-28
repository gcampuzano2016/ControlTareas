/* ============================================================================
   REFERENCIA - NO EJECUTAR

   Copia fiel de dbo.FN_RTA_ClasificarTramosHorario tal como estaba en
   produccion el 2026-09-28. Se versiona para poder leerla y revisarla: hasta
   hoy solo existia en la base.

   NO correr este archivo. La usa Sp_RTAInsertaDetalleTarea_V2 en cada registro
   de actividad y ahora tambien Sp_RTA_HeFueraDeHorario. Volver a crearla desde
   aqui es la forma mas facil de cambiarla sin querer, y si el texto de este
   archivo quedo viejo, se pisaria la version buena.

   Si la funcion cambia en la base, se vuelve a volcar este archivo; no al
   reves.
   ============================================================================ */

SET NOEXEC ON;
GO

CREATE FUNCTION dbo.FN_RTA_ClasificarTramosHorario
(
    @Id_Responsable VARCHAR(20),
    @FechaInicio    DATETIME,
    @FechaFin       DATETIME
)
RETURNS @Tramos TABLE
(
    Orden        INT NOT NULL,
    TramoInicio  DATETIME NOT NULL,
    TramoFin     DATETIME NOT NULL,
    TipoHora     BIGINT NOT NULL,
    Descripcion  VARCHAR(50) NOT NULL
)
AS
BEGIN
    IF @FechaInicio IS NULL
       OR @FechaFin IS NULL
       OR @FechaInicio >= @FechaFin
    BEGIN
        RETURN;
    END;

    IF CONVERT(DATE, @FechaInicio) <> CONVERT(DATE, @FechaFin)
    BEGIN
        RETURN;
    END;

    DECLARE @FechaTrabajo DATE;
    DECLARE @DiaSemana TINYINT;
    DECLARE @IdHorarioLaboral INT;
    DECLARE @EsLaborable BIT;
    DECLARE @HoraInicioOficina TIME(0);
    DECLARE @HoraFinOficina TIME(0);
    DECLARE @EsFeriado BIT;

    DECLARE @InicioDia DATETIME;
    DECLARE @LimiteSeis DATETIME;
    DECLARE @InicioOficina DATETIME;
    DECLARE @FinOficina DATETIME;

    SET @FechaTrabajo = CONVERT(DATE, @FechaInicio);
    SET @EsFeriado = 0;

    /* 1900-01-01 fue lunes. No depende de SET DATEFIRST. */
    SET @DiaSemana =
        CONVERT(TINYINT, (DATEDIFF(DAY, '19000101', @FechaTrabajo) % 7) + 1);

    /* 1. Horario especifico del empleado */
    SELECT TOP (1)
        @IdHorarioLaboral = Asignacion.IdHorarioLaboral
    FROM dbo.R_UsuarioHorarioLaboral AS Asignacion
    INNER JOIN dbo.R_HorarioLaboral AS Horario
        ON Horario.IdHorarioLaboral = Asignacion.IdHorarioLaboral
    WHERE Asignacion.Id_Responsable = @Id_Responsable
      AND Asignacion.Activo = 1
      AND Horario.Activo = 1
      AND Asignacion.FechaDesde <= @FechaTrabajo
      AND (Asignacion.FechaHasta IS NULL OR Asignacion.FechaHasta >= @FechaTrabajo)
    ORDER BY Asignacion.FechaDesde DESC;

    /* 2. Si no existe asignacion, usar horario predeterminado */
    IF @IdHorarioLaboral IS NULL
    BEGIN
        SELECT TOP (1)
            @IdHorarioLaboral = Horario.IdHorarioLaboral
        FROM dbo.R_HorarioLaboral AS Horario
        WHERE Horario.EsPredeterminado = 1
          AND Horario.Activo = 1
        ORDER BY Horario.IdHorarioLaboral;
    END;

    IF @IdHorarioLaboral IS NULL
    BEGIN
        RETURN;
    END;

    /* 3. Configuracion del dia */
    SELECT TOP (1)
        @EsLaborable = Detalle.EsLaborable,
        @HoraInicioOficina = Detalle.HoraInicio,
        @HoraFinOficina = Detalle.HoraFin
    FROM dbo.R_HorarioLaboralDetalle AS Detalle
    WHERE Detalle.IdHorarioLaboral = @IdHorarioLaboral
      AND Detalle.DiaSemana = @DiaSemana
      AND Detalle.Activo = 1;

    IF @EsLaborable IS NULL
    BEGIN
        RETURN;
    END;

    /* 4. Validar feriado nacional */
    IF EXISTS
    (
        SELECT 1
        FROM dbo.Feriado AS Feriado
        WHERE Feriado.Fecha = @FechaTrabajo
          AND Feriado.Activo = 1
    )
    BEGIN
        SET @EsFeriado = 1;
    END;

    /* 5. Feriado o dia no laborable: 100% */
    IF @EsFeriado = 1 OR @EsLaborable = 0
    BEGIN
        INSERT INTO @Tramos (Orden, TramoInicio, TramoFin, TipoHora, Descripcion)
        VALUES (1, @FechaInicio, @FechaFin, 2, 'HORAS EXTRAORDINARIAS 100%');
        RETURN;
    END;

    SET @InicioDia = CONVERT(DATETIME, @FechaTrabajo);
    SET @LimiteSeis = DATEADD(HOUR, 6, @InicioDia);

    SET @InicioOficina =
        DATEADD(SECOND,
            DATEDIFF(SECOND, CAST('00:00:00' AS TIME(0)), @HoraInicioOficina),
            @InicioDia);

    SET @FinOficina =
        DATEADD(SECOND,
            DATEDIFF(SECOND, CAST('00:00:00' AS TIME(0)), @HoraFinOficina),
            @InicioDia);

    DECLARE @Puntos TABLE (Punto DATETIME NOT NULL PRIMARY KEY);

    INSERT INTO @Puntos (Punto)
    VALUES (@FechaInicio), (@FechaFin);

    IF @LimiteSeis > @FechaInicio AND @LimiteSeis < @FechaFin
       AND NOT EXISTS (SELECT 1 FROM @Puntos WHERE Punto = @LimiteSeis)
    BEGIN
        INSERT INTO @Puntos (Punto) VALUES (@LimiteSeis);
    END;

    IF @InicioOficina > @FechaInicio AND @InicioOficina < @FechaFin
       AND NOT EXISTS (SELECT 1 FROM @Puntos WHERE Punto = @InicioOficina)
    BEGIN
        INSERT INTO @Puntos (Punto) VALUES (@InicioOficina);
    END;

    IF @FinOficina > @FechaInicio AND @FinOficina < @FechaFin
       AND NOT EXISTS (SELECT 1 FROM @Puntos WHERE Punto = @FinOficina)
    BEGIN
        INSERT INTO @Puntos (Punto) VALUES (@FinOficina);
    END;

    ;WITH PuntosOrdenados AS
    (
        SELECT Punto, ROW_NUMBER() OVER (ORDER BY Punto) AS Orden
        FROM @Puntos
    ),
    Segmentos AS
    (
        SELECT
            Inicio.Orden,
            Inicio.Punto AS TramoInicio,
            Fin.Punto AS TramoFin,
            CASE
                WHEN CONVERT(TIME(0), Inicio.Punto) < CAST('06:00:00' AS TIME(0))
                    THEN 2
                WHEN Inicio.Punto >= @InicioOficina AND Fin.Punto <= @FinOficina
                    THEN 0
                ELSE 1
            END AS TipoHora
        FROM PuntosOrdenados AS Inicio
        INNER JOIN PuntosOrdenados AS Fin
            ON Fin.Orden = Inicio.Orden + 1
    )
    INSERT INTO @Tramos (Orden, TramoInicio, TramoFin, TipoHora, Descripcion)
    SELECT
        Segmentos.Orden,
        Segmentos.TramoInicio,
        Segmentos.TramoFin,
        Segmentos.TipoHora,
        CASE Segmentos.TipoHora
            WHEN 0 THEN 'HORAS NORMALES'
            WHEN 1 THEN 'HORAS SUPLEMENTARIAS 50%'
            WHEN 2 THEN 'HORAS EXTRAORDINARIAS 100%'
        END
    FROM Segmentos;

    RETURN;
END;

GO
SET NOEXEC OFF;
GO
