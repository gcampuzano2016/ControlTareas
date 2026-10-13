# Horas Extras — Fase 2: la pantalla

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que Nómina pueda abrir un período mensual, digitar horas al 50% y al 100% de los 64 colaboradores, y ver el valor a pagar — reemplazando el Excel.

**Architecture:** La pantalla es `HorasExtras.aspx` con el molde de la casa (`Master.Master`, Bootstrap 3, un `.ashx` que recibe `[{"action":"...","parameters":{...}}]`). **El cálculo existe una sola vez, en `NegHorasExtras` (C#).** El SQL no calcula: guarda lo que C# le da. Al guardar, el servidor recalcula, ignora los números del cliente y **devuelve los suyos para que la grilla se repinte** — si el cliente calculó distinto, el usuario ve el número saltar y la divergencia se vuelve visible en vez de silenciosa.

**Tech Stack:** ASP.NET WebForms, .NET Framework 4.8 (web) / **4.6.1 (capas)**, SQL Server, jQuery plano, Bootstrap 3, MSTest v1.

**Spec:** `docs/superpowers/specs/2026-09-15-horas-extras-design.md` (autoridad vinculante). Especificación funcional de origen: `Actualizacion/ESPEC_MODULO_HORAS_EXTRAS.md` (fuera de git a propósito; se puede leer).

**Estado de partida:** fase 1 **en producción** desde el 2026-09-16. Las seis tablas existen, hay 64 colaboradores, 69 sueldos y 7 parámetros. `CapaNegocio/NegHorasExtras.cs` es la clase pura de cálculo y tiene 128 pruebas verdes. **No existe todavía ningún DAO, ninguna pantalla ni ningún handler del módulo.**

---

## Global Constraints

- **`.NET Framework 4.6.1` en `CapaEntidad`, `CapaDato`, `CapaNegocio` y `CapaPruebas`:** nada de `record`, `init`, `switch` de expresión, ni interpolación con `$` en esos proyectos si el compilador la rechaza. La web es 4.8.
- **`decimal`, nunca `double`,** para dinero y horas. Redondeo `MidpointRounding.AwayFromZero`.
- **Las capas van `CapaNegocio → CapaDato → CapaEntidad`.** `CapaDato` **no puede** referenciar `CapaNegocio`: es un ciclo y no compila.
- **Sin tildes ni eñes en los archivos `.sql` versionados.** El servidor los sirve en windows-1252 y se rompen. Los comentarios de C# tampoco las llevan. **Los textos que ve el usuario en `.aspx` y `.js` SÍ van con tildes correctas.**
- **`HorasExtras.aspx` se guarda en UTF-8 CON BOM.** `Web.config:39` declara `requestEncoding="windows-1252"` y no trae `fileEncoding`: una `.aspx` sin BOM sale con caracteres raros en producción. Ya pasó con `MiPerfil.aspx`.
- **Parámetros SQL tipados con `SqlDbType` explícito**, nunca `AddWithValue`.
- **La identidad sale SIEMPRE de `context.Session`.** Nunca de un campo que mande el cliente. El handler declara `IRequiresSessionState` — sin eso `context.Session` es `null` en un `IHttpHandler`.
- **Las tablas no llevan `bg-primary` ni `style="font-size:..."`.** `css/dos-tema.css` ya diseña las tablas del sistema.
- **El repositorio de GitHub es PÚBLICO.** Ningún dato personal en archivos versionados, en comentarios ni en mensajes de error.
- Compilar con el MSBuild de **VS2019**: `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`. El del PATH falla con errores que despistan.

## Decisiones que ya están tomadas y NO se reabren

1. **Los perfiles que ven la pantalla son el 14 (Talento Humano) y el 18 (Super Admin).** Decidido por el usuario el 2026-09-16. Verificado en producción: el 14 es quien ve hoy `RRHHEmpleados.aspx` y `ListaEmpleados.aspx`.
2. **Sin aviso de topes en esta fase.** Decidido por el usuario el 2026-09-16. `TopeDiario50` y `TopeSemanal50` están cargados y no se usan: el módulo guarda un total mensual y no hay días ni semanas contra los que comparar. El umbral mensual lo define RRHH y el bloqueo real es al cerrar, que es fase 3.
3. **Sin autoguardado** (decisión 9 del diseño), aunque el §5.4 funcional lo pida. Salir con cambios sin guardar sí dispara confirmación.
4. **Sin botón "Cerrar período" ni "Exportar a Excel".** Son fase 3.
5. **El cálculo NO se reimplementa en SQL.** `docs/sql/2026-09-15-horas-extras-verificacion.sql` tiene una CTE que replica el desempate de salario: **es una consulta de verificación cruzada, no un camino de producción.** No la reutilices como origen de datos de la pantalla.
6. **`Observacion` se guarda pero no se audita en esta fase.** `HE_DetalleAuditoria.ValorAnterior`/`ValorNuevo` son `DECIMAL(9,2)` y no pueden guardar texto. La auditoría es fase 3.

---

## Estructura de archivos

| Archivo | Responsabilidad |
|---|---|
| `CapaEntidad/EntHePeriodo.cs` | La cabecera del período: año, mes, estado |
| `CapaEntidad/EntHeFila.cs` | Una fila de la grilla: el snapshot más las horas más lo calculado |
| `CapaEntidad/EntHePantalla.cs` | Lo que la pantalla necesita de una sola ida: el período, sus filas y los totales |
| `CapaDato/DaoHorasExtras.cs` | Acceso a datos del módulo. No calcula nada |
| `CapaNegocio/NegHorasExtrasPantalla.cs` | La orquestación: abrir un período, cargarlo, y guardar recalculando |
| `CapaPruebas/NegHorasExtrasPantallaTests.cs` | Pruebas de la orquestación con dobles, sin base |
| `docs/sql/2026-09-16-horas-extras-fase2.sql` | Los cinco procedimientos |
| `docs/sql/2026-09-16-horas-extras-fase2-menu.sql` | El registro en `MenuDos` + `PerfilMenu` |
| `ReporteTareas/Formulario/HorasExtras.aspx` (+ `.cs`, `.designer.cs`) | La pantalla |
| `ReporteTareas/Formulario/AdministrarHorasExtras.ashx` (+ `.cs`) | El handler JSON |
| `ReporteTareas/js/horasExtras.js` | La grilla, el tablero y el repintado |

**`NegHorasExtras.cs` no se toca.** Es la clase pura de cálculo y ya está probada. La orquestación va en un archivo nuevo para que la clase pura siga sin saber que existe una base de datos.

---

## Task 1: El cargo, que la fase 1 no cargó

**Files:**
- Create: `docs/sql/2026-09-16-horas-extras-cargo.sql`
- Modify: `docs/sql/generar-carga-horas-extras.py`

**Por qué existe esta tarea.** La grilla del §5.3 tiene una columna `Cargo` y `HE_Detalle` tiene `CargoSnapshot`, pero **el cargo no está en ninguna parte de donde la pantalla pueda leerlo**. Verificado contra producción el 2026-09-16:

- `dbo.Empleados` tiene 22 columnas y **ninguna es `Cargo`**.
- `dbo.R_Usuarios` sí tiene `Cargo`, pero llegar ahí exige unir por cédula, y **una cédula la comparten seis usuarios activos**. Es exactamente la ambigüedad que el diseño evitó colgando el módulo de `Empleados`. No se usa.
- La hoja `Colaboradores` de la plantilla **sí trae `Cargo`, lleno en las 64 filas**, con un máximo de 28 caracteres y 6 con tildes. La fase 1 no lo cargó porque `HE_ColaboradorParametro` no tenía dónde ponerlo.

Así que el dato existe, RRHH ya lo entregó, y sólo falta darle destino. Sin esta tarea, la tarea 2 no tiene de dónde sacar el cargo y la columna saldría vacía.

**Interfaces:**
- Produces: `HE_ColaboradorParametro.Cargo VARCHAR(200) NULL`, cargado para los 64.

- [ ] **Step 1: El script que añade la columna**

`docs/sql/2026-09-16-horas-extras-cargo.sql`. La tabla **ya existe en producción con datos**, así que aquí no sirve el `IF OBJECT_ID(...) IS NULL` de la fase 1: hay que preguntar por la columna.

```sql
/* ============================================================================
   El cargo del colaborador.

   La grilla de la fase 2 lo muestra y HE_Detalle tiene CargoSnapshot, pero no
   habia de donde leerlo: Empleados no tiene esa columna, y la de R_Usuarios
   exige unir por cedula -que seis usuarios activos comparten-. La plantilla si
   lo trae, lleno en las 64 filas, asi que se le da destino aqui.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @Fallos INT = 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_ColaboradorParametro') AND name = 'Cargo')
BEGIN
    ALTER TABLE dbo.HE_ColaboradorParametro ADD Cargo VARCHAR(200) NULL;
    PRINT 'Columna Cargo agregada.';
END
ELSE
    PRINT 'Columna Cargo ya existia.';

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_ColaboradorParametro') AND name = 'Cargo')
BEGIN
    RAISERROR('FALLO: la columna Cargo no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF @Fallos > 0
    RAISERROR('FALLO: %d verificaciones no pasaron.', 16, 1, @Fallos);
ELSE
    PRINT 'Horas Extras: la columna Cargo esta lista. Falta volver a correr la carga para llenarla.';
GO
```

Fíjate en el `@Fallos` con veredicto condicional al final: es el patrón de esta casa **y existe por un motivo**. `RAISERROR` de severidad 16 no aborta el batch, así que sin el contador el script terminaría afirmando éxito aunque una aserción hubiera fallado. **No puede haber un `GO` entre el `DECLARE @Fallos` y su último uso**, o el script entero falla con «must declare the scalar variable».

- [ ] **Step 2: Enseñarle `Cargo` al generador**

En `docs/sql/generar-carga-horas-extras.py`: añade `Cargo` a la tabla temporal `#C`, al `INSERT INTO #C`, al `USING`, al `UPDATE SET` y al `INSERT`/`VALUES` del `MERGE`. **Son cinco sitios** — una columna que entra en cuatro de los cinco carga datos en la columna equivocada sin dar ningún error.

Escápalo con `q()` como cualquier texto y añádelo a la validación de longitud que ya existe, con tope 200.

- [ ] **Step 3: Regenerar la carga y comprobar que no entra a git**

```bash
python docs/sql/generar-carga-horas-extras.py
git status --porcelain docs/sql/carga-generada/
git check-ignore -v docs/sql/carga-generada/carga-horas-extras.sql
```
Expected: `git status` vacío y `check-ignore` confirmando la regla. **El archivo generado tiene nombres, cédulas y sueldos de 64 personas reales y este repositorio es público.** Los conteos deben seguir en 64 colaboradores y 69 salarios: si cambian, algo rechazó una fila y hay que entender por qué antes de seguir.

- [ ] **Step 4: No ejecutar nada**

**No ejecutes ni el script ni la carga contra la base.** El usuario los corre. Deja dicho en tu reporte, con estas palabras: *"El script del cargo y la carga regenerada están listos. No ejecuté nada. Hace falta correr primero `docs/sql/2026-09-16-horas-extras-cargo.sql` y después la carga regenerada, para que los 64 cargos queden en la base."*

- [ ] **Step 5: Commit**

```bash
git add docs/sql/2026-09-16-horas-extras-cargo.sql docs/sql/generar-carga-horas-extras.py
git commit -m "feat(horas-extras): el cargo del colaborador, que la fase 1 dejo sin destino"
```

---

## Task 2: Los cinco procedimientos

**Files:**
- Create: `docs/sql/2026-09-16-horas-extras-fase2.sql`

**Interfaces:**
- Consumes: las seis tablas de la fase 1, ya en producción.
- Produces: `Sp_RTA_HeListarPeriodos`, `Sp_RTA_HeCrearPeriodo`, `Sp_RTA_HeInsumos`, `Sp_RTA_HeGuardarFila`, `Sp_RTA_HeCargarPeriodo`.

**Contexto que necesitas.** Los procedimientos de escritura de esta casa devuelven una columna llamada `Respuestas`: `0` = bien, `-1` = no existe o no corresponde, `-2` = el período está cerrado. El lector en C# hace `dr["Respuestas"]`. Los de lectura devuelven varios result sets que C# recorre con `NextResult()` **en el mismo orden en que el procedimiento los declara**; ese orden es un contrato y sólo se extiende por el final.

**Ninguno de estos procedimientos calcula un valor hora ni un total.** `Sp_RTA_HeInsumos` entrega materia prima y `Sp_RTA_HeGuardarFila` guarda lo que C# ya calculó. Si te ves escribiendo `Monto / Divisor * 1.5` en SQL, para y relee la decisión 5.

- [ ] **Step 1: Escribir el script con los cinco procedimientos**

```sql
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
     ORDER BY e.Nombres;

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
```

- [ ] **Step 2: Comprobar que el archivo no lleva tildes ni eñes**

Run: `LC_ALL=C grep -n '[^ -~\t]' docs/sql/2026-09-16-horas-extras-fase2.sql`
Expected: sin salida.

- [ ] **Step 3: Comprobar que ningún procedimiento calcula**

Run: `grep -nE "Monto\s*/|/\s*Divisor|\* *1\.5|\* *2\b|Factor" docs/sql/2026-09-16-horas-extras-fase2.sql`
Expected: sin salida. Si sale algo, has reimplementado la fórmula en SQL — quítalo.

- [ ] **Step 4: Los nombres de columna ya están verificados contra producción**

No los cambies sin motivo: se comprobaron contra la base real el 2026-09-16.

- `dbo.Empleados` tiene **`Nombre`** (singular, `nvarchar(350)`) y `Cedula` (`nvarchar(200)`). **No tiene `Nombres` ni `Cargo`** — son 22 columnas y `Cargo` no está entre ellas.
- Por eso `Cargo` sale de `HE_ColaboradorParametro`, donde lo deja la tarea 1 (esta numeracion es correcta: el cargo es la tarea 1). `R_Usuarios` sí tiene `Cargo`, pero llegar ahí exige unir por cédula y **una cédula la comparten seis usuarios activos**: es la ambigüedad que el diseño evitó colgando el módulo de `Empleados`.

Si aun así encuentras que algo no calza, corrígelo y **dilo en tu reporte** en vez de adivinar.

- [ ] **Step 5: No ejecutar nada**

**No ejecutes este script contra ninguna base de datos ni abras conexión.** El usuario los ejecuta en producción. Deja dicho en tu reporte, con estas palabras: *"El script está listo y commiteado. No lo ejecuté. Hace falta correrlo contra producción antes de que la pantalla sirva."*

- [ ] **Step 6: Commit**

```bash
git add docs/sql/2026-09-16-horas-extras-fase2.sql
git commit -m "feat(horas-extras): los procedimientos de la pantalla, que guardan pero no calculan"
```

---

## Task 3: Las entidades y el acceso a datos

**Files:**
- Create: `CapaEntidad/EntHePeriodo.cs`, `CapaEntidad/EntHeFila.cs`, `CapaEntidad/EntHePantalla.cs`
- Create: `CapaDato/DaoHorasExtras.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj`, `CapaDato/CapaDato.csproj` (añadir los `<Compile Include=...>`)

**Interfaces:**
- Consumes: los cinco procedimientos de la tarea 2; `EntHeSalario` y `EntHeInsumo` de la fase 1.
- Produces:
  - `EntHePeriodo { int IdPeriodo; int Anio; int Mes; string Descripcion; string EstadoPeriodo; bool EstaAbierto { get; } }`
  - `EntHeFila` — ver el código de abajo.
  - `EntHePantalla { EntHePeriodo Periodo; List<EntHeFila> Filas; decimal TotalHoras50; decimal TotalHoras100; decimal TotalPago50; decimal TotalPago100; decimal TotalHoras; decimal TotalPagar; }`
  - `DaoHorasExtras.ListarPeriodos()` → `List<EntHePeriodo>`
  - `DaoHorasExtras.CrearPeriodo(int anio, int mes, string usuario, string ip, out int idPeriodo)` → `int` (Respuestas)
  - `DaoHorasExtras.LeerInsumos(DateTime corte, out List<EntHeFila> colaboradores, out Dictionary<long, List<EntHeSalario>> salarios)`
  - `DaoHorasExtras.GuardarFila(int idPeriodo, EntHeFila fila, string usuario, string ip)` → `int` (Respuestas)
  - `DaoHorasExtras.CargarPeriodo(int idPeriodo)` → `EntHePantalla` con `Filas` llenas y los totales en cero (los suma `CapaNegocio`)

**Contexto que necesitas.** `CapaDato` **no puede** referenciar `CapaNegocio`: el proyecto no compila si lo intentas. Por eso `CargarPeriodo` devuelve los totales en cero y quien los suma es `NegHorasExtrasPantalla`. Es el mismo motivo por el que `DaoPerfil` no calcula la edad aunque tenga la fecha a mano.

Los parámetros van con `SqlDbType` explícito. La conexión se abre con `new DaoReporTareaAranda().conectar()`, como en `CapaDato/DaoPerfil.cs`.

- [ ] **Step 1: Crear `CapaEntidad/EntHePeriodo.cs`**

```csharp
using System;

namespace CapaEntidad
{
    /// <summary>La cabecera de un periodo mensual de horas extras.</summary>
    public class EntHePeriodo
    {
        public int IdPeriodo { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Descripcion { get; set; }

        /// <summary>"Abierto", "Cerrado" o "Anulado".</summary>
        public string EstadoPeriodo { get; set; }

        public DateTime? FechaCierre { get; set; }
        public string UsuarioCierre { get; set; }

        /// <summary>
        /// Solo un periodo Abierto admite edicion. Se compara sin distinguir
        /// mayusculas porque el valor viaja como texto y un dia alguien lo
        /// escribira a mano en la base.
        /// </summary>
        public bool EstaAbierto
        {
            get { return string.Equals(EstadoPeriodo, "Abierto", StringComparison.OrdinalIgnoreCase); }
        }
    }
}
```

- [ ] **Step 2: Crear `CapaEntidad/EntHeFila.cs`**

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Una fila de la grilla: el snapshot congelado del colaborador, las dos
    /// columnas que se digitan, y lo que el calculo derivo de ambos.
    ///
    /// El snapshot es deliberado: un periodo cerrado no debe cambiar porque
    /// alguien edito el maestro despues. Por eso la fila guarda su propia copia
    /// del nombre, el cargo, la jornada y el sueldo, y no los vuelve a consultar.
    /// </summary>
    public class EntHeFila
    {
        public int IdDetalle { get; set; }
        public long IdEmpleado { get; set; }

        public string CedulaSnapshot { get; set; }
        public string NombreSnapshot { get; set; }
        public string EmpresaSnapshot { get; set; }
        public string CargoSnapshot { get; set; }
        public int JornadaHorasDiaSnapshot { get; set; }
        public decimal SalarioBaseSnapshot { get; set; }
        public bool AplicaHESnapshot { get; set; }

        /// <summary>
        /// El divisor manual del maestro, si lo hay. No se persiste en
        /// HE_Detalle: ya quedo aplicado dentro de Divisor. Viaja solo desde
        /// los insumos hasta el calculo.
        /// </summary>
        public int? DivisorManual { get; set; }

        public int Divisor { get; set; }
        public decimal ValorHoraOrdinaria { get; set; }
        public decimal ValorHora50 { get; set; }
        public decimal ValorHora100 { get; set; }

        public decimal Horas50 { get; set; }
        public decimal Horas100 { get; set; }

        public decimal Total50 { get; set; }
        public decimal Total100 { get; set; }
        public decimal TotalHoras { get; set; }
        public decimal TotalHE { get; set; }

        public string Observacion { get; set; }

        /// <summary>
        /// Por que esta persona no aplica, o "EnRevisionSalarial" para alguien
        /// que SI aplica pero tiene el sueldo en revision. La columna de la base
        /// se llama MotivoNoAplica y el nombre enganna: hay dos personas que
        /// aplican horas extras y llevan texto aqui. La pantalla los distingue
        /// con AplicaHESnapshot.
        /// </summary>
        public string MotivoNoAplica { get; set; }

        /// <summary>El calculo marco esta fila como dato que alguien debe revisar.</summary>
        public bool TieneAdvertencia { get; set; }
    }
}
```

- [ ] **Step 3: Crear `CapaEntidad/EntHePantalla.cs`**

```csharp
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Todo lo que la pantalla necesita de una sola ida: el periodo, sus filas
    /// y los seis totales del tablero.
    /// </summary>
    public class EntHePantalla
    {
        public EntHePeriodo Periodo { get; set; }
        public List<EntHeFila> Filas { get; set; }

        public decimal TotalHoras50 { get; set; }
        public decimal TotalHoras100 { get; set; }
        public decimal TotalPago50 { get; set; }
        public decimal TotalPago100 { get; set; }
        public decimal TotalHoras { get; set; }
        public decimal TotalPagar { get; set; }

        public EntHePantalla()
        {
            Filas = new List<EntHeFila>();
        }
    }
}
```

- [ ] **Step 4: Crear `CapaDato/DaoHorasExtras.cs`**

Sigue el molde de `CapaDato/DaoPerfil.cs`: `SqlDbType` explícito, `using` en conexión y comando, y un helper privado para las escrituras. El lector de `LeerInsumos` recorre los dos result sets con `NextResult()` **en el orden en que los declara `Sp_RTA_HeInsumos`**.

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Acceso a datos del modulo de horas extras.
    ///
    /// Esta clase NO calcula: no hay un solo valor hora ni un solo total que
    /// salga de aqui. La formula vive una sola vez, en CapaNegocio. La
    /// dependencia va CapaNegocio -> CapaDato y nunca al reves, asi que llamar
    /// a NegHorasExtras desde aqui seria una referencia circular y no compila.
    /// Por eso CargarPeriodo devuelve los totales en cero: los suma quien puede.
    /// </summary>
    public class DaoHorasExtras
    {
        public static List<EntHePeriodo> ListarPeriodos()
        {
            List<EntHePeriodo> lista = new List<EntHePeriodo>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeListarPeriodos", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read()) { lista.Add(LeerPeriodo(dr)); }
                }
            }

            return lista;
        }

        public static int CrearPeriodo(int anio, int mes, string usuario, string ip, out int idPeriodo)
        {
            int resultado = -1;
            int id = 0;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeCrearPeriodo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Anio", SqlDbType.Int).Value = anio;
                cmd.Parameters.Add("@Mes", SqlDbType.Int).Value = mes;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario ?? "";
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        resultado = Convert.ToInt32(dr["Respuestas"]);
                        id = Convert.ToInt32(dr["IdPeriodo"]);
                    }
                }
            }

            idPeriodo = id;
            return resultado;
        }

        /// <summary>
        /// La materia prima para armar un snapshot. Devuelve los colaboradores y,
        /// aparte, TODO su historial de sueldos sin resolver cual rige: eso lo
        /// decide NegHorasExtras.SalarioVigente, que a igual fecha da prioridad
        /// al ajuste sobre el rol.
        /// </summary>
        public static void LeerInsumos(DateTime corte, out List<EntHeFila> colaboradores,
                                       out Dictionary<long, List<EntHeSalario>> salarios)
        {
            List<EntHeFila> filas = new List<EntHeFila>();
            Dictionary<long, List<EntHeSalario>> historial = new Dictionary<long, List<EntHeSalario>>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeInsumos", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FechaCorte", SqlDbType.Date).Value = corte;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        EntHeFila f = new EntHeFila();
                        f.IdEmpleado = Convert.ToInt64(dr["IdEmpleado"]);
                        f.CedulaSnapshot = Texto(dr, "Cedula");
                        f.NombreSnapshot = Texto(dr, "Nombre");
                        f.CargoSnapshot = Texto(dr, "Cargo");
                        f.EmpresaSnapshot = Texto(dr, "Empresa");
                        f.JornadaHorasDiaSnapshot = Convert.ToInt32(dr["JornadaHorasDia"]);
                        f.DivisorManual = dr["DivisorManual"] == DBNull.Value
                                          ? (int?)null : Convert.ToInt32(dr["DivisorManual"]);
                        f.AplicaHESnapshot = Convert.ToBoolean(dr["AplicaHE"]);
                        f.MotivoNoAplica = Texto(dr, "MotivoNoAplica");
                        f.Observacion = "";
                        filas.Add(f);
                    }

                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            long id = Convert.ToInt64(dr["IdEmpleado"]);
                            if (!historial.ContainsKey(id)) { historial[id] = new List<EntHeSalario>(); }

                            EntHeSalario s = new EntHeSalario();
                            s.Monto = Convert.ToDecimal(dr["Monto"]);
                            s.FechaVigenciaDesde = Convert.ToDateTime(dr["FechaVigenciaDesde"]);
                            s.Origen = Texto(dr, "Origen");
                            historial[id].Add(s);
                        }
                    }
                }
            }

            colaboradores = filas;
            salarios = historial;
        }

        public static int GuardarFila(int idPeriodo, EntHeFila f, string usuario, string ip)
        {
            int resultado = -1;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeGuardarFila", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo;
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.BigInt).Value = f.IdEmpleado;
                cmd.Parameters.Add("@CedulaSnapshot", SqlDbType.VarChar, 20).Value = f.CedulaSnapshot ?? "";
                cmd.Parameters.Add("@NombreSnapshot", SqlDbType.VarChar, 400).Value = f.NombreSnapshot ?? "";
                cmd.Parameters.Add("@EmpresaSnapshot", SqlDbType.VarChar, 120).Value = f.EmpresaSnapshot ?? "";
                cmd.Parameters.Add("@CargoSnapshot", SqlDbType.VarChar, 200).Value = f.CargoSnapshot ?? "";
                cmd.Parameters.Add("@JornadaHorasDiaSnapshot", SqlDbType.Int).Value = f.JornadaHorasDiaSnapshot;
                cmd.Parameters.Add("@SalarioBaseSnapshot", SqlDbType.Decimal).Value = f.SalarioBaseSnapshot;
                cmd.Parameters.Add("@AplicaHESnapshot", SqlDbType.Bit).Value = f.AplicaHESnapshot;
                cmd.Parameters.Add("@Divisor", SqlDbType.Int).Value = f.Divisor;
                cmd.Parameters.Add("@ValorHoraOrdinaria", SqlDbType.Decimal).Value = f.ValorHoraOrdinaria;
                cmd.Parameters.Add("@ValorHora50", SqlDbType.Decimal).Value = f.ValorHora50;
                cmd.Parameters.Add("@ValorHora100", SqlDbType.Decimal).Value = f.ValorHora100;
                cmd.Parameters.Add("@Horas50", SqlDbType.Decimal).Value = f.Horas50;
                cmd.Parameters.Add("@Horas100", SqlDbType.Decimal).Value = f.Horas100;
                cmd.Parameters.Add("@Total50", SqlDbType.Decimal).Value = f.Total50;
                cmd.Parameters.Add("@Total100", SqlDbType.Decimal).Value = f.Total100;
                cmd.Parameters.Add("@TotalHoras", SqlDbType.Decimal).Value = f.TotalHoras;
                cmd.Parameters.Add("@TotalHE", SqlDbType.Decimal).Value = f.TotalHE;
                cmd.Parameters.Add("@Observacion", SqlDbType.VarChar, 400).Value = f.Observacion ?? "";
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario ?? "";
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { resultado = Convert.ToInt32(dr["Respuestas"]); }
                }
            }

            return resultado;
        }

        /// <summary>
        /// El periodo y sus filas. Los totales quedan en cero: sumarlos aqui
        /// obligaria a CapaDato a conocer la regla de redondeo, que es de negocio.
        /// </summary>
        public static EntHePantalla CargarPeriodo(int idPeriodo)
        {
            EntHePantalla pantalla = new EntHePantalla();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeCargarPeriodo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { pantalla.Periodo = LeerPeriodo(dr); }

                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            EntHeFila f = new EntHeFila();
                            f.IdDetalle = Convert.ToInt32(dr["IdDetalle"]);
                            f.IdEmpleado = Convert.ToInt64(dr["IdEmpleado"]);
                            f.CedulaSnapshot = Texto(dr, "CedulaSnapshot");
                            f.NombreSnapshot = Texto(dr, "NombreSnapshot");
                            f.EmpresaSnapshot = Texto(dr, "EmpresaSnapshot");
                            f.CargoSnapshot = Texto(dr, "CargoSnapshot");
                            f.JornadaHorasDiaSnapshot = Convert.ToInt32(dr["JornadaHorasDiaSnapshot"]);
                            f.SalarioBaseSnapshot = Convert.ToDecimal(dr["SalarioBaseSnapshot"]);
                            f.AplicaHESnapshot = Convert.ToBoolean(dr["AplicaHESnapshot"]);
                            f.Divisor = Convert.ToInt32(dr["Divisor"]);
                            f.ValorHoraOrdinaria = Convert.ToDecimal(dr["ValorHoraOrdinaria"]);
                            f.ValorHora50 = Convert.ToDecimal(dr["ValorHora50"]);
                            f.ValorHora100 = Convert.ToDecimal(dr["ValorHora100"]);
                            f.Horas50 = Convert.ToDecimal(dr["Horas50"]);
                            f.Horas100 = Convert.ToDecimal(dr["Horas100"]);
                            f.Total50 = Convert.ToDecimal(dr["Total50"]);
                            f.Total100 = Convert.ToDecimal(dr["Total100"]);
                            f.TotalHoras = Convert.ToDecimal(dr["TotalHoras"]);
                            f.TotalHE = Convert.ToDecimal(dr["TotalHE"]);
                            f.Observacion = Texto(dr, "Observacion");
                            f.MotivoNoAplica = Texto(dr, "MotivoNoAplica");
                            pantalla.Filas.Add(f);
                        }
                    }
                }
            }

            return pantalla;
        }

        private static EntHePeriodo LeerPeriodo(SqlDataReader dr)
        {
            EntHePeriodo p = new EntHePeriodo();
            p.IdPeriodo = Convert.ToInt32(dr["IdPeriodo"]);
            p.Anio = Convert.ToInt32(dr["Anio"]);
            p.Mes = Convert.ToInt32(dr["Mes"]);
            p.Descripcion = Texto(dr, "Descripcion");
            p.EstadoPeriodo = Texto(dr, "EstadoPeriodo");
            p.FechaCierre = dr["FechaCierre"] == DBNull.Value
                            ? (DateTime?)null : Convert.ToDateTime(dr["FechaCierre"]);
            p.UsuarioCierre = Texto(dr, "UsuarioCierre");
            return p;
        }

        private static string Texto(SqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? "" : Convert.ToString(dr[columna]).Trim();
        }
    }
}
```

