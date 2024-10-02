function Create() {
    if ($.fn.DataTable.isDataTable('#table-users')) {
        $('#table-users').DataTable().destroy();
    }
    $('#table-users tbody').empty();
    //Here call the Datatable Bind function;
}

function BtnConsultaForeCast() {
    ObtenerConsultarForeCastDetalleFiltros(0);
}

function BtnDescargaForeCast() {
    ObtenerConsultarForeCastDetalleFiltrosDescargar(0);
}

function ObtenerConsultarForeCastDetalleFiltros(tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarReporteVentas\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\", anio: \"" + $("#cboAnio1").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipalForeCast', 'ObtenerListaTareas.ashx', Datos, "tableSelectForeCast", tipo);
}


function ObtenerConsultarForeCastDetalleFiltrosDescargar(tipo) {
    var Datos = "[{ \"action\": \"ReporteConsultarReporteVentasDescargar\", \"parameters\" : { session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",anio: \"" + $("#cboAnio1").val() + "\"} }]";
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
    if (tipoControl == "tableSelectForeCast") {
        contenido = RecorreJSONTableSelectForeCast(json, boton, idSeleccionado);
        //dtUsers(json);
        $(div).html(contenido);
    }
    return contenido;
}

function RecorreJSONTableSelectForeCast(json, boton, idSeleccionado) {
    var info = "";

    if (json.length == 0) {
        $('#txtTotalOportunidad').val("0.00");
        $('#txtSubtotal').val("0.00");
        $('#txttotal').val("0.00");
        alerta("No existe un Forecast registrado");
    }
    else {
        var esImpar = true;
        var iniciaBarrido = true;
        var totalValor3 = 0;

        var totalValor1 = 0;
        var totalValor2 = 0;

        $('#txtTotalOportunidad').val(json.length.toFixed(2));

        //
        // Despliegue de titulos de cabecera
        //
        var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 200px;text-align:center;'>";
        var thFinal = "</th>";
        info = info + "<table id='tbl_forecast'  width='100%' class='table table-striped table-bordered table-hover dataTable' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
        info = info + "<thead><tr role='row'>";
        info = info + thInicial + "SUCURSAL" + thFinal;
        info = info + thInicial + "CUOTA ANUAL" + thFinal;
        info = info + thInicial + "A LA FECHA" + thFinal;
        info = info + thInicial + "% CUMPLIMIENTO A LA FECHA" + thFinal;
        info = info + thInicial + "POR FACTURAR + RECURRENTE (BACKLOG)" + thFinal;
        info = info + thInicial + "PROYECCIÓN TOTAL ANUAL" + thFinal;
        info = info + thInicial + "PROYECCIÓN ANUAL %" + thFinal;
        info = info + thInicial + "POR CERRAR FORECAST ONLINE" + thFinal;
        info = info + thInicial + "PROYECCION FINAL TOTAL" + thFinal;
        info = info + thInicial + "ESTIMADA 25% MARGEN" + thFinal;
        info = info + thInicial + "A LA FECHA" + thFinal;
        info = info + thInicial + "% CUMPLIMIENTO A LA FECHA" + thFinal;
        info = info + thInicial + "POR FACTURAR + RECURRENTE (BACKLOG)" + thFinal;
        info = info + thInicial + "PROYECCIÓN TOTAL ANUAL" + thFinal;
        info = info + thInicial + "PROYECCIÓN CIERRE" + thFinal;
        info = info + thInicial + "POR CERRAR FORECAST ONLINE" + thFinal;
        info = info + thInicial + "PROYECCIÓN TOTAL" + thFinal;
        info = info + thInicial + "PROYECCION ANUAL" + thFinal;
        info = info + "</tr></thead>";

        info = info + "<tbody>";

        $.each(json, function (i, item) {
            //
            // Despliegue de datos
            // 

            info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.SUCURSAL + "</td>";

            if (item.CUOTAANUAL == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                if (item.SUCURSAL == "MARGEN") {
                    info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.CUOTAANUAL + " % </td>";
                }
                else {
                    info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + item.CUOTAANUAL + "</td>";
                }
            }

            if (item.PORFACTURAR == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>"  +""+ "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.PORFACTURAR) + "</td>";
            }

            if (item.CUMPLIALAFECHA == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + item.CUMPLIALAFECHA + " % </td>";
            }

            if (item.PROYECIONTOANUAL == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.PROYECIONTOANUAL) + "</td>";
            }

            if (item.PROYECIONPORCANUAL == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + (item.PROYECIONPORCANUAL) + " % </td>";
            }

            if (item.PORCERRARFORECAST == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.PORCERRARFORECAST) + "</td>";
            }

            if (item.PROYECIONTOANUALFORECAST == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.PROYECIONTOANUALFORECAST) + "</td>";
            }

            if (item.PROYECIONPORCANUALFORECAST == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + (item.PROYECIONPORCANUALFORECAST) + " % </td>";
            }

            if (item.ESTIMADA25MARGE == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.ESTIMADA25MARGE) + "</td>";
            }

            if (item.ESTIMADALAFECHA == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.ESTIMADALAFECHA) + "</td>";
            }

            if (item.ESTIMADAPORCALAFECHA == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + (item.ESTIMADAPORCALAFECHA) + " % </td>";
            }

            if (item.VALORFACTURADO == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.VALORFACTURADO) + "</td>";
            }

            if (item.VALORPROYECCIONANUAL == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.VALORPROYECCIONANUAL) + "</td>";
            }

            if (item.VALORPORCPROYECIERRE == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + (item.VALORPORCPROYECIERRE) + " % </td>";
            }

            if (item.PORCERRARFORECASTPVP == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {

                if (item.SUCURSAL == "MARGEN") {
                    info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + (item.PORCERRARFORECASTPVP) + " % </td>";
                }
                else {
                    info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.PORCERRARFORECASTPVP) + "</td>";
                }
            }

            if (item.PROYECIONTOANUALFORECASTPVP == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "$ " + (item.PROYECIONTOANUALFORECASTPVP) + "</td>";
            }

            if (item.PROYECIONPORCANUALFORECASTPVP == 0) {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + "" + "</td>";
            }
            else {
                info = info + "<td class='sorting_1' style='color:#060302; font-size:11px;'>" + (item.PROYECIONPORCANUALFORECASTPVP) + " % </td>";
            }

            info = info + "</tr>";
        });

        info = info + "</tbody>";
        info = info + "</table>";

    }
    return info;
}

