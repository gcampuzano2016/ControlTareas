/* ============================================================================
   Horas Extras: personas fuera del modulo, completar datos y ajustes de sueldo
   ReporTarea  |  2026-09-28

   Tres procedimientos nuevos:

     1. Sp_RTA_HeFueraDelModulo       SOLO LECTURA. Empleados activos que NO
                                      estan en HE_ColaboradorParametro y que
                                      en el rango tienen horas extras en el
                                      flujo o tiempo fuera de su horario.
     2. Sp_RTA_HeAgregarColaborador   Inserta (o reactiva) la persona en el
                                      maestro y su sueldo 'Rol', en una sola
                                      transaccion.
     3. Sp_RTA_HeAgregarAjusteSalario Inserta un sueldo 'Ajuste'.

   HE_Salario SOLO recibe INSERT: ninguno de los tres actualiza ni borra un
   sueldo. Cada fila insertada lleva usuario, fecha e ip: es su propio rastro.

   Los perfiles NO se comprueban aqui, igual que en el resto del modulo: la
   barrera esta en AdministrarHorasExtras.ashx.cs (PerfilesQueEditanMaestro).

   Nadie llama a estos procedimientos hasta que se publiquen los binarios.
   Re-correr el script es inofensivo: son nuevos y no los recrea otra fase. Si
   una fase posterior los recrea con otra firma, este script tiene que llevar
   el guard SET NOEXEC de las fases 2, 4 y 5.

   La verificacion del final NO escribe nada: solo prueba los codigos de
   rechazo.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------ 1. personas fuera del modulo -- */
