/* ============================================================================
   Pantalla: Parametrizacion de menus por perfil (arbol padre-hijo)
   Handler : AdministrarMenuPerfil.ashx
   ============================================================================ */

var _menus = [];
var _acciones = [];

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
        PostMenu("ListaAccionesPerfil", { "idPerfil": idPerfil }, function (resAcc) {
            if (resAcc != null && typeof resAcc.estado != "undefined") {
                MostrarMensaje(resAcc.mensaje, resAcc.tipoMensaje);
                _acciones = [];
            } else {
                _acciones = resAcc || [];
            }
            RenderArbol(_menus);
        });
    });
}

/* Acciones de una pantalla, debajo de ella. Son casillas aparte (chkAccion):
   nunca viajan como menus. */
function PintarAcciones(idMenu) {
    var html = "";
    var esSuper = ($("#cmbPerfil").val() === "18");
    $.each(_acciones, function (i, a) {
        if (a.Id_Menu != idMenu) { return; }
        var marcado = (esSuper || a.Activo == 1) ? "checked" : "";
        var bloqueado = esSuper ? "disabled" : "";
        html += "<div style='margin-left:56px'>";
        html += "<label style='font-weight:normal' title='" + EscaparAttr(a.Descripcion) + "'>";
        html += "<input type='checkbox' class='chkAccion' data-accion='" + EscaparAttr(String(a.Id_Accion)) + "'";
        html += " data-menu='" + EscaparAttr(String(a.Id_Menu)) + "' " + marcado + " " + bloqueado;
        html += " onchange='OnAccionChange(" + parseInt(a.Id_Menu, 10) + ", this)' /> ";
        html += "<i class='fa fa-cog' style='color:#777'></i> " + Escapar(a.Titulo);
        html += "</label></div>";
    });
    return html;
}

/* Marcar una accion marca su pantalla (y por OnHijoChange, el padre). */
function OnAccionChange(idMenu, chk) {
    if (!$(chk).is(":checked")) { return; }
    var $pantalla = $(".chkMenu[data-menu='" + idMenu + "']");
    $pantalla.prop("checked", true);
    var padre = $pantalla.attr("data-padre");
    if (padre) { OnHijoChange(padre); }
}

/* Desmarcar una pantalla desmarca sus acciones editables y avisa una vez. */
function DesmarcarAccionesDe(idMenu) {
    var $acc = $(".chkAccion[data-menu='" + idMenu + "']:checked").not(":disabled");
    if ($acc.length > 0) {
        $acc.prop("checked", false);
        MostrarMensaje("Se quitaron también las acciones de esa pantalla.", "warning");
    }
}

function RenderArbol(lista) {
    var padres = $.grep(lista, function (m) { return m.Id_MenuPadre == 0; });

    var info = "";
    if (padres.length === 0) {
        info = "<p>No existen menús.</p>";
        $("#datosArbolMenu").html(info);
        return;
    }

    if ($("#cmbPerfil").val() === "18") {
        info += "<div class='alert alert-info'>Super Admin: tiene todas las acciones; las pantallas que desmarque solo dejan de verse en su menú.</div>";
    }

    $.each(padres, function (i, padre) {
        var chkP = (padre.Activo == 1) ? "checked" : "";
        info += "<div style='margin:4px 0'>";
        info += "<label style='font-weight:bold'>";
        info += "<input type='checkbox' class='chkMenu chkPadre' data-menu='" + EscaparAttr(String(padre.Id_Menu)) + "' " + chkP + " onchange='OnPadreChange(" + padre.Id_Menu + ")' /> ";
        info += "<i class='" + EscaparAttr(padre.Class_Icon) + "'></i> " + Escapar(padre.Titulo);
        info += "</label>";
        info += PintarAcciones(padre.Id_Menu);

        var hijos = $.grep(lista, function (m) { return m.Id_MenuPadre == padre.Id_Menu; });
        $.each(hijos, function (j, hijo) {
            var chkH = (hijo.Activo == 1) ? "checked" : "";
            info += "<div style='margin-left:28px'>";
            info += "<label>";
            info += "<input type='checkbox' class='chkMenu chkHijo' data-menu='" + EscaparAttr(String(hijo.Id_Menu)) + "' data-padre='" + EscaparAttr(String(padre.Id_Menu)) + "' " + chkH + " onchange='OnHijoChangeConAcciones(" + parseInt(padre.Id_Menu, 10) + ", " + parseInt(hijo.Id_Menu, 10) + ", this)' /> ";
            info += "<i class='" + EscaparAttr(hijo.Class_Icon) + "'></i> " + Escapar(hijo.Titulo);
            info += "</label>";
            info += PintarAcciones(hijo.Id_Menu);
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

/* Hijo: marca al padre y, si se desmarca, quita sus acciones */
function OnHijoChangeConAcciones(idPadre, idMenu, chk) {
    OnHijoChange(idPadre);
    if (!chk.checked) { DesmarcarAccionesDe(idMenu); }
}

/* Desmarcar el padre desmarca sus hijos y las acciones de todos ellos */
function OnPadreChange(idPadre) {
    var padreChecked = $(".chkPadre[data-menu='" + idPadre + "']").is(":checked");
    if (!padreChecked) {
        $(".chkHijo[data-padre='" + idPadre + "']").each(function () {
            $(this).prop("checked", false);
            DesmarcarAccionesDe($(this).attr("data-menu"));
        });
        DesmarcarAccionesDe(idPadre);
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

    var acciones = [];
    $(".chkAccion:checked").not(":disabled").each(function () {
        acciones.push(parseInt($(this).attr("data-accion"), 10));
    });

    $("#btnGuardar").prop("disabled", true);
    PostMenu("GuardarPermisosPerfil", { "idPerfil": idPerfil, "activos": activos, "acciones": acciones }, function (respuesta) {
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
