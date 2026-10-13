# Rótulos completos y estado vacío — diseño

Fecha: 2026-08-12
Estado: aprobado para plan

## Por qué

Dos deudas de interfaz que una hoja de estilos no puede alcanzar, porque viven en
el texto y en el marcado que genera el JS.

**Los rótulos abrevian donde ya no hace falta.** `Fch. Registro`, `Nom. Cliente`,
`Id. Responsable` son de cuando el ancho de columna se pagaba caro. Hoy solo
obligan a traducir mentalmente en cada lectura.

**El estado vacío es texto pelado.** Cuando una consulta no trae filas, el JS
inyecta una cadena suelta dentro del contenedor de la tabla. Ese es justo el
momento en que alguien piensa que la aplicación se rompió.

## Alcance medido

No es un problema transversal: está concentrado y es contable.

### Rótulos — 43 ocurrencias en 6 pantallas

| Pantalla | Ocurrencias |
|---|---|
| `Tareas.aspx` | 17 |
| `AprobarHorasExtras.aspx` | 15 |
| `ReporteHorasExtras.aspx` | 4 |
| `TiempoTarea.aspx` | 4 |
| `CreaTarea.aspx` | 1 |
| `CreaTareas.aspx` | 1 |
| `RegistroInventario.aspx` | 1 |

### Estado vacío — 101 sitios, dos grupos distintos

La cadena `"No existen datos para esta consulta."` aparece 101 veces, pero **no
significa lo mismo en los dos grupos**. La clasificación se hizo mirando qué
función contiene cada uso:

| Grupo | Sitios | Archivos | Función contenedora | Qué es |
|---|---|---|---|---|
| A | 70 | 49 | `CargarPagina` y variantes | estado vacío real, en el contenedor |
| B | 31 | 30 | `DetalleTareasDescargaXLS` | descarga de Excel sin datos que exportar |

Los 31 del grupo B son **todos la misma función copiada**. No son un estado
vacío: son la respuesta a un clic en "descargar". Por eso conservan su diálogo.

## Decisiones

### Los colores no se tocan

Restricción del usuario. Ni se introducen valores nuevos ni se cambian los
existentes. El estado vacío usa solo tokens que ya están en `dos-tema.css`
(`--crm-tinta-media`, `--crm-linea`). La cabecera del diálogo de descarga
conserva su `#f2dede`.

Lo único que cambia visualmente: el texto suelto del contenedor pasa a ser un
bloque con aire y jerarquía, y los rótulos se leen completos.

### Grupo B conserva el diálogo

Se consideró convertirlo en un mensaje no bloqueante y se descartó: es la
respuesta a una acción explícita del usuario, y ahí un diálogo es la forma
correcta. Lo que se corrige es el texto, que habla de "consulta" cuando el
usuario pidió una descarga.

### Un helper global, no marcado repetido

`CargarPagina` está duplicada en 50 archivos y `DetalleTareasDescargaXLS` en 30.
Copiar el marcado del estado vacío en cada sitio repetiría exactamente el error
que produjo esas duplicaciones. El marcado y los textos van a un archivo único.

Se consideró y se descartó por ahora unificar `CargarPagina` en una sola función
compartida: las copias divergieron (unas reciben `idSeleccionado`, otras no), es
un trabajo con riesgo real y merece su propio ciclo. Queda anotado como pendiente.

## Diseño

### Archivo nuevo: `js/dos-vacio.js`

Cargado desde `Master.Master`, junto a los demás scripts del pie. Las seis
páginas que no usan la master (`Login`, `PaginaError`, `Plantilla`,
`PrubaWebServices`, `ResetPassword`, `RespuestaAprobacion`) son auxiliares y no
tienen tablas, así que no lo necesitan.

Expone dos funciones:

```js
DosVacio(mensaje, pista)      // devuelve el marcado del estado vacío
DosTextoDescargaVacia()       // devuelve el texto del diálogo de descarga
```

Ambos argumentos de `DosVacio` son opcionales. `CargarPagina` no sabe qué está
cargando —recibe un selector y una URL, no un concepto— así que el texto por
defecto es genérico a propósito. Una pantalla concreta puede afinarlo después
pasando sus propios textos, sin tocar el patrón.

