/* ============================================================================
   Pantalla: Cálculo de horas extras 50% y 100%
   Handler : AdministrarHorasExtras.ashx

   El Cod_Usuario no se manda nunca: el handler lo saca de la sesion. Las
   horas que digita la persona si viajan; el dinero lo calcula y lo devuelve
   siempre el servidor.
   ============================================================================ */

var _idPeriodoActual = null;
var _periodoAbierto = false;

var MESES_HE = ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];

$(document).ready(function () {
    var hoy = new Date();
    $("#inAnioAbrir").val(hoy.getFullYear());
    $("#inMesAbrir").val(String(hoy.getMonth() + 1));

    CargarListaPeriodos();
});

/* Confirmacion al salir con cambios sin guardar. El texto que se ve depende
   del navegador -la mayoria ya no muestra el propio-, pero devolver algo
   distinto de null es lo que dispara el aviso nativo. */
window.onbeforeunload = function () {
    if (HayCambiosSinGuardar()) {
        return "Hay cambios sin guardar. Si sale ahora, se perderán.";
    }
};

/* Llama al handler con el formato [{action, parameters}] */
function PostHE(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarHorasExtras.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) {
            if (r.estado === "1") { onSuccess(r); }
            else { MostrarMensaje(r.mensaje, r.tipoMensaje); }
        },
        error: function () {
            MostrarMensaje("No se pudo contactar al servidor. Intente nuevamente.", "danger");
        }
    });
}

/* Guarda una fila y REPINTA con lo que devolvio el servidor.
   El calculo del cliente es solo para que el numero aparezca al instante; el
   que vale es el del servidor. Si difieren, el usuario ve el numero saltar, y
   esa es justamente la idea: una divergencia visible en vez de silenciosa.

   Esta funcion queda disponible tal cual para guardar UNA fila suelta. El
   boton "Guardar" del encabezado, que puede tener que guardar varias filas a
   la vez, NO la reusa en bucle: si lo hiciera, el repintado completo que
   dispara cada llamada borraria de la pantalla -antes de que le tocara su
   turno- las horas que el usuario ya escribio en otra fila todavia no
   guardada. GuardarTodo(), mas abajo, resuelve eso tomando una foto de los
   valores pendientes antes de empezar y repintando una sola vez, al final. */
function GuardarFila(idPeriodo, idEmpleado) {
    var $fila = $('tr[data-empleado="' + idEmpleado + '"]');

    PostHE("GuardarFila", {
        idPeriodo: idPeriodo,
        idEmpleado: idEmpleado,
        horas50: $fila.find(".he-horas50").val(),
        horas100: $fila.find(".he-horas100").val(),
        observacion: $fila.find(".he-observacion").val()
    }, function (r) {
        PintarGrilla(r.resultado);
        MarcarGuardado();
    });
}

/* ------------------------------------------------------------- periodos -- */

function CargarListaPeriodos(idPeriodoASeleccionar) {
    PostHE("ListarPeriodos", {}, function (r) {
        var lista = r.resultado || [];
        PintarListaPeriodos(lista, idPeriodoASeleccionar);

        if (idPeriodoASeleccionar) { return; }

        /* Al entrar, se muestra el periodo Abierto mas reciente. Cerrar
           periodo es fase 3, asi que hoy puede haber mas de uno abierto a la
           vez. Sp_RTA_HeListarPeriodos ordena por Anio DESC, Mes DESC, asi
           que el primero de la lista que este Abierto es el mas reciente. */
        var abierto = null;
        $.each(lista, function (i, p) {
            if (p.EstadoPeriodo === "Abierto") { abierto = p; return false; }
        });

        if (abierto) {
            $("#selPeriodo").val(abierto.IdPeriodo);
            SeleccionarPeriodo();
        }
    });
}

function PintarListaPeriodos(lista, idPeriodoASeleccionar) {
    var $sel = $("#selPeriodo").empty();
    $sel.append('<option value="">Seleccione…</option>');

    $.each(lista, function (i, p) {
        var etiqueta = MESES_HE[p.Mes - 1] + " " + p.Anio + " — " + p.EstadoPeriodo;
        $sel.append($("<option></option>").val(p.IdPeriodo).text(etiqueta));
    });

    if (idPeriodoASeleccionar) { $sel.val(idPeriodoASeleccionar); }
}

