# Permisos desde el portal — Diseño

**Fecha:** 2026-10-14
**Estado:** aprobado en conversación; pendiente de revisión escrita
**Rama:** `ProyectoNuevosCambios`
**Antecedentes:**
- `2026-07-27-menus-por-perfil-design.md` (menú por perfil)
- `2026-08-03-modulos-extra-por-usuario-design.md` (menú por usuario). Ahí se dejó pendiente el sub-proyecto «permisos de acción», que es este.
- `DESPLIEGUE.md` §16.5 (acceso por usuario al Tablero de Costos, 2026-10-14)

## Objetivo

Que quién entra a cada pantalla sensible, y quién puede hacer cada acción delicada dentro de ella, se administre **desde el portal** y no con listas de perfiles escritas en el código.

Hoy hay 13 listas quemadas en 8 módulos. Darle algo a una persona exige compilar y publicar: fue el caso del 13-10 con el Tablero de Costos.

## Lo que se acordó

| Decisión | Valor |
|---|---|
| Alcance | pantallas **y** acciones |
| Pantallas con control obligatorio | las 8 sensibles de este documento más las dos de administración del menú. Las demás (~85) se suman después, con una línea cada una |
| Respaldo | el perfil **18 (Super Admin) puede todo**, siempre. Es la única regla fija del código: así nadie queda sin poder administrar permisos |
| Modelo | los permisos de una persona son **los de su perfil más sus adicionales**. A nadie se le quitan los de su perfil, igual que hoy en «Menú por usuario» |
| Dónde se administra | opción **A**: las acciones aparecen dentro del árbol de las pantallas que ya existen, «Menú por perfil» y «Menú por usuario». No hay pantalla aparte |
| Caché | los permisos se guardan en la sesión **2 minutos**. Un permiso quitado tarda hasta 2 minutos en cortarse |

## Contexto verificado (producción, 2026-10-14)

### Las listas quemadas

| Archivo | Lista | Uso |
|---|---|---|
| `CapaNegocio/NegTableroCostos.cs` | `PerfilesAutorizados {14,18}` | entrar al Tablero, junto con la opción por usuario (`Sp_RTA_TableroCostosAccesoUsuario`) |
| `AdministrarHorasExtras.ashx.cs` | `PerfilesAutorizados {2,14,18}` | entrar a Horas Extras y descargar (`DescargarHorasExtras`, `HorasExtras.aspx.cs`) |
| `AdministrarHorasExtras.ashx.cs` | `PerfilesQueReabren {18}` | reabrir un período cerrado |
| `AdministrarHorasExtras.ashx.cs` | `PerfilesQueEditanMaestro {14,18}` | completar datos y ajustes de sueldo |
| `HorasExtras.aspx.cs` | `idPerfil == 18` | mostrar el botón de reabrir |
| `AdministrarParametrosHE.ashx.cs` | `PerfilesAutorizados {14,18}` | parámetros de Horas Extras (también `ParametrizacionHorasExtras.aspx.cs`) |
| `AdministrarCurvaAprendizaje.ashx.cs` | `PerfilesAutorizados {14,18}` | todo el módulo Curva (también `CurvaAcceso.cs`) |
| `CapaNegocio/NegPerfilAcceso.cs` | `PerfilesRRHH {14,18}` | ver y corregir el perfil de otra persona (`PerfilIdentidad`, `PerfilesPersonal.aspx.cs`) |
| `AdministrarPerfiles.ashx.cs` | `PerfilesAutorizados {2,18,19}` | administración de perfiles |
| `AdministrarUsuarios.ashx.cs` | `PerfilesQueAdministranUsuarios {14,18,19}` | administración de usuarios |
| `AdministrarUsuarios.ashx.cs` | `PerfilesQueCambianPerfil {18}` | cambiar el perfil de un usuario |
| `ParametrizacionUsuarios.aspx.cs` | `idPerfil == 18` | mostrar el control de cambiar perfil |

### Hueco de seguridad que este cambio cierra

- `AdministrarMenuPerfil.ashx` **no exige sesión**: ni siquiera implementa `IRequiresSessionState`.
- `AdministrarMenuUsuario.ashx` exige sesión iniciada, pero con **cualquier** perfil.

Hoy cualquiera con la dirección del handler puede asignarse opciones de menú. Cuando el menú pase a decidir el acceso, eso equivaldría a poder darse cualquier permiso. **Las dos quedan protegidas en este cambio, no después.**

