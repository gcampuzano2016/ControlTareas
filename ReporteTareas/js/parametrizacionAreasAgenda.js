/* ==========================================================================
   Areas de la agenda — ParametrizacionAreasAgenda.aspx
   Handler: AdministrarAgenda.ashx (ListarAreas, GuardarArea, ListarPersonas,
   AsignarArea). Las validaciones de verdad estan en el servidor.
   ========================================================================== */

var paAreas = [];
var paPersonas = [];
var paAvisoTimer = null;

$(document).ready(function () {
    $('#paNueva').on('click', function () {
        FilaArea({ IdArea: 0, Nombre: '', Color: '#0078D4', Activa: true, EsSinArea: false, Personas: 0 }, true).prependTo('#paAreas');
    });
    $('#paBuscar').on('input', DibujarPersonas).on('keydown', SinEnter);
    $('#paFiltro').on('change', DibujarPersonas);
    CargarTodo();
});

/* La pagina vive dentro del <form> de la maestra: Enter haria un postback. */
function SinEnter(e) { if (e.which === 13) { e.preventDefault(); } }

function Llamar(accion, parametros, ok, fallo) {
    $.ajax({
        type: 'POST',
        url: 'AdministrarAgenda.ashx',
        data: JSON.stringify([{ action: accion, parameters: parametros }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') { Fallar((r && r.mensaje) || 'No se pudo completar la operación.', fallo); return; }
            Mostrar('#paError', '');
            if (r.mensaje) { Aviso(r.mensaje, r.tipoMensaje); }
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
    if (fallo) { fallo(mensaje); } else { Mostrar('#paError', mensaje); }
}

function CargarTodo() {
    Llamar('ListarAreas', {}, function (a) {
        paAreas = a || [];
        DibujarAreas();
        Llamar('ListarPersonas', {}, function (p) {
            paPersonas = p || [];
            LlenarFiltro();
            DibujarPersonas();
        });
    });
}

function DibujarAreas() {
    var $t = $('#paAreas').empty();
    $.each(paAreas, function (i, a) { FilaArea(a, false).appendTo($t); });
}

function FilaArea(a, nueva) {
    var $nombre = $('<input type="text" class="form-control input-sm" maxlength="100" />')
        .val(a.Nombre).prop('disabled', a.EsSinArea).on('keydown', SinEnter);
    var $color = $('<input type="color" />').val(a.Color);
    var $activa = $('<input type="checkbox" />').prop('checked', a.Activa).prop('disabled', a.EsSinArea);
    var $guardar = $('<button type="button" class="btn btn-primary btn-sm">Guardar</button>');
    $guardar.on('click', function () {
        $guardar.prop('disabled', true);
        Llamar('GuardarArea', {
            id: a.IdArea,
            nombre: $.trim($nombre.val() || ''),
            color: $color.val(),
            activa: $activa.prop('checked') ? '1' : '0'
        }, function () { CargarTodo(); }, function (m) { $guardar.prop('disabled', false); Mostrar('#paError', m); });
    });
    return $('<tr></tr>').append(
        $('<td></td>').append($nombre),
        $('<td></td>').append($color),
        $('<td></td>').append($activa),
        $('<td></td>').text(nueva ? '' : a.Personas),
        $('<td></td>').append($guardar));
}

function LlenarFiltro() {
    var $f = $('#paFiltro');
    var actual = $f.val() || '';
    $f.empty().append('<option value="">Todas las áreas</option>');
    $.each(paAreas, function (i, a) {
        $f.append($('<option></option>').val(String(a.IdArea)).text(a.Nombre + (a.Activa ? '' : ' (inactiva)')));
    });
    $f.val(actual);
    if ($f.val() === null) { $f.val(''); }
}

function DibujarPersonas() {
    var texto = $.trim($('#paBuscar').val() || '').toLowerCase();
    var filtro = $('#paFiltro').val() || '';
    var $t = $('#paPersonas').empty();
    $.each(paPersonas, function (i, p) {
        if (texto && p.Nombre.toLowerCase().indexOf(texto) < 0) { return; }
        if (filtro && String(p.IdArea) !== filtro) { return; }
        var $sel = $('<select class="form-control input-sm"></select>');
        $.each(paAreas, function (j, a) {
            if (a.Activa || a.IdArea === p.IdArea) { $sel.append($('<option></option>').val(String(a.IdArea)).text(a.Nombre)); }
        });
        $sel.val(String(p.IdArea));
        $sel.on('change', function () {
            $sel.prop('disabled', true);
            Llamar('AsignarArea', { cod: p.CodUsuario, idArea: $sel.val() },
                function () { CargarTodo(); },
                function (m) { $sel.val(String(p.IdArea)).prop('disabled', false); Mostrar('#paError', m); });
        });
        $t.append($('<tr></tr>').append($('<td></td>').text(p.Nombre), $('<td></td>').append($sel)));
    });
    if (!$t.children().length) { $t.append('<tr><td colspan="2" class="text-muted">Sin personas para mostrar.</td></tr>'); }
}

function Mostrar(selector, texto) { $(selector).text(texto || '').toggle(!!texto); }

function Aviso(texto, tipo) {
    var $a = $('#paAviso');
    $a.removeClass('alert-success alert-warning alert-info alert-danger')
        .addClass('alert-' + (tipo || 'info')).text(texto).show();
    clearTimeout(paAvisoTimer);
    paAvisoTimer = setTimeout(function () { $a.hide(); }, 5000);
}
