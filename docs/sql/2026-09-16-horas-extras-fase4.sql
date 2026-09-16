/* ============================================================================
   Horas Extras fase 4 -- 2026-09-16

   1. HE_Periodo pasa de (Anio, Mes) a (FechaInicio, FechaFin).
   2. HE_Detalle gana HorasOrigen: de donde salieron las horas de la fila.
   3. Sp_RTA_HeHorasAprobadas agrega las horas aprobadas de R_DetTareasAranda.
   4. Los cuatro procedimientos que hablaban de mes pasan a hablar de rango.

   Re-correrlo es inofensivo: el guard de abajo lo salta entero.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ----------------------------------------------------------- GUARD -------- */
/* Si la fase 4 ya esta aplicada, este script NO debe volver a correr. No es
   solo cuestion de ruido: el paso 1 elimina Anio y Mes, y una segunda pasada
   fallaria a mitad del archivo dejando el esquema y los procedimientos en
   estados distintos.

   No hay forma de saltarse solo una seccion -CREATE PROCEDURE tiene que ser la
   primera instruccion de su lote y no se puede envolver en un IF-, asi que se
   salta el script entero con NOEXEC, igual que hace el de la fase 2.

   La senal es la columna FechaInicio: existe si y solo si el paso 1 corrio. */
IF EXISTS (SELECT 1 FROM sys.columns
            WHERE object_id = OBJECT_ID('dbo.HE_Periodo') AND name = 'FechaInicio')
BEGIN
    PRINT 'La fase 4 ya esta aplicada. No se hace nada.';
    SET NOEXEC ON;
END
GO

/* ------------------------------------------ 1. HE_Periodo: el rango ------- */

/* Las columnas entran NULL porque HE_Periodo ya tiene filas y no hay un valor
   por omision honesto que ponerles: el rango de cada periodo sale de su propio
   Anio/Mes, en el UPDATE de abajo. */
ALTER TABLE dbo.HE_Periodo ADD FechaInicio DATE NULL, FechaFin DATE NULL;
GO

UPDATE dbo.HE_Periodo
   SET FechaInicio = DATEFROMPARTS(Anio, Mes, 1),
       FechaFin    = EOMONTH(DATEFROMPARTS(Anio, Mes, 1))
 WHERE FechaInicio IS NULL;
GO

ALTER TABLE dbo.HE_Periodo ALTER COLUMN FechaInicio DATE NOT NULL;
ALTER TABLE dbo.HE_Periodo ALTER COLUMN FechaFin    DATE NOT NULL;
GO

/* El unico indice unico pasa a ser el del rango. El viejo tiene que irse antes
   que las columnas: un indice sobre Anio y Mes impide eliminarlas. */
DROP INDEX UX_HE_Periodo_AnioMes ON dbo.HE_Periodo;
CREATE UNIQUE INDEX UX_HE_Periodo_Rango ON dbo.HE_Periodo (FechaInicio, FechaFin);
GO

/* Anio y Mes se eliminan a proposito. Dejar las cuatro columnas seria tener dos
   respuestas para "que periodo es este", y la que manda seria distinta segun
   quien pregunte: la pantalla leeria el rango y cualquier consulta vieja
   seguiria leyendo el mes, sin que nada las obligue a coincidir. */
ALTER TABLE dbo.HE_Periodo DROP COLUMN Anio, Mes;
GO

/* --------------------------------------- 2. HE_Detalle: HorasOrigen ------- */

/* De donde salieron las horas de esta fila: 'Tareas' si las sembro la lectura
   de R_DetTareasAranda al abrir el periodo, 'Manual' si las escribio una
   persona. Es lo que permite re-sembrar sin pisar una correccion.

   Las 124 filas que ya existen quedan en 'Tareas' por el DEFAULT. Se comprobo
   en produccion el 2026-09-16 que estan TODAS en cero y sin observacion: nadie
   ha capturado nada todavia, asi que marcarlas como sembradas no pisa el
   trabajo de nadie. */
