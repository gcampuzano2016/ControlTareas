# Perfil del colaborador — Fase 2 · Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que cada colaborador registre su hoja de vida dentro del sistema — estudios, certificaciones, experiencia laboral y cargas familiares — desde la misma pantalla de perfil que ya usa.

**Architecture:** Se extiende el módulo que dejó la fase 1, sin cambiar su forma. Las cuatro tablas ya existen en producción y `Sp_RTA_PerfilColaborador` ya devuelve tres de las cuatro listas; falta leerlas, escribirlas y pintarlas. La lógica que puede fallar en silencio se extrae a funciones puras en `CapaNegocio` para probarla sin base de datos, igual que en la fase 1.

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.8 en el web, 4.6.1 en las capas), SQL Server, jQuery + Bootstrap 3, MSTest v1 (ensamblado de VS2019, sin NuGet), `vstest.console.exe`.

**Spec:** `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md`

**Fase anterior:** `docs/superpowers/plans/2026-09-14-perfil-colaborador-fase1.md` (completa, desplegada en producción)

## Global Constraints

- **Rama:** `ProyectoNuevosCambios`. No commitear ni publicar sin que el usuario lo pida.
- **Compilar con el MSBuild de VS2019**, nunca el del PATH: `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"`. En Git Bash hace falta `MSYS_NO_PATHCONV=1` y `-p:` en vez de `/p:`.
- **`Cod_Usuario` sale de `context.Session["Cod_Usuario"]`. Nunca del payload.**
- **Todo procedimiento de escritura lleva la guarda `@CodigoRepetido`** con `RETURN;` real. Un `SELECT` de retorno no interrumpe la ejecución en T-SQL.
- **`LEFT(@Ip, 32)`** en cualquier escritura a `Emp_CargaFamiliar`: su `Ip_Modificacion` es `varchar(32)`, y con una IPv6 el `UPDATE` no trunca, **falla y tumba el guardado entero**.
- **La validación vive en el servidor.** El handler es alcanzable por HTTP; una validación que sólo esté en el navegador no es una validación.
- **Nada se inserta en la página construyendo HTML.** Todo con `.text()`: estos datos los teclea el propio usuario.
- **El script SQL corre antes que los binarios**, es idempotente, y arranca con `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;` — no los toques.
- **`miPerfil.js` cambia: el `?v=` sube de 4 a 5** en `MiPerfil.aspx`.
- Comentarios de código en español **sin tildes**; el texto de cara al usuario sí las lleva, **y habla de usted**, como el resto del sistema.
- **Commitear con rutas explícitas de archivo.** Nunca `git add` de directorio: el repositorio versiona carpetas `bin/`.
- Estilo SQL de la casa: `INT IDENTITY(1,1)`, constraints con nombre, `DATETIME2(0)` con `SYSDATETIME()`, **sin claves foráneas**.

## Lo que la fase 1 ya dejó montado

No lo vuelvas a construir:

| Pieza | Estado |
|---|---|
| `Perfil_Estudio`, `Perfil_Certificacion`, `Perfil_Experiencia` | Creadas en producción, vacías |
| `Emp_CargaFamiliar` con `Cod_Usuario` | Existía; la fase 1 le agregó la columna. **0 filas** |
| `Sp_RTA_PerfilColaborador` | Ya devuelve estudios (4), certificaciones (5) y experiencia (6), con la guarda aplicada |
| `DaoPerfil.CargarPerfil` | Lee sólo los conjuntos 1-3; los demás los salta |
| `AdministrarPerfil.ashx` | Acciones `CargarPerfil`, `GuardarContacto`, `GuardarEmergencia`, `EliminarEmergencia` |
| `MiPerfil.aspx` | Pestañas *Datos personales*, *Contacto y domicilio*, *Emergencia*. Falta añadir tres |
| `MostrarMensaje` y el modal | Presentes en la pantalla y funcionando |
| `NegPerfilCampos` | `EdadDesdeTexto`, `LeerContacto`, `ValidarEmergencia`. 24 pruebas pasando |

## Tres decisiones tomadas antes de escribir el plan

**1. Las cargas familiares se leen en un result set nuevo, el 8, añadido al final.**
`Sp_RTA_PerfilColaborador` devuelve siete conjuntos y el Dao los identifica **por posición**. Insertar las cargas en medio renumeraría todo lo que va detrás. Hoy nadie lee los conjuntos 4-7, así que renumerar sería inocuo — pero un contrato posicional se amplía por el final, no por el medio, y esa disciplina es lo que evita que un día alguien desplace un conjunto y los datos aterricen en la propiedad equivocada sin error.

**2. Las cargas familiares usan `'Activo'`/`'Inactivo'`, no `'1'`/`'0'`.**
`Emp_CargaFamiliar` la comparte `RRHHEmpleados.aspx`, cuyo `Sp_RTACambiarEstadoCargaFam` alterna entre esos dos textos. Si la fase 2 escribiera `'1'`/`'0'`, las dos pantallas discreparían sobre qué está eliminado: una carga borrada desde el perfil seguiría activa para RRHH.

La consistencia **dentro de una tabla** pesa más que la consistencia con las tablas nuevas del módulo.

Además, el `INSERT` de RRHH **no escribe `Estado`**, así que deja `NULL`, y su propio SP de alternar no puede moverlo porque el `CASE` cae en `ELSE Estado`. Por eso el filtro de lectura de la fase 2 es `ISNULL(Estado,'') <> 'Inactivo'` y no `= 'Activo'`: así también se ven las filas que cree RRHH.

**3. `IdEmpleado` se deja nulo cuando la persona no tiene ficha.**
La columna es nullable (verificado). Las cargas cuelgan de `Cod_Usuario`, así que los 119 sin ficha las registran igual. No hace falta tocar el esquema.

## Estructura de archivos

| Archivo | Responsabilidad | Acción |
|---|---|---|
| `CapaNegocio/NegPerfilCampos.cs` | Validaciones puras de las cuatro entidades | Modificar |
| `CapaPruebas/NegPerfilCamposTests.cs` | Sus pruebas | Modificar |
| `CapaEntidad/EntPerfilEstudio.cs` | Un estudio | Crear |
| `CapaEntidad/EntPerfilCertificacion.cs` | Una certificación | Crear |
| `CapaEntidad/EntPerfilExperiencia.cs` | Una experiencia laboral | Crear |
| `CapaEntidad/EntPerfilCargaFamiliar.cs` | Una carga familiar | Crear |
| `CapaEntidad/EntPerfilCompleto.cs` | Contenedor: cuatro listas nuevas | Modificar |
| `CapaDato/DaoPerfil.cs` | Lectura de los conjuntos 4-8 y las ocho escrituras | Modificar |
| `CapaNegocio/NegPerfil.cs` | Fachada | Modificar |
| `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` | Ocho acciones nuevas | Modificar |
| `ReporteTareas/Formulario/MiPerfil.aspx` | Tres pestañas nuevas | Modificar |
| `ReporteTareas/js/miPerfil.js` | Render y guardado de las cuatro listas | Modificar |
| `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` | Ocho SPs nuevos y el conjunto 8 | Crear |

**El SQL de la fase 2 va en un archivo propio**, no dentro del de la fase 1. Aquel ya está aplicado en producción y volver a tocarlo obligaría a releer 700 líneas para encontrar lo nuevo. El archivo nuevo empieza con los mismos `SET` y es igualmente idempotente.

---

### Task 1: Validaciones de las cuatro entidades

Es la primera porque es lo único de la fase que se puede probar sin base de datos, y porque las cuatro escrituras dependen de ella.

**Files:**
- Modify: `CapaEntidad/CapaEntidad.csproj`
- Create: `CapaEntidad/EntPerfilEstudio.cs`, `EntPerfilCertificacion.cs`, `EntPerfilExperiencia.cs`, `EntPerfilCargaFamiliar.cs`
- Modify: `CapaNegocio/NegPerfilCampos.cs`
- Modify: `CapaPruebas/NegPerfilCamposTests.cs`

**Interfaces:**
- Consumes: nada de tareas posteriores.
- Produces:
  - `CapaEntidad.EntPerfilEstudio` con `int IdEstudio`, `string Nivel`, `string Institucion`, `string Titulo`, `int? AnioGraduacion`
  - `CapaEntidad.EntPerfilCertificacion` con `int IdCertificacion`, `string Nombre`, `string Entidad`, `string FechaObtencion` (texto `yyyy-MM`)
  - `CapaEntidad.EntPerfilExperiencia` con `int IdExperiencia`, `string Empresa`, `string Cargo`, `int? AnioDesde`, `int? AnioHasta`, `string Funciones`
  - `CapaEntidad.EntPerfilCargaFamiliar` con `int IdCargaFam`, `string Nombre`, `string Parentesco`, `string FechaNacimiento` (texto `yyyy-MM-dd`)
  - `NegPerfilCampos.ValidarEstudio(EntPerfilEstudio)` → `string`
  - `NegPerfilCampos.ValidarCertificacion(EntPerfilCertificacion)` → `string`
  - `NegPerfilCampos.ValidarExperiencia(EntPerfilExperiencia)` → `string`
  - `NegPerfilCampos.ValidarCargaFamiliar(EntPerfilCargaFamiliar)` → `string`

  Las cuatro devuelven **cadena vacía si el dato sirve**, o el mensaje listo para mostrar al usuario, con tildes y en usted. Mismo contrato que `ValidarEmergencia`.

- [ ] **Step 1: Crear las cuatro entidades**

`CapaEntidad/EntPerfilEstudio.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>Un estudio formal del colaborador.</summary>
    public class EntPerfilEstudio
    {
        public int IdEstudio { get; set; }
        public string Nivel { get; set; } = "";
        public string Institucion { get; set; } = "";
        public string Titulo { get; set; } = "";

        /// <summary>Nulo mientras no se sepa; nunca cero, que se leeria como dato real.</summary>
        public int? AnioGraduacion { get; set; }
    }
}
```

`CapaEntidad/EntPerfilCertificacion.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>Una certificacion o curso con entidad emisora.</summary>
    public class EntPerfilCertificacion
    {
        public int IdCertificacion { get; set; }
        public string Nombre { get; set; } = "";
        public string Entidad { get; set; } = "";

        /// <summary>
        /// Texto "yyyy-MM", que es lo que produce un input type="month". Se deja
        /// como texto hasta la capa de datos por la misma razon que la fecha de
        /// nacimiento: la conversion vive en un solo sitio y con formato explicito.
        /// </summary>
        public string FechaObtencion { get; set; } = "";
    }
}
```

