# Ventana de aprobación por jefe inmediato — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Permitir configurar por jefe inmediato una ventana de fechas (desde–hasta) y bloquear la aprobación de tareas de sus colaboradores fuera de esa ventana.

**Architecture:** Se sigue el patrón existente en 3 capas (CapaEntidad / CapaDato / CapaNegocio) + WebForms con handler `.ashx`. Una tabla nueva (`RTA_VentanaAprobacionJefe`) y 3 stored procedures `Sp_RTA_*`. La pantalla de administración clona el patrón de `ParametrizacionHorarioUsuario`. La aplicación del bloqueo son 2 bloques mínimos en `AdministrarTarea.ashx.cs`, reutilizando el email del jefe (`usuario.E_Mail`) ya disponible ahí. Toda conexión reutiliza `DaoReporTareaAranda.conectar()`.

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.6.1), C#, SQL Server, jQuery/Bootstrap, JavaScriptSerializer.

## Global Constraints

- **No escribir la credencial de BD en ningún archivo nuevo.** Toda conexión usa `new CapaDato.DaoReporTareaAranda().conectar()` (producción `192.168.11.14 / ReporTarea`), igual que `DaoUsuarioHorario`.
- **Restricción opt-in:** jefe sin ventana registrada → SE PERMITE aprobar.
- **Una sola ventana por jefe** (upsert por `MailJefe`).
- **`MailJefe` = `R_Usuarios.MailCodJefeInm` = email del jefe.** El jefe logueado se resuelve por `usuario.E_Mail`.
- **Menú (semántica invertida):** en `PerfilMenu`, `Estado='0'` MUESTRA y `'1'` OCULTA; una opción hija solo se ve si su grupo padre (`Id_MenuPadre=0`) también tiene `Estado='0'` para ese perfil. Perfiles objetivo: **1, 2, 18, 19**.
- **Solución/compilación:** `ReporteTareas.sln` en la raíz. Compilar en Visual Studio (Ctrl+Shift+B sobre la solución) o `msbuild ReporteTareas.sln /p:Configuration=Debug` desde un *Developer Command Prompt*. "Verificar compila" = build sin errores.
- **Serialización JSON en handlers:** usar el `ToJson` local (JavaScriptSerializer, escapa no-ASCII) como en `AdministrarHorarioUsuario.ashx.cs`. En `AdministrarTarea.ashx.cs` usar `.SerializaToJson()` (extensión de `JSONHelper`, ya importada en ese archivo).
- **Codificación de archivos:** mantener UTF-8; el proyecto usa tildes/ñ en textos.

---

## Task 1: Base de datos — tabla y stored procedures

**Files:**
- Create: `docs/superpowers/plans/sql/2026-07-20-ventana-aprobacion-jefe.sql`

