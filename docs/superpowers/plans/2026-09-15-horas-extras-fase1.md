# Horas Extras — Fase 1 · Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que los 64 colaboradores, sus 69 sueldos y los 7 parámetros estén cargados en producción y que el cálculo de horas extras esté probado contra los casos de aceptación — todo sin que exista aún ninguna pantalla.

**Architecture:** El cálculo es una clase pura en `CapaNegocio`, sin base de datos ni UI, cubierta por diez pruebas que son el contrato. Los datos viven en seis tablas nuevas con prefijo `HE_` que cuelgan de `Empleados` por `IdEmpleado`. La carga de personas se **genera** desde la plantilla Excel hacia un archivo que nunca entra al repositorio: lo que se versiona es el generador, no los datos.

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.8 en el web, 4.6.1 en las capas), SQL Server, MSTest v1 (ensamblado de VS2019, sin NuGet), `vstest.console.exe`. Python 3 sólo para el generador de la carga, fuera de la solución.

**Spec:** `docs/superpowers/specs/2026-09-15-horas-extras-design.md`

**Especificación funcional de origen:** `Actualizacion/ESPEC_MODULO_HORAS_EXTRAS.md`. Cuando este plan diga «el §N funcional», es ese archivo.

## Global Constraints

- **Rama:** `ProyectoNuevosCambios`. No commitear ni publicar sin que el usuario lo pida.
- **EL REPOSITORIO ES PÚBLICO.** La plantilla trae nombres, cédulas y sueldos de 64 personas. **Ningún dato personal entra en un archivo versionado**: ni en un `.sql`, ni en un `.md`, ni en un mensaje de commit, ni en un reporte. Lo que se versiona es la lógica; los datos viajan por archivos ignorados.
- **Compilar con el MSBuild de VS2019**, nunca el del PATH: `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"`. En Git Bash hace falta `MSYS_NO_PATHCONV=1` y `-p:` en vez de `/p:`.
- **Las capas son .NET Framework 4.6.1: no hay `record`, ni `init`, ni expresiones `switch` modernas.** La especificación funcional usa `sealed record` en su §4; **eso no compila aquí**. Se usan clases normales con propiedades `{ get; set; }`, como el resto de `CapaEntidad`.
- **Todo monto es `decimal`, nunca `double`.**
- La dirección de las capas es `CapaNegocio → CapaDato → CapaEntidad`. `CapaDato` no referencia `CapaNegocio`: cerraría un ciclo y no compila.
- **El script SQL corre antes que los binarios**, es idempotente, y arranca con `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;` — no los toques.
- **El SQL contra producción lo ejecuta el controlador de la sesión, no el implementador.** Escribe el script, avisa, y commitea después de que te devuelvan la salida.
- Comentarios de código en español **sin tildes**; el texto de cara al usuario sí las lleva, **y habla de usted**.
- **Commitear con rutas explícitas de archivo.** Nunca `git add` de directorio: el repositorio versiona carpetas `bin/`.
- **El commit termina con `Co-Authored-By: <tu modelo> <noreply@anthropic.com>`**, y su cuerpo explica **por qué**, no qué archivos tocaste.
- Estilo SQL de la casa: `INT IDENTITY(1,1)`, constraints con nombre, `DATETIME2(0)` con `SYSDATETIME()`, **sin claves foráneas**.

## Terreno ya verificado — no lo vuelvas a consultar

| Dato | Valor |
|---|---|
| Cédulas de la plantilla presentes en `Empleados` | **64 de 64**, únicas en esa tabla |
| Cédulas con dígito verificador ecuatoriano válido | 64 de 64 |
| Una cédula que en `R_Usuarios` comparten 6 usuarios activos | sí, **1** — y es la única de las 64 sin `Empleados.Cod_Usuario` |
| Filas de la plantilla | `Parametros` 7 · `Colaboradores` 64 · `Salarios` 69 · `Horas_Periodo` 61, **todas con horas en 0** |
| Jornada declarada | 56 de 8 h/día, 8 de 4 h/día. Ninguna con divisor manual |
| Elegibilidad | 59 `Activo`, 2 `Inactivo`, 1 `NoAplicaHorasExtras`, 2 `EnRevisionSalarial` |
| Sueldos que ya existan en la base | **ninguno, en ninguna tabla** |
| Columnas útiles de `Empleados` | `IdEmpleado` (bigint, PK), `Cedula`, `Nombre`, `Sociedad`, `Ciudad`, `AreaTrabajo`, `PuestoTrabajo`, `Cod_Usuario` |

## Estructura de archivos

**Se crean:**

| Archivo | Responsabilidad |
|---|---|
| `CapaEntidad/EntHeParametros.cs` | Los 7 parámetros de cálculo, ya resueltos a números |
| `CapaEntidad/EntHeInsumo.cs` | Lo que entra al cálculo de una fila |
| `CapaEntidad/EntHeResultado.cs` | Lo que sale |
| `CapaEntidad/EntHeSalario.cs` | Una fila del historial de sueldos |
| `CapaNegocio/NegHorasExtras.cs` | El cálculo. Clase pura, sin base ni UI |
| `CapaPruebas/NegHorasExtrasTests.cs` | Las diez pruebas que son el contrato |
| `docs/sql/2026-09-15-horas-extras-fase1.sql` | Las seis tablas y los 7 parámetros. **Sin un solo dato personal** |
| `docs/sql/generar-carga-horas-extras.py` | Lee la plantilla y **genera** el script de carga. Versionado porque es lógica; su salida, no |
| `docs/sql/2026-09-15-horas-extras-verificacion.sql` | Comprueba la carga por conteos, sin mostrar a nadie |

**Se modifican:**

| Archivo | Cambio |
|---|---|
| `.gitignore` | Ignorar `Actualizacion/` y la carpeta de cargas generadas |
| `CapaEntidad/CapaEntidad.csproj` · `CapaNegocio/CapaNegocio.csproj` · `CapaPruebas/CapaPruebas.csproj` | Registrar los archivos nuevos |

---

### Task 1: Proteger los datos personales antes que nada

**Files:**
- Modify: `.gitignore`

Esta tarea va primera porque hasta que esté hecha, cualquier `git add -A` de cualquiera publica los sueldos de 64 personas en un repositorio público de GitHub.

**Interfaces:**
- Produces: la carpeta `Actualizacion/` y `docs/sql/carga-generada/` quedan fuera de git.

- [ ] **Step 1: Comprobar el riesgo actual**

```bash
git check-ignore -v Actualizacion/Plantilla_Carga_Modulo_HE.xlsx || echo "NO IGNORADA - este es el problema"
git status --porcelain Actualizacion/
```

Esperado: dice `NO IGNORADA` y `git status` muestra `?? Actualizacion/`. Pega la salida en el reporte: es la prueba de que el problema existía.

- [ ] **Step 2: Añadir las dos reglas**

Al final de `.gitignore`, añade:

```gitignore

# Insumos de carga con datos personales. La plantilla de horas extras trae
# nombres, cedulas y sueldos de 64 personas y este repositorio es PUBLICO.
# Los archivos entran por aqui, se procesan, y nunca se versionan.
Actualizacion/

# Los scripts de carga que genera docs/sql/generar-carga-horas-extras.py.
# Se versiona el generador -que es logica- y no su salida -que son datos-.
docs/sql/carga-generada/
```

- [ ] **Step 3: Comprobar que ahora sí protege**

```bash
git check-ignore -v Actualizacion/Plantilla_Carga_Modulo_HE.xlsx
git status --porcelain Actualizacion/ ; echo "(vacio = protegida)"
```

Esperado: la primera línea nombra la regla de `.gitignore` que la atrapa; la segunda no devuelve nada. Pega las dos salidas.

- [ ] **Step 4: Comprobar que no se coló nada antes**

```bash
git log --all --diff-filter=A --name-only --format="" -- "Actualizacion/*" | head
echo "(vacio = nunca se commiteo nada de esa carpeta)"
```

Si esto devuelve algo, **para y avísame**: significa que ya hay datos personales en el historial del repositorio público y eso no se arregla con un `.gitignore`.

