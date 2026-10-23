/* ==========================================================================
   Equipo y agenda — Agenda.aspx
   Handler: AdministrarAgenda.ashx
   Este archivo NO decide permisos ni estados: quien se ve, que se puede
   editar o marcar, y que esta completa o atrasada llegan resueltos del
   servidor (NegAgendaVista). Solo dibuja y envia.
   ========================================================================== */

var agInicio = null;     /* respuesta de Inicio */
var agLunes = '';        /* lunes de la semana en pantalla (yyyy-mm-dd) */
var agVista = '';        /* 'equipo' | 'persona' */
var agPersona = '';      /* Cod_Usuario de la agenda abierta */
var agEquipo = [];       /* tarjetas de area de la semana */
var agAgenda = null;     /* vista de la persona abierta */
var agEditando = null;   /* actividad en edicion; null = nueva */
var agConsulta = 0;      /* solo se dibuja la ultima consulta */
var agGuardando = false;
var agAvisoTimer = null;

var AG_MESES = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic'];

$(document).ready(function () {
    $('#agSemAnt').on('click', function () { MoverSemana(-7); });
    $('#agSemSig').on('click', function () { MoverSemana(7); });
    $('#agSemHoy').on('click', function () { agLunes = agInicio.Lunes; Recargar(); });
    $('#agBuscar').on('input', DibujarEquipo);
    $('#agMiAgenda').on('click', function () { AbrirPersona(agInicio.CodUsuario); });
    $('#agVolver').on('click', AbrirEquipo);
    $('#agAsignar').on('click', function () { AbrirFormulario(null, ''); });
    $('#agNueva').on('click', function () { AbrirFormulario(null, agPersona); });
    $('#agFormGuardar').on('click', Guardar);
    /* La pagina vive dentro del <form> de la maestra: Enter haria un postback. */
    $('#agBuscar, #agFormTexto, #agFormFecha').on('keydown', function (e) { if (e.which === 13) { e.preventDefault(); } });
    $(document).on('click', '.ag-modal [data-cerrar]', function () { Cerrar($(this).closest('.ag-modal')); });

    Llamar('Inicio', {}, function (r) {
        agInicio = r;
        agLunes = r.Lunes;
        if (r.TieneEquipo) { AbrirEquipo(); } else { AbrirPersona(r.CodUsuario); }
    }, function (mensaje) {
        $('.ag-toolbar').hide();
        Mostrar('#agError', mensaje);
    });
});

/* ------------------------------------------------------------ servidor */

