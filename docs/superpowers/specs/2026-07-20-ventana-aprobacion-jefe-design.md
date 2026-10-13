# Diseño: Ventana de aprobación por jefe inmediato

- **Fecha:** 2026-07-20
- **Proyecto:** Sistema Reporte Tareas (ReporTarea)
- **Autor:** Guillermo Campuzano (con Claude Code)
- **Rama sugerida:** feature/ventana-aprobacion-jefe

## 1. Objetivo

Permitir que un administrador defina, por cada **jefe inmediato**, una **ventana de fechas
(desde – hasta)** dentro de la cual ese jefe puede aprobar las actividades de sus
colaboradores. Fuera de esa ventana, el sistema le impide aprobar.

Los jefes inmediatos son los valores distintos de `R_Usuarios.MailCodJefeInm`
(actualmente 22 correos). Este campo contiene el **email** del jefe.

## 2. Reglas de negocio (confirmadas)

1. La ventana es un **rango de fechas de calendario exactas** (no un patrón recurrente).
2. **Una sola ventana vigente por jefe**: se edita/sobrescribe (no se guarda historial).
3. **Jefe sin ventana registrada → SE PERMITE aprobar** (restricción opt-in: solo se
   bloquea a quien tenga ventana registrada).
4. El bloqueo evalúa **la fecha actual del servidor** contra la ventana del jefe.
5. Alcance de aplicación: pantalla **AprobacionTareasJefatura** (acciones `AprobarTarea`
   y `AprobarTareaIndividual`).
6. Visibilidad en menú: perfiles **1, 2, 18, 19**.

## 3. Confidencialidad de la credencial

La conexión a `192.168.11.14 / ReporTarea` **ya existe** en
`CapaDato/DaoReporTareaAranda.cs` (método `conectar()`). Todo el código nuevo la
**reutiliza**; no se escribe la credencial en ningún punto adicional. (Nota fuera de
alcance: la contraseña está hardcodeada en el fuente desde antes; migrarla a un almacén
seguro es un trabajo separado.)

## 4. Identificación del jefe en tiempo de aprobación

`MailCodJefeInm` es el email del jefe. En `AdministrarTarea.ashx.cs`, los métodos
`AprobarTareas` y `AprobarTareasIndividual`:
1. Desencriptan `campos["session"]` → `IdUsuarioSession` (Cod_Usuario del jefe logueado).
2. Ya ejecutan `usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession)`,
   que devuelve un `EntUsuario` con `E_Mail`.

Por lo tanto el email del jefe (`usuario.E_Mail`) **ya está disponible** en el punto de
aprobación. La validación se hace contra `RTA_VentanaAprobacionJefe.MailJefe = usuario.E_Mail`.

## 5. Base de datos

### 5.1 Tabla nueva: `dbo.RTA_VentanaAprobacionJefe`

| Columna | Tipo | Restricción |
|---|---|---|
| Id | INT IDENTITY(1,1) | PK |
| MailJefe | VARCHAR(150) | UNIQUE, NOT NULL (= `MailCodJefeInm`) |
| FechaDesde | DATE | NOT NULL |
| FechaHasta | DATE | NOT NULL |
| Estado | BIT | NOT NULL DEFAULT 1 (1 activo / 0 inactivo) |
| UsuarioRegistro | VARCHAR(100) | NULL |
| FechaRegistro | DATETIME | NOT NULL DEFAULT GETDATE() |

Índice único en `MailJefe` para garantizar una ventana por jefe.

### 5.2 Stored procedures (patrón `Sp_RTA_`)

- **`Sp_RTA_ListarJefesVentanaAprobacion @Filtro VARCHAR(150)`**
  Devuelve la lista de jefes (correos distintos de `MailCodJefeInm`) con:
  `MailJefe`, `NombreJefe` (resuelto desde `R_Usuarios.Nom_Usuario` vía
  `R_Usuarios.E_Mail = MailCodJefeInm`; si no existe usuario con ese correo, `NombreJefe`
  = el propio correo), `NumColaboradores`, y la ventana actual si existe
  (`FechaDesde`, `FechaHasta`, `TieneVentana`).
  El `@Filtro` filtra por correo o nombre (LIKE); vacío = todos.

