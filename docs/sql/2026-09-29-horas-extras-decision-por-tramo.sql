/* ============================================================================
   Horas Extras: decision por tramo y pago por minutos (Parte 1: datos)
   ReporTarea  |  2026-09-29

   Nomina decide, tramo por tramo, que se paga en cada periodo. Este script
   deja la base lista para eso:

     1. HE_TramoDecision            Tabla nueva, SOLO INSERT. Cambiar una
                                    decision es insertar otra: manda la de
                                    IdDecision mas alto de cada clave
                                    (IdPeriodo, Id_RegDetTareas, SegmentoInicio).
                                    Nunca se actualiza ni se borra, asi que la
                                    tabla es su propio historial.
     2. Sp_RTA_HeTramosCandidatos   SOLO LECTURA. Los segmentos que Nomina
                                    puede incluir en el periodo, las decisiones
                                    vigentes sobre ellos y los periodos.
     3. Sp_RTA_HeGuardarDecisiones  Inserta un lote de decisiones, todo o nada,
                                    con la proteccion contra el pago doble.

   Que NO hace, a proposito:
     - No agrega Minutos50 / Minutos100 a HE_Detalle: los minutos se recalculan
       desde las decisiones en cada siembra, asi que guardarlos seria una
       segunda verdad que podria desincronizarse.
     - No crea ni altera ningun procedimiento existente. En particular
       Sp_RTA_HeHorasAprobadas queda igual: es la vuelta atras de la siembra.
     - No calcula minutos ni une superposiciones: eso vive en CapaNegocio, que
       tiene pruebas. En T-SQL no las tendria.

   El puente de identidad es el de siempre y en el mismo sentido: tarea ->
   usuario -> cedula -> Empleados -> maestro activo. Al reves, una cedula con
   varias cuentas contaria varias veces.

   Los perfiles NO se comprueban aqui, igual que en el resto del modulo: la
   barrera esta en AdministrarHorasExtras.ashx.cs.

   Nadie llama a estos objetos hasta que se publiquen los binarios. Re-correr
   el script es inofensivo: la tabla se crea solo si no existe (sus filas no se
   tocan) y los dos procedimientos son nuevos.

   La verificacion del final NO escribe nada: prueba los codigos de rechazo con
   lotes que se rechazan ANTES de insertar, y cuenta las filas antes y despues.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------------ 1. HE_TramoDecision ----- */
/* SegmentoInicio dice QUE parte del tramo se decide: el inicio del tramo si
   es un tramo aprobado (se decide completo), o el inicio del segmento fuera de
   horario que da FN_RTA_ClasificarTramosHorario si no lo es.

   MinutosIncluidos NULL = el segmento completo. Si trae un numero, se pagan los
   primeros N minutos del segmento, y ese N nunca puede subir por encima del
   largo del segmento (lo valida el procedimiento que guarda).

   Sin clave foranea a HE_Periodo ni a R_DetTareasAranda, como el resto del
   esquema: la integridad la cuida Sp_RTA_HeGuardarDecisiones, unica puerta de
   entrada. */
