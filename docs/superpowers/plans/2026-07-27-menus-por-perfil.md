# Menús por perfil (árbol padre-hijo) — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Pantalla nueva para asignar menús por perfil con un árbol padre-hijo, que fuerza la invariante "hijo activo ⇒ padre activo" en el guardado, más un arreglo de los datos ya inconsistentes — cerrando el síntoma "activo un submenú y no aparece".

**Architecture:** SPs nuevos sobre las tablas existentes `MenuDos`/`PerfilMenu`; un SP de guardado atómico que agrega los padres de los hijos activos; 3 capas C# (Entidad/Dato/Negocio); un handler `.ashx` y una pantalla `.aspx` con árbol; un script one-off de arreglo de datos; y el registro en el menú. NO se toca `Master.Master.cs` ni `PruebaMenu.aspx`. Mismo patrón que "Página de inicio por perfil".

**Tech Stack:** ASP.NET WebForms (C#, .NET Framework 4.6.1), 3 capas + web `ReporteTareas`, SQL Server (stored procedures), jQuery.

## Global Constraints

- **Sin framework de pruebas automatizadas.** Verificación manual: MSBuild EXIT 0, `sqlcmd`, HTTP (PowerShell), navegador.
- **`Estado = 0` = ACTIVO** en `PerfilMenu`. `Estado = 1` (o fila ausente) = inactivo.
- **Identidad:** `IdPerfil = R_Perfil.Id_Perfil = Session["Id_Perfil"] = PerfilMenu.IdPerfil`.
- **Nombres reales verificados:** `MenuDos(Id_Menu, Titulo, Href, Class_Icon, Id_MenuPadre)`, `PerfilMenu(IdPerfil, id_Menu, Estado)` (ojo: `id_Menu` con esa capitalización), `R_Perfil(Id_Perfil, Nombre, Estado_Logico_Registro)`.
- **Invariante:** hijo activo para un perfil ⇒ su padre también activo. Se fuerza en `Sp_RTA_GuardarMenuPerfil` y se repara en el script de datos.
- **Codificación:** `.js` en **UTF-8 con BOM**; handler `context.Response.ContentEncoding = Encoding.UTF8`; usar `EscaparAttr` (codifica comillas) para valores de atributos en el JS. Mensajes fijos de SP **sin tildes**.
- **Scripts SQL con `SET QUOTED_IDENTIFIER ON;`** al inicio (requerido por XML/MERGE y por el patrón de menú de esta BD).
- **Base de datos:** `-S "tcp:192.168.11.14,1433" -U sa -P "CAfKsUBnD0s" -d ReporTarea` (sólo responde por TCP desde esta máquina; conectividad intermitente — reintentar si da timeout).
- **MSBuild:** `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`, `ReporteTareas.sln`, `Debug`. Ejecutar **vía PowerShell** (`& $msb ...`) por el espacio en la ruta.
- **App local:** requiere **IIS Express de 32 bits**; el `SqlClient` de la app puede no alcanzar la BD desde esta máquina, así que HTTP/navegador se difieren al entorno del usuario cuando dependan de datos.
- **No modificar** `Master.Master.cs` ni `PruebaMenu.aspx`.

---

## File Structure

- **Crear** `docs/superpowers/plans/sql/2026-07-27-menus-por-perfil.sql` — 3 SPs.
- **Crear** `docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-fix-datos.sql` — arreglo one-off de datos.
- **Crear** `docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-menu.sql` — registro en menú.
- **Crear** `CapaEntidad/EntMenuPerfil.cs`, `CapaEntidad/EntPerfil.cs`.
- **Crear** `CapaDato/DaoMenuPerfil.cs`.
- **Crear** `CapaNegocio/NegMenuPerfil.cs`.
- **Crear** `ReporteTareas/Formulario/AdministrarMenuPerfil.ashx` (+ `.ashx.cs`).
- **Crear** `ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx` (+ `.aspx.cs`, `.aspx.designer.cs`).
- **Crear** `ReporteTareas/js/parametrizacionMenuPerfil.js`.
- **Modificar** `ReporteTareas/ReporteTareas.csproj` y los `.csproj` de las capas.

---

## Task 1: Base de datos — SPs + arreglo de datos

**Files:**
- Create: `docs/superpowers/plans/sql/2026-07-27-menus-por-perfil.sql`
- Create: `docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-fix-datos.sql`

**Interfaces:**
- Produces:
  - `Sp_RTA_ListarPerfiles()` → `Id_Perfil, Nombre`.
  - `Sp_RTA_ListarMenuPerfil(@IdPerfil)` → `Id_Menu, Id_MenuPadre, Titulo, Class_Icon, Activo`.
  - `Sp_RTA_GuardarMenuPerfil(@IdPerfil, @ActivosCsv)` → `Respuestas INT, Mensaje VARCHAR` (atómico; agrega padres).

- [ ] **Step 1: Crear el script de SPs**

Crear `docs/superpowers/plans/sql/2026-07-27-menus-por-perfil.sql`:

```sql
/* ============================================================================
   Menus por perfil — stored procedures
   Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- 1) Listar perfiles (combo) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarPerfiles','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPerfiles;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarPerfiles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id_Perfil, Nombre
    FROM dbo.R_Perfil
    WHERE ISNULL(Estado_Logico_Registro,1) = 1
    ORDER BY Nombre;
END
GO

/* ---------- 2) Listar menus con estado para el perfil ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarMenuPerfil','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarMenuPerfil;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarMenuPerfil
    @IdPerfil INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        m.Id_Menu,
        Id_MenuPadre = ISNULL(m.Id_MenuPadre,0),
        m.Titulo,
        Class_Icon   = ISNULL(m.Class_Icon,''),
        Activo       = CASE WHEN pm.id_Menu IS NOT NULL THEN 1 ELSE 0 END
    FROM dbo.MenuDos m
    LEFT JOIN dbo.PerfilMenu pm
        ON pm.id_Menu = m.Id_Menu AND pm.IdPerfil = @IdPerfil AND pm.Estado = 0
    ORDER BY ISNULL(m.Id_MenuPadre,0), m.Titulo;
END
GO

/* ---------- 3) Guardar (atomico; agrega los padres de los hijos activos) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_GuardarMenuPerfil','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarMenuPerfil;
GO
CREATE PROCEDURE dbo.Sp_RTA_GuardarMenuPerfil
    @IdPerfil   INT,
    @ActivosCsv VARCHAR(MAX) = ''
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    IF ISNULL(@IdPerfil,0) = 0 OR NOT EXISTS (SELECT 1 FROM dbo.R_Perfil WHERE Id_Perfil = @IdPerfil)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El perfil indicado no existe.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Activos TABLE (IdMenu INT PRIMARY KEY);

        /* parsear el CSV de enteros via XML-nodes (compatible SQL 2008+) */
        IF ISNULL(LTRIM(RTRIM(@ActivosCsv)),'') <> ''
        BEGIN
            DECLARE @xml XML = CAST('<i>' + REPLACE(@ActivosCsv, ',', '</i><i>') + '</i>' AS XML);
            INSERT INTO @Activos (IdMenu)
            SELECT DISTINCT T.c.value('.', 'INT')
            FROM @xml.nodes('/i') AS T(c)
            WHERE ISNULL(T.c.value('.', 'VARCHAR(20)'),'') <> '';
        END

        /* invariante: agregar el padre de todo hijo activo que no este ya en el set */
        INSERT INTO @Activos (IdMenu)
        SELECT DISTINCT m.Id_MenuPadre
        FROM @Activos a
        JOIN dbo.MenuDos m ON m.Id_Menu = a.IdMenu
        WHERE ISNULL(m.Id_MenuPadre,0) <> 0
          AND m.Id_MenuPadre NOT IN (SELECT IdMenu FROM @Activos);

        /* upsert: existentes -> Estado 0/1 segun set; faltantes activos -> insertar Estado 0 */
        MERGE dbo.PerfilMenu AS tgt
        USING (
            SELECT m.Id_Menu,
                   EsActivo = CASE WHEN a.IdMenu IS NOT NULL THEN 1 ELSE 0 END
            FROM dbo.MenuDos m
            LEFT JOIN @Activos a ON a.IdMenu = m.Id_Menu
        ) AS src
        ON tgt.IdPerfil = @IdPerfil AND tgt.id_Menu = src.Id_Menu
        WHEN MATCHED THEN
            UPDATE SET tgt.Estado = CASE WHEN src.EsActivo = 1 THEN 0 ELSE 1 END
        WHEN NOT MATCHED BY TARGET AND src.EsActivo = 1 THEN
            INSERT (IdPerfil, id_Menu, Estado) VALUES (@IdPerfil, src.Id_Menu, 0);

        SET @Respuestas = 1;
        SET @Mensaje = 'Menus del perfil guardados correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar los menus del perfil: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO
```

- [ ] **Step 2: Crear el script de arreglo de datos**

Crear `docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-fix-datos.sql`:

```sql
/* ============================================================================
   Arreglo one-off: activar el padre de todo hijo activo (invariante).
   Idempotente. Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

/* Conteo ANTES (submenus activos cuyo padre no esta activo para el perfil) */
SELECT CasosAntes = COUNT(*)
FROM dbo.PerfilMenu pm
JOIN dbo.MenuDos child ON child.Id_Menu = pm.id_Menu
WHERE pm.Estado = 0 AND ISNULL(child.Id_MenuPadre,0) <> 0
  AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu p2
                  WHERE p2.IdPerfil = pm.IdPerfil AND p2.id_Menu = child.Id_MenuPadre AND p2.Estado = 0);

/* 1) Insertar la fila del padre (Estado=0) cuando no existe */
INSERT INTO dbo.PerfilMenu (IdPerfil, id_Menu, Estado)
SELECT DISTINCT pm.IdPerfil, child.Id_MenuPadre, 0
FROM dbo.PerfilMenu pm
JOIN dbo.MenuDos child ON child.Id_Menu = pm.id_Menu
WHERE pm.Estado = 0 AND ISNULL(child.Id_MenuPadre,0) <> 0
  AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu p2
                  WHERE p2.IdPerfil = pm.IdPerfil AND p2.id_Menu = child.Id_MenuPadre);

/* 2) Reactivar la fila del padre cuando existe pero esta inactiva */
UPDATE p2 SET p2.Estado = 0
FROM dbo.PerfilMenu p2
WHERE p2.Estado <> 0
  AND EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
              JOIN dbo.MenuDos child ON child.Id_Menu = pm.id_Menu
              WHERE pm.Estado = 0 AND pm.IdPerfil = p2.IdPerfil AND child.Id_MenuPadre = p2.id_Menu);

/* Conteo DESPUES (debe ser 0) */
SELECT CasosDespues = COUNT(*)
FROM dbo.PerfilMenu pm
JOIN dbo.MenuDos child ON child.Id_Menu = pm.id_Menu
WHERE pm.Estado = 0 AND ISNULL(child.Id_MenuPadre,0) <> 0
  AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu p2
                  WHERE p2.IdPerfil = pm.IdPerfil AND p2.id_Menu = child.Id_MenuPadre AND p2.Estado = 0);
```

- [ ] **Step 3: Ejecutar ambos scripts**

Run:
```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas" && SQL="tcp:192.168.11.14,1433"
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -b -i "docs/superpowers/plans/sql/2026-07-27-menus-por-perfil.sql" && echo "SPs OK"
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -b -i "docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-fix-datos.sql"
```
Expected: SPs sin errores; el fix imprime `CasosAntes` (≥0) y `CasosDespues = 0`. (Si da timeout, reintentar — la conexión es intermitente.)

- [ ] **Step 4: Verificar los SPs**

Run (usa un perfil real, p. ej. 1):
```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas" && SQL="tcp:192.168.11.14,1433"
echo "=== perfiles ==="
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "EXEC dbo.Sp_RTA_ListarPerfiles;"
echo "=== menus del perfil 1 (algunas filas) ==="
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "SET NOCOUNT ON; SELECT TOP 10 * FROM (SELECT * FROM (SELECT Id_Menu,Id_MenuPadre,Titulo,Class_Icon,Activo=CASE WHEN pm.id_Menu IS NOT NULL THEN 1 ELSE 0 END FROM dbo.MenuDos m LEFT JOIN dbo.PerfilMenu pm ON pm.id_Menu=m.Id_Menu AND pm.IdPerfil=1 AND pm.Estado=0) z) y;"
echo "=== guardar: activar SOLO un hijo (p.ej. 3, hijo de 2) para un perfil de prueba y ver que activa el padre ==="
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "EXEC dbo.Sp_RTA_GuardarMenuPerfil @IdPerfil=1, @ActivosCsv='3';"
echo "=== confirmar que 2 (padre) y 3 (hijo) quedaron activos para perfil 1 ==="
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "SET NOCOUNT ON; SELECT id_Menu, Estado FROM dbo.PerfilMenu WHERE IdPerfil=1 AND id_Menu IN (2,3) ORDER BY id_Menu;"
```
Expected: `Sp_RTA_ListarMenuPerfil` trae los menús con `Activo` 0/1; el guardar con `@ActivosCsv='3'` devuelve `Respuestas=1` y deja `id_Menu` 2 **y** 3 con `Estado=0` (el padre 2 se activó por la invariante).

> Nota: el Step 4 modifica los menús del perfil 1 en la BD de dev. Es aceptable en dev; si prefieres no alterar el perfil 1, usa un IdPerfil de prueba que exista en `R_Perfil`.

- [ ] **Step 5: Commit**

```bash
git add "docs/superpowers/plans/sql/2026-07-27-menus-por-perfil.sql" "docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-fix-datos.sql"
git commit -m "feat(db): SPs de menus por perfil (guardado atomico con invariante padre) + arreglo de datos"
```

---

## Task 2: Backend C# (Entidad + Dato + Negocio)

**Files:**
- Create: `CapaEntidad/EntMenuPerfil.cs`
- Create: `CapaEntidad/EntPerfil.cs`
- Create: `CapaDato/DaoMenuPerfil.cs`
- Create: `CapaNegocio/NegMenuPerfil.cs`

**Interfaces:**
- Consumes (de Task 1): los 3 SPs. Reutiliza `EntRespuesta`.
- Produces (para Tasks 3):
  - `EntMenuPerfil { int Id_Menu; int Id_MenuPadre; string Titulo; string Class_Icon; int Activo; }`
  - `EntPerfil { int Id_Perfil; string Nombre; }`
  - `NegMenuPerfil.ListarPerfiles()` → `List<EntPerfil>`
  - `NegMenuPerfil.ListarMenuPerfil(int idPerfil)` → `List<EntMenuPerfil>`
  - `NegMenuPerfil.GuardarMenuPerfil(int idPerfil, string activosCsv)` → `EntRespuesta`

- [ ] **Step 1: Crear las entidades**

Crear `CapaEntidad/EntMenuPerfil.cs`:

```csharp
namespace CapaEntidad
{
    public class EntMenuPerfil
    {
        public int Id_Menu { get; set; }
        public int Id_MenuPadre { get; set; }
        public string Titulo { get; set; }
        public string Class_Icon { get; set; }
        public int Activo { get; set; }
    }
}
```

Crear `CapaEntidad/EntPerfil.cs`:

```csharp
namespace CapaEntidad
{
    public class EntPerfil
    {
        public int Id_Perfil { get; set; }
        public string Nombre { get; set; }
    }
}
```

- [ ] **Step 2: Crear el Dao**

Crear `CapaDato/DaoMenuPerfil.cs`:

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoMenuPerfil
    {
        /// <summary>Lista los perfiles activos (para el combo).</summary>
        public static List<EntPerfil> ListarPerfiles()
        {
            List<EntPerfil> lista = new List<EntPerfil>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarPerfiles", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntPerfil()
                        {
                            Id_Perfil = Convert.ToInt32(dr["Id_Perfil"].ToString()),
                            Nombre = dr["Nombre"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Lista todos los menús con su estado (activo 0/1) para el perfil.</summary>
        public static List<EntMenuPerfil> ListarMenuPerfil(int idPerfil)
        {
            List<EntMenuPerfil> lista = new List<EntMenuPerfil>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarMenuPerfil", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = idPerfil;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntMenuPerfil()
                        {
                            Id_Menu = Convert.ToInt32(dr["Id_Menu"].ToString()),
                            Id_MenuPadre = Convert.ToInt32(dr["Id_MenuPadre"].ToString()),
                            Titulo = dr["Titulo"].ToString(),
                            Class_Icon = dr["Class_Icon"].ToString(),
                            Activo = Convert.ToInt32(dr["Activo"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Guarda (atómico) el set de menús activos del perfil; el SP agrega los padres.</summary>
        public static EntRespuesta GuardarMenuPerfil(int idPerfil, string activosCsv)
        {
            EntRespuesta respuesta = new EntRespuesta()
            {
                estado = "0",
                resultado = "0",
                tipoMensaje = "danger",
                mensaje = ""
            };

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarMenuPerfil", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = idPerfil;
                    cmd.Parameters.Add("@ActivosCsv", SqlDbType.VarChar, -1).Value = activosCsv ?? string.Empty;

                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            int respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());
                            respuesta.resultado = respuestaSP.ToString();
                            respuesta.mensaje = dr["Mensaje"].ToString();

                            if (respuestaSP > 0)
                            {
                                respuesta.estado = "1";
                                respuesta.tipoMensaje = "success";
                            }
                            else
                            {
                                respuesta.estado = "0";
                                respuesta.tipoMensaje = "warning";
                            }
                        }
                        else
                        {
                            respuesta.mensaje = "El procedimiento no devolvió información.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.resultado = "0";
                respuesta.tipoMensaje = "danger";
                respuesta.mensaje = "Ocurrió un error al guardar los menús del perfil. Detalle: " + ex.Message;
            }

            return respuesta;
        }
    }
}
```

- [ ] **Step 3: Crear la capa de Negocio**

Crear `CapaNegocio/NegMenuPerfil.cs`:

```csharp
using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegMenuPerfil
    {
        public static List<EntPerfil> ListarPerfiles()
        {
            return DaoMenuPerfil.ListarPerfiles();
        }

        public static List<EntMenuPerfil> ListarMenuPerfil(int idPerfil)
        {
            return DaoMenuPerfil.ListarMenuPerfil(idPerfil);
        }

        public static EntRespuesta GuardarMenuPerfil(int idPerfil, string activosCsv)
        {
            return DaoMenuPerfil.GuardarMenuPerfil(idPerfil, activosCsv);
        }
    }
}
```

- [ ] **Step 4: Registrar los archivos en los `.csproj`**

Los proyectos de capa listan archivos con `<Compile Include>` explícitos (verificado). Agregar junto a las entradas existentes:
- `CapaEntidad/CapaEntidad.csproj`: `<Compile Include="EntMenuPerfil.cs" />` y `<Compile Include="EntPerfil.cs" />`
- `CapaDato/CapaDato.csproj`: `<Compile Include="DaoMenuPerfil.cs" />`
- `CapaNegocio/CapaNegocio.csproj`: `<Compile Include="NegMenuPerfil.cs" />`

- [ ] **Step 5: Compilar**

Run (PowerShell):
```powershell
$msb = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"; & $msb "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly; "EXIT CODE: $LASTEXITCODE"
```
Expected: `EXIT CODE: 0`.

- [ ] **Step 6: Commit**

```bash
git add "CapaEntidad/EntMenuPerfil.cs" "CapaEntidad/EntPerfil.cs" "CapaDato/DaoMenuPerfil.cs" "CapaNegocio/NegMenuPerfil.cs" "CapaEntidad/CapaEntidad.csproj" "CapaDato/CapaDato.csproj" "CapaNegocio/CapaNegocio.csproj"
git commit -m "feat(backend): capas de menus por perfil"
```

---

## Task 3: Handler (.ashx)

**Files:**
- Create: `ReporteTareas/Formulario/AdministrarMenuPerfil.ashx`
- Create: `ReporteTareas/Formulario/AdministrarMenuPerfil.ashx.cs`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes (de Task 2): `NegMenuPerfil.ListarPerfiles()`, `ListarMenuPerfil(int)`, `GuardarMenuPerfil(int, string)`.
- Produces (para Task 4): acciones `ListaPerfiles`, `ListaMenuPerfil {idPerfil}`, `GuardarMenuPerfil {idPerfil, activos:[ids]}`.

- [ ] **Step 1: Crear el markup**

Crear `ReporteTareas/Formulario/AdministrarMenuPerfil.ashx`:

```
<%@ WebHandler Language="C#" CodeBehind="AdministrarMenuPerfil.ashx.cs" Class="JsonJQueryNetMenuPerfil.AdministrarMenuPerfil" %>
```

- [ ] **Step 2: Crear el code-behind**

Crear `ReporteTareas/Formulario/AdministrarMenuPerfil.ashx.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetMenuPerfil
{
    /// <summary>
    /// Handler de la pantalla "Parametrización de menús por perfil".
    /// Acciones: ListaPerfiles, ListaMenuPerfil, GuardarMenuPerfil.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarMenuPerfil : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();

                JavaScriptSerializer i = new JavaScriptSerializer();
                dynamic parametros = i.Deserialize(inputJson.ToString(), typeof(object));

                var parameters = parametros[0]["parameters"];
                var Action = parametros[0]["action"];
                bool existAction = false;

                if (Action == "ListaPerfiles")
                {
                    existAction = true;
                    responseAction.Append(ListaPerfiles(parameters));
                }

                if (Action == "ListaMenuPerfil")
                {
                    existAction = true;
                    responseAction.Append(ListaMenuPerfil(parameters));
                }

                if (Action == "GuardarMenuPerfil")
                {
                    existAction = true;
                    responseAction.Append(GuardarMenuPerfil(parameters));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }

            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        private string ListaPerfiles(dynamic campos)
        {
            try
            {
                return ToJson(NegMenuPerfil.ListarPerfiles());
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los perfiles. " + ex.Message, "danger");
            }
        }

        private string ListaMenuPerfil(dynamic campos)
        {
            try
            {
                int idPerfil = 0;
                try { idPerfil = Convert.ToInt32(campos["idPerfil"]); }
                catch { idPerfil = 0; }

                if (idPerfil <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un perfil.", "warning");
                }

                return ToJson(NegMenuPerfil.ListarMenuPerfil(idPerfil));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los menús. " + ex.Message, "danger");
            }
        }

        private string GuardarMenuPerfil(dynamic campos)
        {
            try
            {
                int idPerfil = 0;
                try { idPerfil = Convert.ToInt32(campos["idPerfil"]); }
                catch { idPerfil = 0; }

                if (idPerfil <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un perfil.", "warning");
                }

                // 'activos' es un arreglo de ids (enteros). Se arma un CSV validado.
                List<string> ids = new List<string>();
                try
                {
                    var activos = campos["activos"];
                    if (activos != null)
                    {
                        foreach (var v in activos)
                        {
                            int id = Convert.ToInt32(v);
                            if (id > 0) { ids.Add(id.ToString()); }
                        }
                    }
                }
                catch { ids = new List<string>(); }

                string csv = string.Join(",", ids);

                EntRespuesta respuesta = NegMenuPerfil.GuardarMenuPerfil(idPerfil, csv);
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar los menús del perfil. " + ex.Message, "danger");
            }
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

- [ ] **Step 3: Registrar en el .csproj**

En `ReporteTareas/ReporteTareas.csproj`, junto a las entradas de otro `.ashx` (p. ej. `AdministrarPerfilInicio.ashx`):
```xml
    <Content Include="Formulario\AdministrarMenuPerfil.ashx" />
```
y
```xml
    <Compile Include="Formulario\AdministrarMenuPerfil.ashx.cs">
      <DependentUpon>AdministrarMenuPerfil.ashx</DependentUpon>
    </Compile>
```

- [ ] **Step 4: Compilar**

Run (PowerShell):
```powershell
$msb = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"; & $msb "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly; "EXIT CODE: $LASTEXITCODE"
```
Expected: `EXIT CODE: 0`.

- [ ] **Step 5: Verificar por HTTP (opcional / entorno con BD)**

Con IIS Express 32-bit en `:51037`, POST a `AdministrarMenuPerfil.ashx`:
```powershell
function Call($body) {
  $url = "http://localhost:51037/Formulario/AdministrarMenuPerfil.ashx"
  $req = [System.Net.HttpWebRequest]::Create($url); $req.Method="POST"; $req.ContentType="application/json; charset=utf-8"
  $b=[System.Text.Encoding]::UTF8.GetBytes($body); $req.ContentLength=$b.Length
  $s=$req.GetRequestStream(); $s.Write($b,0,$b.Length); $s.Close()
  $resp=$req.GetResponse(); $ms=New-Object System.IO.MemoryStream; $resp.GetResponseStream().CopyTo($ms); $resp.Close()
  [System.Text.Encoding]::UTF8.GetString($ms.ToArray())
}
Call '[{"action":"ListaPerfiles","parameters":{}}]'
Call '[{"action":"ListaMenuPerfil","parameters":{"idPerfil":1}}]'
Call '[{"action":"GuardarMenuPerfil","parameters":{"idPerfil":1,"activos":[3]}}]'
```
Expected: perfiles con tildes correctas; menús con `Activo`; guardar → `{"estado":"1",...,"mensaje":"Menus del perfil guardados correctamente."}`. (Si la app no alcanza la BD desde esta máquina, diferir; el nivel SQL y la compilación ya cubren la lógica.)

- [ ] **Step 6: Commit**

```bash
git add "ReporteTareas/Formulario/AdministrarMenuPerfil.ashx" "ReporteTareas/Formulario/AdministrarMenuPerfil.ashx.cs" "ReporteTareas/ReporteTareas.csproj"
git commit -m "feat(handler): AdministrarMenuPerfil.ashx (listar/guardar menus por perfil)"
```

---

## Task 4: Frontend (ASPX + designer + JS)

**Files:**
- Create: `ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx`
- Create: `ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx.cs`
- Create: `ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx.designer.cs`
- Create: `ReporteTareas/js/parametrizacionMenuPerfil.js`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes (de Task 3): `ListaPerfiles`, `ListaMenuPerfil`, `GuardarMenuPerfil`.

- [ ] **Step 1: Crear el `.aspx`**

Crear `ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx`:

```html
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionMenuPerfil.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionMenuPerfil" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionMenuPerfil.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Parametrización de menús por perfil</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    Menús que ve cada perfil. Al activar un submenú se activa también su menú padre.
                    <div style="display: none">
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                    </div>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-5">
                            <label>Perfil:</label>
                            <select id="cmbPerfil" class="form-control" onchange="BuscarMenus()"></select>
                        </div>
                        <div class="form-group col-lg-4" style="padding-top: 25px">
                            <button id="btnGuardar" onclick="GuardarMenus()" type="button" class="btn btn-success">Guardar</button>
                        </div>
                    </div>

                    <div class="col-lg-12" style="padding: 0px">
                        <div class="panel panel-default">
                            <div class="panel-heading"><h4>Menús</h4></div>
                            <div class="panel-body" style="height: 460px; overflow-y: auto; overflow-x: auto;">
                                <div id="datosArbolMenu" style="padding: 0px"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Informativo -->
        <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-labelledby="modalMensajeInformativoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeInformativoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="myModalLabel">Informativo</h4>
                    </div>
                    <div class="modal-body" id="MensajeInformativo"></div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->
    </div>
</asp:Content>
```

- [ ] **Step 2: Crear el code-behind**

Crear `ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx.cs`:

```csharp
using ReporteTareas.Controles;
using SeguridadAppHelper;
using System;

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionMenuPerfil : System.Web.UI.Page
    {
        #region Variables
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (Session["UserLogin"] != null)
            {
                if (!IsPostBack)
                {
                    try
                    {
                        string CodUnico = Session["Cod_Usuario"].ToString();
                        SeguridadHelper seguridad = new SeguridadHelper();
                        txtUsuario.Text = seguridad.Encripta(CodUnico.ToString());
                        txtLoginUsuario.Text = Session["UserLogin"].ToString();

                        if (Session["IdCliente"] != null)
                        {
                            txtIdCliente.Text = Session["IdCliente"].ToString();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
```

- [ ] **Step 3: Crear el `.designer.cs`**

Crear `ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx.designer.cs`:

```csharp
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionMenuPerfil
    {
        /// <summary>Control txtUsuario.</summary>
        protected global::System.Web.UI.WebControls.TextBox txtUsuario;

        /// <summary>Control txtLoginUsuario.</summary>
        protected global::System.Web.UI.WebControls.TextBox txtLoginUsuario;

        /// <summary>Control txtIdCliente.</summary>
        protected global::System.Web.UI.WebControls.TextBox txtIdCliente;
    }
}
```

- [ ] **Step 4: Crear el `.js`**

Crear `ReporteTareas/js/parametrizacionMenuPerfil.js` (recordar **UTF-8 con BOM** — Step 6):

```javascript
/* ============================================================================
   Pantalla: Parametrizacion de menus por perfil (arbol padre-hijo)
   Handler : AdministrarMenuPerfil.ashx
   ============================================================================ */

var _menus = [];

$(document).ready(function () {
    CargarPerfiles();
});

function PostMenu(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);
    $.ajax({
        type: "POST",
        url: "AdministrarMenuPerfil.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) { onSuccess(respuesta); },
        error: function () {
            MostrarMensaje("La operación está tomando demasiado tiempo o la red está saturada. Intente nuevamente.", "danger");
        }
    });
}

function CargarPerfiles() {
    PostMenu("ListaPerfiles", {}, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        var lista = respuesta || [];
        var html = "<option value=''>-- Seleccione --</option>";
        $.each(lista, function (i, p) {
            html += "<option value='" + EscaparAttr(String(p.Id_Perfil)) + "'>" + Escapar(p.Nombre) + "</option>";
        });
        $("#cmbPerfil").html(html);
        $("#datosArbolMenu").html("");
    });
}

function BuscarMenus() {
    var idPerfil = $("#cmbPerfil").val();
    if (idPerfil == null || idPerfil === "") {
        $("#datosArbolMenu").html("");
        return;
    }
    PostMenu("ListaMenuPerfil", { "idPerfil": idPerfil }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _menus = respuesta || [];
        RenderArbol(_menus);
    });
}

function RenderArbol(lista) {
    var padres = $.grep(lista, function (m) { return m.Id_MenuPadre == 0; });

    var info = "";
    if (padres.length === 0) {
        info = "<p>No existen menús.</p>";
        $("#datosArbolMenu").html(info);
        return;
    }

    $.each(padres, function (i, padre) {
        var chkP = (padre.Activo == 1) ? "checked" : "";
        info += "<div style='margin:4px 0'>";
        info += "<label style='font-weight:bold'>";
        info += "<input type='checkbox' class='chkMenu chkPadre' data-menu='" + EscaparAttr(String(padre.Id_Menu)) + "' " + chkP + " onchange='OnPadreChange(" + padre.Id_Menu + ")' /> ";
        info += "<i class='" + EscaparAttr(padre.Class_Icon) + "'></i> " + Escapar(padre.Titulo);
        info += "</label>";

        var hijos = $.grep(lista, function (m) { return m.Id_MenuPadre == padre.Id_Menu; });
        $.each(hijos, function (j, hijo) {
            var chkH = (hijo.Activo == 1) ? "checked" : "";
            info += "<div style='margin-left:28px'>";
            info += "<label>";
            info += "<input type='checkbox' class='chkMenu chkHijo' data-menu='" + EscaparAttr(String(hijo.Id_Menu)) + "' data-padre='" + EscaparAttr(String(padre.Id_Menu)) + "' " + chkH + " onchange='OnHijoChange(" + padre.Id_Menu + ")' /> ";
            info += "<i class='" + EscaparAttr(hijo.Class_Icon) + "'></i> " + Escapar(hijo.Titulo);
            info += "</label>";
            info += "</div>";
        });
        info += "</div>";
    });

    $("#datosArbolMenu").html(info);
}

/* Marcar un hijo auto-marca el padre */
function OnHijoChange(idPadre) {
    var algunHijo = $(".chkHijo[data-padre='" + idPadre + "']:checked").length > 0;
    if (algunHijo) {
        $(".chkPadre[data-menu='" + idPadre + "']").prop("checked", true);
    }
}

/* Desmarcar el padre desmarca sus hijos */
function OnPadreChange(idPadre) {
    var padreChecked = $(".chkPadre[data-menu='" + idPadre + "']").is(":checked");
    if (!padreChecked) {
        $(".chkHijo[data-padre='" + idPadre + "']").prop("checked", false);
    }
}

function GuardarMenus() {
    var idPerfil = $("#cmbPerfil").val();
    if (idPerfil == null || idPerfil === "") {
        MostrarMensaje("Debe seleccionar un perfil.", "warning");
        return;
    }

    var activos = [];
    $(".chkMenu:checked").each(function () {
        activos.push(parseInt($(this).attr("data-menu"), 10));
    });

    $("#btnGuardar").prop("disabled", true);
    PostMenu("GuardarMenuPerfil", { "idPerfil": idPerfil, "activos": activos }, function (respuesta) {
        $("#btnGuardar").prop("disabled", false);
        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado == "1") {
            BuscarMenus();
        }
    });
}

/* ----------------------------- utilitarios ------------------------------- */

function MostrarMensaje(mensaje, tipo) {
    var color = "#fcf8e3";
    if (tipo == "success") { color = "#dff0d8"; }
    if (tipo == "danger") { color = "#f2dede"; }
    if (tipo == "warning") { color = "#fcf8e3"; }
    $("#modalMensajeInformativoTipo").css("background", color);
    $("#MensajeInformativo").html(mensaje);
    $("#modalMensajeInformativo").modal("show");
}

function Escapar(texto) {
    if (texto == null) { return ""; }
    return $("<div>").text(texto).html();
}

function EscaparAttr(texto) {
    return Escapar(texto).replace(/'/g, "&#39;").replace(/"/g, "&quot;");
}
```

- [ ] **Step 5: Registrar en el .csproj**

En `ReporteTareas/ReporteTareas.csproj`, junto a las entradas de otra pantalla (p. ej. `ParametrizacionPerfilInicio.aspx`):
```xml
    <Content Include="Formulario\ParametrizacionMenuPerfil.aspx" />
```
```xml
    <Content Include="js\parametrizacionMenuPerfil.js" />
```
```xml
    <Compile Include="Formulario\ParametrizacionMenuPerfil.aspx.cs">
      <DependentUpon>ParametrizacionMenuPerfil.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\ParametrizacionMenuPerfil.aspx.designer.cs">
      <DependentUpon>ParametrizacionMenuPerfil.aspx</DependentUpon>
    </Compile>
```

- [ ] **Step 6: Verificar/re-agregar el BOM del JS**

Run (PowerShell):
```powershell
$f = (Resolve-Path "ReporteTareas\js\parametrizacionMenuPerfil.js")
$b = [System.IO.File]::ReadAllBytes($f)
if (-not ($b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF)) {
  $bom = [byte[]](0xEF,0xBB,0xBF)
  $out = New-Object byte[] ($bom.Length + $b.Length)
  [Array]::Copy($bom,0,$out,0,3); [Array]::Copy($b,0,$out,3,$b.Length)
  [System.IO.File]::WriteAllBytes($f,$out); "BOM re-agregado"
} else { "BOM OK" }
```
Expected: `BOM OK` o `BOM re-agregado`.

- [ ] **Step 7: Compilar**

Run (PowerShell):
```powershell
$msb = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"; & $msb "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly; "EXIT CODE: $LASTEXITCODE"
```
Expected: `EXIT CODE: 0`.

- [ ] **Step 8: Verificación en navegador (entorno con BD)**

Abrir `http://localhost:51037/Formulario/ParametrizacionMenuPerfil.aspx` con sesión iniciada, **Ctrl+F5**. Comprobar:
1. El combo trae los perfiles; al elegir uno se dibuja el árbol (padres con hijos indentados) con los checkboxes reflejando el estado actual.
2. Marcar un **submenú** auto-marca su **padre**; desmarcar un padre desmarca sus hijos.
3. **Guardar** → mensaje de éxito; iniciar sesión con ese perfil y confirmar que el submenú **ahora aparece** en el sidebar.

- [ ] **Step 9: Commit**

```bash
git add "ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx" "ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx.cs" "ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx.designer.cs" "ReporteTareas/js/parametrizacionMenuPerfil.js" "ReporteTareas/ReporteTareas.csproj"
git commit -m "feat(ui): pantalla de menus por perfil con arbol padre-hijo"
```

---

## Task 5: Registro en el menú (SQL)

**Files:**
- Create: `docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-menu.sql`

**Interfaces:**
- Consumes: la pantalla `ParametrizacionMenuPerfil.aspx` (Task 4) debe existir.
- Produces: ítem de menú bajo "Manejo de Perfiles" (20042) habilitado para perfiles 1, 2, 18, 19.

- [ ] **Step 1: Crear el script**

Crear `docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-menu.sql`:

```sql
/* ============================================================================
   Registro en menu de "Parametrizacion de menus por perfil"
   Base: ReporTarea — Padre: 20042 (Manejo de Perfiles) — Perfiles: 1,2,18,19
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

DECLARE @Titulo VARCHAR(200) = 'Menus por Perfil';
DECLARE @Href   VARCHAR(200) = 'ParametrizacionMenuPerfil.aspx';
DECLARE @Icono  VARCHAR(100) = 'fa fa-sitemap';
DECLARE @Padre  INT = 20042;
DECLARE @IdMenu INT;

SELECT @IdMenu = Id_Menu FROM dbo.MenuDos WHERE Href = @Href;

IF @IdMenu IS NULL
BEGIN
    INSERT INTO dbo.MenuDos (Titulo, Href, Class_Icon, Id_MenuPadre)
    VALUES (@Titulo, @Href, @Icono, @Padre);
    SET @IdMenu = SCOPE_IDENTITY();
END

DECLARE @Perfiles TABLE (IdPerfil INT);
INSERT INTO @Perfiles (IdPerfil) VALUES (1),(2),(18),(19);

INSERT INTO dbo.PerfilMenu (IdPerfil, id_Menu, Estado)
SELECT p.IdPerfil, m.id_Menu, 0
FROM @Perfiles p
CROSS JOIN (SELECT @IdMenu AS id_Menu UNION SELECT @Padre) m
WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
                  WHERE pm.IdPerfil = p.IdPerfil AND pm.id_Menu = m.id_Menu);

UPDATE pm SET pm.Estado = 0
FROM dbo.PerfilMenu pm
JOIN @Perfiles p ON p.IdPerfil = pm.IdPerfil
WHERE pm.id_Menu IN (@IdMenu, @Padre);

SELECT Id_Menu = @IdMenu, Href = @Href;
```

- [ ] **Step 2: Ejecutar**

Run:
```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas" && sqlcmd -S "tcp:192.168.11.14,1433" -U sa -P "CAfKsUBnD0s" -d ReporTarea -b -i "docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-menu.sql"
```
Expected: devuelve el `Id_Menu` insertado; sin errores.

- [ ] **Step 3: Verificar**

Run:
```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas" && SQL="tcp:192.168.11.14,1433"
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "SET NOCOUNT ON; SELECT m.Id_Menu, m.Titulo, m.Id_MenuPadre, m.Href FROM dbo.MenuDos m WHERE m.Href='ParametrizacionMenuPerfil.aspx'; SELECT pm.IdPerfil, pm.id_Menu, pm.Estado FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu=pm.id_Menu WHERE m.Href='ParametrizacionMenuPerfil.aspx' ORDER BY pm.IdPerfil;"
```
Expected: 1 fila en MenuDos con `Id_MenuPadre=20042`; 4 filas en PerfilMenu (perfiles 1,2,18,19) con `Estado=0`.

- [ ] **Step 4: Commit**

```bash
git add "docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-menu.sql"
git commit -m "feat(menu): registro de ParametrizacionMenuPerfil (perfiles 1,2,18,19)"
```

---

## Self-Review (cobertura del spec)

- **SP `ListaPerfiles` / `ListarMenuPerfil` / `GuardarMenuPerfil` (atómico, agrega padres)** → Task 1, Step 1. ✅
- **Script one-off de arreglo de datos (idempotente, conteo antes/después)** → Task 1, Step 2. ✅
- **Entidades `EntMenuPerfil` / `EntPerfil`** → Task 2, Step 1. ✅
- **Dao (3 métodos) + Negocio** → Task 2, Steps 2-3. ✅
- **Handler `ListaPerfiles`/`ListaMenuPerfil`/`GuardarMenuPerfil` (arma CSV validado del arreglo `activos`) + UTF-8** → Task 3. ✅
- **Pantalla con árbol padre-hijo; hijo marca padre; padre desmarca hijos** → Task 4, Step 4 (`RenderArbol`, `OnHijoChange`, `OnPadreChange`). ✅
- **BOM del JS / `EscaparAttr`** → Task 4, Steps 4 y 6. ✅
- **Registro en menú (perfiles 1,2,18,19)** → Task 5. ✅
- **No se toca `Master.Master.cs` ni `PruebaMenu.aspx`** → ningún task los incluye. ✅

Sin placeholders. Firmas consistentes entre tareas: `ListarPerfiles()→List<EntPerfil>`, `ListarMenuPerfil(int)→List<EntMenuPerfil>`, `GuardarMenuPerfil(int,string)→EntRespuesta`; acción `GuardarMenuPerfil {idPerfil, activos:[ids]}`; `Activo` 0/1; `Estado=0`=activo.
