/* ==========================================================================
   Tablero de Costos — TableroCostos.aspx
   Handler : AdministrarTableroCostos.ashx  (accion ConsultarTablero)
             DescargarTableroCostos.ashx    (Excel)
   Este archivo NO calcula nada: clasificacion, valoracion, filtros, totales y
   avisos llegan resueltos de NegTableroCostos. Solo dibuja.
   ========================================================================== */

var tcDatos = null;
var tcRango = '';
/* Rango de la ultima consulta correcta: es el que esta en pantalla. */
var tcDesde = '';
var tcHasta = '';
var tcGraficos = {};
var tcPagina = 0;
var TC_POR_PAGINA = 50;

/* Mismo orden que el Resumen y que las propiedades H* / V* de EntTcPeriodo. */
var TC_CATEGORIAS = ['Normal', 'Extra 50%', 'Extra 100%', 'Traslado sin recargo', 'Traslado 50%', 'Traslado 100%'];
var TC_SUFIJOS = ['Normal', 'Extra50', 'Extra100', 'TrasladoSin', 'Traslado50', 'Traslado100'];
var TC_COLORES = ['#2F6DB5', '#E07B24', '#B85A12', '#1F9E89', '#4CC3AE', '#0F6B5C'];
var TC_COLOR_TIPO = { Normal: '#2F6DB5', Extra: '#E07B24', Traslado: '#1F9E89' };

/* Posiciones de cada fila de tcDatos.Detalle (AdministrarTableroCostos.ParaPantalla). */
var D_INICIO = 1, D_RESPONSABLE = 2, D_EMPRESA = 3, D_CONTRATO = 4, D_TICKET = 5, D_TAREA = 6,
    D_ESTADO = 7, D_TIPO = 8, D_RECARGO = 9, D_ESTADOHE = 10, D_HORAS = 11, D_VALOR = 12;

$(document).ready(function () {
    var hoy = new Date();
    $('#txtDesde').val(hoy.getFullYear() + '-01-01');
    $('#txtHasta').val(FechaIso(hoy));

    $('#cboMes, #cboResponsable, #cboEmpresa, #cboContrato, #cboRecargo, #cboEstadoHE')
        .on('change', function () { Consultar(false); });
    $('#btnConsultar').on('click', function () { Consultar(true); });
    $('#txtDesde, #txtHasta').on('change keyup', RangoCambiado);
    $('#btnExcel').on('click', Descargar);
    $('#btnAnterior').on('click', function () { CambiarPagina(-1); });
    $('#btnSiguiente').on('click', function () { CambiarPagina(1); });

    Consultar(true);
});

function FechaIso(d) {
    var m = d.getMonth() + 1, dia = d.getDate();
    return d.getFullYear() + '-' + (m < 10 ? '0' : '') + m + '-' + (dia < 10 ? '0' : '') + dia;
}

/* El rango sale de las cajas solo si usarCajas (boton Consultar); si no, es el de la pantalla. */
function Parametros(usarCajas) {
    return {
        desde: usarCajas ? ($('#txtDesde').val() || '') : tcDesde,
        hasta: usarCajas ? ($('#txtHasta').val() || '') : tcHasta,
        mes: $('#cboMes').val() || 'Todos',
        responsable: $('#cboResponsable').val() || 'Todos',
        empresa: $('#cboEmpresa').val() || 'Todos',
        contrato: $('#cboContrato').val() || 'Todos',
        recargo: $('#cboRecargo').val() || 'Todos',
        estadoHE: $('#cboEstadoHE').val() || 'Todos'
    };
}

function Mostrar(selector, texto) {
    $(selector).text(texto || '').toggle(!!texto);
}

