<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ReporteSalud.aspx.cs" Inherits="ReporteTareas.Formulario.ReporteSalud" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <script src="../js/ReporteSalud.js?v=1"></script>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <link href="../bower_components/sweetalert/css/animate.css" rel="stylesheet" />
    <link href="../bower_components/sweetalert/css/sweetalert2.min.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert2.all.min.js"></script>

    <link href="../dist/css/depMedico.css" rel="stylesheet" />

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content;">
        <div class="col-lg-12" style="background-color: #0D2538;padding-bottom: 1.5rem;padding-top: 0.5rem;">

            <div class="row" style="background-color: #0D2538; margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs">
                    <ul class="breadcrumb">
                        <li>
                            <a href="#"><b>Dashboard Departamento Médico</b></a>
                        </li>
                    </ul>
                </div>
            </div>

            <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
                 <div class="panel-default">
                    <div>
                        <div style="display: none">
                            <asp:TextBox ID="hiddenCedulaField" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtPerfil" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                        </div>
                    </div>
                </div>

                <!-- ******************************************************************** -->
                <!--                            PAGINA                                    -->
                <!-- ******************************************************************** -->
                       
                <div class="col-lg-12" style="background-color: #0D2538; padding:0.5rem 0 1.5rem 0;">
                    
                   <div class="col-lg-12" style="width: 100%; height: 100%; padding:0;">
                        <iframe
                            id="inlineFrameExample"
                            title="Informe Dep Medico"
                            style="width: 100%; height:630px; border: none;"
                            src="https://app.powerbi.com/view?r=eyJrIjoiOGFhZmU3MDItZWZkZC00ZmFjLTlhMDAtYmJlMzk2NjQ0MGQ5IiwidCI6IjgwODc0OTA5LTMzMjUtNDEzMS1iNmFiLTdiMDVjZDEwNmJhZSIsImMiOjR9&filterPaneEnabled=false&navContentPaneEnabled=false&viewMode=fitToWidth">
                        </iframe>
                    </div>



                </div>

                <div class="col-lg-12" id="btnCarga" style=" background-color: #0D2538; justify-content:center; margin: 2rem;">
                    <button class="btn btn-info col-sm-6" type="button" id="btn-buscarInfoHis" style="width: 25%; margin-left:5rem;">Cargar Datos</button>
                </div>

                
            </asp:Panel>         

        </div>
          
     </div>

</asp:Content>
