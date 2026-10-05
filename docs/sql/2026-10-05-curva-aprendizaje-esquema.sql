/* ============================================================================
   Curva de Aprendizaje: esquema y procedimientos
   ReporTarea  |  2026-10-05

   Modulo de onboarding del primer anio (ESPECIFICACION-MODULO-WEBFORMS.md,
   secciones 4 y 8). Este script deja la base lista; nadie llama a estos
   objetos hasta que se publiquen los binarios.

     1. Nueve tablas CA_*, con los nombres de columna del sistema original
        (snake_case) para que la migracion desde SQLite copie sin traducir.
        CA_empleados agrega IdEmpleado, vinculo OPCIONAL a dbo.Empleados.
     2. Procedimientos Sp_RTA_Curva*: uno por lectura y uno por escritura.

   Que NO hace, a proposito:
     - No siembra etapas ni preguntas: eso es -siembra.sql, y NO se corre si se
       migra desde el SQLite (el catalogo sale de alli, con sus IDs).
     - No pone ON DELETE CASCADE: los borrados en cascada los hacen
       Sp_RTA_CurvaEvaluacionEliminar y Sp_RTA_CurvaEmpleadoEliminar, hijos
       antes que padres, igual que el sistema original.
     - No comprueba perfiles: la barrera esta en AdministrarCurvaAprendizaje.ashx.cs.
     - Los procedimientos de escritura NO abren transaccion (salvo los dos de
       borrado completo): la orquestacion -completar, avanzar de etapa- vive en
       CapaNegocio, que los llama dentro de una SqlTransaction propia.

   Prefijo Sp_RTA_Curva y no Sp_RTA_Ca: "Sp_RTA_Ca%" ya atrapa a
   Sp_RTA_CambiarEstadoUsuario y compania.

   Idempotente: las tablas se crean solo si no existen (sus filas no se
   tocan) y los procedimientos se recrean.
   ============================================================================ */

USE [ReporTarea];
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

PRINT '== Curva de Aprendizaje: esquema - inicio ==';
GO

/* ------------------------------------------------------------ 1. tablas --- */

IF OBJECT_ID('dbo.CA_empleados','U') IS NULL
CREATE TABLE dbo.CA_empleados (
  id               INT IDENTITY(1,1) PRIMARY KEY,
  nombre_completo  NVARCHAR(200) NOT NULL,
  cargo            NVARCHAR(200) NULL,
  area             NVARCHAR(150) NULL,
  empresa          NVARCHAR(150) NULL,
  unidad_negocio   NVARCHAR(150) NULL,
  jefe_id          INT NULL REFERENCES dbo.CA_empleados(id),
  fecha_ingreso    DATE NOT NULL,
  estado           VARCHAR(10) NOT NULL CONSTRAINT DF_CA_emp_estado DEFAULT 'activo'
                   CONSTRAINT CK_CA_emp_estado CHECK (estado IN ('activo','inactivo')),
  created_at       DATETIME2(0) NOT NULL CONSTRAINT DF_CA_emp_created DEFAULT SYSDATETIME(),
  IdEmpleado       BIGINT NULL,       -- vinculo opcional a dbo.Empleados (sin FK, como el resto de la base)
  Usu_Registro     VARCHAR(50) NULL
);
GO

IF OBJECT_ID('dbo.CA_etapas','U') IS NULL
CREATE TABLE dbo.CA_etapas (
  id           INT IDENTITY(1,1) PRIMARY KEY,
  codigo       VARCHAR(5)    NOT NULL UNIQUE,
  nombre       NVARCHAR(100) NOT NULL,
  dias_offset  INT NOT NULL,
  orden        INT NOT NULL
);
GO

IF OBJECT_ID('dbo.CA_preguntas','U') IS NULL
CREATE TABLE dbo.CA_preguntas (
  id              INT IDENTITY(1,1) PRIMARY KEY,
  etapa_id        INT NOT NULL REFERENCES dbo.CA_etapas(id),
  rol_evaluador   VARCHAR(12) NOT NULL CHECK (rol_evaluador IN ('colaborador','jefe')),
  seccion         NVARCHAR(200) NULL,
  texto_pregunta  NVARCHAR(1000) NOT NULL,
  tipo_respuesta  VARCHAR(30) NOT NULL CHECK (tipo_respuesta IN (
                    'escala','opcion_unica','opcion_multiple','texto_abierto','si_no',
                    'decision_periodo_prueba','compromisos','competencias_multi')),
  opciones        NVARCHAR(MAX) NULL,
  orden           INT NOT NULL,
  activa          BIT NOT NULL DEFAULT 1,
  created_at      DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  updated_at      DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
);
GO

IF OBJECT_ID('dbo.CA_procesos_onboarding','U') IS NULL
CREATE TABLE dbo.CA_procesos_onboarding (
  id                       INT IDENTITY(1,1) PRIMARY KEY,
  empleado_id              INT NOT NULL REFERENCES dbo.CA_empleados(id),
  fecha_inicio             DATE NOT NULL,
  estado                   VARCHAR(12) NOT NULL DEFAULT 'en_curso'
                           CHECK (estado IN ('en_curso','aprobado','rechazado','extendido','completado')),
  resumen_ejecutivo_anual  NVARCHAR(MAX) NULL,
  created_at               DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
);
GO

