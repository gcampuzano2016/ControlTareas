/* Información de Nuestro Framework */
var tipoGeneral = 0;
var idregistroGeneral = 0;


function ObtenerListaUsuarios() {
    var Datos = "[{ \"action\": \"ListaUsuarios\", \"parameters\" : \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\" }]";
    CargarPagina('#cmbUsuarios', 'ObtenerListaTareas.ashx', Datos, "select", "");
}

//actualizacion de Guillo
function ObtenerListaComboAprobacionTarea() {

    document.getElementById('cmbAprobacionTarea').innerText = null
    var Datos = "[{ \"action\": \"ListaComboAprobacionTarea\", \"parameters\" : { \"catalogo\" : \"9\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#cmbAprobacionTarea', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboAprobacionTareaQA() {

    document.getElementById('cmbAprobacionTareaQA').innerText = null
    var Datos = "[{ \"action\": \"ListaComboAprobacionTareaQA\", \"parameters\" : { \"catalogo\" : \"10\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#cmbAprobacionTareaQA', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboTipoGasto() {

    document.getElementById('frmCmbTipoGasto').innerText = null
    var Datos = "[{ \"action\": \"ListaComboGastoReporte\", \"parameters\" : { \"catalogo\" : \"11\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#frmCmbTipoGasto', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboTipoFecha() {

    document.getElementById('cmbTipoFecha').innerText = null
    var Datos = "[{ \"action\": \"ListaComboFecha\", \"parameters\" : { \"catalogo\" : \"12\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#cmbTipoFecha', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}


function SeleccionarTipoFecha(idTipoFecha) {
    var val = idTipoFecha;
    if (val == 1) {

    }
    else if (val == 2) {

    }
    else if (val == 3) {

    }
}

//actualizacion de Guillo

function SeleccionarListaTareasPorUsuario() {
    var Datos = "[{ \"action\": \"ListaTareasPorUsuario\", \"parameters\" : \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\" }]";
    CargarPagina('#datosTablaTareasSeleccion', 'ObtenerListaTareas.ashx', Datos, "tableSelect", "");
}


function ObtenerListaTareasDescargaPorUsuario() {
    var Datos = "[{ \"action\": \"DetalleTareasDescargaXLS\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios").val() + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", idtipogasto: \"" + $("#frmCmbTipoGasto").val() + "\", cliente: \"" + $("#txtCliente").val() + "\", orden: \"" + $("#txtNumeroOrden").val() + "\", AprobarTarea: \"" + $("#cmbAprobacionTarea").val() + "\", AprobarTareaQA: \"" + $("#cmbAprobacionTareaQA").val() + "\", TipoFecha: \"" + $("#cmbTipoFecha").val() + "\"} }]";
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


function ObtenerListaTareasPorUsuario() {
    var Datos = "[{ \"action\": \"ListaDetalleTareasPorUsuario\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios").val() + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", idtipogasto: \"" + $("#frmCmbTipoGasto").val() + "\", cliente: \"" + $("#txtCliente").val() + "\", orden: \"" + $("#txtNumeroOrden").val() + "\", AprobarTarea: \"" + $("#cmbAprobacionTarea").val() + "\", AprobarTareaQA: \"" + $("#cmbAprobacionTareaQA").val() + "\", TipoFecha: \"" + $("#cmbTipoFecha").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table", "");
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

    if (tipoControl == "tableSelect2") {
        contenido = RecorreJSONTableSelect2(json);
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
    info = info + thInicial + "Estado Aprobación" + thFinal;
    info = info + thInicial + "Categoria de Servicio" + thFinal;
    info = info + thInicial + "Cliente" + thFinal;
    info = info + thInicial + "Num Aranda" + thFinal;
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
    info = info + thInicial + "Estado Aprobación Tarea" + thFinal;
    info = info + thInicial + "Fecha Aprobación Tarea" + thFinal;
    info = info + thInicial + "Estado Aprobación QA Tarea" + thFinal;
    info = info + thInicial + "Fecha Aprobación QA Tarea" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";

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

        info = info + "<td class='sorting_1' style='text-align:center;'>";
        if (item.Det_Aprobacion_Tarea_Estado == '2') {
            if (item.Det_Aprobacion_Tarea_Estado_QA == '2') {
                info = info + "<i class='fa fa-check-circle'></i>";
            } else {
                info = info + "<i class='fa fa-check-circle-o'></i>";
            }
        } else {
            info = info + "<i class='fa fa-circle-o'></i>";
        }
        info = info + "</td > ";
        info = info + "<td class='sorting_1'>" + item.DetCategoria + "</td>";
        info = info + "<td class='sorting_1'>" + item.Nom_Cliente + "</td>";
        info = info + "<td class='sorting_1'>" + item.NumAranda + "</td>";
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

        if (item.Det_Aprobacion_Tarea_Estado == '1') {
            info = info + "<td class='sorting_1'><div class='alert alert-info'>" + item.Det_Aprobacion_Tarea_Estado_Descripcion + "</div></td>";
        } else if (item.Det_Aprobacion_Tarea_Estado == '2') {
            info = info + "<td class='sorting_1'><div class='alert alert-success'>" + item.Det_Aprobacion_Tarea_Estado_Descripcion + "</div></td>";
        } else if (item.Det_Aprobacion_Tarea_Estado == '3') {
            info = info + "<td class='sorting_1'><div class='alert alert-danger'>" + item.Det_Aprobacion_Tarea_Estado_Descripcion + "</div></td>";
        } else {
            info = info + "<td class='sorting_1'>" + item.Det_Aprobacion_Tarea_Estado_Descripcion + "</td>";
        }
        info = info + "<td class='sorting_1'>" + item.Det_Fecha_Aprobacion_Tarea + "</td>";

        if (item.Det_Aprobacion_Tarea_Estado_QA == '1') {
            info = info + "<td class='sorting_1'><div class='alert alert-info'>" + item.Det_Aprobacion_Tarea_Estado_QA_Descripcion + "</div></td>";
        } else if (item.Det_Aprobacion_Tarea_Estado_QA == '2') {
            info = info + "<td class='sorting_1'><div class='alert alert-success'>" + item.Det_Aprobacion_Tarea_Estado_QA_Descripcion + "</div></td>";
        } else if (item.Det_Aprobacion_Tarea_Estado_QA == '3') {
            info = info + "<td class='sorting_1'><div class='alert alert-danger'>" + item.Det_Aprobacion_Tarea_Estado_QA_Descripcion + "</div></td>";
        } else {
            info = info + "<td class='sorting_1'>" + item.Det_Aprobacion_Tarea_Estado_QA_Descripcion + "</td>";
        }
        info = info + "<td class='sorting_1'>" + item.Det_Fecha_Aprobacion_Tarea_QA + "</td>";


        info = info + "</tr>";
    });

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
    ObtenerListaTareasPorUsuario();
}

