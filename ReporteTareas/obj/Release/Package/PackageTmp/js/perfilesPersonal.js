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
    /* miPerfil.js llama a CargarPerfil() al cargar la pagina, lo que aqui
       mostraria el perfil de quien esta mirando antes de que elija a nadie. Las
       fichas arrancan ocultas y no se muestran hasta que hay alguien elegido. */
    $("#panelFichas").hide();

    $("#btnBuscarPersonal").on("click", BuscarPersonal);

    $("#txtBuscarPersonal").on("keypress", function (e) {
        if (e.which === 13) { e.preventDefault(); BuscarPersonal(); }
    });
});

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
    $("#personaElegida").text(nombre || codUsuario);
    $("#panelFichas").show();

    /* Fija el objetivo y recarga las fichas. A partir de aqui, cada guardado de
       miPerfil.js viaja con este codigo y el servidor decide si lo acepta. */
    FijarPerfilObjetivo(codUsuario, nombre);

    $("html, body").animate({ scrollTop: $("#panelFichas").offset().top - 20 }, 300);
}
