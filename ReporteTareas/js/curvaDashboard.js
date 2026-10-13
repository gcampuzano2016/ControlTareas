/* Curva de Aprendizaje - dashboard (CurvaDashboard.aspx). Los agregados los
   calcula el servidor (NegCurvaDashboard); aqui solo se pintan. Un porcentaje
   null se muestra como "—", nunca "0%". */
$(function () {
    "use strict";
    var esc = CA.esc;

    CA.api("dashboard", {}, function (d) {
        $("#kpiColab").text(d.TotalColaboradores);
        $("#kpiAprob").text(d.AprobacionPct === null ? "—%" : d.AprobacionPct + "%");
        $("#kpiComp").text(d.CumplimientoPct === null ? "—%" : d.CumplimientoPct + "%");
        $("#kpiCurso").text(d.ProcesosEnCurso);

        // casos con alerta
        if (!d.Casos.length) { $("#caCasos").html('<div class="ca-vacio">Sin casos con alerta en este momento.</div>'); }
        else {
            $("#caCasos").html('<table class="ca-tabla"><thead><tr><th>Colaborador</th><th>Área</th><th>Alerta</th><th>Motivo</th></tr></thead><tbody>' +
                $.map(d.Casos, function (c) {
                    return '<tr class="ca-clic" data-url="CurvaColaboradorFicha.aspx?id=' + c.EmpleadoId + '"><td><b>' + esc(c.NombreCompleto) + "</b></td><td>" +
                        esc(c.Area || "—") + "</td><td>" + CA.badge(c.Nivel) + "</td><td>" + esc(c.Motivo) + "</td></tr>";
                }).join("") + "</tbody></table>");
        }

        // temas
        $("#caTemas").html(d.TemasRecurrentes.length
            ? '<table class="ca-tabla"><thead><tr><th>Categoría</th><th>Compromisos</th></tr></thead><tbody>' +
              $.map(d.TemasRecurrentes, function (t) { return "<tr><td>" + esc(t.Clave) + "</td><td>" + t.N + "</td></tr>"; }).join("") + "</tbody></table>"
            : '<div class="ca-vacio">Todavía no hay compromisos registrados.</div>');

        // proximas
        $("#caProximas").html(d.Proximas.length
            ? '<table class="ca-tabla"><thead><tr><th>Colaborador</th><th>Área</th><th>Etapa</th><th>Rol</th><th>Fecha objetivo</th></tr></thead><tbody>' +
              $.map(d.Proximas, function (p) {
                  var fecha = CA.fechaIso(p.FechaObjetivo);
                  return '<tr class="ca-clic" data-url="CurvaColaboradorFicha.aspx?id=' + p.EmpleadoId + '"><td>' + esc(p.NombreCompleto) + "</td><td>" + esc(p.Area || "—") +
                      "</td><td>" + esc(p.EtapaCodigo) + " días</td><td>" + CA.rolTexto(p.Rol) + '</td><td class="ca-fecha' + (p.Vencida ? " ca-vencida" : "") + '">' +
                      fecha + (p.Vencida ? " (vencida)" : "") + "</td></tr>";
              }).join("") + "</tbody></table>"
            : '<div class="ca-vacio">No hay evaluaciones pendientes.</div>');

        graficos(d);
    });

    $(document).on("click", "tr.ca-clic", function () { window.location.href = $(this).data("url"); });

    function graficos(d) {
        if (typeof Chart === "undefined") { return; }
        var dec = d.DecisionesPorTipo;
        if (d.DecisionesTotal > 0) {
            new Chart(document.getElementById("grafDecisiones"), {
                type: "doughnut",
                data: { labels: ["Aprobado", "Rechazado", "Extendido"], datasets: [{ data: [dec.aprobado, dec.rechazado, dec.extendido], backgroundColor: ["#026666", "#8f181b", "#6d33a0"] }] },
                options: { maintainAspectRatio: false, plugins: { legend: { position: "bottom" } } }
            });
        } else { $("#grafDecisiones").parent().hide(); $("#sinDecisiones").show(); }

        var c = d.CompromisosPorEstado;
        new Chart(document.getElementById("grafCompromisos"), {
            type: "bar",
            data: {
                labels: ["Realizado", "En progreso", "Pendiente", "No realizado", "Sin revisar"],
                datasets: [{ data: [c.realizado, c.en_progreso, c.pendiente, c.no_realizado, c.sin_revisar], backgroundColor: ["#026666", "#f9d710", "#77787b", "#8f181b", "#e5e3e2"] }]
            },
            options: { maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
        });

        if (d.EvolucionCompetencias.length) {
            new Chart(document.getElementById("grafCompetencias"), {
                type: "line",
                data: {
                    labels: $.map(d.EvolucionCompetencias, function (x) { return x.Etapa + " días (" + x.Evaluaciones + ")"; }),
                    datasets: [{ label: "Promedio", data: $.map(d.EvolucionCompetencias, function (x) { return x.Promedio; }), borderColor: "#e20000", backgroundColor: "#e20000", tension: 0.35 }]
                },
                options: { maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { min: 0, max: 5 } } }
            });
        } else { $("#grafCompetencias").parent().hide(); $("#sinCompetencias").show(); }
    }
});
