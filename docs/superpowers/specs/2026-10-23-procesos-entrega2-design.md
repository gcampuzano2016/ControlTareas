# Procesos entre áreas (Entrega 2 de «Equipo, Calendario y Procesos») — Diseño

**Fecha:** 2026-10-23
**Estado:** aprobado por el usuario el 2026-10-23
**Rama:** `ProyectoNuevosCambios`
**Antecedentes:**
- Entrega 1 (equipo y agenda): `docs/superpowers/specs/2026-10-15-agenda-equipo-entrega1-design.md`, en producción desde
  el 2026-10-22 (tablas `AG_*`, `Agenda.aspx`, `ParametrizacionAreasAgenda.aspx`).
- Maqueta: `equipo-completo-preview (6).html` (pestaña «Procesos», plantillas de ejemplo «Nuevo contrato con cliente»,
  «Contratación de personal», «Compra de equipos»).
- Decisiones de la conversación del 2026-10-15 sobre procesos, fechas y correos. El usuario pidió el 2026-10-23 pasar
  directo al diseño sin más preguntas: lo que no se acordó entonces está marcado como **decisión mía**.

## Objetivo

Que un trabajo que pasa por varias áreas (un contrato nuevo, una contratación, una compra) se lleve como un **proceso**:
una secuencia de pasos, cada uno con su responsable, que avanza sola. Al terminar un paso, el sistema activa el siguiente,
lo pone en la agenda de su responsable y le avisa por correo. Quien inició el proceso ve en todo momento en qué paso va y
quién lo tiene.

**Éxito:** Gerencia inicia un «Nuevo contrato con cliente», cada responsable recibe su paso solo cuando le toca, y nadie
tiene que preguntar por correo o por teléfono «¿en qué va?».

**Antiobjetivos** (aunque funcione, sería un fracaso):
- que a alguien le llegue un paso antes de que termine el anterior;
- que cambiar una plantilla altere un proceso que ya está en curso;
- que un proceso quede trabado sin que nadie pueda reasignarlo o cancelarlo desde el portal;
- que la agenda de la Entrega 1 cambie su comportamiento para las actividades que no son de procesos.

## Lo que se acordó (2026-10-15)

| Decisión | Valor |
|---|---|
| Orden | pasos **en orden**; al iniciar se ven todos, solo el primero queda **en curso** y solo se avisa a su responsable |
| Avance | al completarse un paso, se activa el siguiente y se avisa a su responsable |
| Paralelo | pasos en paralelo **quedan para después** |
| Plazos | cada paso de la plantilla tiene un plazo en **días hábiles** (salta sábados, domingos y la tabla `Feriado`) |
| Correos | solo por evento: **paso activado** (a su responsable) y **proceso terminado** (a quien lo inició) |
| Plantillas | **administrables desde el portal** |
| Agenda | cada paso en curso es una actividad de la agenda; `AG_Actividad` tiene la columna que apunta al paso |
| Pantallas | dentro de `Master.Master`, colores claros del portal, no el tema oscuro de la maqueta |
| Relación con Aranda | ninguna: no se tocan las tareas de servicio (`R_DetTareasAranda`) |

## Cómo funciona

1. Alguien con permiso administra **plantillas**: un nombre y una lista ordenada de pasos. Cada paso tiene el **área**
   responsable, un **título**, un **plazo en días hábiles** y, si quiere, **entregables** propuestos.
2. Cualquiera que vea la pantalla «Procesos» **inicia un proceso** desde una plantilla: le pone un nombre («Contrato —
   Hospital Metropolitano») y elige, para cada paso, la **persona responsable** dentro del área del paso. Puede ajustar el
   título y el plazo de cada paso antes de iniciar.
3. Al iniciar, los pasos se **copian** al proceso (la plantilla puede cambiar después sin afectarlo). El primer paso queda
   **En curso**: se crea su actividad en la agenda del responsable, con fecha de entrega = hoy + plazo en días hábiles, sus
   entregables, y el correo de asignación. Los demás quedan **Pendientes**, visibles en el proceso pero no en ninguna agenda.
