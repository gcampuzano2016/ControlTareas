/* ============================================================================
   Pantalla: Perfiles del personal
   Handler : AdministrarPerfil.ashx

   Este archivo hace dos cosas y nada mas: pinta la lista de personal y fija de
   quien es el perfil que se esta mirando. Todo lo demas -cargar las fichas,
   guardar, subir, descargar- lo hace miPerfil.js, que es el mismo archivo que
   usa "Mi perfil". Por eso los dos se cargan en esta pagina, en ese orden.

   La barrera no esta aca. Que esta pantalla solo la vean los perfiles 14 y 18
   lo deciden el menu y PerfilesPersonal.aspx.cs; que solo ellos puedan pedir
   datos ajenos lo decide NegPerfilAcceso, en el servidor.
   ============================================================================ */

$(document).ready(function () {
    /* El modal de avisos vive dentro del control de fichas, que en esta pantalla
       arranca oculto. Bootstrap 3 solo lo reubica al body cuando el elemento no
       tiene padre; aca lo tiene, asi que mostraria el velo gris sin el dialogo y
       sin forma de cerrarlo. Se lo saca del panel una vez, al cargar. */
    $("#modalMensajeInformativo").appendTo("body");

    /* Las fichas arrancan ocultas y no se muestran hasta que hay alguien
       elegido. La carga inicial de miPerfil.js no ocurre en esta pantalla: la
       apaga PERFIL_SIN_CARGA_INICIAL, declarada en PerfilesPersonal.aspx. */
    $("#panelFichas").hide();

    $("#btnBuscarPersonal").on("click", BuscarPersonal);

    $("#txtBuscarPersonal").on("keypress", function (e) {
        if (e.which === 13) { e.preventDefault(); BuscarPersonal(); }
    });

    $("#lnkHistorialCertificados").on("click", function () {
        var $cuerpo = $("#cuerpoHistorialCertificados").toggle();
        $("#icoHistorialCertificados").toggleClass("fa-chevron-down", !$cuerpo.is(":visible"))
                                      .toggleClass("fa-chevron-up", $cuerpo.is(":visible"));
        if ($cuerpo.is(":visible")) { CargarHistorialCertificados(); }
    });

    $("#fCertTexto").on("input", PintarHistorialCertificados);
    $("#fCertEmpresa, #fCertTipo").on("change", PintarHistorialCertificados);
    $("#btnCertCsv").on("click", ExportarCertificadosCsv);

    CargarHistorialCertificados();
});

/* ------------------------------------------- historial de certificados -- */

var _certificados = [];

function CargarHistorialCertificados() {
    PostPerfil("HistorialCertificados", {}, function (respuesta) {
        // Un objeto con "estado" es un EntRespuesta, es decir, un error.
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        _certificados = respuesta || [];
        $("#resumenCertificados").text(_certificados.length + " solicitados");
        PintarHistorialCertificados();
    });
}

function CertificadosFiltrados() {
    var texto = $.trim($("#fCertTexto").val()).toLowerCase();
    var empresa = $("#fCertEmpresa").val();
    var tipo = $("#fCertTipo").val();

    return $.grep(_certificados, function (c) {
        return (texto === "" || c.Nombre.toLowerCase().indexOf(texto) >= 0 || c.Cedula.indexOf(texto) >= 0) &&
               (empresa === "" || c.Empresa === empresa) &&
               (tipo === "" || c.Tipo === tipo);
    });
}

function PintarHistorialCertificados() {
    /* Los indicadores cuentan todo el historial, no lo filtrado: son el
       resumen de la pantalla, no del filtro. */
    var mes = new Date();
    var prefijoMes = mes.getFullYear() + "-" + ("0" + (mes.getMonth() + 1)).slice(-2);
    var kpis = [
        ["Total solicitados", _certificados.length],
        ["Este mes", $.grep(_certificados, function (c) { return c.Ts.indexOf(prefijoMes) === 0; }).length],
        ["Con salario", $.grep(_certificados, function (c) { return c.Tipo === "CON"; }).length],
        ["Agility", $.grep(_certificados, function (c) { return c.Empresa === "AGILITY"; }).length]
    ];

    var $kpis = $("#kpisCertificados").empty();
    $.each(kpis, function (i, k) {
        $kpis.append($('<div class="col-md-3 col-xs-6"></div>').append(
            $('<div class="well well-sm text-center" style="margin-bottom: 6px"></div>')
                .append($('<div style="font-size: 22px; font-weight: bold"></div>').text(k[1]))
                .append($('<small class="text-muted"></small>').text(k[0]))));
    });

    var lista = CertificadosFiltrados();
    var $cuerpo = $("#cuerpoCertificadosTodos").empty();

    if (lista.length === 0) {
        $cuerpo.append('<tr><td colspan="6" class="text-center text-muted">Sin resultados.</td></tr>');
        return;
    }

    $.each(lista, function (i, c) {
        var $fila = $("<tr></tr>");
        $fila.append($("<td></td>").append($("<b></b>").text(c.Fecha))
                                   .append($('<small class="text-muted"></small>').text(" " + c.Hora)));
        $fila.append($("<td></td>")
            .append($("<div></div>").text(c.Nombre))
            .append($('<small class="text-muted"></small>').text(c.Cargo + " · CI " + c.Cedula)));
        $fila.append($("<td></td>").text(c.Empresa === "AGILITY" ? "AGILITY" : "COMPUEQUIP DOS"));
        $fila.append($("<td></td>").append(EtiquetaTipoCertificado(c.Tipo)));
        $fila.append($('<td style="font-size: 12px"></td>').text(c.Firma));
        $fila.append($('<td class="text-center"></td>').append(
            $('<a class="btn btn-default btn-xs" title="Descargar PDF"><i class="fa fa-download"></i> PDF</a>')
                .attr("href", "DescargarPerfil.ashx?cert=" + encodeURIComponent(c.IdCertificado))));
        $cuerpo.append($fila);
    });
}

