<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ReporteTareasDetalle.aspx.cs" Inherits="ReporteTareas.Formulario.ReporteTareasDetalle" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%--    <script src="../js/actualizarTareas.js" type="text/javascript"></script>--%>
    <script src="../js/reporteTareasDetalle.js" type="text/javascript"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="panel-body">
            <h3>Reporte de Tareas</h3>
        </div>
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading" style="display: none">
                        Filtros
                            <div style="display: none">
                                <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            </div>
							<div class="row">
								<div class="form-group col-lg-3">
                                    <label>Tipo Reporte</label>
                                    <select id="cboReporte" class="form-control" oninput="VerTipoReporte()">
                                        <option value="1">REPORTE GENERAL</option>
                                        <option value="2">TAREAS NO EJECUTADAS</option>
                                        <option value="3">TAREAS SIN INFORMES</option>
                                    </select>
                                </div>							
							</div>
                    </div>					
                    <div class="panel-body" id="ReporteGeneral" style="display: block">
                        <div class="row">
                            <div class="form-group col-lg-2">
                                <label>Categoria de Servicio:</label>
                                <select id="frmCmbTipoGasto" class="form-control">
                                </select>
                            </div>
                            <div class="form-group col-lg-3">
                                <label>Filtro de Fecha</label>
                                <select id="cmbTipoFecha" class="form-control" onclick="SeleccionarTipoFecha($('#cmbTipoFecha').val())" >
                                </select>
                            </div>
                            <div class="form-group col-lg-2">
                                <label>Fecha desde:</label>
                                <input type="text" class="form-control" id="txtFechaDesde">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-2">
                                <label>Fecha Hasta:</label>
                                <input type="text" class="form-control" id="txtFechaHasta">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-3">
                                <label>Usuarios</label>
                                <select id="cmbUsuarios" class="form-control">
                                </select>
                            </div>
                        </div>
                        <div class="row">
                            <div class="form-group col-lg-3">
                                <label>Número de Orden</label>
                                <input type="text" class="form-control" id="txtNumeroOrden">
                            </div>
                            <div class="form-group col-lg-3">
                                <label>Clientes</label>
                                <input type="text" class="form-control" id="txtCliente">
                            </div>
                            <div class="form-group col-lg-3">
                                <label>Aprobacion Tarea</label>
                                <select id="cmbAprobacionTarea" class="form-control">
                                </select>
                            </div>
                            <div class="form-group col-lg-3">
                                <label>Aprobacion Tarea QA</label>
                                <select id="cmbAprobacionTareaQA" class="form-control">
                                </select>
                            </div>
                        </div>
                        <div class="row">
                            <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                        </div>
						<div class="panel-footer">
							<div class="" style="text-align: center">
								<button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-default">Consultar</button>
								<button id="btnDescarga" onclick="BtnDescarga()" type="button" class="btn btn-default">Descargar XLS</button>
							</div>
						</div>
						<div class="col-lg-12" style="padding: 0px">
							<div class="panel panel-default">
								<div class="panel-heading">
									<div>
										<div style="float: right">
										</div>
										<h4 class="" id="listPrincipalTitleLabel">Listado de Tareas</h4>
									</div>
								</div>
								<!-- /.panel-heading -->
								<div class="panel-body" style="height: 450px; overflow-y: auto; overflow-x: auto;">
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
					<div class="panel-body" id="ReporteTareaNoEjecutada" style="display: none">
						<div class="row">
							<div class="form-group col-lg-4">
								<label>Fecha desde:</label>
                                <input type="text" class="form-control" id="txtFechaDesdeNo">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-4">
                                <label>Fecha Hasta:</label>
                                <input type="text" class="form-control" id="txtFechaHastaNo">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-4">
                                <label>Número de Orden/Cod Aranda/Nombre Tecnico</label>
								<input type="text" class="form-control" id="txtUsuariosNo">
                            </div>
                        </div>	
						<div class="panel-footer">
							<div class="" style="text-align: center">
								<button id="btnConsultaNo" onclick="BtnConsultaNo()" type="button" class="btn btn-default">Consultar</button>
								<button id="btnDescargaNo" onclick="BtnDescargaNo()" type="button" class="btn btn-default">Descargar XLS</button>
							</div>
						</div>	
						<div class="col-lg-12" style="padding: 0px">
							<div class="panel panel-default">
								<div class="panel-heading">
									<div>
										<div style="float: right">
										</div>
										<h4 class="" id="listPrincipalTitleLabel">Tareas no ejecutadas</h4>
									</div>
								</div>
								<!-- /.panel-heading -->
								<div class="panel-body" style="height: 450px; overflow-y: auto; overflow-x: auto;">
										<div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
											<div class="row">
												<div class="col-sm-12" id='datosTablaPrincipalNo' style="padding: 0px">
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
					<div class="panel-body" id="ReporteTareaSinInforme" style="display: none">
						<div class="row">
							<div class="form-group col-lg-4">
								<label>Fecha desde:</label>
                                <input type="text" class="form-control" id="txtFechaDesdeSin">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-4">
                                <label>Fecha Hasta:</label>
                                <input type="text" class="form-control" id="txtFechaHastaSin">
                                <p class="help-block"></p>
                            </div>
                            <div class="form-group col-lg-4">
                                <label>Número de Orden/Cod Aranda/Nombre Tecnico</label>
								<input type="text" class="form-control" id="txtUsuariosSin">
                            </div>
                        </div>	
						<div class="panel-footer">
							<div class="" style="text-align: center">
								<button id="btnConsultaSin" onclick="BtnConsultaSin()" type="button" class="btn btn-default">Consultar</button>
								<button id="btnDescargaSin" onclick="BtnDescargaSin()" type="button" class="btn btn-default">Descargar XLS</button>
							</div>
						</div>	
						<div class="col-lg-12" style="padding: 0px">
							<div class="panel panel-default">
								<div class="panel-heading">
									<div>
										<div style="float: right">
										</div>
										<h4 class="" id="listPrincipalTitleLabel">Tareas sin informes</h4>
									</div>
								</div>
								<!-- /.panel-heading -->
								<div class="panel-body" style="height: 450px; overflow-y: auto; overflow-x: auto;">
										<div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
											<div class="row">
												<div class="col-sm-12" id='datosTablaPrincipalSin' style="padding: 0px">
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
        </asp:Panel>


        <asp:Panel ID="Panel3" runat="server" Visible="true" Enabled="true">
            <div id="formActualizaTarea" class="panel panel-default col-lg-12" style="display: none;">

                <!-- Modal -->
                <div class="" id="formModalActualizacion" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios()">&times;</button>
                                <h4 class="modal-title" id="fomTitleLabel">Titulo Formulario</h4>
                            </div>
                            <div class="modal-body">

                                <!-- Form -->
                                <div class="panel panel-default">
                                    <div class="panel-body">

                                        <div class="col-lg-12">
                                            <form role="form">
                                                <div class="col-lg-6" id="divFrmTxtCodigo">
                                                    <label>Código:</label>
                                                    <input type="text" class="form-control" id="frmTxtCodigo" disabled>
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
                                                <div class="col-lg-12" id="divFrmTxtTareaPrincipalDescripcion">
                                                    <label>Descripción Actividad:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtTareaPrincipalDescripcion" disabled>
                                                    </textarea>
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="col-lg-12">
                                                    <label>Tarea:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtTareaDetalle">
                                                    </textarea>
                                                    <p class="help-block" id="frmTxtTareaDetalleMsg"></p>
                                                </div>
                                                <div class="col-lg-6">
                                                    <label>Fecha:</label>
                                                    <input type="text" class="form-control" id="frmTxtFecha">
                                                    <p class="help-block" id="frmTxtFechaMsg"></p>
                                                </div>
                                                <div class="col-lg-6">
                                                    <label>Tiempo:</label>
                                                    <input type="text" class="form-control" id="frmTxtTiempo" disabled>
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="col-lg-6">
                                                    <label>Hora Desde:</label>
                                                    <input type="text" class="form-control" id="frmTxtHoraDesde">
                                                    <p class="help-block" id="frmTxtHoraDesdeMsg"></p>
                                                </div>
                                                <div class="col-lg-6">
                                                    <label>Hora Hasta:</label>
                                                    <input type="text" class="form-control" id="frmTxtHoraHasta">
                                                    <p class="help-block" id="frmTxtHoraHastaMsg"></p>
                                                </div>
                                                <div class="col-lg-12">
                                                    <label>Observaciones:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtObservaciones">
                                                    </textarea>
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="col-lg-12">
                                                    <label>¿Hora extra?</label>
                                                    <select id="frmcmbHorasExtras" class="form-control">
                                                        <option value="0"></option>
                                                        <option value="1">50%</option>
                                                        <option value="2">100%</option>
                                                    </select>
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="col-lg-12">
                                                    <p class="help-block" id="messageNotify" style="display: none;"></p>
                                                </div>
                                            </form>
                                        </div>

                                    </div>
                                </div>
                                <!-- /.Form -->

                            </div>
                            <div class="modal-footer">
                                <button type="button" onclick="CancelarCambios()" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                                <button type="button" onclick="GuardarCambiosTarea()" class="btn btn-primary" id="btnGuardarCambiosTarea" hidden>Guardar cambios</button>
                                <button type="button" onclick="GuardarNuevaTarea()" class="btn btn-primary" id="btnGuardarNuevaTarea" hidden>Guardar tarea</button>
                                <button type="button" onclick="BorrarTarea()" class="btn btn-primary" id="btnBorrarTarea" hidden>Borrar tarea</button>
                            </div>
                        </div>
                        <!-- /.modal-content -->
                    </div>
                    <!-- /.modal-dialog -->
                </div>
                <!-- /.modal -->

                <div class="row">
                    <div id="divFormDetalleTarea">
                    </div>


                </div>


            </div>
        </asp:Panel>
        <asp:Panel ID="Panel6" runat="server" Visible="true" Enabled="true">


            <div id="dialogListaTarea2" class="panel panel-default col-lg-12" style="display: none; width: 100%;">

                <!-- Modal -->
                <div class="col-lg-12" id="listModalTareas" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                                <h4 class="modal-title" id="listModalTitleLabel2">Titulo Formulario</h4>
                            </div>

                            <!-- div lista seleccion -->
                            <div id="" class="dataTables_wrapper form-inline dt-bootstrap no-footer" style="overflow-x: auto;">
                                <div class="row">
                                    <div class="col-sm-12" id=''>
                                    </div>
                                    <!-- /.table-responsive -->
                                    <!-- /.panel-body -->
                                </div>
                                <!-- /.panel -->
                            </div>
                            <!-- /.div lista seleccion -->


                            <div class="modal-footer">
                            </div>
                        </div>
                        <!-- /.modal-content -->
                    </div>
                    <!-- /.modal-dialog -->
                </div>
                <!-- /.modal -->


            </div>

            <div class="col-lg-12" id="dialogListaTarea" style="display: none; width: 100%;">

                <div class="panel panel-default">
                    <div class="panel-heading">
                        <div>
                            <div style="float: right">
                                <button type="button" onclick="CerrarSeleccionTarea()" class="btn btn-default">Cerrar</button>
                            </div>
                            <h4 class="" id="listModalTitleLabel">Titulo Lista de Selección</h4>
                        </div>
                    </div>
                    <!-- /.panel-heading -->
                    <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                        <div id="table-datosTablaTareasSeleccion" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                            <div class="row">

                                <div class="col-sm-12" id='datosTablaTareasSeleccion'>
                                </div>
                                <!-- /.table-responsive -->
                                <!-- /.panel-body -->
                            </div>
                            <!-- /.panel -->
                        </div>
                        <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align: center">
                        </div>
                    </div>
                    <div class="panel-footer">
                        <button type="button" onclick="CerrarSeleccionTarea()" class="btn btn-default">Cerrar</button>
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

    </div>
    <div>
    </div>

</asp:Content>



<%--<asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True"></asp:DropDownList>--%>

