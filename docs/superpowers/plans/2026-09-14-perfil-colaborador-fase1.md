# Perfil del colaborador — Fase 1 · Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que cada colaborador entre a su propio perfil desde el desplegable de usuario, vea los datos que RRHH mantiene sobre él, actualice su contacto personal y registre a quién llamar en una emergencia.

**Architecture:** Pantalla WebForms bajo `Master.Master` que habla con un handler propio (`AdministrarPerfil.ashx`) por JSON. El eje es `R_Usuarios.Cod_Usuario`, tomado siempre de la sesión y nunca del payload; `dbo.Empleados` entra por `LEFT JOIN` para que los 113 usuarios sin ficha vean su perfil igual. La lógica que puede fallar en silencio se extrae a funciones puras en `CapaNegocio` para poder probarla sin base de datos.

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.8 en el web, 4.6.1 en las capas), SQL Server, jQuery + Bootstrap 3, MSTest v1 (ensamblado de VS2019, sin NuGet), `vstest.console.exe`.

**Spec:** `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md`

## Global Constraints

- **Rama:** `ProyectoNuevosCambios`. No commitear ni publicar sin que el usuario lo pida.
- **Compilar con el MSBuild de VS2019**, nunca el del PATH: `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"`.
- **`Cod_Usuario` sale de `context.Session["Cod_Usuario"]`. Nunca del payload.**
- **Toda conversión de `Empleados.Fecha_nacimiento` usa formato `dd/MM/yyyy` explícito.** Sin él la edad se rompe para 80 de 133 personas y no da ningún error.
- **Sin claves foráneas.** El esquema no tiene ninguna; agregar una daría integridad de mentira.
- **El script SQL corre antes que los binarios**, siempre, y es idempotente con `PRINT`.
- **`miPerfil.js` sale con `?v=1`** y cada despliegue posterior sube el número.
- Comentarios de código en español **sin tildes** (convención del repositorio); el texto de cara al usuario sí lleva tildes.
- Estilo SQL de `docs/sql/2026-08-25-*.sql`: `INT IDENTITY(1,1)`, constraints con nombre, `DATETIME2(0)` con `SYSDATETIME()`.

## Estructura de archivos

| Archivo | Responsabilidad |
|---|---|
| `docs/sql/2026-09-14-perfil-colaborador.sql` | Siete tablas, dos `ALTER`, poblado del enlace, SPs, aserciones |
| `CapaPruebas/CapaPruebas.csproj` | Proyecto de pruebas (nuevo, cuarto de la solución) |
| `CapaPruebas/NegPerfilCamposTests.cs` | Pruebas de las tres funciones puras |
| `CapaNegocio/NegPerfilCampos.cs` | Funciones puras: edad, lista blanca, validación de emergencia |
| `CapaEntidad/EntPerfilCabecera.cs` | Cabecera del perfil (lectura) |
| `CapaEntidad/EntPerfilContacto.cs` | Contacto editable |
| `CapaEntidad/EntPerfilEmergencia.cs` | Un contacto de emergencia |
| `CapaEntidad/EntPerfilCompleto.cs` | Contenedor de los siete result sets |
| `CapaDato/DaoPerfil.cs` | Acceso a datos del módulo |
| `CapaNegocio/NegPerfil.cs` | Fachada de negocio |
| `ReporteTareas/Formulario/AdministrarPerfil.ashx(.cs)` | Handler |
| `ReporteTareas/Formulario/MiPerfil.aspx(.cs)` | Pantalla |
| `ReporteTareas/js/miPerfil.js` | Comportamiento de la pantalla |
| `ReporteTareas/Formulario/Master.Master:104` | Enlace del desplegable (modificar) |

Las tres funciones puras van juntas en `NegPerfilCampos` porque comparten una sola responsabilidad: traducir lo que llega de afuera —texto de la base, payload del navegador— a algo confiable. Es lo que se puede probar sin base de datos, y por eso está separado de `NegPerfil`, que sí la toca.

---

### Task 1: Proyecto de pruebas y cálculo de edad

Es la primera tarea porque el proyecto de pruebas no existe todavía y la edad es la función que más silenciosamente puede fallar. La creación del proyecto va aquí, dentro de la tarea cuyo entregable la necesita.

**Files:**
- Create: `CapaPruebas/CapaPruebas.csproj`
- Create: `CapaPruebas/Properties/AssemblyInfo.cs`
- Create: `CapaPruebas/NegPerfilCamposTests.cs`
- Create: `CapaNegocio/NegPerfilCampos.cs`
- Modify: `ReporteTareas.sln`
- Modify: `CapaNegocio/CapaNegocio.csproj` (agregar el `Compile Include`)

**Interfaces:**
- Consumes: nada.
- Produces: `CapaNegocio.NegPerfilCampos.EdadDesdeTexto(string fechaTexto)` → `int?`. Devuelve la edad en años cumplidos, o `null` si el texto está vacío o no es una fecha `dd/MM/yyyy` válida.

- [ ] **Step 1: Crear el proyecto de pruebas**

Crear `CapaPruebas/CapaPruebas.csproj`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{7C3F1A62-9D84-4B57-A0E1-2F5C8B4D6E33}</ProjectGuid>
    <OutputType>Library</OutputType>
    <RootNamespace>CapaPruebas</RootNamespace>
    <AssemblyName>CapaPruebas</AssemblyName>
    <TargetFrameworkVersion>v4.6.1</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <!-- MSTest v1 viene con Visual Studio. No hace falta NuGet ni red. -->
    <Reference Include="Microsoft.VisualStudio.QualityTools.UnitTestFramework" />
    <Reference Include="System" />
    <Reference Include="System.Core" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="NegPerfilCamposTests.cs" />
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\CapaEntidad\CapaEntidad.csproj">
      <Project>{74CB46FC-75CC-4C3A-BE1C-6A16FDA79638}</Project>
      <Name>CapaEntidad</Name>
    </ProjectReference>
    <ProjectReference Include="..\CapaNegocio\CapaNegocio.csproj">
      <Project>{93C45744-E854-4720-AD80-85A44BF05A76}</Project>
      <Name>CapaNegocio</Name>
    </ProjectReference>
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
</Project>
```

Los GUID de `ProjectReference` son los reales de este repositorio, ya verificados. La dirección de dependencias de la solución es `CapaNegocio → CapaDato → CapaEntidad`; `CapaEntidad` no referencia a nadie. `CapaPruebas` referencia a las dos de arriba y nadie la referencia a ella.

Crear `CapaPruebas/Properties/AssemblyInfo.cs`:

```csharp
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("CapaPruebas")]
[assembly: AssemblyProduct("ReporteTareas")]
[assembly: ComVisible(false)]
[assembly: Guid("7c3f1a62-9d84-4b57-a0e1-2f5c8b4d6e33")]
[assembly: AssemblyVersion("1.0.0.0")]
```

Agregar el proyecto a `ReporteTareas.sln` con:

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe" ReporteTareas.sln /Command "File.AddExistingProject CapaPruebas\CapaPruebas.csproj"
```

Si `devenv` no está disponible, agregar a mano el bloque `Project(...) = "CapaPruebas"` copiando la forma de las entradas existentes en el `.sln`.

- [ ] **Step 2: Escribir la prueba que falla**

Crear `CapaPruebas/NegPerfilCamposTests.cs`:

```csharp
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CapaPruebas
{
    /// <summary>
    /// Las tres funciones que pueden fallar sin dar la cara. Un error aca no
    /// lanza excepcion ni pinta rojo en ningun lado: la edad sale mal, el campo
    /// bloqueado se guarda, el contacto de emergencia queda sin telefono.
    /// </summary>
    [TestClass]
    public class NegPerfilCamposTests
    {
        /* ---------------------------------------------------------- edad ---- */

        /// <summary>
        /// El caso que motiva toda esta funcion. En la base hay 129 fechas en
        /// formato dd/MM/yyyy y 80 de ellas tienen el dia por encima de 12. Si
        /// alguien las interpreta como mm/dd/yyyy, esas 80 fallan y las otras 49
        /// salen bien: el error se ve como "a algunos no les carga la edad".
        /// </summary>
        [TestMethod]
        public void EdadDesdeTexto_DiaMayorQueDoce_NoLoConfundeConElMes()
        {
            int? edad = NegPerfilCampos.EdadDesdeTexto("25/12/1990");

            Assert.IsNotNull(edad, "25/12/1990 es una fecha valida en dd/MM/yyyy");
            Assert.AreEqual(AniosDesde(new DateTime(1990, 12, 25)), edad.Value);
        }

        [TestMethod]
        public void EdadDesdeTexto_DiaMenorQueDoce_LeeElDiaPrimero()
        {
            // 03/07 es 3 de julio, no 7 de marzo. Sin formato explicito esta
            // fecha "funciona" en los dos sentidos y da edades distintas.
            int? edad = NegPerfilCampos.EdadDesdeTexto("03/07/1985");

            Assert.AreEqual(AniosDesde(new DateTime(1985, 7, 3)), edad.Value);
        }

        [TestMethod]
        public void EdadDesdeTexto_CumpleAunNoCumplido_RestaUnAnio()
        {
            DateTime manana = DateTime.Today.AddDays(1);
            string texto = manana.AddYears(-30).ToString("dd/MM/yyyy");

            Assert.AreEqual(29, NegPerfilCampos.EdadDesdeTexto(texto).Value,
                            "si el cumpleanios es manana todavia tiene 29");
        }

        [TestMethod]
        public void EdadDesdeTexto_CumpleHoy_CuentaElAnio()
        {
            string texto = DateTime.Today.AddYears(-30).ToString("dd/MM/yyyy");

            Assert.AreEqual(30, NegPerfilCampos.EdadDesdeTexto(texto).Value);
        }

        [TestMethod]
        public void EdadDesdeTexto_VacioOBasura_DevuelveNull()
        {
            // 4 empleados tienen la fecha vacia. No es un error: es que no se
            // cargo. La pantalla muestra un guion, no un cero ni una excepcion.
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto(null));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto(""));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto("   "));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto("no es fecha"));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto("31/02/1990"));
        }

        [TestMethod]
        public void EdadDesdeTexto_ConEspacios_LosIgnora()
        {
            Assert.IsNotNull(NegPerfilCampos.EdadDesdeTexto("  14/03/1996  "));
        }

        private static int AniosDesde(DateTime nacimiento)
        {
            DateTime hoy = DateTime.Today;
            int anios = hoy.Year - nacimiento.Year;
            if (nacimiento.Date > hoy.AddYears(-anios)) { anios--; }
            return anios;
        }
    }
}
```

- [ ] **Step 3: Compilar y confirmar que falla**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
```
Expected: FALLA la compilación con `CS0103: El nombre 'NegPerfilCampos' no existe en el contexto actual` (o `CS0246`). Esa es la falla correcta: la clase todavía no existe.

- [ ] **Step 4: Escribir la implementación mínima**

Crear `CapaNegocio/NegPerfilCampos.cs`:

```csharp
using System;
using System.Globalization;

namespace CapaNegocio
{
    /// <summary>
    /// Traduce lo que llega de afuera -texto guardado en la base, payload del
    /// navegador- a algo en lo que se pueda confiar.
    ///
    /// Esta clase no toca la base de datos ni HttpContext a proposito: es lo
    /// unico del modulo que se puede probar sin levantar nada, y es justamente
    /// donde estan los errores que no dan la cara.
    /// </summary>
    public static class NegPerfilCampos
    {
        /* Empleados.Fecha_nacimiento es nvarchar(50) y guarda dd/MM/yyyy. El
           formato va explicito y con InvariantCulture: si se deja que lo adivine
           la cultura del servidor, las fechas con dia mayor que 12 fallan y las
           demas se interpretan al reves sin avisar. */
        private const string FormatoFecha = "dd/MM/yyyy";

