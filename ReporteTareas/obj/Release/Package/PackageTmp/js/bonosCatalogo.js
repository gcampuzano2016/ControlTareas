/* ==========================================================================
   Catalogo de bonos — BonosCatalogo.aspx
   Handler: AdministrarBonos.ashx (ListarTipos, GuardarTipo).
   Las validaciones de verdad estan en el servidor.
   ========================================================================== */

var bcTipos = [];
var bcEditando = null;
var bcAvisoTimer = null;

$(document).ready(function () {
    $('#bcNuevo').on('click', function () {
        AbrirFormulario({ idTipo: 0, nombre: '', familia: 'Standby', valorSugerido: null, porSemana: false, activo: true, conceptos: { DOS: '', AGILITY: '' } });
    });
    $('#bcCancelar').on('click', function () { $('#bcPanelForm').hide(); bcEditando = null; });
    $('#bcGuardar').on('click', GuardarFormulario);
    /* La pagina vive dentro del <form> de la maestra: Enter haria un postback. */
    $('#bcPanelForm input').on('keydown', function (e) { if (e.which === 13) { e.preventDefault(); } });
    CargarTipos();
});

function Mostrar(sel, texto) {
    var $e = $(sel);
    if (texto) { $e.text(texto).show(); } else { $e.hide().text(''); }
}

function Aviso(texto, tipo) {
    var clase = (tipo === 'warning') ? 'alert-warning' : 'alert-success';
    $('#bcAviso').removeClass('alert-success alert-warning alert-info').addClass(clase).text(texto).show();
    clearTimeout(bcAvisoTimer);
    bcAvisoTimer = setTimeout(function () { $('#bcAviso').hide(); }, 6000);
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
            Mostrar('#bcError', '');
            if (r.aviso) { Aviso(r.aviso, 'warning'); }
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
    if (fallo) { fallo(mensaje); } else { Mostrar('#bcError', mensaje); }
}

function Dinero(v) {
    if (v === null || v === undefined || v === '') { return ''; }
    var n = Number(v);
    return isNaN(n) ? '' : n.toFixed(2);
}

function CargarTipos() {
    Llamar('ListarTipos', {}, function (t) {
        bcTipos = t || [];
        DibujarTipos();
    });
}

function DibujarTipos() {
    var $t = $('#bcTipos').empty();
    if (!bcTipos.length) {
        $('<tr><td colspan="8" class="text-muted">No hay tipos de bono.</td></tr>').appendTo($t);
        return;
    }
    $.each(bcTipos, function (i, x) {
        var c = x.conceptos || {};
        var $tr = $('<tr></tr>');
        $('<td></td>').text(x.nombre).appendTo($tr);
        $('<td></td>').text(x.familia).appendTo($tr);
        $('<td class="bc-num"></td>').text(Dinero(x.valorSugerido)).appendTo($tr);
        $('<td></td>').text(x.porSemana ? 'Sí' : 'No').appendTo($tr);
        $('<td></td>').append($('<span class="label"></span>').addClass(x.activo ? 'label-success' : 'label-default').text(x.activo ? 'Activo' : 'Inactivo')).appendTo($tr);
        $('<td></td>').text(c.DOS || '').appendTo($tr);
        $('<td></td>').text(c.AGILITY || '').appendTo($tr);
        var $edit = $('<button type="button" class="btn btn-default btn-sm">Editar</button>').on('click', function () { AbrirFormulario(x); });
        $('<td></td>').append($edit).appendTo($tr);
        $tr.appendTo($t);
    });
}

function AbrirFormulario(x) {
    var c = x.conceptos || {};
    bcEditando = x.idTipo;
    $('#bcTituloForm').text(x.idTipo ? 'Editar tipo de bono' : 'Nuevo tipo de bono');
    $('#bcNombre').val(x.nombre);
    $('#bcFamilia').val(x.familia);
    $('#bcSugerido').val(Dinero(x.valorSugerido));
    $('#bcPorSemana').prop('checked', !!x.porSemana);
    $('#bcActivo').prop('checked', !!x.activo);
    $('#bcConceptoDOS').val(c.DOS || '');
    $('#bcConceptoAGILITY').val(c.AGILITY || '');
    Mostrar('#bcError', '');
    $('#bcPanelForm').show();
    $('#bcNombre').focus();
}

function GuardarFormulario() {
    var nombre = $.trim($('#bcNombre').val() || '');
    if (!nombre) { Mostrar('#bcError', 'El nombre es obligatorio.'); return; }
    var sug = $.trim($('#bcSugerido').val() || '').replace(',', '.');
    if (sug !== '' && (isNaN(Number(sug)) || Number(sug) < 0)) { Mostrar('#bcError', 'El valor sugerido no es válido.'); return; }
    var $btn = $('#bcGuardar').prop('disabled', true);
    Llamar('GuardarTipo', {
        idTipo: bcEditando || 0,
        nombre: nombre,
        familia: $('#bcFamilia').val(),
        valorSugerido: sug,
        porSemana: $('#bcPorSemana').is(':checked'),
        activo: $('#bcActivo').is(':checked'),
        conceptos: {
            DOS: $.trim($('#bcConceptoDOS').val() || '').toUpperCase(),
            AGILITY: $.trim($('#bcConceptoAGILITY').val() || '').toUpperCase()
        }
    }, function () {
        $btn.prop('disabled', false);
        $('#bcPanelForm').hide();
        bcEditando = null;
        CargarTipos();
    }, function (m) {
        $btn.prop('disabled', false);
        Mostrar('#bcError', m);
    });
}
