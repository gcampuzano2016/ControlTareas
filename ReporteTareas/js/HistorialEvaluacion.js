//  ***   Mostrar boton Cargar informacion   ***
// Obtener el elemento del botón
var btnBuscar = document.getElementById("btnBuscarDatosPersonales");
var buttons = document.querySelectorAll(".btn-custom"); // Obtener todos los botones
let txtAutorizacionBtn1, txtAutorizacionBtn2= "";
//let txtLentes, txtAlergias, txtExam1, txtExam2, txtExam3, txtExam4, metPlanifiacionM, tipoPlanificacionM, vidaSxActiva, txtExam5, txtExam6, metPlanifiacionF, tipoPlanificacionF, txtNumHijosVivosM, txtNumHijosMuertosM = "";
let txtMedicacionHabSelect, txtCualMedicamento1, txtCantdidadMed1, txtCualMedicamento2, txtCantdidadMed2, txtCualMedicamento3, txtCantdidadMed3;
let txtActividadFisicaSelect, txtCualActividad1, txtFrecuenciaActividad1, txtCualActividad2, txtFrecuenciaActividad2, txtCualActividad3, txtFrecuenciaActividad3 = "";
let contadorActividad = 0;
let contadorMedicacion = 0;
let IdPerfil = 0;
var inputActivoNum = null; // Variable global para rastrear el input usado en DIAGNOSTICO

var contadorActLab = 1;


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


// PONE CON OTRO COLOR LOS PLACEHOLDER DE TODOS LOS SELECT CON LA CLASE select-placeholder
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.select-placeholder').forEach(function (select) {
        select.addEventListener('change', function () {
            if (!this.value || this.value === '') {
                this.style.color = '#94a3b8';
            } else {
                this.style.color = '#334155';
            }
        });
    });
});


