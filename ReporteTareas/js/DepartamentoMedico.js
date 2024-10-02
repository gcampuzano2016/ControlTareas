﻿//  ***   Mostrar boton Cargar informacion   ***
// Obtener el elemento del botón
var btnBuscar = document.getElementById("btnBuscarDatosPersonales");
var buttons = document.querySelectorAll(".btn-custom"); // Obtener todos los botones
var txtLentes, txtAlergias, txtExam1, txtExam2, txtExam3, txtExam4, metPlanifiacionM, tipoPlanificacionM, vidaSxActiva , txtExam5, txtExam6, metPlanifiacionF, tipoPlanificacionF, txtNumHijosVivosM, txtNumHijosMuertosM = "";
let txtMedicacionHabSelect, txtCualMedicamento1, txtCantdidadMed1, txtCualMedicamento2, txtCantdidadMed2, txtCualMedicamento3, txtCantdidadMed3;
let contadorMedicacion = 0;
let IdPerfil = 0;
var inputActivoNum = null; // Variable global para rastrear el input usado en DIAGNOSTICO


function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

// Cambia el color de las pestañas
function cambiarColorHover(pestaniaId) {
    var pestania1 = document.getElementById(pestaniaId);
    pestania1.style.backgroundColor = "#F4F4F4"; // Nuevo color de fondo al pasar el mouse
    pestania1.style.color = "#07BCC8"; // Nuevo color del texto al pasar el mouse
}
function restaurarColor(pestaniaId) {
    var pestania = document.getElementById(pestaniaId);
    pestania.style.backgroundColor = "#A90505"; // Restaurar color de fondo original
    pestania.style.color = "#EDEDED"; // Restaurar color del texto original
}



//cambios---------------------------------------------------------------------------------------------------------------------
function seleccionarPestania(pestaniaId) {
    var pestanias = document.querySelectorAll('.nav-link');
    pestanias.forEach(function (pestania) {
        pestania.classList.remove('tab-seleccionada');
        // Ejecutas una función específica según la pestaña seleccionada
        switch (pestaniaId) {
            case 'pestania1':
                VerFormDatosPersonales();
                break;
            case 'pestania2':
                VerFormConsulta();
                break;
            case 'pestania3':
                VerFormTrabajo();
                break;
            case 'pestania4':
                VerFormResultados();
                break;
            default:
                break;
        }
    });

    var pestaniaSeleccionada = document.getElementById(pestaniaId);
    pestaniaSeleccionada.classList.add('tab-seleccionada');
}
//cambios---------------------------------------------------------------------------------------------------------------------

/* =================================================================
 *          Mover de pagina a la izquierda o derecha
 * ================================================================*/
// Función para ir a la página anterior
function irAPaginaAnterior(pagina) {
    switch (pagina) {
        case 1:
            // No hacer nada en la primera página
            break;
        case 2:
            seleccionarPestania('pestania1');
            break;
        case 3:
            seleccionarPestania('pestania2');
            break;
        case 4:
            seleccionarPestania('pestania3');
            break;
        default:
            break;
    }
}

// Función para ir a la página siguiente
function irAPaginaSiguiente(pagina) {
    switch (pagina) {
        case 1:
            seleccionarPestania('pestania2');
            break;
        case 2:
            seleccionarPestania('pestania3');
            break;
        case 3:
            seleccionarPestania('pestania4');
            break;
        case 4:
            // No hacer nada en la última página
            break;
        default:
            break;
    }
}





// Obtener todos los enlaces con la clase "nav-link"
const links = document.querySelectorAll('.nav-link');
// Agregar eventos para cambiar el color del enlace cuando el mouse está encima
links.forEach(link => {
    link.addEventListener('mouseover', () => {
        link.style.backgroundColor = '#FF0000'; // Cambiar el color de fondo a rojo
        link.style.color = 'white'; // Cambiar el color del texto a blanco (opcional)
    });

    link.addEventListener('mouseout', () => {
        link.style.backgroundColor = '#B81111'; // Restaurar el color de fondo predeterminado
        link.style.color = 'aliceblue'; // Restaurar el color del texto predeterminado (opcional)
    });
});

// INGRESAR SOLO MAYUSCOLAS EN EL INGRESO DEL NOMBRE
function convertirAMayusculas(input) {
    input.value = input.value.toUpperCase();
}


// ********   Mostrar campos tipo y porcentaje de discpacidad si se selecciona SI  *********
$(document).ready(function () {
    $('input[name="discapacidad"]').change(function () {
        if ($(this).val() === "SI") {
            $('#contenedorDiscapacidad').show();
        } else {
            $('#contenedorDiscapacidad').hide();
            BorrarCajas();
        }
    });
});
// Actualiza el valor mostrado en pantalla de la barra porcentaje de discapacidad
function updateValue(val) {
    document.getElementById('txtPorcentajeDiscapacidad').textContent = val;
    $('#txtPorcentajeDiscapacidad').text(val);
}


//----------------  Activar Pestaña 1 ------------------------
function VerFormDatosPersonales() {
    //IdEmpleado = id;
    document.getElementById("pestaniaDatosPersonales").style.display = "block";
    document.getElementById("pestaniaConsulta").style.display = "none";
    document.getElementById("pestaniaTrabajo").style.display = "none";
    document.getElementById("pestaniaResultados").style.display = "none";
}

//----------------  Activar Pestaña 2 ------------------------
function VerFormConsulta() {
    // función para activar los cuadros para llenar la informacion dependiendo si es Fem o Masc
    var selectedSexo = document.getElementById("txtSexo").value;
    var antecedentesGinecologicosF = document.getElementById("antecedentesGinecologicosF");
    var antecedentesGinecologicosM = document.getElementById("antecedentesGinecologicosM");

    // Mostrar u ocultar cuadros "well" basados en el género
    if (selectedSexo === "FEMENINO") {
        antecedentesGinecologicosF.style.display = "block"; // Mostrar cuadro well femenino
        antecedentesGinecologicosM.style.display = "none";  // Ocultar cuadro well masculino
    } else if (selectedSexo === "MASCULINO") {
        antecedentesGinecologicosF.style.display = "none";  // Ocultar cuadro well femenino
        antecedentesGinecologicosM.style.display = "block"; // Mostrar cuadro well masculino
    }

    document.getElementById("pestaniaDatosPersonales").style.display = "none";
    document.getElementById("pestaniaConsulta").style.display = "block";
    document.getElementById("pestaniaTrabajo").style.display = "none";
    document.getElementById("pestaniaResultados").style.display = "none";
}
//----------------  Activar Pestaña 3 ------------------------
function VerFormTrabajo() {
    document.getElementById("pestaniaDatosPersonales").style.display = "none";
    document.getElementById("pestaniaConsulta").style.display = "none";
    document.getElementById("pestaniaTrabajo").style.display = "block";
    document.getElementById("pestaniaResultados").style.display = "none";
}
//----------------  Activar Pestaña 4 ------------------------
function VerFormResultados() {    
    document.getElementById("pestaniaDatosPersonales").style.display = "none";
    document.getElementById("pestaniaConsulta").style.display = "none";
    document.getElementById("pestaniaTrabajo").style.display = "none";
    document.getElementById("pestaniaResultados").style.display = "block";
}

/* =================================================================
 *          Mover de pagina a la izquierda o derecha
 * ================================================================*/
// Función para ir a la página anterior
function irAPaginaAnterior(pagina) {
    switch (pagina) {
        case 1:            
            break;
        case 2:
            VerFormDatosPersonales();
            break;
        case 3:
            VerFormConsulta();
            break;
        case 4:
            VerFormTrabajo();
            break;
    }
}
// Función para ir a la página siguiente
function irAPaginaSiguiente(pagina) {
    switch (pagina) {
        case 1:
            VerFormConsulta();
            break;
        case 2:
            VerFormTrabajo();
            break;
        case 3:
            VerFormResultados();
            break;
        case 4:
            break;
    }
}


// Obtener la cedula ingresada en la pagina principal HistoriaClinica
document.addEventListener('DOMContentLoaded', function () {
    
    var cedulaEmpleado = $("#ContentPlaceHolder1_hiddenCedulaField").val();
    if (cedulaEmpleado) {
        BuscarEmpleado(cedulaEmpleado); // Ejecuta la función BuscarEmpleado
    } else {
        console.error('CedulaEmpleado está vacío o no se pudo encontrar el campo oculto.');
    }
});



/* =================================================================================
 *                      Cargar datos de la base al Historial
 * ================================================================================*/

// Definir la función para buscar un empleado
function BuscarEmpleado(cedula) {
    //let cedulaBuscar = document.getElementById("txtEmpleado").value;

    let cedulaBuscar = cedula;

    ObtenerListaEmpleados(cedulaBuscar, "", "");
    document.getElementById("btnCarga").style.display = "flex";
}
function ObtenerListaEmpleados(cedula, descripcion, tipo2) {

    var Datos = "[{ \"action\": \"BuscarEmpleadoPorCedula\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" +cedula+ "\"} }]";

    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}
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

    if (tipoControl == "tableSelectBusqueda") {
        contenido = RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectBusquedaCIE") {
        contenido = RecorreJSONTableSelectCodigoCIE(json, boton, idSeleccionado);
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
        let FechaNacimiento = item.Fecha_Nacimiento
        document.getElementById("fechaNac").value = FechaNacimiento;
        let Sociedad = item.Sociedad
        document.getElementById("txtSociedad").value = Sociedad;
        let AreaTrabajo = item.AreaTrabajo
        document.getElementById("txtAreaTrabajo").value = AreaTrabajo; 
        let Edad = calcularEdad(FechaNacimiento);
        document.getElementById("txtEdad").value = Edad;
        let Sexo = item.Sexo;
        document.getElementById("txtSexo").value = Sexo;
        let EstadoCivil = item.EstadoCivil;
        document.getElementById("txtEstadoCivil").value = EstadoCivil;
        let PuestoTrabajo = item.PuestoTrabajo;
        document.getElementById("txtPuestoTrabajo").value = PuestoTrabajo;

        let Telefono = item.Telefono;
        document.getElementById("txtTelefono").value = Telefono;
        let Direccion = item.Direccion;
        document.getElementById("txtDireccion").value = Direccion;
        let Correo = item.Correo;
        document.getElementById("txtCorreo").value = Correo;


        // Llama a la función para cargar la imagen del empleado
        var nombreApellido = Nombre.split(" ");
        if (nombreApellido.length >= 4) {
            var primerApellido  = nombreApellido[0];
            var primerNombre  = nombreApellido[2];
            var segundoNombre = nombreApellido[3];
            cargarImagen(primerNombre, segundoNombre, primerApellido);
        } else {
            console.error("El formato del nombre no es válido");
        }        
    });    
}


