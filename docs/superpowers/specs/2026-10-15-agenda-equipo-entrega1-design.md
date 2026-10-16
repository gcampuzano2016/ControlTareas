# Equipo y agenda (Entrega 1 de «Equipo, Calendario y Procesos») — Diseño

**Fecha:** 2026-10-15
**Estado:** aprobado en conversación; pendiente de revisión escrita
**Rama:** `ProyectoNuevosCambios`
**Pedido por:** Gerencia General
**Antecedentes:**
- Maqueta `equipo-completo-preview (6).html` (raíz del repo), revisada en navegador el 2026-10-15.
- `2026-10-14-permisos-desde-el-portal-design.md`: este módulo usa esa fachada de Permisos y agrega una acción.

## Objetivo

Que Gerencia y cada jefe vean la carga de trabajo de su gente semana a semana, que puedan repartir actividades con
entregables, y que cada persona lleve su propia agenda y marque su avance.

Es la primera de dos entregas del módulo «Equipo, Calendario y Procesos». La **Entrega 2** (procesos entre áreas con
plantillas que avanzan en orden) tendrá su propia especificación, plan y despliegue. Las **reuniones de Teams/Outlook**
son una fase aparte que depende de que TI apruebe una aplicación en Azure AD; nada de esta entrega depende de ellas.

**Las actividades de este módulo son un registro propio.** No son las tareas de servicio de Aranda
(`R_DetTareasAranda`, horas, Tablero de Costos), no se cruzan con ellas y no usan sus tablas.

## Lo que se acordó

| Decisión | Valor |
|---|---|
| Uso | los tres por igual: ver la carga por área y persona, que los jefes repartan trabajo y vean entregables, y (Entrega 2) seguir procesos entre áreas |
| Quién ve a quién | **por jerarquía**: Gerencia ve a todos; cada jefe ve a **toda su cadena hacia abajo**; cada colaborador ve solo lo suyo |
| Áreas | **catálogo propio** con color, administrado desde el portal, una área por persona; se precarga desde `R_Usuarios.Departamento`, que no se toca |
| Actividades | las asignan jefes y Gerencia, y cada persona **registra las propias**; la persona marca sus entregables |
| Fechas | cada actividad tiene **fecha de entrega**; vencida sin completar = **atrasada**; quien asigna puede cambiarla |
| Correos | **solo cuando pasa algo**; en esta entrega, cuando alguien le asigna una actividad a otra persona |
| Aspecto | **dentro del portal** (`Master.Master`), con la estructura de la maqueta y los colores claros del portal |
| Entrega | **dos entregas**: esta (equipo y agenda) y después procesos |

## Contexto verificado (producción, 2026-10-15)

### Personas y jerarquía

`R_Usuarios` tiene lo necesario: `Cod_Usuario`, `Nom_Usuario`, `E_Mail`, `Cod_Jefe_Inm`, `Departamento`, `Id_Perfil`,
`IdCliente` y las dos columnas de estado (`Usuario_Estado`, `EstadoUsuario`).

Usuarios internos (`IdCliente` 0 o NULL) por estado:

| `Usuario_Estado` | `EstadoUsuario` | Personas |
|---|---|---|
| `A` | NULL | 105 |
| `A` | 0 | 119 |
| `A` | 1 | 7 |
| `0` | 0 | 2 |

Con el criterio de los selectores de jefe del sistema (`Usuario_Estado = 'A'` y `EstadoUsuario` NULL) hay **105
activos**: todos tienen `Cod_Jefe_Inm`, ninguno es su propio jefe, 16 son jefes de alguien. La cima es
**Carlos Enrique Pinos Hernández** (`1074`, perfil 10 «Gerencia», `Cod_Jefe_Inm = '0'`).

Dos personas tienen un jefe que ese criterio deja fuera, porque el jefe tiene `EstadoUsuario = 0`:

| Persona | Su jefe |
|---|---|
| `1345` Pinos Agila Nathaly Pamela | `1087` Lasso Peñafiel Isabel Cristina |
| `3408342` Viteri Egas Verónica Elizabeth | `3178922` Luis Alfonso Salazar Vaca |

Ver la **pregunta abierta 1**.

### Departamentos (de los 105)

