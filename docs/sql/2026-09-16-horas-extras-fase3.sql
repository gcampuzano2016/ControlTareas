/* ============================================================================
   Horas Extras - fase 3: la auditoria y el cierre del periodo.

   La auditoria se escribe DENTRO de Sp_RTA_HeGuardarFila y no desde C#: si se
   escribiera despues de guardar, un fallo entre las dos operaciones dejaria un
   cambio de horas sin rastro, y el rastro es justo lo que el 8 funcional exige
   que no falte.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------ 1. la auditoria admite texto --------- */
/* ValorAnterior y ValorNuevo son DECIMAL(9,2) y sirven para Horas50 y Horas100.
   Observacion tambien es editable desde la pantalla y es texto: sin estas dos
   columnas no hay donde registrar su cambio. Se anaden en vez de ensanchar las
   existentes para no perder el tipo de las que si son numericas. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_DetalleAuditoria') AND name = 'TextoAnterior')
BEGIN
    ALTER TABLE dbo.HE_DetalleAuditoria ADD TextoAnterior VARCHAR(400) NULL;
    PRINT 'HE_DetalleAuditoria: columna TextoAnterior agregada.';
END
ELSE
    PRINT 'HE_DetalleAuditoria: TextoAnterior ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_DetalleAuditoria') AND name = 'TextoNuevo')
BEGIN
    ALTER TABLE dbo.HE_DetalleAuditoria ADD TextoNuevo VARCHAR(400) NULL;
    PRINT 'HE_DetalleAuditoria: columna TextoNuevo agregada.';
END
ELSE
    PRINT 'HE_DetalleAuditoria: TextoNuevo ya existia.';
GO

/* Para responder "que le paso a esta fila" sin recorrer la tabla entera. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.HE_DetalleAuditoria') AND name = 'IX_HE_DetalleAuditoria_Detalle_Fecha')
BEGIN
    CREATE INDEX IX_HE_DetalleAuditoria_Detalle_Fecha
        ON dbo.HE_DetalleAuditoria (IdDetalle, Fecha DESC);
    PRINT 'HE_DetalleAuditoria: indice por detalle y fecha creado.';
END
GO

/* ------------------------------- 2. la auditoria del periodo --------------- */
/* Pocas filas -una por cierre y una por reapertura, no una por colaborador-.

   Se crea porque acabamos de restringir quien puede reabrir un periodo -solo
   el perfil 18, decision de la capa web- y una restriccion sin registro es
   medio control: impide, pero no deja saber quien la uso ni cuando. Sin esta
   tabla, HE_Periodo solo guarda el ULTIMO cierre: cerrar el dia A con un
   usuario, reabrir, y cerrar de nuevo el dia B con otro usuario borra todo
   rastro del primer cierre. Esta tabla es la que preserva la historia
   completa, cierre por cierre y reapertura por reapertura.

   Estilo igual al resto del modulo: IDENTITY, constraints con nombre,
   DATETIME2(0) con SYSDATETIME() por omision, y sin claves foraneas -el
   modulo entero no las usa-. */