/* fallo(mensaje), si viene, se encarga del error; si no, va arriba en #agError. */
function Llamar(accion, parametros, ok, fallo) {
    $.ajax({
        type: 'POST',
        url: 'AdministrarAgenda.ashx',
        data: JSON.stringify([{ action: accion, parameters: parametros }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') {
                Fallar((r && r.mensaje) || 'No se pudo completar la operación.', fallo);
                return;
            }
            Mostrar('#agError', '');
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
    if (fallo) { fallo(mensaje); } else { Mostrar('#agError', mensaje); }
}

function Recargar() {
    if (agVista === 'equipo') { CargarEquipo(); } else { CargarPersona(); }
}

/* ------------------------------------------------------------ semana */

function DeIso(iso) { var p = iso.split('-'); return new Date(+p[0], +p[1] - 1, +p[2]); }

function AIso(d) {
    var m = d.getMonth() + 1, dia = d.getDate();
    return d.getFullYear() + '-' + (m < 10 ? '0' : '') + m + '-' + (dia < 10 ? '0' : '') + dia;
}

function SumarDias(iso, n) { var d = DeIso(iso); d.setDate(d.getDate() + n); return AIso(d); }

function FechaCorta(iso) { var d = DeIso(iso); return d.getDate() + ' ' + AG_MESES[d.getMonth()]; }

function MoverSemana(dias) { agLunes = SumarDias(agLunes, dias); Recargar(); }

function DibujarSemana() {
    var viernes = SumarDias(agLunes, 4);
    var texto = FechaCorta(agLunes) + ' – ' + FechaCorta(viernes) + ' ' + DeIso(viernes).getFullYear();
    if (agLunes === agInicio.Lunes) { texto += ' · Esta semana'; }
    $('#agSemLabel').text(texto);
}

/* ------------------------------------------------------------ equipo */

function AbrirEquipo() {
    agVista = 'equipo';
    agPersona = '';
    $('#vistaPersona').hide();
    $('#vistaEquipo').show();
    $('#agBuscar, #agMiAgenda, #agAsignar').show();
    CargarEquipo();
}

function CargarEquipo() {
    var numero = ++agConsulta;
    DibujarSemana();
    Llamar('Equipo', { lunes: agLunes }, function (r) {
        if (numero !== agConsulta) { return; }
        agEquipo = r || [];
        DibujarEquipo();
    });
}

function DibujarEquipo() {
    var texto = $.trim($('#agBuscar').val() || '').toLowerCase();
    var $c = $('#agAreas').empty();
    if (texto) {
        var halladas = [];
        $.each(agEquipo, function (i, area) {
            $.each(area.Personas, function (j, p) {
                if (p.Nombre.toLowerCase().indexOf(texto) >= 0) { halladas.push({ persona: p, area: area }); }
            });
        });
        if (!halladas.length) {
            $c.append('<div class="ag-vacio">No se encontró ningún colaborador con ese nombre.</div>');
            return;
        }
        var $g = $('<div class="ag-personas"></div>').appendTo($c);
        $.each(halladas, function (i, h) { $g.append(TarjetaPersona(h.persona, h.area)); });
        return;
    }
    if (!agEquipo.length) { $c.append('<div class="ag-vacio">No hay personas para mostrar.</div>'); return; }
    $.each(agEquipo, function (i, area) { $c.append(TarjetaArea(area)); });
}

function TarjetaArea(area) {
    var n = area.Personas.length;
    var avatares = '';
    $.each(area.Personas.slice(0, 4), function (i, p) {
        avatares += '<span class="ag-avatar" style="background:' + Html(area.Color) + '">' + Html(p.Iniciales) + '</span>';
    });
    if (n > 4) { avatares += '<span class="ag-avatar ag-mas">+' + (n - 4) + '</span>'; }
    var c = area.Contador;
    var $t = $('<div class="ag-area"></div>').css('border-top-color', area.Color).html(
        '<div class="ag-area-nombre">' + Html(area.Area) + '</div>' +
        '<div class="ag-area-cuenta">' + n + (n === 1 ? ' colaborador' : ' colaboradores') + '</div>' +
        '<div class="ag-avatares">' + avatares + '</div>' +
        '<div class="ag-cifras"><span><b>' + c.Actividades + '</b> actividades</span>' +
        '<span class="' + (c.Atrasadas ? 'ag-rojo' : '') + '"><b>' + c.Atrasadas + '</b> atrasadas</span></div>');
    $t.on('click', function () { AbrirArea(area); });
    return $t;
}

function TarjetaPersona(p, area) {
    var c = p.Contador;
    var $t = $('<div class="ag-persona"></div>').html(
        '<div class="ag-persona-top"><span class="ag-avatar" style="background:' + Html(area.Color) + '">' + Html(p.Iniciales) + '</span>' +
        '<div><div class="ag-nombre">' + Html(p.Nombre) + '</div>' +
        '<span class="ag-chip" style="background:' + Html(area.Color) + '">' + Html(area.Area) + '</span></div></div>' +
        '<div class="ag-cifras"><span><b>' + c.Actividades + '</b> actividades</span>' +
        (c.Porcentaje === null ? '' : '<span><b>' + c.Porcentaje + '%</b> entregables</span>') +
        '<span class="' + (c.Atrasadas ? 'ag-rojo' : '') + '"><b>' + c.Atrasadas + '</b> atrasadas</span></div>');
    $t.on('click', function () { Cerrar($('#agModalArea')); AbrirPersona(p.CodUsuario); });
    return $t;
}

function AbrirArea(area) {
    $('#agModalAreaTitulo').text(area.Area);
    $('#agModalAreaPunto').css('background', area.Color);
    var $g = $('#agModalAreaPersonas').empty();
    $.each(area.Personas, function (i, p) { $g.append(TarjetaPersona(p, area)); });
    Abrir($('#agModalArea'));
}

/* ------------------------------------------------------------ agenda de una persona */

function AbrirPersona(cod) {
    agVista = 'persona';
    agPersona = cod;
    $('#vistaEquipo').hide();
    $('#agBuscar, #agMiAgenda, #agAsignar').hide();
    $('#vistaPersona').show();
    $('#agVolver').toggle(!!agInicio.TieneEquipo);
    CargarPersona();
}

function CargarPersona(despues) {
    var numero = ++agConsulta;
    DibujarSemana();
    Llamar('Agenda', { cod: agPersona, lunes: agLunes }, function (v) {
        if (numero !== agConsulta) { return; }
        agAgenda = v;
        DibujarPersona();
        if (despues) { despues(); }
    });
}

function DibujarPersona() {
    var v = agAgenda;
    $('#agPerIniciales').text(v.Iniciales).css('background', v.Color);
    $('#agPerNombre').text(v.Nombre);
    $('#agPerArea').text(v.Area).css('background', v.Color);
    $('#agNueva').text(v.EsPropia ? '+ Nueva actividad' : '+ Asignar a esta persona').toggle(!!v.PuedeAsignar);

    var $a = $('#agAtrasadas').empty();
    if (v.Atrasadas.length) {
        var $f = $('<div class="ag-franja"></div>').appendTo($a);
        $f.append($('<div class="ag-franja-titulo"></div>').text('Atrasadas (' + v.Atrasadas.length + ')'));
        var $lista = $('<div class="ag-franja-lista"></div>').appendTo($f);
        $.each(v.Atrasadas, function (i, it) { $lista.append(TarjetaActividad(it, true)); });
    }

    var $d = $('#agDias').empty();
    $.each(v.Dias, function (i, dia) {
        var $col = $('<div class="ag-dia"></div>').appendTo($d);
        $col.append('<div class="ag-dia-cab' + (dia.EsHoy ? ' ag-hoy' : '') + '"><span class="ag-dia-nombre">' + Html(dia.Nombre) +
            '</span><span class="ag-dia-fecha">' + FechaCorta(dia.Fecha) + '</span></div>');
        var $cuerpo = $('<div class="ag-dia-cuerpo"></div>').appendTo($col);
        if (!dia.Actividades.length) { $cuerpo.append('<div class="ag-vacio-dia">Sin actividades</div>'); }
        $.each(dia.Actividades, function (j, it) { $cuerpo.append(TarjetaActividad(it, false)); });
    });
}

function TarjetaActividad(it, conFecha) {
    var total = it.Entregables.length, hechos = 0;
    $.each(it.Entregables, function (i, e) { if (e.Hecho) { hechos++; } });
    var html = '<div class="ag-tit">' + Html(it.Titulo) + '</div>';
    if (conFecha) { html += '<div class="ag-meta">Vencía el ' + FechaCorta(it.FechaEntrega) + '</div>'; }
    if (total) {
        var pct = Math.round(hechos * 100 / total);
        html += '<div class="ag-progreso"><div class="ag-barra"><div style="width:' + pct + '%"></div></div><span>' + hechos + '/' + total + '</span></div>';
    }
    if (it.AsignadoPor) { html += '<div class="ag-meta">Asignó: ' + Html(it.AsignadoPor) + '</div>'; }
    if (it.EsPasoDeProceso) { html += '<div class="ag-meta"><span class="ag-proceso">Proceso: ' + Html(it.NombreProceso) + '</span></div>'; }
    var clase = 'ag-act' + (it.Completa ? ' ag-completa' : '') + (it.Atrasada ? ' ag-atrasada' : '');
    var $t = $('<div></div>').addClass(clase).html(html);
    $t.on('click', function () { AbrirDetalle(it.IdActividad); });
    return $t;
}

/* ------------------------------------------------------------ detalle */

function Buscar(id) {
    var hallada = null;
    if (!agAgenda) { return null; }
    $.each(agAgenda.Atrasadas, function (i, it) { if (it.IdActividad === id) { hallada = it; } });
    $.each(agAgenda.Dias, function (i, d) {
        $.each(d.Actividades, function (j, it) { if (it.IdActividad === id) { hallada = it; } });
    });
    return hallada;
}

/* Vuelve a pedir la agenda y reabre el detalle con lo que quedo guardado. */
function RefrescarDetalle(id) {
    CargarPersona(function () { AbrirDetalle(id); });
}

function AbrirDetalle(id) {
    var it = Buscar(id);
    if (!it) { Cerrar($('#agModalDetalle')); return; }
    Mostrar('#agDetError', '');
    $('#agDetTitulo').text(it.Titulo);
    var estado = it.Completa ? 'Completa' : (it.Atrasada ? 'Atrasada' : 'Pendiente');
    $('#agDetMeta').text('Entrega: ' + FechaCorta(it.FechaEntrega) + ' · ' + estado + (it.AsignadoPor ? ' · Asignó: ' + it.AsignadoPor : ''));
    if (it.EsPasoDeProceso) {
        /* Paso de un proceso: se gestiona desde Procesos (el servidor ya manda PuedeEditar = false). */
        $('#agDetMeta').append(' · ').append($('<a class="ag-proceso"></a>').attr('href', 'Procesos.aspx').text('Proceso: ' + it.NombreProceso));
    }

    var $l = $('#agDetEntregables').empty();
    if (!it.Entregables.length) { $l.append('<div class="ag-meta">Sin entregables.</div>'); }
    $.each(it.Entregables, function (i, e) {
        var $chk = $('<input type="checkbox" />').prop('checked', e.Hecho).prop('disabled', !it.PuedeMarcar);
        $chk.on('change', function () {
            var marcar = $chk.prop('checked');
            $chk.prop('disabled', true);
            var enviar = function () {
                Llamar('MarcarEntregable', { idActividad: it.IdActividad, idEntregable: e.IdEntregable, hecho: marcar ? '1' : '0' },
                    function () { RefrescarDetalle(id); },
                    function (m) { $chk.prop('checked', !marcar).prop('disabled', false); Mostrar('#agDetError', m); });
            };
            if (it.EsPasoDeProceso && it.AvisoAlCompletar && marcar && PendientesDe(it) === 1) {
                /* Es el ultimo entregable: completa el paso. */
                ConfirmarPaso(it.AvisoAlCompletar, enviar, function () { $chk.prop('checked', !marcar).prop('disabled', false); });
                return;
            }
            enviar();
        });
        $l.append($('<label class="ag-entregable"></label>').append($chk).append(' ').append($('<span></span>').text(e.Descripcion)));
    });

    var $b = $('#agDetBotones').empty();
    if (it.PuedeMarcar && !it.Entregables.length) {
        $('<button type="button" class="btn btn-success"></button>')
            .text(it.Completa ? 'Marcar como pendiente' : 'Marcar como completa')
            .on('click', function () {
                var $btn = $(this).prop('disabled', true);
                var enviar = function () {
                    Llamar('MarcarCompleta', { id: it.IdActividad, completa: it.Completa ? '0' : '1' },
                        function () { RefrescarDetalle(id); },
                        function (m) { $btn.prop('disabled', false); Mostrar('#agDetError', m); });
                };
                if (it.EsPasoDeProceso && it.AvisoAlCompletar && !it.Completa) {
                    ConfirmarPaso(it.AvisoAlCompletar, enviar, function () { $btn.prop('disabled', false); });
                    return;
                }
                enviar();
            }).appendTo($b);
    }
    if (it.PuedeEditar) {
        $('<button type="button" class="btn btn-default">Editar</button>')
            .on('click', function () { Cerrar($('#agModalDetalle')); AbrirFormulario(it, agPersona); })
            .appendTo($b);
        $('<button type="button" class="btn btn-danger">Anular</button>')
            .on('click', function () {
                var $btn = $(this);
                if (!$btn.data('confirmar')) { $btn.data('confirmar', true).text('¿Seguro? Anular'); return; }
                $btn.prop('disabled', true);
                Llamar('Anular', { id: it.IdActividad },
                    function () { Cerrar($('#agModalDetalle')); CargarPersona(); },
                    function (m) { $btn.prop('disabled', false); Mostrar('#agDetError', m); });
            }).appendTo($b);
    }
    Abrir($('#agModalDetalle'));
}

/* ------------------------------------------------------------ pasos de procesos */

/* Entregables sin hacer de una actividad. */
function PendientesDe(it) {
    var n = 0;
    $.each(it.Entregables, function (i, e) { if (!e.Hecho) { n++; } });
    return n;
}

/* Dialogo propio antes de completar un paso. Nada se envia hasta Aceptar. */
function ConfirmarPaso(texto, siAcepta, siCancela) {
    $('#agConfTexto').text(texto);
    $('#agConfAceptar').off('click').on('click', function () { Cerrar($('#agModalConfirmar')); siAcepta(); });
    $('#agConfCancelar').off('click').on('click', function () { Cerrar($('#agModalConfirmar')); siCancela(); });
    Abrir($('#agModalConfirmar'));
}

/* ------------------------------------------------------------ formulario */

function AbrirFormulario(it, codFijo) {
    agEditando = it;
    $('#agFormTitulo').text(it ? 'Editar actividad' : (codFijo === agInicio.CodUsuario ? 'Nueva actividad' : 'Asignar actividad'));

    var $p = $('#agFormPersona').empty();
    $.each(agInicio.Asignables, function (i, p) {
        if (codFijo && p.CodUsuario !== codFijo) { return; }
        $p.append($('<option></option>').val(p.CodUsuario).text(p.Nombre + ' (' + p.Area + ')'));
    });
    if (!codFijo) { $p.prepend('<option value="">Seleccione...</option>').val(''); }
    $p.prop('disabled', !!codFijo || !!it);

    $('#agFormTexto').val(it ? it.Titulo : '');
    $('#agFormFecha').val(it ? it.FechaEntrega : agInicio.ProximoHabil);
    $('#agFormEntregables').val(it ? $.map(it.Entregables, function (e) { return e.Descripcion; }).join('\n') : '');
    Mostrar('#agFormError', '');
    Abrir($('#agModalForm'));
    $('#agFormTexto').focus();
}

function Guardar() {
    if (agGuardando) { return; }

    var lineas = $.grep($.map(($('#agFormEntregables').val() || '').split('\n'), function (s) { return $.trim(s); }),
        function (s) { return s !== ''; });
    /* Al editar, un entregable cuyo texto no cambio conserva su id, y con el
       su marca de hecho. Uno con texto nuevo es un entregable nuevo. */
    var libres = agEditando ? agEditando.Entregables.slice() : [];
    var entregables = $.map(lineas, function (texto) {
        var id = 0;
        for (var i = 0; i < libres.length; i++) {
            if (libres[i].Descripcion === texto) { id = libres[i].IdEntregable; libres.splice(i, 1); break; }
        }
        return { id: id, descripcion: texto };
    });

    var p = {
        id: agEditando ? agEditando.IdActividad : 0,
        titulo: $.trim($('#agFormTexto').val() || ''),
        cod: agEditando ? '' : ($('#agFormPersona').val() || ''),
        fecha: $('#agFormFecha').val() || '',
        entregables: entregables
    };
    if (!p.titulo) { Mostrar('#agFormError', 'Escriba qué hay que hacer.'); return; }
    if (!agEditando && !p.cod) { Mostrar('#agFormError', 'Elija a quién se asigna.'); return; }
    if (!p.fecha) { Mostrar('#agFormError', 'Elija la fecha de entrega.'); return; }
    var dia = DeIso(p.fecha).getDay();
    if (dia === 0 || dia === 6) { Mostrar('#agFormError', 'La fecha de entrega debe ser de lunes a viernes.'); return; }

    agGuardando = true;
    $('#agFormGuardar').prop('disabled', true);
    Llamar('GuardarActividad', p, function () {
        agGuardando = false;
        $('#agFormGuardar').prop('disabled', false);
        Cerrar($('#agModalForm'));
        Recargar();
    }, function (m) {
        agGuardando = false;
        $('#agFormGuardar').prop('disabled', false);
        Mostrar('#agFormError', m);
    });
}

/* ------------------------------------------------------------ utilidades */

function Abrir($m) { $m.addClass('ag-abierto'); }

function Cerrar($m) { $m.removeClass('ag-abierto'); }

function Mostrar(selector, texto) { $(selector).text(texto || '').toggle(!!texto); }

function Aviso(texto, tipo) {
    var $a = $('#agAviso');
    $a.removeClass('alert-success alert-warning alert-info alert-danger')
        .addClass('alert-' + (tipo || 'info')).text(texto).show();
    clearTimeout(agAvisoTimer);
    agAvisoTimer = setTimeout(function () { $a.hide(); }, tipo === 'warning' ? 10000 : 5000);
}

function Html(s) {
    return $('<div>').text(s == null ? '' : String(s)).html();
}
