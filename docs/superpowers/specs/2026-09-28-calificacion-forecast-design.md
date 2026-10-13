# Calificación de oportunidades del Forecast — Diseño

**Fecha:** 2026-09-28
**Estado:** aprobado, pendiente de plan de implementación
**Rama:** `ProyectoNuevosCambios`
**Pantalla nueva:** `CalificacionForeCast.aspx`

> **Sin datos personales.** Este documento describe estructura y reglas. El repositorio es público:
> no se citan clientes, contactos ni montos reales.

---

## Qué se pide

Una pantalla que **califique cada oportunidad abierta del Forecast** con un puntaje de 0 a 100,
la clasifique (Caliente / Tibia / Fría), muestre sus **alertas y datos faltantes** y sugiera la
**siguiente acción**, para que ventas sepa qué priorizar cada semana.

---

## Cómo se llegó hasta aquí

Antes de diseñar se hizo una evaluación manual de las oportunidades abiertas (LEAD, PIPELINE,
UPSIDE, FORECAST) leyendo `dbo.ForeCast`. De esa evaluación salieron tres hallazgos que definen
el diseño:

1. **El 60 % del puntaje no se puede calcular.** Presupuesto, Autoridad y Necesidad solo se
   podían inferir leyendo `Observaciones` en texto libre. Un sistema no lo hace de forma
   confiable, y el CRM **no registra el cargo** de ningún contacto. Se decidió que **el vendedor
   los responda** con opciones cerradas.
2. **`CierreNegocio` no es una probabilidad.** Es el mismo valor que `IdNegocio`, la llave de
   `PrmPorcentaje`, y el porcentaje real está en `PrmPorcentaje.Porcentaje`. Leerlo como
   «n / 10» casi duplicaba el monto ponderado.
3. **`FechaCobro` es la fecha de última modificación del registro**, no una fecha de cobro. Es
   la única fecha con hora que se actualiza y se usa como señal de actividad. Muchas filas
   comparten la misma fecha por ediciones en lote, así que calificar también cuenta como
   actividad.

Otros datos verificados:

- `FechaRegistroOportunidad` dejó de llenarse en 2022. No se usa.
- `MesEstimadoCierre` y `FechaFacturacion` (texto) están casi siempre en «-- SELECCIONE --».
  La fecha de cierre real es `FechaFacturacionProyecto`.
- `ForeCast` **no tiene triggers**. `ForeCastHistorico` lo llena el SP de guardado y no tiene
  fecha del cambio, pero sí permite contar cuántas fechas de cierre distintas tuvo una oportunidad.
- La visibilidad por usuario vive en `Sp_RTAListaForeCast`: filtros de sucursal por usuario
  escritos en el SP y reglas de gerente de cuenta. **No se duplica.**

---

## Decisiones tomadas

| Decisión | Valor |
|---|---|
| Presupuesto, Autoridad, Necesidad | Los **responde el vendedor** con opciones cerradas y puntaje fijo |
| Dónde se responden | Modal en la **pantalla nueva**. `RegistroForeCast` no se toca |
| Quién ve qué | **La misma visibilidad que el listado actual de Forecast** |
| Menú | Mismos perfiles que tienen la opción de Forecast |
| Dónde vive la lógica | **C# (`NegCalificacionForeCast`)**, probada con MSTest. SQL solo trae datos |
| `ForeCast` | **No se modifica**. Las respuestas van en una tabla nueva |
| Probabilidad | `PrmPorcentaje.Porcentaje` vía `IdNegocio` |
| Última actividad | La fecha más reciente entre `FechaCobro` y la última calificación |
| Cliente ideal | Se **recalcula** con los GANADO y PERDIDO de los últimos 36 meses |

---

## Puntaje (100)

### Respondidos por el vendedor (60)

**Presupuesto (20): ¿en qué estado está el presupuesto del cliente?**

| Opción | Puntos |
|---|---|
| Aprobado o asignado (partida, PAC u OC) | 20 |
| En trámite (en POA/PAC, pendiente de aprobación) | 12 |
| Solo lo estimamos nosotros | 5 |
| No tiene / no sabemos | 0 |

