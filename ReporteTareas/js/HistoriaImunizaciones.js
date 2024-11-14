//  ***   Mostrar boton Cargar informacion   ***
// Obtener el elemento del botón
var btnBuscar = document.getElementById("btnBuscarDatosPersonales");
var btnCargar = document.getElementById("btnCargarDatosInm");
var buttons = document.querySelectorAll(".btn-custom"); // Obtener todos los botones
let contadorMedicacion = 0;
let IdPerfil = 0;
var table1;
var nomArchivo = "";
var operacion = "0";

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
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

// INGRESAR SOLO MAYUSCOLAS EN EL INGRESO DEL NOMBRE
function convertirAMayusculas(input) {
    input.value = input.value.toUpperCase();
}

function VerListaInms() {
    document.getElementById("cuadroInm").style.display = "none";
    document.getElementById("cuadroDatosPer").style.display = "block";
    document.getElementById("btnCarga").style.display = "none";
    document.getElementById("cuadroListaForms").style.display = "block";
}
function VerNuevaEditInms() {
    document.getElementById("cuadroInm").style.display = "block";
    document.getElementById("cuadroDatosPer").style.display = "block";
    document.getElementById("btnCarga").style.display = "flex";
    document.getElementById("cuadroListaForms").style.display = "none";
}

/*===============================================================================
 *                  Obtener la cedula de la pagina principal
 *==============================================================================*/
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
 * =================================================================================*/
// Definir la función para buscar un empleado
function BuscarEmpleado(cedula) {
    //let cedulaBuscar = document.getElementById("txtEmpleado").value;

    let cedulaBuscar = cedula;

    ObtenerListaEmpleados(cedulaBuscar, "", "");
    //document.getElementById("btnCarga").style.display = "flex";
}
function ObtenerListaEmpleados(cedula, descripcion, tipo2) {

    var Datos = "[{ \"action\": \"BuscarEmpleadoPorCedula\", \"parameters\" : { tipo : \"" + "" + "\", descripcion: \"" + "" + "\", session: \"" + cedula + "\"} }]";

    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}


