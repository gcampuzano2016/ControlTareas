/* ============================================================================
   Pantalla: Parámetros de cálculo de horas extras
   Handler : AdministrarParametrosHE.ashx

   El Cod_Usuario no se manda nunca: el handler lo saca de la sesión, igual que
   en horasExtras.js.

   Este archivo NO conoce ni una sola clave de HE_Parametro ni ninguna de sus
   etiquetas. Todo lo que pinta -el nombre del parámetro, si algún cálculo lo
   usa, si lo fija el Código del Trabajo- llega en la respuesta del handler,
   que a su vez lo toma de NegHeParametroPantalla. Escribir aquí la lista de
   las siete claves sería tenerla en dos sitios, y así es como una pantalla
   acaba llamando de una forma a algo que en CapaNegocio ya se renombró.
   ============================================================================ */

/* Todo el historial tal como lo devolvió el servidor la última vez. La tabla
   de arriba son las filas con EsVigente; el panel de abajo, las de la clave
   elegida. Se guarda entero para no volver al servidor solo por cambiar de
   clave en el historial: son 7 claves, no una consulta que valga una ida. */
var _filas = [];

/* La clave cuyo historial se está mostrando. Se conserva entre recargas para
   que al guardar un parámetro el panel de abajo no se vacíe justo cuando la
   persona quiere comprobar que su cambio quedó. */
var _claveHistorial = null;

/* La fila que se está editando en el modal. Es el objeto completo que mandó
   el servidor, no solo la clave: el modal muestra su etiqueta y su valor
   vigente, y releerlos de la tabla obligaría a parsear de vuelta lo que ya
   está pintado. */
var _filaEnEdicion = null;

$(document).ready(function () {
    CargarParametros();

    /* El aviso de vigencia se repinta con cada tecla en la fecha: lleva la
       fecha dentro, y un aviso con la fecha de ayer mientras se teclea la de
       mañana es peor que no tener aviso. */
    $("#edDesde").on("change input", PintarAviso);
});

/* Llama al handler con el formato [{action, parameters}], misma forma que
   PostHE en horasExtras.js.

   onError es opcional: sin él, un estado "0" o un fallo de red muestran el
   mensaje y ahí se detiene la cadena. Con él, se invoca ADEMÁS de mostrar el
   mensaje, que es lo que necesita el botón Guardar para volver a habilitarse
   -sin una rama de error explícita se quedaba deshabilitado para siempre-. */
function PostPA(action, parameters, onSuccess, onError) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);

    $.ajax({
        type: "POST",
        url: "AdministrarParametrosHE.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) {
            if (r.estado === "1") {
                if (r.mensaje) { MostrarMensaje(r.mensaje, r.tipoMensaje || "success"); }
                onSuccess(r);
            } else {
                MostrarMensaje(r.mensaje, r.tipoMensaje);
                if (onError) { onError(r); }
            }
        },
        error: function () {
            MostrarMensaje("No se pudo contactar al servidor. Intente nuevamente.", "danger");
            if (onError) { onError(null); }
        }
    });
}

/* ----------------------------------------------------------- la tabla ---- */

function CargarParametros() {
    PostPA("Listar", {}, function (r) {
        _filas = r.resultado || [];
        PintarParametros();
        PintarHistorial();
    });
}

/* Las filas vigentes, en el orden en que llegaron. Sp_RTA_HeParametrosListar
   ordena por Clave y luego por FechaVigenciaDesde descendente, así que este
   filtro conserva ese orden por clave y no hay que reordenar aquí -ni saber
   cuál es el orden "bueno", que es algo que esta pantalla no tiene por qué
   decidir-. */
function FilasVigentes() {
    var vigentes = [];
    $.each(_filas, function (i, f) { if (f.EsVigente) { vigentes.push(f); } });
    return vigentes;
}

