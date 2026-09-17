# Perfil: edición por Talento Humano — Entrega 2 (la pantalla)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Darle a Talento Humano una pantalla propia donde buscar a cualquier empleado y editar su perfil, reusando sin duplicar las mismas fichas que ya usa «Mi perfil».

**Architecture:** Las 498 líneas de marcado de `MiPerfil.aspx` se mudan una sola vez a un control de usuario, `Controles/PerfilFichas.ascx`, que las dos pantallas incluyen. `miPerfil.js` gana una variable, `_codObjetivo`, que viaja en las llamadas: vacía en «Mi perfil» —y entonces el servidor devuelve el de la sesión, como hoy— y fijada por el buscador en la pantalla nueva. La regla que decide si ese código ajeno se acepta ya existe y está desplegada: es la entrega 1.

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.8 en el web, 4.6.1 en las capas), SQL Server, jQuery + Bootstrap 3, MSTest v1 (ensamblado de VS2019, sin NuGet), `vstest.console.exe`.

**Spec:** `docs/superpowers/specs/2026-09-16-perfil-edicion-rrhh-design.md`

**Entrega anterior:** `docs/superpowers/plans/2026-09-16-perfil-edicion-rrhh-entrega1.md`

## Global Constraints

- **La entrega 1 tiene que estar desplegada antes que ésta.** Esta pantalla pide perfiles ajenos; sin la entrega 1, el handler no sabe autorizarlos y los rechaza todos. Verificarlo es el Step 1 de la Task 1.
- **Perfiles autorizados: 14 (Talento Humano) y 18 (Super Admin).** La lista vive en **un solo sitio**, `CapaNegocio/NegPerfilAcceso.cs` → `public static readonly int[] PerfilesRRHH = { 14, 18 }`. Nadie la vuelve a escribir.
- **La pestaña «Equipo» NO se dibuja en la pantalla nueva.** `ListaEquipo` y `PerfilEquipo` siguen tomando al jefe **de la sesión**, así que ahí mostraría el equipo de quien mira, con el nombre de otro en la cabecera.
- **El menú no es la barrera.** Los handlers son alcanzables por HTTP directo por cualquiera con sesión. Toda acción nueva comprueba el perfil en el servidor.
- **Los `.csproj` son de estilo antiguo:** todo archivo nuevo va listado (`<Content Include>` para `.aspx`/`.ascx`, `<Compile Include>` con su `<DependentUpon>` para el code-behind). Lo que no se lista, no se compila, y nadie avisa.
- **El `.aspx` y el `.ascx` nuevos van con BOM.** `Web.config` declara `windows-1252` y no trae `fileEncoding`; sin BOM salen con caracteres raros en producción. Ya le pasó a `MiPerfil.aspx`.
- **Convención de comentarios: C#, SQL y JavaScript sin tildes.** Los textos que ve el usuario, con tildes.
- **Compilar con el MSBuild de VS2019, desde PowerShell** (desde Git Bash los `/p:` sufren la conversión de rutas de MSYS y sale `MSB1008`):
  `& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal`
- **Pruebas:** `& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll`. Al empezar esta entrega son **201**.
- **Los scripts SQL no se ejecutan durante la implementación.** Se escriben, se revisan y los corre el usuario en producción: es la norma de `docs/sql/`. El SQL va siempre antes que los binarios.
- **DataTables no carga en este sistema.** Ninguna tabla nueva depende de él.

---

## Estructura de archivos

| Archivo | Responsabilidad | Acción |
|---|---|---|
| `CapaNegocio/NegPerfilCampos.cs` | Suma `ValidarFiltroPersonal`: el filtro del buscador exige 2 caracteres | Modificar |
| `CapaPruebas/NegPerfilCamposTests.cs` | Las pruebas de esa regla | Modificar |
| `CapaEntidad/EntPerfilEquipoItem.cs` | Se reusa tal cual para las filas del listado de personal | — |
| `docs/sql/2026-09-17-perfil-personal-lista.sql` | `Sp_RTA_PerfilPersonalLista` | **Crear** |
| `CapaDato/DaoPerfil.cs` | `ListaPersonal(filtro)` | Modificar |
| `CapaNegocio/NegPerfil.cs` | `ListaPersonal(filtro)` | Modificar |
| `ReporteTareas/clases/PerfilIdentidad.cs` | Suma `EsRRHH(context)` | Modificar |
| `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` | La acción `ListaPersonal`, sólo para 14 y 18 | Modificar |
| `ReporteTareas/Controles/PerfilFichas.ascx` | **Las fichas, una sola vez.** Todo el marcado que hoy vive en `MiPerfil.aspx` | **Crear** |
| `ReporteTareas/Formulario/MiPerfil.aspx` | Queda en ~12 líneas: incluye el control | Modificar |
| `ReporteTareas/js/miPerfil.js` | Aprende `_codObjetivo` y lo manda por los cuatro caminos | Modificar |
| `ReporteTareas/Formulario/PerfilesPersonal.aspx` (+ `.cs`, `.designer.cs`) | La pantalla nueva: buscador + el control | **Crear** |
| `ReporteTareas/js/perfilesPersonal.js` | Pintar la lista y fijar `_codObjetivo` | **Crear** |
| `docs/sql/2026-09-17-perfil-personal-menu.sql` | `MenuDos` + `PerfilMenu` para 14 y 18 | **Crear** |
| `ReporteTareas/Formulario/DescargarPerfil.ashx.cs` | Dos textos que hablan de la persona equivocada | Modificar |
| `DESPLIEGUE.md` | La sección de esta entrega | Modificar |

---

## Task 1: La regla del filtro del buscador

Es lo único de esta entrega que se puede probar de verdad, y es lo que impide que un POST directo se lleve la plantilla entera. Va primera y va con TDD.

**Files:**
- Modify: `CapaNegocio/NegPerfilCampos.cs`
- Modify: `CapaPruebas/NegPerfilCamposTests.cs`

**Interfaces:**
- Consumes: nada.
- Produces: `CapaNegocio.NegPerfilCampos.ValidarFiltroPersonal(string filtro)` → `string` (cadena vacía si es válido; el mensaje de error si no). Lo consume `NegPerfil.ListaPersonal` en la Task 3.

- [ ] **Step 1: Confirmar que la entrega 1 está desplegada**

Antes de escribir una línea. En la base de producción:

```sql
SELECT procedimiento = OBJECT_NAME(p.object_id),
       tieneUsuAccion = MAX(CASE WHEN pa.name = '@Usu_Accion' THEN 1 ELSE 0 END)
  FROM sys.procedures p
  LEFT JOIN sys.parameters pa ON pa.object_id = p.object_id
 WHERE OBJECT_NAME(p.object_id) LIKE 'Sp_RTA_Perfil%'
 GROUP BY OBJECT_NAME(p.object_id);
```

Esperado: **14 con `tieneUsuAccion = 1`**. Si dan 0, la entrega 1 no está en la base y esta entrega no se puede probar contra ella — pararse y avisar.

- [ ] **Step 2: Escribir las pruebas, que todavía no compilan**

Agregar al final de `CapaPruebas/NegPerfilCamposTests.cs`, dentro de la clase:

