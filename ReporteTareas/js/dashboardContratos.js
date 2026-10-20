/* ==========================================================================
   Dashboard de Contratos — sexta pestaña de RegistrarContrato.aspx
   Handler : ObtenerListaTareas.ashx  (accion ReporteListaContratos, la misma
             de "Listas de Contratos", sin filtros: trae todo OrdenServicio
             activo y aqui se filtra y se agrega).
   Los numeros llegan como texto con la cultura del sitio (es-ES): coma
   decimal y sin separador de miles ("1250,5"). Las fechas como
   "dd/MM/yyyy H:mm:ss", y 1900-01-01 significa vacio (ISNULL del SP).
   ========================================================================== */

var dcContratos = null;
var dcCargado = false;
var dcGraficos = {};
var dcAlertaActiva = '';

var DC_AZUL = '#2F6DB5';
var DC_NARANJA = '#E07B24';
var DC_GRIS = '#9AA5B1';
var DC_DIAS_CIERRE = 30;
var DC_UMBRAL_HORAS = 0.8;

/* Orden de gravedad: es el orden de las etiquetas y el de la tabla. */
var DC_ALERTAS = [
    { id: 'horasExcedidas', texto: 'Horas excedidas', clase: 'label-danger' },
    { id: 'costoExcedido', texto: 'Costo excedido', clase: 'label-danger' },
    { id: 'cierreVencido', texto: 'Cierre vencido', clase: 'label-danger' },
    { id: 'horas80', texto: 'Horas ≥ 80 %', clase: 'label-warning' },
    { id: 'cierreProximo', texto: 'Cierra en ≤ ' + DC_DIAS_CIERRE + ' días', clase: 'label-warning' },
    { id: 'sinHoras', texto: 'Consume sin horas contratadas', clase: 'label-default' }
];

$(document).ready(function () {
    $('a[href="#Dashboard-pills"]').on('shown.bs.tab', function () {
        if (!dcCargado) { DcConsultar(); }
    });
    $('#dcActualizar').on('click', DcConsultar);
    $('.dc-filtro').on('change', DcDibujar);
    $('#dcAlertasResumen').on('click', '.dc-filtro-alerta', function () {
        var id = $(this).data('alerta');
        dcAlertaActiva = dcAlertaActiva === id ? '' : id;
        DcDibujarAlertas(DcFiltrados());
    });
});

/* ------------------------------------------------------------- datos */

function DcConsultar() {
    var Datos = "[{ \"action\": \"ReporteListaContratos\", \"parameters\" : { fechaDesde: \"\", fechaHasta: \"\", busqueda: \"\", IdCliente: \"0\", IdGerenteCuenta: \"0\", IdGestorResponsable: \"0\", sucursal: \"\", estado: \"\", area: \"\", IdClasificacion: \"0\" } }]";
    $('#dcMensaje').text('Cargando contratos...');
    $('#dcActualizar').prop('disabled', true);
    $.ajax({
        type: "POST",
        url: 'ObtenerListaTareas.ashx',
        data: Datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) {
            if (respuesta == null || !$.isArray(respuesta)) {
                $('#dcMensaje').text('No se pudo leer la lista de contratos.');
                return;
            }
            dcContratos = $.map(respuesta, DcNormalizar);
            dcCargado = true;
            DcLlenarCombos();
            DcDibujar();
            $('#dcMensaje').text('Datos leídos a las ' + DcHora(new Date()) + '.');
        },
        error: function () {
            $('#dcMensaje').text('La consulta de contratos falló; vuelva a intentarlo con "Actualizar".');
        },
        complete: function () {
            $('#dcActualizar').prop('disabled', false);
        }
    });
}