- [ ] **Step 5: Registrar los cuatro archivos en sus `.csproj`**

Añade un `<Compile Include="EntHePeriodo.cs" />` (y los otros dos) al `ItemGroup` de `CapaEntidad/CapaEntidad.csproj` donde ya están `EntHeSalario.cs` y `EntHeInsumo.cs`, y `<Compile Include="DaoHorasExtras.cs" />` a `CapaDato/CapaDato.csproj` junto a `DaoPerfil.cs`. **Sin esto el archivo existe y no compila: nadie lo ve.**

- [ ] **Step 6: Compilar**

```bash
MSYS_NO_PATHCONV=1 "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" CapaPruebas/CapaPruebas.csproj -t:Build -p:Configuration=Debug -v:quiet -nologo
```
Expected: compila sin errores. Las 128 pruebas de la fase 1 siguen pasando:
```bash
MSYS_NO_PATHCONV=1 "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" "CapaPruebas/bin/Debug/CapaPruebas.dll"
```

- [ ] **Step 7: Commit**

```bash
git add CapaEntidad/EntHePeriodo.cs CapaEntidad/EntHeFila.cs CapaEntidad/EntHePantalla.cs CapaEntidad/CapaEntidad.csproj CapaDato/DaoHorasExtras.cs CapaDato/CapaDato.csproj
git commit -m "feat(horas-extras): las entidades de la pantalla y su acceso a datos"
```