IF OBJECT_ID('dbo.HE_PeriodoAuditoria','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_PeriodoAuditoria
    (
        IdPeriodoAuditoria INT IDENTITY(1,1) NOT NULL,
        IdPeriodo          INT          NOT NULL,
        Accion             VARCHAR(10)  NOT NULL,   -- Cerrar | Reabrir
        EstadoAnterior     VARCHAR(10)  NOT NULL,
        EstadoNuevo        VARCHAR(10)  NOT NULL,
        Fecha              DATETIME2(0) NOT NULL
            CONSTRAINT DF_HePeriodoAud_Fecha DEFAULT (SYSDATETIME()),
        Usuario            VARCHAR(50)  NULL,
        Ip                 VARCHAR(64)  NULL,

        CONSTRAINT PK_HE_PeriodoAuditoria PRIMARY KEY (IdPeriodoAuditoria)
    );
    CREATE INDEX IX_HE_PeriodoAuditoria_Periodo_Fecha
        ON dbo.HE_PeriodoAuditoria (IdPeriodo, Fecha DESC);
    PRINT 'HE_PeriodoAuditoria creada.';
END
ELSE PRINT 'HE_PeriodoAuditoria ya existia.';
GO

/* --------------------------- 3. guardar fila, ahora con auditoria ---------- */
IF OBJECT_ID('dbo.Sp_RTA_HeGuardarFila','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeGuardarFila;
GO
/* Identico al de la fase 2 salvo por @Auditar y el bloque que registra los
   cambios. Lo llaman dos caminos y solo uno audita:
     GuardarHoras  -> @Auditar = 1, es una persona editando una fila
     AbrirPeriodo  -> @Auditar = 0, son 62 filas de golpe al abrir o reabrir;
                      registrarlas enterraria los cambios reales bajo el ruido.

   El UPDATE sigue yendo primero y el INSERT solo si @@ROWCOUNT = 0, igual que
   en produccion: no se ensancha la ventana de check-then-act agregando una
   lectura previa. Los valores de ANTES se capturan con la clausula OUTPUT del
   propio UPDATE -deleted.*-, que es atomica y no cuesta una consulta aparte.

   Horas50 y Horas100 son NOT NULL en HE_Detalle, asi que se comparan
   directo, sin ISNULL: un centinela como -1 seria peligroso aqui porque -1 es
   un valor que si se puede guardar (la fase 2 decidio persistir horas
   negativas, marcandolas solo con advertencia). Observacion si es nullable de
   verdad, y ahi ISNULL(..., '') si hace falta para no confundir NULL con ''.

   Respuestas: 0 bien, -1 el periodo no existe, -2 el periodo no esta Abierto.
   Devuelve ademas IdDetalle, que es con lo que se escribe la auditoria.

   Sobre @Auditar = 0 por omision: los scripts SQL se despliegan antes que los
   binarios, asi que hay una ventana en la que el DAO viejo -sin este
   parametro- sigue llamando a este procedimiento. Con el valor por omision en
   0 esa ventana no audita nada, lo cual es inofensivo porque en esa ventana
   todavia nadie esta editando con el codigo nuevo. Si el valor por omision
   fuera 1, esa misma ventana escribiria auditoria de mas en cada apertura de
   periodo -62 filas de ruido por AbrirPeriodo, que nunca manda @Auditar=1-.
   Quien llame sin pasar @Auditar por accion u omision obtiene entonces
   comportamiento de AbrirPeriodo -sin auditoria-, nunca al reves: si el
   llamador olvidadizo fuera GuardarHoras, la fila se guardaria igual, pero
   el cambio quedaria sin rastro y sin ningun error que lo delate. Por
   eso GuardarHoras debe pasar @Auditar = 1 explicito; no hay forma de que la
   base lo detecte por si sola. */
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
    @Ip                      VARCHAR(64),
    @Auditar                 BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Estado VARCHAR(10);
    SELECT @Estado = EstadoPeriodo FROM dbo.HE_Periodo WHERE IdPeriodo = @IdPeriodo;

    IF @Estado IS NULL
    BEGIN
        SELECT Respuestas = -1, IdDetalle = CAST(NULL AS INT);
        RETURN;
    END

    IF @Estado <> 'Abierto'
    BEGIN
        SELECT Respuestas = -2, IdDetalle = CAST(NULL AS INT);
        RETURN;
    END

    DECLARE @IdDetalle INT;

    /* Aqui caen los valores de ANTES, uno por cada fila que el UPDATE toque
       -0 o 1, por el indice unico de IdPeriodo+IdEmpleado-. */
    DECLARE @Previos TABLE (
        IdDetalle    INT,
        Horas50Antes DECIMAL(9,2),
        Horas100Antes DECIMAL(9,2),
        ObsAntes     VARCHAR(400)
    );

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
       OUTPUT deleted.IdDetalle, deleted.Horas50, deleted.Horas100, deleted.Observacion
         INTO @Previos (IdDetalle, Horas50Antes, Horas100Antes, ObsAntes)
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

        SET @IdDetalle = CAST(SCOPE_IDENTITY() AS INT);
    END
    ELSE
    BEGIN
        DECLARE @Horas50Antes DECIMAL(9,2), @Horas100Antes DECIMAL(9,2), @ObsAntes VARCHAR(400);

        SELECT @IdDetalle = IdDetalle, @Horas50Antes = Horas50Antes,
               @Horas100Antes = Horas100Antes, @ObsAntes = ObsAntes
          FROM @Previos;

        /* Una fila por campo que de verdad cambio. Los tres INSERT son
           independientes a proposito: si alguien cambia las horas al 50% y la
           observacion en el mismo guardado, son dos hechos distintos y se
           registran por separado. */
        IF @Auditar = 1
        BEGIN
            IF @Horas50Antes <> @Horas50
                INSERT INTO dbo.HE_DetalleAuditoria (IdDetalle, Campo, ValorAnterior, ValorNuevo, Usuario, Ip)
                VALUES (@IdDetalle, 'Horas50', @Horas50Antes, @Horas50, @Usuario, @Ip);

            IF @Horas100Antes <> @Horas100
                INSERT INTO dbo.HE_DetalleAuditoria (IdDetalle, Campo, ValorAnterior, ValorNuevo, Usuario, Ip)
                VALUES (@IdDetalle, 'Horas100', @Horas100Antes, @Horas100, @Usuario, @Ip);

            IF ISNULL(@ObsAntes, '') <> ISNULL(@Observacion, '')
                INSERT INTO dbo.HE_DetalleAuditoria (IdDetalle, Campo, TextoAnterior, TextoNuevo, Usuario, Ip)
                VALUES (@IdDetalle, 'Observacion', @ObsAntes, @Observacion, @Usuario, @Ip);
        END
    END

    SELECT Respuestas = 0, IdDetalle = @IdDetalle;
END
GO

/* ----------------------------------------- 4. cerrar el periodo ------------ */
IF OBJECT_ID('dbo.Sp_RTA_HeCerrarPeriodo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeCerrarPeriodo;
GO
/* Respuestas: 0 cerrado, -1 no existe, -2 no estaba Abierto,
               -3 hay filas con valor hora en cero.

   El -3 es la validacion del 6 funcional: una fila cuyo valor hora salio en
   cero es un dato malo -falta el sueldo- y cerrar el mes con ella dejaria
   congelado un pago que nadie calculo.

   El cierre queda registrado en HE_PeriodoAuditoria, en el mismo camino que
   hace el UPDATE de HE_Periodo: el INSERT va justo despues, antes del unico
   SELECT final que devuelve exito. Los tres RETURN de arriba -periodo
   inexistente, no Abierto, filas en cero- salen del procedimiento sin pasar
   por ahi, asi que nunca se audita un cierre que no ocurrio. */
CREATE PROCEDURE dbo.Sp_RTA_HeCerrarPeriodo
    @IdPeriodo INT,
    @Usuario   VARCHAR(50),
    @Ip        VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Estado VARCHAR(10);
    SELECT @Estado = EstadoPeriodo FROM dbo.HE_Periodo WHERE IdPeriodo = @IdPeriodo;

    IF @Estado IS NULL
    BEGIN
        SELECT Respuestas = -1, FilasConProblema = 0;
        RETURN;
    END

    IF @Estado <> 'Abierto'
    BEGIN
        SELECT Respuestas = -2, FilasConProblema = 0;
        RETURN;
    END

    /* Solo cuentan las filas de quien SI aplica horas extras: a quien no
       aplica se le calcula el valor hora igual, pero no cobra, y bloquear el
       cierre por su culpa seria bloquearlo para siempre. */
    DECLARE @EnCero INT;
    SELECT @EnCero = COUNT(*)
      FROM dbo.HE_Detalle
     WHERE IdPeriodo = @IdPeriodo AND AplicaHESnapshot = 1 AND ValorHoraOrdinaria = 0;

    IF @EnCero > 0
    BEGIN
        SELECT Respuestas = -3, FilasConProblema = @EnCero;
        RETURN;
    END

    UPDATE dbo.HE_Periodo
       SET EstadoPeriodo = 'Cerrado', FechaCierre = SYSDATETIME(),
           UsuarioCierre = @Usuario, Ip_Modificacion = @Ip
     WHERE IdPeriodo = @IdPeriodo;

    INSERT INTO dbo.HE_PeriodoAuditoria (IdPeriodo, Accion, EstadoAnterior, EstadoNuevo, Usuario, Ip)
    VALUES (@IdPeriodo, 'Cerrar', @Estado, 'Cerrado', @Usuario, @Ip);

    SELECT Respuestas = 0, FilasConProblema = 0;
END
GO

/* --------------------------------------- 5. reabrir el periodo ------------- */
IF OBJECT_ID('dbo.Sp_RTA_HeReabrirPeriodo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeReabrirPeriodo;
GO
/* Respuestas: 0 reabierto, -1 no existe, -2 no estaba Cerrado.

   Quien puede llamar a esto lo decide la capa web -solo el perfil 18-, no este
   procedimiento: la base no conoce perfiles. Aqui se comprueba solo que el
   periodo este en un estado desde el que reabrir tenga sentido.

   FechaCierre y UsuarioCierre NO se borran: son el rastro de que este mes
   estuvo cerrado alguna vez y quien lo cerro LA ULTIMA VEZ -si se reabre y se
   vuelve a cerrar, estas dos columnas quedan con el cierre mas reciente y el
   anterior se pierde de HE_Periodo-. La historia COMPLETA, cierre por cierre y
   reapertura por reapertura, esta en HE_PeriodoAuditoria: cada cierre y cada
   reapertura de este periodo, con su usuario, su ip y la fecha, en el orden en
   que ocurrieron. Restringir quien reabre a un solo perfil sin dejar este
   registro seria un control a medias: impediria pero no permitiria saber
   quien lo uso. */
CREATE PROCEDURE dbo.Sp_RTA_HeReabrirPeriodo
    @IdPeriodo INT,
    @Usuario   VARCHAR(50),
    @Ip        VARCHAR(64)
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

    IF @Estado <> 'Cerrado'
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.HE_Periodo
       SET EstadoPeriodo = 'Abierto', Ip_Modificacion = @Ip
     WHERE IdPeriodo = @IdPeriodo;

    INSERT INTO dbo.HE_PeriodoAuditoria (IdPeriodo, Accion, EstadoAnterior, EstadoNuevo, Usuario, Ip)
    VALUES (@IdPeriodo, 'Reabrir', @Estado, 'Abierto', @Usuario, @Ip);

    SELECT Respuestas = 0;
END
GO

/* ------------------------------------------------- 6. aserciones ----------- */

/* Todo este bloque va en un solo batch, sin GO en medio: un GO entre el
   DECLARE y su ultimo uso hace fallar el script entero con "must declare
   the scalar variable". Por eso va entero al final, despues del ultimo GO
   de arriba, y no intercalado entre cada CREATE PROCEDURE. */
DECLARE @Fallos INT = 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_DetalleAuditoria') AND name = 'TextoAnterior')
BEGIN
    RAISERROR('FALLO: HE_DetalleAuditoria.TextoAnterior no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_DetalleAuditoria') AND name = 'TextoNuevo')
BEGIN
    RAISERROR('FALLO: HE_DetalleAuditoria.TextoNuevo no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.HE_PeriodoAuditoria','U') IS NULL
BEGIN
    RAISERROR('FALLO: HE_PeriodoAuditoria no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeGuardarFila','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeGuardarFila no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeCerrarPeriodo','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeCerrarPeriodo no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeReabrirPeriodo','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeReabrirPeriodo no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

/* El parametro nuevo tiene que existir Y tener valor por omision, o las
   llamadas de 22 parametros que ya hay en produccion dejarian de funcionar. */
IF NOT EXISTS (SELECT 1 FROM sys.parameters
                WHERE object_id = OBJECT_ID('dbo.Sp_RTA_HeGuardarFila')
                  AND name = '@Auditar' AND has_default_value = 1)
BEGIN
    RAISERROR('FALLO: @Auditar no existe o no tiene valor por omision.', 16, 1);
    SET @Fallos += 1;
END

IF @Fallos > 0
    RAISERROR('FALLO: %d verificaciones no pasaron.', 16, 1, @Fallos);
ELSE
    PRINT 'Horas Extras fase 3: auditoria y cierre listos.';
GO
