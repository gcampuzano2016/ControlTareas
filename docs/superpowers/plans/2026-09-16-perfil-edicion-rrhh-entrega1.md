# Perfil: edición por Talento Humano — Entrega 1 (la regla y el autor)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Separar «de quién es el perfil» de «quién lo está tocando» en todo el módulo de perfil, de modo que `Usu_Modificacion` guarde al autor real, y dejar puesta —probada y desplegada— la regla que decide si alguien puede actuar sobre un perfil ajeno.

**Architecture:** La regla vive en `CapaNegocio/NegPerfilAcceso.cs`, sin `HttpContext`, para que se pueda probar. Un ayudante delgado en el proyecto web (`PerfilIdentidad`) extrae el código pedido de cada uno de los tres transportes del módulo (JSON, multipart, query string) y delega. La cascada de firmas en C# se hace en tres pasos que **siempre compilan**: primero sobrecargas nuevas que delegan en las viejas, después la migración de los 16 sitios, y al final se borran las sobrecargas viejas — ese borrado es la prueba de que no quedó ningún sitio sin migrar.

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.8 en el web, 4.6.1 en las capas), SQL Server, MSTest v1 (ensamblado de VS2019, sin NuGet), `vstest.console.exe`.

**Spec:** `docs/superpowers/specs/2026-09-16-perfil-edicion-rrhh-design.md`

## Global Constraints

- **Al terminar esta entrega, el usuario no ve absolutamente ningún cambio.** `MiPerfil.aspx` no manda `codUsuario`, así que la regla devuelve el de la sesión, igual que hoy. Si algo se ve distinto, hay un error.
- **Perfiles autorizados: 14 (Talento Humano) y 18 (Super Admin).** Mismo criterio que `AdministrarHorasExtras.ashx`, que ya declara `internal static readonly int[] PerfilesAutorizados = { 14, 18 };`.
- **Los `.csproj` son de estilo antiguo: todo archivo nuevo debe listarse explícitamente** con `<Compile Include="..." />`. Un `.cs` que no esté listado no se compila y **nadie avisa**.
- **Convención de comentarios del repositorio: los comentarios en C# y SQL van sin tildes** (`identidad de la sesion`, `no es decorativo`). Los **textos que ve el usuario sí las llevan** (`"Su sesión expiró."`). Respetar las dos cosas.
- **Compilar con el MSBuild de VS2019**, no con el del PATH: `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`.
- **Correr las pruebas con:** `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll`
- **El SQL va siempre antes que los binarios** (`DESPLIEGUE.md`). Por eso `@Usu_Accion` es opcional: ver Task 3.
- **Los scripts SQL son idempotentes**, como todos los de `docs/sql/`.
- **No se tocan** `Sp_RTA_PerfilColaborador`, `Sp_RTA_PerfilDocumentoArchivo`, `Sp_RTA_PerfilEquipo` ni `Sp_RTA_PerfilEquipoLista`. Los cuatro son de lectura y ya reciben el código como parámetro.
- **No se tocan** `ListaEquipo` ni `PerfilEquipo` en el handler: ahí el código de la sesión es el del **jefe**, no el del dueño de un perfil.

---

## Estructura de archivos

| Archivo | Responsabilidad | Acción |
|---|---|---|
| `CapaEntidad/EntPerfilObjetivo.cs` | El resultado de la regla: permitido o no, el código, y el mensaje | **Crear** |
| `CapaNegocio/NegPerfilAcceso.cs` | **La regla.** Sin `HttpContext`. Es la pieza de la que depende toda la seguridad | **Crear** |
| `CapaPruebas/NegPerfilAccesoTests.cs` | Las 11 pruebas de la regla | **Crear** |
| `ReporteTareas/clases/PerfilIdentidad.cs` | Extrae de `HttpContext` y delega. Sin decisiones propias | **Crear** |
| `docs/sql/2026-09-16-perfil-autor-columnas.sql` | Columnas de autor en `Emp_CargaFamiliar` y `Empleados` | **Crear** |
| `docs/sql/2026-09-16-perfil-autor-procedimientos.sql` | Los 14 procedimientos con `@Usu_Accion` | **Crear** |
| `CapaDato/DaoPerfil.cs` | 15 métodos de escritura ganan `codAutor` | Modificar |
| `CapaNegocio/NegPerfil.cs` | 15 métodos de la fachada ganan `codAutor` | Modificar |
| `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` | 16 de sus 18 sitios pasan por la regla | Modificar |
| `ReporteTareas/Formulario/DescargarPerfil.ashx.cs` | El GET pasa por la regla | Modificar |

---

## Task 1: La regla, con sus pruebas

Es la única pieza de esta entrega que se puede probar de verdad, y es de la que depende todo lo demás. Va primera y va con TDD.

**Files:**
- Create: `CapaEntidad/EntPerfilObjetivo.cs`
- Create: `CapaNegocio/NegPerfilAcceso.cs`
- Create: `CapaPruebas/NegPerfilAccesoTests.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj` (junto a la línea 86, `EntPerfilCompleto.cs`)
- Modify: `CapaNegocio/CapaNegocio.csproj` (junto a la línea 207, `NegPerfil.cs`)
- Modify: `CapaPruebas/CapaPruebas.csproj` (junto a la línea 49, `NegPerfilCamposTests.cs`)

**Interfaces:**
- Consumes: nada.
- Produces:
  - `CapaEntidad.EntPerfilObjetivo` con `bool Permitido`, `string CodUsuario`, `string Mensaje`
  - `CapaNegocio.NegPerfilAcceso.PerfilesRRHH` → `int[]`
  - `CapaNegocio.NegPerfilAcceso.Objetivo(string codSesion, string idPerfilSesion, string codPedido)` → `EntPerfilObjetivo`
  - `CapaNegocio.NegPerfilAcceso.EsRRHH(string idPerfilSesion)` → `bool`

> **Por qué `idPerfilSesion` es `string` y no `int`.** `Session["Id_Perfil"]` es un `object` que puede venir nulo, vacío o con basura. Si la conversión se hiciera en el proyecto web, el caso «perfil ausente o no numérico» quedaría fuera del alcance de las pruebas — que es justo el caso en el que la regla tiene que cerrar. La conversión va **dentro** de `NegPerfilAcceso`.

- [ ] **Step 1: Crear la entidad del resultado**

Crear `CapaEntidad/EntPerfilObjetivo.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// El resultado de preguntar "sobre que perfil se esta actuando".
    ///
    /// No es un simple string porque hay tres desenlaces y no dos: el propio
    /// perfil, el de otra persona con permiso para ello, y el rechazo. Un string
    /// vacio para el rechazo obligaria a cada sitio a inventarse el mensaje, y
    /// entonces el mensaje dependeria del sitio en vez de la regla.
    /// </summary>
    public class EntPerfilObjetivo
    {
        public bool Permitido { get; set; }

        /// <summary>El Cod_Usuario del perfil sobre el que se actua. Vacio si no se permite.</summary>
        public string CodUsuario { get; set; }

        /// <summary>Que decirle al usuario cuando no se permite. Vacio si se permite.</summary>
        public string Mensaje { get; set; }
    }
}
```

Agregar a `CapaEntidad/CapaEntidad.csproj`, en orden alfabético junto a las otras `EntPerfil*`:

```xml
    <Compile Include="EntPerfilObjetivo.cs" />
```

- [ ] **Step 2: Escribir las pruebas, que todavía no compilan**