`CapaEntidad/EntPerfilExperiencia.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>Un empleo anterior.</summary>
    public class EntPerfilExperiencia
    {
        public int IdExperiencia { get; set; }
        public string Empresa { get; set; } = "";
        public string Cargo { get; set; } = "";
        public int? AnioDesde { get; set; }

        /// <summary>Nulo significa "hasta hoy", no "no se sabe".</summary>
        public int? AnioHasta { get; set; }

        public string Funciones { get; set; } = "";
    }
}
```

`CapaEntidad/EntPerfilCargaFamiliar.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Una carga familiar. Cuelga de Cod_Usuario y no de IdEmpleado, que es
    /// nullable: 119 de 231 usuarios no tienen ficha de empleado enlazada y
    /// tienen que poder registrar sus cargas igual.
    /// </summary>
    public class EntPerfilCargaFamiliar
    {
        public int IdCargaFam { get; set; }
        public string Nombre { get; set; } = "";
        public string Parentesco { get; set; } = "";

        /// <summary>Texto "yyyy-MM-dd", que es lo que produce un input type="date".</summary>
        public string FechaNacimiento { get; set; } = "";
    }
}
```

Agregar las cuatro a `CapaEntidad/CapaEntidad.csproj`:

```xml
<Compile Include="EntPerfilEstudio.cs" />
<Compile Include="EntPerfilCertificacion.cs" />
<Compile Include="EntPerfilExperiencia.cs" />
<Compile Include="EntPerfilCargaFamiliar.cs" />
```

- [ ] **Step 2: Escribir las pruebas que fallan**

Agregar al final de la clase `NegPerfilCamposTests` en `CapaPruebas/NegPerfilCamposTests.cs`. La clase ya tiene `[TestInitialize] FijarCulturaHostil` y `[TestCleanup] RestaurarCultura` que fuerzan cultura `en-US`: **no los toques ni los dupliques.**

```csharp
        /* ------------------------------------------------------ estudios ---- */

        [TestMethod]
        public void ValidarEstudio_CompletoYCorrecto_SinError()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas", AnioGraduacion = 2019
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_SinTitulo_Rechaza()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "   ", AnioGraduacion = 2019
            };

            Assert.AreEqual("Escriba el título obtenido.", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_SinInstitucion_Rechaza()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "", Titulo = "Ingenieria en Sistemas",
                AnioGraduacion = 2019
            };

            Assert.AreEqual("Escriba la institución donde estudió.", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_SinNivel_Rechaza()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas", AnioGraduacion = 2019
            };

            Assert.AreEqual("Seleccione el nivel de estudio.", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_AnioEnElFuturo_Rechaza()
        {
            // Se permite el anio que viene -alguien que se gradua en diciembre lo
            // registra en enero- pero no mas alla.
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas",
                AnioGraduacion = System.DateTime.Today.Year + 2
            };

            Assert.AreEqual("El año de graduación no puede ser posterior al próximo año.",
                            NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_AnioProximo_LoAcepta()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas",
                AnioGraduacion = System.DateTime.Today.Year + 1
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_AnioDemasiadoAntiguo_Rechaza()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas", AnioGraduacion = 1939
            };

            Assert.AreEqual("El año de graduación no parece correcto.",
                            NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_SinAnio_LoAcepta()
        {
            // Nulo es "no lo recuerdo", que es legitimo y no debe bloquear el registro.
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas", AnioGraduacion = null
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_Nulo_Rechaza()
        {
            Assert.AreEqual("No se recibió el estudio.", NegPerfilCampos.ValidarEstudio(null));
        }

        /* ------------------------------------------------ certificaciones --- */

        [TestMethod]
        public void ValidarCertificacion_CompletaYCorrecta_SinError()
        {
            var c = new EntPerfilCertificacion
            {
                Nombre = "ITIL Foundation v4", Entidad = "AXELOS", FechaObtencion = "2023-05"
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_SinNombre_Rechaza()
        {
            var c = new EntPerfilCertificacion
            {
                Nombre = "", Entidad = "AXELOS", FechaObtencion = "2023-05"
            };

            Assert.AreEqual("Escriba el nombre de la certificación.",
                            NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_SinEntidad_Rechaza()
        {
            var c = new EntPerfilCertificacion
            {
                Nombre = "ITIL Foundation v4", Entidad = "  ", FechaObtencion = "2023-05"
            };

            Assert.AreEqual("Escriba la entidad que la emitió.",
                            NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_FechaConFormatoRaro_Rechaza()
        {
            var c = new EntPerfilCertificacion
            {
                Nombre = "ITIL Foundation v4", Entidad = "AXELOS", FechaObtencion = "mayo 2023"
            };

            Assert.AreEqual("La fecha de obtención no es válida.",
                            NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_FechaFutura_Rechaza()
        {
            string futura = System.DateTime.Today.AddYears(1).ToString("yyyy-MM");
            var c = new EntPerfilCertificacion
            {
                Nombre = "ITIL Foundation v4", Entidad = "AXELOS", FechaObtencion = futura
            };

            Assert.AreEqual("La fecha de obtención no puede estar en el futuro.",
                            NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_SinFecha_LaAcepta()
        {
            var c = new EntPerfilCertificacion
            {
                Nombre = "ITIL Foundation v4", Entidad = "AXELOS", FechaObtencion = ""
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_Nula_Rechaza()
        {
            Assert.AreEqual("No se recibió la certificación.",
                            NegPerfilCampos.ValidarCertificacion(null));
        }

        /* ----------------------------------------------------- experiencia -- */

        [TestMethod]
        public void ValidarExperiencia_CompletaYCorrecta_SinError()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = 2017, AnioHasta = 2019, Funciones = "Mesa de ayuda."
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_SinEmpresa_Rechaza()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "", Cargo = "Tecnico de Soporte N1", AnioDesde = 2017, AnioHasta = 2019
            };

            Assert.AreEqual("Escriba el nombre de la empresa.",
                            NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_SinCargo_Rechaza()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "   ", AnioDesde = 2017, AnioHasta = 2019
            };

            Assert.AreEqual("Escriba el cargo que ocupó.", NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_SinAnioDesde_Rechaza()
        {
            // A diferencia del anio de graduacion, este si es obligatorio: sin el,
            // el CV no puede ordenar la experiencia, que es para lo que existe.
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = null, AnioHasta = 2019
            };

            Assert.AreEqual("Indique el año en que empezó.", NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_HastaAnteriorADesde_Rechaza()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = 2019, AnioHasta = 2017
            };

            Assert.AreEqual("El año en que terminó no puede ser anterior al año en que empezó.",
                            NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_MismoAnioDesdeYHasta_LoAcepta()
        {
            // Un contrato de pocos meses empieza y termina el mismo anio.
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = 2019, AnioHasta = 2019
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_HastaNulo_SignificaActualidad()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = 2017, AnioHasta = null
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_DesdeEnElFuturo_Rechaza()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = System.DateTime.Today.Year + 1
            };

            Assert.AreEqual("El año en que empezó no puede estar en el futuro.",
                            NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_Nula_Rechaza()
        {
            Assert.AreEqual("No se recibió la experiencia laboral.",
                            NegPerfilCampos.ValidarExperiencia(null));
        }

        /* ------------------------------------------------ carga familiar ---- */

        [TestMethod]
        public void ValidarCargaFamiliar_CompletaYCorrecta_SinError()
        {
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "Mateo Comina Ortiz", Parentesco = "Hijo/a", FechaNacimiento = "2019-06-02"
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_SinNombre_Rechaza()
        {
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "", Parentesco = "Hijo/a", FechaNacimiento = "2019-06-02"
            };

            Assert.AreEqual("Escriba el nombre completo de la carga familiar.",
                            NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_SinParentesco_Rechaza()
        {
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "Mateo Comina Ortiz", Parentesco = "", FechaNacimiento = "2019-06-02"
            };

            Assert.AreEqual("Seleccione el parentesco.", NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_FechaFutura_Rechaza()
        {
            string futura = System.DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "Mateo Comina Ortiz", Parentesco = "Hijo/a", FechaNacimiento = futura
            };

            Assert.AreEqual("La fecha de nacimiento no puede estar en el futuro.",
                            NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_FechaConFormatoRaro_Rechaza()
        {
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "Mateo Comina Ortiz", Parentesco = "Hijo/a", FechaNacimiento = "02/06/2019"
            };

            Assert.AreEqual("La fecha de nacimiento no es válida.",
                            NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_SinFecha_Rechaza()
        {
            // Aqui la fecha SI es obligatoria: sin ella no se sabe si la carga
            // sigue siendo carga, que es para lo que Talento Humano la usa.
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "Mateo Comina Ortiz", Parentesco = "Hijo/a", FechaNacimiento = ""
            };

            Assert.AreEqual("Indique la fecha de nacimiento.",
                            NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_Nula_Rechaza()
        {
            Assert.AreEqual("No se recibió la carga familiar.",
                            NegPerfilCampos.ValidarCargaFamiliar(null));
        }
```

- [ ] **Step 3: Compilar y confirmar que falla**

Run:
```
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
```
Expected: FALLA con varios `CS0117: 'NegPerfilCampos' no contiene una definición para 'ValidarEstudio'` (y las otras tres). **Captura la salida literal**: es la evidencia de que las pruebas prueban algo.

Si **no** falla, párate y repórtalo: significaría que los métodos ya existen.

- [ ] **Step 4: Implementar las cuatro validaciones**

Agregar a `CapaNegocio/NegPerfilCampos.cs`, dentro de la clase:

```csharp
        /* Rango plausible para un anio academico o laboral. El piso es el mismo
           1940 que usa el reporte de excepciones del script para marcar fechas de
           nacimiento absurdas. El techo es el anio proximo, no el actual: alguien
           que se gradua en diciembre registra su titulo en enero. */
        private const int AnioMinimoPlausible = 1940;

        /// <summary>Cadena vacia si el estudio sirve; si no, el mensaje para el usuario.</summary>
        public static string ValidarEstudio(EntPerfilEstudio estudio)
        {
            if (estudio == null) { return "No se recibió el estudio."; }

            if (string.IsNullOrWhiteSpace(estudio.Nivel))
            {
                return "Seleccione el nivel de estudio.";
            }

            if (string.IsNullOrWhiteSpace(estudio.Institucion))
            {
                return "Escriba la institución donde estudió.";
            }

            if (string.IsNullOrWhiteSpace(estudio.Titulo))
            {
                return "Escriba el título obtenido.";
            }

            /* El anio es opcional: "no lo recuerdo" es una respuesta legitima y no
               debe impedir que registre el estudio. Solo se valida si vino. */
            if (estudio.AnioGraduacion.HasValue)
            {
                if (estudio.AnioGraduacion.Value > DateTime.Today.Year + 1)
                {
                    return "El año de graduación no puede ser posterior al próximo año.";
                }

                if (estudio.AnioGraduacion.Value < AnioMinimoPlausible)
                {
                    return "El año de graduación no parece correcto.";
                }
            }

            return "";
        }

        /// <summary>Cadena vacia si la certificacion sirve; si no, el mensaje.</summary>
        public static string ValidarCertificacion(EntPerfilCertificacion cert)
        {
            if (cert == null) { return "No se recibió la certificación."; }

            if (string.IsNullOrWhiteSpace(cert.Nombre))
            {
                return "Escriba el nombre de la certificación.";
            }

            if (string.IsNullOrWhiteSpace(cert.Entidad))
            {
                return "Escriba la entidad que la emitió.";
            }

            /* La fecha es opcional. Si vino, tiene que ser "yyyy-MM" -lo que produce
               un input type="month"- y con formato explicito, por la misma razon que
               la fecha de nacimiento: sin el, la cultura del servidor decide y en
               produccion no es la misma que aqui. */
            if (!string.IsNullOrWhiteSpace(cert.FechaObtencion))
            {
                DateTime obtenida;
                bool valida = DateTime.TryParseExact(cert.FechaObtencion.Trim(),
                                                     "yyyy-MM",
                                                     CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None,
                                                     out obtenida);
                if (!valida)
                {
                    return "La fecha de obtención no es válida.";
                }

                /* Se compara contra el primer dia del mes siguiente: una certificacion
                   obtenida "este mes" es valida aunque el dia 1 ya haya pasado. */
                DateTime inicioMesSiguiente = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1);
                if (obtenida >= inicioMesSiguiente)
                {
                    return "La fecha de obtención no puede estar en el futuro.";
                }
            }

            return "";
        }

        /// <summary>Cadena vacia si la experiencia sirve; si no, el mensaje.</summary>
        public static string ValidarExperiencia(EntPerfilExperiencia exp)
        {
            if (exp == null) { return "No se recibió la experiencia laboral."; }

            if (string.IsNullOrWhiteSpace(exp.Empresa))
            {
                return "Escriba el nombre de la empresa.";
            }

            if (string.IsNullOrWhiteSpace(exp.Cargo))
            {
                return "Escriba el cargo que ocupó.";
            }

            /* Aqui el anio de inicio SI es obligatorio, al reves que en los estudios:
               el CV ordena la experiencia por fecha, y una fila sin anio no tiene
               donde colocarse. */
            if (!exp.AnioDesde.HasValue)
            {
                return "Indique el año en que empezó.";
            }

            if (exp.AnioDesde.Value > DateTime.Today.Year)
            {
                return "El año en que empezó no puede estar en el futuro.";
            }

            if (exp.AnioDesde.Value < AnioMinimoPlausible)
            {
                return "El año en que empezó no parece correcto.";
            }

            /* AnioHasta nulo significa "sigo ahi", no "no se sabe". Por eso no se
               exige, pero si vino tiene que ser coherente. */
            if (exp.AnioHasta.HasValue)
            {
                if (exp.AnioHasta.Value < exp.AnioDesde.Value)
                {
                    return "El año en que terminó no puede ser anterior al año en que empezó.";
                }

                if (exp.AnioHasta.Value > DateTime.Today.Year + 1)
                {
                    return "El año en que terminó no puede estar en el futuro.";
                }
            }

            return "";
        }

        /// <summary>Cadena vacia si la carga familiar sirve; si no, el mensaje.</summary>
        public static string ValidarCargaFamiliar(EntPerfilCargaFamiliar carga)
        {
            if (carga == null) { return "No se recibió la carga familiar."; }

            if (string.IsNullOrWhiteSpace(carga.Nombre))
            {
                return "Escriba el nombre completo de la carga familiar.";
            }

            if (string.IsNullOrWhiteSpace(carga.Parentesco))
            {
                return "Seleccione el parentesco.";
            }

            /* Aqui la fecha SI es obligatoria, al reves que en las certificaciones:
               Talento Humano usa la edad para saber si la carga sigue siendolo, y
               sin fecha ese calculo no existe. */
            if (string.IsNullOrWhiteSpace(carga.FechaNacimiento))
            {
                return "Indique la fecha de nacimiento.";
            }

            DateTime nacimiento;
            bool fechaValida = DateTime.TryParseExact(carga.FechaNacimiento.Trim(),
                                                      "yyyy-MM-dd",
                                                      CultureInfo.InvariantCulture,
                                                      DateTimeStyles.None,
                                                      out nacimiento);
            if (!fechaValida)
            {
                return "La fecha de nacimiento no es válida.";
            }

            if (nacimiento.Date > DateTime.Today)
            {
                return "La fecha de nacimiento no puede estar en el futuro.";
            }

            return "";
        }
```

`NegPerfilCampos.cs` ya tiene `using System;`, `using System.Globalization;`, `using System.Collections.Generic;` y `using CapaEntidad;`. No borres ninguno.

- [ ] **Step 5: Correr las pruebas**

Run:
```
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```
Expected: **56 pruebas, 56 correctas** (las 24 de la fase 1 más las 32 nuevas).

Si el total no es 56, párate y repórtalo en vez de ajustar el número.

- [ ] **Step 6: Commit**

```bash
git add CapaEntidad/EntPerfilEstudio.cs CapaEntidad/EntPerfilCertificacion.cs \
        CapaEntidad/EntPerfilExperiencia.cs CapaEntidad/EntPerfilCargaFamiliar.cs \
        CapaEntidad/CapaEntidad.csproj \
        CapaNegocio/NegPerfilCampos.cs CapaPruebas/NegPerfilCamposTests.cs
git commit -m "test(perfil): validaciones de estudios, certificaciones, experiencia y cargas"
```

Mensaje con asunto, **línea en blanco**, después la línea `Co-Authored-By:`.

---

### Task 2: Script SQL de la fase 2

Ocho procedimientos de escritura y el result set de cargas familiares.

**Files:**
- Create: `docs/sql/2026-09-14-perfil-colaborador-fase2.sql`

**Interfaces:**
- Consumes: las tablas y `Sp_RTA_PerfilColaborador` que dejó la fase 1.
- Produces:
  - `Sp_RTA_PerfilGuardarEstudio @Cod_Usuario, @IdEstudio, @Nivel, @Institucion, @Titulo, @AnioGraduacion, @Ip`
  - `Sp_RTA_PerfilEliminarEstudio @Cod_Usuario, @IdEstudio, @Ip`
  - `Sp_RTA_PerfilGuardarCertificacion @Cod_Usuario, @IdCertificacion, @Nombre, @Entidad, @FechaObtencion, @Ip`
  - `Sp_RTA_PerfilEliminarCertificacion @Cod_Usuario, @IdCertificacion, @Ip`
  - `Sp_RTA_PerfilGuardarExperiencia @Cod_Usuario, @IdExperiencia, @Empresa, @Cargo, @AnioDesde, @AnioHasta, @Funciones, @Ip`
  - `Sp_RTA_PerfilEliminarExperiencia @Cod_Usuario, @IdExperiencia, @Ip`
  - `Sp_RTA_PerfilGuardarCargaFamiliar @Cod_Usuario, @IdCargaFam, @Nombre, @Parentesco, @FechaNacimiento, @Ip`
  - `Sp_RTA_PerfilEliminarCargaFamiliar @Cod_Usuario, @IdCargaFam, @Ip`
  - `Sp_RTA_PerfilColaborador` ampliado con un **octavo** result set: `IdCargaFam, Nombre, Parentesco, FechaNacTexto`

  Los ocho devuelven `Respuestas`: `0` correcto, `-1` el registro no es suyo o no existe, `-2` el `Cod_Usuario` está repetido. Los de alta devuelven además el identificador.

- [ ] **Step 1: Cabecera y procedimientos de estudios**

Crear `docs/sql/2026-09-14-perfil-colaborador-fase2.sql`:

```sql
/* ============================================================================
   Modulo: Perfil del colaborador — FASE 2 (hoja de vida)
   Spec  : docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md
   Previo: docs/sql/2026-09-14-perfil-colaborador.sql (fase 1, ya aplicado)

   1. Estudios        : guardar / eliminar
   2. Certificaciones : guardar / eliminar
   3. Experiencia     : guardar / eliminar
   4. Cargas familiares: guardar / eliminar
   5. Sp_RTA_PerfilColaborador con el octavo result set (cargas familiares)
   6. Aserciones

   Las tablas ya existen: las creo la fase 1. Aqui solo van procedimientos.

   Se puede ejecutar varias veces sin efecto adicional.
   Base: ReporTarea
   ============================================================================ */

SET NOCOUNT ON;
GO

/* Igual que en el script de la fase 1. No se heredan entre archivos: cada uno
   tiene que encenderlos por su cuenta o un indice filtrado -o cualquier objeto
   que los exija- falla con el error 1934. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------------------------------------------------------- 1. estudios --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarEstudio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarEstudio;
GO

/* IdEstudio = 0 es alta; cualquier otro, edicion.

   La guarda @CodigoRepetido es la misma de todos los procedimientos del modulo:
   R_Usuarios.Cod_Usuario NO es unico -la clave primaria real es Id_Usuario- y
   hay dos personas distintas compartiendo un codigo. Cuando eso pasa no se
   entrega ni se escribe nada, antes que adivinar de quien es el dato. Mismo
   criterio que CapaDato/DaoFirmaUsuario.cs.

   El RETURN despues del SELECT no es opcional: un SELECT de retorno NO
   interrumpe la ejecucion en T-SQL, asi que sin el se informaria el bloqueo y
   se escribiria igual.

   El WHERE de la edicion lleva Cod_Usuario ademas del IdEstudio: el id viene
   del cliente y no es de fiar, el Cod_Usuario viene de la sesion y si. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarEstudio
    @Cod_Usuario    VARCHAR(50),
    @IdEstudio      INT,
    @Nivel          VARCHAR(60),
    @Institucion    VARCHAR(200),
    @Titulo         VARCHAR(200),
    @AnioGraduacion SMALLINT,
    @Ip             VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdEstudio = @IdEstudio;
        RETURN;
    END

    IF @IdEstudio = 0
    BEGIN
        INSERT INTO dbo.Perfil_Estudio
            (Cod_Usuario, Nivel, Institucion, Titulo, AnioGraduacion,
             Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nivel, @Institucion, @Titulo, NULLIF(@AnioGraduacion, 0),
                @Cod_Usuario, @Ip);

        SELECT Respuestas = 0, IdEstudio = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Perfil_Estudio
           SET Nivel            = @Nivel,
               Institucion      = @Institucion,
               Titulo           = @Titulo,
               AnioGraduacion   = NULLIF(@AnioGraduacion, 0),
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Cod_Usuario,
               Ip_Modificacion  = @Ip
         WHERE IdEstudio  = @IdEstudio
           AND Cod_Usuario = @Cod_Usuario;

        SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdEstudio  = @IdEstudio;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarEstudio creado.';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarEstudio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarEstudio;
GO

/* Borrado logico. Con Cod_Usuario en el WHERE por la misma razon de arriba:
   sin el, cualquiera borraria el estudio de otra persona mandando su id. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarEstudio
    @Cod_Usuario VARCHAR(50),
    @IdEstudio   INT,
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.Perfil_Estudio
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Cod_Usuario,
           Ip_Modificacion  = @Ip
     WHERE IdEstudio   = @IdEstudio
       AND Cod_Usuario = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarEstudio creado.';
GO
```