- **`Sp_RTA_GuardarVentanaAprobacionJefe @MailJefe, @FechaDesde, @FechaHasta, @UsuarioRegistro`**
  Upsert por `MailJefe` (una sola ventana por jefe). Valida `@FechaHasta >= @FechaDesde`.
  Devuelve `Respuestas` (>0 éxito) y `Mensaje`, igual que `Sp_RTA_AsignarHorarioUsuario`.

- **`Sp_RTA_ValidarVentanaAprobacionJefe @MailJefe VARCHAR(150), @Fecha DATE`**
  Devuelve `PuedeAprobar` (BIT) y `Mensaje`.
  - Sin fila activa para `@MailJefe` → `PuedeAprobar = 1` (permitir).
  - Con fila activa y `@Fecha` entre `FechaDesde` y `FechaHasta` → `PuedeAprobar = 1`.
  - Con fila activa y `@Fecha` fuera del rango → `PuedeAprobar = 0` + mensaje explicativo
    (incluye el rango permitido).

## 6. Capas de aplicación (siguiendo el patrón existente)

### 6.1 CapaEntidad
`EntVentanaAprobacionJefe`: `MailJefe`, `NombreJefe`, `NumColaboradores`, `FechaDesde`
(string), `FechaHasta` (string), `TieneVentana` (int). Se reutiliza `EntRespuesta`.

### 6.2 CapaDato — `DaoVentanaAprobacion` (estático, como `DaoUsuarioHorario`)
- `List<EntVentanaAprobacionJefe> ListarJefes(string filtro)`
- `EntRespuesta GuardarVentana(string mailJefe, string fechaDesde, string fechaHasta, string usuarioRegistro)`
- `EntRespuesta ValidarVentana(string mailJefe, DateTime fecha)`
Todos abren conexión con `new DaoReporTareaAranda().conectar()`.

### 6.3 CapaNegocio — `NegVentanaAprobacion`
Fachada delgada que expone `ListarJefes`, `GuardarVentana`, `ValidarVentana` delegando
en el DAO (mismo estilo que `NegUsuarioHorario`).

## 7. Capa web

### 7.1 Handler nuevo: `Formulario/AdministrarVentanaAprobacion.ashx` (+ `.cs`)
Copia estructural de `AdministrarHorarioUsuario.ashx.cs`. Acciones:
- `ListaJefes` → `NegVentanaAprobacion.ListarJefes(filtro)` (JSON).
- `GuardarVentana` → valida entradas y llama `NegVentanaAprobacion.GuardarVentana(...)`.
Usa el mismo `ToJson` que escapa no-ASCII y `responseMessage(...)`.