**Interfaces:**
- Produces (contratos que consumen las capas C#):
  - `Sp_RTA_ListarJefesVentanaAprobacion(@Filtro VARCHAR(150))` → columnas: `MailJefe`, `NombreJefe`, `NumColaboradores` (int), `FechaDesde` (varchar 'yyyy-MM-dd' o NULL), `FechaHasta` (idem), `TieneVentana` (0/1).
  - `Sp_RTA_GuardarVentanaAprobacionJefe(@MailJefe, @FechaDesde DATE, @FechaHasta DATE, @UsuarioRegistro)` → columnas: `Respuestas` (int, >0 éxito), `Mensaje` (varchar).
  - `Sp_RTA_ValidarVentanaAprobacionJefe(@MailJefe VARCHAR(150), @Fecha DATE)` → columnas: `PuedeAprobar` (bit), `Mensaje` (varchar).

- [ ] **Step 1: Crear el script SQL**

Crear `docs/superpowers/plans/sql/2026-07-20-ventana-aprobacion-jefe.sql`:

```sql
/* ============================================================================
   Ventana de aprobación por jefe inmediato — objetos de BD
   Base: ReporTarea
   ============================================================================ */

/* ---------- 1) Tabla ---------- */
IF OBJECT_ID('dbo.RTA_VentanaAprobacionJefe','U') IS NULL
BEGIN
    CREATE TABLE dbo.RTA_VentanaAprobacionJefe
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        MailJefe        VARCHAR(150)      NOT NULL,
        FechaDesde      DATE              NOT NULL,
        FechaHasta      DATE              NOT NULL,
        Estado          BIT               NOT NULL CONSTRAINT DF_RTA_VentAprob_Estado   DEFAULT(1),
        UsuarioRegistro VARCHAR(100)      NULL,
        FechaRegistro   DATETIME          NOT NULL CONSTRAINT DF_RTA_VentAprob_FechaReg DEFAULT(GETDATE()),
        CONSTRAINT PK_RTA_VentanaAprobacionJefe PRIMARY KEY (Id),
        CONSTRAINT UQ_RTA_VentAprob_MailJefe UNIQUE (MailJefe)
    );
END
GO

/* ---------- 2) SP Listar jefes con su ventana ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarJefesVentanaAprobacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarJefesVentanaAprobacion;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarJefesVentanaAprobacion
    @Filtro VARCHAR(150) = ''
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Jefes AS (
        SELECT MailJefe = LTRIM(RTRIM(MailCodJefeInm)),
               NumColaboradores = COUNT(*)
        FROM dbo.R_Usuarios
        WHERE ISNULL(MailCodJefeInm,'') <> ''
        GROUP BY LTRIM(RTRIM(MailCodJefeInm))
    )
    SELECT
        j.MailJefe,
        NombreJefe        = ISNULL(u.Nom_Usuario, j.MailJefe),
        j.NumColaboradores,
        FechaDesde        = CONVERT(VARCHAR(10), v.FechaDesde, 23),
        FechaHasta        = CONVERT(VARCHAR(10), v.FechaHasta, 23),
        TieneVentana      = CASE WHEN v.Id IS NOT NULL THEN 1 ELSE 0 END
    FROM Jefes j
    OUTER APPLY (
        SELECT TOP 1 ru.Nom_Usuario
        FROM dbo.R_Usuarios ru
        WHERE ru.E_Mail = j.MailJefe
    ) u
    LEFT JOIN dbo.RTA_VentanaAprobacionJefe v
        ON v.MailJefe = j.MailJefe AND v.Estado = 1
    WHERE (@Filtro = ''
           OR j.MailJefe LIKE '%' + @Filtro + '%'
           OR ISNULL(u.Nom_Usuario,'') LIKE '%' + @Filtro + '%')
    ORDER BY NombreJefe;
END
GO

/* ---------- 3) SP Guardar (upsert una ventana por jefe) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_GuardarVentanaAprobacionJefe','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarVentanaAprobacionJefe;
GO
CREATE PROCEDURE dbo.Sp_RTA_GuardarVentanaAprobacionJefe
    @MailJefe        VARCHAR(150),
    @FechaDesde      DATE,
    @FechaHasta      DATE,
    @UsuarioRegistro VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';
    SET @MailJefe = LTRIM(RTRIM(@MailJefe));

    IF ISNULL(@MailJefe,'') = ''
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Debe indicar el jefe.'; RETURN;
    END
    IF @FechaHasta < @FechaDesde
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'La fecha hasta no puede ser menor que la fecha desde.'; RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.RTA_VentanaAprobacionJefe WHERE MailJefe = @MailJefe)
    BEGIN
        UPDATE dbo.RTA_VentanaAprobacionJefe
           SET FechaDesde = @FechaDesde,
               FechaHasta = @FechaHasta,
               Estado = 1,
               UsuarioRegistro = @UsuarioRegistro,
               FechaRegistro = GETDATE()
         WHERE MailJefe = @MailJefe;
        SET @Respuestas = 1; SET @Mensaje = 'Ventana de aprobación actualizada correctamente.';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.RTA_VentanaAprobacionJefe (MailJefe, FechaDesde, FechaHasta, Estado, UsuarioRegistro, FechaRegistro)
        VALUES (@MailJefe, @FechaDesde, @FechaHasta, 1, @UsuarioRegistro, GETDATE());
        SET @Respuestas = 1; SET @Mensaje = 'Ventana de aprobación registrada correctamente.';
    END

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO

/* ---------- 4) SP Validar (usado en la aprobación) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ValidarVentanaAprobacionJefe','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ValidarVentanaAprobacionJefe;
GO
CREATE PROCEDURE dbo.Sp_RTA_ValidarVentanaAprobacionJefe
    @MailJefe VARCHAR(150),
    @Fecha    DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @PuedeAprobar BIT = 1, @Mensaje VARCHAR(400) = '';
    DECLARE @Desde DATE, @Hasta DATE;
    SET @MailJefe = LTRIM(RTRIM(@MailJefe));

    SELECT TOP 1 @Desde = FechaDesde, @Hasta = FechaHasta
    FROM dbo.RTA_VentanaAprobacionJefe
    WHERE MailJefe = @MailJefe AND Estado = 1;

    IF @Desde IS NULL
        SET @PuedeAprobar = 1;                 -- sin ventana => permitir (opt-in)
    ELSE IF @Fecha BETWEEN @Desde AND @Hasta
        SET @PuedeAprobar = 1;
    ELSE
    BEGIN
        SET @PuedeAprobar = 0;
        SET @Mensaje = 'No puede aprobar en esta fecha. Su ventana de aprobación es del '
            + CONVERT(VARCHAR(10), @Desde, 103) + ' al ' + CONVERT(VARCHAR(10), @Hasta, 103) + '.';
    END

    SELECT PuedeAprobar = @PuedeAprobar, Mensaje = @Mensaje;
END
GO
```

- [ ] **Step 2: Ejecutar el script en la BD**

Abrir SSMS conectado a `ReporTarea` (idealmente primero un entorno de prueba: `ReporTareaTest` o `ReporTareaPreProd`; luego producción). Ejecutar el script completo. Esperado: `Commands completed successfully`.

- [ ] **Step 3: Verificar objetos y comportamiento**

Ejecutar en SSMS:

```sql
-- 3.1 Lista jefes (debe traer ~22 filas, TieneVentana=0 al inicio)
EXEC dbo.Sp_RTA_ListarJefesVentanaAprobacion @Filtro = '';

-- 3.2 Guardar una ventana de prueba para un jefe real
EXEC dbo.Sp_RTA_GuardarVentanaAprobacionJefe
     @MailJefe='mmejia@dos.com.ec', @FechaDesde='2026-08-01', @FechaHasta='2026-08-05', @UsuarioRegistro='PRUEBA';
-- Esperado: Respuestas=1

-- 3.3 Validar dentro y fuera de la ventana
EXEC dbo.Sp_RTA_ValidarVentanaAprobacionJefe @MailJefe='mmejia@dos.com.ec', @Fecha='2026-08-03'; -- PuedeAprobar=1
EXEC dbo.Sp_RTA_ValidarVentanaAprobacionJefe @MailJefe='mmejia@dos.com.ec', @Fecha='2026-08-20'; -- PuedeAprobar=0 + mensaje
EXEC dbo.Sp_RTA_ValidarVentanaAprobacionJefe @MailJefe='correo.sin.ventana@x.com', @Fecha='2026-08-20'; -- PuedeAprobar=1

-- 3.4 Limpiar la ventana de prueba
DELETE FROM dbo.RTA_VentanaAprobacionJefe WHERE UsuarioRegistro='PRUEBA';
```

Confirmar que cada resultado coincide con lo indicado en los comentarios.

- [ ] **Step 4: Commit**

```bash
git add "docs/superpowers/plans/sql/2026-07-20-ventana-aprobacion-jefe.sql"
git commit -m "feat(bd): tabla y SPs de ventana de aprobacion por jefe"
```

---

## Task 2: Capas Entidad / Dato / Negocio

**Files:**
- Create: `CapaEntidad/EntVentanaAprobacionJefe.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj` (agregar `<Compile Include>` junto a `EntUsuarioHorario.cs`, línea ~88)
- Create: `CapaDato/DaoVentanaAprobacion.cs`
- Modify: `CapaDato/CapaDato.csproj` (agregar `<Compile Include>` junto a `DaoUsuarioHorario.cs`, línea ~87)
- Create: `CapaNegocio/NegVentanaAprobacion.cs`
- Modify: `CapaNegocio/CapaNegocio.csproj` (agregar `<Compile Include>` junto a `NegUsuarioHorario.cs`, línea ~207)

**Interfaces:**
- Consumes: `CapaEntidad.EntRespuesta`; `CapaDato.DaoReporTareaAranda.conectar()`; SPs de Task 1.
- Produces:
  - `EntVentanaAprobacionJefe { string MailJefe; string NombreJefe; int NumColaboradores; string FechaDesde; string FechaHasta; int TieneVentana; }`
  - `NegVentanaAprobacion.ListarJefes(string filtro) : List<EntVentanaAprobacionJefe>`
  - `NegVentanaAprobacion.GuardarVentana(string mailJefe, string fechaDesde, string fechaHasta, string usuarioRegistro) : EntRespuesta`
  - `NegVentanaAprobacion.ValidarVentana(string mailJefe, DateTime fecha) : EntRespuesta`

- [ ] **Step 1: Crear la entidad `EntVentanaAprobacionJefe.cs`**

```csharp
namespace CapaEntidad
{
    public class EntVentanaAprobacionJefe
    {
        public string MailJefe { get; set; }
        public string NombreJefe { get; set; }
        public int NumColaboradores { get; set; }
        public string FechaDesde { get; set; }
        public string FechaHasta { get; set; }
        public int TieneVentana { get; set; }
    }
}
```

- [ ] **Step 2: Registrar la entidad en `CapaEntidad.csproj`**

Junto a la línea `<Compile Include="EntUsuarioHorario.cs" />` agregar:

```xml
    <Compile Include="EntVentanaAprobacionJefe.cs" />
```

- [ ] **Step 3: Crear el DAO `DaoVentanaAprobacion.cs`**

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoVentanaAprobacion
    {
        /// <summary>Lista los jefes inmediatos (MailCodJefeInm) con su ventana vigente.</summary>
        public static List<EntVentanaAprobacionJefe> ListarJefes(string filtro)
        {
            List<EntVentanaAprobacionJefe> lista = new List<EntVentanaAprobacionJefe>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarJefesVentanaAprobacion", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 150).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntVentanaAprobacionJefe()
                        {
                            MailJefe = dr["MailJefe"].ToString(),
                            NombreJefe = dr["NombreJefe"].ToString(),
                            NumColaboradores = Convert.ToInt32(dr["NumColaboradores"].ToString()),
                            FechaDesde = dr["FechaDesde"] == DBNull.Value ? "" : dr["FechaDesde"].ToString(),
                            FechaHasta = dr["FechaHasta"] == DBNull.Value ? "" : dr["FechaHasta"].ToString(),
                            TieneVentana = Convert.ToInt32(dr["TieneVentana"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Inserta o actualiza (una por jefe) la ventana de aprobación.</summary>
        public static EntRespuesta GuardarVentana(string mailJefe, string fechaDesde, string fechaHasta, string usuarioRegistro)
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
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarVentanaAprobacionJefe", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@MailJefe", SqlDbType.VarChar, 150).Value = mailJefe ?? string.Empty;
                    cmd.Parameters.Add("@FechaDesde", SqlDbType.Date).Value = Convert.ToDateTime(fechaDesde);
                    cmd.Parameters.Add("@FechaHasta", SqlDbType.Date).Value = Convert.ToDateTime(fechaHasta);
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
                respuesta.mensaje = "Ocurrió un error al guardar la ventana. Detalle: " + ex.Message;
            }

            return respuesta;
        }

        /// <summary>
        /// Valida si el jefe puede aprobar en la fecha dada.
        /// Sin ventana => permite. Ante error inesperado => permite (fail-open, opt-in).
        /// </summary>
        public static EntRespuesta ValidarVentana(string mailJefe, DateTime fecha)
        {
            EntRespuesta respuesta = new EntRespuesta()
            {
                estado = "1",
                resultado = "1",
                tipoMensaje = "success",
                mensaje = ""
            };

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_ValidarVentanaAprobacionJefe", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@MailJefe", SqlDbType.VarChar, 150).Value = mailJefe ?? string.Empty;
                    cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha.Date;

                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            bool puedeAprobar = Convert.ToBoolean(dr["PuedeAprobar"]);
                            if (puedeAprobar)
                            {
                                respuesta.estado = "1";
                                respuesta.tipoMensaje = "success";
                                respuesta.mensaje = "";
                            }
                            else
                            {
                                respuesta.estado = "0";
                                respuesta.tipoMensaje = "warning";
                                respuesta.mensaje = dr["Mensaje"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fail-open: un problema al validar no debe frenar la operación de aprobación.
                respuesta.estado = "1";
                respuesta.resultado = "1";
                respuesta.tipoMensaje = "success";
                respuesta.mensaje = "";
            }

            return respuesta;
        }
    }
}
```

- [ ] **Step 4: Registrar el DAO en `CapaDato.csproj`**

Junto a la línea `<Compile Include="DaoUsuarioHorario.cs" />` agregar:

```xml
    <Compile Include="DaoVentanaAprobacion.cs" />
```

- [ ] **Step 5: Crear la capa de negocio `NegVentanaAprobacion.cs`**

```csharp
using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegVentanaAprobacion
    {
        public static List<EntVentanaAprobacionJefe> ListarJefes(string filtro)
        {
            return DaoVentanaAprobacion.ListarJefes(filtro);
        }

        public static EntRespuesta GuardarVentana(string mailJefe, string fechaDesde, string fechaHasta, string usuarioRegistro)
        {
            return DaoVentanaAprobacion.GuardarVentana(mailJefe, fechaDesde, fechaHasta, usuarioRegistro);
        }

        public static EntRespuesta ValidarVentana(string mailJefe, DateTime fecha)
        {
            return DaoVentanaAprobacion.ValidarVentana(mailJefe, fecha);
        }
    }
}
```

- [ ] **Step 6: Registrar la clase en `CapaNegocio.csproj`**

Junto a la línea `<Compile Include="NegUsuarioHorario.cs" />` agregar:

```xml
    <Compile Include="NegVentanaAprobacion.cs" />
