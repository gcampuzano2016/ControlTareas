/* ==========================================================================
   Curva de Aprendizaje - utilidades compartidas por las pantallas del modulo.

   Todo texto que viene de la base pasa por CA.esc() antes de entrar al DOM:
   nombres, respuestas y planes de accion los escribe gente, y .html() con
   eso crudo es una inyeccion esperando su turno.
   ========================================================================== */
var CA = (function () {
    "use strict";

    var BADGES = {
        en_curso: ["En curso", "amarillo"], pendiente: ["Pendiente", "amarillo"], en_progreso: ["En progreso", "amarillo"],
        atencion: ["Dar seguimiento", "amarillo"], aprobado: ["Aprobado", "verde"], completada: ["Completada", "verde"],
        completado: ["Completado", "verde"], realizado: ["Realizado", "verde"], ok: ["En buen curso", "verde"],
        activo: ["Activo", "verde"], rechazado: ["Rechazado", "rojo"], no_realizado: ["No realizado", "rojo"],
        seguimiento: ["Alerta: plan de acción", "rojo"], extendido: ["Extendido", "purpura"],
        sin_revisar: ["Sin revisar", "gris"], inactivo: ["Inactivo", "gris"]
    };
    var MESES = ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];
    var PALETA_AVATAR = ["#f7902b", "#0f4560", "#026666", "#6d33a0"];

    function esc(s) {
        if (s === null || s === undefined) { return ""; }
        return String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
                        .replace(/"/g, "&quot;").replace(/'/g, "&#39;");
    }

    function badge(clave) {
        var v = BADGES[clave] || [clave || "—", "gris"];
        return '<span class="ca-badge ca-badge-' + v[1] + '">' + esc(v[0]) + "</span>";
    }

    /* "2025-03-01" -> "1 mar 2025" (sin pasar por Date: nada de zonas horarias). */
    function fechaCorta(iso) {
        if (!iso) { return "—"; }
        var p = String(iso).substr(0, 10).split("-");
        if (p.length !== 3) { return esc(iso); }
        return parseInt(p[2], 10) + " " + MESES[parseInt(p[1], 10) - 1] + " " + p[0];
    }

    function fechaIso(iso) { return iso ? String(iso).substr(0, 10) : "—"; }

    /* Color deterministico: el mismo nombre siempre da el mismo color. */
    function colorAvatar(nombre) {
        var h = 0, s = nombre || "";
        for (var i = 0; i < s.length; i++) { h = (h * 31 + s.charCodeAt(i)) % 997; }
        return PALETA_AVATAR[h % 4];
    }

    function rolTexto(rol) { return rol === "jefe" ? "Jefe" : "Colaborador"; }

    function parametro(nombre) {
        var m = new RegExp("[?&]" + nombre + "=([^&#]*)").exec(window.location.search);
        return m ? decodeURIComponent(m[1].replace(/\+/g, " ")) : null;
    }

    function mensaje(texto, tipo) {
        var color = "#fcf8e3";
        if (tipo === "success") { color = "#dff0d8"; }
        if (tipo === "danger") { color = "#f2dede"; }
        $("#modalMensajeInformativoTipo").css("background", color);
        $("#MensajeInformativo").text(texto);
        $("#modalMensajeInformativo").modal("show");
    }

    /* Llamada al handler, con el mismo sobre que el resto del sistema:
       [{ action, parameters }] -> { estado, resultado, mensaje, tipoMensaje }. */
    function post(url, action, parameters, ok, error, silencioso) {
        $.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify([{ action: action, parameters: parameters || {} }]),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (r) {
                if (r.estado === "1") {
                    if (ok) { ok(r.resultado, r); }
                } else {
                    if (!silencioso) { (CA.alMensaje || mensaje)(r.mensaje || "Ocurrió un error.", r.tipoMensaje || "danger"); }
                    if (error) { error(r); }
                }
            },
            error: function () {
                if (!silencioso) { (CA.alMensaje || mensaje)("No se pudo contactar al servidor. Intente nuevamente.", "danger"); }
                if (error) { error(null); }
            }
        });
    }

    function api(action, parameters, ok, error, silencioso) {
        post("AdministrarCurvaAprendizaje.ashx", action, parameters, ok, error, silencioso);
    }

    /* Deshabilita un boton durante una llamada lenta (la IA puede tardar). */
    function ocupado(boton, texto) {
        var $b = $(boton);
        if (texto) {
            $b.data("ca-texto", $b.text()).prop("disabled", true).text(texto);
        } else {
            $b.prop("disabled", false).text($b.data("ca-texto") || $b.text());
        }
    }

    var ICONO_CALENDARIO = '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><rect x="3" y="4" width="18" height="18" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/></svg>';

    return {
        esc: esc, badge: badge, fechaCorta: fechaCorta, fechaIso: fechaIso, colorAvatar: colorAvatar,
        rolTexto: rolTexto, parametro: parametro, mensaje: mensaje, post: post, api: api, ocupado: ocupado,
        ICONO_CALENDARIO: ICONO_CALENDARIO, alMensaje: null
    };
})();
