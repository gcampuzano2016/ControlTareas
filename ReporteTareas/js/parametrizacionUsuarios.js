/* ============================================================================
   Pantalla: Administracion de usuarios
   Handler : AdministrarUsuarios.ashx
   Esta pantalla no restablece contrasenas: el login autentica contra Active
   Directory, no contra R_Usuarios.Pass_Usuario.
   ============================================================================ */

var _usuarios = [];

$(document).ready(function () {
    BuscarUsuarios();
});

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
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='7' style='text-align:center'>No existen usuarios para esta búsqueda.</td></tr>";
    }

    $.each(lista, function (i, item) {
        var estado = (item.Usuario_Estado === "A")
            ? "<span class='label label-success'>Activo</span>"
            : "<span class='label label-default'>Inactivo</span>";

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
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaUsuarios").html(info);
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
    $("#txtDepartamento").val(u.Departamento);
    $("#txtEmpresa").val(u.Empresa);
    $("#txtCodSap").val(u.Cod_Sap);
    $("#txtJefe").val(u.Cod_Jefe_Inm);
    $("#txtCorreoJefe").val(u.MailCodJefeInm);

    $("#panelDetalle").show();
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
        "departamento": $.trim($("#txtDepartamento").val()),
        "empresa": $.trim($("#txtEmpresa").val()),
        "codSap": $.trim($("#txtCodSap").val()),
        "jefe": $.trim($("#txtJefe").val()),
        "correoJefe": correoJefe
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
