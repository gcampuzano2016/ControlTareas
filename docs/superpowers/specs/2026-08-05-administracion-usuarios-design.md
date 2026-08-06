# Administración de usuarios — Diseño

**Fecha:** 2026-08-05
**Estado:** Aprobado (diseño)
**Rama:** `ProyectoNuevosCambios`.
**Antecedente:** continúa la línea de "administrar desde el sistema" (página de inicio por perfil, menús por perfil, módulos extra por usuario).

## Objetivo

Poder **ver la tabla `R_Usuarios` y editar los datos de los usuarios ya registrados** desde una pantalla, sin entrar a la base de datos.

## Contexto verificado (contra la base `ReporTarea`, 2026-08-05)

### Esquema de `dbo.R_Usuarios` (244 filas)

| Columna | Tipo | Nulo |
|---|---|---|
| `Id_Usuario` | `numeric(5)` | no — **PK**, 244 valores distintos en 244 filas |
| `Cod_Usuario` | `varchar(50)` | no |
| `Nom_Usuario` | `varchar(100)` | no |
| `Log_Usuario` | `varchar(50)` | no |
| `Pass_Usuario` | `varchar(max)` | no |
| `Cod_Perfil` | `numeric(5)` | sí |
| `E_Mail` | `varchar(100)` | sí |
| `Rol_Usuario` | `numeric(5)` | sí |
| `Fec_Creacion` | `varchar(50)` | no |
| `Usuario_Estado` | `varchar(1)` | no — `'A'` = activo |
| `Cod_Jefe_Inm` | `varchar(100)` | sí |
| `MailCodJefeInm` | `varchar(100)` | sí |
| `Cod_Sap` | `varchar(50)` | sí |
| `Id_Perfil` | `bigint` | no |
| `IdCliente` | `varchar(50)` | sí |
| `Departamento` | `varchar(128)` | sí |
| `Cedula` | `varchar(32)` | sí |
| `EstadoUsuario` | `int` | sí |
| `CodigoReset` | `varchar(50)` | sí |
| `Empresa` | `varchar(50)` | sí |

### Hallazgos que condicionan el diseño

1. **`Id_Usuario` es la PK y es única** (244/244). **`Cod_Usuario` NO es única:** 2 usuarios activos tienen filas duplicadas con perfiles distintos (hallazgo heredado del módulo de módulos extra). Por eso **toda la edición va por `Id_Usuario`**.
2. **El login NO compara contra `Pass_Usuario` para la inmensa mayoría de los usuarios.** `Login.aspx.cs:105-109` autentica contra Active Directory (`Autentificacion2.AuthenticationSoapClient.AutenticateUserAD(...)`) cuando `objUsuario.IdCliente == 0`, y **239 de los 244 usuarios tienen `IdCliente` nulo**. `Sp_RTAAutenticaUsuario` y su envoltorio `NegUsuario.RTA_AutenticaUsuario` (`CapaDato/DaoRTAUsuario.cs:144`, `CapaNegocio/NegUsuario.cs:132`), que sí compararían `Pass_Usuario` hasheado con MD5 (`ReporteTareas/clases/SeguridadHelper.cs`, `namespace SeguridadAppHelper`, `GetMd5Hash(string)`), **no los llama nadie en el flujo de login actual**. Cualquier operación que escriba `Pass_Usuario` es, en la práctica, invisible para el login de los 239 usuarios que entran por AD.
3. **15 de 244 contraseñas están en texto plano** (largos 8, 10, 13 y 15; las otras 229 son 32 caracteres hexadecimales válidos). Esto es irrelevante para el login real (que no lee esta columna en la mayoría de los casos), pero se deja anotado porque documenta el estado de la columna.
4. **Columnas duplicadas y ambiguas:** `Cod_Perfil` **y** `Id_Perfil`; `Usuario_Estado` (`'A'`/`'I'`) **y** `EstadoUsuario` (int). El módulo **no expone ninguna de las cuatro** para edición, precisamente para no dejarlas inconsistentes.
5. **Ya existe solape parcial:** `Perfiles.aspx` (con `js/Perfiles.js`) lista usuarios por perfil y edita **perfil y correo**, guardando con `Sp_RTActualizarPerfil(@IdCambioPerfil, @IdUsuario, @CorreoCambio)` — que también trabaja por `Id_Usuario`. **Esa pantalla no se toca.** El correo queda editable desde dos lugares; se documenta a propósito porque unificarlas es un trabajo aparte.
6. `R_Usuarios` **no tiene columnas de auditoría** (no hay `Usuario_Modificacion` ni `Fecha_Modificacion`). Por eso el rastro va en una tabla nueva.