function SeleccionarPeriodo() {
    var idPeriodo = $("#selPeriodo").val();
    if (!idPeriodo) { return; }

    if (HayCambiosSinGuardar() &&
        !confirm("Tiene cambios sin guardar. Si continúa, se perderán. ¿Desea continuar?")) {
        $("#selPeriodo").val(_idPeriodoActual || "");
        return;
    }

    PostHE("CargarPeriodo", { idPeriodo: idPeriodo }, function (r) {
        PintarPantalla(r.resultado);
    });
}

function AbrirPeriodoSeleccionado() {
    var anio = parseInt($("#inAnioAbrir").val(), 10);
    var mes = parseInt($("#inMesAbrir").val(), 10);

    if (!anio || !mes) {
        MostrarMensaje("Indique año y mes para abrir el período.", "warning");
        return;
    }

    if (HayCambiosSinGuardar() &&
        !confirm("Tiene cambios sin guardar. Si continúa, se perderán. ¿Desea continuar?")) {
        return;
    }

    /* Abrir un periodo que ya existe es inofensivo -NegHorasExtrasPantalla lo
       trata igual que cargarlo-, asi que este mismo boton sirve tanto para
       crear un mes nuevo como para volver a uno existente. */
    PostHE("AbrirPeriodo", { anio: anio, mes: mes }, function (r) {
        CargarListaPeriodos(r.resultado.Periodo.IdPeriodo);
        PintarPantalla(r.resultado);
    });
}

function PintarPantalla(pantalla) {
    _idPeriodoActual = pantalla.Periodo.IdPeriodo;
    _periodoAbierto = pantalla.Periodo.EstaAbierto;

    ActualizarEstadoPeriodo(pantalla.Periodo);
    ActualizarFiltroEmpresa(pantalla.Filas);
    PintarGrilla(pantalla);
}

function ActualizarEstadoPeriodo(periodo) {
    var $lbl = $("#lblEstadoPeriodo").text(periodo.EstadoPeriodo)
        .removeClass("label-success label-default label-danger");

    if (periodo.EstadoPeriodo === "Abierto") { $lbl.addClass("label-success"); }
    else if (periodo.EstadoPeriodo === "Anulado") { $lbl.addClass("label-danger"); }
    else { $lbl.addClass("label-default"); }

    /* En Cerrado o Anulado el boton de guardado desaparece: no hay nada que
       guardar en un periodo de solo lectura. */
    $("#btnGuardar").toggle(periodo.EstaAbierto);
}

/* ------------------------------------------------------------------ grilla -- */

function PintarGrilla(pantalla) {
    var $cuerpo = $("#cuerpoHE").empty();

    if (!pantalla.Filas || pantalla.Filas.length === 0) {
        $cuerpo.append('<tr><td colspan="15" class="text-center text-muted">' +
                       'Este período no tiene colaboradores.</td></tr>');
    } else {
        $.each(pantalla.Filas, function (i, fila) {
            $cuerpo.append(ConstruirFila(fila));
        });
    }

    LimpiarMarcasSucias();
    PintarTotalesServidor(pantalla);
    AplicarFiltros();
}

/* Construye una fila de la grilla a partir del snapshot que manda el
   servidor. Las columnas siguen el orden del documento funcional §5.3, salvo
   Cédula, Empresa y Salario Base -la Empresa se usa como filtro, no como
   columna, y las otras dos no forman parte del recorte que pidio esta tarea-,
   y se agrega Observación porque GuardarFila la necesita y sin un campo para
   escribirla quedaria un parametro del contrato que nadie llena nunca. */
