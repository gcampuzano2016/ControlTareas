var idCliente = 0;
var idCliente2 = 0;
var IdForeCast = 0;
var tabla;
var IdGuardar = 0;
var tipo = 0;
var idValidacion = 0;
var ExisteCliente = 0;
var table;

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

function format(input) {
    var num = input.value.replace(/\./g, '');
    if (!isNaN(num)) {
        num = num.toString().split('').reverse().join('').replace(/(?=\d*\.?)(\d{3})/g, '$1.');
        num = num.split('').reverse().join('').replace(/^[\.]/, '');
        input.value = num;
    }

    else {
        alerta('Solo se permiten numeros');
        input.value = input.value.replace(/[^\d\.]*/g, '');
    }
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


function ObtenerListaComboGerenteCuentaUnico2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"31\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuenta2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"29\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboVendedor2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteProducto2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"32\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteProductoUnico2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"32\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboMarca2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"16\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboMarca2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboFecha1() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"23\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboFecha1', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboFecha1Buscar() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"23\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboFecha1Buscar', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboFecha2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"28\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboFecha2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboFecha2Buscar() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"28\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboFecha2Buscar', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboSegmentacion2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"26\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboSegmentacion2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboPrioridad2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"27\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboPrioridad2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboSucursal2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"33\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboSucursal2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function BuscarCliente2() {

    if ($('#cboCliente2').val().length > 2) {
        idSeleccionado = 0;
        ObtenerListaClientes(2, $('#cboCliente2').val(), 4);
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

function BuscarCliente3() {

    if ($('#cboCliente3').val().length > 2) {
        idSeleccionado = 0;
        ObtenerListaClientes(2, $('#cboCliente3').val(), 5);
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

function ObtenerListaClientes(tipo, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", idproceso);
}

function ObtenerConsultarForeCast(idForeCast, tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarForeCast\", \"parameters\" : { idForeCast: \"" + idForeCast + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "detalleDatosForeCast", tipo);
}

function ObtenerValidacionClienteForeCast(Descripcion, tipo) {
    idValidacion = tipo;
    var Datos = "[{ \"action\": \"ReporteValidacionForeCast\", \"parameters\" : { Descripcion: \"" + Descripcion + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "ValidacionForeCast", tipo);
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

    if (tipoControl == "tableSelectForeCast") {
        contenido = RecorreJSONTableSelectForeCast(json, boton, idSeleccionado);
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

    if (tipoControl == "detalleDatosForeCast") {
        if (boton > 0) {
            contenido = RecorreDatosPantallaForeCast(json);
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
            var combo13 = document.getElementById("cboInstitucion");
            combo13.value = "SELECCIONAR";

            var combo7 = document.getElementById("cboSegmentacion");
            combo7.value = 0;
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

            var combo13 = document.getElementById("cboInstitucion");
            combo13.value = item.TipoEmpresa;

            var combo7 = document.getElementById("cboSegmentacion");
            combo7.value = item.SegmentodeMercado;
        }

    });
}

function RecorreDatosPantallaForeCast(json) {

    $.each(json, function (i, item) {
        IdForeCast = item.IdForeCast;
        idCliente = item.IdCliente;
        var combo2 = document.getElementById("cboMarca");
        combo2.value = item.IdMarca;

        $('#cboCliente2').val(item.Cliente);
        $('#txtDetalleProyecto').val(item.DetalleProyecto);
        //$('#txtPrecioEstimado').val(formatearNumero(item.PVPEstimado.toFixed(2)));
        $('#txtUtilidad').val(item.Utilidad);
        //$('#txtUtilidadBruta').val(formatearNumero(item.UtilidadBrutaEstimada.toFixed(2)));

        $('#txtNumeroRegistro').val(item.NumeroRegistro);
        $('#txtGerenteCuenFabricante').val(item.GerenteCuenFabricante);
        $('#txtLiderProyectoCliente').val(item.LiderProyectoCliente);
        $('#txtMayorista').val(item.Mayorista);
        $('#txtFormaPago').val(item.FormaPago);

        $('#txtPrecioEstimado2').val(item.PVPEstimado);
        $('#txtUtilidadBruta2').val(item.UtilidadBrutaEstimada);

        //$('#txtPrecioEstimado').val(item.PVPEstimado);
        //$('#txtUtilidadBruta').val(item.UtilidadBrutaEstimada);

        $('#txtPrecioEstimado').val(formatearNumero(parseFloat(item.PVPEstimado)));
        $('#txtUtilidadBruta').val(formatearNumero(parseFloat(item.UtilidadBrutaEstimada)));

        var combo3 = document.getElementById("cboFecha1");
        combo3.value = item.FechaFacturacion;
        var combo4 = document.getElementById("cboFecha2");
        combo4.value = item.MesEstimadoCierre;
        $('#txtObservacion').val(item.Observaciones);

        var combo5 = document.getElementById("cboGerente");
        combo5.value = item.IdGerenteProducto;

        var combo6 = document.getElementById("cboVendedor");
        combo6.value = item.IdGerenteCuenta;

        var combo7 = document.getElementById("cboSegmentacion");
        combo7.value = item.SegmentodeMercado;

        var combo8 = document.getElementById("cboPrioridad");
        combo8.value = item.Prioridad;

        var combo9 = document.getElementById("cboSucursal");
        combo9.value = item.Sucursal;

        var combo10 = document.getElementById("cboRegistro");
        combo10.value = item.RegistroAprobado;

        var combo11 = document.getElementById("cboCierreNegocio");
        combo11.value = item.IdNegocio;

        var combo12 = document.getElementById("cboPrioridad");
        combo12.value = item.IdPrioridad;

        var combo13 = document.getElementById("cboInstitucion");
        combo13.value = item.TipoEmpresa;

        var combo14 = document.getElementById("cboJustificacion");
        combo14.value = item.Justificacion;

        $('#txtFechaInicioProyecto').val(item.FechaInicioProyecto.replace("'", ""));
        $('#txtFechaFacturacionProyecto').val(item.FechaFacturacionProyecto.replace("'", ""));
        $('#txtContactoLiderProyecto').val(item.Contacto);

        BuscarPrioridad();

    });
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

function BtnConsultaForeCast() {
    IdForeCast = 0;
    var fechaInicio = "";
    var fechaFinal = "";

    ObtenerConsultarForeCastDetalleFiltros(IdForeCast, 0, fechaInicio, fechaFinal);
}

function BtnDescargaForeCast() {
    IdForeCast = 0;
    var fechaInicio = "";
    var fechaFinal = "";

    ObtenerConsultarForeCastDetalleFiltrosDescargar(IdForeCast, 0, fechaInicio, fechaFinal);
}

function ObtenerConsultarForeCastDetalleFiltros(idForeCast, tipo, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ReporteConsultarForeCastFiltros\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", idForeCast: \"" + idForeCast + "\", FechaFacturacion: \"" + $("#cboFecha1Buscar").val() + "\", MesEstimadoCierre: \"" + "" + "\", IdCliente: \"" + idCliente2 + "\",IdGerenteCuenta: \"" + $("#cboVendedor2").val() + "\",IdGestorProducto: \"" + $("#cboGerente2").val() + "\",IdMarca: \"" + $("#cboMarca2").val() + "\",sucursal: \"" + $("#cboSucursal2").val() + "\",SegmentodeMercado: \"" + $("#cboSegmentacion2").val() + "\",IdPrioridad: \"" + $("#cboPrioridad2").val() + "\",mespago: \"" + $("#cboFecha2Buscar").val() + "\",tipofecha: \"" + $("#cboTipoFecha").val() + "\",anio: \"" + $("#cboAnio1").val() + "\",cierrenegocio: \"" + "" + "\",IdPrioProyecto: \"" + "0" + "\",ProyecEstrategico: \"" + "0" + "\",TipoProyecto: \"" + "" + "\"} }]";
    CargarPagina('#datosTablaPrincipalForeCast', 'ObtenerListaTareas.ashx', Datos, "tableSelectForeCast", tipo);
}

function ObtenerConsultarForeCastDetalleFiltrosDescargar(idForeCast, tipo, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ReporteConsultarForeCastFiltrosDescargar\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", idForeCast: \"" + idForeCast + "\", FechaFacturacion: \"" + $("#cboFecha1Buscar").val() + "\", MesEstimadoCierre: \"" + "" + "\", IdCliente: \"" + idCliente2 + "\",IdGerenteCuenta: \"" + $("#cboVendedor2").val() + "\",IdGestorProducto: \"" + $("#cboGerente2").val() + "\",IdMarca: \"" + $("#cboMarca2").val() + "\",sucursal: \"" + $("#cboSucursal2").val() + "\",SegmentodeMercado: \"" + $("#cboSegmentacion2").val() + "\",IdPrioridad: \"" + $("#cboPrioridad2").val() + "\",mespago: \"" + $("#cboFecha2Buscar").val() + "\",tipofecha: \"" + $("#cboTipoFecha").val() + "\",anio: \"" + $("#cboAnio1").val() + "\",cierrenegocio: \"" + "" + "\",IdPrioProyecto: \"" + "0" + "\",ProyecEstrategico: \"" + "0" + "\",TipoProyecto: \"" + "" + "\"} }]";
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

function RecorreJSONTableSelectForeCast(json, boton, idSeleccionado) {
    var info = "";

    if (json.length == 0) {
        $('#txtTotalOportunidad').val("0.00");
        $('#txtSubtotal').val("0.00");
        $('#txttotal').val("0.00");
        alerta("No existe un Forecast registrado");
    }
    else {
        var esImpar = true;
        var iniciaBarrido = true;
        var totalValor3 = 0;

        var totalValor1 = 0;
        var totalValor2 = 0;

        $('#txtTotalOportunidad').val(json.length.toFixed(2));

        //
        // Despliegue de titulos de cabecera
        //
        var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 200px;text-align:center;'>";
        var thFinal = "</th>";
        info = info + "<table id='tbl_forecast'  width='100%' class='table table-striped table-bordered table-hover dataTable' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
        info = info + "<thead><tr role='row'>";
        info = info + thInicial + "--ACCIONES--" + thFinal;
        info = info + thInicial + "Marca" + thFinal;
        info = info + thInicial + "Cliente" + thFinal;
        info = info + thInicial + "Detalle del Proyecto" + thFinal;
        info = info + thInicial + "PVP Estimado" + thFinal;
        info = info + thInicial + "% Utilidad" + thFinal;
        info = info + thInicial + "Utilidad Bruta Estimada" + thFinal;
        info = info + thInicial + "Fecha de Facturación" + thFinal;
        info = info + thInicial + "% Cierre Negocio" + thFinal;
        info = info + thInicial + "Fecha Estimado de Cierre" + thFinal;
        info = info + thInicial + "Observaciones" + thFinal;
        info = info + thInicial + "Gerente de Producto" + thFinal;
        info = info + thInicial + "Gerente de Cuenta" + thFinal;
        info = info + thInicial + "Segmento de Mercado" + thFinal;
        info = info + thInicial + "Prioridad" + thFinal;
        info = info + thInicial + "Sucursal" + thFinal;
        info = info + thInicial + "Registro Aprobado" + thFinal;
        info = info + thInicial + "Ult. reg. usuario" + thFinal;
        info = info + thInicial + "Ult. reg. fecha Modificado" + thFinal;
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
            info = info + "<i class='fa fa-eye' style='cursor: pointer' aria-hidden='true' title='Ver las ForeCast' onclick='CargarIdForeCastLista(\"" + item.IdForeCast + "\");'></i>";
            //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            //if (item.conteoArchivosAdjuntos != '0') {
            //    info = info + "<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosContrato(\"#MensajeInformativo\", \"" + item.IdPoliza + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
            //}
            //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            //info = info + "<i class='fa fa-file-text-o' style='cursor: pointer' aria-hidden='true' title='Registrar nueva poliza' onclick='GenerarPolizaNuevo(\"" + item.IdPedido + "\",\"" + item.OS + "\",\"" + item.BENEFICIARIO + "\",\"" + item.PEDIDO + "\");'></i>";
            //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            //info = info + "<i class='fa fa-envelope' style='cursor: pointer' aria-hidden='true' title='Enviar Mail de la Poliza' onclick='EnviarMail(\"" + item.IdPoliza + "\",\"" + item.NumFactura + "\",\"" + item.OS + "\",\"" + item.BENEFICIARIO + "\",\"" + item.PEDIDO + "\");'></i>";
            info = info + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.Marca + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.Cliente + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.DetalleProyecto + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.PVPEstimado) + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.Utilidad + " % </td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.UtilidadBrutaEstimada) + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.FechaFacturacion + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.StrCierreNegocio + " % </td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.MesEstimadoCierre + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.Observaciones + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.GerentedeProducto + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.GerentedeCuenta + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.SegmentodeMercado + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.Prioridad + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.Sucursal + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.RegistroAprobado + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.Usuario + "</td>";
            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.FechaCobro + "</td>";
            totalValor1 = totalValor1 + item.PVPEstimado;
            totalValor2 = totalValor2 + item.UtilidadBrutaEstimada;

            info = info + "</tr>";
        });

        info = info + "</tbody>";
        info = info + "</table>";


        $('#txtSubtotal').val(formatearNumero(totalValor1.toFixed(2)));



        $('#txttotal').val(formatearNumero(totalValor2.toFixed(2)));

        $('#tbl_forecast').dataTable({
            "order": [[0, 'asc'], [1, 'asc']]
        });

    }
    return info;
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
            { defaultContent: "<button type='button' value='Actualizar' title='Ver forecast' class='btn btn btn-editClientes btn-xs'><i class='fa fa-eye' aria-hidden='true'></i></button>" },
            { data: 'Marca' },
            { data: 'Cliente' },
            { data: 'DetalleProyecto' },
            { data: 'strPVPEstimado' },
            { data: 'strUtilidad' },
            { data: 'strUtilidadBrutaEstimada' },
            { data: 'FechaFacturacion' },
            { data: 'StrCierreNegocio' },
            { data: 'MesEstimadoCierre' },
            { data: 'Observaciones' },
            { data: 'GerentedeProducto' },
            { data: 'GerentedeCuenta' },
            { data: 'SegmentodeMercado' },
            { data: 'Prioridad' },
            { data: 'Sucursal' },
            { data: 'RegistroAprobado' },
            { data: 'Usuario' },
            { data: 'FechaCobro' }
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
    CargarIdForeCastLista(data.IdForeCast);
});

