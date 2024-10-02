<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="HistoriaRetiro.aspx.cs" Inherits="ReporteTareas.Formulario.HistoriaRetiro" %>

<asp:Content ID="HistoriaRetiroContent" ContentPlaceHolderID="head" runat="server">
    <!-- Estilos -->
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <link href="../dist/css/depMedico.css" rel="stylesheet" />

    <!-- Scripts -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../js/HistoriaRetiro.js?v=1"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

     HistoriaRetiro.aspx.cs
    
    <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content;">
        <div class="col-lg-12" style="background-color: #0D2538;padding-bottom: 1.5rem;padding-top: 0.5rem;">

            <div class="row" style="background-color: #0D2538; margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs">

                    <ul class="breadcrumb">
                        <li>
                            <a href="#"><b>Formulario Historia Clínica Periódica</b></a>
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

                <div class="input-group col-sm-4" style="display:none; margin: 0 auto; padding-top: 1rem; padding-bottom: 1rem;">
                    <input type="text" id="txtEmpleado" class="form-control" placeholder="Buscar paciente">
                    <div class="input-group-btn">
                        <button class="btn btn-default" type="button" id="btnBuscarDatosPersonales" onclick="BuscarEmpleado()"><i class="glyphicon glyphicon-search"></i></button>
                    </div>
                </div>

                <!-- Contenido de "Evaluación de Reintegro" -->
                <div style="padding: 1rem 1rem 0.2rem 1rem; background-color: #9DA8AD;">                        

                     <!--  Cuadro Datos personales -->
                     <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                          <div class="well-header" style="margin-top: 0;">
                               <div>
                                   <h4 class="well-title">DATOS PERSONALES</h4>
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
                                        </div>

                                    </formview>
                                </div>       
                        </div>


                        <!-- Cuadro de Actividades " -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">DATOS ESTABLECIMIENTO - EMPRESA</h4>
                                </div>
                            </div>
                            <div class="horizontal-group-simple" style="margin: 2rem 2rem 0 2rem;">
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <label for="disabledTextInput">ACTIVIDADES</label>
                                        <input id="txtActividad1" type="text" class="form-control" placeholder="Actividad" value="">
                                    </div>
                                </div> 
                                <div class="cuadros-ingreso-normal7" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <label for="disabledTextInput">FACTORES DE RIESGO</label>
                                        <input id="txtFactorRiesgo1" type="text" class="form-control select-medidas" placeholder="" value="">
                                    </div>
                                </div>                                 
                            </div>                            
                            <div class="horizontal-group-simple" style="margin: 0 2rem;">
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <input id="txtActividad2" type="text" class="form-control" placeholder="Actividad" value="">
                                    </div>
                                </div> 
                                <div class="cuadros-ingreso-normal7" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <input id="txtFactorRiesgo2" type="text" class="form-control select-medidas" placeholder="" value="">
                                    </div>
                                </div>                                 
                            </div>
                            <div class="horizontal-group-simple" style="margin: 0 2rem;">
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <input id="txtActividad3" type="text" class="form-control" placeholder="Actividad" value="">
                                    </div>
                                </div> 
                                <div class="cuadros-ingreso-normal7" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <input id="txtFactorRiesgo3" type="text" class="form-control select-medidas" placeholder="" value="">
                                    </div>
                                </div>                                 
                            </div>

                        </div>

                        <!-- Cuadro de Antecedentes  " -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">ANTECEDENTES PERSONALES</h4>
                                </div>
                            </div>

                            <div class="horizontal-group-special contenedor-horizontal" style="margin:3rem 2rem 4rem 2rem;">
                                <div class="col-sm-12">
                                    <label class="col-form-label" style="margin-left:1rem;">ANTECEDENTES CLÍNICOS Y QUIRÚRGICOS</label>
                                    <textarea id="txtAntecedentesPersonales" rows="4" style="width: 100%; resize: vertical;" placeholder="Registrar la información proporcionada por el usuario en la 
