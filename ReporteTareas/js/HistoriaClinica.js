//  ***   Mostrar boton Cargar informacion   ***
// Obtener el elemento del botón
var btnBuscar = document.getElementById("btnBuscarDatosPersonales");
var btnCargar = document.getElementById("btnCargarDatosPersonales");
var buttons = document.querySelectorAll(".btn-custom"); // Obtener todos los botones
let txtAlergias;
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

function VerSeleccionFormulario() {
    document.getElementById("cuadroFormularios").style.display = "block";
    document.getElementById("cuadroListaForms").style.display = "none"; 
}
function VerListaFormulario() {
    document.getElementById("cuadroFormularios").style.display = "none";
    document.getElementById("cuadroListaForms").style.display = "block";
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
                VerSeleccionFormulario();
                break;
            case 'pestania2':
                VerListaFormulario();
                break;
            case 'pestania3':
                VerFormConsulta();
                break;            
            default:
                break;
        }
    });

    var pestaniaSeleccionada = document.getElementById(pestaniaId);
    pestaniaSeleccionada.classList.add('tab-seleccionada');
}
//cambios---------------------------------------------------------------------------------------------------------------------

document.addEventListener('DOMContentLoaded', function () {
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

});





function BuscarEmpleado2() {

    if ($('#txtEmpleado').val().length > 4) {
        let cedulaBuscar = document.getElementById("txtEmpleado").value;
        idSeleccionado = 0;
        ObtenerListaEmpleados2(cedulaBuscar, "","");
    }
    else {
        document.getElementById("comboEmpleados2").style.display = "none";
    }
}
function ObtenerListaEmpleados2(cedula, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarEmpleadoPorCedula\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" + cedula + "\"} }]";
    CargarPagina('#comboEmpleados2', 'ObtenerListaTareas.ashx', Datos, "tableSelectEmpleado", idproceso);
}
function CargarEmpleado2(ID, NOMBRE) {
    $('#txtEmpleado').val(NOMBRE);
    idCliente2 = ID;
    document.getElementById("comboEmpleados2").style.display = "none";
}
/*function ObtenerListaComboEmpleados2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"0\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboCliente2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}*/
function RecorreJSONTableSelectEmpleado(json, boton, idSeleccionado) {

    var x = "";

        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarEmpleado2(\"" + item.Cedula + "\", \"" + item.Nombre + "\");'>" + item.Nombre + "</a></li>";
        });

    document.getElementById("comboEmpleados2").innerHTML = x;
    document.getElementById("comboEmpleados2").style.display = "block";
    
}


// Obtener la cedula ingresada de cualquier formulario
document.addEventListener('DOMContentLoaded', () => {
    //const txtEmpleado = document.getElementById('txtEmpleado');
    
    const urlParams = new URLSearchParams(window.location.search);
    const cedulaEmpleado = urlParams.get('cedula');

    document.getElementById("txtEmpleado").value = cedulaEmpleado;
    if (cedulaEmpleado) {
        // Si hay un valor de cédula en la URL, ejecutar la función buscarEmpleado()
        BuscarEmpleado();
    }
});

/* =================================================================================
 *                      Cargar datos de la base al Historial
 * =================================================================================*/

// Definir la función para buscar un empleado
function BuscarEmpleado(cedulaBuscar) {
    // Si no se recibe una cédula, obtén el valor del campo de texto
    if (!cedulaBuscar) {
        cedulaBuscar = document.getElementById("txtEmpleado").value;
    }

    // Muestra las pestañas y los formularios
    document.getElementById("pestanias").style.display = "block";
    document.getElementById("cuadroFormularios").style.display = "block";

    // Llama a la función para obtener la lista de empleados
    ObtenerListaEmpleados(cedulaBuscar, "", "");
}

document.addEventListener('DOMContentLoaded', function () {
    // Obtén el valor del campo oculto
    var cedulaEmpleado = $("#ContentPlaceHolder1_hiddenCedulaField").val();

    // Si existe una cédula en el campo oculto, la pasamos a la función
    if (cedulaEmpleado && cedulaEmpleado.trim() !== "") {

        document.getElementById("txtEmpleado").value = cedulaEmpleado;
        BuscarEmpleado(cedulaEmpleado); // Llama a la función con la cédula del campo oculto
    } 
});


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
    if (tipoControl == "tableSelectFormularios") {
        contenido = RecorreJSONTableSelectFormulario(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectContactos") {
        contenido = RecorreJSONTableSelectContacto(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectEmpleado") {
        contenido = RecorreJSONTableSelectEmpleado(json, boton, idSeleccionado);
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
        let FechaNacimiento = item.Fecha_Nacimiento;
        document.getElementById("fechaNac").value = FechaNacimiento;
        let Sociedad = item.Sociedad;
        document.getElementById("txtSociedad").value = Sociedad;
        let AreaTrabajo = item.AreaTrabajo;
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
        document.getElementById("txtNombreUsuario").textContent = Nombre; 
        document.getElementById("txtIdEmpleado").textContent = Cedula;
    });
}



