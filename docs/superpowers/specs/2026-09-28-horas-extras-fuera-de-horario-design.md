# Horas Extras: tiempo registrado fuera de horario — Diseño

**Fecha:** 2026-09-28
**Estado:** aprobado en conversación, pendiente de revisión escrita y de plan de implementación
**Rama:** `ProyectoNuevosCambios`
**Módulo que se extiende:** Horas Extras (`2026-09-15-horas-extras-design.md`, fases 1 a 5 ya en producción)

> **Sin datos personales.** Este diseño describe reglas y estructura. El diagnóstico que lo motivó tiene nombres, horas y sueldos reales, y está en `Actualizacion/` (ignorado por git). El repositorio es público, así que aquí las personas aparecen con una letra.

---

## Por qué

Al validar el período 16-08 / 15-09-2026, el reporte que Nómina armó a mano no cuadra con el sistema. El diagnóstico (`Actualizacion/diagnostico-he-16ago-15sep-2026.sql`, ejecutado el 2026-09-28) comprobó dos cosas:

1. **El sistema cuadra consigo mismo.** Para las 14 personas con diferencia, lo que `Sp_RTA_HeHorasAprobadas` sembraría hoy es igual a lo congelado en `HE_Detalle`. Reabrir el período y volver a sembrar no cambia nada.
2. **La mayor parte de la diferencia es tiempo registrado fuera del horario que nunca llegó al flujo de aprobación.** El caso tipo es la persona A: tiene horario de 08:30 a 17:30 y el 18-08 registró de 08:00 a 09:00 y de 15:00 a 18:00 como trabajo normal. Queda 1 hora fuera de horario que nunca solicitó. El reporte manual la paga; el sistema no la ve.

Desde el 26-08, el procedimiento de registro ya parte el tramo según el horario, con `FN_RTA_ClasificarTramosHorario`. Pero hay dos límites:
- Lo registrado **antes del 26-08**, o entre el 01 y el 07-09 (cuando la clasificación estuvo apagada), quedó sin partir.
- Aun partido, el pedazo de hora extra nace **«sin solicitar»**. Si la persona no lo solicita y la jefatura no lo aprueba, no se paga.

**El objetivo no es pagar ese tiempo solo.** Se decidió mostrarlo, sin pagarlo automáticamente (opción B de la conversación). Nómina tiene que **verlo antes de cerrar** y decidir qué hacer: pedir que se solicite y apruebe, o capturarlo a mano (la captura ya existe y queda auditada). La aprobación de la jefatura sigue siendo el control.

## Decisiones

| # | Decisión | Descartado y por qué |
|---|---|---|
| 1 | El tiempo fuera de horario **no se paga automáticamente**: es información, no un monto. | Pagarlo solo dejaba la aprobación de la jefatura sin efecto. Crear solicitudes automáticas tocaba datos de muchas personas de golpe. |
| 2 | Dos cifras separadas: **sin aprobar** y **rechazado**. | Juntarlas mezcla dos cosas que se resuelven distinto: una todavía se puede regularizar, la otra la jefatura ya la decidió. |
| 3 | El detalle tramo por tramo se ve **en la pantalla y en el Excel**. | Una cifra sola dice cuánto, pero no dónde está el tiempo, y sin eso nadie puede pedir que se regularice. |
| 4 | Al cerrar, el sistema **avisa y deja cerrar**. | Bloquear el cierre hacía que un solo tramo que nadie va a aprobar dejara el período sin poder cerrarse. |
| 5 | **Enfoque 1:** el SQL entrega los segmentos y una clase pura de `CapaNegocio` hace las cuentas. | Todo en SQL: las restas de intervalos no se pueden probar sin la base. Guardarlo en `HE_Detalle`: exige un `ALTER` y la cifra queda vieja apenas alguien solicita una hora. |
| 6 | Se calcula **cada vez que se abre la pantalla**, también en períodos cerrados. | Congelarlo al cerrar ocultaba justo lo que interesa: si alguien regularizó algo después. |

**Nada de esto toca tablas ni cambia un monto.** `HE_Detalle`, sus totales y la hoja «Pagos» siguen exactamente igual.