function PintarParametros() {
    var $cuerpo = $("#cuerpoParametros").empty();
    var vigentes = FilasVigentes();

    if (vigentes.length === 0) {
        $cuerpo.append($("<tr></tr>").append(
            $('<td colspan="6" class="text-center text-muted"></td>')
                .text("No hay ningún parámetro cargado.")));
        return;
    }

    $.each(vigentes, function (i, f) {
        var $fila = $("<tr></tr>").attr("data-clave", f.Clave);

        if (!f.Activo) { $fila.addClass("pa-fila-inactiva"); }
        if (f.Clave === _claveHistorial) { $fila.addClass("pa-fila-seleccionada"); }

        /* .text() y no .html() en todo lo que viene del servidor. Hoy son
           etiquetas fijas de CapaNegocio, pero Usu_Modificacion sale de la
           sesión de quien guardó y la clave puede haberla metido alguien a
           mano en la tabla: las dos son texto de fuera. */
        var $etiqueta = $("<td></td>")
            .append($("<strong></strong>").text(f.Etiqueta))
            .append($('<span class="pa-clave"></span>').text(f.Clave));

        /* Las dos marcas. Ninguna de las dos decide nada aquí: FijadoPorLey y
           Activo llegan resueltos del handler. */
        if (f.FijadoPorLey) {
            $etiqueta.append($('<span class="pa-marca pa-marca-ley"></span>')
                .text("Fijado por el Código del Trabajo"));
        }

        if (!f.Activo) {
            $etiqueta.append($('<span class="pa-marca pa-marca-inactivo"></span>')
                .text("Cargado, todavía sin uso en el cálculo"));
        }

        $fila.append($etiqueta);
        $fila.append($('<td class="pa-col-valor pa-valor"></td>').text(FormatoValor(f.Valor)));
        $fila.append($("<td></td>").text(FormatoFecha(f.FechaVigenciaDesde)));
        $fila.append($("<td></td>").text(f.Usu_Modificacion || "—"));
        $fila.append($("<td></td>").text(FormatoFechaHora(f.Fec_Modificacion) || "—"));

        var $acciones = $('<td class="text-right"></td>');

        $acciones.append($('<button type="button" class="btn btn-default btn-xs"></button>')
            .html('<i class="fa fa-history"></i> Ver historial')
            .on("click", function () { VerHistorial(f.Clave); }));

        $acciones.append(" ");

        $acciones.append($('<button type="button" class="btn btn-primary btn-xs"></button>')
            .html('<i class="fa fa-pencil"></i> Editar')
            .on("click", function () { AbrirEdicion(f.Clave); }));

        $fila.append($acciones);
        $cuerpo.append($fila);
    });
}

/* -------------------------------------------------------- el historial --- */

function VerHistorial(clave) {
    _claveHistorial = clave;
    PintarParametros();
    PintarHistorial();
}

function PintarHistorial() {
    var $cuerpo = $("#cuerpoHistorial").empty();

    if (!_claveHistorial) {
        $("#lblClaveHistorial").text("—");
        $cuerpo.append($("<tr></tr>").append(
            $('<td colspan="5" class="text-center pa-historial-vacio"></td>')
                .text("Elija «Ver historial» en un parámetro para ver sus versiones.")));
        return;
    }

    var versiones = [];
    $.each(_filas, function (i, f) { if (f.Clave === _claveHistorial) { versiones.push(f); } });

    /* La etiqueta sale de la primera versión que haya, no de una tabla local:
       todas las versiones de una clave comparten etiqueta porque la resuelve
       el handler con EtiquetaDe. */
    $("#lblClaveHistorial").text(versiones.length > 0 ? versiones[0].Etiqueta : _claveHistorial);

    if (versiones.length === 0) {
        $cuerpo.append($("<tr></tr>").append(
            $('<td colspan="5" class="text-center pa-historial-vacio"></td>')
                .text("Este parámetro no tiene versiones guardadas.")));
        return;
    }

    $.each(versiones, function (i, f) {
        var $fila = $("<tr></tr>");
        if (f.EsVigente) { $fila.addClass("pa-fila-vigente"); }

        $fila.append($('<td class="pa-col-valor"></td>').text(FormatoValor(f.Valor)));
        $fila.append($("<td></td>").text(FormatoFecha(f.FechaVigenciaDesde)));
        /* «Vigente» y no una celda vacía: FechaVigenciaHasta NULL significa
           que sigue abierta, y una celda en blanco se lee como un dato que
           falta. */
        $fila.append($("<td></td>").text(f.EsVigente ? "Vigente" : FormatoFecha(f.FechaVigenciaHasta)));
        $fila.append($("<td></td>").text(f.Usu_Modificacion || "—"));
        $fila.append($("<td></td>").text(FormatoFechaHora(f.Fec_Modificacion) || "—"));

        $cuerpo.append($fila);
    });
}

