<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ListaEmpleados.aspx.cs"  Inherits="ReporteTareas.Formulario.ListaEmpleados" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/EmpleadosLista.js?v=5"></script>

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
    <div id="page-wrapper" style="padding: 0px">
        <div class="col-lg-12" style="padding: 0px">
            <div class="row">
                <div class="breadcrumbs ace-save-state" id="breadcrumbs">
                    <ul class="breadcrumb">
                        <li>
                            <i class="ace-icon fa fa-home home-icon"></i>
                            <a href="#">RRHH</a>
                        </li>
                        <li>
                            <a href="#">Lista de Empleados</a>
                        </li>
                    </ul>
                </div>
            </div>
            <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
                <div class="col-lg-12" style="padding: 0px">
                    <div class="panel panel-default">
                        <div>
                            <div style="display: none">
                                <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                <asp:TextBox ID="txtPerfil" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            </div>
                        </div>
                    </div>
                </div>

                                    <!-- ------------------------------------------------ -->
                                    <!-- Botones de crear nuevo empleado y descargar xls  -->
                                    <!-- ------------------------------------------------ -->

                <div class="col-lg-12" style="display: none" id="editBoton">
                    
                    <div class="text-right">
                        <button type="button" title="Nuevo empleado" class="btn btn-success btn-sm mr-4" onclick="nuevoRegistroEmpleados()">
                            <i class="fa fa-plus-circle margin-right"></i> Nuevo Empleado
                        </button>

                        <button type="button" title="Descargar empleados" class="btn btn-primary btn-sm" onclick="BtnDescarga()">
                            <i class="fa-regular fa fa-download"></i> Descargar
                        </button>
                    </div>

                </div>


                <div style="display: block; padding-left:10px" id="editMenu">
                    <div>
                        <div class="box box-primary">
                            <div class="box-header">
                                <h3 class="box-title">Lista de Empleados con carga familiar</h3>
                            </div>
                            <div class="box-body">
                                <table id="tbl_Empleados" style="border-color: #808080; font-size:80%;" class="table table-bordered table-hover table-striped text-center">
                                    <thead>
                                      <tr class="bg-primary">                                        
                                        <th style="width: 5%; text-align: center; vertical-align: inherit;">ID</th>
                                        <th style="width: 6%; text-align: center; vertical-align: inherit;">Cédula</th>
                                        <th style="width: 15%; text-align: center; vertical-align: inherit;">Nombre</th>
                                        <th style="width: 15%; text-align: center; vertical-align: inherit;">Sociedad</th>
                                        <th style="width: 15%; text-align: center; vertical-align: inherit;">Ciudad</th>
                                        <th style="width: 15%; text-align: center; vertical-align: inherit;">Area de Trabajo</th>
                                        <th style="width: 20%; text-align: center; vertical-align: inherit;">Dirección</th>
                                        <th style="width: 10%; text-align: center; vertical-align: inherit;">Teléfono</th>
                                        <th style="width: 8%; text-align: center; vertical-align: inherit;">Sexo</th>
                                        <th style="width: 15%; text-align: center; vertical-align: inherit;">Fecha de Nacimiento</th>
                                        <th style="width: 15%; text-align: center; vertical-align: inherit;">Provincia</th>
                                        <th style="width: 8%; text-align: center; vertical-align: inherit;">Carga Familiar</th>
                                        <th style="width: 5%; text-align: center; vertical-align: inherit;">Acciones</th>
                                        <th style="width: 5%; text-align: center; vertical-align: inherit;">Estado</th>
                                      </tr>
                                    </thead>

                                                                        
                                    <tbody>
                                        <!-- DATA POR MEDIO DE AJAX-->
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>

                <!----------------------------------------------------------------------->
                <!--     Carga de pagina editEmpleados / editar o actualizar empleado  -->
                <!----------------------------------------------------------------------->

                <div class="col-lg-12" style="display: none" id="editEmpleados">
                    <div class="form-horizontal">

                        <div class="col-lg-10">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Nombre Completo</label>
                                </div>
                                <div class="form-group">
                                    <input type="text" class="form-control" id="txtNombre">
                                </div>
                            </div>
                        </div>

                       <div class="col-lg-3">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Cedula empleado</label>
                                </div>
                                <div class="input-group m-bot15">
                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                    <input type="text" class="form-control" id="txtCedula" readonly>
                                </div>
                            </div>
                        </div>
                    

                         <div class="col-lg-3">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Sociedad</label>
                                </div>
                                <div class="input-group m-bot15">
                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                    <input type="text" class="form-control" id="txtSociedad">
                                </div>
                            </div>
                        </div>                        

                         <div class="col-lg-3">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Area de trabajo</label>
                                </div>
                                <div class="input-group m-bot15">
                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                    <input type="text" class="form-control" id="txtAreaTrabajo">
                                </div>
                            </div>
                        </div>

                         <div class="col-lg-3">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Provincia</label>
                                </div>
                                <div class="input-group m-bot15">
                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                    <input type="text" class="form-control" id="txtProvincia">
                                </div>
                            </div>
                        </div>

                         <div class="col-lg-3">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Ciudad</label>
                                </div>
                                <div class="input-group m-bot15">
                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                    <input type="text" class="form-control" id="txtCiudad">
                                </div>
                            </div>
                        </div>                                                 

                        <div class="col-lg-3">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Teléfono</label>
                                </div>
                                <div class="input-group m-bot15">
                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                    <input type="text" class="form-control" id="txtTelefono">
                                </div>
                            </div>
                        </div>

                        <div class="col-lg-3">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Sexo</label>
                                </div>
                                <div class="input-group m-bot15">
                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                    <input type="text" class="form-control" id="txtSexo">
                                </div>
                            </div>
                        </div>

                        <div class="col-lg-3">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Fecha de nacimiento</label>
                                </div>
                                <div class="input-group m-bot15">
                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                    <input type="text" class="form-control" id="txtFechaNacimiento">
                                </div>
                            </div>
                        </div>

                        <div class="col-lg-6">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Dirección</label>
                                </div>
                                <div class="input-group m-bot15">
                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                    <input type="text" class="form-control" id="txtDireccion">
                                </div>
                            </div>
                        </div>
                        

                        <div class="col-xs-12">
                            <br>
                            <div class="text-left">
                                <button type="button" title="lista producto" class="btn btn-info btn-sm" onclick="VerListaEmpleados()"> Lista empleados </button>
                                <button type="button" title="empleado" class="btn btn-success btn-sm" id="btnGEmpleado" onclick="GuardarEmpleado()">Guardar</button>
                            </div>
                        </div>
                    </div>
                </div>


                <!------------------------------------------------------------>
                <!--     Carga pagina editCargaFamiliar / lista de cargas   -->
                <!------------------------------------------------------------>

                <div class="col-lg-12" style="display: none" id="editCargaFamiliar">
                    <div class="form-horizontal">

                        <div class="box box-primary">

                            <div class="box-header">
                                <h3 class="box-title">Carga familiar de empleado</h3>
                            </div>

                            <div class="table-responsive">
                                <table id="tbl_CargaFam" class="table table-bordered table-hover text-center" style="border-color: #808080; font-size:85%;">
                                    <thead class="bg-primary">
                                      <tr>
                                        <th class="col-1">ID</th>
                                        <th class="col-2">Parentesco</th>
                                        <th class="col-4">Nombre</th>
                                        <th class="col-2">Fecha de Nacimiento</th>
                                        <th class="col-1">Acciones</th>
                                        <th class="col-2">Estado</th>
                                      </tr>
                                    </thead>
                                   
                                    <tbody>
                                        <!-- DATA POR MEDIO DE AJAX-->
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>

                
                <!------------------------------------------------------------------------------>
                <!--     Carga de pagina editBotonCargaFam  / botones de la lista de cargas   -->
                <!------------------------------------------------------------------------------>
                <div class="col-lg-12" style="display: none" id="editBotonCargaFam">
                    <div class="col-xs-12">
                        <div class="text-left">

                            <button type="button" title="Nueva carga" class="btn btn-success btn-sm" onclick="EditarCargaFamiliarEmpleado()">
                                <i class="fa fa-plus-circle margin-right"></i>  Nueva carga
                            </button>

                             <button type="button" title="Regresar" class="btn btn-danger btn-sm " onclick="VerListaEmpleados()">
                                <i class="fa fa-reply margin-right"></i> Regresar
                            </button>
                        </div>
                    </div>
                </div>

                <!---------------------------------------------------------------------------------->
                <!--     Carga de pagina editRegistroCargaFamiliar  / editar o actualizar carga   -->
                <!---------------------------------------------------------------------------------->

                <div class="col-lg-12" style="display: none" id="editRegistroCargaFamiliar">
                    <div class="form-horizontal">

                        <div class="col-lg-4">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Parentesco</label>
                                </div>
                                <div class="form-group">
                                    <input type="text" class="form-control" id="txtParentescoCF">
                                </div>
                            </div>
                        </div>

                        <div class="col-lg-4">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Nombre</label>
                                </div>
                                <div class="form-group">
                                    <input type="text" class="form-control" id="txtNombreCF">
                                </div>
                            </div>
                        </div>

                         <div class="col-lg-3">
                            <div class="col-md-12">
                                <div class="form-group">
                                    <label>Fecha de nacimiento</label>
                                </div>
                                <div class="input-group m-bot15">
                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                    <input type="text" class="form-control" id="txtFechaNacimientoCF">
                                </div>
                            </div>
                        </div>                           
                        
                        <div class="col-xs-12">
                            
                            <div class="text-left">                                
                                <button type="button" title="carga" class="btn btn-success btn-sm" id="btnGCargaFam" onclick="GuardarCargaFam()">Guardar</button>
                                
                                <button type="button" title="Regresar" class="btn btn-danger btn-sm " onclick="VerCargaFamiliarEmpleado2()">
                                    <i class="fa fa-reply margin-right"></i> Regresar
                                </button>
                            </div>                           

                        </div>
                    </div>
                </div>


            </asp:Panel>
        </div>
    </div>
</asp:Content>


