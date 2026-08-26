/* ============================================================================
   Pantalla: Parametrizacion de ventana de aprobacion por jefe
   Handler : AdministrarVentanaAprobacion.ashx
   ============================================================================ */

var _jefes = [];

$(document).ready(function () {
    BuscarJefes();
});

/* Llama al handler con el formato [{action, parameters}] */
function PostVentana(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarVentanaAprobacion.ashx",
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

/* Lista jefes aplicando el filtro y el checkbox de inactivos */
function BuscarJefes() {
    var filtro = $("#txtBuscar").val();
    var incluirInactivos = $("#chkMostrarInactivos").is(":checked");

    PostVentana("ListaJefes", { "filtro": filtro, "incluirInactivos": incluirInactivos }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        _jefes = respuesta || [];
        RenderTablaJefes(_jefes);
    });
}

function LimpiarBusqueda() {
    $("#txtBuscar").val("");
    BuscarJefes();
}

function RenderTablaJefes(lista) {
    var info = "";
    info += "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer'>";
    info += "<thead><tr role='row'>";
    info += "<th style='text-align:center'>Acciones</th>";
    info += "<th>Jefe</th>";
    info += "<th>Correo</th>";
    info += "<th style='text-align:center'>Colaboradores</th>";
    info += "<th>Ventana de aprobación</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='5' style='text-align:center'>No existen jefes para esta búsqueda.</td></tr>";
    }

    $.each(lista, function (i, item) {
        var esInactivo = (item.Inactivo == 1);

        var ventana = (item.TieneVentana == 1)
            ? (Escapar(item.FechaDesde) + " a " + Escapar(item.FechaHasta))
            : "<span class='label label-default'>Sin ventana</span>";

        var acciones = "";
        if (esInactivo) {
            acciones += "<i class='fa fa-check' title='Reactivar' style='cursor:pointer;color:#3c763d' onclick='ActivarJefe(" + i + ")'></i>";
        } else {
            acciones += "<i class='fa fa-hand-o-right' title='Asignar ventana' style='cursor:pointer' onclick='AbrirAsignar(" + i + ")'></i>";
            acciones += " &nbsp; ";
            acciones += "<i class='fa fa-ban' title='Inactivar de este módulo' style='cursor:pointer;color:#a94442' onclick='InactivarJefe(" + i + ")'></i>";
        }

        var estiloFila = esInactivo ? " style='opacity:0.55'" : "";
        var etiquetaInactivo = esInactivo ? " <span class='label label-default'>Inactivo</span>" : "";

        info += "<tr role='row'" + estiloFila + ">";
        info += "<td style='text-align:center'>" + acciones + "</td>";
        info += "<td>" + Escapar(item.NombreJefe) + etiquetaInactivo + "</td>";
        info += "<td>" + Escapar(item.MailJefe) + "</td>";
        info += "<td style='text-align:center'>" + Escapar(String(item.NumColaboradores)) + "</td>";
        info += "<td>" + ventana + "</td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaJefes").html(info);
}

/* Abre el modal con los datos del jefe seleccionado */
function AbrirAsignar(indice) {
    var item = _jefes[indice];
    if (item == null) { return; }

    $("#txtMailJefeSel").val(item.MailJefe);
    $("#txtNombreJefeSel").val(item.NombreJefe + " (" + item.MailJefe + ")");
    $("#txtVentanaActualSel").val(item.TieneVentana == 1 ? (item.FechaDesde + " a " + item.FechaHasta) : "Sin ventana registrada");

    $("#txtFechaDesde").val(item.TieneVentana == 1 ? item.FechaDesde : FechaHoyISO());
    $("#txtFechaHasta").val(item.TieneVentana == 1 ? item.FechaHasta : FechaHoyISO());
    $("#modalAsignar").modal("show");
}

/* Inactiva/reactiva un jefe en este módulo (acción directa) */
function InactivarJefe(indice) {
    var item = _jefes[indice];
    if (item == null) { return; }
    CambiarEstadoJefe(item.MailJefe, 1);
}

function ActivarJefe(indice) {
    var item = _jefes[indice];
    if (item == null) { return; }
    CambiarEstadoJefe(item.MailJefe, 0);
}

function CambiarEstadoJefe(mailJefe, excluir) {
    var usuarioRegistro = $("#ContentPlaceHolder1_txtLoginUsuario").val();

    var parameters = {
        "mailJefe": mailJefe,
        "excluir": excluir,
        "usuarioRegistro": usuarioRegistro
    };

    PostVentana("ExcluirJefe", parameters, function (respuesta) {
        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado == "1") {
            BuscarJefes();
        }
    });
}

/* Envia la ventana al handler */
function GuardarVentana() {
    var mailJefe = $("#txtMailJefeSel").val();
    var fechaDesde = $("#txtFechaDesde").val(); // yyyy-MM-dd
    var fechaHasta = $("#txtFechaHasta").val();
    var usuarioRegistro = $("#ContentPlaceHolder1_txtLoginUsuario").val();

    if (fechaDesde == null || fechaDesde === "") {
        MostrarMensaje("Debe indicar la fecha desde.", "warning");
        return;
    }
    if (fechaHasta == null || fechaHasta === "") {
        MostrarMensaje("Debe indicar la fecha hasta.", "warning");
        return;
    }
    if (fechaHasta < fechaDesde) {
        MostrarMensaje("La fecha hasta no puede ser menor que la fecha desde.", "warning");
        return;
    }

    var parameters = {
        "mailJefe": mailJefe,
        "fechaDesde": fechaDesde,
        "fechaHasta": fechaHasta,
        "usuarioRegistro": usuarioRegistro
    };

    $("#btnGuardarVentana").prop("disabled", true);

    PostVentana("GuardarVentana", parameters, function (respuesta) {
        $("#btnGuardarVentana").prop("disabled", false);

        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }

        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);

        if (respuesta.estado == "1") {
            $("#modalAsignar").modal("hide");
            BuscarJefes();
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
