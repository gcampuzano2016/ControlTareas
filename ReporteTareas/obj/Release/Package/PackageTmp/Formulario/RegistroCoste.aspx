<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="RegistroCoste.aspx.cs" Inherits="ReporteTareas.Formulario.RegistroCoste" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>
    <script src="../js/costeo.js?v=26"></script>
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
                        <h3>Registro Costeo</h3>
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
                        </div>
                    </div>
                    <div class="panel-body">
                        <!-- Nav tabs -->
                        <ul class="nav nav-pills">
                            <li class="active" id="tab1"><a href="#home-pills" data-toggle="tab">Ingreso de Costeo</a>
                            </li>
                            <li id="tab2"><a href="#profile-pills" data-toggle="tab">Listas de Costeo</a>
                            </li>
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div class="tab-pane fade in active" id="home-pills">
                                <div class="panel panel-default">
                                    <div class="panel-body">
                                        <div class="row">
                                            <div class="form-group col-lg-8">
                                            </div>
                                            <div class="form-group col-lg-1">
                                                <button id="btnNuevo" onclick="NuevoCosteo()" type="button" class="btn btn-primary">Nuevo</button>
                                            </div>
                                            <div class="form-group col-lg-1">
                                                <button id="btnGuardar" onclick="GuardarNuevoCosteo()" type="button" class="btn btn-primary">Guardar</button>
                                            </div>
                                            <div class="form-group col-lg-1">
                                                <button id="btnActualizar" onclick="ActualizarCosteo()" type="button" class="btn btn-warning">Actualizar</button>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="form-group col-lg-2">
                                                <label for="exampleInputTicket">Ticket</label>
                                                <input type="text" class="form-control" id="txtTicket" placeholder="Ticket">
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>Fecha Solicitud</label>
                                                <input type="text" class="form-control" id="txtFechaSolicitud" placeholder="Fecha Solicitud">
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>Fecha Actual</label>
                                                <input type="text" class="form-control" id="txtFechaActual" placeholder="Fecha Actual">
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Sucursal</label>
                                                <select id="cboSucursal" class="form-control" onchange="BuscarSucursal2()">
                                                </select>
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Gerente Cuenta</label>
                                                <select id="cboVendedor" class="form-control">
                                                </select>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="form-group col-lg-3">
                                                <label>Cliente</label>
                                                <input type="text" class="form-control" id="cboCliente2" placeholder="Cliente" oninput="BuscarCliente2()">
                                                <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes2">
                                                </ul>
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label for="exampleInputTicket">Sector</label>
                                                <%--<input type="text" class="form-control" id="txtSector" placeholder="Sector">--%>
                                                <select id="txtSector" class="form-control">
                                                    <option value="SELECCIONAR">SELECCIONAR</option>
                                                    <option value="PUBLICO">PUBLICO</option>
                                                    <option value="PRIVADO">PRIVADO</option>
                                                </select>
                                            </div>
                                            <div class="form-group col-lg-7">
                                                <label for="exampleInputTicket">Concepto</label>
                                                <input type="text" class="form-control" id="txtConcepto" placeholder="Concepto">
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="form-group col-lg-4">
                                                <label>Unidad Negocio</label>
                                                <%--<input type="text" class="form-control" id="txtUnidadNegocio" placeholder="Unidad Negocio">--%>
                                                <select id="txtUnidadNegocio" class="form-control">
                                                    <option value="SELECCIONAR">SELECCIONAR</option>
                                                    <option value="SERVICIOS ADMINISTRADOS">SERVICIOS ADMINISTRADOS</option>
                                                    <option value="NETWORKING">NETWORKING</option>
                                                    <option value="SOFTWARE">SOFTWARE</option>
                                                    <option value="DATACENTER">DATACENTER</option>
                                                    <option value="SEGURIDAD Y APLICACIONES">SEGURIDAD Y APLICACIONES</option>
                                                </select>
                                            </div>
                                            <div class="form-group col-lg-5">
                                                <label>Responsable Dimensionamiento</label>
                                                <input type="text" class="form-control" id="txtecnico" placeholder="Responsable Dimensionamiento" oninput="BuscarEspecialista()">
                                                <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboEspecialista">
                                                </ul>
                                            </div>
                                            <div class="form-group col-lg-3" style="display: none">
                                                <label>Tipo Servicio</label>
                                                <input type="text" class="form-control" id="txtTipoServicio" placeholder="Tipo Servicio">
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Estado Servicio</label>
                                                <%--<input type="text" class="form-control" id="txtEstadoServicio" placeholder="Estado Servicio">--%>
                                                <select id="txtEstadoServicio" class="form-control">
                                                    <option value="SELECCIONAR">SELECCIONAR</option>
                                                    <option value="ENTREGADO">ENTREGADO</option>
                                                    <option value="CANCELADO">CANCELADO</option>
                                                    <option value="PENDIENTE">PENDIENTE</option>
                                                </select>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="form-group col-lg-3">
                                                <label>Fecha Est. Entg. Especialista</label>
                                                <input type="text" class="form-control" id="txtPlazoEntrega" placeholder="Fecha Est. Entg. Especialista">
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Fecha Entrega Especialista</label>
                                                <input type="text" class="form-control" id="txtFechaEntregaEsp" placeholder="Fecha Entrega Especialista">
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>Fecha Entrega Alcance</label>
                                                <input type="text" class="form-control" id="txtFechaEntregaAlc" placeholder="Fecha Entrega Alcance">
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>Rev. Luis Cadena</label>
                                                <%--<input type="text" class="form-control" id="txtRevision" placeholder="Revisión">--%>
                                                <input class="form-check-input" type="checkbox" value="" id="txtRevision">
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>Costo</label>
                                                <input type="number" class="form-control" id="txtCosto" placeholder="Costo">
                                            </div>
                                            <div class="form-group col-lg-12">
                                                <label>Observacion</label>
                                                <input type="text" class="form-control" id="txtObservacion" placeholder="Observacion">
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
                                    </div>
                                </div>
                            </div>
                            <div class="tab-pane fade" id="profile-pills">
                                <div class="panel panel-default">
                                    <div class="panel-heading">
                                        Filtros
                                    </div>
                                    <div class="panel-body">
                                        <div class="row">
                                            <div class="form-group col-lg-2">
                                                <label>Filtrar sin fecha</label>
                                                <input class="form-check-input" type="checkbox" value="" id="idFiltros" onchange="LimpiarBusqueda()">
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Tipo Fecha</label>
                                                <select id="cboTipoFecha" class="form-control">
                                                    <option value="0">Seleccionar</option>
                                                    <option value="1">Fecha Solicitud</option>
                                                    <option value="2">Fecha Actual</option>
                                                    <option value="3">Fecha Entrega Especialista</option>
                                                    <option value="4">Fecha Entrega Alcance</option>
                                                </select>
                                                <p class="help-block"></p>
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
                                        <div class="row">
                                            <div class="form-group col-lg-3">
                                                <label>Sucursal</label>
                                                <select id="cboSucursal2" class="form-control" onchange="BuscarSucursal3()">
                                                </select>
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Gerente Cuenta</label>
                                                <select id="cboVendedor2" class="form-control">
                                                </select>
                                                <p class="help-block"></p>
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Responsable Dimensionamiento</label>
                                                <input type="text" class="form-control" id="txtecnico2" placeholder="Responsable Dimensionamiento" oninput="BuscarEspecialista2()">
                                                <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboEspecialista2">
                                                </ul>
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Cliente</label>
                                                <input type="text" class="form-control" id="cboCliente3" placeholder="Cliente" oninput="BuscarCliente3()">
                                                <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes3">
                                                </ul>
                                                <p class="help-block"></p>
                                            </div>
                                        </div>
                                        <div class="panel-footer">
                                            <div class="" style="text-align: center">
                                                <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-primary">Consultar</button>
                                                <button id="btnDescarga" onclick="BtnDescarga()" type="button" class="btn btn-primary">Descargar XLS</button>
                                            </div>
                                        </div>
                                    </div>
                                    <br />
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                <div>
                                                    <h4 class="" id="listPrincipalTitleLabel">Lista de Costeo</h4>
                                                </div>
                                            </div>
                                            <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                                <div id="table-datosTablaPrincipal1" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                    <table id="table-users" class="table table-striped table-bordered table-hover dataTable no-footer dtr-inline">
                                                        <thead>
                                                            <th>Acciones</th>
                                                            <th>Ticket</th>
                                                            <th>Fecha Solicitud</th>
                                                            <th>Fecha Actual</th>
                                                            <th>Nombre del Vendedor o solicitante</th>
                                                            <th>Sucursal</th>
                                                            <th>Sector</th>
                                                            <th>Nombre Cliente</th>
                                                            <th>Concepto del Servicio</th>
                                                            <th>Unidad de Negocio</th>
                                                            <th>Responsable Dimensionamiento</th>
                                                            <th>Tipo de Servicio</th>
                                                            <th>Plazo Entrega Propuesta</th>
                                                            <th>Status del Servicio</th>
                                                            <th>Fecha de Entrega-Especialt</th>
                                                            <th>Fecha de Entrega de Alcance</th>
                                                            <th>Revisión</th>
                                                            <th>Costo</th>
                                                        </thead>
                                                    </table>
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
