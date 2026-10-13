# Calificación de oportunidades del Forecast — Plan de implementación

> **Para quien ejecute esto:** SUB-SKILL REQUERIDA: usar `superpowers:subagent-driven-development` (recomendado) o `superpowers:executing-plans` para ejecutar tarea por tarea. Los pasos usan casillas (`- [ ]`) para llevar la cuenta.

**Objetivo:** Una pantalla nueva, `CalificacionForeCast.aspx`, que califica de 0 a 100 cada oportunidad abierta del Forecast. Muestra su estado (Caliente / Tibia / Fría), sus alertas, los datos faltantes y la acción sugerida, y permite que el vendedor responda Presupuesto, Autoridad y Necesidad en un modal.

**Arquitectura:**
- **SQL solo trae datos.** Un SP nuevo reutiliza `Sp_RTAListaForeCast` con `INSERT … EXEC` para respetar la visibilidad por usuario, y las respuestas del vendedor se guardan en una tabla nueva.
- **Las reglas viven en `CapaNegocio`**, como funciones puras probadas con MSTest. Reciben la fecha de hoy como parámetro para no depender del reloj.
- **La pantalla solo dibuja** lo que devuelve el handler.

**Desviación menor respecto al spec:** el spec le daba al SP de lista los parámetros de filtro (gerente, sucursal, marca, etapa). Aquí el SP recibe **solo `@Idusuario`**, que es lo que decide la visibilidad, y los filtros se aplican en `NegCalificacionForeCast.Armar`. Así los combos salen siempre del conjunto completo y los indicadores se calculan en un único lugar. Para 134 filas no hay costo de rendimiento.

**Tecnologías:** SQL Server (T-SQL), C# .NET Framework 4.6.1 (C# 7.3), MSTest, EPPlus 6.2.7, jQuery 1.9.1, Bootstrap 3, DataTables 1.10.15, SweetAlert 1.

**Spec:** `docs/superpowers/specs/2026-09-28-calificacion-forecast-design.md`

## Restricciones globales

- **Compilar solo con** `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`, desde PowerShell. El MSBuild del PATH falla con errores que despistan.
- **Correr las pruebas con:** `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll`
- **C# 7.3:** los proyectos no fijan `LangVersion`. Nada de `switch` de expresiones, `??=`, `using` sin llaves ni tipos de referencia anulables.
- **Las tres capas se publican como `.exe`**, no `.dll`: `CapaEntidad.exe`, `CapaNegocio.exe`, `CapaDato.exe`.
- **`CapaDato` no puede llamar a `CapaNegocio`.** El proyecto web referencia `CapaEntidad` y `CapaNegocio`, nunca `CapaDato`.
- **Codificación de archivos:** los `.cs`, `.js` y `.aspx` nuevos van en **UTF-8 con BOM**, como los existentes. Los `.sql` versionados van **sin BOM**.
- **Cada archivo nuevo se registra en su `.csproj`.** Son proyectos de estilo antiguo: un archivo que no está en el `.csproj` no se compila ni se publica.
- **Correr un script SQL con** `bash docs/sql/run-sql.sh <archivo.sql>`, que apunta a la base de la aplicación (`ReporTarea`, **producción**). **Pedir confirmación explícita al usuario antes de correrlo.**
- **El repositorio es público:** ningún nombre de cliente, contacto, persona ni monto real en código, pruebas, scripts ni documentos.
- **`ForeCast` no se modifica.** Ni esquema ni datos.
- **Valores verificados en la base el 2026-09-28** (no cambiarlos sin volver a verificar):
  - `PrmPorcentaje.Porcentaje` es `varchar(50)`, y el `IdNegocio` = 10 es «Perdida o Descartada».
  - `Sp_RTAListaForeCast` devuelve **un solo** resultado de **40 columnas**, la primera `IdForeCast`.
  - La opción de menú de Forecast cuelga de `Id_MenuPadre = 39`.

## Review Focus

1. **Calificar una oportunidad que el usuario no ve.** El `IdForeCast` llega del navegador. Quien lo cambie a mano no debe poder calificar fuera de su visibilidad. → Prueba en la Task 4 (`Guardar` rechaza un id que no está entre los visibles).
2. **Cultura del servidor en es-EC/es-ES.** Las fechas en texto y los montos del JSON no deben salir con coma decimal ni con nombres de mes en otro idioma. → Pruebas en las Tasks 3 y 4 con la cultura fijada en es-ES.
3. **Segmentos con tildes o espacios distintos.** «Educación» aparece escrito de dos maneras en la base, y el `TipoEmpresa` puede traer espacios. El cruce con las tasas de éxito no debe fallar por un espacio o una mayúscula. → Prueba en la Task 2 (`PuntosEncaje` con espacios y mayúsculas).
4. **Oportunidad sin fecha de cierre, sin monto o sin porcentaje.** No debe haber excepción. Debe sumar 0 donde corresponde y listarse en datos faltantes. → Pruebas en las Tasks 2 y 3.
5. **Guardar dos veces seguidas.** La calificación vigente es la última. Con dos filas del mismo segundo, desempata `IdCalificacion`. → Cubierto en el SP de la Task 1 (`ORDER BY FechaRegistro DESC, IdCalificacion DESC`), y la verificación del Paso 4 de esa tarea inserta dos filas dentro de una transacción revertida.

---

### Task 1: Base de datos — tabla, procedimientos y menú

**Archivos:**
- Crear: `docs/sql/2026-09-28-calificacion-forecast.sql`
- Referencia (no se modifica): `Script_Menu_ParametrizacionHorario.sql`

**Interfaces:**
- Produce:
  - Tabla `dbo.ForeCastCalificacion`.
  - `dbo.Sp_RTAListaCalificacionForeCast @Idusuario VARCHAR(10)`, con **dos** resultados:
    - (1) Una fila por oportunidad abierta y visible, con las columnas `IdForeCast, IdCliente, Cliente, DetalleProyecto, Marca, Prioridad, GerentedeCuenta, Sucursal, SegmentodeMercado, TipoEmpresa, PVPEstimado, UtilidadBrutaEstimada, IdNegocio, Porcentaje, DescripcionNegocio, FechaCierre, FechaModificacion, Contacto, LiderProyectoCliente, Observaciones, FechasCierreDistintas, GanadosCliente, ContactosCuenta, Presupuesto, Autoridad, Necesidad, DecisorCargo, PropuestaEnviada, AccionComprometida, FechaAccion, UsuarioCalificacion, FechaCalificacion`.
    - (2) Las tasas de éxito, con las columnas `Dimension, Valor, Ganados, Perdidos`.
  - `dbo.Sp_RTAInsertaCalificacionForeCast`, que devuelve una fila `Estado INT, Mensaje VARCHAR(200)`.
- Consume: `dbo.Sp_RTAListaForeCast` (no se toca), `dbo.ForeCast`, `dbo.ForeCastHistorico` y `dbo.PrmPorcentaje`.

- [ ] **Paso 1: Escribir el script** (UTF-8 **sin BOM**)

```sql
/* ============================================================================
   Calificacion de oportunidades del Forecast
   ReporTarea  |  2026-09-28

   PENDIENTE DE EJECUTAR.

   Crea:
     1. dbo.ForeCastCalificacion           (respuestas del vendedor, con historial)
     2. dbo.Sp_RTAListaCalificacionForeCast (solo lectura: datos + tasas de exito)
     3. dbo.Sp_RTAInsertaCalificacionForeCast
     4. La opcion de menu, con los mismos perfiles que ven RegistroForeCast.aspx

   La visibilidad NO se reescribe: el SP de lista hace INSERT ... EXEC de
   Sp_RTAListaForeCast y se queda solo con IdForeCast. Esa regla (sucursales
   por usuario escritas en el SP) vive en un unico lugar.

   ATENCION: #Visibles replica las 40 columnas de Sp_RTAListaForeCast. Si
   alguien le agrega una columna a ese SP, este falla con "Column name or
   number of supplied values does not match table definition". Agregar la
   columna aqui tambien.

   ForeCast NO se modifica. Idempotente: la tabla se crea si no existe; los
   SP se reemplazan (DROP y CREATE); el menu no se duplica.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTAListaForeCast','P') IS NULL
   OR OBJECT_ID('dbo.ForeCastHistorico','U') IS NULL
   OR OBJECT_ID('dbo.PrmPorcentaje','U') IS NULL
BEGIN
    RAISERROR('Falta Sp_RTAListaForeCast, ForeCastHistorico o PrmPorcentaje. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

/* 1. Tabla ------------------------------------------------------------------ */
IF OBJECT_ID('dbo.ForeCastCalificacion','U') IS NULL
BEGIN
    CREATE TABLE dbo.ForeCastCalificacion
    (
        IdCalificacion     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ForeCastCalificacion PRIMARY KEY,
        IdForeCast         BIGINT       NOT NULL,
        Presupuesto        TINYINT      NOT NULL,
        Autoridad          TINYINT      NOT NULL,
        Necesidad          TINYINT      NOT NULL,
        DecisorCargo       VARCHAR(150) NULL,
        PropuestaEnviada   BIT          NOT NULL CONSTRAINT DF_ForeCastCalificacion_Propuesta DEFAULT (0),
        AccionComprometida VARCHAR(300) NULL,
        FechaAccion        DATE         NULL,
        Usuario            VARCHAR(50)  NOT NULL,
        FechaRegistro      DATETIME     NOT NULL CONSTRAINT DF_ForeCastCalificacion_Fecha DEFAULT (GETDATE()),
        CONSTRAINT CK_ForeCastCalificacion_Presupuesto CHECK (Presupuesto IN (0,5,12,20)),
        CONSTRAINT CK_ForeCastCalificacion_Autoridad   CHECK (Autoridad   IN (0,5,12,20)),
        CONSTRAINT CK_ForeCastCalificacion_Necesidad   CHECK (Necesidad   IN (0,6,14,20))
    );

    CREATE INDEX IX_ForeCastCalificacion_IdForeCast
        ON dbo.ForeCastCalificacion (IdForeCast, FechaRegistro DESC, IdCalificacion DESC);

    PRINT 'ForeCastCalificacion creada.';
END
ELSE
    PRINT 'ForeCastCalificacion ya existia.';
GO

/* 2. Lista ------------------------------------------------------------------ */
IF OBJECT_ID('dbo.Sp_RTAListaCalificacionForeCast','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTAListaCalificacionForeCast;
GO

CREATE PROCEDURE dbo.Sp_RTAListaCalificacionForeCast
    @Idusuario VARCHAR(10) = ''
AS
BEGIN
    SET NOCOUNT ON;

    /* Las 40 columnas de Sp_RTAListaForeCast. Solo se usa IdForeCast; el
       resto va como NVARCHAR(MAX) para que ninguna conversion de tipo falle. */
    CREATE TABLE #Visibles
    (
        IdForeCast BIGINT,
        c02 NVARCHAR(MAX), c03 NVARCHAR(MAX), c04 NVARCHAR(MAX), c05 NVARCHAR(MAX),
        c06 NVARCHAR(MAX), c07 NVARCHAR(MAX), c08 NVARCHAR(MAX), c09 NVARCHAR(MAX),
        c10 NVARCHAR(MAX), c11 NVARCHAR(MAX), c12 NVARCHAR(MAX), c13 NVARCHAR(MAX),
        c14 NVARCHAR(MAX), c15 NVARCHAR(MAX), c16 NVARCHAR(MAX), c17 NVARCHAR(MAX),
        c18 NVARCHAR(MAX), c19 NVARCHAR(MAX), c20 NVARCHAR(MAX), c21 NVARCHAR(MAX),
        c22 NVARCHAR(MAX), c23 NVARCHAR(MAX), c24 NVARCHAR(MAX), c25 NVARCHAR(MAX),
        c26 NVARCHAR(MAX), c27 NVARCHAR(MAX), c28 NVARCHAR(MAX), c29 NVARCHAR(MAX),
        c30 NVARCHAR(MAX), c31 NVARCHAR(MAX), c32 NVARCHAR(MAX), c33 NVARCHAR(MAX),
        c34 NVARCHAR(MAX), c35 NVARCHAR(MAX), c36 NVARCHAR(MAX), c37 NVARCHAR(MAX),
        c38 NVARCHAR(MAX), c39 NVARCHAR(MAX), c40 NVARCHAR(MAX)
    );

    INSERT INTO #Visibles
    EXEC dbo.Sp_RTAListaForeCast @Idusuario = @Idusuario;

    /* Resultado 1: oportunidades abiertas y visibles */
    SELECT
        f.IdForeCast,
        f.IdCliente,
        LTRIM(RTRIM(f.Cliente))            AS Cliente,
        LTRIM(RTRIM(f.DetalleProyecto))    AS DetalleProyecto,
        f.Marca,
        f.Prioridad,
        LTRIM(RTRIM(f.GerentedeCuenta))    AS GerentedeCuenta,
        LTRIM(RTRIM(f.Sucursal))           AS Sucursal,
        LTRIM(RTRIM(f.SegmentodeMercado))  AS SegmentodeMercado,
        LTRIM(RTRIM(f.TipoEmpresa))        AS TipoEmpresa,
        f.PVPEstimado,
        f.UtilidadBrutaEstimada,
        f.IdNegocio,
        TRY_CONVERT(DECIMAL(5,2), NULLIF(LTRIM(RTRIM(p.Porcentaje)), '')) AS Porcentaje,
        p.Descripcion                      AS DescripcionNegocio,
        f.FechaFacturacionProyecto         AS FechaCierre,
        f.FechaCobro                       AS FechaModificacion,
        f.Contacto,
        f.LiderProyectoCliente,
        f.Observaciones,
        (SELECT COUNT(DISTINCT h.FechaFacturacionProyecto)
           FROM dbo.ForeCastHistorico h
          WHERE h.IdForeCast = f.IdForeCast)                        AS FechasCierreDistintas,
        (SELECT COUNT(*)
           FROM dbo.ForeCast g
          WHERE g.IdCliente = f.IdCliente
            AND g.Estado = 1
            AND g.Prioridad = 'GANADO')                              AS GanadosCliente,
        (SELECT COUNT(DISTINCT x.Nombre)
           FROM (SELECT NULLIF(LTRIM(RTRIM(c.Contacto)), '') AS Nombre
                   FROM dbo.ForeCast c WHERE c.IdCliente = f.IdCliente
                 UNION ALL
                 SELECT NULLIF(LTRIM(RTRIM(c.LiderProyectoCliente)), '')
                   FROM dbo.ForeCast c WHERE c.IdCliente = f.IdCliente) x) AS ContactosCuenta,
        k.Presupuesto,
        k.Autoridad,
        k.Necesidad,
        k.DecisorCargo,
        k.PropuestaEnviada,
        k.AccionComprometida,
        k.FechaAccion,
        k.Usuario                          AS UsuarioCalificacion,
        k.FechaRegistro                    AS FechaCalificacion
    FROM dbo.ForeCast f
    INNER JOIN (SELECT DISTINCT IdForeCast FROM #Visibles) v ON v.IdForeCast = f.IdForeCast
    LEFT JOIN dbo.PrmPorcentaje p ON p.IdNegocio = f.IdNegocio
    OUTER APPLY (SELECT TOP 1 q.*
                   FROM dbo.ForeCastCalificacion q
                  WHERE q.IdForeCast = f.IdForeCast
                  ORDER BY q.FechaRegistro DESC, q.IdCalificacion DESC) k
    WHERE f.Prioridad IN ('LEAD','PIPELINE','UPSIDE','FORECAST');

    /* Resultado 2: tasas de exito de los ultimos 36 meses (FechaCobro es la
       ultima modificacion; en un cierre coincide con la fecha del cierre). */
    SELECT 'SEGMENTO' AS Dimension,
           LTRIM(RTRIM(SegmentodeMercado)) AS Valor,
           SUM(CASE WHEN Prioridad = 'GANADO'  THEN 1 ELSE 0 END) AS Ganados,
           SUM(CASE WHEN Prioridad = 'PERDIDO' THEN 1 ELSE 0 END) AS Perdidos
      FROM dbo.ForeCast
     WHERE Estado = 1
       AND Prioridad IN ('GANADO','PERDIDO')
       AND FechaCobro >= DATEADD(MONTH, -36, GETDATE())
       AND ISNULL(LTRIM(RTRIM(SegmentodeMercado)), '') <> ''
     GROUP BY LTRIM(RTRIM(SegmentodeMercado))
    UNION ALL
    SELECT 'TIPO',
           LTRIM(RTRIM(TipoEmpresa)),
           SUM(CASE WHEN Prioridad = 'GANADO'  THEN 1 ELSE 0 END),
           SUM(CASE WHEN Prioridad = 'PERDIDO' THEN 1 ELSE 0 END)
      FROM dbo.ForeCast
     WHERE Estado = 1
       AND Prioridad IN ('GANADO','PERDIDO')
       AND FechaCobro >= DATEADD(MONTH, -36, GETDATE())
       AND ISNULL(LTRIM(RTRIM(TipoEmpresa)), '') <> ''
     GROUP BY LTRIM(RTRIM(TipoEmpresa));
END
GO

/* 3. Insertar ---------------------------------------------------------------- */
IF OBJECT_ID('dbo.Sp_RTAInsertaCalificacionForeCast','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTAInsertaCalificacionForeCast;
GO

CREATE PROCEDURE dbo.Sp_RTAInsertaCalificacionForeCast
    @IdForeCast         BIGINT,
    @Presupuesto        TINYINT,
    @Autoridad          TINYINT,
    @Necesidad          TINYINT,
    @DecisorCargo       VARCHAR(150) = NULL,
    @PropuestaEnviada   BIT = 0,
    @AccionComprometida VARCHAR(300) = NULL,
    @FechaAccion        DATE = NULL,
    @Usuario            VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.ForeCast
                    WHERE IdForeCast = @IdForeCast
                      AND Estado = 1
                      AND Prioridad IN ('LEAD','PIPELINE','UPSIDE','FORECAST'))
    BEGIN
        SELECT 0 AS Estado, CONVERT(VARCHAR(200), 'La oportunidad no existe o ya no esta abierta.') AS Mensaje;
        RETURN;
    END

    IF @Presupuesto NOT IN (0,5,12,20) OR @Autoridad NOT IN (0,5,12,20) OR @Necesidad NOT IN (0,6,14,20)
    BEGIN
        SELECT 0 AS Estado, CONVERT(VARCHAR(200), 'Puntaje no permitido.') AS Mensaje;
        RETURN;
    END

    INSERT INTO dbo.ForeCastCalificacion
        (IdForeCast, Presupuesto, Autoridad, Necesidad, DecisorCargo, PropuestaEnviada,
         AccionComprometida, FechaAccion, Usuario)
    VALUES
        (@IdForeCast, @Presupuesto, @Autoridad, @Necesidad, NULLIF(LTRIM(RTRIM(@DecisorCargo)), ''),
         ISNULL(@PropuestaEnviada, 0), NULLIF(LTRIM(RTRIM(@AccionComprometida)), ''), @FechaAccion, @Usuario);

    SELECT 1 AS Estado, CONVERT(VARCHAR(200), 'Calificacion guardada.') AS Mensaje;
END
GO

/* 4. Menu -------------------------------------------------------------------- */
BEGIN TRANSACTION;

DECLARE @IdMenuPadre BIGINT       = 39;   -- grupo "Forecacst" (asi esta escrito en MenuDos)
DECLARE @Href        VARCHAR(512) = 'CalificacionForeCast.aspx';
DECLARE @Titulo      VARCHAR(128) = 'Calificación ForeCast';
DECLARE @Descripcion VARCHAR(512) = 'Puntaje, alertas y siguiente accion de las oportunidades abiertas';
DECLARE @Icono       VARCHAR(128) = 'fa fa-star-half-o';
DECLARE @IdMenu      BIGINT;
DECLARE @Orden       INT;

SELECT @IdMenu = Id_Menu FROM dbo.MenuDos WHERE Href = @Href AND Estado_Logico_Registro = 1;

IF @IdMenu IS NULL
BEGIN
    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1 FROM dbo.MenuDos WHERE Id_MenuPadre = @IdMenuPadre;

    INSERT INTO dbo.MenuDos
        (Id_MenuPadre, Es_Opcion_de_Menu, Href, Class_Opcion, Class_Icon, Titulo, Descripcion,
         Es_Opcion_Publica, Orden_Opcion, Estado_Registro, Fecha_Creacion, Id_Usuario_Creacion,
         Ip_Creacion, Estado_Logico_Registro)
    VALUES
        (@IdMenuPadre, 0, @Href, NULL, @Icono, @Titulo, @Descripcion,
         0, @Orden, 1, GETDATE(), 'admin', '127.0.0.1', 1);

    SET @IdMenu = CONVERT(BIGINT, SCOPE_IDENTITY());
    PRINT 'MenuDos: opcion creada con Id_Menu = ' + CONVERT(VARCHAR(20), @IdMenu);
END
ELSE
    PRINT 'MenuDos: la opcion ya existia (Id_Menu = ' + CONVERT(VARCHAR(20), @IdMenu) + ').';

/* Mismos perfiles que VEN RegistroForeCast.aspx. En PerfilMenu, Estado = '0'
   MUESTRA la opcion y Estado = '1' la oculta. */
INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT DISTINCT CONVERT(INT, @IdMenu), pm.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM dbo.PerfilMenu pm
  JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
 WHERE m.Href = 'RegistroForeCast.aspx'
   AND m.Estado_Logico_Registro = 1
   AND pm.Estado = '0'
   AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu x
                    WHERE x.id_Menu = CONVERT(INT, @IdMenu) AND x.IdPerfil = pm.IdPerfil);

COMMIT TRANSACTION;
GO

SET NOEXEC OFF;
GO

/* Verificacion */
SELECT M.Id_Menu, M.Id_MenuPadre, M.Titulo, M.Href, PM.IdPerfil, PM.Estado
  FROM dbo.MenuDos M
  LEFT JOIN dbo.PerfilMenu PM ON PM.id_Menu = M.Id_Menu
 WHERE M.Href = 'CalificacionForeCast.aspx'
 ORDER BY PM.IdPerfil;
GO
```