4. El responsable trabaja el paso como cualquier actividad de la Entrega 1: marca sus entregables o «completa». Cuando la
   actividad queda **completa**, el paso pasa a **Completo** y se activa el siguiente (mismo tratamiento que el punto 3).
5. Cuando se completa el último paso, el proceso queda **Terminado** y se avisa a quien lo inició.

## Arquitectura

### 1. Datos

Tablas nuevas con el prefijo `AG_`, las columnas de auditoría habituales (`Usuario_Registro`, `Fecha_Registro`,
`Usuario_Modificacion`, `Fecha_Modificacion`) y **claves foráneas entre sí**, como la Entrega 1. Hacia `R_Usuarios` no hay
clave foránea (`Cod_Usuario` no es su clave primaria).

**`AG_Plantilla`**: `IdPlantilla` (identity, PK), `Nombre` (varchar(150), único), `Descripcion` (varchar(500) NULL),
`Activa` (bit; una plantilla no se borra, se desactiva).

**`AG_PlantillaPaso`**: `IdPlantillaPaso` (PK), `IdPlantilla` (FK), `Orden` (int, único por plantilla), `IdArea` (FK a
`AG_Area`), `Titulo` (varchar(200)), `PlazoDias` (int, 1 a 60 días hábiles), `Entregables` (varchar(2000) NULL, uno por
línea).

**`AG_Proceso`**: `IdProceso` (PK), `Nombre` (varchar(200)), `IdPlantilla` (FK; solo para saber de dónde salió),
`Cod_IniciadoPor` (varchar(20)), `Estado` (`EnCurso` / `Terminado` / `Cancelado`), `FechaInicio`, `FechaFin` NULL,
`MotivoCancelacion` varchar(400) NULL.

**`AG_ProcesoPaso`**: la **copia** de cada paso al iniciar. `IdProcesoPaso` (PK), `IdProceso` (FK), `Orden`, `IdArea`
(FK), `Titulo`, `PlazoDias`, `Entregables`, `Cod_Responsable`, `Estado` (`Pendiente` / `EnCurso` / `Completo` /
`Cancelado`), `FechaActivacion` NULL, `FechaCompletado` NULL. La actividad de un paso se encuentra por
`AG_Actividad.IdProcesoPaso` (una sola dirección de clave foránea, sin ciclo entre las dos tablas; el procedimiento de
activación garantiza a lo sumo una actividad por paso).

**Cambios a tablas de la Entrega 1** (son tablas propias del módulo; ninguna tabla fuera de `AG_*` se toca):
- `AG_Actividad.IdProcesoPaso` int NULL, FK a `AG_ProcesoPaso`. NULL en toda actividad que no es de un proceso.
- `AG_CorreoBitacora`: `IdActividad` pasa a NULL y se agrega `IdProceso` int NULL (FK), para registrar el correo de
  «proceso terminado», que no tiene actividad. Un `CHECK` exige que al menos uno de los dos tenga valor.

### 2. Reglas (clases puras en `CapaNegocio`, con pruebas en `CapaPruebas`)

**`NegAgendaDiasHabiles`**
- `SumarDiasHabiles(DateTime desde, int dias, ISet<DateTime> feriados) : DateTime` — cuenta a partir del día siguiente a
  `desde`, salta sábados, domingos y feriados (`Feriado.Activo = 1`). Un plazo de 1 que arranca un viernes vence el lunes
  (o el martes si el lunes es feriado).
- La fecha de entrega que resulta nunca es sábado ni domingo, como exige la Entrega 1.

**`NegAgendaProceso`**
- **Activación:** activar un paso = estado `EnCurso`, `FechaActivacion` = ahora, crear su actividad (título del paso,
  responsable, `Cod_AsignadoPor` = quien inició el proceso, fecha de entrega con `SumarDiasHabiles`, entregables del paso,
  `IdProcesoPaso`) y avisar al responsable.
