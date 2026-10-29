/* ==========================================================================
   Saldos de vacaciones (GTH) — VacacionesSaldos.aspx
   Handler: AdministrarVacSaldos.ashx (ConsultarSaldos, ConsultarFicha).
   PDF del estado individual: DescargarVacEstado.ashx.
   El calculo es de Sp_RTA_VacCalcular; aqui solo se filtra, ordena y pinta.
   Los filtros (colaboradores, estado, area, buscar) no vuelven al servidor:
   solo la fecha de calculo.
   ========================================================================== */

var vsFilas = [];
var vsFecha = '';
var vsOrden = { campo: 'Nombre', asc: true };
var vsElegido = null;

var VS_NUMERICOS = ['cumplidos', 'diasPorAnio', 'disponible', 'proporcional', 'usadoEnCurso', 'saldo', 'porVencer'];

$(document).ready(function () {
    $('#txtFecha').val(Hoy());
    $('#btnConsultar').on('click', Consultar);
    $('#cboColaborador, #cboEstado, #cboArea').on('change', Pintar);
    $('#txtBuscar').on('input', Pintar);
    $('#btnDescargar').on('click', Descargar);
    $('#btnCerrarFicha').on('click', CerrarFicha);
    $('#tblSaldos thead').on('click', 'th.vs-orden', function () {
        var campo = $(this).data('campo');
        vsOrden = { campo: campo, asc: vsOrden.campo === campo ? !vsOrden.asc : true };
        Pintar();
    });
    $('#tblSaldos tbody').on('click', 'tr[data-id]', function () {
        AbrirFicha(Number($(this).data('id')));
    });
    Consultar();
});

// ------------------------------------------------------------ utilidades

function Mostrar(sel, texto) {
    var $e = $(sel);
    if (texto) { $e.text(texto).show(); } else { $e.hide().text(''); }
}