- [ ] **Paso 2: Pedir confirmación al usuario** antes de correr el script contra `ReporTarea`, que es producción. Mostrarle qué crea: una tabla, dos SP y una opción de menú. No toca `ForeCast`.

- [ ] **Paso 3: Correr el script**

Run: `bash docs/sql/run-sql.sh docs/sql/2026-09-28-calificacion-forecast.sql`
Expected: `ForeCastCalificacion creada.`, `MenuDos: opcion creada …`, y una verificación con **una fila por perfil** (hoy 8, 18 y 19), todas con `Estado = 0`.

- [ ] **Paso 4: Verificar los SP sin dejar datos**

Crear `docs/sql/2026-09-28-calificacion-forecast-verificacion.sql` (sin BOM):

```sql
/* Verificacion de 2026-09-28-calificacion-forecast.sql. No deja datos: todo
   va dentro de una transaccion que se revierte. */
USE [ReporTarea];
GO
SET NOCOUNT ON;

/* 1. La lista devuelve dos resultados y solo abiertas */
EXEC dbo.Sp_RTAListaCalificacionForeCast @Idusuario = '';

/* 2. La vigente es la ultima, aun en el mismo segundo */
BEGIN TRANSACTION;
DECLARE @Id BIGINT = (SELECT TOP 1 IdForeCast FROM dbo.ForeCast
                       WHERE Estado = 1 AND Prioridad = 'PIPELINE' ORDER BY IdForeCast DESC);
EXEC dbo.Sp_RTAInsertaCalificacionForeCast @IdForeCast = @Id, @Presupuesto = 5,  @Autoridad = 5,  @Necesidad = 6,  @Usuario = 'verificacion';
EXEC dbo.Sp_RTAInsertaCalificacionForeCast @IdForeCast = @Id, @Presupuesto = 20, @Autoridad = 12, @Necesidad = 14, @Usuario = 'verificacion';
EXEC dbo.Sp_RTAInsertaCalificacionForeCast @IdForeCast = @Id, @Presupuesto = 7,  @Autoridad = 5,  @Necesidad = 6,  @Usuario = 'verificacion';  -- debe rechazar
SELECT TOP 1 Presupuesto, Autoridad, Necesidad FROM dbo.ForeCastCalificacion
 WHERE IdForeCast = @Id ORDER BY FechaRegistro DESC, IdCalificacion DESC;   -- debe ser 20, 12, 14
ROLLBACK TRANSACTION;
SELECT COUNT(*) AS FilasQueQuedaron FROM dbo.ForeCastCalificacion WHERE Usuario = 'verificacion';  -- debe ser 0
GO
```

Run: `bash docs/sql/run-sql.sh docs/sql/2026-09-28-calificacion-forecast-verificacion.sql`
Expected:
- El primer resultado tiene alrededor de 134 filas, todas con `Prioridad` LEAD, PIPELINE, UPSIDE o FORECAST.
- El segundo resultado tiene filas `SEGMENTO` y `TIPO`.
- Los dos primeros inserts devuelven `Estado 1` y el tercero `Estado 0, Puntaje no permitido.`.
- La consulta del medio devuelve `20 | 12 | 14`.
- `FilasQueQuedaron = 0`.

- [ ] **Paso 5: Commit**

```bash
git add docs/sql/2026-09-28-calificacion-forecast.sql docs/sql/2026-09-28-calificacion-forecast-verificacion.sql
git commit -m "feat(forecast): tabla y procedimientos de la calificacion de oportunidades"
```

Después de correrlo, cambiar la cabecera `PENDIENTE DE EJECUTAR.` por `YA APLICADO EN PRODUCCION (verificado el <fecha>).` e incluirlo en el mismo commit.

---

### Task 2: Entidades y criterios calculados (Tiempo, Actividad, Encaje, clasificación, ponderado)

**Archivos:**
- Crear: `CapaEntidad/EntCalificacionForeCast.cs`
- Crear: `CapaNegocio/NegCalificacionForeCast.cs`
- Crear: `CapaPruebas/NegCalificacionForeCastTests.cs`
- Modificar: `CapaEntidad/CapaEntidad.csproj`, `CapaNegocio/CapaNegocio.csproj` y `CapaPruebas/CapaPruebas.csproj` (un `<Compile Include>` cada uno, junto a los de `*DashboardAprobacion*`)

**Interfaces:**
- Produce (entidades, en `namespace CapaEntidad`):
  - `EntCalificacionRespuesta`, `EntTasaExito`, `EntAlertaCalificacion`, `EntCalificacionForeCast`, `EntDatosCalificacion`, `EntFiltroCalificacion`, `EntPantallaCalificacion`. Las propiedades son exactamente las del código de abajo.
- Produce (`CapaNegocio.NegCalificacionForeCast`, estática):
  - `int PuntosTiempo(DateTime? cierre, DateTime hoy)`
  - `DateTime? UltimaActividad(DateTime? modificacion, DateTime? calificacion)`
  - `int PuntosActividad(DateTime? ultima, DateTime hoy)`
  - `int PuntosEncaje(string segmento, string tipoEmpresa, int ganadosCliente, IList<EntTasaExito> tasas)`
  - `string Clasificar(int puntaje)`
  - `decimal MontoPonderado(decimal? pvp, decimal? porcentaje)`
  - `bool EsAvanzada(string prioridad, decimal? porcentaje)`
  - Constantes: `Caliente = "Caliente"`, `Tibia = "Tibia"`, `Fria = "Fría"`, `SinCalificarTexto = "Sin calificar"`, `IdNegocioPerdida = 10`, `MinimoCierresSegmento = 10`.

- [ ] **Paso 1: Escribir las entidades** (`CapaEntidad/EntCalificacionForeCast.cs`, UTF-8 con BOM)

```csharp
using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>Lo que responde el vendedor en el modal. Una fila de ForeCastCalificacion.</summary>
    public class EntCalificacionRespuesta
    {
        public long IdForeCast { get; set; }
        public int Presupuesto { get; set; }
        public int Autoridad { get; set; }
        public int Necesidad { get; set; }
        public string DecisorCargo { get; set; }
        public bool PropuestaEnviada { get; set; }
        public string AccionComprometida { get; set; }
        public DateTime? FechaAccion { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    /// <summary>GANADO y PERDIDO de los ultimos 36 meses por segmento o por tipo de empresa.</summary>
    public class EntTasaExito
    {
        /// <summary>"SEGMENTO" o "TIPO".</summary>
        public string Dimension { get; set; }
        public string Valor { get; set; }
        public int Ganados { get; set; }
        public int Perdidos { get; set; }
    }

    public class EntAlertaCalificacion
    {
        public int Codigo { get; set; }
        public string Texto { get; set; }
    }

    /// <summary>
    /// Una oportunidad abierta: los datos crudos que trae la base y, despues de
    /// NegCalificacionForeCast.Evaluar, el resultado de la calificacion.
    /// </summary>
    public class EntCalificacionForeCast
    {
        /* Crudos */
        public long IdForeCast { get; set; }
        public int IdCliente { get; set; }
        public string Cliente { get; set; }
        public string DetalleProyecto { get; set; }
        public string Marca { get; set; }
        public string Prioridad { get; set; }
        public string GerentedeCuenta { get; set; }
        public string Sucursal { get; set; }
        public string SegmentodeMercado { get; set; }
        public string TipoEmpresa { get; set; }
        public decimal? PVPEstimado { get; set; }
        public decimal? UtilidadBrutaEstimada { get; set; }
        public int? IdNegocio { get; set; }
        public decimal? Porcentaje { get; set; }
        public string DescripcionNegocio { get; set; }
        public DateTime? FechaCierre { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string Contacto { get; set; }
        public string LiderProyectoCliente { get; set; }
        public string Observaciones { get; set; }
        public int FechasCierreDistintas { get; set; }
        public int GanadosCliente { get; set; }
        public int ContactosCuenta { get; set; }
        /// <summary>La calificacion vigente, o null si nunca se califico.</summary>
        public EntCalificacionRespuesta Calificacion { get; set; }

        /* Resultado */
        public int Presupuesto { get; set; }
        public int Autoridad { get; set; }
        public int Necesidad { get; set; }
        public int Tiempo { get; set; }
        public int Actividad { get; set; }
        public int Encaje { get; set; }
        public int Puntaje { get; set; }
        public string Estado { get; set; }
        public bool SinCalificar { get; set; }
        public decimal MontoPonderado { get; set; }
        public DateTime? UltimaActividad { get; set; }
        public List<EntAlertaCalificacion> Alertas { get; set; } = new List<EntAlertaCalificacion>();
        public List<string> Faltantes { get; set; } = new List<string>();
        public string AccionSugerida { get; set; }

        /* Textos para la pantalla (dd/MM/yyyy, cultura invariante) */
        public string FechaCierreTexto { get; set; }
        public string UltimaActividadTexto { get; set; }
        public string FechaCalificacionTexto { get; set; }
        public string FechaAccionTexto { get; set; }
    }

    /// <summary>Lo que devuelve el DAO: los dos resultados del procedimiento.</summary>
    public class EntDatosCalificacion
    {
        public List<EntCalificacionForeCast> Oportunidades { get; set; } = new List<EntCalificacionForeCast>();
        public List<EntTasaExito> Tasas { get; set; } = new List<EntTasaExito>();
    }

    /// <summary>Filtros de la pantalla. Vacio = todos.</summary>
    public class EntFiltroCalificacion
    {
        public string GerentedeCuenta { get; set; } = "";
        public string Sucursal { get; set; } = "";
        public string Prioridad { get; set; } = "";
        /// <summary>"Caliente", "Tibia", "Fría" o "Sin calificar".</summary>
        public string Estado { get; set; } = "";
        public string Marca { get; set; } = "";
    }

    public class EntPantallaCalificacion
    {
        public List<EntCalificacionForeCast> Oportunidades { get; set; } = new List<EntCalificacionForeCast>();
        public int Total { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoPonderado { get; set; }
        public int Calientes { get; set; }
        public int Tibias { get; set; }
        public int Frias { get; set; }
        public int SinCalificar { get; set; }
        /// <summary>Entre 0 y 100, con un decimal.</summary>
        public decimal PorcentajeSinCalificar { get; set; }
        public List<long> Top5 { get; set; } = new List<long>();
        public List<long> EnRiesgo { get; set; } = new List<long>();
        /* Valores de los combos, tomados del conjunto SIN filtrar */
        public List<string> Gerentes { get; set; } = new List<string>();
        public List<string> Sucursales { get; set; } = new List<string>();
        public List<string> Etapas { get; set; } = new List<string>();
        public List<string> Marcas { get; set; } = new List<string>();
    }
}
```

