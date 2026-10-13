# Horas Extras: decisión por tramo y pago por minutos — Diseño

**Fecha:** 2026-09-29
**Estado:** diseño aprobado en conversación; esta especificación espera la revisión del usuario
**Rama:** `ProyectoNuevosCambios`
**Extiende:**
- `2026-09-28-horas-extras-fuera-de-horario-design.md`
- `docs/superpowers/plans/2026-09-28-horas-extras-fuera-del-modulo.md`

> **Sin datos personales.** Las personas del análisis aparecen con una letra. Los nombres, horas y sueldos reales están en `Actualizacion/`, que git ignora. El repositorio es público.

---

## Por qué

Nómina necesita que el sistema dé **exactamente** el mismo resultado que el análisis manual del período 16-08 / 15-09-2026, que suma USD 6.241,14. El diagnóstico (`Actualizacion/conciliacion-he-16ago-15sep.py`) mostró que ese análisis **no sigue una regla fija**: el mismo tipo de tiempo se pagó a unas personas y a otras no.

| Tipo de tiempo | Pagado a | No pagado a |
|---|---|---|
| Fuera de horario, nunca solicitado | 5 personas | 4 personas (19,5 h, 6 h, 4,5 h, 3,25 h) |
| Rechazado por la jefatura | 1 persona (6,2 h) | 2 personas |
| Aprobado por la jefatura | casi todos | 1 persona, 2 tramos |
| Fuera del período | 1 persona (el 16-09) | — |

Además:
- Una persona tiene un tramo aprobado al 50% que el horario marca al 100%, y el análisis lo paga al 100%.
- Otra tiene 64 minutos aprobados al 100% que el horario marca como jornada normal, y el análisis los paga al 50%.
- El análisis calcula con **minutos exactos**; el sistema redondea las horas a 2 decimales antes de multiplicar. De ahí salen diferencias de ±0,05 USD en 15 personas.

**Ninguna regla automática reproduce eso.** Lo que sí lo reproduce es registrar en el sistema **las decisiones de Nómina, tramo por tramo**. Esta especificación hace eso, y deja cada decisión con su rastro.

## Decisiones (aprobadas una por una en conversación)

| # | Decisión | Descartado y por qué |
|---|---|---|
| 1 | Marcan **Talento Humano (14) y Super Admin (18)**. El perfil 2 ve todo sin poder cambiar nada. | Todos (2, 14, 18): demasiada gente puede cambiar el pago. Solo el 18: Talento Humano dependería de otro perfil. |
| 2 | Se pueden incluir tramos de **hasta 7 días antes o después** del período, con protección contra el pago doble. | Solo dentro del período: el caso del 16-09 no cuadraría. |
| 3 | Nómina puede **cambiar el tipo (50% / 100%)** de cada tramo, y queda registrado. | Siempre el tipo del horario: el caso de los 64 minutos no se pagaría. |
| 4 | El pago se calcula con **minutos exactos**: se redondea solo el dinero. **El pago real cambia unos centavos.** | Horas a 2 decimales: quedan los centavos de diferencia. |
| 5 | Las horas de la grilla **salen solo de los tramos**; la captura manual de horas se retira. | Mantener las dos: una hora tecleada no tiene tramo que la explique. |
| 6 | **Agregado en la verificación, pendiente de confirmar:** se pueden **reducir** los minutos incluidos de un tramo, con motivo obligatorio. | Solo tramos completos: dos personas del análisis no se pueden reproducir. Una necesita 156 minutos y sus tramos permiten 154, 155, 164 o 165. La otra necesita 1.280, y sus tramos permiten 1.275, 1.289 o más. |

**Regla que ya existía y se mantiene:** un período **cerrado** no admite decisiones. Para corregirlo se reabre (perfil 18).

---

## Parte 1: datos y cálculo

### Tabla nueva `HE_TramoDecision` (solo INSERT)

