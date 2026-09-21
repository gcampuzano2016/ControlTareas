# Dashboard de aprobación de tareas para jefatura — Diseño

**Fecha:** 2026-09-21
**Estado:** aprobado, pendiente de plan de implementación
**Rama:** `ProyectoNuevosCambios`
**Pantalla:** `AprobacionTareasJefatura.aspx`

> **Sin datos personales.** Este documento describe estructura y reglas. El repositorio es público.

---

## Qué se pide

Un dashboard con Chart.js dentro de `AprobacionTareasJefatura.aspx` que permita ver
los estados de las tareas y hacer análisis sobre ellos.

---

## Decisiones tomadas

| Pregunta | Decisión |
|---|---|
| ¿Dónde vive? | **Una tercera vista en la misma pantalla** (`Pagina3`), con código en archivos nuevos |
| ¿Qué unidad compara? | **Horas y personas-día.** Las tareas no ejecutadas van como indicador aparte, nunca mezcladas en un gráfico |
| ¿Qué análisis? | **Los cuatro:** evolución en el tiempo, por persona, demora en aprobar, por empresa |
| ¿Chart.js? | **Archivo local** en `js/`, no CDN |

---

## Terreno verificado

Medido **contra la base de producción** el 2026-09-21, con consultas de lectura.

### Los tres «estados» de la pantalla no son tres cortes del mismo dato

Es el hallazgo que da forma a todo el diseño. El combo ofrece tres opciones, pero
detrás hay **dos consultas distintas con unidades distintas**:

| Opción | De dónde sale | Qué es una fila |
|---|---|---|
| PENDIENTE DE APROBACIÓN (1) | `Sp_RTAListaHorasRecursosPorJefatura`, con `Det_Aprobacion_Tarea_Estado = 1` | Una **persona-día** |
| TAREAS APROBADAS (2) | la misma, con estado `2` | Una **persona-día** |
| TAREAS NO EJECUTADAS (3) | **otra**: `Sp_RTAConsultaTareasGeneradas @Tipo = 2` | Una **tarea** |

La tercera **no filtra por estado 3**. El JavaScript la trata aparte: oculta
`Pagina1`, muestra `Pagina2` y llama a otra acción. Dibujarlas como tres porciones
de una torta sumaría personas-día con tareas: el número se vería bien y no
significaría nada.

> **Este documento se equivocó primero en esto.** La lectura inicial fue que la
> opción 3 filtraba por un estado inexistente y por lo tanto no devolvía nada
> nunca. Es falso, y sólo se descubre leyendo el JavaScript, no el procedimiento.
> Queda escrito para que nadie repita el atajo.

### Qué hay de verdad en `Det_Aprobacion_Tarea_Estado`

Sobre las **86.496 filas** de `R_DetTareasAranda`:

| Estado | Filas | En el combo | En el catálogo |
|---|---|---|---|
| 2 | 80.329 | sí, «TAREAS APROBADAS» | sí, `Aprobado` |
| 1 | 5.575 | sí, «PENDIENTE DE APROBACION» | sí, `En Aprobación` |
| **5** | **584** | **no** | **no** |
| **7** | **8** | **no** | **no** |

**Nunca hubo un estado 3.** Y los estados **5 y 7 no los ve nadie hoy**: no están
en el combo ni en `Catalogo` (tipo 3, que sólo define 1 y 2). Son 592 filas
invisibles.

### La calidad de los datos alcanza para los cuatro análisis

Últimos 6 meses: **5.665 filas, 51 responsables, 71 empresas.**

| Columna | Poblada | Habilita |
|---|---|---|
| `Det_Tiempo` | 100% | Horas |
| `IdTipoGasto` | 100% | — |
| `Det_Nom_Empresa` | 99,8% | Análisis por empresa |
| `Det_Fecha_Aprobacion_Tarea` | 97,7% | **Demora en aprobar** |
| `Det_Id_Usuario_Aprobacion_Tarea` | 97,7% | Quién aprueba |

### `Det_Tiempo` es `HH:MM:SS`, y el parseo actual pierde los segundos

No es `HH:MM`: son 8 caracteres. El procedimiento vigente hace
`SUBSTRING(Det_Tiempo,1,2)*60 + SUBSTRING(Det_Tiempo,4,2)`, que lo lee bien pero
**descarta los segundos**. Hay **5.616 filas (6,5%) con segundos distintos de
cero**.