### El marcado del estado vacío

```html
<div class="dos-vacio">
    <i class="fa fa-inbox dos-vacio__icono" aria-hidden="true"></i>
    <p class="dos-vacio__mensaje">No hay resultados para lo que buscaste.</p>
    <p class="dos-vacio__pista">Prueba con otro rango de fechas o cambia los filtros.</p>
</div>
```

Font Awesome ya lo carga `Master.Master`, así que el ícono no agrega dependencia.
La pista se omite del marcado si se pasa vacía.

### El CSS

Va al final de `dos-tema.css`, en su propio bloque comentado, siguiendo la
convención del archivo. Centrado, con aire generoso, ícono grande y tenue en
`--crm-linea`, mensaje en `--crm-tinta-media`, pista un paso más pequeña. Sin
borde ni fondo: el contenedor ya es una tarjeta blanca.

Se sube `dos-tema.css` a `?v=10` en `Master.Master` y en `Login.aspx`. Sin eso el
navegador sirve la hoja vieja y el bloque sale sin estilo.

### Los textos

| Dónde | Hoy | Propuesto |
|---|---|---|
| Grupo A, mensaje | No existen datos para esta consulta. | No hay resultados para lo que buscaste. |
| Grupo A, pista | — | Prueba con otro rango de fechas o cambia los filtros. |
| Grupo B, diálogo | No existen datos para esta consulta. | No hay datos que descargar con los filtros elegidos. |

### Los rótulos

| Hoy | Propuesto |
|---|---|
| `Nom. Cliente` | Cliente |
| `Nom. Responsable` | Responsable |
| `Id. Responsable` | Código del responsable |
| `Fch. Registro` | Fecha de registro |
| `Fch. Tarea` | Fecha de la tarea |
| `Fch. Inicio` | Inicio |
| `Fch. Fin` | Fin |
| `Fch. Est. Solucion` | Solución estimada |
| `Fch. Est. Atencion` | Atención estimada |
| `N° OS` | N° de orden de servicio |
| `Num. OS` | N° de orden de servicio |
| `N° Orden de Servicio` | N° de orden de servicio |
| `Cod. Ticket` | N° de ticket |
| `N° Ticket` | N° de ticket |
| `N° PARTE` | N° de parte |

Se quita el prefijo cuando el contexto ya lo explica: en la tabla de tareas,
`Nom. Cliente` es simplemente **Cliente**. `N° Orden de Servicio` se normaliza
junto con sus abreviaturas para que la misma columna se llame igual en las tres
pantallas donde aparece.

## Verificación

Sin framework de pruebas en el proyecto. La verificación es una página de
reproducción servida localmente, como la que se usó para la espera de las
consultas y para el arreglo de `HistoriaClinica`:

1. `DosVacio()` sin argumentos devuelve el marcado con los textos por defecto.
2. `DosVacio("otro mensaje")` respeta el mensaje y omite la pista.
3. El bloque se ve centrado y con la jerarquía correcta dentro de un `.panel`.
4. Ninguna regla nueva introduce un color que no esté ya en `dos-tema.css`
   (revisión del diff: cero literales de color).
5. En las 6 pantallas de rótulos, ninguna cadena abreviada sobrevive.
6. Recuento posterior: cero apariciones de la cadena vieja en los grupos A y B.

## Fuera de alcance

- Unificar `CargarPagina` en una función compartida.
- Los otros textos vacíos del sistema (`"No existen archivos adjuntos."` ×29,
  `"No hay información"` ×20). Mismo patrón aplicable después.
- Las otras 3711 declaraciones `style=` inline.

## Deuda encontrada de paso, no se toca

- `js/DetallesContrato - copia.js` aparece en los dos grupos. Es una copia con
  espacio y "copia" en el nombre; conviene confirmar si alguna pantalla la carga
  antes de editarla o borrarla.
- La variable se llama `mesnajeError` (error de tipeo por `mensajeError`) en
  decenas de archivos. No es visible para el usuario; renombrarla no aporta.
