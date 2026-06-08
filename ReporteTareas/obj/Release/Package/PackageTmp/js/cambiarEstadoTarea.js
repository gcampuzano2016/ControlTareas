/* Información de Nuestro Framework */

function ObtenerListaTareasPorUsuario() {
    var idResponsable = "";
    var Datos = "[{ \"action\": \"ListaTareasPorUsuario\", \"parameters\" : { \"codigoResponsable\" : \"" + idResponsable + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table", "");
}

function ObtenerListaComboEstados(idTarea,idProyecto) {
    var Datos = "[{ \"action\": \"ListaComboEstados\", \"parameters\" : { \"idTarea\" : \"" + idTarea + "\", idProyeto: \"" + idProyecto + "\"} }]";
    CargarPagina('#cmbEstadoSeleccionado', 'ObtenerListaTareas.ashx', Datos, "select", "");
}

function ObtenerDetalleTarea(idDetalleTarea) {
    $('#frmTxtCodigo').val(idDetalleTarea);
    var Datos = "[{ \"action\": \"DetalleTareaPrincipal\", \"parameters\" : \"" + idDetalleTarea + "\" }]";
    CargarPagina('#divFormDetalleTarea', 'ObtenerListaTareas.ashx', Datos, "form", "#btnGuardarCambioEstado");
}

function ObtenerDetalleTareaBorrar(idDetalleTarea) {
    $('#frmTxtCodigo').val(idDetalleTarea);
    var Datos = "[{ \"action\": \"DetalleTarea\", \"parameters\" : \"" + idDetalleTarea + "\" }]";
    CargarPagina('#divFormDetalleTarea', 'ObtenerListaTareas.ashx', Datos, "form", "#btnBorrarTarea");
}

function CargarPagina(div, url, datos, tipoControl, boton) {

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
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, boton));
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

