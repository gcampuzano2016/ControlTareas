<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ReportesSap.aspx.cs"  Inherits="ReporteTareas.Formulario.ReportesSap" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    
    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>


    <script src="../js/ReportesSap.js?v=1"></script>

    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>
    <link href="../dist/css/Montos.css" rel="stylesheet" />

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    ReportesSap.aspx.cs

    <section id="page-wrapper" style="padding: 0;">
        <header class="col-lg-12" style="background-color: #003F64;">
            <nav class="row" style="margin-left: 0; margin-right: 0;">
                <ul class="breadcrumb" style="margin-top:1.5rem;">
                    <li>
                        <i class="ace-icon fa fa-home home-icon"></i>
                        <a href="#">Finanzas</a>
                    </li>
                    <li>
                        <a href="#">Reportes SAP</a>
                    </li>
                </ul>
            </nav>
        </header>

        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <section class="col-lg-12" style="padding: 0px; margin:0; background-color: #9DA8AD; height: max-content;">
                <article class="panel panel-default" style="border:none; "> 
                    <div>
                        <div style="display: none">
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtPerfil" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                        </div>
                    </div>
                    
                </article>
            
               <div style="display:block; padding: 0 1rem 0.2rem 1rem;">
                    <!--  Cuadro de ingreso  -->
                    <div class="col-sm-12" style="background-color:#f0f0f0cf; margin-right:1rem; border-radius:5px; color:black; font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">
                        <div class="text-center" style="font-weight:700;">
                            <h2>Datos de Facturas</h2>
                        </div>
                        <div class="horizontal-group-evenly col-sm-12  text-center" style="margin-left:2.5rem; font-weight:600;">
                            <div class="horizontal-group-evenly form-group col-sm-5">
                                <label class="col-sm-4 control-label">Fecha de inicio</label>
                                <div class="" style="width:50%; margin-left:-4rem;">
                                    <input type="date" style="cursor: pointer;" class="form-control input-sm" id="txtFechaInicio" value="">
                                </div>
                            </div>
                            <div class="horizontal-group-evenly form-group col-sm-5">
                                <label class="col-sm-4 control-label">Fecha de fin</label>
                                <div class="" style="width:50%; margin-left:-4rem;">
                                    <input type="date" style="cursor: pointer;" class="form-control input-sm" id="txtFechaFin" value="">
                                </div>
                            </div>
                        </div>
                        <div class="horizontal-group-evenly col-sm-12  text-center" style="margin-left:2.5rem; font-weight:600;">
                            <div class="horizontal-group-evenly form-group col-sm-5">
                                <label for="txtNombre" class="col-sm-4 control-label">N° Factura</label>
                                <div class="" style="width:50%; margin-left:-4rem;">
                                    <input type="text" class="form-control input-sm" id="txtFactura" value="">
                                </div>
                            </div>
                            <div class="horizontal-group-evenly form-group col-sm-5">
                                <label for="txtNombre" class="col-sm-4 control-label">Proveedor</label>
                                <div class="" style="width:50%; margin-left:-4rem;">
                                    <input type="text" class="form-control input-sm" id="txtProveedor" value="">
                                </div>
                            </div>
                        </div>
                    </div>

                   <div class="horizontal-group-simple col-sm-12">
                       <div class="input-group horizontal-group-simple col-sm-5" style="justify-content:center;">
                           <div class="btn-group" role="group"">
                                <button type="button" name="tipoTabla" id="completo" class="btn btn-custom" onclick="toggleButtonColor('completo')">Tabla completa</button>
                                <button type="button" name="tipoTabla" id="sumarizado" class="btn btn-custom" onclick="toggleButtonColor('sumarizado')">Resumen</button>
                           </div>
                       </div>
                       <div class="col-sm-5" id="btnCarga" style="display: flex; justify-content: center;">
                            <button class="btn btn-buscar col-sm-6" type="button" id="btnBuscar" onclick="BuscarProv()" style="margin: 3rem;">
                                <i class="fa fa-search" aria-hidden="true"></i> Buscar
                            </button>
                        </div>
                   </div>
                    

                   <div class="col-sm-12" style="background-color:rgb(7 36 56 / 94%); margin: 1rem 1rem 1rem 0; border-radius:5px; color:black; padding-bottom:2rem; display:block;"  id="tblProveedores">
                       <div>
                            <div class="box box-primary">
                            
                                <div class="box-header" style="color:aliceblue;">
                                    <div >
                                        <h3 class="box-title" style="padding:0 1rem 1rem 2rem;">Lista de Proveedores</h3>
                                    </div>
                                    
                                </div>
                            
                                <div class="box-body" style="padding:1rem; background-color:#92B8CE4D;">
                                    <div style="max-height: 600px; max-width: 100%; overflow-y: auto; overflow-x: auto; ">
                                        <table id="tbl_Proveedores" 
                                               style="font-size:85%; font-weight:700; width: 100%; margin:0.5rem 2rem 0.5rem 0;" 
                                               class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                            <thead>
                                              <tr class="bg-primary">                                        
                                                <th style="width:5%; min-width: 50px; text-align: center; vertical-align: inherit;">#</th>
                                                <th style="width:25%; min-width: 250px; text-align: center; vertical-align: inherit;">Nombre Proveedor</th>
                                                <th style="width:10%; min-width: 120px; text-align: center; vertical-align: inherit;">Valor</th>
                                                <th style="width:10%; min-width: 120px; text-align: center; vertical-align: inherit;">Abono</th>
                                                <th style="width:10%; min-width: 120px; text-align: center; vertical-align: inherit;">Saldo</th>
                                                <th style="width:15%; min-width: 180px; text-align: center; vertical-align: inherit;">Fecha1</th>
                                                <th style="width:15%; min-width: 180px; text-align: center; vertical-align: inherit;">Factura</th>
                                                <th style="width:35%; min-width: 300px; text-align: center;vertical-align: inherit;">Descripcion</th>
                                                <th style="width:10%; min-width: 50px; text-align: center; vertical-align: inherit;">Otro</th>
                                                <th style="width:10%; min-width: 50px; text-align: center; vertical-align: inherit;">Sum</th>
                                              </tr>
                                            </thead>
                                            <tbody>
                                                <!-- DATA POR MEDIO DE AJAX -->
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>          
                   
                   <div class="col-sm-12" style="background-color:rgb(7 36 56 / 94%); margin: 1rem 1rem 1rem 0; border-radius:5px; color:black; display: block; padding-bottom:2rem; display:none;" id="tblProveedoresRes">
                       <div>
                            <div class="box box-primary">
                            
                                <div class="box-header" style="color:aliceblue;">
                                    <div >
                                        <h3 class="box-title" style="padding:0 1rem 1rem 2rem;">Lista de Proveedores Sumarizada</h3>
                                    </div>
                                    
                                </div>
                            
                                <div class="box-body" style="padding:1rem; background-color:#92B8CE4D;">
                                    <div style="max-height: 600px; max-width: 100%; overflow-y: auto; overflow-x: auto; ">
                                        <table id="tbl_ProveedoresRes" 
                                               style="font-size:85%; font-weight:700; width: 100%; margin:0.5rem 2rem 0.5rem 0;" 
                                               class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                            <thead>
                                              <tr class="bg-primary">
                                                <th style="width:25%; min-width: 250px; text-align: center; vertical-align: inherit;">Nombre Proveedor</th>
                                                <th style="width:10%; min-width: 120px; text-align: center; vertical-align: inherit;">Valor</th>
                                                <th style="width:10%; min-width: 120px; text-align: center; vertical-align: inherit;">Abono</th>
                                                <th style="width:10%; min-width: 120px; text-align: center; vertical-align: inherit;">Saldo</th>
                                              </tr>
                                            </thead>
                                            <tbody>
                                                <!-- DATA POR MEDIO DE AJAX -->
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

               </div>

                    <!--------------------------------------------->
                    <!--        Modal cuadro SUMATORIA           -->
                    <!--------------------------------------------->
                    <!-- Modal -->
                    <div id="msgDatosSumarizados" class="modal fade" role="dialog">
                        <div class="modal-dialog">
                            <!-- Modal content-->
                            <div class="modal-content">
                                <div class="modal-header bg-info">
                                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                                    <h4 class="modal-title">DATOS SUMARIZADOS</h4>
                                </div>
                                <div class="modal-body">
                                        
                                    <div class="horizontal-group-start" style="margin-top:0;">
                                        <p style="margin-right: 1rem; justify-content:center;">Datos Sumarizados:</p>
                                        <label id="txtNombreUsuario"></label>                                    
                                    </div>  
                                    <section class="contenedor-horizontal">
                                        
                                        <div>
                                            <div class="box box-primary">                            
                            
                                                <div class="box-body" style="padding:1rem; background-color:rgb(7 36 56 / 94%);">
                                                    <div style="max-height: 600px; max-width: 100%; overflow-y: auto; overflow-x: auto; ">
                                                        <table id="tbl_ProveedoresSum" 
                                                               style="font-size:85%; font-weight:700; width: 100%; margin:0.5rem 2rem 0.5rem 0;" 
                                                               class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                            <thead>
                                                              <tr class="bg-primary">
                                                                <th style="width:10%; min-width: 120px; text-align: center; vertical-align: inherit;">Valor</th>
                                                                <th style="width:10%; min-width: 120px; text-align: center; vertical-align: inherit;">Fecha</th>
                                                                <th style="width:10%; min-width: 180px; text-align: center; vertical-align: inherit;">Descripción</th>
                                                              </tr>
                                                            </thead>
                                                            <tbody>
                                                                <!-- DATA POR MEDIO DE AJAX -->
                                                            </tbody>
                                                        </table>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                    </section>
                                
                                    <div class="horizontal-group-end" style="margin-top:3rem;">
                                        <button type="button" class="btn btn-danger" data-dismiss="modal">Cerrar</button>
                                    </div>
                                    <div style="display:none;">
                                        <input id="txtIdEmpleado" type="text" class="form-control select-medidas" name="cedula">
                                    </div>

                                 </div>
                                
                             </div>
                            
                          </div>
                    </div>

                    <!-- Spinner -->
                    <div id="divSpinner" style="display:none;">
                      <div class="overlay"></div> <!-- Agregamos un elemento overlay -->
                      <div class="spinner-container">
                        <div class="spinner"></div>
                        <p class="loading-text">Cargando...</p>
                      </div>
                    </div>



            </section>
        </asp:Panel>
    </section>
</asp:Content>
