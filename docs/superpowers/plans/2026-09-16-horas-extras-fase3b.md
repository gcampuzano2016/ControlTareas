# Horas Extras — Fase 3b: la exportación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que Nómina pueda descargar un período de horas extras como archivo Excel, para llevarlo al proceso de pago.

**Architecture:** Un constructor puro en `CapaNegocio` que recibe el `EntHePantalla` que ya existe y devuelve los bytes de un `.xlsx`, y un handler `.ashx` aparte que los entrega. El archivo **se arma en memoria y nunca toca el disco**: lleva el sueldo de 62 personas. La descarga va por un handler propio y no por el JSON que ya existe, igual que `DescargarPerfil.ashx` en el módulo de perfil: uno responde JSON a un POST y el otro escribe bytes en un GET, y juntarlos obligaría a que uno rompiera su contrato.

**Tech Stack:** ASP.NET WebForms, .NET Framework 4.8 (web) / **4.6.1 (capas)**, EPPlus 6.2.7, MSTest v1.

**Spec:** `docs/superpowers/specs/2026-09-15-horas-extras-design.md`. Especificación funcional de origen: `Actualizacion/ESPEC_MODULO_HORAS_EXTRAS.md`.

**Estado de partida:** las fases 1, 2 y 3 están **en producción**. 154 pruebas verdes. Los binarios de las fases 2 y 3 **aún no se han desplegado**.

---

## Global Constraints

- **`.NET Framework 4.6.1`** en las capas: nada de `record`, `init` ni `switch` de expresión. La web es 4.8.
- **`decimal`, nunca `double`.**
- **`CapaNegocio → CapaDato → CapaEntidad`.** `CapaDato` no referencia `CapaNegocio`.
- **Sin tildes ni eñes en los comentarios de C#.** Los textos que ve el usuario —incluidos los encabezados del Excel— **sí las llevan**.
- **El repositorio de GitHub es PÚBLICO.** Ningún dato personal en el código, los comentarios, los mensajes de error ni las pruebas.
- **El `.aspx` se guarda con BOM.**
- Compilar con el MSBuild de **VS2019**.

## Decisiones tomadas

1. **El formato es «el Excel completo», y se ajustará cuando Nómina diga qué necesita.** Decidido por el usuario el 2026-09-16: la quinta pregunta abierta del §12 funcional sigue sin respuesta y no se puede adivinar el formato que espera un proceso aguas abajo. Este archivo sirve como insumo aunque luego haya que recortarlo o reordenarlo.
2. **Se puede exportar un período abierto, no sólo uno cerrado.** Revisar antes de cerrar es legítimo. Pero el archivo **dice en su primera hoja si el período estaba abierto**, para que nadie confunda un borrador con lo que se va a pagar.
3. **El archivo se arma en memoria, nunca en disco.** Lleva 62 sueldos. El módulo de perfil ya hace esto con el CV en PDF, por el mismo motivo.
4. **La descarga exige los mismos perfiles que la pantalla: 14 y 18.** Y la comprobación va en el handler: un archivo con 62 sueldos no se protege ocultando un botón.

---

## Estructura de archivos

| Archivo | Responsabilidad |
|---|---|
| `CapaNegocio/NegHeExportacion.cs` | Arma el `.xlsx` en memoria desde un `EntHePantalla`. No consulta la base ni sabe de HTTP |
| `CapaPruebas/NegHeExportacionTests.cs` | Sus pruebas: se abre el resultado con EPPlus y se leen las celdas |
| `ReporteTareas/Formulario/DescargarHorasExtras.ashx` (+ `.cs`) | Entrega los bytes. Comprueba sesión y perfil |
| `ReporteTareas/Formulario/HorasExtras.aspx` | El botón |
| `ReporteTareas/js/horasExtras.js` | Su comportamiento |

---

## Task 1: El constructor del archivo

