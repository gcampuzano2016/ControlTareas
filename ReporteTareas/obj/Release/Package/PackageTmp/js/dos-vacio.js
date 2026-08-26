/* ============================================================================
   Estado vacio
   ============================================================================
   Lo que se pinta cuando una consulta no trae filas. Antes era una cadena
   suelta inyectada en el contenedor; ese es justo el momento en que alguien
   piensa que la aplicacion se rompio.

   Vive aca y no en cada pantalla porque CargarPagina esta duplicada en 50
   archivos y DetalleTareasDescargaXLS en 30. Copiar el marcado en cada sitio
   repetiria el error que produjo esas duplicaciones: cambiar la redaccion
   serian cien ediciones en vez de una.

   Master.Master lo carga para las 75 pantallas. Las seis que no usan la master
   (Login, PaginaError, Plantilla, PrubaWebServices, ResetPassword,
   RespuestaAprobacion) son auxiliares y no tienen tablas.
   ========================================================================== */

/* Escapa el texto antes de meterlo en el marcado. Los mensajes pueden venir
   del servidor, asi que no se concatenan crudos. */
function DosEscapar(t) {
    return String(t)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");
}

/* Devuelve el marcado del bloque.

   Los dos argumentos son opcionales. CargarPagina no sabe que esta cargando
   —recibe un selector y una URL, no un concepto— asi que el texto por defecto
   es generico a proposito. Una pantalla concreta puede afinarlo pasando los
   suyos, sin tocar el patron.

   Pasar "" como pista la omite del marcado. */
function DosVacio(mensaje, pista) {
    var m = mensaje || "No hay resultados para lo que buscaste.";
    var p = arguments.length > 1
        ? pista
        : "Prueba con otro rango de fechas o cambia los filtros.";

    var html = '<div class="dos-vacio">' +
               '<i class="fa fa-inbox dos-vacio__icono" aria-hidden="true"></i>' +
               '<p class="dos-vacio__mensaje">' + DosEscapar(m) + '</p>';

    if (p) {
        html += '<p class="dos-vacio__pista">' + DosEscapar(p) + '</p>';
    }

    return html + '</div>';
}

/* Version de una sola linea, para las listas desplegables de busqueda.

   El bloque de arriba no sirve ahi: un desplegable de typeahead mide unos
   pocos pixeles de alto y sus hijos tienen que ser <li>. Devuelve un <li> sin
   enlace, asi que no es clicable ni entra en la navegacion por teclado. */
function DosVacioLinea(mensaje) {
    return '<li class="dos-vacio-linea">' + DosEscapar(mensaje) + '</li>';
}

/* El texto del dialogo de "descargar Excel" cuando no hay nada que exportar.
   Es un dialogo y no un estado vacio: responde a una accion explicita del
   usuario. Por eso conserva su forma; lo que cambia es que hable de la
   descarga y no de una consulta. */
function DosTextoDescargaVacia() {
    return "No hay datos que descargar con los filtros elegidos.";
}
