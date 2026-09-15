/* ============================================================================
   Pantalla: Mi perfil
   Handler : AdministrarPerfil.ashx

   El Cod_Usuario no se manda nunca: el handler lo saca de la sesion. Si algun
   dia hace falta ver el perfil de otra persona, es una accion distinta con su
   propia validacion, no un parametro de estas.
   ============================================================================ */

var _perfil = null;

$(document).ready(function () {
    CargarPerfil();
});

/* Llama al handler con el formato [{action, parameters}] */
function PostPerfil(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarPerfil.ashx",
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

function CargarPerfil() {
    PostPerfil("CargarPerfil", {}, function (respuesta) {
        // Un objeto con "estado" es un EntRespuesta, es decir, un error.
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        _perfil = respuesta;

        /* Cero filas de cabecera no es "no tiene datos": es que su codigo de usuario
           esta repetido en R_Usuarios y el procedimiento se nego a adivinar cual de
           las dos personas es. Mostrar la pantalla vacia seria peor que no mostrarla:
           pareceria que el perfil no tiene nada, cuando el problema es de identidad. */
        if (!respuesta.PerfilEncontrado) {
            $("#perfilNoIdentificado").show();
            $(".nav-tabs, .tab-content").hide();
            return;
        }

        PintarCabecera(respuesta.Cabecera);
        PintarContacto(respuesta.Contacto);
        PintarEmergencia(respuesta.Emergencia);
    });
}

function PintarCabecera(c) {
    $("#perfilNombre").text(c.NombreCompleto || "–");
    $("#perfilCargo").text(c.Cargo || "–");
    $("#perfilArea").text(c.Area || "–");
    $("#perfilCiudad").text(c.Ciudad || "–");
    $("#perfilAvatar").text(Iniciales(c.NombreCompleto));

    // Edad null significa "no se sabe": guion, nunca cero.
    $("#perfilEdad").text(c.Edad === null ? "–" : c.Edad + " años");

    $("#dpNombre").text(c.NombreCompleto || "–");
    $("#dpCedula").text(c.Cedula || "–");
    $("#dpFnac").text(c.FechaNacTexto || "–");
    $("#dpCargo").text(c.Cargo || "–");
    $("#dpArea").text(c.Area || "–");
    $("#dpJefe").text(c.JefeInmediato || "–");
    $("#dpCiudad").text(c.Ciudad || "–");
    $("#dpCorreo").text(c.CorreoNotificacion || "–");
    // 143 de 231 no tienen horario asignado todavia: guion, no cadena vacia.
    $("#dpHorario").text(c.Horario || "–");

    if (!c.TieneFicha) {
        $("#perfilSinFicha").show();
    }
}

function PintarContacto(c) {
    $("#inDireccion").val(c.Direccion || "");
    $("#inCorreoPersonal").val(c.CorreoPersonal || "");
    $("#inTelefonoPersonal").val(c.TelefonoPersonal || "");

    /* En produccion conviven varias capitalizaciones del mismo estado civil
       ("soltero/a", "Soltero", "SOLTERO/A"...) porque distintas personas de
       Talento Humano lo escribieron a mano con los anios. $.val(x) compara
       tal cual, con mayusculas y minusculas, asi que la mayoria de esos
       valores no calzaba con ninguna opcion del combo y se veia "Seleccione…"
       como si el dato no existiera. Se busca la opcion ignorando mayusculas
       y espacios en vez de forzar el valor crudo. */
    var estadoCivil = c.EstadoCivil || "";
    var $estadoCivil = $("#inEstadoCivil");
    var normalizado = estadoCivil.trim().toUpperCase();
    var $opcionQueCalza = null;

    $estadoCivil.find("option").each(function () {
        if ($(this).text().trim().toUpperCase() === normalizado) {
            $opcionQueCalza = $(this);
            return false;
        }
    });

    if (normalizado === "") {
        $estadoCivil.val("");
        $("#notaEstadoCivilValorSinCalzar").hide();
    } else if ($opcionQueCalza) {
        $estadoCivil.val($opcionQueCalza.val());
        $("#notaEstadoCivilValorSinCalzar").hide();
    } else {
        /* Ninguna opcion calza (por ejemplo "NN" o cualquier variante rara):
           se deja "Seleccione…" pero no se pierde el dato. Si se forzara aca
           una opcion del combo y la persona guardara, se sobrescribiria en
           Empleados.EstadoCivil -de donde lo lee el modulo medico- con una
           tercera capitalizacion inventada por esta pantalla. */
        $estadoCivil.val("");
        $("#notaEstadoCivilValorSinCalzar")
            .text("Valor registrado por Talento Humano: " + estadoCivil)
            .show();
    }

    /* Sin ficha de Talento Humano enlazada, el procedimiento no tiene donde
       escribir el estado civil: el UPDATE afecta cero filas. Se deshabilita
       el campo y se explica antes de que la persona lo llene, en vez de
       dejar que lo guarde y descubra despues -al recargar y verlo vacio-
       que no sirvio de nada. Los otros tres campos si se guardan para
       todo el mundo y quedan habilitados. */
    var tieneFicha = _perfil && _perfil.Cabecera && _perfil.Cabecera.TieneFicha;
    $("#inEstadoCivil").prop("disabled", !tieneFicha);
    $("#notaEstadoCivilSinFicha").toggle(!tieneFicha);
}

function GuardarContacto() {
    /* La comprobacion real esta en el servidor -el handler es alcanzable por
       HTTP directo-, pero esta evita una peticion inutil cuando ya se sabe
       que el codigo de usuario esta repetido: PerfilEncontrado en falso es
       exactamente ese caso (ver CargarPerfil). */
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.", "warning");
        return;
    }

    var datos = {
        correoPersonal:   $("#inCorreoPersonal").val(),
        telefonoPersonal: $("#inTelefonoPersonal").val(),
        direccion:        $("#inDireccion").val(),
        estadoCivil:      $("#inEstadoCivil").val()
    };

    PostPerfil("GuardarContacto", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
    });
}