**Files:**
- Create: `CapaNegocio/NegHeExportacion.cs`, `CapaPruebas/NegHeExportacionTests.cs`
- Modify: `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Consumes: `EntHePantalla`, `EntHePeriodo`, `EntHeFila` (fase 2).
- Produces:
  - `NegHeExportacion.NombreDeArchivo(EntHePeriodo periodo)` → `string`
  - `NegHeExportacion.Construir(EntHePantalla pantalla)` → `byte[]`

**Contexto que necesitas.**

EPPlus está en la solución (`EPPlus 6.2.7`) y **exige fijar el contexto de licencia antes del primer uso**, o lanza excepción. La casa lo hace dentro del propio método que lo usa — mira `CapaNegocio/TemplateExcel.cs:19` y `CapaNegocio/EditExcel.cs:66`:

```csharp
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
```

Los otros exportadores del sistema nombran sus archivos con código de documento controlado: `F-CS-001 Reporte de Tareas20260916_1432.xlsx`. **Sigue esa forma**, con el nombre del módulo y el período.

`TemplateExcel` rellena plantillas desde un archivo en disco. **Aquí no uses plantilla**: no existe ninguna para esto y crear una añadiría un archivo que hay que desplegar y que puede faltar sin dar error. Construye la hoja desde cero.

- [ ] **Step 1: Escribir las pruebas primero**

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System;
using System.IO;

namespace CapaPruebas
{
    /// <summary>
    /// El archivo se arma en memoria, asi que se puede probar entero sin tocar
    /// disco ni base: se construye, se vuelve a abrir con EPPlus y se leen las
    /// celdas.
    ///
    /// Los datos de estas pruebas son inventados a proposito. El repositorio es
    /// publico y un fixture con una cedula o un sueldo real seria una fuga.
    /// </summary>
    [TestClass]
    public class NegHeExportacionTests
    {
        private static EntHePantalla PantallaDeEjemplo(string estado)
        {
            EntHePantalla p = new EntHePantalla();
            p.Periodo = new EntHePeriodo();
            p.Periodo.IdPeriodo = 7;
            p.Periodo.Anio = 2026;
            p.Periodo.Mes = 9;
            p.Periodo.EstadoPeriodo = estado;

            EntHeFila f = new EntHeFila();
            f.CedulaSnapshot = "0000000000";
            f.NombreSnapshot = "PERSONA DE PRUEBA";
            f.EmpresaSnapshot = "EMPRESA DE PRUEBA";
            f.CargoSnapshot = "CARGO DE PRUEBA";
            f.JornadaHorasDiaSnapshot = 8;
            f.SalarioBaseSnapshot = 1200m;
            f.AplicaHESnapshot = true;
            f.Divisor = 240;
            f.ValorHoraOrdinaria = 5m;
            f.ValorHora50 = 7.5m;
            f.ValorHora100 = 10m;
            f.Horas50 = 10m;
            f.Horas100 = 2m;
            f.Total50 = 75m;
            f.Total100 = 20m;
            f.TotalHoras = 12m;
            f.TotalHE = 95m;
            f.Observacion = "";
            p.Filas.Add(f);

            p.TotalHoras50 = 10m;
            p.TotalHoras100 = 2m;
            p.TotalPago50 = 75m;
            p.TotalPago100 = 20m;
            p.TotalHoras = 12m;
            p.TotalPagar = 95m;
            return p;
        }

        private static ExcelWorksheet Abrir(byte[] bytes)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage paquete = new ExcelPackage(new MemoryStream(bytes));
            return paquete.Workbook.Worksheets[0];
        }

        [TestMethod]
        public void Construir_DevuelveUnXlsxQueSePuedeAbrir()
        {
            byte[] bytes = NegHeExportacion.Construir(PantallaDeEjemplo("Cerrado"));

            Assert.IsTrue(bytes.Length > 0);
            Assert.IsNotNull(Abrir(bytes));
        }

        /// <summary>
        /// El dinero tiene que salir como NUMERO, no como texto. Si saliera
        /// como texto, quien reciba el archivo no puede sumarlo ni pasarlo a
        /// su proceso sin convertirlo a mano, y una columna de 62 filas
        /// convertida a mano es justo donde se cuelan los errores.
        /// </summary>
        [TestMethod]
        public void Construir_ElDineroEsNumeroNoTexto()
        {
            byte[] bytes = NegHeExportacion.Construir(PantallaDeEjemplo("Cerrado"));
            ExcelWorksheet hoja = Abrir(bytes);

            object total = hoja.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaTotalHE].Value;

            Assert.IsInstanceOfType(total, typeof(double), "el total salio como " + total.GetType().Name);
            Assert.AreEqual(95d, (double)total, 0.001);
        }

        /// <summary>
        /// Un periodo abierto son cifras provisionales, y el archivo tiene que
        /// decirlo: si no, un borrador descargado el dia 20 es indistinguible
        /// del definitivo y alguien puede pagar con el.
        /// </summary>
        [TestMethod]
        public void Construir_PeriodoAbierto_LoDiceEnElArchivo()
        {
            ExcelWorksheet abierto = Abrir(NegHeExportacion.Construir(PantallaDeEjemplo("Abierto")));
            ExcelWorksheet cerrado = Abrir(NegHeExportacion.Construir(PantallaDeEjemplo("Cerrado")));

            string textoAbierto = Convert.ToString(abierto.Cells[NegHeExportacion.FilaDeEstado, 1].Value);
            string textoCerrado = Convert.ToString(cerrado.Cells[NegHeExportacion.FilaDeEstado, 1].Value);

            StringAssert.Contains(textoAbierto.ToUpperInvariant(), "PROVISIONAL");
            Assert.IsFalse(textoCerrado.ToUpperInvariant().Contains("PROVISIONAL"));
        }

        [TestMethod]
        public void NombreDeArchivo_LlevaElPeriodoYLaExtension()
        {
            EntHePeriodo p = new EntHePeriodo();
            p.Anio = 2026;
            p.Mes = 9;

            string nombre = NegHeExportacion.NombreDeArchivo(p);

            StringAssert.Contains(nombre, "2026");
            StringAssert.Contains(nombre, "09");
            StringAssert.EndsWith(nombre, ".xlsx");
        }

        /// <summary>
        /// Un periodo recien abierto sin nadie dentro no debe reventar la
        /// descarga: devuelve un archivo con sus encabezados y sin filas.
        /// </summary>
        [TestMethod]
        public void Construir_SinFilas_NoLanzaYDevuelveArchivo()
        {
            EntHePantalla vacia = PantallaDeEjemplo("Abierto");
            vacia.Filas.Clear();

            byte[] bytes = NegHeExportacion.Construir(vacia);

            Assert.IsTrue(bytes.Length > 0);
        }
    }
}
```

