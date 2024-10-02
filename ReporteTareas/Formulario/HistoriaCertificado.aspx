<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="HistoriaCertificado.aspx.cs" Inherits="ReporteTareas.Formulario.HistoriaCertificado" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <!-- Cargar jQuery primero -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <!-- Scripts que dependen de jQuery -->
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    
    <!-- Script específico de la página -->
    <script src="../js/HistoriaCertificado.js?v=4"></script>

    <!-- Archivos CSS -->
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <link href="../dist/css/depMedico.css" rel="stylesheet" />

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content; ">
        <div class="col-lg-12" style="background-color: #0D2538;padding-bottom: 1.5rem;padding-top: 0.5rem;">

            <div class="row" style="background-color: #0D2538; margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs">

                    <ul class="breadcrumb">
                        <li style="display:flex; justify-content:space-between;">
                            <a href="#"><b>Certificado de salud en el trabajo</b></a>
                            <a href="#"><i class="glyphicon glyphicon-log-out"></i></a>
                        </li>
                    </ul>                    
                </div>
            </div>


            <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
                <div class="panel-default">
                    <div>
                        <div style="display: none">
                            <asp:TextBox ID="hiddenCedulaField" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtPerfil" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                        </div>
                    </div>
                </div>


                

                <!-- ******************************************************************** -->
                <!--                            PAGINA                                    -->
                <!-- ******************************************************************** -->
                                
                <div class="" style="background-color: #9DA8AD;">

                    <div class="input-group col-sm-4" style="display:none; margin: 0 auto; padding-top: 1rem; padding-bottom: 1rem;">
                        <input type="text" id="txtEmpleado" class="form-control" placeholder="Buscar paciente">                    
                        <div class="input-group-btn">
                            <button class="btn btn-default" type="button" id="btnBuscarDatosPersonales" oninput="BuscarCodigosCIE()"><i class="glyphicon glyphicon-search"></i></button>
                        </div>
                    </div>

                    <div class="col-sm-12" style="background-color: #9DA8AD;padding: 1rem 1rem 1rem 1rem;">                        
                                      
                        <!--  Cuadro Datos personales  -->
                        <div class="well" style="margin-bottom: 5px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">Datos Personales</h4>
                                </div>
                            </div>
                            <div class="row" style="margin: 0 0.5rem 1rem 0.5rem; justify-content: space-between">
                                    <formview id="frmDatos grid" class="form-horizontal" action="/action_page.php">
                                        <div class="horizontal-simple col-sm-9">
                                            <!-- Campos Nombre y Cedula -->
                                             <div class="horizontal-group">
                                                <div class="input-group col-sm-12">
                                                    <span class="input-group-addon select-dorado" style="width: 25%;"><i class="glyphicon glyphicon-user"></i>  Nombre:</span>
                                                    <input id="txtNombre" type="text" class="form-control" name="nombre" placeholder="Nombre" oninput="convertirAMayusculas(this)" readonly>
                                                </div>
                                            </div>
                                            <div class="horizontal">
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon" style="width: 35%;"><i class="glyphicon glyphicon-credit-card"></i>  Cédula:</span>
                                                    <input id="txtCedula" type="text" class="form-control" name="cedula" placeholder="Cedula" readonly>
                                                </div>
                                            </div>
                                            <div class="horizontal">  
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon" style="width: 35%;"><i class="glyphicon glyphicon-leaf"></i>  Estado civil:</span>
                                                    <input id="txtEstadoCivil" type="text" class="form-control" name="estadocivil" placeholder="Estado civil" readonly>
                                                </div>
                                                <div class="input-group col-sm-4" style="margin-left: 6rem;">
                                                    <span class="input-group-addon" style="width: 25px;"><i class="glyphicon glyphicon-ok-circle"></i>  Sexo:</span>                                                    
                                                    <input type="text" class="form-control" style="" id="txtSexo" name="txtSexo" readonly>
                                                </div>
                                            </div>
                                            <div class="horizontal">    
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon input-basic3" ><i class="glyphicon glyphicon-gift"></i>  Fecha de Nacimiento:</span>
                                                    <input type="text" class="form-control" style="" id="fechaNac" name="fechaNac" readonly>
                                                </div>
                                                <div class="input-group col-sm-4" style="margin-left: 6rem;">
                                                    <span class="input-group-addon input-basic"><i class="glyphicon glyphicon-star"></i>  Edad:</span>
                                                    <input id="txtEdad" type="number" class="form-control" name="edad" placeholder="0" readonly>
                                                </div>
                                            </div>

                                            <div class="horizontal">                                                
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon" style="width: 35%;"><i class="glyphicon glyphicon-home"></i>  Sociedad:</span>
                                                    <input id="txtSociedad" type="text" class="form-control" name="sociedad" placeholder="Sociedad" readonly>
                                                </div>    
                                                <div class="input-group col-sm-5" style="margin-left: 6rem;">
                                                    <span class="input-group-addon input-medium"><i class="glyphicon glyphicon-link"></i>  Area de trabajo:</span>
                                                    <input id="txtAreaTrabajo" type="text" class="form-control" name="areatrabajo" placeholder="Area de trabajo" readonly>
                                                </div>
                                            </div>
                                            <div class="horizontal-group">
                                                <div class="input-group col-sm-8">
                                                    <span class="input-group-addon input-medium"><i class="glyphicon glyphicon-lock"></i>  Puesto de trabajo:</span>
                                                    <input id="txtPuestoTrabajo" type="text" class="form-control" name="puestotrabajo" placeholder="Puesto de trabajo" readonly>
                                                </div>                                                
                                            </div>
                                            <!-- ... otros campos ... -->
                                        </div>

                                        <div class="col-sm-3" style="text-align: center;">
                                            <div id="imagenDiv" class="imagen-div"></div>
                                            <!--img id="imgEmpleado" src="../carrusel/imagenes/usuarios.png" alt="Imagen de Empleado" height="200" width="200"-->
                                            <button type="button" class="btn btn-danger" onclick="MensajeContactoEmergencia()" style="margin-top: 1rem;"><i class="glyphicon glyphicon-user"></i>  Contacto de emergencia</button>
                                        </div>

                                    </formview>
                                </div>                                                    

                        </div>    

                        <!-- Cuadro de Resultados " -->
                        <div class="well" style="margin-bottom: 5px; margin-top: 0; padding-top: 1rem; padding-bottom:2rem;">
                     
                            <div class="well-header" style="margin-top: 2rem;">
                                <div style="margin-left:2rem;">
                                    <h4 class="well-title">DATOS GENERALES</h4>
                                </div>
                            </div>

                            <div class="horizontal-group-evenly" style="margin: 2rem 3rem 3rem 3rem;">
                                <div class="col-sm-4 grupo-input text-center" style="margin-left:1rem;">
                                     <label for="disabledTextInput" class="control-label">Fecha de Emisión: </label>
                                     <input type="date" class="form-control input-azulmedio" id="fechaEmision" value="">
                                </div>
                                <div class="col-sm-6 grupo-input text-center">
                                     <label for="disabledTextInput"style="width:30%;" class="control-label">Evaluación: </label>
                                     <select id="txtEvaluacion" class="form-control input-azulmedio">
                                          <option value="" disabled selected>- Seleccionar -</option>
                                          <option value="Ingreso">Ingreso</option>
                                          <option value="Periódico">Periódico</option>
                                         <option value="Reintegro">Reintegro</option>
                                         <option value="Retiro">Retiro</option>
                                     </select>
                                </div>
                            </div>    
                            
                            <hr style="width:95%;"/>

                            <div class="well-header" style="margin-top: 3rem;">
                                <div style="margin-left:2rem;">
                                    <h4 class="well-title">APTITUD MÉDICA PARA EL TRABAJO</h4>
                                </div>
                            </div>

                            <div class="horizontal-group-evenly" style="margin: 2rem 3rem 3rem 3rem;">

                                <div class="col-sm-4 grupo-input text-center" style="margin-left:1rem;">
                                    <fieldset class="grupo-input text-center">
                                        <label for="disabledTextInput">Aptitud:</label>
                                            <select id="txtAptitudSelect" class="form-control input-azulmedio">
                                                <option value="" selected>- Seleccionar -</option>
                                                <option value="apto">APTO </option>
                                                <option value="aptoObservacion">APTO EN OBSERVACIÓN</option>      
                                                <option value="aptoLimitacion">APTO CON LIMITACIONES </option>   
                                                <option value="noApto">NO APTO</option>   
                                            </select>
                                     </fieldset>
                                </div>
                                <div class="col-sm-6 grupo-input text-center" id="obsAptitud">
                                    <div class="grupo-input text-center" >
                                        <label for="disabledTextInput">Observación</label>
                                        <input id="txtDescAptitud" type="text" class="form-control select-temas" placeholder="Ingrese una observacion de ser necesario." value="">
                                    </div>
                                </div> 
                                <div class="col-sm-6 grupo-input text-cente" id="descObservacion">
                                    <div class="grupo-input text-center" >
                                        <label for="disabledTextInput">Observación</label>
                                        <input id="txtDescObservacion" type="text" class="form-control select-principal" placeholder="Detalle de la observación" value="">
                                    </div>
                                </div>     
                                <div class="col-sm-6 grupo-input text-cente" id="descLimitacion">
                                    <div class="grupo-input text-center" >
                                        <label for="disabledTextInput">Limitación</label>
                                        <input id="txtDescLimitacion" type="text" class="form-control select-principal" placeholder="Detalle de la limitación" value="">
                                    </div>
                                </div>  
                           </div>

                        </div>


                        <!-- Cuadro Información salud -->
                        <div class="well" style="margin-bottom: 5px; margin-top: 0; padding-top: 1rem;">

                            <div class="well-header" style="margin-top: 0;">
                                <div style="margin-left:2rem;">
                                    <h4 class="well-title">EVALUACIÓN MÉDICA DE RETIRO</h4>
                                </div>
                            </div>

                            <div class="contenedor-horizontal">

                                <div class="well" style="background-color: #d8e4e9;padding-top: 0;">
                                    
                                    <div class="input-group horizontal-group">
                                        <label class="col-sm-6 control-label">El usuario se realizó la evaluación médica de retiro:</label>
                                        <div class="col-sm-6 horizontal-group-simple" style="background-color:#9DA8AD; padding:0.3rem 3rem 0.3rem 2rem;" >
                                            <div class="checkbox" style="margin:0;">
                                                <label></label>
                                                </div>
                                            <div class="checkbox" style="margin:0 0 0 2rem;">
                                                <label>
                                                    <input type="checkbox" id="evalRetiroSi" name="evalRetiro" value="si">
                                                    <span class="checkbox-material"><span class="check"></span></span>
                                                    Sí
                                                </label>
                                            </div>
                                            <div class="checkbox" style="margin:0 4rem 0 0;">
                                                <label>
                                                    <input type="checkbox" id="evalRetiroNo"  name="evalRetiro" value="no">
                                                    <span class="checkbox-material"><span class="check"></span></span>
                                                    No
                                                </label>
                                            </div>                                                                                                                                  
                                        </div>                    
                                    </div>

                                    <div class="input-group horizontal-group">
                                      <label class="col-sm-5 control-label">Condición del diagnóstico:</label>
                                      <div class="col-sm-6 horizontal-group-simple" style="background-color:#9DA8AD; padding:0.5rem 3rem 0.5rem 2rem;">
                                        <div class="checkbox" style="margin:0;">
                                          <label>
                                            <input type="checkbox" id="diagPresuntiva" name="diag" value="presuntiva">
                                            <span class="checkbox-material"><span class="check"></span></span>
                                            Presuntiva
                                          </label>
                                        </div>
                                        <div class="checkbox" style="margin:0;">
                                          <label>
                                            <input type="checkbox" id="diagDefinitiva" name="diag" value="definitiva">
                                            <span class="checkbox-material"><span class="check"></span></span>
                                            Definitiva
                                          </label>
                                        </div>
                                        <div class="checkbox" style="margin:0;">
                                          <label>
                                            <input type="checkbox" id="diagNoAplica" name="diag" value="noAplica">
                                            <span class="checkbox-material"><span class="check"></span></span>
                                            No aplica
                                          </label>
                                        </div>
                                      </div>
                                    </div>

                                    <div class="input-group horizontal-group" style="">
                                      <label class="col-sm-6 control-label">La condición de salud está relacionada con el trabajo:</label>
                                      <div class="col-sm-6 horizontal-group-simple" style="background-color:#9DA8AD; padding:0.5rem 3rem 0.5rem 2rem;">
                                        <div class="checkbox" style="margin:0;">
                                          <label>
                                            <input type="checkbox" id="relacionTrabajoSi" name="relacionTrabajo" value="si">
                                            <span class="checkbox-material"><span class="check"></span></span>
                                            Sí
                                          </label>
                                        </div>
                                        <div class="checkbox" style="margin:0 0 0 1rem;">
                                          <label>
                                            <input type="checkbox" id="relacionTrabajoNo" name="relacionTrabajo" value="no">
                                            <span class="checkbox-material"><span class="check"></span></span>
                                            No
                                          </label>
                                        </div>
                                        <div class="checkbox" style="margin:0;">
                                          <label>
                                            <input type="checkbox" id="relacionTrabajoNoAplica" name="relacionTrabajo" value="noAplica">
                                            <span class="checkbox-material"><span class="check"></span></span>
                                            No aplica
                                          </label>
                                        </div>
                                      </div>
                                    </div>

                                </div>
                            </div>
                        </div>

                        <div class="well" style=" margin-top: 0; margin-bottom:0; padding-top: 1rem;">
                             <div class="well-header" style="margin-left:2rem;">
                                  <h4 class="well-title">RECOMENDACIONES</h4>
                             </div>
                             <div class="form-group" style="margin: 0 2rem;">
                                 <textarea id="txtRecomendacion" rows="4" style="width: 100%; resize: vertical; padding:1rem;" value="" placeholder="De acuerdo a la valoración médica efectuada colocar las recomendaciones y/o tratamiento farmacológico y no farmacológico."></textarea>
                             </div>  
                        </div>


                    </div>
                    <div class="col-sm-12" style="padding: 1rem 1rem 0.2rem 1rem;">
                        <div class="" id="btnCarga" style="display:block; justify-content:center;">
                            <button class="btn btn-info col-sm-6" type="button" id="btnCargarDatosPersonales" onclick="GuardarCertificado()" style="width: 25%; margin: 3rem;">Cargar Datos</button>
                        </div>        
                
                        <!-- Boton flotante de regreso-->
                        <a id="btnRegresar" class="chat-button"></a>
                    </div>
                    

                    <!-- Modal de advertencia -->
                    <div id="confirmModal" class="modal fade" role="dialog">
                        <div class="modal-dialog">
                            <!-- Modal content-->
                            <div class="modal-content">
                                <div class="modal-header bg-info">
                                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                                    <h4 class="modal-title">Advertencia</h4>
                                </div>
                                <div class="modal-body">
                                    ¿Realmente desea salir del formulario?
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                                    <button type="button" class="btn btn-primary" id="confirmBtn">Aceptar</button>
                                </div>
                            </div>
                        </div>
                    </div>

              </div>    

            </asp:Panel>

        </div>

    </div>
    
</asp:Content>
