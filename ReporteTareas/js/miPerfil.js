/* ============================================================================
   Pantalla: Mi perfil
   Handler : AdministrarPerfil.ashx

   Con _codObjetivo vacio (el caso de "Mi perfil") el Cod_Usuario no se manda
   nunca: el handler lo saca de la sesion. Cuando Talento Humano abre el perfil
   de otra persona (ver FijarPerfilObjetivo, mas abajo), _codObjetivo viaja en
   cada llamada; NegPerfilAcceso en el servidor decide si se acepta.
   ============================================================================ */

var _perfil = null;

/* De quien es el perfil que esta pantalla esta mostrando.

   Vacia en "Mi perfil": entonces no se manda nada y el servidor usa el de la
   sesion, exactamente como antes de que esto existiera. La fija el buscador de
   PerfilesPersonal.aspx al elegir a una persona.

   Que este valor viaje NO significa que el servidor lo acepte: la regla que
   decide eso es NegPerfilAcceso, y rechaza a quien no sea perfil 14 o 18. */
var _codObjetivo = "";

$(document).ready(function () {
    CargarPerfil();
});

/* De quien es el perfil que esta pantalla va a mostrar, y deja el enlace del
   CV apuntando a esa persona.

   Los cuatro caminos por los que el codigo del perfil sale al servidor.
   El primero cubre las 16 acciones JSON de una sola vez; los otros tres NO
   pasan por PostPerfil y por eso hay que acordarse de ellos uno por uno:
     1. PostPerfil          -> lo agrega a parameters
     2. PedirArchivo        -> lo agrega al FormData (multipart)
     3. CeldaDocumentos     -> lo agrega a la URL de descarga del documento
     4. el enlace del CV    -> se le reescribe el href, porque es marcado
                               estatico y ningun codigo lo tocaba antes
   Si aparece un quinto, va en esta lista. */
function FijarPerfilObjetivo(codUsuario, nombre) {
    _codObjetivo = codUsuario || "";

    var enlace = $("#lnkHojaVida");
    if (enlace.length) {
        enlace.attr("href", "DescargarPerfil.ashx?cv=1" +
                            (_codObjetivo === "" ? "" : "&u=" + encodeURIComponent(_codObjetivo)));
        $("#txtHojaVida").text(_codObjetivo === ""
            ? "Descargar mi hoja de vida"
            : "Descargar la hoja de vida de " + (nombre || "esta persona"));
    }

    CargarPerfil();
}

/* Llama al handler con el formato [{action, parameters}] */
function PostPerfil(action, parameters, onSuccess) {
    /* El codigo del perfil viaja en TODAS las llamadas, incluidas las del
       propio perfil, donde va vacio. Un solo camino es mas facil de revisar que
       una excepcion por accion. */
    var p = parameters || {};
    if (_codObjetivo !== "") { p.codUsuario = _codObjetivo; }

    var datos = JSON.stringify([{ "action": action, "parameters": p }]);

    $.ajax({
        type: "POST",
        url: "AdministrarPerfil.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) {
            onSuccess(respuesta);
        },
        error: function () {
            MostrarMensaje("La operación está tomando demasiado tiempo o la red está saturada. Intente nuevamente.", "danger");
        }
    });
}

function CargarPerfil() {
    PostPerfil("CargarPerfil", {}, function (respuesta) {
        // Un objeto con "estado" es un EntRespuesta, es decir, un error.
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        _perfil = respuesta;

        /* Cero filas de cabecera no es "no tiene datos": es que su codigo de usuario
           esta repetido en R_Usuarios y el procedimiento se nego a adivinar cual de
           las dos personas es. Mostrar la pantalla vacia seria peor que no mostrarla:
           pareceria que el perfil no tiene nada, cuando el problema es de identidad. */
        if (!respuesta.PerfilEncontrado) {
            $("#perfilNoIdentificado").show();
            $(".nav-tabs, .tab-content").hide();
            return;
        }

        PintarCabecera(respuesta.Cabecera);
        PintarContacto(respuesta.Contacto);
        PintarEmergencia(respuesta.Emergencia);
        PintarEstudios(respuesta.Estudios);
        PintarCertificaciones(respuesta.Certificaciones);
        PintarExperiencia(respuesta.Experiencia);
        PintarCargasFamiliares(respuesta.CargasFamiliares);
        PintarFoto(respuesta.Foto);
        MostrarPestanaEquipo(respuesta.Cabecera.EsJefe);
    });
}

