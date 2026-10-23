/* ==========================================================================
   Proponer bonos (jefe) — BonosProponer.aspx
   Handler: AdministrarBonos.ashx (CargarProponer, GuardarBorrador, Borrar, Enviar).
   El sugerido que se ve aqui es solo una ayuda; el servidor decide.
   ========================================================================== */

var bpDatos = null;          /* resultado de CargarProponer */
var bpEdit = null;           /* bono en edicion (null = nuevo) */
var bpSemanasSel = {};       /* lunes marcados: { 'yyyy-MM-dd': true } */
var bpValorManual = false;   /* el jefe escribio el valor a mano */
var bpLugarManual = false;
var bpOcupado = false;
var bpAvisoTimer = null;
var bpMeses = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];

$(document).ready(function () {
    $('#bpNuevo').on('click', function () { AbrirFormulario(null); });
    $('#bpCancelar').on('click', CerrarFormulario);
    $('#bpGuardar').on('click', function () { Guardar(false); });
    $('#bpEnviar').on('click', EnviarTodo);
    $('#bpAviso').attr('title', 'Clic para cerrar').css('cursor', 'pointer').on('click', function () { $(this).hide(); });
    $('#bpPersona').on('change', function () { CambioPersona(); });
    $('#bpTipo').on('change', function () { DibujarSemanas(); ActualizarSugerido(true); });
    $('#bpValor').on('input', function () { bpValorManual = true; ActualizarSugerido(false); });
    $('#bpLugar').on('input', function () { bpLugarManual = true; });
    /* La pagina vive dentro del <form> de la maestra: Enter haria un postback. */
    $('#bpPanelForm input').on('keydown', function (e) { if (e.which === 13) { e.preventDefault(); } });
    Cargar();
});

/* ---------- utilidades ---------- */

function Mostrar(sel, texto) {
    var $e = $(sel);
    if (texto) { $e.text(texto).show(); } else { $e.hide().text(''); }
}

function Aviso(texto, tipo) {
    var clase = (tipo === 'warning') ? 'alert-warning' : 'alert-success';
    $('#bpAviso').removeClass('alert-success alert-warning alert-info').addClass(clase).text(texto).show();
    clearTimeout(bpAvisoTimer);
    /* Una advertencia (p. ej. bonos que no se enviaron) queda a la vista hasta la próxima acción. */
    if (tipo !== 'warning') { bpAvisoTimer = setTimeout(function () { $('#bpAviso').hide(); }, 8000); }
}

function Ocupado(si) {
    bpOcupado = si;
    $('#bpGuardar, #bpEnviar').prop('disabled', si);
}

