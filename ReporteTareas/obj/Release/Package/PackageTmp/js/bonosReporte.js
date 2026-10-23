/* ==========================================================================
   Reporte mensual de bonos — BonosReporte.aspx
   Handler: AdministrarBonos.ashx (ListarMeses); Excel: DescargarBonos.ashx.
   ========================================================================== */

var brNombresMes = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];

$(document).ready(function () {
    CargarMeses();
});

function Mostrar(sel, texto) {
    var $e = $(sel);
    if (texto) { $e.text(texto).show(); } else { $e.hide().text(''); }
}

function Llamar(accion, parametros, ok, fallo) {
    $.ajax({
        type: 'POST',
        url: 'AdministrarBonos.ashx',
        data: JSON.stringify([{ action: accion, parameters: parametros }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') { Fallar((r && r.mensaje) || 'No se pudo completar la operación.', fallo); return; }
            Mostrar('#brError', '');
            ok(r.resultado);
        },
        error: function (x) {
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            Fallar((r && r.mensaje) || 'No se pudo comunicar con el servidor.', fallo);
        }
    });
}

function Fallar(mensaje, fallo) {
    if (fallo) { fallo(mensaje); } else { Mostrar('#brError', mensaje); }
}

/* 3195 -> "$ 3.195,00" (formato es-EC, sin depender del idioma del navegador). */
function Dinero(v) {
    var n = Number(v);
    if (v === null || v === undefined || isNaN(n)) { return '—'; }
    var partes = Math.abs(n).toFixed(2).split('.');
    var ent = partes[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    return (n < 0 ? '-' : '') + '$ ' + ent + ',' + partes[1];
}

/* Fraccion con signo: -0.044 -> "-4,4 %"; null -> "—". */
function Variacion(f) {
    if (f === null || f === undefined) { return '—'; }
    var n = Number(f) * 100;
    if (isNaN(n)) { return '—'; }
    var t = Math.abs(n).toFixed(1).replace('.', ',');
    if (t === '0,0') { return '0,0 %'; }
    return (n < 0 ? '-' : '+') + t + ' %';
}

function NombreMes(m) {
    return brNombresMes[Number(m) - 1] || String(m);
}

function CargarMeses() {
    Llamar('ListarMeses', {}, function (lista) {
        var meses = (lista || []).slice().sort(function (a, b) {
            return (b.anio - a.anio) || (b.mes - a.mes);
        });
        DibujarMeses(meses);
    });
}

function DibujarMeses(meses) {
    var $t = $('#brMeses').empty();
    if (!meses.length) {
        $('<tr><td colspan="6" class="text-muted">No hay meses registrados.</td></tr>').appendTo($t);
        return;
    }
    $.each(meses, function (i, x) {
        var $tr = $('<tr/>').appendTo($t);
        $('<td/>').text(x.anio).appendTo($tr);
        $('<td/>').text(NombreMes(x.mes)).appendTo($tr);
        $('<td/>').text(x.estado === 'Historico' ? 'Histórico' : x.estado).appendTo($tr);
        $('<td class="br-num"/>').text(Dinero(x.total)).appendTo($tr);
        $('<td class="br-num"/>').text(Variacion(x.variacion)).appendTo($tr);
        var $acc = $('<td class="br-num"/>').appendTo($tr);
        var texto = x.estado === 'Cerrado' ? 'Excel para Nómina' : (x.estado === 'Abierto' ? 'Excel provisional' : null);
        if (texto) {
            $('<a class="btn btn-sm btn-default"/>')
                .attr('href', 'DescargarBonos.ashx?idPeriodo=' + encodeURIComponent(x.idPeriodo))
                .text(texto).appendTo($acc);
        }
    });
}