| Departamento | Personas |
|---|---|
| SERVICIOS | 46 |
| CAS | 20 |
| COMERCIAL | 16 |
| FINANCIERO | 7 |
| GTH | 4 |
| MARKETING | 2 |
| ADMINISTRACION / ADMINISTRACIÓN | 1 + 1 |
| OBSERVABILIDAD E IA | 1 |
| CIBERSEGURIDAD Y NETWORKING | 1 |
| *(vacío)* | 6 |

No coinciden con las 9 áreas de la maqueta (no hay Hybrid Cloud ni Compras; SERVICIOS agrupa a 46). Por eso el catálogo es
propio y `Departamento` solo sirve para la precarga: lo usan Vacaciones y Horarios y no se modifica.

### Perfil «Gerencia»

El perfil 10 «Gerencia» tiene tres personas activas: Carlos Pinos (`1074`), Rubén Suquillo (`1072`, Ciberseguridad y
Networking) y Rafael Parreño (`2641457`, Comercial). Ver la **pregunta abierta 2**.

### Infraestructura que se reutiliza

- **Permisos** (`ReporteTareas/clases/Permisos.cs`, `NegPermisos`): pantallas por menú y acciones en `RTA_Accion`, cada
  acción ligada a un `Id_Menu`.
- **Correo**: `EnvioCorreoHelper` envía por SMTP en el momento, desde la aplicación web. No hace falta el servicio
  `WinSerEnvioCorreo`.
- **Nombres libres**: no existe ningún objeto `AG_*` ni `Sp_RTA_Agenda*` en la base.

## Arquitectura

### 1. Datos

Todas las tablas nuevas llevan las columnas de auditoría habituales (`Usuario_Registro`, `Fecha_Registro`,
`Usuario_Modificacion`, `Fecha_Modificacion`) y **claves foráneas entre sí**. Hacia `R_Usuarios` no se crea clave
foránea: `Cod_Usuario` no es su clave primaria. Se valida en los procedimientos.

**`AG_Area`**: el catálogo.

| Columna | Tipo | Nota |
|---|---|---|
| `IdArea` | int identity, PK | |
| `Nombre` | varchar(100) | único |
| `Color` | char(7) | `#RRGGBB` |
| `Activa` | bit | un área no se borra, se desactiva |
| `EsSinArea` | bit | marca la fila «Sin área»; hay exactamente una y no se puede desactivar |

**`AG_PersonaArea`**: el área de cada persona.

| Columna | Tipo | Nota |
|---|---|---|
| `Cod_Usuario` | varchar(20), PK | una fila por persona |
| `IdArea` | int, FK a `AG_Area` | |

Una persona activa sin fila en `AG_PersonaArea` se trata como «Sin área»: así quien entra al sistema después de la
precarga aparece igual y Gerencia lo ve para asignarlo.

**`AG_Actividad`**

| Columna | Tipo | Nota |
|---|---|---|
| `IdActividad` | int identity, PK | |
| `Titulo` | varchar(200) | obligatorio |
| `Cod_Responsable` | varchar(20) | a quién le toca |
| `Cod_AsignadoPor` | varchar(20) | igual al responsable si la registró él mismo |
| `FechaEntrega` | date | nunca sábado ni domingo |
| `CompletadaManual` | bit | solo para actividades sin entregables |
| `FechaCompletada` | datetime NULL | se llena cuando queda completa, se vacía si deja de estarlo |
| `Anulada` | bit | una actividad no se borra, se anula |

**`AG_Entregable`**

| Columna | Tipo | Nota |
|---|---|---|
| `IdEntregable` | int identity, PK | |
| `IdActividad` | int, FK a `AG_Actividad` | |
| `Descripcion` | varchar(200) | |
| `Orden` | int | |
| `Hecho` | bit | |
| `Cod_Hecho`, `Fecha_Hecho` | | quién lo marcó y cuándo |

**`AG_CorreoBitacora`**: cada intento de correo.

| Columna | Tipo | Nota |
|---|---|---|
| `IdCorreo` | int identity, PK | |
| `IdActividad` | int, FK a `AG_Actividad` | |
| `Destinatario` | varchar(200) NULL | NULL si la persona no tiene correo |
| `Asunto` | varchar(300) | |
| `Enviado` | bit | el resultado **real** del envío |
| `Error` | varchar(1000) NULL | |
| `Fecha` | datetime | |

La fila se escribe **después** de conocer el resultado del envío, nunca antes.