```csharp
        /* ------------------------------------ filtro del buscador de personal --- */

        /// <summary>
        /// El buscador de la pantalla de Talento Humano no lista a nadie hasta que
        /// se escriben dos caracteres. La regla vive aca y no solo en el navegador
        /// porque el handler es alcanzable por HTTP directo: sin esto, un POST con
        /// el filtro vacio se lleva la plantilla entera de una sola vez.
        /// </summary>
        [TestMethod]
        public void ValidarFiltroPersonal_Vacio_NoSePermite()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFiltroPersonal(""));
        }

        [TestMethod]
        public void ValidarFiltroPersonal_Nulo_NoSePermite()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFiltroPersonal(null));
        }

        /// <summary>
        /// Solo espacios es lo mismo que vacio. Sin el recorte, tres espacios
        /// pasarian la comprobacion de largo y devolverian a todo el personal.
        /// </summary>
        [TestMethod]
        public void ValidarFiltroPersonal_SoloEspacios_NoSePermite()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFiltroPersonal("   "));
        }

        [TestMethod]
        public void ValidarFiltroPersonal_UnCaracter_NoSePermite()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFiltroPersonal("a"));
        }

        [TestMethod]
        public void ValidarFiltroPersonal_UnCaracterConRelleno_NoSePermite()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFiltroPersonal("  a  "));
        }

        [TestMethod]
        public void ValidarFiltroPersonal_DosCaracteres_SePermite()
        {
            Assert.AreEqual("", NegPerfilCampos.ValidarFiltroPersonal("ab"));
        }

        [TestMethod]
        public void ValidarFiltroPersonal_NombreCompleto_SePermite()
        {
            Assert.AreEqual("", NegPerfilCampos.ValidarFiltroPersonal("Rodriguez"));
        }
```

- [ ] **Step 3: Compilar y verificar que NO compila**

```
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
```

Esperado: **FALLA** con `CS0117` o `CS1061` sobre `ValidarFiltroPersonal`.

- [ ] **Step 4: Escribir la regla**

Agregar a `CapaNegocio/NegPerfilCampos.cs`, junto a las otras validaciones:

```csharp
        /// <summary>
        /// Cuantos caracteres exige el buscador de personal antes de traer nada.
        ///
        /// No es un capricho de interfaz: sin minimo, un filtro vacio devuelve a
        /// todo el personal activo de una sola vez, en una tabla sin paginar y en
        /// un sistema donde DataTables no carga. Dos caracteres bastan para que
        /// quien busca sepa a quien busca.
        /// </summary>
        public const int MinimoFiltroPersonal = 2;

        /// <summary>
        /// Valida el filtro del buscador de personal. Cadena vacia si sirve.
        ///
        /// Recorta antes de medir: sin eso, tres espacios pasan la comprobacion de
        /// largo y el procedimiento recibe un filtro que no filtra nada.
        /// </summary>
        public static string ValidarFiltroPersonal(string filtro)
        {
            string limpio = (filtro ?? "").Trim();

            if (limpio.Length < MinimoFiltroPersonal)
            {
                return "Escriba al menos " + MinimoFiltroPersonal + " caracteres para buscar.";
            }

            return "";
        }
```

- [ ] **Step 5: Compilar y correr las pruebas**

```
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: **208 de 208** (201 + 7). Anotar el número.

- [ ] **Step 6: Commit**

```bash
git add CapaNegocio/NegPerfilCampos.cs CapaPruebas/NegPerfilCamposTests.cs
git commit -m "feat(perfil): el buscador de personal exige dos caracteres

La regla va en CapaNegocio y no solo en el navegador: el handler es
alcanzable por HTTP directo y un filtro vacio devolveria a todo el
personal activo en una tabla sin paginar.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 2: `Sp_RTA_PerfilPersonalLista`

**Files:**
- Create: `docs/sql/2026-09-17-perfil-personal-lista.sql`

**Interfaces:**
- Consumes: nada.
- Produces: `dbo.Sp_RTA_PerfilPersonalLista @Filtro VARCHAR(100)`, que devuelve un result set con las columnas `CodUsuario`, `NombreCompleto`, `Cargo`, `Area`, `Ciudad` — **las mismas cinco y con los mismos nombres** que `Sp_RTA_PerfilEquipoLista`, para que `DaoPerfil` reuse `EntPerfilEquipoItem` sin una entidad nueva.

> **No se ejecuta.** Se escribe, se revisa y lo corre el usuario. Dejar al final del archivo, dentro de un bloque `/* ... */` rotulado `VERIFICACION (correr a mano despues del script)`, una consulta que llame al procedimiento con un filtro de prueba.

- [ ] **Step 1: Escribir el script**

Crear `docs/sql/2026-09-17-perfil-personal-lista.sql`. Es `Sp_RTA_PerfilEquipoLista` (de `docs/sql/2026-09-15-perfil-colaborador-fase3b.sql:40`) **sin el filtro por `Cod_Jefe_Inm` y sin la guarda del código del jefe**, porque aquí no hay jefe. Todo lo demás se conserva.

```sql
/* ============================================================================
   Perfil del colaborador: el listado de personal para Talento Humano
   ReporTarea  |  2026-09-17

   PENDIENTE DE EJECUTAR. Es el paso 1 de la entrega 2; el orden completo esta
   en DESPLIEGUE.md.

   ----------------------------------------------------------------------------
   Que hace y por que

   Sp_RTA_PerfilEquipoLista devuelve el equipo directo de una jefatura. Este
   procedimiento es el mismo SELECT sin el filtro por Cod_Jefe_Inm: el personal
   activo entero, para que Talento Humano busque a cualquiera.

   Quien puede llamarlo NO se decide aca. El procedimiento no conoce perfiles;
   la barrera esta en AdministrarPerfil.ashx.cs, que solo expone la accion a los
   perfiles 14 y 18. Mismo reparto que el resto del modulo: SQL hace, C# decide.

   Conserva a proposito dos cosas del original:

   1. NO lista a quien tiene el Cod_Usuario repetido entre usuarios activos.
      Sp_RTA_PerfilColaborador se niega a abrir esos perfiles -no puede saber de
      cual de dos personas son los datos-, asi que listarlos seria ofrecer un
      boton que nunca funciona. Son 4 usuarios; necesitan que alguien les
      corrija el codigo en R_Usuarios, y eso no se arregla desde esta pantalla.

   2. LTRIM(RTRIM(...)) en las comparaciones. Los codigos traen relleno.

   El filtro es obligatorio y de 2 caracteres como minimo. Eso lo impone
   NegPerfilCampos.ValidarFiltroPersonal antes de llegar aca; el WHERE de abajo
   lo repite por si alguien llama al procedimiento a mano.

   Idempotente: DROP + CREATE.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '== Perfil: listado de personal - inicio ==';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilPersonalLista','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilPersonalLista;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilPersonalLista
    @Filtro VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @F VARCHAR(100) = LTRIM(RTRIM(ISNULL(@Filtro, '')));

    /* Sin filtro no se devuelve nada. La regla de los 2 caracteres vive en
       NegPerfilCampos.ValidarFiltroPersonal, que es donde se puede probar; esto
       es la red por si alguien llega al procedimiento por otro camino. */
    IF LEN(@F) < 2
    BEGIN
        SELECT  CodUsuario     = CAST(NULL AS VARCHAR(50)),
                NombreCompleto = CAST(NULL AS VARCHAR(200)),
                Cargo          = CAST(NULL AS VARCHAR(200)),
                Area           = CAST(NULL AS VARCHAR(200)),
                Ciudad         = CAST(NULL AS VARCHAR(200))
         WHERE 1 = 0;
        RETURN;
    END

    SELECT  CodUsuario     = LTRIM(RTRIM(u.Cod_Usuario)),
            NombreCompleto = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario),
            Cargo          = ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo),
            Area           = ISNULL(NULLIF(LTRIM(RTRIM(e.AreaTrabajo)), ''), u.Departamento),
            Ciudad         = e.Ciudad
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados e ON e.Cod_Usuario = u.Cod_Usuario
     WHERE  ISNULL(u.EstadoUsuario, 0) = 0

       /* Misma regla que la lista de equipo: si el codigo esta repetido entre
          usuarios activos, Sp_RTA_PerfilColaborador se niega a abrir ese perfil.
          Mejor no listarlo que listarlo con un boton que nunca funciona. */
       /* La comparacion va CRUDA, sin LTRIM/RTRIM, y no es un olvido: asi la
          hace el original y asi la hace Sp_RTA_PerfilColaborador, que es el
          que de verdad se niega a abrir estos perfiles. Esta subconsulta
          tiene que contar como cuenta aquel: si recortara, dos codigos que
          difieran solo por relleno a la izquierda se contarian como
          repetidos y los DOS quedarian fuera de la lista, aunque el gate
          deje abrir cada uno. Esconder a alguien que si se puede gestionar
          es peor que mostrarlo. */
       AND  (SELECT COUNT(*) FROM dbo.R_Usuarios r
              WHERE r.Cod_Usuario = u.Cod_Usuario
                AND ISNULL(r.EstadoUsuario,0) = 0) = 1

       AND  (ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario) LIKE '%' + @F + '%'
             OR LTRIM(RTRIM(u.Cod_Usuario)) LIKE '%' + @F + '%'
             OR ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo) LIKE '%' + @F + '%')
     ORDER BY NombreCompleto;
END
GO
PRINT 'Sp_RTA_PerfilPersonalLista creado.';
GO

PRINT '== Perfil: listado de personal - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   1. Un filtro corto no devuelve nada:
        EXEC dbo.Sp_RTA_PerfilPersonalLista @Filtro = 'a';
      Esperado: 0 filas.

   2. Un filtro normal devuelve personal activo:
        EXEC dbo.Sp_RTA_PerfilPersonalLista @Filtro = 'ar';
      Esperado: filas ordenadas por nombre, ninguna con Cod_Usuario repetido.

   3. Los codigos repetidos quedan fuera. Esta consulta los nombra:
        SELECT Cod_Usuario, COUNT(*) AS Veces
          FROM dbo.R_Usuarios
         WHERE ISNULL(EstadoUsuario,0) = 0
         GROUP BY Cod_Usuario
        HAVING COUNT(*) > 1;
      Ninguno de esos codigos puede aparecer en el punto 2. Son los que hay que
      corregir a mano en R_Usuarios; Talento Humano tiene que saberlo, o va a
      buscar a esas personas y no las va a encontrar.
   ============================================================================ */
```

