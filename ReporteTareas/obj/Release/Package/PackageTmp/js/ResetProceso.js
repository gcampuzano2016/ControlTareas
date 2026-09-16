function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

function ResetUsuario() {

        var md = $("#processing-modal");
        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var mensajeVerificacion = "";
        var tipoMensaje = "warning";
        var contadorVerificacion = 0;

      
        if ($('#txtUsuario').val() == "") {
            mensajeVerificacion += "- Debe ingresar el usuario ";
            contadorVerificacion += 1;
        }

        if ($('#txtClaveNueva').val() == "") {
            mensajeVerificacion += "- Debe ingresar la nueva contraseña ";
            contadorVerificacion += 1;
        }

        if ($('#txtConfirmarClave').val() == "") {
            mensajeVerificacion += "- Debe ingresar la confirmación de la contraseña ";
            contadorVerificacion += 1;
        }


        if (contadorVerificacion > 0) {
            alerta(mensajeVerificacion);
            return;
        }

        var datosFormulario = "";

        datosFormulario = datosFormulario + "{";
        datosFormulario = datosFormulario + "'Usuario': '" + $('#txtUsuario').val() + "',";
        datosFormulario = datosFormulario + "'Password': '" + $('#txtClaveNueva').val() + "',";
        datosFormulario = datosFormulario + "'CodigoSeguridad': '" + $('#txtcodigoConfirmacion').val() + "',";
        datosFormulario = datosFormulario + "'ConfirmarPassword': '" + $('#txtConfirmarClave').val() + "'";
        datosFormulario = datosFormulario + "}";

        datos = "[{'action': 'ResetearPassword', 'parameters' : " + datosFormulario + " }]";

        $.ajax({
            type: "POST",
            url: url,
            data: datos,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function () {
                md.modal('show');
            },
            success: function (respuesta) {
                var mensaje = "";
                if (respuesta.estado == "1") {
                    md.modal('hide');
                    MensajeCorrecto(respuesta.mensaje);
                    window.location.href = "http://portaldeservicios.dos.com.ec/RTareas/Formulario/Login.aspx";  
                }
                else if (respuesta.estado == "0") {
                    md.modal('hide');
                    MensajeIncorrecto(respuesta.mensaje);
                }
            },
            error: function (objeto, msgError, objError) {
                var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
                MensajeIncorrecto(mesnajeError);
            }
        });

        return;
    
}