Agregar en `CapaEntidad/CapaEntidad.csproj`, junto a `EntDashboardAprobacion.cs`:

```xml
    <Compile Include="EntCalificacionForeCast.cs" />
```

- [ ] **Paso 2: Escribir las pruebas que fallan** (`CapaPruebas/NegCalificacionForeCastTests.cs`, UTF-8 con BOM)

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace CapaPruebas
{
    /// <summary>
    /// Las reglas de la calificacion del Forecast. La fecha de hoy entra como
    /// parametro: ninguna prueba depende del reloj.
    /// </summary>
    [TestClass]
    public class NegCalificacionForeCastTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 9, 28);
        private CultureInfo _culturaOriginal;

        /// <summary>Cultura hostil: coma decimal y meses en espanol.</summary>
        [TestInitialize]
        public void FijarCulturaHostil()
        {
            _culturaOriginal = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
        }

        [TestCleanup]
        public void RestaurarCultura()
        {
            Thread.CurrentThread.CurrentCulture = _culturaOriginal;
        }

        /* ---------------- Tiempo ---------------- */

        [TestMethod]
        public void Tiempo_SinFecha_Cero()
        {
            Assert.AreEqual(0, NegCalificacionForeCast.PuntosTiempo(null, Hoy));
        }

        [TestMethod]
        public void Tiempo_Tramos()
        {
            Assert.AreEqual(3,  NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(-1), Hoy));
            Assert.AreEqual(15, NegCalificacionForeCast.PuntosTiempo(Hoy, Hoy));
            Assert.AreEqual(15, NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(60), Hoy));
            Assert.AreEqual(12, NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(61), Hoy));
            Assert.AreEqual(12, NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(120), Hoy));
            Assert.AreEqual(8,  NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(121), Hoy));
            Assert.AreEqual(8,  NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(240), Hoy));
            Assert.AreEqual(5,  NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(241), Hoy));
        }

        [TestMethod]
        public void Tiempo_IgnoraLaHora()
        {
            // Cierre hoy a las 23:59 sigue siendo "hoy", no "manana".
            Assert.AreEqual(15, NegCalificacionForeCast.PuntosTiempo(Hoy.AddHours(23.9), Hoy.AddHours(8)));
        }

        /* ---------------- Actividad ---------------- */

        [TestMethod]
        public void UltimaActividad_TomaLaMasReciente()
        {
            DateTime a = Hoy.AddDays(-40), b = Hoy.AddDays(-3);
            Assert.AreEqual(b, NegCalificacionForeCast.UltimaActividad(a, b));
            Assert.AreEqual(b, NegCalificacionForeCast.UltimaActividad(b, a));
            Assert.AreEqual(a, NegCalificacionForeCast.UltimaActividad(a, null));
            Assert.AreEqual(b, NegCalificacionForeCast.UltimaActividad(null, b));
            Assert.IsNull(NegCalificacionForeCast.UltimaActividad(null, null));
        }

        [TestMethod]
        public void Actividad_Tramos()
        {
            Assert.AreEqual(0,  NegCalificacionForeCast.PuntosActividad(null, Hoy));
            Assert.AreEqual(15, NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-14), Hoy));
            Assert.AreEqual(10, NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-15), Hoy));
            Assert.AreEqual(10, NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-30), Hoy));
            Assert.AreEqual(5,  NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-31), Hoy));
            Assert.AreEqual(5,  NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-60), Hoy));
            Assert.AreEqual(0,  NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-61), Hoy));
        }

        /* ---------------- Encaje ---------------- */

        private static List<EntTasaExito> Tasas()
        {
            return new List<EntTasaExito>
            {
                new EntTasaExito { Dimension = "SEGMENTO", Valor = "Alto",    Ganados = 60, Perdidos = 40 },  // 60 %
                new EntTasaExito { Dimension = "SEGMENTO", Valor = "Medio",   Ganados = 50, Perdidos = 50 },  // 50 %
                new EntTasaExito { Dimension = "SEGMENTO", Valor = "Bajo",    Ganados = 49, Perdidos = 51 },  // 49 %
                new EntTasaExito { Dimension = "SEGMENTO", Valor = "Chico",   Ganados = 9,  Perdidos = 0  },  // < 10 cierres
                new EntTasaExito { Dimension = "TIPO",     Valor = "PRIVADO", Ganados = 62, Perdidos = 38 },
                new EntTasaExito { Dimension = "TIPO",     Valor = "PUBLICO", Ganados = 55, Perdidos = 45 }
            };
        }

        [TestMethod]
        public void Encaje_Segmento()
        {
            Assert.AreEqual(5 + 1, NegCalificacionForeCast.PuntosEncaje("Alto",  "PUBLICO", 0, Tasas()));
            Assert.AreEqual(3 + 1, NegCalificacionForeCast.PuntosEncaje("Medio", "PUBLICO", 0, Tasas()));
            Assert.AreEqual(1 + 1, NegCalificacionForeCast.PuntosEncaje("Bajo",  "PUBLICO", 0, Tasas()));
            Assert.AreEqual(3 + 1, NegCalificacionForeCast.PuntosEncaje("Chico", "PUBLICO", 0, Tasas()));
            Assert.AreEqual(3 + 1, NegCalificacionForeCast.PuntosEncaje("Desconocido", "PUBLICO", 0, Tasas()));
        }

        [TestMethod]
        public void Encaje_TipoConMejorTasaSumaDos()
        {
            Assert.AreEqual(1 + 2, NegCalificacionForeCast.PuntosEncaje("Bajo", "PRIVADO", 0, Tasas()));
        }

        [TestMethod]
        public void Encaje_VentasPreviasYTope()
        {
            Assert.AreEqual(1 + 1 + 2, NegCalificacionForeCast.PuntosEncaje("Bajo", "PUBLICO", 1, Tasas()));
            Assert.AreEqual(1 + 1 + 3, NegCalificacionForeCast.PuntosEncaje("Bajo", "PUBLICO", 3, Tasas()));
            Assert.AreEqual(10,        NegCalificacionForeCast.PuntosEncaje("Alto", "PRIVADO", 9, Tasas()));  // 5+2+3
        }

        [TestMethod]
        public void Encaje_NoDependeDeEspaciosNiMayusculas()
        {
            Assert.AreEqual(5 + 2, NegCalificacionForeCast.PuntosEncaje("  alto ", " privado", 0, Tasas()));
        }

        [TestMethod]
        public void Encaje_SinTasas_NoFalla()
        {
            Assert.AreEqual(3 + 1, NegCalificacionForeCast.PuntosEncaje("Alto", "PRIVADO", 0, null));
            Assert.AreEqual(3 + 1, NegCalificacionForeCast.PuntosEncaje(null, null, 0, new List<EntTasaExito>()));
        }

        /* ---------------- Clasificacion, ponderado, etapa ---------------- */

        [TestMethod]
        public void Clasificar_Limites()
        {
            Assert.AreEqual("Fría",     NegCalificacionForeCast.Clasificar(49));
            Assert.AreEqual("Tibia",    NegCalificacionForeCast.Clasificar(50));
            Assert.AreEqual("Tibia",    NegCalificacionForeCast.Clasificar(74));
            Assert.AreEqual("Caliente", NegCalificacionForeCast.Clasificar(75));
        }

        [TestMethod]
        public void Ponderado()
        {
            Assert.AreEqual(40000m, NegCalificacionForeCast.MontoPonderado(100000m, 40m));
            Assert.AreEqual(0m,     NegCalificacionForeCast.MontoPonderado(100000m, null));
            Assert.AreEqual(0m,     NegCalificacionForeCast.MontoPonderado(100000m, 0m));
            Assert.AreEqual(0m,     NegCalificacionForeCast.MontoPonderado(null, 40m));
            Assert.AreEqual(333.33m, NegCalificacionForeCast.MontoPonderado(3333.33m, 10m));
        }

        [TestMethod]
        public void EsAvanzada()
        {
            Assert.IsTrue(NegCalificacionForeCast.EsAvanzada("UPSIDE", 0m));
            Assert.IsTrue(NegCalificacionForeCast.EsAvanzada("FORECAST", null));
            Assert.IsTrue(NegCalificacionForeCast.EsAvanzada("LEAD", 40m));
            Assert.IsFalse(NegCalificacionForeCast.EsAvanzada("PIPELINE", 20m));
            Assert.IsFalse(NegCalificacionForeCast.EsAvanzada(null, null));
        }
    }
}
```

Agregar en `CapaPruebas/CapaPruebas.csproj`, junto a `NegDashboardAprobacionTests.cs`:

```xml
    <Compile Include="NegCalificacionForeCastTests.cs" />
```

- [ ] **Paso 3: Compilar para ver que falla**

Run:
```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
```
Expected: FAIL con `CS0103: The name 'NegCalificacionForeCast' does not exist in the current context`.

- [ ] **Paso 4: Escribir los criterios** (`CapaNegocio/NegCalificacionForeCast.cs`, UTF-8 con BOM)

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Las reglas de docs/superpowers/specs/2026-09-28-calificacion-forecast-design.md.
    /// Todo lo que decide un puntaje, una alerta o una accion vive aca y en
    /// ningun otro lado: ni el procedimiento ni el JavaScript calculan nada.
    ///
    /// Ninguna funcion lee el reloj: la fecha de hoy entra como parametro, para
    /// que las pruebas fijen el dia y los limites (60 dias, 61 dias) no se
    /// muevan solos.
    /// </summary>
    public static partial class NegCalificacionForeCast
    {
        public const string Caliente = "Caliente";
        public const string Tibia = "Tibia";
        public const string Fria = "Fría";
        public const string SinCalificarTexto = "Sin calificar";

        /// <summary>PrmPorcentaje: IdNegocio 10 = "Perdida o Descartada" (verificado 2026-09-28).</summary>
        public const int IdNegocioPerdida = 10;

        /// <summary>Con menos cierres que esto un segmento no tiene evidencia y suma el neutro.</summary>
        public const int MinimoCierresSegmento = 10;

        public static int PuntosTiempo(DateTime? cierre, DateTime hoy)
        {
            if (!cierre.HasValue) { return 0; }
            int dias = (cierre.Value.Date - hoy.Date).Days;
            if (dias < 0) { return 3; }
            if (dias <= 60) { return 15; }
            if (dias <= 120) { return 12; }
            if (dias <= 240) { return 8; }
            return 5;
        }

        /// <summary>Calificar tambien es actividad: gana la fecha mas reciente.</summary>
        public static DateTime? UltimaActividad(DateTime? modificacion, DateTime? calificacion)
        {
            if (!modificacion.HasValue) { return calificacion; }
            if (!calificacion.HasValue) { return modificacion; }
            return modificacion.Value >= calificacion.Value ? modificacion : calificacion;
        }

        public static int PuntosActividad(DateTime? ultima, DateTime hoy)
        {
            if (!ultima.HasValue) { return 0; }
            int dias = (hoy.Date - ultima.Value.Date).Days;
            if (dias <= 14) { return 15; }
            if (dias <= 30) { return 10; }
            if (dias <= 60) { return 5; }
            return 0;
        }

        public static int PuntosEncaje(string segmento, string tipoEmpresa, int ganadosCliente, IList<EntTasaExito> tasas)
        {
            if (tasas == null) { tasas = new List<EntTasaExito>(); }

            int puntos = PuntosSegmento(segmento, tasas) + PuntosTipo(tipoEmpresa, tasas);
            if (ganadosCliente >= 3) { puntos += 3; }
            else if (ganadosCliente >= 1) { puntos += 2; }

            return Math.Min(puntos, 10);
        }

        private static int PuntosSegmento(string segmento, IList<EntTasaExito> tasas)
        {
            EntTasaExito t = Buscar(tasas, "SEGMENTO", segmento);
            if (t == null || t.Ganados + t.Perdidos < MinimoCierresSegmento) { return 3; }

            decimal tasa = (decimal)t.Ganados / (t.Ganados + t.Perdidos);
            if (tasa >= 0.60m) { return 5; }
            if (tasa >= 0.50m) { return 3; }
            return 1;
        }

        /// <summary>2 si es el tipo de empresa con mejor tasa de exito; 1 en cualquier otro caso.</summary>
        private static int PuntosTipo(string tipoEmpresa, IList<EntTasaExito> tasas)
        {
            EntTasaExito mejor = null;
            decimal mejorTasa = -1m;

            foreach (EntTasaExito t in tasas)
            {
                if (!Igual(t.Dimension, "TIPO") || t.Ganados + t.Perdidos == 0) { continue; }
                decimal tasa = (decimal)t.Ganados / (t.Ganados + t.Perdidos);
                if (tasa > mejorTasa) { mejorTasa = tasa; mejor = t; }
            }

            return mejor != null && Igual(mejor.Valor, tipoEmpresa) ? 2 : 1;
        }

        private static EntTasaExito Buscar(IList<EntTasaExito> tasas, string dimension, string valor)
        {
            foreach (EntTasaExito t in tasas)
            {
                if (Igual(t.Dimension, dimension) && Igual(t.Valor, valor)) { return t; }
            }
            return null;
        }

        /// <summary>Comparacion sin espacios alrededor ni mayusculas. Null solo es igual a null o vacio.</summary>
        internal static bool Igual(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public static string Clasificar(int puntaje)
        {
            if (puntaje >= 75) { return Caliente; }
            if (puntaje >= 50) { return Tibia; }
            return Fria;
        }

        /// <summary>PVPEstimado x PrmPorcentaje.Porcentaje / 100, redondeado a centavos.</summary>
        public static decimal MontoPonderado(decimal? pvp, decimal? porcentaje)
        {
            if (!pvp.HasValue || !porcentaje.HasValue) { return 0m; }
            return Math.Round(pvp.Value * porcentaje.Value / 100m, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>UPSIDE o FORECAST, o "Propuesta entregada" (40 %) en adelante.</summary>
        public static bool EsAvanzada(string prioridad, decimal? porcentaje)
        {
            if (Igual(prioridad, "UPSIDE") || Igual(prioridad, "FORECAST")) { return true; }
            return porcentaje.HasValue && porcentaje.Value >= 40m;
        }
    }
}
```

Agregar en `CapaNegocio/CapaNegocio.csproj`, junto a `NegDashboardAprobacion.cs`:

```xml
    <Compile Include="NegCalificacionForeCast.cs" />
```

- [ ] **Paso 5: Compilar y correr las pruebas**

Run:
```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```
Expected: compila; todas las pruebas pasan, las nuevas y las existentes.

- [ ] **Paso 6: Commit**

```bash
git add CapaEntidad/EntCalificacionForeCast.cs CapaEntidad/CapaEntidad.csproj CapaNegocio/NegCalificacionForeCast.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegCalificacionForeCastTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(forecast): criterios calculados de la calificacion"
```

---

### Task 3: Evaluar una oportunidad — alertas, datos faltantes y acción sugerida

**Archivos:**
- Crear: `CapaNegocio/NegCalificacionForeCast.Evaluacion.cs` (otra parte de la clase `partial`)
- Modificar: `CapaNegocio/CapaNegocio.csproj`
- Modificar: `CapaPruebas/NegCalificacionForeCastTests.cs` (agregar pruebas)

**Interfaces:**
- Consume: las funciones de la Task 2.
- Produce:
  - `void Evaluar(EntCalificacionForeCast o, IList<EntTasaExito> tasas, DateTime hoy)`: llena todos los campos de «Resultado» y los textos de fecha.
  - Constantes de alerta: `AlertaSinActividad = 1`, `AlertaCierreVencido = 2`, `AlertaMontoSinDefinir = 3`, `AlertaSinPropuesta = 4`, `AlertaSinDecisor = 5`, `AlertaCierreCambiado = 6`, `AlertaCalificacionPendiente = 7`, `AlertaInconsistente = 8`.
  - `bool TieneAlerta(EntCalificacionForeCast o, int codigo)`.

