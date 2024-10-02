// Obtener el elemento del botón
var btnBuscar = document.getElementById("btnBuscarDatosPersonales");
var buttons = document.querySelectorAll(".btn-custom"); // Obtener todos los botones
let IdPerfil = 0;
var inputActivoNum = null; // Variable global para rastrear el input usado en DIAGNOSTICO

// Variables globales para almacenar la opción seleccionada en los checkbox
let evalRetiroSelected = null;
let diagSelected = null;
let relacionTrabajoSelected = null;


function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
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
    var obsAptitud = document.getElementById("obsAptitud");
    var descObservacion = document.getElementById("descObservacion");
    var descLimitacion = document.getElementById("descLimitacion");
    var txtobsAptitud = document.getElementById("txtObsAptitud");
    var txtDescObservacion = document.getElementById("txtDescObservacion");
    var txtDescLimitacion = document.getElementById("txtDescLimitacion");

    // Agrega un controlador de eventos change al campo select
    aptitudSelect.addEventListener("change", toggleAptitudDiv);

    // Función para mostrar u ocultar los campos según la selección
    function toggleAptitudDiv() {
        if (aptitudSelect.value === "aptoObservacion") {
            obsAptitud.style.display = "none";
            descObservacion.style.display = "block";
            descLimitacion.style.display = "none";
            txtobsAptitud.disabled = true;
            txtDescObservacion.disabled = false;
            txtDescLimitacion.disabled = true;
        } else if (aptitudSelect.value === "aptoLimitacion") {
            obsAptitud.style.display = "none";
            descObservacion.style.display = "none";
            descLimitacion.style.display = "block";
            txtobsAptitud.disabled = true;
            txtDescObservacion.disabled = true;
            txtDescLimitacion.disabled = false;
        } else {
            obsAptitud.style.display = "block";
            descObservacion.style.display = "none";
            descLimitacion.style.display = "none";
            txtobsAptitud.disabled = false;
            txtDescObservacion.disabled = true;
            txtDescLimitacion.disabled = true;
        }
    }

    // Llama a la función inicialmente para manejar el estado inicial
    toggleAptitudDiv();
});




document.addEventListener('DOMContentLoaded', function () {
    console.log('La página ha sido cargada');

    // Selecciona todos los checkbox con el atributo name "evalRetiro"
    const evalRetiroCheckboxes = document.querySelectorAll('input[name="evalRetiro"]');
    evalRetiroCheckboxes.forEach((checkbox) => {
        checkbox.addEventListener('click', (e) => {
            evalRetiroCheckboxes.forEach((groupCheckbox) => {
                if (groupCheckbox !== checkbox) {
                    groupCheckbox.checked = false;
                }
            });
            evalRetiroSelected = checkbox.value;
        });
    });

    // Selecciona todos los checkbox con el atributo name "diag"
    const diagCheckboxes = document.querySelectorAll('input[name="diag"]');
    diagCheckboxes.forEach((checkbox) => {
        checkbox.addEventListener('click', (e) => {
            diagCheckboxes.forEach((groupCheckbox) => {
                if (groupCheckbox !== checkbox) {
                    groupCheckbox.checked = false;
                }
            });
            diagSelected = checkbox.value;
        });
    });

    // Selecciona todos los checkbox con el atributo name "relacionTrabajo"
    const relacionTrabajoCheckboxes = document.querySelectorAll('input[name="relacionTrabajo"]');
    relacionTrabajoCheckboxes.forEach((checkbox) => {
        checkbox.addEventListener('click', (e) => {
            relacionTrabajoCheckboxes.forEach((groupCheckbox) => {
                if (groupCheckbox !== checkbox) {
                    groupCheckbox.checked = false;
                }
            });
            relacionTrabajoSelected = checkbox.value;
        });
    });
});