function PintarCabecera(c) {
    $("#perfilNombre").text(c.NombreCompleto || "–");
    $("#perfilCargo").text(c.Cargo || "–");
    $("#perfilArea").text(c.Area || "–");
    $("#perfilCiudad").text(c.Ciudad || "–");
    $("#perfilAvatar").text(Iniciales(c.NombreCompleto));

    // Edad null significa "no se sabe": guion, nunca cero.
    $("#perfilEdad").text(c.Edad === null ? "–" : c.Edad + " años");

    $("#dpNombre").text(c.NombreCompleto || "–");
    $("#dpCedula").text(c.Cedula || "–");
    $("#dpFnac").text(c.FechaNacTexto || "–");
    $("#dpCargo").text(c.Cargo || "–");
    $("#dpArea").text(c.Area || "–");
    $("#dpJefe").text(c.JefeInmediato || "–");
    $("#dpCiudad").text(c.Ciudad || "–");
    $("#dpCorreo").text(c.CorreoNotificacion || "–");
    // 143 de 231 no tienen horario asignado todavia: guion, no cadena vacia.
    $("#dpHorario").text(c.Horario || "–");

    if (!c.TieneFicha) {
        $("#perfilSinFicha").show();
    }
}

/* La foto y las iniciales son excluyentes: si hay foto, las iniciales sobran.
   Esta funcion corre despues de PintarCabecera, que es la que escribe las
   iniciales, asi que el orden de las dos llamadas importa. */
function PintarFoto(foto) {
    if (foto && foto.DataUri) {
        $("#perfilFoto").attr("src", foto.DataUri).show();
        $("#perfilAvatar").hide();
        $("#btnQuitarFoto").show();
    } else {
        $("#perfilFoto").hide().removeAttr("src");
        $("#perfilAvatar").show();
        $("#btnQuitarFoto").hide();
    }
}

function PintarContacto(c) {
    $("#inDireccion").val(c.Direccion || "");
    $("#inCorreoPersonal").val(c.CorreoPersonal || "");
    $("#inTelefonoPersonal").val(c.TelefonoPersonal || "");

    /* En produccion conviven varias capitalizaciones del mismo estado civil
       ("soltero/a", "Soltero", "SOLTERO/A"...) porque distintas personas de
       Talento Humano lo escribieron a mano con los anios. $.val(x) compara
       tal cual, con mayusculas y minusculas, asi que la mayoria de esos
       valores no calzaba con ninguna opcion del combo y se veia "Seleccione…"
       como si el dato no existiera. Se busca la opcion ignorando mayusculas
       y espacios en vez de forzar el valor crudo. */
    var estadoCivil = c.EstadoCivil || "";
    var $estadoCivil = $("#inEstadoCivil");
    var normalizado = estadoCivil.trim().toUpperCase();
    var $opcionQueCalza = null;

    $estadoCivil.find("option").each(function () {
        if ($(this).text().trim().toUpperCase() === normalizado) {
            $opcionQueCalza = $(this);
            return false;
        }
    });

    if (normalizado === "") {
        $estadoCivil.val("");
        $("#notaEstadoCivilValorSinCalzar").hide();
    } else if ($opcionQueCalza) {
        $estadoCivil.val($opcionQueCalza.val());
        $("#notaEstadoCivilValorSinCalzar").hide();
    } else {
        /* Ninguna opcion calza (por ejemplo "NN" o cualquier variante rara):
           se deja "Seleccione…" pero no se pierde el dato. Si se forzara aca
           una opcion del combo y la persona guardara, se sobrescribiria en
           Empleados.EstadoCivil -de donde lo lee el modulo medico- con una
           tercera capitalizacion inventada por esta pantalla. */
        $estadoCivil.val("");
        $("#notaEstadoCivilValorSinCalzar")
            .text("Valor registrado por Talento Humano: " + estadoCivil)
            .show();
    }

    /* Sin ficha de Talento Humano enlazada, el procedimiento no tiene donde
       escribir el estado civil: el UPDATE afecta cero filas. Se deshabilita
       el campo y se explica antes de que la persona lo llene, en vez de
       dejar que lo guarde y descubra despues -al recargar y verlo vacio-
       que no sirvio de nada. Los otros tres campos si se guardan para
       todo el mundo y quedan habilitados. */
    var tieneFicha = _perfil && _perfil.Cabecera && _perfil.Cabecera.TieneFicha;
    $("#inEstadoCivil").prop("disabled", !tieneFicha);
    $("#notaEstadoCivilSinFicha").toggle(!tieneFicha);
}

