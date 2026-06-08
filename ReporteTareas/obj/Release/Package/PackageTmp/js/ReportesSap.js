var txtTabla = "";
var txtTipoTablaSelect = "";
var table1;

var buttons = document.querySelectorAll(".btn-custom"); // Obtener todos los botones

function MensajeIncorrecto(resultado) {
    Swal.fire({
        icon: "error",
        title: "Oops...",
        text: '¡Algo salió mal...!   ' + resultado
    });
}

function MensajeCorrecto(resultado) {
    Swal.fire({
        type: 'success',
        title: 'Éxito',
        text: '' + resultado,
        width: '600px',
        height: '250px'
    });
}

function alerta(respuesta) {
    Swal.fire({
        title: "Advertencia...!!!",
        text: "Este es un mensaje de advertencia..." + respuesta,
        icon: "info",
        width: '600px',
        height: '250px'
    });
}

function msgTimerPositivo() {
    const Toast = Swal.mixin({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.onmouseenter = Swal.stopTimer;
            toast.onmouseleave = Swal.resumeTimer;
        }
    });
    Toast.fire({
        icon: "success",
        title: "Signed in successfully"
    });
}
function msgTimerNegativo(respuesta) {
    const Toast = Swal.mixin({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 3500,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.onmouseenter = Swal.stopTimer;
            toast.onmouseleave = Swal.resumeTimer;
        },
        width: 500, // Ajusta el ancho del mensaje
        padding: '2rem', // Ajusta el padding del mensaje
        fontSize: '20px', // Ajusta el tamaño de la letra
        font: 'Arial', // Ajusta la fuente del mensaje
        customClass: { // Ajusta la clase personalizada para el mensaje
            popup: 'swal2-popup',
            header: 'swal2-header',
            title: 'swal2-title',
            closeButton: 'swal2-close',
            icon: 'swal2-icon',
            image: 'swal2-image',
            content: 'swal2-content',
            input: 'swal2-input',
            actions: 'swal2-actions',
            confirmButton: 'swal2-confirm',
            cancelButton: 'swal2-cancel',
            footer: 'swal2-footer'
        }
    });
    Toast.fire({
        icon: "error",
        title: "Advertencia: " + respuesta
    });
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



/*============================================================================
 *           Función para manejar la selección de opción Si / NO 
 *============================================================================*/
function toggleButtonColor(buttonId) {
    // Obtiene el elemento del botón según su ID
    var button = document.getElementById(buttonId);

    // Define variables para almacenar texto (inicialmente vacío)
    txtTabla = "";

    // Variable para almacenar el ID del otro botón (para alternar entre "Si" y "No")
    var otherButtonId;

    // Comprueba el ID del botón y establece el texto correspondiente
    if (buttonId === 'completo') {
        otherButtonId = 'sumarizado';
        txtTabla = "completo";
    } else if (buttonId === 'sumarizado') {
        otherButtonId = 'completo';
        txtTabla = "sumarizado";
    }
    // Obtiene el elemento del otro botón
    var otherButton = document.getElementById(otherButtonId);

    // Si el otro botón tiene la clase "active", la elimina y restablece el color de fondo
    if (otherButton.classList.contains("active")) {
        otherButton.classList.remove("active");
        otherButton.style.backgroundColor = ""; // Restablecer el color de fondo
        otherButton.style.color = ""; // Restablecer el color del texto
    }

    // Si el botón actual no tiene la clase "active", la agrega y cambia el color de fondo y texto
    if (!button.classList.contains("active")) {
        button.classList.add("active");
        button.style.color = "white"; // Cambiar el color del texto a blanco cuando está activo
        button.style.backgroundColor = "black"; // Cambiar el color de fondo (opcional)
    } else {
        // Si el botón ya estaba activo, lo desactiva
        button.classList.remove("active");
        button.style.color = ""; // Restablecer el color del texto
        button.style.backgroundColor = ""; // Restablecer el color de fondo
    }

}

//Funcion para activar las tablas segun se seleccionen
$(document).ready(function () {
    var txtTipoTablaSelect = "";

    // Listen for click event on the buttons
    $('button[name="tipoTabla"]').click(function () {
        txtTipoTablaSelect = $(this).attr('id');

        if (txtTipoTablaSelect === "completo") {
            document.getElementById("tblProveedores").style.display = "block";
            document.getElementById("tblProveedoresRes").style.display = "none";
        } else {
            document.getElementById("tblProveedores").style.display = "none";
            document.getElementById("tblProveedoresRes").style.display = "block";
        }
    });
    // Seleccionar el primer botón por defecto y mostrar la primera tabla
    toggleButtonColor('completo');

});
$('#msgDatosSumarizados').on('shown.bs.modal', function () {
    // Seleccionar el primer botón por defecto y mostrar la primera tabla
    toggleButtonColor('completo');
});



function ObtenerListaProv(fechaIni, FechaFin, Factura, Cliente,tipo2,tipo) {
    var Datos = "[{ \"action\": \"ReporteFacturasProveedor\", \"parameters\" : { fechaIni : \"" + fechaIni + "\", FechaFin: \"" + FechaFin + "\", Factura: \"" + Factura + "\", Cliente: \"" + Cliente + "\", tipo : \"" + tipo + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}

