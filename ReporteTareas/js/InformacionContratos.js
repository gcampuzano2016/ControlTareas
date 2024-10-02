var buttons = document.querySelectorAll(".btn-custom"); // Obtener todos los botones
var formaPago = "";
var txtHardware = "";
var txtServiciosDOS = "";
var txtServiciosExt = "";
var txtPolizas = "";
var txtTDR = "";
var txtPreguntas = "";
var txtActAdj = "";
var txtActNeg = "";
var txtGarantiasFIN = "";
var txtGarantiasTEC = "";
var txtLicenciaTemporales = "";
var otherButtonId = "";
var idCliente2 = 0;

var valTotalContrato, valTotalCinco;
var archivosSeleccionados = [];
var archivosTemporales = [];
var contadorVerificacion = 0;

var CodUsuario = 0;
var opcionUsuario = 0;

var OrdenesServicioJoin = "";
var numeroContratoGlobal;


function MensajeIncorrecto(resultado) {
    Swal.fire({
        //error
        type: 'error',
        title: 'Error',
        text: '¡Algo salió mal...!   ' + resultado,
        width: '700px',
        height: '200px'
    });
}

function MensajeCorrecto(resultado) {
    Swal.fire({
        type: 'success',
        title: 'Éxito',
        text: '' + resultado,
        width: '600px',
        height: '200px'
    });
}

function alerta(respuesta) {
    Swal.fire({
        title: "Advertencia...!!!",
        text: "Este es un mensaje de advertencia..." + respuesta,
        icon: "info",
        width: '800px',
        height: '200px'
    });
}

$("#opLentes").click(function () {
    Swal.fire('Ejemplo basico de Sweet Alert 2');
});

//con opción de TYPE  //tipos de popups: error, success, warning, info, question
$("#btn2").click(function () {
    Swal.fire({
        type: 'success',
        title: 'Éxito',
        text: '¡Perfecto!',
    });
});



// Funcion para saber si se desea  Crear un NUEVO contrato o EDITAR
function manejarBoton(contrato, numeroBoton) {
    if (numeroBoton === 1) {
        BuscarEstadoNotificacion("", "", 1);
    } else if (numeroBoton === 2) {
        // Pedir al usuario que ingrese el número de contrato
        const numeroContrato = prompt("Por favor, ingrese el número de contrato:");
        numeroContratoGlobal = numeroContrato;
        if (numeroContrato) {
            BuscarEstadoNotificacion(numeroContrato, "", 2);
        } else {
            console.error("Número de contrato no proporcionado");
        }
    } else {
        console.error("Número de botón no válido");
    }
}



/* =================================================================|
 *          Mover de pagina a la izquierda o derecha
 * ================================================================*/
// Función para ir a la página anterior
function irAPaginaAnterior(pagina) {
    switch (pagina) {
        case 1:
            break;
        case 2:
            VerForm1();
            break;
    }
}
// Función para ir a la página siguiente
function irAPaginaSiguiente(pagina) {
    switch (pagina) {
        case 1:
            VerForm2();
            break;
        case 2:
            break;
    }
}

let currentPage = 1;


// Función para verificar el usuario y mostrar/ocultar pestañas
function verificarUsuario() {

    if (opcionUsuario === 1) {
        VerForm1();
    } else if (opcionUsuario === 2) {                
        VerForm1();
        //deshabilitarInputs();
    } else if (opcionUsuario === 3) {
        //BuscarEstadoNotificacion();
        VerForm1();
        document.getElementById("btn-cargaContrato1").style.display = "none";
        document.getElementById("btn-paginas1").style.display = "none";
    }    
}


//----------------  Activar Pestaña 1 ------------------------------------
function VerForm1() {
    document.getElementById("pestaniaRoja").style.display = "block";
    document.getElementById("pestaniaRegistro").style.display = "none"; //Ya no usamos aqui
    document.getElementById("pestaniaFechas").style.display = "block";
    document.getElementById("pestaniaCafe").style.display = "none";
    document.getElementById("pestaniaCargaInfo").style.display = "none";

    document.getElementById("btn-cargaContrato1").style.display = "none";
    document.getElementById("btn-paginas1").style.display = "block";
    document.getElementById("btn-cargaContrato2").style.display = "none";
    document.getElementById("btn-paginas2").style.display = "none";
}

//----------------  Activar Pestaña 2 -------------------------------------
function VerForm2() {
    document.getElementById("pestaniaRoja").style.display = "none";
    document.getElementById("pestaniaRegistro").style.display = "none";  //Ya no usamos aqui
    document.getElementById("pestaniaFechas").style.display = "block";
    document.getElementById("pestaniaCafe").style.display = "block";
    document.getElementById("pestaniaCargaInfo").style.display = "block";

    document.getElementById("btn-cargaContrato1").style.display = "none";
    document.getElementById("btn-paginas1").style.display = "none";
    document.getElementById("btn-cargaContrato2").style.display = "flex";
    document.getElementById("btn-paginas2").style.display = "block";
}

//----------------  Activar Parte 3 -------------------------------------
function VerForm3() {
    //document.getElementById("pestaniaRoja").style.display = "block";
    document.getElementById("pestaniaRegistro").style.display = "none"; //Ya no usamos aqui
    //document.getElementById("pestaniaFechas").style.display = "none";
    //document.getElementById("pestaniaCafe").style.display = "block";
    document.getElementById("pestaniaCargaInfo").style.display = "block";

    document.getElementById("btn-cargaContrato1").style.display = "none";
    document.getElementById("btn-paginas1").style.display = "none";
    document.getElementById("btn-cargaContrato2").style.display = "flex";
    document.getElementById("btn-paginas2").style.display = "block";
}


// funcion para deshabilitar algunos inputs o textarea o botones
function deshabilitarInputs() {
    // Obtener el div exterior
    var exteriorDiv = document.getElementById('pestaniaRoja');

    // Obtener todos los inputs, textareas y botones dentro del div exterior
    var elements = exteriorDiv.querySelectorAll('input, textarea, button');

    // Recorrer todos los elementos
    elements.forEach(function (element) {
        // Verificar si el elemento está dentro de datos-otros o datos-gerentes
        var datosGerentesAncestor = element.closest('.datos-gerentes');

        // Verificar si el elemento está dentro de un div con la clase 'btn-group'
        var btnGroupAncestor = element.closest('.btn-group');

        // Si no está dentro de datos-otros o datos-gerentes, deshabilitarlo y aplicar estilos
        if (!datosGerentesAncestor) {
            element.disabled = true; // Usar readOnly en lugar de disabled
            element.style.color = 'white'; // Cambiar color de texto
            element.addEventListener('focus', function () {
                this.blur(); // Evitar que se enfoque en el elemento
            });

            // Aplicar el color de fondo solo si el elemento no está dentro de un .btn-group
            if (!btnGroupAncestor) {
                element.style.backgroundColor = 'rgba(15, 15, 15, 0.5)'; // Cambiar color de fondo
            }
        }
    });
}



/*==========================================================================*
 *      Manejamos el formato del valor total del contrato usando . , $      
 * =========================================================================*/