function ExportarCertificadosCsv() {
    var encabezado = ["Fecha solicitud", "Hora", "Colaborador", "Cédula", "Cargo", "Empresa", "Tipo", "Generado por"];
    var lineas = $.map(CertificadosFiltrados(), function (c) {
        return [[c.Fecha, c.Hora, c.Nombre, c.Cedula, c.Cargo,
                 c.Empresa === "AGILITY" ? "AGILITY" : "COMPUEQUIP DOS",
                 c.Tipo === "CON" ? "Con salario" : "Sin salario", c.UsuSolicita]
            .map(function (x) { return '"' + String(x).replace(/"/g, '""') + '"'; }).join(",")];
    });

    /* El BOM es para que Excel abra las tildes bien. */
    var blob = new Blob(["﻿" + [encabezado.join(",")].concat(lineas).join("\r\n")],
                        { type: "text/csv;charset=utf-8" });
    var a = document.createElement("a");
    a.href = URL.createObjectURL(blob);
    a.download = "historial_certificados_laborales.csv";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

function BuscarPersonal() {
    var filtro = $("#txtBuscarPersonal").val();

    /* La misma regla que NegPerfilCampos.ValidarFiltroPersonal. Esto es
       comodidad, no seguridad: el servidor vuelve a comprobarlo. */
    if ($.trim(filtro).length < 2) {
        MostrarMensaje("Escriba al menos 2 caracteres para buscar.", "warning");
        return;
    }

    PostPerfil("ListaPersonal", { filtro: filtro }, function (respuesta) {
        // Un objeto con "estado" es un EntRespuesta, es decir, un error.
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        PintarListaPersonal(respuesta);
    });
}

function PintarListaPersonal(lista) {
    var cuerpo = $("#cuerpoPersonal");
    cuerpo.empty();

    if (lista == null || lista.length === 0) {
        cuerpo.append('<tr><td colspan="5" class="text-muted">Sin resultados.</td></tr>');
        return;
    }

    for (var i = 0; i < lista.length; i++) {
        var p = lista[i];

        var fila = $("<tr>");
        fila.append($("<td>").text(p.NombreCompleto || "–"));
        fila.append($("<td>").text(p.CodUsuario || "–"));
        fila.append($("<td>").text(p.Cargo || "–"));
        fila.append($("<td>").text(p.Area || "–"));

        /* El boton lleva los datos colgados con .data() y no en el onclick: asi
           un nombre con comillas o con un apostrofo no rompe el marcado. */
        var boton = $('<button type="button" class="btn btn-default btn-xs" title="Abrir perfil">')
            .append('<i class="fa fa-eye"></i>')
            .data("cod", p.CodUsuario)
            .data("nombre", p.NombreCompleto)
            .on("click", function () {
                AbrirPerfilDe($(this).data("cod"), $(this).data("nombre"));
            });

        fila.append($("<td>").append(boton));
        cuerpo.append(fila);
    }
}

function AbrirPerfilDe(codUsuario, nombre) {
    /* Sin codigo no hay a quien abrir. FijarPerfilObjetivo("") dejaria el
       objetivo vacio, y entonces la pantalla mostraria -y guardaria sobre- el
       perfil de quien esta mirando, con el nombre de otra persona en el aviso. */
    if ($.trim(codUsuario || "") === "") {
        MostrarMensaje("Esa persona no tiene código de usuario y su perfil no se puede abrir.", "warning");
        return;
    }

    $("#personaElegida").text(nombre || codUsuario);
    $("#panelFichas").show();

    /* Fija el objetivo y recarga las fichas. A partir de aqui, cada guardado de
       miPerfil.js viaja con este codigo y el servidor decide si lo acepta. */
    FijarPerfilObjetivo(codUsuario, nombre);

    $("html, body").animate({ scrollTop: $("#panelFichas").offset().top - 20 }, 300);
}
