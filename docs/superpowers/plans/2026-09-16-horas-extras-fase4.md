# Horas Extras fase 4: periodo por rango de fechas y siembra desde las tareas aprobadas

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** que un periodo de horas extras se defina por un rango de fechas libre en vez de por ano y mes, y que al abrirlo traiga ya calculadas las horas extras aprobadas en ese rango desde `R_DetTareasAranda`.

**Architecture:** el periodo deja de identificarse por `(Anio, Mes)` y pasa a identificarse por `(FechaInicio, FechaFin)`. La fecha de corte del sueldo, que hoy se deduce del mes con `UltimoDiaDelMes`, pasa a ser `FechaFin` — el metodo desaparece. Un procedimiento nuevo agrega las horas aprobadas por colaborador y `AbrirPeriodo` las siembra en las filas que nadie ha corregido a mano, distinguidas por una columna nueva `HorasOrigen`.

**Tech Stack:** ASP.NET WebForms, .NET Framework 4.8 (web) / 4.6.1 (capas), SQL Server, MSTest v1, EPPlus 6.2.7, jQuery.

**Spec:** `docs/superpowers/specs/2026-09-15-horas-extras-design.md` — con dos decisiones **revocadas por el usuario el 2026-09-16**, anotadas abajo en Constraints.

## Global Constraints

- **Las capas van a .NET Framework 4.6.1.** No existen `record`, `init`, ni `switch` de expresion. `CapaDato` **nunca** referencia `CapaNegocio`.
- **Todo importe y toda hora en `decimal`, nunca `double`.** Redondeo `MidpointRounding.AwayFromZero`. `DecimalesValorHora = 6`.
- **Los `.sql` versionados van sin BOM y sin tildes.** Escribirlos en UTF-8 plano.
- **Las aserciones de los `.sql` usan el contador `@Fallos` con veredicto condicional al final.** `RAISERROR` severidad 16 no aborta el lote. **No puede haber un `GO` entre el `DECLARE @Fallos` y su ultimo uso.**
- **`CREATE PROCEDURE` tiene que ser la primera instruccion de su lote:** no se puede envolver en un `IF`. Para saltar un script entero, `SET NOEXEC ON` / `SET NOEXEC OFF`.
- **`sys.parameters.has_default_value` vale 0 para todo procedimiento T-SQL.** Que un default funciona se comprueba llamando al procedimiento sin ese parametro, no mirando el catalogo.
- **Los `.aspx` se guardan con BOM UTF-8.** `Web.config:39` declara `windows-1252` sin `fileEncoding`.
- **Compilar con el MSBuild de VS2019:** `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`. El del PATH falla con errores que despistan.
- **Los contratos posicionales de result sets se extienden solo por el FINAL.** Se leen con `NextResult()`.
- **Codigos de `Respuestas`:** `0` OK, `-1` no encontrado, `-2` cerrado, `-3` filas en cero, `-4` filas faltantes. Esta fase anade `-5` solapamiento.
- **No ejecutar ningun script contra produccion.** El controlador los ejecuta y verifica. Los subagentes escriben y reportan.
- **Nunca imprimir cedulas, nombres ni sueldos** en reportes ni en salida de consola. El repositorio de GitHub es publico.

### Dos decisiones del diseno que el usuario revoco el 2026-09-16

1. La espec dice *«el flujo de aprobacion de 2019 no se toca, no se consume»*. Se investigo: esa frase es cierta de `R_TareasArandaHorasExtras` (31 filas, 0 horas) pero **falsa del flujo vivo**, que esta en `R_DetTareasAranda` con 22.170 solicitudes. **Esta fase lo consume, en solo lectura.**
2. La decision 5 dice *«periodo mensual calendario»* y deja las quincenas fuera de alcance. **Queda revocada:** el periodo pasa a ser un rango libre.

### Datos de produccion verificados el 2026-09-16 (base de las aserciones)

