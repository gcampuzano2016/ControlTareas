var nomArchivo = "";
var nomGenerico = "";

function MensajeIncorrecto(resultado) {
    Swal.fire({
        type: 'error',
        icon: 'error', // Cambié 'type' a 'icon' ya que 'type' está obsoleto
        title: 'Error',
        text: '¡Algo salió mal...! ' + resultado,
        width: '700px',
        height: '200px',
        customClass: {
            title: 'custom-swal-title', // Clase para el título
            text: 'custom-swal-text'    // Clase para el texto
        }
    });
}

function MensajeCorrecto(resultado) {
    Swal.fire({
        type: 'success',
        icon: 'success', // Cambié 'type' a 'icon'
        title: 'Éxito',
        text: '' + resultado,
        width: '600px',
        height: '200px',
        customClass: {
            title: 'custom-swal-title',
            text: 'custom-swal-text'
        }
    });
}

function alerta(respuesta) {
    Swal.fire({
        type: 'warning',
        icon: 'warning', // Cambié 'type' a 'icon'
        title: "Advertencia...!",
        text: "Este es un mensaje de advertencia... " + respuesta,
        width: '800px',
        height: '200px',
        customClass: {
            title: 'custom-swal-title',
            text: 'custom-swal-text'
        }
    });
}

function MostrarMensajeDialogo(divModalTipo, divMensaje, divModal, mensaje, tipoMensaje) {
    if (tipoMensaje == "warning") {
        $(divModalTipo).attr("style", "background: #fcf8e3")
    }
    if (tipoMensaje == "danger") {
        $(divModalTipo).attr("style", "background: #f2dede")
    }
    if (tipoMensaje == "info") {
        $(divModalTipo).attr("style", "background: #d9edf7")
    }
    if (tipoMensaje == "success") {
        $(divModalTipo).attr("style", "background: #dff0d8")
    }

    $(divMensaje).html(mensaje);
    $(divModal).modal('show');
    $("#divMensajes").html("");

    return;
}

// Función para Editar y continuar un nuevo formulario
$(document).on('click', '#btn-buscarInfoHis', function (e) {
    e.preventDefault();
    operacion = "2";
    listaInfoHis("todos", operacion);
});

// Generamos la informacionde los campos y de las celdas donde se encuentran dentro del Excel
function listaInfoHis(nombreArchivo, op) {

    nomArchivo = nombreArchivo;

    // Preparamos los parámetros para enviar
    //var nombreArchivo = "INMUNIZACIONES.xlsx"; // Cambia esto según sea necesario
    var nombreHoja = "078-PERIODICA "; // Cambia esto según sea necesario
    var campos = [
        { nombre: "nombre1", celda: "AD6" },
        { nombre: "nombre2", celda: "AR6" },
        { nombre: "apellido1", celda: "B6" },
        { nombre: "apellido2", celda: "P6" },
        { nombre: "empresa", celda: "B4" },
        { nombre: "sexo", celda: "BF6" },
        { nombre: "fecha", celda: "F129" },
        { nombre: "diagnostico1", celda: "D111" },
        { nombre: "diagnostico2", celda: "D112" },
        { nombre: "diagnostico3", celda: "D113" }
    ];

    // Llamada a la función ObtenerDatosHistoria
    ObtenerDatosHistoria(nombreArchivo, nombreHoja, campos, "", op);
}

// Obtenemos el nombre del Template y el nombre de la hoja
function ObtenerDatosHistoria(nombreArchivo, nombreHoja, campos, tipo2, op) {
    nomGenerico = "PERIODICA*.xlsx";
    // Asegurarse de que los datos sean serializados correctamente.
    var Datos = JSON.stringify([{
        action: "InformacionMedica",
        parameters: {
            operacion: op,
            anio: 2024,
            nombreArchivo: nombreArchivo,
            nombre: nomGenerico,
            nombreHoja: nombreHoja,
            campos: campos, // No hace falta convertirlo a string manualmente
            session: "" // Debes verificar de dónde proviene `cedula`, lo dejé vacío
        }
    }]);

    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectDatos", tipo2);
}

function CargarPagina(div, url, datos, tipoControl, boton, idSeleccionado) {
    if (div !== undefined) {
        $.ajax({
            type: "POST",
            url: url,
            data: datos,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function () {
                $("#divMensajes").html("Cargando Información...");
            },
            success: function (respuesta) {
                if (respuesta !== null) {
                    if (respuesta.tipoMensaje == "danger") {
                        // Mostrar mensajes de error con SweetAlert
                        MensajeIncorrecto(respuesta.mensaje);
                    } else if (respuesta.tipoMensaje == "success") {
                        // Mostrar mensaje de éxito con SweetAlert
                        MensajeCorrecto(respuesta.mensaje);
                    } else if (respuesta.tipoMensaje == "warning") {
                        alerta(respuesta.mensaje);
                    }

                    // Procesar datos cuando no hay errores
                    var idTotalRegistro = respuesta.length;
                    $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
                } else {
                    $(div).html(DosVacio());
                }

                $("#divMensajes").html("");
            },
            error: function () {
                // Mostrar error con SweetAlert
                MensajeIncorrecto(
                    "La búsqueda de la información está tomando demasiado tiempo, la red podría estar saturada. Por favor, inténtelo nuevamente."
                );
            }
        });
    }
}

function RecorreJSON(div, json, tipoControl, boton, idSeleccionado) {
    var contenido = "";

    if (tipoControl === "tableSelectDatos") {
        contenido = RecorreJSONtableSelectDatos(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    return contenido;
}

function RecorreJSONtableSelectDatos(json, boton, idSeleccionado) {
    dtInfoHis(json);
}

/*==========================================================================
 *   Funcion para cargar los datos de la inmunizacion al front HTML
 *=========================================================================*/
function dtInfoHis(json) {
    // Verifica si json es un objeto
    if (typeof json === 'object') {

        let Cedula = json.ruc;
        //document.getElementById("txtCedula").value = Cedula;
        BuscarEmpleado(Cedula);

        let tetanosFec11 = json.tetanosFec11;
        document.getElementById("fechaTetanos1").value = tetanosFec11;
        let tetanosLote21 = json.tetanosLote21;

    } else {
        console.error("json no es un objeto válido.");
    }
}
