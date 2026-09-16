# Inactivar jefe en "Ventana de aprobación" — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Permitir inactivar/reactivar a un jefe únicamente en el módulo de ventana de aprobación, para que no aparezca en su tabla, de forma reversible.

**Architecture:** Se agrega una tabla dedicada `RTA_JefeExcluidoVentana` (Opción A del spec). El SP de listado hace `LEFT JOIN` y filtra según un nuevo parámetro `@IncluirInactivos`; un SP nuevo hace upsert del estado. La capa C# (Entidad/Dao/Negocio/Handler) expone la acción y el frontend (ASPX + JS) agrega un checkbox "Mostrar inactivos" e íconos de inactivar/reactivar por fila.

**Tech Stack:** ASP.NET WebForms (C#, .NET Framework), arquitectura 3 capas (CapaEntidad/CapaDato/CapaNegocio) + web `ReporteTareas`, SQL Server (stored procedures), jQuery.

## Global Constraints

- **Sin framework de pruebas automatizadas.** La verificación es manual: SQL directo (SPs), HTTP al handler (PowerShell) revisando el JSON, y navegador para la UI.
- **Codificación:** los `.js` van en **UTF-8 con BOM**; el handler ya escribe con `Response.ContentEncoding = Encoding.UTF8`. Tras editar el `.js`, **verificar que el BOM siga presente** y re-agregarlo si se perdió.
- **Identidad del jefe:** siempre por `MailJefe` = `LTRIM(RTRIM(MailCodJefeInm))` / email, igual que `RTA_VentanaAprobacionJefe`.
- **Alcance:** solo oculta de esta pantalla. No toca `R_Usuarios`, login, ni la validación de aprobación.
- **UX:** acción directa (sin modal de confirmación).
- **Base de datos (dev/servidor actual):** `Data Source=192.168.11.14; Initial Catalog=ReporTarea; User Id=sa`.
- **MSBuild:** `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`, solución `ReporteTareas.sln`, configuración `Debug`.
- **App corriendo en:** `http://localhost:51037` (IIS Express, shadow-copy: el DLL en `bin` no queda bloqueado).

---

## File Structure

- **Crear** `docs/superpowers/plans/sql/2026-07-24-inactivar-jefe-ventana-aprobacion.sql` — tabla + SP modificado + SP nuevo.
- **Modificar** `CapaEntidad/EntVentanaAprobacionJefe.cs` — propiedad `Inactivo`.
- **Modificar** `CapaDato/DaoVentanaAprobacion.cs` — `ListarJefes` (nuevo parámetro + campo) y método `ExcluirJefe`.
- **Modificar** `CapaNegocio/NegVentanaAprobacion.cs` — firma `ListarJefes` + `ExcluirJefe`.
- **Modificar** `ReporteTareas/Formulario/AdministrarVentanaAprobacion.ashx.cs` — parámetro en `ListaJefes` + acción `ExcluirJefe`.
- **Modificar** `ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx` — checkbox "Mostrar inactivos" + bump `?v=3`.
- **Modificar** `ReporteTareas/js/parametrizacionVentanaAprobacion.js` — enviar `incluirInactivos`, render de acciones/estado, funciones `InactivarJefe`/`ActivarJefe`/`CambiarEstadoJefe`.

---

## Task 1: Base de datos (tabla + SPs)

**Files:**
- Create: `docs/superpowers/plans/sql/2026-07-24-inactivar-jefe-ventana-aprobacion.sql`

**Interfaces:**
- Produces:
  - Tabla `dbo.RTA_JefeExcluidoVentana(Id, MailJefe, Estado, UsuarioRegistro, FechaRegistro)`.
  - `Sp_RTA_ListarJefesVentanaAprobacion(@Filtro VARCHAR(150)='', @IncluirInactivos BIT=0)` → columnas: `MailJefe, NombreJefe, NumColaboradores, FechaDesde, FechaHasta, TieneVentana, Inactivo`.
  - `Sp_RTA_ExcluirJefeVentana(@MailJefe VARCHAR(150), @Excluir BIT, @UsuarioRegistro VARCHAR(100)=NULL)` → `SELECT Respuestas INT, Mensaje VARCHAR`.

- [ ] **Step 1: Crear el script SQL**

Crear `docs/superpowers/plans/sql/2026-07-24-inactivar-jefe-ventana-aprobacion.sql` con:

```sql
/* ============================================================================
   Inactivar jefe en el módulo "Ventana de aprobación" — objetos de BD
   Base: ReporTarea
   ============================================================================ */

/* ---------- 1) Tabla de exclusión ---------- */
IF OBJECT_ID('dbo.RTA_JefeExcluidoVentana','U') IS NULL
BEGIN
    CREATE TABLE dbo.RTA_JefeExcluidoVentana
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        MailJefe        VARCHAR(150)      NOT NULL,
        Estado          BIT               NOT NULL CONSTRAINT DF_RTA_JefeExcl_Estado   DEFAULT(1),
        UsuarioRegistro VARCHAR(100)      NULL,
        FechaRegistro   DATETIME          NOT NULL CONSTRAINT DF_RTA_JefeExcl_FechaReg DEFAULT(GETDATE()),
        CONSTRAINT PK_RTA_JefeExcluidoVentana PRIMARY KEY (Id),
        CONSTRAINT UQ_RTA_JefeExcl_MailJefe UNIQUE (MailJefe)
    );
END
GO

/* ---------- 2) SP Listar (modificado: agrega @IncluirInactivos y campo Inactivo) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarJefesVentanaAprobacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarJefesVentanaAprobacion;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarJefesVentanaAprobacion
    @Filtro           VARCHAR(150) = '',
    @IncluirInactivos BIT = 0
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
        TieneVentana      = CASE WHEN v.Id IS NOT NULL THEN 1 ELSE 0 END,
        Inactivo          = CASE WHEN e.Id IS NOT NULL THEN 1 ELSE 0 END
    FROM Jefes j
    OUTER APPLY (
        SELECT TOP 1 ru.Nom_Usuario
        FROM dbo.R_Usuarios ru
        WHERE LTRIM(RTRIM(ru.E_Mail)) = j.MailJefe
        ORDER BY ru.Cod_Usuario
    ) u
    LEFT JOIN dbo.RTA_VentanaAprobacionJefe v
        ON v.MailJefe = j.MailJefe AND v.Estado = 1
    LEFT JOIN dbo.RTA_JefeExcluidoVentana e
        ON e.MailJefe = j.MailJefe AND e.Estado = 1
    WHERE (@Filtro = ''
           OR j.MailJefe LIKE '%' + @Filtro + '%'
           OR ISNULL(u.Nom_Usuario,'') LIKE '%' + @Filtro + '%')
      AND (@IncluirInactivos = 1 OR e.Id IS NULL)
    ORDER BY NombreJefe;
END
GO

/* ---------- 3) SP Excluir/Reactivar (upsert del estado) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ExcluirJefeVentana','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ExcluirJefeVentana;
GO
CREATE PROCEDURE dbo.Sp_RTA_ExcluirJefeVentana
    @MailJefe        VARCHAR(150),
    @Excluir         BIT,
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

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM dbo.RTA_JefeExcluidoVentana WITH (UPDLOCK, HOLDLOCK) WHERE MailJefe = @MailJefe)
        BEGIN
            UPDATE dbo.RTA_JefeExcluidoVentana
               SET Estado = @Excluir,
                   UsuarioRegistro = @UsuarioRegistro,
                   FechaRegistro = GETDATE()
             WHERE MailJefe = @MailJefe;
        END
        ELSE IF @Excluir = 1
        BEGIN
            INSERT INTO dbo.RTA_JefeExcluidoVentana (MailJefe, Estado, UsuarioRegistro, FechaRegistro)
            VALUES (@MailJefe, 1, @UsuarioRegistro, GETDATE());
        END

        SET @Respuestas = 1;
        SET @Mensaje = CASE WHEN @Excluir = 1
                            THEN 'Jefe inactivado en este modulo correctamente.'
                            ELSE 'Jefe reactivado correctamente.' END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo actualizar el estado del jefe: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO
```

> Nota: los mensajes del SP se dejan **sin tildes** ("modulo") para evitar cualquier problema de codificación al ejecutar el script; el resto de la app ya envía UTF-8 pero el texto fijo del SP es intrascendente y así se evita el riesgo.

- [ ] **Step 2: Ejecutar el script en la base**

Ejecutar el script contra `ReporTarea` (SSMS o el runner que uses). Debe crear la tabla y (re)crear los 2 SPs sin errores.

- [ ] **Step 3: Verificar los SPs con datos de prueba**

Ejecutar en SSMS (usa un jefe real con acento, p. ej. `cmejia@dos.com.ec`):

```sql
-- Inactivar
EXEC dbo.Sp_RTA_ExcluirJefeVentana @MailJefe='cmejia@dos.com.ec', @Excluir=1, @UsuarioRegistro='test';
-- 1) NO debe aparecer (solo activos)
EXEC dbo.Sp_RTA_ListarJefesVentanaAprobacion @Filtro='cmejia', @IncluirInactivos=0;
-- 2) SÍ debe aparecer con Inactivo=1
EXEC dbo.Sp_RTA_ListarJefesVentanaAprobacion @Filtro='cmejia', @IncluirInactivos=1;
-- Reactivar (limpieza)
EXEC dbo.Sp_RTA_ExcluirJefeVentana @MailJefe='cmejia@dos.com.ec', @Excluir=0;
-- 3) Debe volver a aparecer con Inactivo=0
EXEC dbo.Sp_RTA_ListarJefesVentanaAprobacion @Filtro='cmejia', @IncluirInactivos=0;
```

Expected: (1) sin filas para ese jefe; (2) una fila con `Inactivo=1`; (3) una fila con `Inactivo=0`. Cada `EXEC` de excluir devuelve `Respuestas=1` con su mensaje.

- [ ] **Step 4: Commit**

```bash
git add "docs/superpowers/plans/sql/2026-07-24-inactivar-jefe-ventana-aprobacion.sql"
git commit -m "feat(db): tabla y SPs para inactivar jefe en ventana de aprobacion"
```

---

## Task 2: Backend C# (Entidad + Dao + Negocio + Handler)

**Files:**
- Modify: `CapaEntidad/EntVentanaAprobacionJefe.cs`
- Modify: `CapaDato/DaoVentanaAprobacion.cs`
- Modify: `CapaNegocio/NegVentanaAprobacion.cs`
- Modify: `ReporteTareas/Formulario/AdministrarVentanaAprobacion.ashx.cs`

**Interfaces:**
- Consumes (de Task 1): SPs `Sp_RTA_ListarJefesVentanaAprobacion(@Filtro,@IncluirInactivos)` y `Sp_RTA_ExcluirJefeVentana(@MailJefe,@Excluir,@UsuarioRegistro)`.
- Produces (para Task 3): acción handler `ExcluirJefe` con `parameters {mailJefe, excluir, usuarioRegistro}`; acción `ListaJefes` acepta `parameters {filtro, incluirInactivos}`; el JSON de cada jefe incluye `Inactivo` (0/1).

- [ ] **Step 1: Agregar la propiedad `Inactivo` a la entidad**

En `CapaEntidad/EntVentanaAprobacionJefe.cs`, dentro de la clase, agregar:

```csharp
        public int Inactivo { get; set; }
```

(queda junto a `TieneVentana`.)

- [ ] **Step 2: Modificar `ListarJefes` en el Dao**

En `CapaDato/DaoVentanaAprobacion.cs`, reemplazar la firma y cuerpo de `ListarJefes` por:

```csharp
        /// <summary>Lista los jefes inmediatos (MailCodJefeInm) con su ventana vigente.</summary>
        public static List<EntVentanaAprobacionJefe> ListarJefes(string filtro, bool incluirInactivos)
        {
            List<EntVentanaAprobacionJefe> lista = new List<EntVentanaAprobacionJefe>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarJefesVentanaAprobacion", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 150).Value = filtro ?? string.Empty;
                cmd.Parameters.Add("@IncluirInactivos", SqlDbType.Bit).Value = incluirInactivos;
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
                            TieneVentana = Convert.ToInt32(dr["TieneVentana"].ToString()),
                            Inactivo = Convert.ToInt32(dr["Inactivo"].ToString())
                        });
                    }
                }
            }

            return lista;
        }
```

- [ ] **Step 3: Agregar `ExcluirJefe` al Dao**

En el mismo archivo, agregar este método (junto a `GuardarVentana`):

```csharp
        /// <summary>Activa (excluir=true) o quita (excluir=false) la exclusión del jefe en este módulo.</summary>
        public static EntRespuesta ExcluirJefe(string mailJefe, bool excluir, string usuarioRegistro)
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
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_ExcluirJefeVentana", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@MailJefe", SqlDbType.VarChar, 150).Value = mailJefe ?? string.Empty;
                    cmd.Parameters.Add("@Excluir", SqlDbType.Bit).Value = excluir;
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
                respuesta.mensaje = "Ocurrió un error al actualizar el estado del jefe. Detalle: " + ex.Message;
            }

            return respuesta;
        }
```

- [ ] **Step 4: Actualizar la capa de Negocio**

En `CapaNegocio/NegVentanaAprobacion.cs`, reemplazar `ListarJefes` y agregar `ExcluirJefe`:

```csharp
        public static List<EntVentanaAprobacionJefe> ListarJefes(string filtro, bool incluirInactivos)
        {
            return DaoVentanaAprobacion.ListarJefes(filtro, incluirInactivos);
        }

        public static EntRespuesta ExcluirJefe(string mailJefe, bool excluir, string usuarioRegistro)
        {
            return DaoVentanaAprobacion.ExcluirJefe(mailJefe, excluir, usuarioRegistro);
        }
```

- [ ] **Step 5: Handler — parámetro en `ListaJefes` y acción `ExcluirJefe`**

En `ReporteTareas/Formulario/AdministrarVentanaAprobacion.ashx.cs`:

(a) Registrar la nueva acción en `ProcessRequest` (junto a los otros `if (Action == ...)`):

```csharp
                if (Action == "ExcluirJefe")
                {
                    existAction = true;
                    responseAction.Append(ExcluirJefe(parameters));
                }
```

(b) Reemplazar el método `ListaJefes` por:

```csharp
        private string ListaJefes(dynamic campos)
        {
            try
            {
                string filtro = "";
                try { filtro = Convert.ToString(campos["filtro"]); }
                catch { filtro = ""; }

                bool incluirInactivos = false;
                try { incluirInactivos = Convert.ToBoolean(campos["incluirInactivos"]); }
                catch { incluirInactivos = false; }

                return ToJson(NegVentanaAprobacion.ListarJefes(filtro, incluirInactivos));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los jefes. " + ex.Message, "danger");
            }
        }
```

(c) Agregar el método `ExcluirJefe` (junto a `GuardarVentana`):

```csharp
        private string ExcluirJefe(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();

            try
            {
                string mailJefe = Convert.ToString(campos["mailJefe"]).Trim();

                bool excluir = false;
                try { excluir = Convert.ToBoolean(campos["excluir"]); }
                catch { excluir = false; }

                string usuarioRegistro = "";
                try { usuarioRegistro = Convert.ToString(campos["usuarioRegistro"]).Trim(); }
                catch { usuarioRegistro = ""; }

                if (string.IsNullOrWhiteSpace(mailJefe))
                {
                    return responseMessage("0", "Debe seleccionar un jefe.", "warning");
                }

                respuesta = NegVentanaAprobacion.ExcluirJefe(mailJefe, excluir, usuarioRegistro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al actualizar el estado del jefe. " + ex.Message, "danger");
            }

            return ToJson(respuesta);
        }
```

- [ ] **Step 6: Compilar**

Run:
```bash
"C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly
```
Expected: `EXIT CODE: 0`, sin errores de compilación.

- [ ] **Step 7: Verificar por HTTP (handler en vivo)**

Con la app corriendo, ejecutar en PowerShell (usa un jefe real, p. ej. `cmejia@dos.com.ec`):

```powershell
function Call-Handler($bodyJson) {
  $url = "http://localhost:51037/Formulario/AdministrarVentanaAprobacion.ashx"
  $req = [System.Net.HttpWebRequest]::Create($url)
  $req.Method = "POST"; $req.ContentType = "application/json; charset=utf-8"
  $b = [System.Text.Encoding]::UTF8.GetBytes($bodyJson); $req.ContentLength = $b.Length
  $s = $req.GetRequestStream(); $s.Write($b,0,$b.Length); $s.Close()
  $resp = $req.GetResponse(); $ms = New-Object System.IO.MemoryStream
  $resp.GetResponseStream().CopyTo($ms); $resp.Close()
  return [System.Text.Encoding]::UTF8.GetString($ms.ToArray())
}
# Inactivar
Call-Handler '[{"action":"ExcluirJefe","parameters":{"mailJefe":"cmejia@dos.com.ec","excluir":1,"usuarioRegistro":"test"}}]'
# Solo activos: NO debe salir cmejia
Call-Handler '[{"action":"ListaJefes","parameters":{"filtro":"cmejia","incluirInactivos":false}}]'
# Incluyendo inactivos: SÍ sale, con "Inactivo":1
Call-Handler '[{"action":"ListaJefes","parameters":{"filtro":"cmejia","incluirInactivos":true}}]'
# Reactivar (limpieza)
Call-Handler '[{"action":"ExcluirJefe","parameters":{"mailJefe":"cmejia@dos.com.ec","excluir":0,"usuarioRegistro":"test"}}]'
```

Expected:
- `ExcluirJefe excluir=1` → `{"estado":"1", ... ,"mensaje":"Jefe inactivado en este modulo correctamente.", ...}`.
- `ListaJefes incluirInactivos=false` → array **sin** `cmejia`.
- `ListaJefes incluirInactivos=true` → incluye `cmejia` con `"Inactivo":1` y el nombre con tildes correctas (`Hernán Mejía`).
- `ExcluirJefe excluir=0` → `{"estado":"1", ... ,"mensaje":"Jefe reactivado correctamente."}`.

- [ ] **Step 8: Commit**

```bash
git add "CapaEntidad/EntVentanaAprobacionJefe.cs" "CapaDato/DaoVentanaAprobacion.cs" "CapaNegocio/NegVentanaAprobacion.cs" "ReporteTareas/Formulario/AdministrarVentanaAprobacion.ashx.cs"
git commit -m "feat(backend): accion ExcluirJefe y filtro de inactivos en ventana de aprobacion"
```

---

## Task 3: Frontend (ASPX + JS)

**Files:**
- Modify: `ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx`
- Modify: `ReporteTareas/js/parametrizacionVentanaAprobacion.js`

**Interfaces:**
- Consumes (de Task 2): acción `ListaJefes` con `incluirInactivos`; acción `ExcluirJefe` con `{mailJefe, excluir, usuarioRegistro}`; campo `Inactivo` por jefe.

- [ ] **Step 1: Agregar checkbox "Mostrar inactivos" y bump de versión del JS (ASPX)**

En `ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx`:

(a) Cambiar la referencia del script de `?v=2` a `?v=3`:

```html
    <script src="../js/parametrizacionVentanaAprobacion.js?v=3" type="text/javascript"></script>
```

(b) Dentro del `<div class="row">` de búsqueda, después del `form-group` de los botones (el que contiene `btnBuscar`/`btnRefrescar`), agregar:

```html
                        <div class="form-group col-lg-4" style="padding-top: 25px">
                            <label class="checkbox-inline">
                                <input type="checkbox" id="chkMostrarInactivos" onchange="BuscarJefes()" /> Mostrar inactivos
                            </label>
                        </div>
```

- [ ] **Step 2: Enviar `incluirInactivos` desde `BuscarJefes` (JS)**

En `ReporteTareas/js/parametrizacionVentanaAprobacion.js`, reemplazar `BuscarJefes` por:

```javascript
/* Lista jefes aplicando el filtro y el checkbox de inactivos */
function BuscarJefes() {
    var filtro = $("#txtBuscar").val();
    var incluirInactivos = $("#chkMostrarInactivos").is(":checked");

    PostVentana("ListaJefes", { "filtro": filtro, "incluirInactivos": incluirInactivos }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        _jefes = respuesta || [];
        RenderTablaJefes(_jefes);
    });
}
```

- [ ] **Step 3: Render de acciones y marca de inactivo (JS)**

Reemplazar `RenderTablaJefes` por:

```javascript
function RenderTablaJefes(lista) {
    var info = "";
    info += "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer'>";
    info += "<thead><tr role='row'>";
    info += "<th style='text-align:center'>Acciones</th>";
    info += "<th>Jefe</th>";
    info += "<th>Correo</th>";
    info += "<th style='text-align:center'>Colaboradores</th>";
    info += "<th>Ventana de aprobación</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='5' style='text-align:center'>No existen jefes para esta búsqueda.</td></tr>";
    }

    $.each(lista, function (i, item) {
        var esInactivo = (item.Inactivo == 1);

        var ventana = (item.TieneVentana == 1)
            ? (Escapar(item.FechaDesde) + " a " + Escapar(item.FechaHasta))
            : "<span class='label label-default'>Sin ventana</span>";

        var acciones = "";
        if (esInactivo) {
            acciones += "<i class='fa fa-check' title='Reactivar' style='cursor:pointer;color:#3c763d' onclick='ActivarJefe(" + i + ")'></i>";
        } else {
            acciones += "<i class='fa fa-hand-o-right' title='Asignar ventana' style='cursor:pointer' onclick='AbrirAsignar(" + i + ")'></i>";
            acciones += " &nbsp; ";
            acciones += "<i class='fa fa-ban' title='Inactivar de este módulo' style='cursor:pointer;color:#a94442' onclick='InactivarJefe(" + i + ")'></i>";
        }

        var estiloFila = esInactivo ? " style='opacity:0.55'" : "";
        var etiquetaInactivo = esInactivo ? " <span class='label label-default'>Inactivo</span>" : "";

        info += "<tr role='row'" + estiloFila + ">";
        info += "<td style='text-align:center'>" + acciones + "</td>";
        info += "<td>" + Escapar(item.NombreJefe) + etiquetaInactivo + "</td>";
        info += "<td>" + Escapar(item.MailJefe) + "</td>";
        info += "<td style='text-align:center'>" + Escapar(String(item.NumColaboradores)) + "</td>";
        info += "<td>" + ventana + "</td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaJefes").html(info);
}
```

- [ ] **Step 4: Funciones de inactivar/reactivar (JS)**

Agregar (por ejemplo después de `AbrirAsignar`):

```javascript
/* Inactiva/reactiva un jefe en este módulo (acción directa) */
function InactivarJefe(indice) {
    var item = _jefes[indice];
    if (item == null) { return; }
    CambiarEstadoJefe(item.MailJefe, 1);
}

function ActivarJefe(indice) {
    var item = _jefes[indice];
    if (item == null) { return; }
    CambiarEstadoJefe(item.MailJefe, 0);
}

function CambiarEstadoJefe(mailJefe, excluir) {
    var usuarioRegistro = $("#ContentPlaceHolder1_txtLoginUsuario").val();

    var parameters = {
        "mailJefe": mailJefe,
        "excluir": excluir,
        "usuarioRegistro": usuarioRegistro
    };

    PostVentana("ExcluirJefe", parameters, function (respuesta) {
        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado == "1") {
            BuscarJefes();
        }
    });
}
```

- [ ] **Step 5: Verificar/re-agregar el BOM del JS**

Editar el `.js` con herramientas de texto puede quitar el BOM. Verificar y re-agregarlo si falta:

```powershell
$f = (Resolve-Path "ReporteTareas\js\parametrizacionVentanaAprobacion.js")
$b = [System.IO.File]::ReadAllBytes($f)
if (-not ($b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF)) {
  $bom = [byte[]](0xEF,0xBB,0xBF)
  $out = New-Object byte[] ($bom.Length + $b.Length)
  [Array]::Copy($bom,0,$out,0,3); [Array]::Copy($b,0,$out,3,$b.Length)
  [System.IO.File]::WriteAllBytes($f,$out); "BOM re-agregado"
} else { "BOM OK" }
```
Expected: `BOM OK` o `BOM re-agregado`.

- [ ] **Step 6: Verificación funcional en el navegador**

Con la app corriendo y sesión iniciada, abrir
`http://localhost:51037/Formulario/ParametrizacionVentanaAprobacion.aspx` y **Ctrl+F5**. Comprobar:
1. Se ve la columna **Acciones** con el ícono 🚫 (inactivar) en cada jefe activo.
2. Clic en 🚫 de un jefe → mensaje de éxito y el jefe **desaparece** de la tabla.
3. Marcar **"Mostrar inactivos"** → el jefe reaparece **atenuado**, con etiqueta **Inactivo** y el ícono ✔️.
4. Clic en ✔️ → mensaje de éxito y el jefe vuelve a estar **activo** (sin atenuar).
5. Los nombres con tilde (`Hernán Mejía`) y los textos fijos se ven correctos.

- [ ] **Step 7: Commit**

```bash
git add "ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx" "ReporteTareas/js/parametrizacionVentanaAprobacion.js"
git commit -m "feat(ui): inactivar/reactivar jefe y filtro Mostrar inactivos en ventana de aprobacion"
```

---

## Self-Review (cobertura del spec)

- **Tabla `RTA_JefeExcluidoVentana`** → Task 1, Step 1. ✅
- **SP listar con `@IncluirInactivos` + campo `Inactivo`** → Task 1, Step 1. ✅
- **SP `Sp_RTA_ExcluirJefeVentana` (upsert, contrato Respuestas/Mensaje)** → Task 1, Step 1. ✅
- **Entidad `Inactivo`** → Task 2, Step 1. ✅
- **Dao `ListarJefes(filtro, incluirInactivos)` + `ExcluirJefe`** → Task 2, Steps 2-3. ✅
- **Negocio** → Task 2, Step 4. ✅
- **Handler: `ListaJefes` con `incluirInactivos` + acción `ExcluirJefe`** → Task 2, Step 5. ✅
- **Checkbox "Mostrar inactivos"** → Task 3, Step 1. ✅
- **Íconos inactivar/reactivar + fila atenuada + etiqueta** → Task 3, Step 3. ✅
- **Acción directa (sin confirmación)** → Task 3, Step 4 (llamada directa). ✅
- **Codificación (BOM del JS / handler UTF-8)** → Task 3, Step 5 + Global Constraints. ✅
- **`usuarioRegistro` desde `txtLoginUsuario`** → Task 3, Step 4. ✅

Sin placeholders. Firmas/tipos consistentes entre tareas (`ListarJefes(string, bool)`, `ExcluirJefe(string, bool, string)`, campo `Inactivo`, acción `ExcluirJefe` con `{mailJefe, excluir, usuarioRegistro}`).
