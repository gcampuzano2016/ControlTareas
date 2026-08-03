# Módulos extra por usuario — Diseño

**Fecha:** 2026-08-03
**Estado:** Aprobado (diseño)
**Sub-proyecto:** continuación de "administrar perfiles desde el sistema" (#1 página de inicio por perfil y #2 menús por perfil ya están; el #3 —permisos de acción— sigue aparte).
**Rama:** `ProyectoNuevosCambios`.

## Objetivo

Hoy el menú lateral se decide **solo por perfil**: todos los usuarios de un perfil ven exactamente lo mismo. Se necesita poder darle a un **usuario puntual** módulos que su perfil no tiene, y ver en pantalla a qué perfil pertenece ese usuario para distinguir lo heredado de lo asignado a mano.

**Regla única del sistema:** un usuario ve `menús de su perfil` **∪** `sus módulos extra`. El perfil nunca se recorta.

## Contexto verificado (contra la base `ReporTarea`, 2026-08-03)

### Cómo se arma el menú hoy

- `Master.Master.cs:17,20` toma `Session["Id_Perfil"]` y `Session["Cod_Usuario"]`. **El `Cod_Usuario` ya está disponible; no hay que tocar el login.**
- `Master.Master.cs:25` llama `NegMenuDos.Sp_RTA_ConsultarMenuPerfilUsuario(Idperfil)` — solo perfil.
- Definición actual del SP (leída de `sys.sql_modules`):

  ```sql
  CREATE PROCEDURE [dbo].[Sp_RTA_ConsultarMenuPerfilUsuario]
  @tipoPerfil AS int = 0
  AS
  BEGIN
      SET NOCOUNT ON;
      IF( EXISTS (SELECT * FROM PerfilMenu where IdPerfil=@tipoPerfil ))
      BEGIN
          print 'existe'
          SELECT P.id_Menu, M.Titulo, P.Estado, M.Id_MenuPadre, M.Class_Icon, M.Href
          FROM PerfilMenu P
          INNER JOIN MenuDos M ON P.id_Menu = M.Id_Menu
          where P.IdPerfil=@tipoPerfil AND P.Estado=0
      END
      SET NOCOUNT OFF;
  END
  ```

- El `IF EXISTS` es redundante: si el perfil no tiene filas en `PerfilMenu`, el `SELECT` no devuelve nada de todos modos. Quitarlo **no cambia el comportamiento actual** y es necesario para que un usuario cuyo perfil no tiene menús pueda ver sus extras.
- `Master.Master.cs` consume las columnas por nombre: `id_Menu`, `Titulo`, `Class_Icon`, `Href`, y filtra por `Id_MenuPadre`. **El alias `id_Menu` (con `i` minúscula) y la forma del result set deben conservarse exactamente.**

### Los dos catálogos de perfiles (hallazgo)

| Tabla | Contenido | ¿Corresponde a `R_Usuarios.Id_Perfil`? |
|---|---|---|
| `dbo.Perfiles(IdPerfiles, Codigo, NombrePerfil, Estado, Fecha)` | 19 perfiles (`18 = Super Admin`, `9 = Gerente de Producto`, `14 = Talento Humano`…) | **Sí.** 19 de los 21 valores distintos en uso mapean. |
| `dbo.R_Perfil(Id_Perfil, Nombre, …)` | solo `1..5`, con otros nombres (`4 = 'Jefe de Área'`, mientras en `Perfiles` el 4 es `'falta'`) | No. |

`Sp_RTA_ListarPerfiles` —que llena el combo de `ParametrizacionMenuPerfil.aspx`— lee de `R_Perfil`. Por eso ese combo muestra **5 perfiles** y **no incluye el 18 ni el 19**, que son precisamente los que tienen menús cargados: `PerfilMenu` solo tiene filas para los perfiles **1 (42 ítems), 2 (43), 18 (65) y 19 (43)**.

Usuarios activos por perfil (los que no mapean van como *Sin perfil*):
`-1 (1 usuario), 20 (1), 21 (3), 41 (1)` — 6 en total.

### Tipos y llaves confirmados

- `R_Usuarios.Cod_Usuario varchar(50) NOT NULL` — es la llave que ya usa `R_UsuarioHorarioLaboral.Id_Responsable`.
- `R_Usuarios.Nom_Usuario varchar(100) NOT NULL`, `R_Usuarios.Cedula varchar(32) NULL`, `R_Usuarios.Usuario_Estado` (`'A'` = activo).
- `R_Usuarios.Id_Perfil bigint NOT NULL` y `Perfiles.IdPerfiles bigint`.
- `dbo.R_UsuarioMenu` **no existe** (`OBJECT_ID` devuelve NULL): la tabla es nueva.
- `MenuDos(Id_Menu, Titulo, Href, Class_Icon, Id_MenuPadre)` y `PerfilMenu(IdPerfil, id_Menu, Estado)` donde **`Estado = 0` = activo** (convención heredada, se respeta).

### Trampas del repositorio

- Existen **dos copias** de `DaoMenuDos.cs`: `CapaDato/DaoMenuDos.cs` (incluida en `CapaDato.csproj:71`, **es la que compila**) y `CapaNegocio/CapaDato/DaoMenuDos.cs` (huérfana, ningún `.csproj` la referencia). Editar solo la primera.
- `Master.Master.cs:43-45` declara `view2` pero luego llama `view.ToTable(...)`. Funciona **por accidente**: `view` y `view2` son el mismo objeto `dtPrincipal.DefaultView`, así que el `RowFilter` de hijos aplica igual. **No se toca** (fuera de alcance).

## Decisiones de diseño (acordadas)

1. **Aditivo puro.** Se pueden dar módulos extra a un usuario; **no** se le puede quitar nada de su perfil. Nadie pierde accesos por un descuido.
2. **Pantalla nueva** `ParametrizacionMenuUsuario.aspx`. `ParametrizacionMenuPerfil.aspx` y `PruebaMenu.aspx` quedan como están. Nada de un botón Guardar que haga dos cosas según un combo.
3. **Llave del usuario:** `Cod_Usuario` (varchar), no `Id_Usuario`. Es lo que ya está en sesión y lo que usan las tablas de horarios; evita un lookup extra en el render del menú.
4. **Auditoría:** la tabla guarda quién asignó y cuándo. El "quién" sale de la sesión en el handler, nunca del cliente.
5. **Sí se toca `Master.Master.cs`** (una línea) y el SP del sidebar, con parámetro opcional para que el comportamiento sin usuario sea idéntico al de hoy.
6. **Se corrige `Sp_RTA_ListarPerfiles`** para que lea del catálogo correcto (`dbo.Perfiles`). Va como ítem independiente y verificable por separado.

## Arquitectura

### 1. Tabla nueva `dbo.R_UsuarioMenu`

```sql
Id_UsuarioMenu   INT IDENTITY(1,1) PRIMARY KEY
Cod_Usuario      VARCHAR(50)  NOT NULL   -- R_Usuarios.Cod_Usuario
Id_Menu          INT          NOT NULL   -- MenuDos.Id_Menu
Estado           CHAR(1)      NOT NULL   -- 'A' activo / 'I' inactivo
Usuario_Registro VARCHAR(50)  NOT NULL
Fecha_Registro   DATETIME     NOT NULL DEFAULT GETDATE()
CONSTRAINT UQ_R_UsuarioMenu UNIQUE (Cod_Usuario, Id_Menu)
```

Se **desactiva** (`Estado='I'`), no se borra, para conservar el rastro de auditoría. Índice implícito por el `UNIQUE` cubre las dos consultas (por usuario, y por usuario+menú en el upsert).

Nota de convención: esta tabla usa `'A'/'I'` (como `R_Usuarios.Usuario_Estado`) mientras `PerfilMenu` usa `0/1`. Se documenta a propósito: no se cambia `PerfilMenu` para no arriesgar el render actual, y `'A'/'I'` es más legible en la tabla nueva. Los SPs traducen entre ambas.

### 2. Stored procedures nuevos

- **`Sp_RTA_ListarUsuariosMenu @Filtro VARCHAR(100) = ''`**
  Devuelve `Cod_Usuario, Nom_Usuario, Cedula, Id_Perfil, NombrePerfil, TotalExtras`.
  - `LEFT JOIN dbo.Perfiles ON IdPerfiles = R_Usuarios.Id_Perfil`, con `ISNULL(NombrePerfil, 'Sin perfil')` — los 6 usuarios con perfil huérfano **aparecen igual**, no se esconden.
  - `TotalExtras` = conteo de `R_UsuarioMenu` con `Estado='A'`, para ver de un vistazo quién ya tiene excepciones.
  - Filtro por `Nom_Usuario` / `Cod_Usuario` / `Cedula` con `LIKE '%…%'`, y `WHERE Usuario_Estado = 'A'`. Mismo criterio que `Sp_RTA_ListarUsuariosConHorario`, que ya funciona en la pantalla de horarios.
  - No se reutiliza `Sp_RTA_ListarUsuariosConHorario` porque su proyección es de horarios (`IdHorarioLaboral`, `CodigoHorario`, `EsPredeterminado`…) y no trae perfil.

- **`Sp_RTA_ListarMenuUsuario @CodUsuario VARCHAR(50)`**
  Devuelve **todos** los ítems de `MenuDos` con dos banderas:
  `Id_Menu, Id_MenuPadre, Titulo, Class_Icon, ActivoPerfil BIT, ActivoUsuario BIT`.
  - `ActivoPerfil = 1` si existe `PerfilMenu(IdPerfil = perfil del usuario, Estado = 0)` → checkbox marcado y **deshabilitado**.
  - `ActivoUsuario = 1` si existe `R_UsuarioMenu(Cod_Usuario, Estado='A')` → checkbox marcado y **editable**.
  - Ordenado por `Id_MenuPadre, Titulo` para que el cliente arme el árbol.

- **`Sp_RTA_GuardarMenuUsuario @CodUsuario VARCHAR(50), @ExtrasCsv VARCHAR(MAX), @UsuarioRegistro VARCHAR(50)`**
  1. Valida que `@CodUsuario` exista en `R_Usuarios`; si no, devuelve error sin tocar nada.
  2. Parsea `@ExtrasCsv` (enteros separados por coma) con split por XML-nodes, igual que `Sp_RTA_GuardarMenuPerfil` (compatible con SQL Server 2008+, sin depender de `STRING_SPLIT`).
  3. **Descarta** los ids que ya vienen del perfil del usuario: no tiene sentido guardarlos como extra y así la tabla solo contiene excepciones reales.
  4. **Agrega el padre** de todo hijo extra cuyo padre no esté activo ni por perfil ni por extras — misma invariante hijo⇒padre que se corrigió en el módulo por perfil, porque `Master.Master.cs` no dibuja un hijo cuyo padre no exista.
  5. Upsert en `R_UsuarioMenu`: `Estado='A'` para el set final, `Estado='I'` para el resto de sus filas. Actualiza `Usuario_Registro` y `Fecha_Registro` en cada cambio de estado.
  6. Todo dentro de `TRANSACTION` con `TRY/CATCH` + `ROLLBACK`. Devuelve `Respuestas INT, Mensaje VARCHAR` (mismo contrato que los otros módulos). Mensajes fijos **sin tildes**.

  Set vacío es válido: significa "quitarle todos los extras".

### 3. Cambio al SP del sidebar

`Sp_RTA_ConsultarMenuPerfilUsuario` recibe un parámetro nuevo **opcional**:

```sql
@tipoPerfil AS int = 0,
@CodUsuario AS varchar(50) = NULL
```

Cuerpo: se elimina el `IF EXISTS` (redundante, ver contexto) y el `print 'existe'`, y el `SELECT` pasa a ser la unión de dos fuentes, **conservando alias y orden de columnas** (`id_Menu, Titulo, Estado, Id_MenuPadre, Class_Icon, Href`):

- los ítems del perfil (`PerfilMenu.Estado = 0`), tal cual hoy;
- `UNION` con los extras del usuario (`R_UsuarioMenu.Estado = 'A'`) cuando `@CodUsuario IS NOT NULL`, proyectando `Estado = 0` para mantener la forma del result set.

`UNION` (no `UNION ALL`) elimina el duplicado si un ítem está por ambos lados.

**Con `@CodUsuario = NULL` el resultado es idéntico al actual.** Eso acota el riesgo: si algo sale mal, se revierte el cambio de `Master.Master.cs` y el SP sigue sirviendo al resto.

### 4. Cambio a `Sp_RTA_ListarPerfiles` (corrección independiente)

Pasa a leer del catálogo correcto, conservando **los mismos nombres de columna** (`Id_Perfil`, `Nombre`) para no tocar `DaoMenuPerfil.ListarPerfiles()` (`CapaDato/DaoMenuPerfil.cs:29`) ni el JS del combo:

```sql
SELECT IdPerfiles AS Id_Perfil, NombrePerfil AS Nombre
FROM dbo.Perfiles
WHERE ISNULL(Estado, 1) = 1
ORDER BY NombrePerfil;
```

Efecto: el combo de `ParametrizacionMenuPerfil.aspx` pasa de 5 a 19 perfiles y por fin permite configurar Super Admin (18) y Servicios4 (19). **Cero cambios en C# y cero cambios en la pantalla.**

### 5. Capas C#

- `CapaEntidad/EntUsuarioMenuBusqueda.cs` — `Cod_Usuario, Nom_Usuario, Cedula, Id_Perfil, NombrePerfil, TotalExtras`.
- `CapaEntidad/EntMenuUsuario.cs` — `Id_Menu, Id_MenuPadre, Titulo, Class_Icon, ActivoPerfil, ActivoUsuario`.
- `CapaDato/DaoMenuUsuario.cs` — `ListarUsuarios(string filtro)`, `ListarMenuUsuario(string codUsuario)`, `GuardarMenuUsuario(string codUsuario, string extrasCsv, string usuarioRegistro)`. Patrón `using` + `SqlConnection`/`SqlCommand` de `DaoMenuPerfil.cs`.
- `CapaNegocio/NegMenuUsuario.cs` — pass-through.
- `CapaDato/DaoMenuDos.cs:160` y `CapaNegocio/NegMenuDos.cs:16` — `Sp_RTA_ConsultarMenuPerfilUsuario` recibe el `codUsuario` y lo pasa como `@CodUsuario`. **Solo la copia de `CapaDato/`**, no la huérfana bajo `CapaNegocio/CapaDato/`.

### 6. Handler y pantalla

- `ReporteTareas/Formulario/AdministrarMenuUsuario.ashx` (+ `.ashx.cs`, namespace `JsonJQueryNetMenuUsuario`), patrón de `AdministrarMenuPerfil.ashx`:
  - `BuscarUsuarios {filtro}` → lista de usuarios con su perfil.
  - `ListaMenuUsuario {codUsuario}` → ítems con `ActivoPerfil` / `ActivoUsuario`.
  - `GuardarMenuUsuario {codUsuario, extras:[ids]}` → valida que los ids sean enteros, arma el CSV y llama al SP. **`Usuario_Registro` se toma de la sesión en el servidor**, no del payload.
  - `Response.ContentEncoding = Encoding.UTF8`; `ToJson`; `EntRespuesta`.
- `ReporteTareas/Formulario/ParametrizacionMenuUsuario.aspx` (+ `.cs`, `.designer.cs`) — `Page_Load` con `GenLogin.RedireccionarALogin` y los ocultos `txtUsuario` / `txtLoginUsuario` / `txtIdCliente`, igual que las otras pantallas de parametrización.
- `ReporteTareas/js/parametrizacionMenuUsuario.js` (UTF-8 **con BOM**, `EscaparAttr` para atributos):
  - Buscador (input + botón + Enter) y tabla de resultados con **Usuario, Cédula, Perfil, Extras**. Reutiliza la estructura de `parametrizacionHorarioUsuario.js`.
  - Al elegir un usuario: muestra su **nombre y perfil como campos de solo lectura** y carga el árbol.
  - Árbol de dos niveles. Cada ítem:
    - heredado del perfil → checkbox **marcado y deshabilitado**, con la etiqueta `(perfil)` en gris;
    - extra del usuario → checkbox **marcado y editable**, con la etiqueta `EXTRA` destacada;
    - ninguno → checkbox vacío y editable.
  - Marcar un hijo **auto-marca su padre** si el padre no viene del perfil (coherente con la invariante del SP). Desmarcar un padre que no viene del perfil desmarca sus hijos extra y avisa.
  - Guardar envía solo los `Id_Menu` **editables marcados**; los deshabilitados no viajan.

### 7. Registro en el menú

`docs/superpowers/plans/sql/2026-08-03-modulos-extra-por-usuario-menu.sql` — idempotente, con `SET QUOTED_IDENTIFIER ON`, mismo patrón que `2026-07-27-menus-por-perfil-menu.sql`: inserta `ParametrizacionMenuUsuario.aspx` en `MenuDos` bajo el padre **20042 ("Manejo de Perfiles")** y lo habilita en `PerfilMenu` (`Estado=0`) para los perfiles **2, 18, 19** — los mismos que tienen `ParametrizacionMenuPerfil.aspx`, incluyendo el padre. Se excluye el perfil 1 a propósito, igual que en aquel script.

## Flujo de datos

- **Administración:** pantalla → `AdministrarMenuUsuario.ashx` → `NegMenuUsuario` → `DaoMenuUsuario` → SPs.
- **Efecto en el sidebar:** `Master.Master.cs` pasa `Cod_Usuario` → `Sp_RTA_ConsultarMenuPerfilUsuario` devuelve perfil ∪ extras → el `<ul>` se dibuja igual que siempre, con los ítems de más.

## Manejo de errores

- Guardado atómico con `TRY/CATCH` + `ROLLBACK`; contrato `Respuestas/Mensaje` traducido a `EntRespuesta` (`estado/tipoMensaje/mensaje`) en el handler.
- `@CodUsuario` inexistente → error controlado, sin escritura.
- Ids no enteros → rechazados en el handler antes de armar el CSV.
- Usuario sin perfil válido (los 6 casos `-1/20/21/41`): `ActivoPerfil` sale 0 en todo el árbol y **todos** sus módulos son extras. La pantalla lo muestra como *Sin perfil*; funciona sin caso especial.
- Codificación: mensajes de SP sin tildes; handler UTF-8; `.js` con BOM.

## Pruebas (manuales — no hay framework de pruebas en el proyecto)

**SQL directo**
1. Crear la tabla y los SPs; verificar que la tabla queda vacía y los SPs compilan.
2. `Sp_RTA_ListarPerfiles` → debe devolver **19** filas e incluir `18 = Super Admin`.
3. `Sp_RTA_ConsultarMenuPerfilUsuario 18` (sin `@CodUsuario`) → **el mismo conteo de filas que antes del cambio** (capturar el conteo *antes* de aplicar el script). Esta es la prueba de no-regresión clave.
4. `Sp_RTA_ListarUsuariosMenu 'campuzano'` → devuelve el usuario con su `NombrePerfil` correcto.
5. `Sp_RTA_GuardarMenuUsuario` con un **hijo cuyo padre no está en el perfil** → confirmar que el SP insertó también el padre.
6. `Sp_RTA_GuardarMenuUsuario` con un id que **ya viene del perfil** → confirmar que **no** se guardó como extra.
7. `Sp_RTA_ConsultarMenuPerfilUsuario 18, '<codUsuario>'` → devuelve las del perfil **más** los extras, sin duplicados.

**HTTP al handler**
POST JSON a `AdministrarMenuUsuario.ashx` para las tres acciones, revisando que las tildes salgan bien en el JSON.

**Navegador**
Elegir un usuario, comprobar que se ve su perfil, marcar un módulo extra, guardar, **iniciar sesión con ese usuario** y confirmar que el módulo aparece en el sidebar; y que otro usuario del mismo perfil **no** lo ve.

## Fuera de alcance (YAGNI)

- No se pueden **quitar** a un usuario módulos que le da su perfil.
- No se toca `ParametrizacionMenuPerfil.aspx`, `PruebaMenu.aspx` ni el armado del `<ul>` en `Master.Master.cs` (líneas 27-57), incluido el `view`/`view2` que funciona por accidente.
- Sin CRUD de ítems de menú en la pantalla nueva.
- Sin permisos de acción dentro de cada pantalla (sub-proyecto #3).
- Sin copia masiva de extras entre usuarios ni plantillas.
- No se unifican los catálogos `Perfiles` y `R_Perfil` — solo se corrige de cuál lee el combo. `R_Perfil` sigue en uso por otros módulos y consolidarlas es un trabajo aparte.
- Menú de dos niveles; no se soporta anidamiento más profundo, que hoy no existe.

## Entorno (referencia)

- BD: `Data Source=192.168.11.14; Initial Catalog=ReporTarea; User Id=sa` (la credencial está en `CapaDato/DaoReporTareaAranda.cs`). Desde esta máquina se llega por `sqlcmd -S tcp:192.168.11.14,1433`.
- MSBuild 2019 Community; solución `ReporteTareas.sln`, configuración `Debug`; se compila desde PowerShell. La app requiere **IIS Express de 32 bits** (dependencia `Pechkin` x86).
- Al cambiar un `.js` hay que subir el `?v=` en el `.aspx`, o el navegador sirve la versión cacheada.
