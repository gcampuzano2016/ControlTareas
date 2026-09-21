/* Dashboard de aprobacion de tareas.
   ---------------------------------------------------------------------------
   Vive aparte de aprobacionTareasJefatura.js a proposito: aquel tiene 750 lineas
   y esta pantalla ya hace tres cosas distintas. Lo unico que comparten son los
   filtros de arriba, que se leen del DOM.

   Los cuatro graficos se guardan en _graficos para poder destruirlos antes de
   volver a dibujar: Chart.js NO reemplaza un grafico sobre un canvas ocupado,
   lo superpone, y al segundo "Consultar" quedan dos leyendas encimadas y el
   tooltip mostrando datos de la consulta anterior. */
var _graficos = {};

/* Paleta fija y no aleatoria: aprobado verde y pendiente amarillo, los mismos
   colores que la tabla usa para CumpleJornada. Que el mismo concepto cambie de
   color entre dos partes de la pantalla hace dudar de las dos. */
var DASH_VERDE = "#5cb85c";
var DASH_AMARILLO = "#f0ad4e";
var DASH_GRIS = "#999999";
var DASH_ROJO = "#d9534f";

var DASH_PALETA = ["#5cb85c", "#5bc0de", "#f0ad4e", "#d9534f", "#337ab7",
                   "#8e6cae", "#61b7a0", "#c9a227", "#7f8c8d", "#e07b39",
                   "#bdc3c7"];

function CargarDashboardAprobacion() {
    DashMensaje("");

    /* Si el archivo de Chart.js no se publico, decirlo aca. Sin esto el error
       sale como "Chart is not defined" en la consola del navegador, donde nadie
       mira, y la pantalla queda con seis tarjetas vacias sin explicacion. */
    if (typeof Chart === "undefined") {
        DashMensaje("No se pudo cargar la librería de gráficos. Avise a Sistemas: falta publicar js/chart.umd.js.");
        return;
    }

    var datos = "[{ \"action\": \"DashboardAprobacion\", \"parameters\" : { " +
                "\"usuario\" : \"" + $("#cmbUsuarios").val() + "\", " +
                "fechaDesde: \"" + $("#txtFechaDesde").val() + "\", " +
                "fechaHasta: \"" + $("#txtFechaHasta").val() + "\", " +
                "session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";

    $.ajax({
        type: "POST",
        url: "ObtenerListaTareas.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) {
            /* Un objeto con "estado" es la respuesta de error del handler, no el
               dashboard. Se distingue asi en todo el modulo. */
            if (r != null && typeof r.estado != "undefined") {
                DashMensaje(r.mensaje);
                return;
            }
            PintarDashboard(r);
        },
        error: function () {
            DashMensaje("No se pudo obtener el dashboard. Intente nuevamente.");
        }
    });
}

function DashMensaje(texto) {
    var $m = $("#dashMensaje");
    if (!texto) { $m.hide().text(""); return; }
    $m.text(texto).show();
}

/* Minutos a horas con un decimal. Es la MISMA regla que
   NegDashboardAprobacion.HorasDecimales; se repite aca solo para las etiquetas
   que se arman en el navegador, y por eso redondea igual. */
function DashHoras(minutos) {
    if (!minutos || minutos < 0) { return 0; }
    return Math.round((minutos / 60) * 10) / 10;
}

function PintarDashboard(d) {
    var t = d.Totales || {};

    $("#dashHorasAprobadas").text(DashHoras(t.MinutosAprobados) + " h");
    $("#dashHorasPendientes").text(DashHoras(t.MinutosPendientes) + " h");
    $("#dashHorasOtros").text(DashHoras(t.MinutosOtros) + " h");
    $("#dashPersonasDia").text((t.PersonasDiaAprob || 0) + (t.PersonasDiaPend || 0));

    var dem = d.Demora || {};
    $("#dashDemoraPromedio").text(DashTextoDemora(dem.DiasPromedio));
    $("#dashMasViejo").text(DashTextoDemora(dem.DiasMasViejoPendiente));

    PintarEvolucion(d.Semanas || []);
    PintarPorPersona(d.Responsables || []);
    PintarDemora(dem);
    PintarPorEmpresa(d.Empresas || []);

    CargarNoEjecutadas();
}

