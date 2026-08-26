<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="HistorialClinico.aspx.cs" Inherits="ReporteTareas.Formulario.HistorialClinico" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <!-- Estilos de Select2 -->
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <!-- Estilos de SweetAlert2 (desde CDN, se recomienda usar solo esta fuente) -->
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <!-- Estilo personalizado del proyecto -->
    <link href="../dist/css/depMedico.css" rel="stylesheet" />
    <!-- Estilo de animación de sweetalert -->
    <link href="../bower_components/sweetalert/css/animate.css" rel="stylesheet" />

    <!-- === Scripts === -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>

    <!-- SweetAlert2  -->    
    <script src="../bower_components/sweetalert/js/sweetalert2.all.min.js"></script> 
    <link href="../bower_components/sweetalert/css/sweetalert2.min.css" rel="stylesheet" /> 

    <!-- Tu script principal -->
    <script src="../js/HistorialClinico.js?v=7"></script>

<!-- El script de la librería PDF-->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jspdf-autotable/3.5.29/jspdf.plugin.autotable.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js"></script>

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
                            <button class="btn btn-default" type="button" id="btnBuscarDatosPersonales" ><i class="glyphicon glyphicon-search"></i></button>
                        </div>
                    </div>

                    <div class="col-sm-12" id="idHistorialClinico" style="background-color: #9DA8AD; padding: 1rem 1rem 1rem 1rem;">                        
                                      
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
                                             <div class="horizontal-group">
                                                <div class="input-group col-sm-12">
                                                    <span class="input-group-addon select-dorado" style="width: 25%;"><i class="glyphicon glyphicon-user"></i>  Nombre:</span>
                                                    <input id="txtNombre" type="text" class="form-control" name="nombre" placeholder="Nombre" oninput="convertirAMayusculas(this)" readonly>
                                                </div>
                                             </div>
                                             <div class="horizontal" style="margin-top: 1rem;">
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon select-temas" style="width: 35%; text-align:left;"><i class="glyphicon glyphicon-credit-card"></i>  Cédula:</span>
                                                    <input id="txtCedula" type="text" class="form-control" name="cedula" placeholder="Cedula" readonly>
                                                </div>
                                             </div>
                                             <div class="horizontal" style="margin-top: 1rem;">  
                                                 <div class="input-group col-sm-6">
                                                     <span class="input-group-addon select-temas" style="width: 35%; text-align:left;"><i class="glyphicon glyphicon-leaf"></i>  Estado civil:</span>
                                                     <input id="txtEstadoCivil" type="text" class="form-control" name="estadocivil" placeholder="Estado civil" readonly>
                                                 </div>
                                                 <div class="input-group col-sm-4" style="margin-left: 6rem;">
                                                     <span class="input-group-addon select-temas" style="width: 25px;"><i class="glyphicon glyphicon-ok-circle"></i>  Sexo:</span>                                                    
                                                     <input type="text" class="form-control" style="" id="txtSexo" name="txtSexo" readonly>
                                                 </div>
                                             </div>
                                             <div class="horizontal" style="margin-top: 1rem;">    
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon input-basic3 select-temas" ><i class="glyphicon glyphicon-gift"></i>  Fecha de Nacimiento:</span>
                                                    <input type="text" class="form-control" style="" id="fechaNac" name="fechaNac" readonly>
                                                </div>
                                                <div class="input-group col-sm-4" style="margin-left: 6rem;">
                                                    <span class="input-group-addon input-basic select-temas"><i class="glyphicon glyphicon-star"></i>  Edad:</span>
                                                    <input id="txtEdad" type="number" class="form-control" name="edad" placeholder="0" readonly>
                                                </div>
                                            </div>

                                             <div class="horizontal" style="margin-top: 1rem;">                                                
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon select-temas" style="width: 35%; text-align:left;"><i class="glyphicon glyphicon-home"></i>  Sociedad:</span>
                                                    <input id="txtSociedad" type="text" class="form-control" name="sociedad" placeholder="Sociedad" readonly>
                                                </div>    
                                                <div class="input-group col-sm-5" style="margin-left: 6rem;">
                                                    <span class="input-group-addon input-medium select-temas"><i class="glyphicon glyphicon-link"></i>  Area de trabajo:</span>
                                                    <input id="txtAreaTrabajo" type="text" class="form-control" name="areatrabajo" placeholder="Area de trabajo" readonly>
                                                </div>
                                            </div>
                                             <div class="horizontal" style="margin-top: 1rem;">
                                                <div class="input-group col-sm-8">
                                                    <span class="input-group-addon input-medium select-temas" style="text-align:left;"><i class="glyphicon glyphicon-lock"></i>  Puesto de trabajo:</span>
                                                    <input id="txtPuestoTrabajo" type="text" class="form-control" name="puestotrabajo" placeholder="Puesto de trabajo" readonly>
                                                </div>                                                
                                            </div>
                                        </div>

                                        <div class="col-sm-3" style="text-align: center;">
                                            <div id="imagenDiv" class="imagen-div"></div>
                                        </div>

                                    </formview>
                            </div>   
                            
                            <div class="horizontal-group" style="margin: 3rem auto 0 auto; justify-content:center;">
                                <div class="row col-sm-10" style="margin: 0 auto; justify-content:center;">
                                    <div class="well" style="display:flex; flex-direction:column; align-items:center; background-color: #211d5b0f;padding: 0;">
                                        <div class="input-group horizontal-group-simple" style="margin-top:1rem;margin-bottom:1rem;">
                                             <label for="disabledTextInput" class="control-label">Pertenece al grupo de personas vulnerables? </label>
                                             <div class="btn-group" role="group" style="margin-left:3rem;">
                                                 <button type="button" id="siVulnerable" class="btn btn-custom" onclick="toggleButtonColor('siVulnerable'); mostrarOcultarVulnerables('si')">Si</button>
                                                 <button type="button" id="noVulnerable" class="btn btn-custom" onclick="toggleButtonColor('noVulnerable'); mostrarOcultarVulnerables('no')">No</button>
                                             </div>                                            
                                        </div>     
                                        <div class="col-sm-12 text-center horizontal-cuadriculado-flow" id="opsVulnerables" style="display:none; background-color:#5d7c8326; padding:1rem;">
                                                <div class="input-group horizontal-group" style="margin-top:0;">
                                                <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                    <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#bdb5ccd6;" >
                                                        <div class="checkbox" style="margin:0;">
                                                            <label>
                                                                <input type="checkbox" id="cboxEmbarazo" name="vulnerable" value="">
                                                                <span class="">Embarazo<span class="check"></span></span>
                                                            </label>
                                                        </div>                                                                                                                                
                                                    </div>                    
                                                </div>
                                                <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                    <%--<label class="col-sm-6 control-label">Mareo:</label>--%>
                                                    <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#bdb5ccd6;" >
                                                        <div class="checkbox" style="margin:0;">
                                                            <label>
                                                                <input type="checkbox" id="cboxLactancia" name="vulnerable" value="">
                                                                <span class="">Período de lactancia<span class="check"></span></span>
                                                            </label>
                                                        </div>                                                                                                                                
                                                    </div>                    
                                                </div>
                                                <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                    <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                    <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#bdb5ccd6;" >
                                                        <div class="checkbox" style="margin:0;">
                                                            <label>
                                                                <input type="checkbox" id="cboxEnfCatastrofica" name="vulnerable" value="">
                                                                <span class="">Enfermedad catastrófica<span class="check"></span></span>
                                                            </label>
                                                        </div>                                                                                                                                
                                                    </div>                    
                                                </div>
                                                <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                    <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                    <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#bdb5ccd6;" >
                                                        <div class="checkbox" style="margin:0;">
                                                            <label>
                                                                <input type="checkbox" id="cboxDiscapacidad" name="vulnerable" value="">
                                                                <span class="">Discapacidad<span class="check"></span></span>
                                                            </label>
                                                        </div>                                                                                                                              
                                                    </div>                    
                                                </div>
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="margin-top:0.5rem; background-color:#bdb5ccd6;">
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>                                                    
                                                            <input type="checkbox" id="cboxOtroVulnerable" name="vulnerable" value="">
                                                            <span class="checkbox-material">Otro<span class="check"></span></span>
                                                        </label>
                                                    </div>
                                                    <div class="col-sm-7 horizontal-group" style="margin:0 2rem 0 0;">
                                                        <span style="margin-right:1rem;">Especifique:</span>
                                                        <input type="text" id="idtxtOtroVulnerable" class="inputs-control-mod" name="" value="" style="width:80%;">
                                                    </div>                                                                                                                                  
                                                </div>
                                         </div>
                                    </div>
                                </div>
                            </div>

                        </div>    

                        <!-- Cuadro de Seguimiento-->
                        <div class="well" id="seguimiento" style="display:none; margin-bottom: 5px; margin-top: 0; padding-top: 1rem;">

                            <div class="well-header" style="margin-top: 1rem;">
                                <div style="margin-left:2rem;">
                                    <label for="disabledTextInput" class="col-form-label" style="">Segimiento de atención médica</label>
                                </div>
                            </div>
                            <div class="row" id="" style=" margin: 0 auto 0 auto; justify-content:center;">
                                <!-- Cuadro de Seguimiento de Atenciones " -->
                                <div class="horizontal-group-special col-sm-12" style="margin-top: 0;">
                                     <div class="col-sm-11" style="justify-content:center; ">  
                                          <!-- Contenedor donde se crearán las tarjetas dinámicamente -->
                                          <div id="contenedorTarjetas"></div>                                        
                                     </div>
                                </div> 
                            </div>
                            
                        </div>

                        <!-- Cuadro de Motivo de consulta-->
                        <div class="well" style="margin-bottom: 5px; margin-top: 0; padding-top: 1rem;">

                            <div class="well-header" style="margin-top: 1rem;">
                                <div style="margin-left:2rem;">
                                    <label for="disabledTextInput" class="col-form-label" style="">MOTIVO DE CONSULTA / SINTOMAS</label>
                                </div>
                            </div>

                            <div class="row" id="opsMotivos" style=" margin: 2rem auto 0 auto; justify-content:center;">

                                    <div class="horizontal-group-evenly col-sm-10" style="margin: 0 3rem 2rem 3rem;">
                                        <div class="col-sm-5 grupo-input text-center" style="margin-left:1rem;">
                                             <label for="disabledTextInput" class="control-label">Fecha de Atención: </label>
                                             <input type="date" class="form-control input-azulmedio" id="txtfechaAtencion" value="">
                                        </div>
                                        <div class="col-sm-3 grupo-input text-center">
                                             <label for="disabledTextInput"style="width:30%;" class="control-label">Hora: </label>
                                             <input id="txtHoraAtencion" type="time" class="form-control" name="" placeholder="09:00" >
                                        </div>
                                    </div>     
                                                                       
                                    <div class="horizontal-group-special col-sm-12">
                                        <div class="well col-sm-10" style="background-color: #d8e4e9; padding:1rem; justify-content:center; margin: 0 auto 1rem auto;">
                                            <div class="input-group horizontal-group" style="margin-top:0;">
                                                <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;" >
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>
                                                            <input type="checkbox" id="cboxDolorCabeza" name="motivos" value="si">
                                                            <span class="">Dolor de cabeza<span class="check"></span></span>
                                                        </label>
                                                    </div>                                                                                                                                
                                                </div>                    
                                            </div>
                                            <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                <%--<label class="col-sm-6 control-label">Mareo:</label>--%>
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;" >
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>
                                                            <input type="checkbox" id="cboxMareo" name="motivos" value="si">
                                                            <span class="">Mareo<span class="check"></span></span>
                                                        </label>
                                                    </div>                                                                                                                                
                                                </div>                    
                                            </div>                                        
                                            <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;">
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>                                                    
                                                            <input type="checkbox" id="cboxDolorMuscular" name="motivos" value="si">  
                                                            <span class="">Dolor muscular<span class="check"></span></span>
                                                        </label>
                                                    </div>
                                                    <div class="col-sm-7 horizontal-group" style="margin:0 2rem 0 0;">
                                                        <span style="margin-right:1rem;">Donde?</span>
                                                        <input type="text" id="idtxtDolorMuscular" class="inputs-control-mod" name="" value="" style="width:80%;" disabled>
                                                    </div>                                                                                                                                  
                                                </div>
                                            </div>
                                            <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;" >
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>
                                                            <input type="checkbox" id="cboxDolorLumbar" name="motivos" value="si">
                                                            <span class="">Dolor lumbar / espalda<span class="check"></span></span>
                                                        </label>
                                                    </div>                                                                                                                                
                                                </div>                    
                                            </div>
                                            <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;" >
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>
                                                            <input type="checkbox" id="cboxCansancio" name="motivos" value="si">
                                                            <span class="">Cansancio / fatiga<span class="check"></span></span>
                                                        </label>
                                                    </div>                                                                                                                                
                                                </div>                    
                                            </div>
                                            <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;" >
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>
                                                            <input type="checkbox" id="cboxDolorExtSup" name="motivos" value="si">
                                                            <span class="">Dolor en extremidades superiores<span class="check"></span></span>
                                                        </label>
                                                    </div>                                                                                                                                
                                                </div>                    
                                            </div>
                                            <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;" >
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>
                                                            <input type="checkbox" id="cboxDolorExtInf" name="motivos" value="si">
                                                            <span class="">Dolor en extremidades inferiores<span class="check"></span></span>
                                                        </label>
                                                    </div>                                                                                                                                
                                                </div>                    
                                            </div>
                                            <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;" >
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>
                                                            <input type="checkbox" id="cboxNauseas" name="motivos" value="si">
                                                            <span class="">Náuseas / malestar estomacal<span class="check"></span></span>
                                                        </label>
                                                    </div>                                                                                                                                
                                                </div>                    
                                            </div>
                                            <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;" >
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>
                                                            <input type="checkbox" id="cboxIrritacion" name="motivos" value="si">
                                                            <span class="">Irritación ocular<span class="check"></span></span>
                                                        </label>
                                                    </div>                                                                                                                                
                                                </div>                    
                                            </div>
                                            <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;" >
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>
                                                            <input type="checkbox" id="cboxTos" name="motivos" value="si">
                                                            <span class="">Tos / congestión / dificultad respiratoria<span class="check"></span></span>
                                                        </label>
                                                    </div>                                                                                                                                
                                                </div>                    
                                            </div>
                                            <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                                <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#2b416033;">
                                                    <div class="checkbox" style="margin:0;">
                                                        <label>                                                    
                                                            <input type="checkbox" id="cboxOtroMotivo" name="motivos" value="si">  
                                                            <span class="">Otro<span class="check"></span></span>
                                                        </label>
                                                    </div>
                                                    <div class="col-sm-7 horizontal-group" style="margin:0 1rem 0 0;">
                                                        <span style="margin-right:1rem;">Especifique:</span>
                                                        <input type="text" id="idtxtOtroMotivo" class="inputs-control-mod" name="" value="" style="width:80%;">
                                                    </div>       
                                                </div>                    
                                            </div>
                                        </div>
                                   </div>
                            </div>
                        </div>

                        <!-- Cuadro de Signos vitales -->
                        <div class="well" style="margin-bottom: 5px; margin-top: 1rem; padding-top: 1rem;">

                            <div class="well-header" style="margin-top: 1rem;">
                                <div style="margin-left:2rem;">
                                    <label for="disabledTextInput" class="col-form-label" style="">SIGNOS VITALES</label>
                                </div>
                            </div>

                            <div class="contenedor-horizontal">
                                <div class="horizontal-group-special" style="margin-top:1rem;">
                                    <div class="horizontal col-sm-10" style="margin-top:0;">
                                        <div class="" style="width:25%;">
                                                <span class="input-group-addon select-temas" style="text-align:left;"><i class="glyphicon glyphicon-tint" style="margin-right: 0.5rem;"></i>  Presión arterial</span>
                                                <input id="idtxtSignosPresion" type="number" class="form-control" placeholder="mmHg">
                                        </div>
                                        <div class="" style="width:25%;">
                                                <span class="input-group-addon select-temas" style="text-align:left;"><i class="glyphicon glyphicon-heart-empty" style="margin-right: 0.5rem;"></i>  Frecuencia cardíaca</span>
                                                <input id="idtxtSignosFrecuencia" type="text" class="form-control" placeholder="lpm">
                                        </div>
                                        <div class="" style="width:25%;">
                                                <span class="input-group-addon select-temas" style="text-align:left;"><i class="glyphicon glyphicon-erase" style="margin-right: 0.5rem;"></i>  Temperatura</span>
                                                <input id="idtxtSignosTemperatura" type="text" class="form-control" placeholder="°C">
                                        </div>
                                        <div class="" style="width:25%;">
                                                <span class="input-group-addon select-temas" style="text-align:left;"><i class="glyphicon glyphicon-dashboard" style="margin-right: 0.5rem;"></i>  Saturación</span>
                                                <input id="idtxtSignosSaturacion" type="text" class="form-control" placeholder="%">
                                        </div>  
                                    </div>                                                                                      
                                </div>
                                <div class="horizontal-group-special" style="margin-top:1rem;">
                                    <div class="col-sm-10">
                                        <span class="input-group-addon select-temas" style="text-align:center;"><i class="glyphicon glyphicon-align-left" style="margin-right: 1rem;"></i>  Observaciones:</span>
                                        <textarea id="idtxtSignosObsSignos" rows="3" class="form-control" placeholder="Ingrese las observaciones de signos vitales"></textarea>
                                    </div>
                                </div>
                            </div>   
                        </div>

                        <!-- Cuadro de Antecedentes relevantes-->
                        <div class="well" style="margin-bottom: 5px; margin-top: 0; padding-top: 1rem;">

                            <div class="well-header" style="margin-top: 1rem;">
                                <div style="margin-left:2rem;">
                                    <label for="disabledTextInput" class="col-form-label" style="">ANTECEDENTES RELEVANTES</label>
                                </div>
                            </div>

                            <div class="horizontal-group" style="margin: 0 auto; justify-content:center;">
                                <div class="col-sm-10" id="opsAntRelevantes" style="margin: 0 auto; justify-content:center;">

                                    <div class="well" style="background-color: #d8e4e9; padding: 10px; margin-bottom: 1rem;">
                                    
                                        <div class="input-group horizontal-group" style="margin-top:0;">
                                            <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                            <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#43797f5c;" >
                                                <div class="checkbox" style="margin:0;">
                                                    <label>
                                                        <input type="checkbox" id="cboxNoAntecedentes" name="antecedente" value="si">
                                                        <span class="">No refiere antecedentes<span class="check"></span></span>
                                                    </label>
                                                </div>                                                                                                                                
                                            </div>                    
                                        </div>
                                        <div class="input-group horizontal-group" style="margin-top:0.5rem;"> 
                                            <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#43797f5c;">
                                                <div class="checkbox" style="margin:0;">
                                                    <label>                                                    
                                                        <input type="checkbox" id="cboxAntPerRelevantes" name="antecedente" value="si">  
                                                        <span class="">Antecedentes personales relevantes:<span class="check"></span></span>
                                                    </label>
                                                </div>
                                                <div class="col-sm-7 mb-3" style="margin:0 2rem 0 0;">
                                                    <input type="text" id="idtxtAntPerRelevantes" class="inputs-control-mod" name="" value="" style="width:100%;" disabled>
                                                </div>                                                                                                                                  
                                            </div>                    
                                        </div>
                                        <div class="input-group horizontal-group" style="margin-top:0.5rem;"> 
                                            <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#43797f5c;">
                                                <div class="checkbox" style="margin:0;">
                                                    <label>                                                    
                                                        <input type="checkbox" id="cboxMedActual" name="antecedente" value="si">  
                                                        <span class="">Medicación actual:<span class="check"></span></span>
                                                    </label>
                                                </div>
                                                <div class="col-sm-7 mb-3" style="margin:0 2rem 0 0;">
                                                     <input type="text" id="idtxtMedActual" class="inputs-control-mod" name="" value="" style="width:100%;" disabled>
                                                </div>                                                                                                                                  
                                            </div>                    
                                        </div>
                                        <div class="input-group horizontal-group" style="margin-top:0.5rem;"> 
                                            <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#43797f5c;">
                                                <div class="checkbox" style="margin:0;">
                                                    <label>                                                    
                                                        <input type="checkbox" id="cboxAlergias" name="antecedente" value="si">  
                                                        <span class="">Alergias:<span class="check"></span></span>
                                                    </label>
                                                </div>
                                                <div class="col-sm-7 horizontal-group mb-3" style="margin:0 2rem 0 0;">
                                                     <span class="form-label" style="margin-right:1rem;">Especifique:</span>
                                                     <input type="text" id="idtxtAlergias" class="inputs-control-mod" name="" value="" style="width:80%;" disabled>
                                                </div>                                                                                                                                  
                                            </div>                    
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Cuadro de Intervencion y recomendaciones-->
                        <div class="well" style="margin-bottom: 5px; margin-top: 0; padding-top: 1rem;">

                            <div class="well-header" style="margin-top: 1rem;">
                                <div style="margin-left:2rem;">
                                    <label for="disabledTextInput" class="col-form-label" style="">INTERVENCIÓN Y RECOMENDACIONES</label>
                                </div>
                            </div>

                            <div class="horizontal-group" style="margin: 0 auto; justify-content:center;">
                                <div class="col-sm-10" id="opsIntervencion" style="margin: 0 auto; justify-content:center;">

                                    <div class="well" style="background-color: #b8bdcf7a;padding: 10px;">
                                    
                                        <div class="input-group horizontal-group" style="margin-top:0;">
                                            <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                            <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#906e8b69;" >
                                                <div class="checkbox" style="margin:0;">
                                                    <label>
                                                        <input type="checkbox" id="cboxIntReposo" name="intervencion" value="">
                                                        <span class="">Reposo breve / pausa activa<span class="check"></span></span>
                                                    </label>
                                                </div>                                                                                                                                
                                            </div>                    
                                        </div>
                                        <div class="input-group horizontal-group" style="margin-top:0.5rem;"> 
                                            <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#906e8b69;">
                                                <div class="checkbox" style="margin:0;">
                                                    <label>                                                    
                                                        <input type="checkbox" id="cboxIntMed" name="intervencion" value="">  
                                                        <span class="">Medicación entregada:<span class="check"></span></span>
                                                    </label>
                                                </div>
                                                <div class="col-sm-7 mb-3" style="margin:0 2rem 0 0;">
                                                    <input type="text" id="idtxtMedEntregada" class="inputs-control-mod" name="" value="" style="width:100%;">
                                                </div>                                                                                                                                  
                                            </div>                    
                                        </div>
                                        <div class="input-group horizontal-group" style="margin-top:0.5rem;"> 
                                            <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#906e8b69;">
                                                <div class="checkbox" style="margin:0;">
                                                    <label>                                                    
                                                        <input type="checkbox" id="cboxIntDerivado" name="intervencion" value="">  
                                                        <span class="">Derivado a:<span class="check"></span></span>
                                                    </label>
                                                </div>
                                                <div class="col-sm-7 mb-3" style="margin:0 2rem 0 0;">
                                                    <input type="text" id="idtxtDerivado" class="inputs-control-mod" name="" value="" style="width:100%;" disabled>
                                                </div>                                                                                                                                 
                                            </div>                    
                                        </div>
                                        <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                            <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                            <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#906e8b69;" >
                                                <div class="checkbox" style="margin:0;">
                                                    <label>
                                                        <input type="checkbox" id="cboxIntAtencionMed" name="intervencion" value="">
                                                        <span class="">Se recomienda atención médica externa<span class="check"></span></span>
                                                    </label>
                                                </div>                                                                                                                                
                                            </div>                    
                                        </div>
                                        <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                            <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                            <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#906e8b69;" >
                                                <div class="checkbox" style="margin:0;">
                                                    <label>
                                                        <input type="checkbox" id="cboxIntRegresa" name="intervencion" value="">
                                                        <span class="">Regresa a su puesto de trabajo<span class="check"></span></span>
                                                    </label>
                                                </div>                                                                                                                                
                                            </div>                    
                                        </div>
                                        <div class="input-group horizontal-group" style="margin-top:0.5rem;">
                                            <%--<label class="col-sm-6 control-label">Dolor de cabeza:</label>--%>
                                            <div class="col-sm-12 horizontal-group-simple opcCheckboxMargen" style="background-color:#906e8b69;" >
                                                <div class="checkbox" style="margin:0;">
                                                    <label>
                                                        <input type="checkbox" id="cboxIntRequiereSeg" name="intervencion" value="si">
                                                        <span class="">Requiere seguimiento<span class="check"></span></span>
                                                    </label>
                                                </div>                                                                                                                                
                                            </div>                    
                                        </div>                                        

                                    </div>

                                    <div class="" style="margin-left:2rem;">
                                          <span class="">RECOMENDACIONES</span>
                                    </div>
                                    <div class="form-group" style="margin: 0 auto;">
                                          <textarea id="txtRecomendacionG" rows="3" style="width: 100%; resize: vertical; padding:1rem;" value="" placeholder="De acuerdo a la valoración médica efectuada colocar las recomendaciones y/o tratamiento farmacológico y no farmacológico."></textarea>
                                    </div>  

                                    <div class="" style="margin-left:2rem; margin-top: 1rem;">
                                         <span class="well-title">OBSERVACIONES</span>
                                    </div>
                                    <div class="form-group" style="margin: 0 auto;">
                                         <textarea id="txtObservacionG" rows="3" style="width: 100%; resize: vertical; padding:1rem;" value="" placeholder="De acuerdo a la valoración médica efectuada colocar las recomendaciones y/o tratamiento farmacológico y no farmacológico."></textarea>
                                    </div>  

                                </div>
                            </div>
                        </div>

                        <!--    CUADRO PARA CARGAR LOS ARCHIVOS    -->
                        <div class="well contenedor-horizontal" style=" margin:0 0 0 0; padding-top:2rem; padding-bottom:2rem;">                                            
                            
                            <div class="well-header" style="margin-top: 0;">
                                <div style="margin-left:2rem;">
                                    <label for="disabledTextInput" class="col-form-label" style="">CARGAR ARCHIVOS</label>
                                </div>
                            </div>

                             <div class="containerFiles">
                                 <div id="dropZone" class="drop-zone" onclick="document.getElementById('fileInput').click()">
                                      <span class="glyphicon glyphicon-plus icon" style="margin-right:1rem; font-size: 3rem;"></span>
                                            Arrastra los archivos aquí o haz clic para seleccionarlos
                                 </div>
                             </div>

                             <input type="file" id="fileInput" multiple style="display: none;">
                             <div id="fileList" >
                                  <!-- Lista de archivos cargados -->
                             </div>                                        
                        </div>

                    </div>

                    <div class="horizontal col-sm-12" id="idBtnsFinales" style="padding: 1rem 1rem 0.2rem 1rem;">
                        <div class="col-sm-4" style="display:flex; align-items:center;">
                            <span style="color:white; margin-right:1rem;">Seguimiento?</span>
                            <label class="switch">                                
                                <input type="checkbox" id="chkSeguimiento">
                                <span class="slider"></span>
                            </label>                            
                        </div>
                        <div class="col-sm-5" id="btnCarga" style="display:block; justify-content:center;">
                            <%--<button class="btn btn-info col-sm-10" type="button" id="btnCargarHsCln" onclick="GuardarHistoria()" style="margin: 3rem;">Cargar Datos</button>--%>
                            <button class="btn btn-info col-sm-10" type="button" id="btnGenerarPDF" onclick="prepararModalPDF()" style="margin: 3rem;">Generar atención médica</button>
                        </div>        
                
                        <!-- Boton flotante de regreso-->
                        <div>
                            <a id="btnRegresar" class="chat-button"></a>
                        </div>
                        
                    </div>     



                    <!-- Modal de Visualizacion de PDF -->
                    <div id="ModalEgresoInventarioPDF" class="ModalPDF modal fade" role="dialog">
                        <div class="modal-dialog modal-lg"> <!-- Agrega la clase 'modal-lg' para que el modal sea grande -->
                            <div class="modal-content">
                                <div class="modal-header bg-info">
                                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                                    <h4 class="modal-title">Vista previa</h4>
                                </div>
                                <div class="modal-body ">
                                    <div class="container" style="margin-top:0; margin-bottom: 1rem; padding:0;">

                                        <div class="row-flex" style="margin: 0 0 1rem 0;"><!-- Cuadros 1-->            
                            
                                            <div class="div1-mitad" style="max-width:50%; padding: 0; margin: 0 0 0 0;">

                                                <div class="image-container" style=" display: grid; justify-items: center; align-items: center; padding: 0; ">
                                                    <img src="../carrusel/imagenes/logo_dos_textoGris.png" style="width: 30%;" alt="logo DOS">
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row-flex" style="margin: 0 0 1rem 0;">               
                                            <div class="columna div1" style="padding-left: 0.2rem; padding-bottom:0.5rem;">                                                
                                                <div class="centered-element letra-bold" style="margin-bottom:1rem;">                                                  
                                                    <span class="">FICHA DE ATENCIÓN MÉDICA OCUPACIONAL</span>                                                
                                                </div>                                                
                                                <div class="centered-element">                                                    
                                                    <span id="">Area de Salud Ocupacional - Uso confidencial</span>                                                
                                                </div>  
                                           </div>                                               
                                        </div>
                                
                                        <div class="row-flex" id="informacionPDF" style="margin: 0 0 1rem 0;"> <!-- Cuadros 3-->
                                            <div class="columna div1" style="padding-left: 0.2rem; padding-bottom:0.5rem;">
                                                <div class="centered-element">
                                                    <span class="letra-bold">1. DATOS DEL COLABORADOR</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold" style="">Nombre completo:</span><span style="margin-left:1rem;" id="txtPDFNombre">MEJIA MONTENEGRO MARIA AUGUSTA</span>
                                                </div>     
                                            
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold" style="">Cedula/ID:</span><span style="margin-left:1rem;" id="txtPDFCedula">-- -- --</span>
                                                </div>   

                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold" style="">Sexo:</span><span style="margin-left:1rem;" id="txtPDFSexo">-- -- --</span>
                                                </div>   

                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold" style="">Área o puesto de trabajo:</span><span style="margin-left:1rem;" id="txtPDFAreaTrabajo">-- -- --</span>
                                                </div>   

                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold" style="">Fecha de atención:</span><span style="margin-left:1rem;" id="txtPDFfechaAtencion">-- -- --</span>
                                                </div>   

                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold"style="">Hora:</span><span style="margin-left:1rem;" id="txtPDFtxtHoraAtencion">-- -- --</span>
                                                </div>   

                                                <div class="doble-columna" style="padding:1rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold">Pertenece al grupo de personas vulnerables?</span>
                                                    <div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                        <input type="checkbox" id="PDFsiVulnerable" name="vulnerable" value="si" style="margin-left:4rem;"><span class="letra-bold" style="margin-left:0.5rem;">Si</span>
                                                        <input type="checkbox" id="PDFnoVulnerable" name="vulnerable" value="no" style="margin-left:2rem;"><span class="letra-bold" style="margin-left:0.5rem;">No</span>
                                                    </div>
                                                </div>                                                 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFEmbarazo" name="vulnerable" value=""><span class="letra-bold" style="margin-left:0.5rem;">Embarazo</span>
                                                </div>    
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFLactancia" name="vulnerable" value=""><span class="letra-bold" style="margin-left:0.5rem;">Período de lactancia</span>
                                                </div>                                                 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFEnfCatastrofica" name="vulnerable" value=""><span class="letra-bold" style="margin-left:0.5rem;">Enfermedad catastrófica</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFDiscapacidad" name="vulnerable" value=""><span class="letra-bold" style="margin-left:0.5rem;">Discapacidad</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0.5rem 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFOtroVulnerable" name="vulnerable" value=""><span class="letra-bold" style="margin-left:0.5rem;">Otro Especifique:</span><span style="margin-left:0.5rem;" id="idtxtPDFOtroVulnerable"></span>
                                                </div> 

                                            
                                                <hr class="linea">
                                                <div class="centered-element" style="margin-top:1rem;">
                                                    <span class="letra-bold">2. MOTIVO DE CONSULTA / SINTOMAS</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                    <span style="">Describa lo que siente el colaborador:</span>
                                                </div>
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFDolorCabeza" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Dolor de cabeza</span>
                                                </div>    
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFMareo" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Mareo</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFDolorMuscular" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Dolor muscular (¿Donde?</span><span style="margin-left:0.5rem;" id="idtxtPDFDolorMuscular"></span><span style="margin-left:0.5rem;">)</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFDolorLumbar" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Dolor lumbar / espalda</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFCansancio" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Cansancio / fatiga</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFDolorExtSup" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Dolor en extremidades superiores</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFDolorExtInf" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Dolor en extremidades inferiores</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFNauseas" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Náuseas / malestar estomacal</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFIrritacion" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Irritación ocular</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFTos" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Tos / congestión / dificultad respiratoria</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0.5rem 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFOtroMotivo" name="motivos" value="si"><span class="letra-bold" style="margin-left:0.5rem;">Otro (especifique):</span><span style="margin-left:1rem;" id="idtxtPDFOtroMotivo"></span>
                                                </div> 


                                                <hr class="linea">
                                                <div class="centered-element" style="margin-top:1rem;">
                                                    <span class="letra-bold">3. SIGNOS VITALES</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold" style="margin-right:1rem;">Presión arterial:</span><span style="margin-right:1rem;" id="idtxtPDFSignosPresion">___</span><span>mmHg</span>
                                                </div>
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold" style="margin-right:1rem;">Frecuencia cardíaca:</span><span style="margin-right:1rem;" id="idtxtPDFSignosFrecuencia">___</span><span>lpm</span>
                                                </div>
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold" style="margin-right:1rem;">Temperatura:</span><span style="margin-right:1rem;" id="idtxtPDFSignosTemperatura">___</span><span>°C</span>
                                                </div>
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <span class="letra-bold" style="margin-right:1rem;">Saturación O2:</span><span style="margin-right:1rem;" id="idtxtPDFSignosSaturacion">___</span><span>%</span>
                                                </div>
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0.5rem 0.5rem;">
                                                    <span class="letra-bold" style="margin-right:1rem; width:30%;">Observaciones:</span><span style="margin-right:1rem;" id="idtxtPDFSignosObsSignos">___</span>
                                                </div>                                                                                       
                                                                                       
                                                                 
                                                 <hr class="linea">
                                                <div class="centered-element" style="margin-top:1rem;">
                                                    <span class="letra-bold">4. ANTECEDENTES RELEVANTES</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFNoAntecedentes" name="motivos"><span class="letra-bold" style="margin-left:0.5rem;">No refiere antecedentes</span>
                                                </div>    
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFAntPerRelevantes" name="motivos"><span class="letra-bold" style="margin-left:0.5rem;">Antecedentes personales relevantes:</span><span style="margin-left:1rem;" id="idtxtPDFAntPerRelevantes">___</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFMedActual" name="motivos"><span class="letra-bold" style="margin-left:0.5rem;">Medicación actual:</span><span style="margin-left:1rem;" id="idtxtPDFMedActual">___</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0.5rem 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFAlergias" name="motivos"><span class="letra-bold" style="margin-left:0.5rem;">Alergias: - Especifique:</span><span style="margin-left:1rem;" id="idtxtPDFAlergias">___</span>
                                                </div> 


                                                <hr class="linea">
                                                <div class="centered-element" style="margin-top:1rem;">
                                                    <span class="letra-bold">5. INTERVENCIÓN Y RECOMENDACIONES</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFIntReposo" name="motivos"><span class="letra-bold" style="margin-left:0.5rem;">Reposo breve / pausa activa</span>
                                                </div>    
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFIntMed" name="motivos"><span class="letra-bold" style="margin-left:0.5rem;">Medicación entregada:</span><span style="margin-left:1rem;" id="idtxtPDFMedEntregada">___</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFIntDerivado" name="motivos"><span class="letra-bold" style="margin-left:0.5rem;">Derivado a:</span><span style="margin-left:1rem;" id="idtxtPDFDerivado">___</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFIntAtencionMed" name="motivos"><span class="letra-bold" style="margin-left:0.5rem;">Se recomienda atención médica externa</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFIntRegresa" name="motivos"><span class="letra-bold" style="margin-left:0.5rem;">Regresa a su puesto de trabajo</span>
                                                </div>  
                                                <div class="doble-columna" style="padding:0.5rem 0.5rem 0.5rem 0.5rem;">
                                                    <input type="checkbox" id="cboxPDFIntRequiereSeg" name="motivos"><span class="letra-bold" style="margin-left:0.5rem;">Requiere seguimiento</span>
                                                </div>  

                                                <hr class="linea">
                                                <div class="centered-element" style="margin-top:1rem;">
                                                    <span class="letra-bold">6. RECOMENDACIONES</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:1rem 2rem 0.5rem 2rem;">
                                                    <span style="text-align: center;" id="txtPDFRecomendacionG">---</span>
                                                </div> 

                                                <hr class="linea">
                                                <div class="centered-element" style="margin-top:1rem;">
                                                    <span class="letra-bold" >7. OBSERVACIONES</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:1rem 2rem 0.5rem 2rem;">
                                                    <span style="text-align: center;" id="txtPDFObservacionG">---</span>
                                                </div> 

                                                <hr class="linea">
                                                <div class="" style="margin-top:3rem;">
                                                    <span class="letra-bold" >FIRMAS</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:3rem 0.5rem 0 0.5rem;">
                                                   <span class="letra-bold" style="margin-left:0.5rem;">Firma del colaborador: ____________________________</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:4rem 0.5rem 0 0.5rem;">
                                                   <span class="letra-bold" style="margin-left:0.5rem;">Nombre y firma del médico ocupacional: __________________________________________________________________</span>
                                                </div> 
                                                <div class="doble-columna" style="padding:5rem 0.5rem 0 0.5rem;">
                                                   <span class="letra-bold" style="margin-left:0.5rem;">Sello profesional:</span>
                                                </div> 
                                            </div>
                                        </div>


                                    </div>            
                                </div>            
                
                                <div class="container" style="margin: 1rem auto 0 33%">
                                    <%--<button class="btn btn-info col-sm-3" type="button"onclick="generatePDF(), descargarModalComoPDF('ModalEgresoInventarioPDF')">Descargar</button>--%>
                                    <button class="btn btn-info col-sm-3" type="button"onclick="descargarModalComoPDF('ModalEgresoInventarioPDF')">Guardar y Descargar</button>
                                </div>
                                <div class="modal-footer" style="margin-top:2rem;">                
                                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                                </div>
                                <!-- /.modal-content -->
                            </div>
                        <!-- /.modal-dialog -->    
                       </div>
                    </div>


                    <!-- Modal de advertencia -->
                    <div id="confirmModal" class="modal fade" role="dialog">
                        <div class="modal-dialog">
                            <!-- Modal content-->
                            <div class="modal-content">
                                <div class="modal-header bg-primary">
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
