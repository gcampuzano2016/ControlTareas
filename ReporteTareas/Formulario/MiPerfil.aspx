<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="MiPerfil.aspx.cs" Inherits="ReporteTareas.Formulario.MiPerfil" ResponseEncoding="utf-8" %>
<%@ Register Src="~/Controles/PerfilFichas.ascx" TagPrefix="rta" TagName="PerfilFichas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/miPerfil.js?v=8" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <rta:PerfilFichas runat="server" ID="fichas" />
</asp:Content>
