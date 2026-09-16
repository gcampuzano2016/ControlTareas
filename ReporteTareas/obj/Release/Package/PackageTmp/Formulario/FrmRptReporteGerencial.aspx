<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="FrmRptReporteGerencial.aspx.cs" Inherits="ReporteTareas.Formulario.FrmRptReporteGerencial" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/ReporteV.js?v=6" type="text/javascript"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="panel-body">
                    <div class="row">
                        <h3>Reporte de Ventas</h3>
                    </div>
                </div>
            </div>
        </div>
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Registro
						<div style="display: none">
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                        </div>
                    </div>
                </div>
                <div class="panel-body">
                    <asp:Panel ID="Panel6" runat="server" Visible="true" Enabled="true">
                        <div class="col-lg-12" style="padding: 0px">
                            <div class="panel panel-default">
                                <div class="panel-heading">
                                    Filtros
                                </div>
                                <div class="panel-body">
                                    <div class="row">
                                        <div class="form-group col-lg-2">
                                            <label>Años</label>
                                            <select id="cboAnio1" class="form-control">
                                                <option value="0">-- SELECCIONE --</option>
                                                <option value="2022">2022</option>
                                            </select>
                                            <p class="help-block"></p>
                                        </div>
                                    </div>
                                </div>
                                <div class="panel-footer">
                                    <div class="" style="text-align: center">
                                        <button id="btnConsulta" onclick="BtnConsultaForeCast()" type="button" class="btn btn-primary">Consultar</button>
                                        <button id="btnDescargar" onclick="BtnDescargaForeCast()" type="button" class="btn btn-primary">Descargar XLS</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-lg-12" style="padding: 0px">
                            <div class="panel panel-default">
                                <div class="panel-heading">
                                    <div>
                                        <h4 class="" id="listPrincipalTitleLabel">Reporte Gerencial</h4>
                                    </div>
                                </div>
                                <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                    <%--  <div id="table-datosTablaPrincipal1" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                        <table id="table-users" class="table table-striped table-bordered table-hover dataTable no-footer dtr-inline">
                                            <thead>
                                                <th>Acciones</th>
                                                <th>SUCURSAL</th>
                                                <th>CUOTA ANUAL</th>
                                                <th>A LA FECHA</th>
                                                <th>% CUMPLIMIENTO A LA FECHA</th>
                                                <th>POR FACTURAR + RECURRENTE (BACKLOG)</th>
                                                <th>PROYECCIÓN TOTAL ANUAL</th>
                                                <th>PROYECCIÓN ANUAL %</th>
                                                <th>POR CERRAR FORECAST ONLINE</th>
                                                <th>PROYECCION FINAL TOTAL</th>
                                                <th>ESTIMADA 25% MARGEN</th>
                                                <th>A LA FECHA</th>
                                                <th>% CUMPLIMIENTO A LA FECHA</th>
                                                <th>POR FACTURAR + RECURRENTE (BACKLOG)</th>
                                                <th>PROYECCIÓN TOTAL ANUAL</th>
                                                <th>PROYECCIÓN CIERRE</th>
                                                <th>POR CERRAR FORECAST ONLINE</th>
                                                <th>PROYECCIÓN TOTAL</th>
                                                <th>PROYECCION ANUAL</th>
                                            </thead>
                                        </table>
                                    </div>--%>
                                    <!-- /.panel-heading -->
                                    <div class="panel-body" style="height: 450px; overflow-y: auto; overflow-x: auto; display: block;" id="Pagina1">
                                        <div id="table-datosTablaPrincipal1" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                            <div class="row">
                                                <div class="col-sm-12" id='datosTablaPrincipalForeCast'>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align: center">
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
        </asp:Panel>
    </div>
</asp:Content>
