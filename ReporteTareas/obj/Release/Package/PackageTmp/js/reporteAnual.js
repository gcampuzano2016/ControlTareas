/* Información de Nuestro Framework */

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

function ObtenerListaComboAnioCuotas() {
    var Datos = "[{ \"action\": \"ListaComboContrato\", \"parameters\" : { \"Tipo\" : \"22\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#cboAnio', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

function ObtenerListaCoutasAnual() {

    var combocboAnio = document.getElementById("cboAnio");
    var selectedcombocboAnio = combocboAnio.options[combocboAnio.selectedIndex].text;

    var Datos = "[{ \"action\": \"ListaDatosCoutasAnual\", \"parameters\" : {  \"anio\" : \"" + selectedcombocboAnio + "\",\"tipo\" : \"" + $("#cboArea").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table", "", "", "#divListadoDetalleTareasDiarias");
}

function ObtenerMetasAnual() {

    var Datos = "[{ \"action\": \"ListaDatosCoutasAnual\", \"parameters\" : {  \"anio\" : \"" + "3" + "\",\"tipo\" : \"" + "3" + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableMetas", "", "", "#divListadoDetalleTareasDiarias");
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
    if (tipoControl == "tableMetas") {
        contenido = RecorreJSONTableMetas(json);
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
            info = info + "<table id='tbl_cuotas' width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
            info = info + "<thead><tr role='row'>";
            info = info + thInicial + "CODIGO" + thFinal;
            info = info + thInicial + "NOMBRE" + thFinal;
            info = info + thInicial + "CUOTA ANUAL" + thFinal;
            info = info + thInicial + "CUOTA TRIMESTRAL" + thFinal;
            info = info + thInicial + "SUCURSAL" + thFinal;
            info = info + thInicial + "ESTADO" + thFinal;
            info = info + thInicial + "---ACCIONES---" + thFinal;
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
        info = info + "<td class='sorting_1'>" + item.IdCuotaAnual + "-" + item.ID + "</td>";
        //info = info + "<td class='sorting_1' style='text-align: left;'>" + item.Nombres + "</td>";
        info = info + "<td class='text-left'><input type='text' class='form-control' id='txtNombre' value=" + item.Nombres + " /></td>";
        info = info + "<td class='sorting_1' style='text-align: right;'><input type='text' class='form-control' id='txtDescrip'  value=" + item.CuotaAnual + " /></td>";
        totalValor3 = 0;

        if (item.CuotaAnual > 0) {
            totalValor3 = totalValor3 + parseFloat(item.CuotaAnual) / 4;
        }

        info = info + "<td class='sorting_1' style='text-align: right;'>" + "$ " + format_two_digits((totalValor3).toFixed(2)) + "</td>";
        if (item.IdSucursal == 1) {

            info = info + "<td class='text-center'><select id='cboSucursalTabla' class='form-control' name='cboSucursalTabla'><option value='3'>QUITO</option><option value='4'>GUAYAQUIL</option><option value='2'>CUENCA</option><option value='1' selected=''>AMBATO</option></select></td>";
        }
        else if (item.IdSucursal == 2) {

            info = info + "<td class='text-center'><select id='cboSucursalTabla' class='form-control' name='cboSucursalTabla'><option value='3'>QUITO</option><option value='4'>GUAYAQUIL</option><option value='2' selected=''>CUENCA</option><option value='1'>AMBATO</option></select></td>";
        }
        else if (item.IdSucursal == 3) {

            info = info + "<td class='text-center'><select id='cboSucursalTabla' class='form-control' name='cboSucursalTabla'><option value='3' selected=''>QUITO</option><option value='4'>GUAYAQUIL</option><option value='2'>CUENCA</option><option value='1'>AMBATO</option></select></td>";
        }
        else if (item.IdSucursal == 4) {

            info = info + "<td class='text-center'><select id='cboSucursalTabla' class='form-control' name='cboSucursalTabla'><option value='3'>QUITO</option><option value='4' selected=''>GUAYAQUIL</option><option value='2'>CUENCA</option><option value='1'>AMBATO</option></select></td>";
        }
        if (item.Estado == "ACTIVO") {
            info = info + "<td class='text-center'><select id='cboEstadoTabla' class='form-control' name='cboEstadoTabla'><option value='1' selected=''>ACTIVO</option><option value='0'>INACTIVO</option></select></td>";

        }
        else if (item.Estado == "INACTIVO") {
            info = info + "<td class='text-center'><select id='cboEstadoTabla' class='form-control' name='cboEstadoTabla'><option value='1'>ACTIVO</option><option value='0' selected=''>INACTIVO</option></select></td>";

        }

        info = info + "<td class='sorting_1' style='text-align:center'>";
        info = info + "<i class='fa fa-plus' style='cursor: pointer' onclick='AgregarMetas()' title'Ver por Numero de Orden'></i>";
        //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        //info = info + "<a href='#' class='btn-eliminaVendGeren' title='Eliminar meta' data-id=''><i class='fa fa-trash-o'></i></a>";
        //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        //info = info + "<a href='#' class='btn-EditarVendGeren' title='Editar meta' data-id=''><i class='fa fa-pencil-square-o'></i></a>";
        info = info + "</td>";

        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function RecorreJSONTableMetas(json) {
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
            info = info + "<table id='tbl_metas' width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
            info = info + "<thead><tr role='row'>";
            info = info + thInicial + "CODIGO" + thFinal;
            info = info + thInicial + "AÑO" + thFinal;
            info = info + thInicial + "META FACTURACION" + thFinal;
            info = info + thInicial + "META MARGEN BRUTO" + thFinal;
            info = info + thInicial + "---ACCIONES---" + thFinal;
            info = info + "</tr></thead>";

            info = info + "<tbody>";

            iniciaBarrido = false;
        }
        //
        // Despliegue de datos
        // 
        info = info + "<td class='sorting_1'>" + item.IdMetas + "</td>";
        info = info + "<td class='sorting_1' style='text-align: center;'><input type='text' class='form-control' id='txtAnio'  value=" + item.Anio + " /></td>";
        info = info + "<td class='sorting_1' style='text-align: right;'><input type='text' class='form-control' id='txtDescrip'  value=" + financial(item.MetaFacturacion) + " /></td>";
        info = info + "<td class='sorting_1' style='text-align: right;'><input type='text' class='form-control' id='txtDescrip2'  value=" + financial(item.MetaMargenBruto) + " /></td>";
        info = info + "<td class='sorting_1' style='text-align:center'>";
        info = info + "<i class='fa fa-plus' style='cursor: pointer' onclick='Agregar();' title'Ver por Numero de Orden'></i>";
        //info = info + "&nbsp;&nbsp;|&nbsp;&nbsp;";
        //info = info + "<a href='#' class='btn-eliminaDetalle' title='Eliminar meta' data-id=''><i class='fa fa-trash-o'></i></a>";
        info = info + "</td>";
        info = info + "</tr>";
    });

    info = info + "</tbody>";
    info = info + "</table>";

    return info;
}