function Consultar(refrescar) {
    /* Si no se publico chart.umd.js, decirlo aqui y no en la consola. */
    if (typeof Chart === 'undefined') {
        Mostrar('#tcError', 'No se pudo cargar la librería de gráficos. Avise a Sistemas: falta publicar js/chart.umd.js.');
        return;
    }

    /* Un rango nuevo puede no tener el mes, la persona o la empresa elegidos. */
    var p = Parametros(refrescar);
    var rango = p.desde + '|' + p.hasta;
    /* Se guardan los combos de la pantalla: si la consulta falla se restauran. */
    var combosAntes = {};
    $.each(['#cboMes', '#cboResponsable', '#cboEmpresa', '#cboContrato', '#cboRecargo', '#cboEstadoHE'], function (i, id) {
        combosAntes[id] = $(id).val();
    });
    /* Un rango nuevo puede no tener el mes, la persona o la empresa elegidos. */
    if (refrescar && rango !== tcRango) {
        $('#cboMes, #cboResponsable, #cboEmpresa, #cboContrato').val('Todos');
    }
    p.refrescar = refrescar ? '1' : '0';
    $('#btnConsultar').prop('disabled', true).text('Consultando…');

    $.ajax({
        type: 'POST',
        url: 'AdministrarTableroCostos.ashx',
        data: JSON.stringify([{ action: 'ConsultarTablero', parameters: p }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') {
                RestaurarCombos(combosAntes);
                Mostrar('#tcError', (r && r.mensaje) || 'No se pudo cargar el tablero.');
                return;
            }
            Mostrar('#tcError', '');
            tcDatos = r.resultado;
            tcRango = rango;
            tcDesde = p.desde;
            tcHasta = p.hasta;
            RangoCambiado();
            tcPagina = 0;
            Dibujar();
        },
        error: function (x) {
            RestaurarCombos(combosAntes);
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            Mostrar('#tcError', (r && r.mensaje) || 'No se pudo comunicar con el servidor.');
        },
        complete: function () {
            $('#btnConsultar').prop('disabled', false).text('Consultar');
        }
    });
}

/* Si falla una consulta, los combos vuelven a lo que corresponde a los datos que siguen en pantalla. */
function RestaurarCombos(valores) {
    $.each(valores, function (id, v) { $(id).val(v); });
}

/* Avisa y bloquea Excel mientras las cajas de fecha no coinciden con el rango consultado. */
function RangoCambiado() {
    var distinto = !!tcDatos && (($('#txtDesde').val() || '') !== tcDesde || ($('#txtHasta').val() || '') !== tcHasta);
    $('#btnExcel').prop('disabled', distinto);
    Mostrar('#tcRangoAviso', distinto ? 'Las fechas cambiaron: pulse Consultar para ver y descargar ese rango.' : '');
}

/* ------------------------------------------------------------ formato */