---

## Parte 1: los datos y la regla de cálculo

### Qué tramos entran

Todos los tramos de `R_DetTareasAranda` cuyo `Det_Fch_RegDetalleIni` cae en `[FechaInicio, FechaFin]` del período, con `Det_Fch_RegDetalleFin` no nulo. Las personas se identifican con **el mismo cruce que `Sp_RTA_HeHorasAprobadas`**:

```
R_DetTareasAranda.Id_Responsable
  -> R_Usuarios.Cod_Usuario -> R_Usuarios.Cedula
  -> Empleados.Cedula -> Empleados.IdEmpleado
  -> HE_ColaboradorParametro.IdEmpleado  (Estado = '1')
```

El cruce va en ese sentido y no al revés: invertido, una cédula con seis cuentas suma seis veces. Usar el mismo cruce garantiza que las columnas nuevas y las horas pagadas hablen exactamente de las mismas personas.

### Qué es «fuera de horario»

Cada tramo pasa por `FN_RTA_ClasificarTramosHorario(Id_Responsable, Inicio, Fin)`, que usa el horario asignado a la persona **en la fecha del tramo** (o el predeterminado si no tiene uno). De lo que devuelve, se quedan solo los segmentos con `TipoHora` 1 (50%) o 2 (100%); los de tipo 0 son horas normales y se descartan.

La regla es la misma que aplica el registro:
- 100% antes de las 06:00, en día no laborable o en feriado;
- 50% fuera de la franja de oficina;
- el resto, normal.

Por eso la detección no puede contradecir al registro.

### En qué columna cae cada segmento

Depende del estado del tramo del que sale (`Det_Horas_Extras_Estado`, catálogo 4):

| Estado del tramo | Categoría |
|---|---|
| 2 Aprobado **y** `Det_Horas_Extras_Tipo` en (1, 2) | **Pagado**: no suma; sirve para descontar |
| 0 Sin solicitar, 1 Solicitado, o 2 Aprobado con tipo 0 | **Sin aprobar** |
| 3 Rechazado | **Rechazado** |

Para descontar, se usa el **intervalo completo** del tramo pagado, no solo su parte fuera de horario: si un minuto ya está pagado, no vuelve a aparecer en ninguna otra columna.

### Cómo se evita contar dos veces (clase de `CapaNegocio`, por persona)

1. Se arma la unión de los intervalos **pagados**.
2. A los segmentos **sin aprobar** se les resta esa unión, y lo que queda se une entre sí: si dos tramos cubren el mismo minuto, cuenta una vez.
3. A los segmentos **rechazados** se les resta lo pagado y lo sin aprobar, y lo que queda se une entre sí.
4. En cada paso se conserva el tipo (50% / 100%) del segmento. Si en un mismo minuto coinciden un 50% y un 100% de la misma categoría, **gana el 100%**, que es lo que devolvería la función sobre ese minuto.
5. Los totales se suman en **minutos enteros**. Las horas con dos decimales se calculan solo al mostrar.

Prioridad resultante para un minuto disputado: **pagado > sin aprobar > rechazado**.

### Qué queda fuera, a propósito

- **Los tramos que cruzan la medianoche o que caen en un día sin configuración de horario.** La función no devuelve nada para ellos y el registro ya los rechaza (código -7 / -8). No se inventa una clasificación.
- **Los tramos pagados cuyo tipo registrado no coincide con el que daría el horario** (por ejemplo, un sábado aprobado al 50%). Es otra clase de diferencia —el monto pagado está mal, no falta tiempo— y se trata aparte.
- **Estimar el monto en USD.** Las columnas muestran horas. Convertirlas a dinero sugiere que es algo a pagar, y la decisión 1 es justamente que no lo es.

### Componentes

- **`Sp_RTA_HeFueraDeHorario(@FechaInicio DATE, @FechaFin DATE)`**, nuevo y de solo lectura. Devuelve **dos conjuntos de resultados**:
  1. **Segmentos:** `IdEmpleado`, `Id_RegDetTareas`, inicio y fin del tramo registrado, inicio y fin del segmento fuera de horario, `TipoHora`, estado del tramo, tipo registrado y la actividad (`Det_Det_Tarea`, recortada).
  2. **Pagados:** `IdEmpleado` e intervalo de cada tramo aprobado con tipo 1 o 2 del rango.