- [ ] **Step 2: Procedimientos de certificaciones y experiencia**

Agregar al mismo archivo:

```sql
/* --------------------------------------------------- 2. certificaciones --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarCertificacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarCertificacion;
GO

/* @FechaObtencion llega como texto "yyyy-MM" desde un input type="month". Se
   convierte aqui con estilo 126 (ISO) y agregando el dia 01, que es lo que
   TRY_CONVERT necesita. Si no convierte queda NULL: la validacion de C# ya
   rechazo los formatos raros, esto es la red por si alguien llama al
   procedimiento por fuera. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarCertificacion
    @Cod_Usuario     VARCHAR(50),
    @IdCertificacion INT,
    @Nombre          VARCHAR(200),
    @Entidad         VARCHAR(200),
    @FechaObtencion  VARCHAR(10),
    @Ip              VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdCertificacion = @IdCertificacion;
        RETURN;
    END

    DECLARE @Fecha DATE = TRY_CONVERT(date, NULLIF(LTRIM(RTRIM(@FechaObtencion)),'') + '-01', 126);

    IF @IdCertificacion = 0
    BEGIN
        INSERT INTO dbo.Perfil_Certificacion
            (Cod_Usuario, Nombre, Entidad, FechaObtencion, Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nombre, @Entidad, @Fecha, @Cod_Usuario, @Ip);

        SELECT Respuestas = 0, IdCertificacion = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Perfil_Certificacion
           SET Nombre           = @Nombre,
               Entidad          = @Entidad,
               FechaObtencion   = @Fecha,
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Cod_Usuario,
               Ip_Modificacion  = @Ip
         WHERE IdCertificacion = @IdCertificacion
           AND Cod_Usuario     = @Cod_Usuario;

        SELECT Respuestas      = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdCertificacion = @IdCertificacion;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarCertificacion creado.';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCertificacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarCertificacion;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarCertificacion
    @Cod_Usuario     VARCHAR(50),
    @IdCertificacion INT,
    @Ip              VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.Perfil_Certificacion
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Cod_Usuario,
           Ip_Modificacion  = @Ip
     WHERE IdCertificacion = @IdCertificacion
       AND Cod_Usuario     = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarCertificacion creado.';
GO

/* ------------------------------------------------------ 3. experiencia --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarExperiencia','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarExperiencia;
GO

/* @AnioHasta = 0 significa "sigue ahi" y se guarda como NULL. No es lo mismo
   que "no se sabe": la lectura ordena poniendo los nulos primero, porque el
   empleo actual va arriba en un CV. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarExperiencia
    @Cod_Usuario   VARCHAR(50),
    @IdExperiencia INT,
    @Empresa       VARCHAR(200),
    @Cargo         VARCHAR(200),
    @AnioDesde     SMALLINT,
    @AnioHasta     SMALLINT,
    @Funciones     VARCHAR(MAX),
    @Ip            VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdExperiencia = @IdExperiencia;
        RETURN;
    END

    IF @IdExperiencia = 0
    BEGIN
        INSERT INTO dbo.Perfil_Experiencia
            (Cod_Usuario, Empresa, Cargo, AnioDesde, AnioHasta, Funciones,
             Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Empresa, @Cargo, NULLIF(@AnioDesde,0), NULLIF(@AnioHasta,0),
                @Funciones, @Cod_Usuario, @Ip);

        SELECT Respuestas = 0, IdExperiencia = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Perfil_Experiencia
           SET Empresa          = @Empresa,
               Cargo            = @Cargo,
               AnioDesde        = NULLIF(@AnioDesde,0),
               AnioHasta        = NULLIF(@AnioHasta,0),
               Funciones        = @Funciones,
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Cod_Usuario,
               Ip_Modificacion  = @Ip
         WHERE IdExperiencia = @IdExperiencia
           AND Cod_Usuario   = @Cod_Usuario;

        SELECT Respuestas    = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdExperiencia = @IdExperiencia;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarExperiencia creado.';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarExperiencia','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarExperiencia;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarExperiencia
    @Cod_Usuario   VARCHAR(50),
    @IdExperiencia INT,
    @Ip            VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.Perfil_Experiencia
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Cod_Usuario,
           Ip_Modificacion  = @Ip
     WHERE IdExperiencia = @IdExperiencia
       AND Cod_Usuario   = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarExperiencia creado.';
GO
```

- [ ] **Step 3: Procedimientos de cargas familiares**

Agregar al mismo archivo. **Estos son distintos de los tres anteriores**: la tabla la comparte otra pantalla.

```sql
/* ------------------------------------------------- 4. cargas familiares --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarCargaFamiliar','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarCargaFamiliar;
GO

/* Emp_CargaFamiliar NO es una tabla de este modulo: la comparte
   RRHHEmpleados.aspx. De ahi tres diferencias con los otros procedimientos:

   1. Estado usa 'Activo'/'Inactivo', no '1'/'0'. Es el vocabulario que maneja
      Sp_RTACambiarEstadoCargaFam, el procedimiento de RRHH que alterna el
      estado. Si aqui escribieramos '1', una carga creada desde el perfil no se
      podria desactivar desde RRHH y viceversa.

   2. Ip_Modificacion es varchar(32), mas angosta que el varchar(64) de las
      tablas nuevas. Con una IPv6 este INSERT no truncaria: fallaria con
      "String or binary data would be truncated" y tumbaria el guardado entero.
      Por eso LEFT(@Ip, 32).

   3. Usu_Modificacion es numeric y no admite un Cod_Usuario, asi que no se
      escribe. No es un descuido: no cabe. Queda sin registro de quien lo
      cambio, igual que en Empleados.

   IdEmpleado se deja nulo a proposito. La columna es nullable y 119 de 231
   usuarios no tienen ficha enlazada; sus cargas cuelgan solo de Cod_Usuario. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarCargaFamiliar
    @Cod_Usuario     VARCHAR(50),
    @IdCargaFam      INT,
    @Nombre          VARCHAR(150),
    @Parentesco      VARCHAR(50),
    @FechaNacimiento VARCHAR(10),
    @Ip              VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdCargaFam = @IdCargaFam;
        RETURN;
    END

    DECLARE @Nacimiento DATETIME = TRY_CONVERT(datetime, NULLIF(LTRIM(RTRIM(@FechaNacimiento)),''), 126);

    IF @IdCargaFam = 0
    BEGIN
        INSERT INTO dbo.Emp_CargaFamiliar
            (Cod_Usuario, Nombre, Parentesco, Fecha_nacimiento,
             Estado, Fec_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nombre, @Parentesco, @Nacimiento,
                'Activo', GETDATE(), LEFT(@Ip, 32));

        SELECT Respuestas = 0, IdCargaFam = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Emp_CargaFamiliar
           SET Nombre           = @Nombre,
               Parentesco       = @Parentesco,
               Fecha_nacimiento = @Nacimiento,
               Fec_Modificacion = GETDATE(),
               Ip_Modificacion  = LEFT(@Ip, 32)
         WHERE IdCargaFam  = @IdCargaFam
           AND Cod_Usuario = @Cod_Usuario;

        SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END,
               IdCargaFam = @IdCargaFam;
    END
END
GO
PRINT 'Sp_RTA_PerfilGuardarCargaFamiliar creado.';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCargaFamiliar','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarCargaFamiliar;
GO

/* Escribe 'Inactivo', no '0', para que RRHH lo vea como desactivado. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarCargaFamiliar
    @Cod_Usuario VARCHAR(50),
    @IdCargaFam  INT,
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.Emp_CargaFamiliar
       SET Estado           = 'Inactivo',
           Fec_Modificacion = GETDATE(),
           Ip_Modificacion  = LEFT(@Ip, 32)
     WHERE IdCargaFam  = @IdCargaFam
       AND Cod_Usuario = @Cod_Usuario;

    SELECT Respuestas = CASE WHEN @@ROWCOUNT = 0 THEN -1 ELSE 0 END;
END
GO
PRINT 'Sp_RTA_PerfilEliminarCargaFamiliar creado.';
GO
```

- [ ] **Step 4: Ampliar el procedimiento de lectura con el octavo result set**

Agregar al mismo archivo. **Recrea el procedimiento entero**: hay que copiar los siete conjuntos tal como están hoy en `docs/sql/2026-09-14-perfil-colaborador.sql` (sección 4) y añadir el octavo al final.

Antes de escribirlo, **léelo desde la base** para copiar la versión que realmente corre, no la del archivo por si divergieran:

```
sqlcmd ... -y 0 -Q "SELECT m.definition FROM sys.sql_modules m JOIN sys.procedures p ON p.object_id=m.object_id WHERE p.name='Sp_RTA_PerfilColaborador';"
```

El bloque nuevo, al final, justo antes del `END`:

```sql
    /* 8. cargas familiares

       Va al final y no junto a los otros datos personales a proposito: el Dao
       recorre los result sets POR POSICION, asi que un contrato posicional se
       amplia por el final. Insertarlo en medio desplazaria los conjuntos 4 a 7
       y sus datos aterrizarian en la propiedad equivocada, sin ningun error.

       El filtro es "distinto de Inactivo" y no "igual a Activo" porque
       RRHHEmpleados.aspx inserta sin escribir Estado, dejandolo NULL. Con
       = 'Activo' no veriamos las filas que crea esa pantalla. */
    SELECT IdCargaFam,
           Nombre,
           Parentesco,
           FechaNacTexto = CONVERT(VARCHAR(10), Fecha_nacimiento, 23)
      FROM dbo.Emp_CargaFamiliar
     WHERE Cod_Usuario = @Cod_Usuario
       AND ISNULL(Estado,'') <> 'Inactivo'
       AND @CodigoRepetido = 0
     ORDER BY Fecha_nacimiento DESC, IdCargaFam;
```

`CONVERT(..., 23)` produce `yyyy-MM-dd`, que es justo lo que un `input type="date"` necesita y lo que espera `ValidarCargaFamiliar`.

