# Administración de usuarios — Diseño

**Fecha:** 2026-08-05
**Estado:** Aprobado (diseño)
**Rama:** `ProyectoNuevosCambios`.
**Antecedente:** continúa la línea de "administrar desde el sistema" (página de inicio por perfil, menús por perfil, módulos extra por usuario).

## Objetivo

Poder **ver la tabla `R_Usuarios` y editar los datos de los usuarios ya registrados** desde una pantalla, sin entrar a la base de datos. Incluye restablecer la contraseña de un usuario que la olvidó.

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
2. **Las contraseñas son MD5 hexadecimal en minúsculas, sin sal.** Se calculan en `ReporteTareas/clases/SeguridadHelper.cs` (`namespace SeguridadAppHelper`, método `GetMd5Hash(string)`: `MD5.ComputeHash(Encoding.UTF8.GetBytes(input))` formateado con `ToString("x2")`), y el login las usa en `Login.aspx.cs:101`. `Sp_RTAAutenticaUsuario` compara `Pass_Usuario = @Passw` **tal cual**, sin transformar.
3. **15 de 244 contraseñas están en texto plano** (largos 8, 10, 13 y 15; las otras 229 son 32 caracteres hexadecimales válidos). Esos usuarios no pueden iniciar sesión porque el login envía el hash. **Queda fuera de alcance**, anotado como pendiente.
4. **Columnas duplicadas y ambiguas:** `Cod_Perfil` **y** `Id_Perfil`; `Usuario_Estado` (`'A'`/`'I'`) **y** `EstadoUsuario` (int). El módulo **no expone ninguna de las cuatro** para edición, precisamente para no dejarlas inconsistentes.
5. **Ya existe solape parcial:** `Perfiles.aspx` (con `js/Perfiles.js`) lista usuarios por perfil y edita **perfil y correo**, guardando con `Sp_RTActualizarPerfil(@IdCambioPerfil, @IdUsuario, @CorreoCambio)` — que también trabaja por `Id_Usuario`. **Esa pantalla no se toca.** El correo queda editable desde dos lugares; se documenta a propósito porque unificarlas es un trabajo aparte.
6. `R_Usuarios` **no tiene columnas de auditoría** (no hay `Usuario_Modificacion` ni `Fecha_Modificacion`). Por eso el rastro va en una tabla nueva.

## Decisiones de diseño (acordadas)

1. **Solo ver y editar.** No se crean usuarios, no se eliminan y no se inactivan.
2. **Ocho campos editables**, todos de datos personales u organizativos, ninguno de control de acceso:
   `Nom_Usuario`, `E_Mail`, `Cedula`, `Departamento`, `Empresa`, `Cod_Sap`, `Cod_Jefe_Inm`, `MailCodJefeInm`.
3. **Solo lectura en pantalla:** `Id_Usuario`, `Cod_Usuario`, `Log_Usuario`, nombre del perfil y estado. Se muestran para dar contexto. **`Cod_Usuario` no se edita nunca**: es la llave con la que `R_UsuarioHorarioLaboral` y `R_UsuarioMenu` asocian horarios y módulos extra, y cambiarlo rompería esas asignaciones en silencio.
4. **Contraseña: solo restablecer.** Nunca se lee, ni se muestra, ni se envía al navegador. El hash se calcula **en el servidor** reutilizando `SeguridadHelper.GetMd5Hash`, para que quede idéntico a lo que el login espera.
5. **Bitácora en tabla nueva** `R_UsuarioBitacora`, con quién, cuándo, qué acción y qué campos cambiaron.
6. **Se listan todos los usuarios**, activos e inactivos, con el estado visible en la tabla. Es una pantalla de administración: ocultar filas sería sorprendente, y como no se puede cambiar el estado desde aquí, no hay riesgo de confusión.

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

**En `RESET_PASSWORD` el `Detalle` guarda solo el hecho** (`'Contrasena restablecida'`), nunca la contraseña ni el hash.

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

- **`Sp_RTA_RestablecerPassword @Id_Usuario NUMERIC(5), @HashMd5 VARCHAR(32), @UsuarioRegistro VARCHAR(50)`**
  Valida que el usuario exista y que `@HashMd5` tenga **exactamente 32 caracteres hexadecimales** (defensa en profundidad: si algún día alguien llamara al SP con texto plano, se rechaza). `UPDATE Pass_Usuario` + `INSERT` en la bitácora con `Accion='RESET_PASSWORD'` y `Detalle='Contrasena restablecida'`, en transacción.

- **`Sp_RTA_ListarBitacoraUsuario @Id_Usuario NUMERIC(5)`**
  Devuelve `Accion, Detalle, Usuario_Registro, Fecha_Registro` de los últimos 50 movimientos, para ver el historial del usuario seleccionado en la propia pantalla.

### 3. Capas C#

