# Administración de perfiles — plan de implementación

> **Para quien ejecute este plan:** SUB-SKILL REQUERIDA: usar
> `superpowers:subagent-driven-development` (recomendado) o
> `superpowers:executing-plans` para implementarlo tarea por tarea. Los pasos usan
> casillas (`- [ ]`) para llevar el control.

**Objetivo:** Dar una pantalla que liste, cree, edite y elimine perfiles, donde eliminar
se bloquea si el perfil todavía tiene usuarios asignados.

**Arquitectura:** Se sigue el patrón de la Administración de Usuarios: una pantalla
WebForms sin code-behind de lógica, un handler `.ashx` con sesión obligatoria que
despacha por acción, y las capas `CapaNegocio` → `CapaDato` → procedimiento almacenado.
Toda la validación de negocio vive en el procedimiento almacenado.

**Tecnologías:** ASP.NET WebForms (.NET Framework), SQL Server, jQuery, Bootstrap 3,
SweetAlert.

## Restricciones globales

- Base de datos `ReporTarea` en `192.168.11.14`. Credenciales en
  `CapaDato/DaoReporTareaAranda.cs`.
- El catálogo de perfiles es **`dbo.Perfiles`**, nunca `dbo.R_Perfil`.
- En `PerfilMenu` la columna es **`IdPerfil`**, sin guion bajo. En `R_Usuarios` es
  **`Id_Perfil`**, con guion bajo. No son intercambiables.
- En `PerfilMenu`, `Estado='0'` **muestra** la opción y `Estado='1'` la **oculta**.
  La semántica está invertida respecto de lo que sugiere el nombre.
- `sqlcmd` deja `QUOTED_IDENTIFIER` en OFF. Todo script SQL empieza con
  `SET QUOTED_IDENTIFIER ON;`.
- No se toca el markup de otras pantallas ni `dos-tema.css`.
- Los mensajes al usuario van en español, en segunda persona formal ("Reasígnelos"),
  igual que el resto de la aplicación.
- Cada archivo estático nuevo o modificado que cargue una página obliga a subir su `?v=`
  en la página que lo referencia.

---

## Tarea 0: Confirmar el esquema real contra la base

El spec dejó dos supuestos sin verificar porque la base no respondía desde el entorno de
desarrollo. **Si alguno resulta falso, las tareas siguientes cambian.** Esta tarea no
produce código: produce certeza.

**Archivos:**
- Crear: `docs/superpowers/plans/sql/2026-08-10-perfiles-reconocimiento.sql`

**Interfaces:**
- Produce: los nombres exactos de columnas de `dbo.Perfiles` y de la tabla de página de
  inicio, y la lista de claves foráneas hacia `dbo.Perfiles`. Las tareas 1 y 2 dependen
  de esto.

- [ ] **Paso 1: Escribir el script de reconocimiento**

```sql
SET QUOTED_IDENTIFIER ON;
USE ReporTarea;
GO

-- 1. Columnas reales de dbo.Perfiles
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Perfiles'
ORDER BY ORDINAL_POSITION;

-- 2. Cómo se llama de verdad la tabla de página de inicio por perfil
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%PerfilInicio%' OR TABLE_NAME LIKE '%Inicio%';

-- 3. Sus columnas (ajustar el nombre con el resultado de la consulta 2)
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PerfilInicio'
ORDER BY ORDINAL_POSITION;

-- 4. TODA referencia declarada hacia dbo.Perfiles.
--    Si esto devuelve 0 filas NO hay red de seguridad por integridad referencial,
--    y el paso 5 pasa a ser obligatorio.
SELECT
    OBJECT_NAME(fk.parent_object_id)          AS TablaQueReferencia,
    COL_NAME(fkc.parent_object_id,
             fkc.parent_column_id)            AS ColumnaQueReferencia
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
WHERE OBJECT_NAME(fk.referenced_object_id) = 'Perfiles';

-- 5. Búsqueda a mano de otras tablas que guarden un id de perfil.
--    Hace falta porque este esquema casi no declara claves foráneas.
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME IN ('IdPerfil', 'Id_Perfil', 'IdPerfiles', 'Perfil')
ORDER BY TABLE_NAME;

-- 6. Cuántos usuarios tiene hoy cada perfil. Sirve para elegir un perfil
--    de prueba que esté vacío en la tarea 1.
SELECT p.IdPerfiles, p.NombrePerfil, COUNT(u.Id_Usuario) AS Usuarios
FROM dbo.Perfiles p
LEFT JOIN R_Usuarios u ON u.Id_Perfil = p.IdPerfiles
GROUP BY p.IdPerfiles, p.NombrePerfil
ORDER BY Usuarios DESC, p.IdPerfiles;
```

- [ ] **Paso 2: Ejecutarlo**

```bash
sqlcmd -S 192.168.11.14 -U sa -P "<clave de DaoReporTareaAranda.cs>" -d ReporTarea \
  -i docs/superpowers/plans/sql/2026-08-10-perfiles-reconocimiento.sql
```

- [ ] **Paso 3: Contrastar con el spec y anotar las diferencias**

Escribir al final del propio archivo `.sql`, como comentario, el resultado de las seis
consultas. Si la consulta 5 devuelve una tabla que el spec no mencionó, **detenerse y
avisar**: hay una cuarta referencia y el diseño del borrado cambia.

