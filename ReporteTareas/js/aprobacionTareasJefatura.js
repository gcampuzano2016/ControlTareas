/* Información de Nuestro Framework */
var fechaProceso = "";
var tipoGeneral = 0;
var idregistroGeneral = 0;

function ObtenerListaUsuarios() {
    var Datos = "[{ \"action\": \"ListaUsuarios\", \"parameters\" : \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\" }]";
    CargarPagina('#cmbUsuarios', 'ObtenerListaTareas.ashx', Datos, "select", "", "", "");
}


function SeleccionarListaRecursosHorasDiarias() {
    var Datos = "[{ \"action\": \"ListaRecursosHorasDiarias\", \"parameters\" : \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\" }]";
    CargarPagina('#datosTablaTareasSeleccion', 'ObtenerListaTareas.ashx', Datos, "tableSelect", "", "", "");
}

function SeleccionarListaTareasCambioPorUsuario(idTareaCambio, idResponsable) {
    $("#frmTxtCodigoCambio").val(idTareaCambio);
    var Datos = "[{ \"action\": \"ListaTareasPorUsuario\", \"parameters\" : { \"codigoResponsable\" : \"" + idResponsable + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", idtipogasto: \"" + "0" + "\", cliente: \"" + "" + "\", orden: \"" + "" + "\", AprobarTarea: \"" + "0" + "\", AprobarTareaQA: \"" + "0" + "\", TipoFecha: \"" + "1" + "\"} }]";
    CargarPagina('#datosTablaTareasSeleccionCambio', 'ObtenerListaTareas.ashx', Datos, "tableSelectChange", "", "");
}

function ObtenerListaRecursosHorasDiarias() {
    var Datos = "[{ \"action\": \"ListaRecursosHorasDiarias\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios").val() + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", estado: \"" + $("#cmbEstados").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table", "", "", "#divListadoDetalleTareasDiarias");
}

function ObtenerDetalleTareaBorrar(idDetalleTarea, id) {

    if (id == "5") {
        document.getElementById("btnBorrarTarea").innerHTML = "Restaurar tarea"; 
    }
    else if(id == "1" || id == "2") {
        document.getElementById("btnBorrarTarea").innerHTML = "Borrar tarea"; 
    }

    $('#frmTxtCodigo').val(idDetalleTarea);
    var Datos = "[{ \"action\": \"DetalleTarea\", \"parameters\" : \"" + idDetalleTarea + "\" }]";
    CargarPagina('#divFormDetalleTarea', 'ObtenerListaTareas.ashx', Datos, "form", "#btnBorrarTarea", "", "", "");
}


//actualizacion de Guillo
function SeleccionarTipoActividades(idTipoActividad) {
    var val = idTipoActividad;
    if (val > 0) {
        document.getElementById('frmCmbTipoActividad').innerText = null
        $("#divFrmCmbTipoActividad").show();
        var Datos = "[{ \"action\": \"ListaComboCatalogo\", \"parameters\" : { \"catalogo\" : \"" + idTipoActividad + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
        CargarPagina('#frmCmbTipoActividad', 'ObtenerListaTareas.ashx', Datos, "select", "", idTipoActividad);
    }
}

function SeleccionarTipoActividades2(idTipoGasto, idTipoActividad) {
    document.getElementById('frmCmbTipoActividad').innerText = null
    var Datos = "[{ \"action\": \"ListaComboCatalogo\", \"parameters\" : { \"catalogo\" : \"" + idTipoGasto + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#frmCmbTipoActividad', 'ObtenerListaTareas.ashx', Datos, "select", "", idTipoActividad);
}

