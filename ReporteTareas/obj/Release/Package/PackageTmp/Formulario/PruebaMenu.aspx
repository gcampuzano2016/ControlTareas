<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="PruebaMenu.aspx.cs" Inherits="ReporteTareas.Formulario.PruebaMenu" %>
   <asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/PruebaMenu.js?v=8" type="text/javascript"></script>
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
							<button id="btnGuardar" onclick="GuardarDatosMenu()" type="button" class="btn btn-success">Guardar</button>	
                            <button id="btnAgregar" onclick="AgregarNuevo()" type="button" class="btn btn-success">Nuevo</button>	


                        </div>
                    </div>

         <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <div>
                            
                            <h4 class="" id="listPrincipalTitleLabel">Lista de Menu</h4>
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
            <asp:Panel ID="Panel3" runat="server" Visible="true" Enabled="true">
            <div id="formActualizaTarea" class="panel panel-default col-lg-12" style="display: none;">

                            <!-- Modal -->
                            <div class="" id="formModalActualizacion" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" >
                                <div class="modal-dialog">
                                    <div class="modal-content">
                                        <div class="modal-header">
                                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios()">&times;</button>
                                            <h4 class="modal-title" id="fomTitleLabel">Nuevo Menu /SubMenu</h4>
                                        </div>
                                        <div class="modal-body">

                    <!-- Form -->
                            <div class="panel panel-default">
                                <div class="panel-body">
                            
                                        <div class="col-lg-12">
                                            <form role="form">                                                            
                                                <div class="col-lg-6">
                                                    <label>Seleccione el tipo de menú :</label>
                                                    <div class="" style="text-align: left">
                                                        <button id="btnMenu" onclick="BtnMenu()" type="button" class="btn btn-default" style="background-color:lightblue">MENU PRINCIPAL</button>
                                                        <br />
                                                        <br />
                                                       
							                            <button id="btnSubMenu" onclick="BtnSubMenu()" type="button" class="btn btn-default"style="background-color:lightblue">SUB-MENU</button>	


                                                </div>
                                                   
                                                    <div id="divComboPadre"  >
                                                     <label>Seleccionar Menú Principal :</label><br />

                                                     <select id="cmbTipoMenuDos" class="form-control" >
                                                          
                                                        </select>
                                                        </div>
                                                    <p class="help-block" id="cmbEstadoSeleccionadoMsg"></p>
                                                    
                                                </div>
                                                
                                                <div class="col-lg-12">
                                                    <label>Título:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtTitulo">
                                                    </textarea>
                                                    <p class="help-block"></p>

                                                </div>
                                                 <div class="col-lg-12" id="divReferencia" style="display:none">
                                                    <label>Ingrese la dirección HTML de la página:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtRef"  placeholder="Ejemplo: Principal.aspx" >
                                                    </textarea>
                                                    <p class="help-block" id="frmTxtTareaDetalleMsg3"></p>
                                                </div>
                                                <div class="col-lg-12">
                                                    <label>Descripción:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtDescripcion"  >
                                                    </textarea>
                                                    <p class="help-block" id="frmTxtTareaDetalleMsg2"></p>
                                                </div>
                                                <div class="col-lg-12">
                                                    <label>Icono:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtIcono"  placeholder="Ejemplo: fa fa-user fa-fw" >
                                                    </textarea>
                                                    <p class="help-block" id="frmTxtTareaDetalleMsg"></p>
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
                                            <button type="button" onclick="Agregar()" class="btn btn-primary" id="btnGuardarCambioEstado" hidden>Agregar</button>
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

                         <asp:Panel ID="Panel1" runat="server" Visible="true" Enabled="true">
            <div id="formEditarIcono" class="panel panel-default col-lg-12" style="display: none;">

                            <!-- Modal -->
                            <div class="" id="formEditarIcono2" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" >
                                <div class="modal-dialog">
                                    <div class="modal-content">
                                        <div class="modal-header">
                                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios2()">&times;</button>
                                            <h4 class="modal-title" id="fomTitleLabel2"> Editar Menú /SubMenú</h4>
                                        </div>
                                        <div class="modal-body">

                    <!-- Form -->
                            <div class="panel panel-default">
                                <div class="panel-body">
                            
                                        <div class="col-lg-12">
                                            <form role="form">                                                            
                                                
                                                <div class="col-lg-12">
                                                    <label>Icono Actual:</label>
                                                    <input type="text" class="form-control" id="frmTxtIconoActual" >
                                                    <p class="help-block" id="frmTxtEstadoActualMsg5"></p>
                                                </div>
                                                
                                                <div class="col-lg-12">
                                                    <label>Referencia:</label>
                                                    <input type="text" class="form-control" id="frmTxtReferenciaActual" >
                                                    <p class="help-block" id="frmTxtEstadoActualMsg"></p>
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
                                            <button type="button" onclick="CancelarCambios2()" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                                            <button type="button" onclick="GuardarDatosIconoRef()" class="btn btn-primary" id="btnGuardarIcono" hidden>Guardar</button>
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
                    </div>

                </div>
            </div>
        
                   
                 </div>


    
</asp:Content>