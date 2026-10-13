# Guía técnica para parametrizar horarios laborales y calcular horas suplementarias y extraordinarias

## 1. Objetivo

Implementar una parametrización de horarios laborales por empleado para que el sistema determine correctamente:

- Horas normales.
- Horas suplementarias al `50%`.
- Horas extraordinarias al `100%`.
- Tramos mixtos que deben registrarse por separado.
- Feriados nacionales de Ecuador.
- Usuarios existentes en `R_Usuarios` que no constan en el archivo Excel.

La implementación está diseñada para integrarse con:

```text
Base de datos: ReporTarea
Tabla de actividades: dbo.R_DetTareasAranda
Tabla de usuarios: dbo.R_Usuarios
SP de registro: dbo.Sp_RTAInsertaDetalleTarea_V2
```

> **Importante:** antes de desplegar en producción, Recursos Humanos o el área legal debe confirmar que estas reglas aplican a todos los tipos de contrato administrados por el sistema.

---

## 2. Resultado del análisis del archivo Excel

Archivo revisado:

```text
Listado de contratos CAS con horarios .xlsx
```

El archivo contiene `33` empleados y cuatro jornadas laborales diferentes:

| Código propuesto | Horario laboral | Cantidad de empleados |
|---|---:|---:|
| `H0830_1730` | `08:30 a 17:30` | `14` |
| `H0800_1700` | `08:00 a 17:00` | `8` |
| `H0830_1230` | `08:30 a 12:30` | `8` |
| `H0800_1630` | `08:00 a 16:30` | `3` |

No se debe utilizar un único horario global para todos los usuarios.

Los empleados adicionales registrados en `dbo.R_Usuarios` que no consten en el Excel utilizarán inicialmente el horario predeterminado:

```text
08:30 a 17:30
```

Este valor debe poder modificarse desde una tabla de parametrización.

---

## 3. Reglas funcionales

### 3.1 Tipos de horas

| Tipo | Descripción | Valor almacenado |
|---|---|---:|
| Horas normales | Dentro del horario laboral parametrizado para el empleado | `0` |
| Horas suplementarias | Fuera del horario laboral, de lunes a viernes, entre `06:00` y `24:00` | `1` |
| Horas extraordinarias | Entre `00:00` y `06:00`, en feriados o en días no laborables | `2` |

### 3.2 Campos de `R_DetTareasAranda`

Cuando el horario sea válido y corresponda a horas adicionales, se deben guardar estos valores:

| Campo | Horas suplementarias `50%` | Horas extraordinarias `100%` |
|---|---:|---:|
| `Det_Horas_Extras_Tipo` | `1` | `2` |
| `Det_Horas_Extras_Estado` | `1` | `2` |
| `Det_Horas_Extras_Descripcion` | `50%` | `100%` |

Para horas normales:

```text
Det_Horas_Extras_Tipo        = 0
Det_Horas_Extras_Estado      = 0
Det_Horas_Extras_Descripcion = ''
```

### 3.3 Rangos mixtos

Si una actividad combina distintos tipos de horas, no se debe guardar como un único registro.

Ejemplo para un empleado con jornada `08:30 a 17:30`:

```text
Inicio: 07:00
Fin:    18:30
```

El SP debe devolver:

```text
El horario enviado contiene diferentes tipos de horas.
Registre los siguientes tramos por separado:
07:00 a 08:30 = HORAS SUPLEMENTARIAS 50%;
08:30 a 17:30 = HORAS NORMALES;
17:30 a 18:30 = HORAS SUPLEMENTARIAS 50%.
```

---

## 4. Diseño propuesto

Se recomienda crear estas tablas:

```text
dbo.R_HorarioLaboral
dbo.R_HorarioLaboralDetalle
dbo.R_UsuarioHorarioLaboral
dbo.Feriado
```

Relación principal:

```text
R_Usuarios
    │
    └── R_UsuarioHorarioLaboral
            │
            └── R_HorarioLaboral
                    │
                    └── R_HorarioLaboralDetalle
```

La función utilizará este orden:

1. Buscar el horario específico del empleado.
2. Si no existe, utilizar el horario marcado como predeterminado.
3. Buscar el detalle del día de la semana.
4. Verificar si la fecha es feriado.
5. Dividir el rango recibido en tramos.
6. Clasificar cada tramo como normal, suplementario o extraordinario.

