<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="RegistrarContrato.aspx.cs" Inherits="ReporteTareas.Formulario.RegistrarContrato" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/RegistroContrato.js?v=15" type="text/javascript"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>
    <%--    <script src="../bower_components/Excel/xlsx.full.min.js"></script>
    <script src="../bower_components/Excel/excel.js"></script>--%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="panel-body">
                    <div class="row">
                        <h3>Registro Contrato</h3>
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
                            </div>
                    </div>
                    <div class="panel-body">
                        <!-- Nav tabs -->
                        <ul class="nav nav-pills">
                            <li class="active"><a href="#home-pills" data-toggle="tab">Registro de Contratos</a>
                            </li>
                            <li><a href="#profile-pills" data-toggle="tab">Listas de Contratos</a>
                            </li>
                            <li><a href="#Accion-pills" data-toggle="tab">Ver detalle de Ticket Aranda</a>
                            </li>
                            <li><a href="#Total-pills" data-toggle="tab">Total de Horas por Número de OS</a>
                            </li>
                            <li><a href="#Costo-pills" data-toggle="tab">Cargar Costos de Contratos</a>
                            </li>
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div class="tab-pane fade in active" id="home-pills">
                                <div id="RegistroContrato" style="display: block">
                                    <div class="row">
                                        <div class="form-group col-lg-2">
                                            <label>Fecha Registro</label>
                                            <input type="text" class="form-control" id="txtFechaDesde" placeholder="Fecha Registro">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-3">
                                            <label>Pedido</label>
                                            <input type="text" class="form-control" id="txtPedido" placeholder="Pedido">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-3">
                                            <label>Cliente</label>
                                            <!--<select id="cboCliente" class="form-control">
                                        </select>-->
                                            <input type="text" class="form-control" id="cboCliente" placeholder="Cliente" oninput="BuscarCliente()">
                                            <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes">
                                            </ul>
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-1">
                                            <label></label>
                                            <button class="btn btn-lg btn-success" onclick="BtnCrearCliente()" type="button">
                                                <i class="ace-icon glyphicon-plus  bigger-110 icon-only"></i>
                                            </button>
                                        </div>
                                        <div class="form-group col-lg-3">
                                            <label>Número del Contrato</label>
                                            <input type="text" class="form-control" id="txtReferencia" placeholder="Referencia de Cliente">
                                            <p class="help-block"></p>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="form-group col-lg-3">
                                            <label>Area</label>
                                            <select id="cboArea" class="form-control">
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
                                        <div class="form-group col-lg-2">
                                            <label>Gestor Responsable</label>
                                            <select id="cboResponsable" class="form-control">
                                            </select>
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-2">
                                            <label>Número de Orden</label>
                                            <input type="text" class="form-control" id="txtOrden" placeholder="Número de Orden">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-3">
                                            <label>Clasificación</label>
                                            <select id="cboClasificacion" class="form-control">
                                            </select>
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-1">
                                            <label>---</label>
                                            <button id="btnCargarProceso" onclick="CargarProceso()" type="button" class="btn btn-success">Carg. Requerimientos</button>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="form-group col-lg-12">
                                            <label>Descripción</label>
                                            <textarea class="form-control" id="txtDescripcion" name="txtDescripcion" rows="4" cols="50"></textarea>
                                            <p class="help-block"></p>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="form-group col-lg-2">
                                            <label>SLA</label>
                                            <select id="cboSla" class="form-control">
                                                <option value="SELECCIONAR AREA">SELECCIONAR SLA</option>
                                                <option value=""></option>
                                                <option value="7X24">7X24</option>
                                                <option value="8X5">8X5</option>
                                            </select>
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-2">
                                            <label>Tiempo de Respueta</label>
                                            <select id="cbotiempo" class="form-control">
                                                <option value="SELECCIONAR TIEMPO DE RESPUESTA">SELECCIONAR TIEMPO DE RESPUESTA</option>
                                                <option value="0">0</option>
                                                <option value="1">1</option>
                                                <option value="2">2</option>
                                                <option value="3">3</option>
                                                <option value="4">4</option>
                                            </select>
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-2">
                                            <label>Tiempo de Solucion</label>
                                            <input type="text" class="form-control" id="txtSolucion" placeholder="Tiempo de Solucion">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-2">
                                            <label>Tiempo de Diagnóstico</label>
                                            <input type="text" class="form-control" id="txtDiagnostico" placeholder="Tiempo de Diagnóstico">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-2">
                                            <label>Horas Contratadas</label>
                                            <input type="text" class="form-control" id="txtHContratadas" placeholder="Horas Contratadas">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-2">
                                            <label>Horas Entregadas</label>
                                            <input type="text" class="form-control" id="txtHEntregadas" disabled placeholder="Horas Entregadas">
                                            <p class="help-block"></p>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="form-group col-lg-3">
                                            <label>Horas Disponible</label>
                                            <input type="text" class="form-control" id="txtHDisponible" disabled placeholder="Horas Disponible">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-3">
                                            <label>Costo Plan</label>
                                            <input type="text" class="form-control" id="txtCostoPlan" placeholder="Costo Plan">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-3">
                                            <label>Costo Real</label>
                                            <input type="text" class="form-control" id="txtCostoReal" disabled placeholder="Costo Real">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-3">
                                            <label>Saldo Costo</label>
                                            <input type="text" class="form-control" id="txtSaldoCosto" disabled placeholder="Saldo Costo">
                                            <p class="help-block"></p>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="form-group col-lg-2">
                                            <label>Estado</label>
                                            <select id="cboEstado" class="form-control">
                                                <option value="SELECCIONAR ESTADO">SELECCIONAR ESTADO</option>
                                                <option value="EN EJECUCION">EN EJECUCION</option>
                                                <option value="CERRADA">CERRADA</option>
                                            </select>
                                        </div>
                                        <div class="form-group col-lg-3">
                                            <label>Fecha Estimada de Cierre</label>
                                            <input type="text" class="form-control" id="txtFechaECierre" placeholder="Fecha Estimada de Cierre">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-2">
                                            <label>Fecha Cierre</label>
                                            <input type="text" class="form-control" id="txtFechaCierre" placeholder="Fecha Cierre">
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-3">
                                            <label>Gerente Cuenta</label>
                                            <select id="cboGerente" class="form-control">
                                            </select>
                                            <p class="help-block"></p>
                                        </div>
                                        <div class="form-group col-lg-2">
                                            <label>Sucursal</label>
                                            <select id="cboSucursal" class="form-control">
                                                <option value="SELECCIONAR SUCURSAL">SELECCIONAR SUCURSAL</option>
                                                <option value="AMBATO">AMBATO</option>
                                                <option value="CUENCA">CUENCA</option>
                                                <option value="QUITO">QUITO</option>
                                                <option value="GUAYAQUIL">GUAYAQUIL</option>
                                            </select>
                                            <p class="help-block"></p>
                                        </div>
                                        <!--<div class="form-group col-lg-2">
										<label>Mantenimiento</label>										
                                        <input type="text" class="form-control" id="txtMantenimiento" placeholder="Mantenimiento">
                                        <p class="help-block"></p>
                                    </div>
                                    <div class="form-group col-lg-2">
										<label>Mant. Entregado</label>										
                                        <input type="text" class="form-control" id="txtMantEntregado" placeholder="Mant. Entregado">
                                        <p class="help-block"></p>
                                    </div>-->
                                    </div>
                                    <div class="row">
                                        <div class="form-group col-lg-12">
                                            <label>Observación</label>
                                            <textarea class="form-control" id="txtObservacion" name="txtObservacion" rows="4" cols="50"></textarea>
                                            <p class="help-block"></p>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="form-group col-lg-12">
                                            <label>Contacto Cliente</label>
                                            <textarea class="form-control" id="txtContactoCliente" name="txtContactoCliente" rows="2" cols="50"></textarea>
                                            <p class="help-block"></p>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="form-group col-lg-6">
                                            <label>E-Mail</label>
                                            <input type="text" class="form-control" id="txtMail" placeholder="E-Mail">
                                            <p class="help-block"></p>
                                        </div>
                                    </div>
                                    <div class="row">
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
                                            <button id="btnGuardar" onclick="GuardarNuevoContrato()" type="button" class="btn btn-primary">Guardar</button>
                                            <button id="btnActualizar" onclick="ActualizarContrato()" type="button" class="btn btn-primary">Actualizar</button>
                                            <button id="btnEliminar" onclick="EliminarContrato()" type="button" class="btn btn-primary">Eliminar</button>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                                    </div>
                                </div>
                                <div id="RegistrarMatenimiento" style="display: none">
                                    <asp:Panel ID="Panel31" runat="server" Visible="true" Enabled="true">
                                        <div id="formActualizaTarea2" class="panel panel-default col-lg-12">
                                            <!-- Modal -->
                                            <div class="" id="formModalActualizacion2" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
                                                <div class="modal-dialog modal-lg">
                                                    <div class="modal-content">
                                                        <div class="modal-header">
                                                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios2()">&times;</button>
                                                            <h4 class="modal-title" id="fomTitleLabel2">Registrar Mantenimiento</h4>
                                                        </div>
                                                        <div class="modal-body" id="divMensajeAprobar">
                                                            <div class="row">
                                                                <div class="form-group col-lg-3">
                                                                    <label>orden:</label>
                                                                    <input type="text" class="form-control" disabled id="txtOrdenReq">
                                                                    <p class="help-block"></p>
                                                                </div>
                                                                <div class="form-group col-lg-5">
                                                                    <label id="lblFechaProceso">Fecha Planificada:</label>
                                                                    <input type="text" class="form-control" id="txtfechaReq1">
                                                                    <p class="help-block"></p>
                                                                </div>
                                                                <div class="form-group col-lg-3">
                                                                    <label>Estado:</label>
                                                                    <select id="cboClasificacion4" class="form-control" onclick="CambiarEstado()">
                                                                        <option value="0">-- SELECCIONE --</option>
                                                                        <option value="REALIZADO">REALIZADO</option>
                                                                        <option value="COORDINADO">COORDINADO</option>
                                                                        <option value="EN EJECUCIÓN">EN EJECUCIÓN</option>
                                                                        <option value="POR PLANIFICAR">POR PLANIFICAR</option>
                                                                        <option value="SUSPENDIDO">SUSPENDIDO</option>
                                                                        <option value="EN COORDINACION">EN COORDINACION</option>
                                                                        <option value="POR CONFIRMAR CLIENTE">POR CONFIRMAR CLIENTE</option>
                                                                        <option value="NO SE REALIZO">NO SE REALIZO</option>
                                                                    </select>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <div class="form-group col-lg-3">
                                                                    <label>Registrar pago</label>
                                                                    <input class="form-check-input" type="checkbox" value="" id="IdRegistroPago" onchange="DesactivarCheck1()">
                                                                </div>
                                                                <div class="form-group col-lg-3" style="display: none" id="ValorMant">
                                                                    <label>Valor:</label>
                                                                    <input type="text" class="form-control" id="txtValor" value="0.00">
                                                                    <p class="help-block"></p>
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <div class="form-group col-lg-12">
                                                                    <label>Descripción:</label>
                                                                    <textarea class="form-control" id="txtObservacionReq" name="txtObservacion" rows="2" cols="50"></textarea>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <div class="form-group col-lg-12">
                                                                    <label>Observacion:</label>
                                                                    <textarea class="form-control" id="txtDescripcionReq" name="txtDescripcionReq" rows="2" cols="50"></textarea>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                            </div>
                                                            <div id="Proceso1" style="display: block">
                                                                <div class="col-lg-12" style="padding: 0px">
                                                                    <div class="panel panel-default">
                                                                        <!-- /.panel-heading -->
                                                                        <div class="panel-body" style="height: 200px; overflow-y: auto; overflow-x: auto;">
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
                                                            </div>
                                                            <div id="Proceso2" style="display: none">
                                                                <div class="modal-header">
                                                                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios3()">&times;</button>
                                                                    <h4 class="modal-title" id="fomTitleLabel">Cargar Archivo</h4>
                                                                </div>
                                                                <div class="modal-body">
                                                                    <div class="panel panel-default">
                                                                        <div class="panel-body">
                                                                            <div class="col-lg-12">
                                                                                <form role="form">
                                                                                    <div class="col-lg-8">
                                                                                        <label>Adjuntar Archivos:</label>
                                                                                        <input type="file" id="archivosAdjuntos3" multiple />
                                                                                        <p class="help-block"></p>
                                                                                        <progress id="fileProgress" style="display: none"></progress>
                                                                                        <hr />
                                                                                        <span id="lblMessage" style="color: Green"></span>
                                                                                    </div>
                                                                                    <div class="col-lg-4">
                                                                                        <br>
                                                                                        <input type="button" id="btnCargarArchivosAdjuntos3" value="Cargar Archivos" onclick="CargarArchivosAdjuntos2()" class="btn btn-info" />
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
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div class="modal-footer">
                                                                <input type="text" class="form-control" style="display: none;" id="txtCodigo2">
                                                                <button type="button" class="btn btn-danger" onclick="CancelarCambios2()">Close</button>
                                                                <button type="button" id="btnAceptarAprobar" onclick="GuardarProceso()" class="btn btn-primary">Guardar</button>
                                                                <button type="button" id="btnAceptarActualizar" onclick="ActualizarProceso()" class="btn btn-primary">Actualizar</button>
                                                                <button type="button" id="btnAceptarEliminar" onclick="EliminarProceso()" class="btn btn-primary">Eliminar</button>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </asp:Panel>
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
                                                    <div class="form-group col-lg-2">
                                                        <label>Gerente Cuenta</label>
                                                        <select id="cboGerente2" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Clasificación</label>
                                                        <select id="cboClasificacion5" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>
                                                <div class="row">
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
                                            <div class="panel-body" style="height: 800px; overflow-y: auto; overflow-x: auto;">
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
                                <div class="row" id="DetalleTicket" style="display: block">
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
                                                            <label>Número de OS</label>
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
                                                <div class="panel-body" style="height: 800px; overflow-y: auto; overflow-x: auto;">
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
                                <div class="row" id="DetalleCargaArchivo" style="display: none">

                                    <asp:Panel ID="Panel3" runat="server" Visible="true" Enabled="true">
                                        <div id="formActualizaTarea" class="panel panel-default col-lg-12">
                                            <!-- Modal -->
                                            <div class="" id="formModalActualizacion" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
                                                <div class="modal-dialog">
                                                    <div class="modal-content">
                                                        <div class="modal-header">
                                                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios()">&times;</button>
                                                            <h4 class="modal-title" id="fomTitleLabel">Cargar Archivo</h4>
                                                        </div>
                                                        <div class="modal-body">
                                                            <!-- Form -->
                                                            <div class="panel panel-default">
                                                                <div class="panel-body">
                                                                    <div class="col-lg-12">
                                                                        <form role="form">
                                                                            <div class="col-lg-6" id="divFrmTxtOrdenServicio">
                                                                                <label>No. Orden de Servicio:</label>
                                                                                <input type="text" class="form-control" id="frmTxtOrdenServicio" disabled>
                                                                                <p class="help-block"></p>
                                                                            </div>
                                                                            <div class="col-lg-6" id="divFrmTxtCodigoAranda">
                                                                                <label>Código Aranda:</label>
                                                                                <input type="text" class="form-control" id="frmTxtCodigoAranda" disabled>
                                                                                <p class="help-block"></p>
                                                                            </div>
                                                                            <div class="col-lg-6" id="divFrmTxtCodigoTarea">
                                                                                <label>Código Tarea:</label>
                                                                                <input type="text" class="form-control" id="frmTxtCodigoTarea" disabled>
                                                                                <p class="help-block"></p>
                                                                            </div>
                                                                            <div class="col-lg-8">
                                                                                <label>Adjuntar Archivos:</label>
                                                                                <input type="file" id="archivosAdjuntos2" multiple />
                                                                                <p class="help-block"></p>
                                                                                <progress id="fileProgress" style="display: none"></progress>
                                                                                <hr />
                                                                                <span id="lblMessage" style="color: Green"></span>
                                                                            </div>
                                                                            <div class="col-lg-4">
                                                                                <br>
                                                                                <input type="button" id="btnCargarArchivosAdjuntos2" value="Cargar Archivos" onclick="CargarArchivosAdjuntos()" class="btn btn-info" />
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
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </asp:Panel>
                                </div>
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
                                                        <label>Cod Aranda/Nombre Tecnico</label>
                                                        <input type="text" class="form-control" id="txtNumeroOrdenOS">
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
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
                                            <div class="panel-body" style="height: 800px; overflow-y: auto; overflow-x: auto;">
                                                <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                    <div class="row">

                                                        <div class="col-sm-12" id='datosTablaPrincipal3' style="padding: 0px">
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
                            <div class="tab-pane fade" id="Costo-pills">
                                <div class="row">
                                    <div class="col-lg-12" style="padding: 20px">
                                        <div class="panel-body">
                                            <div class="row">
                                                <h3>Cargar Documento</h3>
                                            </div>
                                            <asp:Panel ID="Panel9" runat="server" Visible="true" Enabled="true">
                                                <div class="col-lg-12" style="padding: 0px">
                                                    <div class="panel panel-default">
                                                        <div class="conatiner mt-5">
                                                            <div class="row">
                                                                <div class="col-md-3"></div>
                                                                <div class="col-md-3">
                                                                    <input class="form-control" type="file" id="input" accept=".xls,.xlsx">
                                                                </div>
                                                                <div class="col-md-2">
                                                                    <button class="btn btn-primary" id="button">Cargar</button>
                                                                </div>
                                                                <div class="col-md-12">
                                                                    <pre id="jsondata"></pre>
                                                                </div>
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
                        <h4 class="modal-title" id="modalMensajeAprobarLabelTitulo">Registrar Requerimientos</h4>
                    </div>
                    <div class="modal-body" id="divMensajeAprobar">
                        <div class="row">
                            <div class="form-group col-lg-3">
                                <label>orden:</label>
                                <input type="text" class="form-control" disabled id="txtOrdenReq">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-5">
                                <label>Fecha Planificada:</label>
                                <input type="text" class="form-control" id="txtfechaReq1">
                                <p class="help-block"></p>
                            </div>
                            <!--<div class="form-group col-lg-3" disabled>
                                <label>F. Final:</label>
                                <input type="text" class="form-control" id="txtfechaReq2">
                                <p class="help-block"></p>
                            </div>-->
                            <div class="form-group col-lg-3">
                                <label>Estado:</label>
                                <select id="cboClasificacion42" class="form-control">
                                    <option value="0">-- SELECCIONE --</option>
                                    <option value="REALIZADO">REALIZADO</option>
                                    <option value="COORDINADO">COORDINADO</option>
                                    <option value="EN EJECUCIÓN">EN EJECUCIÓN</option>
                                    <option value="POR PLANIFICAR">POR PLANIFICAR</option>
                                    <option value="SUSPENDIDO">SUSPENDIDO</option>
                                    <option value="EN COORDINACION">EN COORDINACION</option>
                                    <option value="POR CONFIRMAR CLIENTE">POR CONFIRMAR CLIENTE</option>
                                </select>
                                <p class="help-block"></p>
                            </div>
                            <!--<div class="form-group col-lg-2">
                                <label>Valor :</label>
                                <input type="text" class="form-control" id="txtValor">
                                <p class="help-block"></p>
                            </div>-->
                        </div>
                        <div class="row">
                            <div class="form-group col-lg-12">
                                <label>Descripción:</label>
                                <textarea class="form-control" id="txtObservacionReq2" name="txtObservacion" rows="2" cols="50"></textarea>
                                <p class="help-block"></p>
                            </div>
                        </div>
                        <div class="row">
                            <div class="form-group col-lg-12">
                                <label>Observacion:</label>
                                <textarea class="form-control" id="txtDescripcionReq2" name="txtDescripcionReq" rows="2" cols="50"></textarea>
                                <p class="help-block"></p>
                            </div>
                        </div>
                        <div class="col-lg-12" style="padding: 0px">
                            <div class="panel panel-default">
                                <!--<div class="panel-heading">
                                    <div>
                                        <h4 class="" id="listPrincipalTitleLabel">Listado de Requerimientos</h4>
                                    </div>
                                </div>-->
                                <!-- /.panel-heading -->
                                <div class="panel-body" style="height: 200px; overflow-y: auto; overflow-x: auto;">
                                    <div id="table-datosTablaPrincipal2" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                        <div class="row">
                                            <div class="col-sm-12" id='datosTablaPrincipal42' style="padding: 0px">
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
                    <div class="modal-footer">
                        <input type="text" class="form-control" style="display: none;" id="txtCodigo">
                        <button type="button" class="btn btn-danger" data-dismiss="modal">Close</button>
                        <button type="button" id="btnAceptarAprobar2" onclick="GuardarProceso()" class="btn btn-primary">Guardar</button>
                        <button type="button" id="btnAceptarActualizar2" onclick="ActualizarProceso()" class="btn btn-primary">Actualizar</button>
                        <button type="button" id="btnAceptarEliminar2" onclick="EliminarProceso()" class="btn btn-primary">Eliminar</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <!-- Modal -->
        <div class="modal fade" id="modalMensajeConfirmacion" tabindex="-1" role="dialog" aria-labelledby="modalMensajeConfirmacionLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeConfirmacionTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="myModalConfirmacionLabel">Confirmación</h4>
                    </div>
                    <div class="modal-body" id="modalMensajeConfirmacioMensaje">
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" id="txtCodigoItem" disabled style="visibility: hidden">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="EliminarArchivo()" class="btn btn-primary" id="btnBorrarArchivo" hidden>Continuar Borrado</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <!-- Modal -->
        <div class="modal fade" id="modalMensajeAprobar2" tabindex="-1" role="dialog" aria-labelledby="modalMensajeAprobarLabel" aria-hidden="true">
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
                                <textarea class="form-control" id="txtObservacionReq3" name="txtObservacionReq3" rows="4" cols="50"></textarea>
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

    </div>
</asp:Content>



<%--<asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True"></asp:DropDownList>--%>

