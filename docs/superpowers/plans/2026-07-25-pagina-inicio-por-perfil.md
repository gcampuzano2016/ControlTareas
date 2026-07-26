# Página de inicio y "tipo" por perfil — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hacer configurable desde el sistema la página de inicio y el "tipo" (`Session["Id_Usuario"]`) por perfil, eliminando los `if (Id_Perfil == N)` quemados y duplicados de `Login.aspx.cs`.

**Architecture:** Tabla dedicada `RTA_PerfilInicio` (una fila por perfil con `Href` + `IdTipo`) con seed que preserva el comportamiento actual; 4 SPs; 3 capas C# (Entidad/Dato/Negocio); un handler `.ashx` y una pantalla `.aspx` de administración; y un refactor del `Login` que lee la tabla vía un SP con fallback fail-safe. Mismo patrón que el módulo "Ventana de Aprobación".

**Tech Stack:** ASP.NET WebForms (C#, .NET Framework 4.6.1), arquitectura 3 capas (CapaEntidad/CapaDato/CapaNegocio) + web `ReporteTareas`, SQL Server (stored procedures), jQuery.

## Global Constraints

- **Sin framework de pruebas automatizadas.** Verificación manual: SQL directo (sqlcmd), HTTP al handler (PowerShell) revisando el JSON, y navegador para la UI.
- **Codificación:** los `.js` van en **UTF-8 con BOM**; el handler escribe con `context.Response.ContentEncoding = Encoding.UTF8`. Tras editar el `.js`, **verificar que el BOM siga presente** y re-agregarlo si se perdió.
- **Identidad del perfil:** `IdPerfil` = `R_Perfil.Id_Perfil` = `Session["Id_Perfil"]` = `PerfilMenu.IdPerfil`.
- **Fallback fail-safe:** un perfil sin configuración o cualquier error ⇒ `Href='Principal.aspx'`, `IdTipo=0`. El login nunca se bloquea por este módulo.
- **Origen de la página de inicio:** desplegable poblado desde `MenuDos` (Titulo + Href). No texto libre.
- **Base de datos:** `Data Source=192.168.11.14; Initial Catalog=ReporTarea; User Id=sa; Password=CAfKsUBnD0s`. Desde esta máquina el server responde por `tcp:192.168.11.14,1433` (usar ese `-S` en sqlcmd).
- **MSBuild:** `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`, solución `ReporteTareas.sln`, configuración `Debug`. Ejecutar vía PowerShell (`& $msb ...`) para evitar el quoting de la ruta con espacios.
- **App local para pruebas:** requiere **IIS Express de 32 bits** (`C:\Program Files (x86)\IIS Express\iisexpress.exe`) por la dependencia x86 `Pechkin`. Lanzar con `/path:` a `...\ReporteTareas` y `/port:51037`. Nota: desde esta máquina el `SqlClient` de la app puede no alcanzar la BD (negocia Named Pipes); las pruebas HTTP/navegador que dependen de datos pueden requerir el entorno real del usuario. El nivel SQL y la compilación sí se verifican aquí.
- **Mensajes fijos de SP sin tildes** ("configuracion", "pagina") para evitar cualquier riesgo de codificación al ejecutar el script.

---

## File Structure

- **Crear** `docs/superpowers/plans/sql/2026-07-25-pagina-inicio-por-perfil.sql` — tabla + seed + 4 SPs.
- **Crear** `docs/superpowers/plans/sql/2026-07-25-pagina-inicio-por-perfil-menu.sql` — registro en `MenuDos` + `PerfilMenu`.
- **Crear** `CapaEntidad/EntPerfilInicio.cs` — entidad de configuración.
- **Crear** `CapaDato/DaoPerfilInicio.cs` — acceso a los 4 SPs.
- **Crear** `CapaNegocio/NegPerfilInicio.cs` — pass-through.
- **Crear** `ReporteTareas/Formulario/AdministrarPerfilInicio.ashx` (+ `.ashx.cs`) — handler.
- **Crear** `ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx` (+ `.aspx.cs`, `.aspx.designer.cs`) — pantalla.
- **Crear** `ReporteTareas/js/parametrizacionPerfilInicio.js` — lógica de la pantalla.
- **Modificar** `ReporteTareas/ReporteTareas.csproj` — registrar los archivos nuevos.
- **Modificar** `ReporteTareas/Formulario/Login.aspx.cs` — reemplazar los dos bloques quemados por la lectura del SP.

---

## Task 1: Base de datos — tabla, seed y SPs

**Files:**
- Create: `docs/superpowers/plans/sql/2026-07-25-pagina-inicio-por-perfil.sql`

**Interfaces:**
- Produces:
  - Tabla `dbo.RTA_PerfilInicio(IdPerfil PK, Href, IdTipo, Estado, UsuarioRegistro, FechaRegistro)`.
  - `Sp_RTA_ListarPerfilInicio()` → `IdPerfil, NombrePerfil, Href, TituloPagina, IdTipo`.
  - `Sp_RTA_GuardarPerfilInicio(@IdPerfil, @Href, @IdTipo, @UsuarioRegistro)` → `Respuestas INT, Mensaje VARCHAR`.
  - `Sp_RTA_ObtenerPerfilInicio(@IdPerfil)` → una fila `Href, IdTipo` (con fallback `Principal.aspx`/`0`).
  - `Sp_RTA_ListarPaginasMenu()` → `Titulo, Href`.

- [ ] **Step 1: Crear el script SQL**

Crear `docs/superpowers/plans/sql/2026-07-25-pagina-inicio-por-perfil.sql`:

```sql
/* ============================================================================
   Pagina de inicio y "tipo" por perfil — objetos de BD
   Base: ReporTarea
   ============================================================================ */

/* ---------- 1) Tabla ---------- */
IF OBJECT_ID('dbo.RTA_PerfilInicio','U') IS NULL
BEGIN
    CREATE TABLE dbo.RTA_PerfilInicio
    (
        IdPerfil        INT           NOT NULL,
        Href            VARCHAR(150)  NOT NULL,
        IdTipo          INT           NOT NULL,
        Estado          BIT           NOT NULL CONSTRAINT DF_RTA_PerfilInicio_Estado   DEFAULT(1),
        UsuarioRegistro VARCHAR(100)  NULL,
        FechaRegistro   DATETIME      NOT NULL CONSTRAINT DF_RTA_PerfilInicio_FechaReg DEFAULT(GETDATE()),
        CONSTRAINT PK_RTA_PerfilInicio PRIMARY KEY (IdPerfil)
    );
END
GO

/* ---------- 2) Seed (preserva el comportamiento actual del Login) — idempotente ---------- */
;WITH Semilla(IdPerfil, IdTipo) AS (
    SELECT * FROM (VALUES
        (2,1),(3,1),(4,4),(5,5),(6,6),(7,7),(8,8),(9,9),(10,10),(11,11),
        (12,12),(13,13),(14,14),(15,15),(16,16),(17,17),(18,18),(19,19),
        (20,20),(21,21),(41,41)
    ) AS S(IdPerfil, IdTipo)
)
INSERT INTO dbo.RTA_PerfilInicio (IdPerfil, Href, IdTipo, Estado, UsuarioRegistro)
SELECT s.IdPerfil, 'Principal.aspx', s.IdTipo, 1, 'seed'
FROM Semilla s
WHERE NOT EXISTS (SELECT 1 FROM dbo.RTA_PerfilInicio p WHERE p.IdPerfil = s.IdPerfil);
GO

/* ---------- 3) SP Listar (para la pantalla) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarPerfilInicio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPerfilInicio;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarPerfilInicio
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        IdPerfil     = p.Id_Perfil,
        NombrePerfil = p.Nombre,
        Href         = i.Href,
        TituloPagina = m.Titulo,
        IdTipo       = i.IdTipo
    FROM dbo.R_Perfil p
    LEFT JOIN dbo.RTA_PerfilInicio i ON i.IdPerfil = p.Id_Perfil AND i.Estado = 1
    OUTER APPLY (
        SELECT TOP 1 md.Titulo
        FROM dbo.MenuDos md
        WHERE md.Href = i.Href
        ORDER BY md.Id_Menu
    ) m
    WHERE ISNULL(p.Estado_Logico_Registro,1) = 1
    ORDER BY p.Id_Perfil;
END
GO

/* ---------- 4) SP Guardar (upsert, contrato Respuestas/Mensaje) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_GuardarPerfilInicio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarPerfilInicio;
GO
CREATE PROCEDURE dbo.Sp_RTA_GuardarPerfilInicio
    @IdPerfil        INT,
    @Href            VARCHAR(150),
    @IdTipo          INT,
    @UsuarioRegistro VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';
    SET @Href = LTRIM(RTRIM(@Href));

    IF ISNULL(@IdPerfil,0) = 0 OR NOT EXISTS (SELECT 1 FROM dbo.R_Perfil WHERE Id_Perfil = @IdPerfil)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El perfil indicado no existe.'; RETURN;
    END
    IF ISNULL(@Href,'') = ''
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Debe indicar la pagina de inicio.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM dbo.RTA_PerfilInicio WITH (UPDLOCK, HOLDLOCK) WHERE IdPerfil = @IdPerfil)
            UPDATE dbo.RTA_PerfilInicio
               SET Href = @Href, IdTipo = @IdTipo, Estado = 1,
                   UsuarioRegistro = @UsuarioRegistro, FechaRegistro = GETDATE()
             WHERE IdPerfil = @IdPerfil;
        ELSE
            INSERT INTO dbo.RTA_PerfilInicio (IdPerfil, Href, IdTipo, Estado, UsuarioRegistro, FechaRegistro)
            VALUES (@IdPerfil, @Href, @IdTipo, 1, @UsuarioRegistro, GETDATE());

        SET @Respuestas = 1;
        SET @Mensaje = 'Configuracion guardada correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar la configuracion: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO

/* ---------- 5) SP Obtener (para el Login, siempre 1 fila con fallback) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ObtenerPerfilInicio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ObtenerPerfilInicio;
GO
CREATE PROCEDURE dbo.Sp_RTA_ObtenerPerfilInicio
    @IdPerfil INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1
        Href   = ISNULL(i.Href, 'Principal.aspx'),
        IdTipo = ISNULL(i.IdTipo, 0)
    FROM (SELECT 1 AS x) d
    LEFT JOIN dbo.RTA_PerfilInicio i ON i.IdPerfil = @IdPerfil AND i.Estado = 1;
END
GO

/* ---------- 6) SP Listar paginas (para el desplegable) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarPaginasMenu','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPaginasMenu;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarPaginasMenu
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT Titulo, Href
    FROM dbo.MenuDos
    WHERE ISNULL(Href,'') <> '' AND Href NOT LIKE 'Es Men%'
    ORDER BY Titulo;
END
GO
```

- [ ] **Step 2: Ejecutar el script en la base**

Run:
```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas" && sqlcmd -S "tcp:192.168.11.14,1433" -U sa -P "CAfKsUBnD0s" -d ReporTarea -b -i "docs/superpowers/plans/sql/2026-07-25-pagina-inicio-por-perfil.sql"
```
Expected: sin errores (EXIT 0). Crea tabla, seed y 4 SPs.

- [ ] **Step 3: Verificar los SPs**

Run:
```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas" && SQL="tcp:192.168.11.14,1433"
# Listar (debe traer perfiles; los sembrados con Href=Principal.aspx)
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "EXEC dbo.Sp_RTA_ListarPerfilInicio;"
# Obtener perfil configurado (2) -> Principal.aspx / 1
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "EXEC dbo.Sp_RTA_ObtenerPerfilInicio @IdPerfil=2;"
# Obtener perfil NO configurado (9999) -> fallback Principal.aspx / 0
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "EXEC dbo.Sp_RTA_ObtenerPerfilInicio @IdPerfil=9999;"
# Guardar (upsert) perfil 5 -> ReporteGerencia.aspx / 99
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "EXEC dbo.Sp_RTA_GuardarPerfilInicio @IdPerfil=5, @Href='ReporteGerencia.aspx', @IdTipo=99, @UsuarioRegistro='test';"
# Obtener 5 -> ReporteGerencia.aspx / 99
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "EXEC dbo.Sp_RTA_ObtenerPerfilInicio @IdPerfil=5;"
# Restaurar 5 a su seed
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "EXEC dbo.Sp_RTA_GuardarPerfilInicio @IdPerfil=5, @Href='Principal.aspx', @IdTipo=5, @UsuarioRegistro='test';"
# Paginas (debe traer titulos+href, sin los 'Es Menu Principal...')
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "EXEC dbo.Sp_RTA_ListarPaginasMenu;"
```
Expected: `ListarPerfilInicio` sin filas duplicadas por perfil; `ObtenerPerfilInicio` siempre 1 fila; el guardar de 5 devuelve `Respuestas=1`; el fallback de 9999 devuelve `Principal.aspx`/`0`.

- [ ] **Step 4: Commit**

```bash
git add "docs/superpowers/plans/sql/2026-07-25-pagina-inicio-por-perfil.sql"
git commit -m "feat(db): tabla y SPs de pagina de inicio y tipo por perfil"
```

---

## Task 2: Backend C# (Entidad + Dato + Negocio)

**Files:**
- Create: `CapaEntidad/EntPerfilInicio.cs`
- Create: `CapaDato/DaoPerfilInicio.cs`
- Create: `CapaNegocio/NegPerfilInicio.cs`

**Interfaces:**
- Consumes (de Task 1): los 4 SPs. Reutiliza `EntMenuDos` (ya tiene `Titulo` y `Href`) y `EntRespuesta`.
- Produces (para Tasks 3 y 6):
  - `EntPerfilInicio { int IdPerfil; string NombrePerfil; string Href; string TituloPagina; int IdTipo; }`
  - `NegPerfilInicio.ListarPerfilInicio()` → `List<EntPerfilInicio>`
  - `NegPerfilInicio.GuardarPerfilInicio(int idPerfil, string href, int idTipo, string usuarioRegistro)` → `EntRespuesta`
  - `NegPerfilInicio.ObtenerPerfilInicio(int idPerfil)` → `EntPerfilInicio` (con `Href`/`IdTipo`, fallback `Principal.aspx`/`0`)
  - `NegPerfilInicio.ListarPaginasMenu()` → `List<EntMenuDos>`

- [ ] **Step 1: Crear la entidad**

Crear `CapaEntidad/EntPerfilInicio.cs`:

```csharp
namespace CapaEntidad
{
    public class EntPerfilInicio
    {
        public int IdPerfil { get; set; }
        public string NombrePerfil { get; set; }
        public string Href { get; set; }
        public string TituloPagina { get; set; }
        public int IdTipo { get; set; }
    }
}
```

- [ ] **Step 2: Crear el Dao**

Crear `CapaDato/DaoPerfilInicio.cs`:

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoPerfilInicio
    {
        /// <summary>Lista todos los perfiles con su configuración de inicio (o NULL si no la tienen).</summary>
        public static List<EntPerfilInicio> ListarPerfilInicio()
        {
            List<EntPerfilInicio> lista = new List<EntPerfilInicio>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarPerfilInicio", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntPerfilInicio()
                        {
                            IdPerfil = Convert.ToInt32(dr["IdPerfil"].ToString()),
                            NombrePerfil = dr["NombrePerfil"].ToString(),
                            Href = dr["Href"] == DBNull.Value ? "" : dr["Href"].ToString(),
                            TituloPagina = dr["TituloPagina"] == DBNull.Value ? "" : dr["TituloPagina"].ToString(),
                            IdTipo = dr["IdTipo"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdTipo"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Inserta o actualiza la configuración de inicio de un perfil.</summary>
        public static EntRespuesta GuardarPerfilInicio(int idPerfil, string href, int idTipo, string usuarioRegistro)
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
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarPerfilInicio", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = idPerfil;
                    cmd.Parameters.Add("@Href", SqlDbType.VarChar, 150).Value = href ?? string.Empty;
                    cmd.Parameters.Add("@IdTipo", SqlDbType.Int).Value = idTipo;
                    cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 100).Value = (object)usuarioRegistro ?? DBNull.Value;

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
                respuesta.mensaje = "Ocurrió un error al guardar la configuración. Detalle: " + ex.Message;
            }

            return respuesta;
        }

        /// <summary>Devuelve la página de inicio y el "tipo" del perfil. Fail-safe: Principal.aspx / 0.</summary>
        public static EntPerfilInicio ObtenerPerfilInicio(int idPerfil)
        {
            EntPerfilInicio inicio = new EntPerfilInicio()
            {
                IdPerfil = idPerfil,
                Href = "Principal.aspx",
                IdTipo = 0
            };

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_ObtenerPerfilInicio", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = idPerfil;
                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string href = dr["Href"] == DBNull.Value ? "" : dr["Href"].ToString().Trim();
                            if (!string.IsNullOrEmpty(href))
                            {
                                inicio.Href = href;
                            }
                            inicio.IdTipo = dr["IdTipo"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdTipo"].ToString());
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fail-safe: nunca bloquear el login. Se mantienen los valores por defecto.
                inicio.Href = "Principal.aspx";
                inicio.IdTipo = 0;
            }

            return inicio;
        }

        /// <summary>Lista las páginas navegables de MenuDos (para el desplegable).</summary>
        public static List<EntMenuDos> ListarPaginasMenu()
        {
            List<EntMenuDos> lista = new List<EntMenuDos>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarPaginasMenu", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntMenuDos()
                        {
                            Titulo = dr["Titulo"].ToString(),
                            Href = dr["Href"].ToString()
                        });
                    }
                }
            }

            return lista;
        }
    }
}
```

- [ ] **Step 3: Crear la capa de Negocio**

Crear `CapaNegocio/NegPerfilInicio.cs`:

```csharp
using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegPerfilInicio
    {
        public static List<EntPerfilInicio> ListarPerfilInicio()
        {
            return DaoPerfilInicio.ListarPerfilInicio();
        }

        public static EntRespuesta GuardarPerfilInicio(int idPerfil, string href, int idTipo, string usuarioRegistro)
        {
            return DaoPerfilInicio.GuardarPerfilInicio(idPerfil, href, idTipo, usuarioRegistro);
        }

        public static EntPerfilInicio ObtenerPerfilInicio(int idPerfil)
        {
            return DaoPerfilInicio.ObtenerPerfilInicio(idPerfil);
        }

        public static List<EntMenuDos> ListarPaginasMenu()
        {
            return DaoPerfilInicio.ListarPaginasMenu();
        }
    }
}
```

- [ ] **Step 4: Registrar los archivos nuevos en sus proyectos**

`CapaEntidad`, `CapaDato` y `CapaNegocio` incluyen los `.cs` por convención de carpeta o por `<Compile Include>` explícito. Revisar cada `.csproj`: si usa `<Compile Include>` explícitos (busca otras entidades/daos listadas), agregar las líneas correspondientes junto a las existentes:
- En `CapaEntidad/CapaEntidad.csproj`: `<Compile Include="EntPerfilInicio.cs" />`
- En `CapaDato/CapaDato.csproj`: `<Compile Include="DaoPerfilInicio.cs" />`
- En `CapaNegocio/CapaNegocio.csproj`: `<Compile Include="NegPerfilInicio.cs" />`

(Si el `.csproj` no lista archivos individuales —estilo SDK o wildcard— omitir; el build los tomará solo.)

- [ ] **Step 5: Compilar**

Run (PowerShell):
```powershell
$msb = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"; & $msb "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly; "EXIT CODE: $LASTEXITCODE"
```
Expected: `EXIT CODE: 0`.

- [ ] **Step 6: Commit**

```bash
git add "CapaEntidad/EntPerfilInicio.cs" "CapaDato/DaoPerfilInicio.cs" "CapaNegocio/NegPerfilInicio.cs" "CapaEntidad/CapaEntidad.csproj" "CapaDato/CapaDato.csproj" "CapaNegocio/CapaNegocio.csproj"
git commit -m "feat(backend): capas de pagina de inicio y tipo por perfil"
```
(Los `.csproj` solo si se modificaron en el Step 4.)

---

## Task 3: Handler (.ashx)

**Files:**
- Create: `ReporteTareas/Formulario/AdministrarPerfilInicio.ashx`
- Create: `ReporteTareas/Formulario/AdministrarPerfilInicio.ashx.cs`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes (de Task 2): `NegPerfilInicio.ListarPerfilInicio()`, `NegPerfilInicio.ListarPaginasMenu()`, `NegPerfilInicio.GuardarPerfilInicio(...)`.
- Produces (para Task 4): acciones HTTP:
  - `ListaPerfilInicio` → JSON array de perfiles.
  - `ListaPaginas` → JSON array de `{Titulo, Href}`.
  - `GuardarPerfilInicio` con `parameters { idPerfil, href, idTipo, usuarioRegistro }` → `EntRespuesta` JSON.

- [ ] **Step 1: Crear el markup del handler**

Crear `ReporteTareas/Formulario/AdministrarPerfilInicio.ashx`:

```
<%@ WebHandler Language="C#" CodeBehind="AdministrarPerfilInicio.ashx.cs" Class="JsonJQueryNetPerfilInicio.AdministrarPerfilInicio" %>
```

- [ ] **Step 2: Crear el code-behind del handler**

Crear `ReporteTareas/Formulario/AdministrarPerfilInicio.ashx.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetPerfilInicio
{
    /// <summary>
    /// Handler de la pantalla "Parametrización de página de inicio por perfil".
    /// Acciones: ListaPerfilInicio, ListaPaginas, GuardarPerfilInicio.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarPerfilInicio : IHttpHandler
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

                if (Action == "ListaPerfilInicio")
                {
                    existAction = true;
                    responseAction.Append(ListaPerfilInicio(parameters));
                }

                if (Action == "ListaPaginas")
                {
                    existAction = true;
                    responseAction.Append(ListaPaginas(parameters));
                }

                if (Action == "GuardarPerfilInicio")
                {
                    existAction = true;
                    responseAction.Append(GuardarPerfilInicio(parameters));
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

        private string ListaPerfilInicio(dynamic campos)
        {
            try
            {
                return ToJson(NegPerfilInicio.ListarPerfilInicio());
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los perfiles. " + ex.Message, "danger");
            }
        }

        private string ListaPaginas(dynamic campos)
        {
            try
            {
                return ToJson(NegPerfilInicio.ListarPaginasMenu());
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar las páginas. " + ex.Message, "danger");
            }
        }

        private string GuardarPerfilInicio(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();

            try
            {
                int idPerfil = 0;
                try { idPerfil = Convert.ToInt32(campos["idPerfil"]); }
                catch { idPerfil = 0; }

                string href = "";
                try { href = Convert.ToString(campos["href"]).Trim(); }
                catch { href = ""; }

                int idTipo = 0;
                try { idTipo = Convert.ToInt32(campos["idTipo"]); }
                catch { idTipo = 0; }

                string usuarioRegistro = "";
                try { usuarioRegistro = Convert.ToString(campos["usuarioRegistro"]).Trim(); }
                catch { usuarioRegistro = ""; }

                if (idPerfil <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un perfil.", "warning");
                }
                if (string.IsNullOrWhiteSpace(href))
                {
                    return responseMessage("0", "Debe seleccionar la página de inicio.", "warning");
                }

                respuesta = NegPerfilInicio.GuardarPerfilInicio(idPerfil, href, idTipo, usuarioRegistro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar la configuración. " + ex.Message, "danger");
            }

            return ToJson(respuesta);
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

- [ ] **Step 3: Registrar el handler en el .csproj**

En `ReporteTareas/ReporteTareas.csproj`, junto a la línea `<Content Include="Formulario\AdministrarVentanaAprobacion.ashx" />` agregar:
```xml
    <Content Include="Formulario\AdministrarPerfilInicio.ashx" />
```
Y junto al bloque `<Compile Include="Formulario\AdministrarVentanaAprobacion.ashx.cs">` agregar:
```xml
    <Compile Include="Formulario\AdministrarPerfilInicio.ashx.cs">
      <DependentUpon>AdministrarPerfilInicio.ashx</DependentUpon>
    </Compile>
```

- [ ] **Step 4: Compilar**

Run (PowerShell):
```powershell
$msb = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"; & $msb "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly; "EXIT CODE: $LASTEXITCODE"
```
Expected: `EXIT CODE: 0`.

- [ ] **Step 5: Verificar por HTTP (si la app corre y alcanza la BD)**

Lanzar IIS Express 32-bit (`/path:...\ReporteTareas /port:51037`) y en PowerShell:
```powershell
function Call($body) {
  $url = "http://localhost:51037/Formulario/AdministrarPerfilInicio.ashx"
  $req = [System.Net.HttpWebRequest]::Create($url); $req.Method="POST"; $req.ContentType="application/json; charset=utf-8"
  $b=[System.Text.Encoding]::UTF8.GetBytes($body); $req.ContentLength=$b.Length
  $s=$req.GetRequestStream(); $s.Write($b,0,$b.Length); $s.Close()
  $resp=$req.GetResponse(); $ms=New-Object System.IO.MemoryStream; $resp.GetResponseStream().CopyTo($ms); $resp.Close()
  [System.Text.Encoding]::UTF8.GetString($ms.ToArray())
}
Call '[{"action":"ListaPaginas","parameters":{}}]'
Call '[{"action":"ListaPerfilInicio","parameters":{}}]'
Call '[{"action":"GuardarPerfilInicio","parameters":{"idPerfil":5,"href":"Principal.aspx","idTipo":5,"usuarioRegistro":"test"}}]'
```
Expected: `ListaPaginas` array `{Titulo,Href}`; `ListaPerfilInicio` array de perfiles con tildes correctas; `GuardarPerfilInicio` → `{"estado":"1",...,"mensaje":"Configuracion guardada correctamente."}`.
(Si la app no alcanza la BD desde esta máquina, dejar esta verificación para el entorno del usuario; la compilación y el nivel SQL ya cubren la lógica.)

- [ ] **Step 6: Commit**

```bash
git add "ReporteTareas/Formulario/AdministrarPerfilInicio.ashx" "ReporteTareas/Formulario/AdministrarPerfilInicio.ashx.cs" "ReporteTareas/ReporteTareas.csproj"
git commit -m "feat(handler): AdministrarPerfilInicio.ashx (lista/guardar pagina de inicio por perfil)"
```

---

## Task 4: Frontend (ASPX + designer + JS)

**Files:**
- Create: `ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx`
- Create: `ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx.cs`
- Create: `ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx.designer.cs`
- Create: `ReporteTareas/js/parametrizacionPerfilInicio.js`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes (de Task 3): acciones `ListaPaginas`, `ListaPerfilInicio`, `GuardarPerfilInicio`.

- [ ] **Step 1: Crear el `.aspx`**

Crear `ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx`:

```html
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionPerfilInicio.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionPerfilInicio" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionPerfilInicio.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Parametrización de página de inicio por perfil</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    Página a la que entra cada perfil al iniciar sesión y su "tipo" (Id_Usuario)
                    <div style="display: none">
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                    </div>
                </div>
                <div class="panel-body">
                    <div class="col-lg-12" style="padding: 0px">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4>Perfiles</h4>
                            </div>
                            <div class="panel-body" style="height: 460px; overflow-y: auto; overflow-x: auto;">
                                <div id="datosTablaPerfiles" style="padding: 0px">
                                </div>
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
                    <div class="modal-body" id="MensajeInformativo">
                    </div>
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

- [ ] **Step 2: Crear el code-behind del `.aspx`**

Crear `ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx.cs`:

```csharp
using ReporteTareas.Controles;
using SeguridadAppHelper;
using System;

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionPerfilInicio : System.Web.UI.Page
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

Crear `ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx.designer.cs`:

```csharp
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionPerfilInicio
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

Crear `ReporteTareas/js/parametrizacionPerfilInicio.js` (recordar **UTF-8 con BOM** — ver Step 6):

```javascript
/* ============================================================================
   Pantalla: Parametrizacion de pagina de inicio por perfil
   Handler : AdministrarPerfilInicio.ashx
   ============================================================================ */

var _perfiles = [];
var _paginas = [];

$(document).ready(function () {
    CargarPaginas(function () {
        BuscarPerfiles();
    });
});

/* Llama al handler con el formato [{action, parameters}] */
function PostInicio(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarPerfilInicio.ashx",
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

/* Carga la lista de paginas (MenuDos) para los desplegables */
function CargarPaginas(callback) {
    PostInicio("ListaPaginas", {}, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            _paginas = [];
        } else {
            _paginas = respuesta || [];
        }
        if (typeof callback === "function") { callback(); }
    });
}

/* Lista los perfiles con su configuracion */
function BuscarPerfiles() {
    PostInicio("ListaPerfilInicio", {}, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _perfiles = respuesta || [];
        RenderTablaPerfiles(_perfiles);
    });
}

function OpcionesPagina(hrefSeleccionado) {
    var html = "<option value=''>-- Seleccione --</option>";
    $.each(_paginas, function (i, p) {
        var sel = (p.Href === hrefSeleccionado) ? " selected" : "";
        html += "<option value='" + Escapar(p.Href) + "'" + sel + ">" + Escapar(p.Titulo) + "</option>";
    });
    return html;
}

function RenderTablaPerfiles(lista) {
    var info = "";
    info += "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer'>";
    info += "<thead><tr role='row'>";
    info += "<th style='text-align:center'>Perfil</th>";
    info += "<th>Nombre</th>";
    info += "<th>Página de inicio</th>";
    info += "<th style='text-align:center'>Tipo (Id_Usuario)</th>";
    info += "<th style='text-align:center'>Acción</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='5' style='text-align:center'>No existen perfiles.</td></tr>";
    }

    $.each(lista, function (i, item) {
        info += "<tr role='row'>";
        info += "<td style='text-align:center'>" + Escapar(String(item.IdPerfil)) + "</td>";
        info += "<td>" + Escapar(item.NombrePerfil) + "</td>";
        info += "<td><select id='pag_" + i + "' class='form-control'>" + OpcionesPagina(item.Href) + "</select></td>";
        info += "<td style='text-align:center'><input id='tipo_" + i + "' type='number' class='form-control' style='width:110px;display:inline-block' value='" + Escapar(String(item.IdTipo)) + "' /></td>";
        info += "<td style='text-align:center'><button type='button' class='btn btn-success btn-sm' onclick='GuardarPerfil(" + i + ")'>Guardar</button></td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaPerfiles").html(info);
}

function GuardarPerfil(indice) {
    var item = _perfiles[indice];
    if (item == null) { return; }

    var href = $("#pag_" + indice).val();
    var idTipo = parseInt($("#tipo_" + indice).val(), 10);
    if (isNaN(idTipo)) { idTipo = 0; }

    if (href == null || href === "") {
        MostrarMensaje("Debe seleccionar la página de inicio.", "warning");
        return;
    }

    var usuarioRegistro = $("#ContentPlaceHolder1_txtLoginUsuario").val();

    var parameters = {
        "idPerfil": item.IdPerfil,
        "href": href,
        "idTipo": idTipo,
        "usuarioRegistro": usuarioRegistro
    };

    PostInicio("GuardarPerfilInicio", parameters, function (respuesta) {
        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado == "1") {
            BuscarPerfiles();
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
```

- [ ] **Step 5: Registrar los archivos en el .csproj**

En `ReporteTareas/ReporteTareas.csproj`:
- Junto a `<Content Include="Formulario\ParametrizacionVentanaAprobacion.aspx" />` agregar:
```xml
    <Content Include="Formulario\ParametrizacionPerfilInicio.aspx" />
```
- Junto a `<Content Include="js\parametrizacionVentanaAprobacion.js" />` agregar:
```xml
    <Content Include="js\parametrizacionPerfilInicio.js" />
```
- Junto al bloque `<Compile Include="Formulario\ParametrizacionVentanaAprobacion.aspx.cs">` agregar:
```xml
    <Compile Include="Formulario\ParametrizacionPerfilInicio.aspx.cs">
      <DependentUpon>ParametrizacionPerfilInicio.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\ParametrizacionPerfilInicio.aspx.designer.cs">
      <DependentUpon>ParametrizacionPerfilInicio.aspx</DependentUpon>
    </Compile>
```

- [ ] **Step 6: Verificar/re-agregar el BOM del JS**

Run (PowerShell):
```powershell
$f = (Resolve-Path "ReporteTareas\js\parametrizacionPerfilInicio.js")
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

- [ ] **Step 8: Verificación funcional en el navegador (entorno con BD)**

Con la app corriendo y sesión iniciada, abrir
`http://localhost:51037/Formulario/ParametrizacionPerfilInicio.aspx` y **Ctrl+F5**. Comprobar:
1. Se lista una fila por perfil (con su nombre con tildes correcto).
2. El desplegable de "Página de inicio" trae las pantallas de MenuDos y aparece preseleccionada la actual.
3. Cambiar la página y/o el Tipo de un perfil y **Guardar** → mensaje de éxito y la fila queda con el nuevo valor.

- [ ] **Step 9: Commit**

```bash
git add "ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx" "ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx.cs" "ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx.designer.cs" "ReporteTareas/js/parametrizacionPerfilInicio.js" "ReporteTareas/ReporteTareas.csproj"
git commit -m "feat(ui): pantalla de parametrizacion de pagina de inicio por perfil"
```

---

## Task 5: Registro en el menú (SQL)

**Files:**
- Create: `docs/superpowers/plans/sql/2026-07-25-pagina-inicio-por-perfil-menu.sql`

**Interfaces:**
- Consumes: la pantalla `ParametrizacionPerfilInicio.aspx` (Task 4) debe existir para que el ítem sea navegable.
- Produces: ítem de menú bajo "Manejo de Perfiles" (Id_Menu 20042) habilitado para perfiles 1, 2, 18, 19.

- [ ] **Step 1: Crear el script de menú**

Crear `docs/superpowers/plans/sql/2026-07-25-pagina-inicio-por-perfil-menu.sql`:

```sql
/* ============================================================================
   Registro en menu de la pantalla "Parametrizacion de pagina de inicio por perfil"
   Base: ReporTarea  — Padre: 20042 (Manejo de Perfiles)  — Perfiles: 1,2,18,19
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

DECLARE @Titulo   VARCHAR(200) = 'Pagina de Inicio por Perfil';
DECLARE @Href     VARCHAR(200) = 'ParametrizacionPerfilInicio.aspx';
DECLARE @Icono    VARCHAR(100) = 'fa fa-sign-in';
DECLARE @Padre    INT = 20042;
DECLARE @IdMenu   INT;

/* Insertar en MenuDos solo si no existe (por Href) */
SELECT @IdMenu = Id_Menu FROM dbo.MenuDos WHERE Href = @Href;

IF @IdMenu IS NULL
BEGIN
    INSERT INTO dbo.MenuDos (Titulo, Href, Class_Icon, Id_MenuPadre)
    VALUES (@Titulo, @Href, @Icono, @Padre);
    SET @IdMenu = SCOPE_IDENTITY();
END

/* Habilitar el menu (y su padre) para los perfiles admin, sin duplicar */
DECLARE @Perfiles TABLE (IdPerfil INT);
INSERT INTO @Perfiles (IdPerfil) VALUES (1),(2),(18),(19);

INSERT INTO dbo.PerfilMenu (IdPerfil, id_Menu, Estado)
SELECT p.IdPerfil, m.id_Menu, 0
FROM @Perfiles p
CROSS JOIN (SELECT @IdMenu AS id_Menu UNION SELECT @Padre) m
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PerfilMenu pm
    WHERE pm.IdPerfil = p.IdPerfil AND pm.id_Menu = m.id_Menu
);

/* Asegurar Estado=0 (activo) del item y su padre para esos perfiles */
UPDATE pm SET pm.Estado = 0
FROM dbo.PerfilMenu pm
JOIN @Perfiles p ON p.IdPerfil = pm.IdPerfil
WHERE pm.id_Menu IN (@IdMenu, @Padre);

SELECT Id_Menu = @IdMenu, Href = @Href;
```

> Nota: los nombres reales de columnas de `MenuDos` (`Id_Menu, Titulo, Href, Class_Icon, Id_MenuPadre`) y `PerfilMenu` (`IdPerfil, id_Menu, Estado`) están verificados. Ajustar `@Icono`/`@Titulo` si se desea otro texto.

- [ ] **Step 2: Ejecutar el script**

Run:
```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas" && sqlcmd -S "tcp:192.168.11.14,1433" -U sa -P "CAfKsUBnD0s" -d ReporTarea -b -i "docs/superpowers/plans/sql/2026-07-25-pagina-inicio-por-perfil-menu.sql"
```
Expected: devuelve el `Id_Menu` insertado; sin errores.

- [ ] **Step 3: Verificar el registro**

Run:
```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas" && SQL="tcp:192.168.11.14,1433"
sqlcmd -S "$SQL" -U sa -P "CAfKsUBnD0s" -d ReporTarea -W -s"|" -Q "SET NOCOUNT ON; SELECT m.Id_Menu, m.Titulo, m.Id_MenuPadre, m.Href FROM dbo.MenuDos m WHERE m.Href='ParametrizacionPerfilInicio.aspx'; SELECT pm.IdPerfil, pm.id_Menu, pm.Estado FROM dbo.PerfilMenu pm JOIN dbo.MenuDos m ON m.Id_Menu=pm.id_Menu WHERE m.Href='ParametrizacionPerfilInicio.aspx' ORDER BY pm.IdPerfil;"
```
Expected: 1 fila en MenuDos con `Id_MenuPadre=20042`; 4 filas en PerfilMenu (perfiles 1,2,18,19) con `Estado=0`.

- [ ] **Step 4: Commit**

```bash
git add "docs/superpowers/plans/sql/2026-07-25-pagina-inicio-por-perfil-menu.sql"
git commit -m "feat(menu): registro de ParametrizacionPerfilInicio (perfiles 1,2,18,19)"
```

---

## Task 6: Refactor del Login

**Files:**
- Modify: `ReporteTareas/Formulario/Login.aspx.cs` (dos bloques: ~129-173 y ~286-307)

**Interfaces:**
- Consumes (de Task 2): `NegPerfilInicio.ObtenerPerfilInicio(int idPerfil)` → `EntPerfilInicio` con `Href` e `IdTipo`.

- [ ] **Step 1: Asegurar el `using`**

En `ReporteTareas/Formulario/Login.aspx.cs`, verificar que esté `using CapaNegocio;` (ya se usa `NegUsuario`, así que normalmente está). Si falta, agregarlo.

- [ ] **Step 2: Reemplazar el primer bloque (ruta principal, ~líneas 129-173)**

Reemplazar todo el bloque desde `if (objUsuario.Id_Perfil == 2 || objUsuario.Id_Perfil == 3)` hasta `Response.Redirect("Principal.aspx");` (inclusive) por:

```csharp
                                var inicio = NegPerfilInicio.ObtenerPerfilInicio(objUsuario.Id_Perfil);
                                Session["Id_Usuario"] = inicio.IdTipo;
                                Response.Redirect(inicio.Href);
```

(La línea previa `Session["Id_Perfil"] = objUsuario.Id_Perfil;` se conserva.)

- [ ] **Step 3: Reemplazar el segundo bloque (ruta secundaria, ~líneas 288-307)**

Reemplazar el bloque desde `if (objUsuario.Id_Perfil == 2 || objUsuario.Id_Perfil == 3)` hasta `Response.Redirect("Principal.aspx");` (inclusive) por el mismo patrón (misma indentación del bloque, un nivel menos):

```csharp
                        var inicio = NegPerfilInicio.ObtenerPerfilInicio(objUsuario.Id_Perfil);
                        Session["Id_Usuario"] = inicio.IdTipo;
                        Response.Redirect(inicio.Href);
```

> Ambos bloques usan `objUsuario.Id_Perfil`, que ya está en alcance en cada punto. Nombrar la variable `inicio` en los dos está bien porque están en ramas `if/else` separadas; si el compilador reclama por redefinición en el mismo alcance, renombrar el segundo a `inicioSec`.

- [ ] **Step 4: Compilar**

Run (PowerShell):
```powershell
$msb = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"; & $msb "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly; "EXIT CODE: $LASTEXITCODE"
```
Expected: `EXIT CODE: 0`.

- [ ] **Step 5: Verificación del login (entorno con BD)**

Con la app corriendo:
1. Iniciar sesión con un usuario de un perfil configurado (p. ej. perfil 5 apuntando a `Principal.aspx`) → entra a la página configurada y `Session["Id_Usuario"]` tiene el `IdTipo` configurado.
2. (Opcional) Configurar temporalmente un perfil a otra página desde la pantalla nueva, iniciar sesión con ese perfil y confirmar la redirección.
3. Iniciar sesión con un usuario de un perfil sin configuración → entra a `Principal.aspx` con `Id_Usuario=0` (fallback).

- [ ] **Step 6: Commit**

```bash
git add "ReporteTareas/Formulario/Login.aspx.cs"
git commit -m "refactor(login): pagina de inicio y tipo por perfil desde tabla (elimina if quemados)"
```

---

## Self-Review (cobertura del spec)

- **Tabla `RTA_PerfilInicio` + seed** → Task 1, Step 1. ✅
- **SP Listar / Guardar / Obtener / ListarPaginasMenu** → Task 1, Step 1. ✅
- **Entidad `EntPerfilInicio`** → Task 2, Step 1. ✅
- **Dao (4 métodos) + fail-safe en Obtener** → Task 2, Step 2. ✅
- **Negocio (pass-through)** → Task 2, Step 3. ✅
- **Handler con `ListaPerfilInicio`/`ListaPaginas`/`GuardarPerfilInicio` + UTF-8** → Task 3. ✅
- **Pantalla: grilla perfil / desplegable MenuDos / Tipo / guardar por fila** → Task 4. ✅
- **Fallback a MenuDos para el desplegable** → Task 1 (SP) + Task 4 (render). ✅
- **BOM del JS / handler UTF-8** → Task 4, Step 6 + Global Constraints. ✅
- **Registro en menú (MenuDos+PerfilMenu, perfiles 1,2,18,19)** → Task 5. ✅
- **Refactor del Login (dos bloques) con fallback** → Task 6. ✅
- **`usuarioRegistro` desde `txtLoginUsuario`** → Task 4, Step 4 (GuardarPerfil). ✅

Sin placeholders. Firmas consistentes entre tareas: `ObtenerPerfilInicio(int)→EntPerfilInicio`, `GuardarPerfilInicio(int,string,int,string)→EntRespuesta`, `ListarPerfilInicio()→List<EntPerfilInicio>`, `ListarPaginasMenu()→List<EntMenuDos>`; acción `GuardarPerfilInicio` con `{idPerfil, href, idTipo, usuarioRegistro}`.
```
