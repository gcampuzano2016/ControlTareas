/* ============================================================================
   Pantalla: Catalogo de horarios laborales
   Handler : AdministrarHorarioLaboral.ashx
   Requiere: editorDiasHorario.js (la grilla de los siete dias)
   ============================================================================ */

var _horarios = [];
var _accionConfirmada = null;

$(document).ready(function () {
    DibujarEditorDias("cuerpoDiasHorario", "cat");
    BuscarHorarios();
});

/* Llama al handler con el formato [{action, parameters}] */
function PostHorarioLaboral(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarHorarioLaboral.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) {
            onSuccess(respuesta);
        },
        error: function () {
            MostrarMensaje("La operación está tomando demasiado tiempo o la red está saturada. Intente nuevamente.", "danger");
        }
    });
}

/* ------------------------------- la grilla ------------------------------- */

function BuscarHorarios() {
    var parameters = {
        "filtro": $("#txtBuscar").val(),
        "incluirInactivos": $("#chkInactivos").is(":checked") ? "1" : "0",
        "incluirPropios": $("#chkPropios").is(":checked") ? "1" : "0"
    };

    PostHorarioLaboral("ListaHorarios", parameters, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        _horarios = respuesta || [];
        RenderTablaHorarios(_horarios);
    });
}

function LimpiarBusqueda() {
    $("#txtBuscar").val("");
    BuscarHorarios();
}

function RenderTablaHorarios(lista) {
    var info = "";
    info += "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer'>";
    info += "<thead><tr role='row'>";
    info += "<th style='text-align:center'>Editar</th>";
    info += "<th>Código</th>";
    info += "<th>Nombre</th>";
    info += "<th>Días y horas</th>";
    info += "<th style='text-align:center'>Usuarios</th>";
    info += "<th style='text-align:center'>Estado</th>";
    info += "<th style='text-align:center'>Activar / Inactivar</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='7' style='text-align:center'>No existen horarios para esta búsqueda.</td></tr>";
    }

    $.each(lista, function (i, item) {
        var esPropio = (item.EsPropio == 1);

        var etiquetas = "";
        if (item.EsPredeterminado == 1) {
            etiquetas += " <span class='label label-primary'>Predeterminado</span>";
        }
        if (esPropio) {
            etiquetas += " <span class='label label-info'>Propio de " + Escapar(item.NombreDueno) + "</span>";
        }

        var estado = (item.Activo == 1)
            ? "<span class='label label-success'>Activo</span>"
            : "<span class='label label-default'>Inactivo</span>";

        info += "<tr role='row'>";
        info += "<td style='text-align:center'>";

        /* Los horarios propios se editan desde la pantalla de asignación, junto
           a la persona a la que pertenecen. Acá solo se consultan. */
        if (esPropio) {
            info += "<i class='fa fa-lock' title='Se edita desde Parametrización de horario por usuario'></i>";
        } else {
            info += "<i class='fa fa-pencil' title='Editar horario' style='cursor:pointer' onclick='AbrirEditarHorario(" + i + ")'></i>";
        }

        info += "</td>";
        info += "<td>" + Escapar(item.Codigo) + "</td>";
        info += "<td>" + Escapar(item.Nombre) + etiquetas + "</td>";
        info += "<td>" + Escapar(item.Resumen) + "</td>";
        info += "<td style='text-align:center'>" + item.UsuariosAsignados + "</td>";
        info += "<td style='text-align:center'>" + estado + "</td>";
        info += "<td style='text-align:center'>";

        if (esPropio) {
            info += "-";
        } else if (item.Activo == 1) {
            info += "<button type='button' class='btn btn-xs btn-warning' onclick='PedirCambioEstado(" + i + ")'>Inactivar</button>";
        } else {
            info += "<button type='button' class='btn btn-xs btn-success' onclick='PedirCambioEstado(" + i + ")'>Activar</button>";
        }

        info += "</td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaHorarios").html(info);
}

/* ------------------------------- el editor ------------------------------- */

function AbrirNuevoHorario() {
    $("#txtIdHorario").val("0");
    $("#txtCodigo").val("").prop("disabled", false);
    $("#txtNombre").val("");
    $("#chkPredeterminado").prop("checked", false).prop("disabled", false);
    $("#chkActivo").prop("checked", true).prop("disabled", false);
    $("#modalHorarioLabel").text("Nuevo horario laboral");
    $("#avisoAsignados").hide();

    LimpiarEditorDias("cat");
    $("#modalHorario").modal("show");
}

