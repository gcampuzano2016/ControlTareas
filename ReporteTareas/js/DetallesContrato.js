var tipoGeneral = 0;
var idregistroGeneral = 0;
var os = "";
var bandera = 0;
var tiempoEntregado = 0;
var ObtenerValor = 0;
var idCliente2 = 0;
//detalle de tareas

function ObtenerListaClientes(tipo, descripcion, idproceso) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", idproceso);
}

function BuscarCliente2() {

    if ($('#cboCliente2').val().length > 4) {
        idSeleccionado = 0;
        ObtenerListaClientes(1, $('#cboCliente2').val(), 4);
    }
    else {
        document.getElementById("comboClientes2").style.display = "none";
    }
}


function ObtenerListaComboAnioServicios() {
    var Datos = "[{ \"action\": \"ListaComboAnioServicioYGestor\", \"parameters\" : { \"Tipo\" : \"51\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboAnioServicios', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}


function ObtenerListaComboGestor() {
    var Datos = "[{ \"action\": \"ListaComboAnioServicioYGestor\", \"parameters\" : { \"Tipo\" : \"52\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGestor', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function CancelarCambios() {
    // Ocultar formulario y visualizar grid
    $("#formActualizaTarea").hide();
    $("#ContentPlaceHolder1_Panel1").show();

    return;
}


function ObtenerListaReportes() {

    //año
    var comboAnioServicios = document.getElementById("cboAnioServicios");
    var selectedcomboAnioServicios = comboAnioServicios.options[comboAnioServicios.selectedIndex].text;
    //gestor responsable
    var comboGestor = document.getElementById("cboGestor");
    var selectedcomboGestor = comboGestor.options[comboGestor.selectedIndex].text;

    //sucursal
    var comboSucusal = document.getElementById("cboSucursalReporte");
    var selectedcomboSucursal = comboSucusal.options[comboSucusal.selectedIndex].text;

    //Estado
    var comboEstado = document.getElementById("cboEstadoReporte");
    var selectedcomboEstado = comboEstado.options[comboEstado.selectedIndex].text;

    //area 
    var comboAreaReporte = document.getElementById("cboAreaReporte");
    var selectedcomboArea = comboAreaReporte.options[comboAreaReporte.selectedIndex].text;


    //Gerente de cuenta 
    var comboGerenteCuenta = document.getElementById("cboGerente3");
    var selectedcomboGerente = comboGerenteCuenta.options[comboGerenteCuenta.selectedIndex].text;
    

    if (selectedcomboAnioServicios == "--SELECCIONE--") {
        selectedcomboAnioServicios = "";
        //alerta("Seleccione un año");

    }
    $("#datosTablaPrincipal6").show();
    $("#datosTablaPrincipal7").hide();
    $("#datosTablaPrincipal8").hide();
    $("#botonRegresar").hide();
    $("#botonRegresarTabla7").hide();


    var Datos = "[{ \"action\": \"ListaReportes\", \"parameters\" : {  \"Anio\" : \"" + selectedcomboAnioServicios + "\", Gestor:\"" + selectedcomboGestor + "\", Sucursal:\"" + selectedcomboSucursal + "\", Estado:\"" + selectedcomboEstado + "\", Area:\"" + selectedcomboArea + "\", GerenteCuenta:\"" + selectedcomboGerente +"\"} }]";
    CargarPagina('#datosTablaPrincipal6', 'ObtenerListaTareas.ashx', Datos, "table", "", "");
}

function ListaRequerimientos(os, fechaInicio, fechaFinal, tipo) {

    var combooClasificacion = document.getElementById("cboClasificacion4");
    var selectedoClasificacion = combooClasificacion.options[combooClasificacion.selectedIndex].text;

    if (selectedoClasificacion == "-- SELECCIONE --") {
        selectedoClasificacion = "";
    }

    var Datos = "[{ \"action\": \"ListaRequerimientosContrato\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", os: \"" + os + "\", tipo: \"" + tipo + "\", IdCliente: \"" + $("#cboCliente4").val() + "\", Clasificacion: \"" + selectedoClasificacion + "\"} }]";
    CargarPagina('#datosTablaPrincipal5', 'ObtenerListaTareas.ashx', Datos, "tableSelect5", tipo);
}

function ListaRequerimientosDescarga(os, fechaInicio, fechaFinal, tipo) {

    var combooClasificacion = document.getElementById("cboClasificacion4");
    var selectedoClasificacion = combooClasificacion.options[combooClasificacion.selectedIndex].text;

    if (selectedoClasificacion == "-- SELECCIONE --") {
        selectedoClasificacion = "";
    }

    var Datos = "[{ \"action\": \"ListaRequerimientosContratoDescargar\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", os: \"" + os + "\", tipo: \"" + tipo + "\", IdCliente: \"" + $("#cboCliente4").val() + "\", Clasificacion: \"" + selectedoClasificacion + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal5', 'ObtenerListaTareas.ashx', Datos, "table");
}

function ObtenerListaTareasHorasExtras(tipo, idRegistro,busqueda,os,fechaInicio,fechaFinal) {
    var Datos = "[{ \"action\": \"ListaConsultaTareasGeneradasContrato\", \"parameters\" : { registro : \"" + idRegistro + "\", fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", estado: \"" + tipo + "\", busqueda: \"" + busqueda + "\", os: \"" + os + "\", idCliente: \"" + "0" + "\", idProceso: \"" + "0" + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelect2", tipo);
}

function ObtenerListaHorasExtrasDescarga(tipo, idRegistro, busqueda, os, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ListaConsultaTareasContratoDescargaXLS\", \"parameters\" : { registro : \"" + idRegistro + "\", fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", estado: \"" + tipo + "\", busqueda: \"" + busqueda + "\", os: \"" + os + "\", idCliente: \"" + "0" + "\", idProceso: \"" + "0" + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "table");
}

function ObtenerListaTareasHorasOS(tipo, idRegistro, busqueda, os, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ListaConsultaTareasGeneradasContrato\", \"parameters\" : { registro : \"" + idRegistro + "\", fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", estado: \"" + tipo + "\", busqueda: \"" + busqueda + "\", os: \"" + os + "\", idCliente: \"" + $("#cboCliente3").val() + "\", idProceso: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal4', 'ObtenerListaTareas.ashx', Datos, "tableSelect3", tipo);
}

function ObtenerListaTareasHorasContratadas(tipo, idRegistro, busqueda, os, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ListaConsultaHorasContratadas\", \"parameters\" : { registro : \"" + idRegistro + "\", fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", estado: \"" + tipo + "\", busqueda: \"" + busqueda + "\", os: \"" + os + "\", idCliente: \"" + $("#cboCliente3").val() + "\", idProceso: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('', 'ObtenerListaTareas.ashx', Datos, "tableSelect4", tipo);
}

function ObtenerListaTareasHorasDescargarOS(tipo, idRegistro, busqueda, os, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ListaConsultaTareasContratoDescargaXLS\", \"parameters\" : { registro : \"" + idRegistro + "\", fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", estado: \"" + tipo + "\", busqueda: \"" + busqueda + "\", os: \"" + os + "\", idCliente: \"" + $("#cboCliente3").val() + "\", idProceso: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal4', 'ObtenerListaTareas.ashx', Datos, "table");
}


