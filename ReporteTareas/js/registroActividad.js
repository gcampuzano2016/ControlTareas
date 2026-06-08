var IdProceso = 0;

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

function ObtenerUsuario() {
    var comboUsuario = document.getElementById("cmbUsuarios2");
    var selectedUsuario = comboUsuario.options[comboUsuario.selectedIndex].text;
}

var IdProceso = 0;

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

function ObtenerUsuario() {
    var comboUsuario = document.getElementById("cmbUsuarios2");
    var selectedUsuario = comboUsuario.options[comboUsuario.selectedIndex].text;
}

function ObtenerListaUsuarios() {
    var Datos = "[{ \"action\": \"ListaUsuarios\", \"parameters\" : \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\" }]";
    CargarPagina('#cmbUsuarios2', 'ObtenerListaTareas.ashx', Datos, "select", "", "", "");
}

function ObtenerListaRecursosHorasDiarias() {
    var Datos = "[{ \"action\": \"ListaRecursosHorasDiariasAsistencia\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios2").val() + "\", fechaDesde: \"" + $("#txtFechaConsulta1").val() + "\", fechaHasta: \"" + $("#txtFechaConsulta2").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", estado: \"" + "0" + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table", "", "", "#divListadoDetalleTareasDiarias");
}

function ObtenerConsultarForeCastDetalleFiltrosDescargar(idForeCast, tipo, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ObtenerRecursosHorasDiariasAsistenciaDescargar\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios2").val() + "\", fechaDesde: \"" + $("#txtFechaConsulta1").val() + "\", fechaHasta: \"" + $("#txtFechaConsulta2").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", estado: \"" + "0" + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table");
}

function BtnDescargar() {
    IdForeCast = 0;
    var fechaInicio = "";
    var fechaFinal = "";

    ObtenerConsultarForeCastDetalleFiltrosDescargar(IdForeCast, 0, fechaInicio, fechaFinal);
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

function RecorreJSON(div, json, tipoControl, boton, idSeleccionado) {

    var contenido = "";

    if (tipoControl == "select") {
        contenido = RecorreJSONSelect(div, json, idSeleccionado);
    }

    if (tipoControl == "table") {
        contenido = RecorreJSONTable(json);
        $(div).html(contenido);
    }

    return contenido;
}

function BtnAgregarAsistencia() {

    document.getElementById("ListaAsistencia").style.display = "none";
    document.getElementById("AgregarAsistencia").style.display = "block";
}

function RegresarLista() {

    document.getElementById("ListaAsistencia").style.display = "block";
    document.getElementById("AgregarAsistencia").style.display = "none";

}

function confirmarAntesDeGuardar(onConfirm) {
    swal({
        title: "¿Guardar registro?",
        text: "Esta seguro de guardar la asistencia.",
        type: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, guardar",
        cancelButtonText: "Cancelar",
        closeOnConfirm: true,   // cierra el modal al confirmar
        closeOnCancel: true
    }, function (isConfirm) {
        if (isConfirm && typeof onConfirm === "function") {
            onConfirm(); // <-- aquí llamas a tu método
        }
    });
}

$(document).on('click', '#btnNuevo', function (e) {
    e.preventDefault();
    if ($('#cmbUsuarios2').val() == "0") {
        alerta("Seleccione el usuario..");
    }
    else {
        if (IdProceso != 0) {
            confirmarAntesDeGuardar(function () { GuardarAsistencia(IdProceso); });
        }
        else {
            confirmarAntesDeGuardar(function () { GuardarAsistencia(0); });
        }
    }
});

function GuardarAsistencia(IdProceso) {

    var FechaEntrada = $('#txtFechaEntrada').val();
    var FechaSalida = $('#txtFechaSalida').val();
    var FechaSalidaAct = $('#txtFechaSalidaAct').val();
    var Observacion = $('#txtObservacion').val();
    var Estado = $('#cboTipoFecha').val();
    var Usuario = $("#ContentPlaceHolder1_txtUsuario").val();
    var tipo = 1;
    if (IdProceso !=0) {
        tipo = 1;
    }

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'Usuario': '" + Usuario + "',";
    datosFormulario = datosFormulario + "'FechaEntrada': '" + FechaEntrada + "',";
    datosFormulario = datosFormulario + "'FechaSalida': '" + FechaSalida + "',";
    datosFormulario = datosFormulario + "'FechaSalidaAct': '" + FechaSalidaAct + "',";
    datosFormulario = datosFormulario + "'Observacion': '" + Observacion + "',";
    datosFormulario = datosFormulario + "'Estado': '" + Estado + "',";
    datosFormulario = datosFormulario + "'Tipo': '" + Estado + "',";
    datosFormulario = datosFormulario + "'IdProceso': '" + IdProceso + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'RegistrarEvento3', 'parameters' : " + datosFormulario + " }]";

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
                $('#txtObservacion').val("");
                RegresarLista();
                $("#divMensajes").html("");
                ObtenerListaRecursosHorasDiarias();
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