- `HE_Periodo`: **2 filas** (2026/9 y 2026/8), ambas `Abierto`. `HE_Detalle`: **124 filas** (62 por periodo), **todas con `Horas50 = 0`, `Horas100 = 0` y `Observacion` vacia**. Nadie ha capturado nada: se pueden re-sembrar sin perder trabajo.
- `HE_ColaboradorParametro` con `Estado = '1'`: **62**.
- Catalogos: `IdTipoCatalogo = 4` -> `1 Solicitado, 2 Aprobado, 3 Rechazado`. `IdTipoCatalogo = 7` -> `1 = 50%, 2 = 100%`.
- Rango 2026-08-17 a 2026-09-15, aprobadas y con tipo 1 o 2: **9 colaboradores, 24,51 h al 50% y 97,79 h al 100%**, que valen **USD 1.908,96** (redondeando POR PERSONA, que es como se guarda en HE_Detalle; sumar los minutos y redondear una vez al final da 24,52 y 97,78 y no es lo que el sistema muestra). Estos numeros son la asercion de corte del Task 1.

---

## File Structure

| Archivo | Responsabilidad |
|---|---|
| `docs/sql/2026-09-16-horas-extras-fase4.sql` | **Crear.** Esquema, relleno, procedimiento nuevo y los cuatro modificados, con su bloque de verificacion. |
| `docs/sql/2026-09-16-horas-extras-fase3.sql` | **Modificar.** Anadir el guard `SET NOEXEC` que ya lleva el de la fase 2. |
| `CapaEntidad/EntHePeriodo.cs` | **Modificar.** `Anio`/`Mes` -> `FechaInicio`/`FechaFin`. |
| `CapaEntidad/EntHeFila.cs` | **Modificar.** Anadir `HorasOrigen`. |
| `CapaDato/DaoHorasExtras.cs` | **Modificar.** Firmas, lectura de las columnas nuevas, y `LeerHorasAprobadas`. |
| `CapaNegocio/NegHorasExtrasPantalla.cs` | **Modificar.** Se va `UltimoDiaDelMes`; `AbrirPeriodo` recibe el rango y siembra. |
| `CapaNegocio/NegHeExportacion.cs` | **Modificar.** Nombre de archivo y etiqueta de cabecera. |
| `ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs` | **Modificar.** Lee dos fechas en vez de dos enteros. |
| `ReporteTareas/Formulario/HorasExtras.aspx` | **Modificar.** Dos selectores de fecha; columna de origen en la grilla. |
| `ReporteTareas/js/horasExtras.js` | **Modificar.** Envia fechas; pinta el origen. |
| `CapaPruebas/NegHorasExtrasPantallaTests.cs` | **Modificar.** Pruebas del rango y de la siembra. |

---

### Task 1: el script de base de datos

**Files:**
- Create: `docs/sql/2026-09-16-horas-extras-fase4.sql`
- Modify: `docs/sql/2026-09-16-horas-extras-fase3.sql`

**Interfaces:**
- Produces: la tabla `HE_Periodo` con `FechaInicio`/`FechaFin` y sin `Anio`/`Mes`; `HE_Detalle.HorasOrigen`; el procedimiento `Sp_RTA_HeHorasAprobadas`; `Sp_RTA_HeCrearPeriodo`, `Sp_RTA_HeCargarPeriodo`, `Sp_RTA_HeListarPeriodos` y `Sp_RTA_HeGuardarFila` modificados.

**Contexto que el brief no puede saber:** el script **no se ejecuta aqui**. El controlador lo corre contra produccion y verifica. Tu entregas el archivo y el reporte.

- [ ] **Step 1: el encabezado y el guard**

El script empieza con un comentario que diga que hace y en que orden, y con el mismo guard que lleva el de la fase 2: si `HE_Periodo` ya tiene la columna `FechaInicio`, saltar el script entero.

```sql
/* Horas Extras fase 4 -- 2026-09-16
   1. HE_Periodo pasa de (Anio, Mes) a (FechaInicio, FechaFin).
   2. HE_Detalle gana HorasOrigen: de donde salieron las horas de la fila.
   3. Sp_RTA_HeHorasAprobadas agrega las horas aprobadas de R_DetTareasAranda.
   4. Los cuatro procedimientos que hablaban de mes pasan a hablar de rango.
   Re-correrlo es inofensivo: el guard de abajo lo salta entero. */

IF EXISTS (SELECT 1 FROM sys.columns
            WHERE object_id = OBJECT_ID('dbo.HE_Periodo') AND name = 'FechaInicio')
BEGIN
    PRINT 'La fase 4 ya esta aplicada. No se hace nada.';
    SET NOEXEC ON;
END
GO
```