function DcNormalizar(c) {
    var registro = DcFecha(c.FECHA);
    return {
        id: c.IdServicio,
        orden: c.ORDEN || '',
        cliente: $.trim(c.CLIENTE || ''),
        area: $.trim(c.AREA || ''),
        gestor: $.trim(c.GESTOR_RESPONSABLE || ''),
        gerente: $.trim(c.GERENTE_DE_CUENTA || ''),
        sucursal: $.trim(c.SUCURSAL || ''),
        estado: $.trim(c.ESTATUS || ''),
        anio: registro ? String(registro.getFullYear()) : 'Sin fecha',
        horasC: DcNumero(c.HORAS_CONTRATADAS),
        horasE: DcNumero(c.HORAS_ENTREGADAS),
        costoP: DcNumero(c.COSTO_PLAN),
        costoR: DcNumero(c.COSTO_REAL),
        saldo: DcNumero(c.SALDO_DE_COSTOS),
        cierreEstimado: DcFecha(c.FECHA_ESTIMADA_DE_CIERRE)
    };
}

function DcNumero(s) {
    var n = parseFloat(String(s == null ? '' : s).replace(/\s/g, '').replace(',', '.'));
    return isNaN(n) ? 0 : n;
}

function DcFecha(s) {
    var m = /^(\d{1,2})\/(\d{1,2})\/(\d{4})/.exec(s || '');
    var d = m ? new Date(+m[3], +m[2] - 1, +m[1])
              : (/^(\d{4})-(\d{2})-(\d{2})/.test(s || '') ? new Date(+s.substr(0, 4), +s.substr(5, 2) - 1, +s.substr(8, 2)) : null);
    return d && d.getFullYear() > 1900 ? d : null;
}

function DcLlenarCombos() {
    DcCombo('#dcArea', 'area');
    DcCombo('#dcSucursal', 'sucursal');
    DcCombo('#dcGerente', 'gerente');
    DcCombo('#dcGestor', 'gestor');
    DcCombo('#dcAnio', 'anio', true);
}

/* Conserva lo elegido si el valor sigue existiendo tras "Actualizar". */
function DcCombo(id, campo, descendente) {
    var actual = $(id).val();
    var valores = [];
    $.each(dcContratos, function (i, c) {
        if (c[campo] && valores.indexOf(c[campo]) < 0) { valores.push(c[campo]); }
    });
    valores.sort();
    if (descendente) { valores.reverse(); }
    $(id).empty().append($('<option>').val('').text('TODOS'));
    $.each(valores, function (i, v) { $(id).append($('<option>').val(v).text(v)); });
    if (actual && valores.indexOf(actual) >= 0) { $(id).val(actual); }
}

function DcFiltrados() {
    var f = {
        estado: $('#dcEstado').val(), area: $('#dcArea').val(), sucursal: $('#dcSucursal').val(),
        gerente: $('#dcGerente').val(), gestor: $('#dcGestor').val(), anio: $('#dcAnio').val()
    };
    return $.grep(dcContratos || [], function (c) {
        for (var k in f) {
            if (f[k] && c[k] !== f[k]) { return false; }
        }
        return true;
    });
}

function DcAlertasDe(c, hoy) {
    var alertas = [];
    if (c.estado !== 'EN EJECUCION') { return alertas; }
    var limite = new Date(hoy.getTime() + DC_DIAS_CIERRE * 86400000);
    if (c.horasC > 0 && c.horasE > c.horasC) { alertas.push('horasExcedidas'); }
    if (c.costoP > 0 && c.saldo < 0) { alertas.push('costoExcedido'); }
    if (c.cierreEstimado && c.cierreEstimado < hoy) { alertas.push('cierreVencido'); }
    if (c.horasC > 0 && c.horasE >= DC_UMBRAL_HORAS * c.horasC && c.horasE <= c.horasC) { alertas.push('horas80'); }
    if (c.cierreEstimado && c.cierreEstimado >= hoy && c.cierreEstimado <= limite) { alertas.push('cierreProximo'); }
    if (c.horasC <= 0 && c.horasE > 0) { alertas.push('sinHoras'); }
    return alertas;
}