function ObtenerReporteDescarga(){

    //año
    var comboAnioServicios = document.getElementById("cboAnioServicios");
    var selectedcomboAnioServicios = comboAnioServicios.options[comboAnioServicios.selectedIndex].text;
    //gestor responsable
    var comboGestor = document.getElementById("cboGestor");
    var selectedcomboGestor = comboGestor.options[comboGestor.selectedIndex].text;

    //sucursal
    var comboSucusal = document.getElementById("cboSucursalReporte");
    var selectedcomboSucursal = comboSucusal.options[comboSucusal.selectedIndex].text;

    //Estado
    var comboEstado = document.getElementById("cboEstadoReporte");
    var selectedcomboEstado = comboEstado.options[comboEstado.selectedIndex].text;

    //area 
    var comboAreaReporte = document.getElementById("cboAreaReporte");
    var selectedcomboArea = comboAreaReporte.options[comboAreaReporte.selectedIndex].text;

    //Gerente de cuenta 
    var comboGerenteCuenta = document.getElementById("cboGerente3");
    var selectedcomboGerente = comboGerenteCuenta.options[comboGerenteCuenta.selectedIndex].text;
    


    if (selectedcomboAnioServicios == "--SELECCIONE--") {
        selectedcomboAnioServicios = "";
        //alerta("Seleccione un año");

    }


    
    if ($('#datosTablaPrincipal6').is(':visible')) {
        var Datos = "[{ \"action\": \"ReporteDescargaXLS\", \"parameters\" : {  \"Anio\" : \"" + selectedcomboAnioServicios + "\", Gestor:\"" + selectedcomboGestor + "\", Sucursal:\"" + selectedcomboSucursal + "\", Estado:\"" + selectedcomboEstado + "\", Area:\"" + selectedcomboArea + "\", GerenteCuenta:\"" + selectedcomboGerente+"\"} }]";
        DetalleTareasDescargaXLS('#datosTablaPrincipal6', 'ObtenerListaTareas.ashx', Datos, "table");
    } else if ($('#datosTablaPrincipal7').is(':visible')) {
        //ORDEN 
        var valorBoton = ObtenerValor;


        var Datos = "[{ \"action\": \"ReporteDescargaTabla7XLS\", \"parameters\" : {  \"Tipo\" : \"1\",\"ORDEN\" : \"" + valorBoton + "\"} }]";
        DetalleTareasDescargaXLS('#datosTablaPrincipal7', 'ObtenerListaTareas.ashx', Datos, "table2");
    } else if ($('#datosTablaPrincipal8').is(':visible')) {
        //ORDEN 
        var botonordenSeleccion = ObtenerValor;
        var Datos = "[{ \"action\": \"ReporteDescargaTabla7XLS\", \"parameters\" : {  \"Tipo\" : \"2\",\"ORDEN\" : \"" + botonordenSeleccion + "\"} }]";
        DetalleTareasDescargaXLS('#datosTablaPrincipal8', 'ObtenerListaTareas.ashx', Datos, "table3");
    }
}

function VerListadoArchivosTarea(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
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

//detalle de tareas

//detalle combos

function ObtenerListaComboClientes() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"0\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboCliente', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboClasificacion() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"1\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboClasificacion', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuentas() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"2\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGestorResponsable() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"3\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboResponsable', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboClientes2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"0\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboCliente2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboClientes3() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"0\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboCliente3', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboClientes4() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"0\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboCliente4', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuentas2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"2\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuentasUnico() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"20\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuentas3() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"2\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente3', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGerenteCuentasUnico3() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"20\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboGerente3', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboGestorResponsable2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"3\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboResponsable2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function RecorreJSONSelect(div, selectValues) {

    $(div).empty();

    if (selectValues.length > 0) {

        $.each(selectValues, function (key, value) {
            //
            // Despliegue de datos
            //
            $(div)
                .append($("<option></option>")
                    .attr("value", value.Id)
                    .text(value.Valor));
        });
    }

    return;
}

//detalle combos

function BtnDescargaOrden() {
    $("#divMensajes").html("");

    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros2");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaConsulta3").val();
        fechaFinal = $("#txtFechaConsulta4").val();
    }

    ObtenerListaHorasExtrasDescarga(tipoGeneral, idregistroGeneral, "", os, fechaInicio, fechaFinal);
}



function BtnDescargaReporte() {
    $("#divMensajes").html("");

    var anio = document.getElementById("cboAnioServicios");
    var selectedAnio = anio.options[anio.selectedIndex].text;
    if (selectedAnio == "--SELECCIONE--") {
        alerta("No ha seleccionado un Anio");

    } else {
        ObtenerReporteDescarga();
    }

}

function BtnConsultaOrden() {
    $("#divMensajes").html("");
    tipoGeneral = 0;
    idregistroGeneral = 0;

    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros2");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaConsulta3").val();
        fechaFinal = $("#txtFechaConsulta4").val();
    }

    ObtenerListaTareasHorasExtras(0, 0, $("#txtNumeroOrden").val(), os, fechaInicio, fechaFinal);
}

function ObtenerListaContratos(tipo, idRegistro, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ReporteListaContratos\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", busqueda: \"" + $("#txtNumeroOrden2").val() + "\", IdCliente: \"" + idCliente2 + "\", IdGerenteCuenta: \"" + $("#cboGerente2").val() + "\", IdGestorResponsable: \"" + $("#cboResponsable2").val() + "\", sucursal: \"" + $("#cboSucursal2").val() + "\", estado: \"" + $("#cboEstado2").val() + "\", area: \"" + $("#cboArea2").val() + "\" , IdClasificacion: \"" + '0' + "\" } }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "tableSelect", tipo);
}

function ObtenerConsultarContratos(idServicio, orden, tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarContratos\", \"parameters\" : { idServicio: \"" + idServicio + "\", orden: \"" + orden + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "detalleDatos", tipo);
}

function ObtenerListaContratosDescarga(tipo, idRegistro, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ListaContratosDescargaXLS\", \"parameters\" : { fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", busqueda: \"" + $("#txtNumeroOrden2").val() + "\", IdCliente: \"" + $("#cboCliente2").val() + "\", IdGerenteCuenta: \"" + $("#cboGerente2").val() + "\", IdGestorResponsable: \"" + $("#cboResponsable2").val() + "\", sucursal: \"" + $("#cboSucursal2").val() + "\", estado: \"" + $("#cboEstado2").val() + "\", area: \"" + $("#cboArea2").val() + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table");
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
        document.getElementById("idFiltros2").checked = true
    } else {
        fechaInicio = $("#txtFechaConsulta1").val();
        fechaFinal = $("#txtFechaConsulta2").val();
    }

    ObtenerListaContratosDescarga(0, 0, fechaInicio, fechaFinal);
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

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
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
                        alerta("No existe tareas registradas..");
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