/* El conteo de tareas no ejecutadas sale de la accion que YA existe, la misma que
   usa la vista de Pagina2, y se cuentan las filas en el navegador.

   Pedir la lista entera para contarla es un desperdicio, y se elige igual: es la
   unica forma de garantizar que la tarjeta y la vista del detalle digan el mismo
   numero. Un conteo calculado aparte, con su propia consulta, es exactamente como
   dos partes de la misma pantalla terminan contradiciendose. */
function CargarNoEjecutadas() {
    var datos = "[{ \"action\": \"ListaConsultaTareasGeneradasPorRevisar\", \"parameters\" : { " +
                "\"usuario\" : \"" + $("#cmbUsuarios").val() + "\", " +
                "registro : \"0\", " +
                "fechaDesde: \"" + $("#txtFechaDesde").val() + "\", " +
                "fechaHasta: \"" + $("#txtFechaHasta").val() + "\", " +
                "estado: \"0\", busqueda: \"\", " +
                "session: \"" + $("#ContentPlaceHolder1_txtUsuario").val() + "\"} }]";

    $.ajax({
        type: "POST", url: "ObtenerListaTareas.ashx", data: datos,
        contentType: "application/json; charset=utf-8", dataType: "json",
        success: function (r) {
            if (r != null && typeof r.estado != "undefined") {
                /* No se pisa el mensaje del dashboard con este error: el resto del
                   tablero es valido. La tarjeta dice que no se pudo y ya. */
                $("#dashNoEjecutadas").text("?");
                return;
            }
            $("#dashNoEjecutadas").text($.isArray(r) ? r.length : 0);
        },
        error: function () { $("#dashNoEjecutadas").text("?"); }
    });
}

/* Lleva a la vista que ya existe en vez de duplicar el detalle aca. Mueve el combo
   para que la pantalla quede coherente con lo que se esta mostrando. */
function IrANoEjecutadas() {
    $("#cmbEstados").val("3");
    BuscarEstado();
    BtnConsulta();
}

/* Misma regla que NegDashboardAprobacion.TextoDemora. */
function DashTextoDemora(dias) {
    if (!dias || dias <= 0) { return "hoy"; }
    if (dias === 1) { return "1 día"; }
    return dias + " días";
}

/* Destruye el grafico anterior de ese canvas, si lo hay, y dice "sin datos"
   cuando la serie viene vacia. Un canvas en blanco y un canvas que no se dibujo
   se ven igual. */
function DashPreparar(idCanvas, hayDatos) {
    if (_graficos[idCanvas]) {
        _graficos[idCanvas].destroy();
        delete _graficos[idCanvas];
    }

    var canvas = document.getElementById(idCanvas);
    var ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    if (!hayDatos) {
        ctx.font = "14px sans-serif";
        ctx.fillStyle = "#777";
        ctx.fillText("Sin datos en el rango", 10, 24);
        return null;
    }

    return ctx;
}

function PintarEvolucion(semanas) {
    var ctx = DashPreparar("graficoEvolucion", semanas.length > 0);
    if (!ctx) { return; }

    var etiquetas = [];
    var aprobadas = [];
    var pendientes = [];

    $.each(semanas, function (i, s) {
        /* La fecha llega como "/Date(...)/" o ISO segun el serializador; se
           parte la cadena ISO cuando se puede, y si no se cae a Date. Mostrar
           dd/MM alcanza: el anio ya esta en el filtro de arriba. */
        etiquetas.push(DashFechaCorta(s.Semana));
        aprobadas.push(DashHoras(s.MinutosAprobados));
        pendientes.push(DashHoras(s.MinutosPendientes));
    });

    _graficos["graficoEvolucion"] = new Chart(ctx, {
        type: "line",
        data: {
            labels: etiquetas,
            datasets: [
                { label: "Aprobadas", data: aprobadas, borderColor: DASH_VERDE, backgroundColor: DASH_VERDE, tension: 0.2 },
                { label: "Pendientes", data: pendientes, borderColor: DASH_AMARILLO, backgroundColor: DASH_AMARILLO, tension: 0.2 }
            ]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            scales: { y: { beginAtZero: true, title: { display: true, text: "Horas" } } }
        }
    });
}