function GuardarContacto() {
    /* La comprobacion real esta en el servidor -el handler es alcanzable por
       HTTP directo-, pero esta evita una peticion inutil cuando ya se sabe
       que el codigo de usuario esta repetido: PerfilEncontrado en falso es
       exactamente ese caso (ver CargarPerfil). */
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.", "warning");
        return;
    }

    var datos = {
        correoPersonal:   $("#inCorreoPersonal").val(),
        telefonoPersonal: $("#inTelefonoPersonal").val(),
        direccion:        $("#inDireccion").val(),
        estadoCivil:      $("#inEstadoCivil").val()
    };

    PostPerfil("GuardarContacto", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
    });
}

function PintarEmergencia(lista) {
    var $cuerpo = $("#cuerpoEmergencia").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="4" class="text-center text-muted">' +
                       'Todavía no ha registrado ningún contacto de emergencia.</td></tr>');
        return;
    }

    $.each(lista, function (i, c) {
        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(c.Nombre));
        $fila.append($("<td></td>").text(c.Parentesco));
        $fila.append($("<td></td>").text(c.Telefono));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarEmergencia(' + c.IdContacto + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarEmergencia() {
    var datos = {
        idContacto: 0,
        nombre:     $("#emNombre").val(),
        parentesco: $("#emParentesco").val(),
        telefono:   $("#emTelefono").val()
    };

    PostPerfil("GuardarEmergencia", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#emNombre, #emParentesco, #emTelefono").val("");
            CargarPerfil();
        }
    });
}

function EliminarEmergencia(idContacto) {
    PostPerfil("EliminarEmergencia", { idContacto: idContacto }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}

/* Los nombres de instituciones y titulos los teclea el propio usuario, asi que
   todo entra con .text(). Nunca concatenando HTML. */
function PintarEstudios(lista) {
    var $cuerpo = $("#cuerpoEstudios").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="5" class="text-center text-muted">' +
                       'Todavía no ha registrado ningún estudio.</td></tr>');
        return;
    }

    $.each(lista, function (i, e) {
        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(e.Nivel));
        $fila.append($("<td></td>").text(e.Institucion));
        $fila.append($("<td></td>").text(e.Titulo));
        $fila.append($("<td></td>").text(e.AnioGraduacion === null ? "–" : e.AnioGraduacion));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarEstudio(' + e.IdEstudio + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarEstudio() {
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.", "warning");
        return;
    }

    var datos = {
        idEstudio:      0,
        nivel:          $("#esNivel").val(),
        institucion:    $("#esInstitucion").val(),
        titulo:         $("#esTitulo").val(),
        anioGraduacion: $("#esAnio").val()
    };

    PostPerfil("GuardarEstudio", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#esNivel").val("");
            $("#esInstitucion, #esTitulo, #esAnio").val("");
            CargarPerfil();
        }
    });
}

function EliminarEstudio(idEstudio) {
    PostPerfil("EliminarEstudio", { idEstudio: idEstudio }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}

function PintarCertificaciones(lista) {
    var $cuerpo = $("#cuerpoCertificaciones").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="5" class="text-center text-muted">' +
                       'Todavía no ha registrado ninguna certificación.</td></tr>');
        return;
    }

    $.each(lista, function (i, c) {
        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(c.Nombre));
        $fila.append($("<td></td>").text(c.Entidad));
        $fila.append($("<td></td>").text(c.FechaObtencion === "" ? "–" : c.FechaObtencion));
        $fila.append(CeldaDocumentos("CERTIFICACION", c.IdCertificacion));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarCertificacion(' + c.IdCertificacion + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarCertificacion() {
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.", "warning");
        return;
    }

    var datos = {
        idCertificacion: 0,
        nombre:          $("#ceNombre").val(),
        entidad:         $("#ceEntidad").val(),
        fechaObtencion:  $("#ceFecha").val()
    };

    PostPerfil("GuardarCertificacion", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#ceNombre, #ceEntidad, #ceFecha").val("");
            CargarPerfil();
        }
    });
}