function Llamar(accion, parametros, ok, fin) {
    $.ajax({
        type: 'POST',
        url: 'AdministrarVacSaldos.ashx',
        data: JSON.stringify([{ action: accion, parameters: parametros }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') { Mostrar('#vsError', (r && r.mensaje) || 'No se pudo completar la consulta.'); return; }
            Mostrar('#vsError', '');
            ok(r.resultado);
        },
        error: function (x) {
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            Mostrar('#vsError', (r && r.mensaje) || 'No se pudo comunicar con el servidor.');
        },
        complete: function () { if (fin) { fin(); } }
    });
}

function Hoy() {
    var d = new Date();
    return d.getFullYear() + '-' + ('0' + (d.getMonth() + 1)).slice(-2) + '-' + ('0' + d.getDate()).slice(-2);
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

function Etiqueta(estado) {
    var clase = { 'ANTICIPO': 'ANTICIPO', 'POR VENCER': 'POR-VENCER', 'ACUMULANDO': 'ACUMULANDO', 'AL DIA': 'AL-DIA' }[estado] || 'otro';
    var texto = estado === 'AL DIA' ? 'AL DÍA' : (estado || '—');
    return $('<span class="vs-etiqueta"/>').addClass('vs-e-' + clase).text(texto);
}

function Celda($tr, texto, clase) {
    return $('<td/>').addClass(clase || '').text(texto).appendTo($tr);
}

function CeldaNum($tr, v, decimales) {
    var $td = Celda($tr, Num(v, decimales), 'vs-num');
    if (Number(v) < 0) { $td.addClass('vs-neg'); }
    return $td;
}

// ------------------------------------------------------------ tablero

function Consultar() {
    var fecha = $('#txtFecha').val();
    if (!fecha) { Mostrar('#vsError', 'Indique la fecha de cálculo.'); return; }
    $('#btnConsultar').prop('disabled', true);
    Llamar('ConsultarSaldos', { fecha: fecha }, function (r) {
        vsFilas = r.filas || [];
        vsFecha = r.fechaCalculo;
        $('#lblFecha').text(FechaTexto(vsFecha));
        Mostrar('#vsAvisos', (r.avisos || []).join(' '));
        PintarResumen(r.resumen || {});
        LlenarAreas();
        Pintar();
        if (vsElegido) { AbrirFicha(vsElegido); }
    }, function () { $('#btnConsultar').prop('disabled', false); });
}

function PintarResumen(r) {
    $('#indSaldo').text(Num(r.SaldoTotal) + ' días');
    $('#indActivos').text(r.Activos + ' activos' + (r.Inactivos ? ' (' + r.Inactivos + ' inactivos aparte)' : ''));
    $('#indPorVencer').text(Num(r.DiasPorVencer) + ' días');
    $('#indPorVencerP').text(r.PersonasPorVencer + (r.PersonasPorVencer === 1 ? ' persona' : ' personas'));
    $('#indAnticipo').text(Num(r.DiasAnticipo) + ' días');
    $('#indAnticipoP').text(r.PersonasAnticipo + (r.PersonasAnticipo === 1 ? ' persona' : ' personas'));
    $('#indAcumula').text(r.PersonasAcumulando + (r.PersonasAcumulando === 1 ? ' persona' : ' personas'));
    $('#indVencidos').text(Num(r.DiasVencidos) + ' días');
}

function LlenarAreas() {
    var actual = $('#cboArea').val();
    var areas = {};
    $.each(vsFilas, function (i, f) { if (f.Area) { areas[f.Area] = true; } });
    var $c = $('#cboArea').empty().append($('<option value=""/>').text('Todas'));
    $.each(Object.keys(areas).sort(function (a, b) { return a.localeCompare(b, 'es'); }), function (i, a) {
        $('<option/>').val(a).text(a).appendTo($c);
    });
    $c.val(areas[actual] ? actual : '');
}

function Filtradas() {
    var col = $('#cboColaborador').val();
    var est = $('#cboEstado').val();
    var area = $('#cboArea').val();
    var q = $.trim($('#txtBuscar').val()).toLowerCase();
    var lista = $.grep(vsFilas, function (f) {
        if (col && f.estadoColaborador !== col) { return false; }
        if (est && f.estadoSaldo !== est) { return false; }
        if (area && f.Area !== area) { return false; }
        if (q && (f.Nombre || '').toLowerCase().indexOf(q) < 0 && (f.Cedula || '').indexOf(q) < 0) { return false; }
        return true;
    });
    var campo = vsOrden.campo;
    var numerico = VS_NUMERICOS.indexOf(campo) >= 0;
    lista.sort(function (a, b) {
        var x = a[campo], y = b[campo], c;
        if (numerico) {
            c = (x === null ? -Infinity : Number(x)) - (y === null ? -Infinity : Number(y));
        } else {
            c = String(x || '').localeCompare(String(y || ''), 'es', { sensitivity: 'base' });
        }
        if (c === 0) { c = String(a.Nombre).localeCompare(String(b.Nombre), 'es', { sensitivity: 'base' }); }
        return vsOrden.asc ? c : -c;
    });
    return lista;
}

function Pintar() {
    var lista = Filtradas();
    $('#tblSaldos th.vs-orden').removeClass('vs-asc vs-desc')
        .filter(function () { return $(this).data('campo') === vsOrden.campo; })
        .addClass(vsOrden.asc ? 'vs-asc' : 'vs-desc');

    var $b = $('#tblSaldos tbody').empty();
    var suma = { disponible: 0, proporcional: 0, usadoEnCurso: 0, saldo: 0, porVencer: 0 };
    if (!lista.length) {
        $('<tr><td colspan="13" class="text-muted">No hay colaboradores con esos filtros.</td></tr>').appendTo($b);
    }
    $.each(lista, function (i, f) {
        var $tr = $('<tr/>').attr('data-id', f.IdColaborador).appendTo($b);
        if (f.IdColaborador === vsElegido) { $tr.addClass('vs-elegida'); }
        Celda($tr, f.Nombre + (f.estadoColaborador === 'INACTIVO' ? ' (inactivo)' : ''));
        Celda($tr, f.Cedula);
        Celda($tr, f.Area || '—');
        Celda($tr, FechaTexto(f.ingreso));
        CeldaNum($tr, f.cumplidos, 0);
        CeldaNum($tr, f.diasPorAnio, 0);
        CeldaNum($tr, f.disponible);
        CeldaNum($tr, f.proporcional);
        CeldaNum($tr, f.usadoEnCurso);
        CeldaNum($tr, f.saldo).css('font-weight', 'bold');
        CeldaNum($tr, f.porVencer);
        Celda($tr, Number(f.porVencer) > 0 ? FechaTexto(f.proximoVencimiento) : '—');
        $('<td/>').append(Etiqueta(f.estadoSaldo)).appendTo($tr);
        $.each(suma, function (k) { suma[k] += Number(f[k]) || 0; });
    });

    var $f = $('#tblSaldos tfoot').empty();
    if (lista.length) {
        var $tr = $('<tr style="font-weight: bold; background: #f5f5f5"/>').appendTo($f);
        $('<td colspan="6"/>').text('Total de lo filtrado (' + lista.length + ')').appendTo($tr);
        CeldaNum($tr, suma.disponible);
        CeldaNum($tr, suma.proporcional);
        CeldaNum($tr, suma.usadoEnCurso);
        CeldaNum($tr, suma.saldo);
        CeldaNum($tr, suma.porVencer);
        $('<td colspan="2"/>').appendTo($tr);
    }
    $('#lblConteo').text('(' + lista.length + ' de ' + vsFilas.length + ')');
}

/* CSV con ";" y BOM: asi lo abre Excel en es-EC con tildes y decimales con coma. */
function Descargar() {
    var lista = Filtradas();
    if (!lista.length) { Mostrar('#vsError', 'No hay nada que descargar con esos filtros.'); return; }
    var cab = ['Nombre', 'Cédula', 'Empresa', 'Área', 'Cargo', 'Estado colaborador', 'Ingreso', 'Años cumplidos', 'Días por año',
               'Disponible', 'Proporcional', 'Usado en curso', 'Saldo', 'Por vencer', 'Próximo vencimiento', 'Vencidos', 'Total gozado', 'Estado del saldo'];
    var lineas = [cab];
    $.each(lista, function (i, f) {
        lineas.push([f.Nombre, f.Cedula, f.Empresa, f.Area, f.Cargo, f.estadoColaborador, FechaTexto(f.ingreso),
            f.cumplidos, NumCsv(f.diasPorAnio), NumCsv(f.disponible), NumCsv(f.proporcional), NumCsv(f.usadoEnCurso),
            NumCsv(f.saldo), NumCsv(f.porVencer), Number(f.porVencer) > 0 ? FechaTexto(f.proximoVencimiento) : '',
            NumCsv(f.vencidos), NumCsv(f.totalGozado), f.estadoSaldo]);
    });
    var texto = lineas.map(function (l) {
        return l.map(function (v) {
            var s = v === null || v === undefined ? '' : String(v);
            return /[";\n\r]/.test(s) ? '"' + s.replace(/"/g, '""') + '"' : s;
        }).join(';');
    }).join('\r\n');
    var blob = new Blob(['﻿' + texto], { type: 'text/csv;charset=utf-8' });
    var a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = 'saldos-vacaciones-' + vsFecha + '.csv';
    document.body.appendChild(a);
    a.click();
    setTimeout(function () { URL.revokeObjectURL(a.href); document.body.removeChild(a); }, 0);
}

function NumCsv(v) {
    if (v === null || v === undefined || v === '') { return ''; }
    return String(Number(v)).replace('.', ',');
}

// ------------------------------------------------------------ ficha

function AbrirFicha(id) {
    vsElegido = id;
    $('#tblSaldos tbody tr').removeClass('vs-elegida').filter('[data-id="' + id + '"]').addClass('vs-elegida');
    Llamar('ConsultarFicha', { fecha: vsFecha || $('#txtFecha').val(), idColaborador: id }, function (r) {
        PintarFicha(r);
        $('#vsFicha').show();
        $('html, body').animate({ scrollTop: $('#vsFicha').offset().top - 60 }, 200);
    });
}

function CerrarFicha() {
    vsElegido = null;
    $('#vsFicha').hide();
    $('#tblSaldos tbody tr').removeClass('vs-elegida');
}

function Dato($c, etiqueta, valor) {
    $('<div class="vs-dato"/>').append($('<b/>').text(etiqueta)).append(document.createTextNode(' ' + valor)).appendTo($c);
}

function PintarFicha(r) {
    var p = r.persona;
    /* Cada clic genera un folio nuevo en Vac_EstadoEmitido: es el documento que se imprime y firma. */
    $('#btnPdfEstado').attr('href', 'DescargarVacEstado.ashx?id=' + encodeURIComponent(p.IdColaborador) +
        '&fecha=' + encodeURIComponent(r.fechaCalculo));
    $('#fichaTitulo').text(p.Nombre + ' — ' + p.Cedula);

    var $per = $('#fichaPersona').empty();
    Dato($per, 'Empresa / Área:', (p.Empresa || '—') + ' / ' + (p.Area || '—'));
    Dato($per, 'Cargo:', p.Cargo || '—');
    Dato($per, 'Estado:', p.estadoColaborador + (p.salida ? ', salió el ' + FechaTexto(p.salida) : ''));
    Dato($per, 'Ingreso:', FechaTexto(p.ingreso) + ' (' + p.cumplidos + ' años cumplidos, ' + Num(p.diasPorAnio, 0) + ' días por año)');
    if (p.corte) { Dato($per, 'Saldo de corte:', Num(p.SaldoCorte) + ' días al ' + FechaTexto(p.corte)); }
    if (p.observacionColaborador) { Dato($per, 'Observación:', p.observacionColaborador); }

    var $s = $('#fichaSaldo').empty();
    $('<div class="vs-dato"/>').append($('<b/>').text('Estado del saldo:')).append(' ').append(Etiqueta(p.estadoSaldo)).appendTo($s);
    Dato($s, 'Disponible:', Num(p.disponible) + ' días (periodos vigentes)');
    Dato($s, 'Proporcional:', Num(p.proporcional) + ' días (periodo en curso, desde el ' + FechaTexto(p.inicioPeriodo) + ')');
    Dato($s, 'Usado en curso:', Num(p.usadoEnCurso) + ' días');
    Dato($s, 'Saldo:', Num(p.saldo) + ' días al ' + FechaTexto(p.fechaSaldo) + (p.fechaSaldo !== r.fechaCalculo ? ' (fecha de salida)' : ''));
    if (Number(p.porVencer) > 0) { Dato($s, 'Por vencer:', Num(p.porVencer) + ' días el ' + FechaTexto(p.proximoVencimiento)); }
    Dato($s, 'Vencidos:', Num(p.vencidos) + ' días · Total gozado: ' + Num(p.totalGozado) + ' días');

    var $bp = $('#tblPeriodos tbody').empty();
    $.each(r.periodos || [], function (i, x) {
        var $tr = $('<tr/>').appendTo($bp);
        if (x.Estado === 'FUTURO' || x.Estado === 'CERRADO AL CORTE') { $tr.addClass('vs-tenue'); }
        CeldaNum($tr, x.N, 0);
        Celda($tr, x.Periodo);
        Celda($tr, FechaTexto(x.inicio));
        Celda($tr, FechaTexto(x.fin));
        CeldaNum($tr, x.DiasGanados);
        CeldaNum($tr, x.DiasTomados);
        CeldaNum($tr, x.DiasVencidos);
        CeldaNum($tr, x.Saldo);
        Celda($tr, FechaTexto(x.vence));
        Celda($tr, x.Estado);
    });

    var $bm = $('#tblMovimientos tbody').empty();
    if (!(r.movimientos || []).length) {
        $('<tr><td colspan="11" class="text-muted">Sin solicitudes registradas.</td></tr>').appendTo($bm);
    }
    $.each(r.movimientos || [], function (i, x) {
        var $tr = $('<tr/>').appendTo($bm);
        if (x.Situacion !== 'DESCUENTA') { $tr.addClass('vs-tenue'); }
        Celda($tr, x.Tipo + (x.BeneficioDos ? ' · Beneficio DOS' : ''));
        Celda($tr, x.Estado);
        Celda($tr, FechaTexto(x.desde));
        Celda($tr, FechaTexto(x.hasta));
        Celda($tr, Pedido(x), 'vs-num');
        Celda($tr, x.AnioServicio === null ? '—' : x.AnioServicio, 'vs-num');
        CeldaNum($tr, x.DiasBase);
        CeldaNum($tr, x.DiasFinDeSemana);
        CeldaNum($tr, x.DiasDescuenta).css('font-weight', 'bold');
        Celda($tr, Situacion(x.Situacion) + (x.Cruces > 0 ? ' · se cruza con ' + x.Cruces : ''));
        Celda($tr, x.Observacion || '', 'vs-texto');
    });
}

function Pedido(x) {
    if (x.Tipo === 'PERMISO POR HORAS') {
        return (Number(x.Horas) || 0) + ' h' + (Number(x.Minutos) ? ' ' + x.Minutos + ' min' : '');
    }
    return x.Dias === null ? '—' : Num(x.Dias) + ' d';
}

function Situacion(s) {
    return {
        'DESCUENTA': 'Descuenta',
        'SIN FECHA': 'Sin fecha',
        'ANULADO': 'Dada de baja',
        'RELACION ANTERIOR': 'Antes del reingreso',
        'ANTES DEL CORTE': 'Antes del saldo de corte',
        'POSTERIOR': 'Posterior a la fecha de cálculo'
    }[s] || s;
}