**El dashboard replica ese parseo tal cual**, con su imprecisión incluida. No es
pereza: si el gráfico sumara los segundos y la tabla de abajo no, dirían cifras
distintas sobre los mismos datos y se perdería la confianza en las dos. El desvío
máximo es de 59 segundos por fila sobre el 6,5% de las filas.

### Una fecha imposible

Hay **una fila con `Det_Fch_RegDetalleIni` = 18/03/2502**. Una sola, pero en un eje
temporal estira el gráfico siete siglos y aplasta todo lo demás. El filtro por
rango que la pantalla ya aplica la deja fuera; se documenta para que quien mire un
gráfico raro sepa dónde buscar.

### El handler no puede comprobar la sesión

`ObtenerListaTareas` se declara `IHttpHandler` **sin `IRequiresSessionState`**, así
que ahí `context.Session` es null. Por eso la identidad de quien consulta viaja
como **parámetro cifrado desde el cliente** y se resuelve con
`SeguridadHelper.Desencripta`.

Es el mismo patrón contra el que advierte el módulo de perfil a propósito de
`CargaArchivos.ashx`. **La acción nueva lo hereda**, porque cambiarlo obligaría a
tocar las más de veinte acciones de este handler y es un trabajo aparte. Queda
dicho para que sea una decisión consciente y no un descuido: el dashboard agrega
horas de todo un equipo, y quien consiga un token cifrado válido de otro jefe vería
su equipo.

---

## 1. Dónde vive

Una tercera vista, `Pagina3`, encendida por una opción nueva del combo de estados.
La pantalla **ya alterna** entre `Pagina1` y `Pagina2` con ese mismo interruptor, así
que el dashboard es una rama más y no un mecanismo nuevo.

**Código nuevo en archivos nuevos.** `aprobacionTareasJefatura.js` tiene 750 líneas
y el `.aspx` 471; meter cuatro gráficos adentro es exactamente cómo se rompe lo que
hoy funciona.

| Archivo | Responsabilidad |
|---|---|
| `ReporteTareas/js/chart.min.js` | La librería, servida local |
| `ReporteTareas/js/dashboardAprobacion.js` | Pedir los datos y dibujar |
| `docs/sql/2026-09-21-dashboard-aprobacion.sql` | `Sp_RTA_DashboardAprobacionJefatura` |
| `CapaEntidad/EntDashboardAprobacion.cs` | Los cinco bloques del resultado |
| `CapaDato/DaoDashboardAprobacion.cs` | Leer los cinco conjuntos |
| `CapaNegocio/NegDashboardAprobacion.cs` | Conversiones y agrupamientos, con pruebas |
| `ObtenerListaTareas.ashx.cs` | Una acción: `DashboardAprobacion` |
| `AprobacionTareasJefatura.aspx` | El marcado de `Pagina3` y la opción del combo |

---

## 2. Un procedimiento, cinco conjuntos

`Sp_RTA_DashboardAprobacionJefatura` recibe los **mismos tres parámetros que la
pantalla ya usa** —jefe, fecha desde, fecha hasta— y devuelve todo **ya agregado**.

El navegador dibuja; no suma. Traer 5.665 filas para que el JavaScript las recorra
sería lento, y además pondría el cálculo de horas en un segundo lugar, distinto del
que usa la tabla.

| # | Conjunto | Columnas | Alimenta |
|---|---|---|---|
| 1 | **Totales** | horas y personas-día aprobadas, pendientes, otros estados; días bajo 8 h | Las tarjetas |
| 2 | **Por semana** | semana, horas aprobadas, horas pendientes | Línea de evolución |
| 3 | **Por responsable** | nombre, horas, personas-día, días incompletos | Barras por persona |
| 4 | **Demora** | promedio y máximo de días entre registro y aprobación; fecha de lo más viejo sin aprobar | Barras + una cifra |
| 5 | **Por empresa** | empresa, horas — top 10 y el resto como «Otras» | Torta |

### Por qué el top 10 y no las 71

Una torta de 71 porciones no se lee. Las diez primeras por horas, y el resto sumado
en «Otras», que además deja ver de un vistazo cuánto pesa la cola larga.

### Los estados 5 y 7

Entran en las tarjetas agrupados como **«Otros estados»**. Son 592 filas que hoy no
aparecen en ninguna pantalla. No se inventa un nombre para ellos —no está en el
catálogo— pero tampoco se los esconde: que el total de las tarjetas cuadre con el
total del rango es lo que hace creíble al resto.

---

## 3. Los cuatro análisis

