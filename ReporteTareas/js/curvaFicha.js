/* Curva de Aprendizaje - ficha del colaborador (CurvaColaboradorFicha.aspx).
   Orden vertical de la especificacion (seccion 9.4): encabezado, banner de
   alerta, progreso y competencias, evaluaciones por etapa (tarjetas),
   reportes en PDF y compromisos acumulados. */
$(function () {
    "use strict";
    var esc = CA.esc;
    var id = parseInt(CA.parametro("id"), 10);
    var ficha = null;
    var graficos = [];
    var ETAPAS = ["30", "60", "90", "180", "365"];

    if (!(id > 0)) { $("#caFicha").html('<div class="ca-vacio">Empleado no encontrado</div>'); return; }
    cargar();

    function cargar() {
        CA.api("ficha", { id: id }, function (f) { ficha = f; pintar(); },
            function (r) { $("#caFicha").html('<div class="ca-vacio">' + esc(r && r.mensaje ? r.mensaje : "No se pudo cargar la ficha.") + "</div>"); }, true);
    }

    function pintar() {
        var e = ficha.Empleado, p = ficha.Proceso;
        var h = [];

        // 1. encabezado
        var linea = [e.Cargo, e.Area, "Ingresó el " + CA.fechaIso(e.FechaIngreso)].filter(function (x) { return !!x; });
        h.push('<div class="ca-encabezado">',
            '<div style="display:flex;justify-content:space-between;gap:10px;flex-wrap:wrap">',
            "<div><h1>" + esc(e.NombreCompleto) + '</h1><p class="ca-sub">' + $.map(linea, esc).join(" · ") + "</p></div>",
            '<div><button type="button" class="ca-enlace-peligro" id="caEliminarColab">Eliminar colaborador</button></div></div>',
            '<div class="ca-acciones">' + CA.badge(e.Estado),
            '<button type="button" class="ca-btn ca-btn-outline-claro" id="caCambiarEstado">' +
                (e.Estado === "activo" ? "Marcar como inactivo (ya no está en la empresa)" : "Marcar como activo") + "</button>",
            (p ? "<span>Proceso: " + CA.badge(p.Estado) + "</span>" : ""),
            "</div></div>");

        // 2. banner de alerta general (null = sin datos, ok = nada que avisar)
        var a = ficha.Alerta;
        if (a && a.Nivel && a.Nivel !== "ok") {
            h.push('<div class="ca-panel ca-banner' + (a.Nivel === "seguimiento" ? " ca-banner-seguimiento" : "") + '">' +
                CA.badge(a.Nivel) + " <span>" + esc(a.Motivo) + "</span></div>");
        }

        // 3. progreso y competencias
        var hayCurva = ficha.Curva.Colaborador.length || ficha.Curva.Jefe.length;
        var c90 = ficha.Competencias["90"], c365 = ficha.Competencias["365"];
        if (hayCurva || c90 || c365) {
            h.push('<div class="ca-panel"><h2>Progreso y competencias</h2>');
            if (hayCurva) {
                h.push("<b>Curva de Aprendizaje</b>",
                    '<p class="ca-nota">Índice de progreso (0-100) por etapa, según autoevaluación del colaborador y evaluación de competencias del jefe.</p>',
                    '<div class="ca-grafico ca-linea"><canvas id="caGrafCurva"></canvas></div>');
                h.push(semaforo());
            }
            if (c90 || c365) {
                h.push('<b style="display:block;margin-top:16px">Competencias evaluadas por el jefe</b><div class="ca-graficos">');
                if (c90) { h.push('<div><p class="ca-nota">90 días</p><div class="ca-grafico"><canvas id="caGraf90"></canvas></div></div>'); }
                if (c365) { h.push('<div><p class="ca-nota">365 días</p><div class="ca-grafico"><canvas id="caGraf365"></canvas></div></div>'); }
                h.push("</div>");
            }
            h.push("</div>");
        }

        // 4. evaluaciones por etapa
        h.push('<div class="ca-panel"><h2>Evaluaciones por etapa</h2>');
        if (!ficha.Evaluaciones.length) { h.push('<div class="ca-vacio">Este colaborador no tiene evaluaciones.</div>'); }
        h.push('<div class="ca-tarjetas">' + $.map(ficha.Evaluaciones, tarjeta).join("") + "</div></div>");

        // 5. reportes (ocultos si el proceso fue rechazado)
        if (p && p.Estado !== "rechazado") {
            var ops = function (sel) {
                return $.map(ETAPAS, function (x) { return '<option value="' + x + '"' + (x === sel ? " selected" : "") + ">" + x + " días</option>"; }).join("");
            };
            h.push('<div class="ca-panel"><h2>Reportes en PDF</h2><div class="ca-reportes">',
                '<a class="ca-btn ca-btn-secundario" target="_blank" href="DescargarCurvaAprendizaje.ashx?tipo=anual&procesoId=' + p.Id + '">Descargar informe anual consolidado</a>',
                '<span><label>Desde</label><select id="caDesde">' + ops("30") + "</select></span>",
                '<span><label>Hasta</label><select id="caHasta">' + ops("60") + "</select></span>",
                '<button type="button" class="ca-btn ca-btn-outline" id="caRango">Descargar informe del tramo</button>',
                "</div></div>");
        }

        // 6. compromisos acumulados
        h.push('<div class="ca-panel"><h2>Compromisos acumulados</h2>');
        if (!ficha.Compromisos.length) { h.push('<div class="ca-vacio">Todavía no se acordaron compromisos.</div>'); }
        else {
            h.push('<table class="ca-tabla"><thead><tr><th>Compromiso</th><th>Categoría</th><th>Responsable</th><th>Fecha límite</th><th>Acordado en</th><th>Estado</th></tr></thead><tbody>');
            $.each(ficha.Compromisos, function (_, c) {
                h.push("<tr><td>" + esc(c.Descripcion) + "</td><td>" + esc(c.Categoria || "—") + "</td><td>" + esc(c.Responsable || "—") +
                    '</td><td class="ca-fecha">' + CA.fechaIso(c.FechaLimite) + "</td><td>Etapa " + esc(c.EtapaCodigo) + " días</td><td>" +
                    CA.badge(c.UltimoEstado || "sin_revisar") + "</td></tr>");
            });
            h.push("</tbody></table>");
        }
        h.push("</div>");

        $.each(graficos, function (_, g) { g.destroy(); });
        graficos = [];
        $("#caFicha").html(h.join(""));
        dibujarGraficos();
    }

    function semaforo() {
        if (!ficha.AlertaPorEtapa.length) { return ""; }
        var celda = function (n) { return n ? CA.badge(n.Nivel) + " " + n.Indice + "/100" : "—"; };
        return '<table class="ca-tabla" style="margin-top:12px"><thead><tr><th>Etapa</th><th>Colaborador</th><th>Jefe</th><th>Semáforo</th></tr></thead><tbody>' +
            $.map(ficha.AlertaPorEtapa, function (x) {
                return "<tr><td>" + esc(x.Etapa) + " días</td><td>" + celda(x.Colaborador) + "</td><td>" + celda(x.Jefe) + "</td><td>" + CA.badge(x.Nivel) + "</td></tr>";
            }).join("") + "</tbody></table>";
    }

    function tarjeta(t) {
        var completada = t.Estado === "completada";
        var h = ['<div class="ca-tarjeta' + (completada ? "" : " ca-pendiente") + '" data-id="' + t.Id + '">',
            '<div class="ca-tarjeta-cab"><div><b>Etapa ' + esc(t.EtapaCodigo) + ' días</b><div class="ca-tarjeta-rol">' + CA.rolTexto(t.Rol) + "</div></div>" + CA.badge(t.Estado) + "</div>",
            "<p>Avance: " + (t.Indice !== null && t.Indice !== undefined ? CA.badge(t.Nivel) + " " + t.Indice + "/100" : '<span class="ca-nota">Sin avance calculado todavía</span>') + "</p>",
            '<p>Fecha objetivo: <span class="ca-fecha">' + CA.fechaIso(t.FechaObjetivo) + "</span></p>"];
        if (completada && t.FechaCompletada) { h.push('<p>Completada el <span class="ca-fecha">' + CA.fechaIso(t.FechaCompletada) + "</span></p>"); }
        if (t.AvisoPlanAccion) { h.push('<div class="ca-aviso">⚠ Requiere plan de acción de GTH — calificación del jefe: ' + t.Indice + "/100</div>"); }

        h.push('<div class="ca-acciones-tarjeta">');
        if (!completada) {
            h.push('<a class="ca-btn ca-btn-primario" href="CurvaEvaluacion.aspx?id=' + t.Id + '">Completar →</a>',
                   '<button type="button" class="ca-btn ca-btn-outline ca-generar-enlace">' + (t.TieneEnlace ? "Generar enlace nuevo" : "Generar enlace remoto") + "</button>");
        } else {
            h.push('<a class="ca-btn ca-btn-secundario" target="_blank" href="DescargarCurvaAprendizaje.ashx?tipo=evaluacion&id=' + t.Id + '">Ver informe (PDF)</a>',
                   '<a class="ca-btn ca-btn-outline" href="CurvaEvaluacion.aspx?id=' + t.Id + '">Editar respuestas</a>');
        }
        h.push('<button type="button" class="ca-btn ca-btn-peligro ca-eliminar-eval">Eliminar</button></div>');

        if (t.MostrarPlanAccion) {
            h.push('<div class="ca-plan"><label>Plan de acción GTH — ¿qué se va a hacer frente a esta evaluación?</label>',
                '<textarea rows="3" class="ca-plan-texto">' + esc(t.PlanAccionGth || "") + "</textarea>",
                '<button type="button" class="ca-btn ca-btn-secundario ca-guardar-plan" style="margin-top:6px">Guardar plan de acción</button></div>');
        }
        if (!completada) {
            h.push('<div class="ca-enlace"><div class="ca-enlace-fila"><input type="text" readonly class="ca-enlace-url">',
                '<button type="button" class="ca-btn ca-btn-outline ca-copiar">Copiar</button></div>',
                '<p class="ca-nota">Solo funciona para quien tenga acceso a la red de DOS donde corre este sistema. Envíalo por el medio que prefieras. Un enlace nuevo invalida el anterior.</p></div>');
        }
        h.push("</div>");
        return h.join("");
    }

    function dibujarGraficos() {
        if (typeof Chart === "undefined") { return; }
        var curva = ficha.Curva;
        if (document.getElementById("caGrafCurva")) {
            var serie = function (puntos) {
                return $.map(ETAPAS, function (et) {
                    var p = $.grep(puntos, function (x) { return x.Etapa === et; })[0];
                    return [p ? p.Indice : null];
                });
            };
            var ds = [];
            if (curva.Colaborador.length) { ds.push({ label: "Colaborador", data: serie(curva.Colaborador), borderColor: "#0f4560", backgroundColor: "#0f4560", spanGaps: true, tension: 0.35 }); }
            if (curva.Jefe.length) { ds.push({ label: "Jefe", data: serie(curva.Jefe), borderColor: "#f7902b", backgroundColor: "#f7902b", spanGaps: true, tension: 0.35 }); }
            graficos.push(new Chart(document.getElementById("caGrafCurva"), {
                type: "line",
                data: { labels: $.map(ETAPAS, function (x) { return x + " días"; }), datasets: ds },
                options: { maintainAspectRatio: false, scales: { y: { min: 0, max: 100 } } }
            }));
        }
        barras("caGraf90", ficha.Competencias["90"], "#0f4560");
        barras("caGraf365", ficha.Competencias["365"], "#f7902b");
    }

    function barras(canvasId, comp, color) {
        var el = document.getElementById(canvasId);
        if (!el || !comp) { return; }
        var nombres = Object.keys(comp);
        graficos.push(new Chart(el, {
            type: "bar",
            data: { labels: nombres, datasets: [{ data: $.map(nombres, function (n) { return comp[n]; }), backgroundColor: color }] },
            options: { indexAxis: "y", maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { x: { min: 0, max: 5 } } }
        }));
    }

    /* ----------------------------------------------------------- acciones */

    $("#caFicha").on("click", "#caCambiarEstado", function () {
        var nuevo = ficha.Empleado.Estado === "activo" ? "inactivo" : "activo";
        CA.api("cambiarEstado", { id: id, estado: nuevo }, cargar);
    });

    $("#caFicha").on("click", "#caEliminarColab", function () {
        var nombre = ficha.Empleado.NombreCompleto;
        if (!window.confirm("¿Eliminar a " + nombre + "? Se borrará todo su historial: evaluaciones, respuestas, compromisos y decisiones.")) { return; }
        if (!window.confirm("Esta acción es PERMANENTE e IRREVERSIBLE. No hay forma de recuperar la información de " + nombre + ". ¿Confirmas que quieres eliminarlo?")) { return; }
        CA.api("eliminarColaborador", { id: id }, function () { window.location.href = "CurvaColaboradores.aspx"; });
    });

    $("#caFicha").on("click", ".ca-eliminar-eval", function () {
        var evalId = $(this).closest(".ca-tarjeta").data("id");
        if (!window.confirm("¿Eliminar esta evaluación? Se borrarán sus respuestas, sus compromisos y su decisión de período de prueba, si la tiene.")) { return; }
        CA.api("eliminarEvaluacion", { id: evalId }, cargar);
    });

    $("#caFicha").on("click", ".ca-guardar-plan", function () {
        var $t = $(this).closest(".ca-tarjeta"), boton = this;
        CA.ocupado(boton, "Guardando…");
        CA.api("guardarPlan", { id: $t.data("id"), plan: $t.find(".ca-plan-texto").val() },
            function () { CA.ocupado(boton); cargar(); }, function () { CA.ocupado(boton); });
    });

    $("#caFicha").on("click", ".ca-generar-enlace", function () {
        var $t = $(this).closest(".ca-tarjeta");
        CA.api("generarEnlace", { id: $t.data("id") }, function (r) {
            $t.find(".ca-enlace-url").val(r.url);
            $t.find(".ca-enlace").show();
            $t.find(".ca-copiar").text("Copiar");
        });
    });

    $("#caFicha").on("click", ".ca-copiar", function () {
        var $b = $(this), input = $b.closest(".ca-enlace").find(".ca-enlace-url")[0];
        var listo = function () { $b.text("¡Copiado!"); };
        if (navigator.clipboard && window.isSecureContext) {
            navigator.clipboard.writeText(input.value).then(listo, function () { input.select(); document.execCommand("copy"); listo(); });
        } else { input.select(); document.execCommand("copy"); listo(); }
    });

    $("#caFicha").on("click", "#caRango", function () {
        var desde = $("#caDesde").val(), hasta = $("#caHasta").val();
        if (ETAPAS.indexOf(desde) > ETAPAS.indexOf(hasta)) {
            CA.mensaje("La etapa «desde» no puede ser posterior a la etapa «hasta».", "warning");
            return;
        }
        window.open("DescargarCurvaAprendizaje.ashx?tipo=rango&procesoId=" + ficha.Proceso.Id + "&desde=" + desde + "&hasta=" + hasta, "_blank");
    });
});