```

- [ ] **Step 7: Compilar**

Compilar la solución (Ctrl+Shift+B) o `msbuild ReporteTareas.sln /p:Configuration=Debug`.
Esperado: build sin errores; los 3 proyectos (CapaEntidad, CapaDato, CapaNegocio) compilan.

- [ ] **Step 8: Commit**

```bash
git add "CapaEntidad/EntVentanaAprobacionJefe.cs" "CapaEntidad/CapaEntidad.csproj" "CapaDato/DaoVentanaAprobacion.cs" "CapaDato/CapaDato.csproj" "CapaNegocio/NegVentanaAprobacion.cs" "CapaNegocio/CapaNegocio.csproj"
git commit -m "feat(capas): entidad, DAO y negocio de ventana de aprobacion"
```

---

## Task 3: Handler `AdministrarVentanaAprobacion.ashx`

**Files:**
- Create: `ReporteTareas/Formulario/AdministrarVentanaAprobacion.ashx`
- Create: `ReporteTareas/Formulario/AdministrarVentanaAprobacion.ashx.cs`
- Modify: `ReporteTareas/ReporteTareas.csproj` (agregar `<Content Include>` del `.ashx` y `<Compile Include>` del `.ashx.cs` con `DependentUpon`, junto a los de `AdministrarHorarioUsuario`, líneas ~1292 y ~1425)

**Interfaces:**
- Consumes: `CapaNegocio.NegVentanaAprobacion.*` (Task 2); `CapaEntidad.EntRespuesta`.
- Produces (contrato HTTP para el JS de Task 4): POST JSON `[{ "action": "...", "parameters": {...} }]` a `AdministrarVentanaAprobacion.ashx`. Acciones:
  - `ListaJefes` con `{ "filtro": "..." }` → array JSON de jefes.
  - `GuardarVentana` con `{ "mailJefe", "fechaDesde", "fechaHasta", "usuarioRegistro" }` → `EntRespuesta` JSON.

- [ ] **Step 1: Crear el markup `AdministrarVentanaAprobacion.ashx`**

```
<%@ WebHandler Language="C#" CodeBehind="AdministrarVentanaAprobacion.ashx.cs" Class="JsonJQueryNetVentanaAprobacion.AdministrarVentanaAprobacion" %>
```

- [ ] **Step 2: Crear el code-behind `AdministrarVentanaAprobacion.ashx.cs`**

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetVentanaAprobacion
{
    /// <summary>
    /// Handler de la pantalla "Parametrización de ventana de aprobación por jefe".
    /// Acciones: ListaJefes, GuardarVentana.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarVentanaAprobacion : IHttpHandler
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

                if (Action == "ListaJefes")
                {
                    existAction = true;
                    responseAction.Append(ListaJefes(parameters));
                }

                if (Action == "GuardarVentana")
                {
                    existAction = true;
                    responseAction.Append(GuardarVentana(parameters));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }

            context.Response.ContentType = "application/json";
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        private string ListaJefes(dynamic campos)
        {
            try
            {
                string filtro = "";
                try { filtro = Convert.ToString(campos["filtro"]); }
                catch { filtro = ""; }

                return ToJson(NegVentanaAprobacion.ListarJefes(filtro));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los jefes. " + ex.Message, "danger");
            }
        }

        private string GuardarVentana(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();

            try
            {
                string mailJefe = Convert.ToString(campos["mailJefe"]).Trim();
                string fechaDesde = Convert.ToString(campos["fechaDesde"]).Trim();
                string fechaHasta = Convert.ToString(campos["fechaHasta"]).Trim();

                string usuarioRegistro = "";
                try { usuarioRegistro = Convert.ToString(campos["usuarioRegistro"]).Trim(); }
                catch { usuarioRegistro = ""; }

                if (string.IsNullOrWhiteSpace(mailJefe))
                {
                    return responseMessage("0", "Debe seleccionar un jefe.", "warning");
                }
                if (string.IsNullOrWhiteSpace(fechaDesde) || string.IsNullOrWhiteSpace(fechaHasta))
                {
                    return responseMessage("0", "Debe indicar la fecha desde y la fecha hasta.", "warning");
                }

                respuesta = NegVentanaAprobacion.GuardarVentana(mailJefe, fechaDesde, fechaHasta, usuarioRegistro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar la ventana. " + ex.Message, "danger");
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

        /// <summary>
        /// Serializa a JSON escapando no-ASCII (tildes, ñ) como \uXXXX,
        /// mismo criterio que AdministrarHorarioUsuario.ashx.cs.
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

- [ ] **Step 3: Registrar el handler en `ReporteTareas.csproj`**

Junto a `<Content Include="Formulario\AdministrarHorarioUsuario.ashx" />` agregar:

```xml
    <Content Include="Formulario\AdministrarVentanaAprobacion.ashx" />
