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

var numeroContratoGlobal;
var OrdenesServicioJoin = "";

var numPedido = "";

//import { jsPDF } from "jspdf";

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
        timer: 4000,
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

function alerta(respuesta) {
    Swal.fire({
        type: 'info',
        title: "Advertencia...!!!",
        text: respuesta,
        icon: "info",
        timer: 4000,
        width: '800px',
        heightAuto: false,        
    });
}


//con opción de TYPE  //tipos de popups: error, success, warning, info, question
$("#btn2").click(function () {
    Swal.fire({
        type: 'success',
        title: 'Éxito',
        text: '¡Perfecto!',
    });
});


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
    //document.getElementById("pestaniaRegistro").style.display = "none"; //Ya no usamos aqui
    document.getElementById("pestaniaFechas").style.display = "block";
    document.getElementById("pestaniaCafe").style.display = "none";
    document.getElementById("pestaniaCargaInfo").style.display = "none";

    document.getElementById("btn-cargaContrato1").style.display = "flex";
    document.getElementById("btn-paginas1").style.display = "none";
    document.getElementById("btn-cargaContrato2").style.display = "none";
    document.getElementById("btn-paginas2").style.display = "none";
    //numeroContratoGlobal = document.getElementById('txtNumContrato');
}

//----------------  Activar Pestaña 2 -------------------------------------
function VerForm2() {
    document.getElementById("pestaniaRoja").style.display = "none";
    //document.getElementById("pestaniaRegistro").style.display = "none";  //Ya no usamos aqui
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
    //document.getElementById("pestaniaRegistro").style.display = "none"; //Ya no usamos aqui
    //document.getElementById("pestaniaFechas").style.display = "none";
    //document.getElementById("pestaniaCafe").style.display = "block";
    document.getElementById("pestaniaCargaInfo").style.display = "block";

    document.getElementById("btn-cargaContrato1").style.display = "none";
    document.getElementById("btn-paginas1").style.display = "none";
    document.getElementById("btn-cargaContrato2").style.display = "flex";
    document.getElementById("btn-paginas2").style.display = "block";
}

//----------------  Activar editar o crear Proyecto -------------------------------------
function VerFormCrear() {
    expandirCarousel(290);
    document.getElementById("FormularioPrincipal").style.display = "block";
    document.getElementById("menuActionProyectos").style.display = "none";
}

function VerConsultaDocs() {
    expandirCarousel(290);    
    //MostrarModalConsulta();
    $('#ModalConsultaDocs').modal('show')
}

// INGRESAR SOLO MAYUSCOLAS EN LOS INGRESOS
function convertirAMayusculas(input) {
    input.value = input.value.toUpperCase();
}



// Funcion para saber si se desea  Crear un NUEVO contrato o EDITAR
//function manejarBoton(numeroBoton, opcionSeleccionada, numContratoMOdal) {
//    if (numeroBoton === 1) {
//        BuscarEstadoNotificacion("", "", 1);

//    } else if (numeroBoton === 2) {
//        // Pedir al usuario que ingrese el número de contrato
//        const numeroContrato = numContratoMOdal
//        if (numeroContrato) {
//            BuscarEstadoNotificacion(numeroContrato, opcionSeleccionada, numeroBoton);
//        } else {
//            console.error("Número de contrato no proporcionado");
//        }
//    } else {
//        console.error("Número de botón no válido");
//    }
//}
function manejarBoton(numeroBoton, tipoBusqueda, numContratoModal) {
    if (numeroBoton === 1) {
        BuscarEstadoNotificacion("", "", 1);

    } else if (numeroBoton === 2) {
        // Pedir al usuario que ingrese el número de contrato
        if (numContratoModal) {
            BuscarEstadoNotificacion(numContratoModal, tipoBusqueda, 2);
        } else {
            console.error("Número de contrato no proporcionado");
        }
    } else {
        console.error("Número de botón no válido");
    }
}


/*================================================================
 *      Funcion para manejar el modal de la consulta inicial     *
 *===============================================================*/
function MostrarModalConsulta() {
    $('#ModalConsulta').modal('show')
}
document.addEventListener("DOMContentLoaded", function () {
    const modal_input = document.getElementById("modal_input");
    const inputNumero = document.getElementById("numero");
    const modal_table = document.getElementById("modal_table");
    const radioButtons = document.querySelectorAll("input[name='tipoBusqueda']");
    const buscarBtnModal = document.getElementById("buscarBtnModal");

    // Función para verificar y seleccionar radio según el número encontrado
    function seleccionarBusqueda(numContrato, numPedido) {
        if (numContrato) {
            document.getElementById("OpContrato").checked = true;
            inputNumero.value = numContrato;
        } else if (numPedido) {
            document.getElementById("OpPedido").checked = true;
            inputNumero.value = numPedido;
        }
    }

    // Mostrar el input o la tabla cuando se selecciona un tipo de búsqueda
    radioButtons.forEach(radio => {
        radio.addEventListener("change", function () {
            modal_input.style.display = "none";
            modal_table.style.display = "none";
            inputNumero.classList.remove("input-error");

            if (this.value === "contrato" || this.value === "pedido") {
                modal_input.style.display = "block";
                inputNumero.placeholder = this.value === "contrato" ? "Ingrese aquí el Numero de Contrato" : "Ingrese aquí el Numero de Pedido";
            } else if (this.value === "cliente") {
                modal_table.style.display = "block";
            }
        });
    });

    // Validación del campo de entrada
    buscarBtnModal.addEventListener("click", function () {
        const tipoSeleccionado = document.querySelector("input[name='tipoBusqueda']:checked");
        const numeroContratoIngresado = inputNumero.value.trim();

        if (!tipoSeleccionado) {
            alert("Debes seleccionar un tipo de búsqueda.");
            return;
        }

        let tipoValor;
        if (tipoSeleccionado.value === "contrato") {
            tipoValor = 1;
        } else if (tipoSeleccionado.value === "pedido") {
            tipoValor = 2;
        } else if (tipoSeleccionado.value === "cliente") {
            tipoValor = 3;
        }

        // Validación del input si es contrato o pedido
        if ((tipoValor === 1 || tipoValor === 2) && !numeroContratoIngresado) {
            if (!document.getElementById("alertaNumero")) {
                const alerta2 = document.createElement("div");
                alerta2.id = "alertaNumero";
                alerta2.className = "alert alert-danger mt-2";
                alerta2.textContent = "Debes ingresar un número de Contrato o Pedido.";
                modal_input.appendChild(alerta);
            }
            inputNumero.classList.add("input-error");
            return;
        }

        // Eliminar alerta si todo está correcto
        const alertaExistente = document.getElementById("alertaNumero");
        if (alertaExistente) {
            alertaExistente.remove();
        }
        inputNumero.classList.remove("input-error");

        // Llamar a la función manejarBoton para enviar la información ingresada
        manejarBoton(2, tipoValor, numeroContratoIngresado);

        // Cerrar el modal
        //$('#ModalConsulta').modal('hide');
    });

    // Eliminar alerta al escribir en el input
    inputNumero.addEventListener("input", function () {
        if (this.value.trim() !== "") {
            const alertaExistente = document.getElementById("alertaNumero");
            if (alertaExistente) {
                alertaExistente.remove();
            }
            this.classList.remove("input-error");
        }
    });

    // Evento cuando se selecciona una fila en la tabla
    $(document).on("click", ".btn-seleccionarProy", function () {
        const fila = $(this).closest("tr");
        const numContrato = fila.find("td:eq(1)").text().trim();
        const numPedido = fila.find("td:eq(2)").text().trim();

        seleccionarBusqueda(numContrato, numPedido);
    });

    // Cerrar modal y limpiar la pantalla
    $('#ModalConsulta').on('hidden.bs.modal', function () {
        $('body').removeClass('modal-open');
        $('body').css('padding-right', '0px');
    });
});



/*================================================================ 
 *    Función para manejar el modal de la consulta DOCUMENTOS    *
 *================================================================*/
document.addEventListener("DOMContentLoaded", function () {
    const consultarDocsBtnModal = document.getElementById("consultarDocsBtnModal");

    consultarDocsBtnModal.addEventListener("click", function () {        
        const valor = $('#numeroProyecto').val();

        if (valor === "") {
            MensajeAlerta("Debe ingresar un número para buscar");
        } else {
            ObtenerListaDocsCargados(valor,"");
        }
    });
});
function ObtenerListaDocsCargados(Codigo,tipo2) {
    var DatosLF = `[{"action":"BuscarListaArchivosContrato","parameters":{"numContrato":"${Codigo}"}}]`;
    CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', DatosLF, "tableSelectDocsCargados", tipo2);
}
function RecorreJSONTableSelectDocsCargados(json, boton, idSeleccionado) {
    if (json && json.length > 0) {
        document.getElementById("FormularioPrincipal").style.display = "none";
        document.getElementById("menuActionProyectos").style.display = "none";
        document.getElementById("tablaDocsCargados").style.display = "block";
        dtDocsCargados(json);        
                
        $('#ModalConsultaDocs').modal('hide')
    } else {
        MensajeAlerta("El Proyecto no se ha podido encontrar. Asegúrese de ingresar el número de pedido proporcionado.");
    }
}
function Create3() {
    if ($.fn.DataTable.isDataTable('#tbl_Docs')) {
        $('#tbl_Docs').DataTable().destroy();
    }
    $('#tbl_Docs tbody').empty();
}
function dtDocsCargados(json) {
    Create3();
    table2 = null;

    // 🔹 Filtramos los documentos que no están eliminados
    const documentosActivos = json.filter(doc => !doc.Nombre.startsWith('eliminado-'));

    table2 = $('#tbl_Docs').DataTable({
        data: documentosActivos,
        columns: [
            {
                data: 'Nombre',
                render: function (data) {
                    return data; // Ya no se muestran eliminados, así que no hace falta poner en gris
                }
            },
            {
                data: 'Nombre',
                render: function (data) {
                    return `<a title='Ver archivo' class='btn btn-abrirDoc btn-xs'>
                                <i class='glyphicon glyphicon-save' aria-hidden='true' style='color:white'></i>
                            </a>`;
                }
            }
        ],
        language: {
            "decimal": ",",
            "thousands": ".",
            "emptyTable": "No hay información",
            "infoEmpty": "Mostrando 0 to 0 of 0 Entradas",
            "infoPostFix": "",
            "loadingRecords": "Cargando...",
            "processing": "Procesando...",
            "search": "Buscar:",
            "zeroRecords": "Sin resultados encontrados",
        },
        orderCellsTop: false,
        fixedHeader: true,
        lengthChange: false,
        paging: false,
        info: false,
        searching: false
    });
}

$(document).on('click', '.btn-abrirDoc', function () {
    // Obtiene el nombre del archivo desde los datos de la fila
    var data = table2.row($(this).closest('tr')).data();
    var nombreArchivo = data.Nombre; // Asegúrate de que el nombre sea el correcto
    var nombreCarpeta = $('#numeroProyecto').val();
    // Construye la URL completa al archivo
    //var url = '/HistoriasClinicas/' + nombreCarpeta + '/' + nombreArchivo;
    var DatosA = "[{ \"action\": \"AbrirDocArchivo\", \"parameters\" : { nombreArchivo : \"" + nombreArchivo + "\", nombreCarpeta: \"" + nombreCarpeta + "\"} }]";
    CargarAbrirArchivo('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', DatosA, "tableSelectArchivos");
});






/* ====================================================================================== *
 * Validamos que el usuario no ingrese caracteres especiales en NumContrato o Num Pedido  *
 * =======================================================================================*/
// Función para mostrar tooltip
function mostrarTooltip(input, mensaje) {
    let tooltip = document.createElement("div");
    tooltip.className = "tooltip-custom";
    tooltip.textContent = mensaje;

    document.body.appendChild(tooltip);

    let rect = input.getBoundingClientRect();
    tooltip.style.left = `${rect.left + window.scrollX}px`;
    tooltip.style.top = `${rect.top + window.scrollY - 30}px`;
    tooltip.style.display = "block";

    // Función para eliminar el tooltip de manera segura
    function removeTooltip() {
        if (tooltip && tooltip.parentNode) {
            tooltip.parentNode.removeChild(tooltip);
        }
    }

    // Eliminar el tooltip cuando el mouse sale del input
    input.addEventListener("mouseleave", removeTooltip, { once: true });
}
function validarSinCaracteresEspeciales(input) {
    // Expresión regular: solo letras, números, espacios, guiones, comas y puntos
    const regex = /^[A-Za-z0-9\s\-,.]*$/;

    if (!regex.test(input.value)) {
        // Eliminar los caracteres inválidos automáticamente
        input.value = input.value.replace(/[^A-Za-z0-9\s\-,.]/g, '');
    }
}
// Agregar eventos a los inputs al cargar la página
document.addEventListener("DOMContentLoaded", function () {
    let inputs = ["txtNumContrato", "txtConPedido"];

    inputs.forEach(id => {
        let input = document.getElementById(id);

        if (input) {
            // Validar en tiempo real
            input.addEventListener("input", function () {
                validarSinCaracteresEspeciales(this);
            });

            // Mostrar mensaje al pasar el mouse
            input.addEventListener("mouseenter", function () {
                mostrarTooltip(this, "No ingresar caracteres especiales, tildes o ñ, solo letras y números.");
            });
        }
    });
});



/* ================================================================
 *  Cambiamos el color de textarea que son para Observaciones
 * ================================================================*/
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("textarea[id$='Obs']").forEach(textarea => {
        textarea.style.color = "black"; // Cambia el color del texto
        textarea.style.backgroundColor = "#7dacbc91"; // Cambia el color de fondo
        textarea.style.borderRadius = "5px";
    });
});