Y `SET NOEXEC OFF;` como ultima linea del archivo.

- [ ] **Step 2: el esquema de HE_Periodo**

Anadir las columnas como `NULL`, rellenarlas desde `Anio`/`Mes`, ponerlas `NOT NULL`, cambiar el indice unico y **eliminar `Anio` y `Mes`**. Se eliminan a proposito: dejar las cuatro columnas seria tener dos respuestas para «que periodo es este», y la que manda seria distinta segun quien pregunte.

```sql
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

DROP INDEX UX_HE_Periodo_AnioMes ON dbo.HE_Periodo;
CREATE UNIQUE INDEX UX_HE_Periodo_Rango ON dbo.HE_Periodo (FechaInicio, FechaFin);
GO

ALTER TABLE dbo.HE_Periodo DROP COLUMN Anio, Mes;
GO
```

- [ ] **Step 3: HorasOrigen en HE_Detalle**

`VARCHAR(10) NOT NULL` con default `'Tareas'`. Las 124 filas que ya existen se rellenan con `'Tareas'` porque **estan todas en cero y sin observacion** — se comprobo en produccion el 2026-09-16 —, asi que sembrarlas no pisa trabajo de nadie.

```sql
ALTER TABLE dbo.HE_Detalle
  ADD HorasOrigen VARCHAR(10) NOT NULL
      CONSTRAINT DF_HE_Detalle_HorasOrigen DEFAULT 'Tareas';
GO
```

- [ ] **Step 4: Sp_RTA_HeHorasAprobadas**

El procedimiento nuevo. **Solo lectura sobre `R_DetTareasAranda`.** La cadena de identidad tiene cuatro saltos y ninguno es evidente; va documentada en el propio procedimiento porque quien lo lea dentro de un ano no la va a reconstruir.

```sql
CREATE PROCEDURE dbo.Sp_RTA_HeHorasAprobadas
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    /* Las horas no son una columna: son la duracion del tramo. Det_Tiempo trae
       lo mismo ya formateado y cuadra con el DATEDIFF, pero se usa el DATEDIFF
       porque es el dato y no su presentacion.

       El puente de identidad va Id_Responsable -> Cod_Usuario -> Cedula ->
       Empleados -> IdEmpleado. HE_ColaboradorParametro.IdEmpleado NO es la
       cedula: es un codigo de 1 a 3 digitos, y cruzarlo directo contra
       R_Usuarios.Cedula da cero de 64. Empleados es el eslabon que falta.

       En este sentido -de la tarea hacia el maestro- no se duplica: cada
       Id_Responsable llega a un solo IdEmpleado. Al reves si: una cedula tiene
       seis cuentas de usuario. No invertir este JOIN.

       Estado 2 es Aprobado y Tipo 1 y 2 son 50% y 100% (tabla Catalogo,
       IdTipoCatalogo 4 y 7). Las filas con Tipo 0 quedan fuera a proposito:
       sin recargo no hay con que valorarlas. */
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
```

- [ ] **Step 5: Sp_RTA_HeCrearPeriodo con rango y sin solapamiento**

Conserva la idempotencia que ya tenia —abrir dos veces el mismo periodo devuelve el que hay— y anade el rechazo de solapamientos con `Respuestas = -5`. Sin esa validacion, dos periodos que compartan dias pagarian las mismas horas dos veces, y eso no lo detecta nadie hasta que alguien reclama su rol.

Recrear el procedimiento entero con `DROP` + `CREATE` (no `ALTER`), y recordar que el `CREATE` tiene que abrir su propio lote. La firma pasa a `@FechaInicio DATE, @FechaFin DATE, @Usuario VARCHAR(50), @Ip VARCHAR(64)`. Devuelve los mismos dos campos que hoy, `Respuestas` e `IdPeriodo`. Validaciones, en este orden:

1. `@FechaInicio > @FechaFin` -> `Respuestas = -1, IdPeriodo = 0`
2. rango identico ya existente -> `Respuestas = 0` con su `IdPeriodo` (idempotente, igual que hoy)
3. cualquier solapamiento (`@FechaInicio <= FechaFin AND FechaInicio <= @FechaFin`) -> `Respuestas = -5, IdPeriodo = 0`
4. si no, `INSERT` y devolver `Respuestas = 0` con el `SCOPE_IDENTITY()`