- [ ] **Step 2: Revisar el SQL sabiendo que nadie lo va a ejecutar antes de producción**

Comprobar, leyendo: que el `CREATE PROCEDURE` es la primera sentencia de su lote; que los `GO` están donde corresponde; que el bloque `VERIFICACION` es un comentario bien cerrado; que las cinco columnas del `SELECT` se llaman exactamente `CodUsuario`, `NombreCompleto`, `Cargo`, `Area`, `Ciudad`; y que el archivo es ASCII puro, sin tildes en comentarios ni en `PRINT`.

Escribir en el reporte qué se comprobó en cada punto.

- [ ] **Step 3: Commit**

```bash
git add docs/sql/2026-09-17-perfil-personal-lista.sql
git commit -m "feat(perfil): el listado de personal para Talento Humano

Es Sp_RTA_PerfilEquipoLista sin el filtro por Cod_Jefe_Inm. Conserva la
regla de no listar a quien tiene el Cod_Usuario repetido: esos perfiles
no se pueden abrir, y listarlos seria ofrecer un boton que nunca anda.

Quien puede llamarlo lo decide el handler, no el procedimiento.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 3: La acción `ListaPersonal`, sólo para 14 y 18

**Files:**
- Modify: `CapaDato/DaoPerfil.cs` (junto a `ListaEquipo`, alrededor de la línea 599)
- Modify: `CapaNegocio/NegPerfil.cs` (junto a `ListaEquipo`)
- Modify: `ReporteTareas/clases/PerfilIdentidad.cs`
- Modify: `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` (despacho y método nuevo)

**Interfaces:**
- Consumes: `NegPerfilCampos.ValidarFiltroPersonal(string)` (Task 1); `Sp_RTA_PerfilPersonalLista` (Task 2); `NegPerfilAcceso.EsRRHH(string idPerfilSesion)` → `bool` (entrega 1).
- Produces:
  - `CapaDato.DaoPerfil.ListaPersonal(string filtro)` → `List<EntPerfilEquipoItem>`
  - `CapaNegocio.NegPerfil.ListaPersonal(string filtro)` → `List<EntPerfilEquipoItem>`
  - `ReporteTareas.clases.PerfilIdentidad.EsRRHH(HttpContext context)` → `bool`
  - Acción JSON `ListaPersonal` con `parameters: { filtro: "<texto>" }`. La consume `perfilesPersonal.js` en la Task 7.

- [ ] **Step 1: `DaoPerfil.ListaPersonal`**

Agregar a `CapaDato/DaoPerfil.cs`, inmediatamente después de `ListaEquipo`:

```csharp
        /// <summary>
        /// Todo el personal activo que calce con el filtro.
        ///
        /// Devuelve EntPerfilEquipoItem y no una entidad propia porque las cinco
        /// columnas son las mismas que las de la lista de equipo: codigo, nombre,
        /// cargo, area y ciudad. Una entidad nueva identica seria dos sitios donde
        /// agregar una columna en vez de uno.
        ///
        /// No comprueba perfiles: eso lo hace AdministrarPerfil.ashx.cs antes de
        /// llegar aca. Esta capa no conoce la sesion.
        ///
        /// Sin paginacion, igual que ListaEquipo: el procedimiento exige dos
        /// caracteres de filtro, asi que nunca devuelve la plantilla entera.
        /// </summary>
        public static List<EntPerfilEquipoItem> ListaPersonal(string filtro)
        {
            List<EntPerfilEquipoItem> lista = new List<EntPerfilEquipoItem>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilPersonalLista", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntPerfilEquipoItem
                        {
                            CodUsuario     = Texto(dr, "CodUsuario"),
                            NombreCompleto = Texto(dr, "NombreCompleto"),
                            Cargo          = Texto(dr, "Cargo"),
                            Area           = Texto(dr, "Area"),
                            Ciudad         = Texto(dr, "Ciudad")
                        });
                    }
                }
            }

            return lista;
        }
```

> **Comparar con `ListaEquipo` (línea 624) antes de dar el paso por terminado:** este
> método es el mismo salvo el procedimiento y que no lleva `@Cod_Jefe`. Si `ListaEquipo`
> lee alguna columna que aquí falte, o al revés, una de las dos listas va a salir con
> celdas vacías sin dar ningún error.

- [ ] **Step 2: `NegPerfil.ListaPersonal`**

Agregar a `CapaNegocio/NegPerfil.cs`, junto a `ListaEquipo`:

```csharp
        /// <summary>
        /// El personal activo que calce con el filtro, para la pantalla de
        /// Talento Humano.
        ///
        /// La validacion del filtro va aqui y no solo en el navegador: el handler
        /// es alcanzable por HTTP directo. Con el filtro invalido devuelve lista
        /// vacia en vez de lanzar, para que la pantalla no tenga que distinguir
        /// "no valido" de "sin resultados" -el mensaje se lo da el handler-.
        /// </summary>
        public static List<EntPerfilEquipoItem> ListaPersonal(string filtro)
        {
            if (NegPerfilCampos.ValidarFiltroPersonal(filtro) != "")
            {
                return new List<EntPerfilEquipoItem>();
            }

            return DaoPerfil.ListaPersonal(filtro);
        }
```

- [ ] **Step 3: `PerfilIdentidad.EsRRHH`**

Agregar a `ReporteTareas/clases/PerfilIdentidad.cs`, junto a los otros miembros:

```csharp
        /// <summary>
        /// Si la sesion es de Talento Humano o Super Admin.
        ///
        /// Delega en NegPerfilAcceso, igual que Objetivo: la lista de perfiles
        /// vive en un solo sitio y esta clase sigue sin decidir nada. Hace falta
        /// aparte porque hay acciones -listar a todo el personal- que no son
        /// "sobre el perfil de alguien" y por lo tanto no pasan por Objetivo.
        /// </summary>
        internal static bool EsRRHH(HttpContext context)
        {
            string idPerfil = "";
            if (context.Session != null && context.Session["Id_Perfil"] != null)
            {
                idPerfil = context.Session["Id_Perfil"].ToString();
            }

            return NegPerfilAcceso.EsRRHH(idPerfil);
        }