// Generamos la informacionde los campos y de las celdas donde se encuentran dentro del Excel
function listaInmunizaciones(nombreArchivo) {

    nomArchivo = nombreArchivo;

    // Preparamos los parámetros para enviar
    //var nombreArchivo = "INMUNIZACIONES.xlsx"; // Cambia esto según sea necesario
    var nombreHoja = "Form 083 Registro Inmunizacione"; // Cambia esto según sea necesario
    var campos = [
        { nombre: "institucion", celda: "A3" },
        { nombre: "ruc", celda: "I3" },
        { nombre: "numClinica", celda: "W3" },
        { nombre: "sexo", celda: "S5" },
        { nombre: "nombre", celda: "K5" },

        { nombre: "tetanosFec11", celda: "H10" },
        { nombre: "tetanosLote21", celda: "J10" },
        { nombre: "tetanosEsquema31", celda: "L10" },
        { nombre: "tetanosNombres41", celda: "N10" },
        { nombre: "tetanosEstab51", celda: "R10" },
        { nombre: "tetanosObs61", celda: "W10" },

        { nombre: "tetanosFec12", celda: "H11" },
        { nombre: "tetanosLote22", celda: "J11" },
        { nombre: "tetanosEsquema32", celda: "L11" },
        { nombre: "tetanosNombres42", celda: "N11" },
        { nombre: "tetanosEstab52", celda: "R11" },
        { nombre: "tetanosObs62", celda: "W11" },

        { nombre: "tetanosFec13", celda: "H12" },
        { nombre: "tetanosLote23", celda: "J12" },
        { nombre: "tetanosEsquema33", celda: "L12" },
        { nombre: "tetanosNombres43", celda: "N12" },
        { nombre: "tetanosEstab53", celda: "R12" },
        { nombre: "tetanosObs63", celda: "W12" },

        { nombre: "tetanosFec14", celda: "H13" },
        { nombre: "tetanosLote24", celda: "J13" },
        { nombre: "tetanosEsquema34", celda: "L13" },
        { nombre: "tetanosNombres44", celda: "N13" },
        { nombre: "tetanosEstab54", celda: "R13" },
        { nombre: "tetanosObs64", celda: "W13" },

        { nombre: "tetanosFec15", celda: "H14" },
        { nombre: "tetanosLote25", celda: "J14" },
        { nombre: "tetanosEsquema35", celda: "L14" },
        { nombre: "tetanosNombres45", celda: "N14" },
        { nombre: "tetanosEstab55", celda: "R14" },
        { nombre: "tetanosObs65", celda: "W14" },

        // Datos de hepatitis
        { nombre: "hepatitisAFec11", celda: "H15" },
        { nombre: "hepatitisALote21", celda: "J15" },
        { nombre: "hepatitisAEsquema31", celda: "L15" },
        { nombre: "hepatitisANombres41", celda: "N15" },
        { nombre: "hepatitisAEstab51", celda: "R15" },
        { nombre: "hepatitisAObs61", celda: "W15" },

        { nombre: "hepatitisAFec12", celda: "H16" },
        { nombre: "hepatitisALote22", celda: "J16" },
        { nombre: "hepatitisAEsquema32", celda: "L16" },
        { nombre: "hepatitisANombres42", celda: "N16" },
        { nombre: "hepatitisAEstab52", celda: "R16" },
        { nombre: "hepatitisAObs62", celda: "W16" },

        { nombre: "hepatitisAFec13", celda: "H17" },
        { nombre: "hepatitisALote23", celda: "J17" },
        { nombre: "hepatitisAEsquema33", celda: "L17" },
        { nombre: "hepatitisANombres43", celda: "N17" },
        { nombre: "hepatitisAEstab53", celda: "R17" },
        { nombre: "hepatitisAObs63", celda: "W17" },

        { nombre: "hepatitisBFec11", celda: "H18" },
        { nombre: "hepatitisBLote21", celda: "J18" },
        { nombre: "hepatitisBEsquema31", celda: "L18" },
        { nombre: "hepatitisBNombres41", celda: "N18" },
        { nombre: "hepatitisBEstab51", celda: "R18" },
        { nombre: "hepatitisBObs61", celda: "W18" },

        { nombre: "hepatitisBFec12", celda: "H19" },
        { nombre: "hepatitisBLote22", celda: "J19" },
        { nombre: "hepatitisBEsquema32", celda: "L19" },
        { nombre: "hepatitisBNombres42", celda: "N19" },
        { nombre: "hepatitisBEstab52", celda: "R19" },
        { nombre: "hepatitisBObs62", celda: "W19" },

        { nombre: "hepatitisBFec13", celda: "H20" },
        { nombre: "hepatitisBLote23", celda: "J20" },
        { nombre: "hepatitisBEsquema33", celda: "L20" },
        { nombre: "hepatitisBNombres43", celda: "N20" },
        { nombre: "hepatitisBEstab53", celda: "R20" },
        { nombre: "hepatitisBObs63", celda: "W20" },

        { nombre: "influenzaFec", celda: "H21" },
        { nombre: "influenzaLote", celda: "J21" },
        { nombre: "influenzaEsquema", celda: "L21" },
        { nombre: "influenzaNombres", celda: "N21" },
        { nombre: "influenzaEstab", celda: "R21" },
        { nombre: "influenzaObs", celda: "W21" },

        { nombre: "fiebreFec", celda: "H22" },
        { nombre: "fiebreLote", celda: "J22" },
        { nombre: "fiebreEsquema", celda: "L22" },
        { nombre: "fiebreNombres", celda: "N22" },
        { nombre: "fiebreEstab", celda: "R22" },
        { nombre: "fiebreObs", celda: "W22" },

        { nombre: "sarampionFec11", celda: "H23" },
        { nombre: "sarampionLote21", celda: "J23" },
        { nombre: "sarampionEsquema31", celda: "L23" },
        { nombre: "sarampionNombres41", celda: "N23" },
        { nombre: "sarampionEstab51", celda: "R23" },
        { nombre: "sarampionObs61", celda: "W23" },

        { nombre: "sarampionFec12", celda: "H24" },
        { nombre: "sarampionLote22", celda: "J24" },
        { nombre: "sarampionEsquema32", celda: "L24" },
        { nombre: "sarampionNombres42", celda: "N24" },
        { nombre: "sarampionEstab52", celda: "R24" },
        { nombre: "sarampionObs62", celda: "W24" },

        { nombre: "inm1ExtraTit", celda: "A26" },

        { nombre: "inm1ExtraFec11", celda: "H26" },
        { nombre: "inm1ExtraLote21", celda: "J26" },
        { nombre: "inm1ExtraEsquema31", celda: "L26" },
        { nombre: "inm1ExtraNombres41", celda: "N26" },
        { nombre: "inm1ExtraEstab51", celda: "R26" },
        { nombre: "inm1ExtraObs61", celda: "W26" },

        { nombre: "inm1ExtraFec12", celda: "H27" },
        { nombre: "inm1ExtraLote22", celda: "J27" },
        { nombre: "inm1ExtraEsquema32", celda: "L27" },
        { nombre: "inm1ExtraNombres42", celda: "N27" },
        { nombre: "inm1ExtraEstab52", celda: "R27" },
        { nombre: "inm1ExtraObs62", celda: "W27" },

        { nombre: "inm1ExtraFec13", celda: "H28" },
        { nombre: "inm1ExtraLote23", celda: "J28" },
        { nombre: "inm1ExtraEsquema33", celda: "L28" },
        { nombre: "inm1ExtraNombres43", celda: "N28" },
        { nombre: "inm1ExtraEstab53", celda: "R28" },
        { nombre: "inm1ExtraObs63", celda: "W28" },

        { nombre: "inm1ExtraFec14", celda: "H29" },
        { nombre: "inm1ExtraLote24", celda: "J29" },
        { nombre: "inm1ExtraEsquema34", celda: "L29" },
        { nombre: "inm1ExtraNombres44", celda: "N29" },
        { nombre: "inm1ExtraEstab54", celda: "R29" },
        { nombre: "inm1ExtraObs64", celda: "W29" },

        { nombre: "inm1ExtraFec15", celda: "H30" },
        { nombre: "inm1ExtraLote25", celda: "J30" },
        { nombre: "inm1ExtraEsquema35", celda: "L30" },
        { nombre: "inm1ExtraNombres45", celda: "N30" },
        { nombre: "inm1ExtraEstab55", celda: "R30" },
        { nombre: "inm1ExtraObs65", celda: "W30" },

        { nombre: "inm2ExtraTit", celda: "A31" },

        { nombre: "inm2ExtraFec11", celda: "H31" },
        { nombre: "inm2ExtraLote21", celda: "J31" },
        { nombre: "inm2ExtraEsquema31", celda: "L31" },
        { nombre: "inm2ExtraNombres41", celda: "N31" },
        { nombre: "inm2ExtraEstab51", celda: "R31" },
        { nombre: "inm2ExtraObs61", celda: "W31" },

        { nombre: "inm2ExtraFec12", celda: "H32" },
        { nombre: "inm2ExtraLote22", celda: "J32" },
        { nombre: "inm2ExtraEsquema32", celda: "L32" },
        { nombre: "inm2ExtraNombres42", celda: "N32" },
        { nombre: "inm2ExtraEstab52", celda: "R32" },
        { nombre: "inm2ExtraObs62", celda: "W32" },

        { nombre: "inm2ExtraFec13", celda: "H33" },
        { nombre: "inm2ExtraLote23", celda: "J33" },
        { nombre: "inm2ExtraEsquema33", celda: "L33" },
        { nombre: "inm2ExtraNombres43", celda: "N33" },
        { nombre: "inm2ExtraEstab53", celda: "R33" },
        { nombre: "inm2ExtraObs63", celda: "W33" },

        { nombre: "inm2ExtraFec14", celda: "H34" },
        { nombre: "inm2ExtraLote24", celda: "J34" },
        { nombre: "inm2ExtraEsquema34", celda: "L34" },
        { nombre: "inm2ExtraNombres44", celda: "N34" },
        { nombre: "inm2ExtraEstab54", celda: "R34" },
        { nombre: "inm2ExtraObs64", celda: "W34" },

        { nombre: "inm2ExtraFec15", celda: "H35" },
        { nombre: "inm2ExtraLote25", celda: "J35" },
        { nombre: "inm2ExtraEsquema35", celda: "L35" },
        { nombre: "inm2ExtraNombres45", celda: "N35" },
        { nombre: "inm2ExtraEstab55", celda: "R35" },
        { nombre: "inm2ExtraObs65", celda: "W35" },

        { nombre: "inm3ExtraTit", celda: "A36" },

        { nombre: "inm3ExtraFec11", celda: "H36" },
        { nombre: "inm3ExtraLote21", celda: "J36" },
        { nombre: "inm3ExtraEsquema31", celda: "L36" },
        { nombre: "inm3ExtraNombres41", celda: "N36" },
        { nombre: "inm3ExtraEstab51", celda: "R36" },
        { nombre: "inm3ExtraObs61", celda: "W36" },

        { nombre: "inm3ExtraFec12", celda: "H37" },
        { nombre: "inm3ExtraLote22", celda: "J37" },
        { nombre: "inm3ExtraEsquema32", celda: "L37" },
        { nombre: "inm3ExtraNombres42", celda: "N37" },
        { nombre: "inm3ExtraEstab52", celda: "R37" },
        { nombre: "inm3ExtraObs62", celda: "W37" },

        { nombre: "inm3ExtraFec13", celda: "H38" },
        { nombre: "inm3ExtraLote23", celda: "J38" },
        { nombre: "inm3ExtraEsquema33", celda: "L38" },
        { nombre: "inm3ExtraNombres43", celda: "N38" },
        { nombre: "inm3ExtraEstab53", celda: "R38" },
        { nombre: "inm3ExtraObs63", celda: "W38" },

        { nombre: "inm3ExtraFec14", celda: "H39" },
        { nombre: "inm3ExtraLote24", celda: "J39" },
        { nombre: "inm3ExtraEsquema34", celda: "L39" },
        { nombre: "inm3ExtraNombres44", celda: "N39" },
        { nombre: "inm3ExtraEstab54", celda: "R39" },
        { nombre: "inm3ExtraObs64", celda: "W39" },

        { nombre: "inm3ExtraFec15", celda: "H40" },
        { nombre: "inm3ExtraLote25", celda: "J40" },
        { nombre: "inm3ExtraEsquema35", celda: "L40" },
        { nombre: "inm3ExtraNombres45", celda: "N40" },
        { nombre: "inm3ExtraEstab55", celda: "R40" },
        { nombre: "inm3ExtraObs65", celda: "W40" },

        { nombre: "inm4ExtraTit", celda: "A41" },

        { nombre: "inm4ExtraFec11", celda: "H41" },
        { nombre: "inm4ExtraLote21", celda: "J41" },
        { nombre: "inm4ExtraEsquema31", celda: "L41" },
        { nombre: "inm4ExtraNombres41", celda: "N41" },
        { nombre: "inm4ExtraEstab51", celda: "R41" },
        { nombre: "inm4ExtraObs61", celda: "W41" },

        { nombre: "inm4ExtraFec12", celda: "H42" },
        { nombre: "inm4ExtraLote22", celda: "J42" },
        { nombre: "inm4ExtraEsquema32", celda: "L42" },
        { nombre: "inm4ExtraNombres42", celda: "N42" },
        { nombre: "inm4ExtraEstab52", celda: "R42" },
        { nombre: "inm4ExtraObs62", celda: "W42" },

        { nombre: "inm4ExtraFec13", celda: "H43" },
        { nombre: "inm4ExtraLote23", celda: "J43" },
        { nombre: "inm4ExtraEsquema33", celda: "L43" },
        { nombre: "inm4ExtraNombres43", celda: "N43" },
        { nombre: "inm4ExtraEstab53", celda: "R43" },
        { nombre: "inm4ExtraObs63", celda: "W43" },

        { nombre: "inm4ExtraFec14", celda: "H44" },
        { nombre: "inm4ExtraLote24", celda: "J44" },
        { nombre: "inm4ExtraEsquema34", celda: "L44" },
        { nombre: "inm4ExtraNombres44", celda: "N44" },
        { nombre: "inm4ExtraEstab54", celda: "R44" },
        { nombre: "inm4ExtraObs64", celda: "W44" },

        { nombre: "inm4ExtraFec15", celda: "H45" },
        { nombre: "inm4ExtraLote25", celda: "J45" },
        { nombre: "inm4ExtraEsquema35", celda: "L45" },
        { nombre: "inm4ExtraNombres45", celda: "N45" },
        { nombre: "inm4ExtraEstab55", celda: "R45" },
        { nombre: "inm4ExtraObs65", celda: "W45" }

    ];



    // Llamada a la función ObtenerDatosHistoria
    ObtenerDatosHistoria(nombreArchivo, nombreHoja, campos, "");
}

