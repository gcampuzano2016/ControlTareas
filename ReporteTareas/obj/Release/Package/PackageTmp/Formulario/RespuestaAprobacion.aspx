<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RespuestaAprobacion.aspx.cs" Inherits="ReporteTareas.Formulario.RespuestaAprobacion" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
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
</body>
</html>
