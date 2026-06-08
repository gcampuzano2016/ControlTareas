<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="DashboardRebates.aspx.cs" Inherits="ReporteTareas.Formulario.DashboardRebates" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>

    <script src="../js/RebatesDashboard.js?v=1"></script>

    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>


    <!--link href="../dist/css/Rebates.css" rel="stylesheet" /-->
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

     <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content;">
        <div class="col-lg-12" style="background-color: #0D2538; padding-bottom:2rem; margin:0;">

            <div class="row" style=" margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs">

                    <ul class="breadcrumb" style="margin-top:1.5rem;">
                        <li style="display:flex; justify-content:space-between;">
                            <a href="#"><b>Rebates Dashboard</b></a>
                            <a href="#"><i class="glyphicon glyphicon-log-out"></i></a>
                        </li>
                    </ul>
                </div>
            </div>

            <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
                <div class="panel-default">
                        <div class="panel-heading" style="padding:0;border:none;">
                             <div style="display: none">
                                 <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                 <asp:TextBox ID="txtCodUnico" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                 <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                 <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                 <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                 <asp:TextBox ID="txtPerfil" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                             </div>
                        </div>

                        <!-- ******************************************************************** -->
                        <!--                            PAGINA                                    -->
                        <!-- ******************************************************************** -->
                                                                  

                        <div class="generalesInfoContratos" >

                            <!--div id="pestaniaPrincipal" class="pagina" style="display: block; padding: 0.5rem 0 0.2rem 0;"-->        
                                <!-- Cuadro Datos del Contrato  background-color: rgba(169, 14, 14, 0.50);-->
                                <!--div class="well fondo-azul" style="text-align:center; margin:10px 0 0 0; padding-top: 1rem; border:none;">
                                    <div class="well-header" style="margin-top: 0;">
                                        <h2 class="well-title" style="color:aliceblue;">Dashboard Rebates</h2>
                                    </div>                                    
                                </!--div>
                            </div-->

                            <div class="col-lg-12" style="display:flex; justify-content:center;">
                                <iframe src="https://app.powerbi.com/view?r=eyJrIjoiYjg1MDRiODItNjhhOC00OWU0LWFmODgtY2RiNGVmNWQ0MDkzIiwidCI6IjgwODc0OTA5LTMzMjUtNDEzMS1iNmFiLTdiMDVjZDEwNmJhZSIsImMiOjR9" frameborder="0" width="1000" height="600"></iframe>
                            </div>                      
                        </div>   
                </div>
            </asp:Panel>
        </div>
     </div>
</asp:Content>