---

## Task 4: La orquestación, y el recálculo en servidor

**Files:**
- Create: `CapaNegocio/NegHorasExtrasPantalla.cs`
- Create: `CapaPruebas/NegHorasExtrasPantallaTests.cs`
- Modify: `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Consumes: `NegHorasExtras.Calcular(EntHeInsumo, EntHeParametros)`, `NegHorasExtras.SalarioVigente(List<EntHeSalario>, DateTime)`, `NegHorasExtras.TotalDelPeriodo(List<decimal>)`, y todo `DaoHorasExtras`.
- Produces:
  - `NegHorasExtrasPantalla.UltimoDiaDelMes(int anio, int mes)` → `DateTime`
  - `NegHorasExtrasPantalla.AplicarCalculo(EntHeFila fila, decimal salario, EntHeParametros p)` → `void` (rellena la fila)
  - `NegHorasExtrasPantalla.SumarTotales(EntHePantalla pantalla)` → `void`
  - `NegHorasExtrasPantalla.AbrirPeriodo(int anio, int mes, string usuario, string ip)` → `EntRespuesta`
  - `NegHorasExtrasPantalla.CargarPantalla(int idPeriodo)` → `EntRespuesta`
  - `NegHorasExtrasPantalla.GuardarHoras(int idPeriodo, long idEmpleado, decimal horas50, decimal horas100, string observacion, string usuario, string ip)` → `EntRespuesta`

**Lo que esta tarea tiene que dejar claro.** `GuardarHoras` **no recibe ni un solo total del cliente.** Recibe horas y observación, relee el snapshot de la base, recalcula con `NegHorasExtras`, persiste sus propios números y **los devuelve**. Ése es el mecanismo que hace visible una divergencia entre el JavaScript y el C#: la grilla se repinta con lo que dijo el servidor.

**Tres funciones se separan a propósito** (`UltimoDiaDelMes`, `AplicarCalculo`, `SumarTotales`): son la parte que se puede probar sin base de datos. `AbrirPeriodo`, `CargarPantalla` y `GuardarHoras` hablan con el DAO y no se prueban aquí.

- [ ] **Step 1: Escribir las pruebas primero**

`CapaPruebas/NegHorasExtrasPantallaTests.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CapaPruebas
{
    /// <summary>
    /// Pruebas de la orquestacion que NO necesitan base de datos. Lo que habla
    /// con el DAO no se prueba aqui; lo que decide numeros, si.
    /// </summary>
    [TestClass]
    public class NegHorasExtrasPantallaTests
    {
        private static EntHeParametros Parametros()
        {
            EntHeParametros p = new EntHeParametros();
            p.DiasMes = 30;
            p.HorasMesJornadaCompleta = 240;
            p.Factor50 = 1.5m;
            p.Factor100 = 2m;
            p.DecimalesMonto = 2;
            return p;
        }

        [TestMethod]
        public void UltimoDiaDelMes_Septiembre2026_Da30()
        {
            Assert.AreEqual(new DateTime(2026, 9, 30), NegHorasExtrasPantalla.UltimoDiaDelMes(2026, 9));
        }

        [TestMethod]
        public void UltimoDiaDelMes_FebreroBisiesto_Da29()
        {
            Assert.AreEqual(new DateTime(2028, 2, 29), NegHorasExtrasPantalla.UltimoDiaDelMes(2028, 2));
        }

        /// <summary>
        /// El corte tiene que ser el ULTIMO dia del mes y no el primero. Con el
        /// primero, un ajuste que entra en vigencia el dia 1 del periodo se
        /// tomaria, pero uno que entra el dia 15 quedaria fuera y la persona
        /// cobraria el mes entero al sueldo viejo.
        /// </summary>
        [TestMethod]
        public void AplicarCalculo_UsaElSalarioVigenteAlCorte()
        {
            List<EntHeSalario> historial = new List<EntHeSalario>
            {
                new EntHeSalario { Monto = 1200m, FechaVigenciaDesde = new DateTime(2026, 1, 1), Origen = "Rol" },
                new EntHeSalario { Monto = 1500m, FechaVigenciaDesde = new DateTime(2026, 9, 15), Origen = "Ajuste" }
            };

            decimal alCorte = NegHorasExtras.SalarioVigente(historial, NegHorasExtrasPantalla.UltimoDiaDelMes(2026, 9));

            Assert.AreEqual(1500m, alCorte);
        }

        [TestMethod]
        public void AplicarCalculo_JornadaCompleta_LlenaLaFila()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.AplicaHESnapshot = true;
            f.Horas50 = 10m;
            f.Horas100 = 0m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 1200m, Parametros());

            Assert.AreEqual(1200m, f.SalarioBaseSnapshot);
            Assert.AreEqual(240, f.Divisor);
            Assert.AreEqual(5m, f.ValorHoraOrdinaria);
            Assert.AreEqual(7.5m, f.ValorHora50);
            Assert.AreEqual(75.00m, f.Total50);
            Assert.AreEqual(75.00m, f.TotalHE);
            Assert.AreEqual(10m, f.TotalHoras);
            Assert.IsFalse(f.TieneAdvertencia);
        }

        /// <summary>
        /// El divisor manual manda sobre la jornada. Es la unica columna del
        /// maestro que cambia el valor de la hora, asi que si se perdiera por
        /// el camino la persona cobraria distinto sin que nada avise.
        /// </summary>
        [TestMethod]
        public void AplicarCalculo_ConDivisorManual_MandaSobreLaJornada()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.DivisorManual = 120;
            f.AplicaHESnapshot = true;
            f.Horas50 = 10m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 1200m, Parametros());

            Assert.AreEqual(120, f.Divisor);
            Assert.AreEqual(10m, f.ValorHoraOrdinaria);
            Assert.AreEqual(150.00m, f.Total50);
        }

        [TestMethod]
        public void AplicarCalculo_SinSalario_MarcaAdvertenciaYNoPaga()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.AplicaHESnapshot = true;
            f.Horas50 = 10m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 0m, Parametros());

            Assert.IsTrue(f.TieneAdvertencia);
            Assert.AreEqual(0m, f.TotalHE);
            Assert.AreEqual(10m, f.TotalHoras, "las horas cargadas son validas: lo que falta es el sueldo");
        }

        [TestMethod]
        public void AplicarCalculo_NoAplicaHE_ConHoras_PagaCero()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.AplicaHESnapshot = false;
            f.Horas50 = 10m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 1200m, Parametros());

            Assert.AreEqual(0m, f.TotalHE);
        }

        /// <summary>
        /// El total del periodo es la suma de los totales YA redondeados, no el
        /// redondeo de la suma. Con 0.125 por fila la diferencia se ve: tres
        /// filas dan 0.39 sumando redondeados y 0.38 redondeando la suma.
        /// </summary>
        [TestMethod]
        public void SumarTotales_SumaLosTotalesYaRedondeados()
        {
            EntHePantalla p = new EntHePantalla();
            for (int i = 0; i < 3; i++)
            {
                EntHeFila f = new EntHeFila();
                f.Horas50 = 1m;
                f.Total50 = 0.13m;
                f.TotalHE = 0.13m;
                f.TotalHoras = 1m;
                p.Filas.Add(f);
            }

            NegHorasExtrasPantalla.SumarTotales(p);

            Assert.AreEqual(0.39m, p.TotalPago50);
            Assert.AreEqual(0.39m, p.TotalPagar);
            Assert.AreEqual(3m, p.TotalHoras50);
            Assert.AreEqual(3m, p.TotalHoras);
        }

        [TestMethod]
        public void SumarTotales_SinFilas_DaCeroYNoLanza()
        {
            EntHePantalla p = new EntHePantalla();

            NegHorasExtrasPantalla.SumarTotales(p);

            Assert.AreEqual(0m, p.TotalPagar);
        }
    }
}
```

- [ ] **Step 2: Correrlas y verlas fallar**

```bash
MSYS_NO_PATHCONV=1 "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" CapaPruebas/CapaPruebas.csproj -t:Build -p:Configuration=Debug -v:quiet -nologo
```
Expected: **no compila**, con `El nombre 'NegHorasExtrasPantalla' no existe`. Ése es el fallo correcto en este paso.

- [ ] **Step 3: Escribir `CapaNegocio/NegHorasExtrasPantalla.cs`**

```csharp
using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// La orquestacion de la pantalla de horas extras: abrir un periodo,
    /// cargarlo y guardar.
    ///
    /// Vive aparte de NegHorasExtras a proposito. Aquella es una clase pura que
    /// no sabe que existe una base de datos, y se prueba entera sin levantar
    /// nada; esta habla con el DAO. Mezclarlas obligaria a una u otra a mentir
    /// sobre lo que necesita para funcionar.
    /// </summary>
    public static class NegHorasExtrasPantalla
    {
        /// <summary>
        /// El corte de un periodo es el ULTIMO dia del mes. Con el primero, un
        /// ajuste salarial que entra en vigencia a mitad de mes quedaria fuera
        /// y la persona cobraria el mes entero al sueldo anterior.
        /// </summary>
        public static DateTime UltimoDiaDelMes(int anio, int mes)
        {
            return new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));
        }

        /// <summary>
        /// Rellena una fila a partir de su salario y los parametros. Es el unico
        /// punto donde la pantalla toca el calculo, y delega entero en
        /// NegHorasExtras: aqui no hay ni una division ni un factor.
        /// </summary>
        public static void AplicarCalculo(EntHeFila fila, decimal salario, EntHeParametros parametros)
        {
            if (fila == null) { return; }

            fila.SalarioBaseSnapshot = salario;

            EntHeInsumo insumo = new EntHeInsumo();
            insumo.SalarioBaseVigente = salario;
            insumo.JornadaHorasDia = fila.JornadaHorasDiaSnapshot;
            insumo.DivisorManual = fila.DivisorManual;
            insumo.AplicaHE = fila.AplicaHESnapshot;
            insumo.Horas50 = fila.Horas50;
            insumo.Horas100 = fila.Horas100;

            EntHeResultado r = NegHorasExtras.Calcular(insumo, parametros);

            fila.Divisor = r.Divisor;
            fila.ValorHoraOrdinaria = r.ValorHoraOrdinaria;
            fila.ValorHora50 = r.ValorHora50;
            fila.ValorHora100 = r.ValorHora100;
            fila.Total50 = r.Total50;
            fila.Total100 = r.Total100;
            fila.TotalHoras = r.TotalHoras;
            fila.TotalHE = r.TotalHE;
            fila.TieneAdvertencia = r.TieneAdvertencia;
        }

        /// <summary>
        /// Los seis indicadores del tablero. Suma totales YA redondeados, que no
        /// es lo mismo que redondear la suma: asi el gran total cuadra con lo que
        /// cualquiera obtiene sumando a mano la columna de la pantalla.
        /// </summary>
        public static void SumarTotales(EntHePantalla pantalla)
        {
            if (pantalla == null || pantalla.Filas == null) { return; }

            List<decimal> pago50 = new List<decimal>();
            List<decimal> pago100 = new List<decimal>();
            List<decimal> pagoTotal = new List<decimal>();
            decimal horas50 = 0m, horas100 = 0m, horas = 0m;

            foreach (EntHeFila f in pantalla.Filas)
            {
                if (f == null) { continue; }
                pago50.Add(f.Total50);
                pago100.Add(f.Total100);
                pagoTotal.Add(f.TotalHE);
                horas50 += f.Horas50;
                horas100 += f.Horas100;
                horas += f.TotalHoras;
            }

            pantalla.TotalPago50 = NegHorasExtras.TotalDelPeriodo(pago50);
            pantalla.TotalPago100 = NegHorasExtras.TotalDelPeriodo(pago100);
            pantalla.TotalPagar = NegHorasExtras.TotalDelPeriodo(pagoTotal);
            pantalla.TotalHoras50 = horas50;
            pantalla.TotalHoras100 = horas100;
            pantalla.TotalHoras = horas;
        }

        /// <summary>
        /// Abre un periodo: lo crea si no existe y le arma el snapshot, una fila
        /// por colaborador activo.
        ///
        /// Correrlo dos veces sobre el mismo mes es inofensivo y ademas util: si
        /// la primera vez fallo a medias, la segunda completa lo que falte. Lo
        /// que NO hace es pisar las horas ya digitadas de una fila que existe,
        /// porque Sp_RTA_HeGuardarFila recibe las horas que se le pasan y aqui
        /// solo se le pasan las de la fila que ya estaba.
        /// </summary>
        public static EntRespuesta AbrirPeriodo(int anio, int mes, string usuario, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();

            int idPeriodo;
            int creado = DaoHorasExtras.CrearPeriodo(anio, mes, usuario, ip, out idPeriodo);

            if (creado != 0 || idPeriodo <= 0)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "No se pudo abrir el periodo.";
                respuesta.tipoMensaje = "danger";
                return respuesta;
            }

            EntHePantalla existente = DaoHorasExtras.CargarPeriodo(idPeriodo);

            if (existente.Periodo != null && !existente.Periodo.EstaAbierto)
            {
                return CargarPantalla(idPeriodo);
            }

            Dictionary<long, EntHeFila> yaEstan = new Dictionary<long, EntHeFila>();
            foreach (EntHeFila f in existente.Filas) { yaEstan[f.IdEmpleado] = f; }

            EntHeParametros parametros = NegHeParametros.Vigentes();
            DateTime corte = UltimoDiaDelMes(anio, mes);

            List<EntHeFila> colaboradores;
            Dictionary<long, List<EntHeSalario>> salarios;
            DaoHorasExtras.LeerInsumos(corte, out colaboradores, out salarios);

            foreach (EntHeFila fila in colaboradores)
            {
                if (yaEstan.ContainsKey(fila.IdEmpleado))
                {
                    fila.Horas50 = yaEstan[fila.IdEmpleado].Horas50;
                    fila.Horas100 = yaEstan[fila.IdEmpleado].Horas100;
                    fila.Observacion = yaEstan[fila.IdEmpleado].Observacion;
                }

                List<EntHeSalario> historial = salarios.ContainsKey(fila.IdEmpleado)
                                               ? salarios[fila.IdEmpleado]
                                               : new List<EntHeSalario>();

                AplicarCalculo(fila, NegHorasExtras.SalarioVigente(historial, corte), parametros);
                DaoHorasExtras.GuardarFila(idPeriodo, fila, usuario, ip);
            }

            return CargarPantalla(idPeriodo);
        }

        /// <summary>El periodo, sus filas y el tablero, listos para la pantalla.</summary>
        public static EntRespuesta CargarPantalla(int idPeriodo)
        {
            EntRespuesta respuesta = new EntRespuesta();
            EntHePantalla pantalla = DaoHorasExtras.CargarPeriodo(idPeriodo);

            if (pantalla.Periodo == null)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Ese periodo no existe.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            SumarTotales(pantalla);

            respuesta.estado = "1";
            respuesta.resultado = pantalla;
            respuesta.mensaje = "";
            respuesta.tipoMensaje = "success";
            return respuesta;
        }

        /// <summary>
        /// Guarda las horas de una fila.
        ///
        /// NO recibe ningun total del cliente y no usaria uno aunque se lo
        /// mandaran: relee el snapshot de la base, recalcula, guarda sus propios
        /// numeros y los devuelve para que la grilla se repinte con ellos. Si el
        /// JavaScript calculo distinto, el usuario ve el numero saltar y la
        /// divergencia se vuelve visible en vez de silenciosa.
        /// </summary>
        public static EntRespuesta GuardarHoras(int idPeriodo, long idEmpleado,
                                                decimal horas50, decimal horas100,
                                                string observacion, string usuario, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();
            EntHePantalla pantalla = DaoHorasExtras.CargarPeriodo(idPeriodo);

            if (pantalla.Periodo == null)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Ese periodo no existe.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            if (!pantalla.Periodo.EstaAbierto)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "El periodo esta cerrado: ya no admite cambios.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            EntHeFila fila = null;
            foreach (EntHeFila f in pantalla.Filas)
            {
                if (f.IdEmpleado == idEmpleado) { fila = f; break; }
            }

            if (fila == null)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Esa persona no esta en este periodo.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            fila.Horas50 = horas50;
            fila.Horas100 = horas100;
            fila.Observacion = observacion ?? "";

            /* El 6.6 funcional manda revalidar la elegibilidad y el sueldo contra
               la base al guardar, porque pudieron cambiar desde que se cargo la
               pantalla. No choca con el snapshot: el snapshot protege al periodo
               CERRADO de que lo muevan por detras, y un periodo cerrado ni
               siquiera llega aqui -la guarda de arriba lo rechaza-. Mientras
               esta abierto, que un sueldo corregido se aplique es lo correcto:
               si no, alguien arregla un sueldo mal cargado y el periodo del mes
               sigue pagando sobre el equivocado, sin avisar. */
            DateTime corte = UltimoDiaDelMes(pantalla.Periodo.Anio, pantalla.Periodo.Mes);

            List<EntHeFila> colaboradores;
            Dictionary<long, List<EntHeSalario>> salarios;
            DaoHorasExtras.LeerInsumos(corte, out colaboradores, out salarios);

            foreach (EntHeFila actual in colaboradores)
            {
                if (actual.IdEmpleado != idEmpleado) { continue; }

                fila.AplicaHESnapshot = actual.AplicaHESnapshot;
                fila.JornadaHorasDiaSnapshot = actual.JornadaHorasDiaSnapshot;
                fila.DivisorManual = actual.DivisorManual;
                fila.CargoSnapshot = actual.CargoSnapshot;
                fila.EmpresaSnapshot = actual.EmpresaSnapshot;
                break;
            }

            List<EntHeSalario> historial = salarios.ContainsKey(idEmpleado)
                                           ? salarios[idEmpleado]
                                           : new List<EntHeSalario>();

            AplicarCalculo(fila, NegHorasExtras.SalarioVigente(historial, corte), NegHeParametros.Vigentes());

            int guardado = DaoHorasExtras.GuardarFila(idPeriodo, fila, usuario, ip);

            if (guardado == -2)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "El periodo esta cerrado: ya no admite cambios.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            if (guardado != 0)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "No se pudo guardar.";
                respuesta.tipoMensaje = "danger";
                return respuesta;
            }

            return CargarPantalla(idPeriodo);
        }
    }
}
```

- [ ] **Step 4: Crear `CapaNegocio/NegHeParametros.cs`**

`AbrirPeriodo` y `GuardarHoras` usan `NegHeParametros.Vigentes()`, que todavía no existe. Léelos de `HE_Parametro` en lugar de incrustarlos: **son configurables por diseño y hay un permiso de la fase 3 que va a dejar editarlos.**

Añade a `CapaDato/DaoHorasExtras.cs` un método más:

```csharp
/// <summary>Los parametros vigentes hoy, tal cual estan en HE_Parametro.</summary>
public static Dictionary<string, decimal> LeerParametrosVigentes()
{
    Dictionary<string, decimal> valores = new Dictionary<string, decimal>();
    DaoReporTareaAranda conexion = new DaoReporTareaAranda();

    using (SqlConnection cnx = conexion.conectar())
    using (SqlCommand cmd = new SqlCommand(
        "SELECT Clave, Valor FROM dbo.HE_Parametro WHERE FechaVigenciaHasta IS NULL", cnx))
    {
        cmd.CommandType = CommandType.Text;
        cnx.Open();

        using (SqlDataReader dr = cmd.ExecuteReader())
        {
            while (dr.Read())
            {
                valores[Convert.ToString(dr["Clave"]).Trim()] = Convert.ToDecimal(dr["Valor"]);
            }
        }
    }

    return valores;
}
```

Y crea `CapaNegocio/NegHeParametros.cs`:

```csharp
using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Los parametros de calculo, leidos de HE_Parametro.
    ///
    /// No se incrustan en el codigo porque son configurables por diseno: hay un
    /// permiso previsto para editarlos. Si una clave faltara, se usa el valor con
    /// el que se cargo la base, que es lo que el modulo ha usado siempre.
    /// </summary>
    public static class NegHeParametros
    {
        public static EntHeParametros Vigentes()
        {
            Dictionary<string, decimal> v = DaoHorasExtras.LeerParametrosVigentes();

            EntHeParametros p = new EntHeParametros();
            p.DiasMes = (int)Valor(v, "DiasMes", 30m);
            p.HorasMesJornadaCompleta = (int)Valor(v, "HorasMesJornadaCompleta", 240m);
            p.Factor50 = Valor(v, "Factor50", 1.5m);
            p.Factor100 = Valor(v, "Factor100", 2m);
            p.DecimalesMonto = (int)Valor(v, "DecimalesMonto", 2m);
            return p;
        }

        private static decimal Valor(Dictionary<string, decimal> v, string clave, decimal porOmision)
        {
            return v.ContainsKey(clave) ? v[clave] : porOmision;
        }
    }
}
```

**Comprueba los nombres de las propiedades de `EntHeParametros` contra el archivo real** (`CapaEntidad/EntHeParametros.cs`, de la fase 1) antes de dar esto por bueno. Si alguno no calza, corrígelo y dilo en tu reporte.

- [ ] **Step 5: Registrar los archivos nuevos en los `.csproj` y compilar**

```bash
MSYS_NO_PATHCONV=1 "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" CapaPruebas/CapaPruebas.csproj -t:Build -p:Configuration=Debug -v:quiet -nologo
```

- [ ] **Step 6: Correr la suite entera**

```bash
MSYS_NO_PATHCONV=1 "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" "CapaPruebas/bin/Debug/CapaPruebas.dll"
```
Expected: **137 de 137** (128 de la fase 1 + 9 nuevas). Si el número no cuadra, **dilo en tu reporte en vez de ajustarlo.**

- [ ] **Step 7: La prueba de ida y vuelta**

Comenta la línea `fila.DivisorManual = ...` dentro de `AplicarCalculo` (o pásale `null`), vuelve a correr, y comprueba que `AplicarCalculo_ConDivisorManual_MandaSobreLaJornada` **falla** con divisor 240 en vez de 120. Deshaz el cambio. **Escribe en tu reporte el valor que dio al fallar.** Una prueba que no se ha visto fallar no prueba nada.

- [ ] **Step 8: Commit**

```bash
git add CapaNegocio/NegHorasExtrasPantalla.cs CapaNegocio/NegHeParametros.cs CapaNegocio/CapaNegocio.csproj CapaDato/DaoHorasExtras.cs CapaPruebas/NegHorasExtrasPantallaTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(horas-extras): la orquestacion de la pantalla, con el recalculo del lado del servidor"
```

---

## Task 5: La pantalla y su handler

**Files:**
- Create: `ReporteTareas/Formulario/HorasExtras.aspx`, `.aspx.cs`, `.aspx.designer.cs`
- Create: `ReporteTareas/Formulario/AdministrarHorasExtras.ashx`, `.ashx.cs`
- Create: `ReporteTareas/js/horasExtras.js`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: `NegHorasExtrasPantalla.AbrirPeriodo`, `.CargarPantalla`, `.GuardarHoras`; `DaoHorasExtras.ListarPeriodos()`.
- Produces: las acciones JSON `ListarPeriodos`, `AbrirPeriodo`, `CargarPeriodo`, `GuardarFila`.

**Contexto que necesitas.** El molde exacto está en `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` (handler) y `ReporteTareas/js/miPerfil.js` (cliente). Cópialos en estructura, **no en su manejo de identidad si difiere**: aquí la identidad sale de la sesión, siempre.

El handler declara `IRequiresSessionState`. Sin eso `context.Session` es `null` en un `IHttpHandler` y no hay con qué comprobar nada.

- [ ] **Step 1: El handler `AdministrarHorasExtras.ashx.cs`**

```csharp
using CapaDato;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetHorasExtras
{
    /// <summary>
    /// Handler de la pantalla de horas extras.
    ///
    /// IRequiresSessionState no es decorativo: sin el, context.Session es null en
    /// un IHttpHandler y no habria identidad con la que sellar quien guardo que.
    ///
    /// Ninguna accion acepta un total del cliente. Las horas si vienen de ahi
    /// -son lo que el usuario digita-, pero el dinero lo recalcula el servidor y
    /// devuelve lo suyo.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarHorasExtras : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder salida = new StringBuilder();

            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                salida.Append(Mensaje("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger"));
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                var lector = new System.IO.StreamReader(context.Request.InputStream);
                var json = lector.ReadToEnd();

                JavaScriptSerializer s = new JavaScriptSerializer();
                dynamic parametros = s.Deserialize(json.ToString(), typeof(object));

                var accion = parametros[0]["action"];
                bool existe = false;

                if (accion == "ListarPeriodos") { existe = true; salida.Append(ListarPeriodos()); }
                if (accion == "AbrirPeriodo")   { existe = true; salida.Append(AbrirPeriodo(context, parametros[0]["parameters"])); }
                if (accion == "CargarPeriodo")  { existe = true; salida.Append(CargarPeriodo(parametros[0]["parameters"])); }
                if (accion == "GuardarFila")    { existe = true; salida.Append(GuardarFila(context, parametros[0]["parameters"])); }

                if (!existe) { salida.Append(Mensaje("0", "La acción solicitada no existe.", "danger")); }
            }
            else
            {
                salida.Append(Mensaje("0", "Petición no válida.", "danger"));
            }

            context.Response.ContentType = "application/json";
            context.Response.Write(salida.ToString());
        }

        private string ListarPeriodos()
        {
            EntRespuesta r = new EntRespuesta();
            r.estado = "1";
            r.resultado = DaoHorasExtras.ListarPeriodos();
            r.tipoMensaje = "success";
            return new JavaScriptSerializer().Serialize(r);
        }

        private string AbrirPeriodo(HttpContext context, dynamic p)
        {
            int anio = Entero(p["anio"]);
            int mes = Entero(p["mes"]);
            EntRespuesta r = NegHorasExtrasPantalla.AbrirPeriodo(anio, mes, Usuario(context), Ip(context));
            return new JavaScriptSerializer().Serialize(r);
        }

        private string CargarPeriodo(dynamic p)
        {
            EntRespuesta r = NegHorasExtrasPantalla.CargarPantalla(Entero(p["idPeriodo"]));
            return new JavaScriptSerializer().Serialize(r);
        }

        private string GuardarFila(HttpContext context, dynamic p)
        {
            EntRespuesta r = NegHorasExtrasPantalla.GuardarHoras(
                Entero(p["idPeriodo"]),
                Convert.ToInt64(Entero(p["idEmpleado"])),
                Decimal(p["horas50"]),
                Decimal(p["horas100"]),
                Convert.ToString(p["observacion"]),
                Usuario(context), Ip(context));
            return new JavaScriptSerializer().Serialize(r);
        }

        private static string Usuario(HttpContext context)
        {
            object v = context.Session["Cod_Usuario"];
            return v == null ? "" : Convert.ToString(v).Trim();
        }

        private static string Ip(HttpContext context)
        {
            string ip = context.Request.UserHostAddress;
            return string.IsNullOrEmpty(ip) ? "" : ip;
        }

        private static int Entero(object v)
        {
            int n;
            return int.TryParse(Convert.ToString(v), out n) ? n : 0;
        }

        /// <summary>
        /// Lo que manda el cliente puede venir con coma, vacio o con basura. Un
        /// dato ilegible vale cero: el servidor no adivina cuantas horas quiso
        /// escribir alguien.
        /// </summary>
        private static decimal Decimal(object v)
        {
            decimal d;
            string texto = Convert.ToString(v);
            if (texto == null) { return 0m; }
            texto = texto.Trim().Replace(",", ".");
            return decimal.TryParse(texto, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out d) ? d : 0m;
        }

        private static string Mensaje(string estado, string mensaje, string tipo)
        {
            EntRespuesta r = new EntRespuesta();
            r.estado = estado;
            r.mensaje = mensaje;
            r.tipoMensaje = tipo;
            return new JavaScriptSerializer().Serialize(r);
        }

        public bool IsReusable { get { return false; } }
    }
}
```

El `.ashx` es una línea:
```
<%@ WebHandler Language="C#" CodeBehind="AdministrarHorasExtras.ashx.cs" Class="JsonJQueryNetHorasExtras.AdministrarHorasExtras" %>
```

- [ ] **Step 2: La pantalla `HorasExtras.aspx`**

**Guárdala en UTF-8 CON BOM.** Estructura: `MasterPageFile="~/Formulario/Master.Master"`, `ResponseEncoding="utf-8"`, el `<script src="../js/horasExtras.js?v=1">` en el `ContentPlaceHolderID="head"`, y en el cuerpo: la cabecera del §5.1 (selector de período, indicador de estado, filtro de empresa, buscar, «Solo con horas», botón Guardar), el tablero de seis indicadores del §5.2, y la tabla de la grilla con `<thead>` **sin** `bg-primary`.

La grilla muestra por omisión ocho columnas —colaborador, cargo, jornada, aplica, horas 50%, horas 100%, total horas y total HE— y un botón despliega las seis derivadas (divisor, valor hora ordinaria, valor hora 50%, total 50%, valor hora 100%, total 100%). Son 64 filas: **sin paginación**.

**No pongas los botones «Cerrar período» ni «Exportar a Excel»:** son fase 3 y un botón que no hace nada es peor que no tenerlo.

- [ ] **Step 3: El cliente `horasExtras.js`**

El núcleo, que es lo que no se puede improvisar:

```javascript
/* Llama al handler con el formato [{action, parameters}] */
function PostHE(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarHorasExtras.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) {
            if (r.estado === "1") { onSuccess(r); }
            else { MostrarMensaje(r.mensaje, r.tipoMensaje); }
        },
        error: function () {
            MostrarMensaje("No se pudo contactar al servidor. Intente nuevamente.", "danger");
        }
    });
}