**Para la Entrega 2:** cuando lleguen los procesos se agrega a `AG_Actividad` la columna que apunta al paso del proceso.
No se reserva ahora.

### 2. Las reglas (clases puras)

Viven en `CapaNegocio`, sin base, sesión ni `HttpContext`, igual que `NegPermisos`, y tienen pruebas en `CapaPruebas`.

**`NegAgendaEquipo`: quién ve a quién**
- Recibe la lista de personas activas (`Cod_Usuario`, `Cod_Jefe_Inm`) y un usuario, y devuelve su **cadena hacia abajo**:
  sus reportes, los reportes de ellos, y así.
- Recorre con un conjunto de visitados: un **ciclo** de jefes (A jefe de B y B jefe de A) no lo cuelga, y la persona no
  se incluye a sí misma por un ciclo.
- `PuedeVer(yo, persona)` = es él mismo, o la persona está en su cadena, o tiene `Agenda.VerTodos`.
- La lista de personas se carga una vez por consulta (son ~105) y la cadena se calcula en memoria.
- `Agenda.VerTodos` es una acción de Permisos ligada a la pantalla de la agenda: como toda acción, solo vale si
  además el perfil tiene «Equipo y agenda» en su menú. Por eso el script de menú da la pantalla a todos los perfiles
  internos. El perfil 18 la tiene siempre.

**Quién cuenta como activo** sale de **un solo procedimiento** (`Sp_RTA_AgendaPersonasActivas`): si la respuesta a la
pregunta abierta 1 cambia el criterio, se cambia en un lugar.

**`NegAgendaActividad`: qué se puede hacer**

| Acción | Quién puede |
|---|---|
| Registrarse una actividad propia | cualquiera, para sí mismo |
| Asignar a otra persona | quien puede verla, sin ser él mismo: un jefe de su cadena o quien tiene `Agenda.VerTodos` |
| Editar título, fecha o entregables, y anular | quien la asignó; cualquiera que hoy pueda asignarle a ese responsable; y el responsable si la actividad es propia (`Cod_AsignadoPor = Cod_Responsable`) |
| Marcar entregables y «completa» | solo el responsable |

El responsable **no puede cambiar la fecha** de una actividad que le asignó otra persona. Una actividad anulada no se
edita ni se marca.

**Estado** (se calcula, no se guarda; la fecha de hoy la pone el servidor):
- **Completa**: con entregables, todos `Hecho`; sin entregables, `CompletadaManual`.
- **Atrasada**: no está completa, no está anulada y `FechaEntrega < hoy`. Vencer hoy **no** es estar atrasada.

**Fechas:**
- `FechaEntrega` no puede ser sábado ni domingo.
- La fecha propuesta al crear es el **próximo día hábil** (lunes a viernes; un viernes propone el lunes).
- El cálculo con feriados (tabla `Feriado`) queda para la Entrega 2, donde lo usan los plazos de los pasos.

**Contadores** de las tarjetas, por persona y por área, para una semana: actividades con entrega en la semana, cuántas
completas, porcentaje de entregables hechos y atrasadas (estas, de cualquier fecha).

### 3. Capas

- **`CapaEntidad`**: `EntAgendaArea`, `EntAgendaPersona`, `EntAgendaActividad`, `EntAgendaEntregable`.
- **`CapaDato`**: `DaoAgenda`, que llama a los `Sp_RTA_Agenda*`. No decide nada.
- **`CapaNegocio`**: las reglas de arriba, puras, y `NegAgenda.Datos.cs` como único archivo que llama a `CapaDato`
  (mismo esquema que `NegPermisos`).
- **Web**:
  - `AdministrarAgenda.ashx`: lee la sesión, pregunta a las reglas antes de cada consulta y cada cambio, y devuelve JSON
    con `ContentEncoding` UTF-8. Lo que mande el navegador sobre quién es el usuario o qué perfil tiene **no se usa**.
  - `Agenda.aspx` y `ParametrizacionAreasAgenda.aspx`, con sus `.js`, en `Master.Master`.
  - En `Permisos.cs`: `Pantallas.Agenda`, `Pantallas.ParametrizacionAreasAgenda` y `Acciones.AgendaVerTodos`.

### 4. Pantallas

**«Equipo y agenda» (`Agenda.aspx`)**

