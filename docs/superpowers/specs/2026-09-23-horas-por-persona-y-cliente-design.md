# Horas por persona y cliente en el dashboard de jefatura — Diseño

**Fecha:** 2026-09-23
**Estado:** aprobado, pendiente de plan de implementación
**Rama:** `ProyectoNuevosCambios`
**Pantalla:** `AprobacionTareasJefatura.aspx`, vista DASHBOARD

> **Sin datos personales.** Este documento describe estructura y reglas. El repositorio es público.

---

## Qué se pide

Saber **en qué clientes se fue el tiempo de cada persona**, dentro del dashboard que ya
existe, contando solo las horas aprobadas.

---

## Cómo se llegó hasta aquí

El pedido entró como «que un colaborador pueda tener dos o más horarios vigentes a la
vez». Conviene dejar escrito por qué no se construyó eso, porque el camino se va a volver
a proponer:

1. Los dos horarios del ejemplo **no se solapaban** (08:00–12:00 y 14:00–18:00): eso no
   son dos horarios, es una jornada partida.
2. El hueco entre bloques debía seguir contando como **horas normales**. Con esa regla,
   partir el día **no cambia ningún cálculo**: la jornada para el recargo sigue yendo de
   la primera hora a la última.
3. El control de jornada tampoco cambiaría: `CumpleJornada` compara contra un **8 fijo
   escrito en `Sp_RTAListaHorasRecursosPorJefatura`** y no lee el horario.
4. Lo que se quería ver era **horas por área**, y el área resultó ser el cliente.

El horario dice *cuándo* trabaja alguien; la pregunta era *en qué* se fue su tiempo. Son
dos ejes distintos, y el segundo **ya está capturado en cada tarea**.

---

## Decisiones tomadas

| Decisión | Valor |
|---|---|
| Dónde vive | Dentro del dashboard que ya existe, no en una pantalla nueva |
| Qué horas cuentan | **Solo aprobadas** (`Estado = 2`) |
| El corte | **Persona × cliente**: filas las personas, columnas los clientes |
| Qué es «cliente» | `R_TareasAranda.Nom_Empresa` |
| Quién lo ve | Lo mismo que rige hoy esa pantalla: la jefatura su equipo, el perfil 18 todo |
| El gráfico de empresas que ya existe | **Se corrige**: pasa a mostrar solo aprobadas |

---

## Terreno verificado

**La atribución ya existe y está completa.** Sobre las últimas 4.000 tareas:

| dimensión | llenas |
|---|---|
| `Nom_Empresa` | 3.982 |
| `IdProyecto` | 4.000 |
| `Categoria` | 4.000 |
| `Num_OrdenServicio` | 3.871 |

No hay que capturar ningún dato nuevo. Se eligió `Nom_Empresa` y no `IdProyecto` porque
los valores de proyecto (9, 6, 1, 2) **no tienen catálogo con nombres en la base**: un
reporte que diga «Proyecto 9» no lo puede leer nadie.

**El dashboard ya agrupa por empresa, pero mezcla estados.** El conjunto 5 de
`Sp_RTA_DashboardAprobacionJefatura` hace `SUM(Minutos)` sin mirar el estado, mientras las
tarjetas de arriba sí separan aprobadas de pendientes. Ese gráfico está hoy en producción
mostrando un número que no corresponde a ninguna de las dos tarjetas.

**El recorte a «top N + Otras» ya está escrito y probado:**
`NegDashboardAprobacion.TopConOtras`. El procedimiento devuelve todas las empresas a
propósito, para que esa regla viva en un solo lenguaje y con pruebas.

---

## 1. El procedimiento

Un script recrea `Sp_RTA_DashboardAprobacionJefatura` con dos cambios sobre `#Base`, la
tabla temporal que ya arma el mismo parseo de `Det_Tiempo` que usa la tabla de APROBADAS.
Compartir `#Base` es lo que garantiza que el reporte **cuadre** con lo que la pantalla ya
muestra, en vez de contradecirlo.

`#Base` ya trae todo lo que el conjunto nuevo necesita, así que la consulta que la llena
**no se toca**:

```
Id_Responsable, Nombre, Fecha, Estado, Empresa, Minutos, FechaAprob
```

**Conjunto 5, corregido.** Gana una columna:

```
Empresa, Minutos, MinutosAprobados
```

`Minutos` se conserva —quitarla rompería el DAO desplegado durante la ventana entre el
script y los binarios— pero la pantalla pasa a pintar `MinutosAprobados`.

**Conjunto 6, nuevo.** Agrupa por responsable y empresa, solo aprobadas:

```
Id_Responsable, Nombre, Empresa, MinutosAprobados      -- solo Estado = 2
```

Devuelve **todas** las combinaciones, sin recortar: el top N y la columna «Otras» los arma
`CapaNegocio`, igual que hoy.

El script es idempotente (`DROP` + `CREATE`) y **no toca ninguna tabla: solo lee**.

---

## 2. Las capas

