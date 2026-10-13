/* Curva de Aprendizaje - lista de colaboradores (CurvaColaboradores.aspx).
   El filtrado es en el cliente: texto + estado, combinables. */
$(function () {
    "use strict";
    var filtroEstado = "";

    CA.api("listar", {}, function (r) {
        var lista = r.colaboradores || [];
        if (lista.length) {
            $("#caChips").html(
                '<span class="ca-chip"><b>' + r.total + "</b> Colaboradores</span>" +
                '<span class="ca-chip"><b>' + r.activos + "</b> Activos</span>" +
                '<span class="ca-chip"><b>' + r.enCurso + "</b> Procesos en curso</span>").show();
        }
        if (!lista.length) {
            $("#caLista").html('<div class="ca-vacio">Aún no hay colaboradores registrados. Usa "Nuevo colaborador" para iniciar su curva de aprendizaje.</div>');
            return;
        }
        $("#caLista").html($.map(lista, fila).join(""));
        aplicarFiltro();
    });

    function fila(c) {
        var nombre = c.NombreCompleto || "";
        var url = "CurvaColaboradorFicha.aspx?id=" + c.Id;
        return '<div class="ca-fila" data-url="' + url + '" data-nombre="' + CA.esc(nombre.toLowerCase()) + '" data-estado="' + CA.esc(c.Estado) + '">' +
            '<div class="ca-colab"><span class="ca-avatar" style="background:' + CA.colorAvatar(nombre) + '">' + CA.esc(nombre.charAt(0).toUpperCase()) + "</span>" +
            '<div class="ca-recorte"><div class="ca-nombre ca-recorte">' + CA.esc(nombre) + '</div><div class="ca-cargo ca-recorte">' + CA.esc(c.Cargo || "Cargo no especificado") + "</div></div></div>" +
            '<div class="ca-recorte">' + CA.esc(c.Area || "—") + "</div>" +
            '<div class="ca-fecha">' + CA.ICONO_CALENDARIO + CA.fechaCorta(c.FechaIngreso) + "</div>" +
            "<div>" + CA.badge(c.Estado) + "</div>" +
            "<div>" + (c.ProcesoEstado ? CA.badge(c.ProcesoEstado) : "—") + "</div>" +
            '<div><a class="ca-btn ca-btn-outline ca-ver" href="' + url + '">Ver ficha →</a></div>' +
            "</div>";
    }

    function aplicarFiltro() {
        var texto = $.trim($("#caBuscar").val() || "").toLowerCase();
        var visibles = 0;
        $("#caLista .ca-fila").each(function () {
            var $f = $(this);
            var ok = (!texto || String($f.attr("data-nombre")).indexOf(texto) >= 0) && (!filtroEstado || $f.attr("data-estado") === filtroEstado);
            $f.toggle(ok);
            if (ok) { visibles += 1; }
        });
        $("#caSinResultados").toggle($("#caLista .ca-fila").length > 0 && visibles === 0);
    }

    $("#caBuscar").on("input keyup", aplicarFiltro);
    $(".ca-filtro").on("click", function () {
        $(".ca-filtro").removeClass("activo");
        $(this).addClass("activo");
        filtroEstado = $(this).data("filtro") || "";
        aplicarFiltro();
    });
    // Toda la fila es clicable; el boton detiene la propagacion (navega por su href).
    $("#caLista").on("click", ".ca-fila", function () { window.location.href = $(this).data("url"); });
    $("#caLista").on("click", ".ca-ver", function (e) { e.stopPropagation(); });
});
