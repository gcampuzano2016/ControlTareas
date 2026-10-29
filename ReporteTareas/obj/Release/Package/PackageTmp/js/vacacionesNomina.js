/* ==========================================================================
   Nomina de vacaciones (GTH) - VacacionesNomina.aspx
   Handler: AdministrarVacNomina.ashx (ConsultarNomina). Solo la fecha va al
   servidor; los demas filtros se aplican aqui.
   ========================================================================== */

var vnFilas = [];
var vnFecha = '';
var vnOrden = { campo: 'Nombre', asc: true };

var VN_NUMERICOS = ['Sueldo', 'ValorDia', 'disponible', 'proporcional', 'usadoEnCurso', 'dias', 'Valor'];

$(document).ready(function () {
    $('#txtFecha').val(Hoy());
    $('#btnConsultar').on('click', Consultar);
    $('#cboColaborador, #cboEmpresa, #cboSueldo').on('change', Pintar);
    $('#txtBuscar').on('input', Pintar);
    $('#btnDescargar').on('click', Descargar);
    $('#tblNomina thead').on('click', 'th.vn-orden', function () {
        var campo = $(this).data('campo');
        vnOrden = { campo: campo, asc: vnOrden.campo === campo ? !vnOrden.asc : true };
        Pintar();
    });
    Consultar();
});

