/* ============================================================================
   Administración de perfiles.

   El botón Eliminar se deshabilita cuando el perfil tiene usuarios, pero eso es
   solo una guía: quien decide de verdad es Sp_RTA_EliminarPerfil. El handler es
   alcanzable por HTTP, así que una validación que viva solo acá no es una
   validación.
   ============================================================================ */

var _perfiles = [];

function PostPerfil(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);
    $.ajax({
        type: "POST",
        url: "AdministrarPerfiles.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) { onSuccess(respuesta); },
        error: function () {
            MostrarMensajePerfil("La operación está tomando demasiado tiempo o la red está saturada. Intente nuevamente.", "danger");
        }
    });
}

function MostrarMensajePerfil(mensaje, tipo) {
    var clase = (tipo === "success") ? "alert-success"
              : (tipo === "warning") ? "alert-warning" : "alert-danger";
    $("#divMensajes").html("<div class='alert " + clase + "'>" + mensaje + "</div>");
}

function BuscarPerfiles() {
    PostPerfil("ListarPerfiles", { "filtro": $("#txtBuscar").val() }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensajePerfil(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _perfiles = respuesta || [];
        RenderTablaPerfiles(_perfiles);
        $("#panelDetalle").hide();
    });
}

function RenderTablaPerfiles(lista) {
    var info = "";
    info += "<table width='100%' class='table table-hover'>";
    info += "<thead><tr>";
    info += "<th>Perfil</th><th>Estado</th><th style='text-align:center'>Usuarios</th>";
    info += "<th style='text-align:center'>Editar</th><th style='text-align:center'>Eliminar</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='5'>No hay perfiles que coincidan con la búsqueda.</td></tr>";
    }

    for (var i = 0; i < lista.length; i++) {
        var p = lista[i];
        var etiqueta = (p.Estado === 1)
            ? "<span class='label label-success'>Activo</span>"
            : "<span class='label label-default'>Inactivo</span>";

        /* Con usuarios asignados el botón se deshabilita y el title explica por
           qué. Es más claro que dejarlo activo para que el servidor rechace. */
        var puedeBorrar = (p.Usuarios === 0);
        var botonBorrar = puedeBorrar
            ? "<button type='button' class='btn btn-danger btn-xs' onclick='ConfirmarEliminar(" + p.IdPerfil + ")'>Eliminar</button>"
            : "<button type='button' class='btn btn-danger btn-xs' disabled title='Tiene " + p.Usuarios + " usuarios asignados'>Eliminar</button>";

        info += "<tr>";
        info += "<td>" + p.NombrePerfil + "</td>";
        info += "<td>" + etiqueta + "</td>";
        info += "<td style='text-align:center'>" + p.Usuarios + "</td>";
        info += "<td style='text-align:center'><button type='button' class='btn btn-info btn-xs' onclick='EditarPerfil(" + p.IdPerfil + ")'>Editar</button></td>";
        info += "<td style='text-align:center'>" + botonBorrar + "</td>";
        info += "</tr>";
    }

    info += "</tbody></table>";
    $("#datosTablaPerfiles").html(info);
}

function NuevoPerfil() {
    $("#hdnIdPerfil").val("0");
    $("#txtNombrePerfil").val("");
    $("#cmbEstado").val("1");
    $("#panelDetalle").show();
}

function EditarPerfil(idPerfil) {
    for (var i = 0; i < _perfiles.length; i++) {
        if (_perfiles[i].IdPerfil === idPerfil) {
            $("#hdnIdPerfil").val(_perfiles[i].IdPerfil);
            $("#txtNombrePerfil").val(_perfiles[i].NombrePerfil);
            $("#cmbEstado").val(_perfiles[i].Estado);
            $("#panelDetalle").show();
            return;
        }
    }
}

function CancelarPerfil() {
    $("#panelDetalle").hide();
}

function GuardarPerfil() {
    var nombre = $("#txtNombrePerfil").val();
    if (nombre === null || nombre.replace(/^\s+|\s+$/g, "") === "") {
        MostrarMensajePerfil("Debe escribir el nombre del perfil.", "warning");
        return;
    }

    var parametros = {
        "idPerfil": $("#hdnIdPerfil").val(),
        "nombrePerfil": nombre,
        "estado": $("#cmbEstado").val()
    };

    PostPerfil("GuardarPerfil", parametros, function (respuesta) {
        MostrarMensajePerfil(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#panelDetalle").hide();
            BuscarPerfiles();
        }
    });
}

function ConfirmarEliminar(idPerfil) {
    var nombre = "";
    for (var i = 0; i < _perfiles.length; i++) {
        if (_perfiles[i].IdPerfil === idPerfil) { nombre = _perfiles[i].NombrePerfil; }
    }

    swal({
        title: "¿Eliminar el perfil?",
        text: "Se eliminará \"" + nombre + "\" junto con los menús que tenga asignados. Esta acción no se puede deshacer.",
        type: "warning",
        showCancelButton: true,
        confirmButtonText: "Eliminar",
        cancelButtonText: "Cancelar",
        closeOnConfirm: true
    }, function () {
        PostPerfil("EliminarPerfil", { "idPerfil": idPerfil }, function (respuesta) {
            MostrarMensajePerfil(respuesta.mensaje, respuesta.tipoMensaje);
            BuscarPerfiles();
        });
    });
}

$(function () {
    BuscarPerfiles();
});
