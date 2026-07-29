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


$(document).on('click', '#btnEntrada', function (e) {
    e.preventDefault();
    confirmarAntesDeGuardar(function () { RegistrarEvento(1); });
});

$(document).on('click', '#btnSalida', function (e) {
    e.preventDefault();
    confirmarAntesDeGuardar(function () { RegistrarEvento(2); });
});



function RegistrarEvento(Accion) {

    var url = "AdministrarMarcacion.ashx";
    var datos = "";
    var mensajeVerificacion = "";
    var tipoMensaje = "warning";
    var contadorVerificacion = 0;

    if (contadorVerificacion > 0) {
        alerta(mensajeVerificacion);
        return;
    }

    var datosFormulario = "";

    var valor =  document.getElementById('txtUsuario').value.trim();

    datosFormulario = datosFormulario + "{";
    datosFormulario = datosFormulario + "'session': '" + valor + "',";
    datosFormulario = datosFormulario + "'Accion': '" + Accion + "'";

    datosFormulario = datosFormulario + "}";

    datos = "[{'action': 'RegistrarMarcacion', 'parameters' : " + datosFormulario + " }]";

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
                ConsultarEvento();
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

function ConsultarEvento() {
    var tipo = 0;
    var valor = document.getElementById('txtUsuario').value.trim();
    var Datos = "[{ \"action\": \"ConsultarEvento\", \"parameters\" : { session: \"" + valor + "\", tipo: \"" + tipo + "\"} }]";
    CargarPagina('#datosTablaPrincipal', 'ObtenerListaTareas.ashx', Datos, "detalleDatos", tipo);
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

    if (tipoControl == "detalleDatos") {
        contenido = RecorreJSONTable(json);
        $(div).html(contenido);
    }

    return contenido;
}

function RecorreJSONTable(json) {
    $.each(json, function (i, item) {
        document.getElementById('btnSalida').disabled = true;
        if (item.IdProceso == 0) {

        }
        else {
            IdProceso = item.IdProceso;
            var fecha1 = dotNetDateToDDMMYYYY(item.FechaEntrada)
            var fecha2 = dotNetDateToDDMMYYYY(item.FechaSalida)
            var fechaActual = hoyDDMMYYYY();
            var estadoEntrada = compareDates(fecha1, fechaActual);
            if (estadoEntrada == 0) {
                document.getElementById('btnEntrada').disabled = true;
                document.getElementById('btnSalida').disabled = false;
                //const btn = document.getElementById('btnEntrada');
                //if (btn) btn.title = 'la acción esta inactiva porque ya se realizo el registro';
                $('#btnEntrada').attr('title', 'la acción esta inactiva porque ya se realizo el registro');
            }
            var estadoSalida = compareDates(fecha2, fechaActual);
            if (estadoSalida == 0) {
                document.getElementById('btnSalida').disabled = true;
                //const btn = document.getElementById('btnSalida');
                //if (btn) btn.title = 'la acción esta inactiva porque ya se realizo el registro';
                $('#btnSalida').attr('title', 'la acción esta inactiva porque ya se realizo el registro');
            }
        }
    });
}


function parseDDMMYYYY(s) {
    const m = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(s);
    if (!m) return null;
    const [_, dd, mm, yyyy] = m;
    const d = new Date(Number(yyyy), Number(mm) - 1, Number(dd));
    // Validación (evita 31/02/2025, etc.)
    if (d.getFullYear() !== Number(yyyy) ||
        d.getMonth() !== Number(mm) - 1 ||
        d.getDate() !== Number(dd)) return null;
    d.setHours(0, 0, 0, 0);
    return d;
}

function compareDates(aStr, bStr) {
    const a = parseDDMMYYYY(aStr);
    const b = parseDDMMYYYY(bStr);
    if (!a || !b) throw new Error("Fecha inválida");
    const diff = a.getTime() - b.getTime();
    return diff < 0 ? -1 : diff > 0 ? 1 : 0;
}

function hoyDDMMYYYY() {
    const d = new Date();
    const dd = String(d.getDate()).padStart(2, '0');
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const yyyy = d.getFullYear();
    return `${dd}/${mm}/${yyyy}`;
}

function dotNetDateToDDMMYYYY(str) {
    const m = /\/Date\((-?\d+)([+-]\d{4})?\)\//.exec(str);
    if (!m) return null;

    const ms = parseInt(m[1], 10);                 // milisegundos desde epoch (UTC)
    const off = m[2];                               // p.ej. -0500, +0230, etc.

    let d;
    if (off) {
        // calcular minutos de offset, ej. -0500 => -300 min
        const sign = off[0] === '-' ? -1 : 1;
        const hh = parseInt(off.slice(1, 3), 10);
        const mm = parseInt(off.slice(3, 5), 10);
        const offsetMinutes = sign * (hh * 60 + mm);

        // UTC + offset = hora local del offset. Usamos getters UTC para no mezclar con tu zona local.
        d = new Date(ms + offsetMinutes * 60000);
        var day = String(d.getUTCDate()).padStart(2, '0');
        var month = String(d.getUTCMonth() + 1).padStart(2, '0');
        var year = d.getUTCFullYear();
    } else {
        // sin offset: usamos la hora local del navegador/servidor
        d = new Date(ms);
        var day = String(d.getDate()).padStart(2, '0');
        var month = String(d.getMonth() + 1).padStart(2, '0');
        var year = d.getFullYear();
    }

    return `${day}/${month}/${year}`;
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
    const rutaActual = window.location.pathname;
    if (rutaActual == "/Formulario/Principal.aspx") {
        ConsultarEvento();
    }
    else {
        document.getElementById('btnEntrada').disabled = true;
        document.getElementById('btnSalida').disabled = true;
    }
});