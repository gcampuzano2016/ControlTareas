/* Curva de Aprendizaje - completar o editar una evaluacion desde GTH
   (CurvaEvaluacion.aspx). El formulario lo pinta curvaFormulario.js. */
$(function () {
    "use strict";
    var id = parseInt(CA.parametro("id"), 10);
    var datos = null;

    if (!(id > 0)) { $("#caFormulario").html('<div class="ca-vacio">Evaluación no encontrada</div>'); return; }

    CA.api("cargarEvaluacion", { id: id }, function (d) {
        datos = d;
        var editar = d.Estado === "completada";
        var ficha = "CurvaColaboradorFicha.aspx?id=" + d.EmpleadoId;
        $("#caVolver, #caCancelar").attr("href", ficha);
        $("#caEncabezado").addClass(d.Rol === "jefe" ? "ca-jefe" : "ca-colaborador").show();
        $("#caKicker").text(d.EmpleadoNombre + (d.EmpleadoCargo ? " · " + d.EmpleadoCargo : ""));
        $("#caTitulo").text((editar ? "Editar evaluación" : "Evaluación") + " — Etapa " + d.EtapaCodigo + " días");
        $("#caSubtitulo").text(d.Rol === "jefe" ? "Evaluación del jefe / evaluador" : "Autoevaluación del colaborador");
        $("#caAvisoEdicion").toggle(editar);
        $("#caGuardar").text(editar ? "Guardar cambios" : "Guardar y completar evaluación");
        CAForm.pintar($("#caFormulario"), d);
        $("#caBotonera, #caNotaIA").show();
    }, function (r) {
        $("#caFormulario").html('<div class="ca-vacio">' + CA.esc(r && r.mensaje ? r.mensaje : "No se pudo cargar la evaluación.") + "</div>");
    }, true);

    $("#caGuardar").on("click", function () {
        var problemas = CAForm.validar($("#caFormulario"));
        if (problemas.length) { CA.mensaje(problemas.join(" "), "warning"); return; }
        var boton = this;
        var editar = datos.Estado === "completada";
        CA.ocupado(boton, "Guardando…");   // evita el doble envio mientras la IA redacta el resumen
        CA.api(editar ? "editar" : "completar", { id: id, payload: CAForm.recolectar($("#caFormulario")) }, function (r) {
            window.location.href = "CurvaColaboradorFicha.aspx?id=" + r.empleadoId;
        }, function () { CA.ocupado(boton); });
    });
});