IF OBJECT_ID('dbo.Sp_RTA_HeFueraDelModulo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeFueraDelModulo;
GO
/* Misma forma que Sp_RTA_HeFueraDeHorario mas un conjunto de personas, para que
   CapaNegocio reutilice la misma cuenta de minutos. El puente de identidad es
   el mismo, en el mismo sentido; lo unico que cambia es que aqui se buscan los
   que NO estan en el maestro activo.

   1. Personas: las que tienen algo en los conjuntos 2 o 3.
   2. Segmentos fuera de horario de sus tramos no pagados.
   3. Sus tramos pagados (aprobados con tipo 1 o 2), con el tipo, para separar
      50% de 100% en lo aprobado. */
CREATE PROCEDURE dbo.Sp_RTA_HeFueraDelModulo
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Tramos
    (
        IdEmpleado     BIGINT   NOT NULL,
        Id_Responsable VARCHAR(20) NOT NULL,
        IdRegDetTareas BIGINT   NOT NULL,
        Inicio         DATETIME NOT NULL,
        Fin            DATETIME NOT NULL,
        Estado         INT      NOT NULL,
        Tipo           INT      NOT NULL,
        Actividad      VARCHAR(200) NOT NULL
    );

    INSERT INTO #Tramos (IdEmpleado, Id_Responsable, IdRegDetTareas, Inicio, Fin, Estado, Tipo, Actividad)
    SELECT e.IdEmpleado,
           d.Id_Responsable,
           CAST(d.Id_RegDetTareas AS BIGINT),
           d.Det_Fch_RegDetalleIni,
           d.Det_Fch_RegDetalleFin,
           CAST(d.Det_Horas_Extras_Estado AS INT),
           CAST(ISNULL(d.Det_Horas_Extras_Tipo, 0) AS INT),
           LEFT(ISNULL(d.Det_Det_Tarea, ''), 200)
      FROM dbo.R_DetTareasAranda d
      JOIN dbo.R_Usuarios u ON u.Cod_Usuario = d.Id_Responsable
      JOIN dbo.Empleados  e ON LTRIM(RTRIM(e.Cedula)) = LTRIM(RTRIM(u.Cedula))
     WHERE e.Estado = 'Activo'
       AND NOT EXISTS (SELECT 1 FROM dbo.HE_ColaboradorParametro c
                        WHERE c.IdEmpleado = e.IdEmpleado AND c.Estado = '1')
       AND d.Det_Fch_RegDetalleIni >= @FechaInicio
       AND d.Det_Fch_RegDetalleIni <  DATEADD(DAY, 1, @FechaFin)
       AND d.Det_Fch_RegDetalleFin IS NOT NULL
       AND d.Det_Horas_Extras_Estado IN (0, 1, 2, 3);

    CREATE TABLE #Segmentos
    (
        IdEmpleado BIGINT NOT NULL, IdRegDetTareas BIGINT NOT NULL,
        TramoInicio DATETIME NOT NULL, TramoFin DATETIME NOT NULL,
        SegmentoInicio DATETIME NOT NULL, SegmentoFin DATETIME NOT NULL,
        TipoHora INT NOT NULL, EstadoTramo INT NOT NULL, TipoRegistrado INT NOT NULL,
        Actividad VARCHAR(200) NOT NULL
    );

    INSERT INTO #Segmentos
    SELECT t.IdEmpleado, t.IdRegDetTareas, t.Inicio, t.Fin, s.TramoInicio, s.TramoFin,
           CAST(s.TipoHora AS INT), t.Estado, t.Tipo, t.Actividad
      FROM #Tramos t
     CROSS APPLY dbo.FN_RTA_ClasificarTramosHorario(t.Id_Responsable, t.Inicio, t.Fin) s
     WHERE s.TipoHora IN (1, 2)
       AND NOT (t.Estado = 2 AND t.Tipo IN (1, 2));

    /* 1. Personas */
    SELECT e.IdEmpleado,
           Nombre = LTRIM(RTRIM(ISNULL(e.Nombre, ''))),
           Cargo  = LTRIM(RTRIM(ISNULL(e.PuestoTrabajo, '')))
      FROM dbo.Empleados e
     WHERE e.IdEmpleado IN (SELECT IdEmpleado FROM #Segmentos
                            UNION
                            SELECT IdEmpleado FROM #Tramos WHERE Estado IN (1, 2, 3))
     ORDER BY e.Nombre, e.IdEmpleado;

    /* 2. Segmentos */
    SELECT IdEmpleado, IdRegDetTareas, TramoInicio, TramoFin, SegmentoInicio, SegmentoFin,
           TipoHora, EstadoTramo, TipoRegistrado, Actividad
      FROM #Segmentos
     ORDER BY IdEmpleado, SegmentoInicio;

    /* 3. Pagados */
    SELECT IdEmpleado, Inicio, Fin, TipoRegistrado = Tipo
      FROM #Tramos
     WHERE Estado = 2 AND Tipo IN (1, 2)
     ORDER BY IdEmpleado, Inicio;
END
GO

/* ---------------------------------------- 2. agregar colaborador ---------- */
IF OBJECT_ID('dbo.Sp_RTA_HeAgregarColaborador','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeAgregarColaborador;
GO
/* Respuestas: 0 guardado, -1 el empleado no existe o no esta activo, -2 ya
   esta en el maestro activo (usar el ajuste de sueldo), -3 datos invalidos.

   HE_ColaboradorParametro tiene indice unico por IdEmpleado: si ya hay una fila
   desactivada (Estado <> '1') se REACTIVA con los datos nuevos; insertar otra
   violaria el indice.

   El sueldo entra como 'Rol'. Maestro y sueldo van en una transaccion: una
   persona en el maestro sin sueldo calcularia su pago en cero. */
CREATE PROCEDURE dbo.Sp_RTA_HeAgregarColaborador
    @IdEmpleado         BIGINT,
    @Empresa            VARCHAR(120),
    @Cargo              VARCHAR(200),
    @JornadaHorasDia    INT,
    @AplicaHE           BIT,
    @Monto              DECIMAL(18,2),
    @FechaVigenciaDesde DATE,
    @Usuario            VARCHAR(50),
    @Ip                 VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Empleados WHERE IdEmpleado = @IdEmpleado AND Estado = 'Activo')
    BEGIN
        SELECT -1 AS Respuestas; RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.HE_ColaboradorParametro WHERE IdEmpleado = @IdEmpleado AND Estado = '1')
    BEGIN
        SELECT -2 AS Respuestas; RETURN;
    END

    IF LTRIM(RTRIM(ISNULL(@Empresa, ''))) = ''
       OR @JornadaHorasDia IS NULL OR @JornadaHorasDia < 1 OR @JornadaHorasDia > 12
       OR @Monto IS NULL OR @Monto <= 0
       OR @FechaVigenciaDesde IS NULL
    BEGIN
        SELECT -3 AS Respuestas; RETURN;
    END

    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.HE_ColaboradorParametro WHERE IdEmpleado = @IdEmpleado)
    BEGIN
        UPDATE dbo.HE_ColaboradorParametro
           SET JornadaHorasDia  = @JornadaHorasDia,
               DivisorManual    = NULL,
               AplicaHE         = @AplicaHE,
               MotivoNoAplica   = NULL,
               Empresa          = LTRIM(RTRIM(@Empresa)),
               Cargo            = NULLIF(LTRIM(RTRIM(ISNULL(@Cargo, ''))), ''),
               Estado           = '1',
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Usuario,
               Ip_Modificacion  = @Ip
         WHERE IdEmpleado = @IdEmpleado;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.HE_ColaboradorParametro
            (IdEmpleado, JornadaHorasDia, DivisorManual, AplicaHE, MotivoNoAplica, Empresa, Cargo,
             Estado, Fec_Modificacion, Usu_Modificacion, Ip_Modificacion)
        VALUES
            (@IdEmpleado, @JornadaHorasDia, NULL, @AplicaHE, NULL, LTRIM(RTRIM(@Empresa)),
             NULLIF(LTRIM(RTRIM(ISNULL(@Cargo, ''))), ''),
             '1', SYSDATETIME(), @Usuario, @Ip);
    END

    INSERT INTO dbo.HE_Salario
        (IdEmpleado, Monto, FechaVigenciaDesde, Origen, Observacion, Estado,
         Fec_Modificacion, Usu_Modificacion, Ip_Modificacion)
    VALUES
        (@IdEmpleado, @Monto, @FechaVigenciaDesde, 'Rol',
         'Cargado desde la pantalla de Horas Extras', '1',
         SYSDATETIME(), @Usuario, @Ip);

    COMMIT TRANSACTION;

    SELECT 0 AS Respuestas;
