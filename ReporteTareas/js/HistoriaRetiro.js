//  ***   Mostrar boton Cargar informacion   ***
// Obtener el elemento del botón
var btnBuscar = document.getElementById("btnBuscarDatosPersonales");
var btnCargar = document.getElementById("btnCargarDatosReintegro");
var buttons = document.querySelectorAll(".btn-custom"); // Obtener todos los botones
let txtLentes, txtExam1, txtExam2, txtExam3, txtExam4, txtExam5, txtExam6, metPlanifiacionF, tipoPlanificacionF, txtNumHijosVivosM, txtNumHijosMuertosM;
let txtMedicacionHabSelect, txtCualMedicamento1, txtCantdidadMed1, txtCualMedicamento2, txtCantdidadMed2, txtCualMedicamento3, txtCantdidadMed3;
let contadorMedicacion = 0;
let IdPerfil = 0;

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}
function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}
function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

// INGRESAR SOLO MAYUSCOLAS EN EL INGRESO DEL NOMBRE
function convertirAMayusculas(input) {
    input.value = input.value.toUpperCase();
}


/*===============================================================================
 *                  Obtener la cedula de la pagina principal
 *==============================================================================*/
// Obtener la cedula ingresada en la pagina principal HistoriaClinica
//document.addEventListener('DOMContentLoaded', () => {
//    //const txtEmpleado = document.getElementById('txtEmpleado');
//    const txtEmpleado = document.getElementById("txtEmpleado").value;
//    const urlParams = new URLSearchParams(window.location.search);
//    const cedulaEmpleado = urlParams.get('cedula');

//    if (cedulaEmpleado) {
//        //txtEmpleado.value = cedulaEmpleado; // Carga la cédula en el campo
//        BuscarEmpleado(cedulaEmpleado); // Ejecuta la función BuscarEmpleado
//    }
//});
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
 * =================================================================================*/