Crear `CapaPruebas/NegPerfilAccesoTests.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// La regla de la que depende que nadie escriba en el perfil de otro.
    ///
    /// Vive en CapaNegocio y no en el proyecto web por esto mismo: CapaPruebas
    /// solo referencia CapaEntidad y CapaNegocio. Una regla escrita dentro del
    /// handler no tendria ni una sola prueba.
    ///
    /// Los codigos de estas pruebas son inventados. No hay datos reales aca.
    /// </summary>
    [TestClass]
    public class NegPerfilAccesoTests
    {
        private const string Yo    = "USR001";
        private const string Otro  = "USR002";
        private const string Rrhh  = "14";
        private const string Admin = "18";
        private const string Comun = "7";

        /* ------------------------------------------------ sin codigo pedido --- */

        /// <summary>
        /// El caso de MiPerfil.aspx, que es el 99% del trafico: esa pantalla no
        /// manda codUsuario nunca. Si esta prueba falla, la pantalla que ya esta
        /// en produccion deja de funcionar.
        /// </summary>
        [TestMethod]
        public void Objetivo_SinCodigoPedido_DevuelveElDeLaSesion()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Comun, "");

            Assert.IsTrue(r.Permitido, "Sin codigo pedido siempre se permite: es el propio perfil");
            Assert.AreEqual(Yo, r.CodUsuario);
        }

        [TestMethod]
        public void Objetivo_CodigoPedidoNulo_DevuelveElDeLaSesion()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Comun, null);

            Assert.IsTrue(r.Permitido);
            Assert.AreEqual(Yo, r.CodUsuario);
        }

        /* --------------------------------------------------------- permitido --- */

        [TestMethod]
        public void Objetivo_PerfilTalentoHumano_PuedePedirUnoAjeno()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Rrhh, Otro);

            Assert.IsTrue(r.Permitido);
            Assert.AreEqual(Otro, r.CodUsuario);
        }

        [TestMethod]
        public void Objetivo_PerfilSuperAdmin_PuedePedirUnoAjeno()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Admin, Otro);

            Assert.IsTrue(r.Permitido);
            Assert.AreEqual(Otro, r.CodUsuario);
        }

        /// <summary>
        /// Pedir el propio codigo no es pedir uno ajeno. Sin este caso, un cliente
        /// que mandara su propio codigo -por simetria, por copiar y pegar- se
        /// llevaria un rechazo incomprensible.
        /// </summary>
        [TestMethod]
        public void Objetivo_UsuarioComunPideElSuyo_SePermite()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Comun, Yo);

            Assert.IsTrue(r.Permitido);
            Assert.AreEqual(Yo, r.CodUsuario);
        }

        /// <summary>
        /// Todo el modulo compara recortando -Cod_Jefe_Inm trae relleno y
        /// Cod_Usuario tambien-. Si esta comparacion fuera cruda, un codigo propio
        /// con un espacio delante se leeria como ajeno y el usuario recibiria un
        /// rechazo por editar su propio perfil.
        /// </summary>
        [TestMethod]
        public void Objetivo_CodigoPedidoConRelleno_SeComparaRecortado()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Comun, "  " + Yo + "  ");

            Assert.IsTrue(r.Permitido);
            Assert.AreEqual(Yo, r.CodUsuario, "Ademas de permitirlo, devuelve el codigo ya recortado");
        }

        /* ---------------------------------------------------------- rechazos --- */

        /// <summary>
        /// El caso que motiva toda la clase: alguien sin perfil de Talento Humano
        /// llamando al handler por HTTP directo con el codigo de otra persona.
        /// </summary>
        [TestMethod]
        public void Objetivo_UsuarioComunPideUnoAjeno_SeRechaza()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Comun, Otro);

            Assert.IsFalse(r.Permitido);
            Assert.AreEqual("", r.CodUsuario, "Un rechazo no puede devolver ningun codigo utilizable");
            Assert.AreNotEqual("", r.Mensaje, "El rechazo trae su propio mensaje: no lo inventa quien llama");
        }

        /// <summary>
        /// Cierra por defecto. Una sesion sin Id_Perfil, o con uno que no es un
        /// numero, no es una sesion de Talento Humano: es una sesion de la que no
        /// sabemos nada.
        /// </summary>
        [TestMethod]
        public void Objetivo_PerfilNoNumerico_SeRechazaElAjeno()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, "no-es-un-numero", Otro);

            Assert.IsFalse(r.Permitido);
        }

        [TestMethod]
        public void Objetivo_PerfilAusente_SeRechazaElAjeno()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, null, Otro);

            Assert.IsFalse(r.Permitido);
        }

        /// <summary>
        /// Sin sesion no hay ni perfil propio que devolver. Es el caso que hoy
        /// cubre el "No se pudo identificar al usuario de la sesion" de cada
        /// accion del handler, y que a partir de ahora vive aca.
        /// </summary>
        [TestMethod]
        public void Objetivo_SesionSinCodigo_SeRechaza()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo("", Comun, "");

            Assert.IsFalse(r.Permitido);
            Assert.AreNotEqual("", r.Mensaje);
        }

        /* ------------------------------------------------------------ EsRRHH --- */

        [TestMethod]
        public void EsRRHH_CatorceYDieciocho_SonLosDosUnicos()
        {
            Assert.IsTrue(NegPerfilAcceso.EsRRHH("14"));
            Assert.IsTrue(NegPerfilAcceso.EsRRHH("18"));
            Assert.IsFalse(NegPerfilAcceso.EsRRHH("7"));
            Assert.IsFalse(NegPerfilAcceso.EsRRHH("1"));
            Assert.IsFalse(NegPerfilAcceso.EsRRHH(""));
            Assert.IsFalse(NegPerfilAcceso.EsRRHH(null));
        }
    }
}
```

Agregar a `CapaPruebas/CapaPruebas.csproj`:

```xml
    <Compile Include="NegPerfilAccesoTests.cs" />
```

- [ ] **Step 3: Compilar y verificar que NO compila**

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
```

Esperado: **FALLA** con `CS0103: El nombre 'NegPerfilAcceso' no existe en el contexto actual` (o `CS0246`). Si compila, es que el archivo de pruebas no quedó en el `.csproj`.

- [ ] **Step 4: Escribir la regla**

Crear `CapaNegocio/NegPerfilAcceso.cs`:

```csharp
using CapaEntidad;
using System;

namespace CapaNegocio
{
    /// <summary>
    /// Quien puede actuar sobre el perfil de quien.
    ///
    /// Esta clase es la unica barrera real del modulo. El menu solo controla que
    /// la pantalla se VEA; los handlers son alcanzables por HTTP directo por
    /// cualquiera con sesion iniciada. Mismo razonamiento que
    /// AdministrarHorasExtras.ashx.
    ///
    /// Vive en CapaNegocio y no en el proyecto web a proposito: CapaPruebas solo
    /// referencia CapaEntidad y CapaNegocio, asi que una regla escrita dentro del
    /// handler seria una regla sin pruebas. Es el mismo motivo por el que la
    /// fase 3b saco su guarda a Fn_RTA_EsSubordinado en vez de escribirla a mano
    /// dentro del procedimiento.
    ///
    /// No conoce HttpContext. Quien la llama extrae los tres datos de donde sea
    /// que vengan -sesion, JSON, formulario multipart, query string- y los pasa.
    /// </summary>
    public class NegPerfilAcceso
    {
        /// <summary>
        /// Talento Humano (14) y Super Admin (18): los mismos dos perfiles a los
        /// que el menu le muestra las pantallas de nomina, y los mismos que
        /// AdministrarHorasExtras.ashx ya autoriza.
        /// </summary>
        public static readonly int[] PerfilesRRHH = { 14, 18 };

        /// <summary>
        /// Si ese perfil de sesion es uno de los dos que pueden ver y editar
        /// perfiles ajenos. Cualquier cosa que no sea exactamente 14 o 18
        /// -vacio, nulo, texto, un numero cualquiera- es que no.
        /// </summary>
        public static bool EsRRHH(string idPerfilSesion)
        {
            int idPerfil;
            if (!int.TryParse((idPerfilSesion ?? "").Trim(), out idPerfil)) { return false; }

            return Array.IndexOf(PerfilesRRHH, idPerfil) >= 0;
        }