| Columna | Tipo | Qué es |
|---|---|---|
| `IdDecision` | INT IDENTITY, clave primaria | Orden de las decisiones: **manda la más alta** de cada clave |
| `IdPeriodo` | INT | El período en el que se decide |
| `Id_RegDetTareas` | BIGINT | El tramo |
| `SegmentoInicio` | DATETIME | Qué parte del tramo: su inicio, o el inicio del segmento fuera de horario |
| `Incluir` | BIT | 1 se paga, 0 no |
| `TipoHora` | INT | 1 = 50%, 2 = 100% |
| `MinutosIncluidos` | INT NULL | NULL = todo el segmento; si no, **menos o igual** que los minutos del segmento |
| `Motivo` | VARCHAR(400) NULL | Obligatorio si se reducen minutos o se incluye algo no aprobado |
| `Usuario`, `Fecha`, `Ip` | — | Quién, cuándo y desde dónde |

Nunca se actualiza ni se borra: cambiar una decisión es insertar otra. Índice por `(IdPeriodo, Id_RegDetTareas, SegmentoInicio, IdDecision DESC)`.

### Columnas nuevas en `HE_Detalle`

- `Minutos50 INT NULL` y `Minutos100 INT NULL`: la base del pago.
- `Horas50` y `Horas100` se siguen guardando, redondeadas a 2 decimales, **solo para mostrar**.
- Una fila con minutos en NULL es de antes del cambio y se recalcula al abrir o recalcular.

### Qué es un «tramo candidato»

Para cada persona del maestro activo, con el mismo cruce de identidad de siempre:

- **Tramos aprobados** (estado 2 y tipo 1 o 2): el tramo **completo**, un solo segmento, con el tipo que aprobó la jefatura.
- **Tramos sin solicitar, pendientes, rechazados o aprobados sin tipo:** **solo los segmentos fuera de horario** que da `FN_RTA_ClasificarTramosHorario`, con su tipo.
- **Rango de fechas:** `Det_Fch_RegDetalleIni` entre `FechaInicio − 7 días` y `FechaFin + 7 días`. Los que caen fuera de `[FechaInicio, FechaFin]` se marcan «fuera del período».

### Valor por omisión (sin decisión)

- **Aprobado dentro del período:** incluido, con el tipo aprobado.
- **Todo lo demás, aprobado o no, fuera del período, y todo lo no aprobado:** excluido, con el tipo que da el horario o el que tenía.

Por eso el pago de hoy **solo cambia en centavos** (decisión 4) hasta que Nómina marque algo.

### Período «dueño» y protección contra el pago doble

- **El dueño** de un tramo es el período cuyo `[FechaInicio, FechaFin]` contiene su fecha. Puede no haber ninguno: hoy el 14 y el 15-08 no pertenecen a ningún período.
- **Un segmento se considera incluido en un período** si hay una decisión `Incluir = 1` en ese período, o si ese período es el dueño y el valor por omisión lo incluye y nadie decidió otra cosa.
- **Se rechaza incluir** un segmento que ya está incluido en **otro** período. La pantalla lo muestra bloqueado, con la leyenda «incluido en el período dd/mm – dd/mm».
- **La regla se valida en el procedimiento que guarda**, dentro de una transacción con `UPDLOCK, HOLDLOCK`, no solo en la pantalla.

### Cálculo (clase pura en `CapaNegocio`)

- **Minutos de un segmento incluido:** los del segmento, o `MinutosIncluidos` si se redujo.
- **Superposiciones:** se unen por minuto, como en `NegHeFueraDeHorario`: un minuto cuenta una sola vez y, entre 50% y 100%, gana el 100%. Con minutos reducidos, se toman **los primeros N minutos** del segmento.
- **Pago:** `Total50 = redondeo(Minutos50 / 60 × ValorHora50, 2)`, y lo mismo para el 100%. `TotalHE = Total50 + Total100`.
- **Lo no incluido** se valora igual, con el valor hora de la persona, y se muestra como «no incluido en el pago».
- **Personas fuera del módulo:** no tienen fila ni valor hora, así que no se pueden incluir hasta completar sus datos. Siguen en el recuadro amarillo.