        /// <summary>
        /// Edad en anios cumplidos, o null si el texto esta vacio o no es una
        /// fecha valida. Null significa "no se sabe", y la pantalla muestra un
        /// guion; nunca un cero, que se leeria como un dato real.
        /// </summary>
        public static int? EdadDesdeTexto(string fechaTexto)
        {
            if (string.IsNullOrWhiteSpace(fechaTexto)) { return null; }

            DateTime nacimiento;
            bool valida = DateTime.TryParseExact(fechaTexto.Trim(),
                                                 FormatoFecha,
                                                 CultureInfo.InvariantCulture,
                                                 DateTimeStyles.None,
                                                 out nacimiento);
            if (!valida) { return null; }

            DateTime hoy = DateTime.Today;
            int anios = hoy.Year - nacimiento.Year;

            // Todavia no cumple anios este anio.
            if (nacimiento.Date > hoy.AddYears(-anios)) { anios--; }

            return anios;
        }
    }
}
```

Agregar a `CapaNegocio/CapaNegocio.csproj`, dentro del `<ItemGroup>` que lista los `.cs`:

```xml
<Compile Include="NegPerfilCampos.cs" />
```

- [ ] **Step 5: Correr las pruebas y confirmar que pasan**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```
Expected: `Total tests: 6. Passed: 6.`

- [ ] **Step 6: Commit**

Primero agregar al final de `.gitignore`:

```
# EXCEPCION a la regla de arriba: CapaPruebas no se despliega nunca -no lo
# referencia ningun proyecto- asi que su bin/ no tiene por que viajar. Son
# ~38 MB de dependencias que ya viven en el bin/ de las otras capas.
CapaPruebas/bin/
CapaPruebas/obj/
```

Después commitear **con rutas explícitas**. Un `git add CapaPruebas` a secas arrastra 60 archivos y 37.6 MB de binarios compilados:

```bash
git add .gitignore ReporteTareas.sln \
        CapaPruebas/CapaPruebas.csproj \
        CapaPruebas/Properties/AssemblyInfo.cs \
        CapaPruebas/NegPerfilCamposTests.cs \
        CapaNegocio/NegPerfilCampos.cs CapaNegocio/CapaNegocio.csproj
git commit -m "test(perfil): la edad se calcula con dd/MM/yyyy explicito y ya no depende de la cultura del servidor"
```

Confirmar con `git show --stat HEAD` que no aparece ningún `CapaPruebas/bin/*` ni `CapaPruebas/obj/*`.

---

### Task 2: Lista blanca de campos editables

**Files:**
- Create: `CapaEntidad/EntPerfilContacto.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj`
- Modify: `CapaNegocio/NegPerfilCampos.cs`
- Modify: `CapaPruebas/NegPerfilCamposTests.cs`

**Interfaces:**
- Consumes: `NegPerfilCampos` de la Task 1.
- Produces:
  - `CapaEntidad.EntPerfilContacto` con propiedades `string CorreoPersonal`, `string TelefonoPersonal`, `string Direccion`, `string EstadoCivil`.
  - `CapaNegocio.NegPerfilCampos.LeerContacto(System.Collections.Generic.IDictionary<string, object> campos)` → `EntPerfilContacto`. Lee **solo** esas cuatro claves; cualquier otra se descarta.

- [ ] **Step 1: Crear la entidad**

Crear `CapaEntidad/EntPerfilContacto.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Lo unico que el colaborador puede cambiar de su propio contacto.
    ///
    /// Que esta clase tenga exactamente cuatro propiedades no es casualidad: es
    /// la lista blanca. Un campo que no esta aca no se puede guardar, porque no
    /// hay donde ponerlo.
    /// </summary>
    public class EntPerfilContacto
    {
        public string CorreoPersonal { get; set; }
        public string TelefonoPersonal { get; set; }
        public string Direccion { get; set; }
        public string EstadoCivil { get; set; }
    }
}
```

Agregar a `CapaEntidad/CapaEntidad.csproj`:

```xml
<Compile Include="EntPerfilContacto.cs" />
```

- [ ] **Step 2: Escribir las pruebas que fallan**

Agregar dentro de la clase `NegPerfilCamposTests` en `CapaPruebas/NegPerfilCamposTests.cs`:

```csharp
        /* -------------------------------------------------- lista blanca ---- */

        /// <summary>
        /// El corazon del control de acceso del modulo. En la maqueta los campos
        /// bloqueados eran un disabled de CSS, que no detiene a nadie que sepa
        /// abrir la consola del navegador. Aca el payload puede traer lo que
        /// quiera: si no esta en la lista, no hay propiedad donde aterrice.
        /// </summary>
        [TestMethod]
        public void LeerContacto_PayloadConCamposBloqueados_LosIgnora()
        {
            var payload = new System.Collections.Generic.Dictionary<string, object>
            {
                { "correoPersonal",   "alguien@gmail.com" },
                { "telefonoPersonal", "0991234567" },
                { "direccion",        "Av. Amazonas y Naciones Unidas" },
                { "estadoCivil",      "Casado/a" },
                // Los que RRHH administra. Vienen en el payload a proposito.
                { "cargo",            "Gerente General" },
                { "areaTrabajo",      "Directorio" },
                { "cedula",           "9999999999" },
                { "fechaNacimiento",  "01/01/1900" },
                { "puestoTrabajo",    "Gerente General" }
            };

            EntPerfilContacto contacto = NegPerfilCampos.LeerContacto(payload);

            Assert.AreEqual("alguien@gmail.com", contacto.CorreoPersonal);
            Assert.AreEqual("0991234567", contacto.TelefonoPersonal);
            Assert.AreEqual("Av. Amazonas y Naciones Unidas", contacto.Direccion);
            Assert.AreEqual("Casado/a", contacto.EstadoCivil);

            // La comprobacion de verdad: la entidad no tiene forma de cargar un
            // cargo. Si alguien le agrega la propiedad, esta prueba deja de
            // compilar y obliga a mirar por que.
            Assert.AreEqual(4, typeof(EntPerfilContacto).GetProperties().Length,
                            "EntPerfilContacto es la lista blanca: cuatro campos, ni uno mas");
        }

        [TestMethod]
        public void LeerContacto_ClavesAusentes_QuedanEnCadenaVacia()
        {
            var payload = new System.Collections.Generic.Dictionary<string, object>
            {
                { "correoPersonal", "alguien@gmail.com" }
            };

            EntPerfilContacto contacto = NegPerfilCampos.LeerContacto(payload);

            Assert.AreEqual("alguien@gmail.com", contacto.CorreoPersonal);
            Assert.AreEqual("", contacto.TelefonoPersonal);
            Assert.AreEqual("", contacto.Direccion);
            Assert.AreEqual("", contacto.EstadoCivil);
        }

        [TestMethod]
        public void LeerContacto_RecortaEspacios()
        {
            var payload = new System.Collections.Generic.Dictionary<string, object>
            {
                { "correoPersonal", "  alguien@gmail.com  " }
            };

            Assert.AreEqual("alguien@gmail.com",
                            NegPerfilCampos.LeerContacto(payload).CorreoPersonal);
        }

        [TestMethod]
        public void LeerContacto_PayloadNulo_DevuelveEntidadVacia()
        {
            EntPerfilContacto contacto = NegPerfilCampos.LeerContacto(null);

            Assert.IsNotNull(contacto);
            Assert.AreEqual("", contacto.CorreoPersonal);
        }
```

Agregar al inicio del archivo, junto a los demás `using`:

```csharp
using CapaEntidad;
```

- [ ] **Step 3: Compilar y confirmar que falla**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
```
Expected: FALLA con `CS0117: 'NegPerfilCampos' no contiene una definición para 'LeerContacto'`.

- [ ] **Step 4: Implementar**

Agregar a `CapaNegocio/NegPerfilCampos.cs` (dentro de la clase, y `using CapaEntidad;` y `using System.Collections.Generic;` arriba):

```csharp
        /* Las unicas cuatro claves que se leen del payload. Todo lo demas que
           venga se descarta sin avisar: no es un error del cliente, es que no
           le corresponde. Cargo, area, cedula, fecha de nacimiento y puesto los
           administra RRHH y no tienen entrada por aca. */
        private static readonly string[] CamposPermitidos =
        {
            "correoPersonal", "telefonoPersonal", "direccion", "estadoCivil"
        };

        /// <summary>
        /// Arma el contacto editable a partir del payload, leyendo solo las
        /// claves permitidas.
        /// </summary>
        public static EntPerfilContacto LeerContacto(IDictionary<string, object> campos)
        {
            return new EntPerfilContacto
            {
                CorreoPersonal   = Texto(campos, "correoPersonal"),
                TelefonoPersonal = Texto(campos, "telefonoPersonal"),
                Direccion        = Texto(campos, "direccion"),
                EstadoCivil      = Texto(campos, "estadoCivil")
            };
        }

        /// <summary>
        /// El valor de una clave permitida, recortado. Cadena vacia si la clave
        /// no vino, si vino nula, o si no esta en la lista blanca.
        /// </summary>
        private static string Texto(IDictionary<string, object> campos, string clave)
        {
            if (campos == null) { return ""; }
            if (Array.IndexOf(CamposPermitidos, clave) < 0) { return ""; }

            object valor;
            if (!campos.TryGetValue(clave, out valor) || valor == null) { return ""; }

            return valor.ToString().Trim();
        }
```

- [ ] **Step 5: Correr las pruebas**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```
Expected: `Total tests: 10. Passed: 10.`

- [ ] **Step 6: Commit**

```bash
git add CapaEntidad CapaNegocio CapaPruebas
git commit -m "test(perfil): los campos que administra RRHH no tienen entrada por el payload"
```

---

### Task 3: Validación de contactos de emergencia

Un contacto de emergencia sin teléfono es peor que ninguno: ocupa el lugar del bueno y da una falsa sensación de que el dato está.

**Files:**
- Create: `CapaEntidad/EntPerfilEmergencia.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj`
- Modify: `CapaNegocio/NegPerfilCampos.cs`
- Modify: `CapaPruebas/NegPerfilCamposTests.cs`

**Interfaces:**
- Consumes: `NegPerfilCampos` de las tareas 1 y 2.
- Produces:
  - `CapaEntidad.EntPerfilEmergencia` con `int IdContacto`, `string Nombre`, `string Parentesco`, `string Telefono`.
  - `CapaNegocio.NegPerfilCampos.ValidarEmergencia(EntPerfilEmergencia contacto)` → `string`. Devuelve cadena vacía si es válido, o el mensaje de error listo para mostrar al usuario (con tildes).

- [ ] **Step 1: Crear la entidad**

Crear `CapaEntidad/EntPerfilEmergencia.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>A quien llamar. Una fila por contacto; puede haber varios.</summary>
    public class EntPerfilEmergencia
    {
        public int IdContacto { get; set; }
        public string Nombre { get; set; }
        public string Parentesco { get; set; }
        public string Telefono { get; set; }
    }
}
```

Agregar a `CapaEntidad/CapaEntidad.csproj`:

```xml
<Compile Include="EntPerfilEmergencia.cs" />
```

- [ ] **Step 2: Escribir las pruebas que fallan**

Agregar dentro de `NegPerfilCamposTests`:

```csharp
        /* ------------------------------------------- emergencia valida ------ */

        [TestMethod]
        public void ValidarEmergencia_CompletoYCorrecto_SinError()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "0987654321"
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_SinNombre_Rechaza()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "   ", Parentesco = "Madre", Telefono = "0987654321"
            };

            Assert.AreEqual("Escriba el nombre del contacto de emergencia.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_SinTelefono_Rechaza()
        {
            // El caso que importa: un contacto sin telefono ocupa el lugar del
            // bueno y hace creer que el dato esta.
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = ""
            };

            Assert.AreEqual("Escriba el teléfono del contacto de emergencia.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_TelefonoSinDigitosSuficientes_Rechaza()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "no tengo"
            };

            Assert.AreEqual("El teléfono debe tener al menos 7 dígitos.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_TelefonoConGuionesYEspacios_LoAcepta()
        {
            // La gente escribe "099 123-4567". Contar digitos, no caracteres.
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "099 123-4567"
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_SinParentesco_Rechaza()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "", Telefono = "0987654321"
            };

            Assert.AreEqual("Indique el parentesco del contacto de emergencia.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_ContactoNulo_Rechaza()
        {
            Assert.AreEqual("No se recibió el contacto de emergencia.",
                            NegPerfilCampos.ValidarEmergencia(null));
        }
```