function dtUsers(json) {
    Create();
    table = null;

    table = $('#table-users').DataTable({
        data: json,
        columns: [
            { defaultContent: "" },
            { data: 'SUCURSAL' },
            { data: 'CUOTAANUAL' },
            { data: 'VALORALAFECHA' },
            { data: 'CUMPLIALAFECHA' },
            { data: 'PORFACTURAR' },
            { data: 'PROYECIONTOANUAL' },
            { data: 'PROYECIONPORCANUAL' },
            { data: 'PORCERRARFORECAST' },
            { data: 'PROYECIONTOANUALFORECAST' },
            { data: 'PROYECIONPORCANUALFORECAST' },
            { data: 'ESTIMADA25MARGE' },
            { data: 'ESTIMADALAFECHA' },
            { data: 'ESTIMADAPORCALAFECHA' },
            { data: 'VALORFACTURADO' },
            { data: 'VALORPROYECCIONANUAL' },
            { data: 'VALORPORCPROYECIERRE' },
            { data: 'PORCERRARFORECASTPVP' },
            { data: 'PROYECIONTOANUALFORECASTPVP' },
            { data: 'PROYECIONPORCANUALFORECASTPVP' }
        ],

        language: {
            "decimal": ",",
            "thousands": ".",
            "emptyTable": "No hay información",
            "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
            "infoEmpty": "Mostrando 0 to 0 of 0 Entradas",
            "infoFiltered": "(Filtrado de _MAX_ total entradas)",
            "infoPostFix": "",
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ Entradas",
            "loadingRecords": "Cargando...",
            "processing": "Procesando...",
            "search": "Buscar:",
            "zeroRecords": "Sin resultados encontrados",
            "paginate": {
                "first": "Primero",
                "last": "Ultimo",
                "next": "Siguiente",
                "previous": "Anterior"
            },
            //"oAria": {
            //    "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
            //    "sSortDescending": ": Activar para ordenar la columna de manera descendente"
            //}
        },
        orderCellsTop: true,
        fixedHeader: true
    });

    //$('#table-users').on('click', 'button', function () {
    //    var data = table.row($(this).parents('tr')).data();
    //    //alert(data.IdForeCast);
    //    CargarIdForeCastLista(data.IdForeCast);
    //});
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