- [ ] **Paso 4: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-10-perfiles-reconocimiento.sql
git commit -m "docs(perfiles): reconocimiento del esquema real de perfiles"
```

---

## Tarea 1: Procedimiento `Sp_RTA_EliminarPerfil`

**Archivos:**
- Crear: `docs/superpowers/plans/sql/2026-08-10-perfiles-eliminar-sp.sql`

**Interfaces:**
- Consume: los nombres de columna confirmados en la Tarea 0.
- Produce: `Sp_RTA_EliminarPerfil @IdPerfiles int`, que devuelve una fila con las columnas
  `Respuestas int` (1 = borrado, 0 = rechazado) y `Mensaje varchar(300)`. La Tarea 2 lee
  exactamente esas dos columnas.

- [ ] **Paso 1: Escribir la prueba que debe fallar**

Crear `docs/superpowers/plans/sql/2026-08-10-perfiles-eliminar-prueba.sql`. Se ejecuta
antes de crear el procedimiento y **debe fallar** porque el procedimiento no existe.

```sql
SET QUOTED_IDENTIFIER ON;
USE ReporTarea;
GO

-- Perfil de prueba, sin usuarios, con un menú y una página de inicio colgando.
DECLARE @idPrueba int;

INSERT INTO dbo.Perfiles (NombrePerfil, Estado) VALUES ('ZZ_PRUEBA_BORRAR', 1);
SET @idPrueba = SCOPE_IDENTITY();

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, Estado) VALUES (20042, @idPrueba, '0');

PRINT '--- CASO A: perfil sin usuarios, debe borrarse ---';
EXEC Sp_RTA_EliminarPerfil @IdPerfiles = @idPrueba;

PRINT '--- Debe devolver 0 filas en las tres tablas ---';
SELECT COUNT(*) AS PerfilQueQuedo   FROM dbo.Perfiles   WHERE IdPerfiles = @idPrueba;
SELECT COUNT(*) AS MenusQueQuedaron FROM dbo.PerfilMenu WHERE IdPerfil   = @idPrueba;

PRINT '--- CASO B: perfil CON usuarios, debe rechazarse ---';
-- Se usa un perfil real poblado; el 18 (Super Admin) tiene usuarios.
EXEC Sp_RTA_EliminarPerfil @IdPerfiles = 18;

PRINT '--- El perfil 18 TIENE que seguir existiendo ---';
SELECT COUNT(*) AS Perfil18SigueVivo FROM dbo.Perfiles WHERE IdPerfiles = 18;

PRINT '--- CASO C: perfil inexistente, debe rechazarse sin reventar ---';
EXEC Sp_RTA_EliminarPerfil @IdPerfiles = 999999;
```

- [ ] **Paso 2: Ejecutarla para verificar que falla**

```bash
sqlcmd -S 192.168.11.14 -U sa -P "<clave>" -d ReporTarea \
  -i docs/superpowers/plans/sql/2026-08-10-perfiles-eliminar-prueba.sql
```

Esperado: FALLA con `Could not find stored procedure 'Sp_RTA_EliminarPerfil'`.

- [ ] **Paso 3: Escribir el procedimiento**

```sql
SET QUOTED_IDENTIFIER ON;
USE ReporTarea;
GO

