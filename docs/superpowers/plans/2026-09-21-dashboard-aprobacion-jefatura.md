# Dashboard de aprobación para jefatura — Plan de implementación

> **Para trabajadores agénticos:** SUB-SKILL OBLIGATORIA: usar
> superpowers:subagent-driven-development (recomendado) o
> superpowers:executing-plans para ejecutar este plan tarea por tarea. Los pasos
> usan casillas (`- [ ]`) para seguimiento.

**Goal:** Una tercera vista en `AprobacionTareasJefatura.aspx` con cuatro gráficos
de Chart.js —evolución, por persona, demora en aprobar y por empresa— y tarjetas de
totales, sobre los mismos datos y con los mismos criterios que la tabla que ya
existe en esa pantalla.

**Architecture:** La pantalla ya alterna entre `Pagina1` y `Pagina2` con el combo de
estados; el dashboard es `Pagina3`, una rama más del mismo interruptor. Un
procedimiento nuevo devuelve cinco conjuntos **ya agregados** y el navegador sólo
dibuja. Todo el código nuevo vive en archivos nuevos; del código existente sólo se
agregan ramas, nunca se modifica lo que hay.

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.8 en el web, 4.6.1 en las capas),
SQL Server, jQuery + Bootstrap 3, Chart.js 4.5.1 (UMD, local), MSTest v1 (ensamblado
de VS2019, sin NuGet), `vstest.console.exe`.

**Spec:** `docs/superpowers/specs/2026-09-21-dashboard-aprobacion-jefatura-design.md`

## Global Constraints

- **Comentarios de código y SQL sin tildes; textos de usuario con tildes.**
- **Los `.aspx` y `.ascx` se guardan con BOM.** `Web.config` declara `windows-1252`
  y no trae `fileEncoding`; sin BOM salen con caracteres rotos en producción.
- **Todo archivo nuevo se agrega a su `.csproj`**, incluidos los `.js`, que van como
  `<Content Include="..." />`. Un archivo que no está listado no se publica.
- **Los finales de línea NO son uniformes en este repositorio.** Medidos el
  2026-09-21, todos los archivos que este plan toca llevan BOM, pero:

  | Archivo | Finales |
  |---|---|
  | **`ReporteTareas/ReporteTareas.csproj`** | **LF** (el único) |
  | `CapaNegocio.csproj`, `CapaEntidad.csproj`, `CapaDato.csproj`, `CapaPruebas.csproj` | CRLF |
  | `AprobacionTareasJefatura.aspx`, `aprobacionTareasJefatura.js`, `ObtenerListaTareas.ashx.cs` | CRLF |

  **Preservá lo que cada archivo ya tiene; no lo uniformes.** `core.autocrlf` está
  en `true` y no hay `.gitattributes`, así que «normalizar»
  `ReporteTareas.csproj` a CRLF produce un diff de 2.625 líneas que entierra el
  cambio real. Comprobalo antes de escribir, no lo supongas.
- **Compilar con el MSBuild de VS2019:**
  `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`
- **Correr las pruebas con:**
  `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll`
- **Invocar MSBuild desde PowerShell, no desde Git Bash** (`/p:` se corrompe).
- **El parseo de horas es el del procedimiento vigente y no se mejora:**
  `CONVERT(INT,SUBSTRING(Det_Tiempo,1,2))*60 + CONVERT(INT,SUBSTRING(Det_Tiempo,4,2))`.
  Descarta los segundos en 5.616 filas a propósito: si el gráfico y la tabla de la
  misma pantalla sumaran distinto, ninguna de las dos sería creíble.
- **El umbral de jornada es 8 horas**, el mismo que ya usa `CumpleJornada`.
- **La regla de visibilidad es la del procedimiento vigente**, verruga incluida
  (ver Task 3). El dashboard tiene que cubrir exactamente a la misma gente que la
  tabla.
- **Ningún subagente ejecuta SQL contra la base, ni publica a IIS, ni abre la
  aplicación.** Los scripts se entregan escritos; los corre el usuario.

---

## Corrección al spec, aplicada en este plan

El spec (§5) lista `MinutosDesdeTiempo` entre las funciones probables de
`CapaNegocio`. **Se elimina.** El parseo de `HH:MM:SS` lo hace el procedimiento, y
una copia en C# sería una segunda fuente de verdad para la misma regla — que es
exactamente lo que el propio spec advierte que hay que evitar. El SQL devuelve
**minutos enteros** y C# sólo los convierte a horas decimales para las etiquetas.

Las funciones probables quedan en tres: `HorasDecimales`, `TopConOtras` y
`TextoDemora`.

---

## Estructura de archivos

| Archivo | Responsabilidad |
|---|---|
| `ReporteTareas/js/chart.umd.js` | **Crear.** Chart.js 4.5.1, compilación UMD, servida local |
| `CapaNegocio/NegDashboardAprobacion.cs` | **Crear.** Las tres funciones puras, con pruebas |
| `CapaPruebas/NegDashboardAprobacionTests.cs` | **Crear.** Sus pruebas |
| `docs/sql/2026-09-21-dashboard-aprobacion.sql` | **Crear.** `Sp_RTA_DashboardAprobacionJefatura` |
| `CapaEntidad/EntDashboardAprobacion.cs` | **Crear.** Los cinco bloques del resultado |
| `CapaDato/DaoDashboardAprobacion.cs` | **Crear.** Leer los cinco conjuntos |
| `ReporteTareas/Formulario/ObtenerListaTareas.ashx.cs` | **Modificar.** Una acción: `DashboardAprobacion` |
| `ReporteTareas/Formulario/AprobacionTareasJefatura.aspx` | **Modificar.** Opción del combo, `Pagina3`, dos `<script>` |
| `ReporteTareas/js/aprobacionTareasJefatura.js` | **Modificar.** Dos ramas, en `BtnConsulta` y `BuscarEstado` |
| `ReporteTareas/js/dashboardAprobacion.js` | **Crear.** Pedir los datos y dibujar |
| `ReporteTareas/ReporteTareas.csproj` | **Modificar.** Los dos `.js` nuevos |
| `CapaNegocio/CapaNegocio.csproj`, `CapaEntidad/CapaEntidad.csproj`, `CapaDato/CapaDato.csproj`, `CapaPruebas/CapaPruebas.csproj` | **Modificar.** Los archivos nuevos |
| `DESPLIEGUE.md` | **Modificar.** Qué se publica |

---

## Task 1: Chart.js local

**Files:**
- Create: `ReporteTareas/js/chart.umd.js`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: nada.
- Produces: la variable global `Chart` en cualquier página que cargue el archivo.
  La Task 7 la usa.

Chart.js no está en el repositorio. Hay `flot` y `morris` de la plantilla, y sólo
`morris` se usa. Va **local y no por CDN**: la aplicación es interna y un gráfico
que no carga por red deja la pantalla a medias sin decir por qué.

- [ ] **Step 1: Traer el archivo**

```bash
cd /tmp && rm -rf chartjs && mkdir chartjs && cd chartjs
npm pack chart.js@4.5.1
tar -xzf chart.js-4.5.1.tgz package/dist/chart.umd.js
cp package/dist/chart.umd.js "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas/js/chart.umd.js"
```

Se usa **`chart.umd.js`** y no `chart.js`: la compilación UMD es la única que
expone `Chart` como variable global, que es lo que necesita una página sin
empaquetador.

Tampoco `chart.umd.min.js`, pero **no por tamaño**: medidos, los dos pesan 203 KB.
`chart.umd.js` es la distribución que la documentación de Chart.js indica para una
etiqueta `<script>`, y con archivos del mismo peso no hay nada que ganar del otro
lado. Ojo con una suposición fácil: `chart.umd.js` **ya viene minificado** —una
sola línea de casi 200.000 caracteres—, así que no esperes poder leerlo.

- [ ] **Step 2: Comprobar que es el archivo correcto y que se basta solo**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
python -c "
import io
s = io.open('ReporteTareas/js/chart.umd.js', encoding='utf-8').read()
print('tamano KB      :', round(len(s.encode('utf-8'))/1024))
print('version        :', 'v4.5.1' in s[:300])
print('licencia MIT   :', 'MIT' in s[:300])
print('require externo:', s.count('require('))
print('expone Chart   :', '.Chart=' in s[:400])
"
```

Esperado: `~204`, `True`, `True`, **`0`**, `True`.

El `require externo: 0` es el que importa: Chart.js declara depender de
`@kurkle/color`, y si la compilación elegida no lo trajera adentro, la página
fallaría con un `require is not defined` que no dice nada. La UMD lo incluye.

- [ ] **Step 3: Registrarlo en el `.csproj`**

En `ReporteTareas/ReporteTareas.csproj`, junto a los demás `Content Include` de
`js\`:

```xml
    <Content Include="js\chart.umd.js" />
```

Comprobar que quedó:

```bash
grep -c 'Content Include="js\\chart.umd.js"' ReporteTareas/ReporteTareas.csproj
```

Esperado: `1`.

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/js/chart.umd.js ReporteTareas/ReporteTareas.csproj
git commit -m "build: Chart.js 4.5.1 como archivo local"
```

---

## Task 2: Las tres funciones puras

**Files:**
- Create: `CapaNegocio/NegDashboardAprobacion.cs`
- Create: `CapaPruebas/NegDashboardAprobacionTests.cs`
- Create: `CapaEntidad/EntDashboardAprobacion.cs` (sólo `EntDashboardEmpresa`; la
  Task 4 le agrega las demás clases)
- Modify: `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`,
  `CapaEntidad/CapaEntidad.csproj`

**Interfaces:**
- Consumes: `CapaEntidad.EntDashboardEmpresa` — **la define la Task 4**. Para que
  esta tarea compile antes, se crea aquí el archivo
  `CapaEntidad/EntDashboardAprobacion.cs` **sólo con esa clase**, y la Task 4 le
  agrega las demás.