function EliminarCertificacion(idCertificacion) {
    PostPerfil("EliminarCertificacion", { idCertificacion: idCertificacion }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}

function PintarExperiencia(lista) {
    var $cuerpo = $("#cuerpoExperiencia").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="5" class="text-center text-muted">' +
                       'Todavía no ha registrado experiencia laboral.</td></tr>');
        return;
    }

    $.each(lista, function (i, x) {
        /* AnioHasta nulo significa "sigue ahi", no "no se sabe". */
        var periodo = x.AnioDesde + " – " + (x.AnioHasta === null ? "Actual" : x.AnioHasta);

        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(x.Empresa));
        $fila.append($("<td></td>").text(x.Cargo));
        $fila.append($("<td></td>").text(periodo));
        $fila.append($("<td></td>").text(x.Funciones));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarExperiencia(' + x.IdExperiencia + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarExperiencia() {
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.", "warning");
        return;
    }

    var datos = {
        idExperiencia: 0,
        empresa:       $("#exEmpresa").val(),
        cargo:         $("#exCargo").val(),
        anioDesde:     $("#exDesde").val(),
        anioHasta:     $("#exHasta").val(),
        funciones:     $("#exFunciones").val()
    };

    PostPerfil("GuardarExperiencia", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#exEmpresa, #exCargo, #exDesde, #exHasta, #exFunciones").val("");
            CargarPerfil();
        }
    });
}

function EliminarExperiencia(idExperiencia) {
    PostPerfil("EliminarExperiencia", { idExperiencia: idExperiencia }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}

function PintarCargasFamiliares(lista) {
    var $cuerpo = $("#cuerpoCargas").empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append('<tr><td colspan="5" class="text-center text-muted">' +
                       'Todavía no ha registrado ninguna carga familiar.</td></tr>');
        return;
    }

    $.each(lista, function (i, c) {
        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").text(c.Nombre));
        $fila.append($("<td></td>").text(c.Parentesco));
        $fila.append($("<td></td>").text(c.FechaNacimiento));
        $fila.append(CeldaDocumentos("CARGAFAMILIAR", c.IdCargaFam));
        $fila.append('<td class="text-center"><button type="button" class="btn btn-danger btn-xs" ' +
                     'onclick="EliminarCargaFamiliar(' + c.IdCargaFam + ')"><i class="fa fa-trash"></i></button></td>');
        $cuerpo.append($fila);
    });
}

function AgregarCargaFamiliar() {
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.", "warning");
        return;
    }

    var datos = {
        idCargaFam:      0,
        nombre:          $("#cfNombre").val(),
        parentesco:      $("#cfParentesco").val(),
        fechaNacimiento: $("#cfFecha").val()
    };

    PostPerfil("GuardarCargaFamiliar", datos, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") {
            $("#cfNombre, #cfFecha").val("");
            $("#cfParentesco").val("");
            CargarPerfil();
        }
    });
}