```

- [ ] **Step 4: La acción en el handler**

En `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`, agregar al despacho, junto a los bloques `if (Action == ...)` existentes:

```csharp
                if (Action == "ListaPersonal")
                {
                    existAction = true;
                    responseAction.Append(ListaPersonal(context, parametros[0]["parameters"]));
                }
```

Y el método, junto a `ListaEquipo`:

```csharp
        /// <summary>
        /// Todo el personal activo que calce con el filtro, para la pantalla de
        /// Talento Humano.
        ///
        /// Es la UNICA accion del modulo que no habla del perfil de nadie en
        /// particular, asi que no pasa por PerfilIdentidad.Objetivo: aqui la
        /// pregunta no es "de quien es este perfil" sino "puede esta sesion ver
        /// la nomina completa". Por eso comprueba el perfil directamente.
        ///
        /// Sin esta comprobacion, cualquiera con sesion iniciada podria pedir el
        /// nombre, el cargo y el area de todo el personal con una peticion
        /// directa a este .ashx, sin pasar nunca por el menu.
        /// </summary>
        private string ListaPersonal(HttpContext context, dynamic campos)
        {
            try
            {
                if (!PerfilIdentidad.EsRRHH(context))
                {
                    return responseMessage("0", "No tiene permiso para ver el listado de personal.", "danger");
                }

                string filtro = Texto(campos, "filtro", "");

                string error = NegPerfilCampos.ValidarFiltroPersonal(filtro);
                if (error != "")
                {
                    return responseMessage("0", error, "warning");
                }

                return ToJson(NegPerfil.ListaPersonal(filtro));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al buscar personal. " + ex.Message, "danger");
            }
        }
```

- [ ] **Step 5: Compilar y correr las pruebas**

```
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: compila sin errores; **208 de 208**, el mismo total que al final de la Task 1.

- [ ] **Step 6: Commit**

```bash
git add CapaDato/DaoPerfil.cs CapaNegocio/NegPerfil.cs \
        ReporteTareas/clases/PerfilIdentidad.cs \
        ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
git commit -m "feat(perfil): la accion que lista al personal, solo para 14 y 18

Es la unica accion del modulo que no habla del perfil de nadie en
particular, asi que no pasa por PerfilIdentidad.Objetivo: la pregunta no
es de quien es este perfil sino si esta sesion puede ver la nomina.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 4: `PerfilFichas.ascx` — el marcado, una sola vez

El primer control de usuario del repositorio. El movimiento es **sólo marcado**: se corta y se pega, no se reescribe nada.

**Files:**
- Create: `ReporteTareas/Controles/PerfilFichas.ascx`
- Modify: `ReporteTareas/Formulario/MiPerfil.aspx` (queda en ~12 líneas)
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: nada.
- Produces: el control `~/Controles/PerfilFichas.ascx`, con `ClassName="PerfilFichas"`. Lo incluyen `MiPerfil.aspx` (esta tarea) y `PerfilesPersonal.aspx` (Task 6). Dentro viven todos los `id` que `miPerfil.js` ya usa (`perfilFoto`, `dpNombre`, `inDireccion`, `cuerpoEmergencia`, `liTabEquipo`, …); **ninguno cambia de nombre**, porque el JS los busca por `id`.

- [ ] **Step 1: Crear el control con el marcado de `MiPerfil.aspx`**

Crear `ReporteTareas/Controles/PerfilFichas.ascx`, **con BOM**, con esta primera línea:

```
<%@ Control Language="C#" AutoEventWireup="true" ClassName="PerfilFichas" %>
```

y debajo, **todo el contenido que hoy está entre `<asp:Content ID="Content2" ...>` y su `</asp:Content>` en `MiPerfil.aspx`** — es decir desde `<div id="page-wrapper" style="padding: 0px">` hasta su `</div>` de cierre, incluyendo el modal informativo y el `<input type="file" id="inDocumento" ... />`. Copiar literalmente, sin reordenar ni reindentar.

Tres cambios, y sólo tres, sobre lo copiado:

1. **El enlace del CV** (hoy `MiPerfil.aspx:57`) gana un `id`, porque la pantalla nueva tiene que reescribirle el destino y el texto:

```html
                        <a id="lnkHojaVida" href="DescargarPerfil.ashx?cv=1" target="_blank"
                           class="btn btn-primary btn-block btn-sm">
                            <i class="fa fa-file-pdf-o"></i> <span id="txtHojaVida">Descargar mi hoja de vida</span>
                        </a>
```

2. El comentario de cabecera del control explica qué es y quién lo usa:

```html
<%--
    Las fichas del perfil: datos personales, contacto, emergencia, formacion,
    experiencia, cargas familiares y equipo.

    Vive aca y no dentro de una pagina porque lo usan DOS pantallas: MiPerfil.aspx
    -el perfil propio- y PerfilesPersonal.aspx -el de otra persona, para Talento
    Humano-. Duplicar 498 lineas de marcado era garantizar que al primer arreglo
    las dos se separaran sin que nadie se enterara.

    Los id de aqui adentro son los que busca miPerfil.js. No renombrar ninguno.

    La pestana Equipo esta en el control pero PerfilesPersonal.aspx no la dibuja:
    ListaEquipo toma al jefe de la SESION, asi que alli mostraria el equipo de
    quien mira con el nombre de otro en la cabecera.
--%>
```

3. Nada más. Ni un `id`, ni una clase, ni un texto que no sea el del punto 1.

- [ ] **Step 2: Dejar `MiPerfil.aspx` en lo mínimo**

`ReporteTareas/Formulario/MiPerfil.aspx` queda así, **con BOM**, completo:

```
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="MiPerfil.aspx.cs" Inherits="ReporteTareas.Formulario.MiPerfil" ResponseEncoding="utf-8" %>
<%@ Register Src="~/Controles/PerfilFichas.ascx" TagPrefix="rta" TagName="PerfilFichas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/miPerfil.js?v=8" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <rta:PerfilFichas runat="server" ID="fichas" />
</asp:Content>
```

**El `?v=` sube de 7 a 8.** La Task 5 cambia `miPerfil.js`; sin subirlo, los navegadores que ya tienen la versión 7 en caché se quedan con ella y la pantalla nueva no funciona para ellos — sin ningún error.

- [ ] **Step 3: Registrar los archivos en el `.csproj`**

En `ReporteTareas/ReporteTareas.csproj`, junto a las otras entradas de `Content` (por ejemplo la de la línea 1118, `Formulario\MiPerfil.aspx`):

```xml
    <Content Include="Controles\PerfilFichas.ascx" />
```

- [ ] **Step 4: Compilar y comprobar que «Mi perfil» sigue igual**

```
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: compila sin errores.

Comprobar además, sin ejecutar nada: que el marcado del `.ascx` más las 11 líneas de `MiPerfil.aspx` contienen **exactamente** lo que había antes. La forma barata de verlo:

```bash
git show HEAD:ReporteTareas/Formulario/MiPerfil.aspx | grep -c 'id="'
grep -c 'id="' ReporteTareas/Controles/PerfilFichas.ascx
```

El segundo tiene que ser **el primero más dos**. Verificado: el `<a>` del CV **no tenía
`id`**, así que el Step 1 agrega dos (`lnkHojaVida` en el enlace y `txtHojaVida` en el
`<span>` nuevo). Cualquier otro número significa que se perdió marcado al cortar, o que
se agregó algo que no estaba pedido.

- [ ] **Step 5: Commit**