function Agregar() {
    var tr = `<tr>
                <td class='sorting_1'></td>
                <td class='sorting_1' style='text-align: center;'><input type='text' class='form-control' id='txtAnio'  value="" /></td>
                <td class='sorting_1' style='text-align: right;'><input type='text' class='form-control' id='txtDescrip'  value="" /></td>
                <td class='sorting_1' style='text-align: right;'><input type='text' class='form-control' id='txtDescrip2'  value="" /></td>`
    tr = tr + `<td class='sorting_1' style='text-align:center'>`
    tr = tr + `<i class='fa fa-plus' style='cursor: pointer' onclick='Agregar();' title'Ver por Numero de Orden'></i>`
    tr = tr + `&nbsp;&nbsp;|&nbsp;&nbsp;`
    tr = tr + `<a href="#" class="btn-eliminaDetalle" title="Eliminar meta" data-id=""><i class="fa fa-trash-o"></i></a>`
    tr = tr + `</td>`
    tr = tr + `</tr>`;
    $("#tbl_metas").append(tr);
}

$(document).on('click', '.btn-eliminaDetalle', function (e) {
    e.preventDefault();
    //primer método: eliminar la fila del datatble
    var row = $(this).parent().parent()[0];
    var res = parseFloat(row.innerText.replace("|", ""));
    if (Number.isNaN(res)) {
        var i = row.rowIndex;
        document.getElementById('tbl_metas').deleteRow(i);
    }
    else {
        alerta("Este elemento no se puede eliminar.");
    }
});

function AgregarMetas() {
    var tr = `<tr>
                <td class='sorting_1'></td>
                <td class='sorting_1' style='text-align: center;'><input type='text' class='form-control' id='txtNombre'  value="" /></td>
                <td class='sorting_1' style='text-align: right;'><input type='text' class='form-control' id='txtDescrip'  value="" /></td>
                <td class='sorting_1' style = 'text-align: center;'></td>
                <td class='text-center'><select id='cboSucursalTabla' class='form-control' name='cboSucursalTabla'><option value='3' selected=''>QUITO</option><option value='4'>GUAYAQUIL</option><option value='2'>CUENCA</option><option value='1'>AMBATO</option></select></td>
                <td class='text-center'><select id='cboEstadoTabla' class='form-control' name='cboEstadoTabla'><option value='1' selected=''>ACTIVO</option><option value='0'>INACTIVO</option></select></td>`
    tr = tr + `<td class='sorting_1' style='text-align:center'>`
    tr = tr + `<i class='fa fa-plus' style='cursor: pointer' onclick='Agregar();' title'Ver por Numero de Orden'></i>`
    tr = tr + `&nbsp;&nbsp;|&nbsp;&nbsp;`
    tr = tr + `<a href="#" class="btn-eliminaVendGeren" title="Eliminar meta" data-id=""><i class="fa fa-trash-o"></i></a>`
    //tr = tr + `&nbsp;&nbsp;|&nbsp;&nbsp;`
    //tr = tr + `<a href='#' class='btn-EditarVendGeren' title='Editar meta' data-id=''><i class='fa fa-pencil-square-o'></i></a>`
    tr = tr + `</td>`
    tr = tr + `</tr>`;
    $("#tbl_cuotas").append(tr);
}

