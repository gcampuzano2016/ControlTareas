<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="Perfiles.aspx.cs" Inherits="ReporteTareas.Formulario.Perfiles" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/Perfiles.js?v=5" type="text/javascript"></script>
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>	
    <script src="//cdnjs.cloudflare.com/ajax/libs/numeral.js/2.0.6/numeral.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div id="page-wrapper" style="padding: 0px">
                <div class="row">
                    <div class="col-lg-12"style="padding: 20px">

                        <div class="panel-heading">
                        
						<div style="display: none">
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                        </div>
                    </div>

                            <div class="panel-body">
                                    <div class="row"   >     
                                        <H3 >Perfiles </H3>  
                                    </div>
                              </div>
                                </div>
                                </div>
                                <div class="panel panel-default">
                                    <div class="panel-heading">
                                        Lista de Perfiles
                                     </div>
                                </div>

                                <div class="form-group col-lg-4" id="divCmbPerfiles">
                                <label>Tipo Perfiles:</label>
                                <select id="CmbPerfiles" class="form-control">
                                </select>
                                <p class="help-block"></p>
                                </div>
                            <div class="row">
                                <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                            </div>

                    <div class="panel-footer">
                        <div class="" style="text-align: center">
                            <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-default">Consultar</button>
							<button id="btnGuardar" onclick="GuardarDatosUsuario()" type="button" class="btn btn-success">Guardar</button>	
                            <button id="btnDescargar" onclick="DescargarPerfiles()" type="button" class="btn btn-success">DescargarXLS</button>									


                        </div>
                    </div>

         <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <div>
                            
                            <h4 class="" id="listPrincipalTitleLabel">Listado de Usuarios</h4>
                        </div>
                    </div>
                    <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                        <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                            <div class="row">

                                <div class="col-sm-12" id='datosTablaPrincipal' style="padding: 0px">
                                </div> 
                            </div>          
                        </div>
                        <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align: center">
                        </div>
                    </div>

                </div>
            </div>

                 </div>
</asp:Content>
