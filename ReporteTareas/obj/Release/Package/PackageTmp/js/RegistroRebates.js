var IdTipoIngreso = 0;
var IdPago = 0;
var IdMarca = 0;
var IdRebates = 0;
var IdBanco = 0;
var IdAnioFiscal = 0;

function ObtenerListaComboTipoPago() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"13\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboTipoPago', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboTipoIngreso() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"14\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboTipoIngreso', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboBanco() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"15\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboBanco', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboMarca() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"16\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboMarca', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboMarca2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"16\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboMarca2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboTipoPago2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"13\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboTipoPago2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboTipoIngreso2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"14\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboTipoIngreso2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboTipoAnio() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"17\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboAnio', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboQ() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"18\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboQ', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function BtnConsulta() {
    $("#divMensajes").html("");

    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaConsulta1").val();
        fechaFinal = $("#txtFechaConsulta2").val();
    }

    ObtenerListaRebates(0, 0, fechaInicio, fechaFinal);
}

function BtnDescarga() {
    //$("#divMensajes").html("");
    //ObtenerListaContratosDescarga(tipoGeneral, idregistroGeneral);
    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaConsulta1").val();
        fechaFinal = $("#txtFechaConsulta2").val();
    }

    ObtenerListaRebatesDescarga(0, 0, fechaInicio, fechaFinal);

}

function ObtenerListaRebates(tipo, idRegistro, fechaInicio, fechaFinal) {

    //var comboAnio = document.getElementById("cboAnio");
    //var selectedAnio = comboAnio.options[comboAnio.selectedIndex].text;
    //if (selectedAnio == "-- SELECCIONE --") {
    //    selectedAnio = "0";
    //}

    var comboQ = document.getElementById("cboQ");
    var selectedQ = comboQ.options[comboQ.selectedIndex].text;
    if (selectedQ == "-- SELECCIONE --") {
        selectedQ = "";
    }

    var Datos = "[{ \"action\": \"ReporteListaRebates\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", IdTipoIngreso: \"" + $("#cboTipoIngreso2").val() + "\", IdPago: \"" + $("#cboTipoPago2").val() + "\", IdMarca: \"" + $("#cboMarca2").val() + "\",  estado: \"" + $("#cboEstado2").val() + "\", Anio: \"" + $("#cboAnio").val()  + "\", meses: \"" + selectedQ + "\", idFecha: \"" + $("#cboFecha").val() + "\" } }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "tableSelect", tipo);
}