- Produces:
  - `public static decimal NegDashboardAprobacion.HorasDecimales(int minutos)`
  - `public static List<EntDashboardEmpresa> NegDashboardAprobacion.TopConOtras(List<EntDashboardEmpresa> lista, int tope)`
  - `public static string NegDashboardAprobacion.TextoDemora(int dias)`
  Las usa la Task 5.

- [ ] **Step 1: Crear la entidad mínima que las pruebas necesitan**

`CapaEntidad/EntDashboardAprobacion.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Una empresa y los minutos que se le dedicaron en el rango.
    ///
    /// Minutos y no horas: la conversion a horas decimales es para la etiqueta
    /// del grafico y la hace NegDashboardAprobacion.HorasDecimales. Guardar aqui
    /// un decimal obligaria a redondear antes de sumar, y las sumas de valores ya
    /// redondeados no dan lo mismo que el redondeo de la suma.
    /// </summary>
    public class EntDashboardEmpresa
    {
        public string Empresa { get; set; } = "";
        public int Minutos { get; set; }
    }
}
```

Registrarla en `CapaEntidad/CapaEntidad.csproj`, junto a las demás `Ent*`:

```xml
    <Compile Include="EntDashboardAprobacion.cs" />
```

- [ ] **Step 2: Escribir las pruebas que fallan**

`CapaPruebas/NegDashboardAprobacionTests.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace CapaPruebas
{
    /// <summary>
    /// Lo unico del dashboard que se puede probar sin base: las tres
    /// conversiones que ocurren entre el dato agregado y lo que se dibuja.
    /// </summary>
    [TestClass]
    public class NegDashboardAprobacionTests
    {
        private CultureInfo _culturaOriginal;

        /// <summary>
        /// Cultura hostil a proposito: en es-ES el separador decimal es la coma.
        /// Si alguien construye el numero con ToString() sin InvariantCulture,
        /// al JSON le llega "7,5" y Chart.js lo lee como texto, no como numero:
        /// la barra sale en cero sin un solo error.
        /// </summary>
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

        /* ------------------------------------------- horas decimales ------- */

        [TestMethod]
        public void HorasDecimales_DeUnaHoraExacta_DaUno()
        {
            Assert.AreEqual(1.0m, NegDashboardAprobacion.HorasDecimales(60));
        }

        [TestMethod]
        public void HorasDecimales_DeMediaHora_DaMedio()
        {
            Assert.AreEqual(0.5m, NegDashboardAprobacion.HorasDecimales(30));
        }

        [TestMethod]
        public void HorasDecimales_RedondeaAUnDecimal()
        {
            // 100 minutos son 1,666... horas
            Assert.AreEqual(1.7m, NegDashboardAprobacion.HorasDecimales(100));
        }

        [TestMethod]
        public void HorasDecimales_DeCero_DaCero()
        {
            Assert.AreEqual(0m, NegDashboardAprobacion.HorasDecimales(0));
        }

        /// <summary>
        /// Minutos negativos no deberian existir, pero si la base devolviera uno
        /// -una fila con Det_Tiempo raro que pase el filtro- la grafica tiene que
        /// mostrar cero y no una barra hacia abajo que nadie sabe leer.
        /// </summary>
        [TestMethod]
        public void HorasDecimales_DeNegativo_DaCero()
        {
            Assert.AreEqual(0m, NegDashboardAprobacion.HorasDecimales(-30));
        }

        /* ----------------------------------------------- top con otras ----- */

        private static List<EntDashboardEmpresa> Empresas(params int[] minutos)
        {
            var lista = new List<EntDashboardEmpresa>();
            for (int i = 0; i < minutos.Length; i++)
            {
                lista.Add(new EntDashboardEmpresa
                {
                    Empresa = "Empresa " + (i + 1),
                    Minutos = minutos[i]
                });
            }
            return lista;
        }

        [TestMethod]
        public void TopConOtras_ConMenosQueElTope_DevuelveTodoSinAgregarOtras()
        {
            var r = NegDashboardAprobacion.TopConOtras(Empresas(50, 40, 30), 10);

            Assert.AreEqual(3, r.Count);
            Assert.IsFalse(r.Exists(e => e.Empresa == "Otras"));
        }

        [TestMethod]
        public void TopConOtras_ConExactamenteElTope_NoAgregaOtras()
        {
            var r = NegDashboardAprobacion.TopConOtras(Empresas(10, 9, 8), 3);

            Assert.AreEqual(3, r.Count);
            Assert.IsFalse(r.Exists(e => e.Empresa == "Otras"));
        }

        [TestMethod]
        public void TopConOtras_ConMasQueElTope_AgrupaElRestoEnOtras()
        {
            var r = NegDashboardAprobacion.TopConOtras(Empresas(10, 9, 8, 5, 3), 3);

            Assert.AreEqual(4, r.Count);
            Assert.AreEqual("Otras", r[3].Empresa);
            Assert.AreEqual(8, r[3].Minutos);   // 5 + 3
        }

        /// <summary>
        /// El orden manda: si la lista llega desordenada, el top tiene que ser el
        /// de MAS minutos, no los primeros que vinieron.
        /// </summary>
        [TestMethod]
        public void TopConOtras_OrdenaPorMinutosAntesDeCortar()
        {
            var lista = new List<EntDashboardEmpresa>
            {
                new EntDashboardEmpresa { Empresa = "Chica", Minutos = 5 },
                new EntDashboardEmpresa { Empresa = "Grande", Minutos = 100 },
                new EntDashboardEmpresa { Empresa = "Media", Minutos = 50 }
            };

            var r = NegDashboardAprobacion.TopConOtras(lista, 2);

            Assert.AreEqual("Grande", r[0].Empresa);
            Assert.AreEqual("Media", r[1].Empresa);
            Assert.AreEqual("Otras", r[2].Empresa);
            Assert.AreEqual(5, r[2].Minutos);
        }

        [TestMethod]
        public void TopConOtras_ConListaVacia_DevuelveVacia()
        {
            var r = NegDashboardAprobacion.TopConOtras(new List<EntDashboardEmpresa>(), 10);

            Assert.AreEqual(0, r.Count);
        }

        [TestMethod]
        public void TopConOtras_ConNulo_DevuelveVacia()
        {
            var r = NegDashboardAprobacion.TopConOtras(null, 10);

            Assert.AreEqual(0, r.Count);
        }

        /// <summary>
        /// Con tope cero o negativo no se puede armar un top: se devuelve todo
        /// sin agrupar, que es menos sorprendente que devolver una sola porcion
        /// llamada "Otras" con el total.
        /// </summary>
        [TestMethod]
        public void TopConOtras_ConTopeCero_DevuelveLaListaSinAgrupar()
        {
            var r = NegDashboardAprobacion.TopConOtras(Empresas(10, 9), 0);

            Assert.AreEqual(2, r.Count);
            Assert.IsFalse(r.Exists(e => e.Empresa == "Otras"));
        }

        /* ------------------------------------------------ texto demora ----- */

        [TestMethod]
        public void TextoDemora_DeCero_DiceHoy()
        {
            Assert.AreEqual("hoy", NegDashboardAprobacion.TextoDemora(0));
        }

        [TestMethod]
        public void TextoDemora_DeUno_NoDicePluralRaro()
        {
            Assert.AreEqual("1 día", NegDashboardAprobacion.TextoDemora(1));
        }

        [TestMethod]
        public void TextoDemora_DeVarios_DicePlural()
        {
            Assert.AreEqual("12 días", NegDashboardAprobacion.TextoDemora(12));
        }

        [TestMethod]
        public void TextoDemora_DeNegativo_DiceHoy()
        {
            /* Una aprobacion con fecha anterior al registro da dias negativos.
               Existe en datos viejos y no es un error que valga la pena gritar:
               se lee como "sin demora". */
            Assert.AreEqual("hoy", NegDashboardAprobacion.TextoDemora(-3));
        }
    }
}
```

Registrarla en `CapaPruebas/CapaPruebas.csproj`:

```xml
    <Compile Include="NegDashboardAprobacionTests.cs" />
```

