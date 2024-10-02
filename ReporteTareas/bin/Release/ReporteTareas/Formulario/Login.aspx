<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ReporteTareas.Formulario.Login" EnableEventValidation="false"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <!--//////////////////////////////////////////////////////////////////////////////////////////////////////////////-->
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="description" content="">
    <meta name="author" content="Carlos López">
    <title class="">Control de Tareas</title>
    <link href="../bower_components/bootstrap/dist/css/bootstrap.min.css" rel="stylesheet">
    <link href="../bower_components/metisMenu/dist/metisMenu.min.css" rel="stylesheet">
    <link href="../dist/css/sb-admin-2.css" rel="stylesheet">
    <link href="../bower_components/font-awesome/css/font-awesome.min.css" rel="stylesheet" type="text/css">
    
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
                        <div class="panel-heading">
                            <h1 class="text-center panel-collapse">Control de Tareas</h1>
                        </div>
                        <div class="panel-body">
                            <fieldset>
                                <div class="form-group">
                                    <h4 class="panel-title text-left">Usuario</h4>
                                    <%--<asp:TextBox ID="txt_login" runat="server" CssClass="form-control center-block tam-text-box" OnTextChanged="txt_login_TextChanged"></asp:TextBox>--%>
                                    <asp:TextBox ID="txt_login" runat="server" CssClass="form-control center-block tam-text-box" OnTextChanged="txt_login_TextChanged"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <h4 class="panel-title text-left"><asp:Label ID="lab2" runat="server" Visible="false" Enabled="false"></asp:Label></h4>
                                    <%--<asp:TextBox ID="txt_pass" runat="server" Visible="false" CssClass="form-control center-block tam-text-box" AutoPostBack="true" TextMode="Password" Enabled="false" OnTextChanged="txt_pass_TextChanged"></asp:TextBox>--%>
                                    <asp:TextBox ID="txt_pass" runat="server" Visible="false" CssClass="form-control center-block tam-text-box" AutoPostBack="true" TextMode="Password" Enabled="false" OnTextChanged="txt_pass_TextChanged"></asp:TextBox>
                                    
                                    <asp:TextBox ID="txt_passIngresa" runat="server" Visible="false" CssClass="form-control center-block tam-text-box" AutoPostBack="true" TextMode="Password" Enabled="false" OnTextChanged="txt_passIngresa_TextChanged"></asp:TextBox>
                                    <br />
                                    <h4 class="panel-title text-left"><asp:Label ID="lab4" runat="server" Visible="false" Enabled="false"></asp:Label></h4>
                                    <asp:TextBox ID="txt_passConfir" runat="server" Visible="false" CssClass="form-control center-block tam-text-box" AutoPostBack="true" TextMode="Password" Enabled="false" OnTextChanged="txt_passConfir_TextChanged"></asp:TextBox>
                                </div>
                                <%--
                                    <div class="form-group">
                                    <h4 class="panel-title text-left"><asp:Label ID="lab3" Text="Empresa" runat="server" Visible="false" Enabled="false"></asp:Label></h4>
                                    <%--<asp:DropDownList ID="dbl_Empresa" runat="server" CssClass="form-control tam-txtbox-combo" Visible="false" Enabled="false" AutoPostBack="true" OnTextChanged="dbl_Empresa_TextChanged"></asp:DropDownList>--%>
                                    <%--<asp:DropDownList ID="dbl_Empresa" runat="server" CssClass="form-control tam-txtbox-combo" Visible="false" Enabled="false" AutoPostBack="true"></asp:DropDownList>
                                    </div>
                                --%>
                                <div class="form-group">
                                    <asp:Label ID="lab1" runat="server" Visible="false" CssClass="text-danger"></asp:Label>
                                </div>
                                <div class="form-group">
                                    <asp:HyperLink ID="Hpl" runat="server" CssClass="alert-info" Visible="false" Enabled="false">¿Olvido su contraseña?</asp:HyperLink>
                                </div>
                                <div class="form-group">
                                    <%--<asp:Button ID="btn_ingresar" runat="server" Text="Ingresar" Visible="false" CssClass="btn center-block" Enabled="false" OnClick="btn_ingresar_Click"/>--%>
                                    <asp:Button ID="btn_ingresar" runat="server" Text="Ingresar" Visible="false" CssClass="btn center-block" Enabled="false"/>
                                </div>
                            </fieldset>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script src="../bower_components/jquery/dist/jquery.min.js"></script>
        <script src="../bower_components/bootstrap/dist/js/bootstrap.min.js"></script>
        <script src="../bower_components/metisMenu/dist/metisMenu.min.js"></script>
        <script src="../dist/js/sb-admin-2.js"></script>
    <!--//////////////////////////////////////////////////////////////////////////////////////////////////////////////-->    
    </div>
    </form>
</body>
</html>
