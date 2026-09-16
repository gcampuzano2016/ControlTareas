<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="AprobarHorasExtraPendientes.aspx.cs" Inherits="ReporteTareas.Formulario.AprobarHorasExtraPendientes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/AprobrarHoraExtra.js?v=1" type="text/javascript"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="panel-body">
                    <div class="row">
                        <h3>Aprobar Horas Extras de Tareas</h3>
                    </div>
                </div>
            </div>
        </div>
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Filtros
                            <div style="display: none">
                                <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            </div>
                    </div>
                    <div class="panel-body">
                        <div class="row">
                            <div class="form-group col-lg-4">
                                <label>Fecha desde:</label>
                                <input type="text" class="form-control" id="txtFechaDesde">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-4">
                                <label>Fecha Hasta:</label>
                                <input type="text" class="form-control" id="txtFechaHasta">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-4">
                                <label>Usuarios</label>
                                <select id="cmbUsuarios" class="form-control">
                                </select>
                            </div>
                        </div>
                        <div class="row">
                            <div class="form-group col-lg-4">
                                <label>Estado Aprobación</label>
                                <select id="cmbEstados" class="form-control">
                                    <option value="1">PENDIENTE DE APROBACION</option>
                                    <option value="2">APROBADA HORAS EXTRAS</option>
                                    <option value="3">RECHAZADA HORAS EXTRAS</option>
                                </select>
                            </div>
                        </div>
                        <div class="row">
                            <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                        </div>
                    </div>
                    <div class="panel-footer">
                        <div class="" style="text-align: center">
                            <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-default">Consultar</button>
                            <button id="btnDescarga" onclick="BtnDescarga()" type="button" class="btn btn-default">Descargar XLS</button>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <div>
                            <h4 class="" id="listPrincipalTitleLabel">Listado de Tareas</h4>
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

        <!-- Modal -->
        <div class="modal fade" id="modalMensajeAprobar" tabindex="-1" role="dialog" aria-labelledby="modalMensajeAprobarLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #d9edf7" id="modalMensajeAprobarTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalMensajeAprobarLabelTitulo">Confirmar</h4>
                    </div>
                    <div class="modal-body" id="divMensajeAprobar">
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" style="display: none;" id="txtCodigo">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                        <button type="button" id="btnAceptarAprobar" onclick="" class="btn btn-default">Aceptar</button>
                        <button type="button" id="btnAceptarRechazar" onclick="" class="btn btn-default">Aceptar</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

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

