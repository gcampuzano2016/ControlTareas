<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionHorario.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionHorario" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/editorDiasHorario.js?v=1" type="text/javascript"></script>
    <script src="../js/parametrizacionHorario.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Catálogo de horarios laborales</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    Horarios disponibles para asignar
                    <div style="display: none">
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                    </div>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-4">
                            <label>Buscar (código o nombre):</label>
                            <input type="text" class="form-control" id="txtBuscar" placeholder="Escriba para filtrar..." onkeypress="if(event.keyCode==13){BuscarHorarios();return false;}">
                        </div>
                        <div class="form-group col-lg-3" style="padding-top: 25px">
                            <button id="btnBuscar" onclick="BuscarHorarios()" type="button" class="btn btn-primary">Buscar</button>
                            <button id="btnRefrescar" onclick="LimpiarBusqueda()" type="button" class="btn btn-default">Mostrar todos</button>
                        </div>
                        <div class="form-group col-lg-3" style="padding-top: 25px">
                            <label style="font-weight: normal">
                                <input type="checkbox" id="chkInactivos" onchange="BuscarHorarios()" />
                                Ver inactivos
                            </label>
                            <br />
                            <label style="font-weight: normal">
                                <input type="checkbox" id="chkPropios" onchange="BuscarHorarios()" />
                                Ver horarios propios
                            </label>
                        </div>
                        <div class="form-group col-lg-2" style="padding-top: 25px; text-align: right">
                            <button onclick="AbrirNuevoHorario()" type="button" class="btn btn-success">
                                <i class="fa fa-plus"></i>&nbsp;Nuevo horario
                            </button>
                        </div>
                    </div>

                    <div class="col-lg-12" style="padding: 0px">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 id="listTitleLabel">Horarios registrados</h4>
                            </div>
                            <div class="panel-body" style="height: 430px; overflow-y: auto; overflow-x: auto;">
                                <div id="datosTablaHorarios" style="padding: 0px">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Crear / Editar horario -->
        <div class="modal fade" id="modalHorario" tabindex="-1" role="dialog" aria-labelledby="modalHorarioLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalHorarioLabel">Nuevo horario laboral</h4>
                    </div>
                    <div class="modal-body">
                        <input type="hidden" id="txtIdHorario" value="0" />

                        <div class="alert alert-warning" id="avisoAsignados" style="display: none">
                            <i class="fa fa-exclamation-triangle"></i>&nbsp;<span id="textoAsignados"></span>
                        </div>

                        <div class="form-group col-lg-4">
                            <label>Código:</label>
                            <input type="text" class="form-control" id="txtCodigo" maxlength="30" placeholder="H0800_1700" />
                        </div>
                        <div class="form-group col-lg-8">
                            <label>Nombre:</label>
                            <input type="text" class="form-control" id="txtNombre" maxlength="120" placeholder="Jornada 08:00 a 17:00" />
                        </div>

                        <div class="form-group col-lg-12">
                            <label style="font-weight: normal">
                                <input type="checkbox" id="chkPredeterminado" />
                                Es el horario predeterminado (lo usa quien no tenga uno asignado)
                            </label>
                            &nbsp;&nbsp;&nbsp;
                            <label style="font-weight: normal">
                                <input type="checkbox" id="chkActivo" checked="checked" />
                                Activo
                            </label>
                        </div>

                        <div class="form-group col-lg-12">
                            <label>Días y horas:</label>
                            <button type="button" class="btn btn-xs btn-default pull-right" onclick="CopiarLunesAViernes()">
                                Copiar el lunes a toda la semana laboral
                            </button>
                            <table class="table table-bordered table-condensed" style="margin-top: 5px">
                                <thead>
                                    <tr>
                                        <th>Día</th>
                                        <th style="text-align: center; width: 90px">Laborable</th>
                                        <th style="width: 130px">Entrada</th>
                                        <th style="width: 130px">Salida</th>
                                    </tr>
                                </thead>
                                <tbody id="cuerpoDiasHorario">
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="GuardarHorario(false)" class="btn btn-primary" id="btnGuardarHorario">Guardar horario</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->

        <!-- Modal Confirmar -->
        <div class="modal fade" id="modalConfirmar" tabindex="-1" role="dialog" aria-labelledby="modalConfirmarLabel" aria-hidden="true">
            <div class="modal-dialog modal-sm">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalConfirmarLabel">Confirmar</h4>
                    </div>
                    <div class="modal-body" id="TextoConfirmar">
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="EjecutarConfirmacion()" class="btn btn-primary">Sí, continuar</button>
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