/* ---------------------------------------------------------- la edición --- */

function AbrirEdicion(clave) {
    var fila = null;
    $.each(FilasVigentes(), function (i, f) { if (f.Clave === clave) { fila = f; return false; } });

    if (!fila) {
        MostrarMensaje("No se encontró la versión vigente de ese parámetro. Recargue la pantalla.", "warning");
        return;
    }

    _filaEnEdicion = fila;

    $("#edEtiqueta").text(fila.Etiqueta);
    $("#edClave").text(fila.Clave);
    $("#edValorActual").text(FormatoValor(fila.Valor));
    $("#edValorNuevo").val(FormatoValor(fila.Valor));

    /* La fecha propuesta es mañana y no hoy: Sp_RTA_HeParametroGuardar exige
       que la fecha nueva sea POSTERIOR a la de la versión vigente (-4), y si
       alguien ya cambió el mismo parámetro hoy, proponer hoy garantiza el
       rechazo. Mañana es además lo que casi siempre se quiere: un cambio que
       empieza a regir a partir de ya, sin tocar lo de hoy. */
    var manana = new Date();
    manana.setDate(manana.getDate() + 1);
    $("#edDesde").val(ISOFecha(manana));

    PintarAviso();
    $("#btnGuardarParametro").prop("disabled", false);
    $("#modalEditarParametro").modal("show");
}

/* El aviso que exige la fase: dice exactamente qué se recalcula y qué no.

   Va dentro del formulario y se repite en la confirmación, a propósito. En el
   formulario para que se lea mientras se elige la fecha -es la fecha la que
   decide qué períodos entran- y en la confirmación porque es el último punto
   en el que todavía se puede no hacerlo. */
function TextoAviso(fechaISO) {
    return "Este cambio no altera los períodos ya cerrados. " +
           "Los períodos abiertos que se recalculen con fecha de fin posterior al " +
           FormatoFechaISO(fechaISO) + " usarán el valor nuevo.";
}

function PintarAviso() {
    $("#edAviso").text(TextoAviso($("#edDesde").val()));
}

/* Las tres comprobaciones de aquí las repite el servidor -y por debajo,
   Sp_RTA_HeParametroGuardar, que es quien manda-. Se hacen igual para no
   gastar una ida al servidor en algo que se ve desde el navegador, y para
   poder decir qué campo falta en vez de "no se pudo guardar". */
