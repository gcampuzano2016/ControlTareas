/* ==========================================================================
   Calificacion ForeCast
   Handler : AdministrarCalificacionForeCast.ashx  (listar, guardar)
             DescargarCalificacionForeCast.ashx    (Excel)
   Este archivo NO calcula nada: puntajes, estados, alertas y acciones llegan
   resueltos de NegCalificacionForeCast. Solo dibuja.
   ========================================================================== */

var cfPantalla = null;
var cfTabla = null;

$(document).ready(function () {
    Cargar();
    $('#tblCalificacion tbody').on('click', 'tr.cf-fila', function (e) {
        if ($(e.target).closest('button').length) { return; }
        AlternarDetalle(this);
    });
});

function Sesion() {
    return $("#ContentPlaceHolder1_txtUsuario").val();
}

function Filtros() {
    return {
        gerente: $('#cboGerente').val() || '',
        sucursal: $('#cboSucursal').val() || '',
        etapa: $('#cboEtapa').val() || '',
        estado: $('#cboEstado').val() || '',
        marca: $('#cboMarca').val() || ''
    };
}

function Llamar(action, parameters, alTerminar) {
    $.ajax({
        type: "POST",
        url: "AdministrarCalificacionForeCast.ashx",
        data: JSON.stringify([{ "action": action, "parameters": parameters }]),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) { alTerminar(r); },
        error: function () { swal("Error", "No se pudo comunicar con el servidor.", "error"); }
    });
}

function Cargar() {
    var p = Filtros();
    p.session = Sesion();
    Llamar("ListaCalificacionForeCast", p, function (r) {
        if (!r || r.estado !== "1") {
            swal("Error", (r && r.mensaje) || "No se pudo cargar la calificación.", "error");
            return;
        }
        cfPantalla = r.resultado;
        LlenarCombo('#cboGerente', cfPantalla.Gerentes);
        LlenarCombo('#cboSucursal', cfPantalla.Sucursales);
        LlenarCombo('#cboEtapa', cfPantalla.Etapas);
        LlenarCombo('#cboMarca', cfPantalla.Marcas);
        DibujarIndicadores();
        DibujarListas();
        DibujarTabla();
    });
}

/* Conserva lo elegido: los combos llegan del conjunto sin filtrar. */
function LlenarCombo(id, valores) {
    var combo = $(id), actual = combo.val() || '';
    combo.empty().append($('<option>').val('').text('Todos'));
    $.each(valores || [], function (i, v) { combo.append($('<option>').val(v).text(v)); });
    combo.val(actual);
}

function Dinero(n) {
    var v = Math.round(Number(n) || 0).toString();
    return '$' + v.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}

function Html(s) {
    return $('<div>').text(s == null ? '' : String(s)).html();
}

function ClaseEstado(estado) {
    return estado === 'Fría' ? 'cf-Fria' : 'cf-' + estado;
}

function DibujarIndicadores() {
    $('#indTotal').text(cfPantalla.Total);
    $('#indMonto').text(Dinero(cfPantalla.MontoTotal));
    $('#indPonderado').text(Dinero(cfPantalla.MontoPonderado));
    $('#indEstados').text(cfPantalla.Calientes + ' · ' + cfPantalla.Tibias + ' · ' + cfPantalla.Frias);
    $('#indSinCalificar').text(String(cfPantalla.PorcentajeSinCalificar).replace('.', ',') + ' %');
}

function Buscar(id) {
    for (var i = 0; i < cfPantalla.Oportunidades.length; i++) {
        if (cfPantalla.Oportunidades[i].IdForeCast === id) { return cfPantalla.Oportunidades[i]; }
    }
    return null;
}

function DibujarListas() {
    var top = $('#lstTop5').empty(), riesgo = $('#lstRiesgo').empty();
    $.each(cfPantalla.Top5, function (i, id) {
        var o = Buscar(id);
        if (o) { top.append('<li><b>' + Html(o.Cliente) + '</b> – ' + Html(o.DetalleProyecto) + ' (' + Dinero(o.MontoPonderado) + ' pond.) · ' + Html(o.AccionSugerida) + '</li>'); }
    });
    $.each(cfPantalla.EnRiesgo, function (i, id) {
        var o = Buscar(id);
        if (o && o.Alertas.length) { riesgo.append('<li><b>' + Html(o.Cliente) + '</b> – ' + Html(o.DetalleProyecto) + ': ' + Html(o.Alertas[0].Texto) + '</li>'); }
    });
    if (!cfPantalla.Top5.length) { top.append('<li>Sin oportunidades calientes o tibias.</li>'); }
    if (!cfPantalla.EnRiesgo.length) { riesgo.append('<li>Ninguna.</li>'); }
}