function EliminarCargaFamiliar(idCargaFam) {
    PostPerfil("EliminarCargaFamiliar", { idCargaFam: idCargaFam }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}

function Iniciales(nombre) {
    if (!nombre) { return "–"; }
    var partes = nombre.trim().split(/\s+/);
    if (partes.length === 1) { return partes[0].substring(0, 2).toUpperCase(); }
    return (partes[0].charAt(0) + partes[1].charAt(0)).toUpperCase();
}

/* ----------------------------- utilitarios ------------------------------- */

/* Copiada tal cual de parametrizacionHorarioUsuario.js. Cada pantalla lleva
   su propia copia (es el patron de la casa); no se crea una utilidad
   compartida solo para esta pantalla. */
function MostrarMensaje(mensaje, tipo) {
    var color = "#fcf8e3";
    if (tipo == "success") { color = "#dff0d8"; }
    if (tipo == "danger") { color = "#f2dede"; }
    if (tipo == "warning") { color = "#fcf8e3"; }

    $("#modalMensajeInformativoTipo").css("background", color);
    $("#MensajeInformativo").html(mensaje);
    $("#modalMensajeInformativo").modal("show");
}

/* ------------------------------------------------------------------ foto -- */

/* 256 es el doble de los 96 px con los que se muestra: se ve nitida en
   pantallas de densidad doble y sigue pesando unos 25 KB en base64. */
var FOTO_LADO = 256;

function ElegirFoto() {
    $("#inFoto").click();
}

$(document).on("change", "#inFoto", function () {
    var archivo = this.files && this.files[0];

    /* Se limpia el input antes de nada: si no, volver a elegir el mismo archivo
       no dispara "change" y el usuario cree que el boton dejo de funcionar. */
    this.value = "";

    if (!archivo) { return; }

    if (archivo.type !== "image/jpeg" && archivo.type !== "image/png") {
        MostrarMensaje("La foto debe ser una imagen JPG o PNG.", "warning");
        return;
    }

    var lector = new FileReader();

    lector.onload = function (e) {
        var imagen = new Image();
        imagen.onload = function () { EnviarFoto(RecortarCuadrado(imagen)); };
        imagen.onerror = function () {
            MostrarMensaje("No pudimos leer esa imagen. Pruebe con otra.", "warning");
        };
        imagen.src = e.target.result;
    };

    lector.onerror = function () {
        MostrarMensaje("No pudimos leer ese archivo. Intente nuevamente.", "warning");
    };

    lector.readAsDataURL(archivo);
});

/* Recorta el cuadrado central y reduce a 256x256.
   Se hace en el navegador y no en el servidor porque lo que viaja es el
   resultado: la foto de 4 MB de un telefono sale de la maquina convertida en
   unos 25 KB. El servidor igual valida lo que recibe -es alcanzable por HTTP
   directo- pero no tiene que cargar con la imagen original. */
function RecortarCuadrado(imagen) {
    var lado = Math.min(imagen.width, imagen.height);
    var x = (imagen.width - lado) / 2;
    var y = (imagen.height - lado) / 2;

    var lienzo = document.createElement("canvas");
    lienzo.width = FOTO_LADO;
    lienzo.height = FOTO_LADO;
    lienzo.getContext("2d").drawImage(imagen, x, y, lado, lado, 0, 0, FOTO_LADO, FOTO_LADO);

    /* Siempre JPEG, aunque el original sea PNG: una foto de una persona pesa
       mucho menos en JPEG y la transparencia no aporta nada en un avatar. */
    return lienzo.toDataURL("image/jpeg", 0.85);
}

function EnviarFoto(dataUri) {
    /* El servidor guarda solo el payload; el prefijo "data:image/jpeg;base64,"
       lo vuelve a armar al leer. Mandar el data URI entero lo rechaza
       ValidarFoto a proposito. */
    var coma = dataUri.indexOf(",");

    PostPerfil("GuardarFoto", { base64: dataUri.substring(coma + 1), tipo: "image/jpeg" },
        function (respuesta) {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            if (respuesta.estado === "1") { CargarPerfil(); }
        });
}

function QuitarFoto() {
    PostPerfil("EliminarFoto", {}, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}

/* ------------------------------------------------------------ documentos -- */

/* Los respaldos vienen todos en una sola lista y la pantalla los reparte: asi
   una unica consulta sirve a las dos pestanias. */
function DocumentosDe(origen, idOrigen) {
    var encontrados = [];

    if (_perfil && _perfil.Documentos) {
        $.each(_perfil.Documentos, function (i, d) {
            if (d.Origen === origen && d.IdOrigen === idOrigen) { encontrados.push(d); }
        });
    }

    return encontrados;
}

/* La celda de respaldos de una fila: los que ya tiene, y el enlace para sumar
   uno mas. Se arma con jQuery y .text() -nunca concatenando HTML- porque el
   nombre del archivo lo escribio la persona al guardarlo en su maquina. */
function CeldaDocumentos(origen, idOrigen) {
    var $celda = $('<td class="text-center"></td>');

    $.each(DocumentosDe(origen, idOrigen), function (i, d) {
        var $fila = $('<div style="margin-bottom:3px"></div>');

        var $enlace = $('<a target="_blank" style="font-size:11px"></a>')
            .attr("href", "DescargarPerfil.ashx?doc=" + d.IdDocumento +
                          (_codObjetivo === "" ? "" : "&u=" + encodeURIComponent(_codObjetivo)))
            .attr("title", d.NombreArchivo)
            .text(d.NombreArchivo);

        var $quitar = $('<a href="javascript:void(0)" title="Quitar" style="margin-left:6px">' +
                        '<i class="fa fa-times text-danger"></i></a>')
            .on("click", function () { EliminarDocumento(d.IdDocumento); });

        $celda.append($fila.append($enlace).append($quitar));
    });

    var $adjuntar = $('<a href="javascript:void(0)" style="font-size:11px">' +
                      '<i class="fa fa-paperclip"></i> Adjuntar</a>')
        .on("click", function () { PedirArchivo(origen, idOrigen); });

    return $celda.append($adjuntar);
}

/* Le cuelga al input compartido a que fila pertenece y lo abre. */
function PedirArchivo(origen, idOrigen) {
    $("#inDocumento").data("origen", origen).data("idOrigen", idOrigen).click();
}

$(document).on("change", "#inDocumento", function () {
    var archivo = this.files && this.files[0];
    var origen = $(this).data("origen");
    var idOrigen = $(this).data("idOrigen");

    /* Se limpia antes de nada: si no, elegir dos veces el mismo archivo no
       vuelve a disparar "change". */
    this.value = "";

    if (!archivo) { return; }

    /* Mismo limite y mismo mensaje que NegPerfilCampos.ValidarDocumento. El
       servidor sigue siendo quien valida de verdad -esto es comodidad, no
       seguridad-, pero sin este chequeo un archivo de 40 MB no llega ni a
       esa validacion: IIS lo corta antes (alrededor de 28 MB) y el usuario
       ve el mensaje generico de error de red, mientras que uno de 10 MB si
       llega al servidor y recibe el mensaje correcto. La misma accion no
       deberia contar dos historias distintas segun el tamano del archivo. */
    if (archivo.size > 5242880) {
        MostrarMensaje("El documento no puede pesar más de 5 MB.", "warning");
        return;
    }

    var datos = new FormData();
    datos.append("origen", origen);
    datos.append("idOrigen", idOrigen);
    datos.append("archivo", archivo);
    /* Esta llamada no pasa por PostPerfil -es multipart-, asi que el codigo del
       perfil se agrega a mano. El handler lo lee de Request.Form. */
    if (_codObjetivo !== "") { datos.append("codUsuario", _codObjetivo); }

    /* Esta llamada no puede usar PostPerfil: aquella manda JSON y esto es
       multipart. processData y contentType en false son lo que hace que jQuery
       entregue el FormData tal cual y deje que el navegador ponga el boundary. */
    $.ajax({
        type: "POST",
        url: "AdministrarPerfil.ashx",
        data: datos,
        processData: false,
        contentType: false,
        dataType: "json",
        success: function (respuesta) {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            if (respuesta.estado === "1") { CargarPerfil(); }
        },
        error: function () {
            MostrarMensaje("No pudimos subir el archivo. Intente nuevamente.", "danger");
        }
    });
});

function EliminarDocumento(idDocumento) {
    PostPerfil("EliminarDocumento", { idDocumento: idDocumento }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}

/* ---------------------------------------------------------------- equipo -- */

/* La pestana existe o no segun el dato, no segun un perfil ni una fila de
   menu: si alguien tiene gente que le reporta, la ve. Son 22 personas hoy y
   el dia que cambie no hay nada que mantener. */
function MostrarPestanaEquipo(esJefe) {
    /* Nunca al mirar el perfil de otra persona. ListaEquipo y PerfilEquipo
       toman al jefe de la SESION, asi que aqui se veria el equipo de quien mira
       con el nombre de otro en la cabecera. No es cosmetica: es el dato de
       otra persona bajo una etiqueta equivocada. */
    if (_codObjetivo !== "") { return; }
    if (!esJefe) { return; }

    $("#liTabEquipo").show();
    BuscarEquipo();
}

function BuscarEquipo() {
    PostPerfil("ListaEquipo", { filtro: $("#txtBuscarEquipo").val() }, function (respuesta) {
        // Un objeto con "estado" es un EntRespuesta, es decir, un error.
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        PintarListaEquipo(respuesta || []);
    });
}

function LimpiarBusquedaEquipo() {
    $("#txtBuscarEquipo").val("");
    BuscarEquipo();
}

function PintarListaEquipo(lista) {
    var $cuerpo = $("#cuerpoEquipo").empty();

    /* Al cambiar la lista se cierra el detalle: dejarlo abierto mostraria a una
       persona que ya no esta en los resultados. */
    $("#panelSubordinado").hide();

    if (lista.length === 0) {
        $cuerpo.append('<tr><td colspan="5" class="text-center text-muted">' +
                       'No hay personas que coincidan con esa búsqueda.</td></tr>');
        return;
    }

    $.each(lista, function (i, p) {
        var $fila = $("<tr></tr>");

        /* .text() y no concatenacion de HTML: estos nombres y cargos son de
           OTRAS personas y los teclearon ellas o Talento Humano. */
        $fila.append($("<td></td>").text(p.NombreCompleto || "–"));
        $fila.append($("<td></td>").text(p.Cargo || "–"));
        $fila.append($("<td></td>").text(p.Area || "–"));
        $fila.append($("<td></td>").text(p.Ciudad || "–"));

        var $ver = $('<button type="button" class="btn btn-primary btn-xs"><i class="fa fa-eye"></i></button>')
            .on("click", function () { VerSubordinado(p.CodUsuario); });

        $fila.append($('<td class="text-center"></td>').append($ver));
        $cuerpo.append($fila);
    });
}

function VerSubordinado(codUsuario) {
    PostPerfil("PerfilEquipo", { codUsuario: codUsuario }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        /* El servidor no distingue "no es de su equipo" de "no existe", y aqui
           tampoco: un mensaje distinto le confirmaria a quien prueba codigos
           cual de ellos es real. */
        if (!respuesta.PerfilEncontrado) {
            MostrarMensaje("No pudimos mostrar ese perfil.", "warning");
            $("#panelSubordinado").hide();
            return;
        }

        PintarSubordinado(respuesta);
    });
}

function PintarSubordinado(perfil) {
    var c = perfil.Cabecera;

    $("#subNombre").text(c.NombreCompleto || "–");
    $("#subCargo").text(c.Cargo || "–");
    $("#subArea").text(c.Area || "–");
    $("#subCiudad").text(c.Ciudad || "–");
    $("#subCorreo").text(c.CorreoNotificacion || "–");
    $("#subJefe").text(c.JefeInmediato || "–");
    $("#subHorario").text(c.Horario || "–");

    if (perfil.Foto && perfil.Foto.DataUri) {
        $("#subFoto").attr("src", perfil.Foto.DataUri).show();
        $("#subAvatar").hide();
    } else {
        $("#subFoto").hide().removeAttr("src");
        $("#subAvatar").text(Iniciales(c.NombreCompleto)).show();
    }

    PintarFilas("#cuerpoSubEmergencia", perfil.Emergencia, 3, function (x) {
        return [x.Nombre, x.Parentesco, x.Telefono];
    });

    PintarFilas("#cuerpoSubEstudios", perfil.Estudios, 4, function (x) {
        return [x.Titulo, x.Institucion, x.Nivel,
                x.AnioGraduacion === null ? "–" : String(x.AnioGraduacion)];
    });

    PintarFilas("#cuerpoSubCertificaciones", perfil.Certificaciones, 3, function (x) {
        return [x.Nombre, x.Entidad, x.FechaObtencion === "" ? "–" : x.FechaObtencion];
    });

    PintarFilas("#cuerpoSubExperiencia", perfil.Experiencia, 4, function (x) {
        var desde = x.AnioDesde === null ? "" : String(x.AnioDesde);
        var hasta = x.AnioHasta === null ? "Actual" : String(x.AnioHasta);
        return [x.Empresa, x.Cargo, desde === "" ? "–" : desde + " - " + hasta, x.Funciones];
    });

    $("#panelSubordinado").show();
}

/* Las cuatro tablas del detalle son de solo lectura y tienen la misma forma:
   vaciar, y pintar una celda por columna con .text(). Se comparte una funcion
   en vez de repetir el bucle cuatro veces con distinto numero de columnas. */
function PintarFilas(selectorCuerpo, lista, columnas, celdasDe) {
    var $cuerpo = $(selectorCuerpo).empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append($('<tr></tr>').append(
            $('<td class="text-center text-muted"></td>')
                .attr("colspan", columnas)
                .text("Sin registros.")));
        return;
    }

    $.each(lista, function (i, x) {
        var $fila = $("<tr></tr>");

        $.each(celdasDe(x), function (j, valor) {
            $fila.append($("<td></td>").text(valor || "–"));
        });

        $cuerpo.append($fila);
    });
}