/*=================================================================================
 *     Función para cargar la imagen del paciente y manejar si no se encuentra
 *=================================================================================*/
function cargarImagen(primerNombre, segundoNombre, primerApellido) {
    // Crea un elemento <img>
    let divImagen = document.getElementById("imagenDiv");
    var imagen = document.createElement("img");

    // Capitaliza la primera letra de cada nombre y apellido
    primerNombre = primerNombre.charAt(0).toUpperCase() + primerNombre.slice(1).toLowerCase();
    primerApellido = primerApellido.charAt(0).toUpperCase() + primerApellido.slice(1).toLowerCase();

    // Construye la ruta de la imagen utilizando el nombre y apellido
    var rutaImagen = "../carrusel/imagenes/fotos/";

    // Intenta cargar la imagen con primer nombre y primer apellido
    rutaImagen += primerNombre + "_" + primerApellido + ".jpg";

    // Verifica si la imagen existe en la ruta especificada
    try {
        let imagen = new Image();
        imagen.src = rutaImagen;
        imagen.alt = "Descripción de la imagen";

        imagen.onload = function () {
            divImagen.style.backgroundImage = 'url(' + rutaImagen + ')';
        };

        imagen.onerror = function () {
            // Si la imagen con el primer nombre y primer apellido no existe, intenta con el segundo nombre
            var rutaImagenSegundoNombre = "../carrusel/imagenes/fotos/";

            if (segundoNombre) {
                segundoNombre = segundoNombre.charAt(0).toUpperCase() + segundoNombre.slice(1).toLowerCase();
                rutaImagenSegundoNombre += segundoNombre + "_" + primerApellido + ".jpg";

                var imagenSegundoNombre = new Image();
                imagenSegundoNombre.src = rutaImagenSegundoNombre;
                imagenSegundoNombre.alt = "Descripción de la imagen";

                imagenSegundoNombre.onload = function () {
                    // Si la imagen con el segundo nombre y primer apellido existe, cárgala
                    divImagen.style.backgroundImage = 'url(' + rutaImagenSegundoNombre + ')';
                };
                imagenSegundoNombre.onerror = function () {
                    // Si no hay segunda imagen, muestra la imagen por defecto
                    rutaImagenSegundoNombre = "../carrusel/imagenes/fotos/usuarios.png";
                    divImagen.style.backgroundImage = 'url(' + rutaImagenSegundoNombre + ')';
                };
            } else {
                // Si no hay segundo nombre, muestra la imagen por defecto
                rutaImagenSegundoNombre = "../carrusel/imagenes/fotos/usuarios.png";
                divImagen.style.backgroundImage = 'url(' + rutaImagenSegundoNombre + ')';
            }
        };

    } catch (error) {
        // Maneja cualquier excepción que pueda ocurrir
        console.error("Error al cargar la imagen:", error);
    }
}