| Capa | Cambio |
|---|---|
| `CapaEntidad` | `EntDashboardEmpresa` gana `MinutosAprobados`. Entidad nueva `EntDashboardPersonaEmpresa` (Id_Responsable, Nombre, Empresa, Minutos) y su lista en la entidad raíz |
| `CapaDato` | `DaoDashboardAprobacion` lee un sexto conjunto y la columna nueva del quinto |
| `CapaNegocio` | El pivote persona × cliente y el recorte de columnas. Reutiliza `TopConOtras` para elegir columnas, y la conversión de minutos a horas que ya existe |
| `ReporteTareas` | Sin cambios en el handler: la acción `DashboardAprobacion` ya serializa la entidad entera |

**El pivote va en `CapaNegocio`, no en SQL ni en JavaScript.** Es la misma razón por la que
las tres conversiones del dashboard viven ahí: es una regla, se prueba sin base, y no se
puede duplicar en dos lenguajes sin que las copias se separen.

---

## 3. La pantalla

Debajo del gráfico de empresas que ya existe, una tabla:

```
Persona          GPF    COMPUEQUIP    HPI    Otras    Total
-----------------------------------------------------------
PEREZ JUAN      42,0        8,5       2,0      1,5     54,0
GOMEZ ANA       18,0       21,0       0,0      3,0     42,0
-----------------------------------------------------------
Total           60,0       29,5       2,0      4,5     96,0
```

- Columnas: los **8 clientes con más horas aprobadas** del rango, más «Otras» y el total.
  Ocho y no diez porque aquí cada columna compite con el ancho de la pantalla, no con la
  leyenda de un gráfico.
- Filas ordenadas por total descendente.
- **Sin gráfico nuevo.** Con treinta personas y cuarenta clientes un apilado no se lee; una
  tabla sí, y además se puede copiar a una hoja de cálculo.
- Horas con coma decimal, como el resto de la pantalla.

---

## 4. Errores y vacíos

- **Rango sin horas aprobadas:** la tabla dice «sin horas aprobadas en el rango», no queda
  vacía. Un cuerpo en blanco y un error se ven igual.
- **Empresa vacía:** las tareas sin `Nom_Empresa` (18 de cada 4.000) se agrupan bajo
  `(sin empresa)`. No se descartan: si se descartaran, la suma de la tabla dejaría de
  coincidir con la tarjeta de horas aprobadas, y esa diferencia haría dudar de las dos.
- **Una persona sin horas aprobadas** no aparece como fila. La tabla es del tiempo
  aprobado, no del equipo.

---

## 5. Pruebas

En `CapaPruebas/NegDashboardAprobacionTests.cs`, escritas antes del código:

- El pivote coloca cada valor en su celda, y las celdas sin dato quedan en cero.
- Los totales por fila y por columna cuadran con la suma de las celdas.
- Con más clientes que columnas, el sobrante cae entero en «Otras» —ni se pierde ni se
  duplica— y el total general no cambia.
- Una empresa vacía o nula se agrupa bajo `(sin empresa)` y sigue sumando.
- Las filas salen ordenadas por total descendente.

La lectura del sexto conjunto en `CapaDato` no se prueba automáticamente: necesita base. Se
comprueba a mano con el criterio de abajo.

---

## 6. Cómo se verifica que no miente

Para un mismo rango y una misma persona, **el total de su fila tiene que ser igual a sus
horas aprobadas en la tabla de APROBADAS**. Comparten `#Base`, así que cualquier diferencia
es un defecto del pivote y no una diferencia de criterio.

---

## 7. Lo que queda fuera, a propósito

- **Jornada partida y dos horarios simultáneos.** Ver «Cómo se llegó hasta aquí».
- **Elegir la dimensión** (cliente / proyecto / categoría) desde un combo: `IdProyecto` no
  tiene nombres y `Categoria` responde otra pregunta.
- **Descarga a Excel.** Si el reporte hay que mandarlo por correo todos los meses, es una
  entrega aparte, con el patrón de `DescargarHorasExtras.ashx`.
- **Horas pendientes en la tabla.** Se pidió solo aprobadas.

---

## 8. Riesgos conocidos

**El gráfico de empresas va a mostrar números más bajos de un día para el otro.** Es la
corrección pedida —hoy mezcla aprobadas con pendientes—, pero la jefatura ya está mirando
ese gráfico. **Hay que avisarlo antes de publicar**, o va a parecer una pérdida de datos.

**La ventana entre el script y los binarios.** El script agrega una columna y un conjunto;
el DAO viejo ignora los dos y sigue funcionando igual: ese es el sentido seguro. Al revés
—binarios nuevos sobre el procedimiento viejo— el dashboard falla al leer el conjunto que
todavía no existe. El script va primero, como manda `DESPLIEGUE.md` sección 4.

**`Nom_Empresa` es texto libre.** Si el mismo cliente está escrito de dos formas, salen dos
columnas. No se normaliza en esta entrega: hacerlo a ciegas uniría cosas que quizá no son
la misma, y el dato viene de Aranda.

---

## 9. Despliegue

1. `docs/sql/2026-09-23-dashboard-horas-por-cliente.sql`
2. `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe`, `bin\CapaDato.exe`
3. `Formulario\AprobacionTareasJefatura.aspx` y `js\dashboardAprobacion.js`, con el `?v=`
   subido — sin eso, el navegador sigue con el JavaScript viejo y la tabla no aparece.

Los tres de capa son `.exe`, no `.dll`.
