<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionUsuarios.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionUsuarios" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionUsuarios.js?v=4" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Administración de usuarios</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    Datos de los usuarios registrados. El código, el login, el perfil y el estado no se editan desde aquí.
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
                            <div class="panel-heading"><h4>Datos del usuario</h4></div>
                            <div class="panel-body">
                                <input type="hidden" id="txtIdUsuarioSel" />
                                <div class="row">
                                    <div class="form-group col-lg-3">
                                        <label>Código:</label>
                                        <input type="text" class="form-control" id="txtCodUsuarioSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Login:</label>
                                        <input type="text" class="form-control" id="txtLoginSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-2">
                                        <label>Perfil:</label>
                                        <input type="text" class="form-control" id="txtPerfilSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-2">
                                        <label>Estado:</label>
                                        <input type="text" class="form-control" id="txtEstadoSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-2">
                                        <label title="Si aparece en selectores de jefe, autocompletado y solicitudes">Selectores:</label>
                                        <input type="text" class="form-control" id="txtSelectoresSel" disabled />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-6">
                                        <label>Nombre: <span style="color:#a94442">*</span></label>
                                        <input type="text" class="form-control" id="txtNombre" maxlength="100" />
                                    </div>
                                    <div class="form-group col-lg-6">
                                        <label>Correo:</label>
                                        <input type="text" class="form-control" id="txtCorreo" maxlength="100" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-4">
                                        <label>Cédula:</label>
                                        <input type="text" class="form-control" id="txtCedula" maxlength="32" />
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Departamento:</label>
                                        <select class="form-control" id="cboDepartamento"></select>
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Empresa:</label>
                                        <select class="form-control" id="cboEmpresa">
                                            <option value=""></option>
                                            <option value="DOS">DOS</option>
                                            <option value="AGILITY">AGILITY</option>
                                        </select>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-4">
                                        <label>Código SAP:</label>
                                        <input type="text" class="form-control" id="txtCodSap" maxlength="50" />
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Jefe inmediato:</label>
                                        <input type="text" class="form-control" id="txtJefe" maxlength="100" />
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Correo del jefe:</label>
                                        <input type="text" class="form-control" id="txtCorreoJefe" maxlength="100" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-4">
                                        <label>Teléfonos de emergencia:</label>
                                        <input type="text" class="form-control" id="txtTelefonosEmergencia" maxlength="100" placeholder="Uno o varios, separados por / o ," />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <button id="btnGuardar" onclick="GuardarUsuario()" type="button" class="btn btn-success">Guardar</button>
                                        <button id="btnCambiarEstado" onclick="CambiarEstado()" type="button" class="btn btn-warning">Inactivar usuario</button>
                                        <button id="btnHistorial" onclick="VerHistorial()" type="button" class="btn btn-default">Ver historial</button>
                                        <p class="text-muted" style="margin-top: 8px">
                                            <strong>Estado</strong> filtra los listados de menús y horarios.
                                            <strong>Selectores</strong> controla si el usuario aparece en los selectores de jefe,
                                            el autocompletado y las listas de solicitudes; es el que cambia este botón.
                                            Ninguno de los dos impide iniciar sesión: eso lo controla el dominio.
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal de confirmación de estado -->
        <div class="modal fade" id="modalConfirmarEstado" tabindex="-1" role="dialog" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title">Confirmar cambio de estado</h4>
                    </div>
                    <div class="modal-body" id="MensajeConfirmarEstado"></div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button id="btnConfirmarEstado" onclick="ConfirmarCambioEstado()" type="button" class="btn btn-warning">Inactivar</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal de historial -->
        <div class="modal fade" id="modalHistorial" tabindex="-1" role="dialog" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title">Historial de cambios</h4>
                    </div>
                    <div class="modal-body" style="max-height: 400px; overflow-y: auto">
                        <div id="datosHistorial"></div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Informativo -->
        <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeInformativoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title">Informativo</h4>
                    </div>
                    <div class="modal-body" id="MensajeInformativo"></div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