        /// <summary>
        /// Sobre que perfil se esta actuando.
        ///
        /// El orden de las ramas importa: primero se descarta la sesion sin
        /// identidad, porque sin ella no hay ni perfil propio que devolver.
        /// Despues el caso normal -no se pidio ninguno-, que es el de
        /// MiPerfil.aspx y el 99% del trafico. Recien al final se evalua el
        /// permiso, que es el caso raro.
        ///
        /// Pedir el propio codigo NO es pedir uno ajeno: se compara recortando,
        /// igual que compara todo el resto del modulo.
        /// </summary>
        public static EntPerfilObjetivo Objetivo(string codSesion, string idPerfilSesion, string codPedido)
        {
            string propio = (codSesion ?? "").Trim();
            string pedido = (codPedido ?? "").Trim();

            if (propio == "")
            {
                return Rechazo("No se pudo identificar al usuario de la sesión.");
            }

            if (pedido == "" || string.Equals(pedido, propio, StringComparison.OrdinalIgnoreCase))
            {
                return Permitido(propio);
            }

            if (EsRRHH(idPerfilSesion))
            {
                return Permitido(pedido);
            }

            return Rechazo("No tiene permiso para ver ni editar el perfil de otra persona.");
        }

        private static EntPerfilObjetivo Permitido(string codUsuario)
        {
            return new EntPerfilObjetivo
            {
                Permitido  = true,
                CodUsuario = codUsuario,
                Mensaje    = ""
            };
        }

        /// <summary>
        /// Un rechazo devuelve el codigo VACIO, no el propio. Si devolviera el
        /// propio, un sitio que se olvidara de mirar Permitido escribiria sobre el
        /// perfil de quien llama en vez de fallar, y eso es un guardado silencioso
        /// en la fila equivocada: peor que un error.
        /// </summary>
        private static EntPerfilObjetivo Rechazo(string mensaje)
        {
            return new EntPerfilObjetivo
            {
                Permitido  = false,
                CodUsuario = "",
                Mensaje    = mensaje
            };
        }
    }
}
```

Agregar a `CapaNegocio/CapaNegocio.csproj`, junto a `NegPerfil.cs`:

```xml
    <Compile Include="NegPerfilAcceso.cs" />
```

- [ ] **Step 5: Compilar y correr las pruebas**

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: **todas pasan**, incluidas las que ya existían (`NegPerfilCamposTests`, `NegPerfilCvTests`, `EntPerfilEquipoTests`, las de horas extras). Anotar el total de pruebas: tiene que ser el de antes **+ 11**.

- [ ] **Step 6: Commit**

```bash
git add CapaEntidad/EntPerfilObjetivo.cs CapaEntidad/CapaEntidad.csproj \
        CapaNegocio/NegPerfilAcceso.cs CapaNegocio/CapaNegocio.csproj \
        CapaPruebas/NegPerfilAccesoTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(perfil): la regla de quien puede editar el perfil de quien

Vive en CapaNegocio y no en el handler porque CapaPruebas solo alcanza
a CapaEntidad y CapaNegocio: ahi dentro seria una regla sin pruebas.

Todavia no la llama nadie.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 2: Las columnas de autor en las dos tablas ajenas

`Emp_CargaFamiliar` y `Empleados` son de Talento Humano; las comparte `RRHHEmpleados.aspx`. Ninguna tiene hoy dónde anotar quién hizo un cambio desde el perfil.

**Files:**
- Create: `docs/sql/2026-09-16-perfil-autor-columnas.sql`

**Interfaces:**
- Consumes: nada.
- Produces: `dbo.Emp_CargaFamiliar.Usu_Modificacion VARCHAR(50) NULL` y `dbo.Empleados.Usu_ModificacionCod VARCHAR(50) NULL`, que Task 3 escribe.

> **Por qué una columna nueva en `Empleados` y no la que ya existe.** `Empleados.Usu_Modificacion` es `numeric(5)`, pensada para un correlativo interno de Talento Humano, y no admite un `Cod_Usuario` (que es texto). Ya está documentado dentro de `Sp_RTA_PerfilGuardarContacto`. No se toca ni se convierte: se agrega `Usu_ModificacionCod` al lado.

- [ ] **Step 1: Escribir el script**

Crear `docs/sql/2026-09-16-perfil-autor-columnas.sql`:

```sql
/* ============================================================================
   Perfil del colaborador: donde anotar QUIEN hizo el cambio
   ReporTarea  |  2026-09-16

   PENDIENTE DE EJECUTAR. Es el paso 1 de la entrega 1; el orden completo esta
   en docs/superpowers/specs/2026-09-16-perfil-edicion-rrhh-design.md.

   ----------------------------------------------------------------------------
   Que hace y por que

   Hasta hoy, en el modulo de perfil el dueno del perfil y el autor del cambio
   eran siempre la misma persona, asi que Usu_Modificacion = Cod_Usuario era
   cierto por construccion. En cuanto Talento Humano pueda corregir el perfil de
   otro, esa columna pasaria a MENTIR: diria que el cambio lo hizo el empleado.

   Las siete tablas Perfil_* ya tienen Usu_Modificacion VARCHAR(50) y no
   necesitan nada. Las dos que este script toca no son de este modulo:

     Emp_CargaFamiliar   no tiene ninguna columna de autor.
     Empleados           tiene Usu_Modificacion, pero es numeric(5) -un
                         correlativo interno de Talento Humano- y no admite un
                         Cod_Usuario. No se convierte: se agrega una al lado.

   Las dos columnas son NULL y no las nombra ningun INSERT existente, asi que
   RRHHEmpleados.aspx sigue funcionando exactamente igual.

   Idempotente.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '== Perfil: columnas de autor - inicio ==';
GO

/* ------------------------------------------------ 1. Emp_CargaFamiliar --- */

IF OBJECT_ID('dbo.Emp_CargaFamiliar','U') IS NULL
BEGIN
    RAISERROR('dbo.Emp_CargaFamiliar no existe. Script detenido.', 16, 1);
    RETURN;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.Emp_CargaFamiliar')
                  AND name = 'Usu_Modificacion')
BEGIN
    ALTER TABLE dbo.Emp_CargaFamiliar ADD Usu_Modificacion VARCHAR(50) NULL;
    PRINT 'Emp_CargaFamiliar.Usu_Modificacion creada.';
END
ELSE PRINT 'Emp_CargaFamiliar.Usu_Modificacion ya existia.';
GO

/* -------------------------------------------------------- 2. Empleados --- */

IF OBJECT_ID('dbo.Empleados','U') IS NULL
BEGIN
    RAISERROR('dbo.Empleados no existe. Script detenido.', 16, 1);
    RETURN;
END
GO

/* Usu_ModificacionCod, no Usu_Modificacion: esa ya existe y es numeric(5).
   El sufijo Cod dice de que esta hecha -un Cod_Usuario- y evita que alguien
   la confunda con la vieja. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.Empleados')
                  AND name = 'Usu_ModificacionCod')
BEGIN
    ALTER TABLE dbo.Empleados ADD Usu_ModificacionCod VARCHAR(50) NULL;
    PRINT 'Empleados.Usu_ModificacionCod creada.';
END
ELSE PRINT 'Empleados.Usu_ModificacionCod ya existia.';
GO

PRINT '== Perfil: columnas de autor - fin ==';
GO
```

- [ ] **Step 2: Ejecutar el script contra la base**

Ejecutarlo en SQL Server Management Studio contra `ReporTarea`.

Esperado en la pestaña de mensajes:

```
== Perfil: columnas de autor - inicio ==
Emp_CargaFamiliar.Usu_Modificacion creada.
Empleados.Usu_ModificacionCod creada.
== Perfil: columnas de autor - fin ==
```

- [ ] **Step 3: Verificar que las dos columnas existen**

```sql
SELECT  tabla    = OBJECT_NAME(c.object_id),
        columna  = c.name,
        tipo     = t.name,
        largo    = c.max_length,
        admiteNulo = c.is_nullable
  FROM  sys.columns c
  JOIN  sys.types  t ON t.user_type_id = c.user_type_id
 WHERE (OBJECT_NAME(c.object_id) = 'Emp_CargaFamiliar' AND c.name = 'Usu_Modificacion')
    OR (OBJECT_NAME(c.object_id) = 'Empleados'         AND c.name = 'Usu_ModificacionCod');
```

Esperado: **2 filas**, las dos `varchar`, largo `50`, `admiteNulo = 1`.

- [ ] **Step 4: Verificar que no se rompió la pantalla de Talento Humano**

Abrir `RRHHEmpleados.aspx` en la aplicación, entrar a un empleado y **guardar una carga familiar**. Tiene que funcionar igual que antes. Es la comprobación de que agregar las columnas no tocó nada de lo suyo.

