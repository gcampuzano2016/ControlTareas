<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="RegistroCoutas.aspx.cs" Inherits="ReporteTareas.Formulario.RegistroCoutas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/reporteAnual.js?v=1" type="text/javascript"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>	
    <script src="//cdnjs.cloudflare.com/ajax/libs/numeral.js/2.0.6/numeral.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="panel-body">
			<h3>Registro de Cuotas Anual</h3>
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
                                <div class="form-group col-lg-3">
                                    <label>Año</label>
                                    <select id="cboAnio" class="form-control">								
                                    </select>
                                </div>
                                <div class="form-group col-lg-3">
                                    <label>Tipo Cuotas</label>
                                    <select id="cboArea" class="form-control">
                                            <option value="0">SELECCIONAR TIPO CUOTAS</option>
                                            <option value="1">VENDEDORES</option>
                                            <option value="2">GERENTES</option>							
                                    </select>
                                </div>								
                            </div>
                            <div class="row">
                                <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                            </div>

                        </div>
                        <div class="panel-footer">
                            <div class="" style="text-align:center">
                                <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-primary">Consultar</button>
                                <button id="btnDescargar" onclick="BtnDescargar()" type="button" class="btn btn-primary">Descargar</button>	
								<button id="btnDescargar" onclick="GuardarCuota()" type="button" class="btn btn-success">Guardar</button>									
                            </div>
                        </div>
						<div class="col-lg-12" style="padding: 0px">
							<div class="panel panel-default">
								<div class="panel-heading">
									<div>
										<div style="float:right">
										</div>
										<h4 class="" id="listPrincipalTitleLabel">Detalle de Cuotas</h4>
									</div>
								</div>
								<!-- /.panel-heading -->
								<div class="panel-body" style="height: 450px;overflow-y: auto;overflow-x: auto;">
									<div class="panel-body">
										<!-- Nav tabs -->
										<ul class="nav nav-pills" id="questType">
										<li class="active"><a href="#Parte1-pills" data-toggle="tab">Registro de Metas Anual</a>
										</li>
										<li><a href="#Parte2-pills" data-toggle="tab">Registro de Cuota por Vendedor y Gerente</a>
										</li>						
										</ul>
										<!-- Tab panes -->	
										<div class="tab-content">
											<div class="tab-pane fade in active" id="Parte1-pills">
												<div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
													<div class="row">                                    
														<div class="col-sm-12"  id='datosTablaPrincipal2' style="padding: 0px">
														</div>
														<!-- /.table-responsive -->
														<!-- /.panel-body -->
													</div>
													<!-- /.panel -->
												</div>
												<div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align:center">
												</div>											
											</div>
											<div class="tab-pane fade" id="Parte2-pills">							
												<div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
													<div class="row">                                    
														<div class="col-sm-12"  id='datosTablaPrincipal' style="padding: 0px">
														</div>
														<!-- /.table-responsive -->
														<!-- /.panel-body -->
													</div>
													<!-- /.panel -->
												</div>
												<div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align:center">
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