// INGRESAR SOLO MAYUSCOLAS EN EL INGRESO DEL NOMBRE
function convertirAMayusculas(input) {
    input.value = input.value.toUpperCase();
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

    var Datos = "[{ \"action\": \"BuscarEmpleadoPorCedula\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" + cedula + "\"} }]";

    CargarPagina('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
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

        //let Telefono = item.Telefono;
        //document.getElementById("txtTelefono").value = Telefono;
        //let Direccion = item.Direccion;
        //document.getElementById("txtDireccion").value = Direccion;
        //let Correo = item.Correo;
        //document.getElementById("txtCorreo").value = Correo;


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
    if (buttonId === 'siMetodoFem') {
        otherButtonId = 'noMetodoFem';
        MetodoFem = "SI";
    } else if (buttonId === 'noMetodoFem') {
        otherButtonId = 'siMetodoFem';
        MetodoFem = "NO";
    } else if (buttonId === 'siMetodomMasc') {
        otherButtonId = 'noMetodomMasc';
        MetodoFem = "SI";
    } else if (buttonId === 'noMetodomMasc') {
        otherButtonId = 'siMetodomMasc';
        MetodoFem = "NO";
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

// Botones de SI o NO de los botones nuevos rojo y verde
function mostrarInput(inputId, mostrar) {
    const inputDiv = document.getElementById(inputId);
    if (mostrar) {
        inputDiv.classList.add('visible');
    } else {
        inputDiv.classList.remove('visible');
    }
}
// Limpiar el input de Cual? en caso de que se seleccione NO
function mostrarInput(inputId, mostrar) {
    const inputDiv = document.getElementById(inputId);
    if (mostrar) {
        inputDiv.classList.add('visible');
    } else {
        inputDiv.classList.remove('visible');
        // Limpiar todos los inputs dentro del div
        inputDiv.querySelectorAll('input, textarea').forEach(function (el) {
            el.value = '';
        });
    }
}




// ── NUEVA: Validacion de año ────────────────────────
function validarAnio(input) {
    var valor = parseInt(input.value);
    if (input.value.length > 4) {
        input.value = input.value.slice(0, 4);
        return;
    }
    if (input.value.length === 4) {
        if (valor < 1900) {
            input.value = '';
            alert('El año debe ser mayor a 1900');
        } else if (valor > 2030) {
            input.value = '';
            alert('El año debe ser menor al año actual');
        }
    }
}




/*===================================================================================================
 *  Funciones para que los campos sean editables en Habitos Toxicos si la opcion seleccionada es SI
 *===================================================================================================*/
function handleTabacoSelect() {
    var tabacoSelect = document.getElementById("tabacoSelect");
    var txtTiempoconsumo = document.getElementById("txtTiempoconsumoTabaco");
    //var txtCantidad = document.getElementById("txtCantidadTabaco");
    var exConsumidoraSelectTabaco = document.getElementById("exConsumidoraSelectTabaco");
    var txtTiempoAbstinenciaTabaco = document.getElementById("txtTiempoAbstinenciaTabaco");

    if (tabacoSelect.value === "si") {
        txtTiempoconsumo.disabled = false;
        //txtCantidad.disabled = false;
        exConsumidoraSelectTabaco.disabled = false;
        txtTiempoAbstinenciaTabaco.disabled = false;
    } else {
        txtTiempoconsumo.disabled = true;
        //txtCantidad.disabled = true;
        exConsumidoraSelectTabaco.disabled = true;
        txtTiempoAbstinenciaTabaco.disabled = true;

        // Si se selecciona "No", restablecer los valores a su estado inicial
        txtTiempoconsumo.value = "";
        //txtCantidad.value = "";
        exConsumidoraSelectTabaco.value = "no";
        txtTiempoAbstinenciaTabaco.value = "";
    }
}
function handleAlcoholSelect() {
    var alcoholSelect = document.getElementById("alcoholSelect");
    var txtTiempoconsumoAlcohol = document.getElementById("txtTiempoconsumoAlcohol");
    //var txtCantidadAlcohol = document.getElementById("txtCantidadAlcohol");
    var exConsumidoraSelectAlcohol = document.getElementById("exConsumidoraSelectAlcohol");
    var txtTiempoAbstinenciaAlcohol = document.getElementById("txtTiempoAbstinenciaAlcohol");

    if (alcoholSelect.value === "si") {
        txtTiempoconsumoAlcohol.disabled = false;
        //txtCantidadAlcohol.disabled = false;
        exConsumidoraSelectAlcohol.disabled = false;
        txtTiempoAbstinenciaAlcohol.disabled = false;
    } else {
        txtTiempoconsumoAlcohol.disabled = true;
        //txtCantidadAlcohol.disabled = true;
        exConsumidoraSelectAlcohol.disabled = true;
        txtTiempoAbstinenciaAlcohol.disabled = true;

        txtTiempoconsumoAlcohol.value = "";
        //txtCantidadAlcohol.value = "";
        exConsumidoraSelectAlcohol.value = "no";
        txtTiempoAbstinenciaAlcohol.value = "";
    }
}
function handleOtraSelect() {
    var otraSelect = document.getElementById("otraSelect");
    var txtTiempoconsumoOtra = document.getElementById("txtTiempoconsumoOtra");
    //var txtCantidadOtra = document.getElementById("txtCantidadOtra");
    var exConsumidorSelectOtra = document.getElementById("exConsumidorSelectOtra");
    var txtTiempoAbstinenciaOtra = document.getElementById("txtTiempoAbstinenciaOtra");

    if (otraSelect.value === "si") {
        txtTiempoconsumoOtra.disabled = false;
        //txtCantidadOtra.disabled = false;
        exConsumidorSelectOtra.disabled = false;
        txtTiempoAbstinenciaOtra.disabled = false;
    } else {
        txtTiempoconsumoOtra.disabled = true;
        //txtCantidadOtra.disabled = true;
        exConsumidorSelectOtra.disabled = true;
        txtTiempoAbstinenciaOtra.disabled = true;

        txtTiempoconsumoOtra.value = "";
        //txtCantidadOtra.value = "";
        exConsumidorSelectOtra.value = "no";
        txtTiempoAbstinenciaOtra.value = "";
    }
}


//funcion para que los campos sean editables en Estilo de vida Actividad fisica si la opcion seleccionada es SI
function handleActividadSelect() {
    var txtActividadFisicaSelect = document.getElementById("ActividadFisiscaSelect");
    var txtCualActividad1 = document.getElementById("txtCualActividad1");
    var txtFrecuenciaActividad1 = document.getElementById("txtFrecuenciaActividad1");
    var botonAgregarActividad = document.getElementById("botonAgregarActividad");

    if (txtActividadFisicaSelect.value === "si") {
        txtCualActividad1.disabled = false;
        txtFrecuenciaActividad1.disabled = false;
        botonAgregarActividad.disabled = false;
    } else {
        txtCualActividad1.disabled = true;
        txtFrecuenciaActividad1.disabled = true;
        botonAgregarActividad.disabled = true;

        txtCualActividad1.value = "";
        txtFrecuenciaActividad1.value = "";
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

/*=================================================================================
 *                Función para agregar actividad fisica y una nueva medicación
 *=================================================================================*/
function agregarActividad() {
    var boton = document.getElementById("botonAgregarActividad");
    var newId = contadorActividad + 2;

    // Crear nueva fila sin el select
    var newActividad = document.createElement("div");
    newActividad.className = "tabla-data-row";
    newActividad.innerHTML =
        '<div class="tabla-col-label">Actividad física</div>' +
        '<div class="tabla-col-sm"></div>' + // espacio vacío donde iba el select
        '<div class="tabla-col-md">' +
        '<input id="txtCualActividad' + newId + '" type="text" class="cuadros-fondo-input-ingreso" placeholder="Actividad">' +
        '</div>' +
        '<div class="tabla-col-md">' +
        '<input id="txtFrecuenciaActividad' + newId + '" type="number" class="cuadros-fondo-input-ingreso" placeholder="Cantidad">' +
        '</div>';

    // Insertar ANTES del botón
    boton.parentNode.insertBefore(newActividad, boton.parentNode.querySelector('div[style]'));

    contadorActividad++;
    if (contadorActividad >= 2) {
        boton.disabled = true;
    }
}
function agregarMedicacion() {
    var boton = document.getElementById("botonAgregarMedicacion");
    var newId = contadorMedicacion + 2;

    // Crear nueva fila sin el select
    var newMedicacion = document.createElement("div");
    newMedicacion.className = "tabla-data-row";
    newMedicacion.innerHTML =
        '<div class="tabla-col-label">Medicación habitual</div>' +
        '<div class="tabla-col-sm"></div>' + // espacio vacío donde iba el select
        '<div class="tabla-col-md">' +
        '<input id="txtCualMedicamento' + newId + '" type="text" class="cuadros-fondo-input-ingreso" placeholder="Medicamento">' +
        '</div>' +
        '<div class="tabla-col-md">' +
        '<input id="txtCantdidadMed' + newId + '" type="number" class="cuadros-fondo-input-ingreso" placeholder="Cantidad">' +
        '</div>';

    // Insertar ANTES del botón
    boton.parentNode.insertBefore(newMedicacion, boton.parentNode.querySelector('div[style]'));

    contadorMedicacion++;
    if (contadorMedicacion >= 2) {
        boton.disabled = true;
    }
}





/*=====================================================================
 *          Buscamos los codigos CIE en la base de datos
 *====================================================================*/

function BuscarCodigosCIE() {
    let txtDiagDescripcion = document.getElementById(`txtDiagDescripcion${inputActivoNum}`);
    if (txtDiagDescripcion.value.length > 2) {
        let CIEBuscar = txtDiagDescripcion.value;
        idSeleccionado = 0;
        ObtenerListaCodigosCIE(CIEBuscar, "", "", inputActivoNum);
    } else {
        document.getElementById(`comboCodigos${inputActivoNum}`).style.display = "none";
    }
}

function ObtenerListaCodigosCIE(codigo, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarCodigoCIE\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" + codigo + "\"} }]";
    CargarPagina(`#comboCodigos${inputActivoNum}`, 'ObtenerNuevaListaTareas.ashx', Datos, "tableSelectBusquedaCIE", idproceso);
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

        const dropdownContent = container.querySelector('.dropdown-content, .dropdown-content2');

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


/* ====================================================================
 *      Funciones para la seccion de Accidentes laborales
 * ====================================================================*/
function toggleActLab(btnId, otherBtnId) {
    var btn = document.getElementById(btnId);
    var other = document.getElementById(otherBtnId);
    if (other) {
        other.classList.remove('active');
        other.style.backgroundColor = '';
        other.style.color = '';
    }
    btn.classList.add('active');
    btn.style.backgroundColor = '#7E38D8';
    btn.style.color = '#FFFFFF';
}

function agregarActividadLaboral() {
    contadorActLab++;
    var n = contadorActLab;
    var container = document.getElementById('actLabContainer');

    var registro = document.createElement('div');
    registro.className = 'act-registro cuadros-blancos-redondeados';
    registro.style.marginBottom = '12px';

    registro.innerHTML =
        // Tabla superior
        '<table style="width:100%;table-layout:fixed;border-collapse:collapse;">' +
        '<thead><tr>' +
        '<th style="width:30%;background:#1a9ea8;color:#fff;font-size:10px;font-weight:700;text-transform:uppercase;text-align:center;padding:8px 4px;">Centro de Trabajo</th>' +
        '<th style="width:30%;background:#17b8c4;color:#fff;font-size:10px;font-weight:700;text-transform:uppercase;text-align:center;padding:8px 4px;">Actividad Desempeñada</th>' +
        '<th style="width:20%;background:#14c4c4;color:#fff;font-size:10px;font-weight:700;text-transform:uppercase;text-align:center;padding:8px 4px;">Trabajo</th>' +
        '<th style="width:20%;background:#0eb8a8;color:#fff;font-size:10px;font-weight:700;text-transform:uppercase;text-align:center;padding:8px 4px;">Tiempo de Trabajo</th>' +
        '</tr></thead>' +
        '<tbody><tr>' +
        '<td style="padding:4px;border:1px solid #e2e8f0;">' +
        '<input id="txtCentroTrabajo' + n + '" type="text" class="cuadros-fondo-input-ingreso" placeholder="Centro de trabajo" style="width:100%;">' +
        '</td>' +
        '<td style="padding:4px;border:1px solid #e2e8f0;">' +
        '<input id="txtActividadDesempeñaba' + n + '" type="text" class="cuadros-fondo-input-ingreso" placeholder="Actividad" style="width:100%;">' +
        '</td>' +
        '<td style="padding:4px;border:1px solid #e2e8f0;">' +
        '<select id="txtActividadDesempeñada' + n + '" class="cuadros-fondo-input-ingreso select-placeholder" style="width:100%;">' +
        '<option value="" selected>Selec.</option>' +
        '<option value="Anterior">Anterior</option>' +
        '<option value="Actual">Actual</option>' +
        '</select>' +
        '</td>' +
        '<td style="padding:4px;border:1px solid #e2e8f0;">' +
        '<input id="txtTiemporTrabajo' + n + '" type="text" class="cuadros-fondo-input-ingreso" placeholder="Tiempo" style="width:100%;">' +
        '</td>' +
        '</tr></tbody>' +
        '</table>' +

        // Tabla inferior
        '<table style="width:100%;table-layout:fixed;border-collapse:collapse;margin-top:0.5rem;">' +
        '<thead><tr>' +
        '<th style="width:25%;background:#0ca898;color:#fff;font-size:10px;font-weight:700;text-transform:uppercase;text-align:center;padding:8px 4px;">Acc. / Enf. Profesionales</th>' +
        '<th style="width:15%;background:#0a9888;color:#fff;font-size:10px;font-weight:700;text-transform:uppercase;text-align:center;padding:8px 4px;">IESS</th>' +
        '<th style="width:15%;background:#099080;color:#fff;font-size:10px;font-weight:700;text-transform:uppercase;text-align:center;padding:8px 4px;">Fecha</th>' +
        '<th style="width:22.5%;background:#088878;color:#fff;font-size:10px;font-weight:700;text-transform:uppercase;text-align:center;padding:8px 4px;">Especificar</th>' +
        '<th style="width:22.5%;background:#077870;color:#fff;font-size:10px;font-weight:700;text-transform:uppercase;text-align:center;padding:8px 4px;">Observación</th>' +
        '</tr></thead>' +
        '<tbody><tr>' +
        '<td style="padding:4px;border:1px solid #e2e8f0;">' +
        '<select id="selectAccEnfTrabajo' + n + '" class="cuadros-fondo-input-ingreso select-placeholder" style="width:100%;">' +
        '<option value="" selected>Selec.</option>' +
        '<option value="Incidente">Incidente</option>' +
        '<option value="Accidente">Accidente</option>' +
        '<option value="Enfermedad Profesional">Enf. Profesional</option>' +
        '</select>' +
        '</td>' +
        '<td style="padding:4px;border:1px solid #e2e8f0;text-align:center;">' +
        '<div style="display:flex;gap:4px;justify-content:center;">' +
        '<button style="padding:6px 18px;border-radius:6px;" type="button" id="AccTrab-si' + n + '" class="btn btn-custom btn-sm" onclick="toggleActLab(\'AccTrab-si' + n + '\',\'AccTrab-no' + n + '\')">Sí</button>' +
        '<button style="padding:6px 18px;border-radius:6px;" type="button" id="AccTrab-no' + n + '" class="btn btn-custom btn-sm" onclick="toggleActLab(\'AccTrab-no' + n + '\',\'AccTrab-si' + n + '\')">No</button>' +
        '</div>' +
        '</td>' +
        '<td style="border:1px solid #e2e8f0;">' +
        '<input type="date" class="form-control" id="fechaAccTrabLab' + n + '" style="width:100%;">' +
        '</td>' +
        '<td style="padding:4px;border:1px solid #e2e8f0;">' +
        '<input id="txtCualActividadLab' + n + '" type="text" class="cuadros-fondo-input-ingreso" placeholder="Especificar" style="width:100%;">' +
        '</td>' +
        '<td style="padding:4px;border:1px solid #e2e8f0;">' +
        '<input id="txtObservacionActividadLab' + n + '" type="text" class="cuadros-fondo-input-ingreso" placeholder="Observación" style="width:100%;">' +
        '</td>' +
        '</tr></tbody>' +
        '</table>';

    container.appendChild(registro);

    if (contadorActLab >= 19) {
        document.querySelector('[onclick="agregarActividadLaboral()"]').disabled = true;
        alert('Se ha alcanzado el límite máximo de 19 registros.');
    }
}

var actLabRows = [];
for (var i = 1; i <= contadorActLab; i++) {
    actLabRows.push({
        centro: $('#txtCentroTrabajo' + i).val(),
        actividad: $('#txtActividadDesempeñaba' + i).val(),
        trabajo: $('#txtActividadDesempeñada' + i).val(),
        tiempo: $('#txtTiemporTrabajo' + i).val(),
        accEnf: $('#selectAccEnfTrabajo' + i).val(),
        iess: $('#AccTrab-si' + i).hasClass('active') ? 'SI' : ($('#AccTrab-no' + i).hasClass('active') ? 'NO' : ''),
        fecha: $('#fechaAccTrabLab' + i).val(),
        especificar: $('#txtCualActividadLab' + i).val(),
        observacion: $('#txtObservacionActividadLab' + i).val()
    });
}
//'actLabRows': JSON.stringify(actLabRows),



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

    var url = "ObtenerNuevaListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;



    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }



    // datos adicionales para ingresar a la base de datos

    //if (txtAlergias == undefined) {
    //    txtAlergias = "";
    //    //mensajeVerificacion += "- Debe seleccionar SI o No en Examen ECO PROSTÁTICO";
    //    //contadorVerificacion += 1;
    //}
    //if (txtLentes == undefined) {
    //    txtLentes = "";
    //    //mensajeVerificacion += "- Debe seleccionar SI o No en Examen ECO PROSTÁTICO";
    //    //contadorVerificacion += 1;
    //}

    //var datosInfoAdicional = "";

    //datosInfoAdicional = {
    //    'txtOpcion': "1",
    //    'session': $("#txtCedula").val(),
    //    'txtLentes': txtLentes,
    //    'txtGrpVulnerables': $('#txtGrpVulnerables').val(),
    //    'txtAlergias': txtAlergias,
    //    'txtNombreAlergia': $('#txtNombreAlergia').val(),
    //    'txtTipoAlergia': $('#txtTipoAlergia').val(),
    //    'txtReaccionesAlergia': $('#txtReaccionesAlergia').val()
    //};


    //var datos1 = JSON.stringify([{ 'action': 'GuardarContactoEmergencia', 'parameters': datosInfoAdicional }]);


    //$.ajax({
    //    type: "POST",
    //    url: url,
    //    data: datos1,
    //    contentType: "application/json; charset=utf-8",
    //    dataType: "json",
    //    beforeSend: function () {
    //        $("#divMensajes").html("Guardando Información...");
    //    },
    //    success: function (respuesta) {
    //        var mensaje = "";
    //        if (respuesta.estado == "1") {
    //            MensajeCorrecto(respuesta.mensaje);
    //            $("#divMensajes").html("");
    //        }
    //        else if (respuesta.estado == "0") {
    //            MensajeIncorrecto(respuesta.mensaje);
    //        }
    //    },
    //    error: function (objeto, msgError, objError) {
    //        var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
    //        MensajeIncorrecto(mesnajeError);
    //    }
    //});




    // Ingreso para datos preocupacional

    var datosFormulario = "";


    datosFormulario = {
        'formulario': "6",
        'session': $("#ContentPlaceHolder1_txtUsuario").val(),
        'txtNumHistoria': $('#txtNumHistoria').val(),
        'txtNumArchivo': $('#txtNumArchivo').val(),
        'txtSociedad': $('#txtSociedad').val(),
        'txtNombre': $('#txtNombre').val(),
        'txtEdad': $('#txtEdad').val(),
        'txtSexo': $('#txtSexo').val(), 
        'fechaNac': $('#fechaNac').val(),
        'txtGruposanguineo': $('#txtGruposanguineo').val(),
        'txtLateralidad': $('#txtLateralidad').val(), 
        'AtencionPrioritariaSelect': $('#AtencionPrioritariaSelect').val(),
        'txtPuestoTrabajo': $('#txtPuestoTrabajo').val(),
        'txtAreaTrabajo': $('#txtAreaTrabajo').val(),
        'txtMotivoConsulta': $('#txtMotivoConsulta').val(), 
        'fechaAtencion': $('#fechaAtencion').val(),
        'fechaIngresoTrab': $('#fechaIngresoTrab').val(),
        'fechaReintegroTrab': $('#fechaReintegroTrab').val(),
        'fechaUltimoDiaLab': $('#fechaUltimoDiaLab').val(), 
        'selectMotivoConsulta': $('#selectMotivoConsulta').val(),

        'txtAntecedentesPersonales': $('#txtAntecedentesPersonales').val(),
        'txtAntecedentesFamiliares': $('#txtAntecedentesFamiliares').val(),

        'txtAutorizacionBtn1': $("input[name='txtAutorizacionBtn1']:checked").val() || '',
        'txtAutorizacionBtn2': $("input[name='txtAutorizacionBtn2']:checked").val() || '',
        'tratamientohormonalcual': $('#tratamientohormonalcual').val(),

        // Gineco femenino
        'fechaUltimaMens': $('#fechaUltimaMens').val(),
        'txtNumGestas': $('#txtNumGestas').val(),
        'txtNumPartos': $('#txtNumPartos').val(),
        'txtNumCesareas': $('#txtNumCesareas').val(),
        'txtNumAbortos': $('#txtNumAbortos').val(),
        'txtPlanificacionFam1': $('#siMetodoFem').hasClass('active') ? 'si' : ($('#noMetodoFem').hasClass('active') ? 'no' : ''),
        'txtTipoPlanificacion1': $('#txtCualMetodoFem').val(),

        'txtPlanificacionFam2': $('#siMetodomMasc').hasClass('active') ? 'si' : ($('#noMetodomMasc').hasClass('active') ? 'no' : ''),
        'txtTipoPlanificacion2': $('#txtCualMasc').val(),

        'txtNomExamen1F': $('#txtNomExamen1F').val(),
        'fechaExam1F': $('#fechaExam1F').val(),
        'txtResExamen1F': $('#txtResExamen1F').val(),
        'txtNomExamen2F': $('#txtNomExamen2F').val(),
        'fechaExam2F': $('#fechaExam2F').val(),
        'txtResExamen2F': $('#txtResExamen2F').val(), 

        'txtNomExamen1M': $('#txtNomExamen1M').val(),
        'fechaExam1M': $('#fechaExam1M').val(),
        'txtResExamen1M': $('#txtResExamen1M').val(),
        'txtNomExamen2M': $('#txtNomExamen2M').val(),
        'fechaExam2M': $('#fechaExam2M').val(),
        'txtResExamen2M': $('#txtResExamen2M').val(),


        // Gineco masculino
        //'txtPlanificacionFam2': $('#siMetodomMasc').is(':checked') ? 'SI' : ($('#noMetodomMasc').is(':checked') ? 'NO' : ''),
        //'txtTipoPlanificacion2': $('#txtCualMasc').val(),
        'txtExam5': $('#txtNomExamen1M').val(),
        'txtanioExam5': $('#fechaExam1M').val(),
        'txtResultadoExam5': $('#txtResExamen1M').val(),
        'txtExam6': $('#txtNomExamen2M').val(),
        'txtanioExam6': $('#fechaExam2M').val(),
        'txtResultadoExam6': $('#txtResExamen2M').val(),

        // Consumo sustancias
        'txtTabacoSelect': $('#tabacoSelect').val(),
        'txtTiempoconsumoTabaco': $('#txtTiempoconsumoTabaco').val(),
        //'txtCantidadTabaco': $('#txtCantidadTabaco').val(),
        'exConsumidoraSelectTabaco': $('#exConsumidoraSelectTabaco').val(),
        'txtTiempoAbstinenciaTabaco': $('#txtTiempoAbstinenciaTabaco').val(),
        'txtalcoholSelect': $('#alcoholSelect').val(),
        'txtTiempoconsumoAlcohol': $('#txtTiempoconsumoAlcohol').val(),
        //'txtCantidadAlcohol': $('#txtCantidadAlcohol').val(),
        'exConsumidoraSelectAlcohol': $('#exConsumidoraSelectAlcohol').val(),
        'txtTiempoAbstinenciaAlcohol': $('#txtTiempoAbstinenciaAlcohol').val(),
        'txtOtraSustancia': $('#txtOtraSustancia').val(),
        'txtotraSelect': $('#otraSelect').val(),
        'txtTiempoconsumoOtra': $('#txtTiempoconsumoOtra').val(),
        //'txtCantidadOtra': $('#txtCantidadOtra').val(),
        'exConsumidorSelectOtra': $('#exConsumidorSelectOtra').val(),
        'txtTiempoAbstinenciaOtra': $('#txtTiempoAbstinenciaOtra').val(),
        'txtHbtsIncidentes': $('#txtHbtsIncidentes').val(),

        // Estilo de vida
        'txtActividadFisicaSelect': $('#txtActividadFisicaSelect').val() || '',
        'txtCualActividad1': $('#txtCualActividad1').val() || '',
        'txtFrecuenciaActividad1': $('#txtFrecuenciaActividad1').val() || '',

        'txtCualActividad2': $('#txtCualActividad2').val() || '',
        'txtFrecuenciaActividad2': $('#txtFrecuenciaActividad2').val() || '',

        'txtCualActividad3': $('#txtCualActividad3').val() || '',
        'txtFrecuenciaActividad3': $('#txtFrecuenciaActividad3').val() || '',


        'txtMedicacionHabSelect': $('#MedicacionHabSelect').val(),
        'txtCualMedicamento1': $('#txtCualMedicamento1').val(),
        'txtCantdidadMed1': $('#txtCantdidadMed1').val(),

        'txtCualMedicamento2': $('#txtCualMedicamento2').val() || '',
        'txtCantdidadMed2': $('#txtCantdidadMed2').val() || '',

        'txtCualMedicamento3': $('#txtCualMedicamento3').val() || '',
        'txtCantdidadMed3': $('#txtCantdidadMed3').val() || '',

        // Enfermedad actual y constantes
        'txtEnfermedadActual': $('#txtEnfermedadActual').val(),
        'txtConstPresionArterial': $('#txtConstPresionArterial').val(),
        'txtConstTemperatura': $('#txtConstTemperatura').val(),
        'txtConstFrecuenciaCardicaca': $('#txtConstFrecuenciaCardicaca').val(),
        'txtConstSaturacionOxigeno': $('#txtConstSaturacionOxigeno').val(),
        'txtConstFrecuenciaRespiratoria': $('#txtConstFrecuenciaRespiratoria').val(),
        'txtConstPeso': $('#txtConstPeso').val(),
        'txtConstTalla': $('#txtConstTalla').val(),
        'txtConstMasaCorporal': $('#txtConstMasaCorporal').val(),
        'txtConstPerimetroAbdominal': $('#txtConstPerimetroAbdominal').val(),

        // Examen fisico - checkboxes (HTML id sin prefijo txt)
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

        // Factores de riesgo puesto 1-7
        'txtFisicoSelect1': $('#txtFisicoSelect1').val(),
        'txtMecanicoSelect1': $('#txtMecanicoSelect1').val(),
        'txtQuimicoSelect1': $('#txtQuimicoSelect1').val(),
        'txtBiologicoSelect1': $('#txtBiologicoSelect1').val(),
        'txtErgonomicoSelect1': $('#txtErgonomicoSelect1').val(),
        'txtPSicosocialSelect1': $('#txtPSicosocialSelect1').val(),
        'txtMedidadPreventivaA1': $('#txtMedidadPreventivaA1').val(),
        'txtMedidadPreventivaB1': $('#txtMedidadPreventivaB1').val(),
        'txtMedidadPreventivaC1': $('#txtMedidadPreventivaC1').val(),

        'txtFisicoSelect2': $('#txtFisicoSelect2').val(),
        'txtMecanicoSelect2': $('#txtMecanicoSelect2').val(),
        'txtQuimicoSelect2': $('#txtQuimicoSelect2').val(),
        'txtBiologicoSelect2': $('#txtBiologicoSelect2').val(),
        'txtErgonomicoSelect2': $('#txtErgonomicoSelect2').val(),
        'txtPSicosocialSelect2': $('#txtPSicosocialSelect2').val(),
        'txtMedidadPreventivaA2': $('#txtMedidadPreventivaA2').val(),
        'txtMedidadPreventivaB2': $('#txtMedidadPreventivaB2').val(),
        'txtMedidadPreventivaC2': $('#txtMedidadPreventivaC2').val(),

        'txtFisicoSelect3': $('#txtFisicoSelect3').val(),
        'txtMecanicoSelect3': $('#txtMecanicoSelect3').val(),
        'txtQuimicoSelect3': $('#txtQuimicoSelect3').val(),
        'txtBiologicoSelect3': $('#txtBiologicoSelect3').val(),
        'txtErgonomicoSelect3': $('#txtErgonomicoSelect3').val(),
        'txtPSicosocialSelect3': $('#txtPSicosocialSelect3').val(),
        'txtMedidadPreventivaA3': $('#txtMedidadPreventivaA3').val(),
        'txtMedidadPreventivaB3': $('#txtMedidadPreventivaB3').val(),
        'txtMedidadPreventivaC3': $('#txtMedidadPreventivaC3').val(),

        'txtFisicoSelect4': $('#txtFisicoSelect4').val(),
        'txtMecanicoSelect4': $('#txtMecanicoSelect4').val(),
        'txtQuimicoSelect4': $('#txtQuimicoSelect4').val(),
        'txtBiologicoSelect4': $('#txtBiologicoSelect4').val(),
        'txtErgonomicoSelect4': $('#txtErgonomicoSelect4').val(),
        'txtPSicosocialSelect4': $('#txtPSicosocialSelect4').val(),
        'txtMedidadPreventivaA4': $('#txtMedidadPreventivaA4').val(),
        'txtMedidadPreventivaB4': $('#txtMedidadPreventivaB4').val(),
        'txtMedidadPreventivaC4': $('#txtMedidadPreventivaC4').val(),

        'txtFisicoSelect5': $('#txtFisicoSelect5').val(),
        'txtMecanicoSelect5': $('#txtMecanicoSelect5').val(),
        'txtQuimicoSelect5': $('#txtQuimicoSelect5').val(),
        'txtBiologicoSelect5': $('#txtBiologicoSelect5').val(),
        'txtErgonomicoSelect5': $('#txtErgonomicoSelect5').val(),
        'txtPSicosocialSelect5': $('#txtPSicosocialSelect5').val(),
        'txtMedidadPreventivaA5': $('#txtMedidadPreventivaA5').val(),
        'txtMedidadPreventivaB5': $('#txtMedidadPreventivaB5').val(),
        'txtMedidadPreventivaC5': $('#txtMedidadPreventivaC5').val(),

        'txtFisicoSelect6': $('#txtFisicoSelect6').val(),
        'txtMecanicoSelect6': $('#txtMecanicoSelect6').val(),
        'txtQuimicoSelect6': $('#txtQuimicoSelect6').val(),
        'txtBiologicoSelect6': $('#txtBiologicoSelect6').val(),
        'txtErgonomicoSelect6': $('#txtErgonomicoSelect6').val(),
        'txtPSicosocialSelect6': $('#txtPSicosocialSelect6').val(),
        'txtMedidadPreventivaA6': $('#txtMedidadPreventivaA6').val(),
        'txtMedidadPreventivaB6': $('#txtMedidadPreventivaB6').val(),
        'txtMedidadPreventivaC6': $('#txtMedidadPreventivaC6').val(),

        'txtFisicoSelect7': $('#txtFisicoSelect7').val(),
        'txtMecanicoSelect7': $('#txtMecanicoSelect7').val(),
        'txtQuimicoSelect7': $('#txtQuimicoSelect7').val(),
        'txtBiologicoSelect7': $('#txtBiologicoSelect7').val(),
        'txtErgonomicoSelect7': $('#txtErgonomicoSelect7').val(),
        'txtPSicosocialSelect7': $('#txtPSicosocialSelect7').val(),
        'txtMedidadPreventivaA7': $('#txtMedidadPreventivaA7').val(),
        'txtMedidadPreventivaB7': $('#txtMedidadPreventivaB7').val(),
        'txtMedidadPreventivaC7': $('#txtMedidadPreventivaC7').val(),


        // Examenes realizados
        'txtNomExamen1': $('#txtNomExamen1').val(),
        'fechaExam1': $('#fechaExam1').val(),
        'txtResExamen1': $('#txtResExamen1').val(),
        'txtNomExamen2': $('#txtNomExamen2').val(),
        'fechaExam2': $('#fechaExam2').val(),
        'txtResExamen2': $('#txtResExamen2').val(),
        'txtNomExamen3': $('#txtNomExamen3').val(),
        'fechaExam3': $('#fechaExam3').val(),
        'txtResExamen3': $('#txtResExamen3').val(),
        'txtNomExamen4': $('#txtNomExamen4').val(),
        'fechaExam4': $('#fechaExam4').val(),
        'txtResExamen4': $('#txtResExamen4').val(),
        'txtNomExamen5': $('#txtNomExamen5').val(),
        'fechaExam5': $('#fechaExam5').val(),
        'txtResExamen5': $('#txtResExamen5').val(),
        'txtExamenbservacion': $('#txtExamenbservacion').val(),

        'txtDescActividadExtraLab': $('#txtDescActividadExtraLab').val(),
        'fechaActExtraLab': $('#fechaActExtraLab').val(),

        

        // Diagnosticos
        'txtDiagDescripcion1': $('#txtDiagDescripcion1').val(),
        'txtDiagCIE1': $('#txtDiagCIE1').val(),
        'txtDiagnositicoSelect1': $('#txtDiagnositicoSelect1').val(),
        'txtDiagDescripcion2': $('#txtDiagDescripcion2').val(),
        'txtDiagCIE2': $('#txtDiagCIE2').val(),
        'txtDiagnositicoSelect2': $('#txtDiagnositicoSelect2').val(),
        'txtDiagDescripcion3': $('#txtDiagDescripcion3').val(),
        'txtDiagCIE3': $('#txtDiagCIE3').val(),
        'txtDiagnositicoSelect3': $('#txtDiagnositicoSelect3').val(),
        'txtDiagDescripcion4': $('#txtDiagDescripcion4').val(),
        'txtDiagCIE4': $('#txtDiagCIE4').val(),
        'txtDiagnositicoSelect4': $('#txtDiagnositicoSelect4').val(),
        'txtDiagDescripcion5': $('#txtDiagDescripcion5').val(),
        'txtDiagCIE5': $('#txtDiagCIE5').val(),
        'txtDiagnositicoSelect5': $('#txtDiagnositicoSelect5').val(),
        'txtDiagDescripcion6': $('#txtDiagDescripcion6').val(),
        'txtDiagCIE6': $('#txtDiagCIE6').val(),
        'txtDiagnositicoSelect6': $('#txtDiagnositicoSelect6').val(),


        // Aptitud
        'txtAptitudSelect': $('#txtAptitudSelect').val(),
        'txtDescObservacion': $('#txtDescObservacion').val(),
        'txtDescLimitacion': $('#txtDescLimitacion').val(),
        'txtRecomendacion': $('#txtRecomendacion').val(),

        // Fechas formulario
        'txtfechaFormulario': $('#fechaFormulario').val(),
        'txthoraFormulario': $('#horaFormulario').val(),
        'nombre': $("#ContentPlaceHolder1_txtLoginUsuario").val(),

        // Actividad laboral - filas dinamicas
        'actLabRows': (function () {
            var rows = [];
            for (var i = 1; i <= contadorActLab; i++) {
                rows.push({
                    centro: $('#txtCentroTrabajo' + i).val(),
                    actividad: $('#txtActividadDesempeñaba' + i).val(),
                    trabajo: $('#txtActividadDesempeñada' + i).val(),
                    tiempo: $('#txtTiemporTrabajo' + i).val(),
                    accEnf: $('#selectAccEnfTrabajo' + i).val(),
                    iess: $('#AccTrab-si' + i).hasClass('active') ? 'SI' : ($('#AccTrab-no' + i).hasClass('active') ? 'NO' : ''),
                    fecha: $('#fechaAccTrabLab' + i).val(),
                    especificar: $('#txtCualActividadLab' + i).val(),
                    observacion: $('#txtObservacionActividadLab' + i).val()
                });
            }
            return JSON.stringify(rows);
        })()

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
                window.open(respuesta.mensaje);
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