/*function formatearValor(input) {
    // Implementación de la función aquí
    let valor = input.value;
    valor = valor.replace(/[^\d,.]/g, '');
    let partes = valor.split('.');
    let parteEntera = partes[0] || '';
    let parteDecimal = partes[1] || '';
    parteDecimal = parteDecimal.slice(0, 2);
    let valorFormateado = parteEntera;
    if (parteDecimal !== '') {
        valorFormateado += ',' + parteDecimal;
    }
    input.value = '$' + valorFormateado;
}*/
function formatearValor(input) {
    // Eliminar caracteres no numéricos (excepto los que forman parte del número)
    let valor = input.value.replace(/[^0-9]/g, '');

    // Si el valor está vacío, salir de la función
    if (valor === '') {
        input.value = '';
        return;
    }

    // Convertir el valor a número entero (sin decimales)
    let valorNumerico = parseInt(valor);

    // Formatear el número con separadores de miles
    let valorFormateado = valorNumerico.toLocaleString('es-ES');

    // Agregar el símbolo de $
    input.value = '$' + valorFormateado;
}



//  Función para mostrar mensaje modal y marcar como archivo cargado
function MensajeCargaArchivo(archivo, textoTema) {
    //var texto = document.getElementsByClassName("tituloTema");
    $('#msgCargarArchivos').modal('show');
    document.getElementById("txtnombreVentana").textContent = textoTema;
    document.getElementById("txtnombreArchivo").textContent = archivo;
}

function confirmacionCargaArchivo(idBoton) {

    var archivosSeleccionados = $('#archivosAdjuntos')[0].files;
    for (var i = 0; i < archivosSeleccionados.length; i++) {
        archivosTemporales.push(archivosSeleccionados[i]);
    }
    var mensaje = document.getElementById(idBoton);
    if (mensaje) {
        mensaje.innerHTML = "<i class='glyphicon glyphicon-link'></i> Archivo cargado";
        mensaje.classList.remove("btn-primary");
        mensaje.classList.add("btn-success");
    }
    $('#msgCargarArchivos').modal('hide');
}


// INGRESAR SOLO MAYUSCOLAS EN LOS INGRESOS
function convertirAMayusculas(input) {
    input.value = input.value.toUpperCase();
}


//--------------------    Obtenemos todos los clientes de la base de datos    ---------------------------------------
function BuscarCliente2() {

    if ($('#txtCliente').val().length > 4) {
        idSeleccionado = 0;
        ObtenerListaClientes(2, $('#txtCliente').val(), 4);
    }
    else {
        document.getElementById("comboClientes2").style.display = "none";
    }
}
function CargarCliente2(ID, NOMBRE) {
    $('#txtCliente').val(NOMBRE);
    idCliente2 = ID;
    document.getElementById("comboClientes2").style.display = "none";
}
function ObtenerListaClientes(tipo, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusquedaCli", idproceso);
}




/*============================================================================
 *           Función para manejar la selección de opción Si / NO 
 *===========================================================================*/
function toggleButtonColor(buttonId) {
    // Obtiene el elemento del botón según su ID
    var button = document.getElementById(buttonId);

    // Verifica si el botón existe
    if (!button) {
        console.error(`Button with id "${buttonId}" not found.`);
        return;
    }

    // Variable para almacenar el ID del otro botón (para alternar entre "Si" y "No")
    var otherButtonId;

    // Comprueba el ID del botón y establece el texto correspondiente
    if (buttonId === 'siHardware') {
        otherButtonId = 'noHardware';
        txtHardware = "SI";
    } else if (buttonId === 'noHardware') {
        otherButtonId = 'siHardware';
        txtHardware = "NO";
    } else if (buttonId === 'siServiciosDOS') {
        otherButtonId = 'noServiciosDOS';
        txtServiciosDOS = "SI";
    } else if (buttonId === 'noServiciosDOS') {
        otherButtonId = 'siServiciosDOS';
        txtServiciosDOS = "NO";
    } else if (buttonId === 'siServiciosExt') {
        otherButtonId = 'noServiciosExt';
        txtServiciosExt = "SI";
    } else if (buttonId === 'noServiciosExt') {
        otherButtonId = 'siServiciosExt';
        txtServiciosExt = "NO";
    } else if (buttonId === 'siPolizas') {
        otherButtonId = 'noPolizas';
        txtPolizas = "SI";
    } else if (buttonId === 'noPolizas') {
        otherButtonId = 'siPolizas';
        txtPolizas = "NO";
    } else if (buttonId === 'siTDR') {
        otherButtonId = 'noTDR';
        txtTDR = "SI";
    } else if (buttonId === 'noTDR') {
        otherButtonId = 'siTDR';
        txtTDR = "NO";
    } else if (buttonId === 'siPreguntas') {
        otherButtonId = 'noPreguntas';
        txtPreguntas = "SI";
    } else if (buttonId === 'noPreguntas') {
        otherButtonId = 'siPreguntas';
        txtPreguntas = "NO";
    } else if (buttonId === 'siAdj') {
        otherButtonId = 'noAdj';
        txtActAdj = "SI";
    } else if (buttonId === 'noAdj') {
        otherButtonId = 'siAdj';
        txtActAdj = "NO";
    } else if (buttonId === 'siNeg') {
        otherButtonId = 'noNeg';
        txtActNeg = "SI";
    } else if (buttonId === 'noNeg') {
        otherButtonId = 'siNeg';
        txtActNeg = "NO";
    } else if (buttonId === 'siGarantiasFIN') {
        otherButtonId = 'noGarantiasFIN';
        txtGarantiasFIN = "SI";
    } else if (buttonId === 'noGarantiasFIN') {
        otherButtonId = 'siGarantiasFIN';
        txtGarantiasFIN = "NO";
    } else if (buttonId === 'siGarantiasTEC') {
        otherButtonId = 'noGarantiasTEC';
        txtGarantiasTEC = "SI";
    } else if (buttonId === 'noGarantiasTEC') {
        otherButtonId = 'siGarantiasTEC';
        txtGarantiasTEC = "NO";
    } else if (buttonId === 'siLicenciaTemporales') {
        otherButtonId = 'noLicenciaTemporales';
        txtLicenciaTemporales = "SI";
    } else if (buttonId === 'noLicenciaTemporales') {
        otherButtonId = 'siLicenciaTemporales';
        txtLicenciaTemporales = "NO";
    }

    // Obtiene el elemento del otro botón
    var otherButton = document.getElementById(otherButtonId);

    // Verifica si el otro botón existe
    if (otherButton && otherButton.classList.contains("active")) {
        otherButton.classList.remove("active");
        otherButton.style.backgroundColor = ""; // Restablecer el color de fondo
    }

    // Si el botón actual no tiene la clase "active", la agrega y cambia el color de fondo y texto
    if (!button.classList.contains("active")) {
        button.classList.add("active");
        button.style.backgroundColor = "#7E38D8"; // Cambiar el color de fondo
        button.style.color = "#FFFFFF"; // Cambiar el color del texto
    }

     /* Funcion para bloquear los textarea y botones si se ha seleccionado "No" */
    // Agregar esta parte para deshabilitar los textarea y botones correspondientesn /-------------------------------  revisar
    if (buttonId.endsWith('No')) {
        switch (buttonId) {
            case 'noHardware':
                document.getElementById('txtHardwareObs').disabled = true;
                break;
            case 'noServiciosDOS':
                document.getElementById('txtServiciosDOSObs').disabled = true;
                document.getElementById('btnServDOS').disabled = true;
                break;
            case 'noServiciosExt':
                document.getElementById('txtServiciosExternosObs').disabled = true;
                document.getElementById('btnServExternos').disabled = true;
                break;
            case 'noTDR':
                document.getElementById('txtTerminosObs').disabled = true;
                document.getElementById('btnTerminosTDR').disabled = true;
                break;
            case 'noPreguntas':
                document.getElementById('txtActaPreguntasObs').disabled = true;
                document.getElementById('btnActaPreguntas').disabled = true;
                break;
            case 'noAdj':
                document.getElementById('txtActaAdjObs').disabled = true;
                document.getElementById('btnActaAdjudicacion').disabled = true;
                break;
            case 'noNeg':
                document.getElementById('txtActaNegObs').disabled = true;
                document.getElementById('btnActaNegociacion').disabled = true;
                break;
            case 'noGarantiasFIN':
                document.getElementById('txtGarFinObs').disabled = true;
                break;
            case 'noGarantiasTEC':
                document.getElementById('txtGarTECObs').disabled = true;
                document.getElementById('btnGarantiasTEC').disabled = true;
                break;
            default:
                // Habilitar todos los textarea y botones
                document.querySelectorAll('textarea, button').forEach(element => {
                    element.disabled = false;
                });
        }
        
    } else {
        // Habilitar todos los textarea y botones
        document.querySelectorAll('textarea, button').forEach(element => {
            element.disabled = false;
        });
    }
}