ALTER TABLE dbo.HE_Detalle
  ADD HorasOrigen VARCHAR(10) NOT NULL
      CONSTRAINT DF_HE_Detalle_HorasOrigen DEFAULT 'Tareas';
GO

/* --------------------------------- 3. las horas aprobadas de las tareas --- */
IF OBJECT_ID('dbo.Sp_RTA_HeHorasAprobadas','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeHorasAprobadas;
GO
/* Horas extras ya aprobadas en las tareas, agregadas por colaborador y listas
   para sembrar un periodo. SOLO LECTURA sobre R_DetTareasAranda: esta fase no
   toca el flujo de aprobacion, lo consume.

   Las horas no son una columna: son la duracion del tramo. Det_Tiempo trae lo
   mismo ya formateado y cuadra con el DATEDIFF, pero se usa el DATEDIFF porque
   es el dato y no su presentacion.

   El puente de identidad va Id_Responsable -> Cod_Usuario -> Cedula ->
   Empleados -> IdEmpleado. HE_ColaboradorParametro.IdEmpleado NO es la cedula:
   es un codigo de 1 a 3 digitos, y cruzarlo directo contra R_Usuarios.Cedula da
   cero de 64. Empleados es el eslabon que falta.

   En este sentido -de la tarea hacia el maestro- no se duplica: cada
   Id_Responsable llega a un solo IdEmpleado. Al reves si: una cedula tiene seis
   cuentas de usuario. No invertir este JOIN.

   El filtro de fechas va por Det_Fch_RegDetalleIni, la fecha del TRABAJO. La de
   aprobacion no sirve como filtro de periodo: sobre el 17-ago / 15-sep de 2026
   desfasa hasta 33 dias y arrastra tramos trabajados en julio.

   Estado 2 es Aprobado y Tipo 1 y 2 son 50% y 100% (tabla Catalogo,
   IdTipoCatalogo 4 y 7). Las filas con Tipo 0 quedan fuera a proposito: sin
   recargo no hay con que valorarlas. */
CREATE PROCEDURE dbo.Sp_RTA_HeHorasAprobadas
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.IdEmpleado,
           Horas50 = CAST(SUM(CASE WHEN d.Det_Horas_Extras_Tipo = 1
                                   THEN DATEDIFF(MINUTE, d.Det_Fch_RegDetalleIni, d.Det_Fch_RegDetalleFin)
                                   ELSE 0 END) / 60.0 AS DECIMAL(9,2)),
           Horas100 = CAST(SUM(CASE WHEN d.Det_Horas_Extras_Tipo = 2
                                    THEN DATEDIFF(MINUTE, d.Det_Fch_RegDetalleIni, d.Det_Fch_RegDetalleFin)
                                    ELSE 0 END) / 60.0 AS DECIMAL(9,2))
      FROM dbo.R_DetTareasAranda d
      JOIN dbo.R_Usuarios u ON u.Cod_Usuario = d.Id_Responsable
      JOIN dbo.Empleados  e ON LTRIM(RTRIM(e.Cedula)) = LTRIM(RTRIM(u.Cedula))
      JOIN dbo.HE_ColaboradorParametro c ON c.IdEmpleado = e.IdEmpleado AND c.Estado = '1'
     WHERE d.Det_Fch_RegDetalleIni >= @FechaInicio
       AND d.Det_Fch_RegDetalleIni <  DATEADD(DAY, 1, @FechaFin)
       AND d.Det_Fch_RegDetalleFin IS NOT NULL
       AND d.Det_Horas_Extras_Estado = 2
       AND d.Det_Horas_Extras_Tipo IN (1, 2)
     GROUP BY c.IdEmpleado
     ORDER BY c.IdEmpleado;
END
GO

/* ---------------------------------------- 4. crear periodo, por rango ----- */
IF OBJECT_ID('dbo.Sp_RTA_HeCrearPeriodo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeCrearPeriodo;
GO
/* Devuelve los mismos dos campos que antes, Respuestas e IdPeriodo. Lo que
   cambia es la firma -el mes se vuelve un rango- y una validacion nueva.

   Respuestas: 0 bien, -1 el rango esta al reves o viene vacio, -5 el rango se
   solapa con un periodo que ya existe.

   El -5 no es un lujo. Sin esa validacion, dos periodos que compartan dias
   pagarian las mismas horas dos veces, y eso no lo detecta nadie hasta que
   alguien reclama su rol. El indice unico del rango NO alcanza: solo atrapa el
   rango identico, y 17-ago/15-sep contra 1-sep/30-sep son dos claves distintas
   que se pisan en quince dias.

   El orden de las validaciones importa, y sobre todo que la idempotencia vaya
   ANTES del solapamiento: un rango identico tambien se solapa consigo mismo, y
   al reves abrir dos veces el mismo periodo pasaria de inofensivo a error.

   La Descripcion se construye del rango. De paso arregla que antes dijera
   "September 2026" en ingles: DATENAME(MONTH, ...) usa el idioma del servidor,
   no el del que lee la pantalla. */
CREATE PROCEDURE dbo.Sp_RTA_HeCrearPeriodo
    @FechaInicio DATE,
    @FechaFin    DATE,
    @Usuario     VARCHAR(50),
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    /* El NULL entra por aqui y no mas abajo: con NULL las comparaciones de
       rango dan UNKNOWN, no coinciden con nada, y el INSERT reventaria contra
       el NOT NULL en vez de devolver un codigo. */
    IF @FechaInicio IS NULL OR @FechaFin IS NULL OR @FechaInicio > @FechaFin
    BEGIN
        SELECT Respuestas = -1, IdPeriodo = 0;
        RETURN;
    END

    DECLARE @IdPeriodo INT;

    SELECT @IdPeriodo = IdPeriodo
      FROM dbo.HE_Periodo
     WHERE FechaInicio = @FechaInicio AND FechaFin = @FechaFin;

    /* Ya existe: no se toca. Devolverlo tal cual es lo que hace que abrir dos
       veces el mismo periodo sea inofensivo. */
    IF @IdPeriodo IS NOT NULL
    BEGIN
        SELECT Respuestas = 0, IdPeriodo = @IdPeriodo;
        RETURN;
    END

    /* Dos rangos se solapan si cada uno empieza antes de que el otro termine.
       Escrito asi cubre los cuatro casos -contiene, contenido y los dos
       cruces- sin enumerarlos. */
    IF EXISTS (SELECT 1 FROM dbo.HE_Periodo
                WHERE @FechaInicio <= FechaFin AND FechaInicio <= @FechaFin)
    BEGIN
        SELECT Respuestas = -5, IdPeriodo = 0;
        RETURN;
    END

    INSERT INTO dbo.HE_Periodo (FechaInicio, FechaFin, Descripcion, EstadoPeriodo, UsuarioCreacion, Ip_Modificacion)
    VALUES (@FechaInicio, @FechaFin,
            CONVERT(VARCHAR(10), @FechaInicio, 103) + ' - ' + CONVERT(VARCHAR(10), @FechaFin, 103),
            'Abierto', @Usuario, @Ip);

    SELECT Respuestas = 0, IdPeriodo = SCOPE_IDENTITY();
END
GO

/* ----------------------------------------------- 5. listar periodos ------- */
IF OBJECT_ID('dbo.Sp_RTA_HeListarPeriodos','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeListarPeriodos;
GO
/* Anio y Mes salen del contrato y entran FechaInicio y FechaFin, en el mismo
   sitio: son dos columnas por dos columnas y las de atras no se mueven. El
   orden por FechaInicio DESC dice lo mismo que decia Anio DESC, Mes DESC. */
CREATE PROCEDURE dbo.Sp_RTA_HeListarPeriodos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdPeriodo, FechaInicio, FechaFin, Descripcion, EstadoPeriodo,
           FechaCierre, UsuarioCierre
      FROM dbo.HE_Periodo
     ORDER BY FechaInicio DESC;
END
GO

/* ----------------------------------------------- 6. cargar periodo -------- */
IF OBJECT_ID('dbo.Sp_RTA_HeCargarPeriodo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeCargarPeriodo;
GO
/* Dos result sets, en este orden:
     1. la cabecera del periodo (0 filas si no existe)
     2. sus filas de detalle

   En la cabecera, Anio y Mes se cambian por FechaInicio y FechaFin en su misma
   posicion. En el detalle, HorasOrigen va AL FINAL, despues de MotivoNoAplica,
   que es como se extiende un contrato posicional: en cualquier otro sitio le
   correria el indice a las columnas de atras y el lector las leeria cruzadas. */
CREATE PROCEDURE dbo.Sp_RTA_HeCargarPeriodo
    @IdPeriodo INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdPeriodo, FechaInicio, FechaFin, Descripcion, EstadoPeriodo,
           FechaCierre, UsuarioCierre
      FROM dbo.HE_Periodo WHERE IdPeriodo = @IdPeriodo;

    SELECT d.IdDetalle, d.IdEmpleado, d.CedulaSnapshot, d.NombreSnapshot,
           d.EmpresaSnapshot, d.CargoSnapshot, d.JornadaHorasDiaSnapshot,
           d.SalarioBaseSnapshot, d.AplicaHESnapshot, d.Divisor,
           d.ValorHoraOrdinaria, d.ValorHora50, d.ValorHora100,
           d.Horas50, d.Horas100, d.Total50, d.Total100, d.TotalHoras, d.TotalHE,
           Observacion = ISNULL(d.Observacion, ''),
           MotivoNoAplica = ISNULL(c.MotivoNoAplica, ''),
           HorasOrigen = ISNULL(d.HorasOrigen, 'Tareas')
      FROM dbo.HE_Detalle d
      LEFT JOIN dbo.HE_ColaboradorParametro c ON c.IdEmpleado = d.IdEmpleado
     WHERE d.IdPeriodo = @IdPeriodo
     ORDER BY d.NombreSnapshot, d.IdEmpleado;
END
GO

/* ------------------------------------------------- 7. guardar fila -------- */
IF OBJECT_ID('dbo.Sp_RTA_HeGuardarFila','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeGuardarFila;
GO
/* Identico al de la fase 3 salvo por @HorasOrigen, que se escribe tanto en el
   UPDATE como en el INSERT.

   @HorasOrigen = 'Manual' por omision, y es a proposito. Los scripts van antes
   que los binarios, asi que hay una ventana en la que el DAO viejo llama a este
   procedimiento sin el parametro mientras una persona de Nomina digita horas.
   Marcar esas ediciones como manuales es el lado seguro: una fila 'Manual' no
   se re-siembra, asi que en el peor caso se pierde una siembra -que se puede
   volver a pedir- y nunca una correccion escrita a mano, que no se recupera.

   El resto del comentario de la fase 3 sigue valiendo entero; aqui solo lo
   imprescindible para leer el cuerpo:

     @Auditar = 1 por omision por la misma razon de ventana de despliegue. El
     olvidadizo audita de mas, y "de mas" es exactamente nada: el bloque de
     auditoria solo escribe cuando cambia Horas50, Horas100 u Observacion, que
     son los tres campos que una apertura de periodo NO toca.

     El UPDATE va primero y el INSERT solo si @@ROWCOUNT = 0: no se ensancha la
     ventana de check-then-act con una lectura previa. Los valores de ANTES los
     captura la clausula OUTPUT del propio UPDATE, que es atomica.

     Respuestas: 0 bien, -1 el periodo no existe, -2 el periodo no esta
     Abierto. Devuelve ademas IdDetalle, que es con lo que se escribe la
     auditoria.

   HorasOrigen no se audita. La auditoria registra lo que decidio una persona, y
   el origen es una etiqueta del sistema: cuando pasa de 'Tareas' a 'Manual' es
   porque cambiaron las horas, y ESE cambio ya queda registrado. */
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
    @Auditar                 BIT = 1,
    @HorasOrigen             VARCHAR(10) = 'Manual'
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
           Observacion = @Observacion, HorasOrigen = @HorasOrigen,
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
             Observacion, HorasOrigen, Usu_Modificacion, Ip_Modificacion)
        VALUES
            (@IdPeriodo, @IdEmpleado, @CedulaSnapshot, @NombreSnapshot, @EmpresaSnapshot,
             @CargoSnapshot, @JornadaHorasDiaSnapshot, @SalarioBaseSnapshot, @AplicaHESnapshot,
             @Divisor, @ValorHoraOrdinaria, @ValorHora50, @ValorHora100,
             @Horas50, @Horas100, @Total50, @Total100, @TotalHoras, @TotalHE,
             @Observacion, @HorasOrigen, @Usuario, @Ip);

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

/* ------------------------------------------------- 8. aserciones ---------- */

/* Todo este bloque va en un solo batch, sin GO en medio: un GO entre el
   DECLARE y su ultimo uso hace fallar el script entero con "must declare
   the scalar variable". Por eso va entero al final, despues del ultimo GO
   de arriba, y no intercalado entre cada CREATE PROCEDURE. */
DECLARE @Fallos INT = 0;

/* --- 1. el esquema de HE_Periodo --- */
IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_Periodo') AND name = 'FechaInicio')
BEGIN
    RAISERROR('FALLO: HE_Periodo.FechaInicio no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_Periodo') AND name = 'FechaFin')
BEGIN
    RAISERROR('FALLO: HE_Periodo.FechaFin no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF EXISTS (SELECT 1 FROM sys.columns
            WHERE object_id = OBJECT_ID('dbo.HE_Periodo') AND name IN ('Anio','Mes'))
BEGIN
    RAISERROR('FALLO: HE_Periodo todavia tiene Anio o Mes.', 16, 1);
    SET @Fallos += 1;
END

/* --- 2. los dos periodos que ya existian quedaron con su rango --- */
/* Si uno de estos dos falta, el UPDATE de conversion no corrio o corrio mal, y
   la pantalla mostraria un periodo que no cubre los dias que dice cubrir. */
IF NOT EXISTS (SELECT 1 FROM dbo.HE_Periodo
                WHERE FechaInicio = '2026-08-01' AND FechaFin = '2026-08-31')
BEGIN
    RAISERROR('FALLO: no quedo el periodo 2026-08-01 / 2026-08-31.', 16, 1);
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM dbo.HE_Periodo
                WHERE FechaInicio = '2026-09-01' AND FechaFin = '2026-09-30')
BEGIN
    RAISERROR('FALLO: no quedo el periodo 2026-09-01 / 2026-09-30.', 16, 1);
    SET @Fallos += 1;
END

/* --- 3. el indice unico cambio de columnas --- */
IF EXISTS (SELECT 1 FROM sys.indexes
            WHERE object_id = OBJECT_ID('dbo.HE_Periodo') AND name = 'UX_HE_Periodo_AnioMes')
BEGIN
    RAISERROR('FALLO: UX_HE_Periodo_AnioMes sigue existiendo.', 16, 1);
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.HE_Periodo')
                  AND name = 'UX_HE_Periodo_Rango' AND is_unique = 1)
BEGIN
    RAISERROR('FALLO: UX_HE_Periodo_Rango no existe o no es unico.', 16, 1);
    SET @Fallos += 1;
END

/* --- 4. HorasOrigen --- */
IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_Detalle') AND name = 'HorasOrigen')
BEGIN
    RAISERROR('FALLO: HE_Detalle.HorasOrigen no quedo creada.', 16, 1);
    SET @Fallos += 1;
END
ELSE
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.HE_Detalle WHERE ISNULL(HorasOrigen, '') <> 'Tareas')
    BEGIN
        RAISERROR('FALLO: hay filas de HE_Detalle con HorasOrigen distinto de Tareas.', 16, 1);
        SET @Fallos += 1;
    END

    /* Las 124 filas medidas el 2026-09-16. Que salgan mas o menos no es un
       defecto del script, pero si la senal de que el mundo cambio desde que se
       midio -y el numero de corte de mas abajo se midio el mismo dia-. */
    IF (SELECT COUNT(*) FROM dbo.HE_Detalle) <> 124
    BEGIN
        RAISERROR('FALLO: HE_Detalle no tiene las 124 filas que se midieron el 2026-09-16.', 16, 1);
        SET @Fallos += 1;
    END