// Definir la función para buscar un empleado
function BuscarEmpleado(cedula) {
    //let cedulaBuscar = document.getElementById("txtEmpleado").value;

    let cedulaBuscar = cedula;

    ObtenerListaEmpleados(cedulaBuscar, "", "");
    document.getElementById("btnCarga").style.display = "flex";
}
function ObtenerListaEmpleados(cedula, descripcion, tipo2) {

    var Datos = "[{ \"action\": \"BuscarEmpleadoPorCedula\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" + cedula + "\"} }]";

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

// Controlamos los menus de seleccin multiple Examen fisicos
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

function GuardarRetiro() {
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

    /*if ($('#txtReligion').val() == "") {
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
    if ($('#txtMotivoConsulta').val() == "") {
        mensajeVerificacion += "- Debe ingresar el motivo de la consulta";
        contadorVerificacion += 1;
    }*/
    /*if ($('#txtAntecedentesPersonales').val() == "") {
        mensajeVerificacion += "- Debe ingresar el campo Antecedentes personales";
        contadorVerificacion += 1;
    }
    if ($('#txtAntecedentesFamiliares').val() == "") {
        mensajeVerificacion += "- Debe ingresar el campo Antecedentes familiares";
        contadorVerificacion += 1;
    }*/


    /*if ($('#tabacoSelect').val() == "si") {

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
    }*/


    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }


    var datosFormulario = "";

    datosFormulario = {
        'formulario': "4",
        'session': $("#ContentPlaceHolder1_txtUsuario").val(),
        'txtNombre': $('#txtNombre').val(),
        'txtSexo': $('#txtSexo').val(),
        'actividades1': $('#txtActividad1').val(),
        'txtActividadesTrabajo1': $('#txtFactorRiesgo1').val(),
        'actividades2': $('#txtActividad2').val(),
        'txtActividadesTrabajo2': $('#txtFactorRiesgo2').val(),
        'actividades3': $('#txtActividad3').val(),
        'txtActividadesTrabajo3': $('#txtFactorRiesgo3').val(),
        'txtAntecedentesPersonales': $('#txtAntecedentesPersonales').val(),
        'AccidentesTrabajoSelect': $('#AccidentesTrabajoSelect').val(),
        'txtEspecificarAccidentesTrabajoSelect': $('#txtEspecificarAccidentesTrabajoSelect').val(),
        'txtObservacionesAccTrabajo': $('#txtObservacionesAccTrabajo').val(),
        'EnfermedadesProfSelect': $('#EnfermedadesProfSelect').val(),
        'txtEspecificarEnfermedadesProfSelect': $('#txtEspecificarEnfermedadesProfSelect').val(),
        'txtObservacionesEnfermedadesProf': $('#txtObservacionesEnfermedadesProf').val(),
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
        'EvalRetiroSelect': $('#EvaluacionRetiroSelect').val(),
        'EvalRetiroObservacion': $('#txtEvaluacionRetiroObservacion').val(),        
        'txtRecomendacion': $('#txtRecomendacion').val(),

        'txtEdad': "",
        'txtReligion': "",
        'txtGruposanguineo': "",
        'txtLateralidad': "",
        'txtOrientacionSexual': "",
        'txtIdentidadGenero': "",
        'txtDiscapacidad': "",
        'txtTipoDiscapacidad': "",
        'txtPorcentajeDiscapacidad': "",
        'fechaUltimaMens': "",
        'txtAreaTrabajo': "",
        'txtAntecedentesFamiliares': "",
        'txtPuestoTrabajo1': "",        
        'txtFisicoSelect1': "",
        'txtMecanicoSelect1': "",
        'txtQuimicoSelect1': "",
        'txtBiologicoSelect1': "",
        'txtErgonomicoSelect1': "",
        'txtPSicosocialSelect1': "",
        'txtMedidadPreventiva1': "",
        'txtPuestoTrabajo2': "",        
        'txtFisicoSelect2': "",
        'txtMecanicoSelect2': "",
        'txtQuimicoSelect2': "",
        'txtBiologicoSelect2': "",
        'txtErgonomicoSelect2': "",
        'txtPSicosocialSelect2': "",
        'txtMedidadPreventiva2': "",
        'txtPuestoTrabajo3': "",
        'txtFisicoSelect3': "",
        'txtMecanicoSelect3': "",
        'txtQuimicoSelect3': "",
        'txtBiologicoSelect3': "",
        'txtErgonomicoSelect3': "",
        'txtPSicosocialSelect3': "",
        'txtMedidadPreventiva3': "",
        'txtPatologia': "",
        'txtRevisionOrganos': "",
        'txtMenarquia': "",
        'txtCiclos': "",
        'txtNumGestas': "",
        'txtNumPartos': "",
        'txtNumCesareas': "",
        'txtNumAbortos': "",
        'txtNumHijosVivos': "",
        'txtNumHijosMuertos': "",
        'txtPlanificacionFam1': "",
        'txtTipoPlanificacion1': "",
        'txtvidaSxActiva': "",
        'txtExam1': "",
        'txtanioExam1': "",
        'txtResultadoExam1': "",
        'txtExam2': "",
        'txtanioExam2': "",
        'txtResultadoExam2': "",
        'txtExam3': "",
        'txtanioExam3': "",
        'txtResultadoExam3': "",
        'txtExam4': "",
        'txtanioExam4': "",
        'txtResultadoExam4': "",
        'txtExam5': "",
        'txtanioExam5': "",
        'txtResultadoExam5': "",
        'txtExam6': "",
        'txtanioExam6': "",
        'txtResultadoExam6': "",
        'txtPlanificacionFam2': "",
        'txtTipoPlanificacion2': "",
        'txtNumHijosVivosM': "",
        'txtNumHijosMuertosM': "",
        'txtTipoExamenSelect3': "",
        'txtNomExamen3': "",
        'fechaExam3': "",
        'txtResExamen3': "",

        'txtTabacoSelect': "",
        'txtTiempoconsumoTabaco': "",
        'txtCantidadTabaco': "",
        'exConsumidoraSelectTabaco': "",
        'txtTiempoAbstinenciaTabaco': "",
        'txtalcoholSelect': "",
        'txtTiempoconsumoAlcohol': "",
        'txtCantidadAlcohol': "",
        'exConsumidoraSelectAlcohol': "",
        'txtTiempoAbstinenciaAlcohol': "",
        'txtOtraSustancia': "",
        'txtotraSelect': "",
        'txtTiempoconsumoOtra': "",
        'txtCantidadOtra': "",
        'exConsumidorSelectOtra': "",
        'txtTiempoAbstinenciaOtra': "",
        'txtActividadFisiscaSelect': "",
        'txtCualActividad': "",
        'txtFrecuenciaActividad': "",
        'txtMedicacionHabSelect': "",
        'txtCualMedicamento1': "",
        'txtCantdidadMed1': "",
        'txtCualMedicamento2': "",
        'txtCantdidadMed2': "",
        'txtCualMedicamento3': "",
        'txtCantdidadMed3': "",
        'txtMotivoConsulta': "",
        'txtEnfermedadActual': "",
        'txtAntEmpresa1': "",
        'txtAntPuestoTrabajo1': "",
        'txtAntActividades1': "",
        'txtAntTiempoTrabajo1': "",
        'riesgoTrabajoSelect1': "",
        'txtObservacion1': "",
        'txtAntEmpresa2': "",
        'txtAntPuestoTrabajo2': "",
        'txtAntActividades2': "",
        'txtAntTiempoTrabajo2': "",
        'riesgoTrabajoSelect2': "",
        'txtObservacion2': "",
        'txtAntEmpresa3': "",
        'txtAntPuestoTrabajo3': "",
        'txtAntActividades3': "",
        'txtAntTiempoTrabajo3': "",
        'riesgoTrabajoSelect3': "",
        'txtObservacion3': "",
        'txtAntEmpresa4': "",
        'txtAntPuestoTrabajo4': "",
        'txtAntActividades4': "",
        'txtAntTiempoTrabajo4': "",
        'riesgoTrabajoSelect4': "",
        'txtObservacion4': "",
        'txtActividadesExtraLaborales': "",
        'txtTipoExamenSelect4': "",
        'txtNomExamen4': "",
        'fechaExam4': "",
        'txtResExamen4': "",
        'txtAptitudSelect': "",
        'txtDescObservacion': "",
        'txtDescLimitacion': "",
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