- [ ] **Step 5: Ejecutarlo una segunda vez**

Volver a ejecutar el mismo script completo.

Esperado: `... ya existia.` en las dos, sin errores. Es un script de producción: tiene que poder correrse dos veces.

- [ ] **Step 6: Commit**

```bash
git add docs/sql/2026-09-16-perfil-autor-columnas.sql
git commit -m "feat(perfil): columnas donde anotar el autor del cambio

Emp_CargaFamiliar no tenia ninguna. Empleados tiene Usu_Modificacion pero
es numeric(5) y no admite un Cod_Usuario, asi que se agrega una al lado en
vez de convertirla.

Las dos son NULL y ningun INSERT de RRHHEmpleados.aspx las nombra.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 3: Los 14 procedimientos con `@Usu_Accion`

**Files:**
- Create: `docs/sql/2026-09-16-perfil-autor-procedimientos.sql`

**Interfaces:**
- Consumes: las dos columnas de Task 2.
- Produces: 14 procedimientos que aceptan `@Usu_Accion VARCHAR(50) = NULL` como **último parámetro**. Task 5 los llama por nombre de parámetro desde `DaoPerfil`.

> **`@Usu_Accion` es opcional, y esa es la decisión más importante de esta tarea.**
> `DESPLIEGUE.md` manda ejecutar el SQL **antes** de publicar los binarios. Si el parámetro fuera obligatorio, desde que corre este script hasta que suben los binarios nuevos, la aplicación viva llamaría a procedimientos que exigen un parámetro que todavía no manda: **todo el módulo de perfil dejaría de guardar**, con un error de SQL por cada acción.
>
> Con `= NULL` y la línea de respaldo, los binarios viejos siguen comportándose **exactamente** como hoy (autor = dueño), y en cuanto entran los nuevos empieza a llegar el autor real.

### La transformación, que es la misma en los 14

Cada procedimiento recibe **tres** cambios y ninguno más:

**(a)** Un parámetro nuevo, **siempre el último** de la lista:

```sql
    @Usu_Accion VARCHAR(50) = NULL
```

**(b)** Inmediatamente después de `SET NOCOUNT ON;`, la línea de respaldo:

```sql
    /* Binarios viejos sobre base nueva: si nadie mando el autor, el autor es el
       dueno, que es exactamente como se comportaba este procedimiento antes.
       Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);
```

**(c)** Cada escritura de `Usu_Modificacion` pasa de `@Cod_Usuario` a `@Usu_Accion`. **Solo la de `Usu_Modificacion`** — la columna `Cod_Usuario` sigue recibiendo `@Cod_Usuario`, siempre.

### De dónde sale el cuerpo de cada uno

El script nuevo **hace `DROP` y `CREATE` de los 14**. El cuerpo de cada uno se copia de su script de origen y se le aplican los tres cambios. Origen y líneas exactas:

| # | Procedimiento | Archivo de origen | Línea del `CREATE` | Líneas con `Usu_Modificacion = @Cod_Usuario` |
|---|---|---|---:|---|
| 1 | `Sp_RTA_PerfilGuardarContacto` | `docs/sql/2026-09-14-perfil-colaborador.sql` | 505 | 536, y el `VALUES` de 540 |
| 2 | `Sp_RTA_PerfilGuardarEmergencia` | `docs/sql/2026-09-14-perfil-colaborador.sql` | 598 | 636, y el `VALUES` de 624 |
| 3 | `Sp_RTA_PerfilEliminarEmergencia` | `docs/sql/2026-09-14-perfil-colaborador.sql` | 659 | 682 |
| 4 | `Sp_RTA_PerfilGuardarEstudio` | `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` | 49 | 91, y el `VALUES` de 77 |
| 5 | `Sp_RTA_PerfilEliminarEstudio` | `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` | 110 | 133 |
| 6 | `Sp_RTA_PerfilGuardarCertificacion` | `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` | 155 | 195, y el `VALUES` de 183 |
| 7 | `Sp_RTA_PerfilEliminarCertificacion` | `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` | 212 | 235 |
| 8 | `Sp_RTA_PerfilGuardarExperiencia` | `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` | 255 | 299, y el `VALUES` de 284 |
| 9 | `Sp_RTA_PerfilEliminarExperiencia` | `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` | 316 | 339 |
| 10 | `Sp_RTA_PerfilGuardarCargaFamiliar` | `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` | 406 | **ninguna todavía** — ver (d) |
| 11 | `Sp_RTA_PerfilEliminarCargaFamiliar` | `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` | 468 | **ninguna todavía** — ver (d) |
| 12 | `Sp_RTA_PerfilGuardarFoto` | `docs/sql/2026-09-15-perfil-colaborador-fase3a.sql` | 38 | 70, y el `VALUES` de 75 |
| 13 | `Sp_RTA_PerfilGuardarDocumento` | `docs/sql/2026-09-15-perfil-colaborador-fase3a.sql` | 130 | el `VALUES` de 185 |
| 14 | `Sp_RTA_PerfilEliminarDocumento` | `docs/sql/2026-09-15-perfil-colaborador-fase3a.sql` | 205 | 227 |

**(d) Los cuatro casos especiales**, que además de (a), (b) y (c) necesitan una escritura **nueva**, porque hasta ahora no tenían dónde:

- **#1 `Sp_RTA_PerfilGuardarContacto`** — su `UPDATE dbo.Empleados` (el del estado civil) suma `Usu_ModificacionCod = @Usu_Accion`.
- **#10 `Sp_RTA_PerfilGuardarCargaFamiliar`** — su `INSERT` y su `UPDATE` sobre `Emp_CargaFamiliar` suman `Usu_Modificacion = @Usu_Accion`.
- **#11 `Sp_RTA_PerfilEliminarCargaFamiliar`** — su `UPDATE` sobre `Emp_CargaFamiliar` suma `Usu_Modificacion = @Usu_Accion`.

> **`Sp_RTA_PerfilEliminarFoto` NO está en la lista de 14, y no es un olvido.** Hace `DELETE FROM dbo.Perfil_Foto`: borrada la fila, no queda ninguna columna donde anotar nada. Un parámetro que no se usa es ruido. **No se toca ese procedimiento.** La consecuencia se asume: quitar una foto no deja rastro de quién la quitó — tampoco lo dejaba antes.

### Los dos ejemplos completos

El primero es el caso simple; el segundo, el que además escribe en tabla ajena. Los otros doce siguen uno de estos dos moldes.

**Ejemplo A — `Sp_RTA_PerfilEliminarEmergencia`** (el molde de los cinco `Eliminar*` que escriben en tablas `Perfil_*`):

```sql
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarEmergencia','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarEmergencia;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarEmergencia
    @Cod_Usuario VARCHAR(50),
    @IdContacto  INT,
    @Ip          VARCHAR(64),
    @Usu_Accion  VARCHAR(50) = NULL      -- (a) siempre el ultimo
AS
BEGIN
    SET NOCOUNT ON;

    /* (b) Binarios viejos sobre base nueva: si nadie mando el autor, el autor
       es el dueno, que es exactamente como se comportaba este procedimiento
       antes. Sin esto, el SQL no se podria desplegar antes que los binarios. */
    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.Perfil_ContactoEmergencia
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Usu_Accion,   -- (c) era @Cod_Usuario
           Ip_Modificacion  = @Ip
     WHERE IdContacto  = @IdContacto
       AND Cod_Usuario = @Cod_Usuario;       -- el dueno sigue siendo el dueno

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarEmergencia actualizado.';
GO
```

> El cuerpo entre `DECLARE @CodigoRepetido` y el `UPDATE` se copia **tal cual** del original: la guarda de código repetido, su comparación y su `RETURN` no se tocan en ningún procedimiento.

**Ejemplo B — `Sp_RTA_PerfilEliminarCargaFamiliar`** (el molde de las dos que escriben en `Emp_CargaFamiliar`, con la escritura nueva de (d)):

```sql
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCargaFamiliar','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarCargaFamiliar;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarCargaFamiliar
    @Cod_Usuario VARCHAR(50),
    @IdCargaFam  INT,
    @Ip          VARCHAR(64),
    @Usu_Accion  VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @Usu_Accion = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    /* Estado sigue siendo '1'/'0' y NO 'Activo'/'Inactivo'. Es deliberado: el
       boton de RRHHEmpleados.aspx (Sp_RTACambiarEstadoCargaFam) es un toggle
       'Activo'<->'Inactivo' POR NOMBRE sobre TODA la tabla, sin filtrar por
       dueno. Con este vocabulario, ese CASE cae en ELSE y no alcanza estas
       filas. No cambiar sin entender esto.

       Usu_Modificacion es la columna nueva de 2026-09-16-perfil-autor-columnas. */
    UPDATE dbo.Emp_CargaFamiliar
       SET Estado           = '0',
           Fec_Modificacion = GETDATE(),
           Usu_Modificacion = @Usu_Accion,   -- (d) escritura nueva
           Ip_Modificacion  = LEFT(@Ip, 32)
     WHERE IdCargaFam  = @IdCargaFam
       AND Cod_Usuario = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarCargaFamiliar actualizado.';