function PintarEmergencia(lista) {
    var $cuerpo = $("#cuerpoEmergencia").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="4" class="text-center text-muted">' +
                       'Todavía no ha registrado ningún contacto de emergencia.</td></tr>');
        return;
    }

    $.each(lista, function (i, c) {
        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(c.Nombre));
        $fila.append($("<td></td>").text(c.Parentesco));
        $fila.append($("<td></td>").text(c.Telefono));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarEmergencia(' + c.IdContacto + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarEmergencia() {
    var datos = {
        idContacto: 0,
        nombre:     $("#emNombre").val(),
        parentesco: $("#emParentesco").val(),
        telefono:   $("#emTelefono").val()
    };

    PostPerfil("GuardarEmergencia", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#emNombre, #emParentesco, #emTelefono").val("");
            CargarPerfil();
        }
    });
}

function EliminarEmergencia(idContacto) {
    PostPerfil("EliminarEmergencia", { idContacto: idContacto }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}

function Iniciales(nombre) {
    if (!nombre) { return "–"; }
    var partes = nombre.trim().split(/\s+/);
    if (partes.length === 1) { return partes[0].substring(0, 2).toUpperCase(); }
    return (partes[0].charAt(0) + partes[1].charAt(0)).toUpperCase();
}

/* ----------------------------- utilitarios ------------------------------- */

/* Copiada tal cual de parametrizacionHorarioUsuario.js. Cada pantalla lleva
   su propia copia (es el patron de la casa); no se crea una utilidad
   compartida solo para esta pantalla. */
function MostrarMensaje(mensaje, tipo) {
    var color = "#fcf8e3";
    if (tipo == "success") { color = "#dff0d8"; }
    if (tipo == "danger") { color = "#f2dede"; }
    if (tipo == "warning") { color = "#fcf8e3"; }

    $("#modalMensajeInformativoTipo").css("background", color);
    $("#MensajeInformativo").html(mensaje);
    $("#modalMensajeInformativo").modal("show");
}