function ObtenerListaComboTipoGastoReporte() {

    document.getElementById('frmCmbTipoGasto').innerText = null
    var Datos = "[{ \"action\": \"ListaComboGastoReporte\", \"parameters\" : { \"catalogo\" : \"11\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#frmCmbTipoGastoReporte', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}
//actualizacion de Guillo


function CargarPagina(div, url, datos, tipoControl, boton, divPanelShow, divPanelHide, idSeleccionado) {

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
                    //if (typeof respuesta.estado == "undefined") {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, boton, divPanelShow, divPanelHide, idSeleccionado));
                    //} else {
                    //    MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);
                    //}

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

function ObtenerDetalleTarea(idDetalleTarea) {
    $('#frmTxtCodigo').val(idDetalleTarea);
    $("#divFrmBtnCambiarTareaPrincipal").show();
    $("#divListadoDetalleTareasDiarias").hide();
    $("#datosTablaDetail").html();
    var Datos = "[{ \"action\": \"DetalleTarea\", \"parameters\" : \"" + idDetalleTarea + "\" }]";
    CargarPagina('#divFormDetalleTarea', 'ObtenerListaTareas.ashx', Datos, "form", "#btnGuardarCambiosTarea", "");
}

function ObtenerListaComboTipoActividad(idSeleccionado) {
    var Datos = "[{ \"action\": \"ListaComboCatalogo\", \"parameters\" : { \"catalogo\" : \"5\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#frmCmbTipoActividad', 'ObtenerListaTareas.ashx', Datos, "select", "", "", "", idSeleccionado);
}


function RecorreJSON(div, json, tipoControl, boton, divPanelShow, divPanelHide, idSeleccionado) {

    var contenido = "";

    if (tipoControl == "table") {
        contenido = RecorreJSONTable(json);
        $(div).html(contenido);
        $(divPanelHide).hide();
    }
    if (tipoControl == "select") {
        contenido = RecorreJSONSelect(div, json, idSeleccionado);
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

    if (tipoControl == "tableSelectChange") {
        contenido = RecorreJSONTableSelectChange(json);
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
            info = info + "<i class='fa fa-check-square' style='cursor: pointer' onclick='VerAprobarTareas(\"" + item.NombreResponsable + "\",\"" + item.Id + "\");'></i>";
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
            info = info + "<i class='fa fa-undo' style='cursor: pointer' onclick='VerAnularAprobacionTareas(\"" + item.NombreResponsable + "\",\"" + item.Id + "\");'></i>";
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
    $("#datosTablaDetail").show();   
    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;text-align:center;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "Acciones" + thFinal;
    info = info + thInicial + "Aprobación Mensual" + thFinal;
    info = info + thInicial + "Horas Disponible" + thFinal;
    info = info + thInicial + "Saldo Costo" + thFinal;
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

    var d = new Date();
    var theDate = d.getFullYear() + '-' + (d.getMonth() + 1) + '-' + d.getDate();
    var theTime = theDate + " 15:00:00";
    var newTime = new Date(Date.parse(theTime) + 30 * 60 * 1000);
    var formatoTiempo = "";
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

        info = info + "<td class='sorting_1' style='text-align:center'>";
        if (item.conteoArchivosAdjuntos != '0') {
            info = info + "<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosTarea(\"#MensajeInformativo\", \"" + item.idTarea + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
            info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        }
        info = info + "<i class='fa fa-check-square' style='cursor: pointer' onclick='VerAprobarTareasIndividual(\"" + item.idDetalleTarea + "\",\"" + item.UsuarioResponsable + "\",\"" + item.Id_Responsable + "\",\"" + item.Det_Fecha + "\",\"" + item.Det_Fecha + "\");'></i>";
        info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        info = info + "<i class='fa fa-edit' style='cursor: pointer' onclick='ObtenerDetalleTarea(\"" + item.idDetalleTarea + "\");'></i>";
        info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";

        if (item.Det_Aprobacion_Tarea_Estado == "1") {
            info = info + "<i class='fa fa-trash-o' style='cursor: pointer' onclick='ObtenerDetalleTareaBorrar(\"" + item.idDetalleTarea + "\",\"" + item.Det_Aprobacion_Tarea_Estado + "\");'></i>";
            document.getElementById("btnBorrarTarea").innerHTML = "Borrar tarea"; 
        } else if (item.Det_Aprobacion_Tarea_Estado == "2") {
            info = info + "<i class='fa fa-trash-o' style='cursor: pointer' onclick='ObtenerDetalleTareaBorrar(\"" + item.idDetalleTarea + "\",\"" + item.Det_Aprobacion_Tarea_Estado + "\");'></i>";
            document.getElementById("btnBorrarTarea").innerHTML = "Borrar tarea"; 
        } else if (item.Det_Aprobacion_Tarea_Estado == "5") {
            info = info + "<i class='fa fa-pencil fa-fw' style='cursor: pointer' onclick='ObtenerDetalleTareaBorrar(\"" + item.idDetalleTarea + "\",\"" + item.Det_Aprobacion_Tarea_Estado + "\");'></i>";
            document.getElementById("btnBorrarTarea").innerHTML = "Restaurar tarea"; 
        }


        info = info + "</td>";
        info = info + "<td class='sorting_1'>";
        if (item.Det_Aprobacion_Tarea_Estado == "1") {
            info = info + "<div class='alert alert-info'>Pendiente</div>";
        } else if (item.Det_Aprobacion_Tarea_Estado == "2") {
            info = info + "<div class='alert alert-success'>Aprobado</div>";
        } else if (item.Det_Aprobacion_Tarea_Estado == "5") {
            info = info + "<div class='alert alert-danger'>Rechazada</div>";

        } else {
            info = info + "";
        }
        info = info + "</td>";
        info = info + "<td class='sorting_1'>" + item.HORAS_DISPONIBLES + "</td>";
        info = info + "<td class='sorting_1'>" + item.SALDO_DE_COSTOS + "</td>";
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

        //theTime = theDate + " " + item.Det_Tiempo;
        //newTime = new Date(Date.parse(theTime));
        //formatoTiempo = time_format(newTime);
        //tiempoTotal = tiempoTotal + newTime;

        //info = info + "<td class='sorting_1'>" + tiempoTotal + "</td>";
        info = info + "</tr>";

    });

    info = info + "<tr>";
    info = info + "<td class='sorting_1' colspan='10' style='text-align: right;'>TOTAL HORAS:</td>";
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


function RecorreJSONSelect(div, selectValues, idSeleccionado) {

    if (div == "#frmCmbTipoActividad") {
        document.getElementById('frmCmbTipoActividad').innerText = null;
    }

    $(div).empty();

    $.each(selectValues, function (key, value) {
        //
        // Despliegue de datos
        //

        if (value.Id == idSeleccionado) {
            $(div)
                .append($("<option></option>")
                    .attr("value", value.Id)
                    .text(value.Valor)
                    .prop("selected", true)
                );
        } else {
            $(div)
                .append($("<option></option>")
                    .attr("value", value.Id)
                    .text(value.Valor));
        }
    });

    return;
}

function RecorreJSONTableSelectChange(json) {
    var info = "";

    $("#listModalTitleLabelCambio").html("Seleccionar Tarea Aranda");

    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "Sel." + thFinal;
    info = info + thInicial + "Codigo Tarea" + thFinal;
    info = info + thInicial + "Cliente" + thFinal;
    info = info + thInicial + "N° de orden de servicio" + thFinal;
    info = info + thInicial + "Cod. Aranda" + thFinal;
    info = info + thInicial + "Tipo de actividad" + thFinal;
    info = info + thInicial + "Descripción Tarea" + thFinal;
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
        info = info + "<td class='sorting_1' style='cursor: pointer;' onclick='RecorreJSONFormCambio(\"" + item.Id_RegTareas + "\", \"" + item.Num_OrdenServicio + "\", \"" + item.Id_CompAranda + "\", \"" + item.Det_Tarea + "\", \"" + "#btnGuardarNuevaTarea" + "\");'><i class='fa fa-hand-o-right'></i></td>";
        info = info + "<td class='sorting_1'>" + item.Id_RegTareas + "</td>";
        info = info + "<td class='sorting_1'>" + item.Nom_Empresa + "</td>";
        info = info + "<td class='sorting_1'>" + item.Num_OrdenServicio + "</td>";
        info = info + "<td class='sorting_1'>" + item.Id_CompAranda + "</td>";
        info = info + "<td class='sorting_1'>" + item.CatalogoTareaSap + "</td>";
        info = info + "<td class='sorting_1' id='td-descripcionTarea-" + item.Id_RegTareas + "'>" + item.Det_Tarea + "</td>";
        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    $("#formActualizaTarea").hide();
    $("#dialogListaTareaCambio").show();

    return info;
}

function RecorreJSONFormCambio(idTarea, numeroOrdenServicio, Id_CompAranda, detalleTarea, div) {
    // Ocultar visualizar grid
    $("#dialogListaTareaCambio").hide();

    //ObtenerDetalleTarea($("#frmTxtCodigoCambio").val());
    $("#frmTxtCodigoTarea").val(idTarea);
    $("#frmTxtOrdenServicio").val(numeroOrdenServicio);
    $("#frmTxtCodigoAranda").val(Id_CompAranda);
    $("#frmTxtTareaPrincipalDescripcion").val(detalleTarea);

    $("#divFrmBtnCambiarTareaPrincipal").show();
    $("#formActualizaTarea").show();

    return;
}


function RecorreJSONForm(div, item, boton) {
    var info = "";

    $("#fomTitleLabel").html("Actualizar Tarea");

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

    ObtenerListaComboTipoActividad(item.Cod_CatalogoTareaSap);
    $("#frmTxtOrdenServicio").val(item.Det_Num_OrdenServicio);
    $("#frmTxtTareaPrincipalDescripcion").val(item.Det_Tarea); // TODO
    $("#divFrmTxtCodigoTarea").hide();
    $("#frmTxtCodigoTarea").val(item.Id_RegTareas);
    $("#frmTxtCodigoAranda").val(item.Det_Id_CompAranda);
    $("#frmTxtCodigoResponsable").val(item.Id_Responsable);
    $("#frmTxtFecha").val(dateTarea);
    $("#frmTxtHoraDesde").val(timeTareaInicio);
    $("#frmTxtHoraHasta").val(timeTareaFin);
    $("#frmTxtObservaciones").val(item.Det_Observaciones);
    $("#frmTxtTareaDetalle").val(item.Det_Det_Tarea);
    //$("#frmcmbHorasExtras option[value=" + item.Det_Horas_Extras_Tipo + "]").attr("selected", true);
    $("#frmcmbHorasExtras").val(item.Det_Horas_Extras_Tipo);

    // Calcular intervalo de tiempo en horas
    calculeTime(timeTareaInicio, timeTareaFin, '#frmTxtTiempo');

    ListadoArchivosTarea("#divArchivosAdjuntosAnteriores", item.Id_RegTareas);

    $("#divFrmTxtTareaPrincipalDescripcion").show();
    $("#divFrmTxtCodigo").show();
    $("#divFrmTxtCodigoAranda").show();
    $("#btnGuardarNuevaTarea").hide();
    $("#btnGuardarCambiosTarea").hide();
    $("#btnBorrarTarea").hide();
    $(boton).show();

    $("#divMensajes").html("");


    return info;
}

function CancelarCambios() {
    // Ocultar formulario y visualizar grid
    $("#formActualizaTarea").hide();
    $("#divFrmBtnCambiarTareaPrincipal").hide();
    $("#ContentPlaceHolder1_Panel5").show();

    return;
}

function ConfirmarEliminarArchivo(codigoItem, iconArchivo, nombreArchivo) {
    // Habilitar mensaje
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

function CerrarSeleccionTarea() {
    // Ocultar grid selección y visualizar grid tareas detalle
    $('#messageNotify').html("");
    $('#messageNotify').hide();
    $("#dialogListaTarea").hide();
    $("#dialogListaTareaCambio").hide();
    $("#ContentPlaceHolder1_Panel5").show();

    return;
}

function BtnConsulta() {

    if ($("#cmbEstados").val() == "1" || $("#cmbEstados").val() == "2") {

        $("#divMensajes").html("");
        ObtenerListaRecursosHorasDiarias();
        document.getElementById("Pagina1").style.display = "block";
        document.getElementById("Pagina2").style.display = "none";
    }
    else if( $("#cmbEstados").val() == "3" ){
        document.getElementById("Pagina1").style.display = "none";
        document.getElementById("Pagina2").style.display = "block";
        tipoGeneral = 0;
        idregistroGeneral = 0;
        ObtenerListaTareasHorasExtras(0, 0);
    }
}

function BtnDescarga() {
    $("#divMensajes").html("");
    ObtenerListaHorasExtrasDescarga(tipoGeneral, idregistroGeneral);
}

function BuscarEstado() {

    if ($("#cmbEstados").val() == "1" || $("#cmbEstados").val() == "2") {
        document.getElementById("btnDescarga").style.display = "none";
    }
    else if ($("#cmbEstados").val() == "3") {
        document.getElementById("btnDescarga").style.display = "block";
    }

}

function ObtenerListaTareasHorasExtras(tipo, idRegistro) {
    var Datos = "[{ \"action\": \"ListaConsultaTareasGeneradasPorRevisar\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios").val() + "\", registro : \"" + idRegistro + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", estado: \"" + tipo + "\", busqueda: \"" + "" + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal1', 'ObtenerListaTareas.ashx', Datos, "tableSelect2", tipo);
}

function ObtenerListaHorasExtrasDescarga(tipo, idRegistro) {
    var Datos = "[{ \"action\": \"ListaConsultaTareasGeneradasDescargaPorRevisar\", \"parameters\" : { \"usuario\" : \"" + $("#cmbUsuarios").val() + "\", registro : \"" + idRegistro + "\", fechaDesde: \"" + $("#txtFechaDesde").val() + "\", fechaHasta: \"" + $("#txtFechaHasta").val() + "\", estado: \"" + tipo + "\", busqueda: \"" + "" + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    DetalleTareasDescargaXLS('#datosTablaPrincipal1', 'ObtenerListaTareas.ashx', Datos, "table");
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

function VerAprobarTareasIndividual(iddetalleTarea,responsable,id,fechaInicio,FechaFinal) {
    var mensaje = "<p style='font-size: 14px'>Realizar la aprobación de Tareas de <b>" + responsable + " desde el <b>" + fechaInicio + "</b> hasta el <b>" + FechaFinal + "?</b></p>";
    $("#txtCodigo").val(id);
    fechaProceso = fechaInicio;
    $("#divMensajeAprobar").html(mensaje);
    $("#modalMensajeAprobarLabelTitulo").html("Confirmar Aprobación de Tareas");
    $("#btnAceptarAprobar").attr("onclick", "AprobarTareasIndividual(  " + iddetalleTarea + "," + fechaInicio  +")");
    $("#btnAceptarAprobar").html("Aprobar Tareas");
    $("#modalMensajeAprobar").modal('show');
    return;
}


function GuardarCambiosTarea() {
    var div = "#divMensajes";
    var url = "AdministrarTarea.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    if ($('#frmTxtTareaDetalle').val() == "") {
        mensajeVerificacion += "- Debe ingresar la descripción de la actividad.<br>";
        contadorVerificacion += 1;
    }

    if ($('#frmCmbTipoActividad').val() == "" || $('#frmCmbTipoActividad').val() == "0") {
        mensajeVerificacion += "- Debe seleccionar el tipo de actividad.<br>";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        mensaje = "<div class='alert alert-" + tipoMensaje + " alert-dismissable'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>" + mensajeVerificacion + "</div>";
        $('#messageNotify').html(mensaje);
        $('#messageNotify').show();
        return;
    }

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'frmTxtCodigo': '" + $('#frmTxtCodigo').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtTareaDetalle': '" + $('#frmTxtTareaDetalle').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtFecha': '" + $('#frmTxtFecha').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtTiempo': '" + $('#frmTxtTiempo').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtHoraDesde': '" + $('#frmTxtHoraDesde').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtHoraHasta': '" + $('#frmTxtHoraHasta').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtObservaciones': '" + $('#frmTxtObservaciones').val() + "',";
    datosFormulario = datosFormulario + "'frmcmbHorasExtras': '" + $('#frmcmbHorasExtras').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtCodigoTarea': '" + $('#frmTxtCodigoTarea').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtOrdenServicio': '" + $('#frmTxtOrdenServicio').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtCodigoAranda': '" + $('#frmTxtCodigoAranda').val() + "',";
    datosFormulario = datosFormulario + "'frmCmbTipoActividad': '" + $('#frmCmbTipoActividad').val() + "',";
    datosFormulario = datosFormulario + "'frmCmbTipoGasto': '0'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'ActualizarDetalleTarea', 'parameters' : " + datosFormulario + " }]";

    $.ajax({
        type: "POST",
        url: url,
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function (respuesta) {
            $("#divMensajes").html("Guardando Información...");
        },
        success: function (respuesta) {
            var mensaje = "";
            if (respuesta.estado == "1") {
                //ObtenerListaTareasPorUsuario();
            }
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });

    $("#btnGuardarCambiosTarea").hide();


    // Ocultar formulario y visualizar grid
    $("#formActualizaTarea").hide();
    $("#divFrmBtnCambiarTareaPrincipal").hide();
    $("#ContentPlaceHolder1_Panel5").show();

    return;
}


function VerListadoDetalleTareas(id, fecha, responsable) {
    $("#listDetailTitleLabel").html("Lista de Tareas de <b> " + responsable + "</b> del <b>" + fecha + "</b>");
    $("#txtCodigo").val("");
    var Datos = "[{ \"action\": \"ListaDetalleTareasPorUsuarioRechazada\", \"parameters\" : { \"usuario\" : \"" + id + "\", fechaDesde: \"" + fecha + "\", fechaHasta: \"" + fecha + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", idtipogasto: \"" + "0" + "\", cliente: \"" + "" + "\", orden: \"" + "" + "\", AprobarTarea: \"" + "0" + "\", AprobarTareaQA: \"" + "0" + "\", TipoFecha: \"" + "1" + "\"} }]";
    CargarPagina('#datosTablaDetail', 'ObtenerListaTareas.ashx', Datos, "tableDetail", "", "#divListadoDetalleTareasDiarias");
    $("#txtCodigo").val(id);
 
    //$("html, body").animate({ scrollTop: $(document).height() - $(window).height() });
    $("html, body").animate({ scrollTop: $(document).height() }, 1000);
}

function VerAprobarTareas(responsable, id) {
    var mensaje = "<p style='font-size: 14px'>Realizar la aprobación de Tareas de <b>" + responsable + "</b> desde el <b>" + $("#txtFechaDesde").val() + "</b> hasta el <b>" + $("#txtFechaHasta").val() + "?</b></p>";
    $("#txtCodigo").val(id);
    $("#divMensajeAprobar").html(mensaje);
    $("#modalMensajeAprobarLabelTitulo").html("Confirmar Aprobación de Tareas");
    $("#btnAceptarAprobar").attr("onclick", "AprobarTareas()");
    $("#btnAceptarAprobar").html("Aprobar Tareas");
    $("#modalMensajeAprobar").modal('show');
    return;
}



function AprobarTareas() {
    var url = "AdministrarTarea.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    $("#modalMensajeAprobar").modal('hide');

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaHasta': '" + $('#txtFechaHasta').val() + "',";
    datosFormulario = datosFormulario + "'txtCodigo': '" + $('#txtCodigo').val() + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'AprobarTarea', 'parameters' : " + datosFormulario + " }]";

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
                // ObtenerListaTareasPorUsuario();
            }
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });

    $("#divListadoDetalleTareasDiarias").hide();

    return;

}

