<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PrubaWebServices.aspx.cs" Inherits="ReporteTareas.Formulario.PrubaWebServices" ValidateRequest="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="Label1" runat="server" Text="IdUnicoTarea"></asp:Label>
            <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox><br />
            <asp:Label ID="Label2" runat="server" Text="Comentario"></asp:Label>
            <asp:TextBox ID="TextBox2" runat="server" TextMode="MultiLine" Width="377px"></asp:TextBox><br />
            <asp:Label ID="Label3" runat="server" Text="Responsable"></asp:Label>
            <asp:TextBox ID="TextBox3" runat="server" TextMode="MultiLine" Width="377px"></asp:TextBox><br />
            <asp:Label ID="Label4" runat="server" Text="Estado"></asp:Label>
            <asp:TextBox ID="TextBox4" runat="server" TextMode="MultiLine" Width="377px"></asp:TextBox><br />
            <asp:Label ID="Label5" runat="server" Text="EstadoId"></asp:Label>
            <asp:TextBox ID="TextBox5" runat="server" TextMode="MultiLine" Width="377px"></asp:TextBox><br />
            <asp:Button ID="Button1" runat="server" Text="Consulta" OnClick="Button1_Click" />

        </div>
    </form>
</body>
</html>
