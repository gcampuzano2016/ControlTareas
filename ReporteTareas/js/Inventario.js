let idCliente = 0;
let idCliente2 = 0;
let table;
let CantidadSolicitada = 0;

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

function BuscarCliente2() {

    if ($('#cboCliente2').val().length > 2) {
        idSeleccionado = 0;
        ObtenerListaClientes2(2, $('#cboCliente2').val(), 4);
    }
    else {
        idCliente = 0;
        document.getElementById("comboClientes2").style.display = "none";
    }
}

function CargarCliente2(ID, NOMBRE) {
    $('#cboCliente2').val(NOMBRE);
    idCliente = ID;
    document.getElementById("comboClientes2").style.display = "none";
}

function BuscarCliente3() {

    if ($('#cboCliente3').val().length > 2) {
        idSeleccionado = 0;
        ObtenerListaClientes2(2, $('#cboCliente3').val(), 5);
    }
    else {
        idCliente2 = 0;
        document.getElementById("comboClientes3").style.display = "none";
    }
}

function CargarCliente3(ID, NOMBRE) {
    $('#cboCliente3').val(NOMBRE);
    idCliente2 = ID;
    document.getElementById("comboClientes3").style.display = "none";
}

function ObtenerListaClientes2(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}

function ObtenerListaInventario(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaInventario\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}