- [ ] **Step 2: Correrlas y verlas fallar**

```bash
MSYS_NO_PATHCONV=1 "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" CapaPruebas/CapaPruebas.csproj -t:Build -p:Configuration=Debug -v:quiet -nologo
```
Expected: **no compila**, con `El nombre 'NegHeExportacion' no existe`.

- [ ] **Step 3: Escribir `CapaNegocio/NegHeExportacion.cs`**

Una clase estática con:

- Las constantes públicas que las pruebas usan para no depender de números mágicos: `FilaDeEstado`, `PrimeraFilaDeDatos` y `ColumnaTotalHE`.
- `NombreDeArchivo(EntHePeriodo)`, con la forma de la casa: `F-CS-001 Reporte de Horas Extras <anio><mes>_<hhmmss>.xlsx`, con el mes a dos dígitos.
- `Construir(EntHePantalla)`, que fija `ExcelPackage.LicenseContext = LicenseContext.NonCommercial`, arma una hoja y devuelve `paquete.GetAsByteArray()`.

La hoja lleva, en este orden: un título con el período y su estado —y la palabra **PROVISIONAL** cuando no está cerrado—; quién y cuándo lo cerró si lo está; los encabezados; una fila por colaborador con las 18 columnas; y una fila de totales al final.

**Lo que no se puede hacer mal:**
- **Los números van como números.** Escribe `decimal` en la celda, no una cadena ya formateada. El formato se pone con `Style.Numberformat.Format`, que es cosa aparte. Si sale texto, quien recibe el archivo no puede sumarlo.
- **Nunca escribas el archivo a disco.** `GetAsByteArray()` y nada de `SaveAs(FileInfo)`.
- Los encabezados que ve la persona van **con tildes**; los comentarios del código, sin.