IF OBJECT_ID('dbo.HE_TramoDecision','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_TramoDecision
    (
        IdDecision       INT IDENTITY(1,1) NOT NULL
                         CONSTRAINT PK_HE_TramoDecision PRIMARY KEY CLUSTERED,
        IdPeriodo        INT          NOT NULL,
        Id_RegDetTareas  BIGINT       NOT NULL,
        SegmentoInicio   DATETIME     NOT NULL,
        Incluir          BIT          NOT NULL,
        TipoHora         INT          NOT NULL
                         CONSTRAINT CK_HE_TramoDecision_TipoHora CHECK (TipoHora IN (1, 2)),
        MinutosIncluidos INT          NULL,
        Motivo           VARCHAR(400) NULL,
        Usuario          VARCHAR(50)  NOT NULL,
        Fecha            DATETIME2(0) NOT NULL
                         CONSTRAINT DF_HE_TramoDecision_Fecha DEFAULT SYSDATETIME(),
        Ip               VARCHAR(64)  NULL
    );

    /* La decision vigente de un periodo: la primera fila de cada clave. */
    CREATE INDEX IX_HE_TramoDecision_Clave
        ON dbo.HE_TramoDecision (IdPeriodo, Id_RegDetTareas, SegmentoInicio, IdDecision DESC);

    /* El pago doble mira el MISMO segmento en TODOS los periodos: sin este
       indice seria un recorrido completo, y el HOLDLOCK bloquearia la tabla
       entera en vez del rango de ese segmento. */
    CREATE INDEX IX_HE_TramoDecision_Segmento
        ON dbo.HE_TramoDecision (Id_RegDetTareas, SegmentoInicio);
END
GO

/* ------------------------------------------ 2. candidatos del periodo ----- */
IF OBJECT_ID('dbo.Sp_RTA_HeTramosCandidatos','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeTramosCandidatos;
GO
/* Tres conjuntos:

   1. Candidatos: segmentos de las personas del maestro activo cuyo tramo
      empieza entre FechaInicio - 7 dias y FechaFin + 7 dias (inclusive).
        - Tramo aprobado (estado 2 y tipo 1 o 2): UN segmento, el tramo
          completo, con el tipo que aprobo la jefatura. Es exactamente lo que
          hoy paga Sp_RTA_HeHorasAprobadas.
        - Cualquier otro tramo (sin solicitar, pendiente, aprobado sin tipo o
          rechazado): solo sus segmentos fuera de horario, con el tipo que da
          el horario de la persona ese dia.
      DentroDelPeriodo = 1 si el tramo empieza dentro de [FechaInicio,
      FechaFin]. Los siete dias de cada lado existen para que Nomina pueda
      traer algo que quedo en el borde; por omision no se pagan.
   2. Decisiones vigentes (la de IdDecision mas alto por periodo y segmento)
      de CUALQUIER periodo sobre los tramos del conjunto 1. Con ellas
      CapaNegocio sabe si un segmento ya esta incluido en otro periodo.
   3. Todos los periodos, para saber el dueno de cada tramo (el periodo cuyo
      rango contiene su fecha) y su estado.

   Un periodo inexistente devuelve los tres conjuntos vacios (salvo el 3). */
CREATE PROCEDURE dbo.Sp_RTA_HeTramosCandidatos
    @IdPeriodo INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Ini DATE, @Fin DATE;
    SELECT @Ini = FechaInicio, @Fin = FechaFin
      FROM dbo.HE_Periodo
     WHERE IdPeriodo = @IdPeriodo;

    /* Ventana: desde FechaInicio - 7 hasta FechaFin + 7, ambos inclusive. */
    DECLARE @VentanaIni DATE = DATEADD(DAY, -7, @Ini),
            @VentanaFin DATE = DATEADD(DAY,  8, @Fin);

    CREATE TABLE #Tramos
    (
        IdEmpleado     BIGINT       NOT NULL,
        Id_Responsable VARCHAR(20)  NOT NULL,
        IdRegDetTareas BIGINT       NOT NULL,
        Inicio         DATETIME     NOT NULL,
        Fin            DATETIME     NOT NULL,
        Estado         INT          NOT NULL,
        Tipo           INT          NOT NULL,
        Actividad      VARCHAR(200) NOT NULL
    );

    INSERT INTO #Tramos (IdEmpleado, Id_Responsable, IdRegDetTareas, Inicio, Fin, Estado, Tipo, Actividad)
    SELECT c.IdEmpleado,
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
      JOIN dbo.HE_ColaboradorParametro c ON c.IdEmpleado = e.IdEmpleado AND c.Estado = '1'
     WHERE d.Det_Fch_RegDetalleIni >= @VentanaIni
       AND d.Det_Fch_RegDetalleIni <  @VentanaFin
       AND d.Det_Fch_RegDetalleFin IS NOT NULL
       AND d.Det_Horas_Extras_Estado IN (0, 1, 2, 3);

    CREATE TABLE #Candidatos
    (
        IdEmpleado     BIGINT       NOT NULL,
        IdRegDetTareas BIGINT       NOT NULL,
        TramoInicio    DATETIME     NOT NULL,
        TramoFin       DATETIME     NOT NULL,
        SegmentoInicio DATETIME     NOT NULL,
        SegmentoFin    DATETIME     NOT NULL,
        TipoHora       INT          NOT NULL,
        EstadoTramo    INT          NOT NULL,
        TipoRegistrado INT          NOT NULL,
        Actividad      VARCHAR(200) NOT NULL
    );

    /* Aprobados: el tramo completo, con su tipo. */
    INSERT INTO #Candidatos
    SELECT t.IdEmpleado, t.IdRegDetTareas, t.Inicio, t.Fin, t.Inicio, t.Fin,
           t.Tipo, t.Estado, t.Tipo, t.Actividad
      FROM #Tramos t
     WHERE t.Estado = 2 AND t.Tipo IN (1, 2);

    /* El resto: solo lo que cae fuera de horario. Los tramos que cruzan la
       medianoche o caen en un dia sin configuracion no devuelven nada de la
       funcion, igual que en Sp_RTA_HeFueraDeHorario. */
    INSERT INTO #Candidatos
    SELECT t.IdEmpleado, t.IdRegDetTareas, t.Inicio, t.Fin, s.TramoInicio, s.TramoFin,
           CAST(s.TipoHora AS INT), t.Estado, t.Tipo, t.Actividad
      FROM #Tramos t
     CROSS APPLY dbo.FN_RTA_ClasificarTramosHorario(t.Id_Responsable, t.Inicio, t.Fin) s
     WHERE s.TipoHora IN (1, 2)
       AND NOT (t.Estado = 2 AND t.Tipo IN (1, 2));

    /* 1. Candidatos */
    SELECT IdEmpleado, IdRegDetTareas, TramoInicio, TramoFin, SegmentoInicio, SegmentoFin,
           TipoHora, EstadoTramo, TipoRegistrado, Actividad,
           DentroDelPeriodo = CAST(CASE WHEN TramoInicio >= @Ini
                                         AND TramoInicio <  DATEADD(DAY, 1, @Fin)
                                        THEN 1 ELSE 0 END AS BIT)
      FROM #Candidatos
     ORDER BY IdEmpleado, SegmentoInicio, IdRegDetTareas;

    /* 2. Decisiones vigentes, de cualquier periodo, sobre esos tramos */
    SELECT v.IdDecision, v.IdPeriodo, IdRegDetTareas = v.Id_RegDetTareas, v.SegmentoInicio,
           v.Incluir, v.TipoHora, v.MinutosIncluidos, v.Motivo, v.Usuario, v.Fecha
      FROM (SELECT d.IdDecision, d.IdPeriodo, d.Id_RegDetTareas, d.SegmentoInicio,
                   d.Incluir, d.TipoHora, d.MinutosIncluidos, d.Motivo, d.Usuario, d.Fecha,
                   Orden = ROW_NUMBER() OVER (PARTITION BY d.IdPeriodo, d.Id_RegDetTareas, d.SegmentoInicio
                                              ORDER BY d.IdDecision DESC)
              FROM dbo.HE_TramoDecision d
             WHERE d.Id_RegDetTareas IN (SELECT IdRegDetTareas FROM #Candidatos)) v
     WHERE v.Orden = 1
     ORDER BY v.Id_RegDetTareas, v.SegmentoInicio, v.IdPeriodo;

    /* 3. Periodos */
    SELECT IdPeriodo, FechaInicio, FechaFin, EstadoPeriodo
      FROM dbo.HE_Periodo
     ORDER BY FechaInicio, IdPeriodo;
END
GO

/* ------------------------------------------ 3. guardar decisiones --------- */
IF OBJECT_ID('dbo.Sp_RTA_HeGuardarDecisiones','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeGuardarDecisiones;
GO
/* @Decisiones: un elemento por decision, en cualquier nivel del documento
   (se recomienda <ds> como raiz):

     <ds><d t="IdRegDetTareas" s="yyyy-MM-ddTHH:mm:ss" i="0|1" h="1|2" m="" r="motivo"/></ds>

     t  el tramo                       s  SegmentoInicio
     i  1 se paga, 0 no                h  1 = 50%, 2 = 100%
     m  minutos (vacio = todo el segmento)
     r  motivo (obligatorio si se reducen minutos o se incluye algo no aprobado)

   Respuestas (una sola fila):
      0  guardado: TODO el lote
     -1  el periodo no existe o no esta 'Abierto'
     -2  un segmento con i = 1 ya esta incluido en OTRO periodo: por una
         decision vigente con Incluir = 1, o porque ese otro periodo es su
         dueno, el tramo esta aprobado y alli nadie lo excluyo. Un periodo
         'Anulado' no cuenta para nada: ni es dueno, ni sus decisiones
         incluyen (no se paga, asi que no puede provocar un pago doble)
     -3  datos invalidos: tipo fuera de 1 o 2, i fuera de 0 o 1, minutos que
         no son un numero entre 1 y lo que queda del tramo desde el segmento,
         falta el motivo, motivo de mas de 400, el mismo segmento dos veces en
         el lote, lote vacio o usuario vacio
     -4  el tramo no existe, empieza fuera de la ventana del periodo
         (FechaInicio - 7 a FechaFin + 7), el segmento no cae dentro del
         tramo, o el tramo esta aprobado y s no es su inicio (un aprobado tiene
         UN solo segmento, el tramo completo: otro s esquivaria la regla (b))

   Orden: primero -1; despues, sobre el lote entero, -4, -3 y -2, en ese
   orden. Todo o nada: si un solo elemento falla, no se inserta ninguno.

   Los minutos se validan aqui contra el FIN DEL TRAMO, no contra el fin del
   segmento, porque el segmento fuera de horario lo calcula una funcion que no
   conviene repetir fila por fila dentro de la transaccion. CapaNegocio valida
   contra el largo exacto del segmento antes de llamar; esto es el respaldo.

   Todo lo que depende de la base va DENTRO de la transaccion: el periodo con
   UPDLOCK + HOLDLOCK (no se puede cerrar a mitad de un guardado) y las
   decisiones del mismo segmento en todos los periodos, tambien con UPDLOCK +
   HOLDLOCK (dos pedidos simultaneos desde dos periodos no pueden pasar los dos
   la regla del pago doble).

   Las salidas tempranas hacen COMMIT y no ROLLBACK: en ese punto no se
   escribio nada, asi que el efecto es el mismo (solo suelta los bloqueos), y
   un ROLLBACK dentro de INSERT ... EXEC da el error 3915. El ROLLBACK queda
   solo en el CATCH, ante un error de verdad. */
CREATE PROCEDURE dbo.Sp_RTA_HeGuardarDecisiones
    @IdPeriodo  INT,
    @Decisiones XML,
    @Usuario    VARCHAR(50),
    @Ip         VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    /* El lote se lee como texto y se convierte con TRY_CONVERT: un valor mal
       formado queda en NULL y se rechaza con su codigo, en vez de cortar el
       procedimiento con un error de conversion. */
    CREATE TABLE #Lote
    (
        Pos             INT           NOT NULL,
        TTexto          NVARCHAR(40)  NULL,
        STexto          NVARCHAR(40)  NULL,
        ITexto          NVARCHAR(10)  NULL,
        HTexto          NVARCHAR(10)  NULL,
        MTexto          NVARCHAR(20)  NULL,
        RTexto          NVARCHAR(MAX) NULL,
        Id_RegDetTareas BIGINT        NULL,
        SegmentoInicio  DATETIME      NULL,
        Incluir         BIT           NULL,
        TipoHora        INT           NULL,
        MinutosDados    BIT           NOT NULL DEFAULT 0,
        Minutos         INT           NULL,
        Motivo          NVARCHAR(MAX) NULL,
        TramoInicio     DATETIME      NULL,
        TramoFin        DATETIME      NULL,
        Aprobado        BIT           NULL
    );

    INSERT INTO #Lote (Pos, TTexto, STexto, ITexto, HTexto, MTexto, RTexto)
    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)),
           n.x.value('@t', 'NVARCHAR(40)'),
           n.x.value('@s', 'NVARCHAR(40)'),
           n.x.value('@i', 'NVARCHAR(10)'),
           n.x.value('@h', 'NVARCHAR(10)'),
           n.x.value('@m', 'NVARCHAR(20)'),
           n.x.value('@r', 'NVARCHAR(MAX)')
      FROM @Decisiones.nodes('//d') AS n(x);

    UPDATE #Lote
       SET Id_RegDetTareas = TRY_CONVERT(BIGINT, LTRIM(RTRIM(TTexto))),
           SegmentoInicio  = TRY_CONVERT(DATETIME, LTRIM(RTRIM(STexto)), 126),
           Incluir         = CASE LTRIM(RTRIM(ITexto)) WHEN '1' THEN 1 WHEN '0' THEN 0 END,
           TipoHora        = TRY_CONVERT(INT, LTRIM(RTRIM(HTexto))),
           MinutosDados    = CASE WHEN LTRIM(RTRIM(ISNULL(MTexto, ''))) = '' THEN 0 ELSE 1 END,
           Minutos         = TRY_CONVERT(INT, NULLIF(LTRIM(RTRIM(ISNULL(MTexto, ''))), '')),
           Motivo          = NULLIF(LTRIM(RTRIM(ISNULL(RTexto, ''))), '');

    /* El tramo, UNA sola vez por Id: R_DetTareasAranda no tiene clave primaria
       ni indice por Id_RegDetTareas, asi que un cruce por elemento recorreria
       la tabla N veces. Un Id repetido en la tabla (existe al menos uno, de
       2019) no se puede decidir sin ambiguedad: queda sin tramo y da -4. */
    CREATE TABLE #Tramo
    (
        Id_RegDetTareas BIGINT   NOT NULL PRIMARY KEY,
        Filas           INT      NOT NULL,
        Inicio          DATETIME NULL,
        Fin             DATETIME NULL,
        Estado          INT      NULL,
        Tipo            INT      NULL
    );

    INSERT INTO #Tramo (Id_RegDetTareas, Filas, Inicio, Fin, Estado, Tipo)
    SELECT CAST(d.Id_RegDetTareas AS BIGINT), COUNT(*),
           MIN(d.Det_Fch_RegDetalleIni), MIN(d.Det_Fch_RegDetalleFin),
           MIN(CAST(d.Det_Horas_Extras_Estado AS INT)),
           MIN(CAST(ISNULL(d.Det_Horas_Extras_Tipo, 0) AS INT))
      FROM dbo.R_DetTareasAranda d
     WHERE d.Id_RegDetTareas IN (SELECT Id_RegDetTareas FROM #Lote WHERE Id_RegDetTareas IS NOT NULL)
     GROUP BY d.Id_RegDetTareas;

    UPDATE l
       SET TramoInicio = t.Inicio,
           TramoFin    = t.Fin,
           Aprobado    = CASE WHEN t.Estado = 2 AND t.Tipo IN (1, 2) THEN 1 ELSE 0 END
      FROM #Lote l
      JOIN #Tramo t ON t.Id_RegDetTareas = l.Id_RegDetTareas AND t.Filas = 1;

    CREATE TABLE #Vigente
    (
        IdPeriodo       INT      NOT NULL,
        Id_RegDetTareas BIGINT   NOT NULL,
        SegmentoInicio  DATETIME NOT NULL,
        Incluir         BIT      NOT NULL
    );

    BEGIN TRY
        BEGIN TRANSACTION;

        /* -1: el periodo, bloqueado para que no se cierre durante el guardado. */
        DECLARE @Ini DATE = NULL, @Fin DATE = NULL, @Estado VARCHAR(20) = NULL;

        SELECT @Ini = FechaInicio, @Fin = FechaFin, @Estado = EstadoPeriodo
          FROM dbo.HE_Periodo WITH (UPDLOCK, HOLDLOCK)
         WHERE IdPeriodo = @IdPeriodo;

        IF @Ini IS NULL OR ISNULL(@Estado, '') <> 'Abierto'
        BEGIN
            COMMIT TRANSACTION; SELECT -1 AS Respuestas; RETURN;
        END

        /* Un lote vacio o sin usuario no es un guardado: se rechaza como dato
           invalido en vez de devolver 0 sin haber hecho nada. */
        IF NOT EXISTS (SELECT 1 FROM #Lote) OR LTRIM(RTRIM(ISNULL(@Usuario, ''))) = ''
        BEGIN
            COMMIT TRANSACTION; SELECT -3 AS Respuestas; RETURN;
        END

        /* -4: tramo inexistente, fuera de la ventana, segmento fuera del tramo,
              o un aprobado con un segmento que no es el tramo completo. */
        IF EXISTS (SELECT 1 FROM #Lote
                    WHERE Id_RegDetTareas IS NULL
                       OR SegmentoInicio IS NULL
                       OR TramoInicio IS NULL
                       OR TramoFin IS NULL
                       OR TramoInicio <  DATEADD(DAY, -7, @Ini)
                       OR TramoInicio >= DATEADD(DAY,  8, @Fin)
                       OR SegmentoInicio <  TramoInicio
                       OR SegmentoInicio >= TramoFin
                       OR (Aprobado = 1 AND SegmentoInicio <> TramoInicio))
        BEGIN
            COMMIT TRANSACTION; SELECT -4 AS Respuestas; RETURN;
        END

        /* -3: tipo, incluir, minutos o motivo invalidos, o segmento repetido. */
        IF EXISTS (SELECT 1 FROM #Lote
                    WHERE Incluir IS NULL
                       OR TipoHora IS NULL OR TipoHora NOT IN (1, 2)
                       OR (MinutosDados = 1
                           AND (Minutos IS NULL
                                OR Minutos < 1
                                OR Minutos > DATEDIFF(MINUTE, SegmentoInicio, TramoFin)))
                       OR (Motivo IS NULL AND (MinutosDados = 1 OR (Incluir = 1 AND Aprobado = 0)))
                       OR LEN(Motivo) > 400)
           OR EXISTS (SELECT 1 FROM #Lote
                       GROUP BY Id_RegDetTareas, SegmentoInicio
                      HAVING COUNT(*) > 1)
        BEGIN
            COMMIT TRANSACTION; SELECT -3 AS Respuestas; RETURN;
        END

        /* -2: el pago doble. Las decisiones vigentes del MISMO segmento en
           TODOS los periodos, leidas con UPDLOCK + HOLDLOCK: el rango de esa
           clave queda tomado hasta el COMMIT, asi que otro guardado del mismo
           segmento espera y, al entrar, ya ve lo que este inserto. Las
           decisiones de un periodo 'Anulado' se dejan afuera: ese periodo no
           paga, asi que su Incluir = 1 no bloquea a nadie. */
        INSERT INTO #Vigente (IdPeriodo, Id_RegDetTareas, SegmentoInicio, Incluir)
        SELECT v.IdPeriodo, v.Id_RegDetTareas, v.SegmentoInicio, v.Incluir
          FROM (SELECT d.IdPeriodo, d.Id_RegDetTareas, d.SegmentoInicio, d.Incluir,
                       Orden = ROW_NUMBER() OVER (PARTITION BY d.IdPeriodo, d.Id_RegDetTareas, d.SegmentoInicio
                                                  ORDER BY d.IdDecision DESC)
                  FROM dbo.HE_TramoDecision d WITH (UPDLOCK, HOLDLOCK)
                  JOIN #Lote l ON l.Id_RegDetTareas = d.Id_RegDetTareas
                              AND l.SegmentoInicio  = d.SegmentoInicio
                 WHERE NOT EXISTS (SELECT 1 FROM dbo.HE_Periodo pa
                                    WHERE pa.IdPeriodo = d.IdPeriodo
                                      AND pa.EstadoPeriodo = 'Anulado')) v
         WHERE v.Orden = 1;

        IF EXISTS (SELECT 1
                     FROM #Lote l
                    WHERE l.Incluir = 1
                      AND (
                           /* (a) otro periodo lo tiene incluido por decision */
                           EXISTS (SELECT 1 FROM #Vigente v
                                    WHERE v.Id_RegDetTareas = l.Id_RegDetTareas
                                      AND v.SegmentoInicio  = l.SegmentoInicio
                                      AND v.IdPeriodo      <> @IdPeriodo
                                      AND v.Incluir         = 1)
                           /* (b) otro periodo, no anulado, es el dueno de un
                                  tramo aprobado, lo incluye por omision y nadie
                                  lo excluyo alli */
                        OR (l.Aprobado = 1
                            AND l.SegmentoInicio = l.TramoInicio
                            AND EXISTS (SELECT 1 FROM dbo.HE_Periodo p
                                         WHERE p.IdPeriodo <> @IdPeriodo
                                           AND p.EstadoPeriodo <> 'Anulado'
                                           AND l.TramoInicio >= p.FechaInicio
                                           AND l.TramoInicio <  DATEADD(DAY, 1, p.FechaFin)
                                           AND NOT EXISTS (SELECT 1 FROM #Vigente v
                                                            WHERE v.IdPeriodo       = p.IdPeriodo
                                                              AND v.Id_RegDetTareas = l.Id_RegDetTareas
                                                              AND v.SegmentoInicio  = l.SegmentoInicio
                                                              AND v.Incluir         = 0)))
                          ))
        BEGIN
            COMMIT TRANSACTION; SELECT -2 AS Respuestas; RETURN;
        END

        INSERT INTO dbo.HE_TramoDecision
            (IdPeriodo, Id_RegDetTareas, SegmentoInicio, Incluir, TipoHora,
             MinutosIncluidos, Motivo, Usuario, Fecha, Ip)
        SELECT @IdPeriodo, l.Id_RegDetTareas, l.SegmentoInicio, l.Incluir, l.TipoHora,
               l.Minutos, CAST(l.Motivo AS VARCHAR(400)), LTRIM(RTRIM(@Usuario)), SYSDATETIME(), @Ip
          FROM #Lote l
         ORDER BY l.Pos;

        COMMIT TRANSACTION;

        SELECT 0 AS Respuestas;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* ------------------------------------------------------ VERIFICACION ------ */
/* Sin GO entre el DECLARE y el veredicto. No escribe nada: cada llamado a
   Sp_RTA_HeGuardarDecisiones usa un lote que se rechaza ANTES de insertar, y
   se cuentan las filas de HE_TramoDecision antes y despues. No lleva ningun
   tramo escrito: el caso -3 toma uno real de la ventana del periodo. */
SET NOCOUNT ON;
DECLARE @Fallos INT = 0;
DECLARE @R TABLE (Respuestas INT);

IF OBJECT_ID('dbo.HE_TramoDecision','U') IS NULL
   OR OBJECT_ID('dbo.Sp_RTA_HeTramosCandidatos','P') IS NULL
   OR OBJECT_ID('dbo.Sp_RTA_HeGuardarDecisiones','P') IS NULL
BEGIN
    PRINT 'FALLO: falta la tabla o alguno de los dos procedimientos.';
    SET @Fallos += 1;
END

IF (SELECT COUNT(*) FROM sys.indexes
     WHERE object_id = OBJECT_ID('dbo.HE_TramoDecision')
       AND name IN ('IX_HE_TramoDecision_Clave', 'IX_HE_TramoDecision_Segmento')) <> 2
BEGIN
    PRINT 'FALLO: faltan indices de HE_TramoDecision.';
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.CK_HE_TramoDecision_TipoHora','C') IS NULL
BEGIN
    PRINT 'FALLO: falta el CHECK de TipoHora en HE_TramoDecision.';
    SET @Fallos += 1;
END

DECLARE @FilasAntes INT = (SELECT COUNT(*) FROM dbo.HE_TramoDecision);

/* Candidatos sobre el periodo mas reciente. Devuelve tres grillas: son solo
   lectura y se pueden ignorar. */
DECLARE @IdReciente INT;
SELECT TOP (1) @IdReciente = IdPeriodo FROM dbo.HE_Periodo ORDER BY FechaInicio DESC;
IF @IdReciente IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC dbo.Sp_RTA_HeTramosCandidatos @IdPeriodo = @IdReciente;
    END TRY
    BEGIN CATCH
        PRINT 'FALLO: Sp_RTA_HeTramosCandidatos fallo al ejecutarse: ' + ERROR_MESSAGE();
        SET @Fallos += 1;
    END CATCH
END

/* -1: periodo inexistente. */
INSERT INTO @R EXEC dbo.Sp_RTA_HeGuardarDecisiones
    @IdPeriodo = -1, @Decisiones = '<ds><d t="1" s="2026-01-01T08:00:00" i="1" h="1" m="" r=""/></ds>',
    @Usuario = 'verificacion', @Ip = '';
IF ISNULL((SELECT TOP 1 Respuestas FROM @R), 99) <> -1
BEGIN
    PRINT 'FALLO: Sp_RTA_HeGuardarDecisiones con un periodo inexistente deberia devolver -1.';
    SET @Fallos += 1;
END
DELETE FROM @R;

/* -4 y -3 sobre el periodo abierto mas reciente. */
DECLARE @IdAbierto INT, @PIni DATE, @PFin DATE;
SELECT TOP (1) @IdAbierto = IdPeriodo, @PIni = FechaInicio, @PFin = FechaFin
  FROM dbo.HE_Periodo
 WHERE EstadoPeriodo = 'Abierto'
 ORDER BY FechaInicio DESC;

IF @IdAbierto IS NULL
    PRINT 'AVISO: no hay un periodo abierto; se omiten las pruebas -4 y -3.';
ELSE
BEGIN
    INSERT INTO @R EXEC dbo.Sp_RTA_HeGuardarDecisiones
        @IdPeriodo = @IdAbierto, @Decisiones = '<ds><d t="-1" s="2026-01-01T08:00:00" i="0" h="1" m="" r=""/></ds>',
        @Usuario = 'verificacion', @Ip = '';
    IF ISNULL((SELECT TOP 1 Respuestas FROM @R), 99) <> -4
    BEGIN
        PRINT 'FALLO: Sp_RTA_HeGuardarDecisiones con un tramo inexistente deberia devolver -4.';
        SET @Fallos += 1;
    END
    DELETE FROM @R;

    /* Un tramo real de la ventana, para que pase el -4 y caiga en el -3. */
    DECLARE @T BIGINT, @S DATETIME;
    SELECT TOP (1) @T = CAST(d.Id_RegDetTareas AS BIGINT), @S = d.Det_Fch_RegDetalleIni
      FROM dbo.R_DetTareasAranda d
     WHERE d.Det_Fch_RegDetalleIni >= DATEADD(DAY, -7, @PIni)
       AND d.Det_Fch_RegDetalleIni <  DATEADD(DAY,  8, @PFin)
       AND d.Det_Fch_RegDetalleFin >  d.Det_Fch_RegDetalleIni
       AND DATEPART(MILLISECOND, d.Det_Fch_RegDetalleIni) = 0
       AND (SELECT COUNT(*) FROM dbo.R_DetTareasAranda o
             WHERE o.Id_RegDetTareas = d.Id_RegDetTareas) = 1
     ORDER BY d.Det_Fch_RegDetalleIni DESC;

    IF @T IS NULL
        PRINT 'AVISO: no hay tramos en la ventana del periodo abierto; se omite la prueba -3.';
    ELSE
    BEGIN
        DECLARE @X XML = '<ds><d t="' + CAST(@T AS VARCHAR(20))
                       + '" s="' + CONVERT(VARCHAR(19), @S, 126)
                       + '" i="0" h="3" m="" r=""/></ds>';
        INSERT INTO @R EXEC dbo.Sp_RTA_HeGuardarDecisiones
            @IdPeriodo = @IdAbierto, @Decisiones = @X, @Usuario = 'verificacion', @Ip = '';
        IF ISNULL((SELECT TOP 1 Respuestas FROM @R), 99) <> -3
        BEGIN
            PRINT 'FALLO: Sp_RTA_HeGuardarDecisiones con h = 3 deberia devolver -3.';
            SET @Fallos += 1;
        END
        DELETE FROM @R;
    END
END

IF (SELECT COUNT(*) FROM dbo.HE_TramoDecision) <> @FilasAntes
BEGIN
    PRINT 'FALLO: la verificacion escribio filas en HE_TramoDecision.';
    SET @Fallos += 1;
END

IF @Fallos = 0
    PRINT 'OK: decisiones por tramo listas (tabla y dos procedimientos).';
ELSE
    PRINT 'VEREDICTO: FALLO (' + CAST(@Fallos AS VARCHAR(10)) + ' comprobaciones). Revisar arriba.';
GO
