/* ============================================================================
   Feriados y saldo en las solicitudes de vacaciones.

   Hasta agosto de 2026 el colaborador escribía a mano cuántos feriados caían
   dentro de sus vacaciones. Ese número no era decorativo: se restaba de los días
   que se cobran al saldo, porque un feriado dentro del rango no consume
   vacaciones. La especificación pidió quitar el campo; quitarlo sin más le
   habría cobrado los días completos, así que se quitó el trabajo manual y no el
   concepto: ahora el número sale de la tabla Feriado.

   Este archivo existe porque la pantalla de solicitud y la de lista tienen el
   mismo formulario duplicado, con su lógica duplicada en Convenio.js y en
   ProcesoConvenio.js. Lo nuevo se escribe una sola vez y las dos lo usan.
   ============================================================================ */

/* Día siguiente a una fecha dd/MM/yyyy, en el mismo formato. Cadena vacía si la
   fecha no se entiende — la pantalla llama a esto con el formulario a medio
   llenar. */
function FeriadosDiaSiguiente(fecha) {
    var partes = (fecha || "").split('/');
    if (partes.length != 3) { return ""; }

    var d = new Date(partes[2], partes[1] - 1, partes[0]);
    if (isNaN(d.getTime())) { return ""; }

    d.setDate(d.getDate() + 1);

    var dd = ("0" + d.getDate()).slice(-2);
    var mm = ("0" + (d.getMonth() + 1)).slice(-2);
    return dd + "/" + mm + "/" + d.getFullYear();
}

/* Deja los campos de feriados en cero y sin mensajes. */
function FeriadosLimpiarUI() {
    $('#frmTxtTiempoF').val("0");
    $('#frmTxtFeriadosMsg').text("").removeClass("text-danger");
    $('#IdAvisoSaldo').hide().text("");
}

/* Descuenta de "Días" los feriados del rango y explica de dónde sale el número. */
function FeriadosAplicarRespuesta(respuesta, naturales) {
    var $msg = $('#frmTxtFeriadosMsg').removeClass("text-danger");

    if (respuesta == null || typeof respuesta.Feriados == "undefined") {
        $('#frmTxtTiempoF').val("0");
        $msg.text("");
        return;
    }

    var feriados = parseFloat(respuesta.Feriados) || 0;
    var sinCargar = respuesta.AniosSinCargar || "";

    $('#frmTxtTiempoF').val(feriados);
    $('#frmTxtTiempoDiasV').val(naturales - feriados);

    if (sinCargar !== "") {
        /* La tabla de feriados se carga a mano y hoy no hay pantalla que la
           mantenga. Un año sin cargar devuelve 0 feriados: sin este aviso el
           colaborador gastaría días de más sin enterarse. */
        $msg.text("No hay feriados cargados para " + sinCargar + ". Verifique con Talento Humano antes de enviar.")
            .addClass("text-danger");
    }
    else if (feriados > 0) {
        $msg.text(feriados == 1
            ? "1 feriado en el rango; no descuenta de sus vacaciones."
            : feriados + " feriados en el rango; no descuentan de sus vacaciones.");
    }
    else {
        $msg.text("");
    }
}

/* Pide los feriados del rango y actualiza la pantalla. Llama a onListo cuando
   "Días" ya tiene su valor definitivo: el saldo se recarga ahí y no antes, o
   mostraría un total que no corresponde. */
function FeriadosConsultar(desde, hasta, naturales, onListo) {
    var datos = JSON.stringify([{
        "action": "ContarFeriadosRango",
        "parameters": { "fechaDesde": desde, "fechaHasta": hasta }
    }]);

    $.ajax({
        type: "POST",
        url: "ObtenerListaTareas.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) {
            FeriadosAplicarRespuesta(respuesta, naturales);
            if (typeof onListo == "function") { onListo(); }
        },
        error: function () {
            /* Sin respuesta se cobran los días completos, igual que cuando
               alguien dejaba el campo en 0. Se avisa, porque callarlo le
               costaría días al colaborador. */
            $('#frmTxtTiempoF').val("0");
            $('#frmTxtFeriadosMsg')
                .text("No se pudo calcular los feriados. Se están cobrando los días completos.")
                .addClass("text-danger");
            if (typeof onListo == "function") { onListo(); }
        }
    });
}

/* Avisa cuando los días pedidos no alcanzan con el saldo disponible.

   Solo informa: enviar y aprobar siguen funcionando igual que antes. Hoy el jefe
   aprueba sin ver esta cuenta, y bloquearlo de entrada corre el riesgo de frenar
   aprobaciones legítimas por un error de cálculo. Primero que el número se vea
   contra casos reales; el bloqueo viene después. */
function FeriadosAvisarSaldo(diasSolicitados, saldoRestante) {
    var $aviso = $('#IdAvisoSaldo');
    if ($aviso.length === 0) { return; }

    if (saldoRestante >= 0) {
        $aviso.hide().text("");
        return;
    }

    var faltan = Math.abs(saldoRestante);
    $aviso
        .text("Está solicitando " + diasSolicitados.toFixed(2) +
              " días y le faltan " + faltan.toFixed(2) +
              " para cubrirlos. Puede enviar la solicitud, pero su jefe inmediato " +
              "tendrá que autorizar el adelanto o ajustar las fechas.")
        .show();
}