IF OBJECT_ID('dbo.CA_evaluaciones','U') IS NULL
CREATE TABLE dbo.CA_evaluaciones (
  id                 INT IDENTITY(1,1) PRIMARY KEY,
  proceso_id         INT NOT NULL REFERENCES dbo.CA_procesos_onboarding(id),
  etapa_id           INT NOT NULL REFERENCES dbo.CA_etapas(id),
  evaluador_rol      VARCHAR(12) NOT NULL CHECK (evaluador_rol IN ('colaborador','jefe')),
  estado             VARCHAR(10) NOT NULL DEFAULT 'pendiente' CHECK (estado IN ('pendiente','completada')),
  fecha_objetivo     DATE NULL,
  fecha_completada   DATE NULL,
  conclusion         NVARCHAR(MAX) NULL,
  resumen_ejecutivo  NVARCHAR(MAX) NULL,
  plan_accion_gth    NVARCHAR(MAX) NULL,
  token              CHAR(48) NULL,
  created_at         DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  CONSTRAINT UQ_CA_eval UNIQUE (proceso_id, etapa_id, evaluador_rol)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_CA_eval_token' AND object_id = OBJECT_ID('dbo.CA_evaluaciones'))
  CREATE UNIQUE INDEX UX_CA_eval_token ON dbo.CA_evaluaciones(token) WHERE token IS NOT NULL;
GO

IF OBJECT_ID('dbo.CA_respuestas','U') IS NULL
CREATE TABLE dbo.CA_respuestas (
  id               INT IDENTITY(1,1) PRIMARY KEY,
  evaluacion_id    INT NOT NULL REFERENCES dbo.CA_evaluaciones(id),
  pregunta_id      INT NOT NULL REFERENCES dbo.CA_preguntas(id),
  valor_respuesta  NVARCHAR(MAX) NULL,
  created_at       DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  CONSTRAINT UQ_CA_resp UNIQUE (evaluacion_id, pregunta_id)
);
GO

IF OBJECT_ID('dbo.CA_decisiones_periodo_prueba','U') IS NULL
CREATE TABLE dbo.CA_decisiones_periodo_prueba (
  id               INT IDENTITY(1,1) PRIMARY KEY,
  proceso_id       INT NOT NULL REFERENCES dbo.CA_procesos_onboarding(id),
  evaluacion_id    INT NOT NULL REFERENCES dbo.CA_evaluaciones(id),
  decision         VARCHAR(10) NOT NULL CHECK (decision IN ('aprobado','rechazado','extendido')),
  nueva_fecha_fin  DATE NULL,
  comentarios      NVARCHAR(MAX) NULL,
  fecha_decision   DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
);
GO

IF OBJECT_ID('dbo.CA_compromisos','U') IS NULL
CREATE TABLE dbo.CA_compromisos (
  id             INT IDENTITY(1,1) PRIMARY KEY,
  evaluacion_id  INT NOT NULL REFERENCES dbo.CA_evaluaciones(id),
  descripcion    NVARCHAR(1000) NOT NULL,
  categoria      NVARCHAR(200) NULL,
  responsable    NVARCHAR(200) NULL,
  fecha_limite   DATE NULL,
  created_at     DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
);
GO

IF OBJECT_ID('dbo.CA_compromiso_seguimientos','U') IS NULL
CREATE TABLE dbo.CA_compromiso_seguimientos (
  id              INT IDENTITY(1,1) PRIMARY KEY,
  compromiso_id   INT NOT NULL REFERENCES dbo.CA_compromisos(id),
  evaluacion_id   INT NOT NULL REFERENCES dbo.CA_evaluaciones(id),
  estado          VARCHAR(12) NOT NULL CHECK (estado IN ('realizado','en_progreso','pendiente','no_realizado')),
  justificacion   NVARCHAR(1000) NULL,
  fecha_revision  DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  CONSTRAINT UQ_CA_seg UNIQUE (compromiso_id, evaluacion_id)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CA_preg_etapa_rol') CREATE INDEX IX_CA_preg_etapa_rol ON dbo.CA_preguntas(etapa_id, rol_evaluador);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CA_eval_proceso')   CREATE INDEX IX_CA_eval_proceso   ON dbo.CA_evaluaciones(proceso_id);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CA_resp_eval')      CREATE INDEX IX_CA_resp_eval      ON dbo.CA_respuestas(evaluacion_id);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CA_comp_eval')      CREATE INDEX IX_CA_comp_eval      ON dbo.CA_compromisos(evaluacion_id);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CA_seg_comp')       CREATE INDEX IX_CA_seg_comp       ON dbo.CA_compromiso_seguimientos(compromiso_id);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CA_emp_area')       CREATE INDEX IX_CA_emp_area       ON dbo.CA_empleados(area);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CA_proc_empleado')  CREATE INDEX IX_CA_proc_empleado  ON dbo.CA_procesos_onboarding(empleado_id);
GO

/* ===================================================== 2. lecturas ======== */

