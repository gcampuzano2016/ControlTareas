# Página de inicio y "tipo" por perfil — Diseño

**Fecha:** 2026-07-25
**Estado:** Aprobado (diseño)
**Sub-proyecto:** #1 de 3 del pedido "administrar perfiles desde el sistema" (los otros dos —menús por perfil y permisos de acción— van en specs aparte).

## Objetivo

Volver **configurable desde el sistema** dos cosas que hoy están quemadas y duplicadas en `Login.aspx.cs`:

1. **Página de inicio por perfil** (nueva capacidad): hoy todos los perfiles caen en `Principal.aspx`. Se busca que cada perfil pueda entrar a una página distinta al iniciar sesión.
2. **El "tipo" por perfil** (`Session["Id_Usuario"]`): hoy un bloque de ~30 `if (Id_Perfil == N)` asigna este valor, que varias pantallas de reportes leen como filtro. Está **duplicado** en las dos rutas de login y **desalineado** (la segunda ruta mapea menos perfiles, dejando a algunos en `Id_Usuario=0`).

Ambas cosas se unifican en **una sola tabla por perfil** y **una sola pantalla** de administración.

## Contexto actual (verificado en código y BD)

- El menú del master ya es dinámico por `Session["Id_Perfil"]` vía `PerfilMenu` + `MenuDos` (fuera del alcance de este spec).
- `Login.aspx.cs`, en dos bloques (líneas ~129-170 y ~288-305):
  - Fija `Session["Id_Usuario"]` con `if (Id_Perfil == N)` (perfiles 2 y 3 → 1; el resto → = Id_Perfil).
  - Siempre hace `Response.Redirect("Principal.aspx")`.
  - El segundo bloque cubre menos perfiles → inconsistencia entre rutas.
- Catálogo de perfiles: `dbo.R_Perfil (Id_Perfil, Nombre, ...)`. `Id_Perfil` coincide con `Session["Id_Perfil"]` y `PerfilMenu.IdPerfil`.
- Catálogo de pantallas: `dbo.MenuDos (Id_Menu, Titulo, Href, ...)`; las pantallas navegables tienen `Href` no nulo.
- El combo de perfiles ya se llena con la acción `ListaComboPerfiles` de `ObtenerListaTareas.ashx` (reutilizable si conviene).

## Decisiones de diseño (acordadas)

- **Fallback:** si un perfil no tiene fila de configuración (o ante cualquier error), el login usa `Principal.aspx` + `Id_Usuario = 0`. Nunca bloquea el acceso (fail-safe). Preserva el comportamiento actual del día 1.
- **Origen de la página de inicio:** desplegable poblado desde `MenuDos` (Titulo + Href), no texto libre. Evita errores de tipeo y garantiza que la página exista.
- **Enfoque:** tabla dedicada + SPs + 3 capas (Entidad/Dato/Negocio) + handler + pantalla + refactor del Login. Mismo patrón que el módulo "Ventana de Aprobación". Se descartó agregar columnas a `R_Perfil` (toca tabla núcleo) y config en `web.config` (no administrable desde el sistema).

## Arquitectura

### 1. Modelo de datos

```sql
CREATE TABLE dbo.RTA_PerfilInicio
(
    IdPerfil        INT           NOT NULL,   -- = R_Perfil.Id_Perfil / Session["Id_Perfil"]
    Href            VARCHAR(150)  NOT NULL,   -- página de inicio (de MenuDos), ej. 'Principal.aspx'
    IdTipo          INT           NOT NULL,   -- valor para Session["Id_Usuario"] ("tipo")
    Estado          BIT           NOT NULL CONSTRAINT DF_RTA_PerfilInicio_Estado   DEFAULT(1),
    UsuarioRegistro VARCHAR(100)  NULL,
    FechaRegistro   DATETIME      NOT NULL CONSTRAINT DF_RTA_PerfilInicio_FechaReg DEFAULT(GETDATE()),
    CONSTRAINT PK_RTA_PerfilInicio PRIMARY KEY (IdPerfil)
);
```

**Seed inicial** (preserva el comportamiento actual): una fila por cada perfil hoy mapeado en `Login.aspx.cs`.
- Perfiles 2 y 3 → `Href='Principal.aspx'`, `IdTipo=1`.
- Resto de perfiles mapeados (4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,41) → `Href='Principal.aspx'`, `IdTipo = Id_Perfil`.
- Los perfiles no sembrados quedan cubiertos por el fallback (`Principal.aspx` / `IdTipo=0`).

### 2. Stored procedures

- **`Sp_RTA_ListarPerfilInicio`** — para la pantalla. `R_Perfil P LEFT JOIN RTA_PerfilInicio I ON I.IdPerfil = P.Id_Perfil`, filtrando perfiles activos de `R_Perfil`. Columnas: `IdPerfil, NombrePerfil, Href, TituloPagina, IdTipo`. `TituloPagina` se resuelve con `LEFT JOIN MenuDos` por `Href` para mostrar el nombre legible; `Href`/`IdTipo` en NULL indican "sin configurar".
- **`Sp_RTA_GuardarPerfilInicio @IdPerfil, @Href, @IdTipo, @UsuarioRegistro`** — upsert (UPDATE si existe, si no INSERT). Devuelve `Respuestas INT, Mensaje VARCHAR` (mismo contrato que `Sp_RTA_GuardarVentanaAprobacionJefe`). Valida que `@IdPerfil` exista en `R_Perfil` y que `@Href` no sea vacío.
- **`Sp_RTA_ObtenerPerfilInicio @IdPerfil`** — para el Login. Devuelve una fila `Href, IdTipo`. Si no hay configuración: `Href='Principal.aspx'`, `IdTipo=0`.
- **`Sp_RTA_ListarPaginasMenu`** — para el desplegable: `SELECT Titulo, Href FROM dbo.MenuDos WHERE ISNULL(Href,'') <> '' AND Href NOT LIKE 'Es Men%' ORDER BY Titulo` (excluye los "Es Menú Principal, No tiene referencia."). SP propio, por consistencia con el resto del módulo.