---

## 5. Script SQL: agregar el campo de estado

Ejecute este bloque una sola vez.

```sql
USE [ReporTarea];
GO

IF COL_LENGTH('dbo.R_DetTareasAranda', 'Det_Horas_Extras_Estado') IS NULL
BEGIN
    ALTER TABLE dbo.R_DetTareasAranda
    ADD Det_Horas_Extras_Estado BIGINT NOT NULL
        CONSTRAINT DF_R_DetTareasAranda_HorasExtrasEstado
        DEFAULT (0) WITH VALUES;
END;
GO
```

---

## 6. Script SQL: crear perfiles de horario

```sql
USE [ReporTarea];
GO

IF OBJECT_ID(N'dbo.R_HorarioLaboral', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.R_HorarioLaboral
    (
        IdHorarioLaboral    INT IDENTITY(1,1) NOT NULL,
        Codigo              VARCHAR(30) NOT NULL,
        Nombre              VARCHAR(120) NOT NULL,
        EsPredeterminado    BIT NOT NULL
            CONSTRAINT DF_R_HorarioLaboral_Predeterminado DEFAULT (0),
        Activo              BIT NOT NULL
            CONSTRAINT DF_R_HorarioLaboral_Activo DEFAULT (1),
        FechaRegistro       DATETIME2(0) NOT NULL
            CONSTRAINT DF_R_HorarioLaboral_FechaRegistro
            DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_R_HorarioLaboral
            PRIMARY KEY (IdHorarioLaboral),

        CONSTRAINT UQ_R_HorarioLaboral_Codigo
            UNIQUE (Codigo)
    );
END;
GO

IF OBJECT_ID(N'dbo.R_HorarioLaboralDetalle', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.R_HorarioLaboralDetalle
    (
        IdHorarioLaboralDetalle BIGINT IDENTITY(1,1) NOT NULL,
        IdHorarioLaboral        INT NOT NULL,
        DiaSemana               TINYINT NOT NULL,
        NombreDia               VARCHAR(15) NOT NULL,
        EsLaborable             BIT NOT NULL,
        HoraInicio              TIME(0) NULL,
        HoraFin                 TIME(0) NULL,
        Activo                  BIT NOT NULL
            CONSTRAINT DF_R_HorarioLaboralDetalle_Activo DEFAULT (1),

        CONSTRAINT PK_R_HorarioLaboralDetalle
            PRIMARY KEY (IdHorarioLaboralDetalle),

        CONSTRAINT FK_R_HorarioLaboralDetalle_Horario
            FOREIGN KEY (IdHorarioLaboral)
            REFERENCES dbo.R_HorarioLaboral (IdHorarioLaboral),

        CONSTRAINT UQ_R_HorarioLaboralDetalle
            UNIQUE (IdHorarioLaboral, DiaSemana),

        CONSTRAINT CK_R_HorarioLaboralDetalle_DiaSemana
            CHECK (DiaSemana BETWEEN 1 AND 7),

        CONSTRAINT CK_R_HorarioLaboralDetalle_Horas
            CHECK
            (
                (
                    EsLaborable = 0
                    AND HoraInicio IS NULL
                    AND HoraFin IS NULL
                )
                OR
                (
                    EsLaborable = 1
                    AND HoraInicio IS NOT NULL
                    AND HoraFin IS NOT NULL
                    AND HoraInicio < HoraFin
                )
            )
    );
END;
GO
```

---

## 7. Script SQL: registrar los cuatro perfiles identificados en el Excel