El orden importa: el 2 antes que el 3, porque un rango identico **tambien** se solapa consigo mismo y sin ese orden abrir dos veces el mismo periodo pasaria de inofensivo a error.

La `Descripcion` pasa a construirse del rango, `CONVERT(VARCHAR(10), @FechaInicio, 103) + ' - ' + CONVERT(VARCHAR(10), @FechaFin, 103)`. De paso arregla que hoy diga «September 2026» en ingles, porque `DATENAME(MONTH, ...)` usa el idioma del servidor.

- [ ] **Step 6: los otros tres procedimientos**

`Sp_RTA_HeCargarPeriodo`: el primer result set cambia `Anio, Mes` por `FechaInicio, FechaFin`; el segundo anade `d.HorasOrigen` **al final**, que es como se extienden los contratos posicionales.

`Sp_RTA_HeListarPeriodos`: lo mismo en su select, y el `ORDER BY` pasa a `FechaInicio DESC`.

`Sp_RTA_HeGuardarFila`: anade **al final** de la lista de parametros `@HorasOrigen VARCHAR(10) = 'Manual'`, y lo escribe en el `INSERT` y en el `UPDATE`. **El default es `'Manual'` a proposito**: durante la ventana entre este script y los binarios, el DAO viejo llama sin el parametro, y marcar esas ediciones como manuales es el lado seguro — una fila manual no se re-siembra, asi que en el peor caso se pierde una siembra, nunca una correccion.

- [ ] **Step 7: el bloque de verificacion**

Al final, con el patron `@Fallos` y **sin ningun `GO` entre el `DECLARE` y el veredicto**. Comprueba al menos:

1. `HE_Periodo` tiene `FechaInicio` y `FechaFin`, y ya no tiene `Anio` ni `Mes`.
2. Los 2 periodos existentes quedaron con rango: uno de `2026-08-01` a `2026-08-31` y otro de `2026-09-01` a `2026-09-30`.
3. `UX_HE_Periodo_AnioMes` no existe y `UX_HE_Periodo_Rango` si, y es unico.
4. `HE_Detalle.HorasOrigen` existe y las 124 filas dicen `'Tareas'`.
5. Los cinco procedimientos existen.
6. **La asercion de corte:** `Sp_RTA_HeHorasAprobadas '2026-08-17', '2026-09-15'` devuelve **9 filas**, con `SUM(Horas50) = 24.51` y `SUM(Horas100) = 97.79`. Capturar el resultado en una tabla temporal con `INSERT ... EXEC` para poder contarlo y sumarlo.
7. Que el solapamiento se rechaza: no se puede probar con un `INSERT` real porque ensuciaria produccion. Comprobar en su lugar que la definicion del procedimiento contiene el `-5`, y anotar en el reporte que la prueba de comportamiento la hace el controlador a mano.

- [ ] **Step 8: el guard en el script de la fase 3**

El de la fase 3 recrea `Sp_RTA_HeGuardarFila` sin `@HorasOrigen` y `Sp_RTA_HeCrearPeriodo` con `@Anio`/`@Mes`. Re-correrlo despues de la fase 4 romperia las dos cosas. Anadirle al principio el mismo guard, saltando el script entero si detecta la fase 4:

```sql
IF EXISTS (SELECT 1 FROM sys.columns
            WHERE object_id = OBJECT_ID('dbo.HE_Periodo') AND name = 'FechaInicio')
BEGIN
    PRINT 'La fase 4 ya esta aplicada: este script la deshace. No se ejecuta.';
    SET NOEXEC ON;
END
GO
```

Y `SET NOEXEC OFF;` al final de ese mismo archivo, si no lo tiene ya.

- [ ] **Step 9: commit**

```bash
git add docs/sql/2026-09-16-horas-extras-fase4.sql docs/sql/2026-09-16-horas-extras-fase3.sql
git commit -m "feat(horas extras): el periodo pasa a ser un rango de fechas y se leen las horas aprobadas"
```

---

### Task 2: las entidades y la capa de datos

**Files:**
- Modify: `CapaEntidad/EntHePeriodo.cs`, `CapaEntidad/EntHeFila.cs`, `CapaDato/DaoHorasExtras.cs`