```

Junto al bloque `<Compile Include="Formulario\AdministrarHorarioUsuario.ashx.cs">...` agregar:

```xml
    <Compile Include="Formulario\AdministrarVentanaAprobacion.ashx.cs">
      <DependentUpon>AdministrarVentanaAprobacion.ashx</DependentUpon>
    </Compile>
```

- [ ] **Step 4: Compilar**

Ctrl+Shift+B / `msbuild`. Esperado: build sin errores.

- [ ] **Step 5: Commit**

```bash
git add "ReporteTareas/Formulario/AdministrarVentanaAprobacion.ashx" "ReporteTareas/Formulario/AdministrarVentanaAprobacion.ashx.cs" "ReporteTareas/ReporteTareas.csproj"
git commit -m "feat(handler): AdministrarVentanaAprobacion.ashx (ListaJefes, GuardarVentana)"
```

---

## Task 4: Pantalla `ParametrizacionVentanaAprobacion` (aspx + designer + code-behind + JS)

**Files:**
- Create: `ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx`
- Create: `ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx.cs`
- Create: `ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx.designer.cs`
- Create: `ReporteTareas/js/parametrizacionVentanaAprobacion.js`
- Modify: `ReporteTareas/ReporteTareas.csproj` (Content del `.aspx` y del `.js`; Compile del `.aspx.cs` + `.aspx.designer.cs` con `DependentUpon`)

**Interfaces:**
- Consumes: handler `AdministrarVentanaAprobacion.ashx` (Task 3).
- Produces: URL navegable `Formulario/ParametrizacionVentanaAprobacion.aspx` (registrada en menú en Task 6).

- [ ] **Step 1: Crear el `.aspx`**

```aspx
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionVentanaAprobacion.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionVentanaAprobacion" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionVentanaAprobacion.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Parametrización de ventana de aprobación por jefe</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    Ventana en la que cada jefe inmediato puede aprobar las actividades de sus colaboradores
                    <div style="display: none">
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                    </div>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-5">
                            <label>Buscar jefe (nombre o correo):</label>
                            <input type="text" class="form-control" id="txtBuscar" placeholder="Escriba para filtrar..." onkeypress="if(event.keyCode==13){BuscarJefes();return false;}">
                        </div>
                        <div class="form-group col-lg-3" style="padding-top: 25px">
                            <button id="btnBuscar" onclick="BuscarJefes()" type="button" class="btn btn-primary">Buscar</button>
                            <button id="btnRefrescar" onclick="LimpiarBusqueda()" type="button" class="btn btn-default">Mostrar todos</button>
                        </div>
                    </div>

                    <div class="col-lg-12" style="padding: 0px">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 id="listTitleLabel">Jefes inmediatos y su ventana de aprobación</h4>
                            </div>
                            <div class="panel-body" style="height: 430px; overflow-y: auto; overflow-x: auto;">
                                <div id="datosTablaJefes" style="padding: 0px">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Asignar Ventana -->
        <div class="modal fade" id="modalAsignar" tabindex="-1" role="dialog" aria-labelledby="modalAsignarLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalAsignarLabel">Asignar ventana de aprobación</h4>
                    </div>
                    <div class="modal-body">
                        <input type="hidden" id="txtMailJefeSel" />
                        <div class="form-group col-lg-12">
                            <label>Jefe:</label>
                            <input type="text" class="form-control" id="txtNombreJefeSel" disabled />
                        </div>
                        <div class="form-group col-lg-12">
                            <label>Ventana actual:</label>
                            <input type="text" class="form-control" id="txtVentanaActualSel" disabled />
                        </div>
                        <div class="form-group col-lg-6">
                            <label>Fecha desde:</label>
                            <input type="date" class="form-control" id="txtFechaDesde" />
                        </div>
                        <div class="form-group col-lg-6">
                            <label>Fecha hasta:</label>
                            <input type="date" class="form-control" id="txtFechaHasta" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="GuardarVentana()" class="btn btn-primary" id="btnGuardarVentana">Guardar ventana</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->

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

