<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="RegistroForeCastGD.aspx.cs" Inherits="ReporteTareas.Formulario.RegistroForeCastGD" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

	<link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
	<script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/RegistroForeCastGD.js?v=9" type="text/javascript"></script>
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
                <div class="panel-body">
                    <div class="row">
                        <h3>Registro Forecast</h3>
                    </div>
                </div>
            </div>
        </div>
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Registro
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
                            <li class="active" id="tab1"><a href="#home-pills" data-toggle="tab">Ingreso de Forecast</a>
                            </li>
                            <li id="tab2"><a href="#profile-pills" data-toggle="tab">Listas de Forecast</a>
                            </li>
                            <li id="tab3"><a href="#profile-reporte" data-toggle="tab">Dashboard</a>
                            </li>							
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div class="tab-pane fade in active" id="home-pills">
                                <div class="panel panel-default">
                                    <div class="panel-body">
                                        <div class="row">
                                            <div class="form-group col-lg-2">
                                                <label>Marca</label>
                                                <select id="cboMarca" class="form-control">
                                                </select>
                                            </div>
                                            <!--<div class="form-group col-lg-1">
                                                <label></label>
                                                <button class="btn btn-lg btn-success" onclick="BtnCrearMarca()" type="button">
                                                    <i class="ace-icon glyphicon-plus  bigger-110 icon-only" title="Registrar Marca"></i>
                                                </button>
                                            </div>	-->										
                                            <div class="form-group col-lg-3">
                                                <label>Cliente</label>
                                                <input type="text" class="form-control" id="cboCliente2" placeholder="Cliente" oninput="BuscarCliente2()">
                                                <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes2">
                                                </ul>
                                            </div>
                                            <div class="form-group col-lg-1">
                                                <label></label>
                                                <button class="btn btn-lg btn-success" onclick="BtnCrearCliente()" type="button">
                                                    <i class="ace-icon glyphicon-plus  bigger-110 icon-only" title="Registrar Clientes"></i>
                                                </button>
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>Nombre del Proyecto</label>
                                                <input type="text" class="form-control" id="txtDetalleProyecto" placeholder="Detalle Proyecto">
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>PVP Estimado</label>
                                                <input type="text" class="form-control" id="txtPrecioEstimado" placeholder="0.00" oninput="CalcularMargen($('#txtPrecioEstimado').val(),$('#txtUtilidad').val())">
                                                <input type="text" class="form-control" id="txtPrecioEstimado2" style="width: 0px; height: 0px; border-width: 0px">
                                            </div>
                                            <div class="form-group col-lg-1">
                                                <label>% Utilidad</label>
                                                <input type="text" class="form-control" id="txtUtilidad" placeholder="0.00" oninput="CalcularMargen($('#txtPrecioEstimado').val(),$('#txtUtilidad').val())">
                                                <input type="text" class="form-control" id="txtUtilidad2" style="width: 0px; height: 0px; border-width: 0px">
                                            </div>											
                                        </div>
                                        <div class="row">
                                            <div class="form-group col-lg-2">
                                                <label>Util. Bruta Estimada</label>
                                                <input type="text" class="form-control" id="txtUtilidadBruta" placeholder="0.00">
                                                <input type="text" class="form-control" id="txtUtilidadBruta2" style="width: 0px; height: 0px; border-width: 0px">
                                            </div>
                                            <div class="form-group col-lg-2" style="display: none" >
                                                <label>Fecha Facturación</label>
                                                <select id="cboFecha1" class="form-control">
                                                </select>
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>% Estado del Negocio</label>
                                                <select id="cboCierreNegocio" class="form-control">
                                                </select>
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>Prioridad</label>
                                                <select id="cboPrioridad" class="form-control" onchange="BuscarPrioridad()">
                                                </select>
                                            </div>											
											<div class="form-group col-lg-3">
												<label>Fecha Estimado de Cierre</label>
												<input type="text" class="form-control" id="txtFechaFacturacionProyecto" placeholder="Fecha Facturacion Proyecto" >
												<p class="help-block"></p>
											</div>												
											<div class="form-group col-lg-2">
												<label>Fecha Facturación</label>
												<input type="text" class="form-control" id="txtFechaInicioProyecto" placeholder="Fecha Inicio Proyecto" >
												<p class="help-block"></p>
											</div>											
                                            <div class="form-group col-lg-3" style="display: none">
                                                <label>Fecha Estimado de Cierre</label>
                                                <select id="cboFecha2" class="form-control">
                                                </select>
                                            </div>											
                                        </div>
										<div class="row">
											<div class="form-group col-lg-5">
											</div>	
											<div id="IdJustificacion" class="form-group col-lg-6" style="display: none">
                                                <label>Justificacion</label>
                                                <select id="cboJustificacion" class="form-control">
                                                    <option value="SOBREDIMENSIONAMIENTO">SOBREDIMENSIONAMIENTO</option>
                                                    <option value="CAMBIOS INTERNOS DEL CLIENTE">CAMBIOS INTERNOS DEL CLIENTE</option>
                                                    <option value="RELACIONAMIENTO">RELACIONAMIENTO</option>
                                                    <option value="TECNOLOGIA DE LA SOLUCIÓN">TECNOLOGIA DE LA SOLUCIÓN</option>
                                                    <option value="FALTA DE SEGUIMIENTO PARA CIERRE">FALTA DE SEGUIMIENTO PARA CIERRE</option>
                                                    <option value="PRECIO">PRECIO</option>
                                                    <option value="OPORTUNIDAD YA DECLARADA">OPORTUNIDAD YA DECLARADA</option>
                                                    <option value="FALTA INNOVACION">FALTA INNOVACION</option>
                                                    <option value="FALTA CAPACIDAD TECNICA">FALTA CAPACIDAD TECNICA</option>
                                                    <option value="DISEÑO DE LA SOLUCION">DISEÑO DE LA SOLUCION</option>														
                                                </select>											
											</div>	
										</div>
                                        <div class="row">
                                            <div class="form-group col-lg-12">
                                                <label>estado Actual y siguientes pasos</label>
                                                <textarea class="form-control" id="txtObservacion" name="txtObservacion" rows="2" cols="50"></textarea>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="form-group col-lg-3">
                                                <label>Gerente Cuenta</label>
                                                <select id="cboVendedor" class="form-control">
                                                </select>
                                            </div>										
                                            <div class="form-group col-lg-3">
                                                <label>Gerente Producto</label>
                                                <select id="cboGerente" class="form-control">
                                                </select>
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Instituciones</label>
                                                <select id="cboInstitucion" class="form-control">
                                                    <option value="SELECCIONAR">SELECCIONAR</option>
                                                    <option value="PUBLICO">PUBLICO</option>
                                                    <option value="PRIVADO">PRIVADO</option>
                                                </select>
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Segmento de Mercado</label>
                                                <select id="cboSegmentacion" class="form-control">
                                                </select>
                                            </div>
                                        </div>
										<!-- Nav tabs -->
                                        <div class="row">
                                            <div class="form-group col-lg-3">
													<label>Sucursal</label>
													<select id="cboSucursal" class="form-control" >
                                                    <!--<option value="SELECCIONAR SUCURSAL">SELECCIONAR SUCURSAL</option>
                                                    <option value="AMBATO">AMBATO</option>
                                                    <option value="CUENCA">CUENCA</option>
                                                    <option value="QUITO">QUITO</option>
                                                    <option value="GUAYAQUIL">GUAYAQUIL</option>
												    <option value="CUENTAS ESTRATEGICAS">CUENTAS ESTRATEGICAS</option>-->									
													</select>
                                            </div>
                                            <div class="form-group col-lg-2" style="display: none">
                                                <label>Registro Aprobado</label>
                                                <select id="cboRegistro" class="form-control">
                                                    <option value="SELECCIONAR">SELECCIONAR</option>
                                                    <option value=""></option>
                                                    <option value="SI">SI</option>
                                                    <option value="NO">NO</option>
                                                </select>
                                            </div>
											<div class="form-group col-lg-2">
												<label>Registro Aprobado</label>
												<input class="form-check-input" type="checkbox" value="" id="IdProceso">
											</div>											
                                            <div class="form-group col-lg-2">
                                                <label>Número registro</label>
                                                <input id="txtNumeroRegistro" class="form-control" placeholder="Número registro">
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Gerente Cuenta Fabricante</label>
                                                <input id="txtGerenteCuenFabricante" class="form-control" placeholder="Gerente Cuenta Fabricante">
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="form-group col-lg-3">
                                                <label>Lider Proyecto Cliente</label>
                                                <input id="txtLiderProyectoCliente" class="form-control" placeholder="Lider Proyecto Cliente">
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Contacto Lider Proyecto</label>
                                                <input id="txtContactoLiderProyecto" class="form-control" placeholder="Contacto Lider Proyecto">
                                            </div>											
                                            <div class="form-group col-lg-3">
                                                <label>Mayorista</label>
                                                <input id="txtMayorista" class="form-control" placeholder="Mayorista" style="display: none">
												<select id="txtMayorista" class="form-control">
                                                    <option value="SELECCIONAR">SELECCIONAR</option>
                                                    <option value="SYNNEX-WESTCON">SYNNEX-WESTCON</option>
                                                    <option value="MEGAMICRO">MEGAMICRO</option>
                                                    <option value="INACORP">INACORP</option>
													<option value="NEXSYS">NEXSYS</option>
													<option value="SIGLO XX1">SIGLO XX1</option>
													<option value="TECHDATA">TECHDATA</option>
													<option value="LOL">LOL</option>
													<option value="DURAPOWER">DURAPOWER</option>
													<option value="XEROX">XEROX</option>
													<option value="CASA DEL CABLE">CASA DEL CABLE</option>
													<option value="MULTIMAYORISTA">MULTIMAYORISTA</option>													
                                                </select>												
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Forma Pago</label>
                                                <input id="txtFormaPago" class="form-control" placeholder="Forma Pago">
                                            </div>											
                                        </div>
                                        <div class="row">
                                            <div class="form-group col-lg-8">
                                            </div>
                                            <div class="form-group col-lg-1">
                                                <button id="btnNuevo" onclick="NuevoForeCast()" type="button" class="btn btn-primary">Nuevo</button>
                                            </div>
                                            <div class="form-group col-lg-1">
                                                <button id="btnGuardar" onclick="GuardarNuevoForeCast()" type="button" class="btn btn-primary">Guardar</button>
                                            </div>
                                            <div class="form-group col-lg-1">
                                                <button id="btnActualizar" onclick="ActualizarForeCast()" type="button" class="btn btn-warning">Actualizar</button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="tab-pane fade" id="profile-pills">
                                <asp:Panel ID="Panel6" runat="server" Visible="true" Enabled="true">
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                Filtros
                                            </div>
                                            <div class="panel-body">
                                                <div class="row">
                                                    <div class="form-group col-lg-3">
                                                        <label>Gerente Producto</label>
                                                        <select id="cboGerente2" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Marca</label>
                                                        <select id="cboMarca2" class="js-example-basic-multiple6" name="sucursal[]" multiple="multiple">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Gerente Cuenta</label>
                                                        <select id="cboVendedor2" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Cliente</label>
                                                        <input type="text" class="form-control" id="cboCliente3" placeholder="Cliente" oninput="BuscarCliente3()">
                                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes3">
                                                        </ul>
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>
												<!-- Nav tabs -->
                                                <div class="row">
                                                    <div class="form-group col-lg-3">
                                                        <label>Sucursal</label>
                                                        <select id="cboSucursal2" class="js-example-basic-multiple" name="sucursal[]" multiple="multiple">
                                                            <!--<option value="SELECCIONAR SUCURSAL">SELECCIONAR SUCURSAL</option>
                                                            <option value="AMBATO">AMBATO</option>
                                                            <option value="CUENCA">CUENCA</option>
                                                            <option value="QUITO">QUITO</option>
                                                            <option value="GUAYAQUIL">GUAYAQUIL</option>
															<option value="CUENTAS ESTRATEGICAS">CUENTAS ESTRATEGICAS</option>-->
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Segmento de Mercado</label>
                                                        <select id="cboSegmentacion2" class="js-example-basic-multiple2" name="mercado[]" multiple="multiple">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Prioridad</label>
                                                        <select id="cboPrioridad2" class="js-example-basic-multiple3" name="mercado[]" multiple="multiple">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>% Estado del Negocio</label>
                                                        <select id="cboCierreNegocio2" class="js-example-basic-multiple7" name="mercado[]" multiple="multiple">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>
												<div class="row">
												    <div class="form-group col-lg-2">
                                                        <label>Filtrar sin fecha</label>
                                                        <input class="form-check-input" type="checkbox" value="" id="idFiltros" onchange="LimpiarBusqueda()">
                                                    </div>
													<div class="form-group col-lg-3">
														<label>Tipo Fecha</label>
														<select id="cboTipoFecha" class="form-control">															
															<option value="1">Fecha Facturación</option>
															<option value="2">Fecha Estimación Cierre</option>																
														</select>
														<p class="help-block"></p>
													</div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Años</label>
                                                        <select id="cboAnio1" class="form-control">
															<option value="0">-- SELECCIONE --</option>														
															<option value="2021">2021</option>
															<option value="2022">2022</option>															
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>													
													<div class="form-group col-lg-2">
														<label>Mes</label>
														<select id="cboFecha1Buscar" class="js-example-basic-multiple4" name="facturacion[]" multiple="multiple">
														</select>
													</div>													
                                                    <div class="form-group col-lg-4" style="display: none">
                                                        <label>Mes Est. de Cierre</label>
                                                        <select id="cboFecha2Buscar" class="js-example-basic-multiple5" name="cierre[]" multiple="multiple">
                                                        </select>
                                                    </div>												
												</div>
                                            </div>
                                            <div class="panel-footer">
                                                <div class="" style="text-align: center">
                                                    <button id="btnConsulta" onclick="BtnConsultaForeCast()" type="button" class="btn btn-primary">Consultar</button>
                                                    <button id="btnDescargar" onclick="BtnDescargaForeCast()" type="button" class="btn btn-primary">Descargar XLS</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="col-lg-12" style="padding: 20px">
                                            <div class="form-group col-lg-3">
                                                <label>Totales Oportunidades: </label>
                                                <input type="text" class="form-control" id="txtTotalOportunidad" disabled placeholder="0.00">
                                            </div>
                                            <div class="form-group col-lg-2">
                                                <label>PVP Estimado: </label>
                                                <input type="text" class="form-control" id="txtSubtotal" disabled placeholder="0.00">
                                            </div>
                                            <div class="form-group col-lg-3">
                                                <label>Utilidad Bruta Estimada: </label>
                                                <input type="text" class="form-control" id="txttotal" disabled placeholder="0.00">
                                            </div>
                                        </div>
                                    </div>
                                    <!--<br />
											<div class="row">
												<div class="col-xs-12" style="overflow: auto;">
													<div class="box box-primary">
														<div class="box-body">
															<table id="table-users" class="table table-striped table-bordered table-hover dataTable no-footer dtr-inline">
																<thead>
																	<tr>
																		<th>Marca</th>
																		<th>Cliente</th>
																		<th>Detalle del Proyecto</th>
																		<th>PVP Estimado</th>
																		<th>% Utilidad</th>
																		<th>Utilidad Bruta Estimada</th>
																		<th>Fecha de Facturación</th>
																		<th>% Cierre Negocio</th>
																		<th>Mes Estimado de Cierre</th>
																		<th>Observaciones</th>
																		<th>Gerente de Producto</th>
																		<th>Gerente de Cuenta</th>
																		<th>Segmento de Mercado</th>
																		<th>Prioridad</th>
																		<th>Sucursal</th>
																		<th>Registro Aprobado</th>
																		<th>Ult. reg. usuario</th>
																		<th>Ult. reg. fecha Modificado</th>
																	</tr>															
																</thead>
															</table>
														</div>
													</div>
												</div>
											</div>-->
                                    <br />
                                    <div class="col-lg-12" style="padding: 0px">
                                        <div class="panel panel-default">
                                            <div class="panel-heading">
                                                <div>
                                                    <h4 class="" id="listPrincipalTitleLabel">Lista de Oportunidades</h4>
                                                </div>
                                            </div>
                                            <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                                <div id="table-datosTablaPrincipal1" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                    <table id="table-users" class="table table-striped table-bordered table-hover dataTable no-footer dtr-inline">
                                                        <thead>
                                                            <th>Acciones</th>
                                                            <th>Marca</th>
                                                            <th>Cliente</th>
                                                            <th>Detalle del Proyecto</th>
                                                            <th>PVP Estimado</th>
                                                            <th>% Utilidad</th>
                                                            <th>Utilidad Bruta Estimada</th>
                                                            <th>Forma de pago</th>
                                                            <th>Fecha de Facturación</th>
                                                            <th>% Cierre Negocio</th>
                                                            <th>Fecha Estimado de Cierre</th>
                                                            <th>Observaciones</th>
                                                            <th>Gerente de Producto</th>
                                                            <th>Gerente de Cuenta</th>
                                                            <th>Segmento de Mercado</th>
                                                            <th>Prioridad</th>
                                                            <th>Sucursal</th>
                                                            <th>Registro Aprobado</th>
                                                            <th>Ult. reg. usuario</th>
                                                            <th>Ult. reg. fecha Modificado</th>
                                                        </thead>
                                                    </table>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- /.panel-heading -->
                                    <!--<div class="panel-body" style="height: 450px;overflow-y: auto;overflow-x: auto; display: block;" id="Pagina1">
												<div id="table-datosTablaPrincipal1" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
													<div class="row">                                   
														<div class="col-sm-12"  id='datosTablaPrincipalForeCast'>
														</div>
													</div>
												</div>
												<div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align:center">
												</div>
											</div>-->
                                </asp:Panel>
                            </div>
							<div class="tab-pane fade" id="profile-reporte">
								</br>
								<div class="row">
									<div class="col-lg-1">
                                    </div>
                                    <div class="col-lg-11">
										<iframe width="900" height="600" src="https://app.powerbi.com/view?r=eyJrIjoiOWU1ODUzYWEtOGE2ZS00YzI0LTg1OTUtYWQ0NTYzYjQzMzg2IiwidCI6IjgwODc0OTA5LTMzMjUtNDEzMS1iNmFiLTdiMDVjZDEwNmJhZSIsImMiOjR9&pageName=ReportSection9369fdc225283ef5a4f2" frameborder="0" allowFullScreen="true"></iframe>                                                        
                                    </div>
                                </div>							
							</div>
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
                        <h4 class="modal-title" id="modalMensajeAprobarLabelTitulo">Registro Cliente</h4>
                    </div>
                    <div class="modal-body" id="divMensajeAprobar">
                        <div class="form-group col-lg-12">
                            <label>Descripción:</label>
                            <!--<input type="text" class="form-control" id="txtObservacionReq">-->
                            <textarea class="form-control" id="txtObservacionReq" name="txtObservacionReq" rows="4" cols="50"></textarea>
                            <p class="help-block"></p>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" style="display: none;" id="txtCodigo">
                        <button type="button" class="btn btn-danger" data-dismiss="modal">Close</button>
                        <button type="button" id="btnAceptarAprobar" onclick="GuardarCliente()" class="btn btn-primary">Guardar</button>
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
