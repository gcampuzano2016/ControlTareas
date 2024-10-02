﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="DetalleContrato.aspx.cs" Inherits="ReporteTareas.Formulario.DetalleContrato" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/DetallesContrato.js?v=19" type="text/javascript"></script>
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
                        <h3>Detalle Contrato</h3>
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
                                <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            </div>
                    </div>
                    <div class="panel-body">
                        <!-- Nav tabs -->
                        <ul class="nav nav-pills">
                            <li class="active" id="tab1"><a href="#profile-pills" data-toggle="tab">Listas de Contratos</a>
                            </li>
                            <li id="tab2"><a href="#Accion-pills" data-toggle="tab">Ver detalle de Ticket Aranda</a>
                            </li>
                            <li><a href="#Total-pills" data-toggle="tab">Total de Horas por Número de OS</a>
                            </li>
                            <li id="tab3"><a href="#Mantenimiento-pills" data-toggle="tab">Detalle de Mantenimientos</a>
                            </li>	
                            <li id="tab4"><a href="#ReporteServicios-pills" data-toggle="tab">Reporte de servicios</a>
                            </li>	
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div class="tab-pane fade in active" id="profile-pills">
                                <asp:Panel ID="Panel6" runat="server" Visible="true" Enabled="true">
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                Filtros
                                            </div>
                                            <div class="panel-body">
                                                <div class="row">
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
                                                    <div class="form-group col-lg-2">
                                                        <label>Filtrar sin fecha</label>
                                                        <input class="form-check-input" type="checkbox" value="" id="idFiltros">
                                                    </div>
                                                    <div class="form-group col-lg-3" id="idGerente" runat="server">
                                                        <label>Gerente Cuenta</label>
                                                        <select id="cboGerente2" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>

                                                </div>
                                                <div class="row" id="Detalle" runat="server">
                                                    <div class="form-group col-lg-2">
                                                        <label>Gestor Responsable</label>
                                                        <select id="cboResponsable2" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Sucursal</label>
                                                        <select id="cboSucursal2" class="form-control">
                                                            <option value="SELECCIONAR SUCURSAL">SELECCIONAR SUCURSAL</option>
                                                            <option value="AMBATO">AMBATO</option>
                                                            <option value="CUENCA">CUENCA</option>
                                                            <option value="QUITO">QUITO</option>
                                                            <option value="GUAYAQUIL">GUAYAQUIL</option>
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Estado</label>
                                                        <select id="cboEstado2" class="form-control">
                                                            <option value="SELECCIONAR ESTADO">SELECCIONAR ESTADO</option>
                                                            <option value="EN EJECUCION">EN EJECUCION</option>
                                                            <option value="CERRADA">CERRADA</option>
                                                        </select>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Cliente</label>
                                                        <!--<select id="cboCliente2" class="form-control">
                                                        </select>-->
                                                        <input type="text" class="form-control" id="cboCliente2" placeholder="Cliente" oninput="BuscarCliente2()">
                                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes2">
                                                        </ul>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Area</label>
                                                        <select id="cboArea2" class="form-control">
                                                            <option value="SELECCIONAR AREA">SELECCIONAR AREA</option>
                                                            <option value="F5">F5</option>
                                                            <option value="CAS">CAS</option>
                                                            <option value="DATA CENTER">DATA CENTER</option>
                                                            <option value="NETWORKING">NETWORKING</option>
                                                            <option value="MULTIPLATAFORMA">MULTIPLATAFORMA</option>
                                                            <option value="SOFTWARE">SOFTWARE</option>
                                                            <option value="OUTSOURCING DE IMPRESION">OUTSOURCING DE IMPRESION</option>
                                                        </select>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="form-group col-lg-6">
                                                        <label>Número de Orden/Pedido</label>
                                                        <input type="text" class="form-control" id="txtNumeroOrden2">
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="panel-footer">
                                                <div class="" style="text-align: center">
                                                    <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-primary">Consultar</button>
                                                    <button id="btnDescarga" onclick="BtnDescarga()" type="button" class="btn btn-primary">Descargar XLS</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                <div>
                                                    <h4 class="" id="listPrincipalTitleLabel">Contratos</h4>
                                                </div>
                                            </div>
                                            <!-- /.panel-heading -->
                                            <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
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
                                </asp:Panel>
                            </div>
                            <div class="tab-pane fade" id="Accion-pills">
                                <div class="row">
                                    <div class="col-lg-12" style="padding: 20px">
                                        <div class="panel-body">
                                            <div class="row">
                                                <h3>Estados de las Tareas</h3>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <asp:Panel ID="Panel7" runat="server" Visible="true" Enabled="true">
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                Filtros
                                            </div>
                                            <div class="panel-body">
                                                <div class="row">
                                                    <div class="form-group col-lg-5">
                                                        <label>Fecha desde:</label>
                                                        <input type="text" class="form-control" id="txtFechaConsulta3">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-5">
                                                        <label>Fecha Hasta:</label>
                                                        <input type="text" class="form-control" id="txtFechaConsulta4">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Filtrar sin fecha</label>
                                                        <input class="form-check-input" type="checkbox" value="" id="idFiltros2">
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="form-group col-lg-6">
                                                        <label>Numero de OS</label>
                                                        <input type="text" class="form-control" id="txtNumeroOrden">
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                                                </div>
                                            </div>
                                            <div class="panel-footer">
                                                <div class="" style="text-align: center">
                                                    <button id="btnConsulta" onclick="BtnConsultaOrden()" type="button" class="btn btn-primary">Consultar</button>
                                                    <button id="btnDescarga" onclick="BtnDescargaOrden()" type="button" class="btn btn-primary">Descargar XLS</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                <div>
                                                    <h4 class="" id="listPrincipalTitleLabel">Listado de Ticket</h4>
                                                </div>
                                            </div>
                                            <!-- /.panel-heading -->
                                            <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                                <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                    <div class="row">

                                                        <div class="col-sm-12" id='datosTablaPrincipal2' style="padding: 0px">
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
                                </asp:Panel>
                            </div>
                            <div class="tab-pane fade" id="Total-pills">
                                <div class="row">
                                    <div class="col-lg-12" style="padding: 20px">
                                        <div class="panel-body">
                                            <div class="row">
                                                <h3>Horas de totales por OS</h3>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <asp:Panel ID="Panel8" runat="server" Visible="true" Enabled="true">
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                Filtros
                                            </div>
                                            <div class="panel-body">
                                                <div class="row">
                                                    <div class="form-group col-lg-5">
                                                        <label>Fecha desde:</label>
                                                        <input type="text" class="form-control" id="txtFechaConsulta5">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-5">
                                                        <label>Fecha Hasta:</label>
                                                        <input type="text" class="form-control" id="txtFechaConsulta6">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Filtrar sin fecha</label>
                                                        <input class="form-check-input" type="checkbox" value="" id="idFiltros3">
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="form-group col-lg-6">
                                                        <label>Cliente</label>
                                                        <select id="cboCliente3" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>													
                                                </div>												
                                                <div class="row">
                                                    <div class="form-group col-lg-6">
                                                        <label>Número de OS</label>
                                                        <input type="text" class="form-control" id="txtNumeroOrdenOS">
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                                                </div>
                                                <div class="form-group col-lg-3" id="txt1">
                                                     <label>Horas Contratadas:</label>
                                                     <input type="text" class="form-control" id="txtHorasGeneradas1" disabled>
                                                     <p class="help-block"></p>
                                                </div>	
                                                <div class="form-group col-lg-3" id="txt2">
                                                     <label>Horas Entregada:</label>
                                                     <input type="text" class="form-control" id="txtHorasEntregadas1" disabled>
                                                     <p class="help-block"></p>
                                                </div>	
                                                <div class="form-group col-lg-3" id="txt3">
                                                     <label>Horas Disponible:</label>
                                                     <input type="text" class="form-control" id="txtHorasDisponibles1" disabled>
                                                     <p class="help-block"></p>
                                                </div>													
                                            </div>
                                            <div class="panel-footer">
                                                <div class="" style="text-align: center">
                                                    <button id="btnConsulta" onclick="BtnConsultaOS()" type="button" class="btn btn-primary">Consultar</button>
                                                    <button id="btnDescarga" onclick="BtnDescargaOS()" type="button" class="btn btn-primary">Descargar XLS</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                <div>
                                                    <h4 class="" id="listPrincipalTitleLabel">Reporte de Horas Consumida por OS</h4>
                                                </div>
                                            </div>
                                            <!-- /.panel-heading -->
                                            <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                                <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                    <div class="row">

                                                        <div class="col-sm-12" id='datosTablaPrincipal4' style="padding: 0px">
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
                                </asp:Panel>
                            </div>
                            <div class="tab-pane fade" id="Mantenimiento-pills">
                                <div class="row">
                                    <div class="col-lg-12" style="padding: 20px">
                                        <div class="panel-body">
                                            <div class="row">
                                                <h3>Lista de Mantenimentos</h3>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <asp:Panel ID="Panel9" runat="server" Visible="true" Enabled="true">
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                Filtros
                                            </div>
                                            <div class="panel-body">
                                                <div class="row">
                                                    <div class="form-group col-lg-5">
                                                        <label>Fecha desde:</label>
                                                        <input type="text" class="form-control" id="txtFechaConsulta7">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-5">
                                                        <label>Fecha Hasta:</label>
                                                        <input type="text" class="form-control" id="txtFechaConsulta8">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Filtrar sin fecha</label>
                                                        <input class="form-check-input" type="checkbox" value="" id="idFiltros4">
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="form-group col-lg-6">
                                                        <label>Cliente</label>
                                                        <select id="cboCliente4" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>	
                                                    <div class="form-group col-lg-6">
														<label>Estado:</label>
														<select id="cboClasificacion4" class="form-control">
															<option value="0">-- SELECCIONE --</option>
															<option value="1">REALIZADO</option>
															<option value="2">PLANIFICADO</option>
															<option value="3">EN EJECUCIÓN</option>
															<option value="4">POR PLANIFICAR</option>
														</select>									
														<p class="help-block"></p>
                                                    </div>														
                                                </div>												
                                                <div class="row">
                                                    <div class="form-group col-lg-6">
                                                        <label>Número de OS</label>
                                                        <input type="text" class="form-control" id="txtNumeroOrdenOS4">
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                                                </div>												
                                            </div>
                                            <div class="panel-footer">
                                                <div class="" style="text-align: center">
                                                    <button id="btnConsulta" onclick="BtnConsultaOSM()" type="button" class="btn btn-primary">Consultar</button>
                                                    <button id="btnDescarga" onclick="BtnDescargaOSM()" type="button" class="btn btn-primary">Descargar XLS</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                <div>
                                                    <h4 class="" id="listPrincipalTitleLabel">Reporte de Mantenimiento</h4>
                                                </div>
                                            </div>
                                            <!-- /.panel-heading -->
                                            <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                                <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                    <div class="row">

                                                        <div class="col-sm-12" id='datosTablaPrincipal5' style="padding: 0px">
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
                                </asp:Panel>
                            </div>		

                            <!--- AUMENTADO-->
                <div class="tab-pane fade" id="ReporteServicios-pills">
                                <div class="row">
                                    <div class="col-lg-12" style="padding: 20px">
                                        <div class="panel-body">
                                            <div class="row">
                                                <h3>Lista de Servicios</h3>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <asp:Panel ID="Panel1" runat="server" Visible="true" Enabled="true">
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                Filtros
                                            </div>
                                            <div class="panel-body">
                                                <div class="row">
                                                    <div class="form-group col-lg-4" id="divCmbAnio">
                                                        <label>AÑO:</label>
                                                        <select id="cboAnioServicios" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                        </div>

                                                     <div class="form-group col-lg-4" id="divCmbGestor">
                                                        <label>GESTOR RESPONSABLE:</label>
                                                        <select id="cboGestor" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>

                                                    <div class="form-group col-lg-2">
                                                        <label>SUCURSAL:</label>
                                                        <select id="cboSucursalReporte" class="form-control">
                                                            <option value="SELECCIONAR SUCURSAL">--SELECCIONE--</option>
                                                            <option value="AMBATO">AMBATO</option>
                                                            <option value="CUENCA">CUENCA</option>
                                                            <option value="QUITO">QUITO</option>
                                                            <option value="GUAYAQUIL">GUAYAQUIL</option>
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    
                                                </div>
                                                <div class="row" id="Div2" runat="server">
                                                   
                                                    
                                                    <div class="form-group col-lg-2">
                                                        <label>ESTADO:</label>
                                                        <select id="cboEstadoReporte" class="form-control">
                                                            <option value="SELECCIONAR ESTADO">--SELECCIONE--</option>
                                                            <option value="EN EJECUCION">EN EJECUCION</option>
                                                            <option value="CERRADA">CERRADA</option>
                                                        </select>
                                                    </div>
                                                   
                                                    <div class="form-group col-lg-3">
                                                        <label>AREA:</label>
                                                        <select id="cboAreaReporte" class="form-control">
                                                            <option value="SELECCIONAR AREA">--SELECCIONE--</option>
                                                            <option value="F5">F5</option>
                                                            <option value="CAS">CAS</option>
                                                            <option value="DATA CENTER">DATA CENTER</option>
                                                            <option value="NETWORKING">NETWORKING</option>
                                                            <option value="MULTIPLATAFORMA">MULTIPLATAFORMA</option>
                                                            <option value="SOFTWARE">SOFTWARE</option>
                                                            <option value="OUTSOURCING DE IMPRESION">OUTSOURCING DE IMPRESION</option>
                                                        </select>
                                                    </div>

                                                    <div class="form-group col-lg-3" id="idGerente2" runat="server">
                                                        <label>Gerente Cuenta</label>
                                                        <select id="cboGerente3" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>
                                                
                                                   														
                                                </div>												
                                                
                                                <div class="row">
                                                    <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                                                </div>												
                                            </div>

                            <!-- /.modal -->

                                            <div class="panel-footer">
                                                <div class="" style="text-align: center">
                                                    <button id="btnConsulta" onclick="BtnConsultaReporte()" type="button" class="btn btn-primary">Consultar</button>
                                                    <button id="btnDescarga" onclick="BtnDescargaReporte()" type="button" class="btn btn-primary">Descargar XLS</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                <div>
                                                    <h4 class="" id="listPrincipalTitleLabel">Reporte de Servicios</h4>
                                                </div>
                                            </div>
                                            <!-- /.panel-heading -->
                                            <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                                <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                    <div class="row">

                                                        <div class="col-sm-12" id='datosTablaPrincipal6' style="padding: 0px">
                                                        </div>
                                                       <div class="col-sm-12" id='datosTablaPrincipal7'  style="padding: 0px" >
                                                        
                                                        </div>
                                                        <div class="col-sm-12" id='datosTablaPrincipal8' style="padding: 0px">
                                                        </div>

                                                        <!-- /.table-responsive -->
                                                        <!-- /.panel-body -->
                                                    </div>
                                                    <!-- /.panel -->
                                                </div>
                                                <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align: center">
                                                </div>

                                                
                                            </div>

                                            <div class="panel-footer">
                                                             <div class="" style="text-align: center ; display:none " id='botonRegresar'>
                                                                <button id="btnRegresar" onclick="BtnRegresar()" type="button" class="btn btn-primary">Regresar</button>
                                                        </div>
                                                <div class="" style="text-align: center ; display:none " id='botonRegresarTabla7'>
                                                                <button id="btnRegresarTabla7" onclick="BtnRegresarTabla7()" type="button" class="btn btn-primary">Regresar</button>
                                                        </div>
                                                            </div>

                                        </div>
                                    </div>
                                </asp:Panel>
                            </div>		


                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Modal -->
        <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-labelledby="modalMensajeInformativoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeInformativoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="myModalLabel">Informativo</h4>
                    </div>
                    <div class="modal-body" id="MensajeInformativo">
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </div>
</asp:Content>



<%--<asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True"></asp:DropDownList>--%>