GO
```

- [ ] **Step 1: Escribir el script con los 14**

Crear `docs/sql/2026-09-16-perfil-autor-procedimientos.sql` con esta cabecera, y a continuación los 14 bloques `DROP`/`CREATE` en el orden de la tabla de arriba:

```sql
/* ============================================================================
   Perfil del colaborador: Usu_Modificacion pasa a guardar al AUTOR
   ReporTarea  |  2026-09-16

   PENDIENTE DE EJECUTAR. Paso 2 de la entrega 1. Requiere haber corrido antes
   docs/sql/2026-09-16-perfil-autor-columnas.sql.

   ----------------------------------------------------------------------------
   Que hace y por que

   Los 14 procedimientos de escritura del modulo guardaban
   Usu_Modificacion = @Cod_Usuario, es decir, el DUENO del perfil. Era cierto
   por construccion: solo el dueno podia escribir. Cuando Talento Humano pueda
   corregir el perfil de otro, esa columna diria que el cambio lo hizo el
   empleado. Pasa a guardar @Usu_Accion: quien lo hizo de verdad.

   @Usu_Accion es OPCIONAL a proposito. El SQL se despliega antes que los
   binarios (DESPLIEGUE.md). Si fuera obligatorio, entre un paso y el otro la
   aplicacion viva llamaria a procedimientos que exigen un parametro que
   todavia no manda, y el modulo entero dejaria de guardar. Con el valor por
   omision, los binarios viejos se comportan igual que hoy.

   Sp_RTA_PerfilEliminarFoto NO esta aca: hace un DELETE fisico y, borrada la
   fila, no hay columna donde anotar al autor.

   Los cuatro de lectura (PerfilColaborador, PerfilDocumentoArchivo,
   PerfilEquipo, PerfilEquipoLista) tampoco: no escriben.

   Idempotente: cada procedimiento se hace DROP y CREATE.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* Las dos columnas de autor tienen que existir antes: tres de estos
   procedimientos escriben en ellas y el CREATE fallaria al compilarse. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.Emp_CargaFamiliar')
                  AND name = 'Usu_Modificacion')
   OR NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.Empleados')
                     AND name = 'Usu_ModificacionCod')
BEGIN
    /* SET NOEXEC ON y no RETURN: RETURN fuera de un procedimiento sale del LOTE,
       no del script. Despues del GO la ejecucion seguiria y los 14 CREATE
       PROCEDURE se intentarian igual, fallando uno por uno con errores de
       columna inexistente en vez de con este mensaje. NOEXEC hace que los lotes
       siguientes se analicen pero no se ejecuten. Se apaga al final del archivo. */
    RAISERROR('Falta correr 2026-09-16-perfil-autor-columnas.sql. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

PRINT '== Perfil: el autor en los 14 procedimientos - inicio ==';
GO
```

Y **al final del archivo**, despues del ultimo procedimiento, siempre:

```sql
/* Incondicional: si la guarda de arriba encendio NOEXEC, apagarlo aca es lo que
   evita que el resto de la sesion de SSMS quede sin ejecutar nada y parezca que
   los scripts siguientes "no hacen nada". */
SET NOEXEC OFF;
GO

PRINT '== Perfil: el autor en los 14 procedimientos - fin ==';
GO
```

- [ ] **Step 2: Ejecutar el script contra la base**

Esperado: 14 líneas `Sp_RTA_Perfil... actualizado.`, sin errores.

- [ ] **Step 3: Verificar que los 14 tienen el parámetro y que `EliminarFoto` no**

```sql
SELECT  procedimiento = OBJECT_NAME(p.object_id),
        tieneUsuAccion = MAX(CASE WHEN pa.name = '@Usu_Accion' THEN 1 ELSE 0 END)
  FROM  sys.procedures p
  LEFT JOIN sys.parameters pa ON pa.object_id = p.object_id
 WHERE  OBJECT_NAME(p.object_id) LIKE 'Sp_RTA_Perfil%'
 GROUP BY OBJECT_NAME(p.object_id)
 ORDER BY tieneUsuAccion DESC, procedimiento;
```

Esperado: **14 filas con `tieneUsuAccion = 1`** y **5 con `0`** (`Sp_RTA_PerfilEliminarFoto`, `Sp_RTA_PerfilColaborador`, `Sp_RTA_PerfilDocumentoArchivo`, `Sp_RTA_PerfilEquipo`, `Sp_RTA_PerfilEquipoLista`). Total: 19.

- [ ] **Step 4: Verificar que los binarios VIEJOS siguen guardando igual**

Esta es la comprobación que justifica el `= NULL`, y hay que hacerla **antes** de publicar binarios nuevos.

Con la aplicación **tal como está desplegada hoy**, entrar a `MiPerfil.aspx` con un usuario cualquiera, ir a la pestaña **Contacto** y guardar un cambio en el teléfono.

```sql
SELECT Cod_Usuario, TelefonoPersonal, Usu_Modificacion, Fec_Modificacion
  FROM dbo.Perfil_ContactoPersonal
 WHERE Cod_Usuario = 'EL_CODIGO_DE_PRUEBA';
```

Esperado: el teléfono cambió y `Usu_Modificacion` trae **su propio código**. Si trae `NULL`, la línea de respaldo (b) falta en ese procedimiento.

- [ ] **Step 5: Ejecutarlo una segunda vez**

Esperado: las mismas 14 líneas, sin errores.

- [ ] **Step 6: Commit**

```bash
git add docs/sql/2026-09-16-perfil-autor-procedimientos.sql
git commit -m "feat(perfil): Usu_Modificacion guarda al autor, no al dueno

Los 14 procedimientos de escritura ganan @Usu_Accion. Es opcional a
proposito: el SQL se despliega antes que los binarios y, si fuera
obligatorio, el modulo dejaria de guardar en esa ventana.

EliminarFoto queda fuera: hace un DELETE fisico y no hay donde anotar.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 4: Las capas aceptan el autor, sin cambiar nada todavía

Se agregan sobrecargas nuevas de 4 parámetros; las viejas de 3 se quedan y delegan pasando el dueño como autor — que es el comportamiento de hoy. **Todo compila y nada cambia de comportamiento.** Las viejas se borran en Task 6, y ese borrado es lo que demuestra que no quedó ningún sitio sin migrar.

**Files:**
- Modify: `CapaDato/DaoPerfil.cs` (15 métodos, líneas 206-558)
- Modify: `CapaNegocio/NegPerfil.cs` (15 métodos, líneas 32-115)

**Interfaces:**
- Consumes: los procedimientos de Task 3.
- Produces, en `CapaDato.DaoPerfil` y replicado en `CapaNegocio.NegPerfil`:
  - `GuardarContacto(string codUsuario, string codAutor, EntPerfilContacto contacto, string ip)` → `EntRespuesta`
  - `GuardarEmergencia(string codUsuario, string codAutor, EntPerfilEmergencia c, string ip)` → `EntRespuesta`
  - `EliminarEmergencia(string codUsuario, string codAutor, int idContacto, string ip)` → `EntRespuesta`
  - `GuardarEstudio(string codUsuario, string codAutor, EntPerfilEstudio e, string ip)` → `EntRespuesta`
  - `EliminarEstudio(string codUsuario, string codAutor, int idEstudio, string ip)` → `EntRespuesta`
  - `GuardarCertificacion(string codUsuario, string codAutor, EntPerfilCertificacion c, string ip)` → `EntRespuesta`
  - `EliminarCertificacion(string codUsuario, string codAutor, int idCertificacion, string ip)` → `EntRespuesta`
  - `GuardarExperiencia(string codUsuario, string codAutor, EntPerfilExperiencia x, string ip)` → `EntRespuesta`
  - `EliminarExperiencia(string codUsuario, string codAutor, int idExperiencia, string ip)` → `EntRespuesta`
  - `GuardarCargaFamiliar(string codUsuario, string codAutor, EntPerfilCargaFamiliar c, string ip)` → `EntRespuesta`
  - `EliminarCargaFamiliar(string codUsuario, string codAutor, int idCargaFam, string ip)` → `EntRespuesta`
  - `GuardarFoto(string codUsuario, string codAutor, EntPerfilFoto foto, string ip)` → `EntRespuesta`
  - `EliminarFoto(string codUsuario, string codAutor, string ip)` → `EntRespuesta`
  - `GuardarDocumento(string codUsuario, string codAutor, EntPerfilDocumento doc, string ip)` → `EntRespuesta`
  - `EliminarDocumento(string codUsuario, string codAutor, int idDocumento, string ip)` → `EntRespuesta`

> **`codAutor` va siempre en segundo lugar**, pegado a `codUsuario`. Los dos son `string` y confundirlos no da error de compilación: tenerlos juntos hace que un intercambio se vea al leer, y que el orden sea el mismo en los 15.

- [ ] **Step 1: Convertir los 15 métodos de `DaoPerfil`**

Para cada uno: renombrar el actual agregando `codAutor` en segundo lugar, pasar `@Usu_Accion` al comando, y dejar debajo la sobrecarga vieja delegando.

`GuardarContacto` queda así (reemplaza `CapaDato/DaoPerfil.cs:206-219`):

```csharp
        public static EntRespuesta GuardarContacto(string codUsuario, string codAutor,
                                                   EntPerfilContacto contacto, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarContacto", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",      SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@CorreoPersonal",   SqlDbType.VarChar, 150).Value = contacto.CorreoPersonal;
                cmd.Parameters.Add("@TelefonoPersonal", SqlDbType.VarChar,  50).Value = contacto.TelefonoPersonal;
                cmd.Parameters.Add("@Direccion",        SqlDbType.VarChar, 400).Value = contacto.Direccion;
                cmd.Parameters.Add("@EstadoCivil",      SqlDbType.VarChar, 100).Value = contacto.EstadoCivil;
                cmd.Parameters.Add("@Ip",               SqlDbType.VarChar,  64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",       SqlDbType.VarChar,  50).Value = codAutor ?? "";
            });

            return RespuestaDe(r, "Sus datos de contacto se guardaron correctamente.", "No se pudo guardar el contacto.");
        }

        /// <summary>
        /// TEMPORAL. Sobrecarga de transicion: el autor es el dueno, que es como
        /// se comportaba el modulo antes de que Talento Humano pudiera editar
        /// perfiles ajenos. Se borra al final de la entrega 1, y ese borrado es
        /// lo que demuestra que ningun sitio se quedo sin migrar.
        /// </summary>
        public static EntRespuesta GuardarContacto(string codUsuario, EntPerfilContacto contacto, string ip)
        {
            return GuardarContacto(codUsuario, codUsuario, contacto, ip);
        }
```

Repetir la misma forma para los otros 14. Dos advertencias concretas:

- **`EliminarFoto`** recibe `codAutor` y **no lo pasa al comando**, porque `Sp_RTA_PerfilEliminarFoto` no tiene `@Usu_Accion` (hace un `DELETE` físico). Lleva este comentario, o alguien lo va a "arreglar":

```csharp
        public static EntRespuesta EliminarFoto(string codUsuario, string codAutor, string ip)
        {
            /* codAutor se recibe y no se usa, a proposito.
               Sp_RTA_PerfilEliminarFoto hace un DELETE fisico sobre Perfil_Foto:
               borrada la fila, no hay columna donde anotar al autor. El parametro
               esta para que esta capa no tenga una excepcion de firma que haya que
               recordar en cada sitio que la llama. */
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarFoto", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "La foto se quitó correctamente.", "No se pudo quitar la foto.");
        }
```

- **`GuardarDocumento` es el único que usa `EjecutarEscrituraConId`** (el ayudante de la línea 332); los otros catorce van por `EjecutarEscritura`. Se le agrega `@Usu_Accion` igual que a los demás; lo que no cambia es cómo lee el id de vuelta. Verificado método por método sobre `CapaDato/DaoPerfil.cs`, no supuesto.

- [ ] **Step 2: Convertir los 15 métodos de `NegPerfil`**

Misma operación sobre la fachada. `GuardarContacto` queda así (reemplaza `CapaNegocio/NegPerfil.cs:31-35`):

```csharp
        /// <summary>
        /// Guarda el contacto editable de un perfil.
        ///
        /// codUsuario es DE QUIEN es el perfil; codAutor es QUIEN lo esta
        /// tocando. Hasta la edicion por Talento Humano eran siempre el mismo y
        /// por eso habia un solo parametro.
        /// </summary>
        public static EntRespuesta GuardarContacto(string codUsuario, string codAutor,
                                                   EntPerfilContacto contacto, string ip)
        {
            return DaoPerfil.GuardarContacto(codUsuario, codAutor, contacto, ip);
        }

        /// <summary>TEMPORAL. Se borra al final de la entrega 1. Ver DaoPerfil.</summary>
        public static EntRespuesta GuardarContacto(string codUsuario, EntPerfilContacto contacto, string ip)
        {
            return GuardarContacto(codUsuario, codUsuario, contacto, ip);
        }
```

Repetir para los otros 14.

- [ ] **Step 3: Compilar la solución entera**

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: **compila sin errores**. `AdministrarPerfil.ashx.cs` todavía no se tocó y sigue llamando a las sobrecargas de 3 parámetros: por eso compila.

- [ ] **Step 4: Correr las pruebas**

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: **todas pasan**, el mismo total que al final de Task 1.

- [ ] **Step 5: Commit**

```bash
git add CapaDato/DaoPerfil.cs CapaNegocio/NegPerfil.cs
git commit -m "refactor(perfil): las capas aceptan el autor del cambio

Sobrecargas nuevas con codAutor en segundo lugar. Las viejas se quedan
delegando -autor = dueno, como hoy- para que todo compile mientras se
migran los sitios que llaman. Se borran al final de la entrega.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 5: Los handlers pasan por la regla

**Files:**
- Create: `ReporteTareas/clases/PerfilIdentidad.cs`
- Modify: `ReporteTareas/ReporteTareas.csproj` (junto a la línea 1443, `clases\SeguridadHelper.cs`)
- Modify: `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` (16 sitios)
- Modify: `ReporteTareas/Formulario/DescargarPerfil.ashx.cs`

**Interfaces:**
- Consumes: `NegPerfilAcceso.Objetivo` (Task 1) y las sobrecargas de 4 parámetros (Task 4).
- Produces:
  - `PerfilIdentidad.Autor(HttpContext)` → `string`
  - `PerfilIdentidad.Objetivo(HttpContext, string codPedido)` → `EntPerfilObjetivo`
  - `PerfilIdentidad.CodigoPedidoJson(dynamic campos)` → `string`
  - `PerfilIdentidad.CodigoPedidoFormulario(HttpContext)` → `string`
  - `PerfilIdentidad.CodigoPedidoQuery(HttpContext)` → `string`

- [ ] **Step 1: Escribir el ayudante**

Crear `ReporteTareas/clases/PerfilIdentidad.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using System.Web;

namespace ReporteTareas.clases
{
    /// <summary>
    /// De donde sale la identidad en el modulo de perfil.
    ///
    /// Esta clase NO decide nada: extrae de HttpContext y le pregunta a
    /// NegPerfilAcceso, que es donde vive la regla y donde estan las pruebas.
    /// Si alguna vez aparece un "if" con un numero de perfil en este archivo,
    /// esta en el lugar equivocado.
    ///
    /// Existe porque el modulo habla por tres transportes distintos -JSON,
    /// multipart y query string- y el criterio tiene que ser uno solo. Cada
    /// transporte aporta su extractor; la decision es compartida.
    /// </summary>
    internal static class PerfilIdentidad
    {
        /// <summary>
        /// QUIEN esta actuando. Sale de la sesion y nunca de otro lado: no hay
        /// parametro, no hay rama, el cliente no puede influir. Es lo que hace
        /// que Usu_Modificacion signifique algo.
        /// </summary>
        internal static string Autor(HttpContext context)
        {
            if (context.Session != null && context.Session["Cod_Usuario"] != null)
            {
                return context.Session["Cod_Usuario"].ToString().Trim();
            }
            return "";
        }

        /// <summary>DE QUIEN es el perfil sobre el que se actua.</summary>
        internal static EntPerfilObjetivo Objetivo(HttpContext context, string codPedido)
        {
            string idPerfil = "";
            if (context.Session != null && context.Session["Id_Perfil"] != null)
            {
                idPerfil = context.Session["Id_Perfil"].ToString();
            }

            return NegPerfilAcceso.Objetivo(Autor(context), idPerfil, codPedido);
        }

        /// <summary>El codigo pedido en el payload JSON, o cadena vacia.</summary>
        internal static string CodigoPedidoJson(dynamic campos)
        {
            var d = campos as System.Collections.Generic.IDictionary<string, object>;
            if (d == null) { return ""; }

            object valor;
            if (!d.TryGetValue("codUsuario", out valor) || valor == null) { return ""; }

            return valor.ToString().Trim();
        }

        /// <summary>El codigo pedido en el formulario multipart, o cadena vacia.</summary>
        internal static string CodigoPedidoFormulario(HttpContext context)
        {
            return (context.Request.Form.Get("codUsuario") ?? "").Trim();
        }

        /// <summary>El codigo pedido en la query string, o cadena vacia.</summary>
        internal static string CodigoPedidoQuery(HttpContext context)
        {
            return (context.Request.QueryString["u"] ?? "").Trim();
        }
    }
}
```

Agregar a `ReporteTareas/ReporteTareas.csproj`:

```xml
    <Compile Include="clases\PerfilIdentidad.cs" />
```

- [ ] **Step 2: Migrar los 16 sitios de `AdministrarPerfil.ashx.cs`**

En los **13 métodos que reciben `campos`**, reemplazar este bloque:

```csharp
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }
```

por este:

```csharp
                EntPerfilObjetivo objetivo =
                    PerfilIdentidad.Objetivo(context, PerfilIdentidad.CodigoPedidoJson(campos));

                if (!objetivo.Permitido)
                {
                    return responseMessage("0", objetivo.Mensaje, "danger");
                }

                string codUsuario = objetivo.CodUsuario;
                string codAutor   = PerfilIdentidad.Autor(context);