$(document).on('click', '.btn-eliminaVendGeren', function (e) {
    e.preventDefault();
    //primer método: eliminar la fila del datatble
    var row = $(this).parent().parent()[0];
    var res = parseFloat(row.innerText.replace("|", ""));
    if (Number.isNaN(res)) {
        var i = row.rowIndex;
        document.getElementById('tbl_cuotas').deleteRow(i);
    }
    else {
        alerta("Este elemento no se puede eliminar.");
    }
});


function financial(x) {
    return Number.parseFloat(x).toFixed(2);
}

function ActualizarCuota() {
    alerta("Actualizar Cuota"); 
}

function GuardarCuota() {

    //var current_index = $("#nav-pills a").tabs("option", "selected");
    //var current_index = $(".nav-pills a").tabs('option', 'active');

    var questionType = 0;
    for (var i = 0; i < 2; i++) {
        if ($("#questType").children('li')[i].className == "active") {
            questionType = i + 1;
            break;
        }
    }
    if (questionType == 1) {

        var cells = document.querySelectorAll('#tbl_metas tr');
        if (cells.length > 1) {
            var ti = "";
            var re = "↨";
            for (i = 1; i < cells.length; i++) {
                var ri = cells[i].getElementsByTagName('td');
                ti = ti + re;
                for (r = 0; r < ri.length; r++) {
                    if (r == 1 || r == 2 || r == 3) {
                        ti = ti + ri[r].children[0].value + ';';
                    }
                    else if (r == 0) {
                        ti = ti + ri[r].innerText + ';';
                    }
                }
            }

            var url = "ObtenerListaTareas.ashx";
            var datos = "";
            var mensajeVerificacion = "";
            var tipoMensaje = "warning";
            var contadorVerificacion = 0;

            var datosFormulario = "";

            datosFormulario = datosFormulario + "{";
            datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
            datosFormulario = datosFormulario + "'Tipo': '" + "0" + "',";
            datosFormulario = datosFormulario + "'resultadoDatos': '" + ti + "'";

            datosFormulario = datosFormulario + "}";

            datos = "[{'action': 'GuardarCuotaAnualEmpresa', 'parameters' : " + datosFormulario + " }]";

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
                        ObtenerMetasAnual();
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

        }
    }
    else if (questionType == 2) {

        var cells = document.querySelectorAll('#tbl_cuotas tr');
        if (cells.length > 1) {
            var ti = "";
            var re = "↨";
            for (i = 1; i < cells.length; i++) {
                var ri = cells[i].getElementsByTagName('td');
                ti = ti + re;
                for (r = 0; r < ri.length; r++) {
                    if (r == 1 || r == 2 || r == 4 || r == 5) {
                        ti = ti + ri[r].children[0].value + ';';
                    }
                    else if (r == 0 || r == 3 ) {
                        ti = ti + ri[r].innerText + ';';
                    }
                }
            }

            var url = "ObtenerListaTareas.ashx";
            var datos = "";
            var mensajeVerificacion = "";
            var tipoMensaje = "warning";
            var contadorVerificacion = 0;

            var datosFormulario = "";

            datosFormulario = datosFormulario + "{";
            datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
            datosFormulario = datosFormulario + "'Tipo': '" + $("#cboArea").val()  + "',";
            datosFormulario = datosFormulario + "'resultadoDatos': '" + ti + "'";

            datosFormulario = datosFormulario + "}";

            datos = "[{'action': 'GuardarCuotaAnual', 'parameters' : " + datosFormulario + " }]";

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
                        ObtenerListaCoutasAnual();
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
        }
        else {
            alerta("No hay registros para guardar");
        }
    }


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

    var combocboAnio = document.getElementById("cboAnio");

    if (combocboAnio.selectedIndex == 0 || $("#cboArea").val() == "0") {
        alerta("Debe agregar todo los parametros");
    }
    else {
        ObtenerListaCoutasAnual();
        $('.nav-pills a[href="#Parte2-pills"]').tab('show');
    }
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
    ObtenerListaComboAnioCuotas();
    ObtenerMetasAnual();
});