function ObtenerPedidoCliente(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarClientePedido\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
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

    if (tipoControl == "table") {
        contenido = RecorreJSONTable(json);
        $(div).html(contenido);
    }
    if (tipoControl == "select") {
        contenido = RecorreJSONSelect(div, json, idSeleccionado);
    }
    if (tipoControl == "form") {
        contenido = RecorreJSONForm(div, json, boton);
    }
    if (tipoControl == "tableSelect") {
        contenido = RecorreJSONTableSelect(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectBusqueda") {
        contenido = RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    //---------------------------
    if (tipoControl == "tableSelectEgresoInventario") {
        contenido = RecorreJSONTableSelectEgresoInventario(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectTablaEgresoInventario") {
        contenido = RecorreJSONTableSelectTablaEgresoInventario(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
   
    return contenido;
}

function CargarClientePedido(json) {
    if (json.length > 0) {
        $.each(json, function (i, item) {
            idCliente = item.IdCliente,
                $('#cboCliente2').val(item.CLIENTE);
        });
    }
    else {
        idCliente = 0;
        $('#cboCliente2').val("");
        alerta("El pedido no existe..");
    }
}

function RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado) {

    var x = "";

    if (idSeleccionado == 7) {
        dtProductos(json);
    }
    else if (idSeleccionado == 8) {
        dtDetalleEgreso(json);
    }
    else if (idSeleccionado == 9) {
        CargarClientePedido(json);
    }
    else if (idSeleccionado ==10) {
        ValidarStock(json);
    }
    else if (idSeleccionado == 4) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarCliente2(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboClientes2").innerHTML = x;
        document.getElementById("comboClientes2").style.display = "block";
    }
    else if (idSeleccionado == 5) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarCliente3(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboClientes3").innerHTML = x;
        document.getElementById("comboClientes3").style.display = "block";
    }
        
}

function BuscarProducto() {

    var cells = document.querySelectorAll('#tbl_Productos tr');
    if (cells.length > 1) {
        var tb = document.getElementById('tbl_Productos');
        while (tb.rows.length > 1) {
            tb.deleteRow(1);
        }
    }

    $('#imodalProductos').modal('show');
}

function BuscarProductoLike() {
    ObtenerListaInventario(8, $('#txtDescripcionProducto').val(), 7);
}

function ValidarStock(json) {
    let stock = 0;
    if (json.length > 0) {
        $.each(json, function (i, item) {
            stock = item.Cantidad
        });
        ActualizarCampo();
        if (CantidadSolicitada > stock) {
            alerta("La cantida solicitada es mayor al stock..");
        }

    }
}

function ConsultarStock(CodigoSAP) {
    ObtenerListaInventario(11, CodigoSAP, 10);
}

function ConsultarPedido() {
    if ($('#txtCodigoProceso').val().length == 5) {
        ObtenerPedidoCliente(10, $('#txtCodigoProceso').val(), 9);
    }
    else if ($('#txtCodigoProceso').val()==""){
        idCliente = 0;
        $('#cboCliente2').val("");
    }
}

function Create() {
    if ($.fn.DataTable.isDataTable('#tbl_Productos')) {
        $('#tbl_Productos').DataTable().destroy();
    }
    $('#tbl_Productos tbody').empty();
    //Here call the Datatable Bind function;
}

function dtProductos(json) {
    Create();
    table = null;

    table = $('#tbl_Productos').DataTable({
        data: json,
        columns: [
            { data: 'IdInventario' },
            { data: 'CodigoSAP' },
            { data: 'Descripcion' },
            { data: 'PrecioUnitario' },
            { data: 'Cantidad' },
            { data: 'Ubicacion' },
            { data: 'NumSerie' },
            { defaultContent: "<button type='button' value='Actualizar' title='Seleccionar Items' class='btn btn btn-editClientes btn-xs'><i class='fa fa-eye' aria-hidden='true'></i></button>" },
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

function CreateDetalleEgreso() {
    if ($.fn.DataTable.isDataTable('#tbl_Egreso')) {
        $('#tbl_Egreso').DataTable().destroy();
    }
    $('#tbl_Egreso tbody').empty();
    //Here call the Datatable Bind function;
}

function dtDetalleEgreso(json) {
    CreateDetalleEgreso();
    table = null;

    table = $('#tbl_Egreso').DataTable({
        data: json,
        columns: [
            { data: 'IdEncabezado' },
            { data: 'StrFechaRegistro' },
            { data: 'Nombres' },
            { data: 'Total' },
            { data: 'CodigoProceso' },
            { defaultContent: "<button type='button' value='Actualizar' title='Ver Ride de Egreso' class='btn btn btn-VerEgreso btn-xs'><i class='fa fa-eye' aria-hidden='true'></i></button>" },
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

$(document).on('click', '.btn-VerEgreso', function (e) {
    e.preventDefault();
    var data = table.row($(this).parents('tr')).data();
    VerRide(data.NombreArchivo, data.IdEncabezado);
});

function VerRide(NombreArchivo,id) {
    var htmlListaArchivosAdjuntos = "<table border='1' style='border-collapse: collapse;border: 1px solid #efefef; width:100%'>";
    htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<tr id='tdArchivo_" + NombreArchivo + "'><td>";
    htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<a href='../descargas/" + NombreArchivo + "' target='_blank' style='font-size:12px'>&nbsp;<i class='fa " + NombreArchivo + "'></i>&nbsp;" + NombreArchivo + "</a><br>";
    htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td><td>" + "";
    htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td></tr>";
    htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</table>";
    $("#MensajeInformativo").html(htmlListaArchivosAdjuntos);

    $("#divMensajes").html("");

    $("#modalMensajeInformativoLabel").html('Listado de Archivos');
    BuscarInfoEgresoInventarios(id);
    $("#ModalEgresoInventarioPDF").modal('show');
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

$(document).on('click', '.btn-editClientes', function (e) {
    e.preventDefault();
    var data = table.row($(this).parents('tr')).data();
    CargarItems(data);
    $('#imodalProductos').modal('hide');
});

function CargarItems(data) {

    var tr = `<tr>
                <td style="display:none;">`+ data.IdInventario.toString() + 'à' + data.NumSerie.toString() + 'à' + data.CodigoSAP.toString() + 'à' + data.NumParte.toString() + `</td>
                <td class="text-right"><input type="text" class="form-control" id="txtCantidad" value="1.00" oninput="ConsultarStock(`+ data.CodigoSAP + `)"/></td>
                <td class="text-left"><input type="text" class="form-control" id="txtDescrip" readonly="readonly" value="`+ data.Descripcion + `" /></td>
                <td class="text-center"><input type="text" class="form-control" id="txtPrecio"  value="`+ "0.00" + `" oninput="ActualizarCampo()"/></td>
                <td class="text-center"><input type="text" class="form-control" id="txtTotal"  value="`+ "0.00" + `" /></td>`
    tr = tr + ` <td class="text-center"><a href="#" class="btn-eliminaDetalle" title="Eliminar detalle" data-id=""><i class="fa fa-trash-o text-danger"></i></a></td>`
    tr = tr + `</tr>`;

    $("#tbl_Detalle").append(tr);
    var total = 0;
    var filas = $("#tbl_Detalle").find("tr"); //devulve las filas del body de tu tabla segun el ejemplo que brindaste
    for (i = 1; i < filas.length; i++) { //Recorre las filas 1 a 1
        var celdas = $(filas[i]).find("td"); //devolverá las celdas de una fila
        var cantidad = $($(celdas[1]).children("input")[0]).val();
        var precio = $($(celdas[3]).children("input")[0]).val();

        total += (precio * cantidad)
    }
    document.getElementById('valorTotal').innerHTML = total.toFixed(2);
}

function ActualizarCampo() {
    var total = 0;
    var precio_unitario = 0;
    var cantidad = 0;
    var subtotal = 0;

    $(".table-items tbody tr").each(function (indexFila) {
        $(this).children("td").each(function (indexColumna) {

            switch (indexColumna) {
                case 1:
                    cantidad = $(this).find('input[id="txtCantidad"]').val();
                    CantidadSolicitada = cantidad;
                    break;
                case 3:
                    precio_unitario = $(this).find('input[id="txtPrecio"]').val();
                    break;
                case 4:
                    subtotal = (precio_unitario * cantidad);
                    $(this).find('input[id="txtTotal"]').val(subtotal);
                    break;
            }
           
        });

        total += (precio_unitario * cantidad);

    });

    document.getElementById('valorTotal').innerHTML = total.toFixed(2);
}

function NuevoEgreso() {
    $('#cboCliente2').val("");
    $('#txtObservacion').val("");
    $('#txtCodigoProceso').val("0000");
    document.getElementById('valorTotal').innerHTML = "0.00";
    idCliente = 0;
    var tb = document.getElementById('tbl_Detalle');
    while (tb.rows.length > 1) {
        tb.deleteRow(1);
    }
}

function formatearNumero(nStr) {
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? ',' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + '.' + '$2');
    }
    return x1 + x2;
}

function GuardarNuevoEgreso() {

    var contadorVerificacion = 0;
    var mensajeVerificacion = "";

    if ($('#txtCodigoProceso').val() == "") {
        mensajeVerificacion += "- Debe ingresar el codigo del proceso. ";
        contadorVerificacion += 1;
    }

    //if ($('#cboCliente2').val() == "") {
    //    mensajeVerificacion += "- Debe agregar un cliente. ";
    //    contadorVerificacion += 1;
    //}

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var cells = document.querySelectorAll('#tbl_Detalle tr');
    if (cells.length > 1) {

        var ti = "";
        var re = "↨";
        for (i = 1; i < cells.length; i++) {
            var ri = cells[i].getElementsByTagName('td');
            ti = ti + re;
            for (r = 0; r < ri.length; r++) {
                if (r === 3 || r === 1 || r === 2) {
                    ti = ti + ri[r].children[0].value + ';';
                }
                else if (r === 0){
                    ti = ti + ri[r].innerText + ';';
                }
            }
        }

        var strTotal = document.getElementById('valorTotal').innerHTML;

        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var tipoMensaje = "warning";
        var datosFormulario = "";

        datosFormulario = datosFormulario + "{";
        datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
        datosFormulario = datosFormulario + "'idCliente': '" + idCliente + "',";
        datosFormulario = datosFormulario + "'totalEgreso': '" + formatearNumero(parseFloat(strTotal)) + "',";
        datosFormulario = datosFormulario + "'txtFechaActual': '" + $('#txtFechaActual').val() + "',";
        datosFormulario = datosFormulario + "'CodigoProceso': '" + $('#txtCodigoProceso').val() + "',";
        datosFormulario = datosFormulario + "'detalle': '" + ti + "',";
        datosFormulario = datosFormulario + "'nombreCliente': '" + $('#cboCliente2').val() + "',";
        datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
        datosFormulario = datosFormulario + "'Tipo': '" + "0" + "'";
        datosFormulario = datosFormulario + "}";

        datos = "[{'action': 'GuardarInventario', 'parameters' : " + datosFormulario + " }]";

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
                    NuevoEgreso();

                    //var htmlListaArchivosAdjuntos = "<table border='1' style='border-collapse: collapse;border: 1px solid #efefef; width:100%'>";
                    //htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<tr id='tdArchivo_" + respuesta.Archivo + "'><td>";
                    //htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<a href='../descargas/" + respuesta.Archivo + "' target='_blank' style='font-size:12px'>&nbsp;<i class='fa " + respuesta.Archivo + "'></i>&nbsp;" + respuesta.Archivo + "</a><br>";
                    //htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td><td>" + "";
                    //htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td></tr>";
                    //htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</table>";
                    //$("#MensajeInformativo").html(htmlListaArchivosAdjuntos);

                    //$("#divMensajes").html("");

                    //$("#modalMensajeInformativoLabel").html('Listado de Archivos');
                    //$("#modalMensajeInformativo").modal('show');

                    idCliente = 0;
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
    else {
        alerta("Debe seleccionar al menos un producto para realizar el egreso.");
    }

    return;
}

function BtnConsulta() {
    ObtenerListaEgreso(0,"",8)
}

function BtnDescarga() {
    ObtenerListaEgresoDescargar(0, "", 8)
}

function ObtenerListaEgreso(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"MostrarListaEgreso\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", IdCliente : \"" + idCliente2 + "\", fecha1: \"" + $('#txtFechaActual2').val() + "\", fecha2: \"" + $('#txtFechaActual3').val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}

function ObtenerListaEgresoDescargar(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"MostrarListaEgresoDescargar\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", IdCliente : \"" + idCliente2 + "\", fecha1: \"" + $('#txtFechaActual2').val() + "\", fecha2: \"" + $('#txtFechaActual3').val() + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table");
}

function DetalleTareasDescargaXLS(div, url, datos, tipoControl) {
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
                if (respuesta.estado == "1") {
                    // Se abre el enlace al documento
                    window.open(respuesta.mensaje);
                } else {
                    $("#MensajeInformativo").html(respuesta.mensaje);
                    $('#modalMensajeInformativo').modal('show');
                }
            } else {
                var mesnajeError = "No existen datos para esta consulta.";
                $("#modalMensajeInformativoTipo").attr("style", "background: #f2dede")
                $("#MensajeInformativo").html(mesnajeError);
                $('#modalMensajeInformativo').modal('show');
            }

            $("#divMensajes").html("");

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La busqueda de la información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');

        }
    });

    return;
}

$(document).on('click', '.btn-eliminaDetalle', function (e) {
    e.preventDefault();
    //primer método: eliminar la fila del datatble
    var row = $(this).parent().parent()[0];
    var i = row.rowIndex;
    document.getElementById('tbl_Detalle').deleteRow(i);
    var total = 0;
    var filas = $("#tbl_Detalle").find("tr"); //devulve las filas del body de tu tabla segun el ejemplo que brindaste
    for (i = 1; i < filas.length; i++) { //Recorre las filas 1 a 1
        var celdas = $(filas[i]).find("td"); //devolverá las celdas de una fila
        var cantidad = $($(celdas[1]).children("input")[0]).val();
        var precio = $($(celdas[3]).children("input")[0]).val();

        total += (precio * cantidad)
    }
    document.getElementById('valorTotal').innerHTML = total.toFixed(2);
 
});

// Crear y descargar PDF
function generatePDF() {
    const element = document.querySelector('.modal-body');
    const opt = {
        margin: 0.5,
        filename: 'myfile.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
    };

    html2pdf().from(element).set(opt).save();
}



//Cambios

/* ==============================================================================
 *                      Cargar datos de la base 
 * =============================================================================*/

// Definir la función para buscar un empleado
function BuscarInfoEgresoInventarios(id) {
    ObtenerListaEmpleados2(id, "", "");
}
function ObtenerListaEmpleados2(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"ConsultarClienteInventarios\", \"parameters\" : { tipo : \"" + tipo + "\", opcion: \"" + "1" + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    var Datos2 = "[{ \"action\": \"ConsultarClienteInventarios\", \"parameters\" : { tipo : \"" + tipo + "\", opcion: \"" + "2" + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";

    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectEgresoInventario", tipo2);
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos2, "tableSelectTablaEgresoInventario", tipo2);
}

function RecorreJSONTableSelectEgresoInventario(json, boton, idSeleccionado) {
    dtEgresoInventarios(json);
}
function RecorreJSONTableSelectTablaEgresoInventario(json, boton, idSeleccionado) {
    dtProductosInv(json);
}

function dtEgresoInventarios(json) {

    $.each(json, function (i, item) {

        let FechaRegistro = item.FechaRegistro;
        let varCliente = item.Nombres;

        var fecha1 = document.getElementById("txtFechaIniTrans");
        fecha1.textContent = FechaRegistro

        var fecha2 = document.getElementById("txtFechaFinTrans");
        fecha2.textContent = FechaRegistro

        var fecha3 = document.getElementById("txtFechaAuto");
        fecha3.textContent = FechaRegistro

        var fecha4 = document.getElementById("TxtFechaEmision");
        fecha4.textContent = FechaRegistro

        var Cliente = document.getElementById("TxtRazonSocNomApe");
        Cliente.textContent = varCliente
    });
}

function Create() {
    if ($.fn.DataTable.isDataTable('#tbl_ProductosInv')) {
        $('#tbl_ProductosInv').DataTable().destroy();
    }
    $('#tbl_ProductosInv tbody').empty();
}
function dtProductosInv(json) {
    Create();
    table1 = null;

    // Agregar filas a la tabla
    $.each(json, function (i, item) {
        var fila = '<tr><td>' + item.Cantidad + '</td><td>' + item.Descripcion + '</td><td>' + item.CodigoSAP +
            '</td><td>' + item.NumParte + '</td><td>' + item.NumSerie + '</td></tr>';
        $('#tbl_ProductosInv tbody').append(fila);
    });

}

$(function () {


    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    var dateFormat = "dd/mm/yy";

    from = $("#txtFechaActual").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
    //.on("change", function () {
    //    to.datepicker("option", "minDate", getDate(this));
    //    $("#btn_Descarga").hide();
    //}),

    tofrom2 = $("#txtFechaActual2").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        //.on("change", function () {
        //    from.datepicker("option", "maxDate", getDate(this));
        //    $("#btn_Descarga").hide();
        //});
    tofrom2 = $("#txtFechaActual3").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        //.on("change", function () {
        //    from.datepicker("option", "maxDate", getDate(this));
        //    $("#btn_Descarga").hide();
        //});

    $("#txtFechaActual").datepicker('setDate', 'today');
    $("#txtFechaActual2").datepicker('setDate', 'today');
    $("#txtFechaActual3").datepicker('setDate', 'today');

});