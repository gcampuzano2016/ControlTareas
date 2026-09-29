/* ============================================================================
   Pantalla: Cálculo de horas extras 50% y 100%
   Handler : AdministrarHorasExtras.ashx

   El Cod_Usuario no se manda nunca: el handler lo saca de la sesion. Las
   horas ya no se digitan: salen de los tramos que Nomina incluye en la ventana
   de detalle (GuardarDecisiones). El dinero lo calcula y lo devuelve siempre
   el servidor.
   ============================================================================ */

var _idPeriodoActual = null;
var _periodoAbierto = false;

/* El rango del periodo que esta cargado, en "yyyy-MM-dd". Se guarda para poder
   recalcular sin que nadie vuelva a teclear las fechas: reenviar un rango
   distinto -aunque sea por un dia- no recalcularia nada, lo rechazaria por
   solapamiento con este mismo periodo. */
var _fechaInicioActual = null;
var _fechaFinActual = null;

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

/* El rango propuesto al entrar es el mes en curso -del dia 1 al ultimo dia-,
   que es lo que antes proponian el año y el mes de la pantalla vieja. No es
   una restriccion: es el rango mas frecuente ya tecleado, y una quincena se
   saca de ahi corriendo una de las dos fechas. */
$(document).ready(function () {
    var hoy = new Date();

    $("#inFechaInicio").val(ISOFecha(new Date(hoy.getFullYear(), hoy.getMonth(), 1)));
    $("#inFechaFin").val(ISOFecha(new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0)));

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
           vez. Sp_RTA_HeListarPeriodos ordena por FechaInicio DESC, asi que
           el primero de la lista que este Abierto es el mas reciente. */
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
        $sel.append($("<option></option>").val(p.IdPeriodo)
                                          .text(EtiquetaPeriodo(p) + " — " + p.EstadoPeriodo));
    });

    if (idPeriodoASeleccionar) { $sel.val(idPeriodoASeleccionar); }
}

/* El nombre de un periodo ya no es «Septiembre 2026»: es su rango. La
   Descripcion la arma Sp_RTA_HeCrearPeriodo con las dos fechas
   ("17/08/2026 - 15/09/2026"), y se prefiere sobre construirla aqui para que
   la pantalla, el Excel y la base digan exactamente lo mismo. El respaldo
   con FechaInicio/FechaFin es para las filas que quedaron sin descripcion
   -las que se crearon antes de esta fase-: sin el, esas salian en blanco en
   el desplegable y no habia forma de distinguirlas entre si. */