function BtnDescarga() {
    $("#divMensajes").html("");
    ObtenerListaTareasDescargaPorUsuario();
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


function VerTipoReporte() {

    var comboReporte = document.getElementById("cboReporte");
    var selectedoReporte = comboReporte.options[comboReporte.selectedIndex].text;

    if (selectedoReporte == "REPORTE GENERAL") {
        document.getElementById("ReporteGeneral").style.display = "block";
        document.getElementById("ReporteTareaNoEjecutada").style.display = "none";
        document.getElementById("ReporteTareaSinInforme").style.display = "none";
    }
    else if (selectedoReporte == "TAREAS NO EJECUTADAS") {
        document.getElementById("ReporteGeneral").style.display = "none";
        document.getElementById("ReporteTareaNoEjecutada").style.display = "block";
        document.getElementById("ReporteTareaSinInforme").style.display = "none";
    }
    else if (selectedoReporte == "TAREAS SIN INFORMES") {
        document.getElementById("ReporteGeneral").style.display = "none";
        document.getElementById("ReporteTareaNoEjecutada").style.display = "none";
        document.getElementById("ReporteTareaSinInforme").style.display = "block";
    }

}

function BtnConsultaNo() {
    tipoGeneral = 0;
    idregistroGeneral = 0;
    ObtenerListaTareasHorasExtras(0, 0)
}

function ObtenerListaTareasHorasExtras(tipo, idRegistro) {
    var Datos = "[{ \"action\": \"ListaConsultaTareasGeneradasPorRevisarUnificado\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios").val() + "\", registro : \"" + idRegistro + "\", fechaDesde: \"" + $("#txtFechaDesdeNo").val() + "\", fechaHasta: \"" + $("#txtFechaHastaNo").val() + "\", estado: \"" + tipo + "\", busqueda: \"" + $("#txtUsuariosNo").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipalNo', 'ObtenerListaTareas.ashx', Datos, "tableSelect2", tipo);
}

function RecorreJSONTableSelect2(json, idSeleccionado) {
    var info = "";

    $("#listModalTitleLabel").html("Seleccionar Tarea Aranda");

    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    //info = info + thInicial + "ACCIONES" + thFinal;
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
        //info = info + "<td class='sorting_1' style='text-align:center'>";
        //if (item.conteoArchivosAdjuntos != '0') {
        //    info = info + "<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosTarea(\"#MensajeInformativo\", \"" + item.Id_RegTareas + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
        //    info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        //}
        //if (idSeleccionado == 0) {

        //    info = info + "<i class='fa fa-check-square' style='cursor: pointer' onclick='VerDetalleTareas(\"" + 1 + "\",\"" + item.Id_RegTareas + "\");'></i>";
        //}
        //else {
        //    info = info + "<i class='fa fa-check-square' style='cursor: pointer' onclick='VerDetalleTareas(\"" + 0 + "\",\"" + item.Id_RegTareas + "\");'></i>";
        //}
        //info = info + "</td > ";
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


function BtnDescargaNo() {
    ObtenerListaHorasExtrasDescarga(tipoGeneral, idregistroGeneral);
}

function ObtenerListaHorasExtrasDescarga(tipo, idRegistro) {
    var Datos = "[{ \"action\": \"ListaConsultaTareasGeneradasDescargaPorRevisarUnificado\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios").val() + "\", registro : \"" + idRegistro + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", estado: \"" + tipo + "\", busqueda: \"" + $("#txtUsuariosNo").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipalNo', 'ObtenerListaTareas.ashx', Datos, "table");
}

$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    ObtenerListaUsuarios();
    ObtenerListaComboTipoGasto();
    ObtenerListaComboAprobacionTareaQA();
    ObtenerListaComboAprobacionTarea();
    ObtenerListaComboTipoFecha();

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


    from = $("#txtFechaDesdeNo").datepicker(
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

        to = $("#txtFechaHastaNo").datepicker(
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

    $("#txtFechaDesdeNo").datepicker('setDate', 'today');
    $("#txtFechaHastaNo").datepicker('setDate', 'today');


    from = $("#txtFechaDesdeSin").datepicker(
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

        to = $("#txtFechaHastaSin").datepicker(
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

    $("#txtFechaDesdeSin").datepicker('setDate', 'today');
    $("#txtFechaHastaSin").datepicker('setDate', 'today');


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