- [ ] **Step 3: Compilar para verlas fallar**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
```

Esperado: **falla** con `CS0103: El nombre 'NegDashboardAprobacion' no existe en el
contexto actual`.

- [ ] **Step 4: Escribir la implementación**

`CapaNegocio/NegDashboardAprobacion.cs`:

```csharp
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Lo que ocurre entre el dato agregado que devuelve la base y lo que se
    /// dibuja. No toca la base ni HttpContext: es la unica parte del dashboard
    /// que se puede probar sin levantar nada.
    ///
    /// Aca NO se parsea Det_Tiempo. Eso lo hace el procedimiento, y una copia en
    /// C# seria una segunda fuente de verdad para la misma regla; el dia que una
    /// cambie y la otra no, el grafico y la tabla de la misma pantalla diran
    /// cifras distintas y no habra forma de saber cual miente.
    /// </summary>
    public static class NegDashboardAprobacion
    {
        /// <summary>Etiqueta "Otras" de la porcion que agrupa la cola larga.</summary>
        private const string EtiquetaOtras = "Otras";

        /// <summary>
        /// Minutos a horas con un decimal. Un decimal y no dos: son horas de
        /// trabajo en un grafico, y el segundo decimal es ruido que solo hace la
        /// etiqueta mas larga.
        ///
        /// Los negativos dan cero: no deberian existir, pero una barra hacia
        /// abajo en un grafico de horas no la sabe leer nadie.
        /// </summary>
        public static decimal HorasDecimales(int minutos)
        {
            if (minutos <= 0) { return 0m; }

            return System.Math.Round(minutos / 60m, 1);
        }

        /// <summary>
        /// Las <paramref name="tope"/> empresas con mas minutos, y el resto sumado
        /// en una porcion "Otras".
        ///
        /// Una torta de 71 porciones no se lee. Agrupar la cola en vez de
        /// descartarla deja ver de un vistazo cuanto pesa: si "Otras" es la
        /// porcion mas grande, el top 10 no estaba contando la historia.
        ///
        /// Ordena antes de cortar: si la lista llega desordenada, quedarse con
        /// los primeros daria un "top" que no es el de mas horas.
        /// </summary>
        public static List<EntDashboardEmpresa> TopConOtras(List<EntDashboardEmpresa> lista, int tope)
        {
            List<EntDashboardEmpresa> resultado = new List<EntDashboardEmpresa>();
            if (lista == null) { return resultado; }

            List<EntDashboardEmpresa> ordenada = new List<EntDashboardEmpresa>(lista);
            ordenada.Sort((a, b) => b.Minutos.CompareTo(a.Minutos));

            /* Con un tope que no acota, devolver todo sin agrupar es menos
               sorprendente que una sola porcion "Otras" con el total. */
            if (tope <= 0 || ordenada.Count <= tope) { return ordenada; }

            int minutosDeLaCola = 0;

            for (int i = 0; i < ordenada.Count; i++)
            {
                if (i < tope) { resultado.Add(ordenada[i]); }
                else { minutosDeLaCola += ordenada[i].Minutos; }
            }

            resultado.Add(new EntDashboardEmpresa
            {
                Empresa = EtiquetaOtras,
                Minutos = minutosDeLaCola
            });

            return resultado;
        }

        /// <summary>
        /// Los dias de demora, en texto. "hoy", "1 dia", "N dias".
        ///
        /// Los negativos se leen como "hoy": una aprobacion fechada antes del
        /// registro existe en datos viejos, y mostrar "-3 dias" haria que quien
        /// lo vea desconfie de todo el tablero por un caso que no importa.
        /// </summary>
        public static string TextoDemora(int dias)
        {
            if (dias <= 0) { return "hoy"; }
            if (dias == 1) { return "1 día"; }

            return dias.ToString(System.Globalization.CultureInfo.InvariantCulture) + " días";
        }
    }
}
```

Registrarla en `CapaNegocio/CapaNegocio.csproj`:

```xml
    <Compile Include="NegDashboardAprobacion.cs" />
```

- [ ] **Step 5: Compilar y correr las pruebas**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: las 16 pruebas nuevas en verde y **ninguna de las anteriores en rojo**.

- [ ] **Step 6: Commit**

```bash
git add CapaNegocio/NegDashboardAprobacion.cs CapaNegocio/CapaNegocio.csproj CapaEntidad/EntDashboardAprobacion.cs CapaEntidad/CapaEntidad.csproj CapaPruebas/NegDashboardAprobacionTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(tareas): las tres conversiones del dashboard, con pruebas"
```

---

## Task 3: El procedimiento con los cinco conjuntos

**Files:**
- Create: `docs/sql/2026-09-21-dashboard-aprobacion.sql`

**Interfaces:**
- Consumes: nada.
- Produces: `dbo.Sp_RTA_DashboardAprobacionJefatura(@IdUsuarioJefe VARCHAR(10),
  @FechaInicio VARCHAR(20), @FechaFin VARCHAR(20))`, que devuelve **cinco
  conjuntos** en el orden y con los nombres de columna que lee la Task 5.

### La verruga que hay que replicar, y por qué

`Sp_RTAListaHorasRecursosPorJefatura` tiene **cuatro códigos de usuario escritos a
mano** —`1314`, `1171`, `222692`, `1655906`— que toman una rama distinta y ven a
**todo el mundo**, no sólo a su equipo. El resto ve
`Id_Responsable = @jefe OR Cod_Jefe_Inm = @jefe`.

**Se replica tal cual.** No porque esté bien, sino porque si el dashboard usara la
regla buena y la tabla la vieja, esas cuatro personas verían un gráfico y una tabla
que no cuadran, y el tablero entero perdería credibilidad. Arreglar la regla es un
trabajo aparte y hay que hacerlo **en los dos lugares a la vez**.

- [ ] **Step 1: Escribir el script**

Archivo nuevo, **con BOM UTF-8**, comentarios sin tildes:

```sql
/* ============================================================================
   Dashboard de aprobacion para jefatura: los cinco conjuntos
   ReporTarea  |  2026-09-21

   PENDIENTE DE EJECUTAR.

   ----------------------------------------------------------------------------
   Devuelve TODO ya agregado. El navegador dibuja, no suma: traer 5.665 filas para
   que el JavaScript las recorra seria lento y, peor, pondria el calculo de horas
   en un segundo lugar distinto del que usa la tabla de la misma pantalla.

   El parseo de Det_Tiempo es el de Sp_RTAListaHorasRecursosPorJefatura, copiado:
   SUBSTRING(...,1,2)*60 + SUBSTRING(...,4,2). Descarta los segundos -5.616 filas
   los tienen distintos de cero- y se deja asi A PROPOSITO. Si el grafico sumara
   los segundos y la tabla no, dirian cifras distintas sobre el mismo dato.

   Idempotente: DROP y CREATE.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.R_DetTareasAranda','U') IS NULL
BEGIN
    RAISERROR('No existe dbo.R_DetTareasAranda. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

PRINT '== Dashboard de aprobacion - inicio ==';
GO

IF OBJECT_ID('dbo.Sp_RTA_DashboardAprobacionJefatura','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_DashboardAprobacionJefatura;
GO

CREATE PROCEDURE dbo.Sp_RTA_DashboardAprobacionJefatura
    @IdUsuarioJefe VARCHAR(10),
    @FechaInicio   VARCHAR(20),
    @FechaFin      VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Desde DATE, @Hasta DATE;
    SET @Desde = CONVERT(DATE, @FechaInicio, 105);
    SET @Hasta = CONVERT(DATE, @FechaFin, 105);

    /* Los cuatro codigos escritos a mano son los mismos de
       Sp_RTAListaHorasRecursosPorJefatura: ven a todo el mundo en vez de a su
       equipo. Se replica la regla, verruga incluida, para que el dashboard cubra
       EXACTAMENTE a la misma gente que la tabla de la misma pantalla. Si un dia
       se arregla, hay que arreglarlo en los dos lugares a la vez. */
    DECLARE @VeTodo BIT;
    SET @VeTodo = CASE WHEN @IdUsuarioJefe IN ('1314','1171','222692','1655906')
                       THEN 1 ELSE 0 END;

    /* Tabla temporal y no CTE: los cinco conjuntos la recorren, y con un CTE se
       recalcularia cinco veces. */
    CREATE TABLE #Base
    (
        Id_Responsable VARCHAR(50),
        Nombre         VARCHAR(100),
        Fecha          DATE,
        Estado         BIGINT,
        Empresa        VARCHAR(200),
        Minutos        INT,
        FechaAprob     DATETIME
    );

    /* El LIKE sobre Det_Tiempo protege al CONVERT: hoy las 86.496 filas tienen
       formato HH:MM:SS, pero una sola fila mal formada tumbaria el dashboard
       entero con un error de conversion, y un tablero que no abre es peor que uno
       al que le falta una fila. */
    INSERT INTO #Base (Id_Responsable, Nombre, Fecha, Estado, Empresa, Minutos, FechaAprob)
    SELECT  a.Id_Responsable,
            u.Nom_Usuario,
            CONVERT(DATE, a.Det_Fch_RegDetalleIni),
            a.Det_Aprobacion_Tarea_Estado,
            LTRIM(RTRIM(ISNULL(a.Det_Nom_Empresa, '(sin empresa)'))),
            CONVERT(INT, SUBSTRING(a.Det_Tiempo, 1, 2)) * 60
          + CONVERT(INT, SUBSTRING(a.Det_Tiempo, 4, 2)),
            a.Det_Fecha_Aprobacion_Tarea
      FROM  dbo.R_DetTareasAranda a
      INNER JOIN dbo.R_Usuarios u ON a.Id_Responsable = u.Cod_Usuario
     WHERE  CONVERT(DATE, a.Det_Fch_RegDetalleIni) BETWEEN @Desde AND @Hasta
       AND  a.Det_Tiempo LIKE '[0-9][0-9]:[0-9][0-9]:[0-9][0-9]'
       AND  (@VeTodo = 1
             OR a.Id_Responsable = LTRIM(RTRIM(@IdUsuarioJefe))
             OR u.Cod_Jefe_Inm   = LTRIM(RTRIM(@IdUsuarioJefe)));

    /* --- 1. totales ------------------------------------------------------
       Personas-dia con COUNT(DISTINCT responsable + fecha): una persona que
       cargo seis tareas el martes es UNA persona-dia, no seis.

       Los estados 5 y 7 van juntos en "Otros". Son 592 filas que hoy no aparecen
       en ninguna pantalla y no estan en el catalogo; no se les inventa un nombre,
       pero esconderlas haria que el total de las tarjetas no cuadre con el rango,
       y un tablero cuyos numeros no suman no lo cree nadie. */
    SELECT
        MinutosAprobados   = ISNULL(SUM(CASE WHEN Estado = 2 THEN Minutos END), 0),
        MinutosPendientes  = ISNULL(SUM(CASE WHEN Estado = 1 THEN Minutos END), 0),
        MinutosOtros       = ISNULL(SUM(CASE WHEN Estado NOT IN (1,2) THEN Minutos END), 0),
        PersonasDiaAprob   = ISNULL(COUNT(DISTINCT CASE WHEN Estado = 2 THEN Id_Responsable + '|' + CONVERT(VARCHAR(10), Fecha, 112) END), 0),
        PersonasDiaPend    = ISNULL(COUNT(DISTINCT CASE WHEN Estado = 1 THEN Id_Responsable + '|' + CONVERT(VARCHAR(10), Fecha, 112) END), 0),
        Responsables       = ISNULL(COUNT(DISTINCT Id_Responsable), 0)
      FROM #Base;

    /* --- 2. por semana ---------------------------------------------------
       DATEADD/DATEDIFF por WEEK da el lunes de cada semana, que es la etiqueta
       del eje. Se devuelve como fecha y no como "semana 38": el numero de semana
       obliga a quien mira a traducirlo, y cruza mal el cambio de anio. */
    SELECT
        Semana            = DATEADD(WEEK, DATEDIFF(WEEK, 0, Fecha), 0),
        MinutosAprobados  = ISNULL(SUM(CASE WHEN Estado = 2 THEN Minutos END), 0),
        MinutosPendientes = ISNULL(SUM(CASE WHEN Estado = 1 THEN Minutos END), 0)
      FROM #Base
     GROUP BY DATEADD(WEEK, DATEDIFF(WEEK, 0, Fecha), 0)
     ORDER BY 1;

    /* --- 3. por responsable ----------------------------------------------
       DiasBajoJornada usa el mismo umbral de 8 horas que CumpleJornada en
       Sp_RTAListaHorasRecursosPorJefatura. Se cuenta sobre el total del dia, sin
       importar el estado: un dia incompleto lo es aunque ya este aprobado. */
    SELECT
        Nombre            = MAX(b.Nombre),
        MinutosAprobados  = ISNULL(SUM(CASE WHEN b.Estado = 2 THEN b.Minutos END), 0),
        MinutosPendientes = ISNULL(SUM(CASE WHEN b.Estado = 1 THEN b.Minutos END), 0),
        PersonasDia       = COUNT(DISTINCT CONVERT(VARCHAR(10), b.Fecha, 112)),
        DiasBajoJornada   = ISNULL((SELECT COUNT(1)
                                      FROM (SELECT d.Fecha
                                              FROM #Base d
                                             WHERE d.Id_Responsable = b.Id_Responsable
                                             GROUP BY d.Fecha
                                            HAVING SUM(d.Minutos) < 480) x), 0)
      FROM #Base b
     GROUP BY b.Id_Responsable
     ORDER BY 2 DESC;

    /* --- 4. demora en aprobar --------------------------------------------
       Solo sobre lo aprobado y con fecha de aprobacion: el 97,7% de las filas la
       tiene. DiasMasViejoPendiente mira lo que sigue en estado 1 y responde la
       pregunta que un promedio no puede: hace cuanto que lo mas viejo espera. */
    SELECT
        DiasPromedio = ISNULL(AVG(CASE WHEN Estado = 2 AND FechaAprob IS NOT NULL
                                       THEN DATEDIFF(DAY, Fecha, CONVERT(DATE, FechaAprob)) END), 0),
        DiasMaximo   = ISNULL(MAX(CASE WHEN Estado = 2 AND FechaAprob IS NOT NULL
                                       THEN DATEDIFF(DAY, Fecha, CONVERT(DATE, FechaAprob)) END), 0),
        AprobadasConFecha = ISNULL(SUM(CASE WHEN Estado = 2 AND FechaAprob IS NOT NULL THEN 1 ELSE 0 END), 0),
        AprobadasSinFecha = ISNULL(SUM(CASE WHEN Estado = 2 AND FechaAprob IS NULL THEN 1 ELSE 0 END), 0),
        DiasMasViejoPendiente = ISNULL(DATEDIFF(DAY, MIN(CASE WHEN Estado = 1 THEN Fecha END), CONVERT(DATE, GETDATE())), 0)
      FROM #Base;

    /* --- 5. por empresa ---------------------------------------------------
       Se devuelven TODAS. El top 10 y la porcion "Otras" los arma
       NegDashboardAprobacion.TopConOtras, que esta probado; hacerlo aqui dejaria
       esa regla sin prueba y repartida en dos lenguajes. */
    SELECT
        Empresa = Empresa,
        Minutos = ISNULL(SUM(Minutos), 0)
      FROM #Base
     GROUP BY Empresa
     ORDER BY 2 DESC;

    DROP TABLE #Base;
