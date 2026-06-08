<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="RegistroProyecto.aspx.cs" Inherits="ReporteTareas.Formulario.RegistroProyecto" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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
                        <h3>Registro Proyecto</h3>
                    </div>
                </div>
            </div>
        </div>
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Filtros
                    </div>
                    <div class="panel-body">
                        <div class="row">
                            <div class="form-group col-lg-4">
                                <label>CÓDIGO DE PROCESO DE VENTA:</label>
                                <input type="text" class="form-control" id="txtCodigoProcesoVentaFiltro">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-2">
                                <label>CONTRATO:</label>
                                <input type="text" class="form-control" id="txtContratoFiltro">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-4">
                                <label>DESCRIPCION CONTRATO</label>
                                <input type="text" class="form-control" id="txtDescripcionFiltro">
                                <p class="help-block"></p>
                            </div>
                        </div>
                        <div class="row">
                            <div class="form-group col-lg-4">
                                <label>CLIENTE</label>
                                <input type="text" class="form-control" id="txtClienteFiltro">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-4">
                                <label>OS</label>
                                <input type="text" class="form-control" id="txtOSFiltro">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-4">
                                <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-primary">Consultar</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <div style="display: none">
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                        </div>
                    </div>
                    <div class="panel-body">
                        <!-- Nav tabs -->
                        <ul class="nav nav-pills">
                            <li class="active"><a href="#home-pills" data-toggle="tab">INFORMACIÓN DE CONTRATO</a>
                            </li>
                            <li><a href="#profile-pills" data-toggle="tab">FORMA DE PAGO</a>
                            </li>
                            <li><a href="#Accion-pills" data-toggle="tab">PLAZO</a>
                            </li>
                            <li><a href="#Total-pills" data-toggle="tab">MULTAS</a>
                            </li>
                            <li><a href="#Costo-pills" data-toggle="tab">DETALLE EQUIPOS</a>
                            </li>
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div class="tab-pane fade in active" id="home-pills">
                                <div class="row">
                                    <div class="col-lg-12" style="padding: 20px">
                                        <div class="panel-body">
                                            <div class="row">
                                                <h3>Información de Contrato</h3>
                                            </div>
                                            <div class="row">
                                                <div class="form-group col-lg-4">
                                                    <label>CÓDIGO DE PROCESO DE VENTA:</label>
                                                    <input type="text" class="form-control" id="txtCodigoProcesoVenta">
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="form-group col-lg-2">
                                                    <label>CONTRATO:</label>
                                                    <input type="text" class="form-control" id="txtContrato">
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="form-group col-lg-4">
                                                    <label>DESCRIPCION CONTRATO</label>
                                                    <input type="text" class="form-control" id="txtDescripcion">
                                                    <p class="help-block"></p>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="form-group col-lg-4">
                                                    <label>CLIENTE</label>
                                                    <input type="text" class="form-control" id="txtCliente">
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="form-group col-lg-4">
                                                    <label>OS</label>
                                                    <input type="text" class="form-control" id="txtOS">
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="form-group col-lg-4">
                                                    <button id="btnGuardar" onclick="BtnConsulta()" type="button" class="btn btn-primary">Guardar</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="tab-pane fade" id="profile-pills">
                                <div class="row">
                                    <div class="col-lg-12" style="padding: 20px">
                                        <div class="panel-body">
                                            <div class="row">
                                                <h3>Forma de Pago</h3>
                                            </div>
                                            <div class="row">
                                                <div class="form-group col-lg-4">
                                                    <label>DESGLOSE:</label>
                                                    <input type="text" class="form-control" id="txtDesglose">
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="form-group col-lg-2">
                                                    <label>VALOR:</label>
                                                    <input type="number" class="form-control" id="txtValor">
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="form-group col-lg-4">
                                                    <button id="btnAgregarFormaPago" onclick="BtnAgregarFormaPago()" type="button" class="btn btn-primary">Agregar</button>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="form-group col-lg-4">
                                                    <button id="btnGuardarFormaPago" onclick="BtnGuardarFormaPago()" type="button" class="btn btn-primary">Guardar</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="tab-pane fade" id="Accion-pills">
                                <div class="row">
                                    <div class="col-lg-12" style="padding: 20px">
                                        <div class="panel-body">
                                            <div class="row">
                                                <h3>Plazo</h3>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="tab-pane fade" id="Total-pills">
                                <div class="row">
                                    <div class="col-lg-12" style="padding: 20px">
                                        <div class="panel-body">
                                            <div class="row">
                                                <h3>Multas</h3>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="tab-pane fade" id="Costo-pills">
                                <div class="row">
                                    <div class="col-lg-12" style="padding: 20px">
                                        <div class="panel-body">
                                            <div class="row">
                                                <h3>Detalle de Equipo</h3>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