- **Avance:** cuando la actividad de un paso `EnCurso` queda completa (regla «Completa» de la Entrega 1), el paso pasa a
  `Completo` con `FechaCompletado` y se activa el siguiente por `Orden`. Sin siguiente: el proceso queda `Terminado` y se
  avisa a quien lo inició. El avance ocurre **en la misma transacción** en que se marca el último entregable o «completa»:
  dos marcas simultáneas no pueden activar dos veces el mismo paso.
- **Un paso completo no se reabre.** Su actividad queda de solo lectura: no se desmarcan entregables ni «completa».
  **(Decisión mía.)** Reabrir obligaría a retroceder un paso que ya se le avisó a otra persona.
- **La actividad de un paso no se anula ni se le cambia la fecha desde la agenda** con las reglas de la Entrega 1: se
  maneja desde el proceso (reasignar, cambiar fecha, cancelar). **(Decisión mía.)**
- **Reasignar un paso** `Pendiente` o `EnCurso` a otra persona del área: si está en curso, la actividad pasa al nuevo
  responsable (se conserva el avance de sus entregables), con correo de asignación al nuevo. **(Decisión mía.)**
- **Cambiar la fecha de entrega** del paso en curso: permitido, nunca sábado ni domingo. **(Decisión mía.)**
- **Cancelar el proceso** (con motivo): el proceso queda `Cancelado`, los pasos no completos quedan `Cancelado` y la
  actividad del paso en curso se anula. Sin correo. **(Decisión mía.)**
- **Quién hace qué:**

  | Acción | Quién puede |
  |---|---|
  | Ver la lista de procesos y su avance | quien lo inició, cualquier responsable de alguno de sus pasos, los jefes de la cadena de esas personas (regla `Cadena` de la Entrega 1) y quien tiene `Agenda.VerTodos` |
  | Iniciar un proceso | cualquiera que entre a «Procesos» **(decisión mía)** |
  | Elegir responsables al iniciar | cualquier persona activa del área de cada paso, sin la restricción de la cadena: un proceso cruza áreas por definición **(decisión mía)** |
  | Reasignar un paso, cambiar su fecha, cancelar el proceso | quien lo inició y quien tiene `Agenda.VerTodos` **(decisión mía)** |
  | Trabajar un paso (entregables, completa) | solo su responsable, como en la Entrega 1 |
  | Administrar plantillas | quien entra a «Plantillas de procesos» (menú: perfiles **10 y 18**) **(decisión mía)** |

- **Plantillas:** al menos un paso; `Orden` consecutivo desde 1; área activa; plazo de 1 a 60. Una plantilla con
  procesos no se borra, se desactiva. Editar una plantilla no toca los procesos ya iniciados (tienen su copia).
- **Área sin personas activas:** no se puede iniciar un proceso si algún paso no tiene a quién asignar; el mensaje dice qué
  área.

**`NegAgendaCorreo`** (se amplía): asunto y cuerpo de «Te toca un paso: *título* — *proceso*» (con la fecha de entrega y
el enlace a la agenda) y «Proceso terminado: *proceso*» (con la lista de pasos, responsables y fechas). Todo texto
escapado, como la Entrega 1. Un fallo de correo nunca deshace el guardado y queda en `AG_CorreoBitacora`.

### 3. Capas

Mismo patrón que la Entrega 1:
- `CapaEntidad`: `EntAgendaPlantilla`, `EntAgendaPlantillaPaso`, `EntAgendaProceso`, `EntAgendaProcesoPaso`; `EntAgendaActividad`
  gana `IdProcesoPaso` y `NombreProceso` (para mostrarlo en la agenda).
- `CapaDato`: `DaoAgenda` se amplía con los procedimientos `Sp_RTA_AgendaPlantilla*` y `Sp_RTA_AgendaProceso*`. La
  activación y el avance viven en el procedimiento que marca entregables / completa (una sola transacción).
- `CapaNegocio`: las reglas de arriba; `NegAgenda.Datos.cs` sigue siendo el único archivo de la agenda que llama a
  `CapaDato`.