```sql
USE [ReporTarea];
GO

MERGE dbo.R_HorarioLaboral AS Destino
USING
(
    SELECT 'H0830_1730' AS Codigo, 'Jornada general 08:30 a 17:30' AS Nombre, CAST(1 AS BIT) AS EsPredeterminado
    UNION ALL
    SELECT 'H0800_1700', 'Jornada 08:00 a 17:00', CAST(0 AS BIT)
    UNION ALL
    SELECT 'H0830_1230', 'Jornada 08:30 a 12:30', CAST(0 AS BIT)
    UNION ALL
    SELECT 'H0800_1630', 'Jornada 08:00 a 16:30', CAST(0 AS BIT)
) AS Origen
ON Destino.Codigo = Origen.Codigo

WHEN MATCHED THEN
    UPDATE SET
        Destino.Nombre = Origen.Nombre,
        Destino.EsPredeterminado = Origen.EsPredeterminado,
        Destino.Activo = 1

WHEN NOT MATCHED THEN
    INSERT
    (
        Codigo,
        Nombre,
        EsPredeterminado,
        Activo
    )
    VALUES
    (
        Origen.Codigo,
        Origen.Nombre,
        Origen.EsPredeterminado,
        1
    );
GO

/* ============================================================
   Lunes a viernes laborables.
   Sábado y domingo no laborables.
   ============================================================ */

DECLARE @Horarios TABLE
(
    Codigo      VARCHAR(30),
    HoraInicio  TIME(0),
    HoraFin     TIME(0)
);

INSERT INTO @Horarios
(
    Codigo,
    HoraInicio,
    HoraFin
)
VALUES
    ('H0830_1730', '08:30:00', '17:30:00'),
    ('H0800_1700', '08:00:00', '17:00:00'),
    ('H0830_1230', '08:30:00', '12:30:00'),
    ('H0800_1630', '08:00:00', '16:30:00');

DECLARE @Dias TABLE
(
    DiaSemana   TINYINT,
    NombreDia   VARCHAR(15),
    EsLaborable BIT
);

INSERT INTO @Dias
(
    DiaSemana,
    NombreDia,
    EsLaborable
)
VALUES
    (1, 'LUNES',     1),
    (2, 'MARTES',    1),
    (3, 'MIERCOLES', 1),
    (4, 'JUEVES',    1),
    (5, 'VIERNES',   1),
    (6, 'SABADO',    0),
    (7, 'DOMINGO',   0);

MERGE dbo.R_HorarioLaboralDetalle AS Destino
USING
(
    SELECT
        Horario.IdHorarioLaboral,
        Dia.DiaSemana,
        Dia.NombreDia,
        Dia.EsLaborable,
        CASE
            WHEN Dia.EsLaborable = 1 THEN Parametro.HoraInicio
            ELSE NULL
        END AS HoraInicio,
        CASE
            WHEN Dia.EsLaborable = 1 THEN Parametro.HoraFin
            ELSE NULL
        END AS HoraFin
    FROM @Horarios AS Parametro
    INNER JOIN dbo.R_HorarioLaboral AS Horario
        ON Horario.Codigo = Parametro.Codigo
    CROSS JOIN @Dias AS Dia
) AS Origen
ON Destino.IdHorarioLaboral = Origen.IdHorarioLaboral
AND Destino.DiaSemana = Origen.DiaSemana

WHEN MATCHED THEN
    UPDATE SET
        Destino.NombreDia = Origen.NombreDia,
        Destino.EsLaborable = Origen.EsLaborable,
        Destino.HoraInicio = Origen.HoraInicio,
        Destino.HoraFin = Origen.HoraFin,
        Destino.Activo = 1

WHEN NOT MATCHED THEN
    INSERT
    (
        IdHorarioLaboral,
        DiaSemana,
        NombreDia,
        EsLaborable,
        HoraInicio,
        HoraFin,
        Activo
    )
    VALUES
    (
        Origen.IdHorarioLaboral,
        Origen.DiaSemana,
        Origen.NombreDia,
        Origen.EsLaborable,
        Origen.HoraInicio,
        Origen.HoraFin,
        1
    );
GO
```

---

## 8. Script SQL: asignar un horario por empleado