anamnesis, en lo referente a antecedentes clínicos registrar enfermedades, alergias, traumatismos; en 
antecedentes quirúrgicos, detallar las principales intervenciones quirúrgicas a las que se ha sometido."></textarea>
                                </div>
                            </div>                            
                            <div class="contenedor-horizontal" style="margin: 0 3rem 4rem 3rem;">   
                                <label class="col-form-label" style="margin-left:1.5rem;">ACCIDENTES DE TRABAJO</label>
                                <div class="horizontal-group-simple">
                                    <div class="input-group col-sm-2">
                                        <span class="input-group-addon" style="width: 25%;">Fecha:</span>
                                        <input type="date" class="form-control" id="fechaInst1" name="fechaInst1">
                                    </div>
                                    <div class="col-sm-1 text-center input-group">
                                        <select id="AccidentesTrabajoSelect" class="form-control" onchange="handleAccidentesTrabajoSelect()">
                                             <option value="" selected hidden>Si/No</option>
                                             <option value="si">SI</option>
                                             <option value="no">NO</option>
                                        </select>
                                    </div>
                                    <div class="col-sm-4 input-group">
                                        <select id="txtEspecificarAccidentesTrabajoSelect" class="form-control input-medium" disabled>
                                            <option value="" selected>Seleccionar</option>
                                            <option value="IESS">Instituto Ecuatoriano de Seguridad Social (IESS)</option>
                                            <option value="ISSFA">Instituto de Seguridad Social de las Fuerzas Armadas (ISSFA)</option>
                                            <option value="ISSPOL">Instituto de Seguridad Social de la Policía Nacional (ISSPOL) </option>
                                            <option value="MDT">Comisión Calificadora de Riesgos (MDT)</option>
                                        </select>
                                    </div>
                                    <div class="col-sm-4 input-group">
                                        <textarea id="txtObservacionesAccTrabajo" rows="3" style="width: 100%; resize: vertical;" placeholder="Registrar el nombre de la empresa, puesto de trabajo, área, datos sobre la enfermedad, así también colocar el diagnóstico con el Código CIE, y otra información relacionada con la enfermedad profesional suscitada, como secuelas en caso de existir." disabled></textarea>
                                    </div>

                                </div>
                            </div>

                            <div class="contenedor-horizontal" style="margin: 0 3rem 4rem 3rem;">
                                <label class="col-form-label" style="margin-left:1.5rem;">ENFERMEDADES PROFESIONALES</label>
                                <div class="horizontal-group-simple">
                                    <div class="input-group col-sm-2">
                                        <span class="input-group-addon" style="width: 25%;">Fecha:</span>
                                        <input type="date" class="form-control" id="fechaInst2" name="fechaInst2">
                                    </div>
                                    <div class="col-sm-1 text-center input-group">
                                        <select id="EnfermedadesProfSelect" class="form-control" onchange="handleEfermedadesProfSelect()">
                                            <option value="" disabled selected hidden>Si/No</option>
                                            <option value="si">SI</option>
                                            <option value="no">NO</option>
                                        </select>
                                    </div>
                                    <div class="col-sm-4 input-group">
                                            <select id="txtEspecificarEnfermedadesProfSelect" class="form-control input-medium" disabled>
                                                <option value="" selected>Seleccionar</option>
                                                <option value="IESS">Instituto Ecuatoriano de Seguridad Social (IESS)</option>
                                                <option value="ISSFA">Instituto de Seguridad Social de las Fuerzas Armadas (ISSFA)</option>
                                                <option value="ISSPOL">Instituto de Seguridad Social de la Policía Nacional (ISSPOL) </option>
                                                <option value="MDT">Comisión Calificadora de Riesgos (MDT)</option>
                                            </select>
                                    </div>

                                    <div class="col-sm-4 input-group">
                                            <textarea id="txtObservacionesEnfermedadesProf" rows="3" style="width: 100%; resize: vertical;" placeholder="Registrar el nombre de la empresa,
