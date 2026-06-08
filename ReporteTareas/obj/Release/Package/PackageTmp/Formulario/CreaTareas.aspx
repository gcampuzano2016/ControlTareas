<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CreaTareas.aspx.cs" Inherits="ReporteTareas.Formulario.CreaTareas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/CreaTarea.js"></script>
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
                        <h3>Creación de Ticket</h3>
                            <div style="display: none">
                                <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            </div>						
                    </div>
                </div>

            </div>
        </div>
        <asp:Panel ID="Panel2" runat="server" Visible="true" Enabled="true">
            <%--/////////////////////////////////////////////--%>
            <div class="row">
                <div class="col-lg-12">
                    <div class="panel panel-default">
                        <div class="panel-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <div class="row">
                                        <div class="row">
                                            <div class="col-lg-12">
                                                <div class="col-lg-4">
													<label>Num. OS</label>
													<input type="text" class="form-control"  id="txt_Os" placeholder="Num. OS">
													<p class="help-block"></p>														
                                                </div>

                                                <div class="col-lg-4">
													<label>Cliente</label>
													<input type="text" class="form-control"  id="txtCliente" placeholder="Cliente" oninput="BuscarCliente()">
                                                    <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboClientes">
                                                    </ul>													
													<p class="help-block"></p>													
                                                </div>
                                                <div class="col-lg-4">
													<label>Fecha Registro</label>
													<input type="text" class="form-control"  id="txtFechaDesde" placeholder="Fecha Registro">
													<p class="help-block"></p>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-lg-12">
											    <div class="col-lg-6">
													<label>Especialista</label>
													<input type="text" class="form-control"  id="txtecnico" placeholder="Especialista" oninput="BuscarEspecialista()">
                                                    <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboEspecialista">
                                                    </ul>													
													<p class="help-block"></p>												    
												</div>
                                                <div class="col-lg-6">
													<label>Categoria</label>
													<select id="cboCategoria" class="form-control">
														<option value="--SELECCIONE--">-- SELECCIONE --</option>													
														<option value="Requerimiento de Soporte">Requerimiento de Soporte</option>
														<option value="Requerimiento del Cliente">Requerimiento del Cliente</option>
														<option value="Solicitud de Ofertas Técnicas y Costeos">Solicitud de Ofertas Técnicas y Costeos</option>														
													</select>
													<p class="help-block"></p>												
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-lg-12">
                                                <div class="col-lg-6">												
													<label>Detalle de la Tarea</label>
													<textarea class="form-control" id="txt_DetCamEstado" name="txt_DetCamEstado" rows="4" cols="50"></textarea>
													<p class="help-block"></p>												
                                                </div>
                                                <div class="col-lg-1">
                                                </div>
                                            </div>
                                            <div class="col-lg-6">
                                            </div>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col-lg-12">
                                            <br />
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col-lg-12 col-md-12 col-sm-12">
                                            <div class="col-lg-4">
                                            </div>
                                            <div class="col-lg-4">
												 <button type="button" id="btnGuardar" onclick="GuardarTicket()" class="btn btn-primary">Guardar</button>
                                            </div>
                                            <div class="col-lg-4">
                                            </div>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col-lg-12">
                                            <br />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <%--///////////////////////////////////////////////////////////////////////////////////--%>
        </asp:Panel>
    </div>
</asp:Content>