La puede abrir **cualquier usuario interno con sesión**: cada uno ve lo suyo y, si es jefe, a su gente. Un usuario de
cliente (`IdCliente ≠ 0`) va a Principal.

- **Al entrar:** quien no tiene a nadie a cargo ni `Agenda.VerTodos` va directo a **su agenda**. Los demás llegan a la
  **vista de equipo**, con un botón «Mi agenda».
- **Vista de equipo:**
  - Buscador por nombre, selector de semana (‹ esta semana ›) y «+ Asignar actividad».
  - Una **tarjeta por área** con gente visible para el usuario: color, cantidad de personas, iniciales, y para la semana
    elegida actividades y **atrasadas** (en rojo si hay). «Sin área» aparece como una tarjeta más.
  - Clic en un área: sus personas, cada una con actividades de la semana, porcentaje de entregables y atrasadas.
  - Con texto en el buscador se muestran directo las personas que coinciden, solo entre las visibles.
- **Vista de agenda de una persona:**
  - Encabezado con nombre y área, selector de semana y «+ Asignar a esta persona» (o «+ Nueva actividad» en la propia).
  - **Franja de atrasadas** arriba, con todas las atrasadas aunque sean de semanas anteriores.
  - Cinco columnas, lunes a viernes, con hoy resaltado. Cada actividad en su fecha de entrega: título, avance de
    entregables (1/2), estado (completa en verde, atrasada en rojo) y quién la asignó si no es propia.
  - Clic en una actividad: detalle con los entregables (casillas solo para el responsable), «Marcar como completa» si no
    tiene entregables, y «Editar» / «Anular» solo para quien puede.
- **Formulario de actividad:** qué hay que hacer, a quién (solo personas visibles; en la agenda propia viene fijo), fecha de
  entrega (propone el próximo día hábil; rechaza sábado y domingo) y entregables, uno por línea.

**«Áreas de la agenda» (`ParametrizacionAreasAgenda.aspx`)**

Protegida con `Permisos.PuedeEntrar`. Al desplegar: perfiles 14 (Talento Humano) y 18.
- **Áreas:** nombre, color con selector, activa sí/no y cantidad de personas. Crear, editar y desactivar. «Sin área» no
  se desactiva ni se renombra.
- **Personas:** usuarios activos con un combo para su área, y un filtro «Sin área».

Se usan jQuery y tablas HTML simples, como Tablero y Curva. **No se usa DataTables.**

### 5. Correos

- **Cuándo:** solo cuando alguien asigna una actividad a otra persona (`Cod_AsignadoPor ≠ Cod_Responsable`). Registrarse
  una actividad propia no envía nada. **No se avisa** al cambiar la fecha ni al anular.
- **Para:** `R_Usuarios.E_Mail` del responsable.
- **Asunto:** «Nueva actividad asignada: *título*».
- **Cuerpo:** quién la asignó, título, fecha de entrega, entregables y un botón «Ver en mi agenda» a `Agenda.aspx`. Mismo
  remitente y formato de marca que los demás correos del sistema.
- **Si falla** (o la persona no tiene correo): la actividad **se guarda igual**, el intento queda en `AG_CorreoBitacora` y
  quien asignó ve «Actividad asignada, pero no se pudo avisar por correo a *persona*».

### 6. Scripts SQL

En `docs/sql/`, re-ejecutables, cada uno termina en `VEREDICTO: OK`:

1. `2026-10-15-agenda-esquema.sql`: las tablas `AG_*`, los `Sp_RTA_Agenda*`, y la acción `Agenda.VerTodos` en `RTA_Accion`
   ligada al `Id_Menu` de la agenda.
2. `2026-10-15-agenda-precarga.sql`: un área por cada `Departamento` distinto entre los activos, con `ADMINISTRACION` y
   `ADMINISTRACIÓN` unificados y colores por defecto; la fila «Sin área»; y el área de cada persona. No pisa lo que ya
   esté cargado si se corre de nuevo.
3. `2026-10-15-agenda-menu.sql`: «Equipo y agenda» en `PerfilMenu` para todos los perfiles con usuarios internos activos;
   «Áreas de la agenda» para 14 y 18; `Agenda.VerTodos` para el perfil que resulte de la pregunta abierta 2. Trae la
   reversa comentada.