- [ ] **Step 5: Commit**

```bash
git add .gitignore
git commit -m "chore: los insumos de carga con datos personales quedan fuera de git"
```

---

### Task 2: El cálculo y sus diez pruebas

**Files:**
- Create: `CapaEntidad/EntHeParametros.cs`, `CapaEntidad/EntHeInsumo.cs`, `CapaEntidad/EntHeResultado.cs`, `CapaEntidad/EntHeSalario.cs`, `CapaNegocio/NegHorasExtras.cs`, `CapaPruebas/NegHorasExtrasTests.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj`, `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

Esta tarea va antes que el SQL a propósito: no depende de la base, es la parte donde los números pueden salir mal, y es la única que se puede probar de verdad.

**Interfaces:**
- Produces: `NegHorasExtras.Calcular(EntHeInsumo, EntHeParametros) → EntHeResultado` y `NegHorasExtras.SalarioVigente(List<EntHeSalario>, DateTime) → decimal`. Las fases 2 y 3 llaman a esas dos.

- [ ] **Step 1: Escribir las pruebas que fallan**

Crea `CapaPruebas/NegHorasExtrasTests.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CapaPruebas
{
    /// <summary>
    /// El contrato del calculo de horas extras.
    ///
    /// Los ocho primeros casos son los criterios de aceptacion del documento
    /// funcional, con cifras verificables contra el Excel que este modulo
    /// reemplaza. Los dos ultimos no estan en ese documento y son los que
    /// fallan en silencio: el momento del redondeo y como se suma el total.
    /// Un error en cualquiera de los diez no lanza excepcion: paga mal.
    /// </summary>
    [TestClass]
    public class NegHorasExtrasTests
    {
        private static EntHeParametros Par()
        {
            return new EntHeParametros
            {
                DiasMes = 30,
                HorasMesJornadaCompleta = 240,
                Factor50 = 1.50m,
                Factor100 = 2.00m,
                TopeDiario50 = 4,
                TopeSemanal50 = 12,
                DecimalesMonto = 2
            };
        }

        private static EntHeInsumo Insumo(decimal salario, int jornada, decimal h50, decimal h100,
                                          bool aplica = true, int? divisorManual = null)
        {
            return new EntHeInsumo
            {
                SalarioBaseVigente = salario,
                JornadaHorasDia = jornada,
                DivisorManual = divisorManual,
                AplicaHE = aplica,
                Horas50 = h50,
                Horas100 = h100
            };
        }

        /* --------------------------------- criterios de aceptacion ------ */

        [TestMethod]
        public void Caso1_Salario1200_Jornada8_Diez_Horas_Al50()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 8, 10m, 0m), Par());

            Assert.AreEqual(240, r.Divisor);
            Assert.AreEqual(5.00m, r.ValorHoraOrdinaria);
            Assert.AreEqual(7.50m, r.ValorHora50);
            Assert.AreEqual(75.00m, r.Total50);
        }

        [TestMethod]
        public void Caso2_Salario1200_Jornada8_Diez_Horas_Al100()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 8, 0m, 10m), Par());

            Assert.AreEqual(10.00m, r.ValorHora100);
            Assert.AreEqual(100.00m, r.Total100);
        }

        [TestMethod]
        public void Caso3_Salario600_Ocho_Al50_Y_Cuatro_Al100()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(600m, 8, 8m, 4m), Par());

            Assert.AreEqual(2.50m, r.ValorHoraOrdinaria);
            Assert.AreEqual(30.00m, r.Total50);
            Assert.AreEqual(20.00m, r.Total100);
            Assert.AreEqual(50.00m, r.TotalHE);
        }

        /// <summary>
        /// Media jornada. Es el caso de las 8 personas que trabajan 4 h/dia, y
        /// el unico donde el divisor no es 240. 329/120 = 2.741666... y el
        /// total sale 24.675, que redondeado a dos decimales alejandose del
        /// cero da 24.68 y no 24.67.
        /// </summary>
        [TestMethod]
        public void Caso4_MediaJornada_Divisor120()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(329m, 4, 6m, 0m), Par());

            Assert.AreEqual(120, r.Divisor);
            Assert.AreEqual(2.741667m, r.ValorHoraOrdinaria);
            Assert.AreEqual(4.112500m, r.ValorHora50);
            Assert.AreEqual(24.68m, r.Total50);
        }

        [TestMethod]
        public void Caso5_SinHoras_TodoEnCero_SinError()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(2500m, 8, 0m, 0m), Par());

            Assert.AreEqual(0.00m, r.Total50);
            Assert.AreEqual(0.00m, r.Total100);
            Assert.AreEqual(0.00m, r.TotalHE);
        }

        /// <summary>
        /// La pantalla no deberia dejar cargar horas a quien no aplica, pero el
        /// calculo no confia en la pantalla: con AplicaHE en false los totales
        /// son cero aunque lleguen horas.
        /// </summary>
        [TestMethod]
        public void Caso6_NoAplica_ConHorasCargadas_PagaCero()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 8, 10m, 0m, aplica: false), Par());

            Assert.AreEqual(0.00m, r.Total50);
            Assert.AreEqual(0.00m, r.Total100);
            Assert.AreEqual(0.00m, r.TotalHE);
        }

        /// <summary>
        /// Equivalente al IFERROR del Excel: en vez de reventar, la hora vale
        /// cero y la fila queda marcada para que alguien la mire.
        /// </summary>
        [TestMethod]
        public void Caso7_SalarioCero_HoraOrdinariaCero()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(0m, 8, 10m, 0m), Par());

            Assert.AreEqual(0m, r.ValorHoraOrdinaria);
            Assert.AreEqual(0.00m, r.TotalHE);
            Assert.IsTrue(r.TieneAdvertencia);
        }

        [TestMethod]
        public void Caso7b_DivisorCero_NoLanzaYMarcaAdvertencia()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 0, 10m, 0m), Par());

            Assert.AreEqual(0m, r.ValorHoraOrdinaria);
            Assert.AreEqual(0.00m, r.TotalHE);
            Assert.IsTrue(r.TieneAdvertencia);
        }

        /// <summary>
        /// El ajuste salarial manda sobre el sueldo del rol. En el Excel esto
        /// era una columna llamada con el nombre de un mes; aqui es un
        /// historial con fecha de vigencia y gana el mas reciente que no sea
        /// posterior al corte.
        /// </summary>
        [TestMethod]
        public void Caso8_ConAjusteSalarial_UsaElAjuste()
        {
            List<EntHeSalario> historial = new List<EntHeSalario>
            {
                new EntHeSalario { Monto = 1200m, FechaVigenciaDesde = new DateTime(2026, 1, 1), Origen = "Rol" },
                new EntHeSalario { Monto = 1500m, FechaVigenciaDesde = new DateTime(2026, 9, 1), Origen = "Ajuste" }
            };

            Assert.AreEqual(1500m, NegHorasExtras.SalarioVigente(historial, new DateTime(2026, 9, 30)));
        }

        [TestMethod]
        public void Caso8b_AjustePosteriorAlCorte_NoSeUsa()
        {
            List<EntHeSalario> historial = new List<EntHeSalario>
            {
                new EntHeSalario { Monto = 1200m, FechaVigenciaDesde = new DateTime(2026, 1, 1), Origen = "Rol" },
                new EntHeSalario { Monto = 1500m, FechaVigenciaDesde = new DateTime(2026, 10, 1), Origen = "Ajuste" }
            };

            Assert.AreEqual(1200m, NegHorasExtras.SalarioVigente(historial, new DateTime(2026, 9, 30)));
        }

        /* ------------------- los dos que el documento funcional no tiene -- */

        /// <summary>
        /// El redondeo va SOLO al final. Si alguien redondea el valor hora
        /// durante la cadena, este caso cambia: 100/240 = 0.416666..., por 1.5
        /// da 0.625 exacto, y por 7 horas da 4.375 -> 4.38. Redondeando el
        /// valor hora a 2 decimales primero (0.42 * 1.5 = 0.63) daria 4.41.
        /// La diferencia es de centavos por fila y de dolares por nomina.
        /// </summary>
        [TestMethod]
        public void Redondeo_SoloAlFinal_NoDuranteLaCadena()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(100m, 8, 7m, 0m), Par());

            Assert.AreEqual(4.38m, r.Total50);
        }

        /// <summary>
        /// El total del periodo es la suma de los totales YA redondeados, no el
        /// redondeo de la suma. Asi el gran total cuadra con lo que se ve en
        /// pantalla sumado a mano. Tres filas de 0.125 redondean a 0.13 cada
        /// una y suman 0.39; sumar primero daria 0.375 -> 0.38.
        /// </summary>
        [TestMethod]
        public void TotalDelPeriodo_EsLaSumaDeLosRedondeados()
        {
            List<decimal> totalesPorFila = new List<decimal>();

            for (int i = 0; i < 3; i++)
            {
                totalesPorFila.Add(NegHorasExtras.Calcular(Insumo(20m, 8, 1m, 0m), Par()).Total50);
            }

            Assert.AreEqual(0.13m, totalesPorFila[0]);
            Assert.AreEqual(0.39m, NegHorasExtras.TotalDelPeriodo(totalesPorFila));
        }
    }
}
```

- [ ] **Step 2: Registrar la prueba en `CapaPruebas.csproj`**

Junto a `<Compile Include="EntPerfilEquipoTests.cs" />`, añade:

```xml
    <Compile Include="NegHorasExtrasTests.cs" />
