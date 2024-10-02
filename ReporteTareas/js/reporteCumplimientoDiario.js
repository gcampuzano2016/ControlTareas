/* Información de Nuestro Framework */


function ObtenerListaUsuarios() {
    var Datos = "[{ \"action\": \"ListaUsuarios\", \"parameters\" : \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\" }]";
    CargarPagina('#cmbUsuarios', 'ObtenerListaTareas.ashx', Datos, "select", "", "", "");
}


function SeleccionarListaRecursosHorasDiarias() {
    var Datos = "[{ \"action\": \"ListaRecursosHorasDiarias\", \"parameters\" : \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\" }]";
    CargarPagina('#datosTablaTareasSeleccion', 'ObtenerListaTareas.ashx', Datos, "tableSelect", "", "", "");
}


function ObtenerListaRecursosHorasDiarias() {
    var Datos = "[{ \"action\": \"ListaRecursosHorasDiarias\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios").val() + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", estado: \"" + "0" + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table", "", "", "#divListadoDetalleTareasDiarias");
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
    $.each(json, function (i, item) {

        if (iniciaBarrido) {
            //
            // Despliegue de titulos de cabecera
            //
            var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;text-align:center;'>";
            var thFinal = "</th>";
            info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
            info = info + "<thead><tr role='row'>";
            info = info + thInicial;
            info = info + "ACCIONES";
            info = info + thFinal;
            info = info + thInicial + "ID" + thFinal;
            info = info + thInicial + "NOMBRE RESPONSABLE" + thFinal;
            info = info + thInicial + "FECHA DE TAREAS" + thFinal;
            info = info + thInicial + "DIA" + thFinal;
            info = info + thInicial + "TOTAL HORAS DIARIAS" + thFinal;
            info = info + thInicial + "CUMPLE JORNADA" + thFinal;
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

        info = info + "<td class='sorting_1' style='text-align:center'>";
        info = info + "<i class='fa fa-eye' style='cursor: pointer' onclick='VerListadoDetalleTareas(\"" + item.Id + "\",\"" + item.FechaTareas + "\",\"" + item.NombreResponsable + "\");'></i>";
        if (item.Seleccionar == "1") {
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            info = info + "<i class='fa fa-check-square' style='cursor: pointer' onclick='VerAprobarTareas(\"" + item.NombreResponsable + "\");'></i>";
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            info = info + "<i class='fa fa-undo' style='cursor: pointer' onclick='VerAnularAprobacionTareas(\"" + item.NombreResponsable + "\");'></i>";
        }
        info = info + "</td > ";
        info = info + "<td class='sorting_1' style='text-align: center;'>" + item.Id + "</td>";
        info = info + "<td class='sorting_1'>" + item.NombreResponsable + "</td>";
        info = info + "<td class='sorting_1' style='text-align: center;'>" + item.FechaTareas + "</td>";
        info = info + "<td class='sorting_1' style='text-align: center;'>" + item.Dia + "</td>";
        info = info + "<td class='sorting_1' style='text-align: center;'>" + item.TotalHorasDiarias + "</td>";
        info = info + "<td class='sorting_1' style='background: " + item.BgColorJornada + ";text-align: center;'>" + item.CumpleJornada + "</td>";
        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function RecorreJSONTableDetail(json) {
    var info = "";

    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;text-align:center;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "Aprobación Mensual" + thFinal;
    info = info + thInicial + "Cliente" + thFinal;
    info = info + thInicial + "N° de orden de servicio" + thFinal;
    info = info + thInicial + "Tipo de actividad" + thFinal;
    info = info + thInicial + "Tarea Principal" + thFinal;
    info = info + thInicial + "Descripción Tarea" + thFinal;
    info = info + thInicial + "Fecha actividad (dd / mm / aaaa)" + thFinal;
    info = info + thInicial + "Inicio actividad (hh:mm)" + thFinal;
    info = info + thInicial + "Fin actividad (hh:mm)" + thFinal;
    info = info + thInicial + "Tiempo total de la actividad (hh: mm)" + thFinal;
    info = info + thInicial + "Observaciones" + thFinal;
    info = info + thInicial + "¿Hora extra?" + thFinal;
    info = info + thInicial + "Cod Sap" + thFinal;
    info = info + thInicial + "Orden Operacion" + thFinal;
    info = info + thInicial + "Usuario Responsable" + thFinal;
    info = info + thInicial + "Fecha Solicitud de Horas Extras" + thFinal;
    info = info + thInicial + "Fecha Aprobación o Rechazo de Horas Extras" + thFinal;
    info = info + thInicial + "Estado Solicitud Horas Extras" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";

    var tiempoTotal = 0;
    var tiempoTarea = 0;
 

    var esImpar = true;
    $.each(json, function (i, item) {
        //
        // Despliegue de datos
        // 
        if (esImpar) {
            if (item.Motivo == " " || item.Motivo == "") {
                info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.idDetalleTarea + "' style = 'background: #d9edf7'>";
            } else {
                info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.idDetalleTarea + "'>";
            }
            esImpar = false;
        } else {
            if (item.Motivo == " " || item.Motivo == "") {
                info = info + "<tr class='gradeA even' role = 'row' id='tr-" + item.idDetalleTarea + "' style = 'background: #d9edf7'>";
            } else {
                info = info + "<tr class='gradeA even' role = 'row' id='tr-" + item.idDetalleTarea + "'>";
            }
            esImpar = true;
        }

        info = info + "<td class='sorting_1'>";
        if (item.Det_Aprobacion_Tarea_Estado == "1") {
            info = info + "<div class='alert alert-info'>Pendiente</div>";
        } else if (item.Det_Aprobacion_Tarea_Estado == "2") {
            info = info + "<div class='alert alert-success'>Aprobado</div>";
        } else {
            info = info + "";
        }
        info = info + "</td>";
        info = info + "<td class='sorting_1'>" + item.Nom_Cliente + "</td>";
        info = info + "<td class='sorting_1'>" + item.Num_OrdenServicio + "</td>";
        info = info + "<td class='sorting_1'>" + item.Actividad + "</td>";
        info = info + "<td class='sorting_1'>" + item.Det_Tarea + "</td>";
        info = info + "<td class='sorting_1'>" + item.Det_Det_Tarea + "</td>";
        info = info + "<td class='sorting_1'>" + item.Det_Fecha + "</td>";
        info = info + "<td class='sorting_1'>" + item.Det_Fch_RegDetalleIni + "</td>";
        if (item.Motivo == " " || item.Motivo == "") {
            info = info + "<td class='sorting_1' style='background: #d9edf7'>En Progreso</td>";
            info = info + "<td class='sorting_1'></td>";
        } else {
            info = info + "<td class='sorting_1'>" + item.Det_Fch_RegDetalleFin + "</td>";
            info = info + "<td class='sorting_1'>" + item.Det_Tiempo + "</td>";
        }

        info = info + "<td class='sorting_1'>" + item.Det_Observaciones + "</td>";
        info = info + "<td class='sorting_1'>" + item.Det_Horas_Extras_Descripcion + "</td>";
        info = info + "<td class='sorting_1'>" + item.Cod_Sap + "</td>";
        info = info + "<td class='sorting_1'>" + item.OrdenOperacion + "</td>";
        info = info + "<td class='sorting_1'>" + item.UsuarioResponsable + "</td>";
        info = info + "<td class='sorting_1'>" + item.Det_Horas_Extras_Fecha_Solicitud + "</td>";
        info = info + "<td class='sorting_1'>" + item.Det_Horas_Extras_Fecha_Aprobacion + "</td>";
        if (item.Det_Horas_Extras_Estado == '1') {
            info = info + "<td class='sorting_1'><div class='alert alert-info'>" + item.Det_Horas_Extras_Estado_Descripcion + "</div></td>";
        } else if (item.Det_Horas_Extras_Estado == '2') {
            info = info + "<td class='sorting_1'><div class='alert alert-success'>" + item.Det_Horas_Extras_Estado_Descripcion + "</div></td>";
        } else if (item.Det_Horas_Extras_Estado == '3') {
            info = info + "<td class='sorting_1'><div class='alert alert-danger'>" + item.Det_Horas_Extras_Estado_Descripcion + "</div></td>";
        } else {
            info = info + "<td class='sorting_1'>" + item.Det_Horas_Extras_Estado_Descripcion + "</td>";
        }

        tiempoTarea = item.Det_Tiempo.split(':');
        tiempoTotal = tiempoTotal + (parseInt(tiempoTarea[0]) * 60) + parseInt(tiempoTarea[1]);


        info = info + "</tr>";


    });

    info = info + "<tr>";
    info = info + "<td class='sorting_1' colspan='9' style='text-align: right;'>TOTAL HORAS:</td>";
    info = info + "<td class='sorting_1'>" + format_two_digits(Math.floor(tiempoTotal / 60)) + ':' + format_two_digits((tiempoTotal % 60)) + "</td>";
    info = info + "<td class='sorting_1' colspan='8'></td>";
    info = info + "</tr>";


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
    ObtenerListaRecursosHorasDiarias();
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

    ObtenerListaUsuarios();

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
