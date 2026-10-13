# Inactivar jefe en el módulo "Ventana de aprobación" — Diseño

**Fecha:** 2026-07-24
**Base:** ReporTarea
**Pantalla:** `Formulario/ParametrizacionVentanaAprobacion.aspx`
**Handler:** `Formulario/AdministrarVentanaAprobacion.ashx`

## Objetivo

Permitir marcar a un jefe como **inactivo únicamente en este módulo**, de modo que no
aparezca en la tabla de "Ventana de aprobación". La exclusión es **reversible** desde la
misma pantalla. No afecta el login del usuario, su estado global, ni ningún otro módulo.

## Decisiones tomadas (brainstorming)

1. **Alcance:** solo ocultar de esta tabla. No bloquea aprobaciones ni toca `R_Usuarios`.
2. **Reactivación:** checkbox **"Mostrar inactivos"** en la pantalla; por defecto solo se ven
   los activos. Al marcarlo se ven todos, con los inactivos marcados y con opción de reactivar.
3. **Almacenamiento:** tabla nueva dedicada (Opción A), independiente de la ventana de
   aprobación. Identidad por `MailJefe` (email), igual que `RTA_VentanaAprobacionJefe`.
4. **UX:** acción **directa** (sin modal de confirmación previa), porque es reversible.
   Se muestra un mensaje de éxito tras inactivar/reactivar.

## 1) Base de datos

### Tabla nueva `dbo.RTA_JefeExcluidoVentana`

| Columna | Tipo | Notas |
|---|---|---|
| Id | INT IDENTITY(1,1) PK | |
| MailJefe | VARCHAR(150) NOT NULL | **UNIQUE** (una fila por jefe) |
| Estado | BIT NOT NULL DEFAULT(1) | 1 = inactivo/oculto en este módulo; 0 = activo |
| UsuarioRegistro | VARCHAR(100) NULL | quién realizó el último cambio |
| FechaRegistro | DATETIME NOT NULL DEFAULT(GETDATE()) | fecha del último cambio |

Se conserva una sola fila por `MailJefe`; reactivar pone `Estado = 0` (mantiene historial de
quién y cuándo). El jefe se considera "inactivo" solo si existe fila con `Estado = 1`.

### SP modificado `Sp_RTA_ListarJefesVentanaAprobacion`

- Nuevo parámetro `@IncluirInactivos BIT = 0` (default 0 = comportamiento actual).
- `LEFT JOIN dbo.RTA_JefeExcluidoVentana e ON e.MailJefe = j.MailJefe AND e.Estado = 1`.
- Nuevo campo de salida `Inactivo = CASE WHEN e.Id IS NOT NULL THEN 1 ELSE 0 END`.
- Filtro:
  - `@IncluirInactivos = 0` → `WHERE ... AND e.Id IS NULL` (solo activos).
  - `@IncluirInactivos = 1` → devuelve todos (activos + inactivos marcados).
- El resto (conteo de colaboradores, ventana vigente, filtro de búsqueda, orden) se mantiene.

### SP nuevo `Sp_RTA_ExcluirJefeVentana`

- Parámetros: `@MailJefe VARCHAR(150)`, `@Excluir BIT`, `@UsuarioRegistro VARCHAR(100) = NULL`.
- Upsert por `MailJefe`:
  - Si existe fila → `UPDATE Estado = @Excluir`, actualiza `UsuarioRegistro` y `FechaRegistro`.
  - Si no existe y `@Excluir = 1` → `INSERT` con `Estado = 1`.
  - Si no existe y `@Excluir = 0` → no hace nada (ya está activo).
- Valida `@MailJefe` no vacío.
- Devuelve `SELECT Respuestas, Mensaje` (mismo contrato que `Sp_RTA_GuardarVentanaAprobacionJefe`):
  - `Respuestas = 1` y mensaje de éxito ("Jefe inactivado…" / "Jefe reactivado…").
  - `Respuestas = 0` con mensaje si falla la validación.
- `BEGIN TRY/CATCH` con transacción, igual que el SP de guardar.

## 2) Backend (C#)