function AbrirEditarHorario(indice) {
    var item = _horarios[indice];
    if (item == null) { return; }

    $("#txtIdHorario").val(item.IdHorarioLaboral);
    $("#txtCodigo").val(item.Codigo).prop("disabled", false);
    $("#txtNombre").val(item.Nombre);
    $("#chkPredeterminado").prop("checked", item.EsPredeterminado == 1);
    $("#chkActivo").prop("checked", item.Activo == 1);
    $("#modalHorarioLabel").text("Editar horario laboral");

    /* Quitarle la marca al único predeterminado dejaría sin horario a quien no
       tiene asignación explícita, y el SP lo rechaza. Mejor no ofrecerlo. */
    $("#chkPredeterminado").prop("disabled", item.EsPredeterminado == 1);

    if (item.UsuariosAsignados > 0) {
        $("#textoAsignados").text(
            "Este horario lo tienen asignado " + item.UsuariosAsignados +
            " usuario(s). Si cambia los días o las horas, se recalculan las horas suplementarias y extraordinarias ya registradas con él.");
        $("#avisoAsignados").show();
    } else {
        $("#avisoAsignados").hide();
    }

    LimpiarEditorDias("cat");

    PostHorarioLaboral("ObtenerHorario", { "idHorario": item.IdHorarioLaboral }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        LlenarEditorDias("cat", respuesta);
        $("#modalHorario").modal("show");
    });
}

function CopiarLunesAViernes() {
    var error = AplicarLunesAViernes("cat");

    if (error !== "") {
        MostrarMensaje(error, "warning");
    }
}

function GuardarHorario(confirmaSobrescribir) {
    var codigo = $.trim($("#txtCodigo").val());
    var nombre = $.trim($("#txtNombre").val());

    if (codigo === "") {
        MostrarMensaje("Debe indicar el código del horario.", "warning");
        return;
    }
    if (nombre === "") {
        MostrarMensaje("Debe indicar el nombre del horario.", "warning");
        return;
    }

    var errorDias = ValidarEditorDias("cat");
    if (errorDias !== "") {
        MostrarMensaje(errorDias, "warning");
        return;
    }

    var parameters = {
        "idHorario": $("#txtIdHorario").val(),
        "codigo": codigo,
        "nombre": nombre,
        "esPredeterminado": $("#chkPredeterminado").is(":checked") ? "1" : "0",
        "activo": $("#chkActivo").is(":checked") ? "1" : "0",
        "dias": LeerEditorDias("cat"),
        "usuarioRegistro": $("#ContentPlaceHolder1_txtLoginUsuario").val(),
        "confirmaSobrescribir": confirmaSobrescribir ? "1" : "0"
    };

    $("#btnGuardarHorario").prop("disabled", true);

    PostHorarioLaboral("GuardarHorario", parameters, function (respuesta) {
        $("#btnGuardarHorario").prop("disabled", false);

        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }

        /* -5: el SP no guardó nada porque cambian las horas de un horario en uso */
        if (respuesta.resultado == "-5") {
            PedirConfirmacion("Confirmar el cambio de horas", respuesta.mensaje + " ¿Desea continuar?", function () {
                GuardarHorario(true);
            });
            return;
        }

        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);

        if (respuesta.estado == "1") {
            $("#modalHorario").modal("hide");
            BuscarHorarios();
        }
    });
}

/* --------------------------- activar / inactivar -------------------------- */

function PedirCambioEstado(indice) {
    var item = _horarios[indice];
    if (item == null) { return; }

    var activar = (item.Activo != 1);

    var texto = activar
        ? "¿Activar el horario \"" + item.Nombre + "\"? Volverá a estar disponible para asignarlo."
        : "¿Inactivar el horario \"" + item.Nombre + "\"? Dejará de ofrecerse al asignar, pero no se borra el historial de quienes lo tuvieron.";

    PedirConfirmacion(activar ? "Activar horario" : "Inactivar horario", texto, function () {
        var parameters = {
            "idHorario": item.IdHorarioLaboral,
            "activo": activar ? "1" : "0",
            "usuarioRegistro": $("#ContentPlaceHolder1_txtLoginUsuario").val()
        };

        PostHorarioLaboral("CambiarEstadoHorario", parameters, function (respuesta) {
            if (respuesta == null) {
                MostrarMensaje("No se recibió respuesta del servidor.", "danger");
                return;
            }

            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);

            if (respuesta.estado == "1") {
                BuscarHorarios();
            }
        });
    });
}

/* ----------------------------- utilitarios ------------------------------- */

function PedirConfirmacion(titulo, texto, alConfirmar) {
    _accionConfirmada = alConfirmar;

    $("#modalConfirmarLabel").text(titulo);
    $("#TextoConfirmar").text(texto);
    $("#modalConfirmar").modal("show");
}

function EjecutarConfirmacion() {
    $("#modalConfirmar").modal("hide");

    if (typeof _accionConfirmada === "function") {
        var accion = _accionConfirmada;
        _accionConfirmada = null;
        accion();
    }
}

function MostrarMensaje(mensaje, tipo) {
    var color = "#fcf8e3";
    if (tipo == "success") { color = "#dff0d8"; }
    if (tipo == "danger") { color = "#f2dede"; }
    if (tipo == "warning") { color = "#fcf8e3"; }

    $("#modalMensajeInformativoTipo").css("background", color);
    $("#MensajeInformativo").html(mensaje);
    $("#modalMensajeInformativo").modal("show");
}

function Escapar(texto) {
    if (texto == null) { return ""; }
    return $("<div>").text(texto).html();
}
