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

/* Los factores de recargo de HE_Parametro (50% y 100%), tal como los aplico
   el servidor a la pantalla que esta cargada ahora mismo. Se llenan desde
   pantalla.Factor50/Factor100 -que manda el servidor en CargarPeriodo,
   AbrirPeriodo y GuardarFila- en PintarPantalla, y son la fuente de verdad
   para RecalcularFilaLocal. */
var _factor50 = null;
var _factor100 = null;

/* Estos dos son SOLO el respaldo para el primerisimo instante en que la
   pantalla todavia no cargo ningun periodo -antes de la primera respuesta
   del servidor, _factor50/_factor100 son null-. Nunca deberian usarse una
   vez que hay un periodo cargado: si HE_Parametro cambiara estos valores, un
   respaldo desactualizado que se usara por error mostraria un numero
   equivocado en las 64 filas a la vez sin que nada lo avisara. Por eso
   RecalcularFilaLocal cae en ellos solo si _factor50/_factor100 siguen en
   null, nunca los prefiere sobre lo que trajo el servidor. */
var FACTOR_HE_50_RESPALDO = 1.5;
var FACTOR_HE_100_RESPALDO = 2.0;

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

/* Llama al handler con el formato [{action, parameters}].

   onError es opcional: cuando no se da, un estado "0" o un fallo de red solo
   muestran el mensaje y ahi se detiene la cadena -es lo que le paso al boton
   Guardar en la revision: sin una rama de error explicita, la promesa nunca
   seguia y el boton se quedaba deshabilitado para siempre-. Cuando se da, se
   invoca ademas de mostrar el mensaje, para que quien llamo pueda decidir
   como seguir (por ejemplo, continuar con la siguiente fila de un lote).

   suprimirMensaje es tambien opcional: evita el modal automatico, tanto en
   exito con advertencia (estado "1" con mensaje no vacio) como en el propio
   fallo. Sirve para el guardado por lotes: cada fila puede traer su propio
   aviso, o su propio fallo -por ejemplo si el periodo se cierra a mitad de
   camino-, y mostrar un modal por fila seria una cadena de decenas de
   modales encima del resumen final. El lote junta todo y muestra un unico
   resumen al terminar. */
function PostHE(action, parameters, onSuccess, onError, suprimirMensaje) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarHorasExtras.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) {
            if (r.estado === "1") {
                /* GuardarHoras y AbrirPeriodo pueden devolver estado "1" con
                   un mensaje de advertencia (sueldo congelado, filas que no
                   se guardaron al abrir, etc.). Descartarlo porque la
                   operacion en si tuvo exito era el defecto: la advertencia
                   es justo la que Nomina necesita ver. */
                if (!suprimirMensaje && r.mensaje) { MostrarMensaje(r.mensaje, r.tipoMensaje || "warning"); }
                onSuccess(r);
            } else {
                if (!suprimirMensaje) { MostrarMensaje(r.mensaje, r.tipoMensaje); }
                if (onError) { onError(r); }
            }
        },
        error: function () {
            if (!suprimirMensaje) { MostrarMensaje("No se pudo contactar al servidor. Intente nuevamente.", "danger"); }
            if (onError) { onError(null); }
        }
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

    /* Abrir un periodo son unas 66 idas a la base -una por colaborador, mas
       la cabecera- y tarda varios segundos. Sin deshabilitar el boton, un
       doble clic manda dos peticiones concurrentes que chocan entre si contra
       el indice unico de HE_Periodo. Se reactiva en los dos caminos, exito y
       error, para que un fallo no deje el boton inutilizable. */
    $("#btnAbrirPeriodo").prop("disabled", true);

    /* Abrir un periodo que ya existe es inofensivo -NegHorasExtrasPantalla lo
       trata igual que cargarlo-, asi que este mismo boton sirve tanto para
       crear un mes nuevo como para volver a uno existente. */
    PostHE("AbrirPeriodo", { anio: anio, mes: mes }, function (r) {
        CargarListaPeriodos(r.resultado.Periodo.IdPeriodo);
        PintarPantalla(r.resultado);
        $("#btnAbrirPeriodo").prop("disabled", false);
    }, function () {
        $("#btnAbrirPeriodo").prop("disabled", false);
    });
}