/*==========================================================================*
 *      Manejamos el formato del valor total del contrato usando . , $      
 * =========================================================================*/
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
function formatearValorGeneral(input) {
    // Eliminar caracteres no numéricos excepto la coma (para decimales) y el punto (para miles)
    let valor = input.value.replace(/[^0-9,]/g, '');

    // Separar la parte entera de la decimal
    let partes = valor.split(',');
    let parteEntera = partes[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.'); // Agregar puntos como separadores de miles

    let parteDecimal = '';
    if (partes.length > 1) {
        parteDecimal = ',' + partes[1].replace(/[^0-9]/g, '').slice(0, 2); // permite ingresar hasta un maximo de 2 decimales
    }

    // Construir el valor final formateado
    input.value = '$' + parteEntera + parteDecimal;
}



//--------------------    Obtenemos todos los clientes de la base de datos    ---------------------------------------
function BuscarCliente2() {

    if ($('#txtCliente').val().length > 2) {
        idSeleccionado = 0;
        ObtenerListaClientes(2, $('#txtCliente').val(), 4);
    }
    else {
        document.getElementById("comboClientes2").style.display = "none";
    }
}
//obtenemos los clientes para el modal Consulta
function BuscarCliente3() {

    if ($('#txtCliente3').val().length > 2) {
        //idSeleccionado = 0;
        ObtenerListaClientes(2, $('#txtCliente3').val(), 5);
    }
    else {
        document.getElementById("comboClientes3").style.display = "none";
    }
}
function CargarCliente2(ID, NOMBRE) {
    $('#txtCliente').val(NOMBRE);
    idCliente2 = ID;
    document.getElementById("comboClientes2").style.display = "none";
}
function CargarCliente3(ID, NOMBRE) {
    $('#txtCliente3').val(NOMBRE);
    idCliente2 = ID;
    document.getElementById("comboClientes3").style.display = "none";
}
function ObtenerListaClientes(tipo, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', Datos, "tableSelectBusquedaCli", idproceso);
}





/* =============================================================================
 *      funcion para cargar info de contrato y saber si tiene permiso
 * ============================================================================*/
function BuscarEstadoNotificacion(contrato, tipoBusqueda, opcion) {
    //var nContrato = document.getElementById("txtNomContacto").val();

    var nContrato = '';
    var Datos = '';
    var table = '';

    if (opcion == 1) {
        document.getElementById("tblaArchivos").style.display = "none";
        document.getElementById("btn-mostrarPDF").style.display = "none";

        nContrato = 'NUEVO';
        VerForm1();
        limpiarFormulario();
        limpiarPDF();
        limpiarSeleccionBotones();
    }
    else if (opcion == 2) { // opcion usada para consultar un contrato        
        limpiarSeleccionBotones();

        nContrato = contrato;
        Datos = "[{ \"action\": \"BuscarContrato\", \"parameters\" : { tipo : \"" + tipoBusqueda + "\", nContrato: \"" + nContrato + "\", op: \"" + 1 + "\"} }]"; //op = 1 para buscar solo 1 contrato
        table = "tableSelectBusquedaContrato";
        CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', Datos, table, tipoBusqueda);
        limpiarPDF();
    }
    else if (opcion == 3) { //opcion usada para el pdf
        nContrato = contrato;

        Datos = "[{ \"action\": \"BuscarContrato\", \"parameters\" : { tipo : \"" + tipoBusqueda + "\", nContrato: \"" + nContrato + "\", op: \"" + 1 + "\"} }]"; //op = 1 para buscar solo 1 contrato
        table = "tableSelectContratoPDF";
        CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', Datos, table, 0);
    }
    else if (opcion == 4) { // esta opcion la usamos para obtener los contratos y cargarlos al modal de consulta
        nContrato = contrato;

        Datos = "[{ \"action\": \"BuscarContrato\", \"parameters\" : { tipo : \"" + 2 + "\", nContrato: \"" + nContrato + "\", op: \"" + 2 + "\"} }]";
        table = "tableSelectContratoConsulta";
        CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', Datos, table, 0);
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
function CargarPaginaPDF(div, url, datos, tipoControl, boton, idSeleccionado) {
    if (!div) {
        console.error("❌ El parámetro 'div' es indefinido.");
        return;
    }

    $.ajax({
        url: url,
        method: "GET",
        data: datos,
        dataType: "json",
        success: function (response) {
            console.log("🔍 Respuesta del servidor:", response);

            if (!response) {
                console.error("❌ La respuesta del servidor está vacía o es nula.");
                return;
            }

            if (typeof response === "string") {
                try {
                    response = JSON.parse(response);
                } catch (error) {
                    console.error("❌ No se pudo parsear la respuesta como JSON:", error);
                    console.error("❌ Respuesta recibida:", response);
                    return;
                }
            }

            if (Array.isArray(response)) {
                console.log("✅ La respuesta es un array.");
                console.table(response);
                mostrarArchivos(response);
            } else if (typeof response === "object" && Object.keys(response).length > 0) {
                console.log("ℹ️ La respuesta es un objeto con claves:", Object.keys(response));

                if (response.archivos && Array.isArray(response.archivos)) {
                    console.log("✅ Se encontró un array en 'archivos'.");
                    console.table(response.archivos);
                    mostrarArchivos(response.archivos);
                } else {
                    console.error("❌ No se encontró un array en 'archivos'.");
                }
            } else {
                console.error("❌ La respuesta no contiene datos válidos.");
            }
        },
        error: function (xhr, status, error) {
            console.error("❌ Error en la petición AJAX:", error);
            console.error("❌ Estado de la petición:", status);
            console.error("❌ Respuesta del servidor:", xhr.responseText);
        }
    });
}



function RecorreJSON(div, json, tipoControl, boton, idSeleccionado) {
    var contenido = "";

    if (tipoControl == "tableSelectBusquedaContrato") {
        contenido = RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    //--------------Modal Consulta-------------
    if (tipoControl == "tableSelectContratoConsulta") {
        contenido = RecorreJSONTableContratoConsulta(json, boton, idSeleccionado);
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
    if (tipoControl == "tableSelectArchivosPDF") {
        contenido = RecorreJSONTableSelectArchivoPDF(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectArchivos") {
        contenido = RecorreJSONTableSelectArchivo(json, boton, idSeleccionado); 
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectDocsCargados") {
        contenido = RecorreJSONTableSelectDocsCargados(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectPolizas") {
        contenido = RecorreJSONTableSelectPolizas(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    //--------------PDF-------------
    if (tipoControl == "tableSelectContratoPDF") {
        contenido = RecorreJSONTableSelectContratoPDF(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    return contenido;
}

function RecorreJSONTableSelectBusqueda(json, boton, tipoBusqueda) {
    // Validar si el JSON no tiene datos o está vacío
    if (
        !json ||
        (tipoBusqueda === 1 && json.NUM_CONTRATO === null) ||
        (tipoBusqueda === 2 && json.NUM_PEDIDO === null)
    ) {
        alerta("No se encontró el proyecto ingresado.");
        return;
    }    
    // Cerrar el modal
    $('#ModalConsulta').modal('hide');
    document.getElementById("tblaArchivos").style.display = "block";
    document.getElementById("btn-mostrarPDF").style.display = "block";
    // Si hay datos, llamar a la función para mostrarlos
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


    //Limpiamos las tablas 
    document.querySelector("#dynamicTableServiciosExt tbody").innerHTML = "";
    document.querySelector("#dynamicTableCosteo tbody").innerHTML = "";
    document.querySelector("#dynamicTableHwd tbody").innerHTML = "";
    document.querySelector("#dynamicTableLicencias tbody").innerHTML = "";
    document.querySelector("#dynamicTableLicencias tbody").innerHTML = "";
    document.querySelector("#dynamicTableServFab tbody").innerHTML = "";
    document.querySelector("#dynamicTablePoliza tbody").innerHTML = "";
    document.querySelector("#dynamicTableFormas tbody").innerHTML = "";


    opcionUsuario = json.opcion;

    verificarUsuario();

    // Carga los datos de la base en los input del HTML
    let Cliente = json.CLIENTE || '';
    document.getElementById("txtCliente").value = Cliente;
    let NumPedido = json.NUM_PEDIDO || '';
    document.getElementById("txtConPedido").value = NumPedido;

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

    let valorMargen = json.MARGEN || '';
    document.getElementById("txtConMargen").value = valorMargen;

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
        

    let objeto = json.OBJETO || '';
    document.getElementById("txtObjeto").value = objeto;
    let objetoObs = json.OBS_OBJETO || '';
    document.getElementById("txtObjetoObs").value = objetoObs;
    //document.getElementById("txtServiciosFabObs").value = servFabObs;
    let servDosObs = json.OBS_SERVICIO_DOS || '';
    document.getElementById("txtServiciosDOSObs").value = servDosObs;
    //------> tabla de Servicios EXTERNOS
    loadTableData(json.SERVICIO_EXTERNOS, addRowServExt, "dynamicTableServiciosExt");
    let servExtObs = json.OBS_SERVICIO_EXTERNOS || '';
    document.getElementById("txtServiciosExternosObs").value = servExtObs;
    //------> tabla de COSTEO
    loadTableData(json.ALCANCE, addRowCosteo, "dynamicTableCosteo");
    let alcanceObs = json.OBS_ALCANCE || '';
    document.getElementById("txtAlcanceObjetoObs").value = alcanceObs;
    //------> tabla de HARDWARE
    loadTableData(json.HARDWARE, addRowHwd, "dynamicTableHwd");
    let hardwareObs = json.OBS_HARDWARE || '';
    document.getElementById("txtHardwareObs").value = hardwareObs;
    //------> tabla de LICENCIAS
    loadTableData(json.LICENCIAS, addRowLicencias, "dynamicTableLicencias");
    let licenciasObs = json.OBS_LICENCIAS || '';
    document.getElementById("txtLicenciasObs").value = licenciasObs;

    loadTableData(json.SERVICIOS_FABRICANTE, addRowServFab, "dynamicTableServFab");
    let servFabObs = json.OBS_SERVICIOS_FABRICANTE || '';
    document.getElementById("txtServFabObs").value = servFabObs;
    //------> tabla de POLIZAS
    loadTableData(json.POLIZAS, addRowPoliza, "dynamicTablePoliza");
    let PolizasObs = json.OBS_POLIZAS || '';
    document.getElementById("txtPolizasObs").value = PolizasObs;

    loadTableData(json.FORMA_PAGO, addRow,"dynamicTableFormas");
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

    let nomContacto = json.CLI_NOMBRE || '';
    document.getElementById("txtNomContacto").value = nomContacto;
    let telfContacto = json.CLI_TELEFONO || '';
    document.getElementById("txtTelefono").value = telfContacto;
    let dirContacto = json.CLI_DIRECCION || '';
    document.getElementById("txtDireccion").value = dirContacto;
    let correoContacto = json.CLI_CORREO || '';
    document.getElementById("txtCorreo").value = correoContacto;

    //deshabilitarInputs();
    
    VerArchivos();
    updateValues();
    marcarBotonesSeleccionados(json.ITEMS);
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
    if (idSeleccionado == 5) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarCliente3(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboClientes3").innerHTML = x;
        document.getElementById("comboClientes3").style.display = "block";
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

// Funcion para obtener el valor de rentabilidad 
function ObtenerRentabilidad() {
    let valorContrato = parseFloat(getDecimalValue()) || 0;
    let margenInput = document.getElementById("txtConMargen").value.trim();

    // Validar que el margen no esté vacío
    if (margenInput === "") {
        document.getElementById("txtPDFRentabilidadValor").textContent = "--- ---";
        document.getElementById("txtPDFRentabilidadPorcentaje").textContent = "---";
        return;
    }

    let margen = parseFloat(margenInput);

    // Convertir margen a decimal si es necesario
    if (margen > 1) {
        margen = margen / 100;
    }

    let rentabilidad = valorContrato * margen;

    // Mostrar los valores en los spans
    document.getElementById("txtPDFRentabilidadPorcentaje").textContent = `${(margen * 100).toFixed(2)}%`;
    document.getElementById("txtPDFRentabilidadValor").textContent = `$ ${rentabilidad.toFixed(2)}`;
}

/* =====================================================================
 *  Función para mostrar mensaje modal y marcar como archivo cargado    *
 *  ====================================================================*/
//function MensajeCargaArchivo(archivo, textoTema) {
//    //var texto = document.getElementsByClassName("tituloTema");
//    $('#msgCargarArchivos').modal('show');
//    document.getElementById("txtnombreVentana").textContent = textoTema;
//    document.getElementById("txtnombreArchivo").textContent = archivo;
//}
//function confirmacionCargaArchivo(idBoton) {

//    var archivosSeleccionados = $('#archivosAdjuntos')[0].files;
//    for (var i = 0; i < archivosSeleccionados.length; i++) {
//        archivosTemporales.push(archivosSeleccionados[i]);
//    }
//    var mensaje = document.getElementById(idBoton);
//    if (mensaje) {
//        mensaje.innerHTML = "<i class='glyphicon glyphicon-link'></i> Archivo cargado";
//        mensaje.classList.remove("btn-primary");
//        mensaje.classList.add("btn-success");
//    }
//    $('#msgCargarArchivos').modal('hide');
//}

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

function cargarArchivosAlServidor(codigoContrato) {
    var mensajeVerificacion = "";
    var url = 'CargaArchivos.ashx';

    if ($('#txtNumContrato').val() == "") {
        mensajeVerificacion += "   Debe ingresar el Numero de Contrato ";
        contadorVerificacion += 1;
    }

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
        MensajeAlerta(mensajeError);
    }
}
// Funcion para obtener la lista de botones seleccionados
function obtenerListaBtnArchivos() {
    let botonesSeleccionados = document.querySelectorAll(".title-button.selected");
    let nombresArchivos = Array.from(botonesSeleccionados).map(boton => boton.textContent);

    return nombresArchivos.join(";"); // Retorna una cadena con los nombres separados por ";"
}
// Funcion para marcar como seleccionados los botones que estan almacenados en la base de datos
//function marcarBotonesSeleccionados(listaArchivos) {
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
function marcarBotonesSeleccionados(listaArchivos) {
    if (!listaArchivos || typeof listaArchivos !== "string") {
        console.warn("La lista de archivos es inválida o vacía:", listaArchivos);
        return; // Detenemos la ejecución si no hay datos válidos
    }

    let nombresArchivos = listaArchivos.split(";"); // Convertimos la cadena en un array
    let botones = document.querySelectorAll(".title-button");

    botones.forEach(boton => {
        if (nombresArchivos.includes(boton.textContent)) {
            boton.classList.add("selected"); // Marcar como seleccionado
        } else {
            boton.classList.remove("selected"); // Asegurar que los no seleccionados se mantengan sin clase
        }
    });

    // También actualizamos la lista de archivos en el HTML
    actualizarListaArchivos();
}


// Limpiamos la seleccion anterior de botones
function limpiarSeleccionBotones() {
    let botones = document.querySelectorAll(".title-button.selected");

    botones.forEach(boton => {
        boton.classList.remove("selected"); // Quita la clase 'selected'
    });

    // También limpiamos la lista de archivos en el HTML
    document.getElementById("listaArchivos").textContent = "";
}


function GuardarArchivo(codigoContrato) {
    //IdEmpleado = id;
    if (IdEmpleado == 0) {
        alert("Para cargar un archivo debe seleccionar un contrato...");
    } else {
        var url = 'CargaArchivos.ashx';

        // Checking whether FormData is available in browser  
        if (window.FormData !== undefined) {
            var fileUpload = $("#archivosAdjuntos").get(0);
            var files = fileUpload.files;

            // Create FormData object  
            var fileData = new FormData();

            // Looping over all files and add them to the FormData object  
            for (var i = 0; i < files.length; i++) {
                fileData.append(files[i].name, files[i]);
            }

            // Adding more keys to the FormData object  
            fileData.append('session', $("#ContentPlaceHolder1_txtUsuario").val());
            fileData.append('action', 'CargarArchivos');
            fileData.append('Id_RegTareas', 12);
            fileData.append('idServicio', "10");
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
                        MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);
                        ListadoArchivosContrato("#divArchivosAdjuntosAnteriores", IdEmpleado);
                    }
                    $("progress").hide();
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
                    var mensajeError = "La acción de cargar el archivo está tomando demasiado tiempo. Verifique su conexión de red y luego intente nuevamente cargar los archivos.";
                    MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mensajeError, 'danger');
                }
            });

        } else {
            var mensajeError = "FormData no es soportado por su navegador.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mensajeError, 'danger');
        }
    }
}

// Función para validar caracteres especiales, tildes y "ñ"
//function contieneCaracteresInvalidos(valor) {
//    let regex = /[^a-zA-Z0-9\s]/; // Solo permite letras y números sin tildes ni ñ
//    return regex.test(valor);
//}
function contieneCaracteresInvalidos(valor) {
    let regex = /[^a-zA-Z0-9\s\-,.]/; // Permite letras, números, espacios, guiones, comas y puntos
    return regex.test(valor);
}
function GuardarContrato(codigoContrato) {

    var url = "ObtenerNuevaListaTareas.ashx";
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
    }*/   

    // Validar si los campos están vacíos
    if (contieneCaracteresInvalidos($('#txtNumContrato').val().trim())) {
        mensajeVerificacion += "- El número de Contrato no debe contener caracteres especiales, tildes o ñ\n";
        contadorVerificacion++;
    }

    if ($('#txtConPedido').val().trim() === "") {
        mensajeVerificacion += "- Debe ingresar el número de Pedido\n";
        contadorVerificacion++;
    } else if (contieneCaracteresInvalidos($('#txtConPedido').val().trim())) {
        mensajeVerificacion += "- El número de Pedido no debe contener caracteres especiales, tildes o ñ\n";
        contadorVerificacion++;
    }
    /*if ($('#txtValorContrato').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Valor TOTAL del Contrato";
        contadorVerificacion += 1;
    }
    if ($('#txtObjeto').val() == "") {
        mensajeVerificacion += "- Debe ingresar el objeto";
        contadorVerificacion += 1;
    }*/

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    const listaCorreosExtra = obtenerCorreos(); // Creamos una variable para los correos de notificación

       
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
        'txtNumPedido': $('#txtConPedido').val(),
        'txtValorContrato': getDecimalValue(),
        'txtConMargen': $('#txtConMargen').val(),
        'txtObjeto': $('#txtObjeto').val(),
        'txtServiciosExt': getTablesData(['dynamicTableServiciosExt']),
        
        'txtAlcance': getTablesData(['dynamicTableCosteo']),
        'txtAlcanceCosteos': getTablesData(['dynamicTableCosteo']),

        'txtHardware': getTablesData(['dynamicTableHwd']),

        'txtLicencias': getTablesData(['dynamicTableLicencias']),
        'txtServiciosFab': getTablesData(['dynamicTableServFab']),
        'txtPolizas': getTablesData(['dynamicTablePoliza']),
        'selectFormaPago': getTablesData(['dynamicTableFormas']),

        'fechaSuscripContrato': $('#fechaSuscripContrato').val(), 
        'fechaNotifAnticipo': $('#fechaNotifAnticipo').val(), 
        'fechaIniActivacion': $('#fechaIniActivacion').val(), 
        'fechaFinActivacion': $('#fechaFinActivacion').val(),

        'txtNumContratoObs': $('#txtNumContratoObs').val(), 
        'txtValorContratoObs': $('#txtValorContratoObs').val(),
        'txtObjetoObs': $('#txtObjetoObs').val(),
        'txtServiciosDOSObs': $('#txtServiciosDOSObs').val(),
        'txtServiciosExternosObs': $('#txtServiciosExternosObs').val(),
        'txtAlcanceObjetoObs': $('#txtAlcanceObjetoObs').val(),
        'txtHardwareObs': $('#txtHardwareObs').val(),
        'txtLicenciasObs': $('#txtLicenciasObs').val(),
        'txtServiciosFabObs': $('#txtServFabObs').val(),
        
        'txtPolizasObs': $('#txtPolizasObs').val(),
        'txtFormaPagoObs': $('#txtFormaPagoObs').val(),

        'txtCorreosAdicionales': listaCorreosExtra,

        'txtItems': obtenerListaBtnArchivos()

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

    document.getElementById("btn-mostrarPDF").style.display = "block";

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


// Funcion para Sumar los valores de la tabla Costeo
//function actualizarSuma() {
//    let total = 0;

//    // Obtener todos los inputs de valores
//    const inputs = document.querySelectorAll('[id^="txtValorCosteo-"]');

//    inputs.forEach(input => {
//        let valor = input.value;

//        // Eliminar el símbolo de dólar y espacios
//        valor = valor.replace(/\$/g, '').trim();

//        // Convertir formato latino ($54.376,90 → 54376.90)
//        valor = valor.replace(/\./g, '').replace(',', '.');

//        if (!isNaN(valor) && valor !== "") {
//            total += parseFloat(valor);
//        }
//    });

//    // Aplicar formato de moneda nuevamente
//    document.getElementById("sumaValueCosteo").innerText = `Valor total: $${formatearMoneda(total)}`;
//}

function actualizarSuma(prefixInput, idSumaTotal) {
    let total = 0;

    // Obtener todos los inputs de valores con el prefijo indicado
    const inputs = document.querySelectorAll(`[id^="${prefixInput}"]`);

    inputs.forEach(input => {
        let valor = input.value;

        // Eliminar el símbolo de dólar y espacios
        valor = valor.replace(/\$/g, '').trim();

        // Convertir formato latino ($54.376,90 → 54376.90)
        valor = valor.replace(/\./g, '').replace(',', '.');

        if (!isNaN(valor) && valor !== "") {
            total += parseFloat(valor);
        }
    });

    // Aplicar formato de moneda nuevamente
    document.getElementById(idSumaTotal).innerText = `Valor total: $${formatearMoneda(total)}`;
}


// Función para formatear un número en formato moneda latinoamericano
function formatearMoneda(numero) {
    return numero.toLocaleString('es-ES', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}



/* =============================================================================================================*
 *                 Agregar filas en las tablas de "forma de pago", "servExt", "Costeo", etc                     *
 * =============================================================================================================*/

//------>1 Primero creamos un evento para identificar que se ha realizado algun cambio en el Valor del contrato ya sea en el entero o en el decimal
document.addEventListener('DOMContentLoaded', function () {
    // Agregar evento al botón para agregar fila
    document.getElementById('addRowButton').addEventListener('click', function (event) {
        event.preventDefault();
        addRow();
        updateValues();
    });
    // Escuchar cambios en el input de valor del contrato para actualizar valores
    document.getElementById('txtValorContrato').addEventListener('input', updateValues);
    document.getElementById('txtDecimales').addEventListener('input', updateValues);
});

//------>7 Primero creamos un evento para identificar que se ha realizado un cambio en ServiciosExt
document.addEventListener('DOMContentLoaded', function () {
    // Agregar evento al botón para agregar fila
    document.getElementById('addRowButtonServExt').addEventListener('click', function (event) {
        event.preventDefault();
        addRowServExt();
    });    
});

//------->2 Agrega nuevas filas a Costeo
document.addEventListener('DOMContentLoaded', function () {
    // Agregar evento al botón para agregar fila
    document.getElementById('addRowButtonCosteo').addEventListener('click', function (event) {
        event.preventDefault();
        addRowCosteo();
    });
    // Escuchar cambios en el input de valor del contrato para actualizar valores
    //document.getElementById('txtValorContrato').addEventListener('input', updateValues);
    //document.getElementById('txtDecimales').addEventListener('input', updateValues);
});

//------->3 Agrega nuevas filas a Hardware
document.addEventListener('DOMContentLoaded', function () {
    // Agregar evento al botón para agregar fila
    document.getElementById('addRowButtonHwd').addEventListener('click', function (event) {
        event.preventDefault();
        addRowHwd();
    });
    // Escuchar cambios en el input de valor del contrato para actualizar valores
    //document.getElementById('txtValorContrato').addEventListener('input', updateValues);
    //document.getElementById('txtDecimales').addEventListener('input', updateValues);
});

//------->4 Agrega nuevas filas a Hardware
document.addEventListener('DOMContentLoaded', function () {
    // Agregar evento al botón para agregar fila
    document.getElementById('addRowButtonLicencias').addEventListener('click', function (event) {
        event.preventDefault();
        addRowLicencias();
    });
    // Escuchar cambios en el input de valor del contrato para actualizar valores
    //document.getElementById('txtValorContrato').addEventListener('input', updateValues);
    //document.getElementById('txtDecimales').addEventListener('input', updateValues);
});

//------->5 Agrega nuevas filas a Servicios Fabricante
document.addEventListener('DOMContentLoaded', function () {
    // Agregar evento al botón para agregar fila
    document.getElementById('addRowButtonServFab').addEventListener('click', function (event) {
        event.preventDefault();
        addRowServFab();
    });
    // Escuchar cambios en el input de valor del contrato para actualizar valores
    //document.getElementById('txtValorContrato').addEventListener('input', updateValues);
    //document.getElementById('txtDecimales').addEventListener('input', updateValues);
});

//------->6 Agrega nuevas filas a Polizas
document.addEventListener('DOMContentLoaded', function () {
    // Agregar evento al botón para agregar fila
    document.getElementById('addRowButtonPoliza').addEventListener('click', function (event) {
        event.preventDefault();
        addRowPoliza();
    });
    // Escuchar cambios en el input de valor del contrato para actualizar valores
    //document.getElementById('txtValorContrato').addEventListener('input', updateValues);
    //document.getElementById('txtDecimales').addEventListener('input', updateValues);
});



//-------->1.2 Función para agregar una nueva fila a la tabla FORMA DE PAGO
function addRow() {
    const table = document.getElementById('dynamicTableFormas').getElementsByTagName('tbody')[0];
    const newRow = table.insertRow();
    const rowIndex = newRow.rowIndex;

    // Insertar celdas para descripción, porcentaje, valor y acción
    const descriptionCell = newRow.insertCell(0);
    const percentageCell = newRow.insertCell(1);
    const valueCell = newRow.insertCell(2);
    const fecEstimadaCell = newRow.insertCell(3);
    const actionCell = newRow.insertCell(4);

    // Insertar inputs y botón de eliminar
    descriptionCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtDesFormaPago-${rowIndex}" class="form-control" type="text" placeholder="Detalle" value="-">
        </div>`;
    percentageCell.innerHTML = `
        <div style = "display:flex; justify-content: center;" >
            <input type="number" min="0" max="100" placeholder="%" oninput="validateAndUpdate(this)" >
        </div>`;
    valueCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtValorContrato-${rowIndex}" type="text" class="form-control" placeholder="$" style="background-color: rgba(25, 25, 25, 0.4);" readonly>
        </div>`;
    fecEstimadaCell.innerHTML = `<input id="txtfecEstimada-${rowIndex}" class="form-control" type="date" placeholder="">`;
    actionCell.innerHTML = '<button class="delete-button" onclick="deleteRow(this)"><i class="fa fa-trash-o" aria-hidden="true"></i></button>';

    //updateValues(); // Actualizar valores después de agregar la fila
}
//------->7.2 Función para agregar una nueva fila a la tabla dinámica de Servicios Externos
function addRowServExt() {
    const table = document.getElementById('dynamicTableServiciosExt').getElementsByTagName('tbody')[0];
    const newRow = table.insertRow();
    const rowIndex = newRow.rowIndex;

    // Insertar celdas para descripción, porcentaje, valor y acción
    const descriptionCell = newRow.insertCell(0);
    //const percentageCell = newRow.insertCell(1); 
    const valueCell = newRow.insertCell(1);
    const actionCell = newRow.insertCell(2);

    // Insertar inputs y botón de eliminar
    descriptionCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtDesServExt-${rowIndex}" class="form-control" type="text" placeholder="Descripción" value="-">
        </div>`;
    valueCell.innerHTML = `
    <div style="display:flex; justify-content: center;">
        <input id="txtValorServExt-${rowIndex}" type="text" class="form-control" placeholder="$"
        oninput="formatearValorGeneral(this); actualizarSuma('txtValorServExt-', 'sumaValueSerExt');">
    </div>`;

    actionCell.innerHTML = '<button class="delete-button" onclick="deleteRowTable(this, \'txtValorServExt-\', \'sumaValueSerExt\')"><i class="fa fa-trash-o" aria-hidden="true"></i></button>';

    updateValues(); // Actualizar valores después de agregar la fila
}
//------->2.2 Función para agregar una nueva fila a la tabla dinámica de Costeo
function addRowCosteo() {
    const table = document.getElementById('dynamicTableCosteo').getElementsByTagName('tbody')[0];
    const newRow = table.insertRow();
    const rowIndex = newRow.rowIndex;

    // Insertar celdas para descripción, porcentaje, valor y acción
    const osCell = newRow.insertCell(0);
    const descriptionCell = newRow.insertCell(1);
    //const percentageCell = newRow.insertCell(1); 
    const valueCell = newRow.insertCell(2);
    const actionCell = newRow.insertCell(3);

    // Insertar inputs y botón de eliminar
    osCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtOSCosteo-${rowIndex}" class="form-control" type="text" placeholder="Num OS" value="-">
        </div>`;
    descriptionCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtDesCosteo-${rowIndex}" class="form-control" type="text" placeholder="Descripción" value="-">
        </div>`;
    valueCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtValorCosteo-${rowIndex}" type="text" class="form-control" placeholder="$"
            oninput="formatearValorGeneral(this); actualizarSuma('txtValorCosteo-', 'sumaValueCosteo');">
        </div>`;
    actionCell.innerHTML = '<button class="delete-button" onclick="deleteRowTable(this, \'txtValorCosteo-\', \'sumaValueCosteo\')"><i class="fa fa-trash-o" aria-hidden="true"></i></button>';

    updateValues(); // Actualizar valores después de agregar la fila
}
//------->3.2 Función para agregar una nueva fila a la tabla dinámica de Hardware
function addRowHwd() {
    const table = document.getElementById('dynamicTableHwd').getElementsByTagName('tbody')[0];
    const newRow = table.insertRow();
    const rowIndex = newRow.rowIndex;

    // Insertar celdas para descripción, porcentaje, valor y acción
    const cantidadCell = newRow.insertCell(0);
    const descriptionCell = newRow.insertCell(1);
    //const percentageCell = newRow.insertCell(1); 
    const valueCell = newRow.insertCell(2);
    const actionCell = newRow.insertCell(3);

    // Insertar inputs y botón de eliminar
    cantidadCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtCanHwd-${rowIndex}" class="form-control" type="number" placeholder="#">
        <div> `;
    descriptionCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtDesHwd-${rowIndex}" class="form-control" type="text" placeholder="Descripción" value="-">
        <div> `;
    valueCell.innerHTML = ` 
        <div style="display:flex; justify-content: center;">
            <input id="txtValorHwd-${rowIndex}" type="text" class="form-control" placeholder="$"
            oninput="formatearValorGeneral(this); actualizarSuma('txtValorHwd-', 'sumaValueSerHwd');">
        </div>`;

    actionCell.innerHTML = '<button class="delete-button" onclick="deleteRowTable(this, \'txtValorHwd-\', \'sumaValueSerHwd\')"><i class="fa fa-trash-o" aria-hidden="true"></i></button>';


    updateValues(); // Actualizar valores después de agregar la fila
}
//------->4.2 Función para agregar una nueva fila a la tabla dinámica de Licencias
function addRowLicencias() {
    const table = document.getElementById('dynamicTableLicencias').getElementsByTagName('tbody')[0];
    const newRow = table.insertRow();
    const rowIndex = newRow.rowIndex;

    // Insertar celdas para descripción, porcentaje, valor y acción
    const cantidadCell = newRow.insertCell(0);
    const descriptionCell = newRow.insertCell(1);
    const valueCell = newRow.insertCell(2);
    const actionCell = newRow.insertCell(3);

    // Insertar inputs y botón de eliminar
    cantidadCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtCanLic-${rowIndex}" class="form-control" type="number" placeholder="#">
        </div> `;
    descriptionCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtDesLic-${rowIndex}" class="form-control" type="text" placeholder="Descripción" value="-">
        </div> `;
    valueCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtValorLic-${rowIndex}" type="text" class="form-control" placeholder="$"
            oninput="formatearValorGeneral(this); actualizarSuma('txtValorLic-', 'sumaValueSerLic');">
        </div> `;
    actionCell.innerHTML = '<button class="delete-button" onclick="deleteRowTable(this, \'txtValorLic-\', \'sumaValueSerLic\')"><i class="fa fa-trash-o" aria-hidden="true"></i></button>';

    updateValues();
}
//------->5.2 Función para agregar una nueva fila a la tabla dinámica de Servicios de fabricante
function addRowServFab() {
    const table = document.getElementById('dynamicTableServFab').getElementsByTagName('tbody')[0];
    const newRow = table.insertRow();
    const rowIndex = newRow.rowIndex;

    // Insertar celdas para descripción, porcentaje, valor y acción
    const cantidadCell = newRow.insertCell(0);
    const descriptionCell = newRow.insertCell(1);
    const valueCell = newRow.insertCell(2);
    const actionCell = newRow.insertCell(3);

    // Insertar inputs y botón de eliminar
    cantidadCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtCanServFab-${rowIndex}" class="form-control" type="number" placeholder="#">
        </div> `;
    descriptionCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtDesServFab-${rowIndex}" class="form-control" type="text" placeholder="Descripción" value="-">
        </div> `;
    valueCell.innerHTML = `
        <div style="display:flex;">
            <input id="txtValorServFab-${rowIndex}" type="text" class="form-control" placeholder="$"
            oninput="formatearValorGeneral(this); actualizarSuma('txtValorServFab-', 'sumaValueServFab');">
        </div>`;
    actionCell.innerHTML = '<button class="delete-button" onclick="deleteRowTable(this, \'txtValorServFab-\', \'sumaValueServFab\')"><i class="fa fa-trash-o" aria-hidden="true"></i></button>';

    updateValues(); // Actualizar valores después de agregar la fila
}
//------->6.2 Función para agregar una nueva fila a la tabla dinámica de Polizas
function addRowPoliza() {
    const table = document.getElementById('dynamicTablePoliza').getElementsByTagName('tbody')[0];
    const newRow = table.insertRow();
    const rowIndex = newRow.rowIndex;

    // Insertar celdas para descripción, porcentaje, valor y acción
    const descriptionCell = newRow.insertCell(0);
    //const percentageCell = newRow.insertCell(1); 
    const tipeCell = newRow.insertCell(1);
    const fecEmisionCell = newRow.insertCell(2);
    const fecCaducidadCell = newRow.insertCell(3);
    const valueCell = newRow.insertCell(4);
    const actionCell = newRow.insertCell(5);

    // Insertar inputs y botón de eliminar
    descriptionCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtDesPoliza-${rowIndex}" class="form-control" type="text" placeholder="Descripción" value="-">
        </div>`;
    tipeCell.innerHTML = `
        <div style="display:flex;">
            <select id="txtTipoPoliza-${rowIndex}" class="form-control" style="font-size:12px;">
                <option value="-" selected>- Seleccionar -</option>
                <option value="BUEN USO DEL ANTICIPO">BUEN USO DEL ANTICIPO</option>
                <option value="CUMPLIMIENTO DEL CONTRATO">CUMPLIMIENTO DEL CONTRATO</option>
                <option value="GARANTIA BANCARIA">GARANTIA BANCARIA</option>
            </select>
        </div>`;
    fecEmisionCell.innerHTML = `
        <div style="display:flex;">
            <input id="txtfecEmisionPoliza-${rowIndex}" class="form-control" type="date" placeholder="Descripción" value="-">
        </div>`;
    fecCaducidadCell.innerHTML = `
        <div style="display:flex;">
            <input id="txtfecCaducidadPoliza-${rowIndex}" class="form-control" type="date" placeholder="Descripción" value="-">
        </div>`;
    valueCell.innerHTML = `
        <div style="display:flex; justify-content: center;">
            <input id="txtValorPoliza-${rowIndex}" type="text" class="form-control" placeholder="$"
            oninput="formatearValorGeneral(this); actualizarSuma('txtValorPoliza-', 'sumaValuePoliza');">
        </div>`;
    actionCell.innerHTML = '<button class="delete-button" onclick="deleteRowTable(this, \'txtValorPoliza-\', \'sumaValuePoliza\')"><i class="fa fa-trash-o" aria-hidden="true"></i></button>';

    updateValues(); // Actualizar valores después de agregar la fila
}


// Función para validar y actualizar el porcentaje ingresado
function validateAndUpdate(input) {
   
    const table = document.getElementById('dynamicTableFormas').getElementsByTagName('tbody')[0];
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

    // Llamar a updateValues() después de la validación
    updateValues();
}

// Función para actualizar los valores calculados en la tabla
function updateValues() {
    const txtValorContrato = parseFloat(document.getElementById('txtValorContrato').value.replace(/\./g, '').replace('$', '')) || 0;
    const txtDecimales = parseFloat(document.getElementById('txtDecimales').value) || 0;
    const valorContrato = parseFloat(`${txtValorContrato}.${txtDecimales}`);

    const table = document.getElementById('dynamicTableFormas').getElementsByTagName('tbody')[0];
    const rows = table.getElementsByTagName('tr');

    let totalUsed = 0;

    // Calcular y actualizar los valores en cada fila de la tabla
    for (let i = 0; i < rows.length; i++) {
        const percentageInput = rows[i].cells[1].getElementsByTagName('input')[0];
        const valueInput = rows[i].cells[2].getElementsByTagName('input')[0];

        const percentage = parseFloat(percentageInput.value) || 0;
        const calculatedValue = (valorContrato * percentage) / 100;

        // Aplicar la función de formato antes de mostrar el valor
        valueInput.value = formatCurrency(calculatedValue);
        totalUsed += calculatedValue;
    }

    // Calcular el valor restante y mostrarlo formateado
    const remainingValue = valorContrato - totalUsed;
    document.getElementById('remainingValue').innerText = `[Valor Contrato] - [Suma de pagos] =  ${formatCurrency(remainingValue)}`;
}

// Función para formatear el valor a moneda correctamente
function formatCurrency(value) {
    return `$${new Intl.NumberFormat('es-ES', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(value)}`;
}

// Función para eliminar una fila de la tabla
function deleteRow(button) {
    const row = button.parentNode.parentNode;
    row.parentNode.removeChild(row);
    updateValues(); // Actualizar valores después de eliminar la fila
}
function deleteRowCosteo(button) {
    const row = button.closest("tr");
    row.remove();
    actualizarSuma("txtValorCosteo-", "sumaValueCosteo"); // Recalcular la suma
}
function deleteRowTable(button, prefixInput, idSumaTotal) {
    const row = button.parentNode.parentNode;
    row.parentNode.removeChild(row);
    //const row = button.closest("tr");  // Encontrar la fila más cercana al botón
    //row.remove();  // Eliminar la fila
    actualizarSuma(prefixInput, idSumaTotal);  // Recalcular la suma
}


/****************************************************************************************************
 *   Función para obtener los datos de la tabla en un formato lineal separado por (;) para guardar  *
 ****************************************************************************************************/

//          FUNCIONA

//function getTablesData(tableIds) {
//    if (!Array.isArray(tableIds)) {
//        console.error('El parámetro debe ser un array de identificadores de tabla.');
//        return '';
//    }

//    let allData = [];

//    tableIds.forEach(tableId => {
//        const table = document.getElementById(tableId)?.getElementsByTagName('tbody')[0];
//        if (!table) {
//            console.error(`Tabla con ID '${tableId}' no encontrada.`);
//            return;
//        }

//        const rows = table.getElementsByTagName('tr');
//        let rowData = [];

//        for (let i = 0; i < rows.length; i++) {
//            const cells = rows[i].getElementsByTagName('td');
//            let rowValues = [];

//            for (let j = 0; j < cells.length - 1; j++) { // Excluye la última celda de acciones
//                const input = cells[j].querySelector('input');
//                let value = input ? input.value : cells[j].innerText.trim();

//                // Verificar si el valor comienza con '$' y convertirlo en un número
//                if (value.startsWith('$')) {
//                    value = value.replace(/[^\d.-]/g, ''); // Eliminar el símbolo $ y cualquier separador de miles
//                    value = parseFloat(value); // Convertir a número
//                }

//                rowValues.push(value);
//            }

//            if (rowValues.length > 0) {
//                rowData.push(rowValues.join('|'));
//            }
//        }

//        if (rowData.length > 0) {
//            allData.push(rowData.join(';'));
//        }
//    });

//    return allData.join(';'); // Unir todas las tablas con ';'
//}

function getTablesData(tableIds) {
    if (!Array.isArray(tableIds)) {
        console.error('El parámetro debe ser un array de identificadores de tabla.');
        return '';
    }

    let allData = [];

    tableIds.forEach(tableId => {
        const table = document.getElementById(tableId)?.getElementsByTagName('tbody')[0];
        if (!table) {
            console.error(`Tabla con ID '${tableId}' no encontrada.`);
            return;
        }

        const rows = table.getElementsByTagName('tr');
        let rowData = [];

        for (let i = 0; i < rows.length; i++) {
            const cells = rows[i].getElementsByTagName('td');
            let rowValues = [];

            for (let j = 0; j < cells.length - 1; j++) { // Excluye la última celda de acciones
                const input = cells[j].querySelector('input');
                const select = cells[j].querySelector('select'); // Verificar si hay un select
                let value;

                if (select) {
                    value = select.value; // Obtener el valor del select
                } else if (input) {
                    value = input.value; // Obtener el valor del input
                } else {
                    value = cells[j].innerText.trim(); // Obtener el texto de la celda
                }

                // Verificar si el valor comienza con '$' y convertirlo en un número
                //if (value.startsWith('$')) {
                //    value = value.replace(/[^\d.-]/g, ''); // Eliminar el símbolo $ y cualquier separador de miles
                //    value = parseFloat(value); // Convertir a número
                //}

                rowValues.push(value);
            }

            if (rowValues.length > 0) {
                rowData.push(rowValues.join('|||'));
            }
        }

        if (rowData.length > 0) {
            allData.push(rowData.join('|;|'));
        }
    });

    return allData.join('-|;|-'); // Unir todas las tablas con ';'
}





// Función para guardar los datos de la tabla.... eliminar
//function saveTableData() {
    
//    const dataToSave = getTablesData();
//    console.log(dataToSave); // Imprimir los datos de la tabla en formato de cadena separada por ';'
//    // Aquí puedes enviar `dataToSave` a tu servidor o base de datos
//}

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
// Funcion para cargar un valor con deciamles al pdf y darle el formato de dinero
function formatearValorParaSpan(valor) {
    if (valor === null || valor === undefined || valor === "") {
        return "$0";
    }

    // Convertir a cadena para manipulación
    let valorStr = String(valor).replace(/[^0-9.,]/g, ''); // Permitir números, comas y puntos

    // Reemplazar comas por puntos si el valor tiene un formato incorrecto
    if (valorStr.includes(',')) {
        valorStr = valorStr.replace(/\./g, '').replace(',', '.'); // Para asegurar formato decimal correcto
    }

    let numero = parseFloat(valorStr);
    if (isNaN(numero)) {
        return "$0";
    }

    // Formatear con separadores de miles (.) y dos decimales (,)
    return '$' + numero.toLocaleString('es-ES', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}



/* ======================================================
 *           Muestra en well o lo oculta
 * ======================================================*/
//function toggleContent(well) {
//    const contenido = well.querySelector('.contenido-oculto');
//    if (contenido.style.display === 'none' || !contenido.style.display) {
//        contenido.style.display = 'block';
//        well.classList.add('open'); // Agrega la clase para indicar que está abierto
//        well.style.backgroundColor = 'rgba(115, 115, 115, 0.4)'; // Cambia el color de fondo a blanco
//        well.style.border = 'none';
//        //well.style.backgroundColor = '#6A8D92'; // Cambia el color de fondo a blanco
//    } else {
//        contenido.style.display = 'none';
//        well.classList.remove('open'); // Quita la clase cuando está cerrado
//        well.style.backgroundColor = '#d8a1a1f4'; // Restaura el color de fondo original
//        well.style.color = '#1a2c34f4';
//    }
//}

///* ===================================================================
// *      Cambio de color en cuadros para seleccionar formulario
// * ===================================================================*/
function toggleContent(well) {
    const contenido = well.querySelector('.contenido-oculto');
    if (contenido.style.display === 'none' || !contenido.style.display) {
        contenido.style.display = 'block';
        well.classList.add('open'); // Marca como abierto
        well.style.backgroundColor = 'rgba(30, 40, 50, 0.35)'; // Color cuando está abierto
        well.style.color = '#ffffff'; // Letras blancas cuando está abierto
        well.style.border = 'none';
        
    } else {
        contenido.style.display = 'none';
        well.classList.remove('open'); // Marca como cerrado
        //well.style.backgroundColor = '#d8a1a1f4'; // Color rosa cuando está cerrado
        well.style.backgroundColor = '#d7a75e'; // Color rosa cuando está cerrado
        well.style.color = 'black'; // Letras negras cuando está cerrado
    }
}

/* ===================================================================
 *      Cambio de color en cuadros para seleccionar formulario
 * ===================================================================*/
document.addEventListener('DOMContentLoaded', () => {
    // Selecciona todos los elementos con la clase "box"
    const boxes = document.querySelectorAll('.box');

    // Recorre cada cuadro y aplica el comportamiento
    boxes.forEach((box) => {
        const originalBgColor = box.style.backgroundColor;
        const originalTextColor = box.style.color;

        box.addEventListener('mouseenter', () => {
            // Solo cambia de color si el cuadro no está abierto
            if (!box.classList.contains('open')) {
                //box.style.backgroundColor = '#d8a1a1f4'; // Color rosa
                box.style.backgroundColor = '#d7a75e'; // Color rosa
                box.style.color = '#007d81'; // Letras negras
            }
            box.style.cursor = 'pointer';
        });

        box.addEventListener('mouseleave', () => {
            // Restaura el color original solo si el cuadro no está abierto
            if (!box.classList.contains('open')) {
                box.style.backgroundColor = originalBgColor;
                box.style.color = originalTextColor;
            }
            box.style.cursor = 'default';
        });
    });
});


//// Funcion para cambiar de color el titulo seleccionado
//function toggleTitle(event, element) {
//    event.preventDefault(); // Evita que la página se desplace hacia arriba

//    // Alternar la clase 'selected' para cambiar el color del botón
//    element.classList.toggle("selected");
//}
function toggleTitle(event, element) {
    event.preventDefault(); // Evita que la página se desplace hacia arriba

    // Alternar la clase 'selected' para cambiar el color del botón
    element.classList.toggle("selected");

    // Actualizar la lista de archivos seleccionados
    actualizarListaArchivos();
}
function actualizarListaArchivos() {
    let botonesSeleccionados = document.querySelectorAll(".title-button.selected");
    let listaArchivosDiv = document.getElementById("listaArchivos2");

    // Limpiar contenido previo
    listaArchivosDiv.innerHTML = "";

    if (botonesSeleccionados.length > 0) {
        let ul = document.createElement("ul"); // Crear una lista
        botonesSeleccionados.forEach(boton => {
            let li = document.createElement("li");
            li.textContent = boton.textContent;
            ul.appendChild(li);
        });
        listaArchivosDiv.appendChild(ul);
    } else {
        listaArchivosDiv.textContent = "No hay archivos seleccionados.";
    }
}



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



/*=======================================================================================
 *          Función para cargar los datos de la base a cada tabla en el HTML
 *======================================================================================*/

function loadTableData(datos, funcionAgregarFila, tableId) {
    if (!datos || typeof datos !== "string") {
        console.error("❌ warning: 'datos' está vacío, es null o no es un string.", datos);
        return;
    }
    if (typeof funcionAgregarFila !== "function") {
        console.error("El parámetro funcionAgregarFila no es una función válida.");
        return;
    }

    const table = document.getElementById(tableId);
    if (!table) {
        console.error(`Tabla con ID '${tableId}' no encontrada.`);
        return;
    }

    const filas = datos.split('|;|').filter(fila => fila.trim() !== ""); // Filtra vacíos

    filas.forEach((fila, index) => {
        funcionAgregarFila(table); // Agregar nueva fila

        let tbody = table.getElementsByTagName("tbody")[0];
        let newRow = tbody.rows[tbody.rows.length - 1]; // Última fila agregada

        if (!newRow) {
            console.error("No se pudo encontrar la fila recién creada.");
            return;
        }

        const columnas = fila.split('|||').map(col => col.trim());

        // Obtener todas las celdas de la nueva fila
        let celdas = newRow.cells;

        columnas.forEach((dato, index) => {
            if (celdas[index]) {
                let input = celdas[index].querySelector("input, select");

                if (input) {
                    if (input.tagName === "INPUT") {
                        let inputValue = dato;

                        // Si es un campo de fecha, convertir formato a YYYY-MM-DD
                        if (input.type === "date" && inputValue.includes('/')) {
                            let partes = inputValue.split('/');
                            inputValue = `${partes[2]}-${partes[1]}-${partes[0]}`;
                        }

                        // Si es un campo de "Valor ($)", aplicar formato
                        if (input.id && input.id.includes("txtValorContrato")) {
                            input.value = inputValue;
                            formatearValorGeneral(input);
                        } else {
                            input.value = inputValue;
                        }
                    }
                    else if (input.tagName === "SELECT") {
                        // Buscar la opción en el select y seleccionarla
                        let opciones = input.options;
                        for (let i = 0; i < opciones.length; i++) {
                            if (opciones[i].value === dato || opciones[i].text === dato) {
                                input.selectedIndex = i;
                                break;
                            }
                        }
                    }
                } else {
                    console.warn(`No se encontró input o select en la celda ${index + 1} con valor: ${dato}`);
                }
            }
        });

        actualizarSuma("txtValorServExt-", "sumaValueSerExt");
        actualizarSuma("txtValorCosteo-", "sumaValueCosteo");
        actualizarSuma("txtValorHwd-", "sumaValueSerHwd");
        actualizarSuma("txtValorLic-", "sumaValueSerLic");
        actualizarSuma("txtValorServFab-", "sumaValueServFab");
        actualizarSuma("txtValorPoliza-", "sumaValuePoliza");

        console.log("Fila cargada correctamente:", newRow);
    });

    console.log("Todos los datos fueron cargados correctamente.");
}





/*=======================================================================================
 *      Función para cargar los datos de forma pago a una tabla en el PDF
 *======================================================================================*/
//  FUNCIONA BN
//function cargarDatosEnTablaPDF(data, tableId) {
//    const table = document.querySelector(`#${tableId}`);
//    if (!table) {
//        console.error(`No se encontró la tabla con ID: ${tableId}`);
//        return;
//    }

//    const tableBody = table.querySelector('tbody');
//    tableBody.innerHTML = ''; // Limpiar el contenido existente

//    if (!data) {
//        console.error('No hay datos para cargar');
//        return;
//    }

//    const rows = data.split(';');
//    const hasIndex = table.querySelector('thead th:first-child')?.textContent.trim() === "#"; // Detecta si hay índice

//    rows.forEach((row, index) => {
//        const cells = row.split('|');
//        if (cells.length >= 2) { // Verifica que al menos haya dos columnas
//            const newRow = document.createElement('tr');

//            // Si la tabla tiene índice, agregamos el número de fila
//            if (hasIndex) {
//                const cellIndex = document.createElement('td');
//                cellIndex.textContent = index + 1;
//                newRow.appendChild(cellIndex);
//            }

//            // Agregar las demás celdas dinámicamente
//            cells.forEach(cell => {
//                const newCell = document.createElement('td');
//                newCell.textContent = cell;
//                newRow.appendChild(newCell);
//            });

//            tableBody.appendChild(newRow);
//        } else {
//            console.error('El formato de la fila no es válido:', row);
//        }
//    });
//}

function cargarDatosEnTablaPDF(data, tableId) {
    const table = document.querySelector(`#${tableId}`);
    if (!table) {
        console.error(`No se encontró la tabla con ID: ${tableId}`);
        return;
    }

    const tableBody = table.querySelector('tbody');
    tableBody.innerHTML = ''; // Limpiar contenido existente

    if (!data) {
        console.error('No hay datos para cargar');
        return;
    }

    const rows = data.split('|;|');
    const headers = Array.from(table.querySelectorAll('thead th')).map(th => th.textContent.trim()); // Obtener cabeceras
    const hasIndex = headers[0] === "#"; // Detecta si hay índice en la primera columna
    const valorIndex = headers.indexOf("Valor ($)"); // Obtiene el índice de la columna "Valor ($)"

    rows.forEach((row, index) => {
        const cells = row.split('|||');
        if (cells.length >= 2) { // Verifica que haya al menos dos columnas
            const newRow = document.createElement('tr');

            // Si la tabla tiene índice, agregar el número de fila
            if (hasIndex) {
                const cellIndex = document.createElement('td');
                cellIndex.textContent = index + 1;
                newRow.appendChild(cellIndex);
            }

            // Agregar las demás celdas dinámicamente
            cells.forEach((cell, cellIndex) => {
                const newCell = document.createElement('td');
                newCell.textContent = cell;

                // Si es la columna "Valor ($)", agregar el evento oninput
                if (cellIndex === valorIndex) {
                    newCell.setAttribute("oninput", "formatearValorGeneral(this);");
                }

                newRow.appendChild(newCell);
            });

            tableBody.appendChild(newRow);
        } else {
            console.error('El formato de la fila no es válido:', row);
        }
    });

    console.log("Datos cargados correctamente en la tabla.");
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

    CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', Datos, "tableSelectBusquedaOS", tipo2);
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

    let ordenesConcatenadas = ""; // Variable para almacenar las órdenes de servicio concatenadas

    ordenesServicio.forEach(orden => {
        const item = document.createElement('div');
        item.textContent = orden.ORDEN; // Asegurarse de acceder al campo ORDEN
        contenedor.appendChild(item);

        // Concatenar las órdenes de servicio con un punto y coma
        if (ordenesConcatenadas === "") {
            ordenesConcatenadas = orden.ORDEN;
        } else {
            ordenesConcatenadas += ";" + orden.ORDEN;
        }
    });

    contenedor.style.display = 'block';
}
//------------------------------------------------------------------------------------------------




/***************************************************************************************
                                        PDF
 ***************************************************************************************/
//function MostrarPDF(numContract) {
//    // Verificar si el campo con id="txtNumContrato" tiene algún valor
//    let numContratoValue = document.getElementById("txtNumContrato").value;

//    // Mostrar el modal independientemente de si el campo está vacío
//    $("#ModalEgresoInventarioPDF").modal('show');

//    // Solo ejecutar BuscarEstadoNotificacion si el campo no está vacío
//    if (numContratoValue.trim() !== "") {
//        BuscarEstadoNotificacion(numContract, "", 3);
//    }
//}
// Funciona perfecto
function MostrarPDF(numContract) {
    let numContratoValue = document.getElementById("txtNumContrato").value.trim();
    let numPedidoValue = document.getElementById("txtConPedido").value.trim();

    // Mostrar el modal siempre
    $("#ModalEgresoInventarioPDF").modal('show');

    // Verificar primero si hay un número de contrato, si no, revisar el número de pedido
    if (numContratoValue !== "") {
        BuscarEstadoNotificacion(numContratoValue, 1, 3);
    } else if (numPedidoValue !== "") {
        BuscarEstadoNotificacion(numPedidoValue, 2, 3);
    }
}




function RecorreJSONTableSelectContratoPDF(json, boton, idSeleccionado) {
    dtContratoDatos(json);    
}
function RecorreJSONTableContratoConsulta(json, boton, idSeleccionado) {
    // Verifica si json es un array y tiene datos
    if (Array.isArray(json) && json.length > 0) {
        document.getElementById("tabla_clientes_consulta").style.display = "flex";
        dtContratoDatosConsulta(json);
    } else {
        console.warn("El JSON está vacío o no es válido.");
        // Puedes agregar una acción aquí, como mostrar un mensaje en la página
        $('#tbl_ProyectosConsulta tbody').html('<tr><td colspan="4">No hay datos disponibles</td></tr>');
    }
}




function dtContratoDatos(json) {

    var Cliente = document.getElementById("txtPDFCliente");
    Cliente.textContent = json.CLIENTE;   

    var numPedido = document.getElementById("txtPDFNumeroPedido");
    numPedido.textContent = json.NUM_PEDIDO;

    var numContrato = document.getElementById("txtPDFNumeroContrato");
    numContrato.textContent = json.NUM_CONTRATO;
    var numContratoOBS = document.getElementById("txtPDFNumeroContratoOBS");
    numContratoOBS.textContent = json.OBS_NUM_CONTRATO;

    var valorContrato = document.getElementById("txtPDFValorTotalContrato");
    valorContrato.textContent = formatearValorParaSpan(json.VALOR_TOTAL_CONTRATO);
    var valorContratoOBS = document.getElementById("txtPDFValorTotalContratoOBS");
    valorContratoOBS.textContent = json.OBS_VALOR_TOTAL;

    ObtenerRentabilidad();

    var objeto = document.getElementById("txtPDFObjeto");
    objeto.textContent = json.OBJETO;
    var objetoOBS = document.getElementById("txtPDFObjetoOBS");
    objetoOBS.textContent = json.OBS_OBJETO;

    //var servDos = document.getElementById("txtPDFServiciosDOS");
    //servDos.textContent = json.SERVICIO_DOS;
    var servDosOBS = document.getElementById("txtPDFServiciosDOSOBS"); 
    servDosOBS.textContent = json.OBS_SERVICIO_DOS;

    //var servExt = document.getElementById("txtPDFServiciosExternos");
    //servExt.textContent = json.SERVICIO_EXTERNOS;
    var servExtOBS = document.getElementById("txtPDFServiciosExternosOBS"); 
    servExtOBS.textContent = json.OBS_SERVICIO_EXTERNOS;

    //var alcance = document.getElementById("txtPDFAlcance");
    //alcance.textContent = json.ALCANCE;
    var alcanceOBS = document.getElementById("txtPDFAlcanceOBS");
    alcanceOBS.textContent = json.OBS_ALCANCE;

    //var hardware = document.getElementById("txtPDFHardware");
    //hardware.textContent = json.HARDWARE;
    var hardwareOBS = document.getElementById("txtPDFHardwareOBS");
    hardwareOBS.textContent = json.OBS_HARDWARE;

    //var licencias = document.getElementById("txtPDFLicencias");
    //licencias.textContent = json.LICENCIAS;
    var licenciasOBS = document.getElementById("txtPDFLicenciasOBS"); 
    licenciasOBS.textContent = json.OBS_LICENCIAS;    

    //var servFab = document.getElementById("txtPDFServiciosFabricante");
    //servFab.textContent = json.SERVICIOS_FABRICANTE;
    var servFabOBS = document.getElementById("txtPDFServiciosFabricanteOBS");
    servFabOBS.textContent = json.OBS_SERVICIOS_FABRICANTE;

    //var polizas = document.getElementById("txtPDFPolizas"); 
    //polizas.textContent = json.POLIZAS;
    var polizasOBS = document.getElementById("txtPDFPolizasOBS");
    polizasOBS.textContent = json.OBS_POLIZAS;

    var formasPagoOBS = document.getElementById("txtPDFFormasPagoOBS");
    formasPagoOBS.textContent = json.OBS_FORMA_PAGO;

    //var terTdr = document.getElementById("txtPDFTerminosReferencia");
    //terTdr.textContent = json.TERMINOS_TDR;
    //var terTdrOBS = document.getElementById("txtPDFTerminosReferenciaOBS");
    //terTdrOBS.textContent = json.OBS_TERMINOS_TDR;
   
    //var actaPreguntas = document.getElementById("txtPDFActaPreguntasRespuestas");
    //actaPreguntas.textContent = json.ACTA_PREGUNTAS;
    //var actaPreguntasOBS = document.getElementById("txtPDFActaPreguntasRespuestasOBS"); 
    //actaPreguntasOBS.textContent = json.OBS_ACTA_PREGUNTAS;

    //var actaAdj = document.getElementById("txtPDFActaAdjudicacion");
    //actaAdj.textContent = json.ACTA_ADJUDICACION;
    //var actaAdjOBS = document.getElementById("txtPDFActaAdjudicacionOBS"); 
    //actaAdjOBS.textContent = json.OBS_ACTA_ADJUDICACION;

    //var actaNeg = document.getElementById("txtPDFActaNegociacion");
    //actaNeg.textContent = json.ACTA_NEGOCIACION;
    //var actaNegOBS = document.getElementById("txtPDFActaNegociacionOBS");
    //actaNegOBS.textContent = json.OBS_ACTA_NEGOCIACION;

    //var bomSolucion = document.getElementById("txtPDFBoMSolucion");
    //bomSolucion.textContent = json.BOM_SOLUCION;
    //var bomSolucionOBS = document.getElementById("txtPDFBoMSolucionOBS");
    //bomSolucionOBS.textContent = json.OBS_BOM_SOLUCION;

    //var acuMay = document.getElementById("txtPDFAcuerdosMayoristas");
    //acuMay.textContent = json.ACUERDOS_MAY;
    //var acuMayOBS = document.getElementById("txtPDFAcuerdosMayoristasOBS");
    //acuMayOBS.textContent = json.OBS_ACUERDOS_MAY;

    //var acuFab = document.getElementById("txtPDFAcuerdosFabricantes");
    //acuFab.textContent = json.ACUERDOS_FAB;
    //var acuFabOBS = document.getElementById("txtPDFAcuerdosFabricantesOBS");
    //acuFabOBS.textContent = json.OBS_ACUERDOS_FAB;

    //var garFin = document.getElementById("txtPDFGarantiasFIN");
    //garFin.textContent = json.GARANTIAS_FIN;
    //var garFinOBS = document.getElementById("txtPDFGarantiasFINOBS");
    //garFinOBS.textContent = json.OBS_GARANTIAS_FIN;

    //var garTec = document.getElementById("txtPDFGarantiasLicenciasTEC"); 
    //garTec.textContent = json.GARANTIAS_TEC;
    //var garTecOBS = document.getElementById("txtPDFGarantiasLicenciasTECOBS");
    //garTecOBS.textContent = json.OBS_GARANTIAS_TEC;

    //var genPedidos = document.getElementById("txtPDFGeneracionPedidos");
    //genPedidos.textContent = json.GENERACION_PEDIDOS;
    //var ordServicio = document.getElementById("txtPDFOrdenesServicio");
    //ordServicio.textContent = json.ORDEN_SERVICIO;
    //var genPedidosOBS = document.getElementById("txtPDFGeneracionPedidosOBS");
    //genPedidosOBS.textContent = json.OBS_GENERACION_PEDIDOS;

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

    cargarDatosEnTablaPDF(json.SERVICIO_EXTERNOS, "tbl_pdfServExt");
    var valServExt = document.getElementById("sumaValueSerExtPDF");
    valServExt.textContent = document.getElementById("sumaValueSerExt").textContent;

    cargarDatosEnTablaPDF(json.ALCANCE, "tbl_pdfCosteo");
    var valCosteo = document.getElementById("sumaValueCosteoPDF");
    valCosteo.textContent = document.getElementById("sumaValueCosteo").textContent;

    cargarDatosEnTablaPDF(json.HARDWARE, "tbl_pdfHardware");
    var valHdw = document.getElementById("sumaValueSerHwdPDF");
    valHdw.textContent = document.getElementById("sumaValueSerHwd").textContent;

    cargarDatosEnTablaPDF(json.LICENCIAS, "tbl_pdfLicencias");
    var valLicencias = document.getElementById("sumaValueSerLic").innerText;
    document.getElementById("sumaValueSerLicPDF").innerText = valLicencias;

    cargarDatosEnTablaPDF(json.SERVICIOS_FABRICANTE, "tbl_pdfServiciosFab");
    var valSerFab = document.getElementById("sumaValueServFabPDF");
    valSerFab.textContent = document.getElementById("sumaValueServFab").textContent;

    cargarDatosEnTablaPDF(json.POLIZAS, "tbl_pdfPolizas"); 
    var valPolizas = document.getElementById("sumaValuePolizaPDF");
    valPolizas.textContent = document.getElementById("sumaValuePoliza").textContent;

    cargarDatosEnTablaPDF(json.FORMA_PAGO, "tbl_FormasPago"); 
    var valForPago = document.getElementById("remainingValuePDF");
    valForPago.textContent = document.getElementById("remainingValue").textContent;

    //document.getElementById("TablaArchivos").style.display = "block";    
    VerArchivosPDF();

    //var formasPago = document.getElementById("txtPDFFormasPago");
    //formasPago.textContent = json.FORMA_PAGO;
}



//---------> Seccion donde ingresa informacion a la tabla del modal Cosulta -----------------
function Create3() {
    if ($.fn.DataTable.isDataTable('#tbl_ProyectosConsulta')) {
        $('#tbl_ProyectosConsulta').DataTable().destroy();
    }
    $('#tbl_ProyectosConsulta tbody').empty();
}
function dtContratoDatosConsulta(json) {
    // Elimina cualquier DataTable previo
    if ($.fn.DataTable.isDataTable('#tbl_ProyectosConsulta')) {
        $('#tbl_ProyectosConsulta').DataTable().destroy();
    }

    // Limpia la tabla antes de agregar nuevos datos
    $('#tbl_ProyectosConsulta tbody').empty();

    // Recorre los datos JSON y agrega filas a la tabla manualmente
    json.forEach(item => {
        let fila = `<tr>
                        <td>${item.CLIENTE}</td>
                        <td>${item.NUM_CONTRATO}</td>
                        <td>${item.NUM_PEDIDO}</td>
                        <td><a title='Seleccionar' class='btn btn-seleccionarProy btn-xs'>
                                <i class='glyphicon glyphicon-hand-left' aria-hidden='true'></i>
                            </a>
                        </td>
                    </tr>`;
        $('#tbl_ProyectosConsulta tbody').append(fila);
    });
}
$(document).on('click', '.btn-seleccionarProy', function () {
    var fila = $(this).closest('tr');

    // Elimina la clase de selección de todas las filas
    $('#tbl_ProyectosConsulta tbody tr').removeClass('fila-seleccionada');

    // Restaura todos los iconos a la mano izquierda
    $('#tbl_ProyectosConsulta tbody tr .btn-seleccionarProy i')
        .removeClass('glyphicon-ok text-success')
        .addClass('glyphicon-hand-left');

    // Agrega la clase para resaltar la fila seleccionada
    fila.addClass('fila-seleccionada');

    // Cambia el icono de la fila seleccionada a un check verde
    fila.find('.btn-seleccionarProy i')
        .removeClass('glyphicon-hand-left')
        .addClass('glyphicon-ok text-success');

    // Obtiene los valores de la fila seleccionada
    var numContrato = fila.find("td:eq(1)").text().trim();
    var numPedido = fila.find("td:eq(2)").text().trim();

    // Determina qué número asignar al input
    var numeroSeleccionado = numContrato || numPedido;

    if (numeroSeleccionado) {
        $('#numero').val(numeroSeleccionado);
    } else {
        alert("No hay número de contrato ni número de pedido en esta fila.");
    }
});






/********************************************************************
 *          Funcion para descargar el HTML en formato PDF
 ********************************************************************/

   //       FUNCIONA MUY BIEn

//function descargarModalComoPDF(modalId, nombreArchivo = "documento.pdf") {
//    const modalBody = document.querySelector(`#${modalId} .modal-body`);

//    if (!modalBody) {
//        alert("No se encontró el contenido del modal.");
//        return;
//    }

//    // Clonar el modal-body para evitar interferencias
//    let clone = modalBody.cloneNode(true);
//    clone.style.display = "block";

//    // Contenedor temporal fuera de pantalla
//    let tempDiv = document.createElement("div");
//    tempDiv.style.position = "absolute";
//    tempDiv.style.left = "-9999px";
//    tempDiv.appendChild(clone);
//    document.body.appendChild(tempDiv);

//    html2canvas(clone, {
//        scale: 1, // Mayor resolución
//        useCORS: true
//    }).then(canvas => {
//        const imgData = canvas.toDataURL("image/png");

//        const { jsPDF } = window.jspdf;
//        let pdf = new jsPDF({
//            orientation: "p", // "p" para vertical
//            unit: "mm",
//            format: "a4"
//        });

//        let pageWidth = pdf.internal.pageSize.getWidth();
//        let pageHeight = pdf.internal.pageSize.getHeight();
//        let imgWidth = pageWidth - 10; // Margen de 5mm a cada lado
//        let imgHeight = (canvas.height * imgWidth) / canvas.width;

//        if (imgHeight > pageHeight) {
//            // Si la imagen es muy alta, la ajustamos dividiendo en páginas
//            let yPos = 0;
//            while (yPos < imgHeight) {
//                pdf.addImage(imgData, "PNG", 5, 5 - yPos, imgWidth, imgHeight);
//                yPos += pageHeight;
//                if (yPos < imgHeight) pdf.addPage();
//            }
//        } else {
//            pdf.addImage(imgData, "PNG", 5, 5, imgWidth, imgHeight);
//        }

//        pdf.save(nombreArchivo);
//        document.body.removeChild(tempDiv);
//    });
//}


//  OPCION 2 FUNCIONA BN
//function descargarModalComoPDF(modalId) {

//    var nombreArchivo = "NEW TRANSFER MEETING " + $('#txtNumContrato').val()+".pdf";

//    const modalBody = document.querySelector(`#${modalId} .modal-body`);

//    if (!modalBody) {
//        alert("No se encontró el contenido del modal.");
//        return;
//    }

//    // Clonar el modal-body para evitar interferencias
//    let clone = modalBody.cloneNode(true);
//    clone.style.display = "block";

//    // Contenedor temporal fuera de pantalla
//    let tempDiv = document.createElement("div");
//    tempDiv.style.position = "absolute";
//    tempDiv.style.left = "-9999px";
//    tempDiv.appendChild(clone);
//    document.body.appendChild(tempDiv);

//    html2canvas(clone, {
//        scale: 2, // Aumentamos la calidad sin que el archivo sea muy pesado
//        useCORS: true
//    }).then(canvas => {
//        // Convertir la imagen a JPEG para reducir el tamaño del archivo
//        const imgData = canvas.toDataURL("image/jpeg", 0.8);

//        const { jsPDF } = window.jspdf;
//        let pdf = new jsPDF({
//            orientation: "p", // "p" para vertical
//            unit: "mm",
//            format: "a4"
//        });

//        let pageWidth = pdf.internal.pageSize.getWidth();
//        let pageHeight = pdf.internal.pageSize.getHeight();
//        let imgWidth = pageWidth - 10; // Margen de 5mm a cada lado
//        let imgHeight = (canvas.height * imgWidth) / canvas.width;

//        let yPos = 5;
//        if (imgHeight > pageHeight + 2) {
//            while (yPos < imgHeight) {
//                pdf.addImage(imgData, "JPEG", 5, 5 - yPos, imgWidth, imgHeight);
//                yPos += pageHeight - 10;
//                if (yPos < imgHeight) pdf.addPage();
//            }
//        } else {
//            pdf.addImage(imgData, "JPEG", 5, 5, imgWidth, imgHeight);
//        }

//        pdf.save(nombreArchivo);
//        document.body.removeChild(tempDiv);
//    });
//}
function descargarModalComoPDF(modalId) {
    var nombreArchivo = "NEW TRANSFER MEETING " + $('#txtNumContrato').val() + ".pdf";

    const modalBody = document.querySelector(`#${modalId} .modal-body`);
    if (!modalBody) {
        alert("No se encontró el contenido del modal.");
        return;
    }

    // Capturar el modal como imagen
    html2canvas(modalBody, {
        scale: 2, // Aumenta la resolución
        useCORS: true
    }).then(canvas => {
        const imgData = canvas.toDataURL("image/png"); // Convertir a imagen PNG

        // Crear el PDF con jsPDF
        const { jsPDF } = window.jspdf;
        let pdf = new jsPDF({
            orientation: "p", // Vertical
            unit: "mm",
            format: "a4"
        });

        // Obtener dimensiones de la imagen y ajustar al tamaño del PDF
        let imgWidth = 210; // A4 width en mm
        let pageHeight = 297; // A4 height en mm
        let imgHeight = (canvas.height * imgWidth) / canvas.width; // Mantener proporción

        if (imgHeight > pageHeight) {
            pdf.addImage(imgData, "PNG", 0, 0, imgWidth, pageHeight); // Ajuste en caso de sobrepasar
        } else {
            pdf.addImage(imgData, "PNG", 0, 10, imgWidth, imgHeight); // Imagen dentro del margen
        }

        // Descargar el PDF
        pdf.save(nombreArchivo);
    }).catch(error => {
        console.error("Error al capturar el modal:", error);
        alert("Hubo un error al generar el PDF.");
    });
}



/*********************************************************************
 *                 Actualizamos la tabla de Polizas                  *
 *********************************************************************/
function BtnConsultaPoliza() {
    //IdPedidos = 0;
    var fechaInicio = "";
    var fechaFinal = "";
    var IdPedidos = $('#txtConPedido').val();

    fechaInicio = "";
    fechaFinal = "";


    ObtenerConsultarPolizasDetalleFiltros(IdPedidos, 0, fechaInicio, fechaFinal);
}
function ObtenerConsultarPolizasDetalleFiltros(idPedidos, tipo, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ReporteConsultarPolizasFiltros\", \"parameters\" : { buscar: \"" + idPedidos + "\", fechaInicio: \"" + fechaInicio + "\", fechaFinal: \"" + fechaFinal + "\",idPedidos: \"" + "" + "\",beneficiario: \"" + "" + "\",Proceso: \"" + "" + "\",idFecha: \"" + "" + "\"} }]";
    CargarPagina('#datosTablaPrincipalPolizas', 'ObtenerNuevaListaTareas.ashx', Datos, "tableSelectPolizas", tipo);
}
function RecorreJSONTableSelectPolizas(json, boton, idSeleccionado) {
    // Verificamos si el JSON está vacío o no tiene datos
    if (!json || (Array.isArray(json) && json.length === 0)) {
        MensajeAlerta("No hay pólizas por cargar.");
        return;
    }

    // Si hay datos, se continúa con la carga
    cargarDatosPoliza(json);
}


//  Limpiamos la tabla antes de cargar lo datos
function clearTablePoliza() {
    const table = document.getElementById('dynamicTablePoliza').getElementsByTagName('tbody')[0];
    table.innerHTML = ""; // Elimina todas las filas existentes
}
// Nos aseguramos de que el formato sea el correcto antes de cargar las fechas
function formatDate(dateString) {
    if (!dateString) return ""; // Si no hay fecha, retornar vacío
    const parts = dateString.split("/");
    if (parts.length === 3) {
        return `${parts[2]}-${parts[1]}-${parts[0]}`; // Convertir a formato YYYY-MM-DD
    }
    return "";
}
function cargarDatosPoliza(jsonData) {
    clearTablePoliza(); // Limpiar la tabla antes de cargar nuevos datos

    jsonData.forEach((data, index) => {
        const table = document.getElementById('dynamicTablePoliza').getElementsByTagName('tbody')[0];
        const newRow = table.insertRow();

        // Insertar celdas
        const descriptionCell = newRow.insertCell(0);
        const tipeCell = newRow.insertCell(1);
        const fecEmisionCell = newRow.insertCell(2);
        const fecCaducidadCell = newRow.insertCell(3);
        const valueCell = newRow.insertCell(4);
        const actionCell = newRow.insertCell(5);

        // Insertar datos en las celdas
        descriptionCell.innerHTML = `
            <div style="display:flex; justify-content: center;">
                <input id="txtDesPoliza-${index}" class="form-control" type="text" value="${data.OBJETO}" placeholder="Descripción">
            </div>`;

        // Crear el select con JavaScript para poder seleccionar opción por código
        const selectHtml = `
            <select id="txtTipoPoliza-${index}" class="form-control" style="font-size:12px;">
                <option value="">- Seleccionar -</option>
                <option value="BUEN USO DEL ANTICIPO">BUEN USO DEL ANTICIPO</option>
                <option value="FIEL CUMPLIMIENTO DEL CONTRATO">FIEL CUMPLIMIENTO DEL CONTRATO</option>
                <option value="GARANTIA BANCARIA">GARANTIA BANCARIA</option>
            </select>`;
        tipeCell.innerHTML = `<div style="display:flex;">${selectHtml}</div>`;

        // Normalizar el valor recibido para que coincida con el del select
        const tipoPoliza = (data.TipoPoliza || '').toUpperCase().trim();

        // Obtener el select recién creado y establecer el valor correspondiente
        const selectElement = document.getElementById(`txtTipoPoliza-${index}`);

        if (tipoPoliza.includes("BUEN USO")) {
            selectElement.value = "BUEN USO DEL ANTICIPO";
        } else if (tipoPoliza.includes("FIEL CUMPLIMIENTO")) {
            selectElement.value = "FIEL CUMPLIMIENTO DEL CONTRATO";
        } else if (tipoPoliza.includes("GARANTIA")) {
            selectElement.value = "GARANTIA BANCARIA";
        }

        fecEmisionCell.innerHTML = `
            <div style="display:flex;">
                <input id="txtfecEmisionPoliza-${index}" class="form-control" type="date" value="${formatDate(data.FechaInicio)}">
            </div>`;

        fecCaducidadCell.innerHTML = `
            <div style="display:flex;">
                <input id="txtfecCaducidadPoliza-${index}" class="form-control" type="date" value="${formatDate(data.FechaFin)}">
            </div>`;

        valueCell.innerHTML = `
            <div style="display:flex; justify-content: center;">
                <input id="txtValorPoliza-${index}" type="text" class="form-control" placeholder="$"
                value="${data.VALOR.replace('.', ',')}" 
                oninput="formatearValorGeneral(this); actualizarSuma('txtValorPoliza-', 'sumaValuePoliza');">
            </div>`;


        actionCell.innerHTML = '<button class="delete-button" onclick="deleteRowTable(this, \'txtValorPoliza-\', \'sumaValuePoliza\')"><i class="fa fa-trash-o" aria-hidden="true"></i></button>';
    });

    actualizarSuma("txtValorPoliza-", "sumaValuePoliza");
    updateValues(); // Actualizar valores después de agregar todas las filas
}






/******************************************************************************************************
 *      Obtenemos la lista de archivos existentes en la carpeta del proyecto para el PDF y el HTML   
 ******************************************************************************************************/
function VerArchivosPDF() {
    Codigo = $('#txtConPedido').val();
    //document.getElementById("editBoton").style.display = "none";

    ObtenerListaArchivos(Codigo);
}
function ObtenerListaArchivos(Codigo,  tipo2) {
    var DatosLF = "[{ \"action\": \"BuscarListaArchivosContrato\", \"parameters\" : { numContrato : \"" + Codigo + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', DatosLF, "tableSelectArchivosPDF", tipo2);
}
function RecorreJSONTableSelectArchivoPDF(json, boton, idSeleccionado) {
    mostrarArchivos(json);
    //cargarDatosEnTablaPDF(json.NOMBRE, "tbl_Formularios");
}
function Create() {
    if ($.fn.DataTable.isDataTable('#tbl_Formularios')) {
        $('#tbl_Formularios').DataTable().destroy();
    }
    $('#tbl_Formularios tbody').empty();
}
//function mostrarArchivos(json) {
//    let contenedor = document.getElementById("listaArchivos");
//    contenedor.innerHTML = ""; // Limpiar lista antes de agregar nuevos archivos

//    //if (json.length === 0) {
//    //    contenedor.innerHTML = "<p>No hay archivos disponibles.</p>";
//    //    return;
//    //}
//    if (!json || Object.keys(json).length === 0) {
//        console.warn("El JSON está vacío o no tiene datos válidos:", json);
//        return;
//    }else {
//        let ul = document.createElement("ul"); // Crear lista
//        console.log("Contenido de json:", json);
//        console.log("Es un array?", Array.isArray(json));

//        json.forEach(archivo => {
//            // Verificar si el nombre del archivo no comienza con "eliminado-"
//            if (!archivo.Nombre.startsWith("eliminado-")) {
//                let li = document.createElement("li");
//                li.textContent = archivo.Nombre; // Agregar solo el nombre del archivo
//                ul.appendChild(li);
//            }
//        });

//        contenedor.appendChild(ul);
//    }
//}
function mostrarArchivos(json) {
    let contenedor = document.getElementById("listaArchivos");
    contenedor.innerHTML = ""; // Limpiar lista antes de agregar nuevos archivos

    try {
        if (!json || !Array.isArray(json) || json.length === 0) {
            console.warn("No hay archivos disponibles o el JSON no es un array válido:", json);
            contenedor.innerHTML = "<p>No hay archivos disponibles.</p>";
            return;
        }

        let ul = document.createElement("ul"); // Crear lista
        console.log("Contenido de json:", json);

        json.forEach(archivo => {
            if (archivo?.Nombre && !archivo.Nombre.startsWith("eliminado-")) {
                let li = document.createElement("li");
                li.textContent = archivo.Nombre; // Agregar solo el nombre del archivo
                ul.appendChild(li);
            }
        });

        if (ul.children.length > 0) {
            contenedor.appendChild(ul);
        } else {
            contenedor.innerHTML = "<p>No hay archivos disponibles.</p>";
        }

    } catch (error) {
        console.error("Error al procesar los archivos:", error);
        contenedor.innerHTML = "<p>Error al cargar los archivos.</p>";
    }
}



function VerArchivos() {
    Codigo = $('#txtConPedido').val();
    //document.getElementById("editBoton").style.display = "none";

    ObtenerListaFormularios(Codigo);
}
function ObtenerListaFormularios(Codigo, tipo2) {
    var DatosLF = "[{ \"action\": \"BuscarListaArchivosContrato\", \"parameters\" : { numContrato : \"" + Codigo + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', DatosLF, "tableSelectArchivos", tipo2);
}
function RecorreJSONTableSelectArchivo(json, boton, idSeleccionado) {

    dtArchivos(json);
}
function Create2() {
    if ($.fn.DataTable.isDataTable('#tbl_Archivos')) {
        $('#tbl_Archivos').DataTable().destroy();
    }
    $('#tbl_Archivos tbody').empty();
}
function dtArchivos(json) {
    Create2();
    table2 = null;

    table2 = $('#tbl_Archivos').DataTable({
        data: json,
        columns: [
            {
                data: 'Nombre', render: function (data) {
                    // Si el archivo está marcado como eliminado, lo mostramos en gris
                    return data.startsWith('eliminado-') ? `<span style="color:#a5a5a5">${data}</span>` : data;
                }
            },
            {
                // Aquí vamos a modificar la columna de las acciones
                data: 'Nombre', render: function (data) {
                    // Si el nombre empieza con 'eliminado-', cambiamos el ícono a uno de restaurar
                    if (data.startsWith('eliminado-')) {
                        return `<a title='Restaurar archivo' class='btn btn-restaurarArchivo btn-xs'>
                                    <i class='fa fa-undo' aria-hidden='true' style='color:white'></i>
                                </a>`;
                    } else {
                        return `<a title='Ver archivo' class='btn btn-abrirFormulario btn-xs'>
                                    <i class='glyphicon glyphicon-save' aria-hidden='true' style='color:white'></i>
                                </a>  
                                <a title='Eliminar' class='btn btn-eliminarArchivo btn-xs'>
                                    <i class='fa fa-trash-o' aria-hidden='true' style='color:white'></i>
                                </a>`;
                    }
                }
            }
        ],
        language: {
            "decimal": ",",
            "thousands": ".",
            "emptyTable": "No hay información",
            "infoEmpty": "Mostrando 0 to 0 of 0 Entradas",
            "infoPostFix": "",
            "thousands": ",",
            "loadingRecords": "Cargando...",
            "processing": "Procesando...",
            "search": "Buscar:",
            "zeroRecords": "Sin resultados encontrados",
        },
        orderCellsTop: false,
        fixedHeader: true,

        // 🔹 Oculta el selector de cantidad de filas y la paginación
        lengthChange: false,   // Oculta "Show X entries"
        paging: false,         // Oculta la paginación ("Previous | Next")
        info: false,           // Oculta "Showing X to Y of Z entries"
        searching: false       // 🔹 Oculta el input de búsqueda
    });
}

// FUNCIONA BIEN PERO LA VERSION .0 ES LA SIGUIENTE
//$(document).on('click', '.btn-abrirFormulario', function () {
//    // Obtiene el nombre del archivo desde los datos de la fila
//    var data = table2.row($(this).closest('tr')).data();
//    var nombreArchivo = data.Nombre; // Asegúrate de que el nombre sea el correcto
//    var nombreCarpeta = $('#txtConPedido').val();
//    // Construye la URL completa al archivo
//    //var url = '/HistoriasClinicas/' + nombreCarpeta + '/' + nombreArchivo;
//    var DatosA = "[{ \"action\": \"AbrirDocArchivo\", \"parameters\" : { nombreArchivo : \"" + nombreArchivo + "\", nombreCarpeta: \"" + nombreCarpeta + "\"} }]";
//    CargarAbrirArchivo('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', DatosA, "tableSelectArchivos");
//});
//function CargarAbrirArchivo(div, url, datos, tipoControl, boton, idSeleccionado) {

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
//                var mensaje = "";
//                if (respuesta.estado == "1") {
//                    MensajeCorrecto("El archivo se ha descargado con exito.");
//                    //VerListaEmpleados();
//                    $("#divMensajes").html("");
//                    window.open(respuesta.mensaje);
//                }
//                else if (respuesta.estado == "0") {
//                    MensajeIncorrecto("Ha ocurrido un error al descargar el archivo: ");
//                }
//            },
//            error: function (objeto, msgError, objError) {
//                var mesnajeError = "La acción de está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
//                MensajeIncorrecto(mesnajeError);
//            }            
//        });
//    }
//}
$(document).on('click', '.btn-abrirFormulario', function () {
    // Obtiene el nombre del archivo desde los datos de la fila
    var data = table2.row($(this).closest('tr')).data();
    var nombreArchivo = data.Nombre; // Asegúrate de que el nombre sea el correcto
    var nombreCarpeta = $('#txtConPedido').val();
    // Construye la URL completa al archivo
    //var url = '/HistoriasClinicas/' + nombreCarpeta + '/' + nombreArchivo;
    var DatosA = "[{ \"action\": \"AbrirDocArchivo\", \"parameters\" : { nombreArchivo : \"" + nombreArchivo + "\", nombreCarpeta: \"" + nombreCarpeta + "\"} }]";
    CargarAbrirArchivo('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', DatosA, "tableSelectArchivos");

});
// FUNCIONA BIEN PERO LA SIGUIENTE VERSION AYUDA A DESCARGAR EL ARCHIVO
function CargarAbrirArchivo(div, url, datos, tipoControl, boton, idSeleccionado) {

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
                var mensaje = "";
                if (respuesta.estado == "1") {
                    MensajeCorrecto("El archivo se ha descargado con exito.");

                    $("#divMensajes").html("");
                    window.open(respuesta.resultado);
                }
                else if (respuesta.estado == "0") {
                    MensajeIncorrecto("Ha ocurrido un error al descargar el archivo: ");
                }
            },
            error: function (objeto, msgError, objError) {
                var mesnajeError = "La acción de está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
                MensajeIncorrecto(mesnajeError);
            }
        });
    }
}



/* =========================================================
 *              Eliminar archivo de la carpeta
 * =========================================================*/
// Evento para eliminar archivo
$(document).on('click', '.btn-eliminarArchivo, .btn-restaurarArchivo', function () {
    var data = table2.row($(this).closest('tr')).data();
    var nombreArchivo = data.Nombre; 
    var nombreCarpeta = $('#txtConPedido').val();

    var DatosA = JSON.stringify([{
        "action": "EliminarArchivo",
        "parameters": { "nombreArchivo": nombreArchivo, "nombreCarpeta": nombreCarpeta }
    }]);

    $.ajax({
        type: "POST",
        url: "ObtenerNuevaListaTareas.ashx",
        data: DatosA,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) {
            if (respuesta.estado == "1") {
                MensajeCorrecto(respuesta.mensaje);
                VerArchivos(); // Refrescar la lista de archivos
            } else {
                MensajeIncorrecto("Error al procesar la acción.");
            }
        },
        error: function () {
            MensajeIncorrecto("Error de comunicación con el servidor.");
        }
    });
});


/* =========================================================
 *                      Enviar
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

function limpiarFormulario() {
    let contenedor = document.getElementById("FormularioPrincipal");  // Seleccionar el div por su ID
    let inputs = contenedor.querySelectorAll("input, select, textarea");  // Seleccionar los elementos dentro del div
    inputs.forEach(input => {
        if (input.type === "checkbox" || input.type === "radio") {
            input.checked = false;  // Limpiar checkboxes y radios
        } else {
            input.value = "";  // Limpiar texto y valores
        }
    });
    let tablas = document.querySelectorAll("table");  // Seleccionar todas las tablas
    tablas.forEach(tabla => {
        let filas = tabla.querySelectorAll("tbody tr");  // Seleccionar las filas de la tabla
        filas.forEach(fila => {
            fila.remove();  // Eliminar cada fila
        });
    });
    let parrafos = document.querySelectorAll("p");  // Seleccionar todas las etiquetas <p>
    parrafos.forEach(p => {
        p.textContent = "";  // Limpiar el contenido de cada <p>
    });

}

function limpiarPDF() {
    let contenedor = document.getElementById("informacionPDF");  // Seleccionar el div por su ID
    let inputs = contenedor.querySelectorAll("input, select, textarea");  // Seleccionar los elementos dentro del div
    inputs.forEach(input => {
        if (input.type === "checkbox" || input.type === "radio") {
            input.checked = false;  // Limpiar checkboxes y radios
        } else {
            input.value = "";  // Limpiar texto y valores
        }
    });

    // Limpiar todas las tablas
    document.querySelectorAll('table').forEach(table => {
        let tbody = table.querySelector('tbody');  // Seleccionamos el cuerpo de la tabla
        if (tbody) {
            tbody.innerHTML = "";  // Limpiamos todas las filas de la tabla
        }
    });

    // Limpiar spans con ID que comiencen con "txtPDF"
    let spans = document.querySelectorAll('span');
    spans.forEach(span => {
        if (span.id.startsWith("txtPDF")) {  // Verificamos que el span tiene el prefijo "txtPDF"
            span.textContent = "-- -- --";  // Valor predeterminado
        }
    });

    // Limpiar todos los párrafos
    let parrafos = document.querySelectorAll("p");  // Seleccionar todas las etiquetas <p>
    parrafos.forEach(p => {
        p.textContent = "";  // Limpiar el contenido de cada <p>
    });

    // (Opcional) Limpiar eventos asociados a las tablas - solo si es necesario
    // Si las tablas ya tienen eventos asociados, no es necesario agregar estos listeners repetidamente
    // Por lo tanto, puedes comentar o eliminar esta parte si no la necesitas
    /* 
    document.getElementById("tbl_pdfCosteo").addEventListener("click", limpiarTablas);
    document.getElementById("tbl_pdfHardware").addEventListener("click", limpiarTablas);
    document.getElementById("tbl_pdfLicencias").addEventListener("click", limpiarTablas);
    document.getElementById("tbl_pdfServExt").addEventListener("click", limpiarTablas);
    document.getElementById("tbl_pdfServiciosFab").addEventListener("click", limpiarTablas);
    document.getElementById("tbl_pdfPolizas").addEventListener("click", limpiarTablas);
    document.getElementById("tbl_FormasPago").addEventListener("click", limpiarTablas);
    */
}


window.onload = function () {
    setTimeout(function () {
        window.scrollTo(0, 0);
    }, 100);
};






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


// OTRO CODIGO
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
    //if (buttonId === 'siHardware') {
    //    otherButtonId = 'noHardware';
    //    txtHardware = "SI";
    //} else if (buttonId === 'noHardware') {
    //    otherButtonId = 'siHardware';
    //    txtHardware = "NO";
    //} else if (buttonId === 'siServiciosDOS') {
    //    otherButtonId = 'noServiciosDOS';
    //    txtServiciosDOS = "SI";
    //} else if (buttonId === 'noServiciosDOS') {
    //    otherButtonId = 'siServiciosDOS';
    //    txtServiciosDOS = "NO";
    //} else if (buttonId === 'siServiciosExt') {
    //    otherButtonId = 'noServiciosExt';
    //    txtServiciosExt = "SI";
    //} else if (buttonId === 'noServiciosExt') {
    //    otherButtonId = 'siServiciosExt';
    //    txtServiciosExt = "NO";
    //} else if (buttonId === 'siPolizas') {
    //    otherButtonId = 'noPolizas';
    //    txtPolizas = "SI";
    //} else if (buttonId === 'noPolizas') {
    //    otherButtonId = 'siPolizas';
    //    txtPolizas = "NO";
    //} else if (buttonId === 'siTDR') {
    //    otherButtonId = 'noTDR';
    //    txtTDR = "SI";
    //} else if (buttonId === 'noTDR') {
    //    otherButtonId = 'siTDR';
    //    txtTDR = "NO";
    //} else if (buttonId === 'siPreguntas') {
    //    otherButtonId = 'noPreguntas';
    //    txtPreguntas = "SI";
    //} else if (buttonId === 'noPreguntas') {
    //    otherButtonId = 'siPreguntas';
    //    txtPreguntas = "NO";
    //} else if (buttonId === 'siAdj') {
    //    otherButtonId = 'noAdj';
    //    txtActAdj = "SI";
    //} else if (buttonId === 'noAdj') {
    //    otherButtonId = 'siAdj';
    //    txtActAdj = "NO";
    //} else if (buttonId === 'siNeg') {
    //    otherButtonId = 'noNeg';
    //    txtActNeg = "SI";
    //} else if (buttonId === 'noNeg') {
    //    otherButtonId = 'siNeg';
    //    txtActNeg = "NO";
    //} else if (buttonId === 'siGarantiasFIN') {
    //    otherButtonId = 'noGarantiasFIN';
    //    txtGarantiasFIN = "SI";
    //} else if (buttonId === 'noGarantiasFIN') {
    //    otherButtonId = 'siGarantiasFIN';
    //    txtGarantiasFIN = "NO";
    //} else if (buttonId === 'siGarantiasTEC') {
    //    otherButtonId = 'noGarantiasTEC';
    //    txtGarantiasTEC = "SI";
    //} else if (buttonId === 'noGarantiasTEC') {
    //    otherButtonId = 'siGarantiasTEC';
    //    txtGarantiasTEC = "NO";
    //} else if (buttonId === 'siLicenciaTemporales') {
    //    otherButtonId = 'noLicenciaTemporales';
    //    txtLicenciaTemporales = "SI";
    //} else if (buttonId === 'noLicenciaTemporales') {
    //    otherButtonId = 'siLicenciaTemporales';
    //    txtLicenciaTemporales = "NO";
    //}

    // Obtiene el elemento del otro botón
    //var otherButton = document.getElementById(otherButtonId);

    //// Verifica si el otro botón existe
    //if (otherButton && otherButton.classList.contains("active")) {
    //    otherButton.classList.remove("active");
    //    otherButton.style.backgroundColor = ""; // Restablecer el color de fondo
    //}

    //// Si el botón actual no tiene la clase "active", la agrega y cambia el color de fondo y texto
    //if (!button.classList.contains("active")) {
    //    button.classList.add("active");
    //    button.style.backgroundColor = "#7E38D8"; // Cambiar el color de fondo
    //    button.style.color = "#FFFFFF"; // Cambiar el color del texto
    //}

    /* Funcion para bloquear los textarea y botones si se ha seleccionado "No" */
    // Agregar esta parte para deshabilitar los textarea y botones correspondientesn /-------------------------------  revisar
    //if (buttonId.endsWith('No')) {
    //    switch (buttonId) {
    //        case 'noHardware':
    //            document.getElementById('txtHardwareObs').disabled = true;
    //            break;
    //        case 'noServiciosDOS':
    //            document.getElementById('txtServiciosDOSObs').disabled = true;
    //            document.getElementById('btnServDOS').disabled = true;
    //            break;
    //        case 'noServiciosExt':
    //            document.getElementById('txtServiciosExternosObs').disabled = true;
    //            document.getElementById('btnServExternos').disabled = true;
    //            break;
    //        case 'noTDR':
    //            document.getElementById('txtTerminosObs').disabled = true;
    //            document.getElementById('btnTerminosTDR').disabled = true;
    //            break;
    //        case 'noPreguntas':
    //            document.getElementById('txtActaPreguntasObs').disabled = true;
    //            document.getElementById('btnActaPreguntas').disabled = true;
    //            break;
    //        case 'noAdj':
    //            document.getElementById('txtActaAdjObs').disabled = true;
    //            document.getElementById('btnActaAdjudicacion').disabled = true;
    //            break;
    //        case 'noNeg':
    //            document.getElementById('txtActaNegObs').disabled = true;
    //            document.getElementById('btnActaNegociacion').disabled = true;
    //            break;
    //        case 'noGarantiasFIN':
    //            document.getElementById('txtGarFinObs').disabled = true;
    //            break;
    //        case 'noGarantiasTEC':
    //            document.getElementById('txtGarTECObs').disabled = true;
    //            document.getElementById('btnGarantiasTEC').disabled = true;
    //            break;
    //        default:
    //            // Habilitar todos los textarea y botones
    //            document.querySelectorAll('textarea, button').forEach(element => {
    //                element.disabled = false;
    //            });
    //    }

    //} else {
    //    // Habilitar todos los textarea y botones
    //    document.querySelectorAll('textarea, button').forEach(element => {
    //        element.disabled = false;
    //    });
    //}
}

// Función para inicializar el estado de un botón basándose en su valor
function initializeButtonState(buttonName, value) {
    if (value == null) {
        console.warn(`Valor es nulo o indefinido para el botón ${buttonName}`);
        return;
    }
    if (typeof value !== 'string') {
        console.error(`Valor inválido: "${value}". Se esperaba una cadena.`);
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


//  Funcion para cambiar el tamaño del carrusel
function expandirCarousel(height) {
    const carousel = document.querySelector('.carousel-inner');
    if (carousel) {
        carousel.style.height = height + 'px';
    }
}


/*==============================================================*
 *      Funcion para crear nuevos inputs para los correos       *
 * =============================================================*/
let contadorCorreos = 2; // el primero es el #1

function agregarNuevoInputCorreo() {
    const contenedor = document.getElementById('contenedorCorreos');
    const contadorCorreo = contenedor.querySelectorAll('.correo-item').length + 1;

    const divGrupo = document.createElement('div');
    divGrupo.className = 'input-group col-sm-12 correo-item';
    divGrupo.style.marginTop = '0.5rem';

    const spanLabel = document.createElement('span');
    spanLabel.className = 'input-azulmedio input-group-addon ingresosTitulos';
    spanLabel.style.width = '20%';
    spanLabel.style.textAlign = 'left';
    spanLabel.innerHTML = '<i class="glyphicon glyphicon-envelope"></i> Correo:';

    const input = document.createElement('input');
    input.type = 'text';
    input.className = 'form-control';
    input.name = 'correo[]';
    input.placeholder = 'Correo';
    input.id = 'txtCorreoNotificacion' + contadorCorreo;

    const spanEliminar = document.createElement('span');
    spanEliminar.className = 'input-group-addon';
    spanEliminar.style.cursor = 'pointer';
    spanEliminar.title = 'Eliminar';
    spanEliminar.innerHTML = '<i class="glyphicon glyphicon-trash text-danger"></i>';
    spanEliminar.onclick = function () {
        contenedor.removeChild(divGrupo);
    };

    divGrupo.appendChild(spanLabel);
    divGrupo.appendChild(input);
    divGrupo.appendChild(spanEliminar);

    contenedor.appendChild(divGrupo);
}

function obtenerCorreos() {
    const correos = document.querySelectorAll('input[name="correo[]"]');
    const listaCorreos = [];

    correos.forEach(correo => {
        const valor = correo.value.trim();
        if (valor !== "") {
            listaCorreos.push(valor);
        }
    });

    return listaCorreos.join(';'); // Usa ';' o ',' según cómo los necesites
}






$(function () {
    // Establecer altura inicial del carrusel
    expandirCarousel(150);

    CodUsuario = $("#ContentPlaceHolder1_txtCodUnico").val();
    limpiarFormulario();
    BuscarEstadoNotificacion("", "", 1);

    calcularPorcentaje();
    //mostrarInputOtro();

    //deshabilitarInputs();
    //verificarUsuario();
});