**Interfaces:**
- Consumes: del Task 1, `Sp_RTA_HeHorasAprobadas(@FechaInicio, @FechaFin)`, `Sp_RTA_HeCrearPeriodo(@FechaInicio, @FechaFin, @Usuario, @Ip)`, `HorasOrigen` al final de los dos contratos.
- Produces:
  - `EntHePeriodo.FechaInicio` / `.FechaFin` (ambos `DateTime`), sin `Anio` ni `Mes`
  - `EntHeFila.HorasOrigen` (`string`, inicializada en `"Tareas"`)
  - `DaoHorasExtras.CrearPeriodo(DateTime inicio, DateTime fin, string usuario, string ip, out int idPeriodo)` -> `int`
  - `DaoHorasExtras.LeerHorasAprobadas(DateTime inicio, DateTime fin)` -> `Dictionary<long, EntHeFila>` con solo `IdEmpleado`, `Horas50` y `Horas100` puestos
  - `DaoHorasExtras.GuardarFila(...)` con un parametro mas al final, `string horasOrigen`

- [ ] **Step 1: EntHePeriodo y EntHeFila**

En `EntHePeriodo.cs`, cambiar las lineas 9 y 10:

```csharp
public DateTime FechaInicio { get; set; }
public DateTime FechaFin { get; set; }
```

En `EntHeFila.cs`, anadir la propiedad con el comentario que explica que decide:

```csharp
/// <summary>
/// De donde salieron las horas de esta fila: "Tareas" si las sembro el
/// sistema desde las aprobaciones, "Manual" si alguien las corrigio.
///
/// Decide si una reapertura la vuelve a sembrar: las manuales no se tocan.
/// Por eso se inicializa en "Tareas" y no en cadena vacia -- una fila nueva
/// que nadie ha corregido es sembrable, y una cadena vacia la dejaria en un
/// tercer estado que ninguna rama contempla.
/// </summary>
public string HorasOrigen { get; set; }
```

Inicializarla en `"Tareas"` donde la clase inicializa sus otras cadenas.

- [ ] **Step 2: no escribas una prueba que no pueda fallar**

`DaoHorasExtras` habla con la base y no se prueba con MSTest en este proyecto — no hay doble de `SqlConnection`. La verificacion de esta capa es que compila y que el Task 3 la consume. No inventes una prueba aqui.

- [ ] **Step 3: CrearPeriodo y GuardarFila**

En `DaoHorasExtras.cs` linea 40, cambiar la firma y los dos parametros (lineas 50-51) a `SqlDbType.Date`. En `GuardarFila`, anadir `@HorasOrigen` **al final** de los que ya manda.

- [ ] **Step 4: leer las columnas nuevas**

Lineas 369-370, donde hoy lee `Anio` y `Mes`:

```csharp
p.FechaInicio = Convert.ToDateTime(dr["FechaInicio"]);
p.FechaFin = Convert.ToDateTime(dr["FechaFin"]);
```

Y donde arma la fila desde `CargarPeriodo` (cerca de la linea 295), anadir `f.HorasOrigen = Texto(dr, "HorasOrigen");`.

- [ ] **Step 5: LeerHorasAprobadas**

```csharp
/// <summary>
/// Las horas extras aprobadas en el rango, agregadas por colaborador. Devuelve
/// solo a quien tiene alguna: el que no aparece es que no registro ninguna, y
/// quien llama lo traduce a cero.
/// </summary>
public static Dictionary<long, EntHeFila> LeerHorasAprobadas(DateTime inicio, DateTime fin)
```

Que abra la conexion, llame a `Sp_RTA_HeHorasAprobadas` y llene el diccionario. Seguir el mismo estilo de conexion y de manejo de errores que el resto del archivo.