function ObtenerListaProv2(fechaIni, FechaFin, Factura, Cliente, tipo2,tipo) {
    var Datos = "[{ \"action\": \"ReporteFacturasProveedor\", \"parameters\" : { fechaIni : \"" + fechaIni + "\", FechaFin: \"" + FechaFin + "\", tipo : \"" + tipo + "\", Factura: \"" + Factura + "\", Cliente: \"" + Cliente + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusquedaResumen", tipo2);
}
function ObtenerListaProv3(fechaIni, FechaFin, Factura, Cliente, tipo2,tipo) {
    var Datos = "[{ \"action\": \"ReporteFacturasProveedor\", \"parameters\" : { fechaIni : \"" + fechaIni + "\", FechaFin: \"" + FechaFin + "\", Factura: \"" + Factura + "\", Cliente: \"" + Cliente + "\", tipo: \"" + tipo + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusquedaSuma", tipo2);
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
    if (tipoControl == "tableSelectBusquedaResumen") {
        contenido = RecorreJSONTableSelectBusquedaResumen(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectBusquedaSuma") {
        contenido = RecorreJSONTableSelectBusquedaSuma(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    return contenido;
}

function RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado) {

    dtProveedores(json);

    // Ocultar el spinner
    $("#divSpinner").hide();
}
function RecorreJSONTableSelectBusquedaResumen(json, boton, idSeleccionado) {

    dtProveedoresResumen(json);
}
function RecorreJSONTableSelectBusquedaSuma(json, boton, idSeleccionado) {
    var mensajeVerificacion = "";
    var contadorVerificacion = 0;

    if (Object.keys(json).length === 0) {
        mensajeVerificacion += "No hay datos para mostrar ";
        contadorVerificacion += 1;
        if (contadorVerificacion > 0) {
            msgTimerNegativo(mensajeVerificacion);        
        }
    } else {
        $('#msgDatosSumarizados').modal('show');
        dtProveedoresSuma(json);
    }
    return;
}




function Create() {
    if ($.fn.DataTable.isDataTable('#tbl_Proveedores')) {
        $('#tbl_Proveedores').DataTable().destroy();
    }
    $('#tbl_Proveedores tbody').empty();
    //Here call the Datatable Bind function;
}
function CreateRes() {
    if ($.fn.DataTable.isDataTable('#tbl_ProveedoresRes')) {
        $('#tbl_ProveedoresRes').DataTable().destroy();
    }
    $('#tbl_ProveedoresRes tbody').empty();
    //Here call the Datatable Bind function;
}
function CreateSum() {
    if ($.fn.DataTable.isDataTable('#tbl_ProveedoresSum')) {
        $('#tbl_ProveedoresSum').DataTable().destroy();
    }
    $('#tbl_ProveedoresSum tbody').empty();
    //Here call the Datatable Bind function;
}


function dtProveedores(json) {
    //<button type='button' value='Actualizar' title='Editar' class='btn btn btn-editMenu btn-xs'><i class='fa fa-edit' aria-hidden='true'></i></button>  
    Create();
    table1 = null;

    table1 = $('#tbl_Proveedores').DataTable({
        data: json,
        columns: [
            { data: 'ID' },
            { data: 'Nombre' },
            { data: 'Valor' },
            { data: 'Abono' },
            { data: 'Saldo' },
            { data: 'Fecha1' },
            { data: 'Factura' },
            { data: 'Descripcion' },
            { data: 'Otro' },
            { defaultContent: "<button type='button' value='Actualizar' title='Editar' class='btn btn-editSum btn-xs'><i class='glyphicon glyphicon-hourglass' aria-hidden='true'></i></button>" }
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
            //"oAria": {
            //    "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
            //    "sSortDescending": ": Activar para ordenar la columna de manera descendente"
            //}
        },
        orderCellsTop: true,
        fixedHeader: true
    });

}

$(document).on('click', '.btn-editSum', function (e) {
    e.preventDefault();
    var data = table1.row($(this).parents('tr')).data();
    MensajeDatosSumarizados(data);
});
function MensajeDatosSumarizados(data) {
    
    var DatoFactura = data.Factura;
    //let cedulaBu = document.getElementById("txtEmpleado").value;
    ObtenerListaProv3("", "", DatoFactura, "", "",1);
}


function dtProveedoresResumen(json) {
    
    CreateRes();
    table2 = null;

    table2 = $('#tbl_ProveedoresRes').DataTable({
        data: json,
        columns: [
            { data: 'Nombre' },
            { data: 'Valor' },
            { data: 'Abono' },
            { data: 'Saldo' }
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
            //"oAria": {
            //    "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
            //    "sSortDescending": ": Activar para ordenar la columna de manera descendente"
            //}
        },
        orderCellsTop: true,
        fixedHeader: true
    });

}


function dtProveedoresSuma(json) {

    CreateSum();
    table3 = null;

    table3 = $('#tbl_ProveedoresSum').DataTable({
        data: json,
        columns: [
            { data: 'Valor' },
            { data: 'Fecha1' },
            { data: 'Descripcion' }
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
            //"oAria": {
            //    "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
            //    "sSortDescending": ": Activar para ordenar la columna de manera descendente"
            //}
        },
        orderCellsTop: true,
        fixedHeader: true
    });

}


function BuscarProv() {
    var fecIni = document.getElementById("txtFechaInicio").value;
    var fecFin = document.getElementById("txtFechaFin").value;
    var txtFac = document.getElementById("txtFactura").value;
    var txtCli = document.getElementById("txtProveedor").value;

    // Mostrar el spinner
    $("#divSpinner").show();

    ObtenerListaProv(fecIni, fecFin, txtFac, txtCli, "", 0);

    ObtenerListaProv2("","",txtFac, txtCli, "",2);
}
