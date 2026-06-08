var IdPedidos = 0;
var idCliente = 0;
var idCliente2 = 0;
var idClasificacion = 0;
var tipo = 0;
var IdPoliza = 0;
var chekRenovacionFiltrar = 0;

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

function ConfirmarEliminarArchivo(codigoItem, iconArchivo, nombreArchivo) {
    // Habilitar mensaje
    var mensaje = 'Desea continuar con la eliminación del archivo.';
    var mensaje = "Desea continuar con la eliminación del archivo <i class='fa " + iconArchivo + "'></i>&nbsp;(" + nombreArchivo + ").";
    $("#txtCodigoItem").val(codigoItem);
    $("#modalMensajeConfirmacioMensaje").html(mensaje);
    $("#modalMensajeConfirmacion").modal('show');

    return;
}

function NuevaPoliza() {
    BorrarBotonesPoliza();
    ListadoArchivosPoliza("#divArchivosAdjuntosAnteriores", 0);
    document.getElementById("IdListaPoliza").style.display = "none";
    document.getElementById("IdRegistroPoliza").style.display = "block";
    $("#txtFechaInicioPo").datepicker('setDate', 'today');
    $("#txtFechFinalPo").datepicker('setDate', 'today');
}

function ListaPoliza() {
    ObtenerConsultarPolizaDetalle(IdPedidos, 1);
    IdPoliza = 0;
    document.getElementById("IdListaPoliza").style.display = "block";
    document.getElementById("IdRegistroPoliza").style.display = "none";

}

function ObtenerListaClientes(tipo, descripcion,idproceso) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", idproceso);
}

function BuscarCliente() {

    if ($('#cboCliente').val().length > 4) {
        idSeleccionado = 0;
        ObtenerListaClientes(2, $('#cboCliente').val(),2);
    }
    else {
        document.getElementById("comboClientes").style.display = "none";
    }
}

function BuscarCliente2() {

    if ($('#cboCliente2').val().length > 4) {
        idSeleccionado = 0;
        ObtenerListaClientes(2, $('#cboCliente2').val(),4);
    }
    else {
        document.getElementById("comboClientes2").style.display = "none";
    }
}

function BuscarCliente3() {

    if ($('#cboCliente3').val().length > 4) {
        idSeleccionado = 0;
        ObtenerListaClientes(2, $('#cboCliente3').val(), 5);
    }
    else {
        document.getElementById("comboClientes3").style.display = "none";
    }
}

function BuscarCliente4() {

    if ($('#cboCliente4').val().length > 4) {
        idSeleccionado = 0;
        ObtenerListaClientes(2, $('#cboCliente4').val(), 6);
    }
    else {
        document.getElementById("comboClientes4").style.display = "none";
    }
}

function CargarCliente(ID, NOMBRE) {
    $('#cboCliente').val(NOMBRE);
    idCliente = ID;
    document.getElementById("comboClientes").style.display = "none";
}

function CargarCliente2(ID, NOMBRE) {
    $('#cboCliente2').val(NOMBRE);
    idCliente2 = ID;
    document.getElementById("comboClientes2").style.display = "none";
}

function CargarCliente3(ID, NOMBRE) {
    $('#cboCliente3').val(NOMBRE);
    idCliente2 = ID;
    document.getElementById("comboClientes3").style.display = "none";
}

function CargarCliente4(ID, NOMBRE) {
    $('#cboCliente4').val(NOMBRE);
    idCliente2 = ID;
    document.getElementById("comboClientes4").style.display = "none";
}

function BuscarClasificacion() {

    if ($('#cboClasificacion').val().length > 0) {
        idSeleccionado = 0;
        ObtenerListaClientes(3, $('#cboClasificacion').val(),3);
    }
    else {
        document.getElementById("comboClasificacion").style.display = "none";
    }
}

function CargarEspecialista(ID, NOMBRE) {
    $('#cboClasificacion').val(NOMBRE);
    idClasificacion = ID;
    document.getElementById("comboClasificacion").style.display = "none";
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

function time_format(d) {
    hours = format_two_digits(d.getHours());
    minutes = format_two_digits(d.getMinutes());
    seconds = format_two_digits(d.getSeconds());
    return hours + ":" + minutes + ":" + seconds;
}

function format_two_digits(n) {
    return n < 10 ? '0' + n : n;
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

function ObtenerListaComboClasificacionPedido() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"5\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboClasificacion', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboCliente() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"7\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboCliente', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuenta() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"4\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuentaGeneral() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"11\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuentaGeneral2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"11\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor3', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteProducto() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"6\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboClasificacionPedido2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"5\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboClasificacion2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboSucursal() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"8\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboSucursal', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboSucursal2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"8\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboSucursal2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboAnio() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"9\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboAnio', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboAnio2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"9\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboAnio1', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboMeses() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"10\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboMeses', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboMeses2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"10\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboMeses1', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerConsultarPedidos(idPedidos, orden, tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarPedidos\", \"parameters\" : { idPedidos: \"" + idPedidos + "\", orden: \"" + orden + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "detalleDatos", tipo);
}

function ObtenerConsultarPoliza(idPedidos, tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarPoliza\", \"parameters\" : { idPedidos: \"" + idPedidos + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "detalleDatosPoliza", tipo);
}

function ObtenerConsultarPolizaDetalle(idPedidos, tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarPoliza\", \"parameters\" : { idPedidos: \"" + idPedidos + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelect4", tipo);
}

function ObtenerConsultarPolizaDetalleFiltros(idPedidos, tipo, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ReporteConsultarPolizaFiltros\", \"parameters\" : { buscar: \"" + $("#txtBuscarProceso").val() + "\", fechaInicio: \"" + fechaInicio + "\", fechaFinal: \"" + fechaFinal + "\",idPedidos: \"" + idPedidos + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelect4", tipo);
}