- [ ] **Step 3: Compilar y confirmar que falla**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
```
Expected: FALLA con `CS0117: 'NegPerfilCampos' no contiene una definición para 'ValidarEmergencia'`.

- [ ] **Step 4: Implementar**

Agregar a `CapaNegocio/NegPerfilCampos.cs`:

```csharp
        /// <summary>Minimo de digitos de un telefono utilizable en Ecuador.</summary>
        private const int DigitosMinimosTelefono = 7;

        /// <summary>
        /// Cadena vacia si el contacto sirve; si no, el mensaje para el usuario.
        ///
        /// Se valida aca y no solo en el navegador porque el handler es
        /// alcanzable por HTTP directo: una validacion que solo vive en el
        /// cliente no es una validacion.
        /// </summary>
        public static string ValidarEmergencia(EntPerfilEmergencia contacto)
        {
            if (contacto == null)
            {
                return "No se recibió el contacto de emergencia.";
            }

            if (string.IsNullOrWhiteSpace(contacto.Nombre))
            {
                return "Escriba el nombre del contacto de emergencia.";
            }

            if (string.IsNullOrWhiteSpace(contacto.Parentesco))
            {
                return "Indique el parentesco del contacto de emergencia.";
            }

            if (string.IsNullOrWhiteSpace(contacto.Telefono))
            {
                return "Escriba el teléfono del contacto de emergencia.";
            }

            /* Se cuentan digitos, no caracteres: "099 123-4567" es un telefono
               perfectamente valido y la gente lo escribe asi. */
            int digitos = 0;
            foreach (char c in contacto.Telefono)
            {
                if (char.IsDigit(c)) { digitos++; }
            }

            if (digitos < DigitosMinimosTelefono)
            {
                return "El teléfono debe tener al menos 7 dígitos.";
            }

            return "";
        }
```

- [ ] **Step 5: Correr las pruebas**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```
Expected: `Total tests: 17. Passed: 17.`

- [ ] **Step 6: Commit**

```bash
git add CapaEntidad CapaNegocio CapaPruebas
git commit -m "test(perfil): un contacto de emergencia sin telefono util se rechaza en el servidor"
```

---

### Task 4: Script SQL

Crea las siete tablas de una vez —aunque la fase 1 use tres— para que las fases 2 y 3 no vuelvan a tocar el esquema en producción. Una sola ventana de cambio de base en vez de tres.

**Files:**
- Create: `docs/sql/2026-09-14-perfil-colaborador.sql`

**Interfaces:**
- Consumes: nada.
- Produces: tablas `Perfil_ContactoPersonal`, `Perfil_ContactoEmergencia`, `Perfil_Estudio`, `Perfil_Certificacion`, `Perfil_Experiencia`, `Perfil_Documento`, `Perfil_Foto`; columnas `dbo.Empleados.Cod_Usuario` y `dbo.Emp_CargaFamiliar.Cod_Usuario`; procedimiento `dbo.Sp_RTA_PerfilColaborador @Cod_Usuario` con siete result sets.

- [ ] **Step 1: Escribir la cabecera, las tablas y los ALTER**

Crear `docs/sql/2026-09-14-perfil-colaborador.sql`:

```sql
/* ============================================================================
   Modulo: Perfil del colaborador (MiPerfil.aspx)
   Spec  : docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md

   1. Siete tablas nuevas, todas con eje Cod_Usuario
   2. Cod_Usuario en Empleados y en Emp_CargaFamiliar
   3. Poblado del enlace por cedula
   4. Sp_RTA_PerfilColaborador
   5. Aserciones y reporte de excepciones para RRHH

   Las secciones estan en el orden en que deben ejecutarse.
   Se puede ejecutar varias veces sin efecto adicional.
   Base: ReporTarea
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ------------------------------------------------------------ 1. tablas --- */

/* Una fila por persona: aca no hay borrado logico porque no significa nada.
   Nace esta tabla, y no se reusa Empleados.Correo, porque ese campo tiene 108
   direcciones corporativas y 23 externas: no es un correo personal, es un
   correo a secas. Ver la seccion "Ninguno de los dos correos..." del spec. */
IF OBJECT_ID('dbo.Perfil_ContactoPersonal','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_ContactoPersonal
    (
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        CorreoPersonal   VARCHAR(150) NULL,
        TelefonoPersonal VARCHAR(50)  NULL,
        Direccion        VARCHAR(400) NULL,

        Fec_Modificacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilContactoPersonal_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_ContactoPersonal PRIMARY KEY (Cod_Usuario)
    );
    PRINT 'Perfil_ContactoPersonal creada.';
END
ELSE PRINT 'Perfil_ContactoPersonal ya existia.';
GO

IF OBJECT_ID('dbo.Perfil_ContactoEmergencia','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_ContactoEmergencia
    (
        IdContacto       INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        Nombre           VARCHAR(150) NOT NULL,
        Parentesco       VARCHAR(50)  NULL,
        Telefono         VARCHAR(50)  NULL,

        Estado           CHAR(1)      NOT NULL
            CONSTRAINT DF_PerfilContactoEmergencia_Estado DEFAULT ('1'),
        Fec_Modificacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilContactoEmergencia_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_ContactoEmergencia PRIMARY KEY (IdContacto)
    );
    CREATE INDEX IX_Perfil_ContactoEmergencia_Usuario
        ON dbo.Perfil_ContactoEmergencia (Cod_Usuario, Estado);
    PRINT 'Perfil_ContactoEmergencia creada.';
END
ELSE PRINT 'Perfil_ContactoEmergencia ya existia.';
GO

IF OBJECT_ID('dbo.Perfil_Estudio','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_Estudio
    (
        IdEstudio        INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        Nivel            VARCHAR(60)  NULL,
        Institucion      VARCHAR(200) NULL,
        Titulo           VARCHAR(200) NULL,
        AnioGraduacion   SMALLINT     NULL,

        Estado           CHAR(1)      NOT NULL
            CONSTRAINT DF_PerfilEstudio_Estado DEFAULT ('1'),
        Fec_Modificacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilEstudio_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_Estudio PRIMARY KEY (IdEstudio)
    );
    CREATE INDEX IX_Perfil_Estudio_Usuario ON dbo.Perfil_Estudio (Cod_Usuario, Estado);
    PRINT 'Perfil_Estudio creada.';
END
ELSE PRINT 'Perfil_Estudio ya existia.';
GO

IF OBJECT_ID('dbo.Perfil_Certificacion','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_Certificacion
    (
        IdCertificacion  INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        Nombre           VARCHAR(200) NULL,
        Entidad          VARCHAR(200) NULL,
        FechaObtencion   DATE         NULL,

        Estado           CHAR(1)      NOT NULL
            CONSTRAINT DF_PerfilCertificacion_Estado DEFAULT ('1'),
        Fec_Modificacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilCertificacion_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_Certificacion PRIMARY KEY (IdCertificacion)
    );
    CREATE INDEX IX_Perfil_Certificacion_Usuario
        ON dbo.Perfil_Certificacion (Cod_Usuario, Estado);
    PRINT 'Perfil_Certificacion creada.';
END
ELSE PRINT 'Perfil_Certificacion ya existia.';
GO

/* AnioDesde/AnioHasta separados y numericos, y no el "2017 - 2019" de texto
   libre de la maqueta, porque el CV ordena la experiencia por fecha y con
   texto no se puede. AnioHasta NULL significa "hasta hoy". */
IF OBJECT_ID('dbo.Perfil_Experiencia','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_Experiencia
    (
        IdExperiencia    INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        Empresa          VARCHAR(200) NULL,
        Cargo            VARCHAR(200) NULL,
        AnioDesde        SMALLINT     NULL,
        AnioHasta        SMALLINT     NULL,
        Funciones        VARCHAR(MAX) NULL,

        Estado           CHAR(1)      NOT NULL
            CONSTRAINT DF_PerfilExperiencia_Estado DEFAULT ('1'),
        Fec_Modificacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilExperiencia_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)  NULL,
        Ip_Modificacion  VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_Experiencia PRIMARY KEY (IdExperiencia)
    );
    CREATE INDEX IX_Perfil_Experiencia_Usuario
        ON dbo.Perfil_Experiencia (Cod_Usuario, Estado);
    PRINT 'Perfil_Experiencia creada.';
END
ELSE PRINT 'Perfil_Experiencia ya existia.';
GO

/* Una sola tabla para todos los respaldos. Origen dice de que cuelga cada uno
   e IdOrigen a cual. Asi el documento de una certificacion y el de una carga
   familiar se suben, listan y borran con el mismo codigo. */
IF OBJECT_ID('dbo.Perfil_Documento','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_Documento
    (
        IdDocumento         INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario         VARCHAR(50)  NOT NULL,
        Origen              VARCHAR(20)  NOT NULL,   -- CERTIFICACION | CARGAFAMILIAR
        IdOrigen            INT          NOT NULL,
        NombreArchivo       VARCHAR(260) NULL,
        NombreArchivoCodigo VARCHAR(260) NULL,
        Ruta                VARCHAR(400) NULL,

        Estado              CHAR(1)      NOT NULL
            CONSTRAINT DF_PerfilDocumento_Estado DEFAULT ('1'),
        Fec_Modificacion    DATETIME2(0) NOT NULL
            CONSTRAINT DF_PerfilDocumento_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion    VARCHAR(50)  NULL,
        Ip_Modificacion     VARCHAR(64)  NULL,

        CONSTRAINT PK_Perfil_Documento PRIMARY KEY (IdDocumento)
    );
    CREATE INDEX IX_Perfil_Documento_Origen
        ON dbo.Perfil_Documento (Cod_Usuario, Origen, IdOrigen, Estado);
    PRINT 'Perfil_Documento creada.';
END
ELSE PRINT 'Perfil_Documento ya existia.';
GO

/* Tabla aparte y no una columna de R_Usuarios: un VARCHAR(MAX) en la tabla que
   se lee en cada request del menu se paga en todas las pantallas. Mismo patron
   que la firma guardada (DaoFirmaUsuario): base64 + tipo, y el data URI se arma
   al leer. */
IF OBJECT_ID('dbo.Perfil_Foto','U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_Foto
    (
        Cod_Usuario      VARCHAR(50)   NOT NULL,
        FotoBase64       VARCHAR(MAX)  NULL,
        FotoTipo         VARCHAR(50)   NULL,

        Fec_Modificacion DATETIME2(0)  NOT NULL
            CONSTRAINT DF_PerfilFoto_Fec DEFAULT (SYSDATETIME()),
        Usu_Modificacion VARCHAR(50)   NULL,
        Ip_Modificacion  VARCHAR(64)   NULL,

        CONSTRAINT PK_Perfil_Foto PRIMARY KEY (Cod_Usuario)
    );
    PRINT 'Perfil_Foto creada.';
END
ELSE PRINT 'Perfil_Foto ya existia.';
GO

/* ------------------------------------------- 2. Cod_Usuario en lo existente */

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Empleados' AND COLUMN_NAME = 'Cod_Usuario')
BEGIN
    ALTER TABLE dbo.Empleados ADD Cod_Usuario VARCHAR(50) NULL;
    PRINT 'Empleados.Cod_Usuario creada.';
END
ELSE PRINT 'Empleados.Cod_Usuario ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Empleados_Cod_Usuario')
BEGIN
    CREATE INDEX IX_Empleados_Cod_Usuario ON dbo.Empleados (Cod_Usuario);
    PRINT 'IX_Empleados_Cod_Usuario creado.';
END
GO

/* Emp_CargaFamiliar tiene 0 filas: nunca se uso. Recibe Cod_Usuario para que
   los 113 usuarios sin ficha de empleado tambien puedan registrar cargas. */
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Emp_CargaFamiliar' AND COLUMN_NAME = 'Cod_Usuario')
BEGIN
    ALTER TABLE dbo.Emp_CargaFamiliar ADD Cod_Usuario VARCHAR(50) NULL;
    PRINT 'Emp_CargaFamiliar.Cod_Usuario creada.';
END
ELSE PRINT 'Emp_CargaFamiliar.Cod_Usuario ya existia.';
GO
```

- [ ] **Step 2: Escribir el poblado del enlace**