function GuardarCertificado() {
    
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;
        
    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    evalRetiroSelected = $("input[name='evalRetiro']:checked").val();

    var datosFormulario = "";

    datosFormulario = {
        'formulario': "5",
        'session': $("#ContentPlaceHolder1_txtUsuario").val(),
        'txtNombre': $('#txtNombre').val(),
        'txtSexo': $('#txtSexo').val(),
        'txtEdad': $('#txtEdad').val(),
        'txtPuestoTrabajo': $('#txtPuestoTrabajo').val(),

        'txtFechaEmision': $('#fechaEmision').val(),
        'txtEvaluacion': $('#txtEvaluacion').val(),
               
        'txtAptitudSelect': $('#txtAptitudSelect').val(),
        'txtDescAptitud': $('#txtDescAptitud').val(),
        'txtDescObservacion': $('#txtDescObservacion').val(),
        'txtDescLimitacion': $('#txtDescLimitacion').val(),

        
        'txtevalRetiro': evalRetiroSelected,
        'txtdiag': diagSelected,
        'txtrelacionTrabajo': relacionTrabajoSelected,

        'txtRecomendacion': $('#txtRecomendacion').val(),
      

        'txtMotivoConsulta': "",
        'txtEnfermedadActual': "",
        'txtConstPresionArterial': "",
        'txtConstTemperatura': "",
        'txtConstFrecuenciaCardicaca': "",
        'txtConstSaturacionOxigeno': "",
        'txtConstFrecuenciaRespiratoria': "",
        'txtConstPeso': "",
        'txtConstTalla': "",
        'txtConstMasaCorporal': "",
        'txtConstPerimetroAbdominal': "",
        'txtpielA': "",
        'txtpielB': "",
        'txtpielC': "",
        'txtojosA': "",
        'txtojosB': "",
        'txtojosC': "",
        'txtojosD': "",
        'txtojosE': "",
        'txtoidoA': "",
        'txtoidoB': "",
        'txtoidoC': "",
        'txtoroA': "",
        'txtoroB': "",
        'txtoroC': "",
        'txtoroD': "",
        'txtoroE': "",
        'txtnarizA': "",
        'txtnarizB': "",
        'txtnarizC': "",
        'txtnarizD': "",
        'txtcuelloA': "",
        'txtcuelloB': "",
        'txttoraxA': "",
        'txttoraxB': "",
        'txttoraxC': "",
        'txtToraxD': "",
        'txtabdomenA': "",
        'txtabdomenB': "",
        'txtcolumnaA': "",
        'txtcolumnaB': "",
        'txtcolumnaC': "",
        'txtpelvisA': "",
        'txtpelvisB': "",
        'txtextremidadesA': "",
        'txtextremidadesB': "",
        'txtextremidadesC': "",
        'txtneurologicoA': "",
        'txtneurologicoB': "",
        'txtneurologicoC': "",
        'txtneurologicoD': "",
        'txtExamFisicoObservacion': "",
        'txtTipoExamenSelect1': "",
        'txtNomExamen1': "",
        'fechaExam1': "",
        'txtResExamen1': "",
        'txtTipoExamenSelect2': "",
        'txtNomExamen2': "",
        'fechaExam2': "",
        'txtResExamen2': "",
        'txtTipoExamenSelect3': "",
        'txtNomExamen3': "",
        'fechaExam3': "",
        'txtResExamen3': "",
        'txtExamenbservacion': "",
        'txtDiagDescripcion1': "",
        'txtDiagCIE1': "",
        'txtDiagnositicoSelect1': "",
        'txtDiagDescripcion2': "",
        'txtDiagCIE2': "",
        'txtDiagnositicoSelect2': "",
        'txtDiagDescripcion3': "",
        'txtDiagCIE3': "",
        'txtDiagnositicoSelect3': "",

        'txtReligion': "",
        'txtGruposanguineo': "",
        'txtLateralidad': "",
        'txtOrientacionSexual': "",
        'txtIdentidadGenero': "",
        'txtDiscapacidad': "",
        'txtTipoDiscapacidad': "",
        'txtPorcentajeDiscapacidad': "",
        'txtAreaTrabajo': "",
        'txtAntecedentesPersonales': "",
        'txtAntecedentesFamiliares': "",
        'txtPuestoTrabajo1': "",
        'txtActividadesTrabajo1': "",
        'txtFisicoSelect1': "",
        'txtMecanicoSelect1': "",
        'txtQuimicoSelect1': "",
        'txtBiologicoSelect1': "",
        'txtErgonomicoSelect1': "",
        'txtPSicosocialSelect1': "",
        'txtMedidadPreventiva1': "",
        'txtPuestoTrabajo2': "",
        'txtActividadesTrabajo2': "",
        'txtFisicoSelect2': "",
        'txtMecanicoSelect2': "",
        'txtQuimicoSelect2': "",
        'txtBiologicoSelect2': "",
        'txtErgonomicoSelect2': "",
        'txtPSicosocialSelect2': "",
        'txtMedidadPreventiva2': "",
        'txtPuestoTrabajo3': "",
        'txtActividadesTrabajo3': "",
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
        'fechaUltimaMens': "",
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
        'AccidentesTrabajoSelect': "",
        'txtEspecificarAccidentesTrabajoSelect': "",
        'txtObservacionesAccTrabajo': "",
        'EnfermedadesProfSelect': "",
        'txtEspecificarEnfermedadesProfSelect': "",
        'txtObservacionesEnfermedadesProf': "",
        'txtActividadesExtraLaborales': "",
        'txtTipoExamenSelect4': "",
        'txtNomExamen4': "",
        'fechaExam4': "",
        'txtResExamen4': "",
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
