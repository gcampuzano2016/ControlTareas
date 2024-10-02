
var IdEmpleado = 0;
var IdCargaFam = 0;
var estado = "";
var Operacion = 0;
var table1;
var table2;
var IdPerfil = 0;

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

function nuevoRegistroEmpleados() {
    document.getElementById("editBoton").style.display = "none";
    document.getElementById("editMenu").style.display = "none";
    document.getElementById("editCargaFamiliar").style.display = "none";
    document.getElementById("editEmpleados").style.display = "block";
    document.getElementById("editRegistroCargaFamiliar").style.display = "none";
    document.getElementById("editBotonCargaFam").style.display = "none";
    BorrarCajas();
}

function editarRegistroEmpleados() {
    document.getElementById("editBoton").style.display = "none";
    document.getElementById("editMenu").style.display = "none";
    document.getElementById("editCargaFamiliar").style.display = "none";
    document.getElementById("editEmpleados").style.display = "block";
    document.getElementById("editRegistroCargaFamiliar").style.display = "none";
    document.getElementById("editBotonCargaFam").style.display = "none";
    //BorrarCajas();
}

function VerListaEmpleados() {

    if (IdPerfil == 14) {
        document.getElementById("editBoton").style.display = "block";
    }
    document.getElementById("editMenu").style.display = "block";
    document.getElementById("editEmpleados").style.display = "none";
    document.getElementById("editCargaFamiliar").style.display = "none";
    document.getElementById("editRegistroCargaFamiliar").style.display = "none";
    document.getElementById("editBotonCargaFam").style.display = "none";

    //document.getElementById("btnGProducto").innerHTML = "Guardar";
}

function VerGuardarEmpleado() {
    if (IdPerfil == 14) {
        document.getElementById("editBoton").style.display = "block";
    }
    document.getElementById("editMenu").style.display = "block";
    document.getElementById("editEmpleados").style.display = "none";
    document.getElementById("editCargaFamiliar").style.display = "none";
    document.getElementById("editRegistroCargaFamiliar").style.display = "none";
    document.getElementById("editBotonCargaFam").style.display = "none";

    BorrarCajas2();
    //document.getElementById("btnGEmpleado").innerHTML = "Guardar";
}

function VerCargaFamiliarEmpleado(id) {
    IdEmpleado = id;
    document.getElementById("editBoton").style.display = "none";
    document.getElementById("editMenu").style.display = "none";
    document.getElementById("editEmpleados").style.display = "none";
    document.getElementById("editRegistroCargaFamiliar").style.display = "none";
    document.getElementById("editBotonCargaFam").style.display = "block";
    document.getElementById("editCargaFamiliar").style.display = "block";

    ObtenerListaCargaFamiliar(IdEmpleado);

    //document.getElementById("btnGProducto").innerHTML = "Guardar";
}

function VerCargaFamiliarEmpleado2() {
    document.getElementById("editBoton").style.display = "none";
    document.getElementById("editMenu").style.display = "none";
    document.getElementById("editEmpleados").style.display = "none";
    document.getElementById("editRegistroCargaFamiliar").style.display = "none";
    document.getElementById("editBotonCargaFam").style.display = "block";
    document.getElementById("editCargaFamiliar").style.display = "block";

    //document.getElementById("btnGProducto").innerHTML = "Guardar";
}