END

/* --- 5. los cinco procedimientos --- */
IF OBJECT_ID('dbo.Sp_RTA_HeHorasAprobadas','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeHorasAprobadas no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeCrearPeriodo','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeCrearPeriodo no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeListarPeriodos','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeListarPeriodos no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeCargarPeriodo','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeCargarPeriodo no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeGuardarFila','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeGuardarFila no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

/* Las firmas nuevas. Que @HorasOrigen tenga valor por omision NO se puede
   afirmar desde el catalogo: sys.parameters.has_default_value vale 0 para
   TODOS los parametros de un procedimiento T-SQL, tengan o no default. Que el
   parametro exista es lo unico comprobable aqui; que el default funcione se
   comprueba llamando al procedimiento con 23 parametros, y eso lo hace el
   controlador a mano. */
IF NOT EXISTS (SELECT 1 FROM sys.parameters
                WHERE object_id = OBJECT_ID('dbo.Sp_RTA_HeGuardarFila')
                  AND name = '@HorasOrigen')
BEGIN
    RAISERROR('FALLO: el parametro @HorasOrigen no quedo en Sp_RTA_HeGuardarFila.', 16, 1);
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.parameters
                WHERE object_id = OBJECT_ID('dbo.Sp_RTA_HeCrearPeriodo')
                  AND name = '@FechaInicio')
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeCrearPeriodo no recibe @FechaInicio.', 16, 1);
    SET @Fallos += 1;
