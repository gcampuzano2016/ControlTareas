﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ReporteHorarioLaboral.aspx.cs" Inherits="ReporteTareas.Formulario.ReporteHorarioLaboral" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/ProcesoConvenio.js?v=16" type="text/javascript"></script>

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
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Solicitud de Vacaciones y Permisos</h3>
                    </div>
                </div>
            </div>
        </div>
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Lista de solicitud
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
                            <li class="active" id="tab1"><a href="#home-pills" data-toggle="tab">Lista de Solicitud</a>
                            </li>
                            <li id="tab2"><a href="#profile-pills" data-toggle="tab">Detalle de Vacaciones</a>
                            </li>
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div class="tab-pane fade in active" id="home-pills">
                                <div class="panel panel-default">
                                    <div class="panel-body">
                                        <div class="row">
                                            <div id="RegistroVacaciones" style="display: none">
                                                <div class="modal-header">
                                                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios1()">&times;</button>
                                                    <h4 class="modal-title" id="fomTitleLabel">Convenio de vacaciones</h4>
                                                </div>
                                                <div class="modal-body">
                                                    <div class="panel panel-default">
                                                        <div class="panel-body">
                                                            <div class="col-lg-12">
                                                                <div class="form-group col-lg-6">
                                                                    <label>Fecha:</label>
                                                                    <input type="text" class="form-control" id="txtfechaV">
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Cédula:</label>
                                                                    <input type="text" class="form-control" id="txtCedulaV" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Nombre del Colaborador:</label>
                                                                    <input type="text" class="form-control" id="txtNombreColaboradorV" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Departamento:</label>
                                                                    <input type="text" class="form-control" id="txtDepartamentoV" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-12">
                                                                    <label>Jefe inmediato:</label>
                                                                    <input type="text" class="form-control" id="txtJefaAreaV" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-12">
                                                                    <label>Reemplazo:</label>
                                                                    <select id="txtReemplazo" class="form-control">
                                                                    </select>
                                                                    <!--<input type="text" class="form-control" id="txtReemplazo">-->
                                                                </div>
                                                                <div class="col-lg-3">
                                                                    <label>Fecha Desde:</label>
                                                                    <input type="text" class="form-control" id="frmTxtHoraDesdeV" onchange="DiasVacaciones()">
                                                                    <p class="help-block" id="frmTxtHoraDesdeMsg"></p>
                                                                </div>
                                                                <div class="col-lg-3">
                                                                    <label>Fecha Hasta:</label>
                                                                    <input type="text" class="form-control" id="frmTxtHoraHastaV" onchange="DiasVacaciones()">
                                                                    <p class="help-block" id="frmTxtHoraHastaMsg"></p>
                                                                </div>
                                                                <div class="col-lg-3">
                                                                    <label>Feriados:</label>
                                                                    <input type="text" class="form-control" id="frmTxtTiempoF" value="0" onchange="DiasVacaciones()">
                                                                    <p class="help-block"></p>
                                                                </div>
                                                                <div class="col-lg-3">
                                                                    <label>Dias:</label>
                                                                    <input type="text" class="form-control" id="frmTxtTiempoDiasV" value="0" disabled>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                                <div class="form-group col-lg-3">
                                                                    <label>Saldo de vacaciones</label>
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <div class="" style="text-align: center">
                                                                    <button id="btnGuardar1" onclick="GuardarSolicitud1()" type="button" class="btn btn-primary">Solicitar</button>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div id="RegistroPermisos" style="display: none">
                                                <div class="modal-header">
                                                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios2()">&times;</button>
                                                    <h4 class="modal-title" id="fomTitleLabel">Convenio de permisos en horas laborales</h4>
                                                </div>
                                                <div class="modal-body">
                                                    <div class="panel panel-default">
                                                        <div class="panel-body">
                                                            <div class="col-lg-12">
                                                                <div class="form-group col-lg-6">
                                                                    <label>Fecha:</label>
                                                                    <input type="text" class="form-control" id="txtfechaP">
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Cédula:</label>
                                                                    <input type="text" class="form-control" id="txtCedulaP" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Nombre del Colaborador:</label>
                                                                    <input type="text" class="form-control" id="txtNombreColaboradorP" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Departamento:</label>
                                                                    <input type="text" class="form-control" id="txtDepartamentoP" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-12">
                                                                    <label>Jefe de Area:</label>
                                                                    <input type="text" class="form-control" id="txtJefaAreaP" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-12">
                                                                    <label>Actividad a realizar:</label>
                                                                    <input type="text" class="form-control" id="txtActividadP">
                                                                </div>
                                                                <div class="col-lg-3">
                                                                    <label>Hora Desde:</label>
                                                                    <input type="text" class="form-control" id="frmTxtHoraDesdeP">
                                                                    <p class="help-block" id="frmTxtHoraDesdePMsg"></p>
                                                                </div>
                                                                <div class="col-lg-3">
                                                                    <label>Hora Hasta:</label>
                                                                    <input type="text" class="form-control" id="frmTxtHoraHastaP">
                                                                    <p class="help-block" id="frmTxtHoraHastaPMsg"></p>
                                                                </div>
                                                                <div class="col-lg-6">
                                                                    <label>Tiempo:</label>
                                                                    <input type="text" class="form-control" id="frmTxtTiempoP" disabled>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                                <div class="form-group col-lg-3">
                                                                    <label>Cargo a vacaciones</label>
                                                                </div>
                                                                <div class="form-group col-lg-3">
                                                                    <label>SI</label>
                                                                    <input class="form-check-input" type="checkbox" value="" id="IdSI">
                                                                </div>
                                                                <div class="form-group col-lg-3">
                                                                    <label>NO</label>
                                                                    <input class="form-check-input" type="checkbox" value="" id="IdNO">
                                                                </div>
                                                                <div class="col-lg-12">
                                                                    <label>Observaciones:</label>
                                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtObservacionesP">
																	</textarea>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <div class="" style="text-align: center">
                                                                    <button id="btnGuardar2" onclick="GuardarSolicitud2()" type="button" class="btn btn-primary">Solicitar</button>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div id="ListaSolicitud" style="display: block">
                                                <div class="col-lg-12" style="padding: 0px">
                                                    <div class="panel panel-default">
                                                        <!--<div class="panel-heading">
														Registro de Solicitud
														</div>-->
                                                        <div class="panel-body">
                                                            <!--<div class="row">
																<div class="panel panel-default">
																	<div class="panel-heading">																
																		<label>Tipo Solicitud</label>
																	</div>
																</div>
																<div class="form-group col-lg-3">
																	<select id="cboSolicitud" class="form-control" onchange="BuscarSolicitud()">
																	</select>
																	<p class="help-block"></p>
																</div>																
																
																<div class="form-group col-lg-3" id="idGerente" runat="server">
																	<label>Colaborador</label>
																	<input type="text" class="form-control" id="txtEmpledos">
																	<p class="help-block"></p>
																</div>																
																<div class="form-group col-lg-3">
																<button id="btnConsulta" onclick="BtnPermiso()" type="button" class="btn btn-primary">Permiso</button>
																<button id="btnDescarga" onclick="BtnVacaciones()" type="button" class="btn btn-primary">Vacaciones</button>
																</div>																
															</div>-->
                                                            <div class="row">
                                                                <div class="panel panel-default">
                                                                    <div class="panel-heading">
                                                                        <label>Consulta de la solicitud</label>
                                                                    </div>
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
                                                                <div class="form-group col-lg-2">
                                                                    <label>Filtrar sin fecha</label>
                                                                    <input class="form-check-input" type="checkbox" value="" id="idFiltros">
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <div class="form-group col-lg-3">
                                                                    <label>Tipo Solicitud</label>
                                                                    <select id="cboSolicitud" class="form-control">
                                                                    </select>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                                <div class="form-group col-lg-3">
                                                                    <label>Estado Solicitud</label>
                                                                    <select id="cboEstadoSolicitud" class="form-control">
                                                                    </select>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div class="panel-footer">
                                                            <div class="" style="text-align: center">
                                                                <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-primary">Consultar</button>
                                                                <button id="btnDescarga" onclick="BtnDescarga()" type="button" class="btn btn-primary">Descargar XLS</button>
                                                                <button id="btnProcesar" onclick="BtnProcesar()" type="button" class="btn btn-primary">Procesar en SAP</button>
                                                                <button id="btnPlanificar" onclick="BtnPlaficar()" type="button" class="btn btn-primary">Enviar Email Planif. Vacaciones</button>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-12" style="padding: 0px">
                                                        <div class="panel panel-default">
                                                            <div class="panel-heading">
                                                                <div>
                                                                    <h4 class="" id="listPrincipalTitleLabel">Detalle de Solicitud</h4>
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
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="tab-pane fade" id="profile-pills">
                                <div class="panel panel-default">
                                    <div class="panel-body">
                                        <div class="row">
                                            <div class="form-group col-lg-4">
                                                <label>Usuarios</label>
                                                <select id="cmbUsuarios" class="form-control" onclick="CargarDetalleVacaciones()">
                                                </select>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div id="DetalleIndividual" style="display: none">
                                                <div class="col-lg-12">
                                                    <div class="form-group col-lg-3">
                                                        <label>Saldo de vacaciones</label>
                                                    </div>
                                                    <div class="col-lg-12">
                                                        <label style="color: red;" id="Idlista1">---</label>
                                                        <label style="color: red;" id="Idlista2">---</label>
                                                    </div>
                                                    <div class="form-group col-lg-12">
                                                        <!-- /.panel-heading -->
                                                        <div class="panel-body" style="height: 200px; overflow-y: auto; overflow-x: auto;">
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
                                            <div id="DetalleMasivo" style="display: none">
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
        <div class="modal fade" id="modalCargarProceso" tabindex="-1" role="dialog" aria-labelledby="modalCargarProcesoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalCargarProcesoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="mymodalCargarProcesoLabel">Aprobar o rechazar Solicitud</h4>
                    </div>
                    <div class="modal-body" id="modalCargarProcesoMensaje">
                        <div class="form-group col-lg-12">
                            </br>
							<label>Estado</label>
                            <select id="cboEstado2" class="form-control">
                            </select>
                        </div>
                        <div class="form-group col-lg-12">
                            <label>Motivo:</label>
                            <textarea class="form-control" id="txtDescripcionReq2" name="txtDescripcionReq" rows="2" cols="50"></textarea>
                            <p class="help-block"></p>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" id="txtCodigoItem2" disabled style="visibility: hidden">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="ActualizarProceso()" class="btn btn-primary" id="btnBorrarArchivo2" hidden>Guardar</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <!-- Modal -->
        <div class="modal fade" id="modalEnvioMail" tabindex="-1" role="dialog" aria-labelledby="modalCargarProcesoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalEnvioMailTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="mymodalEnvioMailLabel">Enviar Mail</h4>
                    </div>
                    <div class="modal-body" id="modalEnvioMailMensaje">
                        <div class="form-group col-lg-12">
                            <label>Enviar Email Destinatario:</label>
                            <input type="text" class="form-control" id="txtEmailEnvio">
                        </div>
                        <div class="form-group col-lg-12">
                            <label>Detalle Mail:</label>
                            <textarea class="form-control" id="txtDetalleMail" name="txtDetalleMail" rows="4" cols="50"></textarea>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" id="txtCodigoItem3" disabled style="visibility: hidden">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="GuardarEnvioCorreo()" class="btn btn-primary" id="btnBorrarArchivo3" hidden>Enviar Mail</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

    </div>
</asp:Content>

