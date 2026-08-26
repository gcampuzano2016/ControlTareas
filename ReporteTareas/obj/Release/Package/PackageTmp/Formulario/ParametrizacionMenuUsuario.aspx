<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionMenuUsuario.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionMenuUsuario" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionMenuUsuario.js?v=2" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Parametrización de módulos por usuario</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    Módulos adicionales a los que ya da el perfil del usuario. Lo heredado del perfil no se puede quitar desde aquí.
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
                        <div class="form-group col-lg-4" style="padding-top: 25px">
                            <button id="btnBuscar" onclick="BuscarUsuarios()" type="button" class="btn btn-primary">Buscar</button>
                        </div>
                    </div>

                    <div class="col-lg-12" style="padding: 0px">
                        <div class="panel panel-default">
                            <div class="panel-heading"><h4>Usuarios</h4></div>
                            <div class="panel-body" style="height: 260px; overflow-y: auto; overflow-x: auto;">
                                <div id="datosTablaUsuarios" style="padding: 0px"></div>
                            </div>
                        </div>
                    </div>

                    <div class="col-lg-12" style="padding: 0px; display: none" id="panelDetalle">
                        <div class="panel panel-default">
                            <div class="panel-heading"><h4>Módulos del usuario</h4></div>
                            <div class="panel-body">
                                <div class="row">
                                    <input type="hidden" id="txtCodUsuarioSel" />
                                    <div class="form-group col-lg-5">
                                        <label>Usuario:</label>
                                        <input type="text" class="form-control" id="txtNombreUsuarioSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Perfil:</label>
                                        <input type="text" class="form-control" id="txtPerfilSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-3" style="padding-top: 25px">
                                        <button id="btnGuardar" onclick="GuardarModulos()" type="button" class="btn btn-success">Guardar</button>
                                    </div>
                                </div>
                                <div style="margin-bottom: 8px">
                                    <span class="label label-default">Perfil</span> heredado, no se puede desmarcar &nbsp;
                                    <span class="label label-success">Extra</span> asignado a este usuario
                                    <span class="pull-right">
                                        <button id="btnExpandirTodo" onclick="ExpandirTodo()" type="button" class="btn btn-default btn-xs">
                                            <i class="fa fa-chevron-down"></i> Expandir todo
                                        </button>
                                        <button id="btnContraerTodo" onclick="ContraerTodo()" type="button" class="btn btn-default btn-xs">
                                            <i class="fa fa-chevron-right"></i> Contraer todo
                                        </button>
                                    </span>
                                </div>
                                <div style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                    <div id="datosArbolMenu" style="padding: 0px"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Informativo -->
        <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-labelledby="modalMensajeInformativoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeInformativoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="myModalLabel">Informativo</h4>
                    </div>
                    <div class="modal-body" id="MensajeInformativo"></div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->
    </div>
</asp:Content>