- [ ] **Paso 1: Agregar las pruebas que fallan** al final de la clase `NegCalificacionForeCastTests`

```csharp
        /* ---------------- Evaluar ---------------- */

        /// <summary>
        /// Una oportunidad "sana": calificada hoy con todo en maximo, cierre en 30
        /// dias, modificada hoy, con contacto y monto. No dispara ninguna alerta.
        /// </summary>
        private static EntCalificacionForeCast Sana()
        {
            return new EntCalificacionForeCast
            {
                IdForeCast = 1,
                IdCliente = 1,
                Cliente = "Cliente A",
                Prioridad = "PIPELINE",
                SegmentodeMercado = "Alto",
                TipoEmpresa = "PRIVADO",
                PVPEstimado = 100000m,
                UtilidadBrutaEstimada = 20000m,
                IdNegocio = 5,
                Porcentaje = 40m,
                FechaCierre = Hoy.AddDays(30),
                FechaModificacion = Hoy,
                Contacto = "Contacto A",
                ContactosCuenta = 2,
                FechasCierreDistintas = 1,
                GanadosCliente = 3,
                Calificacion = new EntCalificacionRespuesta
                {
                    IdForeCast = 1, Presupuesto = 20, Autoridad = 20, Necesidad = 20,
                    DecisorCargo = "Decisor, Gerente", PropuestaEnviada = true,
                    AccionComprometida = "Reunion de cierre", FechaAccion = Hoy.AddDays(3),
                    Usuario = "u1", FechaRegistro = Hoy
                }
            };
        }

        private static EntCalificacionForeCast Evaluada(EntCalificacionForeCast o)
        {
            NegCalificacionForeCast.Evaluar(o, Tasas(), Hoy);
            return o;
        }

        [TestMethod]
        public void Evaluar_Sana_PuntajeCompletoYSinAlertas()
        {
            EntCalificacionForeCast o = Evaluada(Sana());

            Assert.AreEqual(100, o.Puntaje);           // 20+20+20+15+15+10
            Assert.AreEqual("Caliente", o.Estado);
            Assert.IsFalse(o.SinCalificar);
            Assert.AreEqual(40000m, o.MontoPonderado);
            Assert.AreEqual(0, o.Alertas.Count);
            Assert.AreEqual(0, o.Faltantes.Count);
            Assert.AreEqual("Avanzar a la siguiente etapa", o.AccionSugerida);
        }

        [TestMethod]
        public void Evaluar_FechasEnTextoInvariante()
        {
            EntCalificacionForeCast o = Evaluada(Sana());
            Assert.AreEqual("28/10/2026", o.FechaCierreTexto);
            Assert.AreEqual("28/09/2026", o.UltimaActividadTexto);
            Assert.AreEqual("28/09/2026", o.FechaCalificacionTexto);
            Assert.AreEqual("01/10/2026", o.FechaAccionTexto);
        }

        [TestMethod]
        public void Evaluar_SinCalificar_CeroEnLosTresYMarcada()
        {
            EntCalificacionForeCast o = Sana();
            o.Calificacion = null;
            Evaluada(o);

            Assert.IsTrue(o.SinCalificar);
            Assert.AreEqual(0, o.Presupuesto + o.Autoridad + o.Necesidad);
            Assert.AreEqual(40, o.Puntaje);            // 15+15+10
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(o, NegCalificacionForeCast.AlertaCalificacionPendiente));
            CollectionAssert.Contains(o.Faltantes, "Calificación");
            CollectionAssert.Contains(o.Faltantes, "Decisor con cargo");
            Assert.AreEqual("Calificar la oportunidad", o.AccionSugerida);
        }

        [TestMethod]
        public void Alerta1_SinActividad()
        {
            EntCalificacionForeCast o = Sana();
            o.FechaModificacion = Hoy.AddDays(-31);
            o.Calificacion.FechaRegistro = Hoy.AddDays(-31);
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(Evaluada(o), NegCalificacionForeCast.AlertaSinActividad));

            EntCalificacionForeCast p = Sana();
            p.FechaModificacion = Hoy.AddDays(-30);
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaSinActividad));
        }

        [TestMethod]
        public void Alerta2_CierreVencido()
        {
            EntCalificacionForeCast o = Sana();
            o.FechaCierre = Hoy.AddDays(-1);
            Evaluada(o);
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(o, NegCalificacionForeCast.AlertaCierreVencido));
            Assert.AreEqual("Actualizar la fecha de cierre con el cliente", o.AccionSugerida);

            EntCalificacionForeCast p = Sana();
            p.FechaCierre = Hoy;
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaCierreVencido));
        }

        [TestMethod]
        public void Alerta3_MontoSinDefinirEnAvanzada()
        {
            EntCalificacionForeCast o = Sana();
            o.PVPEstimado = 0m;
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(Evaluada(o), NegCalificacionForeCast.AlertaMontoSinDefinir));

            EntCalificacionForeCast p = Sana();
            p.PVPEstimado = null; p.Porcentaje = 20m;   // no avanzada
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaMontoSinDefinir));
        }

        [TestMethod]
        public void Alerta4_AvanzadaSinPropuesta()
        {
            EntCalificacionForeCast o = Sana();
            o.Calificacion.PropuestaEnviada = false;
            Evaluada(o);
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(o, NegCalificacionForeCast.AlertaSinPropuesta));
            Assert.AreEqual("Enviar la propuesta", o.AccionSugerida);

            EntCalificacionForeCast p = Sana();
            p.Calificacion.PropuestaEnviada = false; p.Porcentaje = 20m;  // no avanzada
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaSinPropuesta));
        }

        [TestMethod]
        public void Alerta5_SinDecisor()
        {
            EntCalificacionForeCast o = Sana();
            o.ContactosCuenta = 1; o.Calificacion.Autoridad = 12;
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(Evaluada(o), NegCalificacionForeCast.AlertaSinDecisor));

            EntCalificacionForeCast p = Sana();
            p.ContactosCuenta = 1;   // Autoridad 20: hay decisor
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaSinDecisor));
        }

        [TestMethod]
        public void Alerta6_CierreCambiado()
        {
            EntCalificacionForeCast o = Sana();
            o.FechasCierreDistintas = 3;
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(Evaluada(o), NegCalificacionForeCast.AlertaCierreCambiado));

            EntCalificacionForeCast p = Sana();
            p.FechasCierreDistintas = 2;
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaCierreCambiado));
        }

        [TestMethod]
        public void Alerta7_CalificacionDesactualizada()
        {
            EntCalificacionForeCast o = Sana();
            o.Calificacion.FechaRegistro = Hoy.AddDays(-31);
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(Evaluada(o), NegCalificacionForeCast.AlertaCalificacionPendiente));

            EntCalificacionForeCast p = Sana();
            p.Calificacion.FechaRegistro = Hoy.AddDays(-30);
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaCalificacionPendiente));
        }

        [TestMethod]
        public void Alerta8_Inconsistente_TienePrioridadEnLaAccion()
        {
            EntCalificacionForeCast o = Sana();
            o.IdNegocio = NegCalificacionForeCast.IdNegocioPerdida;
            o.FechaCierre = Hoy.AddDays(-5);          // tambien vencida
            Evaluada(o);
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(o, NegCalificacionForeCast.AlertaInconsistente));
            Assert.AreEqual("Cerrar o corregir la etapa: el código indica perdida o descartada", o.AccionSugerida);
        }

        [TestMethod]
        public void Accion_CriterioMasDebil_EmpateGanaPresupuesto()
        {
            EntCalificacionForeCast o = Sana();
            o.Calificacion.Presupuesto = 12; o.Calificacion.Autoridad = 12; o.Calificacion.Necesidad = 14;
            o.ContactosCuenta = 3;
            Assert.AreEqual("Confirmar el presupuesto con el cliente", Evaluada(o).AccionSugerida);

            EntCalificacionForeCast p = Sana();
            p.Calificacion.Autoridad = 5; p.ContactosCuenta = 3;
            Assert.AreEqual("Identificar y reunirse con el decisor", Evaluada(p).AccionSugerida);

            EntCalificacionForeCast q = Sana();
            q.Calificacion.Necesidad = 6;
            Assert.AreEqual("Validar la necesidad y la fecha del requerimiento", Evaluada(q).AccionSugerida);
        }

        [TestMethod]
        public void Accion_SinActividad_CuandoLoDemasEstaBien()
        {
            EntCalificacionForeCast p = Sana();
            p.FechaModificacion = Hoy.AddDays(-40);
            p.Calificacion.FechaRegistro = Hoy.AddDays(-31);
            Evaluada(p);
            // Tiene las alertas 1 y 7. La 7 no esta en la precedencia de la accion
            // y los tres criterios estan en 20: gana "sin actividad".
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(p, NegCalificacionForeCast.AlertaSinActividad));
            Assert.AreEqual("Retomar contacto con el cliente", p.AccionSugerida);
        }

        [TestMethod]
        public void UltimaActividad_LaCalificacionCuentaComoActividad()
        {
            EntCalificacionForeCast o = Sana();
            o.FechaModificacion = Hoy.AddDays(-40);
            o.Calificacion.FechaRegistro = Hoy.AddDays(-29);
            Evaluada(o);
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(o, NegCalificacionForeCast.AlertaSinActividad));
            Assert.AreEqual(10, o.Actividad);
        }

        [TestMethod]
        public void Faltantes_SinContactoSinFechaSinUtilidad_NoFalla()
        {
            EntCalificacionForeCast o = Sana();
            o.Contacto = " "; o.LiderProyectoCliente = null;
            o.FechaCierre = null; o.UtilidadBrutaEstimada = null;
            o.PVPEstimado = null; o.Porcentaje = null;
            Evaluada(o);

            CollectionAssert.Contains(o.Faltantes, "Contacto");
            CollectionAssert.Contains(o.Faltantes, "Fecha de cierre");
            CollectionAssert.Contains(o.Faltantes, "Utilidad bruta estimada");
            Assert.AreEqual(0, o.Tiempo);
            Assert.AreEqual(0m, o.MontoPonderado);
            Assert.AreEqual("", o.FechaCierreTexto);
        }
```

- [ ] **Paso 2: Compilar para ver que falla**

Run: el mismo MSBuild de la Task 2.
Expected: FAIL con `CS0117: 'NegCalificacionForeCast' does not contain a definition for 'Evaluar'`.

