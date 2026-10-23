/* ==========================================================================
   Revisar y aprobar bonos (Talento Humano) — BonosRevisar.aspx
   Handler: AdministrarBonos.ashx (CargarRevisar, Aprobar, Ajustar, Rechazar,
   AprobarVarios, RegistrarDirecto, AbrirMes, PrevioCierre, CerrarMes).
   Secciones: utilidades · carga y tabla · filas y aprobación · diálogo ·
   bono directo · cierre del mes.
   El sugerido y los filtros son ayudas del navegador; el servidor decide.
   ========================================================================== */

var brDatos = null;          /* resultado de CargarRevisar */
var brOcupado = false;
var brAvisoTimer = null;
var brSel = {};              /* idBono -> true (solo Enviado) */
var brValorManual = false;
var brLugarManual = false;
var brSemanasSel = {};
var brTomadas = [];          /* SemanasTomadas de la persona elegida en el bono directo (todos los periodos) */
var brLote = 100;            /* AprobarVarios: el servidor revisa uno por uno, se manda por tandas */
var brMeses = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];

$(document).ready(function () {
    $('#brFiltroEstado, #brFiltroJefe').on('change', function () { brSel = {}; DibujarTabla(); });
    $('#brSelTodos').on('change', MarcarTodos);
    $('#brAprobarSel').on('click', AprobarSeleccionados);
    $('#brDirecto').on('click', AbrirDirecto);
    $('#brCancelar').on('click', function () { $('#brPanelForm').hide(); });
    $('#brGuardar').on('click', function () { GuardarDirecto(false); });
    $('#brAbrirMes').on('click', AbrirMes);
    $('#brCerrarMes').on('click', CerrarMes);
    $('#brPersona').on('change', CambioPersona);
    $('#brTipo').on('change', function () { DibujarSemanas(); ActualizarSugerido(true); });
    $('#brValor').on('input', function () { brValorManual = true; ActualizarSugerido(false); });
    $('#brLugar').on('input', function () { brLugarManual = true; });
    /* La pagina vive dentro del <form> de la maestra: Enter haria un postback. */
    $('#brPanelForm input, #brOverlay input').on('keydown', function (e) { if (e.which === 13) { e.preventDefault(); } });
    Cargar();
});

/* ---------- utilidades ---------- */

function Mostrar(sel, texto) {
    var $e = $(sel);
    if (texto) { $e.text(texto).show(); } else { $e.hide().text(''); }
}

function Aviso(texto, tipo) {
    var clase = (tipo === 'warning') ? 'alert-warning' : 'alert-success';
    $('#brAviso').removeClass('alert-success alert-warning alert-info').addClass(clase).text(texto).show();
    clearTimeout(brAvisoTimer);
    brAvisoTimer = setTimeout(function () { $('#brAviso').hide(); }, 8000);
}

function Ocupado(si) {
    brOcupado = si;
    $('#brGuardar, #brAprobarSel, #brAbrirMes, #brCerrarMes, #brDirecto, #brDialogoSi, #brDialogoNo, .br-fila').prop('disabled', si);
}

