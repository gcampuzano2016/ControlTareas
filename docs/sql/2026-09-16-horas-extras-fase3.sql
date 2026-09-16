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
        -- Cerrar | Reabrir. 'ReabrirDenegado' no cabe aqui: lo ensancha el
        -- ALTER del punto 2b, que es el que corre sobre la tabla que ya
        -- existe en produccion. Esta definicion se deja como quedo creada.
        Accion             VARCHAR(10)  NOT NULL,
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

/* ------------------------- 2b. lo que el cierre tiene que congelar --------- */
/* Todo lo de aqui va por ALTER y no dentro del CREATE TABLE de arriba: la
   tabla YA existe en produccion, asi que un cambio escrito solo en el CREATE
   nunca le llegaria. Cada uno comprueba antes si hace falta, para que correr
   el script de nuevo no falle.

   1) Accion se ensancha porque 'ReabrirDenegado' -el punto 5b- son 15
      caracteres y la columna nacio con 10. SQL Server no trunca en silencio:
      el INSERT reventaria con "String or binary data would be truncated".

   2) FilasCerradas y TotalHECerrado existen porque un periodo cerrado NO
      tiene cifras congeladas en ninguna parte. Se puede reabrir, corregir un
      sueldo, volver a actualizar -lo que recalcula el dinero- y cerrar otra
      vez; el segundo cierre pisa FechaCierre y UsuarioCierre del primero, y
      HE_Detalle ya no contiene lo que se exporto la primera vez. Sin estas
      dos columnas, a la pregunta "el archivo de nomina de octubre, es esto?"
      no hay forma de responder. Con ellas, cada cierre deja dicho cuantas
      filas y cuanto dinero habia en el instante en que se cerro.

      No son la nomina completa -no reconstruyen fila por fila-, pero si son
      la huella con la que se comprueba si un archivo exportado corresponde a
      un cierre concreto o a otro.

      En la reapertura quedan NULL a proposito: una reapertura no congela
      nada, y un 0 ahi se leeria como "se cerro con cero filas". */
IF EXISTS (SELECT 1 FROM sys.columns
            WHERE object_id = OBJECT_ID('dbo.HE_PeriodoAuditoria')
              AND name = 'Accion' AND max_length < 20)
BEGIN
    ALTER TABLE dbo.HE_PeriodoAuditoria ALTER COLUMN Accion VARCHAR(20) NOT NULL;
    PRINT 'HE_PeriodoAuditoria: Accion ensanchada a VARCHAR(20).';
END
ELSE
    PRINT 'HE_PeriodoAuditoria: Accion ya admitia 20 caracteres.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_PeriodoAuditoria') AND name = 'FilasCerradas')
BEGIN
    ALTER TABLE dbo.HE_PeriodoAuditoria ADD FilasCerradas INT NULL;
    PRINT 'HE_PeriodoAuditoria: columna FilasCerradas agregada.';
END
ELSE
    PRINT 'HE_PeriodoAuditoria: FilasCerradas ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_PeriodoAuditoria') AND name = 'TotalHECerrado')
BEGIN
    ALTER TABLE dbo.HE_PeriodoAuditoria ADD TotalHECerrado DECIMAL(18,2) NULL;
    PRINT 'HE_PeriodoAuditoria: columna TotalHECerrado agregada.';
END
ELSE
    PRINT 'HE_PeriodoAuditoria: TotalHECerrado ya existia.';
GO