IF OBJECT_ID('dbo.Sp_RTA_EliminarPerfil', 'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_EliminarPerfil;
GO

/* ============================================================================
   Elimina un perfil del catalogo dbo.Perfiles.

   Se BLOQUEA si el perfil todavia tiene usuarios asignados: borrarlo los
   dejaria apuntando a un perfil inexistente y entrarian al sistema sin menu.

   Si esta libre, se borra junto con su configuracion (menus y pagina de
   inicio) dentro de una transaccion. Un borrado a medias es peor que no
   borrar: dejaria el perfil vivo pero sin menus, o los menus sin perfil.
   ============================================================================ */
CREATE PROCEDURE dbo.Sp_RTA_EliminarPerfil
    @IdPerfiles int
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @usuarios int;

    IF NOT EXISTS (SELECT 1 FROM dbo.Perfiles WHERE IdPerfiles = @IdPerfiles)
    BEGIN
        SELECT 0 AS Respuestas, 'El perfil ya no existe.' AS Mensaje;
        RETURN;
    END

    /* Se cuentan TODOS los usuarios, activos e inactivos. Un usuario inactivo
       sigue siendo una referencia: si se reactiva despues de borrar el perfil,
       queda sin menu. */
    SELECT @usuarios = COUNT(*)
    FROM R_Usuarios
    WHERE Id_Perfil = @IdPerfiles;

    IF @usuarios > 0
    BEGIN
        SELECT 0 AS Respuestas,
               'Este perfil tiene ' + CAST(@usuarios AS varchar(10)) +
               ' usuarios asignados. Reasignelos antes de eliminarlo.' AS Mensaje;
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

            DELETE FROM dbo.PerfilMenu   WHERE IdPerfil   = @IdPerfiles;
            DELETE FROM dbo.PerfilInicio WHERE IdPerfil   = @IdPerfiles;
            DELETE FROM dbo.Perfiles     WHERE IdPerfiles = @IdPerfiles;

        COMMIT TRANSACTION;

        SELECT 1 AS Respuestas, 'El perfil se elimino correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT 0 AS Respuestas,
               'No se pudo eliminar el perfil: ' + ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO
```

**Antes de ejecutar:** ajustar `dbo.PerfilInicio` y su columna `IdPerfil` con lo que haya
devuelto la Tarea 0. Si esa tabla tiene otro nombre, corregirlo acá.

- [ ] **Paso 4: Crear el procedimiento y volver a correr la prueba**

```bash
sqlcmd -S 192.168.11.14 -U sa -P "<clave>" -d ReporTarea \
  -i docs/superpowers/plans/sql/2026-08-10-perfiles-eliminar-sp.sql

sqlcmd -S 192.168.11.14 -U sa -P "<clave>" -d ReporTarea \
  -i docs/superpowers/plans/sql/2026-08-10-perfiles-eliminar-prueba.sql
```

Esperado:
- CASO A → `Respuestas = 1`, y `PerfilQueQuedo = 0`, `MenusQueQuedaron = 0`
- CASO B → `Respuestas = 0`, mensaje con el número de usuarios, y `Perfil18SigueVivo = 1`
- CASO C → `Respuestas = 0`, mensaje "El perfil ya no existe.", sin error de servidor

- [ ] **Paso 5: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-10-perfiles-eliminar-sp.sql \
        docs/superpowers/plans/sql/2026-08-10-perfiles-eliminar-prueba.sql
git commit -m "feat(perfiles): SP de eliminar perfil, bloqueado si tiene usuarios"
```

---

## Tarea 2: Procedimiento `Sp_RTA_ListarPerfilesAdmin`

El listado necesita la cantidad de usuarios por perfil: es lo que permite ver de antemano
qué perfiles se pueden eliminar. `Sp_RTA_ListarPerfiles` ya existe pero no trae ese dato
y lo usan otras pantallas, así que **no se toca**: se agrega uno nuevo.

**Archivos:**
- Crear: `docs/superpowers/plans/sql/2026-08-10-perfiles-listar-sp.sql`

**Interfaces:**
- Produce: `Sp_RTA_ListarPerfilesAdmin @filtro varchar(100)`, que devuelve
  `IdPerfiles int`, `NombrePerfil varchar`, `Estado int`, `Usuarios int`. La Tarea 3 mapea
  esas cuatro columnas.

- [ ] **Paso 1: Escribir el procedimiento**

```sql
SET QUOTED_IDENTIFIER ON;
USE ReporTarea;
GO

IF OBJECT_ID('dbo.Sp_RTA_ListarPerfilesAdmin', 'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPerfilesAdmin;
GO

/* Lista el catalogo con la cantidad de usuarios de cada perfil. Ese conteo es
   lo que hace utilizable el boton Eliminar: se ve antes de intentarlo.
   Se cuentan todos los usuarios, activos e inactivos, por el mismo motivo que
   en Sp_RTA_EliminarPerfil. */
CREATE PROCEDURE dbo.Sp_RTA_ListarPerfilesAdmin
    @filtro varchar(100) = ''
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.IdPerfiles,
        p.NombrePerfil,
        p.Estado,
        COUNT(u.Id_Usuario) AS Usuarios
    FROM dbo.Perfiles p
    LEFT JOIN R_Usuarios u ON u.Id_Perfil = p.IdPerfiles
    WHERE (@filtro = '' OR p.NombrePerfil LIKE '%' + @filtro + '%')
    GROUP BY p.IdPerfiles, p.NombrePerfil, p.Estado
    ORDER BY p.NombrePerfil;
END
GO
```

- [ ] **Paso 2: Crearlo y comprobar que el conteo es correcto**

```bash
sqlcmd -S 192.168.11.14 -U sa -P "<clave>" -d ReporTarea \
  -i docs/superpowers/plans/sql/2026-08-10-perfiles-listar-sp.sql

sqlcmd -S 192.168.11.14 -U sa -P "<clave>" -d ReporTarea -Q \
  "EXEC Sp_RTA_ListarPerfilesAdmin ''"
```

Esperado: una fila por perfil. El total de la columna `Usuarios` tiene que coincidir con
la consulta 6 de la Tarea 0.

- [ ] **Paso 3: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-10-perfiles-listar-sp.sql
git commit -m "feat(perfiles): SP de listado con cantidad de usuarios por perfil"
```

---

## Tarea 3: Capa de datos y de negocio

**Archivos:**
- Modificar: `CapaEntidad/EntPerfiles.cs` — agregar la propiedad `Usuarios`
- Modificar: `CapaDato/DaoPerfiles.cs` — agregar dos métodos
- Modificar: `CapaNegocio/NegPerfiles.cs` — agregar los dos pasos correspondientes

**Interfaces:**
- Consume: `Sp_RTA_EliminarPerfil` y `Sp_RTA_ListarPerfilesAdmin` de las tareas 1 y 2.
- Produce:
  - `NegPerfiles.ListarPerfilesAdmin(string filtro)` → `List<EntPerfiles>`
  - `NegPerfiles.EliminarPerfil(int idPerfil)` → `EntRespuesta`

  La Tarea 4 llama exactamente a esos dos nombres.

- [ ] **Paso 1: Agregar la propiedad a la entidad**

En `CapaEntidad/EntPerfiles.cs`, dentro de la clase, junto a las demás propiedades:

```csharp
        // Cantidad de usuarios que tienen este perfil. La usa la pantalla de
        // administracion para saber si el perfil se puede eliminar.
        public Int32 Usuarios { get; set; }
```

- [ ] **Paso 2: Agregar los métodos a `CapaDato/DaoPerfiles.cs`**

Dentro de la clase `DaoPerfiles`, siguiendo el estilo de los métodos que ya están:

```csharp
        public static List<EntPerfiles> ListarPerfilesAdmin(string filtro)
        {
            List<EntPerfiles> lista = new List<EntPerfiles>();

            DaoReporTareaAranda cn = new DaoReporTareaAranda();
            using (SqlConnection cnx = cn.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarPerfilesAdmin", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntPerfiles()
                        {
                            IdPerfil = Convert.ToInt32(dr["IdPerfiles"]),
                            NombrePerfil = Convert.ToString(dr["NombrePerfil"]),
                            Estado = Convert.ToInt32(dr["Estado"]),
                            Usuarios = Convert.ToInt32(dr["Usuarios"])
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Elimina un perfil. Quien decide si se puede es el procedimiento
        /// almacenado, no este metodo: la validacion tiene que estar donde no se
        /// la pueda saltar llamando al handler por HTTP.
        /// </summary>
        public static EntRespuesta EliminarPerfil(int idPerfil)
        {
            EntRespuesta respuesta = new EntRespuesta();

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                using (SqlConnection cnx = cn.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_EliminarPerfil", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdPerfiles", SqlDbType.Int).Value = idPerfil;
                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            int codigo = Convert.ToInt32(dr["Respuestas"]);
                            respuesta.estado = codigo == 1 ? "1" : "0";
                            respuesta.mensaje = Convert.ToString(dr["Mensaje"]);
                            respuesta.tipoMensaje = codigo == 1 ? "success" : "warning";
                            respuesta.resultado = codigo.ToString();
                        }
                        else
                        {
                            respuesta.estado = "0";
                            respuesta.mensaje = "El servidor no devolvio respuesta.";
                            respuesta.tipoMensaje = "danger";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Error al eliminar el perfil. " + ex.Message;
                respuesta.tipoMensaje = "danger";
            }

            return respuesta;
        }
```

- [ ] **Paso 3: Agregar los pasos en `CapaNegocio/NegPerfiles.cs`**

Dentro de la clase `NegPerfiles`:

```csharp
        public static List<EntPerfiles> ListarPerfilesAdmin(string filtro)
        {
            return DaoPerfiles.ListarPerfilesAdmin(filtro);
        }

        public static EntRespuesta EliminarPerfil(int idPerfil)
        {
            return DaoPerfiles.EliminarPerfil(idPerfil);
        }
```

- [ ] **Paso 4: Compilar**

```bash
msbuild "ReporteTareas.sln" /p:Configuration=Release /v:minimal
```

Esperado: EXIT 0, sin errores.

**Ojo:** existe `CapaNegocio/CapaDato/DaoPerfiles.cs`, una copia del archivo dentro de
otro proyecto. Verificar cuál de los dos compila la solución antes de editar; si compilan
los dos, aplicar el cambio en ambos.

- [ ] **Paso 5: Commit**

```bash
git add CapaEntidad/EntPerfiles.cs CapaDato/DaoPerfiles.cs CapaNegocio/NegPerfiles.cs
git commit -m "feat(perfiles): capa de datos y negocio para listar y eliminar"
```

---

## Tarea 4: Handler `AdministrarPerfiles.ashx`

**Archivos:**
- Crear: `ReporteTareas/Formulario/AdministrarPerfiles.ashx`
- Crear: `ReporteTareas/Formulario/AdministrarPerfiles.ashx.cs`
- Modificar: `ReporteTareas/ReporteTareas.csproj` — declarar los dos archivos

**Interfaces:**
- Consume: `NegPerfiles.ListarPerfilesAdmin(string)` y `NegPerfiles.EliminarPerfil(int)`
  de la Tarea 3.
- Produce: el endpoint `AdministrarPerfiles.ashx` con las acciones `ListarPerfiles`,
  `GuardarPerfil` y `EliminarPerfil`. La Tarea 5 las llama por esos nombres exactos.

- [ ] **Paso 1: Crear `AdministrarPerfiles.ashx`**

```
<%@ WebHandler Language="C#" CodeBehind="AdministrarPerfiles.ashx.cs" Class="JsonJQueryNetPerfiles.AdministrarPerfiles" %>
```

- [ ] **Paso 2: Crear `AdministrarPerfiles.ashx.cs`**

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetPerfiles
{
    /// <summary>
    /// Handler de la pantalla "Administración de perfiles".
    /// Acciones: ListarPerfiles, GuardarPerfil, EliminarPerfil.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarPerfiles : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            // Sin sesión no se ejecuta nada: es una pantalla de administración.
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

                var parameters = parametros[0]["parameters"];
                var Action = parametros[0]["action"];
                bool existAction = false;

                if (Action == "ListarPerfiles")
                {
                    existAction = true;
                    responseAction.Append(ListarPerfiles(parameters));
                }

                if (Action == "GuardarPerfil")
                {
                    existAction = true;
                    responseAction.Append(GuardarPerfil(parameters));
                }

                if (Action == "EliminarPerfil")
                {
                    existAction = true;
                    responseAction.Append(EliminarPerfil(parameters));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }
            else
            {
                responseAction.Append(responseMessage("0", "Solicitud no válida.", "danger"));
            }

            context.Response.ContentType = "application/json";
            context.Response.Write(responseAction.ToString());
        }

        private string ListarPerfiles(dynamic campos)
        {
            try
            {
                return ToJson(NegPerfiles.ListarPerfilesAdmin(Texto(campos, "filtro")));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al listar los perfiles. " + ex.Message, "danger");
            }
        }

        private string GuardarPerfil(dynamic campos)
        {
            try
            {
                string nombre = Texto(campos, "nombrePerfil");
                if (nombre == string.Empty)
                {
                    return responseMessage("0", "Debe escribir el nombre del perfil.", "warning");
                }

                EntPerfiles perfil = new EntPerfiles();
                perfil.IdPerfil = Entero(campos, "idPerfil");
                perfil.NombrePerfil = nombre;
                perfil.Estado = Entero(campos, "estado");
                perfil.Fecha = DateTime.Now;

                // Id 0 significa alta; cualquier otro, modificación.
                if (perfil.IdPerfil == 0)
                {
                    return ToJson(NegPerfiles.RTAInsertarNuevoPerfil(perfil));
                }

                return ToJson(NegPerfiles.Sp_RTActualizarPerfil(perfil));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar el perfil. " + ex.Message, "danger");
            }
        }

        private string EliminarPerfil(dynamic campos)
        {
            try
            {
                int idPerfil = Entero(campos, "idPerfil");
                if (idPerfil <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un perfil.", "warning");
                }

                // Quien decide si se puede borrar es el SP. Acá no se repite la
                // validación: repetirla invita a que las dos versiones se separen.
                return ToJson(NegPerfiles.EliminarPerfil(idPerfil));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar el perfil. " + ex.Message, "danger");
            }
        }

        private string Texto(dynamic campos, string clave)
        {
            try
            {
                object valor = campos[clave];
                if (valor == null) { return string.Empty; }
                return Convert.ToString(valor).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private int Entero(dynamic campos, string clave)
        {
            try
            {
                int valor;
                if (!int.TryParse(Convert.ToString(campos[clave]), out valor)) { return 0; }
                return valor;
            }
            catch
            {
                return 0;
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

- [ ] **Paso 3: Declarar los archivos en el `.csproj`**

En `ReporteTareas/ReporteTareas.csproj`, junto a las entradas de
`AdministrarUsuarios.ashx`:

```xml
    <Content Include="Formulario\AdministrarPerfiles.ashx" />
```

y en el `ItemGroup` de compilación:

```xml
    <Compile Include="Formulario\AdministrarPerfiles.ashx.cs">
      <DependentUpon>AdministrarPerfiles.ashx</DependentUpon>
    </Compile>
```

- [ ] **Paso 4: Averiguar qué espera `Sp_RTAInsertaNuevoPerfil` en `@Codigo`**

`DaoPerfiles.RTAInsertarNuevoPerfil` le pasa cuatro parámetros: `@Codigo`,
`@NombrePerfil`, `@Estado` y `@Fecha`. El handler de arriba **no fija `Codigo`**, así que
llega en 0. Antes de dar por buena el alta hay que ver qué hace el procedimiento con ese
valor:

```bash
sqlcmd -S 192.168.11.14 -U sa -P "<clave>" -d ReporTarea -Q \
  "EXEC sp_helptext 'Sp_RTAInsertaNuevoPerfil'"
```

Según lo que devuelva:

- Si `@Codigo` se ignora o admite 0 → dejar el handler como está y anotarlo en el
  comentario del método `GuardarPerfil`.
- Si `@Codigo` se inserta en una columna con significado → agregar el campo al formulario
  de la Tarea 5 y a `parametros` en `GuardarPerfil`, con esta línea antes de la llamada:

```csharp
                perfil.Codigo = Entero(campos, "codigo");
```

No dar por sentado que un 0 es inofensivo: si esa columna alimenta algún filtro, todos
los perfiles nuevos saldrían con el mismo código.

- [ ] **Paso 5: Compilar**

```bash
msbuild "ReporteTareas.sln" /p:Configuration=Release /v:minimal
```

Esperado: EXIT 0.

- [ ] **Paso 6: Probar que sin sesión rechaza**

Con el sitio corriendo, desde otra terminal:

```bash
curl -s -X POST http://localhost:59044/Formulario/AdministrarPerfiles.ashx \
  -H "Content-Type: application/json" \
  -d '[{"action":"EliminarPerfil","parameters":{"idPerfil":18}}]'
```

Esperado: la respuesta contiene `"Su sesión expiró"` y el perfil 18 sigue existiendo.
Esta prueba es la que demuestra que la validación no depende del navegador.

- [ ] **Paso 7: Commit**

```bash
git add ReporteTareas/Formulario/AdministrarPerfiles.ashx \
        ReporteTareas/Formulario/AdministrarPerfiles.ashx.cs \
        ReporteTareas/ReporteTareas.csproj
git commit -m "feat(perfiles): handler AdministrarPerfiles con sesion obligatoria"
```

---

## Tarea 5: Pantalla y JavaScript

**Archivos:**
- Crear: `ReporteTareas/Formulario/ParametrizacionPerfiles.aspx`
- Crear: `ReporteTareas/Formulario/ParametrizacionPerfiles.aspx.cs`
- Crear: `ReporteTareas/Formulario/ParametrizacionPerfiles.aspx.designer.cs`
- Crear: `ReporteTareas/js/parametrizacionPerfiles.js`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consume: las tres acciones del handler de la Tarea 4.
- Produce: la pantalla `ParametrizacionPerfiles.aspx`, que la Tarea 6 registra en el menú.

- [ ] **Paso 1: Crear `ParametrizacionPerfiles.aspx`**

```aspx
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionPerfiles.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionPerfiles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionPerfiles.js?v=1" type="text/javascript"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="panel-body">
                    <div class="row">
                        <h3>Administración de Perfiles</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">Buscar</div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-6">
                            <label for="txtBuscar">Nombre del perfil</label>
                            <input type="text" class="form-control" id="txtBuscar" placeholder="Escriba parte del nombre" />
                        </div>
                    </div>
                    <div id="divMensajes"></div>
                </div>
                <div class="panel-footer" style="text-align: center">
                    <button id="btnBuscar" onclick="BuscarPerfiles()" type="button" class="btn btn-default">Consultar</button>
                    <button id="btnNuevo" onclick="NuevoPerfil()" type="button" class="btn btn-primary">Nuevo Perfil</button>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h4>Listado de Perfiles</h4>
                </div>
                <div class="panel-body" style="padding: 0">
                    <div class="dataTables_wrapper">
                        <div id="datosTablaPerfiles"></div>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" id="panelDetalle" style="padding: 0px; display: none">
            <div class="panel panel-default">
                <div class="panel-heading">Datos del perfil</div>
                <div class="panel-body">
                    <input type="hidden" id="hdnIdPerfil" value="0" />
                    <div class="row">
                        <div class="form-group col-lg-6">
                            <label for="txtNombrePerfil">Nombre</label>
                            <input type="text" class="form-control" id="txtNombrePerfil" maxlength="100" />
                        </div>
                        <div class="form-group col-lg-3">
                            <label for="cmbEstado">Estado</label>
                            <select id="cmbEstado" class="form-control">
                                <option value="1">Activo</option>
                                <option value="0">Inactivo</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="panel-footer" style="text-align: center">
                    <button id="btnGuardar" onclick="GuardarPerfil()" type="button" class="btn btn-primary">Guardar</button>
                    <button id="btnCancelar" onclick="CancelarPerfil()" type="button" class="btn btn-default">Cancelar</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
```

- [ ] **Paso 2: Crear `ParametrizacionPerfiles.aspx.cs`**

Sin lógica: la pantalla trabaja por AJAX contra el handler.

```csharp
using System;

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionPerfiles : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // La pantalla se alimenta por AJAX desde AdministrarPerfiles.ashx.
            // La sesión la valida ese handler en cada llamada.
        }
    }
}
```

- [ ] **Paso 3: Crear `ParametrizacionPerfiles.aspx.designer.cs`**

```csharp
namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionPerfiles
    {
    }
}
```

- [ ] **Paso 4: Crear `ReporteTareas/js/parametrizacionPerfiles.js`**

```javascript
/* ============================================================================
   Administración de perfiles.

   El botón Eliminar se deshabilita cuando el perfil tiene usuarios, pero eso es
   solo una guía: quien decide de verdad es Sp_RTA_EliminarPerfil. El handler es
   alcanzable por HTTP, así que una validación que viva solo acá no es una
   validación.
   ============================================================================ */