- [ ] **Paso 3: Implementar** (`CapaNegocio/NegCalificacionForeCast.Evaluacion.cs`, UTF-8 con BOM)

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaNegocio
{
    public static partial class NegCalificacionForeCast
    {
        public const int AlertaSinActividad = 1;
        public const int AlertaCierreVencido = 2;
        public const int AlertaMontoSinDefinir = 3;
        public const int AlertaSinPropuesta = 4;
        public const int AlertaSinDecisor = 5;
        public const int AlertaCierreCambiado = 6;
        public const int AlertaCalificacionPendiente = 7;
        public const int AlertaInconsistente = 8;

        private const string FormatoFecha = "dd/MM/yyyy";

        /// <summary>
        /// Llena el resultado de UNA oportunidad. El orden importa: las alertas
        /// usan Autoridad y UltimaActividad, y la accion usa las alertas.
        /// </summary>
        public static void Evaluar(EntCalificacionForeCast o, IList<EntTasaExito> tasas, DateTime hoy)
        {
            if (o == null) { throw new ArgumentNullException(nameof(o)); }

            EntCalificacionRespuesta c = o.Calificacion;
            o.SinCalificar = c == null;
            o.Presupuesto = c == null ? 0 : c.Presupuesto;
            o.Autoridad = c == null ? 0 : c.Autoridad;
            o.Necesidad = c == null ? 0 : c.Necesidad;

            o.Tiempo = PuntosTiempo(o.FechaCierre, hoy);
            o.UltimaActividad = UltimaActividad(o.FechaModificacion, c == null ? (DateTime?)null : c.FechaRegistro);
            o.Actividad = PuntosActividad(o.UltimaActividad, hoy);
            o.Encaje = PuntosEncaje(o.SegmentodeMercado, o.TipoEmpresa, o.GanadosCliente, tasas);

            o.Puntaje = o.Presupuesto + o.Autoridad + o.Necesidad + o.Tiempo + o.Actividad + o.Encaje;
            o.Estado = Clasificar(o.Puntaje);
            o.MontoPonderado = MontoPonderado(o.PVPEstimado, o.Porcentaje);

            o.Alertas = Alertas(o, hoy);
            o.Faltantes = Faltantes(o);
            o.AccionSugerida = Accion(o);

            o.FechaCierreTexto = Texto(o.FechaCierre);
            o.UltimaActividadTexto = Texto(o.UltimaActividad);
            o.FechaCalificacionTexto = c == null ? "" : Texto(c.FechaRegistro);
            o.FechaAccionTexto = c == null ? "" : Texto(c.FechaAccion);
        }

        public static bool TieneAlerta(EntCalificacionForeCast o, int codigo)
        {
            if (o == null || o.Alertas == null) { return false; }
            foreach (EntAlertaCalificacion a in o.Alertas)
            {
                if (a.Codigo == codigo) { return true; }
            }
            return false;
        }

        private static List<EntAlertaCalificacion> Alertas(EntCalificacionForeCast o, DateTime hoy)
        {
            List<EntAlertaCalificacion> r = new List<EntAlertaCalificacion>();
            bool avanzada = EsAvanzada(o.Prioridad, o.Porcentaje);

            if (!o.UltimaActividad.HasValue)
            {
                r.Add(Alerta(AlertaSinActividad, "Sin actividad registrada"));
            }
            else
            {
                int dias = (hoy.Date - o.UltimaActividad.Value.Date).Days;
                if (dias > 30) { r.Add(Alerta(AlertaSinActividad, "Sin actividad hace " + dias + " días")); }
            }

            if (o.FechaCierre.HasValue && o.FechaCierre.Value.Date < hoy.Date)
            {
                r.Add(Alerta(AlertaCierreVencido, "Fecha de cierre vencida (" + Texto(o.FechaCierre) + ")"));
            }

            if (avanzada && (!o.PVPEstimado.HasValue || o.PVPEstimado.Value == 0m))
            {
                r.Add(Alerta(AlertaMontoSinDefinir, "Monto en cero o sin definir en etapa avanzada"));
            }

            if (avanzada && (o.Calificacion == null || !o.Calificacion.PropuestaEnviada))
            {
                r.Add(Alerta(AlertaSinPropuesta, "Etapa avanzada sin propuesta enviada"));
            }

            if (o.ContactosCuenta <= 1 && o.Autoridad < 20)
            {
                r.Add(Alerta(AlertaSinDecisor, "Un solo contacto en la cuenta y sin decisor identificado"));
            }

            if (o.FechasCierreDistintas >= 3)
            {
                r.Add(Alerta(AlertaCierreCambiado, o.FechasCierreDistintas + " fechas de cierre distintas en el historial"));
            }

            if (o.Calificacion == null)
            {
                r.Add(Alerta(AlertaCalificacionPendiente, "Sin calificar"));
            }
            else
            {
                int dias = (hoy.Date - o.Calificacion.FechaRegistro.Date).Days;
                if (dias > 30) { r.Add(Alerta(AlertaCalificacionPendiente, "Calificación desactualizada (hace " + dias + " días)")); }
            }

            if (o.IdNegocio.HasValue && o.IdNegocio.Value == IdNegocioPerdida)
            {
                r.Add(Alerta(AlertaInconsistente, "Sigue abierta pero el código de cierre indica perdida o descartada"));
            }

            return r;
        }

        private static List<string> Faltantes(EntCalificacionForeCast o)
        {
            List<string> r = new List<string>();
            if (Vacio(o.Contacto) && Vacio(o.LiderProyectoCliente)) { r.Add("Contacto"); }
            if (!o.FechaCierre.HasValue) { r.Add("Fecha de cierre"); }
            if (!o.UtilidadBrutaEstimada.HasValue || o.UtilidadBrutaEstimada.Value == 0m) { r.Add("Utilidad bruta estimada"); }
            if (o.Calificacion == null || Vacio(o.Calificacion.DecisorCargo)) { r.Add("Decisor con cargo"); }
            if (o.Calificacion == null) { r.Add("Calificación"); }
            return r;
        }

        /// <summary>La primera que aplique, en el orden de la especificacion.</summary>
        private static string Accion(EntCalificacionForeCast o)
        {
            if (TieneAlerta(o, AlertaInconsistente)) { return "Cerrar o corregir la etapa: el código indica perdida o descartada"; }
            if (TieneAlerta(o, AlertaCierreVencido)) { return "Actualizar la fecha de cierre con el cliente"; }
            if (o.Calificacion == null) { return "Calificar la oportunidad"; }
            if (TieneAlerta(o, AlertaSinPropuesta)) { return "Enviar la propuesta"; }

            /* Los tres tienen el mismo maximo (20), asi que "en proporcion a su
               maximo" es comparar los puntos. En empate gana el primero. */
            int minimo = Math.Min(o.Presupuesto, Math.Min(o.Autoridad, o.Necesidad));
            if (minimo < 20)
            {
                if (o.Presupuesto == minimo) { return "Confirmar el presupuesto con el cliente"; }
                if (o.Autoridad == minimo) { return "Identificar y reunirse con el decisor"; }
                return "Validar la necesidad y la fecha del requerimiento";
            }

            if (TieneAlerta(o, AlertaSinActividad)) { return "Retomar contacto con el cliente"; }
            return "Avanzar a la siguiente etapa";
        }

        private static EntAlertaCalificacion Alerta(int codigo, string texto)
        {
            return new EntAlertaCalificacion { Codigo = codigo, Texto = texto };
        }

        private static bool Vacio(string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        /// <summary>dd/MM/yyyy con cultura invariante: el servidor corre en es-EC.</summary>
        private static string Texto(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString(FormatoFecha, CultureInfo.InvariantCulture) : "";
        }
    }
}
```

Agregar en `CapaNegocio/CapaNegocio.csproj`, debajo de `NegCalificacionForeCast.cs`:

```xml
    <Compile Include="NegCalificacionForeCast.Evaluacion.cs" />
```

- [ ] **Paso 4: Compilar y correr las pruebas**

Run: el MSBuild y el vstest de la Task 2.
Expected: todas pasan.

- [ ] **Paso 5: Commit**

```bash
git add CapaNegocio/NegCalificacionForeCast.Evaluacion.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegCalificacionForeCastTests.cs
git commit -m "feat(forecast): alertas, datos faltantes y accion sugerida"
```

---

### Task 4: Pantalla armada, validación y guardado seguro

**Archivos:**
- Crear: `CapaNegocio/NegCalificacionForeCast.Pantalla.cs`
- Crear: `CapaDato/DaoCalificacionForeCast.cs`
- Modificar: `CapaNegocio/CapaNegocio.csproj`, `CapaDato/CapaDato.csproj`, `CapaPruebas/CapaPruebas.csproj` (referencia a `System.Web.Extensions`) y `CapaPruebas/NegCalificacionForeCastTests.cs`

**Interfaces:**
- Consume: `Evaluar` (Task 3) y los SP de la Task 1.
- Produce:
  - `EntPantallaCalificacion Armar(EntDatosCalificacion datos, EntFiltroCalificacion filtro, DateTime hoy)`: función pura.
  - `string ValidarRespuesta(EntCalificacionRespuesta r)`: devuelve `""` si es válida o el mensaje de error.
  - `string ValidarGuardado(EntCalificacionRespuesta r, IEnumerable<long> idsVisibles)`: función pura.
  - `EntPantallaCalificacion Cargar(string idUsuario, EntFiltroCalificacion filtro)`: usa la base y `DateTime.Today`.
  - `EntRespuesta Guardar(EntCalificacionRespuesta r, string idUsuario)`: usa la base.
  - `CapaDato.DaoCalificacionForeCast.Cargar(string idUsuario) : EntDatosCalificacion`
  - `CapaDato.DaoCalificacionForeCast.Guardar(EntCalificacionRespuesta r) : EntRespuesta`

- [ ] **Paso 1: Agregar las pruebas que fallan** al final de la clase

```csharp
        /* ---------------- Armar ---------------- */

        private static EntDatosCalificacion Datos()
        {
            EntCalificacionForeCast caliente = Sana();                 // 100, pond 40.000

            EntCalificacionForeCast tibiaGrande = Sana();
            tibiaGrande.IdForeCast = 2; tibiaGrande.PVPEstimado = 500000m;          // pond 200.000
            tibiaGrande.Calificacion.Presupuesto = 5; tibiaGrande.Calificacion.Autoridad = 5;
            tibiaGrande.Calificacion.Necesidad = 6;                                   // 16+15+15+10 = 56
            tibiaGrande.Sucursal = "SUR"; tibiaGrande.GerentedeCuenta = "G2";

            EntCalificacionForeCast fria = Sana();
            fria.IdForeCast = 3; fria.Calificacion = null;
            fria.FechaModificacion = Hoy.AddDays(-90);                                // alerta 1
            fria.FechaCierre = Hoy.AddDays(-10);                                      // alerta 2

            EntCalificacionForeCast friaSinRiesgo = Sana();
            friaSinRiesgo.IdForeCast = 4; friaSinRiesgo.Calificacion = null;          // 40, sin alertas 1/2/8

            caliente.Sucursal = fria.Sucursal = friaSinRiesgo.Sucursal = "NORTE";
            caliente.GerentedeCuenta = fria.GerentedeCuenta = friaSinRiesgo.GerentedeCuenta = "G1";

            return new EntDatosCalificacion
            {
                Oportunidades = new List<EntCalificacionForeCast> { fria, friaSinRiesgo, tibiaGrande, caliente },
                Tasas = Tasas()
            };
        }

        [TestMethod]
        public void Armar_OrdenaIndicadoresTopYRiesgo()
        {
            EntPantallaCalificacion p = NegCalificacionForeCast.Armar(Datos(), new EntFiltroCalificacion(), Hoy);

            CollectionAssert.AreEqual(new long[] { 1, 2, 4, 3 }, IdsDe(p));   // por puntaje desc
            Assert.AreEqual(4, p.Total);
            Assert.AreEqual(1, p.Calientes);
            Assert.AreEqual(1, p.Tibias);
            Assert.AreEqual(2, p.Frias);
            Assert.AreEqual(2, p.SinCalificar);
            Assert.AreEqual(50.0m, p.PorcentajeSinCalificar);
            Assert.AreEqual(800000m, p.MontoTotal);
            CollectionAssert.AreEqual(new long[] { 2, 1 }, p.Top5);            // por ponderado desc
            CollectionAssert.AreEqual(new long[] { 3 }, p.EnRiesgo);
            CollectionAssert.AreEqual(new[] { "G1", "G2" }, p.Gerentes);
        }

        [TestMethod]
        public void Armar_Filtros_NoCambianLosCombos()
        {
            EntPantallaCalificacion p = NegCalificacionForeCast.Armar(
                Datos(), new EntFiltroCalificacion { Sucursal = " sur " }, Hoy);
            CollectionAssert.AreEqual(new long[] { 2 }, IdsDe(p));
            CollectionAssert.AreEqual(new[] { "NORTE", "SUR" }, p.Sucursales);

            EntPantallaCalificacion q = NegCalificacionForeCast.Armar(
                Datos(), new EntFiltroCalificacion { Estado = "Sin calificar" }, Hoy);
            CollectionAssert.AreEqual(new long[] { 4, 3 }, IdsDe(q));

            EntPantallaCalificacion r = NegCalificacionForeCast.Armar(
                Datos(), new EntFiltroCalificacion { Estado = "Fría" }, Hoy);
            Assert.AreEqual(2, r.Total);
        }

        [TestMethod]
        public void Armar_Vacio_NoDivideEntreCero()
        {
            EntPantallaCalificacion p = NegCalificacionForeCast.Armar(new EntDatosCalificacion(), null, Hoy);
            Assert.AreEqual(0, p.Total);
            Assert.AreEqual(0m, p.PorcentajeSinCalificar);
        }

        [TestMethod]
        public void Json_ConPuntoDecimalEnCulturaEsES()
        {
            EntPantallaCalificacion p = new EntPantallaCalificacion { MontoPonderado = 1234.5m, PorcentajeSinCalificar = 33.3m };
            string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(p);
            StringAssert.Contains(json, "\"MontoPonderado\":1234.5");
            StringAssert.Contains(json, "\"PorcentajeSinCalificar\":33.3");
            Assert.IsFalse(json.Contains("1234,5"));
        }

        private static long[] IdsDe(EntPantallaCalificacion p)
        {
            List<long> ids = new List<long>();
            foreach (EntCalificacionForeCast o in p.Oportunidades) { ids.Add(o.IdForeCast); }
            return ids.ToArray();
        }

        /* ---------------- Validacion ---------------- */

        private static EntCalificacionRespuesta Respuesta()
        {
            return new EntCalificacionRespuesta { IdForeCast = 7, Presupuesto = 12, Autoridad = 5, Necesidad = 14 };
        }

        [TestMethod]
        public void ValidarRespuesta_AceptaSoloLosPuntosDelModal()
        {
            Assert.AreEqual("", NegCalificacionForeCast.ValidarRespuesta(Respuesta()));

            EntCalificacionRespuesta a = Respuesta(); a.Presupuesto = 7;
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(a));
            EntCalificacionRespuesta b = Respuesta(); b.Necesidad = 12;   // 12 es de Presupuesto, no de Necesidad
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(b));
            EntCalificacionRespuesta c = Respuesta(); c.IdForeCast = 0;
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(c));
            EntCalificacionRespuesta d = Respuesta(); d.DecisorCargo = new string('x', 151);
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(d));
            EntCalificacionRespuesta e = Respuesta(); e.AccionComprometida = new string('x', 301);
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(e));
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(null));
        }

        [TestMethod]
        public void ValidarGuardado_RechazaUnaOportunidadQueElUsuarioNoVe()
        {
            Assert.AreEqual("", NegCalificacionForeCast.ValidarGuardado(Respuesta(), new long[] { 5, 7 }));
            Assert.AreEqual("No tiene acceso a esa oportunidad.",
                NegCalificacionForeCast.ValidarGuardado(Respuesta(), new long[] { 5, 6 }));
        }
```

Agregar en `CapaPruebas/CapaPruebas.csproj`, junto a `<Reference Include="System.Core" />`:

```xml
    <Reference Include="System.Web.Extensions" />
```

- [ ] **Paso 2: Compilar para ver que falla**

Expected: FAIL con `CS0117: … does not contain a definition for 'Armar'`.

- [ ] **Paso 3: Escribir el DAO** (`CapaDato/DaoCalificacionForeCast.cs`, UTF-8 con BOM)

```csharp
using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Los dos procedimientos de la calificacion. El de lista devuelve dos
    /// resultados en este orden: oportunidades y tasas de exito.
    /// </summary>
    public class DaoCalificacionForeCast
    {
        public static EntDatosCalificacion Cargar(string idUsuario)
        {
            EntDatosCalificacion d = new EntDatosCalificacion();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTAListaCalificacionForeCast", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 90;
                cmd.Parameters.Add("@Idusuario", SqlDbType.VarChar, 10).Value = idUsuario ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        EntCalificacionForeCast o = new EntCalificacionForeCast
                        {
                            IdForeCast            = Convert.ToInt64(dr["IdForeCast"]),
                            IdCliente             = EnteroNulo(dr, "IdCliente") ?? 0,
                            Cliente               = Texto(dr, "Cliente"),
                            DetalleProyecto       = Texto(dr, "DetalleProyecto"),
                            Marca                 = Texto(dr, "Marca"),
                            Prioridad             = Texto(dr, "Prioridad"),
                            GerentedeCuenta       = Texto(dr, "GerentedeCuenta"),
                            Sucursal              = Texto(dr, "Sucursal"),
                            SegmentodeMercado     = Texto(dr, "SegmentodeMercado"),
                            TipoEmpresa           = Texto(dr, "TipoEmpresa"),
                            PVPEstimado           = DecimalNulo(dr, "PVPEstimado"),
                            UtilidadBrutaEstimada = DecimalNulo(dr, "UtilidadBrutaEstimada"),
                            IdNegocio             = EnteroNulo(dr, "IdNegocio"),
                            Porcentaje            = DecimalNulo(dr, "Porcentaje"),
                            DescripcionNegocio    = Texto(dr, "DescripcionNegocio"),
                            FechaCierre           = FechaNula(dr, "FechaCierre"),
                            FechaModificacion     = FechaNula(dr, "FechaModificacion"),
                            Contacto              = Texto(dr, "Contacto"),
                            LiderProyectoCliente  = Texto(dr, "LiderProyectoCliente"),
                            Observaciones         = Texto(dr, "Observaciones"),
                            FechasCierreDistintas = EnteroNulo(dr, "FechasCierreDistintas") ?? 0,
                            GanadosCliente        = EnteroNulo(dr, "GanadosCliente") ?? 0,
                            ContactosCuenta       = EnteroNulo(dr, "ContactosCuenta") ?? 0
                        };

                        if (dr["FechaCalificacion"] != DBNull.Value)
                        {
                            o.Calificacion = new EntCalificacionRespuesta
                            {
                                IdForeCast         = o.IdForeCast,
                                Presupuesto        = EnteroNulo(dr, "Presupuesto") ?? 0,
                                Autoridad          = EnteroNulo(dr, "Autoridad") ?? 0,
                                Necesidad          = EnteroNulo(dr, "Necesidad") ?? 0,
                                DecisorCargo       = Texto(dr, "DecisorCargo"),
                                PropuestaEnviada   = dr["PropuestaEnviada"] != DBNull.Value && Convert.ToBoolean(dr["PropuestaEnviada"]),
                                AccionComprometida = Texto(dr, "AccionComprometida"),
                                FechaAccion        = FechaNula(dr, "FechaAccion"),
                                Usuario            = Texto(dr, "UsuarioCalificacion"),
                                FechaRegistro      = Convert.ToDateTime(dr["FechaCalificacion"])
                            };
                        }

                        d.Oportunidades.Add(o);
                    }

                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.Tasas.Add(new EntTasaExito
                            {
                                Dimension = Texto(dr, "Dimension"),
                                Valor     = Texto(dr, "Valor"),
                                Ganados   = EnteroNulo(dr, "Ganados") ?? 0,
                                Perdidos  = EnteroNulo(dr, "Perdidos") ?? 0
                            });
                        }
                    }
                }
            }

            return d;
        }

        public static EntRespuesta Guardar(EntCalificacionRespuesta r)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTAInsertaCalificacionForeCast", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdForeCast", SqlDbType.BigInt).Value = r.IdForeCast;
                cmd.Parameters.Add("@Presupuesto", SqlDbType.TinyInt).Value = r.Presupuesto;
                cmd.Parameters.Add("@Autoridad", SqlDbType.TinyInt).Value = r.Autoridad;
                cmd.Parameters.Add("@Necesidad", SqlDbType.TinyInt).Value = r.Necesidad;
                cmd.Parameters.Add("@DecisorCargo", SqlDbType.VarChar, 150).Value = (object)r.DecisorCargo ?? DBNull.Value;
                cmd.Parameters.Add("@PropuestaEnviada", SqlDbType.Bit).Value = r.PropuestaEnviada;
                cmd.Parameters.Add("@AccionComprometida", SqlDbType.VarChar, 300).Value = (object)r.AccionComprometida ?? DBNull.Value;
                cmd.Parameters.Add("@FechaAccion", SqlDbType.Date).Value = r.FechaAccion.HasValue ? (object)r.FechaAccion.Value.Date : DBNull.Value;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = r.Usuario ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        respuesta.estado = Convert.ToString(dr["Estado"]);
                        respuesta.mensaje = Texto(dr, "Mensaje");
                    }
                    else
                    {
                        respuesta.estado = "0";
                        respuesta.mensaje = "El procedimiento no devolvio respuesta.";
                    }
                }
            }

            respuesta.tipoMensaje = respuesta.estado == "1" ? "success" : "warning";
            return respuesta;
        }

        private static string Texto(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? "" : Convert.ToString(v).Trim();
        }

        private static int? EnteroNulo(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (int?)null : Convert.ToInt32(v);
        }

        private static decimal? DecimalNulo(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (decimal?)null : Convert.ToDecimal(v);
        }

        private static DateTime? FechaNula(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(v);
        }
    }
}
```

Agregar en `CapaDato/CapaDato.csproj`, junto a `DaoDashboardAprobacion.cs`:

```xml
    <Compile Include="DaoCalificacionForeCast.cs" />