//  Función para calcular la edad usando la fecha de nacimiento
function calcularEdad(fechaNacimiento) {
    const fechaActual = new Date();
    const partes = fechaNacimiento.split('/');
    const fechaNac = new Date(partes[2], partes[1] - 1, partes[0]); // Restamos 1 al mes porque en JavaScript los meses van de 0 a 11
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
    txtLentes, txtAlergias, txtExam1, txtExam2, txtExam3, txtExam4, txtExam5, txtExam6 = "";

    // Variable para almacenar el ID del otro botón (para alternar entre "Si" y "No")
    var otherButtonId;

    // Comprueba el ID del botón y establece el texto correspondiente
    if (buttonId === 'siLentes') {
        otherButtonId = 'noLentes';
        txtLentes = "SI";
    } else if (buttonId === 'noLentes') {
        otherButtonId = 'siLentes';
        txtLentes = "NO";
    } else if (buttonId === 'siExam1') {
        otherButtonId = 'noExam1';
        txtExam1 = "SI";
    } else if (buttonId === 'noExam1') {
        otherButtonId = 'siExam1';
        txtExam1 = "NO";
    } else if (buttonId === 'siExam2') {
        otherButtonId = 'noExam2';
        txtExam2 = "SI";
    } else if (buttonId === 'noExam2') {
        otherButtonId = 'siExam2';
        txtExam2 = "NO";
    } else if (buttonId === 'siExam3') {
        otherButtonId = 'noExam3';
        txtExam3 = "SI";
    } else if (buttonId === 'noExam3') {
        otherButtonId = 'siExam3';
        txtExam3 = "NO";
    } else if (buttonId === 'siExam4') {
        otherButtonId = 'noExam4';
        txtExam4 = "SI";
    } else if (buttonId === 'noExam4') {
        otherButtonId = 'siExam4';
        txtExam4 = "NO";
    } else if (buttonId === 'siExam5') {
        otherButtonId = 'noExam5';
        txtExam5 = "SI";
    } else if (buttonId === 'noExam5') {
        otherButtonId = 'siExam5';
        txtExam5 = "NO";
    } else if (buttonId === 'siExam6') {
        otherButtonId = 'noExam6';
        txtExam6 = "SI";
    } else if (buttonId === 'noExam6') {
        otherButtonId = 'siExam6';
        txtExam6 = "NO";
    } else if (buttonId === 'siAlergias') {
        otherButtonId = 'noAlergias';
        txtAlergias = "SI";
    } else if (buttonId === 'noAlergias') {
        otherButtonId = 'siAlergias';
        txtAlergias = "NO";
    } else if (buttonId != 'siAlergias' && buttonId != 'noAlergias') {
        txtAlergias = "";
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


// ********   Mostrar campos TIPO de planificación al seleccionar SI  *********
$(document).ready(function () {
    // Metodo de planificación femenino
    $('input[name="metodoplanif1"]').change(function () {
        if ($(this).val() === "SI") {
            $('#contenedorMetodo1').show();
        } else {
            $('#contenedorMetodo1').hide();
            BorrarCajas();
        }
    });

    // Metodo de planificación Masculino
    $('input[name="metodoplanif2"]').change(function () {
        if ($(this).val() === "SI") {
            $('#contenedorMetodo2').show();
        } else {
            $('#contenedorMetodo2').hide();
            // Realiza otras acciones si es necesario
        }
    });

});


//   ******   Mostrar la fecha actual  ******
/*function mostrarFechaActual() {
    var fechaActual = new Date();
    var opciones = { year: 'numeric', month: 'long', day: 'numeric' };
    var fechaFormateada = fechaActual.toLocaleDateString(undefined, opciones);
    var elementoFecha = document.getElementById("<%= fechaActual.ClientID %>");
    if (elementoFecha) {
        elementoFecha.innerHTML = fechaFormateada;
    }
}
// Llama a la función para mostrar la fecha actual al cargar la página
window.onload = mostrarFechaActual;*/

//function mostrarFechaActual() {
//    const elementoFecha = document.getElementById("txtFechaActual");

//    if (elementoFecha) {
//        const fecha = new Date();
//        const fechaActual = fecha.toLocaleDateString('en-GB'); // Formato DD/MM/YYYY
//        elementoFecha.textContent = fechaActual;
//    } else {
//        console.error("El elemento con ID 'txtFechaActual' no fue encontrado.");
//    }
//}

//document.addEventListener("DOMContentLoaded", function () {
//    // Llamada inicial para mostrar la fecha actual al cargar la página
//    mostrarFechaActual();

//    // Puedes llamar a mostrarFechaActual() en cualquier otro lugar donde necesites mostrar la fecha actual, por ejemplo, al hacer clic en un botón.
//});


/*=================================================================================
 *                Función para agregar una nueva medicación
 *=================================================================================*/
function agregarMedicacion() {
    var container = document.getElementById("medicacionContainer");
    var newMedicacion = container.cloneNode(true);

    // Modificar IDs de los campos de la nueva medicación
    var newId = contadorMedicacion + 2; // Generar un nuevo ID
    newMedicacion.querySelector("#MedicacionHabSelect").id = "MedicacionHabSelect" + newId;
    newMedicacion.querySelector("#txtCualMedicamento1").id = "txtCualMedicamento" + newId;
    newMedicacion.querySelector("#txtCantdidadMed1").id = "txtCantdidadMed" + newId;

    // Restablecer valores de los campos
    var txtMedicacionHabSelect = newMedicacion.querySelector("#MedicacionHabSelect" + newId);
    var txtCualMedicamento = newMedicacion.querySelector("#txtCualMedicamento" + newId);
    var txtCantdidadMed = newMedicacion.querySelector("#txtCantdidadMed" + newId);

    txtMedicacionHabSelect.disabled = true;
    txtCualMedicamento.disabled = false;
    txtCantdidadMed.disabled = false;

    // Agregar la nueva medicación arriba del botón
    container.parentNode.append(newMedicacion, container.nextSibling);

    contadorMedicacion++;

    if (contadorMedicacion >= 2) {
        // Deshabilitar el botón después de crear dos filas
        document.getElementById("botonAgregarMedicacion").disabled = true;
    }
}


/*===================================================================================================
 *  Funciones para que los campos sean editables en Habitos Toxicos si la opcion seleccionada es SI
 *===================================================================================================*/
function handleTabacoSelect() {
    var tabacoSelect = document.getElementById("tabacoSelect");
    var txtTiempoconsumo = document.getElementById("txtTiempoconsumoTabaco");
    var txtCantidad = document.getElementById("txtCantidadTabaco");
    var exConsumidoraSelectTabaco = document.getElementById("exConsumidoraSelectTabaco");
    var txtTiempoAbstinenciaTabaco = document.getElementById("txtTiempoAbstinenciaTabaco");

    if (tabacoSelect.value === "si") {
        txtTiempoconsumo.disabled = false;
        txtCantidad.disabled = false;
        exConsumidoraSelectTabaco.disabled = false;
        txtTiempoAbstinenciaTabaco.disabled = false;
    } else {
        txtTiempoconsumo.disabled = true;
        txtCantidad.disabled = true;
        exConsumidoraSelectTabaco.disabled = true;
        txtTiempoAbstinenciaTabaco.disabled = true;

        // Si se selecciona "No", restablecer los valores a su estado inicial
        txtTiempoconsumo.value = "";
        txtCantidad.value = "";
        exConsumidoraSelectTabaco.value = "no";
        txtTiempoAbstinenciaTabaco.value = "";
    }
}
function handleAlcoholSelect() {
    var alcoholSelect = document.getElementById("alcoholSelect");
    var txtTiempoconsumoAlcohol = document.getElementById("txtTiempoconsumoAlcohol");
    var txtCantidadAlcohol = document.getElementById("txtCantidadAlcohol");
    var exConsumidoraSelectAlcohol = document.getElementById("exConsumidoraSelectAlcohol");
    var txtTiempoAbstinenciaAlcohol = document.getElementById("txtTiempoAbstinenciaAlcohol");

    if (alcoholSelect.value === "si") {
        txtTiempoconsumoAlcohol.disabled = false;
        txtCantidadAlcohol.disabled = false;
        exConsumidoraSelectAlcohol.disabled = false;
        txtTiempoAbstinenciaAlcohol.disabled = false;
    } else {
        txtTiempoconsumoAlcohol.disabled = true;
        txtCantidadAlcohol.disabled = true;
        exConsumidoraSelectAlcohol.disabled = true;
        txtTiempoAbstinenciaAlcohol.disabled = true;

        txtTiempoconsumoAlcohol.value = "";
        txtCantidadAlcohol.value = "";
        exConsumidoraSelectAlcohol.value = "no";
        txtTiempoAbstinenciaAlcohol.value = "";
    }
}
function handleOtraSelect() {
    var otraSelect = document.getElementById("otraSelect");
    var txtTiempoconsumoOtra = document.getElementById("txtTiempoconsumoOtra");
    var txtCantidadOtra = document.getElementById("txtCantidadOtra");
    var exConsumidorSelectOtra = document.getElementById("exConsumidorSelectOtra");
    var txtTiempoAbstinenciaOtra = document.getElementById("txtTiempoAbstinenciaOtra");

    if (otraSelect.value === "si") {
        txtTiempoconsumoOtra.disabled = false;
        txtCantidadOtra.disabled = false;
        exConsumidorSelectOtra.disabled = false;
        txtTiempoAbstinenciaOtra.disabled = false;
    } else {
        txtTiempoconsumoOtra.disabled = true;
        txtCantidadOtra.disabled = true;
        exConsumidorSelectOtra.disabled = true;
        txtTiempoAbstinenciaOtra.disabled = true;

        txtTiempoconsumoOtra.value = "";
        txtCantidadOtra.value = "";
        exConsumidorSelectOtra.value = "no";
        txtTiempoAbstinenciaOtra.value = "";
    }
}


//funcion para que los campos sean editables en Estilo de vida Actividad física si la opcion seleccionada es SI
function handleActividadFisicaSelect() {
    var txtActividadFisiscaSelect = document.getElementById("txtActividadFisiscaSelect");
    var txtCualActividad = document.getElementById("txtCualActividad");
    var txtFrecuenciaActividad = document.getElementById("txtFrecuenciaActividad");

    if (txtActividadFisiscaSelect.value === "si") {
        txtCualActividad.disabled = false;
        txtFrecuenciaActividad.disabled = false;
    } else {
        txtCualActividad.disabled = true;
        txtFrecuenciaActividad.disabled = true;

        txtCualActividad.value = "";
        txtFrecuenciaActividad.value = "";
    }
}

//funcion para que los campos sean editables en Estilo de vida Medicación habitual si la opcion seleccionada es SI
function handleMedicacionSelect() {
    var txtMedicacionHabSelect = document.getElementById("MedicacionHabSelect");
    var txtCualMedicamento1 = document.getElementById("txtCualMedicamento1");
    var txtCantidadMed1 = document.getElementById("txtCantdidadMed1");
    var botonAgregarMedicacion = document.getElementById("botonAgregarMedicacion");

    if (txtMedicacionHabSelect.value === "si") {
        txtCualMedicamento1.disabled = false;
        txtCantidadMed1.disabled = false;
        botonAgregarMedicacion.disabled = false;
    } else {
        txtCualMedicamento1.disabled = true;
        txtCantidadMed1.disabled = true;
        botonAgregarMedicacion.disabled = true;

        txtCualMedicamento1.value = "";
        txtCantidadMed1.value = "";
    }
}

//Funcion para activar los accidentes de trabajo si se selecciona SI
function handleAccidentesTrabajoSelect() {
    var AccidentesTrabajoSelect = document.getElementById("AccidentesTrabajoSelect");
    var txtEspecificarAccidentesTrabajoSelect = document.getElementById("txtEspecificarAccidentesTrabajoSelect");
    var txtObservacionesAccTrabajo = document.getElementById("txtObservacionesAccTrabajo");    

    if (AccidentesTrabajoSelect.value === "si") {
        txtEspecificarAccidentesTrabajoSelect.disabled = false;
        txtObservacionesAccTrabajo.disabled = false;
    } else {
        txtEspecificarAccidentesTrabajoSelect.disabled = true;
        txtObservacionesAccTrabajo.disabled = true;

        txtEspecificarAccidentesTrabajoSelect.value = "";
        txtObservacionesAccTrabajo.value = "";
    }
}

//Funcion para activar Enfermedades profesionales si se selecciona SI
function handleEfermedadesProfSelect() {
    var EnfermedadesProfSelect = document.getElementById("EnfermedadesProfSelect");
    var txtEspecificarEnfermedadesProfSelect = document.getElementById("txtEspecificarEnfermedadesProfSelect");
    var txtObservacionesEnfermedadesProf = document.getElementById("txtObservacionesEnfermedadesProf");

    if (EnfermedadesProfSelect.value === "si") {
        txtEspecificarEnfermedadesProfSelect.disabled = false;
        txtObservacionesEnfermedadesProf.disabled = false;
    } else {
        txtEspecificarEnfermedadesProfSelect.disabled = true;
        txtObservacionesEnfermedadesProf.disabled = true;
        txtEspecificarEnfermedadesProfSelect.value = "";
        txtObservacionesEnfermedadesProf.value = "";
    }
}

// Manejo de opciones para FACTORES DE RIESGOS DEL PUESTO DE TRABAJO ACTUAL
document.addEventListener("DOMContentLoaded", function () {
    // Obtén referencias a los elementos relevantes
    var fisicoSelect1 = document.getElementById("txtFisicoSelect1");
    var mecanicoSelect1 = document.getElementById("txtMecanicoSelect1");
    var otrosDiv1 = document.getElementById("otros1");
    var otrofisico1 = document.getElementById("txtOtrosFisico1");
    var otromecanico1 = document.getElementById("txOtrosMecanico1");

    var fisicoSelect2 = document.getElementById("txtFisicoSelect2");
    var mecanicoSelect2 = document.getElementById("txtMecanicoSelect2");
    var otrosDiv2 = document.getElementById("otros2");
    var otrofisico2 = document.getElementById("txtOtrosFisico2");
    var otromecanico2 = document.getElementById("txOtrosMecanico2");

    var fisicoSelect3= document.getElementById("txtFisicoSelect3");
    var mecanicoSelect3 = document.getElementById("txtMecanicoSelect3");
    var otrosDiv3 = document.getElementById("otros3");
    var otrofisico3 = document.getElementById("txtOtrosFisico3");
    var otromecanico3 = document.getElementById("txOtrosMecanico3");    

    // Agrega controladores de eventos change a los campos select
    fisicoSelect1.addEventListener("change", toggleOtrosDiv);
    mecanicoSelect1.addEventListener("change", toggleOtrosDiv);
    fisicoSelect2.addEventListener("change", toggleOtrosDiv2);
    mecanicoSelect2.addEventListener("change", toggleOtrosDiv2);
    fisicoSelect3.addEventListener("change", toggleOtrosDiv3);
    mecanicoSelect3.addEventListener("change", toggleOtrosDiv3);

    // Función para mostrar u ocultar el div "otros1" según la selección
    function toggleOtrosDiv() {
        if (fisicoSelect1.value === "OtrosFisico" || mecanicoSelect1.value === "OtrosMecanico") {
            otrosDiv1.style.display = "flex"; // Mostrar el div
            if (fisicoSelect1.value === "OtrosFisico" && mecanicoSelect1.value != "OtrosMecanico") {
                otromecanico1.disabled = true;
            } else if (mecanicoSelect1.value === "OtrosMecanico" && fisicoSelect1.value != "OtrosFisico") {
                otrofisico1.disabled = true;
            } else if (fisicoSelect1.value === "OtrosFisico" && mecanicoSelect1.value === "OtrosMecanico") {
                otrofisico1.disabled = false;
                otromecanico1.disabled = false;
            }
        } else {
            otrosDiv1.style.display = "none";  // Ocultar el div
        }
    }
    function toggleOtrosDiv2() {

        if (fisicoSelect2.value === "OtrosFisico" || mecanicoSelect2.value === "OtrosMecanico") {
            otrosDiv2.style.display = "flex"; // Mostrar el div
            if (fisicoSelect2.value === "OtrosFisico" && mecanicoSelect2.value != "OtrosMecanico") {
                otromecanico2.disabled = true;
            } else if (mecanicoSelect2.value === "OtrosMecanico" && fisicoSelect2.value != "OtrosFisico") {
                otrofisico2.disabled = true;
            } else if (fisicoSelect2.value === "OtrosFisico" && mecanicoSelect2.value === "OtrosMecanico") {
                otrofisico2.disabled = false;
                otromecanico2.disabled = false;
            }
        } else {
            otrosDiv2.style.display = "none";  // Ocultar el div
        }
    }
    function toggleOtrosDiv3() {

        if (fisicoSelect3.value === "OtrosFisico" || mecanicoSelect3.value === "OtrosMecanico") {
            otrosDiv3.style.display = "flex"; // Mostrar el div
            if (fisicoSelect3.value === "OtrosFisico" && mecanicoSelect3.value != "OtrosMecanico") {
                otromecanico3.disabled = true;
            } else if (mecanicoSelect3.value === "OtrosMecanico" && fisicoSelect3.value != "OtrosFisico") {
                otrofisico3.disabled = true;
            } else if (fisicoSelect3.value === "OtrosFisico" && mecanicoSelect3.value === "OtrosMecanico") {
                otrofisico3.disabled = false;
                otromecanico3.disabled = false;
            }
        } else {
            otrosDiv3.style.display = "none";  // Ocultar el div
        }
    }

    // Llama a la función inicialmente para manejar el estado inicial
    toggleOtrosDiv();
});

document.addEventListener("DOMContentLoaded", function () {
    // Obtén referencias a los elementos relevantes
    var quimicoSelect1 = document.getElementById("txtQuimicoSelect1");
    var biologicoSelect1 = document.getElementById("txtBiologicoSelect1");
    var ergonomicoSelect1 = document.getElementById("txtErgonomicoSelect1");
    var psicosocialSelect1 = document.getElementById("txtPSicosocialSelect1");
    var otrosDiv4 = document.getElementById("otros4");

    // Obtén referencias a los campos input relacionados con "Otros"
    var otroquimico1 = document.getElementById("txtOtrosQuimico1");
    var otrobiologico1 = document.getElementById("txOtrosBiologico1");
    var otroergonomico1 = document.getElementById("txOtrosErgonomico1");
    var otropsicosocial1 = document.getElementById("txOtrosPSicosocial1");

    // Agrega controladores de eventos change a los campos select
    quimicoSelect1.addEventListener("change", toggleOtrosDiv4);
    biologicoSelect1.addEventListener("change", toggleOtrosDiv4);
    ergonomicoSelect1.addEventListener("change", toggleOtrosDiv4);
    psicosocialSelect1.addEventListener("change", toggleOtrosDiv4);

    function toggleOtrosDiv4() {
        // Verificar si al menos una de las opciones "Otros" está seleccionada
        var isAtLeastOneOtrosSelected =
            quimicoSelect1.value === "OtrosQuimico" ||
            biologicoSelect1.value === "OtrosBiologico" ||
            ergonomicoSelect1.value === "OtrosErgonomico" ||
            psicosocialSelect1.value === "OtrosPSicosocial";

        // Habilitar o deshabilitar el div "otros4" según la selección
        otrosDiv4.style.display = isAtLeastOneOtrosSelected ? "flex" : "none";

        // Habilitar o deshabilitar los campos input según la selección
        otroquimico1.disabled = !isAtLeastOneOtrosSelected || quimicoSelect1.value !== "OtrosQuimico";
        otrobiologico1.disabled = !isAtLeastOneOtrosSelected || biologicoSelect1.value !== "OtrosBiologico";
        otroergonomico1.disabled = !isAtLeastOneOtrosSelected || ergonomicoSelect1.value !== "OtrosErgonomico";
        otropsicosocial1.disabled = !isAtLeastOneOtrosSelected || psicosocialSelect1.value !== "OtrosPSicosocial";
    }

    // Llama a la función inicialmente para manejar el estado inicial
    toggleOtrosDiv4();
});
document.addEventListener("DOMContentLoaded", function () {
    // Obtén referencias a los elementos relevantes del segundo conjunto
    var quimicoSelect2 = document.getElementById("txtQuimicoSelect2");
    var biologicoSelect2 = document.getElementById("txtBiologicoSelect2");
    var ergonomicoSelect2 = document.getElementById("txtErgonomicoSelect2");
    var psicosocialSelect2 = document.getElementById("txtPSicosocialSelect2");
    var otrosDiv5 = document.getElementById("otros5");

    // Obtén referencias a los campos input relacionados con "Otros" del segundo conjunto
    var otroquimico2 = document.getElementById("txtOtrosQuimico2");
    var otrobiologico2 = document.getElementById("txOtrosBiologico2");
    var otroergonomico2 = document.getElementById("txOtrosErgonomico2");
    var otropsicosocial2 = document.getElementById("txOtrosPSicosocial2");

    // Agrega controladores de eventos change al segundo conjunto de campos select
    quimicoSelect2.addEventListener("change", toggleOtrosDiv5);
    biologicoSelect2.addEventListener("change", toggleOtrosDiv5);
    ergonomicoSelect2.addEventListener("change", toggleOtrosDiv5);
    psicosocialSelect2.addEventListener("change", toggleOtrosDiv5);

    function toggleOtrosDiv5() {
        // Verificar si al menos una de las opciones "Otros" está seleccionada en el segundo conjunto
        var isAtLeastOneOtrosSelected =
            quimicoSelect2.value === "OtrosQuimico" ||
            biologicoSelect2.value === "OtrosBiologico" ||
            ergonomicoSelect2.value === "OtrosErgonomico" ||
            psicosocialSelect2.value === "OtrosPSicosocial";

        // Habilitar o deshabilitar el div "otros5" del segundo conjunto según la selección
        otrosDiv5.style.display = isAtLeastOneOtrosSelected ? "flex" : "none";

        // Habilitar o deshabilitar los campos input del segundo conjunto según la selección
        otroquimico2.disabled = !isAtLeastOneOtrosSelected || quimicoSelect2.value !== "OtrosQuimico";
        otrobiologico2.disabled = !isAtLeastOneOtrosSelected || biologicoSelect2.value !== "OtrosBiologico";
        otroergonomico2.disabled = !isAtLeastOneOtrosSelected || ergonomicoSelect2.value !== "OtrosErgonomico";
        otropsicosocial2.disabled = !isAtLeastOneOtrosSelected || psicosocialSelect2.value !== "OtrosPSicosocial";
    }

    // Llama a la función inicialmente para manejar el estado inicial del segundo conjunto
    toggleOtrosDiv5();
});
document.addEventListener("DOMContentLoaded", function () {
    // Obtén referencias a los elementos relevantes del tercer conjunto
    var quimicoSelect3 = document.getElementById("txtQuimicoSelect3");
    var biologicoSelect3 = document.getElementById("txtBiologicoSelect3");
    var ergonomicoSelect3 = document.getElementById("txtErgonomicoSelect3");
    var psicosocialSelect3 = document.getElementById("txtPSicosocialSelect3");
    var otrosDiv6 = document.getElementById("otros6");

    // Obtén referencias a los campos input relacionados con "Otros" del tercer conjunto
    var otroquimico3 = document.getElementById("txtOtrosQuimico3");
    var otrobiologico3 = document.getElementById("txOtrosBiologico3");
    var otroergonomico3 = document.getElementById("txOtrosErgonomico3");
    var otropsicosocial3 = document.getElementById("txOtrosPSicosocial3");

    // Agrega controladores de eventos change al tercer conjunto de campos select
    quimicoSelect3.addEventListener("change", toggleOtrosDiv6);
    biologicoSelect3.addEventListener("change", toggleOtrosDiv6);
    ergonomicoSelect3.addEventListener("change", toggleOtrosDiv6);
    psicosocialSelect3.addEventListener("change", toggleOtrosDiv6);

    function toggleOtrosDiv6() {
        // Verificar si al menos una de las opciones "Otros" está seleccionada en el tercer conjunto
        var isAtLeastOneOtrosSelected =
            quimicoSelect3.value === "OtrosQuimico" ||
            biologicoSelect3.value === "OtrosBiologico" ||
            ergonomicoSelect3.value === "OtrosErgonomico" ||
            psicosocialSelect3.value === "OtrosPSicosocial";

        // Habilitar o deshabilitar el div "otros6" del tercer conjunto según la selección
        otrosDiv6.style.display = isAtLeastOneOtrosSelected ? "flex" : "none";

        // Habilitar o deshabilitar los campos input del tercer conjunto según la selección
        otroquimico3.disabled = !isAtLeastOneOtrosSelected || quimicoSelect3.value !== "OtrosQuimico";
        otrobiologico3.disabled = !isAtLeastOneOtrosSelected || biologicoSelect3.value !== "OtrosBiologico";
        otroergonomico3.disabled = !isAtLeastOneOtrosSelected || ergonomicoSelect3.value !== "OtrosErgonomico";
        otropsicosocial3.disabled = !isAtLeastOneOtrosSelected || psicosocialSelect3.value !== "OtrosPSicosocial";
    }

    // Llama a la función inicialmente para manejar el estado inicial del tercer conjunto
    toggleOtrosDiv6();
});


/*=====================================================================
 *          Buscamos los codigos CIE en la base de datos
 *====================================================================*/

function BuscarCodigosCIE() {
    let txtDiagDescripcion = document.getElementById(`txtDiagDescripcion${inputActivoNum}`);
    if (txtDiagDescripcion.value.length > 3) {
        let CIEBuscar = txtDiagDescripcion.value;
        idSeleccionado = 0;
        ObtenerListaCodigosCIE(CIEBuscar, "", "", inputActivoNum);
    } else {
        document.getElementById(`comboCodigos${inputActivoNum}`).style.display = "none";
    }
}

function ObtenerListaCodigosCIE(codigo, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarCodigoCIE\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" + codigo + "\"} }]";
    CargarPagina(`#comboCodigos${inputActivoNum}`, 'ObtenerListaTareas.ashx', Datos, "tableSelectBusquedaCIE", idproceso);
}

function CargarCodigoCIE(CODIGO, DESCRIPCION) {
    // Mostrar el código seguido por la descripción
    document.getElementById(`txtDiagDescripcion${inputActivoNum}`).value = `${DESCRIPCION}`;

    // Colocar el código en el campo CIE y desactivar la edición
    document.getElementById(`txtDiagCIE${inputActivoNum}`).value = CODIGO;
    document.getElementById(`txtDiagCIE${inputActivoNum}`).disabled = true;

    // Ocultar la lista desplegable
    document.getElementById(`comboCodigos${inputActivoNum}`).style.display = "none";
}

function RecorreJSONTableSelectCodigoCIE(json, boton, idSeleccionado) {
    var x = "";
    $.each(json, function (i, item) {
        x = x + `<li><a role='option' onclick='CargarCodigoCIE("${item.Codigo}","${item.DescCodigo}");'>${item.Codigo} - ${item.DescCodigo}</a></li>`;
    });
    document.getElementById(`comboCodigos${inputActivoNum}`).innerHTML = x;
    document.getElementById(`comboCodigos${inputActivoNum}`).style.display = "block";
}
function setInputActivo(num) {
    inputActivoNum = num;
}



/* ====================================================================
 *      Controlamos las opciones de APTITUD MEDICA PARA EL TRABAJO
 * ====================================================================*/
document.addEventListener('DOMContentLoaded', function () {
    // Obtener todos los contenedores de menús desplegables
    const dropdownContainers = document.querySelectorAll('.dropdown-container');

    // Agregar la funcionalidad a cada contenedor
    dropdownContainers.forEach(function (container) {
        const dropdownButton = container.querySelector('.dropdown-button');
        const dropdownContent = container.querySelector('.dropdown-content');

        dropdownButton.addEventListener('click', function (event) {
            event.stopPropagation(); // Evita que el clic se propague y cierre inmediatamente
            dropdownContent.style.display = dropdownContent.style.display === 'block' ? 'none' : 'block';
        });

        // Cierra el menú desplegable solo si se hace clic fuera del menú y no en las casillas de verificación
        document.addEventListener('click', function (event) {
            if (!dropdownContent.contains(event.target) && event.target !== dropdownButton) {
                dropdownContent.style.display = 'none';
            }
        });
    });
});

document.addEventListener("DOMContentLoaded", function () {
    // Obtén referencias a los elementos relevantes
    var aptitudSelect = document.getElementById("txtAptitudSelect");
    var descObservacion = document.getElementById("descObservacion");
    var descLimitacion = document.getElementById("descLimitacion");
    var txtDescObservacion = document.getElementById("txtDescObservacion");
    var txtDescLimitacion = document.getElementById("txtDescLimitacion");

    // Agrega un controlador de eventos change al campo select
    aptitudSelect.addEventListener("change", toggleAptitudDiv);

    // Función para mostrar u ocultar los campos según la selección
    function toggleAptitudDiv() {
        if (aptitudSelect.value === "aptoObservacion") {
            descObservacion.style.display = "block";
            descLimitacion.style.display = "none";
            txtDescObservacion.disabled = false;
            txtDescLimitacion.disabled = true;
        } else if (aptitudSelect.value === "aptoLimitacion") {
            descObservacion.style.display = "none";
            descLimitacion.style.display = "block";
            txtDescObservacion.disabled = true;
            txtDescLimitacion.disabled = false;
        } else {
            descObservacion.style.display = "none";
            descLimitacion.style.display = "none";
            txtDescObservacion.disabled = true;
            txtDescLimitacion.disabled = true;
        }
    }

    // Llama a la función inicialmente para manejar el estado inicial
    toggleAptitudDiv();
});



/* =================================================================================
 *                      Guardar datos de Historia Clinica
 * ===============================================================================*/

//--------------  Cargar Datos  -----------------------
/*function CargarItems(data) {
    //var combo1 = document.getElementById("cboMarca");
    //combo1.value = data.IdMarca;
    IdEmpleado = data.IdEmpleado;
    $('#txtNombre').val(data.Nombre);
    $('#txtCedula').val(data.Cedula);
    $('#txtSociedad').val(data.Sociedad);
    $('#txtCiudad').val(data.Ciudad);
    $('#txtAreaTrabajo').val(data.AreaTrabajo);
    $('#txtDireccion').val(data.Direccion);
    $('#txtTelefono').val(data.Telefono);
    $('#txtSexo').val(data.Sexo);
    $('#txtFechaNacimiento').val(data.Fecha_Nacimiento);
    $('#txtProvincia').val(data.Provincia);
    document.getElementById("btnGEmpleado").innerHTML = "Actualizar";
}*/

function BorrarCajas() {
    /* Esta función se encarga de borrar el contenido de ciertos campos de entrada en el formulario.
    Por ejemplo, parece borrar los campos '#txtTipoDiscapacidad' y '#txtPorcentajeDiscapacidad'. */

    $('#txtTipoDiscapacidad').val("");
    $('#txtPorcentajeDiscapacidad').text("");
}



function GuardarHistoria() {
    /*
    Esta función se encarga de guardar la información de una historia clínica en el servidor.
    Realiza una serie de verificaciones de campos requeridos y muestra mensajes de alerta en caso de faltar información.

    Luego, construye un objeto 'datosFormulario' con los datos del formulario que se desean enviar al servidor.
    Después, convierte este objeto en una cadena JSON y lo envía al servidor a través de una solicitud AJAX (asíncrona).

    Si la respuesta del servidor indica que la acción se realizó con éxito, muestra un mensaje de éxito.
    Si la respuesta indica un error, muestra un mensaje de error.

    Finalmente, limpia el contenido del div 'divMensajes' (donde se muestran mensajes informativos).
    */

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;    


    /*if ($('#txtnumhistoria').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Nombre ";
        contadorVerificacion += 1;
    }

    if ($('#numarchivo').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Edad ";
        contadorVerificacion += 1;
    }*/

    if ($('#txtNombre').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Nombre ";
        contadorVerificacion += 1;
    }

    if ($('#txtReligion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la religión ";
        contadorVerificacion += 1;
    }
    if ($('#txtGruposanguineo').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Nombre ";
        contadorVerificacion += 1;
    }

    if ($('#txtLateralidad').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Edad ";
        contadorVerificacion += 1;
    }
    if ($('#txtOrientacionSexual').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Sexo ";
        contadorVerificacion += 1;
    }

    if ($('#txtIdentidadGenero').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Sexo ";
        contadorVerificacion += 1;
    }
    if (!$('input[name="discapacidad"]:checked').val()) {
        mensajeVerificacion += "- Debe seleccionar si tiene discapacidad o no ";
        contadorVerificacion += 1;

        if ($('#txtTipoDiscapacidad').val() == "") {
            mensajeVerificacion += "- Debe ingresar el tipo de discapacidad ";
            contadorVerificacion += 1;
        }
        if ($('#txtPorcentajeDiscapacidad').text() == "") {
            mensajeVerificacion += "- Debe seleccionar el porcentaje de discapacidad";
            contadorVerificacion += 1;
        }
    }

    if ($('#txtMotivoConsulta').val() == "") {
        mensajeVerificacion += "- Debe ingresar el motivo de la consulta";
        contadorVerificacion += 1;
    }
    /*if ($('#txtAntecedentesPersonales').val() == "") {
        mensajeVerificacion += "- Debe ingresar el campo Antecedentes personales";
        contadorVerificacion += 1;
    }
    if ($('#txtAntecedentesFamiliares').val() == "") {
        mensajeVerificacion += "- Debe ingresar el campo Antecedentes familiares";
        contadorVerificacion += 1;
    }*/

    if ($('#txtSexo').val() == "FEMENINO") {
        txtExam5 = "";
        txtExam6 = "";
        if ($('#txtMenarquia').val() == "") {
            mensajeVerificacion += "- Debe ingresar la menarquia";
            contadorVerificacion += 1;
        }
        if ($('#txtCiclos').val() == "") {
            mensajeVerificacion += "- Debe ingresar los ciclos";
            contadorVerificacion += 1;
        }
        if ($('#fechaUltimaMens').val() == "") {
            mensajeVerificacion += "- Debe ingresar fecha ultima menstruación";
            contadorVerificacion += 1;
        }
        if ($('#txtNumGestas').val() == "") {
            mensajeVerificacion += "- Debe ingresar # gestas";
            contadorVerificacion += 1;
        }
        if ($('#txtNumPartos').val() == "") {
            mensajeVerificacion += "- Debe ingresar # partos";
            contadorVerificacion += 1;
        }
        if ($('#txtNumCesareas').val() == "") {
            mensajeVerificacion += "- Debe ingresar # cesareas";
            contadorVerificacion += 1;
        }
        if ($('#txtNumAbortos').val() == "") {
            mensajeVerificacion += "- Debe ingresar # abortos";
            contadorVerificacion += 1;
        }
        if ($('#txtNumHijosVivos').val() == "") {
            mensajeVerificacion += "- Debe ingresar # hijos vivos";
            contadorVerificacion += 1;
        }
        if ($('#txtNumHijosMuertos').val() == "") {
            mensajeVerificacion += "- Debe ingresar # hijos muertos";
            contadorVerificacion += 1;
        }
        if (!$('input[name="metodoplanif1"]:checked').val()) {

            mensajeVerificacion += "- Debe seleccionar si tiene metodo de planificacion ";
            contadorVerificacion += 1;

            if ($('#txtTipoPlanificacion1').val() == "") {
                mensajeVerificacion += "- Debe ingresar el tipo de planificación";
                contadorVerificacion += 1;
            }
        }
        if (txtLentes == undefined) {
            txtLentes = "";
            mensajeVerificacion += "- Debe seleccionar SI o No usa Lentes";
            contadorVerificacion += 1;
        }

        if (txtExam1 == "") {
            mensajeVerificacion += "- Debe seleccionar SI o No en Examen Papanicolaou";
            contadorVerificacion += 1;
        } else if (txtExam1 == "SI") {
            if ($('#txtanioExam1').val() == "") {
                mensajeVerificacion += "- Debe ingresar el año de examen Papanicolaou";
                contadorVerificacion += 1;
            }
            if ($('#txtResultadoExam1').val() == "") {
                mensajeVerificacion += "- Debe ingresar el resultado del examen Papanicolaou";
                contadorVerificacion += 1;
            }
        }

        if (txtExam2 == "") {
            mensajeVerificacion += "- Debe seleccionar SI o No en Examen Eco Mamario";
            contadorVerificacion += 1;
        } else if (txtExam2 == "SI") {
            if ($('#txtanioExam2').val() == "") {
                mensajeVerificacion += "- Debe ingresar el año de examen Eco Mamario";
                contadorVerificacion += 1;
            }
            if ($('#txtResultadoExam2').val() == "") {
                mensajeVerificacion += "- Debe ingresar el resultado del examen Eco Mamario";
                contadorVerificacion += 1;
            }
        }

        if (txtExam3 == "") {
            mensajeVerificacion += "- Debe seleccionar SI o No en Examen Eco Mamario";
            contadorVerificacion += 1;
        } else if (txtExam3 == "SI") {
            if ($('#txtanioExam3').val() == "") {
                mensajeVerificacion += "- Debe ingresar el año de examen Eco Mamario";
                contadorVerificacion += 1;
            }
            if ($('#txtResultadoExam3').val() == "") {
                mensajeVerificacion += "- Debe ingresar el resultado del examen Eco Mamario";
                contadorVerificacion += 1;
            }
        }

        if (txtExam4 == "") {
            mensajeVerificacion += "- Debe seleccionar SI o No en Examen Eco Mamario";
            contadorVerificacion += 1;
        } else if (txtExam4 == "SI") {
            if ($('#txtanioExam4').val() == "") {
                mensajeVerificacion += "- Debe ingresar el año de examen Eco Mamario";
                contadorVerificacion += 1;
            }
            if ($('#txtResultadoExam4').val() == "") {
                mensajeVerificacion += "- Debe ingresar el resultado del examen Eco Mamario";
                contadorVerificacion += 1;
            }
        }

        txtExam5 = "";
        txtExam6 = "";
        vidaSxActiva = $("input[name='vidaSxActiva']:checked").val();
        metPlanifiacionF = $("input[name='metodoplanif1']:checked").val();
        tipoPlanificacionF = $('#txtTipoPlanificacion1').val();
        metPlanifiacionM = "";
        tipoPlanificacionM = "";
        txtNumHijosVivosM = "";
        txtNumHijosMuertosM = "";    
    }
    if ($('#txtSexo').val() == "MASCULINO") {


        if (txtExam5 == "") {
            mensajeVerificacion += "- Debe seleccionar SI o No en Examen ANTÍGENO PROSTÁTICO";
            contadorVerificacion += 1;
        } else if (txtExam5 == "SI") {
            if ($('#txtanioExam5').val() == "") {
                mensajeVerificacion += "- Debe ingresar el año de examen ANTÍGENO PROSTÁTICO";
                contadorVerificacion += 1;
            }
            if ($('#txtResultadoExam5').val() == "") {
                mensajeVerificacion += "- Debe ingresar el resultado del examen ANTÍGENO PROSTÁTICO";
                contadorVerificacion += 1;
            }
        }

        if (txtExam6 == "") {
            mensajeVerificacion += "- Debe seleccionar SI o No en Examen ECO PROSTÁTICO";
            contadorVerificacion += 1;
        } else if (txtExam6 == "SI") {
            if ($('#txtanioExam6').val() == "") {
                mensajeVerificacion += "- Debe ingresar el año de examen ECO PROSTÁTICO";
                contadorVerificacion += 1;
            }
            if ($('#txtResultadoExam6').val() == "") {
                mensajeVerificacion += "- Debe ingresar el resultado del examen ECO PROSTÁTICO";
                contadorVerificacion += 1;
            }
        }

        if (!$('input[name="metodoplanif2"]:checked').val()) {

            mensajeVerificacion += "- Debe seleccionar si tiene metodo de planificacion ";
            contadorVerificacion += 1;

            if ($('#txtTipoPlanificacion2').val() == "") {
                mensajeVerificacion += "- Debe ingresar el tipo de planificación";
                contadorVerificacion += 1;
            }
        }
        txtExam1 = "";
        txtExam2 = "";
        txtExam3 = "";
        txtExam4 = "";
        vidaSxActiva = "";
        metPlanifiacionF = "";
        tipoPlanificacionF = "";
        metPlanifiacionM = $("input[name='metodoplanif2']:checked").val();
        tipoPlanificacionM = $('#txtTipoPlanificacion2').val();
        if ($('#txtNumHijosVivosM').val() == "") {
            mensajeVerificacion += "- Debe ingresar # hijos vivos";
            contadorVerificacion += 1;
        }
        if ($('#txtNumHijosMuertosM').val() == "") {
            mensajeVerificacion += "- Debe ingresar # hijos muertos";
            contadorVerificacion += 1;
        }
    }
    txtNumHijosVivosM = $('#txtNumHijosVivosM').val();
    txtNumHijosMuertosM = $('#txtNumHijosMuertosM').val();

    if ($('#tabacoSelect').val() == "si") {

        if ($('#txtTiempoconsumoTabaco').val() == "") {
            mensajeVerificacion += "- Debe ingresar el tiempo de consumo Tabaco ";
            contadorVerificacion += 1;
        }
        if ($('#txtCantidadTabaco').val() == "") {
            mensajeVerificacion += "- Debe seleccionar la cantidad de Tabaco";
            contadorVerificacion += 1;
        }
        if ($('#txtTiempoAbstinenciaTabaco').val() == "") {
            mensajeVerificacion += "- Debe seleccionar la cantidad de Tabaco";
            contadorVerificacion += 1;
        }
    }
    if ($('#alcoholSelect').val() == "si") {

        if ($('#txtTiempoconsumoAlcohol').val() == "") {
            mensajeVerificacion += "- Debe ingresar el tiempo de consumo Alcohol ";
            contadorVerificacion += 1;
        }
        if ($('#txtCantidadAlcohol').val() == "") {
            mensajeVerificacion += "- Debe seleccionar la cantidad de Alcohol";
            contadorVerificacion += 1;
        }
        if ($('#txtTiempoAbstinenciaAlcohol').val() == "") {
            mensajeVerificacion += "- Debe seleccionar el tiempo de abstinencia de Alcohol";
            contadorVerificacion += 1;
        }
    }
    if ($('#otraSelect').val() == "si") {
        
        if ($('#txtOtraSustancia').val() == "") {
            mensajeVerificacion += "- Debe ingresar el nombre de Otra ssubstancia ";
            contadorVerificacion += 1;
        }
        if ($('#txtTiempoconsumoOtra').val() == "") {
            mensajeVerificacion += "- Debe ingresar el tiempo de consumo Otra ssubstancia ";
            contadorVerificacion += 1;
        }
        if ($('#txtCantidadOtra').val() == "") {
            mensajeVerificacion += "- Debe seleccionar la cantidad de Otra ssubstancia";
            contadorVerificacion += 1;
        }
        if ($('#txtTiempoAbstinenciaOtra').val() == "") {
            mensajeVerificacion += "- Debe seleccionar el tiempo de abstinencia de Otra ssubstancia";
            contadorVerificacion += 1;
        }
    }
    if ($('#txtActividadFisiscaSelect').val() == "si") {
        if ($('#txtCualActividad').val() == "") {
            mensajeVerificacion += "- Debe ingresar cual actividad física realiza";
            contadorVerificacion += 1;
        }
        if ($('#txtFrecuenciaActividad').val() == "") {
            mensajeVerificacion += "- Debe ingresar el tiempo que realiza la actividad física";
            contadorVerificacion += 1;
        }
    }

    if ($('#txtMedicacionHabSelect').val() == "si") {
        if ($('#txtCualMedicamento1').val() == "") {
            mensajeVerificacion += "- Debe ingresar cual actividad física realiza";
            contadorVerificacion += 1;
        }
        if ($('#txtCantdidadMed1').val() == "") {
            mensajeVerificacion += "- Debe ingresar el tiempo que realiza la actividad física";
            contadorVerificacion += 1;
        }
    }

    if ($('#MedicacionHabSelect').val() == "") {
        mensajeVerificacion += "- Debe seleccionar si hay o no medicación habitual";
        contadorVerificacion += 1;
    } else if ($('#MedicacionHabSelect').val() == "si") {
        txtMedicacionHabSelect = $('#MedicacionHabSelect').val();
        if (contadorMedicacion === 0) {            
            txtCualMedicamento1 = $('#txtCualMedicamento1').val();
            txtCantdidadMed1 = $('#txtCantdidadMed1').val();
            txtCualMedicamento2 = "";
            txtCantdidadMed2 = "";
            txtCualMedicamento3 = "";
            txtCantdidadMed3 = "";
        } else if (contadorMedicacion === 1) {
            txtCualMedicamento1 = $('#txtCualMedicamento1').val();
            txtCantdidadMed1 = $('#txtCantdidadMed1').val();
            txtCualMedicamento2 = $('#txtCualMedicamento2').val();
            txtCantdidadMed2 = $('#txtCantdidadMed2').val();
            txtCualMedicamento3 = "";
            txtCantdidadMed3 = "";

        } else if (contadorMedicacion === 2) {
            txtCualMedicamento1 = $('#txtCualMedicamento1').val();
            txtCantdidadMed1 = $('#txtCantdidadMed1').val();
            txtCualMedicamento2 = $('#txtCualMedicamento2').val();
            txtCantdidadMed2 = $('#txtCantdidadMed2').val();
            txtCualMedicamento3 = $('#txtCualMedicamento3').val();
            txtCantdidadMed3 = $('#txtCantdidadMed3').val();
        }
    } else {
        txtMedicacionHabSelect = $('#MedicacionHabSelect').val();
        txtCualMedicamento1 = "";
        txtCantdidadMed1 = "";
        txtCualMedicamento2 = "";
        txtCantdidadMed2 = "";
        txtCualMedicamento3 = "";
        txtCantdidadMed3 = "";
    }
    if ($('#txtEnfermedadActual').val() == "") {
        mensajeVerificacion += "- Debe ingresar la descripción de la enfermedad actual";
        contadorVerificacion += 1;
    }

    if ($('#AccidentesTrabajoSelect').val() == "si") {
        if ($('#txtEspecificarAccidentesTrabajoSelect').val() == "") {
            mensajeVerificacion += "- Debe seleccionar la entidad que calificó el accidente de trabajo";
            contadorVerificacion += 1;
        }
    }
    if ($('#EnfermedadesProfSelect').val() == "si") {
        if ($('#txtEspecificarEnfermedadesProfSelect').val() == "") {
            mensajeVerificacion += "- Debe seleccionar la entidad que calificó la enfermedad Profesional";
            contadorVerificacion += 1;
        }
    }

    if ($('#txtConstPresionArterial').val() == "") {
        mensajeVerificacion += "- Debe ingresar la presión arterial";
        contadorVerificacion += 1;
    }
    if ($('#txtConstTemperatura').val() == "") {
        mensajeVerificacion += "- Debe ingresar la temperatura";
        contadorVerificacion += 1;
    }
    if ($('#txtConstFrecuenciaCardicaca').val() == "") {
        mensajeVerificacion += "- Debe ingresar la frecuancia cardiaca";
        contadorVerificacion += 1;
    }
    if ($('#txtConstSaturacionOxigeno').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Saturación de Oxígeno";
        contadorVerificacion += 1;
    }
    if ($('#txtConstFrecuenciaRespiratoria').val() == "") {
        mensajeVerificacion += "- Debe ingresar la frecuencia respiratoria";
        contadorVerificacion += 1;
    }
    if ($('#txtConstPeso').val() == "") {
        mensajeVerificacion += "- Debe ingresar el peso actual";
        contadorVerificacion += 1;
    }
    if ($('#txtConstTalla').val() == "") {
        mensajeVerificacion += "- Debe ingresar la talla";
        contadorVerificacion += 1;
    }
    if ($('#txtConstMasaCorporal').val() == "") {
        mensajeVerificacion += "- Debe ingresar el indice de masa corporal";
        contadorVerificacion += 1;
    }
    if ($('#txtConstPerimetroAbdominal').val() == "") {
        mensajeVerificacion += "- Debe ingresar la información del perimetro abdominal";
        contadorVerificacion += 1;
    }

    //----- revisar
    if ($('#txtConstPerimetroAbdominal').val() == "") {
        mensajeVerificacion += "- Debe ingresar la información del perimetro abdominal";
        contadorVerificacion += 1;
    }


    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }



    // datos adicionales para ingresar a la base de datos

    var datosInfoAdicional = "";

    datosInfoAdicional = {
        'txtOpcion': "1",
        'session': $("#txtCedula").val(),
        'txtLentes': txtLentes,
        'txtGrpVulnerables': $('#txtGrpVulnerables').val(),
        'txtAlergias': txtAlergias,
        'txtNombreAlergia': $('#txtNombreAlergia').val(),
        'txtTipoAlergia': $('#txtTipoAlergia').val(),
        'txtReaccionesAlergia': $('#txtReaccionesAlergia').val()
    };


    var datos1 = JSON.stringify([{ 'action': 'GuardarContactoEmergencia', 'parameters': datosInfoAdicional }]);


    $.ajax({
        type: "POST",
        url: url,
        data: datos1,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            $("#divMensajes").html("Guardando Información...");
        },
        success: function (respuesta) {
            var mensaje = "";
            if (respuesta.estado == "1") {
                MensajeCorrecto(respuesta.mensaje);
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




    // Ingreso para datos preocupacional

    var datosFormulario = "";

    datosFormulario = {
        'formulario': "1",
        'session': $("#ContentPlaceHolder1_txtUsuario").val(),
        'txtNombre': $('#txtNombre').val(),
        'txtEdad': $('#txtEdad').val(),
        'txtSexo': $('#txtSexo').val(),
        'txtReligion': $('#txtReligion').val(),
        'txtGruposanguineo': $('#txtGruposanguineo').val(),
        'txtLateralidad': $('#txtLateralidad').val(),
        'txtOrientacionSexual': $('#txtOrientacionSexual').val(),
        'txtIdentidadGenero': $('#txtIdentidadGenero').val(),
        'txtDiscapacidad': $("input[name='discapacidad']:checked").val(),
        'txtTipoDiscapacidad': $('#txtTipoDiscapacidad').val(),
        'txtPorcentajeDiscapacidad': $('#txtPorcentajeDiscapacidad').text(),
        'txtAreaTrabajo': $('#txtAreaTrabajo').val(),
        'txtMotivoConsulta': $('#txtMotivoConsulta').val(),
        'txtAntecedentesPersonales': $('#txtAntecedentesPersonales').val(),
        'txtMenarquia': $('#txtMenarquia').val(),
        'txtCiclos': $('#txtCiclos').val(),
        'fechaUltimaMens': $('#fechaUltimaMens').val(),
        'txtNumGestas': $('#txtNumGestas').val(),
        'txtNumPartos': $('#txtNumPartos').val(),
        'txtNumCesareas': $('#txtNumCesareas').val(),
        'txtNumAbortos': $('#txtNumAbortos').val(),
        'txtNumHijosVivos': $('#txtNumHijosVivos').val(),
        'txtNumHijosMuertos': $('#txtNumHijosMuertos').val(),
        'txtvidaSxActiva': vidaSxActiva,
        'txtPlanificacionFam1': metPlanifiacionF,
        'txtTipoPlanificacion1': tipoPlanificacionF,
        'txtExam1': txtExam1,
        'txtanioExam1': $('#txtanioExam1').val(),
        'txtResultadoExam1': $('#txtResultadoExam1').val(),
        'txtExam2': txtExam2,
        'txtanioExam2': $('#txtanioExam2').val(),
        'txtResultadoExam2': $('#txtResultadoExam2').val(),
        'txtExam3': txtExam3,
        'txtanioExam3': $('#txtanioExam3').val(),
        'txtResultadoExam3': $('#txtResultadoExam3').val(),
        'txtExam4': txtExam4,
        'txtanioExam4': $('#txtanioExam4').val(),
        'txtResultadoExam4': $('#txtResultadoExam4').val(),
        'txtExam5': txtExam5,
        'txtanioExam5': $('#txtanioExam5').val(),
        'txtResultadoExam5': $('#txtResultadoExam5').val(),
        'txtExam6': txtExam6,
        'txtanioExam6': $('#txtanioExam6').val(),
        'txtResultadoExam6': $('#txtResultadoExam6').val(),
        'txtPlanificacionFam2': metPlanifiacionM,
        'txtTipoPlanificacion2': tipoPlanificacionM,
        'txtNumHijosVivosM': txtNumHijosVivosM,
        'txtNumHijosMuertosM': txtNumHijosMuertosM,
        'txtTabacoSelect': $('#tabacoSelect').val(),
        'txtTiempoconsumoTabaco': $('#txtTiempoconsumoTabaco').val(),
        'txtCantidadTabaco': $('#txtCantidadTabaco').val(),
        'exConsumidoraSelectTabaco': $('#exConsumidoraSelectTabaco').val(),
        'txtTiempoAbstinenciaTabaco': $('#txtTiempoAbstinenciaTabaco').val(),
        'txtalcoholSelect': $('#alcoholSelect').val(),
        'txtTiempoconsumoAlcohol': $('#txtTiempoconsumoAlcohol').val(),
        'txtCantidadAlcohol': $('#txtCantidadAlcohol').val(),
        'exConsumidoraSelectAlcohol': $('#exConsumidoraSelectAlcohol').val(),
        'txtTiempoAbstinenciaAlcohol': $('#txtTiempoAbstinenciaAlcohol').val(),
        'txtOtraSustancia': $('#txtOtraSustancia').val(),
        'txtotraSelect': $('#otraSelect').val(),
        'txtTiempoconsumoOtra': $('#txtTiempoconsumoOtra').val(),
        'txtCantidadOtra': $('#txtCantidadOtra').val(),
        'exConsumidorSelectOtra': $('#exConsumidorSelectOtra').val(),
        'txtTiempoAbstinenciaOtra': $('#txtTiempoAbstinenciaOtra').val(),
        'txtActividadFisiscaSelect': $('#txtActividadFisiscaSelect').val(),
        'txtCualActividad': $('#txtCualActividad').val(),
        'txtFrecuenciaActividad': $('#txtFrecuenciaActividad').val(),
        'txtMedicacionHabSelect': txtMedicacionHabSelect,
        'txtCualMedicamento1': txtCualMedicamento1,
        'txtCantdidadMed1': txtCantdidadMed1,
        'txtCualMedicamento2': txtCualMedicamento2,
        'txtCantdidadMed2': txtCantdidadMed2,
        'txtCualMedicamento3': txtCualMedicamento3,
        'txtCantdidadMed3': txtCantdidadMed3,

        'txtAntEmpresa1': $('#txtAntEmpresa1').val(),
        'txtAntPuestoTrabajo1': $('#txtAntPuestoTrabajo1').val(),
        'txtAntActividades1': $('#txtAntActividades1').val(),
        'txtAntTiempoTrabajo1': $('#txtAntTiempoTrabajo1').val(),
        'riesgoTrabajoSelect1': $('#riesgoTrabajoSelect1').val(),
        'txtObservacion1': $('#txtObservacion1').val(),
        'txtAntEmpresa2': $('#txtAntEmpresa2').val(),
        'txtAntPuestoTrabajo2': $('#txtAntPuestoTrabajo2').val(),
        'txtAntActividades2': $('#txtAntActividades2').val(),
        'txtAntTiempoTrabajo2': $('#txtAntTiempoTrabajo2').val(),
        'riesgoTrabajoSelect2': $('#riesgoTrabajoSelect2').val(),
        'txtObservacion2': $('#txtObservacion2').val(),
        'txtAntEmpresa3': $('#txtAntEmpresa3').val(),
        'txtAntPuestoTrabajo3': $('#txtAntPuestoTrabajo3').val(),
        'txtAntActividades3': $('#txtAntActividades3').val(),
        'txtAntTiempoTrabajo3': $('#txtAntTiempoTrabajo3').val(),
        'riesgoTrabajoSelect3': $('#riesgoTrabajoSelect3').val(),
        'txtObservacion3': $('#txtObservacion3').val(),
        'txtAntEmpresa4': $('#txtAntEmpresa4').val(),
        'txtAntPuestoTrabajo4': $('#txtAntPuestoTrabajo4').val(),
        'txtAntActividades4': $('#txtAntActividades4').val(),
        'txtAntTiempoTrabajo4': $('#txtAntTiempoTrabajo4').val(),
        'riesgoTrabajoSelect4': $('#riesgoTrabajoSelect4').val(),
        'txtObservacion4': $('#txtObservacion4').val(),
        'AccidentesTrabajoSelect': $('#AccidentesTrabajoSelect').val(),
        'txtEspecificarAccidentesTrabajoSelect': $('#txtEspecificarAccidentesTrabajoSelect').val(),
        'txtObservacionesAccTrabajo': $('#txtObservacionesAccTrabajo').val(),
        'EnfermedadesProfSelect': $('#EnfermedadesProfSelect').val(),
        'txtEspecificarEnfermedadesProfSelect': $('#txtEspecificarEnfermedadesProfSelect').val(),
        'txtObservacionesEnfermedadesProf': $('#txtObservacionesEnfermedadesProf').val(),
        'txtAntecedentesFamiliares': $('#txtAntecedentesFamiliares').val(),
        'txtPuestoTrabajo1': $('#txtPuestoTrabajo1').val(),
        'txtActividadesTrabajo1': $('#txtActividadesTrabajo1').val(),
        'txtFisicoSelect1': $('#txtFisicoSelect1').val(),
        'txtMecanicoSelect1': $('#txtMecanicoSelect1').val(),
        'txtQuimicoSelect1': $('#txtQuimicoSelect1').val(),
        'txtBiologicoSelect1': $('#txtBiologicoSelect1').val(),
        'txtErgonomicoSelect1': $('#txtErgonomicoSelect1').val(),
        'txtPSicosocialSelect1': $('#txtPSicosocialSelect1').val(),
        'txtMedidadPreventiva1': $('#txtMedidadPreventiva1').val(),
        'txtPuestoTrabajo2': $('#txtPuestoTrabajo2').val(),
        'txtActividadesTrabajo2': $('#txtActividadesTrabajo2').val(),
        'txtFisicoSelect2': $('#txtFisicoSelect2').val(),
        'txtMecanicoSelect2': $('#txtMecanicoSelect2').val(),
        'txtQuimicoSelect2': $('#txtQuimicoSelect2').val(),
        'txtBiologicoSelect2': $('#txtBiologicoSelect2').val(),
        'txtErgonomicoSelect2': $('#txtErgonomicoSelect2').val(),
        'txtPSicosocialSelect2': $('#txtPSicosocialSelect2').val(),
        'txtMedidadPreventiva2': $('#txtMedidadPreventiva2').val(),
        'txtPuestoTrabajo3': $('#txtPuestoTrabajo3').val(),
        'txtActividadesTrabajo3': $('#txtActividadesTrabajo3').val(),
        'txtFisicoSelect3': $('#txtFisicoSelect3').val(),
        'txtMecanicoSelect3': $('#txtMecanicoSelect3').val(),
        'txtQuimicoSelect3': $('#txtQuimicoSelect3').val(),
        'txtBiologicoSelect3': $('#txtBiologicoSelect3').val(),
        'txtErgonomicoSelect3': $('#txtErgonomicoSelect3').val(),
        'txtPSicosocialSelect3': $('#txtPSicosocialSelect3').val(),
        'txtMedidadPreventiva3': $('#txtMedidadPreventiva3').val(),
        'txtActividadesExtraLaborales': $('#txtActividadesExtraLaborales').val(),
        'txtEnfermedadActual': $('#txtEnfermedadActual').val(),
        'txtPatologia': $('#txtPatologia').val(),
        'txtRevisionOrganos': $('#txtRevisionOrganos').val(),
        'txtConstPresionArterial': $('#txtConstPresionArterial').val(),
        'txtConstTemperatura': $('#txtConstTemperatura').val(),
        'txtConstFrecuenciaCardicaca': $('#txtConstFrecuenciaCardicaca').val(),
        'txtConstSaturacionOxigeno': $('#txtConstSaturacionOxigeno').val(),
        'txtConstFrecuenciaRespiratoria': $('#txtConstFrecuenciaRespiratoria').val(),
        'txtConstPeso': $('#txtConstPeso').val(),
        'txtConstTalla': $('#txtConstTalla').val(),
        'txtConstMasaCorporal': $('#txtConstMasaCorporal').val(),
        'txtConstPerimetroAbdominal': $('#txtConstPerimetroAbdominal').val(),
        'txtpielA': $('#pielA').prop('checked'),
        'txtpielB': $('#pielB').prop('checked'),
        'txtpielC': $('#pielC').prop('checked'),
        'txtojosA': $('#ojosA').prop('checked'),
        'txtojosB': $('#ojosB').prop('checked'),
        'txtojosC': $('#ojosC').prop('checked'),
        'txtojosD': $('#ojosD').prop('checked'),
        'txtojosE': $('#ojosE').prop('checked'),
        'txtoidoA': $('#oidoA').prop('checked'),
        'txtoidoB': $('#oidoB').prop('checked'),
        'txtoidoC': $('#oidoC').prop('checked'),
        'txtoroA': $('#oroA').prop('checked'),
        'txtoroB': $('#oroB').prop('checked'),
        'txtoroC': $('#oroC').prop('checked'),
        'txtoroD': $('#oroD').prop('checked'),
        'txtoroE': $('#oroE').prop('checked'),
        'txtnarizA': $('#narizA').prop('checked'),
        'txtnarizB': $('#narizB').prop('checked'),
        'txtnarizC': $('#narizC').prop('checked'),
        'txtnarizD': $('#narizD').prop('checked'),
        'txtcuelloA': $('#cuelloA').prop('checked'),
        'txtcuelloB': $('#cuelloB').prop('checked'),
        'txttoraxA': $('#toraxA').prop('checked'),
        'txttoraxB': $('#toraxB').prop('checked'),
        'txttoraxC': $('#toraxC').prop('checked'),
        'txtToraxD': $('#ToraxD').prop('checked'),
        'txtabdomenA': $('#abdomenA').prop('checked'),
        'txtabdomenB': $('#abdomenB').prop('checked'),
        'txtcolumnaA': $('#columnaA').prop('checked'),
        'txtcolumnaB': $('#columnaB').prop('checked'),
        'txtcolumnaC': $('#columnaC').prop('checked'),
        'txtpelvisA': $('#pelvisA').prop('checked'),
        'txtpelvisB': $('#pelvisB').prop('checked'),
        'txtextremidadesA': $('#extremidadesA').prop('checked'),
        'txtextremidadesB': $('#extremidadesB').prop('checked'),
        'txtextremidadesC': $('#extremidadesC').prop('checked'),
        'txtneurologicoA': $('#neurologicoA').prop('checked'),
        'txtneurologicoB': $('#neurologicoB').prop('checked'),
        'txtneurologicoC': $('#neurologicoC').prop('checked'),
        'txtneurologicoD': $('#neurologicoD').prop('checked'),
        'txtExamFisicoObservacion': $('#txtExamFisicoObservacion').val(),

        'txtTipoExamenSelect1': $('#txtTipoExamenSelect1').val(),
        'txtNomExamen1': $('#txtNomExamen1').val(),
        'fechaExam1': $('#fechaExam1').val(),
        'txtResExamen1': $('#txtResExamen1').val(),
        'txtTipoExamenSelect2': $('#txtTipoExamenSelect2').val(),
        'txtNomExamen2': $('#txtNomExamen2').val(),
        'fechaExam2': $('#fechaExam2').val(),
        'txtResExamen2': $('#txtResExamen2').val(),
        'txtTipoExamenSelect3': $('#txtTipoExamenSelect3').val(),
        'txtNomExamen3': $('#txtNomExamen3').val(),
        'fechaExam3': $('#fechaExam3').val(),
        'txtResExamen3': $('#txtResExamen3').val(),
        'txtTipoExamenSelect4': $('#txtTipoExamenSelect4').val(),
        'txtNomExamen4': $('#txtNomExamen4').val(),
        'fechaExam4': $('#fechaExam4').val(),
        'txtResExamen4': $('#txtResExamen4').val(),
        'txtExamenbservacion': $('#txtExamenbservacion').val(),

        'txtDiagDescripcion1': $('#txtDiagDescripcion1').val(),
        'txtDiagCIE1': $('#txtDiagCIE1').val(),
        'txtDiagnositicoSelect1': $('#txtDiagnositicoSelect1').val(),
        'txtDiagDescripcion2': $('#txtDiagDescripcion2').val(),
        'txtDiagCIE2': $('#txtDiagCIE2').val(),
        'txtDiagnositicoSelect2': $('#txtDiagnositicoSelect2').val(),
        'txtDiagDescripcion3': $('#txtDiagDescripcion3').val(),
        'txtDiagCIE3': $('#txtDiagCIE3').val(),
        'txtDiagnositicoSelect3': $('#txtDiagnositicoSelect3').val(),

        'txtAptitudSelect': $('#txtAptitudSelect').val(),
        'txtDescObservacion': $('#txtDescObservacion').val(),
        'txtDescLimitacion': $('#txtDescLimitacion').val(),
        'txtRecomendacion': $('#txtRecomendacion').val(),
        'nombre': $("#ContentPlaceHolder1_txtLoginUsuario").val()
        };

        var datos = JSON.stringify([{ 'action': 'GuardarNuevaHistoria', 'parameters': datosFormulario }]);


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

        return;    
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
    //let cedulaBuscar = document.getElementById("txtEmpleado").value;

});