function DcAgrupar(lista, campo) {
    var grupos = {};
    $.each(lista, function (i, c) {
        var clave = c[campo] || '(sin dato)';
        var g = grupos[clave] || (grupos[clave] = { nombre: clave, n: 0, horasC: 0, horasE: 0, costoP: 0, costoR: 0, ejecucion: 0, cerradas: 0 });
        g.n++;
        g.horasC += c.horasC;
        g.horasE += c.horasE;
        g.costoP += c.costoP;
        g.costoR += c.costoR;
        if (c.estado === 'EN EJECUCION') { g.ejecucion++; } else { g.cerradas++; }
    });
    return $.map(grupos, function (g) { return g; });
}

/* ----------------------------------------------------------- formato */

function DcNum(n, decimales) {
    var texto = Math.abs(Number(n) || 0).toFixed(decimales);
    var partes = texto.split('.');
    partes[0] = partes[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    var signo = n < 0 && Number(texto) !== 0 ? '-' : '';
    return signo + (partes.length > 1 ? partes[0] + ',' + partes[1] : partes[0]);
}

function DcUsd(n) { return (n < 0 ? '-$' : '$') + DcNum(Math.abs(n), 2); }

function DcPct(parte, total) { return total > 0 ? DcNum(parte / total * 100, 1) + ' %' : '—'; }

function DcHtml(s) { return $('<div>').text(s == null ? '' : String(s)).html(); }

function DcHora(d) { return ('0' + d.getHours()).slice(-2) + ':' + ('0' + d.getMinutes()).slice(-2); }

function DcDia(d) { return d ? ('0' + d.getDate()).slice(-2) + '/' + ('0' + (d.getMonth() + 1)).slice(-2) + '/' + d.getFullYear() : ''; }

/* ------------------------------------------------------------ dibujo */

function DcDibujar() {
    if (!dcContratos) { return; }
    /* Con otros filtros la alerta elegida puede quedar sin contratos y su
       etiqueta desaparece: no habria como desmarcarla. */
    dcAlertaActiva = '';
    var lista = DcFiltrados();
    DcDibujarIndicadores(lista);
    DcDibujarAlertas(lista);
    DcDibujarGraficos(lista);
}

function DcDibujarIndicadores(lista) {
    var t = { ejecucion: 0, horasC: 0, horasE: 0, horasEConHoras: 0, sinHoras: 0, horasSinContrato: 0, costoP: 0, costoR: 0, saldo: 0 };
    $.each(lista, function (i, c) {
        if (c.estado === 'EN EJECUCION') { t.ejecucion++; }
        t.horasC += c.horasC;
        t.horasE += c.horasE;
        if (c.horasC > 0) { t.horasEConHoras += c.horasE; } else { t.sinHoras++; t.horasSinContrato += c.horasE; }
        t.costoP += c.costoP;
        t.costoR += c.costoR;
        t.saldo += c.saldo;
    });

    $('#dcKpiContratos').text(DcNum(lista.length, 0));
    $('#dcKpiContratosSub').text(DcNum(t.ejecucion, 0) + ' en ejecución · ' + DcNum(lista.length - t.ejecucion, 0) + ' cerradas');
    $('#dcKpiHorasC').text(DcNum(t.horasC, 0) + ' h');
    $('#dcKpiHorasCSub').text(t.sinHoras ? DcNum(t.sinHoras, 0) + ' contratos sin horas contratadas' : '');
    $('#dcKpiHorasE').text(DcNum(t.horasE, 0) + ' h');
    /* El % solo compara contratos que tienen horas contratadas: los que no
       tienen dispararian el porcentaje sin decir nada de su consumo real. */
    $('#dcKpiHorasESub').text(DcPct(t.horasEConHoras, t.horasC) + ' consumido' +
        (t.horasSinContrato > 0 ? ' · ' + DcNum(t.horasSinContrato, 0) + ' h sin contratar' : ''));
    $('#dcKpiCostoP').text(DcUsd(t.costoP));
    $('#dcKpiCostoPSub').text('');
    $('#dcKpiCostoR').text(DcUsd(t.costoR));
    $('#dcKpiCostoRSub').text(DcPct(t.costoR, t.costoP) + ' del plan');
    $('#dcKpiSaldo').text(DcUsd(t.saldo)).toggleClass('dc-negativo', t.saldo < 0);
    $('#dcKpiSaldoSub').text(t.saldo < 0 ? 'Costo real supera el plan' : 'Disponible del plan');
}

function DcDibujarAlertas(lista) {
    var hoy = new Date();
    hoy.setHours(0, 0, 0, 0);
    var conteo = {};
    var filas = [];
    $.each(lista, function (i, c) {
        var alertas = DcAlertasDe(c, hoy);
        $.each(alertas, function (j, a) { conteo[a] = (conteo[a] || 0) + 1; });
        if (alertas.length) { filas.push({ c: c, alertas: alertas }); }
    });

    var resumen = $.map(DC_ALERTAS, function (a) {
        if (!conteo[a.id]) { return null; }
        var apagada = dcAlertaActiva && dcAlertaActiva !== a.id ? ' dc-apagada' : '';
        return '<span class="label ' + a.clase + ' dc-filtro-alerta' + apagada + '" data-alerta="' + a.id +
            '" title="Clic para ver solo estos contratos">' + DcHtml(a.texto) + ': ' + conteo[a.id] + '</span>';
    }).join(' ');
    $('#dcAlertasResumen').html(resumen || '<p class="text-muted">Ningún contrato en ejecución con alertas para estos filtros.</p>');

    if (dcAlertaActiva) {
        filas = $.grep(filas, function (f) { return f.alertas.indexOf(dcAlertaActiva) >= 0; });
    }
    /* Mas alertas graves primero; a igualdad, el cierre estimado mas antiguo. */
    filas.sort(function (a, b) {
        var ga = DcGravedad(a.alertas), gb = DcGravedad(b.alertas);
        if (ga !== gb) { return gb - ga; }
        return (a.c.cierreEstimado || 0) - (b.c.cierreEstimado || 0);
    });

    $('#dcAlertasCuerpo').html($.map(filas, function (f) {
        var c = f.c;
        var etiquetas = $.map(DC_ALERTAS, function (a) {
            return f.alertas.indexOf(a.id) >= 0 ? '<span class="label ' + a.clase + '">' + DcHtml(a.texto) + '</span>' : null;
        }).join('');
        return '<tr>' +
            '<td><a href="javascript:void(0)" onclick="VerContratos(' + Number(c.id) + ')">' + DcHtml(c.orden) + '</a></td>' +
            '<td>' + DcHtml(c.cliente) + '</td>' +
            '<td>' + DcHtml(c.gerente) + '</td>' +
            '<td>' + DcHtml(c.area) + '</td>' +
            '<td class="text-right">' + DcNum(c.horasC, 1) + '</td>' +
            '<td class="text-right">' + DcNum(c.horasE, 1) + '</td>' +
            '<td class="text-right">' + DcPct(c.horasE, c.horasC) + '</td>' +
            '<td class="text-right' + (c.saldo < 0 ? ' dc-negativo' : '') + '">' + DcUsd(c.saldo) + '</td>' +
            '<td>' + DcDia(c.cierreEstimado) + '</td>' +
            '<td>' + etiquetas + '</td>' +
            '</tr>';
    }).join('') || '<tr><td colspan="10" class="text-muted">Sin contratos que mostrar.</td></tr>');
}

function DcGravedad(alertas) {
    var peso = 0;
    $.each(alertas, function (i, a) {
        peso += a === 'horasExcedidas' || a === 'costoExcedido' || a === 'cierreVencido' ? 10 : 1;
    });
    return peso;
}

function DcGrafico(id, config) {
    if (dcGraficos[id]) { dcGraficos[id].destroy(); }
    dcGraficos[id] = new Chart(document.getElementById(id), config);
}

function DcBarras(id, etiquetas, series, formato, horizontal) {
    var ejeValor = { beginAtZero: true, ticks: { callback: function (v) { return formato(v); } }, grid: { color: '#eee' } };
    /* Nombres de cliente de hasta 90 caracteres: el eje muestra el inicio y el
       tooltip el nombre completo. */
    var ejeCategoria = {
        grid: { display: false },
        ticks: {
            callback: function (v) {
                var texto = this.getLabelForValue(v);
                return texto.length > 32 ? texto.substr(0, 30) + '…' : texto;
            }
        }
    };
    DcGrafico(id, {
        type: 'bar',
        data: {
            labels: etiquetas,
            datasets: $.map(series, function (s) {
                return {
                    label: s.label, data: s.data, backgroundColor: s.color, stack: s.stack,
                    borderRadius: 4, borderSkipped: 'start', maxBarThickness: 28,
                    borderColor: '#fff', borderWidth: s.stack ? 1 : 0
                };
            })
        },
        options: {
            indexAxis: horizontal ? 'y' : 'x',
            maintainAspectRatio: false,
            animation: { duration: 300 },
            scales: horizontal ? { x: ejeValor, y: ejeCategoria } : { x: ejeCategoria, y: ejeValor },
            plugins: {
                legend: { display: series.length > 1 },
                tooltip: {
                    callbacks: {
                        label: function (c) {
                            return (series.length > 1 ? c.dataset.label + ': ' : '') + formato(horizontal ? c.parsed.x : c.parsed.y);
                        }
                    }
                }
            }
        }
    });
}

function DcDibujarGraficos(lista) {
    var horas = function (v) { return DcNum(v, 0) + ' h'; };
    var dolares = function (v) { return '$' + DcNum(v, 0); };

    var areas = DcAgrupar(lista, 'area').sort(function (a, b) { return b.horasC - a.horasC; });
    DcBarras('dcGrfHorasArea', $.map(areas, function (g) { return g.nombre; }), [
        { label: 'Contratadas', color: DC_AZUL, data: $.map(areas, function (g) { return g.horasC; }) },
        { label: 'Entregadas', color: DC_NARANJA, data: $.map(areas, function (g) { return g.horasE; }) }
    ], horas, false);

    var gerentes = DcAgrupar(lista, 'gerente').sort(function (a, b) { return b.costoP - a.costoP; }).slice(0, 10);
    DcBarras('dcGrfCostoGerente', $.map(gerentes, function (g) { return g.nombre; }), [
        { label: 'Costo plan', color: DC_AZUL, data: $.map(gerentes, function (g) { return g.costoP; }) },
        { label: 'Costo real', color: DC_NARANJA, data: $.map(gerentes, function (g) { return g.costoR; }) }
    ], dolares, true);

    var clientes = DcAgrupar(lista, 'cliente').sort(function (a, b) { return b.costoR - a.costoR; }).slice(0, 10);
    DcBarras('dcGrfClientes', $.map(clientes, function (g) { return g.nombre; }), [
        { label: 'Costo real', color: DC_NARANJA, data: $.map(clientes, function (g) { return g.costoR; }) }
    ], dolares, true);

    var anios = DcAgrupar(lista, 'anio').sort(function (a, b) { return a.nombre < b.nombre ? -1 : 1; });
    DcBarras('dcGrfAnios', $.map(anios, function (g) { return g.nombre; }), [
        { label: 'En ejecución', color: DC_AZUL, stack: 'estado', data: $.map(anios, function (g) { return g.ejecucion; }) },
        { label: 'Cerradas', color: DC_GRIS, stack: 'estado', data: $.map(anios, function (g) { return g.cerradas; }) }
    ], function (v) { return DcNum(v, 0); }, false);
}