function CargarCliente2(ID, NOMBRE) {
    $('#cboCliente2').val(NOMBRE);
    idCliente2 = ID;
    document.getElementById("comboClientes2").style.display = "none";
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
    else if (idSeleccionado == 7) {

        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarEmpleado(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboEmpleado").innerHTML = x;
        document.getElementById("comboEmpleado").style.display = "block";
    }
    else if (idSeleccionado == 8) {

        $.each(json, function (i, item) {
            x = x + "<li><a role='option' onclick='CargarEmpleadoPrincipal(\"" + item.ID + "\", \"" + item.NOMBRE + "\");'>" + item.NOMBRE + "</a></li>";
        });

        document.getElementById("comboEmpleadoPrincipal").innerHTML = x;
        document.getElementById("comboEmpleadoPrincipal").style.display = "block";
    }

}

function RecorreJSON(div, json, tipoControl, boton, idSeleccionado) {

    var contenido = "";

    if (tipoControl == "table") {
        contenido = RecorreJSONTable(json);
        $(div).html(contenido);
        
    }
    if (tipoControl == "table3") {
        contenido = RecorreJSONTableDetalleOrden(json);
        $(div).html(contenido);

    }
    if (tipoControl == "tableSelectBusqueda") {
        contenido = RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    if (tipoControl == "table2") {
        contenido = RecorreJSONTableOrden(json);
        $(div).html(contenido);

    }
    if (tipoControl == "select") {
        contenido = RecorreJSONSelect(div, json, idSeleccionado);
    }
    if (tipoControl == "form") {
        contenido = RecorreJSONForm(json);
    }
    if (tipoControl == "tableSelect") {
        contenido = RecorreJSONTableSelect(json, boton);
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

    if (tipoControl == "tableSelect5") {
        contenido = RecorreJSONTableSelect5(json, boton, idSeleccionado);

        $(div).html(contenido);
    }

    if (tipoControl == "tableSelectChange") {
        contenido = RecorreJSONTableSelectChange(json);
        $(div).html(contenido);
    }
    if (tipoControl == "detalleDatos") {
        contenido = RecorreDatosPantalla(json);
        $(div).html(contenido);
    }

    return contenido;
}


//NUEVO 


function RecorreJSONForm(div, json,boton) {
    var info = "";

    // Limpiar formulario

    $("#fomTitleLabel").html("Detalle de Orden");
   

    // Ocultar grid y visualizar formulario
    $("#ContentPlaceHolder1_Panel1").hide();
    $("#formActualizaTarea").show();
    $(boton).show();

    var iniciaBarrido = true;
    $.each(json, function (i, item) {

        if (iniciaBarrido) {

            //
            // Despliegue de titulos de cabecera
            //
            var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
            var thFinal = "</th>";
            info = info + "<table id='tbl_perfiles'width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
            info = info + "<thead><tr role='row'>";
            info = info + thInicial + "ORDEN" + thFinal;
            info = info + thInicial + "ID_TAREA" + thFinal;
            info = info + thInicial + "ID_COMPRA" + thFinal;
            info = info + thInicial + "RESPONSABLE" + thFinal;
            info = info + thInicial + "EMPRESA" + thFinal;
            info = info + thInicial + "DETALLE" + thFinal;
            info = info + thInicial + "FECHA CREACION" + thFinal;
           

            //info = info + thInicial + "ESATDO" + thFinal;
            info = info + "</tr></thead>";

            info = info + "<tbody>";

            iniciaBarrido = false;
        }
        
        



        info = info + "</td>";

        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;



    //
    // Llenado de formulario con datos del registro seleccionado
   

    $("#divMensajes").html("");


    return info;
}


function BtnOrden(ORDEN) {
    $("#datosTablaPrincipal6").hide();
    $("#datosTablaPrincipal7").show();
    $("#botonRegresar").show();
    $("#divMensajes").html("");
    ObtenerOrdenServicio(ORDEN);
    ObtenerValor = ORDEN;
}

function BtnRegresar() {
    $("#datosTablaPrincipal7").hide();
    $("#datosTablaPrincipal6").show();
    $("#botonRegresar").hide();
}

function BtnRegresarTabla7() {
    $("#datosTablaPrincipal7").show();
    $("#datosTablaPrincipal8").hide();
    $("#botonRegresarTabla7").hide();
    $("#botonRegresar").show();
}

function ObtenerOrdenServicio(ORDEN) {

   
    var Datos = "[{ \"action\": \"DetalleOrdenServicio\", \"parameters\" : {  \"Tipo\" : \"1\",\"ORDEN\" : \"" + ORDEN + "\"} }]";
    CargarPagina('#datosTablaPrincipal7', 'ObtenerListaTareas.ashx', Datos, "table2", "", "");
}




function BtnDetalleOrden(ORDEN) {
    $("#datosTablaPrincipal7").hide();
    $("#datosTablaPrincipal8").show();
    $("#botonRegresarTabla7").show();
    $("#botonRegresar").hide();
    $("#divMensajes").html("");
    ObtenerDetalleOrdenServicio(ORDEN);
}

function ObtenerDetalleOrdenServicio(ORDEN) {


    var Datos = "[{ \"action\": \"DetalleOrdenServicio\", \"parameters\" : {  \"Tipo\" : \"2\",\"ORDEN\" : \"" + ORDEN + "\"} }]";

    CargarPagina('#datosTablaPrincipal8', 'ObtenerListaTareas.ashx', Datos, "table3", "", "");

}

function RecorreJSONTable(json) {
    var info = "";
    var esImpar = true;
    var iniciaBarrido = true;
    var totalValor3 = 0;
    $.each(json, function (i, item) {

        if (iniciaBarrido) {

            //
            // Despliegue de titulos de cabecera
            //
            var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
            var thFinal = "</th>";
            info = info + "<table id='tbl_perfiles'width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
            info = info + "<thead><tr role='row'>";
            info = info + thInicial + "FECHA" + thFinal;
            info = info + thInicial + "PEDIDO" + thFinal;
            info = info + thInicial + "ORDEN" + thFinal;
            info = info + thInicial + "CLIENTE" + thFinal;
            info = info + thInicial + "AREA" + thFinal;
            info = info + thInicial + "SUCURSAL" + thFinal;
            info = info + thInicial + "RESPONSABLE" + thFinal;
            info = info + thInicial + "ESTADO" + thFinal;
            info = info + thInicial + "HORAS CONTRATADAS" + thFinal;
            info = info + thInicial + "HORAS ENTREGADAS" + thFinal;
            info = info + thInicial + "HORAS DISPONIBLES" + thFinal;
            info = info + thInicial + "GERENTE CUENTA" + thFinal;

            //info = info + thInicial + "ESATDO" + thFinal;
            info = info + "</tr></thead>";

            info = info + "<tbody>";

            iniciaBarrido = false;
        }
        //
        // Despliegue de datos
        // 
        info = info + "<td class='sorting_1'>" + item.FECHA + "</td>";

        info = info + "<td class='sorting_1'>" + item.PEDIDO + "</td>";
        //info = info + "<td class='sorting_1'>" + item.ORDEN + "</td>";
        //info = info + "<td>" + "<button type='button' data-bs-toggle='modal' data-bs-target='#payments - 1'onclick='ObtenerDetalleOrdenServicio(\"" + item.ORDEN + "\" >" + item.ORDEN+"</button>"+"</td>"
        //info = info + "<td><button  style='cursor: pointer;' onclick='BtnOrden(\"" + item.ORDEN + "\");'>" + item.ORDEN + "</button></td>"
        info = info + "<td><button type='button' title='Ver Orden' id='botonOrdenSeleccion' class='btn btn-warning btn-sm' style='cursor: pointer;' onclick='BtnOrden(\"" + item.ORDEN + "\");'>" + item.ORDEN + "</button></td>"

        //info = info +   "< td > <button type='button' class='btn btn-warning btn-sm' data-bs-toggle='modal' data-bs-target='#payments-' . $id . '> "+ item.ORDEN +" <i class='fas fa-euro-sign'></i> </button></td >"

        info = info + "<td class='sorting_1'>" + item.CLIENTE + "</td>";
        info = info + "<td class='sorting_1'>" + item.AREA + "</td>";
        info = info + "<td class='sorting_1'>" + item.SUCURSAL + "</td>";
        info = info + "<td class='sorting_1'>" + item.GESTOR_RESPONSABLE + "</td>";
        info = info + "<td class='sorting_1'>" + item.ESTATUS + "</td>";
        info = info + "<td class='sorting_1'>" + item.HORAS_CONTRATADAS + "</td>";
        info = info + "<td class='sorting_1'>" + item.HORAS_ENTREGADAS + "</td>";
        info = info + "<td class='sorting_1'>" + item.HORAS_DISPONIBLES + "</td>";
        info = info + "<td class='sorting_1'>" + item.GERENTE_DE_CUENTA + "</td>";



        //if (item.Estado == 0) {
        //    info = info + "<td class='text-center'><select id='cboEstadoTabla' class='form-control' name='cboEstadoTabla'><option value='0' selected=''>ACTIVO</option><option value='1'>INACTIVO</option></select></td>";

        //}
        //else if (item.Estado == 1) {
        //    info = info + "<td class='text-center'><select id='cboEstadoTabla' class='form-control' name='cboEstadoTabla'><option value='0'>ACTIVO</option><option value='1' selected=''>INACTIVO</option></select></td>";

        //}

        // info = info + "<td class='sorting_1' style='text-align:center'>";
        //info = info + "<i class='fa fa-plus' style='cursor: pointer' onclick='AgregarUsuario()' title'Ver por Numero de Orden'></i>";
        //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        //info = info + "<a href='#' class='btn-eliminaVendGeren' title='Eliminar meta' data-id=''><i class='fa fa-trash-o'></i></a>";
        //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        //info = info + "<a href='#' class='btn-EditarVendGeren' title='Editar meta' data-id=''><i class='fa fa-pencil-square-o'></i></a>";
        info = info + "</td>";

        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;

}

function RecorreJSONTableOrden(json) {
    var info = "";
    var esImpar = true;
    var iniciaBarrido = true;
    var totalValor3 = 0;
    $.each(json, function (i, item) {

        if (iniciaBarrido) {

            //
            // Despliegue de titulos de cabecera
            //
            var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
            var thFinal = "</th>";
            info = info + "<table id='tbl_perfiles'width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
            info = info + "<thead><tr role='row'>";
            info = info + thInicial + "ORDEN" + thFinal;
            info = info + thInicial + "ID_TAREA" + thFinal;
            info = info + thInicial + "NOMBRE TICKET" + thFinal;
            info = info + thInicial + "RESPONSABLE" + thFinal;
            info = info + thInicial + "EMPRESA" + thFinal;
            info = info + thInicial + "DETALLE" + thFinal;
            info = info + thInicial + "FECHA_CREACION" + thFinal;

            //info = info + thInicial + "ESATDO" + thFinal;
            info = info + "</tr></thead>";

            info = info + "<tbody>";

            iniciaBarrido = false;
        }
        //
        // Despliegue de datos
        // 

        info = info + "<td><button type='button' title='Ver Detalle Orden' class='btn btn-warning btn-sm' style='cursor: pointer;' onclick='BtnDetalleOrden(\"" + item.ORDEN + "\");'>" + item.ORDEN + "</button></td>"
        info = info + "<td class='sorting_1'>" + item.ID_TAREA + "</td>";
        info = info + "<td class='sorting_1'>" + item.ID_COMPRA + "</td>";
        info = info + "<td class='sorting_1'>" + item.NOM_RESPONSABLE + "</td>";
        info = info + "<td class='sorting_1'>" + item.NOM_EMPRESA + "</td>";
        info = info + "<td class='sorting_1'>" + item.DETALLE + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA_CREACION + "</td>";


        info = info + "</td>";

        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;

}

function RecorreJSONTableDetalleOrden(json) {
    var info = "";
    var esImpar = true;
    var iniciaBarrido = true;
    var totalValor3 = 0;

    var tiempoTotalDia = 0;
    var tiempoTareaDia = 0;
    $.each(json, function (i, item) {

        if (iniciaBarrido) {

            //
            // Despliegue de titulos de cabecera
            //
            var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
            var thFinal = "</th>";
            info = info + "<table id='tbl_perfiles'width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
            info = info + "<thead><tr role='row'>";
            info = info + thInicial + "ID_TAREA" + thFinal;
            info = info + thInicial + "ORDEN" + thFinal;

            info = info + thInicial + "FECHA INICIO" + thFinal;
            info = info + thInicial + "FECHA FIN" + thFinal;
            info = info + thInicial + "TIEMPO" + thFinal;
            info = info + thInicial + "DETALLE TAREA" + thFinal;

            info = info + thInicial + "OBSERVACIONES" + thFinal;



            //info = info + thInicial + "ESATDO" + thFinal;
            info = info + "</tr></thead>";

            info = info + "<tbody>";

            iniciaBarrido = false;
        }

        

                  
               
        //
        // Despliegue de datos
        // 
        info = info + "<td class='sorting_1'>" + item.ID_TAREA + "</td>";
        info = info + "<td class='sorting_1'>" + item.ORDEN + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA_INICIO + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA_FIN + "</td>";
        info = info + "<td class='sorting_1'>" + item.TIEMPO + "</td>";

        tiempoTareaDia = item.TIEMPO.split(':');
        //tiempoTotalDia = (parseInt(tiempoTareaDia[0]) * 60) + parseInt(tiempoTareaDia[1]);
        tiempoTotalDia = tiempoTotalDia + (parseInt(tiempoTareaDia[0]) * 60) + parseInt(tiempoTareaDia[1]);



        info = info + "<td class='sorting_1'>" + item.DETALLE_TAREA + "</td>";
        info = info + "<td class='sorting_1'>" + item.OBSERVACIONES + "</td>";


        info = info + "</td>";

        info = info + "</tr>";
    });

   

    if (tiempoTotalDia == 0 && bandera == 0) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='10'>No informacion registrada</td>";
        info = info + "</tr>";
    }
    else if (tiempoTotalDia > 0) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='9' style='text-align: center;'><b>TOTAL HORAS = " + format_two_digits(Math.floor(tiempoTotalDia / 60)) + ':' + format_two_digits((tiempoTotalDia % 60)) + "</b></td>";
        info = info + "</tr>";
    }
    
    info = info + "</tbody>";
    info = info + "</table>";

    return info;
  

}

function getMinutes(element) {

    var timeElement = moment(element.val(), "HH:mm");
    var minutesTotal = (timeElement.hours() * 60) + timeElement.minutes();

    return parseInt(minutesTotal);
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

function RecorreJSONTableSelect(json, idSeleccionado) {
    var info = "";
    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "ACCIONES" + thFinal;
    info = info + thInicial + "FECHA" + thFinal;
    info = info + thInicial + "PEDIDO" + thFinal;
    info = info + thInicial + "CLIENTE" + thFinal;
    info = info + thInicial + "REFERENCIA CLIENTE" + thFinal;
    info = info + thInicial + "AREA" + thFinal;
    info = info + thInicial + "GESTOR RESPONSABLE" + thFinal;
    info = info + thInicial + "ORDEN" + thFinal;
    info = info + thInicial + "CLASIFICACION" + thFinal;
    info = info + thInicial + "DESCRIPCION DE SERVICIO" + thFinal;
    info = info + thInicial + "SLA CONTRATO" + thFinal;
    info = info + thInicial + "HORAS CONTRATADAS" + thFinal;
    info = info + thInicial + "HORAS ENTREGADAS" + thFinal;
    info = info + thInicial + "HORAS DISPONIBLES" + thFinal;
    info = info + thInicial + "COSTO PLAN" + thFinal;
    info = info + thInicial + "COSTO REAL" + thFinal;
    info = info + thInicial + "SALDO DE COSTOS" + thFinal;
    info = info + thInicial + "ESTATUS" + thFinal;
    info = info + thInicial + "FECHA ESTIMADA DE CIERRE" + thFinal;
    info = info + thInicial + "GERENTE DE CUENTA" + thFinal;
    info = info + thInicial + "SUCURSAL" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";

    var esImpar = true;
    $.each(json, function (i, item) {

        info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.IdServicio + "'>";

        info = info + "<td class='sorting_1' style='text-align:center'>";

        info = info + "<i class='fa fa-hand-o-right' style='cursor: pointer' onclick='CargarTareaContrato(\"" + item.ORDEN + "\");' title'Ver por Numero de Orden'></i>";
        info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        info = info + "<i class='fa fa-square' style='cursor: pointer' onclick='CargarTareaContratoPorOs(\"" + item.ORDEN + "\");' title'Ver numero de contrato'></i>";
        if (item.conteoArchivosAdjuntos != '0') {
            info = info + "<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosContrato(\"#MensajeInformativo\", \"" + item.IdServicio + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        }
        info = info + "</td>";
        info = info + "<td class='sorting_1'>" + item.FECHA + "</td>";
        info = info + "<td class='sorting_1'>" + item.PEDIDO + "</td>";
        info = info + "<td class='sorting_1'>" + item.CLIENTE + "</td>";
        info = info + "<td class='sorting_1'>" + item.REFERENCIA_CLIENTE + "</td>";
        info = info + "<td class='sorting_1'>" + item.AREA + "</td>";
        info = info + "<td class='sorting_1'>" + item.GESTOR_RESPONSABLE + "</td>";
        info = info + "<td class='sorting_1'>" + item.ORDEN + "</td>";
        info = info + "<td class='sorting_1'>" + item.CLASIFICACION + "</td>";
        info = info + "<td class='sorting_1'>" + item.DESCRIPCION_DE_SERVICIO + "</td>";
        info = info + "<td class='sorting_1'>" + item.SLA_CONTRATO + "</td>";
        info = info + "<td class='sorting_1'>" + item.HORAS_CONTRATADAS + "</td>";
        info = info + "<td class='sorting_1'>" + item.HORAS_ENTREGADAS + "</td>";
        info = info + "<td class='sorting_1'>" + item.HORAS_DISPONIBLES + "</td>";
        info = info + "<td class='sorting_1'>" + item.COSTO_PLAN + "</td>";
        info = info + "<td class='sorting_1'>" + item.COSTO_REAL + "</td>";
        info = info + "<td class='sorting_1'>" + item.SALDO_DE_COSTOS + "</td>";
        if (item.ESTATUS == "EN EJECUCION") {
            info = info + "<td class='sorting_1' style='background: " + "#92a88f" + ";text-align: center;'>" + item.ESTATUS + "</td>";
        }
        else if (item.ESTATUS == "CERRADA") {
            info = info + "<td class='sorting_1' style='background: " + "#fe9c7d" + ";text-align: center;'>" + item.ESTATUS + "</td>";
        }
        info = info + "<td class='sorting_1'>" + item.FECHA_ESTIMADA_DE_CIERRE + "</td>";
        info = info + "<td class='sorting_1'>" + item.GERENTE_DE_CUENTA + "</td>";
        info = info + "<td class='sorting_1'>" + item.SUCURSAL + "</td>";
        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function RecorreJSONTableSelect2(json,boton, idSeleccionado) {
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
    info = info + thInicial + "Codigo Tarea" + thFinal;
    info = info + thInicial + "N° de orden de servicio" + thFinal;
    info = info + thInicial + "Cod. Aranda" + thFinal;
    if (idSeleccionado == 0) {

        info = info + thInicial + "Fecha Registro" + thFinal;
    }
    else {
        info = info + thInicial + "Fecha Registro Inicio" + thFinal;
        info = info + thInicial + "Fecha Registro Final" + thFinal;
        info = info + thInicial + "Tiempo" + thFinal;
    }

    info = info + thInicial + "Usuario Responsable" + thFinal;
    info = info + thInicial + "Empresa" + thFinal;
    info = info + thInicial + "SLA" + thFinal;
    info = info + thInicial + "Descripción Tarea" + thFinal;
    info = info + thInicial + "Estado Tarea" + thFinal;
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

        if (esImpar) {
            info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.Id_RegTareas + "'>";
            esImpar = false;
        } else {
            info = info + "<tr class='gradeA even' role = 'row' id='tr-" + item.Id_RegTareas + "'>";
            esImpar = true;
        }
        info = info + "<td class='sorting_1' style='text-align:center'>";
        if (item.conteoArchivosAdjuntos != '0') {
            info = info + "<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosTarea(\"#MensajeInformativo\", \"" + item.Id_RegTareas + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        }
        if (idSeleccionado == 0) {

            info = info + "<i class='fa fa-hand-o-right' style='cursor: pointer' onclick='VerDetalleTareas(\"" + 1 + "\",\"" + item.Id_RegTareas + "\",\"" + item.Num_OrdenServicio + "\");'></i>";
        }
        else {
            info = info + "<i class='fa fa-hand-o-right' style='cursor: pointer' onclick='VerDetalleTareas(\"" + 0 + "\",\"" + item.Id_RegTareas + "\",\"" + item.Num_OrdenServicio + "\");'></i>";
        }
        info = info + "</td > ";
        info = info + "<td class='sorting_1'>" + item.Id_RegTareas + "</td>";
        info = info + "<td class='sorting_1'>" + item.Num_OrdenServicio + "</td>";
        info = info + "<td class='sorting_1'>" + item.Id_CompAranda + "</td>";

        if (idSeleccionado == 0) {

            info = info + "<td class='sorting_1'>" + item.Det_Fch_RegDetalleIni + "</td>";
        }
        else {

            info = info + "<td class='sorting_1'>" + item.Det_Fch_RegDetalleIni + "</td>";
            info = info + "<td class='sorting_1'>" + item.Det_Fch_RegDetalleFin + "</td>";
            info = info + "<td class='sorting_1'>" + item.Det_Tiempo + "</td>";

            tiempoTareaDia = item.Det_Tiempo.split(':');
            //tiempoTotalDia = (parseInt(tiempoTareaDia[0]) * 60) + parseInt(tiempoTareaDia[1]);
            tiempoTotalDia = tiempoTotalDia + (parseInt(tiempoTareaDia[0]) * 60) + parseInt(tiempoTareaDia[1]);


        }

        info = info + "<td class='sorting_1'>" + item.Nom_Responsable + "</td>";
        info = info + "<td class='sorting_1'>" + item.Nom_Empresa + "</td>";
        info = info + "<td class='sorting_1'>" + item.Nom_SlaAranda + "</td>";
        info = info + "<td class='sorting_1'>" + item.Det_Tarea + "</td>";
        info = info + "<td class='sorting_1'>" + item.EstadoTarea + "</td>";
        info = info + "</tr>";
    });

    if (tiempoTotalDia == 0 && bandera == 0) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='10'>No informacion registrada</td>";
        info = info + "</tr>";
    }
    else if (tiempoTotalDia > 0) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='9' style='text-align: right;'><b>TOTAL HORAS:</b></td>";
        info = info + "<td class='sorting_1'><b>" + format_two_digits(Math.floor(tiempoTotalDia / 60)) + ':' + format_two_digits((tiempoTotalDia % 60)) + "</b></td>";
        info = info + "<td class='sorting_1' colspan='8'></td>";
        info = info + "</tr>";
    }
    else if (boton == 0) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='10' style='text-align: center;'>No informacion registrada</td>";
        info = info + "</tr>";
    }
    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}


function RecorreJSONTableSelect5(json, boton, idSeleccionado) {
    var info = "";

    $("#listPrincipalTitleLabel").html("Detalle de Ticket");

    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "---ACCIONES---" + thFinal;
    info = info + thInicial + "Codigo Mant." + thFinal;
    info = info + thInicial + "N° de orden de servicio" + thFinal;
    info = info + thInicial + "Cliente" + thFinal;
    info = info + thInicial + "Fecha Planificada" + thFinal;
    info = info + thInicial + "Fecha Realizada" + thFinal;
    info = info + thInicial + "Descripcion Mant." + thFinal;
    info = info + thInicial + "Clasificacion" + thFinal;
    info = info + thInicial + "Valor" + thFinal;
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

        if (esImpar) {
            info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.IdRequerimiento + "'>";
            esImpar = false;
        } else {
            info = info + "<tr class='gradeA even' role = 'row' id='tr-" + item.IdRequerimiento + "'>";
            esImpar = true;
        }
        info = info + "<td class='sorting_1' style='text-align:center'>";
        if (item.conteoArchivosAdjuntos != '0') {
            info = info + "<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosMantenimiento(\"#MensajeInformativo\", \"" + item.IdRequerimiento + "\", \"" + '5' + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
        }
        info = info + "</td>";
        info = info + "<td class='sorting_1'>" + item.IdRequerimiento + "</td>";
        info = info + "<td class='sorting_1'>" + item.Orden + "</td>";
        info = info + "<td class='sorting_1'>" + item.CLIENTE + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaInicio + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaFinal + "</td>";

        info = info + "<td class='sorting_1'>" + item.Descripcion + "</td>";
        info = info + "<td class='sorting_1'>" + item.Clasificacion + "</td>";
        info = info + "<td class='sorting_1'>" + item.Valor + "</td>";
        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}


function VerListadoArchivosMantenimiento(div, idTarea, idServicio) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + idTarea + "',";
    datosParametros = datosParametros + "'IdServicio': '" + idServicio + "'";
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

function RecorreJSONTableSelect3(json, boton, idSeleccionado) {
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
    info = info + thInicial + "Codigo Tarea" + thFinal;
    info = info + thInicial + "N° de orden de servicio" + thFinal;
    info = info + thInicial + "Cod. Aranda" + thFinal;
    if (idSeleccionado == 0) {

        info = info + thInicial + "Fecha Registro" + thFinal;
    }
    else {
        info = info + thInicial + "Fecha Registro Inicio" + thFinal;
        info = info + thInicial + "Fecha Registro Final" + thFinal;
        info = info + thInicial + "Tiempo" + thFinal;
    }

    info = info + thInicial + "Usuario Responsable" + thFinal;
    info = info + thInicial + "Empresa" + thFinal;
    info = info + thInicial + "SLA" + thFinal;
    info = info + thInicial + "Descripción Tarea" + thFinal;
    info = info + thInicial + "Estado Tarea" + thFinal;
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

        if (esImpar) {
            info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.Id_RegTareas + "'>";
            esImpar = false;
        } else {
            info = info + "<tr class='gradeA even' role = 'row' id='tr-" + item.Id_RegTareas + "'>";
            esImpar = true;
        }
        info = info + "<td class='sorting_1' style='text-align:center'>";
        if (item.conteoArchivosAdjuntos != '0') {
            info = info + "<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosTarea(\"#MensajeInformativo\", \"" + item.Id_RegTareas + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        }
        //if (idSeleccionado == 0) {

        //    info = info + "<i class='fa fa-hand-o-right' style='cursor: pointer' onclick='VerDetalleTareas(\"" + 1 + "\",\"" + item.Id_RegTareas + "\",\"" + item.Num_OrdenServicio + "\");'></i>";
        //}
        //else {
        //    info = info + "<i class='fa fa-hand-o-right' style='cursor: pointer' onclick='VerDetalleTareas(\"" + 0 + "\",\"" + item.Id_RegTareas + "\",\"" + item.Num_OrdenServicio + "\");'></i>";
        //}
        info = info + "</td > ";
        info = info + "<td class='sorting_1'>" + item.Id_RegTareas + "</td>";
        info = info + "<td class='sorting_1'>" + item.Num_OrdenServicio + "</td>";
        info = info + "<td class='sorting_1'>" + item.Id_CompAranda + "</td>";

        if (idSeleccionado == 0) {

            info = info + "<td class='sorting_1'>" + item.Det_Fch_RegDetalleIni + "</td>";
        }
        else {

            info = info + "<td class='sorting_1'>" + item.Det_Fch_RegDetalleIni + "</td>";
            info = info + "<td class='sorting_1'>" + item.Det_Fch_RegDetalleFin + "</td>";
            info = info + "<td class='sorting_1'>" + item.Det_Tiempo + "</td>";

            tiempoTareaDia = item.Det_Tiempo.split(':');
            //tiempoTotalDia = (parseInt(tiempoTareaDia[0]) * 60) + parseInt(tiempoTareaDia[1]);
            tiempoTotalDia = tiempoTotalDia + (parseInt(tiempoTareaDia[0]) * 60) + parseInt(tiempoTareaDia[1]);


        }

        info = info + "<td class='sorting_1'>" + item.Nom_Responsable + "</td>";
        info = info + "<td class='sorting_1'>" + item.Nom_Empresa + "</td>";
        info = info + "<td class='sorting_1'>" + item.Nom_SlaAranda + "</td>";
        info = info + "<td class='sorting_1'>" + item.Det_Tarea + "</td>";
        info = info + "<td class='sorting_1'>" + item.EstadoTarea + "</td>";
        info = info + "</tr>";
    });

    if (tiempoTotalDia == 0 && bandera == 0) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='10'>No informacion registrada</td>";
        info = info + "</tr>";
    }
    else if (tiempoTotalDia > 0) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='9' style='text-align: right;'><b>TOTAL HORAS:</b></td>";
        info = info + "<td class='sorting_1'><b>" + format_two_digits(Math.floor(tiempoTotalDia / 60)) + ':' + format_two_digits((tiempoTotalDia % 60)) + "</b></td>";
        info = info + "<td class='sorting_1' colspan='8'></td>";
        info = info + "</tr>";
    }
    else if (boton == 0) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='10' style='text-align: center;'>No informacion registrada</td>";
        info = info + "</tr>";
    }
    info = info + "</tbody>";
    info = info + "</table>";

    //tiempoEntregado = 0;

    //tiempoEntregado = tiempoTotalDia;

    //$('#txtHorasEntregadas1').val(tiempoTotalDia);

    return info;
}

function RecorreJSONTableSelect4(json, boton, idSeleccionado) {
    var info = ""; 
    var valor1 = 0;
    var valor2 = 0;
    var valor3 = 0;

    $.each(json, function (i, item) {
       
        valor1 = item.HORAS_CONTRATADAS;  
        valor2 = item.HORAS_ENTREGADAS; 
        valor3 = item.HORAS_DISPONIBLES; 
    });  

    $('#txtHorasGeneradas1').val(valor1);
    $('#txtHorasEntregadas1').val(valor2);
    $('#txtHorasDisponibles1').val(valor3);

}

function RecorreDatosPantalla(json)
{
    $.each(json, function (i, item) {

        $('#txtFechaDesde').val(item.FECHA);
        $('#txtPedido').val(item.PEDIDO);
        $('#txtCliente').val(item.CLIENTE);
        $('#txtReferencia').val(item.REFERENCIA_CLIENTE);

        var combo1 = document.getElementById("cboArea");
        combo1.value = item.AREA;

        $('#txtResponsable').val(item.GESTOR_RESPONSABLE);
        $('#txtOrden').val(item.ORDEN);
        $('#txtClasificacion').val(item.CLASIFICACION);
        $('#txtDescripcion').val(item.DESCRIPCION_DE_SERVICIO);
        $('#txtSla').val(item.SLA_CONTRATO);
        $('#txtHContratadas').val(item.HORAS_CONTRATADAS);
        $('#txtHEntregadas').val(item.HORAS_ENTREGADAS);
        $('#txtHDisponible').val(item.HORAS_DISPONIBLES);
        $('#txtCostoPlan').val(item.COSTO_PLAN);
        $('#txtCostoReal').val(item.COSTO_REAL);
        $('#txtSaldoCosto').val(item.SALDO_DE_COSTOS);

        var combo2 = document.getElementById("cboEstado");
        combo2.value = item.ESTATUS;

        $('#txtFechaECierre').val(item.FECHA_ESTIMADA_DE_CIERRE);
        $('#txtFechaCierre').val(item.FECHA_CIERRE);
        $('#txtMantenimiento').val(item.MANTENIMIENTO);
        $('#txtMantEntregado').val(item.MANT_ENTREGADOS);
        $('#txtObservacion').val(item.OBSERVACIONES);
        $('#txtContactoCliente').val(item.CONTACTO_CLIENTE);
        $('#txtSolucion').val(item.TiempoSolucion);
        //$('#txtSucursal').val(item.SUCURSAL);

        var combo3 = document.getElementById("cboSucursal");
        combo3.value = item.SUCURSAL;

        var combo4 = document.getElementById("cboGerente");
        combo4.value = item.IdGerenteCuenta;

        var combo5 = document.getElementById("cbotiempo");
        combo5.value = item.TiempoRespuesta;

        var combo6 = document.getElementById("cboSla");
        combo6.value = item.SLA_CONTRATO;

        var combo7 = document.getElementById("cboClasificacion");
        combo7.value = item.IdClasificacion;

        var combo8 = document.getElementById("cboResponsable");
        combo8.value = item.IdGestorResponsable;

        var combo9 = document.getElementById("cboCliente");
        combo9.value = item.IdCliente;
    });
}

function BtnConsulta() {
    $("#divMensajes").html("");

    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
        document.getElementById("idFiltros2").checked = true
    } else {
        fechaInicio = $("#txtFechaConsulta1").val();
        fechaFinal = $("#txtFechaConsulta2").val();
    }

    ObtenerListaContratos(0, 0, fechaInicio, fechaFinal);
}

