<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="HistoriaClinica.aspx.cs" Inherits="ReporteTareas.Formulario.HistoriaClinica" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Asegúrate de cargar primero jQuery -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    
    <!-- Carga los otros scripts que dependen de jQuery -->
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>

    <script src="../js/HistoriaClinica.js?v=4"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    
    <!-- Estilos -->
    <link href="../dist/css/depMedico.css" rel="stylesheet" />
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    
    <!-- SweetAlert scripts -->
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content;">
        <div class="col-lg-12" style="background-color: #0D2538;padding-bottom: 1.5rem;padding-top: 0.5rem;">

            <div class="row" style="background-color: #0D2538; margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs">

                    <ul class="breadcrumb">
                        <li>
                            <a href="#"><b>Historia Clínica</b></a>
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

                <div id="myCarousel" class="carousel slide" data-ride="carousel">
                    <!-- Target para los slide lde las imagenes  -->
                    <ol class="carousel-indicators">
                        <li data-target="#myCarousel" data-slide-to="0" class="active"></li>
                        <li data-target="#myCarousel" data-slide-to="1"></li>
                        <li data-target="#myCarousel" data-slide-to="2"></li>
                        <li data-target="#myCarousel" data-slide-to="3"></li>
                        <li data-target="#myCarousel" data-slide-to="4"></li>

                    </ol>

                    <!-- Imagenes del carrusel  -->
                    <div class="carousel-inner">
                        <div class="item active">
                            <img src="../carrusel/imagenes/slogan2.png">
                            <div class="carousel-caption">
                            </div>
                        </div>

                        <div class="item ">
                            <img src="../carrusel/imagenes/medicina6.jpg">
                            <div class="carousel-caption">
                            </div>
                        </div>

                        <div class="item ">
                            <img src="../carrusel/imagenes/medicina1.jpg">
                            <div class="carousel-caption">
                            </div>
                        </div>

                        <div class="item ">
                            <img src="../carrusel/imagenes/medicina5.jpg">
                            <div class="carousel-caption">
                            </div>
                        </div>

                        <div class="item ">
                            <img src="../carrusel/imagenes/medicina4.jpg">
                            <div class="carousel-caption">
                            </div>
                        </div>

                    </div>

                    <!-- flechas para controlar el carrusel izquierda derecha -->
                    <a class="left carousel-control" href="#myCarousel" data-slide="prev">
                        <span class="glyphicon glyphicon-chevron-left"></span>
                        <span class="sr-only">Previous</span>
                    </a>
                    <a class="right carousel-control" href="#myCarousel" data-slide="next">
                        <span class="glyphicon glyphicon-chevron-right"></span>
                        <span class="sr-only">Next</span>
                    </a>
                </div>

                <div class="input-group col-sm-4" style="margin: 0 auto; padding-top: 1rem; padding-bottom: 1rem;">
                    <input type="text" id="txtEmpleado" class="form-control" placeholder="Buscar paciente" oninput="BuscarEmpleado2()" value="">
                    <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboEmpleados2"></ul>
                    <div class="input-group-btn">
                        <button class="btn btn-default" type="button" id="btnBuscarDatosPersonales" onclick="BuscarEmpleado()"><i class="glyphicon glyphicon-search"></i></button>
                    </div>
                </div>

                <div class="" style="background-color: #9DA8AD; padding: 1rem 1rem 0.2rem 1rem;">
              
                                      
                        <!--  Cuadro Datos personales  -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
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


                        <div class="tabs" id="pestanias" style="display:none; background-color: #0D2538; padding-top: 1rem;">
                            <ul class="nav nav-tabs">
                                <li class="nav-item">
                                    <a class="nav-link tab-seleccionada"" id="pestania1" style="background-color: #A90505; color: #EDEDED; cursor: pointer; font-weight: bold;" 
                                        onmouseover="cambiarColorHover('pestania1')" onmouseout="restaurarColor('pestania1')" onclick="seleccionarPestania('pestania1')">NUEVO FORMULARIO</a>
                                </li>
                                <li class="nav-item">
                                    <a class="nav-link" id="pestania2" style="background-color: #A90505; color: #EDEDED; cursor: pointer; font-weight: bold;" 
                                        onmouseover="cambiarColorHover('pestania2')" onmouseout="restaurarColor('pestania2')" onclick="seleccionarPestania('pestania2')">BUSCAR</a>
                                </li>
                                <li class="nav-item">
                                    <a class="nav-link" id="pestania3" style="background-color: #A90505; color: #EDEDED; cursor: pointer; font-weight: bold;"
                                        onmouseover="cambiarColorHover('pestania3')" onmouseout="restaurarColor('pestania3')" onclick="seleccionarPestania('pestania3')">HISTORIAL</a>
                                </li>                                
                            </ul>
                        </div>


                        <!-- Cuadro de seleccion de Nuevo formularios -->
                        <div class="well" id="cuadroFormularios" style="display:none; margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                           
                            <div class="well horizontal-group-evenly TipoFormulario box" id="btnEvaluacionPreocupacional" style="background-color:#B3C6E8; margin-top: 1rem;">
                                <div class="icon-box" style="width:10rem;">
                                   <i class="icon-seleccion fa fa-file-text "></i>                               
                                </div>
                                
                                <div class="cuadros-ingreso-normal7">
                                    <h3>Evaluación Preocupacional</h3>
                                    <p>El formulario de evaluación preocupacional – inicio, será llenado por el personal médico y se aplicará al usuario en las siguientes instancias:</p>
                                    <ul>
                                        <li>Al postularse para un puesto de trabajo</li>
                                        <li>Al inicio de las actividades laborales en una empresa, institución pública o privada</li>
                                    </ul>
                                </div>
                            </div>

                            <div class="well horizontal-group-evenly TipoFormulario box" id="btnEvaluacionPeriodica" style="background-color:#D1BBED">
                                <div class="icon-box" style="width:10rem;">
                                   <i class="icon-seleccion fa fa-stethoscope  "></i>                               
                                </div>
                                
                                <div class="cuadros-ingreso-normal7">
                                    <h3>Evaluación Periódica</h3>
                                    <p>El formulario de evaluación periódica, se aplicará al usuario anualmente a partir del segundo año de