/* Llamada de bajo nivel: fin(r, errorDeRed). r es la respuesta completa del handler. */
function Enviar(accion, parametros, fin) {
    $.ajax({
        type: 'POST',
        url: 'AdministrarBonos.ashx',
        data: JSON.stringify([{ action: accion, parameters: parametros }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) { fin(r, null); },
        error: function (x) {
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            fin(r, (r && r.mensaje) || 'No se pudo comunicar con el servidor.');
        }
    });
}

/* ok(resultado). fallo(mensaje). repetido(aviso): estado "2". */
function Llamar(accion, parametros, ok, fallo, repetido) {
    Enviar(accion, parametros, function (r, errRed) {
        if (errRed) { Fallar(errRed, fallo); return; }
        if (r && r.estado === '2' && repetido) { repetido(r.aviso || ''); return; }
        if (!r || r.estado !== '1') { Fallar((r && r.mensaje) || 'No se pudo completar la operación.', fallo); return; }
        Mostrar('#brError', '');
        if (r.aviso) { Aviso((r.mensaje ? r.mensaje + ' ' : '') + r.aviso, 'warning'); }
        else if (r.mensaje) { Aviso(r.mensaje, r.tipoMensaje); }
        ok(r.resultado);
    });
}

function Fallar(mensaje, fallo) {
    if (fallo) { fallo(mensaje); } else { Mostrar('#brError', mensaje); }
}

function Dinero(v) {
    if (v === null || v === undefined || v === '') { return ''; }
    var n = Number(v);
    return isNaN(n) ? '' : n.toFixed(2);
}

function Fecha(iso) { /* 'yyyy-MM-dd' -> Date en UTC */
    var p = String(iso).split('-');
    return new Date(Date.UTC(+p[0], +p[1] - 1, +p[2]));
}

function EtiquetaSemana(iso) {
    var a = Fecha(iso), b = new Date(a.getTime() + 6 * 86400000);
    if (a.getUTCMonth() === b.getUTCMonth()) {
        return 'Semana del ' + a.getUTCDate() + ' al ' + b.getUTCDate() + ' de ' + brMeses[b.getUTCMonth()];
    }
    return 'Semana del ' + a.getUTCDate() + ' de ' + brMeses[a.getUTCMonth()] + ' al ' + b.getUTCDate() + ' de ' + brMeses[b.getUTCMonth()];
}

function EtiquetaCorta(iso) {
    var a = Fecha(iso);
    return a.getUTCDate() + ' ' + brMeses[a.getUTCMonth()].substring(0, 3);
}

function TipoPorId(id) {
    var t = null;
    $.each(brDatos.tipos, function (i, x) { if (Number(x.idTipo) === Number(id)) { t = x; } });
    return t;
}

function PersonaPorCod(cod) {
    var p = null;
    $.each(brDatos.personas, function (i, x) { if (String(x.codUsuario) === String(cod)) { p = x; } });
    return p;
}

function NombreJefe(cod) {
    if (cod === null || cod === undefined || cod === '') { return '—'; }
    var p = PersonaPorCod(cod);
    return p ? p.nombre : String(cod);
}

/* ---------- carga y tabla ---------- */

/* mensaje: error que debe sobrevivir a la recarga (p. ej. «El bono cambió…»). */
function Cargar(mensaje) {
    Llamar('CargarRevisar', {}, function (r) {
        brDatos = r || {};
        brDatos.bonos = brDatos.bonos || [];
        brDatos.personas = brDatos.personas || [];
        brDatos.tipos = brDatos.tipos || [];
        brDatos.resumen = brDatos.resumen || {};
        brDatos.semanasMarcables = brDatos.semanasMarcables || [];
        /* Lo marcado que ya no sigue Enviado deja de estar marcado. */
        var vivos = {};
        $.each(brDatos.bonos, function (i, b) { if (b.estado === 'Enviado') { vivos[b.idBono] = true; } });
        $.each(brSel, function (id) { if (!vivos[id]) { delete brSel[id]; } });
        Dibujar();
        if (mensaje) { Mostrar('#brError', mensaje); }
    }, function (m) {
        Mostrar('#brError', mensaje ? mensaje + ' ' + m : m);
    });
}

function Dibujar() {
    $('#brContenido').show();
    var p = brDatos.periodo;
    var abierto = !!p;
    if (abierto) {
        $('#brBarra').text('Período: Bonos ' + brMeses[p.mes - 1] + ' ' + p.anio + ' (abierto)');
    } else {
        $('#brBarra').text('No hay un mes de bonos abierto.');
        $('#brPanelForm').hide();
    }
    $('#brAbrirMes').toggle(!abierto);
    $('#brCerrarMes, #brDirecto, #brAprobarSel').toggle(abierto);
    var r = brDatos.resumen;
    $('#brResEnviados').text(r.enviados || 0);
    $('#brResAprobados').text(r.aprobados || 0);
    $('#brResRechazados').text(r.rechazados || 0);
    $('#brResTotal').text('$' + Dinero(r.totalAprobado || 0));
    DibujarFiltroJefe();
    DibujarTabla();
}

function DibujarFiltroJefe() {
    var $f = $('#brFiltroJefe');
    var actual = $f.val();
    $f.empty();
    $('<option></option>').val('').text('Todos').appendTo($f);
    var vistos = {}, lista = [];
    $.each(brDatos.bonos, function (i, b) {
        var c = (b.codJefe === null || b.codJefe === undefined) ? '' : String(b.codJefe);
        if (c !== '' && !vistos[c]) { vistos[c] = true; lista.push({ cod: c, nombre: NombreJefe(c) }); }
    });
    lista.sort(function (a, b) { return a.nombre.localeCompare(b.nombre); });
    $.each(lista, function (i, j) { $('<option></option>').val(j.cod).text(j.nombre).appendTo($f); });
    if (actual && vistos[actual]) { $f.val(actual); }
}

function Filtrados() {
    var est = $('#brFiltroEstado').val(), jefe = $('#brFiltroJefe').val();
    return $.grep(brDatos.bonos, function (b) {
        if (est && b.estado !== est) { return false; }
        if (jefe && String(b.codJefe) !== jefe) { return false; }
        return true;
    });
}

function DibujarTabla() {
    var $t = $('#brBonos').empty();
    var abierto = !!brDatos.periodo;
    var lista = Filtrados();
    if (!lista.length) {
        $('<tr><td colspan="9" class="text-muted">No hay bonos para mostrar.</td></tr>').appendTo($t);
    }
    var clases = { Enviado: 'label-info', Aprobado: 'label-success', Rechazado: 'label-danger', Borrador: 'label-default' };
    $.each(lista, function (i, b) {
        var $tr = $('<tr></tr>');
        var $c = $('<td></td>');
        if (abierto && b.estado === 'Enviado') {
            $('<input type="checkbox" class="br-fila" />').prop('checked', !!brSel[b.idBono]).on('change', function () {
                if (this.checked) { brSel[b.idBono] = true; } else { delete brSel[b.idBono]; }
            }).appendTo($c);
        }
        $c.appendTo($tr);
        $('<td></td>').text(NombreJefe(b.codJefe)).appendTo($tr);
        var $per = $('<td></td>').text(b.nombre || '');
        if (b.identificacion) { $('<span class="br-nota"></span>').text(b.identificacion).appendTo($per); }
        if (b.lugar) { $('<span class="br-nota"></span>').text(b.lugar).appendTo($per); }
        $per.appendTo($tr);
        $('<td></td>').text(b.nombreTipo || '').appendTo($tr);
        $('<td></td>').text($.map(b.lunes || [], EtiquetaCorta).join(', ') || '—').appendTo($tr);
        $('<td class="br-num"></td>').text(b.valorSugerido === null || b.valorSugerido === undefined ? '' : '$' + Dinero(b.valorSugerido)).appendTo($tr);
        var $val = $('<td class="br-num"></td>').text('$' + Dinero(b.valor));
        if (b.motivoAjuste) { $('<span class="br-nota"></span>').text('Motivo: ' + b.motivoAjuste).appendTo($val); }
        if (b.observacion) { $('<span class="br-nota"></span>').text(b.observacion).appendTo($val); }
        $val.appendTo($tr);
        var $est = $('<td></td>').append($('<span class="label"></span>').addClass(clases[b.estado] || 'label-default').text(b.estado));
        if (b.estado === 'Rechazado' && b.motivoRechazo) { $('<span class="br-rechazo"></span>').text('Motivo: ' + b.motivoRechazo).appendTo($est); }
        $est.appendTo($tr);
        var $acc = $('<td style="white-space:nowrap"></td>');
        if (abierto && b.estado === 'Enviado') {
            Boton('Aprobar', 'btn-success', function () { Aprobar(b); }).appendTo($acc);
            $acc.append(' ');
        }
        if (abierto && (b.estado === 'Enviado' || b.estado === 'Aprobado')) {
            Boton('Ajustar', 'btn-default', function () { Ajustar(b); }).appendTo($acc);
            $acc.append(' ');
            Boton('Rechazar', 'btn-danger', function () { Rechazar(b); }).appendTo($acc);
        }
        $acc.appendTo($tr);
        $tr.appendTo($t);
    });
    $('#brSelTodos').prop('checked', false);
}

function Boton(texto, clase, fn) {
    return $('<button type="button" class="btn btn-sm br-fila"></button>').addClass(clase).text(texto).prop('disabled', brOcupado).on('click', fn);
}

/* ---------- acciones por fila y aprobación en lote ---------- */

function MarcarTodos() {
    var si = this.checked;
    $.each(Filtrados(), function (i, b) {
        if (b.estado !== 'Enviado') { return; }
        if (si) { brSel[b.idBono] = true; } else { delete brSel[b.idBono]; }
    });
    DibujarTabla();
    $('#brSelTodos').prop('checked', si);
}

function TerminarFila(fallo) {
    Ocupado(false);
    Cargar(fallo);
}

function Aprobar(b) {
    if (brOcupado) { return; }
    Ocupado(true);
    Llamar('Aprobar', { idBono: b.idBono, estado: b.estado }, function () { TerminarFila(); }, TerminarFila);
}

function Ajustar(b) {
    if (brOcupado) { return; }
    AbrirDialogo({
        titulo: 'Ajustar bono',
        texto: (b.nombre || '') + ' · ' + (b.nombreTipo || '') + ' · valor actual $' + Dinero(b.valor),
        valor: Dinero(b.valor), motivo: true, si: 'Ajustar',
        enSi: function (v, fin) {
            var n = LeerNumero(v.valor);
            if (isNaN(n) || n <= 0) { fin('El valor debe ser un número mayor que cero.'); return; }
            if (!v.motivo) { fin('El motivo es obligatorio.'); return; }
            Ocupado(true);
            Llamar('Ajustar', { idBono: b.idBono, estado: b.estado, valor: String(n), motivo: v.motivo },
                function () { fin(null); TerminarFila(); },
                function (m) { fin(m); Cargar(); Ocupado(false); });
        }
    });
}

function Rechazar(b) {
    if (brOcupado) { return; }
    AbrirDialogo({
        titulo: 'Rechazar bono',
        texto: (b.nombre || '') + ' · ' + (b.nombreTipo || '') + ' · $' + Dinero(b.valor) + '. El jefe recibirá el motivo.',
        valor: null, motivo: true, si: 'Rechazar',
        enSi: function (v, fin) {
            if (!v.motivo) { fin('El motivo es obligatorio.'); return; }
            Ocupado(true);
            Llamar('Rechazar', { idBono: b.idBono, estado: b.estado, motivo: v.motivo },
                function () { fin(null); TerminarFila(); },
                function (m) { fin(m); Cargar(); Ocupado(false); });
        }
    });
}

function AprobarSeleccionados() {
    if (brOcupado) { return; }
    /* Solo lo que se ve con el filtro actual: lo oculto no se paga por accidente. */
    var items = [];
    $.each(Filtrados(), function (i, b) {
        if (b.estado === 'Enviado' && brSel[b.idBono]) { items.push({ idBono: b.idBono, estado: b.estado }); }
    });
    if (!items.length) { Mostrar('#brError', 'Marque al menos un bono enviado.'); return; }
    Mostrar('#brError', '');
    AbrirDialogo({
        titulo: 'Aprobar seleccionados',
        texto: 'Se aprobarán ' + items.length + ' bono(s).',
        si: 'Aprobar',
        enSi: function (v, fin) { fin(null); AprobarLotes(items); }
    });
}

function AprobarLotes(items) {
    if (brOcupado) { return; }
    Ocupado(true);
    var aprobados = 0, fallidos = [], errorTanda = '';
    var tandas = [];
    for (var i = 0; i < items.length; i += brLote) { tandas.push(items.slice(i, i + brLote)); }
    var k = 0;
    (function siguiente() {
        if (k >= tandas.length) { TerminarLote(aprobados, items.length, fallidos, errorTanda); return; }
        var tanda = tandas[k++];
        Enviar('AprobarVarios', { items: tanda }, function (r, errRed) {
            var res = r && r.resultado;
            if (res) {
                aprobados += Number(res.aprobados) || 0;
                $.each(res.fallidos || [], function (j, f) { fallidos.push(f); });
            } else {
                errorTanda = errRed || (r && r.mensaje) || 'No se pudo aprobar una tanda de bonos.';
                $.each(tanda, function (j, it) { fallidos.push({ idBono: it.idBono, mensaje: errorTanda }); });
            }
            siguiente();
        });
    })();
}

function TerminarLote(aprobados, total, fallidos, errorTanda) {
    Ocupado(false);
    brSel = {};
    Aviso(aprobados + ' aprobado(s) de ' + total + '.', fallidos.length ? 'warning' : 'success');
    if (fallidos.length) {
        var porId = {};
        $.each(brDatos.bonos, function (i, b) { porId[b.idBono] = b; });
        AbrirDialogo({
            titulo: aprobados + ' aprobado(s)',
            texto: 'No se pudieron aprobar ' + fallidos.length + ' bono(s):',
            lista: $.map(fallidos, function (f) {
                var b = porId[f.idBono];
                return (b ? (b.nombre + ' · ' + b.nombreTipo) : ('Bono ' + f.idBono)) + ': ' + (f.mensaje || 'no se pudo aprobar');
            }),
            si: null, no: 'Cerrar'
        });
    }
    Cargar();
}

/* ---------- diálogo único ---------- */
/* op: {titulo, texto, valor (texto inicial o null = sin campo), motivo (bool), lista ([texto]),
        si (rótulo o null = sin botón), no (rótulo), enSi(vals, fin)}
   fin(null) cierra; fin('mensaje') lo muestra dentro y deja reintentar. */

function AbrirDialogo(op) {
    $('#brDialogoTitulo').text(op.titulo || '');
    $('#brDialogoTexto').text(op.texto || '');
    var hayCampos = (op.valor !== null && op.valor !== undefined) || !!op.motivo;
    $('#brDialogoCampos').toggle(hayCampos);
    $('#brGrupoValor').toggle(op.valor !== null && op.valor !== undefined);
    $('#brGrupoMotivo').toggle(!!op.motivo);
    $('#brDialogoValor').val(op.valor || '');
    $('#brDialogoMotivo').val('');
    var $l = $('#brDialogoLista').empty();
    if (op.lista && op.lista.length) {
        var $ul = $('<ul></ul>').appendTo($l);
        $.each(op.lista, function (i, t) { $('<li></li>').text(t).appendTo($ul); });
    }
    $l.toggle(!!(op.lista && op.lista.length));
    if (op.extra) { op.extra($l); $l.show(); }
    Mostrar('#brDialogoError', '');
    $('#brDialogoNo').text(op.no || 'Cancelar').off('click').on('click', function () { if (!brOcupado) { $('#brOverlay').hide(); } });
    var $si = $('#brDialogoSi');
    $si.toggle(!!op.si).text(op.si || '').prop('disabled', brOcupado).off('click');
    if (op.si) {
        $si.on('click', function () {
            if (brOcupado) { return; }
            Mostrar('#brDialogoError', '');
            op.enSi({ valor: $.trim($('#brDialogoValor').val() || ''), motivo: $.trim($('#brDialogoMotivo').val() || '') }, function (err) {
                if (err) { Mostrar('#brDialogoError', err); Ocupado(false); } else { $('#brOverlay').hide(); }
            });
        });
    }
    $('#brOverlay').show();
    if (op.valor !== null && op.valor !== undefined) { $('#brDialogoValor').focus(); }
    else if (op.motivo) { $('#brDialogoMotivo').focus(); }
}

function LeerNumero(texto) {
    var s = $.trim(texto || '').replace(',', '.');
    if (s === '') { return 0; }
    var n = Number(s);
    return isNaN(n) ? NaN : n;
}

/* ---------- registrar bono directo ---------- */

function AbrirDirecto() {
    if (brOcupado || !brDatos.periodo) { return; }
    brSemanasSel = {};
    brTomadas = [];
    brValorManual = false;
    brLugarManual = false;
    var $p = $('#brPersona').empty();
    var personas = brDatos.personas.slice().sort(function (a, b) { return String(a.nombre).localeCompare(String(b.nombre)); });
    $.each(personas, function (i, g) { $('<option></option>').val(g.codUsuario).text(g.nombre + ' (' + g.empresa + ')').appendTo($p); });
    var $ti = $('#brTipo').empty();
    $.each(brDatos.tipos, function (i, t) { if (t.activo) { $('<option></option>').val(t.idTipo).text(t.nombre).appendTo($ti); } });
    $('#brLugar').val('');
    $('#brValor').val('');
    $('#brMotivo').val('');
    $('#brObservacion').val('');
    Mostrar('#brError', '');
    $('#brPanelForm').show();
    $('#brPersona').focus();
    CambioPersona();
}

/* Las semanas ya tomadas de la persona (todos los periodos) las da el servidor. */
var brPedidoPersona = 0;
function CambioPersona() {
    var cod = $('#brPersona').val(), mio = ++brPedidoPersona;
    brTomadas = [];
    brSemanasSel = {};
    DibujarSemanas();
    ActualizarSugerido(true);
    if (!cod) { return; }
    Llamar('SemanasTomadas', { codUsuario: cod }, function (r) {
        if (mio !== brPedidoPersona) { return; }
        brTomadas = r || [];
        DibujarSemanas();
        ActualizarSugerido(true);
    }, function (m) {
        if (mio === brPedidoPersona) { Mostrar('#brError', m); }
    });
}

function TipoActual() { return TipoPorId($('#brTipo').val()); }

/* Semanas que ya tomó la persona elegida para el tipo elegido (en cualquier período). */
function Tomadas() {
    var tipo = Number($('#brTipo').val()), set = {};
    $.each(brTomadas, function (i, s) {
        if (Number(s.idTipo) !== tipo) { return; }
        $.each(s.lunes || [], function (j, l) { set[l] = true; });
    });
    return set;
}

function DibujarSemanas() {
    var t = TipoActual();
    var porSemana = !!(t && t.porSemana);
    $('#brFilaSemanas').toggle(porSemana);
    var $c = $('#brSemanas').empty();
    if (!porSemana) { brSemanasSel = {}; return; }
    var tomadas = Tomadas();
    var lunes = brDatos.semanasMarcables.slice().sort().reverse();
    if (!lunes.length) { $('<span class="text-muted">No hay semanas terminadas para marcar.</span>').appendTo($c); }
    $.each(lunes, function (i, l) {
        var tomada = !!tomadas[l];
        if (tomada) { delete brSemanasSel[l]; }
        var $cb = $('<input type="checkbox" />').prop('checked', !!brSemanasSel[l]).prop('disabled', tomada).on('change', function () {
            if (this.checked) { brSemanasSel[l] = true; } else { delete brSemanasSel[l]; }
            ActualizarSugerido(true);
        });
        var $lb = $('<label></label>').append($cb).append(' ').append(document.createTextNode(EtiquetaSemana(l)));
        if (tomada) { $lb.addClass('br-tomada').attr('title', 'Ya tomada por otro bono'); }
        $lb.appendTo($c);
    });
}

function NumSemanas() { var n = 0; $.each(brSemanasSel, function () { n++; }); return n; }

/* null = sin sugerido (libre) */
function Sugerido() {
    var t = TipoActual();
    if (!t || t.valorSugerido === null || t.valorSugerido === undefined || t.valorSugerido === '') { return null; }
    var v = Number(t.valorSugerido);
    if (isNaN(v)) { return null; }
    return t.porSemana ? Math.round(v * NumSemanas() * 100) / 100 : v;
}

function ActualizarSugerido(sigueSugerido) {
    var sug = Sugerido();
    if (sigueSugerido && !brValorManual) { $('#brValor').val(sug === null ? '' : Dinero(sug)); }
    $('#brSugerido').text(sug === null ? 'Sugerido: (libre)' : 'Sugerido: $' + Dinero(sug));
    var v = LeerNumero($('#brValor').val());
    $('#brMotivoReq').toggle(sug !== null && (isNaN(v) || Math.abs(v - sug) >= 0.005));
}

function GuardarDirecto(confirmarRepetido) {
    if (brOcupado) { return; }
    var t = TipoActual();
    var cod = $('#brPersona').val();
    if (!cod) { Mostrar('#brError', 'Elija la persona.'); return; }
    if (!t) { Mostrar('#brError', 'Elija el tipo de bono.'); return; }
    var lunes = $.map(brSemanasSel, function (v, k) { return k; }).sort();
    if (t.porSemana && !lunes.length) { Mostrar('#brError', 'Marque al menos una semana.'); return; }
    var valor = LeerNumero($('#brValor').val());
    if (isNaN(valor) || valor <= 0) { Mostrar('#brError', 'El valor debe ser un número mayor que cero.'); return; }
    var sug = Sugerido();
    var motivo = $.trim($('#brMotivo').val() || '');
    if (sug !== null && Math.abs(valor - sug) >= 0.005 && !motivo) {
        Mostrar('#brError', 'El valor difiere del sugerido: escriba el motivo del ajuste.');
        return;
    }
    Mostrar('#brError', '');
    Ocupado(true);
    Llamar('RegistrarDirecto', {
        idBono: 0,
        estado: '',
        codUsuario: cod,
        idTipo: Number(t.idTipo),
        lugar: $.trim($('#brLugar').val() || ''),
        valor: String(valor),
        motivoAjuste: motivo,
        observacion: $.trim($('#brObservacion').val() || ''),
        lunes: t.porSemana ? lunes : [],
        confirmarRepetido: !!confirmarRepetido
    }, function () {
        Ocupado(false);
        $('#brPanelForm').hide();
        Cargar();
    }, function (m) {
        Ocupado(false);
        Mostrar('#brError', m);
    }, function (aviso) {
        Ocupado(false);
        AbrirDialogo({
            titulo: 'Bono repetido', texto: aviso, si: 'Registrar de todos modos',
            enSi: function (v, fin) { fin(null); GuardarDirecto(true); }
        });
    });
}

/* ---------- abrir y cerrar el mes ---------- */

function AbrirMes() {
    if (brOcupado) { return; }
    Ocupado(true);
    Llamar('AbrirMes', {}, function () { TerminarFila(); }, TerminarFila);
}

function CerrarMes() {
    if (brOcupado) { return; }
    Mostrar('#brError', '');
    Ocupado(true);
    Llamar('PrevioCierre', {}, function (c) {
        Ocupado(false);
        MostrarPrevio(c || {});
    }, function (m) {
        Ocupado(false);
        Mostrar('#brError', m);
    });
}

function MostrarPrevio(c) {
    var fuera = c.quedanFuera || [], sinConcepto = c.sinConcepto || [];
    var puede = !!c.puedeCerrar && !sinConcepto.length;
    AbrirDialogo({
        titulo: 'Cerrar el mes',
        texto: 'Total a pagar (aprobado): $' + Dinero(c.total || 0),
        si: puede ? 'Cerrar el mes' : null, no: puede ? 'Cancelar' : 'Cerrar',
        extra: function ($l) {
            if (sinConcepto.length) {
                $('<p class="text-danger"></p>').text('No se puede cerrar: falta el concepto SAP de estos tipos.').appendTo($l);
                var $u = $('<ul></ul>').appendTo($l);
                $.each(sinConcepto, function (i, t) { $('<li></li>').text(t).appendTo($u); });
            }
            if (fuera.length) {
                $('<p></p>').text('Quedan fuera (pasan pendientes al próximo mes):').appendTo($l);
                var grupos = {}, orden = [];
                $.each(fuera, function (i, b) {
                    if (!grupos[b.estado]) { grupos[b.estado] = []; orden.push(b.estado); }
                    grupos[b.estado].push(b);
                });
                $.each(orden, function (i, e) {
                    $('<strong></strong>').text(e + ' (' + grupos[e].length + ')').appendTo($l);
                    var $v = $('<ul></ul>').appendTo($l);
                    $.each(grupos[e], function (j, b) {
                        $('<li></li>').text((b.nombre || '') + ' · ' + (b.nombreTipo || '') + ' · $' + Dinero(b.valor)).appendTo($v);
                    });
                });
            } else {
                $('<p class="text-muted"></p>').text('No queda nada fuera del cierre.').appendTo($l);
            }
        },
        enSi: function (v, fin) {
            Ocupado(true);
            Llamar('CerrarMes', {},
                function () { fin(null); TerminarFila(); },
                function (m) { fin(m); Cargar(); Ocupado(false); });
        }
    });
}
