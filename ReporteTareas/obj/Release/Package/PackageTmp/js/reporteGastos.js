/* Información de Nuestro Framework */

function ObtenerListaGastosCuentas() {
    var Datos = "[{ \"action\": \"ListaGastosCuentasContable\", \"parameters\" : {  \"sucursal\" : \"" + $("#cboSucursal").val() + "\",\"area\" : \"" + $("#cboArea").val() + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table", "", "", "#divListadoDetalleTareasDiarias");
}

function ObtenerListaGastosCuentasDescarga() {
    var Datos = "[{ \"action\": \"ListaGastosCuentasContableDescargaXLS\", \"parameters\" : {  \"sucursal\" : \"" + $("#cboSucursal").val() + "\",\"area\" : \"" + $("#cboArea").val() + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table");
}


function CargarPagina(div, url, datos, tipoControl, boton, divPanelShow, divPanelHide) {

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
                    if (typeof respuesta.estado == "undefined") {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, boton, divPanelShow, divPanelHide));
                    } else {
                        MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);
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


function RecorreJSON(div, json, tipoControl, boton, divPanelShow, divPanelHide) {

    var contenido = "";

    if (tipoControl == "table") {
        contenido = RecorreJSONTable(json);
        $(div).html(contenido);
        $(divPanelHide).hide();
    }
    if (tipoControl == "select") {
        contenido = RecorreJSONSelect(div, json);
    }
    if (tipoControl == "form") {
        contenido = RecorreJSONForm(div, json, boton);
    }
    if (tipoControl == "tableSelect") {
        contenido = RecorreJSONTableSelect(json);
        $(div).html(contenido);
    }
    if (tipoControl == "tableDetail") {
        contenido = RecorreJSONTableDetail(json);
        $(divPanelShow).show();
        $(div).html(contenido);
    }
    
    return contenido;
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
            var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;text-align:center;'>";
            var thFinal = "</th>";
            info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
            info = info + "<thead><tr role='row'>";
            info = info + thInicial + "CODIGO" + thFinal;
            info = info + thInicial + "DESCRIPCION" + thFinal;
            info = info + thInicial + "TOTAL" + thFinal;
            info = info + "</tr></thead>";

            info = info + "<tbody>";

            iniciaBarrido = false;
        }
        //
        // Despliegue de datos
        // 
        if (esImpar) {
            info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.ID + "'>";
            esImpar = false;
        } else {
            info = info + "<tr class='gradeA even' role = 'row' id='tr-" + item.ID + "'>";
            esImpar = true;
        }
        info = info + "<td class='sorting_1'>" + item.CODIGO + "</td>";
        info = info + "<td class='sorting_1' style='text-align: left;'>" + item.DESCRIPCION + "</td>";
        info = info + "<td class='sorting_1' style='text-align: right;'>" + "$ " + item.TOTAL + "</td>";
        info = info + "</tr>";
        totalValor3 = totalValor3 + parseFloat(item.TOTAL);
    });

    if (totalValor3 > 0) {
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='1' style='text-align: right;'><b></b></td>";
        info = info + "<td class='sorting_1' colspan='1' style='text-align: left;'>TOTAL: </td>";
        info = info + "<td class='sorting_1' style='text-align: right;'><b>" + "$ " + format_two_digits((totalValor3).toFixed(2)) + "</b></td>";
        info = info + "</tr>";
    }

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}


function RecorreJSONTableSelect(json) {
    var info = "";

    return info;
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

function RecorreJSONForm(div, item, boton) {
    var info = "";

    return info;
}

function BtnConsulta() {
    $("#divMensajes").html("");
    ObtenerListaGastosCuentas();
}

function BtnDescargar() {
    $("#divMensajes").html("");
    ObtenerListaGastosCuentasDescarga()
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

function VerListadoDetalleTareas(id, fecha, responsable) {
    $("#listDetailTitleLabel").html("Lista de Tareas de <b> " + responsable + "</b> del <b>" + fecha + "</b>");
    $("#txtCodigo").val("");
    var Datos = "[{ \"action\": \"ListaDetalleTareasPorUsuario\", \"parameters\" : { \"usuario\" : \"" + id + "\", fechaDesde: \"" + fecha + "\", fechaHasta: \"" + fecha + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", idtipogasto: \"" + "0" + "\", cliente: \"" + "" + "\", orden: \"" + "" + "\", AprobarTarea: \"" + "0" + "\", AprobarTareaQA: \"" + "0" + "\", TipoFecha: \"" + "1" + "\"} }]";
    CargarPagina('#datosTablaDetail', 'ObtenerListaTareas.ashx', Datos, "tableDetail", "", "#divListadoDetalleTareasDiarias");
    $("#txtCodigo").val(id);
 
    $("html, body").animate({ scrollTop: $(document).height() }, 1000);
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

$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    $("#divListadoDetalleTareasDiarias").hide();

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
    $("#txtFechaHasta").datepicker('setDate', 'today');


    formFrom = $("#frmTxtFecha").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"],
        });

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