END
GO

PRINT 'Sp_RTA_DashboardAprobacionJefatura actualizado.';
GO

SET NOEXEC OFF;
GO

PRINT '== Dashboard de aprobacion - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   1) Que exista:

SELECT nombre = name, creado = CONVERT(VARCHAR(20), modify_date, 120)
  FROM sys.procedures WHERE name = 'Sp_RTA_DashboardAprobacionJefatura';

   2) Que devuelva cinco conjuntos y que los totales cuadren con la pantalla.
      Usar un jefe y un rango reales, los mismos que se elijan en la pantalla:

EXEC dbo.Sp_RTA_DashboardAprobacionJefatura '1171', '01-03-2026', '31-03-2026';

   ============================================================================ */
```

- [ ] **Step 2: Verificar la forma del archivo sin ejecutarlo**

```bash
python -c "
import io, re
p = 'docs/sql/2026-09-21-dashboard-aprobacion.sql'
raw = io.open(p,'rb').read()
cuerpo = raw[3:] if raw[:3]==b'\xef\xbb\xbf' else raw
s = raw.decode('utf-8')
codigo = re.sub(r'/\*.*?\*/', '', s, flags=re.S)
print('BOM            :', raw[:3]==b'\xef\xbb\xbf')
print('no-ascii       :', sorted(set(b for b in bytearray(cuerpo) if b>127)) or 'ninguno')
print('CREATE/DROP    :', s.count('CREATE PROCEDURE'), '/', s.count('DROP PROCEDURE'))
print('SELECT finales :', len(re.findall(r'^    SELECT', codigo, re.M)))
print('temporal       :', codigo.count('CREATE TABLE #Base'), '/', codigo.count('DROP TABLE #Base'))
print('comentarios    :', s.count('/*') == s.count('*/'))
"
```

Esperado: `BOM: True`, `no-ascii: ninguno`, `CREATE/DROP: 1 / 1`,
`temporal: 1 / 1`, `comentarios: True`.

**Sobre el conteo de `SELECT`: no lo hagas por indentación.** Una versión anterior
de este paso contaba líneas que empezaran con exactamente cuatro espacios y
`SELECT`, y esperaba 5. Eso mide el formato, no la estructura: el `SELECT` del
`INSERT INTO #Base` también va a cuatro espacios, así que con el código correcto
el conteo da 6. Quien se encuentre con esa diferencia corre el riesgo de
re-indentar hasta que el número cuadre, que es acomodar el código a la
comprobación en vez de al revés.

Lo que de verdad importa —que el procedimiento devuelva **cinco** conjuntos, en
ese orden, porque la Task 4 los lee por posición— no se puede saber leyendo el
archivo: **se comprueba ejecutándolo**, y eso ocurre en el Step 4.

- [ ] **Step 3: Commit**

```bash
git add docs/sql/2026-09-21-dashboard-aprobacion.sql
git commit -m "feat(tareas): el procedimiento del dashboard, cinco conjuntos ya agregados"
```

- [ ] **Step 4: Que una persona lo corra y cuente los conjuntos**

**Este paso NO lo hace un subagente.** Quien tenga acceso a la base corre el
script y despues ejecuta el procedimiento con un jefe y un rango reales,
contando los conjuntos que devuelve:

```powershell
$c = $cn.CreateCommand(); $c.CommandText = "Sp_RTA_DashboardAprobacionJefatura"
$c.CommandType = [System.Data.CommandType]::StoredProcedure
[void]$c.Parameters.AddWithValue("@IdUsuarioJefe", "1171")
[void]$c.Parameters.AddWithValue("@FechaInicio", "01-03-2026")
[void]$c.Parameters.AddWithValue("@FechaFin", "31-03-2026")
$r = $c.ExecuteReader(); $n = 0
do { $n++ } while ($r.NextResult())
"conjuntos devueltos: $n"
$r.Close()
```

Esperado: **`conjuntos devueltos: 5`**. Es la única comprobación que mide lo que
la Task 4 necesita, y el archivo en disco no puede darla.

---

## Task 4: Las entidades y la lectura

**Files:**
- Modify: `CapaEntidad/EntDashboardAprobacion.cs`
- Create: `CapaDato/DaoDashboardAprobacion.cs`
- Modify: `CapaDato/CapaDato.csproj`

**Interfaces:**
- Consumes: el procedimiento de la Task 3; `EntDashboardEmpresa` de la Task 2.
- Produces:
  - `EntDashboardTotales` { `MinutosAprobados`, `MinutosPendientes`, `MinutosOtros`, `PersonasDiaAprob`, `PersonasDiaPend`, `Responsables` } — todos `int`
  - `EntDashboardSemana` { `Semana` (`DateTime`), `MinutosAprobados`, `MinutosPendientes` }
  - `EntDashboardResponsable` { `Nombre` (`string`), `MinutosAprobados`, `MinutosPendientes`, `PersonasDia`, `DiasBajoJornada` }
  - `EntDashboardDemora` { `DiasPromedio`, `DiasMaximo`, `AprobadasConFecha`, `AprobadasSinFecha`, `DiasMasViejoPendiente` }
  - `EntDashboardAprobacion` { `Totales`, `Semanas`, `Responsables`, `Demora`, `Empresas` }
  - `public static EntDashboardAprobacion DaoDashboardAprobacion.Cargar(string idUsuarioJefe, string fechaDesde, string fechaHasta)`
  La Task 5 usa `Cargar`.

- [ ] **Step 1: Completar las entidades**

Agregar a `CapaEntidad/EntDashboardAprobacion.cs`, **antes** de la llave de cierre
del `namespace` y dejando `EntDashboardEmpresa` donde está:

