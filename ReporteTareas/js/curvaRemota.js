/* Curva de Aprendizaje - evaluacion remota por enlace (CurvaEvaluacionRemota.aspx).
   Pagina anonima: el token del enlace es la unica credencial. */
$(function () {
    "use strict";
    var token = CA.parametro("token") || "";
    var URL = "CurvaEvaluacionRemota.ashx";

    // Sin bootstrap en esta pagina: los mensajes van a un recuadro propio.
    CA.alMensaje = function (texto) { $("#caError").text(texto).show(); };

    function estado(titulo, texto) {
        $("#caFormulario, #caBotonera, #caError").hide();
        $("#caTitulo").text(titulo);
        $("#caSubtitulo").text("");
        $("#caEstado").html("<p>" + CA.esc(texto) + "</p>").show();
    }

    function situacion(r) {
        if (r.situacion === "incompleto") { estado("Enlace incompleto", "Este enlace no incluye la información necesaria. Pide que te reenvíen el link completo."); return true; }
        if (r.situacion === "invalido") { estado("Enlace inválido o expirado", "Este enlace ya no está disponible. Pide a Talento Humano que te genere uno nuevo."); return true; }
        if (r.situacion === "completada") { estado("¡Gracias!", "Esta evaluación ya fue completada. No es necesario hacer nada más."); return true; }
        if (r.situacion === "enviada") { estado("¡Gracias por completar tu evaluación!", "Tus respuestas quedaron registradas."); return true; }
        return false;
    }

    CA.post(URL, "cargar", { token: token }, function (r) {
        if (situacion(r)) { return; }
        var d = r.formulario;
        var jefe = d.Rol === "jefe";
        $("#caEncabezado").addClass(jefe ? "ca-jefe" : "ca-colaborador");
        $("#caTitulo").text((jefe ? "Evaluación del jefe / evaluador" : "Autoevaluación") + " — Etapa " + d.EtapaCodigo + " días");
        $("#caSubtitulo").text((jefe ? "Estás evaluando a: " : "Tu autoevaluación: ") + d.EmpleadoNombre + (d.EmpleadoCargo ? " · " + d.EmpleadoCargo : ""));
        CAForm.pintar($("#caFormulario"), d);
        $("#caBotonera").show();
    }, function (r) {
        estado("Ocurrió un problema", r && r.mensaje ? r.mensaje : "No se pudo cargar la evaluación. Intenta nuevamente.");
    }, true);

    $("#caEnviar").on("click", function () {
        $("#caError").hide();
        var problemas = CAForm.validar($("#caFormulario"));
        if (problemas.length) { CA.alMensaje(problemas.join(" ")); return; }
        var boton = this;
        CA.ocupado(boton, "Enviando…");
        CA.post(URL, "enviar", { token: token, payload: CAForm.recolectar($("#caFormulario")) }, function (r) {
            situacion(r);
            window.scrollTo(0, 0);
        }, function () { CA.ocupado(boton); });
    });
});