var _perfiles = [];

function PostPerfil(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);
    $.ajax({
        type: "POST",
        url: "AdministrarPerfiles.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) { onSuccess(respuesta); },
        error: function () {
            MostrarMensajePerfil("La operación está tomando demasiado tiempo o la red está saturada. Intente nuevamente.", "danger");
        }
    });
}

function MostrarMensajePerfil(mensaje, tipo) {
    var clase = (tipo === "success") ? "alert-success"
              : (tipo === "warning") ? "alert-warning" : "alert-danger";
    $("#divMensajes").html("<div class='alert " + clase + "'>" + mensaje + "</div>");
}

function BuscarPerfiles() {
    PostPerfil("ListarPerfiles", { "filtro": $("#txtBuscar").val() }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensajePerfil(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _perfiles = respuesta || [];
        RenderTablaPerfiles(_perfiles);
        $("#panelDetalle").hide();
    });
}

function RenderTablaPerfiles(lista) {
    var info = "";
    info += "<table width='100%' class='table table-hover'>";
    info += "<thead><tr>";
    info += "<th>Perfil</th><th>Estado</th><th style='text-align:center'>Usuarios</th>";
    info += "<th style='text-align:center'>Editar</th><th style='text-align:center'>Eliminar</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='5'>No hay perfiles que coincidan con la búsqueda.</td></tr>";
    }

    for (var i = 0; i < lista.length; i++) {
        var p = lista[i];
        var etiqueta = (p.Estado === 1)
            ? "<span class='label label-success'>Activo</span>"
            : "<span class='label label-default'>Inactivo</span>";

        /* Con usuarios asignados el botón se deshabilita y el title explica por
           qué. Es más claro que dejarlo activo para que el servidor rechace. */
        var puedeBorrar = (p.Usuarios === 0);
        var botonBorrar = puedeBorrar
            ? "<button type='button' class='btn btn-danger btn-xs' onclick='ConfirmarEliminar(" + p.IdPerfil + ")'>Eliminar</button>"
            : "<button type='button' class='btn btn-danger btn-xs' disabled title='Tiene " + p.Usuarios + " usuarios asignados'>Eliminar</button>";

        info += "<tr>";
        info += "<td>" + p.NombrePerfil + "</td>";
        info += "<td>" + etiqueta + "</td>";
        info += "<td style='text-align:center'>" + p.Usuarios + "</td>";
        info += "<td style='text-align:center'><button type='button' class='btn btn-info btn-xs' onclick='EditarPerfil(" + p.IdPerfil + ")'>Editar</button></td>";
        info += "<td style='text-align:center'>" + botonBorrar + "</td>";
        info += "</tr>";
    }

    info += "</tbody></table>";
    $("#datosTablaPerfiles").html(info);
}