- [ ] **Step 5: Aserciones**

Agregar al final del archivo:

```sql
/* ------------------------------------------------------- 6. aserciones --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarEstudio','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarEstudio no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarEstudio','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarEstudio no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarCertificacion','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarCertificacion no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCertificacion','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarCertificacion no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarExperiencia','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarExperiencia no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarExperiencia','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarExperiencia no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarCargaFamiliar','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarCargaFamiliar no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarCargaFamiliar','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarCargaFamiliar no quedo creado.', 16, 1);

/* El octavo result set tiene que estar: el Dao lo lee por posicion y si
   faltara leeria nulos sin quejarse. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilColaborador'
                 AND m.definition LIKE '%8. cargas familiares%')
    RAISERROR('FALLO: Sp_RTA_PerfilColaborador no tiene el octavo result set.', 16, 1);

PRINT 'Aserciones de la fase 2 OK.';
GO

PRINT 'Script 2026-09-14-perfil-colaborador-fase2 completado.';
GO
```

- [ ] **Step 6: Avisar — no ejecutar**

**No ejecutes el script.** El clasificador de permisos bloquea `sqlcmd` desde un subagente. Avisa al coordinador, que lo ejecuta contra `ReporTarea` (producción, autorizado), corre las dos pasadas de idempotencia y te devuelve la salida.

**No commitees hasta que el script se haya ejecutado con éxito.**

- [ ] **Step 7: Commit, tras la confirmación del coordinador**

```bash
git add docs/sql/2026-09-14-perfil-colaborador-fase2.sql
git commit -m "feat(perfil): procedimientos de la hoja de vida y cargas familiares"
```

---

### Task 3: Lectura de las cuatro listas

**Files:**
- Modify: `CapaEntidad/EntPerfilCompleto.cs`
- Modify: `CapaDato/DaoPerfil.cs`
- Modify: `CapaNegocio/NegPerfil.cs`

**Interfaces:**
- Consumes: las cuatro entidades de la Task 1; el octavo result set de la Task 2.
- Produces: `EntPerfilCompleto` con `List<EntPerfilEstudio> Estudios`, `List<EntPerfilCertificacion> Certificaciones`, `List<EntPerfilExperiencia> Experiencia`, `List<EntPerfilCargaFamiliar> CargasFamiliares`, todas inicializadas en el constructor.

- [ ] **Step 1: Ampliar el contenedor**

En `CapaEntidad/EntPerfilCompleto.cs`, agregar las cuatro propiedades y su inicialización:

```csharp
        public List<EntPerfilEstudio> Estudios { get; set; }
        public List<EntPerfilCertificacion> Certificaciones { get; set; }
        public List<EntPerfilExperiencia> Experiencia { get; set; }

        /// <summary>
        /// Cuelgan de Cod_Usuario, no de la ficha de empleado: los 119 usuarios
        /// sin ficha tambien las registran.
        /// </summary>
        public List<EntPerfilCargaFamiliar> CargasFamiliares { get; set; }
```

Y en el constructor, junto a las que ya están:

```csharp
            Estudios = new List<EntPerfilEstudio>();
            Certificaciones = new List<EntPerfilCertificacion>();
            Experiencia = new List<EntPerfilExperiencia>();
            CargasFamiliares = new List<EntPerfilCargaFamiliar>();
```

Una lista nula que llegue a la pantalla es una excepción en tiempo de ejecución, no un dato vacío. Por eso se inicializan aquí y no al leer.

- [ ] **Step 2: Leer los conjuntos 4 a 8 en el Dao**

En `CapaDato/DaoPerfil.cs`, dentro de `CargarPerfil`, **después** del bloque que lee los contactos de emergencia (conjunto 3) y **antes** del cierre del `using`:

```csharp
                    /* 4. estudios */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Estudios.Add(new EntPerfilEstudio
                            {
                                IdEstudio      = EnteroDe(dr, "IdEstudio"),
                                Nivel          = Texto(dr, "Nivel"),
                                Institucion    = Texto(dr, "Institucion"),
                                Titulo         = Texto(dr, "Titulo"),
                                AnioGraduacion = EnteroNuloDe(dr, "AnioGraduacion")
                            });
                        }
                    }

                    /* 5. certificaciones */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Certificaciones.Add(new EntPerfilCertificacion
                            {
                                IdCertificacion = EnteroDe(dr, "IdCertificacion"),
                                Nombre          = Texto(dr, "Nombre"),
                                Entidad         = Texto(dr, "Entidad"),
                                /* La fecha viaja como "yyyy-MM" para que el input
                                   type="month" del navegador la reciba tal cual. */
                                FechaObtencion  = FechaMesDe(dr, "FechaObtencion")
                            });
                        }
                    }

                    /* 6. experiencia */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Experiencia.Add(new EntPerfilExperiencia
                            {
                                IdExperiencia = EnteroDe(dr, "IdExperiencia"),
                                Empresa       = Texto(dr, "Empresa"),
                                Cargo         = Texto(dr, "Cargo"),
                                AnioDesde     = EnteroNuloDe(dr, "AnioDesde"),
                                AnioHasta     = EnteroNuloDe(dr, "AnioHasta"),
                                Funciones     = Texto(dr, "Funciones")
                            });
                        }
                    }

                    /* 7. documentos de respaldo: son de la fase 3. Se salta el
                       conjunto sin leerlo, pero HAY que saltarlo: el Dao avanza
                       por posicion y sin este NextResult las cargas familiares
                       se leerian de la lista de documentos. */
                    dr.NextResult();

                    /* 8. cargas familiares */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.CargasFamiliares.Add(new EntPerfilCargaFamiliar
                            {
                                IdCargaFam      = EnteroDe(dr, "IdCargaFam"),
                                Nombre          = Texto(dr, "Nombre"),
                                Parentesco      = Texto(dr, "Parentesco"),
                                FechaNacimiento = Texto(dr, "FechaNacTexto")
                            });
                        }
                    }
```

Y agregar estos tres ayudantes privados junto al `Texto` que ya existe:

```csharp
        /// <summary>Entero de una columna que el esquema declara NOT NULL.</summary>
        private static int EnteroDe(SqlDataReader dr, string columna)
        {
            return dr[columna] == System.DBNull.Value ? 0 : Convert.ToInt32(dr[columna]);
        }

        /// <summary>
        /// Entero de una columna que si puede venir nula. Devuelve null y no cero:
        /// para un anio, el cero se leeria como un dato real.
        /// </summary>
        private static int? EnteroNuloDe(SqlDataReader dr, string columna)
        {
            if (dr[columna] == System.DBNull.Value) { return null; }
            return Convert.ToInt32(dr[columna]);
        }

        /// <summary>
        /// Una fecha como texto "yyyy-MM", que es lo que consume un input
        /// type="month". Cadena vacia si la columna viene nula.
        /// </summary>
        private static string FechaMesDe(SqlDataReader dr, string columna)
        {
            if (dr[columna] == System.DBNull.Value) { return ""; }
            return Convert.ToDateTime(dr[columna]).ToString("yyyy-MM",
                       System.Globalization.CultureInfo.InvariantCulture);
        }
```

El `dr.NextResult()` suelto del conjunto 7 es el punto delicado de esta tarea: sin él, las cargas familiares se leerían de la lista de documentos y las columnas no coincidirían.

- [ ] **Step 3: Compilar y correr las pruebas**

Run:
```
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```
Expected: `0 Error(s)` y **56 de 56** pruebas correctas.

- [ ] **Step 4: Commit**

```bash
git add CapaEntidad/EntPerfilCompleto.cs CapaDato/DaoPerfil.cs CapaNegocio/NegPerfil.cs
git commit -m "feat(perfil): la lectura trae estudios, certificaciones, experiencia y cargas"
```

---

### Task 4: Escritura y acciones del handler

**Files:**
- Modify: `CapaDato/DaoPerfil.cs`
- Modify: `CapaNegocio/NegPerfil.cs`
- Modify: `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`

**Interfaces:**
- Consumes: las validaciones de la Task 1, los procedimientos de la Task 2.
- Produces: ocho acciones en el handler — `GuardarEstudio`, `EliminarEstudio`, `GuardarCertificacion`, `EliminarCertificacion`, `GuardarExperiencia`, `EliminarExperiencia`, `GuardarCargaFamiliar`, `EliminarCargaFamiliar`.

- [ ] **Step 1: Métodos de escritura en el Dao**

Agregar a `CapaDato/DaoPerfil.cs`. Los ocho siguen el mismo molde que `GuardarEmergencia`, que ya está en el archivo: parámetros tipados con `SqlDbType` explícito, y el `-2` traducido antes que el `0`.