/* Guarda una fila y REPINTA con lo que devolvio el servidor.
   El calculo del cliente es solo para que el numero aparezca al instante; el
   que vale es el del servidor. Si difieren, el usuario ve el numero saltar, y
   esa es justamente la idea: una divergencia visible en vez de silenciosa. */
function GuardarFila(idPeriodo, idEmpleado) {
    var $fila = $('tr[data-empleado="' + idEmpleado + '"]');

    PostHE("GuardarFila", {
        idPeriodo: idPeriodo,
        idEmpleado: idEmpleado,
        horas50: $fila.find(".he-horas50").val(),
        horas100: $fila.find(".he-horas100").val(),
        observacion: $fila.find(".he-observacion").val()
    }, function (r) {
        PintarGrilla(r.resultado);
        MarcarGuardado();
    });
}
```

Además: recálculo en cliente al salir de una celda, `Enter` que baja a la misma columna de la fila siguiente, pegar una columna desde Excel sobre `Horas 50%`/`Horas 100%`, validación en celda (numérico, ≥ 0, máximo 2 decimales, tope 200 por celda), celdas deshabilitadas cuando `AplicaHESnapshot` es falso, y confirmación al salir con cambios sin guardar.

Estilos condicionales de fila (§5.5), **sin los de topes**:

| Condición | Presentación |
|---|---|
| `AplicaHESnapshot` falso | Fila gris, celdas de horas deshabilitadas, tooltip con `MotivoNoAplica` |
| `AplicaHESnapshot` verdadero **y** `MotivoNoAplica` no vacío | Franja amarilla a la izquierda, tooltip «Salario en revisión — validar antes de cerrar» |
| `ValorHoraOrdinaria` igual a 0 | Fila roja |
| `TotalHoras` mayor que 0 | Resaltado sutil |

La segunda fila de esa tabla es la que distingue a las **dos personas que aplican horas extras y llevan texto en `MotivoNoAplica`**. La columna está mal nombrada y no se renombra en esta fase; la pantalla tiene que leerla bien de todos modos.

- [ ] **Step 4: Registrar los cinco archivos en `ReporteTareas.csproj`**

`.aspx` y `.ashx` como `<Content Include=...>`, los `.cs` como `<Compile Include=...>` con su `<DependentUpon>`, y el `.js` como `<Content Include=...>`. Copia la forma exacta de las entradas de `MiPerfil.aspx` y `AdministrarPerfil.ashx` que ya están en ese archivo.

- [ ] **Step 5: Comprobar el BOM**

```bash
head -c 3 ReporteTareas/Formulario/HorasExtras.aspx | od -An -tx1
```
Expected: `ef bb bf`. Si no está, la página sale con caracteres raros en producción.

- [ ] **Step 6: Compilar la solución**

```bash
MSYS_NO_PATHCONV=1 "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" ReporteTareas/ReporteTareas.csproj -t:Build -p:Configuration=Debug -v:quiet -nologo
```

- [ ] **Step 7: Commit**

```bash
git add ReporteTareas/Formulario/HorasExtras.aspx ReporteTareas/Formulario/HorasExtras.aspx.cs ReporteTareas/Formulario/HorasExtras.aspx.designer.cs ReporteTareas/Formulario/AdministrarHorasExtras.ashx ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs ReporteTareas/js/horasExtras.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(horas-extras): la pantalla de captura y su handler"
```

---

## Task 6: El registro en el menú

**Files:**
- Create: `docs/sql/2026-09-16-horas-extras-fase2-menu.sql`

**Contexto que necesitas, y es la parte donde es fácil equivocarse.**

- El menú se arma en `Master.Master.cs` con `Sp_RTA_ConsultarMenuPerfilUsuario(Session["Id_Perfil"])`.
- **`PerfilMenu.Estado` tiene la semántica INVERTIDA: `'0'` MUESTRA la opción y `'1'` la OCULTA.** El SP filtra `WHERE P.Estado = 0`. Equivocarse aquí no da ningún error: simplemente nadie ve la pantalla, o la ve quien no debe.
- **Una opción hija sólo se ve si su grupo padre también tiene `Estado='0'` para ese perfil.** Si creas un grupo nuevo, necesita sus propias filas en `PerfilMenu`.
- Las columnas de `PerfilMenu` tienen casing inconsistente: `id_Menu`, **`IdPerfil`** (sin guion bajo), `Estado` (varchar).
- En `MenuDos`, `Href` es sólo el nombre del `.aspx` (relativo a `/Formulario/`), la etiqueta va en `Titulo`, `Es_Opcion_de_Menu = 1` es cabecera de grupo con `Id_MenuPadre = 0`, y `= 0` es página hoja.
- Los perfiles son el **14 (Talento Humano)** y el **18 (Super Admin)**, y no otros.

- [ ] **Step 1: Escribir el script**

Crea un grupo nuevo «Nomina» con `Es_Opcion_de_Menu = 1` e `Id_MenuPadre = 0`, cuelga de él la hoja `HorasExtras.aspx`, y da visibilidad (`Estado = '0'`) a los perfiles 14 y 18 **tanto en el grupo como en la hoja**. El script debe ser idempotente (`IF NOT EXISTS`) y terminar con un `SELECT` que muestre qué quedó, para poder comprobarlo de un vistazo.

Sin tildes ni eñes: por eso el grupo se llama `Nomina` y no `Nómina`.

- [ ] **Step 2: Comprobar que no lleva tildes**

Run: `LC_ALL=C grep -n '[^ -~\t]' docs/sql/2026-09-16-horas-extras-fase2-menu.sql`
Expected: sin salida.

- [ ] **Step 3: No ejecutarlo**

**No lo ejecutes.** Déjalo dicho en tu reporte: el usuario lo corre en producción después del script de la tarea 2.

- [ ] **Step 4: Commit**

```bash
git add docs/sql/2026-09-16-horas-extras-fase2-menu.sql
git commit -m "feat(horas-extras): la pantalla entra al menu de los perfiles 14 y 18"
```

---

## Cierre de la fase

Antes de dar la fase por terminada:

1. **Regenerar el paquete de despliegue.** El repo versiona `ReporteTareas/bin/` y `obj/Release/Package/PackageTmp` **a propósito**: el despliegue sale de ahí. Esta fase sí trae pantalla, así que un paquete viejo significa que la pantalla no llega y **nada da error**. Se comprueba mirando que `HorasExtras.aspx` exista dentro del paquete. Esto se olvidó en las cuatro entregas anteriores del módulo de perfil.
2. **El orden de ejecución en producción**, y este orden importa: el paso 2 llena una columna que el paso 1 crea, y el paso 3 la lee.

   | # | Qué | Por qué en ese lugar |
   |---|---|---|
   | 1 | `docs/sql/2026-09-16-horas-extras-cargo.sql` | Crea la columna `Cargo` |
   | 2 | `docs/sql/carga-generada/carga-horas-extras.sql` (regenerada) | La llena para los 64. **No está en git**, se regenera con el `.py` |
   | 3 | `docs/sql/2026-09-16-horas-extras-fase2.sql` | Los cinco procedimientos |
   | 4 | `docs/sql/2026-09-16-horas-extras-fase2-menu.sql` | La opción de menú |
   | 5 | Los binarios | Los scripts SQL van siempre antes (`DESPLIEGUE.md`) |

   **Comprobación después del paso 2, y hacen falta las dos:**

   ```sql
   SELECT Cargos = COUNT(CASE WHEN Cargo IS NOT NULL THEN 1 END),   -- 64
          Activos = COUNT(CASE WHEN Estado = '1' THEN 1 END),        -- 62
          Inactivos = COUNT(CASE WHEN Estado = '0' THEN 1 END)       -- 2
     FROM dbo.HE_ColaboradorParametro;
   ```

   La de `Estado` no es redundante. `docs/sql/carga-generada/` está fuera de git, así que en la máquina de despliegue puede haberse quedado el archivo generado durante la **fase 1**. Correrlo **tiene éxito**: no toca `Cargo` ni `Estado`, imprime «Carga de colaboradores y sueldos terminada» y deja los 64 como activos. El conteo de `Cargo` atraparía ese caso; el de `Estado` atrapa además el de un generado intermedio que traiga `Cargo` y no `Estado`.

   Si los números no dan, **regenera** con `python docs/sql/generar-carga-horas-extras.py` y vuelve a correr la carga. La cabecera del archivo generado dice desde cuándo es y qué columnas escribe, precisamente para poder distinguir una copia vieja sin abrirla entera.
3. **Comprobación manual mínima:** entrar con un usuario del perfil 14, abrir el período de septiembre 2026, verificar que aparecen 64 filas, digitar horas en una y guardar, y comprobar que el total de la fila y el del tablero cuadran con la calculadora. Después entrar con un perfil que no sea 14 ni 18 y comprobar que **la opción no aparece en el menú**.