```sql
USE [ReporTarea];
GO

IF OBJECT_ID(N'dbo.R_UsuarioHorarioLaboral', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.R_UsuarioHorarioLaboral
    (
        IdUsuarioHorario    BIGINT IDENTITY(1,1) NOT NULL,
        Id_Responsable      VARCHAR(20) NOT NULL,
        IdHorarioLaboral    INT NOT NULL,
        FechaDesde          DATE NOT NULL
            CONSTRAINT DF_R_UsuarioHorarioLaboral_FechaDesde
            DEFAULT ('19000101'),
        FechaHasta          DATE NULL,
        Activo              BIT NOT NULL
            CONSTRAINT DF_R_UsuarioHorarioLaboral_Activo DEFAULT (1),
        UsuarioRegistro     VARCHAR(100) NULL,
        FechaRegistro       DATETIME2(0) NOT NULL
            CONSTRAINT DF_R_UsuarioHorarioLaboral_FechaRegistro
            DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_R_UsuarioHorarioLaboral
            PRIMARY KEY (IdUsuarioHorario),

        CONSTRAINT FK_R_UsuarioHorarioLaboral_Horario
            FOREIGN KEY (IdHorarioLaboral)
            REFERENCES dbo.R_HorarioLaboral (IdHorarioLaboral),

        CONSTRAINT CK_R_UsuarioHorarioLaboral_Fechas
            CHECK
            (
                FechaHasta IS NULL
                OR FechaDesde <= FechaHasta
            )
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_R_UsuarioHorarioLaboral_Responsable'
      AND object_id = OBJECT_ID(N'dbo.R_UsuarioHorarioLaboral')
)
BEGIN
    CREATE INDEX IX_R_UsuarioHorarioLaboral_Responsable
    ON dbo.R_UsuarioHorarioLaboral
    (
        Id_Responsable,
        Activo,
        FechaDesde,
        FechaHasta
    );
END;
GO
```

### Observación

La tabla permite conservar el historial de horarios.

Ejemplo: si un empleado cambia de horario desde el `01/07/2026`, cierre su registro anterior y agregue uno nuevo.

```sql
UPDATE dbo.R_UsuarioHorarioLaboral
SET
    FechaHasta = '2026-06-30'
WHERE Id_Responsable = 'CODIGO_USUARIO'
  AND Activo = 1
  AND FechaHasta IS NULL;

INSERT INTO dbo.R_UsuarioHorarioLaboral
(
    Id_Responsable,
    IdHorarioLaboral,
    FechaDesde,
    UsuarioRegistro
)
SELECT
    'CODIGO_USUARIO',
    Horario.IdHorarioLaboral,
    '2026-07-01',
    'admin'
FROM dbo.R_HorarioLaboral AS Horario
WHERE Horario.Codigo = 'H0800_1700';
```

---

## 9. Carga inicial desde el Excel

### 9.1 Crear tabla temporal de carga

```sql
USE [ReporTarea];
GO

IF OBJECT_ID(N'dbo.R_UsuarioHorarioLaboralCargaExcel', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.R_UsuarioHorarioLaboralCargaExcel
    (
        ApellidosNombres    VARCHAR(250) NULL,
        Cedula              VARCHAR(20) NOT NULL,
        Empresa             VARCHAR(100) NULL,
        Cliente             VARCHAR(150) NULL,
        HorarioTexto        VARCHAR(30) NOT NULL
    );
END;
GO
```

### 9.2 Importar el archivo

Utilice el asistente de importación de SQL Server o una carga controlada desde C# para insertar el contenido del Excel en:

```text
dbo.R_UsuarioHorarioLaboralCargaExcel
```

### 9.3 Confirmar las columnas de `R_Usuarios`

Antes de generar el `MERGE` definitivo, ejecute:

```sql
EXEC sys.sp_help 'dbo.R_Usuarios';
GO

SELECT TOP (20) *
FROM dbo.R_Usuarios;
GO
```

Se necesita identificar cuál columna de `R_Usuarios` contiene:

```text
Id_Responsable
Cédula
Ciudad o localidad, si existe
Estado del usuario
```

### 9.4 Plantilla de asignación

Adapte únicamente los campos marcados con `AJUSTAR`.

```sql
/* ============================================================
   AJUSTAR:
   U.Id_Responsable
   U.Cedula
   ============================================================ */

INSERT INTO dbo.R_UsuarioHorarioLaboral
(
    Id_Responsable,
    IdHorarioLaboral,
    FechaDesde,
    UsuarioRegistro
)
SELECT
    U.Id_Responsable, /* AJUSTAR */
    Horario.IdHorarioLaboral,
    '19000101',
    'CARGA_EXCEL'
FROM dbo.R_Usuarios AS U
INNER JOIN dbo.R_UsuarioHorarioLaboralCargaExcel AS Excel
    ON Excel.Cedula = U.Cedula /* AJUSTAR */
INNER JOIN dbo.R_HorarioLaboral AS Horario
    ON Horario.Codigo =
        CASE LTRIM(RTRIM(Excel.HorarioTexto))
            WHEN '8:30 a 17:30' THEN 'H0830_1730'
            WHEN '8:00 a 17:00' THEN 'H0800_1700'
            WHEN '8:30 a 12:30' THEN 'H0830_1230'
            WHEN '8:00 a 16:30' THEN 'H0800_1630'
        END
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.R_UsuarioHorarioLaboral AS Asignacion
    WHERE Asignacion.Id_Responsable = U.Id_Responsable /* AJUSTAR */
      AND Asignacion.Activo = 1
      AND Asignacion.FechaHasta IS NULL
);
GO
```

