let idTipoSolicitud = 0;
let idVacaciones = 0;
let SaldoDias = 0;
let EstadoSolicitud = "";
let TipoSolicitud = 0;
let StrTipoSolicitud = "";

function DesactivarCheck1() {
    var text = document.getElementById("IdNO");
    if (text.checked == true) {
        text.checked = false;
    }
    document.getElementById("CargarHorarioRecuperacion").style.display = "none";
}

function CargarTodoEmpleados() {

    if ($('#txtReemplazo').val() == "0") {
        ObtenerListaComboReemplazo();
    }
    else if ($('#txtReemplazo').val() == "-1") {
        ObtenerListaComboReemplazoTodos()
    }

}

function DesactivarCheck2() {
    var text = document.getElementById("IdSI");
    if (text.checked == true) {
        text.checked = false;
    }

    var text2 = document.getElementById("IdNO");
    if (text2.checked == true) {
       // document.getElementById("CargarHorarioRecuperacion").style.display = "block";
    }
    else {
        //document.getElementById("CargarHorarioRecuperacion").style.display = "none";
    }
  }

function AgregarFormaDePago() {

        var tr = $(".tr_clone_field_forma_pago");
        var clone = tr.clone(true);
        //clone.find(':text').val('0');
        clone.removeClass("tr_clone_field_forma_pago");
        clone.append("<td><a class='table_delete_row_field' href='#' alt='Eliminar Campo Adicional'><i class='fa fa-trash-o'></i></button></td>")
        $(".table-forma-pago").find('tbody').append(clone);
        $(".table-forma-pago tbody").children(':last').hide().fadeIn(750);

        $(".table_delete_row_field").click(function () {
            $(this).closest('tr').fadeOut(500, function () {
                $(this).remove();
            });
            return false;
        });
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

function alerta2(respuesta) {
    sweetAlert("Advertencia", respuesta, "");
}

function CancelarCambios1() {
    document.getElementById("RegistroVacaciones").style.display = "none";
    document.getElementById("RegistroPermisos").style.display = "none";
    document.getElementById("ListaSolicitud").style.display = "block";
    var comboSolicitud = document.getElementById("cboSolicitud");
    comboSolicitud.selectedIndex = 0;
}

function CancelarCambios2() {
    document.getElementById("RegistroVacaciones").style.display = "none";
    document.getElementById("RegistroPermisos").style.display = "none";
    document.getElementById("ListaSolicitud").style.display = "block";
    var comboSolicitud = document.getElementById("cboSolicitud");
    comboSolicitud.selectedIndex = 0;
}

function BtnPermiso() {
    document.getElementById("RegistroVacaciones").style.display = "none";
    document.getElementById("RegistroPermisos").style.display = "block";
    document.getElementById("ListaSolicitud").style.display = "none";
}

function BtnVacaciones() {
    document.getElementById("RegistroVacaciones").style.display = "block";
    document.getElementById("RegistroPermisos").style.display = "none";
    document.getElementById("ListaSolicitud").style.display = "none";
}

function ObtenerListaComboSolicitud() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"35\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboSolicitud', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboSolicitud2() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"39\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#cboEstado2', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboReemplazo() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"36\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#txtReemplazo', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaComboReemplazoTodos() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"49\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + $("#ContentPlaceHolder1_txtIdCliente").val() + "\"} }]";
    CargarPagina('#txtReemplazo', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
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

    if (tipoControl == "select") {
        contenido = RecorreJSONSelect(div, json, idSeleccionado);
    }

    if (tipoControl == "tableSelect") {
        contenido = RecorreJSONTableSelect(json, boton);
        $(div).html(contenido);
    }

    if (tipoControl == "tableSelectPantalla") {
        contenido = RecorreDatosPantalla(json);
    }
    if (tipoControl == "tableSelectSaldo") {
        contenido = RecorreJSONTableSelectSaldos(json, boton);
    }
    if (tipoControl == "tableSelectDias") {
        contenido = RecorreDatosDiasSolicitados(json, boton);
    }
    if (tipoControl == "detalleDatos") {
        contenido = RecorreDatosSolicitud(json);
        $(div).html(contenido);
    }
    
    return contenido;
}

function VerDocumento() {
    let proceso = "";
}

function AprobarSolicitud(idvacaciones, estadoSolicitud, Descripcion) {

    if (estadoSolicitud == "APROBADO") {
        alerta2("ya no puede realizar cambio.")
    }
    else {
        idVacaciones = idvacaciones;
        StrTipoSolicitud = Descripcion;
        $("#modalCargarProceso").modal('show');
    }
}

function EditarSolicitud(idvacaciones, tipoVacacion) {
    idVacaciones = idvacaciones;
    BuscarSolicitudEditar(tipoVacacion);
    ObtenerSolicitudIndividual(idvacaciones);
    ListadoArchivosSolicitud("#divArchivosAdjuntosAnteriores",idvacaciones);
    document.getElementById("IdCargarArchivo").style.display = "block";
}

function ActualizarProceso() {
    if ($('#cboEstado2').val() == "-- SELECCIONE --") {
        alerta("Debe seleccionar un estado");
    }
    else {
        if ($('#txtDescripcionReq2').val() == "") {
            alerta("Debe agregar el motivo");
        }
        else {

            if ($('#cboEstado2').val() == "ANULAR") {
                ActualizarSolicitudProceso(4);
            }           
        }
    }

}

function ActualizarSolicitudProceso(tipo) {

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    if ($('#cboEstado2').val() == "SELECCIONAR") {
        mensajeVerificacion += "- Debe ingresar el estado ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'IdVacaciones': '" + idVacaciones + "',";
    datosFormulario = datosFormulario + "'EstadoSolicitud': '" + $('#cboEstado2').val() + "',";
    datosFormulario = datosFormulario + "'Motivo': '" + $('#txtDescripcionReq2').val() + "',";
    datosFormulario = datosFormulario + "'StrTipoSolicitud': '" + StrTipoSolicitud + "',";
    datosFormulario = datosFormulario + "'tipo': '" + tipo + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'ActualizarSolicitud', 'parameters' : " + datosFormulario + " }]";

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
                $("#modalCargarProceso").modal('hide');
                var combo2 = document.getElementById("cboEstado2");
                combo2.value = "SELECCIONAR";
                ObtenerListaSolicitud(0, 0, $("#txtFechaConsulta1").val(), $("#txtFechaConsulta2").val(), 1, "");
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

function RecorreJSONTableSelect(json, idSeleccionado) {
    var info = "";
    //
    // Despliegue de titulos de cabecera
    //
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "--ACCIONES--" + thFinal;
    info = info + thInicial + "ESTADO SOLICITUD" + thFinal;
    info = info + thInicial + "CODIGO" + thFinal;
    info = info + thInicial + "TIPO SOLICITUD" + thFinal;
    info = info + thInicial + "FECHA REGISTRO" + thFinal;
    info = info + thInicial + "CEDULA" + thFinal;
    info = info + thInicial + "COLABORADOR" + thFinal;
    info = info + thInicial + "DEPARTAMENTO" + thFinal;
    info = info + thInicial + "JEFE INMEDIATO" + thFinal;
    info = info + thInicial + "FECHA DESDE" + thFinal;
    info = info + thInicial + "FECHA HASTA" + thFinal;
    info = info + thInicial + "HORAS" + thFinal;
    info = info + thInicial + "ACTIVIDAD" + thFinal;
    info = info + thInicial + "DIAS" + thFinal;
    info = info + thInicial + "FERIADO" + thFinal;
    info = info + thInicial + "TOTAL DIAS" + thFinal;
    info = info + thInicial + "SALDO DIAS" + thFinal;
    info = info + thInicial + "MOTIVO RECHAZO" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";
    var totalValor1 = 0;
    var totalValor2 = 0;
    var totalValor3 = 0;
    var esImpar = true;
    $.each(json, function (i, item) {

        info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.IdPedido + "'>";

        info = info + "<td class='sorting_1' style='text-align:center'>";
        info = info + "<button type='button' value='Actualizar' title='Imprimir Solicitud' class='btn btn-btn-editClientes btn-xs' onclick='VerListadoArchivosVacaciones(\"#MensajeInformativo\", \"" + item.IdVacaciones + "\");' ><i class='fa fa-print' aria-hidden='true'></i></button>";
        info = info + "&nbsp;|&nbsp;";
        info = info + "<button type='button' value='Actualizar' title='Anular solicitud' class='btn btn-btn-editClientes btn-xs' onclick='AprobarSolicitud(\"" + item.IdVacaciones + "\",\"" + item.EstadoSolicitud + "\",\"" + item.Descripcion + "\");' ><i class='fa fa-tasks' aria-hidden='true'></i></button>";
        info = info + "&nbsp;|&nbsp;";
        info = info + "<button type='button' value='Actualizar' title='Editar solicitud' class='btn btn-btn-editClientes btn-xs' onclick='EditarSolicitud(\"" + item.IdVacaciones + "\",\"" + item.Descripcion + "\");' ><i class='fa fa-pencil-square-o' aria-hidden='true'></i></button>";
        info = info + "&nbsp;|&nbsp;";
        if (item.conteoArchivosAdjuntos != '0') {
            info = info + "<button type=\"button\" class=\"btn btn-info btn-circle\" style='cursor: pointer' onclick='VerListadoArchivosSolicitud(\"#MensajeInformativo\", \"" + item.IdVacaciones + "\");'>" + item.conteoArchivosAdjuntos + "&nbsp;<i class='fa fa-folder-open-o'></i></button>";
        }
        info = info + "</td>";
        info = info + "<td class='sorting_1'>" + item.EstadoSolicitud + "</td>";
        info = info + "<td class='sorting_1'>" + item.IdVacaciones + "</td>";
        info = info + "<td class='sorting_1'>" + item.Descripcion + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaRegistro + "</td>";
        info = info + "<td class='sorting_1'>" + item.Cedula + "</td>";
        info = info + "<td class='sorting_1'>" + item.Colaborador + "</td>";
        info = info + "<td class='sorting_1'>" + item.Departamento + "</td>";
        info = info + "<td class='sorting_1'>" + item.JefeInmediato + "</td>";

        info = info + "<td class='sorting_1'>" + item.FechaDesde + "</td>";
        info = info + "<td class='sorting_1'>" + item.FechaHasta + "</td>";
        info = info + "<td class='sorting_1'>" + item.Horas + "</td>";
        info = info + "<td class='sorting_1'>" + item.Actividad + "</td>";
        info = info + "<td class='sorting_1'>" + item.TotalDias.toFixed(2) + "</td>";
        info = info + "<td class='sorting_1'>" + item.Feriado.toFixed(2) + "</td>";
        info = info + "<td class='sorting_1'>" + format_two_digits((item.Feriado + item.TotalDias).toFixed(2)) + "</td>";
        info = info + "<td class='sorting_1'>" + item.SaldoDias + "</td>";
        info = info + "<td class='sorting_1'>" + item.MotivoAnulacion + "</td>";

    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function RecorreJSONTableSelectSaldos(json, idSeleccionado) {
    var info = "";
    //
    // Despliegue de titulos de cabecera
    //
    var totalValor3 = 0;
    var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
    var thFinal = "</th>";
    info = info + "<table width='70%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 70%;'>";
    info = info + "<thead><tr role='row'>";
    info = info + thInicial + "PERIODO" + thFinal;
    info = info + thInicial + "DIAS GENERADOS" + thFinal;
    info = info + thInicial + "DIAS TOMADOS" + thFinal;
    info = info + thInicial + "SALDO" + thFinal;
    info = info + "</tr></thead>";

    info = info + "<tbody>";

    var esImpar = true;
    $.each(json, function (i, item) {

        info = info + "<tr class='gradeA odd' role = 'row' id='tr-" + item.IdSaldos + "'>";
        info = info + "<td class='sorting_1'>" + item.PERIODO + "</td>";
        info = info + "<td class='sorting_1'>" + item.DIASGENERADOS + "</td>";
        info = info + "<td class='sorting_1'>" + item.DIASTOMADOS + "</td>";
        info = info + "<td class='sorting_1' style='text-align: right;'>" + item.SALDO + "</td>";
        info = info + "</tr>";
        totalValor3 = totalValor3 + parseFloat(item.SALDO);
    });

    if (totalValor3 > 0) {

        $('#frmTxtTiempoDiasV').val(parseFloat($('#frmTxtTiempoDiasV').val()) - parseFloat($('#frmTxtTiempoF').val()));
        totalValor3 = parseFloat(totalValor3) - (parseFloat($('#frmTxtTiempoDiasV').val()) + parseFloat($('#frmTxtTiempoF').val()));
        totalValor3 = parseFloat(totalValor3) + (parseFloat($('#frmTxtTiempoF').val()));
        info = info + "<tr>";
        info = info + "<td class='sorting_1' colspan='1' style='text-align: right;'><b></b></td>";
        info = info + "<td class='sorting_1' colspan='1' style='text-align: right;'><b></b></td>";
        info = info + "<td class='sorting_1' colspan='1' style='text-align: left;'>TOTAL: </td>";
        info = info + "<td class='sorting_1' style='text-align: right;'><b>" + "" + format_two_digits((totalValor3).toFixed(2)) + "</b></td>";
        info = info + "</tr>";
        SaldoDias = totalValor3;
    }

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

function BuscarSolicitudEditar(seleccionarTipo) {

    var selectedSolicitud = seleccionarTipo;
    if (selectedSolicitud == "PERMISO") {
        BtnPermiso();
        EstadoSolicitud = selectedSolicitud;
        ObtenerDatosEmpleado(1, 1);
        document.getElementById("btnGuardar2").innerHTML = "Actualizar";
    }
    else if (selectedSolicitud == "VACACIONES") {
        BtnVacaciones();
        EstadoSolicitud = selectedSolicitud;
        ObtenerDatosEmpleado(1, 2);
        ObtenerListaComboReemplazo();
        ObtenerListaSaldoVacaciones(0);
        ObtenerListaDiasSolicitados(1);
        document.getElementById("fomTitleLabel").innerHTML = "Convenio de vacaciones";
        document.getElementById("IdFeriado").style.display = "block";

        //document.getElementById("Vacaciones").style.display = "block";
        //document.getElementById("Planificacion").style.display = "none";

        document.getElementById("btnGuardar1").innerHTML = "Actualizar";
    }
    else if (selectedSolicitud == "PLANIFICAR VACACIONES") {
        BtnVacaciones();
        EstadoSolicitud = selectedSolicitud;
        ObtenerDatosEmpleado(1, 2);
        ObtenerListaComboReemplazo();
        ObtenerListaSaldoVacaciones(0);
        ObtenerListaDiasSolicitados(1);
        document.getElementById("fomTitleLabel").innerHTML = "Planificación de vacaciones";
        document.getElementById("IdFeriado").style.display = "none";

        //document.getElementById("Vacaciones").style.display = "none";
        //document.getElementById("Planificacion").style.display = "block";

        document.getElementById("btnGuardar1").innerHTML = "Actualizar";

        var contenido = "";
        var div = "datosTablaPrincipal3";
        contenido = RecorreJSONTableMetas();
        $(div).html(contenido);
    }
}

function BuscarSolicitud() {
    var combooSolicitud = document.getElementById("cboSolicitud");
    var selectedSolicitud = combooSolicitud.options[combooSolicitud.selectedIndex].text;
    if (selectedSolicitud == "PERMISO") {
        BtnPermiso();
        EstadoSolicitud = selectedSolicitud;
        ObtenerDatosEmpleado(1, 1);
        document.getElementById("btnGuardar2").innerHTML = "Solicitar";
        document.getElementById("IdCargarArchivo").style.display = "none";
        //BorrarBotones(1);
    }
    else if (selectedSolicitud == "VACACIONES") {
        BtnVacaciones();
        EstadoSolicitud = selectedSolicitud;
        ObtenerDatosEmpleado(1, 2);
        ObtenerListaComboReemplazo();
        ObtenerListaSaldoVacaciones(0);
        ObtenerListaDiasSolicitados(1);
        document.getElementById("fomTitleLabel").innerHTML = "Convenio de vacaciones";
        document.getElementById("IdFeriado").style.display = "block";

        //document.getElementById("Vacaciones").style.display = "block";
        //document.getElementById("Planificacion").style.display = "none";
        document.getElementById("btnGuardar1").innerHTML = "Solicitar";
        //BorrarBotones(0);
    }
    else if (selectedSolicitud == "PLANIFICAR VACACIONES") {
        BtnVacaciones();
        EstadoSolicitud = selectedSolicitud;
        ObtenerDatosEmpleado(1, 2);
        ObtenerListaComboReemplazo();
        ObtenerListaSaldoVacaciones(0);
        ObtenerListaDiasSolicitados(1);
        document.getElementById("fomTitleLabel").innerHTML = "Planificación de vacaciones";
        document.getElementById("IdFeriado").style.display = "none";

        //document.getElementById("Vacaciones").style.display = "none";
        //document.getElementById("Planificacion").style.display = "block";

        document.getElementById("btnGuardar1").innerHTML = "Solicitar";

        var contenido = "";
        var div = "datosTablaPrincipal3";
        contenido = RecorreJSONTableMetas();
        $(div).html(contenido);
        //BorrarBotones(0);
    }
}

function RecorreJSONTableMetas() {
    var info = "";

    var esImpar = true;
    var iniciaBarrido = true;
    var totalValor3 = 0;

            //
            // Despliegue de titulos de cabecera
            //
            var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;text-align:center;'>";
            var thFinal = "</th>";
            info = info + "<table id='tbl_metas' width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
            info = info + "<thead><tr role='row'>";
            info = info + thInicial + "Fecha Desde" + thFinal;
            info = info + thInicial + "Fecha Hasta" + thFinal;
            info = info + thInicial + "Dias" + thFinal;
            info = info + thInicial + "---ACCIONES---" + thFinal;
            info = info + "</tr></thead>";
            info = info + "<tbody>";

        //
        // Despliegue de datos
        // 
        info = info + "<td class='sorting_1'>" + 1 + "</td>";
        info = info + "<td class='sorting_1' style='text-align: center;'><input type='text' class='form-control' id='frmTxtHoraDesdeV1'/></td>";
        info = info + "<td class='sorting_1' style='text-align: right;'><input type='text' class='form-control' id='frmTxtHoraHastaV1'/></td>";
        info = info + "<td class='sorting_1' style='text-align: right;'><input type='text' class='form-control' id='frmTxtTiempoDiasV1'/></td>";
        info = info + "<td class='sorting_1' style='text-align:center'>";
        info = info + "<i class='fa fa-plus' style='cursor: pointer' onclick='Agregar();' title'Agregar mas fechas'></i>";
        //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        //info = info + "<a href='#' class='btn-eliminaDetalle' title='Eliminar meta' data-id=''><i class='fa fa-trash-o'></i></a>";
        info = info + "</td>";
        info = info + "</tr>";


    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function SacarDias(f1, f2) {
    var aFecha1 = f1.split('/');
    var aFecha2 = f2.split('/');
    var fFecha1 = Date.UTC(aFecha1[2], aFecha1[1] - 1, aFecha1[0]);
    var fFecha2 = Date.UTC(aFecha2[2], aFecha2[1] - 1, aFecha2[0]);
    var dif = fFecha2 - fFecha1;
    var dias = Math.floor(dif / (1000 * 60 * 60 * 24));
    return parseFloat(dias)+1;
}

function DiasVacaciones() {
    $('#frmTxtTiempoDiasV').val(SacarDias($('#frmTxtHoraDesdeV').val(), $('#frmTxtHoraHastaV').val()));
    ObtenerListaSaldoVacaciones(0);
}

function BorrarBotones(idTipo) {

    SaldoDias = 0;
    if (idTipo == 0) {
        $('#txtCedulaV').val("");
        $('#txtNombreColaboradorV').val("");
        $('#txtDepartamentoV').val("");
        $('#txtJefaAreaV').val("");
        $('#txtActividadP').val("");
        //$('#frmTxtHoraDesdeV').val("");
        //$('#frmTxtHoraHastaV').val("");
        $("#frmTxtHoraDesdeV").datepicker('setDate', 'today');
        $("#frmTxtHoraHastaV").datepicker('setDate', 'today');
        $('#frmTxtTiempoF').val("0");
        $('#frmTxtTiempoDiasV').val("0");
        CancelarCambios1();
    }
    else if (idTipo == 1) {
        $('#txtCedulaP').val("");
        $('#txtNombreColaboradorP').val("");
        $('#txtDepartamentoP').val("");
        $('#txtJefaAreaP').val("");
        $('#txtActividadP').val("");
        //$('#frmTxtHoraDesdeP').val("");
        //$('#frmTxtHoraHastaP').val("");
        $("#frmTxtHoraDesdeP").datepicker('setDate', 'today');
        $("#frmTxtHoraHastaP").datepicker('setDate', 'today');
        $('#frmTxtTiempoP').val("0");
        $('#frmTxtObservacionesP').val("");
        CancelarCambios2();
    }
    else if (idTipo == 2) {
        let prueba = 0;
    }
}

function ObtenerDatosEmpleado(tipo,id) {
    TipoSolicitud = id;
    var Datos = "[{ \"action\": \"ReporteDatosEmpleado\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", tipo: \"" + tipo + "\" } }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "tableSelectPantalla", tipo);
}

function RecorreDatosPantalla(json) {

    $.each(json, function (i, item) {
        if (TipoSolicitud == 1) {
            $('#txtCedulaP').val(item.Cedula);
            $('#txtNombreColaboradorP').val(item.Nom_Usuario);
            $('#txtDepartamentoP').val(item.Departamento);
            $('#txtJefaAreaP').val(item.JefeInmediato);
        }
        else if (TipoSolicitud == 2){
            $('#txtCedulaV').val(item.Cedula);
            $('#txtNombreColaboradorV').val(item.Nom_Usuario);
            $('#txtDepartamentoV').val(item.Departamento);
            $('#txtJefaAreaV').val(item.JefeInmediato);
        }
    });
}

function RecorreDatosSolicitud(json) {
   
    if (json.Descripcion == "PERMISO") {
        $('#txtActividadP').val(json.Actividad);
        $('#frmTxtHoraDesdeP').val(json.FechaDesde);
        $('#frmTxtHoraHastaP').val(json.FechaHasta);
        $('#frmTxtTiempoP').val(json.Horas);
        $('#frmTxtObservacionesP').val(json.Observacion);
        if (json.StrCargoVacaciones == "SI") {
            document.getElementById("IdSI").checked = true
            document.getElementById("IdNO").checked = false
        }
        else if (json.StrCargoVacaciones == "NO") {
            document.getElementById("IdSI").checked = false
            document.getElementById("IdNO").checked = true
        }
        var combo3 = document.getElementById("cboSolicitud");
        combo3.value = json.IdTipoSolicitud;
    }
    else if (json.Descripcion == "VACACIONES") {
        //$('#txtReemplazo').val(json.Remplazo);
        //var combo2 = document.getElementById("txtReemplazo");
        //combo2.value = json.Remplazo;
        $('#frmTxtHoraDesdeV').val(json.FechaDesde);
        $('#frmTxtHoraHastaV').val(json.FechaHasta);
        $('#frmTxtTiempoF').val(json.Feriado);
        $('#frmTxtTiempoDiasV').val(json.TotalDias);
        var combo3 = document.getElementById("cboSolicitud");
        combo3.value = json.IdTipoSolicitud;
    }
    else if (json.Descripcion == "PLANIFICAR VACACIONES") {
        //$('#txtReemplazo').val(json.Remplazo);
        //var combo2 = document.getElementById("txtReemplazo");
        //combo2.value = json.Remplazo;
        $('#frmTxtHoraDesdeV').val(json.FechaDesde);
        $('#frmTxtHoraHastaV').val(json.FechaHasta);
        $('#frmTxtTiempoDiasV').val(json.TotalDias);
        var combo3 = document.getElementById("cboSolicitud");
        combo3.value = json.IdTipoSolicitud;
    }
}

function RecorreDatosDiasSolicitados(json) {

    $.each(json, function (i, item) {
        let proceso2 = item.TOTAL;
        if (proceso2 != 0) {
            document.getElementById('Idlista1').innerHTML = "Días solicitados desde el aplicativo pendiente de registrar en SAP: "
            document.getElementById('Idlista2').innerHTML = proceso2.toFixed(2);
        }
    });
}

function ObtenerListaSolicitud(tipo, idRegistro, fechaInicio, fechaFinal, pagina, estadosolicitud) {

    var Datos = "[{ \"action\": \"ReporteListaSolicitud\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", busqueda: \"" + "0" + "\", tipo: \"" + tipo + "\", pagina: \"" + pagina + "\", estadosolicitud: \"" + estadosolicitud + "\", usuario: \"" + "0" + "\" } }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "tableSelect", tipo);
}

function ObtenerSolicitudIndividual(idvaluar) {

    var Datos = "[{ \"action\": \"SolicitudIndividual\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", IdVacaciones: \"" + idvaluar + "\" } }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "detalleDatos", 0);
}

function ObtenerListaSolicitudDescarga(tipo, idRegistro, fechaInicio, fechaFinal, pagina, estadosolicitud) {

    var Datos = "[{ \"action\": \"ListaSolicitudDescargaXLS\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", fechaDesde: \"" + fechaInicio + "\", fechaHasta: \"" + fechaFinal + "\", busqueda: \"" + "0" + "\", tipo: \"" + tipo + "\", pagina: \"" + pagina + "\", estadosolicitud: \"" + estadosolicitud + "\" } }]";
    idCliente2 = 0;
    DetalleTareasDescargaXLS('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table");
}

function ObtenerListaSaldoVacaciones(tipo) {

    var Datos = "[{ \"action\": \"ReporteListaSaldoVacaciones\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", tipo: \"" + tipo + "\" } }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectSaldo", tipo);
}

function ObtenerListaDiasSolicitados(tipo) {

    var Datos = "[{ \"action\": \"ReporteListaSaldoVacaciones\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", tipo: \"" + tipo + "\" } }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectDias", tipo);
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

function BtnConsulta() {
    $("#divMensajes").html("");

    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaConsulta1").val();
        fechaFinal = $("#txtFechaConsulta2").val();
    }

    ObtenerListaSolicitud(0, 0, fechaInicio, fechaFinal,1,"");
}

function BtnDescarga() {
    $("#divMensajes").html("");
    var fechaInicio = "";
    var fechaFinal = "";

    var text = document.getElementById("idFiltros");
    if (text.checked == true) {
        fechaInicio = "";
        fechaFinal = "";
    } else {
        fechaInicio = $("#txtFechaConsulta1").val();
        fechaFinal = $("#txtFechaConsulta2").val();
    }

    ObtenerListaSolicitudDescarga(0, 0, fechaInicio, fechaFinal, 1, "");

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

function GuardarSolicitud1() {
    if (document.getElementById('btnGuardar1').innerHTML == "Solicitar") {
        GuardarSolicitudVacaciones(0);
    }
    else if (document.getElementById('btnGuardar1').innerHTML == "Actualizar")
    {
        GuardarSolicitudVacaciones(1);
    }
    else if (document.getElementById('btnGuardar1').innerHTML == "Eliminar") {
        GuardarSolicitudVacaciones(4);
    }
}

function GuardarSolicitud2() {
    if (document.getElementById('btnGuardar2').innerHTML == "Solicitar") {
        GuardarSolicitudPermiso(0);
    }
    else if (document.getElementById('btnGuardar2').innerHTML == "Actualizar") {
        GuardarSolicitudPermiso(1);
    }
    else if (document.getElementById('btnGuardar2').innerHTML == "Eliminar") {
        GuardarSolicitudPermiso(4);
    }
}

function GuardarSolicitudPermiso(tipo) {

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;
    var ActividadArealizar = "";

    var proceso = 0;
    var text1 = document.getElementById("IdSI");
    var text2 = document.getElementById("IdNO");
    if (text1.checked == true) {
        proceso =1;
    }
    else if(text1.checked == true) { 
        proceso =0;
    }


    if ($('#txtfechaP').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha ";
        contadorVerificacion += 1;
    }

    if ($('#txtCedulaP').val() == "") {
        mensajeVerificacion += "- Debe ingresar la identificación ";
        contadorVerificacion += 1;
    }

    if ($('#txtNombreColaboradorP').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre del Colaborador ";
        contadorVerificacion += 1;
    }

    if ($('#txtDepartamentoP').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre del departamento ";
        contadorVerificacion += 1;
    }

    if ($('#txtJefaAreaP').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre de su jefe inmediato ";
        contadorVerificacion += 1;
    }

    if ($('#txtActividadP').val() == "" && $('#txtActividadPC').val() =="OTROS") {
        mensajeVerificacion += "- Debe ingresar actividad a realizar ";
        contadorVerificacion += 1;
    }

    if ($('#frmTxtHoraDesdeP').val() == "") {
        mensajeVerificacion += "- Debe ingresar las horas de permiso ";
        contadorVerificacion += 1;
    }

    if ($('#frmTxtHoraHastaP').val() == "") {
        mensajeVerificacion += "- Debe ingresar las horas de permiso ";
        contadorVerificacion += 1;
    }

    if ($('#frmTxtTiempoP').val() == "") {
        mensajeVerificacion += "- Debe ingresar las horas de permiso ";
        contadorVerificacion += 1;
    }

    if ($('#frmTxtObservacionesP').val() == "") {
        mensajeVerificacion += "- Debe ingresar la observación ";
        contadorVerificacion += 1;
    }

    if ($('#txtActividadP').val() != "" && $('#txtActividadPC').val() == "OTROS") {
        ActividadArealizar = $('#txtActividadP').val();
    }
    else {
        ActividadArealizar = $('#txtActividadPC').val();
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'IdVacaciones': '" + idVacaciones + "',";
    datosFormulario = datosFormulario + "'IdTipoSolicitud': '" + $('#cboSolicitud').val()  + "',";
    datosFormulario = datosFormulario + "'txtfechaP': '" + $('#txtfechaP').val() + "',";
    datosFormulario = datosFormulario + "'txtCedulaP': '" + $('#txtCedulaP').val() + "',";
    datosFormulario = datosFormulario + "'txtNombreColaboradorP': '" + $('#txtNombreColaboradorP').val() + "',";
    datosFormulario = datosFormulario + "'txtDepartamentoP': '" + $('#txtDepartamentoP').val() + "',";
    datosFormulario = datosFormulario + "'txtJefaAreaP': '" + $('#txtJefaAreaP').val() + "',";
    datosFormulario = datosFormulario + "'txtActividadP': '" + ActividadArealizar + "',";
    datosFormulario = datosFormulario + "'frmTxtHoraDesdeP': '" + $('#frmTxtHoraDesdeP').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtHoraHastaP': '" + $('#frmTxtHoraHastaP').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtTiempoP': '" + $('#frmTxtTiempoP').val() + "',";
    datosFormulario = datosFormulario + "'IdSI': '" + proceso + "',";
    datosFormulario = datosFormulario + "'IdNO': '" + $('#IdNO').val() + "',";
    datosFormulario = datosFormulario + "'EstadoSolicitud': '" + EstadoSolicitud + "',";
    datosFormulario = datosFormulario + "'tipo': '" + tipo + "',";
    datosFormulario = datosFormulario + "'frmTxtObservacionesP': '" + $('#frmTxtObservacionesP').val() + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevoPermiso', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotones(1);
                ObtenerListaSolicitud(0, 0, $("#txtFechaConsulta1").val(), $("#txtFechaConsulta2").val(), 1, "");
                idVacaciones = 0;

                if (respuesta.resultado != 0 && window.FormData !== undefined) {
                    var url = 'CargaArchivos.ashx';
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
                    fileData.append('Id_RegTareas', respuesta.resultado);
                    fileData.append('idServicio', "6");

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
                                ListadoArchivosSolicitud("#divArchivosAdjuntosAnteriores", respuesta.resultado);
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

                }
                else {
                    MensajeCorrecto(respuesta.mensaje);
                    $("#divMensajes").html("");
                }
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

function VerListadoArchivosSolicitud(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + idTarea + "',";
    datosParametros = datosParametros + "'IdServicio': '" + "6" + "'";
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

function ListadoArchivosSolicitud(div, idTarea) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + idTarea + "',";
    datosParametros = datosParametros + "'IdServicio': '" + "6" + "'";
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

    datos = "[{'action': 'BorrarArchivosContrato', 'parameters' : " + datosParametros + " }]";

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

function GuardarSolicitudVacaciones(tipo) {

    var url = "ObtenerListaTareas.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;



    if ($('#txtfechaV').val() == "") {
        mensajeVerificacion += "- Debe ingresar la fecha ";
        contadorVerificacion += 1;
    }

    if ($('#txtCedulaV').val() == "") {
        mensajeVerificacion += "- Debe ingresar la identificación ";
        contadorVerificacion += 1;
    }

    if ($('#txtNombreColaboradorV').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre del Colaborador ";
        contadorVerificacion += 1;
    }

    if ($('#txtDepartamentoV').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre del departamento ";
        contadorVerificacion += 1;
    }

    if ($('#txtJefaAreaV').val() == "") {
        mensajeVerificacion += "- Debe ingresar el nombre de su jefe inmediato ";
        contadorVerificacion += 1;
    }

    if ($('#frmTxtHoraDesdeV').val() == "") {
        mensajeVerificacion += "- Debe ingresar los dias de vacaciones ";
        contadorVerificacion += 1;
    }


    if ($('#frmTxtHoraHastaV').val() == "") {
        mensajeVerificacion += "- Debe ingresar los dias de vacaciones ";
        contadorVerificacion += 1;
    }

    if ($('#txtReemplazo').val() == null) {
        mensajeVerificacion += "- Debe ingresar el reemplazo ";
        contadorVerificacion += 1;
    }


    if ($('#frmTxtTiempoDiasV').val() == "") {
        mensajeVerificacion += "- Debe ingresar los dias de vacaciones ";
        contadorVerificacion += 1;
    }

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }


    var datosFormulario = "";

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosFormulario = datosFormulario + "'IdVacaciones': '" + idVacaciones + "',";
    datosFormulario = datosFormulario + "'IdTipoSolicitud': '" + $('#cboSolicitud').val() + "',";
    datosFormulario = datosFormulario + "'txtfechaV': '" + $('#txtfechaV').val() + "',";
    datosFormulario = datosFormulario + "'txtCedulaV': '" + $('#txtCedulaV').val() + "',";
    datosFormulario = datosFormulario + "'txtNombreColaboradorV': '" + $('#txtNombreColaboradorV').val() + "',";
    datosFormulario = datosFormulario + "'txtDepartamentoV': '" + $('#txtDepartamentoV').val() + "',";
    datosFormulario = datosFormulario + "'txtJefaAreaV': '" + $('#txtJefaAreaV').val() + "',";
    datosFormulario = datosFormulario + "'txtReemplazo': '" + $('#txtReemplazo').val()  + "',";
    datosFormulario = datosFormulario + "'frmTxtHoraDesdeV': '" + $('#frmTxtHoraDesdeV').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtHoraHastaV': '" + $('#frmTxtHoraHastaV').val() + "',";
    datosFormulario = datosFormulario + "'frmTxtTiempoF': '" + $('#frmTxtTiempoF').val() + "',";
    datosFormulario = datosFormulario + "'SaldoDias': '" + SaldoDias.toString().replace(".",",") + "',";
    datosFormulario = datosFormulario + "'EstadoSolicitud': '" + EstadoSolicitud + "',";
    datosFormulario = datosFormulario + "'tipo': '" + tipo + "',";
    datosFormulario = datosFormulario + "'frmTxtTiempoDiasV': '" + $('#frmTxtTiempoDiasV').val() + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'GuardarNuevaVacaciones', 'parameters' : " + datosFormulario + " }]";

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
                BorrarBotones(0);
                ObtenerListaSolicitud(0, 0, $("#txtFechaConsulta1").val(), $("#txtFechaConsulta2").val(), 1, "");
                idVacaciones = 0;
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

function time_format(d) {
    hours = format_two_digits(d.getHours());
    minutes = format_two_digits(d.getMinutes());
    seconds = format_two_digits(d.getSeconds());
    return hours + ":" + minutes + ":" + seconds;
}

function format_two_digits(n) {
    return n < 10 ? '0' + n : n;
}

function VerListadoArchivosVacaciones(div, IdSolicitud) {

    var url = "CargaArchivos.ashx";

    var datosParametros = "";
    datosParametros = datosParametros + "{";
    datosParametros = datosParametros + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
    datosParametros = datosParametros + "'frmTxtCodigo': '" + IdSolicitud + "'";
    datosParametros = datosParametros + "}";

    datos = "[{'action': 'ListaArchivosVacaciones', 'parameters' : " + datosParametros + " }]";

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

function AgregaActividad() {
    var combooActividad = document.getElementById("txtActividadPC");
    var selectedActividad = combooActividad.options[combooActividad.selectedIndex].text;

    if (selectedActividad == "OTROS") {
        document.getElementById("IdOtrasActividad").style.display = "block";
        document.getElementById("IdOtrasActividadCargar").style.display = "none";
    }
    else {
        document.getElementById("IdOtrasActividad").style.display = "none";
        document.getElementById("IdOtrasActividadCargar").style.display = "block";
    }
}

$(function () {


    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);
    ObtenerListaComboSolicitud2();
    ObtenerListaComboSolicitud();

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

    tofrom11 = $("#txtfechaV").datepicker(
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

    tofrom12 = $("#txtfechaP").datepicker(
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

    tofrom13 = $("#frmTxtHoraDesdeV").datepicker(
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

    tofrom14 = $("#frmTxtHoraHastaV").datepicker(
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

    tofrom15 = $("#txtfechaR").datepicker(
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

    $("#txtfechaP").datepicker('setDate', 'today');
    $("#txtfechaV").datepicker('setDate', 'today');
    $("#txtfechaR").datepicker('setDate', 'today');

    $("#frmTxtHoraDesdeV").datepicker('setDate', 'today');
    $("#frmTxtHoraHastaV").datepicker('setDate', 'today');


    formFrom = $("#frmTxtFecha").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"],
        });


    formFromTime = $('#frmTxtHoraDesdeP').datetimepicker({
        format: 'HH:mm'
    }).on('dp.change', function (e) {
        var minutosInicioTotal = getMinutes(formFromTime);
        var minutosFinalTotal = getMinutes(formToTime);

        if (minutosInicioTotal > minutosFinalTotal) {
            formToTime.val(formFromTime.val());
        }

        calculeTime(formFromTime.val(), formToTime.val(), '#frmTxtTiempoP');

    });


    formToTime = $('#frmTxtHoraHastaP').datetimepicker({
        format: 'HH:mm'
    }).on('dp.change', function (e) {
        var minutosInicioTotal = getMinutes(formFromTime);
        var minutosFinalTotal = getMinutes(formToTime);

        if (minutosInicioTotal > minutosFinalTotal) {
            formFromTime.val(formToTime.val());
        }

        calculeTime(formFromTime.val(), formToTime.val(), '#frmTxtTiempoP');

    });


    formFromTime2 = $('#frmTxtHoraDesdeR').datetimepicker({
        format: 'HH:mm'
    }).on('dp.change', function (e) {
        var minutosInicioTotal = getMinutes(formFromTime);
        var minutosFinalTotal = getMinutes(formToTime);

        if (minutosInicioTotal > minutosFinalTotal) {
            formToTime2.val(formFromTime2.val());
        }

        calculeTime(formFromTime2.val(), formToTime2.val(), '#frmTxtTiempoR');

    });


    formToTime2 = $('#frmTxtHoraHastaR').datetimepicker({
        format: 'HH:mm'
    }).on('dp.change', function (e) {
        var minutosInicioTotal = getMinutes(formFromTime);
        var minutosFinalTotal = getMinutes(formToTime);

        if (minutosInicioTotal > minutosFinalTotal) {
            formFromTime2.val(formToTime2.val());
        }

        calculeTime(formFromTime2.val(), formToTime2.val(), '#frmTxtTiempoR');

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

        if (idVacaciones == 0) {

            alerta("Para cargar un archivo debe cargar una solicitud de permisos...");
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
                fileData.append('Id_RegTareas', idVacaciones);
                fileData.append('idServicio', "6");

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
                            ListadoArchivosCosteo("#divArchivosAdjuntosAnteriores", idVacaciones);
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


$(document).ready(function () {

    $('.js-example-basic-multiple').select2({
        width: '500px'
    });
});