```csharp
    /// <summary>Las cifras de las tarjetas de arriba.</summary>
    public class EntDashboardTotales
    {
        public int MinutosAprobados { get; set; }
        public int MinutosPendientes { get; set; }

        /// <summary>
        /// Estados distintos de 1 y 2. Son los estados 5 y 7 -592 filas- que hoy
        /// no aparecen en ninguna pantalla ni en el catalogo. Se muestran
        /// agrupados para que el total de las tarjetas cuadre con el rango.
        /// </summary>
        public int MinutosOtros { get; set; }

        public int PersonasDiaAprob { get; set; }
        public int PersonasDiaPend { get; set; }
        public int Responsables { get; set; }
    }

    /// <summary>Un punto de la linea de evolucion. Semana es el lunes.</summary>
    public class EntDashboardSemana
    {
        public System.DateTime Semana { get; set; }
        public int MinutosAprobados { get; set; }
        public int MinutosPendientes { get; set; }
    }

    /// <summary>Una barra del grafico por persona.</summary>
    public class EntDashboardResponsable
    {
        public string Nombre { get; set; } = "";
        public int MinutosAprobados { get; set; }
        public int MinutosPendientes { get; set; }
        public int PersonasDia { get; set; }

        /// <summary>Dias del rango en que esa persona no llego a 8 horas.</summary>
        public int DiasBajoJornada { get; set; }
    }

    /// <summary>
    /// Cuanto se tarda en aprobar. AprobadasSinFecha no es decorativo: si crece,
    /// el promedio se calcula sobre cada vez menos filas y deja de representar.
    /// </summary>
    public class EntDashboardDemora
    {
        public int DiasPromedio { get; set; }
        public int DiasMaximo { get; set; }
        public int AprobadasConFecha { get; set; }
        public int AprobadasSinFecha { get; set; }
        public int DiasMasViejoPendiente { get; set; }
    }

    /// <summary>Los cinco bloques, tal como viajan al navegador.</summary>
    public class EntDashboardAprobacion
    {
        public EntDashboardTotales Totales { get; set; } = new EntDashboardTotales();
        public System.Collections.Generic.List<EntDashboardSemana> Semanas { get; set; }
            = new System.Collections.Generic.List<EntDashboardSemana>();
        public System.Collections.Generic.List<EntDashboardResponsable> Responsables { get; set; }
            = new System.Collections.Generic.List<EntDashboardResponsable>();
        public EntDashboardDemora Demora { get; set; } = new EntDashboardDemora();
        public System.Collections.Generic.List<EntDashboardEmpresa> Empresas { get; set; }
            = new System.Collections.Generic.List<EntDashboardEmpresa>();
    }
```

- [ ] **Step 2: Escribir el Dao**

`CapaDato/DaoDashboardAprobacion.cs`:

```csharp
using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Lee los cinco conjuntos del dashboard en UNA sola ida a la base.
    ///
    /// El orden de los NextResult() es el mismo en que el procedimiento declara
    /// los SELECT, y no hay forma de que el compilador lo verifique: si alguien
    /// agrega un conjunto en el medio, esta clase lee el equivocado sin dar
    /// error. Por eso el script tiene una comprobacion que cuenta los cinco.
    /// </summary>
    public class DaoDashboardAprobacion
    {
        public static EntDashboardAprobacion Cargar(string idUsuarioJefe, string fechaDesde, string fechaHasta)
        {
            EntDashboardAprobacion d = new EntDashboardAprobacion();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_DashboardAprobacionJefatura", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdUsuarioJefe", SqlDbType.VarChar, 10).Value = idUsuarioJefe ?? "";
                cmd.Parameters.Add("@FechaInicio", SqlDbType.VarChar, 20).Value = fechaDesde ?? "";
                cmd.Parameters.Add("@FechaFin", SqlDbType.VarChar, 20).Value = fechaHasta ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    /* 1. totales */
                    if (dr.Read())
                    {
                        d.Totales.MinutosAprobados  = Entero(dr, "MinutosAprobados");
                        d.Totales.MinutosPendientes = Entero(dr, "MinutosPendientes");
                        d.Totales.MinutosOtros      = Entero(dr, "MinutosOtros");
                        d.Totales.PersonasDiaAprob  = Entero(dr, "PersonasDiaAprob");
                        d.Totales.PersonasDiaPend   = Entero(dr, "PersonasDiaPend");
                        d.Totales.Responsables      = Entero(dr, "Responsables");
                    }

                    /* 2. por semana */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.Semanas.Add(new EntDashboardSemana
                            {
                                Semana            = Convert.ToDateTime(dr["Semana"]),
                                MinutosAprobados  = Entero(dr, "MinutosAprobados"),
                                MinutosPendientes = Entero(dr, "MinutosPendientes")
                            });
                        }
                    }

                    /* 3. por responsable */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.Responsables.Add(new EntDashboardResponsable
                            {
                                Nombre            = Texto(dr, "Nombre"),
                                MinutosAprobados  = Entero(dr, "MinutosAprobados"),
                                MinutosPendientes = Entero(dr, "MinutosPendientes"),
                                PersonasDia       = Entero(dr, "PersonasDia"),
                                DiasBajoJornada   = Entero(dr, "DiasBajoJornada")
                            });
                        }
                    }

                    /* 4. demora */
                    if (dr.NextResult() && dr.Read())
                    {
                        d.Demora.DiasPromedio          = Entero(dr, "DiasPromedio");
                        d.Demora.DiasMaximo            = Entero(dr, "DiasMaximo");
                        d.Demora.AprobadasConFecha     = Entero(dr, "AprobadasConFecha");
                        d.Demora.AprobadasSinFecha     = Entero(dr, "AprobadasSinFecha");
                        d.Demora.DiasMasViejoPendiente = Entero(dr, "DiasMasViejoPendiente");
                    }

                    /* 5. por empresa */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.Empresas.Add(new EntDashboardEmpresa
                            {
                                Empresa = Texto(dr, "Empresa"),
                                Minutos = Entero(dr, "Minutos")
                            });
                        }
                    }
                }
            }

            return d;
        }

        private static int Entero(SqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? 0 : Convert.ToInt32(dr[columna]);
        }

        private static string Texto(SqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? "" : dr[columna].ToString().Trim();
        }
    }
}
```

Registrarla en `CapaDato/CapaDato.csproj`:

```xml
    <Compile Include="DaoDashboardAprobacion.cs" />
```

- [ ] **Step 3: Compilar y correr las pruebas**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: compila y las pruebas siguen en verde. **Sin pruebas automáticas para el
`Dao`:** `CapaPruebas` sólo referencia `CapaEntidad` y `CapaNegocio`.

- [ ] **Step 4: Commit**

```bash
git add CapaEntidad/EntDashboardAprobacion.cs CapaDato/DaoDashboardAprobacion.cs CapaDato/CapaDato.csproj
git commit -m "feat(tareas): entidades y lectura de los cinco conjuntos del dashboard"
```

---

## Task 5: La acción del handler

**Files:**
- Modify: `ReporteTareas/Formulario/ObtenerListaTareas.ashx.cs`

**Interfaces:**
- Consumes: `DaoDashboardAprobacion.Cargar`, `NegDashboardAprobacion.TopConOtras`.
- Produces: la acción `"DashboardAprobacion"`, que devuelve el JSON de un
  `EntDashboardAprobacion` con las empresas ya reducidas a top 10 + «Otras».
  La Task 7 la consume.

> **La identidad de este handler viene del cliente.** `ObtenerListaTareas` se
> declara `IHttpHandler` **sin** `IRequiresSessionState`, así que `context.Session`
> es null y el usuario llega como parámetro cifrado que se resuelve con
> `SeguridadHelper.Desencripta`. La acción nueva **hereda ese mecanismo** —es el de
> las otras veinte— y el spec lo deja registrado como deuda, no como decisión de
> esta tarea. **No inventar aquí una comprobación de sesión**: no hay sesión que
> comprobar.

- [ ] **Step 1: Agregar el despacho**

En `ProcessRequest`, junto a los demás bloques, después del de
`ListaRecursosHorasDiarias`:

```csharp
                if (Action == "DashboardAprobacion")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDashboardAprobacion(parameters));
                }
```

- [ ] **Step 2: Escribir el método**

Después de `ObtenerRecursosHorasDiarias`:

```csharp
        /// <summary>
        /// Los cinco conjuntos del dashboard, con las empresas ya reducidas a un
        /// top 10 mas "Otras".
        ///
        /// La resolucion de quien consulta es la MISMA que ObtenerRecursosHorasDiarias,
        /// copiada a proposito: si el dashboard mirara a otra gente que la tabla de
        /// la misma pantalla, los numeros no cuadrarian y nadie sabria cual creer.
        /// </summary>
        public string ObtenerDashboardAprobacion(dynamic parameters)
        {
            SeguridadHelper seguridad = new SeguridadHelper();

            string idUsuario = parameters["usuario"].ToString();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string session = parameters["session"].ToString();

            string IdUsuarioConsulta = "";

            try
            {
                string IdUsuarioSession = seguridad.Desencripta(session.ToString());
                bool usuarioEsJefe = NegUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuarioSession);

                if (usuarioEsJefe)
                {
                    IdUsuarioConsulta = Convert.ToInt32(idUsuario) > 0 ? idUsuario : IdUsuarioSession;
                }
                else
                {
                    IdUsuarioConsulta = idUsuario;
                }

                EntDashboardAprobacion datos =
                    DaoDashboardAprobacion.Cargar(IdUsuarioConsulta, fechaDesde, fechaHasta);

                /* El top se arma aca y no en SQL: la regla esta probada en
                   NegDashboardAprobacion y en SQL quedaria sin prueba y repartida
                   en dos lenguajes. */
                datos.Empresas = NegDashboardAprobacion.TopConOtras(datos.Empresas, 10);

                return JsonConvert.SerializeObject(datos);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener el dashboard. " + ex.Message.ToString(), "danger", "");
            }
        }
```

