let txtVulnerable, txtembarazoSi, txtAntecedentes, txtMotivos, txtIntervencion, txtSignos = "";
var seguimiento = 0;
let IdPerfil = 0;
var archivosTemporales = [];

// Mensajes de Exito o Error con SweetAlert2
function MensajeIncorrecto(resultado) {
    Swal.fire({
        //error
        type: 'error',
        title: 'Error',
        text: '¡Algo salió mal...!   ' + resultado,
        width: '700px',
        height: '300px'
    });
}
function MensajeCorrecto(resultado) {
    Swal.fire({
        type: 'success',
        title: 'Éxito',
        text: '' + resultado,
        width: '700px',
        height: '300px'
    });
}
function MensajeAlerta(resultado) {
    let timerInterval;
    Swal.fire({
        type: 'warning',
        title: "Importante...!!!",
        text: resultado,
        icon: "warning",
        timer: 3000,
        timerProgressBar: true,
        didOpen: () => {
            Swal.showLoading();
            const timer = Swal.getPopup().querySelector("b");
            timerInterval = setInterval(() => {
                timer.textContent = `${Swal.getTimerLeft()}`;
            }, 100);
        },
        willClose: () => {
            clearInterval(timerInterval);
        }
    });
}
function MensajePequeño(icono, resultado) {
    Swal.fire({
        type: icono,
        title: resultado,
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 3000,
        //timerProgressBar: true,
        didOpen: () => {
            Swal.onmouseenter = Swal.stopTimer;
            Swal.onmouseleave = Swal.resumeTimer;
        }
    });
}


function alerta(respuesta) {
    Swal.fire({
        type: 'info',
        title: "Advertencia...!!!",
        text: respuesta,
        icon: "info",
        timer: 3000,
        width: '800px',
        heightAuto: false,
    });
}


/* =================================================================================
 *                      Cargar datos de la base al Historial
 * ================================================================================*/
document.addEventListener('DOMContentLoaded', function () {

    var cedulaEmpleado = $("#ContentPlaceHolder1_hiddenCedulaField").val();
    if (cedulaEmpleado) {
        BuscarEmpleado(cedulaEmpleado); // Ejecuta la función BuscarEmpleado
    } else {
        console.error('CedulaEmpleado está vacío o no se pudo encontrar el campo oculto.');
    }
});
// Definir la función para buscar un empleado
function BuscarEmpleado(cedula) {
    //let cedulaBuscar = document.getElementById("txtEmpleado").value;

    let cedulaBuscar = cedula;

    ObtenerListaEmpleados(cedulaBuscar, "", "");
    document.getElementById("btnCarga").style.display = "flex";
}
function ObtenerListaEmpleados(cedula, descripcion, tipo2) {

    var Datos = "[{ \"action\": \"BuscarEmpleadoPorCedula\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" + cedula + "\"} }]";

    CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}

//function CargarPagina(div, url, datos, tipoControl, boton, idSeleccionado) {

//    if (div != undefined) {
//        $.ajax({
//            type: "POST",
//            url: url,
//            data: datos,
//            contentType: "application/json; charset=utf-8",
//            dataType: "json",
//            beforeSend: function (respuesta) {
//                $("#divMensajes").html("Cargando Información...");
//            },
//            success: function (respuesta) {
//                if (respuesta != null) {
//                    var idTotalRegistro = respuesta.length;
//                    if (respuesta.length == 0) {
//                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
//                    }
//                    else {
//                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
//                    }
//                } else {
//                    $(div).html("No existen datos para esta consulta.");
//                }

//                $("#divMensajes").html("");

//            },
//            error: function (xhr, textStatus, errorThrown) {
//                console.error("Error en AJAX:", {
//                    status: xhr.status,
//                    statusText: xhr.statusText,
//                    responseText: xhr.responseText,
//                    textStatus: textStatus,
//                    errorThrown: errorThrown
//                });

//                // Intenta mostrar el mensaje personalizado del backend si viene en el response
//                var mensajeError = "Ocurrió un error al procesar la solicitud.";

//                try {
//                    var respuesta = JSON.parse(xhr.responseText);
//                    if (respuesta && respuesta.mensaje) {
//                        mensajeError = respuesta.mensaje;
//                    }
//                } catch (e) {
//                    mensajeError = xhr.responseText || mensajeError;
//                }

//                MensajeIncorrecto(mensajeError);
//            }
//        });
//    }
//}
function CargarPagina(div, url, datos, tipoControl, boton, idSeleccionado) {

    if (div != undefined) {
        $.ajax({
            type: "POST",
            url: url,
            data: datos,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function (respuesta) {
                MensajePequeño("info", "Cargando Información...");
            },
            success: function (respuesta) {
                if (respuesta != null) {
                    var idTotalRegistro = "";
                    if (respuesta.length == 0) {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
                    }
                    else {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
                    }
                } else {
                    $(div).html(DosVacio());
                }

                $("#divMensajes").html("");

            },
            error: function (objeto, msgError, objError) {
                var mesnajeError = "La busqueda de la información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
                MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
            }
        });
    }
}

