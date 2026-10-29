/* ==========================================================================
   Liquidacion de vacaciones (GTH) - VacacionesLiquidacion.aspx
   Handler: AdministrarVacLiquidacion.ashx (ListarColaboradores, Calcular).
   Solo simula: no registra nada.
   ========================================================================== */

var vlPersonas = [];

$(document).ready(function () {
    Llamar('ListarColaboradores', {}, function (lista) {
        vlPersonas = lista || [];
        LlenarPersonas();
    });
    $('#txtBuscar').on('input', LlenarPersonas);
    $('#cboColaborador').on('change', function () {
        var p = Persona($(this).val());
        $('#txtFecha').val(p ? p.fechaPorDefecto : '');
    });
    $('#btnCalcular').on('click', Calcular);
});

function Mostrar(sel, texto) {
    var $e = $(sel);
    if (texto) { $e.text(texto).show(); } else { $e.hide().text(''); }
}

function Llamar(accion, parametros, ok, fin) {
    $.ajax({
        type: 'POST',
        url: 'AdministrarVacLiquidacion.ashx',
        data: JSON.stringify([{ action: accion, parameters: parametros }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') { Mostrar('#vlError', (r && r.mensaje) || 'No se pudo completar la operación.'); return; }
            Mostrar('#vlError', '');
            ok(r.resultado);
        },
        error: function (x) {
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            Mostrar('#vlError', (r && r.mensaje) || 'No se pudo comunicar con el servidor.');
        },
        complete: function () { if (fin) { fin(); } }
    });
}

function Persona(id) {
    for (var i = 0; i < vlPersonas.length; i++) { if (String(vlPersonas[i].IdColaborador) === String(id)) { return vlPersonas[i]; } }
    return null;
}

function LlenarPersonas() {
    var q = $.trim($('#txtBuscar').val()).toLowerCase();
    var actual = $('#cboColaborador').val();
    var $c = $('#cboColaborador').empty().append($('<option value=""/>').text('Elija...'));
    $.each(vlPersonas, function (i, p) {
        if (q && (p.Nombre || '').toLowerCase().indexOf(q) < 0 && (p.Cedula || '').indexOf(q) < 0) { return; }
        $('<option/>').val(p.IdColaborador).text(p.Nombre + (p.Estado === 'INACTIVO' ? ' (inactivo)' : '')).appendTo($c);
    });
    if (actual && $c.find('option[value="' + actual + '"]').length) { $c.val(actual); }
}

/* "2026-10-28" -> "28/10/2026"; "" -> "—". */
function FechaTexto(iso) {
    if (!iso) { return '—'; }
    var p = String(iso).split('-');
    return p.length === 3 ? p[2] + '/' + p[1] + '/' + p[0] : String(iso);
}

/* 1234.5 -> "1.234,50" (formato es-EC, sin depender del idioma del navegador). */
function Num(v, decimales) {
    var n = Number(v);
    if (v === null || v === undefined || v === '' || isNaN(n)) { return '—'; }
    var partes = Math.abs(n).toFixed(decimales === undefined ? 2 : decimales).split('.');
    var ent = partes[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    var s = ent + (partes.length > 1 ? ',' + partes[1] : '');
    return (n < 0 && Number(s.replace(/\./g, '').replace(',', '.')) !== 0 ? '-' : '') + s;
}

/* "14.400,50" o "14400.50" -> "14400.50" para el servidor; "" si esta vacio. */
function NumeroAlServidor(texto) {
    var t = $.trim(texto || '').replace(/\s/g, '').replace(/\$/g, '');
    if (!t) { return ''; }
    if (t.indexOf(',') >= 0) { t = t.replace(/\./g, '').replace(',', '.'); }
    return t;
}

function Dato($c, etiqueta, valor) {
    $('<div class="vl-dato"/>').append($('<b/>').text(etiqueta)).append(document.createTextNode(' ' + valor)).appendTo($c);
}

function Celda($tr, texto, clase) {
    return $('<td/>').addClass(clase || '').text(texto).appendTo($tr);
}

function CeldaNum($tr, v, decimales) {
    var $td = Celda($tr, Num(v, decimales), 'vl-num');
    if (Number(v) < 0) { $td.addClass('vl-neg'); }
    return $td;
}

function Calcular() {
    var id = $('#cboColaborador').val();
    if (!id) { Mostrar('#vlError', 'Elija un colaborador.'); return; }
    $('#btnCalcular').prop('disabled', true);
    Llamar('Calcular', { idColaborador: id, fecha: $('#txtFecha').val(), ganado: NumeroAlServidor($('#txtGanado').val()) },
        Pintar, function () { $('#btnCalcular').prop('disabled', false); });
}

function Pintar(r) {
    var p = r.persona, e = r.enCurso, s = r.resumen;

    var $per = $('#vlPersona').empty();
    Dato($per, 'Colaborador:', p.Nombre);
    Dato($per, 'Cédula:', p.Cedula);
    Dato($per, 'Empresa:', p.Empresa || '—');
    Dato($per, 'Cargo:', p.Cargo || '—');
    Dato($per, 'Sueldo mensual:', r.Sueldo === null ? 'falta sueldo' : '$ ' + Num(r.Sueldo) + ' (' + r.FuenteSueldo + ')');

    var $sal = $('#vlSalida').empty();
    Dato($sal, 'Fecha de ingreso:', FechaTexto(p.ingreso));
    Dato($sal, 'Fecha de salida:', FechaTexto(r.fechaSalida) + (p.salidaRegistrada && p.salidaRegistrada !== r.fechaSalida ? ' (la registrada es ' + FechaTexto(p.salidaRegistrada) + ')' : ''));
    Dato($sal, 'Antigüedad:', r.Antiguedad);
    Dato($sal, 'Estado:', p.Estado);
    if (p.corte) { Dato($sal, 'Saldo de corte:', Num(p.SaldoCorte) + ' días al ' + FechaTexto(p.corte)); }

    var $ec = $('#vlEnCurso').empty();
    var $tr = $('<tr/>').appendTo($ec);
    Celda($tr, FechaTexto(e.inicio));
    Celda($tr, FechaTexto(e.proximoAniversario));
    Celda($tr, e.DiasTranscurridos, 'vl-num');
    CeldaNum($tr, e.DiasPorAnio, 0);
    CeldaNum($tr, e.Proporcional).css('font-weight', 'bold');
    $('#vlFormula').text('Fórmula: ' + Num(e.DiasPorAnio, 0) + ' días × ' + e.DiasTranscurridos + ' días transcurridos ÷ ' +
        e.DiasDelPeriodo + ' días del periodo = ' + Num(e.Proporcional) + ' días');

    $('#indDisponible').text(Num(s.Disponible));
    $('#indProporcional').text(Num(s.Proporcional));
    $('#indUsado').text(Num(s.UsadoEnCurso));
    $('#indDias').text(Num(s.dias)).toggleClass('vl-neg', Number(s.dias) < 0);
    $('#indDiasNota').text(Number(s.dias) < 0 ? 'anticipo: evaluar descuento' : 'cumplidos + proporcional − usado');
    $('#indValor').text(s.ValorReferencial === null ? '—' : '$ ' + Num(s.ValorReferencial));
    $('#indArt71').text(s.ValorArt71 === null ? '—' : '$ ' + Num(s.ValorArt71));
    Mostrar('#vlPosteriores', r.Posteriores > 0
        ? 'No se cuentan ' + r.Posteriores + (r.Posteriores === 1 ? ' solicitud registrada' : ' solicitudes registradas') +
          ' con fecha posterior a la salida.'
        : '');

    var $bp = $('#tblPeriodos tbody').empty();
    $.each(r.periodos || [], function (i, x) {
        var $t = $('<tr/>').appendTo($bp);
        Celda($t, x.Periodo);
        Celda($t, FechaTexto(x.inicio));
        Celda($t, FechaTexto(x.fin));
        CeldaNum($t, x.DiasGanados, 0);
        CeldaNum($t, x.DiasTomados);
        CeldaNum($t, x.saldo).css('font-weight', 'bold');
        CeldaNum($t, x.DiasVencidos);
        Celda($t, x.vence ? FechaTexto(x.vence) : '');
        Celda($t, x.estado);
    });
    Mostrar('#vlCorte', r.cerradosAlCorte > 0
        ? 'No se muestran ' + r.cerradosAlCorte + ' periodos cerrados al saldo de corte.' : '');

    var $bs = $('#tblSolicitudes tbody').empty();
    var lista = (r.solicitudes || []).slice().reverse();
    if (!lista.length) { $('<tr><td colspan="7" class="text-muted">Sin solicitudes que descuenten.</td></tr>').appendTo($bs); }
    $.each(lista, function (i, x) {
        var $t = $('<tr/>').appendTo($bs);
        Celda($t, FechaTexto(x.desde));
        Celda($t, FechaTexto(x.hasta));
        Celda($t, x.Tipo);
        Celda($t, x.Estado);
        Celda($t, x.Tipo === 'PERMISO POR HORAS'
            ? (Number(x.Horas) || 0) + ' h' + (Number(x.Minutos) ? ' ' + x.Minutos + ' min' : '')
            : (x.Dias === null ? '—' : Num(x.Dias) + ' d'), 'vl-num');
        CeldaNum($t, x.DiasDescuenta).css('font-weight', 'bold');
        Celda($t, x.periodo);
    });

    $('#vlResultado').show();
}
