// menuBuscador.js — filtra el menú lateral y marca el ítem de la página actual.
// No depende de jQuery. No toca datos ni seguridad; solo presentación cliente.
(function () {
    "use strict";

    function normaliza(txt) {
        // NFD + quitar marcas diacríticas (acentos) para búsqueda sin acento-sensibilidad
        return (txt || "").toLowerCase()
            .normalize("NFD").replace(/[̀-ͯ]/g, "");
    }

    function filtrar(termino) {
        var menu = document.getElementById("side-menu");
        if (!menu) return;
        var q = normaliza(termino);
        var padres = menu.querySelectorAll(":scope > li");
        padres.forEach(function (padre) {
            var enlaces = padre.querySelectorAll(".nav-second-level > li");
            var algunoVisible = false;
            enlaces.forEach(function (li) {
                var texto = normaliza(li.textContent);
                var coincide = q === "" || texto.indexOf(q) !== -1;
                li.classList.toggle("dos-menu-hidden", !coincide);
                if (coincide) algunoVisible = true;
            });
            // Si no hay hijos, evalúa el propio padre
            var textoPadre = normaliza(padre.textContent);
            var mostrarPadre = q === "" || algunoVisible || textoPadre.indexOf(q) !== -1;
            padre.classList.toggle("dos-menu-hidden", !mostrarPadre);
            if (q !== "" && algunoVisible) {
                var subUl = padre.querySelector(".nav-second-level");
                if (subUl) subUl.classList.add("in");
            }
        });
    }

    function marcarActivo() {
        var menu = document.getElementById("side-menu");
        if (!menu) return;
        var actual = window.location.pathname.split("/").pop().toLowerCase();
        if (!actual) return;
        var enlaces = menu.querySelectorAll("a[href]");
        enlaces.forEach(function (a) {
            var href = (a.getAttribute("href") || "").split("/").pop().toLowerCase();
            if (href && href === actual) {
                var li = a.closest("li");
                if (li) li.classList.add("active");
                var padreUl = a.closest(".nav-second-level");
                if (padreUl) {
                    padreUl.classList.add("in");
                    var liPadre = padreUl.closest("li");
                    if (liPadre) liPadre.classList.add("active");
                }
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        var input = document.getElementById("dos-menu-filtro");
        if (input) {
            input.addEventListener("input", function () { filtrar(input.value); });
        }
        marcarActivo();
    });
})();