Los usuarios de `R_Usuarios` que no estén asignados utilizarán automáticamente el perfil predeterminado:

```text
H0830_1730
```

---

## 10. Script SQL: tabla de feriados

```sql
USE [ReporTarea];
GO

IF OBJECT_ID(N'dbo.Feriado', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Feriado
    (
        IdFeriado       INT IDENTITY(1,1) NOT NULL,
        Fecha           DATE NOT NULL,
        Descripcion     NVARCHAR(200) NOT NULL,
        TipoFeriado     VARCHAR(15) NOT NULL
            CONSTRAINT DF_Feriado_Tipo DEFAULT ('NACIONAL'),
        IdCiudad        INT NULL,
        Activo          BIT NOT NULL
            CONSTRAINT DF_Feriado_Activo DEFAULT (1),
        UsuarioRegistro NVARCHAR(100) NULL,
        FechaRegistro   DATETIME2(0) NOT NULL
            CONSTRAINT DF_Feriado_FechaRegistro
            DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_Feriado
            PRIMARY KEY (IdFeriado),

        CONSTRAINT UQ_Feriado
            UNIQUE (Fecha, TipoFeriado, IdCiudad),

        CONSTRAINT CK_Feriado_Tipo
            CHECK (TipoFeriado IN ('NACIONAL', 'LOCAL'))
    );
END;
GO
```

### Feriados nacionales efectivos de Ecuador para 2026

```sql
USE [ReporTarea];
GO

MERGE dbo.Feriado AS Destino
USING
(
    SELECT CAST('2026-01-01' AS DATE) AS Fecha, N'Año Nuevo' AS Descripcion
    UNION ALL SELECT '2026-02-16', N'Carnaval - lunes'
    UNION ALL SELECT '2026-02-17', N'Carnaval - martes'
    UNION ALL SELECT '2026-04-03', N'Viernes Santo'
    UNION ALL SELECT '2026-05-01', N'Día del Trabajo'
    UNION ALL SELECT '2026-05-25', N'Batalla del Pichincha - descanso trasladado'
    UNION ALL SELECT '2026-08-10', N'Primer Grito de la Independencia'
    UNION ALL SELECT '2026-10-09', N'Independencia de Guayaquil'
    UNION ALL SELECT '2026-11-02', N'Día de los Difuntos'
    UNION ALL SELECT '2026-11-03', N'Independencia de Cuenca'
    UNION ALL SELECT '2026-12-25', N'Navidad'
) AS Origen
ON Destino.Fecha = Origen.Fecha
AND Destino.TipoFeriado = 'NACIONAL'
AND Destino.IdCiudad IS NULL

WHEN MATCHED THEN
    UPDATE SET
        Destino.Descripcion = Origen.Descripcion,
        Destino.Activo = 1

WHEN NOT MATCHED THEN
    INSERT
    (
        Fecha,
        Descripcion,
        TipoFeriado,
        IdCiudad,
        Activo,
        UsuarioRegistro
    )
    VALUES
    (
        Origen.Fecha,
        Origen.Descripcion,
        'NACIONAL',
        NULL,
        1,
        'SCRIPT_INICIAL'
    );
GO
```

### Feriados locales

La tabla soporta feriados locales:

```sql
INSERT INTO dbo.Feriado
(
    Fecha,
    Descripcion,
    TipoFeriado,
    IdCiudad,
    Activo,
    UsuarioRegistro
)
VALUES
(
    '2026-12-06',
    N'Fundación de Quito',
    'LOCAL',
    1, /* AJUSTAR IdCiudad */
    1,
    'admin'
);
```

Para aplicar feriados locales automáticamente, se debe identificar la columna de ciudad o localidad de `R_Usuarios`.

---

## 11. Función SQL: clasificar el horario por empleado

La función recibe:

```text
Id_Responsable
FechaHoraInicio
FechaHoraFin
```

y devuelve uno o varios tramos.

