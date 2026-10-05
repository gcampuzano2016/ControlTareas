/* ==========================================================================
   Curva de Aprendizaje - motor del formulario dinamico (especificacion,
   seccion 10). Lo usan CurvaEvaluacion.aspx (GTH) y CurvaEvaluacionRemota.aspx
   (anonima): el formulario no se disena a mano, se pinta desde las preguntas
   del catalogo.

   CAForm.pintar($contenedor, datos)  pinta seguimiento + preguntas.
   CAForm.recolectar($contenedor)     devuelve el payload como texto JSON:
       { respuestas: { "<idPregunta>": valor }, seguimientos: [ ... ] }

   El servidor (NegCurvaFormulario) vuelve a validar cada valor contra su
   pregunta: lo de aqui es comodidad, no barrera.
   ========================================================================== */
var CAForm = (function () {
    "use strict";
    var esc = CA.esc;
    var RE_OTRO = /^otros?\b/i;
    var ESTADOS_SEG = [["realizado", "Realizado"], ["en_progreso", "En progreso"], ["pendiente", "Pendiente"], ["no_realizado", "No realizado"]];
    var DECISIONES = [["aprobado", "Aprobado — supera el período de prueba"], ["extendido", "Extendido — necesita más tiempo"], ["rechazado", "Rechazado — no continúa"]];
    var categorias = [];
    var unico = 0;

    function esOtro(op) { return RE_OTRO.test($.trim(op || "")); }
    function id() { unico += 1; return "caf" + unico; }

    /* Valor guardado "Otro: texto" -> { opcion: "Otro", texto: "texto" }. */
    function separarOtro(valor, opciones) {
        for (var i = 0; i < opciones.length; i++) {
            if (valor === opciones[i]) { return { opcion: opciones[i], texto: "" }; }
            if (esOtro(opciones[i]) && typeof valor === "string" && valor.indexOf(opciones[i] + ": ") === 0) {
                return { opcion: opciones[i], texto: valor.substr(opciones[i].length + 2) };
            }
        }
        return null;
    }

    function textoDe(v) {
        if (v === null || v === undefined) { return ""; }
        return typeof v === "string" ? v : String(v);
    }

    /* ---------------------------------------------------------- pintar */

    function pintar($c, datos) {
        categorias = datos.Categorias || [];
        var h = [];
        if (datos.Abiertos && datos.Abiertos.length) { h.push(pintarSeguimiento(datos.Abiertos)); }
        var seccion = null;
        $.each(datos.Preguntas || [], function (_, p) {
            if (p.Seccion !== seccion) {
                seccion = p.Seccion;
                if (seccion) { h.push('<div class="ca-seccion-titulo">' + esc(seccion) + "</div>"); }
            }
            h.push(pintarPregunta(p, (datos.Respuestas || {})[p.Id]));
        });
        $c.addClass("ca-formulario").html(h.join(""));
        enlazar($c);
    }

    function pintarSeguimiento(abiertos) {
        var h = ['<div class="ca-panel"><h2>Seguimiento de compromisos anteriores</h2>',
                 '<p class="ca-nota">Revisa el estado de los compromisos acordados en evaluaciones previas de este proceso.</p>'];
        $.each(abiertos, function (_, c) {
            var meta = [];
            if (c.Categoria) { meta.push(esc(c.Categoria)); }
            if (c.Responsable) { meta.push("Responsable: " + esc(c.Responsable)); }
            if (c.EtapaCodigo) { meta.push("Acordado en etapa " + esc(c.EtapaCodigo) + " días"); }
            if (c.FechaLimite) { meta.push("Límite: " + esc(CA.fechaIso(c.FechaLimite))); }
            var nombre = id();
            var estados = $.map(ESTADOS_SEG, function (e) {
                return '<label><input type="radio" name="' + nombre + '" value="' + e[0] + '"' + (c.EstadoActual === e[0] ? " checked" : "") + "> " + e[1] + "</label>";
            }).join("");
            h.push('<div class="ca-seguimiento" data-compromiso="' + c.Id + '">',
                   "<b>" + esc(c.Descripcion) + "</b>",
                   '<div class="ca-meta">' + meta.join(" · ") + "</div>",
                   '<div class="ca-estados">' + estados + "</div>",
                   '<input type="text" class="ca-justificacion" maxlength="1000" placeholder="Justificación (opcional, recomendada si no está realizado)" value="' + esc(c.JustificacionActual) + '">',
                   "</div>");
        });
        h.push("</div>");
        return h.join("");
    }

    function pintarPregunta(p, valor) {
        var cuerpo = "";
        var ops = p.Opciones || [];
        switch (p.Tipo) {
            case "escala":
            case "opcion_unica":
            case "si_no":
                cuerpo = pintarOpciones(ops, "radio", valor === undefined ? [] : [textoDe(valor)]);
                break;
            case "opcion_multiple":
                cuerpo = pintarOpciones(ops, "checkbox", $.isArray(valor) ? $.map(valor, textoDe) : (valor ? [textoDe(valor)] : []));
                break;
            case "texto_abierto":
                cuerpo = '<textarea class="ca-texto" rows="3" maxlength="4000">' + esc(textoDe(valor)) + "</textarea>";
                break;
            case "competencias_multi":
                cuerpo = pintarCompetencias(ops, valor && typeof valor === "object" ? valor : {});
                break;
            case "compromisos":
                cuerpo = '<div class="ca-filas-compromiso"></div>' +
                         '<button type="button" class="ca-btn ca-btn-outline ca-agregar-compromiso">+ Agregar compromiso</button>';
                break;
            case "decision_periodo_prueba":
                cuerpo = pintarDecision(valor && typeof valor === "object" ? valor : {});
                break;
        }
        var html = '<div class="ca-pregunta" data-id="' + p.Id + '" data-tipo="' + esc(p.Tipo) + '">' +
                   '<label class="ca-enunciado">' + esc(p.Texto) + "</label>" + cuerpo + "</div>";
        if (p.Tipo === "compromisos") {
            // las filas se agregan despues de insertar el html (necesitan el DOM)
            pendientesCompromisos.push({ id: p.Id, filas: $.isArray(valor) ? valor : [] });
        }
        return html;
    }

    var pendientesCompromisos = [];

    function pintarOpciones(ops, tipo, marcados) {
        var nombre = id();
        var h = ['<div class="ca-opciones">'];
        $.each(ops, function (_, op) {
            var sel = null;
            $.each(marcados, function (_, m) { var s = separarOtro(m, [op]); if (s) { sel = s; } });
            var otro = esOtro(op);
            h.push('<label><input type="' + tipo + '" name="' + nombre + '" value="' + esc(op) + '"' + (sel ? " checked" : "") +
                   (otro ? ' data-otro="1"' : "") + "> " + esc(op) + "</label>");
            if (otro) {
                h.push('<input type="text" class="ca-otro-texto" maxlength="500" placeholder="Especifica…" value="' + esc(sel ? sel.texto : "") + '"' +
                       (sel ? ' style="display:block"' : "") + ">");
            }
        });
        h.push("</div>");
        return h.join("");
    }

    function pintarCompetencias(ops, valor) {
        var h = [];
        $.each(ops, function (_, comp) {
            var nombre = id();
            var actual = valor[comp];
            var radios = "";
            for (var n = 1; n <= 5; n++) {
                radios += '<label><input type="radio" name="' + nombre + '" value="' + n + '"' + (String(actual) === String(n) ? " checked" : "") + "> " + n + "</label>";
            }
            h.push('<div class="ca-competencia" data-competencia="' + esc(comp) + '"><span>' + esc(comp) + '</span><span class="ca-escala-num">' + radios + "</span></div>");
        });
        return h.join("");
    }

    function pintarDecision(valor) {
        var nombre = id();
        var h = ['<div class="ca-decision">'];
        $.each(DECISIONES, function (_, d) {
            h.push('<label><input type="radio" name="' + nombre + '" value="' + d[0] + '"' + (valor.decision === d[0] ? " checked" : "") + "> " + esc(d[1]) + "</label>");
        });
        // Visible al CARGAR si la decision guardada es "extendido" (seccion 18, punto 1).
        h.push('<div class="ca-nueva-fecha"' + (valor.decision === "extendido" ? ' style="display:block"' : "") + '><label>Nueva fecha límite</label><br>',
               '<input type="date" class="ca-fecha-fin" value="' + esc(valor.nueva_fecha_fin || "") + '"></div>',
               '<label>Comentarios / justificación</label><textarea class="ca-comentarios" rows="3" maxlength="4000">' + esc(valor.comentarios || "") + "</textarea>",
               "</div>");
        return h.join("");
    }

    function filaCompromiso(f) {
        f = f || {};
        var cat = f.categoria || "";
        var enLista = !cat || $.inArray(cat, categorias) >= 0;
        var opciones = '<option value="">Selecciona...</option>' + $.map(categorias, function (c) {
            var sel = (enLista && c === cat) || (!enLista && c === "Otro");
            return '<option value="' + esc(c) + '"' + (sel ? " selected" : "") + ">" + esc(c) + "</option>";
        }).join("");
        var otroVisible = !enLista || cat === "Otro";
        return '<div class="ca-compromiso">' +
            '<div><label>Descripción del compromiso</label><input type="text" class="ca-c-desc" maxlength="1000" placeholder="Ej. Tomar curso de Excel avanzado" value="' + esc(f.descripcion) + '"></div>' +
            '<div><label>Tipo de compromiso <span title="Se usa para agrupar los temas de capacitación más recurrentes en el dashboard de RRHH">ⓘ</span></label>' +
            '<select class="ca-c-cat">' + opciones + "</select>" +
            '<input type="text" class="ca-c-cat-otro" maxlength="200" placeholder="Especifica el tipo de compromiso" style="margin-top:4px;' + (otroVisible ? "" : "display:none") + '" value="' + esc(enLista ? "" : cat) + '"></div>' +
            '<div><label>Responsable</label><input type="text" class="ca-c-resp" maxlength="200" placeholder="Nombre" value="' + esc(f.responsable) + '"></div>' +
            '<div><label>Fecha límite</label><input type="date" class="ca-c-fecha" value="' + esc(f.fecha_limite || "") + '"></div>' +
            '<div><button type="button" class="ca-quitar" title="Quitar">&times;</button></div>' +
            "</div>";
    }

    function enlazar($c) {
        // compromisos guardados (o una fila vacia si no hay)
        $.each(pendientesCompromisos, function (_, pc) {
            var $filas = $c.find('.ca-pregunta[data-id="' + pc.id + '"] .ca-filas-compromiso');
            if (pc.filas.length === 0) { $filas.append(filaCompromiso()); }
            $.each(pc.filas, function (_, f) { $filas.append(filaCompromiso(f)); });
        });
        pendientesCompromisos = [];

        $c.on("change", ".ca-opciones input", function () {
            var $op = $(this).closest(".ca-opciones");
            $op.find("input[data-otro]").each(function () {
                $(this).closest("label").next(".ca-otro-texto").toggle(this.checked);
            });
        });
        $c.on("change", ".ca-decision input[type=radio]", function () {
            $(this).closest(".ca-decision").find(".ca-nueva-fecha").toggle(this.value === "extendido" && this.checked);
        });
        $c.on("click", ".ca-agregar-compromiso", function () {
            $(this).siblings(".ca-filas-compromiso").append(filaCompromiso());
        });
        $c.on("click", ".ca-quitar", function () { $(this).closest(".ca-compromiso").remove(); });
        $c.on("change", ".ca-c-cat", function () {
            $(this).siblings(".ca-c-cat-otro").toggle(this.value === "Otro");
        });
    }

    /* ------------------------------------------------------- recolectar */

    function valorConOtro($input) {
        var op = $input.val();
        if (!$input.data("otro")) { return op; }
        var t = $.trim($input.closest("label").next(".ca-otro-texto").val() || "");
        return t.length ? op + ": " + t : op;
    }

    function recolectar($c) {
        var respuestas = {};
        $c.find(".ca-pregunta").each(function () {
            var $p = $(this), pid = $p.data("id"), tipo = $p.data("tipo");
            switch (tipo) {
                case "escala":
                case "opcion_unica":
                case "si_no":
                    var $r = $p.find(".ca-opciones input:checked").first();
                    if ($r.length) { respuestas[pid] = valorConOtro($r); }
                    break;
                case "opcion_multiple":
                    var lista = $p.find(".ca-opciones input:checked").map(function () { return valorConOtro($(this)); }).get();
                    if (lista.length) { respuestas[pid] = lista; }
                    break;
                case "texto_abierto":
                    var t = $.trim($p.find(".ca-texto").val() || "");
                    if (t.length) { respuestas[pid] = t; }
                    break;
                case "competencias_multi":
                    var comp = {}, alguna = false;
                    $p.find(".ca-competencia").each(function () {
                        var v = $(this).find("input:checked").val();
                        if (v) { comp[$(this).data("competencia")] = parseInt(v, 10); alguna = true; }
                    });
                    if (alguna) { respuestas[pid] = comp; }
                    break;
                case "compromisos":
                    // Siempre se envia (aunque vacia): el servidor reemplaza la lista.
                    respuestas[pid] = $p.find(".ca-compromiso").map(function () {
                        var $f = $(this);
                        return {
                            descripcion: $.trim($f.find(".ca-c-desc").val() || ""),
                            categoria: $f.find(".ca-c-cat").val() || "",
                            categoria_otro: $.trim($f.find(".ca-c-cat-otro").val() || ""),
                            responsable: $.trim($f.find(".ca-c-resp").val() || ""),
                            fecha_limite: $f.find(".ca-c-fecha").val() || ""
                        };
                    }).get();
                    break;
                case "decision_periodo_prueba":
                    var d = $p.find(".ca-decision input[type=radio]:checked").val();
                    if (d) {
                        respuestas[pid] = {
                            decision: d,
                            nueva_fecha_fin: d === "extendido" ? ($p.find(".ca-fecha-fin").val() || "") : "",
                            comentarios: $.trim($p.find(".ca-comentarios").val() || "")
                        };
                    }
                    break;
            }
        });

        var seguimientos = $c.find(".ca-seguimiento").map(function () {
            var estado = $(this).find(".ca-estados input:checked").val();
            if (!estado) { return null; }
            return { compromiso_id: $(this).data("compromiso"), estado: estado, justificacion: $.trim($(this).find(".ca-justificacion").val() || "") };
        }).get();

        return JSON.stringify({ respuestas: respuestas, seguimientos: seguimientos });
    }

    /* Avisos de cliente antes de enviar (el servidor no exige nada). */
    function validar($c) {
        var problemas = [];
        $c.find(".ca-decision").each(function () {
            var d = $(this).find("input[type=radio]:checked").val();
            if (d === "extendido" && !$(this).find(".ca-fecha-fin").val()) {
                problemas.push("Indica la nueva fecha límite del período de prueba extendido.");
            }
        });
        return problemas;
    }

    return { pintar: pintar, recolectar: recolectar, validar: validar };
})();