function Num(n, decimales) {
    var partes = (Number(n) || 0).toFixed(decimales).split('.');
    partes[0] = partes[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    return partes.length > 1 ? partes[0] + ',' + partes[1] : partes[0];
}

function Usd(n) { return '$' + Num(n, 2); }

function Pct(n) { return Num((Number(n) || 0) * 100, 1) + ' %'; }

function Html(s) {
    return $('<div>').text(s == null ? '' : String(s)).html();
}

/* ------------------------------------------------------------ dibujo */

function Dibujar() {
    LlenarCombo('#cboMes', tcDatos.Meses);
    LlenarCombo('#cboResponsable', tcDatos.Responsables);
    LlenarCombo('#cboEmpresa', tcDatos.Empresas);
    LlenarCombo('#cboContrato', tcDatos.Contratos);

    Mostrar('#tcAviso', tcDatos.Aviso);
    Mostrar('#tcVacio', tcDatos.Indicadores.Total.Actividades === 0
        ? 'No hay actividades para el rango y los filtros elegidos.' : '');
    Mostrar('#tcParametros', tcDatos.ParametrosPorDefecto
        ? 'Faltan parámetros vigentes de Horas Extras (Factor50, Factor100 o DiasMes): se usan 1,5 / 2 / 30, como el Excel.' : '');
    Mostrar('#tcSinSalario', tcDatos.SinSalario.length
        ? 'Sin salario en Horas Extras (sus horas se valoran en $0; complételos en Horas Extras): ' + tcDatos.SinSalario.join(', ') + '.'
        : '');

    DibujarIndicadores();
    DibujarTablas();
    DibujarGraficos();
    DibujarDetalle();
}

/* Conserva lo elegido. El servidor ya agrega el valor elegido si no tiene datos. */
function LlenarCombo(id, valores) {
    var combo = $(id), actual = combo.val() || 'Todos';
    combo.empty().append($('<option>').val('Todos').text('Todos'));
    $.each(valores || [], function (i, v) { combo.append($('<option>').val(v).text(v)); });
    combo.val(actual);
    if (combo.val() === null) { combo.val('Todos'); }
}

function DibujarIndicadores() {
    $.each(['Normal', 'Extra', 'Traslado', 'Total'], function (i, k) {
        var c = tcDatos.Indicadores[k];
        $('#ind' + k + 'Horas').text(Num(c.Horas, 2) + ' h');
        $('#ind' + k + 'Valor').text(Usd(c.Valor));
        $('#ind' + k + 'Act').text(Num(c.Actividades, 0) + ' actividades');
    });
}

/* filas: [{ negrita: bool, celdas: [html...] }]. La primera celda ya viene escapada. */
function Tabla(id, cabeceras, filas) {
    var html = '<thead><tr>';
    $.each(cabeceras, function (i, c) { html += '<th>' + Html(c) + '</th>'; });
    html += '</tr></thead><tbody>';
    $.each(filas, function (i, f) {
        html += '<tr' + (f.negrita ? ' class="tc-subtotal"' : '') + '>';
        $.each(f.celdas, function (j, c) { html += '<td' + (j > 0 ? ' class="text-right"' : '') + '>' + c + '</td>'; });
        html += '</tr>';
    });
    $(id).html(html + '</tbody>');
}

function DibujarTablas() {
    Tabla('#tblResumen', ['Tipo / Recargo', 'Horas', '% horas', 'Valor $', '% valor', 'Actividades', '$ por hora'],
        $.map(tcDatos.Resumen, function (r) {
            return { negrita: r.EsSubtotal, celdas: [Html(r.Concepto), Num(r.Horas, 2), Pct(r.PctHoras), Usd(r.Valor), Pct(r.PctValor), Num(r.Actividades, 0), Usd(r.ValorPorHora)] };
        }));

    Tabla('#tblExtras', ['Estado HE', 'Horas 50%', 'Horas 100%', '$ 50%', '$ 100%', 'Total $'],
        $.map(tcDatos.ExtrasPorEstado, function (e) {
            return { negrita: e.Estado === 'Total', celdas: [Html(e.Estado), Num(e.Horas50, 2), Num(e.Horas100, 2), Usd(e.Valor50), Usd(e.Valor100), Usd(e.Total)] };
        }));

    Tabla('#tblTraslados', ['Estado HE', 'Horas sin recargo', 'Horas 50%', 'Horas 100%', 'Total horas', 'Total $'],
        $.map(tcDatos.TrasladosPorEstado, function (e) {
            return { negrita: e.Estado === 'Total', celdas: [Html(e.Estado), Num(e.HorasSinRecargo, 2), Num(e.Horas50, 2), Num(e.Horas100, 2), Num(e.TotalHoras, 2), Usd(e.TotalValor)] };
        }));

    var cabPeriodo = function (primera) {
        var c = [primera];
        $.each(TC_CATEGORIAS, function (i, k) { c.push('Horas ' + k); });
        c.push('Total horas');
        $.each(TC_CATEGORIAS, function (i, k) { c.push('$ ' + k); });
        c.push('Total $');
        return c;
    };
    Tabla('#tblAnio', cabPeriodo('Año'), FilasPeriodo(tcDatos.PorAnio));
    Tabla('#tblMes', cabPeriodo('Mes'), FilasPeriodo(tcDatos.PorMes));

    Tabla('#tblTopNormal', ['Responsable', 'Horas', 'Valor $'],
        $.map(tcDatos.TopNormal, function (t) { return { celdas: [Html(t.Responsable), Num(t.Horas, 0), Usd(t.Valor)] }; }));
    Tabla('#tblTopExtra', ['Responsable', 'Horas 50%', 'Horas 100%', 'Valor $'],
        $.map(tcDatos.TopExtra, function (t) { return { celdas: [Html(t.Responsable), Num(t.Horas50, 0), Num(t.Horas100, 0), Usd(t.Valor)] }; }));
    Tabla('#tblTopTraslado', ['Responsable', 'Sin recargo', '50%', '100%', 'Valor $'],
        $.map(tcDatos.TopTraslado, function (t) { return { celdas: [Html(t.Responsable), Num(t.HorasSinRecargo, 0), Num(t.Horas50, 0), Num(t.Horas100, 0), Usd(t.Valor)] }; }));
}

function FilasPeriodo(lista) {
    return $.map(lista, function (p) {
        var celdas = [Html(p.Periodo)];
        $.each(TC_SUFIJOS, function (i, s) { celdas.push(Num(p['H' + s], 2)); });
        celdas.push(Num(p.HTotal, 2));
        $.each(TC_SUFIJOS, function (i, s) { celdas.push(Usd(p['V' + s])); });
        celdas.push(Usd(p.VTotal));
        return { negrita: p.Periodo === 'Total', celdas: celdas };
    });
}

/* ---------------------------------------------------------- graficos */

function Grafico(id, config) {
    if (tcGraficos[id]) { tcGraficos[id].destroy(); }
    tcGraficos[id] = new Chart(document.getElementById(id), config);
}

function Categorias() {
    return $.grep(tcDatos.Resumen, function (r) { return TC_CATEGORIAS.indexOf(r.Concepto) >= 0; });
}

function Dona(id, campo, formato) {
    var filas = Categorias();
    Grafico(id, {
        type: 'doughnut',
        data: {
            labels: $.map(filas, function (r) { return r.Concepto; }),
            datasets: [{ data: $.map(filas, function (r) { return r[campo]; }), backgroundColor: TC_COLORES }]
        },
        options: {
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'right' },
                tooltip: { callbacks: { label: function (c) { return c.label + ': ' + formato(c.parsed); } } }
            }
        }
    });
}