// Función para inicializar el estado de un botón basándose en su valor
function initializeButtonState(buttonName, value) {
    if (value == null) {
        //console.warn(`Valor es nulo o indefinido para el botón ${buttonName}`);
        return;
    }
    if (typeof value !== 'string') {
        //console.error(`Valor inválido: "${value}". Se esperaba una cadena.`);
        return;
    }

    var normalizedValue = value.trim().toUpperCase();
    var buttonId;

    if (normalizedValue === 'SI') {
        buttonId = 'si' + buttonName;
    } else if (normalizedValue === 'NO') {
        buttonId = 'no' + buttonName;
    } else {
        console.error(`Invalid value: "${value}". Expected "SI" or "NO".`);
        return;
    }
    toggleButtonColor(buttonId);
}

// Función auxiliar para obtener valores de json y manejarlos adecuadamente
function safeInitializeButtonState(buttonName, value) {
    if (value == null) {
        return; // Regresa inmediatamente si value es nulo o indefinido
    }
    initializeButtonState(buttonName, value);
}





/* =============================================================================
 *      funcion para cargar info de contrato y saber si tiene permiso
 * ============================================================================*/
function BuscarEstadoNotificacion(contrato, descripcion, opcion) {
    //var nContrato = document.getElementById("txtNomContacto").val();

    var nContrato = '';
    var Datos = '';
    var table = '';

    if (opcion == 1) {
        nContrato = 'NUEVO';
        Datos = "[{ \"action\": \"BuscarContrato\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + nContrato + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
        table = "tableSelectBusquedaContrato";
        CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, table, 0);
    } else if (opcion == 2) {
        nContrato = contrato;
        Datos = "[{ \"action\": \"BuscarContrato\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + nContrato + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
        table = "tableSelectBusquedaContrato";
        CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, table, 0);
    }
    else if (opcion == 3) {
        nContrato = contrato;
        Datos = "[{ \"action\": \"BuscarContrato\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + nContrato + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
        table = "tableSelectContratoPDF";
        CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, table, 0);
    }

}



/* =================================================================================
 *                      Cargar datos de la base al InfoContrato
 * ================================================================================*/