END

IF EXISTS (SELECT 1 FROM sys.parameters
            WHERE object_id = OBJECT_ID('dbo.Sp_RTA_HeCrearPeriodo')
              AND name IN ('@Anio','@Mes'))
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeCrearPeriodo todavia recibe @Anio o @Mes.', 16, 1);
    SET @Fallos += 1;
END

/* Lo que NO se comprueba aqui: que ningun otro modulo de la base siga leyendo
   HE_Periodo.Anio. El DROP COLUMN no avisa -un procedimiento que la lea sigue
   existiendo y solo revienta cuando alguien lo llama- y buscarlo por texto en
   sys.sql_modules da falsos positivos garantizados, porque los comentarios de
   estos mismos procedimientos explican el cambio y nombran las dos columnas.
   La consulta que si sirve la corre el controlador a mano, mirando el
   resultado uno por uno:

     SELECT OBJECT_NAME(object_id) FROM sys.sql_modules
      WHERE definition LIKE '%HE_Periodo%';

   Los conocidos al 2026-09-16 son los cinco de este script mas
   Sp_RTA_HeCerrarPeriodo, Sp_RTA_HeReabrirPeriodo,
   Sp_RTA_HeRegistrarReaperturaDenegada y Sp_RTA_HeRegistrarDescarga, y ninguno
   de esos cuatro toca Anio ni Mes. */