- `ReporteTareas`: `AdministrarAgenda.ashx` se amplía con las acciones de procesos y plantillas; `Permisos.cs` gana las
  constantes de las dos pantallas nuevas.

### 4. Pantallas

1. **`Procesos.aspx` — «Procesos»** (grupo de menú «Equipo y agenda»). Lista de procesos visibles, filtrable por estado
   (en curso / terminados / cancelados) y por área. Cada proceso, como en la maqueta: nombre, plantilla, «n/m pasos
   completos», estado general y la **línea de pasos** (completo en verde, en curso resaltado, pendiente en gris, cancelado
   tachado), cada paso con su área (color del catálogo), responsable y fecha de entrega; un paso atrasado en rojo.
   Botón **«Iniciar un proceso»**: elegir plantilla, nombre, y por cada paso responsable (lista de personas activas del
   área), título y plazo editables. Acciones según permiso: reasignar, cambiar fecha, cancelar con motivo.
2. **`ParametrizacionPlantillasProcesos.aspx` — «Plantillas de procesos»** (mismo grupo; perfiles 10 y 18): alta y edición
   de plantillas con sus pasos (agregar, quitar, subir/bajar, área, título, plazo, entregables), activar/desactivar.
3. **`Agenda.aspx`** (Entrega 1): la actividad de un paso muestra una etiqueta con el nombre del proceso y un enlace a
   «Procesos»; los controles que el punto 2 de las reglas prohíbe (anular, cambiar fecha, desmarcar un paso completo) no se
   muestran para esas actividades. El resto de la agenda no cambia.

Acceso con `Permisos.PuedeEntrar`, y el handler valida cada acción en el servidor.

### 5. Plantillas iniciales

No se precarga ninguna. Las tres de la maqueta («Nuevo contrato con cliente», «Contratación de personal», «Compra de
equipos») se documentan como ejemplo en `DESPLIEGUE.md` para que Gerencia las cree desde la pantalla si le sirven.
**(Decisión mía.)** Los pasos y áreas de la maqueta son ilustrativos y no coinciden con las 9 áreas reales del catálogo.

### 6. Scripts SQL

En `docs/sql/`, re-ejecutables, terminan en `VEREDICTO: OK`, sin tildes en el texto (`NCHAR`): esquema (tablas, columnas
nuevas en `AG_Actividad` y `AG_CorreoBitacora`, procedimientos nuevos y los de la Entrega 1 que cambian), menú (dos hojas
en el grupo «Equipo y agenda», con reversa comentada; se corre al desplegar) y verificación. La base local es la de
producción: cada script que escribe se corre con autorización explícita del usuario.

**Cuidado con la Entrega 1 en producción:** los procedimientos que marcan entregables, «completa» y que guardan una
actividad ya están en uso (hay actividades reales). Sus versiones nuevas tienen que comportarse **idéntico** para las
actividades sin `IdProcesoPaso`, y el script lleva el guard de versión (`SET NOEXEC`) para no deshacer cambios si se vuelve
a correr uno viejo, y la versión anterior de cada procedimiento comentada para la reversa.

## Resuelto en la revisión de constructor (2026-10-23, decisiones mías)

1. **La actividad de un paso no se edita desde la agenda.** Título y entregables vienen del paso (plantilla, ajustable al
   iniciar) y quedan fijos al activarse. `Sp_RTA_AgendaGuardarActividad` rechaza editar una actividad con `IdProcesoPaso`;
   así quitar o borrar entregables nunca completa un paso por la puerta de atrás. Los pasos `Pendiente` sí se pueden
   ajustar desde «Procesos» (título, plazo, entregables) por quien inició el proceso o tiene `Agenda.VerTodos`.
2. **El bloqueo vive también en los procedimientos**, no solo en la pantalla y el handler: marcar o desmarcar entregables
   o «completa» en la actividad de un paso `Completo` o `Cancelado` se rechaza en el SQL.
