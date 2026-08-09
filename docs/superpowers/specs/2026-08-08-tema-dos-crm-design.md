# Tema DOS — dirección CRM

Fecha: 2026-08-08
Rama: `ProyectoNuevosCambios`
Implementado en: `1a3d7db`
Continúa: `fb3d175` (tema DOS sobre SB Admin 2) … `e778c73` (ubicación actual y filtro del menú)

> Este documento reemplaza a la versión del 2026-08-08 titulada «componentes de
> terceros», que proponía sólo tematizar select2, el datepicker y SweetAlert. Ese
> alcance se descartó: no resolvía el problema real. Lo que sigue es lo que
> efectivamente se construyó.

## Problema

El tema anterior cubría todo el markup propio de la aplicación —barras, paneles,
botones, formularios, tablas y estados— y aun así la aplicación no se leía como un
sistema moderno. La causa no era que faltara cubrir componentes: era la dirección.

El tema estaba construido sobre densidad industrial: radio de 2px, sin sombras,
hairlines, cuerpo de 13.5px, cebra desactivada. Es un vocabulario de terminal, y es
exactamente el opuesto del que usa el software de gestión que la gente reconoce. El
CSS estaba haciendo lo contrario de lo que se le pedía.

## El sujeto

ReporteTareas no es un CRM de ventas. Es un CRM de personas y su tiempo: marcación de
jornada, tareas diarias, aprobación de horas extras, historia clínica y contratos de
RRHH. El registro que maneja no es una oportunidad de negocio, es una jornada de
trabajo. Esa es la fuente de las decisiones que siguen.

## Terreno verificado

El reconocimiento corrigió dos supuestos de partida.

### DataTables no se carga

Ninguna página ni `Master.Master` incluye el script del plugin. Las 44 llamadas
`.DataTable()` repartidas por `js/` no tienen plugin detrás. La clase
`dataTables_wrapper` aparece en 34 páginas pero está escrita a mano en el markup como
`<div>` envoltorio:

```html
<div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
```

No existen buscador, paginación ni contador de DataTables que tematizar. Queda sólo el
envoltorio, al que se le da `overflow-x: auto`.

### Cada librería convive en dos versiones

| Librería | Cómo llega | Selectores cubiertos |
|---|---|---|
| select2 v3 | global, `Master.Master` → `bower_components/assets/js/select2.min.js` | `.select2-container`, `.select2-choice` |
| select2 v4 | CDN jsdelivr en páginas puntuales (4.0.13 y 4.1.0-rc.0) | `.select2-container--default` |
| SweetAlert 1 | global, `Master.Master` | `.sweet-alert` |
| SweetAlert 2 | 7 páginas | `.swal2-popup` |
| jQuery UI datepicker | CDN `code.jquery.com/ui/1.10.3/themes/smoothness` | `.ui-datepicker` |

Cubrir un solo juego dejaría el tema correcto en unas pantallas e inconsistente en
otras, sin patrón aparente para quien lo use.

## Diseño

### Restricciones que no cambian

- sólo apariencia: color, tipografía, borde, espaciado y estados
- ningún cambio de markup en ninguna de las 74 pantallas
- revertible quitando el `<link>` de `Master.Master`
- una única excepción de layout, documentada más abajo

### Tokens

**Color.** Fondo `#F4F2EF`, un gris *cálido*. La elección es deliberada: el gris
azulado es el reflejo automático de cualquier panel de administración, y sobre un frío
el bermellón de la marca se apaga. Superficies en blanco, tinta `#211E1C`, y
`#E53212` —muestreado del logo, no elegido a ojo— como único acento. El default de
este género es azul índigo sobre gris frío; el bermellón sobre neutro cálido es lo que
hace que se lea como DOS y no como una plantilla.

Estados con tinte de fondo y tinta del mismo tono: verde `#176B44`, ámbar `#8A5600`,
rojo `#A81D0E`.

**Tipografía.** Segoe UI para texto a 14px, Segoe UI Semibold para títulos con tracking
negativo, Consolas tabular para horas y cifras. Fuentes del sistema a propósito: la
aplicación corre en red interna y ya depende de un CDN para arrancar; no se le agrega
otra dependencia externa por una fuente. La personalidad se consigue con escala y peso,
no con familias exóticas.

**Elevación y forma.** Sombra en dos capas —una de contacto y una de distancia—, radio
de 12px en tarjetas y 7px en controles. Las tarjetas pierden el borde y se sostienen
sólo con la sombra.

**Ritmo.** Aire de 20px en tarjetas, filas de tabla de 48px, campos de 38px.

### La firma: el riel de diafragma