function DashFechaCorta(valor) {
    if (!valor) { return ""; }

    var m = /\/Date\((-?\d+)\)\//.exec(valor);
    var f = m ? new Date(parseInt(m[1], 10)) : new Date(valor);
    if (isNaN(f.getTime())) { return String(valor); }

    var dd = ("0" + f.getDate()).slice(-2);
    var mm = ("0" + (f.getMonth() + 1)).slice(-2);
    return dd + "/" + mm;
}

function PintarPorPersona(responsables) {
    var ctx = DashPreparar("graficoPorPersona", responsables.length > 0);
    if (!ctx) { return; }

    var nombres = [];
    var aprobadas = [];
    var pendientes = [];

    $.each(responsables, function (i, r) {
        /* El nombre entra como dato de Chart.js, que lo dibuja en un canvas: no
           hay HTML donde inyectar nada. */
        nombres.push(r.Nombre);
        aprobadas.push(DashHoras(r.MinutosAprobados));
        pendientes.push(DashHoras(r.MinutosPendientes));
    });

    _graficos["graficoPorPersona"] = new Chart(ctx, {
        type: "bar",
        data: {
            labels: nombres,
            datasets: [
                { label: "Aprobadas", data: aprobadas, backgroundColor: DASH_VERDE },
                { label: "Pendientes", data: pendientes, backgroundColor: DASH_AMARILLO }
            ]
        },
        options: {
            indexAxis: "y",
            responsive: true, maintainAspectRatio: false,
            scales: { x: { stacked: true, beginAtZero: true, title: { display: true, text: "Horas" } },
                      y: { stacked: true } }
        }
    });
}

function PintarDemora(dem) {
    var hayDatos = (dem && (dem.AprobadasConFecha || 0) > 0);
    var ctx = DashPreparar("graficoDemora", hayDatos);
    if (!ctx) { return; }

    _graficos["graficoDemora"] = new Chart(ctx, {
        type: "bar",
        data: {
            labels: ["Promedio", "Máximo", "Más viejo pendiente"],
            datasets: [{
                label: "Días",
                data: [dem.DiasPromedio || 0, dem.DiasMaximo || 0, dem.DiasMasViejoPendiente || 0],
                backgroundColor: [DASH_GRIS, DASH_AMARILLO, DASH_ROJO]
            }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true, title: { display: true, text: "Días" } } }
        }
    });

    /* Si hay aprobadas sin fecha, el promedio se calcula sobre menos filas de las
       que el usuario cree. Se dice, en vez de dejar un numero que parece completo. */
    if ((dem.AprobadasSinFecha || 0) > 0) {
        DashMensaje("Atención: " + dem.AprobadasSinFecha +
                    " tarea(s) aprobadas no tienen fecha de aprobación y quedan fuera del cálculo de demora.");
    }
}

function PintarPorEmpresa(empresas) {
    var ctx = DashPreparar("graficoPorEmpresa", empresas.length > 0);
    if (!ctx) { return; }

    var nombres = [];
    var horas = [];

    $.each(empresas, function (i, e) {
        nombres.push(e.Empresa);
        horas.push(DashHoras(e.Minutos));
    });

    _graficos["graficoPorEmpresa"] = new Chart(ctx, {
        type: "doughnut",
        data: {
            labels: nombres,
            datasets: [{ data: horas, backgroundColor: DASH_PALETA }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: { legend: { position: "right", labels: { boxWidth: 12, font: { size: 10 } } } }
        }
    });
}