```

- [ ] **Paso 4: Escribir la pantalla, la validación y el guardado** (`CapaNegocio/NegCalificacionForeCast.Pantalla.cs`, UTF-8 con BOM)

```csharp
using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    public static partial class NegCalificacionForeCast
    {
        private static readonly int[] PuntosPresupuesto = { 0, 5, 12, 20 };
        private static readonly int[] PuntosAutoridad = { 0, 5, 12, 20 };
        private static readonly int[] PuntosNecesidad = { 0, 6, 14, 20 };

        public static EntPantallaCalificacion Cargar(string idUsuario, EntFiltroCalificacion filtro)
        {
            EntDatosCalificacion datos = DaoCalificacionForeCast.Cargar(idUsuario);
            return Armar(datos, filtro, DateTime.Today);
        }

        /// <summary>
        /// Evalua todo, arma los combos con el conjunto SIN filtrar (para que un
        /// filtro no se quede sin la opcion de volver atras), filtra, ordena y
        /// calcula los indicadores sobre lo filtrado.
        /// </summary>
        public static EntPantallaCalificacion Armar(EntDatosCalificacion datos, EntFiltroCalificacion filtro, DateTime hoy)
        {
            if (datos == null) { datos = new EntDatosCalificacion(); }
            if (filtro == null) { filtro = new EntFiltroCalificacion(); }

            List<EntCalificacionForeCast> todas = datos.Oportunidades ?? new List<EntCalificacionForeCast>();
            foreach (EntCalificacionForeCast o in todas) { Evaluar(o, datos.Tasas, hoy); }

            EntPantallaCalificacion p = new EntPantallaCalificacion
            {
                Gerentes = Distintos(todas.Select(o => o.GerentedeCuenta)),
                Sucursales = Distintos(todas.Select(o => o.Sucursal)),
                Etapas = Distintos(todas.Select(o => o.Prioridad)),
                Marcas = Distintos(todas.Select(o => o.Marca))
            };

            p.Oportunidades = todas
                .Where(o => Pasa(o, filtro))
                .OrderByDescending(o => o.Puntaje)
                .ThenByDescending(o => o.MontoPonderado)
                .ThenBy(o => o.IdForeCast)
                .ToList();

            p.Total = p.Oportunidades.Count;
            p.MontoTotal = p.Oportunidades.Sum(o => o.PVPEstimado ?? 0m);
            p.MontoPonderado = p.Oportunidades.Sum(o => o.MontoPonderado);
            p.Calientes = p.Oportunidades.Count(o => o.Estado == Caliente);
            p.Tibias = p.Oportunidades.Count(o => o.Estado == Tibia);
            p.Frias = p.Oportunidades.Count(o => o.Estado == Fria);
            p.SinCalificar = p.Oportunidades.Count(o => o.SinCalificar);
            p.PorcentajeSinCalificar = p.Total == 0
                ? 0m
                : Math.Round(100m * p.SinCalificar / p.Total, 1, MidpointRounding.AwayFromZero);

            p.Top5 = p.Oportunidades
                .Where(o => o.Estado == Caliente || o.Estado == Tibia)
                .OrderByDescending(o => o.MontoPonderado)
                .ThenBy(o => o.FechaCierre.HasValue ? 0 : 1)
                .ThenBy(o => o.FechaCierre)
                .Take(5)
                .Select(o => o.IdForeCast)
                .ToList();

            p.EnRiesgo = p.Oportunidades
                .Where(o => o.Estado == Fria
                         && (TieneAlerta(o, AlertaSinActividad)
                          || TieneAlerta(o, AlertaCierreVencido)
                          || TieneAlerta(o, AlertaInconsistente)))
                .Select(o => o.IdForeCast)
                .ToList();

            return p;
        }

        private static bool Pasa(EntCalificacionForeCast o, EntFiltroCalificacion f)
        {
            if (!Vacio(f.GerentedeCuenta) && !Igual(o.GerentedeCuenta, f.GerentedeCuenta)) { return false; }
            if (!Vacio(f.Sucursal) && !Igual(o.Sucursal, f.Sucursal)) { return false; }
            if (!Vacio(f.Prioridad) && !Igual(o.Prioridad, f.Prioridad)) { return false; }
            if (!Vacio(f.Marca) && !Igual(o.Marca, f.Marca)) { return false; }
            if (!Vacio(f.Estado))
            {
                if (Igual(f.Estado, SinCalificarTexto)) { return o.SinCalificar; }
                return Igual(o.Estado, f.Estado);
            }
            return true;
        }

        private static List<string> Distintos(IEnumerable<string> valores)
        {
            return valores
                .Where(v => !Vacio(v))
                .Select(v => v.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static string ValidarRespuesta(EntCalificacionRespuesta r)
        {
            if (r == null) { return "No llegó la calificación."; }
            if (r.IdForeCast <= 0) { return "No se indicó la oportunidad."; }
            if (!PuntosPresupuesto.Contains(r.Presupuesto)) { return "La respuesta de Presupuesto no es válida."; }
            if (!PuntosAutoridad.Contains(r.Autoridad)) { return "La respuesta de Autoridad no es válida."; }
            if (!PuntosNecesidad.Contains(r.Necesidad)) { return "La respuesta de Necesidad no es válida."; }
            if ((r.DecisorCargo ?? "").Trim().Length > 150) { return "El decisor y su cargo no pueden pasar de 150 caracteres."; }
            if ((r.AccionComprometida ?? "").Trim().Length > 300) { return "La acción comprometida no puede pasar de 300 caracteres."; }
            return "";
        }

        /// <summary>
        /// El IdForeCast llega del navegador: sin esta comprobacion, quien lo
        /// cambie a mano podria calificar una oportunidad fuera de su visibilidad.
        /// </summary>
        public static string ValidarGuardado(EntCalificacionRespuesta r, IEnumerable<long> idsVisibles)
        {
            string error = ValidarRespuesta(r);
            if (error != "") { return error; }
            if (idsVisibles == null || !idsVisibles.Contains(r.IdForeCast)) { return "No tiene acceso a esa oportunidad."; }
            return "";
        }

        public static EntRespuesta Guardar(EntCalificacionRespuesta r, string idUsuario)
        {
            EntDatosCalificacion visibles = DaoCalificacionForeCast.Cargar(idUsuario);
            string error = ValidarGuardado(r, visibles.Oportunidades.Select(o => o.IdForeCast));
            if (error != "")
            {
                return new EntRespuesta { estado = "0", mensaje = error, tipoMensaje = "warning" };
            }

            r.DecisorCargo = Vacio(r.DecisorCargo) ? null : r.DecisorCargo.Trim();
            r.AccionComprometida = Vacio(r.AccionComprometida) ? null : r.AccionComprometida.Trim();
            r.Usuario = idUsuario;
            return DaoCalificacionForeCast.Guardar(r);
        }
    }
}
```

Agregar en `CapaNegocio/CapaNegocio.csproj`:

```xml
    <Compile Include="NegCalificacionForeCast.Pantalla.cs" />
```

- [ ] **Paso 5: Compilar y correr las pruebas**

Expected: todas pasan.

- [ ] **Paso 6: Commit**

```bash
git add CapaDato/DaoCalificacionForeCast.cs CapaDato/CapaDato.csproj CapaNegocio/NegCalificacionForeCast.Pantalla.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegCalificacionForeCastTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(forecast): pantalla armada, validacion y guardado de la calificacion"
```

---

### Task 5: Exportar a Excel

**Archivos:**
- Crear: `CapaNegocio/NegCalificacionExportacion.cs`
- Crear: `CapaPruebas/NegCalificacionExportacionTests.cs`
- Modificar: `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Consume: `EntPantallaCalificacion` (Task 2) y `Armar` (Task 4).
- Produce: `byte[] NegCalificacionExportacion.Construir(EntPantallaCalificacion pantalla)` y `string NegCalificacionExportacion.NombreDeArchivo(DateTime hoy)`, que devuelve `"Calificacion_ForeCast_yyyyMMdd.xlsx"`.

- [ ] **Paso 1: Escribir la prueba que falla** (`CapaPruebas/NegCalificacionExportacionTests.cs`, UTF-8 con BOM)

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace CapaPruebas
{
    [TestClass]
    public class NegCalificacionExportacionTests
    {
        private static EntPantallaCalificacion Pantalla()
        {
            EntCalificacionForeCast o = new EntCalificacionForeCast
            {
                IdForeCast = 9, Cliente = "Cliente A", DetalleProyecto = "Proyecto A", Prioridad = "PIPELINE",
                GerentedeCuenta = "G1", PVPEstimado = 1234.5m, MontoPonderado = 493.8m,
                Presupuesto = 12, Autoridad = 5, Necesidad = 14, Tiempo = 15, Actividad = 10, Encaje = 7,
                Puntaje = 63, Estado = "Tibia", FechaCierreTexto = "30/10/2026",
                Alertas = new List<EntAlertaCalificacion> { new EntAlertaCalificacion { Codigo = 1, Texto = "Uno" },
                                                           new EntAlertaCalificacion { Codigo = 2, Texto = "Dos" } },
                Faltantes = new List<string> { "Contacto" },
                AccionSugerida = "Enviar la propuesta"
            };
            return new EntPantallaCalificacion { Oportunidades = new List<EntCalificacionForeCast> { o }, Total = 1 };
        }

        [TestMethod]
        public void Construir_EncabezadoFilaYNumeros()
        {
            byte[] archivo = NegCalificacionExportacion.Construir(Pantalla());

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage p = new ExcelPackage(new MemoryStream(archivo)))
            {
                ExcelWorksheet h = p.Workbook.Worksheets["Calificación"];
                Assert.IsNotNull(h);
                Assert.AreEqual("ID", h.Cells[1, 1].Text);
                Assert.AreEqual("Puntaje", h.Cells[1, 15].Text);
                Assert.AreEqual(9L, Convert.ToInt64(h.Cells[2, 1].Value));
                Assert.AreEqual("Cliente A", h.Cells[2, 2].Text);
                Assert.AreEqual(1234.5m, Convert.ToDecimal(h.Cells[2, 6].Value));   // numero, no texto
                Assert.AreEqual(63, Convert.ToInt32(h.Cells[2, 15].Value));
                Assert.AreEqual("Uno; Dos", h.Cells[2, 17].Text);
                Assert.IsNull(h.Cells[3, 1].Value);
            }
        }

        [TestMethod]
        public void Construir_Vacio_SoloEncabezado()
        {
            byte[] archivo = NegCalificacionExportacion.Construir(new EntPantallaCalificacion());
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage p = new ExcelPackage(new MemoryStream(archivo)))
            {
                Assert.IsNull(p.Workbook.Worksheets["Calificación"].Cells[2, 1].Value);
            }
        }

        [TestMethod]
        public void NombreDeArchivo()
        {
            Assert.AreEqual("Calificacion_ForeCast_20260928.xlsx",
                NegCalificacionExportacion.NombreDeArchivo(new DateTime(2026, 9, 28)));
        }
    }
}
```

Agregar en `CapaPruebas/CapaPruebas.csproj`:

```xml
    <Compile Include="NegCalificacionExportacionTests.cs" />
```

- [ ] **Paso 2: Compilar para ver que falla**

Expected: FAIL con `CS0103: The name 'NegCalificacionExportacion' does not exist`.

- [ ] **Paso 3: Implementar** (`CapaNegocio/NegCalificacionExportacion.cs`, UTF-8 con BOM)

```csharp
using CapaEntidad;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// La tabla de la pantalla en xlsx, armada en memoria, como NegHeExportacion.
    /// Montos y puntajes van como numeros, no como texto, para que se puedan sumar.
    /// </summary>
    public static class NegCalificacionExportacion
    {
        public const string NombreHoja = "Calificación";

        private static readonly string[] Encabezados =
        {
            "ID", "Cliente", "Oportunidad", "Etapa", "Gerente de cuenta", "Monto (USD)", "Monto ponderado (USD)",
            "Cierre estimado", "Presupuesto /20", "Autoridad /20", "Necesidad /20", "Tiempo /15", "Actividad /15",
            "Encaje /10", "Puntaje", "Estado", "Alertas", "Datos faltantes", "Acción sugerida", "Acción comprometida"
        };

        public static string NombreDeArchivo(DateTime hoy)
        {
            return "Calificacion_ForeCast_" + hoy.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".xlsx";
        }

        public static byte[] Construir(EntPantallaCalificacion pantalla)
        {
            if (pantalla == null) { throw new ArgumentNullException(nameof(pantalla)); }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage paquete = new ExcelPackage())
            {
                ExcelWorksheet h = paquete.Workbook.Worksheets.Add(NombreHoja);

                for (int c = 0; c < Encabezados.Length; c++)
                {
                    h.Cells[1, c + 1].Value = Encabezados[c];
                    h.Cells[1, c + 1].Style.Font.Bold = true;
                }

                int fila = 2;
                foreach (EntCalificacionForeCast o in pantalla.Oportunidades ?? new List<EntCalificacionForeCast>())
                {
                    h.Cells[fila, 1].Value = o.IdForeCast;
                    h.Cells[fila, 2].Value = o.Cliente;
                    h.Cells[fila, 3].Value = o.DetalleProyecto;
                    h.Cells[fila, 4].Value = o.Prioridad;
                    h.Cells[fila, 5].Value = o.GerentedeCuenta;
                    h.Cells[fila, 6].Value = o.PVPEstimado ?? 0m;
                    h.Cells[fila, 7].Value = o.MontoPonderado;
                    h.Cells[fila, 8].Value = o.FechaCierreTexto;
                    h.Cells[fila, 9].Value = o.Presupuesto;
                    h.Cells[fila, 10].Value = o.Autoridad;
                    h.Cells[fila, 11].Value = o.Necesidad;
                    h.Cells[fila, 12].Value = o.Tiempo;
                    h.Cells[fila, 13].Value = o.Actividad;
                    h.Cells[fila, 14].Value = o.Encaje;
                    h.Cells[fila, 15].Value = o.Puntaje;
                    h.Cells[fila, 16].Value = o.SinCalificar ? o.Estado + " (" + NegCalificacionForeCast.SinCalificarTexto + ")" : o.Estado;
                    h.Cells[fila, 17].Value = string.Join("; ", (o.Alertas ?? new List<EntAlertaCalificacion>()).Select(a => a.Texto));
                    h.Cells[fila, 18].Value = string.Join(", ", o.Faltantes ?? new List<string>());
                    h.Cells[fila, 19].Value = o.AccionSugerida;
                    h.Cells[fila, 20].Value = o.Calificacion == null ? "" : o.Calificacion.AccionComprometida;
                    fila++;
                }

                h.Cells[2, 6, Math.Max(fila - 1, 2), 7].Style.Numberformat.Format = "#,##0";
                h.View.FreezePanes(2, 3);
                h.Cells[1, 1, Math.Max(fila - 1, 1), Encabezados.Length].AutoFilter = true;
                h.Cells[h.Dimension.Address].AutoFitColumns(8, 60);

                return paquete.GetAsByteArray();
            }
        }
    }
}
```

Agregar en `CapaNegocio/CapaNegocio.csproj`:

```xml
    <Compile Include="NegCalificacionExportacion.cs" />