function AprobarTareasIndividual(idDetalleTarea,fecha) {
    var url = "AdministrarTarea.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    $("#modalMensajeAprobar").modal('hide');

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaHasta': '" + $('#txtFechaHasta').val() + "',";
    datosFormulario = datosFormulario + "'idDetalleTarea': '" + idDetalleTarea + "',";
    datosFormulario = datosFormulario + "'txtCodigo': '" + $('#txtCodigo').val() + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'AprobarTareaIndividual', 'parameters' : " + datosFormulario + " }]";

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
                // ObtenerListaTareasPorUsuario();
                VerListadoDetalleTareas($('#txtCodigo').val(), fechaProceso, "");
            }
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });

    //$("#divListadoDetalleTareasDiarias").hide();

    return;

}

function VerAnularAprobacionTareas(responsable, id) {
    var mensaje = "<p style='font-size: 14px'>Anular la aprobación de las Tareas de <b>" + responsable + "</b> desde el <b>" + $("#txtFechaDesde").val() + "</b> hasta el <b>" + $("#txtFechaHasta").val() + "?</b></p>";
    $("#txtCodigo").val(id);
    $("#divMensajeAprobar").html(mensaje);
    $("#modalMensajeAprobarLabelTitulo").html("Confirmar Anulación de Aprobaciones");
    $("#btnAceptarAprobar").attr("onclick", "AnularAprobacionTareas()");
    $("#btnAceptarAprobar").html("Anular Aprobación de Tareas");
    $("#modalMensajeAprobar").modal('show');
    return;
}