**Autoridad (20): ¿con quién estamos hablando?**

| Opción | Puntos |
|---|---|
| Decisor final (gerente, director, rectorado con firma) | 20 |
| Influenciador con acceso al decisor (por ejemplo, jefe de TI) | 12 |
| Solo usuario técnico | 5 |
| Sin contacto | 0 |

**Necesidad (20): ¿qué tan clara es la necesidad?**

| Opción | Puntos |
|---|---|
| Crítica y con fecha (vence el soporte, equipo en EOSL, proceso publicado o TDR en curso) | 20 |
| Clara y reconocida por el cliente | 14 |
| Interés exploratorio | 6 |
| No definida | 0 |

Además, el modal pide:
- **Nombre y cargo del decisor** (texto).
- **¿Propuesta enviada?** (Sí / No).
- **Próxima acción comprometida**, en texto, y su fecha.

Una oportunidad **sin calificar** suma 0 en estos tres criterios.

### Calculados (40)

**Tiempo (15)**, según `FechaFacturacionProyecto` frente a la fecha del servidor:

| Condición | Puntos |
|---|---|
| Cierre en 0 a 60 días | 15 |
| 61 a 120 días | 12 |
| 121 a 240 días | 8 |
| Más de 240 días | 5 |
| Fecha vencida | 3 |
| Sin fecha | 0 |

**Actividad (15)**, según la última actividad (la fecha más reciente entre `FechaCobro` y la
última calificación):

| Días desde la última actividad | Puntos |
|---|---|
| 0 a 14 | 15 |
| 15 a 30 | 10 |
| 31 a 60 | 5 |
| Más de 60 | 0 |

**Encaje con cliente ideal (10)**, calculado con los cierres GANADO y PERDIDO de los últimos
36 meses. La ventana se mide sobre `FechaCobro`:

| Componente | Regla | Puntos |
|---|---|---|
| Segmento (`SegmentodeMercado`) | Tasa de éxito ≥ 60 % | 5 |
| | ≥ 50 % | 3 |
| | < 50 % | 1 |
| | Menos de 10 cierres en el segmento (sin evidencia) | 3 |
| Tipo de empresa (`TipoEmpresa`) | El tipo con mejor tasa de éxito | 2 |
| | El otro | 1 |
| Ventas previas al cliente | 3 o más GANADO | 3 |
| | 1 o 2 GANADO | 2 |

El total de Encaje se limita a 10.

### Clasificación

| Puntaje | Estado |
|---|---|
| 75 – 100 | Caliente |
| 50 – 74 | Tibia |
| 0 – 49 | Fría |

Una oportunidad sin calificación vigente muestra además la etiqueta **«Sin calificar»**, que
funciona como filtro.

---

## Monto ponderado

`PVPEstimado × PrmPorcentaje.Porcentaje / 100`. Si el porcentaje es nulo, el ponderado es 0.

**Etapa avanzada:** `Prioridad` UPSIDE o FORECAST, **o** `Porcentaje` ≥ 40 («Propuesta
entregada» en adelante).

---

## Alertas

| # | Alerta | Regla |
|---|---|---|
| 1 | Sin actividad | Última actividad hace más de 30 días |
| 2 | Cierre vencido | `FechaFacturacionProyecto` < hoy |
| 3 | Monto sin definir | Etapa avanzada y `PVPEstimado` nulo o 0 |
| 4 | Avanzada sin propuesta | Etapa avanzada y «¿Propuesta enviada?» ≠ Sí (incluye sin calificar) |
| 5 | Sin decisor | Contactos distintos en la cuenta ≤ 1 **y** Autoridad < 20 |
| 6 | Cierre cambiado varias veces | 3 o más `FechaFacturacionProyecto` distintas en `ForeCastHistorico` |
| 7 | Calificación pendiente | Sin calificar, o última calificación hace más de 30 días |
| 8 | Dato inconsistente | Etapa abierta con `IdNegocio` = «Perdida o Descartada» |

