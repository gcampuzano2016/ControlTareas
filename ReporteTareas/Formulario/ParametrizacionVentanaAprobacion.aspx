<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionVentanaAprobacion.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionVentanaAprobacion" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionVentanaAprobacion.js?v=2" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Parametrización de ventana de aprobación por jefe</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    Ventana en la que cada jefe inmediato puede aprobar las actividades de sus colaboradores
                    <div style="display: none">
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                    </div>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-5">
                            <label>Buscar jefe (nombre o correo):</label>
                            <input type="text" class="form-control" id="txtBuscar" placeholder="Escriba para filtrar..." onkeypress="if(event.keyCode==13){BuscarJefes();return false;}">
                        </div>
                        <div class="form-group col-lg-3" style="padding-top: 25px">
                            <button id="btnBuscar" onclick="BuscarJefes()" type="button" class="btn btn-primary">Buscar</button>
                            <button id="btnRefrescar" onclick="LimpiarBusqueda()" type="button" class="btn btn-default">Mostrar todos</button>
                        </div>
                    </div>

                    <div class="col-lg-12" style="padding: 0px">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 id="listTitleLabel">Jefes inmediatos y su ventana de aprobación</h4>
                            </div>
                            <div class="panel-body" style="height: 430px; overflow-y: auto; overflow-x: auto;">
                                <div id="datosTablaJefes" style="padding: 0px">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Asignar Ventana -->
        <div class="modal fade" id="modalAsignar" tabindex="-1" role="dialog" aria-labelledby="modalAsignarLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalAsignarLabel">Asignar ventana de aprobación</h4>
                    </div>
                    <div class="modal-body">
                        <input type="hidden" id="txtMailJefeSel" />
                        <div class="form-group col-lg-12">
                            <label>Jefe:</label>
                            <input type="text" class="form-control" id="txtNombreJefeSel" disabled />
                        </div>
                        <div class="form-group col-lg-12">
                            <label>Ventana actual:</label>
                            <input type="text" class="form-control" id="txtVentanaActualSel" disabled />
                        </div>
                        <div class="form-group col-lg-6">
                            <label>Fecha desde:</label>
                            <input type="date" class="form-control" id="txtFechaDesde" />
                        </div>
                        <div class="form-group col-lg-6">
                            <label>Fecha hasta:</label>
                            <input type="date" class="form-control" id="txtFechaHasta" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="GuardarVentana()" class="btn btn-primary" id="btnGuardarVentana">Guardar ventana</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->

        <!-- Modal Informativo -->
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
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->
    </div>
</asp:Content>