Comprobar que los `using` necesarios ya están en el archivo (`CapaDato`,
`CapaEntidad`, `CapaNegocio`, `Newtonsoft.Json`); agregar los que falten.

- [ ] **Step 3: Comprobar el conteo de acciones**

```bash
git show HEAD:ReporteTareas/Formulario/ObtenerListaTareas.ashx.cs | grep -c 'if (Action == "'
grep -c 'if (Action == "' ReporteTareas/Formulario/ObtenerListaTareas.ashx.cs
```

El segundo tiene que ser el primero **más 1**. Si no, parar y avisar.

- [ ] **Step 4: Compilar el proyecto web**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas\ReporteTareas.csproj /p:Configuration=Debug /v:minimal
```

Esperado: compila sin errores.

- [ ] **Step 5: Commit**

```bash
git add ReporteTareas/Formulario/ObtenerListaTareas.ashx.cs
git commit -m "feat(tareas): la accion que sirve el dashboard de aprobacion"
```

---

## Task 6: La opción del combo y la tercera vista

**Files:**
- Modify: `ReporteTareas/Formulario/AprobacionTareasJefatura.aspx`
- Modify: `ReporteTareas/js/aprobacionTareasJefatura.js`

**Interfaces:**
- Consumes: nada de C#.
- Produces, con estos `id` exactos, que la Task 7 usa:
  - Contenedor: `Pagina3`
  - Tarjetas: `dashHorasAprobadas`, `dashHorasPendientes`, `dashHorasOtros`,
    `dashPersonasDia`, `dashDemoraPromedio`, `dashMasViejo`,
    `dashNoEjecutadas` (y su enlace, `dashIrNoEjecutadas`)
  - Lienzos: `graficoEvolucion`, `graficoPorPersona`, `graficoDemora`,
    `graficoPorEmpresa`
  - Mensajes: `dashMensaje`
  - La opción `<option value="4">DASHBOARD</option>`

- [ ] **Step 1: Agregar la opción al combo**

En `AprobacionTareasJefatura.aspx`, en el `<select id="cmbEstados">`:

```html
                                    <option value="1">PENDIENTE DE APROBACION</option>
                                    <option value="2">TAREAS APROBADAS</option>
                                    <option value="3">TAREAS NO EJECUTADAS</option>
                                    <option value="4">DASHBOARD</option>
```

- [ ] **Step 2: Agregar los dos `<script>`**

Junto a los que ya están, al principio del archivo. **`chart.umd.js` va antes** de
`dashboardAprobacion.js`: el segundo usa `Chart` al dibujar.

```html
    <script src="../js/chart.umd.js" type="text/javascript"></script>
    <script src="../js/dashboardAprobacion.js?v=1" type="text/javascript"></script>
```

- [ ] **Step 3: Agregar `Pagina3`**

Inmediatamente después del `</div>` que cierra `Pagina2`:

```html
                        <%-- La tercera vista. Nace oculta igual que Pagina2; la
                             enciende BtnConsulta cuando el combo vale 4. --%>
                        <div class="panel-body" style="display: none;" id="Pagina3">

                            <div id="dashMensaje" class="alert alert-warning" style="display: none"></div>

                            <div class="row">
                                <div class="col-lg-2 col-md-4 col-sm-6">
                                    <div class="panel panel-success">
                                        <div class="panel-body">
                                            <div style="font-size: 22px" id="dashHorasAprobadas">–</div>
                                            <div class="text-muted">Horas aprobadas</div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-2 col-md-4 col-sm-6">
                                    <div class="panel panel-warning">
                                        <div class="panel-body">
                                            <div style="font-size: 22px" id="dashHorasPendientes">–</div>
                                            <div class="text-muted">Horas pendientes</div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-2 col-md-4 col-sm-6">
                                    <div class="panel panel-default">
                                        <div class="panel-body">
                                            <div style="font-size: 22px" id="dashHorasOtros">–</div>
                                            <div class="text-muted" title="Estados distintos de pendiente y aprobado">Otros estados</div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-2 col-md-4 col-sm-6">
                                    <div class="panel panel-info">
                                        <div class="panel-body">
                                            <div style="font-size: 22px" id="dashPersonasDia">–</div>
                                            <div class="text-muted">Personas-día</div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-2 col-md-4 col-sm-6">
                                    <div class="panel panel-default">
                                        <div class="panel-body">
                                            <div style="font-size: 22px" id="dashDemoraPromedio">–</div>
                                            <div class="text-muted">Demora promedio</div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-2 col-md-4 col-sm-6">
                                    <div class="panel panel-danger">
                                        <div class="panel-body">
                                            <div style="font-size: 22px" id="dashMasViejo">–</div>
                                            <div class="text-muted">Lo más viejo sin aprobar</div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <%-- Las tareas no ejecutadas van en una tarjeta aparte y NUNCA
                                 dentro de un gráfico: una fila de esa consulta es una tarea,
                                 y las de arriba son horas y personas-día. Mezclarlas daría un
                                 número que se ve bien y no significa nada. --%>
                            <div class="row">
                                <div class="col-lg-4 col-md-6">
                                    <div class="panel panel-default">
                                        <div class="panel-body">
                                            <span style="font-size: 22px" id="dashNoEjecutadas">–</span>
                                            <span class="text-muted">tareas no ejecutadas en el rango</span>
                                            <a href="javascript:void(0)" id="dashIrNoEjecutadas"
                                               onclick="IrANoEjecutadas()">Ver el detalle</a>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-lg-8">
                                    <div class="panel panel-default">
                                        <div class="panel-heading">Evolución por semana</div>
                                        <div class="panel-body" style="height: 260px">
                                            <canvas id="graficoEvolucion"></canvas>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="panel panel-default">
                                        <div class="panel-heading">Horas por empresa</div>
                                        <div class="panel-body" style="height: 260px">
                                            <canvas id="graficoPorEmpresa"></canvas>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-lg-8">
                                    <div class="panel panel-default">
                                        <div class="panel-heading">Horas por persona</div>
                                        <div class="panel-body" style="height: 320px; overflow-y: auto">
                                            <canvas id="graficoPorPersona"></canvas>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="panel panel-default">
                                        <div class="panel-heading">Demora en aprobar</div>
                                        <div class="panel-body" style="height: 320px">
                                            <canvas id="graficoDemora"></canvas>
                                        </div>
                                    </div>
                                </div>
                            </div>

                        </div>
```

- [ ] **Step 4: Las dos ramas del JavaScript existente**

En `ReporteTareas/js/aprobacionTareasJefatura.js`, en `BtnConsulta`, agregar la
rama del 4 y **apagar `Pagina3` en las otras dos**, que hoy no la conocen:

```javascript
function BtnConsulta() {

    if ($("#cmbEstados").val() == "1" || $("#cmbEstados").val() == "2") {

        $("#divMensajes").html("");
        ObtenerListaRecursosHorasDiarias();
        document.getElementById("Pagina1").style.display = "block";
        document.getElementById("Pagina2").style.display = "none";
        document.getElementById("Pagina3").style.display = "none";
    }
    else if( $("#cmbEstados").val() == "3" ){
        document.getElementById("Pagina1").style.display = "none";
        document.getElementById("Pagina2").style.display = "block";
        document.getElementById("Pagina3").style.display = "none";
        tipoGeneral = 0;
        idregistroGeneral = 0;
        ObtenerListaTareasHorasExtras(0, 0);
    }
    else if ($("#cmbEstados").val() == "4") {
        $("#divMensajes").html("");
        document.getElementById("Pagina1").style.display = "none";
        document.getElementById("Pagina2").style.display = "none";
        document.getElementById("Pagina3").style.display = "block";
        CargarDashboardAprobacion();
    }
}
```

Y en `BuscarEstado`, que el botón de descarga no aparezca en el dashboard:

```javascript
function BuscarEstado() {

    if ($("#cmbEstados").val() == "1" || $("#cmbEstados").val() == "2" || $("#cmbEstados").val() == "4") {
        document.getElementById("btnDescarga").style.display = "none";
    }
    else if ($("#cmbEstados").val() == "3") {
        document.getElementById("btnDescarga").style.display = "block";
    }

}
```

- [ ] **Step 5: Verificar el BOM y los `id`**

```bash
python -c "
import io
raw = io.open('ReporteTareas/Formulario/AprobacionTareasJefatura.aspx','rb').read()
print('BOM:', raw[:3]==b'\xef\xbb\xbf')
s = raw.decode('utf-8-sig')
for x in ['Pagina3','graficoEvolucion','graficoPorPersona','graficoDemora','graficoPorEmpresa','dashMensaje','dashNoEjecutadas','dashIrNoEjecutadas']:
    print('%-20s %d' % (x, s.count('id=\"'+x+'\"')))
