# Menús por perfil (árbol padre-hijo) — Diseño

**Fecha:** 2026-07-27
**Estado:** Aprobado (diseño)
**Sub-proyecto:** #2 de 3 del pedido "administrar perfiles desde el sistema" (el #1 —página de inicio por perfil— ya está; el #3 —permisos de acción— va aparte).
**Rama:** `ProyectoNuevosCambios`.

## Objetivo

Rehacer la **asignación de menús por perfil** en una pantalla nueva y limpia, y **cerrar de raíz** el síntoma reportado: *"activo un submenú para el perfil y no aparece en el menú."*

## Contexto y causa raíz (verificados en código)

- El menú del sidebar lo arma `Master.Master.cs` a partir de `Sp_RTA_ConsultarMenuPerfilUsuario(@Id_Perfil)`, que devuelve los ítems de `MenuDos` que el perfil tiene activos en `PerfilMenu` (`Estado=0`).
- `Master.Master.cs` recorre **primero los padres activos** (`Id_MenuPadre=0`) y, dentro de cada padre, sus hijos. **Si el padre no está activo para el perfil, su `<li>` nunca se dibuja y sus hijos tampoco** — aunque el hijo esté activo. Esa es la causa del síntoma (regla padre-hijo).
- Módulo actual `PruebaMenu.aspx` (bajo "Manejo de Perfiles") ya permite: asignar menús por perfil (checkbox por ítem + guardar vía acción `GuardarDatosMenu` de `ObtenerListaTareas.ashx`, con una serialización frágil por carácter especial `↨` y `id` de checkbox repetido por fila), crear ítems de menú y editar ícono/referencia.
- Tablas: `MenuDos(Id_Menu, Titulo, Href, Class_Icon, Id_MenuPadre, ...)` y `PerfilMenu(IdPerfil, id_Menu, Estado)` donde **`Estado=0` = activo**. Catálogo de perfiles: `R_Perfil(Id_Perfil, Nombre)`. El menú es de **dos niveles** (padre `Id_MenuPadre=0`, hijo apunta a un padre).

## Decisiones de diseño (acordadas)

- **Dónde se corrige la regla:** en el **guardado** (server-side) + un **script one-off** de arreglo de datos. **No se toca `Master.Master.cs`** (menos riesgo global; los datos quedan consistentes y el render actual ya funciona con datos consistentes).
- **Guardado atómico:** un solo SP recibe el conjunto de menús activos del perfil, le **agrega los padres** de cualquier hijo activo, y reemplaza el estado del perfil en `PerfilMenu` dentro de una transacción.
- **Pantalla nueva** `ParametrizacionMenuPerfil.aspx` (árbol padre-hijo) reemplaza la *asignación por perfil*. `PruebaMenu.aspx` **se conserva** para crear ítems y editar ícono/referencia (fuera de alcance de este sub-proyecto).
- **Enfoque:** tabla existente + SPs nuevos + 3 capas + handler + pantalla + script de datos + registro en menú. Mismo patrón que los módulos "Ventana de Aprobación" y "Página de inicio por perfil".

## Arquitectura

### 1. Stored procedures (sobre `MenuDos` / `PerfilMenu` existentes)

- **`Sp_RTA_ListarPerfiles`** → `Id_Perfil, Nombre` de `R_Perfil` activos, ordenado por `Nombre`. Para el combo (auto-contenido).
- **`Sp_RTA_ListarMenuPerfil @IdPerfil`** → todos los ítems de `MenuDos` con su estado para el perfil:
  `Id_Menu, Id_MenuPadre, Titulo, Class_Icon, Activo` (`Activo = CASE WHEN EXISTS(PerfilMenu con Estado=0) THEN 1 ELSE 0 END`). Ordenado por `Id_MenuPadre, Titulo` para que el cliente arme el árbol.
- **`Sp_RTA_GuardarMenuPerfil @IdPerfil, @ActivosCsv VARCHAR(MAX)`** → guardado atómico:
  1. Parsear `@ActivosCsv` (lista de `Id_Menu` activos, enteros separados por coma) a una tabla temporal, usando split por XML-nodes (compatible con SQL Server 2008+, sin depender de `STRING_SPLIT`).
  2. **Agregar los padres**: incluir en el set activo el `Id_MenuPadre` de todo hijo activo (invariante hijo⇒padre).
  3. Para **cada** ítem de `MenuDos`: `MERGE`/upsert en `PerfilMenu(IdPerfil, id_Menu)` con `Estado = 0` si está en el set activo, `Estado = 1` si no. Todo en una `TRANSACTION` con `TRY/CATCH`.
  4. Devolver `Respuestas INT, Mensaje VARCHAR` (mismo contrato que los otros módulos). Mensajes fijos **sin tildes**.
- Los `Id_Menu` viajan como enteros; el handler los valida como enteros antes de armar el CSV (no hay inyección: son numéricos y el SP los trata como datos).

### 2. Script one-off de arreglo de datos

`docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-fix-datos.sql` — **idempotente**: para todo `(perfil, hijo activo)` cuyo padre no esté activo, **insertar** la fila del padre con `Estado=0` (si no existe) y **reactivar** (`Estado=0`) la fila del padre si existe pero está inactiva. Deja consistentes los datos actuales que causan el síntoma. Antes/después imprime un conteo de casos afectados para verificación.

### 3. Capas C#