function CargarIdForeCastLista(IdForeCasts) {
    IdForeCast = IdForeCasts;
    IdGuardar = 1;
    ObtenerConsultarForeCast(IdForeCast, 1);

    $('.nav-pills a[href="#home-pills"]').tab('show');
}

const formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 0
})

function CalcularMargen(valor, margen) {

    if (IdGuardar == 0) {
        valor = $('#txtPrecioEstimado').val();
        margen = $('#txtUtilidad').val();
    }

    margen = margen.replace("%", "");
    if (margen.length > 0 && valor.length > 0) {
        var rentabilidad = parseFloat(valor) * (parseFloat(margen) / 100);
        $('#txtUtilidadBruta').val(formatearNumero(parseFloat(rentabilidad)));
    }
    else {
        $('#txtUtilidadBruta').val("");
    }

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

function GuardarNuevoForeCast() {

    if (IdGuardar == 1) {
        alerta("En este momento solo se puede actualizar el registro");
    }
    else {

        if (idCliente == 0) {
            alerta("El cliente no se encuentra registrado, Por favor registrelo");
            return;
        }

        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var mensajeVerificacion = "";
        var tipoMensaje = "warning";
        var contadorVerificacion = 0;

        var proceso = "";
        var text = document.getElementById("IdProceso");
        if (text.checked == true) {
            proceso = "SI";
        }
        else {
            proceso = "NO";
        }
        var comboPrioridad2 = document.getElementById("cboPrioridad");
        var selectedoPrioridad2 = comboPrioridad2.options[comboPrioridad2.selectedIndex].text;


        if ($('#cboMarca').val() == "0") {
            mensajeVerificacion += "- Debe ingresar la marca ";
            contadorVerificacion += 1;
        }
        else {
            var comboMarca = document.getElementById("cboMarca");
            var selectedoMarca = comboMarca.options[comboMarca.selectedIndex].text;
        }

        if ($('#cboCliente2').val() == "") {
            mensajeVerificacion += "- Debe ingresar el cliente ";
            contadorVerificacion += 1;
        }

        if ($('#txtDetalleProyecto').val() == "") {
            mensajeVerificacion += "- Debe ingresar el nombre del proyecto ";
            contadorVerificacion += 1;
        }

        if ($('#txtPrecioEstimado').val() == "") {
            mensajeVerificacion += "- Debe ingresar el PVP Estimado ";
            contadorVerificacion += 1;
        }

        if ($('#txtUtilidadBruta').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Util. Bruta Estimada ";
            contadorVerificacion += 1;
        }

        if ($('#cboCierreNegocio').val() == "0") {
            mensajeVerificacion += "- Debe ingresar el estado del negocio ";
            contadorVerificacion += 1;
        }
        else {
            var comboNegocio = document.getElementById("cboCierreNegocio");
            var selectedoNegocio = comboNegocio.options[comboNegocio.selectedIndex].text;
        }

        if ($('#txtObservacion').val() == "") {
            mensajeVerificacion += "- Debe ingresar el estado actual y siguientes pasos ";
            contadorVerificacion += 1;
        }

        if ($('#cboGerente').val() == "0") {
            mensajeVerificacion += "- Debe ingresar el gerente producto ";
            contadorVerificacion += 1;
        }
        else {
            var combooGerente = document.getElementById("cboGerente");
            var selectedoGerente = combooGerente.options[combooGerente.selectedIndex].text;
        }

        if ($('#cboVendedor').val() == "0") {
            mensajeVerificacion += "- Debe ingresar el gerente cuenta ";
            contadorVerificacion += 1;
        }
        else {
            var comboVendedor = document.getElementById("cboVendedor");
            var selectedoVendedor = comboVendedor.options[comboVendedor.selectedIndex].text;
        }

        if ($('#cboInstitucion').val() == "SELECCIONAR") {
            mensajeVerificacion += "- Debe ingresar el tipo de institucion ";
            contadorVerificacion += 1;
        }

        if ($('#cboSegmentacion').val() == "0") {
            mensajeVerificacion += "- Debe ingresar la segmentacion ";
            contadorVerificacion += 1;
        }

        if ($('#cboSucursal').val() == "SELECCIONAR SUCURSAL") {
            mensajeVerificacion += "- Debe ingresar la sucursal ";
            contadorVerificacion += 1;
        }

        if ($('#txtFechaFacturacionProyecto').val() == "") {
            mensajeVerificacion += "- Debe ingresar la fecha estimada de cierre ";
            contadorVerificacion += 1;
        }

        if ($('#txtFechaInicioProyecto').val() == "" && selectedoPrioridad2 == "FORECAST") {
            mensajeVerificacion += "- Debe ingresar la Fecha Facturación ";
            contadorVerificacion += 1;
        }

        if ($('#cboPrioridad').val() == "0") {
            mensajeVerificacion += "- Debe ingresar la prioridad ";
            contadorVerificacion += 1;
        }
        else {

            var comboPrioridad = document.getElementById("cboPrioridad");
            var selectedoPrioridad = comboPrioridad.options[comboPrioridad.selectedIndex].text;
        }

        //if ($('#txtNumeroRegistro').val() == "") {
        //    mensajeVerificacion += "- Debe ingresar el Número registro ";
        //    contadorVerificacion += 1;
        //}

        //if ($('#txtGerenteCuenFabricante').val() == "") {
        //    mensajeVerificacion += "- Debe ingresar el Gerente Cuenta Fabricante ";
        //    contadorVerificacion += 1;
        //}

        //if ($('#txtLiderProyectoCliente').val() == "") {
        //    mensajeVerificacion += "- Debe ingresar el Lider Proyecto Cliente ";
        //    contadorVerificacion += 1;
        //}

        //if ($('#txtMayorista').val() == "") {
        //    mensajeVerificacion += "- Debe ingresar el Mayorista ";
        //    contadorVerificacion += 1;
        //}

        //if ($('#txtFormaPago').val() == "") {
        //    mensajeVerificacion += "- Debe ingresar el Forma Pago ";
        //    contadorVerificacion += 1;
        //}

        if (contadorVerificacion > 0) {
            alerta(mensajeVerificacion);
            return;
        }

        var datosFormulario = "";

        var datosFormulario = "";

        datosFormulario = datosFormulario + "{";
        datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
        datosFormulario = datosFormulario + "'IdForeCast': '" + "0" + "',";
        datosFormulario = datosFormulario + "'IdMarca': '" + $('#cboMarca').val() + "',";
        datosFormulario = datosFormulario + "'cboMarca': '" + selectedoMarca + "',";
        datosFormulario = datosFormulario + "'cboCliente2': '" + $('#cboCliente2').val() + "',";
        datosFormulario = datosFormulario + "'idCliente': '" + idCliente + "',";
        datosFormulario = datosFormulario + "'txtDetalleProyecto': '" + $('#txtDetalleProyecto').val() + "',";
        datosFormulario = datosFormulario + "'txtPrecioEstimado': '" + $('#txtPrecioEstimado').val() + "',";
        datosFormulario = datosFormulario + "'txtUtilidad': '" + $('#txtUtilidad').val().replace(".", ",") + "',";
        datosFormulario = datosFormulario + "'txtUtilidadBruta': '" + $('#txtUtilidadBruta').val() + "',";
        datosFormulario = datosFormulario + "'cboFecha1': '" + $('#cboFecha1').val() + "',";
        datosFormulario = datosFormulario + "'IdNegocio': '" + $('#cboCierreNegocio').val() + "',";
        datosFormulario = datosFormulario + "'cboCierreNegocio': '" + selectedoNegocio + "',";
        datosFormulario = datosFormulario + "'cboFecha2': '" + $('#cboFecha2').val() + "',";
        datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
        datosFormulario = datosFormulario + "'cboGerente': '" + selectedoGerente + "',";
        datosFormulario = datosFormulario + "'IdGerenteProducto': '" + $('#cboGerente').val() + "',";
        datosFormulario = datosFormulario + "'cboVendedor': '" + selectedoVendedor + "',";
        datosFormulario = datosFormulario + "'IdGerenteCuenta': '" + $('#cboVendedor').val() + "',";
        datosFormulario = datosFormulario + "'cboInstitucion': '" + $('#cboInstitucion').val() + "',";
        datosFormulario = datosFormulario + "'cboSegmentacion': '" + $('#cboSegmentacion').val() + "',";
        datosFormulario = datosFormulario + "'cboSucursal': '" + $('#cboSucursal').val() + "',";
        datosFormulario = datosFormulario + "'cboRegistro': '" + proceso + "',";
        datosFormulario = datosFormulario + "'IdPrioridad': '" + $('#cboPrioridad').val() + "',";
        datosFormulario = datosFormulario + "'cboPrioridad': '" + selectedoPrioridad + "',";

        datosFormulario = datosFormulario + "'txtNumeroRegistro': '" + $('#txtNumeroRegistro').val() + "',";
        datosFormulario = datosFormulario + "'txtGerenteCuenFabricante': '" + $('#txtGerenteCuenFabricante').val() + "',";
        datosFormulario = datosFormulario + "'txtLiderProyectoCliente': '" + $('#txtLiderProyectoCliente').val() + "',";
        datosFormulario = datosFormulario + "'txtMayorista': '" + $('#txtMayorista').val() + "',";
        datosFormulario = datosFormulario + "'txtFormaPago': '" + $('#txtFormaPago').val() + "',";

        datosFormulario = datosFormulario + "'txtFechaInicioProyecto': '" + $('#txtFechaInicioProyecto').val() + "',";
        datosFormulario = datosFormulario + "'txtFechaFacturacionProyecto': '" + $('#txtFechaFacturacionProyecto').val() + "',";
        datosFormulario = datosFormulario + "'txtContactoLiderProyecto': '" + $('#txtContactoLiderProyecto').val() + "',";
        datosFormulario = datosFormulario + "'cboJustificacion': '" + $('#cboJustificacion').val() + "',";

        datosFormulario = datosFormulario + "'Tipo': '" + "0" + "'";
        datosFormulario = datosFormulario + "}";

        datos = "[{'action': 'GuardarNuevoForecast', 'parameters' : " + datosFormulario + " }]";

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
                    BorrarBotonesForecast()
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

function ActualizarForeCast() {
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    if (idCliente == 0) {
        alerta("El cliente no se encuentra registrado, Por favor registrelo");
        return;
    }

    var proceso = "";
    var text = document.getElementById("IdProceso");
    if (text.checked == true) {
        proceso = "SI";
    }
    else {
        proceso = "NO";
    }
    var comboPrioridad2 = document.getElementById("cboPrioridad");
    var selectedoPrioridad2 = comboPrioridad2.options[comboPrioridad2.selectedIndex].text;


    if ($('#cboMarca').val() == "0") {
        mensajeVerificacion += "- Debe ingresar la marca ";
        contadorVerificacion += 1;
    }
    else {
        var comboMarca = document.getElementById("cboMarca");
        var selectedoMarca = comboMarca.options[comboMarca.selectedIndex].text;
    }

    if ($('#cboCliente2').val() == "") {
        mensajeVerificacion += "- Debe ingresar el cliente ";
        contadorVerificacion += 1;
    }

    if ($('#txtDetalleProyecto').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre del proyecto ";
        contadorVerificacion += 1;
    }

    if ($('#txtPrecioEstimado').val() == "") {
        mensajeVerificacion += "- Debe ingresar el PVP Estimado ";
        contadorVerificacion += 1;
    }

    if ($('#txtUtilidadBruta').val() == "") {
        mensajeVerificacion += "- Debe ingresar la Util. Bruta Estimada ";
        contadorVerificacion += 1;
    }

    //if ($('#cboFecha1').val() == "-- SELECCIONE --") {
    //    mensajeVerificacion += "- Debe ingresar la fecha facturación ";
    //    contadorVerificacion += 1;
    //}

    if ($('#cboCierreNegocio').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el estado del negocio ";
        contadorVerificacion += 1;
    }
    else {
        var comboNegocio = document.getElementById("cboCierreNegocio");
        var selectedoNegocio = comboNegocio.options[comboNegocio.selectedIndex].text;
    }

    //if ($('#cboFecha1').val() == "-- SELECCIONE --") {
    //    mensajeVerificacion += "- Debe ingresar el Mes Estimado de Cierre ";
    //    contadorVerificacion += 1;
    //}

    if ($('#txtObservacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar el estado actual y siguientes pasos ";
        contadorVerificacion += 1;
    }

    if ($('#cboGerente').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el gerente producto ";
        contadorVerificacion += 1;
    }
    else {
        var combooGerente = document.getElementById("cboGerente");
        var selectedoGerente = combooGerente.options[combooGerente.selectedIndex].text;
    }

    if ($('#cboVendedor').val() == "0") {
        mensajeVerificacion += "- Debe ingresar el gerente cuenta ";
        contadorVerificacion += 1;
    }
    else {
        var comboVendedor = document.getElementById("cboVendedor");
        var selectedoVendedor = comboVendedor.options[comboVendedor.selectedIndex].text;
    }

    if ($('#cboInstitucion').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar el tipo de institucion ";
        contadorVerificacion += 1;
    }

    if ($('#cboSegmentacion').val() == "0") {
        mensajeVerificacion += "- Debe ingresar la segmentacion ";
        contadorVerificacion += 1;
    }

    if ($('#cboSucursal').val() == "SELECCIONAR SUCURSAL") {
        mensajeVerificacion += "- Debe ingresar la sucursal ";
        contadorVerificacion += 1;
    }

    //if ($('#cboRegistro').val() == "SELECCIONAR") {
    //    mensajeVerificacion += "- Debe ingresar el Registro Aprobado ";
    //    contadorVerificacion += 1;
    //}

    if ($('#txtFechaFacturacionProyecto').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha estimada de cierre ";
        contadorVerificacion += 1;
    }

    if ($('#txtFechaInicioProyecto').val() == "" && selectedoPrioridad2 == "FORECAST") {
        mensajeVerificacion += "- Debe ingresar la Fecha Facturación ";
        contadorVerificacion += 1;
    }

    if ($('#cboPrioridad').val() == "0") {
        mensajeVerificacion += "- Debe ingresar la prioridad ";
        contadorVerificacion += 1;
    }
    else {

        var comboPrioridad = document.getElementById("cboPrioridad");
        var selectedoPrioridad = comboPrioridad.options[comboPrioridad.selectedIndex].text;
    }

    //if ($('#txtNumeroRegistro').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el Número registro ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtGerenteCuenFabricante').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el Gerente Cuenta Fabricante ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtLiderProyectoCliente').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el Lider Proyecto Cliente ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtMayorista').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el Mayorista ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtFormaPago').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el Forma Pago ";
    //    contadorVerificacion += 1;
    //}

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'IdForeCast': '" + IdForeCast + "',";
    datosFormulario = datosFormulario + "'IdMarca': '" + $('#cboMarca').val() + "',";
    datosFormulario = datosFormulario + "'cboMarca': '" + selectedoMarca + "',";
    datosFormulario = datosFormulario + "'cboCliente2': '" + $('#cboCliente2').val() + "',";
    datosFormulario = datosFormulario + "'idCliente': '" + idCliente + "',";
    datosFormulario = datosFormulario + "'txtDetalleProyecto': '" + $('#txtDetalleProyecto').val() + "',";
    datosFormulario = datosFormulario + "'txtPrecioEstimado': '" + $('#txtPrecioEstimado').val() + "',";
    datosFormulario = datosFormulario + "'txtUtilidad': '" + $('#txtUtilidad').val().replace(".", ",") + "',";
    datosFormulario = datosFormulario + "'txtUtilidadBruta': '" + $('#txtUtilidadBruta').val() + "',";
    datosFormulario = datosFormulario + "'cboFecha1': '" + $('#cboFecha1').val() + "',";
    datosFormulario = datosFormulario + "'IdNegocio': '" + $('#cboCierreNegocio').val() + "',";
    datosFormulario = datosFormulario + "'cboCierreNegocio': '" + selectedoNegocio + "',";
    datosFormulario = datosFormulario + "'cboFecha2': '" + $('#cboFecha2').val() + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    datosFormulario = datosFormulario + "'cboGerente': '" + selectedoGerente + "',";
    datosFormulario = datosFormulario + "'IdGerenteProducto': '" + $('#cboGerente').val() + "',";
    datosFormulario = datosFormulario + "'cboVendedor': '" + selectedoVendedor + "',";
    datosFormulario = datosFormulario + "'IdGerenteCuenta': '" + $('#cboVendedor').val() + "',";
    datosFormulario = datosFormulario + "'cboInstitucion': '" + $('#cboInstitucion').val() + "',";
    datosFormulario = datosFormulario + "'cboSegmentacion': '" + $('#cboSegmentacion').val() + "',";
    datosFormulario = datosFormulario + "'cboSucursal': '" + $('#cboSucursal').val() + "',";
    datosFormulario = datosFormulario + "'cboRegistro': '" + proceso + "',";
    datosFormulario = datosFormulario + "'IdPrioridad': '" + $('#cboPrioridad').val() + "',";
    datosFormulario = datosFormulario + "'cboPrioridad': '" + selectedoPrioridad + "',";

    datosFormulario = datosFormulario + "'txtNumeroRegistro': '" + $('#txtNumeroRegistro').val() + "',";
    datosFormulario = datosFormulario + "'txtGerenteCuenFabricante': '" + $('#txtGerenteCuenFabricante').val() + "',";
    datosFormulario = datosFormulario + "'txtLiderProyectoCliente': '" + $('#txtLiderProyectoCliente').val() + "',";
    datosFormulario = datosFormulario + "'txtMayorista': '" + $('#txtMayorista').val() + "',";
    datosFormulario = datosFormulario + "'txtFormaPago': '" + $('#txtFormaPago').val() + "',";

    datosFormulario = datosFormulario + "'txtFechaInicioProyecto': '" + $('#txtFechaInicioProyecto').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaFacturacionProyecto': '" + $('#txtFechaFacturacionProyecto').val() + "',";
    datosFormulario = datosFormulario + "'txtContactoLiderProyecto': '" + $('#txtContactoLiderProyecto').val() + "',";
    datosFormulario = datosFormulario + "'cboJustificacion': '" + $('#cboJustificacion').val() + "',";

    datosFormulario = datosFormulario + "'Tipo': '" + "0" + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoForecast', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesForecast()
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

function BorrarBotonesForecast() {

    var combo1 = document.getElementById("cboMarca");
    combo1.value = 0;
    $('#cboCliente2').val("");
    $('#cboCliente3').val("");
    $('#txtDetalleProyecto').val("");
    $('#txtPrecioEstimado').val("");
    $('#txtUtilidad').val("");
    $('#txtUtilidadBruta').val("");

    var combo2 = document.getElementById("cboFecha1");
    combo2.value = "-- SELECCIONE --";
    var combo3 = document.getElementById("cboCierreNegocio");
    combo3.value = 0;
    var combo4 = document.getElementById("cboFecha2");
    combo4.value = "-- SELECCIONE --";
    $('#txtObservacion').val("");
    var combo5 = document.getElementById("cboGerente");
    combo5.value = 0;
    var combo6 = document.getElementById("cboVendedor");
    combo6.value = 0;
    var combo7 = document.getElementById("cboInstitucion");
    combo7.value = "SELECCIONAR";
    var combo8 = document.getElementById("cboSegmentacion");
    combo8.value = 0;
    var combo9 = document.getElementById("cboSucursal");
    combo9.value = "SELECCIONAR SUCURSAL";
    var combo10 = document.getElementById("cboRegistro");
    combo10.value = "SELECCIONAR";
    var combo11 = document.getElementById("cboPrioridad");
    combo11.value = 0;
    idCliente = 0;
    idCliente2 = 0;

    $('#txtNumeroRegistro').val("");
    $('#txtGerenteCuenFabricante').val("");
    $('#txtLiderProyectoCliente').val("");
    $('#txtContactoLiderProyecto').val("");
    $('#txtMayorista').val("");
    $('#txtFormaPago').val("");

    $('#txtFechaInicioProyecto').val("");
    $('#txtFechaFacturacionProyecto').val("");
    document.getElementById("IdJustificacion").style.display = "none";
}

function BuscarPrioridad() {
    var comboPrioridad = document.getElementById("cboPrioridad");
    var selectedoPrioridad = comboPrioridad.options[comboPrioridad.selectedIndex].text;
    if (selectedoPrioridad == "PERDIDO" || selectedoPrioridad == "DESCARTADO") {
        document.getElementById("IdJustificacion").style.display = "block";
    }
    else {
        document.getElementById("IdJustificacion").style.display = "none";
    }
}

function NuevoForeCast() {
    IdGuardar = 0;
    var combo1 = document.getElementById("cboMarca");
    combo1.value = 0;
    $('#cboCliente2').val("");
    $('#txtDetalleProyecto').val("");
    $('#txtPrecioEstimado').val("");
    $('#txtUtilidad').val("");
    $('#txtUtilidadBruta').val("");
    $('#txtUtilidadBruta').val("");
    var combo2 = document.getElementById("cboFecha1");
    combo2.value = "-- SELECCIONE --";
    var combo3 = document.getElementById("cboCierreNegocio");
    combo3.value = 0;
    var combo4 = document.getElementById("cboFecha2");
    combo4.value = "-- SELECCIONE --";
    $('#txtObservacion').val("");
    var combo5 = document.getElementById("cboGerente");
    combo5.value = 0;
    var combo6 = document.getElementById("cboVendedor");
    combo6.value = 0;
    var combo7 = document.getElementById("cboInstitucion");
    combo7.value = "SELECCIONAR";
    var combo8 = document.getElementById("cboSegmentacion");
    combo8.value = 0;
    var combo9 = document.getElementById("cboSucursal");
    combo9.value = "SELECCIONAR SUCURSAL";
    var combo10 = document.getElementById("cboRegistro");
    combo10.value = "SELECCIONAR";
    var combo11 = document.getElementById("cboPrioridad");
    combo11.value = 0;
    idCliente = 0;
    idCliente2 = 0;

    $('#txtNumeroRegistro').val("");
    $('#txtGerenteCuenFabricante').val("");
    $('#txtLiderProyectoCliente').val("");
    $('#txtMayorista').val("");
}

function BtnCrearCliente() {
    tipo = 0;
    document.getElementById("modalMensajeAprobarLabelTitulo").innerHTML = "Registro de Cliente";
    $("#modalMensajeAprobar").modal('show');
}

function BtnCrearMarca() {
    tipo = 4;
    document.getElementById("modalMensajeAprobarLabelTitulo").innerHTML = "Registro de Marca";
    $("#modalMensajeAprobar").modal('show');

}

function GuardarCliente() {

    //ObtenerValidacionClienteForeCast($('#txtObservacionReq').val(), 1);

    if ($('#txtObservacionReq').val() == "") {
        alerta("Debe registrar la descripción..");
    }
    else {

        //if (ExisteCliente == 0) {

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
                        ExisteCliente = 0;
                        ObtenerListaComboMarca();
                        ObtenerListaComboMarca2();
                        $('#txtObservacionReq').val("");
                        MensajeCorrecto(respuesta.mensaje);
                        $("#modalMensajeAprobar").modal('hide');
                        $("#divMensajes").html("");
                    }
                    else if (respuesta.estado == "0") {
                        MensajeIncorrecto(respuesta.mensaje);
                    }
                    else if (respuesta.estado == "-1") {
                        MensajeIncorrecto(respuesta.mensaje);
                    }
                },
                error: function (objeto, msgError, objError) {
                    var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
                    MensajeIncorrecto(mesnajeError);
                }
            });
        }
        //}
        //else {
        //    alerta("El cliente ya se encuentra registrado..");
        //}
        return;

    }
}