function BtnConsultaOSM() {

    $("#divMensajes").html("");
    tipoGeneral = 0;
    idregistroGeneral = 0;

    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros4");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaConsulta7").val();
        fechaFinal = $("#txtFechaConsulta8").val();
    }

    ListaRequerimientos($("#txtNumeroOrdenOS4").val(), fechaInicio, fechaFinal,0);

}

function BtnDescargaOSM() {

    $("#divMensajes").html("");
    tipoGeneral = 0;
    idregistroGeneral = 0;

    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros4");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaConsulta7").val();
        fechaFinal = $("#txtFechaConsulta8").val();
    }

    ListaRequerimientosDescarga($("#txtNumeroOrdenOS4").val(), fechaInicio, fechaFinal, 0);

}


function BtnConsultaReporte() {
    $("#divMensajes").html("");
    var comboAnio = document.getElementById("cboAnioServicios");
    var selectedAnio = comboAnio.options[comboAnio.selectedIndex].text;
    if (selectedAnio == "--SELECCIONE--") {
        alerta("Seleccione un Anio");
    } else {
        //limpiar div
        document.getElementById("datosTablaPrincipal6").innerHTML = "";
        ObtenerListaReportes();
    }
}

function BtnConsultaOS() {
    $("#divMensajes").html("");
    tipoGeneral = 0;
    idregistroGeneral = 0;

    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros3");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaConsulta5").val();
        fechaFinal = $("#txtFechaConsulta6").val();
    }

    if (tiempoEntregado == 0) {
        ObtenerListaTareasHorasContratadas(5, 0, $("#txtNumeroOrdenOS").val(), os, fechaInicio, fechaFinal);
    }

    ObtenerListaTareasHorasOS(5, 0, $("#txtNumeroOrdenOS").val(), $("#txtNumeroOrdenOS").val(), fechaInicio, fechaFinal);


}