function ConfirmarGuardar() {
    if (!_filaEnEdicion) { return; }

    var valor = NumeroDe($("#edValorNuevo").val());
    var desde = $("#edDesde").val();

    if (valor <= 0) {
        MostrarMensaje("El valor tiene que ser mayor que cero.", "warning");
        return;
    }

    if (!desde) {
        MostrarMensaje("Indique desde qué fecha rige el valor nuevo.", "warning");
        return;
    }

    /* Comparación de texto y no de Date: en "yyyy-MM-dd" el orden alfabético
       ES el cronológico, y así no hay que construir dos Date -que sobre una
       fecha sin hora se interpretan como UTC y pueden correrse un día contra
       la hora local-. El servidor lo vuelve a comprobar con el código -4. */
    var desdeVigente = ISODesdeValor(_filaEnEdicion.FechaVigenciaDesde);

    if (desdeVigente && desde <= desdeVigente) {
        MostrarMensaje("La fecha nueva tiene que ser posterior al " +
                       FormatoFecha(_filaEnEdicion.FechaVigenciaDesde) +
                       ", que es desde cuándo rige el valor actual.", "warning");
        return;
    }

    var $cuerpo = $("<div></div>");

    $cuerpo.append($("<p></p>")
        .append(document.createTextNode("Va a cambiar "))
        .append($("<strong></strong>").text(_filaEnEdicion.Etiqueta))
        .append(document.createTextNode(" de " + FormatoValor(_filaEnEdicion.Valor) +
                                        " a " + FormatoValor(valor) + ", desde el "))
        .append($("<strong></strong>").text(FormatoFechaISO(desde)))
        .append(document.createTextNode(".")));

    if (_filaEnEdicion.FijadoPorLey) {
        $cuerpo.append($('<p class="text-danger"></p>')
            .text("Este valor lo fija el Código del Trabajo. Cambiarlo hace que la " +
                  "nómina deje de calcularse como manda la ley."));
    }

    if (!_filaEnEdicion.Activo) {
        $cuerpo.append($('<p class="text-muted"></p>')
            .text("Este parámetro está cargado pero ningún cálculo lo usa todavía: " +
                  "cambiarlo no altera ningún monto."));
    }

    $cuerpo.append($("<p></p>").text(TextoAviso(desde)));

    var clave = _filaEnEdicion.Clave;
    var confirmado = false;

    /* Los dos modales NO se apilan: se cierra el de edición y recién cuando
       terminó de cerrarse se abre el de confirmación. Bootstrap 3 no soporta
       modales encima de modales -el segundo en cerrarse se lleva el backdrop
       del primero y la pantalla queda con el velo gris puesto y sin nada que
       lo quite, con el scroll bloqueado-. Y se espera al evento hidden en vez
       de encadenarlos: llamar a show() mientras el otro todavía se está
       cerrando deja igualmente el backdrop huérfano.

       Si la persona no confirma, el de edición se vuelve a abrir con lo que
       había tecleado: nunca se limpian los campos, así que basta con
       mostrarlo otra vez. Perder el valor recién escrito por decir "mejor no"
       en la confirmación sería castigar justo a quien se detuvo a pensarlo. */
    $("#modalConfirmar").one("hidden.bs.modal", function () {
        if (!confirmado) { $("#modalEditarParametro").modal("show"); }
    });

    $("#modalEditarParametro").one("hidden.bs.modal", function () {
        MostrarConfirmacion($cuerpo, function () {
            confirmado = true;
            GuardarParametro(clave, valor, desde);
        }, "Confirmar el cambio", "Sí, guardar");
    });

    $("#modalEditarParametro").modal("hide");
}

function GuardarParametro(clave, valor, desde) {
    /* Sin deshabilitar el botón, un doble clic manda dos veces la misma
       versión: la primera la guarda y la segunda la rechaza con el -3 de
       "ya existe una versión con esa misma fecha", y quien la hizo se lleva
       un error por algo que en realidad sí funcionó. */
    $("#btnGuardarParametro").prop("disabled", true);

    PostPA("Guardar", { clave: clave, valor: String(valor), desde: desde }, function (r) {
        $("#btnGuardarParametro").prop("disabled", false);
        _filaEnEdicion = null;

        /* El historial de abajo pasa a ser el de la clave que se acaba de
           cambiar: es donde se ve que quedaron dos versiones y que la
           anterior se cerró sola. Sin esto, el único rastro visible del
           cambio es un número distinto en la tabla de arriba, que es
           exactamente lo que esta pantalla vino a dejar de ser. */
        _claveHistorial = clave;

        /* Guardar devuelve el historial ya recargado, así que la pantalla se
           repinta con lo que de verdad quedó en la base y no con lo que el
           navegador cree que mandó. Si por lo que fuera no viniera, se pide
           otra vez antes que dejar la tabla mintiendo. */
        if (r.resultado) {
            _filas = r.resultado;
            PintarParametros();
            PintarHistorial();
        } else {
            CargarParametros();
        }
    }, function () {
        /* El modal de edición ya se cerró para dar paso al de confirmación,
           así que un rechazo del servidor -el -4 de la fecha, el -5 de los
           decimales- dejaría a la persona mirando la tabla sin forma de
           corregir lo que acababa de escribir. Se vuelve a abrir con los
           campos tal como los dejó.

           Se espera al hidden del modal informativo por lo mismo que en el
           encadenado de arriba: PostPA acaba de abrirlo con el mensaje del
           error, y abrir el de edición encima deja el velo gris pegado. */
        $("#btnGuardarParametro").prop("disabled", false);

        $("#modalMensajeInformativo").one("hidden.bs.modal", function () {
            $("#modalEditarParametro").modal("show");
        });
    });
}