- [ ] **Step 6: compilar**

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaDato\CapaDato.csproj /t:Build /v:minimal
```

`CapaNegocio` va a seguir rota en este punto: es esperado, lo arregla el Task 3.

- [ ] **Step 7: commit**

```bash
git add CapaEntidad/EntHePeriodo.cs CapaEntidad/EntHeFila.cs CapaDato/DaoHorasExtras.cs
git commit -m "feat(horas extras): las entidades y el DAO hablan de rangos y de origen de horas"
```

---

### Task 3: la logica de negocio y la siembra

**Files:**
- Modify: `CapaNegocio/NegHorasExtrasPantalla.cs`, `CapaNegocio/NegHeExportacion.cs`
- Test: `CapaPruebas/NegHorasExtrasPantallaTests.cs`

**Interfaces:**
- Consumes: todo lo que produce el Task 2.
- Produces: `NegHorasExtrasPantalla.AbrirPeriodo(DateTime inicio, DateTime fin, string usuario, string ip)` y `NegHorasExtrasPantalla.ResolverHoras(EntHeFila fila, EntHeFila anterior, EntHeFila aprobadas)`. `UltimoDiaDelMes` **deja de existir**.

- [ ] **Step 1: escribir primero las pruebas de la siembra**

Son la parte que de verdad decide el comportamiento.

```csharp
[TestMethod]
public void SiembraLaFilaNuevaConLasHorasAprobadas()
{
    EntHeFila fila = new EntHeFila();
    fila.IdEmpleado = 68;
    EntHeFila aprobadas = new EntHeFila();
    aprobadas.Horas50 = 6.98m;
    aprobadas.Horas100 = 37.82m;

    NegHorasExtrasPantalla.ResolverHoras(fila, null, aprobadas);

    Assert.AreEqual(6.98m, fila.Horas50);
    Assert.AreEqual(37.82m, fila.Horas100);
    Assert.AreEqual("Tareas", fila.HorasOrigen);
}

[TestMethod]
public void NoPisaLaCorreccionManualAlReabrir()
{
    EntHeFila fila = new EntHeFila();
    EntHeFila anterior = new EntHeFila();
    anterior.Horas50 = 3m;
    anterior.Horas100 = 0m;
    anterior.HorasOrigen = "Manual";
    EntHeFila aprobadas = new EntHeFila();
    aprobadas.Horas50 = 6.98m;
    aprobadas.Horas100 = 37.82m;

    NegHorasExtrasPantalla.ResolverHoras(fila, anterior, aprobadas);

    Assert.AreEqual(3m, fila.Horas50);
    Assert.AreEqual(0m, fila.Horas100);
    Assert.AreEqual("Manual", fila.HorasOrigen);
}

[TestMethod]
public void VuelveASembrarLaFilaQueSeguiaSiendoDeTareas()
{
    /* Las aprobaciones llegan tarde: al 2026-09-16 el 80% de las horas del
       periodo seguia en Solicitado. Reabrir es como entran las que se
       aprobaron despues. */
    EntHeFila fila = new EntHeFila();
    EntHeFila anterior = new EntHeFila();
    anterior.Horas50 = 2m;
    anterior.HorasOrigen = "Tareas";
    EntHeFila aprobadas = new EntHeFila();
    aprobadas.Horas50 = 9m;

    NegHorasExtrasPantalla.ResolverHoras(fila, anterior, aprobadas);

    Assert.AreEqual(9m, fila.Horas50);
    Assert.AreEqual("Tareas", fila.HorasOrigen);
}

[TestMethod]
public void SinHorasAprobadasLaFilaQuedaEnCeroYSigueSiendoSembrable()
{
    EntHeFila fila = new EntHeFila();

    NegHorasExtrasPantalla.ResolverHoras(fila, null, null);

    Assert.AreEqual(0m, fila.Horas50);
    Assert.AreEqual(0m, fila.Horas100);
    Assert.AreEqual("Tareas", fila.HorasOrigen);
}