haber ingresado a la empresa o institución, instancia pública y privada.</p>
                                    
                                </div>
                            </div>
                            
                            <div class="well horizontal-group-evenly TipoFormulario box" id="btnEvaluacionReintegro" style="background-color:#B9E3AE">
                                
                                  <div class="icon-box" style="width:10rem;">
                                     <i class="icon-seleccion fa fa-medkit "></i>                               
                                 </div>
                                
                                 <div class="cuadros-ingreso-normal7">
                                     <h3>Evaluación de Reintegro</h3>
                                     <p>El formulario de reintegro se debe aplicar de manera obligatoria cuando el usuario se reincorpore a su
    actividad laboral (oficio, labor u ocupación que desempeña el individuo) en caso de ausencia
    prologada mayor o igual a quince días (15) por motivos de salud, periodo de maternidad o
    incapacidad laboral.</p>                                    
                                 </div>
                                
                            </div>

                            <div class="well horizontal-group-evenly TipoFormulario box" id="btnEvaluacionRetiro" style="background-color:#B3C6E8; margin-top: 1rem;">
                                <div class="icon-box" style="width:10rem;">
                                   <i class="icon-seleccion fa fa-sign-out "></i>                               
                                </div>
                                
                                <div class="cuadros-ingreso-normal7">
                                    <h3>Evaluación de Retiro</h3>
                                    <p>Este formulario se debe utilizar cuando el usuario se desvincule de la empresa o institución pública o
