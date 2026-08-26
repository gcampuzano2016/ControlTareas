/* ============================================================================
   Pantalla: Parametrizacion de menus por perfil (arbol padre-hijo)
   Handler : AdministrarMenuPerfil.ashx
   ============================================================================ */

var _menus = [];

$(document).ready(function () {
    CargarPerfiles();
});

function PostMenu(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);
    $.ajax({
        type: "POST",
        url: "AdministrarMenuPerfil.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) { onSuccess(respuesta); },
        error: function () {
            MostrarMensaje("La operación está tomando demasiado tiempo o la red está saturada. Intente nuevamente.", "danger");
        }
    });
}

function CargarPerfiles() {
    PostMenu("ListaPerfiles", {}, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        var lista = respuesta || [];
        var html = "<option value=''>-- Seleccione --</option>";
        $.each(lista, function (i, p) {
            html += "<option value='" + EscaparAttr(String(p.Id_Perfil)) + "'>" + Escapar(p.Nombre) + "</option>";
        });
        $("#cmbPerfil").html(html);
        $("#datosArbolMenu").html("");
    });
}

function BuscarMenus() {
    var idPerfil = $("#cmbPerfil").val();
    if (idPerfil == null || idPerfil === "") {
        $("#datosArbolMenu").html("");
        return;
    }
    PostMenu("ListaMenuPerfil", { "idPerfil": idPerfil }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _menus = respuesta || [];
        RenderArbol(_menus);
    });
}

function RenderArbol(lista) {
    var padres = $.grep(lista, function (m) { return m.Id_MenuPadre == 0; });

    var info = "";
    if (padres.length === 0) {
        info = "<p>No existen menús.</p>";
        $("#datosArbolMenu").html(info);
        return;
    }

    $.each(padres, function (i, padre) {
        var chkP = (padre.Activo == 1) ? "checked" : "";
        info += "<div style='margin:4px 0'>";
        info += "<label style='font-weight:bold'>";
        info += "<input type='checkbox' class='chkMenu chkPadre' data-menu='" + EscaparAttr(String(padre.Id_Menu)) + "' " + chkP + " onchange='OnPadreChange(" + padre.Id_Menu + ")' /> ";
        info += "<i class='" + EscaparAttr(padre.Class_Icon) + "'></i> " + Escapar(padre.Titulo);
        info += "</label>";

        var hijos = $.grep(lista, function (m) { return m.Id_MenuPadre == padre.Id_Menu; });
        $.each(hijos, function (j, hijo) {
            var chkH = (hijo.Activo == 1) ? "checked" : "";
            info += "<div style='margin-left:28px'>";
            info += "<label>";
            info += "<input type='checkbox' class='chkMenu chkHijo' data-menu='" + EscaparAttr(String(hijo.Id_Menu)) + "' data-padre='" + EscaparAttr(String(padre.Id_Menu)) + "' " + chkH + " onchange='OnHijoChange(" + padre.Id_Menu + ")' /> ";
            info += "<i class='" + EscaparAttr(hijo.Class_Icon) + "'></i> " + Escapar(hijo.Titulo);
            info += "</label>";
            info += "</div>";
        });
        info += "</div>";
    });

    $("#datosArbolMenu").html(info);
}

/* Marcar un hijo auto-marca el padre */
function OnHijoChange(idPadre) {
    var algunHijo = $(".chkHijo[data-padre='" + idPadre + "']:checked").length > 0;
    if (algunHijo) {
        $(".chkPadre[data-menu='" + idPadre + "']").prop("checked", true);
    }
}

/* Desmarcar el padre desmarca sus hijos */
function OnPadreChange(idPadre) {
    var padreChecked = $(".chkPadre[data-menu='" + idPadre + "']").is(":checked");
    if (!padreChecked) {
        $(".chkHijo[data-padre='" + idPadre + "']").prop("checked", false);
    }
}

function GuardarMenus() {
    var idPerfil = $("#cmbPerfil").val();
    if (idPerfil == null || idPerfil === "") {
        MostrarMensaje("Debe seleccionar un perfil.", "warning");
        return;
    }

    var activos = [];
    $(".chkMenu:checked").each(function () {
        activos.push(parseInt($(this).attr("data-menu"), 10));
    });

    $("#btnGuardar").prop("disabled", true);
    PostMenu("GuardarMenuPerfil", { "idPerfil": idPerfil, "activos": activos }, function (respuesta) {
        $("#btnGuardar").prop("disabled", false);
        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado == "1") {
            BuscarMenus();
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