/* ok(resultado). fallo(mensaje). repetido(aviso): estado "2". */
function Llamar(accion, parametros, ok, fallo, repetido) {
    $.ajax({
        type: 'POST',
        url: 'AdministrarBonos.ashx',
        data: JSON.stringify([{ action: accion, parameters: parametros }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (r && r.estado === '2' && repetido) { repetido(r.aviso || ''); return; }
            if (!r || r.estado !== '1') { Fallar((r && r.mensaje) || 'No se pudo completar la operación.', fallo); return; }
            Mostrar('#bpError', '');
            if (r.aviso) { Aviso((r.mensaje ? r.mensaje + ' ' : '') + r.aviso, 'warning'); }
            else if (r.mensaje) { Aviso(r.mensaje, r.tipoMensaje); }
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
    if (fallo) { fallo(mensaje); } else { Mostrar('#bpError', mensaje); }
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
        return 'Semana del ' + a.getUTCDate() + ' al ' + b.getUTCDate() + ' de ' + bpMeses[b.getUTCMonth()];
    }
    return 'Semana del ' + a.getUTCDate() + ' de ' + bpMeses[a.getUTCMonth()] + ' al ' + b.getUTCDate() + ' de ' + bpMeses[b.getUTCMonth()];
}

function EtiquetaCorta(iso) {
    var a = Fecha(iso);
    return a.getUTCDate() + ' ' + bpMeses[a.getUTCMonth()].substring(0, 3);
}

function TipoPorId(id) {
    var t = null;
    $.each(bpDatos.tipos, function (i, x) { if (Number(x.idTipo) === Number(id)) { t = x; } });
    return t;
}

function PersonaPorCod(cod) {
    var p = null;
    $.each(bpDatos.gente, function (i, x) { if (String(x.codUsuario) === String(cod)) { p = x; } });
    return p;
}

/* ---------- carga y lista ---------- */

function Cargar() {
    Llamar('CargarProponer', {}, function (r) {
        bpDatos = r || {};
        bpDatos.gente = bpDatos.gente || [];
        bpDatos.bonos = bpDatos.bonos || [];
        bpDatos.tipos = bpDatos.tipos || [];
        bpDatos.semanasTomadas = bpDatos.semanasTomadas || [];
        bpDatos.semanasMarcables = bpDatos.semanasMarcables || [];
        Dibujar();
    });
}

function Dibujar() {
    /* Sin gente pero con bonos propios (la persona cambió de jefe): se muestran para poder borrarlos. */
    if (!bpDatos.gente.length && !bpDatos.bonos.length) {
        $('#bpContenido').hide();
        Mostrar('#bpSinGente', 'No tienes personas a cargo para proponer bonos.');
        return;
    }
    Mostrar('#bpSinGente', '');
    $('#bpContenido').show();
    var p = bpDatos.periodo;
    var abierto = !!p;
    if (abierto) {
        $('#bpBarra').text('Período: Bonos ' + bpMeses[p.mes - 1] + ' ' + p.anio + ' (abierto) · Mi equipo: ' + bpDatos.gente.length + ' persona(s)');
    } else {
        $('#bpBarra').text('No hay un mes de bonos abierto.');
        $('#bpPanelForm').hide();
    }
    $('#bpNuevo').toggle(abierto && bpDatos.gente.length > 0);
    var hayBorrador = false;
    $.each(bpDatos.bonos, function (i, b) { if (b.estado === 'Borrador') { hayBorrador = true; } });
    $('#bpEnviar').toggle(abierto).prop('disabled', bpOcupado || !hayBorrador);
    DibujarBonos(abierto);
}

function DibujarBonos(abierto) {
    var $t = $('#bpBonos').empty();
    if (!bpDatos.bonos.length) {
        $('<tr><td colspan="6" class="text-muted">No hay bonos propuestos.</td></tr>').appendTo($t);
        return;
    }
    var clases = { Borrador: 'label-default', Enviado: 'label-info', Aprobado: 'label-success', Rechazado: 'label-danger' };
    $.each(bpDatos.bonos, function (i, b) {
        var $tr = $('<tr></tr>');
        $('<td></td>').text(b.nombre).appendTo($tr);
        $('<td></td>').text(b.nombreTipo).appendTo($tr);
        var sem = $.map(b.lunes || [], EtiquetaCorta).join(', ');
        $('<td></td>').text(sem || '—').appendTo($tr);
        $('<td class="bp-num"></td>').text('$' + Dinero(b.valor)).appendTo($tr);
        var $est = $('<td></td>').append($('<span class="label"></span>').addClass(clases[b.estado] || 'label-default').text(b.estado));
        if (b.estado === 'Rechazado' && b.motivoRechazo) {
            $('<span class="bp-rechazo"></span>').text('Motivo: ' + b.motivoRechazo).appendTo($est);
        }
        $est.appendTo($tr);
        var $acc = $('<td></td>');
        if (abierto && (b.estado === 'Borrador' || b.estado === 'Rechazado')) {
            if (PersonaPorCod(b.codUsuario)) {
                $('<button type="button" class="btn btn-default btn-sm">Editar</button>').on('click', function () { AbrirFormulario(b); }).appendTo($acc);
                $acc.append(' ');
            } else {
                /* Ya no está a mi cargo: no se edita ni se envía, pero bloquea sus semanas hasta que se borre. */
                $('<span class="bp-rechazo"></span>').text('Ya no está a su cargo: solo puede borrarlo.').appendTo($est);
            }
            $('<button type="button" class="btn btn-danger btn-sm">Borrar</button>').on('click', function () { Borrar(b); }).appendTo($acc);
        }
        $acc.appendTo($tr);
        $tr.appendTo($t);
    });
}

/* ---------- formulario ---------- */

function AbrirFormulario(b) {
    bpEdit = b;
    bpSemanasSel = {};
    bpValorManual = false;
    bpLugarManual = !!b;
    $('#bpTituloForm').text(b ? 'Editar bono' : 'Nuevo bono');

    var $p = $('#bpPersona').empty();
    var hay = false;
    $.each(bpDatos.gente, function (i, g) {
        $('<option></option>').val(g.codUsuario).text(g.nombre + ' (' + g.empresa + ')').appendTo($p);
        if (b && String(g.codUsuario) === String(b.codUsuario)) { hay = true; }
    });
    if (b && !hay) { $('<option></option>').val(b.codUsuario).text(b.nombre).appendTo($p); }
    var $ti = $('#bpTipo').empty();
    var hayTipo = false;
    $.each(bpDatos.tipos, function (i, t) {
        var suyo = b && Number(t.idTipo) === Number(b.idTipo);
        /* Un tipo desactivado solo llega si es de un bono mío, y solo se ofrece al editar ese bono. */
        if (t.activo === false && !suyo) { return; }
        $('<option></option>').val(t.idTipo).text(t.nombre + (t.activo === false ? ' (desactivado)' : '')).appendTo($ti);
        if (suyo) { hayTipo = true; }
    });
    if (b && !hayTipo) { $('<option></option>').val(b.idTipo).text(b.nombreTipo).appendTo($ti); }

    if (b) {
        $p.val(String(b.codUsuario));
        $ti.val(String(b.idTipo));
        $.each(b.lunes || [], function (i, l) { bpSemanasSel[l] = true; });
        $('#bpLugar').val(b.lugar || '');
        $('#bpValor').val(Dinero(b.valor));
        $('#bpMotivo').val(b.motivoAjuste || '');
        $('#bpObservacion').val(b.observacion || '');
        var sug = Sugerido();
        bpValorManual = !(sug !== null && Math.abs(sug - Number(b.valor)) < 0.005);
    } else {
        $('#bpLugar').val('');
        $('#bpValor').val('');
        $('#bpMotivo').val('');
        $('#bpObservacion').val('');
    }
    Mostrar('#bpError', '');
    MostrarAvisoId();
    if (!b) { CambioPersona(); }
    DibujarSemanas();
    ActualizarSugerido(!b);
    $('#bpPanelForm').show();
    $('#bpPersona').focus();
}

function CerrarFormulario() {
    $('#bpPanelForm').hide();
    bpEdit = null;
}

function MostrarAvisoId() {
    var g = PersonaPorCod($('#bpPersona').val());
    var a = g && g.avisoIdentificacion;
    Mostrar('#bpAvisoId', a ? (typeof a === 'string' ? a : 'Falta un dato de identificación: el bono se guardará pero no se podrá enviar.') : '');
}

function CambioPersona() {
    MostrarAvisoId();
    var g = PersonaPorCod($('#bpPersona').val());
    if (!bpLugarManual) { $('#bpLugar').val(g ? (g.lugarSugerido || '') : ''); }
    DibujarSemanas();
    ActualizarSugerido(true);
}

function TipoActual() { return TipoPorId($('#bpTipo').val()); }

/* Semanas que otro bono (no el que se edita) ya tomo para la persona y el tipo elegidos. */
function Tomadas() {
    var cod = String($('#bpPersona').val()), tipo = Number($('#bpTipo').val()), set = {};
    $.each(bpDatos.semanasTomadas, function (i, s) {
        if (String(s.codUsuario) !== cod || Number(s.idTipo) !== tipo) { return; }
        if (bpEdit && Number(s.idBono) === Number(bpEdit.idBono)) { return; }
        $.each(s.lunes || [], function (j, l) { set[l] = true; });
    });
    return set;
}

function DibujarSemanas() {
    var t = TipoActual();
    var porSemana = !!(t && t.porSemana);
    $('#bpFilaSemanas').toggle(porSemana);
    var $c = $('#bpSemanas').empty();
    if (!porSemana) { bpSemanasSel = {}; return; }
    var tomadas = Tomadas();
    var lunes = bpDatos.semanasMarcables.slice().sort().reverse();
    if (!lunes.length) { $('<span class="text-muted">No hay semanas terminadas para marcar.</span>').appendTo($c); }
    $.each(lunes, function (i, l) {
        var tomada = !!tomadas[l];
        if (tomada) { delete bpSemanasSel[l]; }
        var $cb = $('<input type="checkbox" />').prop('checked', !!bpSemanasSel[l]).prop('disabled', tomada).on('change', function () {
            if (this.checked) { bpSemanasSel[l] = true; } else { delete bpSemanasSel[l]; }
            ActualizarSugerido(true);
        });
        var $lb = $('<label></label>').append($cb).append(' ').append(document.createTextNode(EtiquetaSemana(l)));
        if (tomada) { $lb.addClass('bp-tomada').attr('title', 'Ya tomada por otro bono'); }
        $lb.appendTo($c);
    });
}

function NumSemanas() { var n = 0; $.each(bpSemanasSel, function () { n++; }); return n; }

/* null = sin sugerido (libre) */
function Sugerido() {
    var t = TipoActual();
    if (!t || t.valorSugerido === null || t.valorSugerido === undefined || t.valorSugerido === '') { return null; }
    var v = Number(t.valorSugerido);
    if (isNaN(v)) { return null; }
    return t.porSemana ? Math.round(v * NumSemanas() * 100) / 100 : v;
}

function LeerValor() {
    var s = $.trim($('#bpValor').val() || '').replace(',', '.');
    if (s === '') { return 0; }
    var n = Number(s);
    return isNaN(n) ? NaN : n;
}

/* sigueSugerido = true: si el jefe no escribio el valor, el campo toma el sugerido. */
function ActualizarSugerido(sigueSugerido) {
    var sug = Sugerido();
    if (sigueSugerido && !bpValorManual) { $('#bpValor').val(sug === null ? '' : Dinero(sug)); }
    $('#bpSugerido').text(sug === null ? 'Sugerido: (libre)' : 'Sugerido: $' + Dinero(sug));
    var v = LeerValor();
    var difiere = sug !== null && (isNaN(v) || Math.abs(v - sug) >= 0.005);
    $('#bpMotivoReq').toggle(difiere);
}

/* ---------- acciones ---------- */

function Guardar(confirmarRepetido) {
    if (bpOcupado) { return; }
    var t = TipoActual();
    var cod = $('#bpPersona').val();
    if (!cod) { Mostrar('#bpError', 'Elija la persona.'); return; }
    if (!t) { Mostrar('#bpError', 'Elija el tipo de bono.'); return; }
    var lunes = $.map(bpSemanasSel, function (v, k) { return k; }).sort();
    if (t.porSemana && !lunes.length) { Mostrar('#bpError', 'Marque al menos una semana.'); return; }
    var valor = LeerValor();
    if (isNaN(valor) || valor <= 0) { Mostrar('#bpError', 'El valor debe ser un número mayor que cero.'); return; }
    var sug = Sugerido();
    var motivo = $.trim($('#bpMotivo').val() || '');
    if (sug !== null && Math.abs(valor - sug) >= 0.005 && !motivo) {
        Mostrar('#bpError', 'El valor difiere del sugerido: escriba el motivo del ajuste.');
        return;
    }
    Mostrar('#bpError', '');
    Ocupado(true);
    Llamar('GuardarBorrador', {
        idBono: bpEdit ? bpEdit.idBono : 0,
        estado: bpEdit ? bpEdit.estado : '',
        codUsuario: cod,
        idTipo: Number(t.idTipo),
        lugar: $.trim($('#bpLugar').val() || ''),
        valor: String(valor),
        motivoAjuste: motivo,
        observacion: $.trim($('#bpObservacion').val() || ''),
        lunes: t.porSemana ? lunes : [],
        confirmarRepetido: !!confirmarRepetido
    }, function () {
        Ocupado(false);
        CerrarFormulario();
        Cargar();
    }, function (m) {
        Ocupado(false);
        Mostrar('#bpError', m);
    }, function (aviso) {
        Ocupado(false);
        PedirConfirmacion(aviso, function () { Guardar(true); });
    });
}

function PedirConfirmacion(texto, siAcepta) {
    $('#bpDialogoTexto').text(texto);
    $('#bpOverlay').show();
    $('#bpDialogoNo').off('click').on('click', function () { $('#bpOverlay').hide(); });
    $('#bpDialogoSi').off('click').on('click', function () { $('#bpOverlay').hide(); siAcepta(); });
}

function Borrar(b) {
    if (bpOcupado) { return; }
    Ocupado(true);
    Llamar('Borrar', { idBono: b.idBono, estado: b.estado }, function () {
        Ocupado(false);
        if (bpEdit && Number(bpEdit.idBono) === Number(b.idBono)) { CerrarFormulario(); }
        Cargar();
    }, function (m) {
        Ocupado(false);
        Mostrar('#bpError', m);
        Cargar();
    });
}

function EnviarTodo() {
    if (bpOcupado) { return; }
    Ocupado(true);
    Llamar('Enviar', {}, function () {
        Ocupado(false);
        CerrarFormulario();
        Cargar();
    }, function (m) {
        Ocupado(false);
        Mostrar('#bpError', m);
        Cargar();
    });
}
