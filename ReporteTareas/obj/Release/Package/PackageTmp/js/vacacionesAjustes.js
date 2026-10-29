/* ==========================================================================
   Ajustes de vacaciones (GTH) - VacacionesAjustes.aspx
   Handler: AdministrarVacAjustes.ashx (ListarColaboradores, Cargar,
   SimularCorte, GuardarCorte, RegistrarCompra, AnularCompra).
   Cada escritura pide motivo y queda en Vac_Bitacora. Sin alert/confirm:
   las confirmaciones van dentro de la pagina.
   ========================================================================== */

var vaPersonas = [];
var vaActual = null;   // lo que devolvio Cargar para la persona elegida

$(document).ready(function () {
    Llamar('ListarColaboradores', {}, function (lista) {
        vaPersonas = lista || [];
        LlenarPersonas();
    });
    $('#txtBuscar').on('input', LlenarPersonas);
    $('#cboColaborador').on('change', function () { Cargar(''); });
    $('#txtCorteFecha, #txtCorteSaldo').on('input change', function () { Mostrar('#vaSimulacion', ''); });
    $('#btnCorteSimular').on('click', SimularCorte);
    $('#btnCorteGuardar').on('click', function () { GuardarCorte(false); });
    $('#btnCorteQuitar').on('click', PedirQuitar);
    $('#btnQuitarSi').on('click', function () { GuardarCorte(true); });
    $('#btnQuitarNo').on('click', function () { $('#vaQuitarConfirmar').hide(); });
    $('#txtCompraDias').on('input', PreviaCompra);
    $('#btnCompraRegistrar').on('click', RegistrarCompra);
    $('#tblCompras').on('click', '.va-anular', PedirAnular);
    $('#tblCompras').on('click', '.va-anular-no', function () { $(this).closest('tr').remove(); });
    $('#tblCompras').on('click', '.va-anular-si', AnularCompra);
});

function Mostrar(sel, texto) {
    var $e = $(sel);
    if (texto) { $e.text(texto).show(); } else { $e.hide().text(''); }
}

