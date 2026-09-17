<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="MiPerfil.aspx.cs" Inherits="ReporteTareas.Formulario.MiPerfil" ResponseEncoding="utf-8" %>
<%@ Register Src="~/Controles/PerfilFichas.ascx" TagPrefix="rta" TagName="PerfilFichas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/miPerfil.js?v=9" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%-- El page-wrapper va aca y no dentro del control: es un id, y solo puede
         haber uno por pagina. Es el que despeja los 250px del menu lateral. --%>
    <div id="page-wrapper" style="padding: 0px">
        <rta:PerfilFichas runat="server" ID="fichas" />
    </div>
</asp:Content>
