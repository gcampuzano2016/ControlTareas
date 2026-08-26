<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CambiarEstadoTarea.aspx.cs" Inherits="ReporteTareas.Formulario.CambiarEstadoTarea" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/cambiarEstadoTarea.js" type="text/javascript"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
   
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper"  style="padding: 0px">
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel-body">
                    <h3>Cambiar Estado de Tarea</h3>
                </div>
            </div>
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
                            <div style="display: none">
                                <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            </div>
                <div class="col-lg-12" style="padding: 0px">
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <div>
                                <div style="float:right">
                                </div>
                                <h4 class="" id="listPrincipalTitleLabel">Listado de Tareas</h4>
                            </div>
                        </div>
                        <!-- /.panel-heading -->
                        <div class="panel-body" style="height: 500px;overflow-y: auto;overflow-x: auto;padding: 0px;">
                            <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                <div class="row">
                                    
                                    <div class="col-sm-12"  id='datosTablaPrincipal'>
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
        </asp:Panel>


        <asp:Panel ID="Panel3" runat="server" Visible="true" Enabled="true">
            <div id="formActualizaTarea" class="panel panel-default col-lg-12" style="display: none;">

                            <!-- Modal -->
                            <div class="" id="formModalActualizacion" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" >
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
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtTareaPrincipalDescripcion"  disabled>
                                                    </textarea>
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="col-lg-6">
                                                    <label>Estado Actual:</label>
                                                    <input type="text" class="form-control" id="frmTxtEstadoActual" disabled>
                                                    <p class="help-block" id="frmTxtEstadoActualMsg"></p>
                                                </div>
                                                <div class="col-lg-6">
                                                    <label>Cambiar al Estado :</label>
                                                    <select id="cmbEstadoSeleccionado" class="form-control">
                                                    </select>
                                                    <p class="help-block" id="cmbEstadoSeleccionadoMsg"></p>
                                                </div>
                                                <div class="col-lg-12">
                                                    <label>Actividad a realizar:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtTareaDetalle"   maxlength="30" placeholder="solo permite ingresar 30 Carácteres">
                                                    </textarea>
                                                    <p class="help-block" id="frmTxtTareaDetalleMsg"></p>
                                                </div>
                                                <div class="col-lg-12">
                                                    <label>Observaciones:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtObservaciones">
                                                    </textarea>
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="col-lg-8">
                                                    <label>Adjuntar Archivos:</label>
                                                    <input type="file" id="archivosAdjuntos" multiple />  
                                                    <p class="help-block"></p>
                                                    <progress id="fileProgress" style="display: none"></progress><hr/><span id="lblMessage" style="color: Green"></span>
                                                </div>
                                                <div class="col-lg-4">
                                                    <br>
                                                    <input type="button" id="btnCargarArchivosAdjuntos" value="Cargar Archivos" class="btn btn-info" />  
                                                    <p class="help-block"></p>
                                                </div>
                                                
                                                <div class="col-lg-12" id="divArchivosAdjuntosAnteriores">
                                                    
                                                </div>
                                                <div class="col-lg-12" id="divArchivosAdjuntos">
                                                    
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
                                            <button type="button" onclick="GuardarCambioEstado()" class="btn btn-primary" id="btnGuardarCambioEstado" hidden>Guardar Cambio</button>
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
                            <div class="col-lg-12" id="listModalTareas" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" >
                                <div class="modal-dialog">
                                    <div class="modal-content">
                                        <div class="modal-header">
                                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                                            <h4 class="modal-title" id="listModalTitleLabel2">Titulo Formulario</h4>
                                        </div>

                    <!-- div lista seleccion -->
                                            <div id="" class="dataTables_wrapper form-inline dt-bootstrap no-footer" style="overflow-x: auto;">
                                <div class="row">
                                    <div class="col-sm-12"  id=''>
                                
                                            
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
                                <div style="float:right">
                                    <button type="button" onclick="CerrarSeleccionTarea()" class="btn btn-default">Cerrar</button>
                                </div>
                                <h4 class="" id="listModalTitleLabel">Titulo Lista de Selección</h4>
                            </div>
                        </div>
                        <!-- /.panel-heading -->
                        <div class="panel-body" style="height: 500px;overflow-y: auto;overflow-x: auto;">
                            <div id="table-datosTablaTareasSeleccion" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                <div class="row">
                                    
                                    <div class="col-sm-12"  id='datosTablaTareasSeleccion'>
                                
                                            
                                    </div>
                                        <!-- /.table-responsive -->
                                    <!-- /.panel-body -->
                                </div>
                                <!-- /.panel -->
                            </div>
                            <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align:center">
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
                        <h4 class="modal-title" id="modalMensajeInformativoLabel">Informativo</h4>
                    </div>
                    <div class="modal-body"  id="MensajeInformativo">
                                            
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
                    <div class="modal-body"  id="modalMensajeConfirmacioMensaje">
                                            
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" id="txtCodigoItem" disabled style="visibility:hidden">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="EliminarArchivo()" class="btn btn-primary" id="btnBorrarArchivo" hidden>Continuar Borrado</button>
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

