/* ============================================================================
   Pantalla: Parametrizacion de modulos por usuario
   Handler : AdministrarMenuUsuario.ashx
   Regla   : el usuario ve los menus de su perfil MAS estos extras.
             Lo heredado del perfil sale marcado y deshabilitado.
   ============================================================================ */

var _usuarios = [];
var _modulos = [];
var _acciones = [];
var _accionesOk = false;
var _seleccion = 0;
var _usuarioCargado = "";

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

    _seleccion++;
    var miSeleccion = _seleccion;
    _accionesOk = false;
    _usuarioCargado = "";
    $("#datosArbolMenu").html("");
    PostMenuUsuario("ListaMenuUsuario", { "codUsuario": item.Cod_Usuario }, function (respuesta) {
        if (miSeleccion !== _seleccion) { return; }
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _modulos = respuesta || [];
        PostMenuUsuario("ListaAccionesUsuario", { "codUsuario": item.Cod_Usuario }, function (resAcc) {
            if (miSeleccion !== _seleccion) { return; }
            _usuarioCargado = String(item.Cod_Usuario);
            if (resAcc != null && typeof resAcc.estado != "undefined") {
                MostrarMensaje(resAcc.mensaje, resAcc.tipoMensaje);
                _acciones = [];
                _accionesOk = false;
            } else {
                _acciones = resAcc || [];
                _accionesOk = true;
            }
            RenderArbol(_modulos);
            $("#panelDetalle").show();
        });
    });
}

/* Acciones de una pantalla, debajo de ella. Heredada del perfil -> marcada y
   deshabilitada. Adicional -> marcada y editable. Son casillas aparte
   (chkAccion, nunca chkModulo): no viajan como modulos. */
function PintarAcciones(idMenu) {
    var html = "";
    $.each(_acciones, function (i, a) {
        if (a.Id_Menu != idMenu) { return; }
        var esPerfil = (a.ActivoPerfil == 1);
        var marcado = (esPerfil || a.ActivoUsuario == 1) ? "checked" : "";
        var bloqueado = esPerfil ? "disabled" : "";
        var etiqueta = "";
        if (esPerfil) {
            etiqueta = " <span class='label label-default'>Perfil</span>";
        } else if (a.ActivoUsuario == 1) {
            etiqueta = " <span class='label label-success'>Extra</span>";
        }
        html += "<div style='margin-left:56px'>";
        html += "<label style='font-weight:normal' title='" + EscaparAttr(a.Descripcion) + "'>";
        html += "<input type='checkbox' class='chkAccion' data-accion='" + EscaparAttr(String(a.Id_Accion)) + "'";
        html += " data-menu='" + EscaparAttr(String(a.Id_Menu)) + "' " + marcado + " " + bloqueado;
        html += " onchange='OnAccionChange(" + parseInt(a.Id_Menu, 10) + ", this)' /> ";
        html += "<i class='fa fa-cog' style='color:#777'></i> " + Escapar(a.Titulo);
        html += "</label>" + etiqueta + "</div>";
    });
    return html;
}

/* Marcar una accion marca su pantalla si es editable (y por OnHijoChange, el padre). */
function OnAccionChange(idMenu, chk) {
    if (!$(chk).is(":checked")) { return; }
    var $pantalla = $(".chkModulo[data-menu='" + idMenu + "']").not(":disabled");
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

    if (padres.length === 0) {
        $("#datosArbolMenu").html("<p>No existen módulos.</p>");
        return;
    }

    var info = "";
    $.each(padres, function (i, padre) {
        var hijos = $.grep(lista, function (m) { return m.Id_MenuPadre == padre.Id_Menu; });

        info += "<div style='margin:4px 0'>";

        /* El icono de plegado va FUERA del <label>: dentro, cualquier clic sobre el
           marcaria el checkbox asociado. Las ramas sin hijos llevan un hueco del
           mismo ancho para que los titulos queden alineados. */
        if (hijos.length > 0) {
            info += "<i class='fa fa-chevron-down ramaCarro' style='cursor:pointer; width:14px; color:#777'";
            info += " data-rama='" + EscaparAttr(String(padre.Id_Menu)) + "'";
            info += " title='Contraer o expandir'";
            info += " onclick='ToggleRama(" + padre.Id_Menu + ")'></i> ";
        } else {
            info += "<span style='display:inline-block; width:14px'></span> ";
        }

        info += "<label style='font-weight:bold'>";
        info += PintarCheck(padre, "chkPadre", 0);
        info += "<i class='" + EscaparAttr(padre.Class_Icon) + "'></i> " + Escapar(padre.Titulo);
        info += "</label>";
        info += Etiqueta(padre);
        info += PintarAcciones(padre.Id_Menu);
        info += "<span class='ramaResumen' data-rama='" + EscaparAttr(String(padre.Id_Menu)) + "'></span>";

        info += "<div class='ramaHijos' data-rama='" + EscaparAttr(String(padre.Id_Menu)) + "'>";
        $.each(hijos, function (j, hijo) {
            info += "<div style='margin-left:28px'>";
            info += "<label>";
            info += PintarCheck(hijo, "chkHijo", padre.Id_Menu);
            info += "<i class='" + EscaparAttr(hijo.Class_Icon) + "'></i> " + Escapar(hijo.Titulo);
            info += "</label>";
            info += Etiqueta(hijo);
            info += PintarAcciones(hijo.Id_Menu);
            info += "</div>";
        });
        info += "</div>";

        info += "</div>";
    });

    $("#datosArbolMenu").html(info);
}