function NuevoPerfil() {
    $("#hdnIdPerfil").val("0");
    $("#txtNombrePerfil").val("");
    $("#cmbEstado").val("1");
    $("#panelDetalle").show();
}

function EditarPerfil(idPerfil) {
    for (var i = 0; i < _perfiles.length; i++) {
        if (_perfiles[i].IdPerfil === idPerfil) {
            $("#hdnIdPerfil").val(_perfiles[i].IdPerfil);
            $("#txtNombrePerfil").val(_perfiles[i].NombrePerfil);
            $("#cmbEstado").val(_perfiles[i].Estado);
            $("#panelDetalle").show();
            return;
        }
    }
}

function CancelarPerfil() {
    $("#panelDetalle").hide();
}

function GuardarPerfil() {
    var nombre = $("#txtNombrePerfil").val();
    if (nombre === null || nombre.replace(/^\s+|\s+$/g, "") === "") {
        MostrarMensajePerfil("Debe escribir el nombre del perfil.", "warning");
        return;
    }

    var parametros = {
        "idPerfil": $("#hdnIdPerfil").val(),
        "nombrePerfil": nombre,
        "estado": $("#cmbEstado").val()
    };

    PostPerfil("GuardarPerfil", parametros, function (respuesta) {
        MostrarMensajePerfil(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#panelDetalle").hide();
            BuscarPerfiles();
        }
    });
}