```bash
git add ReporteTareas/Controles/PerfilFichas.ascx \
        ReporteTareas/Formulario/MiPerfil.aspx ReporteTareas/ReporteTareas.csproj
git commit -m "refactor(perfil): las fichas se mudan a un control de usuario

Primer .ascx del repositorio. Lo van a incluir las dos pantallas: el
perfil propio y el de otra persona. La alternativa era duplicar 498
lineas de marcado y que se separaran al primer arreglo.

Solo se movio marcado. El unico cambio es un id en el enlace del CV,
que la pantalla nueva necesita reescribir.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 5: `miPerfil.js` aprende `_codObjetivo`

**Files:**
- Modify: `ReporteTareas/js/miPerfil.js`

**Interfaces:**
- Consumes: el `id` `lnkHojaVida` y `txtHojaVida` del control (Task 4).
- Produces: la variable global `_codObjetivo` (cadena vacía por omisión) y la función `FijarPerfilObjetivo(codUsuario, nombre)`. Las usa `perfilesPersonal.js` en la Task 7.

> **Son cuatro caminos, no tres.** El diseño decía tres; al mirar el código apareció un cuarto: el enlace del CV es marcado estático y el JS no lo tocaba.

- [ ] **Step 1: La variable y la función que la fija**

Al principio de `ReporteTareas/js/miPerfil.js`, junto a `var _perfil = null;`:

```javascript
/* De quien es el perfil que esta pantalla esta mostrando.

   Vacia en "Mi perfil": entonces no se manda nada y el servidor usa el de la
   sesion, exactamente como antes de que esto existiera. La fija el buscador de
   PerfilesPersonal.aspx al elegir a una persona.

   Que este valor viaje NO significa que el servidor lo acepte: la regla que
   decide eso es NegPerfilAcceso, y rechaza a quien no sea perfil 14 o 18. */
var _codObjetivo = "";

/* Los cuatro caminos por los que el codigo del perfil sale al servidor.
   El primero cubre las 16 acciones JSON de una sola vez; los otros tres NO
   pasan por PostPerfil y por eso hay que acordarse de ellos uno por uno:
     1. PostPerfil          -> lo agrega a parameters
     2. PedirArchivo        -> lo agrega al FormData (multipart)
     3. CeldaDocumentos     -> lo agrega a la URL de descarga del documento
     4. el enlace del CV    -> se le reescribe el href, porque es marcado
                               estatico y ningun codigo lo tocaba antes
   Si aparece un quinto, va en esta lista. */