function ObtenerConsultarPolizasDetalleFiltrosDescargar(idPedidos, tipo, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ReporteConsultarPolizasFiltrosDescargar\", \"parameters\" : { buscar: \"" + $("#txtBuscarProcesoPoliza").val() + "\", fechaInicio: \"" + fechaInicio + "\", fechaFinal: \"" + fechaFinal + "\",idPedidos: \"" + idPedidos + "\",beneficiario: \"" + $("#cboCliente4").val() + "\",Proceso: \"" + $("#cboEstadoPoliza").val() + "\",idFecha: \"" + $("#cboFechaPoliza").val() + "\"} }]";
    //CargarPagina('#datosTablaPrincipalPolizas', 'ObtenerListaTareas.ashx', Datos, "tableSelectPolizas", tipo);
    DetalleTareasDescargaXLS('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table");
}

function ObtenerConsultarPolizasDetalleFiltros(idPedidos, tipo, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ReporteConsultarPolizasFiltros\", \"parameters\" : { buscar: \"" + $("#txtBuscarProcesoPoliza").val() + "\", fechaInicio: \"" + fechaInicio + "\", fechaFinal: \"" + fechaFinal + "\",idPedidos: \"" + idPedidos + "\",beneficiario: \"" + $("#cboCliente4").val() + "\",Proceso: \"" + $("#cboEstadoPoliza").val() + "\",idFecha: \"" + $("#cboFechaPoliza").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipalPolizas', 'ObtenerListaTareas.ashx', Datos, "tableSelectPolizas", tipo);
}

function EnviarMailPolizasRegistradas(poliza) {
    var url = "AdministrarTarea.ashx";
    var Datos = "[{ \"action\": \"EnviarEmailPoliza\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", idpoliza: \"" + poliza + "\", numfactura: \"" + $("#txtNumFacturaEM").val() + "\", os: \"" + $("#txtOSEM").val() + "\",beneficiario: \"" + $("#txtBeneficiarioEM").val() + "\",pedidoPoliza: \"" + $("#txtPedidoPolizaEM").val() + "\",correo: \"" + $("#txtEmailEnvio").val() + "\",detalle: \"" + $("#txtDetalleMail").val() + "\"} }]";
    //CargarPagina('#datosTablaPrincipalPolizas', 'AdministrarTarea.ashx', Datos, "tableSelectPolizas", tipo);
    $.ajax({
        type: "POST",
        url: url,
        data: Datos,
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

function ObtenerConsultarPedidos2(idPedidos, orden, tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarPedidos\", \"parameters\" : { idPedidos: \"" + idPedidos + "\", orden: \"" + orden + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "detalleDatos2", tipo);
}

function ObtenerListaComboCliente2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"7\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboCliente2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteProducto2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"6\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteProductoUnico() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"21\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteProducto3() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"6\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente3', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuenta2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"4\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuentaUnico() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"19\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuenta3() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"4\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor3', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaPedidos(tipo, idRegistro, fechaInicio, fechaFinal) {

    //var comboSucursal = document.getElementById("cboSucursal2");
    //var selectedSucursal = comboSucursal.options[comboSucursal.selectedIndex].text;

    var Datos = "[{ \"action\": \"ReporteListaPedidos\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", busqueda: \"" + $("#txtNumeroOrden2").val() + "\", IdCliente: \"" + idCliente2 + "\", IdGerenteCuenta: \"" + $("#cboVendedor2").val() + "\", IdGestorProducto: \"" + $("#cboGerente2").val() + "\", sucursal: \"" + $("#cboSucursal2").val() + "\", estado: \"" + $("#cboEstado2").val() + "\" , IdClasificacion: \"" + $("#cboClasificacion2").val() + "\", Anio: \"" + $("#cboAnio").val() + "\", meses: \"" + $("#cboMeses").val() + "\", idFecha: \"" + $("#cboFecha").val() + "\", IdRenovacion: \"" + 0 + "\" } }]";
    //idCliente2 = 0;
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "tableSelect", tipo);
}

function ObtenerListaPedidosVisualizar(tipo, idRegistro, fechaInicio, fechaFinal) {

    var comboSucursal = document.getElementById("cboSucursal2");
    var selectedSucursal = comboSucursal.options[comboSucursal.selectedIndex].text;

    var Datos = "[{ \"action\": \"ReporteListaPedidos\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", busqueda: \"" + $("#txtNumeroOrden2").val() + "\", IdCliente: \"" + idCliente2 + "\", IdGerenteCuenta: \"" + $("#cboVendedor2").val() + "\", IdGestorProducto: \"" + $("#cboGerente2").val() + "\", sucursal: \"" + selectedSucursal + "\", estado: \"" + $("#cboEstado2").val() + "\" , IdClasificacion: \"" + $("#cboClasificacion2").val() + "\", Anio: \"" + $("#cboAnio").val() + "\", meses: \"" + $("#cboMeses").val() + "\", idFecha: \"" + $("#cboFecha").val() + "\", IdRenovacion: \"" + 0 + "\" } }]";
    //idCliente2 = 0;
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "tableSelect5", tipo);
}

function ObtenerListaPedidosRe(tipo, idRegistro, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ReporteListaPedidos\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", busqueda: \"" + "" + "\", IdCliente: \"" + "0" + "\", IdGerenteCuenta: \"" + "0" + "\", IdGestorProducto: \"" + "0" + "\", sucursal: \"" + $("#cboSucursal2").val() + "\", estado: \"" + $("#cboEstado2").val() + "\" , IdClasificacion: \"" + "0" + "\", Anio: \"" + $("#cboAnio").val() + "\", meses: \"" + $("#cboMeses").val() + "\", idFecha: \"" + 0 + "\", IdRenovacion: \"" + 0 + "\"  } }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "tableSelect", tipo);
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

function RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado) {

    var x = "";

    if (idSeleccionado == 2) {

        $.each(json, function (i, item) {     
            x = x + "<li><a role='option' onclick='CargarCliente(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboClientes").innerHTML = x;
        document.getElementById("comboClientes").style.display = "block";
    }
    else if (idSeleccionado == 4)
    {
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
    else if (idSeleccionado == 6) {
        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarCliente4(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboClientes4").innerHTML = x;
        document.getElementById("comboClientes4").style.display = "block";
    }
    else if (idSeleccionado == 3) {


        $.each(json, function (i, item) {    
            x = x + "<li><a role='option' onclick='CargarEspecialista(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboClasificacion").innerHTML = x;
        document.getElementById("comboClasificacion").style.display = "block";
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
        contenido = RecorreJSONTableSelect(json, boton);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectBusqueda") {
        contenido = RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelect2") {
        contenido = RecorreJSONTableSelect2(json, boton, idSeleccionado);
        if (boton > 0) {
            $('.nav-pills a[href="#Accion-pills"]').tab('show');
        }
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelect3") {
        contenido = RecorreJSONTableSelect3(json, boton, idSeleccionado);
        if (boton > 0) {
            $('.nav-pills a[href="#Total-pills"]').tab('show');
        }
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelect4") {
        contenido = RecorreJSONTableSelect4(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    if (tipoControl == "tableSelectPolizas") {
        contenido = RecorreJSONTableSelectPolizas(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    if (tipoControl == "tableSelect5") {
        contenido = RecorreJSONTableSelectVisualizar(json, boton);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectChange") {
        contenido = RecorreJSONTableSelectChange(json);
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
            BorrarBotonesPedidosMargen(1);
        }
    }

    if (tipoControl == "detalleDatosPoliza") {
        if (boton > 0) {
            contenido = RecorreDatosPantallaPoliza(json);
            $(div).html(contenido);
        }
        else {
            //BorrarBotonesPedidosMargen(1);
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

function RecorreJSONTableSelectVisualizar(json, idSeleccionado) {
    var info = "";
    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    //info = info + thInicial + "ACCIONES" + thFinal;
    //info = info + thInicial + "FECHA" + thFinal;
    info = info + thInicial + "PEDIDO" + thFinal;
    info = info + thInicial + "CLIENTE" + thFinal;
    info = info + thInicial + "SEGMENTACION" + thFinal;
    info = info + thInicial + "CLASIFICACION" + thFinal;
    info = info + thInicial + "DETALLE" + thFinal;
    info = info + thInicial + "VALOR PROCESO" + thFinal;
    info = info + thInicial + "MARGEN BRUTO" + thFinal;
    info = info + thInicial + "MARGEN" + thFinal;
    info = info + thInicial + "ESTADO" + thFinal;
    info = info + thInicial + "N_FACTURA" + thFinal;
    info = info + thInicial + "FECHA FACTURACION" + thFinal;
    info = info + thInicial + "FECHA ESTIMADA DE FACTURACION" + thFinal;
    //info = info + thInicial + "OBSERVACION" + thFinal;
    info = info + thInicial + "VENDEDOR" + thFinal;
    info = info + thInicial + "GERENTE DE PRODUCTO" + thFinal;
    //info = info + thInicial + "ORDEN DE COMPRA" + thFinal;
    //info = info + thInicial + "ORDEN DE SERVICIOS INTERNA" + thFinal;
    //info = info + thInicial + "ORDEN DE SERVICIOSEXTERNA" + thFinal;
    //info = info + thInicial + "ORDEN DE SERVICIOS DE FINANZAS" + thFinal;
    //info = info + thInicial + "SUCURSAL" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";
    var totalValor1 = 0;
    var totalValor2 = 0;
    var totalValor3 = 0;
    var esImpar = true;
    $.each(json, function (i, item) {

        info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.IdPedido + "'>";

        //info = info + "<td class='sorting_1' style='text-align:center'>";
        //info = info + "<i class='fa fa-eye' style='cursor: pointer' onclick='VerPedidos(\"" + item.IdPedido + "\");' title'Ver numero de contrato'></i>";
        //info = info + "</td>";
        //info = info + "<td class='sorting_1'>" + item.FECHA + "</td>";
        info = info + "<td class='sorting_1'>" + item.PEDIDO + "</td>";
        info = info + "<td class='sorting_1'>" + item.CLIENTE + "</td>";
        info = info + "<td class='sorting_1'>" + item.SEGMENTACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.CLASIFICACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.DETALLE + "</td>";
        info = info + "<td class='sorting_1'>" + "$ " + item.VALOR + "</td>";
        info = info + "<td class='sorting_1'>" + "$ " + item.RENTABILIDAD + "</td>";
        info = info + "<td class='sorting_1'>" + "% " + item.MARGEN + "</td>";


        if (item.ESTADO == "FACTURADO") {
            info = info + "<td class='sorting_1' style='background: " + "#92a88f" + ";text-align: center;'>" + item.ESTADO + "</td>";
        }
        else if (item.ESTADO == "POR FACTURAR") {
            info = info + "<td class='sorting_1' style='background: " + "#fe9c7d" + ";text-align: center;'>" + item.ESTADO + "</td>";
        }

        //info = info + "<td class='sorting_1'>" + item.ESTADO + "</td>";


        info = info + "<td class='sorting_1'>" + item.N_FACTURA + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA_FACTURACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA_ESTIMADA_DE_FACTURACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.OBSERVACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.VENDEDOR + "</td>";
        //info = info + "<td class='sorting_1'>" + item.GERENTE_DE_PRODUCTO + "</td>";
        //info = info + "<td class='sorting_1'>" + item.ORDEN_DE_COMPRA + "</td>";
        //info = info + "<td class='sorting_1'>" + item.ORDEN_DE_SERVICIOS_INTERNA + "</td>";
        //info = info + "<td class='sorting_1'>" + item.ORDEN_DE_SERVICIOS_EXTERNA + "</td>";
        //info = info + "<td class='sorting_1'>" + item.ORDEN_DE_SERVICIOS_DE_FINANZAS + "</td>";
        //info = info + "<td class='sorting_1'>" + item.SUCURSAL + "</td>";
        info = info + "</tr>";

        totalValor1 = totalValor1 + item.VALOR;
        totalValor2 = totalValor2 + item.RENTABILIDAD;
        totalValor3 = totalValor3 + item.MARGEN;

    });

    if (totalValor1 > 0 || totalValor2 > 0 || totalValor3 > 0) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='7' style='text-align: right;'><b>VALORES:</b></td>";
        info = info + "<td class='sorting_1'><b>" + "$ " + format_two_digits(totalValor1.toFixed(2)) + "</b></td>";
        info = info + "<td class='sorting_1'><b>" + "$ " + format_two_digits(totalValor2.toFixed(2)) + "</b></td>";
        info = info + "<td class='sorting_1'><b>" + "% " + format_two_digits((totalValor2 / totalValor3).toFixed(2)) + "</b></td>";
        info = info + "<td class='sorting_1' colspan='2'></td>";
        info = info + "</tr>";
    }

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
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

function RecorreJSONTableSelect4(json, boton, idSeleccionado) {
    var info = "";

    $("#listPrincipalTitleLabel").html("Detalle de Ticket");

    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "ACCIONES" + thFinal;
    info = info + thInicial + "Codigo Poliza" + thFinal;
    info = info + thInicial + "Num. Factura" + thFinal;
    info = info + thInicial + "Num. Poliza" + thFinal;
    info = info + thInicial + "Fecha Inicio" + thFinal;
    info = info + thInicial + "Fecha Final" + thFinal;
    info = info + thInicial + "Monto" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";

    var esImpar = true;

    var fechaSumaTiempo = "";
    var tiempoTotalDia = 0;
    var tiempoTareaDia = 0;
    var fechaDia = "";

    $.each(json, function (i, item) {
        //
        // Despliegue de datos
        // 
        
        info = info + "<td class='sorting_1' style='text-align:center'>";
        info = info + "<i class='fa fa-eye' style='cursor: pointer' onclick='CargarIdPolizas(\"" + item.IdPoliza + "\");' title'Ver por Numero de Orden'></i>";
        info = info + "&nbsp;&nbsp;";
        info = info + "</td>";

        info = info + "<td class='sorting_1'>" + item.IdPoliza + "</td>";
        info = info + "<td class='sorting_1'>" + item.NumFactura + "</td>";
        info = info + "<td class='sorting_1'>" + item.NumPoliza + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaInicio + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaFin + "</td>";
        info = info + "<td class='sorting_1'>" + "$ " + item.Monto  + "</td>";
        info = info + "</tr>";
    });
    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function RecorreJSONTableSelectPolizas(json, boton, idSeleccionado) {
    var info = "";

    if (json.length == 0) {
        alerta("No existe una poliza registrada");
    }
    else {
        var esImpar = true;
        var iniciaBarrido = true;
        var totalValor3 = 0;

        //
        // Despliegue de titulos de cabecera
        //
        var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;text-align:center;'>";
        var thFinal = "</th>";
        info = info + "<table id='tbl_metas' width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
        info = info + "<thead><tr role='row'>";
        info = info + thInicial + "--ACCIONES--" + thFinal;
        info = info + thInicial + "COD. POLIZA" + thFinal;
        info = info + thInicial + "ESTADO" + thFinal;
        //info = info + thInicial + "COD. PEDIDO" + thFinal;
        info = info + thInicial + "FACTURA" + thFinal;
        //info = info + thInicial + "VALOR FACTURA" + thFinal;
        //info = info + thInicial + "OC" + thFinal;
        info = info + thInicial + "OS" + thFinal;
        info = info + thInicial + "PEDIDO" + thFinal;
        info = info + thInicial + "POLIZA" + thFinal;
        info = info + thInicial + "ANEXO" + thFinal;
        info = info + thInicial + "BENEFICIARIO" + thFinal;
        //info = info + thInicial + "EMISION" + thFinal;
        info = info + thInicial + "INICIO" + thFinal;
        info = info + thInicial + "VENCIMIENTO" + thFinal;
        info = info + thInicial + "VALOR" + thFinal;
        info = info + thInicial + "OBJETO" + thFinal;
        info = info + thInicial + "TIPO POLIZA" + thFinal;
        info = info + thInicial + "USUARIO REGISTRO" + thFinal;
        //info = info + thInicial + "FECHA REGISTRO" + thFinal;
        info = info + "</tr></thead>";

        info = info + "<tbody>";

        $.each(json, function (i, item) {
            //
            // Despliegue de datos
            // 
            //info = info + "<td class='sorting_1'>" + item.IdPoliza + "</td>";
            info = info + "<td class='sorting_1' style='text-align:center'>";
            //info = info + "<i class='fa fa-eye' style='cursor: pointer' onclick='VerPedidos(\"" + item.IdPedido + "\");' title'Ver numero de contrato'></i>";
            //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            //info = info + "<i class='fa fa-pencil-square-o' style='cursor: pointer' aria-hidden='true' title'Editar la Observación' onclick='EditarObservacion(\"" + item.IdPedido + "\");'></i>";
            //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            info = info + "<i class='fa fa-eye' style='cursor: pointer' aria-hidden='true' title='Ver las polizas' onclick='CargarIdPolizasLista(\"" + item.IdPoliza + "\",\"" + item.IdPedido + "\");'></i>";
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            if (item.conteoArchivosAdjuntos != '0') {
                info = info + "<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosContrato(\"#MensajeInformativo\", \"" + item.IdPoliza + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
            }
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            info = info + "<i class='fa fa-file-text-o' style='cursor: pointer' aria-hidden='true' title='Registrar nueva poliza' onclick='GenerarPolizaNuevo(\"" + item.IdPedido + "\",\"" + item.OS + "\",\"" + item.BENEFICIARIO + "\",\"" + item.PEDIDO + "\");'></i>";
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            info = info + "<i class='fa fa-envelope' style='cursor: pointer' aria-hidden='true' title='Enviar Mail de la Poliza' onclick='EnviarMail(\"" + item.IdPoliza + "\",\"" + item.NumFactura + "\",\"" + item.OS + "\",\"" + item.BENEFICIARIO + "\",\"" + item.PEDIDO + "\");'></i>";
            info = info + "</td>";
            if (item.Proceso == "ACTIVO") {
                info = info + "<td class='sorting_1' style='background: " + "#92a88f" + ";text-align: center;'>" + item.IdPoliza + "</td>";
            }
            else if (item.Proceso == "ANULADO" || item.Proceso == "VENCIDA" ) {
                info = info + "<td class='sorting_1' style='background: " + "#fe9c7d" + ";text-align: center;'>" + item.IdPoliza + "</td>";
            }
            else if (item.Proceso == "POR VENCER") {
                info = info + "<td class='sorting_1' style='background: " + "#ffff33" + ";text-align: center;'>" + item.IdPoliza + "</td>";
            }
            //info = info + "<td class='sorting_1'>" + item.IdPedido + "</td>";
            info = info + "<td class='sorting_1'>" + item.Proceso + "</td>";
            info = info + "<td class='sorting_1'>" + item.NumFactura + "</td>";
            //info = info + "<td class='sorting_1'>" + "$ "+ item.Monto + "</td>";
            //info = info + "<td class='sorting_1'>" + item.OC + "</td>";
            info = info + "<td class='sorting_1'>" + item.OS + "</td>";
            info = info + "<td class='sorting_1'>" + item.PEDIDO + "</td>";
            info = info + "<td class='sorting_1'>" + item.NumPoliza + "</td>";
            info = info + "<td class='sorting_1'>" + item.ANEXO + "</td>";
            info = info + "<td class='sorting_1'>" + item.BENEFICIARIO + "</td>";
            //info = info + "<td class='sorting_1'>" + item.EMISION + "</td>";
            info = info + "<td class='sorting_1'>" + item.FechaInicio + "</td>";
            info = info + "<td class='sorting_1'>" + item.FechaFin + "</td>";
            info = info + "<td class='sorting_1'>" + "$ " + item.VALOR + "</td>";
            info = info + "<td class='sorting_1'>" + item.OBJETO + "</td>";
            info = info + "<td class='sorting_1'>" + item.TipoPoliza + "</td>";
            info = info + "<td class='sorting_1'>" + item.UsuarioRegistro + "</td>";

            info = info + "</tr>";
        });

        info = info + "</tbody>";
        info = info + "</table>";
    }
    return info;
}

function RecorreJSONTableSelect(json, idSeleccionado) {
    var info = "";
    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "--ACCIONES--" + thFinal;
    info = info + thInicial + "FECHA" + thFinal;
    info = info + thInicial + "PEDIDO" + thFinal;
    info = info + thInicial + "CLIENTE" + thFinal;
    info = info + thInicial + "SEGMENTACION" + thFinal;
    info = info + thInicial + "CLASIFICACION" + thFinal;
    info = info + thInicial + "DETALLE" + thFinal;
    info = info + thInicial + "VALOR PROCESO" + thFinal;
    info = info + thInicial + "MARGEN BRUTO" + thFinal;
    info = info + thInicial + "MARGEN" + thFinal;
    info = info + thInicial + "ESTADO" + thFinal;
    info = info + thInicial + "N_FACTURA" + thFinal;
    info = info + thInicial + "FECHA FACTURACION" + thFinal;
    info = info + thInicial + "FECHA ESTIMADA DE FACTURACION" + thFinal;
    info = info + thInicial + "OBSERVACION" + thFinal;
    info = info + thInicial + "VENDEDOR" + thFinal;
    info = info + thInicial + "GERENTE DE PRODUCTO" + thFinal;
    info = info + thInicial + "ORDEN DE COMPRA" + thFinal;
    info = info + thInicial + "ORDEN DE SERVICIOS INTERNA" + thFinal;
    info = info + thInicial + "ORDEN DE SERVICIOSEXTERNA" + thFinal;
    info = info + thInicial + "ORDEN DE SERVICIOS DE FINANZAS" + thFinal;
    info = info + thInicial + "SUCURSAL" + thFinal;
    info = info + thInicial + "ESTADO RENOV" + thFinal;
    info = info + thInicial + "FECHA DESDE RENOV." + thFinal;
    info = info + thInicial + "FECHA HASTA RENOV" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";
    var totalValor1 = 0;
    var totalValor2 = 0;
    var totalValor3 = 0;
    var esImpar = true;
    $.each(json, function (i, item) {

        info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.IdPedido + "'>";

        info = info + "<td class='sorting_1' style='text-align:center'>";
        info = info + "<i class='fa fa-eye' style='cursor: pointer' onclick='VerPedidos(\"" + item.IdPedido + "\");' title'Ver numero de contrato'></i>";       
        info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        info = info + "<i class='fa fa-pencil-square-o' style='cursor: pointer' aria-hidden='true' title'Editar la Observación' onclick='EditarObservacion(\"" + item.IdPedido + "\");'></i>";
        info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        info = info + "<i class='fa fa-minus-square' style='cursor: pointer' aria-hidden='true' title='Ver las polizas' onclick='BtnConsultaPolizasListas(\"" + item.IdPedido + "\");'></i>";
        info = info + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA + "</td>";
        info = info + "<td class='sorting_1'>" + item.PEDIDO + "</td>";
        info = info + "<td class='sorting_1'>" + item.CLIENTE + "</td>";
        info = info + "<td class='sorting_1'>" + item.SEGMENTACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.CLASIFICACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.DETALLE + "</td>";
        info = info + "<td class='sorting_1'>" + "$ " + item.VALOR + "</td>";
        info = info + "<td class='sorting_1'>" + "$ " + item.RENTABILIDAD + "</td>";
        info = info + "<td class='sorting_1'>" + "% " + item.MARGEN + "</td>";


        if (item.ESTADO == "FACTURADO") {
            info = info + "<td class='sorting_1' style='background: " + "#92a88f" + ";text-align: center;'>" + item.ESTADO + "</td>";
        }
        else if (item.ESTADO == "POR FACTURAR") {
            info = info + "<td class='sorting_1' style='background: " + "#fe9c7d" + ";text-align: center;'>" + item.ESTADO + "</td>";
        }

        //info = info + "<td class='sorting_1'>" + item.ESTADO + "</td>";


        info = info + "<td class='sorting_1'>" + item.N_FACTURA + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA_FACTURACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA_ESTIMADA_DE_FACTURACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.OBSERVACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.VENDEDOR + "</td>";
        info = info + "<td class='sorting_1'>" + item.GERENTE_DE_PRODUCTO + "</td>";
        info = info + "<td class='sorting_1'>" + item.ORDEN_DE_COMPRA + "</td>";
        info = info + "<td class='sorting_1'>" + item.ORDEN_DE_SERVICIOS_INTERNA + "</td>";
        info = info + "<td class='sorting_1'>" + item.ORDEN_DE_SERVICIOS_EXTERNA + "</td>";
        info = info + "<td class='sorting_1'><a onclick='GenerarPoliza(\"" + item.IdPedido + "\",\"" + item.ORDEN_DE_SERVICIOS_DE_FINANZAS + "\",\"" + item.CLIENTE + "\",\"" + item.PEDIDO + "\");'>" + item.ORDEN_DE_SERVICIOS_DE_FINANZAS + "</a></td>";
        info = info + "<td class='sorting_1'>" + item.SUCURSAL + "</td>";
        info = info + "<td class='sorting_1'>" + item.ChekRenovacion  + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaInicioR  + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaFinalR  + "</td>";
        info = info + "</tr>";

        totalValor1 = totalValor1 + item.VALOR;
        totalValor2 = totalValor2 + item.RENTABILIDAD;
        totalValor3 = totalValor3 + item.MARGEN;

    });

    if (totalValor1 > 0 || totalValor2 > 0 || totalValor3 > 0 ) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='7' style='text-align: right;'><b>VALORES:</b></td>";
        info = info + "<td class='sorting_1'><b>" + "$ " + format_two_digits(totalValor1.toFixed(2)) + "</b></td>";
        info = info + "<td class='sorting_1'><b>" + "$ " + format_two_digits(totalValor2.toFixed(2)) + "</b></td>";
        info = info + "<td class='sorting_1'><b>" + "% " + format_two_digits((totalValor2 / totalValor1).toFixed(2)) + "</b></td>";
        info = info + "<td class='sorting_1' colspan='2'></td>";
        info = info + "</tr>";
    }

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function RecorreDatosPantalla(json) {

    $.each(json, function (i, item) {

        //$('#cboVendedor').empty();
        var combo4 = document.getElementById("cboSucursal");
        combo4.value = item.IdSucursal;
        //var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"4\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"" + item.IdSucursal + "\"} }]";
        //CargarPagina('#cboVendedor', 'ObtenerListaTareas.ashx', Datos, "select", "", "");


        $('#txtFechaDesde').val(item.FECHA);
        $('#txtPedido').val(item.PEDIDO);

        //var combo1 = document.getElementById("cboCliente");
        //combo1.value = item.IdCliente;

        $('#cboCliente').val(item.CLIENTE);

        var combo2 = document.getElementById("cboSegmentacion");
        combo2.value = item.SEGMENTACION;

        //var combo3 = document.getElementById("cboClasificacion");
        //combo3.value = item.IdClasificacion;

        $('#cboClasificacion').val(item.CLASIFICACION);

        $('#txtValor').val(item.VALOR);
        $('#txtRentabilidad').val(item.RENTABILIDAD);
        $('#txtMargen').val(item.MARGEN);

        var combo4 = document.getElementById("cboEstado");
        combo4.value = item.ESTADO;

        $('#txtDetalle').val(item.DETALLE);
        $('#txtNFactura').val(item.N_FACTURA);
        $('#txtFechaFacturacion').val(item.FECHA_FACTURACION);
        $('#txtFechaEstimadaFacturacion').val(item.FECHA_ESTIMADA_DE_FACTURACION);


        $('#txtObservacion').val(item.OBSERVACION);

        var combo6 = document.getElementById("cboGerente");
        combo6.value = item.IdGerenteProducto;

        $('#txtOrdenCompra').val(item.ORDEN_DE_COMPRA);
        $('#txtOrdenServicioInterno').val(item.ORDEN_DE_SERVICIOS_INTERNA);
        $('#txtOrdenServicioExterno').val(item.ORDEN_DE_SERVICIOS_EXTERNA);
        $('#txtOrdenServicioFinanza').val(item.ORDEN_DE_SERVICIOS_DE_FINANZAS);    

        //$('#cboVendedor').empty();
        //ObtenerListaComboGerenteCuentaGeneral();

        var combo5 = document.getElementById("cboVendedor");
        combo5.value = item.IdVendedor;

        if (item.ChekRenovacion == 1 && $("#ContentPlaceHolder1_txtLoginUsuario").val() == "mbastidas") {
            document.getElementById("fecha1").style.display = "block";
            document.getElementById("fecha2").style.display = "block";

            $('#txtFechaDesdeRenova').val(item.FechaInicioR);
            $('#txtFechaHastaRenova').val(item.FechaFinalR);
            document.getElementById("idRenovacion").checked = true

        }
        else if (item.ChekRenovacion == 0) {
            document.getElementById("fecha1").style.display = "none";
            document.getElementById("fecha2").style.display = "none";
            $('#txtFechaDesdeRenova').val("");
            $('#txtFechaHastaRenova').val("");
            document.getElementById("idRenovacion").checked = false
        }


    });
}

function RecorreDatosPantalla2(json) {

    $.each(json, function (i, item) {

        $('#txtPedido2').val(item.PEDIDO);
        $('#txtValor2').val(item.VALOR);
        $('#txtRentabilidad2').val(item.RENTABILIDAD);
        $('#txtMargen2').val(item.MARGEN);
        var combo6 = document.getElementById("cboGerente3");
        combo6.value = item.IdGerenteProducto;
        var combo5 = document.getElementById("cboVendedor3");
        combo5.value = item.IdVendedor;

    });
}

function RecorreDatosPantallaPoliza(json) {

    $.each(json, function (i, item) {
        IdPoliza = item.IdPoliza;
        $('#txtNumFactura').val(item.NumFactura);
        $('#txtPoliza').val(item.NumPoliza);
        $('#txtFechaInicioPo').val(item.FechaInicio);
        $('#txtFechFinalPo').val(item.FechaFin);
        $('#txtValorMonto').val(item.VALOR);
        $('#txtNumFactura').val(item.NumFactura);
        $('#txtObjetivo').val(item.OBJETO);
        $('#txtOS').val(item.OS);
        $('#txtAnexo').val(item.ANEXO);
        $('#txtBeneficiario').val(item.BENEFICIARIO);
        $('#txtPedidoPoliza').val(item.PEDIDO);
        var combo2 = document.getElementById("cboPoliza");
        combo2.value = item.TipoPoliza;

        var combo3 = document.getElementById("cboEstadoPoliza2");
        combo3.value = item.Proceso;

    });
}

function ObtenerListaPedidosDescarga(tipo, idRegistro, fechaInicio, fechaFinal) {

    //var comboSucursal = document.getElementById("cboSucursal2");
    //var selectedSucursal = comboSucursal.options[comboSucursal.selectedIndex].text;

    var Datos = "[{ \"action\": \"ListaPedidosDescargaXLS\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", busqueda: \"" + $("#txtNumeroOrden2").val() + "\", IdCliente: \"" + idCliente2 + "\", IdGerenteCuenta: \"" + $("#cboVendedor2").val() + "\", IdGestorProducto: \"" + $("#cboGerente2").val() + "\", sucursal: \"" + $("#cboSucursal2").val() + "\", estado: \"" + $("#cboEstado2").val() + "\", IdClasificacion: \"" + $("#cboClasificacion2").val() + "\", Anio: \"" + $("#cboAnio").val() + "\", meses: \"" + $("#cboMeses").val() + "\", idFecha: \"" + $("#cboFecha").val() + "\", IdRenovacion: \"" + 0 + "\"} }]";
    idCliente2 = 0;
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

function GuardarNuevoPedido() {
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;


    if ($('#txtFechaDesde').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha de registro ";
        contadorVerificacion += 1;
    }

    if ($('#txtPedido').val() == "") {
        mensajeVerificacion += "- Debe ingresar el numero de Pedido ";
        contadorVerificacion += 1;
    }

    if ($('#cboCliente').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre del cliente ";
        contadorVerificacion += 1;
    }
    else {
        var combooCliente = $('#cboCliente').val();
        var selectedCliente = $('#cboCliente').val();
    }

    if ($('#cboSegmentacion').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar la segmentación ";
        contadorVerificacion += 1;
    }

    if ($('#cboClasificacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar el numero de clasificacion ";
        contadorVerificacion += 1;
    }
    else {
        var combooClasificacion = $('#cboClasificacion').val();
        var selectedoClasificacion = $('#cboClasificacion').val();
    }

    if ($('#txtValor').val() == "") {
        mensajeVerificacion += "- Debe ingresar el valor ";
        contadorVerificacion += 1;
    }

    if ($('#txtMargen').val() == "") {
        mensajeVerificacion += "- Debe ingresar el marge ";
        contadorVerificacion += 1;
    }

    if ($('#txtRentabilidad').val() == "") {
        mensajeVerificacion += "- Debe ingresar la rentabilidad ";
        contadorVerificacion += 1;
    }

    if ($('#cboEstado').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar el estado ";
        contadorVerificacion += 1;
    }

    if ($('#txtDetalle').val() == "") {
        mensajeVerificacion += "- Debe ingresar el detalle ";
        contadorVerificacion += 1;
    }

    //if ($('#txtNFactura').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el Número de Factura ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtFechaFacturacion').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la fecha de facturación ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtFechaEstimadaFacturacion').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la fecha estimada de facturación ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtFechaEstimadaFacturacion').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la fecha estimada de facturación ";
    //    contadorVerificacion += 1;
    //}

    if ($('#cboSucursal').val() == "0") {
        mensajeVerificacion += "- Debe ingresar la secursal ";
        contadorVerificacion += 1;
    }
    else {
        var combooSucursal = document.getElementById("cboSucursal");
        var selectedoSucursal = combooSucursal.options[combooSucursal.selectedIndex].text;
    }

    if ($('#txtObservacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la observación ";
        contadorVerificacion += 1;
    }


    if ($('#cboVendedor').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el vendedor ";
        contadorVerificacion += 1;
    }
    else {
        var comboVendedor = document.getElementById("cboVendedor");
        var selectedoVendedor = comboVendedor.options[comboVendedor.selectedIndex].text;
    }

    if ($('#cboGerente').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el gerente de cuenta ";
        contadorVerificacion += 1;
    }
    else {
        var combooGerente = document.getElementById("cboGerente");
        var selectedoGerente = combooGerente.options[combooGerente.selectedIndex].text;
    }

    if ($('#txtOrdenCompra').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Orden Compra ";
        contadorVerificacion += 1;
    }

    if ($('#txtOrdenServicioInterno').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Orden Servicio Interno ";
        contadorVerificacion += 1;
    }

    if ($('#txtOrdenServicioExterno').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Orden Servicio Externo ";
        contadorVerificacion += 1;
    }

    if ($('#txtOrdenServicioFinanza').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Orden Servicio Finanza ";
        contadorVerificacion += 1;
    }

    var text = document.getElementById("idRenovacion");
    var checkIngreso = 0;
    if (text.checked == true &&  $("#ContentPlaceHolder1_txtLoginUsuario").val() == "mbastidas") {
        checkIngreso = 1;
        if ($('#txtFechaDesdeRenova').val() == "" && $('#txtFechaHastaRenova').val() == "" ) {
            mensajeVerificacion += "- Debe ingresar la fecha de desde y hasta de la renovacion ";
            contadorVerificacion += 1;
        }
    }
  


    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";


    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtPedido': '" + $('#txtPedido').val() + "',";
    datosFormulario = datosFormulario + "'cboCliente': '" + selectedCliente + "',";
    datosFormulario = datosFormulario + "'cboSegmentacion': '" + $('#cboSegmentacion').val() + "',";
    datosFormulario = datosFormulario + "'txtClasificacion': '" + selectedoClasificacion + "',";
    datosFormulario = datosFormulario + "'txtValor': '" + $('#txtValor').val() + "',";
    datosFormulario = datosFormulario + "'txtRentabilidad': '" + $('#txtRentabilidad').val() + "',";
    datosFormulario = datosFormulario + "'txtMargen': '" + $('#txtMargen').val().replace("%","") + "',";
    datosFormulario = datosFormulario + "'cboEstado': '" + $('#cboEstado').val() + "',";
    datosFormulario = datosFormulario + "'txtDetalle': '" + $('#txtDetalle').val() + "',";
    datosFormulario = datosFormulario + "'txtNFactura': '" + $('#txtNFactura').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaFacturacion': '" + $('#txtFechaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEstimadaFacturacion': '" + $('#txtFechaEstimadaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtSucursal': '" + selectedoSucursal + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    datosFormulario = datosFormulario + "'cboVendedor': '" + selectedoVendedor + "',";
    datosFormulario = datosFormulario + "'cboGerente': '" + selectedoGerente + "',";
    datosFormulario = datosFormulario + "'txtOrdenCompra': '" + $('#txtOrdenCompra').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioInterno': '" + $('#txtOrdenServicioInterno').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioExterno': '" + $('#txtOrdenServicioExterno').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "0" + "',";
    datosFormulario = datosFormulario + "'IdPedido': '" + "0" + "',";
    datosFormulario = datosFormulario + "'txtFechaDesdeRenova': '" + $('#txtFechaDesdeRenova').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaHastaRenova': '" + $('#txtFechaHastaRenova').val() + "',";
    datosFormulario = datosFormulario + "'checkIngreso': '" + checkIngreso + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioFinanza': '" + $('#txtOrdenServicioFinanza').val() + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoPedido', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesPedidos(0);
                //ObtenerListaPedidosRe(0, 0, $("#txtFechaConsulta1").val(), $("#txtFechaConsulta2").val());
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

function VisualizarPoliza() {
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

    ObtenerConsultarPolizaDetalleFiltros(IdPedidos,0, fechaInicio, fechaFinal);
}

function BtnVisualizar() {
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

    ObtenerListaPedidosVisualizar(0, 0, fechaInicio, fechaFinal);
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

    ObtenerListaPedidos(0, 0, fechaInicio, fechaFinal);
}

function BtnConsultaPoliza() {
    IdPedidos = 0;
    var fechaInicio = "";
    var fechaFinal = "";
    var text = document.getElementById("idFiltrosPoliza");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaBusInicioPoliza").val();
        fechaFinal = $("#txtFechaBusFinalPoliza").val();
    }

    ObtenerConsultarPolizasDetalleFiltros(IdPedidos, 0, fechaInicio, fechaFinal);
}

function BtnConsultaPolizaLista() {
    var fechaInicio = "";
    var fechaFinal = "";
    document.getElementById("RegistroPoliza").style.display = "none";
    document.getElementById("ListaPedido").style.display = "block";
    ObtenerConsultarPolizasDetalleFiltros(IdPedidos, 0, fechaInicio, fechaFinal);
    $('.nav-pills a[href="#polizas-pills"]').tab('show');
}

function BtnConsultaPolizasListas(id) {
    var fechaInicio = "";
    var fechaFinal = "";
    document.getElementById("RegistroPoliza").style.display = "none";
    document.getElementById("ListaPedido").style.display = "block";
    ObtenerConsultarPolizasDetalleFiltros(id, 0, fechaInicio, fechaFinal);
    $('.nav-pills a[href="#polizas-pills"]').tab('show');
}

function BtnDescargaPoliza() {

    var fechaInicio = "";
    var fechaFinal = "";
    var text = document.getElementById("idFiltrosPoliza");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaBusInicioPoliza").val();
        fechaFinal = $("#txtFechaBusFinalPoliza").val();
    }

    ObtenerConsultarPolizasDetalleFiltrosDescargar(IdPedidos, 0, fechaInicio, fechaFinal);

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

    ObtenerListaPedidosDescarga(0, 0, fechaInicio, fechaFinal);

}

function GenerarPoliza(IdPedido, OSFinanza, cliente, pedido) {
    BorrarBotonesPolizaNuevo();
    IdPoliza = 0;
    IdPedidos = IdPedido;
    $('#txtOS').val(OSFinanza);
    $('#txtBeneficiario').val(cliente);
    $('#txtPedidoPoliza').val(pedido);
    ListadoArchivosPoliza("#divArchivosAdjuntosAnteriores", 0);
    document.getElementById("RegistroPoliza").style.display = "block";
    document.getElementById("ListaPedido").style.display = "none";
}


function GenerarPolizaNuevo(IdPedido, OSFinanza, cliente, pedido) {
    BorrarBotonesPolizaNuevo();
    IdPoliza = 0;
    IdPedidos = IdPedido;
    $('#txtOS').val(OSFinanza);
    $('#txtBeneficiario').val(cliente);
    $('#txtPedidoPoliza').val(pedido);
    ListadoArchivosPoliza("#divArchivosAdjuntosAnteriores", 0);
    document.getElementById("RegistroPoliza").style.display = "block";
    document.getElementById("ListaPedido").style.display = "none";
    $('.nav-pills a[href="#profile-pills"]').tab('show');
}

function CancelarCambios() {
    document.getElementById("RegistroPoliza").style.display = "none";
    document.getElementById("ListaPedido").style.display = "block";
}

function CancelarEnvioMail() {
    document.getElementById("ListaRegistroPoliza").style.display = "block";
    document.getElementById("EnvioMail").style.display = "none";
}

function VerPedidos(IdPedido) {
    ObtenerConsultarPedidos(IdPedido, 0, 1)
    IdPedidos = IdPedido;
    $('.nav-pills a[href="#home-pills"]').tab('show');
}

function EditarObservacion(IdPedido) {
    tipo = 5;
    IdPedidos = IdPedido;
    //document.getElementById("IdPoliza").style.display = "none";
    //document.getElementById("IdCliente").style.display = "block";
    document.getElementById("modalMensajeAprobarLabelTitulo").innerHTML = "Editar Observación";
    $("#modalMensajeAprobar").modal('show');
}

function CargarPoliza(IdPedido, orden) {
    IdPedidos = IdPedido;
    ListaPoliza();
    document.getElementById("modalMensajeAprobarLabelTitulo").innerHTML = "Registrar Poliza";

    $("#modalMensajePoliza").modal('show');
}

function CargarIdPolizas(IdPolizas) {
    IdPoliza = IdPolizas;
    ObtenerConsultarPoliza(IdPoliza, 2);
    ListadoArchivosPoliza("#divArchivosAdjuntosAnteriores", IdPoliza);
    document.getElementById("RegistroPoliza").style.display = "block";
    document.getElementById("ListaPedido").style.display = "none";
    $('.nav-pills a[href="#profile-pills"]').tab('show');
}

function CargarIdPolizasLista(IdPolizas,idPedido) {
    IdPoliza = IdPolizas;
    IdPedidos = idPedido;
    ObtenerConsultarPoliza(IdPoliza, 2);
    ListadoArchivosPoliza("#divArchivosAdjuntosAnteriores", IdPoliza);
    document.getElementById("RegistroPoliza").style.display = "block";
    document.getElementById("ListaPedido").style.display = "none";
    $('.nav-pills a[href="#profile-pills"]').tab('show');
}

function EnviarMail(id, NumFactura, OSFinanza, cliente, pedido) {
    IdPoliza = id;
    document.getElementById("ListaRegistroPoliza").style.display = "none";
    document.getElementById("EnvioMail").style.display = "block";
    $('#txtNumFacturaEM').val(NumFactura);
    $('#txtOSEM').val(OSFinanza);
    $('#txtBeneficiarioEM').val(cliente);
    $('#txtPedidoPolizaEM').val(pedido);  
}

function GuardarEnvioCorreo() {

    if ($('#txtEmailEnvio').val() == "")
    {
        alerta("Debe agregar el correo del remitente");
    }
    else
    {
        if ($('#txtDetalleMail').val() == "")
        {
            alerta("Debe agregar una breve descripción");
        }
        else
        {
            EnviarMailPolizasRegistradas(IdPoliza);
        }

    }
}

function BorrarBotonesPedidos2() {
    var combo1 = document.getElementById("cboAnio1");
    combo1.selectedIndex = 0;
    var combo1 = document.getElementById("cboMeses1");
    combo1.selectedIndex = 0;
}

function BorrarBotonesPoliza() {
    $('#txtNumFactura').val("");
    $('#txtPoliza').val("");
    $('#txtValorMonto').val("");
    $('#txtBuscarProceso').val("");
    $('#txtOS').val("");
    $('#txtPedidoPoliza').val("");
    $('#txtAnexo').val("");
    $('#txtValor').val("");
    $('#txtBeneficiario').val("");
    $('#txtObjetivo').val("");
    var combo2 = document.getElementById("cboPoliza");
    combo2.value = "SELECCIONAR";
}

function BorrarBotonesPolizaNuevo() {
    $('#txtNumFactura').val("");
    $('#txtPoliza').val("");
    $('#txtValorMonto').val("");
    $('#txtBuscarProceso').val("");
    $('#txtAnexo').val("");
    $('#txtValor').val("");
    $('#txtObjetivo').val("");
    var combo2 = document.getElementById("cboPoliza");
    combo2.value = "SELECCIONAR";
}

function BorrarBotonesPedidosMargen(tipo) {


    var combo5 = document.getElementById("cboVendedor3");
    combo5.selectedIndex = 0;

    var combo6 = document.getElementById("cboGerente3");
    combo6.selectedIndex = 0;


    idCliente2 = 0;
    $('#txtValor2').val("");
    $('#txtRentabilidad2').val("");
    $('#txtMargen2').val("");
    $('#cboCliente3').val("");
   
}

function BorrarBotonesPedidos(tipo) {

    if (tipo == 0) {
        $('#txtPedido').val("");
    }

    var combo1 = document.getElementById("cboCliente");
    combo1.selectedIndex = 0;

    var combo2 = document.getElementById("cboSegmentacion");
    combo2.selectedIndex = 0;

    var combo3 = document.getElementById("cboEstado");
    combo3.selectedIndex = 0;

    var combo4 = document.getElementById("cboSucursal");
    combo4.selectedIndex = 0;

    var combo5 = document.getElementById("cboVendedor");
    combo5.selectedIndex = 0;

    var combo6 = document.getElementById("cboGerente");
    combo6.selectedIndex = 0;

    var combo7 = document.getElementById("cboClasificacion");
    combo7.selectedIndex = 0;


    $('#txtValor').val("");
    $('#txtRentabilidad').val("");
    $('#txtMargen').val("");
    $('#txtDetalle').val("");
    $('#txtNFactura').val("");
    $('#txtFechaFacturacion').val("");
    $('#txtFechaEstimadaFacturacion').val("");
    $('#txtObservacion').val("");
    $('#txtOrdenCompra').val("");
    $('#txtOrdenServicioInterno').val("");
    $('#txtOrdenServicioExterno').val("");
    $('#txtOrdenServicioFinanza').val("");

    document.getElementById("fecha1").style.display = "none";
    document.getElementById("fecha2").style.display = "none";
    var text = document.getElementById("idRenovacion");
    text.checked = false;
    $('#txtFechaDesdeRenova').val("");
    $('#txtFechaHastaRenova').val("");
}

function BuscarPedido(pedido) {

    if (pedido.length == 4 || pedido.length == 5) {
        ObtenerConsultarPedidos(0, pedido, 2)
    }
    else {
        BorrarBotonesPedidos(1);
    }
}

function BuscarPedido2(pedido) {

    if (pedido.length == 4 || pedido.length == 5) {
        ObtenerConsultarPedidos2(0, pedido, 2)
    }
    else {
        BorrarBotonesPedidosMargen(1);
    }
}

function BuscarEstado() {

    //var combooEstado = document.getElementById("cboEstado");
    //var selectedoEstado = combooEstado.options[combooEstado.selectedIndex].text;

    //if (selectedoEstado == "FACTURADO") {
    //    $("#txtNFactura").prop("disabled", false);
    //    $("#txtFechaFacturacion").prop("disabled", false);
    //    $("#txtFechaEstimadaFacturacion").prop("disabled", true);   

    //    $('#txtFechaEstimadaFacturacion').val("");
    //}
    //else if(selectedoEstado == "POR FACTURAR") {
    //    $("#txtNFactura").prop("disabled", true);
    //    $("#txtFechaFacturacion").prop("disabled", true);

    //    $('#txtFechaFacturacion').val("");

    //    $("#txtFechaEstimadaFacturacion").prop("disabled", false);   
    //}
    //else if (selectedoEstado == "SELECCIONAR") {
    //    $("#txtNFactura").prop("disabled", true);
    //    $("#txtFechaFacturacion").prop("disabled", true);
    //    $("#txtFechaEstimadaFacturacion").prop("disabled", true);   
    //}

}

function BuscarSucursal() {

    //var combooEstado = document.getElementById("cboSucursal");
    //var selectedoVendedor = combooEstado.options[combooEstado.selectedIndex].text;
    //var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"4\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"" + combooEstado.selectedIndex +"\"} }]";
    //CargarPagina('#cboVendedor', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
    //var selectedoEstado = combooEstado.options[combooEstado.selectedIndex].text;
}

function BuscarSucursal2() {

    if ($("#ContentPlaceHolder1_txtIdTipo").val() == "8") {
        ObtenerListaComboGerenteCuentaUnico();
        //document.getElementById('cboSucursal2').disabled = false;
    }
    else {
        var combooEstado = document.getElementById("cboSucursal2");
        var selectedoVendedor = combooEstado.options[combooEstado.selectedIndex].text;
        var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"4\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"" + combooEstado.selectedIndex + "\", IdCliente: \"" + "0" + "\"} }]";
        CargarPagina('#cboVendedor2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
    //var selectedoEstado = combooEstado.options[combooEstado.selectedIndex].text;
    }
}

function CalcularMargen(margen, valor) {

    margen = margen.replace("%", "");
    if (margen.length > 0 && valor.length > 0) {
        var rentabilidad = valor * (margen / 100);
        $('#txtRentabilidad').val(rentabilidad);
    }
    else {
        $('#txtRentabilidad').val("");
    }
}

function CalcularMargen2(margen, valor) {

    margen = margen.replace("%", "");
    if (margen.length > 0 && valor.length > 0) {
        var rentabilidad = valor * (margen / 100);
        $('#txtRentabilidad2').val(rentabilidad);
    }
    else {
        $('#txtRentabilidad2').val("");
    }
}

function BtnCrearCliente() {
    tipo = 0;
    document.getElementById("modalMensajeAprobarLabelTitulo").innerHTML = "Registro de Cliente";
    $("#modalMensajeAprobar").modal('show');
}

function BtnCrearClasificacion() {
    tipo = 2;
    document.getElementById("modalMensajeAprobarLabelTitulo").innerHTML = "Registro de Clasificacion";
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
                        ObtenerListaComboCliente();
                        ObtenerListaComboCliente2();
                        ObtenerListaComboClasificacionPedido();
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

function GuardarPoliza() {

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    if ($('#txtNumFactura').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Numero de factura ";
        contadorVerificacion += 1;
    }

    if ($('#txtPedidoPoliza').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Numero de Poliza ";
        contadorVerificacion += 1;
    }

    if ($('#txtFechaInicioPo').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha inicio de registro ";
        contadorVerificacion += 1;
    }

    if ($('#txtFechFinalPo').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha final de registro ";
        contadorVerificacion += 1;
    }
    if ($('#cboPoliza').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar el tipo de poliza ";
        contadorVerificacion += 1;
    }
    if ($('#txtAnexo').val() == "") {
        mensajeVerificacion += "- Debe ingresar el anexo ";
        contadorVerificacion += 1;
    }
    if ($('#txtValorMonto').val() == "") {
        mensajeVerificacion += "- Debe ingresar el Monto ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }


    var datosFormulario = "";



    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'IdPoliza': '" + IdPoliza + "',";
    datosFormulario = datosFormulario + "'IdPedidos': '" + IdPedidos + "',";
    datosFormulario = datosFormulario + "'txtNumFactura': '" + $('#txtNumFactura').val() + "',";
    datosFormulario = datosFormulario + "'txtOS': '" + $('#txtOS').val() + "',";
    datosFormulario = datosFormulario + "'txtPedido': '" + $('#txtPedidoPoliza').val() + "',";
    datosFormulario = datosFormulario + "'txtPoliza': '" + $('#txtPoliza').val() + "',";
    datosFormulario = datosFormulario + "'txtAnexo': '" + $('#txtAnexo').val() + "',";
    datosFormulario = datosFormulario + "'txtBeneficiario': '" + $('#txtBeneficiario').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaInicioPo': '" + $('#txtFechaInicioPo').val() + "',";
    datosFormulario = datosFormulario + "'txtFechFinalPo': '" + $('#txtFechFinalPo').val() + "',";
    datosFormulario = datosFormulario + "'txtMonto': '" + $('#txtValorMonto').val() + "',";
    datosFormulario = datosFormulario + "'txtObjetivo': '" + $('#txtObjetivo').val() + "',";
    datosFormulario = datosFormulario + "'cboPoliza': '" + $('#cboPoliza').val() + "',";
    datosFormulario = datosFormulario + "'cboEstadoPoliza2': '" + $('#cboEstadoPoliza2').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "0" + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevaPoliza', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesPoliza();
                //document.getElementById("RegistroPoliza").style.display = "none";
                //document.getElementById("ListaPedido").style.display = "block";
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

function DuplicarPedido() {
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;


    if ($('#txtFechaDesde').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha de registro ";
        contadorVerificacion += 1;
    }

    if ($('#txtPedido').val() == "") {
        mensajeVerificacion += "- Debe ingresar el numero de Pedido ";
        contadorVerificacion += 1;
    }

    if ($('#cboCliente').val() == "-- SELECCIONE --") {
        mensajeVerificacion += "- Debe ingresar el numero del cliente ";
        contadorVerificacion += 1;
    }
    else {
        var combooCliente = document.getElementById("cboCliente");
        var selectedCliente = combooCliente.options[combooCliente.selectedIndex].text;
    }

    if ($('#cboSegmentacion').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar la segmentación ";
        contadorVerificacion += 1;
    }

    if ($('#cboClasificacion').val() == "-- SELECCIONE --") {
        mensajeVerificacion += "- Debe ingresar el numero de clasificacion ";
        contadorVerificacion += 1;
    }
    else {
        var combooClasificacion = document.getElementById("cboClasificacion");
        var selectedoClasificacion = combooClasificacion.options[combooClasificacion.selectedIndex].text;
    }

    if ($('#txtValor').val() == "") {
        mensajeVerificacion += "- Debe ingresar el valor ";
        contadorVerificacion += 1;
    }

    if ($('#txtMargen').val() == "") {
        mensajeVerificacion += "- Debe ingresar el marge ";
        contadorVerificacion += 1;
    }

    if ($('#txtRentabilidad').val() == "") {
        mensajeVerificacion += "- Debe ingresar la rentabilidad ";
        contadorVerificacion += 1;
    }

    if ($('#cboEstado').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar el estado ";
        contadorVerificacion += 1;
    }

    if ($('#txtDetalle').val() == "") {
        mensajeVerificacion += "- Debe ingresar el detalle ";
        contadorVerificacion += 1;
    }

    //if ($('#txtNFactura').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el Número de Factura ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtFechaFacturacion').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la fecha de facturación ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtFechaEstimadaFacturacion').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la fecha estimada de facturación ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtFechaEstimadaFacturacion').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la fecha estimada de facturación ";
    //    contadorVerificacion += 1;
    //}

    if ($('#cboSucursal').val() == "-- SELECCIONE --") {
        mensajeVerificacion += "- Debe ingresar la sucursal ";
        contadorVerificacion += 1;
    }
    else {
        var combooSucursal = document.getElementById("cboSucursal");
        var selectedoSucursal = combooSucursal.options[combooSucursal.selectedIndex].text;
    }

    if ($('#txtObservacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la observación ";
        contadorVerificacion += 1;
    }

    if ($('#txtObservacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la observación ";
        contadorVerificacion += 1;
    }

    if ($('#cboVendedor').val() == "-- SELECCIONE --") {
        mensajeVerificacion += "- Debe ingresar el vendedor ";
        contadorVerificacion += 1;
    }
    else {
        var comboVendedor = document.getElementById("cboVendedor");
        var selectedoVendedor = comboVendedor.options[comboVendedor.selectedIndex].text;
    }

    if ($('#cboGerente').val() == "-- SELECCIONE --") {
        mensajeVerificacion += "- Debe ingresar el gerente de cuenta ";
        contadorVerificacion += 1;
    }
    else {
        var combooGerente = document.getElementById("cboGerente");
        var selectedoGerente = combooGerente.options[combooGerente.selectedIndex].text;
    }

    if ($('#txtOrdenCompra').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Orden Compra ";
        contadorVerificacion += 1;
    }

    if ($('#txtOrdenServicioInterno').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Orden Servicio Interno ";
        contadorVerificacion += 1;
    }

    if ($('#txtOrdenServicioExterno').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Orden Servicio Externo ";
        contadorVerificacion += 1;
    }

    if ($('#txtOrdenServicioFinanza').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Orden Servicio Finanza ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtPedido': '" + $('#txtPedido').val() + "',";
    datosFormulario = datosFormulario + "'cboCliente': '" + selectedCliente + "',";
    datosFormulario = datosFormulario + "'cboSegmentacion': '" + $('#cboSegmentacion').val() + "',";
    datosFormulario = datosFormulario + "'txtClasificacion': '" + selectedoClasificacion + "',";
    datosFormulario = datosFormulario + "'txtValor': '" + $('#txtValor').val() + "',";
    datosFormulario = datosFormulario + "'txtRentabilidad': '" + $('#txtRentabilidad').val() + "',";
    datosFormulario = datosFormulario + "'txtMargen': '" + $('#txtMargen').val().replace("%","") + "',";
    datosFormulario = datosFormulario + "'cboEstado': '" + $('#cboEstado').val() + "',";
    datosFormulario = datosFormulario + "'txtDetalle': '" + $('#txtDetalle').val() + "',";
    datosFormulario = datosFormulario + "'txtNFactura': '" + $('#txtNFactura').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaFacturacion': '" + $('#txtFechaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEstimadaFacturacion': '" + $('#txtFechaEstimadaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtSucursal': '" + selectedoSucursal + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    datosFormulario = datosFormulario + "'cboVendedor': '" + selectedoVendedor + "',";
    datosFormulario = datosFormulario + "'cboGerente': '" + selectedoGerente + "',";
    datosFormulario = datosFormulario + "'txtOrdenCompra': '" + $('#txtOrdenCompra').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioInterno': '" + $('#txtOrdenServicioInterno').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioExterno': '" + $('#txtOrdenServicioExterno').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "1" + "',";
    datosFormulario = datosFormulario + "'IdPedido': '" + "0" + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioFinanza': '" + $('#txtOrdenServicioFinanza').val() + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoPedido', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesPedidos(0);
                //ObtenerListaPedidosRe(0, 0, $("#txtFechaConsulta1").val(), $("#txtFechaConsulta2").val());
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

function ActualizarPedido() {
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;


    if ($('#txtFechaDesde').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha de registro ";
        contadorVerificacion += 1;
    }

    if ($('#txtPedido').val() == "") {
        mensajeVerificacion += "- Debe ingresar el numero de Pedido ";
        contadorVerificacion += 1;
    }

    if ($('#cboCliente').val() == "") {
        mensajeVerificacion += "- Debe ingresar el numero del cliente ";
        contadorVerificacion += 1;
    }
    else {
        var combooCliente = $('#cboCliente').val()
        var selectedCliente = $('#cboCliente').val();
    }

    if ($('#cboSegmentacion').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar la segmentación ";
        contadorVerificacion += 1;
    }

    if ($('#cboClasificacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar el numero de clasificacion ";
        contadorVerificacion += 1;
    }
    else {
        var combooClasificacion = $('#cboClasificacion').val();
        var selectedoClasificacion = $('#cboClasificacion').val();
    }

    if ($('#txtValor').val() == "") {
        mensajeVerificacion += "- Debe ingresar el valor ";
        contadorVerificacion += 1;
    }

    if ($('#txtMargen').val() == "") {
        mensajeVerificacion += "- Debe ingresar el marge ";
        contadorVerificacion += 1;
    }

    if ($('#txtRentabilidad').val() == "") {
        mensajeVerificacion += "- Debe ingresar la rentabilidad ";
        contadorVerificacion += 1;
    }

    if ($('#cboEstado').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar el estado ";
        contadorVerificacion += 1;
    }

    if ($('#txtDetalle').val() == "") {
        mensajeVerificacion += "- Debe ingresar el detalle ";
        contadorVerificacion += 1;
    }

    //if ($('#txtNFactura').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el Número de Factura ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtFechaFacturacion').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la fecha de facturación ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtFechaEstimadaFacturacion').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la fecha estimada de facturación ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtFechaEstimadaFacturacion').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la fecha estimada de facturación ";
    //    contadorVerificacion += 1;
    //}

    if ($('#cboSucursal').val() == "0") {
        mensajeVerificacion += "- Debe ingresar la secursal ";
        contadorVerificacion += 1;
    }
    else {
        var combooSucursal = document.getElementById("cboSucursal");
        var selectedoSucursal = combooSucursal.options[combooSucursal.selectedIndex].text;
    }

    if ($('#txtObservacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la observación ";
        contadorVerificacion += 1;
    }

    if ($('#txtObservacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la observación ";
        contadorVerificacion += 1;
    }

    if ($('#cboVendedor').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el vendedor ";
        contadorVerificacion += 1;
    }
    else {
        var comboVendedor = document.getElementById("cboVendedor");
        var selectedoVendedor = comboVendedor.options[comboVendedor.selectedIndex].text;
    }

    if ($('#cboGerente').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el gerente de cuenta ";
        contadorVerificacion += 1;
    }
    else {
        var combooGerente = document.getElementById("cboGerente");
        var selectedoGerente = combooGerente.options[combooGerente.selectedIndex].text;
    }

    if ($('#txtOrdenCompra').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Orden Compra ";
        contadorVerificacion += 1;
    }

    //if ($('#txtOrdenServicioInterno').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la Orden Servicio Interno ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtOrdenServicioExterno').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la Orden Servicio Externo ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtOrdenServicioFinanza').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la Orden Servicio Finanza ";
    //    contadorVerificacion += 1;
    //}

    var text = document.getElementById("idRenovacion");
    var checkIngreso = 0;
    if (text.checked == true && $("#ContentPlaceHolder1_txtLoginUsuario").val() == "mbastidas") {
        checkIngreso = 1;
        if ($('#txtFechaDesdeRenova').val() == "" && $('#txtFechaHastaRenova').val() == "") {
            mensajeVerificacion += "- Debe ingresar la fecha de desde y hasta de la renovacion ";
            contadorVerificacion += 1;
        }
    }


    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";



    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtPedido': '" + $('#txtPedido').val() + "',";
    datosFormulario = datosFormulario + "'cboCliente': '" + selectedCliente + "',";
    datosFormulario = datosFormulario + "'cboSegmentacion': '" + $('#cboSegmentacion').val() + "',";
    datosFormulario = datosFormulario + "'txtClasificacion': '" + selectedoClasificacion + "',";
    datosFormulario = datosFormulario + "'txtValor': '" + $('#txtValor').val() + "',";
    datosFormulario = datosFormulario + "'txtRentabilidad': '" + $('#txtRentabilidad').val() + "',";
    datosFormulario = datosFormulario + "'txtMargen': '" + $('#txtMargen').val().replace("%","") + "',";
    datosFormulario = datosFormulario + "'cboEstado': '" + $('#cboEstado').val() + "',";
    datosFormulario = datosFormulario + "'txtDetalle': '" + $('#txtDetalle').val() + "',";
    datosFormulario = datosFormulario + "'txtNFactura': '" + $('#txtNFactura').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaFacturacion': '" + $('#txtFechaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEstimadaFacturacion': '" + $('#txtFechaEstimadaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtSucursal': '" + selectedoSucursal + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    datosFormulario = datosFormulario + "'cboVendedor': '" + selectedoVendedor + "',";
    datosFormulario = datosFormulario + "'cboGerente': '" + selectedoGerente + "',";
    datosFormulario = datosFormulario + "'txtOrdenCompra': '" + $('#txtOrdenCompra').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioInterno': '" + $('#txtOrdenServicioInterno').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioExterno': '" + $('#txtOrdenServicioExterno').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "2" + "',";
    datosFormulario = datosFormulario + "'IdPedido': '" + IdPedidos + "',";
    datosFormulario = datosFormulario + "'txtFechaDesdeRenova': '" + $('#txtFechaDesdeRenova').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaHastaRenova': '" + $('#txtFechaHastaRenova').val() + "',";
    datosFormulario = datosFormulario + "'checkIngreso': '" + checkIngreso + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioFinanza': '" + $('#txtOrdenServicioFinanza').val() + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoPedido', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesPedidos(0);
                //ObtenerListaPedidosRe(0, 0, $("#txtFechaConsulta1").val(), $("#txtFechaConsulta2").val());
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

function ActualizarPedidoObservacion(ids) {
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;



    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtPedido': '" + "0" + "',";
    datosFormulario = datosFormulario + "'cboCliente': '" + "" + "',";
    datosFormulario = datosFormulario + "'cboSegmentacion': '" + $('#cboSegmentacion').val() + "',";
    datosFormulario = datosFormulario + "'txtClasificacion': '" + "" + "',";
    datosFormulario = datosFormulario + "'txtValor': '" + "0" + "',";
    datosFormulario = datosFormulario + "'txtRentabilidad': '" + "0" + "',";
    datosFormulario = datosFormulario + "'txtMargen': '" + "0" + "',";
    datosFormulario = datosFormulario + "'cboEstado': '" + $('#cboEstado').val() + "',";
    datosFormulario = datosFormulario + "'txtDetalle': '" + $('#txtDetalle').val() + "',";
    datosFormulario = datosFormulario + "'txtNFactura': '" + $('#txtNFactura').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaFacturacion': '" + $('#txtFechaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEstimadaFacturacion': '" + $('#txtFechaEstimadaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtSucursal': '" + "" + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacionReq').val() + "',";
    datosFormulario = datosFormulario + "'cboVendedor': '" + "0" + "',";
    datosFormulario = datosFormulario + "'cboGerente': '" + "0" + "',";
    datosFormulario = datosFormulario + "'txtOrdenCompra': '" + $('#txtOrdenCompra').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioInterno': '" + $('#txtOrdenServicioInterno').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioExterno': '" + $('#txtOrdenServicioExterno').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "5" + "',";
    datosFormulario = datosFormulario + "'IdPedido': '" + ids + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioFinanza': '" + $('#txtOrdenServicioFinanza').val() + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoPedido', 'parameters' : " + datosFormulario + " }]";

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
                $('#txtObservacionReq').val("");
                $("#modalMensajeAprobar").modal('hide');
                BtnConsulta();
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

function ActualizarPedidoMargen() {
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;


    if ($('#txtPedido2').val() == "") {
        mensajeVerificacion += "- Debe ingresar el numero de Pedido ";
        contadorVerificacion += 1;
    }


    if ($('#txtValor2').val() == "") {
        mensajeVerificacion += "- Debe ingresar el valor ";
        contadorVerificacion += 1;
    }

    if ($('#txtMargen2').val() == "") {
        mensajeVerificacion += "- Debe ingresar el marge ";
        contadorVerificacion += 1;
    }

    if ($('#txtRentabilidad2').val() == "") {
        mensajeVerificacion += "- Debe ingresar la rentabilidad ";
        contadorVerificacion += 1;
    }


    if ($('#cboVendedor3').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el vendedor ";
        contadorVerificacion += 1;
    }
    else {
        var comboVendedor = document.getElementById("cboVendedor3");
        var selectedoVendedor = comboVendedor.options[comboVendedor.selectedIndex].text;
    }

    if ($('#cboGerente3').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el gerente de cuenta ";
        contadorVerificacion += 1;
    }
    else {
        var combooGerente = document.getElementById("cboGerente3");
        var selectedoGerente = combooGerente.options[combooGerente.selectedIndex].text;
    }

   

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtPedido': '" + $('#txtPedido2').val() + "',";
    datosFormulario = datosFormulario + "'cboCliente': '" + $('#cboCliente3').val() + "',";
    datosFormulario = datosFormulario + "'cboSegmentacion': '" + $('#cboSegmentacion').val() + "',";
    datosFormulario = datosFormulario + "'txtClasificacion': '" + "" + "',";
    datosFormulario = datosFormulario + "'txtValor': '" + $('#txtValor2').val() + "',";
    datosFormulario = datosFormulario + "'txtRentabilidad': '" + $('#txtRentabilidad2').val() + "',";
    datosFormulario = datosFormulario + "'txtMargen': '" + $('#txtMargen2').val().replace("%", "") + "',";
    datosFormulario = datosFormulario + "'cboEstado': '" + $('#cboEstado').val() + "',";
    datosFormulario = datosFormulario + "'txtDetalle': '" + $('#txtDetalle').val() + "',";
    datosFormulario = datosFormulario + "'txtNFactura': '" + $('#txtNFactura').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaFacturacion': '" + $('#txtFechaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEstimadaFacturacion': '" + $('#txtFechaEstimadaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtSucursal': '" + "" + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    datosFormulario = datosFormulario + "'cboVendedor': '" + selectedoVendedor + "',";
    datosFormulario = datosFormulario + "'cboGerente': '" + selectedoGerente + "',";
    datosFormulario = datosFormulario + "'txtOrdenCompra': '" + $('#txtOrdenCompra').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioInterno': '" + $('#txtOrdenServicioInterno').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioExterno': '" + $('#txtOrdenServicioExterno').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "4" + "',";
    datosFormulario = datosFormulario + "'IdPedido': '" + 0 + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioFinanza': '" + $('#txtOrdenServicioFinanza').val() + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoPedido', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesPedidosMargen(1);
                $('#txtPedido2').val("");
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

function EliminarPedido() {

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;


   
    if (IdPedidos == 0) {
        mensajeVerificacion += "- Debe seleccionar un Pedido para realizar la eliminacion ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtPedido': '" + $('#txtPedido').val() + "',";
    datosFormulario = datosFormulario + "'cboCliente': '" + "0" + "',";
    datosFormulario = datosFormulario + "'cboSegmentacion': '" + $('#cboSegmentacion').val() + "',";
    datosFormulario = datosFormulario + "'txtClasificacion': '" + "0" + "',";
    datosFormulario = datosFormulario + "'txtValor': '" + $('#txtValor').val() + "',";
    datosFormulario = datosFormulario + "'txtRentabilidad': '" + $('#txtRentabilidad').val() + "',";
    datosFormulario = datosFormulario + "'txtMargen': '" + $('#txtMargen').val().replace("%", "") + "',";
    datosFormulario = datosFormulario + "'cboEstado': '" + $('#cboEstado').val() + "',";
    datosFormulario = datosFormulario + "'txtDetalle': '" + $('#txtDetalle').val() + "',";
    datosFormulario = datosFormulario + "'txtNFactura': '" + $('#txtNFactura').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaFacturacion': '" + $('#txtFechaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEstimadaFacturacion': '" + $('#txtFechaEstimadaFacturacion').val() + "',";
    datosFormulario = datosFormulario + "'txtSucursal': '" + "0" + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    datosFormulario = datosFormulario + "'cboVendedor': '" + "0" + "',";
    datosFormulario = datosFormulario + "'cboGerente': '" + "0" + "',";
    datosFormulario = datosFormulario + "'txtOrdenCompra': '" + $('#txtOrdenCompra').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioInterno': '" + $('#txtOrdenServicioInterno').val() + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioExterno': '" + $('#txtOrdenServicioExterno').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "3" + "',";
    datosFormulario = datosFormulario + "'IdPedido': '" + IdPedidos + "',";
    datosFormulario = datosFormulario + "'txtOrdenServicioFinanza': '" + $('#txtOrdenServicioFinanza').val() + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoPedido', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesPedidos(0);
                //ObtenerListaPedidosRe(0, 0, $("#txtFechaConsulta1").val(), $("#txtFechaConsulta2").val());
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

function BtnActualizarFecha() {

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;



    if ($('#cboAnio1').val() == "0") {
        mensajeVerificacion += "- Debe seleccionar el año ";
        contadorVerificacion += 1;
    }
    else {
        var combocboAnio = document.getElementById("cboAnio1");
        var selectedcboAnio = combocboAnio.options[combocboAnio.selectedIndex].text;
    }

    if ($('#cboMeses1').val() == "0") {
        mensajeVerificacion += "- Debe seleccionar el mes ";
        contadorVerificacion += 1;
    }
    else {
        var combocboMeses = document.getElementById("cboMeses1");
        var selectedcboMeses = combocboMeses.options[combocboMeses.selectedIndex].text;
    }

    if ($('#txtFechaNueva').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";


    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaNueva': '" + $('#txtFechaNueva').val() + "',";
    datosFormulario = datosFormulario + "'Anio': '" + combocboAnio.selectedIndex + "',";
    datosFormulario = datosFormulario + "'meses': '" + combocboMeses.selectedIndex + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "0" + "',";
    datosFormulario = datosFormulario + "'idFecha': '" + $('#cboFecha1').val() + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'ActualizarFechaPedido', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesPedidos2();
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

function VerListadoArchivosPoliza(div, idTarea) {

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

function ListadoArchivosPoliza(div, idTarea) {

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


    to = $("#txtFechaFacturacion").datepicker(
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

    tofrom2 = $("#txtFechaEstimadaFacturacion").datepicker(
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

    tofrom9 = $("#txtFechaNueva").datepicker(
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

    tofrom10 = $("#txtFechaInicioPo").datepicker(
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

    tofrom11 = $("#txtFechFinalPo").datepicker(
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

    tofrom12 = $("#txtFechaBusInicio").datepicker(
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

    tofrom13 = $("#txtFechaBusFinal").datepicker(
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

    tofrom14 = $("#txtFechaBusInicioPoliza").datepicker(
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

    tofrom15 = $("#txtFechaBusFinalPoliza").datepicker(
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

    tofrom16 = $("#txtFechaEmisionPo").datepicker(
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

    tofrom17 = $("#txtFechaDesdeRenova").datepicker(
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

    tofrom18 = $("#txtFechaHastaRenova").datepicker(
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

    $("#txtFechaEmisionPo").datepicker('setDate', 'today');
    $("#txtFechaBusInicioPoliza").datepicker('setDate', 'today');
    $("#txtFechaBusFinalPoliza").datepicker('setDate', 'today');
    $("#txtFechaBusInicio").datepicker('setDate', 'today');
    $("#txtFechaBusFinal").datepicker('setDate', 'today');
    $("#txtFechaInicioPo").datepicker('setDate', 'today');
    $("#txtFechFinalPo").datepicker('setDate', 'today');
    $("#txtFechaNueva").datepicker('setDate', 'today');
    $("#txtFechaDesde").datepicker('setDate', 'today');
    $("#txtFechaConsulta1").datepicker('setDate', 'today');
    $("#txtFechaConsulta2").datepicker('setDate', 'today');
    //$("#txtFechaDesdeRenova").datepicker('setDate', 'today');
    //$("#txtFechaHastaRenova").datepicker('setDate', 'today');

    formFrom = $("#frmTxtFecha").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"],
        });


    formFromTime = $('#frmTxtHoraDesde').datetimepicker({
        format: 'HH:mm'
    }).on('dp.change', function (e) {
        var minutosInicioTotal = getMinutes(formFromTime);
        var minutosFinalTotal = getMinutes(formToTime);

        if (minutosInicioTotal > minutosFinalTotal) {
            formToTime.val(formFromTime.val());
        }

        calculeTime(formFromTime.val(), formToTime.val(), '#frmTxtTiempo');

    });


    formToTime = $('#frmTxtHoraHasta').datetimepicker({
        format: 'HH:mm'
    }).on('dp.change', function (e) {
        var minutosInicioTotal = getMinutes(formFromTime);
        var minutosFinalTotal = getMinutes(formToTime);

        if (minutosInicioTotal > minutosFinalTotal) {
            formFromTime.val(formToTime.val());
        }

        calculeTime(formFromTime.val(), formToTime.val(), '#frmTxtTiempo');

    });

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

    ObtenerListaComboClasificacionPedido();
    ObtenerListaComboCliente();
    ObtenerListaComboCliente2();
    ObtenerListaComboGerenteCuenta();
    ObtenerListaComboGerenteProducto();

    ObtenerListaComboClasificacionPedido2();

    if ($("#ContentPlaceHolder1_txtIdTipo").val() == "8") {
        ObtenerListaComboGerenteCuentaUnico();
        ObtenerListaComboGerenteProducto2();
        //document.getElementById('cboSucursal2').disabled = false;
    }
    else if ($("#ContentPlaceHolder1_txtIdTipo").val() == "9") {
        ObtenerListaComboGerenteProductoUnico();
        ObtenerListaComboGerenteCuenta2();
        //document.getElementById('cboSucursal2').disabled = true;
    }
    else {
        ObtenerListaComboGerenteProducto2();
        ObtenerListaComboGerenteCuenta2();
        //document.getElementById('cboSucursal2').disabled = true;
    }

   
    ObtenerListaComboGerenteProducto3();
    ObtenerListaComboSucursal();
    ObtenerListaComboSucursal2();

    ObtenerListaComboAnio();
    ObtenerListaComboMeses();

    ObtenerListaComboGerenteCuentaGeneral();
    ObtenerListaComboGerenteCuentaGeneral2();
    ObtenerListaComboAnio2();
    ObtenerListaComboMeses2();



    // ------------------------------------------------------------------

    $('#btnCargarArchivosAdjuntos').click(function () {

        if (IdPoliza == 0) {

            alerta("Para cargar un archivo debe seleccionar una poliza...");
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
                fileData.append('Id_RegTareas', IdPoliza);
                fileData.append('idServicio', "3");

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
                            //MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);
                            ListadoArchivosPoliza("#divArchivosAdjuntosAnteriores", IdPoliza);
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

function DesactivarCheck() {

    var text = document.getElementById("idRenovacion");

    if (text.checked == true && $("#ContentPlaceHolder1_txtLoginUsuario").val() == "mbastidas") {
        document.getElementById("fecha1").style.display = "block";
        document.getElementById("fecha2").style.display = "block";
    }
    else if (text.checked == false) {
        document.getElementById("fecha1").style.display = "none";
        document.getElementById("fecha2").style.display = "none";
    }
 
}

function ListaCheck() {
    var text = document.getElementById("idRenovacion2");
    if (text.checked == true) {
        chekRenovacionFiltrar = 1;
    }
    else {
        chekRenovacionFiltrar = 0;
    }
}

$(document).ready(function () {

    $('.js-example-basic-multiple').select2({
        width: '265px'
    });

    $('.js-example-basic-multiple2').select2({
        width: '250px'
    });

    $('.js-example-basic-multiple3').select2({
        width: '250px'
    });

});