function LimpiarBusqueda() {

    var combo1 = document.getElementById("cboAnio1");
    combo1.value = 0;
    ObtenerListaComboFecha1Buscar();
}

$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    var dateFormat = "dd/mm/yy";

    tofrom13 = $("#txtFechaInicioProyecto").datepicker(
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

    tofrom14 = $("#txtFechaFacturacionProyecto").datepicker(
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

    //$("#txtFechaInicioProyecto").datepicker('setDate', 'today');
    //$("#txtFechaFacturacionProyecto").datepicker('setDate', 'today');

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

    ObtenerListaComboMarca2();
    ObtenerListaComboGerenteProducto2()
    ObtenerListaComboGerenteCuentaUnico2();
    ObtenerListaComboSucursal2();
    ObtenerListaComboSegmentacion2();
    ObtenerListaComboPrioridad2();
    ObtenerListaComboFecha1Buscar();

    if ($("#ContentPlaceHolder1_txtIdTipo").val() == "7" ) {
        document.getElementById("btnDescargar").style.display = "none";
    }
});

$(document).ready(function () {

    $('.js-example-basic-multiple').select2({
        width: '300px'
    });

    $('.js-example-basic-multiple2').select2({
        width: '300px'
    });

    $('.js-example-basic-multiple3').select2({
        width: '300px'
    });

    $('.js-example-basic-multiple4').select2({
        width: '300px'
    });

    $('.js-example-basic-multiple5').select2({
        width: '300px'
    });

    $('.js-example-basic-multiple6').select2({
        width: '250px'
    });

});