- **Entidades** en `CapaEntidad`: el segmento, el resumen por persona (minutos sin aprobar 50/100, rechazados 50/100) y la fila de detalle.
- **`DaoHorasExtras.LeerFueraDeHorario(inicio, fin)`** en `CapaDato`: lee los dos conjuntos. No transforma nada: la dirección de las capas no permite que `CapaDato` llame a `CapaNegocio`.
- **`NegHeFueraDeHorario`** en `CapaNegocio`: una clase **pura**, sin base de datos ni pantalla, con la aritmética de intervalos de esta sección. Recibe listas y devuelve el resumen por persona y las filas de detalle que suman.
- **Versionado de la función.** `FN_RTA_ClasificarTramosHorario` hoy solo existe en la base. Se guarda **tal como está** en `docs/sql/referencia/FN_RTA_ClasificarTramosHorario.sql`, marcada como referencia y **no ejecutable**: el registro de actividades la usa todos los días, y volver a crearla desde un archivo es la forma más fácil de cambiarla sin querer.

---

## Parte 2: la pantalla (`Formulario/HorasExtras.aspx`)

**Columnas nuevas.** «Fuera de horario sin aprobar (h)» y «Rechazado (h)», justo después de «Total HE (USD)».
- Con 0 muestran «—». Con horas, la cifra es un **enlace que abre el detalle**.
- Son de solo lectura y no participan en ningún cálculo de la fila.

**Carga aparte.** Una acción nueva del handler, `CargarFueraDeHorario(idPeriodo)`, se pide **después** de dibujar la grilla:
- La función de horarios se ejecuta una vez por tramo. La grilla no espera un dato informativo.
- Si la acción falla, las dos columnas muestran «no disponible» y el resto de la pantalla funciona igual que hoy.
- La acción fija `ContentEncoding` UTF-8, como las demás del handler: la actividad lleva tildes.

**Ventana de detalle.** Por persona y de solo lectura, con estas columnas: fecha, tramo registrado, fuera de horario (desde-hasta), minutos, tipo (50% / 100%), estado (Sin solicitar / Solicitado / Aprobado sin tipo / Rechazado) y actividad. Al pie van los totales por tipo y por categoría.
- Muestra solo lo que efectivamente suma. Lo descontado por superposición no aparece.
- Va directamente en `HorasExtras.aspx`, junto a las ventanas que ya tiene la página.

**Al cerrar.** `ConfirmarCerrarPeriodo` agrega una línea a la confirmación existente, calculada con lo que ya cargó la grilla:
- «Hay N colaboradores con X h fuera de horario sin aprobar y M con Y h rechazadas.»
- Si los datos no llegaron: «No se pudo verificar el tiempo fuera de horario.»
- **El cierre no se bloquea en ningún caso.** El procedimiento de cierre no cambia.

**Quién lo ve.** Los mismos perfiles que ya entran a la pantalla (14 y 18), en períodos abiertos y cerrados. En un período cerrado, la cifra refleja el estado **actual** de los tramos.

---

## Parte 3: el Excel (`NegHeExportacion`)

- **Hoja «Horas Extras»:** se agregan al final las dos columnas nuevas, con su total en la fila de TOTALES.
- **Hoja «Pagos»: no cambia.** Es lo que se paga. Si se le agregaran columnas, alguien podría sumarlas por error al rol.
- **Hoja nueva, «Fuera de horario»:** una fila por cada segmento que suma, de todas las personas.
  - Columnas: ID Empleado, Nombre, Fecha, Tramo registrado, Fuera de horario, Minutos, Horas, Tipo, Estado, Actividad, Id tramo (`Id_RegDetTareas`).
  - Va ordenada por nombre y fecha, con los totales por tipo y por categoría al final.
  - El encabezado advierte: «Información para revisión: estas horas NO están incluidas en los totales a pagar».