## Decisiones de diseño (acordadas)

1. **Solo ver y editar.** No se crean usuarios, no se eliminan y no se inactivan.
2. **Ocho campos editables**, de datos personales u organizativos. **No son todos ajenos al control de acceso**: `Cod_Jefe_Inm` y `MailCodJefeInm` determinan el enrutamiento de aprobaciones y notificaciones (`CapaDato/DaoSolicitud.cs:13,136,448`; `CapaDato/DaoVentanaAprobacion.cs:11`; `CapaDato/DaoRTAUsuario.cs:517`), y son texto libre **sin validar contra usuarios existentes**: un dígito mal tecleado redirige aprobaciones en silencio, sin que la pantalla lo detecte. El resto —
   `Nom_Usuario`, `E_Mail`, `Cedula`, `Departamento`, `Empresa`, `Cod_Sap` — sí son solo datos descriptivos.
3. **Solo lectura en pantalla:** `Id_Usuario`, `Cod_Usuario`, `Log_Usuario`, nombre del perfil y estado. Se muestran para dar contexto. **`Cod_Usuario` no se edita nunca**: es la llave con la que `R_UsuarioHorarioLaboral` y `R_UsuarioMenu` asocian horarios y módulos extra, y cambiarlo rompería esas asignaciones en silencio.
4. **Bitácora en tabla nueva** `R_UsuarioBitacora`, con quién, cuándo, qué acción y qué campos cambiaron.
5. **Se listan todos los usuarios**, activos e inactivos, con el estado visible en la tabla. Es una pantalla de administración: ocultar filas sería sorprendente, y como no se puede cambiar el estado desde aquí, no hay riesgo de confusión.

## Arquitectura

### 1. Tabla nueva `dbo.R_UsuarioBitacora`

```sql
Id_Bitacora      INT IDENTITY(1,1) PRIMARY KEY
Id_Usuario       NUMERIC(5)    NOT NULL   -- R_Usuarios.Id_Usuario
Accion           VARCHAR(30)   NOT NULL   -- 'EDICION' | 'RESET_PASSWORD'
Detalle          VARCHAR(2000) NOT NULL   -- campos que cambiaron, valor anterior -> nuevo
Usuario_Registro VARCHAR(50)   NOT NULL
Fecha_Registro   DATETIME      NOT NULL DEFAULT GETDATE()
```

Índice por `Id_Usuario, Fecha_Registro DESC` para consultar el historial de un usuario.

**`RESET_PASSWORD` no es alcanzable desde esta pantalla:** el botón de restablecer contraseña se quitó (ver Decisión, más abajo). El valor queda documentado porque `Sp_RTA_RestablecerPassword` y la columna siguen existiendo en la base, por si el login migra a autenticar contra `Pass_Usuario` en el futuro. Si alguna vez se usa, el `Detalle` guardaría solo el hecho (`'Contrasena restablecida'`), nunca la contraseña ni el hash.

### 2. Stored procedures

- **`Sp_RTA_ListarUsuariosAdmin @Filtro VARCHAR(100) = ''`**
  Devuelve `Id_Usuario, Cod_Usuario, Nom_Usuario, Log_Usuario, E_Mail, Cedula, Departamento, Empresa, Cod_Sap, Cod_Jefe_Inm, MailCodJefeInm, Id_Perfil, NombrePerfil, Usuario_Estado`.
  - `LEFT JOIN dbo.Perfiles ON IdPerfiles = Id_Perfil` con `ISNULL(NombrePerfil,'Sin perfil')` — los usuarios con perfil huérfano aparecen igual.
  - Filtro `LIKE '%…%'` sobre `Nom_Usuario`, `Cod_Usuario` y `Cedula`. **Sin filtro por `Usuario_Estado`**: se listan todos.
  - Ordenado por `Nom_Usuario`.
  - Trae ya todos los campos editables, así el formulario se llena desde la fila seleccionada sin una segunda llamada.

