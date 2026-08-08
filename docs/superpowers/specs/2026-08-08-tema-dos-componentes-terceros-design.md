# Tema DOS — componentes de terceros

Fecha: 2026-08-08
Rama: `ProyectoNuevosCambios`
Continúa: `fb3d175` (tema DOS sobre SB Admin 2) … `e778c73` (ubicación actual y filtro del menú)

## Problema

El tema DOS cubre lo que es markup propio de la aplicación: barras, paneles, botones,
formularios, tablas y estados. Los widgets de terceros quedaron fuera y conservan su
apariencia original, que viene de paletas ajenas a la marca. El caso más visible es el
calendario de jQuery UI: llega con el tema `smoothness`, cuyo azul choca de frente con el
bermellón en las 36 pantallas que capturan fechas.

El efecto es una aplicación que se ve tematizada hasta que el usuario abre un desplegable,
un calendario o un diálogo de confirmación.

## Terreno verificado

El reconocimiento corrigió dos supuestos de partida. Ambos cambian el alcance.

### DataTables no se carga

Ninguna página ni `Master.Master` incluye el script del plugin. Las 44 llamadas
`.DataTable()` repartidas por `js/` no tienen plugin detrás. La clase
`dataTables_wrapper` aparece en 34 páginas pero está escrita a mano en el markup como
`<div>` envoltorio:

```html
<div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
```

No existen entonces buscador, paginación ni contador de DataTables que tematizar. Queda
sólo el envoltorio.

### Cada librería está duplicada en dos versiones

| Librería | Cómo se carga | Selectores que hay que cubrir |
|---|---|---|
| select2 v3 | global, `Master.Master` → `bower_components/assets/js/select2.min.js` | `.select2-container`, `.select2-choice` |
| select2 v4 | CDN jsdelivr en páginas puntuales (4.0.13 y 4.1.0-rc.0) | `.select2-container--default` |
| SweetAlert v1 | global, `Master.Master` | `.sweet-alert` |
| SweetAlert 2 | 7 páginas | `.swal2-popup` |
| jQuery UI datepicker | CDN `code.jquery.com/ui/1.10.3/themes/smoothness` | `.ui-datepicker` |

Cubrir un solo juego de selectores dejaría el tema a medias: correcto en unas pantallas e
inconsistente en otras, según qué versión cargue cada una.

## Decisiones tomadas

**El datepicker se sobrescribe, no se reemplaza.** Se mantiene el `<link>` al CDN
`smoothness` y se redefine el calendario desde `dos-tema.css`. Es la misma regla que ya
sigue el tema —capa encima, sin tocar el `<head>`— y es revertible borrando un bloque. El
costo es algún `!important`, porque `smoothness` usa selectores muy específicos y en
varias páginas llega después en el orden de carga.

La alternativa (quitar el `<link>` y estilar el widget desde cero) da CSS más limpio y una
petición externa menos, pero cualquier parte del widget que se olvide queda sin estilo
alguno, no sólo desentonada. No compensa para un tema que hasta ahora no ha roto nada.

**SweetAlert conserva forma, alinea color.** Los iconos mantienen su geometría y su
animación —el check que se dibuja, la X, el signo de admiración— y toman los colores del
tema en lugar de los suyos (`#A5DC86`, `#F27474`, `#F8BB86` → `--dos-verde`,
`--dos-rojo-hondo`, `--dos-ambar`). Caja, tipografía y botones pasan al tema. El diálogo
sigue siendo el de siempre para quien lo usa a diario; sólo deja de desentonar.

## Diseño

### Dónde vive

Un bloque nuevo al final de `ReporteTareas/css/dos-tema.css`, sección **11 — Componentes
de terceros**. Se respetan las mismas reglas que el resto del archivo:

- sólo apariencia: color, tipografía, borde, espaciado y estados
- nada de `position`, `display`, `float` ni `width` que altere el layout de un widget
- ningún cambio de markup en ninguna de las 74 pantallas
- revertible borrando el bloque

`dos-tema.css` ya se carga último en `Master.Master`. Para select2 v4 y el datepicker se
usará `!important` puntual, sólo donde el CSS del CDN llegue después.

### Qué cubre

**1. select2 (v3 y v4).** Objetivo: que un desplegable sea indistinguible de un
`.form-control` ya tematizado. Se replican exactamente los valores que el tema ya define
para inputs:

- alto `34px`, borde `1px solid #CFCCC8`, radio `2px`, tipografía `13px`
- foco: borde `--dos-bermellon` y halo `0 0 0 3px rgba(229, 50, 18, .16)`
- panel desplegado: la opción bajo el cursor pasa del azul de select2 a `--dos-niebla`;
  la opción ya seleccionada, a `--dos-bermellon` con texto blanco
- estado deshabilitado igualado a `.form-control[disabled]` (`#F4F2F0` / `--dos-tenue`)

**2. Datepicker de jQuery UI.**

- cabecera del mes en `--dos-grafito`, sin el degradado de `smoothness`
- día de hoy en `--dos-bermellon`; día seleccionado en `--dos-grafito` con texto blanco
- números en `--dos-dato` con `font-variant-numeric: tabular-nums`, para que las columnas
  del calendario queden a plomo
- flechas de mes en `--dos-acero`, en bermellón al pasar el cursor

**3. SweetAlert (v1 y 2).** Caja con el mismo tratamiento que `.modal-content` (radio
`3px`, sombra `0 18px 44px rgba(0,0,0,.22)`), título en `--dos-titulo`, botones heredando
`.btn-primary` y `.btn-default` del tema, e iconos recoloreados según la decisión de
arriba.

**4. `.dataTables_wrapper`.** Ya que existe a mano en 34 páginas, se le da lo único que le
falta y que sí se nota: `overflow-x: auto` consistente, para que una tabla ancha desplace
dentro de su contenedor en lugar de romper el ancho de la página.

### Verificación

No es viable revisar 74 pantallas a ojo. La comprobación es:

1. compilar con MSBuild y confirmar EXIT 0
2. abrir en el navegador tres pantallas representativas y comparar antes/después:
   - una con select2 **v3** (el que carga `Master.Master`, afecta a todas)
   - una con select2 **v4** (CDN, páginas puntuales)
   - una con datepicker **y** SweetAlert en la misma pantalla

Las tres capturas quedan como evidencia antes de commitear.

## Fuera de alcance

Anotado, no se toca en este trabajo:

- **Librerías duplicadas.** Unificar select2 en una versión y SweetAlert en una es
  limpieza de dependencias, con riesgo de romper JS existente. No es look-and-feel.
- **Dependencia de `code.jquery.com`.** `Master.Master` carga jQuery, jQuery UI y el tema
  `smoothness` desde ese CDN. Si la red interna pierde salida a internet no es que el
  calendario se vea mal: la aplicación entera deja de funcionar, porque jQuery no carga.
  Hay copia local en `bower_components/assets/`. Merece su propio trabajo.
- **`Master.Master` suelto en la raíz del repositorio** (46 KB, 5/ago, sin trackear).
  Parece respaldo. Confirmar y borrar.
- **`?v=8` sin commitear** en `Master.Master`, pendiente del trabajo de marcación. Ver
  la memoria `estado-marcacion-correo-trazabilidad`.

## Riesgos

El único riesgo real es que un `!important` tape algo que una pantalla necesitaba. Se
acota manteniendo los `!important` en propiedades puramente visuales (`background`,
`color`, `border-color`) y nunca en las de layout. Si una pantalla se ve mal, el remedio
es borrar el bloque 11: el resto del tema no depende de él.
