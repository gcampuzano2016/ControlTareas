/* ============================================================================
   Horas Extras - fase 2: los procedimientos de la pantalla.

   Ninguno calcula. El valor hora y los totales los calcula NegHorasExtras en
   C#, que es la unica implementacion de la formula; estos procedimientos
   entregan insumos y guardan resultados ya calculados.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* --------------------------------------------------------- 1. periodos ---- */
IF OBJECT_ID('dbo.Sp_RTA_HeListarPeriodos','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeListarPeriodos;
GO
CREATE PROCEDURE dbo.Sp_RTA_HeListarPeriodos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdPeriodo, Anio, Mes, Descripcion, EstadoPeriodo,
           FechaCierre, UsuarioCierre
      FROM dbo.HE_Periodo
     ORDER BY Anio DESC, Mes DESC;
END
GO

/* ----------------------------------------------------- 2. crear periodo --- */
IF OBJECT_ID('dbo.Sp_RTA_HeCrearPeriodo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeCrearPeriodo;
GO
CREATE PROCEDURE dbo.Sp_RTA_HeCrearPeriodo
    @Anio        INT,
    @Mes         INT,
    @Usuario     VARCHAR(50),
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Anio < 2020 OR @Anio > 2100 OR @Mes < 1 OR @Mes > 12
    BEGIN
        SELECT Respuestas = -1, IdPeriodo = 0;
        RETURN;
    END

    DECLARE @IdPeriodo INT;

    SELECT @IdPeriodo = IdPeriodo FROM dbo.HE_Periodo WHERE Anio = @Anio AND Mes = @Mes;

    /* Ya existe: no se toca. Devolverlo tal cual es lo que hace que abrir dos
       veces el mismo mes sea inofensivo. */
    IF @IdPeriodo IS NOT NULL
    BEGIN
        SELECT Respuestas = 0, IdPeriodo = @IdPeriodo;
        RETURN;
    END

    INSERT INTO dbo.HE_Periodo (Anio, Mes, Descripcion, EstadoPeriodo, UsuarioCreacion, Ip_Modificacion)
    VALUES (@Anio, @Mes,
            DATENAME(MONTH, DATEFROMPARTS(@Anio, @Mes, 1)) + ' ' + CONVERT(VARCHAR(4), @Anio),
            'Abierto', @Usuario, @Ip);

    SELECT Respuestas = 0, IdPeriodo = SCOPE_IDENTITY();
END
GO

/* ---------------------------------------------------------- 3. insumos ---- */
IF OBJECT_ID('dbo.Sp_RTA_HeInsumos','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeInsumos;
GO
/* Materia prima para armar el snapshot de un periodo.

   Dos result sets, en este orden:
     1. un renglon por colaborador con lo que Empleados y HE_ColaboradorParametro
        saben de el
     2. TODO el historial de sueldos de esos colaboradores, sin resolver cual
        rige: eso lo decide NegHorasExtras.SalarioVigente, que a igual fecha de
        vigencia da prioridad al ajuste sobre el rol. Resolverlo aqui seria
        escribir esa regla por segunda vez en otro lenguaje. */
CREATE PROCEDURE dbo.Sp_RTA_HeInsumos
    @FechaCorte DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.IdEmpleado,
           Cedula  = LTRIM(RTRIM(ISNULL(e.Cedula, ''))),
           Nombre  = LTRIM(RTRIM(ISNULL(e.Nombre, ''))),
           Cargo   = ISNULL(c.Cargo, ''),
           Empresa = ISNULL(c.Empresa, ''),
           c.JornadaHorasDia,
           c.DivisorManual,
           c.AplicaHE,
           MotivoNoAplica = ISNULL(c.MotivoNoAplica, '')
      FROM dbo.HE_ColaboradorParametro c
      JOIN dbo.Empleados e ON e.IdEmpleado = c.IdEmpleado
     WHERE c.Estado = '1'
     ORDER BY e.Nombre;

    SELECT s.IdEmpleado, s.Monto, s.FechaVigenciaDesde, Origen = ISNULL(s.Origen, '')
      FROM dbo.HE_Salario s
      JOIN dbo.HE_ColaboradorParametro c ON c.IdEmpleado = s.IdEmpleado AND c.Estado = '1'
     WHERE s.Estado = '1' AND s.FechaVigenciaDesde <= @FechaCorte
     ORDER BY s.IdEmpleado, s.FechaVigenciaDesde;
END
GO

/* ------------------------------------------------------ 4. guardar fila --- */
IF OBJECT_ID('dbo.Sp_RTA_HeGuardarFila','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeGuardarFila;
GO
/* Inserta o actualiza UNA fila de HE_Detalle con valores YA calculados.

   Lo usan dos caminos: armar el snapshot al abrir un periodo, y guardar una
   edicion. Por eso recibe el snapshot completo y no solo las horas: al abrir,
   no hay fila que actualizar.

   Respuestas: 0 bien, -1 el periodo no existe, -2 el periodo no esta Abierto. */
CREATE PROCEDURE dbo.Sp_RTA_HeGuardarFila
    @IdPeriodo               INT,
    @IdEmpleado              BIGINT,
    @CedulaSnapshot          VARCHAR(20),
    @NombreSnapshot          VARCHAR(400),
    @EmpresaSnapshot         VARCHAR(120),
    @CargoSnapshot           VARCHAR(200),
    @JornadaHorasDiaSnapshot INT,
    @SalarioBaseSnapshot     DECIMAL(18,2),
    @AplicaHESnapshot        BIT,
    @Divisor                 INT,
    @ValorHoraOrdinaria      DECIMAL(18,6),
    @ValorHora50             DECIMAL(18,6),
    @ValorHora100            DECIMAL(18,6),
    @Horas50                 DECIMAL(9,2),
    @Horas100                DECIMAL(9,2),
    @Total50                 DECIMAL(18,2),
    @Total100                DECIMAL(18,2),
    @TotalHoras              DECIMAL(9,2),
    @TotalHE                 DECIMAL(18,2),
    @Observacion             VARCHAR(400),
    @Usuario                 VARCHAR(50),
    @Ip                      VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Estado VARCHAR(10);
    SELECT @Estado = EstadoPeriodo FROM dbo.HE_Periodo WHERE IdPeriodo = @IdPeriodo;

    IF @Estado IS NULL
    BEGIN
        SELECT Respuestas = -1;
        RETURN;
    END

    IF @Estado <> 'Abierto'
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.HE_Detalle
       SET CedulaSnapshot = @CedulaSnapshot, NombreSnapshot = @NombreSnapshot,
           EmpresaSnapshot = @EmpresaSnapshot, CargoSnapshot = @CargoSnapshot,
           JornadaHorasDiaSnapshot = @JornadaHorasDiaSnapshot,
           SalarioBaseSnapshot = @SalarioBaseSnapshot, AplicaHESnapshot = @AplicaHESnapshot,
           Divisor = @Divisor, ValorHoraOrdinaria = @ValorHoraOrdinaria,
           ValorHora50 = @ValorHora50, ValorHora100 = @ValorHora100,
           Horas50 = @Horas50, Horas100 = @Horas100,
           Total50 = @Total50, Total100 = @Total100,
           TotalHoras = @TotalHoras, TotalHE = @TotalHE,
           Observacion = @Observacion,
           Fec_Modificacion = SYSDATETIME(), Usu_Modificacion = @Usuario, Ip_Modificacion = @Ip
     WHERE IdPeriodo = @IdPeriodo AND IdEmpleado = @IdEmpleado;

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO dbo.HE_Detalle
            (IdPeriodo, IdEmpleado, CedulaSnapshot, NombreSnapshot, EmpresaSnapshot,
             CargoSnapshot, JornadaHorasDiaSnapshot, SalarioBaseSnapshot, AplicaHESnapshot,
             Divisor, ValorHoraOrdinaria, ValorHora50, ValorHora100,
             Horas50, Horas100, Total50, Total100, TotalHoras, TotalHE,
             Observacion, Usu_Modificacion, Ip_Modificacion)
        VALUES
            (@IdPeriodo, @IdEmpleado, @CedulaSnapshot, @NombreSnapshot, @EmpresaSnapshot,
             @CargoSnapshot, @JornadaHorasDiaSnapshot, @SalarioBaseSnapshot, @AplicaHESnapshot,
             @Divisor, @ValorHoraOrdinaria, @ValorHora50, @ValorHora100,
             @Horas50, @Horas100, @Total50, @Total100, @TotalHoras, @TotalHE,
             @Observacion, @Usuario, @Ip);
    END

    SELECT Respuestas = 0;
END
GO

/* ----------------------------------------------------- 5. cargar periodo -- */
IF OBJECT_ID('dbo.Sp_RTA_HeCargarPeriodo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeCargarPeriodo;
GO
/* Dos result sets, en este orden:
     1. la cabecera del periodo (0 filas si no existe)
     2. sus filas de detalle */
CREATE PROCEDURE dbo.Sp_RTA_HeCargarPeriodo
    @IdPeriodo INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdPeriodo, Anio, Mes, Descripcion, EstadoPeriodo, FechaCierre, UsuarioCierre
      FROM dbo.HE_Periodo WHERE IdPeriodo = @IdPeriodo;

    SELECT d.IdDetalle, d.IdEmpleado, d.CedulaSnapshot, d.NombreSnapshot,
           d.EmpresaSnapshot, d.CargoSnapshot, d.JornadaHorasDiaSnapshot,
           d.SalarioBaseSnapshot, d.AplicaHESnapshot, d.Divisor,
           d.ValorHoraOrdinaria, d.ValorHora50, d.ValorHora100,
           d.Horas50, d.Horas100, d.Total50, d.Total100, d.TotalHoras, d.TotalHE,
           Observacion = ISNULL(d.Observacion, ''),
           MotivoNoAplica = ISNULL(c.MotivoNoAplica, '')
      FROM dbo.HE_Detalle d
      LEFT JOIN dbo.HE_ColaboradorParametro c ON c.IdEmpleado = d.IdEmpleado
     WHERE d.IdPeriodo = @IdPeriodo
     ORDER BY d.NombreSnapshot;
END
GO

PRINT 'Horas Extras fase 2: los cinco procedimientos quedaron creados.';
GO