function DibujarTabla() {
    if (cfTabla) { cfTabla.destroy(); cfTabla = null; }
    var cuerpo = $('#tblCalificacion tbody').empty();

    $.each(cfPantalla.Oportunidades, function (i, o) {
        var estado = '<span class="cf-estado ' + ClaseEstado(o.Estado) + '">' + Html(o.Estado) + '</span>'
            + (o.SinCalificar ? '<span class="cf-sin">Sin calificar</span>' : '');
        cuerpo.append(
            '<tr class="cf-fila" data-id="' + o.IdForeCast + '">'
            + '<td>' + Html(o.Cliente) + '</td>'
            + '<td>' + Html(o.DetalleProyecto) + '</td>'
            + '<td>' + Html(o.Prioridad) + '</td>'
            + '<td data-order="' + (o.PVPEstimado || 0) + '">' + Dinero(o.PVPEstimado) + '</td>'
            + '<td>' + o.Puntaje + '</td>'
            + '<td>' + estado + '</td>'
            + '<td>' + o.Alertas.length + '</td>'
            + '<td><button type="button" class="btn btn-xs btn-primary" onclick="AbrirModal(' + o.IdForeCast + ')">Calificar</button></td>'
            + '</tr>');
    });

    cfTabla = $('#tblCalificacion').DataTable({
        order: [[4, 'desc']],
        pageLength: 25,
        language: { url: "https://cdn.datatables.net/plug-ins/1.10.15/i18n/Spanish.json" }
    });
}

function AlternarDetalle(tr) {
    var fila = cfTabla.row(tr);
    if (fila.child.isShown()) { fila.child.hide(); return; }
    var o = Buscar(Number($(tr).data('id')));
    if (!o) { return; }

    var alertas = $.map(o.Alertas, function (a) { return '<li>' + Html(a.Texto) + '</li>'; }).join('') || '<li>Ninguna</li>';
    var comprometida = o.Calificacion && o.Calificacion.AccionComprometida
        ? Html(o.Calificacion.AccionComprometida) + (o.FechaAccionTexto ? ' (' + o.FechaAccionTexto + ')' : '')
        : '—';

    fila.child(
        '<table class="table table-condensed cf-detalle"><tr>'
        + '<td><b>Desglose</b><br/>Presupuesto ' + o.Presupuesto + '/20 · Autoridad ' + o.Autoridad + '/20 · Necesidad ' + o.Necesidad + '/20<br/>'
        + 'Tiempo ' + o.Tiempo + '/15 · Actividad ' + o.Actividad + '/15 · Encaje ' + o.Encaje + '/10<br/>'
        + 'Cierre: ' + (o.FechaCierreTexto || '—') + ' · Última actividad: ' + (o.UltimaActividadTexto || '—') + '<br/>'
        + 'Ponderado: ' + Dinero(o.MontoPonderado) + ' (' + Html(o.DescripcionNegocio) + ')</td>'
        + '<td><b>Alertas</b><ul>' + alertas + '</ul></td>'
        + '<td><b>Datos faltantes</b><br/>' + (Html(o.Faltantes.join(', ')) || '—') + '</td>'
        + '<td><b>Acción sugerida</b><br/>' + Html(o.AccionSugerida) + '<br/><b>Acción comprometida</b><br/>' + comprometida + '</td>'
        + '</tr></table>').show();
}

function AbrirModal(id) {
    var o = Buscar(id);
    if (!o) { return; }
    var c = o.Calificacion;
    $('#hidIdForeCast').val(id);
    $('#lblOportunidad').text(o.Cliente + ' – ' + o.DetalleProyecto);
    $('#cboPresupuesto').val(c ? String(c.Presupuesto) : '');
    $('#cboAutoridad').val(c ? String(c.Autoridad) : '');
    $('#cboNecesidad').val(c ? String(c.Necesidad) : '');
    $('#txtDecisor').val(c ? c.DecisorCargo : '');
    $('#chkPropuesta').prop('checked', c ? c.PropuestaEnviada : false);
    $('#txtAccion').val(c ? c.AccionComprometida : '');
    $('#txtFechaAccion').val(c && o.FechaAccionTexto ? o.FechaAccionTexto.split('/').reverse().join('-') : '');
    $('#modalCalificar').modal('show');
}

function Guardar() {
    if ($('#cboPresupuesto').val() === '' || $('#cboAutoridad').val() === '' || $('#cboNecesidad').val() === '') {
        swal("Faltan respuestas", "Responda Presupuesto, Autoridad y Necesidad.", "warning");
        return;
    }
    $('#btnGuardar').prop('disabled', true);
    Llamar("GuardarCalificacionForeCast", {
        session: Sesion(),
        idForeCast: $('#hidIdForeCast').val(),
        presupuesto: $('#cboPresupuesto').val(),
        autoridad: $('#cboAutoridad').val(),
        necesidad: $('#cboNecesidad').val(),
        decisorCargo: $('#txtDecisor').val(),
        propuestaEnviada: $('#chkPropuesta').is(':checked') ? '1' : '0',
        accionComprometida: $('#txtAccion').val(),
        fechaAccion: $('#txtFechaAccion').val() || ''
    }, function (r) {
        $('#btnGuardar').prop('disabled', false);
        if (r && r.estado === "1") {
            $('#modalCalificar').modal('hide');
            swal("Listo", "Calificación guardada.", "success");
            Cargar();   // recalcula en el servidor; el navegador no calcula
        } else {
            swal("No se guardó", (r && r.mensaje) || "Intente de nuevo.", "warning");
        }
    });
}

function Descargar() {
    window.location = "DescargarCalificacionForeCast.ashx?" + $.param(Filtros());
}
