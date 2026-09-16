/* ============================================================================
   Pantalla: Parametrizacion de pagina de inicio por perfil
   Handler : AdministrarPerfilInicio.ashx
   ============================================================================ */

var _perfiles = [];
var _paginas = [];

$(document).ready(function () {
    CargarPaginas(function () {
        BuscarPerfiles();
    });
});

/* Llama al handler con el formato [{action, parameters}] */
function PostInicio(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarPerfilInicio.ashx",
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

/* Carga la lista de paginas (MenuDos) para los desplegables */
function CargarPaginas(callback) {
    PostInicio("ListaPaginas", {}, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            _paginas = [];
        } else {
            _paginas = respuesta || [];
        }
        if (typeof callback === "function") { callback(); }
    });
}

/* Lista los perfiles con su configuracion */
function BuscarPerfiles() {
    PostInicio("ListaPerfilInicio", {}, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _perfiles = respuesta || [];
        RenderTablaPerfiles(_perfiles);
    });
}

function OpcionesPagina(hrefSeleccionado) {
    var html = "<option value=''>-- Seleccione --</option>";
    $.each(_paginas, function (i, p) {
        var sel = (p.Href === hrefSeleccionado) ? " selected" : "";
        html += "<option value='" + EscaparAttr(p.Href) + "'" + sel + ">" + Escapar(p.Titulo) + "</option>";
    });
    return html;
}

function RenderTablaPerfiles(lista) {
    var info = "";
    info += "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer'>";
    info += "<thead><tr role='row'>";
    info += "<th style='text-align:center'>Perfil</th>";
    info += "<th>Nombre</th>";
    info += "<th>Página de inicio</th>";
    info += "<th style='text-align:center'>Tipo (Id_Usuario)</th>";
    info += "<th style='text-align:center'>Acción</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='5' style='text-align:center'>No existen perfiles.</td></tr>";
    }

    $.each(lista, function (i, item) {
        info += "<tr role='row'>";
        info += "<td style='text-align:center'>" + Escapar(String(item.IdPerfil)) + "</td>";
        info += "<td>" + Escapar(item.NombrePerfil) + "</td>";
        info += "<td><select id='pag_" + i + "' class='form-control'>" + OpcionesPagina(item.Href) + "</select></td>";
        info += "<td style='text-align:center'><input id='tipo_" + i + "' type='number' class='form-control' style='width:110px;display:inline-block' value='" + EscaparAttr(String(item.IdTipo)) + "' /></td>";
        info += "<td style='text-align:center'><button type='button' class='btn btn-success btn-sm' onclick='GuardarPerfil(" + i + ")'>Guardar</button></td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaPerfiles").html(info);
}

function GuardarPerfil(indice) {
    var item = _perfiles[indice];
    if (item == null) { return; }

    var href = $("#pag_" + indice).val();
    var idTipo = parseInt($("#tipo_" + indice).val(), 10);
    if (isNaN(idTipo)) { idTipo = 0; }

    if (href == null || href === "") {
        MostrarMensaje("Debe seleccionar la página de inicio.", "warning");
        return;
    }

    var usuarioRegistro = $("#ContentPlaceHolder1_txtLoginUsuario").val();

    var parameters = {
        "idPerfil": item.IdPerfil,
        "href": href,
        "idTipo": idTipo,
        "usuarioRegistro": usuarioRegistro
    };

    PostInicio("GuardarPerfilInicio", parameters, function (respuesta) {
        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado == "1") {
            BuscarPerfiles();
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

function EscaparAttr(texto) {
    return Escapar(texto).replace(/'/g, "&#39;").replace(/"/g, "&quot;");
}