```

- [ ] **Step 3: Correr y verificar que NO compila**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
```

Esperado: **no compila**, con `El nombre del tipo o del espacio de nombres 'EntHeParametros' no existe`.

- [ ] **Step 4: Las cuatro entidades**

Crea `CapaEntidad/EntHeParametros.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Los parametros de calculo vigentes, ya resueltos a numeros.
    ///
    /// Viven en tabla y no en codigo -HE_Parametro- porque cambian sin que
    /// nadie recompile, y estan versionados por fecha de vigencia para que un
    /// periodo cerrado se pueda recalcular con los que regian entonces.
    /// </summary>
    public class EntHeParametros
    {
        public int DiasMes { get; set; }
        public int HorasMesJornadaCompleta { get; set; }
        public decimal Factor50 { get; set; }
        public decimal Factor100 { get; set; }
        public int TopeDiario50 { get; set; }
        public int TopeSemanal50 { get; set; }

        /// <summary>
        /// Decimales a los que se redondean los montos que se pagan. Es
        /// parametro y no constante porque el documento funcional lo declara
        /// asi, y si va a estar en la tabla tiene que mandar de verdad: un
        /// parametro configurable que el codigo ignora es peor que no tenerlo.
        /// </summary>
        public int DecimalesMonto { get; set; }
    }
}
```

Crea `CapaEntidad/EntHeInsumo.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>Lo que entra al calculo de una fila. Ya resuelto: aqui no se consulta nada.</summary>
    public class EntHeInsumo
    {
        public decimal SalarioBaseVigente { get; set; }
        public int JornadaHorasDia { get; set; }

        /// <summary>
        /// Si tiene valor, manda sobre la formula. Nace nulo para todos: el
        /// divisor de jornada parcial esta pendiente de validar con Legal y
        /// afecta a 8 personas reales.
        /// </summary>
        public int? DivisorManual { get; set; }

        public bool AplicaHE { get; set; }
        public decimal Horas50 { get; set; }
        public decimal Horas100 { get; set; }
    }
}
```

Crea `CapaEntidad/EntHeResultado.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Lo que sale del calculo de una fila.
    ///
    /// Los tres valores hora llevan seis decimales a proposito: son
    /// intermedios y no se redondean durante la cadena. Solo los totales se
    /// redondean, y a dos decimales.
    /// </summary>
    public class EntHeResultado
    {
        public int Divisor { get; set; }
        public decimal ValorHoraOrdinaria { get; set; }
        public decimal ValorHora50 { get; set; }
        public decimal ValorHora100 { get; set; }
        public decimal Total50 { get; set; }
        public decimal Total100 { get; set; }
        public decimal TotalHoras { get; set; }
        public decimal TotalHE { get; set; }

        /// <summary>
        /// El salario o el divisor son cero y la hora ordinaria salio en cero.
        /// La fila se muestra en rojo y bloquea el cierre del periodo: no es un
        /// error de programa, es un dato que alguien tiene que arreglar.
        /// </summary>
        public bool TieneAdvertencia { get; set; }
    }
}
```

Crea `CapaEntidad/EntHeSalario.cs`:

```csharp
using System;

namespace CapaEntidad
{
    /// <summary>
    /// Una fila del historial de sueldos. En el Excel esto era una columna
    /// llamada con el nombre de un mes; aqui cada monto tiene su fecha de
    /// vigencia y el calculo toma el vigente al corte del periodo.
    /// </summary>
    public class EntHeSalario
    {
        public decimal Monto { get; set; }
        public DateTime FechaVigenciaDesde { get; set; }

        /// <summary>"Rol" o "Ajuste". Informativo: el que manda es el mas reciente, venga de donde venga.</summary>
        public string Origen { get; set; } = "";
    }
}
```

- [ ] **Step 5: Registrar las cuatro en `CapaEntidad.csproj`**

Junto a los demás `Compile Include`, añade:

```xml
    <Compile Include="EntHeInsumo.cs" />
    <Compile Include="EntHeParametros.cs" />
    <Compile Include="EntHeResultado.cs" />
    <Compile Include="EntHeSalario.cs" />
```

- [ ] **Step 6: El cálculo**