Agregar al mismo archivo:

```sql
/* ------------------------------------------------- 3. poblado del enlace --- */

/* Se enlaza por cedula, que es la unica llave que resiste el dato real:
   Cod_Sap no sirve -63 de 133 empleados lo tienen en 0 o nulo-.

   Se excluyen a proposito las cedulas repetidas en R_Usuarios (6 usuarios
   activos): dos logins con la misma cedula apuntarian a la misma ficha y no
   hay forma de saber cual es cual. Salen en el reporte de la seccion 8 para
   que RRHH los resuelva. Es el mismo criterio de DaoFirmaUsuario cuando el
   codigo de usuario esta repetido: antes que adivinar, no responder.

   Solo se escribe donde esta vacio, asi que correrlo de nuevo no pisa un
   enlace corregido a mano. */
UPDATE e
   SET e.Cod_Usuario = u.Cod_Usuario
  FROM dbo.Empleados e
  JOIN (
        SELECT LTRIM(RTRIM(Cedula)) AS Cedula, MIN(Cod_Usuario) AS Cod_Usuario
          FROM dbo.R_Usuarios
         WHERE ISNULL(EstadoUsuario, 0) = 0
           AND ISNULL(LTRIM(RTRIM(Cedula)), '') <> ''
         GROUP BY LTRIM(RTRIM(Cedula))
        HAVING COUNT(*) = 1          -- cedula repetida: no se enlaza
       ) u
    ON LTRIM(RTRIM(e.Cedula)) = u.Cedula
 WHERE ISNULL(e.Cod_Usuario, '') = '';

PRINT 'Enlace poblado: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' fichas enlazadas en esta corrida.';
GO
```

- [ ] **Step 3: Escribir `Sp_RTA_PerfilColaborador`**

Agregar al mismo archivo:

```sql
/* --------------------------------------------------- 4. lectura del perfil */

IF OBJECT_ID('dbo.Sp_RTA_PerfilColaborador','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilColaborador;
GO

/* Todo el perfil de una persona en una sola ida: siete result sets.

   La cabecera es un LEFT JOIN contra Empleados a proposito. 113 de 231
   usuarios activos no tienen ficha enlazada; con INNER JOIN entrarian y verian
   una pantalla en blanco. Con LEFT ven su nombre, su area y todo lo que si se
   sabe de ellos, y las secciones nuevas -que cuelgan de Cod_Usuario- les
   funcionan completas.

   Fecha_nacimiento sale como TEXTO, sin convertir. Es nvarchar(50) con formato
   dd/MM/yyyy y convertirla aca obliga a acertarle al estilo (103) o la edad se
   rompe para 80 de 133 personas sin dar error. La conversion la hace
   NegPerfilCampos.EdadDesdeTexto, que tiene pruebas.

   Los result sets 3 a 7 devuelven vacio en la fase 1 porque todavia no hay
   pantalla que los llene. El procedimiento ya los declara para que las fases 2
   y 3 no tengan que volver a tocar la base. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilColaborador
    @Cod_Usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    /* 1. cabecera */
    SELECT  u.Cod_Usuario,
            NombreCompleto  = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario),
            Cedula          = ISNULL(NULLIF(LTRIM(RTRIM(u.Cedula)), ''), e.Cedula),
            FechaNacTexto   = LTRIM(RTRIM(ISNULL(e.Fecha_nacimiento, ''))),
            Cargo           = ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo),
            Area            = ISNULL(NULLIF(LTRIM(RTRIM(e.AreaTrabajo)), ''), u.Departamento),
            Ciudad          = e.Ciudad,
            CorreoNotificacion = u.E_Mail,
            JefeInmediato   = j.Nom_Usuario,

            /* El horario va como subconsulta y NO como LEFT JOIN: hay 5 usuarios
               con mas de una asignacion activa a la vez, y un join los duplicaria.
               La cabecera tiene que devolver exactamente una fila siempre, porque
               el Dao hace un solo Read(): con un join, esas 5 personas verian un
               horario elegido al azar y nadie se enteraria.

               R_UsuarioHorarioLaboral.Id_Responsable guarda un Cod_Usuario, pese
               al nombre. Solo 88 de 231 tienen horario asignado; el resto recibe
               NULL y la pantalla muestra un guion. */
            Horario = (SELECT TOP 1 h.Nombre
                         FROM dbo.R_UsuarioHorarioLaboral uh
                         JOIN dbo.R_HorarioLaboral h
                              ON h.IdHorarioLaboral = uh.IdHorarioLaboral
                        WHERE uh.Id_Responsable = u.Cod_Usuario
                          AND uh.Activo = 1
                        ORDER BY uh.FechaDesde DESC, uh.IdUsuarioHorario DESC),

            TieneFicha      = CASE WHEN e.IdEmpleado IS NULL THEN 0 ELSE 1 END,
            EsJefe          = CASE WHEN EXISTS (SELECT 1 FROM dbo.R_Usuarios s
                                                 WHERE LTRIM(RTRIM(s.Cod_Jefe_Inm)) = LTRIM(RTRIM(u.Cod_Usuario))
                                                   AND ISNULL(s.EstadoUsuario, 0) = 0)
                                   THEN 1 ELSE 0 END
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados  e ON e.Cod_Usuario = u.Cod_Usuario
      LEFT JOIN dbo.R_Usuarios j ON LTRIM(RTRIM(j.Cod_Usuario)) = LTRIM(RTRIM(u.Cod_Jefe_Inm))
     WHERE  u.Cod_Usuario = @Cod_Usuario;

    /* 2. contacto personal (editable) */
    SELECT  CorreoPersonal   = ISNULL(p.CorreoPersonal, ''),
            TelefonoPersonal = ISNULL(p.TelefonoPersonal, ''),
            Direccion        = ISNULL(p.Direccion, CAST(e.Direccion AS VARCHAR(400))),
            EstadoCivil      = ISNULL(e.EstadoCivil, '')
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Perfil_ContactoPersonal p ON p.Cod_Usuario = u.Cod_Usuario
      LEFT JOIN dbo.Empleados e ON e.Cod_Usuario = u.Cod_Usuario
     WHERE  u.Cod_Usuario = @Cod_Usuario;

    /* 3. contactos de emergencia */
    SELECT IdContacto, Nombre, Parentesco, Telefono
      FROM dbo.Perfil_ContactoEmergencia
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
     ORDER BY IdContacto;

    /* 4. estudios (fase 2) */
    SELECT IdEstudio, Nivel, Institucion, Titulo, AnioGraduacion
      FROM dbo.Perfil_Estudio
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
     ORDER BY AnioGraduacion DESC, IdEstudio;

    /* 5. certificaciones (fase 2) */
    SELECT IdCertificacion, Nombre, Entidad, FechaObtencion
      FROM dbo.Perfil_Certificacion
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
     ORDER BY FechaObtencion DESC, IdCertificacion;

    /* 6. experiencia (fase 2) */
    SELECT IdExperiencia, Empresa, Cargo, AnioDesde, AnioHasta, Funciones
      FROM dbo.Perfil_Experiencia
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
     ORDER BY ISNULL(AnioHasta, 9999) DESC, AnioDesde DESC;

    /* 7. documentos de respaldo (fase 3) */
    SELECT IdDocumento, Origen, IdOrigen, NombreArchivo, NombreArchivoCodigo, Ruta
      FROM dbo.Perfil_Documento
     WHERE Cod_Usuario = @Cod_Usuario AND Estado = '1'
     ORDER BY IdDocumento;
END
GO
PRINT 'Sp_RTA_PerfilColaborador creado.';
GO
```

- [ ] **Step 4: Escribir las aserciones y el reporte para RRHH**

Agregar al mismo archivo:

```sql
/* ------------------------------------------ 5. aserciones y excepciones --- */

/* Aserciones: si algo de esto falla, el script no dejo la base como se espera. */
IF OBJECT_ID('dbo.Sp_RTA_PerfilColaborador','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilColaborador no quedo creado.', 16, 1);

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME='Empleados' AND COLUMN_NAME='Cod_Usuario')
    RAISERROR('FALLO: Empleados.Cod_Usuario no existe.', 16, 1);

/* Ninguna ficha puede quedar enlazada a dos usuarios distintos. */
IF EXISTS (SELECT 1 FROM dbo.Empleados
            WHERE ISNULL(Cod_Usuario,'') <> ''
            GROUP BY Cod_Usuario HAVING COUNT(*) > 1)
    RAISERROR('FALLO: hay un Cod_Usuario enlazado a mas de una ficha.', 16, 1);

PRINT 'Aserciones OK.';
GO

/* Reporte para RRHH. No cambia nada: solo lista lo que hay que corregir para
   que el modulo rinda completo. Exportar el resultado y enviarlo. */
PRINT '--- Excepciones para RRHH ---';

SELECT Caso = 'Usuario activo sin cedula (no se puede enlazar)',
       u.Cod_Usuario, u.Nom_Usuario, u.E_Mail, u.Departamento
  FROM dbo.R_Usuarios u
 WHERE ISNULL(u.EstadoUsuario,0) = 0
   AND ISNULL(LTRIM(RTRIM(u.Cedula)),'') = ''
UNION ALL
SELECT 'Usuario activo con cedula pero sin ficha de empleado',
       u.Cod_Usuario, u.Nom_Usuario, u.E_Mail, u.Departamento
  FROM dbo.R_Usuarios u
 WHERE ISNULL(u.EstadoUsuario,0) = 0
   AND ISNULL(LTRIM(RTRIM(u.Cedula)),'') <> ''
   AND NOT EXISTS (SELECT 1 FROM dbo.Empleados e
                    WHERE LTRIM(RTRIM(e.Cedula)) = LTRIM(RTRIM(u.Cedula)))
UNION ALL
SELECT 'Cedula repetida entre usuarios activos (no se enlazo ninguno)',
       u.Cod_Usuario, u.Nom_Usuario, u.E_Mail, u.Departamento
  FROM dbo.R_Usuarios u
 WHERE ISNULL(u.EstadoUsuario,0) = 0
   AND LTRIM(RTRIM(u.Cedula)) IN (
        SELECT LTRIM(RTRIM(Cedula)) FROM dbo.R_Usuarios
         WHERE ISNULL(EstadoUsuario,0) = 0 AND ISNULL(LTRIM(RTRIM(Cedula)),'') <> ''
         GROUP BY LTRIM(RTRIM(Cedula)) HAVING COUNT(*) > 1)
UNION ALL
SELECT 'Jefe inmediato que no corresponde a ningun usuario',
       u.Cod_Usuario, u.Nom_Usuario, u.E_Mail, u.Departamento
  FROM dbo.R_Usuarios u
 WHERE ISNULL(u.EstadoUsuario,0) = 0
   AND ISNULL(LTRIM(RTRIM(u.Cod_Jefe_Inm)),'') <> ''
   AND NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios j
                    WHERE LTRIM(RTRIM(j.Cod_Usuario)) = LTRIM(RTRIM(u.Cod_Jefe_Inm)))
 ORDER BY 1, 3;
GO

/* Fechas de nacimiento fuera de rango razonable: erratas de carga. */
SELECT Caso = 'Fecha de nacimiento fuera de rango', e.IdEmpleado, e.Cedula, e.Nombre,
       e.Fecha_nacimiento
  FROM dbo.Empleados e
 WHERE TRY_CONVERT(date, e.Fecha_nacimiento, 103) IS NOT NULL
   AND (TRY_CONVERT(date, e.Fecha_nacimiento, 103) > DATEADD(year, -15, GETDATE())
     OR TRY_CONVERT(date, e.Fecha_nacimiento, 103) < '1940-01-01');
GO

PRINT 'Script 2026-09-14-perfil-colaborador completado.';
GO
```

- [ ] **Step 5: Correr el script contra TEST y verificar**