/* =================================================================================
 *              Cargar datos del Contacto de emergencia desde la base
 * =================================================================================*/

// Definir la función para buscar un empleado
function ObtenerListaContactos(cedula, descripcion, tipo2) {

    var Datos = "[{ \"action\": \"BuscarContactoEmpleadoPorCedula\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" + cedula + "\"} }]";

    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectContactos", tipo2);
}
function RecorreJSONTableSelectContacto(json, boton, idSeleccionado) {

    dtContacto(json);
}

function dtContacto(json) {

    $.each(json, function (i, item) {

        let NombreContacto = item.NomContactoEmergencia;
        document.getElementById("txtNombreContacto").value = NombreContacto;
        let TelfContacto = item.TelfContactoEmergencia;
        document.getElementById("txtTelefonoContacto").value = TelfContacto;
        let ParentescoContacto = item.ParentescoContacto;
        document.getElementById("txtParentescoContacto").value = ParentescoContacto;
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


function VerFormularios() {
    Tipo = $('#txtTipoEvaluacionSelect').val();
    Nombre = $('#txtNombre').val();
    Sociedad = $('#txtSociedad').val();
    AreaTrabajo = $('#txtAreaTrabajo').val();
    fecha1 = $('#fechaEvaluacionDesde').val();
    fecha2 = $('#fechaEvaluacionHasta').val();
    //document.getElementById("editBoton").style.display = "none";

    ObtenerListaFormularios(Tipo, Sociedad, AreaTrabajo, Nombre, fecha1, fecha2, "");
}
function ObtenerListaFormularios(tipo, sociedad, areaTrabajo, nombre, fecha1, fecha2, tipo2) {
    var DatosLF = "[{ \"action\": \"BuscarListaFormularios\", \"parameters\" : { tipo : \"" + tipo + "\", nombre : \"" + nombre + "\", sociedad : \"" + sociedad + "\", areaTrabajo : \"" + areaTrabajo + "\", fecha1 : \"" + fecha1 + "\", fecha2: \"" + fecha2 + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', DatosLF, "tableSelectFormularios", tipo2);
}
function RecorreJSONTableSelectFormulario(json, boton, idSeleccionado) {

    dtFormularios(json);
}
function Create() {
    if ($.fn.DataTable.isDataTable('#tbl_Formularios')) {
        $('#tbl_Formularios').DataTable().destroy();
    }
    $('#tbl_Formularios tbody').empty();
}
function dtFormularios(json) {
    //<button type='button' value='Actualizar' title='Editar' class='btn btn btn-editMenu btn-xs'><i class='fa fa-edit' aria-hidden='true'></i></button>  
    Create();
    table1 = null;

    table1 = $('#tbl_Formularios').DataTable({
        data: json,
        columns: [
            { data: 'Nombre' },
            { data: 'Sociedad' },
            { data: 'AreaTrabajo' },
            { data: 'Tipo' },
            { data: 'Fecha' },
            { defaultContent: "<a title='Ver archivo' class='btn btn-abrirFormulario btn-xs'><i class='fa fa-eye' aria-hidden='true'></i></a>" }
         ],

        language: {
            "decimal": ",",
            "thousands": ".",
            "emptyTable": "No hay información",
            "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
            "infoEmpty": "Mostrando 0 to 0 of 0 Entradas",
            "infoFiltered": "(Filtrado de _MAX_ total entradas)",
            "infoPostFix": "",
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ Entradas",
            "loadingRecords": "Cargando...",
            "processing": "Procesando...",
            "search": "Buscar:",
            "zeroRecords": "Sin resultados encontrados",
            "paginate": {
                "first": "Primero",
                "last": "Ultimo",
                "next": "Siguiente",
                "previous": "Anterior"
            },
        },
        orderCellsTop: true,
        fixedHeader: true
    });
}


$(document).on('click', '.btn-abrirFormulario', function () {
    // Obtiene el nombre del archivo desde los datos de la fila
    var data = table1.row($(this).closest('tr')).data();
    var nombreArchivo = data.Nombre; // Asegúrate de que el nombre sea el correcto
    var nombreCarpeta = $('#txtNombre').val();
    // Construye la URL completa al archivo
    //var url = '/HistoriasClinicas/' + nombreCarpeta + '/' + nombreArchivo;
    var DatosA = "[{ \"action\": \"AbrirDocHistoria\", \"parameters\" : { nombreArchivo : \"" + nombreArchivo + "\", nombreCarpeta: \"" + nombreCarpeta + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', DatosA, "tableSelectFormularios");

    // Abre el archivo en una nueva ventana del navegador
    //window.open(url, '_blank');

});


/* ===================================================================
 *      Cambio de color de cuadros para seleccionar formulario
 * ===================================================================*/
document.addEventListener('DOMContentLoaded', () => {
    // Selecciona todos los elementos con la clase "box"
    const boxes = document.querySelectorAll('.box');

    // Recorre cada cuadro y aplica el comportamiento
    boxes.forEach((box) => {
        const originalColor = box.style.backgroundColor;
        
        box.addEventListener('mouseenter', () => {
            // Oscurece el color del cuadro actual
            const darkerColor = darkenColor(originalColor, 0.1); // Puedes ajustar el valor para oscurecer más o menos
            box.style.backgroundColor = darkerColor;            
            // Cambia el cursor a una mano
            box.style.cursor = 'pointer';
        });

        box.addEventListener('mouseleave', () => {
            // Restaura el color original del cuadro
            box.style.backgroundColor = originalColor;
            // Restaura el cursor predeterminado
            box.style.cursor = 'default';
        });
    });

    // Función para oscurecer un color dado en formato "rgb(r, g, b)"
    function darkenColor(color, factor) {
        const [r, g, b] = color.match(/\d+/g);
        const newR = Math.max(0, r - factor * 255);
        const newG = Math.max(0, g - factor * 255);
        const newB = Math.max(0, b - factor * 255);
        return `rgb(${newR}, ${newG}, ${newB})`;
    }
});


/* ============================================
 *          Seleccion de formulario
 * ===========================================*/
document.addEventListener('DOMContentLoaded', () => {
    const btnEvaluacionPreocupacional = document.getElementById('btnEvaluacionPreocupacional');
    const btnEvaluacionPeriodica = document.getElementById('btnEvaluacionPeriodica');
    const btnEvaluacionReintegro = document.getElementById('btnEvaluacionReintegro');
    const btnEvaluacionRetiro = document.getElementById('btnEvaluacionRetiro');
    const btnRegInmunizaciones = document.getElementById('btnRegInmunizaciones'); 
    const btnCertificado = document.getElementById('btnCertificado');

    const sendPostRequest = (actionUrl) => {
        let cedulaEmpleado = $('#txtCedula').val();

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

    btnEvaluacionPreocupacional.addEventListener('click', () => {
        sendPostRequest('DepartamentoMedico.aspx');
    });

    btnEvaluacionPeriodica.addEventListener('click', () => {
        sendPostRequest('HistoriaPeriodica.aspx');
    });

    btnEvaluacionReintegro.addEventListener('click', () => {
        sendPostRequest('HistoriaReintegro.aspx');
    });

    btnEvaluacionRetiro.addEventListener('click', () => {
        sendPostRequest('HistoriaRetiro.aspx');
    });

    btnRegInmunizaciones.addEventListener('click', () => {
        sendPostRequest('HistoriaInmunizaciones.aspx');
    });

    btnCertificado.addEventListener('click', () => {
        sendPostRequest('HistoriaCertificado.aspx');
    });
});


/* =============================================================================== *
 *      Funcion para guardar y buscar los datos del Contacto de emrgencia          *
 * =============================================================================== */
function GuardarContacto() {

        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var mensajeVerificacion = "";
        var tipoMensaje = "warning";
        var contadorVerificacion = 0;


        if ($('#txtNombreContacto').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Nombre de Contacto";
            contadorVerificacion += 1;
        }

        if ($('#txtTelefonoContacto').val() == "") {
            mensajeVerificacion += "- Debe ingresar el teléfono de Contacto ";
            contadorVerificacion += 1;
        }

        if ($('#txtParentescoContacto').val() == "") {
            mensajeVerificacion += "- Debe ingresar el parentesco de Contacto ";
            contadorVerificacion += 1;
        } 

        if (contadorVerificacion > 0) {
            alerta(mensajeVerificacion);
            return;
        }

        var datosInfoAdicional = "";

        datosInfoAdicional = {
            'txtOpcion': "2",
            'session': $("#txtEmpleado").val(),
            'txtNombreContacto': $('#txtNombreContacto').val(),
            'txtTelfContacto': $('#txtTelefonoContacto').val(),
            'txtParentescoContacto': $('#txtParentescoContacto').val()
        };

        var datos = JSON.stringify([{ 'action': 'GuardarContactoEmergencia', 'parameters': datosInfoAdicional }]);

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

function MensajeContactoEmergencia() {
    $('#msgContactoEmergencia').modal('show');
    let cedulaBu = document.getElementById("txtEmpleado").value;
    //document.getElementById("pestanias").style.display = "block";
    //document.getElementById("cuadroFormularios").style.display = "block";
    ObtenerListaContactos(cedulaBu, "", "");      
}


/*$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    ObtenerListaComboEmpleados2();
});*/