- [ ] **Step 4: Registrar los dos archivos en sus `.csproj` y compilar**

Sin la entrada `<Compile Include=...>` el archivo existe y no compila: nadie lo ve.

- [ ] **Step 5: Correr la suite**

Expected: **159 de 159** (154 previas + 5 nuevas). Si el número no cuadra, **dilo en tu reporte en vez de ajustarlo.**

- [ ] **Step 6: La prueba de ida y vuelta**

Cambia la escritura del total para que guarde `valor.ToString()` en vez del `decimal`, corre las pruebas y comprueba que `Construir_ElDineroEsNumeroNoTexto` **falla**. Deshaz. **Escribe en tu reporte qué dijo el fallo.**

- [ ] **Step 7: Commit**

```bash
git add CapaNegocio/NegHeExportacion.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegHeExportacionTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(horas-extras): el archivo de exportacion, armado en memoria"
```

---

## Task 2: La descarga y su botón

**Files:**
- Create: `ReporteTareas/Formulario/DescargarHorasExtras.ashx` (+ `.ashx.cs`)
- Modify: `ReporteTareas/Formulario/HorasExtras.aspx`, `ReporteTareas/js/horasExtras.js`, `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: `NegHeExportacion.Construir` y `.NombreDeArchivo` (tarea 1); `NegHorasExtrasPantalla.CargarPantalla` (fase 2).

**Contexto que necesitas.**

El molde exacto es `ReporteTareas/Formulario/DescargarPerfil.ashx.cs`: un `IHttpHandler` con **`IRequiresSessionState`** —sin eso `context.Session` es `null` y no hay identidad—, que hace `Response.ContentType`, `AddHeader("Content-Disposition", ...)` y `BinaryWrite`.

El tipo de contenido de un `.xlsx` es `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`.

**El orden de las comprobaciones importa, y es el mismo que usa `DescargarPerfil`:** sesión → perfil → parámetro. Cada una antes de tocar nada.

**La barrera de perfil va aquí.** El archivo lleva el sueldo de 62 personas; ocultar el botón no lo protege. Los perfiles son **14 y 18**, los mismos que la pantalla. El handler JSON ya tiene esa comprobación (`AdministrarHorasExtras.ashx.cs`, `PerfilesAutorizados`); **replica el criterio, no copies y pegues sin mirar** — si ves forma de compartirlo sin enredar las capas, mejor.

- [ ] **Step 1: El handler**

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Web;

namespace JsonJQueryNetHorasExtras
{
    /// <summary>
    /// La descarga del periodo en Excel. Handler aparte de
    /// AdministrarHorasExtras.ashx porque aquel responde JSON a peticiones POST
    /// y esto es un GET que escribe bytes: juntarlos obligaria a que uno de los
    /// dos rompiera su propio contrato.
    ///
    /// IRequiresSessionState no es decorativo: sin el, context.Session es null
    /// en un IHttpHandler y no habria con que comprobar el perfil.
    ///
    /// El archivo lleva el sueldo de todas las personas del periodo, asi que la
    /// comprobacion de perfil esta AQUI y no en el boton que lo pide.
    /// </summary>
    public class DescargarHorasExtras : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        private static readonly int[] PerfilesAutorizados = { 14, 18 };

        public void ProcessRequest(HttpContext context)
        {
            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.");
                return;
            }

            if (!TienePermiso(context))
            {
                NoDisponible(context, "Su perfil no puede descargar este archivo.");
                return;
            }

            int idPeriodo;
            if (!int.TryParse(context.Request.QueryString["periodo"], out idPeriodo) || idPeriodo <= 0)
            {
                NoDisponible(context, "No se indicó qué período descargar.");
                return;
            }

            EntRespuesta respuesta = NegHorasExtrasPantalla.CargarPantalla(idPeriodo);

            if (respuesta.estado != "1" || respuesta.resultado == null)
            {
                NoDisponible(context, "No se encontró ese período.");
                return;
            }

            EntHePantalla pantalla = (EntHePantalla)respuesta.resultado;
            byte[] archivo = NegHeExportacion.Construir(pantalla);

            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.AddHeader("Content-Disposition",
                "attachment;filename=\"" + NegHeExportacion.NombreDeArchivo(pantalla.Periodo) + "\"");
            context.Response.BinaryWrite(archivo);
        }

        private static bool TienePermiso(HttpContext context)
        {
            int idPerfil;
            if (!int.TryParse(Convert.ToString(context.Session["Id_Perfil"]), out idPerfil)) { return false; }
            return Array.IndexOf(PerfilesAutorizados, idPerfil) >= 0;
        }

        /// <summary>
        /// Un texto plano, no una pagina de error: quien esto recibe es una
        /// descarga, y el navegador va a mostrar lo que llegue.
        /// </summary>
        private static void NoDisponible(HttpContext context, string mensaje)
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.Write(mensaje);
        }

        public bool IsReusable { get { return false; } }
    }
}
```

