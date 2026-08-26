/* ============================================================================
   Pantalla: Parametrizacion de horario por usuario
   Handler : AdministrarHorarioUsuario.ashx
   ============================================================================ */

var _usuariosHorario = [];

$(document).ready(function () {
    CargarPerfilesHorario();
    BuscarUsuarios();
});

/* Llama al handler con el formato [{action, parameters}] */
function PostHorario(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarHorarioUsuario.ashx",
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

/* Carga el combo de perfiles de horario */
function CargarPerfilesHorario() {
    PostHorario("ListaPerfilesHorario", {}, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        var $cmb = $("#cmbPerfilHorario");
        $cmb.empty();
        $.each(respuesta, function (i, item) {
            $cmb.append($("<option></option>").attr("value", item.Id).text(item.Valor));
        });
    });
}

/* Lista usuarios aplicando el filtro del cuadro de busqueda */
function BuscarUsuarios() {
    var filtro = $("#txtBuscar").val();

    PostHorario("ListaUsuariosHorario", { "filtro": filtro }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        _usuariosHorario = respuesta || [];
        RenderTablaUsuarios(_usuariosHorario);
    });
}

function LimpiarBusqueda() {
    $("#txtBuscar").val("");
    BuscarUsuarios();
}

function RenderTablaUsuarios(lista) {
    var info = "";
    info += "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer'>";
    info += "<thead><tr role='row'>";
    info += "<th style='text-align:center'>Asignar</th>";
    info += "<th>Código</th>";
    info += "<th>Nombre</th>";
    info += "<th>Cédula</th>";
    info += "<th>Departamento</th>";
    info += "<th>Horario vigente</th>";
    info += "<th>Desde</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='7' style='text-align:center'>No existen usuarios para esta búsqueda.</td></tr>";
    }

    $.each(lista, function (i, item) {
        var badge = (item.EsPredeterminado == 1)
            ? " <span class='label label-default'>Predeterminado</span>"
            : "";

        info += "<tr role='row'>";
        info += "<td style='text-align:center'>";
        info += "<i class='fa fa-hand-o-right' title='Asignar horario' style='cursor:pointer' onclick='AbrirAsignar(" + i + ")'></i>";
        info += "</td>";
        info += "<td>" + Escapar(item.Cod_Usuario) + "</td>";
        info += "<td>" + Escapar(item.Nom_Usuario) + "</td>";
        info += "<td>" + Escapar(item.Cedula) + "</td>";
        info += "<td>" + Escapar(item.Departamento) + "</td>";
        info += "<td>" + Escapar(item.NombreHorario) + badge + "</td>";
        info += "<td>" + Escapar(item.FechaDesde) + "</td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaUsuarios").html(info);
}

/* Abre el modal con los datos del usuario seleccionado */
function AbrirAsignar(indice) {
    var item = _usuariosHorario[indice];
    if (item == null) { return; }

    $("#txtCodUsuarioSel").val(item.Cod_Usuario);
    $("#txtNombreUsuarioSel").val(item.Nom_Usuario + " (" + item.Cod_Usuario + ")");
    $("#txtHorarioActualSel").val(item.NombreHorario + (item.EsPredeterminado == 1 ? " [Predeterminado]" : ""));

    /* Si el usuario ya tiene un perfil real asignado, lo preselecciona */
    if (item.IdHorarioLaboral && item.IdHorarioLaboral > 0) {
        $("#cmbPerfilHorario").val(item.IdHorarioLaboral.toString());
    }

    $("#txtFechaDesde").val(FechaHoyISO());
    $("#modalAsignar").modal("show");
}

/* Envia la asignacion al handler */
function GuardarAsignacion() {
    var codUsuario = $("#txtCodUsuarioSel").val();
    var idHorario = $("#cmbPerfilHorario").val();
    var fechaDesde = $("#txtFechaDesde").val(); // yyyy-MM-dd (input type=date)
    var usuarioRegistro = $("#ContentPlaceHolder1_txtLoginUsuario").val();

    if (idHorario == null || idHorario === "") {
        MostrarMensaje("Debe seleccionar un perfil de horario.", "warning");
        return;
    }
    if (fechaDesde == null || fechaDesde === "") {
        MostrarMensaje("Debe indicar la fecha desde la cual rige el horario.", "warning");
        return;
    }

    var parameters = {
        "codUsuario": codUsuario,
        "idHorario": idHorario,
        "fechaDesde": fechaDesde,
        "usuarioRegistro": usuarioRegistro
    };

    $("#btnGuardarAsignacion").prop("disabled", true);

    PostHorario("AsignarHorarioUsuario", parameters, function (respuesta) {
        $("#btnGuardarAsignacion").prop("disabled", false);

        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }

        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);

        if (respuesta.estado == "1") {
            $("#modalAsignar").modal("hide");
            BuscarUsuarios();
        }
    });
}

/* ----------------------------- utilitarios ------------------------------- */

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

function FechaHoyISO() {
    var d = new Date();
    var mes = ("0" + (d.getMonth() + 1)).slice(-2);
    var dia = ("0" + d.getDate()).slice(-2);
    return d.getFullYear() + "-" + mes + "-" + dia;
}