/* --- 6. la asercion de corte --- */
/* Medido en produccion el 2026-09-16 sobre el rango 17-ago / 15-sep: 9
   colaboradores, 24,51 h al 50% y 97,79 h al 100%. Es la unica asercion que
   comprueba que el procedimiento nuevo devuelve LO CORRECTO y no solo que
   existe: si el puente de identidad se rompiera -por ejemplo invirtiendo el
   JOIN contra R_Usuarios, donde una cedula tiene seis cuentas- las horas se
   multiplicarian y esto lo cazaria.

   OJO CON ESOS DOS TOTALES. La primera medicion a mano dio 24,52 y 97,78, y
   esta asercion fallo en la primera ejecucion por un centavo en cada una. No
   habia ningun defecto: la medicion sumaba los minutos de todos y redondeaba
   UNA vez al final, mientras que el procedimiento redondea POR PERSONA. Manda
   el procedimiento, porque HE_Detalle.Horas50 es DECIMAL(9,2): cada fila se
   guarda ya redondeada y el total del periodo es una suma de valores
   guardados. Redondear al final describiria un total que el sistema no
   muestra en ninguna pantalla.

   Si algun dia cambias el CAST del procedimiento, estos dos numeros cambian
   con el, y no por eso esta roto. */

   INSERT ... EXEC para poder contar y sumar el result set. No se imprime
   ninguna fila: IdEmpleado identifica a una persona. */