[TestMethod]
public void LaObservacionSePreservaAunqueSeVuelvaASembrar()
{
    EntHeFila fila = new EntHeFila();
    EntHeFila anterior = new EntHeFila();
    anterior.Observacion = "revisar con el jefe";
    anterior.HorasOrigen = "Tareas";
    EntHeFila aprobadas = new EntHeFila();
    aprobadas.Horas50 = 9m;

    NegHorasExtrasPantalla.ResolverHoras(fila, anterior, aprobadas);

    Assert.AreEqual("revisar con el jefe", fila.Observacion);
}
```

- [ ] **Step 2: correr las pruebas y verlas fallar**

Tienen que fallar por «no existe `ResolverHoras`», no por otra cosa.

- [ ] **Step 3: escribir ResolverHoras**

Metodo `public static` y **puro** —sin base, sin pantalla— para que las pruebas de arriba lo alcancen:

```csharp
/// <summary>
/// Decide con que horas se queda la fila. Tres entradas y una regla: la
/// correccion manual gana siempre; si nadie corrigio, mandan las tareas.
///
/// Que la siembra se repita en cada apertura no es un descuido: al 2026-09-16
/// el 80% de las horas del periodo seguia en "Solicitado", asi que volver a
/// abrir es como entran las que se aprobaron despues.
///
/// La Observacion se preserva pase lo que pase: es una nota de quien reviso,
/// no un numero que se recalcule.
/// </summary>
public static void ResolverHoras(EntHeFila fila, EntHeFila anterior, EntHeFila aprobadas)
{
    if (fila == null) { return; }

    if (anterior != null) { fila.Observacion = anterior.Observacion; }

    bool esManual = anterior != null
                    && string.Equals(anterior.HorasOrigen, "Manual", StringComparison.OrdinalIgnoreCase);

    if (esManual)
    {
        fila.Horas50 = anterior.Horas50;
        fila.Horas100 = anterior.Horas100;
        fila.HorasOrigen = "Manual";
        return;
    }

    fila.Horas50 = aprobadas != null ? aprobadas.Horas50 : 0m;
    fila.Horas100 = aprobadas != null ? aprobadas.Horas100 : 0m;
    fila.HorasOrigen = "Tareas";
}
```

- [ ] **Step 4: correr las pruebas y verlas pasar**

- [ ] **Step 5: AbrirPeriodo con rango**

Cambiar la firma de la linea 325 a `AbrirPeriodo(DateTime inicio, DateTime fin, string usuario, string ip)`. Dentro:

- `DaoHorasExtras.CrearPeriodo(inicio, fin, usuario, ip, out idPeriodo)`. Si devuelve `-5`, el mensaje es **«El rango se cruza con otro periodo ya abierto.»** con `tipoMensaje = "warning"`, no el generico de «No se pudo abrir el periodo»: son cosas distintas y quien lo vea tiene que saber cual le paso.
- `DateTime corte = fin;` — **borrar `UltimoDiaDelMes`** (lineas 52-54) y su otro uso en la linea 545, que pasa a `pantalla.Periodo.FechaFin`.
- Antes del bucle, `Dictionary<long, EntHeFila> aprobadas = DaoHorasExtras.LeerHorasAprobadas(inicio, fin);`
- Dentro del bucle, sustituir el bloque `if (anterior != null) { fila.Horas50 = ...; fila.Horas100 = ...; fila.Observacion = ...; }` por la llamada a `ResolverHoras(fila, anterior, aprobadas.ContainsKey(fila.IdEmpleado) ? aprobadas[fila.IdEmpleado] : null)`.
- La llamada a `GuardarFila` pasa `fila.HorasOrigen`.

- [ ] **Step 6: FilaSinCambios tiene que comparar HorasOrigen**

Linea 184 y alrededores. `FilaSinCambios` compara exactamente las columnas que escribe el `UPDATE`; ahora son una mas. **Si no se anade, una fila que solo cambia de origen no se escribe y la siembra se pierde en silencio.** Anadir la comparacion y actualizar el comentario que dice cuantas columnas son.

- [ ] **Step 7: GuardarHoras marca Manual**

El camino que usa una persona editando —el que llama con `@Auditar = 1`— tiene que pasar `"Manual"`. Es lo que hace que la correccion sobreviva a la siguiente apertura.

- [ ] **Step 8: la exportacion**

`NegHeExportacion.cs` lineas 104-105, el nombre del archivo, y la 198, la etiqueta de cabecera. Pasan a usar el rango: `yyyyMMdd_yyyyMMdd` para el nombre y `dd/MM/yyyy - dd/MM/yyyy` para la etiqueta, ambos con `CultureInfo.InvariantCulture`. Anadir `HorasOrigen` como columna al final de la hoja de detalle, para que Nomina vea cuales vinieron de las tareas y cuales reviso una persona.

Las constantes publicas `FilaDeEstado`, `PrimeraFilaDeDatos`, `ColumnaTotalHE`, `NombreHojaDetalle` y `NombreHojaPagos` existen porque las pruebas dependen de ellas: si anades una columna, revisa si alguna se corre.

- [ ] **Step 9: compilar y correr toda la suite**

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /t:Build /v:minimal
```