print('canvas:', s.count('<canvas'))
print('option 4:', s.count('<option value=\"4\">DASHBOARD</option>'))
"
node --check ReporteTareas/js/aprobacionTareasJefatura.js
```

Esperado: `BOM: True`, cada `id` exactamente `1`, `canvas: 4`, `option 4: 1`, y
`node --check` sin salida.

- [ ] **Step 6: Commit**

```bash
git add ReporteTareas/Formulario/AprobacionTareasJefatura.aspx ReporteTareas/js/aprobacionTareasJefatura.js
git commit -m "feat(tareas): la tercera vista del dashboard y su opcion en el combo"
```

---

## Task 7: Dibujar

**Files:**
- Create: `ReporteTareas/js/dashboardAprobacion.js`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: los `id` de la Task 6, la acción `DashboardAprobacion` de la Task 5, la
  global `Chart` de la Task 1, y del archivo existente: `$("#cmbUsuarios")`,
  `$("#txtFechaDesde")`, `$("#txtFechaHasta")`,
  `$("#ContentPlaceHolder1_txtUsuario")`.
- Produces: `CargarDashboardAprobacion()` e `IrANoEjecutadas()`, que llama la
  Task 6 desde el `onclick` de la tarjeta.

- [ ] **Step 1: Escribir el archivo**

`ReporteTareas/js/dashboardAprobacion.js`:

```javascript
/* Dashboard de aprobacion de tareas.
   ---------------------------------------------------------------------------
   Vive aparte de aprobacionTareasJefatura.js a proposito: aquel tiene 750 lineas
   y esta pantalla ya hace tres cosas distintas. Lo unico que comparten son los
   filtros de arriba, que se leen del DOM.

   Los cuatro graficos se guardan en _graficos para poder destruirlos antes de
   volver a dibujar: Chart.js NO reemplaza un grafico sobre un canvas ocupado,
   lo superpone, y al segundo "Consultar" quedan dos leyendas encimadas y el
   tooltip mostrando datos de la consulta anterior. */
var _graficos = {};

/* Paleta fija y no aleatoria: aprobado verde y pendiente amarillo, los mismos
   colores que la tabla usa para CumpleJornada. Que el mismo concepto cambie de
   color entre dos partes de la pantalla hace dudar de las dos. */
var DASH_VERDE = "#5cb85c";
var DASH_AMARILLO = "#f0ad4e";
var DASH_GRIS = "#999999";
var DASH_ROJO = "#d9534f";

var DASH_PALETA = ["#5cb85c", "#5bc0de", "#f0ad4e", "#d9534f", "#337ab7",
                   "#8e6cae", "#61b7a0", "#c9a227", "#7f8c8d", "#e07b39",
                   "#bdc3c7"];

function CargarDashboardAprobacion() {
    DashMensaje("");

    /* Si el archivo de Chart.js no se publico, decirlo aca. Sin esto el error
       sale como "Chart is not defined" en la consola del navegador, donde nadie
       mira, y la pantalla queda con seis tarjetas vacias sin explicacion. */
    if (typeof Chart === "undefined") {
        DashMensaje("No se pudo cargar la librería de gráficos. Avise a Sistemas: falta publicar js/chart.umd.js.");
        return;
    }

    var datos = "[{ \"action\": \"DashboardAprobacion\", \"parameters\" : { " +
                "\"usuario\" : \"" + $("#cmbUsuarios").val() + "\", " +
                "fechaDesde: \"" + $("#txtFechaDesde").val() + "\", " +
                "fechaHasta: \"" + $("#txtFechaHasta").val() + "\", " +
                "session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";

    $.ajax({
        type: "POST",
        url: "ObtenerListaTareas.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) {
            /* Un objeto con "estado" es la respuesta de error del handler, no el
               dashboard. Se distingue asi en todo el modulo. */
            if (r != null && typeof r.estado != "undefined") {
                DashMensaje(r.mensaje);
                return;
            }
            PintarDashboard(r);
        },
        error: function () {
            DashMensaje("No se pudo obtener el dashboard. Intente nuevamente.");
        }
    });
}

function DashMensaje(texto) {
    var $m = $("#dashMensaje");
    if (!texto) { $m.hide().text(""); return; }
    $m.text(texto).show();
}

/* Minutos a horas con un decimal. Es la MISMA regla que
   NegDashboardAprobacion.HorasDecimales; se repite aca solo para las etiquetas
   que se arman en el navegador, y por eso redondea igual. */
function DashHoras(minutos) {
    if (!minutos || minutos < 0) { return 0; }
    return Math.round((minutos / 60) * 10) / 10;
}

function PintarDashboard(d) {
    var t = d.Totales || {};

    $("#dashHorasAprobadas").text(DashHoras(t.MinutosAprobados) + " h");
    $("#dashHorasPendientes").text(DashHoras(t.MinutosPendientes) + " h");
    $("#dashHorasOtros").text(DashHoras(t.MinutosOtros) + " h");
    $("#dashPersonasDia").text((t.PersonasDiaAprob || 0) + (t.PersonasDiaPend || 0));

    var dem = d.Demora || {};
    $("#dashDemoraPromedio").text(DashTextoDemora(dem.DiasPromedio));
    $("#dashMasViejo").text(DashTextoDemora(dem.DiasMasViejoPendiente));

    PintarEvolucion(d.Semanas || []);
    PintarPorPersona(d.Responsables || []);
    PintarDemora(dem);
    PintarPorEmpresa(d.Empresas || []);

    CargarNoEjecutadas();
}

/* El conteo de tareas no ejecutadas sale de la accion que YA existe, la misma que
   usa la vista de Pagina2, y se cuentan las filas en el navegador.

   Pedir la lista entera para contarla es un desperdicio, y se elige igual: es la
   unica forma de garantizar que la tarjeta y la vista del detalle digan el mismo
   numero. Un conteo calculado aparte, con su propia consulta, es exactamente como
   dos partes de la misma pantalla terminan contradiciendose. */
function CargarNoEjecutadas() {
    var datos = "[{ \"action\": \"ListaConsultaTareasGeneradasPorRevisar\", \"parameters\" : { " +
                "\"usuario\" : \"" + $("#cmbUsuarios").val() + "\", " +
                "registro : \"0\", " +
                "fechaDesde: \"" + $("#txtFechaDesde").val() + "\", " +
                "fechaHasta: \"" + $("#txtFechaHasta").val() + "\", " +
                "estado: \"0\", busqueda: \"\", " +
                "session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";

    $.ajax({
        type: "POST", url: "ObtenerListaTareas.ashx", data: datos,
        contentType: "application/json; charset=utf-8", dataType: "json",
        success: function (r) {
            if (r != null && typeof r.estado != "undefined") {
                /* No se pisa el mensaje del dashboard con este error: el resto del
                   tablero es valido. La tarjeta dice que no se pudo y ya. */
                $("#dashNoEjecutadas").text("?");
                return;
            }
            $("#dashNoEjecutadas").text($.isArray(r) ? r.length : 0);
        },
        error: function () { $("#dashNoEjecutadas").text("?"); }
    });
}

/* Lleva a la vista que ya existe en vez de duplicar el detalle aca. Mueve el combo
   para que la pantalla quede coherente con lo que se esta mostrando. */
function IrANoEjecutadas() {
    $("#cmbEstados").val("3");
    BuscarEstado();
    BtnConsulta();
}

/* Misma regla que NegDashboardAprobacion.TextoDemora. */
function DashTextoDemora(dias) {
    if (!dias || dias <= 0) { return "hoy"; }
    if (dias === 1) { return "1 día"; }
    return dias + " días";
}

/* Destruye el grafico anterior de ese canvas, si lo hay, y dice "sin datos"
   cuando la serie viene vacia. Un canvas en blanco y un canvas que no se dibujo
   se ven igual. */
function DashPreparar(idCanvas, hayDatos) {
    if (_graficos[idCanvas]) {
        _graficos[idCanvas].destroy();
        delete _graficos[idCanvas];
    }

    var canvas = document.getElementById(idCanvas);
    var ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    if (!hayDatos) {
        ctx.font = "14px sans-serif";
        ctx.fillStyle = "#777";
        ctx.fillText("Sin datos en el rango", 10, 24);
        return null;
    }

    return ctx;
}

function PintarEvolucion(semanas) {
    var ctx = DashPreparar("graficoEvolucion", semanas.length > 0);
    if (!ctx) { return; }

    var etiquetas = [];
    var aprobadas = [];
    var pendientes = [];

    $.each(semanas, function (i, s) {
        /* La fecha llega como "/Date(...)/" o ISO segun el serializador; se
           parte la cadena ISO cuando se puede, y si no se cae a Date. Mostrar
           dd/MM alcanza: el anio ya esta en el filtro de arriba. */
        etiquetas.push(DashFechaCorta(s.Semana));
        aprobadas.push(DashHoras(s.MinutosAprobados));
        pendientes.push(DashHoras(s.MinutosPendientes));
    });

    _graficos["graficoEvolucion"] = new Chart(ctx, {
        type: "line",
        data: {
            labels: etiquetas,
            datasets: [
                { label: "Aprobadas", data: aprobadas, borderColor: DASH_VERDE, backgroundColor: DASH_VERDE, tension: 0.2 },
                { label: "Pendientes", data: pendientes, borderColor: DASH_AMARILLO, backgroundColor: DASH_AMARILLO, tension: 0.2 }
            ]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            scales: { y: { beginAtZero: true, title: { display: true, text: "Horas" } } }
        }
    });
}

function DashFechaCorta(valor) {
    if (!valor) { return ""; }

    var m = /\/Date\((-?\d+)\)\//.exec(valor);
    var f = m ? new Date(parseInt(m[1], 10)) : new Date(valor);
    if (isNaN(f.getTime())) { return String(valor); }

    var dd = ("0" + f.getDate()).slice(-2);
    var mm = ("0" + (f.getMonth() + 1)).slice(-2);
    return dd + "/" + mm;
}