**Evolución en el tiempo.** Línea con dos series, horas aprobadas y pendientes, por
semana. Responde lo único que un total no puede: si el pendiente se acumula o se
está drenando.

**Por persona.** Barras horizontales por responsable, ordenadas por horas, con los
días que no llegaron a 8 h marcados aparte. El umbral de 8 h **no es nuevo**: es el
mismo que ya calcula `CumpleJornada` en el procedimiento vigente, y se reusa para
que el dashboard y la tabla coincidan.

**Demora en aprobar.** Días entre `Det_Fch_RegDetalleIni` y
`Det_Fecha_Aprobacion_Tarea`, en promedio y máximo, más la fecha de lo más viejo
que sigue pendiente. Es el análisis que el 97,7% de cobertura habilita y el que
convierte el tablero en algo accionable: no «cuánto hay pendiente» sino «hace
cuánto».

**Por empresa.** Torta con el top 10 más «Otras», sobre `Det_Nom_Empresa`.

### Las tareas no ejecutadas

Una **tarjeta con su número y un enlace** a la vista que ya existe. No entran en
ningún gráfico: son tareas, no personas-día, y mezclarlas es el error que este
diseño existe para evitar.

---

## 4. Errores y vacíos

**Si el procedimiento falla**, la vista muestra el mensaje de error y **no dibuja
gráficos**. Un gráfico en cero y un gráfico que no cargó se ven igual y significan
cosas opuestas.

**Si un conjunto viene sin filas**, ese gráfico dice «Sin datos en el rango» en vez
de quedar en blanco.

**Si Chart.js no cargó** —archivo no publicado, por ejemplo— la vista lo dice en
lugar de fallar con `Chart is not defined` en la consola, donde nadie mira.

---

## 5. Pruebas

`CapaPruebas` sólo referencia `CapaEntidad` y `CapaNegocio`. Lo agregado vive en
SQL y no es alcanzable desde ahí.

**Probable, y por lo tanto va en `CapaNegocio` con pruebas:**

- `MinutosDesdeTiempo("HH:MM:SS")`: la conversión a minutos, replicando el descarte
  de segundos del procedimiento vigente, y devolviendo 0 ante cualquier cosa que no
  tenga esa forma en vez de reventar.
- `HorasDecimales(minutos)`: minutos a horas con un decimal, que es como se
  etiquetan los gráficos.
- `TopConOtras(lista, 10)`: el agrupamiento del top 10 más «Otras», incluidos los
  casos de menos de diez elementos y de lista vacía.
- `TextoDemora(dias)`: «hoy», «1 día», «N días», sin plurales raros.

**No probable, y se verifica a mano:** que los cinco conjuntos traigan lo que
dicen. La verificación que importa es **comparar los totales del dashboard contra
la tabla que ya está en la misma pantalla, para el mismo rango**: si no coinciden,
uno de los dos miente, y es la mejor prueba de que el tablero sirve.

---

## 6. Lo que queda fuera, a propósito

- **Arreglar la identidad del handler.** `ObtenerListaTareas` no puede leer la
  sesión y confía en un parámetro cifrado del cliente. Cambiarlo toca más de veinte
  acciones y es un trabajo propio.
- **Darles nombre a los estados 5 y 7.** Se muestran agrupados; averiguar qué
  significan y darlos de alta en `Catalogo` es otra tarea.
- **Corregir la fila con fecha de 2502.** Es un dato a corregir, no código.
- **Exportar el dashboard.** No se pidió.
- **Cambiar el parseo de `Det_Tiempo`** para no perder segundos. Habría que
  cambiarlo en los dos lugares a la vez o quedarían discrepando.

---

## 7. Riesgos conocidos

| Riesgo | Mitigación |
|---|---|
| Que el dashboard y la tabla muestren cifras distintas | Mismo parseo de horas y mismo umbral de 8 h que el procedimiento vigente; la verificación manual los compara |
| Mezclar personas-día con tareas y dar un número sin sentido | Las tareas no ejecutadas van en una tarjeta aparte, nunca en un gráfico |
| Una torta de 71 porciones ilegible | Top 10 más «Otras» |
| Un gráfico vacío que parece «no hay nada» cuando en realidad fallo | Error y vacío se dicen distinto, con texto explícito |
| Chart.js servido por CDN que no se alcanza desde la red interna | Archivo local |
| Un eje temporal estirado por la fila de 2502 | El filtro por rango la deja fuera; documentado por si reaparece |