### 3. Capas C#

- **`CapaEntidad/EntPerfilInicio.cs`** — `IdPerfil, NombrePerfil, Href, TituloPagina, IdTipo`. Para las opciones del desplegable se reutiliza `EntMenuDos` (ya tiene `Titulo` y `Href`).
- **`CapaDato/DaoPerfilInicio.cs`** — `ListarPerfilInicio()`, `GuardarPerfilInicio(...)`, `ObtenerPerfilInicio(int idPerfil)`, `ListarPaginasMenu()`. Usa `DaoReporTareaAranda`.
- **`CapaNegocio/NegPerfilInicio.cs`** — pass-through a las anteriores.

### 4. Handler + pantalla

- **`ReporteTareas/Formulario/AdministrarPerfilInicio.ashx.cs`** — acciones:
  - `ListaPerfilInicio` → grilla de perfiles con su config.
  - `ListaPaginas` → opciones del desplegable (MenuDos).
  - `GuardarPerfilInicio {idPerfil, href, idTipo, usuarioRegistro}`.
  - `Response.ContentEncoding = Encoding.UTF8`; serialización JSON escapando no-ASCII (mismo `ToJson` del handler de Ventana de Aprobación).
- **`ReporteTareas/Formulario/ParametrizacionPerfilInicio.aspx` (+ .cs, .designer.cs)** — grilla con una fila por perfil:
  - *Perfil* (nombre, solo lectura),
  - *Página de inicio* (`<select>` poblado desde `ListaPaginas`, valor = `Href`),
  - *Tipo (Id_Usuario)* (input numérico),
  - acción **Guardar** por fila.
  - Campos ocultos `txtLoginUsuario`/`txtUsuario` como en las otras pantallas, para `usuarioRegistro`.
- **`ReporteTareas/js/parametrizacionPerfilInicio.js`** (UTF-8 con BOM) — cargar páginas, cargar grilla, guardar fila, mensajes.
- **Registro en menú:** insertar la pantalla en `MenuDos` bajo el padre "Manejo de Perfiles" (Id_Menu 20042) y habilitarla en `PerfilMenu` para los perfiles admin **1, 2, 18, 19** (Estado=0). Script SQL siguiendo el patrón de `2026-07-20-ventana-aprobacion-menu.sql`.

### 5. Refactor del Login

Reemplazar **ambos** bloques `if (Id_Perfil == N)` + su `Response.Redirect("Principal.aspx")` por:

```csharp
var inicio = NegPerfilInicio.ObtenerPerfilInicio(objUsuario.Id_Perfil); // devuelve Href e IdTipo
Session["Id_Usuario"] = inicio.IdTipo;      // default 0 (fail-safe)
Response.Redirect(inicio.Href);             // default "Principal.aspx"
```

- Elimina el código quemado y la duplicación; unifica ambas rutas de login en la misma lógica.
- El `else → Session["Id_Usuario"] = 0` actual queda cubierto por el fallback del SP/capa.

## Flujo de datos

- **Login:** usuario autenticado → `NegPerfilInicio.ObtenerPerfilInicio(Id_Perfil)` → set `Session["Id_Usuario"]` + `Response.Redirect(Href)`.
- **Administración:** pantalla → handler → Negocio → Dato → SPs (`ListarPerfilInicio` / `ListaPaginas` / `GuardarPerfilInicio`).

## Manejo de errores

- **Fail-safe en Login:** si `ObtenerPerfilInicio` devuelve vacío o lanza excepción, usar `Principal.aspx` + `Id_Usuario=0`. El acceso nunca se bloquea por este módulo.
- **Guardar:** validaciones en SP (perfil existente, Href no vacío) con contrato `Respuestas/Mensaje`; el handler traduce a `EntRespuesta` (`estado/tipoMensaje/mensaje`).
- **Codificación:** mensajes fijos de SP sin tildes; handler UTF-8; `.js` con BOM.

## Pruebas (manuales)

No hay framework de pruebas automatizadas. Verificación:
- **SQL directo:** ejecutar el script; probar `GuardarPerfilInicio`/`ObtenerPerfilInicio`/`ListarPerfilInicio` con un perfil real (upsert, fallback).
- **HTTP al handler:** POST JSON a `AdministrarPerfilInicio.ashx` revisando el JSON (acciones lista/guardar; tildes correctas en nombres de perfil).
- **Login:** con un perfil configurado, verificar redirección a su página e `Id_Usuario` correcto; con un perfil sin config, verificar fallback a `Principal.aspx` + `Id_Usuario=0`.
- **Navegador:** la pantalla lista perfiles, el desplegable trae las pantallas de MenuDos, guardar por fila muestra éxito y persiste.

## Fuera de alcance (YAGNI)

- Sin roles ni permisos de acción (es el sub-proyecto #3).
- Sin administrar el menú por perfil (es el #2, ya existe `PruebaMenu.aspx`).
- Sin historial de cambios de configuración.
- Sin múltiples páginas de inicio por perfil: una página + un tipo por perfil.

## Entorno (referencia)

- BD: `Data Source=192.168.11.14; Initial Catalog=ReporTarea; User Id=sa` (la app negocia por su cadena; el server responde por `tcp:192.168.11.14,1433`).
- MSBuild 2019 Community; solución `ReporteTareas.sln`, `Debug`.
- La app requiere **IIS Express de 32 bits** (dependencia `Pechkin` x86).
