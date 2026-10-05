/* Curva de Aprendizaje - alta de colaborador (CurvaColaboradorNuevo.aspx). */
$(function () {
    "use strict";
    var resultados = [];
    var temporizador = null;

    $("#caBuscarFicha").on("input", function () {
        clearTimeout(temporizador);
        var texto = $.trim($(this).val() || "");
        if (texto.length < 3) { $("#caResultadosFicha").empty(); return; }
        temporizador = setTimeout(function () {
            CA.api("buscarEmpleadoSistema", { texto: texto }, function (lista) {
                resultados = lista || [];
                if (!resultados.length) { $("#caResultadosFicha").html('<p class="ca-nota">Sin coincidencias en «Datos Empleado».</p>'); return; }
                $("#caResultadosFicha").html('<table class="ca-tabla"><tbody>' + $.map(resultados, function (e, i) {
                    return '<tr class="ca-clic" data-i="' + i + '"><td><b>' + CA.esc(e.Nombre) + "</b></td><td>" + CA.esc(e.Cargo || "") + "</td><td>" + CA.esc(e.Area || "") + "</td></tr>";
                }).join("") + "</tbody></table>");
            });
        }, 300);
    });

    $("#caResultadosFicha").on("click", "tr", function () {
        var e = resultados[$(this).data("i")];
        $("#caNombre").val(e.Nombre || "");
        $("#caCargo").val(e.Cargo || "");
        $("#caArea").val(e.Area || "");
        if (e.Empresa) { $("#caEmpresa").val(e.Empresa); }
        $("#caIdEmpleado").val(e.IdEmpleado);
        $("#caResultadosFicha").empty();
        $("#caFichaVinculada").text("Vinculado a la ficha de " + e.Nombre + ". Puedes corregir los datos antes de guardar.").show();
    });

    $("#caCrear").on("click", function () {
        var nombre = $.trim($("#caNombre").val() || "");
        var ingreso = $("#caIngreso").val() || "";
        if (!nombre || !/^\d{4}-\d{2}-\d{2}$/.test(ingreso)) {
            CA.mensaje("Nombre completo y fecha de ingreso son obligatorios", "warning");
            return;
        }
        var boton = this;
        CA.ocupado(boton, "Guardando…");
        CA.api("crearColaborador", {
            nombre_completo: nombre,
            cargo: $("#caCargo").val(),
            area: $("#caArea").val(),
            empresa: $("#caEmpresa").val(),
            unidad_negocio: $("#caUnidad").val(),
            fecha_ingreso: ingreso,
            id_empleado: $("#caIdEmpleado").val()
        }, function (r) {
            window.location.href = "CurvaColaboradorFicha.aspx?id=" + r.id;
        }, function () { CA.ocupado(boton); });
    });
});