function RecorreJSON(div, json, tipoControl, boton) {

    var contenido = "";

    if (tipoControl == "table") {
        contenido = RecorreJSONTable(json);
        $(div).html(contenido);
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

    return contenido;
}

function RecorreJSONTable(json) {
    var info = "";

    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "Acciones" + thFinal;
    info = info + thInicial + "Codigo Tarea" + thFinal;
    info = info + thInicial + "Cliente" + thFinal;
    info = info + thInicial + "N° de orden de servicio" + thFinal;
    info = info + thInicial + "Cod. Aranda" + thFinal;
    info = info + thInicial + "Tipo de actividad" + thFinal;
    info = info + thInicial + "Estado" + thFinal;
    info = info + thInicial + "Descripción Tarea" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";

    var esImpar = true;
    $.each(json, function (i, item) {
        //
        // Despliegue de datos
        // 
        if (esImpar) {
            if (item.TareaEnEjecucion == "1") {
                info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.Id_RegTareas + "' style = 'background: #d9edf7'>";
            } else {
                info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.Id_RegTareas + "'>";
            }
            esImpar = false;
        } else {
            if (item.TareaEnEjecucion == "1") {
                info = info + "<tr class='gradeA even' role = 'row' id='tr-" + item.Id_RegTareas + "' style = 'background: #d9edf7'>";
            } else {
                info = info + "<tr class='gradeA even' role = 'row' id='tr-" + item.Id_RegTareas + "'>";
            }
            esImpar = true;
        }
        info = info + "<td class='sorting_1' style='text-align:center'>";
        if (item.conteoArchivosAdjuntos != '0') {
            info = info + "<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosTarea(\"#MensajeInformativo\", \"" + item.Id_RegTareas + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        }
        info = info + "<i class='fa fa-hand-o-right' style='cursor: pointer;' onclick='ObtenerDetalleTarea(\"" + item.Id_RegTareas + "\", \"" + item.Id_CompAranda + "\", \"" + "#btnGuardarNuevaTarea" + "\");'></i>";
        info = info + "</td>";
        info = info + "<td class='sorting_1'>" + item.Id_RegTareas + "</td>";
        info = info + "<td class='sorting_1'>" + item.Nom_Empresa + "</td>";
        info = info + "<td class='sorting_1'>" + item.Num_OrdenServicio + "</td>";
        info = info + "<td class='sorting_1'>" + item.Id_CompAranda + "</td>";
        info = info + "<td class='sorting_1'>" + item.CatalogoTareaSap + "</td>";
        if (item.TareaEnEjecucion == "1") {
            info = info + "<td class='sorting_1' style='background: #d9edf7'>" + item.EstadoTarea + "</td>";
        } else {
            info = info + "<td class='sorting_1'>" + item.EstadoTarea + "</td>";
        }

        info = info + "<td class='sorting_1' id='td-descripcionTarea-" + item.Id_RegTareas + "' width='50px'>" + item.Det_Tarea + "</td>";
        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

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

    // Limpiar formulario
    $("#divArchivosAdjuntosAnteriores").html("");
    $("#divArchivosAdjuntos").html("");
    $("#archivosAdjuntos").val("");

    $("#fomTitleLabel").html("Cambio de Tarea");

    // Verificación que la tarea no este "En Progreso"
    if (item.Det_Motivo_Cambio_Estado == "") {
        var txtMensaje = 'La tarea se encuentra <b>"En Progreso"</b>, para modificar debe cambiar de actividad.';
        $("#MensajeInformativo").html(txtMensaje);
        $('#modalMensajeInformativo').modal('show');
        return;
    }

    // Ocultar grid y visualizar formulario
    $("#ContentPlaceHolder1_Panel5").hide();
    $("#formActualizaTarea").show();

    //
    // Llenado de formulario con datos del registro seleccionado
    //
    var dateTarea = moment(item.Det_Fch_RegDetalleIni, "DD-MM-YYYY hh:mm:ss").format("DD/MM/YYYY");
    var timeTareaInicio = moment(item.Det_Fch_RegDetalleIni, "DD-MM-YYYY HH:mm:ss").format("HH:mm");
    var timeTareaFin = moment(item.Det_Fch_RegDetalleFin, "DD-MM-YYYY HH:mm:ss").format("HH:mm");

    $("#frmTxtTareaPrincipalDescripcion").val(item.Det_Tarea); // TODO
    $("#divFrmTxtCodigoTarea").hide();
    $("#frmTxtCodigoAranda").val(item.Id_CompAranda);
    $("#frmTxtEstadoActual").val(item.EstadoTarea);
    ObtenerListaComboEstados(item.IdEstado, item.IdProyecto);
    $("#frmTxtObservaciones").val(item.Det_Observaciones);
    $("#frmTxtTareaDetalle").val(item.Det_Det_Tarea);
    $("#frmcmbHorasExtras").val(item.Det_Horas_Extras_Tipo);

    ListadoArchivosTarea("#divArchivosAdjuntosAnteriores", item.Id_RegTareas);

    $("#divFrmTxtTareaPrincipalDescripcion").show();
    $("#divFrmTxtCodigo").show();
    $("#divFrmTxtCodigoAranda").show();
    $("#btnGuardarCambioEstado").hide();
    $(boton).show();

    $("#divMensajes").html("");


    return info;
}

function ListadoArchivosTarea(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'IdServicio': '" + "0" + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + $('#frmTxtCodigo').val() + "'";
    datosParametros = datosParametros + "}";

    datos = "[{'action': 'ListaArchivosTarea', 'parameters' : " + datosParametros + " }]";

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


function VerListadoArchivosTarea(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'IdServicio': '" + "0" + "',";
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

    datos = "[{'action': 'BorrarArchivosTarea', 'parameters' : " + datosParametros + " }]";

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

function CancelarCambios() {
    // Ocultar formulario y visualizar grid
    $("#formActualizaTarea").hide();
    $("#ContentPlaceHolder1_Panel5").show();

    return;
}

function BtnConsulta() {
    $("#divMensajes").html("");
    ObtenerListaTareasPorUsuario();
}

function BtnDescarga() {
    $("#divMensajes").html("");
    ObtenerListaTareasDescargaPorUsuario();
}

function GuardarCambioEstado() {
    var url = "AdministrarTarea.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    if ($('#frmTxtTareaDetalle').val() == "") {
        mensajeVerificacion += "- Debe ingresar la descripción de la actividad.<br>";
        contadorVerificacion += 1;
    }

    if ($('#cmbEstadoSeleccionado').val() == "" || $('#cmbEstadoSeleccionado').val() == "0") {
        mensajeVerificacion += "- Debe seleccionar el estado de la actividad.<br>";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        mensaje = "<div class='alert alert-" + tipoMensaje + " alert-dismissable'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>" + mensajeVerificacion + "</div>";
        $('#messageNotify').html(mensaje);
        $('#messageNotify').show();
        return;
    }

    var comboEstadoSeleccionado = document.getElementById("cmbEstadoSeleccionado");
    var selectEstadoSeleccionado = comboEstadoSeleccionado.options[comboEstadoSeleccionado.selectedIndex].text;

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'frmTxtCodigoTarea': '" + $('#frmTxtCodigoTarea').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtCodigo': '" + $('#frmTxtCodigo').val() + "',";
    datosFormulario = datosFormulario + "'cmbEstadoSeleccionado': '" + $('#cmbEstadoSeleccionado').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtTareaDetalle': '" + $('#frmTxtTareaDetalle').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtObservaciones': '" + $('#frmTxtObservaciones').val() + "',";
    datosFormulario = datosFormulario + "'selectEstadoSeleccionado': '" + $('#selectEstadoSeleccionado').val() + "',";
    datosFormulario = datosFormulario + "'frmcmbHorasExtras': '" + $('#frmcmbHorasExtras').val() + "'";
    datosFormulario = datosFormulario + "}";
    
    datos = "[{'action': 'GuardarCambioEstado', 'parameters' : " + datosFormulario + " }]";

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
                ObtenerListaTareasPorUsuario();
                //window.location.href = "http://portaldeservicios.dos.com.ec/RTareas/Formulario/Principal.aspx";
            }
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });

    $("#btnGuardarCambioEstado").hide();

    // Ocultar formulario y visualizar grid
    $("#formActualizaTarea").hide();
    $("#ContentPlaceHolder1_Panel5").show();

    return;
}

function SeleccionarTareasNuevoDetalle() {

    SeleccionarListaTareasPorUsuario();

    // Ocultar grid y visualizar modal selección tarea principal
    $("#ContentPlaceHolder1_Panel5").hide();
    $("#dialogListaTarea").show();

    $("#divMensajes").html("");

    return;
}

function NuevoDetalleTarea(codigoTarea, codigoTareaAranda, boton) {

    // Limpio y seteo el formulario para ingresar nuevo registro

    $('#messageNotify').html("");
    $('#messageNotify').hide();
    $("#fomTitleLabel").html("Nueva Tarea");
    $("#divFrmTxtCodigo").hide();
    var tdDescripcion = "#td-descripcionTarea-" + codigoTarea;
    $("#divFrmTxtTareaPrincipalDescripcion").show();
    $("#frmTxtTareaPrincipalDescripcion").val($(tdDescripcion).html());
    $("#frmTxtCodigoAranda").val(codigoTareaAranda);
    $("#divFrmTxtCodigoTarea").show();
    $("#frmTxtCodigoTarea").val(codigoTarea);
    $("#frmTxtTareaDetalle").val("");
    $("#frmTxtFecha").val("");
    $("#frmTxtTiempo").val("");
    $("#frmTxtHoraDesde").val("08:30");
    $("#frmTxtHoraHasta").val("13:00");
    $("#frmTxtObservaciones").val("");
    //$("#frmcmbHorasExtras option[value=0]").attr("selected", true);
    $("#frmcmbHorasExtras").val("0");
    // Ocultar grid y visualizar formulario
    $("#ContentPlaceHolder1_Panel5").hide();
    $("#formActualizaTarea").show();

    $("#dialogListaTarea").hide();

    // Ocultar botones del formulario
    $("#btnGuardarNuevaTarea").hide();
    $("#btnGuardarCambiosTarea").hide();
    $("#btnBorrarTarea").hide();
    $(boton).show(); //Habilita Boton requerido

    $("#divMensajes").html("");

    return;
}

function CerrarSeleccionTarea() {
    // Ocultar grid selección y visualizar grid tareas detalle
    $('#messageNotify').html("");
    $('#messageNotify').hide();
    $("#dialogListaTarea").hide();
    $("#ContentPlaceHolder1_Panel5").show();

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


$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    ObtenerListaTareasPorUsuario();

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


    // ------------------------------------------------------------------

    $('#btnCargarArchivosAdjuntos').click(function () {

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
            fileData.append('Id_RegTareas', $("#frmTxtCodigo").val()); 

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
                        ListadoArchivosTarea("#divArchivosAdjuntosAnteriores", $("#frmTxtCodigo").val());
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
    });

});