function RecorreJSON(div, json, tipoControl, boton, idSeleccionado) {
    var contenido = "";

    if (tipoControl == "tableSelectBusqueda") {
        contenido = RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectBusquedaVul") {
        contenido = RecorreJSONBusquedaVul(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectBusquedaHsCln") {
        contenido = RecorreJSONBusquedaHsCln(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    return contenido;
}

function RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado) {

    dtEmpleados(json);
} 
function dtEmpleados(json) {

    $.each(json, function (i, item) {

        let Cedula = item.Cedula;
        document.getElementById("txtCedula").value = Cedula;
        let Nombre = item.Nombre;
        document.getElementById("txtNombre").value = Nombre;
        let Sexo = item.Sexo;
        document.getElementById("txtSexo").value = Sexo;
        let PuestoTrabajo = item.PuestoTrabajo;
        document.getElementById("txtPuestoTrabajo").value = PuestoTrabajo;


        let FechaNacimiento = item.Fecha_Nacimiento
        document.getElementById("fechaNac").value = FechaNacimiento;
        let Sociedad = item.Sociedad
        document.getElementById("txtSociedad").value = Sociedad;
        let AreaTrabajo = item.AreaTrabajo
        document.getElementById("txtAreaTrabajo").value = AreaTrabajo;
        let Edad = calcularEdad(FechaNacimiento);
        document.getElementById("txtEdad").value = Edad;
        let EstadoCivil = item.EstadoCivil;
        document.getElementById("txtEstadoCivil").value = EstadoCivil;


        // Llama a la función para cargar la imagen del empleado
        var nombreApellido = Nombre.split(" ");
        if (nombreApellido.length >= 4) {
            var primerApellido = nombreApellido[0];
            var primerNombre = nombreApellido[2];
            var segundoNombre = nombreApellido[3];
            cargarImagen(primerNombre, segundoNombre, primerApellido);
        } else {
            console.error("El formato del nombre no es válido");
        }

        if (item.PerVulnerable === true) {
            ObtenerDatosVul(Cedula, "", "");
        }

        ObtenerDatosHsCln(Cedula, "", "");

    });
}
/*      FUNCION PARA OBTENER LOS DATOS VULNS    */
function ObtenerDatosVul(cedula, descripcion, tipo2) {

    var Datos = "[{ \"action\": \"ObteneConsultarDatosPerVulnerable\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" + cedula + "\"} }]";

    CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', Datos, "tableSelectBusquedaVul", tipo2);
}
function RecorreJSONBusquedaVul(json, boton, idSeleccionado) {
    dtVulnerables(json);
}
function dtVulnerables(json) {
    toggleButtonColor('siVulnerable');
    mostrarOcultarVulnerables('si');

        let vulnerabilidades = json.hsVul_Vulnerds;
        const datos = vulnerabilidades; // Cadena como: "Embarazo; Otro - [Texto]"
        cargarOpcionesDesdeCadena(datos, "opsVulnerables", "vulnerable");
}

function cargarOpcionesDesdeCadena(cadenaTexto, divId, nameCheckbox) {
    // Verificar que exista el div contenedor
    const div = document.getElementById(divId);
    if (!div || !cadenaTexto) return;

    // Limpiar checkboxes e inputs existentes
    const checkboxes = div.querySelectorAll(`input[type="checkbox"][name="${nameCheckbox}"]`);
    checkboxes.forEach(chk => {
        chk.checked = false;
        // Limpiar inputs de texto asociados
        const wrapper = chk.closest('.opcCheckboxMargen');
        const inputText = wrapper?.querySelector('input[type="text"]');
        if (inputText) {
            inputText.value = '';
        }
    });

    // Dividir la cadena por ';' y procesar cada opción
    const opciones = cadenaTexto.split(';');

    opciones.forEach(opcion => {
        const opcionLimpia = opcion.trim();
        if (!opcionLimpia) return;

        // Verificar si la opción tiene texto adicional entre corchetes
        const tieneTextoAdicional = opcionLimpia.includes(' - [') && opcionLimpia.includes(']');
        let textoCheckbox, textoAdicional;

        if (tieneTextoAdicional) {
            // Separar el texto del checkbox del texto adicional
            const partes = opcionLimpia.split(' - [');
            textoCheckbox = partes[0].trim();
            textoAdicional = partes[1].replace(']', '').trim();
        } else {
            textoCheckbox = opcionLimpia;
            textoAdicional = '';
        }

        // Buscar el checkbox correspondiente por el texto del span
        let checkboxEncontrado = null;
        checkboxes.forEach(chk => {
            const span = chk.closest('label')?.querySelector('span:not(.check)');
            const textoSpan = span ? span.textContent.trim() : '';

            if (textoSpan === textoCheckbox) {
                checkboxEncontrado = chk;
            }
        });

        // Si se encontró el checkbox, marcarlo
        if (checkboxEncontrado) {
            checkboxEncontrado.checked = true;

            // Si hay texto adicional, buscarlo en el input asociado
            if (textoAdicional) {
                const wrapper = checkboxEncontrado.closest('.opcCheckboxMargen');
                const inputText = wrapper?.querySelector('input[type="text"]');
                if (inputText) {
                    inputText.value = textoAdicional;
                }
            }
        }
    });
}





/*=================================================================================
 *     Función para cargar la imagen del paciente y manejar si no se encuentra
 *=================================================================================*/
//function cargarImagen(primerNombre, segundoNombre, primerApellido) {
//    // Crea un elemento <img>
//    let divImagen = document.getElementById("imagenDiv");
//    var imagen = document.createElement("img");

//    // Capitaliza la primera letra de cada nombre y apellido
//    primerNombre = primerNombre.charAt(0).toUpperCase() + primerNombre.slice(1).toLowerCase();
//    primerApellido = primerApellido.charAt(0).toUpperCase() + primerApellido.slice(1).toLowerCase();

//    // Construye la ruta de la imagen utilizando el nombre y apellido
//    var rutaImagen = "../carrusel/imagenes/fotos/";

//    // Intenta cargar la imagen con primer nombre y primer apellido
//    rutaImagen += primerNombre + "_" + primerApellido + ".jpg";

//    // Verifica si la imagen existe en la ruta especificada
//    try {
//        let imagen = new Image();
//        imagen.src = rutaImagen;
//        imagen.alt = "Descripción de la imagen";

//        imagen.onload = function () {
//            divImagen.style.backgroundImage = 'url(' + rutaImagen + ')';
//        };

//        imagen.onerror = function () {
//            // Si la imagen con el primer nombre y primer apellido no existe, intenta con el segundo nombre
//            var rutaImagenSegundoNombre = "../carrusel/imagenes/fotos/";

//            if (segundoNombre) {
//                segundoNombre = segundoNombre.charAt(0).toUpperCase() + segundoNombre.slice(1).toLowerCase();
//                rutaImagenSegundoNombre += segundoNombre + "_" + primerApellido + ".jpg";

//                var imagenSegundoNombre = new Image();
//                imagenSegundoNombre.src = rutaImagenSegundoNombre;
//                imagenSegundoNombre.alt = "Descripción de la imagen";

//                imagenSegundoNombre.onload = function () {
//                    // Si la imagen con el segundo nombre y primer apellido existe, cárgala
//                    divImagen.style.backgroundImage = 'url(' + rutaImagenSegundoNombre + ')';
//                };
//                imagenSegundoNombre.onerror = function () {
//                    // Si no hay segunda imagen, muestra la imagen por defecto
//                    rutaImagenSegundoNombre = "../carrusel/imagenes/fotos/usuarios.png";
//                    divImagen.style.backgroundImage = 'url(' + rutaImagenSegundoNombre + ')';
//                };
//            } else {
//                // Si no hay segundo nombre, muestra la imagen por defecto
//                rutaImagenSegundoNombre = "../carrusel/imagenes/fotos/usuarios.png";
//                divImagen.style.backgroundImage = 'url(' + rutaImagenSegundoNombre + ')';
//            }
//        };

//    } catch (error) {
//        // Maneja cualquier excepción que pueda ocurrir
//        console.error("Error al cargar la imagen:", error);
//    }
//}
function cargarImagen(primerNombre, segundoNombre, primerApellido) {
    let divImagen = document.getElementById("imagenDiv");

    // Capitaliza primer nombre y primer apellido
    primerNombre = primerNombre.charAt(0).toUpperCase() + primerNombre.slice(1).toLowerCase();
    primerApellido = primerApellido.charAt(0).toUpperCase() + primerApellido.slice(1).toLowerCase();

    // Rutas posibles
    const rutaBase = "../carrusel/imagenes/fotos/";
    const rutaPrimeraOpcion = rutaBase + primerNombre + "_" + primerApellido + ".jpg";
    const rutaSegundaOpcion = segundoNombre
        ? rutaBase + (segundoNombre.charAt(0).toUpperCase() + segundoNombre.slice(1).toLowerCase()) + "_" + primerApellido + ".jpg"
        : null;
    const rutaDefecto = rutaBase + "usuarios.png";

    // Función para cargar imagen en el div
    function cargarImagenEnDiv(ruta) {
        divImagen.style.backgroundImage = 'url("' + ruta + '")';
    }

    // Primer intento con primerNombre_apellido
    const img1 = new Image();
    img1.onload = function () {
        cargarImagenEnDiv(rutaPrimeraOpcion);
    };
    img1.onerror = function () {
        console.warn("Solo es un mensaje de advertencia - No se encontró la imagen:", rutaPrimeraOpcion);

        // Segundo intento si hay segundo nombre
        if (rutaSegundaOpcion) {
            const img2 = new Image();
            img2.onload = function () {
                cargarImagenEnDiv(rutaSegundaOpcion);
            };
            img2.onerror = function () {
                console.warn("No se encontró la imagen:", rutaSegundaOpcion);
                cargarImagenEnDiv(rutaDefecto);
            };
            img2.src = rutaSegundaOpcion;
        } else {
            cargarImagenEnDiv(rutaDefecto);
        }
    };

    // Comienza cargando la primera imagen
    img1.src = rutaPrimeraOpcion;
}


//  Función para calcular la edad usando la fecha de nacimiento
function calcularEdad(fechaNacimiento) {
    const fechaActual = new Date();
    const partes = fechaNacimiento.split('-'); // Dividimos la fecha en partes
    const fechaNac = new Date(partes[0], partes[1] - 1, partes[2]); // Año, mes (ajustado), día

    let edad = fechaActual.getFullYear() - fechaNac.getFullYear();
    const mesActual = fechaActual.getMonth();
    const mesNac = fechaNac.getMonth();

    if (mesActual < mesNac || (mesActual === mesNac && fechaActual.getDate() < fechaNac.getDate())) {
        edad--;
    }
    return edad;
}



/*============================================================================
 *           Función para manejar la selección de opción Si / NO 
 *============================================================================*/
function toggleButtonColor(buttonId) {
    // Obtiene el elemento del botón según su ID
    var button = document.getElementById(buttonId);

    // Define variables para almacenar texto (inicialmente vacío)
    //txtLentes, txtAlergias, txtExam1, txtExam2, txtExam3, txtExam4, txtExam5, txtExam6 = "";

    // Variable para almacenar el ID del otro botón (para alternar entre "Si" y "No")
    var otherButtonId;

    // Comprueba el ID del botón y establece el texto correspondiente
    if (buttonId === 'siVulnerable') {
        otherButtonId = 'noVulnerable';
        txtVulnerable = "Si";
    } else if (buttonId === 'noVulnerable') {
        otherButtonId = 'siVulnerable';
        txtVulnerable = "No";
    }

    // Obtiene el elemento del otro botón
    var otherButton = document.getElementById(otherButtonId);

    // Si el otro botón tiene la clase "active", la elimina y restablece el color de fondo
    if (otherButton.classList.contains("active")) {
        otherButton.classList.remove("active");
        otherButton.style.backgroundColor = ""; // Restablecer el color de fondo
    }

    // Si el botón actual no tiene la clase "active", la agrega y cambia el color de fondo y texto
    if (!button.classList.contains("active")) {
        button.classList.add("active");
        button.style.backgroundColor = "#7E38D8"; // Cambiar el color de fondo
        button.style.color = "#FFFFFF"; // Cambiar el color del texto
    }
}


// ********   Mostrar campos OPs VULNERABLES   *********
function mostrarOcultarVulnerables(valor) {
    var opsVulnerables = document.getElementById('opsVulnerables');

    if (valor === 'si') {
        opsVulnerables.style.display = 'block';
    } else if (valor === 'no') {
        opsVulnerables.style.display = 'none';

        // Limpiar checkboxes
        document.getElementById("cboxEmbarazo").checked = false;
        document.getElementById("cboxLactancia").checked = false;
        document.getElementById("cboxEnfCatastrofica").checked = false;
        document.getElementById("cboxDiscapacidad").checked = false;
        document.getElementById("cboxOtroVulnerable").checked = false;

        // Limpiar campo de texto
        document.getElementById("idOtroVulnerable").value = "";
    }
}


/* ==========================================================================
    funciones para validar y activar los inputs dependiendo los checkbox    
 ============================================================================*/
// Función para controlar inputs basado en checkboxes
function setupCheckboxInputControl(checkboxId, inputId) {
    const checkbox = document.getElementById(checkboxId);
    const input = document.getElementById(inputId);

    if (checkbox && input) {
        // Evento para cuando cambia el estado del checkbox
        checkbox.addEventListener('change', function () {
            if (this.checked) {
                // Habilitar input y darle foco
                input.disabled = false;
                input.focus();
            } else {
                // Deshabilitar input y limpiar valor
                input.disabled = true;
                input.value = '';
            }
        });

        // Estado inicial - asegurarse de que el input esté deshabilitado si el checkbox no está marcado
        if (!checkbox.checked) {
            input.disabled = true;
        }
    }
}
// Configurar todos los pares checkbox-input cuando la página se carga
document.addEventListener('DOMContentLoaded', function () {
    // Array con los pares de checkbox e input
    const checkboxInputPairs = [
        ['cboxOtroVulnerable', 'idtxtOtroVulnerable'],
        ['cboxDolorMuscular', 'idtxtDolorMuscular'],
        ['cboxOtroMotivo', 'idtxtOtroMotivo'],
        ['cboxAntPerRelevantes', 'idtxtAntPerRelevantes'],
        ['cboxMedActual', 'idtxtMedActual'],
        ['cboxAlergias', 'idtxtAlergias'],
        ['cboxMedEntregada', 'idtxtMedEntregada'],
        ['cboxIntDerivado', 'idtxtDerivado']
    ];

    // Configurar cada par
    checkboxInputPairs.forEach(function (pair) {
        setupCheckboxInputControl(pair[0], pair[1]);
    });
});
// Función adicional para validar que los campos requeridos estén completos
function validateRequiredFields() {
    const checkboxes = document.querySelectorAll('#idHistorialClinico input[type="checkbox"]:checked');
    let allValid = true;

    checkboxes.forEach(function (checkbox) {
        // Encontrar el input relacionado
        const inputId = getRelatedInputId(checkbox.id);
        const input = document.getElementById(inputId);

        if (input && input.value.trim() === '') {
            input.style.borderColor = 'red';
            allValid = false;
        } else if (input) {
            input.style.borderColor = '#ccc';
        }
    });

    return allValid;
}
// Función auxiliar para obtener el ID del input relacionado
function getRelatedInputId(checkboxId) {
    const mapping = {
        'cboxOtroVulnerable': 'idtxtOtroVulnerable',
        'cboxDolorMuscular': 'idtxtDolorMuscular',
        'cboxOtroMotivo': 'idtxtOtroMotivo',
        'cboxAntPerRelevantes': 'idtxtAntPerRelevantes',
        'cboxMedActual': 'idtxtMedActual',
        'cboxAlergias': 'idtxtAlergias',
        'cboxIntMed': 'idtxtMedEntregada',
        'cboxDerivado': 'idtxtDerivado'
    };
    return mapping[checkboxId];
}
function obtenerOpcionesPorSeccion(divId, nameCheckbox) {
    const div = document.getElementById(divId);
    if (!div) return '';

    const resultado = [];

    // Buscar todos los checkboxes con el name dado
    const checkboxes = div.querySelectorAll(`input[type="checkbox"][name="${nameCheckbox}"]`);

    checkboxes.forEach(chk => {
        if (chk.checked) {
            const wrapper = chk.closest('.opcCheckboxMargen');
            const inputText = wrapper?.querySelector('input[type="text"]');
            const textoInput = inputText ? inputText.value.trim() : '';

            // Obtener el texto del checkbox desde el <span>
            const span = chk.closest('label')?.querySelector('span:not(.check)');
            const textoCheckbox = span ? span.textContent.trim() : chk.id;

            if (textoInput) {
                resultado.push(`${textoCheckbox} - [${textoInput}]`);
            } else {
                resultado.push(textoCheckbox);
            }
        }
    });

    return resultado.join('; ');
}



/* =================================================
        funcion para guardar la información
 ==================================================*/
function GuardarHistoria() {
    var url = "ObtenerNuevaListaTareas.ashx";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    txtVulnerable = obtenerOpcionesPorSeccion('opsVulnerables', 'vulnerable');
    txtMotivos = obtenerOpcionesPorSeccion('opsMotivos', 'motivos');
    txtSignos = obtenerSignosVitales();
    txtAntecedentes = obtenerOpcionesPorSeccion('opsAntRelevantes', 'antecedente');
    txtIntervencion = obtenerOpcionesPorSeccion('opsIntervencion', 'intervencion');

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    // Generar el código de contrato usando cédula y fecha
    var cedula = $("#txtCedula").val();
    var fecha = $("#txtfechaAtencion").val();
    var codigoHsCln = generarCodigoContrato(cedula, fecha);

    // Validar que se generó correctamente el código
    if (!codigoHsCln) {
        MensajeIncorrecto("Error al generar el código de atención médica. Verifique la cédula y fecha.");
        return;
    }

    var estaMarcado = document.getElementById("chkSeguimiento").checked;

    if (estaMarcado) {
        seguimiento = 1;
    } 

    var datosFormulario = "";
    datosFormulario = {
        'txtCed': cedula,
        'txtfechaAtencion': fecha,
        'txtHoraAtencion': $("#txtHoraAtencion").val(),
        'txtVulnerable': txtVulnerable,
        'txtMotivos': txtMotivos,
        'txtSignos': txtSignos,
        'txtAntecedentes': txtAntecedentes,
        'txtIntervencion': txtIntervencion,
        'txtRecGeneral': $("#txtRecomendacionG").val(),
        'txtObsGeneral': $("#txtObservacionG").val(),
        'txthsCln_Seguir': seguimiento,
        'txtcodigoHsCln': codigoHsCln  // Agrega el código generado
    };

    var datos = JSON.stringify([{ 'action': 'GuardarAtencionMedica', 'parameters': datosFormulario }]);

    $.ajax({
        type: "POST",
        url: url,
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            $("#divMensajes").html("Guardando Información...");
        },
        success: function (respuesta) {
            if (respuesta.estado == "1") {
                MensajeCorrecto(respuesta.mensaje);
                $("#divMensajes").html("");
                //MostrarPDF();

                // Cargar archivos al servidor con el código generado
                cargarArchivosAlServidor(codigoHsCln);
            }
            else if (respuesta.estado == "0") {
                MensajeIncorrecto(respuesta.mensaje);
            }
			$('#ModalEgresoInventarioPDF').modal('hide'); //Cerramos el Modal
        },
        error: function (objeto, msgError, objError) {
            var mensajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MensajeIncorrecto(mensajeError);
        }
    });

    return;
}
/*  funcion para obtener todos los datos de signos vitales  */
function obtenerSignosVitales() {
    const valores = [];

    const presion = document.getElementById('idtxtSignosPresion')?.value.trim() || '';
    const frecuencia = document.getElementById('idtxtSignosFrecuencia')?.value.trim() || '';
    const temperatura = document.getElementById('idtxtSignosTemperatura')?.value.trim() || '';
    const saturacion = document.querySelector('idtxtSignosSaturacion')?.value.trim() || '';
    const observaciones = document.getElementById('idtxtSignosObsSignos')?.value.trim() || '';

    valores.push(presion, frecuencia, temperatura, saturacion, observaciones);

    const txtSignos1 = valores.join(';|;');
    return txtSignos1;
}
/* Funcion para crear el identificador de la Atención Médica genera código basado en cédula y fecha  */
function generarCodigoContrato(cedula, fecha) {
    try {
        // Validar que tengamos los datos necesarios
        if (!cedula || !fecha) {
            console.error("Faltan datos: cédula o fecha");
            return null;
        }

        // Limpiar la cédula (remover espacios y caracteres especiales)
        cedula = cedula.toString().replace(/\D/g, '');

        // Convertir fecha a formato ddmmyyyy
        var fechaFormateada = "";

        // Si la fecha viene en formato yyyy-mm-dd (input type="date")
        if (fecha.includes('-') && fecha.length === 10) {
            var partesFecha = fecha.split('-');
            var año = partesFecha[0];
            var mes = partesFecha[1];
            var dia = partesFecha[2];
            fechaFormateada = dia + mes + año;
        }
        // Si la fecha viene en formato dd/mm/yyyy
        else if (fecha.includes('/')) {
            var partesFecha = fecha.split('/');
            var dia = partesFecha[0].padStart(2, '0');
            var mes = partesFecha[1].padStart(2, '0');
            var año = partesFecha[2];
            fechaFormateada = dia + mes + año;
        }
        // Si la fecha viene en formato dd-mm-yyyy
        else if (fecha.includes('-') && fecha.length !== 10) {
            var partesFecha = fecha.split('-');
            var dia = partesFecha[0].padStart(2, '0');
            var mes = partesFecha[1].padStart(2, '0');
            var año = partesFecha[2];
            fechaFormateada = dia + mes + año;
        }
        else {
            console.error("Formato de fecha no reconocido: " + fecha);
            return null;
        }

        // Generar el código: cedula_fechaddmmyyyy
        var codigoContrato = cedula + "_" + fechaFormateada;

        console.log("Código generado: " + codigoContrato);
        return codigoContrato;

    } catch (error) {
        console.error("Error al generar código de contrato: ", error);
        return null;
    }
}


/* ==================================================================
 *          Mostrar las atenciones con seguimiento al formulario
 * ==================================================================*/
// Crear tarjetas dinámicamente
function crearTarjetasDesdeJSON(datosJSON, contenedorId = 'contenedorTarjetas') {
    const contenedor = document.getElementById(contenedorId);
    var nombre = $("#txtNombre").val();

    if (!contenedor) {
        console.error('No se encontró el contenedor con ID:', contenedorId);
        return;
    }

    // Limpiar contenedor antes de crear nuevas tarjetas
    contenedor.innerHTML = '';

    // Crear una tarjeta por cada registro en el JSON
    datosJSON.forEach((registro, index) => {
        const idTarjeta = `tarjeta_${index}`;

        // Verificamos que la atención médica tenga un valor de 1 en Seguimiento
        if (registro.hsCln_Seguir == 1) {
            //Mostrar sección de seguimiento
            var cuadroSeguimiento = document.getElementById('seguimiento');
            cuadroSeguimiento.style.display = 'block';

            // Crear el HTML de la tarjeta
            const tarjetaHTML = `
            <div class="horizontal-group-simple" id="${idTarjeta}" style="padding:0; margin-top:0;">
                <div class="col-sm-12" style="margin-bottom: 8px; padding:0;">
                    <!-- Header con nombre y botón minimizar -->
                    <div class="seccion" style="display:flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #ccc; padding: 1rem 1rem 0.5rem 1rem; margin-bottom: 0; background-color:#fed28f; border-radius: 5px;">
                        <div>
                            <label for="disabledTextInput">Nombre:</label><span id="nombre_${idTarjeta}" style="margin-left:1rem;">${nombre || '------'}</span>
                        </div>
                        <div class="seccion">
                            <label for="disabledTextInput">Fecha de atención:</label><span id="fecha_${idTarjeta}" style="margin-left:1rem;">${registro.hsCln_Fecha || '------'}</span>
                            <label for="disabledTextInput" style="margin-left: 2rem;">Hora:</label><span id="hora_${idTarjeta}" style="margin-left:1rem;">${registro.hsCln_Hora || '------'}</span>
                        </div>
                        <button type="button" onclick="toggleTarjeta('${idTarjeta}')" 
                                style="background: none; border: none; cursor: pointer; font-size: 1.2rem; padding: 0.25rem 0.5rem; border-radius: 4px; transition: background-color 0.2s;"
                                onmouseover="this.style.backgroundColor='#ddd'" 
                                onmouseout="this.style.backgroundColor='transparent'"
                                title="Minimizar/Expandir">
                            <span id="icono_${idTarjeta}" class="glyphicon glyphicon-minus"></span>
                        </button>                        
                    </div>
                    
                    <!-- Contenido de la tarjeta (minimizable) -->
                    <div id="contenido_${idTarjeta}" style="background-color:#fad8a68a; padding:1rem;" class="contenido-tarjeta">
                        <div class="seccion">
                            <label for="disabledTextInput">Motivo de consulta</label>
                            <div class="checkbox-list" id="motivo_${idTarjeta}" style="margin-left:1rem;"></div>
                        </div>
                        <div class="seccion">
                            <label for="disabledTextInput">Intervención y Recomendación</label>
                            <div class="checkbox-list" id="intervencion_${idTarjeta}" style="margin-left:1rem;"></div>
                        </div>
                        <div class="seccion">
                            <label for="disabledTextInput">Recomendación:</label>
                            <div class="texto" id="recomendacion_${idTarjeta}" style="margin-left:1rem;">${registro.hsCln_Recom || '------'}</div>
                        </div>
                        <div class="seccion" style="margin-top:1.5rem;">
                            <label for="disabledTextInput">Observación:</label>
                            <div class="texto" id="observacion_${idTarjeta}" style="margin-left:1rem;">${registro.hsCln_Obs || '------'}</div>
                        </div>
                    </div>                    
                </div>
            </div>
        `;

            // Agregar la tarjeta al contenedor
            contenedor.insertAdjacentHTML('beforeend', tarjetaHTML);

            // Llenar los checkboxes de motivos
            if (registro.hsCln_Motivo && registro.hsCln_Motivo.trim().length > 0) {
                const motivoContainer = document.getElementById(`motivo_${idTarjeta}`);

                // Dividir el texto por punto y coma
                const motivosArray = registro.hsCln_Motivo.split(';');

                motivosArray.forEach((motivoCompleto, idx) => {
                    // Limpiar espacios en blanco
                    motivoCompleto = motivoCompleto.trim();

                    if (motivoCompleto.length > 0) {
                        let textoMotivo = '';
                        let textoAdicional = '';

                        // Verificar si contiene texto entre corchetes
                        const regex = /^(.+?)\s*-\s*\[(.+?)\]$/;
                        const match = motivoCompleto.match(regex);

                        if (match) {
                            // Si hay texto entre corchetes
                            textoMotivo = match[1].trim();
                            textoAdicional = match[2].trim();
                        } else {
                            // Si no hay corchetes, todo es el texto del motivo
                            textoMotivo = motivoCompleto;
                        }

                        // Crear el HTML del checkbox con el texto adicional si existe
                        const checkboxHTML = `
                <div style="margin-bottom: 0.5rem; display: flex; align-items: center;">
                    <input type="checkbox" id="motivo_${idTarjeta}_${idx}" checked disabled>
                    <label for="motivo_${idTarjeta}_${idx}" style="margin-left: 0.5rem;">${textoMotivo}</label>
                    ${textoAdicional ? `<span style="margin-left: 0.5rem; color: #666; font-style: italic;">[${textoAdicional}]</span>` : ''}
                </div>
            `;

                        motivoContainer.insertAdjacentHTML('beforeend', checkboxHTML);
                    }
                });
            }

            // Llenar los checkboxes de intervenciones
            if (registro.hsCln_Interv && registro.hsCln_Interv.trim().length > 0) {
                const intervencionContainer = document.getElementById(`intervencion_${idTarjeta}`);

                // Dividir el texto por punto y coma
                const intervencionesArray = registro.hsCln_Interv.split(';');

                intervencionesArray.forEach((intervencionCompleta, idx) => {
                    // Limpiar espacios en blanco
                    intervencionCompleta = intervencionCompleta.trim();

                    if (intervencionCompleta.length > 0) {
                        let textoIntervencion = '';
                        let textoAdicional = '';

                        // Verificar si contiene texto entre corchetes
                        const regex = /^(.+?)\s*-\s*\[(.+?)\]$/;
                        const match = intervencionCompleta.match(regex);

                        if (match) {
                            // Si hay texto entre corchetes
                            textoIntervencion = match[1].trim();
                            textoAdicional = match[2].trim();
                        } else {
                            // Si no hay corchetes, todo es el texto de la intervención
                            textoIntervencion = intervencionCompleta;
                        }

                        // Crear el HTML del checkbox con el texto adicional si existe
                        const checkboxHTML = `
                <div style="margin-bottom: 0.5rem; display: flex; align-items: center;">
                    <input type="checkbox" id="intervencion_${idTarjeta}_${idx}" checked disabled>
                    <label for="intervencion_${idTarjeta}_${idx}" style="margin-left: 0.5rem;">${textoIntervencion}</label>
                    ${textoAdicional ? `<span style="margin-left: 0.5rem; color: #666; font-style: italic;">[${textoAdicional}]</span>` : ''}
                </div>
            `;

                        intervencionContainer.insertAdjacentHTML('beforeend', checkboxHTML);
                    }
                });
            }
            minimizarTodasLasTarjetas();
        }
    });
}

// Función para minimizar/expandir tarjetas
function toggleTarjeta(idTarjeta) {
    const contenido = document.getElementById(`contenido_${idTarjeta}`);
    const icono = document.getElementById(`icono_${idTarjeta}`);

    if (contenido && icono) {
        if (contenido.style.display === 'none') {
            // Expandir
            contenido.style.display = 'block';
            icono.className = 'glyphicon glyphicon-minus';
        } else {
            // Minimizar
            contenido.style.display = 'none';
            icono.className = 'glyphicon glyphicon-menu-down';
        }
    }
}

// Función para minimizar todas las tarjetas
function minimizarTodasLasTarjetas() {
    const tarjetas = document.querySelectorAll('[id^="contenido_tarjeta_"]');
    const iconos = document.querySelectorAll('[id^="icono_tarjeta_"]');

    tarjetas.forEach(tarjeta => {
        tarjeta.style.display = 'none';
    });

    iconos.forEach(icono => {
        icono.className = 'glyphicon glyphicon-menu-down';
    });
}

// Función para expandir todas las tarjetas
function expandirTodasLasTarjetas() {
    const tarjetas = document.querySelectorAll('[id^="contenido_tarjeta_"]');
    const iconos = document.querySelectorAll('[id^="icono_tarjeta_"]');

    tarjetas.forEach(tarjeta => {
        tarjeta.style.display = 'block';
    });

    iconos.forEach(icono => {
        icono.className = 'glyphicon glyphicon-minus';
    });
}


/* ===================     FUNCION PARA OBTENER LOS DATOS HSCLN   ==========================  */
function ObtenerDatosHsCln(cedula, fec_ini, fec_fin) {

    var Datos = "[{ \"action\": \"ObtenerBuscarListaHsCln\", \"parameters\" : { fecha_ini: \"" + fec_ini + "\", fecha_fin: \"" + fec_fin + "\", opc: \"" + 2 + "\", session: \"" + cedula + "\"} }]";

    CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', Datos, "tableSelectBusquedaHsCln", "");
}
function RecorreJSONBusquedaHsCln(json, boton, idSeleccionado) {
    cargarHistorialAtenciones(json);
}
// Función para llamar desde tu pestaña
//function cargarHistorialAtenciones(json) {

//    // Crear las tarjetas
//    crearTarjetasDesdeJSON(json, 'contenedorTarjetas');
//}
function cargarHistorialAtenciones(json) {
    if (json && Array.isArray(json.resultado)) {
        crearTarjetasDesdeJSON(json.resultado, 'contenedorTarjetas');
    } else {
        console.warn('No hay historial de atenciones para mostrar.');
    }
}


/* ==========================================================
 *          fUNCIONES PARA CARGAR LOS ARCHIVOS
 * ==========================================================*/
function cargarArchivosAlServidor(codigoPaciente) {
    var mensajeVerificacion = "";
    var url = 'CargaArchivos.ashx';

    if ($('#txtCedula').val() == "") {
        mensajeVerificacion += "   Debe ingresar el Numero de Cedula y la fecha ";
        contadorVerificacion += 1;
    }

    var nombrePaciente = $("#txtNombre").val(); // o como obtengas el nombre completo
    var cedula = $("#txtCedula").val();
    var fecha = $("#txtfechaAtencion").val();

    // Verificar que tengamos los datos necesarios
    if (!nombrePaciente || !codigoPaciente) {
        console.error("Faltan datos para cargar archivos");
        MensajeIncorrecto("Falta información del paciente o código identificador");
        return;
    }

    // Verificar si hay archivos para enviar
    if (archivosTemporales.length > 0) {
        var fileData = new FormData();

        // Looping over all files in archivosTemporales and add them to FormData object
        //for (var i = 0; i < archivosTemporales.length; i++) {
        //    fileData.append(archivosTemporales[i].name, archivosTemporales[i]);
        //}
        for (var i = 0; i < archivosTemporales.length; i++) {
            let archivo = archivosTemporales[i];
            let nombreLimpio = limpiarNombreArchivo(archivo.name);

            // Agregar archivo con nombre limpio como tercer parámetro
            fileData.append('file_' + i, archivo, nombreLimpio);

            console.log(`Archivo: "${archivo.name}" → "${nombreLimpio}"`);
        }

        // Adding additional keys to FormData object
        fileData.append('session', $("#ContentPlaceHolder1_txtUsuario").val());
        fileData.append('action', 'CargarArchivosAtencionMed');
        fileData.append('nombrePaciente', nombrePaciente);
        fileData.append('cedula', cedula);
        fileData.append('codigoPaciente', codigoPaciente);
        fileData.append('fecha', fecha);
        //fileData.append('Id_RegTareas', 0);
        //fileData.append('idServicio', "1");
        //fileData.append('codContrato', codigoContrato);

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
                    //MensajeCorrecto(respuesta.mensaje);
                    $("progress").hide();
                    $("#msgCargarArchivos").modal("hide");
                    MensajeCorrecto("Se han cargado correctamente los archivos seleccionados.");
                } else {
                    MensajeIncorrecto(respuesta.mensaje || "Error al cargar los archivos");
                }
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
                var mesnajeError = "La acción de cargar de archivo está tomando demasiado tiempo. Verifique su conexión de red y luego intente nuevamente cargar los archivos.";
                MensajeIncorrecto(mesnajeError);
            }
        });
        var mensajeExito = "Se han cargado correctamente los archivos seleccionados.";
        MensajeCorrecto(mensajeExito);

    } else {
        MensajeAlerta("No se han seleccionado archivos para cargar.");
    }
}