function ConstruirFila(fila) {
    var aplicaHE = !!fila.AplicaHESnapshot;
    var motivo = fila.MotivoNoAplica || "";
    var horasDeshabilitadas = !_periodoAbierto || !aplicaHE;

    var $fila = $("<tr></tr>")
        .attr("data-empleado", fila.IdEmpleado)
        .attr("data-empresa", fila.EmpresaSnapshot || "")
        .attr("data-buscable", ((fila.NombreSnapshot || "") + " " + (fila.CedulaSnapshot || "") + " " +
                                (fila.CargoSnapshot || "")).toLowerCase())
        .data("valorhora50", fila.ValorHora50)
        .data("valorhora100", fila.ValorHora100);

    /* AplicaHESnapshot falso y AplicaHESnapshot verdadero con motivo NO son el
       mismo caso: el segundo son personas que SI cobran horas extras. La
       columna MotivoNoAplica esta mal nombrada para ese caso -no se renombra
       en esta fase- pero aqui se lee por lo que significa, no por su nombre. */
    if (!aplicaHE) {
        $fila.addClass("he-fila-no-aplica");
        if (motivo !== "") { $fila.attr("title", motivo); }
    } else if (motivo !== "") {
        $fila.addClass("he-fila-revision");
        $fila.attr("title", "Salario en revisión — validar antes de cerrar");
    }

    if (Number(fila.ValorHoraOrdinaria) === 0) { $fila.addClass("he-fila-error"); }
    if (Number(fila.TotalHoras) > 0) { $fila.addClass("he-fila-con-horas"); }

    $fila.append($("<td></td>").text(fila.NombreSnapshot || "–"));
    $fila.append($("<td></td>").text(fila.CargoSnapshot || "–"));
    $fila.append($('<td class="text-center"></td>').text(fila.JornadaHorasDiaSnapshot));

    var $aplica = $("<span></span>")
        .addClass(aplicaHE ? "label label-success" : "label label-default")
        .text(aplicaHE ? "SI" : "NO");
    $fila.append($('<td class="text-center"></td>').append($aplica));

    $fila.append(CeldaDerivada(fila.Divisor));
    $fila.append(CeldaDerivada(FormatoDecimales(fila.ValorHoraOrdinaria, 4)));

    var $horas50 = $('<input type="text" class="form-control input-sm he-horas50" maxlength="6" />')
        .val(FormatoDosDecimales(fila.Horas50))
        .prop("disabled", horasDeshabilitadas);
    $fila.append($("<td></td>").append($horas50));

    $fila.append(CeldaDerivada(FormatoDecimales(fila.ValorHora50, 4)));
    $fila.append($('<td class="text-right he-total50 he-col-derivada" style="display:none"></td>')
        .text(FormatoDosDecimales(fila.Total50)));

    var $horas100 = $('<input type="text" class="form-control input-sm he-horas100" maxlength="6" />')
        .val(FormatoDosDecimales(fila.Horas100))
        .prop("disabled", horasDeshabilitadas);
    $fila.append($("<td></td>").append($horas100));

    $fila.append(CeldaDerivada(FormatoDecimales(fila.ValorHora100, 4)));
    $fila.append($('<td class="text-right he-total100 he-col-derivada" style="display:none"></td>')
        .text(FormatoDosDecimales(fila.Total100)));

    $fila.append($('<td class="text-right he-total-horas"></td>').text(FormatoDosDecimales(fila.TotalHoras)));
    $fila.append($('<td class="text-right he-total-he"></td>').text(FormatoDosDecimales(fila.TotalHE)));

    var $obs = $('<input type="text" class="form-control input-sm he-observacion" maxlength="400" />')
        .val(fila.Observacion || "")
        .prop("disabled", !_periodoAbierto);
    $fila.append($("<td></td>").append($obs));

    return $fila;
}

/* Las seis columnas derivadas comparten esta forma: solo lectura, alineadas a
   la derecha, y ocultas hasta que se pulsa "Mostrar columnas de detalle". */
function CeldaDerivada(valor) {
    return $('<td class="text-right he-col-derivada" style="display:none"></td>').text(valor);
}

var _columnasDerivadasVisibles = false;

function AlternarColumnasDerivadas() {
    _columnasDerivadasVisibles = !_columnasDerivadasVisibles;
    $(".he-col-derivada").toggle(_columnasDerivadasVisibles);
    $("#lblColumnasDerivadas").text(_columnasDerivadasVisibles
        ? "Ocultar columnas de detalle"
        : "Mostrar columnas de detalle");
}

/* --------------------------------------------------------------- filtros -- */