```

y en la llamada a `NegPerfil`, agregar `codAutor` en segundo lugar.

> **`CargarPerfil` es lectura: no lleva la línea de `codAutor`.** Una variable
> local sin usar es un aviso del compilador, y en un archivo de 859 líneas los
> avisos se vuelven ruido que tapa el siguiente de verdad. `GuardarContacto` queda así (`AdministrarPerfil.ashx.cs:213-239`):

```csharp
        private string GuardarContacto(HttpContext context, dynamic campos)
        {
            try
            {
                EntPerfilObjetivo objetivo =
                    PerfilIdentidad.Objetivo(context, PerfilIdentidad.CodigoPedidoJson(campos));

                if (!objetivo.Permitido)
                {
                    return responseMessage("0", objetivo.Mensaje, "danger");
                }

                string codUsuario = objetivo.CodUsuario;
                string codAutor   = PerfilIdentidad.Autor(context);

                var diccionario = campos as System.Collections.Generic.IDictionary<string, object>;
                EntPerfilContacto contacto = NegPerfilCampos.LeerContacto(diccionario);

                string error = NegPerfilCampos.ValidarContacto(contacto);
                if (error != "")
                {
                    return responseMessage("0", error, "warning");
                }

                return ToJson(NegPerfil.GuardarContacto(codUsuario, codAutor, contacto,
                                                        context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar el contacto. " + ex.Message, "danger");
            }
        }
```

Los **tres sitios que no reciben `campos`** usan su propio extractor:

- `CargarPerfil(HttpContext context)` — línea 185. Es lectura: **no necesita `codAutor`**. Cambiar la firma a `CargarPerfil(HttpContext context, dynamic campos)`, usar `CodigoPedidoJson(campos)`, y actualizar el despacho de la línea 47 a `CargarPerfil(context, parametros[0]["parameters"])`.
- `EliminarFoto(HttpContext context)` — línea 547. Cambiar la firma a `EliminarFoto(HttpContext context, dynamic campos)`, usar `CodigoPedidoJson(campos)`, y actualizar el despacho de la línea 126.
- `SubirDocumento(HttpContext context)` — línea 577. Es la rama multipart: usa `PerfilIdentidad.CodigoPedidoFormulario(context)`.

**No tocar `ListaEquipo` (738) ni `PerfilEquipo` (772).** Ahí el código de la sesión es el del **jefe**, no el del dueño de un perfil. Siguen llamando a `CodUsuarioSesion`, que por eso se conserva.

Agregar al principio del archivo, si no está:

```csharp
using ReporteTareas.clases;
```

- [ ] **Step 3: Migrar `DescargarPerfil.ashx.cs`**

Reemplazar las líneas 31-36:

```csharp
            string codUsuario = CodUsuarioSesion(context);

            if (codUsuario == "")
            {
                NoDisponible(context, "No se pudo identificar al usuario de la sesión.");
                return;
            }
```

por:

```csharp
            /* El unico parametro nuevo que se acepta es de QUIEN es el perfil, y
               solo Talento Humano puede usarlo: la regla esta en NegPerfilAcceso.
               Que documento se quiere lo sigue diciendo la query string, y de
               quien es ese documento lo sigue diciendo la base. */
            EntPerfilObjetivo objetivo =
                PerfilIdentidad.Objetivo(context, PerfilIdentidad.CodigoPedidoQuery(context));

            if (!objetivo.Permitido)
            {
                /* Mismo texto que cuando el documento no existe, a proposito: uno
                   distinto le confirmaria a quien esta probando codigos que acerto
                   con uno. */
                NoDisponible(context, "No se encontró ese documento.");
                return;
            }

            string codUsuario = objetivo.CodUsuario;
```

Y **borrar el ayudante `CodUsuarioSesion` de ese archivo** (`DescargarPerfil.ashx.cs:171`).
La línea 31 era su **única** llamada; al reemplazarla queda como método privado muerto.
Ojo con no confundirse: en `AdministrarPerfil.ashx.cs` ese mismo ayudante **se queda**,
porque `ListaEquipo` y `PerfilEquipo` lo siguen usando.

- [ ] **Step 4: Compilar la solución entera**

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: **compila sin errores**.

- [ ] **Step 5: Verificar que Mi perfil sigue igual**

Publicar en el IIS local y entrar a `MiPerfil.aspx` con un usuario **de perfil común** (ni 14 ni 18):

1. El perfil carga completo, con sus pestañas.
2. Guardar un cambio en **Contacto** → funciona.
3. Agregar y borrar un **contacto de emergencia** → funciona.
4. Subir un **documento** y descargarlo → funciona.

Esto es lo que dice el objetivo de la entrega: el usuario no ve **ningún** cambio.

- [ ] **Step 6: Verificar que la barrera existe**

Con ese mismo usuario común y la sesión abierta, desde la consola del navegador en `MiPerfil.aspx`:

```javascript
$.ajax({
    type: "POST",
    url: "AdministrarPerfil.ashx",
    data: JSON.stringify([{ action: "CargarPerfil", parameters: { codUsuario: "CODIGO_DE_OTRA_PERSONA" } }]),
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (r) { console.log(r); }
});
```

Esperado: `{estado: "0", mensaje: "No tiene permiso para ver ni editar el perfil de otra persona.", ...}`.
**No** el perfil de esa persona. Este es el caso que la pantalla no protege y el handler sí.

- [ ] **Step 7: Commit**

```bash
git add ReporteTareas/clases/PerfilIdentidad.cs ReporteTareas/ReporteTareas.csproj \
        ReporteTareas/Formulario/AdministrarPerfil.ashx.cs \
        ReporteTareas/Formulario/DescargarPerfil.ashx.cs
git commit -m "feat(perfil): los tres transportes preguntan por la misma regla

JSON, multipart y query string extraen cada uno el codigo pedido de donde
le toca y delegan en NegPerfilAcceso. PerfilIdentidad no decide nada.

ListaEquipo y PerfilEquipo no se tocan: ahi el codigo de la sesion es el
del jefe, no el del dueno de un perfil.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 6: Borrar las sobrecargas de transición

El compilador demuestra que ningún sitio se quedó llamando a la versión que confunde dueño con autor. Es una tarea corta y es la que cierra la entrega.

**Files:**
- Modify: `CapaDato/DaoPerfil.cs` (borrar 15 sobrecargas)
- Modify: `CapaNegocio/NegPerfil.cs` (borrar 15 sobrecargas)

**Interfaces:**
- Consumes: todo lo anterior.
- Produces: `DaoPerfil` y `NegPerfil` con **una sola** firma por operación, la de 4 parámetros.

- [ ] **Step 1: Borrar las 15 sobrecargas de `DaoPerfil`**

Son las que llevan el comentario `TEMPORAL`. Borrar el método completo y su comentario.

- [ ] **Step 2: Borrar las 15 sobrecargas de `NegPerfil`**

Igual.

- [ ] **Step 3: Compilar y leer los errores como un informe**

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: **compila sin errores**.

Si aparece algún `CS1501: No hay ninguna sobrecarga para el método ... que acepte 3 argumentos`, **no es un problema: es el hallazgo**. Cada error señala un sitio que se quedó sin migrar en Task 5 y que habría guardado el dueño como autor. Migrarlo con la misma forma de Task 5 y volver a compilar.

- [ ] **Step 4: Confirmar que no queda ningún rastro**

```bash
grep -rn "TEMPORAL" CapaDato/DaoPerfil.cs CapaNegocio/NegPerfil.cs
```

Esperado: **sin resultados**.

- [ ] **Step 5: Correr las pruebas**

```
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: **todas pasan**.

- [ ] **Step 6: Commit**

```bash
git add CapaDato/DaoPerfil.cs CapaNegocio/NegPerfil.cs
git commit -m "refactor(perfil): fuera las sobrecargas de transicion

Que la solucion compile sin ellas es la prueba de que no quedo ningun
sitio llamando a la version que confundia dueno con autor.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 7: Verificación contra la base y paquete de despliegue

Nada de lo anterior demuestra que `Usu_Modificacion` guarde al autor cuando dueño y autor **son distintos**, porque hasta aquí no hay pantalla para provocarlo. Se provoca a mano.

**Files:**
- Modify: `ReporteTareas/obj/Release/Package/PackageTmp/**` (regenerado, no editado)

- [ ] **Step 1: Comprobar que Talento Humano ya puede, y que queda registrado**

Iniciar sesión con un usuario de **perfil 14**. Desde la consola del navegador en `MiPerfil.aspx`:

```javascript
$.ajax({
    type: "POST",
    url: "AdministrarPerfil.ashx",
    data: JSON.stringify([{ action: "CargarPerfil", parameters: { codUsuario: "CODIGO_DE_OTRA_PERSONA" } }]),
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (r) { console.log(r); }
});
```

Esperado: **el perfil de esa otra persona**. Esta es la capacidad que la entrega 2 va a usar.

Ahora una escritura, con el mismo usuario de perfil 14:

```javascript
$.ajax({
    type: "POST",
    url: "AdministrarPerfil.ashx",
    data: JSON.stringify([{ action: "GuardarEmergencia", parameters: {
        codUsuario: "CODIGO_DE_OTRA_PERSONA",
        idContacto: 0, nombre: "Prueba Autor", parentesco: "Prueba", telefono: "0999999999"
    }}]),
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (r) { console.log(r); }
});
```

```sql
SELECT TOP 5 Cod_Usuario, Nombre, Usu_Modificacion, Fec_Modificacion
  FROM dbo.Perfil_ContactoEmergencia
 WHERE Cod_Usuario = 'CODIGO_DE_OTRA_PERSONA'
 ORDER BY IdContacto DESC;
```

Esperado: `Cod_Usuario` es **el de la otra persona** y `Usu_Modificacion` es **el del usuario de perfil 14**. Si los dos son iguales, el autor no está llegando y hay que revisar Task 5.

Borrar después la fila de prueba.

- [ ] **Step 2: Comprobar las dos tablas ajenas**

Con el mismo usuario de perfil 14, guardar una **carga familiar** sobre esa otra persona (acción `GuardarCargaFamiliar`, mismo formato que arriba, con `nombre`, `parentesco` y `fechaNacimiento`).

```sql
SELECT TOP 5 Cod_Usuario, Nombre, Estado, Usu_Modificacion, Fec_Modificacion
  FROM dbo.Emp_CargaFamiliar
 WHERE Cod_Usuario = 'CODIGO_DE_OTRA_PERSONA'
 ORDER BY IdCargaFam DESC;
```

Esperado: `Usu_Modificacion` trae el código del perfil 14, y `Estado` es `'1'` — **no** `'Activo'`. Borrar después la fila de prueba.

- [ ] **Step 3: Comprobar el rechazo en la descarga**

Con un usuario **de perfil común**, pedir en la barra de direcciones:

```
DescargarPerfil.ashx?doc=<id de un documento de OTRA persona>&u=<codigo de esa otra persona>
```

Esperado: `No se encontró ese documento.` — el mismo texto que para un documento inexistente.

Con un usuario **de perfil 14**, la misma URL entrega el archivo.

- [ ] **Step 4: Regenerar el paquete de despliegue**

Publicar con el perfil `FolderProfile` en **Release**. El repositorio versiona `obj/Release/Package/PackageTmp` a propósito: el despliegue sale de ahí.

- [ ] **Step 5: Comprobar que el paquete no quedó viejo**

```bash
grep -o 'miPerfil.js?v=[0-9]*' ReporteTareas/Formulario/MiPerfil.aspx
grep -o 'miPerfil.js?v=[0-9]*' ReporteTareas/obj/Release/Package/PackageTmp/Formulario/MiPerfil.aspx
```

Esperado: **el mismo número en los dos**. Si difieren, el paquete no se regeneró y un despliegue llevaría la pantalla vieja sobre la base nueva, **sin ningún error visible**. Ha pasado cuatro veces seguidas en este módulo.

- [ ] **Step 6: Commit del paquete**

`git status` va a marcar cientos de archivos del paquete como modificados: la mayoría son **finales de línea**. Al preparar el índice quedan unas pocas decenas con cambio real. No es motivo de alarma ni para evitar el commit.

```bash
git add ReporteTareas/obj/Release/Package/PackageTmp ReporteTareas/bin CapaDato/bin CapaNegocio/bin CapaEntidad/bin
git commit -m "build: paquete regenerado con la entrega 1 del perfil por Talento Humano

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Qué queda para la entrega 2

Al terminar este plan, el sistema **ya es capaz** de que un perfil 14 o 18 lea y edite el perfil de otro, pero **no hay ninguna pantalla que lo haga**. Eso es intencional: la regla queda probada y desplegada antes de que exista nada que la ejercite.

La entrega 2 construye encima: `PerfilFichas.ascx`, la pantalla nueva con su buscador, `Sp_RTA_PerfilPersonalLista`, el script de menú para los perfiles 14 y 18, y los tres retoques a `miPerfil.js`. Su plan se escribe cuando esta entrega esté desplegada y verificada.