```csharp
        /// <summary>
        /// Traduce el codigo que devuelven los procedimientos del modulo.
        /// 0 correcto, -1 el registro no es suyo o no existe, -2 su Cod_Usuario
        /// esta repetido y no se puede saber de quien seria el dato.
        /// </summary>
        private static EntRespuesta RespuestaDe(int resultado, string mensajeExito, string mensajeNoEncontrado)
        {
            EntRespuesta respuesta = new EntRespuesta();

            if (resultado == -2)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.";
                respuesta.tipoMensaje = "warning";
            }
            else if (resultado == 0)
            {
                respuesta.estado = "1";
                respuesta.mensaje = mensajeExito;
                respuesta.tipoMensaje = "success";
            }
            else
            {
                respuesta.estado = "0";
                respuesta.mensaje = mensajeNoEncontrado;
                respuesta.tipoMensaje = "warning";
            }

            return respuesta;
        }

        /// <summary>Ejecuta un procedimiento de escritura y devuelve su Respuestas.</summary>
        private static int EjecutarEscritura(string procedimiento, Action<SqlCommand> ponerParametros)
        {
            int resultado = -1;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand(procedimiento, cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                ponerParametros(cmd);
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { resultado = Convert.ToInt32(dr["Respuestas"]); }
                }
            }

            return resultado;
        }

        public static EntRespuesta GuardarEstudio(string codUsuario, EntPerfilEstudio e, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarEstudio", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",    SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdEstudio",      SqlDbType.Int).Value          = e.IdEstudio;
                cmd.Parameters.Add("@Nivel",          SqlDbType.VarChar,  60).Value = e.Nivel;
                cmd.Parameters.Add("@Institucion",    SqlDbType.VarChar, 200).Value = e.Institucion;
                cmd.Parameters.Add("@Titulo",         SqlDbType.VarChar, 200).Value = e.Titulo;
                cmd.Parameters.Add("@AnioGraduacion", SqlDbType.SmallInt).Value     = e.AnioGraduacion ?? 0;
                cmd.Parameters.Add("@Ip",             SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Estudio guardado.", "No se encontró ese estudio.");
        }

        public static EntRespuesta EliminarEstudio(string codUsuario, int idEstudio, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarEstudio", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdEstudio",   SqlDbType.Int).Value         = idEstudio;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Estudio eliminado.", "No se encontró ese estudio.");
        }

        public static EntRespuesta GuardarCertificacion(string codUsuario, EntPerfilCertificacion c, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarCertificacion", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",     SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdCertificacion", SqlDbType.Int).Value          = c.IdCertificacion;
                cmd.Parameters.Add("@Nombre",          SqlDbType.VarChar, 200).Value = c.Nombre;
                cmd.Parameters.Add("@Entidad",         SqlDbType.VarChar, 200).Value = c.Entidad;
                cmd.Parameters.Add("@FechaObtencion",  SqlDbType.VarChar,  10).Value = c.FechaObtencion ?? "";
                cmd.Parameters.Add("@Ip",              SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Certificación guardada.", "No se encontró esa certificación.");
        }

        public static EntRespuesta EliminarCertificacion(string codUsuario, int idCertificacion, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarCertificacion", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",     SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdCertificacion", SqlDbType.Int).Value         = idCertificacion;
                cmd.Parameters.Add("@Ip",              SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Certificación eliminada.", "No se encontró esa certificación.");
        }

        public static EntRespuesta GuardarExperiencia(string codUsuario, EntPerfilExperiencia x, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarExperiencia", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",   SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdExperiencia", SqlDbType.Int).Value          = x.IdExperiencia;
                cmd.Parameters.Add("@Empresa",       SqlDbType.VarChar, 200).Value = x.Empresa;
                cmd.Parameters.Add("@Cargo",         SqlDbType.VarChar, 200).Value = x.Cargo;
                cmd.Parameters.Add("@AnioDesde",     SqlDbType.SmallInt).Value     = x.AnioDesde ?? 0;
                cmd.Parameters.Add("@AnioHasta",     SqlDbType.SmallInt).Value     = x.AnioHasta ?? 0;
                cmd.Parameters.Add("@Funciones",     SqlDbType.VarChar, -1).Value  = x.Funciones ?? "";
                cmd.Parameters.Add("@Ip",            SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Experiencia guardada.", "No se encontró esa experiencia.");
        }

        public static EntRespuesta EliminarExperiencia(string codUsuario, int idExperiencia, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarExperiencia", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",   SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdExperiencia", SqlDbType.Int).Value         = idExperiencia;
                cmd.Parameters.Add("@Ip",            SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Experiencia eliminada.", "No se encontró esa experiencia.");
        }

        public static EntRespuesta GuardarCargaFamiliar(string codUsuario, EntPerfilCargaFamiliar c, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarCargaFamiliar", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",     SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdCargaFam",      SqlDbType.Int).Value          = c.IdCargaFam;
                cmd.Parameters.Add("@Nombre",          SqlDbType.VarChar, 150).Value = c.Nombre;
                cmd.Parameters.Add("@Parentesco",      SqlDbType.VarChar,  50).Value = c.Parentesco;
                cmd.Parameters.Add("@FechaNacimiento", SqlDbType.VarChar,  10).Value = c.FechaNacimiento ?? "";
                cmd.Parameters.Add("@Ip",              SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Carga familiar guardada.", "No se encontró esa carga familiar.");
        }

        public static EntRespuesta EliminarCargaFamiliar(string codUsuario, int idCargaFam, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarCargaFamiliar", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdCargaFam",  SqlDbType.Int).Value         = idCargaFam;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Carga familiar eliminada.", "No se encontró esa carga familiar.");
        }
```

`DaoPerfil.cs` necesita `using System;` para `Action<>` y `Convert`. Ya lo tiene; confírmalo antes de compilar.

- [ ] **Step 2: Fachada de negocio**

Agregar a `CapaNegocio/NegPerfil.cs`:

```csharp
        public static EntRespuesta GuardarEstudio(string codUsuario, EntPerfilEstudio e, string ip)
        {
            return DaoPerfil.GuardarEstudio(codUsuario, e, ip);
        }

        public static EntRespuesta EliminarEstudio(string codUsuario, int idEstudio, string ip)
        {
            return DaoPerfil.EliminarEstudio(codUsuario, idEstudio, ip);
        }

        public static EntRespuesta GuardarCertificacion(string codUsuario, EntPerfilCertificacion c, string ip)
        {
            return DaoPerfil.GuardarCertificacion(codUsuario, c, ip);
        }

        public static EntRespuesta EliminarCertificacion(string codUsuario, int idCertificacion, string ip)
        {
            return DaoPerfil.EliminarCertificacion(codUsuario, idCertificacion, ip);
        }

        public static EntRespuesta GuardarExperiencia(string codUsuario, EntPerfilExperiencia x, string ip)
        {
            return DaoPerfil.GuardarExperiencia(codUsuario, x, ip);
        }

        public static EntRespuesta EliminarExperiencia(string codUsuario, int idExperiencia, string ip)
        {
            return DaoPerfil.EliminarExperiencia(codUsuario, idExperiencia, ip);
        }

        public static EntRespuesta GuardarCargaFamiliar(string codUsuario, EntPerfilCargaFamiliar c, string ip)
        {
            return DaoPerfil.GuardarCargaFamiliar(codUsuario, c, ip);
        }

        public static EntRespuesta EliminarCargaFamiliar(string codUsuario, int idCargaFam, string ip)
        {
            return DaoPerfil.EliminarCargaFamiliar(codUsuario, idCargaFam, ip);
        }
```

- [ ] **Step 3: Las ocho acciones del handler**

En `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`, dentro de `ProcessRequest`, junto a las cuatro acciones que ya existen:

```csharp
                if (Action == "GuardarEstudio")
                {
                    existAction = true;
                    responseAction.Append(GuardarEstudio(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarEstudio")
                {
                    existAction = true;
                    responseAction.Append(EliminarEstudio(context, parametros[0]["parameters"]));
                }

                if (Action == "GuardarCertificacion")
                {
                    existAction = true;
                    responseAction.Append(GuardarCertificacion(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarCertificacion")
                {
                    existAction = true;
                    responseAction.Append(EliminarCertificacion(context, parametros[0]["parameters"]));
                }

                if (Action == "GuardarExperiencia")
                {
                    existAction = true;
                    responseAction.Append(GuardarExperiencia(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarExperiencia")
                {
                    existAction = true;
                    responseAction.Append(EliminarExperiencia(context, parametros[0]["parameters"]));
                }

                if (Action == "GuardarCargaFamiliar")
                {
                    existAction = true;
                    responseAction.Append(GuardarCargaFamiliar(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarCargaFamiliar")
                {
                    existAction = true;
                    responseAction.Append(EliminarCargaFamiliar(context, parametros[0]["parameters"]));
                }
```

Y los ocho métodos, junto a los que ya están:

```csharp
        /// <summary>
        /// Las ocho escrituras de la hoja de vida comparten la misma forma:
        /// identidad de la sesion, validacion en el servidor, y recien entonces
        /// la base. La validacion va aqui y no solo en el navegador porque este
        /// handler es alcanzable por HTTP directo.
        /// </summary>
        private string GuardarEstudio(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilEstudio estudio = new EntPerfilEstudio
                {
                    IdEstudio      = Convert.ToInt32(Texto(campos, "idEstudio", "0")),
                    Nivel          = Texto(campos, "nivel", ""),
                    Institucion    = Texto(campos, "institucion", ""),
                    Titulo         = Texto(campos, "titulo", ""),
                    AnioGraduacion = EnteroNuloDelPayload(campos, "anioGraduacion")
                };

                string error = NegPerfilCampos.ValidarEstudio(estudio);
                if (error != "") { return responseMessage("0", error, "warning"); }

                return ToJson(NegPerfil.GuardarEstudio(codUsuario, estudio, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar el estudio. " + ex.Message, "danger");
            }
        }

        private string EliminarEstudio(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int id = Convert.ToInt32(Texto(campos, "idEstudio", "0"));
                return ToJson(NegPerfil.EliminarEstudio(codUsuario, id, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar el estudio. " + ex.Message, "danger");
            }
        }

        private string GuardarCertificacion(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilCertificacion cert = new EntPerfilCertificacion
                {
                    IdCertificacion = Convert.ToInt32(Texto(campos, "idCertificacion", "0")),
                    Nombre          = Texto(campos, "nombre", ""),
                    Entidad         = Texto(campos, "entidad", ""),
                    FechaObtencion  = Texto(campos, "fechaObtencion", "")
                };

                string error = NegPerfilCampos.ValidarCertificacion(cert);
                if (error != "") { return responseMessage("0", error, "warning"); }

                return ToJson(NegPerfil.GuardarCertificacion(codUsuario, cert, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar la certificación. " + ex.Message, "danger");
            }
        }

        private string EliminarCertificacion(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int id = Convert.ToInt32(Texto(campos, "idCertificacion", "0"));
                return ToJson(NegPerfil.EliminarCertificacion(codUsuario, id, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar la certificación. " + ex.Message, "danger");
            }
        }

        private string GuardarExperiencia(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilExperiencia exp = new EntPerfilExperiencia
                {
                    IdExperiencia = Convert.ToInt32(Texto(campos, "idExperiencia", "0")),
                    Empresa       = Texto(campos, "empresa", ""),
                    Cargo         = Texto(campos, "cargo", ""),
                    AnioDesde     = EnteroNuloDelPayload(campos, "anioDesde"),
                    AnioHasta     = EnteroNuloDelPayload(campos, "anioHasta"),
                    Funciones     = Texto(campos, "funciones", "")
                };

                string error = NegPerfilCampos.ValidarExperiencia(exp);
                if (error != "") { return responseMessage("0", error, "warning"); }

                return ToJson(NegPerfil.GuardarExperiencia(codUsuario, exp, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar la experiencia. " + ex.Message, "danger");
            }
        }

        private string EliminarExperiencia(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int id = Convert.ToInt32(Texto(campos, "idExperiencia", "0"));
                return ToJson(NegPerfil.EliminarExperiencia(codUsuario, id, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar la experiencia. " + ex.Message, "danger");
            }
        }

        private string GuardarCargaFamiliar(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilCargaFamiliar carga = new EntPerfilCargaFamiliar
                {
                    IdCargaFam      = Convert.ToInt32(Texto(campos, "idCargaFam", "0")),
                    Nombre          = Texto(campos, "nombre", ""),
                    Parentesco      = Texto(campos, "parentesco", ""),
                    FechaNacimiento = Texto(campos, "fechaNacimiento", "")
                };

                string error = NegPerfilCampos.ValidarCargaFamiliar(carga);
                if (error != "") { return responseMessage("0", error, "warning"); }

                return ToJson(NegPerfil.GuardarCargaFamiliar(codUsuario, carga, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar la carga familiar. " + ex.Message, "danger");
            }
        }

        private string EliminarCargaFamiliar(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int id = Convert.ToInt32(Texto(campos, "idCargaFam", "0"));
                return ToJson(NegPerfil.EliminarCargaFamiliar(codUsuario, id, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar la carga familiar. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Un entero opcional del payload. Devuelve null cuando la clave no vino,
        /// vino vacia o no es un numero: para un anio, el cero significaria "anio
        /// cero" y no "no lo se".
        /// </summary>
        private static int? EnteroNuloDelPayload(dynamic campos, string clave)
        {
            string texto = Texto(campos, clave, "");
            if (texto == "") { return null; }

            int valor;
            if (!int.TryParse(texto, out valor)) { return null; }

            return valor;
        }
```