function AnularAprobacionTareas() {
    var url = "AdministrarTarea.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    $("#modalMensajeAprobar").modal('hide');

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'txtFechaDesde': '" + $('#txtFechaDesde').val() + "',";
    datosFormulario = datosFormulario + "'txtFechaHasta': '" + $('#txtFechaHasta').val() + "',";
    datosFormulario = datosFormulario + "'txtCodigo': '" + $('#txtCodigo').val() + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'AnularAprobacionTareas', 'parameters' : " + datosFormulario + " }]";

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
                // ObtenerListaTareasPorUsuario();
            }
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });

    $("#divListadoDetalleTareasDiarias").hide();

    return;

}

function VerListadoArchivosTarea(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + idTarea + "',";
    datosParametros = datosParametros + "'IdServicio': '" + "0" + "'";
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

function ListadoArchivosTarea(div, idTarea) {

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


// Función de calculo de intervalo de tiempo en horas
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

function BorrarTarea() {
    var div = "#divMensajes";
    var url = "AdministrarTarea.ashx";
    var datos = "";

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'frmTxtCodigo': '" + $('#frmTxtCodigo').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtTareaDetalle': '" + $('#frmTxtTareaDetalle').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtFecha': '" + $('#frmTxtFecha').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtTiempo': '" + $('#frmTxtTiempo').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtHoraDesde': '" + $('#frmTxtHoraDesde').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtHoraHasta': '" + $('#frmTxtHoraHasta').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtObservaciones': '" + $('#frmTxtObservaciones').val() + "',";
    datosFormulario = datosFormulario + "'frmcmbHorasExtras': '" + $('#frmcmbHorasExtras').val() + "'";
    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'CambiarEstadoDetalleTarea', 'parameters' : " + datosFormulario + " }]";

    $.ajax({
        type: "POST",
        url: url,
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function (respuesta) {
            $("#divMensajes").html("Borrarndo Información...");
        },
        success: function (respuesta) {
            var mensaje = "";
            if (respuesta.estado == "1") {
                //ObtenerListaTareasPorUsuario();
                VerListadoDetalleTareas($('#txtCodigo').val(), $('#frmTxtFecha').val(), "");
            }
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);

        },
        error: function (objeto, msgError, objError) {
            var mesnajeError = "La acción de Guardado de información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
            MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
        }
    });

    //$("#btnBorrarTarea").hide();
    //$("#datosTablaDetail").hide();    

    // Ocultar formulario y visualizar grid
    $("#formActualizaTarea").hide();
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

    // ------------------------------------------------------------------------------

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
            fileData.append('Id_RegTareas', $("#frmTxtCodigoTarea").val());

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
                        ListadoArchivosTarea("#divArchivosAdjuntosAnteriores", $("#frmTxtCodigoTarea").val());
                    }
                    $("progress").hide();

                    $("#divListadoDetalleTareasDiarias").hide();
                    $("#datosTablaDetail").html();

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
