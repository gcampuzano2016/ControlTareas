var idEspecialista = 0;
var idEspecialista2 = 0;
var IdGuardar = 0;
var IdCosteos = 0;
var idCliente = 0;
var idCliente2 = 0;

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

function ObtenerListaComboSucursal1() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"40\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboSucursal', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuenta() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"31\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboSucursal2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"40\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboSucursal2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuenta2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"31\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaClientes(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelect", tipo2);
}

function ObtenerListaClientes2(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}

function RecorreJSONTableSelect(json, boton, idSeleccionado) {

    var x = "";

    if (idSeleccionado == 1) {

        $.each(json, function (i, item) {
            //if (item.length > 0) {       
            x = x + "<li><a role='option' onclick='CargarCliente(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
            //}
            //else {
            //    document.getElementById("comboClientes").innerHTML = "";
            //    document.getElementById("comboClientes").style.display = "none";
            //}
        });

        document.getElementById("comboClientes").innerHTML = x;
        document.getElementById("comboClientes").style.display = "block";
    }
    else if (idSeleccionado == 0) {


        $.each(json, function (i, item) {
            //if (item.length > 0) {       
            x = x + "<li><a role='option' onclick='CargarEspecialista(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
            //}
            //else {
            //    document.getElementById("comboClientes").innerHTML = "";
            //    document.getElementById("comboClientes").style.display = "none";
            //}
        });

        document.getElementById("comboEspecialista").innerHTML = x;
        document.getElementById("comboEspecialista").style.display = "block";

    }
    else if (idSeleccionado == 2) {


        $.each(json, function (i, item) {
            //if (item.length > 0) {       
            x = x + "<li><a role='option' onclick='CargarEspecialista2(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
            //}
            //else {
            //    document.getElementById("comboClientes").innerHTML = "";
            //    document.getElementById("comboClientes").style.display = "none";
            //}
        });

        document.getElementById("comboEspecialista2").innerHTML = x;
        document.getElementById("comboEspecialista2").style.display = "block";

    }

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

    if (tipoControl == "tableSelectForeCast") {
        //contenido = RecorreJSONTableSelectForeCast(json, boton, idSeleccionado);
        dtUsers(json);
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

    if (tipoControl == "detalleDatosCosteo") {
        if (boton > 0) {
            //ObtenerListaComboGerenteCuenta();
            contenido = RecorreDatosPantallaCosteo(json);
            $(div).html(contenido);
        }
        else {
            //BorrarBotonesPedidosMargen(1);
        }
    }

    if (tipoControl == "ValidacionForeCast") {
        if (boton > 0) {
            contenido = RecorreValidacionForeCast(json);
            $(div).html(contenido);
        }
        else {
            var combo13 = document.getElementById("txtSector");
            combo13.value = "SELECCIONAR";
        }
    }

    return contenido;
}

function RecorreValidacionForeCast(json) {
    $.each(json, function (i, item) {

        if (idValidacion == 1) {

            ExisteCliente = item.IdForeCast;

        }
        else if (idValidacion == 2) {

            var combo13 = document.getElementById("txtSector");
            combo13.value = item.TipoEmpresa;
        }

    });
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

function BuscarEspecialista() {

    if ($('#txtecnico').val().length > 4) {
        ObtenerListaClientes(0, $('#txtecnico').val(),0);
    }
    else {
        document.getElementById("comboEspecialista").style.display = "none";
    }
}

function BuscarEspecialista2() {

    if ($('#txtecnico2').val().length > 4) {
        ObtenerListaClientes(0, $('#txtecnico2').val(),2);
    }
    else {
        document.getElementById("comboEspecialista2").style.display = "none";
    }
}

function CargarEspecialista(ID, NOMBRE) {
    $('#txtecnico').val(NOMBRE);
    idEspecialista = ID;
    document.getElementById("comboEspecialista").style.display = "none";
}

function CargarEspecialista2(ID, NOMBRE) {
    $('#txtecnico2').val(NOMBRE);
    idEspecialista = ID;
    document.getElementById("comboEspecialista2").style.display = "none";
}

function NuevoCosteo() {
    var combo1 = document.getElementById("cboVendedor");
    combo1.value = 0;
    var combo2 = document.getElementById("cboSucursal");
    combo2.value = 9;
    $('#txtTicket').val("");
    $('#txtSector').val("");
    $('#txtConcepto').val("");
    $('#txtUnidadNegocio').val("");
    $('#txtecnico').val("");
    $('#txtTipoServicio').val("");
    $('#txtEstadoServicio').val("");
    $('#txtRevision').val("");
    $('#txtCosto').val("");
    $('#txtObservacion').val("");
    $('#cboCliente2').val("");
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

function GuardarNuevoCosteo() {
    if (IdGuardar == 1) {
        alerta("En este momento solo se puede actualizar el registro");
    }
    else {
        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var mensajeVerificacion = "";
        var tipoMensaje = "warning";
        var contadorVerificacion = 0;

        if ($('#txtTicket').val() == "") {
            mensajeVerificacion += "- Debe ingresar el numero de ticket ";
            contadorVerificacion += 1;
        }

        if ($('#cboVendedor').val() == "0") {
            mensajeVerificacion += "- Debe seleccionar el gerente de cuenta  ";
            contadorVerificacion += 1;
        }

        if ($('#cboSucursal').val() == "SELECCIONAR SUCURSAL") {
            mensajeVerificacion += "- Debe seleccionar la sucursal ";
            contadorVerificacion += 1;
        }

        if ($('#txtSector').val() == "SELECCIONAR") {
            mensajeVerificacion += "- Debe ingresar el sector ";
            contadorVerificacion += 1;
        }

        if ($('#cboCliente2').val() == "") {
            mensajeVerificacion += "- Debe ingresar el cliente ";
            contadorVerificacion += 1;
        }

        if ($('#txtConcepto').val() == "") {
            mensajeVerificacion += "- Debe ingresar el concepto ";
            contadorVerificacion += 1;
        }

        if ($('#txtUnidadNegocio').val() == "SELECCIONAR") {
            mensajeVerificacion += "- Debe ingresar la unidad negocio ";
            contadorVerificacion += 1;
        }

        if ($('#txtecnico').val() == "") {
            mensajeVerificacion += "- Debe ingresar el responsable ";
            contadorVerificacion += 1;
        }

        //if ($('#txtTipoServicio').val() == "") {
        //    mensajeVerificacion += "- Debe ingresar el tipo servicio ";
        //    contadorVerificacion += 1;
        //}

        if ($('#txtEstadoServicio').val() == "") {
            mensajeVerificacion += "- Debe ingresar el estado servicio ";
            contadorVerificacion += 1;
        }

        //if ($('#txtRevision').val() == "") {
        //    mensajeVerificacion += "- Debe ingresar la revision ";
        //    contadorVerificacion += 1;
        //}

        var proceso = "";
        var text = document.getElementById("txtRevision");
        if (text.checked == true) {
            proceso = "SI";
        }
        else {
            proceso = "NO";
        }

        if ($('#txtCosto').val() == "") {
            mensajeVerificacion += "- Debe ingresar el costo ";
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
        datosFormulario = datosFormulario + "'IdCosteo': '" + "0" + "',";
        datosFormulario = datosFormulario + "'txtTicket': '" + $('#txtTicket').val() + "',";
        datosFormulario = datosFormulario + "'txtFechaSolicitud': '" + $('#txtFechaSolicitud').val() + "',";
        datosFormulario = datosFormulario + "'txtFechaActual': '" + $('#txtFechaActual').val() + "',";
        datosFormulario = datosFormulario + "'cboVendedor': '" + $('#cboVendedor').val()  + "',";
        datosFormulario = datosFormulario + "'cboSucursal': '" + $('#cboSucursal').val() + "',";
        datosFormulario = datosFormulario + "'txtSector': '" + $('#txtSector').val() + "',";
        datosFormulario = datosFormulario + "'cboCliente2': '" + $('#cboCliente2').val() + "',";
        datosFormulario = datosFormulario + "'IdCliente': '" + idCliente + "',";
        datosFormulario = datosFormulario + "'txtConcepto': '" + $('#txtConcepto').val() + "',";
        datosFormulario = datosFormulario + "'txtUnidadNegocio': '" + $('#txtUnidadNegocio').val() + "',";
        datosFormulario = datosFormulario + "'txtecnico': '" + $('#txtecnico').val().replace(".", ",") + "',";
        datosFormulario = datosFormulario + "'txtTipoServicio': '" + $('#txtTipoServicio').val() + "',";
        datosFormulario = datosFormulario + "'txtPlazoEntrega': '" + $('#txtPlazoEntrega').val() + "',";
        datosFormulario = datosFormulario + "'txtEstadoServicio': '" + $('#txtEstadoServicio').val() + "',";
        datosFormulario = datosFormulario + "'txtFechaEntregaEsp': '" + $('#txtFechaEntregaEsp').val() + "',";
        datosFormulario = datosFormulario + "'txtFechaEntregaAlc': '" + $('#txtFechaEntregaAlc').val() + "',";
        datosFormulario = datosFormulario + "'txtRevision': '" + proceso + "',";
        datosFormulario = datosFormulario + "'txtCosto': '" + formatearNumero($('#txtCosto').val()) + "',";
        datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
        datosFormulario = datosFormulario + "'Tipo': '" + "0" + "'";
        datosFormulario = datosFormulario + "}";

        datos = "[{'action': 'GuardarNuevoCosteo', 'parameters' : " + datosFormulario + " }]";

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
                    NuevoCosteo()
                    MensajeCorrecto(respuesta.mensaje);
                    $("#divMensajes").html("");
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

        return;
    }
}

function ActualizarCosteo() {

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    if ($('#txtTicket').val() == "") {
        mensajeVerificacion += "- Debe ingresar el numero de ticket ";
        contadorVerificacion += 1;
    }

    if ($('#cboVendedor').val() == "0") {
        mensajeVerificacion += "- Debe seleccionar el gerente de cuenta  ";
        contadorVerificacion += 1;
    }

    if ($('#cboSucursal').val() == "SELECCIONAR SUCURSAL") {
        mensajeVerificacion += "- Debe seleccionar la sucursal ";
        contadorVerificacion += 1;
    }

    if ($('#txtSector').val() == "") {
        mensajeVerificacion += "- Debe ingresar el sector ";
        contadorVerificacion += 1;
    }

    if ($('#txtConcepto').val() == "") {
        mensajeVerificacion += "- Debe ingresar el concepto ";
        contadorVerificacion += 1;
    }

    if ($('#txtUnidadNegocio').val() == "") {
        mensajeVerificacion += "- Debe ingresar la unidad negocio ";
        contadorVerificacion += 1;
    }

    if ($('#txtecnico').val() == "") {
        mensajeVerificacion += "- Debe ingresar el responsable ";
        contadorVerificacion += 1;
    }

    //if ($('#txtTipoServicio').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el tipo servicio ";
    //    contadorVerificacion += 1;
    //}

    if ($('#txtEstadoServicio').val() == "") {
        mensajeVerificacion += "- Debe ingresar el estado servicio ";
        contadorVerificacion += 1;
    }

    //if ($('#txtRevision').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar la revision ";
    //    contadorVerificacion += 1;
    //}

    if ($('#txtCosto').val() == "") {
        mensajeVerificacion += "- Debe ingresar el costo ";
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
    datosFormulario = datosFormulario + "'IdCosteo': '" + IdCosteos + "',";
    datosFormulario = datosFormulario + "'txtTicket': '" + $('#txtTicket').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaSolicitud': '" + $('#txtFechaSolicitud').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaActual': '" + $('#txtFechaActual').val() + "',";
    datosFormulario = datosFormulario + "'cboVendedor': '" + $('#cboVendedor').val() + "',";
    datosFormulario = datosFormulario + "'cboSucursal': '" + $('#cboSucursal').val() + "',";
    datosFormulario = datosFormulario + "'txtSector': '" + $('#txtSector').val() + "',";
    datosFormulario = datosFormulario + "'cboCliente2': '" + $('#cboCliente2').val() + "',";
    datosFormulario = datosFormulario + "'IdCliente': '" + idCliente + "',";
    datosFormulario = datosFormulario + "'txtConcepto': '" + $('#txtConcepto').val() + "',";
    datosFormulario = datosFormulario + "'txtUnidadNegocio': '" + $('#txtUnidadNegocio').val() + "',";
    datosFormulario = datosFormulario + "'txtecnico': '" + $('#txtecnico').val().replace(".", ",") + "',";
    datosFormulario = datosFormulario + "'txtTipoServicio': '" + $('#txtTipoServicio').val() + "',";
    datosFormulario = datosFormulario + "'txtPlazoEntrega': '" + $('#txtPlazoEntrega').val() + "',";
    datosFormulario = datosFormulario + "'txtEstadoServicio': '" + $('#txtEstadoServicio').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEntregaEsp': '" + $('#txtFechaEntregaEsp').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaEntregaAlc': '" + $('#txtFechaEntregaAlc').val() + "',";
    datosFormulario = datosFormulario + "'txtRevision': '" + $('#txtRevision').val() + "',";
    datosFormulario = datosFormulario + "'txtCosto': '" + formatearNumero($('#txtCosto').val()) + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + "0" + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoCosteo', 'parameters' : " + datosFormulario + " }]";

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
                NuevoCosteo()
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
    ObtenerValidacionClienteForeCast(NOMBRE, 2);
    idCliente = ID;
    document.getElementById("comboClientes2").style.display = "none";
}

function ObtenerValidacionClienteForeCast(Descripcion, tipo) {
    idValidacion = tipo;
    var Datos = "[{ \"action\": \"ReporteValidacionForeCast\", \"parameters\" : { Descripcion: \"" + Descripcion + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "ValidacionForeCast", tipo);
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

function BtnConsulta() {
    var fechaInicio = "";
    var fechaFinal = "";

    ObtenerConsultarCosteoDetalleFiltros(0, fechaInicio, fechaFinal);
}

function BtnDescarga() {
    var fechaInicio = "";
    var fechaFinal = "";

    ObtenerConsultarCosteoDetalleFiltrosDescargar(0, fechaInicio, fechaFinal);
}

function ObtenerConsultarCosteoDetalleFiltros(tipo, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ReporteConsultarCosteoFiltros\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", FchIni: \"" + $("#txtFechaConsulta1").val() + "\", FchFin: \"" + $("#txtFechaConsulta2").val() + "\", IdCliente: \"" + idCliente2 + "\",IdGerenteCuenta: \"" + $("#cboVendedor2").val() + "\",sucursal: \"" + $("#cboSucursal2").val() + "\",ResponsableDimen: \"" + $("#txtecnico2").val() + "\",tipofecha: \"" + $("#cboTipoFecha").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipalForeCast', 'ObtenerListaTareas.ashx', Datos, "tableSelectForeCast", tipo);
}

function ObtenerConsultarCosteoDetalleFiltrosDescargar(tipo, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ReporteConsultarCosteoFiltrosDescargar\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", FchIni: \"" + $("#txtFechaConsulta1").val() + "\", FchFin: \"" + $("#txtFechaConsulta2").val() + "\", IdCliente: \"" + idCliente2 + "\",IdGerenteCuenta: \"" + $("#cboVendedor2").val() + "\",sucursal: \"" + $("#cboSucursal2").val() + "\",ResponsableDimen: \"" + $("#txtecnico2").val() + "\",tipofecha: \"" + $("#cboTipoFecha").val() + "\"} }]";
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

function Create() {
    if ($.fn.DataTable.isDataTable('#table-users')) {
        $('#table-users').DataTable().destroy();
    }
    $('#table-users tbody').empty();
    //Here call the Datatable Bind function;
}

function dtUsers(json) {
    Create();
    table = null;

    table = $('#table-users').DataTable({
        data: json,
        columns: [
            { defaultContent: "<button type='button' value='Actualizar' title='Ver Costeo' class='btn btn btn-editClientes btn-xs'><i class='fa fa-eye' aria-hidden='true'></i></button>" },
            { data: 'Ticket' },
            { data: 'FechaSolicitud' },
            { data: 'FechaActual' },
            { data: 'Vendedor' },
            { data: 'Sucursal' },
            { data: 'Sector' },
            { data: 'Cliente' },
            { data: 'Concepto' },
            { data: 'UnidadNegocio' },
            { data: 'ResponsableDimen' },
            { data: 'TipoServicio' },
            { data: 'PlazoEntrega' },
            { data: 'EstadoServicio' },
            { data: 'FechaEntregaEsp' },
            { data: 'FechaEntregaAlc' },
            { data: 'Revision' },
            { data: 'Costo' },
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

    //$('#table-users').on('click', 'button', function () {
    //    var data = table.row($(this).parents('tr')).data();
    //    //alert(data.IdForeCast);
    //    CargarIdForeCastLista(data.IdForeCast);
    //});
}

$(document).on('click', '.btn-editClientes', function (e) {
    e.preventDefault();
    var data = table.row($(this).parents('tr')).data();
    CargarIdForeCastLista(data.IdCosteo);
    VerListadoArchivosCosteo("#divArchivosAdjuntosAnteriores", data.IdCosteo);
});

function CargarIdForeCastLista(IdCosteo) {
    IdCosteos = IdCosteo;
    IdGuardar = 1;
    ObtenerConsultarCosteo(IdCosteos, 1);

    $('.nav-pills a[href="#home-pills"]').tab('show');
}

function ObtenerConsultarCosteo(idCosteo, tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarCosteo\", \"parameters\" : { idCosteo: \"" + idCosteo + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "detalleDatosCosteo", tipo);
}

function RecorreDatosPantallaCosteo(json) {

    $.each(json, function (i, item) {
        IdCosteos = item.IdCosteo;
        idCliente = item.IdCliente;
        var combo2 = document.getElementById("cboSucursal");
        combo2.value = item.IdSucursal;
        /*  BuscarSucursal2();*/
        $('#cboCliente2').val(item.Cliente);
        $('#txtTicket').val(item.Ticket);
        $('#txtFechaSolicitud').val(item.FechaSolicitud);
        $('#txtFechaActual').val(item.FechaActual);
        $('#txtSector').val(item.Sector);
        $('#txtConcepto').val(item.Concepto);
        $('#txtUnidadNegocio').val(item.UnidadNegocio);
        $('#txtecnico').val(item.ResponsableDimen);
        $('#txtTipoServicio').val(item.TipoServicio);
        $('#txtPlazoEntrega').val(item.PlazoEntrega);
        $('#txtEstadoServicio').val(item.EstadoServicio);

        $('#txtFechaEntregaEsp').val(item.FechaEntregaEsp);
        $('#txtFechaEntregaAlc').val(item.FechaEntregaAlc);
        $('#txtRevision').val(item.Revision);
        $('#txtCosto').val(item.Costo);
        $('#txtObservacion').val(item.Observacion);
        var combo1 = document.getElementById("cboVendedor");
        combo1.value = item.IdVendedor;
    });
}

function BuscarSucursal2() {
    ObtenerListaComboGerenteCuentaUnico();
}

function ObtenerListaComboGerenteCuentaUnico() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"40\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"" + $("#cboSucursal").val() + "\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function BuscarSucursal3() {
    ObtenerListaComboGerenteCuentaUnico2();
}

function ObtenerListaComboGerenteCuentaUnico2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"40\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"" + $("#cboSucursal2").val() + "\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function VerListadoArchivosCosteo(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + idTarea + "',";
    datosParametros = datosParametros + "'IdServicio': '" + "5" + "'";
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

function ListadoArchivosCosteo(div, idTarea) {

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

    from = $("#txtFechaSolicitud").datepicker(
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

    tofrom2 = $("#txtFechaActual").datepicker(
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

    tofrom3 = $("#txtPlazoEntrega").datepicker(
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

    tofrom4 = $("#txtFechaEntregaEsp").datepicker(
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

    tofrom5 = $("#txtFechaEntregaAlc").datepicker(
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

    tofrom6 = $("#txtFechaConsulta1").datepicker(
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

    tofrom7 = $("#txtFechaConsulta2").datepicker(
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
    $("#txtFechaSolicitud").datepicker('setDate', 'today');
    $("#txtFechaActual").datepicker('setDate', 'today');
    $("#txtPlazoEntrega").datepicker('setDate', 'today');
    $("#txtFechaEntregaEsp").datepicker('setDate', 'today');
    $("#txtFechaEntregaAlc").datepicker('setDate', 'today');
    $("#txtFechaConsulta1").datepicker('setDate', 'today');
    $("#txtFechaConsulta2").datepicker('setDate', 'today');

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

    ObtenerListaComboSucursal1();
    ObtenerListaComboGerenteCuenta();

    ObtenerListaComboSucursal2();
    ObtenerListaComboGerenteCuenta2();


    // ------------------------------------------------------------------

    $('#btnCargarArchivosAdjuntos').click(function () {

        if (IdCosteos == 0) {

            alerta("Para cargar un archivo debe seleccionar un costeo...");
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
                fileData.append('Id_RegTareas', IdCosteos);
                fileData.append('idServicio', "5");

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
                            ListadoArchivosCosteo("#divArchivosAdjuntosAnteriores", IdCosteos);
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