//Función para limpiar nombres de archivo
function limpiarNombreArchivo(nombreArchivo) {
    if (!nombreArchivo) return "";

    // Diccionario de reemplazos para caracteres con tilde y especiales
    const reemplazos = {
        'á': 'a', 'à': 'a', 'ä': 'a', 'â': 'a', 'ã': 'a',
        'é': 'e', 'è': 'e', 'ë': 'e', 'ê': 'e',
        'í': 'i', 'ì': 'i', 'ï': 'i', 'î': 'i',
        'ó': 'o', 'ò': 'o', 'ö': 'o', 'ô': 'o', 'õ': 'o',
        'ú': 'u', 'ù': 'u', 'ü': 'u', 'û': 'u',
        'ñ': 'n', 'ç': 'c',
        'Á': 'A', 'À': 'A', 'Ä': 'A', 'Â': 'A', 'Ã': 'A',
        'É': 'E', 'È': 'E', 'Ë': 'E', 'Ê': 'E',
        'Í': 'I', 'Ì': 'I', 'Ï': 'I', 'Î': 'I',
        'Ó': 'O', 'Ò': 'O', 'Ö': 'O', 'Ô': 'O', 'Õ': 'O',
        'Ú': 'U', 'Ù': 'U', 'Ü': 'U', 'Û': 'U',
        'Ñ': 'N', 'Ç': 'C'
    };

    let resultado = "";

    // Reemplazar cada caracter
    for (let char of nombreArchivo) {
        if (reemplazos[char]) {
            resultado += reemplazos[char];
        } else if (/[a-zA-Z0-9\.\-_]/.test(char)) {
            // Mantener solo letras, números, puntos, guiones y guiones bajos
            resultado += char;
        } else if (char === ' ') {
            // Convertir espacios en guiones bajos
            resultado += '_';
        } else {
            // Cualquier otro caracter se convierte en guion bajo
            resultado += '_';
        }
    }

    // Limpiar múltiples guiones bajos consecutivos
    resultado = resultado.replace(/_+/g, '_');

    // Eliminar guiones bajos al inicio y final
    resultado = resultado.replace(/^_+|_+$/g, '');

    return resultado;
}


