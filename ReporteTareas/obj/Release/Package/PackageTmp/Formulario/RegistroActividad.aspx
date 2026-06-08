<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="RegistroActividad.aspx.cs" Inherits="ReporteTareas.Formulario.RegistroActividad" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/registroActividad.js?v=6" type="text/javascript"></script>

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
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Registro de Actividad</h3>
                    </div>
                </div>
            </div>
        </div>
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <div style="display: none">
                <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
            </div>
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-body">
                        <!-- Nav tabs -->
                        <ul class="nav nav-pills">
                            <li class="active" id="tab1"><a href="#home-pills" data-toggle="tab">Registro de Actividad</a>
                            </li>
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div class="tab-pane fade in active" id="home-pills">
                                <div class="panel panel-default">
                                    <div class="panel-body">
                                        <div class="row">
                                            <div class="panel panel-default">
                                                <div class="panel-heading">
                                                    <label>Registro de Actividad</label>
                                                </div>
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>Fecha desde:</label>
                                                <input type="text" class="form-control" id="txtFechaConsulta1">
                                                <p class="help-block"></p>
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>Fecha Hasta:</label>
                                                <input type="text" class="form-control" id="txtFechaConsulta2">
                                                <p class="help-block"></p>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel-footer">
                                        <div class="" style="text-align: center">
                                            <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-primary">Consultar</button>
                                            <button id="btnAgregar" onclick="BtnAgregarAsistencia()" type="button" class="btn btn-primary">Agregar Actividad</button>
                                            <%--<button id="btnDescargar" onclick="BtnDescargar()" type="button" class="btn btn-primary">Descargar xls</button>--%>
                                        </div>
                                    </div>
                                    <div class="col-lg-12" style="padding: 0px" id="ListaAsistencia">                                     
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                <div>
                                                    <div style="float: right">
                                                    </div>
                                                    <h4 class="" id="listPrincipalTitleLabel">Resumen de Actividad</h4>
                                                </div>
                                            </div>
                                            <!-- /.panel-heading -->
                                            <div class="panel-body" style="height: 450px; overflow-y: auto; overflow-x: auto;">
                                                <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                    <div class="row">

                                                        <div class="col-sm-12" id='datosTablaPrincipal' style="padding: 0px">
                                                        </div>
                                                        <!-- /.table-responsive -->
                                                        <!-- /.panel-body -->
                                                    </div>
                                                    <!-- /.panel -->
                                                </div>
                                                <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align: center">
                                                </div>
                                            </div>

                                        </div>
                                    </div>
                                    <div class="col-lg-12" style="padding: 0px" id="AgregarAsistencia">
                                        <div class="panel panel-default">
                                            <div class="panel-body">
                                                <div class="row">
                                                    <div class="form-group col-lg-2">
                                                        <label>Fecha Inicio</label>
                                                        <input type="text" class="form-control" id="txtFechaEntrada" placeholder="Fecha entrada">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Fecha Final</label>
                                                        <input type="text" class="form-control" id="txtFechaSalida" placeholder="Fecha entrada">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Fecha Final Actividad</label>
                                                        <input type="text" class="form-control" id="txtFechaSalidaAct" placeholder="Fecha entrada">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Estado</label>
                                                        <select id="cboTipoFecha" class="form-control">
                                                            <option value="1">ACTIVO</option>
                                                            <option value="0">INACTIVO</option>
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="form-group col-lg-12">
                                                        <label>Observación</label>
                                                        <textarea class="form-control" id="txtObservacion" name="txtObservacion" rows="2" cols="50"></textarea>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="form-group col-lg-8">
                                                    </div>
                                                    <div class="form-group col-lg-1">
                                                        <button id="btnNuevo" type="button" class="btn btn-primary">Guardar</button>
                                                    </div>
                                                    <div class="form-group col-lg-1">
                                                        <button id="btnGuardar" onclick="RegresarLista()" type="button" class="btn btn-primary">Regresar</button>
                                                    </div>
                                                </div>
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