/* --------------------------- 3. guardar fila, ahora con auditoria ---------- */
IF OBJECT_ID('dbo.Sp_RTA_HeGuardarFila','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeGuardarFila;
GO
/* Identico al de la fase 2 salvo por @Auditar y el bloque que registra los
   cambios. Lo llaman dos caminos:
     GuardarHoras  -> @Auditar = 1, es una persona editando una fila
     AbrirPeriodo  -> @Auditar = 0, son decenas de filas de golpe al abrir o
                      reabrir. Pasa 0 por intencion, no por necesidad: como se
                      explica mas abajo, con 1 tampoco escribiria nada.

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

   Sobre @Auditar = 1 por omision: la pregunta es que pasa cuando alguien
   llama a este procedimiento SIN pasar el parametro, y la respuesta tiene un
   lado barato y un lado caro.

   Con 1 por omision, el olvidadizo audita de mas. En este diseno, "de mas"
   es exactamente nada: el bloque de auditoria solo escribe cuando cambia
   Horas50, Horas100 u Observacion, y son los tres unicos campos que una
   apertura de periodo NO toca. AbrirPeriodo los copia verbatim de la fila
   que ya estaba -fila.Horas50 = anterior.Horas50, y lo mismo con Horas100 y
   Observacion- y el calculo que corre despues solo reescribe los derivados
   -divisor, valores hora, totales-. Una apertura de periodo, por lo tanto,
   no puede escribir ni una fila de auditoria aunque se le pase @Auditar = 1:
   no hay ruido que evitar. Las filas nuevas tampoco, porque entran por el
   INSERT y el bloque de auditoria vive solo en la rama del UPDATE.

   Con 0 por omision, el olvidadizo pierde el rastro en silencio: la fila se
   guarda igual, no hay error, y el cambio de horas simplemente no queda
   registrado. Nadie se entera hasta la disputa de nomina en la que ese
   rastro hacia falta. Ese es el lado caro, y no es simetrico con el otro.

   La ventana de despliegue lo decide. Los scripts van antes que los
   binarios, asi que hay un rato en el que el DAO viejo -que no conoce este
   parametro- sigue llamando con 22 argumentos mientras el sitio esta arriba
   y una persona de Nomina digita horas. Con 0 esas ediciones se perderian de
   la auditoria; con 1 quedan registradas. Lo mismo vale al reves si alguna
   vez se revierten los binarios dejando este procedimiento en su sitio.

   Por eso 1 es el valor seguro: el peor caso del olvido es no escribir nada,
   y el mejor es salvar justo las ediciones de la ventana. Aun asi
   GuardarHoras pasa @Auditar = 1 explicito y AbrirPeriodo pasa 0 explicito:
   el valor por omision es una red, no el contrato. */
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
    @Auditar                 BIT = 1
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
               -4 faltan filas, -3 hay filas con valor hora en cero.

   El -3 es la validacion del 6 funcional: una fila cuyo valor hora salio en
   cero es un dato malo -falta el sueldo- y cerrar el mes con ella dejaria
   congelado un pago que nadie calculo.

   El -4 cubre el agujero que dejaba el -3: el -3 mira la CALIDAD de las filas
   que hay, no que esten TODAS. Armar un periodo son decenas de escrituras y
   AbrirPeriodo contempla que alguna falle -por eso avisa "N fila(s) no se
   pudieron guardar" y pide volver a abrir-. Si nadie vuelve a pulsar
   "Abrir/actualizar", el cierre pasaba sin decir una palabra y esas personas
   no cobraban el mes. Un periodo con cero filas tambien cerraba limpio.

   El -4 va ANTES del -3 a proposito: no tiene sentido discutir la calidad de
   las filas si faltan filas. Y devuelve en FilasConProblema cuantas faltan,
   no cuantas hay, que es el numero que le sirve a quien lo lee.

   El cierre queda registrado en HE_PeriodoAuditoria, en el mismo camino que
   hace el UPDATE de HE_Periodo: el INSERT va justo despues, antes del unico
   SELECT final que devuelve exito. Los cuatro RETURN de arriba -periodo
   inexistente, no Abierto, filas que faltan, filas en cero- salen del
   procedimiento sin pasar por ahi, asi que nunca se audita un cierre que no
   ocurrio.

   Las cuatro ramas de error y la de exito devuelven la MISMA forma de result
   set -Respuestas y FilasConProblema-: el DAO lee las dos columnas siempre y
   una rama que devolviera solo una lo romperia. */
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

    /* Cuantos colaboradores activos NO tienen fila en este periodo.

       Se cuenta con NOT EXISTS y no restando dos COUNT(*). La resta contesta
       otra pregunta: si el periodo quedo con filas de alguien que despues se
       desactivo, los dos conteos pueden empatar -o el de HE_Detalle salir
       mayor- estando alguien sin fila igualmente, y la resta daria 0 o
       negativo justo en el caso que hay que detectar. El anti-join nombra a
       los que faltan uno por uno, que es literalmente lo que se pregunta.

       Aqui no se filtra por AplicaHE: quien no aplica tambien necesita su
       fila -la pantalla lo lista y el cierre lo congela con total 0-, y su
       ausencia sigue siendo un snapshot incompleto. */
    DECLARE @Faltantes INT;
    SELECT @Faltantes = COUNT(*)
      FROM dbo.HE_ColaboradorParametro c
     WHERE c.Estado = '1'
       AND NOT EXISTS (SELECT 1 FROM dbo.HE_Detalle d
                        WHERE d.IdPeriodo = @IdPeriodo AND d.IdEmpleado = c.IdEmpleado);

    IF @Faltantes > 0
    BEGIN
        SELECT Respuestas = -4, FilasConProblema = @Faltantes;
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

    /* La huella de ESTE cierre. Se calcula DESPUES del UPDATE, no antes: una
       vez el periodo esta en 'Cerrado', Sp_RTA_HeGuardarFila rechaza con -2
       cualquier edicion, asi que a partir de esa linea HE_Detalle ya no se
       puede mover y lo que se cuenta es de verdad lo que quedo cerrado.
       Calculandolo antes, una edicion que entrara en medio haria que la
       huella no correspondiera a nada.

       ISNULL en la suma porque SUM() sobre cero filas da NULL, no 0. Cero
       filas ya no deberia llegar hasta aqui -lo ataja el -4-, pero la
       diferencia entre "cerro con 0 dolares" y "no se sabe" no puede
       depender de que otra validacion siga en su sitio. */
    DECLARE @FilasCerradas INT, @TotalHECerrado DECIMAL(18,2);
    SELECT @FilasCerradas = COUNT(*), @TotalHECerrado = ISNULL(SUM(TotalHE), 0)
      FROM dbo.HE_Detalle
     WHERE IdPeriodo = @IdPeriodo;

    INSERT INTO dbo.HE_PeriodoAuditoria
        (IdPeriodo, Accion, EstadoAnterior, EstadoNuevo, Usuario, Ip,
         FilasCerradas, TotalHECerrado)
    VALUES
        (@IdPeriodo, 'Cerrar', @Estado, 'Cerrado', @Usuario, @Ip,
         @FilasCerradas, @TotalHECerrado);

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
   quien lo uso.

   FilasCerradas y TotalHECerrado se dejan fuera del INSERT -quedan NULL- a
   proposito: una reapertura no congela nada. Solo un cierre tiene cifras que
   valga la pena guardar. */
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

/* ------------------------- 5b. la reapertura que NO ocurrio ---------------- */
IF OBJECT_ID('dbo.Sp_RTA_HeRegistrarReaperturaDenegada','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeRegistrarReaperturaDenegada;
GO
/* Respuestas: 0 registrado, -1 el periodo no existe -y aun asi se registro-.

   Solo el perfil 18 puede reabrir un periodo y el handler rechaza al resto
   antes de llegar a Sp_RTA_HeReabrirPeriodo. Ese rechazo no quedaba en
   ninguna parte, y es precisamente el evento que la barrera existe para
   detectar: una reapertura autorizada es rutina de Nomina; un intento
   denegado es alguien pulsando un boton que no le toca, o alguien fabricando
   la peticion a mano. El segundo es el que interesa y era el unico invisible.

   Esto NO decide nada: no comprueba perfiles -la base no los conoce- ni
   bloquea. Es solo el renglon del registro, y lo llama el handler en el
   camino de rechazo. Por eso no tiene rama de "ese periodo si se podia
   reabrir": aqui ya se sabe que no se reabrio.

   EstadoAnterior y EstadoNuevo llevan los dos el estado REAL del periodo, y
   son iguales porque no cambio nada. Es la lectura correcta de la columna:
   "de que estado a que estado paso esto" -de ninguno a ninguno-.

   El periodo inexistente no revienta ni se descarta. Quien manda un
   IdPeriodo inventado es el caso MAS sospechoso de todos: si se ignorara en
   silencio, la unica peticion que con seguridad no salio de la pantalla
   seria tambien la unica sin rastro. Se registra con 'NoExiste' en los dos
   estados -EstadoAnterior y EstadoNuevo son NOT NULL, no admiten dejarlo en
   blanco- y se devuelve -1 para que el llamador pueda distinguirlo. No hay
   clave foranea contra HE_Periodo -el modulo entero no las usa-, asi que el
   IdPeriodo inventado no rompe nada; queda como el dato que es. */
CREATE PROCEDURE dbo.Sp_RTA_HeRegistrarReaperturaDenegada
    @IdPeriodo INT,
    @Usuario   VARCHAR(50),
    @Ip        VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Estado VARCHAR(10);
    SELECT @Estado = EstadoPeriodo FROM dbo.HE_Periodo WHERE IdPeriodo = @IdPeriodo;

    DECLARE @EstadoRegistrado VARCHAR(10) = ISNULL(@Estado, 'NoExiste');

    INSERT INTO dbo.HE_PeriodoAuditoria (IdPeriodo, Accion, EstadoAnterior, EstadoNuevo, Usuario, Ip)
    VALUES (@IdPeriodo, 'ReabrirDenegado', @EstadoRegistrado, @EstadoRegistrado, @Usuario, @Ip);

    IF @Estado IS NULL
    BEGIN
        SELECT Respuestas = -1;
        RETURN;
    END

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

IF OBJECT_ID('dbo.Sp_RTA_HeRegistrarReaperturaDenegada','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeRegistrarReaperturaDenegada no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

/* Sin estas tres, el modulo compila y corre pero miente en silencio: el
   registro de reaperturas denegadas reventaria por truncamiento y los
   cierres se auditarian sin sus cifras. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_PeriodoAuditoria')
                  AND name = 'Accion' AND max_length >= 20)
BEGIN
    RAISERROR('FALLO: HE_PeriodoAuditoria.Accion no admite 20 caracteres.', 16, 1);
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_PeriodoAuditoria') AND name = 'FilasCerradas')
BEGIN
    RAISERROR('FALLO: HE_PeriodoAuditoria.FilasCerradas no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_PeriodoAuditoria') AND name = 'TotalHECerrado')
BEGIN
    RAISERROR('FALLO: HE_PeriodoAuditoria.TotalHECerrado no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

/* El parametro nuevo tiene que existir, o las llamadas de 22 parametros que
   ya hay en produccion dejarian de funcionar.

   Esta asercion comprobaba ANTES tambien has_default_value = 1, y fallaba
   siempre. No por un defecto del procedimiento: sys.parameters.has_default_value
   solo se llena para objetos CLR y vale 0 para TODOS los parametros de un
   procedimiento T-SQL, tengan o no valor por omision -se puede ver comparando
   @Auditar con @IdPeriodo, que no tiene ninguno y tambien sale 0-.

   Que el valor por omision funciona no se puede afirmar desde el catalogo:
   se comprueba llamando al procedimiento con 22 parametros. Sobre un
   IdPeriodo inexistente devuelve Respuestas = -1 sin tocar ninguna fila, asi
   que es una prueba inofensiva:

     EXEC dbo.Sp_RTA_HeGuardarFila @IdPeriodo=-999, @IdEmpleado=-999, ... ,
          @Usuario='prueba', @Ip='';

   Si el default faltara, esa llamada daria "expects parameter '@Auditar'"
   en vez de -1. */
IF NOT EXISTS (SELECT 1 FROM sys.parameters
                WHERE object_id = OBJECT_ID('dbo.Sp_RTA_HeGuardarFila')
                  AND name = '@Auditar')
BEGIN
    RAISERROR('FALLO: el parametro @Auditar no quedo en Sp_RTA_HeGuardarFila.', 16, 1);
    SET @Fallos += 1;
END

IF @Fallos > 0
    RAISERROR('FALLO: %d verificaciones no pasaron.', 16, 1, @Fallos);
ELSE
    PRINT 'Horas Extras fase 3: auditoria y cierre listos.';
GO
