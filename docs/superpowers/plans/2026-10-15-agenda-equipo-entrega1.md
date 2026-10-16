# Equipo y agenda (Entrega 1) — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que Gerencia y cada jefe vean la carga semanal de su gente por área, que repartan actividades con entregables y que cada persona lleve su agenda, con aviso por correo al asignar.

**Architecture:** Tablas nuevas `AG_*` y procedimientos `Sp_RTA_Agenda*` en ReporTarea. Toda la lógica (quién ve a quién, permisos de cada acción, estados, contadores, correo) vive en clases puras de `CapaNegocio` con pruebas. Un handler nuevo (`AdministrarAgenda.ashx`) toma la identidad de la sesión, carga datos, pregunta a las reglas y responde JSON. Dos pantallas dentro de `Master.Master` dibujan con jQuery.

**Tech Stack:** ASP.NET WebForms (.NET 4.6.1), C# 7.3, SQL Server 2016 (compatibilidad 130), MSTest, jQuery.

**Spec:** `docs/superpowers/specs/2026-10-15-agenda-equipo-entrega1-design.md`

## Global Constraints

- Rama `ProyectoNuevosCambios`. Commits con la línea `Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>`.
- Prefijos: tablas `AG_*`, procedimientos `Sp_RTA_Agenda*`. Acción de Permisos: `Agenda.VerTodos`.
- Activo = `Usuario_Estado = 'A'` y `EstadoUsuario IS NULL` e `ISNULL(IdCliente, 0) = 0`, **solo** en `Sp_RTA_AgendaPersonasActivas` (y repetido en los scripts de precarga, menú y verificación, que no pueden llamar al procedimiento).
- `Agenda.VerTodos` va al perfil **10 «Gerencia»**; el perfil 18 la tiene siempre por Permisos.
- `R_Usuarios.Departamento` **no se modifica**.
- Las actividades **no** se cruzan con las tareas de Aranda (`R_DetTareasAranda`).
- Correo **solo** al asignar a otra persona. No al cambiar fecha ni al anular.
- La fecha de entrega nunca es sábado ni domingo.
- Compilar **solo** con `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`, **desde PowerShell**.
- Archivos `.cs`, `.aspx` y `.js` nuevos: UTF-8 **con BOM** (las `.aspx` sin BOM salen con caracteres raros en producción; Web.config declara windows-1252).
- Scripts SQL **sin tildes** en el texto: los textos con tilde se arman con `NCHAR` (`á` = `NCHAR(225)`, `Á` = `NCHAR(193)`).
- La base local **es la de producción** (192.168.11.14). Correr cualquier script SQL que escriba exige **autorización explícita del usuario** en ese momento.
- No usar DataTables. jQuery y tablas HTML simples.
- Toda respuesta del handler fija `context.Response.ContentEncoding = Encoding.UTF8`.

## Review Focus

1. **Doble clic en «Guardar»** — se espera una sola actividad. El formulario desactiva el botón mientras guarda (Task 8, paso de prueba manual).
2. **Persona sin correo o servidor de correo caído** — la actividad queda guardada, quien asignó ve el aviso amarillo y hay una fila en `AG_CorreoBitacora` con `Enviado = 0` (Task 7 y prueba manual en Task 11).
3. **Tildes y caracteres especiales** en títulos, nombres y entregables — llegan bien a la pantalla y al correo, y no se ejecuta HTML ajeno (prueba `Cuerpo_EscapaHtmlYTildes` en Task 4; ContentEncoding en Task 7).
4. **Cambiar de semana rápido** — solo se dibuja la última respuesta (contador `agConsulta` en Task 8).
5. **Organigrama con datos raros** — ciclos, códigos con espacios o mayúsculas distintas, personas repetidas (pruebas de `Cadena` en Task 1).

---

## Mapa de archivos

| Archivo | Responsabilidad |
|---|---|
| `CapaEntidad/EntAgenda.cs` | Entidades de datos y de pantalla del módulo |
| `CapaNegocio/NegAgendaEquipo.cs` | Cadena de jefatura, quién ve y a quién se asigna |
| `CapaNegocio/NegAgendaActividad.cs` | Estado, permisos sobre una actividad, fechas, validación |
| `CapaNegocio/NegAgendaVista.cs` | Contadores, tarjetas de área y agenda de una persona |
| `CapaNegocio/NegAgendaCorreo.cs` | Si se avisa, asunto y cuerpo del correo |
| `CapaNegocio/NegAgendaAreas.cs` | Validación y conteo del catálogo de áreas |
| `CapaNegocio/NegAgenda.Datos.cs` | Único archivo de la agenda que llama a `CapaDato` |
| `CapaDato/DaoAgenda.cs` | Llamadas a `Sp_RTA_Agenda*` |
| `CapaPruebas/NegAgenda*Tests.cs` | Pruebas de las cinco clases puras |
| `docs/sql/2026-10-15-agenda-esquema.sql` | Tablas, fila «Sin área», procedimientos |
| `docs/sql/2026-10-15-agenda-precarga.sql` | Áreas desde `Departamento` y área de cada persona |
| `docs/sql/2026-10-15-agenda-menu.sql` | Grupo y dos hojas de menú, visibilidad, acción `Agenda.VerTodos` |
| `docs/sql/2026-10-15-agenda-verificacion.sql` | Comprueba todo lo anterior |
| `ReporteTareas/clases/Permisos.cs` | Constantes de las 2 pantallas y la acción |
| `ReporteTareas/clases/EnvioCorreoHelper.cs` | `EnviarCorreoAgenda` y remitente configurable |
| `ReporteTareas/Formulario/AdministrarAgenda.ashx(.cs)` | Handler de las dos pantallas |
| `ReporteTareas/Formulario/Agenda.aspx(.cs, .designer.cs)` + `js/agenda.js` | Pantalla «Equipo y agenda» |
| `ReporteTareas/Formulario/ParametrizacionAreasAgenda.aspx(.cs, .designer.cs)` + `js/parametrizacionAreasAgenda.js` | Pantalla «Áreas de la agenda» |
| `DESPLIEGUE.md` | Sección 19 |

## Comandos que se repiten

**Compilar** (PowerShell):
```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "C:\respaldodisco\Desarrollo\PRY_Sistema ReporteTareas\ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo | Select-String -Pattern "error" | Select-Object -First 20
```
Sin salida = sin errores. Los cientos de warnings CS0219/CS0162 son previos y no cuentan.

**Probar** (PowerShell; `<Filtro>` es el nombre de la clase de prueba, o se omite `/Tests:` para correr todo):
```powershell
$v = Get-ChildItem "C:\Program Files (x86)\Microsoft Visual Studio\2019" -Recurse -Filter vstest.console.exe -ErrorAction SilentlyContinue | Select-Object -First 1
& $v.FullName "C:\respaldodisco\Desarrollo\PRY_Sistema ReporteTareas\CapaPruebas\bin\Debug\CapaPruebas.dll" /Tests:<Filtro> | Select-String -Pattern "Correcto|Con errores|Error|Total"
```

**Agregar el BOM** a un archivo recién creado (Git Bash):
```bash
python -c "import sys;p=sys.argv[1];d=open(p,'rb').read();open(p,'wb').write(d if d.startswith(b'\xef\xbb\xbf') else b'\xef\xbb\xbf'+d)" "<ruta>"
```

**Correr un script SQL contra producción** (Git Bash, solo con autorización del usuario):
```bash
cd "/c/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
PW=$(grep -m1 "Catalog=ReporTarea" ReporteTareas/connections.config | sed -E 's/.*Password=([^;"]*).*/\1/')
sqlcmd -S 192.168.11.14 -d ReporTarea -U sa -P "$PW" -W -s"|" -i "$(cygpath -w docs/sql/<script>.sql)"
```

---

### Task 1: Entidades y cadena de jefatura

**Files:**
- Create: `CapaEntidad/EntAgenda.cs`
- Create: `CapaNegocio/NegAgendaEquipo.cs`
- Create: `CapaPruebas/NegAgendaEquipoTests.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj` (junto a `<Compile Include="EntPermisos.cs" />`)
- Modify: `CapaNegocio/CapaNegocio.csproj` (junto a `<Compile Include="NegPermisos.cs" />`)
- Modify: `CapaPruebas/CapaPruebas.csproj` (junto a `<Compile Include="NegPermisosTests.cs" />`)

**Interfaces:**
- Produces: todas las entidades `EntAgenda*` (abajo); `NegAgendaEquipo.Codigos` (`StringComparer`), `Normalizar(string) : string`, `Cadena(IEnumerable<EntAgendaPersona>, string) : HashSet<string>`, `PuedeVer(string yo, string persona, ISet<string> cadena, bool verTodos) : bool`, `PuedeAsignarA(...) : bool` (mismos parámetros); helper de prueba `NegAgendaEquipoTests.P(...)` y `Organigrama()`.

- [ ] **Step 1: Crear las entidades**

`CapaEntidad/EntAgenda.cs`:
```csharp
using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Persona activa de la agenda, con su area ya resuelta por
    /// Sp_RTA_AgendaPersonasActivas: si no tiene area, o la suya esta
    /// inactiva, llega con la fila "Sin area" y SinArea = true.
    /// </summary>
    public class EntAgendaPersona
    {
        public string CodUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Correo { get; set; } = "";
        public string CodJefe { get; set; } = "";
        public int IdArea { get; set; }
        public string Area { get; set; } = "";
        public string Color { get; set; } = "";
        public bool SinArea { get; set; }
    }

    public class EntAgendaArea
    {
        public int IdArea { get; set; }
        public string Nombre { get; set; } = "";
        public string Color { get; set; } = "";
        public bool Activa { get; set; }
        public bool EsSinArea { get; set; }
        /// <summary>Personas activas en el area. Lo cuenta NegAgendaAreas.ConPersonas.</summary>
        public int Personas { get; set; }
    }

    public class EntAgendaEntregable
    {
        public int IdEntregable { get; set; }
        public int IdActividad { get; set; }
        public string Descripcion { get; set; } = "";
        public int Orden { get; set; }
        public bool Hecho { get; set; }
    }

    public class EntAgendaActividad
    {
        public int IdActividad { get; set; }
        public string Titulo { get; set; } = "";
        public string CodResponsable { get; set; } = "";
        /// <summary>Igual a CodResponsable si la registro la misma persona.</summary>
        public string CodAsignadoPor { get; set; } = "";
        public DateTime FechaEntrega { get; set; }
        /// <summary>Solo cuenta si la actividad no tiene entregables.</summary>
        public bool CompletadaManual { get; set; }
        public DateTime? FechaCompletada { get; set; }
        public bool Anulada { get; set; }
        public List<EntAgendaEntregable> Entregables { get; set; } = new List<EntAgendaEntregable>();
    }

    // ------------------------------------------------ lo que va a la pantalla

    public class EntAgendaContador
    {
        public int Actividades { get; set; }
        public int Completas { get; set; }
        /// <summary>De cualquier fecha, no solo de la semana.</summary>
        public int Atrasadas { get; set; }
        public int EntregablesTotal { get; set; }
        public int EntregablesHechos { get; set; }
        /// <summary>null si en la semana no hay entregables.</summary>
        public int? Porcentaje { get; set; }
    }

    public class EntAgendaTarjetaPersona
    {
        public string CodUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Iniciales { get; set; } = "";
        public EntAgendaContador Contador { get; set; } = new EntAgendaContador();
    }

    public class EntAgendaTarjetaArea
    {
        public int IdArea { get; set; }
        public string Area { get; set; } = "";
        public string Color { get; set; } = "";
        public bool SinArea { get; set; }
        public List<EntAgendaTarjetaPersona> Personas { get; set; } = new List<EntAgendaTarjetaPersona>();
        public EntAgendaContador Contador { get; set; } = new EntAgendaContador();
    }

    public class EntAgendaItemEntregable
    {
        public int IdEntregable { get; set; }
        public string Descripcion { get; set; } = "";
        public bool Hecho { get; set; }
    }

    public class EntAgendaItem
    {
        public int IdActividad { get; set; }
        public string Titulo { get; set; } = "";
        /// <summary>yyyy-MM-dd.</summary>
        public string FechaEntrega { get; set; } = "";
        /// <summary>Nombre de quien la asigno; vacio si es propia.</summary>
        public string AsignadoPor { get; set; } = "";
        public bool Propia { get; set; }
        public bool Completa { get; set; }
        public bool Atrasada { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeMarcar { get; set; }
        public List<EntAgendaItemEntregable> Entregables { get; set; } = new List<EntAgendaItemEntregable>();
    }

    public class EntAgendaDia
    {
        /// <summary>yyyy-MM-dd.</summary>
        public string Fecha { get; set; } = "";
        /// <summary>"lunes", "martes"... en minusculas.</summary>
        public string Nombre { get; set; } = "";
        public bool EsHoy { get; set; }
        public List<EntAgendaItem> Actividades { get; set; } = new List<EntAgendaItem>();
    }

    public class EntAgendaVistaPersona
    {
        public string CodUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Iniciales { get; set; } = "";
        public string Area { get; set; } = "";
        public string Color { get; set; } = "";
        /// <summary>Es la agenda de quien la mira.</summary>
        public bool EsPropia { get; set; }
        /// <summary>Puede crearle actividades: es la propia, o puede asignarle.</summary>
        public bool PuedeAsignar { get; set; }
        public List<EntAgendaItem> Atrasadas { get; set; } = new List<EntAgendaItem>();
        public List<EntAgendaDia> Dias { get; set; } = new List<EntAgendaDia>();
    }
}
```

En `CapaEntidad/CapaEntidad.csproj`, después de `<Compile Include="EntPermisos.cs" />`:
```xml
    <Compile Include="EntAgenda.cs" />
```

- [ ] **Step 2: Escribir las pruebas (fallan: no existe `NegAgendaEquipo`)**

`CapaPruebas/NegAgendaEquipoTests.cs`:
```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    /// <summary>Datos inventados a proposito: el repositorio es publico.</summary>
    [TestClass]
    public class NegAgendaEquipoTests
    {
        internal static EntAgendaPersona P(string cod, string jefe, int idArea = 1, string area = "AREA A",
            bool sinArea = false, string nombre = null, string correo = "")
        {
            return new EntAgendaPersona
            {
                CodUsuario = cod,
                CodJefe = jefe,
                Nombre = nombre ?? ("PERSONA " + cod),
                Correo = correo,
                IdArea = idArea,
                Area = area,
                Color = "#0078D4",
                SinArea = sinArea
            };
        }

        /// <summary>G (cima) -> J1 -> C1, C2 ;  G -> J2 -> C3 -> C4.</summary>
        internal static List<EntAgendaPersona> Organigrama()
        {
            return new List<EntAgendaPersona>
            {
                P("G", "0", 1, "GERENCIA"),
                P("J1", "G", 2, "SERVICIOS"), P("C1", "J1", 2, "SERVICIOS"), P("C2", "J1", 2, "SERVICIOS"),
                P("J2", "G", 3, "COMERCIAL"), P("C3", "J2", 3, "COMERCIAL"), P("C4", "C3", 3, "COMERCIAL")
            };
        }

        private static List<string> Lista(HashSet<string> c) { return c.OrderBy(x => x).ToList(); }

        [TestMethod]
        public void Cadena_ReportesDirectos()
        {
            CollectionAssert.AreEquivalent(new[] { "C1", "C2" }, Lista(NegAgendaEquipo.Cadena(Organigrama(), "J1")));
        }

        [TestMethod]
        public void Cadena_VariosNiveles_TodaLaRamaHaciaAbajo()
        {
            CollectionAssert.AreEquivalent(new[] { "C3", "C4" }, Lista(NegAgendaEquipo.Cadena(Organigrama(), "J2")));
            CollectionAssert.AreEquivalent(new[] { "J1", "C1", "C2", "J2", "C3", "C4" }, Lista(NegAgendaEquipo.Cadena(Organigrama(), "G")));
        }

        [TestMethod]
        public void Cadena_SinGenteACargo_Vacia()
        {
            Assert.AreEqual(0, NegAgendaEquipo.Cadena(Organigrama(), "C4").Count);
        }

        [TestMethod]
        public void Cadena_Ciclo_NoSeCuelgaNiSeIncluyeASiMismo()
        {
            List<EntAgendaPersona> l = new List<EntAgendaPersona> { P("A", "B"), P("B", "A"), P("C", "B") };
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, Lista(NegAgendaEquipo.Cadena(l, "A")));
        }

        [TestMethod]
        public void Cadena_JefeDeSiMismo_SeIgnora()
        {
            List<EntAgendaPersona> l = new List<EntAgendaPersona> { P("A", "A"), P("B", "A") };
            CollectionAssert.AreEquivalent(new[] { "B" }, Lista(NegAgendaEquipo.Cadena(l, "A")));
        }

        [TestMethod]
        public void Cadena_JefeFueraDeLosActivos_NadieMasLaVe()
        {
            List<EntAgendaPersona> l = Organigrama();
            l.Add(P("X", "INACTIVO"));
            l.Add(P("Y", "X"));
            CollectionAssert.AreEquivalent(new[] { "Y" }, Lista(NegAgendaEquipo.Cadena(l, "X")));
            Assert.IsFalse(NegAgendaEquipo.Cadena(l, "G").Contains("X"));
        }

        [TestMethod]
        public void Cadena_IgnoraMayusculasYEspacios()
        {
            List<EntAgendaPersona> l = new List<EntAgendaPersona> { P("J1", "G"), P(" c1 ", "j1 ") };
            Assert.IsTrue(NegAgendaEquipo.Cadena(l, "J1").Contains("C1"));
        }

        [TestMethod]
        public void Cadena_PersonaRepetida_NoDuplica()
        {
            List<EntAgendaPersona> l = new List<EntAgendaPersona> { P("C1", "J1"), P("C1", "J1") };
            Assert.AreEqual(1, NegAgendaEquipo.Cadena(l, "J1").Count);
        }

        [TestMethod]
        public void Cadena_RaizVaciaONula_Vacia()
        {
            Assert.AreEqual(0, NegAgendaEquipo.Cadena(Organigrama(), "").Count);
            Assert.AreEqual(0, NegAgendaEquipo.Cadena(Organigrama(), null).Count);
            Assert.AreEqual(0, NegAgendaEquipo.Cadena(null, "G").Count);
        }

        [TestMethod]
        public void PuedeVer_ASiMismo_ASuCadena_NoAOtraRama()
        {
            HashSet<string> cadena = NegAgendaEquipo.Cadena(Organigrama(), "J1");
            Assert.IsTrue(NegAgendaEquipo.PuedeVer("J1", "J1", cadena, false));
            Assert.IsTrue(NegAgendaEquipo.PuedeVer("J1", "c1", cadena, false));
            Assert.IsFalse(NegAgendaEquipo.PuedeVer("J1", "C3", cadena, false));
            Assert.IsFalse(NegAgendaEquipo.PuedeVer("J1", "G", cadena, false));
        }

        [TestMethod]
        public void PuedeVer_VerTodos_ACualquiera_PeroNoAVacio()
        {
            Assert.IsTrue(NegAgendaEquipo.PuedeVer("C1", "C3", new HashSet<string>(), true));
            Assert.IsFalse(NegAgendaEquipo.PuedeVer("C1", " ", new HashSet<string>(), true));
        }

        [TestMethod]
        public void PuedeAsignarA_NuncaASiMismo()
        {
            Assert.IsFalse(NegAgendaEquipo.PuedeAsignarA("G", "G", NegAgendaEquipo.Cadena(Organigrama(), "G"), true));
        }

        [TestMethod]
        public void PuedeAsignarA_SuCadena_OVerTodos()
        {
            HashSet<string> cadena = NegAgendaEquipo.Cadena(Organigrama(), "J2");
            Assert.IsTrue(NegAgendaEquipo.PuedeAsignarA("J2", "C4", cadena, false));
            Assert.IsFalse(NegAgendaEquipo.PuedeAsignarA("J2", "C1", cadena, false));
            Assert.IsTrue(NegAgendaEquipo.PuedeAsignarA("J2", "C1", cadena, true));
        }
    }
}
```

En `CapaPruebas/CapaPruebas.csproj`, después de `<Compile Include="NegPermisosTests.cs" />`:
```xml
    <Compile Include="NegAgendaEquipoTests.cs" />
```

- [ ] **Step 3: Compilar para ver que falla**

Correr el comando de compilar. Esperado: `error CS0103: The name 'NegAgendaEquipo' does not exist` (o `CS0246`).

- [ ] **Step 4: Implementar `NegAgendaEquipo`**

`CapaNegocio/NegAgendaEquipo.cs`:
```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Quien ve a quien en la agenda. Clase pura: no toca base, sesion ni
    /// HttpContext.
    ///
    /// - Cada persona se ve a si misma.
    /// - Cada jefe ve a TODA su cadena hacia abajo (R_Usuarios.Cod_Jefe_Inm,
    ///   recursivo).
    /// - Quien tiene la accion Agenda.VerTodos ve a todos.
    ///
    /// La cadena se recorre con un conjunto de visitados: un ciclo de jefes
    /// (A jefe de B y B jefe de A, un error de datos) no cuelga el calculo, y
    /// nadie entra en su propia cadena por un ciclo. Los codigos se comparan
    /// sin espacios y sin distinguir mayusculas: Cod_Usuario no tiene formato
    /// unico.
    /// </summary>
    public static class NegAgendaEquipo
    {
        public static readonly StringComparer Codigos = StringComparer.OrdinalIgnoreCase;

        public static string Normalizar(string cod)
        {
            return (cod ?? "").Trim();
        }

        /// <summary>La cadena hacia abajo de codJefe, sin incluirlo a el.</summary>
        public static HashSet<string> Cadena(IEnumerable<EntAgendaPersona> personas, string codJefe)
        {
            HashSet<string> cadena = new HashSet<string>(Codigos);
            string raiz = Normalizar(codJefe);
            if (raiz == "" || personas == null) { return cadena; }

            Dictionary<string, List<string>> hijos = new Dictionary<string, List<string>>(Codigos);
            foreach (EntAgendaPersona p in personas)
            {
                if (p == null) { continue; }
                string cod = Normalizar(p.CodUsuario);
                string jefe = Normalizar(p.CodJefe);
                if (cod == "" || jefe == "" || Codigos.Equals(cod, jefe)) { continue; }

                List<string> directos;
                if (!hijos.TryGetValue(jefe, out directos))
                {
                    directos = new List<string>();
                    hijos[jefe] = directos;
                }
                directos.Add(cod);
            }

            Queue<string> pendientes = new Queue<string>();
            pendientes.Enqueue(raiz);
            while (pendientes.Count > 0)
            {
                List<string> directos;
                if (!hijos.TryGetValue(pendientes.Dequeue(), out directos)) { continue; }
                foreach (string h in directos)
                {
                    if (Codigos.Equals(h, raiz) || !cadena.Add(h)) { continue; }
                    pendientes.Enqueue(h);
                }
            }
            return cadena;
        }

        public static bool PuedeVer(string yo, string persona, ISet<string> cadena, bool verTodos)
        {
            string p = Normalizar(persona);
            if (p == "") { return false; }
            return verTodos
                || Codigos.Equals(Normalizar(yo), p)
                || (cadena != null && cadena.Contains(p));
        }

        /// <summary>A uno mismo no se le "asigna": se registra una actividad propia.</summary>
        public static bool PuedeAsignarA(string yo, string persona, ISet<string> cadena, bool verTodos)
        {
            return !Codigos.Equals(Normalizar(yo), Normalizar(persona))
                && PuedeVer(yo, persona, cadena, verTodos);
        }
    }
}
```

`cadena.Contains(p)` funciona sin importar mayúsculas porque `Cadena` devuelve un `HashSet` con `Codigos`.

En `CapaNegocio/CapaNegocio.csproj`, después de `<Compile Include="NegPermisos.cs" />`:
```xml
    <Compile Include="NegAgendaEquipo.cs" />
```

- [ ] **Step 5: Agregar el BOM a los tres `.cs` nuevos, compilar y probar**

BOM a `CapaEntidad/EntAgenda.cs`, `CapaNegocio/NegAgendaEquipo.cs` y `CapaPruebas/NegAgendaEquipoTests.cs`. Compilar. Probar con `/Tests:NegAgendaEquipoTests`.
Esperado: `Correcto: 13`, sin errores.

- [ ] **Step 6: Commit**

```bash
git add CapaEntidad/EntAgenda.cs CapaEntidad/CapaEntidad.csproj CapaNegocio/NegAgendaEquipo.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegAgendaEquipoTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(agenda): entidades y cadena de jefatura

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 2: Reglas de una actividad (estado, permisos, fechas, validación)

**Files:**
- Create: `CapaNegocio/NegAgendaActividad.cs`
- Create: `CapaPruebas/NegAgendaActividadTests.cs`
- Modify: `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Consumes: `NegAgendaEquipo.Codigos`, `Normalizar`, `PuedeVer`, `PuedeAsignarA`; `NegAgendaEquipoTests.Organigrama()`.
- Produces: `NegAgendaActividad.EsPropia(EntAgendaActividad) : bool`, `Completa(EntAgendaActividad) : bool`, `Atrasada(EntAgendaActividad, DateTime hoy) : bool`, `PuedeEditar(string yo, EntAgendaActividad, ISet<string> cadena, bool verTodos) : bool`, `PuedeMarcar(string yo, EntAgendaActividad) : bool`, `EsDiaHabil(DateTime) : bool`, `ProximoDiaHabil(DateTime hoy) : DateTime`, `Lunes(DateTime) : DateTime`, `Limpiar(IEnumerable<EntAgendaEntregable>) : List<EntAgendaEntregable>`, `Validar(string titulo, DateTime? fecha, IList<EntAgendaEntregable>) : string` (null = válido); constantes `LargoTexto = 200`, `MaxEntregables = 20`; helper de prueba `NegAgendaActividadTests.A(...)`.

- [ ] **Step 1: Escribir las pruebas**

Fechas de referencia: 2026-10-12 es lunes y 2026-10-15 es jueves.