// Obtenemos el nombre del Template y el nombre de la hoja
function ObtenerDatosHistoria(nombreArchivo, nombreHoja, campos, tipo2) {
    // Asegurarse de que los datos sean serializados correctamente.
    var Datos = JSON.stringify([{
        action: "InformacionMedica",
        parameters: {
            nombreArchivo: nombreArchivo,
            nombre: $('#txtNombre').val(),
            nombreHoja: nombreHoja,
            campos: campos, // No hace falta convertirlo a string manualmente
            session: "" // Debes verificar de dónde proviene `cedula`, lo dejé vacío
        }
    }]);

    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectDatos", tipo2);
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
    if (tipoControl === "tableSelectDatos") {
        contenido = RecorreJSONtableSelectDatos(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectFormularios") {
        contenido = RecorreJSONTableSelectFormulario(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    return contenido;
}
function RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado) {

    dtEmpleados(json);
}
function RecorreJSONtableSelectDatos(json, boton, idSeleccionado) {
    dtInmunizacion(json);
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

        VerFormularios();
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

/* ==========================================================================================
 *   Seccion para verificar si existe un formulario de inmunizaciones o se creara uno nuevo
 * ==========================================================================================*/
function VerFormularios() {
    Tipo = "INMUNIZACIONES";
    Nombre = $('#txtNombre').val();
    Sociedad = $('#txtSociedad').val();
    AreaTrabajo = $('#txtAreaTrabajo').val();
    fecha1 = "2018-02-01";

    // Obtener la fecha actual
    let fechaActual = new Date();

    // Formatear la fecha como YYYY-MM-DD
    let año = fechaActual.getFullYear();
    let mes = String(fechaActual.getMonth() + 1).padStart(2, '0'); // Los meses empiezan en 0, por eso sumamos 1
    let dia = String(fechaActual.getDate()).padStart(2, '0');
    fecha2 = `${año}-${mes}-${dia}`; // Formato final de la fecha actual

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
            { defaultContent: "<a title='Editar' style='color:black' class='btn btn-info btn-xs btn-editarInm'><i class='glyphicon glyphicon-pencil' aria-hidden='true'></i>  Continuar</a>" }
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

    // Ocultar el spinner
    $("#divSpinner").hide();
}


/*==========================================================================
 *   Funcion para cargar los datos de la inmunizacion al front HTML
 *=========================================================================*/
function dtInmunizacion(json) {
    // Verifica si json es un objeto
    if (typeof json === 'object') {

        let Cedula = json.ruc;
        //document.getElementById("txtCedula").value = Cedula;
        BuscarEmpleado(Cedula);
        //let Cedula = json.ruc;
        //document.getElementById("txtCedula").value = Cedula;
        //let Nombre = json.nombre;
        //document.getElementById("txtNombre").value = Nombre;
        //let Sociedad = json.numClinica;
        //document.getElementById("txtSociedad").value = Sociedad;
        //let Sexo = json.sexo;
        //document.getElementById("txtSexo").value = Sexo;
        //let PuestoTrabajo = json.institucion;
        //document.getElementById("txtPuestoTrabajo").value = PuestoTrabajo;

        // TETANOS

        let tetanosFec11 = json.tetanosFec11;
        document.getElementById("fechaTetanos1").value = tetanosFec11;
        let tetanosLote21 = json.tetanosLote21;
        document.getElementById("txtTetanosLote1").value = tetanosLote21;
        if (json.tetanosEsquema31 === 'X') {
            document.getElementById("SelectTetanosEsquema1").value = 'SI';
        } else {
            document.getElementById("SelectTetanosEsquema1").value = ''; // Selecciona "No"
        }
        let tetanosNombres41 = json.tetanosNombres41;
        document.getElementById("txtTetanosNombre1").value = tetanosNombres41;
        let tetanosEstab51 = json.tetanosEstab51;
        document.getElementById("txtTetanosEstablecimiento1").value = tetanosEstab51;
        let tetanosObs61 = json.tetanosObs61;
        document.getElementById("txtTetanosObs1").value = tetanosObs61;

        let tetanosFec12 = json.tetanosFec12;
        document.getElementById("fechaTetanos2").value = tetanosFec12;
        let tetanosLote22 = json.tetanosLote22;
        document.getElementById("txtTetanosLote2").value = tetanosLote22;
        if (json.tetanosEsquema32 === 'X') {
            document.getElementById("SelectTetanosEsquema2").value = 'SI';
        } else {
            document.getElementById("SelectTetanosEsquema2").value = ''; // Selecciona "No"
        }
        let tetanosNombres42 = json.tetanosNombres42;
        document.getElementById("txtTetanosNombre2").value = tetanosNombres42;
        let tetanosEstab52 = json.tetanosEstab52;
        document.getElementById("txtTetanosEstablecimiento2").value = tetanosEstab52;
        let tetanosObs62 = json.tetanosObs62;
        document.getElementById("txtTetanosObs2").value = tetanosObs62;

        let tetanosFec13 = json.tetanosFec13;
        document.getElementById("fechaTetanos3").value = tetanosFec13;
        let tetanosLote23 = json.tetanosLote23;
        document.getElementById("txtTetanosLote3").value = tetanosLote23;
        if (json.tetanosEsquema33 === 'X') {
            document.getElementById("SelectTetanosEsquema3").value = 'SI';
        } else {
            document.getElementById("SelectTetanosEsquema3").value = ''; // Selecciona "No"
        }
        let tetanosNombres43 = json.tetanosNombres43;
        document.getElementById("txtTetanosNombre3").value = tetanosNombres43;
        let tetanosEstab53 = json.tetanosEstab53;
        document.getElementById("txtTetanosEstablecimiento3").value = tetanosEstab53;
        let tetanosObs63 = json.tetanosObs63;
        document.getElementById("txtTetanosObs3").value = tetanosObs63;

        let tetanosFec14 = json.tetanosFec14;
        document.getElementById("fechaTetanos4").value = tetanosFec14;
        let tetanosLote24 = json.tetanosLote24;
        document.getElementById("txtTetanosLote4").value = tetanosLote24;
        if (json.tetanosEsquema34 === 'X') {
            document.getElementById("SelectTetanosEsquema4").value = 'SI';
        } else {
            document.getElementById("SelectTetanosEsquema4").value = ''; // Selecciona "No"
        }
        let tetanosNombres44 = json.tetanosNombres44;
        document.getElementById("txtTetanosNombre4").value = tetanosNombres44;
        let tetanosEstab54 = json.tetanosEstab54;
        document.getElementById("txtTetanosEstablecimiento4").value = tetanosEstab54;
        let tetanosObs64 = json.tetanosObs64;
        document.getElementById("txtTetanosObs4").value = tetanosObs64;

        let tetanosFec15 = json.tetanosFec15;
        document.getElementById("fechaTetanos5").value = tetanosFec15;
        let tetanosLote25 = json.tetanosLote25;
        document.getElementById("txtTetanosLote5").value = tetanosLote25;
        if (json.tetanosEsquema35 === 'X') {
            document.getElementById("SelectTetanosEsquema5").value = 'SI';
        } else {
            document.getElementById("SelectTetanosEsquema5").value = ''; // Selecciona "No"
        }
        let tetanosNombres45 = json.tetanosNombres45;
        document.getElementById("txtTetanosNombre5").value = tetanosNombres45;
        let tetanosEstab55 = json.tetanosEstab55;
        document.getElementById("txtTetanosEstablecimiento5").value = tetanosEstab55;
        let tetanosObs65 = json.tetanosObs65;
        document.getElementById("txtTetanosObs5").value = tetanosObs65;

        // HEPATITIS A

        let hepatitisAFec11 = json.hepatitisAFec11;
        document.getElementById("fechaHepatitisA1").value = hepatitisAFec11;
        let hepatitisALote21 = json.hepatitisALote21;
        document.getElementById("txtHepatitisALote1").value = hepatitisALote21;
        if (json.hepatitisAEsquema31 === 'X') {
            document.getElementById("SelectHepatitisAEsquema1").value = 'SI';
        } else {
            document.getElementById("SelectHepatitisAEsquema1").value = ''; // Selecciona "No"
        }
        let hepatitisANombres41 = json.hepatitisANombres41;
        document.getElementById("txtHepatitisANombre1").value = hepatitisANombres41;
        let hepatitisAEstab51 = json.hepatitisAEstab51;
        document.getElementById("txtHepatitisAEstablecimiento1").value = hepatitisAEstab51;
        let hepatitisAObs61 = json.hepatitisAObs61;
        document.getElementById("txtHepatitisAObs1").value = hepatitisAObs61;

        let hepatitisAFec12 = json.hepatitisAFec12;
        document.getElementById("fechaHepatitisA2").value = hepatitisAFec12;
        let hepatitisALote22 = json.hepatitisALote22;
        document.getElementById("txtHepatitisALote2").value = hepatitisALote22;
        if (json.hepatitisAEsquema32 === 'X') {
            document.getElementById("SelectHepatitisAEsquema2").value = 'SI';
        } else {
            document.getElementById("SelectHepatitisAEsquema2").value = ''; // Selecciona "No"
        }
        let hepatitisANombres42 = json.hepatitisANombres42;
        document.getElementById("txtHepatitisANombre2").value = hepatitisANombres42;
        let hepatitisAEstab52 = json.hepatitisAEstab52;
        document.getElementById("txtHepatitisAEstablecimiento2").value = hepatitisAEstab52;
        let hepatitisAObs62 = json.hepatitisAObs62;
        document.getElementById("txtHepatitisAObs2").value = hepatitisAObs62;

        let hepatitisAFec13 = json.hepatitisAFec13;
        document.getElementById("fechaHepatitisA3").value = hepatitisAFec13;
        let hepatitisALote23 = json.hepatitisALote23;
        document.getElementById("txtHepatitisALote3").value = hepatitisALote23;
        if (json.hepatitisAEsquema33 === 'X') {
            document.getElementById("SelectHepatitisAEsquema3").value = 'SI';
        } else {
            document.getElementById("SelectHepatitisAEsquema3").value = ''; // Selecciona "No"
        }
        let hepatitisANombres43 = json.hepatitisANombres43;
        document.getElementById("txtHepatitisANombre3").value = hepatitisANombres43;
        let hepatitisAEstab53 = json.hepatitisAEstab53;
        document.getElementById("txtHepatitisAEstablecimiento3").value = hepatitisAEstab53;
        let hepatitisAObs63 = json.hepatitisAObs63;
        document.getElementById("txtHepatitisAObs3").value = hepatitisAObs63;

        // Hepatitis B

        let hepatitisBFec11 = json.hepatitisBFec11;
        document.getElementById("fechaHepatitisB1").value = hepatitisBFec11;
        let hepatitisBLote21 = json.hepatitisBLote21;
        document.getElementById("txtHepatitisBLote1").value = hepatitisBLote21;
        if (json.hepatitisBEsquema31 === 'X') {
            document.getElementById("SelectHepatitisBEsquema1").value = 'SI';
        } else {
            document.getElementById("SelectHepatitisBEsquema1").value = ''; // Selecciona "No"
        }
        let hepatitisBNombres41 = json.hepatitisBNombres41;
        document.getElementById("txtHepatitisBNombre1").value = hepatitisBNombres41;
        let hepatitisBEstab51 = json.hepatitisBEstab51;
        document.getElementById("txtHepatitisBEstablecimiento1").value = hepatitisBEstab51;
        let hepatitisBObs61 = json.hepatitisBObs61;
        document.getElementById("txtHepatitisBObs1").value = hepatitisBObs61;

        let hepatitisBFec12 = json.hepatitisBFec12;
        document.getElementById("fechaHepatitisB2").value = hepatitisBFec12;
        let hepatitisBLote22 = json.hepatitisBLote22;
        document.getElementById("txtHepatitisBLote2").value = hepatitisBLote22;
        if (json.hepatitisBEsquema32 === 'X') {
            document.getElementById("SelectHepatitisBEsquema2").value = 'SI';
        } else {
            document.getElementById("SelectHepatitisBEsquema2").value = ''; // Selecciona "No"
        }
        let hepatitisBNombres42 = json.hepatitisBNombres42;
        document.getElementById("txtHepatitisBNombre2").value = hepatitisBNombres42;
        let hepatitisBEstab52 = json.hepatitisBEstab52;
        document.getElementById("txtHepatitisBEstablecimiento2").value = hepatitisBEstab52;
        let hepatitisBObs62 = json.hepatitisBObs62;
        document.getElementById("txtHepatitisBObs2").value = hepatitisBObs62;

        let hepatitisBFec13 = json.hepatitisBFec13;
        document.getElementById("fechaHepatitisB3").value = hepatitisBFec13;
        let hepatitisBLote23 = json.hepatitisBLote23;
        document.getElementById("txtHepatitisBLote3").value = hepatitisBLote23;
        if (json.hepatitisBEsquema33 === 'X') {
            document.getElementById("SelectHepatitisBEsquema3").value = 'SI';
        } else {
            document.getElementById("SelectHepatitisBEsquema3").value = ''; // Selecciona "No"
        }
        let hepatitisBNombres43 = json.hepatitisBNombres43;
        document.getElementById("txtHepatitisBNombre3").value = hepatitisBNombres43;
        let hepatitisBEstab53 = json.hepatitisBEstab53;
        document.getElementById("txtHepatitisBEstablecimiento3").value = hepatitisBEstab53;
        let hepatitisBObs63 = json.hepatitisBObs63;
        document.getElementById("txtHepatitisBObs3").value = hepatitisBObs63;

        // INFLUENZA

        let influenzaFec = json.influenzaFec;
        document.getElementById("fechaInfluenza1").value = influenzaFec;
        let influenzaLote = json.influenzaLote;
        document.getElementById("txtInfluenzaLote1").value = influenzaLote;
        if (json.influenzaEsquema === 'X') {
            document.getElementById("SelectInfluenzaEsquema1").value = 'SI';
        } else {
            document.getElementById("SelectInfluenzaEsquema1").value = ''; // Selecciona "No"
        }
        let influenzaNombres = json.influenzaNombres;
        document.getElementById("txtInfluenzaNombre1").value = influenzaNombres;
        let influenzaEstab = json.influenzaEstab;
        document.getElementById("txtInfluenzaEstablecimiento1").value = influenzaEstab;
        let influenzaObs = json.influenzaObs;
        document.getElementById("txtInfluenzaObs1").value = influenzaObs;

        // FIEBRE

        let fiebreFec = json.fiebreFec;
        document.getElementById("fechaFiebre1").value = fiebreFec;
        let fiebreLote = json.fiebreLote;
        document.getElementById("txtFiebreLote1").value = fiebreLote;
        if (json.fiebreEsquema === 'X') {
            document.getElementById("SelectFiebreEsquema1").value = 'SI';
        } else {
            document.getElementById("SelectFiebreEsquema1").value = ''; // Selecciona "No"
        }
        let fiebreNombres = json.fiebreNombres;
        document.getElementById("txtFiebreNombre1").value = fiebreNombres;
        let fiebreEstab = json.fiebreEstab;
        document.getElementById("txtFiebreEstablecimiento1").value = fiebreEstab;
        let fiebreObs = json.fiebreObs;
        document.getElementById("txtFiebreObs1").value = fiebreObs;

        // SARAMPION

        let sarampionFec11 = json.sarampionFec11;
        document.getElementById("fechaSarampion1").value = sarampionFec11;
        let sarampionLote21 = json.sarampionLote21;
        document.getElementById("txtSarampionLote1").value = sarampionLote21;
        if (json.sarampionEsquema31 === 'X') {
            document.getElementById("SelectSarampionEsquema1").value = 'SI';
        } else {
            document.getElementById("SelectSarampionEsquema1").value = ''; // Selecciona "No"
        }
        let sarampionNombres41 = json.sarampionNombres41;
        document.getElementById("txtSarampionNombre1").value = sarampionNombres41;
        let sarampionEstab51 = json.sarampionEstab51;
        document.getElementById("txtSarampionEstablecimiento1").value = sarampionEstab51;
        let sarampionObs61 = json.sarampionObs61;
        document.getElementById("txtSarampionObs1").value = sarampionObs61;

        let sarampionFec12 = json.sarampionFec12;
        document.getElementById("fechaSarampion2").value = sarampionFec12;
        let sarampionLote22 = json.sarampionLote22;
        document.getElementById("txtSarampionLote2").value = sarampionLote22;
        if (json.sarampionEsquema32 === 'X') {
            document.getElementById("SelectSarampionEsquema2").value = 'SI';
        } else {
            document.getElementById("SelectSarampionEsquema2").value = ''; // Selecciona "No"
        }
        let sarampionNombres42 = json.sarampionNombres42;
        document.getElementById("txtSarampionNombre2").value = sarampionNombres42;
        let sarampionEstab52 = json.sarampionEstab52;
        document.getElementById("txtSarampionEstablecimiento2").value = sarampionEstab52;
        let sarampionObs62 = json.sarampionObs62;
        document.getElementById("txtSarampionObs2").value = sarampionObs62;

        // INM EXTRAS 1

        let inm1ExtraTit = json.inm1ExtraTit;
        document.getElementById("txtNuevaDosis1").value = inm1ExtraTit;

        let inm1ExtraFec11 = json.inm1ExtraFec11;
        document.getElementById("fechaNuevo1").value = inm1ExtraFec11;
        let inm1ExtraLote21 = json.inm1ExtraLote21;
        document.getElementById("txtNuevoLote1").value = inm1ExtraLote21;
        if (json.inm1ExtraEsquema31 === 'X') {
            document.getElementById("SelectNuevoEsquema1").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema1").value = ''; // Selecciona "No"
        }
        let inm1ExtraNombres41 = json.inm1ExtraNombres41;
        document.getElementById("txtNuevoNombre1").value = inm1ExtraNombres41;
        let inm1ExtraEstab51 = json.inm1ExtraEstab51;
        document.getElementById("txtNuevoEstablecimiento1").value = inm1ExtraEstab51;
        let inm1ExtraObs61 = json.inm1ExtraObs61;
        document.getElementById("txtNuevoObs1").value = inm1ExtraObs61;

        let inm1ExtraFec12 = json.inm1ExtraFec12;
        document.getElementById("fechaNuevo2").value = inm1ExtraFec12;
        let inm1ExtraLote22 = json.inm1ExtraLote22;
        document.getElementById("txtNuevoLote2").value = inm1ExtraLote22;
        if (json.inm1ExtraEsquema32 === 'X') {
            document.getElementById("SelectNuevoEsquema2").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema2").value = ''; // Selecciona "No"
        }
        let inm1ExtraNombres42 = json.inm1ExtraNombres42;
        document.getElementById("txtNuevoNombre2").value = inm1ExtraNombres42;
        let inm1ExtraEstab52 = json.inm1ExtraEstab52;
        document.getElementById("txtNuevoEstablecimiento2").value = inm1ExtraEstab52;
        let inm1ExtraObs62 = json.inm1ExtraObs62;
        document.getElementById("txtNuevoObs2").value = inm1ExtraObs62;

        let inm1ExtraFec13 = json.inm1ExtraFec13;
        document.getElementById("fechaNuevo3").value = inm1ExtraFec13;
        let inm1ExtraLote23 = json.inm1ExtraLote23;
        document.getElementById("txtNuevoLote3").value = inm1ExtraLote23;
        if (json.inm1ExtraEsquema33 === 'X') {
            document.getElementById("SelectNuevoEsquema3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema3").value = ''; // Selecciona "No"
        }
        let inm1ExtraNombres43 = json.inm1ExtraNombres43;
        document.getElementById("txtNuevoNombre3").value = inm1ExtraNombres43;
        let inm1ExtraEstab53 = json.inm1ExtraEstab53;
        document.getElementById("txtNuevoEstablecimiento3").value = inm1ExtraEstab53;
        let inm1ExtraObs63 = json.inm1ExtraObs63;
        document.getElementById("txtNuevoObs3").value = inm1ExtraObs63;

        let inm1ExtraFec14 = json.inm1ExtraFec14;
        document.getElementById("fechaNuevo4").value = inm1ExtraFec14;
        let inm1ExtraLote24 = json.inm1ExtraLote24;
        document.getElementById("txtNuevoLote4").value = inm1ExtraLote24;
        if (json.inm1ExtraEsquema34 === 'X') {
            document.getElementById("SelectNuevoEsquema4").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema4").value = ''; // Selecciona "No"
        }
        let inm1ExtraNombres44 = json.inm1ExtraNombres44;
        document.getElementById("txtNuevoNombre4").value = inm1ExtraNombres44;
        let inm1ExtraEstab54 = json.inm1ExtraEstab54;
        document.getElementById("txtNuevoEstablecimiento4").value = inm1ExtraEstab54;
        let inm1ExtraObs64 = json.inm1ExtraObs64;
        document.getElementById("txtNuevoObs4").value = inm1ExtraObs64;

        let inm1ExtraFec15 = json.inm1ExtraFec15;
        document.getElementById("fechaNuevo5").value = inm1ExtraFec15;
        let inm1ExtraLote25 = json.inm1ExtraLote25;
        document.getElementById("txtNuevoLote5").value = inm1ExtraLote25;
        if (json.inm1ExtraEsquema35 === 'X') {
            document.getElementById("SelectNuevoEsquema5").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema5").value = ''; // Selecciona "No"
        }
        let inm1ExtraNombres45 = json.inm1ExtraNombres45;
        document.getElementById("txtNuevoNombre5").value = inm1ExtraNombres45;
        let inm1ExtraEstab55 = json.inm1ExtraEstab55;
        document.getElementById("txtNuevoEstablecimiento5").value = inm1ExtraEstab55;
        let inm1ExtraObs65 = json.inm1ExtraObs65;
        document.getElementById("txtNuevoObs5").value = inm1ExtraObs65;

        // INM EXTRAS 2
        let inm2ExtraTit = json.inm2ExtraTit;
        document.getElementById("txtNuevaDosis12").value = inm2ExtraTit;

        let inm2ExtraFec11 = json.inm2ExtraFec11;
        document.getElementById("fechaNuevo1Inm2").value = inm2ExtraFec11;
        let inm2ExtraLote21 = json.inm2ExtraLote21;
        document.getElementById("txtNuevoLote1Inm2").value = inm2ExtraLote21;
        if (json.inm2ExtraEsquema31 === 'X') {
            document.getElementById("SelectNuevoEsquema1Inm2").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema1Inm2").value = ''; // Selecciona "No"
        }
        let inm2ExtraNombres41 = json.inm2ExtraNombres41;
        document.getElementById("txtNuevoNombre1Inm2").value = inm2ExtraNombres41;
        let inm2ExtraEstab51 = json.inm2ExtraEstab51;
        document.getElementById("txtNuevoEstablecimiento1Inm2").value = inm2ExtraEstab51;
        let inm2ExtraObs61 = json.inm2ExtraObs61;
        document.getElementById("txtNuevoObs1Inm2").value = inm2ExtraObs61;

        let inm2ExtraFec12 = json.inm2ExtraFec12;
        document.getElementById("fechaNuevo2Inm2").value = inm2ExtraFec12;
        let inm2ExtraLote22 = json.inm2ExtraLote22;
        document.getElementById("txtNuevoLote2Inm2").value = inm2ExtraLote22;
        if (json.inm2ExtraEsquema32 === 'X') {
            document.getElementById("SelectNuevoEsquema2Inm2").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema2Inm2").value = ''; // Selecciona "No"
        }
        let inm2ExtraNombres42 = json.inm2ExtraNombres42;
        document.getElementById("txtNuevoNombre2Inm2").value = inm2ExtraNombres42;
        let inm2ExtraEstab52 = json.inm2ExtraEstab52;
        document.getElementById("txtNuevoEstablecimiento2Inm2").value = inm2ExtraEstab52;
        let inm2ExtraObs62 = json.inm2ExtraObs62;
        document.getElementById("txtNuevoObs2Inm2").value = inm2ExtraObs62;

        let inm2ExtraFec13 = json.inm2ExtraFec13;
        document.getElementById("fechaNuevo3Inm2").value = inm2ExtraFec13;
        let inm2ExtraLote23 = json.inm2ExtraLote23;
        document.getElementById("txtNuevoLote3Inm2").value = inm2ExtraLote23;
        if (json.inm2ExtraEsquema33 === 'X') {
            document.getElementById("SelectNuevoEsquema3Inm2").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema3Inm2").value = ''; // Selecciona "No"
        }
        let inm2ExtraNombres43 = json.inm2ExtraNombres43;
        document.getElementById("txtNuevoNombre3Inm2").value = inm2ExtraNombres43;
        let inm2ExtraEstab53 = json.inm2ExtraEstab53;
        document.getElementById("txtNuevoEstablecimiento3Inm2").value = inm2ExtraEstab53;
        let inm2ExtraObs63 = json.inm2ExtraObs63;
        document.getElementById("txtNuevoObs3Inm2").value = inm2ExtraObs63;

        let inm2ExtraFec14 = json.inm2ExtraFec14;
        document.getElementById("fechaNuevo4Inm2").value = inm2ExtraFec14;
        let inm2ExtraLote24 = json.inm2ExtraLote24;
        document.getElementById("txtNuevoLote4Inm2").value = inm2ExtraLote24;
        if (json.inm2ExtraEsquema34 === 'X') {
            document.getElementById("SelectNuevoEsquema4Inm2").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema4Inm2").value = ''; // Selecciona "No"
        }
        let inm2ExtraNombres44 = json.inm2ExtraNombres44;
        document.getElementById("txtNuevoNombre4Inm2").value = inm2ExtraNombres44;
        let inm2ExtraEstab54 = json.inm2ExtraEstab54;
        document.getElementById("txtNuevoEstablecimiento4Inm2").value = inm2ExtraEstab54;
        let inm2ExtraObs64 = json.inm2ExtraObs64;
        document.getElementById("txtNuevoObs4Inm2").value = inm2ExtraObs64;

        let inm2ExtraFec15 = json.inm2ExtraFec15;
        document.getElementById("fechaNuevo5Inm2").value = inm2ExtraFec15;
        let inm2ExtraLote25 = json.inm2ExtraLote25;
        document.getElementById("txtNuevoLote5Inm2").value = inm2ExtraLote25;
        if (json.inm2ExtraEsquema35 === 'X') {
            document.getElementById("SelectNuevoEsquema5Inm2").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema5Inm2").value = ''; // Selecciona "No"
        }
        let inm2ExtraNombres45 = json.inm2ExtraNombres45;
        document.getElementById("txtNuevoNombre5Inm2").value = inm2ExtraNombres45;
        let inm2ExtraEstab55 = json.inm2ExtraEstab55;
        document.getElementById("txtNuevoEstablecimiento5Inm2").value = inm2ExtraEstab55;
        let inm2ExtraObs65 = json.inm2ExtraObs65;
        document.getElementById("txtNuevoObs5Inm2").value = inm2ExtraObs65;

        // INM EXTRAS 3
        let inm3ExtraTit = json.inm3ExtraTit;
        document.getElementById("txtNuevaDosis13").value = inm3ExtraTit;

        let inm3ExtraFec11 = json.inm3ExtraFec11;
        document.getElementById("fechaNuevo1Inm3").value = inm3ExtraFec11;
        let inm3ExtraLote21 = json.inm3ExtraLote21;
        document.getElementById("txtNuevoLote1Inm3").value = inm3ExtraLote21;
        if (json.inm3ExtraEsquema31 === 'X') {
            document.getElementById("SelectNuevoEsquema1Inm3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema1Inm3").value = ''; // Selecciona "No"
        }
        let inm3ExtraNombres41 = json.inm3ExtraNombres41;
        document.getElementById("txtNuevoNombre1Inm3").value = inm3ExtraNombres41;
        let inm3ExtraEstab51 = json.inm3ExtraEstab51;
        document.getElementById("txtNuevoEstablecimiento1Inm3").value = inm3ExtraEstab51;
        let inm3ExtraObs61 = json.inm3ExtraObs61;
        document.getElementById("txtNuevoObs1Inm3").value = inm3ExtraObs61;

        let inm3ExtraFec12 = json.inm3ExtraFec12;
        document.getElementById("fechaNuevo2Inm3").value = inm3ExtraFec12;
        let inm3ExtraLote22 = json.inm3ExtraLote22;
        document.getElementById("txtNuevoLote2Inm3").value = inm3ExtraLote22;
        if (json.inm3ExtraEsquema32 === 'X') {
            document.getElementById("SelectNuevoEsquema2Inm3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema2Inm3").value = ''; // Selecciona "No"
        }
        let inm3ExtraNombres42 = json.inm3ExtraNombres42;
        document.getElementById("txtNuevoNombre2Inm3").value = inm3ExtraNombres42;
        let inm3ExtraEstab52 = json.inm3ExtraEstab52;
        document.getElementById("txtNuevoEstablecimiento2Inm3").value = inm3ExtraEstab52;
        let inm3ExtraObs62 = json.inm3ExtraObs62;
        document.getElementById("txtNuevoObs2Inm3").value = inm3ExtraObs62;

        let inm3ExtraFec13 = json.inm3ExtraFec13;
        document.getElementById("fechaNuevo3Inm3").value = inm3ExtraFec13;
        let inm3ExtraLote23 = json.inm3ExtraLote23;
        document.getElementById("txtNuevoLote3Inm3").value = inm3ExtraLote23;
        if (json.inm3ExtraEsquema33 === 'X') {
            document.getElementById("SelectNuevoEsquema3Inm3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema3Inm3").value = ''; // Selecciona "No"
        }
        let inm3ExtraNombres43 = json.inm3ExtraNombres43;
        document.getElementById("txtNuevoNombre3Inm3").value = inm3ExtraNombres43;
        let inm3ExtraEstab53 = json.inm3ExtraEstab53;
        document.getElementById("txtNuevoEstablecimiento3Inm3").value = inm3ExtraEstab53;
        let inm3ExtraObs63 = json.inm3ExtraObs63;
        document.getElementById("txtNuevoObs3Inm3").value = inm3ExtraObs63;

        let inm3ExtraFec14 = json.inm3ExtraFec14;
        document.getElementById("fechaNuevo4Inm3").value = inm3ExtraFec14;
        let inm3ExtraLote24 = json.inm3ExtraLote24;
        document.getElementById("txtNuevoLote4Inm3").value = inm3ExtraLote24;
        if (json.inm3ExtraEsquema34 === 'X') {
            document.getElementById("SelectNuevoEsquema4Inm3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema4Inm3").value = ''; // Selecciona "No"
        }
        let inm3ExtraNombres44 = json.inm3ExtraNombres44;
        document.getElementById("txtNuevoNombre4Inm3").value = inm3ExtraNombres44;
        let inm3ExtraEstab54 = json.inm3ExtraEstab54;
        document.getElementById("txtNuevoEstablecimiento4Inm3").value = inm3ExtraEstab54;
        let inm3ExtraObs64 = json.inm3ExtraObs64;
        document.getElementById("txtNuevoObs4Inm3").value = inm3ExtraObs64;

        let inm3ExtraFec15 = json.inm3ExtraFec15;
        document.getElementById("fechaNuevo5Inm3").value = inm3ExtraFec15;
        let inm3ExtraLote25 = json.inm3ExtraLote25;
        document.getElementById("txtNuevoLote5Inm3").value = inm3ExtraLote25;
        if (json.inm3ExtraEsquema35 === 'X') {
            document.getElementById("SelectNuevoEsquema5Inm3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema5Inm3").value = ''; // Selecciona "No"
        }
        let inm3ExtraNombres45 = json.inm3ExtraNombres45;
        document.getElementById("txtNuevoNombre5Inm3").value = inm3ExtraNombres45;
        let inm3ExtraEstab55 = json.inm3ExtraEstab55;
        document.getElementById("txtNuevoEstablecimiento5Inm3").value = inm3ExtraEstab55;
        let inm3ExtraObs65 = json.inm3ExtraObs65;
        document.getElementById("txtNuevoObs5Inm3").value = inm3ExtraObs65;

        // INM EXTRAS 4
        let inm4ExtraTit = json.inm4ExtraTit;
        document.getElementById("txtNuevaDosis123").value = inm4ExtraTit;

        let inm4ExtraFec11 = json.inm4ExtraFec11;
        document.getElementById("fechaNuevo1Inm2Inm3").value = inm4ExtraFec11;
        let inm4ExtraLote21 = json.inm4ExtraLote21;
        document.getElementById("txtNuevoLote1Inm2Inm3").value = inm4ExtraLote21;
        if (json.inm4ExtraEsquema31 === 'X') {
            document.getElementById("SelectNuevoEsquema1Inm2Inm3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema1Inm2Inm3").value = ''; // Selecciona "No"
        }
        let inm4ExtraNombres41 = json.inm4ExtraNombres41;
        document.getElementById("txtNuevoNombre1Inm2Inm3").value = inm4ExtraNombres41;
        let inm4ExtraEstab51 = json.inm4ExtraEstab51;
        document.getElementById("txtNuevoEstablecimiento1Inm2Inm3").value = inm4ExtraEstab51;
        let inm4ExtraObs61 = json.inm4ExtraObs61;
        document.getElementById("txtNuevoObs1Inm2Inm3").value = inm4ExtraObs61;

        let inm4ExtraFec12 = json.inm4ExtraFec12;
        document.getElementById("fechaNuevo2Inm2Inm3").value = inm4ExtraFec12;
        let inm4ExtraLote22 = json.inm4ExtraLote22;
        document.getElementById("txtNuevoLote2Inm2Inm3").value = inm4ExtraLote22;
        if (json.inm4ExtraEsquema32 === 'X') {
            document.getElementById("SelectNuevoEsquema2Inm2Inm3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema2Inm2Inm3").value = ''; // Selecciona "No"
        }
        let inm4ExtraNombres42 = json.inm4ExtraNombres42;
        document.getElementById("txtNuevoNombre2Inm2Inm3").value = inm4ExtraNombres42;
        let inm4ExtraEstab52 = json.inm4ExtraEstab52;
        document.getElementById("txtNuevoEstablecimiento2Inm2Inm3").value = inm4ExtraEstab52;
        let inm4ExtraObs62 = json.inm4ExtraObs62;
        document.getElementById("txtNuevoObs2Inm2Inm3").value = inm4ExtraObs62;

        let inm4ExtraFec13 = json.inm4ExtraFec13;
        document.getElementById("fechaNuevo3Inm2Inm3").value = inm4ExtraFec13;
        let inm4ExtraLote23 = json.inm4ExtraLote23;
        document.getElementById("txtNuevoLote3Inm2Inm3").value = inm4ExtraLote23;
        if (json.inm4ExtraEsquema33 === 'X') {
            document.getElementById("SelectNuevoEsquema3Inm2Inm3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema3Inm2Inm3").value = ''; // Selecciona "No"
        }
        let inm4ExtraNombres43 = json.inm4ExtraNombres43;
        document.getElementById("txtNuevoNombre3Inm2Inm3").value = inm4ExtraNombres43;
        let inm4ExtraEstab53 = json.inm4ExtraEstab53;
        document.getElementById("txtNuevoEstablecimiento3Inm2Inm3").value = inm4ExtraEstab53;
        let inm4ExtraObs63 = json.inm4ExtraObs63;
        document.getElementById("txtNuevoObs3Inm2Inm3").value = inm4ExtraObs63;

        let inm4ExtraFec14 = json.inm4ExtraFec14;
        document.getElementById("fechaNuevo4Inm2Inm3").value = inm4ExtraFec14;
        let inm4ExtraLote24 = json.inm4ExtraLote24;
        document.getElementById("txtNuevoLote4Inm2Inm3").value = inm4ExtraLote24;
        if (json.inm4ExtraEsquema34 === 'X') {
            document.getElementById("SelectNuevoEsquema4Inm2Inm3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema4Inm2Inm3").value = ''; // Selecciona "No"
        }
        let inm4ExtraNombres44 = json.inm4ExtraNombres44;
        document.getElementById("txtNuevoNombre4Inm2Inm3").value = inm4ExtraNombres44;
        let inm4ExtraEstab54 = json.inm4ExtraEstab54;
        document.getElementById("txtNuevoEstablecimiento4Inm2Inm3").value = inm4ExtraEstab54;
        let inm4ExtraObs64 = json.inm4ExtraObs64;
        document.getElementById("txtNuevoObs4Inm2Inm3").value = inm4ExtraObs64;

        let inm4ExtraFec15 = json.inm4ExtraFec15;
        document.getElementById("fechaNuevo5Inm2Inm3").value = inm4ExtraFec15;
        let inm4ExtraLote25 = json.inm4ExtraLote25;
        document.getElementById("txtNuevoLote5Inm2Inm3").value = inm4ExtraLote25;
        if (json.inm4ExtraEsquema35 === 'X') {
            document.getElementById("SelectNuevoEsquema5Inm2Inm3").value = 'SI';
        } else {
            document.getElementById("SelectNuevoEsquema5Inm2Inm3").value = ''; // Selecciona "No"
        }
        let inm4ExtraNombres45 = json.inm4ExtraNombres45;
        document.getElementById("txtNuevoNombre5Inm2Inm3").value = inm4ExtraNombres45;
        let inm4ExtraEstab55 = json.inm4ExtraEstab55;
        document.getElementById("txtNuevoEstablecimiento5Inm2Inm3").value = inm4ExtraEstab55;
        let inm4ExtraObs65 = json.inm4ExtraObs65;
        document.getElementById("txtNuevoObs5Inm2Inm3").value = inm4ExtraObs65;



    } else {
        console.error("json no es un objeto válido.");
    }
}

// Función para enviar los datos de una nueva Inmunizacion al servidor y luego al Template
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
        'txtNumHistoria': $('#txtNumHistoria').val(),
        'txtNumArchivo': $('#txtNumArchivo').val(),
        'txtNombre': $('#txtNombre').val(),
        'txtEdad': $('#txtEdad').val(),
        'txtSexo': $('#txtSexo').val(),
        'txtAreaTrabajo': $('#txtAreaTrabajo').val(),
        'txtAntecedentesPersonales': "",
        'txtAntecedentesFamiliares': "",
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


        // INM EXTRAS 1
        'txtNuevaDosis1': $('#txtNuevaDosis1').val() ?? "",

        'fechaNuevo1': $('#fechaNuevo1').val() ?? "",
        'txtNuevoLote1': $('#txtNuevoLote1').val() ?? "",
        'txtSelectNuevoEsquema1': $('#SelectNuevoEsquema1').val() ?? "",
        'txtNuevoNombre1': $('#txtNuevoNombre1').val() ?? "",
        'txtNuevoEstablecimiento1': $('#txtNuevoEstablecimiento1').val() ?? "",
        'txtNuevoObs1': $('#txtNuevoObs1').val() ?? "",

        'fechaNuevo2': $('#fechaNuevo2').val() ?? "",
        'txtNuevoLote2': $('#txtNuevoLote2').val() ?? "",
        'txtSelectNuevoEsquema2': $('#SelectNuevoEsquema2').val() ?? "",
        'txtNuevoNombre2': $('#txtNuevoNombre2').val() ?? "",
        'txtNuevoEstablecimiento2': $('#txtNuevoEstablecimiento2').val() ?? "",
        'txtNuevoObs2': $('#txtNuevoObs2').val() ?? "",

        'fechaNuevo3': $('#fechaNuevo3').val() ?? "",
        'txtNuevoLote3': $('#txtNuevoLote3').val() ?? "",
        'txtSelectNuevoEsquema3': $('#SelectNuevoEsquema3').val() ?? "",
        'txtNuevoNombre3': $('#txtNuevoNombre3').val() ?? "",
        'txtNuevoEstablecimiento3': $('#txtNuevoEstablecimiento3').val() ?? "",
        'txtNuevoObs3': $('#txtNuevoObs3').val() ?? "",

        'fechaNuevo4': $('#fechaNuevo4').val() ?? "",
        'txtNuevoLote4': $('#txtNuevoLote4').val() ?? "",
        'txtSelectNuevoEsquema4': $('#SelectNuevoEsquema4').val() ?? "",
        'txtNuevoNombre4': $('#txtNuevoNombre4').val() ?? "",
        'txtNuevoEstablecimiento4': $('#txtNuevoEstablecimiento4').val() ?? "",
        'txtNuevoObs4': $('#txtNuevoObs4').val() ?? "",

        'fechaNuevo5': $('#fechaNuevo5').val() ?? "",
        'txtNuevoLote5': $('#txtNuevoLote5').val() ?? "",
        'txtSelectNuevoEsquema5': $('#SelectNuevoEsquema5').val() ?? "",
        'txtNuevoNombre5': $('#txtNuevoNombre5').val() ?? "",
        'txtNuevoEstablecimiento5': $('#txtNuevoEstablecimiento5').val() ?? "",
        'txtNuevoObs5': $('#txtNuevoObs5').val() ?? "",

        // INM EXTRAS 2
        'txtNuevaDosis12': $('#txtNuevaDosis12').val() ?? "",

        'fechaNuevo1Inm2': $('#fechaNuevo1Inm2').val() ?? "",
        'txtNuevoLote1Inm2': $('#txtNuevoLote1Inm2').val() ?? "",
        'txtSelectNuevoEsquema1Inm2': $('#SelectNuevoEsquema1Inm2').val() ?? "",
        'txtNuevoNombre1Inm2': $('#txtNuevoNombre1Inm2').val() ?? "",
        'txtNuevoEstablecimiento1Inm2': $('#txtNuevoEstablecimiento1Inm2').val() ?? "",
        'txtNuevoObs1Inm2': $('#txtNuevoObs1Inm2').val() ?? "",

        'fechaNuevo2Inm2': $('#fechaNuevo2Inm2').val() ?? "",
        'txtNuevoLote2Inm2': $('#txtNuevoLote2Inm2').val() ?? "",
        'txtSelectNuevoEsquema2Inm2': $('#SelectNuevoEsquema2Inm2').val() ?? "",
        'txtNuevoNombre2Inm2': $('#txtNuevoNombre2Inm2').val() ?? "",
        'txtNuevoEstablecimiento2Inm2': $('#txtNuevoEstablecimiento2Inm2').val() ?? "",
        'txtNuevoObs2Inm2': $('#txtNuevoObs2Inm2').val() ?? "",

        'fechaNuevo3Inm2': $('#fechaNuevo3Inm2').val() ?? "",
        'txtNuevoLote3Inm2': $('#txtNuevoLote3Inm2').val() ?? "",
        'txtSelectNuevoEsquema3Inm2': $('#SelectNuevoEsquema3Inm2').val() ?? "",
        'txtNuevoNombre3Inm2': $('#txtNuevoNombre3Inm2').val() ?? "",
        'txtNuevoEstablecimiento3Inm2': $('#txtNuevoEstablecimiento3Inm2').val() ?? "",
        'txtNuevoObs3Inm2': $('#txtNuevoObs3Inm2').val() ?? "",

        'fechaNuevo4Inm2': $('#fechaNuevo4Inm2').val() ?? "",
        'txtNuevoLote4Inm2': $('#txtNuevoLote4Inm2').val() ?? "",
        'txtSelectNuevoEsquema4Inm2': $('#SelectNuevoEsquema4Inm2').val() ?? "",
        'txtNuevoNombre4Inm2': $('#txtNuevoNombre4Inm2').val() ?? "",
        'txtNuevoEstablecimiento4Inm2': $('#txtNuevoEstablecimiento4Inm2').val() ?? "",
        'txtNuevoObs4Inm2': $('#txtNuevoObs4Inm2').val() ?? "",

        'fechaNuevo5Inm2': $('#fechaNuevo5Inm2').val() ?? "",
        'txtNuevoLote5Inm2': $('#txtNuevoLote5Inm2').val() ?? "",
        'txtSelectNuevoEsquema5Inm2': $('#SelectNuevoEsquema5Inm2').val() ?? "",
        'txtNuevoNombre5Inm2': $('#txtNuevoNombre5Inm2').val() ?? "",
        'txtNuevoEstablecimiento5Inm2': $('#txtNuevoEstablecimiento5Inm2').val() ?? "",
        'txtNuevoObs5Inm2': $('#txtNuevoObs5Inm2').val() ?? "",

        // INM EXTRAS 3
        'txtNuevaDosis13': $('#txtNuevaDosis13').val() ?? "",

        'fechaNuevo1Inm3': $('#fechaNuevo1Inm3').val() ?? "",
        'txtNuevoLote1Inm3': $('#txtNuevoLote1Inm3').val() ?? "",
        'txtSelectNuevoEsquema1Inm3': $('#SelectNuevoEsquema1Inm3').val() ?? "",
        'txtNuevoNombre1Inm3': $('#txtNuevoNombre1Inm3').val() ?? "",
        'txtNuevoEstablecimiento1Inm3': $('#txtNuevoEstablecimiento1Inm3').val() ?? "",
        'txtNuevoObs1Inm3': $('#txtNuevoObs1Inm3').val() ?? "",

        'fechaNuevo2Inm3': $('#fechaNuevo2Inm3').val() ?? "",
        'txtNuevoLote2Inm3': $('#txtNuevoLote2Inm3').val() ?? "",
        'txtSelectNuevoEsquema2Inm3': $('#SelectNuevoEsquema2Inm3').val() ?? "",
        'txtNuevoNombre2Inm3': $('#txtNuevoNombre2Inm3').val() ?? "",
        'txtNuevoEstablecimiento2Inm3': $('#txtNuevoEstablecimiento2Inm3').val() ?? "",
        'txtNuevoObs2Inm3': $('#txtNuevoObs2Inm3').val() ?? "",

        'fechaNuevo3Inm3': $('#fechaNuevo3Inm3').val() ?? "",
        'txtNuevoLote3Inm3': $('#txtNuevoLote3Inm3').val() ?? "",
        'txtSelectNuevoEsquema3Inm3': $('#SelectNuevoEsquema3Inm3').val() ?? "",
        'txtNuevoNombre3Inm3': $('#txtNuevoNombre3Inm3').val() ?? "",
        'txtNuevoEstablecimiento3Inm3': $('#txtNuevoEstablecimiento3Inm3').val() ?? "",
        'txtNuevoObs3Inm3': $('#txtNuevoObs3Inm3').val() ?? "",

        'fechaNuevo4Inm3': $('#fechaNuevo4Inm3').val() ?? "",
        'txtNuevoLote4Inm3': $('#txtNuevoLote4Inm3').val() ?? "",
        'txtSelectNuevoEsquema4Inm3': $('#SelectNuevoEsquema4Inm3').val() ?? "",
        'txtNuevoNombre4Inm3': $('#txtNuevoNombre4Inm3').val() ?? "",
        'txtNuevoEstablecimiento4Inm3': $('#txtNuevoEstablecimiento4Inm3').val() ?? "",
        'txtNuevoObs4Inm3': $('#txtNuevoObs4Inm3').val() ?? "",

        'fechaNuevo5Inm3': $('#fechaNuevo5Inm3').val() ?? "",
        'txtNuevoLote5Inm3': $('#txtNuevoLote5Inm3').val() ?? "",
        'txtSelectNuevoEsquema5Inm3': $('#SelectNuevoEsquema5Inm3').val() ?? "",
        'txtNuevoNombre5Inm3': $('#txtNuevoNombre5Inm3').val() ?? "",
        'txtNuevoEstablecimiento5Inm3': $('#txtNuevoEstablecimiento5Inm3').val() ?? "",
        'txtNuevoObs5Inm3': $('#txtNuevoObs5Inm3').val() ?? "",

        // INM EXTRAS 4
        'txtNuevaDosis123': $('#txtNuevaDosis123').val() ?? "",

        'fechaNuevo1Inm2Inm3': $('#fechaNuevo1Inm2Inm3').val() ?? "",
        'txtNuevoLote1Inm2Inm3': $('#txtNuevoLote1Inm2Inm3').val() ?? "",
        'txtSelectNuevoEsquema1Inm2Inm3': $('#SelectNuevoEsquema1Inm2Inm3').val() ?? "",
        'txtNuevoNombre1Inm2Inm3': $('#txtNuevoNombre1Inm2Inm3').val() ?? "",
        'txtNuevoEstablecimiento1Inm2Inm3': $('#txtNuevoEstablecimiento1Inm2Inm3').val() ?? "",
        'txtNuevoObs1Inm2Inm3': $('#txtNuevoObs1Inm2Inm3').val() ?? "",

        'fechaNuevo2Inm2Inm3': $('#fechaNuevo2Inm2Inm3').val() ?? "",
        'txtNuevoLote2Inm2Inm3': $('#txtNuevoLote2Inm2Inm3').val() ?? "",
        'txtSelectNuevoEsquema2Inm2Inm3': $('#SelectNuevoEsquema2Inm2Inm3').val() ?? "",
        'txtNuevoNombre2Inm2Inm3': $('#txtNuevoNombre2Inm2Inm3').val() ?? "",
        'txtNuevoEstablecimiento2Inm2Inm3': $('#txtNuevoEstablecimiento2Inm2Inm3').val() ?? "",
        'txtNuevoObs2Inm2Inm3': $('#txtNuevoObs2Inm2Inm3').val() ?? "",

        'fechaNuevo3Inm2Inm3': $('#fechaNuevo3Inm2Inm3').val() ?? "",
        'txtNuevoLote3Inm2Inm3': $('#txtNuevoLote3Inm2Inm3').val() ?? "",
        'txtSelectNuevoEsquema3Inm2Inm3': $('#SelectNuevoEsquema3Inm2Inm3').val() ?? "",
        'txtNuevoNombre3Inm2Inm3': $('#txtNuevoNombre3Inm2Inm3').val() ?? "",
        'txtNuevoEstablecimiento3Inm2Inm3': $('#txtNuevoEstablecimiento3Inm2Inm3').val() ?? "",
        'txtNuevoObs3Inm2Inm3': $('#txtNuevoObs3Inm2Inm3').val() ?? "",

        'fechaNuevo4Inm2Inm3': $('#fechaNuevo4Inm2Inm3').val() ?? "",
        'txtNuevoLote4Inm2Inm3': $('#txtNuevoLote4Inm2Inm3').val() ?? "",
        'txtSelectNuevoEsquema4Inm2Inm3': $('#SelectNuevoEsquema4Inm2Inm3').val() ?? "",
        'txtNuevoNombre4Inm2Inm3': $('#txtNuevoNombre4Inm2Inm3').val() ?? "",
        'txtNuevoEstablecimiento4Inm2Inm3': $('#txtNuevoEstablecimiento4Inm2Inm3').val() ?? "",
        'txtNuevoObs4Inm2Inm3': $('#txtNuevoObs4Inm2Inm3').val() ?? "",

        'fechaNuevo5Inm2Inm3': $('#fechaNuevo5Inm2Inm3').val() ?? "",
        'txtNuevoLote5Inm2Inm3': $('#txtNuevoLote5Inm2Inm3').val() ?? "",
        'txtSelectNuevoEsquema5Inm2Inm3': $('#SelectNuevoEsquema5Inm2Inm3').val() ?? "",
        'txtNuevoNombre5Inm2Inm3': $('#txtNuevoNombre5Inm2Inm3').val() ?? "",
        'txtNuevoEstablecimiento5Inm2Inm3': $('#txtNuevoEstablecimiento5Inm2Inm3').val() ?? "",
        'txtNuevoObs5Inm2Inm3': $('#txtNuevoObs5Inm2Inm3').val() ?? "",
        'archivo': nomArchivo,
        'operacion': operacion,

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
    newInmunizacion.querySelectorAll("[id^='txtNuevaDosis']").forEach(function (element) {
        element.id += newId;
        element.value = "";
    });

    newInmunizacion.querySelectorAll("[id^='fechaNuevo1']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoLote1']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='SelectNuevoEsquema1']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoNombre1']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoEstablecimiento1']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoObs1']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });


    //newInmunizacion.querySelectorAll("[id^='txtNuevaDosis']").forEach(function (element) {
    //    element.id;
    //});
    newInmunizacion.querySelectorAll("[id^='fechaNuevo2']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoLote2']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='SelectNuevoEsquema2']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoNombre2']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoEstablecimiento2']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoObs2']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });


    //newInmunizacion.querySelectorAll("[id^='txtNuevaDosis3']").forEach(function (element) {
    //    element.id;
    //});
    newInmunizacion.querySelectorAll("[id^='fechaNuevo3']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoLote3']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='SelectNuevoEsquema3']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoNombre3']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoEstablecimiento3']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoObs3']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });


    //newInmunizacion.querySelectorAll("[id^='txtNuevaDosis4']").forEach(function (element) {
    //    element.id;
    //});
    newInmunizacion.querySelectorAll("[id^='fechaNuevo4']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoLote4']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='SelectNuevoEsquema4']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoNombre4']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoEstablecimiento4']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoObs4']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });

    newInmunizacion.querySelectorAll("[id^='fechaNuevo5']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoLote5']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='SelectNuevoEsquema5']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoNombre5']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoEstablecimiento5']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
    });
    newInmunizacion.querySelectorAll("[id^='txtNuevoObs5']").forEach(function (element) {
        element.id += "Inm" + newId;
        element.value = "";
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



/*========================================================================
 *  Agregar un evento 'click' al botón para crear una nueva inmunización
 * ======================================================================*/
// Obtener el botón por su ID
$(document).on('click', '#btnNewInmunizacion', function (e) {
    e.preventDefault();
    VerNuevaEditInms();
});

// Función para Editar y continuar un nuevo formulario
$(document).on('click', '.btn-editarInm', function (e) {
    e.preventDefault();
    var data = table1.row($(this).parents('tr')).data();
    VerNuevaEditInms();
    agregarInmunizacion();
    agregarInmunizacion();
    operacion = "1";
    listaInmunizaciones(data.Nombre);
    document.getElementById("btnInm").style.display = "none";
});

/* ============================================
 *          BTN Regresar al Menu Principal
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
    // Mostrar el spinner
    $("#divSpinner").show();

    VerListaInms();

});