// Funcion para obtener la lista de botones seleccionados
//function obtenerListaBtnArchivos() {
//    let botonesSeleccionados = document.querySelectorAll(".title-button.selected");
//    let nombresArchivos = Array.from(botonesSeleccionados).map(boton => boton.textContent);

//    return nombresArchivos.join(";"); // Retorna una cadena con los nombres separados por ";"
//}
//function marcarBotonesSeleccionados(listaArchivos) {
//    if (!listaArchivos || typeof listaArchivos !== "string") {
//        console.warn("La lista de archivos es inválida o vacía:", listaArchivos);
//        return; // Detenemos la ejecución si no hay datos válidos
//    }

//    let nombresArchivos = listaArchivos.split(";"); // Convertimos la cadena en un array
//    let botones = document.querySelectorAll(".title-button");

//    botones.forEach(boton => {
//        if (nombresArchivos.includes(boton.textContent)) {
//            boton.classList.add("selected"); // Marcar como seleccionado
//        } else {
//            boton.classList.remove("selected"); // Asegurar que los no seleccionados se mantengan sin clase
//        }
//    });

//    // También actualizamos la lista de archivos en el HTML
//    actualizarListaArchivos();
//}

//function toggleTitle(event, element) {
//    event.preventDefault(); // Evita que la página se desplace hacia arriba