`CapaPruebas/NegAgendaActividadTests.cs`:
```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegAgendaActividadTests
    {
        internal static readonly DateTime Lunes12 = new DateTime(2026, 10, 12);
        internal static readonly DateTime Jueves15 = new DateTime(2026, 10, 15);

        private static int _id;

        internal static EntAgendaActividad A(string responsable, string asignadoPor, DateTime entrega, params bool[] entregables)
        {
            EntAgendaActividad a = new EntAgendaActividad
            {
                IdActividad = ++_id,
                Titulo = "ACTIVIDAD " + _id,
                CodResponsable = responsable,
                CodAsignadoPor = asignadoPor,
                FechaEntrega = entrega
            };
            int i = 0;
            foreach (bool hecho in entregables)
            {
                i++;
                a.Entregables.Add(new EntAgendaEntregable
                {
                    IdEntregable = a.IdActividad * 100 + i,
                    IdActividad = a.IdActividad,
                    Descripcion = "ENTREGABLE " + i,
                    Orden = i,
                    Hecho = hecho
                });
            }
            return a;
        }

        private static HashSet<string> CadenaDe(string yo) { return NegAgendaEquipo.Cadena(NegAgendaEquipoTests.Organigrama(), yo); }

        // ---------------------------------------------- estado

        [TestMethod]
        public void Completa_ConEntregables_SoloSiTodosEstanHechos()
        {
            Assert.IsTrue(NegAgendaActividad.Completa(A("C1", "C1", Jueves15, true, true)));
            Assert.IsFalse(NegAgendaActividad.Completa(A("C1", "C1", Jueves15, true, false)));
        }

        [TestMethod]
        public void Completa_SinEntregables_UsaLaMarcaManual()
        {
            EntAgendaActividad a = A("C1", "C1", Jueves15);
            Assert.IsFalse(NegAgendaActividad.Completa(a));
            a.CompletadaManual = true;
            Assert.IsTrue(NegAgendaActividad.Completa(a));
        }

        [TestMethod]
        public void Completa_ConEntregables_IgnoraLaMarcaManual()
        {
            EntAgendaActividad a = A("C1", "C1", Jueves15, false);
            a.CompletadaManual = true;
            Assert.IsFalse(NegAgendaActividad.Completa(a));
        }

        [TestMethod]
        public void Atrasada_VenceHoy_No()
        {
            Assert.IsFalse(NegAgendaActividad.Atrasada(A("C1", "C1", Jueves15), Jueves15.AddHours(18)));
        }

        [TestMethod]
        public void Atrasada_VencioAyer_Si()
        {
            Assert.IsTrue(NegAgendaActividad.Atrasada(A("C1", "C1", Jueves15.AddDays(-1)), Jueves15));
        }

        [TestMethod]
        public void Atrasada_CompletaOAnulada_No()
        {
            Assert.IsFalse(NegAgendaActividad.Atrasada(A("C1", "C1", Lunes12, true), Jueves15));
            EntAgendaActividad anulada = A("C1", "C1", Lunes12);
            anulada.Anulada = true;
            Assert.IsFalse(NegAgendaActividad.Atrasada(anulada, Jueves15));
        }

        [TestMethod]
        public void EsPropia_IgnoraMayusculasYEspacios()
        {
            Assert.IsTrue(NegAgendaActividad.EsPropia(A("C1", " c1 ", Jueves15)));
            Assert.IsFalse(NegAgendaActividad.EsPropia(A("C1", "J1", Jueves15)));
        }

        // ---------------------------------------------- permisos

        [TestMethod]
        public void PuedeEditar_Asignada_QuienAsignoYSuCadenaSi_ElResponsableNo()
        {
            EntAgendaActividad a = A("C1", "J1", Jueves15);
            Assert.IsTrue(NegAgendaActividad.PuedeEditar("J1", a, CadenaDe("J1"), false));
            Assert.IsTrue(NegAgendaActividad.PuedeEditar("G", a, CadenaDe("G"), false));
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("C1", a, CadenaDe("C1"), false));
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("C2", a, CadenaDe("C2"), false));
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("J2", a, CadenaDe("J2"), false));
            Assert.IsTrue(NegAgendaActividad.PuedeEditar("OTRO", a, new HashSet<string>(), true));
        }

        [TestMethod]
        public void PuedeEditar_Propia_ElResponsableYSuJefe()
        {
            EntAgendaActividad a = A("C1", "C1", Jueves15);
            Assert.IsTrue(NegAgendaActividad.PuedeEditar("C1", a, CadenaDe("C1"), false));
            Assert.IsTrue(NegAgendaActividad.PuedeEditar("J1", a, CadenaDe("J1"), false));
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("C2", a, CadenaDe("C2"), false));
        }

        [TestMethod]
        public void PuedeEditar_QuienAsignoPeroYaNoLaVe_No()
        {
            /* J2 se la asigno a C1 cuando C1 estaba en su cadena; hoy C1 depende de J1. */
            EntAgendaActividad a = A("C1", "J2", Jueves15);
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("J2", a, CadenaDe("J2"), false));
        }

        [TestMethod]
        public void PuedeEditar_Anulada_Nadie()
        {
            EntAgendaActividad a = A("C1", "J1", Jueves15);
            a.Anulada = true;
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("J1", a, CadenaDe("J1"), true));
        }

        [TestMethod]
        public void PuedeMarcar_SoloElResponsable_YNoSiEstaAnulada()
        {
            EntAgendaActividad a = A("C1", "J1", Jueves15, false);
            Assert.IsTrue(NegAgendaActividad.PuedeMarcar("c1", a));
            Assert.IsFalse(NegAgendaActividad.PuedeMarcar("J1", a));
            a.Anulada = true;
            Assert.IsFalse(NegAgendaActividad.PuedeMarcar("C1", a));
        }

        // ---------------------------------------------- fechas

        [TestMethod]
        public void ProximoDiaHabil_SaltaElFinDeSemana()
        {
            Assert.AreEqual(new DateTime(2026, 10, 13), NegAgendaActividad.ProximoDiaHabil(Lunes12));
            Assert.AreEqual(new DateTime(2026, 10, 16), NegAgendaActividad.ProximoDiaHabil(Jueves15));
            Assert.AreEqual(new DateTime(2026, 10, 19), NegAgendaActividad.ProximoDiaHabil(new DateTime(2026, 10, 16)));
            Assert.AreEqual(new DateTime(2026, 10, 19), NegAgendaActividad.ProximoDiaHabil(new DateTime(2026, 10, 17)));
            Assert.AreEqual(new DateTime(2026, 10, 19), NegAgendaActividad.ProximoDiaHabil(new DateTime(2026, 10, 18, 23, 0, 0)));
        }

        [TestMethod]
        public void Lunes_DeCualquierDiaDeLaSemana()
        {
            Assert.AreEqual(Lunes12, NegAgendaActividad.Lunes(Lunes12));
            Assert.AreEqual(Lunes12, NegAgendaActividad.Lunes(Jueves15.AddHours(10)));
            Assert.AreEqual(Lunes12, NegAgendaActividad.Lunes(new DateTime(2026, 10, 18)));
        }

        // ---------------------------------------------- validacion

        private static List<EntAgendaEntregable> E(int cuantos, int largo = 10)
        {
            return Enumerable.Range(1, cuantos).Select(i => new EntAgendaEntregable { Descripcion = new string('x', largo), Orden = i }).ToList();
        }

        [TestMethod]
        public void Validar_DatosCorrectos_Null()
        {
            Assert.IsNull(NegAgendaActividad.Validar("Preparar informe", Jueves15, E(2)));
            Assert.IsNull(NegAgendaActividad.Validar("Preparar informe", Jueves15, null));
        }

        [TestMethod]
        public void Validar_Titulo()
        {
            Assert.AreEqual("Escriba qué hay que hacer.", NegAgendaActividad.Validar("   ", Jueves15, null));
            Assert.AreEqual("El título admite hasta 200 caracteres.", NegAgendaActividad.Validar(new string('a', 201), Jueves15, null));
            Assert.IsNull(NegAgendaActividad.Validar(new string('a', 200), Jueves15, null));
        }

        [TestMethod]
        public void Validar_Fecha()
        {
            Assert.AreEqual("Elija la fecha de entrega.", NegAgendaActividad.Validar("T", null, null));
            Assert.AreEqual("La fecha de entrega debe ser de lunes a viernes.", NegAgendaActividad.Validar("T", new DateTime(2026, 10, 17), null));
            Assert.AreEqual("La fecha de entrega debe ser de lunes a viernes.", NegAgendaActividad.Validar("T", new DateTime(2026, 10, 18), null));
        }

        [TestMethod]
        public void Validar_Entregables()
        {
            Assert.AreEqual("Una actividad admite hasta 20 entregables.", NegAgendaActividad.Validar("T", Jueves15, E(21)));
            Assert.AreEqual("Cada entregable admite hasta 200 caracteres.", NegAgendaActividad.Validar("T", Jueves15, E(1, 201)));
        }

        [TestMethod]
        public void Limpiar_RecortaQuitaVaciosYNumera()
        {
            List<EntAgendaEntregable> r = NegAgendaActividad.Limpiar(new[]
            {
                new EntAgendaEntregable { IdEntregable = 7, Descripcion = "  Borrador " },
                new EntAgendaEntregable { IdEntregable = 0, Descripcion = "   " },
                null,
                new EntAgendaEntregable { IdEntregable = 0, Descripcion = "Final" }
            });
            Assert.AreEqual(2, r.Count);
            Assert.AreEqual("Borrador", r[0].Descripcion);
            Assert.AreEqual(7, r[0].IdEntregable);
            Assert.AreEqual(1, r[0].Orden);
            Assert.AreEqual("Final", r[1].Descripcion);
            Assert.AreEqual(2, r[1].Orden);
            Assert.AreEqual(0, NegAgendaActividad.Limpiar(null).Count);
        }
    }
}
```

En `CapaPruebas/CapaPruebas.csproj`, junto a la línea del Task 1:
```xml
    <Compile Include="NegAgendaActividadTests.cs" />
```

- [ ] **Step 2: Compilar para ver que falla**

Esperado: error de compilación porque no existe `NegAgendaActividad`.

- [ ] **Step 3: Implementar `NegAgendaActividad`**

`CapaNegocio/NegAgendaActividad.cs`:
```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Reglas de una actividad de la agenda. Clase pura.
    ///
    /// Estado (se calcula, no se guarda):
    /// - Completa: con entregables, todos hechos; sin entregables, la marca manual.
    /// - Atrasada: no completa, no anulada y FechaEntrega anterior a hoy. Vencer
    ///   hoy no es estar atrasada. "hoy" lo pone el servidor.
    ///
    /// Permisos:
    /// - Editar (titulo, fecha, entregables) y anular: hay que ver al
    ///   responsable, y ademas ser quien la asigno o poder asignarle hoy. Para
    ///   una actividad propia, quien la asigno es el mismo responsable. Por eso
    ///   el responsable NO mueve la fecha de lo que le asigno otro.
    /// - Marcar entregables y "completa": solo el responsable.
    /// - Nada de esto sobre una anulada.
    /// </summary>
    public static class NegAgendaActividad
    {
        public const int LargoTexto = 200;
        public const int MaxEntregables = 20;

        private static bool Igual(string a, string b)
        {
            return NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(a), NegAgendaEquipo.Normalizar(b));
        }

        public static bool EsPropia(EntAgendaActividad a)
        {
            return a != null && Igual(a.CodAsignadoPor, a.CodResponsable);
        }

        public static bool Completa(EntAgendaActividad a)
        {
            if (a == null) { return false; }
            if (a.Entregables != null && a.Entregables.Count > 0) { return a.Entregables.All(e => e.Hecho); }
            return a.CompletadaManual;
        }

        public static bool Atrasada(EntAgendaActividad a, DateTime hoy)
        {
            return a != null && !a.Anulada && !Completa(a) && a.FechaEntrega.Date < hoy.Date;
        }

        public static bool PuedeEditar(string yo, EntAgendaActividad a, ISet<string> cadena, bool verTodos)
        {
            if (a == null || a.Anulada) { return false; }
            if (!NegAgendaEquipo.PuedeVer(yo, a.CodResponsable, cadena, verTodos)) { return false; }
            return Igual(yo, a.CodAsignadoPor)
                || NegAgendaEquipo.PuedeAsignarA(yo, a.CodResponsable, cadena, verTodos);
        }

        public static bool PuedeMarcar(string yo, EntAgendaActividad a)
        {
            return a != null && !a.Anulada && Igual(yo, a.CodResponsable);
        }

        public static bool EsDiaHabil(DateTime d)
        {
            return d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>El siguiente dia de lunes a viernes despues de hoy (un viernes propone el lunes).</summary>
        public static DateTime ProximoDiaHabil(DateTime hoy)
        {
            DateTime d = hoy.Date.AddDays(1);
            while (!EsDiaHabil(d)) { d = d.AddDays(1); }
            return d;
        }

        public static DateTime Lunes(DateTime d)
        {
            int desdeLunes = ((int)d.DayOfWeek + 6) % 7;
            return d.Date.AddDays(-desdeLunes);
        }

        /// <summary>Recorta, quita los vacios y numera el orden 1..n. Conserva los IdEntregable.</summary>
        public static List<EntAgendaEntregable> Limpiar(IEnumerable<EntAgendaEntregable> entregables)
        {
            List<EntAgendaEntregable> r = new List<EntAgendaEntregable>();
            if (entregables == null) { return r; }
            foreach (EntAgendaEntregable e in entregables)
            {
                string texto = e == null ? "" : (e.Descripcion ?? "").Trim();
                if (texto == "") { continue; }
                r.Add(new EntAgendaEntregable
                {
                    IdEntregable = e.IdEntregable,
                    IdActividad = e.IdActividad,
                    Descripcion = texto,
                    Orden = r.Count + 1,
                    Hecho = e.Hecho
                });
            }
            return r;
        }

        /// <summary>null si todo esta bien; si no, el mensaje para la persona.</summary>
        public static string Validar(string titulo, DateTime? fecha, IList<EntAgendaEntregable> entregables)
        {
            string t = (titulo ?? "").Trim();
            if (t == "") { return "Escriba qué hay que hacer."; }
            if (t.Length > LargoTexto) { return "El título admite hasta 200 caracteres."; }
            if (fecha == null) { return "Elija la fecha de entrega."; }
            if (!EsDiaHabil(fecha.Value)) { return "La fecha de entrega debe ser de lunes a viernes."; }
            if (entregables != null)
            {
                if (entregables.Count > MaxEntregables) { return "Una actividad admite hasta 20 entregables."; }
                if (entregables.Any(e => e != null && (e.Descripcion ?? "").Trim().Length > LargoTexto))
                {
                    return "Cada entregable admite hasta 200 caracteres.";
                }
            }
            return null;
        }
    }
}
```

En `CapaNegocio/CapaNegocio.csproj`, junto a la línea del Task 1:
```xml
    <Compile Include="NegAgendaActividad.cs" />
```

- [ ] **Step 4: BOM, compilar y probar**

BOM a los dos archivos nuevos. Compilar. Probar con `/Tests:NegAgendaActividadTests`.
Esperado: `Correcto: 19`.

- [ ] **Step 5: Commit**

```bash
git add CapaNegocio/NegAgendaActividad.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegAgendaActividadTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(agenda): estado, permisos, fechas y validacion de una actividad

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 3: Vistas (contadores, tarjetas de área, agenda de una persona)

**Files:**
- Create: `CapaNegocio/NegAgendaVista.cs`
- Create: `CapaPruebas/NegAgendaVistaTests.cs`
- Modify: `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Consumes: `NegAgendaEquipo.*`, `NegAgendaActividad.*`, `NegAgendaEquipoTests.P/Organigrama`, `NegAgendaActividadTests.A/Lunes12/Jueves15`.
- Produces: `NegAgendaVista.Iniciales(string) : string`, `Contar(IEnumerable<EntAgendaActividad>, DateTime lunes, DateTime hoy) : EntAgendaContador`, `Sumar(IEnumerable<EntAgendaContador>) : EntAgendaContador`, `Visibles(string yo, bool verTodos, IList<EntAgendaPersona>) : List<EntAgendaPersona>`, `Equipo(string yo, bool verTodos, IList<EntAgendaPersona>, IList<EntAgendaActividad>, DateTime lunes, DateTime hoy) : List<EntAgendaTarjetaArea>`, `Persona(string yo, bool verTodos, IList<EntAgendaPersona>, IList<EntAgendaActividad>, string cod, DateTime lunes, DateTime hoy) : EntAgendaVistaPersona` (null si no la puede ver).

- [ ] **Step 1: Escribir las pruebas**

`CapaPruebas/NegAgendaVistaTests.cs`:
```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegAgendaVistaTests
    {
        private static readonly DateTime L = NegAgendaActividadTests.Lunes12;
        private static readonly DateTime Hoy = NegAgendaActividadTests.Jueves15;

        private static EntAgendaActividad A(string resp, string asig, DateTime entrega, params bool[] e)
        {
            return NegAgendaActividadTests.A(resp, asig, entrega, e);
        }

        private static List<EntAgendaPersona> ConSinArea()
        {
            List<EntAgendaPersona> l = NegAgendaEquipoTests.Organigrama();
            l.Add(NegAgendaEquipoTests.P("C5", "J2", 9, "Sin área", true));
            return l;
        }

        [TestMethod]
        public void Iniciales_DosPrimerasPalabras()
        {
            Assert.AreEqual("PH", NegAgendaVista.Iniciales("PINOS HERNANDEZ CARLOS"));
            Assert.AreEqual("A", NegAgendaVista.Iniciales("  ana "));
            Assert.AreEqual("?", NegAgendaVista.Iniciales(""));
            Assert.AreEqual("?", NegAgendaVista.Iniciales(null));
        }

        [TestMethod]
        public void Contar_SemanaYAtrasadasDeCualquierFecha()
        {
            EntAgendaActividad anulada = A("C1", "C1", L.AddDays(2));
            anulada.Anulada = true;
            List<EntAgendaActividad> acts = new List<EntAgendaActividad>
            {
                A("C1", "C1", L, true, true),          // en la semana, completa
                A("C1", "J1", L.AddDays(1), false),    // en la semana, atrasada
                A("C1", "C1", L.AddDays(4)),           // en la semana, pendiente
                A("C1", "C1", L.AddDays(7)),           // semana siguiente: no cuenta
                A("C1", "C1", L.AddDays(-7)),          // semana anterior, atrasada
                anulada                                 // no cuenta para nada
            };
            EntAgendaContador c = NegAgendaVista.Contar(acts, L, Hoy);
            Assert.AreEqual(3, c.Actividades);
            Assert.AreEqual(1, c.Completas);
            Assert.AreEqual(2, c.Atrasadas);
            Assert.AreEqual(3, c.EntregablesTotal);
            Assert.AreEqual(2, c.EntregablesHechos);
            Assert.AreEqual(67, c.Porcentaje);
        }

        [TestMethod]
        public void Contar_SinEntregables_PorcentajeNull()
        {
            Assert.IsNull(NegAgendaVista.Contar(new[] { A("C1", "C1", L) }, L, Hoy).Porcentaje);
            Assert.AreEqual(0, NegAgendaVista.Contar(null, L, Hoy).Actividades);
        }

        [TestMethod]
        public void Sumar_RecalculaElPorcentajeConLosTotales()
        {
            EntAgendaContador s = NegAgendaVista.Sumar(new[]
            {
                new EntAgendaContador { Actividades = 1, Completas = 1, Atrasadas = 0, EntregablesTotal = 1, EntregablesHechos = 1 },
                new EntAgendaContador { Actividades = 2, Completas = 0, Atrasadas = 1, EntregablesTotal = 3, EntregablesHechos = 0 }
            });
            Assert.AreEqual(3, s.Actividades);
            Assert.AreEqual(1, s.Completas);
            Assert.AreEqual(1, s.Atrasadas);
            Assert.AreEqual(25, s.Porcentaje);
        }

        [TestMethod]
        public void Visibles_SegunLaCadena()
        {
            CollectionAssert.AreEquivalent(new[] { "C1" }, NegAgendaVista.Visibles("C1", false, ConSinArea()).Select(p => p.CodUsuario).ToList());
            CollectionAssert.AreEquivalent(new[] { "J1", "C1", "C2" }, NegAgendaVista.Visibles("J1", false, ConSinArea()).Select(p => p.CodUsuario).ToList());
            Assert.AreEqual(8, NegAgendaVista.Visibles("C1", true, ConSinArea()).Count);
        }

        [TestMethod]
        public void Equipo_UnJefeVeSoloLasAreasDeSuGente()
        {
            List<EntAgendaTarjetaArea> t = NegAgendaVista.Equipo("J1", false, ConSinArea(), new List<EntAgendaActividad>(), L, Hoy);
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual("SERVICIOS", t[0].Area);
            CollectionAssert.AreEquivalent(new[] { "J1", "C1", "C2" }, t[0].Personas.Select(p => p.CodUsuario).ToList());
        }

        [TestMethod]
        public void Equipo_VerTodos_SinAreaAlFinal_ElRestoPorNombre()
        {
            List<EntAgendaTarjetaArea> t = NegAgendaVista.Equipo("G", true, ConSinArea(), new List<EntAgendaActividad>(), L, Hoy);
            CollectionAssert.AreEqual(new[] { "COMERCIAL", "GERENCIA", "SERVICIOS", "Sin área" }, t.Select(x => x.Area).ToList());
            Assert.IsTrue(t[3].SinArea);
        }

        [TestMethod]
        public void Equipo_ContadorDelAreaSumaASusPersonas()
        {
            List<EntAgendaActividad> acts = new List<EntAgendaActividad>
            {
                A("C1", "J1", L.AddDays(1)),            // atrasada
                A("C2", "C2", L.AddDays(3), true),      // completa
                A("C3", "C3", L.AddDays(3))             // otra area
            };
            EntAgendaTarjetaArea servicios = NegAgendaVista.Equipo("G", true, ConSinArea(), acts, L, Hoy).Single(x => x.Area == "SERVICIOS");
            Assert.AreEqual(2, servicios.Contador.Actividades);
            Assert.AreEqual(1, servicios.Contador.Atrasadas);
            Assert.AreEqual(1, servicios.Personas.Single(p => p.CodUsuario == "C1").Contador.Atrasadas);
            Assert.AreEqual("PC", servicios.Personas.Single(p => p.CodUsuario == "C1").Iniciales);
        }

        [TestMethod]
        public void Persona_NoVisible_Null()
        {
            Assert.IsNull(NegAgendaVista.Persona("C1", false, ConSinArea(), new List<EntAgendaActividad>(), "C2", L, Hoy));
            Assert.IsNull(NegAgendaVista.Persona("J1", false, ConSinArea(), new List<EntAgendaActividad>(), "NO-EXISTE", L, Hoy));
        }

        [TestMethod]
        public void Persona_PropiaYDeUnJefe()
        {
            EntAgendaVistaPersona propia = NegAgendaVista.Persona("C1", false, ConSinArea(), new List<EntAgendaActividad>(), "c1", L, Hoy);
            Assert.IsTrue(propia.EsPropia);
            Assert.IsTrue(propia.PuedeAsignar);
            Assert.AreEqual("C1", propia.CodUsuario);

            EntAgendaVistaPersona deJefe = NegAgendaVista.Persona("J1", false, ConSinArea(), new List<EntAgendaActividad>(), "C1", L, Hoy);
            Assert.IsFalse(deJefe.EsPropia);
            Assert.IsTrue(deJefe.PuedeAsignar);
        }

        [TestMethod]
        public void Persona_CincoDiasYHoyMarcado()
        {
            EntAgendaVistaPersona v = NegAgendaVista.Persona("C1", false, ConSinArea(), new List<EntAgendaActividad>(), "C1", L, Hoy);
            CollectionAssert.AreEqual(new[] { "2026-10-12", "2026-10-13", "2026-10-14", "2026-10-15", "2026-10-16" }, v.Dias.Select(d => d.Fecha).ToList());
            Assert.AreEqual("lunes", v.Dias[0].Nombre);
            Assert.AreEqual("jueves", v.Dias[3].Nombre);
            CollectionAssert.AreEqual(new[] { false, false, false, true, false }, v.Dias.Select(d => d.EsHoy).ToList());
        }

        [TestMethod]
        public void Persona_ActividadAsignada_VistaPorElResponsable()
        {
            EntAgendaActividad a = A("C1", "J1", L.AddDays(2), false);
            EntAgendaVistaPersona v = NegAgendaVista.Persona("C1", false, ConSinArea(), new List<EntAgendaActividad> { a }, "C1", L, Hoy);
            EntAgendaItem it = v.Dias[2].Actividades.Single();
            Assert.AreEqual(a.IdActividad, it.IdActividad);
            Assert.AreEqual("PERSONA J1", it.AsignadoPor);
            Assert.IsFalse(it.Propia);
            Assert.IsTrue(it.PuedeMarcar);
            Assert.IsFalse(it.PuedeEditar);
            Assert.IsTrue(it.Atrasada);
            Assert.AreEqual(1, it.Entregables.Count);
        }

        [TestMethod]
        public void Persona_AtrasadasDeSemanasAnteriores_EnLaFranja_SinAnuladasNiDeOtros()
        {
            EntAgendaActividad anulada = A("C1", "C1", L.AddDays(-3));
            anulada.Anulada = true;
            List<EntAgendaActividad> acts = new List<EntAgendaActividad>
            {
                A("C1", "C1", L.AddDays(-10)),
                A("C1", "C1", L.AddDays(-3), true),   // completa: no
                anulada,                              // anulada: no
                A("C2", "C2", L.AddDays(-3))          // de otro: no
            };
            EntAgendaVistaPersona v = NegAgendaVista.Persona("C1", false, ConSinArea(), acts, "C1", L, Hoy);
            Assert.AreEqual(1, v.Atrasadas.Count);
            Assert.AreEqual("2026-10-02", v.Atrasadas[0].FechaEntrega);
            Assert.AreEqual(0, v.Dias.Sum(d => d.Actividades.Count));
        }
    }
}
```

En `CapaPruebas/CapaPruebas.csproj`:
```xml
    <Compile Include="NegAgendaVistaTests.cs" />
```

- [ ] **Step 2: Compilar para ver que falla**

Esperado: error porque no existe `NegAgendaVista`.

- [ ] **Step 3: Implementar `NegAgendaVista`**