function BarrasMes(id, prefijo, formato) {
    var meses = $.grep(tcDatos.PorMes, function (p) { return p.Periodo !== 'Total'; });
    var series = $.map(TC_SUFIJOS, function (s, i) {
        return {
            label: TC_CATEGORIAS[i],
            backgroundColor: TC_COLORES[i],
            data: $.map(meses, function (p) { return p[prefijo + s]; })
        };
    });
    Grafico(id, {
        type: 'bar',
        data: { labels: $.map(meses, function (p) { return p.Periodo; }), datasets: series },
        options: {
            maintainAspectRatio: false,
            scales: { x: { stacked: true }, y: { stacked: true, ticks: { callback: function (v) { return formato(v); } } } },
            plugins: { tooltip: { callbacks: { label: function (c) { return c.dataset.label + ': ' + formato(c.parsed.y); } } } }
        }
    });
}

function BarrasTop(id, lista, color) {
    Grafico(id, {
        type: 'bar',
        data: {
            labels: $.map(lista, function (t) { return t.Responsable; }),
            datasets: [{ label: 'Valor $', backgroundColor: color, data: $.map(lista, function (t) { return t.Valor; }) }]
        },
        options: {
            indexAxis: 'y',
            maintainAspectRatio: false,
            plugins: { legend: { display: false }, tooltip: { callbacks: { label: function (c) { return Usd(c.parsed.x); } } } }
        }
    });
}

function DibujarGraficos() {
    Dona('grfDonaHoras', 'Horas', function (v) { return Num(v, 2) + ' h'; });
    Dona('grfDonaValor', 'Valor', Usd);
    BarrasMes('grfMesHoras', 'H', function (v) { return Num(v, 0); });
    BarrasMes('grfMesValor', 'V', function (v) { return '$' + Num(v, 0); });
    BarrasTop('grfTopNormal', tcDatos.TopNormal, TC_COLOR_TIPO.Normal);
    BarrasTop('grfTopExtra', tcDatos.TopExtra, TC_COLOR_TIPO.Extra);
    BarrasTop('grfTopTraslado', tcDatos.TopTraslado, TC_COLOR_TIPO.Traslado);
}

/* ----------------------------------------------------------- detalle */

function DibujarDetalle() {
    var filas = tcDatos.Detalle, total = filas.length;
    var ini = tcPagina * TC_POR_PAGINA, fin = Math.min(ini + TC_POR_PAGINA, total);
    var html = '';
    for (var i = ini; i < fin; i++) {
        var f = filas[i];
        html += '<tr><td>' + Html(f[D_INICIO]) + '</td><td>' + Html(f[D_RESPONSABLE]) + '</td><td>' + Html(f[D_EMPRESA])
              + '</td><td>' + Html(f[D_CONTRATO]) + '</td><td>' + Html(f[D_TICKET]) + '</td><td>' + Html(f[D_TAREA])
              + '</td><td>' + Html(f[D_ESTADO]) + '</td><td>' + Html(f[D_TIPO]) + '</td><td>' + Html(f[D_RECARGO])
              + '</td><td>' + Html(f[D_ESTADOHE]) + '</td><td class="text-right">' + Num(f[D_HORAS], 2)
              + '</td><td class="text-right">' + Usd(f[D_VALOR]) + '</td></tr>';
    }
    $('#tblDetalle tbody').html(html);
    $('#lblPagina').text(total === 0 ? 'Sin actividades'
        : 'Mostrando ' + Num(ini + 1, 0) + '–' + Num(fin, 0) + ' de ' + Num(total, 0));
    $('#btnAnterior').prop('disabled', tcPagina === 0);
    $('#btnSiguiente').prop('disabled', fin >= total);
}

function CambiarPagina(delta) {
    if (!tcDatos) { return; }
    var ultima = Math.max(0, Math.ceil(tcDatos.Detalle.length / TC_POR_PAGINA) - 1);
    tcPagina = Math.min(ultima, Math.max(0, tcPagina + delta));
    DibujarDetalle();
}

function Descargar() {
    if (!tcDatos) { return; }
    window.location = 'DescargarTableroCostos.ashx?' + $.param(Parametros(false));
}