function ConfirmarEliminar(idPerfil) {
    var nombre = "";
    for (var i = 0; i < _perfiles.length; i++) {
        if (_perfiles[i].IdPerfil === idPerfil) { nombre = _perfiles[i].NombrePerfil; }
    }

    swal({
        title: "¿Eliminar el perfil?",
        text: "Se eliminará \"" + nombre + "\" junto con los menús que tenga asignados. Esta acción no se puede deshacer.",
        type: "warning",
        showCancelButton: true,
        confirmButtonText: "Eliminar",
        cancelButtonText: "Cancelar",
        closeOnConfirm: true
    }, function () {
        PostPerfil("EliminarPerfil", { "idPerfil": idPerfil }, function (respuesta) {
            MostrarMensajePerfil(respuesta.mensaje, respuesta.tipoMensaje);
            BuscarPerfiles();
        });
    });
}

$(function () {
    BuscarPerfiles();
});
```

- [ ] **Paso 5: Declarar los archivos en el `.csproj`**

```xml
    <Content Include="Formulario\ParametrizacionPerfiles.aspx" />
    <Content Include="js\parametrizacionPerfiles.js" />
```

y en el `ItemGroup` de compilación:

```xml
    <Compile Include="Formulario\ParametrizacionPerfiles.aspx.cs">
      <DependentUpon>ParametrizacionPerfiles.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\ParametrizacionPerfiles.aspx.designer.cs">
      <DependentUpon>ParametrizacionPerfiles.aspx</DependentUpon>
    </Compile>