`CapaNegocio/NegAgendaVista.cs`:
```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Arma lo que dibujan las pantallas de la agenda. Clase pura: recibe las
    /// personas activas y las actividades ya cargadas, y decide con
    /// NegAgendaEquipo y NegAgendaActividad. El handler no decide nada.
    /// </summary>
    public static class NegAgendaVista
    {
        private static readonly CultureInfo Es = new CultureInfo("es-EC");

        public static string Iniciales(string nombre)
        {
            string[] partes = (nombre ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string r = "";
            foreach (string p in partes)
            {
                r += char.ToUpperInvariant(p[0]);
                if (r.Length == 2) { break; }
            }
            return r == "" ? "?" : r;
        }

        /// <summary>
        /// Actividades, completas y entregables: las que vencen de lunes a viernes
        /// de esa semana. Atrasadas: de cualquier fecha. Las anuladas no cuentan.
        /// </summary>
        public static EntAgendaContador Contar(IEnumerable<EntAgendaActividad> actividades, DateTime lunes, DateTime hoy)
        {
            EntAgendaContador c = new EntAgendaContador();
            DateTime desde = lunes.Date;
            DateTime hasta = lunes.Date.AddDays(4);
            foreach (EntAgendaActividad a in actividades ?? Enumerable.Empty<EntAgendaActividad>())
            {
                if (a == null || a.Anulada) { continue; }
                if (NegAgendaActividad.Atrasada(a, hoy)) { c.Atrasadas++; }
                if (a.FechaEntrega.Date < desde || a.FechaEntrega.Date > hasta) { continue; }
                c.Actividades++;
                if (NegAgendaActividad.Completa(a)) { c.Completas++; }
                c.EntregablesTotal += a.Entregables.Count;
                c.EntregablesHechos += a.Entregables.Count(e => e.Hecho);
            }
            c.Porcentaje = Porcentaje(c);
            return c;
        }

        public static EntAgendaContador Sumar(IEnumerable<EntAgendaContador> contadores)
        {
            EntAgendaContador s = new EntAgendaContador();
            foreach (EntAgendaContador c in contadores ?? Enumerable.Empty<EntAgendaContador>())
            {
                s.Actividades += c.Actividades;
                s.Completas += c.Completas;
                s.Atrasadas += c.Atrasadas;
                s.EntregablesTotal += c.EntregablesTotal;
                s.EntregablesHechos += c.EntregablesHechos;
            }
            s.Porcentaje = Porcentaje(s);
            return s;
        }

        private static int? Porcentaje(EntAgendaContador c)
        {
            if (c.EntregablesTotal == 0) { return null; }
            return (int)Math.Round(c.EntregablesHechos * 100.0 / c.EntregablesTotal, MidpointRounding.AwayFromZero);
        }

        /// <summary>Las personas que yo puedo ver, ordenadas por nombre. Me incluye.</summary>
        public static List<EntAgendaPersona> Visibles(string yo, bool verTodos, IList<EntAgendaPersona> personas)
        {
            if (personas == null) { return new List<EntAgendaPersona>(); }
            HashSet<string> cadena = NegAgendaEquipo.Cadena(personas, yo);
            return personas
                .Where(p => p != null && NegAgendaEquipo.PuedeVer(yo, p.CodUsuario, cadena, verTodos))
                .OrderBy(p => p.Nombre, StringComparer.Create(Es, true))
                .ToList();
        }

        public static List<EntAgendaTarjetaArea> Equipo(string yo, bool verTodos, IList<EntAgendaPersona> personas,
            IList<EntAgendaActividad> actividades, DateTime lunes, DateTime hoy)
        {
            ILookup<string, EntAgendaActividad> porPersona = (actividades ?? new List<EntAgendaActividad>())
                .ToLookup(a => NegAgendaEquipo.Normalizar(a.CodResponsable), NegAgendaEquipo.Codigos);

            return Visibles(yo, verTodos, personas)
                .GroupBy(p => p.IdArea)
                .Select(g =>
                {
                    EntAgendaPersona primera = g.First();
                    List<EntAgendaTarjetaPersona> gente = g.Select(p => new EntAgendaTarjetaPersona
                    {
                        CodUsuario = p.CodUsuario,
                        Nombre = p.Nombre,
                        Iniciales = Iniciales(p.Nombre),
                        Contador = Contar(porPersona[NegAgendaEquipo.Normalizar(p.CodUsuario)], lunes, hoy)
                    }).ToList();
                    return new EntAgendaTarjetaArea
                    {
                        IdArea = primera.IdArea,
                        Area = primera.Area,
                        Color = primera.Color,
                        SinArea = primera.SinArea,
                        Personas = gente,
                        Contador = Sumar(gente.Select(x => x.Contador))
                    };
                })
                .OrderBy(t => t.SinArea)
                .ThenBy(t => t.Area, StringComparer.Create(Es, true))
                .ToList();
        }

        /// <summary>null si yo no puedo ver a esa persona o no esta activa.</summary>
        public static EntAgendaVistaPersona Persona(string yo, bool verTodos, IList<EntAgendaPersona> personas,
            IList<EntAgendaActividad> actividades, string cod, DateTime lunes, DateTime hoy)
        {
            if (personas == null) { return null; }
            HashSet<string> cadena = NegAgendaEquipo.Cadena(personas, yo);
            string buscado = NegAgendaEquipo.Normalizar(cod);
            EntAgendaPersona p = personas.FirstOrDefault(x => x != null && NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(x.CodUsuario), buscado));
            if (p == null || !NegAgendaEquipo.PuedeVer(yo, p.CodUsuario, cadena, verTodos)) { return null; }

            Dictionary<string, string> nombres = personas
                .Where(x => x != null)
                .GroupBy(x => NegAgendaEquipo.Normalizar(x.CodUsuario), NegAgendaEquipo.Codigos)
                .ToDictionary(g => g.Key, g => g.First().Nombre, NegAgendaEquipo.Codigos);

            List<EntAgendaActividad> suyas = (actividades ?? new List<EntAgendaActividad>())
                .Where(a => a != null && !a.Anulada && NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(a.CodResponsable), NegAgendaEquipo.Normalizar(p.CodUsuario)))
                .OrderBy(a => a.FechaEntrega)
                .ThenBy(a => a.IdActividad)
                .ToList();

            bool esPropia = NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(yo), NegAgendaEquipo.Normalizar(p.CodUsuario));
            EntAgendaVistaPersona v = new EntAgendaVistaPersona
            {
                CodUsuario = p.CodUsuario,
                Nombre = p.Nombre,
                Iniciales = Iniciales(p.Nombre),
                Area = p.Area,
                Color = p.Color,
                EsPropia = esPropia,
                PuedeAsignar = esPropia || NegAgendaEquipo.PuedeAsignarA(yo, p.CodUsuario, cadena, verTodos)
            };

            Func<EntAgendaActividad, EntAgendaItem> item = a => Item(yo, verTodos, cadena, nombres, a, hoy);
            v.Atrasadas = suyas.Where(a => NegAgendaActividad.Atrasada(a, hoy)).Select(item).ToList();
            for (int i = 0; i < 5; i++)
            {
                DateTime dia = lunes.Date.AddDays(i);
                v.Dias.Add(new EntAgendaDia
                {
                    Fecha = dia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Nombre = Es.DateTimeFormat.GetDayName(dia.DayOfWeek),
                    EsHoy = dia == hoy.Date,
                    Actividades = suyas.Where(a => a.FechaEntrega.Date == dia).Select(item).ToList()
                });
            }
            return v;
        }

        private static EntAgendaItem Item(string yo, bool verTodos, ISet<string> cadena, Dictionary<string, string> nombres,
            EntAgendaActividad a, DateTime hoy)
        {
            bool propia = NegAgendaActividad.EsPropia(a);
            string asignador = "";
            if (!propia)
            {
                string cod = NegAgendaEquipo.Normalizar(a.CodAsignadoPor);
                if (!nombres.TryGetValue(cod, out asignador)) { asignador = cod; }
            }
            return new EntAgendaItem
            {
                IdActividad = a.IdActividad,
                Titulo = a.Titulo,
                FechaEntrega = a.FechaEntrega.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                AsignadoPor = asignador,
                Propia = propia,
                Completa = NegAgendaActividad.Completa(a),
                Atrasada = NegAgendaActividad.Atrasada(a, hoy),
                PuedeEditar = NegAgendaActividad.PuedeEditar(yo, a, cadena, verTodos),
                PuedeMarcar = NegAgendaActividad.PuedeMarcar(yo, a),
                Entregables = a.Entregables
                    .OrderBy(e => e.Orden)
                    .ThenBy(e => e.IdEntregable)
                    .Select(e => new EntAgendaItemEntregable { IdEntregable = e.IdEntregable, Descripcion = e.Descripcion, Hecho = e.Hecho })
                    .ToList()
            };
        }
    }
}
```

En `CapaNegocio/CapaNegocio.csproj`:
```xml
    <Compile Include="NegAgendaVista.cs" />
```

- [ ] **Step 4: BOM, compilar y probar**

Probar con `/Tests:NegAgendaVistaTests`. Esperado: `Correcto: 13`.

Si `Persona_CincoDiasYHoyMarcado` falla en el nombre del día (por ejemplo `"lunes"` contra `"Lunes"`), la cultura `es-EC` del equipo devuelve otro formato: fijar `Nombre = Es.DateTimeFormat.GetDayName(dia.DayOfWeek).ToLowerInvariant()` y volver a probar.

- [ ] **Step 5: Commit**

```bash
git add CapaNegocio/NegAgendaVista.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegAgendaVistaTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(agenda): contadores, tarjetas de area y agenda de una persona

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 4: Correo y catálogo de áreas (reglas puras)

**Files:**
- Create: `CapaNegocio/NegAgendaCorreo.cs`, `CapaNegocio/NegAgendaAreas.cs`
- Create: `CapaPruebas/NegAgendaCorreoTests.cs`, `CapaPruebas/NegAgendaAreasTests.cs`
- Modify: `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Consumes: `NegAgendaActividad.EsPropia`, `NegAgendaActividadTests.A/Jueves15`, `NegAgendaEquipoTests.P`.
- Produces: `NegAgendaCorreo.DebeAvisar(EntAgendaActividad) : bool`, `Asunto(EntAgendaActividad) : string`, `Enlace(string urlSitio) : string`, `Cuerpo(EntAgendaActividad, string nombreResponsable, string nombreAsignador, string urlSitio) : string`; `NegAgendaAreas.Validar(string nombre, string color) : string`, `MensajeResultado(int) : string`, `ConPersonas(IList<EntAgendaArea>, IList<EntAgendaPersona>) : List<EntAgendaArea>`.

- [ ] **Step 1: Escribir las pruebas**

`CapaPruebas/NegAgendaCorreoTests.cs`:
```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    [TestClass]
    public class NegAgendaCorreoTests
    {
        private static EntAgendaActividad Asignada()
        {
            EntAgendaActividad a = NegAgendaActividadTests.A("C1", "J1", NegAgendaActividadTests.Jueves15, false, false);
            a.Titulo = "Preparar informe mensual";
            a.Entregables[0].Descripcion = "Borrador";
            a.Entregables[1].Descripcion = "Versión final";
            return a;
        }

        [TestMethod]
        public void DebeAvisar_SoloSiLaAsignoOtraPersona()
        {
            Assert.IsTrue(NegAgendaCorreo.DebeAvisar(Asignada()));
            Assert.IsFalse(NegAgendaCorreo.DebeAvisar(NegAgendaActividadTests.A("C1", "C1", NegAgendaActividadTests.Jueves15)));
            Assert.IsFalse(NegAgendaCorreo.DebeAvisar(null));
        }

        [TestMethod]
        public void Asunto_LlevaElTitulo()
        {
            Assert.AreEqual("Nueva actividad asignada: Preparar informe mensual", NegAgendaCorreo.Asunto(Asignada()));
        }

        [TestMethod]
        public void Cuerpo_TituloFechaEntregablesYQuienAsigno()
        {
            string c = NegAgendaCorreo.Cuerpo(Asignada(), "Ana Perez", "Luis Mora", "https://portal.ejemplo/RTareas/");
            StringAssert.Contains(c, "Preparar informe mensual");
            StringAssert.Contains(c, "jueves 15 de octubre de 2026");
            StringAssert.Contains(c, "Borrador");
            StringAssert.Contains(c, "Luis Mora");
            StringAssert.Contains(c, "Ana Perez");
            StringAssert.Contains(c, "https://portal.ejemplo/RTareas/Formulario/Agenda.aspx");
            StringAssert.Contains(c, "Ver en mi agenda");
        }

        [TestMethod]
        public void Cuerpo_SinUrlDelSitio_SinBoton()
        {
            string c = NegAgendaCorreo.Cuerpo(Asignada(), "Ana", "Luis", "  ");
            Assert.IsFalse(c.Contains("Ver en mi agenda"));
        }

        [TestMethod]
        public void Cuerpo_EscapaHtmlYTildes()
        {
            EntAgendaActividad a = Asignada();
            a.Titulo = "<script>alert(1)</script>";
            string c = NegAgendaCorreo.Cuerpo(a, "Verónica", "Luis", "");
            Assert.IsFalse(c.Contains("<script>"));
            StringAssert.Contains(c, "&lt;script&gt;");
            StringAssert.Contains(c, "Ver&#243;nica");
        }

        [TestMethod]
        public void Enlace_QuitaLaBarraFinal()
        {
            Assert.AreEqual("https://x/RTareas/Formulario/Agenda.aspx", NegAgendaCorreo.Enlace("https://x/RTareas//"));
            Assert.AreEqual("", NegAgendaCorreo.Enlace(null));
        }
    }
}
```

`CapaPruebas/NegAgendaAreasTests.cs`:
```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegAgendaAreasTests
    {
        [TestMethod]
        public void Validar_NombreYColor()
        {
            Assert.IsNull(NegAgendaAreas.Validar("Hybrid Cloud", "#0078D4"));
            Assert.IsNull(NegAgendaAreas.Validar("Hybrid Cloud", "#a1b2c3"));
            Assert.AreEqual("Escriba el nombre del área.", NegAgendaAreas.Validar("  ", "#0078D4"));
            Assert.AreEqual("El nombre admite hasta 100 caracteres.", NegAgendaAreas.Validar(new string('a', 101), "#0078D4"));
            Assert.AreEqual("Elija un color válido.", NegAgendaAreas.Validar("Compras", "rojo"));
            Assert.AreEqual("Elija un color válido.", NegAgendaAreas.Validar("Compras", "#12345G"));
            Assert.AreEqual("Elija un color válido.", NegAgendaAreas.Validar("Compras", null));
        }

        [TestMethod]
        public void MensajeResultado_CodigosDelProcedimiento()
        {
            Assert.AreEqual("Ya existe un área con ese nombre.", NegAgendaAreas.MensajeResultado(-1));
            Assert.AreEqual("«Sin área» no se puede renombrar ni desactivar.", NegAgendaAreas.MensajeResultado(-2));
            Assert.AreEqual("El área no existe.", NegAgendaAreas.MensajeResultado(-3));
            Assert.AreEqual("No se pudo guardar el área.", NegAgendaAreas.MensajeResultado(0));
            Assert.IsNull(NegAgendaAreas.MensajeResultado(12));
        }

        [TestMethod]
        public void ConPersonas_CuentaPorArea()
        {
            List<EntAgendaArea> areas = new List<EntAgendaArea>
            {
                new EntAgendaArea { IdArea = 1, Nombre = "A" },
                new EntAgendaArea { IdArea = 2, Nombre = "B" },
                new EntAgendaArea { IdArea = 9, Nombre = "Sin área", EsSinArea = true }
            };
            List<EntAgendaPersona> personas = new List<EntAgendaPersona>
            {
                NegAgendaEquipoTests.P("X", "G", 1), NegAgendaEquipoTests.P("Y", "G", 1), NegAgendaEquipoTests.P("Z", "G", 9, "Sin área", true)
            };
            List<EntAgendaArea> r = NegAgendaAreas.ConPersonas(areas, personas);
            CollectionAssert.AreEqual(new[] { 2, 0, 1 }, r.Select(a => a.Personas).ToList());
        }
    }
}
```

En `CapaPruebas/CapaPruebas.csproj`:
```xml
    <Compile Include="NegAgendaCorreoTests.cs" />
    <Compile Include="NegAgendaAreasTests.cs" />
```

- [ ] **Step 2: Compilar para ver que falla**

Esperado: error porque no existen `NegAgendaCorreo` ni `NegAgendaAreas`.

- [ ] **Step 3: Implementar**

`CapaNegocio/NegAgendaCorreo.cs`:
```csharp
using CapaEntidad;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace CapaNegocio
{
    /// <summary>
    /// El aviso por correo de la agenda. Clase pura: arma el texto; el envio y
    /// la bitacora los hace el handler.
    ///
    /// Solo se avisa cuando alguien asigna una actividad a OTRA persona. Una
    /// actividad propia, un cambio de fecha o una anulacion no envian nada.
    /// Todo lo que viene de la base o de la persona se escapa: el titulo lo
    /// escribe un usuario.
    /// </summary>
    public static class NegAgendaCorreo
    {
        private static readonly CultureInfo Es = new CultureInfo("es-EC");

        public static bool DebeAvisar(EntAgendaActividad a)
        {
            return a != null && !NegAgendaActividad.EsPropia(a);
        }

        public static string Asunto(EntAgendaActividad a)
        {
            return "Nueva actividad asignada: " + (a == null ? "" : (a.Titulo ?? "").Trim());
        }

        /// <summary>URL_SITE + /Formulario/Agenda.aspx; vacio si no hay URL configurada.</summary>
        public static string Enlace(string urlSitio)
        {
            if (string.IsNullOrWhiteSpace(urlSitio)) { return ""; }
            return urlSitio.Trim().TrimEnd('/') + "/Formulario/Agenda.aspx";
        }

        public static string Cuerpo(EntAgendaActividad a, string nombreResponsable, string nombreAsignador, string urlSitio)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#222\">");
            sb.Append("<p>Hola ").Append(H(nombreResponsable)).Append(",</p>");
            sb.Append("<p><b>").Append(H(nombreAsignador)).Append("</b> te asign&oacute; una actividad:</p>");
            sb.Append("<p style=\"font-size:16px\"><b>").Append(H(a.Titulo)).Append("</b></p>");
            sb.Append("<p>Fecha de entrega: <b>").Append(H(a.FechaEntrega.ToString("dddd d 'de' MMMM 'de' yyyy", Es))).Append("</b></p>");

            if (a.Entregables != null && a.Entregables.Count > 0)
            {
                sb.Append("<p>Entregables:</p><ul>");
                foreach (EntAgendaEntregable e in a.Entregables.OrderBy(x => x.Orden))
                {
                    sb.Append("<li>").Append(H(e.Descripcion)).Append("</li>");
                }
                sb.Append("</ul>");
            }

            string enlace = Enlace(urlSitio);
            if (enlace != "")
            {
                sb.Append("<p><a href=\"").Append(H(enlace))
                  .Append("\" style=\"display:inline-block;padding:10px 22px;background:#1F3864;color:#ffffff;text-decoration:none;font-weight:bold\">Ver en mi agenda</a></p>");
            }
            sb.Append("<p style=\"font-size:11px;color:#777\">Correo autom&aacute;tico del Sistema de Tareas. No responder.</p>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string H(string s)
        {
            return WebUtility.HtmlEncode(s ?? "");
        }
    }
}
```

`CapaNegocio/NegAgendaAreas.cs`:
```csharp
using CapaEntidad;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CapaNegocio
{
    /// <summary>
    /// Catalogo de areas de la agenda. Clase pura. Los mensajes viven aca y no
    /// en Sp_RTA_AgendaGuardarArea: el procedimiento devuelve un codigo, para
    /// que el script SQL no lleve tildes.
    /// </summary>
    public static class NegAgendaAreas
    {
        private static readonly Regex ColorValido = new Regex("^#[0-9A-Fa-f]{6}$");

        public static string Validar(string nombre, string color)
        {
            string n = (nombre ?? "").Trim();
            if (n == "") { return "Escriba el nombre del área."; }
            if (n.Length > 100) { return "El nombre admite hasta 100 caracteres."; }
            if (color == null || !ColorValido.IsMatch(color.Trim())) { return "Elija un color válido."; }
            return null;
        }

        /// <summary>null si el resultado es un IdArea (positivo).</summary>
        public static string MensajeResultado(int resultado)
        {
            switch (resultado)
            {
                case -1: return "Ya existe un área con ese nombre.";
                case -2: return "«Sin área» no se puede renombrar ni desactivar.";
                case -3: return "El área no existe.";
            }
            return resultado > 0 ? null : "No se pudo guardar el área.";
        }

        public static List<EntAgendaArea> ConPersonas(IList<EntAgendaArea> areas, IList<EntAgendaPersona> personas)
        {
            Dictionary<int, int> cuenta = (personas ?? new List<EntAgendaPersona>())
                .Where(p => p != null)
                .GroupBy(p => p.IdArea)
                .ToDictionary(g => g.Key, g => g.Count());
            List<EntAgendaArea> r = (areas ?? new List<EntAgendaArea>()).ToList();
            foreach (EntAgendaArea a in r)
            {
                int n;
                a.Personas = cuenta.TryGetValue(a.IdArea, out n) ? n : 0;
            }
            return r;
        }
    }
}
```

En `CapaNegocio/CapaNegocio.csproj`:
```xml
    <Compile Include="NegAgendaCorreo.cs" />
    <Compile Include="NegAgendaAreas.cs" />
```

- [ ] **Step 4: BOM, compilar y probar**

Probar con `/Tests:NegAgendaCorreoTests` y luego con `/Tests:NegAgendaAreasTests`. Esperado: `Correcto: 6` y `Correcto: 3`.

Si `Cuerpo_TituloFechaEntregablesYQuienAsigno` falla solo en la fecha, mostrar el valor real con `System.Console.WriteLine(c)` dentro de la prueba, correr de nuevo y ajustar el formato del cuerpo para que diga exactamente `jueves 15 de octubre de 2026`. No cambiar la prueba.

- [ ] **Step 5: Correr toda la suite**

Probar sin filtro. Esperado: `Correcto: 674` (620 + 54) y ningún error.

- [ ] **Step 6: Commit**

```bash
git add CapaNegocio/NegAgendaCorreo.cs CapaNegocio/NegAgendaAreas.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegAgendaCorreoTests.cs CapaPruebas/NegAgendaAreasTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(agenda): aviso por correo y reglas del catalogo de areas

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 5: Esquema SQL (tablas, «Sin área», procedimientos)

**Files:**
- Create: `docs/sql/2026-10-15-agenda-esquema.sql`

**Interfaces:**
- Produces (los usa `DaoAgenda`, Task 6), todos con `SET NOCOUNT ON`:
  - `Sp_RTA_AgendaPersonasActivas` → columnas `Cod_Usuario, Nombre, Correo, CodJefe, IdArea, Area, Color, EsSinArea`
  - `Sp_RTA_AgendaAreas` → `IdArea, Nombre, Color, Activa, EsSinArea`
  - `Sp_RTA_AgendaGuardarArea @IdArea INT, @Nombre NVARCHAR(100), @Color CHAR(7), @Activa BIT, @Usuario VARCHAR(50)` → `Resultado INT` (IdArea, o -1/-2/-3)
  - `Sp_RTA_AgendaAsignarArea @CodUsuario VARCHAR(20), @IdArea INT, @Usuario VARCHAR(50)`
  - `Sp_RTA_AgendaActividades @Desde DATE, @Hasta DATE, @Hoy DATE` → 2 resultados: actividades y entregables
  - `Sp_RTA_AgendaActividad @IdActividad INT` → los mismos 2 resultados, para una
  - `Sp_RTA_AgendaGuardarActividad @IdActividad INT, @Titulo NVARCHAR(200), @CodResponsable VARCHAR(20), @CodAsignadoPor VARCHAR(20), @FechaEntrega DATE, @Entregables XML, @Usuario VARCHAR(50)` → `IdActividad INT`
  - `Sp_RTA_AgendaMarcarEntregable @IdEntregable INT, @Hecho BIT, @Usuario VARCHAR(50)`
  - `Sp_RTA_AgendaMarcarCompleta @IdActividad INT, @Completa BIT, @Usuario VARCHAR(50)`
  - `Sp_RTA_AgendaAnular @IdActividad INT, @Usuario VARCHAR(50)`
  - `Sp_RTA_AgendaRegistrarCorreo @IdActividad INT, @Destinatario VARCHAR(200), @Asunto NVARCHAR(300), @Enviado BIT, @Error NVARCHAR(1000)`
  - Formato de `@Entregables`: `<es><e id="0" o="1">texto</e>...</es>`; `id="0"` es nuevo.

> **Corrección a la spec, §6:** la acción `Agenda.VerTodos` necesita el `Id_Menu` de la pantalla, que se crea en el script de menú. Por eso se crea en `2026-10-15-agenda-menu.sql` (Task 10), no en este. El Task 10 corrige ese renglón de la spec.

- [ ] **Step 1: Escribir el script**

`docs/sql/2026-10-15-agenda-esquema.sql`:
```sql
/* ============================================================================
   Equipo y agenda (Entrega 1) - esquema
   ReporTarea  |  2026-10-15
   Spec: docs/superpowers/specs/2026-10-15-agenda-equipo-entrega1-design.md
   Crea o actualiza:
     1. Tablas AG_Area, AG_PersonaArea, AG_Actividad, AG_Entregable, AG_CorreoBitacora
     2. La fila "Sin area" de AG_Area
     3. Procedimientos Sp_RTA_Agenda*
   La accion Agenda.VerTodos y el menu van en 2026-10-15-agenda-menu.sql.
   Repetible: correrlo dos veces no duplica nada. No toca tablas existentes.
   Sin tildes a proposito: sqlcmd lee el archivo con la pagina de codigos del
   equipo. Los textos con tilde se arman con NCHAR.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------------------- 1. tablas -------- */