El método `Texto(dynamic, string, string)` y `CodUsuarioSesion(HttpContext)` ya existen en este archivo desde la fase 1. No los dupliques.

- [ ] **Step 4: Compilar y correr las pruebas**

Run:
```
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```
Expected: `0 Error(s)` y **56 de 56**.

- [ ] **Step 5: Commit**

```bash
git add CapaDato/DaoPerfil.cs CapaNegocio/NegPerfil.cs ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
git commit -m "feat(perfil): guardar y eliminar estudios, certificaciones, experiencia y cargas"
```

---

### Task 5: Pestaña Formación

Estudios y certificaciones comparten pestaña, en dos tarjetas.

**Files:**
- Modify: `ReporteTareas/Formulario/MiPerfil.aspx`
- Modify: `ReporteTareas/js/miPerfil.js`

**Interfaces:**
- Consumes: las acciones `GuardarEstudio`, `EliminarEstudio`, `GuardarCertificacion`, `EliminarCertificacion` de la Task 4; `PostPerfil`, `MostrarMensaje`, `_perfil` de la fase 1.
- Produces: funciones `PintarEstudios(lista)`, `AgregarEstudio()`, `EliminarEstudio(id)`, `PintarCertificaciones(lista)`, `AgregarCertificacion()`, `EliminarCertificacion(id)`.

- [ ] **Step 1: Añadir la pestaña al marcado**

En `ReporteTareas/Formulario/MiPerfil.aspx`, agregar a la lista `nav nav-tabs`, después de la pestaña de Emergencia:

```aspx
                    <li><a href="#tabFormacion" data-toggle="tab"><i class="fa fa-graduation-cap"></i> Formación</a></li>
```

Y el panel, dentro de `tab-content`, después del de Emergencia:

```aspx
                    <div class="tab-pane" id="tabFormacion">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Estudios
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">Formación académica formal.</p>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover" style="font-size: 90%">
                                        <thead class="bg-primary">
                                            <tr><th>Nivel</th><th>Institución</th><th>Título</th><th style="width:80px">Año</th><th style="width:70px">Quitar</th></tr>
                                        </thead>
                                        <tbody id="cuerpoEstudios"></tbody>
                                    </table>
                                </div>
                                <hr />
                                <div class="row">
                                    <div class="form-group col-lg-3"><label>Nivel</label>
                                        <select class="form-control" id="esNivel">
                                            <option value="">Seleccione…</option>
                                            <option>Bachillerato</option><option>Tercer nivel</option>
                                            <option>Cuarto nivel (Maestría)</option><option>Doctorado</option>
                                        </select></div>
                                    <div class="form-group col-lg-3"><label>Institución</label>
                                        <input type="text" class="form-control" id="esInstitucion" maxlength="200" /></div>
                                    <div class="form-group col-lg-3"><label>Título obtenido</label>
                                        <input type="text" class="form-control" id="esTitulo" maxlength="200" /></div>
                                    <div class="form-group col-lg-1"><label>Año</label>
                                        <input type="number" class="form-control" id="esAnio" min="1940" max="2100" /></div>
                                    <div class="form-group col-lg-2" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary btn-block" onclick="AgregarEstudio()">
                                            <i class="fa fa-plus"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Certificaciones
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">Cursos y certificaciones.</p>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover" style="font-size: 90%">
                                        <thead class="bg-primary">
                                            <tr><th>Nombre</th><th>Entidad emisora</th><th style="width:120px">Obtenida</th><th style="width:70px">Quitar</th></tr>
                                        </thead>
                                        <tbody id="cuerpoCertificaciones"></tbody>
                                    </table>
                                </div>
                                <hr />
                                <div class="row">
                                    <div class="form-group col-lg-4"><label>Nombre de la certificación</label>
                                        <input type="text" class="form-control" id="ceNombre" maxlength="200" /></div>
                                    <div class="form-group col-lg-3"><label>Entidad emisora</label>
                                        <input type="text" class="form-control" id="ceEntidad" maxlength="200" /></div>
                                    <div class="form-group col-lg-3"><label>Fecha de obtención</label>
                                        <input type="month" class="form-control" id="ceFecha" /></div>
                                    <div class="form-group col-lg-2" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary btn-block" onclick="AgregarCertificacion()">
                                            <i class="fa fa-plus"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
```

- [ ] **Step 2: Render y guardado en el JavaScript**

Agregar a `ReporteTareas/js/miPerfil.js`, y llamar `PintarEstudios(respuesta.Estudios);` y `PintarCertificaciones(respuesta.Certificaciones);` dentro de `CargarPerfil`, junto a las que ya están:

```javascript
/* Los nombres de instituciones y titulos los teclea el propio usuario, asi que
   todo entra con .text(). Nunca concatenando HTML. */
function PintarEstudios(lista) {
    var $cuerpo = $("#cuerpoEstudios").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="5" class="text-center text-muted">' +
                       'Todavía no ha registrado ningún estudio.</td></tr>');
        return;
    }

    $.each(lista, function (i, e) {
        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(e.Nivel));
        $fila.append($("<td></td>").text(e.Institucion));
        $fila.append($("<td></td>").text(e.Titulo));
        $fila.append($("<td></td>").text(e.AnioGraduacion === null ? "–" : e.AnioGraduacion));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarEstudio(' + e.IdEstudio + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarEstudio() {
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.", "warning");
        return;
    }

    var datos = {
        idEstudio:      0,
        nivel:          $("#esNivel").val(),
        institucion:    $("#esInstitucion").val(),
        titulo:         $("#esTitulo").val(),
        anioGraduacion: $("#esAnio").val()
    };

    PostPerfil("GuardarEstudio", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#esNivel").val("");
            $("#esInstitucion, #esTitulo, #esAnio").val("");
            CargarPerfil();
        }
    });
}

function EliminarEstudio(idEstudio) {
    PostPerfil("EliminarEstudio", { idEstudio: idEstudio }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}

function PintarCertificaciones(lista) {
    var $cuerpo = $("#cuerpoCertificaciones").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="4" class="text-center text-muted">' +
                       'Todavía no ha registrado ninguna certificación.</td></tr>');
        return;
    }

    $.each(lista, function (i, c) {
        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(c.Nombre));
        $fila.append($("<td></td>").text(c.Entidad));
        $fila.append($("<td></td>").text(c.FechaObtencion === "" ? "–" : c.FechaObtencion));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarCertificacion(' + c.IdCertificacion + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarCertificacion() {
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.", "warning");
        return;
    }

    var datos = {
        idCertificacion: 0,
        nombre:          $("#ceNombre").val(),
        entidad:         $("#ceEntidad").val(),
        fechaObtencion:  $("#ceFecha").val()
    };

    PostPerfil("GuardarCertificacion", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#ceNombre, #ceEntidad, #ceFecha").val("");
            CargarPerfil();
        }
    });
}

function EliminarCertificacion(idCertificacion) {
    PostPerfil("EliminarCertificacion", { idCertificacion: idCertificacion }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}
```

- [ ] **Step 3: Compilar y verificar los símbolos**

Run:
```
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
```
Expected: `0 Error(s)`.

Y haz esta comprobación, que es la que faltó en la fase 1 y dejó pasar un defecto crítico: **recorre `miPerfil.js` y lista cada función global que invoca**, confirmando que cada una está definida en ese mismo archivo o en uno que `MiPerfil.aspx` carga. Comprueba también que **cada `id` que el JavaScript busca existe en el marcado**, escrito igual. Un `id` mal escrito no da error: el campo se queda vacío y nadie lo nota. Pon ambas listas en tu informe.

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/Formulario/MiPerfil.aspx ReporteTareas/js/miPerfil.js
git commit -m "feat(perfil): pestana de formacion con estudios y certificaciones"
```

---

### Task 6: Pestañas Experiencia y Cargas familiares

**Files:**
- Modify: `ReporteTareas/Formulario/MiPerfil.aspx`
- Modify: `ReporteTareas/js/miPerfil.js`

**Interfaces:**
- Consumes: `GuardarExperiencia`, `EliminarExperiencia`, `GuardarCargaFamiliar`, `EliminarCargaFamiliar` de la Task 4.
- Produces: `PintarExperiencia(lista)`, `AgregarExperiencia()`, `EliminarExperiencia(id)`, `PintarCargasFamiliares(lista)`, `AgregarCargaFamiliar()`, `EliminarCargaFamiliar(id)`.

- [ ] **Step 1: Las dos pestañas en el marcado**

Agregar a la lista `nav nav-tabs`, después de Formación:

```aspx
                    <li><a href="#tabExperiencia" data-toggle="tab"><i class="fa fa-briefcase"></i> Experiencia</a></li>
                    <li><a href="#tabCargas" data-toggle="tab"><i class="fa fa-users"></i> Cargas familiares</a></li>
