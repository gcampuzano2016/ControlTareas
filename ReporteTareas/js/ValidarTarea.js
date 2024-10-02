var tipoGeneral = 0;
var idregistroGeneral = 0;


function ObtenerListaTareasHorasExtras(tipo, idRegistro) {
    var Datos = "[{ \"action\": \"ListaConsultaTareasGeneradas\", \"parameters\" : { registro : \"" + idRegistro + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", estado: \"" + tipo + "\", busqueda: \"" + $("#txtNumeroOrden").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "tableSelect", tipo);
}

function ObtenerListaHorasExtrasDescarga(tipo, idRegistro) {
    var Datos = "[{ \"action\": \"ListaConsultaTareasDescargaXLS\", \"parameters\" : { registro : \"" + idRegistro + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", estado: \"" + tipo + "\", busqueda: \"" + $("#txtNumeroOrden").val() + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table");
}

function BtnDescarga() {
    $("#divMensajes").html("");
    ObtenerListaHorasExtrasDescarga(tipoGeneral, idregistroGeneral);
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
                    if (typeof respuesta.estado == "undefined") {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, boton, idSeleccionado));
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
    if (tipoControl == "tableSelectChange") {
        contenido = RecorreJSONTableSelectChange(json);
        $(div).html(contenido);
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

function RecorreJSONTableSelect(json, idSeleccionado) {
    var info = "";

    $("#listModalTitleLabel").html("Seleccionar Tarea Aranda");

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
    else
    {
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

            info = info + "<i class='fa fa-check-square' style='cursor: pointer' onclick='VerDetalleTareas(\"" + 1 + "\",\"" + item.Id_RegTareas + "\");'></i>";
        }
        else {
            info = info + "<i class='fa fa-check-square' style='cursor: pointer' onclick='VerDetalleTareas(\"" + 0 + "\",\"" + item.Id_RegTareas + "\");'></i>";
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

        }

        info = info + "<td class='sorting_1'>" + item.Nom_Responsable + "</td>";
        info = info + "<td class='sorting_1'>" + item.Nom_Empresa + "</td>";
        info = info + "<td class='sorting_1'>" + item.Nom_SlaAranda + "</td>";
        info = info + "<td class='sorting_1'>" + item.Det_Tarea + "</td>";
        info = info + "<td class='sorting_1'>" + item.EstadoTarea + "</td>";
        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function VerDetalleTareas(tipo,idregistro ) {
    var res;
    tipoGeneral = tipo;
    idregistroGeneral = idregistro;
    ObtenerListaTareasHorasExtras(tipo, idregistro);
    return;
}

function BtnConsulta() {
    $("#divMensajes").html("");
    tipoGeneral = 0;
    idregistroGeneral = 0;
    ObtenerListaTareasHorasExtras(0,0);
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

    ObtenerListaTareasHorasExtras(0, 0);

});