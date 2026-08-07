/* ============================================================================
   Hamburguesa de escritorio — Sistema de Tareas DOS
   ----------------------------------------------------------------------------
   Muestra u oculta la barra lateral y recuerda la eleccion.

   Por que se recuerda: cada clic de esta aplicacion recarga la pagina completa
   (WebForms). Sin memoria, el menu se volveria a abrir en cada pantalla y el
   boton no serviria de nada.

   El parpadeo al cargar lo evita un script en linea dentro del <head> de
   Master.Master, que aplica la clase antes del primer pintado. Este archivo
   solo se encarga del boton.

   Se escribe sin depender de jQuery para que funcione aunque cambie la version.
   ============================================================================ */

(function () {
    "use strict";

    var CLAVE = "dosMenuOculto";
    var raiz = document.documentElement;

    function estaOculto() {
        return raiz.className.indexOf("menu-oculto") !== -1;
    }

    function aplicar(oculto) {
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
            /* Modo privado o almacenamiento bloqueado: el boton sigue
               funcionando, solo que la eleccion no sobrevive a la recarga. */
        }
    }

    function iniciar() {
        var boton = document.getElementById("dosMenuBoton");
        if (!boton) { return; }

        /* El <head> ya aplico la clase; aqui solo se sincronizan los atributos. */
        aplicar(estaOculto());

        boton.onclick = function () {
            var oculto = !estaOculto();
            aplicar(oculto);
            recordar(oculto);
            return false;
        };
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", iniciar);
    } else {
        iniciar();
    }
})();
