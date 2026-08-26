# Marcación: correo de confirmación y corrección de trazabilidad — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que cada usuario reciba un correo al registrar su entrada y su salida, y que ese correo confirme algo cierto — corrigiendo los tres defectos que hoy hacen que la marcación no sea confiable.

**Architecture:** Un SP nuevo (`Sp_RTA_RegistrarMarcacion`) pasa a ser el único dueño de la decisión entrada/salida: recibe la acción explícita, resuelve la fila del día por `Id_Usuario` + fecha con `UPDLOCK, HOLDLOCK`, y toma toda hora de `GETDATE()`. Encima van 3 capas C#, un handler `.ashx` nuevo que identifica al usuario por sesión cifrada, y un correo no bloqueante disparado solo tras éxito confirmado. El SP compartido `InsertarModificarEliminarRegistroBiometrico` y la carga manual **no se tocan**.

**Tech Stack:** ASP.NET WebForms (C#, .NET Framework 4.6.1), 3 capas + web `ReporteTareas`, SQL Server (stored procedures), jQuery, `System.Net.Mail` vía `EnvioCorreoHelper`.

## Global Constraints

- **Sin framework de pruebas automatizadas.** Verificación manual: MSBuild EXIT 0, `sqlcmd`, navegador.
- **La BD de desarrollo ES la de producción.** `CapaDato/DaoReporTareaAranda.cs:22` apunta a `192.168.11.14 / ReporTarea`. Toda prueba de datos va **dentro de una transacción con `ROLLBACK`**.
- **Base de datos:** `-S "tcp:192.168.11.14,1433" -U sa -P "$SQLPASS" -d ReporTarea` (conectividad intermitente — reintentar si da timeout). La contraseña **no se escribe en este documento**: definir `SQLPASS` en el entorno antes de correr los comandos (bash: `export SQLPASS='...'`; PowerShell: usar `$env:SQLPASS` en lugar de `$SQLPASS`). Este repositorio es público.
- **MSBuild:** `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`, `ReporteTareas.sln`, `Debug`. Ejecutar **vía PowerShell** (`& $msb ...`) por el espacio en la ruta.
- **Scripts SQL con `SET QUOTED_IDENTIFIER ON;`** al inicio.
- **Mensajes fijos de SP sin tildes.** Los textos en C# sí llevan tildes.
- **`.js` en UTF-8 con BOM.** `RegistroLaboral.js` ya lo tiene (`ef bb bf`): preservarlo al editar.
- **Handler con `context.Response.ContentEncoding = Encoding.UTF8`.**
- **Toda hora viene de `GETDATE()` de SQL.** Nunca `DateTime.Now` del servidor web.
- **NO modificar:** el SP `InsertarModificarEliminarRegistroBiometrico`, las acciones `RegistrarEvento`/`RegistrarEvento2`/`RegistrarEvento3` y `ConsultarEvento` de `ObtenerListaTareas.ashx.cs`, ni la pantalla `RegistroAsistencia.aspx`.
- **Fuera de alcance:** limpiar duplicados existentes, índice único, columnas de auditoría, y cerrar el hueco de autorización de `RegistrarEvento2/3`.

---

## File Structure

- **Crear** `docs/superpowers/plans/sql/2026-07-29-marcacion-registrar.sql` — el SP nuevo.
- **Crear** `CapaEntidad/EntMarcacion.cs` — resultado de una marcación.
- **Crear** `CapaDato/DaoMarcacion.cs` — llamada al SP.
- **Crear** `CapaNegocio/NegMarcacion.cs` — pasarela.
- **Crear** `ReporteTareas/Formulario/AdministrarMarcacion.ashx` (+ `.ashx.cs`) — identidad, validación y respuesta.
- **Modificar** `ReporteTareas/clases/EnvioCorreoHelper.cs` — método `EnvioCorreoMarcacion`.
- **Modificar** `ReporteTareas/js/RegistroLaboral.js:46-70` — apuntar al handler nuevo.
- **Modificar** `CapaEntidad/CapaEntidad.csproj`, `CapaDato/CapaDato.csproj`, `CapaNegocio/CapaNegocio.csproj`, `ReporteTareas/ReporteTareas.csproj`.

---

## Task 1: Stored procedure `Sp_RTA_RegistrarMarcacion`

**Files:**
- Create: `docs/superpowers/plans/sql/2026-07-29-marcacion-registrar.sql`

**Interfaces:**
- Produces (para Task 2):
  - `Sp_RTA_RegistrarMarcacion(@Id_Usuario NUMERIC(6,0), @Accion INT)` → columnas `Respuestas INT`, `Mensaje VARCHAR(300)`, `FechaHora DATETIME`, `IdProceso BIGINT`.

**Supuesto verificable:** el `INSERT` omite `IdProceso` y usa `SCOPE_IDENTITY()`, igual que el SP existente `InsertarModificarEliminarRegistroBiometrico`, es decir, se asume que `RegistroBiometrico.IdProceso` es `IDENTITY`. Si el Step 3 devuelve un error de "cannot insert NULL into IdProceso", el supuesto es falso: confirmarlo con `SELECT COLUMNPROPERTY(OBJECT_ID('dbo.RegistroBiometrico'),'IdProceso','IsIdentity');` y detenerse a replantear con el usuario.

- [ ] **Step 1: Crear el script**

Crear `docs/superpowers/plans/sql/2026-07-29-marcacion-registrar.sql`:

```sql
/* ============================================================================
   Marcacion de entrada/salida — stored procedure
   Base: ReporTarea
   El servidor es el unico dueno de la decision: recibe @Accion explicita,
   resuelve la fila del dia por Id_Usuario + fecha, y toma la hora de GETDATE().
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_RegistrarMarcacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_RegistrarMarcacion;
GO
CREATE PROCEDURE dbo.Sp_RTA_RegistrarMarcacion
    @Id_Usuario NUMERIC(6,0),
    @Accion     INT              -- 1 = entrada, 2 = salida
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuestas INT          = 0;
    DECLARE @Mensaje    VARCHAR(300) = '';
    DECLARE @FechaHora  DATETIME     = NULL;
    DECLARE @IdProceso  BIGINT       = NULL;
    DECLARE @Ahora      DATETIME     = GETDATE();
    DECLARE @Hoy        DATE         = CONVERT(DATE, GETDATE());
    DECLARE @VACIO      DATETIME     = CONVERT(DATETIME, '1900-01-01');

    IF @Accion NOT IN (1, 2)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Accion no valida.',
               FechaHora = NULL, IdProceso = NULL;
        RETURN;
    END

    IF ISNULL(@Id_Usuario, 0) = 0
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Usuario no valido.',
               FechaHora = NULL, IdProceso = NULL;
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @FilaId      BIGINT   = NULL;
        DECLARE @FilaEntrada DATETIME = NULL;
        DECLARE @FilaSalida  DATETIME = NULL;

        /* UPDLOCK + HOLDLOCK: dos clics simultaneos no pueden crear dos filas */
        SELECT TOP 1
               @FilaId      = r.IdProceso,
               @FilaEntrada = r.FechaEntrada,
               @FilaSalida  = r.FechaSalida
        FROM dbo.RegistroBiometrico AS r WITH (UPDLOCK, HOLDLOCK)
        WHERE r.Id_Usuario = @Id_Usuario
          AND CONVERT(DATE, r.FechaRegistro) = @Hoy
        ORDER BY r.IdProceso DESC;

        IF @Accion = 1
        BEGIN
            IF @FilaId IS NOT NULL
            BEGIN
                SET @Respuestas = 0;
                SET @Mensaje = 'Ya registro su entrada hoy a las '
                             + CONVERT(VARCHAR(5), ISNULL(@FilaEntrada, @VACIO), 108) + '.';
            END
            ELSE
            BEGIN
                INSERT INTO dbo.RegistroBiometrico
                    (Id_Usuario, FechaEntrada, FechaAlmorzar, FechaRegAlmorzar,
                     FechaSalida, FechaRegistro, Estado)
                VALUES
                    (@Id_Usuario, @Ahora, @VACIO, @VACIO,
                     @VACIO, @Ahora, 1);

                SET @IdProceso  = SCOPE_IDENTITY();
                SET @FechaHora  = @Ahora;
                SET @Respuestas = 1;
                SET @Mensaje    = 'Entrada registrada a las '
                                + CONVERT(VARCHAR(5), @Ahora, 108) + '.';
            END
        END
        ELSE   /* @Accion = 2 */
        BEGIN
            IF @FilaId IS NULL
            BEGIN
                SET @Respuestas = 0;
                SET @Mensaje = 'Debe registrar primero su entrada.';
            END
            ELSE IF ISNULL(@FilaSalida, @VACIO) > @VACIO
            BEGIN
                SET @Respuestas = 0;
                SET @Mensaje = 'Ya registro su salida hoy a las '
                             + CONVERT(VARCHAR(5), @FilaSalida, 108) + '.';
            END
            ELSE
            BEGIN
                UPDATE dbo.RegistroBiometrico
                   SET FechaSalida = @Ahora
                 WHERE IdProceso = @FilaId;

                SET @IdProceso  = @FilaId;
                SET @FechaHora  = @Ahora;
                SET @Respuestas = 1;
                SET @Mensaje    = 'Salida registrada a las '
                                + CONVERT(VARCHAR(5), @Ahora, 108) + '.';
            END
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @FechaHora  = NULL;
        SET @IdProceso  = NULL;
        SET @Mensaje    = 'No se pudo registrar la marcacion: ' + ERROR_MESSAGE();
    END CATCH

    /* Invariante: Respuestas siempre 0 o 1, Mensaje siempre no vacio */
    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje,
           FechaHora = @FechaHora, IdProceso = @IdProceso;
END
GO
```

- [ ] **Step 2: Ejecutar el script**

Run:
```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas" && sqlcmd -S "tcp:192.168.11.14,1433" -U sa -P "$SQLPASS" -d ReporTarea -b -i "docs/superpowers/plans/sql/2026-07-29-marcacion-registrar.sql"
```
Expected: sin errores. Si da timeout de login, reintentar (la conectividad es intermitente).

- [ ] **Step 3: Verificar los 5 casos dentro de una transacción con ROLLBACK**

Este paso escribe en la BD de producción, por eso todo va dentro de `BEGIN TRANSACTION` / `ROLLBACK`.

Run (PowerShell):
```powershell
$q = @"
SET NOCOUNT ON;
BEGIN TRANSACTION;
DECLARE @U NUMERIC(6,0) = (SELECT TOP 1 Id_Usuario FROM dbo.RegistroBiometrico ORDER BY IdProceso DESC);
/* Aislar el caso: borrar (dentro de la transaccion) lo que el usuario tenga hoy */
DELETE FROM dbo.RegistroBiometrico WHERE Id_Usuario=@U AND CONVERT(DATE,FechaRegistro)=CONVERT(DATE,GETDATE());
PRINT '--- 1) salida sin entrada -> debe rechazar ---';
EXEC dbo.Sp_RTA_RegistrarMarcacion @Id_Usuario=@U, @Accion=2;
PRINT '--- 2) entrada nueva -> debe registrar ---';
EXEC dbo.Sp_RTA_RegistrarMarcacion @Id_Usuario=@U, @Accion=1;
PRINT '--- 3) entrada repetida -> debe rechazar ---';
EXEC dbo.Sp_RTA_RegistrarMarcacion @Id_Usuario=@U, @Accion=1;
PRINT '--- 4) salida valida -> debe registrar ---';
EXEC dbo.Sp_RTA_RegistrarMarcacion @Id_Usuario=@U, @Accion=2;
PRINT '--- 5) salida repetida -> debe rechazar ---';
EXEC dbo.Sp_RTA_RegistrarMarcacion @Id_Usuario=@U, @Accion=2;
PRINT '--- 6) accion invalida -> debe rechazar ---';
EXEC dbo.Sp_RTA_RegistrarMarcacion @Id_Usuario=@U, @Accion=9;
PRINT '--- filas creadas en la transaccion (debe ser 1) ---';
SELECT FilasHoy=COUNT(*) FROM dbo.RegistroBiometrico WHERE Id_Usuario=@U AND CONVERT(DATE,FechaRegistro)=CONVERT(DATE,GETDATE());
ROLLBACK TRANSACTION;
PRINT '--- ROLLBACK hecho ---';
"@; sqlcmd -S "tcp:192.168.11.14,1433" -U sa -P "$SQLPASS" -d ReporTarea -b -W -s "|" -l 30 -Q $q
```

Expected:
1. `Respuestas=0`, `Mensaje='Debe registrar primero su entrada.'`
2. `Respuestas=1`, `Mensaje='Entrada registrada a las HH:mm.'`, `FechaHora` y `IdProceso` no nulos
3. `Respuestas=0`, `Mensaje='Ya registro su entrada hoy a las HH:mm.'`
4. `Respuestas=1`, `Mensaje='Salida registrada a las HH:mm.'`
5. `Respuestas=0`, `Mensaje='Ya registro su salida hoy a las HH:mm.'`
6. `Respuestas=0`, `Mensaje='Accion no valida.'`
7. `FilasHoy=1` — una sola fila, nunca dos
8. `--- ROLLBACK hecho ---`

**En ningún caso `Mensaje` puede venir vacío o NULL.** Si alguno sale vacío, el SP tiene un hueco: corregirlo antes de seguir.

- [ ] **Step 4: Confirmar que no quedó nada en producción**

Run:
```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas" && sqlcmd -S "tcp:192.168.11.14,1433" -U sa -P "$SQLPASS" -d ReporTarea -W -s"|" -Q "SET NOCOUNT ON; SELECT TotalFilas=COUNT(*) FROM dbo.RegistroBiometrico;"
```
Expected: el mismo total que antes de la prueba (17.822 al momento de escribir este plan; el número crece con el uso normal, lo que importa es que la prueba no lo haya alterado ni borrado filas del día).

- [ ] **Step 5: Commit**

```bash
git add "docs/superpowers/plans/sql/2026-07-29-marcacion-registrar.sql"
git commit -m "feat(db): SP de marcacion con accion explicita y reglas de rechazo"
```

---

## Task 2: Capas C# (Entidad + Dato + Negocio)

**Files:**
- Create: `CapaEntidad/EntMarcacion.cs`
- Create: `CapaDato/DaoMarcacion.cs`
- Create: `CapaNegocio/NegMarcacion.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj`, `CapaDato/CapaDato.csproj`, `CapaNegocio/CapaNegocio.csproj`

**Interfaces:**
- Consumes (de Task 1): `Sp_RTA_RegistrarMarcacion(@Id_Usuario, @Accion)`.
- Produces (para Task 3):
  - `EntMarcacion { int Accion; int Respuestas; string Mensaje; DateTime FechaHora; long IdProceso; }`
  - `NegMarcacion.RegistrarMarcacion(decimal idUsuario, int accion)` → `EntMarcacion`

- [ ] **Step 1: Crear la entidad**

Crear `CapaEntidad/EntMarcacion.cs`:

```csharp
using System;

namespace CapaEntidad
{
    public class EntMarcacion
    {
        public int Accion { get; set; }
        public int Respuestas { get; set; }
        public string Mensaje { get; set; }
        public DateTime FechaHora { get; set; }
        public long IdProceso { get; set; }
    }
}
```

- [ ] **Step 2: Crear el Dao**

Crear `CapaDato/DaoMarcacion.cs`:

```csharp
using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoMarcacion
    {
        /// <summary>Registra la entrada (accion=1) o la salida (accion=2) del usuario.</summary>
        public static EntMarcacion RegistrarMarcacion(decimal idUsuario, int accion)
        {
            EntMarcacion resultado = new EntMarcacion()
            {
                Accion = accion,
                Respuestas = 0,
                Mensaje = "",
                FechaHora = Convert.ToDateTime("1900-01-01"),
                IdProceso = 0
            };

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_RegistrarMarcacion", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id_Usuario", SqlDbType.Decimal).Value = idUsuario;
                    cmd.Parameters.Add("@Accion", SqlDbType.Int).Value = accion;
                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            resultado.Respuestas = Convert.ToInt32(dr["Respuestas"].ToString());
                            resultado.Mensaje = dr["Mensaje"].ToString();

                            if (dr["FechaHora"] != DBNull.Value)
                            {
                                resultado.FechaHora = Convert.ToDateTime(dr["FechaHora"]);
                            }

                            if (dr["IdProceso"] != DBNull.Value)
                            {
                                resultado.IdProceso = Convert.ToInt64(dr["IdProceso"]);
                            }
                        }
                        else
                        {
                            resultado.Mensaje = "El procedimiento no devolvió información.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultado.Respuestas = 0;
                resultado.Mensaje = "Ocurrió un error al registrar la marcación. Detalle: " + ex.Message;
            }

            return resultado;
        }
    }
}
```

- [ ] **Step 3: Crear la capa de Negocio**

Crear `CapaNegocio/NegMarcacion.cs`:

```csharp
using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    public class NegMarcacion
    {
        public static EntMarcacion RegistrarMarcacion(decimal idUsuario, int accion)
        {
            return DaoMarcacion.RegistrarMarcacion(idUsuario, accion);
        }
    }
}
```

- [ ] **Step 4: Registrar los archivos en los `.csproj`**

Los proyectos de capa listan archivos con `<Compile Include>` explícitos. Agregar junto a las entradas existentes (ordenadas alfabéticamente):
- `CapaEntidad/CapaEntidad.csproj`: `<Compile Include="EntMarcacion.cs" />` (antes de `<Compile Include="EntMenuPerfil.cs" />`)
- `CapaDato/CapaDato.csproj`: `<Compile Include="DaoMarcacion.cs" />` (antes de `<Compile Include="DaoMenuPerfil.cs" />`)
- `CapaNegocio/CapaNegocio.csproj`: `<Compile Include="NegMarcacion.cs" />` (antes de `<Compile Include="NegMenuPerfil.cs" />`)

- [ ] **Step 5: Compilar**

Run (PowerShell):
```powershell
$msb = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"; & $msb "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly; "EXIT CODE: $LASTEXITCODE"
```
Expected: `EXIT CODE: 0`.

- [ ] **Step 6: Commit**

```bash
git add "CapaEntidad/EntMarcacion.cs" "CapaDato/DaoMarcacion.cs" "CapaNegocio/NegMarcacion.cs" "CapaEntidad/CapaEntidad.csproj" "CapaDato/CapaDato.csproj" "CapaNegocio/CapaNegocio.csproj"
git commit -m "feat(backend): capas de marcacion de entrada/salida"
```

---

## Task 3: Handler `AdministrarMarcacion.ashx`

**Files:**
- Create: `ReporteTareas/Formulario/AdministrarMarcacion.ashx`
- Create: `ReporteTareas/Formulario/AdministrarMarcacion.ashx.cs`
- Modify: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes (de Task 2): `NegMarcacion.RegistrarMarcacion(decimal, int)` → `EntMarcacion`.
- Produces (para Tasks 4 y 5): acción `RegistrarMarcacion` con parámetros `{session, Accion}`, respuesta `EntRespuesta { estado, mensaje, tipoMensaje, resultado }`.

- [ ] **Step 1: Crear el markup**

Crear `ReporteTareas/Formulario/AdministrarMarcacion.ashx`:

```
<%@ WebHandler Language="C#" CodeBehind="AdministrarMarcacion.ashx.cs" Class="JsonJQueryNetMarcacion.AdministrarMarcacion" %>
```

- [ ] **Step 2: Crear el code-behind**

Crear `ReporteTareas/Formulario/AdministrarMarcacion.ashx.cs`. El correo se agrega en Task 4; por ahora solo registra y responde.

```csharp
using CapaEntidad;
using CapaNegocio;
using SeguridadAppHelper;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetMarcacion
{
    /// <summary>
    /// Handler de la marcación de entrada/salida.
    /// La identidad sale SIEMPRE de la sesión cifrada, nunca del request en claro.
    /// Acción: RegistrarMarcacion.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarMarcacion : IHttpHandler
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

                if (Action == "RegistrarMarcacion")
                {
                    existAction = true;
                    responseAction.Append(RegistrarMarcacion(parameters));
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

        private string RegistrarMarcacion(dynamic campos)
        {
            try
            {
                int accion = 0;
                try { accion = Convert.ToInt32(campos["Accion"]); }
                catch { accion = 0; }

                if (accion != 1 && accion != 2)
                {
                    return responseMessage("0", "Acción no válida.", "warning");
                }

                SeguridadHelper seguridad = new SeguridadHelper();
                string codigoUsuario = seguridad.Desencripta(campos["session"]);

                EntUsuario usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(codigoUsuario);
                if (usuario == null || usuario.Id_Usuario <= 0)
                {
                    return responseMessage("0", "No se pudo identificar al usuario. Vuelva a iniciar sesión.", "danger");
                }

                EntMarcacion resultado = NegMarcacion.RegistrarMarcacion(
                    Convert.ToDecimal(usuario.Id_Usuario), accion);

                if (resultado.Respuestas == 1)
                {
                    return responseMessage("1", resultado.Mensaje, "success");
                }

                return responseMessage("0", resultado.Mensaje, "warning");
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al registrar la marcación. " + ex.Message, "danger");
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

En `ReporteTareas/ReporteTareas.csproj`, junto a `AdministrarMenuPerfil.ashx`:
```xml
    <Content Include="Formulario\AdministrarMarcacion.ashx" />
```
y
```xml
    <Compile Include="Formulario\AdministrarMarcacion.ashx.cs">
      <DependentUpon>AdministrarMarcacion.ashx</DependentUpon>
    </Compile>
```

- [ ] **Step 4: Compilar**

Run (PowerShell):
```powershell
$msb = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"; & $msb "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly; "EXIT CODE: $LASTEXITCODE"
```
Expected: `EXIT CODE: 0`.

- [ ] **Step 5: Commit**

```bash
git add "ReporteTareas/Formulario/AdministrarMarcacion.ashx" "ReporteTareas/Formulario/AdministrarMarcacion.ashx.cs" "ReporteTareas/ReporteTareas.csproj"
git commit -m "feat(handler): AdministrarMarcacion.ashx con accion explicita e identidad por sesion"
```

---

## Task 4: Correo de confirmación

**Files:**
- Modify: `ReporteTareas/clases/EnvioCorreoHelper.cs`
- Modify: `ReporteTareas/Formulario/AdministrarMarcacion.ashx.cs`

**Interfaces:**
- Consumes (de Task 3): `EntMarcacion.FechaHora`, `EntUsuario.E_Mail`, `EntUsuario.Nom_Usuario`.
- Produces: `EnvioCorreoHelper.EnvioCorreoMarcacion(string correoDestino, string nombreUsuario, int accion, DateTime fechaHora)` → `bool`.

- [ ] **Step 1: Verificar que existen los parámetros SMTP**

Sin estos parámetros el correo no sale. Verificar **antes** de escribir código.

Run (PowerShell):
```powershell
$q = @"
SET NOCOUNT ON;
EXEC dbo.Sp_RTAConsultaParametroConfiguracion @NombreParametro='smtpAddress';
EXEC dbo.Sp_RTAConsultaParametroConfiguracion @NombreParametro='emailFrom';
EXEC dbo.Sp_RTAConsultaParametroConfiguracion @NombreParametro='emailFromName';
EXEC dbo.Sp_RTAConsultaParametroConfiguracion @NombreParametro='portNumber';
EXEC dbo.Sp_RTAConsultaParametroConfiguracion @NombreParametro='enableSSL';
"@; sqlcmd -S "tcp:192.168.11.14,1433" -U sa -P "$SQLPASS" -d ReporTarea -W -s "|" -l 30 -Q $q
```
Expected: los cinco devuelven un `Valor` no vacío. **Si alguno falta o viene vacío, detenerse y reportarlo al usuario**: hay que cargarlo en la tabla de parámetros antes de que esta funcionalidad sirva de algo. `portNumber` debe ser un entero y `enableSSL` un valor que `Convert.ToBoolean` acepte (`True`/`False`).

- [ ] **Step 2: Agregar `EnvioCorreoMarcacion` a `EnvioCorreoHelper`**

En `ReporteTareas/clases/EnvioCorreoHelper.cs`, agregar el método dentro de la clase `EnvioCorreoHelper` (namespace `CorreoHelper`), junto a los otros `#region EnvioCorreo*`. El archivo ya tiene `using CapaNegocio;`, así que `NegParametrosConfiguracion` es alcanzable.

```csharp
        #region EnvioCorreoMarcacion
        /// <summary>Correo de confirmación de una marcación (accion: 1=entrada, 2=salida).</summary>
        public bool EnvioCorreoMarcacion(string correoDestino, string nombreUsuario, int accion, DateTime fechaHora)
        {
            EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
            bool respuestaEnvioCorreo = false;

            try
            {
                parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                string tipo = (accion == 1) ? "entrada" : "salida";
                string fecha = fechaHora.ToString("dd/MM/yyyy");
                string hora = fechaHora.ToString("HH:mm");

                string titulo = "Registro de " + tipo + " - " + fecha + " " + hora;

                string contenido =
                    "<p>Estimado(a) " + nombreUsuario + ",</p>" +
                    "<p>Se registró su <b>" + tipo + "</b> con los siguientes datos:</p>" +
                    "<table cellpadding='6' style='border-collapse:collapse'>" +
                    "<tr><td style='border:1px solid #ddd'><b>Tipo</b></td><td style='border:1px solid #ddd'>" + tipo + "</td></tr>" +
                    "<tr><td style='border:1px solid #ddd'><b>Fecha</b></td><td style='border:1px solid #ddd'>" + fecha + "</td></tr>" +
                    "<tr><td style='border:1px solid #ddd'><b>Hora</b></td><td style='border:1px solid #ddd'>" + hora + "</td></tr>" +
                    "</table>" +
                    "<p>Si usted no reconoce este registro, comuníquese con Talento Humano.</p>" +
                    "<p style='color:#888;font-size:11px'>Mensaje automático del Sistema de Gestión Interno. No responda a este correo.</p>";

                respuestaEnvioCorreo = EnviarCorreo(correoDestino, titulo, contenido, parametrosServidorCorreo);
            }
            catch (Exception ex)
            {
                ErrorProceso = ex.Message.ToString().Trim();
                respuestaEnvioCorreo = false;
            }

            return respuestaEnvioCorreo;
        }
        #endregion
```

- [ ] **Step 3: Encolar el correo en el handler**

En `ReporteTareas/Formulario/AdministrarMarcacion.ashx.cs`, agregar `using CorreoHelper;` a los `using` del archivo, y reemplazar el bloque de éxito del método `RegistrarMarcacion`.

Reemplazar:
```csharp
                if (resultado.Respuestas == 1)
                {
                    return responseMessage("1", resultado.Mensaje, "success");
                }
```

por:
```csharp
                if (resultado.Respuestas == 1)
                {
                    // Los datos se capturan ANTES de encolar: en el hilo del ThreadPool
                    // no existe HttpContext.Current y nada puede leerse de la sesión ahí.
                    string correoDestino = (usuario.E_Mail ?? string.Empty).Trim();
                    string nombreUsuario = (usuario.Nom_Usuario ?? string.Empty).Trim();
                    int accionCorreo = accion;
                    DateTime fechaHoraCorreo = resultado.FechaHora;

                    if (correoDestino != string.Empty)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(delegate
                        {
                            // try/catch TOTAL y obligatorio: una excepción sin capturar en un
                            // hilo del ThreadPool tumba el worker process de ASP.NET.
                            try
                            {
                                EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                                envioCorreo.EnvioCorreoMarcacion(correoDestino, nombreUsuario, accionCorreo, fechaHoraCorreo);
                            }
                            catch (Exception)
                            {
                            }
                        });
                    }

                    return responseMessage("1", resultado.Mensaje, "success");
                }
```

El correo se dispara **solo** tras `Respuestas == 1`, con la hora que devolvió el SP, y nunca bloquea la respuesta.

- [ ] **Step 4: Compilar**

Run (PowerShell):
```powershell
$msb = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"; & $msb "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly; "EXIT CODE: $LASTEXITCODE"
```
Expected: `EXIT CODE: 0`.

- [ ] **Step 5: Commit**

```bash
git add "ReporteTareas/clases/EnvioCorreoHelper.cs" "ReporteTareas/Formulario/AdministrarMarcacion.ashx.cs"
git commit -m "feat(correo): confirmacion de marcacion por correo, envio no bloqueante"
```

---

## Task 5: Frontend y verificación end-to-end

**Files:**
- Modify: `ReporteTareas/js/RegistroLaboral.js:46-70`

**Interfaces:**
- Consumes (de Tasks 3 y 4): acción `RegistrarMarcacion` de `AdministrarMarcacion.ashx`.

- [ ] **Step 1: Apuntar el JS al handler nuevo**

En `ReporteTareas/js/RegistroLaboral.js`, dentro de `function RegistrarEvento(Accion)`.

Reemplazar:
```javascript
    var url = "ObtenerListaTareas.ashx";
```
por:
```javascript
    var url = "AdministrarMarcacion.ashx";
```

Reemplazar el armado del cuerpo (deja de enviarse `IdProceso`: la fila la resuelve el servidor):
```javascript
    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + valor + "',";
    datosFormulario = datosFormulario + "'IdProceso': '" + IdProceso + "',";
    datosFormulario = datosFormulario + "'Accion': '" + Accion + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'RegistrarEvento', 'parameters' : " + datosFormulario + " }]";
```
por:
```javascript
    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + valor + "',";
    datosFormulario = datosFormulario + "'Accion': '" + Accion + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'RegistrarMarcacion', 'parameters' : " + datosFormulario + " }]";
```

**No tocar** `ConsultarEvento` ni `RecorreJSONTable`: siguen leyendo de `ObtenerListaTareas.ashx` y siguen manejando el estado visual de los botones. La variable global `IdProceso` queda sin uso en la escritura, lo cual es intencional.

El manejo de respuesta existente ya sirve tal cual: `estado=="1"` → `MensajeCorrecto`, `estado=="0"` → `MensajeIncorrecto`. Como el SP siempre devuelve 1 o 0 con mensaje, la falla silenciosa desaparece sin cambiar esa lógica.

- [ ] **Step 2: Verificar que el BOM del JS sigue intacto**

Run (PowerShell):
```powershell
$f = (Resolve-Path "ReporteTareas\js\RegistroLaboral.js")
$b = [System.IO.File]::ReadAllBytes($f)
if (-not ($b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF)) {
  $bom = [byte[]](0xEF,0xBB,0xBF)
  $out = New-Object byte[] ($bom.Length + $b.Length)
  [Array]::Copy($bom,0,$out,0,3); [Array]::Copy($b,0,$out,3,$b.Length)
  [System.IO.File]::WriteAllBytes($f,$out); "BOM re-agregado"
} else { "BOM OK" }
```
Expected: `BOM OK` (el archivo ya lo tenía) o `BOM re-agregado`.

- [ ] **Step 3: Compilar**

Run (PowerShell):
```powershell
$msb = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"; & $msb "ReporteTareas.sln" /t:Build /p:Configuration=Debug /verbosity:minimal /nologo /clp:ErrorsOnly; "EXIT CODE: $LASTEXITCODE"
```
Expected: `EXIT CODE: 0`.

- [ ] **Step 4: Commit**

```bash
git add "ReporteTareas/js/RegistroLaboral.js"
git commit -m "fix(marcacion): el JS envia la accion al handler nuevo y ya no manda IdProceso"
```

- [ ] **Step 5: Verificación en navegador (entorno del usuario, con BD y sesión)**

Este paso lo ejecuta el usuario en su entorno; requiere sesión iniciada y correo configurado.

Abrir `Principal.aspx` con sesión iniciada y **Ctrl+F5** (para saltar el caché del `.js`). Comprobar:

1. **Entrada:** presionar "Entrada" → confirmar en el diálogo → mensaje *"Entrada registrada a las HH:mm."* y **llega un correo** con esa misma hora.
2. **Entrada repetida:** volver a presionar "Entrada" (si el botón quedó deshabilitado, recargar la página) → mensaje *"Ya registro su entrada hoy a las HH:mm."*, y **no llega** un segundo correo.
3. **Salida:** presionar "Salida" → mensaje *"Salida registrada a las HH:mm."* y **llega el segundo correo**.
4. **Salida repetida:** volver a presionar "Salida" → mensaje *"Ya registro su salida hoy a las HH:mm."* — antes de este cambio, aquí no aparecía **nada**.
5. Confirmar en BD que quedó **una sola fila** del día con entrada y salida correctas:
```bash
sqlcmd -S "tcp:192.168.11.14,1433" -U sa -P "$SQLPASS" -d ReporTarea -W -s"|" -Q "SET NOCOUNT ON; SELECT IdProceso, Id_Usuario, FechaEntrada, FechaSalida FROM dbo.RegistroBiometrico WHERE CONVERT(DATE,FechaRegistro)=CONVERT(DATE,GETDATE()) ORDER BY IdProceso DESC;"
```

- [ ] **Step 6: Orden de despliegue a producción**

Desplegar en este orden: **SP → binarios (`bin/`) → `RegistroLaboral.js`**. Si el `.js` sale antes que los binarios, las marcaciones fallan con error de red porque `AdministrarMarcacion.ashx` todavía no existe en el servidor.

---

## Notas

- **Contención de bloqueos:** el `WHERE CONVERT(DATE, r.FechaRegistro) = @Hoy` no es sargable, así que el `UPDLOCK, HOLDLOCK` puede tomar un rango amplio. Cada transacción dura milisegundos, y sobre 17.822 filas es aceptable. Si en producción aparece contención en el pico de marcación de la mañana, el siguiente paso es un índice no único sobre `(Id_Usuario, FechaRegistro)`. **No se agrega en este plan** porque no estaba en el alcance aprobado.
- **Lo que este plan no arregla,** y sigue pendiente de su propio ciclo: los 31 días duplicados y las 5 salidas inválidas ya existentes, la falta de auditoría (quién/IP/fecha de modificación), y el hueco de autorización de `RegistrarEvento2/3`, que aceptan el código de usuario en texto plano.

## Self-Review (cobertura del spec)

- **SP nuevo con `@Accion` explícita, `UPDLOCK/HOLDLOCK`, `GETDATE()` única fuente de tiempo** → Task 1, Step 1. ✅
- **Las 5 reglas de la tabla del spec + acción inválida** → Task 1, Steps 1 y 3. ✅
- **Invariante "siempre 0 o 1 con mensaje no vacío"** → Task 1, Step 1 (`CASE` reemplazado por asignaciones explícitas) y Step 3 (verificación). ✅
- **`IdProceso` del cliente fuera del flujo de escritura** → Task 1 (el SP resuelve la fila) + Task 5, Step 1 (el JS deja de enviarlo). ✅
- **Capas `EntMarcacion`/`DaoMarcacion`/`NegMarcacion`** → Task 2. ✅
- **Handler con identidad por sesión cifrada y UTF-8** → Task 3, Step 2. ✅
- **`EnvioCorreoMarcacion` reutilizando `EnviarCorreo` y `NegParametrosConfiguracion`** → Task 4, Step 2. ✅
- **Envío no bloqueante con las 3 condiciones (captura previa, try/catch total, solo si hay correo y éxito)** → Task 4, Step 3. ✅
- **Verificación de parámetros SMTP pendiente del spec** → Task 4, Step 1, con instrucción de detenerse si faltan. ✅
- **`ConsultarEvento`, `RegistrarEvento/2/3`, `RegistroAsistencia` y el SP viejo intactos** → ningún task los toca; explicitado en Global Constraints y en Task 5, Step 1. ✅
- **Verificación: MSBuild, sqlcmd con ROLLBACK, navegador** → Tasks 1-5. ✅
- **Orden de despliegue** → Task 5, Step 6. ✅

Firmas consistentes entre tareas: `RegistrarMarcacion(decimal idUsuario, int accion)` → `EntMarcacion` en Dao y Negocio; `EnvioCorreoMarcacion(string, string, int, DateTime)` → `bool`; acción `RegistrarMarcacion {session, Accion}`; `Respuestas` 1=éxito / 0=rechazo. Sin placeholders.
