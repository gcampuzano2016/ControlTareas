/* ============================================================================
   Pad de firma.

   La especificacion decia que este componente ya existia en la plataforma de
   Capacitados y que solo habia que reutilizarlo. En este repositorio no hay
   nada de eso: lo unico parecido es html2canvas capturando pantallas, que no es
   un pad de firma. Asi que se construye aca.

   Lo usan las tres firmas de cada solicitud (colaborador, jefe, Talento Humano)
   en Vacaciones y en Permisos. Por eso recibe el contenedor: se instancia una
   vez por lienzo, no una por pantalla.

   Uso:
       var pad = PadFirma("divFirmaJefe");
       ...
       pad.obtenerTrazo();   // data URI, o cadena vacia si no firmo
       pad.estaVacio();
       pad.limpiar();
   ============================================================================ */

/* Clave donde se recuerda la firma del turno.

   Se usa sessionStorage y no localStorage a proposito: en una maquina compartida
   —y en esta empresa las hay— localStorage dejaria la firma de una persona
   disponible para la siguiente que se siente ahi. sessionStorage muere al cerrar
   la pestana.

   Se agrega ademas el token de sesion a la clave, para que dos cuentas abiertas
   en el mismo navegador nunca se ofrezcan la firma de la otra. */
function FirmaRecordadaClave() {
    var token = $("#ContentPlaceHolder1_txtUsuario").val() || "anon";
    return "firmaTurno_" + token;
}

function FirmaRecordadaGuardar(dataUri) {
    try { sessionStorage.setItem(FirmaRecordadaClave(), dataUri); }
    catch (e) { /* modo privado o almacenamiento lleno: se sigue sin recordar */ }
}

function FirmaRecordadaLeer() {
    try { return sessionStorage.getItem(FirmaRecordadaClave()) || ""; }
    catch (e) { return ""; }
}