function Mostrar(sel, texto) {
    var $e = $(sel);
    if (texto) { $e.text(texto).show(); } else { $e.hide().text(''); }
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

function Celda($tr, texto, clase) {
    return $('<td/>').addClass(clase || '').text(texto).appendTo($tr);
}

function CeldaNum($tr, v) {
    var $td = Celda($tr, Num(v), 'vn-num');
    if (Number(v) < 0) { $td.addClass('vn-neg'); }
    return $td;
}

function Consultar() {
    var fecha = $('#txtFecha').val();
    if (!fecha) { Mostrar('#vnError', 'Indique la fecha de cálculo.'); return; }
    $('#btnConsultar').prop('disabled', true);
    $.ajax({
        type: 'POST',
        url: 'AdministrarVacNomina.ashx',
        data: JSON.stringify([{ action: 'ConsultarNomina', parameters: { fecha: fecha } }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') { Mostrar('#vnError', (r && r.mensaje) || 'No se pudo consultar la nómina.'); return; }
            Mostrar('#vnError', '');
            Cargar(r.resultado);
        },
        error: function (x) {
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            Mostrar('#vnError', (r && r.mensaje) || 'No se pudo comunicar con el servidor.');
        },
        complete: function () { $('#btnConsultar').prop('disabled', false); }
    });
}

function Cargar(r) {
    vnFilas = r.filas || [];
    vnFecha = r.fechaCalculo;
    $('#lblFecha').text(FechaTexto(vnFecha));
    Mostrar('#vnAvisos', (r.avisos || []).join(' '));

    $('#indDias').text(Num(r.TotalDias));
    $('#indActivos').text(r.Activos + ' activos');
    $('#indValor').text('$ ' + Num(r.TotalValor));
    $('#indAnticipos').text('$ ' + Num(r.ValorAnticipos));
    $('#indSinSueldo').text(r.SinSueldo + (r.SinSueldo === 1 ? ' persona' : ' personas'));

    var $e = $('#tblEmpresas tbody').empty();
    var sumP = 0, sumD = 0, sumV = 0, sumS = 0;
    $.each(r.PorEmpresa || [], function (i, x) {
        var $tr = $('<tr/>').appendTo($e);
        Celda($tr, x.Empresa);
        Celda($tr, x.Personas, 'vn-num');
        CeldaNum($tr, x.Dias);
        CeldaNum($tr, x.Valor);
        Celda($tr, x.SinSueldo, 'vn-num');
        sumP += x.Personas; sumD += Number(x.Dias); sumV += Number(x.Valor); sumS += x.SinSueldo;
    });
    var $t = $('<tr style="font-weight: bold; background: #f5f5f5"/>').appendTo($e);
    Celda($t, 'Total');
    Celda($t, sumP, 'vn-num');
    CeldaNum($t, sumD);
    CeldaNum($t, sumV);
    Celda($t, sumS, 'vn-num');

    var actual = $('#cboEmpresa').val();
    var empresas = {};
    $.each(vnFilas, function (i, f) { if (f.Empresa) { empresas[f.Empresa] = true; } });
    var $c = $('#cboEmpresa').empty().append($('<option value=""/>').text('Todas'));
    $.each(Object.keys(empresas).sort(), function (i, a) { $('<option/>').val(a).text(a).appendTo($c); });
    $c.val(empresas[actual] ? actual : '');

    Pintar();
}

function Filtradas() {
    var col = $('#cboColaborador').val();
    var emp = $('#cboEmpresa').val();
    var sue = $('#cboSueldo').val();
    var q = $.trim($('#txtBuscar').val()).toLowerCase();
    var lista = $.grep(vnFilas, function (f) {
        if (col && f.estado !== col) { return false; }
        if (emp && f.Empresa !== emp) { return false; }
        if (sue === 'FALTA' && f.Sueldo !== null) { return false; }
        if (sue === 'ANTICIPO' && !(Number(f.dias) < 0)) { return false; }
        if (q && (f.Nombre || '').toLowerCase().indexOf(q) < 0 && (f.Cedula || '').indexOf(q) < 0) { return false; }
        return true;
    });
    var campo = vnOrden.campo;
    var numerico = VN_NUMERICOS.indexOf(campo) >= 0;
    lista.sort(function (a, b) {
        var x = a[campo], y = b[campo], c;
        if (numerico) {
            c = (x === null ? -Infinity : Number(x)) - (y === null ? -Infinity : Number(y));
        } else {
            c = String(x || '').localeCompare(String(y || ''), 'es', { sensitivity: 'base' });
        }
        if (c === 0) { c = String(a.Nombre).localeCompare(String(b.Nombre), 'es', { sensitivity: 'base' }); }
        return vnOrden.asc ? c : -c;
    });
    return lista;
}

function Pintar() {
    var lista = Filtradas();
    $('#tblNomina th.vn-orden').removeClass('vn-asc vn-desc')
        .filter(function () { return $(this).data('campo') === vnOrden.campo; })
        .addClass(vnOrden.asc ? 'vn-asc' : 'vn-desc');

    var $b = $('#tblNomina tbody').empty();
    var sumDias = 0, sumValor = 0;
    if (!lista.length) {
        $('<tr><td colspan="14" class="text-muted">No hay colaboradores con esos filtros.</td></tr>').appendTo($b);
    }
    $.each(lista, function (i, f) {
        var $tr = $('<tr/>').appendTo($b);
        if (f.Sueldo === null) { $tr.addClass('vn-falta-sueldo'); }
        Celda($tr, f.Nombre + (f.estado === 'INACTIVO' ? ' (inactivo)' : ''));
        Celda($tr, f.Cedula);
        Celda($tr, f.Empresa || '—');
        Celda($tr, FechaTexto(f.ingreso));
        Celda($tr, FechaTexto(f.fechaSaldo));
        CeldaNum($tr, f.Sueldo);
        CeldaNum($tr, f.ValorDia);
        CeldaNum($tr, f.disponible);
        CeldaNum($tr, f.proporcional);
        CeldaNum($tr, f.usadoEnCurso);
        CeldaNum($tr, f.dias).css('font-weight', 'bold');
        CeldaNum($tr, f.Valor).css('font-weight', 'bold');
        Celda($tr, f.FuenteSueldo || '—');
        Celda($tr, f.Observacion || '');
        sumDias += Number(f.dias) || 0;
        sumValor += Number(f.Valor) || 0;
    });

    var $f = $('#tblNomina tfoot').empty();
    if (lista.length) {
        var $tr = $('<tr style="font-weight: bold; background: #f5f5f5"/>').appendTo($f);
        $('<td colspan="10"/>').text('Total de lo filtrado (' + lista.length + ')').appendTo($tr);
        CeldaNum($tr, sumDias);
        CeldaNum($tr, sumValor);
        $('<td colspan="2"/>').appendTo($tr);
    }
    $('#lblConteo').text('(' + lista.length + ' de ' + vnFilas.length + ')');
}

/* CSV con ";" y BOM: asi lo abre Excel en es-EC con tildes y decimales con coma. */
function Descargar() {
    var lista = Filtradas();
    if (!lista.length) { Mostrar('#vnError', 'No hay nada que descargar con esos filtros.'); return; }
    var lineas = [['Cédula', 'Colaborador', 'Empresa', 'Área', 'Estado', 'Fecha de ingreso', 'Fecha de salida', 'Calculado al',
                   'Sueldo mensual', 'Valor día', 'Días periodos cumplidos', 'Proporcional acumulado', 'Usado del periodo en curso',
                   'Días a liquidar / provisionar', 'Valor ($)', 'Fuente del sueldo', 'Observación']];
    $.each(lista, function (i, f) {
        lineas.push([f.Cedula, f.Nombre, f.Empresa, f.Area, f.estado, FechaTexto(f.ingreso), f.salida ? FechaTexto(f.salida) : '',
            FechaTexto(f.fechaSaldo), NumCsv(f.Sueldo), NumCsv(f.ValorDia), NumCsv(f.disponible), NumCsv(f.proporcional),
            NumCsv(f.usadoEnCurso), NumCsv(f.dias), NumCsv(f.Valor), f.FuenteSueldo, f.Observacion]);
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
    a.download = 'nomina-vacaciones-' + vnFecha + '.csv';
    document.body.appendChild(a);
    a.click();
    setTimeout(function () { URL.revokeObjectURL(a.href); document.body.removeChild(a); }, 0);
}

function NumCsv(v) {
    if (v === null || v === undefined || v === '') { return ''; }
    return String(Number(v)).replace('.', ',');
}