puesto de trabajo, área, datos sobre la enfermedad, así también colocar el diagnóstico con el Código CIE, y otra información relacionada con la enfermedad profesional suscitada, como secuelas en caso de existir."
                                                disabled></textarea>
                                    </div>
                                </div>
                            </div>

                            <!--  Cuadros   -->
                            <div class="contenedor-horizontal" style="margin-bottom:2rem;">
                                <label for="disabledTextInput" class="col-form-label" style="margin-left:2.5rem;">CONSTANTES VITALES Y ANTROPOMETRÍA </label>
                                <div class="horizontal-group-special" style="margin-top:1rem;">
                                    <div class="cuadros-ingreso-normal4" style="">
                                        <label class="input-group-addon select-medidas titulo-cuadros">PRESIÓN ARTERIAL</label>
                                        <input id="txtConstPresionArterial" type="number" class="form-control" placeholder="mmHg" min="0" style="text-align:center;">
                                    </div>
                                    <div class="cuadros-ingreso-normal4" style="">
                                        <span class="input-group-addon select-medidas titulo-cuadros">TEMPERATURA</span>
                                        <input id="txtConstTemperatura" type="number" class="form-control" placeholder="°C" min="0" style="text-align:center;">
                                    </div>
                                    <div class="cuadros-ingreso-normal4" style="">
                                        <span class="input-group-addon select-medidas titulo-cuadros">FRECUENCIA CARDIACA</span>
                                        <input id="txtConstFrecuenciaCardicaca" type="text" class="form-control" placeholder="Lat/min" min="0" style="text-align:center;">
                                    </div>
                                </div>
                                <div class="horizontal-group-special">
                                    <div class="cuadros-ingreso-normal4" style="">
                                        <span class="input-group-mio select-medidas titulo-cuadros" style="">SATURACIÓN DE OXÍGENO</span>
                                        <input id="txtConstSaturacionOxigeno" type="number" class="form-control" placeholder="O2%" min="0" style="text-align:center;">
                                    </div>
                                    <div class="cuadros-ingreso-normal4" style="">
                                        <span class="input-group-mio select-medidas titulo-cuadros" style="">FRECUENCIA RESPIRATORIA</span>
                                        <input id="txtConstFrecuenciaRespiratoria" type="text" class="form-control" placeholder="fr/min" min="0" style="text-align:center;">
                                    </div>
                                    <div class="cuadros-ingreso-normal4" style="">
                                        <span class="input-group-mio select-medidas titulo-cuadros" style="">PESO</span>
                                        <input id="txtConstPeso" type="text" class="form-control" placeholder="Kg" min="0" style="text-align:center;">
                                    </div>
                                </div>
                                <div class="horizontal-group-special">
                                    <div class="cuadros-ingreso-normal4" style="">
                                        <span class="input-group-mio select-medidas titulo-cuadros">TALLA</span>
                                        <input id="txtConstTalla" type="number" class="form-control" placeholder="cm" min="0" style="text-align:center;">
                                    </div>
                                    <div class="cuadros-ingreso-normal4" style="">
                                        <span class="input-group-mio select-medidas titulo-cuadros">ÍNDICE DE MASA CORPORAL</span>
                                        <input id="txtConstMasaCorporal" type="text" class="form-control" placeholder="kg/m2" style="text-align:center;">
                                    </div>
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px; ">
                                        <span class="input-group-mio select-medidas titulo-cuadros">PERÍMETRO ABDOMINAL</span>
                                        <input id="txtConstPerimetroAbdominal" type="text" class="form-control" placeholder="cm" style="text-align:center;">
                                    </div>
                                </div>

                           </div>   
                    </div>           

                        <!-- Cuadro Examen fisico regional -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">EXAMEN FÍSICO REGIONAL</h4>
                                </div>
                            </div>
                            <div class="well contenedor-horizontal" style="background-color: #d8e4e9; margin:2rem 3rem 1rem 3rem;">
                                    
                                <div class="well-header" style="margin-left:2rem; padding-top:2rem;">
                                        <label class="well-title">REGIONES</label>
                                </div>
                                    <div class="horizontal-cuadriculado-flow" style="margin: 0 2rem;">
                                         <!-- Contenedor del menú desplegable Piel -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Piel</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="pielA">
                                                    <label for="opcion1"> Cicatrices</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="pielB">
                                                    <label for="opcion2"> Tatuajes</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="pielC">
                                                    <label for="opcion3"> Piel  y Faneras</label>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- Contenedor del menú desplegable Ojos -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Ojos</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="ojosA">
                                                    <label for="opcion1"> Párpados</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="ojosB">
                                                    <label for="opcion2"> Conjuntivas</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="ojosC">
                                                    <label for="opcion3"> Pupilas</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="ojosD">
                                                    <label for="opcion4"> Córnea</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="ojosE">
                                                    <label for="opcion5"> Motilidad</label>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- Contenedor del menú desplegable Oido -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Oido</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="oidoA">
                                                    <label for="opcion1"> C. auditivo externo</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="oidoB">
                                                    <label for="opcion2"> Pabellón</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="oidoC">
                                                    <label for="opcion3"> Tímpanos</label>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- Contenedor del menú desplegable Oido -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Oro faringe</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="oroA">
                                                    <label for="opcion1"> Labios</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="oroB">
                                                    <label for="opcion2"> Lengua</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="oroC">
                                                    <label for="opcion3"> Faringe</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="oroD">
                                                    <label for="opcion4"> Amígdalas</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="oroE">
                                                    <label for="opcion5"> Dentadura</label>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- Contenedor del menú desplegable Oido -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Nariz</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="narizA">
                                                    <label for="opcion1"> Tabique</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="narizB">
                                                    <label for="opcion2"> Cornetes</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="narizC">
                                                    <label for="opcion3"> Mucosas</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="narizD">
                                                    <label for="opcion4"> Senos paranasales</label>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- Contenedor del menú desplegable Cuello -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Cuello</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="cuelloA">
                                                    <label for="opcion1"> Tiroides / masas</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="cuelloB">
                                                    <label for="opcion2"> Movilidad</label>
                                                </div>
                                            </div>
                                        </div>

                                    </div>

                                    <div class="horizontal-cuadriculado-flow" style="margin: 0 2rem;">  
                                        <!-- Contenedor del menú desplegable Torax -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Torax</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="toraxA">
                                                    <label for="opcion1"> Mamas</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="toraxB">
                                                    <label for="opcion2"> Corazón</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="toraxC">
                                                    <label for="opcion3">  Pulmones</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="ToraxD">
                                                    <label for="opcion4">  Parrilla Costal</label>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- Contenedor del menú desplegable Abdomen -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Abdomen</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="abdomenA">
                                                    <label for="opcion1"> Vísceras</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="abdomenB">
                                                    <label for="opcion2"> Pared abdominal</label>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- Contenedor del menú desplegable Columna -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Columna</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="columnaA">
                                                    <label for="opcion1"> Flexibilidad</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="columnaB">
                                                    <label for="opcion2"> Desviación</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="columnaC">
                                                    <label for="opcion3"> Dolor</label>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- Contenedor del menú desplegable Pelvis -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Pelvis</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="pelvisA">
                                                    <label for="opcion1"> Pelvis</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="pelvisB">
                                                    <label for="opcion2"> Genitales</label>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- Contenedor del menú desplegable Extremidades -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Extremidades</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="extremidadesA">
                                                    <label for="opcion1"> Vascular</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="extremidadesB">
                                                    <label for="opcion2"> Miembros superiores</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="extremidadesC">
                                                    <label for="opcion3"> Miembros inferiores</label>
                                                </div>
                                            </div>
                                        </div>
                                        <!-- Contenedor del menú desplegable Neurológico -->
                                        <div class="cuadros-ingreso-normal dropdown-container">
                                            <div class="input-group-addon dropdown-button">Neurológico</div>        
                                            <!-- Lista de opciones con casillas de verificación -->
                                            <div class="dropdown-content" style ="max-width:auto;">
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="neurologicoA">
                                                    <label for="opcion1"> Fuerza</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="neurologicoB">
                                                    <label for="opcion2"> Sensibilidad</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="neurologicoC">
                                                    <label for="opcion3"> Marcha</label>
                                                </div>
                                                <div class="dropdown-option">
                                                    <input type="checkbox" class="checkbox" id="neurologicoD">
                                                    <label for="opcion4"> Reflejos</label>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="contenedor-horizontal" style="margin: 1rem 2rem 0 2rem;">
                                        <label for="disabledTextInput" class="col-form-label">Observaciones</label>
                                        <textarea id="txtExamFisicoObservacion" rows="5" style="width: 100%; resize: vertical;" value="" placeholder="En la sección de “observaciones”, detallar la patología encontrada."></textarea>
                                    </div>

                                </div>
                        
                             </div>

                        <!-- Cuadro de Resultados " -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">RESULTADOS DE EXÁMENES GENERALES Y ESPECÍFICOS</h4>
                                </div>
                            </div>

                            <div class="horizontal-group-simple" style="margin:3rem 3rem 10px 3rem;">
                                <div class="cuadros-ingreso-normal3">
                                    <fieldset class="grupo-input text-center">
                                        <label for="disabledTextInput">TIPO DE EXÁMEN</label>
                                            <select id="txtTipoExamenSelect1" class="form-control">
                                                <option value="" selected>- Seleccionar -</option>
                                                <option value="ExamenGeneral">Exámenes generales</option>
                                                <option value="ExamenEspecifico">Exámenes específicos</option>                                                
                                            </select>
                                     </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal4">
                                    <div class="grupo-input text-center" >
                                        <label for="disabledTextInput">EXÁMEN</label>
                                        <input id="txtNomExamen1" type="text" class="form-control select-principal" placeholder="examen" value="">
                                    </div>
                                </div>    
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center">
                                        <label for="disabledTextInput">FECHA</label>
                                        <input type="date" class="form-control" id="fechaExam1" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal5">
                                    <div class="grupo-input text-center" >
                                        <label for="disabledTextInput">RESULTADOS</label>
                                        <input id="txtResExamen1" type="text" class="form-control select-medidas" placeholder="Resultado" value="">
                                    </div>
                                </div> 
                           </div>
                            <div class="horizontal-group-simple" style="margin-left:3rem; margin-right:3rem;">

                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="grupo-input text-center">
                                            <select id="txtTipoExamenSelect2" class="form-control">
                                                <option value="" selected>- Seleccionar -</option>
                                                <option value="ExamenGeneral">Exámenes generales</option>
                                                <option value="ExamenEspecifico">Exámenes específicos</option>                                                
                                            </select>
                                     </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <input id="txtNomExamen2" type="text" class="form-control select-principal" placeholder="examen" value="">
                                    </div>
                                </div>    
                                <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center">
                                        <input type="date" class="form-control" id="fechaExam2" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <input id="txtResExamen2" type="text" class="form-control select-medidas" placeholder="Resultado" value="">
                                    </div>
                                </div> 
                           </div>                           
                           
                            <div class="form-group row" style="margin: 0 2rem;">
                                <label for="disabledTextInput" class="col-form-label">Observaciones</label>
                                <textarea id="txtExamenbservacion" rows="3" style="width: 100%; resize: vertical;" value="" placeholder="En la sección de “observaciones”, detallar la patología encontrada."></textarea>
                            </div>

                        </div>

                         <!-- Cuadro de Diagnosticos " -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">DIAGNÓSTICO</h4>
                                </div>
                            </div>
                            <div class="horizontal-group-simple" style="margin:3rem 3rem 10px 3rem;">
                                <div class="cuadros-ingreso-normal7" >
                                    <div class="grupo-input text-center">
                                        <label for="disabledTextInput">DESCRIPCIÓN</label>
                                        <input id="txtDiagDescripcion1" type="text" class="form-control select-medidas" placeholder="Descripción" value="">
                                    </div>
                                </div> 
                                <div class="cuadros-ingreso-normal3">
                                    <div class="grupo-input text-center" >
                                        <label for="disabledTextInput">CÓDIGO CIE</label>
                                        <input id="txtDiagCIE1" type="text" class="form-control" placeholder="CIE" value="">
                                    </div>
                                </div> 
                                <div class="cuadros-ingreso-normal3">
                                    <fieldset class="grupo-input text-center">
                                        <label for="disabledTextInput">Diagnóstico</label>
                                            <select id="txtDiagnositicoSelect1" class="form-control">
                                                <option value="" selected>- Seleccionar -</option>
                                                <option value="PRE">Presuntivo</option>
                                                <option value="DEF">Definitivo</option>                                                
                                            </select>
                                     </fieldset>
                                </div>
                            </div>
                            <div class="horizontal-group-simple" style="margin-left:3rem; margin-right:3rem;">
                                <div class="cuadros-ingreso-normal7" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <input id="txtDiagDescripcion2" type="text" class="form-control select-medidas" placeholder="Descripción" value="">
                                    </div>
                                </div> 
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <input id="txtDiagCIE2" type="text" class="form-control" placeholder="CIE" value="">
                                    </div>
                                </div> 
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="grupo-input text-center">
                                            <select id="txtDiagnositicoSelect2" class="form-control">
                                                <option value="" selected>- Seleccionar -</option>
                                                <option value="PRE">Presuntivo</option>
                                                <option value="DEF">Definitivo</option>                                                
                                            </select>
                                     </fieldset>
                                </div>
                            </div>
                            <div class="horizontal-group-simple" style="margin-left:3rem; margin-right:3rem; margin-bottom: 4rem;">
                                <div class="cuadros-ingreso-normal7" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <input id="txtDiagDescripcion3" type="text" class="form-control select-medidas" placeholder="Descripción" value="">
                                    </div>
                                </div> 
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <div class="grupo-input text-center" >
                                        <input id="txtDiagCIE3" type="text" class="form-control" placeholder="CIE" value="">
                                    </div>
                                </div> 
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="grupo-input text-center">
                                            <select id="txtDiagnositicoSelect3" class="form-control">
                                                <option value="" selected>- Seleccionar -</option>
                                                <option value="PRE">Presuntivo</option>
                                                <option value="DEF">Definitivo</option>                                                
                                            </select>
                                     </fieldset>
                                </div>
                            </div>

                            <div class="contenedor-horizontal" style="margin-bottom: 2rem;">   
                                <label class="col-form-label" style="margin-left:1rem;">EVALUACIÓN MÉDICA DE RETIRO</label>
                                <div class="horizontal-group-simple" style="margin:0 1rem 0 1rem;">                                    
                                    <div class="text-center input-group" style="width:30%;">
                                        <select id="EvaluacionRetiroSelect" class="form-control" onchange="handleAccidentesTrabajoSelect()">
                                             <option value="" selected hidden>SE REALIZÓ LA EVALUACIÓN?</option>
                                             <option value="" >- Seleccionar -</option>
                                             <option value="si">SI</option>
                                             <option value="no">NO</option>
                                        </select>
                                    </div>
                                    <div class="col-sm-8 input-group">
                                        <textarea id="txtEvaluacionRetiroObservacion" rows="3" style="width: 100%; resize: vertical;" placeholder="En la sección de observaciones colocar todo los datos relacionados con
la condición de salud del usuario al momento de la salida." disabled></textarea>
                                    </div>

                                </div>
                            </div>

                            <div class="form-group" style="margin: 0 2rem;">
                                <label for="disabledTextInput" class="col-form-label">RECOMENDACIONES Y/O TRATAMIENTO</label>
                                <textarea id="txtRecomendacion" rows="4" style="width: 100%; resize: vertical;" value="" placeholder="De acuerdo a la valoración médica efectuada colocar las recomendaciones y/o tratamiento
farmacológico y no farmacológico."></textarea>
                            </div>

                        </div>                    

                </div>
                <div class="" id="btnCarga" style="display:none; justify-content:center;">
                    <button class="btn btn-info col-sm-6" type="button" id="btnCargarDatosPersonales" onclick="GuardarRetiro()" style="width: 25%; margin: 3rem;">Cargar Datos</button>
                </div>

                <!-- Boton flotante de regreso-->
                <a id="btnRegresar" class="chat-button"></a>

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

            </asp:Panel>
        </div>
    </div>
</asp:Content>