//    // Alternar la clase 'selected' para cambiar el color del botón
//    element.classList.toggle("selected");

//    // Actualizar la lista de archivos seleccionados
//    actualizarListaArchivos();
//}
//function actualizarListaArchivos() {
//    let botonesSeleccionados = document.querySelectorAll(".title-button.selected");
//    let listaArchivosDiv = document.getElementById("listaArchivos2");

//    // Limpiar contenido previo
//    listaArchivosDiv.innerHTML = "";

//    if (botonesSeleccionados.length > 0) {
//        let ul = document.createElement("ul"); // Crear una lista
//        botonesSeleccionados.forEach(boton => {
//            let li = document.createElement("li");
//            li.textContent = boton.textContent;
//            ul.appendChild(li);
//        });
//        listaArchivosDiv.appendChild(ul);
//    } else {
//        listaArchivosDiv.textContent = "No hay archivos seleccionados.";
//    }
//}


// Limpiamos la seleccion de botones redondos antes de cargar una nueva informacion
//function limpiarSeleccionBotones() {
//    let botones = document.querySelectorAll(".title-button.selected");

//    botones.forEach(boton => {
//        boton.classList.remove("selected"); // Quita la clase 'selected'
//    });

//    // También limpiamos la lista de archivos en el HTML
//    document.getElementById("listaArchivos").textContent = "";
//}



