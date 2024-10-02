let IdInventario = 0;
let table;

function DescargarProductos() {
    ObtenerListaInventarioDescargar(15, "", 7);
}

function DescargarInventario() {
    ObtenerListaInventarioFinalDescargar(16, "", 7);
}

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

function BuscarReferencia() {
    ObtenerInventarioIndividual(17, $('#txtReferencia').val(), 7);
}

function BuscarReferenciaIngresoInventario() {
    ObtenerInventarioIndividualNumSerie(19, $('#txtCodigoSap').val(), 7);
}

function VerRegistroProductos() {
    document.getElementById("editBoton").style.display = "none";
    document.getElementById("editMenu").style.display = "none";
    document.getElementById("editProductos").style.display = "block";
    BorrarCajas();
}

function VerRegistroProductosEditar() {
    document.getElementById("editBoton").style.display = "none";
    document.getElementById("editMenu").style.display = "none";
    document.getElementById("editProductos").style.display = "block";    
}

function VerListaProductos() {
    document.getElementById("editBoton").style.display = "block";
    document.getElementById("editMenu").style.display = "block";
    document.getElementById("editProductos").style.display = "none";
    document.getElementById("btnGProducto").innerHTML = "Guardar";
}

function ObtenerListaInventario(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaInventario\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}

function ObtenerListaInventarioFinal(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaInventario\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusquedaFinal", tipo2);
}

function ObtenerInventarioIndividual(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaInventario\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectReferencia", tipo2);
}

function ObtenerInventarioIndividualNumSerie(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaInventario\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectNumSerie", tipo2);
}

function ObtenerListaInventarioDescargar(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaInventarioDescargar\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "table");
}


