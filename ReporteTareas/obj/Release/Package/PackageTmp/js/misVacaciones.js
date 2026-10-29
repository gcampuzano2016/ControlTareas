/* ==========================================================================
   Mis vacaciones - MisVacaciones.aspx
   Handler: AdministrarMisVacaciones.ashx (ConsultarMisVacaciones, sin
   parametros: el servidor responde con el saldo del usuario de la sesion).
   ========================================================================== */

$(document).ready(function () {
    $.ajax({
        type: 'POST',
        url: 'AdministrarMisVacaciones.ashx',
        data: JSON.stringify([{ action: 'ConsultarMisVacaciones', parameters: {} }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            $('#mvCargando').hide();
            if (!r || r.estado !== '1') { Mostrar('#mvError', (r && r.mensaje) || 'No se pudo consultar su saldo.'); return; }
            Pintar(r.resultado);
        },
        error: function (x) {
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            $('#mvCargando').hide();
            Mostrar('#mvError', (r && r.mensaje) || 'No se pudo comunicar con el servidor.');
        }
    });
});

function Mostrar(sel, texto) {
    var $e = $(sel);
    if (texto) { $e.text(texto).show(); } else { $e.hide().text(''); }
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

function Celda($tr, texto, clase) {
    return $('<td/>').addClass(clase || '').text(texto).appendTo($tr);
}

function CeldaNum($tr, v) {
    var $td = Celda($tr, Num(v), 'mv-num');
    if (Number(v) < 0) { $td.addClass('mv-neg'); }
    return $td;
}

function Dato($c, etiqueta, valor) {
    $('<div class="mv-dato"/>').append($('<b/>').text(etiqueta)).append(document.createTextNode(' ' + valor)).appendTo($c);
}

function Pintar(r) {
    var p = r.persona, s = r.saldo;

    var $per = $('#mvPersona').empty();
    Dato($per, 'Nombre:', p.Nombre);
    Dato($per, 'Cargo:', (p.Cargo || '—') + (p.Area ? ' · ' + p.Area : ''));
    Dato($per, 'Fecha de ingreso:', FechaTexto(p.ingreso) + ' (' + p.antiguedad + ')');

    var $ani = $('#mvAniversario').empty();
    Dato($ani, 'Días por año hoy:', Num(s.DiasPorAnio, 0));
    Dato($ani, 'Periodo en curso:', 'desde el ' + FechaTexto(s.inicioPeriodo) + ' hasta el ' + FechaTexto(s.proximoAniversario));
    if (p.corte) { Dato($ani, 'Saldo de corte:', Num(p.SaldoCorte) + ' días al ' + FechaTexto(p.corte)); }

    $('#indSaldo').text(Num(s.Saldo) + ' días').toggleClass('mv-neg', Number(s.Saldo) < 0);
    $('#indFecha').text(Number(s.Saldo) < 0 ? 'anticipo al ' + FechaTexto(r.fechaCalculo) : 'al ' + FechaTexto(r.fechaCalculo));
    $('#indDisponible').text(Num(s.Disponible));
    $('#indProporcional').text(Num(s.Proporcional));
    $('#indUsado').text(Num(s.UsadoEnCurso));
    $('#indPorVencer').text(Num(s.PorVencer));
    $('#indVence').text(Number(s.PorVencer) > 0 ? 'vencen el ' + FechaTexto(s.proximoVencimiento) : 'en los próximos 90 días');
    $('#mvMensaje').text(s.mensaje);

    var $bp = $('#tblPeriodos tbody').empty();
    $.each(r.periodos || [], function (i, x) {
        var $tr = $('<tr/>').appendTo($bp);
        if (x.Estado === 'FUTURO' || x.Estado === 'CERRADO AL CORTE') { $tr.addClass('mv-tenue'); }
        Celda($tr, x.Periodo);
        Celda($tr, FechaTexto(x.inicio));
        Celda($tr, FechaTexto(x.fin));
        CeldaNum($tr, x.DiasGanados);
        CeldaNum($tr, x.DiasTomados);
        CeldaNum($tr, x.DiasVencidos);
        CeldaNum($tr, x.Saldo).css('font-weight', 'bold');
        Celda($tr, FechaTexto(x.vence));
        Celda($tr, x.Estado);
    });

    var $bs = $('#tblSolicitudes tbody').empty();
    var lista = (r.solicitudes || []).slice().reverse();
    if (!lista.length) {
        $('<tr><td colspan="9" class="text-muted">No tiene solicitudes que descuenten.</td></tr>').appendTo($bs);
    }
    $.each(lista, function (i, x) {
        var $tr = $('<tr/>').appendTo($bs);
        Celda($tr, FechaTexto(x.desde));
        Celda($tr, FechaTexto(x.hasta));
        Celda($tr, x.Tipo + (x.BeneficioDos ? ' · Beneficio DOS' : ''));
        Celda($tr, x.Estado);
        Celda($tr, x.Tipo === 'PERMISO POR HORAS'
            ? (Number(x.Horas) || 0) + ' h' + (Number(x.Minutos) ? ' ' + x.Minutos + ' min' : '')
            : (x.Dias === null ? '—' : Num(x.Dias) + ' d'), 'mv-num');
        CeldaNum($tr, x.DiasFinDeSemana);
        CeldaNum($tr, x.DiasDescuenta).css('font-weight', 'bold');
        Celda($tr, x.periodo);
        Celda($tr, x.Observacion || '', 'mv-texto');
    });
    Mostrar('#mvOtras', r.otras > 0
        ? 'No se listan ' + r.otras + (r.otras === 1 ? ' solicitud registrada que no descuenta' : ' solicitudes registradas que no descuentan') +
          ' (dadas de baja, sin fecha, o anteriores al saldo de corte o al reingreso).'
        : '');

    $('#mvContenido').show();
}