function PintarPantalla(pantalla) {
    _idPeriodoActual = pantalla.Periodo.IdPeriodo;
    _periodoAbierto = pantalla.Periodo.EstaAbierto;

    /* Factor50/Factor100 vienen del servidor en cada carga: son los que
       HE_Parametro tenia vigentes cuando esta misma pantalla se calculo alla.
       Se toman de aqui, no de una constante local, para que un cambio en
       HE_Parametro se refleje sin tocar este archivo. */
    _factor50 = Number(pantalla.Factor50);
    _factor100 = Number(pantalla.Factor100);

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

    /* ConstruirFila crea las columnas derivadas siempre ocultas (display:none
       inline). Si el usuario ya las habia mostrado con AlternarColumnasDerivadas
       y despues se repinta la grilla -al guardar, al cambiar de periodo-, el
       thead (que no se reconstruye) queda visible pero las celdas nuevas
       quedan ocultas: la tabla se desalinea, 15 columnas de encabezado contra
       9 de datos. Se reaplica aqui el estado vigente para que ambos coincidan
       siempre, sin depender de cuando se repinte. */
    $(".he-col-derivada").toggle(_columnasDerivadasVisibles);

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
        .data("salario", fila.SalarioBaseSnapshot)
        .data("divisor", fila.Divisor);

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

/* Guarda el valor con el que se entro a la celda, para poder restaurarlo si
   lo que se escribe supera el tope (ver el blur, abajo). */
$(document).on("focus", ".he-horas50, .he-horas100", function () {
    $(this).data("valor-antes", $(this).val());
});

/* Al salir de una celda de horas se valida, se normaliza a dos decimales y se
   recalcula la fila EN EL CLIENTE. No se guarda: eso solo lo hace el boton
   Guardar, a proposito -el diseño descarto el autoguardado-.

   El tope de 200 es un RECHAZO, no una sustitucion: 200 horas es un numero
   que se paga, y convertir "250" en "200.00" en silencio dejaria pagar un
   tope que nadie autorizo a escribir -y el servidor nunca llega a verlo,
   porque la celda ya viajaria con 200-. Se restaura el valor con el que se
   entro a la celda y se avisa, en vez de sustituir. Un dato ilegible (texto,
   sin sentido) si se sigue tratando como cero: no es una cantidad real que
   alguien haya querido pagar, es la ausencia de un dato bueno. */
$(document).on("blur", ".he-horas50, .he-horas100", function () {
    var $celda = $(this);
    var detalle = ValidarNumeroConDetalle($celda.val());

    if (detalle.recortado) {
        $celda.val($celda.data("valor-antes") || FormatoDosDecimales(0));
        MostrarMensaje("El máximo por celda es 200 horas. Se mantuvo el valor anterior.", "warning");
    } else if (detalle.invalido) {
        $celda.val(FormatoDosDecimales(0));
        MostrarMensaje("Solo se aceptan números positivos, con máximo 2 decimales.", "warning");
    } else {
        $celda.val(FormatoDosDecimales(detalle.valor));
    }

    RecalcularFilaLocal($celda.closest("tr"));
});

/* Enter baja a la misma columna de la fila siguiente, como en Excel.
   nextAll se filtra con :visible: sin eso, con un filtro activo (empresa,
   busqueda o "Solo con horas") el foco saltaba a un input oculto -que no es
   un error, es un no-op silencioso: el usuario aprieta Enter y no pasa nada
   visible, porque el input que recibio el foco no se ve en pantalla-. */
$(document).on("keydown", ".he-horas50, .he-horas100, .he-observacion", function (e) {
    if (e.which !== 13) { return; }
    e.preventDefault();

    var $actual = $(this);
    var clase = $actual.hasClass("he-horas50") ? "he-horas50"
              : $actual.hasClass("he-horas100") ? "he-horas100"
              : "he-observacion";

    var $filaSiguiente = $actual.closest("tr").nextAll("tr[data-empleado]:visible").first();
    if ($filaSiguiente.length) {
        $filaSiguiente.find("." + clase).focus().select();
    }
});

/* Pegar una columna copiada de Excel sobre Horas 50% / Horas 100%: reparte un
   valor por fila, empezando en la celda donde se pego. Si el portapapeles
   trae un solo valor -sin salto de linea- se deja que el navegador pegue
   normal en esa unica celda.

   El reparto es por POSICION -$filas.eq(indice)-, y la grilla no tiene una
   columna de cedula con la que verificar la correspondencia: el diseño la
   recorto. Eso no es un detalle menor: la plantilla real de Nomina tiene una
   persona activa menos en la hoja de horas que en la de colaboradores, asi
   que una columna de 61 valores pegada sobre una grilla de 62 filas corre
   cada numero de ahi en adelante a la persona equivocada, y son numeros
   plausibles -nadie los nota, se guardan igual-. Por eso el pegado NO aplica
   nada de una: primero pregunta, con nombres y cantidades, y solo reparte si
   alguien confirma. Ver ConfirmarPegado.

   Dos detalles que no son opcionales:

   1) Solo se descarta el ULTIMO salto de linea -el que Excel agrega despues
      de la ultima celda copiada-, nunca los intermedios. Una columna de 64
      personas esta mayoritariamente vacia: con
      "2\r\n\r\n\r\n4\r\n\r\n6\r\n" (fila 1 = 2, fila 4 = 4, fila 6 = 6),
      quitar TODOS los vacios dejaba ["2","4","6"] y el 4 caia en la fila 2 -
      cada celda vacia del portapapeles tiene que respetar su posicion y
      dejar esa fila sin tocar, no desaparecer del reparto.
   2) Solo se recorren las filas VISIBLES. Las que un filtro oculta -empresa,
      busqueda, o "Solo con horas", que es lo natural antes de repasar- siguen
      en el DOM porque AplicarFiltros usa .toggle(), y sin este filtro el
      pegado tambien les repartia valores sin que nadie lo viera en pantalla. */
$(document).on("paste", ".he-horas50, .he-horas100", function (e) {
    var portapapeles = (e.originalEvent.clipboardData || window.clipboardData);
    if (!portapapeles) { return; }

    var texto = portapapeles.getData("text");
    if (texto.indexOf("\n") === -1 && texto.indexOf("\r") === -1) { return; }

    e.preventDefault();

    var clase = $(this).hasClass("he-horas50") ? "he-horas50" : "he-horas100";
    var $filas = $("#cuerpoHE tr[data-empleado]:visible");
    var indiceInicio = $filas.index($(this).closest("tr"));

    var valores = texto.split(/\r\n|\r|\n/);
    if (valores.length > 0 && valores[valores.length - 1] === "") { valores.pop(); }

    if (valores.length === 0 || indiceInicio < 0) { return; }

    ConfirmarPegado(valores, clase, $filas, indiceInicio);
});

/* El nombre del colaborador es la primera celda de la fila. */
function NombreDeFila($fila) {
    return $fila && $fila.length ? $.trim($fila.find("td").first().text()) : "–";
}

/* Muestra el ancla que le falta al reparto por posicion: cuantos valores
   trae el portapapeles, cuantas filas visibles hay desde donde se pego, y en
   que colaborador empieza y en cual termina. No aplica nada hasta que la
   persona confirma en el modal -nunca con confirm() del navegador, que no
   deja lugar para mostrar esta informacion con formato-. */
function ConfirmarPegado(valores, clase, $filas, indiceInicio) {
    var filasDisponibles = $filas.length - indiceInicio;
    var indiceFin = Math.min(indiceInicio + valores.length, $filas.length) - 1;

    var nombreInicio = NombreDeFila($filas.eq(indiceInicio));
    var nombreFin = NombreDeFila($filas.eq(indiceFin));

    var $cuerpo = $("<div></div>");

    $cuerpo.append(
        $("<p></p>").append(
            "Va a pegar ",
            $("<strong></strong>").text(valores.length),
            " valor(es) sobre ",
            $("<strong></strong>").text(filasDisponibles),
            " fila(s) visibles, empezando en «",
            $("<strong></strong>").text(nombreInicio),
            "» y terminando en «",
            $("<strong></strong>").text(nombreFin),
            "»."
        )
    );

    if (valores.length > filasDisponibles) {
        $cuerpo.append($("<p class='text-warning'></p>").text(
            "Sobran " + (valores.length - filasDisponibles) +
            " valor(es) sin fila visible donde caer: no se van a usar."));
    } else if (valores.length < filasDisponibles) {
        $cuerpo.append($("<p class='text-muted'></p>").text(
            "Quedan filas visibles después de «" + nombreFin + "» que este pegado no toca."));
    }

    $cuerpo.append($("<p></p>").text("¿Los números corresponden a estas personas, en este orden?"));

    MostrarConfirmacion($cuerpo, function () {
        AplicarPegado(valores, clase, $filas, indiceInicio);
    });
}

/* Reparte los valores ya confirmados. El tope de 200 es un rechazo: la celda
   que lo supera se deja tal cual estaba, no se sustituye por 200 -mismo
   criterio que el blur de una celda escrita a mano-, y se informa por
   nombre, no solo por cantidad. */
function AplicarPegado(valores, clase, $filas, indiceInicio) {
    var invalidos = 0;
    var filasRecortadas = [];

    $.each(valores, function (i, valor) {
        var $fila = $filas.eq(indiceInicio + i);
        if ($fila.length === 0) { return false; }

        /* Una columna pegada puede traer varias columnas separadas por
           tabulador: solo se usa la primera. */
        var crudo = valor.split("\t")[0];

        /* Celda vacia en el portapapeles = esa fila no se toca. Distinto de
           "0": alguien pudo copiar una columna a medio llenar a proposito. */
        if (crudo.trim() === "") { return; }

        var $celda = $fila.find("." + clase);
        if ($celda.prop("disabled")) { return; }

        var detalle = ValidarNumeroConDetalle(crudo);

        if (detalle.recortado) {
            filasRecortadas.push(NombreDeFila($fila));
            return;
        }

        if (detalle.invalido) { invalidos++; }

        $celda.val(FormatoDosDecimales(detalle.valor));
        MarcarFilaSucia($fila);
        RecalcularFilaLocal($fila);
    });

    var avisos = [];
    if (invalidos > 0) { avisos.push(invalidos + " valor(es) no eran números válidos y se dejaron en 0"); }
    if (filasRecortadas.length > 0) {
        avisos.push("Superaban 200 horas y no se modificaron: " + filasRecortadas.join(", "));
    }
    if (avisos.length > 0) { MostrarMensaje(avisos.join(". ") + ".", "warning"); }
}

/* El modal de confirmacion de la casa: nunca confirm() ni alert() del
   navegador, que no permiten mostrar nombres en negrita ni una lista.
   .off().on() antes de asignar el manejador: sin eso, cada pegado sumaria un
   manejador mas al mismo boton y una confirmacion vieja se dispararia junto
   con la nueva. */
function MostrarConfirmacion(contenido, alConfirmar) {
    var $cuerpo = $("#textoConfirmarPegado").empty();

    if (typeof contenido === "string") { $cuerpo.text(contenido); }
    else { $cuerpo.append(contenido); }

    $("#btnConfirmarPegado").off("click").on("click", function () {
        $("#modalConfirmarPegado").modal("hide");
        alConfirmar();
    });

    $("#modalConfirmarPegado").modal("show");
}

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

/* Recalcula una fila EN EL CLIENTE, reproduciendo la misma cadena que
   NegHorasExtras.Calcular: hora ordinaria = salario / divisor, sin redondear;
   horas x hora-ordinaria x factor, y recien ahi se redondea, una sola vez.
   Multiplicar por ValorHora50/ValorHora100 -que el servidor ya redondeo a 6
   decimales para mostrarlos en la grilla- arrastraba un redondeo intermedio
   que el servidor nunca hace, y el total del cliente quedaba un centavo por
   debajo del real en como 1 de cada 100 combinaciones. Es una vista previa
   igual: el total que manda es el que devuelve el servidor al guardar.

   Los factores salen de _factor50/_factor100 -lo que trajo el servidor con
   esta misma pantalla-, no de una constante local: los de respaldo solo se
   usan si por algun motivo no llegaron (payload viejo o incompleto), y aun
   asi es preferible mostrar el numero de respaldo a poner el total en cero
   delante de Nomina. */
function RecalcularFilaLocal($fila) {
    var horas50 = NumeroDe($fila.find(".he-horas50").val());
    var horas100 = NumeroDe($fila.find(".he-horas100").val());
    var aplica = !$fila.hasClass("he-fila-no-aplica");

    var salario = parseFloat($fila.data("salario")) || 0;
    var divisor = parseFloat($fila.data("divisor")) || 0;
    var horaOrdinaria = divisor > 0 ? (salario / divisor) : 0;

    var factor50 = (_factor50 !== null && !isNaN(_factor50)) ? _factor50 : FACTOR_HE_50_RESPALDO;
    var factor100 = (_factor100 !== null && !isNaN(_factor100)) ? _factor100 : FACTOR_HE_100_RESPALDO;

    var total50 = aplica ? RedondearDos(horas50 * horaOrdinaria * factor50) : 0;
    var total100 = aplica ? RedondearDos(horas100 * horaOrdinaria * factor100) : 0;
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
   fila que todavia no le tocaba.

   El lote SIEMPRE termina con un CargarPeriodo, haya fallado alguna fila o
   ninguna: es la unica fuente de verdad que no depende de cual peticion
   respondio ultimo. Antes se repintaba solo con la respuesta de la fila
   final, lo que funcionaba mientras todas tuvieran exito, pero dejaba el
   boton Guardar deshabilitado para siempre si CUALQUIER fila fallaba a mitad
   de camino -PostHE no seguia la cadena en el camino de error-, y la unica
   salida era recargar la pagina y perder todo lo no guardado. */
function GuardarTodo() {
    if (!_idPeriodoActual) { return; }

    var pendientes = [];
    $("#cuerpoHE tr[data-dirty=\"1\"]").each(function () {
        var $f = $(this);
        pendientes.push({
            idEmpleado: $f.attr("data-empleado"),
            /* Primera celda de la fila: el nombre del colaborador. Solo se
               usa para nombrar la fila en el aviso si algo falla. */
            nombre: $.trim($f.find("td").first().text()),
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
    GuardarUnaAUna(pendientes, 0, [], []);
}

function GuardarUnaAUna(lista, indice, fallidas, avisos) {
    if (indice >= lista.length) {
        TerminarLoteDeGuardado(fallidas, avisos);
        return;
    }

    /* Un lote son hasta unas 250 idas a la base -cuatro por fila: leer,
       revalidar, guardar, releer- y puede tardar 20-30 segundos con el boton
       deshabilitado. Sin un indicador de avance, la pantalla se ve congelada
       y no hay forma de distinguir "esta trabajando" de "se colgo". */
    $("#lblGuardado").removeClass("text-warning text-muted").addClass("text-info")
        .text("Guardando " + (indice + 1) + " de " + lista.length + "…").show();

    var item = lista[indice];

    PostHE("GuardarFila", {
        idPeriodo: _idPeriodoActual,
        idEmpleado: item.idEmpleado,
        horas50: item.horas50,
        horas100: item.horas100,
        observacion: item.observacion
    }, function (r) {
        if (r.mensaje) { avisos.push((item.nombre || ("empleado " + item.idEmpleado)) + ": " + r.mensaje); }
        GuardarUnaAUna(lista, indice + 1, fallidas, avisos);
    }, function () {
        fallidas.push(item.nombre || ("empleado " + item.idEmpleado));
        GuardarUnaAUna(lista, indice + 1, fallidas, avisos);
    }, true /* suprimirMensaje: cada fila fallida NO abre su propio modal -si el periodo se cierra a mitad del lote, serian decenas encadenados-; el resumen final es el unico aviso */);
}

/* Cierra el lote SIEMPRE con una relectura del periodo desde el servidor -asi
   la pantalla refleja lo que de verdad quedo guardado, haya fallado una fila
   o ninguna- y reactiva el boton Guardar en todos los casos: es la correccion
   central de este punto, que antes solo pasaba por el camino feliz. */
function TerminarLoteDeGuardado(fallidas, avisos) {
    PostHE("CargarPeriodo", { idPeriodo: _idPeriodoActual }, function (r) {
        PintarPantalla(r.resultado);
        $("#btnGuardar").prop("disabled", false);

        /* El indicador "Guardado hh:mm" se actualiza siempre que el lote
           termino sin filas fallidas, avisos aparte: un aviso -sueldo
           congelado, por ejemplo- no significa que la fila no se guardo. */
        if (fallidas.length === 0) { MarcarGuardado(); }

        if (fallidas.length > 0) {
            MostrarMensaje("No se pudieron guardar estas filas, vuelva a intentarlo: " +
                           fallidas.join(", ") + ".", "danger");
        } else if (avisos.length > 0) {
            MostrarMensaje(avisos.join(" | "), "warning");
        }
    }, function () {
        /* Ni siquiera la relectura final respondio: se reactiva el boton
           igual -la alternativa es dejarlo deshabilitado para siempre- y se
           deja la grilla tal cual esta, con sus marcas de "sucia" intactas,
           para que el usuario pueda reintentar sin perder lo que escribio. */
        $("#btnGuardar").prop("disabled", false);
    });
}

/* ----------------------------------------------------------- utilitarios -- */

/* Solo numerico, mayor o igual a cero, maximo 2 decimales y tope de 200 por
   celda. "Invalido" (texto sin sentido) y "recortado" (numero valido pero
   por encima de 200) NO se resuelven igual: un dato ilegible vale cero -no
   es una cantidad real, es la ausencia de un dato bueno-, pero un numero por
   encima del tope es un rechazo, no una sustitucion -200 horas se pagan, y
   convertir en silencio un "250" en "200.00" dejaria pagar un tope que nadie
   escribio-. Por eso esta funcion devuelve los dos casos por separado y deja
   que cada quien la llama decida que hacer: el blur de una celda restaura el
   valor anterior si esta recortado; el pegado deja esa fila sin tocar. */
function ValidarNumeroConDetalle(texto) {
    var limpio = String(texto == null ? "" : texto).trim().replace(",", ".");
    if (limpio === "") { return { valor: 0, invalido: false, recortado: false }; }

    if (!/^\d{1,3}(\.\d{1,2})?$/.test(limpio)) {
        return { valor: 0, invalido: true, recortado: false };
    }

    var numero = parseFloat(limpio);

    if (numero > 200) {
        return { valor: 200, invalido: false, recortado: true };
    }

    return { valor: numero, invalido: false, recortado: false };
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

/* Copiada del patron de la casa (miPerfil.js, entre otras), con una
   diferencia: usa .text() y no .html(). Hoy todo lo que llega aqui es texto
   plano -mensajes literales del servidor, o nombres que ya pasaron por
   .text() al leerse de la grilla-, pero .html() es la puerta por la que
   entraria una inyeccion el dia que un mensaje incluyera, por ejemplo, una
   observacion escrita por un colaborador. Cada pantalla lleva su propia
   copia; no hay una utilidad compartida. */
function MostrarMensaje(mensaje, tipo) {
    var color = "#fcf8e3";
    if (tipo == "success") { color = "#dff0d8"; }
    if (tipo == "danger") { color = "#f2dede"; }
    if (tipo == "warning") { color = "#fcf8e3"; }

    $("#modalMensajeInformativoTipo").css("background", color);
    $("#MensajeInformativo").text(mensaje);
    $("#modalMensajeInformativo").modal("show");
}