END
GO

/* ---------------------------------------- 3. agregar ajuste de sueldo ----- */
IF OBJECT_ID('dbo.Sp_RTA_HeAgregarAjusteSalario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeAgregarAjusteSalario;
GO
/* Respuestas: 0 guardado, -1 la persona no esta en el maestro activo, -2 ya hay
   un ajuste activo con esa misma fecha, -3 monto o fecha invalidos.

   NUNCA actualiza: agrega una fila 'Ajuste'. NegHorasExtras.SalarioVigente
   desempata Ajuste sobre Rol a igual fecha; dos Ajuste con la misma fecha no
   tendrian desempate y ganaria el orden de la lista, por eso se rechaza. */
CREATE PROCEDURE dbo.Sp_RTA_HeAgregarAjusteSalario
    @IdEmpleado         BIGINT,
    @Monto              DECIMAL(18,2),
    @FechaVigenciaDesde DATE,
    @Observacion        VARCHAR(400),
    @Usuario            VARCHAR(50),
    @Ip                 VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.HE_ColaboradorParametro WHERE IdEmpleado = @IdEmpleado AND Estado = '1')
    BEGIN
        SELECT -1 AS Respuestas; RETURN;
    END

    IF @Monto IS NULL OR @Monto <= 0 OR @FechaVigenciaDesde IS NULL
    BEGIN
        SELECT -3 AS Respuestas; RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.HE_Salario
                WHERE IdEmpleado = @IdEmpleado AND Estado = '1'
                  AND Origen = 'Ajuste' AND FechaVigenciaDesde = @FechaVigenciaDesde)
    BEGIN
        SELECT -2 AS Respuestas; RETURN;
    END

    INSERT INTO dbo.HE_Salario
        (IdEmpleado, Monto, FechaVigenciaDesde, Origen, Observacion, Estado,
         Fec_Modificacion, Usu_Modificacion, Ip_Modificacion)
    VALUES
        (@IdEmpleado, @Monto, @FechaVigenciaDesde, 'Ajuste',
         NULLIF(LTRIM(RTRIM(ISNULL(@Observacion, ''))), ''), '1',
         SYSDATETIME(), @Usuario, @Ip);

    SELECT 0 AS Respuestas;