- `CapaEntidad/EntMenuPerfil.cs` — `Id_Menu, Id_MenuPadre, Titulo, Class_Icon, Activo`.
- `CapaEntidad/EntPerfil.cs` — entidad ligera para el combo: `Id_Perfil, Nombre` (no existe una equivalente reutilizable; `EntPerfilInicio` tiene otros campos).
- `CapaDato/DaoMenuPerfil.cs` — `ListarPerfiles()`, `ListarMenuPerfil(int idPerfil)`, `GuardarMenuPerfil(int idPerfil, string activosCsv)`.
- `CapaNegocio/NegMenuPerfil.cs` — pass-through.

### 4. Handler + pantalla

- `ReporteTareas/Formulario/AdministrarMenuPerfil.ashx` (+ `.ashx.cs`, namespace `JsonJQueryNetMenuPerfil`) — acciones:
  - `ListaPerfiles` → combo.
  - `ListaMenuPerfil {idPerfil}` → ítems con `Activo`.
  - `GuardarMenuPerfil {idPerfil, activos:[ids]}` → valida ids enteros, arma el CSV y llama al SP.
  - `Response.ContentEncoding = Encoding.UTF8`; `ToJson`; `EntRespuesta`; patrón de `AdministrarPerfilInicio.ashx`.
- `ReporteTareas/Formulario/ParametrizacionMenuPerfil.aspx` (+ `.cs`, `.designer.cs`) — patrón `Page_Load` con `GenLogin.RedireccionarALogin` y `txtUsuario`/`txtLoginUsuario`/`txtIdCliente` ocultos.
- `ReporteTareas/js/parametrizacionMenuPerfil.js` (UTF-8 con BOM):
  - Combo de perfil (carga `ListaPerfiles`).
  - Al elegir perfil (o botón Consultar): carga `ListaMenuPerfil` y **arma el árbol**: cada padre (`Id_MenuPadre=0`) con su checkbox, y debajo sus hijos indentados con checkbox.
  - **Reglas en la UI:** marcar un hijo **auto-marca su padre**; desmarcar un padre **desmarca (y avisa) sus hijos** — el árbol siempre queda coherente con lo que el render puede mostrar.
  - **Guardar:** recolecta los `Id_Menu` marcados (activos) y los envía en `activos:[...]`. Tras éxito, recarga el árbol.
  - Escapado de atributos correcto (`EscaparAttr` que codifica comillas, como en `parametrizacionPerfilInicio.js`).

### 5. Registro en el menú

`docs/superpowers/plans/sql/2026-07-27-menus-por-perfil-menu.sql` — insertar `ParametrizacionMenuPerfil.aspx` en `MenuDos` bajo el padre "Manejo de Perfiles" (Id_Menu 20042) y habilitarla en `PerfilMenu` (Estado=0) para perfiles **1, 2, 18, 19**, con `SET QUOTED_IDENTIFIER ON`, idempotente (patrón del script de menú de #1).

## Flujo de datos

- **Administración:** pantalla → `AdministrarMenuPerfil.ashx` → Negocio → Dato → SPs (`ListaPerfiles` / `ListaMenuPerfil` / `GuardarMenuPerfil`).
- **Efecto en el sidebar:** al guardar con la invariante, `PerfilMenu` queda consistente ⇒ `Master.Master.cs` (sin cambios) ya dibuja el submenú porque su padre está activo.

## Manejo de errores

- Guardado atómico con `TRY/CATCH` + `ROLLBACK`; contrato `Respuestas/Mensaje` → el handler lo traduce a `EntRespuesta` (`estado/tipoMensaje/mensaje`).
- Validación: `@IdPerfil` existe en `R_Perfil`; ids de `@ActivosCsv` enteros; si el perfil no tiene ítems marcados, se desactivan todos (set vacío es válido).
- Codificación: mensajes fijos de SP sin tildes; handler UTF-8; `.js` con BOM y `EscaparAttr`.

## Pruebas (manuales)

Sin framework de pruebas. Verificación:
- **SQL directo:** ejecutar el script de SPs y el de arreglo de datos; probar `ListarMenuPerfil`, y `GuardarMenuPerfil` con un set que incluya un **hijo sin su padre** → confirmar que el SP **activó el padre** y que el estado quedó consistente; confirmar que el script de datos deja 0 casos huérfanos.
- **HTTP al handler:** POST JSON a `AdministrarMenuPerfil.ashx` (listar/guardar) revisando el JSON (tildes correctas).
- **Navegador:** elegir un perfil, marcar un submenú (ver que auto-marca el padre), guardar, e iniciar sesión con ese perfil para confirmar que el submenú **ahora sí aparece** en el sidebar.

## Fuera de alcance (YAGNI)

- **No** se modifica `Master.Master.cs` ni la lógica de render del sidebar.
- **No** se toca `PruebaMenu.aspx` (sigue creando ítems y editando ícono/referencia). Solo la *asignación por perfil* se mueve a la pantalla nueva; PruebaMenu no se borra en este sub-proyecto.
- Sin CRUD de ítems de menú en la pantalla nueva.
- Sin permisos de acción por perfil (eso es el sub-proyecto #3).
- Menú de dos niveles (no se soporta anidamiento más profundo, que hoy no existe).

## Entorno (referencia)

- BD: `Data Source=192.168.11.14; Initial Catalog=ReporTarea; User Id=sa` (la app usa su cadena; sqlcmd desde esta máquina por `tcp:192.168.11.14,1433`). La conectividad ha sido intermitente.
- MSBuild 2019 Community; solución `ReporteTareas.sln`, `Debug`; ejecutar vía PowerShell. La app requiere **IIS Express de 32 bits** (dependencia `Pechkin` x86).