> **Se corre contra `ReporTarea`, que es producción.** No existe una base de pruebas en ese servidor (solo están `ReporTarea` y `ReporTareaGD`), y el usuario autorizó explícitamente ejecutarlo ahí.
>
> El script es seguro para eso: solo crea tablas y procedimientos nuevos, agrega dos columnas `NULL` y no borra ni modifica ningún dato existente. La única escritura de datos es el poblado de `Empleados.Cod_Usuario`, acotado por `WHERE ISNULL(Cod_Usuario,'') = ''`, así que volver a correrlo no pisa un enlace corregido a mano.
>
> **Nada de SQL suelto:** ejecutar únicamente este archivo. No lanzar `DROP`, `DELETE`, `TRUNCATE` ni `ALTER` fuera de lo que el propio script contiene.

Run:
```
sqlcmd -S 192.168.11.14 -d ReporTarea -U <usuario> -i docs/sql/2026-09-14-perfil-colaborador.sql
```
Expected: los `PRINT` de cada tabla creada, `Aserciones OK.`, el reporte de excepciones, y `Script ... completado.` Sin ningún `FALLO:`.

- [ ] **Step 6: Correrlo dos veces para probar idempotencia**

Run el mismo comando otra vez.
Expected: ahora dice `... ya existia.` en cada tabla, `Enlace poblado: 0 fichas enlazadas en esta corrida.`, y otra vez `Aserciones OK.` sin errores.

- [ ] **Step 7: Commit**

```bash
git add docs/sql/2026-09-14-perfil-colaborador.sql
git commit -m "feat(perfil): esquema del modulo de perfil y enlace de fichas por cedula"
```

---

### Task 5: Entidades de lectura y capa de datos

**Files:**
- Create: `CapaEntidad/EntPerfilCabecera.cs`
- Create: `CapaEntidad/EntPerfilCompleto.cs`
- Create: `CapaDato/DaoPerfil.cs`
- Create: `CapaNegocio/NegPerfil.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj`, `CapaDato/CapaDato.csproj`, `CapaNegocio/CapaNegocio.csproj`

**Interfaces:**
- Consumes: `EntPerfilContacto` (Task 2), `EntPerfilEmergencia` (Task 3), `Sp_RTA_PerfilColaborador` (Task 4).
- Produces:
  - `CapaEntidad.EntPerfilCabecera` con `string CodUsuario, NombreCompleto, Cedula, FechaNacTexto, Cargo, Area, Ciudad, CorreoNotificacion, JefeInmediato, Horario`; `bool TieneFicha, EsJefe`; `int? Edad`.
  - `CapaEntidad.EntPerfilCompleto` con `EntPerfilCabecera Cabecera`, `EntPerfilContacto Contacto`, `List<EntPerfilEmergencia> Emergencia`.
  - `CapaNegocio.NegPerfil.CargarPerfil(string codUsuario)` → `EntPerfilCompleto`.

- [ ] **Step 1: Crear las entidades**

Crear `CapaEntidad/EntPerfilCabecera.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Los datos que RRHH administra. El colaborador los ve y no los edita.
    /// </summary>
    public class EntPerfilCabecera
    {
        public string CodUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Cedula { get; set; }

        /// <summary>
        /// Tal como esta en la base: texto dd/MM/yyyy. No se convierte en SQL a
        /// proposito; la convierte NegPerfilCampos.EdadDesdeTexto.
        /// </summary>
        public string FechaNacTexto { get; set; }

        /// <summary>Anios cumplidos, o null si la fecha falta o no es valida.</summary>
        public int? Edad { get; set; }

        public string Cargo { get; set; }
        public string Area { get; set; }
        public string Ciudad { get; set; }
        public string CorreoNotificacion { get; set; }
        public string JefeInmediato { get; set; }

        /// <summary>
        /// Nombre del horario vigente, o vacio para los 143 usuarios que no
        /// tienen ninguno asignado.
        /// </summary>
        public string Horario { get; set; }

        /// <summary>
        /// false para los 113 usuarios sin ficha de empleado enlazada. La
        /// pantalla lo usa para explicar por que faltan datos en vez de mostrar
        /// campos vacios sin motivo aparente.
        /// </summary>
        public bool TieneFicha { get; set; }

        /// <summary>Tiene al menos un subordinado directo (habilita la fase 3).</summary>
        public bool EsJefe { get; set; }
    }
}
```

Crear `CapaEntidad/EntPerfilCompleto.cs`:

```csharp
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Lo que devuelve una sola llamada a Sp_RTA_PerfilColaborador. En la fase 1
    /// se llenan cabecera, contacto y emergencia; las listas de las fases 2 y 3
    /// se agregan aca cuando existan.
    /// </summary>
    public class EntPerfilCompleto
    {
        public EntPerfilCabecera Cabecera { get; set; }
        public EntPerfilContacto Contacto { get; set; }
        public List<EntPerfilEmergencia> Emergencia { get; set; }

        public EntPerfilCompleto()
        {
            Cabecera = new EntPerfilCabecera();
            Contacto = new EntPerfilContacto();
            Emergencia = new List<EntPerfilEmergencia>();
        }
    }
}
```

Agregar ambos a `CapaEntidad/CapaEntidad.csproj`:

```xml
<Compile Include="EntPerfilCabecera.cs" />
<Compile Include="EntPerfilCompleto.cs" />
```

- [ ] **Step 2: Crear el Dao**

Crear `CapaDato/DaoPerfil.cs`:

```csharp
using CapaEntidad;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Acceso a datos del modulo de perfil.
    ///
    /// Las consultas van con parametros tipados (SqlDbType explicito), no con
    /// AddWithValue: en esta base Cod_Usuario es varchar(50) y AddWithValue lo
    /// manda como nvarchar, lo que descarta el indice en tablas grandes.
    ///
    /// Esta clase NO calcula la edad, aunque tenga la fecha a mano: la
    /// dependencia de la solucion va CapaNegocio -> CapaDato, nunca al reves.
    /// Llamar a NegPerfilCampos desde aca seria una referencia circular y no
    /// compila. La edad la pone NegPerfil despues de leer.
    /// </summary>
    public class DaoPerfil
    {
        /// <summary>
        /// Todo el perfil en una sola ida. Recorre los result sets con
        /// NextResult() en el mismo orden en que los declara el procedimiento.
        /// </summary>
        public static EntPerfilCompleto CargarPerfil(string codUsuario)
        {
            EntPerfilCompleto perfil = new EntPerfilCompleto();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilColaborador", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    /* 1. cabecera */
                    if (dr.Read())
                    {
                        perfil.Cabecera.CodUsuario         = Texto(dr, "Cod_Usuario");
                        perfil.Cabecera.NombreCompleto     = Texto(dr, "NombreCompleto");
                        perfil.Cabecera.Cedula             = Texto(dr, "Cedula");
                        perfil.Cabecera.FechaNacTexto      = Texto(dr, "FechaNacTexto");
                        perfil.Cabecera.Cargo              = Texto(dr, "Cargo");
                        perfil.Cabecera.Area               = Texto(dr, "Area");
                        perfil.Cabecera.Ciudad             = Texto(dr, "Ciudad");
                        perfil.Cabecera.CorreoNotificacion = Texto(dr, "CorreoNotificacion");
                        perfil.Cabecera.JefeInmediato      = Texto(dr, "JefeInmediato");
                        perfil.Cabecera.Horario            = Texto(dr, "Horario");
                        perfil.Cabecera.TieneFicha         = Texto(dr, "TieneFicha") == "1";
                        perfil.Cabecera.EsJefe             = Texto(dr, "EsJefe") == "1";
                    }

                    /* 2. contacto personal */
                    if (dr.NextResult() && dr.Read())
                    {
                        perfil.Contacto.CorreoPersonal   = Texto(dr, "CorreoPersonal");
                        perfil.Contacto.TelefonoPersonal = Texto(dr, "TelefonoPersonal");
                        perfil.Contacto.Direccion        = Texto(dr, "Direccion");
                        perfil.Contacto.EstadoCivil      = Texto(dr, "EstadoCivil");
                    }

                    /* 3. contactos de emergencia */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Emergencia.Add(new EntPerfilEmergencia
                            {
                                IdContacto = int.Parse(Texto(dr, "IdContacto")),
                                Nombre     = Texto(dr, "Nombre"),
                                Parentesco = Texto(dr, "Parentesco"),
                                Telefono   = Texto(dr, "Telefono")
                            });
                        }
                    }

                    /* Los result sets 4 a 7 existen y se ignoran hasta la fase 2. */
                }
            }

            return perfil;
        }

        private static string Texto(SqlDataReader dr, string columna)
        {
            return dr[columna] == System.DBNull.Value ? "" : dr[columna].ToString().Trim();
        }
    }
}
```

- [ ] **Step 3: Crear la fachada de negocio**

Crear `CapaNegocio/NegPerfil.cs`:

```csharp
using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    /// <summary>Fachada del modulo de perfil, igual que el resto de la capa.</summary>
    public class NegPerfil
    {
        /// <summary>
        /// Lee el perfil y le pone la edad.
        ///
        /// La edad se calcula aca y no en el Dao porque NegPerfilCampos vive en
        /// esta capa y CapaDato no puede referenciarla sin cerrar un ciclo. No es
        /// un rodeo: leer y calcular son dos cosas distintas y esta es la capa que
        /// calcula.
        ///
        /// Tampoco se calcula en SQL, que seria el otro lugar tentador: alli el
        /// formato dd/MM/yyyy depende de acertarle al estilo 103 y no hay forma de
        /// probarlo. Aca esta cubierto por NegPerfilCamposTests.
        /// </summary>
        public static EntPerfilCompleto CargarPerfil(string codUsuario)
        {
            EntPerfilCompleto perfil = DaoPerfil.CargarPerfil(codUsuario);

            perfil.Cabecera.Edad =
                NegPerfilCampos.EdadDesdeTexto(perfil.Cabecera.FechaNacTexto);

            return perfil;
        }
    }
}
```

Agregar los `Compile Include` correspondientes a `CapaDato/CapaDato.csproj` y `CapaNegocio/CapaNegocio.csproj`.

- [ ] **Step 4: Compilar la solución**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 5: Correr las pruebas para confirmar que no se rompió nada**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```
Expected: `Total tests: 17. Passed: 17.`

- [ ] **Step 6: Commit**

```bash
git add CapaEntidad CapaDato CapaNegocio
git commit -m "feat(perfil): lectura del perfil completo en una sola consulta"
```

---

### Task 6: Handler con la acción CargarPerfil

**Files:**
- Create: `ReporteTareas/Formulario/AdministrarPerfil.ashx`
- Create: `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: `NegPerfil.CargarPerfil` (Task 5).
- Produces: endpoint `POST Formulario/AdministrarPerfil.ashx` con cuerpo `[{"action":"CargarPerfil","parameters":{}}]`, que responde el JSON de `EntPerfilCompleto` o un `EntRespuesta` con `estado:"0"`.

- [ ] **Step 1: Crear el `.ashx`**

Crear `ReporteTareas/Formulario/AdministrarPerfil.ashx`:

```
<%@ WebHandler Language="C#" CodeBehind="AdministrarPerfil.ashx.cs" Class="JsonJQueryNetPerfil.AdministrarPerfil" %>
```

- [ ] **Step 2: Crear el code-behind**