privada dentro de los 5 días posteriores a su salida.</p>
                                </div>
                            </div>

                            <div class="well horizontal-group-evenly TipoFormulario box" id="btnRegInmunizaciones" style="background-color:#D1BBED; margin-top: 1rem;">
                                <div class="icon-box" style="width:10rem;">
                                   <i class="icon-seleccion fa fa-book "></i>                               
                                </div>
                                
                                <div class="cuadros-ingreso-normal7">
                                    <h3>Registro de Inmunizaciones</h3>
                                    <p>Se debe registrar en el esquema de inmunizaciones, las vacunas que el usuario ha recibido en el lugar de trabajo.</p>
                                </div>
                            </div>

                            <div class="well horizontal-group-evenly TipoFormulario box" id="btnCertificado" style="background-color:#95cccb; margin-top: 1rem;">
                                <div class="icon-box" style="width:10rem;"> 
                                    <i class="icon-seleccion glyphicon glyphicon-leaf"></i>                               
                                </div>
                                
                                <div class="cuadros-ingreso-normal7">
                                    <h3>Certificado de salud en el trabajo</h3>
                                    <p>Una vez ejecutadas las evaluaciones médicas preocupacional – inicio, periódica, reintegro, y retiro
como resultado de todo el proceso, se emitirá el <strong>certificado</strong> en dos (2) copias originales, una de las
cuales será archivada como parte de la historia clínica ocupacional y la otra entregada al usuario para
los fines pertinentes.</p>
                                </div>
                            </div>
                            
                        </div>



                        <!-- Cuadro de busqueda para formularios -->
                        <div class="well" id="cuadroListaForms" style="display:none; margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <!-- Ingreso de busqueda -->
                            <div class="well" style="background-color:#d8e4e9; margin-top: 0.5rem;">
                                <div class="well-title" style="margin-top: 0;">
                                    <h4>Buscar</h4>
                                </div>
                                
                                <div class="horizontal-group-special">
                                  <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                         <fieldset class="grupo-input text-center">
                                              <label for="disabledTextInput">TIPO DE EVALUACIÓN</label>
                                              <select id="txtTipoEvaluacionSelect" class="form-control">
                                                     <option value="" selected>- Seleccionar -</option>
                                                     <option value="PREOCUPACIONAL">Evaluación Preocupacional</option>
                                                     <option value="PERIODICA">Evaluación Periodica</option>     
                                                     <option value="REINTEGRO">Evaluación de Reintegro</option> 
                                                     <option value="Todas">Todas</option>
                                               </select>
                                         </fieldset>
                                   </div>
                                   <div class="horizontal-group-evenly cuadros-ingreso-normal5">
                                         <div class="" style=" width:40%; margin-bottom: 10px;">
                                             <div class="grupo-input text-center">
                                                 <label for="disabledTextInput">Fecha desde:</label>
                                                 <input type="date" class="form-control" id="fechaEvaluacionDesde" value="">
                                             </div>
                                         </div>
                                         <div class="" style="width:40%; margin-bottom: 10px;">
                                              <div class="grupo-input text-center">
                                                  <label for="disabledTextInput">Fecha hasta:</label>
                                                  <input type="date" class="form-control" id="fechaEvaluacionHasta" value="">
                                              </div>
                                         </div>
                                    </div>  
                                    <div class="grupo-input text-center" style="display:flex; align-items:end; margin-bottom:1rem;">
                                        <button type="button" title="Buscar formulario" class="btn btn-info btn-VerListaForms" onclick="VerFormularios()"><i class="fa fa-search"></i>   Buscar</button>
                                    </div>
                                </div>     
                            </div>                          
                            
                            <!-- Tabla de busqueda -->
                            <div>                            
                                <div class="row" style="margin: 0 0.5rem 0 0.5rem; justify-content: space-between">
                        
                                    <div style="display: block; padding-left:10px" id="editMenu">
                                        <div>
                                            <div class="box box-primary">
                                                <div class="box-header">
                                                    <h3 class="box-title">Lista de formularios</h3>
                                                </div>
                                                <div class="box-body">
                                                    <table id="tbl_Formularios" style="border-color: #808080; font-size:80%;" class="table table-bordered table-hover table-striped text-center">
                                                        <thead>
                                                          <tr class="bg-primary">                                        
                                                            <th style="width: 25%; text-align: center; vertical-align: inherit;">Nombre</th>
                                                            <th style="width: 15%; text-align: center; vertical-align: inherit;">Sociedad</th>
                                                            <th style="width: 12%; text-align: center; vertical-align: inherit;">Area de Trabajo</th>
                                                            <th style="width: 8%; text-align: center; vertical-align: inherit;">Tipo</th>
                                                            <th style="width: 10%; text-align: center; vertical-align: inherit;">Fecha</th>
                                                            <th style="width: 5%; text-align: center; vertical-align: inherit;">Acciones</th>
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

                                </div>
                            </div>                          

                        </div> <!-- Fin del well de Busqueda -->     
                             
                        
                        <!--------------------------------------------->
                        <!--        Modal Contacto de emergencia     -->
                        <!--------------------------------------------->

                        <div id="msgContactoEmergencia" class="modal fade" role="dialog">
                            <div class="modal-dialog">
                                <!-- Modal content-->
                                <div class="modal-content">
                                    <div class="modal-header bg-info">
                                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                                        <h4 class="modal-title">CONTACTO DE EMERGENCIA</h4>
                                    </div>
                                    <div class="modal-body">
                                        <div class="horizontal-group-special">
                                            <div class="user-icon">
                                                <i class="glyphicon glyphicon-plus-sign"></i>
                                            </div>
                                        </div>     
                                        <div class="horizontal-group-start" style="margin-top:3rem;">
                                            <p style="margin-right: 1rem; justify-content:center;">Datos del contacto de emergencia de:</p>
                                            <label id="txtNombreUsuario"></label>                                    
                                        </div>  
                                        <section class="contenedor-horizontal">
                                            <div class="horizontal">
                                                <div class="input-group col-sm-12">
                                                    <span class="input-group-addon" style="width: 30%; text-align:left;"><i class="glyphicon glyphicon-user"></i>  Nombre:</span>
                                                    <input id="txtNombreContacto" type="text" class="form-control select-medidas" name="nombre" placeholder="Nombre de Contacto" oninput="convertirAMayusculas(this)">
                                                </div>
                                            </div>
                                            <div class="horizontal">
                                                <div class="input-group col-sm-12">
                                                    <span class="input-group-addon" style="width: 30%; text-align:left;"><i class="glyphicon glyphicon-earphone"></i>  Teléfono:</span>
                                                    <input id="txtTelefonoContacto" type="text" class="form-control select-medidas" name="nombre" placeholder="Teléfono de Contacto" oninput="convertirAMayusculas(this)">
                                                </div>
                                            </div>
                                            <div class="horizontal">
                                                <div class="input-group col-sm-12">
                                                    <span class="input-group-addon" style="width: 30%; text-align:left;"><i class="glyphicon glyphicon-link"></i>  Parentesco:</span>
                                                    <input id="txtParentescoContacto" type="text" class="form-control select-medidas" name="nombre" placeholder="Parentesco" oninput="convertirAMayusculas(this)">
                                                </div>
                                            </div>
                                        </section>
                                
                                        <div class="horizontal-group-end" style="margin-top:3rem;">
                                            <button type="button" style="margin-right:1rem;" id="btnGContacto" onclick="GuardarContacto()" class="btn btn-info">Actualizar</button>
                                            <button type="button" class="btn btn-danger" data-dismiss="modal">Cerrar</button>
                                        </div>
                                        <div style="display:none;">
                                            <input id="txtIdEmpleado" type="text" class="form-control select-medidas" name="cedula">
                                        </div>

                                     </div>
                                
                                 </div>
                            
                              </div>
                        </div>
                        
                </div>
            </asp:Panel>
        </div>
    </div>
    
    
</asp:Content>