Crea `CapaNegocio/NegHorasExtras.cs`:

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// El calculo de horas extras. Clase pura: no consulta la base, no sabe de
    /// pantallas, y por eso se puede probar entera sin levantar nada.
    ///
    /// Todo va en decimal y nunca en double: con double, 0.1 + 0.2 no es 0.3 y
    /// un centavo de diferencia por fila se convierte en dolares al cierre.
    /// </summary>
    public static class NegHorasExtras
    {
        /// <summary>
        /// Decimales con los que se GUARDAN los valores hora. No es la
        /// precision con la que se calculan: la cadena corre en decimal a plena
        /// precision y esto solo recorta lo que se muestra y se persiste.
        /// </summary>
        private const int DecimalesValorHora = 6;

        /// <summary>
        /// El sueldo que rige a una fecha: el mas reciente cuya vigencia no sea
        /// posterior al corte. Cero si no hay ninguno, que el calculo traduce a
        /// hora ordinaria cero y fila marcada.
        /// </summary>
        public static decimal SalarioVigente(List<EntHeSalario> historial, DateTime corte)
        {
            if (historial == null) { return 0m; }

            decimal vigente = 0m;
            DateTime mejor = DateTime.MinValue;

            foreach (EntHeSalario s in historial)
            {
                if (s == null) { continue; }
                if (s.FechaVigenciaDesde > corte) { continue; }

                /* >= y no >: si dos montos comparten fecha de vigencia, gana el
                   ultimo de la lista, que es el que la consulta trae mas
                   reciente. Es arbitrario pero tiene que ser estable. */
                if (s.FechaVigenciaDesde >= mejor)
                {
                    mejor = s.FechaVigenciaDesde;
                    vigente = s.Monto;
                }
            }

            return vigente;
        }

        /// <summary>
        /// El divisor mensual de horas.
        ///
        /// Si hay divisor manual, manda. Si no, para jornada completa son las
        /// horas mensuales del parametro y para cualquier otra, horas por dia
        /// por dias del mes. Para 8 h/dia las dos ramas dan 240; la condicion se
        /// conserva para que el origen de ese 240 sea explicito y auditable.
        /// </summary>
        private static int Divisor(EntHeInsumo i, EntHeParametros p)
        {
            if (i.DivisorManual.HasValue && i.DivisorManual.Value > 0)
            {
                return i.DivisorManual.Value;
            }

            if (i.JornadaHorasDia == 8) { return p.HorasMesJornadaCompleta; }

            return i.JornadaHorasDia * p.DiasMes;
        }

        /// <summary>Redondeo de pago: alejandose del cero, que es como se paga.</summary>
        private static decimal Monto(decimal v, EntHeParametros p)
        {
            return Math.Round(v, p.DecimalesMonto, MidpointRounding.AwayFromZero);
        }

        /// <summary>Calcula una fila. No lanza nunca: un dato malo se marca, no revienta.</summary>
        public static EntHeResultado Calcular(EntHeInsumo insumo, EntHeParametros parametros)
        {
            EntHeResultado r = new EntHeResultado();

            if (insumo == null || parametros == null)
            {
                r.TieneAdvertencia = true;
                return r;
            }

            r.Divisor = Divisor(insumo, parametros);

            /* Equivalente al IFERROR del Excel. Sin esto, un divisor en cero
               lanzaria DivideByZeroException a media nomina. */
            if (r.Divisor <= 0 || insumo.SalarioBaseVigente <= 0m)
            {
                r.TieneAdvertencia = true;
                r.ValorHoraOrdinaria = 0m;
                r.ValorHora50 = 0m;
                r.ValorHora100 = 0m;
                r.TotalHoras = insumo.Horas50 + insumo.Horas100;
                return r;
            }

            /* La cadena corre SIN redondear, a plena precision de decimal. El
               documento funcional lo dice con todas las letras: los valores
               intermedios "se almacenan con 6 decimales; no se redondean
               durante la cadena". Son dos cosas distintas y confundirlas
               cambia el resultado.

               Con salario 329 y divisor 120: sin redondear, la hora al 50% es
               4.1125 exactos. Redondeando la hora ordinaria a 6 decimales
               primero -2.741667- da 4.1125005, que a seis decimales es
               4.112501. Un centavo por aqui, otro por alla. */
            decimal horaOrdinaria = insumo.SalarioBaseVigente / r.Divisor;
            decimal hora50  = horaOrdinaria * parametros.Factor50;
            decimal hora100 = horaOrdinaria * parametros.Factor100;

            /* Estos tres SI se recortan, pero solo para mostrarlos y guardarlos:
               los totales de abajo se calculan con los valores sin recortar. */
            r.ValorHoraOrdinaria = Math.Round(horaOrdinaria, DecimalesValorHora, MidpointRounding.AwayFromZero);
            r.ValorHora50        = Math.Round(hora50,        DecimalesValorHora, MidpointRounding.AwayFromZero);
            r.ValorHora100       = Math.Round(hora100,       DecimalesValorHora, MidpointRounding.AwayFromZero);

            r.TotalHoras = insumo.Horas50 + insumo.Horas100;

            /* Quien no aplica no cobra, aunque lleguen horas. La pantalla no
               deberia permitir cargarlas, pero el calculo no confia en la
               pantalla. */
            if (!insumo.AplicaHE)
            {
                r.Total50 = 0m;
                r.Total100 = 0m;
                r.TotalHE = 0m;
                return r;
            }

            /* Del valor SIN recortar, no de r.ValorHora50. Esa es la diferencia
               entre "no se redondea durante la cadena" y redondear dos veces. */
            r.Total50  = Monto(insumo.Horas50  * hora50,  parametros);
            r.Total100 = Monto(insumo.Horas100 * hora100, parametros);
            r.TotalHE  = r.Total50 + r.Total100;

            return r;
        }

        /// <summary>
        /// El total del periodo: la suma de los totales YA redondeados.
        ///
        /// No es lo mismo que redondear la suma, y la diferencia se ve: asi el
        /// gran total cuadra con lo que cualquiera obtiene sumando a mano la
        /// columna de la pantalla. Si no cuadrara, la primera reaccion de quien
        /// revisa la nomina seria desconfiar del sistema entero.
        /// </summary>
        public static decimal TotalDelPeriodo(List<decimal> totalesPorFila)
        {
            if (totalesPorFila == null) { return 0m; }

            decimal suma = 0m;
            foreach (decimal t in totalesPorFila) { suma += t; }

            return suma;
        }
    }
}
```

- [ ] **Step 7: Registrar la clase en `CapaNegocio.csproj`**

Junto a `<Compile Include="NegPerfilCv.cs" />`, añade:

```xml
    <Compile Include="NegHorasExtras.cs" />
```

- [ ] **Step 8: Correr las pruebas**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: **121 pruebas, todas pasando** (109 previas + 12 nuevas). Cuenta las que informe `vstest` y contrástalas; si no coinciden, dilo en el reporte en vez de ajustar el número.

- [ ] **Step 9: Demostrar que la prueba del redondeo discrimina**

Una prueba de redondeo que nadie ha visto fallar no protege nada. Haz esto y pega las dos salidas:

1. En `NegHorasExtras.Calcular`, envuelve temporalmente el cálculo de `hora50` en un redondeo a 2 decimales, que es el error que la prueba existe para atrapar:

```csharp
decimal hora50 = Math.Round(horaOrdinaria * parametros.Factor50, 2, MidpointRounding.AwayFromZero);
```

2. Compila y corre **sólo** `Redondeo_SoloAlFinal_NoDuranteLaCadena`. Debe **FALLAR** dando 4.41 en vez de 4.38: la hora pasa de 0.625 a 0.63 y siete horas se llevan tres centavos de más.
3. Quita el redondeo, compila y corre las 121. Deben pasar.

Para correr una sola: añade `/Tests:Redondeo_SoloAlFinal_NoDuranteLaCadena`.

Si en el paso 2 pasa, avísame: la prueba no está protegiendo lo que dice.

- [ ] **Step 10: Commit**

```bash
git add CapaEntidad/EntHeParametros.cs CapaEntidad/EntHeInsumo.cs CapaEntidad/EntHeResultado.cs CapaEntidad/EntHeSalario.cs CapaEntidad/CapaEntidad.csproj CapaNegocio/NegHorasExtras.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegHorasExtrasTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(horas-extras): el calculo, con los criterios de aceptacion como pruebas"
```

---

### Task 3: Las seis tablas y los parámetros

**Files:**
- Create: `docs/sql/2026-09-15-horas-extras-fase1.sql`

**Este script no lleva ni un dato personal.** Crea la estructura y carga los 7 parámetros, que son números de configuración. Las 64 personas las carga la tarea 4, por otro camino.

**No ejecutes el script.** Cuando esté escrito y commiteado, dilo: lo corre el controlador de la sesión.

**Interfaces:**
- Produces: `HE_ColaboradorParametro`, `HE_Salario`, `HE_Parametro`, `HE_Periodo`, `HE_Detalle`, `HE_DetalleAuditoria`, y los 7 parámetros cargados.

- [ ] **Step 1: La cabecera**

Crea `docs/sql/2026-09-15-horas-extras-fase1.sql`:

```sql
/* ============================================================================
   Modulo de Horas Extras - FASE 1: estructura y parametros

   Crea las seis tablas y carga los 7 parametros de calculo.

   NO CARGA PERSONAS. Los 64 colaboradores, sus 69 sueldos y su elegibilidad
   viajan en un script aparte que se genera desde la plantilla Excel y que NO
   se versiona: este repositorio es publico y esos datos son nombres, cedulas
   y sueldos de gente real.

   Las tablas cuelgan de Empleados por IdEmpleado y no por cedula. La cedula
   se usa una sola vez, en la carga, y nunca mas para identificar a nadie: hay
   una que en R_Usuarios comparten seis usuarios activos distintos.

   Idempotente: se puede correr dos veces sin dano.
   ============================================================================ */

SET NOCOUNT ON;
GO
/* Explicitos y dentro del script, no como parametro de sqlcmd: DESPLIEGUE.md
   dice que esto lo corre una persona a mano y sqlcmd los deja apagados por
   omision. En el modulo de perfil eso aborto un script a media ejecucion. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
```

- [ ] **Step 2: Las tablas de catálogo**

Añade al script:

```sql
/* ------------------------------------------- 1. HE_Parametro --------------- */