function EtiquetaPeriodo(p) {
    if (p.Descripcion) { return p.Descripcion; }
    return FormatoFecha(p.FechaInicio) + " - " + FormatoFecha(p.FechaFin);
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
    /* Un input type="date" devuelve siempre "yyyy-MM-dd" o cadena vacia, sin
       importar el idioma del navegador -que solo cambia como se VE la fecha,
       no como se lee-. Ese es justo el formato que espera Fecha() en el
       handler, asi que el valor viaja tal cual, sin reformatear. */
    var fechaInicio = $("#inFechaInicio").val();
    var fechaFin = $("#inFechaFin").val();

    if (!fechaInicio || !fechaFin) {
        MostrarMensaje("Indique la fecha de inicio y la de fin del período.", "warning");
        return;
    }

    /* Comparacion de texto y no de Date: en "yyyy-MM-dd" el orden alfabetico
       ES el cronologico, y asi no hay que construir dos Date -que sobre una
       fecha sin hora se interpretan como UTC y pueden correrse un dia contra
       la hora local-. El servidor lo vuelve a comprobar por su cuenta: esto
       es para no gastar una ida al servidor en algo que se ve desde aqui. */
    if (fechaInicio > fechaFin) {
        MostrarMensaje("La fecha de inicio no puede ser posterior a la de fin.", "warning");
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
       trata igual que cargarlo, y ademas siembra las horas que se aprobaron
       despues de la ultima apertura-, asi que este mismo boton sirve tanto
       para crear un rango nuevo como para volver a uno existente. Un rango
       que PISA a otro periodo es harina de otro costal: ese lo rechaza el
       servidor con su propio mensaje, y no se replica la comprobacion aqui
       porque el cliente no conoce los rangos ya abiertos. */
    PostHE("AbrirPeriodo", { fechaInicio: fechaInicio, fechaFin: fechaFin }, function (r) {
        CargarListaPeriodos(r.resultado.Periodo.IdPeriodo);
        PintarPantalla(r.resultado);
        $("#btnAbrirPeriodo").prop("disabled", false);
    }, function () {
        $("#btnAbrirPeriodo").prop("disabled", false);
    });
}

/* Cerrar deja de admitir cambios: nadie mas guarda horas en este periodo
   hasta que alguien -solo perfil 18- lo reabra. La confirmacion dice eso
   mismo, no "esta seguro?", y nunca es un confirm() del navegador.

   Con cambios sin guardar, cerrar NO se ofrece como confirmacion -se
   RECHAZA de una-. Cambiar de periodo o abrir uno nuevo con cambios sin
   guardar solo cuesta volver a teclear (SeleccionarPeriodo,
   AbrirPeriodoSeleccionado); cerrar es distinto: las ediciones se perderian
   Y el periodo quedaria cerrado con numeros que la persona nunca llego a
   confirmar, y para intentarlo de nuevo necesitaria que OTRA PERSONA -solo
   perfil 18- le reabra el periodo primero. Ese costo no lo puede ver quien
   solo esta respondiendo "si" a un "esta seguro?", asi que aqui no se le
   pregunta: se le pide que guarde o descarte antes de poder cerrar.

   No hace falta repetir esta comprobacion despues de que la persona confirme,
   y conviene saber POR QUE depende de dos cosas distintas de Bootstrap 3 y no
   de una: el backdrop del modal bloquea el raton, y enforceFocus reatrapa el
   foco dentro del modal en cada focusin de fuera, que es lo que impide llegar
   a una celda con el teclado. Si algun dia se reemplaza o se parchea
   bootstrap.min.js, lo segundo es lo que hay que comprobar que siga estando:
   sin enforceFocus se podria tabular hasta una celda con el modal abierto y
   editarla, y entonces esta comprobacion SI tendria que repetirse al
   confirmar. */
function ConfirmarCerrarPeriodo() {
    if (!_idPeriodoActual) { return; }

    if (HayCambiosSinGuardar()) {
        MostrarMensaje(
            "Hay cambios sin guardar. Guárdelos con el botón «Guardar», o descártelos " +
            "seleccionando el período nuevamente, antes de cerrarlo: cerrar con cambios " +
            "pendientes los perdería sin dejar ningún registro, y solo el perfil Super Admin " +
            "podría reabrir el período para que pueda intentarlo de nuevo.",
            "warning"
        );
        return;
    }

    var $contenido = $("<div></div>")
        .append($("<p></p>").text("El período dejará de admitir cambios: nadie podrá guardar horas ni observaciones hasta que se reabra."))
        .append($("<p></p>").text(TextoAvisoFueraDeHorario()));

    MostrarConfirmacion(
        $contenido,
        EjecutarCerrarPeriodo,
        "Cerrar período",
        "Sí, cerrar período"
    );
}

function EjecutarCerrarPeriodo() {
    $("#btnCerrarPeriodo").prop("disabled", true);
    PostHE("CerrarPeriodo", { idPeriodo: _idPeriodoActual }, function (r) {
        PintarPantalla(r.resultado);
        $("#btnCerrarPeriodo").prop("disabled", false);
    }, function () {
        $("#btnCerrarPeriodo").prop("disabled", false);
    });
}

/* Reabrir es la excepcion, no la norma: solo perfil 18 -este boton ni
   siquiera se pinta para los demas, y el handler lo vuelve a comprobar por su
   cuenta-. La confirmacion avisa que va a quedar registrado quien lo hizo,
   porque desde la tarea 1 eso es verdad: hay auditoria de por medio. */
/* Vuelve a consultar las aprobaciones del mismo rango y reescribe las filas
   que nadie corrigio a mano. Es la misma accion que «Abrir periodo» -que es
   idempotente para un rango identico-, pero sin pedirle a nadie que reescriba
   las fechas.

   Hace falta un boton propio porque elegir el periodo en el desplegable NO
   recalcula: eso llama a CargarPeriodo, que solo lee lo guardado. Sin este
   boton, alguien mira la foto de ayer y concluye que no han aprobado nada. */
function ConfirmarRecalcular() {
    if (!_idPeriodoActual || !_fechaInicioActual || !_fechaFinActual) { return; }

    if (HayCambiosSinGuardar()) {
        MostrarMensaje("Guarde los cambios antes de traer las aprobaciones nuevas.", "warning");
        return;
    }

    MostrarConfirmacion(
        "Se volverán a consultar las horas aprobadas del período. " +
        "Las filas corregidas a mano no se tocan.",
        EjecutarRecalcular,
        "Traer aprobaciones nuevas",
        "Sí, traer las nuevas"
    );
}

function EjecutarRecalcular() {
    $("#btnRecalcular").prop("disabled", true);

    PostHE("AbrirPeriodo",
           { fechaInicio: _fechaInicioActual, fechaFin: _fechaFinActual },
           function (r) {
               PintarPantalla(r.resultado);
               $("#btnRecalcular").prop("disabled", false);
           },
           function () {
               $("#btnRecalcular").prop("disabled", false);
           });
}

/* Corregir las fechas de un periodo que YA existe.

   No se parece a "Abrir / actualizar": aquel manda un rango y el servidor busca
   o crea el periodo de ESE rango. Este manda el IdPeriodo del periodo cargado y
   le cambia las fechas, que es la unica forma de mover un rango sin abandonar el
   periodo y sus correcciones. */
function ConfirmarEditarFechas() {
    if (!_idPeriodoActual) {
        MostrarMensaje("Elija primero el período que quiere corregir.", "warning");
        return;
    }

    var fechaInicio = $("#inFechaInicio").val();
    var fechaFin = $("#inFechaFin").val();

    if (!fechaInicio || !fechaFin) {
        MostrarMensaje("Indique la fecha de inicio y la de fin.", "warning");
        return;
    }

    /* Comparacion de cadenas y no de fechas: un input type="date" siempre
       devuelve "yyyy-MM-dd", que ordena igual como texto que como fecha. Es la
       misma comprobacion que hace AbrirPeriodoSeleccionado. */
    if (fechaInicio > fechaFin) {
        MostrarMensaje("La fecha de inicio no puede ser posterior a la de fin.", "warning");
        return;
    }

    if (fechaInicio === _fechaInicioActual && fechaFin === _fechaFinActual) {
        MostrarMensaje("Las fechas son las mismas que ya tiene el período.", "warning");
        return;
    }

    /* Mismo criterio que recalcular: el guardado se hace fila por fila, y lo que
       este en la grilla sin guardar se perderia cuando el servidor devuelva la
       pantalla recalculada. */
    if (HayCambiosSinGuardar()) {
        MostrarMensaje("Guarde los cambios antes de corregir las fechas.", "warning");
        return;
    }

    MostrarConfirmacion(
        "El período pasará a ir del " + FechaLegible(fechaInicio) +
        " al " + FechaLegible(fechaFin) + ". Se volverán a traer las horas " +
        "aprobadas de ese rango y se recalcularán los sueldos al nuevo corte, " +
        "así que los totales pueden cambiar. Las filas corregidas a mano no se tocan.",
        EjecutarEditarFechas,
        "Corregir las fechas del período",
        "Sí, corregir"
    );
}

/* "2026-08-14" -> "14/08/2026", para el texto de la confirmacion.

   No reusa FormatoFecha a proposito: aquella recibe lo que manda el SERVIDOR y
   pasa por new Date(valor). Aqui el valor viene del input type="date", y
   new Date("2026-08-14") interpreta la cadena como UTC: leida en Ecuador
   (UTC-5) devuelve el dia ANTERIOR. La confirmacion diria una fecha y se
   guardaria otra, que es la peor forma de equivocarse en un aviso que existe
   justamente para que la persona revise las fechas. Partir la cadena no puede
   fallar asi. */
function FechaLegible(iso) {
    var p = String(iso).split("-");
    return p.length === 3 ? p[2] + "/" + p[1] + "/" + p[0] : iso;
}

function EjecutarEditarFechas() {
    $("#btnEditarFechas").prop("disabled", true);

    PostHE("EditarFechasPeriodo",
           {
               idPeriodo: _idPeriodoActual,
               fechaInicio: $("#inFechaInicio").val(),
               fechaFin: $("#inFechaFin").val()
           },
           function (r) {
               PintarPantalla(r.resultado);
               $("#btnEditarFechas").prop("disabled", false);

               /* El desplegable muestra la Descripcion, que se arma del rango:
                  despues de corregir, la opcion cargada sigue diciendo las
                  fechas viejas hasta que se vuelva a listar. */
               CargarListaPeriodos(_idPeriodoActual);
           },
           function () {
               $("#btnEditarFechas").prop("disabled", false);
           });
}

function ConfirmarReabrirPeriodo() {
    if (!_idPeriodoActual) { return; }

    MostrarConfirmacion(
        "El período volverá a admitir cambios, y quedará registrado quién lo reabrió.",
        EjecutarReabrirPeriodo,
        "Reabrir período",
        "Sí, reabrir período"
    );
}

function EjecutarReabrirPeriodo() {
    $("#btnReabrirPeriodo").prop("disabled", true);
    PostHE("ReabrirPeriodo", { idPeriodo: _idPeriodoActual }, function (r) {
        PintarPantalla(r.resultado);
        $("#btnReabrirPeriodo").prop("disabled", false);
    }, function () {
        $("#btnReabrirPeriodo").prop("disabled", false);
    });
}

/* Exporta a Excel el periodo que esta cargado ahora mismo. El boton esta
   habilitado desde que hay un periodo cargado -se habilita aqui mismo, en
   PintarPantalla- y se deja visible tanto en Abierto como en Cerrado: revisar
   el archivo antes de cerrar es legitimo, y el propio archivo dice
   PROVISIONAL en su interior mientras el periodo no este cerrado.

   El archivo sale de lo que hay en la base -DescargarHorasExtras.ashx vuelve
   a leer el periodo por su cuenta con NegHorasExtrasPantalla.CargarPantalla,
   no recibe nada de esta pantalla-, no de lo que se ve en pantalla. Con
   cambios sin guardar se avisa con el mismo modal informativo que usa el
   cierre (ver ConfirmarCerrarPeriodo) y no se descarga: quien exportara 40
   filas editadas sin guardar se llevaria un archivo que no coincide con lo
   que tiene delante, y no tendria forma de notarlo. */
function ExportarExcel() {
    if (!_idPeriodoActual) { return; }

    if (HayCambiosSinGuardar()) {
        MostrarMensaje(
            "Hay cambios sin guardar. El archivo se genera con lo último guardado en la " +
            "base, no con lo que ve en pantalla: guárdelos o descártelos antes de exportar " +
            "para que coincidan.",
            "warning"
        );
        return;
    }

    /* window.open(..., "_blank") y no window.location.href: con
       Content-Disposition:attachment el navegador se queda donde esta de
       cualquier forma, pero cuando el handler falla responde texto plano SIN
       esa cabecera -es justo lo que hace al fallar-, y ahi location.href
       navegaba la pestaña actual: la pantalla de horas extras desaparecia,
       reemplazada por el mensaje de error, y con ella cualquier cambio sin
       guardar que hubiera. Mismo patron que el enlace target="_blank" de la
       descarga de MiPerfil.aspx, adaptado a un boton porque este necesita las
       dos comprobaciones de arriba antes de poder armar la URL. */
    window.open("DescargarHorasExtras.ashx?periodo=" + encodeURIComponent(_idPeriodoActual), "_blank");
}

function PintarPantalla(pantalla) {
    _idPeriodoActual = pantalla.Periodo.IdPeriodo;
    _periodoAbierto = pantalla.Periodo.EstaAbierto;
    _fechaInicioActual = ISODesdeValor(pantalla.Periodo.FechaInicio);
    _fechaFinActual = ISODesdeValor(pantalla.Periodo.FechaFin);

    /* Los dos campos se llenan con las fechas del periodo cargado. Sin esto,
       "Corregir fechas" obligaria a teclear las DOS aunque solo cambie una, y un
       error de tipeo en la que no se queria tocar moveria el periodo sin que
       nadie lo note.

       Efecto de lado, buscado: con un periodo cargado, "Abrir / actualizar" pasa
       a reabrir ESE periodo en vez de lo que hubiera quedado tecleado de antes.
       Es inofensivo -abrir el mismo rango es idempotente- y es lo que la mayoria
       espera al apretarlo. */
    $("#inFechaInicio").val(_fechaInicioActual || "");
    $("#inFechaFin").val(_fechaFinActual || "");

    /* Habilitado desde la primera vez que se carga un periodo, y se queda asi
       para el resto de la sesion en esta pantalla -cambiar de periodo o de
       estado nunca lo vuelve a deshabilitar, porque siempre hay un periodo
       cargado a partir de aqui. */
    $("#btnExportarExcel").prop("disabled", false);

    /* Factor50/Factor100 vienen del servidor en cada carga: son los que
       HE_Parametro tenia vigentes cuando esta misma pantalla se calculo alla.
       Se toman de aqui, no de una constante local, para que un cambio en
       HE_Parametro se refleje sin tocar este archivo. */
    _factor50 = Number(pantalla.Factor50);
    _factor100 = Number(pantalla.Factor100);

    ActualizarEstadoPeriodo(pantalla.Periodo);
    ActualizarFiltroEmpresa(pantalla.Filas);
    PintarGrilla(pantalla);
    CargarFueraDeHorario();
}

function ActualizarEstadoPeriodo(periodo) {
    var $lbl = $("#lblEstadoPeriodo").text(periodo.EstadoPeriodo)
        .removeClass("label-success label-default label-danger");

    /* Los tres colores salen de dos-tema.css a traves de las clases de
       Bootstrap que ya trae la casa -label-success, label-default y
       label-danger-, la misma paleta que el §5.1 funcional pide: verde
       Abierto, gris Cerrado, rojo Anulado. */
    if (periodo.EstadoPeriodo === "Abierto") { $lbl.addClass("label-success"); }
    else if (periodo.EstadoPeriodo === "Anulado") { $lbl.addClass("label-danger"); }
    else { $lbl.addClass("label-default"); }

    /* En Cerrado o Anulado el boton de guardado desaparece: no hay nada que
       guardar en un periodo de solo lectura. */
    $("#btnGuardar").toggle(periodo.EstaAbierto);

    /* Cerrar (14 o 18) solo tiene sentido sobre un periodo Abierto. Reabrir
       (solo 18, y solo cortesia visual -la barrera real esta en el handler-)
       solo sobre uno Cerrado: un periodo Anulado no se reabre desde aqui. */
    $("#btnCerrarPeriodo").toggle(periodo.EstaAbierto);
    $("#btnReabrirPeriodo").toggle(periodo.EstadoPeriodo === "Cerrado" && HE_PUEDE_REABRIR);

    /* Traer aprobaciones nuevas solo sobre un periodo Abierto: en uno Cerrado
       las cifras ya se liquidaron y resembrarlas cambiaria un pago cerrado sin
       que quede rastro de por que. Si hace falta, primero se reabre -y eso si
       queda auditado-. */
    $("#btnRecalcular").toggle(periodo.EstaAbierto && !!_fechaInicioActual);

    /* Corregir fechas, igual que recalcular, solo sobre un periodo Abierto. El
       handler y el procedimiento lo vuelven a comprobar -el -6-: esto es cortesia
       visual, no la barrera. */
    $("#btnEditarFechas").toggle(periodo.EstaAbierto && !!_idPeriodoActual);

    ActualizarInfoCierre(periodo);
}

/* Quien cerro el periodo y cuando, de un vistazo -sin esto, FechaCierre y
   UsuarioCierre viajaban en el payload desde la fase 2 y no se mostraban en
   ningun lado. Solo tiene sentido con el periodo Cerrado: uno Abierto nunca
   tiene fecha de cierre, y uno Anulado no es el caso que pidio el §5.1. */
function ActualizarInfoCierre(periodo) {
    var $info = $("#lblInfoCierre");

    if (periodo.EstadoPeriodo === "Cerrado" && periodo.FechaCierre) {
        $info.text("Cerrado por " + (periodo.UsuarioCierre || "–") +
                    " el " + FormatoFechaHora(periodo.FechaCierre)).show();
    } else {
        $info.hide();
    }
}

/* ------------------------------------------------------------------ grilla -- */

function PintarGrilla(pantalla) {
    var $cuerpo = $("#cuerpoHE").empty();

    if (!pantalla.Filas || pantalla.Filas.length === 0) {
        $cuerpo.append('<tr><td colspan="19" class="text-center text-muted">' +
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
       quedan ocultas: la tabla se desalinea, 16 columnas de encabezado contra
       10 de datos. Se reaplica aqui el estado vigente para que ambos coincidan
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
    /* Las horas ya no se teclean: salen de los tramos que Nomina incluye en la
       ventana de detalle, y el servidor ignora las que manda GuardarFila. Las
       celdas quedan deshabilitadas SIEMPRE -no solo readonly-: un input
       readonly sigue recibiendo foco y el evento paste, y el pegado desde Excel
       (AplicarPegado) repartiria valores que nadie va a guardar. Deshabilitadas,
       .val() sigue funcionando para los totales y para GuardarFila. */
    var horasDeshabilitadas = true;
    var tituloHoras = "Las horas salen de los tramos incluidos: se deciden en el detalle de la persona.";

    var $fila = $("<tr></tr>")
        .attr("data-empleado", fila.IdEmpleado)
        .attr("data-empresa", fila.EmpresaSnapshot || "")
        .attr("data-buscable", ((fila.NombreSnapshot || "") + " " + (fila.CedulaSnapshot || "") + " " +
                                (fila.CargoSnapshot || "")).toLowerCase())
        .data("salario", fila.SalarioBaseSnapshot)
        .data("divisor", fila.Divisor)
        /* Para valorar en la ventana de detalle lo que cambia al decidir, con el
           mismo valor hora que usa el servidor (y 0 si no aplica HE, igual que
           NegHeDecisiones.ValorHora). */
        .data("nombre", fila.NombreSnapshot || "")
        .data("vh50", aplicaHE ? Number(fila.ValorHora50) || 0 : 0)
        .data("vh100", aplicaHE ? Number(fila.ValorHora100) || 0 : 0);

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

    var $nombre = $("<td></td>").text(fila.NombreSnapshot || "–");

    /* Ajuste de sueldo: solo perfiles 14 y 18 (el handler lo vuelve a comprobar).
       El botón es solo un ícono, sin texto, para que .text() de la celda siga
       siendo el nombre: NombreDeFila y el título del detalle lo leen así. */
    if (typeof HE_PUEDE_EDITAR_MAESTRO !== "undefined" && HE_PUEDE_EDITAR_MAESTRO) {
        $nombre.append(" ").append(
            $('<button type="button" class="btn btn-link btn-xs" title="Registrar ajuste de sueldo"><i class="fa fa-usd"></i></button>')
                .on("click", function () {
                    AbrirAjusteSalario(fila.IdEmpleado, fila.NombreSnapshot || "", fila.SalarioBaseSnapshot);
                }));
    }

    /* El detalle de tramos se abre desde aqui para TODOS: los enlaces de las
       columnas «no incluido» solo aparecen si hay algo sin incluir, y Nomina
       tambien necesita ver (o excluir) lo aprobado de quien tiene cero ahi.
       Solo ícono, por la misma razón que el de arriba. */
    $nombre.append(" ").append(
        $('<button type="button" class="btn btn-link btn-xs" title="Ver tramos y decidir qué se paga"><i class="fa fa-list"></i></button>')
            .on("click", function () { MostrarDetalleFuera(fila.IdEmpleado); }));

    $fila.append($nombre);
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
        .attr("title", tituloHoras)
        .prop("disabled", horasDeshabilitadas);
    $fila.append($("<td></td>").append($horas50));

    $fila.append(CeldaDerivada(FormatoDecimales(fila.ValorHora50, 4)));
    $fila.append($('<td class="text-right he-total50 he-col-derivada" style="display:none"></td>')
        .text(FormatoDosDecimales(fila.Total50)));

    var $horas100 = $('<input type="text" class="form-control input-sm he-horas100" maxlength="6" />')
        .val(FormatoDosDecimales(fila.Horas100))
        .attr("title", tituloHoras)
        .prop("disabled", horasDeshabilitadas);
    $fila.append($("<td></td>").append($horas100));

    $fila.append(CeldaDerivada(FormatoDecimales(fila.ValorHora100, 4)));
    $fila.append($('<td class="text-right he-total100 he-col-derivada" style="display:none"></td>')
        .text(FormatoDosDecimales(fila.Total100)));

    $fila.append($('<td class="text-right he-total-horas"></td>').text(FormatoDosDecimales(fila.TotalHoras)));
    $fila.append($('<td class="text-right he-total-he"></td>').text(FormatoDosDecimales(fila.TotalHE)));

    /* Informativas: las llena PintarFueraDeHorario cuando llega la respuesta. */
    $fila.append($('<td class="text-right he-fuera-sin"></td>').text("…"));
    $fila.append($('<td class="text-right he-fuera-rech"></td>').text("…"));
    $fila.append($('<td class="text-right he-total-con-excluido"></td>').text("…"));

    var $obs = $('<input type="text" class="form-control input-sm he-observacion" maxlength="400" />')
        .val(fila.Observacion || "")
        .prop("disabled", !_periodoAbierto);
    $fila.append($("<td></td>").append($obs));

    $fila.append($('<td class="text-center"></td>').append(EtiquetaOrigen(fila.HorasOrigen)));

    return $fila;
}

/* De donde salieron las horas de esta fila. Lo que le importa a quien mira la
   grilla es distinguir de un vistazo lo que revisó una persona: «Manual» es
   la excepcion -alguien corrigio esa fila a mano, y volver a abrir el periodo
   ya no la pisa- y por eso va en azul; «Tareas» es lo normal -sembrado desde
   las solicitudes ya aprobadas- y va en gris discreto, para no gritar sesenta
   veces lo mismo.

   Cualquier valor que no sea "Manual" se pinta como "Tareas": el contrato
   solo tiene esos dos, y una fila de un periodo viejo que llegue sin el campo
   no es una correccion manual. */
function EtiquetaOrigen(origen) {
    var manual = String(origen || "").toLowerCase() === "manual";

    return $("<span></span>")
        .addClass("he-origen " + (manual ? "he-origen-manual" : "he-origen-tareas"))
        .text(manual ? "Manual" : "Tareas");
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
   .off().on() antes de asignar el manejador: sin eso, cada llamada sumaria un
   manejador mas al mismo boton y una confirmacion vieja se dispararia junto
   con la nueva.

   Nacio solo para el pegado desde Excel, con titulo y boton fijos en el
   marcado. Cerrar y reabrir el periodo lo reusan con su propio titulo y
   texto de boton -titulo/textoBoton son opcionales y caen en los mismos
   valores de siempre si no se dan, asi que el pegado sigue exactamente
   igual-. Los dos se fijan SIEMPRE, nunca solo cuando vienen dados: el modal
   es uno solo compartido por toda la pantalla, y sin esto un titulo de un
   cierre anterior se quedaria pegado en la siguiente confirmacion de pegado. */
function MostrarConfirmacion(contenido, alConfirmar, titulo, textoBoton) {
    var $cuerpo = $("#textoConfirmarPegado").empty();

    if (typeof contenido === "string") { $cuerpo.text(contenido); }
    else { $cuerpo.append(contenido); }

    $("#modalConfirmarPegadoLabel").text(titulo || "Confirmar pegado");
    $("#btnConfirmarPegado").text(textoBoton || "Sí, aplicar");

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

/* ------------------------------------------------------ fuera de horario -- */

/* Tiempo registrado fuera del horario de cada persona que NO entra en el pago:
   sin solicitar, pendiente o aprobado sin tipo («sin aprobar»), y lo que la
   jefatura rechazó. Es información para que Nómina decida antes de cerrar;
   nada de esto cambia un monto.

   Se pide aparte de la grilla, y si falla la grilla sigue igual: las dos
   columnas dicen «no disponible» y nada más. */
var _fueraDeHorario = null;
var _idPeriodoFuera = null;
var _estadoFuera = "sin-datos";   // "cargando" | "ok" | "error"

/* Totales de lo que SE VE en la grilla. El servidor puede traer a alguien activo
   en el maestro que no está sembrado en el período; si entrara aquí, el pie y el
   aviso de cierre contarían a una persona que nadie puede ver ni abrir. */
var _resumenFuera = { personasSin: 0, personasRech: 0, minutosSin: 0, minutosRech: 0, usdSin: 0, usdRech: 0, totalConExcluido: 0 };

function CargarFueraDeHorario() {
    var idPeriodo = _idPeriodoActual;
    if (!idPeriodo) { return; }

    _idPeriodoFuera = idPeriodo;
    _fueraDeHorario = null;
    _estadoFuera = "cargando";
    PintarFueraDeHorario();

    /* suprimirMensaje: un fallo aquí no merece un modal encima de la grilla,
       que sí cargó bien. Se dice en las propias columnas. */
    PostHE("CargarFueraDeHorario", { idPeriodo: idPeriodo }, function (r) {
        /* Si entretanto se eligió otro período, esta respuesta ya no es de él. */
        if (idPeriodo !== _idPeriodoActual) { return; }
        _fueraDeHorario = r.resultado;
        _estadoFuera = (r.resultado && r.resultado.Calculado) ? "ok" : "error";
        PintarFueraDeHorario();
    }, function () {
        if (idPeriodo !== _idPeriodoActual) { return; }
        _estadoFuera = "error";
        PintarFueraDeHorario();
    }, true);
}

/* Las columnas «no incluido» y «Total incluyendo no incluido».

   De donde sale cada numero (decision de la Task 5, a proposito):
   - Las HORAS de «sin aprobar» y «rechazado» son las de Personas, que arma el
     servidor (ArmarNoIncluido + NegHeFueraDeHorario): ya descuenta por minuto lo
     que paga otro segmento incluido y deja fuera lo bloqueado y los 7 dias de
     borde. Sumar los candidatos aqui contaria dos veces un minuto que se pisa.
   - Los USD de esas dos columnas se valoran aqui con los mismos minutos por tipo
     (50/100) y el valor hora de la fila, redondeando UNA vez por celda, igual
     que NegHeDecisiones.Usd. El servidor no manda ese USD partido en sin
     aprobar / rechazado: Conciliacion lo trae junto (UsdExcluido).
   - «Total incluyendo no incluido» es Conciliacion.TotalConExcluido, tal cual
     lo calcula el servidor (TotalHE + UsdExcluido). Puede ser mayor que Total HE
     + las dos columnas: UsdExcluido cuenta ademas lo APROBADO que Nomina excluyo
     y lo que sobra de un tramo reducido, que no son ni «sin aprobar» ni
     «rechazado». Recalcularlo aqui con otra regla haria que la pantalla y el
     Excel no cuadren. */
function PintarFueraDeHorario() {
    var porEmpleado = {};
    var conciliacion = {};
    if (_estadoFuera === "ok") {
        $.each(_fueraDeHorario.Personas || [], function (i, p) { porEmpleado[String(p.IdEmpleado)] = p; });
        $.each(_fueraDeHorario.Conciliacion || [], function (i, c) { conciliacion[String(c.IdEmpleado)] = c; });
    }

    var resumen = { personasSin: 0, personasRech: 0, minutosSin: 0, minutosRech: 0, usdSin: 0, usdRech: 0, totalConExcluido: 0 };

    $("#cuerpoHE tr[data-empleado]").each(function () {
        var $fila = $(this);
        var idEmpleado = $fila.attr("data-empleado");
        var $sin = $fila.find(".he-fuera-sin").empty();
        var $rech = $fila.find(".he-fuera-rech").empty();
        var $con = $fila.find(".he-total-con-excluido").empty();

        if (_estadoFuera === "cargando") { $sin.text("…"); $rech.text("…"); $con.text("…"); return; }
        if (_estadoFuera !== "ok") { $sin.text("no disponible"); $rech.text("no disponible"); $con.text("no disponible"); return; }

        var p = porEmpleado[idEmpleado];
        var vh50 = Number($fila.data("vh50")) || 0;
        var vh100 = Number($fila.data("vh100")) || 0;

        var sin50 = p ? p.MinutosSinAprobar50 : 0, sin100 = p ? p.MinutosSinAprobar100 : 0;
        var rech50 = p ? p.MinutosRechazado50 : 0, rech100 = p ? p.MinutosRechazado100 : 0;
        var usdSin = UsdDeMinutos(sin50, sin100, vh50, vh100);
        var usdRech = UsdDeMinutos(rech50, rech100, vh50, vh100);

        $sin.append(EnlaceFuera(sin50 + sin100, usdSin, idEmpleado));
        $rech.append(EnlaceFuera(rech50 + rech100, usdRech, idEmpleado));

        /* Sin fila de conciliacion (no deberia pasar: hay una por fila del
           periodo) se cae en el Total HE de la grilla, que es lo que se paga. */
        var c = conciliacion[idEmpleado];
        var totalCon = c ? Number(c.TotalConExcluido) : NumeroDe($fila.find(".he-total-he").text());
        $con.text(FormatoDosDecimales(totalCon)).toggleClass("text-warning", !!(c && c.Desactualizada));

        /* El desglose va en el title de la celda: sin el, Total HE + las dos
           columnas no suma la columna nueva y nada explica la diferencia. Esa
           diferencia es lo aprobado que Nomina excluyo y lo que sobra de un tramo
           reducido, que el servidor cuenta en UsdExcluido y no es ni «sin
           aprobar» ni «rechazado». Sale por resta de montos ya redondeados, asi
           que un -0.01 de redondeo se muestra como 0.00: nunca negativo. */
        if (c) {
            var otros = RedondearDos(Number(c.UsdExcluido) - usdSin - usdRech);
            if (otros < 0) { otros = 0; }
            var titulo = "Total HE " + FormatoDosDecimales(c.TotalAPagar) +
                         " + sin aprobar " + FormatoDosDecimales(usdSin) +
                         " + rechazado " + FormatoDosDecimales(usdRech) +
                         " + otros no incluidos (aprobado excluido o sobrante de reducciones) " +
                         FormatoDosDecimales(otros) + ".";
            if (c.Desactualizada) { titulo += " " + TextoFilaDesactualizada(); }
            $con.attr("title", titulo);
        } else {
            $con.removeAttr("title");
        }

        resumen.minutosSin += sin50 + sin100;
        resumen.minutosRech += rech50 + rech100;
        resumen.usdSin += usdSin;
        resumen.usdRech += usdRech;
        resumen.totalConExcluido += totalCon;
        if (sin50 + sin100 > 0) { resumen.personasSin++; }
        if (rech50 + rech100 > 0) { resumen.personasRech++; }
    });

    _resumenFuera = resumen;

    if (_estadoFuera === "ok") {
        /* Se suman minutos y se pasa a horas una sola vez, igual que el servidor;
           los USD son la suma de las celdas, ya redondeadas, como el resto del pie. */
        $("#pieFueraSinAprobar").text(TextoHorasUsd(resumen.minutosSin, resumen.usdSin));
        $("#pieFueraRechazado").text(TextoHorasUsd(resumen.minutosRech, resumen.usdRech));
        $("#pieTotalConExcluido").text("USD " + FormatoDosDecimales(resumen.totalConExcluido));
    } else {
        $("#pieFueraSinAprobar, #pieFueraRechazado, #pieTotalConExcluido").text("");
    }

    PintarAvisoDesactualizadas();
    PintarFueraDelModulo();
}

/* Minutos por tipo a USD, redondeado una sola vez: la misma cuenta que
   NegHeDecisiones.Usd. El valor hora ya viene en 0 si la persona no aplica HE. */
function UsdDeMinutos(minutos50, minutos100, vh50, vh100) {
    return RedondearDos((minutos50 * vh50 + minutos100 * vh100) / 60);
}

function TextoHorasUsd(minutos, usd) {
    return FormatoDosDecimales(minutos / 60) + " h · USD " + FormatoDosDecimales(usd);
}

/* La frase del aviso de filas desactualizadas. null si no hay ninguna o si no
   se pudo calcular (ese caso ya lo dice TextoAvisoFueraDeHorario). */
function TextoFilasDesactualizadas() {
    if (_estadoFuera !== "ok" || !_fueraDeHorario) { return null; }
    var n = Number(_fueraDeHorario.FilasDesactualizadas) || 0;
    if (n <= 0) { return null; }
    return "Hay " + n + " fila(s) desactualizadas respecto de las decisiones: " + AccionParaActualizar() + ".";
}

/* Que hacer con una fila desactualizada. El boton que recalcula es «Traer
   aprobaciones nuevas», y solo aparece con el periodo abierto: con el periodo
   cerrado, pedir que se pulse seria mandar a buscar un boton que no esta. */
function AccionParaActualizar() {
    return _periodoAbierto
        ? "pulse «Traer aprobaciones nuevas»"
        : "reabra el período para actualizarlo";
}

function TextoFilaDesactualizada() {
    return "Fila desactualizada respecto de las decisiones: " + AccionParaActualizar() + ".";
}

/* Un Total HE que ya no es lo que pagan los tramos incluidos hoy -alguien decidio
   o aprobo algo y el recalculo no corrio- es un pago equivocado a punto de
   cerrarse. Se dice arriba de la grilla, no escondido en un pie. */
function PintarAvisoDesactualizadas() {
    var texto = TextoFilasDesactualizadas();
    var $aviso = $("#avisoDesactualizadas");
    if (texto) { $aviso.text(texto).show(); } else { $aviso.hide().empty(); }
}

/* ------------------------------------------ personas fuera del módulo -- */

/* Empleados activos con horas en el período que NO están en el módulo: no
   tienen sueldo cargado, así que no se les puede calcular el pago. Se muestran
   para que nadie cierre el período sin saberlo. Todo con .text(). */
function PintarFueraDelModulo() {
    var lista = (_estadoFuera === "ok" && _fueraDeHorario && _fueraDeHorario.FueraDelModulo) || [];
    var $recuadro = $("#recuadroFueraDelModulo");
    var $cuerpo = $("#cuerpoFueraDelModulo").empty();

    if (lista.length === 0) { $recuadro.hide(); return; }

    $("#tituloFueraDelModulo").text(lista.length === 1
        ? "1 persona con horas registradas no está en el módulo de Horas Extras."
        : lista.length + " personas con horas registradas no están en el módulo de Horas Extras.");
    $("#motivoFueraDelModulo").text(lista[0].Motivo);

    $.each(lista, function (i, p) {
        var $accion = $("<td></td>");
        if (typeof HE_PUEDE_EDITAR_MAESTRO !== "undefined" && HE_PUEDE_EDITAR_MAESTRO) {
            $accion.append($('<button type="button" class="btn btn-warning btn-xs"></button>')
                .text("Completar datos")
                .on("click", function () { AbrirCompletarDatos(p); }));
        }

        $cuerpo.append($("<tr></tr>")
            .append($("<td></td>").text(p.Nombre))
            .append($("<td></td>").text(p.Cargo))
            .append($('<td class="text-right"></td>').text(FormatoDosDecimales(p.HorasAprobadas50)))
            .append($('<td class="text-right"></td>').text(FormatoDosDecimales(p.HorasAprobadas100)))
            .append($('<td class="text-right"></td>').text(FormatoDosDecimales(p.HorasSinAprobar)))
            .append($('<td class="text-right"></td>').text(FormatoDosDecimales(p.HorasRechazado)))
            .append($accion));
    });

    $recuadro.show();
}

/* Las empresas que ya existen en el módulo, tomadas de la grilla. Así nadie
   escribe una quinta variante de «DOS S.A.». */
function EmpresasDeLaGrilla() {
    var vistas = {};
    var lista = [];
    $("#cuerpoHE tr[data-empresa]").each(function () {
        var e = $(this).attr("data-empresa");
        if (e && !vistas[e]) { vistas[e] = true; lista.push(e); }
    });
    return lista.sort();
}

function AbrirCompletarDatos(persona) {
    $("#tituloCompletarDatos").text("Completar datos — " + persona.Nombre);

    /* Sin preselección: elegir la empresa es obligatorio, y la primera en orden
       alfabético no tiene por qué ser la de esta persona. */
    var $empresa = $("#cdEmpresa").empty().append($("<option></option>").val("").text("— Seleccione —"));
    $.each(EmpresasDeLaGrilla(), function (i, e) { $empresa.append($("<option></option>").val(e).text(e)); });

    $("#cdCargo").val(persona.Cargo || "");
    $("#cdJornada").val("8");
    $("#cdAplicaHE").prop("checked", true);
    $("#cdMonto").val("");
    $("#cdVigencia").val(_fechaInicioActual || "");

    PrepararConfirmacion($("#modalCompletarDatos"), $("#btnGuardarCompletarDatos"), $("#cdConfirmacion"));

    $("#btnGuardarCompletarDatos").off("click").on("click", function () {
        var $boton = $(this);

        /* Primer clic: mostrar a quién y cuánto. Segundo clic: guardar. Así un
           sueldo no termina en la fila de al lado sin que nadie lo note. */
        if (!$boton.data("confirmado")) {
            PedirConfirmacion($boton, $("#cdConfirmacion"), "Vas a agregar al módulo a ",
                              persona.Nombre, $("#cdMonto").val(), $("#cdVigencia").val());
            return;
        }

        $boton.prop("disabled", true);   // un doble clic no inserta dos veces

        PostHE("AgregarColaborador", {
            idPeriodo: _idPeriodoActual,
            idEmpleado: persona.IdEmpleado,
            empresa: $("#cdEmpresa").val(),
            cargo: $("#cdCargo").val(),
            jornada: $("#cdJornada").val(),
            aplicaHE: $("#cdAplicaHE").is(":checked"),
            monto: $("#cdMonto").val(),
            vigencia: $("#cdVigencia").val()
        }, function (r) {
            $("#modalCompletarDatos").modal("hide");
            if (r.resultado && r.resultado.Periodo) { PintarPantalla(r.resultado); }
        }, function () {
            ReiniciarConfirmacion($boton, $("#cdConfirmacion"));
        });
    });

    $("#modalCompletarDatos").modal("show");
}

function AbrirAjusteSalario(idEmpleado, nombre, salarioActual) {
    $("#tituloAjusteSalario").text("Registrar ajuste de sueldo — " + nombre);
    $("#textoAjusteSalario").text("Sueldo usado en este período: USD " + FormatoDosDecimales(salarioActual) +
        ". El ajuste se agrega al historial; el sueldo anterior no se borra.");
    $("#ajMonto").val("");
    $("#ajVigencia").val(_fechaInicioActual || "");
    $("#ajObservacion").val("");

    PrepararConfirmacion($("#modalAjusteSalario"), $("#btnGuardarAjusteSalario"), $("#ajConfirmacion"));

    $("#btnGuardarAjusteSalario").off("click").on("click", function () {
        var $boton = $(this);

        if (!$boton.data("confirmado")) {
            PedirConfirmacion($boton, $("#ajConfirmacion"), "Vas a registrar un ajuste de sueldo a ",
                              nombre, $("#ajMonto").val(), $("#ajVigencia").val());
            return;
        }

        $boton.prop("disabled", true);   // un doble clic no inserta dos veces

        PostHE("AgregarAjusteSalario", {
            idPeriodo: _idPeriodoActual,
            idEmpleado: idEmpleado,
            monto: $("#ajMonto").val(),
            vigencia: $("#ajVigencia").val(),
            observacion: $("#ajObservacion").val()
        }, function (r) {
            $("#modalAjusteSalario").modal("hide");
            if (r.resultado && r.resultado.Periodo) { PintarPantalla(r.resultado); }
        }, function () {
            ReiniciarConfirmacion($boton, $("#ajConfirmacion"));
        });
    });

    $("#modalAjusteSalario").modal("show");
}

/* Con cero, un guion; con tiempo, un enlace «h · USD» que abre el detalle. */
function EnlaceFuera(minutos, usd, idEmpleado) {
    if (!Number(minutos)) { return $('<span class="text-muted"></span>').text("—"); }

    return $('<a href="javascript:void(0)"></a>')
        .text(TextoHorasUsd(minutos, usd))
        .on("click", function () { MostrarDetalleFuera(idEmpleado); });
}

/* El orden de los grupos en la ventana: primero lo que ya se paga por omision,
   despues lo que Nomina tendria que decidir incluir. Un grupo que el servidor
   mande y no este aqui se pinta al final, para no perder ningun tramo. */
var GRUPOS_DETALLE = ["Aprobado", "Sin solicitar", "Pendiente", "Aprobado sin tipo", "Rechazado"];

/* La ventana de detalle de UNA persona: sus tramos candidatos, agrupados, con la
   decision vigente de cada uno y los controles para cambiarla.

   Se pinta desde _fueraDeHorario.Candidatos, que ya viene resuelto por el
   servidor (decision vigente, valor por omision, bloqueo por otro periodo y
   USD). Se guarda en cada fila el valor inicial de sus controles: al guardar
   solo viajan los tramos que cambiaron, porque cada decision queda registrada
   con usuario y fecha, y reenviar las que nadie toco llenaria el historial de
   decisiones que no se tomaron. */
function MostrarDetalleFuera(idEmpleado) {
    if (_estadoFuera === "cargando") {
        MostrarMensaje("Todavía se están calculando los tramos del período. Intente en unos segundos.", "info");
        return;
    }
    if (_estadoFuera !== "ok" || !_fueraDeHorario || _idPeriodoFuera !== _idPeriodoActual) {
        MostrarMensaje("No se pudieron calcular los tramos del período: " +
                       ((_fueraDeHorario && _fueraDeHorario.Error) || "vuelva a seleccionar el período."), "warning");
        return;
    }

    var $filaGrilla = $("#cuerpoHE tr[data-empleado='" + idEmpleado + "']");
    var nombre = $.trim($filaGrilla.data("nombre") || NombreDeFila($filaGrilla));
    var vh50 = Number($filaGrilla.data("vh50")) || 0;
    var vh100 = Number($filaGrilla.data("vh100")) || 0;

    /* Deshabilitado es cortesia: el handler rechaza al perfil 2 y el
       procedimiento, un periodo cerrado o un tramo que ya paga otro periodo. */
    var puedeEditar = typeof HE_PUEDE_EDITAR_MAESTRO !== "undefined" && HE_PUEDE_EDITAR_MAESTRO;
    var editable = puedeEditar && _periodoAbierto;

    $("#tituloFueraDeHorario").text("Tramos y decisiones — " + nombre);

    var $leyenda = $("#leyendaDecisiones");
    if (!_periodoAbierto) { $leyenda.text("Período cerrado: no admite decisiones.").show(); }
    else if (!puedeEditar) { $leyenda.text("Solo lectura: su perfil puede ver los tramos pero no decidir qué se paga.").show(); }
    else { $leyenda.hide().empty(); }

    var candidatos = [];
    $.each(_fueraDeHorario.Candidatos || [], function (i, c) {
        if (String(c.IdEmpleado) === String(idEmpleado)) { candidatos.push(c); }
    });

    /* ClaveSegmento es "yyyy-MM-ddTHH:mm:ss": como texto ordena igual que como
       fecha, y asi no hay que interpretar el \/Date()\/ de JavaScriptSerializer. */
    candidatos.sort(function (a, b) {
        return a.ClaveSegmento < b.ClaveSegmento ? -1 : a.ClaveSegmento > b.ClaveSegmento ? 1 : 0;
    });

    var porGrupo = {};
    var orden = GRUPOS_DETALLE.slice(0);
    $.each(candidatos, function (i, c) {
        var g = c.Grupo || "Sin solicitar";
        if (!porGrupo[g]) { porGrupo[g] = []; if ($.inArray(g, orden) === -1) { orden.push(g); } }
        porGrupo[g].push(c);
    });

    var $cuerpo = $("#cuerpoFueraDeHorario").empty();

    if (candidatos.length === 0) {
        $cuerpo.append($('<tr><td colspan="10" class="text-center text-muted"></td></tr>')
            .find("td").text("Esta persona no tiene tramos candidatos en este período.").end());
    }

    $.each(orden, function (i, g) {
        var lista = porGrupo[g];
        if (!lista || lista.length === 0) { return; }

        $cuerpo.append($('<tr class="active"></tr>')
            .append($('<td colspan="10" style="font-weight: 600"></td>').text(g + " (" + lista.length + ")")));

        $.each(lista, function (j, c) {
            $cuerpo.append(FilaCandidato(c, editable, vh50, vh100));
        });
    });

    PintarPieDetalle(idEmpleado);

    var $modal = $("#modalFueraDeHorario");
    var $boton = $("#btnGuardarDecisiones").data("textoInicial", "Guardar decisiones");
    var $aviso = $("#fdConfirmacion");

    /* Sin nada que se pueda cambiar, el boton ni aparece: un «Guardar» que
       siempre responde «no hay cambios» solo confunde. */
    var hayEditables = false;
    $.each(candidatos, function (i, c) { if (!c.Bloqueado) { hayEditables = true; } });
    $boton.toggle(editable && hayEditables);

    /* Despues de pintar los controles: PrepararConfirmacion engancha el
       reinicio a los input/select que existen en este momento. */
    PrepararConfirmacion($modal, $boton, $aviso);

    $boton.off("click").on("click", function () {
        GuardarDecisionesDetalle(idEmpleado, nombre, $boton, $aviso);
    });

    $modal.modal("show");
}

/* Una fila de la ventana de detalle. Todo con .text() y .val(): la actividad,
   el motivo y el «bloqueado por» son texto libre o vienen de la base. */
function FilaCandidato(c, editable, vh50, vh100) {
    var bloqueado = !!c.Bloqueado;
    var habilitado = editable && !bloqueado;
    var minutosSegmento = Number(c.MinutosSegmento) || 0;
    var minutosIniciales = (c.MinutosIncluidos === null || c.MinutosIncluidos === undefined) ? "" : String(c.MinutosIncluidos);

    var $tr = $("<tr></tr>")
        .attr("data-tramo", c.IdRegDetTareas)
        .attr("data-segmento", c.ClaveSegmento)
        .data("minutosSegmento", minutosSegmento)
        .data("vh50", vh50)
        .data("vh100", vh100);
    if (c.Actividad) { $tr.attr("title", c.Actividad); }

    var $fecha = $("<td></td>").text(c.Fecha);
    if (!c.DentroDelPeriodo) {
        $fecha.append("<br />").append($('<span class="label label-default"></span>').text("Fuera del período"));
    }
    $tr.append($fecha);
    $tr.append($("<td></td>").text(c.TramoTexto));
    $tr.append($("<td></td>").text(c.ParteTexto));
    $tr.append($('<td class="text-right"></td>').text(minutosSegmento));
    $tr.append($('<td class="text-right fd-usd"></td>').text(FormatoDosDecimales(c.ValorUsd)));

    var $estado = $("<td></td>").text(c.Grupo || "");
    if (bloqueado) {
        $estado.append("<br />").append($('<small class="text-danger"></small>').text(c.BloqueadoPor || "incluido en otro período"));
    } else if (c.DecididoPor) {
        /* Quien decidio y cuando: una decision de Nomina no es anonima. */
        $estado.append("<br />").append($('<small class="text-muted"></small>')
            .text("Decidió " + c.DecididoPor + (c.DecididoEl ? " el " + FormatoFechaHora(c.DecididoEl) : "")));
    }
    $tr.append($estado);

    var $incluir = $('<input type="checkbox" class="fd-incluir" />')
        .prop("checked", !!c.Incluir)
        .prop("disabled", !habilitado);
    $tr.append($('<td class="text-center"></td>').append($incluir));

    var $tipo = $('<select class="form-control input-sm fd-tipo"></select>')
        .append($("<option></option>").val("1").text("50%"))
        .append($("<option></option>").val("2").text("100%"))
        .val(String(c.TipoElegido === 2 ? 2 : 1))
        .prop("disabled", !habilitado);
    /* Si Nomina cambio el tipo que dio el horario o la jefatura, se dice cual era. */
    if (c.TipoElegido !== c.TipoHora) { $tipo.attr("title", "Tipo original: " + (c.TipoHoraTexto || "")); }
    $tr.append($("<td></td>").append($tipo));

    /* type="text" y no "number": con basura («30m», «1e», «-») un input
       number devuelve .val() == "", que aqui significa «el segmento completo»,
       o sea pagar MAS sin que nadie lo haya pedido. Como texto, lo escrito
       llega tal cual y la regex de EstadoFilaCandidato lo rechaza. */
    var $minutos = $('<input type="text" inputmode="numeric" class="form-control input-sm fd-minutos" maxlength="6" />')
        .attr("title", "Vacío = el tramo completo (" + minutosSegmento + " min). Solo para reducir.")
        .attr("placeholder", String(minutosSegmento))
        .val(minutosIniciales)
        .prop("disabled", !habilitado);
    $tr.append($('<td style="min-width: 90px"></td>').append($minutos));

    var $motivo = $('<input type="text" class="form-control input-sm fd-motivo" maxlength="400" />')
        .val(c.Motivo || "")
        .prop("disabled", !habilitado);
    $tr.append($('<td style="min-width: 180px"></td>').append($motivo));

    /* La foto inicial, normalizada igual que lo que se enviaria: asi «cambio»
       significa lo mismo al abrir que al guardar. */
    $tr.data("inicial", EstadoFilaCandidato($tr));

    return $tr;
}

/* Lo que la fila enviaria hoy. Unos minutos iguales al segmento completo se
   envian vacios: es lo mismo -el segmento completo- y asi no exigen un motivo
   de reduccion que nadie hizo. */
function EstadoFilaCandidato($tr) {
    var minutosSegmento = Number($tr.data("minutosSegmento")) || 0;
    var minutos = $.trim(String($tr.find(".fd-minutos").val() || ""));
    /* Solo se normaliza un entero limpio: Number("1e2") tambien da 100. */
    if (/^\d+$/.test(minutos) && Number(minutos) === minutosSegmento) { minutos = ""; }

    return {
        idTramo: $tr.attr("data-tramo"),
        segmentoInicio: $tr.attr("data-segmento"),
        incluir: $tr.find(".fd-incluir").is(":checked"),
        tipoHora: parseInt($tr.find(".fd-tipo").val(), 10) === 2 ? 2 : 1,
        minutos: minutos,
        motivo: $.trim(String($tr.find(".fd-motivo").val() || ""))
    };
}

function EstadoCambio(a, b) {
    return a.incluir !== b.incluir || a.tipoHora !== b.tipoHora ||
           a.minutos !== b.minutos || a.motivo !== b.motivo;
}

/* El USD de la fila se revalora al tocar sus controles, con la misma regla que
   EntHeCandidato.MinutosMostrados: si se incluye, lo que se paga; si no, el
   segmento completo (lo que costaria incluirlo). Es vista previa: lo que manda
   es lo que devuelve el servidor al guardar. */
$(document).on("input change", "#cuerpoFueraDeHorario .fd-incluir, #cuerpoFueraDeHorario .fd-tipo, #cuerpoFueraDeHorario .fd-minutos", function () {
    var $tr = $(this).closest("tr");
    var e = EstadoFilaCandidato($tr);
    var minutosSegmento = Number($tr.data("minutosSegmento")) || 0;
    var minutos = minutosSegmento;
    if (e.incluir && e.minutos !== "") {
        /* Minutos ilegibles: no se valora nada. Mostrar el segmento completo
           haria creer que eso es lo que se va a pagar. */
        if (!/^\d+$/.test(e.minutos)) { $tr.find(".fd-usd").text("—"); return; }
        minutos = Math.min(Number(e.minutos), minutosSegmento);
    }
    var vh = e.tipoHora === 2 ? Number($tr.data("vh100")) || 0 : Number($tr.data("vh50")) || 0;
    $tr.find(".fd-usd").text(FormatoDosDecimales(RedondearDos(minutos * vh / 60)));
});

/* El resumen de la persona, de la conciliacion del servidor: que se paga
   (aprobado y no aprobado incluido), que no, y el total. */
function PintarPieDetalle(idEmpleado) {
    var c = null;
    $.each(_fueraDeHorario.Conciliacion || [], function (i, x) { if (String(x.IdEmpleado) === String(idEmpleado)) { c = x; } });

    var $pie = $("#pieFueraDeHorario").empty();
    if (!c) { return; }

    $pie.text("Incluido: aprobado " + TextoHorasUsd(c.MinutosAprobadoIncluido, c.UsdAprobadoIncluido) +
              "; no aprobado " + TextoHorasUsd(c.MinutosNoAprobadoIncluido, c.UsdNoAprobadoIncluido) +
              ". No incluido: " + TextoHorasUsd(c.MinutosExcluido, c.UsdExcluido) +
              ". Total a pagar: USD " + FormatoDosDecimales(c.TotalAPagar) +
              " (con lo no incluido: USD " + FormatoDosDecimales(c.TotalConExcluido) + ").");

    if (c.Desactualizada) {
        $pie.append($("<br />")).append($('<span class="text-warning"></span>')
            .text("Esta fila está desactualizada respecto de las decisiones: " + AccionParaActualizar() + "."));
    }
}

/* Guardar en dos clics, como completar datos y el ajuste de sueldo: el primero
   muestra a quien y cuantos tramos cambian; el segundo guarda. Cualquier cambio
   en un control vuelve al primer paso (PrepararConfirmacion), asi lo
   confirmado es siempre lo que se ve. */
function GuardarDecisionesDetalle(idEmpleado, nombre, $boton, $aviso) {
    var cambios = [];
    var minutosInvalidos = false;

    $("#cuerpoFueraDeHorario tr[data-tramo]").each(function () {
        var $tr = $(this);
        var actual = EstadoFilaCandidato($tr);
        if (!EstadoCambio(actual, $tr.data("inicial"))) { return; }
        if (actual.minutos !== "" && !/^\d+$/.test(actual.minutos)) { minutosInvalidos = true; }
        cambios.push(actual);
    });

    if (cambios.length === 0) {
        $aviso.text("No cambió ningún tramo: no hay nada que guardar.").show();
        return;
    }

    /* Letras, decimales o signos en los minutos: no se guarda nada. El
       servidor tambien lo rechaza, pero aqui se dice sin gastar una ida. */
    if (minutosInvalidos) {
        $aviso.text("Hay minutos que no son un número entero: corríjalos, o déjelos vacíos para pagar el tramo completo. No se guardó nada.").show();
        return;
    }

    /* Guardar recalcula el periodo y repinta la grilla: lo que haya sin
       guardar en ella (observaciones) se perderia sin aviso. */
    if (HayCambiosSinGuardar()) {
        $aviso.text("Hay cambios sin guardar en la grilla. Guárdelos o descártelos antes de guardar decisiones: al guardar se recalcula el período y la grilla se vuelve a pintar.").show();
        return;
    }

    if (!$boton.data("confirmado")) {
        $aviso.empty()
            .append(document.createTextNode("Vas a cambiar la decisión de " + cambios.length + " tramo(s) de "))
            .append($("<strong></strong>").text(nombre || "–"))
            .append(document.createTextNode(". Se recalculará el pago del período. "))
            .append($("<strong></strong>").text("¿Es la persona correcta?"))
            .append(document.createTextNode(" Si es así, pulse «Sí, guardar»."))
            .show();
        $boton.data("confirmado", true).text("Sí, guardar");
        return;
    }

    $boton.prop("disabled", true);   // un doble clic no guarda dos veces

    PostHE("GuardarDecisiones", {
        idPeriodo: _idPeriodoActual,
        idEmpleado: idEmpleado,
        cambios: cambios
    }, function (r) {
        $("#modalFueraDeHorario").modal("hide");
        if (r.resultado && r.resultado.Periodo) {
            PintarPantalla(r.resultado);
        } else {
            /* Las decisiones SI se guardaron pero el recalculo fallo (el mensaje
               pide «Traer aprobaciones nuevas»). Los candidatos igual cambiaron: se vuelven a
               pedir para que la ventana no muestre lo de antes. */
            CargarFueraDeHorario();
        }
    }, function () {
        ReiniciarConfirmacion($boton, $aviso);
    });
}

/* ------------------------------------------ confirmar antes de guardar -- */

/* Deja el formulario en su primer paso: botón «Guardar», sin aviso. Cualquier
   cambio en un campo vuelve a este paso, para que lo confirmado sea siempre lo
   que se ve en pantalla. */
function PrepararConfirmacion($modal, $boton, $aviso) {
    ReiniciarConfirmacion($boton, $aviso);
    $modal.find("input, select").off("input.confirmar change.confirmar")
          .on("input.confirmar change.confirmar", function () { ReiniciarConfirmacion($boton, $aviso); });
}

/* El texto del primer paso se toma de data("textoInicial") si quien abre el
   formulario lo fijo (la ventana de decisiones dice «Guardar decisiones»); los
   dos formularios de sueldo no lo fijan y siguen diciendo «Guardar». */
function ReiniciarConfirmacion($boton, $aviso) {
    $aviso.hide().empty();
    $boton.data("confirmado", false).prop("disabled", false).text($boton.data("textoInicial") || "Guardar");
}

/* Arma el aviso con el nombre en negrita, el sueldo y la fecha. Todo con
   .text(): el nombre viene de la base. */
function PedirConfirmacion($boton, $aviso, inicio, nombre, monto, vigencia) {
    var fecha = vigencia ? FechaLegible(vigencia) : "(sin fecha)";

    $aviso.empty()
        .append(document.createTextNode(inicio))
        .append($("<strong></strong>").text(nombre || "–"))
        .append(document.createTextNode(" con un sueldo de USD " + ($.trim(monto) || "(vacío)") +
                                        ", vigente desde el " + fecha + ". "))
        .append($("<strong></strong>").text("¿Es la persona correcta?"))
        .append(document.createTextNode(" Si es así, pulse «Sí, guardar»."))
        .show();

    $boton.data("confirmado", true).text("Sí, guardar");
}

/* La línea que se agrega a la confirmación de cierre. Nunca bloquea: informa. */
function TextoAvisoFueraDeHorario() {
    if (_estadoFuera !== "ok" || _idPeriodoFuera !== _idPeriodoActual) {
        return "No se pudo verificar el tiempo fuera de horario.";
    }

    var r = _resumenFuera;
    var texto = (r.personasSin === 0 && r.personasRech === 0)
        ? "No hay tiempo sin aprobar ni rechazado fuera del pago en este período."
        : "Hay " + r.personasSin + " colaborador(es) con " +
          TextoHorasUsd(r.minutosSin, r.usdSin) + " fuera de horario sin aprobar y " +
          r.personasRech + " con " + TextoHorasUsd(r.minutosRech, r.usdRech) +
          " rechazadas. Esas horas no están incluidas en el pago.";

    /* Cerrar con filas desactualizadas cierra un pago que no es el de las
       decisiones: el aviso de cierre lo repite, aunque no lo impide. */
    var desactualizadas = TextoFilasDesactualizadas();
    if (desactualizadas) { texto += " " + desactualizadas; }

    var fuera = _fueraDeHorario.FueraDelModulo || [];
    if (fuera.length > 0) {
        texto += " Además, " + fuera.length + " persona(s) con " +
                 FormatoDosDecimales(_fueraDeHorario.TotalHorasAprobadasFueraDelModulo) +
                 " h aprobadas no están en el módulo y no se les pagarán hasta completar sus datos.";
    }

    return texto;
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
        TerminarLoteDeGuardado(fallidas, avisos, lista.length);
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
   central de este punto, que antes solo pasaba por el camino feliz.

   El aviso de fallo depende del ESTADO que devuelve esa relectura, no de
   adivinar por que fallo cada fila. Si otra persona cerro el periodo a mitad
   del lote, todas las filas restantes fallan con el mismo -2 del
   procedimiento y el mensaje generico "vuelva a intentarlo" era doblemente
   falso: no decia la causa -que no es un fallo pasajero, es un cierre- y
   pedia reintentar sobre una grilla que PintarPantalla acaba de repintar en
   solo lectura, sin el boton Guardar y sin los valores digitados. Reintentar
   ahi no es posible: hace falta que un Super Admin reabra el periodo. */
function TerminarLoteDeGuardado(fallidas, avisos, total) {
    PostHE("CargarPeriodo", { idPeriodo: _idPeriodoActual }, function (r) {
        PintarPantalla(r.resultado);
        $("#btnGuardar").prop("disabled", false);

        /* El indicador "Guardado hh:mm" se actualiza siempre que el lote
           termino sin filas fallidas, avisos aparte: un aviso -sueldo
           congelado, por ejemplo- no significa que la fila no se guardo. */
        if (fallidas.length === 0) { MarcarGuardado(); }

        if (fallidas.length > 0 && r.resultado && r.resultado.Periodo &&
            !r.resultado.Periodo.EstaAbierto) {
            var guardadas = (total || fallidas.length) - fallidas.length;
            MostrarMensaje(
                "El período se cerró mientras guardaba. Alcanzaron a guardarse " +
                guardadas + " fila(s) y " + fallidas.length + " no: " +
                fallidas.join(", ") + ". La grilla volvió a sólo lectura y lo que " +
                "quedó sin guardar se perdió; para digitarlo de nuevo, un usuario " +
                "con perfil Super Admin tiene que reabrir el período.",
                "danger"
            );
        } else if (fallidas.length > 0) {
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

/* JavaScriptSerializer manda un DateTime como "\/Date(ticks)\/" -el formato
   de ASP.NET Ajax, no ISO 8601-, asi que un new Date(valor) directo sobre
   FechaCierre daria "Invalid Date". Se intenta ese formato primero y se cae a
   un parseo directo por si algun dia cambia; cualquier fecha invalida se
   muestra vacia en vez de "NaN/NaN/NaN NaN:NaN" en pantalla. */
function FormatoFechaHora(valor) {
    if (!valor) { return ""; }

    var match = /\/Date\((-?\d+)\)\//.exec(valor);
    var fecha = match ? new Date(parseInt(match[1], 10)) : new Date(valor);
    if (isNaN(fecha.getTime())) { return ""; }

    var dd = ("0" + fecha.getDate()).slice(-2);
    var mm = ("0" + (fecha.getMonth() + 1)).slice(-2);
    var hh = ("0" + fecha.getHours()).slice(-2);
    var mi = ("0" + fecha.getMinutes()).slice(-2);
    return dd + "/" + mm + "/" + fecha.getFullYear() + " " + hh + ":" + mi;
}

/* La misma fecha de FormatoFechaHora pero sin la hora, para las de un periodo
   -FechaInicio y FechaFin son DATE en la base: no tienen hora que mostrar-. */
function FormatoFecha(valor) {
    var conHora = FormatoFechaHora(valor);
    return conHora === "" ? "" : conHora.split(" ")[0];
}

/* "yyyy-MM-dd" a partir de un Date LOCAL, que es el unico formato que acepta
   el value de un input type="date". No se usa toISOString(): ese pasa antes
   por UTC, y en Ecuador (UTC-5) el dia 1 del mes a las 00:00 locales sale
   como el ultimo dia del mes anterior. */
function ISOFecha(fecha) {
    var mm = ("0" + (fecha.getMonth() + 1)).slice(-2);
    var dd = ("0" + fecha.getDate()).slice(-2);
    return fecha.getFullYear() + "-" + mm + "-" + dd;
}

/* "yyyy-MM-dd" a partir de lo que manda el servidor para FechaInicio/FechaFin.
   Parsea igual que FormatoFechaHora -el mismo /Date(...)/ o una cadena ISO- y
   despues reusa ISOFecha, para que el rango que se reenvia al recalcular sea
   byte a byte el mismo con el que se abrio el periodo. Si difiriera en un dia,
   el procedimiento lo rechazaria por solaparse consigo mismo. */
function ISODesdeValor(valor) {
    if (!valor) { return ""; }

    var match = /\/Date\((-?\d+)\)\//.exec(valor);
    var fecha = match ? new Date(parseInt(match[1], 10)) : new Date(valor);
    if (isNaN(fecha.getTime())) { return ""; }

    return ISOFecha(fecha);
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
