var tablaComisiones;

function MensajeIncorrecto(resultado) {
    sweetAlert("Error", resultado, "error");
}

function MensajeCorrecto(resultado) {
    sweetAlert("Exito", resultado, "success");
}

function alerta(respuesta) {
    sweetAlert("Advertencia", respuesta, "error");
}

// 👉 Tipo 1 = Ingresos
const codigosIngresos = [
    { codigo: "1M10", descripcion: "1M10 - Com. Trimestrales M1" },
    { codigo: "1M11", descripcion: "1M11 - Com. Trimestrales M2" },
    { codigo: "1M13", descripcion: "1M13 - Com. Trimestrales M3" },
    { codigo: "1M27", descripcion: "1M27 - Horas Recuperadas" },
    { codigo: "1M30", descripcion: "1M30 - Bono Por Cumplimiento" },
    { codigo: "1M31", descripcion: "1M31 - Movilizacion" },
    { codigo: "1M32", descripcion: "1M32 - Bono Extraordinario" },
    { codigo: "1M33", descripcion: "1M33 - Devolucion Viaticos" },
    { codigo: "1M34", descripcion: "1M34 - Bono" },
    { codigo: "1M35", descripcion: "1M35 - Anticipo de Comisión" },
    { codigo: "1M37", descripcion: "1M37 - Serv. Bono x Cumplimiento" },
    { codigo: "1M38", descripcion: "1M38 - Serv. Movilizacion" },
    { codigo: "1M39", descripcion: "1M39 - Serv. Bono Extraordinario" },
    { codigo: "1M40", descripcion: "1M40 - Serv. Bono" }
];

// 👉 Tipo 2 = Descuentos
const codigosDescuentos = [
    { codigo: "2T40", descripcion: "2T40 - Préstamo Quirografar IESS" },
    { codigo: "2T41", descripcion: "2T41 - Préstamo Hipotecario IESS" },
    { codigo: "2T42", descripcion: "2T42 - Facturas Ventas" },
    { codigo: "2T45", descripcion: "2T45 - Comisiones Acreditadas" },
    { codigo: "2T46", descripcion: "2T46 - Descuentos varios" },
    { codigo: "2T47", descripcion: "2T47 - Desc. por Capacitación" },
    { codigo: "2T48", descripcion: "2T48 - Plan Dental" },
    { codigo: "2T49", descripcion: "2T49 - Multas" },
    { codigo: "2T50", descripcion: "2T50 - Claro" },
    { codigo: "2T53", descripcion: "2T53 - Equipos Celulares" },
    { codigo: "2T54", descripcion: "2T54 - Seguro Celulares" },
    { codigo: "2T55", descripcion: "2T55 - Movistar" },
    { codigo: "2T57", descripcion: "2T57 - Coffee Break" },
    { codigo: "2T59", descripcion: "2T59 - Seguro Por Robo" },
    { codigo: "2T60", descripcion: "2T60 - Seguro Médico" }
];

// 👉 Tu función usando esa descripción
function cargarCodigos() {
    var tipo = document.getElementById("cboSolicitud").value;
    var cboCodigos = document.getElementById("cboCodigos");

    // Limpia el combo de códigos
    cboCodigos.innerHTML = '<option value="">-- Seleccione código --</option>';

    let listaCodigos = [];

    if (tipo === "1") { // Ingresos
        listaCodigos = codigosIngresos;
    } else if (tipo === "2") { // Descuentos
        listaCodigos = codigosDescuentos;
    }

    listaCodigos.forEach(function (item) {
        var opt = document.createElement("option");
        opt.value = item.codigo;              // value = "2T59"
        opt.textContent = item.descripcion;   // texto = "2T59 - Seguro Por Robo"
        cboCodigos.appendChild(opt);
    });
}

