/* ============================================================================
   Pantalla: Administracion de usuarios
   Handler : AdministrarUsuarios.ashx
   Esta pantalla no restablece contrasenas: el login autentica contra Active
   Directory, no contra R_Usuarios.Pass_Usuario.
   ============================================================================ */

var _usuarios = [];
var _departamentos = [];

$(document).ready(function () {
    CargarDepartamentos();
    CargarPerfiles();
    BuscarUsuarios();
});

/* Los departamentos salen de los usuarios que ya existen: no hay catálogo.
   Se cargan una vez al abrir, no con cada búsqueda, porque la lista no depende
   del filtro. */
function CargarDepartamentos() {
    PostUsuario("ListarDepartamentos", {}, function (respuesta) {
        _departamentos = $.isArray(respuesta) ? respuesta : [];

        /* Si alcanzaron a abrir un usuario antes de que llegara la lista, su
           combo tendría una sola opción. Se rellena conservando lo elegido. */
        if ($("#panelDetalle").is(":visible")) {
            LlenarCombo("cboDepartamento", _departamentos, $("#cboDepartamento").val());
        }
    });
}

/* Llena un combo y deja seleccionado el valor que tiene el usuario.
   Si ese valor no está en la lista (dato viejo o escrito a mano), se agrega
   como opción propia: de lo contrario el navegador elegiría la primera y
   guardar sin tocar nada le cambiaría el dato a alguien. */
function LlenarCombo(idCombo, opciones, valorActual) {
    var $cbo = $("#" + idCombo);
    var actual = $.trim(valorActual || "");

    $cbo.empty().append($("<option></option>").attr("value", "").text(""));

    var estaEnLaLista = false;
    $.each(opciones, function (i, opcion) {
        if (opcion === actual) { estaEnLaLista = true; }
        $cbo.append($("<option></option>").attr("value", opcion).text(opcion));
    });

    if (actual !== "" && !estaEnLaLista) {
        $cbo.append($("<option></option>").attr("value", actual).text(actual));
    }

    $cbo.val(actual);
}

