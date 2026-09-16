<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="RegistroIngresosEgresos.aspx.cs" Inherits="ReporteTareas.Formulario.RegistroIngresosEgresos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/RegistrIngDesc.js?v=5" type="text/javascript"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>
    <style>
        .fila-campos {
            display: flex;
            align-items: flex-end; /* alinea los 3 controles por la parte de abajo */
        }

            .fila-campos .form-group {
                width: 100%;
            }

        /* si usas select2 u otro plugin, asegúrate del ancho */
        .select2-container {
            width: 100% !important;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <div style="display: none">
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtPerfil" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                        </div>
                    </div>
                    <div class="panel-body">
                        <!-- Nav tabs -->
                        <ul class="nav nav-pills">
                            <li class="active" id="tab1"><a href="#home-pills" data-toggle="tab">Registro Ingreso o Descuentos</a>
                            </li>
                            <li id="tab2"><a href="#profile-pills" data-toggle="tab">Listas de Ingreso o Descuentos</a>
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
                                                    <label>Tipo Solicitud</label>
                                                </div>
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <select id="cboSolicitud" class="form-control" onchange="cargarCodigos()">
                                                    <option value="">-- Seleccione --</option>
                                                    <option value="1">(1) Ingresos</option>
                                                    <option value="2">(2) Descuentos</option>
                                                </select>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="panel panel-default">
                                                <div class="panel-heading">
                                                    <label>Registrar la solicitud</label>
                                                </div>
                                            </div>
                                            <!-- Combo códigos -->
                                            <div class="form-group col-lg-3">
                                                <select id="cboCodigos" class="form-control">
                                                    <option value="">-- Seleccione código --</option>
                                                </select>
                                                <p class="help-block"></p>
                                            </div>
                                        </div>
                                        <div class="row fila-campos">
                                            <div class="col-lg-6">
                                                <div class="form-group">
                                                    <label for="cmbUsuarios2">Usuarios</label>
                                                    <select id="cmbUsuarios2"
                                                        class="form-control js-example-basic-multiple7"
                                                        name="sucursal[]" multiple="multiple"
                                                        onchange="ObtenerUsuario()">
                                                    </select>
                                                </div>
                                            </div>

                                            <div class="col-lg-2">
                                                <div class="form-group">
                                                    <label for="txtValor">Valor</label>
                                                    <input type="number" id="txtValor" class="form-control" placeholder="Valor">
                                                </div>
                                            </div>

                                            <div class="col-lg-3">
                                                <div class="form-group">
                                                    <label for="txtFecha">Fecha:</label>
                                                    <input type="text" class="form-control" id="txtFecha">
                                                    <p class="help-block"></p>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row">
                                            <div class="form-group col-lg-12">
                                                <label>Observación</label>
                                                <textarea id="txtDetalle" class="form-control" placeholder="Detalle" rows="4" cols="50"></textarea>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="panel panel-default">
                                                <div class="panel-heading">
                                                    <label>Cargar Archivo .csv</label>
                                                </div>
                                            </div>
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
                                        <div class="" style="text-align: center">
                                            <button id="btnGuardar" type="button" class="btn btn-primary">Guardar</button>
                                            <button id="btnAgregar" type="button" class="btn btn-primary">Agregar</button>
                                            <%--<button id="btnCargar" type="button" class="btn btn-primary">Cargar Archivo csv</button>--%>
                                        </div>
                                        <div class="col-lg-12" style="padding: 0px">
                                            <div class="panel-body" style="height: 500px; overflow-y: auto; overflow-x: auto;">
                                                <div id="table-datosTablaPrincipal1" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                    <table id="table-users"
                                                        class="table table-striped table-bordered table-hover dataTable no-footer dtr-inline">
                                                        <thead>
                                                            <tr>
                                                                <th>Acciones</th>
                                                                <th>COD SAP</th>
                                                                <th>VALOR</th>
                                                                <th>OBSERVACION</th>
                                                                <th>FECHA</th>
                                                                <th>CC NOMINA</th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                        </tbody>
                                                    </table>

                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="tab-pane fade" id="profile-pills">
                                <div class="panel panel-default">
                                    <div class="panel-body">
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