- [ ] **Step 2: Crear el `.aspx.designer.cs`**

```csharp
//------------------------------------------------------------------------------
// <generado automáticamente>
//     Este código fue generado por una herramienta.
// </generado automáticamente>
//------------------------------------------------------------------------------

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionVentanaAprobacion
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

- [ ] **Step 3: Crear el `.aspx.cs`**

```csharp
using ReporteTareas.Controles;
using SeguridadAppHelper;
using System;

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionVentanaAprobacion : System.Web.UI.Page
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

- [ ] **Step 4: Crear el JS `parametrizacionVentanaAprobacion.js`**

```javascript
/* ============================================================================
   Pantalla: Parametrizacion de ventana de aprobacion por jefe
   Handler : AdministrarVentanaAprobacion.ashx
   ============================================================================ */

var _jefes = [];

$(document).ready(function () {
    BuscarJefes();
});

/* Llama al handler con el formato [{action, parameters}] */
function PostVentana(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarVentanaAprobacion.ashx",
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

/* Lista jefes aplicando el filtro del cuadro de busqueda */
function BuscarJefes() {
    var filtro = $("#txtBuscar").val();

    PostVentana("ListaJefes", { "filtro": filtro }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        _jefes = respuesta || [];
        RenderTablaJefes(_jefes);
    });
}

function LimpiarBusqueda() {
    $("#txtBuscar").val("");
    BuscarJefes();
}

function RenderTablaJefes(lista) {
    var info = "";
    info += "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer'>";
    info += "<thead><tr role='row'>";
    info += "<th style='text-align:center'>Asignar</th>";
    info += "<th>Jefe</th>";
    info += "<th>Correo</th>";
    info += "<th style='text-align:center'>Colaboradores</th>";
    info += "<th>Ventana de aprobación</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='5' style='text-align:center'>No existen jefes para esta búsqueda.</td></tr>";
    }

    $.each(lista, function (i, item) {
        var ventana = (item.TieneVentana == 1)
            ? (Escapar(item.FechaDesde) + " a " + Escapar(item.FechaHasta))
            : "<span class='label label-default'>Sin ventana</span>";

        info += "<tr role='row'>";
        info += "<td style='text-align:center'>";
        info += "<i class='fa fa-hand-o-right' title='Asignar ventana' style='cursor:pointer' onclick='AbrirAsignar(" + i + ")'></i>";
        info += "</td>";
        info += "<td>" + Escapar(item.NombreJefe) + "</td>";
        info += "<td>" + Escapar(item.MailJefe) + "</td>";
        info += "<td style='text-align:center'>" + Escapar(String(item.NumColaboradores)) + "</td>";
        info += "<td>" + ventana + "</td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaJefes").html(info);
}

/* Abre el modal con los datos del jefe seleccionado */
function AbrirAsignar(indice) {
    var item = _jefes[indice];
    if (item == null) { return; }

    $("#txtMailJefeSel").val(item.MailJefe);
    $("#txtNombreJefeSel").val(item.NombreJefe + " (" + item.MailJefe + ")");
    $("#txtVentanaActualSel").val(item.TieneVentana == 1 ? (item.FechaDesde + " a " + item.FechaHasta) : "Sin ventana registrada");

    $("#txtFechaDesde").val(item.TieneVentana == 1 ? item.FechaDesde : FechaHoyISO());
    $("#txtFechaHasta").val(item.TieneVentana == 1 ? item.FechaHasta : FechaHoyISO());
    $("#modalAsignar").modal("show");
}

/* Envia la ventana al handler */
function GuardarVentana() {
    var mailJefe = $("#txtMailJefeSel").val();
    var fechaDesde = $("#txtFechaDesde").val(); // yyyy-MM-dd
    var fechaHasta = $("#txtFechaHasta").val();
    var usuarioRegistro = $("#ContentPlaceHolder1_txtLoginUsuario").val();

    if (fechaDesde == null || fechaDesde === "") {
        MostrarMensaje("Debe indicar la fecha desde.", "warning");
        return;
    }
    if (fechaHasta == null || fechaHasta === "") {
        MostrarMensaje("Debe indicar la fecha hasta.", "warning");
        return;
    }
    if (fechaHasta < fechaDesde) {
        MostrarMensaje("La fecha hasta no puede ser menor que la fecha desde.", "warning");
        return;
    }

    var parameters = {
        "mailJefe": mailJefe,
        "fechaDesde": fechaDesde,
        "fechaHasta": fechaHasta,
        "usuarioRegistro": usuarioRegistro
    };

    $("#btnGuardarVentana").prop("disabled", true);

    PostVentana("GuardarVentana", parameters, function (respuesta) {
        $("#btnGuardarVentana").prop("disabled", false);

        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }

        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);

        if (respuesta.estado == "1") {
            $("#modalAsignar").modal("hide");
            BuscarJefes();
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

function FechaHoyISO() {
    var d = new Date();
    var mes = ("0" + (d.getMonth() + 1)).slice(-2);
    var dia = ("0" + d.getDate()).slice(-2);
    return d.getFullYear() + "-" + mes + "-" + dia;
}
```