function ActualizarFiltroEmpresa(filas) {
    var actual = $("#selEmpresa").val();
    var vistos = {};
    var empresas = [];

    $.each(filas || [], function (i, f) {
        var e = (f.EmpresaSnapshot || "").trim();
        if (e !== "" && !vistos[e]) { vistos[e] = true; empresas.push(e); }
    });
    empresas.sort();

    var $sel = $("#selEmpresa").empty();
    $sel.append('<option value="">Todas</option>');
    $.each(empresas, function (i, e) {
        $sel.append($("<option></option>").val(e).text(e));
    });

    if (actual && vistos[actual]) { $sel.val(actual); }
}

function AplicarFiltros() {
    var empresa = $("#selEmpresa").val();
    var texto = ($("#txtBuscar").val() || "").trim().toLowerCase();
    var soloConHoras = $("#chkSoloConHoras").is(":checked");

    $("#cuerpoHE tr[data-empleado]").each(function () {
        var $f = $(this);
        var visible = true;

        if (empresa && $f.attr("data-empresa") !== empresa) { visible = false; }

        if (visible && texto !== "" && ($f.attr("data-buscable") || "").indexOf(texto) === -1) {
            visible = false;
        }

        if (visible && soloConHoras && NumeroDe($f.find(".he-total-horas").text()) <= 0) {
            visible = false;
        }

        $f.toggle(visible);
    });
}

/* -------------------------------------------------- captura y recalculo -- */

$(document).on("input", ".he-horas50, .he-horas100, .he-observacion", function () {
    MarcarFilaSucia($(this).closest("tr"));
});

/* Al salir de una celda de horas se valida, se normaliza a dos decimales y se
   recalcula la fila EN EL CLIENTE. No se guarda: eso solo lo hace el boton
   Guardar, a proposito -el diseño descarto el autoguardado-. */
$(document).on("blur", ".he-horas50, .he-horas100", function () {
    var $celda = $(this);
    var validado = ValidarNumero($celda.val());
    $celda.val(FormatoDosDecimales(validado));
    RecalcularFilaLocal($celda.closest("tr"));
});

/* Enter baja a la misma columna de la fila siguiente, como en Excel. */
$(document).on("keydown", ".he-horas50, .he-horas100, .he-observacion", function (e) {
    if (e.which !== 13) { return; }
    e.preventDefault();

    var $actual = $(this);
    var clase = $actual.hasClass("he-horas50") ? "he-horas50"
              : $actual.hasClass("he-horas100") ? "he-horas100"
              : "he-observacion";

    var $filaSiguiente = $actual.closest("tr").nextAll("tr[data-empleado]").first();
    if ($filaSiguiente.length) {
        $filaSiguiente.find("." + clase).focus().select();
    }
});

/* Pegar una columna copiada de Excel sobre Horas 50% / Horas 100%: reparte un
   valor por fila, empezando en la celda donde se pego. Si el portapapeles
   trae un solo valor -sin salto de linea- se deja que el navegador pegue
   normal en esa unica celda. */
$(document).on("paste", ".he-horas50, .he-horas100", function (e) {
    var portapapeles = (e.originalEvent.clipboardData || window.clipboardData);
    if (!portapapeles) { return; }

    var texto = portapapeles.getData("text");
    if (texto.indexOf("\n") === -1 && texto.indexOf("\r") === -1) { return; }

    e.preventDefault();

    var clase = $(this).hasClass("he-horas50") ? "he-horas50" : "he-horas100";
    var $filas = $("#cuerpoHE tr[data-empleado]");
    var indiceInicio = $filas.index($(this).closest("tr"));
    var valores = texto.split(/\r\n|\r|\n/).filter(function (l) { return l !== ""; });
    var invalidos = 0;

    $.each(valores, function (i, valor) {
        var $fila = $filas.eq(indiceInicio + i);
        if ($fila.length === 0) { return false; }

        var $celda = $fila.find("." + clase);
        if ($celda.prop("disabled")) { return; }

        /* Una columna pegada puede traer varias columnas separadas por
           tabulador: solo se usa la primera. */
        var crudo = valor.split("\t")[0];
        var validado = ValidarNumero(crudo, true);
        if (String(crudo).trim() !== "" && validado === 0 && crudo.trim() !== "0") { invalidos++; }

        $celda.val(FormatoDosDecimales(validado));
        MarcarFilaSucia($fila);
        RecalcularFilaLocal($fila);
    });

    if (invalidos > 0) {
        MostrarMensaje(invalidos + " valor(es) pegados no eran números válidos y se dejaron en 0.", "warning");
    }
});