Crear `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetPerfil
{
    /// <summary>
    /// Handler de la pantalla "Mi perfil".
    ///
    /// IRequiresSessionState no es decorativo: sin el, context.Session es null
    /// en un IHttpHandler y no habria de donde sacar la identidad.
    ///
    /// La estructura viene de AdministrarHorarioUsuario.ashx, pero NO su manejo
    /// de identidad. Aquel recibe codUsuario del cliente; aca eso seria que
    /// cualquiera lea y sobrescriba el perfil de cualquiera. Se sigue el patron
    /// de AdministrarUsuarios.ashx: la identidad sale de la sesion, siempre.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarPerfil : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                responseAction.Append(responseMessage("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger"));
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();

                JavaScriptSerializer i = new JavaScriptSerializer();
                dynamic parametros = i.Deserialize(inputJson.ToString(), typeof(object));

                var Action = parametros[0]["action"];
                bool existAction = false;

                if (Action == "CargarPerfil")
                {
                    existAction = true;
                    responseAction.Append(CargarPerfil(context));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }

            context.Response.ContentType = "application/json";
            // La app corre en windows-1252; se fuerza UTF-8 en los bytes para que
            // coincidan con el charset declarado y las tildes lleguen intactas.
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        /// <summary>
        /// El perfil de quien esta conectado. No recibe parametros a proposito:
        /// no hay nada que el cliente pueda decir sobre de quien es este perfil.
        /// </summary>
        private string CargarPerfil(HttpContext context)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);

                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                return ToJson(NegPerfil.CargarPerfil(codUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar el perfil. " + ex.Message, "danger");
            }
        }

        /// <summary>De quien es este perfil. Sale de la sesion, nunca del cliente.</summary>
        private string CodUsuarioSesion(HttpContext context)
        {
            if (context.Session != null && context.Session["Cod_Usuario"] != null)
            {
                return context.Session["Cod_Usuario"].ToString().Trim();
            }
            return "";
        }

        private string responseMessage(string estado, string mensaje, string tipoMensaje, string resultado = "")
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta.estado = estado;
            respuesta.mensaje = mensaje;
            respuesta.tipoMensaje = tipoMensaje;
            respuesta.resultado = resultado;

            return ToJson(respuesta);
        }

        /// <summary>
        /// Serializa escapando lo no ASCII como \uXXXX. El helper compartido usa
        /// Encoding.Default y rompe las tildes.
        /// </summary>
        private static string ToJson(object obj)
        {
            if (obj == null) return string.Empty;
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

Agregar a `ReporteTareas/ReporteTareas.csproj`, junto a los otros handlers:

```xml
<Content Include="Formulario\AdministrarPerfil.ashx" />
<Compile Include="Formulario\AdministrarPerfil.ashx.cs">
  <DependentUpon>AdministrarPerfil.ashx</DependentUpon>
</Compile>
```

- [ ] **Step 3: Compilar**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 4: Verificar que sin sesión rechaza**

Levantar el sitio en IIS Express y, **sin iniciar sesión**, ejecutar:

```bash
curl -s -X POST http://localhost:<puerto>/Formulario/AdministrarPerfil.ashx \
  -H "Content-Type: application/json" \
  -d '[{"action":"CargarPerfil","parameters":{}}]'
```
Expected: `{"estado":"0","mensaje":"Su sesión expiró. Vuelva a iniciar sesión.", ...}`

Esa respuesta es la prueba de que la guarda de sesión funciona.

- [ ] **Step 5: Commit**

```bash
git add ReporteTareas/Formulario/AdministrarPerfil.ashx ReporteTareas/Formulario/AdministrarPerfil.ashx.cs ReporteTareas/ReporteTareas.csproj
git commit -m "feat(perfil): handler del perfil, con la identidad tomada de la sesion"
```

---

### Task 7: Pantalla con la pestaña Datos personales

**Files:**
- Create: `ReporteTareas/Formulario/MiPerfil.aspx`
- Create: `ReporteTareas/Formulario/MiPerfil.aspx.cs`
- Create: `ReporteTareas/Formulario/MiPerfil.aspx.designer.cs`
- Create: `ReporteTareas/js/miPerfil.js`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: `AdministrarPerfil.ashx` acción `CargarPerfil` (Task 6).
- Produces: función JS global `PostPerfil(action, parameters, onSuccess)` y `_perfil` (el último perfil cargado), que usan las tareas 8 y 9.

- [ ] **Step 1: Crear la página**

Crear `ReporteTareas/Formulario/MiPerfil.aspx`:

```aspx
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="MiPerfil.aspx.cs" Inherits="ReporteTareas.Formulario.MiPerfil" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/miPerfil.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Mi perfil</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="row" style="padding: 0 15px">

            <!-- ----------------------------------------------- barra lateral -->
            <div class="col-lg-3">
                <div class="panel panel-default">
                    <div class="panel-body text-center">
                        <div id="perfilAvatar"
                             style="width: 96px; height: 96px; margin: 0 auto 12px; border-radius: 50%;
                                    background: #750202; color: #fff; font-size: 34px; line-height: 96px;">–</div>
                        <h4 id="perfilNombre" style="margin: 0 0 4px">–</h4>
                        <p id="perfilCargo" class="text-muted" style="margin: 0 0 10px">–</p>
                        <span id="perfilArea" class="label label-primary">–</span>
                        <span id="perfilCiudad" class="label label-default">–</span>
                        <hr />
                        <p style="margin: 0"><b id="perfilEdad">–</b><br /><small class="text-muted">Edad</small></p>
                    </div>
                </div>

                <!-- Aviso para los 119 usuarios sin ficha de empleado enlazada. -->
                <div id="perfilSinFicha" class="alert alert-warning" style="display: none; font-size: 12px">
                    <i class="fa fa-info-circle"></i>
                    Algunos datos administrados por Talento Humano todavía no están
                    asociados a tu usuario. Puedes usar el resto del perfil con normalidad.
                </div>

                <!-- Caso distinto y mas grave: el codigo de usuario esta repetido en
                     R_Usuarios y no se puede saber cual de las dos personas eres. El
                     procedimiento devuelve cero filas a proposito, antes que arriesgarse
                     a mostrarte los datos de otra persona. Afecta a 4 usuarios activos. -->
                <div id="perfilNoIdentificado" class="alert alert-danger" style="display: none; font-size: 12px">
                    <i class="fa fa-exclamation-triangle"></i>
                    No pudimos identificar tu perfil de forma única: tu código de usuario
                    está repetido en el sistema. Escribe a Talento Humano para que lo corrijan.
                </div>
            </div>

            <!-- --------------------------------------------------- contenido -->
            <div class="col-lg-9">
                <ul class="nav nav-tabs" role="tablist">
                    <li class="active"><a href="#tabPersonal" data-toggle="tab"><i class="fa fa-lock"></i> Datos personales</a></li>
                    <li><a href="#tabContacto" data-toggle="tab"><i class="fa fa-envelope"></i> Contacto y domicilio</a></li>
                    <li><a href="#tabEmergencia" data-toggle="tab"><i class="fa fa-ambulance"></i> Emergencia</a></li>
                </ul>

                <div class="tab-content" style="padding-top: 15px">

                    <div class="tab-pane active" id="tabPersonal">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Información personal
                                <span class="label label-default pull-right">
                                    <i class="fa fa-lock"></i> Gestionado por Talento Humano
                                </span>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="form-group col-lg-6"><label>Nombres completos</label>
                                        <p class="form-control-static" id="dpNombre">–</p></div>
                                    <div class="form-group col-lg-6"><label>Número de cédula</label>
                                        <p class="form-control-static" id="dpCedula">–</p></div>
                                    <div class="form-group col-lg-6"><label>Fecha de nacimiento</label>
                                        <p class="form-control-static" id="dpFnac">–</p></div>
                                    <div class="form-group col-lg-6"><label>Cargo</label>
                                        <p class="form-control-static" id="dpCargo">–</p></div>
                                    <div class="form-group col-lg-6"><label>Área</label>
                                        <p class="form-control-static" id="dpArea">–</p></div>
                                    <div class="form-group col-lg-6"><label>Jefe inmediato</label>
                                        <p class="form-control-static" id="dpJefe">–</p></div>
                                    <div class="form-group col-lg-6"><label>Ciudad</label>
                                        <p class="form-control-static" id="dpCiudad">–</p></div>
                                    <div class="form-group col-lg-6"><label>Correo de notificación</label>
                                        <p class="form-control-static" id="dpCorreo">–</p></div>
                                    <div class="form-group col-lg-6"><label>Horario</label>
                                        <p class="form-control-static" id="dpHorario">–</p></div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tab-pane" id="tabContacto"></div>
                    <div class="tab-pane" id="tabEmergencia"></div>

                </div>
            </div>
        </div>
    </div>
</asp:Content>
```

- [ ] **Step 2: Crear el code-behind**

Crear `ReporteTareas/Formulario/MiPerfil.aspx.cs`:

```csharp
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Mi perfil. Es la unica pantalla del sistema a la que entra cualquiera que
    /// haya iniciado sesion: no se registra en MenuDos ni en PerfilMenu porque
    /// se llega por el desplegable de usuario, y el perfil propio le corresponde
    /// a todos.
    ///
    /// No pasa el Cod_Usuario a la pagina: el handler lo toma de la sesion.
    /// </summary>
    public partial class MiPerfil : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);
        }
    }
}
```

Crear `ReporteTareas/Formulario/MiPerfil.aspx.designer.cs`:

```csharp
namespace ReporteTareas.Formulario
{
    public partial class MiPerfil
    {
    }
}
```

- [ ] **Step 3: Crear el JavaScript**

Crear `ReporteTareas/js/miPerfil.js`:

```javascript
/* ============================================================================
   Pantalla: Mi perfil
   Handler : AdministrarPerfil.ashx

   El Cod_Usuario no se manda nunca: el handler lo saca de la sesion. Si algun
   dia hace falta ver el perfil de otra persona, es una accion distinta con su
   propia validacion, no un parametro de estas.
   ============================================================================ */

var _perfil = null;

$(document).ready(function () {
    CargarPerfil();
});

/* Llama al handler con el formato [{action, parameters}] */
function PostPerfil(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarPerfil.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) {
            onSuccess(respuesta);
        },
        error: function () {
            MostrarMensaje("La operación está tomando demasiado tiempo o la red está saturada. Intente nuevamente.", "danger");
        }
    });
}

function CargarPerfil() {
    PostPerfil("CargarPerfil", {}, function (respuesta) {
        // Un objeto con "estado" es un EntRespuesta, es decir, un error.
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        _perfil = respuesta;

        /* Cero filas de cabecera no es "no tiene datos": es que su codigo de usuario
           esta repetido en R_Usuarios y el procedimiento se nego a adivinar cual de
           las dos personas es. Mostrar la pantalla vacia seria peor que no mostrarla:
           pareceria que el perfil no tiene nada, cuando el problema es de identidad. */
        if (!respuesta.PerfilEncontrado) {
            $("#perfilNoIdentificado").show();
            $(".nav-tabs, .tab-content").hide();
            return;
        }

        PintarCabecera(respuesta.Cabecera);
    });
}

function PintarCabecera(c) {
    $("#perfilNombre").text(c.NombreCompleto || "–");
    $("#perfilCargo").text(c.Cargo || "–");
    $("#perfilArea").text(c.Area || "–");
    $("#perfilCiudad").text(c.Ciudad || "–");
    $("#perfilAvatar").text(Iniciales(c.NombreCompleto));

    // Edad null significa "no se sabe": guion, nunca cero.
    $("#perfilEdad").text(c.Edad === null ? "–" : c.Edad + " años");

    $("#dpNombre").text(c.NombreCompleto || "–");
    $("#dpCedula").text(c.Cedula || "–");
    $("#dpFnac").text(c.FechaNacTexto || "–");
    $("#dpCargo").text(c.Cargo || "–");
    $("#dpArea").text(c.Area || "–");
    $("#dpJefe").text(c.JefeInmediato || "–");
    $("#dpCiudad").text(c.Ciudad || "–");
    $("#dpCorreo").text(c.CorreoNotificacion || "–");
    // 143 de 231 no tienen horario asignado todavia: guion, no cadena vacia.
    $("#dpHorario").text(c.Horario || "–");

    if (!c.TieneFicha) {
        $("#perfilSinFicha").show();
    }
}

function Iniciales(nombre) {
    if (!nombre) { return "–"; }
    var partes = nombre.trim().split(/\s+/);
    if (partes.length === 1) { return partes[0].substring(0, 2).toUpperCase(); }
    return (partes[0].charAt(0) + partes[1].charAt(0)).toUpperCase();
}
```

Agregar a `ReporteTareas/ReporteTareas.csproj`:

```xml
<Content Include="Formulario\MiPerfil.aspx" />
<Compile Include="Formulario\MiPerfil.aspx.cs">
  <DependentUpon>MiPerfil.aspx</DependentUpon>
  <SubType>ASPXCodeBehind</SubType>
</Compile>
<Compile Include="Formulario\MiPerfil.aspx.designer.cs">
  <DependentUpon>MiPerfil.aspx</DependentUpon>
</Compile>
<Content Include="js\miPerfil.js" />
```

- [ ] **Step 4: Compilar y verificar en el navegador**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```
Expected: `Build succeeded. 0 Error(s)`.