function BtnDescargaOS() {

    $("#divMensajes").html("");
    tipoGeneral = 0;
    idregistroGeneral = 0;

    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros3");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaConsulta5").val();
        fechaFinal = $("#txtFechaConsulta6").val();
    }

    ObtenerListaTareasHorasDescargarOS(5, 0, $("#txtNumeroOrdenOS").val(), os, fechaInicio, fechaFinal);
}

function VerListadoArchivosTarea(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
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

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function BorrarBotonesContrato() {
    $('#txtFechaDesde').val("");
    $('#txtPedido').val("");
    $('#txtCliente').val("");
    $('#txtReferencia').val("");

    var combo1 = document.getElementById("cboArea");
    combo1.selectedIndex = 0;

    var combo2 = document.getElementById("cboEstado");
    combo2.selectedIndex = 0;

    var combo3 = document.getElementById("cboResponsable");
    combo3.selectedIndex = 0;

    var combo4 = document.getElementById("cboClasificacion");
    combo4.selectedIndex = 0;

    var combo5 = document.getElementById("cboGerente");
    combo5.selectedIndex = 0;

    var combo6 = document.getElementById("cboCliente");
    combo6.selectedIndex = 0;

    var combo7 = document.getElementById("cboSucursal");
    combo7.selectedIndex = 0;

    var combo8 = document.getElementById("cboSla");
    combo8.selectedIndex = 0;

    var combo9 = document.getElementById("cbotiempo");
    combo9.selectedIndex = 0;

    $('#txtResponsable').val("");
    $('#txtOrden').val("");
    $('#txtClasificacion').val("");
    $('#txtDescripcion').val("");
    $('#txtSla').val("");
    $('#txtHContratadas').val("");
    $('#txtHEntregadas').val("");
    $('#txtHDisponible').val("");
    $('#txtCostoPlan').val("");
    $('#txtCostoReal').val("");
    $('#txtSaldoCosto').val("");


    $('#txtFechaECierre').val("");
    $('#txtFechaCierre').val("");
    $('#txtMantenimiento').val("");
    $('#txtMantEntregado').val("");
    $('#txtObservacion').val("");
    $('#txtContactoCliente').val("");
    $('#txtGerenteCuenta').val("");
    $('#txtSolucion').val("");


}

function GuardarNuevoContrato() {
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
        mensajeVerificacion += "- Debe ingresar el nombre del cliente ";
        contadorVerificacion += 1;
    }
    else {
        var comboCliente = document.getElementById("cboCliente");
        var selectedCliente = comboCliente.options[comboCliente.selectedIndex].text;
    }

    if ($('#txtReferencia').val() == "") {
        mensajeVerificacion += "- Debe ingresar la referencia ";
        contadorVerificacion += 1;
    }

    if ($('#cboArea').val() == "SELECCIONAR AREA") {
        mensajeVerificacion += "- Debe ingresar el Area ";
        contadorVerificacion += 1;
    }

    if ($('#cboResponsable').val() == "-- SELECCIONE --") {
        mensajeVerificacion += "- Debe ingresar el Responsable ";
        contadorVerificacion += 1;
    }
    else {
        var combooResponsable = document.getElementById("cboResponsable");
        var selectedoResponsable = combooResponsable.options[combooResponsable.selectedIndex].text;
    }

    if ($('#txtOrden').val() == "") {
        mensajeVerificacion += "- Debe ingresar el numero de Orden ";
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

    if ($('#txtDescripcion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la descripcion ";
        contadorVerificacion += 1;
    }

    if ($('#cboSla').val() == "SELECCIONAR SLA") {
        mensajeVerificacion += "- Debe ingresar el sla contrato ";
        contadorVerificacion += 1;
    }

    if ($('#cbotiempo').val() == "SELECCIONAR TIEMPO DE RESPUESTA") {
        mensajeVerificacion += "- Debe ingresar el tiempo de respuesta ";
        contadorVerificacion += 1;
    }

    if ($('#txtSolucion').val() == "") {
        mensajeVerificacion += "- Debe ingresar el tiempo de solucion ";
        contadorVerificacion += 1;
    }

    if ($('#txtHContratadas').val() == "") {
        mensajeVerificacion += "- Debe ingresar las horas de contrato ";
        contadorVerificacion += 1;
    }

    $('#txtHEntregadas').val("0");
    $('#txtHDisponible').val("0");
    $('#txtCostoReal').val("0");
    $('#txtSaldoCosto').val("0");
    

    //if ($('#txtHEntregadas').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar las horas de entregadas ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtHDisponible').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar las horas de disponible ";
    //    contadorVerificacion += 1;
    //}

    if ($('#txtCostoPlan').val() == "") {
        mensajeVerificacion += "- Debe ingresar el costo del plan ";
        contadorVerificacion += 1;
    }

    //if ($('#txtCostoReal').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el costo del Real ";
    //    contadorVerificacion += 1;
    //}

    //if ($('#txtSaldoCosto').val() == "") {
    //    mensajeVerificacion += "- Debe ingresar el costo del saldo ";
    //    contadorVerificacion += 1;
    //}

    if ($('#cboEstado').val() == "SELECCIONAR ESTADO") {
        mensajeVerificacion += "- Debe ingresar el estado ";
        contadorVerificacion += 1;
    }


    if ($('#txtFechaECierre').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha estimada de cierre ";
        contadorVerificacion += 1;
    }

    if ($('#txtFechaCierre').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha de cierre ";
        contadorVerificacion += 1;
    }

    if ($('#txtMantenimiento').val() == "") {
        mensajeVerificacion += "- Debe ingresar el mantenimiento ";
        contadorVerificacion += 1;
    }

    if ($('#txtMantEntregado').val() == "") {
        mensajeVerificacion += "- Debe ingresar el mantenimiento entregado ";
        contadorVerificacion += 1;
    }

    if ($('#txtObservacion').val() == "") {
        mensajeVerificacion += "- Debe ingresar la observacion ";
        contadorVerificacion += 1;
    }

    if ($('#txtContactoCliente').val() == "") {
        mensajeVerificacion += "- Debe ingresar el contacto de cliente ";
        contadorVerificacion += 1;
    }

    if ($('#cboGerente').val() == "-- SELECCIONE --") {
        mensajeVerificacion += "- Debe ingresar el gerente de cuenta ";
        contadorVerificacion += 1;
    }
    else {
        var combooGerente = document.getElementById("cboGerente");
        var selectedoGerente = combooGerente.options[combooGerente.selectedIndex].text;
    }

    if ($('#cboSucursal').val() == "SELECCIONAR SUCURSAL") {
        mensajeVerificacion += "- Debe ingresar la secursal ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        MensajeIncorrecto(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtPedido': '" + $('#txtPedido').val() + "',";
    datosFormulario = datosFormulario + "'txtCliente': '" + selectedCliente + "',";
    datosFormulario = datosFormulario + "'txtReferencia': '" + $('#txtReferencia').val() + "',";
    datosFormulario = datosFormulario + "'cboArea': '" + $('#cboArea').val() + "',";
    datosFormulario = datosFormulario + "'txtResponsable': '" + selectedoResponsable + "',";
    datosFormulario = datosFormulario + "'txtOrden': '" + $('#txtOrden').val() + "',";
    datosFormulario = datosFormulario + "'txtClasificacion': '" + selectedoClasificacion + "',";
    datosFormulario = datosFormulario + "'txtDescripcion': '" + $('#txtDescripcion').val() + "',";
    datosFormulario = datosFormulario + "'txtSla': '" + $('#cboSla').val() + "',";
    datosFormulario = datosFormulario + "'txtHContratadas': '" + $('#txtHContratadas').val() + "',";
    datosFormulario = datosFormulario + "'txtHEntregadas': '" + $('#txtHEntregadas').val() + "',";
    datosFormulario = datosFormulario + "'txtHDisponible': '" + $('#txtHDisponible').val() + "',";
    datosFormulario = datosFormulario + "'txtCostoPlan': '" + $('#txtCostoPlan').val() + "',";
    datosFormulario = datosFormulario + "'txtCostoReal': '" + $('#txtCostoReal').val() + "',";
    datosFormulario = datosFormulario + "'txtSaldoCosto': '" + $('#txtSaldoCosto').val() + "',";
    datosFormulario = datosFormulario + "'cboEstado': '" + $('#cboEstado').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaECierre': '" + $('#txtFechaECierre').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaCierre': '" + $('#txtFechaCierre').val() + "',";
    datosFormulario = datosFormulario + "'txtMantenimiento': '" + $('#txtMantenimiento').val() + "',";
    datosFormulario = datosFormulario + "'txtMantEntregado': '" + $('#txtMantEntregado').val() + "',";
    datosFormulario = datosFormulario + "'txtObservacion': '" + $('#txtObservacion').val() + "',";
    datosFormulario = datosFormulario + "'txtContactoCliente': '" + $('#txtContactoCliente').val() + "',";
    datosFormulario = datosFormulario + "'txtGerenteCuenta': '" + selectedoGerente + "',";
    datosFormulario = datosFormulario + "'txtSolucion': '" + $('#txtSolucion').val() + "',";
    datosFormulario = datosFormulario + "'cbotiempo': '" + $('#cbotiempo').val() + "',";
    datosFormulario = datosFormulario + "'txtSucursal': '" + $('#cboSucursal').val() + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoContrato', 'parameters' : " + datosFormulario + " }]";

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
                ObtenerListaContratos($("#txtFechaConsulta1").val(), $("#txtFechaConsulta2").val());
                MensajeCorrecto(respuesta.mensaje);
                BorrarBotonesContrato();
            }
            else if (respuesta.estado == "0")
            {
                MensajeIncorrecto(mesnajeError);
            }

            //MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            //MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
            MensajeIncorrecto(mesnajeError);
        }
    });

    return;
}