function MarcarFilaSucia($fila) {
    $fila.attr("data-dirty", "1");
    $("#lblGuardado").removeClass("text-muted").addClass("text-warning").text("Cambios sin guardar").show();
}

function LimpiarMarcasSucias() {
    $("#cuerpoHE tr[data-empleado]").removeAttr("data-dirty");
}

function HayCambiosSinGuardar() {
    return $('#cuerpoHE tr[data-dirty="1"]').length > 0;
}

function MarcarGuardado() {
    var ahora = new Date();
    var hh = ("0" + ahora.getHours()).slice(-2);
    var mm = ("0" + ahora.getMinutes()).slice(-2);
    $("#lblGuardado").removeClass("text-warning").addClass("text-muted")
        .text("Guardado " + hh + ":" + mm).show();
}

/* Recalcula una fila con los valores hora que ya trajo el servidor. Es una
   vista previa: el total que manda es el que devuelve GuardarFila. */
function RecalcularFilaLocal($fila) {
    var horas50 = NumeroDe($fila.find(".he-horas50").val());
    var horas100 = NumeroDe($fila.find(".he-horas100").val());
    var aplica = !$fila.hasClass("he-fila-no-aplica");
    var valorHora50 = parseFloat($fila.data("valorhora50")) || 0;
    var valorHora100 = parseFloat($fila.data("valorhora100")) || 0;

    var total50 = aplica ? RedondearDos(horas50 * valorHora50) : 0;
    var total100 = aplica ? RedondearDos(horas100 * valorHora100) : 0;
    var totalHoras = horas50 + horas100;
    var totalHE = total50 + total100;

    $fila.find(".he-total50").text(FormatoDosDecimales(total50));
    $fila.find(".he-total100").text(FormatoDosDecimales(total100));
    $fila.find(".he-total-horas").text(FormatoDosDecimales(totalHoras));
    $fila.find(".he-total-he").text(FormatoDosDecimales(totalHE));

    $fila.toggleClass("he-fila-con-horas", totalHoras > 0);

    RecalcularTotalesLocal();
}

function RecalcularTotalesLocal() {
    var horas50 = 0, horas100 = 0, horas = 0, pago50 = 0, pago100 = 0, pagar = 0;

    $("#cuerpoHE tr[data-empleado]").each(function () {
        var $f = $(this);
        horas50 += NumeroDe($f.find(".he-horas50").val());
        horas100 += NumeroDe($f.find(".he-horas100").val());
        horas += NumeroDe($f.find(".he-total-horas").text());
        pago50 += NumeroDe($f.find(".he-total50").text());
        pago100 += NumeroDe($f.find(".he-total100").text());
        pagar += NumeroDe($f.find(".he-total-he").text());
    });

    ActualizarTotales(horas50, pago50, horas100, pago100, horas, pagar);
}

function PintarTotalesServidor(pantalla) {
    ActualizarTotales(pantalla.TotalHoras50, pantalla.TotalPago50, pantalla.TotalHoras100,
                       pantalla.TotalPago100, pantalla.TotalHoras, pantalla.TotalPagar);
}

function ActualizarTotales(horas50, pago50, horas100, pago100, totalHoras, totalHE) {
    $("#tabHoras50, #pieHoras50").text(FormatoDosDecimales(horas50));
    $("#tabPago50, #piePago50").text("USD " + FormatoDosDecimales(pago50));
    $("#tabHoras100, #pieHoras100").text(FormatoDosDecimales(horas100));
    $("#tabPago100, #piePago100").text("USD " + FormatoDosDecimales(pago100));
    $("#tabTotalHoras, #pieTotalHoras").text(FormatoDosDecimales(totalHoras));
    $("#tabTotalPagar, #pieTotalHE").text("USD " + FormatoDosDecimales(totalHE));
}

