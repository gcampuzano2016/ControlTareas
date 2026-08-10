/* ============================================================================
   Navegación — Sistema de Tareas DOS
   ----------------------------------------------------------------------------
   Tres cosas, todas sobre el menu que ya genera Master.Master.cs. No cambia el
   markup del servidor ni depende de jQuery.

   1. Hamburguesa de escritorio, con la eleccion recordada.
   2. Nombre de la pantalla actual en la barra superior. Hace falta sobre todo
      cuando el menu esta escondido: sin el, no queda ninguna pista de donde
      esta parado el usuario.
   3. Filtro rapido del menu. Hay 48 pantallas repartidas en varios grupos, y
      sin el hay que abrir grupo por grupo para encontrar una.

   Quien marca la pantalla actual es sb-admin-2.js, que ya pone .active en el
   enlace y abre su grupo. Aqui solo se lee ese trabajo, no se repite.
   ============================================================================ */

(function () {
    "use strict";

    var CLAVE = "dosMenuOculto";
    var raiz = document.documentElement;

    /* ---------------------------------------------------------------- utiles */

    function texto(nodo) {
        return (nodo.textContent || nodo.innerText || "").replace(/\s+/g, " ").trim();
    }

    /* Compara sin distinguir mayusculas ni acentos: quien busca "administracion"
       debe encontrar "Administración". */
    function normalizar(cadena) {
        cadena = cadena.toLowerCase();
        if (cadena.normalize) {
            /* El rango va escapado a proposito. Escrito con los caracteres
               combinantes literales, este archivo dependia de que el navegador
               acertara la codificacion: produccion lo sirve como
               application/javascript SIN charset=utf-8 y sin BOM. Con \u escapes
               el archivo es ASCII puro y da igual como se interprete. */
            cadena = cadena.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
        }
        return cadena;
    }

    /* ------------------------------------------------------------ hamburguesa */

    function estaOculto() {
        return raiz.className.indexOf("menu-oculto") !== -1;
    }

    function aplicarOculto(oculto) {
        /* Sin classList.toggle(clase, fuerza): IE11 ignora el segundo argumento. */
        if (oculto) {
            if (!estaOculto()) { raiz.className += " menu-oculto"; }
        } else {
            raiz.className = raiz.className.replace(/\s*menu-oculto\b/g, "");
        }

        var boton = document.getElementById("dosMenuBoton");
        if (boton) {
            /* aria-expanded describe el menu, no el boton: expandido = visible. */
            boton.setAttribute("aria-expanded", oculto ? "false" : "true");
            boton.setAttribute("title", oculto ? "Mostrar el menú" : "Ocultar el menú");
        }
    }

    function recordar(oculto) {
        try {
            window.localStorage.setItem(CLAVE, oculto ? "1" : "0");
        } catch (e) {
            /* Modo privado o almacenamiento bloqueado: el boton sigue sirviendo,
               solo que la eleccion no sobrevive a la recarga. */
        }
    }

    function iniciarHamburguesa() {
        var boton = document.getElementById("dosMenuBoton");
        if (!boton) { return; }

        aplicarOculto(estaOculto());

        boton.onclick = function () {
            var oculto = !estaOculto();
            aplicarOculto(oculto);
            recordar(oculto);
            return false;
        };
    }

    /* ------------------------------------------------- pantalla actual arriba */

    function iniciarTitulo() {
        var menu = document.getElementById("side-menu");
        var cabecera = document.querySelector(".navbar-header");
        if (!menu || !cabecera) { return; }

        var activo = menu.querySelector("a.active");
        if (!activo) { return; }

        var nombre = texto(activo);
        if (!nombre) { return; }

        /* El grupo padre da el contexto: "Manejo de perfiles / Usuarios". */
        var grupo = "";
        var contenedor = activo.parentNode;
        while (contenedor && contenedor !== menu) {
            if (contenedor.className && contenedor.className.indexOf("nav-second-level") !== -1) {
                var padre = contenedor.parentNode.querySelector("a");
                if (padre) { grupo = texto(padre); }
                break;
            }
            contenedor = contenedor.parentNode;
        }

        var caja = document.createElement("span");
        caja.className = "dos-ubicacion";
        if (grupo) {
            caja.innerHTML = "<span class='dos-ubicacion-grupo'></span>" +
                             "<span class='dos-ubicacion-sep'>/</span>" +
                             "<span class='dos-ubicacion-hoja'></span>";
            caja.firstChild.appendChild(document.createTextNode(grupo));
            caja.lastChild.appendChild(document.createTextNode(nombre));
        } else {
            caja.appendChild(document.createTextNode(nombre));
        }
        cabecera.appendChild(caja);
    }

    /* -------------------------------------------------------- filtro del menu */

    function iniciarFiltro() {
        var menu = document.getElementById("side-menu");
        if (!menu || !menu.parentNode) { return; }

        var grupos = menu.children;
        if (grupos.length < 4) { return; }   /* con pocos items no aporta nada */

        var caja = document.createElement("div");
        caja.className = "dos-filtro";
        caja.innerHTML =
            "<label class='sr-only' for='dosFiltroMenu'>Buscar en el menú</label>" +
            "<input type='text' id='dosFiltroMenu' autocomplete='off' placeholder='Buscar pantalla...'>" +
            "<span class='dos-filtro-vacio'>Ninguna pantalla coincide</span>";
        menu.parentNode.insertBefore(caja, menu);

        var entrada = document.getElementById("dosFiltroMenu");
        var aviso = caja.querySelector(".dos-filtro-vacio");

        /* Estado original, para poder devolver el menu como estaba al limpiar. */
        var abiertosAlInicio = [];
        var i;
        for (i = 0; i < grupos.length; i++) {
            var sub = grupos[i].querySelector("ul");
            abiertosAlInicio.push(sub && sub.className.indexOf("in") !== -1);
        }

        function mostrar(elemento, visible) {
            elemento.style.display = visible ? "" : "none";
        }

        function abrir(sub, abierto) {
            if (!sub) { return; }
            if (abierto) {
                if (sub.className.indexOf("in") === -1) { sub.className += " in"; }
            } else {
                sub.className = sub.className.replace(/\s*\bin\b/g, "");
            }
        }

        function filtrar() {
            var busqueda = normalizar(entrada.value.trim());
            var hallazgos = 0;
            var g, sub, hijos, j, coincideGrupo, coincidenHijos;

            for (g = 0; g < grupos.length; g++) {
                sub = grupos[g].querySelector("ul");
                hijos = sub ? sub.children : [];
                coincideGrupo = false;
                coincidenHijos = 0;

                if (busqueda === "") {
                    mostrar(grupos[g], true);
                    for (j = 0; j < hijos.length; j++) { mostrar(hijos[j], true); }
                    abrir(sub, abiertosAlInicio[g]);
                    continue;
                }

                var enlaceGrupo = grupos[g].querySelector("a");
                if (enlaceGrupo && normalizar(texto(enlaceGrupo)).indexOf(busqueda) !== -1) {
                    coincideGrupo = true;
                }

                for (j = 0; j < hijos.length; j++) {
                    var visible = coincideGrupo ||
                        normalizar(texto(hijos[j])).indexOf(busqueda) !== -1;
                    mostrar(hijos[j], visible);
                    if (visible) { coincidenHijos++; }
                }

                var vale = coincideGrupo || coincidenHijos > 0;
                mostrar(grupos[g], vale);
                abrir(sub, vale);          /* si coincide, se abre para verlo */
                if (vale) { hallazgos++; }
            }

            aviso.style.display = (busqueda !== "" && hallazgos === 0) ? "block" : "none";
        }

        entrada.oninput = filtrar;
        entrada.onkeydown = function (evento) {
            if (evento.keyCode === 27) {   /* Escape limpia y devuelve el foco */
                entrada.value = "";
                filtrar();
            }
        };
    }

    /* ------------------------------------------------------------------ arranque */

    /* El arranque va en DOS tiempos, y la diferencia importa.

       La hamburguesa y el filtro solo necesitan que exista el DOM. Antes
       esperaban a "load", que no dispara hasta que termino de bajar TODO:
       el carrusel de la pagina de inicio y los scripts de code.jquery.com.
       Mientras tanto el boton no respondia, que es como se reporto el fallo
       desde produccion. Ahora arrancan apenas el documento esta listo.

       Solo el nombre de la pantalla espera a "load": necesita leer el .active
       que sb-admin-2.js pone dentro de su propio $(function), y ese depende de
       jQuery. Si tarda, lo unico que llega tarde es el rotulo de arriba. */

    function iniciarInmediato() {
        iniciarHamburguesa();
        iniciarFiltro();
    }

    function iniciarTrasCarga() {
        iniciarTitulo();
    }

    function cuandoElDomEsteListo(fn) {
        if (document.readyState === "interactive" || document.readyState === "complete") {
            fn();
        } else {
            document.addEventListener("DOMContentLoaded", fn);
        }
    }

    cuandoElDomEsteListo(iniciarInmediato);

    if (document.readyState === "complete") {
        iniciarTrasCarga();
    } else {
        window.addEventListener("load", iniciarTrasCarga);
    }
})();