- **`Sp_RTA_ActualizarUsuario @Id_Usuario NUMERIC(5), @Nom_Usuario, @E_Mail, @Cedula, @Departamento, @Empresa, @Cod_Sap, @Cod_Jefe_Inm, @MailCodJefeInm, @UsuarioRegistro`**
  1. Valida que `@Id_Usuario` exista; si no, devuelve error sin escribir.
  2. Valida que `@Nom_Usuario` no venga vacío (es `NOT NULL` en el esquema).
  3. Lee la fila actual y arma el `Detalle` comparando campo por campo, en formato `Campo: 'anterior' -> 'nuevo'`, solo con los que cambiaron.
  4. Si **nada** cambió: devuelve `Respuestas = 1` con mensaje de que no hubo cambios, y **no** escribe en la bitácora.
  5. Si cambió algo: `UPDATE` de los ocho campos + `INSERT` en `R_UsuarioBitacora` con `Accion='EDICION'`, todo en una `TRANSACTION` con `TRY/CATCH`.
  6. Devuelve `Respuestas INT, Mensaje VARCHAR(300)` — mismo contrato que el resto de los módulos. Mensajes fijos **sin tildes**.

- **`Sp_RTA_ListarBitacoraUsuario @Id_Usuario NUMERIC(5)`**
  Devuelve `Accion, Detalle, Usuario_Registro, Fecha_Registro` de los últimos 50 movimientos, para ver el historial del usuario seleccionado en la propia pantalla.

### 3. Capas C#

- `CapaEntidad/EntUsuarioAdmin.cs` — los 14 campos que devuelve el listado.
- `CapaEntidad/EntUsuarioBitacora.cs` — `Accion, Detalle, Usuario_Registro, Fecha_Registro`.
- `CapaDato/DaoUsuarioAdmin.cs` — `ListarUsuarios(string filtro)`, `ActualizarUsuario(EntUsuarioAdmin, string usuarioRegistro)`, `ListarBitacora(decimal idUsuario)`.
- `CapaNegocio/NegUsuarioAdmin.cs` — pass-through.

`Id_Usuario` viaja como `decimal` en C# porque en la base es `numeric(5)`.

### 4. Handler y pantalla

- `ReporteTareas/Formulario/AdministrarUsuarios.ashx` (+ `.ashx.cs`, namespace `JsonJQueryNetUsuarios`), patrón del handler de módulos extra **ya corregido**:
  - **Exige sesión en la primera línea de `ProcessRequest`**: si `context.Session` o `Session["UserLogin"]` son nulos, responde error y no ejecuta nada. Implementa `IRequiresSessionState`.
  - Acciones: `BuscarUsuarios {filtro}`, `GuardarUsuario {…8 campos + idUsuario}`, `VerBitacora {idUsuario}`.
  - **`GuardarUsuario` exige que las ocho claves editables vengan presentes en el payload** (aunque su valor sea vacío); si falta alguna, rechaza sin guardar. Un valor vacío sí se acepta, porque significa borrar ese campo a propósito.
  - `Usuario_Registro` sale de `Session["Cod_Usuario"]`, nunca del payload.
  - `Response.ContentEncoding = Encoding.UTF8`.
- `ReporteTareas/Formulario/ParametrizacionUsuarios.aspx` (+ `.cs`, `.designer.cs`) — patrón `Page_Load` con `GenLogin.RedireccionarALogin`.
- `ReporteTareas/js/parametrizacionUsuarios.js` (UTF-8 **con BOM**, con `Escapar`/`EscaparAttr`):
  - Buscador + tabla (código, nombre, login, perfil, estado, correo).
  - Al elegir un usuario se abre el formulario con los 8 campos editables y los 5 de solo lectura deshabilitados.
  - Botón **Ver historial**: muestra la bitácora del usuario.

### 5. Validaciones

Se validan en el handler (servidor) y se avisan en el JS (comodidad, no seguridad):

| Campo | Regla |
|---|---|
| `Nom_Usuario` | obligatorio, máximo 100 |
| `E_Mail` | vacío o con formato válido, máximo 100 |
| `Cedula` | máximo 32 |
| `Departamento` | máximo 128 |
| `Empresa` | máximo 50 |
| `Cod_Sap` | máximo 50 |
| `Cod_Jefe_Inm` | máximo 100 |
| `MailCodJefeInm` | vacío o con formato válido, máximo 100 |

Los máximos evitan que SQL Server trunque en silencio al guardar.