IF OBJECT_ID('dbo.HE_Parametro','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_Parametro
    (
        IdParametro        INT IDENTITY(1,1) NOT NULL,
        Clave              VARCHAR(60)   NOT NULL,
        Valor              DECIMAL(18,6) NOT NULL,
        FechaVigenciaDesde DATE          NOT NULL,
        FechaVigenciaHasta DATE          NULL,

        Fec_Modificacion   DATETIME2(0)  NOT NULL
            CONSTRAINT DF_HeParametro_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion   VARCHAR(50)   NULL,
        Ip_Modificacion    VARCHAR(64)   NULL,

        CONSTRAINT PK_HE_Parametro PRIMARY KEY (IdParametro)
    );
    CREATE INDEX IX_HE_Parametro_Clave ON dbo.HE_Parametro (Clave, FechaVigenciaDesde DESC);
    PRINT 'HE_Parametro creada.';
END
ELSE PRINT 'HE_Parametro ya existia.';
GO

/* --------------------------------- 2. HE_ColaboradorParametro -------------- */

/* Lo que Empleados no tiene y este modulo necesita. Tabla satelite y no
   columnas nuevas en Empleados, que la comparte RRHHEmpleados.aspx: es el
   mismo criterio que uso el modulo de perfil.

   Empresa se guarda aqui y no se toma de Empleados.Sociedad a proposito: son
   dos clasificaciones distintas. La plantilla usa cuatro valores -separa
   "servicios profesionales"- y Sociedad tiene tres que no los distinguen. La
   que importa para pagar es la de la plantilla. */
IF OBJECT_ID('dbo.HE_ColaboradorParametro','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_ColaboradorParametro
    (
        IdColaboradorParametro INT IDENTITY(1,1) NOT NULL,
        IdEmpleado             BIGINT       NOT NULL,
        JornadaHorasDia        INT          NOT NULL,
        DivisorManual          INT          NULL,
        AplicaHE               BIT          NOT NULL,
        MotivoNoAplica         VARCHAR(40)  NULL,
        Empresa                VARCHAR(120) NULL,

        Estado                 CHAR(1)      NOT NULL
            CONSTRAINT DF_HeColabPar_Estado DEFAULT ('1'),
        Fec_Modificacion       DATETIME2(0) NOT NULL
            CONSTRAINT DF_HeColabPar_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion       VARCHAR(50)  NULL,
        Ip_Modificacion        VARCHAR(64)  NULL,

        CONSTRAINT PK_HE_ColaboradorParametro PRIMARY KEY (IdColaboradorParametro)
    );
    CREATE UNIQUE INDEX UX_HE_ColaboradorParametro_Empleado
        ON dbo.HE_ColaboradorParametro (IdEmpleado);
    PRINT 'HE_ColaboradorParametro creada.';
END
ELSE PRINT 'HE_ColaboradorParametro ya existia.';
GO

/* --------------------------------------------- 3. HE_Salario --------------- */

/* El primer lugar del sistema donde vive un sueldo. Hoy no hay ninguno en
   ninguna tabla de esta base. */
IF OBJECT_ID('dbo.HE_Salario','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_Salario
    (
        IdSalario          INT IDENTITY(1,1) NOT NULL,
        IdEmpleado         BIGINT        NOT NULL,
        Monto              DECIMAL(18,2) NOT NULL,
        FechaVigenciaDesde DATE          NOT NULL,
        Origen             VARCHAR(20)   NOT NULL,   -- Rol | Ajuste
        Observacion        VARCHAR(400)  NULL,

        Estado             CHAR(1)       NOT NULL
            CONSTRAINT DF_HeSalario_Estado DEFAULT ('1'),
        Fec_Modificacion   DATETIME2(0)  NOT NULL
            CONSTRAINT DF_HeSalario_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion   VARCHAR(50)   NULL,
        Ip_Modificacion    VARCHAR(64)   NULL,

        CONSTRAINT PK_HE_Salario PRIMARY KEY (IdSalario)
    );
    CREATE INDEX IX_HE_Salario_Vigencia
        ON dbo.HE_Salario (IdEmpleado, FechaVigenciaDesde DESC);
    PRINT 'HE_Salario creada.';
END
ELSE PRINT 'HE_Salario ya existia.';
GO
```

- [ ] **Step 3: Las tablas del período**

Añade al script:

```sql
/* --------------------------------------------- 4. HE_Periodo --------------- */

IF OBJECT_ID('dbo.HE_Periodo','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_Periodo
    (
        IdPeriodo        INT IDENTITY(1,1) NOT NULL,
        Anio             INT          NOT NULL,
        Mes              INT          NOT NULL,
        Descripcion      VARCHAR(120) NULL,
        EstadoPeriodo    VARCHAR(10)  NOT NULL,   -- Abierto | Cerrado | Anulado

        FechaCreacion    DATETIME2(0) NOT NULL
            CONSTRAINT DF_HePeriodo_FecCrea DEFAULT (SYSDATETIME()),
        UsuarioCreacion  VARCHAR(50)  NULL,
        FechaCierre      DATETIME2(0) NULL,
        UsuarioCierre    VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_HE_Periodo PRIMARY KEY (IdPeriodo)
    );
    CREATE UNIQUE INDEX UX_HE_Periodo_AnioMes ON dbo.HE_Periodo (Anio, Mes);
    PRINT 'HE_Periodo creada.';
END
ELSE PRINT 'HE_Periodo ya existia.';
GO

/* --------------------------------------------- 5. HE_Detalle --------------- */

/* Una fila por periodo y colaborador, con el SNAPSHOT congelado.

   El snapshot es lo que impide que un periodo cerrado cambie porque alguien
   edito el maestro despues. En el Excel la cedula estaba escrita a mano
   mientras el resto venia por formula: reordenar la hoja desalineaba los datos
   sin aviso. Aqui el detalle guarda su propia copia de lo que uso para
   calcular, y la relacion es por IdEmpleado. */
IF OBJECT_ID('dbo.HE_Detalle','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_Detalle
    (
        IdDetalle              INT IDENTITY(1,1) NOT NULL,
        IdPeriodo              INT           NOT NULL,
        IdEmpleado             BIGINT        NOT NULL,

        /* snapshot */
        CedulaSnapshot         VARCHAR(20)   NULL,
        NombreSnapshot         VARCHAR(400)  NULL,
        EmpresaSnapshot        VARCHAR(120)  NULL,
        CargoSnapshot          VARCHAR(200)  NULL,
        JornadaHorasDiaSnapshot INT          NOT NULL,
        SalarioBaseSnapshot    DECIMAL(18,2) NOT NULL,
        AplicaHESnapshot       BIT           NOT NULL,

        /* calculo */
        Divisor                INT           NOT NULL,
        ValorHoraOrdinaria     DECIMAL(18,6) NOT NULL,
        ValorHora50            DECIMAL(18,6) NOT NULL,
        ValorHora100           DECIMAL(18,6) NOT NULL,
        Horas50                DECIMAL(9,2)  NOT NULL CONSTRAINT DF_HeDetalle_H50  DEFAULT (0),
        Horas100               DECIMAL(9,2)  NOT NULL CONSTRAINT DF_HeDetalle_H100 DEFAULT (0),
        Total50                DECIMAL(18,2) NOT NULL CONSTRAINT DF_HeDetalle_T50  DEFAULT (0),
        Total100               DECIMAL(18,2) NOT NULL CONSTRAINT DF_HeDetalle_T100 DEFAULT (0),
        TotalHoras             DECIMAL(9,2)  NOT NULL CONSTRAINT DF_HeDetalle_TH   DEFAULT (0),
        TotalHE                DECIMAL(18,2) NOT NULL CONSTRAINT DF_HeDetalle_THE  DEFAULT (0),

        Observacion            VARCHAR(400)  NULL,
        Fec_Modificacion       DATETIME2(0)  NOT NULL
            CONSTRAINT DF_HeDetalle_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion       VARCHAR(50)   NULL,
        Ip_Modificacion        VARCHAR(64)   NULL,

        CONSTRAINT PK_HE_Detalle PRIMARY KEY (IdDetalle)
    );
    CREATE UNIQUE INDEX UX_HE_Detalle_PeriodoEmpleado
        ON dbo.HE_Detalle (IdPeriodo, IdEmpleado);
    CREATE INDEX IX_HE_Detalle_Periodo ON dbo.HE_Detalle (IdPeriodo);
    PRINT 'HE_Detalle creada.';
END
ELSE PRINT 'HE_Detalle ya existia.';
GO

/* ------------------------------------ 6. HE_DetalleAuditoria --------------- */

/* Solo sobre las dos columnas editables. Auditar las derivadas seria auditar
   una formula: se recalculan solas y su rastro es el de las horas. */
IF OBJECT_ID('dbo.HE_DetalleAuditoria','U') IS NULL
BEGIN
    CREATE TABLE dbo.HE_DetalleAuditoria
    (
        IdAuditoria      INT IDENTITY(1,1) NOT NULL,
        IdDetalle        INT           NOT NULL,
        Campo            VARCHAR(20)   NOT NULL,   -- Horas50 | Horas100
        ValorAnterior    DECIMAL(9,2)  NULL,
        ValorNuevo       DECIMAL(9,2)  NULL,
        Fecha            DATETIME2(0)  NOT NULL
            CONSTRAINT DF_HeDetAud_Fec DEFAULT (SYSDATETIME()),
        Usuario          VARCHAR(50)   NULL,
        Ip               VARCHAR(64)   NULL,

        CONSTRAINT PK_HE_DetalleAuditoria PRIMARY KEY (IdAuditoria)
    );
    CREATE INDEX IX_HE_DetalleAuditoria_Detalle ON dbo.HE_DetalleAuditoria (IdDetalle);
    PRINT 'HE_DetalleAuditoria creada.';
END
ELSE PRINT 'HE_DetalleAuditoria ya existia.';
GO
```

- [ ] **Step 4: Los siete parámetros**

Añade al script:

```sql
/* ------------------------------------- 7. parametros iniciales ------------- */

/* Los 7 valores del documento funcional, con vigencia desde el arranque del
   modulo. No son datos personales: son configuracion.

   Se insertan solo si no existe ya una vigencia para esa clave, para que
   correr el script dos veces no duplique ni pise un valor que alguien ajusto. */
DECLARE @Desde DATE = '2026-09-01';

INSERT INTO dbo.HE_Parametro (Clave, Valor, FechaVigenciaDesde, Usu_Modificacion)
SELECT v.Clave, v.Valor, @Desde, 'carga-inicial'
  FROM (VALUES
        ('DiasMes',                  30),
        ('HorasMesJornadaCompleta', 240),
        ('Factor50',                1.5),
        ('Factor100',                 2),
        ('TopeDiario50',              4),
        ('TopeSemanal50',            12),
        ('DecimalesMonto',            2)
       ) v (Clave, Valor)
 WHERE NOT EXISTS (SELECT 1 FROM dbo.HE_Parametro p
                    WHERE p.Clave = v.Clave AND p.FechaVigenciaDesde = @Desde);

PRINT 'Parametros iniciales revisados.';
GO
```

- [ ] **Step 5: Las aserciones**

Añade al final del script:

```sql
/* ------------------------------------------------- 8. aserciones ----------- */

IF OBJECT_ID('dbo.HE_Parametro','U')             IS NULL RAISERROR('FALLO: HE_Parametro no quedo creada.', 16, 1);
IF OBJECT_ID('dbo.HE_ColaboradorParametro','U')  IS NULL RAISERROR('FALLO: HE_ColaboradorParametro no quedo creada.', 16, 1);
IF OBJECT_ID('dbo.HE_Salario','U')               IS NULL RAISERROR('FALLO: HE_Salario no quedo creada.', 16, 1);
IF OBJECT_ID('dbo.HE_Periodo','U')               IS NULL RAISERROR('FALLO: HE_Periodo no quedo creada.', 16, 1);
IF OBJECT_ID('dbo.HE_Detalle','U')               IS NULL RAISERROR('FALLO: HE_Detalle no quedo creada.', 16, 1);
IF OBJECT_ID('dbo.HE_DetalleAuditoria','U')      IS NULL RAISERROR('FALLO: HE_DetalleAuditoria no quedo creada.', 16, 1);

IF (SELECT COUNT(*) FROM dbo.HE_Parametro WHERE FechaVigenciaDesde = '2026-09-01') <> 7
    RAISERROR('FALLO: no quedaron los 7 parametros con la vigencia inicial.', 16, 1);

/* Un colaborador no puede tener dos filas de parametros: el calculo tomaria
   una al azar. El indice unico lo impide, y esta asercion comprueba que el
   indice existe y no que alguien lo creo sin UNIQUE. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.HE_ColaboradorParametro')
                  AND name = 'UX_HE_ColaboradorParametro_Empleado' AND is_unique = 1)
    RAISERROR('FALLO: falta el indice UNICO por empleado en HE_ColaboradorParametro.', 16, 1);

IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.HE_Detalle')
                  AND name = 'UX_HE_Detalle_PeriodoEmpleado' AND is_unique = 1)
    RAISERROR('FALLO: falta el indice UNICO por periodo y empleado en HE_Detalle.', 16, 1);

/* Este script no puede haber cargado personas. Si lo hizo, algo se colo. */
IF (SELECT COUNT(*) FROM dbo.HE_ColaboradorParametro) > 0
   AND NOT EXISTS (SELECT 1 FROM dbo.HE_ColaboradorParametro WHERE Usu_Modificacion <> 'carga-inicial')
    PRINT 'AVISO: HE_ColaboradorParametro ya tiene filas. Vienen de la carga generada, no de este script.';

PRINT 'Horas Extras fase 1: estructura y parametros listos.';
GO
```

- [ ] **Step 6: Comprobar que no lleva datos personales**

```bash
grep -nE "[0-9]{10}" docs/sql/2026-09-15-horas-extras-fase1.sql ; echo "(vacio = ninguna cedula)"
grep -niE "nombre.*'[A-Z][a-z]+ " docs/sql/2026-09-15-horas-extras-fase1.sql ; echo "(vacio = ningun nombre)"
```

Las dos deben salir vacías. Pega la salida: es la comprobación que distingue este script de los que no se pueden versionar.

- [ ] **Step 7: Commit y avisar**

```bash
git add docs/sql/2026-09-15-horas-extras-fase1.sql
git commit -m "feat(horas-extras): las seis tablas y los parametros de calculo"
```

En el reporte escribe textualmente: **«El script `docs/sql/2026-09-15-horas-extras-fase1.sql` está listo y commiteado. No lo ejecuté. Hace falta correrlo contra producción antes de la tarea 4.»**

---

### Task 4: El generador de la carga y su verificación

**Files:**
- Create: `docs/sql/generar-carga-horas-extras.py`, `docs/sql/2026-09-15-horas-extras-verificacion.sql`

**El corazón de esta tarea es qué se versiona y qué no.** Se versiona el **generador**, que es lógica y se puede revisar. Su **salida** —el script con los 64 nombres, cédulas y sueldos— va a `docs/sql/carga-generada/`, que la tarea 1 dejó fuera de git. Si en algún momento te ves escribiendo un dato de una persona dentro de un archivo que vas a commitear, para.

**Requisito previo:** el script de la tarea 3 tiene que estar aplicado en producción. Si no lo está, di **BLOCKED**.

**No ejecutes nada contra la base.** Generas el script y lo dejas; lo corre el controlador.

**Interfaces:**
- Consumes: las seis tablas de la tarea 3.
- Produces: `docs/sql/carga-generada/carga-horas-extras.sql` (ignorado por git) y el script de verificación versionado.

- [ ] **Step 1: El generador**

Crea `docs/sql/generar-carga-horas-extras.py`:

```python
# -*- coding: utf-8 -*-
"""
Genera el script de carga del modulo de Horas Extras desde la plantilla Excel.

POR QUE EXISTE ESTE ARCHIVO Y NO UN .sql CON LOS DATOS DENTRO:
la plantilla trae nombres, cedulas y sueldos de 64 personas, y este
repositorio es PUBLICO. Se versiona la logica de la carga -este archivo, que
se puede revisar- y no su resultado, que son datos personales.

La salida va a docs/sql/carga-generada/, que esta en .gitignore.

Uso:
    python docs/sql/generar-carga-horas-extras.py

La plantilla se espera en Actualizacion/Plantilla_Carga_Modulo_HE.xlsx, que
tambien esta ignorada.
"""
import os
import re
import sys
import zipfile
import xml.etree.ElementTree as ET

NS = {'m': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main',
      'r': 'http://schemas.openxmlformats.org/officeDocument/2006/relationships'}

RAIZ = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PLANTILLA = os.path.join(RAIZ, 'Actualizacion', 'Plantilla_Carga_Modulo_HE.xlsx')
SALIDA_DIR = os.path.join(RAIZ, 'docs', 'sql', 'carga-generada')
SALIDA = os.path.join(SALIDA_DIR, 'carga-horas-extras.sql')

VIGENCIA_ROL = '2026-09-01'


def leer_hojas(ruta):
    z = zipfile.ZipFile(ruta)
    compartidas = []
    if 'xl/sharedStrings.xml' in z.namelist():
        raiz = ET.fromstring(z.read('xl/sharedStrings.xml'))
        for si in raiz.findall('m:si', NS):
            compartidas.append(''.join(t.text or '' for t in si.iter('{%s}t' % NS['m'])))

    wb = ET.fromstring(z.read('xl/workbook.xml'))
    rels = ET.fromstring(z.read('xl/_rels/workbook.xml.rels'))
    destino = {x.get('Id'): x.get('Target') for x in rels}

    def valor(c):
        v = c.find('m:v', NS)
        if v is None:
            inline = c.find('m:is', NS)
            if inline is None:
                return ''
            return ''.join(t.text or '' for t in inline.iter('{%s}t' % NS['m']))
        return compartidas[int(v.text)] if c.get('t') == 's' else (v.text or '')

    hojas = {}
    for sh in wb.find('m:sheets', NS):
        nombre = sh.get('name')
        ruta_hoja = destino[sh.get('{%s}id' % NS['r'])].lstrip('/')
        if not ruta_hoja.startswith('xl/'):
            ruta_hoja = 'xl/' + ruta_hoja
        hoja = ET.fromstring(z.read(ruta_hoja))
        filas = []
        for fila in hoja.iter('{%s}row' % NS['m']):
            celdas = {}
            for c in fila.findall('m:c', NS):
                v = valor(c)
                if v != '':
                    celdas[re.match(r'[A-Z]+', c.get('r')).group()] = v
            if celdas:
                filas.append(celdas)
        hojas[nombre] = filas
    return hojas


def tabla(hojas, nombre):
    filas = hojas[nombre]
    cabecera = filas[0]
    columnas = {v: k for k, v in cabecera.items()}
    return [{c: f.get(columnas[c], '') for c in columnas} for f in filas[1:]]


def q(texto):
    """Escapa una cadena para T-SQL doblando la comilla simple."""
    return "N'" + (texto or '').replace("'", "''") + "'"


def cedula_valida(c):
    """Digito verificador ecuatoriano. Una cedula mal escrita no debe cargarse."""
    if len(c) != 10 or not c.isdigit():
        return False
    if not (1 <= int(c[:2]) <= 24 or int(c[:2]) == 30):
        return False
    if int(c[2]) >= 6:
        return False
    suma = 0
    for i, ch in enumerate(c[:9]):
        n = int(ch) * (2 if i % 2 == 0 else 1)
        suma += n - 9 if n > 9 else n
    return (10 - (suma % 10)) % 10 == int(c[9])


def main():
    if not os.path.exists(PLANTILLA):
        print('No se encontro la plantilla en %s' % PLANTILLA)
        return 1

    hojas = leer_hojas(PLANTILLA)
    colaboradores = tabla(hojas, 'Colaboradores')
    salarios = tabla(hojas, 'Salarios')

    malas = [c['Cedula'].strip() for c in colaboradores if not cedula_valida(c['Cedula'].strip())]
    if malas:
        print('Hay %d cedulas que no pasan el digito verificador. No se genera nada.' % len(malas))
        return 1

    conocidas = set(c['Cedula'].strip() for c in colaboradores)
    huerfanos = [s for s in salarios if s['Cedula'].strip() not in conocidas]
    if huerfanos:
        print('Hay %d salarios cuya cedula no esta en Colaboradores. No se genera nada.' % len(huerfanos))
        return 1

    if not os.path.isdir(SALIDA_DIR):
        os.makedirs(SALIDA_DIR)

    L = []
    L.append('/* ' + '=' * 74)
    L.append('   CARGA DEL MODULO DE HORAS EXTRAS - GENERADO, NO EDITAR A MANO')
    L.append('')
    L.append('   Lo produce docs/sql/generar-carga-horas-extras.py desde la plantilla.')
    L.append('   CONTIENE DATOS PERSONALES: nombres, cedulas y sueldos. NO SE COMMITEA.')
    L.append('')
    L.append('   Idempotente: concilia por cedula contra Empleados y no duplica.')
    L.append('   ' + '=' * 74 + ' */')
    L.append('')
    L.append('SET NOCOUNT ON;')
    L.append('GO')
    L.append('SET QUOTED_IDENTIFIER ON;')
    L.append('SET ANSI_NULLS ON;')
    L.append('GO')
    L.append('')
    L.append('BEGIN TRY')
    L.append('BEGIN TRANSACTION;')
    L.append('')
    L.append('/* La conciliacion es por cedula contra Empleados, UNA SOLA VEZ. A partir')
    L.append('   de aqui la llave es IdEmpleado: hay una cedula que en R_Usuarios')
    L.append('   comparten seis usuarios activos, y elegir uno seria inventar. */')
    L.append('DECLARE @Faltantes INT;')
    L.append('CREATE TABLE #C (Cedula VARCHAR(20), Nombre NVARCHAR(400), Empresa VARCHAR(120),')
    L.append('                 Jornada INT, AplicaHE BIT, Motivo VARCHAR(40));')
    L.append('CREATE TABLE #S (Cedula VARCHAR(20), Monto DECIMAL(18,2), Desde DATE, Origen VARCHAR(20));')
    L.append('')

    for c in colaboradores:
        aplica = '1' if c['AplicaHE'].strip().upper() == 'SI' else '0'
        situacion = c['Situacion'].strip()
        motivo = 'NULL' if aplica == '1' and situacion == 'Activo' else q(situacion)
        L.append('INSERT INTO #C VALUES (%s, %s, %s, %s, %s, %s);' % (
            q(c['Cedula'].strip()), q(c['NombreCompleto'].strip()), q(c['Empresa'].strip()),
            c['JornadaHorasDia'].strip() or '0', aplica, motivo))

    L.append('')
    for s in salarios:
        desde = s['FechaVigenciaDesde'].strip() or VIGENCIA_ROL
        L.append('INSERT INTO #S VALUES (%s, %s, %s, %s);' % (
            q(s['Cedula'].strip()), s['Monto'].strip() or '0', q(desde), q(s['Origen'].strip() or 'Rol')))

    L.append('')
    L.append('/* Si alguna cedula no esta en Empleados, no se carga nada: media carga')
    L.append('   es peor que ninguna. */')
    L.append('SELECT @Faltantes = COUNT(*) FROM #C c')
    L.append(' WHERE NOT EXISTS (SELECT 1 FROM dbo.Empleados e WHERE LTRIM(RTRIM(e.Cedula)) = c.Cedula);')
    L.append('IF @Faltantes > 0')
    L.append('BEGIN')
    L.append("    RAISERROR('FALLO: %d cedulas de la plantilla no estan en Empleados. No se cargo nada.', 16, 1, @Faltantes);")
    L.append('    ROLLBACK TRANSACTION; RETURN;')
    L.append('END')
    L.append('')
    L.append('MERGE dbo.HE_ColaboradorParametro AS d')
    L.append('USING (SELECT e.IdEmpleado, c.Jornada, c.AplicaHE, c.Motivo, c.Empresa')
    L.append('         FROM #C c JOIN dbo.Empleados e ON LTRIM(RTRIM(e.Cedula)) = c.Cedula) AS o')
    L.append('   ON d.IdEmpleado = o.IdEmpleado')
    L.append(' WHEN MATCHED THEN UPDATE SET d.JornadaHorasDia = o.Jornada, d.AplicaHE = o.AplicaHE,')
    L.append('                              d.MotivoNoAplica = o.Motivo, d.Empresa = o.Empresa,')
    L.append("                              d.Fec_Modificacion = SYSDATETIME(), d.Usu_Modificacion = 'carga-plantilla'")
    L.append(' WHEN NOT MATCHED THEN INSERT (IdEmpleado, JornadaHorasDia, AplicaHE, MotivoNoAplica, Empresa, Usu_Modificacion)')
    L.append("                        VALUES (o.IdEmpleado, o.Jornada, o.AplicaHE, o.Motivo, o.Empresa, 'carga-plantilla');")
    L.append('')
    L.append('/* Los sueldos no se pisan: se insertan los que falten. Un historial no se')
    L.append('   reescribe, se le agregan filas. */')
    L.append('INSERT INTO dbo.HE_Salario (IdEmpleado, Monto, FechaVigenciaDesde, Origen, Usu_Modificacion)')
    L.append("SELECT e.IdEmpleado, s.Monto, s.Desde, s.Origen, 'carga-plantilla'")
    L.append('  FROM #S s JOIN dbo.Empleados e ON LTRIM(RTRIM(e.Cedula)) = s.Cedula')
    L.append(' WHERE NOT EXISTS (SELECT 1 FROM dbo.HE_Salario h')
    L.append('                    WHERE h.IdEmpleado = e.IdEmpleado AND h.FechaVigenciaDesde = s.Desde')
    L.append('                      AND h.Monto = s.Monto);')
    L.append('')
    L.append('DROP TABLE #C; DROP TABLE #S;')
    L.append('COMMIT TRANSACTION;')
    L.append("PRINT 'Carga de colaboradores y sueldos terminada.';")
    L.append('END TRY')
    L.append('BEGIN CATCH')
    L.append('    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;')
    L.append("    PRINT 'Error, la carga quedo revertida: ' + ERROR_MESSAGE();")
    L.append('END CATCH')
    L.append('GO')

    with open(SALIDA, 'w', encoding='utf-8') as f:
        f.write('\n'.join(L) + '\n')

    print('Generado: %s' % SALIDA)
    print('  colaboradores: %d' % len(colaboradores))
    print('  salarios:      %d' % len(salarios))
    print('NO COMMITEAR ese archivo: contiene datos personales.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
```

- [ ] **Step 2: Correr el generador**

```bash
python docs/sql/generar-carga-horas-extras.py
```

Esperado: dice `colaboradores: 64` y `salarios: 69`. Pega la salida.

- [ ] **Step 3: Comprobar que la salida NO entra en git**

```bash
git status --porcelain docs/sql/carga-generada/ ; echo "(vacio = ignorada, como debe ser)"
git check-ignore -v docs/sql/carga-generada/carga-horas-extras.sql
```

La primera debe salir vacía y la segunda nombrar la regla. Si la primera muestra el archivo, **para**: la tarea 1 no protegió lo que debía.

- [ ] **Step 4: El script de verificación**

Crea `docs/sql/2026-09-15-horas-extras-verificacion.sql`:

```sql
/* ============================================================================
   Verificacion de la carga del modulo de Horas Extras.

   Solo cuenta. No muestra ni un nombre, ni una cedula, ni un sueldo: este
   archivo si se versiona y el repositorio es publico. Para saber si la carga
   quedo bien basta con los numeros.
   ============================================================================ */

SET NOCOUNT ON;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

SELECT Comprobacion = 'colaboradores cargados (esperado 64)',
       Valor        = CONVERT(VARCHAR(12), COUNT(*)) FROM dbo.HE_ColaboradorParametro
UNION ALL
SELECT 'sueldos cargados (esperado 69)', CONVERT(VARCHAR(12), COUNT(*)) FROM dbo.HE_Salario
UNION ALL
SELECT 'personas con mas de un sueldo (esperado 5)', CONVERT(VARCHAR(12), COUNT(*)) FROM (
    SELECT IdEmpleado FROM dbo.HE_Salario GROUP BY IdEmpleado HAVING COUNT(*) > 1) x
UNION ALL
SELECT 'colaboradores sin ningun sueldo (esperado 0)', CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro c
 WHERE NOT EXISTS (SELECT 1 FROM dbo.HE_Salario s WHERE s.IdEmpleado = c.IdEmpleado)
UNION ALL
SELECT 'jornada de 8 h (esperado 56)', CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro WHERE JornadaHorasDia = 8
UNION ALL
SELECT 'jornada de 4 h (esperado 8)', CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro WHERE JornadaHorasDia = 4
UNION ALL
SELECT 'no aplican horas extras (esperado 3)', CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro WHERE AplicaHE = 0
UNION ALL
SELECT 'con divisor manual (esperado 0)', CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro WHERE DivisorManual IS NOT NULL
UNION ALL
SELECT 'parametros vigentes (esperado 7)', CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_Parametro WHERE FechaVigenciaHasta IS NULL
UNION ALL
SELECT 'colaboradores que no existen en Empleados (esperado 0)', CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro c
 WHERE NOT EXISTS (SELECT 1 FROM dbo.Empleados e WHERE e.IdEmpleado = c.IdEmpleado);

/* La jornada que declara la plantilla contra la que el sistema ya conoce.
   58 de los 64 tienen horario asignado. Esto NO corrige nada: reporta para que
   RRHH lo mire, porque el Excel no es autoridad sobre el horario de nadie. */
SELECT Comprobacion = 'colaboradores con horario asignado en el sistema',
       Valor        = CONVERT(VARCHAR(12), COUNT(DISTINCT c.IdEmpleado))
  FROM dbo.HE_ColaboradorParametro c
  JOIN dbo.Empleados e ON e.IdEmpleado = c.IdEmpleado
  JOIN dbo.R_UsuarioHorarioLaboral uh ON uh.Id_Responsable = e.Cod_Usuario AND uh.Activo = 1;
GO
```

- [ ] **Step 5: Comprobar que el verificador no filtra nada**

```bash
grep -nE "Nombre|Cedula|Monto|Salario[^_]" docs/sql/2026-09-15-horas-extras-verificacion.sql | grep -v "COUNT\|HE_Salario\|--"
echo "(vacio = solo cuenta, no muestra)"
```

Pega la salida.

- [ ] **Step 6: Commit y avisar**

Commitea **sólo** el generador y el verificador. El script generado no:

```bash
git add docs/sql/generar-carga-horas-extras.py docs/sql/2026-09-15-horas-extras-verificacion.sql
git commit -m "feat(horas-extras): generador de la carga y su verificacion por conteos"
```

Comprueba que no se coló la salida:

```bash
git show --stat --name-only HEAD | grep "carga-generada" ; echo "(vacio = correcto)"
```

En el reporte escribe textualmente: **«El script de carga está generado en `docs/sql/carga-generada/carga-horas-extras.sql` y NO está commiteado, a propósito. Hace falta correrlo contra producción, y después `docs/sql/2026-09-15-horas-extras-verificacion.sql`.»**

---

## Lo que queda para el usuario

1. **Correr los tres scripts en este orden:** la estructura (`...fase1.sql`), la carga generada (`carga-generada/carga-horas-extras.sql`) y la verificación (`...verificacion.sql`).
2. **Revisar el reporte de contraste de jornada.** Si el sistema y la plantilla discrepan sobre las horas/día de alguien, gana el contrato, no el Excel.
3. **Decidir el divisor de las 8 personas de media jornada** con RRHH y Legal. Hasta entonces manda `horas/día × 30`.
4. **Entregar `Calculadora_Horas_Extras_50_100.xlsx`** si se quiere ejecutar la validación de corte del §9.5 funcional. Sin ese archivo no hay horas conocidas contra las que cuadrar.
5. **Confirmar el formato del archivo para nómina.** No bloquea las fases 1 y 2; sí la 3.
