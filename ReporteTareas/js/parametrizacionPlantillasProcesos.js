/* ==========================================================================
   Plantillas de procesos — ParametrizacionPlantillasProcesos.aspx
   Handler: AdministrarAgenda.ashx (ListarPlantillas, GuardarPlantilla).
   Una plantilla no se borra: se desactiva. Las validaciones de verdad estan
   en el servidor.
   ========================================================================== */

var ppPlantillas = [];
var ppAreas = [];
var ppEditando = null;   // { id, pasos: [...] } mientras el editor esta abierto
var ppAvisoTimer = null;

$(document).ready(function () {
    $('#ppNueva').on('click', function () {
        Abrir({ IdPlantilla: 0, Nombre: '', Descripcion: '', Activa: true, TieneProcesos: false, Pasos: [] });
    });
    $('#ppAgregar').on('click', function () { LeerPasos(); ppEditando.pasos.push(PasoVacio()); DibujarPasos(); });
    $('#ppCancelar').on('click', Cerrar);
    $('#ppGuardar').on('click', Guardar);
    $('#ppNombre').on('keydown', SinEnter);
    Cargar();
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
            Mostrar('#ppError', '');
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
    if (fallo) { fallo(mensaje); } else { Mostrar('#ppError', mensaje); }
}

function Cargar() {
    Llamar('ListarPlantillas', {}, function (r) {
        ppPlantillas = (r && r.Plantillas) || [];
        ppAreas = (r && r.Areas) || [];
        DibujarLista();
    });
}

function DibujarLista() {
    var $t = $('#ppLista').empty();
    $.each(ppPlantillas, function (i, p) {
        var $editar = $('<button type="button" class="btn btn-default btn-sm">Editar</button>')
            .on('click', function () { Abrir(p); });
        $t.append($('<tr></tr>').append(
            $('<td></td>').text(p.Nombre),
            $('<td></td>').text(p.Pasos.length),
            $('<td></td>').text(p.Activa ? 'Sí' : 'No'),
            $('<td></td>').text(p.TieneProcesos ? 'Sí' : 'No'),
            $('<td></td>').append($editar)));
    });
    if (!$t.children().length) { $t.append('<tr><td colspan="5" class="text-muted">Todavía no hay plantillas.</td></tr>'); }
}

function PasoVacio() { return { idArea: 0, titulo: '', plazoDias: 1, entregables: '' }; }

function Abrir(p) {
    ppEditando = {
        id: p.IdPlantilla,
        pasos: $.map(p.Pasos, function (s) {
            return { idArea: s.IdArea, titulo: s.Titulo, plazoDias: s.PlazoDias, entregables: s.Entregables };
        })
    };
    if (!ppEditando.pasos.length) { ppEditando.pasos.push(PasoVacio()); }
    $('#ppEditorTitulo').text(p.IdPlantilla ? 'Editar plantilla' : 'Nueva plantilla');
    $('#ppNombre').val(p.Nombre);
    $('#ppDescripcion').val(p.Descripcion);
    $('#ppActiva').prop('checked', !!p.Activa);
    $('#ppNotaProcesos').toggle(!!p.TieneProcesos);
    Mostrar('#ppError', '');
    DibujarPasos();
    $('#ppListaPanel').hide();
    $('#ppEditor').show();
    $('#ppGuardar').prop('disabled', false);
}

function Cerrar() {
    ppEditando = null;
    $('#ppEditor').hide();
    $('#ppListaPanel').show();
    Mostrar('#ppError', '');
}

/* Vuelca a ppEditando.pasos lo que hay escrito en pantalla. */
function LeerPasos() {
    $('#ppPasos .pp-paso').each(function (i) {
        var $p = $(this);
        ppEditando.pasos[i] = {
            idArea: parseInt($p.find('.pp-area').val(), 10) || 0,
            titulo: $p.find('.pp-titulo').val() || '',
            plazoDias: $p.find('.pp-plazo').val(),
            entregables: $p.find('.pp-entregables').val() || ''
        };
    });
}

function DibujarPasos() {
    var $c = $('#ppPasos').empty();
    var n = ppEditando.pasos.length;
    $.each(ppEditando.pasos, function (i, s) { $c.append(FilaPaso(s, i, n)); });
}

function FilaPaso(s, i, n) {
    var $area = $('<select class="form-control input-sm pp-area"></select>');
    $area.append($('<option value="0">— Elija un área —</option>'));
    $.each(ppAreas, function (j, a) {
        /* Solo se ofrecen las activas; la inactiva que ya tiene el paso se muestra para no perderla en silencio. */
        if (a.Activa || a.IdArea === s.idArea) {
            $area.append($('<option></option>').val(String(a.IdArea)).text(a.Nombre + (a.Activa ? '' : ' (inactiva)')));
        }
    });
    $area.val(String(s.idArea));
    if ($area.val() === null) { $area.val('0'); }

    var $chip = $('<span class="pp-chip"></span>');
    var pintar = function () {
        var color = '#ffffff';
        var id = parseInt($area.val(), 10);
        $.each(ppAreas, function (j, a) { if (a.IdArea === id) { color = a.Color; } });
        $chip.css('background-color', color);
    };
    $area.on('change', pintar);
    pintar();

    var $titulo = $('<input type="text" class="form-control input-sm pp-titulo" maxlength="200" />')
        .val(s.titulo).on('keydown', SinEnter);
    var $plazo = $('<input type="number" class="form-control input-sm pp-plazo" min="1" max="60" step="1" />')
        .val(s.plazoDias).on('keydown', SinEnter);
    var $entregables = $('<textarea class="form-control input-sm pp-entregables" rows="3" maxlength="2000" placeholder="Un entregable por línea"></textarea>')
        .val(s.entregables);

    var $subir = $('<button type="button" class="btn btn-default btn-xs" title="Subir">&uarr;</button>')
        .prop('disabled', i === 0).on('click', function () { Mover(i, -1); });
    var $bajar = $('<button type="button" class="btn btn-default btn-xs" title="Bajar">&darr;</button>')
        .prop('disabled', i === n - 1).on('click', function () { Mover(i, 1); });
    var $quitar = $('<button type="button" class="btn btn-danger btn-xs">Quitar</button>')
        .prop('disabled', n === 1).on('click', function () {
            LeerPasos(); ppEditando.pasos.splice(i, 1); DibujarPasos();
        });

    return $('<div class="pp-paso"></div>').append(
        $('<div class="pp-paso-cab"></div>').append(
            $('<span></span>').text('Paso ' + (i + 1) + ' '), $subir, ' ', $bajar, ' ', $quitar),
        $('<div class="row"></div>').append(
            $('<div class="col-lg-4 form-group"></div>').append($('<label>Área</label>'),
                $('<div></div>').append($chip, $area.css({ display: 'inline-block', width: 'calc(100% - 20px)' }))),
            $('<div class="col-lg-6 form-group"></div>').append($('<label>Título</label>'), $titulo),
            $('<div class="col-lg-2 form-group"></div>').append($('<label>Plazo (días hábiles, 1 a 60)</label>'), $plazo)),
        $('<div class="form-group"></div>').append($('<label>Entregables</label>'), $entregables));
}

function Mover(i, delta) {
    LeerPasos();
    var j = i + delta;
    var t = ppEditando.pasos[i];
    ppEditando.pasos[i] = ppEditando.pasos[j];
    ppEditando.pasos[j] = t;
    DibujarPasos();
}

function Guardar() {
    LeerPasos();
    var pasos = [];
    for (var i = 0; i < ppEditando.pasos.length; i++) {
        var s = ppEditando.pasos[i];
        var plazo = parseInt(s.plazoDias, 10);
        if (!s.idArea) { Mostrar('#ppError', 'Paso ' + (i + 1) + ': elija un área.'); return; }
        if (!$.trim(s.titulo)) { Mostrar('#ppError', 'Paso ' + (i + 1) + ': el título es obligatorio.'); return; }
        if (isNaN(plazo) || plazo < 1 || plazo > 60) { Mostrar('#ppError', 'Paso ' + (i + 1) + ': el plazo va de 1 a 60 días hábiles.'); return; }
        var lineas = String(s.entregables || '').split(/?
/);
        for (var k = 0; k < lineas.length; k++) {
            if (lineas[k].length > 200) { Mostrar('#ppError', 'Paso ' + (i + 1) + ': cada entregable (una línea) admite hasta 200 caracteres. Acorte la línea ' + (k + 1) + '.'); return; }
        }
        pasos.push({ idArea: s.idArea, titulo: $.trim(s.titulo), plazoDias: plazo, entregables: s.entregables });
    }
    var $g = $('#ppGuardar').prop('disabled', true);
    Llamar('GuardarPlantilla', {
        id: ppEditando.id,
        nombre: $.trim($('#ppNombre').val() || ''),
        descripcion: $.trim($('#ppDescripcion').val() || ''),
        activa: $('#ppActiva').prop('checked') ? '1' : '0',
        pasos: pasos
    }, function () { Cerrar(); Cargar(); }, function (m) { $g.prop('disabled', false); Mostrar('#ppError', m); });
}

function Mostrar(selector, texto) { $(selector).text(texto || '').toggle(!!texto); }

function Aviso(texto, tipo) {
    var $a = $('#ppAviso');
    $a.removeClass('alert-success alert-warning alert-info alert-danger')
        .addClass('alert-' + (tipo || 'info')).text(texto).show();
    clearTimeout(ppAvisoTimer);
    /* Un aviso de correo fallido (warning) no se oculta solo. */
    if (tipo !== 'warning') { ppAvisoTimer = setTimeout(function () { $a.hide(); }, 5000); }
}