IF OBJECT_ID('dbo.Sp_RTA_CurvaEtapas','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEtapas;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEtapas
AS
BEGIN
  SET NOCOUNT ON;
  SELECT id, codigo, nombre, dias_offset, orden FROM dbo.CA_etapas ORDER BY orden ASC;
END
GO

/* Preguntas activas de una etapa y rol, en orden. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaPreguntas','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaPreguntas;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaPreguntas
  @EtapaId INT,
  @Rol     VARCHAR(12)
AS
BEGIN
  SET NOCOUNT ON;
  SELECT id, etapa_id, rol_evaluador, seccion, texto_pregunta, tipo_respuesta, opciones, orden, activa
    FROM dbo.CA_preguntas
   WHERE etapa_id = @EtapaId AND rol_evaluador = @Rol AND activa = 1
   ORDER BY orden ASC, id ASC;
END
GO

/* Lista de colaboradores (mas reciente primero) con el estado de su proceso
   mas reciente. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaEmpleadosListar','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEmpleadosListar;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEmpleadosListar
AS
BEGIN
  SET NOCOUNT ON;
  SELECT e.id, e.nombre_completo, e.cargo, e.area, e.empresa, e.unidad_negocio, e.jefe_id,
         e.fecha_ingreso, e.estado, e.created_at, e.IdEmpleado,
         p.id AS proceso_id, p.estado AS proceso_estado
    FROM dbo.CA_empleados e
   OUTER APPLY (SELECT TOP 1 po.id, po.estado FROM dbo.CA_procesos_onboarding po
                 WHERE po.empleado_id = e.id ORDER BY po.created_at DESC, po.id DESC) p
   ORDER BY e.created_at DESC, e.id DESC;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_CurvaEmpleadoPorId','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEmpleadoPorId;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEmpleadoPorId
  @Id INT
AS
BEGIN
  SET NOCOUNT ON;
  SELECT e.id, e.nombre_completo, e.cargo, e.area, e.empresa, e.unidad_negocio, e.jefe_id,
         e.fecha_ingreso, e.estado, e.created_at, e.IdEmpleado,
         p.id AS proceso_id, p.estado AS proceso_estado,
         j.nombre_completo AS jefe_nombre
    FROM dbo.CA_empleados e
    LEFT JOIN dbo.CA_empleados j ON j.id = e.jefe_id
   OUTER APPLY (SELECT TOP 1 po.id, po.estado FROM dbo.CA_procesos_onboarding po
                 WHERE po.empleado_id = e.id ORDER BY po.created_at DESC, po.id DESC) p
   WHERE e.id = @Id;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_CurvaProcesoPorId','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaProcesoPorId;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaProcesoPorId
  @Id INT
AS
BEGIN
  SET NOCOUNT ON;
  SELECT id, empleado_id, fecha_inicio, estado, resumen_ejecutivo_anual, created_at
    FROM dbo.CA_procesos_onboarding WHERE id = @Id;
END
GO

/* Evaluaciones de un proceso, en el orden de la ficha (seccion 8.2). */
IF OBJECT_ID('dbo.Sp_RTA_CurvaEvaluacionesPorProceso','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEvaluacionesPorProceso;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEvaluacionesPorProceso
  @ProcesoId INT
AS
BEGIN
  SET NOCOUNT ON;
  SELECT ev.id, ev.proceso_id, ev.etapa_id, ev.evaluador_rol, ev.estado, ev.fecha_objetivo,
         ev.fecha_completada, ev.conclusion, ev.resumen_ejecutivo, ev.plan_accion_gth, ev.token,
         et.codigo AS etapa_codigo, et.nombre AS etapa_nombre, et.orden AS etapa_orden,
         po.empleado_id
    FROM dbo.CA_evaluaciones ev
    JOIN dbo.CA_etapas et ON et.id = ev.etapa_id
    JOIN dbo.CA_procesos_onboarding po ON po.id = ev.proceso_id
   WHERE ev.proceso_id = @ProcesoId
   ORDER BY et.orden ASC, ev.evaluador_rol ASC;
END
GO

/* Una evaluacion, por id o por token (uno de los dos). */
IF OBJECT_ID('dbo.Sp_RTA_CurvaEvaluacion','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEvaluacion;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEvaluacion
  @Id    INT = NULL,
  @Token CHAR(48) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF @Id IS NULL AND @Token IS NULL RETURN;
  SELECT ev.id, ev.proceso_id, ev.etapa_id, ev.evaluador_rol, ev.estado, ev.fecha_objetivo,
         ev.fecha_completada, ev.conclusion, ev.resumen_ejecutivo, ev.plan_accion_gth, ev.token,
         et.codigo AS etapa_codigo, et.nombre AS etapa_nombre, et.orden AS etapa_orden,
         po.empleado_id
    FROM dbo.CA_evaluaciones ev
    JOIN dbo.CA_etapas et ON et.id = ev.etapa_id
    JOIN dbo.CA_procesos_onboarding po ON po.id = ev.proceso_id
   WHERE (@Id IS NOT NULL AND ev.id = @Id)
      OR (@Id IS NULL AND ev.token = @Token);
END
GO

/* Respuestas de un proceso entero (o de una sola evaluacion), con los datos
   de su pregunta (seccion 8.3). Se lee por proceso para calcular la curva sin
   una consulta por evaluacion. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaRespuestas','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaRespuestas;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaRespuestas
  @ProcesoId    INT = NULL,
  @EvaluacionId INT = NULL
AS
BEGIN
  SET NOCOUNT ON;
  SELECT r.id, r.evaluacion_id, r.pregunta_id, r.valor_respuesta,
         p.texto_pregunta, p.tipo_respuesta, p.seccion, p.orden, p.opciones
    FROM dbo.CA_respuestas r
    JOIN dbo.CA_preguntas p ON p.id = r.pregunta_id
    JOIN dbo.CA_evaluaciones ev ON ev.id = r.evaluacion_id
   WHERE (@EvaluacionId IS NOT NULL AND r.evaluacion_id = @EvaluacionId)
      OR (@EvaluacionId IS NULL AND ev.proceso_id = @ProcesoId)
   ORDER BY r.evaluacion_id ASC, p.orden ASC, p.id ASC;
END
GO

/* Compromisos de un proceso con su etapa de origen, si tienen seguimientos y
   el estado del ultimo (NULL = sin revisar). */
IF OBJECT_ID('dbo.Sp_RTA_CurvaCompromisos','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaCompromisos;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaCompromisos
  @ProcesoId INT
AS
BEGIN
  SET NOCOUNT ON;
  SELECT c.id, c.evaluacion_id, c.descripcion, c.categoria, c.responsable, c.fecha_limite, c.created_at,
         et.codigo AS etapa_codigo, et.nombre AS etapa_nombre, ev.evaluador_rol AS creado_por_rol,
         (SELECT COUNT(*) FROM dbo.CA_compromiso_seguimientos s WHERE s.compromiso_id = c.id) AS n_seguimientos,
         (SELECT TOP 1 s.estado FROM dbo.CA_compromiso_seguimientos s WHERE s.compromiso_id = c.id ORDER BY s.id DESC) AS ultimo_estado
    FROM dbo.CA_compromisos c
    JOIN dbo.CA_evaluaciones ev ON ev.id = c.evaluacion_id
    JOIN dbo.CA_etapas et ON et.id = ev.etapa_id
   WHERE ev.proceso_id = @ProcesoId
   ORDER BY c.created_at ASC, c.id ASC;
END
GO

/* Seguimientos registrados en las evaluaciones de un proceso. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaSeguimientos','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaSeguimientos;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaSeguimientos
  @ProcesoId INT
AS
BEGIN
  SET NOCOUNT ON;
  SELECT s.id, s.compromiso_id, s.evaluacion_id, s.estado, s.justificacion, s.fecha_revision,
         c.descripcion, c.categoria
    FROM dbo.CA_compromiso_seguimientos s
    JOIN dbo.CA_evaluaciones ev ON ev.id = s.evaluacion_id
    JOIN dbo.CA_compromisos c ON c.id = s.compromiso_id
   WHERE ev.proceso_id = @ProcesoId
   ORDER BY s.id ASC;
END
GO

/* Compromisos abiertos para la evaluacion que se esta llenando (seccion 8.1).
   Ademas de los abiertos, entrega los que se revisaron EN esta misma
   evaluacion, con el estado y la justificacion que se les dio: al editar,
   el bloque de seguimiento se prellena (seccion 18, punto 2). En una
   evaluacion pendiente no hay ninguno de esos, asi que la consulta es la
   original. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaCompromisosAbiertos','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaCompromisosAbiertos;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaCompromisosAbiertos
  @EvaluacionId INT
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @ProcesoId INT, @Orden INT;
  SELECT @ProcesoId = ev.proceso_id, @Orden = et.orden
    FROM dbo.CA_evaluaciones ev JOIN dbo.CA_etapas et ON et.id = ev.etapa_id
   WHERE ev.id = @EvaluacionId;

  SELECT c.id, c.evaluacion_id, c.descripcion, c.categoria, c.responsable, c.fecha_limite, c.created_at,
         et.nombre AS etapa_nombre, et.codigo AS etapa_codigo, ev.evaluador_rol AS creado_por_rol,
         propio.estado AS estado_actual, propio.justificacion AS justificacion_actual
    FROM dbo.CA_compromisos c
    JOIN dbo.CA_evaluaciones ev ON ev.id = c.evaluacion_id
    JOIN dbo.CA_etapas et ON et.id = ev.etapa_id
    LEFT JOIN dbo.CA_compromiso_seguimientos propio
           ON propio.compromiso_id = c.id AND propio.evaluacion_id = @EvaluacionId
   WHERE ev.proceso_id = @ProcesoId
     AND c.evaluacion_id <> @EvaluacionId
     AND et.orden < @Orden
     AND (propio.id IS NOT NULL
          OR c.id NOT IN (
              SELECT cs.compromiso_id FROM dbo.CA_compromiso_seguimientos cs
               WHERE cs.id IN (SELECT MAX(id) FROM dbo.CA_compromiso_seguimientos GROUP BY compromiso_id)
                 AND cs.estado IN ('realizado','no_realizado')))
   ORDER BY c.created_at ASC, c.id ASC;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_CurvaDecisiones','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaDecisiones;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaDecisiones
  @ProcesoId INT
AS
BEGIN
  SET NOCOUNT ON;
  SELECT id, proceso_id, evaluacion_id, decision, nueva_fecha_fin, comentarios, fecha_decision
    FROM dbo.CA_decisiones_periodo_prueba WHERE proceso_id = @ProcesoId ORDER BY id ASC;
END
GO

/* Insumos del dashboard (seccion 7.11), en nueve conjuntos de resultados.
   Los agregados se calculan en CapaNegocio (NegCurvaDashboard), que tiene
   pruebas; aqui solo se lee. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaDashboard','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaDashboard;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaDashboard
AS
BEGIN
  SET NOCOUNT ON;
  -- 1. total de colaboradores
  SELECT COUNT(*) AS total FROM dbo.CA_empleados;
  -- 2. procesos con su colaborador
  SELECT p.id, p.empleado_id, p.estado, e.nombre_completo, e.area
    FROM dbo.CA_procesos_onboarding p JOIN dbo.CA_empleados e ON e.id = p.empleado_id;
  -- 3. decisiones con el area del colaborador
  SELECT d.decision, e.area
    FROM dbo.CA_decisiones_periodo_prueba d
    JOIN dbo.CA_procesos_onboarding p ON p.id = d.proceso_id
    JOIN dbo.CA_empleados e ON e.id = p.empleado_id;
  -- 4. compromisos con categoria, area y el estado de su ultimo seguimiento
  SELECT c.id, c.categoria, e.area,
         (SELECT TOP 1 s.estado FROM dbo.CA_compromiso_seguimientos s WHERE s.compromiso_id = c.id ORDER BY s.id DESC) AS ultimo_estado
    FROM dbo.CA_compromisos c
    JOIN dbo.CA_evaluaciones ev ON ev.id = c.evaluacion_id
    JOIN dbo.CA_procesos_onboarding p ON p.id = ev.proceso_id
    JOIN dbo.CA_empleados e ON e.id = p.empleado_id;
  -- 5. respuestas de competencias con su etapa
  SELECT r.evaluacion_id, et.codigo AS etapa_codigo, r.valor_respuesta
    FROM dbo.CA_respuestas r
    JOIN dbo.CA_preguntas pr ON pr.id = r.pregunta_id
    JOIN dbo.CA_evaluaciones ev ON ev.id = r.evaluacion_id
    JOIN dbo.CA_etapas et ON et.id = ev.etapa_id
   WHERE pr.tipo_respuesta = 'competencias_multi';
  -- 6. proximas evaluaciones (seccion 8.6)
  SELECT TOP 20 ev.id, ev.evaluador_rol, ev.fecha_objetivo, et.codigo AS etapa_codigo, et.nombre AS etapa_nombre,
         emp.id AS empleado_id, emp.nombre_completo, emp.area,
         CASE WHEN ev.fecha_objetivo < CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END AS vencida
    FROM dbo.CA_evaluaciones ev
    JOIN dbo.CA_etapas et ON et.id = ev.etapa_id
    JOIN dbo.CA_procesos_onboarding p ON p.id = ev.proceso_id
    JOIN dbo.CA_empleados emp ON emp.id = p.empleado_id
   WHERE ev.estado = 'pendiente' AND p.estado <> 'rechazado'
   ORDER BY ev.fecha_objetivo ASC, ev.id ASC;
  -- 7..9. insumos de la curva de los procesos en curso o extendidos
  SELECT ev.id, ev.proceso_id, ev.evaluador_rol, ev.estado, et.codigo AS etapa_codigo
    FROM dbo.CA_evaluaciones ev
    JOIN dbo.CA_etapas et ON et.id = ev.etapa_id
    JOIN dbo.CA_procesos_onboarding p ON p.id = ev.proceso_id
   WHERE ev.estado = 'completada' AND p.estado IN ('en_curso','extendido');
  SELECT r.evaluacion_id, r.valor_respuesta, pr.tipo_respuesta, pr.opciones
    FROM dbo.CA_respuestas r
    JOIN dbo.CA_preguntas pr ON pr.id = r.pregunta_id
    JOIN dbo.CA_evaluaciones ev ON ev.id = r.evaluacion_id
    JOIN dbo.CA_procesos_onboarding p ON p.id = ev.proceso_id
   WHERE ev.estado = 'completada' AND p.estado IN ('en_curso','extendido')
   ORDER BY r.evaluacion_id, pr.orden, pr.id;
  SELECT s.evaluacion_id, s.estado
    FROM dbo.CA_compromiso_seguimientos s
    JOIN dbo.CA_evaluaciones ev ON ev.id = s.evaluacion_id
    JOIN dbo.CA_procesos_onboarding p ON p.id = ev.proceso_id
   WHERE ev.estado = 'completada' AND p.estado IN ('en_curso','extendido');
END
GO

/* Busca en dbo.Empleados (fichas activas) para enlazar al colaborador nuevo
   con su ficha del sistema. Solo lectura; el vinculo es opcional. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaBuscarEmpleadoSistema','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaBuscarEmpleadoSistema;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaBuscarEmpleadoSistema
  @Texto NVARCHAR(100)
AS
BEGIN
  SET NOCOUNT ON;
  SELECT TOP 20 e.IdEmpleado, e.Nombre, e.PuestoTrabajo, e.AreaTrabajo, e.Sociedad
    FROM dbo.Empleados e
   WHERE e.Estado = 'Activo'
     AND LEN(LTRIM(RTRIM(@Texto))) >= 3
     AND e.Nombre LIKE '%' + LTRIM(RTRIM(@Texto)) + '%'
   ORDER BY e.Nombre ASC;
END
GO

/* ==================================================== 3. escrituras ====== */

IF OBJECT_ID('dbo.Sp_RTA_CurvaEmpleadoCrear','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEmpleadoCrear;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEmpleadoCrear
  @NombreCompleto NVARCHAR(200),
  @Cargo          NVARCHAR(200) = NULL,
  @Area           NVARCHAR(150) = NULL,
  @Empresa        NVARCHAR(150) = NULL,
  @UnidadNegocio  NVARCHAR(150) = NULL,
  @FechaIngreso   DATE,
  @IdEmpleado     BIGINT = NULL,
  @Usuario        VARCHAR(50) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO dbo.CA_empleados (nombre_completo, cargo, area, empresa, unidad_negocio, fecha_ingreso, estado, IdEmpleado, Usu_Registro)
  VALUES (@NombreCompleto, @Cargo, @Area, @Empresa, @UnidadNegocio, @FechaIngreso, 'activo', @IdEmpleado, @Usuario);
  SELECT CONVERT(INT, SCOPE_IDENTITY()) AS id;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_CurvaEmpleadoEstado','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEmpleadoEstado;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEmpleadoEstado
  @Id     INT,
  @Estado VARCHAR(10)
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE dbo.CA_empleados SET estado = @Estado WHERE id = @Id;
  SELECT @@ROWCOUNT AS filas;
END
GO

/* Borra al colaborador con todo su historial (seccion 7.9), en una sola
   transaccion. Devuelve filas = 0 si no existia. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaEmpleadoEliminar','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEmpleadoEliminar;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEmpleadoEliminar
  @Id INT
AS
BEGIN
  SET NOCOUNT ON;
  SET XACT_ABORT ON;
  IF NOT EXISTS (SELECT 1 FROM dbo.CA_empleados WHERE id = @Id) BEGIN SELECT 0 AS filas; RETURN; END

  DECLARE @Evals TABLE (id INT PRIMARY KEY);
  INSERT INTO @Evals (id)
  SELECT ev.id FROM dbo.CA_evaluaciones ev
    JOIN dbo.CA_procesos_onboarding p ON p.id = ev.proceso_id
   WHERE p.empleado_id = @Id;

  BEGIN TRANSACTION;
    DELETE FROM dbo.CA_compromiso_seguimientos WHERE evaluacion_id IN (SELECT id FROM @Evals);
    DELETE FROM dbo.CA_compromiso_seguimientos
     WHERE compromiso_id IN (SELECT c.id FROM dbo.CA_compromisos c WHERE c.evaluacion_id IN (SELECT id FROM @Evals));
    DELETE FROM dbo.CA_compromisos WHERE evaluacion_id IN (SELECT id FROM @Evals);
    DELETE FROM dbo.CA_decisiones_periodo_prueba WHERE evaluacion_id IN (SELECT id FROM @Evals);
    DELETE FROM dbo.CA_decisiones_periodo_prueba
     WHERE proceso_id IN (SELECT id FROM dbo.CA_procesos_onboarding WHERE empleado_id = @Id);
    DELETE FROM dbo.CA_respuestas WHERE evaluacion_id IN (SELECT id FROM @Evals);
    DELETE FROM dbo.CA_evaluaciones WHERE id IN (SELECT id FROM @Evals);
    DELETE FROM dbo.CA_procesos_onboarding WHERE empleado_id = @Id;
    UPDATE dbo.CA_empleados SET jefe_id = NULL WHERE jefe_id = @Id;
    DELETE FROM dbo.CA_empleados WHERE id = @Id;
  COMMIT TRANSACTION;
  SELECT 1 AS filas;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_CurvaProcesoCrear','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaProcesoCrear;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaProcesoCrear
  @EmpleadoId  INT,
  @FechaInicio DATE
AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO dbo.CA_procesos_onboarding (empleado_id, fecha_inicio, estado) VALUES (@EmpleadoId, @FechaInicio, 'en_curso');
  SELECT CONVERT(INT, SCOPE_IDENTITY()) AS id;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_CurvaProcesoActualizar','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaProcesoActualizar;
GO
/* @Estado NULL = no tocar el estado; @CambiarResumen = 1 escribe @Resumen. */
CREATE PROCEDURE dbo.Sp_RTA_CurvaProcesoActualizar
  @Id             INT,
  @Estado         VARCHAR(12) = NULL,
  @CambiarResumen BIT = 0,
  @Resumen        NVARCHAR(MAX) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE dbo.CA_procesos_onboarding
     SET estado = ISNULL(@Estado, estado),
         resumen_ejecutivo_anual = CASE WHEN @CambiarResumen = 1 THEN @Resumen ELSE resumen_ejecutivo_anual END
   WHERE id = @Id;
  SELECT @@ROWCOUNT AS filas;
END
GO

/* Crea la evaluacion de (proceso, etapa, rol) si no existe. Idempotente:
   creada = 0 si ya estaba. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaEvaluacionCrear','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEvaluacionCrear;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEvaluacionCrear
  @ProcesoId     INT,
  @EtapaId       INT,
  @Rol           VARCHAR(12),
  @FechaObjetivo DATE
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @IdExistente INT = (SELECT id FROM dbo.CA_evaluaciones WITH (UPDLOCK, HOLDLOCK)
                               WHERE proceso_id = @ProcesoId AND etapa_id = @EtapaId AND evaluador_rol = @Rol);
  IF @IdExistente IS NOT NULL BEGIN SELECT @IdExistente AS id, 0 AS creada; RETURN; END
  INSERT INTO dbo.CA_evaluaciones (proceso_id, etapa_id, evaluador_rol, estado, fecha_objetivo)
  VALUES (@ProcesoId, @EtapaId, @Rol, 'pendiente', @FechaObjetivo);
  SELECT CONVERT(INT, SCOPE_IDENTITY()) AS id, 1 AS creada;
END
GO

/* Escribe los campos de una evaluacion.
   @Completar = 1: pasa de pendiente a completada con fecha de hoy; si ya
   estaba completada no toca nada y devuelve filas = 0 (eso frena el doble
   envio aunque dos peticiones lleguen a la vez).
   El token NO se borra al completar: reabrir el enlace tiene que mostrar
   "Gracias" (ya completada), no "enlace invalido". Un enlace usado ya no
   sirve para nada mas porque la evaluacion dejo de estar pendiente.
   @Completar = 0: solo cambia lo que se indica, sin tocar estado ni
   fecha_completada (editar). */
IF OBJECT_ID('dbo.Sp_RTA_CurvaEvaluacionActualizar','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEvaluacionActualizar;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEvaluacionActualizar
  @Id                INT,
  @Completar         BIT = 0,
  @CambiarConclusion BIT = 0,
  @Conclusion        NVARCHAR(MAX) = NULL,
  @CambiarResumen    BIT = 0,
  @Resumen           NVARCHAR(MAX) = NULL,
  @CambiarPlan       BIT = 0,
  @Plan              NVARCHAR(MAX) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE dbo.CA_evaluaciones
     SET estado            = CASE WHEN @Completar = 1 THEN 'completada' ELSE estado END,
         fecha_completada  = CASE WHEN @Completar = 1 THEN CAST(GETDATE() AS DATE) ELSE fecha_completada END,
         conclusion        = CASE WHEN @CambiarConclusion = 1 THEN @Conclusion ELSE conclusion END,
         resumen_ejecutivo = CASE WHEN @CambiarResumen = 1 THEN @Resumen ELSE resumen_ejecutivo END,
         plan_accion_gth   = CASE WHEN @CambiarPlan = 1 THEN @Plan ELSE plan_accion_gth END
   WHERE id = @Id
     AND (@Completar = 0 OR estado = 'pendiente');
  SELECT @@ROWCOUNT AS filas;
END
GO

/* Guarda el token del enlace remoto; solo sobre una evaluacion pendiente.
   Sobrescribir invalida el anterior. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaEvaluacionToken','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEvaluacionToken;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEvaluacionToken
  @Id    INT,
  @Token CHAR(48)
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE dbo.CA_evaluaciones SET token = @Token WHERE id = @Id AND estado = 'pendiente';
  SELECT @@ROWCOUNT AS filas;
END
GO

/* Elimina una evaluacion y revierte sus efectos (seccion 7.8).
   Respuestas: 0 bien, -1 no existe, -2 el proceso ya avanzo a una etapa
   posterior, -3 algun compromiso creado aqui tiene seguimiento en otra
   evaluacion. Los textos para el usuario los pone CapaNegocio. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaEvaluacionEliminar','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaEvaluacionEliminar;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaEvaluacionEliminar
  @Id INT
AS
BEGIN
  SET NOCOUNT ON;
  SET XACT_ABORT ON;
  DECLARE @ProcesoId INT, @Orden INT;
  SELECT @ProcesoId = ev.proceso_id, @Orden = et.orden
    FROM dbo.CA_evaluaciones ev JOIN dbo.CA_etapas et ON et.id = ev.etapa_id
   WHERE ev.id = @Id;
  IF @ProcesoId IS NULL BEGIN SELECT -1 AS Respuestas; RETURN; END

  IF EXISTS (SELECT 1 FROM dbo.CA_evaluaciones ev JOIN dbo.CA_etapas et ON et.id = ev.etapa_id
              WHERE ev.proceso_id = @ProcesoId AND et.orden > @Orden)
  BEGIN SELECT -2 AS Respuestas; RETURN; END

  IF EXISTS (SELECT 1 FROM dbo.CA_compromiso_seguimientos s
               JOIN dbo.CA_compromisos c ON c.id = s.compromiso_id
              WHERE c.evaluacion_id = @Id AND s.evaluacion_id <> @Id)
  BEGIN SELECT -3 AS Respuestas; RETURN; END

  BEGIN TRANSACTION;
    DELETE FROM dbo.CA_compromiso_seguimientos WHERE evaluacion_id = @Id;
    DELETE FROM dbo.CA_compromiso_seguimientos WHERE compromiso_id IN (SELECT id FROM dbo.CA_compromisos WHERE evaluacion_id = @Id);
    DELETE FROM dbo.CA_compromisos WHERE evaluacion_id = @Id;
    IF EXISTS (SELECT 1 FROM dbo.CA_decisiones_periodo_prueba WHERE evaluacion_id = @Id)
    BEGIN
      DELETE FROM dbo.CA_decisiones_periodo_prueba WHERE evaluacion_id = @Id;
      UPDATE dbo.CA_procesos_onboarding SET estado = 'en_curso' WHERE id = @ProcesoId;
    END
    DELETE FROM dbo.CA_respuestas WHERE evaluacion_id = @Id;
    DELETE FROM dbo.CA_evaluaciones WHERE id = @Id;
  COMMIT TRANSACTION;
  SELECT 0 AS Respuestas;
END
GO

/* Upsert de una respuesta (seccion 8.4). */
IF OBJECT_ID('dbo.Sp_RTA_CurvaRespuestaGuardar','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaRespuestaGuardar;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaRespuestaGuardar
  @EvaluacionId INT,
  @PreguntaId   INT,
  @Valor        NVARCHAR(MAX)
AS
BEGIN
  SET NOCOUNT ON;
  MERGE dbo.CA_respuestas WITH (HOLDLOCK) AS t
  USING (SELECT @EvaluacionId AS evaluacion_id, @PreguntaId AS pregunta_id) AS s
     ON t.evaluacion_id = s.evaluacion_id AND t.pregunta_id = s.pregunta_id
  WHEN MATCHED THEN UPDATE SET valor_respuesta = @Valor
  WHEN NOT MATCHED THEN INSERT (evaluacion_id, pregunta_id, valor_respuesta) VALUES (@EvaluacionId, @PreguntaId, @Valor);
END
GO

IF OBJECT_ID('dbo.Sp_RTA_CurvaCompromisoCrear','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaCompromisoCrear;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaCompromisoCrear
  @EvaluacionId INT,
  @Descripcion  NVARCHAR(1000),
  @Categoria    NVARCHAR(200) = NULL,
  @Responsable  NVARCHAR(200) = NULL,
  @FechaLimite  DATE = NULL
AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO dbo.CA_compromisos (evaluacion_id, descripcion, categoria, responsable, fecha_limite)
  VALUES (@EvaluacionId, @Descripcion, @Categoria, @Responsable, @FechaLimite);
  SELECT CONVERT(INT, SCOPE_IDENTITY()) AS id;
END
GO

/* Al editar: borra los compromisos de la evaluacion que no tienen ningun
   seguimiento. Los que ya tienen seguimiento se conservan. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaCompromisosBorrarSinSeguimiento','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaCompromisosBorrarSinSeguimiento;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaCompromisosBorrarSinSeguimiento
  @EvaluacionId INT
AS
BEGIN
  SET NOCOUNT ON;
  DELETE c FROM dbo.CA_compromisos c
   WHERE c.evaluacion_id = @EvaluacionId
     AND NOT EXISTS (SELECT 1 FROM dbo.CA_compromiso_seguimientos s WHERE s.compromiso_id = c.id);
  SELECT @@ROWCOUNT AS filas;
END
GO

/* Upsert del seguimiento de un compromiso en una evaluacion. Solo acepta
   compromisos del mismo proceso que la evaluacion: filas = 0 si no. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaSeguimientoGuardar','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaSeguimientoGuardar;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaSeguimientoGuardar
  @CompromisoId  INT,
  @EvaluacionId  INT,
  @Estado        VARCHAR(12),
  @Justificacion NVARCHAR(1000) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF NOT EXISTS (SELECT 1 FROM dbo.CA_compromisos c
                   JOIN dbo.CA_evaluaciones ec ON ec.id = c.evaluacion_id
                   JOIN dbo.CA_evaluaciones ea ON ea.id = @EvaluacionId
                  WHERE c.id = @CompromisoId AND ec.proceso_id = ea.proceso_id AND c.evaluacion_id <> @EvaluacionId)
  BEGIN SELECT 0 AS filas; RETURN; END

  MERGE dbo.CA_compromiso_seguimientos WITH (HOLDLOCK) AS t
  USING (SELECT @CompromisoId AS compromiso_id, @EvaluacionId AS evaluacion_id) AS s
     ON t.compromiso_id = s.compromiso_id AND t.evaluacion_id = s.evaluacion_id
  WHEN MATCHED THEN UPDATE SET estado = @Estado, justificacion = @Justificacion, fecha_revision = SYSDATETIME()
  WHEN NOT MATCHED THEN INSERT (compromiso_id, evaluacion_id, estado, justificacion) VALUES (@CompromisoId, @EvaluacionId, @Estado, @Justificacion);
  SELECT 1 AS filas;
END
GO

/* Decision de periodo de prueba: actualiza la de esa evaluacion si existe
   (y vuelve a fijar fecha_decision) o la crea. */
IF OBJECT_ID('dbo.Sp_RTA_CurvaDecisionGuardar','P') IS NOT NULL DROP PROCEDURE dbo.Sp_RTA_CurvaDecisionGuardar;
GO
CREATE PROCEDURE dbo.Sp_RTA_CurvaDecisionGuardar
  @ProcesoId     INT,
  @EvaluacionId  INT,
  @Decision      VARCHAR(10),
  @NuevaFechaFin DATE = NULL,
  @Comentarios   NVARCHAR(MAX) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF EXISTS (SELECT 1 FROM dbo.CA_decisiones_periodo_prueba WITH (UPDLOCK, HOLDLOCK) WHERE evaluacion_id = @EvaluacionId)
    UPDATE dbo.CA_decisiones_periodo_prueba
       SET decision = @Decision, nueva_fecha_fin = @NuevaFechaFin, comentarios = @Comentarios, fecha_decision = SYSDATETIME()
     WHERE evaluacion_id = @EvaluacionId;
  ELSE
    INSERT INTO dbo.CA_decisiones_periodo_prueba (proceso_id, evaluacion_id, decision, nueva_fecha_fin, comentarios)
    VALUES (@ProcesoId, @EvaluacionId, @Decision, @NuevaFechaFin, @Comentarios);
END
GO

/* ================================================== 4. verificacion ====== */
SET NOCOUNT ON;
DECLARE @Fallos INT = 0;

IF (SELECT COUNT(*) FROM sys.tables WHERE name IN ('CA_empleados','CA_etapas','CA_preguntas','CA_procesos_onboarding',
      'CA_evaluaciones','CA_respuestas','CA_decisiones_periodo_prueba','CA_compromisos','CA_compromiso_seguimientos')) <> 9
BEGIN PRINT 'FALLO: no estan las 9 tablas CA_*.'; SET @Fallos += 1; END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CA_empleados') AND name = 'IdEmpleado')
BEGIN PRINT 'FALLO: falta CA_empleados.IdEmpleado.'; SET @Fallos += 1; END

DECLARE @NProc INT = (SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'Sp[_]RTA[_]Curva%');
IF @NProc <> 28
BEGIN PRINT 'FALLO: se esperaban 28 procedimientos Sp_RTA_Curva*, hay ' + CAST(@NProc AS VARCHAR(10)) + '.'; SET @Fallos += 1; END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_CA_eval_token' AND has_filter = 1)
BEGIN PRINT 'FALLO: falta el indice filtrado del token.'; SET @Fallos += 1; END

IF @Fallos = 0 PRINT 'OK: esquema de Curva de Aprendizaje listo.'
ELSE PRINT 'VEREDICTO: FALLO (' + CAST(@Fallos AS VARCHAR(10)) + ' comprobaciones). Revisar arriba.';
GO