/********************************************************************
           Funciones para cargar archivos al sistema
 ********************************************************************/
document.addEventListener("DOMContentLoaded", function () {
    var dropZone = document.getElementById("dropZone");
    var fileInput = document.getElementById("fileInput");
    var fileList = document.getElementById("fileList");

    // Cuando el usuario arrastra archivos sobre la zona
    dropZone.addEventListener("dragover", function (e) {
        e.preventDefault();
        dropZone.classList.add("dragover");
    });

    // Cuando el usuario deja de arrastrar archivos fuera de la zona
    dropZone.addEventListener("dragleave", function () {
        dropZone.classList.remove("dragover");
    });

    // Cuando los archivos son soltados en la zona
    dropZone.addEventListener("drop", function (e) {
        e.preventDefault();
        dropZone.classList.remove("dragover");

        var archivos = e.dataTransfer.files;
        manejarArchivos(archivos);
    });

    // Manejo de archivos seleccionados manualmente desde el input
    fileInput.addEventListener("change", function () {
        manejarArchivos(fileInput.files);
    });

    //var archivosTemporales = [];

    //Manejar archivos y agregar botón de eliminación
    function manejarArchivos(archivos) {
        for (let i = 0; i < archivos.length; i++) {
            let archivo = archivos[i];

            // Verificar si el archivo ya está en la lista para evitar duplicados
            if (archivosTemporales.some(f => f.name === archivo.name)) {
                alert(`El archivo "${archivo.name}" ya ha sido agregado.`);
                continue;
            }

            archivosTemporales.push(archivo);

            // Crear un contenedor para el archivo
            let fileItem = document.createElement("div");
            fileItem.classList.add("file-item");

            // Agregar nombre del archivo
            let fileName = document.createElement("span");
            fileName.textContent = archivo.name;

            // Barra de progreso simulada
            let progressBar = document.createElement("progress");
            progressBar.value = 0;
            progressBar.max = 100;

            // Botón de eliminación
            let deleteBtn = document.createElement("button");
            deleteBtn.textContent = "❌";
            deleteBtn.classList.add("delete-btn");

            // Función de eliminación
            deleteBtn.onclick = function () {
                fileItem.remove(); // Elimina el elemento de la interfaz
                archivosTemporales = archivosTemporales.filter(f => f.name !== archivo.name); // Elimina del array
            };

            // Agregar los elementos al contenedor del archivo
            fileItem.appendChild(fileName);
            fileItem.appendChild(progressBar);
            fileItem.appendChild(deleteBtn); // Ahora el botón se agrega correctamente
            fileList.appendChild(fileItem);

            // Simular carga de archivo
            let progress = 0;
            let interval = setInterval(function () {
                progress += 10;
                progressBar.value = progress;

                if (progress >= 100) {
                    clearInterval(interval);
                }
            }, 200);
        }
    }
});