function ObtenerListaInventarioFinalDescargar(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaInventarioFinalDescargar\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "table");
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
    if (tipoControl == "tableSelectBusquedaFinal") {
        contenido = RecorreJSONTableSelectBusquedaFinal(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectReferencia") {
        contenido = RecorreJSONTableSelectReferencia(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectNumSerie") {
        contenido = RecorreJSONTableSelectNumSerie(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    return contenido;
}

function RecorreJSONTableSelectReferencia(json, boton, idSeleccionado) {

    $.each(json, function (i, item) {

        $('#txtArticulo').val(item.Descripcion);
        $('#txtLocalizacion').val(item.Ubicacion);
        $('#txtNumPartes').val(item.NumParte);
        $('#txtUnidadMedida').val("Unidades");
        $('#txtExistencia').val(item.Cantidad);

    });

}

function RecorreJSONTableSelectNumSerie(json, boton, idSeleccionado) {

    $.each(json, function (i, item) {

        $('#txtDescripcion').val(item.Descripcion);
        $('#txtUbicacion').val(item.Ubicacion);
        $('#txtNumParte').val(item.NumParte);
        $('#txtUnidadMedida').val("Unidades");
        $('#txtAlmacen').val(item.Almacen);
        var combo1 = document.getElementById("cboMarca");
        combo1.value = item.IdMarca;
    });

}

function RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado) {
        dtProductos(json);
}

function RecorreJSONTableSelectBusquedaFinal(json, boton, idSeleccionado) {
    dtInventarioFinal(json);
}

function BuscarProducto() {
    ObtenerListaInventario(9, "", 7);
}

function BuscarProductoFinal() {
    ObtenerListaInventarioFinal(18, "", 7);
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
            { data: 'Cantidad' },
            { data: 'Descripcion' },
            { data: 'PrecioUnitario' },
            { data: 'NumParte' },
            { data: 'CodigoSAP' },
            { data: 'NumSerie' },
            { data: 'Ubicacion' },
            { defaultContent: "<button type='button' value='Actualizar' title='Seleccionar Items' class='btn btn btn-editProductos btn-xs'><i class='fa fa-eye' aria-hidden='true'></i></button>" },
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

function CreateInventarioFinal() {
    if ($.fn.DataTable.isDataTable('#tbl_InventarioTotal')) {
        $('#tbl_InventarioTotal').DataTable().destroy();
    }
    $('#tbl_InventarioTotal tbody').empty();
    //Here call the Datatable Bind function;
}

function dtInventarioFinal(json) {
    CreateInventarioFinal();
    table = null;

    table = $('#tbl_InventarioTotal').DataTable({
        data: json,
        columns: [
            { data: 'IdInventario' },
            { data: 'CodigoSAP' },
            { data: 'Descripcion' },
            { data: 'Cantidad' },
            { data: 'Ubicacion' },
            { data: 'PrecioUnitario' },
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

$(document).on('click', '.btn-editProductos', function (e) {
    e.preventDefault();
    var data = table.row($(this).parents('tr')).data();
    CargarItems(data);
    VerRegistroProductosEditar();
});

function ObtenerListaComboMarca() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"16\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboMarca', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function RecorreJSONSelect(div, selectValues) {

    $(div).empty();

    $.each(selectValues, function (key, value) {
        //
        // Despliegue de datos
        //
        $(div)
            .append($("<option></option>")
                .attr("value", value.Id)
                .text(value.Valor));
    });

    return;
}

function BorrarCajas() {
    var combo1 = document.getElementById("cboMarca");
    combo1.value = 0;
    $('#txtDescripcion').val("");
    $('#txtCodigoSap').val("");
    $('#txtNumParte').val("");
    $('#txtCantidad').val("");
    $('#txtUbicacion').val("");
    $('#txtAlmacen').val("");
    $('#txtNumSerie').val("");
    $('#txtPrecioUnitario').val("");
}

function CargarItems(data) {
    var combo1 = document.getElementById("cboMarca");
    combo1.value = data.IdMarca;
    IdInventario = data.IdInventario;
    $('#txtDescripcion').val(data.Descripcion);
    $('#txtCodigoSap').val(data.CodigoSAP);
    $('#txtNumParte').val(data.NumParte);
    $('#txtCantidad').val(data.Cantidad);
    $('#txtUbicacion').val(data.Ubicacion);
    $('#txtAlmacen').val(data.Almacen);
    $('#txtNumSerie').val(data.NumSerie);
    $('#txtPrecioUnitario').val(data.PrecioUnitario);
    document.getElementById("btnGProducto").innerHTML = "Actualizar";
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

function GuardarProducto() {
    if (document.getElementById("btnGProducto").innerHTML == "Actualizar") {

        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var mensajeVerificacion = "";
        var tipoMensaje = "warning";
        var contadorVerificacion = 0;

        if ($('#txtDescripcion').val() == "") {
            mensajeVerificacion += "- Debe ingresar la descripción ";
            contadorVerificacion += 1;
        }

        if ($('#cboMarca').val() == "0") {
            mensajeVerificacion += "- Debe ingresar la marca ";
            contadorVerificacion += 1;
        }
        else {
            var comboMarca = document.getElementById("cboMarca");
            var selectedoMarca = comboMarca.options[comboMarca.selectedIndex].text;
        }

        if ($('#txtCodigoSap').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Codigo Sap ";
            contadorVerificacion += 1;
        }

        if ($('#txtNumParte').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Numero Parte ";
            contadorVerificacion += 1;
        }

        if ($('#txtCantidad').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Cantidad ";
            contadorVerificacion += 1;
        }

        if ($('#txtUbicacion').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Ubicacion ";
            contadorVerificacion += 1;
        }

        if ($('#txtAlmacen').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Almacen ";
            contadorVerificacion += 1;
        }

        if ($('#txtNumSerie').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Numero Serie ";
            contadorVerificacion += 1;
        }

        if ($('#txtPrecioUnitario').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Precio ";
            contadorVerificacion += 1;
        }

        if (contadorVerificacion > 0) {
            alerta(mensajeVerificacion);
            return;
        }

        var datosFormulario = "";

        datosFormulario = datosFormulario + "{";
        datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
        datosFormulario = datosFormulario + "'IdInventario': '" + IdInventario + "',";
        datosFormulario = datosFormulario + "'IdMarca': '" + $('#cboMarca').val() + "',";
        datosFormulario = datosFormulario + "'txtDescripcion': '" + $('#txtDescripcion').val() + "',";
        datosFormulario = datosFormulario + "'txtCodigoSap': '" + $('#txtCodigoSap').val() + "',";
        datosFormulario = datosFormulario + "'txtNumParte': '" + $('#txtNumParte').val() + "',";
        datosFormulario = datosFormulario + "'txtCantidad': '" + formatearNumero(parseFloat($('#txtCantidad').val())) + "',";
        datosFormulario = datosFormulario + "'txtUbicacion': '" + $('#txtUbicacion').val() + "',";
        datosFormulario = datosFormulario + "'txtAlmacen': '" + $('#txtAlmacen').val() + "',";
        datosFormulario = datosFormulario + "'txtNumSerie': '" + $('#txtNumSerie').val() + "',";
        datosFormulario = datosFormulario + "'txtPrecioUnitario': '" + formatearNumero(parseFloat($('#txtPrecioUnitario').val())) + "',";
        datosFormulario = datosFormulario + "'Tipo': '" + "1" + "'";
        datosFormulario = datosFormulario + "}";

        datos = "[{'action': 'GuardarNuevoProducto', 'parameters' : " + datosFormulario + " }]";

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
                    BuscarProducto();
                    VerListaProductos();
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
    else if (document.getElementById("btnGProducto").innerHTML == "Guardar"){

        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var mensajeVerificacion = "";
        var tipoMensaje = "warning";
        var contadorVerificacion = 0;

        if ($('#txtDescripcion').val() == "") {
            mensajeVerificacion += "- Debe ingresar la descripción ";
            contadorVerificacion += 1;
        }

        if ($('#cboMarca').val() == "0") {
            mensajeVerificacion += "- Debe ingresar la marca ";
            contadorVerificacion += 1;
        }
        else {
            var comboMarca = document.getElementById("cboMarca");
            var selectedoMarca = comboMarca.options[comboMarca.selectedIndex].text;
        }

        if ($('#txtCodigoSap').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Codigo Sap ";
            contadorVerificacion += 1;
        }

        if ($('#txtNumParte').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Numero Parte ";
            contadorVerificacion += 1;
        }

        if ($('#txtCantidad').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Cantidad ";
            contadorVerificacion += 1;
        }

        if ($('#txtUbicacion').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Ubicacion ";
            contadorVerificacion += 1;
        }

        if ($('#txtAlmacen').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Almacen ";
            contadorVerificacion += 1;
        }

        if ($('#txtNumSerie').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Numero Serie ";
            contadorVerificacion += 1;
        }

        if ($('#txtPrecioUnitario').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Precio ";
            contadorVerificacion += 1;
        }

        if (contadorVerificacion > 0) {
            alerta(mensajeVerificacion);
            return;
        }

        var datosFormulario = "";

        datosFormulario = datosFormulario + "{";
        datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
        datosFormulario = datosFormulario + "'IdInventario': '" + IdInventario + "',";
        datosFormulario = datosFormulario + "'IdMarca': '" + $('#cboMarca').val() + "',";
        datosFormulario = datosFormulario + "'txtDescripcion': '" + $('#txtDescripcion').val() + "',";
        datosFormulario = datosFormulario + "'txtCodigoSap': '" + $('#txtCodigoSap').val() + "',";
        datosFormulario = datosFormulario + "'txtNumParte': '" + $('#txtNumParte').val() + "',";
        datosFormulario = datosFormulario + "'txtCantidad': '" + formatearNumero(parseFloat($('#txtCantidad').val())) + "',";
        datosFormulario = datosFormulario + "'txtUbicacion': '" + $('#txtUbicacion').val() + "',";
        datosFormulario = datosFormulario + "'txtAlmacen': '" + $('#txtAlmacen').val() + "',";
        datosFormulario = datosFormulario + "'txtNumSerie': '" + $('#txtNumSerie').val() + "',";
        datosFormulario = datosFormulario + "'txtPrecioUnitario': '" + formatearNumero(parseFloat($('#txtPrecioUnitario').val())) + "',";
        datosFormulario = datosFormulario + "'Tipo': '" + "0" + "'";
        datosFormulario = datosFormulario + "}";

        datos = "[{'action': 'GuardarNuevoProducto', 'parameters' : " + datosFormulario + " }]";

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
                    BuscarProducto();
                    VerListaProductos();
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
}

function RefreshProducto() {
    BuscarProductoFinal();
}

$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    BuscarProducto();
    BuscarProductoFinal();
    ObtenerListaComboMarca();
});