### Cruce código ↔ menú

Se cruzó cada lista con `PerfilMenu` (`Estado = '0'`) y `R_UsuarioMenu` (`Estado = 'A'`), contando usuarios activos (`Usuario_Estado = 'A'` y `EstadoUsuario IS NULL`).

- **Coinciden:** Tablero, Horas Extras (el perfil 2 sí la tiene en el menú), Curva, Perfiles del personal y Administración de usuarios.
- **Opciones por usuario activas:** una persona de perfil 2 tiene Horas Extras (redundante con su perfil) y el Tablero de Costos. Las dos se respetan.
- **Diferencias, y lo que el usuario decidió:**

| # | Pantalla | Código | Menú | Decisión |
|---|---|---|---|---|
| 1 | `ParametrizacionHorasExtras.aspx` | 14 sí | 14 no (5 personas) | **gana el código**: se agrega al menú del 14 |
| 2 | `ParametrizacionPerfiles.aspx` | 2 sí | 2 no (2 personas) | **gana el menú**: el 2 deja de entrar |
| 3 | `ParametrizacionPerfiles.aspx` | 14 no | 14 sí (5 personas) | **gana el código**: se quita del menú del 14 |
| 4 | `ParametrizacionMenuPerfil.aspx` y `ParametrizacionMenuUsuario.aspx` | sin protección | 14, 18, 19 | **solo el 18**: se quitan del menú del 14 y del 19 (el 19 no tiene usuarios activos) |

**Fuera de esas cuatro, nadie gana ni pierde acceso el día que se publique.**

## Arquitectura

### 1. Datos

Las pantallas siguen en `PerfilMenu` y `R_UsuarioMenu`. Para las acciones hay tres tablas nuevas, sin FK como el resto de la base:

```sql
RTA_Accion
  Id_Accion    INT IDENTITY PRIMARY KEY
  Clave        VARCHAR(60)  NOT NULL UNIQUE   -- 'HorasExtras.Reabrir'
  Id_Menu      INT          NOT NULL          -- pantalla a la que pertenece (MenuDos)
  Titulo       VARCHAR(100) NOT NULL
  Descripcion  VARCHAR(300) NULL
  Estado       CHAR(1)      NOT NULL DEFAULT 'A'

RTA_PerfilAccion
  IdPerfil, Id_Accion, Estado CHAR(1), Usuario_Registro VARCHAR(50), Fecha_Registro DATETIME
  UNIQUE (IdPerfil, Id_Accion)

RTA_UsuarioAccion
  Cod_Usuario VARCHAR(50), Id_Accion, Estado CHAR(1), Usuario_Registro, Fecha_Registro
  UNIQUE (Cod_Usuario, Id_Accion)
```

- **Estados:** `'A'`/`'I'`, como `R_UsuarioMenu`. Un permiso se desactiva, no se borra.
- **Pantalla de cada acción:** la siembra la resuelve por `Href` y nunca por un `Id_Menu` escrito a mano, porque el Id cambia entre ambientes.

**Catálogo inicial (3 acciones):**

| Clave | Pantalla | Siembra en `RTA_PerfilAccion` |
|---|---|---|
| `HorasExtras.Reabrir` | `HorasExtras.aspx` | ninguna (hoy solo el 18) |
| `HorasExtras.EditarMaestro` | `HorasExtras.aspx` | perfil 14 |
| `Usuarios.CambiarPerfil` | `ParametrizacionUsuarios.aspx` | ninguna (hoy solo el 18) |

### 2. La regla

La regla vive en un solo lugar: `CapaNegocio/NegPermisos`, una clase pura que no toca la base.

- **PuedeEntrar(href):** perfil 18; o `href` está en las pantallas de su perfil; o está en sus adicionales. La comparación ignora mayúsculas.
- **PuedeHacer(clave):** perfil 18; o (PuedeEntrar(pantalla de la acción) **y** la acción está en su perfil o en sus adicionales).
- **Clave desconocida o inactiva:** se niega.
- **No se pudieron cargar los permisos:** se niega.

Una acción sin su pantalla no vale nada, ni siquiera si quedó una fila suelta en la base.

### 3. Capas