Un bloque bermellón de 3px que aparece en exactamente dos lugares y siempre significa
lo mismo: *esto es lo que estás mirando*. Va junto al título de cada tarjeta y en el
ítem de menú activo. Sale del diafragma del logo y ya existía en el ADN del tema
anterior.

Los estados **no** lo llevan: se resuelven con píldora tintada. La versión inicial les
daba píldora y riel a la vez; se quitó el riel para que codifique una sola cosa y no
compita consigo mismo.

### La excepción de layout

Bootstrap da `margin: -15px` a `.row` contando con que el contenedor tenga 15px de
padding que lo compense. Las pantallas traen `style="padding: 0px"` **inline** en
`#page-wrapper`, y ese inline gana. Resultado: cada pantalla desbordaba 15px a la
derecha y mostraba una barra de scroll horizontal que no lleva a ninguna parte.

Es un defecto preexistente, no introducido por el tema. Se neutraliza sólo en los
`.row` que cuelgan directamente del wrapper; los anidados dentro de un panel conservan
su comportamiento.

### Las flechas del datepicker

Las flechas de mes de jQuery UI son un sprite PNG que viaja con el tema `smoothness`
del CDN. Cuando ese sprite no está, jQuery UI deja el texto «Prev»/«Next» oculto con
`text-indent: -99999px` y **no queda forma de cambiar de mes**. El tema las dibuja con
los caracteres `‹` y `›`, así que funcionan con CDN o sin él.

## Verificación

Hecha en navegador sobre `ReporteTareas/dos-tema-demo.html`, una página que carga los
CSS y las librerías reales del proyecto y reproduce el markup real de
`ActualizarTareas.aspx`. No está en el `.csproj`: no se publica ni afecta el build.

**Comprobado funcionando:** barra superior con el par Entrada/Salida, menú lateral con
el riel en el ítem activo, tarjetas con su riel de título, tabla con filas altas y
hover, píldoras de estado en sus cuatro colores, paginación, botones en sus seis
variantes, pestañas, avisos tintados, select2 tomando el estilo de los campos, el
calendario con hoy en bermellón tenue y el día elegido en bermellón sólido, y los
diálogos de SweetAlert con el check verde y la X roja del tema, forma y animación
intactas.

**Defectos encontrados y corregidos durante la verificación:** las líneas divisorias
del menú (`sb-admin-2.css:137`), el desborde horizontal de 15px, y las flechas
invisibles del datepicker.

**Falsa alarma descartada:** en una captura ampliada los días del calendario parecían
tener dos colores. Medidos los 31, todos comparten `#57514B`; era compresión del JPEG.
No se cambió nada por eso.

**No verificado:** el tema se probó sobre una pantalla representativa, no sobre las 74.
Las pantallas con markup atípico —carrusel, dashboards de Morris, reportes— pueden
necesitar ajustes puntuales.

## Fuera de alcance

Anotado, no se tocó:

- **Librerías duplicadas.** Unificar select2 en una versión y SweetAlert en una es
  limpieza de dependencias, con riesgo de romper JS existente. No es look-and-feel.
- **Dependencia de `code.jquery.com`.** `Master.Master` carga jQuery, jQuery UI y el
  tema `smoothness` desde ese CDN. Si la red interna pierde salida a internet no es que
  el calendario se vea mal: la aplicación entera deja de funcionar, porque jQuery no
  carga. Hay copia local en `bower_components/assets/` y `js/`. Merece su propio
  trabajo.
- **Login.aspx.** No carga `dos-tema.css`. Es la primera pantalla que ve todo el mundo
  y sigue con SB Admin 2 puro.
- **El logo.** Se pidió reemplazarlo por la versión con el eslogan «tecnología con
  propósito». No existe en el proyecto: las cinco variantes presentes
  (`IcoDos.png`, `IconoDos.png`, `logo_dos.png`, `logo_ct.png`,
  `imagesCorreo/logo_dos_textoGris.png`) dicen «visión sin límites». Queda pendiente de
  que el archivo oficial esté disponible; los puntos a cambiar son `Master.Master:79`,
  `Login.aspx:41` y el favicon de ambas.
- **`Master.Master` suelto en la raíz del repositorio** (46 KB, 5/ago, sin trackear).
  Parece respaldo. Confirmar y borrar.
- **`?v=8` sin commitear** en `Master.Master`, pendiente del trabajo de marcación. Ver
  la memoria `estado-marcacion-correo-trazabilidad`.

## Riesgos

El riesgo vivo es que un `!important` de la sección de terceros tape algo que una
pantalla necesitaba. Se acota manteniéndolos en propiedades visuales (`background`,
`color`, `border-color`) y nunca en layout.

Si una pantalla se ve mal, el remedio es borrar la sección 11: el resto del tema no
depende de ella. Si el problema es más de fondo, quitar el `<link>` de `Master.Master`
revierte todo.