- **Si el cálculo falla al descargar**, el Excel se entrega igual:
  - «Horas Extras» y «Pagos» salen completas.
  - Las dos columnas nuevas quedan vacías.
  - La hoja «Fuera de horario» trae una sola línea: «No se pudo calcular el tiempo fuera de horario: …».

  El problema se ve en el mismo archivo, y la descarga de lo que se paga no queda bloqueada por un dato informativo.
- **Auditoría:** no cambia. La descarga ya se registra en `HE_PeriodoAuditoria` antes de armar el archivo.

---

## Parte 4: pruebas, validación y despliegue

### Pruebas unitarias (`CapaPruebas/NegHeFueraDeHorarioTests.cs`, datos ficticios)

- Un segmento sin superposición suma completo.
- Un tramo pagado descuenta la parte que se le superpone, incluido el caso de un tramo pagado **dentro** de uno normal más largo.
- Dos segmentos sin aprobar sobre el mismo minuto lo cuentan una vez.
- Un minuto que está a la vez sin aprobar y rechazado cuenta solo como sin aprobar.
- Un minuto pagado no aparece en ninguna categoría.
- En un minuto disputado entre 50% y 100% de la misma categoría gana el 100%.
- Los totales van en minutos, y el redondeo a horas se aplica una sola vez.
- Una persona sin segmentos da cero y no aparece en el detalle.
- Las filas de detalle contienen solo lo que suma, ordenadas por fecha.

Además, las pruebas existentes de `CapaPruebas` tienen que seguir pasando.

### Validación contra producción: el criterio de terminado

Con el período 16-08 / 15-09-2026, las columnas nuevas tienen que **explicar las diferencias que ya encontró el diagnóstico**. La tabla con nombres está en `Actualizacion/diagnostico-resultado.txt`. En resumen:

| Caso | Se espera |
|---|---|
| Persona A (el caso tipo) | 1,00 h sin aprobar (dos segmentos de 30 min del 18-08) |
| Personas B, C, D, E | 1,50 / 2,75 / 2,00 / ~1,02-1,03 h sin aprobar (la última depende de un minuto en el borde de las 17:30) |
| Persona F | 6,20 h rechazadas |
| Persona G | 10,75 h rechazadas, más lo solicitado del 14 y 15-09 como sin aprobar |
| Persona H | **0** el 14-09: su tramo pagado de 08:00 a 08:30 está dentro de uno normal de 08:00 a 17:30 y no debe contarse dos veces |

También se mide cuánto tarda `Sp_RTA_HeFueraDeHorario` sobre el período. Si pasa de unos pocos segundos, se revisa antes de publicar.

### SQL: `docs/sql/2026-09-28-horas-extras-fuera-de-horario.sql`

- Crea `Sp_RTA_HeFueraDeHorario`. Es de solo lectura y nadie lo llama hasta que lleguen los binarios, así que **se puede ejecutar antes que ellos**.
- Trae el bloque de verificación con el contador `@Fallos`, sin `GO` entre la declaración y su último uso: el procedimiento existe, devuelve los dos conjuntos y, para el período 3, entrega los 60 minutos de la persona A.
- Si una fase posterior recrea este procedimiento con otra firma, este script tiene que llevar el guard `SET NOEXEC` que usan las fases 2, 4 y 5.

### Binarios

Van **todas las capas juntas**: `CapaEntidad`, `CapaDato` y `CapaNegocio` (.exe) más `ReporteTareas.dll`, `Formulario/HorasExtras.aspx` y `js/horasExtras.js`. El 18-09 se publicó el .dll sin los .exe de capa y se dejó de enviar todo correo.

- `horasExtras.js?v=7` pasa a `?v=8`, o el navegador seguirá sirviendo el viejo.
- `HorasExtras.aspx` se guarda **con BOM**: `Web.config` declara windows-1252.
- Se compila con el MSBuild de VS2019, no con el del `PATH`.

### Orden y vuelta atrás

1. El script SQL.
2. Los binarios.
3. Se actualiza `DESPLIEGUE.md`.

**Para volver atrás** basta con reponer los binarios anteriores. El procedimiento nuevo puede quedar en la base sin hacer daño.