### `CapaEntidad/EntVentanaAprobacionJefe.cs`
- Agregar propiedad `public int Inactivo { get; set; }` (0/1).

### `CapaDato/DaoVentanaAprobacion.cs`
- `ListarJefes(string filtro, bool incluirInactivos)`:
  - Agrega parámetro `@IncluirInactivos` (`SqlDbType.Bit`).
  - Lee el nuevo campo `Inactivo` en el mapeo del `SqlDataReader`.
- Nuevo `ExcluirJefe(string mailJefe, bool excluir, string usuarioRegistro)`:
  - Llama `Sp_RTA_ExcluirJefeVentana`, mapea `Respuestas`/`Mensaje` a `EntRespuesta`
    (mismo patrón que `GuardarVentana`).

### `CapaNegocio/NegVentanaAprobacion.cs`
- `ListarJefes(string filtro, bool incluirInactivos)` → delega al Dao.
- `ExcluirJefe(string mailJefe, bool excluir, string usuarioRegistro)` → delega al Dao.

### Handler `Formulario/AdministrarVentanaAprobacion.ashx.cs`
- Acción `ListaJefes`: leer parámetro opcional `incluirInactivos` (default false) y pasarlo.
- Nueva acción `ExcluirJefe`: parámetros `mailJefe`, `excluir` (1/0 o bool), `usuarioRegistro`.
  Valida `mailJefe` no vacío y llama a `NegVentanaAprobacion.ExcluirJefe`.

## 3) Frontend

### `Formulario/ParametrizacionVentanaAprobacion.aspx`
- Agregar un **checkbox "Mostrar inactivos"** (`chkMostrarInactivos`) junto al buscador,
  desmarcado por defecto. Al cambiar, dispara `BuscarJefes()`.

### `js/parametrizacionVentanaAprobacion.js`
- `BuscarJefes()`: enviar `incluirInactivos` = estado del checkbox en los `parameters` de
  `ListaJefes`.
- `RenderTablaJefes(lista)`:
  - Columna de acción por fila según `item.Inactivo`:
    - Activo → ícono **Inactivar** (`fa-ban`, title "Inactivar de este módulo") → `InactivarJefe(i)`.
    - Inactivo → fila **atenuada** (clase/estilo) + etiqueta `<span class="label label-default">Inactivo</span>`
      + ícono **Reactivar** (`fa-check`, title "Reactivar") → `ActivarJefe(i)`.
  - El ícono de **Asignar ventana** se mantiene para activos; para inactivos puede ocultarse
    (no tiene sentido asignar ventana a un jefe oculto).
- Nuevas funciones:
  - `InactivarJefe(i)` → `PostVentana("ExcluirJefe", {mailJefe, excluir:1, usuarioRegistro})` →
    en éxito, `MostrarMensaje(...)` y `BuscarJefes()`.
  - `ActivarJefe(i)` → igual con `excluir:0`.
- `usuarioRegistro` se toma de `#ContentPlaceHolder1_txtLoginUsuario` (igual que `GuardarVentana`).

## Flujo

1. Al abrir, `BuscarJefes()` pide la lista con `incluirInactivos = false` → solo activos.
2. Usuario clic en 🚫 de un jefe → `ExcluirJefe(excluir=1)` → refresca → el jefe desaparece.
3. Usuario marca "Mostrar inactivos" → `BuscarJefes()` con `incluirInactivos = true` →
   se ven todos; los inactivos atenuados con ✔️ para reactivar.
4. Clic en ✔️ → `ExcluirJefe(excluir=0)` → refresca → el jefe vuelve a estar activo.

## Fuera de alcance (YAGNI)

- No se bloquea la aprobación de actividades (la validación de ventana no cambia).
- No hay modal de confirmación (acción directa reversible).
- No se toca `R_Usuarios` ni el estado global del usuario.
- No hay auditoría más allá de `UsuarioRegistro`/`FechaRegistro` de la última acción.

## Codificación

Los archivos nuevos/editados siguen la convención ya corregida del módulo:
`.js` en **UTF-8 con BOM** y el handler con `Response.ContentEncoding = Encoding.UTF8`
(ver arreglos previos de codificación).