IF OBJECT_ID('dbo.AG_Area', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_Area
    (
        IdArea               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_Area PRIMARY KEY,
        Nombre               NVARCHAR(100) NOT NULL CONSTRAINT UQ_AG_Area_Nombre UNIQUE,
        Color                CHAR(7)       NOT NULL,
        Activa               BIT           NOT NULL CONSTRAINT DF_AG_Area_Activa DEFAULT 1,
        EsSinArea            BIT           NOT NULL CONSTRAINT DF_AG_Area_EsSinArea DEFAULT 0,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_AG_Area_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    PRINT 'AG_Area creada.';
END
GO

IF OBJECT_ID('dbo.AG_PersonaArea', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_PersonaArea
    (
        Cod_Usuario          VARCHAR(20) NOT NULL CONSTRAINT PK_AG_PersonaArea PRIMARY KEY,
        IdArea               INT         NOT NULL CONSTRAINT FK_AG_PersonaArea_Area REFERENCES dbo.AG_Area (IdArea),
        Usuario_Registro     VARCHAR(50) NOT NULL,
        Fecha_Registro       DATETIME    NOT NULL CONSTRAINT DF_AG_PersonaArea_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50) NULL,
        Fecha_Modificacion   DATETIME    NULL
    );
    PRINT 'AG_PersonaArea creada.';
END
GO

IF OBJECT_ID('dbo.AG_Actividad', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_Actividad
    (
        IdActividad          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_Actividad PRIMARY KEY,
        Titulo               NVARCHAR(200) NOT NULL,
        Cod_Responsable      VARCHAR(20)   NOT NULL,
        Cod_AsignadoPor      VARCHAR(20)   NOT NULL,
        FechaEntrega         DATE          NOT NULL,
        CompletadaManual     BIT           NOT NULL CONSTRAINT DF_AG_Actividad_Manual DEFAULT 0,
        FechaCompletada      DATETIME      NULL,
        Anulada              BIT           NOT NULL CONSTRAINT DF_AG_Actividad_Anulada DEFAULT 0,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_AG_Actividad_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    CREATE INDEX IX_AG_Actividad_Fecha ON dbo.AG_Actividad (FechaEntrega)
        INCLUDE (Cod_Responsable, Anulada, FechaCompletada);
    PRINT 'AG_Actividad creada.';
END
GO

IF OBJECT_ID('dbo.AG_Entregable', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_Entregable
    (
        IdEntregable         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_Entregable PRIMARY KEY,
        IdActividad          INT           NOT NULL CONSTRAINT FK_AG_Entregable_Actividad REFERENCES dbo.AG_Actividad (IdActividad),
        Descripcion          NVARCHAR(200) NOT NULL,
        Orden                INT           NOT NULL,
        Hecho                BIT           NOT NULL CONSTRAINT DF_AG_Entregable_Hecho DEFAULT 0,
        Cod_Hecho            VARCHAR(20)   NULL,
        Fecha_Hecho          DATETIME      NULL,
        Usuario_Registro     VARCHAR(50)   NOT NULL,
        Fecha_Registro       DATETIME      NOT NULL CONSTRAINT DF_AG_Entregable_Fecha DEFAULT GETDATE(),
        Usuario_Modificacion VARCHAR(50)   NULL,
        Fecha_Modificacion   DATETIME      NULL
    );
    CREATE INDEX IX_AG_Entregable_Actividad ON dbo.AG_Entregable (IdActividad);
    PRINT 'AG_Entregable creada.';
END
GO

IF OBJECT_ID('dbo.AG_CorreoBitacora', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AG_CorreoBitacora
    (
        IdCorreo     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AG_CorreoBitacora PRIMARY KEY,
        IdActividad  INT            NOT NULL CONSTRAINT FK_AG_CorreoBitacora_Actividad REFERENCES dbo.AG_Actividad (IdActividad),
        Destinatario VARCHAR(200)   NULL,
        Asunto       NVARCHAR(300)  NOT NULL,
        Enviado      BIT            NOT NULL,
        Error        NVARCHAR(1000) NULL,
        Fecha        DATETIME       NOT NULL CONSTRAINT DF_AG_CorreoBitacora_Fecha DEFAULT GETDATE()
    );
    PRINT 'AG_CorreoBitacora creada.';
END
GO

/* ------------------------------------------------- 2. "Sin area" ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.AG_Area WHERE EsSinArea = 1)
BEGIN
    INSERT INTO dbo.AG_Area (Nombre, Color, Activa, EsSinArea, Usuario_Registro)
    VALUES (N'Sin ' + NCHAR(225) + N'rea', '#8A8886', 1, 1, 'MIGRACION');
    PRINT 'Fila Sin area creada.';
END
GO

/* ------------------------------------------------- 3. procedimientos ------ */
IF OBJECT_ID('dbo.Sp_RTA_AgendaPersonasActivas', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaPersonasActivas AS RETURN 0;');
GO
/* El UNICO lugar que dice quien es "activo" en la agenda: el criterio de los
   selectores de jefe del sistema (Usuario_Estado = 'A' y EstadoUsuario NULL),
   solo usuarios internos. Si un Cod_Usuario esta repetido, la cuenta mas
   reciente. Sin area, o con su area inactiva: la fila "Sin area". */
ALTER PROCEDURE dbo.Sp_RTA_AgendaPersonasActivas
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @IdSin INT, @NomSin NVARCHAR(100), @ColSin CHAR(7);
    SELECT TOP 1 @IdSin = IdArea, @NomSin = Nombre, @ColSin = Color
      FROM dbo.AG_Area WHERE EsSinArea = 1;

    ;WITH U AS
    (
        SELECT Cod_Usuario = LTRIM(RTRIM(u.Cod_Usuario)),
               Nombre      = LTRIM(RTRIM(ISNULL(u.Nom_Usuario, ''))),
               Correo      = LTRIM(RTRIM(ISNULL(u.E_Mail, ''))),
               CodJefe     = LTRIM(RTRIM(ISNULL(u.Cod_Jefe_Inm, ''))),
               Orden       = ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(u.Cod_Usuario)) ORDER BY u.Id_Usuario DESC)
          FROM dbo.R_Usuarios AS u
         WHERE u.Usuario_Estado = 'A'
           AND u.EstadoUsuario IS NULL
           AND ISNULL(u.IdCliente, 0) = 0
           AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
    )
    SELECT U.Cod_Usuario, U.Nombre, U.Correo, U.CodJefe,
           IdArea    = ISNULL(a.IdArea, @IdSin),
           Area      = ISNULL(a.Nombre, @NomSin),
           Color     = ISNULL(a.Color, @ColSin),
           EsSinArea = CONVERT(BIT, CASE WHEN a.IdArea IS NULL THEN 1 ELSE 0 END)
      FROM U
      LEFT JOIN dbo.AG_PersonaArea AS pa ON pa.Cod_Usuario = U.Cod_Usuario
      LEFT JOIN dbo.AG_Area AS a ON a.IdArea = pa.IdArea AND a.Activa = 1 AND a.EsSinArea = 0
     WHERE U.Orden = 1
     ORDER BY U.Nombre;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaAreas', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaAreas AS RETURN 0;');
GO
ALTER PROCEDURE dbo.Sp_RTA_AgendaAreas
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdArea, Nombre, Color, Activa, EsSinArea
      FROM dbo.AG_Area
     ORDER BY EsSinArea, Nombre;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaGuardarArea', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaGuardarArea AS RETURN 0;');
GO
/* Devuelve el IdArea, o un codigo que CapaNegocio (NegAgendaAreas) traduce:
   -1 nombre repetido, -2 se intento renombrar o desactivar "Sin area",
   -3 el area no existe. @IdArea = 0 crea. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaGuardarArea
    @IdArea  INT,
    @Nombre  NVARCHAR(100),
    @Color   CHAR(7),
    @Activa  BIT,
    @Usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET @Nombre = LTRIM(RTRIM(@Nombre));
    SET @IdArea = ISNULL(@IdArea, 0);

    IF EXISTS (SELECT 1 FROM dbo.AG_Area WHERE Nombre = @Nombre AND IdArea <> @IdArea)
    BEGIN
        SELECT Resultado = -1;
        RETURN;
    END

    IF @IdArea = 0
    BEGIN
        INSERT INTO dbo.AG_Area (Nombre, Color, Activa, EsSinArea, Usuario_Registro)
        VALUES (@Nombre, @Color, @Activa, 0, @Usuario);
        SELECT Resultado = CONVERT(INT, SCOPE_IDENTITY());
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.AG_Area WHERE IdArea = @IdArea)
    BEGIN
        SELECT Resultado = -3;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.AG_Area
                WHERE IdArea = @IdArea AND EsSinArea = 1
                  AND (Nombre <> @Nombre OR @Activa = 0))
    BEGIN
        SELECT Resultado = -2;
        RETURN;
    END

    UPDATE dbo.AG_Area
       SET Nombre = @Nombre, Color = @Color, Activa = @Activa,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdArea = @IdArea;
    SELECT Resultado = @IdArea;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaAsignarArea', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaAsignarArea AS RETURN 0;');
GO
/* Asignar "Sin area" borra la fila: sin fila, la persona cae en "Sin area". */
ALTER PROCEDURE dbo.Sp_RTA_AgendaAsignarArea
    @CodUsuario VARCHAR(20),
    @IdArea     INT,
    @Usuario    VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET @CodUsuario = LTRIM(RTRIM(@CodUsuario));

    IF EXISTS (SELECT 1 FROM dbo.AG_Area WHERE IdArea = @IdArea AND EsSinArea = 1)
    BEGIN
        DELETE FROM dbo.AG_PersonaArea WHERE Cod_Usuario = @CodUsuario;
        RETURN;
    END

    UPDATE dbo.AG_PersonaArea
       SET IdArea = @IdArea, Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE Cod_Usuario = @CodUsuario;
    IF @@ROWCOUNT = 0
        INSERT INTO dbo.AG_PersonaArea (Cod_Usuario, IdArea, Usuario_Registro)
        VALUES (@CodUsuario, @IdArea, @Usuario);
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaRecalcular', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaRecalcular AS RETURN 0;');
GO
/* Mantiene FechaCompletada: se llena la primera vez que la actividad queda
   completa y se vacia si deja de estarlo. La regla es la de
   NegAgendaActividad.Completa: con entregables, todos hechos; sin
   entregables, la marca manual. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaRecalcular
    @IdActividad INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE a
       SET FechaCompletada =
           CASE WHEN (CASE WHEN EXISTS (SELECT 1 FROM dbo.AG_Entregable e WHERE e.IdActividad = a.IdActividad)
                           THEN CASE WHEN NOT EXISTS (SELECT 1 FROM dbo.AG_Entregable e
                                                       WHERE e.IdActividad = a.IdActividad AND e.Hecho = 0)
                                     THEN 1 ELSE 0 END
                           ELSE CONVERT(INT, a.CompletadaManual) END) = 1
                THEN ISNULL(a.FechaCompletada, GETDATE())
                ELSE NULL END
      FROM dbo.AG_Actividad AS a
     WHERE a.IdActividad = @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaActividades', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaActividades AS RETURN 0;');
GO
/* Las no anuladas que vencen en el rango, mas las atrasadas de cualquier
   fecha (sin completar y vencidas antes de @Hoy). Dos resultados:
   actividades y entregables. Quien ve que lo decide CapaNegocio. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaActividades
    @Desde DATE,
    @Hasta DATE,
    @Hoy   DATE
AS
BEGIN
    SET NOCOUNT ON;
    CREATE TABLE #Ids (IdActividad INT PRIMARY KEY);
    INSERT INTO #Ids (IdActividad)
    SELECT a.IdActividad
      FROM dbo.AG_Actividad AS a
     WHERE a.Anulada = 0
       AND (   a.FechaEntrega BETWEEN @Desde AND @Hasta
            OR (a.FechaEntrega < @Hoy AND a.FechaCompletada IS NULL));

    SELECT a.IdActividad, a.Titulo, a.Cod_Responsable, a.Cod_AsignadoPor, a.FechaEntrega,
           a.CompletadaManual, a.FechaCompletada, a.Anulada
      FROM dbo.AG_Actividad AS a
      JOIN #Ids AS i ON i.IdActividad = a.IdActividad
     ORDER BY a.FechaEntrega, a.IdActividad;

    SELECT e.IdEntregable, e.IdActividad, e.Descripcion, e.Orden, e.Hecho
      FROM dbo.AG_Entregable AS e
      JOIN #Ids AS i ON i.IdActividad = e.IdActividad
     ORDER BY e.IdActividad, e.Orden, e.IdEntregable;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaActividad', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaActividad AS RETURN 0;');
GO
/* Una sola, aunque este anulada: el handler decide con ella si se puede tocar. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaActividad
    @IdActividad INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.IdActividad, a.Titulo, a.Cod_Responsable, a.Cod_AsignadoPor, a.FechaEntrega,
           a.CompletadaManual, a.FechaCompletada, a.Anulada
      FROM dbo.AG_Actividad AS a
     WHERE a.IdActividad = @IdActividad;

    SELECT e.IdEntregable, e.IdActividad, e.Descripcion, e.Orden, e.Hecho
      FROM dbo.AG_Entregable AS e
     WHERE e.IdActividad = @IdActividad
     ORDER BY e.Orden, e.IdEntregable;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaGuardarActividad', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaGuardarActividad AS RETURN 0;');
GO
/* @IdActividad = 0 crea; si no, actualiza titulo y fecha (responsable y quien
   asigno no cambian). @Entregables es la lista COMPLETA:
   <es><e id="5" o="1">texto</e><e id="0" o="2">nuevo</e></es>
   Los que no vienen se borran, los que tienen id se actualizan (conservan
   Hecho) y los de id 0 se crean. Devuelve el IdActividad. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaGuardarActividad
    @IdActividad    INT,
    @Titulo         NVARCHAR(200),
    @CodResponsable VARCHAR(20),
    @CodAsignadoPor VARCHAR(20),
    @FechaEntrega   DATE,
    @Entregables    XML,
    @Usuario        VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @E TABLE (IdEntregable INT NOT NULL, Orden INT NOT NULL, Descripcion NVARCHAR(200) NOT NULL);
    INSERT INTO @E (IdEntregable, Orden, Descripcion)
    SELECT ISNULL(x.value('@id', 'INT'), 0),
           ISNULL(x.value('@o', 'INT'), 0),
           LEFT(LTRIM(RTRIM(x.value('.', 'NVARCHAR(400)'))), 200)
      FROM @Entregables.nodes('/es/e') AS t(x);

    BEGIN TRANSACTION;

    IF ISNULL(@IdActividad, 0) = 0
    BEGIN
        INSERT INTO dbo.AG_Actividad (Titulo, Cod_Responsable, Cod_AsignadoPor, FechaEntrega, Usuario_Registro)
        VALUES (LTRIM(RTRIM(@Titulo)), LTRIM(RTRIM(@CodResponsable)), LTRIM(RTRIM(@CodAsignadoPor)), @FechaEntrega, @Usuario);
        SET @IdActividad = CONVERT(INT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.AG_Actividad
           SET Titulo = LTRIM(RTRIM(@Titulo)), FechaEntrega = @FechaEntrega,
               Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
         WHERE IdActividad = @IdActividad AND Anulada = 0;
    END

    DELETE e
      FROM dbo.AG_Entregable AS e
     WHERE e.IdActividad = @IdActividad
       AND NOT EXISTS (SELECT 1 FROM @E AS x WHERE x.IdEntregable = e.IdEntregable);

    UPDATE e
       SET Descripcion = x.Descripcion, Orden = x.Orden,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
      FROM dbo.AG_Entregable AS e
      JOIN @E AS x ON x.IdEntregable = e.IdEntregable
     WHERE e.IdActividad = @IdActividad;

    INSERT INTO dbo.AG_Entregable (IdActividad, Descripcion, Orden, Usuario_Registro)
    SELECT @IdActividad, Descripcion, Orden, @Usuario
      FROM @E
     WHERE IdEntregable = 0;

    EXEC dbo.Sp_RTA_AgendaRecalcular @IdActividad;

    COMMIT TRANSACTION;
    SELECT IdActividad = @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaMarcarEntregable', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaMarcarEntregable AS RETURN 0;');
GO
ALTER PROCEDURE dbo.Sp_RTA_AgendaMarcarEntregable
    @IdEntregable INT,
    @Hecho        BIT,
    @Usuario      VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @IdActividad INT;
    SELECT @IdActividad = IdActividad FROM dbo.AG_Entregable WHERE IdEntregable = @IdEntregable;

    UPDATE dbo.AG_Entregable
       SET Hecho       = @Hecho,
           Cod_Hecho   = CASE WHEN @Hecho = 1 THEN @Usuario END,
           Fecha_Hecho = CASE WHEN @Hecho = 1 THEN GETDATE() END,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdEntregable = @IdEntregable;

    IF @IdActividad IS NOT NULL EXEC dbo.Sp_RTA_AgendaRecalcular @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaMarcarCompleta', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaMarcarCompleta AS RETURN 0;');
GO
/* Solo para actividades sin entregables: con entregables, se completa
   marcandolos. El handler ya lo comprueba; esto es el respaldo. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaMarcarCompleta
    @IdActividad INT,
    @Completa    BIT,
    @Usuario     VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE a
       SET CompletadaManual = @Completa,
           Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
      FROM dbo.AG_Actividad AS a
     WHERE a.IdActividad = @IdActividad
       AND a.Anulada = 0
       AND NOT EXISTS (SELECT 1 FROM dbo.AG_Entregable e WHERE e.IdActividad = a.IdActividad);
    EXEC dbo.Sp_RTA_AgendaRecalcular @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaAnular', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaAnular AS RETURN 0;');
GO
ALTER PROCEDURE dbo.Sp_RTA_AgendaAnular
    @IdActividad INT,
    @Usuario     VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AG_Actividad
       SET Anulada = 1, Usuario_Modificacion = @Usuario, Fecha_Modificacion = GETDATE()
     WHERE IdActividad = @IdActividad;
END
GO

IF OBJECT_ID('dbo.Sp_RTA_AgendaRegistrarCorreo', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_AgendaRegistrarCorreo AS RETURN 0;');
GO
/* Se llama DESPUES de saber si el correo salio. */
ALTER PROCEDURE dbo.Sp_RTA_AgendaRegistrarCorreo
    @IdActividad  INT,
    @Destinatario VARCHAR(200),
    @Asunto       NVARCHAR(300),
    @Enviado      BIT,
    @Error        NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AG_CorreoBitacora (IdActividad, Destinatario, Asunto, Enviado, Error)
    VALUES (@IdActividad, @Destinatario, ISNULL(@Asunto, N''), @Enviado, LEFT(@Error, 1000));
END
GO

/* ------------------------------------------------- veredicto -------------- */
DECLARE @Faltan INT =
    (SELECT COUNT(*) FROM (VALUES
        ('dbo.AG_Area', 'U'), ('dbo.AG_PersonaArea', 'U'), ('dbo.AG_Actividad', 'U'),
        ('dbo.AG_Entregable', 'U'), ('dbo.AG_CorreoBitacora', 'U'),
        ('dbo.Sp_RTA_AgendaPersonasActivas', 'P'), ('dbo.Sp_RTA_AgendaAreas', 'P'),
        ('dbo.Sp_RTA_AgendaGuardarArea', 'P'), ('dbo.Sp_RTA_AgendaAsignarArea', 'P'),
        ('dbo.Sp_RTA_AgendaRecalcular', 'P'), ('dbo.Sp_RTA_AgendaActividades', 'P'),
        ('dbo.Sp_RTA_AgendaActividad', 'P'), ('dbo.Sp_RTA_AgendaGuardarActividad', 'P'),
        ('dbo.Sp_RTA_AgendaMarcarEntregable', 'P'), ('dbo.Sp_RTA_AgendaMarcarCompleta', 'P'),
        ('dbo.Sp_RTA_AgendaAnular', 'P'), ('dbo.Sp_RTA_AgendaRegistrarCorreo', 'P')
     ) AS o (Nombre, Tipo)
     WHERE OBJECT_ID(o.Nombre, o.Tipo) IS NULL);
DECLARE @SinArea INT = (SELECT COUNT(*) FROM dbo.AG_Area WHERE EsSinArea = 1 AND Activa = 1);
SELECT ObjetosFaltantes = @Faltan, FilasSinArea = @SinArea;
SELECT Veredicto = CASE WHEN @Faltan = 0 AND @SinArea = 1 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA' END;
GO
```

- [ ] **Step 2: Pedir autorización y correr el script en la base**

Decir al usuario: «El Task 5 crea las tablas `AG_*` y 13 procedimientos `Sp_RTA_Agenda*` en ReporTarea (producción). No toca nada existente. ¿Lo corro?». Esperar el sí. Correr con el comando «Correr un script SQL» y `<script>` = `2026-10-15-agenda-esquema`.
Esperado: los `PRINT` de creación y la última línea `VEREDICTO: OK`. Correrlo una segunda vez: debe dar `VEREDICTO: OK` sin ningún `PRINT ... creada`.

- [ ] **Step 3: Probar los procedimientos dentro de una transacción que se deshace**

Crear `<scratchpad>/agenda-humo.sql` (no se commitea) y correrlo con el mismo comando. No deja datos: todo termina en `ROLLBACK`.
```sql
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
BEGIN TRANSACTION;
DECLARE @Id INT;
DECLARE @T TABLE (IdActividad INT);
INSERT @T EXEC dbo.Sp_RTA_AgendaGuardarActividad 0, N'Prueba de humo', 'ZZPRUEBA', 'ZZJEFE', '2026-10-16',
    N'<es><e id="0" o="1">Uno</e><e id="0" o="2">Dos</e></es>', 'ZZPRUEBA';
SELECT @Id = IdActividad FROM @T;
EXEC dbo.Sp_RTA_AgendaActividad @Id;                                   -- 1 actividad, 2 entregables
DECLARE @E1 INT = (SELECT MIN(IdEntregable) FROM dbo.AG_Entregable WHERE IdActividad = @Id);
DECLARE @E2 INT = (SELECT MAX(IdEntregable) FROM dbo.AG_Entregable WHERE IdActividad = @Id);
EXEC dbo.Sp_RTA_AgendaMarcarEntregable @E1, 1, 'ZZPRUEBA';
EXEC dbo.Sp_RTA_AgendaMarcarEntregable @E2, 1, 'ZZPRUEBA';
SELECT Paso = 'completa tras marcar los dos', FechaCompletada FROM dbo.AG_Actividad WHERE IdActividad = @Id;   -- con fecha
DECLARE @X NVARCHAR(400) = N'<es><e id="' + CONVERT(NVARCHAR(10), @E1) + N'" o="1">Uno editado</e><e id="0" o="2">Tres</e></es>';
DECLARE @XX XML = CONVERT(XML, @X);
INSERT @T EXEC dbo.Sp_RTA_AgendaGuardarActividad @Id, N'Prueba editada', 'ZZPRUEBA', 'ZZJEFE', '2026-10-19', @XX, 'ZZPRUEBA';
SELECT Paso = 'entregables tras editar', IdEntregable, Descripcion, Orden, Hecho FROM dbo.AG_Entregable WHERE IdActividad = @Id ORDER BY Orden;  -- Uno editado (Hecho 1), Tres (Hecho 0); Dos borrado
SELECT Paso = 'ya no completa', FechaCompletada FROM dbo.AG_Actividad WHERE IdActividad = @Id;           -- NULL
EXEC dbo.Sp_RTA_AgendaActividades '2026-10-19', '2026-10-23', '2026-10-15';                                -- la incluye
EXEC dbo.Sp_RTA_AgendaAnular @Id, 'ZZPRUEBA';
SELECT Paso = 'anulada', Anulada FROM dbo.AG_Actividad WHERE IdActividad = @Id;                           -- 1
EXEC dbo.Sp_RTA_AgendaRegistrarCorreo @Id, NULL, N'Asunto', 0, N'La persona no tiene correo registrado.';
SELECT Paso = 'bitacora', COUNT(*) FROM dbo.AG_CorreoBitacora WHERE IdActividad = @Id;                     -- 1
DECLARE @R TABLE (Resultado INT);
INSERT @R EXEC dbo.Sp_RTA_AgendaGuardarArea 0, N'ZZ Area de prueba', '#123456', 1, 'ZZPRUEBA';
INSERT @R EXEC dbo.Sp_RTA_AgendaGuardarArea 0, N'ZZ Area de prueba', '#123456', 1, 'ZZPRUEBA';           -- -1
DECLARE @Sin INT = (SELECT IdArea FROM dbo.AG_Area WHERE EsSinArea = 1);
INSERT @R EXEC dbo.Sp_RTA_AgendaGuardarArea @Sin, N'Otro nombre', '#123456', 1, 'ZZPRUEBA';               -- -2
SELECT Paso = 'areas', Resultado FROM @R;
EXEC dbo.Sp_RTA_AgendaPersonasActivas;                                                                      -- ~105 filas
ROLLBACK TRANSACTION;
SELECT Quedo = COUNT(*) FROM dbo.AG_Actividad WHERE Cod_Responsable = 'ZZPRUEBA';                         -- 0
```
Esperado: lo indicado en cada comentario. Si algo no coincide, corregir el procedimiento en el script del Step 1, volver a correr el script (con la misma autorización) y repetir este paso.

- [ ] **Step 4: Commit**

```bash
git add docs/sql/2026-10-15-agenda-esquema.sql
git commit -m "feat(agenda): esquema SQL, fila Sin area y procedimientos

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 6: Acceso a datos (`DaoAgenda` y `NegAgenda.Datos`)

**Files:**
- Create: `CapaDato/DaoAgenda.cs`
- Create: `CapaNegocio/NegAgenda.Datos.cs`
- Modify: `CapaDato/CapaDato.csproj` (junto a `<Compile Include="DaoPermisos.cs" />`), `CapaNegocio/CapaNegocio.csproj`

**Interfaces:**
- Consumes: los procedimientos del Task 5; `DaoReporTareaAranda.conectar()`.
- Produces (`NegAgenda`, estáticos, lanzan si la base falla):
  - `PersonasActivas() : List<EntAgendaPersona>`
  - `Areas() : List<EntAgendaArea>`
  - `GuardarArea(int idArea, string nombre, string color, bool activa, string usuario) : int`
  - `AsignarArea(string codUsuario, int idArea, string usuario) : void`
  - `Actividades(DateTime desde, DateTime hasta, DateTime hoy) : List<EntAgendaActividad>`
  - `Actividad(int idActividad) : EntAgendaActividad` (null si no existe)
  - `GuardarActividad(int idActividad, string titulo, string codResponsable, string codAsignadoPor, DateTime fechaEntrega, IList<EntAgendaEntregable> entregables, string usuario) : int`
  - `MarcarEntregable(int idEntregable, bool hecho, string usuario) : void`
  - `MarcarCompleta(int idActividad, bool completa, string usuario) : void`
  - `Anular(int idActividad, string usuario) : void`
  - `RegistrarCorreo(int idActividad, string destinatario, string asunto, bool enviado, string error) : void`

`CapaDato` no se puede probar fuera de la aplicación web (lee la cadena de conexión de `ConfigurationManager`). La prueba de los procedimientos ya se hizo en el Task 5; la de esta capa es la compilación y la prueba en navegador del Task 11.

- [ ] **Step 1: Escribir `DaoAgenda`**

`CapaDato/DaoAgenda.cs`:
```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CapaDato
{
    /// <summary>
    /// Llamadas a los Sp_RTA_Agenda* (docs/sql/2026-10-15-agenda-esquema.sql).
    /// No decide nada. Las consultas de actividades devuelven dos resultados;
    /// si falta el de entregables se lanza: un "sin entregables" silencioso
    /// mostraria completas cosas que no lo estan.
    /// </summary>
    public class DaoAgenda
    {
        private static SqlConnection Conexion()
        {
            return new DaoReporTareaAranda().conectar();
        }

        private static SqlCommand Comando(SqlConnection cnx, string procedimiento)
        {
            SqlCommand cmd = new SqlCommand(procedimiento, cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            return cmd;
        }

        private static string Texto(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? "" : Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
        }

        private static bool Bit(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v != DBNull.Value && Convert.ToBoolean(v, CultureInfo.InvariantCulture);
        }

        public static List<EntAgendaPersona> PersonasActivas()
        {
            List<EntAgendaPersona> lista = new List<EntAgendaPersona>();
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaPersonasActivas"))
            {
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntAgendaPersona
                        {
                            CodUsuario = Texto(dr, "Cod_Usuario"),
                            Nombre = Texto(dr, "Nombre"),
                            Correo = Texto(dr, "Correo"),
                            CodJefe = Texto(dr, "CodJefe"),
                            IdArea = Convert.ToInt32(dr["IdArea"], CultureInfo.InvariantCulture),
                            Area = Texto(dr, "Area"),
                            Color = Texto(dr, "Color"),
                            SinArea = Bit(dr, "EsSinArea")
                        });
                    }
                }
            }
            return lista;
        }

        public static List<EntAgendaArea> Areas()
        {
            List<EntAgendaArea> lista = new List<EntAgendaArea>();
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaAreas"))
            {
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntAgendaArea
                        {
                            IdArea = Convert.ToInt32(dr["IdArea"], CultureInfo.InvariantCulture),
                            Nombre = Texto(dr, "Nombre"),
                            Color = Texto(dr, "Color"),
                            Activa = Bit(dr, "Activa"),
                            EsSinArea = Bit(dr, "EsSinArea")
                        });
                    }
                }
            }
            return lista;
        }

        public static int GuardarArea(int idArea, string nombre, string color, bool activa, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaGuardarArea"))
            {
                cmd.Parameters.Add("@IdArea", SqlDbType.Int).Value = idArea;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
                cmd.Parameters.Add("@Color", SqlDbType.Char, 7).Value = color;
                cmd.Parameters.Add("@Activa", SqlDbType.Bit).Value = activa;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        public static void AsignarArea(string codUsuario, int idArea, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaAsignarArea"))
            {
                cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 20).Value = codUsuario;
                cmd.Parameters.Add("@IdArea", SqlDbType.Int).Value = idArea;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static List<EntAgendaActividad> Actividades(DateTime desde, DateTime hasta, DateTime hoy)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaActividades"))
            {
                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date;
                cmd.Parameters.Add("@Hoy", SqlDbType.Date).Value = hoy.Date;
                cnx.Open();
                return LeerActividades(cmd, "Sp_RTA_AgendaActividades");
            }
        }

        public static EntAgendaActividad Actividad(int idActividad)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaActividad"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cnx.Open();
                return LeerActividades(cmd, "Sp_RTA_AgendaActividad").FirstOrDefault();
            }
        }

        private static List<EntAgendaActividad> LeerActividades(SqlCommand cmd, string procedimiento)
        {
            List<EntAgendaActividad> lista = new List<EntAgendaActividad>();
            Dictionary<int, EntAgendaActividad> porId = new Dictionary<int, EntAgendaActividad>();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    EntAgendaActividad a = new EntAgendaActividad
                    {
                        IdActividad = Convert.ToInt32(dr["IdActividad"], CultureInfo.InvariantCulture),
                        Titulo = Texto(dr, "Titulo"),
                        CodResponsable = Texto(dr, "Cod_Responsable"),
                        CodAsignadoPor = Texto(dr, "Cod_AsignadoPor"),
                        FechaEntrega = Convert.ToDateTime(dr["FechaEntrega"], CultureInfo.InvariantCulture),
                        CompletadaManual = Bit(dr, "CompletadaManual"),
                        FechaCompletada = dr["FechaCompletada"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(dr["FechaCompletada"], CultureInfo.InvariantCulture),
                        Anulada = Bit(dr, "Anulada")
                    };
                    lista.Add(a);
                    porId[a.IdActividad] = a;
                }
                if (!dr.NextResult())
                {
                    throw new InvalidOperationException(procedimiento + " no devolvió los entregables.");
                }
                while (dr.Read())
                {
                    EntAgendaActividad a;
                    if (!porId.TryGetValue(Convert.ToInt32(dr["IdActividad"], CultureInfo.InvariantCulture), out a)) { continue; }
                    a.Entregables.Add(new EntAgendaEntregable
                    {
                        IdEntregable = Convert.ToInt32(dr["IdEntregable"], CultureInfo.InvariantCulture),
                        IdActividad = a.IdActividad,
                        Descripcion = Texto(dr, "Descripcion"),
                        Orden = Convert.ToInt32(dr["Orden"], CultureInfo.InvariantCulture),
                        Hecho = Bit(dr, "Hecho")
                    });
                }
            }
            return lista;
        }

        public static int GuardarActividad(int idActividad, string titulo, string codResponsable, string codAsignadoPor,
            DateTime fechaEntrega, IList<EntAgendaEntregable> entregables, string usuario)
        {
            XElement xml = new XElement("es",
                (entregables ?? new List<EntAgendaEntregable>()).Select(e => new XElement("e",
                    new XAttribute("id", e.IdEntregable),
                    new XAttribute("o", e.Orden),
                    e.Descripcion ?? "")));

            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaGuardarActividad"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cmd.Parameters.Add("@Titulo", SqlDbType.NVarChar, 200).Value = titulo;
                cmd.Parameters.Add("@CodResponsable", SqlDbType.VarChar, 20).Value = codResponsable;
                cmd.Parameters.Add("@CodAsignadoPor", SqlDbType.VarChar, 20).Value = codAsignadoPor;
                cmd.Parameters.Add("@FechaEntrega", SqlDbType.Date).Value = fechaEntrega.Date;
                cmd.Parameters.Add("@Entregables", SqlDbType.Xml).Value = xml.ToString(SaveOptions.DisableFormatting);
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        public static void MarcarEntregable(int idEntregable, bool hecho, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaMarcarEntregable"))
            {
                cmd.Parameters.Add("@IdEntregable", SqlDbType.Int).Value = idEntregable;
                cmd.Parameters.Add("@Hecho", SqlDbType.Bit).Value = hecho;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void MarcarCompleta(int idActividad, bool completa, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaMarcarCompleta"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cmd.Parameters.Add("@Completa", SqlDbType.Bit).Value = completa;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void Anular(int idActividad, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaAnular"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void RegistrarCorreo(int idActividad, string destinatario, string asunto, bool enviado, string error)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaRegistrarCorreo"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cmd.Parameters.Add("@Destinatario", SqlDbType.VarChar, 200).Value = (object)destinatario ?? DBNull.Value;
                cmd.Parameters.Add("@Asunto", SqlDbType.NVarChar, 300).Value = asunto ?? "";
                cmd.Parameters.Add("@Enviado", SqlDbType.Bit).Value = enviado;
                cmd.Parameters.Add("@Error", SqlDbType.NVarChar, 1000).Value = (object)error ?? DBNull.Value;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
```

En `CapaDato/CapaDato.csproj`, después de `<Compile Include="DaoPermisos.cs" />`:
```xml
    <Compile Include="DaoAgenda.cs" />
```

- [ ] **Step 2: Escribir `NegAgenda.Datos`**

`CapaNegocio/NegAgenda.Datos.cs`:
```csharp
using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Unico archivo de la agenda que toca la base. Las reglas (NegAgendaEquipo,
    /// NegAgendaActividad, NegAgendaVista, NegAgendaCorreo, NegAgendaAreas) son
    /// puras. Todo lanza si la base falla: quien llama decide que responder.
    /// </summary>
    public static partial class NegAgenda
    {
        public static List<EntAgendaPersona> PersonasActivas()
        {
            return DaoAgenda.PersonasActivas();
        }

        public static List<EntAgendaArea> Areas()
        {
            return DaoAgenda.Areas();
        }

        public static int GuardarArea(int idArea, string nombre, string color, bool activa, string usuario)
        {
            return DaoAgenda.GuardarArea(idArea, nombre, color, activa, usuario);
        }

        public static void AsignarArea(string codUsuario, int idArea, string usuario)
        {
            DaoAgenda.AsignarArea(codUsuario, idArea, usuario);
        }

        public static List<EntAgendaActividad> Actividades(DateTime desde, DateTime hasta, DateTime hoy)
        {
            return DaoAgenda.Actividades(desde, hasta, hoy);
        }

        public static EntAgendaActividad Actividad(int idActividad)
        {
            return DaoAgenda.Actividad(idActividad);
        }

        public static int GuardarActividad(int idActividad, string titulo, string codResponsable, string codAsignadoPor,
            DateTime fechaEntrega, IList<EntAgendaEntregable> entregables, string usuario)
        {
            return DaoAgenda.GuardarActividad(idActividad, titulo, codResponsable, codAsignadoPor, fechaEntrega, entregables, usuario);
        }

        public static void MarcarEntregable(int idEntregable, bool hecho, string usuario)
        {
            DaoAgenda.MarcarEntregable(idEntregable, hecho, usuario);
        }

        public static void MarcarCompleta(int idActividad, bool completa, string usuario)
        {
            DaoAgenda.MarcarCompleta(idActividad, completa, usuario);
        }

        public static void Anular(int idActividad, string usuario)
        {
            DaoAgenda.Anular(idActividad, usuario);
        }

        public static void RegistrarCorreo(int idActividad, string destinatario, string asunto, bool enviado, string error)
        {
            DaoAgenda.RegistrarCorreo(idActividad, destinatario, asunto, enviado, error);
        }
    }
}
```

En `CapaNegocio/CapaNegocio.csproj`:
```xml
    <Compile Include="NegAgenda.Datos.cs" />
```

- [ ] **Step 3: BOM, compilar y correr toda la suite**

Esperado: compila sin errores y la suite completa sigue en `Correcto: 674`.

- [ ] **Step 4: Commit**

```bash
git add CapaDato/DaoAgenda.cs CapaDato/CapaDato.csproj CapaNegocio/NegAgenda.Datos.cs CapaNegocio/CapaNegocio.csproj
git commit -m "feat(agenda): acceso a datos de la agenda

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 7: Handler, constantes de Permisos y envío de correo

**Files:**
- Create: `ReporteTareas/Formulario/AdministrarAgenda.ashx`, `ReporteTareas/Formulario/AdministrarAgenda.ashx.cs`
- Modify: `ReporteTareas/clases/Permisos.cs` (clases `Pantallas` y `Acciones`)
- Modify: `ReporteTareas/clases/EnvioCorreoHelper.cs` (`EnviarCorreoPermiso`, línea ~1632, y un método nuevo)
- Modify: `ReporteTareas/ReporteTareas.csproj` (junto a las entradas de `AdministrarTableroCostos.ashx`)

**Interfaces:**
- Consumes: todo `NegAgenda*`; `Permisos.Autenticado/PuedeEntrar/PuedeHacer`; `NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE")`.
- Produces:
  - `Pantallas.Agenda = "Agenda.aspx"`, `Pantallas.ParametrizacionAreasAgenda = "ParametrizacionAreasAgenda.aspx"`, `Acciones.AgendaVerTodos = "Agenda.VerTodos"`.
  - `EnvioCorreoHelper.EnviarCorreoAgenda(string destinatario, string asunto, string cuerpoHtml) : bool` (motivo del fallo en `ErrorProceso`).
  - `JsonJQueryNetAgenda.AdministrarAgenda.Interno(HttpContext) : bool` (lo usa la página, Task 8).
  - Acciones JSON (`[{action, parameters}]`), todas responden `EntRespuesta {estado, mensaje, tipoMensaje, resultado}`:

| Acción | Parámetros | `resultado` |
|---|---|---|
| `Inicio` | — | `{CodUsuario, Nombre, TieneEquipo, Hoy, Lunes, ProximoHabil, Asignables:[{CodUsuario, Nombre, Area}]}` |
| `Equipo` | `lunes` | `EntAgendaTarjetaArea[]` |
| `Agenda` | `cod`, `lunes` | `EntAgendaVistaPersona` |
| `GuardarActividad` | `id` (0 = nueva), `titulo`, `cod`, `fecha`, `entregables:[{id, descripcion}]` | IdActividad |
| `MarcarEntregable` | `idActividad`, `idEntregable`, `hecho` ('1'/'0') | IdActividad |
| `MarcarCompleta` | `id`, `completa` ('1'/'0') | IdActividad |
| `Anular` | `id` | IdActividad |
| `ListarAreas` | — | `EntAgendaArea[]` |
| `GuardarArea` | `id`, `nombre`, `color`, `activa` ('1'/'0') | IdArea |
| `ListarPersonas` | — | `[{CodUsuario, Nombre, IdArea}]` |
| `AsignarArea` | `cod`, `idArea` | IdArea |

  Las cuatro últimas exigen `Permisos.PuedeEntrar(..., Pantallas.ParametrizacionAreasAgenda)`.

- [ ] **Step 1: Constantes en `Permisos.cs`**

En `ReporteTareas/clases/Permisos.cs`, dentro de `internal static class Pantallas`, después de la línea de `RRHHEmpleados`:
```csharp
        /// <summary>Equipo y agenda: la abre cualquier usuario interno; lo que ve lo decide AdministrarAgenda.</summary>
        internal const string Agenda = "Agenda.aspx";
        internal const string ParametrizacionAreasAgenda = "ParametrizacionAreasAgenda.aspx";
```
Dentro de `internal static class Acciones`, después de `UsuariosCambiarPerfil`:
```csharp
        internal const string AgendaVerTodos = "Agenda.VerTodos";
```

- [ ] **Step 2: Remitente configurable y `EnviarCorreoAgenda`**

En `ReporteTareas/clases/EnvioCorreoHelper.cs`, cambiar la firma de `EnviarCorreoPermiso` (línea ~1632) y su nombre de remitente:
```csharp
        public bool EnviarCorreoPermiso(string correosDestinatarios, string correoTitulo, string correoContenido, EntParametrosCorreo parametrosServidorCorreo, string RutaDocumento, string nombreRemitente = null)
```
y la línea `string emailFromName = "Registro de Permiso - Sistema de Tareas";` por:
```csharp
            string emailFromName = string.IsNullOrEmpty(nombreRemitente) ? "Registro de Permiso - Sistema de Tareas" : nombreRemitente;
```
Los cuatro llamadores actuales no pasan el parámetro y siguen igual.

Agregar, justo antes de `#region EnvioCorreoPoliza`:
```csharp
        #region EnviarCorreoAgenda
        /// <summary>
        /// Aviso de la agenda: un destinatario y el cuerpo HTML ya armado por
        /// NegAgendaCorreo. Devuelve false si no salio; el motivo queda en
        /// ErrorProceso. La bitacora de la agenda la escribe el handler.
        /// </summary>
        public bool EnviarCorreoAgenda(string destinatario, string asunto, string cuerpoHtml)
        {
            EntParametrosCorreo parametros = new EntParametrosCorreo();
            parametros.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
            parametros.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
            parametros.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
            parametros.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
            parametros.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
            parametros.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

            return EnviarCorreoPermiso(destinatario, asunto, cuerpoHtml, parametros, "", "Equipo y agenda - Sistema de Tareas");
        }
        #endregion

```

- [ ] **Step 3: Escribir el handler**

`ReporteTareas/Formulario/AdministrarAgenda.ashx`:
```aspx
<%@ WebHandler Language="C#" CodeBehind="AdministrarAgenda.ashx.cs" Class="JsonJQueryNetAgenda.AdministrarAgenda" %>
```

`ReporteTareas/Formulario/AdministrarAgenda.ashx.cs`:
```csharp
using CapaEntidad;
using CapaNegocio;
using CorreoHelper;
using ReporteTareas.clases;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace JsonJQueryNetAgenda
{
    /// <summary>
    /// Handler de Agenda.aspx y de ParametrizacionAreasAgenda.aspx.
    ///
    /// Quien es el usuario sale SIEMPRE de la sesion: lo que mande el navegador
    /// sobre identidad o perfil no se usa. Quien ve a quien y que se puede hacer
    /// lo deciden las reglas puras de CapaNegocio (NegAgendaEquipo,
    /// NegAgendaActividad, NegAgendaVista); este archivo carga, pregunta y
    /// responde.
    ///
    /// IRequiresSessionState y no la de solo lectura: Permisos guarda su cache
    /// en la sesion.
    /// </summary>
    public class AdministrarAgenda : IHttpHandler, IRequiresSessionState
    {
        private static readonly string[] AccionesDeAreas = { "ListarAreas", "GuardarArea", "ListarPersonas", "AsignarArea" };

        private const string NoActivo = "Su usuario no figura como activo en la agenda. Pida a Talento Humano que revise su estado.";

        public void ProcessRequest(HttpContext context)
        {
            /* Sin esto los bytes salen en windows-1252 (Web.config) aunque la
               cabecera diga utf-8, y cada tilde llega rota. */
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();

            string salida;
            if (!Permisos.Autenticado(context))
            {
                context.Response.TrySkipIisCustomErrors = true;
                context.Response.StatusCode = 401;
                salida = Mensaje("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger");
            }
            else if (!Interno(context))
            {
                context.Response.TrySkipIisCustomErrors = true;
                context.Response.StatusCode = 403;
                salida = Mensaje("0", "No tiene permisos para esta pantalla.", "danger");
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                salida = Despachar(context);
            }
            else
            {
                salida = Mensaje("0", "Petición no válida.", "danger");
            }
            context.Response.Write(salida);
        }

        /// <summary>Los usuarios de clientes (IdCliente distinto de 0) no usan la agenda.</summary>
        internal static bool Interno(HttpContext context)
        {
            if (context == null || context.Session == null) { return false; }
            string id = Convert.ToString(context.Session["IdCliente"], CultureInfo.InvariantCulture).Trim();
            return id == "" || id == "0";
        }

        private string Despachar(HttpContext context)
        {
            IDictionary<string, object> peticion = null;
            try
            {
                string json = new System.IO.StreamReader(context.Request.InputStream, Encoding.UTF8).ReadToEnd();
                object[] lista = new JavaScriptSerializer().Deserialize<object[]>(json);
                peticion = lista != null && lista.Length > 0 ? lista[0] as IDictionary<string, object> : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }
            if (peticion == null) { return Mensaje("0", "Petición no válida.", "danger"); }

            string accion = Campo(peticion, "action");
            object p;
            peticion.TryGetValue("parameters", out p);

            if (AccionesDeAreas.Contains(accion) && !Permisos.PuedeEntrar(context, Pantallas.ParametrizacionAreasAgenda))
            {
                return Mensaje("0", "No tiene permisos para esta pantalla.", "danger");
            }

            try
            {
                switch (accion)
                {
                    case "Inicio": return Inicio(context);
                    case "Equipo": return Equipo(context, p);
                    case "Agenda": return Agenda(context, p);
                    case "GuardarActividad": return GuardarActividad(context, p);
                    case "MarcarEntregable": return MarcarEntregable(context, p);
                    case "MarcarCompleta": return MarcarCompleta(context, p);
                    case "Anular": return Anular(context, p);
                    case "ListarAreas": return ListarAreas();
                    case "GuardarArea": return GuardarArea(context, p);
                    case "ListarPersonas": return ListarPersonas();
                    case "AsignarArea": return AsignarArea(context, p);
                }
                return Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudo completar la operación. Intente nuevamente.", "danger");
            }
        }

        // ------------------------------------------------------ quien pregunta

        private sealed class Quien
        {
            public string Yo;
            public bool VerTodos;
            public List<EntAgendaPersona> Personas;
            public HashSet<string> Cadena;

            public EntAgendaPersona Persona(string cod)
            {
                string c = NegAgendaEquipo.Normalizar(cod);
                return Personas.FirstOrDefault(x => NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(x.CodUsuario), c));
            }
        }

        private static Quien Cargar(HttpContext context)
        {
            Quien q = new Quien
            {
                Yo = Usuario(context),
                VerTodos = Permisos.PuedeHacer(context, Acciones.AgendaVerTodos),
                Personas = NegAgenda.PersonasActivas()
            };
            q.Cadena = NegAgendaEquipo.Cadena(q.Personas, q.Yo);
            return q;
        }

        private static string Usuario(HttpContext context)
        {
            return NegAgendaEquipo.Normalizar(Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture));
        }

        // ------------------------------------------------------ agenda

        private string Inicio(HttpContext context)
        {
            Quien q = Cargar(context);
            EntAgendaPersona yo = q.Persona(q.Yo);
            if (yo == null) { return Mensaje("0", NoActivo, "warning"); }

            DateTime hoy = DateTime.Today;
            return Ok(new
            {
                CodUsuario = yo.CodUsuario,
                Nombre = yo.Nombre,
                TieneEquipo = q.VerTodos || q.Cadena.Count > 0,
                Hoy = Iso(hoy),
                Lunes = Iso(NegAgendaActividad.Lunes(hoy)),
                ProximoHabil = Iso(NegAgendaActividad.ProximoDiaHabil(hoy)),
                Asignables = NegAgendaVista.Visibles(q.Yo, q.VerTodos, q.Personas)
                    .Select(x => new { x.CodUsuario, x.Nombre, x.Area })
                    .ToList()
            });
        }

        private string Equipo(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            if (q.Persona(q.Yo) == null) { return Mensaje("0", NoActivo, "warning"); }
            DateTime hoy = DateTime.Today;
            DateTime lunes = LeerLunes(Campo(p, "lunes"), hoy);
            List<EntAgendaActividad> actividades = NegAgenda.Actividades(lunes, lunes.AddDays(4), hoy);
            return Ok(NegAgendaVista.Equipo(q.Yo, q.VerTodos, q.Personas, actividades, lunes, hoy));
        }

        private string Agenda(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            if (q.Persona(q.Yo) == null) { return Mensaje("0", NoActivo, "warning"); }
            DateTime hoy = DateTime.Today;
            DateTime lunes = LeerLunes(Campo(p, "lunes"), hoy);
            string cod = Campo(p, "cod");
            if (cod == "") { cod = q.Yo; }

            List<EntAgendaActividad> actividades = NegAgenda.Actividades(lunes, lunes.AddDays(4), hoy);
            EntAgendaVistaPersona vista = NegAgendaVista.Persona(q.Yo, q.VerTodos, q.Personas, actividades, cod, lunes, hoy);
            if (vista == null) { return Mensaje("0", "No puede ver la agenda de esa persona.", "warning"); }
            return Ok(vista);
        }

        private string GuardarActividad(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            if (q.Persona(q.Yo) == null) { return Mensaje("0", NoActivo, "warning"); }

            int id = Entero(Campo(p, "id"));
            string titulo = Campo(p, "titulo");
            DateTime? fecha = LeerFecha(Campo(p, "fecha"));
            List<EntAgendaEntregable> entregables = NegAgendaActividad.Limpiar(LeerEntregables(p));
            string error = NegAgendaActividad.Validar(titulo, fecha, entregables);
            if (error != null) { return Mensaje("0", error, "warning"); }

            if (id > 0)
            {
                EntAgendaActividad a = NegAgenda.Actividad(id);
                if (a == null || a.Anulada) { return Mensaje("0", "La actividad no existe o fue anulada.", "warning"); }
                if (!NegAgendaActividad.PuedeEditar(q.Yo, a, q.Cadena, q.VerTodos))
                {
                    return Mensaje("0", "No puede modificar esta actividad.", "warning");
                }
                if (entregables.Any(e => e.IdEntregable != 0 && !a.Entregables.Any(x => x.IdEntregable == e.IdEntregable)))
                {
                    return Mensaje("0", "Petición no válida.", "danger");
                }
                NegAgenda.GuardarActividad(id, titulo, a.CodResponsable, a.CodAsignadoPor, fecha.Value, entregables, q.Yo);
                return Ok(id, "Actividad actualizada.", "success");
            }

            string cod = NegAgendaEquipo.Normalizar(Campo(p, "cod"));
            if (cod == "") { cod = q.Yo; }
            EntAgendaPersona responsable = q.Persona(cod);
            if (responsable == null) { return Mensaje("0", "La persona elegida no está activa.", "warning"); }

            bool propia = NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(responsable.CodUsuario), q.Yo);
            if (!propia && !NegAgendaEquipo.PuedeAsignarA(q.Yo, responsable.CodUsuario, q.Cadena, q.VerTodos))
            {
                return Mensaje("0", "No puede asignarle actividades a esa persona.", "warning");
            }
            foreach (EntAgendaEntregable e in entregables) { e.IdEntregable = 0; }

            int nueva = NegAgenda.GuardarActividad(0, titulo, responsable.CodUsuario, q.Yo, fecha.Value, entregables, q.Yo);
            if (propia) { return Ok(nueva, "Actividad guardada.", "success"); }

            return Avisar(q, nueva, responsable)
                ? Ok(nueva, "Actividad asignada a " + responsable.Nombre + ".", "success")
                : Ok(nueva, "Actividad asignada, pero no se pudo avisar por correo a " + responsable.Nombre + ".", "warning");
        }

        /// <summary>
        /// El correo nunca deshace el guardado. Cada intento queda en
        /// AG_CorreoBitacora DESPUES de saber si salio. Devuelve si salio.
        /// </summary>
        private static bool Avisar(Quien q, int idActividad, EntAgendaPersona responsable)
        {
            string asunto = "";
            bool enviado = false;
            string error = null;
            string destinatario = string.IsNullOrWhiteSpace(responsable.Correo) ? null : responsable.Correo.Trim();
            try
            {
                EntAgendaActividad a = NegAgenda.Actividad(idActividad);
                if (a == null || !NegAgendaCorreo.DebeAvisar(a)) { return true; }
                asunto = NegAgendaCorreo.Asunto(a);
                if (destinatario == null)
                {
                    error = "La persona no tiene correo registrado.";
                }
                else
                {
                    EntAgendaPersona asignador = q.Persona(a.CodAsignadoPor);
                    string cuerpo = NegAgendaCorreo.Cuerpo(a, responsable.Nombre,
                        asignador != null ? asignador.Nombre : a.CodAsignadoPor,
                        NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE"));
                    EnvioCorreoHelper correo = new EnvioCorreoHelper();
                    enviado = correo.EnviarCorreoAgenda(destinatario, asunto, cuerpo);
                    if (!enviado)
                    {
                        error = string.IsNullOrEmpty(correo.ErrorProceso) ? "El servidor de correo no aceptó el envío." : correo.ErrorProceso;
                    }
                }
            }
            catch (Exception ex)
            {
                enviado = false;
                error = ex.Message;
            }

            try
            {
                NegAgenda.RegistrarCorreo(idActividad, destinatario, asunto, enviado, error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Agenda: no se pudo registrar el correo. " + ex);
            }
            return enviado;
        }

        private string MarcarEntregable(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            EntAgendaActividad a = NegAgenda.Actividad(Entero(Campo(p, "idActividad")));
            if (a == null || !NegAgendaActividad.PuedeMarcar(q.Yo, a))
            {
                return Mensaje("0", "Solo el responsable puede marcar sus entregables.", "warning");
            }
            int idEntregable = Entero(Campo(p, "idEntregable"));
            if (!a.Entregables.Any(e => e.IdEntregable == idEntregable)) { return Mensaje("0", "Petición no válida.", "danger"); }
            NegAgenda.MarcarEntregable(idEntregable, Campo(p, "hecho") == "1", q.Yo);
            return Ok(a.IdActividad, "", "success");
        }

        private string MarcarCompleta(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            EntAgendaActividad a = NegAgenda.Actividad(Entero(Campo(p, "id")));
            if (a == null || !NegAgendaActividad.PuedeMarcar(q.Yo, a))
            {
                return Mensaje("0", "Solo el responsable puede marcar su actividad.", "warning");
            }
            if (a.Entregables.Count > 0)
            {
                return Mensaje("0", "Esta actividad se completa marcando sus entregables.", "warning");
            }
            NegAgenda.MarcarCompleta(a.IdActividad, Campo(p, "completa") == "1", q.Yo);
            return Ok(a.IdActividad, "", "success");
        }

        private string Anular(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            EntAgendaActividad a = NegAgenda.Actividad(Entero(Campo(p, "id")));
            if (a == null || !NegAgendaActividad.PuedeEditar(q.Yo, a, q.Cadena, q.VerTodos))
            {
                return Mensaje("0", "No puede anular esta actividad.", "warning");
            }
            NegAgenda.Anular(a.IdActividad, q.Yo);
            return Ok(a.IdActividad, "Actividad anulada.", "success");
        }

        // ------------------------------------------------------ areas

        private string ListarAreas()
        {
            return Ok(NegAgendaAreas.ConPersonas(NegAgenda.Areas(), NegAgenda.PersonasActivas()));
        }

        private string GuardarArea(HttpContext context, object p)
        {
            string nombre = Campo(p, "nombre");
            string color = Campo(p, "color").ToUpperInvariant();
            string error = NegAgendaAreas.Validar(nombre, color);
            if (error != null) { return Mensaje("0", error, "warning"); }

            int resultado = NegAgenda.GuardarArea(Entero(Campo(p, "id")), nombre, color, Campo(p, "activa") == "1", Usuario(context));
            string mensaje = NegAgendaAreas.MensajeResultado(resultado);
            if (mensaje != null) { return Mensaje("0", mensaje, "warning"); }
            return Ok(resultado, "Área guardada.", "success");
        }

        private string ListarPersonas()
        {
            return Ok(NegAgenda.PersonasActivas().Select(x => new { x.CodUsuario, x.Nombre, x.IdArea }).ToList());
        }

        private string AsignarArea(HttpContext context, object p)
        {
            string cod = NegAgendaEquipo.Normalizar(Campo(p, "cod"));
            EntAgendaPersona persona = NegAgenda.PersonasActivas()
                .FirstOrDefault(x => NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(x.CodUsuario), cod));
            if (persona == null) { return Mensaje("0", "La persona no está activa.", "warning"); }

            int idArea = Entero(Campo(p, "idArea"));
            EntAgendaArea area = NegAgenda.Areas().FirstOrDefault(a => a.IdArea == idArea);
            if (area == null || (!area.Activa && !area.EsSinArea)) { return Mensaje("0", "Elija un área activa.", "warning"); }

            NegAgenda.AsignarArea(persona.CodUsuario, idArea, Usuario(context));
            return Ok(idArea, persona.Nombre + " quedó en " + area.Nombre + ".", "success");
        }

        // ------------------------------------------------------ lectura y respuesta

        private static string Campo(object campos, string nombre)
        {
            IDictionary<string, object> d = campos as IDictionary<string, object>;
            object v;
            if (d == null || !d.TryGetValue(nombre, out v) || v == null) { return ""; }
            return Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
        }

        private static int Entero(string texto)
        {
            int n;
            return int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out n) ? n : 0;
        }

        private static DateTime? LeerFecha(string texto)
        {
            DateTime d;
            return DateTime.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)
                ? d.Date
                : (DateTime?)null;
        }

        private static DateTime LeerLunes(string texto, DateTime hoy)
        {
            return NegAgendaActividad.Lunes(LeerFecha(texto) ?? hoy);
        }

        private static string Iso(DateTime d)
        {
            return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static List<EntAgendaEntregable> LeerEntregables(object p)
        {
            List<EntAgendaEntregable> lista = new List<EntAgendaEntregable>();
            IDictionary<string, object> d = p as IDictionary<string, object>;
            object valor;
            if (d == null || !d.TryGetValue("entregables", out valor) || valor == null || valor is string) { return lista; }
            IEnumerable items = valor as IEnumerable;
            if (items == null) { return lista; }
            foreach (object item in items)
            {
                lista.Add(new EntAgendaEntregable
                {
                    IdEntregable = Entero(Campo(item, "id")),
                    Descripcion = Campo(item, "descripcion")
                });
            }
            return lista;
        }

        private static string Ok(object resultado, string mensaje = "", string tipo = "success")
        {
            return Serializar(new EntRespuesta { estado = "1", mensaje = mensaje, tipoMensaje = tipo, resultado = resultado });
        }

        private static string Mensaje(string estado, string mensaje, string tipo)
        {
            return Serializar(new EntRespuesta { estado = estado, mensaje = mensaje, tipoMensaje = tipo, resultado = "" });
        }

        private static string Serializar(object o)
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            s.MaxJsonLength = int.MaxValue;
            return s.Serialize(o);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
```

En `ReporteTareas/ReporteTareas.csproj`, después de `<Content Include="Formulario\AdministrarTableroCostos.ashx" />`:
```xml
    <Content Include="Formulario\AdministrarAgenda.ashx" />
```
y después del bloque `<Compile Include="Formulario\AdministrarTableroCostos.ashx.cs">…</Compile>`:
```xml
    <Compile Include="Formulario\AdministrarAgenda.ashx.cs">
      <DependentUpon>AdministrarAgenda.ashx</DependentUpon>
    </Compile>
```

- [ ] **Step 4: BOM a los dos archivos nuevos, compilar y correr toda la suite**

Esperado: compila sin errores; `Correcto: 674`.

- [ ] **Step 5: Commit**

```bash
git add ReporteTareas/Formulario/AdministrarAgenda.ashx ReporteTareas/Formulario/AdministrarAgenda.ashx.cs ReporteTareas/clases/Permisos.cs ReporteTareas/clases/EnvioCorreoHelper.cs ReporteTareas/ReporteTareas.csproj
git commit -m "feat(agenda): handler de la agenda y aviso por correo al asignar

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 8: Pantalla «Equipo y agenda»

**Files:**
- Create: `ReporteTareas/Formulario/Agenda.aspx`, `Agenda.aspx.cs`, `Agenda.aspx.designer.cs`
- Create: `ReporteTareas/js/agenda.js`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: las acciones `Inicio`, `Equipo`, `Agenda`, `GuardarActividad`, `MarcarEntregable`, `MarcarCompleta`, `Anular` del Task 7; `AdministrarAgenda.Interno`; `Permisos.Autenticado`.

- [ ] **Step 1: Código de la página**

`ReporteTareas/Formulario/Agenda.aspx.cs`:
```csharp
using ReporteTareas.clases;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Equipo y agenda. La abre cualquier usuario interno con sesion: lo que ve
    /// cada uno lo decide AdministrarAgenda.ashx. Este redirect solo evita que
    /// un usuario de cliente vea la pantalla vacia.
    /// </summary>
    public partial class Agenda : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!Permisos.Autenticado(Context) || !JsonJQueryNetAgenda.AdministrarAgenda.Interno(Context))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
```

`ReporteTareas/Formulario/Agenda.aspx.designer.cs`:
```csharp
namespace ReporteTareas.Formulario
{
    public partial class Agenda
    {
        /* Sin controles de servidor: la pagina se dibuja entera desde js/agenda.js. */
    }
}
```

- [ ] **Step 2: Marcado**

`ReporteTareas/Formulario/Agenda.aspx`:
```aspx
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="Agenda.aspx.cs" Inherits="ReporteTareas.Formulario.Agenda" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .ag-toolbar { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; margin: 0 0 12px; }
        .ag-buscar { flex: 1; min-width: 220px; }
        .ag-semana { display: flex; align-items: center; gap: 6px; }
        #agSemLabel { min-width: 210px; text-align: center; font-weight: 600; }
        .ag-areas { display: grid; grid-template-columns: repeat(auto-fill, minmax(230px, 1fr)); gap: 14px; }
        .ag-area { background: #fff; border: 1px solid #ddd; border-top: 4px solid #888; border-radius: 6px; padding: 14px; cursor: pointer; }
        .ag-area:hover { box-shadow: 0 4px 12px rgba(0,0,0,.12); }
        .ag-area-nombre { font-weight: 700; font-size: 14px; }
        .ag-area-cuenta, .ag-meta { font-size: 12px; color: #666; }
        .ag-avatares { display: flex; margin: 10px 0; }
        .ag-avatar { width: 30px; height: 30px; border-radius: 50%; background: #888; color: #fff; display: inline-flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; border: 2px solid #fff; margin-right: -6px; flex-shrink: 0; }
        .ag-mas { background: #e5e5e5; color: #555; }
        .ag-cifras { display: flex; gap: 12px; font-size: 12px; color: #555; flex-wrap: wrap; }
        .ag-rojo { color: #C62828; }
        .ag-personas { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 12px; }
        .ag-persona { background: #fff; border: 1px solid #ddd; border-radius: 6px; padding: 12px; cursor: pointer; }
        .ag-persona:hover { border-color: #2F6DB5; }
        .ag-persona-top { display: flex; gap: 14px; align-items: center; margin-bottom: 8px; }
        .ag-nombre { font-weight: 600; }
        .ag-chip { display: inline-block; color: #fff; font-size: 11px; padding: 1px 8px; border-radius: 10px; background: #888; }
        .ag-persona-cab { display: flex; align-items: center; gap: 14px; margin-bottom: 12px; flex-wrap: wrap; }
        .ag-persona-cab #agNueva { margin-left: auto; }
        .ag-franja { border: 1px solid #F3B4B4; background: #FDECEC; border-radius: 6px; padding: 10px; margin-bottom: 12px; }
        .ag-franja-titulo { font-weight: 700; color: #C62828; margin-bottom: 6px; }
        .ag-franja-lista { display: flex; flex-wrap: wrap; gap: 8px; }
        .ag-franja-lista .ag-act { width: 220px; }
        .ag-dias { display: grid; grid-template-columns: repeat(5, minmax(180px, 1fr)); gap: 10px; overflow-x: auto; }
        .ag-dia { background: #f7f7f7; border-radius: 6px; min-height: 320px; display: flex; flex-direction: column; }
        .ag-dia-cab { display: flex; justify-content: space-between; padding: 8px 10px; border-bottom: 2px solid #2F6DB5; }
        .ag-dia-cab.ag-hoy { border-bottom-color: #2E7D32; background: #EAF5EA; }
        .ag-dia-nombre { font-weight: 700; text-transform: uppercase; font-size: 12px; }
        .ag-dia-fecha { font-size: 11px; color: #666; }
        .ag-dia-cuerpo { padding: 8px; display: flex; flex-direction: column; gap: 6px; }
        .ag-vacio-dia, .ag-vacio { font-size: 12px; color: #888; text-align: center; padding: 12px; }
        .ag-act { background: #fff; border: 1px solid #ddd; border-radius: 6px; padding: 8px; cursor: pointer; font-size: 12px; }
        .ag-act.ag-completa { background: #EAF5EA; border-color: #9FD89F; }
        .ag-act.ag-atrasada { border-color: #E57373; border-left: 4px solid #C62828; }
        .ag-tit { font-weight: 600; margin-bottom: 4px; }
        .ag-progreso { display: flex; align-items: center; gap: 6px; font-size: 10px; color: #666; }
        .ag-barra { flex: 1; height: 4px; background: #e1dfdd; border-radius: 3px; overflow: hidden; }
        .ag-barra div { height: 100%; background: #2E7D32; }
        .ag-modal { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,.35); display: none; align-items: center; justify-content: center; z-index: 1050; padding: 16px; }
        .ag-modal.ag-abierto { display: flex; }
        .ag-modal-caja { background: #fff; border-radius: 6px; width: 100%; max-width: 520px; padding: 18px; max-height: 90vh; overflow-y: auto; }
        .ag-modal-ancha { max-width: 760px; }
        .ag-modal-cab { display: flex; align-items: center; gap: 10px; margin-bottom: 12px; }
        .ag-modal-cab button { margin-left: auto; }
        .ag-modal-titulo { font-size: 16px; font-weight: 700; }
        .ag-punto { width: 12px; height: 12px; border-radius: 50%; display: inline-block; }
        .ag-entregable { display: block; font-weight: normal; margin: 4px 0; }
        .ag-botones { display: flex; justify-content: flex-end; gap: 8px; margin-top: 14px; flex-wrap: wrap; }
    </style>
    <script src="../js/agenda.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Equipo y agenda</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="agError" class="alert alert-danger" style="display: none"></div>
            <div id="agAviso" class="alert" style="display: none"></div>

            <div class="ag-toolbar">
                <input type="text" id="agBuscar" class="form-control ag-buscar" placeholder="Buscar colaborador por nombre..." />
                <div class="ag-semana">
                    <button type="button" id="agSemAnt" class="btn btn-default">&lsaquo;</button>
                    <span id="agSemLabel"></span>
                    <button type="button" id="agSemSig" class="btn btn-default">&rsaquo;</button>
                    <button type="button" id="agSemHoy" class="btn btn-default">Hoy</button>
                </div>
                <button type="button" id="agMiAgenda" class="btn btn-default">Mi agenda</button>
                <button type="button" id="agAsignar" class="btn btn-primary">+ Asignar actividad</button>
            </div>

            <div id="vistaEquipo" style="display: none">
                <div id="agAreas" class="ag-areas"></div>
            </div>

            <div id="vistaPersona" style="display: none">
                <div class="ag-persona-cab">
                    <button type="button" id="agVolver" class="btn btn-default">&lsaquo; Volver al equipo</button>
                    <span id="agPerIniciales" class="ag-avatar"></span>
                    <div>
                        <div id="agPerNombre" class="ag-nombre"></div>
                        <span id="agPerArea" class="ag-chip"></span>
                    </div>
                    <button type="button" id="agNueva" class="btn btn-primary">+ Nueva actividad</button>
                </div>
                <div id="agAtrasadas"></div>
                <div id="agDias" class="ag-dias"></div>
            </div>
        </div>
    </div>

    <div id="agModalArea" class="ag-modal">
        <div class="ag-modal-caja ag-modal-ancha">
            <div class="ag-modal-cab">
                <span id="agModalAreaPunto" class="ag-punto"></span>
                <span id="agModalAreaTitulo" class="ag-modal-titulo"></span>
                <button type="button" class="btn btn-default btn-sm" data-cerrar="1">Cerrar</button>
            </div>
            <div id="agModalAreaPersonas" class="ag-personas"></div>
        </div>
    </div>

    <div id="agModalDetalle" class="ag-modal">
        <div class="ag-modal-caja">
            <div class="ag-modal-cab">
                <span id="agDetTitulo" class="ag-modal-titulo"></span>
                <button type="button" class="btn btn-default btn-sm" data-cerrar="1">Cerrar</button>
            </div>
            <div id="agDetMeta" class="ag-meta"></div>
            <div id="agDetError" class="alert alert-danger" style="display: none; margin-top: 8px"></div>
            <div id="agDetEntregables" style="margin-top: 10px"></div>
            <div id="agDetBotones" class="ag-botones"></div>
        </div>
    </div>

    <div id="agModalForm" class="ag-modal">
        <div class="ag-modal-caja">
            <div class="ag-modal-cab">
                <span id="agFormTitulo" class="ag-modal-titulo"></span>
                <button type="button" class="btn btn-default btn-sm" data-cerrar="1">Cerrar</button>
            </div>
            <div id="agFormError" class="alert alert-danger" style="display: none"></div>
            <div class="form-group">
                <label for="agFormTexto">¿Qué hay que hacer? *</label>
                <input type="text" id="agFormTexto" class="form-control" maxlength="200" />
            </div>
            <div class="form-group">
                <label for="agFormPersona">Asignar a *</label>
                <select id="agFormPersona" class="form-control"></select>
            </div>
            <div class="form-group">
                <label for="agFormFecha">Fecha de entrega *</label>
                <input type="date" id="agFormFecha" class="form-control" />
            </div>
            <div class="form-group">
                <label for="agFormEntregables">Entregables (uno por línea, opcional)</label>
                <textarea id="agFormEntregables" class="form-control" rows="4"></textarea>
            </div>
            <div class="ag-botones">
                <button type="button" class="btn btn-default" data-cerrar="1">Cancelar</button>
                <button type="button" id="agFormGuardar" class="btn btn-primary">Guardar</button>
            </div>
        </div>
    </div>
</asp:Content>
```

- [ ] **Step 3: El script**

`ReporteTareas/js/agenda.js`:
```js
/* ==========================================================================
   Equipo y agenda — Agenda.aspx
   Handler: AdministrarAgenda.ashx
   Este archivo NO decide permisos ni estados: quien se ve, que se puede
   editar o marcar, y que esta completa o atrasada llegan resueltos del
   servidor (NegAgendaVista). Solo dibuja y envia.
   ========================================================================== */

var agInicio = null;     /* respuesta de Inicio */
var agLunes = '';        /* lunes de la semana en pantalla (yyyy-mm-dd) */
var agVista = '';        /* 'equipo' | 'persona' */
var agPersona = '';      /* Cod_Usuario de la agenda abierta */
var agEquipo = [];       /* tarjetas de area de la semana */
var agAgenda = null;     /* vista de la persona abierta */
var agEditando = null;   /* actividad en edicion; null = nueva */
var agConsulta = 0;      /* solo se dibuja la ultima consulta */
var agGuardando = false;
var agAvisoTimer = null;

var AG_MESES = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic'];

$(document).ready(function () {
    $('#agSemAnt').on('click', function () { MoverSemana(-7); });
    $('#agSemSig').on('click', function () { MoverSemana(7); });
    $('#agSemHoy').on('click', function () { agLunes = agInicio.Lunes; Recargar(); });
    $('#agBuscar').on('input', DibujarEquipo);
    $('#agMiAgenda').on('click', function () { AbrirPersona(agInicio.CodUsuario); });
    $('#agVolver').on('click', AbrirEquipo);
    $('#agAsignar').on('click', function () { AbrirFormulario(null, ''); });
    $('#agNueva').on('click', function () { AbrirFormulario(null, agPersona); });
    $('#agFormGuardar').on('click', Guardar);
    /* La pagina vive dentro del <form> de la maestra: Enter haria un postback. */
    $('#agBuscar, #agFormTexto, #agFormFecha').on('keydown', function (e) { if (e.which === 13) { e.preventDefault(); } });
    $(document).on('click', '.ag-modal [data-cerrar]', function () { Cerrar($(this).closest('.ag-modal')); });

    Llamar('Inicio', {}, function (r) {
        agInicio = r;
        agLunes = r.Lunes;
        if (r.TieneEquipo) { AbrirEquipo(); } else { AbrirPersona(r.CodUsuario); }
    });
});

/* ------------------------------------------------------------ servidor */

/* fallo(mensaje), si viene, se encarga del error; si no, va arriba en #agError. */
function Llamar(accion, parametros, ok, fallo) {
    $.ajax({
        type: 'POST',
        url: 'AdministrarAgenda.ashx',
        data: JSON.stringify([{ action: accion, parameters: parametros }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') {
                Fallar((r && r.mensaje) || 'No se pudo completar la operación.', fallo);
                return;
            }
            Mostrar('#agError', '');
            if (r.mensaje) { Aviso(r.mensaje, r.tipoMensaje); }
            ok(r.resultado);
        },
        error: function (x) {
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            Fallar((r && r.mensaje) || 'No se pudo comunicar con el servidor.', fallo);
        }
    });
}

function Fallar(mensaje, fallo) {
    if (fallo) { fallo(mensaje); } else { Mostrar('#agError', mensaje); }
}

function Recargar() {
    if (agVista === 'equipo') { CargarEquipo(); } else { CargarPersona(); }
}

/* ------------------------------------------------------------ semana */

function DeIso(iso) { var p = iso.split('-'); return new Date(+p[0], +p[1] - 1, +p[2]); }

function AIso(d) {
    var m = d.getMonth() + 1, dia = d.getDate();
    return d.getFullYear() + '-' + (m < 10 ? '0' : '') + m + '-' + (dia < 10 ? '0' : '') + dia;
}

function SumarDias(iso, n) { var d = DeIso(iso); d.setDate(d.getDate() + n); return AIso(d); }

function FechaCorta(iso) { var d = DeIso(iso); return d.getDate() + ' ' + AG_MESES[d.getMonth()]; }

function MoverSemana(dias) { agLunes = SumarDias(agLunes, dias); Recargar(); }

function DibujarSemana() {
    var viernes = SumarDias(agLunes, 4);
    var texto = FechaCorta(agLunes) + ' – ' + FechaCorta(viernes) + ' ' + DeIso(viernes).getFullYear();
    if (agLunes === agInicio.Lunes) { texto += ' · Esta semana'; }
    $('#agSemLabel').text(texto);
}

/* ------------------------------------------------------------ equipo */

function AbrirEquipo() {
    agVista = 'equipo';
    agPersona = '';
    $('#vistaPersona').hide();
    $('#vistaEquipo').show();
    $('#agBuscar, #agMiAgenda, #agAsignar').show();
    CargarEquipo();
}

function CargarEquipo() {
    var numero = ++agConsulta;
    DibujarSemana();
    Llamar('Equipo', { lunes: agLunes }, function (r) {
        if (numero !== agConsulta) { return; }
        agEquipo = r || [];
        DibujarEquipo();
    });
}

function DibujarEquipo() {
    var texto = $.trim($('#agBuscar').val() || '').toLowerCase();
    var $c = $('#agAreas').empty();
    if (texto) {
        var halladas = [];
        $.each(agEquipo, function (i, area) {
            $.each(area.Personas, function (j, p) {
                if (p.Nombre.toLowerCase().indexOf(texto) >= 0) { halladas.push({ persona: p, area: area }); }
            });
        });
        if (!halladas.length) {
            $c.append('<div class="ag-vacio">No se encontró ningún colaborador con ese nombre.</div>');
            return;
        }
        var $g = $('<div class="ag-personas"></div>').appendTo($c);
        $.each(halladas, function (i, h) { $g.append(TarjetaPersona(h.persona, h.area)); });
        return;
    }
    if (!agEquipo.length) { $c.append('<div class="ag-vacio">No hay personas para mostrar.</div>'); return; }
    $.each(agEquipo, function (i, area) { $c.append(TarjetaArea(area)); });
}

function TarjetaArea(area) {
    var n = area.Personas.length;
    var avatares = '';
    $.each(area.Personas.slice(0, 4), function (i, p) {
        avatares += '<span class="ag-avatar" style="background:' + Html(area.Color) + '">' + Html(p.Iniciales) + '</span>';
    });
    if (n > 4) { avatares += '<span class="ag-avatar ag-mas">+' + (n - 4) + '</span>'; }
    var c = area.Contador;
    var $t = $('<div class="ag-area"></div>').css('border-top-color', area.Color).html(
        '<div class="ag-area-nombre">' + Html(area.Area) + '</div>' +
        '<div class="ag-area-cuenta">' + n + (n === 1 ? ' colaborador' : ' colaboradores') + '</div>' +
        '<div class="ag-avatares">' + avatares + '</div>' +
        '<div class="ag-cifras"><span><b>' + c.Actividades + '</b> actividades</span>' +
        '<span class="' + (c.Atrasadas ? 'ag-rojo' : '') + '"><b>' + c.Atrasadas + '</b> atrasadas</span></div>');
    $t.on('click', function () { AbrirArea(area); });
    return $t;
}

function TarjetaPersona(p, area) {
    var c = p.Contador;
    var $t = $('<div class="ag-persona"></div>').html(
        '<div class="ag-persona-top"><span class="ag-avatar" style="background:' + Html(area.Color) + '">' + Html(p.Iniciales) + '</span>' +
        '<div><div class="ag-nombre">' + Html(p.Nombre) + '</div>' +
        '<span class="ag-chip" style="background:' + Html(area.Color) + '">' + Html(area.Area) + '</span></div></div>' +
        '<div class="ag-cifras"><span><b>' + c.Actividades + '</b> actividades</span>' +
        (c.Porcentaje === null ? '' : '<span><b>' + c.Porcentaje + '%</b> entregables</span>') +
        '<span class="' + (c.Atrasadas ? 'ag-rojo' : '') + '"><b>' + c.Atrasadas + '</b> atrasadas</span></div>');
    $t.on('click', function () { Cerrar($('#agModalArea')); AbrirPersona(p.CodUsuario); });
    return $t;
}

function AbrirArea(area) {
    $('#agModalAreaTitulo').text(area.Area);
    $('#agModalAreaPunto').css('background', area.Color);
    var $g = $('#agModalAreaPersonas').empty();
    $.each(area.Personas, function (i, p) { $g.append(TarjetaPersona(p, area)); });
    Abrir($('#agModalArea'));
}

/* ------------------------------------------------------------ agenda de una persona */

function AbrirPersona(cod) {
    agVista = 'persona';
    agPersona = cod;
    $('#vistaEquipo').hide();
    $('#agBuscar, #agMiAgenda, #agAsignar').hide();
    $('#vistaPersona').show();
    $('#agVolver').toggle(!!agInicio.TieneEquipo);
    CargarPersona();
}

function CargarPersona(despues) {
    var numero = ++agConsulta;
    DibujarSemana();
    Llamar('Agenda', { cod: agPersona, lunes: agLunes }, function (v) {
        if (numero !== agConsulta) { return; }
        agAgenda = v;
        DibujarPersona();
        if (despues) { despues(); }
    });
}

function DibujarPersona() {
    var v = agAgenda;
    $('#agPerIniciales').text(v.Iniciales).css('background', v.Color);
    $('#agPerNombre').text(v.Nombre);
    $('#agPerArea').text(v.Area).css('background', v.Color);
    $('#agNueva').text(v.EsPropia ? '+ Nueva actividad' : '+ Asignar a esta persona').toggle(!!v.PuedeAsignar);

    var $a = $('#agAtrasadas').empty();
    if (v.Atrasadas.length) {
        var $f = $('<div class="ag-franja"></div>').appendTo($a);
        $f.append($('<div class="ag-franja-titulo"></div>').text('Atrasadas (' + v.Atrasadas.length + ')'));
        var $lista = $('<div class="ag-franja-lista"></div>').appendTo($f);
        $.each(v.Atrasadas, function (i, it) { $lista.append(TarjetaActividad(it, true)); });
    }

    var $d = $('#agDias').empty();
    $.each(v.Dias, function (i, dia) {
        var $col = $('<div class="ag-dia"></div>').appendTo($d);
        $col.append('<div class="ag-dia-cab' + (dia.EsHoy ? ' ag-hoy' : '') + '"><span class="ag-dia-nombre">' + Html(dia.Nombre) +
            '</span><span class="ag-dia-fecha">' + FechaCorta(dia.Fecha) + '</span></div>');
        var $cuerpo = $('<div class="ag-dia-cuerpo"></div>').appendTo($col);
        if (!dia.Actividades.length) { $cuerpo.append('<div class="ag-vacio-dia">Sin actividades</div>'); }
        $.each(dia.Actividades, function (j, it) { $cuerpo.append(TarjetaActividad(it, false)); });
    });
}

function TarjetaActividad(it, conFecha) {
    var total = it.Entregables.length, hechos = 0;
    $.each(it.Entregables, function (i, e) { if (e.Hecho) { hechos++; } });
    var html = '<div class="ag-tit">' + Html(it.Titulo) + '</div>';
    if (conFecha) { html += '<div class="ag-meta">Vencía el ' + FechaCorta(it.FechaEntrega) + '</div>'; }
    if (total) {
        var pct = Math.round(hechos * 100 / total);
        html += '<div class="ag-progreso"><div class="ag-barra"><div style="width:' + pct + '%"></div></div><span>' + hechos + '/' + total + '</span></div>';
    }
    if (it.AsignadoPor) { html += '<div class="ag-meta">Asignó: ' + Html(it.AsignadoPor) + '</div>'; }
    var clase = 'ag-act' + (it.Completa ? ' ag-completa' : '') + (it.Atrasada ? ' ag-atrasada' : '');
    var $t = $('<div></div>').addClass(clase).html(html);
    $t.on('click', function () { AbrirDetalle(it.IdActividad); });
    return $t;
}

/* ------------------------------------------------------------ detalle */

function Buscar(id) {
    var hallada = null;
    if (!agAgenda) { return null; }
    $.each(agAgenda.Atrasadas, function (i, it) { if (it.IdActividad === id) { hallada = it; } });
    $.each(agAgenda.Dias, function (i, d) {
        $.each(d.Actividades, function (j, it) { if (it.IdActividad === id) { hallada = it; } });
    });
    return hallada;
}

/* Vuelve a pedir la agenda y reabre el detalle con lo que quedo guardado. */
function RefrescarDetalle(id) {
    CargarPersona(function () { AbrirDetalle(id); });
}

function AbrirDetalle(id) {
    var it = Buscar(id);
    if (!it) { Cerrar($('#agModalDetalle')); return; }
    Mostrar('#agDetError', '');
    $('#agDetTitulo').text(it.Titulo);
    var estado = it.Completa ? 'Completa' : (it.Atrasada ? 'Atrasada' : 'Pendiente');
    $('#agDetMeta').text('Entrega: ' + FechaCorta(it.FechaEntrega) + ' · ' + estado + (it.AsignadoPor ? ' · Asignó: ' + it.AsignadoPor : ''));

    var $l = $('#agDetEntregables').empty();
    if (!it.Entregables.length) { $l.append('<div class="ag-meta">Sin entregables.</div>'); }
    $.each(it.Entregables, function (i, e) {
        var $chk = $('<input type="checkbox" />').prop('checked', e.Hecho).prop('disabled', !it.PuedeMarcar);
        $chk.on('change', function () {
            var marcar = $chk.prop('checked');
            $chk.prop('disabled', true);
            Llamar('MarcarEntregable', { idActividad: it.IdActividad, idEntregable: e.IdEntregable, hecho: marcar ? '1' : '0' },
                function () { RefrescarDetalle(id); },
                function (m) { $chk.prop('checked', !marcar).prop('disabled', false); Mostrar('#agDetError', m); });
        });
        $l.append($('<label class="ag-entregable"></label>').append($chk).append(' ').append($('<span></span>').text(e.Descripcion)));
    });

    var $b = $('#agDetBotones').empty();
    if (it.PuedeMarcar && !it.Entregables.length) {
        $('<button type="button" class="btn btn-success"></button>')
            .text(it.Completa ? 'Marcar como pendiente' : 'Marcar como completa')
            .on('click', function () {
                var $btn = $(this).prop('disabled', true);
                Llamar('MarcarCompleta', { id: it.IdActividad, completa: it.Completa ? '0' : '1' },
                    function () { RefrescarDetalle(id); },
                    function (m) { $btn.prop('disabled', false); Mostrar('#agDetError', m); });
            }).appendTo($b);
    }
    if (it.PuedeEditar) {
        $('<button type="button" class="btn btn-default">Editar</button>')
            .on('click', function () { Cerrar($('#agModalDetalle')); AbrirFormulario(it, agPersona); })
            .appendTo($b);
        $('<button type="button" class="btn btn-danger">Anular</button>')
            .on('click', function () {
                var $btn = $(this);
                if (!$btn.data('confirmar')) { $btn.data('confirmar', true).text('¿Seguro? Anular'); return; }
                $btn.prop('disabled', true);
                Llamar('Anular', { id: it.IdActividad },
                    function () { Cerrar($('#agModalDetalle')); CargarPersona(); },
                    function (m) { $btn.prop('disabled', false); Mostrar('#agDetError', m); });
            }).appendTo($b);
    }
    Abrir($('#agModalDetalle'));
}

/* ------------------------------------------------------------ formulario */

function AbrirFormulario(it, codFijo) {
    agEditando = it;
    $('#agFormTitulo').text(it ? 'Editar actividad' : (codFijo === agInicio.CodUsuario ? 'Nueva actividad' : 'Asignar actividad'));

    var $p = $('#agFormPersona').empty();
    $.each(agInicio.Asignables, function (i, p) {
        if (codFijo && p.CodUsuario !== codFijo) { return; }
        $p.append($('<option></option>').val(p.CodUsuario).text(p.Nombre + ' (' + p.Area + ')'));
    });
    if (!codFijo) { $p.prepend('<option value="">Seleccione...</option>').val(''); }
    $p.prop('disabled', !!codFijo || !!it);

    $('#agFormTexto').val(it ? it.Titulo : '');
    $('#agFormFecha').val(it ? it.FechaEntrega : agInicio.ProximoHabil);
    $('#agFormEntregables').val(it ? $.map(it.Entregables, function (e) { return e.Descripcion; }).join('\n') : '');
    Mostrar('#agFormError', '');
    Abrir($('#agModalForm'));
    $('#agFormTexto').focus();
}

function Guardar() {
    if (agGuardando) { return; }

    var lineas = $.grep($.map(($('#agFormEntregables').val() || '').split('\n'), function (s) { return $.trim(s); }),
        function (s) { return s !== ''; });
    /* Al editar, un entregable cuyo texto no cambio conserva su id, y con el
       su marca de hecho. Uno con texto nuevo es un entregable nuevo. */
    var libres = agEditando ? agEditando.Entregables.slice() : [];
    var entregables = $.map(lineas, function (texto) {
        var id = 0;
        for (var i = 0; i < libres.length; i++) {
            if (libres[i].Descripcion === texto) { id = libres[i].IdEntregable; libres.splice(i, 1); break; }
        }
        return { id: id, descripcion: texto };
    });

    var p = {
        id: agEditando ? agEditando.IdActividad : 0,
        titulo: $.trim($('#agFormTexto').val() || ''),
        cod: agEditando ? '' : ($('#agFormPersona').val() || ''),
        fecha: $('#agFormFecha').val() || '',
        entregables: entregables
    };
    if (!p.titulo) { Mostrar('#agFormError', 'Escriba qué hay que hacer.'); return; }
    if (!agEditando && !p.cod) { Mostrar('#agFormError', 'Elija a quién se asigna.'); return; }
    if (!p.fecha) { Mostrar('#agFormError', 'Elija la fecha de entrega.'); return; }
    var dia = DeIso(p.fecha).getDay();
    if (dia === 0 || dia === 6) { Mostrar('#agFormError', 'La fecha de entrega debe ser de lunes a viernes.'); return; }

    agGuardando = true;
    $('#agFormGuardar').prop('disabled', true);
    Llamar('GuardarActividad', p, function () {
        agGuardando = false;
        $('#agFormGuardar').prop('disabled', false);
        Cerrar($('#agModalForm'));
        Recargar();
    }, function (m) {
        agGuardando = false;
        $('#agFormGuardar').prop('disabled', false);
        Mostrar('#agFormError', m);
    });
}

/* ------------------------------------------------------------ utilidades */

function Abrir($m) { $m.addClass('ag-abierto'); }

function Cerrar($m) { $m.removeClass('ag-abierto'); }

function Mostrar(selector, texto) { $(selector).text(texto || '').toggle(!!texto); }

function Aviso(texto, tipo) {
    var $a = $('#agAviso');
    $a.removeClass('alert-success alert-warning alert-info alert-danger')
        .addClass('alert-' + (tipo || 'info')).text(texto).show();
    clearTimeout(agAvisoTimer);
    agAvisoTimer = setTimeout(function () { $a.hide(); }, tipo === 'warning' ? 10000 : 5000);
}

function Html(s) {
    return $('<div>').text(s == null ? '' : String(s)).html();
}
```

- [ ] **Step 4: Registrar en el proyecto web**

En `ReporteTareas/ReporteTareas.csproj`, junto a `<Content Include="Formulario\TableroCostos.aspx" />`:
```xml
    <Content Include="Formulario\Agenda.aspx" />
```
junto a `<Content Include="js\tableroCostos.js" />`:
```xml
    <Content Include="js\agenda.js" />
```
y junto al bloque `<Compile Include="Formulario\TableroCostos.aspx.cs">`:
```xml
    <Compile Include="Formulario\Agenda.aspx.cs">
      <DependentUpon>Agenda.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\Agenda.aspx.designer.cs">
      <DependentUpon>Agenda.aspx</DependentUpon>
    </Compile>
```
(Copiar exactamente las etiquetas que tenga el bloque de `TableroCostos.aspx.cs`; si no lleva `<SubType>`, no ponerlo.)

- [ ] **Step 5: BOM a los 4 archivos nuevos y comprobarlo**

```bash
for f in ReporteTareas/Formulario/Agenda.aspx ReporteTareas/Formulario/Agenda.aspx.cs ReporteTareas/Formulario/Agenda.aspx.designer.cs ReporteTareas/js/agenda.js; do printf "%s " $f; head -c3 "$f" | od -An -tx1; done
```
Esperado: `ef bb bf` en los cuatro.

- [ ] **Step 6: Compilar**

Esperado: sin errores.

- [ ] **Step 7: Commit**

```bash
git add ReporteTareas/Formulario/Agenda.aspx ReporteTareas/Formulario/Agenda.aspx.cs ReporteTareas/Formulario/Agenda.aspx.designer.cs ReporteTareas/js/agenda.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(agenda): pantalla Equipo y agenda

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 9: Pantalla «Áreas de la agenda»

**Files:**
- Create: `ReporteTareas/Formulario/ParametrizacionAreasAgenda.aspx`, `.aspx.cs`, `.aspx.designer.cs`
- Create: `ReporteTareas/js/parametrizacionAreasAgenda.js`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: acciones `ListarAreas`, `GuardarArea`, `ListarPersonas`, `AsignarArea`; `Permisos.PuedeEntrar`, `Pantallas.ParametrizacionAreasAgenda`.

- [ ] **Step 1: Código de la página**

`ReporteTareas/Formulario/ParametrizacionAreasAgenda.aspx.cs`:
```csharp
using ReporteTareas.clases;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Catalogo de areas de la agenda y area de cada persona. La barrera de
    /// verdad esta en AdministrarAgenda.ashx; esto evita mostrar una pantalla
    /// vacia a quien no la tiene en su menu.
    /// </summary>
    public partial class ParametrizacionAreasAgenda : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!Permisos.PuedeEntrar(Context, Pantallas.ParametrizacionAreasAgenda))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
```

`ReporteTareas/Formulario/ParametrizacionAreasAgenda.aspx.designer.cs`:
```csharp
namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionAreasAgenda
    {
        /* Sin controles de servidor: la pagina se dibuja entera desde js/parametrizacionAreasAgenda.js. */
    }
}
```

- [ ] **Step 2: Marcado**

`ReporteTareas/Formulario/ParametrizacionAreasAgenda.aspx`:
```aspx
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionAreasAgenda.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionAreasAgenda" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .pa-tabla td { vertical-align: middle !important; }
        .pa-tabla input[type=color] { width: 48px; height: 30px; padding: 0; border: 1px solid #ccc; }
    </style>
    <script src="../js/parametrizacionAreasAgenda.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Áreas de la agenda</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="paError" class="alert alert-danger" style="display: none"></div>
            <div id="paAviso" class="alert" style="display: none"></div>

            <div class="panel panel-default">
                <div class="panel-heading">Áreas</div>
                <div class="panel-body">
                    <table class="table table-condensed pa-tabla">
                        <thead><tr><th>Nombre</th><th>Color</th><th>Activa</th><th>Personas</th><th></th></tr></thead>
                        <tbody id="paAreas"></tbody>
                    </table>
                    <button type="button" id="paNueva" class="btn btn-primary">+ Nueva área</button>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading">Personas</div>
                <div class="panel-body">
                    <div class="row" style="margin-bottom: 10px">
                        <div class="col-lg-4"><input type="text" id="paBuscar" class="form-control" placeholder="Buscar por nombre..." /></div>
                        <div class="col-lg-3"><select id="paFiltro" class="form-control"></select></div>
                    </div>
                    <table class="table table-condensed pa-tabla">
                        <thead><tr><th>Persona</th><th>Área</th></tr></thead>
                        <tbody id="paPersonas"></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
```

- [ ] **Step 3: El script**

`ReporteTareas/js/parametrizacionAreasAgenda.js`:
```js
/* ==========================================================================
   Areas de la agenda — ParametrizacionAreasAgenda.aspx
   Handler: AdministrarAgenda.ashx (ListarAreas, GuardarArea, ListarPersonas,
   AsignarArea). Las validaciones de verdad estan en el servidor.
   ========================================================================== */

var paAreas = [];
var paPersonas = [];
var paAvisoTimer = null;

$(document).ready(function () {
    $('#paNueva').on('click', function () {
        FilaArea({ IdArea: 0, Nombre: '', Color: '#0078D4', Activa: true, EsSinArea: false, Personas: 0 }, true).prependTo('#paAreas');
    });
    $('#paBuscar').on('input', DibujarPersonas).on('keydown', SinEnter);
    $('#paFiltro').on('change', DibujarPersonas);
    CargarTodo();
});

/* La pagina vive dentro del <form> de la maestra: Enter haria un postback. */
function SinEnter(e) { if (e.which === 13) { e.preventDefault(); } }

function Llamar(accion, parametros, ok, fallo) {
    $.ajax({
        type: 'POST',
        url: 'AdministrarAgenda.ashx',
        data: JSON.stringify([{ action: accion, parameters: parametros }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') { Fallar((r && r.mensaje) || 'No se pudo completar la operación.', fallo); return; }
            Mostrar('#paError', '');
            if (r.mensaje) { Aviso(r.mensaje, r.tipoMensaje); }
            ok(r.resultado);
        },
        error: function (x) {
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            Fallar((r && r.mensaje) || 'No se pudo comunicar con el servidor.', fallo);
        }
    });
}

function Fallar(mensaje, fallo) {
    if (fallo) { fallo(mensaje); } else { Mostrar('#paError', mensaje); }
}

function CargarTodo() {
    Llamar('ListarAreas', {}, function (a) {
        paAreas = a || [];
        DibujarAreas();
        Llamar('ListarPersonas', {}, function (p) {
            paPersonas = p || [];
            LlenarFiltro();
            DibujarPersonas();
        });
    });
}

function DibujarAreas() {
    var $t = $('#paAreas').empty();
    $.each(paAreas, function (i, a) { FilaArea(a, false).appendTo($t); });
}

function FilaArea(a, nueva) {
    var $nombre = $('<input type="text" class="form-control input-sm" maxlength="100" />')
        .val(a.Nombre).prop('disabled', a.EsSinArea).on('keydown', SinEnter);
    var $color = $('<input type="color" />').val(a.Color);
    var $activa = $('<input type="checkbox" />').prop('checked', a.Activa).prop('disabled', a.EsSinArea);
    var $guardar = $('<button type="button" class="btn btn-primary btn-sm">Guardar</button>');
    $guardar.on('click', function () {
        $guardar.prop('disabled', true);
        Llamar('GuardarArea', {
            id: a.IdArea,
            nombre: $.trim($nombre.val() || ''),
            color: $color.val(),
            activa: $activa.prop('checked') ? '1' : '0'
        }, function () { CargarTodo(); }, function (m) { $guardar.prop('disabled', false); Mostrar('#paError', m); });
    });
    return $('<tr></tr>').append(
        $('<td></td>').append($nombre),
        $('<td></td>').append($color),
        $('<td></td>').append($activa),
        $('<td></td>').text(nueva ? '' : a.Personas),
        $('<td></td>').append($guardar));
}

function LlenarFiltro() {
    var $f = $('#paFiltro');
    var actual = $f.val() || '';
    $f.empty().append('<option value="">Todas las áreas</option>');
    $.each(paAreas, function (i, a) {
        $f.append($('<option></option>').val(String(a.IdArea)).text(a.Nombre + (a.Activa ? '' : ' (inactiva)')));
    });
    $f.val(actual);
    if ($f.val() === null) { $f.val(''); }
}

function DibujarPersonas() {
    var texto = $.trim($('#paBuscar').val() || '').toLowerCase();
    var filtro = $('#paFiltro').val() || '';
    var $t = $('#paPersonas').empty();
    $.each(paPersonas, function (i, p) {
        if (texto && p.Nombre.toLowerCase().indexOf(texto) < 0) { return; }
        if (filtro && String(p.IdArea) !== filtro) { return; }
        var $sel = $('<select class="form-control input-sm"></select>');
        $.each(paAreas, function (j, a) {
            if (a.Activa || a.IdArea === p.IdArea) { $sel.append($('<option></option>').val(String(a.IdArea)).text(a.Nombre)); }
        });
        $sel.val(String(p.IdArea));
        $sel.on('change', function () {
            $sel.prop('disabled', true);
            Llamar('AsignarArea', { cod: p.CodUsuario, idArea: $sel.val() },
                function () { CargarTodo(); },
                function (m) { $sel.val(String(p.IdArea)).prop('disabled', false); Mostrar('#paError', m); });
        });
        $t.append($('<tr></tr>').append($('<td></td>').text(p.Nombre), $('<td></td>').append($sel)));
    });
    if (!$t.children().length) { $t.append('<tr><td colspan="2" class="text-muted">Sin personas para mostrar.</td></tr>'); }
}

function Mostrar(selector, texto) { $(selector).text(texto || '').toggle(!!texto); }

function Aviso(texto, tipo) {
    var $a = $('#paAviso');
    $a.removeClass('alert-success alert-warning alert-info alert-danger')
        .addClass('alert-' + (tipo || 'info')).text(texto).show();
    clearTimeout(paAvisoTimer);
    paAvisoTimer = setTimeout(function () { $a.hide(); }, 5000);
}
```

- [ ] **Step 4: Registrar en el proyecto web**

En `ReporteTareas/ReporteTareas.csproj`, igual que en el Task 8:
```xml
    <Content Include="Formulario\ParametrizacionAreasAgenda.aspx" />
```
```xml
    <Content Include="js\parametrizacionAreasAgenda.js" />
```
```xml
    <Compile Include="Formulario\ParametrizacionAreasAgenda.aspx.cs">
      <DependentUpon>ParametrizacionAreasAgenda.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\ParametrizacionAreasAgenda.aspx.designer.cs">
      <DependentUpon>ParametrizacionAreasAgenda.aspx</DependentUpon>
    </Compile>
```
(Mismas etiquetas que tenga el bloque de `TableroCostos.aspx.cs`.)

- [ ] **Step 5: BOM a los 4 archivos y comprobarlo** (mismo comando del Task 8, con estos archivos). Esperado: `ef bb bf` en los cuatro.

- [ ] **Step 6: Compilar.** Esperado: sin errores.

- [ ] **Step 7: Commit**

```bash
git add ReporteTareas/Formulario/ParametrizacionAreasAgenda.aspx ReporteTareas/Formulario/ParametrizacionAreasAgenda.aspx.cs ReporteTareas/Formulario/ParametrizacionAreasAgenda.aspx.designer.cs ReporteTareas/js/parametrizacionAreasAgenda.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(agenda): pantalla Areas de la agenda

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 10: Scripts de precarga, menú y verificación

**Files:**
- Create: `docs/sql/2026-10-15-agenda-precarga.sql`, `docs/sql/2026-10-15-agenda-menu.sql`, `docs/sql/2026-10-15-agenda-verificacion.sql`
- Modify: `docs/superpowers/specs/2026-10-15-agenda-equipo-entrega1-design.md` (§6: la acción se crea en el script de menú)

**Interfaces:**
- Consumes: tablas del Task 5; `MenuDos`, `PerfilMenu`, `RTA_Accion`, `RTA_PerfilAccion`.
- Produces: grupo de menú «Equipo y agenda» con dos hojas (`Agenda.aspx`, `ParametrizacionAreasAgenda.aspx`); acción `Agenda.VerTodos` ligada a `Agenda.aspx` y dada al perfil 10.

- [ ] **Step 1: Script de precarga**

`docs/sql/2026-10-15-agenda-precarga.sql`:
```sql
/* ============================================================================
   Equipo y agenda (Entrega 1) - precarga de areas
   ReporTarea  |  2026-10-15
   Requiere 2026-10-15-agenda-esquema.sql.
   1. Un area por cada Departamento distinto entre las personas activas, con
      las variantes que solo difieren en tildes o mayusculas unificadas
      (ADMINISTRACION / ADMINISTRACION con tilde). Colores por defecto.
   2. A cada persona activa con Departamento, su area. Las que no tienen
      Departamento quedan sin fila: caen en "Sin area".
   Repetible: no crea areas que ya existen (comparando sin tildes ni
   mayusculas) ni pisa el area que una persona ya tenga.
   No modifica R_Usuarios.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

/* El chequeo va en el MISMO lote que la carga: un RETURN solo corta su lote. */
IF NOT EXISTS (SELECT 1 FROM dbo.AG_Area WHERE EsSinArea = 1)
BEGIN
    RAISERROR('Falta correr 2026-10-15-agenda-esquema.sql.', 16, 1);
    RETURN;
END

BEGIN TRANSACTION;

DECLARE @Colores TABLE (Orden INT PRIMARY KEY, Color CHAR(7));
INSERT INTO @Colores (Orden, Color) VALUES
 (1, '#0078D4'), (2, '#107C10'), (3, '#CA5010'), (4, '#8764B8'), (5, '#E3008C'), (6, '#00B7C3'),
 (7, '#D13438'), (8, '#5C2D91'), (9, '#8E8CD8'), (10, '#498205'), (11, '#986F0B'), (12, '#004E8C');

;WITH Activos AS
(
    SELECT Cod   = LTRIM(RTRIM(u.Cod_Usuario)),
           Depto = LTRIM(RTRIM(ISNULL(u.Departamento, '')))
      FROM dbo.R_Usuarios AS u
     WHERE u.Usuario_Estado = 'A'
       AND u.EstadoUsuario IS NULL
       AND ISNULL(u.IdCliente, 0) = 0
       AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
),
Deptos AS
(
    SELECT Nombre   = MAX(Depto),
           Personas = COUNT(*)
      FROM Activos
     WHERE Depto <> ''
     GROUP BY UPPER(Depto) COLLATE Latin1_General_CI_AI
),
Nuevos AS
(
    SELECT d.Nombre, d.Personas,
           Fila = ROW_NUMBER() OVER (ORDER BY d.Personas DESC, d.Nombre)
      FROM Deptos AS d
     WHERE NOT EXISTS (SELECT 1 FROM dbo.AG_Area AS a
                        WHERE a.Nombre COLLATE Latin1_General_CI_AI = d.Nombre COLLATE Latin1_General_CI_AI)
)
INSERT INTO dbo.AG_Area (Nombre, Color, Activa, EsSinArea, Usuario_Registro)
SELECT n.Nombre, c.Color, 1, 0, 'MIGRACION'
  FROM Nuevos AS n
  JOIN @Colores AS c ON c.Orden = ((n.Fila - 1) % 12) + 1;

PRINT 'Areas creadas: ' + CONVERT(VARCHAR(10), @@ROWCOUNT);

;WITH Activos AS
(
    SELECT Cod   = LTRIM(RTRIM(u.Cod_Usuario)),
           Depto = LTRIM(RTRIM(ISNULL(u.Departamento, ''))),
           Orden = ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(u.Cod_Usuario)) ORDER BY u.Id_Usuario DESC)
      FROM dbo.R_Usuarios AS u
     WHERE u.Usuario_Estado = 'A'
       AND u.EstadoUsuario IS NULL
       AND ISNULL(u.IdCliente, 0) = 0
       AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
)
INSERT INTO dbo.AG_PersonaArea (Cod_Usuario, IdArea, Usuario_Registro)
SELECT x.Cod, a.IdArea, 'MIGRACION'
  FROM Activos AS x
  JOIN dbo.AG_Area AS a
    ON a.EsSinArea = 0
   AND a.Nombre COLLATE Latin1_General_CI_AI = x.Depto COLLATE Latin1_General_CI_AI
 WHERE x.Orden = 1
   AND x.Depto <> ''
   AND NOT EXISTS (SELECT 1 FROM dbo.AG_PersonaArea AS pa WHERE pa.Cod_Usuario = x.Cod);

PRINT 'Personas asignadas: ' + CONVERT(VARCHAR(10), @@ROWCOUNT);

COMMIT TRANSACTION;
GO

/* --------------------------------------------------------- resumen -------- */
SELECT a.Nombre, a.Color, a.Activa, a.EsSinArea, Personas = COUNT(pa.Cod_Usuario)
  FROM dbo.AG_Area AS a
  LEFT JOIN dbo.AG_PersonaArea AS pa ON pa.IdArea = a.IdArea
 GROUP BY a.Nombre, a.Color, a.Activa, a.EsSinArea
 ORDER BY a.EsSinArea, Personas DESC;

;WITH Activos AS
(
    SELECT Cod   = LTRIM(RTRIM(u.Cod_Usuario)),
           Depto = LTRIM(RTRIM(ISNULL(u.Departamento, '')))
      FROM dbo.R_Usuarios AS u
     WHERE u.Usuario_Estado = 'A' AND u.EstadoUsuario IS NULL AND ISNULL(u.IdCliente, 0) = 0
       AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
)
SELECT Veredicto = CASE WHEN COUNT(*) = 0 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA' END,
       ConDepartamentoSinArea = COUNT(*)
  FROM Activos AS x
 WHERE x.Depto <> ''
   AND NOT EXISTS (SELECT 1 FROM dbo.AG_PersonaArea AS pa WHERE pa.Cod_Usuario = x.Cod);
GO
```

- [ ] **Step 2: Script de menú**

`docs/sql/2026-10-15-agenda-menu.sql`:
```sql
/* ============================================================================
   Equipo y agenda (Entrega 1) - menu y accion Agenda.VerTodos
   ReporTarea  |  2026-10-15
   Se corre AL DESPLEGAR, junto con los binarios: antes, la opcion de menu
   llevaria a una pagina que todavia no existe en el servidor.
   Crea o actualiza:
     1. Grupo de menu "Equipo y agenda" (clon del grupo 5, Vacaciones & Permisos)
     2. Hoja Agenda.aspx "Equipo y agenda"
     3. Hoja ParametrizacionAreasAgenda.aspx "Areas de la agenda" (con tilde, via NCHAR)
     4. Visibilidad (PerfilMenu, Estado '0' = visible):
        grupo y Agenda.aspx -> todo perfil con usuarios internos activos, y el 18
        ParametrizacionAreasAgenda.aspx -> 14 y 18
     5. Accion Agenda.VerTodos en RTA_Accion, ligada a Agenda.aspx, y dada al
        perfil 10 (Gerencia). El 18 la tiene siempre por codigo.
   Repetible. Reversa al final, comentada.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID('tempdb..#ClonarMenu') IS NOT NULL DROP PROCEDURE #ClonarMenu;
GO
/* MenuDos tiene 18 columnas, varias NOT NULL con default: se clona una fila
   modelo en vez de enumerarlas. Mismo bloque que 2026-10-12-tablero-costos.sql. */
CREATE PROCEDURE #ClonarMenu
    @IdModelo INT,
    @IdNuevo  INT OUTPUT
AS
BEGIN
    DECLARE @cols NVARCHAR(MAX) = STUFF
    ((
        SELECT ',' + QUOTENAME(c.name)
          FROM sys.columns AS c
         WHERE c.object_id = OBJECT_ID('dbo.MenuDos')
           AND c.is_identity = 0 AND c.is_computed = 0 AND c.name <> 'Id_Menu'
         ORDER BY c.column_id
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 1, '');
    DECLARE @sql NVARCHAR(MAX);

    IF EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.MenuDos') AND name = 'Id_Menu' AND is_identity = 1)
    BEGIN
        SET @sql = N'INSERT INTO dbo.MenuDos (' + @cols + N') SELECT ' + @cols +
                   N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo; SET @IdOut = CONVERT(INT, SCOPE_IDENTITY());';
        EXEC sys.sp_executesql @sql, N'@Modelo INT, @IdOut INT OUTPUT', @Modelo = @IdModelo, @IdOut = @IdNuevo OUTPUT;
    END
    ELSE
    BEGIN
        SELECT @IdNuevo = ISNULL(MAX(Id_Menu), 0) + 1 FROM dbo.MenuDos WITH (TABLOCKX, HOLDLOCK);
        SET @sql = N'INSERT INTO dbo.MenuDos ([Id_Menu],' + @cols + N') SELECT @Id, ' + @cols +
                   N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo;';
        EXEC sys.sp_executesql @sql, N'@Modelo INT, @Id INT', @Modelo = @IdModelo, @Id = @IdNuevo;
    END
END
GO

BEGIN TRANSACTION;

DECLARE @TituloGrupo  VARCHAR(128) = 'Equipo y agenda';
DECLARE @TituloAreas  VARCHAR(128) = CONVERT(VARCHAR(128), NCHAR(193) + N'reas de la agenda');
DECLARE @IdModeloGrupo INT = 5;
DECLARE @IdModeloHoja  INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'TableroCostos.aspx');
DECLARE @IdGrupo INT, @IdAgenda INT, @IdAreas INT, @Orden INT;

IF NOT EXISTS (SELECT 1 FROM dbo.MenuDos WHERE Id_Menu = @IdModeloGrupo AND Es_Opcion_de_Menu = 1)
   OR @IdModeloHoja IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('No estan las filas modelo (grupo 5 o TableroCostos.aspx). Revisar el menu antes de seguir.', 16, 1);
    RETURN;
END

/* 1. grupo */
SELECT @IdGrupo = MIN(Id_Menu) FROM dbo.MenuDos WHERE Es_Opcion_de_Menu = 1 AND Titulo = @TituloGrupo;
IF @IdGrupo IS NULL
BEGIN
    EXEC #ClonarMenu @IdModeloGrupo, @IdGrupo OUTPUT;
    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1 FROM dbo.MenuDos WHERE Es_Opcion_de_Menu = 1 AND Id_Menu <> @IdGrupo;
    UPDATE dbo.MenuDos
       SET Titulo = @TituloGrupo, Descripcion = 'Agenda semanal del equipo', Orden_Opcion = @Orden
     WHERE Id_Menu = @IdGrupo;
    PRINT 'Grupo creado: ' + CONVERT(VARCHAR(10), @IdGrupo);
END

/* 2. hoja Agenda.aspx */
SELECT @IdAgenda = MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'Agenda.aspx';
IF @IdAgenda IS NULL
BEGIN
    EXEC #ClonarMenu @IdModeloHoja, @IdAgenda OUTPUT;
    UPDATE dbo.MenuDos
       SET Href = 'Agenda.aspx', Titulo = 'Equipo y agenda', Descripcion = 'Equipo por area y agenda semanal',
           Id_MenuPadre = @IdGrupo, Es_Opcion_de_Menu = 0, Orden_Opcion = 1
     WHERE Id_Menu = @IdAgenda;
    PRINT 'Hoja Agenda.aspx creada: ' + CONVERT(VARCHAR(10), @IdAgenda);
END

/* 3. hoja ParametrizacionAreasAgenda.aspx */
SELECT @IdAreas = MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'ParametrizacionAreasAgenda.aspx';
IF @IdAreas IS NULL
BEGIN
    EXEC #ClonarMenu @IdModeloHoja, @IdAreas OUTPUT;
    UPDATE dbo.MenuDos
       SET Href = 'ParametrizacionAreasAgenda.aspx', Titulo = @TituloAreas, Descripcion = 'Catalogo de areas de la agenda',
           Id_MenuPadre = @IdGrupo, Es_Opcion_de_Menu = 0, Orden_Opcion = 2
     WHERE Id_Menu = @IdAreas;
    PRINT 'Hoja ParametrizacionAreasAgenda.aspx creada: ' + CONVERT(VARCHAR(10), @IdAreas);
END

/* 4. visibilidad. Una hoja solo se ve si su grupo tambien es visible para el perfil. */
DECLARE @Todos TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Todos (IdPerfil)
SELECT DISTINCT u.Id_Perfil
  FROM dbo.R_Usuarios AS u
 WHERE u.Usuario_Estado = 'A' AND u.EstadoUsuario IS NULL AND ISNULL(u.IdCliente, 0) = 0
   AND u.Id_Perfil IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM @Todos WHERE IdPerfil = 18) INSERT INTO @Todos (IdPerfil) VALUES (18);

DECLARE @Vis TABLE (IdMenu INT, IdPerfil INT, PRIMARY KEY (IdMenu, IdPerfil));
INSERT INTO @Vis (IdMenu, IdPerfil) SELECT @IdGrupo, IdPerfil FROM @Todos;
INSERT INTO @Vis (IdMenu, IdPerfil) SELECT @IdAgenda, IdPerfil FROM @Todos;
INSERT INTO @Vis (IdMenu, IdPerfil) VALUES (@IdAreas, 14), (@IdAreas, 18);

UPDATE pm SET Estado = '0'
  FROM dbo.PerfilMenu AS pm
  JOIN @Vis AS v ON v.IdMenu = pm.id_Menu AND v.IdPerfil = pm.IdPerfil
 WHERE pm.Estado <> '0';

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT v.IdMenu, v.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM @Vis AS v
 WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu AS pm WHERE pm.id_Menu = v.IdMenu AND pm.IdPerfil = v.IdPerfil);

/* 5. accion */
IF NOT EXISTS (SELECT 1 FROM dbo.RTA_Accion WHERE Clave = 'Agenda.VerTodos')
BEGIN
    INSERT INTO dbo.RTA_Accion (Clave, Id_Menu, Titulo, Descripcion, Estado)
    VALUES ('Agenda.VerTodos', @IdAgenda, 'Ver la agenda de toda la empresa',
            'Ver a todas las personas activas y asignarles actividades, no solo a su cadena de jefatura.', 'A');
END

INSERT INTO dbo.RTA_PerfilAccion (IdPerfil, Id_Accion, Estado, Usuario_Registro, Fecha_Registro)
SELECT 10, a.Id_Accion, 'A', 'MIGRACION', GETDATE()
  FROM dbo.RTA_Accion AS a
 WHERE a.Clave = 'Agenda.VerTodos'
   AND NOT EXISTS (SELECT 1 FROM dbo.RTA_PerfilAccion AS pa WHERE pa.IdPerfil = 10 AND pa.Id_Accion = a.Id_Accion);

COMMIT TRANSACTION;
PRINT 'Menu y accion listos. Correr 2026-10-15-agenda-verificacion.sql.';
GO

/* ------------------------------------------------------------ REVERSA ------
BEGIN TRANSACTION;
DECLARE @A INT = (SELECT Id_Accion FROM dbo.RTA_Accion WHERE Clave = 'Agenda.VerTodos');
DELETE FROM dbo.RTA_PerfilAccion WHERE Id_Accion = @A;
DELETE FROM dbo.RTA_UsuarioAccion WHERE Id_Accion = @A;
DELETE FROM dbo.RTA_Accion WHERE Id_Accion = @A;
DECLARE @M TABLE (Id INT);
INSERT INTO @M SELECT Id_Menu FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) IN ('Agenda.aspx', 'ParametrizacionAreasAgenda.aspx');
INSERT INTO @M SELECT Id_Menu FROM dbo.MenuDos WHERE Es_Opcion_de_Menu = 1 AND Titulo = 'Equipo y agenda';
DELETE FROM dbo.PerfilMenu WHERE id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.R_UsuarioMenu WHERE Id_Menu IN (SELECT Id FROM @M);
DELETE FROM dbo.MenuDos WHERE Id_Menu IN (SELECT Id FROM @M);
COMMIT TRANSACTION;
---------------------------------------------------------------------------- */
```

- [ ] **Step 3: Script de verificación**

`docs/sql/2026-10-15-agenda-verificacion.sql`:
```sql
/* ============================================================================
   Equipo y agenda (Entrega 1) - verificacion (solo lectura)
   Correr despues de los tres scripts anteriores, y otra vez despues de
   publicar los binarios. Termina en VEREDICTO: OK o VEREDICTO: FALLA.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF OBJECT_ID('tempdb..#C2') IS NOT NULL DROP TABLE #C2;
IF OBJECT_ID('tempdb..#Ciclos') IS NOT NULL DROP TABLE #Ciclos;
IF OBJECT_ID('tempdb..#Dif') IS NOT NULL DROP TABLE #Dif;

DECLARE @Fallos INT = 0;

/* 1. una sola "Sin area", activa */
DECLARE @SinArea INT = (SELECT COUNT(*) FROM dbo.AG_Area WHERE EsSinArea = 1 AND Activa = 1);
SELECT Chequeo = '1. filas Sin area activas (esperado 1)', Valor = @SinArea;
IF @SinArea <> 1 SET @Fallos += 1;

/* 2. personas activas con Departamento pero sin area */
;WITH Activos AS
(
    SELECT Cod = LTRIM(RTRIM(u.Cod_Usuario)), Depto = LTRIM(RTRIM(ISNULL(u.Departamento, '')))
      FROM dbo.R_Usuarios AS u
     WHERE u.Usuario_Estado = 'A' AND u.EstadoUsuario IS NULL AND ISNULL(u.IdCliente, 0) = 0
       AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
)
SELECT Chequeo = '2. con Departamento y sin area (esperado 0)', Valor = COUNT(*)
  INTO #C2
  FROM Activos AS x
 WHERE x.Depto <> '' AND NOT EXISTS (SELECT 1 FROM dbo.AG_PersonaArea AS pa WHERE pa.Cod_Usuario = x.Cod);
SELECT * FROM #C2;
IF (SELECT Valor FROM #C2) <> 0 SET @Fallos += 1;

/* 3. ciclos de jefes entre los activos */
;WITH A AS
(
    SELECT Cod = LTRIM(RTRIM(Cod_Usuario)), Jefe = LTRIM(RTRIM(ISNULL(Cod_Jefe_Inm, '')))
      FROM dbo.R_Usuarios
     WHERE Usuario_Estado = 'A' AND EstadoUsuario IS NULL AND ISNULL(IdCliente, 0) = 0
),
R AS
(
    SELECT Origen = Cod, Actual = Jefe, Nivel = 1 FROM A WHERE Jefe <> '' AND Jefe <> Cod
    UNION ALL
    SELECT R.Origen, A.Jefe, R.Nivel + 1
      FROM R JOIN A ON A.Cod = R.Actual
     WHERE A.Jefe <> '' AND A.Jefe <> A.Cod AND R.Actual <> R.Origen AND R.Nivel < 50
)
SELECT DISTINCT Origen INTO #Ciclos FROM R WHERE Actual = Origen OPTION (MAXRECURSION 60);
SELECT Chequeo = '3. personas en un ciclo de jefes (esperado 0)', Valor = COUNT(*) FROM #Ciclos;
SELECT EnCiclo = Origen FROM #Ciclos;
IF EXISTS (SELECT 1 FROM #Ciclos) SET @Fallos += 1;

/* 4. menu */
DECLARE @IdAgenda INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'Agenda.aspx');
DECLARE @IdAreas  INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'ParametrizacionAreasAgenda.aspx');
DECLARE @IdGrupo  INT = (SELECT Id_MenuPadre FROM dbo.MenuDos WHERE Id_Menu = @IdAgenda);
SELECT Chequeo = '4. ids de menu', IdGrupo = @IdGrupo, IdAgenda = @IdAgenda, IdAreas = @IdAreas;
IF @IdAgenda IS NULL OR @IdAreas IS NULL OR @IdGrupo IS NULL SET @Fallos += 1;

DECLARE @Todos TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Todos SELECT DISTINCT Id_Perfil FROM dbo.R_Usuarios
 WHERE Usuario_Estado = 'A' AND EstadoUsuario IS NULL AND ISNULL(IdCliente, 0) = 0 AND Id_Perfil IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM @Todos WHERE IdPerfil = 18) INSERT INTO @Todos VALUES (18);

DECLARE @E TABLE (IdMenu INT, IdPerfil INT);
INSERT INTO @E SELECT @IdGrupo, IdPerfil FROM @Todos;
INSERT INTO @E SELECT @IdAgenda, IdPerfil FROM @Todos;
INSERT INTO @E VALUES (@IdAreas, 14), (@IdAreas, 18);

;WITH R AS (SELECT pm.id_Menu, pm.IdPerfil FROM dbo.PerfilMenu AS pm
             WHERE pm.Estado = '0' AND pm.id_Menu IN (@IdGrupo, @IdAgenda, @IdAreas))
SELECT Diferencia = CASE WHEN r.id_Menu IS NULL THEN 'FALTA en el menu' ELSE 'SOBRA en el menu' END,
       IdMenu = ISNULL(e.IdMenu, r.id_Menu), IdPerfil = ISNULL(e.IdPerfil, r.IdPerfil)
  INTO #Dif
  FROM @E AS e FULL JOIN R AS r ON r.id_Menu = e.IdMenu AND r.IdPerfil = e.IdPerfil
 WHERE e.IdMenu IS NULL OR r.id_Menu IS NULL;
SELECT * FROM #Dif ORDER BY IdMenu, IdPerfil;
IF EXISTS (SELECT 1 FROM #Dif) SET @Fallos += 1;

/* 5. accion Agenda.VerTodos: ligada a Agenda.aspx y dada solo al perfil 10 */
SELECT Chequeo = '5. accion', a.Clave, a.Id_Menu, a.Estado, pa.IdPerfil, PerfilEstado = pa.Estado
  FROM dbo.RTA_Accion AS a
  LEFT JOIN dbo.RTA_PerfilAccion AS pa ON pa.Id_Accion = a.Id_Accion
 WHERE a.Clave = 'Agenda.VerTodos';
IF NOT EXISTS (SELECT 1 FROM dbo.RTA_Accion AS a JOIN dbo.RTA_PerfilAccion AS pa ON pa.Id_Accion = a.Id_Accion
                WHERE a.Clave = 'Agenda.VerTodos' AND a.Id_Menu = @IdAgenda AND a.Estado = 'A'
                  AND pa.IdPerfil = 10 AND pa.Estado = 'A')
   OR EXISTS (SELECT 1 FROM dbo.RTA_Accion AS a JOIN dbo.RTA_PerfilAccion AS pa ON pa.Id_Accion = a.Id_Accion
               WHERE a.Clave = 'Agenda.VerTodos' AND pa.IdPerfil <> 10 AND pa.Estado = 'A')
    SET @Fallos += 1;

/* 6. informativo: personas cuyo jefe no esta entre los activos (solo las ve VerTodos) */
;WITH A AS
(
    SELECT Cod = LTRIM(RTRIM(Cod_Usuario)), Nombre = Nom_Usuario, Jefe = LTRIM(RTRIM(ISNULL(Cod_Jefe_Inm, '')))
      FROM dbo.R_Usuarios
     WHERE Usuario_Estado = 'A' AND EstadoUsuario IS NULL AND ISNULL(IdCliente, 0) = 0
)
SELECT Informativo = '6. jefe fuera de los activos', a.Cod, a.Nombre, a.Jefe
  FROM A AS a
 WHERE a.Jefe NOT IN ('', '0') AND NOT EXISTS (SELECT 1 FROM A AS j WHERE j.Cod = a.Jefe);

SELECT Veredicto = CASE WHEN @Fallos = 0 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA' END, Fallos = @Fallos;
```

- [ ] **Step 4: Corregir la spec**

En `docs/superpowers/specs/2026-10-15-agenda-equipo-entrega1-design.md`, §6:
- en el punto 1 (esquema), cambiar «…los `Sp_RTA_Agenda*`, y la acción `Agenda.VerTodos` en `RTA_Accion` ligada al `Id_Menu` de la agenda.» por «…y los `Sp_RTA_Agenda*`.»
- en el punto 3 (menú), agregar al final: «Crea además la acción `Agenda.VerTodos` en `RTA_Accion`, ligada al `Id_Menu` de `Agenda.aspx`, porque necesita ese id. Se corre al desplegar, con los binarios.»

- [ ] **Step 5: Pedir autorización y correr la precarga**

Decir al usuario: «La precarga crea las áreas desde `Departamento` y le asigna a cada persona la suya, en las tablas nuevas. No toca `R_Usuarios` ni el menú. ¿La corro? El script de menú lo dejo para el despliegue». Esperar el sí. Correrla.
Esperado: la tabla de resumen (SERVICIOS 46, CAS 20, COMERCIAL 16…, una sola ADMINISTRACION) y `VEREDICTO: OK`. Correrla otra vez: `Areas creadas: 0`, `Personas asignadas: 0`, `VEREDICTO: OK`.

**No correr** `2026-10-15-agenda-menu.sql` en este task.

- [ ] **Step 6: Commit**

```bash
git add docs/sql/2026-10-15-agenda-precarga.sql docs/sql/2026-10-15-agenda-menu.sql docs/sql/2026-10-15-agenda-verificacion.sql docs/superpowers/specs/2026-10-15-agenda-equipo-entrega1-design.md
git commit -m "feat(agenda): precarga de areas, menu y verificacion

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 11: Prueba en navegador, despliegue y paquete

**Files:**
- Modify: `DESPLIEGUE.md` (sección 19 al final)
- Modify: paquete `ReporteTareas/obj/Release/Package/PackageTmp/` y binarios trackeados (lo genera MSBuild)

- [ ] **Step 1: Levantar el sitio en local**

Con IIS Express **de 32 bits** (`C:\Program Files (x86)\IIS Express\iisexpress.exe`; el de 64 bits no levanta el sitio por Pechkin). Entrar con el usuario de quien ejecuta (login por AD). Si el usuario es perfil 18, tiene `Agenda.VerTodos`.

**Límite de esta prueba:** en local solo se puede entrar con el propio usuario de AD. La spec pide probar en navegador
con tres roles; los roles «jefe» y «colaborador» no se pueden probar aquí sin sus credenciales. Sus reglas las cubren las
pruebas unitarias (Tasks 1 a 3) y la prueba en producción (§19.4). Decirlo explícitamente en el reporte final.

- [ ] **Step 2: Recorrer la pantalla con Playwright**

Abrir `Formulario/Agenda.aspx` y comprobar, con captura de cada paso:
1. Llega a la vista de equipo con tarjetas por área (SERVICIOS, CAS, COMERCIAL…) y «Sin área» al final.
2. El buscador encuentra a una persona por parte del nombre.
3. Abrir un área, abrir una persona: se ven 5 columnas y hoy resaltado; las flechas cambian de semana y «Hoy» vuelve.
4. «Mi agenda» → «+ Nueva actividad» con 2 entregables para un viernes: aparece en el viernes con `0/2`. Hacer **doble clic** en «Guardar»: debe quedar **una** actividad (Review Focus 1).
5. Abrir el detalle, marcar los dos entregables: la tarjeta pasa a verde.
6. «Editar»: cambiar la fecha a un sábado → el formulario lo rechaza. Cambiar el texto de un entregable → al guardar, ese entregable queda sin marcar y el otro sigue marcado.
7. «Anular» (dos clics): la actividad desaparece.
8. Asignar una actividad a otra persona **de prueba que tenga correo**: el aviso dice «Actividad asignada a …» y llega el correo con tildes correctas y el botón «Ver en mi agenda». Asignar a alguien **sin correo** (buscarlo con `SELECT Cod_Usuario, Nom_Usuario FROM R_Usuarios WHERE Usuario_Estado='A' AND EstadoUsuario IS NULL AND ISNULL(E_Mail,'')=''`, con autorización): el aviso es amarillo, la actividad queda guardada, y `AG_CorreoBitacora` tiene la fila con `Enviado = 0` (Review Focus 2).
9. Un título con tildes y `<b>hola</b>` se ve tal cual, sin negrita (Review Focus 3).
10. Cambiar de semana varias veces muy rápido: termina mostrando la última semana pedida (Review Focus 4).
11. `Formulario/ParametrizacionAreasAgenda.aspx`: crear «ZZ Prueba» con un color, renombrarla, desactivarla; intentar renombrar «Sin área» (no se puede: la caja está desactivada); mover a una persona de área y devolverla.

Si algo falla: anotar el paso, corregir en el task que corresponde, volver a compilar y repetir desde el paso que falló.

- [ ] **Step 3: Limpiar lo creado en la prueba**

Con autorización del usuario: anular o borrar las actividades de prueba y el área «ZZ Prueba». Para listarlas:
```sql
SELECT IdActividad, Titulo, Cod_Responsable, Cod_AsignadoPor, Fecha_Registro FROM dbo.AG_Actividad ORDER BY IdActividad;
SELECT IdArea, Nombre FROM dbo.AG_Area WHERE Nombre LIKE 'ZZ%';
```
Borrar en este orden: `AG_CorreoBitacora`, `AG_Entregable`, `AG_Actividad` de esas actividades; `AG_PersonaArea` y `AG_Area` de «ZZ Prueba». Avisar a la persona de prueba que recibió el correo.

- [ ] **Step 4: Escribir la sección 19 de `DESPLIEGUE.md`**

Agregar al final de `DESPLIEGUE.md`:
```markdown

## 19. Equipo y agenda (Entrega 1 de «Equipo, Calendario y Procesos»)

Spec: `docs/superpowers/specs/2026-10-15-agenda-equipo-entrega1-design.md`.

### 19.1 Qué es

Dos pantallas nuevas en el grupo de menú «Equipo y agenda»:

- **«Equipo y agenda»** (`Agenda.aspx`): tarjetas por área con la carga de la semana, y la agenda de lunes a viernes de
  cada persona, con actividades, entregables y atrasadas. La ve cualquier usuario interno: cada uno a sí mismo, cada jefe a
  toda su cadena hacia abajo (`Cod_Jefe_Inm`), y a todos quien tiene la acción `Agenda.VerTodos` (perfil 10 «Gerencia»; el
  18 siempre). Asignarle algo a otra persona le envía un correo.
- **«Áreas de la agenda»** (`ParametrizacionAreasAgenda.aspx`): catálogo de áreas con color y área de cada persona. Perfiles
  14 y 18.

Las actividades **no** son las tareas de servicio de Aranda.

### 19.2 Orden

1. `docs/sql/2026-10-15-agenda-esquema.sql` (ya aplicado en desarrollo, que es la misma base: repetirlo no hace nada).
2. `docs/sql/2026-10-15-agenda-precarga.sql` (ídem).
3. `docs/sql/2026-10-15-agenda-menu.sql` — **recién ahora**: antes, la opción de menú llevaría a una página que no existe.
4. Binarios **juntos** (`ReporteTareas.dll` y los tres `.exe` de capa: `CapaEntidad`, `CapaNegocio`, `CapaDato`), más
   `Formulario\Agenda.aspx`, `Formulario\ParametrizacionAreasAgenda.aspx`, `Formulario\AdministrarAgenda.ashx`,
   `js\agenda.js` y `js\parametrizacionAreasAgenda.js`.
5. `docs/sql/2026-10-15-agenda-verificacion.sql` — debe dar `VEREDICTO: OK`.

### 19.3 Quién cuenta como activo

`Usuario_Estado = 'A'` y `EstadoUsuario` NULL, solo internos: el criterio de los selectores de jefe. Está en un solo lugar,
`Sp_RTA_AgendaPersonasActivas`. Quien no cumple el criterio no entra a la agenda (ve un aviso para que Talento Humano revise
su estado), y la gente cuyo jefe no cumple el criterio solo la ve `Agenda.VerTodos`. La verificación lista a esas personas
(chequeo 6).

### 19.4 Lista para probar en producción

- **Gerencia (perfil 10):** ve todas las áreas, asigna una actividad a alguien y a esa persona le llega el correo.
- **Un jefe:** ve solo su cadena, aun buscando por nombre a alguien de otra área.
- **Un colaborador:** entra directo a su agenda, marca sus entregables, no puede mover la fecha de lo que le asignaron y no
  abre «Áreas de la agenda» (escribir la dirección lo manda a Principal).

### 19.5 Reversa

Reponer los binarios anteriores, todos juntos, y correr la reversa comentada al final de `2026-10-15-agenda-menu.sql`. Las
tablas `AG_*` pueden quedar: no las usa nada más.
```

- [ ] **Step 5: Correr toda la suite y regenerar el paquete**

Suite completa: `Correcto: 674`. Luego, en PowerShell:
```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "C:\respaldodisco\Desarrollo\PRY_Sistema ReporteTareas\ReporteTareas\ReporteTareas.csproj" -t:Package -p:Configuration=Release -v:minimal -nologo | Select-String -Pattern "error|-> "
```
Esperado: sin errores y la línea `ReporteTareas -> ...ReporteTareas.dll`. Comprobar que el paquete trae los archivos nuevos:
```bash
P=ReporteTareas/obj/Release/Package/PackageTmp
ls $P/Formulario/Agenda.aspx $P/Formulario/ParametrizacionAreasAgenda.aspx $P/Formulario/AdministrarAgenda.ashx $P/js/agenda.js $P/js/parametrizacionAreasAgenda.js
for f in $P/Formulario/Agenda.aspx $P/Formulario/ParametrizacionAreasAgenda.aspx; do head -c3 "$f" | od -An -tx1; done
```
y, en PowerShell, que el `.exe` de negocio trae la regla:
```powershell
$neg = [System.Reflection.Assembly]::LoadFrom("C:\respaldodisco\Desarrollo\PRY_Sistema ReporteTareas\ReporteTareas\obj\Release\Package\PackageTmp\bin\CapaNegocio.exe")
$null -ne $neg.GetType('CapaNegocio.NegAgendaVista')
```
Esperado: los 5 archivos existen, las dos `.aspx` empiezan con `ef bb bf`, y `True`.

- [ ] **Step 6: Commits**

```bash
git add DESPLIEGUE.md
git commit -m "docs(agenda): despliegue de Equipo y agenda

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
git add -u CapaNegocio/bin CapaDato/bin CapaEntidad/bin ReporteTareas/bin ReporteTareas/obj/Release/Package/PackageTmp
git add ReporteTareas/obj/Release/Package/PackageTmp/Formulario/Agenda.aspx ReporteTareas/obj/Release/Package/PackageTmp/Formulario/ParametrizacionAreasAgenda.aspx ReporteTareas/obj/Release/Package/PackageTmp/Formulario/AdministrarAgenda.ashx ReporteTareas/obj/Release/Package/PackageTmp/js/agenda.js ReporteTareas/obj/Release/Package/PackageTmp/js/parametrizacionAreasAgenda.js
git commit -m "build(agenda): paquete con Equipo y agenda

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

Antes del segundo commit, revisar `git status --short`: no debe entrar ningún archivo ajeno a esta entrega (los `??` que ya estaban al empezar se dejan fuera).