/* =========================================================
 *      Funciones para sincronizar el html con el PDF
 * =========================================================*/
function sincronizarCheckboxesParaPDF() {
    // Obtener todos los checkboxes que empiecen con 'cbox' del formulario principal
    const checkboxesFormulario = document.querySelectorAll('input[type="checkbox"][id^="cbox"]');

    // Excepciones para casos especiales donde el nombre no sigue el patrón
    const excepciones = {
        'cboxIrritacion': 'cboxPDFIrritacionOcular' // Caso especial
    };    

    checkboxesFormulario.forEach(checkbox => {
        const idFormulario = checkbox.id;
        let idPDF;

        // Verificar si hay una excepción para este checkbox
        if (excepciones[idFormulario]) {
            idPDF = excepciones[idFormulario];
        } else {
            // Generar automáticamente el ID del PDF: cbox... -> cboxPDF...
            idPDF = idFormulario.replace('cbox', 'cboxPDF');
        }

        const checkboxPDF = document.getElementById(idPDF);

        if (checkboxPDF) {
            // Sincronizar el estado del checkbox
            checkboxPDF.checked = checkbox.checked;
            console.log(`Sincronizado: ${idFormulario} -> ${idPDF} (${checkbox.checked})`);
        } else {
            console.warn(`No se encontró el checkbox PDF correspondiente para: ${idFormulario} (buscando: ${idPDF})`);
        }
    });

    // También sincronizar otros datos del formulario
    sincronizarDatosAdicionales();
}

function sincronizarDatosAdicionales() {
    // Esta función nos mantiene actualizados el html con el pdf pero llama a las funciones específicas
    sincronizarCamposEspeciales();
    sincronizarCamposConTextoAdicional();
    sincronizarDatosColaborador();
} 

function sincronizarCheckboxVulnerableEnPDF() {
    // Obtener los botones
    var btnSi = document.getElementById('siVulnerable');
    var btnNo = document.getElementById('noVulnerable');

    // Obtener los checkboxes del PDF
    var chkSi = document.getElementById('PDFsiVulnerable');
    var chkNo = document.getElementById('PDFnoVulnerable');

    // Limpiar los checkboxes
    chkSi.checked = false;
    chkNo.checked = false;

    // Verificar cuál botón tiene la clase "active" y marcar el checkbox correspondiente
    if (btnSi.classList.contains('active')) {
        chkSi.checked = true;
    } else if (btnNo.classList.contains('active')) {
        chkNo.checked = true;
    }
}
// Función específica para manejar campos con texto adicional (como "Dolor muscular - ¿Dónde?")
function sincronizarCamposConTextoAdicional() {
    // Dolor muscular con especificación de ubicación
    const dolorMuscularCheckbox = document.getElementById('cboxDolorMuscular');
    //const dolorMuscularInput = document.getElementById('idtxtDolorMuscular'); // Asumiendo que seguiste el patrón

    if (dolorMuscularCheckbox && dolorMuscularCheckbox.checked) {
        const checkboxPDF = document.getElementById('cboxPDFDolorMuscular');
        if (checkboxPDF) {
            checkboxPDF.checked = true;
        }
    }

    // Otro motivo con especificación
    const otroMotivoCheckbox = document.getElementById('cboxOtroMotivo');
    //const otroMotivoInput = document.getElementById('idtxtOtroMotivo'); // Asumiendo que seguiste el patrón

    if (otroMotivoCheckbox && otroMotivoCheckbox.checked) {
        const checkboxPDF = document.getElementById('cboxPDFOtroMotivo');
        if (checkboxPDF) {
            checkboxPDF.checked = true;            
        }
    }

    const antPerCheckbox = document.getElementById('cboxAntPerRelevantes');
    //const otroMotivoInput = document.getElementById('idtxtOtroMotivo'); // Asumiendo que seguiste el patrón

    if (antPerCheckbox && antPerCheckbox.checked) {
        const checkboxPDF = document.getElementById('cboxPDFAntPerRelevantes');
        if (checkboxPDF) {
            checkboxPDF.checked = true;
        }
    }
}
// Función genérica para sincronización automática completa
function sincronizacionAutomaticaCompleta() {
    console.log('🚀 Iniciando sincronización automática...');
    //1. Sincronizar checkboxes: de SI o NO
    sincronizarCheckboxVulnerableEnPDF();

    // 1. Sincronizar checkboxes: cbox... -> cboxPDF...
    sincronizarCheckboxesAutomatico();

    // 2. Sincronizar inputs: idtxt... -> idtxtPDF...
    sincronizarInputsAutomatico();

    // 3. Sincronizar campos con texto adicional (dolor muscular, otro motivo)
    sincronizarCamposConTextoAdicional();

    // 4. Sincronizar datos de colaborador si existen
    sincronizarDatosColaborador();

    console.log('✅ Sincronización automática completada.');
}

function sincronizarDatosColaborador() {
    // Datos del colaborador que pueden estar en el formulario
    const datosColaborador = [
        { formulario: 'txtNombre', pdf: 'txtPDFNombre' },
        { formulario: 'txtCedula', pdf: 'txtPDFCedula' },
        { formulario: 'txtSexo', pdf: 'txtPDFSexo' },
        { formulario: 'txtAreaTrabajo', pdf: 'txtPDFAreaTrabajo' },
        { formulario: 'txtfechaAtencion', pdf: 'txtPDFfechaAtencion' },
        { formulario: 'txtHoraAtencion', pdf: 'txtPDFtxtHoraAtencion' },

        { formulario: 'txtRecomendacionG', pdf: 'txtPDFRecomendacionG' },
        { formulario: 'txtObservacionG', pdf: 'txtPDFObservacionG' }
    ];

    datosColaborador.forEach(dato => {
        const inputFormulario = document.getElementById(dato.formulario);
        const spanPDF = document.getElementById(dato.pdf);

        if (inputFormulario && spanPDF && inputFormulario.value) {
            spanPDF.textContent = inputFormulario.value;
            console.log(`✓ Dato colaborador: ${dato.formulario} -> ${dato.pdf} (${inputFormulario.value})`);
        }
    });
}

function sincronizarCheckboxesAutomatico() {
    // Buscar TODOS los checkboxes que comiencen con 'cbox' (formulario principal)
    const checkboxesFormulario = document.querySelectorAll('input[type="checkbox"][id^="cbox"]:not([id*="PDF"])');

    checkboxesFormulario.forEach(checkbox => {
        const idFormulario = checkbox.id;

        // Conversión automática: cbox... -> cboxPDF...
        const idPDF = idFormulario.replace('cbox', 'cboxPDF');

        const checkboxPDF = document.getElementById(idPDF);

        if (checkboxPDF) {
            checkboxPDF.checked = checkbox.checked;
            console.log(`✓ Checkbox: ${idFormulario} -> ${idPDF} (${checkbox.checked})`);
        } else {
            console.warn(`⚠ Checkbox PDF no encontrado: ${idFormulario} -> ${idPDF}`);
        }
    });
}

function sincronizarInputsAutomatico() {
    // Buscar TODOS los inputs que comiencen con 'idtxt' (formulario principal)
    const inputs = document.querySelectorAll('input[id^="idtxt"]:not([id*="PDF"]), textarea[id^="idtxt"]:not([id*="PDF"])');

    inputs.forEach(input => {
        if (!input.value) return; // Saltar si no tiene valor

        const idFormulario = input.id;

        // Conversión automática: idtxt... -> idtxtPDF...
        const idPDF = idFormulario.replace('idtxt', 'idtxtPDF');

        // Buscar el span correspondiente en el PDF
        const spanPDF = document.getElementById(idPDF);

        if (spanPDF) {
            spanPDF.textContent = input.value;
            console.log(`✓ Input: ${idFormulario} -> ${idPDF} (${input.value})`);
        } else {
            console.warn(`⚠ Span PDF no encontrado: ${idFormulario} -> ${idPDF}`);
        }
    });

    // También manejar otros campos especiales que no sigan el patrón idtxt
    sincronizarCamposEspeciales();
}