```sql
USE [ReporTarea];
GO

IF OBJECT_ID(N'dbo.FN_RTA_ClasificarTramosHorario') IS NOT NULL
BEGIN
    DROP FUNCTION dbo.FN_RTA_ClasificarTramosHorario;
END;
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
        CONVERT
        (
            TINYINT,
            (DATEDIFF(DAY, '19000101', @FechaTrabajo) % 7) + 1
        );

    /* --------------------------------------------------------
       1. Buscar horario específico del empleado
       -------------------------------------------------------- */

    SELECT TOP (1)
        @IdHorarioLaboral = Asignacion.IdHorarioLaboral
    FROM dbo.R_UsuarioHorarioLaboral AS Asignacion
    INNER JOIN dbo.R_HorarioLaboral AS Horario
        ON Horario.IdHorarioLaboral = Asignacion.IdHorarioLaboral
    WHERE Asignacion.Id_Responsable = @Id_Responsable
      AND Asignacion.Activo = 1
      AND Horario.Activo = 1
      AND Asignacion.FechaDesde <= @FechaTrabajo
      AND
      (
          Asignacion.FechaHasta IS NULL
          OR Asignacion.FechaHasta >= @FechaTrabajo
      )
    ORDER BY Asignacion.FechaDesde DESC;

    /* --------------------------------------------------------
       2. Si no existe asignación, usar horario predeterminado
       -------------------------------------------------------- */

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

    /* --------------------------------------------------------
       3. Obtener configuración del día
       -------------------------------------------------------- */

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

    /* --------------------------------------------------------
       4. Validar feriado nacional
       -------------------------------------------------------- */

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Feriado AS Feriado
        WHERE Feriado.Fecha = @FechaTrabajo
          AND Feriado.Activo = 1
          AND Feriado.TipoFeriado = 'NACIONAL'
    )
    BEGIN
        SET @EsFeriado = 1;
    END;

    /* --------------------------------------------------------
       5. Feriado o día no laborable: 100%
       -------------------------------------------------------- */

    IF @EsFeriado = 1 OR @EsLaborable = 0
    BEGIN
        INSERT INTO @Tramos
        (
            Orden,
            TramoInicio,
            TramoFin,
            TipoHora,
            Descripcion
        )
        VALUES
        (
            1,
            @FechaInicio,
            @FechaFin,
            2,
            'HORAS EXTRAORDINARIAS 100%'
        );

        RETURN;
    END;

    SET @InicioDia = CONVERT(DATETIME, @FechaTrabajo);
    SET @LimiteSeis = DATEADD(HOUR, 6, @InicioDia);

    SET @InicioOficina =
        DATEADD
        (
            SECOND,
            DATEDIFF
            (
                SECOND,
                CAST('00:00:00' AS TIME(0)),
                @HoraInicioOficina
            ),
            @InicioDia
        );

    SET @FinOficina =
        DATEADD
        (
            SECOND,
            DATEDIFF
            (
                SECOND,
                CAST('00:00:00' AS TIME(0)),
                @HoraFinOficina
            ),
            @InicioDia
        );

    DECLARE @Puntos TABLE
    (
        Punto DATETIME NOT NULL PRIMARY KEY
    );

    INSERT INTO @Puntos (Punto)
    VALUES
        (@FechaInicio),
        (@FechaFin);

    IF @LimiteSeis > @FechaInicio
       AND @LimiteSeis < @FechaFin
       AND NOT EXISTS
       (
           SELECT 1
           FROM @Puntos
           WHERE Punto = @LimiteSeis
       )
    BEGIN
        INSERT INTO @Puntos (Punto)
        VALUES (@LimiteSeis);
    END;

    IF @InicioOficina > @FechaInicio
       AND @InicioOficina < @FechaFin
       AND NOT EXISTS
       (
           SELECT 1
           FROM @Puntos
           WHERE Punto = @InicioOficina
       )
    BEGIN
        INSERT INTO @Puntos (Punto)
        VALUES (@InicioOficina);
    END;

    IF @FinOficina > @FechaInicio
       AND @FinOficina < @FechaFin
       AND NOT EXISTS
       (
           SELECT 1
           FROM @Puntos
           WHERE Punto = @FinOficina
       )
    BEGIN
        INSERT INTO @Puntos (Punto)
        VALUES (@FinOficina);
    END;

    ;WITH PuntosOrdenados AS
    (
        SELECT
            Punto,
            ROW_NUMBER() OVER (ORDER BY Punto) AS Orden
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
                WHEN Inicio.Punto >= @InicioOficina
                     AND Fin.Punto <= @FinOficina
                    THEN 0
                ELSE 1
            END AS TipoHora
        FROM PuntosOrdenados AS Inicio
        INNER JOIN PuntosOrdenados AS Fin
            ON Fin.Orden = Inicio.Orden + 1
    )
    INSERT INTO @Tramos
    (
        Orden,
        TramoInicio,
        TramoFin,
        TipoHora,
        Descripcion
    )
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
```