END
GO

/* ------------------------------------------------------ VERIFICACION ------ */
/* Sin GO entre el DECLARE y el veredicto. No escribe nada: los dos llamados a
   procedimientos que insertan usan datos que se rechazan ANTES de insertar. */
SET NOCOUNT ON;
DECLARE @Fallos INT = 0;
DECLARE @R TABLE (Respuestas INT);

IF OBJECT_ID('dbo.Sp_RTA_HeFueraDelModulo','P') IS NULL
   OR OBJECT_ID('dbo.Sp_RTA_HeAgregarColaborador','P') IS NULL
   OR OBJECT_ID('dbo.Sp_RTA_HeAgregarAjusteSalario','P') IS NULL
BEGIN
    PRINT 'FALLO: falta alguno de los tres procedimientos.';
    SET @Fallos += 1;
END

INSERT INTO @R EXEC dbo.Sp_RTA_HeAgregarColaborador
    @IdEmpleado = -1, @Empresa = 'X', @Cargo = 'X', @JornadaHorasDia = 8, @AplicaHE = 1,
    @Monto = 1, @FechaVigenciaDesde = '2026-01-01', @Usuario = 'verificacion', @Ip = '';
IF (SELECT TOP 1 Respuestas FROM @R) <> -1
BEGIN
    PRINT 'FALLO: Sp_RTA_HeAgregarColaborador con un empleado inexistente deberia devolver -1.';
    SET @Fallos += 1;
END
DELETE FROM @R;

INSERT INTO @R EXEC dbo.Sp_RTA_HeAgregarAjusteSalario
    @IdEmpleado = -1, @Monto = 0, @FechaVigenciaDesde = '2026-01-01',
    @Observacion = '', @Usuario = 'verificacion', @Ip = '';
IF (SELECT TOP 1 Respuestas FROM @R) <> -1
BEGIN
    PRINT 'FALLO: Sp_RTA_HeAgregarAjusteSalario con una persona fuera del maestro deberia devolver -1.';
    SET @Fallos += 1;
END

DECLARE @Ini DATE, @Fin DATE;
SELECT TOP (1) @Ini = FechaInicio, @Fin = FechaFin FROM dbo.HE_Periodo ORDER BY FechaInicio DESC;
IF @Ini IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC dbo.Sp_RTA_HeFueraDelModulo @FechaInicio = @Ini, @FechaFin = @Fin;
    END TRY
    BEGIN CATCH
        PRINT 'FALLO: Sp_RTA_HeFueraDelModulo fallo al ejecutarse: ' + ERROR_MESSAGE();
        SET @Fallos += 1;
    END CATCH
END

IF @Fallos = 0
    PRINT 'OK: procedimientos de personas fuera del modulo listos.';
ELSE
    PRINT 'VEREDICTO: FALLO (' + CAST(@Fallos AS VARCHAR(10)) + ' comprobaciones). Revisar arriba.';
GO