3. **Completar un paso pide confirmación** en la agenda: «Al completar este paso se le avisará a *persona* para el paso
   siguiente. No se puede deshacer.» (o «se terminará el proceso» si es el último). Vale para el último entregable y para
   «Marcar como completa».
4. **Acciones según el estado del paso:**
   - Reasignar un paso `Pendiente` solo cambia su `Cod_Responsable` (no hay actividad ni correo todavía).
   - En un paso `Pendiente` se cambia el **plazo**; en uno `EnCurso`, la **fecha de entrega**. Los `Completo` y
     `Cancelado` no se tocan.
   - Cancelar el proceso anula la actividad del paso en curso; sus entregables marcados quedan como historia. Una
     actividad anulada no cuenta como atrasada ni en los contadores (regla de la Entrega 1).
5. **Reasignar un paso en curso** conserva `Cod_AsignadoPor` = quien inició el proceso; quién reasignó queda en
   `Usuario_Modificacion` y en la bitácora. **Correo:** se manda «Te toca un paso» al activar o reasignar, salvo que el
   responsable sea el mismo que inició el proceso (misma regla que la Entrega 1 para las actividades propias).
6. **Responsable que deja de estar activo:** el paso muestra en «Procesos» la marca roja «Responsable inactivo — reasignar»
   para quien puede reasignar. La activación no se detiene: el paso se activa igual y queda a la vista para reasignarlo.
7. **La fecha de entrega se calcula desde la activación real de cada paso** (fecha de activación + plazo en días hábiles),
   no desde el inicio del proceso: un atraso en un paso no acorta el plazo del siguiente.
8. **Visibilidad igual en todos los estados** (en curso, terminado, cancelado) y sin límite de tiempo; el filtro por
   defecto de «Procesos» es «En curso».
9. **Las actividades de pasos cuentan en las tarjetas y contadores** de la agenda igual que cualquier otra actividad; la
   consulta de la Entrega 1 solo agrega el nombre del proceso para la etiqueta.

## Manejo de errores

- Sin sesión o sin permiso: estado `0` con mensaje; las pantallas redirigen a Principal.
- Cambios sobre un paso o proceso que cambió mientras tanto (otro lo reasignó, lo completó o lo canceló): se rechazan con
  «El proceso cambió; recargue la pantalla».
- JSON con `ContentEncoding` UTF-8 explícito; `.aspx` y `.js` con BOM.

## Pruebas

**Unitarias:** días hábiles (fin de semana, feriado, feriado inactivo, cruce de mes y de año, plazo 1 un viernes);
activación y avance (último paso → terminado; un paso completo no vuelve atrás; doble marca no activa dos veces);
permisos de la tabla de arriba; validación de plantillas; reasignación conservando entregables; cancelación; correos
(asuntos, escape, destinatarios).

**En navegador, en local:** plantilla de 3 pasos en áreas distintas; iniciar; el primero aparece en la agenda de su
responsable y los otros no; completar → el segundo aparece y llega su correo; reasignar el segundo; completar el último →
correo a quien inició; cancelar otro proceso a mitad.

## Publicación y reversa

Orden: esquema → binarios juntos (`ReporteTareas.dll` y los tres `.exe`), las dos `.aspx` nuevas, sus `.js`,
`Agenda.aspx` y `agenda.js` actualizados → menú → verificación. Sección nueva en `DESPLIEGUE.md`. Reversa: binarios
anteriores juntos, reversa del menú y del procedimiento de marcado (el script guarda la versión anterior comentada).

## Fuera de alcance

- Pasos en paralelo o con condiciones («si requiere contratación…»).
- Reuniones de Teams/Outlook (Microsoft Graph), recordatorios diarios y resumen semanal por correo.
- Aviso por correo al reasignar a la persona que pierde el paso, al cambiar fechas o al cancelar.
- Que un proceso se inicie solo (por ejemplo, desde un contrato registrado en el sistema).
- Cualquier relación con las tareas de servicio de Aranda.