Además, el handler exige que las **ocho claves editables estén presentes en el payload** de `GuardarUsuario` (no solo con formato válido): si falta una, rechaza sin guardar nada. Esto es aparte de la validación de formato, porque un payload parcial pasaría las reglas de la tabla de arriba (un campo ausente no viola ningún máximo) y aun así borraría columnas mediante `NULLIF(@x,'')` en el SP.

### 6. Registro en el menú

Script idempotente que inserta `ParametrizacionUsuarios.aspx` en `MenuDos` bajo el padre **20042 ("Manejo de Perfiles")** y lo habilita en `PerfilMenu` (`Estado = 0`) para los perfiles **2, 18 y 19**, incluyendo el padre. Mismo patrón que los registros anteriores.

## Flujo de datos

Pantalla → `AdministrarUsuarios.ashx` → `NegUsuarioAdmin` → `DaoUsuarioAdmin` → SPs → `R_Usuarios` / `R_UsuarioBitacora`.

## Manejo de errores

- Escrituras atómicas con `TRY/CATCH` + `ROLLBACK`; contrato `Respuestas/Mensaje` traducido a `EntRespuesta` en el handler.
- `@Id_Usuario` inexistente → error controlado, sin escritura.
- Payload de `GuardarUsuario` sin alguna de las ocho claves editables → el handler rechaza antes de tocar `NegUsuarioAdmin`/la base, con mensaje de advertencia y sin invocar el SP.
- Guardar sin cambios → mensaje informativo, sin fila de bitácora.
- Codificación: mensajes de SP sin tildes; handler UTF-8; `.js` y `.aspx` con BOM.

## Pruebas (manuales — el proyecto no tiene framework de pruebas)

**SQL**
1. `Sp_RTA_ListarUsuariosAdmin ''` → 244 filas; con filtro, el subconjunto correcto; los usuarios sin perfil válido salen como *Sin perfil*.
2. `Sp_RTA_ActualizarUsuario` cambiando un solo campo → la fila se actualiza y la bitácora registra **solo ese campo**, con valor anterior y nuevo.
3. Llamarlo de nuevo con los **mismos** valores → mensaje de "sin cambios" y **ninguna** fila nueva en la bitácora.
4. Con `@Id_Usuario` inexistente → error, sin escritura.
5. Al terminar, revertir los datos de prueba a sus valores originales.

**Handler**
POST a cada acción; y un POST **sin sesión** debe ser rechazado sin tocar la base. Además, un POST a `GuardarUsuario` con una de las ocho claves editables faltante (no solo vacía, ausente del JSON) debe devolver advertencia sin llamar a `NegUsuarioAdmin.ActualizarUsuario`, y un POST con las ocho claves vacías sí debe guardar (vacío es un valor válido, borra el campo a propósito).

**Navegador**
Buscar, editar los ocho campos, guardar, releer y confirmar que persistieron; ver el historial; confirmar tildes correctas.

## Fuera de alcance (YAGNI)

- **No** se crean, eliminan ni inactivan usuarios.
- **No** se editan `Cod_Usuario`, `Log_Usuario`, `Id_Perfil`, `Cod_Perfil`, `Usuario_Estado`, `EstadoUsuario`, `IdCliente`, `Rol_Usuario`, `Fec_Creacion` ni `CodigoReset`.
- **No** se tocan `Perfiles.aspx` ni `Sp_RTActualizarPerfil`, aunque el correo quede editable desde dos pantallas.
- **No** se corrigen las 15 contraseñas en texto plano (pendiente conocido, script aparte).
- **No** se cambia el algoritmo MD5 sin sal. Migrar a un hash moderno rompería el login de los 244 usuarios y es un proyecto propio.
- Sin control de concurrencia optimista: si dos administradores editan el mismo usuario a la vez, gana el último. La bitácora deja ver qué pasó.

## Entorno (referencia)

- BD: `Data Source=192.168.11.14; Initial Catalog=ReporTarea; User Id=sa` (credencial en `CapaDato/DaoReporTareaAranda.cs:23`). Desde esta máquina, `sqlcmd -S tcp:192.168.11.14,1433`. La conectividad ha sido intermitente.
- MSBuild 2019 Community; `ReporteTareas.sln`, configuración `Debug`. La app requiere **IIS Express de 32 bits** (dependencia `Pechkin` x86).
- Este repositorio **versiona los binarios** (`bin/`, `obj/`): al terminar hay que commitearlos, en un commit aparte del código.
- Al cambiar un `.js` hay que subir el `?v=` en el `.aspx`.