- [ ] **Step 5: Registrar los archivos en `ReporteTareas.csproj`**

Junto a `<Content Include="Formulario\ParametrizacionHorarioUsuario.aspx" />` agregar:

```xml
    <Content Include="Formulario\ParametrizacionVentanaAprobacion.aspx" />
```

Junto a `<Content Include="js\...` (cualquier bloque de scripts) agregar:

```xml
    <Content Include="js\parametrizacionVentanaAprobacion.js" />
```

Junto al bloque `<Compile Include="Formulario\ParametrizacionHorarioUsuario.aspx.cs">...` agregar:

```xml
    <Compile Include="Formulario\ParametrizacionVentanaAprobacion.aspx.cs">
      <DependentUpon>ParametrizacionVentanaAprobacion.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\ParametrizacionVentanaAprobacion.aspx.designer.cs">
      <DependentUpon>ParametrizacionVentanaAprobacion.aspx</DependentUpon>
    </Compile>
```

- [ ] **Step 6: Compilar**

Ctrl+Shift+B / `msbuild`. Esperado: build sin errores.

- [ ] **Step 7: Verificación manual (smoke test de la pantalla)**

Requiere Task 1 desplegada. Ejecutar el sitio (F5 / IIS Express), iniciar sesión con un usuario válido y navegar directamente a `/Formulario/ParametrizacionVentanaAprobacion.aspx`. Verificar:
- La tabla lista ~22 jefes con nombre, correo y # colaboradores.
- El buscador filtra por nombre/correo.
- El botón "Asignar" abre el modal; guardar un rango muestra mensaje de éxito y la fila pasa a mostrar la ventana.
- Reabrir el modal del mismo jefe precarga el rango guardado; cambiarlo lo actualiza (una sola ventana).
- Guardar con Fecha hasta < Fecha desde muestra advertencia (validación cliente).

- [ ] **Step 8: Commit**

```bash
git add "ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx" "ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx.cs" "ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx.designer.cs" "ReporteTareas/js/parametrizacionVentanaAprobacion.js" "ReporteTareas/ReporteTareas.csproj"
git commit -m "feat(ui): pantalla ParametrizacionVentanaAprobacion"
```

---

## Task 5: Aplicar el bloqueo en la aprobación (`AdministrarTarea.ashx.cs`)

**Files:**
- Modify: `ReporteTareas/Formulario/AdministrarTarea.ashx.cs` (métodos `AprobarTareas` ~línea 909 y `AprobarTareasIndividual` ~línea 964)

**Interfaces:**
- Consumes: `CapaNegocio.NegVentanaAprobacion.ValidarVentana(string, DateTime)` (Task 2); `usuario.E_Mail` (ya cargado); `EntRespuesta.SerializaToJson()` (extensión `JSONHelper`, ya importada en este archivo). `using CapaNegocio;` ya está presente en el archivo.
- Produces: cuando el jefe está fuera de su ventana, la acción responde con el `EntRespuesta` de bloqueo (estado="0", tipoMensaje="warning") y NO aprueba. El modal de aprobación existente ya muestra `respuesta.mensaje`/`tipoMensaje` (no se toca el JS).

- [ ] **Step 1: Insertar la validación en `AprobarTareas`**

En el método `AprobarTareas`, localizar (≈línea 909):

```csharp
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                Int32 estadoAprobacion = 2;
```

Insertar el bloque de validación entre la carga de `usuario` y `Int32 estadoAprobacion = 2;`, quedando:

```csharp
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                // Ventana de aprobación del jefe (opt-in): si tiene ventana y hoy está fuera, no aprueba.
                EntRespuesta valVentana = NegVentanaAprobacion.ValidarVentana(usuario.E_Mail, DateTime.Now);
                if (valVentana.estado == "0")
                {
                    return valVentana.SerializaToJson();
                }

                Int32 estadoAprobacion = 2;
```

- [ ] **Step 2: Insertar la validación en `AprobarTareasIndividual`**

En el método `AprobarTareasIndividual`, localizar (≈línea 964):

```csharp
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                Int32 estadoAprobacion = 2;
```

Insertar el mismo bloque, quedando:

```csharp
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                // Ventana de aprobación del jefe (opt-in): si tiene ventana y hoy está fuera, no aprueba.
                EntRespuesta valVentana = NegVentanaAprobacion.ValidarVentana(usuario.E_Mail, DateTime.Now);
                if (valVentana.estado == "0")
                {
                    return valVentana.SerializaToJson();
                }

                Int32 estadoAprobacion = 2;
```

- [ ] **Step 3: Compilar**

Ctrl+Shift+B / `msbuild`. Esperado: build sin errores (verifica que `NegVentanaAprobacion`, `EntRespuesta` y `SerializaToJson` resuelven en este archivo).

- [ ] **Step 4: Verificación manual del bloqueo end-to-end**