function PostUsuario(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);
    $.ajax({
        type: "POST",
        url: "AdministrarUsuarios.ashx",
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

    PostUsuario("BuscarUsuarios", { "filtro": filtro }, function (respuesta) {
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
    info += "<th style='text-align:center'>Editar</th>";
    info += "<th>Código</th>";
    info += "<th>Nombre</th>";
    info += "<th>Login</th>";
    info += "<th>Perfil</th>";
    info += "<th>Correo</th>";
    info += "<th style='text-align:center'>Estado</th>";
    info += "<th style='text-align:center'>Selectores</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='8' style='text-align:center'>No existen usuarios para esta búsqueda.</td></tr>";
    }

    $.each(lista, function (i, item) {
        var estado = (item.Usuario_Estado === "A")
            ? "<span class='label label-success'>Activo</span>"
            : "<span class='label label-default'>Inactivo</span>";

        var selectores = EsActivoEnSelectores(item)
            ? "<span class='label label-success'>Visible</span>"
            : "<span class='label label-warning'>Oculto</span>";

        info += "<tr role='row'>";
        info += "<td style='text-align:center'>";
        info += "<i class='fa fa-pencil' title='Editar' style='cursor:pointer' onclick='SeleccionarUsuario(" + i + ")'></i>";
        info += "</td>";
        info += "<td>" + Escapar(item.Cod_Usuario) + "</td>";
        info += "<td>" + Escapar(item.Nom_Usuario) + "</td>";
        info += "<td>" + Escapar(item.Log_Usuario) + "</td>";
        info += "<td>" + Escapar(item.NombrePerfil) + "</td>";
        info += "<td>" + Escapar(item.E_Mail) + "</td>";
        info += "<td style='text-align:center'>" + estado + "</td>";
        info += "<td style='text-align:center'>" + selectores + "</td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaUsuarios").html(info);
}

/* EstadoUsuario llega como cadena: vacía = NULL en la tabla = activo.
   Cualquier otro valor (0, 1, lo que sea) lo deja fuera de los selectores,
   porque los SPs del sistema filtran con "EstadoUsuario IS NULL". */
function EsActivoEnSelectores(item) {
    return item.EstadoUsuario == null || item.EstadoUsuario === "";
}

function SeleccionarUsuario(indice) {
    var u = _usuarios[indice];
    if (u == null) { return; }

    $("#txtIdUsuarioSel").val(u.Id_Usuario);
    $("#txtCodUsuarioSel").val(u.Cod_Usuario);
    $("#txtLoginSel").val(u.Log_Usuario);
    $("#txtPerfilSel").val(u.NombrePerfil + " (" + u.Id_Perfil + ")");
    $("#txtEstadoSel").val(u.Usuario_Estado === "A" ? "Activo" : "Inactivo");

    $("#txtNombre").val(u.Nom_Usuario);
    $("#txtCorreo").val(u.E_Mail);
    $("#txtCedula").val(u.Cedula);
    LlenarCombo("cboDepartamento", _departamentos, u.Departamento);
    LlenarCombo("cboEmpresa", ["DOS", "AGILITY"], u.Empresa);
    $("#txtCodSap").val(u.Cod_Sap);
    $("#txtJefe").val(u.Cod_Jefe_Inm);
    $("#txtCorreoJefe").val(u.MailCodJefeInm);
    $("#txtTelefonosEmergencia").val(u.TelefonosEmergencia);
    $("#txtCargo").val(u.Cargo);

    /* El botón dice lo que va a hacer, no el estado en el que está. */
    var visible = EsActivoEnSelectores(u);
    $("#txtSelectoresSel").val(visible ? "Visible" : "Oculto");
    $("#btnCambiarEstado")
        .text(visible ? "Inactivar usuario" : "Activar usuario")
        .removeClass("btn-warning btn-info")
        .addClass(visible ? "btn-warning" : "btn-info");

    PrepararCambioDePerfil(u);

    $("#panelDetalle").show();
}

/* Abre la confirmación. El texto explica el efecto real y aclara lo que NO hace,
   porque "inactivar" suena a bloquear el acceso y aquí no lo bloquea. */
function CambiarEstado() {
    var idUsuario = $("#txtIdUsuarioSel").val();
    if (idUsuario == null || idUsuario === "") {
        MostrarMensaje("Debe seleccionar un usuario.", "warning");
        return;
    }

    var visible = ($("#txtSelectoresSel").val() === "Visible");
    var nombre = $("#txtNombre").val();

    var aviso = "";
    if (visible) {
        aviso += "<p>¿Inactivar a <strong>" + Escapar(nombre) + "</strong>?</p>";
        aviso += "<p>Dejará de aparecer en los selectores de jefe, en el autocompletado y en las listas de solicitudes.</p>";
        aviso += "<p class='text-muted'>Esto <strong>no</strong> le impide iniciar sesión: el acceso lo controla el dominio.</p>";
    } else {
        aviso += "<p>¿Activar a <strong>" + Escapar(nombre) + "</strong>?</p>";
        aviso += "<p>Volverá a aparecer en los selectores de jefe, en el autocompletado y en las listas de solicitudes.</p>";
    }

    $("#MensajeConfirmarEstado").html(aviso);
    $("#btnConfirmarEstado").text(visible ? "Inactivar" : "Activar");
    $("#modalConfirmarEstado").modal("show");
}

function ConfirmarCambioEstado() {
    var idUsuario = $("#txtIdUsuarioSel").val();
    var visible = ($("#txtSelectoresSel").val() === "Visible");

    $("#btnConfirmarEstado").prop("disabled", true);
    PostUsuario("CambiarEstadoUsuario", { "idUsuario": idUsuario, "inactivar": visible }, function (respuesta) {
        $("#btnConfirmarEstado").prop("disabled", false);
        $("#modalConfirmarEstado").modal("hide");

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

/* Vacio es valido: el correo es opcional. */
function CorreoValido(correo) {
    if (correo == null || correo === "") { return true; }
    return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(correo);
}

function GuardarUsuario() {
    var idUsuario = $("#txtIdUsuarioSel").val();
    if (idUsuario == null || idUsuario === "") {
        MostrarMensaje("Debe seleccionar un usuario.", "warning");
        return;
    }

    var nombre = $.trim($("#txtNombre").val());
    if (nombre === "") {
        MostrarMensaje("El nombre del usuario es obligatorio.", "warning");
        return;
    }

    var correo = $.trim($("#txtCorreo").val());
    if (!CorreoValido(correo)) {
        MostrarMensaje("El correo no tiene un formato válido.", "warning");
        return;
    }

    var correoJefe = $.trim($("#txtCorreoJefe").val());
    if (!CorreoValido(correoJefe)) {
        MostrarMensaje("El correo del jefe no tiene un formato válido.", "warning");
        return;
    }

    var datos = {
        "idUsuario": idUsuario,
        "nombre": nombre,
        "correo": correo,
        "cedula": $.trim($("#txtCedula").val()),
        "departamento": $.trim($("#cboDepartamento").val() || ""),
        "empresa": $.trim($("#cboEmpresa").val() || ""),
        "codSap": $.trim($("#txtCodSap").val()),
        "jefe": $.trim($("#txtJefe").val()),
        "correoJefe": correoJefe,
        "cargo": $.trim($("#txtCargo").val()),
        "telefonosEmergencia": $.trim($("#txtTelefonosEmergencia").val())
    };

    $("#btnGuardar").prop("disabled", true);
    PostUsuario("GuardarUsuario", datos, function (respuesta) {
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

function VerHistorial() {
    var idUsuario = $("#txtIdUsuarioSel").val();
    if (idUsuario == null || idUsuario === "") {
        MostrarMensaje("Debe seleccionar un usuario.", "warning");
        return;
    }

    PostUsuario("VerBitacora", { "idUsuario": idUsuario }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        var lista = respuesta || [];
        var info = "<table width='100%' class='table table-striped table-bordered'>";
        info += "<thead><tr><th>Fecha</th><th>Acción</th><th>Detalle</th><th>Responsable</th></tr></thead><tbody>";

        if (lista.length === 0) {
            info += "<tr><td colspan='4' style='text-align:center'>Este usuario no tiene cambios registrados.</td></tr>";
        }

        $.each(lista, function (i, item) {
            info += "<tr>";
            info += "<td>" + Escapar(item.Fecha_Registro) + "</td>";
            info += "<td>" + Escapar(item.Accion) + "</td>";
            info += "<td>" + Escapar(item.Detalle) + "</td>";
            info += "<td>" + Escapar(item.Usuario_Registro) + "</td>";
            info += "</tr>";
        });

        info += "</tbody></table>";
        $("#datosHistorial").html(info);
        $("#modalHistorial").modal("show");
    });
}

/* ----------------------------- utilitarios ------------------------------- */

function MostrarMensaje(mensaje, tipo) {
    var color = "#fcf8e3";
    if (tipo == "success") { color = "#dff0d8"; }
    if (tipo == "danger") { color = "#f2dede"; }
    if (tipo == "warning") { color = "#fcf8e3"; }
    $("#modalMensajeInformativoTipo").css("background", color);
    $("#MensajeInformativo").text(mensaje);
    $("#modalMensajeInformativo").modal("show");
}

function Escapar(texto) {
    if (texto == null) { return ""; }
    return $("<div>").text(texto).html();
}

/* ------------------------------------------------- cambio de perfil ----- */

/* Los perfiles del catalogo, para el desplegable. Se cargan una sola vez: a
   diferencia de otros combos de esta pantalla, la lista no depende del usuario
   elegido. */
var _perfiles = [];

/* typeof y no una comparacion directa: si el .aspx no declarara la variable
   -por ejemplo al servirse una version vieja desde la cache-, leer una global
   no declarada lanza ReferenceError y se cae el resto del archivo. */
function PuedeCambiarPerfil() {
    return typeof PUEDE_CAMBIAR_PERFIL !== "undefined" && PUEDE_CAMBIAR_PERFIL;
}

function CargarPerfiles() {
    if (!PuedeCambiarPerfil()) { return; }

    PostUsuario("ListarPerfiles", {}, function (respuesta) {
        // Un objeto con "estado" es un EntRespuesta, es decir, un error.
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        _perfiles = respuesta || [];

        var $combo = $("#cboPerfilNuevo").empty();
        $.each(_perfiles, function (i, p) {
            /* .text(nombre) y no concatenacion: el nombre viene de la base. */
            $combo.append($("<option></option>").attr("value", p.Id_Perfil).text(p.Nombre));
        });
    });
}

/* Se llama al final de SeleccionarUsuario. Deja elegido en el combo el perfil
   que la persona tiene HOY, para que cambiarlo sea un solo gesto y para que se
   vea de que se parte. */
function PrepararCambioDePerfil(u) {
    if (!PuedeCambiarPerfil()) { return; }

    $("#grupoCambiarPerfil").show();
    $("#cboPerfilNuevo").val(u.Id_Perfil);
}

function ConfirmarCambiarPerfil() {
    var idUsuario = $("#txtIdUsuarioSel").val();
    if (idUsuario == null || idUsuario === "") {
        MostrarMensaje("Debe seleccionar un usuario.", "warning");
        return;
    }

    var idPerfilNuevo = $("#cboPerfilNuevo").val();
    if (!idPerfilNuevo) {
        MostrarMensaje("Debe elegir el perfil nuevo.", "warning");
        return;
    }

    var nombre = $("#txtNombre").val();
    var perfilActual = $("#txtPerfilSel").val();
    var perfilNuevo = $("#cboPerfilNuevo option:selected").text();

    var aviso = "";
    aviso += "<p>Cambiar el perfil de <strong>" + Escapar(nombre) + "</strong></p>";
    aviso += "<p>De <strong>" + Escapar(perfilActual) + "</strong> a <strong>" + Escapar(perfilNuevo) + "</strong>.</p>";
    aviso += "<p>Cambia a qué pantallas entra y qué puede hacer en ellas. " +
             "Lo verá <strong>al volver a iniciar sesión</strong>, no de inmediato.</p>";
    aviso += "<p class='text-muted'>Queda registrado en el historial de cambios de esta persona.</p>";

    $("#MensajeConfirmarPerfil").html(aviso);
    $("#modalConfirmarPerfil").modal("show");
}

function EjecutarCambiarPerfil() {
    var idUsuario = $("#txtIdUsuarioSel").val();
    var idPerfil = $("#cboPerfilNuevo").val();

    $("#btnConfirmarPerfil").prop("disabled", true);

    PostUsuario("CambiarPerfilUsuario", { "idUsuario": idUsuario, "idPerfil": idPerfil }, function (respuesta) {
        $("#btnConfirmarPerfil").prop("disabled", false);
        $("#modalConfirmarPerfil").modal("hide");

        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }

        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);

        /* Se recarga la lista para que la columna de perfil y el campo de la
           ficha dejen de decir el valor viejo. BuscarUsuarios oculta el panel
           de detalle, asi que el usuario queda deseleccionado: es deliberado,
           obliga a volver a abrirlo y ver el perfil nuevo escrito. */
        if (respuesta.estado == "1") {
            BuscarUsuarios();
        }
    });
}