/* ------------------------- plegado del arbol ----------------------------- */

/* Contrae o expande la rama de un padre. */
function ToggleRama(idPadre) {
    var $rama = $(".ramaHijos[data-rama='" + idPadre + "']");
    if ($rama.length === 0) { return; }

    if ($rama.is(":visible")) {
        $rama.hide();
    } else {
        $rama.show();
    }
    SincronizarRama(idPadre);
}

function ExpandirTodo() {
    $(".ramaHijos").show();
    $(".ramaCarro").each(function () {
        SincronizarRama($(this).attr("data-rama"));
    });
}

function ContraerTodo() {
    $(".ramaHijos").hide();
    $(".ramaCarro").each(function () {
        SincronizarRama($(this).attr("data-rama"));
    });
}

/* Deja el icono y el resumen coherentes con el estado real de la rama.
   Una rama contraida muestra cuantos extras esconde, para que plegar no oculte
   justo lo que se esta administrando. */
function SincronizarRama(idPadre) {
    var $rama = $(".ramaHijos[data-rama='" + idPadre + "']");
    var $icono = $(".ramaCarro[data-rama='" + idPadre + "']");
    var $resumen = $(".ramaResumen[data-rama='" + idPadre + "']");
    var expandida = $rama.is(":visible");

    $icono.toggleClass("fa-chevron-down", expandida).toggleClass("fa-chevron-right", !expandida);

    var extras = $(".chkHijo[data-padre='" + idPadre + "']:checked").not(":disabled").length;
    if (!expandida && extras > 0) {
        $resumen.html(" <span class='label label-success'>" + extras + " extra" + (extras === 1 ? "" : "s") + "</span>");
    } else {
        $resumen.html("");
    }
}

/* Heredado del perfil -> marcado y deshabilitado. Extra -> marcado y editable. */
function PintarCheck(item, clase, idPadre) {
    var esPerfil = (item.ActivoPerfil == 1);
    var marcado = (esPerfil || item.ActivoUsuario == 1) ? "checked" : "";
    var bloqueado = esPerfil ? "disabled" : "";
    var evento = (clase === "chkHijo")
        ? " onchange='OnHijoChangeConAcciones(" + parseInt(idPadre, 10) + ", " + parseInt(item.Id_Menu, 10) + ", this)'"
        : " onchange='OnPadreChange(" + parseInt(item.Id_Menu, 10) + ")'";

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
    SincronizarRama(idPadre);
}

/* Hijo: marca al padre y, si se desmarca, quita sus acciones editables. */
function OnHijoChangeConAcciones(idPadre, idMenu, chk) {
    OnHijoChange(idPadre);
    if (!chk.checked) { DesmarcarAccionesDe(idMenu); }
}

/* Desmarcar un padre editable desmarca sus hijos editables y las acciones
   editables de todos ellos.
   El padre sigue siendo clicable con la rama contraida, asi que hay que
   refrescar el resumen: si no, quedaria anunciando extras que ya se quitaron. */
function OnPadreChange(idPadre) {
    var padreChecked = $(".chkPadre[data-menu='" + idPadre + "']").is(":checked");
    if (!padreChecked) {
        $(".chkHijo[data-padre='" + idPadre + "']").not(":disabled").each(function () {
            $(this).prop("checked", false);
            DesmarcarAccionesDe($(this).attr("data-menu"));
        });
        DesmarcarAccionesDe(idPadre);
    }
    SincronizarRama(idPadre);
}

function GuardarModulos() {
    var codUsuario = $("#txtCodUsuarioSel").val();
    if (codUsuario == null || codUsuario === "") {
        MostrarMensaje("Debe seleccionar un usuario.", "warning");
        return;
    }
    if (String(codUsuario) !== _usuarioCargado) {
        MostrarMensaje("Vuelva a seleccionar el usuario antes de guardar.", "warning");
        return;
    }
    if (!_accionesOk) {
        MostrarMensaje("No se pudieron cargar las acciones; vuelva a seleccionar el usuario antes de guardar.", "warning");
        return;
    }

    /* Solo viajan los editables marcados: los heredados del perfil no se guardan. */
    var extras = [];
    $(".chkModulo:checked").not(":disabled").each(function () {
        extras.push(parseInt($(this).attr("data-menu"), 10));
    });

    var acciones = [];
    $(".chkAccion:checked").not(":disabled").each(function () {
        acciones.push(parseInt($(this).attr("data-accion"), 10));
    });

    $("#btnGuardar").prop("disabled", true);
    PostMenuUsuario("GuardarPermisosUsuario", { "codUsuario": codUsuario, "extras": extras, "acciones": acciones }, function (respuesta) {
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