Requiere Task 1 desplegada y un jefe de prueba cuyo `E_Mail` exista en `R_Usuarios` y tenga colaboradores. Con SSMS:

```sql
-- Registrar una ventana que EXCLUYE hoy para ese jefe (ajustar correo y fechas fuera de la fecha actual)
EXEC dbo.Sp_RTA_GuardarVentanaAprobacionJefe
     @MailJefe='<correo_jefe_prueba>', @FechaDesde='2000-01-01', @FechaHasta='2000-01-02', @UsuarioRegistro='PRUEBA';
```

Iniciar sesión como ese jefe, ir a `AprobacionTareasJefatura`, intentar aprobar tareas de un colaborador. Esperado: NO aprueba y muestra el mensaje "No puede aprobar en esta fecha. Su ventana de aprobación es del 01/01/2000 al 02/01/2000."

Luego ampliar la ventana para incluir hoy:

```sql
EXEC dbo.Sp_RTA_GuardarVentanaAprobacionJefe
     @MailJefe='<correo_jefe_prueba>', @FechaDesde='2026-01-01', @FechaHasta='2030-12-31', @UsuarioRegistro='PRUEBA';
```

Reintentar la aprobación. Esperado: aprueba normalmente. Finalmente:

```sql
DELETE FROM dbo.RTA_VentanaAprobacionJefe WHERE UsuarioRegistro='PRUEBA';
```

Verificar que, sin ventana, el jefe aprueba normalmente (opt-in).

- [ ] **Step 5: Commit**

```bash
git add "ReporteTareas/Formulario/AdministrarTarea.ashx.cs"
git commit -m "feat(aprobacion): bloquear aprobacion fuera de la ventana del jefe"
```

---

## Task 6: Registro en el menú (perfiles 1, 2, 18, 19)

**Files:**
- Create: `docs/superpowers/plans/sql/2026-07-20-ventana-aprobacion-menu.sql`

**Interfaces:**
- Consumes: página `ParametrizacionVentanaAprobacion.aspx` (Task 4); tablas `dbo.MenuDos`, `dbo.PerfilMenu`.
- Produces: opción de menú visible para perfiles 1, 2, 18, 19.

> **Nota:** el esquema de `MenuDos`/`PerfilMenu` no está en el repositorio y la BD estuvo intermitente al escribir el plan. Por eso este task **descubre** el esquema en vivo y **clona** la opción de la pantalla "Parametrización de Horario" (`Href='ParametrizacionHorarioUsuario.aspx'`), que es el análogo directo, sin depender de nombres de columnas exactos.

- [ ] **Step 1: Descubrir el esquema y la fila plantilla**

En SSMS contra `ReporTarea`, ejecutar y guardar los resultados:

```sql
-- Columnas de MenuDos y PerfilMenu
EXEC sp_help 'dbo.MenuDos';
EXEC sp_help 'dbo.PerfilMenu';

-- Fila de menú de la pantalla de Horario (plantilla) y su Id_Menu / grupo padre
SELECT * FROM dbo.MenuDos WHERE Href = 'ParametrizacionHorarioUsuario.aspx';

-- Permisos por perfil de esa opción y de su grupo padre
DECLARE @idHorario INT = (SELECT TOP 1 Id_Menu FROM dbo.MenuDos WHERE Href='ParametrizacionHorarioUsuario.aspx');
DECLARE @idPadre   INT = (SELECT TOP 1 Id_MenuPadre FROM dbo.MenuDos WHERE Id_Menu=@idHorario);
SELECT 'Opcion' AS Nivel, * FROM dbo.PerfilMenu WHERE Id_Menu = @idHorario;
SELECT 'Padre'  AS Nivel, * FROM dbo.PerfilMenu WHERE Id_Menu = @idPadre;
SELECT Id_Menu, Id_MenuPadre, Href, Es_Opcion_de_Menu FROM dbo.MenuDos WHERE Id_Menu=@idPadre;
```

Anotar: (a) el nombre de la **columna de texto/etiqueta** en `MenuDos` (la que contiene "Parametrización de Horario..."); (b) el nombre de la **columna FK de perfil** en `PerfilMenu`; (c) el `Id_MenuPadre` (grupo) y a qué perfiles está visible (`Estado='0'`).

- [ ] **Step 2: Crear el script de menú (clonado, robusto a columnas)**

Crear `docs/superpowers/plans/sql/2026-07-20-ventana-aprobacion-menu.sql`. El clon copia todas las columnas no-identidad de la fila de Horario (incluye el grupo padre correcto) y luego ajusta `Href` y la etiqueta; después clona los permisos de `PerfilMenu` y garantiza `Estado='0'` para 1, 2, 18, 19. `<COL_TEXTO_MENU>` y `<COL_PERFIL>` se reemplazan con los nombres reales hallados en el Step 1.