function ObtenerListaRebatesDescarga(tipo, idRegistro, fechaInicio, fechaFinal) {

    //var comboAnio = document.getElementById("cboAnio");
    //var selectedAnio = comboAnio.options[comboAnio.selectedIndex].text;
    //if (selectedAnio == "-- SELECCIONE --") {
    //    selectedAnio = "0";
    //}

    var comboQ = document.getElementById("cboQ");
    var selectedQ = comboQ.options[comboQ.selectedIndex].text;
    if (selectedQ == "-- SELECCIONE --") {
        selectedQ = "";
    }

    //var Datos = "[{ \"action\": \"ListaPedidosDescargaXLS\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", busqueda: \"" + $("#txtNumeroOrden2").val() + "\", IdCliente: \"" + idCliente2 + "\", IdGerenteCuenta: \"" + $("#cboVendedor2").val() + "\", IdGestorProducto: \"" + $("#cboGerente2").val() + "\", sucursal: \"" + selectedSucursal + "\", estado: \"" + $("#cboEstado2").val() + "\", IdClasificacion: \"" + $("#cboClasificacion2").val() + "\", Anio: \"" + $("#cboAnio").val() + "\", meses: \"" + $("#cboMeses").val() + "\", idFecha: \"" + $("#cboFecha").val() + "\"} }]";

    var Datos = "[{ \"action\": \"ListaRebatesDescargaXLS\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", IdTipoIngreso: \"" + $("#cboTipoIngreso2").val() + "\", IdPago: \"" + $("#cboTipoPago2").val() + "\", IdMarca: \"" + $("#cboMarca2").val() + "\",  estado: \"" + $("#cboEstado2").val() + "\", Anio: \"" + $("#cboAnio").val() + "\", meses: \"" + selectedQ + "\", idFecha: \"" + $("#cboFecha").val() + "\" } }]";
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

function ObtenerListaClientes(tipo, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", idproceso);
}

function ObtenerListaClientes2(tipo, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda2", idproceso);
}

function ObtenerListaClientes3(tipo, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda3", idproceso);
}

function ObtenerListaClientes4(tipo, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda4", idproceso);
}

function BuscarcboBanco() {

    if ($('#cboBanco').val().length > 1) {
        idSeleccionado = 0;
        ObtenerListaClientes(7, $('#cboBanco').val(), 7);
    }
    else {
        document.getElementById("combocboBanco").style.display = "none";
    }
}

function BuscarTipoIngreso() {

    if ($('#cboTipoIngreso').val().length > 1) {
        idSeleccionado = 0;
        ObtenerListaClientes(4, $('#cboTipoIngreso').val(), 4);
    }
    else {
        document.getElementById("comboTipoIngreso").style.display = "none";
    }
}

function BuscarMarca() {

    if ($('#cboMarca').val().length > 1) {
        idSeleccionado = 0;
        ObtenerListaClientes(5, $('#cboMarca').val(), 5);
    }
    else {
        document.getElementById("comboMarca").style.display = "none";
    }
}

function BuscarTipoPago() {

    if ($('#cboTipoPago').val().length > 2) {
        idSeleccionado = 0;
        ObtenerListaClientes(6, $('#cboTipoPago').val(), 6);
    }
    else {
        document.getElementById("comboTipoPago").style.display = "none";
    }
}

function BuscarTipoIngreso2() {

    if ($('#cboTipoIngreso2').val().length > 1) {
        idSeleccionado = 0;
        ObtenerListaClientes2(4, $('#cboTipoIngreso2').val(), 4);
    }
    else {
        document.getElementById("comboTipoIngreso2").style.display = "none";
    }
}

function BuscarMarca2() {

    if ($('#cboMarca2').val().length > 1) {
        idSeleccionado = 0;
        ObtenerListaClientes2(5, $('#cboMarca2').val(), 5);
    }
    else {
        document.getElementById("comboMarca2").style.display = "none";
    }
}

function BuscarTipoPago2() {

    if ($('#cboTipoPago2').val().length > 2) {
        idSeleccionado = 0;
        ObtenerListaClientes2(6, $('#cboTipoPago2').val(), 6);
    }
    else {
        document.getElementById("comboTipoPago2").style.display = "none";
    }
}

function BuscarMarca3() {

    if ($('#cboMarca3').val().length > 1) {
        idSeleccionado = 0;
        ObtenerListaClientes3(5, $('#cboMarca3').val(), 5);
    }
    else {
        document.getElementById("comboMarca3").style.display = "none";
    }
}

function BuscarMarca4() {

    if ($('#cboMarca4').val().length > 1) {
        idSeleccionado = 0;
        ObtenerListaClientes4(5, $('#cboMarca4').val(), 5);
    }
    else {
        document.getElementById("comboMarca3").style.display = "none";
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

    if (tipoControl == "select") {
        contenido = RecorreJSONSelect(div, json, idSeleccionado);
    }

    if (tipoControl == "tableSelect") {
        contenido = RecorreJSONTableSelect(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    if (tipoControl == "tableSelect2") {
        contenido = RecorreJSONTableSelect2(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    if (tipoControl == "tableSelectBusqueda") {
        contenido = RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    if (tipoControl == "tableSelectBusqueda2") {
        contenido = RecorreJSONTableSelectBusqueda2(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    if (tipoControl == "tableSelectBusqueda3") {
        contenido = RecorreJSONTableSelectBusqueda3(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    if (tipoControl == "tableSelectBusqueda4") {
        contenido = RecorreJSONTableSelectBusqueda4(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    if (tipoControl == "detalleDatos") {
        if (boton > 0) {
            contenido = RecorreDatosPantalla(json);
            $(div).html(contenido);
        }
        else {
            BorrarBotonesPedidos(1);
        }
    }

    if (tipoControl == "detalleDatos2") {
        if (boton > 0) {
            contenido = RecorreDatosPantalla2(json);
            $(div).html(contenido);
        }
        else {
            BorrarBotonesPedidos(1);
        }
    }

    return contenido;
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

function RecorreDatosPantalla(json) {

    $.each(json, function (i, item) {

        if (item.PROCESO == 1) {
            document.getElementById("IdProceso").checked = true;
        }
        else {
            document.getElementById("IdProceso").checked = false;
        }

        $('#txtFechaDesde').val(item.FECHA);
        $('#txtTransaccion').val(item.ID_TRANSACCION);
        $('#txtPrograma').val(item.PROGRAMA);
        //$('#cboTipoIngreso').val(item.TIPO_DE_INGRESO);
        //$('#cboMarca').val(item.MARCA);

        var combo1 = document.getElementById("cboTipoIngreso");
        combo1.value = item.IdTipoIngreso;

        var combo2 = document.getElementById("cboMarca");
        combo2.value = item.IdMarca;


        var combo3 = document.getElementById("cboTipoPago");
        combo3.value = item.IdPago;

        var combo4 = document.getElementById("cboBanco");
        combo4.value = item.IdBanco;

        $('#txtReponsable').val(item.RESPONSABLE);
        $('#txtDetalle').val(item.DESCRIPCION);
        $('#txtValor').val(item.VALOR.replace(",","."));
        $('#txtQFabricante').val(item.Q_FABRICANTE);
        //$('#cboTipoPago').val(item.TIPO_DE_PAGO);
        $('#cboEstado').val(item.ESTADO);
        $('#txtIdTransaccion2').val(item.ID_TRANSACCION1);
        $('#txtIdPago').val(item.ID_PAGO);
        $('#txtFechaPago').val(item.FECHA_PAGO);
        $('#txtFechaEstimadaPago').val(item.FECHA_ESTIMADA_PAGO);
        $('#txtObservacion').val(item.OBSERVACIONES);
        IdRebates = item.IdRebates;
        ListadoArchivosContrato("#divArchivosAdjuntosAnteriores", IdRebates);
    });
}

function RecorreDatosPantalla2(json) {

    $.each(json, function (i, item) {

        $('#cboMarca3').val(item.MARCA);
        $('#txtDescripcionQ').val(item.DESCRIPCION);
        $('#txtFechaInicio').val(item.FechaInicio);
        $('#txtFechaFinal').val(item.FechaFinal);
        $('#txtFechaInicioDOS').val(item.FechaInicioDOS);
        $('#txtFechaFinalDOS').val(item.FechaFinalDOS);
        IdAnioFiscal = item.IdAnioFiscal;
    });
}

function RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado) {

    var x = "";

    if (idSeleccionado == 4) {

        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarTipoIngreso(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboTipoIngreso").innerHTML = x;
        document.getElementById("comboTipoIngreso").style.display = "block";
    }
    else if (idSeleccionado == 5) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarMarca(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboMarca").innerHTML = x;
        document.getElementById("comboMarca").style.display = "block";
    }
    else if (idSeleccionado == 6) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarPago(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboTipoPago").innerHTML = x;
        document.getElementById("comboTipoPago").style.display = "block";
    }
    else if (idSeleccionado == 7) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarBanco(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("combocboBanco").innerHTML = x;
        document.getElementById("combocboBanco").style.display = "block";
    }

}

function RecorreJSONTableSelectBusqueda2(json, boton, idSeleccionado) {

    var x = "";

    if (idSeleccionado == 4) {

        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarTipoIngreso2(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboTipoIngreso2").innerHTML = x;
        document.getElementById("comboTipoIngreso2").style.display = "block";
    }
    else if (idSeleccionado == 5) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarMarca2(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboMarca2").innerHTML = x;
        document.getElementById("comboMarca2").style.display = "block";
    }
    else if (idSeleccionado == 6) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarPago2(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboTipoPago2").innerHTML = x;
        document.getElementById("comboTipoPago2").style.display = "block";
    }

}

function RecorreJSONTableSelectBusqueda3(json, boton, idSeleccionado) {

    var x = "";

    if (idSeleccionado == 5) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarMarca3(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboMarca3").innerHTML = x;
        document.getElementById("comboMarca3").style.display = "block";
    }
}

function RecorreJSONTableSelectBusqueda4(json, boton, idSeleccionado) {

    var x = "";

    if (idSeleccionado == 5) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarMarca4(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboMarca4").innerHTML = x;
        document.getElementById("comboMarca4").style.display = "block";
    }
}

function RecorreJSONTableSelect(json, idSeleccionado) {
    var info = "";

    if (json.length > 0) {

    }
    else {
        alerta("No hay información para mostrar");
    }

    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "ACCIONES" + thFinal;
    info = info + thInicial + "ESTADO" + thFinal;
    info = info + thInicial + "FECHA" + thFinal;
    info = info + thInicial + "ID_ACTIVIDAD" + thFinal;
    info = info + thInicial + "PROGRAMA" + thFinal;
    info = info + thInicial + "TIPO_DE_INGRESO" + thFinal;
    info = info + thInicial + "PROCESO" + thFinal;
    info = info + thInicial + "MARCA" + thFinal;
    info = info + thInicial + "DESCRIPCION" + thFinal;
    info = info + thInicial + "VALOR" + thFinal;
    info = info + thInicial + "Q_FABRICANTE" + thFinal;
    info = info + thInicial + "TIPO_DE_PAGO" + thFinal;
    info = info + thInicial + "EASYPAY_TRANSACTION" + thFinal;
    info = info + thInicial + "ID_PAGO" + thFinal;
    info = info + thInicial + "FECHA_PAGO" + thFinal;
    info = info + thInicial + "FECHA_ESTIMADA_PAGO" + thFinal;
    info = info + thInicial + "OBSERVACIONES" + thFinal;
    info = info + thInicial + "RESPONSABLE" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";
    var totalValor1 = 0;
    var totalValor2 = 0;
    var totalValor3 = 0;
    var esImpar = true;
    $.each(json, function (i, item) {

        info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.IdRebates + "'>";

        info = info + "<td class='sorting_1' style='text-align:center'>";
        info = info + "<i class='fa fa-eye' style='cursor: pointer' onclick='VerRebates(\"" + item.IdRebates + "\");' title'Ver numero de rebates'></i>";
        if (item.conteoArchivosAdjuntos != '0') {
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosTarea(\"#MensajeInformativo\", \"" + item.IdRebates + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
            info = info + "";
        }
        info = info + "</td>";
        if (item.ESTADO == "PAGADO") {
            info = info + "<td class='sorting_1' ;text-align: center;'>" + item.ESTADO + "</td>";
        }
        else if (item.ESTADO == "NO CUMPLIDO") {
            info = info + "<td class='sorting_1' style='background: " + "#DEFC04" + ";text-align: center;'>" + item.ESTADO + "</td>";
        }
        else if (item.ESTADO == "PENDIENTE") {
            info = info + "<td class='sorting_1' style='background: " + "#fe9c7d" + ";text-align: center;'>" + item.ESTADO + "</td>";
        }
        info = info + "<td class='sorting_1'>" + item.FECHA + "</td>";
        info = info + "<td class='sorting_1'>" + item.ID_TRANSACCION + "</td>";
        info = info + "<td class='sorting_1'>" + item.PROGRAMA + "</td>";
        info = info + "<td class='sorting_1'>" + item.TIPO_DE_INGRESO + "</td>";
        info = info + "<td class='sorting_1'>" + item.PROCESO + "</td>";
        info = info + "<td class='sorting_1'>" + " " + item.MARCA + "</td>";
        info = info + "<td class='sorting_1'>" + " " + item.DESCRIPCION + "</td>";
        info = info + "<td class='sorting_1'>" + "$ " + item.VALOR + "</td>";

        info = info + "<td class='sorting_1'>" + item.Q_FABRICANTE + "</td>";
        info = info + "<td class='sorting_1'>" + item.TIPO_DE_PAGO + "</td>";
        info = info + "<td class='sorting_1'>" + item.ID_TRANSACCION1 + "</td>";
        info = info + "<td class='sorting_1'>" + item.ID_PAGO + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA_PAGO + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA_ESTIMADA_PAGO + "</td>";
        info = info + "<td class='sorting_1'>" + item.OBSERVACIONES + "</td>";
        info = info + "<td class='sorting_1'>" + item.RESPONSABLE + "</td>";
        info = info + "</tr>";
    });

    //if (totalValor1 > 0 || totalValor2 > 0 || totalValor3 > 0) {
    //    info = info + "<tr>";
    //    info = info + "<td class='sorting_1' colspan='7' style='text-align: right;'><b>VALORES:</b></td>";
    //    info = info + "<td class='sorting_1'><b>" + "$ " + format_two_digits(totalValor1.toFixed(2)) + "</b></td>";
    //    info = info + "<td class='sorting_1'><b>" + "$ " + format_two_digits(totalValor2.toFixed(2)) + "</b></td>";
    //    info = info + "<td class='sorting_1'><b>" + "% " + format_two_digits((totalValor2 / totalValor1).toFixed(2)) + "</b></td>";
    //    info = info + "<td class='sorting_1' colspan='2'></td>";
    //    info = info + "</tr>";
    //}

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function RecorreJSONTableSelect2(json, idSeleccionado) {
    var info = "";
    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "ACCIONES" + thFinal;
    info = info + thInicial + "DESCRIPCION" + thFinal;
    info = info + thInicial + "FECHA INICIO MARCA" + thFinal;
    info = info + thInicial + "FECHA FINAL MARCA" + thFinal;
    info = info + thInicial + "FECHA INICIO DOS" + thFinal;
    info = info + thInicial + "FECHA FINAL DOS" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";
    var totalValor1 = 0;
    var totalValor2 = 0;
    var totalValor3 = 0;
    var esImpar = true;
    $.each(json, function (i, item) {

        info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.IdAnioFiscal + "'>";

        info = info + "<td class='sorting_1' style='text-align:center'>";
        info = info + "<i class='fa fa-eye' style='cursor: pointer' onclick='VerDescripcion(\"" + item.DESCRIPCION + "\");' title'Ver numero de rebates'></i>";
        info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        info = info + "<i class='fa fa-pencil-square-o' style='cursor: pointer' onclick='EditarPeriodoFiscal(\"" + item.IdAnioFiscal + "\");' title'Ver numero de contrato'></i>";
        info = info + "</td>";
        info = info + "<td class='sorting_1'>" + item.DESCRIPCION + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaInicio + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaFinal + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaInicioDOS + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaFinalDOS + "</td>";
        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function EditarPeriodoFiscal(IdAnioFiscal) {
    ObtenerConsultarPeriodoFiscal(0, '', '', IdAnioFiscal, 1);
    $('.nav-pills a[href="#RFiscal-pills"]').tab('show');
}

function VerDescripcion(detalle) {
    $('#txtQFabricante').val(detalle);
    $("#modalMensajeAprobarAnioFiscal").modal('hide');
}

function VerRebates(IdRebate) {
    ObtenerConsultarRebates(IdRebate, 0, 1)
    IdRebates = IdRebate;
    $('.nav-pills a[href="#home-pills"]').tab('show');
}

function ObtenerConsultarRebates(IdRebates, orden, tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarRebates\", \"parameters\" : { IdRebates: \"" + IdRebates + "\", orden: \"" + orden + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "detalleDatos", tipo);
}

function ObtenerConsultarPeriodoFiscal(IdMarca, fechaInicio, fechaFinal, IdAnioFiscal, tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarPeriodoFiscal\", \"parameters\" : { IdMarca: \"" + IdMarca + "\", fechaInicio: \"" + fechaInicio + "\", fechaFinal: \"" + fechaFinal + "\", IdAnioFiscal: \"" + IdAnioFiscal + "\", tipo: \"" + tipo + "\"} }]";
    if (IdAnioFiscal == 0) {
        CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelect2", tipo);
    }
    else {
        CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "detalleDatos2", tipo);
    }
}

function BtnConsultaAnio() {

    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros2");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaBusInicio").val();
        fechaFinal = $("#txtFechaBusFinal").val();
    }

    ObtenerConsultarPeriodoFiscal(IdMarca, fechaInicio, fechaFinal,0, 1);

}

function CargarTipoIngreso(ID, NOMBRE) {
    $('#cboTipoIngreso').val(NOMBRE);
    IdTipoIngreso = ID;
    document.getElementById("comboTipoIngreso").style.display = "none";
}

function CargarPago(ID, NOMBRE) {
    $('#cboTipoPago').val(NOMBRE);
    IdPago = ID;
    document.getElementById("comboTipoPago").style.display = "none";
}

function CargarBanco(ID, NOMBRE) {
    $('#cboBanco').val(NOMBRE);
    IdBanco = ID;
    document.getElementById("combocboBanco").style.display = "none";
}

function CargarMarca(ID, NOMBRE) {
    $('#cboMarca').val(NOMBRE);
    IdMarca = ID;
    document.getElementById("comboMarca").style.display = "none";
}

function CargarTipoIngreso2(ID, NOMBRE) {
    $('#cboTipoIngreso2').val(NOMBRE);
    IdTipoIngreso = ID;
    document.getElementById("comboTipoIngreso2").style.display = "none";
}

function CargarPago2(ID, NOMBRE) {
    $('#cboTipoPago2').val(NOMBRE);
    IdPago = ID;
    document.getElementById("comboTipoPago2").style.display = "none";
}

function CargarMarca2(ID, NOMBRE) {
    $('#cboMarca2').val(NOMBRE);
    IdMarca = ID;
    document.getElementById("comboMarca2").style.display = "none";
}

function CargarMarca3(ID, NOMBRE) {
    $('#cboMarca3').val(NOMBRE);
    IdMarca = ID;
    document.getElementById("comboMarca3").style.display = "none";
}

function CargarMarca4(ID, NOMBRE) {
    $('#cboMarca4').val(NOMBRE);
    IdMarca = ID;
    document.getElementById("comboMarca4").style.display = "none";
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

function BtnCrearMarca() {
    tipo = 4;
    document.getElementById("modalMensajeAprobarLabelTitulo").innerHTML = "Registro de Marca";
    $("#modalMensajeAprobar").modal('show');

}

function BtnCrearTipoIngreso() {
    tipo = 3;
    document.getElementById("modalMensajeAprobarLabelTitulo").innerHTML = "Registro de Tipo Ingreso";
    $("#modalMensajeAprobar").modal('show');

}

function BtnCrearTipoPago() {
    tipo = 6;
    document.getElementById("modalMensajeAprobarLabelTitulo").innerHTML = "Registro de Tipo Pago";
    $("#modalMensajeAprobar").modal('show');

}

function GuardarCliente() {

    if ($('#txtObservacionReq').val() == "") {
        alerta("Debe registrar la descripción..");
    }
    else {

        if (tipo == 5) {

            ActualizarPedidoObservacion(IdPedidos);
        }
        else {

            var url = "ObtenerListaTareas.ashx";
            var datos = "";
            var mensajeVerificacion = "";
            var tipoMensaje = "warning";
            var contadorVerificacion = 0;

            var datosFormulario = "";

            datosFormulario = datosFormulario + "{";
            datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
            datosFormulario = datosFormulario + "'Tipo': '" + tipo + "',";
            datosFormulario = datosFormulario + "'txtObservacionReq': '" + $('#txtObservacionReq').val() + "'";

            datosFormulario = datosFormulario + "}";

            datos = "[{'action': 'GuardarNuevoCliente', 'parameters' : " + datosFormulario + " }]";

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
                        ObtenerListaComboBanco();
                        ObtenerListaComboMarca();
                        ObtenerListaComboTipoPago();
                        ObtenerListaComboTipoIngreso();

                        ObtenerListaComboMarca2();
                        ObtenerListaComboTipoPago2();
                        ObtenerListaComboTipoIngreso2();

                        ObtenerListaComboTipoAnio();
                        ObtenerListaComboQ();
                        $('#txtObservacionReq').val("");
                        MensajeCorrecto(respuesta.mensaje);
                        $("#modalMensajeAprobar").modal('hide');
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

        return;

    }
}

function GuardarNuevoRebates() {

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;
    var proceso = 0;
    var text = document.getElementById("IdProceso");
    if (text.checked == true) {
        proceso = 1;
    }
    else {
        proceso = 0;
    }


    if ($('#txtFechaDesde').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha de registro ";
        contadorVerificacion += 1;
    }

    if ($('#txtTransaccion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Actividad";
        contadorVerificacion += 1;
    }

    //if ($('#txtPrograma').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la Programa ";
    //    contadorVerificacion += 1;
    //}

    if ($('#cboTipoIngreso').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el nombre tipo ingreso ";
        contadorVerificacion += 1;
    }
    else {
        var comboTipoIngreso = document.getElementById("cboTipoIngreso");
        var selectedTipoIngreso = comboTipoIngreso.options[comboTipoIngreso.selectedIndex].text;
    }

    if ($('#cboMarca').val() == "0") {
        mensajeVerificacion += "- Debe ingresar la Marca ";
        contadorVerificacion += 1;
    }
    else {
        var comboMarca = document.getElementById("cboMarca");
        var selectedoMarca = comboMarca.options[comboMarca.selectedIndex].text;
    }

    if ($('#cboTipoPago').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el Tipo Pago ";
        contadorVerificacion += 1;
    }
    else {
        var comboTipoPago = document.getElementById("cboTipoPago");
        var selectedoTipoPago = comboTipoPago.options[comboTipoPago.selectedIndex].text;
    }

    //if ($('#cboTipoIngreso').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el nombre tipo ingreso ";
    //    contadorVerificacion += 1;
    //}
    //else {
    //    var comboTipoIngreso = $('#cboTipoIngreso').val();
    //    var selectedTipoIngreso = $('#cboTipoIngreso').val();
    //}

    //if ($('#cboMarca').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la Marca ";
    //    contadorVerificacion += 1;
    //}
    //else {
    //    var comboMarca = $('#cboMarca').val();
    //    var selectedoMarca = $('#cboMarca').val();
    //}

    //if ($('#cboTipoPago').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el Tipo Pago ";
    //    contadorVerificacion += 1;
    //}
    //else {
    //    var comboTipoPago = $('#cboTipoPago').val();
    //    var selectedoTipoPago = $('#cboTipoPago').val();
    //}

    //if ($('#txtDetalle').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el detalle ";
    //    contadorVerificacion += 1;
    //}

    if ($('#txtValor').val() == "") {
        mensajeVerificacion += "- Debe ingresar el valor ";
        contadorVerificacion += 1;
    }

    if ($('#txtQFabricante').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Q Fabricante ";
        contadorVerificacion += 1;
    }

    if ($('#cboEstado').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar el estado ";
        contadorVerificacion += 1;
    }

    if ($('#cboEstado').val() == "PAGADO" && $('#txtFechaPago').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha de pago ";
        contadorVerificacion += 1;
    }

    if ($('#cboEstado').val() == "PENDIENTE" && $('#txtFechaEstimadaPago').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha estimada de pago ";
        contadorVerificacion += 1;
    }


    if ($('#txtIdTransaccion2').val() == "") {
        mensajeVerificacion += "- Debe ingresar la transacción ";
        contadorVerificacion += 1;
    }

    if ($('#txtIdPago').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Id Pago ";
        contadorVerificacion += 1;
    }

    if ($('#txtObservacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la observación ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtTransaccion': '" + $('#txtTransaccion').val() + "',";
    datosFormulario = datosFormulario + "'txtPrograma': '" + $('#txtPrograma').val() + "',";
    datosFormulario = datosFormulario + "'cboTipoIngreso': '" + selectedTipoIngreso + "',";
    datosFormulario = datosFormulario + "'cboMarca': '" + selectedoMarca + "',";
    datosFormulario = datosFormulario + "'proceso': '" + proceso + "',";
    datosFormulario = datosFormulario + "'txtReponsable': '" + $('#txtReponsable').val()  + "',";
    datosFormulario = datosFormulario + "'txtDetalle': '" + $('#txtDetalle').val() + "',";
    datosFormulario = datosFormulario + "'txtValor': '" + $('#txtValor').val() + "',";
    datosFormulario = datosFormulario + "'txtQFabricante': '" + $('#txtQFabricante').val() + "',";
    datosFormulario = datosFormulario + "'cboTipoPago': '" + selectedoTipoPago + "',";
    datosFormulario = datosFormulario + "'cboEstado': '" + $('#cboEstado').val() + "',";
    datosFormulario = datosFormulario + "'txtIdTransaccion2': '" + $('#txtIdTransaccion2').val() + "',";
    datosFormulario = datosFormulario + "'txtIdPago': '" + $('#txtIdPago').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaPago': '" + $('#txtFechaPago').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEstimadaPago': '" + $('#txtFechaEstimadaPago').val() + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "0" + "',";
    datosFormulario = datosFormulario + "'IdBanco': '" + $('#cboBanco').val() + "',";
    datosFormulario = datosFormulario + "'IdRebates': '" + "0" + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoRebates', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesRebates(0);
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

function BorrarBotonesPeriodoFiscal() {
    $('#cboMarca3').val("");
    $('#txtDescripcionQ').val("");
    $("#modalMensajeAprobarAnioFiscal").modal('hide');
}

function BorrarBotonesBanco() {
    $('#txtRegistroBanco').val("");
    $('#txtPorcentaje').val("");
    $("#modalMensajeAprobarBanco").modal('hide');
}

function BorrarBotonesRebates() {

     IdTipoIngreso = 0;
     IdPago = 0;
     IdMarca = 0;
     IdRebates = 0;
     IdBanco = 0;
     IdAnioFiscal = 0;

    $('#txtTransaccion').val("");
    $('#txtPrograma').val("");
    $('#txtDetalle').val("");
    $('#cboTipoIngreso').val("");
    $('#cboMarca').val("");
    $('#cboBanco').val("");
    $('#txtDetalle').val("");
    $('#txtQFabricante').val("");
    $('#cboTipoPago').val("");
    $('#txtDetalle').val("");
    $('#txtIdTransaccion2').val("");
    $('#txtIdPago').val("");
    $('#txtObservacion').val("");
    $('#txtValor').val("");
    $('#txtFechaPago').val("");
    $('#txtFechaEstimadaPago').val("");
    var combo3 = document.getElementById("cboEstado");
    combo3.selectedIndex = 0;
    document.getElementById("IdProceso").checked = false;
}

function ActualizarRebates() {

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;
    var proceso = 0;
    var text = document.getElementById("IdProceso");
    if (text.checked == true) {
        proceso = 1;
    }
    else {
        proceso = 0;
    }


    if ($('#txtFechaDesde').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha de registro ";
        contadorVerificacion += 1;
    }

    if ($('#txtTransaccion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la transacción ";
        contadorVerificacion += 1;
    }

    //if ($('#txtPrograma').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la Programa ";
    //    contadorVerificacion += 1;
    //}

    if ($('#cboTipoIngreso').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el nombre tipo ingreso ";
        contadorVerificacion += 1;
    }
    else {
        var comboTipoIngreso = document.getElementById("cboTipoIngreso");
        var selectedTipoIngreso = comboTipoIngreso.options[comboTipoIngreso.selectedIndex].text;
    }

    if ($('#cboMarca').val() == "0") {
        mensajeVerificacion += "- Debe ingresar la Marca ";
        contadorVerificacion += 1;
    }
    else {
        var comboMarca = document.getElementById("cboMarca");
        var selectedoMarca = comboMarca.options[comboMarca.selectedIndex].text;
    }

    //if ($('#txtDetalle').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el detalle ";
    //    contadorVerificacion += 1;
    //}

    if ($('#txtValor').val() == "") {
        mensajeVerificacion += "- Debe ingresar el valor ";
        contadorVerificacion += 1;
    }

    if ($('#txtQFabricante').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Q Fabricante ";
        contadorVerificacion += 1;
    }

    if ($('#cboTipoPago').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el Tipo Pago ";
        contadorVerificacion += 1;
    }
    else {
        var comboTipoPago = document.getElementById("cboTipoPago");
        var selectedoTipoPago = comboTipoPago.options[comboTipoPago.selectedIndex].text;
    }

    if ($('#cboEstado').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar el estado ";
        contadorVerificacion += 1;
    }

    if ($('#txtIdTransaccion2').val() == "") {
        mensajeVerificacion += "- Debe ingresar la transacción ";
        contadorVerificacion += 1;
    }

    if ($('#txtIdPago').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Id Pago ";
        contadorVerificacion += 1;
    }

    if ($('#txtObservacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la observación ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";


    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtTransaccion': '" + $('#txtTransaccion').val() + "',";
    datosFormulario = datosFormulario + "'txtPrograma': '" + $('#txtPrograma').val() + "',";
    datosFormulario = datosFormulario + "'cboTipoIngreso': '" + selectedTipoIngreso + "',";
    datosFormulario = datosFormulario + "'cboMarca': '" + selectedoMarca + "',";
    datosFormulario = datosFormulario + "'proceso': '" + proceso + "',";
    datosFormulario = datosFormulario + "'txtReponsable': '" + $('#txtReponsable').val() + "',";
    datosFormulario = datosFormulario + "'txtDetalle': '" + $('#txtDetalle').val() + "',";
    datosFormulario = datosFormulario + "'txtValor': '" + $('#txtValor').val() + "',";
    datosFormulario = datosFormulario + "'txtQFabricante': '" + $('#txtQFabricante').val() + "',";
    datosFormulario = datosFormulario + "'cboTipoPago': '" + selectedoTipoPago + "',";
    datosFormulario = datosFormulario + "'cboEstado': '" + $('#cboEstado').val() + "',";
    datosFormulario = datosFormulario + "'txtIdTransaccion2': '" + $('#txtIdTransaccion2').val() + "',";
    datosFormulario = datosFormulario + "'txtIdPago': '" + $('#txtIdPago').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaPago': '" + $('#txtFechaPago').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEstimadaPago': '" + $('#txtFechaEstimadaPago').val() + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    datosFormulario = datosFormulario + "'IdBanco': '" + $('#cboBanco').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "1" + "',";
    datosFormulario = datosFormulario + "'IdRebates': '" + IdRebates + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoRebates', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesRebates(0);
                MensajeCorrecto(respuesta.mensaje);
                $("#divMensajes").html("");
                $('.nav-pills a[href="#profile-pills"]').tab('show');
                BtnConsulta();
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

function EliminarRebates() {
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;
    var proceso = 0;
    var text = document.getElementById("IdProceso");
    if (text.checked == true) {
        proceso = 1;
    }
    else {
        proceso = 0;
    }


    if ($('#txtFechaDesde').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha de registro ";
        contadorVerificacion += 1;
    }

    if ($('#txtTransaccion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la transacción ";
        contadorVerificacion += 1;
    }

    if ($('#txtPrograma').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Programa ";
        contadorVerificacion += 1;
    }

    if ($('#cboTipoIngreso').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre tipo ingreso ";
        contadorVerificacion += 1;
    }
    else {
        var comboTipoIngreso = $('#cboTipoIngreso').val();
        var selectedTipoIngreso = $('#cboTipoIngreso').val();
    }

    if ($('#cboMarca').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Marca ";
        contadorVerificacion += 1;
    }
    else {
        var comboMarca = $('#cboMarca').val();
        var selectedoMarca = $('#cboMarca').val();
    }

    if ($('#txtDetalle').val() == "") {
        mensajeVerificacion += "- Debe ingresar el detalle ";
        contadorVerificacion += 1;
    }

    if ($('#txtValor').val() == "") {
        mensajeVerificacion += "- Debe ingresar el valor ";
        contadorVerificacion += 1;
    }

    if ($('#txtQFabricante').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Q Fabricante ";
        contadorVerificacion += 1;
    }

    if ($('#cboTipoPago').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Tipo Pago ";
        contadorVerificacion += 1;
    }
    else {
        var comboTipoPago = $('#cboTipoPago').val();
        var selectedoTipoPago = $('#cboTipoPago').val();
    }

    if ($('#cboEstado').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar el estado ";
        contadorVerificacion += 1;
    }

    if ($('#txtIdTransaccion2').val() == "") {
        mensajeVerificacion += "- Debe ingresar la transacción ";
        contadorVerificacion += 1;
    }

    if ($('#txtIdPago').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Id Pago ";
        contadorVerificacion += 1;
    }

    if ($('#txtObservacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la observación ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";


    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtTransaccion': '" + $('#txtTransaccion').val() + "',";
    datosFormulario = datosFormulario + "'txtPrograma': '" + $('#txtPrograma').val() + "',";
    datosFormulario = datosFormulario + "'cboTipoIngreso': '" + selectedTipoIngreso + "',";
    datosFormulario = datosFormulario + "'cboMarca': '" + selectedoMarca + "',";
    datosFormulario = datosFormulario + "'proceso': '" + proceso + "',";
    datosFormulario = datosFormulario + "'txtReponsable': '" + $('#txtReponsable').val() + "',";
    datosFormulario = datosFormulario + "'txtDetalle': '" + $('#txtDetalle').val() + "',";
    datosFormulario = datosFormulario + "'txtValor': '" + $('#txtValor').val() + "',";
    datosFormulario = datosFormulario + "'txtQFabricante': '" + $('#txtQFabricante').val() + "',";
    datosFormulario = datosFormulario + "'cboTipoPago': '" + selectedoTipoPago + "',";
    datosFormulario = datosFormulario + "'cboEstado': '" + $('#cboEstado').val() + "',";
    datosFormulario = datosFormulario + "'txtIdTransaccion2': '" + $('#txtIdTransaccion2').val() + "',";
    datosFormulario = datosFormulario + "'txtIdPago': '" + $('#txtIdPago').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaPago': '" + $('#txtFechaPago').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEstimadaPago': '" + $('#txtFechaEstimadaPago').val() + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    //datosFormulario = datosFormulario + "'IdBanco': '" + $('#cboBanco').val() + "',";
    datosFormulario = datosFormulario + "'IdBanco': '" + "0" + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "2" + "',";
    datosFormulario = datosFormulario + "'IdRebates': '" + IdRebates + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoRebates', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesRebates(0);
                MensajeCorrecto(respuesta.mensaje);
                $("#divMensajes").html("");
                BtnConsulta();
                $('.nav-pills a[href="#profile-pills"]').tab('show');
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

function GuardarBanco() {
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;
   

    if ($('#txtRegistroBanco').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Banco ";
        contadorVerificacion += 1;
    }

    if ($('#txtPorcentaje').val() == "") {
        mensajeVerificacion += "- Debe ingresar el porcentaje transacción ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";


    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtRegistroBanco': '" + $('#txtRegistroBanco').val() + "',";
    datosFormulario = datosFormulario + "'txtPorcentaje': '" + $('#txtPorcentaje').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "0" + "',";
    datosFormulario = datosFormulario + "'IdBanco': '" + "0" + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoBanco', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesBanco();
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

function GuardarAnioFiscal() {
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    if ($('#cboMarca3').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Marca ";
        contadorVerificacion += 1;
    }
    else {
        var comboMarca = $('#cboMarca3').val();
        var selectedoMarca = $('#cboMarca3').val();
    }

    if ($('#txtDescripcionQ').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Descripcion ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'cboMarca3': '" + IdMarca + "',";
    datosFormulario = datosFormulario + "'txtDescripcionQ': '" + $('#txtDescripcionQ').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaInicio': '" + $('#txtFechaInicio').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaFinal': '" + $('#txtFechaFinal').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaInicioDOS': '" + $('#txtFechaInicioDOS').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaFinalDOS': '" + $('#txtFechaFinalDOS').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "0" + "',";
    datosFormulario = datosFormulario + "'IdAnioFiscal': '" + "0" + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoPeridoFiscal', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesPeriodoFiscal();
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

function BtnAnioFiscal() {
    $("#modalMensajeAprobarAnioFiscal").modal('show');
}

function BtnBanco() {
    $("#modalMensajeAprobarBanco").modal('show');
}

function BtnSeleccionarQF() {
    if ($("#cboMarca").val() > 0) {
        ObtenerConsultarPeriodoFiscal($("#cboMarca").val(),'','',0, 1);
        $('.nav-pills a[href="#RFLista-pills"]').tab('show');
        $("#modalMensajeAprobarAnioFiscal").modal('show');
    }
    else {
        alerta("Debe Seleccionar una Marca..");
    }
}

function VerListadoArchivosTarea(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'IdServicio': '" + "2" + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + idTarea + "'";
    datosParametros = datosParametros + "}";

    datos = "[{'action': 'ListaArchivosTarea', 'parameters' : " + datosParametros + " }]";

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
                    var htmlListaArchivosAdjuntos = "<table border='1' style='border-collapse: collapse;border: 1px solid #efefef; width:100%'>";
                    $.each(respuesta.resultado, function (i, item) {
                        //
                        // Despliegue de datos
                        // 
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<tr id='tdArchivo_" + item.Id_ArchivosTarea + "'><td>";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<a href='../descargas/" + item.Nombre_ArchivoCodigo + "' target='_blank' style='font-size:12px'>&nbsp;<i class='fa " + item.Icon_Nombre + "'></i>&nbsp;" + item.Nombre_Archivo + "</a><br>";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td><td>" + "";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td></tr>";
                    });

                    htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</table>";
                    $(div).html(htmlListaArchivosAdjuntos);
                }
            } else {
                var mesnajeTexto = "No existen archivos adjuntos.";
                $("#divMensajes").html(mesnajeTexto);
                $(div).html(mesnajeTexto);
            }

            $("#divMensajes").html("");

            $("#modalMensajeInformativoLabel").html('Listado de Archivos');
            $("#modalMensajeInformativo").modal('show');

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La busqueda de la información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });

}

function ListadoArchivosTarea(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + idTarea + "'";
    datosParametros = datosParametros + "}";

    datos = "[{'action': 'ListaArchivosContrato', 'parameters' : " + datosParametros + " }]";

    // Create FormData object  
    var fileData = new FormData();
    fileData.append('action', 'ListaArchivosTarea');
    fileData.append('session', $("#ContentPlaceHolder1_txtUsuario").val());
    fileData.append('frmTxtCodigo', $("#frmTxtCodigo").val());

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
                    var htmlListaArchivosAdjuntos = "<table border='1' style='border-collapse: collapse;border: 1px solid #efefef; width:100%'>";
                    $.each(respuesta.resultado, function (i, item) {
                        //
                        // Despliegue de datos
                        // 
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<tr id='tdArchivo_" + item.Id_ArchivosTarea + "'><td>";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<a href='../descargas/" + item.Nombre_ArchivoCodigo + "' target='_blank' style='font-size:10px'>&nbsp;<i class='fa " + item.Icon_Nombre + "'></i>&nbsp;" + item.Nombre_Archivo + "</a><br>";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td><td>" + "<i class='fa fa-trash-o' onclick='ConfirmarEliminarArchivo(\"" + item.Id_ArchivosTarea + "\", \"" + item.Icon_Nombre + "\",\"" + item.Nombre_Archivo + "\")'></i>";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td></tr>";
                    });

                    htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</table>";
                    $(div).html(htmlListaArchivosAdjuntos);
                }
            } else {
                $("#divMensajes").html("No existen datos para esta consulta.");
            }

            $("#divMensajes").html("");

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La busqueda de la información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });


}

function VerListadoArchivosContrato(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + idTarea + "'";
    datosParametros = datosParametros + "}";

    datos = "[{'action': 'ListaArchivosContrato', 'parameters' : " + datosParametros + " }]";

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
                    var htmlListaArchivosAdjuntos = "<table border='1' style='border-collapse: collapse;border: 1px solid #efefef; width:100%'>";
                    $.each(respuesta.resultado, function (i, item) {
                        //
                        // Despliegue de datos
                        // 
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<tr id='tdArchivo_" + item.Id_ArchivosTarea + "'><td>";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<a href='../descargas/" + item.Nombre_ArchivoCodigo + "' target='_blank' style='font-size:12px'>&nbsp;<i class='fa " + item.Icon_Nombre + "'></i>&nbsp;" + item.Nombre_Archivo + "</a><br>";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td><td>" + "";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td></tr>";
                    });

                    htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</table>";
                    $(div).html(htmlListaArchivosAdjuntos);
                }
            } else {
                var mesnajeTexto = "No existen archivos adjuntos.";
                $("#divMensajes").html(mesnajeTexto);
                $(div).html(mesnajeTexto);
            }

            $("#divMensajes").html("");

            $("#modalMensajeInformativoLabel").html('Listado de Archivos');
            $("#modalMensajeInformativo").modal('show');

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La busqueda de la información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });

}

function ListadoArchivosContrato(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + idTarea + "'";
    datosParametros = datosParametros + "}";

    datos = "[{'action': 'ListaArchivosContrato', 'parameters' : " + datosParametros + " }]";

    // Create FormData object  
    var fileData = new FormData();
    fileData.append('action', 'ListaArchivosTarea');
    fileData.append('session', $("#ContentPlaceHolder1_txtUsuario").val());
    fileData.append('frmTxtCodigo', $("#frmTxtCodigo").val());

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
                    var htmlListaArchivosAdjuntos = "<table border='1' style='border-collapse: collapse;border: 1px solid #efefef; width:100%'>";
                    $.each(respuesta.resultado, function (i, item) {
                        //
                        // Despliegue de datos
                        // 
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<tr id='tdArchivo_" + item.Id_ArchivosTarea + "'><td>";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "<a href='../descargas/" + item.Nombre_ArchivoCodigo + "' target='_blank' style='font-size:10px'>&nbsp;<i class='fa " + item.Icon_Nombre + "'></i>&nbsp;" + item.Nombre_Archivo + "</a><br>";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td><td>" + "<i class='fa fa-trash-o' onclick='ConfirmarEliminarArchivo(\"" + item.Id_ArchivosTarea + "\", \"" + item.Icon_Nombre + "\",\"" + item.Nombre_Archivo + "\")'></i>";
                        htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</td></tr>";
                    });

                    htmlListaArchivosAdjuntos = htmlListaArchivosAdjuntos + "</table>";
                    $(div).html(htmlListaArchivosAdjuntos);
                }
            } else {
                $("#divMensajes").html("No existen datos para esta consulta.");
            }

            $("#divMensajes").html("");

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La busqueda de la información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });


}

function ConfirmarEliminarArchivo(codigoItem, iconArchivo, nombreArchivo) {
    // Habilitar mensaje
    var mensaje = 'Desea continuar con la eliminación del archivo.';
    var mensaje = "Desea continuar con la eliminación del archivo <i class='fa " + iconArchivo + "'></i>&nbsp;(" + nombreArchivo + ").";
    $("#txtCodigoItem").val(codigoItem);
    $("#modalMensajeConfirmacioMensaje").html(mensaje);
    $("#modalMensajeConfirmacion").modal('show');

    return;
}

function EliminarArchivo() {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'txtCodigoItem': '" + $("#txtCodigoItem").val() + "'";
    datosParametros = datosParametros + "}";

    datos = "[{'action': 'BorrarArchivosContrato', 'parameters' : " + datosParametros + " }]";

    $.ajax({
        type: "POST",
        url: url,
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function (respuesta) {
            $("#divMensajes").html("Borrando Información...");
        },
        success: function (respuesta) {

            if (respuesta.estado == "1") {
                MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);
                $("#tdArchivo_" + $("#txtCodigoItem").val()).remove();
            } else {
                MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);
            }

            $("#divMensajes").html("");

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "El borrado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });

    $("#modalMensajeConfirmacion").modal('hide');

    return;
}

$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    var dateFormat = "dd/mm/yy";

    from = $("#txtFechaDesde").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            to.datepicker("option", "minDate", getDate(this));
            $("#btn_Descarga").hide();
        }),

    tofrom7 = $("#txtFechaConsulta1").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "maxDate", getDate(this));
            $("#btn_Descarga").hide();
        });

    tofrom8 = $("#txtFechaConsulta2").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "maxDate", getDate(this));
            $("#btn_Descarga").hide();
        });

    tofrom9 = $("#txtFechaPago").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "maxDate", getDate(this));
            $("#btn_Descarga").hide();
        });

    tofrom10 = $("#txtFechaEstimadaPago").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "maxDate", getDate(this));
            $("#btn_Descarga").hide();
        });

    tofrom11 = $("#txtFechaInicio").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "maxDate", getDate(this));
            $("#btn_Descarga").hide();
        });

    tofrom12 = $("#txtFechaFinal").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "maxDate", getDate(this));
            $("#btn_Descarga").hide();
        });

    tofrom13 = $("#txtFechaInicioDOS").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "maxDate", getDate(this));
            $("#btn_Descarga").hide();
        });

    tofrom14 = $("#txtFechaFinalDOS").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "maxDate", getDate(this));
            $("#btn_Descarga").hide();
        });

    tofrom15 = $("#txtFechaBusInicio").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "maxDate", getDate(this));
            $("#btn_Descarga").hide();
        });

    tofrom16 = $("#txtFechaBusFinal").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "maxDate", getDate(this));
            $("#btn_Descarga").hide();
        });

    $("#txtFechaConsulta1").datepicker('setDate', 'today');
    $("#txtFechaConsulta2").datepicker('setDate', 'today');
    $("#txtFechaDesde").datepicker('setDate', 'today');

    $("#txtFechaInicio").datepicker('setDate', 'today');
    $("#txtFechaFinal").datepicker('setDate', 'today');
    $("#txtFechaInicioDOS").datepicker('setDate', 'today');
    $("#txtFechaFinalDOS").datepicker('setDate', 'today');
    $("#txtFechaBusInicio").datepicker('setDate', 'today');
    $("#txtFechaBusFinal").datepicker('setDate', 'today');

   
    function calculeTime(timeInitial, timeFinaly, elementResult) {

        var fromDateCalcule = moment(timeInitial, 'HH:mm');
        var toDateCalcule = moment(timeFinaly, 'HH:mm');

        if (fromDateCalcule.isValid() && toDateCalcule.isValid()) {

            var duration = moment.duration(toDateCalcule.diff(fromDateCalcule));

            $(elementResult).val(moment(duration.hours() + ':' + duration.minutes(), 'HH:mm').format('HH:mm'));

        } else {
            $("#messageNotify").html('Tiempos ingresados no válidos');
        }

        return;
    }

    function getMinutes(element) {

        var timeElement = moment(element.val(), "HH:mm");
        var minutesTotal = (timeElement.hours() * 60) + timeElement.minutes();

        return parseInt(minutesTotal);
    }

    function getDate(element) {

        var date;
        try {
            date = $.datepicker.parseDate(dateFormat, element.value);
        } catch (error) {
            date = null;
        }

        return date;
    }

    ObtenerListaComboBanco();
    ObtenerListaComboMarca();
    ObtenerListaComboTipoPago();
    ObtenerListaComboTipoIngreso();

    ObtenerListaComboMarca2();
    ObtenerListaComboTipoPago2();
    ObtenerListaComboTipoIngreso2();

    ObtenerListaComboTipoAnio();
    ObtenerListaComboQ();

    // ------------------------------------------------------------------

    $('#btnCargarArchivosAdjuntos').click(function () {

        if (IdRebates == 0) {

            alerta("Para cargar un archivo debe seleccionar un rebates...");
        }
        else {
            var url = 'CargaArchivos.ashx';

            // Checking whether FormData is available in browser  
            if (window.FormData !== undefined) {

                var fileUpload = $("#archivosAdjuntos").get(0);
                var files = fileUpload.files;

                // Create FormData object  
                var fileData = new FormData();

                // Looping over all files and add it to FormData object  
                for (var i = 0; i < files.length; i++) {
                    fileData.append(files[i].name, files[i]);
                }

                // Adding one more key to FormData object  
                fileData.append('session', $("#ContentPlaceHolder1_txtUsuario").val());
                fileData.append('action', 'CargarArchivos');
                fileData.append('Id_RegTareas', IdRebates);
                fileData.append('idServicio', "2");

                $.ajax({
                    type: "POST",
                    url: url,
                    data: fileData,
                    contentType: false, // Not to set any content header  
                    processData: false, // Not to process data  
                    dataType: "json",
                    beforeSend: function (respuesta) {
                        $("#divMensajes").html("Cargando Archivos...");
                    },
                    success: function (respuesta) {
                        if (respuesta.estado == "1") {
                            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);
                            ListadoArchivosContrato("#divArchivosAdjuntosAnteriores", IdRebates);
                        }
                        $("progress").hide();


                    },
                    xhr: function () {
                        var fileXhr = $.ajaxSettings.xhr();
                        if (fileXhr.upload) {
                            $("progress").show();
                            fileXhr.upload.addEventListener("progress", function (e) {
                                if (e.lengthComputable) {
                                    $("#fileProgress").attr({
                                        value: e.loaded,
                                        max: e.total
                                    });
                                }
                            }, false);
                        }
                        return fileXhr;
                    },
                    error: function (objeto, msgError, objError) {
                        var mesnajeError = "La acción de cargar de archivo esta tomando demasiado tiempo. Verifique su conexión de red y luego intente nuevamente cargar los archivos.";
                        MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
                    }
                });
            } else {
                var mesnajeError = "FormData no es soportado por su navegador.";
                MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
            }
        }
    });

});

$(document).ready(function () {

    $('.js-example-basic-multiple').select2({
        width: '150px'
    });   

});