«Contactos distintos en la cuenta» cuenta los valores distintos no vacíos de `Contacto` y
`LiderProyectoCliente` en todas las oportunidades del mismo `IdCliente`.

## Datos faltantes

Se listan los vacíos entre:
- Contacto (`Contacto` y `LiderProyectoCliente` vacíos).
- Fecha de cierre.
- Utilidad bruta estimada.
- Decisor con cargo (del modal).
- Calificación.

## Acción sugerida

Regla determinista. Se elige **una** acción, la primera que aplique en este orden:

1. Dato inconsistente → «Cerrar o corregir la etapa: el código indica perdida o descartada».
2. Cierre vencido → «Actualizar la fecha de cierre con el cliente».
3. Sin calificar → «Calificar la oportunidad».
4. Avanzada sin propuesta → «Enviar la propuesta».
5. El criterio del vendedor con menos puntos, en proporción a su máximo:
   - Presupuesto → «Confirmar el presupuesto con el cliente».
   - Autoridad → «Identificar y reunirse con el decisor».
   - Necesidad → «Validar la necesidad y la fecha del requerimiento».
6. Sin actividad → «Retomar contacto con el cliente».
7. Si no aplica ninguna → «Avanzar a la siguiente etapa».

La acción comprometida por el vendedor, cuando existe, se muestra junto a la sugerida.

---

## Pantalla

`ReporteTareas/Formulario/CalificacionForeCast.aspx` + `js/CalificacionForeCast.js`.

- **Filtros:** gerente de cuenta, sucursal, etapa (`Prioridad`), estado (Caliente / Tibia /
  Fría / Sin calificar) y marca.
- **Indicadores:**
  - Oportunidades abiertas.
  - Monto total.
  - Monto ponderado.
  - Cantidad de calientes, tibias y frías.
  - Porcentaje sin calificar.
- **Tabla** (DataTables, ordenada por puntaje descendente):
  - Columnas: cliente, oportunidad, etapa, monto, puntaje, estado (con color), número de
    alertas y botón **Calificar**.
  - Al hacer clic en la fila se despliega el desglose de los seis criterios, las alertas, los
    datos faltantes, la acción sugerida y la acción comprometida.
- **Top 5 de la semana:** calientes o tibias, ordenadas por monto ponderado descendente. En
  empate, gana la de cierre más cercano.
- **En riesgo:** frías con alerta 1, 2 u 8.
- **Modal Calificar:** las preguntas de «Respondidos por el vendedor». Al guardar se recalcula
  la fila sin recargar la página.
- **Exportar a Excel:** la tabla con el desglose, siguiendo el patrón de descarga existente.

---

## Datos

### Tabla nueva `dbo.ForeCastCalificacion`

| Columna | Tipo | Nota |
|---|---|---|
| IdCalificacion | bigint IDENTITY | PK |
| IdForeCast | bigint | FK lógica a `ForeCast` |
| Presupuesto | tinyint | Puntos de la opción elegida (0/5/12/20) |
| Autoridad | tinyint | 0/5/12/20 |
| Necesidad | tinyint | 0/6/14/20 |
| DecisorCargo | varchar(150) | Nombre y cargo |
| PropuestaEnviada | bit | |
| AccionComprometida | varchar(300) | |
| FechaAccion | date | |
| Usuario | varchar(50) | Quién calificó |
| FechaRegistro | datetime | `GETDATE()` |

Cada guardado **inserta** una fila nueva, así queda el historial. La calificación vigente es la
de mayor `FechaRegistro` por `IdForeCast`. Índice por `(IdForeCast, FechaRegistro DESC)`.

### `Sp_RTAListaCalificacionForeCast`

- **Parámetros:** los mismos de visibilidad y filtro que `Sp_RTAListaForeCast` usa en esta
  pantalla (`@Idusuario`, `@IdGerenteCuenta`, `@sucursal`, `@IdMarca`, `@IdPrioridad`).
- **Visibilidad:** hace `INSERT … EXEC Sp_RTAListaForeCast` en una tabla temporal y toma solo
  `IdForeCast`. La regla de visibilidad sigue viviendo en un único lugar.
