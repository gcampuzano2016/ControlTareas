<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RespuestaAprobacion.aspx.cs" Inherits="ReporteTareas.Formulario.RespuestaAprobacion" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title></title>

    <%-- jQuery local y no de un CDN: esta página se abre desde el correo, muchas
         veces desde el teléfono, y si el CDN no responde el pad no se dibuja y el
         jefe no puede firmar. --%>
    <script src="../js/jquery.min.js" type="text/javascript"></script>
    <script src="../js/padFirma.js?v=2" type="text/javascript"></script>

    <style type="text/css">
        body { font-family: 'Segoe UI', Arial, sans-serif; color: #333; }
        .hoja { max-width: 560px; margin: 24px auto; text-align: left; }
        .titulo { font-size: 17px; font-weight: bold; color: #1F3864; margin-bottom: 4px; }
        .ayuda { color: #666; font-size: 13px; margin: 0 0 14px; }
        .btn-confirmar {
            margin-top: 14px; padding: 9px 20px; font-size: 14px; cursor: pointer;
            background: #1F3864; color: #fff; border: none; border-radius: 3px;
        }
        .aviso { color: #a94442; font-size: 13px; }
        .pad-firma .nav-tabs { list-style: none; padding: 0; margin: 0 0 8px; }
        .pad-firma .nav-tabs li { display: inline-block; margin-right: 10px; }
        .pad-firma .nav-tabs li a { text-decoration: none; color: #1F3864; font-size: 13px; }
        .pad-firma .nav-tabs li.active a { font-weight: bold; text-decoration: underline; }
        .pad-firma .tab-pane { display: none; }
        .pad-firma .tab-pane.active { display: block; }
        .pad-firma .help-block { color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div align="center">
            <panel runat="server" width="553px" headertext="Confirmación de Aprobación" height="52px" horizontalalign="Center">
                <br />
                <br />
                <asp:Label ID="lblmensaje" runat="server"></asp:Label>
                <br />
                <br />

                <%-- El paso de firma. Aparece antes de aplicar la decisión: hasta que
                     el jefe no firme, la solicitud no cambia de estado. --%>
                <asp:Panel ID="pnlFirma" runat="server" Visible="false">
                    <div class="hoja">
                        <div class="titulo">
                            <asp:Literal ID="litTituloFirma" runat="server"></asp:Literal></div>
                        <p class="ayuda">
                            <asp:Literal ID="litAyudaFirma" runat="server"></asp:Literal></p>

                        <div id="divFirmaJefe"></div>

                        <asp:HiddenField ID="hfTrazo" runat="server" />
                        <asp:HiddenField ID="hfParametros" runat="server" />

                        <p class="aviso" id="msgFalta" style="display: none">
                            Debe dibujar su firma para continuar.</p>

                        <asp:Button ID="btnConfirmar" runat="server" CssClass="btn-confirmar"
                            Text="Firmar y confirmar" OnClientClick="return TomarFirma();"
                            OnClick="btnConfirmar_Click" />
                    </div>
                </asp:Panel>

                <br />

                <div class="text-center" runat="server" id="Aprobados" visible="false">
                    <a href="#">
                        <img src="../Img/Vacaciones Aprobadas.jpg" alt="Image" width="603" height="520" class="block-center img-rounded">
                    </a>
                </div>
                <div class="text-center" runat="server" id="Rechazado" visible="false">
                    <a href="#">
                        <img src="../Img/Vacaciones Negadas.jpg" alt="Image" width="603" height="520" class="block-center img-rounded">
                    </a>
                </div>

                <div class="text-center" runat="server" id="PermisoAprobado" visible="false">
                    <a href="#">
                        <img src="../Img/image005.jpg" alt="Image" width="603" height="520" class="block-center img-rounded">
                    </a>
                </div>

                <div class="text-center" runat="server" id="PermisoRechazado" visible="false">
                    <a href="#">
                        <img src="../Img/image006.jpg" alt="Image" width="603" height="520" class="block-center img-rounded">
                    </a>
                </div>

            </panel>
        </div>
    </form>

    <script type="text/javascript">
        var _padJefe = null;

        $(function () {
            if ($("#divFirmaJefe").length === 0) { return; }
            _padJefe = PadFirma("divFirmaJefe", { ancho: 420, alto: 150 });

            /* El pad usa las pestañas de Bootstrap para alternar entre dibujar y
               subir una imagen, y acá no hay Bootstrap. Se resuelven a mano con las
               mismas clases, que es lo que espera el marcado del pad. */
            $(".pad-firma .nav-tabs a").on("click", function (e) {
                e.preventDefault();
                var destino = $(this).attr("href");
                $(this).closest(".pad-firma").find(".nav-tabs li").removeClass("active");
                $(this).parent().addClass("active");
                $(this).closest(".pad-firma").find(".tab-pane").removeClass("active");
                $(destino).addClass("active");
            });
        });

        /* Corre antes del postback. Si no hay trazo, no deja continuar: la decisión
           y la firma viajan juntas o no viaja ninguna. */
        function TomarFirma() {
            if (_padJefe === null || _padJefe.estaVacio()) {
                document.getElementById("msgFalta").style.display = "block";
                return false;
            }

            document.getElementById("msgFalta").style.display = "none";
            document.getElementById("<%= hfTrazo.ClientID %>").value = _padJefe.obtenerTrazo();
            return true;
        }
    </script>
</body>
</html>