---

## 12. Cambio requerido en `Sp_RTAInsertaDetalleTarea_V2`

El procedimiento debe utilizar la nueva firma de la función.

Busque:

```sql
FROM dbo.FN_RTA_ClasificarTramosHorario
(
    @FechaInicio,
    @FechaFin
);
```

Reemplace por:

```sql
FROM dbo.FN_RTA_ClasificarTramosHorario
(
    @Id_Responsable,
    @FechaInicio,
    @FechaFin
);
```

La tabla temporal usada en el SP debe tener esta estructura:

```sql
DECLARE @Tramos TABLE
(
    Orden        INT NOT NULL,
    TramoInicio  DATETIME NOT NULL,
    TramoFin     DATETIME NOT NULL,
    TipoHora     BIGINT NOT NULL,
    Descripcion  VARCHAR(50) NOT NULL
);
```

Para construir el mensaje, mantenga este bloque:

```sql
SELECT
    @DetalleTramos =
        STUFF
        (
            (
                SELECT
                    N'; '
                    + CONVERT(VARCHAR(5), Tramo.TramoInicio, 108)
                    + N' a '
                    + CONVERT(VARCHAR(5), Tramo.TramoFin, 108)
                    + N' = '
                    + Tramo.Descripcion
                FROM @Tramos AS Tramo
                ORDER BY Tramo.Orden
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'),
            1,
            2,
            N''
        );
```

Cuando existan diferentes tipos de horas:

```sql
IF @CantidadTipos > 1
BEGIN
    SELECT
        -7 AS Respuestas,
        N'El horario enviado contiene diferentes tipos de horas. '
        + N'Registre los siguientes tramos por separado: '
        + @DetalleTramos
        + N'.'
            AS Mensaje;

    RETURN;
END;
```

Para guardar los campos:

```sql
SET @Det_Horas_Extras_Tipo = @TipoHoraCalculado;
SET @Det_Horas_Extras_Estado = @TipoHoraCalculado;

SET @Det_Horas_Extras_Descripcion =
    CASE @TipoHoraCalculado
        WHEN 1 THEN '50%'
        WHEN 2 THEN '100%'
        ELSE ''
    END;
```

En el `INSERT INTO dbo.R_DetTareasAranda`, confirme que se incluya:

```sql
Det_Horas_Extras_Tipo,
Det_Horas_Extras_Estado,
Det_Horas_Extras_Descripcion
```

---

## 13. Cambio requerido en C#

El método C# debe utilizar primero el campo `Mensaje` devuelto por el SP.

```csharp
string mensajeSP = ObtenerValorColumna(dr, "Mensaje");

respuesta.mensaje =
    !string.IsNullOrWhiteSpace(mensajeSP)
        ? mensajeSP
        : ObtenerMensajePredeterminado(respuestaSP);
```

Método auxiliar:

```csharp
private static string ObtenerValorColumna(
    SqlDataReader dr,
    string nombreColumna)
{
    for (int i = 0; i < dr.FieldCount; i++)
    {
        if (string.Equals(
            dr.GetName(i),
            nombreColumna,
            StringComparison.OrdinalIgnoreCase))
        {
            return dr.IsDBNull(i)
                ? string.Empty
                : dr.GetValue(i).ToString();
        }
    }

    return string.Empty;
}
```

Mensaje predeterminado para tramos mixtos:

```csharp
case -7:
    return "El horario contiene diferentes tipos de horas. Debe registrar los tramos por separado.";
```

---

## 14. Pruebas mínimas

### 14.1 Usuario sin asignación específica

Debe utilizar el horario predeterminado `08:30 a 17:30`.