- **Filtro de estado:** se queda con las oportunidades abiertas (`Prioridad` LEAD, PIPELINE,
  UPSIDE o FORECAST).
- **Qué devuelve por cada una:**
  - Los datos crudos de `ForeCast`.
  - `PrmPorcentaje.Porcentaje`.
  - La calificación vigente.
  - El número de fechas de cierre distintas en `ForeCastHistorico`.
  - Los GANADO del cliente.
  - Los contactos distintos de la cuenta.
- **Segundo resultado:** GANADO y PERDIDO de los últimos 36 meses por `SegmentodeMercado` y por
  `TipoEmpresa`, para calcular el Encaje.
- **Solo lectura.**

### `Sp_RTAInsertaCalificacionForeCast`

Inserta una fila en `ForeCastCalificacion`. Valida que los puntos sean de los permitidos y
que el `IdForeCast` exista y esté abierto.

### Menú

Script idempotente sobre `MenuDos` / `PerfilMenu`, con el mismo patrón que
`Script_Menu_ParametrizacionHorario.sql` y los mismos perfiles que tienen Forecast.

Todo el SQL va en `docs/sql/2026-09-28-calificacion-forecast.sql`.

---

## Código

| Capa | Archivo | Responsabilidad |
|---|---|---|
| Entidad | `CapaEntidad/EntCalificacionForeCast.cs` | Datos crudos + resultado (criterios, puntaje, estado, alertas, faltantes, acción) |
| Dato | `CapaDato/DaoCalificacionForeCast.cs` | Llama a los dos SP |
| Negocio | `CapaNegocio/NegCalificacionForeCast.cs` | **Todas las reglas** de este documento. Recibe la fecha de hoy como parámetro |
| Web | `Formulario/AdministrarCalificacionForeCast.ashx` | Listar, guardar y exportar |
| Web | `Formulario/CalificacionForeCast.aspx` + `js/CalificacionForeCast.js` | Pantalla y modal |

`NegCalificacionForeCast` recibe la fecha de hoy como parámetro para que las pruebas no dependan
del reloj.

---

## Pruebas (MSTest, `CapaPruebas/NegCalificacionForeCastTests.cs`)

- **Tiempo:** límites en −1, 0, 60, 61, 120, 121, 240, 241 días y sin fecha.
- **Actividad:** límites en 14/15, 30/31, 60/61. Toma la fecha más reciente entre modificación
  y calificación.
- **Encaje:** cada tramo de tasa de éxito, un segmento con menos de 10 cierres y el tope de 10.
- **Clasificación:** 49/50 y 74/75.
- **Cada una de las 8 alertas:** un caso que la dispara y uno que no.
- **Acción sugerida:** el orden de precedencia.
- **Ponderado:** con porcentaje nulo y con 0.
- **Cultura hostil (es-ES):** los montos y porcentajes salen al JSON con punto decimal.

---

## Despliegue

Se agrega una sección a `DESPLIEGUE.md`:
1. Correr el script SQL (tabla, SP y menú).
2. Publicar los binarios y las páginas.
3. Verificar que un usuario con filtro de sucursal vea las mismas oportunidades que en Forecast.

---

## Riesgos conocidos

- **`INSERT … EXEC` exige que la tabla temporal replique exactamente las columnas** que devuelve
  `Sp_RTAListaForeCast`. Si alguien agrega una columna a ese SP, la pantalla nueva falla con un
  error de columnas. Mitigación: el plan verifica primero la forma exacta del resultado, y
  `DESPLIEGUE.md` deja la advertencia junto a ese SP. Si la verificación muestra que no es viable
  (por ejemplo, porque devuelve más de un resultado), se detiene y se revisa esta decisión antes
  de duplicar la visibilidad.

---

## Fuera de alcance

- Calificar desde `RegistroForeCast`.
- Análisis automático del texto de las notas (palabras clave o IA).
- Notificaciones o correos por alertas.
- Cambiar datos de `ForeCast` desde esta pantalla, incluido cerrar oportunidades.
