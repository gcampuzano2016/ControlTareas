
//funcion para cargar el combo select 
function ObtenerListaComboPerfiles() {
    var Datos = "[{ \"action\": \"ListaComboPerfiles\", \"parameters\" : { \"Tipo\" : \"50\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#CmbPerfiles', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}

// funcion para el boton consulta 
function ObtenerListaComboPerfilesEditar() {
    var Datos = "[{ \"action\": \"ListaComboPerfiles\", \"parameters\" : { \"Tipo\" : \"50\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\",\"IdSucursal\" : \"0\", IdCliente: \"" + "0" + "\"} }]";
    CargarPagina('#CmbPerfilesDos', 'ObtenerListaTareas.ashx', Datos, "select", "", "");
}


function ObtenerListaUsuariosPerfil() {

    //var comboPerfil = document.getElementById("CmbPerfiles");
    //var selectedcomboPerfil = comboPerfil.options[comboPerfil.selectedIndex].text;

    var Datos = "[{ \"action\": \"ListaUsuariosPerfil\", \"parameters\" : {  \"tipo\" : \"" + $("#CmbPerfiles").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "table", "", "");
}


function DescargarPerfiles() {
    $("#divMensajes").html("");

    var perfiles = document.getElementById("CmbPerfiles");
    var selectedPerfil = perfiles.options[perfiles.selectedIndex].text;
    if (selectedPerfil == "-- SELECCIONE --") {
        alerta("No ha seleccionado un Perfil");

    } else {
        ObtenerPerfilDescarga();
    }

}


function ObtenerPerfilDescarga() {

    var Datos = "[{ \"action\": \"ListaUsuariosPerfilDescarga\", \"parameters\" : {  \"tipo\" : \"" + $("#CmbPerfiles").val() + "\"} }]";
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

    if (tipoControl == "select") {
        contenido = RecorreJSONSelect(div, json, idSeleccionado);
    }
    if (tipoControl == "table") {
        contenido = RecorreJSONTable(json);
        $(div).html(contenido);
        //$(divPanelHide).hide();
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
            var thInicial = "<th class='' tabindex='0' aria-controls='dataTables - Datos' rowspan='1' colspan='1' aria-label='Engine version: activate to sort column ascending' style='width: 147px;'>";
            var thFinal = "</th>";
            info = info + "<table id='tbl_perfiles'width='100%' class='table table-striped table-bordered table-hover dataTable no-footer dtr-inline' role='grid' aria-describedby='dataTables-example_info' style='width: 100%;'>";
            info = info + "<thead><tr role='row'>";
            info = info + thInicial + "CODIGO" + thFinal;
            info = info + thInicial + "PERFIL" + thFinal;
            info = info + thInicial + "USUARIO" + thFinal;
            info = info + thInicial + "MAIL" + thFinal;
            //info = info + thInicial + "ESATDO" + thFinal;
            info = info + "</tr></thead>";

            info = info + "<tbody>";

            iniciaBarrido = false;
        }
        //
        // Despliegue de datos
        // 
        info = info + "<td class='sorting_1'>" + item.Codigo + "-" + item.IdUsuario + "</td>";

        //info = info + "<td class='sorting_1'>" + item.Codigo + "</td>";
        info = info + "<td class='text-center' >" + item.Combo + "</td>";
        //ObtenerListaComboPerfilesEditar();

        //info = info + "<td class='text-left'><input type='text' class='form-control' id='txtNombre' value=" + item.NombrePerfil + " /></td>";

        //info = info + "<td class='sorting_1' style='text-align: left;'>" + item.Nombres + "</td>";
        info = info + "<td class='sorting_1'>" + item.NombreUsuario + "</td>";

        info = info + "<td class='sorting_1' style='text-align: right;'><input type='text' class='form-control' id='txtMail'  value=" + item.Mail + " /></td>";
        //if (item.Estado == 0) {
        //    info = info + "<td class='text-center'><select id='cboEstadoTabla' class='form-control' name='cboEstadoTabla'><option value='0' selected=''>ACTIVO</option><option value='1'>INACTIVO</option></select></td>";

        //}
        //else if (item.Estado == 1) {
        //    info = info + "<td class='text-center'><select id='cboEstadoTabla' class='form-control' name='cboEstadoTabla'><option value='0'>ACTIVO</option><option value='1' selected=''>INACTIVO</option></select></td>";

        //}

        // info = info + "<td class='sorting_1' style='text-align:center'>";
        //info = info + "<i class='fa fa-plus' style='cursor: pointer' onclick='AgregarUsuario()' title'Ver por Numero de Orden'></i>";
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

//load de la pagina 
$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);
    ObtenerListaComboPerfiles();

});

//funcion para consultar los datos 

function BtnConsulta() {
    $("#divMensajes").html("");

    var comboPerfil = document.getElementById("CmbPerfiles");

    if (comboPerfil.selectedIndex == 0 || $("#CmbPerfiles").val() == "0") {
        alerta("Debe seleccionar un perfil");
    }
    else {
        ObtenerListaUsuariosPerfil();

    }
}





function GuardarDatosUsuario() {
    var cells = document.querySelectorAll('#tbl_perfiles tr');
    if (cells.length > 1) {
        var ti = "";
        var re = "↨";
        for (i = 1; i < cells.length; i++) {
            var ri = cells[i].getElementsByTagName('td');
            ti = ti + re;
            for (r = 0; r < ri.length; r++) {
                if (r == 1 || r == 3 || r == 4) {
                    ti = ti + ri[r].children[0].value + ';';
                }
                else if (r == 0 || r == 2) {
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

        datos = "[{'action': 'GuardarUsuarioPerfil', 'parameters' : " + datosFormulario + " }]";

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

                    ObtenerListaUsuariosPerfil();
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

    } else {
        alerta("No hay registros para guardar");
    }

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