/* ------------------------------------------------------------ utilidades - */

/* El valor se muestra tal cual vino, sin forzar decimales: Factor50 es 1.5 y
   DiasMes es 30, y pintar "30.00" días o "2.00" decimales hace que un entero
   parezca un importe. toString() de un número de JavaScript ya quita los
   ceros de la derecha. */
function FormatoValor(valor) {
    var n = NumeroDe(valor);
    return String(n);
}

function NumeroDe(texto) {
    var n = parseFloat(String(texto == null ? "" : texto).replace(",", "."));
    return isNaN(n) ? 0 : n;
}

/* Copiada de horasExtras.js. Cada pantalla lleva su propia copia de estas
   utilidades; no hay una biblioteca compartida en este proyecto. */
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

function FormatoFecha(valor) {
    var conHora = FormatoFechaHora(valor);
    return conHora === "" ? "" : conHora.split(" ")[0];
}

/* "dd/MM/yyyy" a partir del "yyyy-MM-dd" que devuelve un input type="date".
   Se parte el texto en vez de construir un Date: un Date sobre una fecha sin
   hora se interpreta como UTC, y en Ecuador (UTC-5) el aviso saldría con el
   día anterior al elegido. Justo el aviso que dice desde cuándo rige. */
function FormatoFechaISO(iso) {
    if (!iso) { return "__/__/____"; }

    var partes = String(iso).split("-");
    if (partes.length !== 3) { return String(iso); }

    return partes[2] + "/" + partes[1] + "/" + partes[0];
}

/* "yyyy-MM-dd" a partir de un Date LOCAL, que es el único formato que acepta
   el value de un input type="date". No se usa toISOString(): ese pasa antes
   por UTC, y en Ecuador (UTC-5) las primeras horas del día salen como el día
   anterior. */
function ISOFecha(fecha) {
    var mm = ("0" + (fecha.getMonth() + 1)).slice(-2);
    var dd = ("0" + fecha.getDate()).slice(-2);
    return fecha.getFullYear() + "-" + mm + "-" + dd;
}

/* "yyyy-MM-dd" a partir de lo que manda el servidor, para poder comparar dos
   fechas como texto. */
function ISODesdeValor(valor) {
    if (!valor) { return ""; }

    var match = /\/Date\((-?\d+)\)\//.exec(valor);
    var fecha = match ? new Date(parseInt(match[1], 10)) : new Date(valor);
    if (isNaN(fecha.getTime())) { return ""; }

    return ISOFecha(fecha);
}

/* Copiada del patrón de la casa, con .text() y no .html(): hoy todo lo que
   llega aquí es texto plano, pero .html() es la puerta por la que entraría
   una inyección el día que un mensaje incluyera algo escrito por alguien. */
function MostrarMensaje(mensaje, tipo) {
    var color = "#fcf8e3";
    if (tipo == "success") { color = "#dff0d8"; }
    if (tipo == "danger") { color = "#f2dede"; }
    if (tipo == "warning") { color = "#fcf8e3"; }

    $("#modalMensajeInformativoTipo").css("background", color);
    $("#MensajeInformativo").text(mensaje);
    $("#modalMensajeInformativo").modal("show");
}

/* Misma forma que en horasExtras.js: admite texto plano o un fragmento de
   jQuery, y le cambia el título y el botón al modal genérico. Aquí se usa con
   un fragmento porque el aviso de vigencia lleva la fecha en negrita y hasta
   tres párrafos, que un confirm() del navegador no puede mostrar. */
function MostrarConfirmacion(contenido, alConfirmar, titulo, textoBoton) {
    var $cuerpo = $("#textoConfirmar").empty();

    if (typeof contenido === "string") { $cuerpo.text(contenido); }
    else { $cuerpo.append(contenido); }

    $("#modalConfirmarLabel").text(titulo || "Confirmar");
    $("#btnConfirmar").text(textoBoton || "Sí, guardar");

    $("#btnConfirmar").off("click").on("click", function () {
        $("#modalConfirmar").modal("hide");
        alConfirmar();
    });

    $("#modalConfirmar").modal("show");
}