### Procedimientos nuevos

- `Sp_RTA_HeTramosCandidatos(@IdPeriodo)`, de solo lectura: devuelve los segmentos candidatos del período, con su decisión vigente y el período que los tiene incluidos, si hay alguno.
- `Sp_RTA_HeGuardarDecisiones(@IdPeriodo, @Decisiones)`: recibe un lote como tabla con tipo definido o como XML e inserta todo en una transacción. Rechaza:
  - `-1`: el período no está abierto;
  - `-2`: algún segmento ya está incluido en otro período;
  - `-3`: minutos o tipo inválidos, o falta el motivo;
  - `-4`: el tramo no es candidato de ese período.
- **La siembra** (`AbrirPeriodo` y «Recalcular») deja de leer `Sp_RTA_HeHorasAprobadas` y usa los candidatos con sus decisiones. **`Sp_RTA_HeHorasAprobadas` no se borra:** es la vuelta atrás.

## Parte 2: pantalla

- **Ventana de detalle de cada persona:**
  - todos sus segmentos candidatos, agrupados en aprobados, sin solicitar, pendientes y rechazados;
  - por cada uno: fecha, tramo, parte, minutos, **valor en USD**, estado, «fuera del período» si corresponde, casilla **Incluir**, selector **50% / 100%**, campo **minutos** (solo para reducir) y **motivo**;
  - un botón **«Guardar decisiones»**, con el mismo paso de confirmación de dos clics y el nombre destacado;
  - al guardar, se recalcula el período.
- **Perfil 2:** los controles aparecen deshabilitados. **Período cerrado:** deshabilitados, con la leyenda «período cerrado».
- **Grilla:**
  - las celdas de horas pasan a solo lectura;
  - «sin aprobar» y «rechazado» pasan a ser «no incluido», en horas y USD;
  - nueva columna **«Total incluyendo no incluido (USD)»**;
  - la Observación sigue editable, y guardar la fila **ya no acepta horas del cliente**.

## Parte 3: Excel

- **Hoja «Conciliación»:** una fila por persona con aprobado incluido, fuera de horario incluido, rechazado incluido, excluido y **total a pagar**, en horas y USD, con los totales. Con las decisiones del análisis manual, el total es **6.241,14**.
- **Hoja «Decisiones»:** cada decisión vigente, con tramo, incluir, tipo, minutos, motivo, usuario y fecha.
- **Las demás hojas se mantienen.** «Pagos» refleja lo incluido.

## Parte 4: pruebas, validación y despliegue

**Pruebas unitarias:**
- cálculo por minutos, que no multiplica horas redondeadas;
- valores por omisión;
- incluir y excluir;
- cambio de tipo;
- minutos reducidos, sin permitir que suban;
- superposiciones;
- dueño del tramo y pago doble;
- período cerrado;
- las 152 pruebas previas del cálculo, adaptadas a minutos donde corresponda.

**Validación, el criterio de terminado:** un script en `Actualizacion/` aplica **en memoria** las decisiones del análisis manual sobre los datos reales, sin escribir en la base, y el total tiene que dar **6.241,14**, persona por persona. Si no da, no se publica.

**Despliegue:**
- un script SQL con la tabla, las columnas y los procedimientos, que necesita aprobación;
- los binarios y la página;
- **avisar a Nómina** que el pago cambia unos centavos por el cálculo por minutos;
- **vuelta atrás:** reponer los binarios anteriores. La siembra vieja sigue funcionando porque `Sp_RTA_HeHorasAprobadas` no se toca, y las columnas y la tabla nuevas quedan sin uso.

**Fuera de alcance:**
- el 14 y el 15-08, que no pertenecen a ningún período (revisar con Nómina);
- pagar a quien no está en el módulo sin completar sus datos.