```

Y los dos paneles, dentro de `tab-content`:

```aspx
                    <div class="tab-pane" id="tabExperiencia">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Experiencia laboral
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">
                                    Deje el año de fin vacío si sigue trabajando ahí.
                                </p>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover" style="font-size: 90%">
                                        <thead class="bg-primary">
                                            <tr><th>Empresa</th><th>Cargo</th><th style="width:110px">Período</th><th>Funciones</th><th style="width:70px">Quitar</th></tr>
                                        </thead>
                                        <tbody id="cuerpoExperiencia"></tbody>
                                    </table>
                                </div>
                                <hr />
                                <div class="row">
                                    <div class="form-group col-lg-3"><label>Empresa</label>
                                        <input type="text" class="form-control" id="exEmpresa" maxlength="200" /></div>
                                    <div class="form-group col-lg-3"><label>Cargo</label>
                                        <input type="text" class="form-control" id="exCargo" maxlength="200" /></div>
                                    <div class="form-group col-lg-2"><label>Desde (año)</label>
                                        <input type="number" class="form-control" id="exDesde" min="1940" max="2100" /></div>
                                    <div class="form-group col-lg-2"><label>Hasta (año)</label>
                                        <input type="number" class="form-control" id="exHasta" min="1940" max="2100" placeholder="Actual" /></div>
                                    <div class="form-group col-lg-2" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary btn-block" onclick="AgregarExperiencia()">
                                            <i class="fa fa-plus"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-12"><label>Funciones principales</label>
                                        <textarea class="form-control" id="exFunciones" rows="2"></textarea></div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tab-pane" id="tabCargas">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Cargas familiares
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">
                                    Las personas que dependen económicamente de usted.
                                </p>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover" style="font-size: 90%">
                                        <thead class="bg-primary">
                                            <tr><th>Nombre</th><th style="width:130px">Parentesco</th><th style="width:140px">Fecha de nacimiento</th><th style="width:70px">Quitar</th></tr>
                                        </thead>
                                        <tbody id="cuerpoCargas"></tbody>
                                    </table>
                                </div>
                                <hr />
                                <div class="row">
                                    <div class="form-group col-lg-5"><label>Nombre completo</label>
                                        <input type="text" class="form-control" id="cfNombre" maxlength="150" /></div>
                                    <div class="form-group col-lg-3"><label>Parentesco</label>
                                        <select class="form-control" id="cfParentesco">
                                            <option value="">Seleccione…</option>
                                            <option>Hijo/a</option><option>Cónyuge</option>
                                            <option>Padre</option><option>Madre</option><option>Otro</option>
                                        </select></div>
                                    <div class="form-group col-lg-2"><label>Fecha de nacimiento</label>
                                        <input type="date" class="form-control" id="cfFecha" /></div>
                                    <div class="form-group col-lg-2" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary btn-block" onclick="AgregarCargaFamiliar()">
                                            <i class="fa fa-plus"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
```

- [ ] **Step 2: Render y guardado en el JavaScript**

Agregar a `miPerfil.js`, y llamar `PintarExperiencia(respuesta.Experiencia);` y `PintarCargasFamiliares(respuesta.CargasFamiliares);` dentro de `CargarPerfil`:

```javascript
function PintarExperiencia(lista) {
    var $cuerpo = $("#cuerpoExperiencia").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="5" class="text-center text-muted">' +
                       'Todavía no ha registrado experiencia laboral.</td></tr>');
        return;
    }

    $.each(lista, function (i, x) {
        /* AnioHasta nulo significa "sigue ahi", no "no se sabe". */
        var periodo = x.AnioDesde + " – " + (x.AnioHasta === null ? "Actual" : x.AnioHasta);

        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(x.Empresa));
        $fila.append($("<td></td>").text(x.Cargo));
        $fila.append($("<td></td>").text(periodo));
        $fila.append($("<td></td>").text(x.Funciones));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarExperiencia(' + x.IdExperiencia + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarExperiencia() {
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.", "warning");
        return;
    }

    var datos = {
        idExperiencia: 0,
        empresa:       $("#exEmpresa").val(),
        cargo:         $("#exCargo").val(),
        anioDesde:     $("#exDesde").val(),
        anioHasta:     $("#exHasta").val(),
        funciones:     $("#exFunciones").val()
    };

    PostPerfil("GuardarExperiencia", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#exEmpresa, #exCargo, #exDesde, #exHasta, #exFunciones").val("");
            CargarPerfil();
        }
    });
}

function EliminarExperiencia(idExperiencia) {
    PostPerfil("EliminarExperiencia", { idExperiencia: idExperiencia }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}

function PintarCargasFamiliares(lista) {
    var $cuerpo = $("#cuerpoCargas").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="4" class="text-center text-muted">' +
                       'Todavía no ha registrado ninguna carga familiar.</td></tr>');
        return;
    }

    $.each(lista, function (i, c) {
        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(c.Nombre));
        $fila.append($("<td></td>").text(c.Parentesco));
        $fila.append($("<td></td>").text(c.FechaNacimiento));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarCargaFamiliar(' + c.IdCargaFam + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarCargaFamiliar() {
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.", "warning");
        return;
    }

    var datos = {
        idCargaFam:      0,
        nombre:          $("#cfNombre").val(),
        parentesco:      $("#cfParentesco").val(),
        fechaNacimiento: $("#cfFecha").val()
    };

    PostPerfil("GuardarCargaFamiliar", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#cfNombre, #cfFecha").val("");
            $("#cfParentesco").val("");
            CargarPerfil();
        }
    });
}

function EliminarCargaFamiliar(idCargaFam) {
    PostPerfil("EliminarCargaFamiliar", { idCargaFam: idCargaFam }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}
```

- [ ] **Step 3: Compilar y verificar los símbolos y los `id`**

Run:
```
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
```
Expected: `0 Error(s)`.

Repite las dos listas del informe: **cada símbolo global que invoca `miPerfil.js`** y **cada `id` que busca**, comprobando que existen. No la des por hecha porque la hiciste en la tarea anterior: esta tarea añadió catorce `id` nuevos.

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/Formulario/MiPerfil.aspx ReporteTareas/js/miPerfil.js
git commit -m "feat(perfil): pestanas de experiencia laboral y cargas familiares"
```

---

### Task 7: Cierre de la fase

**Files:**
- Modify: `ReporteTareas/Formulario/MiPerfil.aspx`
- Modify: `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md`

**Interfaces:**
- Consumes: todo lo anterior.
- Produces: nada; es la última tarea.

- [ ] **Step 1: Subir la versión del JavaScript**

En `ReporteTareas/Formulario/MiPerfil.aspx`, cambiar `../js/miPerfil.js?v=4` por `../js/miPerfil.js?v=5`.

El archivo cambió en las tareas 5 y 6. Sin subir ese número, el navegador de quien ya abrió el perfil sirve la versión vieja y las pestañas nuevas no funcionan — y el fallo aparece sólo para algunos usuarios, que es lo que lo hace difícil de diagnosticar. Ya ocurrió una vez en este sistema.

- [ ] **Step 2: Actualizar el spec**

En `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md`, en la descripción de las fases, marcar la fase 2 como completa y dejar constancia de las tres decisiones que se tomaron al implementarla y que el diseño no preveía:

- Las cargas familiares se leen en un **octavo** result set, añadido al final, porque el Dao recorre por posición.
- `Emp_CargaFamiliar` usa `'Activo'`/`'Inactivo'`, no `'1'`/`'0'`, porque la comparte `RRHHEmpleados.aspx`. El filtro de lectura es `<> 'Inactivo'` y no `= 'Activo'` para ver también las filas que esa pantalla inserta sin estado.
- `IdEmpleado` se deja nulo: las cargas cuelgan de `Cod_Usuario` para que los 119 sin ficha las registren igual.

- [ ] **Step 3: Compilar y correr todas las pruebas**

Run:
```
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```
Expected: `0 Error(s)` y **56 de 56**.

- [ ] **Step 4: Lista de comprobación manual**

**No la puedes ejecutar**: el login va por Active Directory y desde este entorno no hay forma de autenticarse. **Transcríbela en tu informe y decláralala pendiente para el usuario.** No la marques como hecha.

| Caso | Qué debe pasar |
|---|---|
| Agregar un estudio completo | Aparece en la tabla **sin recargar la página** |
| Agregar un estudio sin título | Lo rechaza con *"Escriba el título obtenido."* |
| Agregar un estudio con año 2 años en el futuro | Lo rechaza |
| Agregar un estudio **sin año** | Lo acepta: no recordarlo es legítimo |
| Agregar experiencia con año de fin anterior al de inicio | Lo rechaza |
| Agregar experiencia **sin año de fin** | Lo acepta y la tabla muestra *"Actual"* |
| Agregar una carga familiar con fecha futura | Lo rechaza |
| Agregar una carga familiar desde el perfil | Se guarda y aparece en la lista del perfil. **No aparece en `RRHHEmpleados.aspx`** — es por diseño, no un defecto: `IdEmpleado` se deja nulo a propósito para que los 119 sin ficha también puedan registrar cargas, y esa pantalla lee `WHERE IdEmpleado = @IdEmpleado`, así que nunca va a coincidir, ni siquiera para los 112 que sí tienen ficha |
| Un usuario **de los 119 sin ficha** | Puede registrar cargas familiares con normalidad |
| Un usuario **de los 4 con código repetido** | Las pestañas siguen ocultas; si fuerza un guardado desde la consola, recibe el aviso y no se escribe nada |

El antepenúltimo es el que nadie más va a probar: confirma que una carga familiar cargada desde el perfil, en efecto, no se ve desde Talento Humano. Esto hay que comunicárselo a RRHH antes de presentar el módulo, para que no lo descubran como si fuera una falla: las cargas familiares que la gente registre desde su perfil no van a aparecer en `RRHHEmpleados.aspx` en ninguna fase futura mientras esa pantalla siga filtrando por `IdEmpleado`.

- [ ] **Step 5: Commit**

```bash
git add ReporteTareas/Formulario/MiPerfil.aspx docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md
git commit -m "chore(perfil): cierre de la fase 2, version del js y spec al dia"
```

---

## Notas de despliegue

Igual que la fase 1, según `DESPLIEGUE.md`:

1. **El script SQL corre primero.** Es `docs/sql/2026-09-14-perfil-colaborador-fase2.sql`; el de la fase 1 ya está aplicado y no hay que volver a correrlo.
2. Publicar con `FolderProfile` (Release) y **copiar sin sincronizar**. Un `robocopy /MIR` borra `connections.config` y `appsettings.config` y el sitio no arranca.
3. Commitear el paquete regenerado de `ReporteTareas/obj/Release/Package/PackageTmp`, que el repositorio versiona a propósito. En la fase 1 se olvidó y quedó apuntando a un estado anterior.

**Rollback:** todo es aditivo — procedimientos nuevos, un result set más y tres pestañas. Se republican los binarios anteriores y se vuelve a crear `Sp_RTA_PerfilColaborador` con sus siete conjuntos. Las tablas quedan con los datos que la gente haya cargado, que no estorban.

## Fuera de alcance de la fase 2

- **Documentos de respaldo** para certificaciones y cargas familiares. Son de la fase 3, sobre `CargaArchivos.ashx`.
- **Editar** un estudio, certificación, experiencia o carga desde la pantalla. Los procedimientos lo soportan (`Id <> 0`) pero la interfaz sólo ofrece agregar y quitar, igual que en emergencia. Se deja el código porque sirve cuando se decida exponerlo.
- **Foto, CV en PDF y vista de jefatura.** Fase 3.