function sincronizarCamposEspeciales() {
    // Manejar campos que pueden tener nombres diferentes
    const camposEspeciales = [
        { formulario: 'txtfechaAtencion', pdf: 'txtPDFfechaAtencion' },
        { formulario: 'txtHoraAtencion', pdf: 'txtPDFHora' },
        { formulario: 'idtxtSignosPresion', pdf: 'idPDFSignos_presion' },
        { formulario: 'idtxtSignosFrecuencia', pdf: 'idPDFSignos_frecuencia' },
        { formulario: 'idtxtSignosTemperatura', pdf: 'idPDFSignos_temperatura' },
        { formulario: 'idtxtSignosSaturacion', pdf: 'idPDFSignos_saturacion' },
        { formulario: 'idtxtSignosObsSignos', pdf: 'idPDFSignos_observaciones' }
    ];

    camposEspeciales.forEach(campo => {
        const inputFormulario = document.getElementById(campo.formulario);
        const elementoPDF = document.getElementById(campo.pdf);

        if (inputFormulario && elementoPDF && inputFormulario.value) {
            elementoPDF.textContent = inputFormulario.value;
            console.log(`✓ Campo especial: ${campo.formulario} -> ${campo.pdf} (${inputFormulario.value})`);
        }
    });
}

// Función para llamar al abrir el modal PDF (versión automática)
function prepararModalPDF() {
    console.log('Preparando modal PDF...');

    // Usar la sincronización automática completa
    sincronizacionAutomaticaCompleta();

    // Mostrar el modal (ajusta el ID según tu modal)
    $('#ModalEgresoInventarioPDF').modal('show');
}

// Función para obtener solo los checkboxes seleccionados (útil para debugging)
function obtenerCheckboxesSeleccionados() {
    const checkboxes = document.querySelectorAll('input[type="checkbox"][name="motivos"]:checked');
    const seleccionados = [];

    checkboxes.forEach(checkbox => {
        const label = checkbox.parentElement.querySelector('span');
        seleccionados.push({
            id: checkbox.id,
            texto: label ? label.textContent.replace('', '').trim() : 'Sin texto',
            valor: checkbox.value
        });
    });

    return seleccionados;
}

// Función para verificar qué elementos se sincronizarán (debugging)
function verificarElementosParaSincronizar() {
    console.log('=== ELEMENTOS PARA SINCRONIZAR ===');

    // Checkboxes: cbox... -> cboxPDF...
    const checkboxes = document.querySelectorAll('input[type="checkbox"][id^="cbox"]:not([id*="PDF"])');
    console.log(`Checkboxes encontrados (cbox...): ${checkboxes.length}`);
    checkboxes.forEach(cb => {
        const idPDF = cb.id.replace('cbox', 'cboxPDF');
        const existe = document.getElementById(idPDF) ? '✓' : '✗';
        const estado = cb.checked ? 'MARCADO' : 'desmarcado';
        console.log(`  ${existe} ${cb.id} -> ${idPDF} (${estado})`);
    });

    // Inputs: idtxt... -> idtxtPDF...
    const inputs = document.querySelectorAll('input[id^="idtxt"]:not([id*="PDF"]), textarea[id^="idtxt"]:not([id*="PDF"])');
    console.log(`Inputs encontrados (idtxt...): ${inputs.length}`);
    inputs.forEach(inp => {
        const idPDF = inp.id.replace('idtxt', 'idtxtPDF');
        const existe = document.getElementById(idPDF) ? '✓' : '✗';
        const valor = inp.value || 'vacío';
        console.log(`  ${existe} ${inp.id} -> ${idPDF} (${valor})`);
    });

    // Campos especiales
    const camposEspeciales = [
        'txtfechaAtencion', 'txtHoraAtencion', 'idtxtSignosPresion', 'idtxtSignosFrecuencia',
        'idtxtSignosTemperatura', 'idtxtSignosSaturacion', 'idtxtSignosObsSignos'
    ];

    console.log(`Campos especiales: ${camposEspeciales.length}`);
    camposEspeciales.forEach(campo => {
        const input = document.getElementById(campo);
        if (input) {
            const valor = input.value || 'vacío';
            console.log(`  ✓ ${campo} (${valor})`);
        } else {
            console.log(`  ✗ ${campo} - NO ENCONTRADO`);
        }
    });
}


function MostrarPDF() {
    // Mostrar el modal siempre
    $("#ModalEgresoInventarioPDF").modal('show');
}

/*     Función para descrgar el PDF de la atención médica   */
function descargarModalComoPDF(modalId) {    

    const nombre = $('#txtNombre').val();
    const fechaAtencion = $('#txtfechaAtencion').val();

    if (!fechaAtencion || fechaAtencion.trim() === "") {
        alert("Debe ingresar la fecha de atención antes de generar el PDF.");
        return;
    }

    const nombreArchivo = "AtencionMedica_" + nombre.replace(/\s+/g, '_') + "_" + fechaAtencion.replace(/\s+/g, '_') + ".pdf";
    const modalBody = document.querySelector(`#${modalId} .modal-body`);

    if (!modalBody) {
        alert("No se encontró el contenido del modal.");
        return;
    }

    html2canvas(modalBody, {
        scale: 2,
        useCORS: true
    }).then(canvas => {
        const imgData = canvas.toDataURL("image/png");
        const { jsPDF } = window.jspdf;
        let pdf = new jsPDF({
            orientation: "p",
            unit: "mm",
            format: "a4"
        });

        const imgWidth = 210; // Ancho total de la hoja A4 en mm
        const pageHeight = 297; // Alto total de la hoja A4 en mm
        const margenSuperior = 10;
        const margenInferior = 14;

        // Altura máxima de la imagen para dejar márgenes
        const maxImgHeight = pageHeight - margenSuperior - margenInferior;
        const imgHeight = (canvas.height * imgWidth) / canvas.width;

        const finalImgHeight = Math.min(imgHeight, maxImgHeight);

        pdf.addImage(imgData, "PNG", 0, margenSuperior, imgWidth, finalImgHeight);

        const pdfBlob = pdf.output("blob");
        const pdfFile = new File([pdfBlob], nombreArchivo, { type: "application/pdf" });

        archivosTemporales.push(pdfFile);
        pdf.save(nombreArchivo);

        GuardarHistoria();

    }).catch(error => {
        console.error("Error al capturar el modal:", error);
        alert("Hubo un error al generar el PDF.");
    });
}





/* ============================================
 *          Regresar al Menu Principal
 * ===========================================*/
document.addEventListener('DOMContentLoaded', () => {
    const btnRegresarForms = document.getElementById('btnRegresar');

    const sendPostRequest = (actionUrl) => {
        let cedulaEmpleado = $('#txtCedula').val();

        if (!cedulaEmpleado) {
            // Evitar el envío si la cédula está vacía
            alert("La cédula no puede estar vacía.");
            return;
        }

        // Crear un formulario dinámico
        let form = document.createElement('form');
        form.method = 'POST';
        form.action = actionUrl;

        // Crear un campo de entrada oculto
        let inputCedula = document.createElement('input');
        inputCedula.type = 'hidden';
        inputCedula.name = 'cedula';
        inputCedula.value = cedulaEmpleado;

        // Añadir el campo al formulario
        form.appendChild(inputCedula);

        // Añadir el formulario al cuerpo del documento
        document.body.appendChild(form);

        // Enviar el formulario
        form.submit();
    };

    btnRegresarForms.addEventListener('click', () => {
        // Mostrar el modal de confirmación
        $('#confirmModal').modal('show');
        document.getElementById('confirmBtn').addEventListener('click', () => {
            sendPostRequest('HistoriaClinica.aspx');
        });
    });
});


$(function () {
    IdPerfil = $("#ContentPlaceHolder1_txtPerfil").val();    
});