```

- [ ] **Paso 4: Compilar y correr las pruebas**

Expected: todas pasan.

- [ ] **Paso 5: Commit**

```bash
git add CapaNegocio/NegCalificacionExportacion.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegCalificacionExportacionTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(forecast): exportar la calificacion a Excel"
```

---

### Task 6: Handlers — JSON (listar, guardar) y descarga

**Archivos:**
- Crear: `ReporteTareas/Formulario/AdministrarCalificacionForeCast.ashx` + `.ashx.cs`
- Crear: `ReporteTareas/Formulario/DescargarCalificacionForeCast.ashx` + `.ashx.cs`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consume: `NegCalificacionForeCast.Cargar`, `NegCalificacionForeCast.Guardar` y `NegCalificacionExportacion.Construir/NombreDeArchivo`.
- Produce:
  - **POST** a `AdministrarCalificacionForeCast.ashx` con el cuerpo `[{ "action": …, "parameters": { … } }]`:
    - `ListaCalificacionForeCast`, con los parámetros `session, gerente, sucursal, etapa, estado, marca`. Devuelve `EntPantallaCalificacion` en JSON.
    - `GuardarCalificacionForeCast`, con los parámetros `session, idForeCast, presupuesto, autoridad, necesidad, decisorCargo, propuestaEnviada ("1"/"0"), accionComprometida, fechaAccion ("yyyy-MM-dd" o "")`. Devuelve `EntRespuesta` en JSON.
  - **GET** a `DescargarCalificacionForeCast.ashx?gerente=&sucursal=&etapa=&estado=&marca=`, que devuelve el xlsx.
- `session` es el `Cod_Usuario` encriptado que la página deja en `txtUsuario`. El handler lo desencripta y lo pasa como `@Idusuario`, **igual que `ObteneConsultarForeCastFiltro`** en `ObtenerListaTareas.ashx.cs`.

- [ ] **Paso 1: Handler JSON**

`ReporteTareas/Formulario/AdministrarCalificacionForeCast.ashx`:

```
<%@ WebHandler Language="C#" CodeBehind="AdministrarCalificacionForeCast.ashx.cs" Class="JsonJQueryNetCalificacionForeCast.AdministrarCalificacionForeCast" %>
```

`ReporteTareas/Formulario/AdministrarCalificacionForeCast.ashx.cs` (UTF-8 con BOM):

```csharp
using CapaEntidad;
using CapaNegocio;
using SeguridadAppHelper;
using System;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetCalificacionForeCast
{
    /// <summary>
    /// Handler de la pantalla "Calificacion ForeCast".
    /// Acciones: ListaCalificacionForeCast, GuardarCalificacionForeCast.
    ///
    /// El usuario sale SIEMPRE de "session" (Cod_Usuario encriptado) y nunca de
    /// un parametro en claro: es lo que decide la visibilidad en Sp_RTAListaForeCast.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarCalificacionForeCast : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();

                JavaScriptSerializer i = new JavaScriptSerializer();
                dynamic parametros = i.Deserialize(inputJson.ToString(), typeof(object));

                var parameters = parametros[0]["parameters"];
                var Action = parametros[0]["action"];
                bool existAction = false;

                if (Action == "ListaCalificacionForeCast")
                {
                    existAction = true;
                    responseAction.Append(Lista(parameters));
                }

                if (Action == "GuardarCalificacionForeCast")
                {
                    existAction = true;
                    responseAction.Append(Guardar(parameters));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }

            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        private string Lista(dynamic campos)
        {
            try
            {
                string idUsuario = Usuario(campos);
                EntFiltroCalificacion filtro = new EntFiltroCalificacion
                {
                    GerentedeCuenta = Campo(campos, "gerente"),
                    Sucursal = Campo(campos, "sucursal"),
                    Prioridad = Campo(campos, "etapa"),
                    Estado = Campo(campos, "estado"),
                    Marca = Campo(campos, "marca")
                };

                EntPantallaCalificacion pantalla = NegCalificacionForeCast.Cargar(idUsuario, filtro);
                return ToJson(new EntRespuesta { estado = "1", resultado = pantalla, mensaje = "", tipoMensaje = "success" });
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar la calificación. " + ex.Message, "danger");
            }
        }

        private string Guardar(dynamic campos)
        {
            try
            {
                string idUsuario = Usuario(campos);

                int presupuesto, autoridad, necesidad;
                long idForeCast;
                if (!long.TryParse(Campo(campos, "idForeCast"), out idForeCast)
                    || !int.TryParse(Campo(campos, "presupuesto"), out presupuesto)
                    || !int.TryParse(Campo(campos, "autoridad"), out autoridad)
                    || !int.TryParse(Campo(campos, "necesidad"), out necesidad))
                {
                    return responseMessage("0", "Responda Presupuesto, Autoridad y Necesidad.", "warning");
                }

                DateTime fecha;
                DateTime? fechaAccion = null;
                if (DateTime.TryParseExact(Campo(campos, "fechaAccion"), "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                {
                    fechaAccion = fecha;
                }

                EntCalificacionRespuesta r = new EntCalificacionRespuesta
                {
                    IdForeCast = idForeCast,
                    Presupuesto = presupuesto,
                    Autoridad = autoridad,
                    Necesidad = necesidad,
                    DecisorCargo = Campo(campos, "decisorCargo"),
                    PropuestaEnviada = Campo(campos, "propuestaEnviada") == "1",
                    AccionComprometida = Campo(campos, "accionComprometida"),
                    FechaAccion = fechaAccion
                };

                return ToJson(NegCalificacionForeCast.Guardar(r, idUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar la calificación. " + ex.Message, "danger");
            }
        }

        private static string Usuario(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            return seguridad.Desencripta(Campo(campos, "session"));
        }

        private static string Campo(dynamic campos, string nombre)
        {
            try
            {
                object v = campos[nombre];
                return v == null ? "" : Convert.ToString(v).Trim();
            }
            catch
            {
                return "";
            }
        }

        private string responseMessage(string estado, string mensaje, string tipoMensaje, string resultado = "")
        {
            return ToJson(new EntRespuesta { estado = estado, mensaje = mensaje, tipoMensaje = tipoMensaje, resultado = resultado });
        }

        /// <summary>
        /// JavaScriptSerializer escapa las tildes como \uXXXX: evita el problema
        /// de codificacion del helper compartido (Encoding.Default).
        /// </summary>
        private static string ToJson(object obj)
        {
            if (obj == null) { return string.Empty; }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return serializer.Serialize(obj);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
```

- [ ] **Paso 2: Handler de descarga**

`ReporteTareas/Formulario/DescargarCalificacionForeCast.ashx`:

```
<%@ WebHandler Language="C#" CodeBehind="DescargarCalificacionForeCast.ashx.cs" Class="JsonJQueryNetCalificacionForeCast.DescargarCalificacionForeCast" %>
```

`ReporteTareas/Formulario/DescargarCalificacionForeCast.ashx.cs` (UTF-8 con BOM):

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Web;

namespace JsonJQueryNetCalificacionForeCast
{
    /// <summary>
    /// La tabla de la calificacion en Excel. Handler aparte porque es un GET que
    /// escribe bytes, como DescargarHorasExtras.ashx.
    ///
    /// IRequiresSessionState no es decorativo: el usuario sale de la sesion
    /// (Cod_Usuario), no de la URL, y sin el context.Session es null.
    ///
    /// El Response.End() queda FUERA del try: End() lanza ThreadAbortException
    /// y un catch alrededor intentaria escribir con la respuesta ya enviada.
    /// </summary>
    public class DescargarCalificacionForeCast : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Session == null || context.Session["UserLogin"] == null || context.Session["Cod_Usuario"] == null)
            {
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.", 401);
                return;
            }

            string idUsuario = Convert.ToString(context.Session["Cod_Usuario"]).Trim();
            EntFiltroCalificacion filtro = new EntFiltroCalificacion
            {
                GerentedeCuenta = Param(context, "gerente"),
                Sucursal = Param(context, "sucursal"),
                Prioridad = Param(context, "etapa"),
                Estado = Param(context, "estado"),
                Marca = Param(context, "marca")
            };

            byte[] archivo;
            try
            {
                EntPantallaCalificacion pantalla = NegCalificacionForeCast.Cargar(idUsuario, filtro);
                archivo = NegCalificacionExportacion.Construir(pantalla);
            }
            catch (Exception ex)
            {
                NoDisponible(context, "No se pudo generar el archivo. " + ex.Message, 500);
                return;
            }

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.AddHeader("Content-Disposition",
                "attachment; filename=" + NegCalificacionExportacion.NombreDeArchivo(DateTime.Today));
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.BinaryWrite(archivo);
            context.Response.End();
        }

        private static string Param(HttpContext context, string nombre)
        {
            return (context.Request.QueryString[nombre] ?? "").Trim();
        }

        private static void NoDisponible(HttpContext context, string mensaje, int statusCode)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(mensaje);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
```

- [ ] **Paso 3: Registrar en `ReporteTareas/ReporteTareas.csproj`**

Junto a `<Content Include="Formulario\AdministrarHorarioUsuario.ashx" />`:

```xml
    <Content Include="Formulario\AdministrarCalificacionForeCast.ashx" />
    <Content Include="Formulario\DescargarCalificacionForeCast.ashx" />
```

Junto a `<Compile Include="Formulario\AdministrarHorarioUsuario.ashx.cs">`:

```xml
    <Compile Include="Formulario\AdministrarCalificacionForeCast.ashx.cs">
      <DependentUpon>AdministrarCalificacionForeCast.ashx</DependentUpon>
    </Compile>
    <Compile Include="Formulario\DescargarCalificacionForeCast.ashx.cs">
      <DependentUpon>DescargarCalificacionForeCast.ashx</DependentUpon>
    </Compile>
```

- [ ] **Paso 4: Compilar y correr las pruebas**

Expected: compila sin errores y todas las pruebas pasan.

- [ ] **Paso 5: Commit**

```bash
git add ReporteTareas/Formulario/AdministrarCalificacionForeCast.ashx ReporteTareas/Formulario/AdministrarCalificacionForeCast.ashx.cs ReporteTareas/Formulario/DescargarCalificacionForeCast.ashx ReporteTareas/Formulario/DescargarCalificacionForeCast.ashx.cs ReporteTareas/ReporteTareas.csproj
git commit -m "feat(forecast): handlers de la calificacion (listar, guardar, descargar)"
```

---

### Task 7: La pantalla y el modal

**Archivos:**
- Crear: `ReporteTareas/Formulario/CalificacionForeCast.aspx`, `.aspx.cs` y `.aspx.designer.cs`
- Crear: `ReporteTareas/js/CalificacionForeCast.js`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consume: las acciones de la Task 6 y el JSON de `EntPantallaCalificacion`, cuyos nombres de propiedad son los de la Task 2.
- Produce: la página en `Formulario/CalificacionForeCast.aspx`, que es el `Href` del menú de la Task 1.

- [ ] **Paso 1: La página** (`CalificacionForeCast.aspx`, UTF-8 **con BOM**)

```aspx
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CalificacionForeCast.aspx.cs" Inherits="ReporteTareas.Formulario.CalificacionForeCast" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .cf-tarjeta { border: 1px solid #ddd; border-radius: 4px; padding: 10px; text-align: center; margin-bottom: 10px; }
        .cf-tarjeta .cf-valor { font-size: 22px; font-weight: bold; }
        .cf-estado { padding: 2px 8px; border-radius: 10px; font-weight: bold; white-space: nowrap; }
        .cf-Caliente { background: #f8cbad; }
        .cf-Tibia { background: #ffe699; }
        .cf-Fria { background: #bdd7ee; }
        .cf-sin { font-size: 11px; color: #a94442; display: block; }
        .cf-detalle td { vertical-align: top; }
    </style>
    <script src="../js/CalificacionForeCast.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Calificación de oportunidades</h3>
                    </div>
                </div>
            </div>
        </div>

        <div style="display: none">
            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">Filtros</div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-2"><label>Gerente de cuenta</label><select id="cboGerente" class="form-control"></select></div>
                        <div class="form-group col-lg-2"><label>Sucursal</label><select id="cboSucursal" class="form-control"></select></div>
                        <div class="form-group col-lg-2"><label>Etapa</label><select id="cboEtapa" class="form-control"></select></div>
                        <div class="form-group col-lg-2"><label>Estado</label>
                            <select id="cboEstado" class="form-control">
                                <option value="">Todos</option>
                                <option value="Caliente">Caliente</option>
                                <option value="Tibia">Tibia</option>
                                <option value="Fría">Fría</option>
                                <option value="Sin calificar">Sin calificar</option>
                            </select>
                        </div>
                        <div class="form-group col-lg-2"><label>Marca</label><select id="cboMarca" class="form-control"></select></div>
                        <div class="form-group col-lg-2" style="padding-top: 25px">
                            <button type="button" class="btn btn-primary" onclick="Cargar()">Buscar</button>
                            <button type="button" class="btn btn-success" onclick="Descargar()"><i class="fa fa-file-excel-o"></i> Excel</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div class="row">
                <div class="col-lg-2"><div class="cf-tarjeta"><div>Abiertas</div><div class="cf-valor" id="indTotal">-</div></div></div>
                <div class="col-lg-2"><div class="cf-tarjeta"><div>Monto total</div><div class="cf-valor" id="indMonto">-</div></div></div>
                <div class="col-lg-2"><div class="cf-tarjeta"><div>Monto ponderado</div><div class="cf-valor" id="indPonderado">-</div></div></div>
                <div class="col-lg-4"><div class="cf-tarjeta"><div>Caliente · Tibia · Fría</div><div class="cf-valor" id="indEstados">-</div></div></div>
                <div class="col-lg-2"><div class="cf-tarjeta"><div>Sin calificar</div><div class="cf-valor" id="indSinCalificar">-</div></div></div>
            </div>
            <div class="row">
                <div class="col-lg-6">
                    <div class="panel panel-success"><div class="panel-heading">Top 5 de la semana</div><div class="panel-body"><ol id="lstTop5"></ol></div></div>
                </div>
                <div class="col-lg-6">
                    <div class="panel panel-danger"><div class="panel-heading">En riesgo</div><div class="panel-body"><ul id="lstRiesgo"></ul></div></div>
                </div>
            </div>
            <div class="panel panel-default">
                <div class="panel-heading">Oportunidades (clic en una fila para ver el desglose)</div>
                <div class="panel-body" style="overflow-x: auto">
                    <table id="tblCalificacion" class="table table-striped table-bordered" style="width: 100%">
                        <thead>
                            <tr><th>Cliente</th><th>Oportunidad</th><th>Etapa</th><th>Monto</th><th>Puntaje</th><th>Estado</th><th>Alertas</th><th></th></tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="modalCalificar" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">Calificar: <span id="lblOportunidad"></span></h4>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="hidIdForeCast" />
                    <div class="form-group"><label>¿En qué estado está el presupuesto del cliente?</label>
                        <select id="cboPresupuesto" class="form-control">
                            <option value="">Seleccione…</option>
                            <option value="20">Aprobado o asignado (partida, PAC u OC)</option>
                            <option value="12">En trámite (en POA/PAC, pendiente de aprobación)</option>
                            <option value="5">Solo lo estimamos nosotros</option>
                            <option value="0">No tiene / no sabemos</option>
                        </select>
                    </div>
                    <div class="form-group"><label>¿Con quién estamos hablando?</label>
                        <select id="cboAutoridad" class="form-control">
                            <option value="">Seleccione…</option>
                            <option value="20">Decisor final (gerente, director, rectorado con firma)</option>
                            <option value="12">Influenciador con acceso al decisor</option>
                            <option value="5">Solo usuario técnico</option>
                            <option value="0">Sin contacto</option>
                        </select>
                    </div>
                    <div class="form-group"><label>¿Qué tan clara es la necesidad?</label>
                        <select id="cboNecesidad" class="form-control">
                            <option value="">Seleccione…</option>
                            <option value="20">Crítica y con fecha (vence soporte, EOSL, proceso o TDR en curso)</option>
                            <option value="14">Clara y reconocida por el cliente</option>
                            <option value="6">Interés exploratorio</option>
                            <option value="0">No definida</option>
                        </select>
                    </div>
                    <div class="form-group"><label>Nombre y cargo del decisor</label>
                        <input type="text" id="txtDecisor" class="form-control" maxlength="150" /></div>
                    <div class="checkbox"><label><input type="checkbox" id="chkPropuesta" /> Propuesta enviada</label></div>
                    <div class="form-group"><label>Próxima acción comprometida</label>
                        <input type="text" id="txtAccion" class="form-control" maxlength="300" /></div>
                    <div class="form-group"><label>Fecha de la acción</label>
                        <input type="date" id="txtFechaAccion" class="form-control" /></div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                    <button type="button" class="btn btn-primary" id="btnGuardar" onclick="Guardar()">Guardar</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
```

- [ ] **Paso 2: El código de la página** (`CalificacionForeCast.aspx.cs`, UTF-8 con BOM)

```csharp
using ReporteTareas.Controles;
using SeguridadAppHelper;
using System;

namespace ReporteTareas.Formulario
{
    public partial class CalificacionForeCast : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (Session["UserLogin"] != null && !IsPostBack)
            {
                try
                {
                    SeguridadHelper seguridad = new SeguridadHelper();
                    txtUsuario.Text = seguridad.Encripta(Session["Cod_Usuario"].ToString());
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
```

`CalificacionForeCast.aspx.designer.cs` (UTF-8 con BOM):

```csharp
namespace ReporteTareas.Formulario
{
    public partial class CalificacionForeCast
    {
        /// <summary>Cod_Usuario encriptado; el handler lo desencripta.</summary>
        protected global::System.Web.UI.WebControls.TextBox txtUsuario;
    }
}
```

- [ ] **Paso 3: El JavaScript** (`ReporteTareas/js/CalificacionForeCast.js`, UTF-8 **con BOM**)

```javascript
/* ==========================================================================
   Calificacion ForeCast
   Handler : AdministrarCalificacionForeCast.ashx  (listar, guardar)
             DescargarCalificacionForeCast.ashx    (Excel)
   Este archivo NO calcula nada: puntajes, estados, alertas y acciones llegan
   resueltos de NegCalificacionForeCast. Solo dibuja.
   ========================================================================== */

var cfPantalla = null;
var cfTabla = null;

$(document).ready(function () {
    Cargar();
    $('#tblCalificacion tbody').on('click', 'tr.cf-fila', function (e) {
        if ($(e.target).closest('button').length) { return; }
        AlternarDetalle(this);
    });
});

function Sesion() {
    return $("#ContentPlaceHolder1_txtUsuario").val();
}

function Filtros() {
    return {
        gerente: $('#cboGerente').val() || '',
        sucursal: $('#cboSucursal').val() || '',
        etapa: $('#cboEtapa').val() || '',
        estado: $('#cboEstado').val() || '',
        marca: $('#cboMarca').val() || ''
    };
}

function Llamar(action, parameters, alTerminar) {
    $.ajax({
        type: "POST",
        url: "AdministrarCalificacionForeCast.ashx",
        data: JSON.stringify([{ "action": action, "parameters": parameters }]),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) { alTerminar(r); },
        error: function () { swal("Error", "No se pudo comunicar con el servidor.", "error"); }
    });
}

function Cargar() {
    var p = Filtros();
    p.session = Sesion();
    Llamar("ListaCalificacionForeCast", p, function (r) {
        if (!r || r.estado !== "1") {
            swal("Error", (r && r.mensaje) || "No se pudo cargar la calificación.", "error");
            return;
        }
        cfPantalla = r.resultado;
        LlenarCombo('#cboGerente', cfPantalla.Gerentes);
        LlenarCombo('#cboSucursal', cfPantalla.Sucursales);
        LlenarCombo('#cboEtapa', cfPantalla.Etapas);
        LlenarCombo('#cboMarca', cfPantalla.Marcas);
        DibujarIndicadores();
        DibujarListas();
        DibujarTabla();
    });
}

/* Conserva lo elegido: los combos llegan del conjunto sin filtrar. */
function LlenarCombo(id, valores) {
    var combo = $(id), actual = combo.val() || '';
    combo.empty().append($('<option>').val('').text('Todos'));
    $.each(valores || [], function (i, v) { combo.append($('<option>').val(v).text(v)); });
    combo.val(actual);
}

function Dinero(n) {
    var v = Math.round(Number(n) || 0).toString();
    return '$' + v.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}

function Html(s) {
    return $('<div>').text(s == null ? '' : String(s)).html();
}

function ClaseEstado(estado) {
    return estado === 'Fría' ? 'cf-Fria' : 'cf-' + estado;
}

function DibujarIndicadores() {
    $('#indTotal').text(cfPantalla.Total);
    $('#indMonto').text(Dinero(cfPantalla.MontoTotal));
    $('#indPonderado').text(Dinero(cfPantalla.MontoPonderado));
    $('#indEstados').text(cfPantalla.Calientes + ' · ' + cfPantalla.Tibias + ' · ' + cfPantalla.Frias);
    $('#indSinCalificar').text(String(cfPantalla.PorcentajeSinCalificar).replace('.', ',') + ' %');
}

function Buscar(id) {
    for (var i = 0; i < cfPantalla.Oportunidades.length; i++) {
        if (cfPantalla.Oportunidades[i].IdForeCast === id) { return cfPantalla.Oportunidades[i]; }
    }
    return null;
}

function DibujarListas() {
    var top = $('#lstTop5').empty(), riesgo = $('#lstRiesgo').empty();
    $.each(cfPantalla.Top5, function (i, id) {
        var o = Buscar(id);
        if (o) { top.append('<li><b>' + Html(o.Cliente) + '</b> – ' + Html(o.DetalleProyecto) + ' (' + Dinero(o.MontoPonderado) + ' pond.) · ' + Html(o.AccionSugerida) + '</li>'); }
    });
    $.each(cfPantalla.EnRiesgo, function (i, id) {
        var o = Buscar(id);
        if (o && o.Alertas.length) { riesgo.append('<li><b>' + Html(o.Cliente) + '</b> – ' + Html(o.DetalleProyecto) + ': ' + Html(o.Alertas[0].Texto) + '</li>'); }
    });
    if (!cfPantalla.Top5.length) { top.append('<li>Sin oportunidades calientes o tibias.</li>'); }
    if (!cfPantalla.EnRiesgo.length) { riesgo.append('<li>Ninguna.</li>'); }
}

function DibujarTabla() {
    if (cfTabla) { cfTabla.destroy(); cfTabla = null; }
    var cuerpo = $('#tblCalificacion tbody').empty();

    $.each(cfPantalla.Oportunidades, function (i, o) {
        var estado = '<span class="cf-estado ' + ClaseEstado(o.Estado) + '">' + Html(o.Estado) + '</span>'
            + (o.SinCalificar ? '<span class="cf-sin">Sin calificar</span>' : '');
        cuerpo.append(
            '<tr class="cf-fila" data-id="' + o.IdForeCast + '">'
            + '<td>' + Html(o.Cliente) + '</td>'
            + '<td>' + Html(o.DetalleProyecto) + '</td>'
            + '<td>' + Html(o.Prioridad) + '</td>'
            + '<td data-order="' + (o.PVPEstimado || 0) + '">' + Dinero(o.PVPEstimado) + '</td>'
            + '<td>' + o.Puntaje + '</td>'
            + '<td>' + estado + '</td>'
            + '<td>' + o.Alertas.length + '</td>'
            + '<td><button type="button" class="btn btn-xs btn-primary" onclick="AbrirModal(' + o.IdForeCast + ')">Calificar</button></td>'
            + '</tr>');
    });

    cfTabla = $('#tblCalificacion').DataTable({
        order: [[4, 'desc']],
        pageLength: 25,
        language: { url: "https://cdn.datatables.net/plug-ins/1.10.15/i18n/Spanish.json" }
    });
}

function AlternarDetalle(tr) {
    var fila = cfTabla.row(tr);
    if (fila.child.isShown()) { fila.child.hide(); return; }
    var o = Buscar(Number($(tr).data('id')));
    if (!o) { return; }

    var alertas = $.map(o.Alertas, function (a) { return '<li>' + Html(a.Texto) + '</li>'; }).join('') || '<li>Ninguna</li>';
    var comprometida = o.Calificacion && o.Calificacion.AccionComprometida
        ? Html(o.Calificacion.AccionComprometida) + (o.FechaAccionTexto ? ' (' + o.FechaAccionTexto + ')' : '')
        : '—';

    fila.child(
        '<table class="table table-condensed cf-detalle"><tr>'
        + '<td><b>Desglose</b><br/>Presupuesto ' + o.Presupuesto + '/20 · Autoridad ' + o.Autoridad + '/20 · Necesidad ' + o.Necesidad + '/20<br/>'
        + 'Tiempo ' + o.Tiempo + '/15 · Actividad ' + o.Actividad + '/15 · Encaje ' + o.Encaje + '/10<br/>'
        + 'Cierre: ' + (o.FechaCierreTexto || '—') + ' · Última actividad: ' + (o.UltimaActividadTexto || '—') + '<br/>'
        + 'Ponderado: ' + Dinero(o.MontoPonderado) + ' (' + Html(o.DescripcionNegocio) + ')</td>'
        + '<td><b>Alertas</b><ul>' + alertas + '</ul></td>'
        + '<td><b>Datos faltantes</b><br/>' + (Html(o.Faltantes.join(', ')) || '—') + '</td>'
        + '<td><b>Acción sugerida</b><br/>' + Html(o.AccionSugerida) + '<br/><b>Acción comprometida</b><br/>' + comprometida + '</td>'
        + '</tr></table>').show();
}

function AbrirModal(id) {
    var o = Buscar(id);
    if (!o) { return; }
    var c = o.Calificacion;
    $('#hidIdForeCast').val(id);
    $('#lblOportunidad').text(o.Cliente + ' – ' + o.DetalleProyecto);
    $('#cboPresupuesto').val(c ? String(c.Presupuesto) : '');
    $('#cboAutoridad').val(c ? String(c.Autoridad) : '');
    $('#cboNecesidad').val(c ? String(c.Necesidad) : '');
    $('#txtDecisor').val(c ? c.DecisorCargo : '');
    $('#chkPropuesta').prop('checked', c ? c.PropuestaEnviada : false);
    $('#txtAccion').val(c ? c.AccionComprometida : '');
    $('#txtFechaAccion').val(c && o.FechaAccionTexto ? o.FechaAccionTexto.split('/').reverse().join('-') : '');
    $('#modalCalificar').modal('show');
}

function Guardar() {
    if ($('#cboPresupuesto').val() === '' || $('#cboAutoridad').val() === '' || $('#cboNecesidad').val() === '') {
        swal("Faltan respuestas", "Responda Presupuesto, Autoridad y Necesidad.", "warning");
        return;
    }
    $('#btnGuardar').prop('disabled', true);
    Llamar("GuardarCalificacionForeCast", {
        session: Sesion(),
        idForeCast: $('#hidIdForeCast').val(),
        presupuesto: $('#cboPresupuesto').val(),
        autoridad: $('#cboAutoridad').val(),
        necesidad: $('#cboNecesidad').val(),
        decisorCargo: $('#txtDecisor').val(),
        propuestaEnviada: $('#chkPropuesta').is(':checked') ? '1' : '0',
        accionComprometida: $('#txtAccion').val(),
        fechaAccion: $('#txtFechaAccion').val() || ''
    }, function (r) {
        $('#btnGuardar').prop('disabled', false);
        if (r && r.estado === "1") {
            $('#modalCalificar').modal('hide');
            swal("Listo", "Calificación guardada.", "success");
            Cargar();   // recalcula en el servidor; el navegador no calcula
        } else {
            swal("No se guardó", (r && r.mensaje) || "Intente de nuevo.", "warning");
        }
    });
}

function Descargar() {
    window.location = "DescargarCalificacionForeCast.ashx?" + $.param(Filtros());
}
```

> Nota sobre «recalcula la fila sin recargar la página» (spec): se vuelve a pedir la pantalla al servidor con `Cargar()`, no se recarga la página. Así el puntaje nunca se calcula en el navegador.

- [ ] **Paso 4: Registrar en `ReporteTareas/ReporteTareas.csproj`**

Junto a `<Content Include="Formulario\ParametrizacionHorarioUsuario.aspx" />`:

```xml
    <Content Include="Formulario\CalificacionForeCast.aspx" />
```

Junto a `<Content Include="js\parametrizacionHorarioUsuario.js" />`:

```xml
    <Content Include="js\CalificacionForeCast.js" />
```

Junto a `<Compile Include="Formulario\ParametrizacionHorarioUsuario.aspx.cs">`:

```xml
    <Compile Include="Formulario\CalificacionForeCast.aspx.cs">
      <DependentUpon>CalificacionForeCast.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\CalificacionForeCast.aspx.designer.cs">
      <DependentUpon>CalificacionForeCast.aspx</DependentUpon>
    </Compile>
```

- [ ] **Paso 5: Compilar y correr las pruebas**

Expected: compila y todas las pruebas pasan.

- [ ] **Paso 6: Probar en el navegador** (IIS Express desde Visual Studio, o el sitio de desarrollo). Iniciar sesión con un usuario de perfil 18 y abrir *Forecacst → Calificación ForeCast*. Comprobar:
1. **Visibilidad:** el total de «Abiertas» coincide con las oportunidades LEAD/PIPELINE/UPSIDE/FORECAST que ese usuario ve en *Registro ForeCast*.
2. **Filtros:** Sucursal y Estado = «Sin calificar» cambian la tabla y las tarjetas, y los combos conservan todas las opciones.
3. **Desglose:** clic en una fila despliega los seis criterios, las alertas, los faltantes y las acciones.
4. **Guardar:** calificar una oportunidad devuelve el mensaje de éxito, y la fila cambia de puntaje y ya no dice «Sin calificar».
5. **Validación:** guardar con una pregunta sin responder muestra «Faltan respuestas» y no llama al servidor.
6. **Excel:** descarga el archivo con las mismas filas que la tabla filtrada.
7. **Tildes:** «Fría», «Calificación» y los nombres con tilde se ven bien en la pantalla y en el Excel.

- [ ] **Paso 7: Commit**

```bash
git add ReporteTareas/Formulario/CalificacionForeCast.aspx ReporteTareas/Formulario/CalificacionForeCast.aspx.cs ReporteTareas/Formulario/CalificacionForeCast.aspx.designer.cs ReporteTareas/js/CalificacionForeCast.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(forecast): pantalla de calificacion de oportunidades"
```

---

### Task 8: Despliegue

**Archivos:**
- Modificar: `DESPLIEGUE.md` (una sección nueva al final, con el número siguiente al último)

- [ ] **Paso 1: Agregar la sección** (ajustar `11` si el último número ya es otro)

```markdown
## 11. Calificación de oportunidades del Forecast

### 11.1 Base de datos

`docs/sql/2026-09-28-calificacion-forecast.sql`: crea la tabla `ForeCastCalificacion`, los
procedimientos `Sp_RTAListaCalificacionForeCast` y `Sp_RTAInsertaCalificacionForeCast`, y la
opción de menú bajo *Forecacst*, para los mismos perfiles que ven *Registro ForeCast*.
Idempotente. **No toca `ForeCast`.** Va **antes** de los binarios.

Verificar con `docs/sql/2026-09-28-calificacion-forecast-verificacion.sql`, que no deja datos.

> **Si se cambia `Sp_RTAListaForeCast`:** `Sp_RTAListaCalificacionForeCast` replica sus
> 40 columnas en una tabla temporal (`INSERT … EXEC`). Si se le agrega una columna, hay que
> agregarla también en `#Visibles`, o la pantalla de calificación deja de cargar.

### 11.2 Archivos a publicar

- `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe`, `bin\CapaDato.exe`
- `Formulario\CalificacionForeCast.aspx`
- `Formulario\AdministrarCalificacionForeCast.ashx`
- `Formulario\DescargarCalificacionForeCast.ashx`
- `js\CalificacionForeCast.js`

Los tres de capa son `.exe`, no `.dll`.

### 11.3 Verificar después de publicar

1. Con un usuario que tenga filtro de sucursal, el número de «Abiertas» coincide con lo que ve en *Registro ForeCast*.
2. Calificar una oportunidad y comprobar que la fila cambia de puntaje.
3. Descargar el Excel.
```

- [ ] **Paso 2: Verificación final**

Run:
```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```
Expected: compila y todas las pruebas pasan. Revisar también que `git status` no muestre archivos nuevos sin registrar en un `.csproj`.

- [ ] **Paso 3: Commit**

```bash
git add DESPLIEGUE.md
git commit -m "docs(forecast): despliegue de la calificacion de oportunidades"
```