El `.ashx` es una línea:
```
<%@ WebHandler Language="C#" CodeBehind="DescargarHorasExtras.ashx.cs" Class="JsonJQueryNetHorasExtras.DescargarHorasExtras" %>
```

- [ ] **Step 2: El botón**

En la cabecera, junto a los que ya existen: **«Exportar a Excel»**, habilitado sólo cuando hay un período cargado. Abre `DescargarHorasExtras.ashx?periodo=<id>`.

**Visible en período abierto y en cerrado** — revisar antes de cerrar es legítimo, y el archivo dice en su interior si las cifras son provisionales.

Si hay cambios sin guardar, **avisa antes de descargar**: el archivo sale de lo que hay en la base, no de lo que se ve en pantalla, y quien exporte con 40 filas sin guardar se llevaría un archivo que no coincide con lo que tiene delante. Usa el mismo modal informativo que ya usa el cierre.

- [ ] **Step 3: Registrar los archivos en `ReporteTareas.csproj`**

El `.ashx` como `<Content Include=...>` y el `.cs` como `<Compile Include=...>` con su `<DependentUpon>`. Copia la forma de las entradas de `DescargarPerfil.ashx` que ya están ahí.

- [ ] **Step 4: Comprobar el BOM**

```bash
head -c 3 ReporteTareas/Formulario/HorasExtras.aspx | od -An -tx1
```
Expected: `ef bb bf`.

- [ ] **Step 5: Compilar y correr la suite**

Expected: compila con 0 errores y **159 de 159**.

- [ ] **Step 6: Commit**

```bash
git add ReporteTareas/Formulario/DescargarHorasExtras.ashx ReporteTareas/Formulario/DescargarHorasExtras.ashx.cs ReporteTareas/Formulario/HorasExtras.aspx ReporteTareas/js/horasExtras.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(horas-extras): descargar el periodo en Excel, con el perfil comprobado en el handler"
```

---

## Cierre de la fase

1. **Regenerar el paquete de despliegue, AL FINAL**, después del último commit de código. Se ha quedado atrás seis veces en este proyecto y nunca por olvido: siempre por hacerlo antes de tiempo. Comprueba que el paquete contiene algo escrito en ese último commit.
2. **No hay script SQL en esta fase.** No hace falta ejecutar nada en producción; sólo desplegar binarios.
3. **Comprobación manual mínima:** con un usuario del perfil 14, descargar un período abierto y comprobar que el archivo dice **PROVISIONAL**; cerrarlo, volver a descargar y comprobar que ya no lo dice y que aparece quién lo cerró. Abrir el archivo y **sumar la columna del total** — si la suma funciona, los números salieron como números.
4. **Y lo que de verdad cierra esta fase no es técnico:** enseñarle el archivo a Nómina y preguntarles qué les sobra, qué les falta y en qué orden lo quieren. El formato está sin confirmar a sabiendas, y este archivo existe para que esa conversación tenga algo concreto delante.