function VerContratos(idContrato)
{
    ObtenerConsultarContratos(idContrato, 0, 1)
    $('.nav-pills a[href="#home-pills"]').tab('show');
}

function CargarTareaContrato(orden)
{
    os = orden;
    bandera = 1;
    $("#txtFechaConsulta3").val($("#txtFechaConsulta1").val());
    $("#txtFechaConsulta4").val($("#txtFechaConsulta2").val());
    ObtenerListaTareasHorasExtras(0, 0,"", orden,"","");
}

function CargarTareaContratoPorOs(orden) {
    os = orden;
    bandera = 1;
    $("#txtFechaConsulta5").val($("#txtFechaConsulta1").val());
    $("#txtFechaConsulta6").val($("#txtFechaConsulta2").val());
    ObtenerListaTareasHorasOS(5, 0, orden, orden, "", "");
    document.getElementById("idFiltros3").checked = true
}

function VerDetalleTareas(tipo, idregistro,orden) {
    var res;
    tipoGeneral = tipo;
    idregistroGeneral = idregistro;
    ObtenerListaTareasHorasExtras(tipo, idregistro,"",orden,"","");
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


$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);


    ObtenerListaComboAnioServicios();
    ObtenerListaComboGestor();
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


    to = $("#txtFechaECierre").datepicker(
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

    tofrom2 = $("#txtFechaCierre").datepicker(
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

    tofrom3 = $("#txtFechaConsulta1").datepicker(
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

    tofrom4 = $("#txtFechaConsulta2").datepicker(
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

    tofrom5 = $("#txtFechaConsulta3").datepicker(
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

    tofrom6 = $("#txtFechaConsulta4").datepicker(
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

    tofrom7 = $("#txtFechaConsulta5").datepicker(
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

    tofrom8 = $("#txtFechaConsulta6").datepicker(
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

    tofrom9 = $("#txtFechaConsulta7").datepicker(
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


    tofrom10 = $("#txtFechaConsulta8").datepicker(
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
    $("#txtFechaConsulta3").datepicker('setDate', 'today');
    $("#txtFechaConsulta4").datepicker('setDate', 'today');
    $("#txtFechaConsulta5").datepicker('setDate', 'today');
    $("#txtFechaConsulta6").datepicker('setDate', 'today');
    $("#txtFechaConsulta7").datepicker('setDate', 'today');
    $("#txtFechaConsulta8").datepicker('setDate', 'today');
    $("#txtFechaDesde").datepicker('setDate', 'today');

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

    ObtenerListaComboClientes();
    ObtenerListaComboClasificacion();
    ObtenerListaComboGerenteCuentas();
    ObtenerListaComboGestorResponsable();

    if ($("#ContentPlaceHolder1_txtIdTipo").val() == "8") {
        ObtenerListaComboGerenteCuentasUnico();
       ObtenerListaComboGerenteCuentasUnico3();
        
    }
    else {
        ObtenerListaComboGerenteCuentas2();
        ObtenerListaComboGerenteCuentas3();

    }


    ObtenerListaComboGestorResponsable2();
    ObtenerListaComboClientes2();
    ObtenerListaComboClientes3();
    ObtenerListaComboClientes4();

    if ($("#ContentPlaceHolder1_txtIdCliente").val() == "0") {

        document.getElementById("ContentPlaceHolder1_idGerente").style.display = "block";
        document.getElementById("ContentPlaceHolder1_Detalle").style.display = "block";

        document.getElementById("txt1").style.display = "none";
        document.getElementById("txt2").style.display = "none";
        document.getElementById("txt3").style.display = "none";
        tiempoEntregado = 1;

    }
    else {

        document.getElementById("ContentPlaceHolder1_idGerente").style.display = "none";
        document.getElementById("ContentPlaceHolder1_Detalle").style.display = "none";

        document.getElementById("tab1").style.display = "none";
        document.getElementById("tab2").style.display = "none";
        document.getElementById("tab3").style.display = "none";
        document.getElementById("profile-pills").style.display = "none";

    }

    // ------------------------------------------------------------------

    $('#btnCargarArchivosAdjuntos').click(function () {

        if (idContrato == 0) {

            alerta("Para cargar un archivo debe seleccionar un contrato...");
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
                fileData.append('Id_RegTareas', idContrato);
                fileData.append('idServicio', "1");

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
                            ListadoArchivosContrato("#divArchivosAdjuntosAnteriores", idContrato);
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