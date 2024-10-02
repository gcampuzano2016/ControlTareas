//  ***   Mostrar boton Cargar informacion   ***
// Obtener el elemento del botón
var btnBuscar = document.getElementById("btnBuscarDatosPersonales");
var btnCargar = document.getElementById("btnCargarDatosInm");
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


function GuardarInmunizaciones() {
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

    


    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }


    var datosFormulario = "";

    datosFormulario = {
        'session': $("#ContentPlaceHolder1_txtUsuario").val(),
        'txtNombre': $('#txtNombre').val(),
        'txtEdad': $('#txtEdad').val(),
        'txtSexo': $('#txtSexo').val(),
        'txtAreaTrabajo': $('#txtAreaTrabajo').val(),
        'txtAntecedentesPersonales': $('#txtAntecedentesPersonales').val(),
        'txtAntecedentesFamiliares': $('#txtAntecedentesFamiliares').val(),
        'txtPuestoTrabajo1': $('#txtPuestoTrabajo').val(),

        'txtfechaTetanos1': $('#fechaTetanos1').val(),
        'txtfechaTetanos2': $('#fechaTetanos2').val(),
        'txtfechaTetanos3': $('#fechaTetanos3').val(),
        'txtfechaTetanos4': $('#fechaTetanos4').val(),
        'txtfechaTetanos5': $('#fechaTetanos5').val(),

        'txtloteTetanos1': $('#txtTetanosLote1').val(),
        'txtloteTetanos2': $('#txtTetanosLote2').val(),
        'txtloteTetanos3': $('#txtTetanosLote3').val(),
        'txtloteTetanos4': $('#txtTetanosLote4').val(),
        'txtloteTetanos5': $('#txtTetanosLote5').val(),

        'txtesquemaTetanos1': $('#SelectTetanosEsquema1').val(),
        'txtesquemaTetanos2': $('#SelectTetanosEsquema2').val(),
        'txtesquemaTetanos3': $('#SelectTetanosEsquema3').val(),
        'txtesquemaTetanos4': $('#SelectTetanosEsquema4').val(),
        'txtesquemaTetanos5': $('#SelectTetanosEsquema5').val(),

        'txtnombreTetanos1': $('#txtTetanosNombre1').val(),
        'txtnombreTetanos2': $('#txtTetanosNombre2').val(),
        'txtnombreTetanos3': $('#txtTetanosNombre3').val(),
        'txtnombreTetanos4': $('#txtTetanosNombre4').val(),
        'txtnombreTetanos5': $('#txtTetanosNombre5').val(),

        'txtestablecimientoTetanos1': $('#txtTetanosEstablecimiento1').val(),
        'txtestablecimientoTetanos2': $('#txtTetanosEstablecimiento2').val(),
        'txtestablecimientoTetanos3': $('#txtTetanosEstablecimiento3').val(),
        'txtestablecimientoTetanos4': $('#txtTetanosEstablecimiento4').val(),
        'txtestablecimientoTetanos5': $('#txtTetanosEstablecimiento5').val(),

        'txtobsTetanos1': $('#txtTetanosObs1').val(),
        'txtobsTetanos2': $('#txtTetanosObs2').val(),
        'txtobsTetanos3': $('#txtTetanosObs3').val(),
        'txtobsTetanos4': $('#txtTetanosObs4').val(),
        'txtobsTetanos5': $('#txtTetanosObs5').val(),


        'txtfechaHepA1': $('#fechaHepatitisA1').val(),
        'txtfechaHepA2': $('#fechaHepatitisA2').val(),
        'txtfechaHepA3': $('#fechaHepatitisA3').val(),

        'txtfechaHepB1': $('#fechaHepatitisB1').val(),
        'txtfechaHepB2': $('#fechaHepatitisB2').val(),
        'txtfechaHepB3': $('#fechaHepatitisB3').val(),

        'txtloteHepA1': $('#txtHepatitisALote1').val(),
        'txtloteHepA2': $('#txtHepatitisALote2').val(),
        'txtloteHepA3': $('#txtHepatitisALote3').val(),

        'txtloteHepB1': $('#txtHepatitisBLote1').val(),
        'txtloteHepB2': $('#txtHepatitisBLote2').val(),
        'txtloteHepB3': $('#txtHepatitisBLote3').val(),

        'txtesquemaHepA1': $('#SelectHepatitisAEsquema1').val(),
        'txtesquemaHepA2': $('#SelectHepatitisAEsquema2').val(),
        'txtesquemaHepA3': $('#SelectHepatitisAEsquema3').val(),

        'txtesquemaHepB1': $('#SelectHepatitisBEsquema1').val(),
        'txtesquemaHepB2': $('#SelectHepatitisBEsquema2').val(),
        'txtesquemaHepB3': $('#SelectHepatitisBEsquema3').val(),

        'txtnombreHepA1': $('#txtHepatitisANombre1').val(),
        'txtnombreHepA2': $('#txtHepatitisANombre2').val(),
        'txtnombreHepA3': $('#txtHepatitisANombre3').val(),

        'txtnombreHepB1': $('#txtHepatitisBNombre1').val(),
        'txtnombreHepB2': $('#txtHepatitisBNombre2').val(),
        'txtnombreHepB3': $('#txtHepatitisBNombre3').val(),

        'txtestablecimientoHepA1': $('#txtHepatitisAEstablecimiento1').val(),
        'txtestablecimientoHepA2': $('#txtHepatitisAEstablecimiento2').val(),
        'txtestablecimientoHepA3': $('#txtHepatitisAEstablecimiento3').val(),

        'txtestablecimientoHepB1': $('#txtHepatitisBEstablecimiento1').val(),
        'txtestablecimientoHepB2': $('#txtHepatitisBEstablecimiento2').val(),
        'txtestablecimientoHepB3': $('#txtHepatitisBEstablecimiento3').val(),

        'txtobsHepA1': $('#txtHepatitisAObs1').val(),
        'txtobsHepA2': $('#txtHepatitisAObs2').val(),
        'txtobsHepA3': $('#txtHepatitisAObs3').val(),

        'txtobsHepB1': $('#txtHepatitisBObs1').val(),
        'txtobsHepB2': $('#txtHepatitisBObs2').val(),
        'txtobsHepB3': $('#txtHepatitisBObs3').val(),


        'txtfechaInfluenza': $('#fechaInfluenza1').val(),
        'txtfechaFiebre': $('#fechaFiebre1').val(),
        'txtfechaSarampion1': $('#fechaSarampion1').val(),
        'txtfechaSarampion2': $('#fechaSarampion2').val(),

        'txtloteInfluenza': $('#txtInfluenzaLote1').val(),
        'txtloteFiebre': $('#txtFiebreLote1').val(),
        'txtloteSarampion1': $('#txtSarampionLote1').val(),
        'txtloteSarampion2': $('#txtSarampionLote2').val(),

        'txtesquemaInfluenza': $('#SelectInfluenzaEsquema1').val(),
        'txtesquemaFiebre': $('#SelectFiebreEsquema1').val(),
        'txtesquemaSarampion1': $('#SelectSarampionEsquema1').val(),
        'txtesquemaSarampion2': $('#SelectSarampionEsquema2').val(),

        'txtnombreInfluenza': $('#txtInfluenzaNombre1').val(),
        'txtnombreFiebre': $('#txtFiebreNombre1').val(),
        'txtnombreSarampion1': $('#txtSarampionNombre1').val(),
        'txtnombreSarampion2': $('#txtSarampionNombre2').val(),

        'txtestablecimientoInfluenza': $('#txtInfluenzaEstablecimiento1').val(),
        'txtestablecimientoFiebre': $('#txtFiebreEstablecimiento1').val(),
        'txtestablecimientoSarampion1': $('#txtSarampionEstablecimiento1').val(),
        'txtestablecimientoSarampion2': $('#txtSarampionEstablecimiento2').val(),

        'txtobsInfluenza': $('#txtInfluenzaObs1').val(),
        'txtobsFiebre': $('#txtFiebreObs1').val(),
        'txtobsSarampion1': $('#txtSarampionObs1').val(),
        'txtobsSarampion2': $('#txtSarampionObs2').val(),

        'nombre': $("#ContentPlaceHolder1_txtLoginUsuario").val()
    };

    var datos = JSON.stringify([{ 'action': 'GuardarHisInmunizaciones', 'parameters': datosFormulario }]);


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

}