Luego iniciar sesión y navegar a `Formulario/MiPerfil.aspx`.
Expected: la barra lateral muestra nombre, cargo, área e iniciales; la pestaña *Datos personales* muestra los ocho campos; la edad sale en años o un guion.

- [ ] **Step 5: Verificar el caso de los 113 sin ficha**

Iniciar sesión con un usuario que aparezca en el reporte de excepciones de la Task 4 bajo `Usuario activo sin cedula`.
Expected: la pantalla carga igual, muestra el nombre y el área que sí tiene, y aparece el aviso amarillo. **No** una pantalla en blanco ni un error.

- [ ] **Step 6: Commit**

```bash
git add ReporteTareas/Formulario/MiPerfil.aspx ReporteTareas/Formulario/MiPerfil.aspx.cs ReporteTareas/Formulario/MiPerfil.aspx.designer.cs ReporteTareas/js/miPerfil.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(perfil): pantalla del perfil con los datos que administra Talento Humano"
```

---

### Task 8: Pestaña Contacto y domicilio

**Files:**
- Create: SQL en `docs/sql/2026-09-14-perfil-colaborador.sql` (sección 6, agregar)
- Modify: `CapaDato/DaoPerfil.cs`, `CapaNegocio/NegPerfil.cs`
- Modify: `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`
- Modify: `ReporteTareas/Formulario/MiPerfil.aspx`, `ReporteTareas/js/miPerfil.js`

**Interfaces:**
- Consumes: `NegPerfilCampos.LeerContacto` (Task 2), `PostPerfil` (Task 7).
- Produces: `NegPerfil.GuardarContacto(string codUsuario, EntPerfilContacto contacto, string ip)` → `EntRespuesta`; acción `GuardarContacto` del handler.

- [ ] **Step 1: Agregar el SP al script**

Agregar a `docs/sql/2026-09-14-perfil-colaborador.sql`, **antes** de la sección 8 de aserciones y reporte:

```sql
/* ------------------------------------------------ 6. guardar el contacto -- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarContacto','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarContacto;
GO

/* Escribe en dos sitios porque el dato vive en dos sitios: lo nuevo va a
   Perfil_ContactoPersonal y el estado civil se queda en Empleados, que es
   donde RRHH ya lo mantiene y lo lee su propia pantalla.

   El estado civil solo se actualiza si la persona tiene ficha enlazada. Para
   los 113 sin ficha no hay donde escribirlo; se ignora en silencio en vez de
   fallar, porque el resto del guardado si tiene sentido para ellos. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarContacto
    @Cod_Usuario      VARCHAR(50),
    @CorreoPersonal   VARCHAR(150),
    @TelefonoPersonal VARCHAR(50),
    @Direccion        VARCHAR(400),
    @EstadoCivil      VARCHAR(100),
    @Ip               VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.Perfil_ContactoPersonal AS destino
    USING (SELECT @Cod_Usuario AS Cod_Usuario) AS origen
       ON destino.Cod_Usuario = origen.Cod_Usuario
    WHEN MATCHED THEN
        UPDATE SET CorreoPersonal   = @CorreoPersonal,
                   TelefonoPersonal = @TelefonoPersonal,
                   Direccion        = @Direccion,
                   Fec_Modificacion = SYSDATETIME(),
                   Usu_Modificacion = @Cod_Usuario,
                   Ip_Modificacion  = @Ip
    WHEN NOT MATCHED THEN
        INSERT (Cod_Usuario, CorreoPersonal, TelefonoPersonal, Direccion,
                Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @CorreoPersonal, @TelefonoPersonal, @Direccion,
                @Cod_Usuario, @Ip);

    IF LTRIM(RTRIM(ISNULL(@EstadoCivil,''))) <> ''
    BEGIN
        /* LEFT(@Ip, 32) no es cosmetica: Empleados.Ip_Modificacion es varchar(32),
           mas angosta que las columnas equivalentes de las tablas nuevas, que son
           varchar(64). Con una IPv6 este UPDATE no truncaria en silencio -fallaria
           con "String or binary data would be truncated" y se caeria el guardado
           entero, incluido lo que si cabia-. Se recorta aqui a proposito. */
        UPDATE dbo.Empleados
           SET EstadoCivil      = @EstadoCivil,
               Fec_Modificacion = GETDATE(),
               Ip_Modificacion  = LEFT(@Ip, 32)
         WHERE Cod_Usuario = @Cod_Usuario;
    END

    SELECT Respuestas = 0;
END
GO
PRINT 'Sp_RTA_PerfilGuardarContacto creado.';
GO
```

- [ ] **Step 2: Agregar el método al Dao**

Agregar a `CapaDato/DaoPerfil.cs`:

```csharp
        /// <summary>Guarda el contacto editable. Devuelve el resultado listo para el cliente.</summary>
        public static EntRespuesta GuardarContacto(string codUsuario, EntPerfilContacto contacto, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilGuardarContacto", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario",      SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@CorreoPersonal",   SqlDbType.VarChar, 150).Value = contacto.CorreoPersonal;
                cmd.Parameters.Add("@TelefonoPersonal", SqlDbType.VarChar,  50).Value = contacto.TelefonoPersonal;
                cmd.Parameters.Add("@Direccion",        SqlDbType.VarChar, 400).Value = contacto.Direccion;
                cmd.Parameters.Add("@EstadoCivil",      SqlDbType.VarChar, 100).Value = contacto.EstadoCivil;
                cmd.Parameters.Add("@Ip",               SqlDbType.VarChar,  64).Value = ip ?? "";
                cnx.Open();
                cmd.ExecuteNonQuery();
            }

            respuesta.estado = "1";
            respuesta.mensaje = "Tus datos de contacto se guardaron correctamente.";
            respuesta.tipoMensaje = "success";
            return respuesta;
        }
```

Agregar a `CapaNegocio/NegPerfil.cs`:

```csharp
        public static EntRespuesta GuardarContacto(string codUsuario, EntPerfilContacto contacto, string ip)
        {
            return DaoPerfil.GuardarContacto(codUsuario, contacto, ip);
        }
```

- [ ] **Step 3: Agregar la acción al handler**

En `ProcessRequest` de `AdministrarPerfil.ashx.cs`, después del bloque de `CargarPerfil`, agregar:

```csharp
                if (Action == "GuardarContacto")
                {
                    existAction = true;
                    responseAction.Append(GuardarContacto(context, parametros[0]["parameters"]));
                }
```

Y el método, junto a los otros:

```csharp
        /// <summary>
        /// Guarda el contacto del usuario de la sesion.
        ///
        /// El payload pasa por NegPerfilCampos.LeerContacto, que solo lee las
        /// cuatro claves permitidas. Un payload que traiga cargo o cedula no
        /// falla: esos valores simplemente no tienen donde aterrizar.
        /// </summary>
        private string GuardarContacto(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                var diccionario = campos as System.Collections.Generic.IDictionary<string, object>;
                EntPerfilContacto contacto = NegPerfilCampos.LeerContacto(diccionario);

                return ToJson(NegPerfil.GuardarContacto(codUsuario, contacto,
                                                        context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar el contacto. " + ex.Message, "danger");
            }
        }
```

- [ ] **Step 4: Agregar la pestaña**

Reemplazar `<div class="tab-pane" id="tabContacto"></div>` en `MiPerfil.aspx` por:

```aspx
                    <div class="tab-pane" id="tabContacto">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Contacto y domicilio
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="form-group col-lg-12"><label>Dirección de domicilio</label>
                                        <input type="text" class="form-control" id="inDireccion" maxlength="400" /></div>
                                    <div class="form-group col-lg-6"><label>Correo personal</label>
                                        <input type="email" class="form-control" id="inCorreoPersonal" maxlength="150" /></div>
                                    <div class="form-group col-lg-6"><label>Número de teléfono</label>
                                        <input type="tel" class="form-control" id="inTelefonoPersonal" maxlength="50" /></div>
                                    <div class="form-group col-lg-6"><label>Estado civil</label>
                                        <select class="form-control" id="inEstadoCivil">
                                            <option value="">Seleccione…</option>
                                            <option>Soltero/a</option><option>Casado/a</option>
                                            <option>Unión de hecho</option><option>Divorciado/a</option>
                                            <option>Viudo/a</option>
                                        </select></div>
                                </div>
                                <button type="button" class="btn btn-primary" onclick="GuardarContacto()">
                                    <i class="fa fa-save"></i> Guardar cambios
                                </button>
                            </div>
                        </div>
                    </div>
```

- [ ] **Step 5: Agregar el JavaScript**

Agregar a `ReporteTareas/js/miPerfil.js`, y llamar `PintarContacto(respuesta.Contacto);` dentro de `CargarPerfil`, justo después de `PintarCabecera(...)`:

```javascript
function PintarContacto(c) {
    $("#inDireccion").val(c.Direccion || "");
    $("#inCorreoPersonal").val(c.CorreoPersonal || "");
    $("#inTelefonoPersonal").val(c.TelefonoPersonal || "");
    $("#inEstadoCivil").val(c.EstadoCivil || "");
}

function GuardarContacto() {
    var datos = {
        correoPersonal:   $("#inCorreoPersonal").val(),
        telefonoPersonal: $("#inTelefonoPersonal").val(),
        direccion:        $("#inDireccion").val(),
        estadoCivil:      $("#inEstadoCivil").val()
    };

    PostPerfil("GuardarContacto", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
    });
}
```

- [ ] **Step 6: Correr el SP nuevo y verificar en el navegador**

Run:
```
sqlcmd -S 192.168.11.14 -d ReporTarea -U <usuario> -i docs/sql/2026-09-14-perfil-colaborador.sql
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```
Expected: `Sp_RTA_PerfilGuardarContacto creado.` y `Build succeeded`.

En el navegador: llenar los cuatro campos, guardar, recargar la página.
Expected: los valores vuelven tal como se guardaron.

- [ ] **Step 7: Verificar que la lista blanca aguanta por HTTP**

Con la sesión iniciada, en la consola del navegador:

```javascript
PostPerfil("GuardarContacto", {
    correoPersonal: "prueba@gmail.com",
    cargo: "Gerente General",
    cedula: "9999999999"
}, function (r) { console.log(r.mensaje); });
```

Luego recargar la página.
Expected: el correo personal cambió; **el cargo y la cédula siguen igual**. Esta es la comprobación de extremo a extremo de la Task 2.

- [ ] **Step 8: Commit**

```bash
git add docs/sql CapaDato CapaNegocio ReporteTareas
git commit -m "feat(perfil): el colaborador actualiza su contacto y domicilio"
```

---

### Task 9: Pestaña Emergencia

**Files:**
- Modify: `docs/sql/2026-09-14-perfil-colaborador.sql` (sección 7)
- Modify: `CapaDato/DaoPerfil.cs`, `CapaNegocio/NegPerfil.cs`
- Modify: `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`
- Modify: `ReporteTareas/Formulario/MiPerfil.aspx`, `ReporteTareas/js/miPerfil.js`

**Interfaces:**
- Consumes: `NegPerfilCampos.ValidarEmergencia` (Task 3), `PostPerfil` (Task 7).
- Produces: `NegPerfil.GuardarEmergencia(string codUsuario, EntPerfilEmergencia c, string ip)` → `EntRespuesta`; `NegPerfil.EliminarEmergencia(string codUsuario, int idContacto, string ip)` → `EntRespuesta`; acciones `GuardarEmergencia` y `EliminarEmergencia`.

- [ ] **Step 1: Agregar los SPs**

Agregar a `docs/sql/2026-09-14-perfil-colaborador.sql`, antes de las aserciones:

```sql
/* ----------------------------------------------------- 7. emergencia ------ */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarEmergencia','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarEmergencia;
GO

/* IdContacto = 0 significa alta; cualquier otro, edicion.

   El WHERE de la edicion lleva Cod_Usuario ademas del IdContacto a proposito:
   sin eso, mandar el IdContacto de otra persona editaria su contacto. El
   IdContacto viene del cliente y no se puede confiar en el; el Cod_Usuario
   viene de la sesion y si. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarEmergencia
    @Cod_Usuario VARCHAR(50),
    @IdContacto  INT,
    @Nombre      VARCHAR(150),
    @Parentesco  VARCHAR(50),
    @Telefono    VARCHAR(50),
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    IF @IdContacto = 0
    BEGIN
        INSERT INTO dbo.Perfil_ContactoEmergencia
            (Cod_Usuario, Nombre, Parentesco, Telefono, Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nombre, @Parentesco, @Telefono, @Cod_Usuario, @Ip);

        SELECT Respuestas = 0, IdContacto = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Perfil_ContactoEmergencia
           SET Nombre           = @Nombre,
               Parentesco       = @Parentesco,
               Telefono         = @Telefono,
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Cod_Usuario,
               Ip_Modificacion  = @Ip
         WHERE IdContacto  = @IdContacto
           AND Cod_Usuario = @Cod_Usuario;

        SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdContacto = @IdContacto;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarEmergencia creado.';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarEmergencia','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarEmergencia;
GO

/* Borrado logico, y con Cod_Usuario en el WHERE por la misma razon de arriba. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarEmergencia
    @Cod_Usuario VARCHAR(50),
    @IdContacto  INT,
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Perfil_ContactoEmergencia
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Cod_Usuario,
           Ip_Modificacion  = @Ip
     WHERE IdContacto  = @IdContacto
       AND Cod_Usuario = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarEmergencia creado.';
GO
```

- [ ] **Step 2: Agregar los métodos al Dao y a Neg**

Agregar a `CapaDato/DaoPerfil.cs`:

```csharp
        /// <summary>Alta o edicion de un contacto de emergencia.</summary>
        public static EntRespuesta GuardarEmergencia(string codUsuario, EntPerfilEmergencia c, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();
            int resultado = -1;

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilGuardarEmergencia", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdContacto",  SqlDbType.Int).Value          = c.IdContacto;
                cmd.Parameters.Add("@Nombre",      SqlDbType.VarChar, 150).Value = c.Nombre;
                cmd.Parameters.Add("@Parentesco",  SqlDbType.VarChar,  50).Value = c.Parentesco;
                cmd.Parameters.Add("@Telefono",    SqlDbType.VarChar,  50).Value = c.Telefono;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar,  64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { resultado = int.Parse(dr["Respuestas"].ToString()); }
                }
            }

            if (resultado == 0)
            {
                respuesta.estado = "1";
                respuesta.mensaje = "Contacto de emergencia guardado.";
                respuesta.tipoMensaje = "success";
            }
            else
            {
                // El unico camino a -1 es un IdContacto que no es de esta persona.
                respuesta.estado = "0";
                respuesta.mensaje = "No se encontró ese contacto de emergencia.";
                respuesta.tipoMensaje = "warning";
            }

            return respuesta;
        }

        /// <summary>Borrado logico de un contacto de emergencia.</summary>
        public static EntRespuesta EliminarEmergencia(string codUsuario, int idContacto, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();
            int resultado = -1;

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilEliminarEmergencia", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdContacto",  SqlDbType.Int).Value         = idContacto;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { resultado = int.Parse(dr["Respuestas"].ToString()); }
                }
            }

            respuesta.estado      = resultado == 0 ? "1" : "0";
            respuesta.mensaje     = resultado == 0 ? "Contacto eliminado." : "No se encontró ese contacto.";
            respuesta.tipoMensaje = resultado == 0 ? "success" : "warning";
            return respuesta;
        }
```

Agregar a `CapaNegocio/NegPerfil.cs`:

```csharp
        public static EntRespuesta GuardarEmergencia(string codUsuario, EntPerfilEmergencia c, string ip)
        {
            return DaoPerfil.GuardarEmergencia(codUsuario, c, ip);
        }

        public static EntRespuesta EliminarEmergencia(string codUsuario, int idContacto, string ip)
        {
            return DaoPerfil.EliminarEmergencia(codUsuario, idContacto, ip);
        }
```

- [ ] **Step 3: Agregar las acciones al handler**

En `ProcessRequest`, junto a las otras:

```csharp
                if (Action == "GuardarEmergencia")
                {
                    existAction = true;
                    responseAction.Append(GuardarEmergencia(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarEmergencia")
                {
                    existAction = true;
                    responseAction.Append(EliminarEmergencia(context, parametros[0]["parameters"]));
                }
```

Y los métodos:

```csharp
        private string GuardarEmergencia(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilEmergencia contacto = new EntPerfilEmergencia
                {
                    IdContacto = Convert.ToInt32(Texto(campos, "idContacto", "0")),
                    Nombre     = Texto(campos, "nombre", ""),
                    Parentesco = Texto(campos, "parentesco", ""),
                    Telefono   = Texto(campos, "telefono", "")
                };

                /* Se valida en el servidor y no solo en el navegador: el handler
                   es alcanzable por HTTP directo. */
                string error = NegPerfilCampos.ValidarEmergencia(contacto);
                if (error != "")
                {
                    return responseMessage("0", error, "warning");
                }

                return ToJson(NegPerfil.GuardarEmergencia(codUsuario, contacto,
                                                          context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar el contacto de emergencia. " + ex.Message, "danger");
            }
        }

        private string EliminarEmergencia(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int idContacto = Convert.ToInt32(Texto(campos, "idContacto", "0"));

                return ToJson(NegPerfil.EliminarEmergencia(codUsuario, idContacto,
                                                           context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar el contacto. " + ex.Message, "danger");
            }
        }

        /// <summary>Lee una clave del payload dinamico, con valor por omision.</summary>
        private static string Texto(dynamic campos, string clave, string omision)
        {
            var d = campos as System.Collections.Generic.IDictionary<string, object>;
            if (d == null) { return omision; }

            object valor;
            if (!d.TryGetValue(clave, out valor) || valor == null) { return omision; }

            string texto = valor.ToString().Trim();
            return texto == "" ? omision : texto;
        }
```

- [ ] **Step 4: Agregar la pestaña**

Reemplazar `<div class="tab-pane" id="tabEmergencia"></div>` en `MiPerfil.aspx` por:

```aspx
                    <div class="tab-pane" id="tabEmergencia">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Contactos de emergencia
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">
                                    A quién debemos llamar si te ocurre algo. Puedes registrar más de uno.
                                </p>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover" style="font-size: 90%">
                                        <thead class="bg-primary">
                                            <tr><th>Nombre</th><th>Parentesco</th><th>Teléfono</th><th style="width:90px">Quitar</th></tr>
                                        </thead>
                                        <tbody id="cuerpoEmergencia"></tbody>
                                    </table>
                                </div>
                                <hr />
                                <div class="row">
                                    <div class="form-group col-lg-4"><label>Nombre completo</label>
                                        <input type="text" class="form-control" id="emNombre" maxlength="150" /></div>
                                    <div class="form-group col-lg-3"><label>Parentesco</label>
                                        <input type="text" class="form-control" id="emParentesco" maxlength="50" /></div>
                                    <div class="form-group col-lg-3"><label>Teléfono</label>
                                        <input type="tel" class="form-control" id="emTelefono" maxlength="50" /></div>
                                    <div class="form-group col-lg-2" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary btn-block" onclick="AgregarEmergencia()">
                                            <i class="fa fa-plus"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
```

- [ ] **Step 5: Agregar el JavaScript**

Agregar a `miPerfil.js`, y llamar `PintarEmergencia(respuesta.Emergencia);` dentro de `CargarPerfil`:

```javascript
function PintarEmergencia(lista) {
    var $cuerpo = $("#cuerpoEmergencia").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="4" class="text-center text-muted">' +
                       'Todavía no has registrado ningún contacto de emergencia.</td></tr>');
        return;
    }

    $.each(lista, function (i, c) {
        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(c.Nombre));
        $fila.append($("<td></td>").text(c.Parentesco));
        $fila.append($("<td></td>").text(c.Telefono));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarEmergencia(' + c.IdContacto + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarEmergencia() {
    var datos = {
        idContacto: 0,
        nombre:     $("#emNombre").val(),
        parentesco: $("#emParentesco").val(),
        telefono:   $("#emTelefono").val()
    };

    PostPerfil("GuardarEmergencia", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#emNombre, #emParentesco, #emTelefono").val("");
            CargarPerfil();
        }
    });
}

function EliminarEmergencia(idContacto) {
    PostPerfil("EliminarEmergencia", { idContacto: idContacto }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}
```

- [ ] **Step 6: Correr el script y verificar**

Run:
```
sqlcmd -S 192.168.11.14 -d ReporTarea -U <usuario> -i docs/sql/2026-09-14-perfil-colaborador.sql
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```
Expected: los dos `PRINT` de los SPs nuevos y `Build succeeded`.

En el navegador: agregar un contacto completo, agregar uno sin teléfono, eliminar uno.
Expected: el completo aparece en la tabla; el que no tiene teléfono se rechaza con *"Escriba el teléfono del contacto de emergencia."*; el eliminado desaparece.

- [ ] **Step 7: Verificar que no se puede editar el contacto de otro**

Con la sesión de la persona A, anotar el `IdContacto` de uno de sus contactos. Iniciar sesión como la persona B y en la consola:

```javascript
PostPerfil("EliminarEmergencia", { idContacto: <IdContacto de A> },
           function (r) { console.log(r.mensaje); });
```
Expected: `No se encontró ese contacto.` — y el contacto de A sigue existiendo. Es la comprobación de que el `Cod_Usuario` en el `WHERE` del SP hace su trabajo.

- [ ] **Step 8: Commit**

```bash
git add docs/sql CapaDato CapaNegocio ReporteTareas
git commit -m "feat(perfil): contactos de emergencia, el primer dato de este tipo que el sistema guarda"
```

---

### Task 10: Conectar el enlace del menú y verificación completa

**Files:**
- Modify: `ReporteTareas/Formulario/Master.Master:104`

**Interfaces:**
- Consumes: `MiPerfil.aspx` (Task 7).
- Produces: nada; es la última tarea.

- [ ] **Step 1: Conectar el enlace**

En `ReporteTareas/Formulario/Master.Master`, línea 104, reemplazar:

```aspx
                            <li><a href="#"><i class="fa fa-user fa-fw"></i>Perfil Usuario</a></li>
```

por:

```aspx
                            <li><a href="MiPerfil.aspx"><i class="fa fa-user fa-fw"></i>Perfil Usuario</a></li>
```

- [ ] **Step 2: Compilar y correr todas las pruebas**

Run:
```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```
Expected: `Build succeeded. 0 Error(s)` y `Total tests: 17. Passed: 17.`

- [ ] **Step 3: Lista de comprobación manual**

Recorrer los seis casos del spec. Cada uno con un usuario distinto, sacados del reporte de excepciones de la Task 4:

| Caso | Qué debe pasar |
|---|---|
| Usuario **de los 118** con ficha | Ve los ocho campos personales llenos y la edad calculada |
| Usuario **de los 66 sin cédula** | Ve su perfil con lo disponible y el aviso amarillo. **No** una pantalla vacía |
| Usuario **de los 6 con cédula repetida** | No ve datos de la otra persona (`TieneFicha` es false y sale el aviso) |
| Usuario con fecha `dd/MM/yyyy` con **día > 12** | La edad sale correcta, no en blanco |
| Guardar contacto con `cargo` en el payload | El cargo no cambia |
| Eliminar el contacto de emergencia de otro | `No se encontró ese contacto.` |

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/Formulario/Master.Master
git commit -m "feat(perfil): la opcion Perfil Usuario del menu ya lleva al perfil"
```

---

## Notas de despliegue a producción

Cuando la fase 1 esté aprobada, seguir `DESPLIEGUE.md` en este orden:

1. Correr `docs/sql/2026-09-14-perfil-colaborador.sql` contra `ReporTarea`. **Guardar el resultado del reporte de excepciones y enviarlo a RRHH.**
2. Publicar con `FolderProfile` (Release) y **copiar sin sincronizar** al servidor.
3. Verificar con la lista de la Task 10, Step 3.

`CapaPruebas` no se publica: es un proyecto de biblioteca que no está referenciado por el web.

**Rollback:** republicar los binarios anteriores y devolver `href="#"` en `Master.Master:104`. Las tablas quedan vacías sin molestar a nadie.