/* --------------------------------------------------------- guardar todo -- */

/* El boton "Guardar" del encabezado guarda TODAS las filas con cambios
   pendientes. Toma la foto de sus valores antes de mandar la primera peticion
   y guarda una por una con esos valores fijos -no relee el DOM entre
   peticiones-, asi un repintado a mitad de camino nunca pisa el turno de la
   fila que todavia no le tocaba. Solo se repinta al final, con la respuesta
   de la ULTIMA fila: como GuardarHoras relee la base antes de responder, esa
   respuesta ya incluye todo lo guardado en este mismo lote. */
function GuardarTodo() {
    if (!_idPeriodoActual) { return; }

    var pendientes = [];
    $("#cuerpoHE tr[data-dirty=\"1\"]").each(function () {
        var $f = $(this);
        pendientes.push({
            idEmpleado: $f.attr("data-empleado"),
            horas50: $f.find(".he-horas50").val(),
            horas100: $f.find(".he-horas100").val(),
            observacion: $f.find(".he-observacion").val()
        });
    });

    if (pendientes.length === 0) {
        MostrarMensaje("No hay cambios para guardar.", "info");
        return;
    }

    $("#btnGuardar").prop("disabled", true);
    GuardarUnaAUna(pendientes, 0);
}

function GuardarUnaAUna(lista, indice) {
    if (indice >= lista.length) {
        $("#btnGuardar").prop("disabled", false);
        return;
    }

    var item = lista[indice];
    var esLaUltima = indice === lista.length - 1;

    PostHE("GuardarFila", {
        idPeriodo: _idPeriodoActual,
        idEmpleado: item.idEmpleado,
        horas50: item.horas50,
        horas100: item.horas100,
        observacion: item.observacion
    }, function (r) {
        if (esLaUltima) {
            PintarGrilla(r.resultado);
            MarcarGuardado();
            $("#btnGuardar").prop("disabled", false);
        } else {
            GuardarUnaAUna(lista, indice + 1);
        }
    });
}

/* ----------------------------------------------------------- utilitarios -- */

/* Solo numerico, mayor o igual a cero, maximo 2 decimales y tope de 200 por
   celda. Un dato ilegible vale cero, igual que del lado del servidor: es la
   misma regla en los dos lenguajes. silencioso evita el modal al pegar
   muchas celdas de una vez -se avisa una sola vez, con el conteo, despues-. */
function ValidarNumero(texto, silencioso) {
    var limpio = String(texto == null ? "" : texto).trim().replace(",", ".");
    if (limpio === "") { return 0; }

    if (!/^\d{1,3}(\.\d{1,2})?$/.test(limpio)) {
        if (!silencioso) {
            MostrarMensaje("Solo se aceptan números positivos, con máximo 2 decimales.", "warning");
        }
        return 0;
    }

    var numero = parseFloat(limpio);

    if (numero > 200) {
        if (!silencioso) { MostrarMensaje("El máximo por celda es 200 horas.", "warning"); }
        return 200;
    }

    return numero;
}

function NumeroDe(texto) {
    var n = parseFloat(String(texto == null ? "" : texto).replace(",", "."));
    return isNaN(n) ? 0 : n;
}

function RedondearDos(valor) {
    return Math.round((valor + (valor >= 0 ? 1 : -1) * 1e-9) * 100) / 100;
}

function FormatoDecimales(valor, decimales) {
    return NumeroDe(valor).toFixed(decimales);
}

function FormatoDosDecimales(valor) {
    return FormatoDecimales(valor, 2);
}

/* Copiada tal cual del patron de la casa (miPerfil.js, entre otras). Cada
   pantalla lleva su propia copia; no hay una utilidad compartida. */
function MostrarMensaje(mensaje, tipo) {
    var color = "#fcf8e3";
    if (tipo == "success") { color = "#dff0d8"; }
    if (tipo == "danger") { color = "#f2dede"; }
    if (tipo == "warning") { color = "#fcf8e3"; }

    $("#modalMensajeInformativoTipo").css("background", color);
    $("#MensajeInformativo").html(mensaje);
    $("#modalMensajeInformativo").modal("show");
}