function CargarPagina(div, url, datos, tipoControl, boton, idSeleccionado) {

    if (div != undefined) {
        $.ajax({
            type: "POST",
            url: url,
            data: datos,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function (respuesta) {
                $("#divMensajes").html("Cargando Información...");
            },
            success: function (respuesta) {
                if (respuesta != null) {
                    var idTotalRegistro = respuesta.length;
                    if (respuesta.length == 0) {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
                    }
                    else {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
                    }
                } else {
                    $(div).html("No existen datos para esta consulta.");
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

    if (tipoControl == "tableSelectBusquedaContrato") {
        contenido = RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectBusquedaCli") {
        contenido = RecorreJSONTableSelectBusquedaCli(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectBusquedaOS") {
        contenido = RecorreJSONobtenerOrdenesServicioBD(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    //--------------PDF-------------
    if (tipoControl == "tableSelectContratoPDF") {
        contenido = RecorreJSONTableSelectContratoPDF(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    return contenido;
}
function RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado) {

    //console.log(json);

    dtContratos(json);
}
function dtContratos(json) {
    // Si el json es un string, intenta parsearlo
    if (typeof json === 'string') {
        try {
            json = JSON.parse(json);
        } catch (e) {
            console.error("El JSON proporcionado no es válido");
            return;
        }
    }

    opcionUsuario = json.opcion;

    verificarUsuario();

    // Carga los datos de la base en los input del HTML
    let Cliente = json.CLIENTE || '';
    document.getElementById("txtCliente").value = Cliente;
    let NumContrato = json.NUM_CONTRATO || '';
    document.getElementById("txtNumContrato").value = NumContrato;
    let NumContratoObs = json.OBS_NUM_CONTRATO || '';
    document.getElementById("txtNumContratoObs").value = NumContratoObs;
    let valorContratoDecimal = json.VALOR_TOTAL_CONTRATO || '';
    let valorContrato = formatDecimalForDisplay(valorContratoDecimal);
    //document.getElementById("txtValorContrato").value = valorContrato;
    document.getElementById('txtValorContrato').value = valorContrato.integerPart;
    document.getElementById('txtDecimales').value = valorContrato.decimalPart;

    let valorContratoObs = json.OBS_VALOR_TOTAL || '';
    document.getElementById("txtValorContratoObs").value = valorContratoObs;

    let nomContacto = json.CLI_NOMBRE || '';
    document.getElementById("txtNomContacto").value = nomContacto;
    let telfContacto = json.CLI_TELEFONO || '';
    document.getElementById("txtTelefono").value = telfContacto;
    let dirContacto = json.CLI_DIRECCION || '';
    document.getElementById("txtDireccion").value = dirContacto;
    let correoContacto = json.CLI_CORREO || '';
    document.getElementById("txtCorreo").value = correoContacto;

    let objeto = json.OBJETO || '';
    document.getElementById("txtObjeto").value = objeto;
    let objetoObs = json.OBS_OBJETO || '';
    document.getElementById("txtObjetoObs").value = objetoObs;
    let alcance = json.ALCANCE || '';
    document.getElementById("txtAlcance").value = alcance;
    let alcanceObs = json.OBS_ALCANCE || '';
    document.getElementById("txtAlcanceObjetoObs").value = alcanceObs;
    let hardwareObs = json.OBS_HARDWARE || '';
    document.getElementById("txtHardwareObs").value = hardwareObs;
    let licencias = json.LICENCIAS || '';
    document.getElementById("txtLicencias").value = licencias;
    let licenciasObs = json.OBS_LICENCIAS || '';
    document.getElementById("txtLicenciasObs").value = licenciasObs;
    let servFab = json.SERVICIOS_FABRICANTE || '';
    document.getElementById("txtServiciosFab").value = servFab;
    let servFabObs = json.OBS_SERVICIOS_FABRICANTE || '';
    document.getElementById("txtServiciosFabObs").value = servFabObs;
    let servDosObs = json.OBS_SERVICIO_DOS || '';
    document.getElementById("txtServiciosDOSObs").value = servDosObs;
    let servExtObs = json.OBS_SERVICIO_EXTERNOS || '';
    document.getElementById("txtServiciosExternosObs").value = servExtObs;
    let PolizasObs = json.OBS_POLIZAS || '';
    document.getElementById("txtPolizasObs").value = PolizasObs;
    let otraterminosTDRObs = json.OBS_TERMINOS_TDR || '';
    document.getElementById("txtTerminosObs").value = otraterminosTDRObs;
    let formasPagoObs = json.FORMA_PAGO || '';
    document.getElementById("txtOtraFormaPago").value = formasPagoObs;
    let obsformasPagoObs = json.OBS_FORMA_PAGO || '';
    document.getElementById("txtFormaPagoObs").value = obsformasPagoObs;

    let actaPregObs = json.OBS_ACTA_PREGUNTAS || '';
    document.getElementById("txtActaPreguntasObs").value = actaPregObs;
    let actaAdjObs = json.OBS_ACTA_ADJUDICACION || '';
    document.getElementById("txtActaAdjObs").value = actaAdjObs;
    let actaNegObs = json.OBS_ACTA_NEGOCIACION || '';
    document.getElementById("txtActaNegObs").value = actaNegObs;
    let bomSolucion = json.BOM_SOLUCION || '';
    document.getElementById("txtBomSolucion").value = bomSolucion;
    let bomSolucionObs = json.OBS_BOM_SOLUCION || '';
    document.getElementById("txtBomSolucionObs").value = bomSolucionObs;
    let acuerdMay = json.OBS_ACUERDOS_MAY || '';
    document.getElementById("txtAcuMayoristasObs").value = acuerdMay;
    let acuerdMayObs = json.ACUERDOS_MAY || '';
    document.getElementById("txtAcuMayoristas").value = acuerdMayObs;
    let acuerdFab = json.ACUERDOS_FAB || '';
    document.getElementById("txtAcuFabricantes").value = acuerdFab; 
    let acuerdFabObs = json.OBS_ACUERDOS_FAB || '';
    document.getElementById("txtAcuFabricantesObs").value = acuerdFabObs;

    let genPedidos = json.GENERACION_PEDIDOS || '';
    document.getElementById("txtGenPedidos").value = genPedidos; 
    let obsgenPedidos = json.OBS_GENERACION_PEDIDOS || '';
    document.getElementById("txtGenPedidosObs").value = obsgenPedidos;
    
    let obsGarFin = json.OBS_GARANTIAS_FIN || '';
    document.getElementById("txtGarFinObs").value = obsGarFin;
    let obsGarTec = json.OBS_GARANTIAS_TEC || '';
    document.getElementById("txtGarTECObs").value = obsGarTec;

    let fechaSuscCon = json.FECHA_SUSCRIPCION_CONTRATO || '';
    if (fechaSuscCon) {
        fechaSuscCon = convertirFecha(fechaSuscCon);
        document.getElementById("fechaSuscripContrato").value = fechaSuscCon;
    }

    let fechaNotAnt = json.FECHA_NOTIF_ANTICIPO || '';
    if (fechaNotAnt) {
        fechaNotAnt = convertirFecha(fechaNotAnt);
        document.getElementById("fechaNotifAnticipo").value = fechaNotAnt;
    }

    let fechaIniGar = json.FECHA_INICIO_GARANTIA || '';
    if (fechaIniGar) {
        fechaIniGar = convertirFecha(fechaIniGar);
        document.getElementById("fechaIniActivacion").value = fechaIniGar;
    }

    let fechaFinGar = json.FECHA_FIN_GARANTIA || '';
    if (fechaFinGar) {
        fechaFinGar = convertirFecha(fechaFinGar);
        document.getElementById("fechaFinActivacion").value = fechaFinGar;
    }

    let plazoActGar = json.PLAZO_ACTIVACION || '';
    document.getElementById("txtPlazoActGarantia").value = plazoActGar;

    let plazoActLic = json.PLAZO_ACTIVACION_LIC || '';
    document.getElementById("txtPlazoActLicencia").value = plazoActLic;

    let duracionVigTec = json.DURACION_VIGENCIA_TEC || '';
    document.getElementById("txtDuracionVigTec").value = duracionVigTec;

    // Llama a la función para inicializar el estado de los botones, Se debe iniciar aqui los botones de Si y No para que cambien de color
    initializeButtonState('Hardware', json.HARDWARE);
    initializeButtonState('ServiciosDOS', json.SERVICIO_DOS);
    initializeButtonState('ServiciosExt', json.SERVICIO_EXTERNOS);
    initializeButtonState('Polizas', json.POLIZAS);
    initializeButtonState('TDR', json.TERMINOS_TDR);
    initializeButtonState('Preguntas', json.ACTA_PREGUNTAS);
    initializeButtonState('Adj', json.ACTA_ADJUDICACION);
    initializeButtonState('Neg', json.ACTA_NEGOCIACION);
    initializeButtonState('GarantiasFIN', json.GARANTIAS_FIN);
    initializeButtonState('GarantiasTEC', json.GARANTIAS_TEC);
    initializeButtonState('LicenciaTemporales', json.ENTREGA_LIC_TEMPORALES);

    //deshabilitarInputs();

}

function RecorreJSONTableSelectBusquedaCli(json, boton, idSeleccionado) {

    var x = "";
    if (idSeleccionado == 4) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarCliente2(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboClientes2").innerHTML = x;
        document.getElementById("comboClientes2").style.display = "block";
    }
}


function convertirFecha(fechaString) {
    // Dividir la cadena de fecha y hora en solo la fecha
    let [date] = fechaString.split(' ');

    // Dividir la fecha en componentes día, mes, año
    let [day, month, year] = date.split('/');

    // Formatear la fecha en el formato compatible con input date (yyyy-mm-dd)
    return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
}


function cargarArchivosAlServidor(codigoContrato) {
    var mensajeVerificacion = "";
    var url = 'CargaArchivos.ashx';

    /*if ($('#txtNumContrato').val() == "") {
        mensajeVerificacion += "   Debe ingresar el Numero de Contrato ";
        contadorVerificacion += 1;
    }*/       

    /*if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }*/

    // Verificar si hay archivos para enviar
    if (archivosTemporales.length > 0) {
        var fileData = new FormData();

        // Looping over all files in archivosTemporales and add them to FormData object
        for (var i = 0; i < archivosTemporales.length; i++) {
            fileData.append(archivosTemporales[i].name, archivosTemporales[i]);
        }

        // Adding additional keys to FormData object
        fileData.append('session', $("#ContentPlaceHolder1_txtUsuario").val());
        fileData.append('action', 'CargarArchivosInfoContratos');
        fileData.append('Id_RegTareas', 0);
        fileData.append('idServicio', "1");
        fileData.append('codContrato', codigoContrato);

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
                var mesnajeError = "La acción de cargar de archivo está tomando demasiado tiempo. Verifique su conexión de red y luego intente nuevamente cargar los archivos.";
                MensajeIncorrecto(mesnajeError);
            }
        });
        var mensajeExito = "Se han cargado correctamente los archivos seleccionados.";
        MensajeCorrecto(mensajeExito);
    } else {
        var mensajeError = "No se han seleccionado archivos para cargar.";
        MensajeIncorrecto(mensajeError);
    }
}

function GuardarContrato(codigoContrato) {

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;


    /*if ($('#txtCliente').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Cliente";
        contadorVerificacion += 1;
    }

    if ($('#txtNomContacto').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre del Contacto ";
        contadorVerificacion += 1;
    }
    if ($('#txtTelefono').val() == "") {
        mensajeVerificacion += "- Debe ingresar el teléfono de Contacto ";
        contadorVerificacion += 1;
    }
    if ($('#txtDireccion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la dirección del Contacto ";
        contadorVerificacion += 1;
    }
    if ($('#txtCorreo').val() == "") {
        mensajeVerificacion += "- Debe ingresar el correo de Contacto ";
        contadorVerificacion += 1;
    }
    if ($('#txtNumContrato').val() == "") {
        mensajeVerificacion += "- Debe ingresar el número de Contrato";
        contadorVerificacion += 1;
    }
    if ($('#txtValorContrato').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Valor TOTAL del Contrato";
        contadorVerificacion += 1;
    }
    if ($('#txtObjeto').val() == "") {
        mensajeVerificacion += "- Debe ingresar el objeto";
        contadorVerificacion += 1;
    }
    if ($('#txtAlcance').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Alcance o Resumen";
        contadorVerificacion += 1;
    }
    if ($('#txtHardware').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Hardware'";   
        contadorVerificacion += 1;
    }
    if ($('#txtServiciosDOS').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Servicios DOS'";
        contadorVerificacion += 1;
    }
    if ($('#txtServiciosExt').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Servicios Externos'";
        contadorVerificacion += 1;
    }
    if ($('#txtPolizas').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Polizas'";
        contadorVerificacion += 1;
    }
    if ($('#txtTDR').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Términos de referencia - TDR's'";
        contadorVerificacion += 1;
    }
    if ($('#txtPreguntas').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Acta preguntas y respuestas'";
        contadorVerificacion += 1;
    }
    if ($('#txtActAdj').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Acta de adjudicación'";
        contadorVerificacion += 1;
    }
    if ($('#txtActNeg').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Acta de negociación'";
        contadorVerificacion += 1;
    }
    if ($('#txtGarantiasFIN').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Garantías FIN'";
        contadorVerificacion += 1;
    }
    if ($('#txtGarantiasTEC').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Garantías y Licencias TEC'";
        contadorVerificacion += 1;
    }
    if ($('#txtLicenciaTemporales').val() == "") {
        mensajeVerificacion += "- Debe seleccionar SI o No en 'Entrega de Licencias temporales'";
        contadorVerificacion += 1;
    }
    if ($('#txtLicencias').val() == "") {
        mensajeVerificacion += "- Debe ingresar 'Licencias'";
        contadorVerificacion += 1;
    }
    if ($('#txtServiciosFab').val() == "") {
        mensajeVerificacion += "- Debe ingresar los Servicios de fabricante";
        contadorVerificacion += 1;
    }
    if ($('#txtBomSolucion').val() == "") {
        mensajeVerificacion += "- Debe ingresar BoM Solución";
        contadorVerificacion += 1;
    }
    if ($('#txtAcuMayoristas').val() == "") {
        mensajeVerificacion += "- Debe ingresar Acuerdos Mayoristas";
        contadorVerificacion += 1;
    }
    if ($('#txtAcuFabricantes').val() == "") {
        mensajeVerificacion += "- Debe ingresar Acuerdos Fabricante";
        contadorVerificacion += 1;
    }
    if ($('#selectFormaPago').val() == "") {
        mensajeVerificacion += "- Debe seleccionar la Forma de pago";
        contadorVerificacion += 1;
    }
    if ($('#txtGenPedidos').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Generación de Pedidos y Ordenes Servicio";
        contadorVerificacion += 1;
    } 
    
    if ($('#fechaSuscripContrato').val() == "") {
        mensajeVerificacion += "- Debe ingresar la FECHA de suscripción del Contrato";
        contadorVerificacion += 1;
    }*/

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }
       
    // Ingreso para datos preocupacional

    var datosFormulario = "";

    datosFormulario = {
        'formulario': 1,
        'session': $("#ContentPlaceHolder1_txtUsuario").val(),
        'txtCliente': $('#txtCliente').val(),
        'txtNomContacto': $('#txtNomContacto').val(),
        'txtTelefono': $('#txtTelefono').val(),
        'txtDireccion': $('#txtDireccion').val(),
        'txtCorreo': $('#txtCorreo').val(),
        'txtNumContrato': $('#txtNumContrato').val(),
        'txtValorContrato': getDecimalValue(),
        'txtObjeto': $('#txtObjeto').val(),
        'txtAlcance': $('#txtAlcance').val(),
        'txtHardware': txtHardware,
        'txtServiciosDOS': txtServiciosDOS,
        'txtServiciosExt': txtServiciosExt,
        'txtPolizas': txtPolizas,
        'txtTDR': txtTDR,
        'txtPreguntas': txtPreguntas,
        'txtActAdj': txtActAdj,
        'txtActNeg': txtActNeg,
        'txtGarantiasFIN': txtGarantiasFIN,
        'txtGarantiasTEC': txtGarantiasTEC,
        'txtLicenciaTemporales': txtLicenciaTemporales,
        'txtLicencias': $('#txtLicencias').val(),
        'txtServiciosFab': $('#txtServiciosFab').val(),
        'txtBomSolucion': $('#txtBomSolucion').val(),
        'txtAcuMayoristas': $('#txtAcuMayoristas').val(),
        'txtAcuFabricantes': $('#txtAcuFabricantes').val(),
        'selectFormaPago': getTableData(),
        'txtGenPedidos': $('#txtGenPedidos').val(),
        'fechaSuscripContrato': $('#fechaSuscripContrato').val(), 
        'fechaNotifAnticipo': $('#fechaNotifAnticipo').val(), 
        'fechaIniActivacion': $('#fechaIniActivacion').val(), 
        'fechaFinActivacion': $('#fechaFinActivacion').val(),
        'txtPlazoActGarantia': $('#txtPlazoActGarantia').val(),
        'txtPlazoActLicencia': $('#txtPlazoActLicencia').val(),
        'txtDuracionVigTec': $('#txtDuracionVigTec').val(),

        'txtNumContratoObs': $('#txtNumContratoObs').val(), 
        'txtValorContratoObs': $('#txtValorContratoObs').val(),
        'txtObjetoObs': $('#txtObjetoObs').val(),
        'txtAlcanceObjetoObs': $('#txtAlcanceObjetoObs').val(),
        'txtHardwareObs': $('#txtHardwareObs').val(),
        'txtLicenciasObs': $('#txtLicenciasObs').val(),
        'txtServiciosFabObs': $('#txtServiciosFabObs').val(),
        'txtServiciosDOSObs': $('#txtServiciosDOSObs').val(),
        'txtServiciosExternosObs': $('#txtServiciosExternosObs').val(),
        'txtPolizasObs': $('#txtPolizasObs').val(),
        'txtTerminosObs': $('#txtTerminosObs').val(),
        'txtActaPreguntasObs': $('#txtActaPreguntasObs').val(),
        'txtActaAdjObs': $('#txtActaAdjObs').val(),
        'txtActaNegObs': $('#txtActaNegObs').val(),
        'txtBomSolucionsObs': $('#txtBomSolucionObs').val(),
        'txtAcuMayoristasObs': $('#txtAcuMayoristasObs').val(),
        'txtAcuFabricantesObs': $('#txtAcuFabricantesObs').val(),
        'txtFormaPagoObs': $('#txtFormaPagoObs').val(),
        'txtGarFinObs': $('#txtGarFinObs').val(),
        'txtGarTECObs': $('#txtGarTECObs').val(),
        'txtGenPedidosObs': $('#txtGenPedidosObs').val(),
        'OrdenesServicio': OrdenesServicioJoin
    };

        var datos = JSON.stringify([{ 'action': 'GuardarNuevoInfoContrato', 'parameters': datosFormulario }]);

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
            var mensaje = "";
            if (respuesta.estado == "1") {
                MensajeCorrecto(respuesta.mensaje);
                //VerListaEmpleados();
                $("#divMensajes").html("");
            }
            else if (respuesta.estado == "0") {
                MensajeIncorrecto(respuesta.mensaje);
            }
        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MensajeIncorrecto(mesnajeError);
        }
    });

    cargarArchivosAlServidor(codigoContrato)

    return;
}


function calcularPorcentaje() {
    // Obtener el valor total ingresado por el usuario
    let total = parseFloat(document.getElementById('txtValorContrato').value);
    document.getElementById('txtPorcentajeMulta').value = "";

    // Calcular el 5% del valor total
    let porcentaje = total * 0.05;

    document.getElementById('txtResultadoCinco').value = porcentaje.toFixed(2);
}

function calcularMulta() {
    // Obtener el valor total ingresado por el usuario
    let total = parseFloat(document.getElementById('txtValorContrato').value);
    let porcentajeMulta = parseFloat(document.getElementById('txtPorcentajeMulta').value);

    // Calcular el valor total de multa
    let valMulta = total * (porcentajeMulta / 100);

    document.getElementById('txtMontoMulta').textContent = valMulta.toFixed(2);
}


/*function mostrarInputOtro() {
    var select = document.getElementById("selectFormaPago");
    var inputOtro = document.getElementById("txtOtraFormaPago");

    if (select.value === "5") {
        inputOtro.style.display = "block";
    } else {
        inputOtro.style.display = "none";
    }
}*/


/* ===============================================================================*
 *                 Agregar filas en la tabla de forma de pago                     *
 * ===============================================================================*/
// Primero creamos un evento para identificar que se ha realizado algun cambio en el Valor del contrato ya sea en el entero o en el decimal
document.addEventListener('DOMContentLoaded', function () {
    // Agregar evento al botón para agregar fila
    document.getElementById('addRowButton').addEventListener('click', function (event) {
        event.preventDefault();
        addRow();
    });
    // Escuchar cambios en el input de valor del contrato para actualizar valores
    document.getElementById('txtValorContrato').addEventListener('input', updateValues);
    document.getElementById('txtDecimales').addEventListener('input', updateValues);
});

// Función para agregar una nueva fila a la tabla dinámica
function addRow() {
    const table = document.getElementById('dynamicTable').getElementsByTagName('tbody')[0];
    const newRow = table.insertRow();
    const rowIndex = newRow.rowIndex;

    // Insertar celdas para descripción, porcentaje, valor y acción
    const descriptionCell = newRow.insertCell(0);
    const percentageCell = newRow.insertCell(1);
    const valueCell = newRow.insertCell(2);
    const actionCell = newRow.insertCell(3);

    // Insertar inputs y botón de eliminar
    descriptionCell.innerHTML = '<input type="text" placeholder="Descripción">';
    percentageCell.innerHTML = '<input type="number" min="0" max="100" placeholder="%" oninput="validateAndUpdate(this)">';
    valueCell.innerHTML = `
        <div style="display:flex;">
            <input id="txtValorContrato-${rowIndex}" type="text" class="form-control" placeholder="$" oninput="calcularPorcentaje();formatearValor(this)">
        </div>`;
    actionCell.innerHTML = '<button class="delete-button" onclick="deleteRow(this)"><i class="fa fa-trash-o" aria-hidden="true"></i></button>';

    updateValues(); // Actualizar valores después de agregar la fila
}

// Función para validar y actualizar el porcentaje ingresado
function validateAndUpdate(input) {
    const txtValorContrato = parseFloat(document.getElementById('txtValorContrato').value.replace(/\./g, '').replace('$', '')) || 0;
    const txtDecimales = parseFloat(document.getElementById('txtDecimales').value) || 0;
    const valorContrato = parseFloat(`${txtValorContrato}.${txtDecimales}`);

    const table = document.getElementById('dynamicTable').getElementsByTagName('tbody')[0];
    const rows = table.getElementsByTagName('tr');

    let totalPercentage = 0;

    // Calcular el total de porcentajes ingresados en la tabla
    for (let i = 0; i < rows.length; i++) {
        const percentageInput = rows[i].cells[1].getElementsByTagName('input')[0];
        if (percentageInput !== input) {
            totalPercentage += parseFloat(percentageInput.value) || 0;
        }
    }

    // Validar que el total de porcentajes no exceda el 100%
    const inputPercentage = parseFloat(input.value) || 0;
    if (totalPercentage + inputPercentage > 100) {
        alert('El porcentaje total no puede exceder el 100%.');
        input.value = '';
    }

    updateValues(); // Actualizar valores después de validar y actualizar
}

// Función para actualizar los valores calculados en la tabla
function updateValues() {
    const txtValorContrato = parseFloat(document.getElementById('txtValorContrato').value.replace(/\./g, '').replace('$', '')) || 0;
    const txtDecimales = parseFloat(document.getElementById('txtDecimales').value) || 0;
    const valorContrato = parseFloat(`${txtValorContrato}.${txtDecimales}`);

    const table = document.getElementById('dynamicTable').getElementsByTagName('tbody')[0];
    const rows = table.getElementsByTagName('tr');

    let totalUsed = 0;

    // Calcular y actualizar los valores en cada fila de la tabla
    for (let i = 0; i < rows.length; i++) {
        const percentageInput = rows[i].cells[1].getElementsByTagName('input')[0];
        const valueInput = rows[i].cells[2].getElementsByTagName('input')[0];

        const percentage = parseFloat(percentageInput.value) || 0;
        const calculatedValue = (valorContrato * percentage) / 100;

        valueInput.value = calculatedValue.toFixed(2);
        totalUsed += calculatedValue;
    }

    // Calcular el valor restante y mostrarlo
    const remainingValue = valorContrato - totalUsed;
    document.getElementById('remainingValue').innerText = `Valor restante: $${remainingValue.toFixed(2)}`;
}

// Función para eliminar una fila de la tabla
function deleteRow(button) {
    const row = button.parentNode.parentNode;
    row.parentNode.removeChild(row);
    updateValues(); // Actualizar valores después de eliminar la fila
}

// Función para obtener el valor formateado de los inputs de valor y decimales
function getFormattedValue() {
    const valorContratoInput = document.getElementById('txtValorContrato');
    const decimalesInput = document.getElementById('txtDecimales');

    if (!valorContratoInput || !decimalesInput) {
        console.error('Inputs not found');
        return '';
    }

    let valorContrato = valorContratoInput.value.replace('$', '').replace(/\./g, '') || '0';
    let decimales = decimalesInput.value.padStart(2, '0') || '00'; // Asegurar que decimales tenga al menos dos dígitos

    return `${valorContrato}.${decimales}`;
}


// Función para obtener los datos de la tabla en un formato lineal separado por (;) para guardar
function getTableData() {
    const table = document.getElementById('dynamicTable').getElementsByTagName('tbody')[0];
    if (!table) {
        console.error('Table not found');
        return '';
    }

    const rows = table.getElementsByTagName('tr');
    let rowData = [];

    // Recorrer las filas y obtener los valores de cada celda
    for (let i = 0; i < rows.length; i++) {
        const cells = rows[i].getElementsByTagName('td');
        let rowValues = [];

        // Recorrer las celdas de cada fila (excluir la última celda de acción)
        for (let j = 0; j < cells.length - 1; j++) {
            const input = cells[j].querySelector('input');
            let value = '';

            // Verificar si es la fila específica que contiene los dos inputs
            if (input && (input.id === 'txtValorContrato' || input.id === 'txtDecimales')) {
                value = getFormattedValue();
            } else if (input) {
                value = input.value;
            } else {
                value = cells[j].innerText; // Si no hay input, tomar el texto de la celda
            }

            rowValues.push(value);
        }

        rowData.push(rowValues.join('|')); // Separar los valores de las celdas con '|'
    }

    return rowData.join(';'); // Separar las filas con ';'
}


// Función para guardar los datos de la tabla
function saveTableData() {
    const dataToSave = getTableData();
    console.log(dataToSave); // Imprimir los datos de la tabla en formato de cadena separada por ';'
    // Aquí puedes enviar `dataToSave` a tu servidor o base de datos
}

// Funcion para obtener el Valor total del contrato concatenando el entero y el decimal
function getDecimalValue() {
    const valorContratoInput = document.getElementById('txtValorContrato');
    const decimalesInput = document.getElementById('txtDecimales');

    // Obtener los valores de los inputs
    let valorContrato = valorContratoInput.value.replace('$', '').replace(/\./g, '') || '0';
    let decimales = (decimalesInput.value.padStart(2, '0')) || '00'; // Asegurar que decimales tenga al menos dos dígitos

    // Concatenar valor entero y decimales en el formato adecuado
    const valorDecimal = `${valorContrato}.${decimales}`;

    return valorDecimal;
}

// Funcion para transformar el valor decimal que llega de la base de datos al html en 2 casillas
function formatDecimalForDisplay(decimalValue) {
    // Separar la parte entera de la parte decimal
    const parts = decimalValue.toString().split('.');
    const integerPart = parts[0];
    const decimalPart = parts.length > 1 ? parts[1] : '00';

    // Formatear la parte entera con puntos como separadores de miles
    const formattedIntegerPart = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, '.');

    // Agregar el símbolo $ al inicio de la parte entera formateada
    const formattedIntegerWithSymbol = `$${formattedIntegerPart}`;

    // Devolver ambas partes
    return {
        integerPart: formattedIntegerWithSymbol,
        decimalPart: decimalPart.padEnd(2, '0') // Asegurar que siempre haya dos dígitos en la parte decimal
    };
}



/*=======================================================================================
 *      Función para cargar los datos de forma pago a una tabla en el PDF
 *======================================================================================*/
function cargarDatosEnTabla(data) {
    const tableBody = document.querySelector('#tbl_FormasPago tbody');
    tableBody.innerHTML = ''; // Limpiar el contenido existente

    if (!data) {
        console.error('No hay datos para cargar');
        return;
    }

    const rows = data.split(';');

    rows.forEach((row, index) => {
        const cells = row.split('|');
        if (cells.length === 3) {
            const newRow = document.createElement('tr');

            // Crear celdas y añadir el contenido
            const cellIndex = document.createElement('td');
            cellIndex.textContent = index + 1;
            newRow.appendChild(cellIndex);

            const cellDescripcion = document.createElement('td');
            cellDescripcion.textContent = cells[0];
            newRow.appendChild(cellDescripcion);

            const cellPorcentaje = document.createElement('td');
            cellPorcentaje.textContent = cells[1];
            newRow.appendChild(cellPorcentaje);

            const cellValor = document.createElement('td');
            cellValor.textContent = cells[2];
            newRow.appendChild(cellValor);

            tableBody.appendChild(newRow);
        } else {
            console.error('El formato de la fila no es válido:', row);
        }
    });
}




/*=======================================================================================
 *      Funciones para obtener las ordenes de servicio y cargarlas en una lista
 *======================================================================================*/
function buscarOrdenesServicio() {

    let numPedido = document.getElementById("txtGenPedidos").value;

    if (numPedido.trim() === '') {
        actualizarListaOrdenesServicio([]);  //Actualiza el espacio  de las ordenes
        return;
    } 

    ObtenerListaOS("", numPedido , "","");


    // Simulación de datos obtenidos de la base de datos (reemplaza esta parte con tu lógica de obtención de datos)
    //const ordenesServicio = obtenerOrdenesServicioDeBaseDatos(numeroPedido);

    // Actualizar la lista de órdenes de servicio
    //actualizarListaOrdenesServicio(ordenesServicio);
}
function ObtenerListaOS(tipo, cod, descripcion, tipo2) {

    var Datos = "[{ \"action\": \"ConsultarOrdenServicioPedido\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" + cod + "\"} }]";

    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusquedaOS", tipo2);
}

function RecorreJSONobtenerOrdenesServicioBD(json, numeroPedido, idSeleccionado) {
    // Si el JSON es un string, intenta parsearlo
    if (typeof json === 'string') {
        try {
            json = JSON.parse(json);
        } catch (e) {
            console.error("El JSON proporcionado no es válido");
            return;
        }
    }

    // Obtenemos todas las órdenes de servicio de la lista
    const ordenesServicio = [];
    const ordenesServicioTexto = []; // nueva variable para almacenar los números de orden como texto

    $.each(json, function (i, item) {
        ordenesServicio.push(item.ORDEN);
        ordenesServicioTexto.push(item.ORDEN.toString()); // convertimos a string cada número de orden
    });

    OrdenesServicioJoin = ordenesServicioTexto.join(','); // unimos los números de orden con comas

    // Actualizar la lista de órdenes de servicio
    actualizarListaOrdenesServicio(json);
}

function actualizarListaOrdenesServicio(ordenesServicio) {
    const contenedor = document.getElementById('listaOrdenesContent');
    contenedor.innerHTML = ''; // Limpiar el contenido existente

    if (ordenesServicio.length === 0) {
        contenedor.style.display = 'none';
        return;
    }

    ordenesServicio.forEach(orden => {
        const item = document.createElement('div');
        item.textContent = orden.ORDEN; // Asegurarse de acceder al campo ORDEN
        contenedor.appendChild(item);
    });

    contenedor.style.display = 'block';
}

//------------------------------------------------------------------------------------------------



/***************************************************************************************
                                        PDF
 ***************************************************************************************/
function MostrarPDF() {
    $("#ModalEgresoInventarioPDF").modal('show');
    BuscarEstadoNotificacion(numeroContratoGlobal, "", 3);
}


function RecorreJSONTableSelectContratoPDF(json, boton, idSeleccionado) {
    dtContratoDatos(json);
}

function dtContratoDatos(json) {

    var Cliente = document.getElementById("txtPDFCliente");
    Cliente.textContent = json.CLIENTE;

    var numContrato = document.getElementById("txtPDFNumeroContrato");
    numContrato.textContent = json.NUM_CONTRATO;

    var valorContrato = document.getElementById("txtPDFValorTotalContrato");
    valorContrato.textContent = json.VALOR_TOTAL_CONTRATO;

    var objeto = document.getElementById("txtPDFObjeto");
    objeto.textContent = json.OBJETO;

    var servDos = document.getElementById("txtPDFServiciosDOS");
    servDos.textContent = json.SERVICIO_DOS;

    var servExt = document.getElementById("txtPDFServiciosExternos");
    servExt.textContent = json.SERVICIO_EXTERNOS;

    var alcance = document.getElementById("txtPDFAlcance");
    alcance.textContent = json.ALCANCE;

    var hardware = document.getElementById("txtPDFHardware");
    hardware.textContent = json.HARDWARE;

    var licencias = document.getElementById("txtPDFLicencias");
    licencias.textContent = json.LICENCIAS;

    var servFab = document.getElementById("txtPDFServiciosFabricante");
    servFab.textContent = json.SERVICIOS_FABRICANTE;

    var polizas = document.getElementById("txtPDFPolizas");
    polizas.textContent = json.POLIZAS;

    var terTdr = document.getElementById("txtPDFTerminosReferencia");
    terTdr.textContent = json.TERMINOS_TDR;

    var formasPago = document.getElementById("txtPDFFormasPago");
    formasPago.textContent = json.FORMA_PAGO;

    var actaPreguntas = document.getElementById("txtPDFActaPreguntasRespuestas");
    actaPreguntas.textContent = json.ACTA_PREGUNTAS;

    var actaAdj = document.getElementById("txtPDFActaAdjudicacion");
    actaAdj.textContent = json.ACTA_ADJUDICACION;

    var actaNeg = document.getElementById("txtPDFActaNegociacion");
    actaNeg.textContent = json.ACTA_NEGOCIACION;

    var bomSolucion = document.getElementById("txtPDFBoMSolucion");
    bomSolucion.textContent = json.BOM_SOLUCION;

    var acuMay = document.getElementById("txtPDFAcuerdosMayoristas");
    acuMay.textContent = json.ACUERDOS_MAY;

    var acuFab = document.getElementById("txtPDFAcuerdosFabricantes");
    acuFab.textContent = json.ACUERDOS_FAB;

    var garFin = document.getElementById("txtPDFGarantiasFIN");
    garFin.textContent = json.GARANTIAS_FIN;

    var garTec = document.getElementById("txtPDFGarantiasLicenciasTEC");
    garTec.textContent = json.GARANTIAS_TEC;

    var genPedidos = document.getElementById("txtPDFGeneracionPedidos");
    genPedidos.textContent = json.GENERACION_PEDIDOS; 

    var fecSusCont = document.getElementById("txtPDFFechaSuscripcionContrato");
    fecSusCont.textContent = json.FECHA_SUSCRIPCION_CONTRATO;

    var fecNotCont = document.getElementById("txtPDFFechaNotificacionAnticipo");
    fecNotCont.textContent = json.FECHA_NOTIF_ANTICIPO;

    var fecIniGarFab = document.getElementById("txtPDFFechaInicioGarantiaFabricante");
    fecIniGarFab.textContent = json.FECHA_INICIO_GARANTIA;

    var fecFinGarFab = document.getElementById("txtPDFFechaFinGarantiaFabricante");
    fecFinGarFab.textContent = json.FECHA_FIN_GARANTIA; 

    var nomCont = document.getElementById("txtPDFNombre");
    nomCont.textContent = json.CLI_NOMBRE;

    var telfCont = document.getElementById("txtPDFTelefono");
    telfCont.textContent = json.CLI_TELEFONO;

    var dirCont = document.getElementById("txtPDFDireccion");
    dirCont.textContent = json.CLI_DIRECCION;

    var corCont = document.getElementById("txtPDFCorreo");
    corCont.textContent = json.CLI_CORREO;

    var ordServicio = document.getElementById("txtPDFOrdenesServicio");
    ordServicio.textContent = json.ORDEN_SERVICIO;

    cargarDatosEnTabla(json.FORMA_PAGO);
}

// Crear y descargar PDF
function generatePDF() {
    const element = document.querySelector('#ModalEgresoInventarioPDF .modal-body');
    const opt = {
        margin: 0.5,
        filename: 'contrato.pdf',
        image: { type: 'jpeg', quality: 1.0 }, // Set image quality to maximum
        html2canvas: { scale: 3 }, // Increase scale for better resolution
        jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
    };

    html2pdf().from(element).set(opt).save();
}

/* =========================================================
 *              Enviar
 * =========================================================*/

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


$(function () {
    CodUsuario = $("#ContentPlaceHolder1_txtCodUnico").val();
    //BuscarEstadoNotificacion("","",1);

    calcularPorcentaje();
    //mostrarInputOtro();
    $('#btnCargarArchivosAdjuntos').click(function () {
        var archivosSeleccionados = $('#archivosAdjuntos')[0].files;
        for (var i = 0; i < archivosSeleccionados.length; i++) {
            archivosTemporales.push(archivosSeleccionados[i]);
        }
        $('#msgCargarArchivos').modal('hide');
    });
    
    //deshabilitarInputs();
    //verificarUsuario();
});