function FijarPerfilObjetivo(codUsuario, nombre) {
    _codObjetivo = codUsuario || "";

    var enlace = $("#lnkHojaVida");
    if (enlace.length) {
        enlace.attr("href", "DescargarPerfil.ashx?cv=1" +
                            (_codObjetivo === "" ? "" : "&u=" + encodeURIComponent(_codObjetivo)));
        $("#txtHojaVida").text(_codObjetivo === ""
            ? "Descargar mi hoja de vida"
            : "Descargar la hoja de vida de " + (nombre || "esta persona"));
    }

    CargarPerfil();
}
```

- [ ] **Step 2: Camino 1 — `PostPerfil`**

En `PostPerfil` (hoy `miPerfil.js:17`), antes de serializar:

```javascript
function PostPerfil(action, parameters, onSuccess) {
    /* El codigo del perfil viaja en TODAS las llamadas, incluidas las del
       propio perfil, donde va vacio. Un solo camino es mas facil de revisar que
       una excepcion por accion. */
    var p = parameters || {};
    if (_codObjetivo !== "") { p.codUsuario = _codObjetivo; }

    var datos = JSON.stringify([{ "action": action, "parameters": p }]);
```

y el resto del cuerpo sin cambios.

- [ ] **Step 3: Camino 2 — el `FormData` de la subida**

En `PedirArchivo` (hoy `miPerfil.js:614`), junto a los otros `append`:

```javascript
    var datos = new FormData();
    datos.append("origen", origen);
    datos.append("idOrigen", idOrigen);
    datos.append("archivo", archivo);
    /* Esta llamada no pasa por PostPerfil -es multipart-, asi que el codigo del
       perfil se agrega a mano. El handler lo lee de Request.Form. */
    if (_codObjetivo !== "") { datos.append("codUsuario", _codObjetivo); }
```

- [ ] **Step 4: Camino 3 — el enlace de descarga de cada documento**

En `CeldaDocumentos` (hoy `miPerfil.js:568`):

```javascript
            .attr("href", "DescargarPerfil.ashx?doc=" + d.IdDocumento +
                          (_codObjetivo === "" ? "" : "&u=" + encodeURIComponent(_codObjetivo)))
```

- [ ] **Step 5: La pestaña Equipo no se dibuja si estamos mirando a otro**

En `MostrarPestanaEquipo` (hoy `miPerfil.js:651`):

```javascript
function MostrarPestanaEquipo(esJefe) {
    /* Nunca al mirar el perfil de otra persona. ListaEquipo y PerfilEquipo
       toman al jefe de la SESION, asi que aqui se veria el equipo de quien mira
       con el nombre de otro en la cabecera. No es cosmetica: es el dato de
       otra persona bajo una etiqueta equivocada. */
    if (_codObjetivo !== "") { return; }
    if (!esJefe) { return; }

    $("#liTabEquipo").show();
    BuscarEquipo();
}
```

- [ ] **Step 6: Compilar y comprobar que «Mi perfil» sigue igual**

```
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Y comprobar leyendo, porque no hay prueba automatizada de JavaScript en este proyecto: que con `_codObjetivo === ""` los cuatro caminos producen **exactamente** lo que producían antes — `parameters` sin `codUsuario`, `FormData` sin `codUsuario`, la URL de descarga sin `&u=`, y el enlace del CV con `?cv=1` a secas. Decirlo en el reporte, camino por camino.

- [ ] **Step 7: Commit**

```bash
git add ReporteTareas/js/miPerfil.js
git commit -m "feat(perfil): el js aprende de quien es el perfil que muestra

Una variable, _codObjetivo, vacia en Mi perfil y fijada por el buscador de
la pantalla de Talento Humano. Viaja por los cuatro caminos que salen al
servidor; el cuarto -el enlace del CV- era marcado estatico y el js no lo
tocaba.

La pestana Equipo deja de dibujarse al mirar a otra persona: ListaEquipo
toma al jefe de la sesion y mostraria el equipo de quien mira.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 6: La pantalla nueva

**Files:**
- Create: `ReporteTareas/Formulario/PerfilesPersonal.aspx` (con BOM)
- Create: `ReporteTareas/Formulario/PerfilesPersonal.aspx.cs`
- Create: `ReporteTareas/Formulario/PerfilesPersonal.aspx.designer.cs`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: el control `~/Controles/PerfilFichas.ascx` (Task 4); `NegPerfilAcceso.PerfilesRRHH` (entrega 1).
- Produces: la página `~/Formulario/PerfilesPersonal.aspx`, que la Task 8 registra en el menú. Sus `id` propios: `txtBuscarPersonal`, `btnBuscarPersonal`, `cuerpoPersonal`, `panelFichas`, `personaElegida`.

- [ ] **Step 1: El code-behind, con la barrera**

Crear `ReporteTareas/Formulario/PerfilesPersonal.aspx.cs`:

```csharp
using CapaNegocio;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Perfiles del personal. Talento Humano busca a cualquier empleado y edita
    /// su perfil con las mismas fichas que cada quien usa para el suyo.
    ///
    /// No pasa ningun identificador a la pagina: el codigo de la persona elegida
    /// lo fija el buscador en el cliente y lo valida el handler contra la sesion.
    ///
    /// El handler ya rechaza cualquier accion de quien no sea perfil 14 o 18 -no
    /// hay fuga de datos sin esta comprobacion aqui-, pero sin ella un usuario
    /// que teclee la URL a mano ve la pantalla cargarse entera y recien le salta
    /// el mensaje al buscar. Un Response.Redirect es mas honesto.
    ///
    /// La lista de perfiles NO se escribe aqui: sale de NegPerfilAcceso, que es
    /// donde vive y donde esta probada. Tenerla dos veces no es un riesgo de
    /// seguridad -la barrera de verdad es la del handler- sino de mantenimiento
    /// callado.
    /// </summary>
    public partial class PerfilesPersonal : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            int idPerfil;
            bool tienePermiso = int.TryParse(Convert.ToString(Session["Id_Perfil"]), out idPerfil)
                                 && Array.IndexOf(NegPerfilAcceso.PerfilesRRHH, idPerfil) >= 0;

            if (!tienePermiso)
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
```

- [ ] **Step 2: El `.designer.cs`**

Crear `ReporteTareas/Formulario/PerfilesPersonal.aspx.designer.cs`. La página no declara controles de servidor salvo el control de usuario, así que:

```csharp
//------------------------------------------------------------------------------
// <auto-generated>
//     Este codigo lo genera una herramienta.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReporteTareas.Formulario
{
    public partial class PerfilesPersonal
    {
        /// <summary>
        /// Control fichas.
        /// </summary>
        protected global::System.Web.UI.UserControl fichas;
    }
}
```

- [ ] **Step 3: La página**

Crear `ReporteTareas/Formulario/PerfilesPersonal.aspx`, **con BOM**:

```
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="PerfilesPersonal.aspx.cs" Inherits="ReporteTareas.Formulario.PerfilesPersonal" ResponseEncoding="utf-8" %>
<%@ Register Src="~/Controles/PerfilFichas.ascx" TagPrefix="rta" TagName="PerfilFichas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/miPerfil.js?v=8" type="text/javascript"></script>
    <script src="../js/perfilesPersonal.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="row">
        <div class="col-lg-12" style="padding: 20px">
            <div class="card card-primary">
                <div class="card-header" style="text-align: center">
                    <h3>Perfiles del personal</h3>
                </div>
                <div class="card-body" style="padding: 15px">

                    <div class="row">
                        <div class="col-md-6">
                            <div class="input-group">
                                <input type="text" class="form-control" id="txtBuscarPersonal"
                                       maxlength="100" placeholder="Nombre, código o cargo" />
                                <span class="input-group-btn">
                                    <button type="button" id="btnBuscarPersonal" class="btn btn-primary">
                                        <i class="fa fa-search"></i> Buscar
                                    </button>
                                </span>
                            </div>
                            <p class="text-muted" style="margin-top: 6px; font-size: 12px">
                                Escriba al menos 2 caracteres. No aparece el personal inactivo,
                                ni quienes tienen el código de usuario repetido.
                            </p>
                        </div>
                    </div>

                    <div class="table-responsive" style="margin-top: 10px">
                        <table class="table table-hover table-condensed">
                            <thead>
                                <tr>
                                    <th>Nombre</th>
                                    <th>Código</th>
                                    <th>Cargo</th>
                                    <th>Área</th>
                                    <th style="width: 60px"></th>
                                </tr>
                            </thead>
                            <tbody id="cuerpoPersonal"></tbody>
                        </table>
                    </div>

                </div>
            </div>
        </div>
    </div>

    <div id="panelFichas" style="display: none">
        <div class="alert alert-info" style="margin: 0 20px">
            Está viendo el perfil de <b id="personaElegida">–</b>.
            Los cambios que guarde quedan registrados a su nombre.
        </div>
        <rta:PerfilFichas runat="server" ID="fichas" />
    </div>
</asp:Content>
```

- [ ] **Step 4: Registrar los tres archivos en el `.csproj`**

Junto a las entradas de `MiPerfil.aspx` (líneas 1118 y 1771-1777):

```xml
    <Content Include="Formulario\PerfilesPersonal.aspx" />
```

```xml
    <Compile Include="Formulario\PerfilesPersonal.aspx.cs">
      <DependentUpon>PerfilesPersonal.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\PerfilesPersonal.aspx.designer.cs">
      <DependentUpon>PerfilesPersonal.aspx</DependentUpon>
    </Compile>
```

- [ ] **Step 5: Compilar**

```
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: compila sin errores. Comprobar además que los tres archivos nuevos quedaron listados en el `.csproj` — un `.aspx` sin `<Content Include>` no viaja en el paquete de despliegue, y eso no se nota hasta que alguien abre la URL en producción y recibe un 404.

- [ ] **Step 6: Comprobar el BOM**

```bash
head -c 3 ReporteTareas/Formulario/PerfilesPersonal.aspx | xxd | head -1
```

Esperado: empieza con `efbb bf`. Si no, el archivo no tiene BOM y en producción va a salir con caracteres raros — `Web.config` declara `windows-1252` y no trae `fileEncoding`.

- [ ] **Step 7: Commit**

```bash
git add ReporteTareas/Formulario/PerfilesPersonal.aspx \
        ReporteTareas/Formulario/PerfilesPersonal.aspx.cs \
        ReporteTareas/Formulario/PerfilesPersonal.aspx.designer.cs \
        ReporteTareas/ReporteTareas.csproj
git commit -m "feat(perfil): la pantalla de perfiles del personal

Buscador mas el control de fichas. La lista de perfiles autorizados sale
de NegPerfilAcceso y no se reescribe aqui: tenerla dos veces no es un
riesgo de seguridad sino de mantenimiento callado.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 7: El JavaScript del buscador

**Files:**
- Create: `ReporteTareas/js/perfilesPersonal.js`

**Interfaces:**
- Consumes: `FijarPerfilObjetivo(codUsuario, nombre)` y `PostPerfil(action, parameters, onSuccess)` de `miPerfil.js` (Task 5); la acción `ListaPersonal` (Task 3); los `id` de la página (Task 6).
- Produces: nada que otra tarea consuma.

- [ ] **Step 1: Escribir el archivo**

Crear `ReporteTareas/js/perfilesPersonal.js`:

```javascript
/* ============================================================================
   Pantalla: Perfiles del personal
   Handler : AdministrarPerfil.ashx

   Este archivo hace dos cosas y nada mas: pinta la lista de personal y fija de
   quien es el perfil que se esta mirando. Todo lo demas -cargar las fichas,
   guardar, subir, descargar- lo hace miPerfil.js, que es el mismo archivo que
   usa "Mi perfil". Por eso los dos se cargan en esta pagina, en ese orden.

   La barrera no esta aca. Que esta pantalla solo la vean los perfiles 14 y 18
   lo deciden el menu y PerfilesPersonal.aspx.cs; que solo ellos puedan pedir
   datos ajenos lo decide NegPerfilAcceso, en el servidor.
   ============================================================================ */

$(document).ready(function () {
    /* miPerfil.js llama a CargarPerfil() al cargar la pagina, lo que aqui
       mostraria el perfil de quien esta mirando antes de que elija a nadie. Las
       fichas arrancan ocultas y no se muestran hasta que hay alguien elegido. */
    $("#panelFichas").hide();

    $("#btnBuscarPersonal").on("click", BuscarPersonal);

    $("#txtBuscarPersonal").on("keypress", function (e) {
        if (e.which === 13) { e.preventDefault(); BuscarPersonal(); }
    });
});

function BuscarPersonal() {
    var filtro = $("#txtBuscarPersonal").val();

    /* La misma regla que NegPerfilCampos.ValidarFiltroPersonal. Esto es
       comodidad, no seguridad: el servidor vuelve a comprobarlo. */
    if ($.trim(filtro).length < 2) {
        MostrarMensaje("Escriba al menos 2 caracteres para buscar.", "warning");
        return;
    }

    PostPerfil("ListaPersonal", { filtro: filtro }, function (respuesta) {
        // Un objeto con "estado" es un EntRespuesta, es decir, un error.
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        PintarListaPersonal(respuesta);
    });
}

function PintarListaPersonal(lista) {
    var cuerpo = $("#cuerpoPersonal");
    cuerpo.empty();

    if (lista == null || lista.length === 0) {
        cuerpo.append('<tr><td colspan="5" class="text-muted">Sin resultados.</td></tr>');
        return;
    }

    for (var i = 0; i < lista.length; i++) {
        var p = lista[i];

        var fila = $("<tr>");
        fila.append($("<td>").text(p.NombreCompleto || "–"));
        fila.append($("<td>").text(p.CodUsuario || "–"));
        fila.append($("<td>").text(p.Cargo || "–"));
        fila.append($("<td>").text(p.Area || "–"));

        /* El boton lleva los datos colgados con .data() y no en el onclick: asi
           un nombre con comillas o con un apostrofo no rompe el marcado. */
        var boton = $('<button type="button" class="btn btn-default btn-xs" title="Abrir perfil">')
            .append('<i class="fa fa-eye"></i>')
            .data("cod", p.CodUsuario)
            .data("nombre", p.NombreCompleto)
            .on("click", function () {
                AbrirPerfilDe($(this).data("cod"), $(this).data("nombre"));
            });

        fila.append($("<td>").append(boton));
        cuerpo.append(fila);
    }
}

function AbrirPerfilDe(codUsuario, nombre) {
    $("#personaElegida").text(nombre || codUsuario);
    $("#panelFichas").show();

    /* Fija el objetivo y recarga las fichas. A partir de aqui, cada guardado de
       miPerfil.js viaja con este codigo y el servidor decide si lo acepta. */
    FijarPerfilObjetivo(codUsuario, nombre);

    $("html, body").animate({ scrollTop: $("#panelFichas").offset().top - 20 }, 300);
}
```

- [ ] **Step 2: Comprobar el orden de carga**

`PerfilesPersonal.aspx` carga `miPerfil.js` **antes** que `perfilesPersonal.js`. Verificarlo: `FijarPerfilObjetivo`, `PostPerfil` y `MostrarMensaje` viven en el primero, y el segundo los usa. Si el orden se invierte, la pantalla falla con `is not defined` al primer clic.

- [ ] **Step 3: Compilar**

```
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

El `.js` no necesita entrada en el `.csproj` para compilar, **pero sí para viajar en el paquete de despliegue**. Comprobar cómo están listados los otros `.js` de `ReporteTareas/js/` y agregar el nuevo del mismo modo.

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/js/perfilesPersonal.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(perfil): el buscador de la pantalla de personal

Pinta la lista y fija de quien es el perfil. Todo lo demas lo hace
miPerfil.js, que es el mismo archivo que usa Mi perfil.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 8: El script de menú

**Files:**
- Create: `docs/sql/2026-09-17-perfil-personal-menu.sql`

**Interfaces:**
- Consumes: la página `PerfilesPersonal.aspx` (Task 6).
- Produces: las filas de `MenuDos` y `PerfilMenu` que hacen visible la pantalla a los perfiles 14 y 18.

> **Dos trampas de `PerfilMenu`, las dos documentadas en `docs/sql/2026-09-16-horas-extras-fase2-menu.sql:22-26`:**
> 1. La semántica de `Estado` está **invertida**: `'0'` **muestra** la opción y `'1'` la oculta.
> 2. Una hoja sólo se ve si su **grupo padre** también tiene `Estado = '0'` para ese perfil. Hay que dar permiso en las dos filas.

- [ ] **Step 1: Escribir el script**

Crear `docs/sql/2026-09-17-perfil-personal-menu.sql`, tomando como modelo la estructura de `docs/sql/2026-09-16-horas-extras-fase2-menu.sql` (clonar una fila modelo por metadata para no inventar las 18 columnas de `MenuDos`, corregir después `Titulo`, `Href`, `Id_MenuPadre`, `Descripcion` y `Orden_Opcion`).

**La diferencia con aquel script: aquí no se crea un grupo nuevo.** La pantalla cuelga del **mismo grupo donde ya vive `RRHHEmpleados.aspx`**, que es la pantalla con la que Talento Humano ya administra datos de empleados — es su vecina natural. Ese grupo se resuelve en el propio script y no se escribe a mano:

```sql
DECLARE @IdGrupo INT;

SELECT TOP 1 @IdGrupo = Id_MenuPadre
  FROM dbo.MenuDos
 WHERE Href LIKE '%RRHHEmpleados.aspx%'
   AND ISNULL(Id_MenuPadre, 0) <> 0;

IF @IdGrupo IS NULL
BEGIN
    RAISERROR('No se encontro el grupo de menu de RRHHEmpleados.aspx. Revisar a mano de que grupo debe colgar PerfilesPersonal.aspx. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO
```

> **`SET NOEXEC ON`, no `RETURN`.** `RETURN` fuera de un procedimiento sale del **lote**, no del script: después del `GO` la ejecución seguiría y el `INSERT` se haría igual, con `Id_MenuPadre` nulo. Es el defecto que costó dos rondas de arreglo en la entrega 1. Cerrar el archivo con un `SET NOEXEC OFF;` incondicional.

Valores de la hoja:

```sql
DECLARE @TituloHoja      VARCHAR(128) = 'Perfiles del personal';
DECLARE @DescripcionHoja VARCHAR(512) = 'Consulta y correccion del perfil de cualquier colaborador';
DECLARE @HrefHoja        VARCHAR(512) = 'PerfilesPersonal.aspx';
```

Y los permisos, para el grupo **y** la hoja:

```sql
DECLARE @Perfiles TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Perfiles (IdPerfil) VALUES (14), (18);
```

Idempotente: no duplicar la hoja si ya existe una con ese `Href`, ni las filas de `PerfilMenu`.

Al final, el bloque `VERIFICACION (correr a mano despues del script)` con la consulta que muestra el grupo, la hoja y sus filas de `PerfilMenu` — la misma forma que la del script de Horas Extras (`:212-228`).

- [ ] **Step 2: Revisar el SQL sabiendo que nadie lo va a ejecutar antes de producción**

Comprobar leyendo: los `GO` en su sitio; que el `RAISERROR` **no** lleva llamadas a función como argumentos de sustitución —sólo literales o variables locales; fue un hallazgo real de la entrega 1—; que el `SET NOEXEC OFF;` final es incondicional y se alcanza siempre; que la semántica invertida de `Estado` está respetada (`'0'` para mostrar) en el grupo y en la hoja; y que el archivo es ASCII puro.

- [ ] **Step 3: Commit**

```bash
git add docs/sql/2026-09-17-perfil-personal-menu.sql
git commit -m "feat(perfil): la pantalla de personal entra al menu

Cuelga del mismo grupo que RRHHEmpleados.aspx, que es la pantalla con la
que Talento Humano ya administra datos de empleados. El grupo se resuelve
en el script y no se escribe a mano.

Estado = '0' MUESTRA la opcion, y hace falta en el grupo y en la hoja.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 9: Los textos que hablan de la persona equivocada

Quedaron diferidos de la entrega 1 con la nota «no se toca ahora: sin pantalla no hay quien lo vea». Ahora hay pantalla.

**Files:**
- Modify: `ReporteTareas/Formulario/DescargarPerfil.ashx.cs`
- Modify: `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` (el despacho, y las 17 lecturas de `parameters`)

**Interfaces:**
- Consumes: nada.
- Produces: nada.

- [ ] **Step 1: El mensaje de `EntregarCv`**

En `ReporteTareas/Formulario/DescargarPerfil.ashx.cs`, `EntregarCv` responde hoy:

```
"No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario."
```

Cuando quien pide el CV **es** Talento Humano y el perfil es de un tercero, ese texto le habla de sí mismo sobre el problema de otro. Cambiarlo por uno que sirva para los dos casos:

```csharp
                NoDisponible(context, "No se pudo armar la hoja de vida: ese código de usuario " +
                                      "está repetido entre usuarios activos y no se sabe de quién " +
                                      "son los datos. Hay que corregirlo en el maestro de usuarios.");
```

Y actualizar el comentario del método si afirma algo que dejó de ser cierto.

- [ ] **Step 2: La guarda del payload sin `parameters`**

El otro diferido de la entrega 1, que la revision final dejo explicitamente para esta.

En `AdministrarPerfil.ashx.cs`, el despacho lee `parametros[0]["parameters"]` en cada
accion. Si un POST no trae esa clave, eso lanza `KeyNotFoundException` **fuera de todo
`try`** y la peticion muere con un error de servidor en vez de con un JSON de error.
Ninguna pantalla lo dispara -`miPerfil.js` siempre manda `parameters`, aunque sea `{}`-,
pero el handler es alcanzable por HTTP directo.

Leer la clave **una sola vez**, de forma segura, justo despues de deserializar, y usar
esa variable en los 17 despachos:

```csharp
                var Action = parametros[0]["action"];

                /* Se lee UNA vez y con tolerancia a que no venga. Antes cada accion
                   hacia parametros[0]["parameters"] por su cuenta: un POST sin esa
                   clave -ninguna pantalla manda uno, pero este handler es alcanzable
                   por HTTP directo- lanzaba KeyNotFoundException fuera de todo try y
                   mataba la peticion con un error de servidor en vez de con un JSON
                   de error como el resto del modulo. */
                var diccionarioRaiz = parametros[0] as System.Collections.Generic.IDictionary<string, object>;
                object parametrosAccion = null;
                if (diccionarioRaiz != null) { diccionarioRaiz.TryGetValue("parameters", out parametrosAccion); }
```

y reemplazar las 17 apariciones de `parametros[0]["parameters"]` por `parametrosAccion`.

Los extractores ya toleran el nulo: `PerfilIdentidad.CodigoPedidoJson` y el ayudante
`Texto` empiezan los dos con `campos as IDictionary<string, object>` y devuelven el
valor por omision cuando da `null`. **Comprobarlo antes de dar el paso por terminado**,
en los dos: si alguno desreferenciara sin comprobar, este cambio cambiaria un error por
otro.

Comprobar ademas a mano que las 17 acciones siguen funcionando con un `parameters`
normal: basta con abrir «Mi perfil», que las ejercita casi todas.

- [ ] **Step 3: Compilar y correr las pruebas**

```
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: compila; **208 de 208**.

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/Formulario/DescargarPerfil.ashx.cs ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
git commit -m "fix(perfil): los dos diferidos de la entrega 1

Un POST sin la clave parameters lanzaba KeyNotFoundException fuera de todo
try y mataba la peticion con un error de servidor. Ahora se lee una vez y
con tolerancia a que no venga.

Y el mensaje del CV ya no habla de la persona equivocada:

Decia 'su perfil' y 'su codigo de usuario'. Cuando Talento Humano baja el
CV de otro, ese texto le hablaba de si mismo sobre el problema de un
tercero.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Task 10: `DESPLIEGUE.md`, paquete y verificación

**Files:**
- Modify: `DESPLIEGUE.md`
- Modify: `ReporteTareas/obj/Release/Package/PackageTmp/**` (regenerado, no editado)

- [ ] **Step 1: La sección de esta entrega en `DESPLIEGUE.md`**

Junto a la de la entrega 1, imitando su forma. Como mínimo:

1. `2026-09-17-perfil-personal-lista.sql`
2. `2026-09-17-perfil-personal-menu.sql`
3. Los binarios

Y la advertencia que importa: **esta entrega exige que la entrega 1 esté desplegada**. La pantalla nueva pide perfiles ajenos; sin la entrega 1 en la base y en los binarios, el handler los rechaza todos y la pantalla se ve pero no sirve.

- [ ] **Step 2: Regenerar el paquete de despliegue**

Publicar con el perfil `FolderProfile` en **Release**.

**Nunca `robocopy /MIR` ni ningún borrado recursivo:** `connections.config` y `appsettings.config` están en `.gitignore`, existen sólo en esa máquina, y sin ellos el sitio no arranca.

- [ ] **Step 3: Comprobar que el paquete no quedó viejo**

Esta entrega **sí** cambia archivos de cliente, así que el `?v=` vuelve a ser una comprobación útil — y además hay que comprobar los binarios y los archivos nuevos:

```bash
grep -o 'miPerfil.js?v=[0-9]*' ReporteTareas/Formulario/MiPerfil.aspx
grep -o 'miPerfil.js?v=[0-9]*' ReporteTareas/obj/Release/Package/PackageTmp/Formulario/MiPerfil.aspx
ls ReporteTareas/obj/Release/Package/PackageTmp/Formulario/PerfilesPersonal.aspx
ls ReporteTareas/obj/Release/Package/PackageTmp/Controles/PerfilFichas.ascx
ls ReporteTareas/obj/Release/Package/PackageTmp/js/perfilesPersonal.js
```

Los dos `?v=` tienen que dar **8**, y los tres archivos nuevos tienen que existir en el paquete. Si alguno falta, es que no quedó listado en el `.csproj`: en producción sería un 404 al abrir la pantalla, o una pantalla en blanco sin ningún error de servidor.

Comprobar además que los cuatro binarios de `PackageTmp/bin/` (`CapaNegocio.exe`, `CapaDato.exe`, `CapaEntidad.exe`, `ReporteTareas.dll`) coinciden en tamaño con los de `ReporteTareas/bin/`, y que `connections.config` y `appsettings.config` siguen existiendo.

- [ ] **Step 4: Commit**

```bash
git add DESPLIEGUE.md ReporteTareas/obj/Release/Package/PackageTmp \
        ReporteTareas/bin CapaDato/bin CapaNegocio/bin CapaEntidad/bin
git commit -m "build: paquete regenerado con la entrega 2 del perfil por Talento Humano

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>"
```

---

## Verificación manual después del despliegue

Se corre **después** de desplegar. La de la entrega 1 (al final de su plan) sigue valiendo: **volver a correrla entera primero**, porque esta entrega toca `miPerfil.js`, que es el archivo del que depende «Mi perfil».

> **Antes de empezar:** confirmar que el `ReporteTareas.dll` del IIS es el recién publicado, y que `PerfilesPersonal.aspx`, `PerfilFichas.ascx` y `perfilesPersonal.js` llegaron al servidor.

### A. «Mi perfil» sigue igual para el usuario común

Con un usuario **de perfil corriente**: cargar, guardar contacto, agregar y borrar un contacto de emergencia, subir y descargar un documento, descargar el CV, y —si es jefe— la pestaña **Equipo** sigue mostrando su equipo.

El botón del CV tiene que seguir diciendo **«Descargar mi hoja de vida»**.

### B. La pantalla nueva no existe para quien no debe

1. Con un usuario corriente, **la opción no aparece en el menú**.
2. Con ese mismo usuario, tecleando `PerfilesPersonal.aspx` en la barra de direcciones → **redirige a `Principal.aspx`**, no muestra la pantalla vacía.
3. Con ese mismo usuario, desde la consola en `MiPerfil.aspx`:

```javascript
$.ajax({ type:"POST", url:"AdministrarPerfil.ashx",
  data: JSON.stringify([{action:"ListaPersonal", parameters:{filtro:"ar"}}]),
  contentType:"application/json; charset=utf-8", dataType:"json",
  success:function(r){ console.log(r); } });
```

→ **rechazo**. Este es el caso que ni el menú ni la redirección protegen.

### C. La pantalla nueva funciona para Talento Humano

Con un usuario **de perfil 14**:

1. La opción aparece en el menú, colgando del mismo grupo que «Empleados».
2. Buscar con **un** carácter → avisa que hacen falta dos, y no lista nada.
3. Buscar con dos o más → lista personal activo, ordenado por nombre.
4. Abrir a una persona → se muestran sus fichas, con su nombre en el aviso de arriba.
5. **La pestaña «Equipo» NO aparece**, ni siquiera si esa persona es jefe. Es lo más fácil de que se cuele.
6. El botón del CV dice **«Descargar la hoja de vida de <nombre>»** y entrega el CV **de esa persona**.
7. Guardar un cambio en su contacto, y comprobar en la base:

```sql
SELECT Cod_Usuario, TelefonoPersonal, Usu_Modificacion, Fec_Modificacion
  FROM dbo.Perfil_ContactoPersonal
 WHERE Cod_Usuario = 'CODIGO_DE_LA_OTRA_PERSONA';
```

`Cod_Usuario` es el de **la otra persona** y `Usu_Modificacion` el del **usuario de perfil 14**.

8. Subir un documento al perfil de esa persona y volver a descargarlo desde la misma pantalla.

### D. Los cuatro códigos repetidos

Buscarlos por nombre en el buscador: **no aparecen**. Es correcto y es lo que hay que avisarle a Talento Humano — esas personas necesitan que alguien les corrija el código en `R_Usuarios` antes de poder gestionarlas desde aquí.