- **`Sp_RTA_PermisosUsuario @IdPerfil, @CodUsuario`** (solo lectura) devuelve dos resultados:
  1. los `Href` de pantallas, perfil ∪ adicionales;
  2. las `Clave` de acciones activas, perfil ∪ adicionales, junto con el `Href` de su pantalla.

  Compara `Cod_Usuario` igual que `Sp_RTA_ConsultarMenuPerfilUsuario`.
- **`CapaEntidad/EntPermisos`:** dos conjuntos, pantallas y acciones con su pantalla. Ignoran mayúsculas.
- **`CapaDato/DaoPermisos.Cargar(idPerfil, codUsuario)`:** si falta uno de los dos resultados, lanza una excepción.
- **`CapaNegocio/NegPermisos`:** la regla del punto 2.
- **`ReporteTareas/clases/Permisos`:** lo único que llaman pantallas y handlers.
  - `Permisos.PuedeEntrar(HttpContext, string href)` y `Permisos.PuedeHacer(HttpContext, string clave)`.
  - Exige sesión completa: `UserLogin` y `Cod_Usuario` con texto, con el mismo criterio que `AdministrarTableroCostos.Autenticado`.
  - Caché en `Session["Permisos"]` con hora de carga y la identidad (perfil y código) con que se cargó. Se recarga a los 2 minutos o si cambia la identidad.
  - Ante cualquier error: `Trace.TraceError` y se niega.
- **`ReporteTareas/clases/Acciones`:** constantes con las claves, para que una clave mal escrita no compile.

### 4. Cambios al código existente

- **Las 13 listas se reemplazan:**
  - entrar a una pantalla → `PuedeEntrar` con el `Href` de esa pantalla;
  - reabrir, completar datos y cambiar perfil → `PuedeHacer`.
- **Curva de Aprendizaje:** el handler y sus 6 pantallas usan `PuedeEntrar("CurvaColaboradores.aspx")`. Varias de esas pantallas no están en el menú porque se abren desde otra.
  - El menú tiene tres opciones de Curva: «Curva de aprendizaje», «Curva: nuevo colaborador» y «Curva: dashboard». **La que da acceso al módulo es la primera.** Marcar solo una de las otras dos dibuja la opción, pero el módulo responde «No tiene permisos».
  - Las tres se dan juntas, como hoy en la siembra para 14 y 18. La pantalla de administración lo avisa.
- **Ver y corregir el perfil ajeno:** `NegPerfilAcceso` deja de tener su lista y recibe el resultado de `PuedeEntrar("PerfilesPersonal.aspx")`.
- **Descarga de Horas Extras:** usa el permiso de la pantalla.
- **`AdministrarMenuPerfil.ashx`:** implementa `IRequiresSessionState` y exige `PuedeEntrar("ParametrizacionMenuPerfil.aspx")`.
- **`AdministrarMenuUsuario.ashx`:** exige `PuedeEntrar("ParametrizacionMenuUsuario.aspx")`.
- **Botones que hoy dependen del perfil** (reabrir, completar datos, cambiar perfil): preguntan por la acción. Mostrarlos es solo cortesía; la barrera de verdad sigue en el handler.
- **Tablero de Costos:** `NegTableroCostos.PuedeVer` y `DaoTableroCostos.AccesoPorUsuario` se reemplazan por `PuedeEntrar("TableroCostos.aspx")`. El procedimiento `Sp_RTA_TableroCostosAccesoUsuario` queda sin uso y no se borra en este cambio.

### 5. Administración en el portal

`ParametrizacionMenuPerfil.aspx` y `ParametrizacionMenuUsuario.aspx`, con su `.js`:

- **El árbol muestra las acciones** de cada pantalla como hijos, con otro estilo (⚙).
- **Marcar una acción** marca su pantalla y el grupo de arriba. **Desmarcar una pantalla** desmarca sus acciones y avisa.
- **En «Menú por usuario»**, lo heredado del perfil sale marcado y bloqueado, con la etiqueta *(perfil)*. Lo adicional sale editable, con la etiqueta *EXTRA*. Igual que hoy, ahora también para las acciones.
- **El perfil 18 no se edita.** El árbol sale todo marcado, con la nota «Super Admin: tiene todos los permisos».
- **Guardar es una sola operación:** menú y acciones en una transacción, con `Usuario_Registro` tomado de la sesión.
  - Se hace con dos procedimientos nuevos, `Sp_RTA_GuardarPermisosPerfil` y `Sp_RTA_GuardarPermisosUsuario`.
  - Cada uno reproduce la lógica de menú de su par actual (descartar lo que viene del perfil, agregar el padre, upsert de estado) y además guarda las acciones.
  - Los procedimientos actuales (`Sp_RTA_GuardarMenuPerfil`, `Sp_RTA_GuardarMenuUsuario`) **no se tocan**: los usan los binarios viejos mientras dura la publicación. Esa duplicación es transitoria y se elimina en un cambio posterior.