Las 166 pruebas que ya existian (linea base medida el 2026-09-16: 166 correctas, 0 fallos) **tienen que seguir pasando**, mas las 5 nuevas. Si alguna de las viejas falla por la firma de `AbrirPeriodo`, arreglala; si falla por el calculo, para y reportalo: eso seria una regresion de verdad.

- [ ] **Step 10: commit**

```bash
git add CapaNegocio/ CapaPruebas/
git commit -m "feat(horas extras): al abrir el periodo se siembran las horas aprobadas del rango"
```

---

### Task 4: la pantalla

**Files:**
- Modify: `ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs`, `ReporteTareas/Formulario/HorasExtras.aspx`, `ReporteTareas/js/horasExtras.js`

**Interfaces:**
- Consumes: `NegHorasExtrasPantalla.AbrirPeriodo(DateTime, DateTime, string, string)`; las filas que llegan traen `HorasOrigen`.

- [ ] **Step 1: el handler**

Lineas 134-136. Leer dos fechas en vez de dos enteros, con un auxiliar `Fecha(...)` que devuelva `DateTime.MinValue` si no parsea, en el mismo estilo que los `Entero(...)` y `Decimal(...)` que ya tiene. Si cualquiera de las dos no parsea, responder `estado = "0"` con **«Indique la fecha de inicio y la de fin del periodo.»** sin llamar a `CapaNegocio`. Parsear con `CultureInfo.InvariantCulture` y formato `yyyy-MM-dd`, que es lo que manda un `<input type="date">`.

Los siete bloques `try`/`catch` que ya tiene el archivo son el patron: no lo cambies.

- [ ] **Step 2: el ASPX**

Sustituir `#inAnioAbrir` (linea 127) y `#inMesAbrir` (lineas 131-144) por dos `<input type="date">` con `id="inFechaInicio"` e `id="inFechaFin"`, con sus etiquetas «Desde» y «Hasta». Anadir a la grilla una columna «Origen» al final de las que ya hay.

**Guardar el archivo con BOM UTF-8**: sin el, las tildes de «período» salen rotas en produccion.

- [ ] **Step 3: el JS**

`AbrirPeriodoSeleccionado()` (linea 150): leer los dos `input type="date"`, validar que las dos tienen valor y que inicio no es posterior a fin, y mandar `PostHE("AbrirPeriodo", { fechaInicio: ..., fechaFin: ... }, ...)`.

Al pintar cada fila, mostrar el origen como etiqueta: `Tareas` en gris discreto, `Manual` en azul. Lo que le importa a quien mira es distinguir de un vistazo lo que reviso una persona.

**Cuidado con el bucle de pintado:** repintar la grilla entera dentro del bucle de guardado borra las filas sin guardar. Si tocas `PintarPantalla`, comprueba que sigue llamandose una sola vez al final.

- [ ] **Step 4: compilar y comprobar que la suite sigue verde**

- [ ] **Step 5: commit**

```bash
git add ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs ReporteTareas/Formulario/HorasExtras.aspx ReporteTareas/js/horasExtras.js
git commit -m "feat(horas extras): la pantalla abre el periodo por rango y muestra el origen de las horas"
```

---

### Task 5: el paquete de despliegue

**Files:**
- Modify: `DESPLIEGUE.md`, y el paquete bajo `ReporteTareas/obj/.../PackageTmp`

- [ ] **Step 1: documentar el orden**

En `DESPLIEGUE.md`, la fase 4: primero `docs/sql/2026-09-16-horas-extras-fase4.sql`, despues los binarios. Anotar **por que el default de `@HorasOrigen` es `'Manual'`** y que el script de la fase 3 ahora lleva guard.

- [ ] **Step 2: regenerar el paquete**

**Este es el ultimo paso y se hace despues del ultimo commit de codigo.** El paquete no se olvida: se hace demasiado pronto y queda viejo igual. Comprobarlo buscando dentro del paquete algo escrito en el ultimo commit — no que los archivos existan.

Ojo con el disco: estaba al 100% y eso ya rompio el empaquetado dos veces. El `.zip` es gitignored y se puede borrar; `PackageTmp` es lo que se versiona.

- [ ] **Step 3: commit**

```bash
git add DESPLIEGUE.md ReporteTareas/obj
git commit -m "build: paquete de despliegue con la fase 4"
```