4. `2026-10-15-agenda-verificacion.sql`: todas las personas activas tienen área o caen en «Sin área»; hay una sola fila
   «Sin área»; no hay ciclos de jefes; el menú y la acción quedaron como se esperaba; y lista a las personas cuyo jefe
   no está entre los activos.

## Manejo de errores

- Sin sesión o sin permiso: el handler responde con estado `0` y un mensaje, y no ejecuta nada. Las pantallas redirigen
  a Principal.
- Validación en el servidor, aunque la pantalla ya valide: título obligatorio, fecha de lunes a viernes, responsable
  activo y visible.
- Si un cambio llega sobre una actividad anulada o de alguien que ya no es visible, se rechaza con un mensaje claro.
- Un fallo de correo nunca deshace el guardado (§5).

## Pruebas

**Unitarias** (`CapaPruebas`), una clase por regla:
- **Cadena:** reportes directos; varios niveles; alguien sin gente a cargo; un ciclo; un jefe fuera de la lista de
  activos.
- **Permisos de actividad:** ver, asignar, editar, anular y marcar para la propia persona, su jefe, el jefe de su jefe,
  alguien de otra rama y quien tiene `Agenda.VerTodos`; el responsable no mueve la fecha de lo asignado pero sí de lo
  propio; nada se edita si está anulada.
- **Estado:** completa con y sin entregables; atrasada (vence hoy: no; venció ayer: sí; completa y vencida: no; anulada:
  no).
- **Fechas:** sábado y domingo rechazados; próximo día hábil desde lunes, jueves, viernes, sábado y domingo.
- **Contadores** de persona y de área para una semana.
- **Correo:** el contenido incluye título, fecha, entregables y quién asignó; no se envía si es propia.

**En navegador, en local**, con tres roles: Gerencia, un jefe con cadena de dos niveles y un colaborador sin gente a cargo.

## Publicación y reversa

**Orden:** los 4 scripts SQL; luego, **juntos**, `ReporteTareas.dll` y los tres `.exe` de capa (`CapaEntidad`,
`CapaNegocio`, `CapaDato`), `Agenda.aspx`, `ParametrizacionAreasAgenda.aspx`, sus `.js` y `AdministrarAgenda.ashx`;
la verificación otra vez. Se documenta en una sección nueva de `DESPLIEGUE.md`.

**Prueba en producción:**
- **Gerencia:** ve todas las áreas, asigna a alguien y a esa persona le llega el correo.
- **Un jefe:** ve solo su cadena, aun buscando por nombre a alguien de otra área.
- **Un colaborador:** entra directo a su agenda, marca sus entregables, no puede mover la fecha de lo asignado y no abre
  «Áreas de la agenda».

**Reversa:** reponer los binarios anteriores, todos juntos, y aplicar la reversa del menú. Las tablas pueden quedar.

## Preguntas abiertas (para la revisión de este documento)

1. **¿Quién cuenta como activo?** El criterio de los selectores de jefe (`EstadoUsuario` NULL) da 105 personas y deja
   fuera a Isabel Lasso y a Luis Salazar, que son jefes de alguien. Sumar `EstadoUsuario = 0` da 224 y trae cuentas de
   prueba (`aranda`, «ADMINISTRADOR DE PRUEBAS») y un código repetido. **Recomendación:** usar el de los selectores
   (105) y que Talento Humano corrija el estado de esas dos jefaturas; mientras tanto, Nathaly Pinos y Verónica Viteri
   solo las ve Gerencia.
2. **¿A quién se le da `Agenda.VerTodos`?** Dárselo al perfil 10 «Gerencia» se lo da también a Rubén Suquillo y a Rafael
   Parreño. **Recomendación:** dárselo al perfil 10 si esas dos personas deben ver a toda la empresa; si no, solo al
   18 y pedir que Carlos Pinos pase a un perfil propio, o habilitar acciones por usuario (hoy la verificación de
   Permisos exige que no haya).

## Fuera de alcance

- **Entrega 2:** procesos, plantillas, plazos en días hábiles con feriados, y los correos «te toca un paso» y «el proceso
  terminó».
- **Reuniones de Teams/Outlook** (Microsoft Graph): fase aparte, cuando TI apruebe la aplicación en Azure AD.
- Recordatorios diarios y resumen semanal por correo.
- Aviso por correo al cambiar la fecha o anular.
- Cualquier relación con las tareas de servicio de Aranda.