### 7.2 Pantalla nueva: `Formulario/ParametrizacionVentanaAprobacion.aspx` (+ `.cs`)
Copia estructural de `ParametrizacionHorarioUsuario.aspx`:
- Master `Master.Master`, `ResponseEncoding="utf-8"`.
- Buscador + tabla de jefes (correo, nombre, # colaboradores, ventana actual, botón asignar).
- Modal "Asignar ventana de aprobación" con `FechaDesde`, `FechaHasta` (`input type=date`).
- Modal informativo reutilizado.
- Code-behind: redirección a login + `txtLoginUsuario` (usuario registro), idéntico patrón.

### 7.3 JS nuevo: `js/parametrizacionVentanaAprobacion.js`
Copia de `parametrizacionHorarioUsuario.js`:
- `PostVentana(action, parameters, onSuccess)` → POST a `AdministrarVentanaAprobacion.ashx`.
- `BuscarJefes()`, `RenderTablaJefes()`, `AbrirAsignar(i)`, `GuardarVentana()`.
- Validaciones cliente: ambas fechas requeridas y `FechaHasta >= FechaDesde`.

## 8. Aplicación del bloqueo (cambio mínimo)

En `Formulario/AdministrarTarea.ashx.cs`, dentro de `AprobarTareas` y
`AprobarTareasIndividual`, inmediatamente después de obtener `usuario`
(línea ~909 y ~964 respectivamente) e **antes** de llamar a
`NegTareas.RTA_CambioEstadoAprobacionTarea[Individual]`:

```csharp
EntRespuesta valVentana = NegVentanaAprobacion.ValidarVentana(usuario.E_Mail, DateTime.Now);
if (valVentana.estado == "0")
{
    return valVentana.SerializaToJson(); // fuera de ventana: no aprueba, muestra mensaje
}
```

`NegVentanaAprobacion.ValidarVentana` mapea el resultado del SP a `EntRespuesta`:
`estado="1"` si `PuedeAprobar=1`; `estado="0"` + `tipoMensaje="warning"` + `mensaje` si
`PuedeAprobar=0`. El modal de aprobación ya muestra `respuesta.mensaje`/`tipoMensaje`,
por lo que **no se modifica el JS de la pantalla de aprobación**.

## 9. Menú y permisos

Registrar en BD (`MenuDos` + `PerfilMenu`), recordando la **semántica invertida**:
`PerfilMenu.Estado='0'` MUESTRA, `'1'` OCULTA; y la opción hija solo se ve si su grupo
padre también está en `Estado=0` para el perfil.

- Nuevo registro en `MenuDos`: `Href='ParametrizacionVentanaAprobacion.aspx'`,
  `Es_Opcion_de_Menu=0`, bajo un grupo padre adecuado (candidato: padre 20 "Tareas",
  igual que "Parametrización de Horario"). **Verificar el padre y los Ids reales contra
  la BD actual antes de insertar.**
- Filas en `PerfilMenu` con `Estado=0` para perfiles **1, 2, 18, 19**, verificando que el
  grupo padre también esté visible para esos perfiles.

## 10. Archivos afectados

**Nuevos**
- `docs/superpowers/specs/2026-07-20-ventana-aprobacion-jefe-design.md`
- `CapaEntidad/EntVentanaAprobacionJefe.cs`
- `CapaDato/DaoVentanaAprobacion.cs`
- `CapaNegocio/NegVentanaAprobacion.cs`
- `ReporteTareas/Formulario/ParametrizacionVentanaAprobacion.aspx` (+ `.cs` + `.designer.cs`)
- `ReporteTareas/Formulario/AdministrarVentanaAprobacion.ashx` (+ `.cs`)
- `ReporteTareas/js/parametrizacionVentanaAprobacion.js`
- Script SQL: tabla + 3 SPs + inserts de menú (en `docs/.../sql/` o carpeta de scripts del proyecto).

**Modificados**
- `ReporteTareas/Formulario/AdministrarTarea.ashx.cs` (2 bloques de validación).
- Archivos de proyecto `.csproj` (incluir los nuevos archivos en la compilación).

## 11. Pruebas de aceptación

1. Jefe SIN ventana registrada → aprueba normalmente.
2. Jefe con ventana y HOY dentro del rango → aprueba normalmente.
3. Jefe con ventana y HOY fuera del rango → NO aprueba; ve mensaje con el rango permitido.
4. Guardar con `FechaHasta < FechaDesde` → rechazado (cliente y SP).
5. Reguardar la ventana de un jefe → actualiza la misma fila (una por jefe).
6. La pantalla aparece en el menú solo para perfiles 1, 2, 18, 19.
7. Los correos de `MailCodJefeInm` sin usuario en `R_Usuarios` aparecen listados
   (nombre = correo) y pueden recibir ventana.

## 12. Fuera de alcance

- Historial de ventanas por período.
- Aplicación del bloqueo en otras pantallas de aprobación (Revisor, Horas Extras, etc.).
- Migración de la credencial de conexión a un almacén seguro.
