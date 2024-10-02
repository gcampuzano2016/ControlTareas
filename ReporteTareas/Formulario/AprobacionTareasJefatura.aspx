﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="AprobacionTareasJefatura.aspx.cs" Inherits="ReporteTareas.Formulario.AprobacionTareasJefatura" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/aprobacionTareasJefatura.js?v=1" type="text/javascript"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
   
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
                <div class="panel-body">
                        <h3>Listado de Recursos con el Total de Horas Diarias</h3>
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
                                <div class="form-group col-lg-4">
                                    <label>Fecha desde:</label>
                                    <input type="text" class="form-control" id="txtFechaDesde">
                                    <p class="help-block"></p>
                                </div>
                                <div class="form-group col-lg-4">
                                    <label>Fecha Hasta:</label>
                                    <input type="text" class="form-control" id="txtFechaHasta">
                                    <p class="help-block"></p>
                                </div>
                                <div class="form-group col-lg-4">
                                            <label>Usuarios</label>
                                            <select id="cmbUsuarios" class="form-control">
                                            </select>
                                </div>
                            </div>
							<div class="row">
                            <div class="form-group col-lg-4">
                                <label>Estado Aprobación</label>
                                <select id="cmbEstados" class="form-control" onchange="BuscarEstado()">
                                    <option value="1">PENDIENTE DE APROBACION</option>
                                    <option value="2">TAREAS APROBADAS</option>
                                    <option value="3">TAREAS NO EJECUTADAS</option>
                                </select>
                            </div>
                        </div>
                            <div class="row">
                                <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                            </div>

                        </div>
                        <div class="panel-footer">
                            <div class="" style="text-align:center">
                                <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-default">Consultar</button>
								<button style="display: none;" id="btnDescarga" onclick="BtnDescarga()" type="button" class="btn btn-default">Descargar XLS</button>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-lg-12" style="padding: 0px">
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <div>
                                <div style="float:right">
                                </div>
                                <h4 class="" id="listPrincipalTitleLabel">Resumen del Total de Horas Por Día</h4>
                            </div>
                        </div>
                        <!-- /.panel-heading -->
                        <div class="panel-body" style="height: 450px;overflow-y: auto;overflow-x: auto; display: block;" id="Pagina1">
                            <div id="table-datosTablaPrincipal1" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                <div class="row">                                   
                                    <div class="col-sm-12"  id='datosTablaPrincipal'>
                                    </div>
                                </div>
                            </div>
                            <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align:center">
                            </div>
                        </div>
                        <div class="panel-body" style="height: 450px;overflow-y: auto;overflow-x: auto;display: none;" id="Pagina2">
                            <div id="table-datosTablaPrincipal2" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                <div class="row">                                   
                                    <div class="col-sm-12"  id='datosTablaPrincipal1'>
                                    </div>
                                </div>
                            </div>
                            <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align:center">
                            </div>
                        </div>						
                        <!-- /.panel-heading -->						
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
                                                <div class="col-lg-6" id="divFrmTxtOrdenServicio">
                                                    <label>No. Orden de Servicio:</label>
                                                    <input type="text" class="form-control" id="frmTxtOrdenServicio" disabled>
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
                                                <div class="col-lg-6" id="divFrmTxtCodigoResponsable" style="display:none">
                                                    <label>Código Responsable:</label>
                                                    <input type="text" class="form-control" id="frmTxtCodigoResponsable" disabled>
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="col-lg-6" id="divFrmBtnCambiarTareaPrincipal" style="height: 65px;vertical-align: -webkit-baseline-middle;text-align: -webkit-center;display:none">
                                                    <br><button type="button" onclick="SeleccionarListaTareasCambioPorUsuario($('#frmTxtCodigo').val(), $('#frmTxtCodigoResponsable').val())" class="btn btn-info" id="btnCambiarTareaPrincipal" hidden>Cambiar Tarea Principal</button>
                                                </div>
                                                <div class="col-lg-12" id="divFrmTxtTareaPrincipalDescripcion">
                                                    <label>Descripción Actividad:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtTareaPrincipalDescripcion"  disabled>
                                                    </textarea>
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="col-lg-6" id="divFrmCmbTipoActividad">
                                                    <label>Tipo de actividad:</label>
                                                    <select id="frmCmbTipoActividad" class="form-control">
                                                    </select>
                                                    <p class="help-block"></p>
                                                </div>
                                                <div class="col-lg-12">
                                                    <label>Tarea:</label>
                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtTareaDetalle"   maxlength="30" placeholder="solo permite ingresar 30 Carácteres">
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
                        <div class="panel-body" style="height: 450px;overflow-y: auto;overflow-x: auto;">
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
            <div class="col-lg-12" id="dialogListaTareaCambio" style="display: none; width: 100%;">

                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <div>
                                <div style="float:right">
                                    <button type="button" onclick="CerrarSeleccionTarea()" class="btn btn-default">Cerrar</button>
                                </div>
                                <h4 class="" id="listModalTitleLabelCambio">Titulo Lista de Selección</h4>
                            </div>
                        </div>
                        <!-- /.panel-heading -->
                        <div class="panel-body" style="height: 400px;overflow-y: auto;overflow-x: auto;">
                            <div id="table-datosTablaTareasSeleccionCambio" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                <div class="row">
                                    <div style="display: none">
                                        <input type="text" class="form-control" id="frmTxtCodigoCambio" disabled>
                                    </div>
                                    <div class="col-sm-12"  id='datosTablaTareasSeleccionCambio'>
                                
                                            
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

        <!-- Listado Detalles -->
        <div class="col-lg-12" style="padding: 0px;" id="divListadoDetalleTareasDiarias">
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <div>
                                <div style="float:right">
                                </div>
                                <h4 class="" id="listDetailTitleLabel">Listado de Tareas</h4>
                            </div>
                        </div>
                        <!-- /.panel-heading -->
                        <div class="panel-body" style="height: 450px;overflow-y: auto;overflow-x: auto;">
                            <div id="table-datosTablaDetail" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                <div class="row">
                                    
                                    <div class="col-sm-12"  id='datosTablaDetail' style="padding: 0px">
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
        <!-- Listado Detalles -->
    
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
        <div class="modal fade" id="modalMensajeAprobar" tabindex="-1" role="dialog" aria-labelledby="modalMensajeAprobarLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #d9edf7" id="modalMensajeAprobarTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalMensajeAprobarLabelTitulo">Confirmar</h4>
                    </div>
                    <div class="modal-body"  id="divMensajeAprobar">
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" style="display: none;" id="txtCodigo">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                        <button type="button" id="btnAceptarAprobar" onclick="" class="btn btn-default">Aceptar</button>
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