```sql
SELECT *
FROM dbo.FN_RTA_ClasificarTramosHorario
(
    'USUARIO_SIN_ASIGNACION',
    '2026-06-08 07:00:00',
    '2026-06-08 18:30:00'
);
```

Resultado esperado:

| Inicio | Fin | Tipo | Descripción |
|---|---|---:|---|
| `07:00` | `08:30` | `1` | `HORAS SUPLEMENTARIAS 50%` |
| `08:30` | `17:30` | `0` | `HORAS NORMALES` |
| `17:30` | `18:30` | `1` | `HORAS SUPLEMENTARIAS 50%` |

### 14.2 Usuario con horario `08:00 a 17:00`

Resultado esperado:

| Inicio | Fin | Tipo | Descripción |
|---|---|---:|---|
| `07:00` | `08:00` | `1` | `HORAS SUPLEMENTARIAS 50%` |
| `08:00` | `17:00` | `0` | `HORAS NORMALES` |
| `17:00` | `18:30` | `1` | `HORAS SUPLEMENTARIAS 50%` |

### 14.3 Horario entre `00:00` y `06:00`

```sql
SELECT *
FROM dbo.FN_RTA_ClasificarTramosHorario
(
    'USUARIO_PRUEBA',
    '2026-06-08 02:00:00',
    '2026-06-08 05:00:00'
);
```

Resultado esperado:

```text
HORAS EXTRAORDINARIAS 100%
```

### 14.4 Feriado nacional

```sql
SELECT *
FROM dbo.FN_RTA_ClasificarTramosHorario
(
    'USUARIO_PRUEBA',
    '2026-05-25 08:30:00',
    '2026-05-25 17:30:00'
);
```

Resultado esperado:

```text
HORAS EXTRAORDINARIAS 100%
```

### 14.5 Domingo

```sql
SELECT *
FROM dbo.FN_RTA_ClasificarTramosHorario
(
    'USUARIO_PRUEBA',
    '2026-06-07 08:30:00',
    '2026-06-07 17:30:00'
);
```

Resultado esperado:

```text
HORAS EXTRAORDINARIAS 100%
```

---

## 15. Orden recomendado de despliegue

Ejecute los cambios en este orden:

1. Respaldar la base de datos `ReporTarea`.
2. Agregar `Det_Horas_Extras_Estado` en `R_DetTareasAranda`.
3. Crear `R_HorarioLaboral`.
4. Crear `R_HorarioLaboralDetalle`.
5. Insertar los cuatro perfiles de horario.
6. Crear `R_UsuarioHorarioLaboral`.
7. Crear la tabla de carga del Excel.
8. Revisar la estructura de `R_Usuarios`.
9. Importar el Excel y asignar los horarios.
10. Crear o actualizar `Feriado`.
11. Cargar los feriados nacionales de 2026.
12. Reemplazar `FN_RTA_ClasificarTramosHorario`.
13. Ajustar `Sp_RTAInsertaDetalleTarea_V2`.
14. Actualizar el método C# para mostrar el mensaje del SP.
15. Ejecutar pruebas funcionales.
16. Publicar primero en ambiente de pruebas.

---

## 16. Pendientes antes de generar un script final de carga automática

Se requieren estos datos de `dbo.R_Usuarios`:

```text
Nombre real de la columna Id_Responsable
Nombre real de la columna Cédula
Nombre real de la columna Ciudad o localidad, si existe
Nombre real de la columna Estado o activo
```

Obtenga esta información mediante:

```sql
EXEC sys.sp_help 'dbo.R_Usuarios';
GO

SELECT TOP (20) *
FROM dbo.R_Usuarios;
GO
```

Con esa estructura se podrá generar un script definitivo para relacionar automáticamente los `33` empleados del Excel y aplicar el perfil predeterminado a los usuarios restantes.

---

## 17. Fuentes oficiales consultadas

### Código del Trabajo de Ecuador

Artículo 55: horas suplementarias y extraordinarias.

```text
https://www.trabajo.gob.ec/wp-content/uploads/downloads/2024/01/CODIGO_DEL_TRABAJO.pdf
```

### Calendario oficial de feriados nacionales 2025-2030

Ministerio de Turismo del Ecuador.

```text
https://www.turismo.gob.ec/wp-content/uploads/2025/08/Calendario-feriados-nacionales-2025-2030.pdf
```
