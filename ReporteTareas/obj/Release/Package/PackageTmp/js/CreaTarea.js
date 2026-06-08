var idCliente = 0;
var idEspecialista = 0;

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

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function ObtenerListaClientes(tipo, descripcion) {
    var Datos = "[{ \"action\": \"BuscarListaCliente\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelect", tipo);
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
                        alerta("No existen datos para esta consulta.");
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
    if (tipoControl == "tableSelect2") {
        contenido = RecorreJSONTableSelect2(json, idSeleccionado);
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

function BuscarCliente() {

    if ($('#txtCliente').val().length > 4) {
        ObtenerListaClientes(1, $('#txtCliente').val());
    }
    else {
        document.getElementById("comboClientes").style.display = "none";
    }
}

function BuscarEspecialista() {

    if ($('#txtecnico').val().length > 4) {
        ObtenerListaClientes(0, $('#txtecnico').val());
    }
    else {
        document.getElementById("comboEspecialista").style.display = "none";
    }
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

}

function CargarCliente(ID, NOMBRE) {
    $('#txtCliente').val(NOMBRE);
    idCliente = ID;
    document.getElementById("comboClientes").style.display = "none";
}

function CargarEspecialista(ID, NOMBRE) {
    $('#txtecnico').val(NOMBRE);
    idEspecialista = ID;
    document.getElementById("comboEspecialista").style.display = "none";
}

function BorrarBotonesTareas() {
    $('#txt_Os').val("");
    $('#txtCliente').val("");
    $('#txtecnico').val("");
    $('#txt_DetCamEstado').val("");
    var combo4 = document.getElementById("cboCategoria");
    combo4.selectedIndex = 0;
}

function GuardarTicket() {
    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;


    if ($('#txt_Os').val() == "") {
        mensajeVerificacion += "- Debe ingresar el numero de OS ";
        contadorVerificacion += 1;
    }

    if ($('#txtCliente').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre del Cliente ";
        contadorVerificacion += 1;
    }

    if ($('#txtFechaDesde').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha de registro ";
        contadorVerificacion += 1;
    }

    if ($('#txtecnico').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre del Especialista ";
        contadorVerificacion += 1;
    }

    if ($('#cboCategoria').val() == "--SELECCIONE--") {
        mensajeVerificacion += "- Debe ingresar la categoria ";
        contadorVerificacion += 1;
    }
    else {
        var comboCategoria = document.getElementById("cboCategoria");
        var selectedCategoria = comboCategoria.options[comboCategoria.selectedIndex].text;
    }

    if ($('#txt_DetCamEstado').val() == "") {
        mensajeVerificacion += "- Debe ingresar el detalle ticket ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        MensajeIncorrecto(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txt_Os': '" + $('#txt_Os').val() + "',";
    datosFormulario = datosFormulario + "'txtCliente': '" + $('#txtCliente').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtecnico': '" + $('#txtecnico').val() + "',";
    datosFormulario = datosFormulario + "'idEspecialista': '" + idEspecialista + "',";
    datosFormulario = datosFormulario + "'cboCategoria': '" + $('#cboCategoria').val() + "',";
    datosFormulario = datosFormulario + "'txt_DetCamEstado': '" + $('#txt_DetCamEstado').val() + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoTicket', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotonesTareas();
            }
            else if (respuesta.estado == "0") {
                MensajeIncorrecto(mesnajeError);
            }
        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MensajeIncorrecto(mesnajeError);
        }
    });

    return;
}

$(function () {

    document.getElementById("comboClientes").style.display = "none";
    document.getElementById("comboEspecialista").style.display = "none";

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
        to = $("#txtFechaHasta").datepicker(
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

    $("#txtFechaDesde").datepicker('setDate', 'today');


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

});