function ObtenerListaUsuarios() {
    var Datos = "[{ \"action\": \"ListaUsuariosSap\", \"parameters\" : \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\" }]";
    CargarPagina('#cmbUsuarios2', 'ObtenerListaTareas.ashx', Datos, "select", "", "", "");
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

    if (tipoControl == "table") {
        contenido = RecorreJSONTable(json);
        $(div).html(contenido);
    }

    return contenido;
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

function ObtenerUsuario() {
    var comboUsuario = document.getElementById("cmbUsuarios2");
    //var selectedUsuario = comboUsuario.options[comboUsuario.selectedIndex].text;
}

$(function () {
    $.blockUI.defaults.message = "Espere un momento, por favor...";

    $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    var dateFormat = "dd/mm/yy";

    from = $("#txtFecha").datepicker(
        {
            dateFormat: dateFormat,
            dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
            dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
            firstDay: 1,
            gotoCurrent: true,
            monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]
        })
        .on("change", function () {
            from.datepicker("option", "minDate", getDate(this));
            $("#btn_Descarga").hide();
        }),

        $("#txtFecha").datepicker('setDate', 'today');

    const toLocalDatetime = d => new Date(d.getTime() - d.getTimezoneOffset() * 60000)
        .toISOString().slice(0, 16);

    const now = new Date();
    $("#txtFecha").val(toLocalDatetime(now));


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

$(document).ready(function () {

    tablaComisiones = $('#table-users').DataTable({
        destroy: true,    // por si se inicializó antes
        data: [],         // empezamos sin filas
        columns: [
            { data: 'acciones', orderable: false, searchable: false },
            { data: 'codSap' },
            { data: 'valor' },
            { data: 'observacion' },
            { data: 'fecha' },
            { data: 'ccNomina' }
        ],
        // Esto evita el warning cuando alguna propiedad viene undefined
        columnDefs: [
            { targets: '_all', defaultContent: '' }
        ]
    });

    $('#btnAgregar').on('click', function () {
        GuardarComision();
    });

    $('#btnGuardar').on('click', function () {
        var jsonTabla = ObtenerJsonTabla();
        alerta(jsonTabla);
        // usar jsonTabla...
    });

    // Evento ELIMINAR (delegado) –> importante que esté DENTRO del ready
    $('#table-users tbody').on('click', '.btn-eliminar', function () {
        var fila = tablaComisiones.row($(this).closest('tr'));
        fila.remove().draw();
    });



    $('#btnCargarArchivosAdjuntos').click(function () {

        // tu código de FormData si igual quieres subirlo al servidor
        if (window.FormData !== undefined) {
            var fileUpload = $("#archivosAdjuntos").get(0);
            var files = fileUpload.files;

            var fileData = new FormData();
            for (var i = 0; i < files.length; i++) {
                fileData.append(files[i].name, files[i]);
            }
            // aquí puedes hacer tu $.ajax(...) si quieres
        }

        // === Leer el CSV en el cliente y pintarlo en la tabla ===
        var file = files[0];
        if (!file) return;

        var reader = new FileReader();

        reader.onload = function (e) {
            var contenido = e.target.result;
            cargarCsvEnTabla(contenido);
        };

        reader.readAsText(file, "UTF-8");

    });
});

function cargarCsvEnTabla(csvText) {
    var lineas = csvText.trim().split(/\r?\n/);
    var delimitador = lineas[0].indexOf(';') !== -1 ? ';' : ',';

    // limpias la tabla
    tablaComisiones.clear();

    for (var i = 1; i < lineas.length; i++) {
        var linea = lineas[i].trim();
        if (!linea) continue;

        var columnas = linea.split(delimitador);

        var idSap = columnas[0] || "";
        var valor = columnas[1] || "";
        var obs = columnas[2] || "";
        var fecha = columnas[3] || "";
        var ccNomina = columnas[4] || "";
        if (columnas[0] != "" && columnas[1] != "" && columnas[2] != "" && columnas[3] != "" && columnas[4] != "") {
            // Botón de acciones
            var accionesHtml =
                '<button type="button" class="btn btn-danger btn-xs btn-eliminar">' +
                '<i class="fa fa-trash"></i></button>';

            // Objeto con las MISMAS propiedades definidas en columns[]
            var nuevaFila = {
                acciones: accionesHtml,
                codSap: idSap,
                valor: valor,
                observacion: obs,
                fecha: fecha,
                ccNomina: ccNomina
            };

            // Agregar fila al DataTable
            tablaComisiones.row.add(nuevaFila).draw(false);
        }
    }  
}

function GuardarComision() {
    // Capturar campos
    var ccNominaValor = $('#cboCodigos').val();                    // ej: "1M10"
    var ccNominaTexto = $('#cboCodigos option:selected').text();   // ej: "1M10 - Com. Trimestrales M1"

    var valor = $('#txtValor').val();          // 220
    var fecha = $('#txtFecha').val();          // 01/12/2025
    var observacion = $('#txtDetalle').val();    // pruebas de sistemas

    // (opcional) usuarios si luego los usas
    var usuariosTexto = $('#cmbUsuarios2 option:selected').map(function () {
        return $(this).text();
    }).get().join(', ');

    // Devuelve un array con los values seleccionados
    var usuariosIds = $('#cmbUsuarios2').val() || [];

    // Botón de acciones
    var accionesHtml =
        '<button type="button" class="btn btn-danger btn-xs btn-eliminar">' +
        '<i class="fa fa-trash"></i></button>';

    // Objeto con las MISMAS propiedades definidas en columns[]
    var nuevaFila = {
        acciones: accionesHtml,
        codSap: usuariosIds,
        valor: valor,
        observacion: observacion,
        fecha: fecha,
        ccNomina: ccNominaValor
    };

    // Agregar fila al DataTable
    tablaComisiones.row.add(nuevaFila).draw(false);

    // (Opcional) limpiar campos
    $('#txtValor').val('');
    $('#txtFecha').val('');
    $('#txtDetalle').val('');
    $('#cmbUsuarios2').val(null).trigger('change');
}

function ObtenerJsonTabla() {
    // Obtiene todas las filas del DataTable (como array de objetos)
    var rows = tablaComisiones.rows().data().toArray();

    // Si quieres excluir la columna "acciones" y solo enviar datos útiles:
    var datosLimpios = rows.map(function (row) {
        return {
            codSap: row.codSap,
            valor: row.valor,
            observacion: row.observacion,
            fecha: row.fecha,
            ccNomina: row.ccNomina
        };
    });

    // Convertir a JSON
    var json = JSON.stringify(datosLimpios);
    console.log(json);

    // Aquí puedes enviarlo al servidor con AJAX si quieres:
    /*
    $.ajax({
        url: 'TuApi.asmx/Metodo',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: json,
        success: function (resp) {
            console.log('OK', resp);
        }
    });
    */

    return json;
}

$(document).ready(function () {

    $('.js-example-basic-multiple7').select2({
        width: '250px'
    });

});