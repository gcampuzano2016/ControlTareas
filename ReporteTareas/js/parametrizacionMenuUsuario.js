/* ============================================================================
   Pantalla: Parametrizacion de modulos por usuario
   Handler : AdministrarMenuUsuario.ashx
   Regla   : el usuario ve los menus de su perfil MAS estos extras.
             Lo heredado del perfil sale marcado y deshabilitado.
   ============================================================================ */

var _usuarios = [];
var _modulos = [];

$(document).ready(function () {
    BuscarUsuarios();
});

function PostMenuUsuario(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);
    $.ajax({
        type: "POST",
        url: "AdministrarMenuUsuario.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) { onSuccess(respuesta); },
        error: function () {
            MostrarMensaje("La operación está tomando demasiado tiempo o la red está saturada. Intente nuevamente.", "danger");
        }
    });
}

function BuscarUsuarios() {
    var filtro = $("#txtBuscar").val();

    PostMenuUsuario("BuscarUsuarios", { "filtro": filtro }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _usuarios = respuesta || [];
        RenderTablaUsuarios(_usuarios);
        $("#panelDetalle").hide();
    });
}

function RenderTablaUsuarios(lista) {
    var info = "";
    info += "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer'>";
    info += "<thead><tr role='row'>";
    info += "<th style='text-align:center'>Módulos</th>";
    info += "<th>Código</th>";
    info += "<th>Nombre</th>";
    info += "<th>Cédula</th>";
    info += "<th>Perfil</th>";
    info += "<th style='text-align:center'>Extras</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='6' style='text-align:center'>No existen usuarios para esta búsqueda.</td></tr>";
    }

    $.each(lista, function (i, item) {
        var extras = (item.TotalExtras > 0)
            ? "<span class='label label-success'>" + item.TotalExtras + "</span>"
            : "";

        info += "<tr role='row'>";
        info += "<td style='text-align:center'>";
        info += "<i class='fa fa-hand-o-right' title='Ver módulos' style='cursor:pointer' onclick='SeleccionarUsuario(" + i + ")'></i>";
        info += "</td>";
        info += "<td>" + Escapar(item.Cod_Usuario) + "</td>";
        info += "<td>" + Escapar(item.Nom_Usuario) + "</td>";
        info += "<td>" + Escapar(item.Cedula) + "</td>";
        info += "<td>" + Escapar(item.NombrePerfil) + "</td>";
        info += "<td style='text-align:center'>" + extras + "</td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaUsuarios").html(info);
}

function SeleccionarUsuario(indice) {
    var item = _usuarios[indice];
    if (item == null) { return; }

    $("#txtCodUsuarioSel").val(item.Cod_Usuario);
    $("#txtNombreUsuarioSel").val(item.Nom_Usuario + " (" + item.Cod_Usuario + ")");
    $("#txtPerfilSel").val(item.NombrePerfil + " (" + item.Id_Perfil + ")");

    PostMenuUsuario("ListaMenuUsuario", { "codUsuario": item.Cod_Usuario }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _modulos = respuesta || [];
        RenderArbol(_modulos);
        $("#panelDetalle").show();
    });
}

function RenderArbol(lista) {
    var padres = $.grep(lista, function (m) { return m.Id_MenuPadre == 0; });

    if (padres.length === 0) {
        $("#datosArbolMenu").html("<p>No existen módulos.</p>");
        return;
    }

    var info = "";
    $.each(padres, function (i, padre) {
        info += "<div style='margin:4px 0'>";
        info += "<label style='font-weight:bold'>";
        info += PintarCheck(padre, "chkPadre", 0);
        info += "<i class='" + EscaparAttr(padre.Class_Icon) + "'></i> " + Escapar(padre.Titulo);
        info += "</label>";
        info += Etiqueta(padre);

        var hijos = $.grep(lista, function (m) { return m.Id_MenuPadre == padre.Id_Menu; });
        $.each(hijos, function (j, hijo) {
            info += "<div style='margin-left:28px'>";
            info += "<label>";
            info += PintarCheck(hijo, "chkHijo", padre.Id_Menu);
            info += "<i class='" + EscaparAttr(hijo.Class_Icon) + "'></i> " + Escapar(hijo.Titulo);
            info += "</label>";
            info += Etiqueta(hijo);
            info += "</div>";
        });
        info += "</div>";
    });

    $("#datosArbolMenu").html(info);
}

/* Heredado del perfil -> marcado y deshabilitado. Extra -> marcado y editable. */
function PintarCheck(item, clase, idPadre) {
    var esPerfil = (item.ActivoPerfil == 1);
    var marcado = (esPerfil || item.ActivoUsuario == 1) ? "checked" : "";
    var bloqueado = esPerfil ? "disabled" : "";
    var evento = (clase === "chkHijo")
        ? " onchange='OnHijoChange(" + idPadre + ")'"
        : " onchange='OnPadreChange(" + item.Id_Menu + ")'";

    var html = "<input type='checkbox' class='chkModulo " + clase + "'";
    html += " data-menu='" + EscaparAttr(String(item.Id_Menu)) + "'";
    if (clase === "chkHijo") {
        html += " data-padre='" + EscaparAttr(String(idPadre)) + "'";
    }
    html += " " + marcado + " " + bloqueado + evento + " /> ";
    return html;
}

function Etiqueta(item) {
    if (item.ActivoPerfil == 1) {
        return " <span class='label label-default'>Perfil</span>";
    }
    if (item.ActivoUsuario == 1) {
        return " <span class='label label-success'>Extra</span>";
    }
    return "";
}

/* Marcar un hijo auto-marca su padre, salvo que el padre ya venga del perfil
   (en ese caso ya esta marcado y deshabilitado). */
function OnHijoChange(idPadre) {
    var algunHijo = $(".chkHijo[data-padre='" + idPadre + "']:checked").length > 0;
    if (algunHijo) {
        $(".chkPadre[data-menu='" + idPadre + "']").not(":disabled").prop("checked", true);
    }
}

/* Desmarcar un padre editable desmarca sus hijos editables. */
function OnPadreChange(idPadre) {
    var padreChecked = $(".chkPadre[data-menu='" + idPadre + "']").is(":checked");
    if (!padreChecked) {
        $(".chkHijo[data-padre='" + idPadre + "']").not(":disabled").prop("checked", false);
    }
}

function GuardarModulos() {
    var codUsuario = $("#txtCodUsuarioSel").val();
    if (codUsuario == null || codUsuario === "") {
        MostrarMensaje("Debe seleccionar un usuario.", "warning");
        return;
    }

    /* Solo viajan los editables marcados: los heredados del perfil no se guardan. */
    var extras = [];
    $(".chkModulo:checked").not(":disabled").each(function () {
        extras.push(parseInt($(this).attr("data-menu"), 10));
    });

    $("#btnGuardar").prop("disabled", true);
    PostMenuUsuario("GuardarMenuUsuario", { "codUsuario": codUsuario, "extras": extras }, function (respuesta) {
        $("#btnGuardar").prop("disabled", false);
        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado == "1") {
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

function EscaparAttr(texto) {
    return Escapar(texto).replace(/'/g, "&#39;").replace(/"/g, "&quot;");
}