```

- [ ] **Paso 6: Compilar y probar los tres caminos en el navegador**

```bash
msbuild "ReporteTareas.sln" /p:Configuration=Release /v:minimal
```

Con el sitio corriendo, navegar a `Formulario/ParametrizacionPerfiles.aspx` y comprobar:

1. La tabla carga sola y muestra la columna Usuarios con números coherentes.
2. En un perfil con usuarios, el botón Eliminar está deshabilitado y al pasar el cursor
   muestra cuántos tiene.
3. Crear un perfil de prueba, verificar que aparece con 0 usuarios, eliminarlo y ver que
   desaparece de la lista.
4. Editar el nombre de un perfil y ver que se guarda.

- [ ] **Paso 7: Commit**

```bash
git add ReporteTareas/Formulario/ParametrizacionPerfiles.aspx \
        ReporteTareas/Formulario/ParametrizacionPerfiles.aspx.cs \
        ReporteTareas/Formulario/ParametrizacionPerfiles.aspx.designer.cs \
        ReporteTareas/js/parametrizacionPerfiles.js \
        ReporteTareas/ReporteTareas.csproj
git commit -m "feat(perfiles): pantalla de administracion de perfiles"
```

---

## Tarea 6: Registrar la pantalla en el menú

**Archivos:**
- Crear: `docs/superpowers/plans/sql/2026-08-10-perfiles-menu.sql`

**Interfaces:**
- Consume: la pantalla creada en la Tarea 5.
- Produce: la opción visible en el menú para los perfiles 2, 18 y 19.

- [ ] **Paso 1: Escribir el script**

Se clona la fila de una pantalla análoga porque `MenuDos` tiene 18 columnas, varias
`NOT NULL` con default. Escribir el `INSERT` a mano es la forma habitual de olvidarse una.

```sql
SET QUOTED_IDENTIFIER ON;
USE ReporTarea;
GO