// Definir el contador fuera de la función para que sea global
var contadorInmunizaciones = 1;

function agregarInmunizacion() {
    var container = document.getElementById("inmunizacionContainer");
    var newInmunizacion = document.createElement('div');

    // Modificar IDs de los campos de la nueva inmunización
    var newId = contadorInmunizaciones + 1; // Generar un nuevo ID

    // Clonar el contenido interno del contenedor original
    newInmunizacion.innerHTML = container.innerHTML;

    // Modificar los IDs en el nuevo elemento con el índice de la inmunización
    newInmunizacion.querySelectorAll("[id^='txtNuevaDosis1']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='fechaNuevo1']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoLote1']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='SelectNuevoEsquema1']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoNombre1']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoEstablecimiento1']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoObs1']").forEach(function (element) {
        element.id += "Inm" + newId;
    });


    newInmunizacion.querySelectorAll("[id^='txtNuevaDosis2']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='fechaNuevo2']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoLote2']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='SelectNuevoEsquema2']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoNombre2']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoEstablecimiento2']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoObs2']").forEach(function (element) {
        element.id += "Inm" + newId;
    });


    newInmunizacion.querySelectorAll("[id^='txtNuevaDosis3']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='fechaNuevo3']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoLote3']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='SelectNuevoEsquema3']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoNombre3']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoEstablecimiento3']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoObs3']").forEach(function (element) {
        element.id += "Inm" + newId;
    });


    newInmunizacion.querySelectorAll("[id^='txtNuevaDosis4']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='fechaNuevo4']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoLote4']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='SelectNuevoEsquema4']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoNombre4']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoEstablecimiento4']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoObs4']").forEach(function (element) {
        element.id += "Inm" + newId;
    });


    newInmunizacion.querySelectorAll("[id^='txtNuevaDosis5']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='fechaNuevo5']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoLote5']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='SelectNuevoEsquema5']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoNombre5']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoEstablecimiento5']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    newInmunizacion.querySelectorAll("[id^='txtNuevoObs5']").forEach(function (element) {
        element.id += "Inm" + newId;
    });

    // Restablecer valores de los campos
    newInmunizacion.querySelector("[id^='txtNuevaDosis1']").textContent = newId + "°";
    newInmunizacion.querySelector("[id^='fechaNuevo1']").value = "";
    newInmunizacion.querySelector("[id^='txtNuevoLote1']").value = "";
    newInmunizacion.querySelector("[id^='SelectNuevoEsquema1']").selectedIndex = 0;
    newInmunizacion.querySelector("[id^='txtNuevoNombre1']").value = "";
    newInmunizacion.querySelector("[id^='txtNuevoEstablecimiento1']").value = "";
    newInmunizacion.querySelector("[id^='txtNuevoObs1']").value = "";

    // Agregar la nueva inmunización arriba del botón
    container.appendChild(newInmunizacion);

    // Incrementar el contador después de agregar el nuevo elemento
    contadorInmunizaciones++;

    // Deshabilitar el botón después de crear cinco filas
    if (contadorInmunizaciones >= 3) {
        document.getElementById("btnNuevaInmunizacion").disabled = true;
    }
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