- `CapaEntidad/EntUsuarioAdmin.cs` — los 14 campos que devuelve el listado.
- `CapaEntidad/EntUsuarioBitacora.cs` — `Accion, Detalle, Usuario_Registro, Fecha_Registro`.
- `CapaDato/DaoUsuarioAdmin.cs` — `ListarUsuarios(string filtro)`, `ActualizarUsuario(EntUsuarioAdmin, string usuarioRegistro)`, `RestablecerPassword(decimal idUsuario, string hashMd5, string usuarioRegistro)`, `ListarBitacora(decimal idUsuario)`.
- `CapaNegocio/NegUsuarioAdmin.cs` — pass-through.

`Id_Usuario` viaja como `decimal` en C# porque en la base es `numeric(5)`.

### 4. Handler y pantalla

- `ReporteTareas/Formulario/AdministrarUsuarios.ashx` (+ `.ashx.cs`, namespace `JsonJQueryNetUsuarios`), patrón del handler de módulos extra **ya corregido**:
  - **Exige sesión en la primera línea de `ProcessRequest`**: si `context.Session` o `Session["UserLogin"]` son nulos, responde error y no ejecuta nada. Implementa `IRequiresSessionState`.
  - Acciones: `BuscarUsuarios {filtro}`, `GuardarUsuario {…8 campos + idUsuario}`, `RestablecerPassword {idUsuario, clave}`, `VerBitacora {idUsuario}`.
  - **`RestablecerPassword` recibe la contraseña en claro y la hashea en el servidor** con `SeguridadHelper.GetMd5Hash`. El cliente nunca calcula el hash.
  - `Usuario_Registro` sale de `Session["Cod_Usuario"]`, nunca del payload.
  - `Response.ContentEncoding = Encoding.UTF8`.
- `ReporteTareas/Formulario/ParametrizacionUsuarios.aspx` (+ `.cs`, `.designer.cs`) — patrón `Page_Load` con `GenLogin.RedireccionarALogin`.
- `ReporteTareas/js/parametrizacionUsuarios.js` (UTF-8 **con BOM**, con `Escapar`/`EscaparAttr`):
  - Buscador + tabla (código, nombre, login, perfil, estado, correo).
  - Al elegir un usuario se abre el formulario con los 8 campos editables y los 5 de solo lectura deshabilitados.
  - Botón **Restablecer contraseña**: pide la nueva y su confirmación en un modal; exige mínimo 6 caracteres y que ambas coincidan antes de enviar.
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
| contraseña nueva | mínimo 6 caracteres, y confirmación idéntica |

Los máximos evitan que SQL Server trunque en silencio al guardar.

### 6. Registro en el menú

Script idempotente que inserta `ParametrizacionUsuarios.aspx` en `MenuDos` bajo el padre **20042 ("Manejo de Perfiles")** y lo habilita en `PerfilMenu` (`Estado = 0`) para los perfiles **2, 18 y 19**, incluyendo el padre. Mismo patrón que los registros anteriores.

## Flujo de datos

Pantalla → `AdministrarUsuarios.ashx` → `NegUsuarioAdmin` → `DaoUsuarioAdmin` → SPs → `R_Usuarios` / `R_UsuarioBitacora`.

## Manejo de errores

- Escrituras atómicas con `TRY/CATCH` + `ROLLBACK`; contrato `Respuestas/Mensaje` traducido a `EntRespuesta` en el handler.
- `@Id_Usuario` inexistente → error controlado, sin escritura.
- Hash con formato inválido → el SP rechaza antes de tocar la tabla.
- Guardar sin cambios → mensaje informativo, sin fila de bitácora.
- Codificación: mensajes de SP sin tildes; handler UTF-8; `.js` y `.aspx` con BOM.

## Pruebas (manuales — el proyecto no tiene framework de pruebas)

**SQL**
1. `Sp_RTA_ListarUsuariosAdmin ''` → 244 filas; con filtro, el subconjunto correcto; los usuarios sin perfil válido salen como *Sin perfil*.
2. `Sp_RTA_ActualizarUsuario` cambiando un solo campo → la fila se actualiza y la bitácora registra **solo ese campo**, con valor anterior y nuevo.
3. Llamarlo de nuevo con los **mismos** valores → mensaje de "sin cambios" y **ninguna** fila nueva en la bitácora.
4. Con `@Id_Usuario` inexistente → error, sin escritura.
5. `Sp_RTA_RestablecerPassword` con un hash de 32 hex → actualiza y registra `RESET_PASSWORD` sin exponer el valor. Con `'123456'` (no hexadecimal de 32) → **rechaza**.
6. Al terminar, revertir los datos de prueba a sus valores originales.

**Handler**
POST a cada acción; y un POST **sin sesión** debe ser rechazado sin tocar la base.

**Contraseña (la prueba que importa)**
Restablecer la contraseña de un usuario de prueba desde la pantalla y **verificar que puede iniciar sesión con la nueva**. Es lo único que demuestra que el hash coincide con lo que el login espera.

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