DECLARE @idNuevo int;

/* Se clona ParametrizacionUsuarios: misma naturaleza, mismo grupo padre
   (20042 = "Manejo de Perfiles"). Copiar la fila entera y ajustar despues es
   mas seguro que enumerar columnas. */
INSERT INTO dbo.MenuDos
SELECT * FROM dbo.MenuDos WHERE Href = 'ParametrizacionUsuarios.aspx';

SET @idNuevo = SCOPE_IDENTITY();

UPDATE dbo.MenuDos
SET Href = 'ParametrizacionPerfiles.aspx',
    Titulo = 'Administración de Perfiles'
WHERE Id_Menu = @idNuevo;

/* Estado='0' MUESTRA la opción. Está invertido respecto de lo que sugiere. */
INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, Estado)
VALUES (@idNuevo, 2, '0'),
       (@idNuevo, 18, '0'),
       (@idNuevo, 19, '0');

SELECT @idNuevo AS IdMenuCreado;
```

**Si el `INSERT ... SELECT *` falla** porque `Id_Menu` es IDENTITY, enumerar las columnas
salvo esa. La consulta para obtener la lista:

```sql
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'MenuDos' AND COLUMN_NAME <> 'Id_Menu'
ORDER BY ORDINAL_POSITION;
```

- [ ] **Paso 2: Ejecutarlo y comprobar que la opción aparece**

```bash
sqlcmd -S 192.168.11.14 -U sa -P "<clave>" -d ReporTarea \
  -i docs/superpowers/plans/sql/2026-08-10-perfiles-menu.sql
```

Recargar el sistema con un usuario de perfil 2 y confirmar que aparece «Administración de
Perfiles» dentro de «Manejo de Perfiles». El menú se reconstruye en cada carga: no hace
falta volver a iniciar sesión.

- [ ] **Paso 3: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-10-perfiles-menu.sql
git commit -m "feat(menu): registro de ParametrizacionPerfiles (perfiles 2,18,19)"
```

---

## Tarea 7: Binarios y guía de despliegue

**Archivos:**
- Modificar: los binarios de `bin/` y `obj/` que genere la compilación
- Crear: `docs/superpowers/plans/2026-08-10-perfiles-despliegue.md`

- [ ] **Paso 1: Compilar en Release**

```bash
msbuild "ReporteTareas.sln" /p:Configuration=Release /v:minimal
```

- [ ] **Paso 2: Escribir la guía de despliegue**

En `docs/superpowers/plans/2026-08-10-perfiles-despliegue.md`, con este contenido:

```markdown
# Despliegue — Administración de perfiles

## Orden

1. **Procedimientos almacenados**, en este orden:
   - `2026-08-10-perfiles-eliminar-sp.sql`
   - `2026-08-10-perfiles-listar-sp.sql`
2. **Binarios** de `bin/`: CapaDato, CapaEntidad, CapaNegocio y ReporteTareas.
3. **Archivos nuevos**:
   - `js/parametrizacionPerfiles.js`
   - `Formulario/AdministrarPerfiles.ashx`
   - `Formulario/ParametrizacionPerfiles.aspx`
4. **Menú**: `2026-08-10-perfiles-menu.sql`, al final.

El menú va último a propósito: si se registra antes de que la pantalla exista, quien
entre por el menú recibe un error.

## Verificación posterior

- Entrar con un usuario de perfil 2 y abrir la pantalla desde el menú.
- Confirmar que la columna Usuarios trae números y no ceros en todos los perfiles: si
  todo da 0, el `LEFT JOIN` está comparando columnas equivocadas.
- Intentar eliminar un perfil poblado y confirmar que lo rechaza con el número correcto.
```

- [ ] **Paso 3: Commit**

```bash
git add ReporteTareas/bin CapaDato/bin CapaNegocio/bin CapaEntidad/bin \
        docs/superpowers/plans/2026-08-10-perfiles-despliegue.md
git commit -m "build: binarios de administracion de perfiles"
```

---

## Notas para quien ejecute el plan

**Si la Tarea 0 revela una cuarta tabla** que guarda un id de perfil y no está en el
spec, detenerse antes de la Tarea 1 y avisar. El borrado tiene que contemplarla o
bloquearse por ella; seguir de largo produciría exactamente el problema que este trabajo
busca evitar.

**Si `dbo.PerfilInicio` no existe con ese nombre**, corregirlo en el
`Sp_RTA_EliminarPerfil` de la Tarea 1 antes de crearlo.

**No agregar una validación de "tiene usuarios" en el handler ni en el JavaScript además
de la del procedimiento.** Duplicarla hace que las dos versiones se separen con el
tiempo. El JavaScript deshabilita el botón como ayuda visual, nada más.
