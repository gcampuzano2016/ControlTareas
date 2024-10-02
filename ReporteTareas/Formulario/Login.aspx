<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ReporteTareas.Formulario.Login" EnableEventValidation="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <!--//////////////////////////////////////////////////////////////////////////////////////////////////////////////-->
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="description" content="">
    <meta name="author" content="DOS">
    <title class="">Sistema Gesti&oacute;n Interno</title>
    <link rel="icon" type="image/png" href="../Img/iconoDos.png" sizes="16x16">
    <link href="../bower_components/bootstrap/dist/css/bootstrap.min.css" rel="stylesheet">
    <link href="../bower_components/metisMenu/dist/metisMenu.min.css" rel="stylesheet">
    <link href="../dist/css/sb-admin-2.css" rel="stylesheet">
    <link href="../bower_components/font-awesome/css/font-awesome.min.css" rel="stylesheet" type="text/css">
    <script>
        window.onload = function () {
            const hostname = window.location.hostname;
            console.log(window.location.hostname); // "www.ejemplo.com"
            console.log(window.location.host);     // "www.ejemplo.com:8080"
            console.log(window.location.href);
            console.log(hostname);
        };
</script>

    <!--//////////////////////////////////////////////////////////////////////////////////////////////////////////////-->
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <!--//////////////////////////////////////////////////////////////////////////////////////////////////////////////-->
            <div class="container">
                <div class="row">
                    <div class="col-md-4 col-md-offset-4">
                        <div class="login-panel panel panel-info">
                            <div class="panel-heading" style="text-align: center; background-color: white">
                                <img src="../Img/iconoDos.png" height="70" /><br />
                                <h1 class="text-center panel-collapse"><font color="#6F6F6F" size="5">Sistema de Gesti&oacute;n Interno </font></h1>
                                <font color="6F6F6F">Versión 2.5</font>
                            </div>
                            <div class="panel-body" style="background-color: #d3d3d3">
                                <fieldset>
                                    <div class="form-group">
                                        <h4 class="panel-title text-left"><font color="#E53935" size="3"><b>Usuario </b></font></h4>
                                        <%--<asp:TextBox ID="txt_login" runat="server" CssClass="form-control center-block tam-text-box" OnTextChanged="txt_login_TextChanged"></asp:TextBox>--%>
                                        <asp:TextBox ID="txt_login" runat="server" CssClass="form-control center-block tam-text-box"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <h4 class="panel-title text-left">
                                            <asp:Label ID="lab2" runat="server" Enabled="False"> <FONT COLOR="#E53935" size="3"><b> Contraseña</b></FONT></asp:Label></h4>
                                        <%--<asp:TextBox ID="txt_pass" runat="server" Visible="false" CssClass="form-control center-block tam-text-box" AutoPostBack="true" TextMode="Password" Enabled="false" OnTextChanged="txt_pass_TextChanged"></asp:TextBox>--%>
                                        <asp:TextBox ID="txt_pass" runat="server" CssClass="form-control center-block tam-text-box" TextMode="Password"></asp:TextBox>

                                        <asp:TextBox ID="txt_passIngresa" runat="server" Visible="false" CssClass="form-control center-block tam-text-box" AutoPostBack="true" TextMode="Password" Enabled="false"></asp:TextBox>
                                        <br />
                                        <h4 class="panel-title text-left">
                                            <asp:Label ID="lab4" runat="server" Visible="False" Enabled="False">Confirmar contraseña</asp:Label></h4>
                                        <asp:TextBox ID="txt_passConfir" runat="server" Visible="false" CssClass="form-control center-block tam-text-box" AutoPostBack="true" TextMode="Password" Enabled="false"></asp:TextBox>
                                    </div>
                                    <div class="form-group" style="text-align: center;">
                                        <asp:Button ID="btn_ingresar" runat="server" Text="Ingresar" CssClass="btn btn-primary" OnClick="btn_ingresar_Click" />
                                    </div>
                                    <div class="form-group">
                                        <asp:Label ID="lab1" runat="server" Visible="false" CssClass="text-danger"></asp:Label>
                                        <asp:HyperLink ID="Hpl" runat="server" CssClass="alert-info" Visible="false" Enabled="false">¿Olvido su contraseña?</asp:HyperLink>
                                    </div>
                                </fieldset>
                                <div class="form-group">
                                    <br />
                                    <div id="divMensajeLabel">
                                        <asp:Label ID="Label1" runat="server" Visible="true" CssClass="text-danger"></asp:Label>
                                    </div>
                                    <br />
                                    <div id="divMensajes" class="">
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <script src="../bower_components/jquery/dist/jquery.min.js"></script>
            <script src="../bower_components/bootstrap/dist/js/bootstrap.min.js"></script>
            <script src="../bower_components/metisMenu/dist/metisMenu.min.js"></script>
            <script src="../dist/js/sb-admin-2.js"></script>
            <script src="../bower_components/flot.tooltip/js/jquery.flot.tooltip.min.js"></script>
            <script>
                var divMensajesFinal = '';
                if (typeof divMensajesContenido_0 === "undefined") {
                    var divMensajesContenido_0 = '';
                }
                if (typeof divMensajesContenido_1 === "undefined") {
                    var divMensajesContenido_1 = '';
                }
                if (typeof divMensajesContenido_2 === "undefined") {
                    var divMensajesContenido_2 = '';
                }
                if (typeof divMensajesContenido_3 === "undefined") {
                    var divMensajesContenido_3 = '';
                }
                if (typeof divMensajesContenido_4 === "undefined") {
                    var divMensajesContenido_4 = '';
                }
                var divMensajesContenido = [divMensajesContenido_0, divMensajesContenido_1, divMensajesContenido_2, divMensajesContenido_3, divMensajesContenido_4];
                divMensajesContenido.forEach(function (element) {
                    divMensajesFinal = divMensajesFinal.concat(element);
                });
                document.getElementById("divMensajes").innerHTML = divMensajesFinal;
            </script>
            <!--//////////////////////////////////////////////////////////////////////////////////////////////////////////////-->
        </div>
    </form>
</body>
</html>
