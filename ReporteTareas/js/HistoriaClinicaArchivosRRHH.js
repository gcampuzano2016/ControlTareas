function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

//  Función para mostrar mensaje en HistoriaClinicaArchivosRRHH.aspx y cargar archivos
function MensajeCargaArchivo() {
    $('#msgCargarArchivos').modal('show');
}


$(function () {

    $('#btnCargarArchivosAdjuntos').click(function () {
        var url = 'CargaArchivos.ashx';

        // Checking whether FormData is available in browser  
        if (window.FormData !== undefined) {
            var fileUpload = $("#archivosAdjuntos").get(0);
            var files = fileUpload.files;

            // Create FormData object  
            var fileData = new FormData();

            // Looping over all files and add it to FormData object  
            for (var i = 0; i < files.length; i++) {
                // Verificar la extensión del archivo antes de agregarlo
                var allowedExtensions = /(\.xlsx)$/i;
                var fileName = files[i].name.toLowerCase();
                if (!fileName.endsWith('_empleados.xlsx')) {
                    // Mostrar mensaje de error o tomar alguna acción según sea necesario
                    var mesnajeError = "Archivo no reconocido";
                    MensajeIncorrecto(mesnajeError);
                    return;
                }

                // Agregar el archivo a FormData si pasa la condición de extensión
                fileData.append(files[i].name, files[i]);
            }

            // Adding one more key to FormData object  
            fileData.append('session', $("#ContentPlaceHolder1_txtUsuario").val());
            fileData.append('action', 'CargarArchivosMedicos');
            fileData.append('Id_RegTareas', 0);
            fileData.append('idServicio', "1");

            $.ajax({
                 type: "POST",
                 url: url,
                 data: fileData,
                 contentType: false, // Not to set any content header  
                 processData: false, // Not to process data  
                 dataType: "json",
                 beforeSend: function (respuesta) {
                     $("#divMensajes").html("Cargando Archivos...");
                 },
                    success: function (respuesta) {
                        if (respuesta.estado == "1") {
                            MensajeCorrecto(respuesta.mensaje);
                        }
                        $("progress").hide();
                        // Cierra el modal
                        $("#msgCargarArchivos").modal("hide");
                    },
                    xhr: function () {
                        var fileXhr = $.ajaxSettings.xhr();
                        if (fileXhr.upload) {
                            $("progress").show();
                            fileXhr.upload.addEventListener("progress", function (e) {
                                if (e.lengthComputable) {
                                    $("#fileProgress").attr({
                                        value: e.loaded,
                                        max: e.total
                                    });
                                }
                            }, false);
                        }
                        return fileXhr;
                    },
                    error: function (objeto, msgError, objError) {
                        var mesnajeError = "La acción de cargar de archivo esta tomando demasiado tiempo. Verifique su conexión de red y luego intente nuevamente cargar los archivos.";
                        MensajeIncorrecto(mesnajeError);
                    }
                });
            } else {
                var mesnajeError = "FormData no es soportado por su navegador.";
                MensajeIncorrecto(mesnajeError);
            }
    });

});