// Para ingresar una nueva carga familiar
function EditarCargaFamiliarEmpleado() {
    document.getElementById("editBoton").style.display = "none";
    document.getElementById("editMenu").style.display = "none";
    document.getElementById("editEmpleados").style.display = "none";
    document.getElementById("editCargaFamiliar").style.display = "none";
    document.getElementById("editBotonCargaFam").style.display = "none";
    document.getElementById("editRegistroCargaFamiliar").style.display = "block";
    BorrarCajas2();

    //document.getElementById("btnGProducto").innerHTML = "Guardar";
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


function ObtenerListaEmpleados(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaEmpleados\", \"parameters\" : { tipo : \"" + IdPerfil + "\", descripcion: \"" + "Lista" + "\", session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}

function ObtenerListaCargaFamiliar(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"BuscarListaCargaFam\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + "" + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectFamiliar", tipo2);
}

function EliminarEmpleado(tipo, descripcion, tipo2) {
    var Datos = "[{ \"action\": \"EliminarEmpleado\", \"parameters\" : { tipo : \"" + tipo + "\", descripcion: \"" + descripcion + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectBusqueda", tipo2);
}

function EliminarCargaFam(Nombre, tipo2) {
    var Datos = "[{ \"action\": \"EliminarCargaFam\", \"parameters\" : { Nombre : \"" + Nombre + "\"} }]";
    CargarPagina('#datosTablaPrincipal2', 'ObtenerListaTareas.ashx', Datos, "tableSelectFamiliar", tipo2);
}

function RecorreJSON(div, json, tipoControl, boton, idSeleccionado) {

    var contenido = "";

    if (tipoControl == "tableSelectBusqueda") {
        contenido = RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectFamiliar") {
        contenido = RecorreJSONTableSelectFamilia(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    return contenido;
}

function RecorreJSONTableSelectBusqueda(json, boton, idSeleccionado) {

    dtEmpleados(json);
}

function Create() {
    if ($.fn.DataTable.isDataTable('#tbl_Empleados')) {
        $('#tbl_Empleados').DataTable().destroy();
    }
    $('#tbl_Empleados tbody').empty();
    //Here call the Datatable Bind function;
}

function dtEmpleados(json) {
    //<button type='button' value='Actualizar' title='Editar' class='btn btn btn-editMenu btn-xs'><i class='fa fa-edit' aria-hidden='true'></i></button>  
    Create();
    table1 = null;

    table1 = $('#tbl_Empleados').DataTable({
        data: json,
        columns: [
            { data: 'IdEmpleado' },
            { data: 'Cedula' },
            { data: 'Nombre' },
            { data: 'Sociedad' },
            { data: 'Ciudad' },
            { data: 'AreaTrabajo' },
            { data: 'Direccion' },
            { data: 'Telefono' },
            { data: 'Sexo' },
            { data: 'Fecha_Nacimiento' },
            { data: 'Provincia' },
            { defaultContent: "<button type='button' value='Actualizar' title='Ver carga familiar' class='btn btn-verCargaFamiliar btn-xs'><i class='fa fa-eye' aria-hidden='true'></i></button>" },
            { defaultContent: "<button type='button' value='Eliminar' title='Eliminar' class='btn btn btn-deleteEmp btn-xs'><i class='fa fa-trash-o' aria-hidden='true'></i></button>" },
            { data: 'Estado' }
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


function BorrarCajas() {
    var combo1 = document.getElementById("tbl_Empleados");
    combo1.value = 0;
    $('#txtNombre').val("");
    $('#txtCedula').val("");
    $('#txtIdEmpleado').val("");
    $('#txtSociedad').val("");
    $('#txtCiudad').val("");
    $('#txtAreaTrabajo').val("");
    $('#txtDireccion').val("");
    $('#txtTelefono').val("");
    $('#txtSexo').val("");
    $('#txtFechaNacimiento').val("");
    $('#txtProvincia').val("");
    document.getElementById("btnGEmpleado").innerHTML = "Guardar";
}


$(document).on('click', '.btn-editMenu', function (e) {
    e.preventDefault();
    var data = table1.row($(this).parents('tr')).data();
    CargarItems(data);
    editarRegistroEmpleados();
});

$(document).on('click', '.btn-verCargaFamiliar', function (e) {
    e.preventDefault();
    var data = table1.row($(this).parents('tr')).data();
    VerCargaFamiliarEmpleado(data.IdEmpleado);
});

$(document).on('click', '.btn-deleteEmp', function (e) {
    e.preventDefault();
    var data = table1.row($(this).parents('tr')).data();

    EliminarEmpleado(data.IdEmpleado, data.Estado);
    // Recarga la página después de 1/2 segundo
    setTimeout(function () {
        location.reload();
    }, 500); // Espera 1/2 segundo (500 milisegundos)

    //ObtenerListaEmpleados("", "", "");
});

function CargarItems(data) {
    //var combo1 = document.getElementById("cboMarca");
    //combo1.value = data.IdMarca;
    IdEmpleado = data.IdEmpleado;
    $('#txtNombre').val(data.Nombre);
    $('#txtCedula').val(data.Cedula);
    $('#txtSociedad').val(data.Sociedad);
    $('#txtCiudad').val(data.Ciudad);
    $('#txtAreaTrabajo').val(data.AreaTrabajo);
    $('#txtDireccion').val(data.Direccion);
    $('#txtTelefono').val(data.Telefono);
    $('#txtSexo').val(data.Sexo);
    $('#txtFechaNacimiento').val(data.Fecha_Nacimiento);
    $('#txtProvincia').val(data.Provincia);
    document.getElementById("btnGEmpleado").innerHTML = "Actualizar";
}

function GuardarEmpleado() {

    if (document.getElementById("btnGEmpleado").innerHTML == "Actualizar") {

        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var mensajeVerificacion = "";
        var tipoMensaje = "warning";
        var contadorVerificacion = 0;

        if ($('#txtNombre').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Nombre ";
            contadorVerificacion += 1;
        }

        if ($('#txtCedula').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Cédula ";
            contadorVerificacion += 1;
        }

        /*if ($('#txtIdEmpleado').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Codigo del empleado ";
            contadorVerificacion += 1;
        }*/

        if ($('#txtSociedad').val() == "") {
            mensajeVerificacion += "- Debe ingresar la sociedad ";
            contadorVerificacion += 1;
        }

        if ($('#txtCiudad').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Ciudad ";
            contadorVerificacion += 1;
        }

        if ($('#txtAreaTrabajo').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Area de trabajo ";
            contadorVerificacion += 1;
        }

        if ($('#txtDireccion').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Dirección ";
            contadorVerificacion += 1;
        }

        if ($('#txtTelefono').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Télefono ";
            contadorVerificacion += 1;
        }

        if ($('#txtSexo').val() == "") {
            mensajeVerificacion += "- Debe ingresar el sexo ";
            contadorVerificacion += 1;
        }

        if ($('#txtFechaNacimiento').val() == "") {
            mensajeVerificacion += "- Debe ingresar la fecha de nacimiento ";
            contadorVerificacion += 1;
        }

        if ($('#txtProvincia').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Provincia";
            contadorVerificacion += 1;
        }

        if (contadorVerificacion > 0) {
            alerta(mensajeVerificacion);
            return;
        }

        var datosFormulario = "";

        datosFormulario = datosFormulario + "{";
        datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
        datosFormulario = datosFormulario + "'txtCedula': '" + $('#txtCedula').val() + "',";
        datosFormulario = datosFormulario + "'txtNombre': '" + $('#txtNombre').val() + "',";
        datosFormulario = datosFormulario + "'txtIdEmpleado': '" + IdEmpleado + "',";
        datosFormulario = datosFormulario + "'txtSociedad': '" + $('#txtSociedad').val() + "',";
        datosFormulario = datosFormulario + "'txtCiudad': '" + $('#txtCiudad').val() + "',";
        datosFormulario = datosFormulario + "'txtAreaTrabajo': '" + $('#txtAreaTrabajo').val() + "',";
        datosFormulario = datosFormulario + "'txtDireccion': '" + $('#txtDireccion').val() + "',";
        datosFormulario = datosFormulario + "'txtTelefono': '" + $('#txtTelefono').val() + "',";
        datosFormulario = datosFormulario + "'txtSexo': '" + $('#txtSexo').val() + "',";
        datosFormulario = datosFormulario + "'txtFechaNacimiento': '" + $('#txtFechaNacimiento').val() + "',";
        datosFormulario = datosFormulario + "'txtProvincia': '" + $('#txtProvincia').val() + "'";
        datosFormulario = datosFormulario + "}";

        datos = "[{'action': 'GuardarNuevoEmpleado', 'parameters' : " + datosFormulario + " }]";

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
                    VerListaEmpleados();
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
    else if (document.getElementById("btnGEmpleado").innerHTML == "Guardar") {

        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var mensajeVerificacion = "";
        var tipoMensaje = "warning";
        var contadorVerificacion = 0;

        if ($('#txtNombre').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Nombre ";
            contadorVerificacion += 1;
        }

        if ($('#txtCedula').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Cédula ";
            contadorVerificacion += 1;
        }

        /*if ($('#txtIdEmpleado').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Codigo del empleado ";
            contadorVerificacion += 1;
        }*/

        if ($('#txtSociedad').val() == "") {
            mensajeVerificacion += "- Debe ingresar la sociedad ";
            contadorVerificacion += 1;
        }

        if ($('#txtCiudad').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Ciudad ";
            contadorVerificacion += 1;
        }

        if ($('#txtAreaTrabajo').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Area de trabajo ";
            contadorVerificacion += 1;
        }

        if ($('#txtDireccion').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Dirección ";
            contadorVerificacion += 1;
        }

        if ($('#txtTelefono').val() == "") {
            mensajeVerificacion += "- Debe ingresar el Télefono ";
            contadorVerificacion += 1;
        }

        if ($('#txtSexo').val() == "") {
            mensajeVerificacion += "- Debe ingresar el sexo ";
            contadorVerificacion += 1;
        }

        if ($('#txtFechaNacimiento').val() == "") {
            mensajeVerificacion += "- Debe ingresar la fecha de nacimiento ";
            contadorVerificacion += 1;
        }

        if ($('#txtProvincia').val() == "") {
            mensajeVerificacion += "- Debe ingresar la Provincia";
            contadorVerificacion += 1;
        }

        if (contadorVerificacion > 0) {
            alerta(mensajeVerificacion);
            return;
        }

        var datosFormulario = "";

        datosFormulario = datosFormulario + "{";
        datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
        datosFormulario = datosFormulario + "'txtCedula': '" + $('#txtCedula').val() + "',";
        datosFormulario = datosFormulario + "'txtNombre': '" + $('#txtNombre').val() + "',";
        datosFormulario = datosFormulario + "'txtIdEmpleado': '" + "0" + "',";
        datosFormulario = datosFormulario + "'txtSociedad': '" + $('#txtSociedad').val() + "',";
        datosFormulario = datosFormulario + "'txtCiudad': '" + $('#txtCiudad').val() + "',";
        datosFormulario = datosFormulario + "'txtAreaTrabajo': '" + $('#txtAreaTrabajo').val() + "',";
        datosFormulario = datosFormulario + "'txtDireccion': '" + $('#txtDireccion').val() + "',";
        datosFormulario = datosFormulario + "'txtTelefono': '" + $('#txtTelefono').val() + "',";
        datosFormulario = datosFormulario + "'txtSexo': '" + $('#txtSexo').val() + "',";
        datosFormulario = datosFormulario + "'txtFechaNacimiento': '" + $('#txtFechaNacimiento').val() + "',";
        datosFormulario = datosFormulario + "'txtProvincia': '" + $('#txtProvincia').val() + "'";
        datosFormulario = datosFormulario + "}";

        datos = "[{'action': 'GuardarNuevoEmpleado', 'parameters' : " + datosFormulario + " }]";

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
                    VerListaEmpleados();
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
}





function RecorreJSONTableSelectFamilia(json, boton, idSeleccionado) {

    dtCargaFam(json);
}

function CreateFamilia() {
    if ($.fn.DataTable.isDataTable('#tbl_CargaFam')) {
        $('#tbl_CargaFam').DataTable().destroy();
    }
    $('#tbl_CargaFam tbody').empty();
    //Here call the Datatable Bind function;
}

function dtCargaFam(json) {
    CreateFamilia();
    table2 = null;

    table2 = $('#tbl_CargaFam').DataTable({
        data: json,
        columns: [
            { data: 'IdCargaFam' },
            { data: 'Parentesco' },
            { data: 'Nombre' },
            { data: 'fecha_nacimiento' },
            { defaultContent: "<button type='button' value='Actualizar' title='Editar' class='btn btn btn-editCarga btn-xs'><i class='fa fa-edit' aria-hidden='true'></i></button>  <button type='button' value='Eliminar' title='Eliminar' class='btn btn btn-deleteCarga btn-xs'><i class='fa fa-trash-o' aria-hidden='true'></i></button>" },
            { data: 'Estado' }
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
}


function CargarItems2(data2) {
    IdEmpleado = data2.IdEmpleado;
    IdCargaFam = data2.IdCargaFam;
    $('#txtParentescoCF').val(data2.Parentesco);
    $('#txtNombreCF').val(data2.Nombre);
    $('#txtFechaNacimientoCF').val(data2.fecha_nacimiento);
    document.getElementById("btnGCargaFam").innerHTML = "Actualizar";
}

function BorrarCajas2() {
    var combo1 = document.getElementById("tbl_CargaFam");
    combo1.value = 0;
    $('#txtParentescoCF').val("");
    $('#txtNombreCF').val("");
    $('#txtFechaNacimientoCF').val("");
    document.getElementById("btnGCargaFam").innerHTML = "Guardar";
}

$(document).on('click', '.btn-editCarga', function (e) {
    e.preventDefault();
    var data2 = table2.row($(this).parents('tr')).data();
    EditarCargaFamiliarEmpleado();
    CargarItems2(data2);

});

$(document).on('click', '.btn-deleteCarga', function (e) {
    e.preventDefault();
    var data2 = table2.row($(this).parents('tr')).data();
    //var data = table1.row($(this).parents('tr')).data();

    EliminarCargaFam(data2.Nombre);

    // Recarga la página después de 1/2 segundo
    setTimeout(function () {
        location.reload();
    }, 500); // Espera 1/2 segundo (500 milisegundos)

});

function GuardarCargaFam() {

    if (document.getElementById("btnGCargaFam").innerHTML == "Actualizar") {
        Operacion = 2;

        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var mensajeVerificacion = "";
        var tipoMensaje = "warning";
        var contadorVerificacion = 0;

        if ($('#txtParentescoCF').val() == "") {
            mensajeVerificacion += "- Debe ingresar el parentesco";
            contadorVerificacion += 1;
        }

        if ($('#txtNombreCF').val() == "") {
            mensajeVerificacion += "- Debe ingresar el nombre ";
            contadorVerificacion += 1;
        }

        if ($('#txtFechaNacimientoCF').val() == "") {
            mensajeVerificacion += "- Debe ingresar la fecha de nacimiento ";
            contadorVerificacion += 1;
        }

        if (contadorVerificacion > 0) {
            alerta(mensajeVerificacion);
            return;
        }

        var datosFormulario = "";

        datosFormulario = datosFormulario + "{";
        datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
        datosFormulario = datosFormulario + "'IdCargaFam': '" + IdCargaFam + "',";
        datosFormulario = datosFormulario + "'IdEmpleado': '" + IdEmpleado + "',";
        datosFormulario = datosFormulario + "'Operacion': '" + Operacion + "',";
        datosFormulario = datosFormulario + "'txtParentescoCF': '" + $('#txtParentescoCF').val() + "',";
        datosFormulario = datosFormulario + "'txtNombreCF': '" + $('#txtNombreCF').val() + "',";
        datosFormulario = datosFormulario + "'txtFechaNacimientoCF': '" + $('#txtFechaNacimientoCF').val() + "'";
        datosFormulario = datosFormulario + "}";

        datos = "[{'action': 'GuardarNuevaCargaFamiliar', 'parameters' : " + datosFormulario + " }]";

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
                    VerCargaFamiliarEmpleado(IdEmpleado);
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
    else if (document.getElementById("btnGCargaFam").innerHTML == "Guardar") {

        var url = "ObtenerListaTareas.ashx";
        var datos = "";
        var mensajeVerificacion = "";
        var tipoMensaje = "warning";
        var contadorVerificacion = 0;
        Operacion = 1;

        if ($('#txtParentescoCF').val() == "") {
            mensajeVerificacion += "- Debe ingresar el parentesco";
            contadorVerificacion += 1;
        }

        if ($('#txtNombreCF').val() == "") {
            mensajeVerificacion += "- Debe ingresar el nombre ";
            contadorVerificacion += 1;
        }

        if ($('#txtFechaNacimientoCF').val() == "") {
            mensajeVerificacion += "- Debe ingresar la fecha de nacimiento ";
            contadorVerificacion += 1;
        }

        if (contadorVerificacion > 0) {
            alerta(mensajeVerificacion);
            return;
        }

        var datosFormulario = "";

        datosFormulario = datosFormulario + "{";
        datosFormulario = datosFormulario + "'session': '" + $("#ContentPlaceHolder1_txtUsuario").val() + "',";
        datosFormulario = datosFormulario + "'IdEmpleado': '" + IdEmpleado + "',";
        datosFormulario = datosFormulario + "'Operacion': '" + Operacion + "',";
        datosFormulario = datosFormulario + "'txtParentescoCF': '" + $('#txtParentescoCF').val() + "',";
        datosFormulario = datosFormulario + "'txtNombreCF': '" + $('#txtNombreCF').val() + "',";
        datosFormulario = datosFormulario + "'txtFechaNacimientoCF': '" + $('#txtFechaNacimientoCF').val() + "'";
        datosFormulario = datosFormulario + "}";

        datos = "[{'action': 'GuardarNuevaCargaFamiliar', 'parameters' : " + datosFormulario + " }]";

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
                    VerListaEmpleados();
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
}



/*================================*/
/**     Accion de descarga xls    */
/*================================*/

function BtnDescarga() {
    //$("#divMensajes").html("");
    //ObtenerListaContratosDescarga(tipoGeneral, idregistroGeneral);

    ObtenerListaEmpleadosDescarga(0, 0,);
}

function ObtenerListaEmpleadosDescarga(tipo, idRegistro) {


    //var Datos = "[{ \"action\": \"BuscarListaEmpleadosDescargar\", \"parameters\" : {busqueda: \"" + $("#txtNumeroOrden2").val() + "\", IdCliente: \"" + idCliente2 + "\", IdGerenteCuenta: \"" + $("#cboVendedor2").val() + "\", IdGestorProducto: \"" + $("#cboGerente2").val() + "\", sucursal: \"" + $("#cboSucursal2").val() + "\", estado: \"" + $("#cboEstado2").val() + "\", IdClasificacion: \"" + $("#cboClasificacion2").val() + "\", Anio: \"" + $("#cboAnio").val() + "\", meses: \"" + $("#cboMeses").val() + "\", idFecha: \"" + $("#cboFecha").val() + "\"} }]";
    var Datos = "[{ \"action\": \"BuscarListaEmpleadosDescargar\", \"parameters\" : { tipo : \"" + "0" + "\", descripcion: \"" + "" + "\"} }]";
    idCliente2 = 0;
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

$(function () {

    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    var dateFormat = "dd/mm/yy";

    from = $("#txtFechaNacimiento, #txtFechaNacimientoCF").datepicker(
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

        $("#txtFechaNacimiento, #txtFechaNacimientoCF").datepicker('setDate', 'today');

    IdPerfil = $("#ContentPlaceHolder1_txtPerfil").val();

    ObtenerListaEmpleados("", "", "");

    VerListaEmpleados();

});

