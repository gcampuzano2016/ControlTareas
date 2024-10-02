<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="RegistroRebates.aspx.cs" Inherits="ReporteTareas.Formulario.RegistroRebates" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/RegistroRebates.js?v=2" type="text/javascript"></script>
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
                        <h3>Registro</h3>
                    </div>
                </div>
            </div>
        </div>
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Rebates
                            <div style="display: none">
                                <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            </div>
                    </div>
                    <div class="panel-body">
                        <!-- Nav tabs -->
                        <ul class="nav nav-pills">
                            <li class="active"><a href="#home-pills" data-toggle="tab">Registro de Rebetes</a>
                            </li>
                            <li><a href="#profile-pills" data-toggle="tab">Lista de Rebates</a>
                            </li>
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div class="tab-pane fade in active" id="home-pills">
                                <div class="row">
                                    <div class="form-group col-lg-4" style="display: none">
                                        <button id="btnBanco" onclick="BtnBanco()" type="button" class="btn btn-success">Registro Banco</button>
                                        <button id="btnFiscal" onclick="BtnAnioFiscal()" type="button" class="btn btn-success">Registro Año Fiscal</button>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-2">
                                        <label>Fecha Registro</label>
                                        <input type="text" class="form-control" id="txtFechaDesde" placeholder="Fecha Registro">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Id Actividad</label>
                                        <input type="text" class="form-control" id="txtTransaccion" placeholder="Id Actividad">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-7">
                                        <label>Programa</label>
                                        <input type="text" class="form-control" id="txtPrograma" placeholder="Programa">
                                        <p class="help-block"></p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-2">
                                        <label>Tipo Ingreso</label>
                                        <select id="cboTipoIngreso" class="form-control">
                                        </select>
                                        <!--<input type="text" class="form-control"  id="cboTipoIngreso" placeholder="Tipo Ingreso" oninput="BuscarTipoIngreso()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboTipoIngreso">
                                        </ul>-->
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-1">
                                        <label></label>
                                        <button class="btn btn-lg btn-success" onclick="BtnCrearTipoIngreso()" type="button">
                                            <i class="ace-icon glyphicon-plus  bigger-110 icon-only"></i>
                                        </button>
                                    </div>
                                    <div class="form-group col-lg-2">
                                        <label>Invertido</label>
                                        <input class="form-check-input" type="checkbox" value="" id="IdProceso">
                                    </div>
                                    <div class="form-group col-lg-2">
                                        <label>Marca</label>
                                        <select id="cboMarca" class="form-control">
                                        </select>
                                        <!--<input type="text" class="form-control"  id="cboMarca" placeholder="Marca" oninput="BuscarMarca()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboMarca">
                                        </ul>-->
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-1">
                                        <label></label>
                                        <button class="btn btn-lg btn-success" onclick="BtnCrearMarca()" type="button">
                                            <i class="ace-icon glyphicon-plus  bigger-110 icon-only"></i>
                                        </button>
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Reponsable</label>
                                        <input type="text" class="form-control" id="txtReponsable" placeholder="Reponsable" value="KARLA ZURITA">
                                        <p class="help-block"></p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-12">
                                        <label>Descripción</label>
                                        <textarea class="form-control" id="txtDetalle" name="txtDetalle" rows="4" cols="50"></textarea>
                                        <p class="help-block"></p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-2">
                                        <label>Valor</label>
                                        <input type="text" class="form-control" id="txtValor" placeholder="Valor">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-2" style="display: block">
                                        <label>Banco</label>
                                        <select id="cboBanco" class="form-control">
                                        </select>
                                        <!--<input type="text" class="form-control"  id="cboBanco" placeholder="Banco" oninput="BuscarcboBanco()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="combocboBanco">
                                        </ul>-->
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-2">
                                        <label>Q Fabricante</label>
                                        <input type="text" class="form-control" id="txtQFabricante" placeholder="Q Fabricante">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-1">
                                        <label></label>
                                        <button class="btn btn-lg btn-success" onclick="BtnSeleccionarQF()" type="button">
                                            <i class="ace-icon glyphicon-plus  bigger-110 icon-only"></i>
                                        </button>
                                    </div>
                                    <div class="form-group col-lg-2">
                                        <label>Tipo de Pago</label>
                                        <select id="cboTipoPago" class="form-control">
                                        </select>
                                        <!--<input type="text" class="form-control"  id="cboTipoPago" placeholder="Tipo de Pago" oninput="BuscarTipoPago()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboTipoPago">
                                        </ul>-->
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-1">
                                        <label></label>
                                        <button class="btn btn-lg btn-success" onclick="BtnCrearTipoPago()" type="button">
                                            <i class="ace-icon glyphicon-plus  bigger-110 icon-only"></i>
                                        </button>
                                    </div>
                                    <div class="form-group col-lg-2">
                                        <label>Estado</label>
                                        <select id="cboEstado" class="form-control">
                                            <option value="SELECCIONAR">SELECCIONAR</option>
                                            <option value="PAGADO">PAGADO</option>
                                            <option value="NO CUMPLIDO">NO CUMPLIDO</option>
                                            <option value="PENDIENTE">PENDIENTE</option>
                                        </select>
                                        <p class="help-block"></p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-3">
                                        <label>Easy Pay Transaccion</label>
                                        <input type="text" class="form-control" id="txtIdTransaccion2" placeholder="Easy Pay Transaccion">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Id Pago</label>
                                        <input type="text" class="form-control" id="txtIdPago" placeholder="Id Pago">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Fecha Pago</label>
                                        <input type="text" class="form-control" id="txtFechaPago" placeholder="Fecha Pago">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Fecha Estimada de Pago</label>
                                        <input type="text" class="form-control" id="txtFechaEstimadaPago" placeholder="Fecha Estimada de Pago">
                                        <p class="help-block"></p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-12">
                                        <label>NOTAS / OBSERVACIONES</label>
                                        <textarea class="form-control" id="txtObservacion" name="txtObservacion" rows="4" cols="50"></textarea>
                                        <p class="help-block"></p>
                                    </div>
                                </div>
                                <div class="row" style="display: none">
                                    <div class="form-group col-lg-8">
                                        <form role="form">
                                            <div class="col-lg-8">
                                                <label>Adjuntar Archivos:</label>
                                                <input type="file" id="archivosAdjuntos" multiple />
                                                <p class="help-block"></p>
                                                <progress id="fileProgress" style="display: none"></progress>
                                                <hr />
                                                <span id="lblMessage" style="color: Green"></span>
                                            </div>
                                            <div class="col-lg-4">
                                                <br>
                                                <input type="button" id="btnCargarArchivosAdjuntos" value="Cargar Archivos" class="btn btn-info" />
                                                <p class="help-block"></p>
                                            </div>

                                            <div class="col-lg-12" id="divArchivosAdjuntosAnteriores">
                                            </div>
                                            <div class="col-lg-12" id="divArchivosAdjuntos">
                                            </div>
                                            <div class="col-lg-12">
                                                <p class="help-block" id="messageNotify" style="display: none;"></p>
                                            </div>
                                        </form>
                                    </div>
                                </div>
                                <div class="panel-footer">
                                    <div class="" style="text-align: right">
                                        <button id="btnGuardar" onclick="GuardarNuevoRebates()" type="button" class="btn btn-danger" title="Guardar un registro">Generar Nuevo</button>
                                        <button id="btnActualizar" onclick="ActualizarRebates()" type="button" class="btn btn-warning" title="Actualizar un registro">Actualizar</button>
                                        <button id="btnEliminar" onclick="EliminarRebates()" type="button" class="btn btn-primary" title="Eliminar un registro">Eliminar</button>
                                    </div>
                                </div>
                                <div class="row">
                                    <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                                </div>
                            </div>
                            <div class="tab-pane fade" id="profile-pills">
                                <asp:Panel ID="Panel6" runat="server" Visible="true" Enabled="true">
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                Filtros
                                            </div>
                                            <div class="panel-body">
                                                <div class="row">
                                                    <div class="form-group col-lg-3">
                                                        <label>Tipo Fecha</label>
                                                        <select id="cboFecha" class="form-control">
                                                            <!--<option value="1">Fecha Registro Actividad</option>-->
                                                            <option value="2">Fecha Estimada de Pago</option>
                                                            <option value="3">Fecha Pago</option>
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>
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
                                                </div>
                                                <div class="row">
                                                    <div class="form-group col-lg-2">
                                                        <label>Tipo Ingreso</label>
                                                        <select id="cboTipoIngreso2" class="form-control">
                                                        </select>
                                                        <!--<input type="text" class="form-control"  id="cboTipoIngreso2" placeholder="Tipo Ingreso" oninput="BuscarTipoIngreso2()">
														<ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboTipoIngreso2">
														</ul>-->
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Marca</label>
                                                        <select id="cboMarca2" class="form-control">
                                                        </select>
                                                        <!--<input type="text" class="form-control"  id="cboMarca2" placeholder="Marca" oninput="BuscarMarca2()">
														<ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboMarca2">
														</ul>-->
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Tipo de Pago</label>
                                                        <select id="cboTipoPago2" class="form-control">
                                                        </select>
                                                        <!--<input type="text" class="form-control"  id="cboTipoPago2" placeholder="Tipo de Pago" oninput="BuscarTipoPago2()">
														<ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboTipoPago2">
														</ul>-->
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Estado</label>
                                                        <select id="cboEstado2" class="form-control">
                                                            <option value="SELECCIONAR">SELECCIONAR</option>
                                                            <option value="PAGADO">PAGADO</option>
                                                            <option value="NO CUMPLIDO">NO CUMPLIDO</option>
                                                            <option value="PENDIENTE">PENDIENTE</option>
                                                        </select>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>AÑO</label>
                                                        <%-- <select id="cboAnio" class="form-control">
                                                        </select>--%>
                                                        <select id="cboAnio" class="js-example-basic-multiple" name="cierre[]" multiple="multiple">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Q</label>
                                                        <select id="cboQ" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
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
                                                    <h4 class="" id="listPrincipalTitleLabel">Lista de Rebates</h4>
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


        <!-- Modal -->
        <div class="modal fade" id="modalMensajeAprobar" tabindex="-1" role="dialog" aria-labelledby="modalMensajeAprobarLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #d9edf7" id="modalMensajeAprobarTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalMensajeAprobarLabelTitulo">Registro Cliente</h4>
                    </div>
                    <div class="modal-body" id="divMensajeAprobar">
                        <div class="row">
                            <div class="form-group col-lg-12">
                                <label>Descripción:</label>
                                <!--<input type="text" class="form-control" id="txtObservacionReq">-->
                                <textarea class="form-control" id="txtObservacionReq" name="txtObservacionReq" rows="4" cols="50"></textarea>
                                <p class="help-block"></p>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" style="display: none;" id="txtCodigo">
                        <button type="button" class="btn btn-danger" data-dismiss="modal">Close</button>
                        <button type="button" id="btnAceptarAprobar" onclick="GuardarCliente()" class="btn btn-primary">Guardar</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <!-- Modal -->
        <div class="modal fade" id="modalMensajeAprobarBanco" tabindex="-1" role="dialog" aria-labelledby="modalMensajeAprobarLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #d9edf7" id="modalMensajeAprobarTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalMensajeAprobarLabelTitulo">Registro Banco</h4>
                    </div>
                    <div class="modal-body" id="divMensajeAprobar">
                        <div class="row">
                            <div class="form-group col-lg-12">
                                <label>Descripción:</label>
                                <input type="text" class="form-control" id="txtRegistroBanco">
                                <p class="help-block"></p>
                            </div>
                        </div>
                        <div class="row">
                            <div class="form-group col-lg-12">
                                <label>Porcentaje de Transferencia:</label>
                                <input type="text" class="form-control" id="txtPorcentaje">
                                <p class="help-block"></p>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" style="display: none;" id="txtCodigo">
                        <button type="button" class="btn btn-danger" data-dismiss="modal">Close</button>
                        <button type="button" id="btnAceptarAprobar" onclick="GuardarBanco()" class="btn btn-primary">Guardar</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <!-- Modal -->
        <div class="modal fade" id="modalMensajeAprobarAnioFiscal" tabindex="-1" role="dialog" aria-labelledby="modalMensajeAprobarLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #d9edf7" id="modalMensajeAprobarTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalMensajeAprobarLabelTitulo">Registro Año Fiscal</h4>
                    </div>
                    <div class="modal-body" id="divMensajeAprobar">
                        <!-- Nav tabs -->
                        <ul class="nav nav-pills">
                            <li class="active"><a href="#RFiscal-pills" data-toggle="tab">Registro fiscal</a>
                            </li>
                            <li><a href="#RFLista-pills" data-toggle="tab">Lista de registro fislcal</a>
                            </li>
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div class="tab-pane fade in active" id="RFiscal-pills">
                                <div class="row">
                                    <div class="form-group col-lg-4">
                                        <label>Marca</label>
                                        <input type="text" class="form-control" id="cboMarca3" placeholder="Marca" oninput="BuscarMarca3()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboMarca3">
                                        </ul>
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-8">
                                        <label>Descripción:</label>
                                        <input type="text" class="form-control" id="txtDescripcionQ">
                                        <p class="help-block"></p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-6">
                                        <label>Fecha Inicio:</label>
                                        <input type="text" class="form-control" id="txtFechaInicio">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-6">
                                        <label>Fecha Final:</label>
                                        <input type="text" class="form-control" id="txtFechaFinal">
                                        <p class="help-block"></p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-6">
                                        <label>Fecha Inicio DOS:</label>
                                        <input type="text" class="form-control" id="txtFechaInicioDOS">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-6">
                                        <label>Fecha Final DOS:</label>
                                        <input type="text" class="form-control" id="txtFechaFinalDOS">
                                        <p class="help-block"></p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-12" style="text-align: right">
                                        <button type="button" class="btn btn-danger" data-dismiss="modal">Close</button>
                                        <button type="button" id="btnAceptarAprobar" onclick="GuardarAnioFiscal()" class="btn btn-primary">Guardar</button>
                                    </div>
                                </div>
                            </div>
                            <div class="tab-pane fade" id="RFLista-pills">
                                <div class="row">
                                    <div class="form-group col-lg-3">
                                        <label>Fecha desde:</label>
                                        <input type="text" class="form-control" id="txtFechaBusInicio">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Fecha Hasta:</label>
                                        <input type="text" class="form-control" id="txtFechaBusFinal">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Filtrar sin fecha</label>
                                        <input class="form-check-input" type="checkbox" value="" id="idFiltros2">
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Marca</label>
                                        <input type="text" class="form-control" id="cboMarca4" placeholder="Marca" oninput="BuscarMarca4()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboMarca4">
                                        </ul>
                                        <p class="help-block"></p>
                                    </div>
                                </div>
                                <div class="panel-footer">
                                    <div class="" style="text-align: center">
                                        <button id="btnConsulta" onclick="BtnConsultaAnio()" type="button" class="btn btn-primary">Consultar</button>
                                    </div>
                                </div>
                                <div class="col-lg-12" style="padding: 0px">
                                    <div class="panel panel-default">
                                        <div class="panel-heading">
                                            <div>
                                                <h4 class="" id="listPrincipalTitleLabel">Lista de Año Fiscal</h4>
                                            </div>
                                        </div>
                                        <!-- /.panel-heading -->
                                        <div class="panel-body" style="height: 150px; overflow-y: auto; overflow-x: auto;">
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
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" style="display: none;" id="txtCodigo">
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