function PadFirma(idContenedor, opciones) {
    opciones = opciones || {};

    var $cont = $("#" + idContenedor);
    if ($cont.length === 0) { return null; }

    var ancho = opciones.ancho || 420;
    var alto = opciones.alto || 150;
    var idBase = idContenedor + "_pf";

    /* El trazo se guarda en el canvas, tanto lo dibujado como la imagen subida:
       asi la salida es siempre la misma sin importar por donde entro. */
    $cont.html(
        '<div class="pad-firma">' +
          '<ul class="nav nav-tabs" style="margin-bottom:8px">' +
            '<li class="active"><a href="#' + idBase + '_dib" data-toggle="tab">Dibujar</a></li>' +
            '<li><a href="#' + idBase + '_sub" data-toggle="tab">Subir archivo</a></li>' +
          '</ul>' +
          '<div class="tab-content">' +
            '<div class="tab-pane active" id="' + idBase + '_dib">' +
              '<canvas id="' + idBase + '_canvas" width="' + ancho + '" height="' + alto + '" ' +
                 'style="border:1px solid #ccc; background:#fff; touch-action:none; cursor:crosshair; max-width:100%"></canvas>' +
            '</div>' +
            '<div class="tab-pane" id="' + idBase + '_sub" style="padding-top:10px">' +
              '<input type="file" id="' + idBase + '_file" accept="image/png,image/jpeg">' +
              '<p class="help-block">Una imagen de su firma. Se ajusta al recuadro sin recortarla.</p>' +
            '</div>' +
          '</div>' +
          '<div style="margin-top:8px">' +
            '<button type="button" class="btn btn-default btn-sm" id="' + idBase + '_limpiar">Limpiar</button>' +
            '<button type="button" class="btn btn-info btn-sm" style="margin-left:6px; display:none" id="' + idBase + '_usar">Usar mi firma</button>' +
            '<span class="help-block" style="display:inline-block; margin:0 0 0 10px" id="' + idBase + '_estado">Sin firmar</span>' +
          '</div>' +
        '</div>'
    );

    var canvas = document.getElementById(idBase + "_canvas");
    var ctx = canvas.getContext("2d");
    var dibujando = false;
    var hayTrazo = false;

    ctx.lineWidth = 2;
    ctx.lineCap = "round";
    ctx.lineJoin = "round";
    ctx.strokeStyle = "#1a1a1a";

    function estado(texto) { $("#" + idBase + "_estado").text(texto); }

    /* El canvas puede estar escalado por CSS (max-width en pantallas chicas), asi
       que la posicion del puntero se convierte a coordenadas del lienzo. Sin esto
       el trazo se dibuja corrido respecto del cursor. */
    function punto(e) {
        var r = canvas.getBoundingClientRect();
        var fuente = (e.touches && e.touches.length) ? e.touches[0] : e;
        return {
            x: (fuente.clientX - r.left) * (canvas.width / r.width),
            y: (fuente.clientY - r.top) * (canvas.height / r.height)
        };
    }

    function iniciar(e) {
        e.preventDefault();
        dibujando = true;
        var p = punto(e);
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        /* Un punto suelto tambien cuenta como firma: hay gente que solo marca. */
        ctx.lineTo(p.x, p.y);
        ctx.stroke();
        hayTrazo = true;
        estado("Firmado");
    }

    function mover(e) {
        if (!dibujando) { return; }
        e.preventDefault();
        var p = punto(e);
        ctx.lineTo(p.x, p.y);
        ctx.stroke();
    }

    function terminar() { dibujando = false; }

    canvas.addEventListener("mousedown", iniciar);
    canvas.addEventListener("mousemove", mover);
    canvas.addEventListener("mouseup", terminar);
    /* Si el puntero sale del lienzo con el boton apretado, el trazo se corta ahi
       en vez de reaparecer al volver a entrar. */
    canvas.addEventListener("mouseleave", terminar);
    canvas.addEventListener("touchstart", iniciar);
    canvas.addEventListener("touchmove", mover);
    canvas.addEventListener("touchend", terminar);

    function limpiar() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        hayTrazo = false;
        $("#" + idBase + "_file").val("");
        estado("Sin firmar");
        AjustarBotonRecordada();
    }

    $("#" + idBase + "_limpiar").on("click", limpiar);

    $("#" + idBase + "_file").on("change", function () {
        var archivo = this.files && this.files[0];
        if (!archivo) { return; }

        if (archivo.size > 2 * 1024 * 1024) {
            estado("La imagen pesa más de 2 MB. Use una más liviana.");
            this.value = "";
            return;
        }

        var lector = new FileReader();
        lector.onload = function (ev) {
            var img = new Image();
            img.onload = function () {
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                /* Se ajusta sin deformar ni recortar: una firma estirada no sirve. */
                var escala = Math.min(canvas.width / img.width, canvas.height / img.height);
                var w = img.width * escala;
                var h = img.height * escala;
                ctx.drawImage(img, (canvas.width - w) / 2, (canvas.height - h) / 2, w, h);
                hayTrazo = true;
                estado("Firmado (imagen)");
            };
            img.onerror = function () { estado("No se pudo leer la imagen."); };
            img.src = ev.target.result;
        };
        lector.readAsDataURL(archivo);
    });

    /* Pinta en el lienzo un data URI ya existente. */
    function cargar(dataUri, etiqueta) {
        var img = new Image();
        img.onload = function () {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            var escala = Math.min(canvas.width / img.width, canvas.height / img.height);
            var w = img.width * escala;
            var h = img.height * escala;
            ctx.drawImage(img, (canvas.width - w) / 2, (canvas.height - h) / 2, w, h);
            hayTrazo = true;
            estado(etiqueta);
        };
        img.src = dataUri;
    }

    /* La firma del turno: se dibuja una vez y se reusa en las demas aprobaciones.
       Aprobar veinte solicitudes no puede significar dibujar veinte veces, o la
       gente termina garabateando cualquier cosa y la firma deja de valer.

       Lo que se reusa es el trazo, no la aprobacion: cada firma guarda igual su
       propia fecha, IP, dispositivo y decision. */
    $("#" + idBase + "_usar").on("click", function () {
        cargar(FirmaRecordadaLeer(), "Firmado (su firma guardada)");
    });

    /* Se revisa cada vez que el pad se reabre y no solo al construirlo: el pad
       se arma una unica vez por pantalla, asi que si la firma se recordo despues
       —firmo primero en Vacaciones y despues abrio Permisos, o al reves— el boton
       nunca aparecia y la firma reusable no servia de nada. */
    function AjustarBotonRecordada() {
        var $b = $("#" + idBase + "_usar");
        if (FirmaRecordadaLeer() !== "") { $b.show(); } else { $b.hide(); }
    }

    AjustarBotonRecordada();

    return {
        /* Data URI listo para viajar al servidor, o cadena vacia si no hay firma.
           Se devuelve vacio en vez del canvas en blanco a proposito: asi quien
           llama puede exigir firma sin tener que inspeccionar pixeles. */
        obtenerTrazo: function () {
            if (!hayTrazo) { return ""; }
            var dataUri = canvas.toDataURL("image/png");
            /* Se recuerda recien al usarla, no al dibujarla: un trazo que se
               descarto sin enviar no deberia quedar como la firma del turno. */
            FirmaRecordadaGuardar(dataUri);
            return dataUri;
        },
        estaVacio: function () { return !hayTrazo; },
        limpiar: limpiar,
        cargar: cargar
    };
}
