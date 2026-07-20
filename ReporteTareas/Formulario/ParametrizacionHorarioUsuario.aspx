<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionHorarioUsuario.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionHorarioUsuario" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionHorarioUsuario.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
        <div class="row">
            <div class="col-lg-12">
                <div class="panel panel-default">
                    <div class="panel-heading" style="text-align: center">
                        <h3 style="margin: 6px 0">Parametrización de horario por usuario</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div class="panel panel-default">
                <div class="panel-heading">
                    Asignación de horario laboral
                    <div style="display: none">
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                    </div>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-5">
                            <label>Buscar usuario (nombre, código o cédula):</label>
                            <input type="text" class="form-control" id="txtBuscar" placeholder="Escriba para filtrar..." onkeypress="if(event.keyCode==13){BuscarUsuarios();return false;}">
                        </div>
                        <div class="form-group col-lg-3" style="padding-top: 25px">
                            <button id="btnBuscar" onclick="BuscarUsuarios()" type="button" class="btn btn-primary">Buscar</button>
                            <button id="btnRefrescar" onclick="LimpiarBusqueda()" type="button" class="btn btn-default">Mostrar todos</button>
                        </div>
                    </div>

                    <div class="col-lg-12">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 id="listTitleLabel">Usuarios y su horario vigente</h4>
                            </div>
                            <div class="panel-body" style="height: 430px; overflow-y: auto; overflow-x: auto;">
                                <div id="datosTablaUsuarios">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Asignar Horario -->
        <div class="modal fade" id="modalAsignar" tabindex="-1" role="dialog" aria-labelledby="modalAsignarLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalAsignarLabel">Asignar horario laboral</h4>
                    </div>
                    <div class="modal-body">
                        <input type="hidden" id="txtCodUsuarioSel" />
                        <div class="form-group col-lg-12">
                            <label>Usuario:</label>
                            <input type="text" class="form-control" id="txtNombreUsuarioSel" disabled />
                        </div>
                        <div class="form-group col-lg-12">
                            <label>Horario actual:</label>
                            <input type="text" class="form-control" id="txtHorarioActualSel" disabled />
                        </div>
                        <div class="form-group col-lg-7">
                            <label>Nuevo perfil de horario:</label>
                            <select id="cmbPerfilHorario" class="form-control">
                            </select>
                        </div>
                        <div class="form-group col-lg-5">
                            <label>Vigente desde:</label>
                            <input type="date" class="form-control" id="txtFechaDesde" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="GuardarAsignacion()" class="btn btn-primary" id="btnGuardarAsignacion">Guardar asignación</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->

        <!-- Modal Informativo -->
        <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-labelledby="modalMensajeInformativoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" id="modalMensajeInformativoTipo">
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