CREATE TABLE #HeAprobadas (IdEmpleado BIGINT, Horas50 DECIMAL(9,2), Horas100 DECIMAL(9,2));

IF OBJECT_ID('dbo.Sp_RTA_HeHorasAprobadas','P') IS NOT NULL
BEGIN
    INSERT INTO #HeAprobadas (IdEmpleado, Horas50, Horas100)
    EXEC dbo.Sp_RTA_HeHorasAprobadas '2026-08-17', '2026-09-15';

    IF (SELECT COUNT(*) FROM #HeAprobadas) <> 9
    BEGIN
        RAISERROR('FALLO: Sp_RTA_HeHorasAprobadas 17-ago/15-sep no devolvio 9 filas.', 16, 1);
        SET @Fallos += 1;
    END

    IF ISNULL((SELECT SUM(Horas50) FROM #HeAprobadas), -1) <> 24.51
    BEGIN
        RAISERROR('FALLO: la suma de Horas50 del rango de corte no es 24.51.', 16, 1);
        SET @Fallos += 1;
    END

    IF ISNULL((SELECT SUM(Horas100) FROM #HeAprobadas), -1) <> 97.79
    BEGIN
        RAISERROR('FALLO: la suma de Horas100 del rango de corte no es 97.79.', 16, 1);
        SET @Fallos += 1;
    END
END

DROP TABLE #HeAprobadas;

/* --- 7. el rechazo del solapamiento --- */
/* Esta no se puede probar de verdad desde aqui: probarla exigiria crear un
   periodo real, y un periodo de prueba en produccion es basura que alguien
   tendria que salir a limpiar -y que ademas bloquearia el rango que ocupe-.
   Lo unico que se comprueba es que el codigo del -5 esta en la definicion.
   La prueba de comportamiento la hace el controlador a mano, llamando al
   procedimiento con un rango que pise a uno existente y mirando que devuelva
   -5 sin insertar nada. */
IF CHARINDEX('Respuestas = -5', ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.Sp_RTA_HeCrearPeriodo')), '')) = 0
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeCrearPeriodo no devuelve -5 por ningun camino.', 16, 1);
    SET @Fallos += 1;
END

IF @Fallos > 0
    RAISERROR('FALLO: %d verificaciones no pasaron.', 16, 1, @Fallos);
ELSE
    PRINT 'Horas Extras fase 4: el periodo es un rango y las horas aprobadas se leen.';
GO

/* Deshace el guard de la cabecera: sin esto, la sesion de quien corriera el
   script se quedaria en NOEXEC y todo lo que ejecutara despues -en la misma
   ventana- se analizaria sin ejecutarse, en silencio. */
SET NOEXEC OFF;
GO
