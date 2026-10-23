/* ==========================================================================
   Procesos entre areas — Procesos.aspx
   Handler: AdministrarAgenda.ashx (ListarProcesos, DatosIniciar, IniciarProceso,
   ReasignarPaso, CambiarPasoPendiente, CambiarFechaPaso, CancelarProceso).
   Las reglas de verdad estan en el servidor; aqui solo se evita el viaje
   inutil. Todo texto de la base entra al DOM con .text().
   ========================================================================== */

var prDatos = null;        // resultado de ListarProcesos
var prAvisoTimer = null;
var prOkAccion = null;     // funcion del boton Aceptar del dialogo abierto
var prEnVuelo = false;

$(document).ready(function () {
    $('#prFiltroEstado, #prFiltroArea').on('change', DibujarLista);
    $('#prIniciar').on('click', AbrirIniciar);
    $('#prModalCancelar').on('click', CerrarDialogo);
    $('#prModalOk').on('click', function () { if (prOkAccion) { prOkAccion(); } });
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
            Mostrar('#prError', '');
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
    if (fallo) { fallo(mensaje); } else { Mostrar('#prError', mensaje); }
}

function Mostrar(selector, texto) { $(selector).text(texto || '').toggle(!!texto); }

function Aviso(texto, tipo) {
    var $a = $('#prAviso');
    $a.removeClass('alert-success alert-warning alert-info alert-danger')
        .addClass('alert-' + (tipo || 'info')).text(texto).show();
    clearTimeout(prAvisoTimer);
    /* Un aviso de correo fallido (warning) no se oculta solo. */
    if (tipo !== 'warning') { prAvisoTimer = setTimeout(function () { $a.hide(); }, 6000); }
}

/* "yyyy-MM-dd" -> "dd/MM/yyyy" sin pasar por Date (evita desfases de zona horaria). */
function Fecha(s) {
    if (!s) { return ''; }
    var p = s.split('-');
    return p.length === 3 ? p[2] + '/' + p[1] + '/' + p[0] : s;
}

/* Sabado o domingo de un "yyyy-MM-dd". */
function EsFinDeSemana(s) {
    var p = (s || '').split('-');
    if (p.length !== 3) { return false; }
    var d = new Date(parseInt(p[0], 10), parseInt(p[1], 10) - 1, parseInt(p[2], 10)).getDay();
    return d === 0 || d === 6;
}

/* ---------- Lista ---------- */

function Cargar() {
    Llamar('ListarProcesos', {}, function (r) {
        prDatos = r || { Procesos: [], Areas: [] };
        $('#prIniciar').toggle(!!prDatos.Activo);
        LlenarAreas();
        DibujarLista();
    });
}

/* Recarga conservando los filtros: solo se rehacen las opciones de area, respetando la elegida. */
function LlenarAreas() {
    var $s = $('#prFiltroArea');
    var actual = $s.val() || '0';
    $s.empty().append($('<option value="0">Todas</option>'));
    $.each(prDatos.Areas || [], function (i, a) {
        $s.append($('<option></option>').val(String(a.IdArea)).text(a.Nombre));
    });
    $s.val(actual);
    if ($s.val() === null) { $s.val('0'); }
}

function DibujarLista() {
    var estado = $('#prFiltroEstado').val() || '';
    var area = parseInt($('#prFiltroArea').val(), 10) || 0;
    var $l = $('#prLista').empty();
    var n = 0;
    $.each((prDatos && prDatos.Procesos) || [], function (i, p) {
        if (estado && p.Estado !== estado) { return; }
        if (area) {
            var tiene = false;
            $.each(p.Pasos, function (j, s) { if (s.IdArea === area) { tiene = true; } });
            if (!tiene) { return; }
        }
        n++;
        $l.append(Tarjeta(p));
    });
    if (!n) { $l.append($('<p class="text-muted"></p>').text('No hay procesos con estos filtros.')); }
}

var NombreEstado = { EnCurso: 'En curso', Terminado: 'Terminado', Cancelado: 'Cancelado' };

function Tarjeta(p) {
    var $cab = $('<div class="pr-cab"></div>').append(
        $('<span class="pr-nombre"></span>').text(p.Nombre),
        $('<span class="pr-estado"></span>').addClass('pr-estado-' + p.Estado).text(NombreEstado[p.Estado] || p.Estado),
        $('<span class="text-muted"></span>').text(p.Resumen));
    if (p.PuedeGestionar) {
        $cab.append($('<button type="button" class="btn btn-danger btn-xs" style="margin-left:auto">Cancelar proceso</button>')
            .on('click', function () { AbrirCancelar(p); }));
    }
    var meta = 'Plantilla: ' + p.Plantilla + ' · Inició: ' + p.IniciadoPor + ' el ' + Fecha(p.FechaInicio) +
        (p.FechaFin ? ' · Terminó el ' + Fecha(p.FechaFin) : '');
    var $pasos = $('<div class="pr-pasos"></div>');
    $.each(p.Pasos, function (i, s) { $pasos.append(Chip(p, s)); });
    var $t = $('<div class="pr-tarjeta"></div>').append($cab, $('<div class="pr-meta"></div>').text(meta), $pasos);
    if (p.Estado === 'Cancelado' && p.MotivoCancelacion) {
        $t.append($('<div class="pr-motivo"></div>').text('Motivo de la cancelación: ' + p.MotivoCancelacion));
    }
    return $t;
}

function Chip(p, s) {
    var atrasado = s.Estado === 'EnCurso' && !s.ActividadCompleta && s.FechaEntrega && prDatos.Hoy && s.FechaEntrega < prDatos.Hoy;
    var $c = $('<div class="pr-chip"></div>')
        .addClass('pr-chip-' + s.Estado).toggleClass('pr-chip-Atrasado', !!atrasado)
        .css('border-left-color', atrasado ? '#a94442' : (s.Color || '#999'));
    $c.append(
        $('<span class="pr-t"></span>').text(s.Orden + '. ' + s.Titulo),
        $('<span class="pr-a"></span>').append($('<span class="pr-punto"></span>').css('background-color', s.Color || '#fff'), $('<span></span>').text(s.Area)),
        $('<span class="pr-a"></span>').text(s.Responsable));
    if (s.Estado === 'Completo') {
        $c.append($('<span class="pr-a"></span>').text('Completo el ' + Fecha(s.FechaCompletado)));
    } else if (s.FechaEntrega) {
        $c.append($('<span class="pr-a"></span>').text((atrasado ? 'Atrasado — entrega: ' : 'Entrega: ') + Fecha(s.FechaEntrega)));
    } else if (s.Estado === 'Pendiente') {
        $c.append($('<span class="pr-a"></span>').text('Plazo: ' + s.PlazoDias + ' días hábiles'));
    }
    if (p.PuedeGestionar) {
        if (s.ResponsableInactivo) { $c.append($('<span class="pr-inactivo"></span>').text('Responsable inactivo — reasignar')); }
        var $acc = $('<div class="pr-acc"></div>');
        if (s.Estado === 'Pendiente' || s.Estado === 'EnCurso') {
            $acc.append($('<button type="button" class="btn btn-default btn-xs">Reasignar</button>').on('click', function () { AbrirReasignar(p, s); }));
        }
        if (s.Estado === 'Pendiente') {
            $acc.append($('<button type="button" class="btn btn-default btn-xs">Cambiar plazo</button>').on('click', function () { AbrirPlazo(p, s); }));
        }
        if (s.Estado === 'EnCurso') {
            $acc.append($('<button type="button" class="btn btn-default btn-xs">Cambiar fecha</button>').on('click', function () { AbrirFecha(p, s); }));
        }
        if ($acc.children().length) { $c.append($acc); }
    }
    return $c;
}

/* ---------- Dialogo propio ---------- */

/* cfg: { titulo, ancho, cuerpo ($), ok (texto), clase, onOk() que llama a Enviar }. */
function Dialogo(cfg) {
    $('#prModalTitulo').text(cfg.titulo);
    $('#prCaja').toggleClass('pr-ancha', !!cfg.ancho);
    $('#prModalCuerpo').empty().append(cfg.cuerpo);
    Mostrar('#prModalError', '');
    $('#prModalOk').text(cfg.ok || 'Aceptar').prop('disabled', false)
        .removeClass('btn-primary btn-danger').addClass(cfg.clase || 'btn-primary');
    $('#prModalCancelar').prop('disabled', false);
    prOkAccion = cfg.onOk;
    prEnVuelo = false;
    $('#prModal').addClass('pr-abierto');
}

function CerrarDialogo() {
    if (prEnVuelo) { return; }
    $('#prModal').removeClass('pr-abierto');
    $('#prModalCuerpo').empty();
    prOkAccion = null;
}

function ErrorDialogo(m) { Mostrar('#prModalError', m); }

/* Llama al handler con los botones del dialogo deshabilitados; al terminar bien cierra y recarga. */
function Enviar(accion, parametros) {
    if (prEnVuelo) { return; }
    prEnVuelo = true;
    $('#prModalOk, #prModalCancelar').prop('disabled', true);
    Mostrar('#prModalError', '');
    Llamar(accion, parametros, function () {
        prEnVuelo = false;
        CerrarDialogo();
        Cargar();
    }, function (m) {
        prEnVuelo = false;
        $('#prModalOk, #prModalCancelar').prop('disabled', false);
        ErrorDialogo(m);
    });
}

function Campo(etiqueta, $control) {
    return $('<div class="form-group"></div>').append($('<label></label>').text(etiqueta), $control);
}

function Contexto(p, s) {
    return $('<p class="text-muted"></p>').text(p.Nombre + ' — paso ' + s.Orden + ': ' + s.Titulo + ' (' + s.Area + ')');
}

/* ---------- Reasignar ---------- */

function AbrirReasignar(p, s) {
    Llamar('DatosIniciar', {}, function (d) {
        var personas = PersonasDeArea(d, s.IdArea);
        var $sel = $('<select class="form-control"></select>').append($('<option value="">— Elija una persona —</option>'));
        $.each(personas, function (i, x) {
            if (x.CodUsuario !== s.CodResponsable) { $sel.append($('<option></option>').val(x.CodUsuario).text(x.Nombre)); }
        });
        var $cuerpo = $('<div></div>').append(Contexto(p, s),
            $('<p></p>').text('Responsable actual: ' + s.Responsable), Campo('Nuevo responsable (personas activas del área)', $sel));
        Dialogo({
            titulo: 'Reasignar paso', cuerpo: $cuerpo, ok: 'Reasignar',
            onOk: function () {
                if (!$sel.val()) { ErrorDialogo('Elija a la persona responsable.'); return; }
                Enviar('ReasignarPaso', { idPaso: s.IdProcesoPaso, codResponsable: $sel.val(), estado: s.Estado });
            }
        });
    });
}

/* ---------- Cambiar plazo (paso pendiente) ---------- */

function AbrirPlazo(p, s) {
    var $tit = $('<input type="text" class="form-control" maxlength="200" />').val(s.Titulo).on('keydown', SinEnter);
    var $pl = $('<input type="number" class="form-control" min="1" max="60" step="1" />').val(s.PlazoDias).on('keydown', SinEnter);
    var $ent = $('<textarea class="form-control" rows="4" maxlength="2000"></textarea>').val(s.Entregables);
    var $cuerpo = $('<div></div>').append(Contexto(p, s), Campo('Título', $tit),
        Campo('Plazo (días hábiles, 1 a 60)', $pl), Campo('Entregables (uno por línea)', $ent));
    Dialogo({
        titulo: 'Cambiar plazo del paso', cuerpo: $cuerpo, ok: 'Guardar',
        onOk: function () {
            var plazo = parseInt($pl.val(), 10);
            if (!$.trim($tit.val())) { ErrorDialogo('El título es obligatorio.'); return; }
            if (isNaN(plazo) || plazo < 1 || plazo > 60) { ErrorDialogo('El plazo tiene que estar entre 1 y 60 días hábiles.'); return; }
            Enviar('CambiarPasoPendiente', { idPaso: s.IdProcesoPaso, titulo: $.trim($tit.val()), plazoDias: plazo, entregables: $ent.val() });
        }
    });
}

/* ---------- Cambiar fecha (paso en curso) ---------- */

function AbrirFecha(p, s) {
    var $f = $('<input type="date" class="form-control" />').val(s.FechaEntrega);
    if (prDatos.Hoy) { $f.attr('min', prDatos.Hoy); }
    var $aviso = $('<p class="text-danger" style="display:none"></p>');
    var revisar = function () {
        var malo = EsFinDeSemana($f.val());
        $aviso.text(malo ? 'La fecha de entrega no puede ser sábado ni domingo.' : '').toggle(malo);
        return !malo;
    };
    $f.on('change keyup', revisar).on('keydown', SinEnter);
    var $cuerpo = $('<div></div>').append(Contexto(p, s), Campo('Nueva fecha de entrega', $f), $aviso);
    Dialogo({
        titulo: 'Cambiar fecha de entrega', cuerpo: $cuerpo, ok: 'Guardar',
        onOk: function () {
            if (!$f.val()) { ErrorDialogo('Elija una fecha de entrega.'); return; }
            if (!revisar()) { return; }
            Enviar('CambiarFechaPaso', { idPaso: s.IdProcesoPaso, fecha: $f.val() });
        }
    });
}

/* ---------- Cancelar proceso ---------- */

function AbrirCancelar(p) {
    var $m = $('<textarea class="form-control" rows="3" maxlength="400"></textarea>');
    var $cuerpo = $('<div></div>').append(
        $('<p></p>').text('Se cancelará «' + p.Nombre + '»: los pasos sin completar quedan cancelados y la actividad del paso en curso se anula. No se puede deshacer.'),
        Campo('Motivo (obligatorio)', $m));
    Dialogo({
        titulo: 'Cancelar proceso', cuerpo: $cuerpo, ok: 'Cancelar proceso', clase: 'btn-danger',
        onOk: function () {
            if (!$.trim($m.val())) { ErrorDialogo('El motivo es obligatorio.'); return; }
            Enviar('CancelarProceso', { idProceso: p.IdProceso, motivo: $.trim($m.val()) });
        }
    });
}

/* ---------- Iniciar un proceso ---------- */

function AbrirIniciar() {
    var $b = $('#prIniciar').prop('disabled', true);
    Llamar('DatosIniciar', {}, function (d) {
        $b.prop('disabled', false);
        var plantillas = (d && d.Plantillas) || [];
        if (!plantillas.length) { Aviso('No hay plantillas activas con pasos para iniciar un proceso.', 'info'); return; }
        MostrarIniciar(d, plantillas);
    }, function (m) { $b.prop('disabled', false); Mostrar('#prError', m); });
}

function PersonasDeArea(d, idArea) {
    var r = [];
    $.each((d && d.PersonasPorArea) || [], function (i, a) { if (a.IdArea === idArea) { r = a.Personas; } });
    return r;
}

function MostrarIniciar(d, plantillas) {
    var $plantilla = $('<select class="form-control"></select>');
    $.each(plantillas, function (i, p) { $plantilla.append($('<option></option>').val(String(i)).text(p.Nombre)); });
    var $nombre = $('<input type="text" class="form-control" maxlength="150" />').on('keydown', SinEnter);
    var $desc = $('<p class="text-muted"></p>');
    var $filas = $('<div></div>');
    var actual = null;
    var bloqueado = false;

    var dibujar = function () {
        actual = plantillas[parseInt($plantilla.val(), 10)];
        $desc.text(actual.Descripcion || '');
        $nombre.val(actual.Nombre);
        $filas.empty();
        bloqueado = false;
        $.each(actual.Pasos, function (i, s) {
            var personas = PersonasDeArea(d, s.IdArea);
            var $resp = $('<select class="form-control input-sm pr-resp"></select>');
            $.each(personas, function (j, x) { $resp.append($('<option></option>').val(x.CodUsuario).text(x.Nombre)); });
            var $tit = $('<input type="text" class="form-control input-sm pr-tit" maxlength="200" />').val(s.Titulo).on('keydown', SinEnter);
            var $pl = $('<input type="number" class="form-control input-sm pr-pl" min="1" max="60" step="1" />').val(s.PlazoDias).on('keydown', SinEnter);
            var $ent = $('<textarea class="form-control input-sm pr-ent" rows="3" maxlength="2000"></textarea>').val(s.Entregables);
            var $fila = $('<div class="pr-fila"></div>').data('orden', s.Orden).append(
                $('<div class="pr-fila-cab"></div>').text('Paso ' + s.Orden + ' — ' + s.Area));
            if (!personas.length) {
                bloqueado = true;
                $fila.append($('<p class="text-danger"></p>').text('El área «' + s.Area + '» no tiene personas activas: no se puede iniciar el proceso.'));
            }
            $fila.append($('<div class="row"></div>').append(
                $('<div class="col-md-4 form-group"></div>').append($('<label>Responsable</label>'), $resp),
                $('<div class="col-md-6 form-group"></div>').append($('<label>Título</label>'), $tit),
                $('<div class="col-md-2 form-group"></div>').append($('<label>Plazo (días hábiles)</label>'), $pl)),
                $('<div class="form-group"></div>').append($('<label>Entregables (uno por línea)</label>'), $ent));
            $filas.append($fila);
        });
        $('#prModalOk').prop('disabled', bloqueado);
    };
    $plantilla.on('change', dibujar);

    var $cuerpo = $('<div></div>').append(Campo('Plantilla', $plantilla), $desc, Campo('Nombre del proceso', $nombre), $filas);
    Dialogo({
        titulo: 'Iniciar un proceso', ancho: true, cuerpo: $cuerpo, ok: 'Iniciar proceso',
        onOk: function () {
            if (bloqueado) { return; }
            var nombre = $.trim($nombre.val());
            if (!nombre) { ErrorDialogo('El nombre del proceso es obligatorio.'); return; }
            var pasos = [];
            var mal = null;
            $filas.find('.pr-fila').each(function () {
                var $f = $(this);
                var orden = $f.data('orden');
                var plazo = parseInt($f.find('.pr-pl').val(), 10);
                var titulo = $.trim($f.find('.pr-tit').val());
                if (!$f.find('.pr-resp').val()) { mal = mal || ('Paso ' + orden + ': elija un responsable.'); }
                if (!titulo) { mal = mal || ('Paso ' + orden + ': el título es obligatorio.'); }
                if (isNaN(plazo) || plazo < 1 || plazo > 60) { mal = mal || ('Paso ' + orden + ': el plazo va de 1 a 60 días hábiles.'); }
                pasos.push({ orden: orden, codResponsable: $f.find('.pr-resp').val(), titulo: titulo, plazoDias: plazo, entregables: $f.find('.pr-ent').val() || '' });
            });
            if (mal) { ErrorDialogo(mal); return; }
            Enviar('IniciarProceso', { idPlantilla: actual.IdPlantilla, nombre: nombre, pasos: pasos });
        }
    });
    dibujar();
}