```sql
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.MenuDos WHERE Href = 'ParametrizacionVentanaAprobacion.aspx')
BEGIN
    PRINT 'La opción de menú ya existe; no se vuelve a crear.';
    RETURN;
END

-- 1) Clonar la fila de MenuDos de Horario (todas las columnas menos la identidad)
DECLARE @cols NVARCHAR(MAX) = STUFF((
    SELECT ',' + QUOTENAME(c.name)
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID('dbo.MenuDos') AND c.is_identity = 0
    ORDER BY c.column_id
    FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 1, '');

DECLARE @sql NVARCHAR(MAX) =
    N'INSERT INTO dbo.MenuDos (' + @cols + N') ' +
    N'SELECT ' + @cols + N' FROM dbo.MenuDos WHERE Href = N''ParametrizacionHorarioUsuario.aspx'';';
EXEC sys.sp_executesql @sql;

-- El clon es el registro de mayor Id_Menu con ese mismo Href (el original conserva su Id menor)
DECLARE @idNuevo INT = (SELECT MAX(Id_Menu) FROM dbo.MenuDos WHERE Href = 'ParametrizacionHorarioUsuario.aspx');

-- 2) Ajustar Href y etiqueta del clon  (reemplazar <COL_TEXTO_MENU> por la columna real de texto)
UPDATE dbo.MenuDos
   SET Href = 'ParametrizacionVentanaAprobacion.aspx',
       <COL_TEXTO_MENU> = 'Parametrización de Ventana de Aprobación'
 WHERE Id_Menu = @idNuevo;

-- 3) Clonar los permisos de PerfilMenu de la opción de Horario hacia la nueva opción
DECLARE @idHorario INT = (SELECT TOP 1 Id_Menu FROM dbo.MenuDos WHERE Href='ParametrizacionHorarioUsuario.aspx' AND Id_Menu < @idNuevo);

DECLARE @colsPM NVARCHAR(MAX) = STUFF((
    SELECT ',' + QUOTENAME(c.name)
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID('dbo.PerfilMenu') AND c.is_identity = 0
    ORDER BY c.column_id
    FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 1, '');

DECLARE @sqlPM NVARCHAR(MAX) =
    N'INSERT INTO dbo.PerfilMenu (' + @colsPM + N') ' +
    N'SELECT ' + @colsPM + N' FROM dbo.PerfilMenu WHERE Id_Menu = @orig;';
DECLARE @pm NVARCHAR(MAX) = REPLACE(@sqlPM, 'Id_Menu = @orig', 'Id_Menu = ' + CAST(@idHorario AS NVARCHAR(20)));
EXEC sys.sp_executesql @pm;

-- Reasignar el Id_Menu de las filas recién clonadas al nuevo menú
UPDATE dbo.PerfilMenu SET Id_Menu = @idNuevo
 WHERE Id_Menu = @idHorario
   AND id_MenuPerfil > (SELECT ISNULL(MAX(id_MenuPerfil),0) FROM dbo.PerfilMenu WHERE Id_Menu = @idHorario) - 0;
-- NOTA: si el paso anterior resulta ambiguo en tu entorno, reemplázalo por INSERTs explícitos
-- (ver Step 3) usando <COL_PERFIL>. Preferir el Step 3 si PerfilMenu no tiene un identity fiable.

-- 4) Garantizar visibilidad (Estado='0') para perfiles 1,2,18,19 en la nueva opción
--    (reemplazar <COL_PERFIL> por la columna real de perfil)
MERGE dbo.PerfilMenu AS t
USING (VALUES (1),(2),(18),(19)) AS p(Perfil)
   ON t.Id_Menu = @idNuevo AND t.<COL_PERFIL> = p.Perfil
WHEN MATCHED THEN UPDATE SET t.Estado = '0'
WHEN NOT MATCHED THEN INSERT (Id_Menu, <COL_PERFIL>, Estado) VALUES (@idNuevo, p.Perfil, '0');

-- 5) Asegurar que el GRUPO PADRE también sea visible (Estado='0') para 2 y 19
--    (el padre de Horario suele estar visible solo para 1 y 18; ver Global Constraints)
DECLARE @idPadre INT = (SELECT TOP 1 Id_MenuPadre FROM dbo.MenuDos WHERE Id_Menu=@idNuevo);
MERGE dbo.PerfilMenu AS t
USING (VALUES (1),(2),(18),(19)) AS p(Perfil)
   ON t.Id_Menu = @idPadre AND t.<COL_PERFIL> = p.Perfil
WHEN MATCHED THEN UPDATE SET t.Estado = '0'
WHEN NOT MATCHED THEN INSERT (Id_Menu, <COL_PERFIL>, Estado) VALUES (@idPadre, p.Perfil, '0');

PRINT 'Menú creado. Nuevo Id_Menu = ' + CAST(@idNuevo AS VARCHAR(20));
```

> Si el `UPDATE ... Id_Menu` del punto 3 no es fiable en tu entorno (PerfilMenu sin identity fiable), **omitirlo** y crear los permisos solo con el `MERGE` del punto 4 (que ya inserta las 4 filas necesarias). El clon del punto 3 es una comodidad, no un requisito.

- [ ] **Step 3: Ejecutar y ajustar**

Reemplazar `<COL_TEXTO_MENU>` y `<COL_PERFIL>` por los nombres reales del Step 1, ejecutar el script y anotar el `Id_Menu` nuevo impreso.

- [ ] **Step 4: Verificar visibilidad por perfil**

Para cada perfil objetivo, confirmar que el SP del menú devuelve la nueva opción:

```sql
EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 1;   -- debe incluir ParametrizacionVentanaAprobacion.aspx
EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 2;
EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 18;
EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 19;
```

Además, iniciar sesión en el sitio con un usuario de perfil 2 (o refrescar, el menú se reconstruye por carga) y confirmar que la opción aparece y navega a la pantalla.

- [ ] **Step 5: Commit**

```bash
git add "docs/superpowers/plans/sql/2026-07-20-ventana-aprobacion-menu.sql"
git commit -m "feat(menu): registro de ParametrizacionVentanaAprobacion para perfiles 1,2,18,19"
```

---

## Notas de despliegue

- Los cambios de BD (Task 1 y Task 6) deben ejecutarse en cada entorno donde corra el sitio (prueba → producción). Recomendado validar primero en `ReporTareaTest`/`ReporTareaPreProd` si están disponibles.
- El sitio es WebForms compilado: desplegar los ensamblados de `CapaEntidad/CapaDato/CapaNegocio` y `ReporteTareas`, más los archivos de contenido nuevos (`.aspx`, `.aspx.cs` compilado, `.ashx`, `.js`).
- No se modifica ninguna credencial ni el `Web.config`.

## Cobertura del spec (self-review)

- Tabla + una-ventana-por-jefe → Task 1 (UNIQUE MailJefe, upsert).
- Opt-in (sin ventana permite) → Task 1 SP Validar + Task 2 fail-open.
- Identificación por email del jefe → Task 5 (`usuario.E_Mail`).
- Pantalla de administración (listar/guardar) → Tasks 3 y 4.
- Aplicación del bloqueo en AprobacionTareasJefatura → Task 5.
- Menú perfiles 1,2,18,19 con semántica invertida y visibilidad del padre → Task 6.
- Reutilizar `DaoReporTareaAranda.conectar()` sin exponer credencial → Tasks 2 (DAO) y Global Constraints.
- Correos de jefe sin usuario en R_Usuarios se listan (NombreJefe = correo) → Task 1 SP Listar (`ISNULL(u.Nom_Usuario, j.MailJefe)`).