- **Al guardar se cumple la invariante:** si llega una acción sin su pantalla, el procedimiento agrega la pantalla, igual que hoy agrega el padre de un hijo.
- **Para leer**, `Sp_RTA_ListarMenuPerfil` y `Sp_RTA_ListarMenuUsuario` siguen igual. Las acciones se piden con dos procedimientos nuevos: `Sp_RTA_ListarAccionesPerfil` y `Sp_RTA_ListarAccionesUsuario`.

### 6. Scripts SQL

1. **`docs/sql/2026-10-14-permisos-esquema.sql`:** crea las tablas, el catálogo, la siembra y los procedimientos nuevos.
   - Es repetible.
   - No modifica ningún procedimiento existente.
   - Termina con `VEREDICTO`.
2. **`docs/sql/2026-10-14-permisos-ajustes-menu.sql`:** aplica las cuatro decisiones del cruce.
   - Las filas de `PerfilMenu` se pasan a `Estado = '1'`, no se borran.
   - Es repetible y termina con `VEREDICTO`.

Los dos se pueden correr **antes** de los binarios. Con los binarios viejos, lo único que cambia es lo que se ve en el menú.

## Manejo de errores

- **Fallo al cargar permisos:** se niega y queda en el log. Las pantallas redirigen a `Principal.aspx`; los handlers responden 403 con «No tiene permisos para esta pantalla.».
- **Sesión vencida o incompleta:** 401 en los handlers y login en las pantallas, como hoy.
- **Guardar permisos:** `TRY/CATCH` con `ROLLBACK` y el contrato `Respuestas`/`Mensaje` de siempre. Los mensajes del procedimiento van sin tildes; el handler responde en UTF-8.
- **Claves de acción en lo que manda la pantalla:** se validan contra el catálogo. Lo desconocido se rechaza antes de escribir.

## Pruebas

- **Unitarias de `NegPermisos`** (MSTest, `CapaPruebas`):
  - el 18 entra a todo y hace todo, aunque no tenga filas;
  - pantalla por perfil, por adicional, y ninguna de las dos;
  - una acción con su pantalla vale y sin su pantalla no;
  - acción por perfil y por adicional;
  - clave desconocida;
  - permisos sin cargar (`null`) y error de carga;
  - mayúsculas distintas en el `Href`.
- **Unitarias de la caché de `Permisos`**, con una fuente de permisos inyectable: recarga a los 2 minutos y cuando cambia la identidad.
- **SQL:** un script de verificación de solo lectura repite el cruce y confirma que, fuera de las cuatro decisiones, el acceso es el mismo de antes.
- **Navegador:** con perfil 18, en IIS Express. Lista corta para que el usuario pruebe en producción con alguien de Talento Humano (14) y con la persona de perfil 2 que tiene el Tablero como adicional:
  - entra o no a cada pantalla;
  - ve o no los botones de reabrir, completar datos y cambiar perfil;
  - escribir la dirección de una pantalla no permitida redirige a Principal.

## Publicación y reversa

**Orden:** los dos scripts SQL; después **todos juntos** `ReporteTareas.dll`, `CapaEntidad.exe`, `CapaDato.exe`, `CapaNegocio.exe`, los `.aspx` y los `.js` (con `?v=` subido).

**Reversa:**
- reponer los binarios anteriores, todos juntos;
- las tablas nuevas pueden quedar;
- si hiciera falta, el script de ajustes de menú tiene su reversa en un bloque comentado.

## Fuera de alcance

- Exigir el menú en las otras ~85 pantallas. Cada una se suma con una línea cuando se decida.
- Borrar `Sp_RTA_TableroCostosAccesoUsuario` y los procedimientos `Sp_RTA_GuardarMenu*` viejos.
- Roles independientes del perfil (opción C).
- Que el perfil 19 vuelva a administrar permisos. Se puede dar desde el portal cuando haga falta.