function BtnConsulta() {
    $("#divMensajes").html("");
    ObtenerListaRecursosHorasDiarias();
}


function EditarAsistencia(ID, FechaEntrada, FechaSalida) {
    IdProceso = ID;
    $('#txtFechaEntrada').val(FechaEntrada);
    $('#txtFechaSalida').val(FechaSalida);
    document.getElementById("ListaAsistencia").style.display = "none";
    document.getElementById("AgregarAsistencia").style.display = "block";

}

$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    var dateFormat = "dd/mm/yy";

    from = $("#txtFechaConsulta1").datepicker(
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

        to = $("#txtFechaConsulta2").datepicker(
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

    tofrom11 = $("#txtFechaEntrada").datepicker(
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

    tofrom12 = $("#txtFechaSalida").datepicker(
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

    tofrom13 = $("#txtFechaSalidaAct").datepicker(
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

    const toLocalDatetime = d => new Date(d.getTime() - d.getTimezoneOffset() * 60000)
        .toISOString().slice(0, 16);

    const now = new Date();
    $("#txtFechaEntrada").val(toLocalDatetime(now));
    $("#txtFechaSalida").val(toLocalDatetime(now));
    $("#txtFechaSalidaAct").val(toLocalDatetime(now));


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


    ObtenerListaUsuarios();
});

function ObtenerListaRecursosHorasDiarias() {
    var Datos = "[{ \"action\": \"ListaRecursosHorasDiariasActividad\", \"parameters\" : { \"usuario\" : \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", fechaDesde: \"" + $("#txtFechaConsulta1").val() + "\", fechaHasta: \"" + $("#txtFechaConsulta2").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", estado: \"" + "0" + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table", "", "", "#divListadoDetalleTareasDiarias");
}

function ObtenerConsultarForeCastDetalleFiltrosDescargar(idForeCast, tipo, fechaInicio, fechaFinal) {
    var Datos = "[{ \"action\": \"ObtenerRecursosHorasDiariasAsistenciaDescargar\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios2").val() + "\", fechaDesde: \"" + $("#txtFechaConsulta1").val() + "\", fechaHasta: \"" + $("#txtFechaConsulta2").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", estado: \"" + "0" + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table");
}

function BtnDescargar() {
    IdForeCast = 0;
    var fechaInicio = "";
    var fechaFinal = "";

    ObtenerConsultarForeCastDetalleFiltrosDescargar(IdForeCast, 0, fechaInicio, fechaFinal);
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

function RecorreJSON(div, json, tipoControl, boton, idSeleccionado) {

    var contenido = "";

    if (tipoControl == "select") {
        contenido = RecorreJSONSelect(div, json, idSeleccionado);
    }

    if (tipoControl == "table") {
        contenido = RecorreJSONTable(json);
        $(div).html(contenido);
    }

    return contenido;
}

function BtnAgregarAsistencia() {

    document.getElementById("ListaAsistencia").style.display = "none";
    document.getElementById("AgregarAsistencia").style.display = "block";
}

function RegresarLista() {

    document.getElementById("ListaAsistencia").style.display = "block";
    document.getElementById("AgregarAsistencia").style.display = "none";

}

function confirmarAntesDeGuardar(onConfirm) {
    swal({
        title: "¿Guardar registro?",
        text: "Esta seguro de guardar la actividad.",
        type: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, guardar",
        cancelButtonText: "Cancelar",
        closeOnConfirm: true,   // cierra el modal al confirmar
        closeOnCancel: true
    }, function (isConfirm) {
        if (isConfirm && typeof onConfirm === "function") {
            onConfirm(); // <-- aquí llamas a tu método
        }
    });
}

$(document).on('click', '#btnNuevo', function (e) {
    e.preventDefault();
    if ($('#cmbUsuarios2').val() == "0") {
        alerta("Seleccione el usuario..");
    }
    else {
        if (IdProceso != 0) {
            confirmarAntesDeGuardar(function () { GuardarAsistencia(IdProceso); });
        }
        else {
            confirmarAntesDeGuardar(function () { GuardarAsistencia(0); });
        }
    }
});

function BtnConsulta() {
    $("#divMensajes").html("");
    ObtenerListaRecursosHorasDiarias();
}

function RecorreJSONTable(json) {
    var info = "";
    document.getElementById("ListaAsistencia").style.display = "block";
    document.getElementById("AgregarAsistencia").style.display = "none";
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
            info = info + thInicial + "FECHA DE ENTRADA" + thFinal;
            info = info + thInicial + "FECHA DE SALIDA" + thFinal;
            info = info + thInicial + "FECHA LIMITE" + thFinal;
            info = info + thInicial + "ESTADO" + thFinal;
            info = info + "</tr></thead>";

            info = info + "<tbody>";

            iniciaBarrido = false;
        }
        //
        // Despliegue de datos
        // 
        if (esImpar) {
            info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.IdConfiguracion + "'>";
            esImpar = false;
        } else {
            info = info + "<tr class='gradeA even' role = 'row' id='tr-" + item.IdConfiguracion + "'>";
            esImpar = true;
        }
        info = info + "<td class='sorting_1' style='text-align:center'>";
        info = info + "<i class='fa fa-eye' style='cursor: pointer' onclick='EditarAsistencia(\"" + item.IdConfiguracion + "\",\"" + item.FechaInicio + "\",\"" + item.FechaFinal + "\",\"" + item.FechaLimite + "\");'></i>";
        info = info + "</td > ";
        info = info + "<td class='sorting_1' style='text-align: center;'>" + item.FechaInicio + "</td>";
        info = info + "<td class='sorting_1' style='text-align: center;'>" + item.FechaFinal + "</td>";
        info = info + "<td class='sorting_1' style='text-align: center;'>" + item.FechaLimite + "</td>";
        info = info + "<td class='sorting_1' style='text-align: center;'>" + item.Estado + "</td>";
        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function EditarAsistencia(ID, FechaEntrada, FechaSalida, FechaLimite) {
    IdProceso = ID;
    $('#txtFechaEntrada').val(FechaEntrada);
    $('#txtFechaSalida').val(FechaSalida);
    $('#txtFechaSalidaAct').val(FechaLimite);
    document.getElementById("ListaAsistencia").style.display = "none";
    document.getElementById("AgregarAsistencia").style.display = "block";

}

$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    var dateFormat = "dd/mm/yy";

    from = $("#txtFechaConsulta1").datepicker(
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

        to = $("#txtFechaConsulta2").datepicker(
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

    tofrom11 = $("#txtFechaEntrada").datepicker(
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

    tofrom12 = $("#txtFechaSalida").datepicker(
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

    const toLocalDatetime = d => new Date(d.getTime() - d.getTimezoneOffset() * 60000)
        .toISOString().slice(0, 16);

    const now = new Date();
    $("#txtFechaEntrada").val(toLocalDatetime(now));
    $("#txtFechaSalida").val(toLocalDatetime(now));


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