function Llamar(accion, parametros, ok, fin) {
    Mostrar('#vaOk', '');
    $.ajax({
        type: 'POST',
        url: 'AdministrarVacAjustes.ashx',
        data: JSON.stringify([{ action: accion, parameters: parametros }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') { Mostrar('#vaError', (r && r.mensaje) || 'No se pudo completar la operación.'); return; }
            Mostrar('#vaError', '');
            ok(r.resultado);
        },
        error: function (x) {
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            Mostrar('#vaError', (r && r.mensaje) || 'No se pudo comunicar con el servidor.');
        },
        complete: function () { if (fin) { fin(); } }
    });
}

/* Deshabilita el boton mientras dura la llamada: evita guardar dos veces. */
function Ocupado($b, ocupado) { $b.prop('disabled', ocupado); }

function LlenarPersonas() {
    var q = $.trim($('#txtBuscar').val()).toLowerCase();
    var actual = $('#cboColaborador').val();
    var $c = $('#cboColaborador').empty().append($('<option value=""/>').text('Elija...'));
    $.each(vaPersonas, function (i, p) {
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

/* "12,79" o "12.79" -> "12.79" para el servidor; "" si esta vacio. */
function NumeroAlServidor(texto) {
    var t = $.trim(texto || '').replace(/\s/g, '');
    if (!t) { return ''; }
    if (t.indexOf(',') >= 0) { t = t.replace(/\./g, '').replace(',', '.'); }
    return t;
}

/* Numero para mostrar en un campo editable: 12.79 -> "12,79". */
function NumCampo(v) {
    return v === null || v === undefined || v === '' ? '' : String(v).replace('.', ',');
}

function Dato($c, etiqueta, valor) {
    $('<div class="va-dato"/>').append($('<b/>').text(etiqueta)).append(document.createTextNode(' ' + valor)).appendTo($c);
}

function Celda($tr, texto, clase) {
    return $('<td/>').addClass(clase || '').text(texto).appendTo($tr);
}

function Id() { return $('#cboColaborador').val(); }

/* aviso: mensaje de exito que se muestra despues de recargar. */
function Cargar(aviso) {
    vaActual = null;
    $('#vaResultado').hide();
    Mostrar('#vaSimulacion', '');
    $('#vaQuitarConfirmar').hide();
    if (!Id()) { return; }
    Llamar('Cargar', { idColaborador: Id() }, function (r) {
        vaActual = r;
        Pintar(r);
        if (aviso) { Mostrar('#vaOk', aviso); }
    });
}

function TextoCorte(fecha, dias) {
    return fecha ? Num(dias) + ' días al ' + FechaTexto(fecha) : 'sin corte';
}

function Pintar(r) {
    var p = r.persona, s = r.saldo;

    var $per = $('#vaPersona').empty();
    Dato($per, 'Colaborador:', p.Nombre);
    Dato($per, 'Cédula:', p.Cedula);
    Dato($per, 'Fecha de ingreso:', FechaTexto(p.ingreso));
    Dato($per, 'Estado:', p.Estado + (p.salida ? ' (salida ' + FechaTexto(p.salida) + ')' : ''));
    Dato($per, 'Saldo de corte:', TextoCorte(p.corte, p.SaldoCorte));

    var $sal = $('#vaSaldo').empty();
    Dato($sal, 'Saldo al ' + FechaTexto(s.fechaCalculo) + ':', Num(s.Saldo) + ' días');
    Dato($sal, 'Periodos cumplidos:', Num(s.Disponible));
    Dato($sal, 'Proporcional:', Num(s.Proporcional));
    Dato($sal, 'Usado en curso:', Num(s.UsadoEnCurso));
    $sal.find('.va-dato').first().toggleClass('va-neg', Number(s.Saldo) < 0);

    Mostrar('#vaEmitidos', r.Emitidos > 0
        ? 'Esta persona tiene ' + r.Emitidos + (r.Emitidos === 1 ? ' estado individual emitido' : ' estados individuales emitidos') +
          ' (el último el ' + FechaTexto(r.ultimoEmitido) + '). Si cambia el corte o una compra, el papel firmado queda desactualizado: emita uno nuevo desde Saldos.'
        : '');

    $('#txtCorteFecha').val(p.corte || '');
    $('#txtCorteSaldo').val(NumCampo(p.SaldoCorte));
    $('#txtCorteMotivo').val('');
    $('#btnCorteQuitar').toggle(!!p.corte);

    $('#txtCompraFecha').val('');
    $('#txtCompraDias').val('');
    $('#txtCompraMotivo').val('');
    PreviaCompra();

    var $bc = $('#tblCompras tbody').empty();
    if (!r.compras.length) { $('<tr><td colspan="6" class="text-muted">Sin compras registradas.</td></tr>').appendTo($bc); }
    $.each(r.compras, function (i, c) {
        var anulada = c.Estado === 'DADA BAJA';
        var $t = $('<tr/>').toggleClass('va-anulada', anulada).appendTo($bc);
        Celda($t, FechaTexto(c.fecha), 'va-nowrap');
        Celda($t, Num(c.Dias), 'va-num va-nowrap');
        Celda($t, anulada ? 'ANULADA' : (c.Situacion === 'DESCUENTA' ? 'DESCUENTA' : c.Situacion), 'va-nowrap');
        Celda($t, c.Origen, 'va-nowrap');
        Celda($t, c.Observacion || '');
        var $a = $('<td class="va-acciones va-nowrap"/>').appendTo($t);
        if (!anulada) {
            $('<button type="button" class="btn btn-default btn-xs va-anular"/>').text('Anular').attr('data-id', c.IdMovimiento).appendTo($a);
        }
    });

    var $bb = $('#tblBitacora tbody').empty();
    if (!r.bitacora.length) { $('<tr><td colspan="6" class="text-muted">Sin cambios registrados desde la carga del Excel.</td></tr>').appendTo($bb); }
    $.each(r.bitacora, function (i, b) {
        var $t = $('<tr/>').appendTo($bb);
        Celda($t, b.fecha, 'va-nowrap');
        Celda($t, b.Usuario, 'va-nowrap');
        Celda($t, (b.Tipo === 'CORTE' ? 'Corte' : 'Compra') + ': ' + b.Accion.toLowerCase(), 'va-nowrap');
        Celda($t, b.fechaAntes ? TextoCorte(b.fechaAntes, b.DiasAntes) : '—', 'va-nowrap');
        Celda($t, b.fechaDespues ? TextoCorte(b.fechaDespues, b.DiasDespues) : '—', 'va-nowrap');
        Celda($t, b.Motivo);
    });

    $('#vaResultado').show();
}

function DatosCorte() {
    return { idColaborador: Id(), fecha: $('#txtCorteFecha').val(), saldo: NumeroAlServidor($('#txtCorteSaldo').val()) };
}

function SimularCorte() {
    if (!vaActual) { return; }
    var d = DatosCorte(), $b = $('#btnCorteSimular');
    Ocupado($b, true);
    Llamar('SimularCorte', d, function (s) {
        var antes = Number(vaActual.saldo.Saldo), despues = Number(s.Saldo), dif = despues - antes;
        Mostrar('#vaSimulacion', 'Con ' + TextoCorte(d.fecha, d.saldo) + ', el saldo al ' + FechaTexto(s.fechaCalculo) +
            ' pasaría de ' + Num(antes) + ' a ' + Num(despues) + ' días (' + (dif > 0 ? '+' : '') + Num(dif) + '). Todavía no se guardó nada.');
    }, function () { Ocupado($b, false); });
}

function PedirQuitar() {
    if (!vaActual) { return; }
    var p = vaActual.persona;
    $('#vaQuitarTexto').text('Se quitará el corte de ' + TextoCorte(p.corte, p.SaldoCorte) +
        ': el saldo se volverá a calcular desde el ingreso, con todas las solicitudes. Se usa el motivo escrito arriba. ');
    $('#vaQuitarConfirmar').show();
}

function GuardarCorte(quitar) {
    if (!vaActual) { return; }
    var p = vaActual.persona;
    var d = quitar ? { idColaborador: Id(), fecha: '', saldo: '' } : DatosCorte();
    d.fechaAnterior = p.corte || '';
    d.saldoAnterior = p.SaldoCorte === null || p.SaldoCorte === undefined ? '' : String(p.SaldoCorte);
    d.motivo = $('#txtCorteMotivo').val();
    var $b = $(quitar ? '#btnQuitarSi' : '#btnCorteGuardar');
    Ocupado($b, true);
    Llamar('GuardarCorte', d, function () {
        Cargar(quitar ? 'Corte quitado.' : 'Saldo de corte guardado.');
    }, function () { Ocupado($b, false); });
}

function PreviaCompra() {
    var t = NumeroAlServidor($('#txtCompraDias').val());
    if (!vaActual || !t || isNaN(Number(t))) { $('#vaCompraPrevia').text(''); return; }
    var antes = Number(vaActual.saldo.Saldo);
    $('#vaCompraPrevia').text('Saldo ' + Num(antes) + ' → quedaría ' + Num(antes - Number(t)) + ' días.');
}

function RegistrarCompra() {
    if (!vaActual) { return; }
    var $b = $('#btnCompraRegistrar');
    Ocupado($b, true);
    Llamar('RegistrarCompra', {
        idColaborador: Id(),
        fecha: $('#txtCompraFecha').val(),
        dias: NumeroAlServidor($('#txtCompraDias').val()),
        motivo: $('#txtCompraMotivo').val()
    }, function () { Cargar('Compra registrada.'); }, function () { Ocupado($b, false); });
}

/* Abre, debajo de la compra, una fila para el motivo y la confirmacion. */
function PedirAnular() {
    var $tr = $(this).closest('tr');
    if ($tr.next().hasClass('va-anular-fila')) { return; }
    var $f = $('<tr class="va-anular-fila"/>').insertAfter($tr);
    var $td = $('<td colspan="6"/>').appendTo($f);
    $('<textarea class="form-control va-motivo" maxlength="1000"/>').attr('placeholder', 'Motivo de la anulación').appendTo($td);
    $('<div style="margin-top: 6px"/>')
        .append($('<button type="button" class="btn btn-danger btn-xs va-anular-si"/>').text('Anular compra').attr('data-id', $(this).attr('data-id')))
        .append(' ')
        .append($('<button type="button" class="btn btn-default btn-xs va-anular-no"/>').text('Cancelar'))
        .appendTo($td);
}

function AnularCompra() {
    var $b = $(this), $f = $b.closest('tr');
    Ocupado($b, true);
    Llamar('AnularCompra', {
        idColaborador: Id(),
        idMovimiento: $b.attr('data-id'),
        motivo: $f.find('textarea').val()
    }, function () { Cargar('Compra anulada.'); }, function () { Ocupado($b, false); });
}