function PintarPorPersona(responsables) {
    var ctx = DashPreparar("graficoPorPersona", responsables.length > 0);
    if (!ctx) { return; }

    var nombres = [];
    var aprobadas = [];
    var pendientes = [];

    $.each(responsables, function (i, r) {
        /* El nombre entra como dato de Chart.js, que lo dibuja en un canvas: no
           hay HTML donde inyectar nada. */
        nombres.push(r.Nombre);
        aprobadas.push(DashHoras(r.MinutosAprobados));
        pendientes.push(DashHoras(r.MinutosPendientes));
    });

    _graficos["graficoPorPersona"] = new Chart(ctx, {
        type: "bar",
        data: {
            labels: nombres,
            datasets: [
                { label: "Aprobadas", data: aprobadas, backgroundColor: DASH_VERDE },
                { label: "Pendientes", data: pendientes, backgroundColor: DASH_AMARILLO }
            ]
        },
        options: {
            indexAxis: "y",
            responsive: true, maintainAspectRatio: false,
            scales: { x: { stacked: true, beginAtZero: true, title: { display: true, text: "Horas" } },
                      y: { stacked: true } }
        }
    });
}

function PintarDemora(dem) {
    var hayDatos = (dem && (dem.AprobadasConFecha || 0) > 0);
    var ctx = DashPreparar("graficoDemora", hayDatos);
    if (!ctx) { return; }

    _graficos["graficoDemora"] = new Chart(ctx, {
        type: "bar",
        data: {
            labels: ["Promedio", "Máximo", "Más viejo pendiente"],
            datasets: [{
                label: "Días",
                data: [dem.DiasPromedio || 0, dem.DiasMaximo || 0, dem.DiasMasViejoPendiente || 0],
                backgroundColor: [DASH_GRIS, DASH_AMARILLO, DASH_ROJO]
            }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true, title: { display: true, text: "Días" } } }
        }
    });

    /* Si hay aprobadas sin fecha, el promedio se calcula sobre menos filas de las
       que el usuario cree. Se dice, en vez de dejar un numero que parece completo. */
    if ((dem.AprobadasSinFecha || 0) > 0) {
        DashMensaje("Atención: " + dem.AprobadasSinFecha +
                    " tarea(s) aprobadas no tienen fecha de aprobación y quedan fuera del cálculo de demora.");
    }
}

function PintarPorEmpresa(empresas) {
    var ctx = DashPreparar("graficoPorEmpresa", empresas.length > 0);
    if (!ctx) { return; }

    var nombres = [];
    var horas = [];

    $.each(empresas, function (i, e) {
        nombres.push(e.Empresa);
        horas.push(DashHoras(e.Minutos));
    });

    _graficos["graficoPorEmpresa"] = new Chart(ctx, {
        type: "doughnut",
        data: {
            labels: nombres,
            datasets: [{ data: horas, backgroundColor: DASH_PALETA }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: { legend: { position: "right", labels: { boxWidth: 12, font: { size: 10 } } } }
        }
    });
}
```

- [ ] **Step 2: Registrarlo en el `.csproj`**

```xml
    <Content Include="js\dashboardAprobacion.js" />
```

- [ ] **Step 3: Verificar sintaxis y cruce de nombres**

```bash
node --check ReporteTareas/js/dashboardAprobacion.js

python -c "
import io, re
js = io.open('ReporteTareas/js/dashboardAprobacion.js', encoding='utf-8').read()
aspx = io.open('ReporteTareas/Formulario/AprobacionTareasJefatura.aspx', encoding='utf-8-sig').read()
otro = io.open('ReporteTareas/js/aprobacionTareasJefatura.js', encoding='utf-8').read()
ids = set(re.findall(r'id=\"([^\"]+)\"', aspx))
sel = set(re.findall(r'\\\$\(\"#([A-Za-z][\\w]*)\"\)', js))
faltan = sorted(s for s in sel if s not in ids)
print('selectores sin id en el aspx:', faltan or 'ninguno')
canv = set(re.findall(r'getElementById\(\"(\w+)\"\)', js))
print('canvas sin id en el aspx    :', sorted(c for c in canv if c not in ids) or 'ninguno')
print('CargarDashboardAprobacion definida:', 'function CargarDashboardAprobacion' in js)
print('y llamada desde el otro archivo  :', 'CargarDashboardAprobacion()' in otro)
print('IrANoEjecutadas definida         :', 'function IrANoEjecutadas' in js)
print('BtnConsulta/BuscarEstado existen :', ('function BtnConsulta' in otro) and ('function BuscarEstado' in otro))
print('graficos que se destruyen        :', js.count('.destroy()'))
print('csproj                           :', io.open('ReporteTareas/ReporteTareas.csproj', encoding='utf-8-sig').read().count('js\\\\dashboardAprobacion.js'))
"
```

Esperado: sin salida de `node --check`; `ninguno` en las dos listas; las dos
funciones en `True`; `.destroy()` **1**; `csproj` **1**.

- [ ] **Step 4: Compilar el proyecto web**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas\ReporteTareas.csproj /p:Configuration=Debug /v:minimal
```

Esperado: compila sin errores.

- [ ] **Step 5: Commit**

```bash
git add ReporteTareas/js/dashboardAprobacion.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(tareas): los cuatro graficos del dashboard de aprobacion"
```

---

## Task 8: Despliegue y verificación a mano

**Files:**
- Modify: `DESPLIEGUE.md`

**Interfaces:**
- Consumes: todo lo anterior.
- Produces: la lista de archivos a publicar.

- [ ] **Step 1: Compilar en Release, regenerar el paquete y correr todas las pruebas**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas\ReporteTareas.csproj /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:Configuration=Release /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: compila, el paquete se regenera y **todas** las pruebas en verde.

- [ ] **Step 2: Comprobar que el paquete lleva los dos `.js` nuevos**

Es la comprobación que más veces se olvidó en este repositorio: un `.js` que no
está en el `.csproj` no viaja, y el dashboard sale sin librería y sin código.

```bash
P="ReporteTareas/obj/Release/Package/PackageTmp"
ls -la "$P/js/chart.umd.js" "$P/js/dashboardAprobacion.js"
grep -c 'id="Pagina3"' "$P/Formulario/AprobacionTareasJefatura.aspx"
```

Esperado: los dos archivos existen y `Pagina3` aparece `1` vez.

- [ ] **Step 3: Agregar la sección a `DESPLIEGUE.md`**

```markdown
## Dashboard de aprobación para jefatura

### 1. Base de datos

`docs/sql/2026-09-21-dashboard-aprobacion.sql` — crea
`Sp_RTA_DashboardAprobacionJefatura`. Es idempotente y no toca ninguna tabla: sólo
lee.

### 2. Archivos

- `Formulario\AprobacionTareasJefatura.aspx`
- `js\aprobacionTareasJefatura.js`
- `js\dashboardAprobacion.js` **(nuevo)**
- `js\chart.umd.js` **(nuevo, 208 KB)**
- `bin\ReporteTareas.dll`
- `bin\CapaEntidad.exe`
- `bin\CapaNegocio.exe`
- `bin\CapaDato.exe`

> Los tres de capa son **`.exe`**, no `.dll`.

**Copiar archivo por archivo. Nunca `robocopy /MIR`:** borra `connections.config` y
`appsettings.config`, y el sitio no levanta.

### 3. Si el dashboard sale en blanco

Lo primero a mirar es si `js/chart.umd.js` llegó al servidor. La pantalla lo avisa
con un mensaje, pero si no aparece ni el mensaje, es que tampoco llegó
`dashboardAprobacion.js`.
```

- [ ] **Step 4: Commit**

```bash
git add DESPLIEGUE.md
git commit -m "docs(tareas): como publicar el dashboard de aprobacion"
```

- [ ] **Step 5: Entregar la verificación a mano**

**La corre el usuario**, después de publicar y de correr el script.

| # | Qué hacer | Qué tiene que pasar |
|---|---|---|
| 1 | Abrir la pantalla y elegir PENDIENTE o APROBADAS | Todo funciona **igual que antes**; la tabla de siempre |
| 2 | Elegir TAREAS NO EJECUTADAS | Sigue funcionando igual, con su botón de descarga |
| 3 | Elegir DASHBOARD y Consultar | Aparecen 6 tarjetas y 4 gráficos; el botón de descarga **no** |
| 4 | **Comparar los totales** con lo que da la tabla de APROBADAS para el mismo rango | Las horas tienen que **coincidir**. Si no, uno de los dos miente y hay que parar |
| 5 | Elegir un rango sin datos | Los gráficos dicen «Sin datos en el rango», no quedan en blanco |
| 6 | Consultar dos veces seguidas | Los gráficos se redibujan limpios, sin leyendas encimadas |
| 7 | Cambiar de DASHBOARD a APROBADAS y volver | Las vistas se alternan sin dejar restos de la anterior |
| 8 | Con un jefe de los cuatro con código especial (`1314`, `1171`, `222692`, `1655906`) | Ve a todo el mundo, igual que en la tabla |
| 9 | Mirar la tarjeta de tareas no ejecutadas y pulsar «Ver el detalle» | El número coincide con las filas que lista la vista a la que salta |

El punto **4 es el que importa**: es la prueba de que el dashboard no inventa.

---

## Notas para quien ejecute este plan

**Lo que más fácil se rompe, en orden:**

1. **Olvidar los `.js` en el `.csproj`.** Son dos archivos nuevos. No se publican,
   no dan error de compilación, y el dashboard sale vacío.
2. **No destruir el gráfico anterior.** Chart.js superpone en vez de reemplazar; al
   segundo «Consultar» quedan dos leyendas y tooltips con datos viejos.
3. **Cambiar el parseo de horas «para mejorarlo».** El gráfico dejaría de coincidir
   con la tabla de la misma pantalla, que es lo único que hace creíble al tablero.
4. **Agregar o mover un `SELECT` en el procedimiento.** El `Dao` los lee por
   posición; uno de más en el medio y lee el equivocado, sin error.
5. **Guardar el `.aspx` sin BOM.** Sale a producción con los acentos rotos.
6. **«Arreglar» la lista de cuatro códigos escritos a mano** sólo en el
   procedimiento nuevo. Hay que hacerlo en los dos a la vez o los números dejan de
   cuadrar para esas cuatro personas.
