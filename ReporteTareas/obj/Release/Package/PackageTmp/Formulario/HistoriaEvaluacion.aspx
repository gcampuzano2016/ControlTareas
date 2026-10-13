<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="HistoriaEvaluacion.aspx.cs" Inherits="ReporteTareas.Formulario.HistoriaEvaluacion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Estilos -->
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <link href="../dist/css/depMedico.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">

    <!-- Scripts -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../js/HistorialEvaluacion.js?v=1"></script>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <link href="../bower_components/sweetalert/css/animate.css" rel="stylesheet" />
    <link href="../bower_components/sweetalert/css/sweetalert2.min.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert2.all.min.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    HistoriaEvaluacion.aspx.cs

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

                

                <!-- PESTAÑAS -->
                <%--<div class="tabs" id="pestanias" style="background-color: #0D2538; padding-top: 1rem; display:block;">
                    <ul class="nav nav-tabs">
                        <li class="nav-item">
                            <a class="nav-link" id="pestania1" style="background-color: #A90505; color: #EDEDED; cursor: pointer; font-weight: bold;" 
                                onmouseover="cambiarColorHover('pestania1')" onmouseout="restaurarColor('pestania1')" onclick="VerFormDatosPersonales()">EVALUCION OCUPACIONAL</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" id="pestania2" style="background-color: #A90505; color: #EDEDED; cursor: pointer; font-weight: bold;"
                                onmouseover="cambiarColorHover('pestania2')" onmouseout="restaurarColor('pestania2')" onclick="VerFormConsulta()">CERTIFICADO</a>
                        </li>
                    </ul>
                </div>--%>

                <!-- ******************************************************************** -->
                <!--                            PAGINA                                    -->
                <!-- ******************************************************************** -->
                                
                <div class="" style="background-color: #9DA8AD; margin-top:1rem;">

                    <div class="input-group col-sm-4" style="display:none; margin: 0 auto; padding-top: 1rem; padding-bottom: 1rem;">
                        <input type="text" id="txtEmpleado" class="form-control" placeholder="Buscar paciente">                    
                        <div class="input-group-btn">
                            <button class="btn btn-default" type="button" id="btnBuscarDatosPersonales" ><i class="glyphicon glyphicon-search"></i></button>
                        </div>
                    </div>


                    <!-- Contenido de la pestaña "Pestaña 1" -->
                    <div id="pestaniaEvaluacion" style="display: block; padding: 1rem 1rem 0.2rem 1rem;">
                    
                        <!-- Cuadro Datos personales -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">Datos Personales</h4>
                                </div>
                            </div>
                            <div class="">
                                <div class="row" style="margin: 0 0.5rem 1rem 0.5rem; justify-content: space-between">
                                    <formview id="frmDatos grid" class="form-horizontal" action="/action_page.php">
                                        <div class="horizontal-simple col-sm-9">
                                            <!-- Campos Nombre y Cedula -->
                                             <div class="horizontal-group">
                                                <div class="input-group col-sm-12">
                                                    <span class="input-group-addon select-dorado" style="width: 25%;"><i class="glyphicon glyphicon-user"></i>  Nombre:</span>
                                                    <input id="txtNombre" type="text" class="form-control" name="nombre" placeholder="Nombre" oninput="convertirAMayusculas(this)">
                                                </div>
                                            </div>
                                            <div class="horizontal">
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon select-temas" style="width: 35%; text-align:left;"><i class="glyphicon glyphicon-credit-card"></i>  Cédula:</span>
                                                    <input id="txtCedula" type="text" class="form-control" name="cedula" placeholder="Cedula" readonly>
                                                </div>
                                            </div>
                                            <div class="horizontal">  
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon select-temas" style="width: 35%; text-align:left;"><i class="glyphicon glyphicon-leaf"></i>  Estado civil:</span>
                                                    <input id="txtEstadoCivil" type="text" class="form-control" name="estadocivil" placeholder="Estado civil" readonly>
                                                </div>
                                                <div class="input-group col-sm-4" style="margin-left: 6rem;">
                                                    <span class="input-group-addon select-temas" style="width: 25px;"><i class="glyphicon glyphicon-ok-circle"></i>  Sexo:</span>                                                    
                                                    <input type="text" class="form-control" id="txtSexo" name="txtSexo" readonly>
                                                </div>
                                            </div>
                                            <div class="horizontal">    
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon input-basic3 select-temas" ><i class="glyphicon glyphicon-gift"></i>  Fecha de Nacimiento:</span>
                                                    <input type="text" class="form-control" style="" id="fechaNac" name="fechaNac" readonly>
                                                </div>
                                                <div class="input-group col-sm-4" style="margin-left: 6rem;">
                                                    <span class="input-group-addon input-basic select-temas"><i class="glyphicon glyphicon-star"></i>  Edad:</span>
                                                    <input id="txtEdad" type="number" class="form-control" name="edad" placeholder="0" readonly>
                                                </div>
                                            </div>

                                            <div class="horizontal">                                                
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon select-temas" style="width: 35%; text-align:left;"><i class="glyphicon glyphicon-home"></i>  Sociedad:</span>
                                                    <input id="txtSociedad" type="text" class="form-control" name="sociedad" placeholder="Sociedad" readonly>
                                                </div>    
                                                <div class="input-group col-sm-5" style="margin-left: 6rem;">
                                                    <span class="input-group-addon input-azulmedio"><i class="fa fa-h-square"></i>  Num de Historia:</span>
                                                    <input id="txtNumHistoria" type="text" class="form-control" name="sociedad" placeholder="XXXX-XXXX" value="">
                                                    <span class="indicator-der"></span>
                                                </div> 
                                            </div>
                                            <div class="horizontal">  
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon input-medium select-temas" style="width: 35%; text-align:left;"><i class="glyphicon glyphicon-link"></i>  Area de trabajo:</span>
                                                    <input id="txtAreaTrabajo" type="text" class="form-control" name="areatrabajo" placeholder="Area de trabajo" readonly>
                                                </div>
                                                    
                                                <div class="input-group col-sm-5" style="margin-left: 6rem;">
                                                    <span class="input-group-addon input-azulmedio"><i class="fa fa-file-text"></i>  Num de Archivo:</span>
                                                    <input id="txtNumArchivo" type="text" class="form-control" name="sociedad" placeholder="XXXX-XXXX" value="">
                                                    <span class="indicator-der"></span>
                                                </div>
                                            </div>
                                            <%--<div class="horizontal-group">
                                                <div class="input-group col-sm-8">
                                                    <span class="input-group-addon input-medium select-temas" style="text-align:left;"><i class="glyphicon glyphicon-lock"></i>  Puesto de trabajo:</span>
                                                    <input id="txtPuestoTrabajo" type="text" class="form-control" name="puestotrabajo" placeholder="Puesto de trabajo" readonly>
                                                </div>                                                
                                            </div>--%>

                                            <div class="horizontal">  
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon input-medium select-temas" style="width: 35%; text-align:left;"><i class="glyphicon glyphicon-link"></i>  Grupo Sanguineo:</span>
                                                    <input id="txtGruposanguineo" type="text" class="form-control" name="grupoSabguineo" value="">
                                                </div>
                                                <div class="input-group col-sm-5" style="margin-left: 6rem;">
                                                    <span class="input-group-addon select-temas" style="width: 25px;"><i class="glyphicon glyphicon-ok-circle"></i>  Lateralidad:</span>                                                    
                                                    <input id="txtLateralidad" type="text" class="form-control" style="" name="txtLateralidad" value="">
                                                </div>
                                            </div>

                                            <div class="horizontal"> 
                                                <div class="input-group col-sm-6">
                                                    <span class="input-group-addon input-medium select-temas" style="width: 35%; text-align:left;"><i class="glyphicon glyphicon-link"></i> Atencion Prioritaria: </span>
                                                    <select id="AtencionPrioritariaSelect" class="form-control">
                                                        <option value="" selected>- Seleccionar -</option>
                                                        <option value="atencionEmb">Embarazada</option>
                                                        <option value="atencionDis">Persona con Discapacidad</option>
                                                        <option value="atencionCat">Enf.Catastrófica</option>
                                                        <option value="atencionLac">Lactancia</option>
                                                        <option value="atencionAdu">Adulto Mayor</option>
                                                    </select>
                                                </div>
                                            </div>

                                            
                                            <!-- ... otros campos ... -->
                                        </div>

                                        <div class="col-sm-3" style="text-align: center;">
                                            <div id="imagenDiv" class="imagen-div"></div>
                                            <!--img id="imgEmpleado" src="../carrusel/imagenes/usuarios.png" alt="Imagen de Empleado" height="200" width="200"-->
                                            <button type="button" class="btn btn-danger" style="margin-top: 1rem;"><i class="glyphicon glyphicon-user"></i>  Contacto de emergencia</button>
                                        </div>

                                    </formview>
                                </div>

                            </div>

                        </div>
                        
                        
                        <!-- Cuadro de Consulta " -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">MOTIVO CONSULTA</h4>
                                </div>
                            </div>
                            <div>

                                <%--<div class="horizontal-group-special contenedor-horizontal" style="justify-content:flex-start; margin-top:0; ">
                                    <div class="horizontal-group-simple form-group col-sm-3" style="text-align:center; margin-bottom:0;">
                                        <label class="control-label" >Fecha actual:</label>
                                        <h4 for="txtFechaActual" style="color:darkblue;">12/05/2023</h4>
                                    </div>  
                                </div>  --%>  
                                
                                <%--<div style="text-align:center;padding:0 0; background-color: #333; border-radius: 10px; margin: 1rem 3rem 20px 3rem; ">
                                    <h3><a style="text-decoration:none;color:white;" href="https://www.zeitverschiebung.net/es/city/3652462"><br />Quito, Ecuador</a></h3>
                                    <iframe src="https://www.zeitverschiebung.net/clock-widget-iframe-v2?language=es&size=medium&timezone=America%2FGuayaquil" width="100%" height="115" frameborder="0" seamless></iframe>
                                </div> --%> 
                                <div class="horizontal-group-special">
                                    <div class="input-group col-sm-3">
                                        <label for="disabledTextInput" class="control-label"> Puesto de Trabajo CIUO: </label>
                                        <input id="txtPuestoTrabajo" type="text" class="form-control" placeholder="Puesto">                                        
                                    </div>  
                                    <div class="input-group col-sm-3">
                                        <label for="disabledTextInput" class="control-label"> Motivo de consulta: </label>
                                        <select id="selectMotivoConsulta" class="form-control">
                                            <option value="" selected>- Seleccionar -</option>
                                            <option value="tipoconsultaIngreso">INGRESO</option>
                                            <option value="tipoconsultaPeriodico">PERIÓDICO</option>
                                            <option value="tipoconsultaReintegro">REINTEGRO</option>
                                            <option value="tipoconsultaRetiro">RETIRO</option>
                                        </select>
                                    </div>
                                    <div class="input-group col-sm-3">
                                        <label for="disabledTextInput" class="control-label"> Fecha de atención: </label>
                                        <%--<span class="input-group-addon" style="width: 25%;"></span>--%>
                                        <input type="date" class="form-control" id="fechaAtencion">
                                    </div> 
                                </div>

                                <div class="horizontal-group-special">
                                    <div class="input-group col-sm-3">
                                        <label for="disabledTextInput" class="control-label"> Fecha de ingreso al trabajo: </label>
                                        <%--<span class="input-group-addon" style="width: 25%;"></span>--%>
                                        <input type="date" class="form-control" id="fechaIngresoTrab">
                                    </div>         
                                    <div class="input-group col-sm-3">
                                        <label for="disabledTextInput" class="control-label"> Fecha de Reintegro: </label>
                                        <%--<span class="input-group-addon" style="width: 25%;">Fecha de Reintegro: </span>--%>
                                        <input type="date" class="form-control" id="fechaReintegroTrab">
                                    </div> 
                                    <div class="input-group col-sm-3">
                                        <label for="disabledTextInput" class="control-label"> Fecha del último día laboral/salida: </label>
                                        <%--<span class="input-group-addon" style="width: 25%;"> </span>--%>
                                        <input type="date" class="form-control" id="fechaUltimoDiaLab">
                                    </div> 
                                </div>

                                <div class="horizontal-group-special" style="justify-content:flex-start; margin:0 2.5rem;">
                                    <div class="form-group col-sm-10" style="margin-bottom:0;">
                                        <label for="disabledTextInput" class="control-label">Observación</label>
                                        <textarea id="txtMotivoConsulta" rows="3" style="width: 100%; resize: vertical;" placeholder="Anotar la causa del problema en la versión del informante"></textarea>
                                    </div>
                                </div>

                            </div>

                        </div>
                        
                        <!-- Cuadro de Antecedentes  " -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0; margin-bottom:3rem;">
                                <div class="color-texto-primario">
                                    <h4 class="well-title">ANTECEDENTES PERSONALES</h4>
                                </div>
                            </div>
                            <div class="horizontal-group-special contenedor-horizontal cuadros-blancos-redondeados">
                                <div class="col-sm-12">
                                    <label class="color-texto-secundario" style="margin: 0.5rem auto 1rem auto">ANTECEDENTES CLÍNICOS Y QUIRÚRGICOS</label>
                                    <textarea id="txtAntecedentesPersonales" class="texto-muted cuadros-fondo-texto-ingreso" rows="4"  placeholder="Registrar la información proporcionada por el usuario en la anamnesis, en lo referente a antecedentes clínicos registrar enfermedades, alergias, traumatismos; en cuanto a antecedentes quirúrgicos, detallar las principales intervenciones quirúrgicas a las que se ha sometido."></textarea>
                                </div>
                            </div>
                            <div class="horizontal-group-special contenedor-horizontal cuadros-blancos-redondeados">
                                <div class="col-sm-12">
                                    <label class="color-texto-secundario" style="margin: 0.5rem auto 1rem auto">ANTECEDENTES FAMILIARES</label>                                    
                                    <textarea id="txtAntecedentesFamiliares" class="texto-muted cuadros-fondo-texto-ingreso" rows="4" placeholder="Registrar la información anotando el número de cada una de las patologías de importancia presentadas en los familiares del usuario."></textarea>
                                </div>
                            </div>

                            <div class="design-section" style="margin: 5rem auto;">
        
                                <div class="horizontal-group-special selector-si-no-3 cuadros-blancos-redondeados" style="margin-left:10rem; margin-right:10rem; border-radius:10px;">
                                    <div>
                                        <label class="color-texto-secundario">¿En caso de necesitar una transfución, autoriza?</label>
                                        <div class="opciones">
                                            <input type="radio" name="txtAutorizacionBtn1" value="si" id="d3-pregunta1-si">
                                            <label for="d3-pregunta1-si">
                                                <i class="fas fa-check icono"></i>
                                                <span class="texto">Sí</span>
                                            </label>
                
                                            <input type="radio" name="txtAutorizacionBtn1" value="no" id="d3-pregunta1-no">
                                            <label for="d3-pregunta1-no">
                                                <i class="fas fa-times icono"></i>
                                                <span class="texto">No</span>
                                            </label>
                                        </div>
                                    </div>                                    
                                </div>

                                <div class="horizontal-group-special selector-si-no-3 cuadros-blancos-redondeados" style="margin-left:10rem; margin-right:10rem; border-radius:10px;">
                                    <div>
                                        <label class="color-texto-secundario">¿Se encuentra bajo algún tratamiento hormonal?</label>                                    
                                        <div class="opciones">
                                            <input type="radio" name="txtAutorizacionBtn2" value="si" id="d3-pregunta2-si" onchange="mostrarInput('d3-input2', true)">
                                            <label for="d3-pregunta2-si">
                                                <i class="fas fa-check icono"></i>
                                                <span class="texto">Sí</span>
                                            </label>
                
                                            <input type="radio" name="txtAutorizacionBtn2" value="no" id="d3-pregunta2-no" onchange="mostrarInput('d3-input2', false)">
                                            <label for="d3-pregunta2-no">
                                                <i class="fas fa-times icono"></i>
                                                <span class="texto">No</span>
                                            </label>
                                        </div> 
                                    </div>                                    
                                    
                                    <div id="d3-input2" class="col-sm-6 horizontal-group-simple input-adicional">
                                        <label>¿Cuál?</label>
                                        <input id="tratamientohormonalcual" class="cuadros-fondo-input-ingreso" type="text" placeholder="Describir...">
                                    </div>
                                    
                                </div>
                                
                            </div>

                            <%--EXAMENES PARA MUJER--%> 
                            <section class="contenedor-horizontal cuadros-blancos-redondeados" style="padding:1rem 0.5rem; margin-top:4rem;">
                                <div class="horizontal-group-around" >
                                    <label class="col-sm-12 color-texto-primario" style="margin: 0.5rem auto 2rem auto"> ANTECEDENTES GINECO OBSTÉTRICOS</label>
                                </div>

                                <div class="horizontal-group-simple">
                                    <div class="grupo-input col-sm-3" style="margin-bottom: 10px; padding-right: 0;">
                                        <label class="color-texto-secundario" style="">Última menstruación:</label>
                                        <input type="date" class="cuadros-fondo-input-ingreso" style="line-height: inherit;" id="fechaUltimaMens" name="fechaUltimaMens" value="">
                                    </div>
                                    <div class="grupo-input col-sm-2" style="margin-bottom: 10px; padding-right: 0px; padding-left: 0;">
                                        <label class="color-texto-secundario">Gestas:</label>
                                        <input id="txtNumGestas" type="number" class="cuadros-fondo-input-ingreso" name="nombre" placeholder="0" min="0" value="">                                    
                                    </div>

                                    <div class="grupo-input col-sm-2" style="margin-bottom: 10px; padding-right: 0px; padding-left: 0;">
                                        <label class="color-texto-secundario" style="">Partos:</label>
                                        <input id="txtNumPartos" type="number" class="cuadros-fondo-input-ingreso" name="nombre" placeholder="0" min="0" value="">
                                    </div>

                                    <div class="grupo-input col-sm-2" style="margin-bottom: 10px; padding-right: 0px; padding-left: 0;">
                                        <label class="color-texto-secundario">Cesáreas:</label>
                                        <input id="txtNumCesareas" type="number" class="cuadros-fondo-input-ingreso" placeholder="0" min="0" value="">
                                    </div>
                                    <div class="grupo-input col-sm-2" style="margin-bottom: 10px; padding-left: 0;">
                                        <label class="color-texto-secundario">Abortos:</label>
                                        <input id="txtNumAbortos" type="number" class="cuadros-fondo-input-ingreso" placeholder="0" min="0" value="">
                                    </div>
                                </div>

                                <div class="horizontal-group-around">     
                                    <div class="col-sm-12 well well-sm cuadros-blancos-redondeado-ingreso cuadros-fondo-texto-ingreso" style="">
                                        <div>
                                            <label class="color-texto-secundario">¿ Método de planificación familiar ?</label>
                                        </div>
                                        <div class="horizontal-group-special">
                                            <div class="col-sm-4 text-center btn-group" role="group" style=" margin-left:5rem;">
                                                <button type="button" id="siMetodoFem" class="btn btn-custom" onclick="toggleButtonColor('siMetodoFem')">Sí</button>
                                                <button type="button" id="noMetodoFem" class="btn btn-custom" onclick="toggleButtonColor('noMetodoFem')">No</button>
                                            </div> 
                                            <div class="grupo-input col-sm-8" style="margin-bottom: 10px;">
                                                <input id="txtCualMetodoFem" type="text" class="form-control" name="nombre" placeholder="Cual" value="">
                                            </div>
                                        </div>

                                    </div>
                                </div>                                

                                <div class="horizontal-group-simple" style="margin:1rem 1rem 0 1rem;">
                                                                
                                    <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center" >
                                            <label class="color-texto-secundario">EXÁMENES REALIZADOS?</label>
                                            <input id="txtNomExamen1F" type="text" class="cuadros-fondo-input-ingreso" placeholder="Cual?" value="">
                                        </div>
                                    </div>    
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center">
                                            <label class="color-texto-secundario">AÑO</label>
                                            <input type="number" class="cuadros-fondo-input-ingreso" id="fechaExam1F" placeholder="1900" value="" oninput="validarAnio(this)" min="1900" max="2030">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal6" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center" >
                                            <label class="color-texto-secundario">RESULTADOS</label>
                                            <input id="txtResExamen1F" type="text" class="cuadros-fondo-input-ingreso" placeholder=" Registrar resultado únicamente si interfiere con la actividad laboral y previa autorización del titular" value="">
                                        </div>
                                    </div> 
                               </div>

                                <div class="horizontal-group-simple" style="margin: 0 1rem 1rem 1rem;">
                                
                                    <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center" >
                                            <input id="txtNomExamen2F" type="text" class="cuadros-fondo-input-ingreso" placeholder="Cual?" value="">
                                        </div>
                                    </div>    
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center">
                                            <input type="number" class="cuadros-fondo-input-ingreso" id="fechaExam2F" placeholder="1900" value="" oninput="validarAnio(this)" min="1900" max="2030">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal6">
                                        <div class="grupo-input text-center" >
                                            <input id="txtResExamen2F" type="text" class="cuadros-fondo-input-ingreso" placeholder=" Registrar resultado únicamente si interfiere con la actividad laboral y previa autorización del titular" value="">
                                        </div>
                                    </div> 
                               </div>                                

                            </section>

                            <%--EXAMENES PARA HOMBRES--%> 
                            <section class="contenedor-horizontal cuadros-blancos-redondeados" style="margin-top: 5rem; margin-bottom:3rem; padding:1rem 0.5rem;">
                                <div class="horizontal-group-around" style="">
                                    <label class="col-sm-12 color-texto-primario" style="margin: 0.5rem auto 1rem auto">ANTECEDENTES REPRODUCTIVOS MASCULINOS</label>
                                </div>

                                <div class="horizontal-group-around">     
                                    <div class="col-sm-12 well well-sm cuadros-blancos-redondeado-ingreso cuadros-fondo-texto-ingreso" style="">
                                        <div>
                                            <label class="color-texto-secundario">¿ Método de planificación familiar ?</label>
                                        </div>
                                        <div class="horizontal-group-special">
                                            <div class="col-sm-4 text-center btn-group" role="group" style="margin-left:5rem;">
                                                <button type="button" id="siMetodomMasc" class="btn btn-custom" onclick="toggleButtonColor('siMetodomMasc')">Sí</button>
                                                <button type="button" id="noMetodomMasc" class="btn btn-custom" onclick="toggleButtonColor('noMetodomMasc')">No</button>
                                            </div> 
                                            <div class="grupo-input col-sm-8" style="margin-bottom: 10px;">
                                                <input id="txtCualMasc" type="text" class="form-control" placeholder="Cual" value="">
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="horizontal-group-simple" style="margin:0 1rem 0 1rem;">
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center">
                                            <label class="color-texto-secundario">EXÁMENES REALIZADOS?</label>
                                            <input id="txtNomExamen1M" type="text" class="texto-muted cuadros-fondo-input-ingreso" placeholder="Cual?" value="">
                                        </div>
                                    </div>    
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center">
                                            <label class="color-texto-secundario">AÑO</label>
                                            <input type="number" class="texto-muted cuadros-fondo-input-ingreso" id="fechaExam1M" placeholder="1900" value="" oninput="validarAnio(this)" min="1900" max="2030">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal7" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center">
                                            <label class="color-texto-secundario">RESULTADOS</label>
                                            <input id="txtResExamen1M" type="text" class="texto-muted cuadros-fondo-input-ingreso" placeholder="Registrar resultado únicamente si interfiere con la actividad laboral..." value="">
                                        </div>
                                    </div> 
                                </div>

                                <div class="horizontal-group-simple" style="margin: 0 1rem 1rem 1rem;">
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center">
                                            <input id="txtNomExamen2M" type="text" class="texto-muted cuadros-fondo-input-ingreso" placeholder="Cual?" value="">
                                        </div>
                                    </div>    
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center">
                                            <input type="number" class="texto-muted cuadros-fondo-input-ingreso" id="fechaExam2M" placeholder="1900" value="" oninput="validarAnio(this)" min="1900" max="2030">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal7" style="margin-bottom: 10px;">
                                        <div class="grupo-input text-center">
                                            <input id="txtResExamen2M" type="text" class="texto-muted cuadros-fondo-input-ingreso" placeholder="Registrar resultado únicamente si interfiere con la actividad laboral..." value="">
                                        </div>
                                    </div> 
                                </div>          
                                
                            </section>                    
 
                            <!--    Cuadros de consumo de sustancias    -->
                            <section class="contenedor-horizontal cuadros-blancos-redondeados" id="consumoSustancias" style="margin-top: 5rem; margin-bottom:3rem; padding:1rem 0.5rem;">
                                <div class="col-sm-12">
                                    <label class="color-texto-primario" style="margin: 0.5rem auto 1.5rem auto; display:block;">CONSUMO DE SUSTANCIAS</label>
                                </div>

                                <!-- Cabecera -->
                                <div class="tabla-header-row">
                                <div class="tabla-col-label">Hábitos tóxicos</div>
                                <div class="tabla-col-sm">Si/No</div>
                                <div class="tabla-col-md">Tiempo consumo</div>
                                <%--<div class="tabla-col-md">Cantidad</div>--%>
                                <div class="tabla-col-sm">Ex cons.</div>
                                <div class="tabla-col-md">Tiempo abstinencia</div>
                            </div>

                                <!-- Tabaco -->
                                <div class="tabla-data-row">
                                    <div class="tabla-col-label">Tabaco</div>
                                    <div class="tabla-col-md">
                                        <select id="tabacoSelect" class="cuadros-fondo-input-ingreso select-placeholder" onchange="handleTabacoSelect()">
                                            <option value=""selected>Si/No</option>
                                            <option value="no">NO</option>
                                            <option value="si">SI</option>
                                        </select>
                                    </div>
                                    <div class="tabla-col-md">
                                        <input id="txtTiempoconsumoTabaco" type="number" class="cuadros-fondo-input-ingreso" placeholder="meses" disabled>
                                    </div>
                                    <%--<div class="tabla-col-md">
                                        <input id="txtCantidadTabaco" type="number" class="cuadros-fondo-input-ingreso" placeholder="por semana" disabled>
                                    </div>--%>
                                    <div class="tabla-col-md">
                                        <select id="exConsumidoraSelectTabaco" class="cuadros-fondo-input-ingreso select-placeholder" disabled>
                                            <option value=""selected>Si/No</option>
                                            <option value="no">NO</option>
                                            <option value="si">SI</option>
                                        </select>
                                    </div>
                                    <div class="tabla-col-md">
                                        <input id="txtTiempoAbstinenciaTabaco" type="number" class="cuadros-fondo-input-ingreso" placeholder="meses" disabled>
                                    </div>
                                </div>

                                <!-- Alcohol -->
                                <div class="tabla-data-row">
                                    <div class="tabla-col-label">Alcohol</div>
                                    <div class="tabla-col-md">
                                        <select id="alcoholSelect" class="cuadros-fondo-input-ingreso select-placeholder" onchange="handleAlcoholSelect()">
                                            <option value=""selected>Si/No</option>
                                            <option value="no">NO</option>
                                            <option value="si">SI</option>
                                        </select>
                                    </div>
                                    <div class="tabla-col-md">
                                        <input id="txtTiempoconsumoAlcohol" type="number" class="cuadros-fondo-input-ingreso" placeholder="meses" disabled>
                                    </div>
                                    <%--<div class="tabla-col-md">
                                        <input id="txtCantidadAlcohol" type="number" class="cuadros-fondo-input-ingreso" placeholder="por semana" disabled>
                                    </div>--%>
                                    <div class="tabla-col-md">
                                        <select id="exConsumidoraSelectAlcohol" class="cuadros-fondo-input-ingreso select-placeholder" disabled>
                                            <option value=""selected>Si/No</option>
                                            <option value="no">NO</option>
                                            <option value="si">SI</option>
                                        </select>
                                    </div>
                                    <div class="tabla-col-md">
                                        <input id="txtTiempoAbstinenciaAlcohol" type="number" class="cuadros-fondo-input-ingreso" placeholder="meses" disabled>
                                    </div>
                                </div>

                                <!-- Otra -->
                                <div class="tabla-data-row" style="">
                                    <div class="tabla-col-label">
                                        <input id="txtOtraSustancia" type="text" class="cuadros-fondo-input-ingreso" placeholder="Otra sustancia">
                                    </div>
                                    <div class="tabla-col-md">
                                        <select id="otraSelect" class="cuadros-fondo-input-ingreso select-placeholder" onchange="handleOtraSelect()">
                                            <option value=""selected>Si/No</option>
                                            <option value="no">NO</option>
                                            <option value="si">SI</option>
                                        </select>
                                    </div>
                                    <div class="tabla-col-md">
                                        <input id="txtTiempoconsumoOtra" type="number" class="cuadros-fondo-input-ingreso" placeholder="meses" disabled>
                                    </div>
                                    <%--<div class="tabla-col-md">
                                        <input id="txtCantidadOtra" type="number" class="cuadros-fondo-input-ingreso" placeholder="por semana" disabled>
                                    </div>--%>
                                    <div class="tabla-col-md">
                                        <select id="exConsumidorSelectOtra" class="cuadros-fondo-input-ingreso select-placeholder" disabled>
                                            <option value=""selected>Si/No</option>
                                            <option value="no">NO</option>
                                            <option value="si">SI</option>
                                        </select>
                                    </div>
                                    <div class="tabla-col-md">
                                        <input id="txtTiempoAbstinenciaOtra" type="number" class="cuadros-fondo-input-ingreso" placeholder="meses" disabled>
                                    </div>
                                </div>

                                <!-- Incidentes -->
                                <div style="margin: 1.5rem 1rem 0 1rem;">
                                    <label class="color-texto-secundario">Observación:</label>
                                    <textarea id="txtHbtsIncidentes" rows="3" class="cuadros-fondo-texto-ingreso" placeholder="Describir los principales incidentes suscitados"></textarea>
                                </div>
                            </section>

                            <!--    Cuadros de estilo de vida    -->  
                            <section class="contenedor-horizontal cuadros-blancos-redondeados" id="estiloVida" style="margin-top: 5rem; margin-bottom:3rem; padding:1rem 0.5rem;">
                                <div class="horizontal-group-around" style="margin-bottom: 1rem;">
                                    <label class="col-sm-12 col-form-label color-texto-primario">ESTILO DE VIDA</label>
                                </div>

                                <!-- Cabecera -->
                                <div class="tabla-header-row">
                                    <div class="tabla-col-label">Estilos de vida</div>
                                    <div class="tabla-col-sm">Si/No</div>
                                    <div class="tabla-col-md">¿Cuál?</div>
                                    <div class="tabla-col-md">Tiempo (horas)</div>
                                </div>

                                <!-- Actividad física -->
                                <div class="tabla-data-row">
                                    <div class="tabla-col-label">Actividad física</div>
                                    <div class="tabla-col-sm">
                                        <select id="ActividadFisiscaSelect" class="cuadros-fondo-input-ingreso select-placeholder" onchange="handleActividadSelect()">
                                            <option value="" selected>Si/No</option>
                                            <option value="no">No</option>
                                            <option value="si">Si</option>
                                        </select>
                                    </div>
                                    <div class="tabla-col-md">
                                        <input id="txtCualActividad1" value="" type="text" class="cuadros-fondo-input-ingreso" placeholder="Actividad" disabled>
                                    </div>
                                    <div class="tabla-col-md">
                                        <input id="txtFrecuenciaActividad1" value="" type="number" class="cuadros-fondo-input-ingreso" placeholder="en semana" disabled>
                                    </div>
                                </div>

                                <!-- Botón agregar actividad -->
                                <div style="margin: 0.5rem 1rem;">
                                    <button class="btn btn-danger" type="button" id="botonAgregarActividad" onclick="agregarActividad()" disabled>+ Agregar</button>
                                </div>

                                <!-- Medicación habitual -->
                                <div class="tabla-data-row" id="medicacionContainer">
                                    <div class="tabla-col-label">Medicación habitual</div>
                                    <div class="tabla-col-sm">
                                        <select id="MedicacionHabSelect" class="cuadros-fondo-input-ingreso select-placeholder" onchange="handleMedicacionSelect()">
                                            <option value="" selected>Si/No</option>
                                            <option value="no">No</option>
                                            <option value="si">Si</option>
                                        </select>
                                    </div>
                                    <div class="tabla-col-md">
                                        <input id="txtCualMedicamento1" value="" type="text" class="cuadros-fondo-input-ingreso" placeholder="Medicamento" disabled>
                                    </div>
                                    <div class="tabla-col-md">
                                        <input id="txtCantdidadMed1" value="" type="number" class="cuadros-fondo-input-ingreso" placeholder="Cantidad" disabled>
                                    </div>
                                </div>

                                <!-- Botón agregar medicación -->
                                <div style="margin: 0.5rem 1rem 1rem 1rem;">
                                    <button class="btn btn-danger" type="button" id="botonAgregarMedicacion" onclick="agregarMedicacion()" disabled>+ Agregar</button>
                                </div>
                            </section>

                        </div>

                        <!--    Cuadros de Enfermedad o problema actual    -->    
                        <section class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <!--    Cuadro de HÁBITOS TÓXICOS    -->
                            <div class="well-header" style="margin-top: 0; margin-bottom:3rem;">
                                <div>
                                     <h4 class="well-title">ENFERMEDAD O PROBLEMA ACTUAL</h4>
                                </div>
                            </div>
                            <div class="horizontal-group-special contenedor-horizontal" style="margin-top:3rem;">
                                <div class="form-group row" style="width:100%; margin-bottom:0;">
                                    <label for="disabledTextInput" class="col-form-label">ENFERMEDAD ACTUAL</label>
                                    <textarea id="txtEnfermedadActual" rows="5" style="width: 100%;" placeholder="Colocar la información recopilada en la anamnesis sobre el origen, la evolución cronológica y las características de todos y cada uno de los síntomas y/o signos del usuario, de los tratamientos efectuados, entre otros datos que puedan aportar y dar indicios de la patología actual."></textarea>
                                </div>
                                
                            </div>

                        </section>                        

                        <!--    Cuadros de CONSTANTES VITALES Y ANTROPOMETRÍA    -->    
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <!--    Cuadro de HÁBITOS TÓXICOS    -->
                            <div class="well-header" style="margin-top: 0; margin-bottom:3rem;">
                                <div>
                                     <h4 class="well-title">CONSTANTES VITALES Y ANTROPOMETRÍA</h4>
                                </div>
                            </div>
                            <!--  CONSTANTES VITALES Y ANTROPOMETRÍA    -->
                            <div class="contenedor-horizontal" style="margin-top:2rem; margin-bottom:2rem;">
                                <label for="disabledTextInput" class="col-form-label" style="margin-left:1.5rem;">CONSTANTES VITALES Y ANTROPOMETRÍA </label>
                                <div class="horizontal-group-special" style="margin-top:1rem;">
                                    <div class="cuadros-ingreso-normal4" style="">
                                        <label class="input-group-addon select-medidas titulo-cuadros">PRESIÓN ARTERIAL</label>
                                        <input id="txtConstPresionArterial" type="text" class="form-control" placeholder="mmHg" min="0" style="text-align:center;">
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

                        </div> <!-- fin well -->

                        <!--    Cuadro de Cuadro de EXAMEN FÍSICO REGIONAL   -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0; margin-bottom: 2rem;">
                                <div>
                                    <h4 class="well-title">EXAMEN FÍSICO REGIONAL</h4>
                                </div>
                            </div>
                            <!-- Cuadro Examen fisico regional -->                            
                            <div class="well contenedor-horizontal" style="background-color: #d8e4e9; margin:2rem 3rem 0 3rem;">
                                    
                                <div class="well-header" style="margin-left:2rem; padding-top:2rem;">
                                    <div>
                                         <label class="well-title">REGIONES</label>
                                    </div>
                                </div>
                                <div class="horizontal-cuadriculado-flow" style="margin: 0 2rem;">
                                   <!-- Contenedor del menú desplegable Piel -->
                                   <div class="cuadros-ingreso-normal dropdown-container">
                                    <div class="input-group-addon dropdown-button">Piel</div>        
                                        <!-- Lista de opciones con casillas de verificación -->
                                        <div class="dropdown-content">
                                            <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="pielA">
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Cicatrices</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="pielB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Tatuajes</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="pielC">
                                                   <label for="opcion3" style="margin-left: 0.5rem;"> Piel  y Faneras</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Párpados</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="ojosB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Conjuntivas</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="ojosC">
                                                   <label for="opcion3" style="margin-left: 0.5rem;"> Pupilas</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="ojosD">
                                                   <label for="opcion4" style="margin-left: 0.5rem;"> Córnea</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="ojosE">
                                                   <label for="opcion5" style="margin-left: 0.5rem;"> Motilidad</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> C. auditivo externo</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="oidoB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Pabellón</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="oidoC">
                                                   <label for="opcion3" style="margin-left: 0.5rem;"> Tímpanos</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Labios</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="oroB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Lengua</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="oroC">
                                                   <label for="opcion3" style="margin-left: 0.5rem;"> Faringe</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="oroD">
                                                   <label for="opcion4" style="margin-left: 0.5rem;"> Amígdalas</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="oroE">
                                                   <label for="opcion5" style="margin-left: 0.5rem;"> Dentadura</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Tabique</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="narizB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Cornetes</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="narizC">
                                                   <label for="opcion3" style="margin-left: 0.5rem;"> Mucosas</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="narizD">
                                                   <label for="opcion4" style="margin-left: 0.5rem;"> Senos paranasales</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Tiroides / masas</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="cuelloB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Movilidad</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Mamas</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="toraxB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Corazón</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="toraxC">
                                                   <label for="opcion3" style="margin-left: 0.5rem;">  Pulmones</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="ToraxD">
                                                   <label for="opcion4" style="margin-left: 0.5rem;">  Parrilla Costal</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Vísceras</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="abdomenB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Pared abdominal</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Flexibilidad</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="columnaB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Desviación</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="columnaC">
                                                   <label for="opcion3" style="margin-left: 0.5rem;"> Dolor</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Pelvis</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="pelvisB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Genitales</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Vascular</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="extremidadesB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Miembros superiores</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="extremidadesC">
                                                   <label for="opcion3" style="margin-left: 0.5rem;"> Miembros inferiores</label>
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
                                                   <label for="opcion1" style="margin-left: 0.5rem;"> Fuerza</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="neurologicoB">
                                                   <label for="opcion2" style="margin-left: 0.5rem;"> Sensibilidad</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="neurologicoC">
                                                   <label for="opcion3" style="margin-left: 0.5rem;"> Marcha</label>
                                               </div>
                                               <div class="dropdown-option">
                                                   <input type="checkbox" class="checkbox" id="neurologicoD">
                                                   <label for="opcion4" style="margin-left: 0.5rem;"> Reflejos</label>
                                               </div>
                                           </div>
                                       </div>
                                </div>

                                <div class="contenedor-horizontal" style="margin: 1rem 2rem 0 2rem;">
                                    <label for="disabledTextInput" class="col-form-label">Observaciones</label>
                                    <textarea id="txtExamFisicoObservacion" rows="5" style="width: 100%; resize: vertical;" value="" placeholder="En la sección de “observaciones”, detallar la patología encontrada."></textarea>
                                </div>
                            </div>
                        </div><!-- fin well -->

                        <!--    Cuadro de Cuadro de FACTORES DE RIESGOS DEL PUESTO DE TRABAJO ACTUAL    -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0; margin-bottom:4rem;">
                                <div>
                                    <h4 class="well-title">FACTORES DE RIESGOS DEL PUESTO DE TRABAJO ACTUAL</h4>
                                </div>
                            </div>
                            <!-- ═══════════════ CABECERAS ═══════════════ -->
                            <div class="horizontal-group-simple" style="margin:0 2rem;">
                                <div class="cuadros-ingreso-normal3">
                                    <label class="control-label text-center" style="width:100%; margin-left:0;">FÍSICO</label>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <label class="control-label text-center" style="width:100%; margin-left:0;">MECÁNICO</label>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <label class="control-label text-center" style="width:100%; margin-left:0;">QUÍMICO</label>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <label class="control-label text-center" style="width:100%; margin-left:0;">BIOLÓGICO</label>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <label class="control-label text-center" style="width:100%; margin-left:0;">ERGONÓMICO</label>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <label class="control-label text-center" style="width:100%; margin-left:0;">PSICOSOCIAL</label>
                                </div>
                                <%--<div class="cuadros-ingreso-normal3">
                                    <label class="control-label text-center" style="width:100%; margin-left:0;">MEDIDAS PREVENTIVAS</label>
                                </div>--%>
                            </div>

                            <!-- ═══════════════ FILA 1 ═══════════════ -->
                            <div class="horizontal-group-simple" style="margin:0 2rem;">
                                <label style="margin-right:0.5rem;">1.</label>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">                                    
                                    <fieldset class="input-group text-center">
                                        <select id="txtFisicoSelect1" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="TemperaturasAltas">Temperaturas altas</option>
                                            <option value="TemperaturasBajas">Temperaturas bajas</option>
                                            <option value="RadiacionIonizante">Radiación Ionizante</option>
                                            <option value="RadiaciónNoIonizante">Radiación No Ionizante</option>
                                            <option value="Ruido">Ruido</option>
                                            <option value="Vibracion">Vibración</option>
                                            <option value="Iluminacion">Iluminación</option>
                                            <option value="Ventilacion">Ventilación</option>
                                            <option value="FluidoElectrico">Fluido eléctrico</option>
                                            <option value="OtrosFisico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtMecanicoSelect1" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="FaltaSeñalizacion">Falta de señalización, aseo, desorden</option>
                                            <option value="AtrapamientoMaquinas">Atrapamiento entre máquinas</option>
                                            <option value="AtrapamientoSuperficies">Atrapamiento entre superficies</option>
                                            <option value="AtrapamientoObjetos">Atrapamiento entre objetos</option>
                                            <option value="CaidaObjetos">Caída de objetos</option>
                                            <option value="CaidasMismoNivel">Caídas al mismo nivel</option>
                                            <option value="CaidasDiferenteNivel">Caídas a diferente nivel</option>
                                            <option value="ContactoElectrico">Contacto eléctrico</option>
                                            <option value="ContactoSuperficiesTrabajos">Contacto con superficies de trabajos</option>
                                            <option value="ProyeccionPartículas">Proyección de partículas – fragmentos</option>
                                            <option value="ProyeccionFluidos">Proyección de fluidos</option>
                                            <option value="Pinchazos">Pinchazos</option>
                                            <option value="Cortes">Cortes</option>
                                            <option value="AtropellamientoVehículo">Atropellamientos por vehículos</option>
                                            <option value="ChoquesVehicular">Choques /colisión vehicular</option>
                                            <option value="OtrosMecanico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtQuimicoSelect1" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Solidos">Sólidos</option>
                                            <option value="Polvos">Polvos</option>
                                            <option value="Humos">Humos</option>
                                            <option value="liquidos">Líquidos</option>
                                            <option value="vapores">Vapores</option>
                                            <option value="Aerosoles">Aerosoles</option>
                                            <option value="Neblinas">Neblinas</option>
                                            <option value="Gaseosos">Gaseosos</option>
                                            <option value="OtrosQuimico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtBiologicoSelect1" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Virus">Virus</option>
                                            <option value="Hongos">Hongos</option>
                                            <option value="Bacterias">Bacterias</option>
                                            <option value="Parasitos">Parásitos</option>
                                            <option value="ExposicionVectores">Exposición a vectores</option>
                                            <option value="ExposicionAnimales">Exposición a animales selváticos</option>
                                            <option value="DiseñoInadecuado">Diseño Inadecuado del puesto</option>
                                            <option value="OtrosBiologico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtErgonomicoSelect1" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="ManejoCargas">Manejo manual de cargas</option>
                                            <option value="MovimientoRepetitivos">Movimiento repetitivos</option>
                                            <option value="PosturasForzadas">Posturas forzadas</option>
                                            <option value="TrabajosPVD">Trabajos con PVD</option>
                                            <option value="OtrosErgonomico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtPSicosocialSelect1" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="MonotoniaTrabajo">Monotonía del trabajo</option>
                                            <option value="SobrecargaLaboral">Sobrecarga laboral</option>
                                            <option value="MinuciosidadTarea">Minuciosidad de la tarea</option>
                                            <option value="AltaResponsabilidad">Alta responsabilidad</option>
                                            <option value="AutonomiaDecisiones">Autonomía en la toma de decisiones</option>
                                            <option value="SupervisionDeficiente">Supervisión y estilos de dirección deficiente</option>
                                            <option value="ConflictoRol">Conflicto de rol</option>
                                            <option value="FaltaClaridadFunciones">Falta de Claridad en las funciones</option>
                                            <option value="IncorrectaDistribuciónTrabajo">Incorrecta distribución del trabajo</option>
                                            <option value="TurnosRotativos">Turnos rotativos</option>
                                            <option value="RelacionesInterpersonales">Relaciones interpersonales</option>
                                            <option value="InestabilidadLaboral">Inestabilidad laboral</option>
                                            <option value="OtrosPSicosocial">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>

                            <!-- Medidas preventivas fila 1 -->
                            <div class="horizontal-group-simple" style="margin: 0 0.8rem 1.5rem 4rem;">
                                <label class="col-sm-3 control-label" style="font-size:11px; color:#64748b; margin-bottom:4px; display:block;">MEDIDAS PREVENTIVAS</label>
                                <div class="col-sm-9 horizontal-group-simple" style="gap:6px;">
                                    <input id="txtMedidadPreventivaA1" type="text" class="form-control select-medidas" placeholder="Medida 1" value="">
                                    <input id="txtMedidadPreventivaB1" type="text" class="form-control select-medidas" placeholder="Medida 2" value="">
                                    <input id="txtMedidadPreventivaC1" type="text" class="form-control select-medidas" placeholder="Medida 3" value="">
                                </div>
                            </div>

                            <div class="horizontal-group-start" id="otros1" style="display: none; margin:0 3rem;">
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosFisico1" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosMecanico1" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosQuimico1" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosBiologico1" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosErgonomico1" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosPSicosocial1" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                            </div>

                            <!-- ═══════════════ FILA 2 ═══════════════ -->
                            <div class="horizontal-group-simple" style="margin:0 2rem;">
                                <label style="margin-right:0.5rem;">2.</label>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtFisicoSelect2" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="TemperaturasAltas">Temperaturas altas</option>
                                            <option value="TemperaturasBajas">Temperaturas bajas</option>
                                            <option value="RadiacionIonizante">Radiación Ionizante</option>
                                            <option value="RadiaciónNoIonizante">Radiación No Ionizante</option>
                                            <option value="Ruido">Ruido</option>
                                            <option value="Vibracion">Vibración</option>
                                            <option value="Iluminacion">Iluminación</option>
                                            <option value="Ventilacion">Ventilación</option>
                                            <option value="FluidoElectrico">Fluido eléctrico</option>
                                            <option value="OtrosFisico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtMecanicoSelect2" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="FaltaSeñalizacion">Falta de señalización, aseo, desorden</option>
                                            <option value="AtrapamientoMaquinas">Atrapamiento entre máquinas</option>
                                            <option value="AtrapamientoSuperficies">Atrapamiento entre superficies</option>
                                            <option value="AtrapamientoObjetos">Atrapamiento entre objetos</option>
                                            <option value="CaidaObjetos">Caída de objetos</option>
                                            <option value="CaidasMismoNivel">Caídas al mismo nivel</option>
                                            <option value="CaidasDiferenteNivel">Caídas a diferente nivel</option>
                                            <option value="ContactoElectrico">Contacto eléctrico</option>
                                            <option value="ContactoSuperficiesTrabajos">Contacto con superficies de trabajos</option>
                                            <option value="ProyeccionPartículas">Proyección de partículas – fragmentos</option>
                                            <option value="ProyeccionFluidos">Proyección de fluidos</option>
                                            <option value="Pinchazos">Pinchazos</option>
                                            <option value="Cortes">Cortes</option>
                                            <option value="AtropellamientoVehículo">Atropellamientos por vehículos</option>
                                            <option value="ChoquesVehicular">Choques /colisión vehicular</option>
                                            <option value="OtrosMecanico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtQuimicoSelect2" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Solidos">Sólidos</option>
                                            <option value="Polvos">Polvos</option>
                                            <option value="Humos">Humos</option>
                                            <option value="liquidos">Líquidos</option>
                                            <option value="vapores">Vapores</option>
                                            <option value="Aerosoles">Aerosoles</option>
                                            <option value="Neblinas">Neblinas</option>
                                            <option value="Gaseosos">Gaseosos</option>
                                            <option value="OtrosQuimico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtBiologicoSelect2" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Virus">Virus</option>
                                            <option value="Hongos">Hongos</option>
                                            <option value="Bacterias">Bacterias</option>
                                            <option value="Parasitos">Parásitos</option>
                                            <option value="ExposicionVectores">Exposición a vectores</option>
                                            <option value="ExposicionAnimales">Exposición a animales selváticos</option>
                                            <option value="DiseñoInadecuado">Diseño Inadecuado del puesto</option>
                                            <option value="OtrosBiologico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtErgonomicoSelect2" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="ManejoCargas">Manejo manual de cargas</option>
                                            <option value="MovimientoRepetitivos">Movimiento repetitivos</option>
                                            <option value="PosturasForzadas">Posturas forzadas</option>
                                            <option value="TrabajosPVD">Trabajos con PVD</option>
                                            <option value="OtrosErgonomico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtPSicosocialSelect2" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="MonotoniaTrabajo">Monotonía del trabajo</option>
                                            <option value="SobrecargaLaboral">Sobrecarga laboral</option>
                                            <option value="MinuciosidadTarea">Minuciosidad de la tarea</option>
                                            <option value="AltaResponsabilidad">Alta responsabilidad</option>
                                            <option value="AutonomiaDecisiones">Autonomía en la toma de decisiones</option>
                                            <option value="SupervisionDeficiente">Supervisión y estilos de dirección deficiente</option>
                                            <option value="ConflictoRol">Conflicto de rol</option>
                                            <option value="FaltaClaridadFunciones">Falta de Claridad en las funciones</option>
                                            <option value="IncorrectaDistribuciónTrabajo">Incorrecta distribución del trabajo</option>
                                            <option value="TurnosRotativos">Turnos rotativos</option>
                                            <option value="RelacionesInterpersonales">Relaciones interpersonales</option>
                                            <option value="InestabilidadLaboral">Inestabilidad laboral</option>
                                            <option value="OtrosPSicosocial">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>
                            <!-- Medidas preventivas fila 2 -->
                            <div class="horizontal-group-simple" style="margin: 0 0.8rem 1.5rem 4rem;">
                                <label class="col-sm-3 control-label" style="font-size:11px; color:#64748b; margin-bottom:4px; display:block;">MEDIDAS PREVENTIVAS</label>
                                <div class="col-sm-9 horizontal-group-simple" style="gap:6px;">
                                    <input id="txtMedidadPreventivaA2" type="text" class="form-control select-medidas" placeholder="Medida 1" value="">
                                    <input id="txtMedidadPreventivaB2" type="text" class="form-control select-medidas" placeholder="Medida 2" value="">
                                    <input id="txtMedidadPreventivaC2" type="text" class="form-control select-medidas" placeholder="Medida 3" value="">
                                </div>
                            </div>

                            <div class="horizontal-group-start" id="otros2" style="display: none; margin:0 3rem;">
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosFisico2" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosMecanico2" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosQuimico2" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosBiologico2" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosErgonomico2" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosPSicosocial2" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                            </div>

                            <!-- ═══════════════ FILA 3 ═══════════════ -->
                            <div class="horizontal-group-simple" style="margin:0 2rem;">
                                <label style="margin-right:0.5rem;">3.</label>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtFisicoSelect3" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="TemperaturasAltas">Temperaturas altas</option>
                                            <option value="TemperaturasBajas">Temperaturas bajas</option>
                                            <option value="RadiacionIonizante">Radiación Ionizante</option>
                                            <option value="RadiaciónNoIonizante">Radiación No Ionizante</option>
                                            <option value="Ruido">Ruido</option>
                                            <option value="Vibracion">Vibración</option>
                                            <option value="Iluminacion">Iluminación</option>
                                            <option value="Ventilacion">Ventilación</option>
                                            <option value="FluidoElectrico">Fluido eléctrico</option>
                                            <option value="OtrosFisico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtMecanicoSelect3" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="FaltaSeñalizacion">Falta de señalización, aseo, desorden</option>
                                            <option value="AtrapamientoMaquinas">Atrapamiento entre máquinas</option>
                                            <option value="AtrapamientoSuperficies">Atrapamiento entre superficies</option>
                                            <option value="AtrapamientoObjetos">Atrapamiento entre objetos</option>
                                            <option value="CaidaObjetos">Caída de objetos</option>
                                            <option value="CaidasMismoNivel">Caídas al mismo nivel</option>
                                            <option value="CaidasDiferenteNivel">Caídas a diferente nivel</option>
                                            <option value="ContactoElectrico">Contacto eléctrico</option>
                                            <option value="ContactoSuperficiesTrabajos">Contacto con superficies de trabajos</option>
                                            <option value="ProyeccionPartículas">Proyección de partículas – fragmentos</option>
                                            <option value="ProyeccionFluidos">Proyección de fluidos</option>
                                            <option value="Pinchazos">Pinchazos</option>
                                            <option value="Cortes">Cortes</option>
                                            <option value="AtropellamientoVehículo">Atropellamientos por vehículos</option>
                                            <option value="ChoquesVehicular">Choques /colisión vehicular</option>
                                            <option value="OtrosMecanico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtQuimicoSelect3" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Solidos">Sólidos</option>
                                            <option value="Polvos">Polvos</option>
                                            <option value="Humos">Humos</option>
                                            <option value="liquidos">Líquidos</option>
                                            <option value="vapores">Vapores</option>
                                            <option value="Aerosoles">Aerosoles</option>
                                            <option value="Neblinas">Neblinas</option>
                                            <option value="Gaseosos">Gaseosos</option>
                                            <option value="OtrosQuimico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtBiologicoSelect3" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Virus">Virus</option>
                                            <option value="Hongos">Hongos</option>
                                            <option value="Bacterias">Bacterias</option>
                                            <option value="Parasitos">Parásitos</option>
                                            <option value="ExposicionVectores">Exposición a vectores</option>
                                            <option value="ExposicionAnimales">Exposición a animales selváticos</option>
                                            <option value="DiseñoInadecuado">Diseño Inadecuado del puesto</option>
                                            <option value="OtrosBiologico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtErgonomicoSelect3" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="ManejoCargas">Manejo manual de cargas</option>
                                            <option value="MovimientoRepetitivos">Movimiento repetitivos</option>
                                            <option value="PosturasForzadas">Posturas forzadas</option>
                                            <option value="TrabajosPVD">Trabajos con PVD</option>
                                            <option value="OtrosErgonomico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtPSicosocialSelect3" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="MonotoniaTrabajo">Monotonía del trabajo</option>
                                            <option value="SobrecargaLaboral">Sobrecarga laboral</option>
                                            <option value="MinuciosidadTarea">Minuciosidad de la tarea</option>
                                            <option value="AltaResponsabilidad">Alta responsabilidad</option>
                                            <option value="AutonomiaDecisiones">Autonomía en la toma de decisiones</option>
                                            <option value="SupervisionDeficiente">Supervisión y estilos de dirección deficiente</option>
                                            <option value="ConflictoRol">Conflicto de rol</option>
                                            <option value="FaltaClaridadFunciones">Falta de Claridad en las funciones</option>
                                            <option value="IncorrectaDistribuciónTrabajo">Incorrecta distribución del trabajo</option>
                                            <option value="TurnosRotativos">Turnos rotativos</option>
                                            <option value="RelacionesInterpersonales">Relaciones interpersonales</option>
                                            <option value="InestabilidadLaboral">Inestabilidad laboral</option>
                                            <option value="OtrosPSicosocial">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>
                            <!-- Medidas preventivas fila 3 -->
                            <div class="horizontal-group-simple" style="margin: 0 0.8rem 1.5rem 4rem;">
                                <label class="col-sm-3 control-label" style="font-size:11px; color:#64748b; margin-bottom:4px; display:block;">MEDIDAS PREVENTIVAS</label>
                                <div class="col-sm-9 horizontal-group-simple" style="gap:6px;">
                                    <input id="txtMedidadPreventivaA3" type="text" class="form-control select-medidas" placeholder="Medida 1" value="">
                                    <input id="txtMedidadPreventivaB3" type="text" class="form-control select-medidas" placeholder="Medida 2" value="">
                                    <input id="txtMedidadPreventivaC3" type="text" class="form-control select-medidas" placeholder="Medida 3" value="">
                                </div>
                            </div>

                            <div class="horizontal-group-start" id="otros3" style="display: none; margin:0 3rem;">
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosFisico3" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosMecanico3" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosQuimico3" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosBiologico3" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosErgonomico3" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosPSicosocial3" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                            </div>

                            <!-- ═══════════════ FILA 4 ═══════════════ -->
                            <div class="horizontal-group-simple" style="margin:0 2rem;">
                                <label style="margin-right:0.5rem;">4.</label>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtFisicoSelect4" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="TemperaturasAltas">Temperaturas altas</option>
                                            <option value="TemperaturasBajas">Temperaturas bajas</option>
                                            <option value="RadiacionIonizante">Radiación Ionizante</option>
                                            <option value="RadiaciónNoIonizante">Radiación No Ionizante</option>
                                            <option value="Ruido">Ruido</option>
                                            <option value="Vibracion">Vibración</option>
                                            <option value="Iluminacion">Iluminación</option>
                                            <option value="Ventilacion">Ventilación</option>
                                            <option value="FluidoElectrico">Fluido eléctrico</option>
                                            <option value="OtrosFisico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtMecanicoSelect4" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="FaltaSeñalizacion">Falta de señalización, aseo, desorden</option>
                                            <option value="AtrapamientoMaquinas">Atrapamiento entre máquinas</option>
                                            <option value="AtrapamientoSuperficies">Atrapamiento entre superficies</option>
                                            <option value="AtrapamientoObjetos">Atrapamiento entre objetos</option>
                                            <option value="CaidaObjetos">Caída de objetos</option>
                                            <option value="CaidasMismoNivel">Caídas al mismo nivel</option>
                                            <option value="CaidasDiferenteNivel">Caídas a diferente nivel</option>
                                            <option value="ContactoElectrico">Contacto eléctrico</option>
                                            <option value="ContactoSuperficiesTrabajos">Contacto con superficies de trabajos</option>
                                            <option value="ProyeccionPartículas">Proyección de partículas – fragmentos</option>
                                            <option value="ProyeccionFluidos">Proyección de fluidos</option>
                                            <option value="Pinchazos">Pinchazos</option>
                                            <option value="Cortes">Cortes</option>
                                            <option value="AtropellamientoVehículo">Atropellamientos por vehículos</option>
                                            <option value="ChoquesVehicular">Choques /colisión vehicular</option>
                                            <option value="OtrosMecanico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtQuimicoSelect4" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Solidos">Sólidos</option>
                                            <option value="Polvos">Polvos</option>
                                            <option value="Humos">Humos</option>
                                            <option value="liquidos">Líquidos</option>
                                            <option value="vapores">Vapores</option>
                                            <option value="Aerosoles">Aerosoles</option>
                                            <option value="Neblinas">Neblinas</option>
                                            <option value="Gaseosos">Gaseosos</option>
                                            <option value="OtrosQuimico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtBiologicoSelect4" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Virus">Virus</option>
                                            <option value="Hongos">Hongos</option>
                                            <option value="Bacterias">Bacterias</option>
                                            <option value="Parasitos">Parásitos</option>
                                            <option value="ExposicionVectores">Exposición a vectores</option>
                                            <option value="ExposicionAnimales">Exposición a animales selváticos</option>
                                            <option value="DiseñoInadecuado">Diseño Inadecuado del puesto</option>
                                            <option value="OtrosBiologico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtErgonomicoSelect4" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="ManejoCargas">Manejo manual de cargas</option>
                                            <option value="MovimientoRepetitivos">Movimiento repetitivos</option>
                                            <option value="PosturasForzadas">Posturas forzadas</option>
                                            <option value="TrabajosPVD">Trabajos con PVD</option>
                                            <option value="OtrosErgonomico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtPSicosocialSelect4" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="MonotoniaTrabajo">Monotonía del trabajo</option>
                                            <option value="SobrecargaLaboral">Sobrecarga laboral</option>
                                            <option value="MinuciosidadTarea">Minuciosidad de la tarea</option>
                                            <option value="AltaResponsabilidad">Alta responsabilidad</option>
                                            <option value="AutonomiaDecisiones">Autonomía en la toma de decisiones</option>
                                            <option value="SupervisionDeficiente">Supervisión y estilos de dirección deficiente</option>
                                            <option value="ConflictoRol">Conflicto de rol</option>
                                            <option value="FaltaClaridadFunciones">Falta de Claridad en las funciones</option>
                                            <option value="IncorrectaDistribuciónTrabajo">Incorrecta distribución del trabajo</option>
                                            <option value="TurnosRotativos">Turnos rotativos</option>
                                            <option value="RelacionesInterpersonales">Relaciones interpersonales</option>
                                            <option value="InestabilidadLaboral">Inestabilidad laboral</option>
                                            <option value="OtrosPSicosocial">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>
                            <!-- Medidas preventivas fila 4 -->
                            <div class="horizontal-group-simple" style="margin: 0 0.8rem 1.5rem 4rem;">
                                <label class="col-sm-3 control-label" style="font-size:11px; color:#64748b; margin-bottom:4px; display:block;">MEDIDAS PREVENTIVAS</label>
                                <div class="col-sm-9 horizontal-group-simple" style="gap:6px;">
                                    <input id="txtMedidadPreventivaA4" type="text" class="form-control select-medidas" placeholder="Medida 1" value="">
                                    <input id="txtMedidadPreventivaB4" type="text" class="form-control select-medidas" placeholder="Medida 2" value="">
                                    <input id="txtMedidadPreventivaC4" type="text" class="form-control select-medidas" placeholder="Medida 3" value="">
                                </div>
                            </div>

                            <div class="horizontal-group-start" id="otros7" style="display: none; margin:0 3rem;">
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosFisico4" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosMecanico4" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosQuimico4" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosBiologico4" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosErgonomico4" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosPSicosocial4" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                            </div>

                            <!-- ═══════════════ FILA 5 ═══════════════ -->
                            <div class="horizontal-group-simple" style="margin:0 2rem;">
                                <label style="margin-right:0.5rem;">5.</label>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtFisicoSelect5" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="TemperaturasAltas">Temperaturas altas</option>
                                            <option value="TemperaturasBajas">Temperaturas bajas</option>
                                            <option value="RadiacionIonizante">Radiación Ionizante</option>
                                            <option value="RadiaciónNoIonizante">Radiación No Ionizante</option>
                                            <option value="Ruido">Ruido</option>
                                            <option value="Vibracion">Vibración</option>
                                            <option value="Iluminacion">Iluminación</option>
                                            <option value="Ventilacion">Ventilación</option>
                                            <option value="FluidoElectrico">Fluido eléctrico</option>
                                            <option value="OtrosFisico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtMecanicoSelect5" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="FaltaSeñalizacion">Falta de señalización, aseo, desorden</option>
                                            <option value="AtrapamientoMaquinas">Atrapamiento entre máquinas</option>
                                            <option value="AtrapamientoSuperficies">Atrapamiento entre superficies</option>
                                            <option value="AtrapamientoObjetos">Atrapamiento entre objetos</option>
                                            <option value="CaidaObjetos">Caída de objetos</option>
                                            <option value="CaidasMismoNivel">Caídas al mismo nivel</option>
                                            <option value="CaidasDiferenteNivel">Caídas a diferente nivel</option>
                                            <option value="ContactoElectrico">Contacto eléctrico</option>
                                            <option value="ContactoSuperficiesTrabajos">Contacto con superficies de trabajos</option>
                                            <option value="ProyeccionPartículas">Proyección de partículas – fragmentos</option>
                                            <option value="ProyeccionFluidos">Proyección de fluidos</option>
                                            <option value="Pinchazos">Pinchazos</option>
                                            <option value="Cortes">Cortes</option>
                                            <option value="AtropellamientoVehículo">Atropellamientos por vehículos</option>
                                            <option value="ChoquesVehicular">Choques /colisión vehicular</option>
                                            <option value="OtrosMecanico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtQuimicoSelect5" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Solidos">Sólidos</option>
                                            <option value="Polvos">Polvos</option>
                                            <option value="Humos">Humos</option>
                                            <option value="liquidos">Líquidos</option>
                                            <option value="vapores">Vapores</option>
                                            <option value="Aerosoles">Aerosoles</option>
                                            <option value="Neblinas">Neblinas</option>
                                            <option value="Gaseosos">Gaseosos</option>
                                            <option value="OtrosQuimico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtBiologicoSelect5" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Virus">Virus</option>
                                            <option value="Hongos">Hongos</option>
                                            <option value="Bacterias">Bacterias</option>
                                            <option value="Parasitos">Parásitos</option>
                                            <option value="ExposicionVectores">Exposición a vectores</option>
                                            <option value="ExposicionAnimales">Exposición a animales selváticos</option>
                                            <option value="DiseñoInadecuado">Diseño Inadecuado del puesto</option>
                                            <option value="OtrosBiologico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtErgonomicoSelect5" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="ManejoCargas">Manejo manual de cargas</option>
                                            <option value="MovimientoRepetitivos">Movimiento repetitivos</option>
                                            <option value="PosturasForzadas">Posturas forzadas</option>
                                            <option value="TrabajosPVD">Trabajos con PVD</option>
                                            <option value="OtrosErgonomico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtPSicosocialSelect5" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="MonotoniaTrabajo">Monotonía del trabajo</option>
                                            <option value="SobrecargaLaboral">Sobrecarga laboral</option>
                                            <option value="MinuciosidadTarea">Minuciosidad de la tarea</option>
                                            <option value="AltaResponsabilidad">Alta responsabilidad</option>
                                            <option value="AutonomiaDecisiones">Autonomía en la toma de decisiones</option>
                                            <option value="SupervisionDeficiente">Supervisión y estilos de dirección deficiente</option>
                                            <option value="ConflictoRol">Conflicto de rol</option>
                                            <option value="FaltaClaridadFunciones">Falta de Claridad en las funciones</option>
                                            <option value="IncorrectaDistribuciónTrabajo">Incorrecta distribución del trabajo</option>
                                            <option value="TurnosRotativos">Turnos rotativos</option>
                                            <option value="RelacionesInterpersonales">Relaciones interpersonales</option>
                                            <option value="InestabilidadLaboral">Inestabilidad laboral</option>
                                            <option value="OtrosPSicosocial">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>
                            <!-- Medidas preventivas fila 5 -->
                            <div class="horizontal-group-simple" style="margin: 0 0.8rem 1.5rem 4rem;">
                                <label class="col-sm-3 control-label" style="font-size:11px; color:#64748b; margin-bottom:4px; display:block;">MEDIDAS PREVENTIVAS</label>
                                <div class="col-sm-9 horizontal-group-simple" style="gap:6px;">
                                    <input id="txtMedidadPreventivaA5" type="text" class="form-control select-medidas" placeholder="Medida 1" value="">
                                    <input id="txtMedidadPreventivaB5" type="text" class="form-control select-medidas" placeholder="Medida 2" value="">
                                    <input id="txtMedidadPreventivaC5" type="text" class="form-control select-medidas" placeholder="Medida 3" value="">
                                </div>
                            </div>

                            <div class="horizontal-group-start" id="otros8" style="display: none; margin:0 3rem;">
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosFisico5" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosMecanico5" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosQuimico5" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosBiologico5" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosErgonomico5" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosPSicosocial5" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                            </div>

                            <!-- ═══════════════ FILA 6 ═══════════════ -->
                            <div class="horizontal-group-simple" style="margin:0 2rem;">
                                <label style="margin-right:0.5rem;">6.</label>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtFisicoSelect6" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="TemperaturasAltas">Temperaturas altas</option>
                                            <option value="TemperaturasBajas">Temperaturas bajas</option>
                                            <option value="RadiacionIonizante">Radiación Ionizante</option>
                                            <option value="RadiaciónNoIonizante">Radiación No Ionizante</option>
                                            <option value="Ruido">Ruido</option>
                                            <option value="Vibracion">Vibración</option>
                                            <option value="Iluminacion">Iluminación</option>
                                            <option value="Ventilacion">Ventilación</option>
                                            <option value="FluidoElectrico">Fluido eléctrico</option>
                                            <option value="OtrosFisico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtMecanicoSelect6" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="FaltaSeñalizacion">Falta de señalización, aseo, desorden</option>
                                            <option value="AtrapamientoMaquinas">Atrapamiento entre máquinas</option>
                                            <option value="AtrapamientoSuperficies">Atrapamiento entre superficies</option>
                                            <option value="AtrapamientoObjetos">Atrapamiento entre objetos</option>
                                            <option value="CaidaObjetos">Caída de objetos</option>
                                            <option value="CaidasMismoNivel">Caídas al mismo nivel</option>
                                            <option value="CaidasDiferenteNivel">Caídas a diferente nivel</option>
                                            <option value="ContactoElectrico">Contacto eléctrico</option>
                                            <option value="ContactoSuperficiesTrabajos">Contacto con superficies de trabajos</option>
                                            <option value="ProyeccionPartículas">Proyección de partículas – fragmentos</option>
                                            <option value="ProyeccionFluidos">Proyección de fluidos</option>
                                            <option value="Pinchazos">Pinchazos</option>
                                            <option value="Cortes">Cortes</option>
                                            <option value="AtropellamientoVehículo">Atropellamientos por vehículos</option>
                                            <option value="ChoquesVehicular">Choques /colisión vehicular</option>
                                            <option value="OtrosMecanico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtQuimicoSelect6" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Solidos">Sólidos</option>
                                            <option value="Polvos">Polvos</option>
                                            <option value="Humos">Humos</option>
                                            <option value="liquidos">Líquidos</option>
                                            <option value="vapores">Vapores</option>
                                            <option value="Aerosoles">Aerosoles</option>
                                            <option value="Neblinas">Neblinas</option>
                                            <option value="Gaseosos">Gaseosos</option>
                                            <option value="OtrosQuimico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtBiologicoSelect6" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Virus">Virus</option>
                                            <option value="Hongos">Hongos</option>
                                            <option value="Bacterias">Bacterias</option>
                                            <option value="Parasitos">Parásitos</option>
                                            <option value="ExposicionVectores">Exposición a vectores</option>
                                            <option value="ExposicionAnimales">Exposición a animales selváticos</option>
                                            <option value="DiseñoInadecuado">Diseño Inadecuado del puesto</option>
                                            <option value="OtrosBiologico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtErgonomicoSelect6" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="ManejoCargas">Manejo manual de cargas</option>
                                            <option value="MovimientoRepetitivos">Movimiento repetitivos</option>
                                            <option value="PosturasForzadas">Posturas forzadas</option>
                                            <option value="TrabajosPVD">Trabajos con PVD</option>
                                            <option value="OtrosErgonomico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtPSicosocialSelect6" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="MonotoniaTrabajo">Monotonía del trabajo</option>
                                            <option value="SobrecargaLaboral">Sobrecarga laboral</option>
                                            <option value="MinuciosidadTarea">Minuciosidad de la tarea</option>
                                            <option value="AltaResponsabilidad">Alta responsabilidad</option>
                                            <option value="AutonomiaDecisiones">Autonomía en la toma de decisiones</option>
                                            <option value="SupervisionDeficiente">Supervisión y estilos de dirección deficiente</option>
                                            <option value="ConflictoRol">Conflicto de rol</option>
                                            <option value="FaltaClaridadFunciones">Falta de Claridad en las funciones</option>
                                            <option value="IncorrectaDistribuciónTrabajo">Incorrecta distribución del trabajo</option>
                                            <option value="TurnosRotativos">Turnos rotativos</option>
                                            <option value="RelacionesInterpersonales">Relaciones interpersonales</option>
                                            <option value="InestabilidadLaboral">Inestabilidad laboral</option>
                                            <option value="OtrosPSicosocial">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>
                            <!-- Medidas preventivas fila 6 -->
                            <div class="horizontal-group-simple" style="margin: 0 0.8rem 1.5rem 4rem;">
                                <label class="col-sm-3 control-label" style="font-size:11px; color:#64748b; margin-bottom:4px; display:block;">MEDIDAS PREVENTIVAS</label>
                                <div class="col-sm-9 horizontal-group-simple" style="gap:6px;">
                                    <input id="txtMedidadPreventivaA6" type="text" class="form-control select-medidas" placeholder="Medida 1" value="">
                                    <input id="txtMedidadPreventivaB6" type="text" class="form-control select-medidas" placeholder="Medida 2" value="">
                                    <input id="txtMedidadPreventivaC6" type="text" class="form-control select-medidas" placeholder="Medida 3" value="">
                                </div>
                            </div>

                            <div class="horizontal-group-start" id="otros9" style="display: none; margin:0 3rem;">
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosFisico6" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosMecanico6" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosQuimico6" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosBiologico6" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosErgonomico6" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosPSicosocial6" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                            </div>
                            <br />

                            <!-- ═══════════════ FILA 7 ═══════════════ -->
                            <div class="horizontal-group-simple" style="margin:0 2rem;">
                                <label style="margin-right:0.5rem;">7.</label>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtFisicoSelect7" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="TemperaturasAltas">Temperaturas altas</option>
                                            <option value="TemperaturasBajas">Temperaturas bajas</option>
                                            <option value="RadiacionIonizante">Radiación Ionizante</option>
                                            <option value="RadiaciónNoIonizante">Radiación No Ionizante</option>
                                            <option value="Ruido">Ruido</option>
                                            <option value="Vibracion">Vibración</option>
                                            <option value="Iluminacion">Iluminación</option>
                                            <option value="Ventilacion">Ventilación</option>
                                            <option value="FluidoElectrico">Fluido eléctrico</option>
                                            <option value="OtrosFisico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtMecanicoSelect7" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="FaltaSeñalizacion">Falta de señalización, aseo, desorden</option>
                                            <option value="AtrapamientoMaquinas">Atrapamiento entre máquinas</option>
                                            <option value="AtrapamientoSuperficies">Atrapamiento entre superficies</option>
                                            <option value="AtrapamientoObjetos">Atrapamiento entre objetos</option>
                                            <option value="CaidaObjetos">Caída de objetos</option>
                                            <option value="CaidasMismoNivel">Caídas al mismo nivel</option>
                                            <option value="CaidasDiferenteNivel">Caídas a diferente nivel</option>
                                            <option value="ContactoElectrico">Contacto eléctrico</option>
                                            <option value="ContactoSuperficiesTrabajos">Contacto con superficies de trabajos</option>
                                            <option value="ProyeccionPartículas">Proyección de partículas – fragmentos</option>
                                            <option value="ProyeccionFluidos">Proyección de fluidos</option>
                                            <option value="Pinchazos">Pinchazos</option>
                                            <option value="Cortes">Cortes</option>
                                            <option value="AtropellamientoVehículo">Atropellamientos por vehículos</option>
                                            <option value="ChoquesVehicular">Choques /colisión vehicular</option>
                                            <option value="OtrosMecanico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtQuimicoSelect7" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Solidos">Sólidos</option>
                                            <option value="Polvos">Polvos</option>
                                            <option value="Humos">Humos</option>
                                            <option value="liquidos">Líquidos</option>
                                            <option value="vapores">Vapores</option>
                                            <option value="Aerosoles">Aerosoles</option>
                                            <option value="Neblinas">Neblinas</option>
                                            <option value="Gaseosos">Gaseosos</option>
                                            <option value="OtrosQuimico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtBiologicoSelect7" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="Virus">Virus</option>
                                            <option value="Hongos">Hongos</option>
                                            <option value="Bacterias">Bacterias</option>
                                            <option value="Parasitos">Parásitos</option>
                                            <option value="ExposicionVectores">Exposición a vectores</option>
                                            <option value="ExposicionAnimales">Exposición a animales selváticos</option>
                                            <option value="DiseñoInadecuado">Diseño Inadecuado del puesto</option>
                                            <option value="OtrosBiologico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtErgonomicoSelect7" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="ManejoCargas">Manejo manual de cargas</option>
                                            <option value="MovimientoRepetitivos">Movimiento repetitivos</option>
                                            <option value="PosturasForzadas">Posturas forzadas</option>
                                            <option value="TrabajosPVD">Trabajos con PVD</option>
                                            <option value="OtrosErgonomico">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal3" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="txtPSicosocialSelect7" class="form-control input-medium select-temas">
                                            <option value="" selected>Seleccionar</option>
                                            <option value="MonotoniaTrabajo">Monotonía del trabajo</option>
                                            <option value="SobrecargaLaboral">Sobrecarga laboral</option>
                                            <option value="MinuciosidadTarea">Minuciosidad de la tarea</option>
                                            <option value="AltaResponsabilidad">Alta responsabilidad</option>
                                            <option value="AutonomiaDecisiones">Autonomía en la toma de decisiones</option>
                                            <option value="SupervisionDeficiente">Supervisión y estilos de dirección deficiente</option>
                                            <option value="ConflictoRol">Conflicto de rol</option>
                                            <option value="FaltaClaridadFunciones">Falta de Claridad en las funciones</option>
                                            <option value="IncorrectaDistribuciónTrabajo">Incorrecta distribución del trabajo</option>
                                            <option value="TurnosRotativos">Turnos rotativos</option>
                                            <option value="RelacionesInterpersonales">Relaciones interpersonales</option>
                                            <option value="InestabilidadLaboral">Inestabilidad laboral</option>
                                            <option value="OtrosPSicosocial">Otros</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>
                            <!-- Medidas preventivas fila 7 -->
                            <div class="horizontal-group-simple" style="margin: 0 0.8rem 1.5rem 4rem;">
                                <label class="col-sm-3 control-label" style="font-size:11px; color:#64748b; margin-bottom:4px; display:block;">MEDIDAS PREVENTIVAS</label>
                                <div class="col-sm-9 horizontal-group-simple" style="gap:6px;">
                                    <input id="txtMedidadPreventivaA7" type="text" class="form-control select-medidas" placeholder="Medida 1" value="">
                                    <input id="txtMedidadPreventivaB7" type="text" class="form-control select-medidas" placeholder="Medida 2" value="">
                                    <input id="txtMedidadPreventivaC7" type="text" class="form-control select-medidas" placeholder="Medida 3" value="">
                                </div>
                            </div>

                            <div class="horizontal-group-start" id="otros10" style="display: none; margin:0 3rem;">
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosFisico7" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosMecanico7" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txtOtrosQuimico7" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosBiologico7" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosErgonomico7" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                                <div class="cuadros-ingreso-normal3"><div class="grupo-input text-center" style="margin-bottom: 10px;"><input id="txOtrosPSicosocial7" type="text" class="form-control select-otros" placeholder="Otros" value=""></div></div>
                            </div>

                            
                        </div> <!-- fin well -->

                        <!--    Cuadros de Actividad laboral   -->  
                        <div id="actividadLaboral" class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">

    <div class="well-header" style="margin-top: 0; margin-bottom:1.5rem;">
        <h4 class="color-texto-primario" style="margin: 0.5rem auto 1rem auto; display:block;">
            Actividad Laboral / Incidentes / Accidentes / Enfermedades Ocupacionales
        </h4>
    </div>

    <div id="actLabContainer">

        <%-- ══ REGISTRO 1 ══ --%>
        <div class="act-registro cuadros-blancos-redondeados" style="margin-bottom:12px;">

            <%-- Tabla superior --%>
            <table style="width:100%; table-layout:fixed; border-collapse:collapse;">
                <thead>
                    <tr>
                        <th style="width:30%; background:#1a9ea8; color:#fff; font-size:10px; font-weight:700; text-transform:uppercase; text-align:center; padding:8px 4px;">Centro de Trabajo</th>
                        <th style="width:30%; background:#17b8c4; color:#fff; font-size:10px; font-weight:700; text-transform:uppercase; text-align:center; padding:8px 4px;">Actividad Desempeñada</th>
                        <th style="width:20%; background:#14c4c4; color:#fff; font-size:10px; font-weight:700; text-transform:uppercase; text-align:center; padding:8px 4px;">Trabajo</th>
                        <th style="width:20%; background:#0eb8a8; color:#fff; font-size:10px; font-weight:700; text-transform:uppercase; text-align:center; padding:8px 4px;">Tiempo de Trabajo</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td style="padding:4px; border:1px solid #e2e8f0;">
                            <input id="txtCentroTrabajo1" type="text" class="cuadros-fondo-input-ingreso" placeholder="Centro de trabajo" style="width:100%;">
                        </td>
                        <td style="padding:4px; border:1px solid #e2e8f0;">
                            <input id="txtActividadDesempeñaba1" type="text" class="cuadros-fondo-input-ingreso" placeholder="Actividad" style="width:100%;">
                        </td>
                        <td style="padding:4px; border:1px solid #e2e8f0;">
                            <select id="txtActividadDesempeñada1" class="cuadros-fondo-input-ingreso select-placeholder" style="width:100%;">
                                <option value="" selected>Selec.</option>
                                <option value="Anterior">Anterior</option>
                                <option value="Actual">Actual</option>
                            </select>
                        </td>
                        <td style="padding:4px; border:1px solid #e2e8f0;">
                            <input id="txtTiemporTrabajo1" type="text" class="cuadros-fondo-input-ingreso" placeholder="Tiempo" style="width:100%;">
                        </td>
                    </tr>
                </tbody>
            </table>

            <%-- Tabla inferior --%>
            <table style="width:100%; table-layout:fixed; border-collapse:collapse; margin-top:0.5rem;">
                <thead>
                    <tr>
                        <th style="width:25%; background:#0ca898; color:#fff; font-size:10px; font-weight:700; text-transform:uppercase; text-align:center; padding:8px 4px;">Acc. / Enf. Profesionales</th>
                        <th style="width:15%; background:#0a9888; color:#fff; font-size:10px; font-weight:700; text-transform:uppercase; text-align:center; padding:8px 4px;">IESS</th>
                        <th style="width:15%; background:#099080; color:#fff; font-size:10px; font-weight:700; text-transform:uppercase; text-align:center; padding:8px 4px;">Fecha</th>
                        <th style="width:22.5%; background:#088878; color:#fff; font-size:10px; font-weight:700; text-transform:uppercase; text-align:center; padding:8px 4px;">Especificar</th>
                        <th style="width:22.5%; background:#077870; color:#fff; font-size:10px; font-weight:700; text-transform:uppercase; text-align:center; padding:8px 4px;">Observación</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td style="padding:4px; border:1px solid #e2e8f0;">
                            <select id="selectAccEnfTrabajo1" class="cuadros-fondo-input-ingreso select-placeholder" style="width:100%;">
                                <option value="" selected>Selec.</option>
                                <option value="Incidente">Incidente</option>
                                <option value="Accidente">Accidente</option>
                                <option value="Enfermedad Profesional">Enf. Profesional</option>
                            </select>
                        </td>
                        <td style="padding:4px; border:1px solid #e2e8f0; text-align:center;">
                            <div style="display:flex; gap:4px; justify-content:center;">
                                <button style="padding:6px 18px; border-radius:6px;" type="button" id="AccTrab-si1" class="btn btn-custom btn-sm" onclick="toggleActLab('AccTrab-si1','AccTrab-no1')">Sí</button>
                                <button style="padding:6px 18px; border-radius:6px;" type="button" id="AccTrab-no1" class="btn btn-custom btn-sm" onclick="toggleActLab('AccTrab-no1','AccTrab-si1')">No</button>
                            </div>
                        </td>
                        <td style="border:1px solid #e2e8f0;">
                            <input type="date" class="form-control" id="fechaAccTrabLab1" style="width:100%;">
                        </td>
                        <td style="padding:4px; border:1px solid #e2e8f0;">
                            <input id="txtCualActividadLab1" type="text" class="cuadros-fondo-input-ingreso" placeholder="Especificar" style="width:100%;">
                        </td>
                        <td style="padding:4px; border:1px solid #e2e8f0;">
                            <input id="txtObservacionActividadLab1" type="text" class="cuadros-fondo-input-ingreso" placeholder="Observación" style="width:100%;">
                        </td>
                    </tr>
                </tbody>
            </table>

        </div>
        <%-- ══ FIN REGISTRO 1 ══ --%>

    </div>

    <div style="margin-top:0.5rem;">
        <button class="btn btn-danger" type="button" onclick="agregarActividadLaboral()">+ Agregar</button>
    </div>
</div>

                        <!--    Cuadros de estilo de vida    -->  
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0; margin-bottom:2rem;">
                                <div>
                                    <h4 class="well-title"> ACTIVIDADES EXTRA LABORALES </h4>
                                </div>
                            </div> 

                            <div class="col" style="margin-left:0; ">                                
                                <div class="horizontal-group-simple">                                                                                                
                                    <div class="col-sm-9" style="padding:0;">   
                                        <label for="disabledTextInput" class="col-sm-5 col-form-label" style="width: 100%;">Descripción</label>
                                        <div class="form-group" style="padding:2px;">
                                            <textarea id="txtDescActividadExtraLab" type="text" class="form-control" placeholder="Ingrese aquí la descripción de la actividad realizada"></textarea>
                                        </div>
                                    </div> 
                                    <div class="col-sm-3" style="padding:0;">  
                                        <label for="disabledTextInput" class="col-sm-12 col-form-label" style="width: 100%;">Fecha</label>
                                        <div class="form-group" style="padding:2px;">
                                            <input type="date" class="form-control" id="fechaActExtraLab" placeholder="Fecha">
                                        </div> 
                                    </div> 
                                </div>

                            </div>

                        </div>


                       <!-- Cuadro de Resultados -->
<div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
    <div class="well-header" style="margin-top: 0; margin-bottom: 3rem;">
        <div>
            <h4 class="well-title">RESULTADOS DE EXÁMENES GENERALES Y ESPECÍFICOS</h4>
        </div>
    </div>

    <%-- Fila 1 con cabeceras --%>
    <div class="horizontal-group-simple" style="margin:0 3rem;">
        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <label>EXÁMEN</label>
                <input id="txtNomExamen1" type="text" class="form-control select-principal" placeholder="Examen" value="">
            </div>
        </div>
        <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <label>FECHA</label>
                <input type="date" class="form-control" id="fechaExam1" value="">
            </div>
        </div>
        <div class="cuadros-ingreso-normal6" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <label>RESULTADOS</label>
                <input id="txtResExamen1" type="text" class="form-control select-medidas" placeholder="Resultado" value="">
            </div>
        </div>
    </div>

    <%-- Fila 2 --%>
    <div class="horizontal-group-simple" style="margin:0 3rem;">
        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input id="txtNomExamen2" type="text" class="form-control select-principal" placeholder="Examen" value="">
            </div>
        </div>
        <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input type="date" class="form-control" id="fechaExam2" value="">
            </div>
        </div>
        <div class="cuadros-ingreso-normal6" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input id="txtResExamen2" type="text" class="form-control select-medidas" placeholder="Resultado" value="">
            </div>
        </div>
    </div>

    <%-- Fila 3 --%>
    <div class="horizontal-group-simple" style="margin:0 3rem;">
        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input id="txtNomExamen3" type="text" class="form-control select-principal" placeholder="Examen" value="">
            </div>
        </div>
        <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input type="date" class="form-control" id="fechaExam3" value="">
            </div>
        </div>
        <div class="cuadros-ingreso-normal6" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input id="txtResExamen3" type="text" class="form-control select-medidas" placeholder="Resultado" value="">
            </div>
        </div>
    </div>

    <%-- Fila 4 --%>
    <div class="horizontal-group-simple" style="margin:0 3rem;">
        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input id="txtNomExamen4" type="text" class="form-control select-principal" placeholder="Examen" value="">
            </div>
        </div>
        <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input type="date" class="form-control" id="fechaExam4" value="">
            </div>
        </div>
        <div class="cuadros-ingreso-normal6" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input id="txtResExamen4" type="text" class="form-control select-medidas" placeholder="Resultado" value="">
            </div>
        </div>
    </div>

    <%-- Fila 5 --%>
    <div class="horizontal-group-simple" style="margin:0 3rem;">
        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input id="txtNomExamen5" type="text" class="form-control select-principal" placeholder="Examen" value="">
            </div>
        </div>
        <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input type="date" class="form-control" id="fechaExam5" value="">
            </div>
        </div>
        <div class="cuadros-ingreso-normal6" style="margin-bottom: 10px;">
            <div class="grupo-input text-center">
                <input id="txtResExamen5" type="text" class="form-control select-medidas" placeholder="Resultado" value="">
            </div>
        </div>
    </div>

    <%-- Observaciones --%>
    <div class="form-group row" style="margin: 0 2rem;">
        <label class="col-form-label">Observaciones</label>
        <textarea id="txtExamenbservacion" rows="2" style="width: 100%; resize: vertical;" placeholder="En la sección de 'observaciones', detallar la patología encontrada."></textarea>
    </div>
</div>


                        <!-- Cuadro de DIAGNÓSTICO " -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            
                            <div class="well-header" style="margin-top: 0; margin-bottom:2rem;">
                                <div>
                                    <h4 class="well-title">DIAGNÓSTICO</h4>
                                </div>
                            </div>
                            <div class="horizontal-group-simple" style="margin: 1rem 2rem 0 2rem;">
                                <div class="cuadros-ingreso-normal7">
                                    <div class="grupo-input text-center">
                                        <label for="disabledTextInput">DESCRIPCIÓN</label>
                                        <input id="txtDiagDescripcion1" type="text" class="form-control select-medidas" placeholder="Descripción" value="" onfocus="setInputActivo(1)" oninput="BuscarCodigosCIE()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboCodigos1"></ul>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <div class="grupo-input text-center">
                                        <label for="disabledTextInput">CÓDIGO CIE</label>
                                        <input id="txtDiagCIE1" type="text" class="form-control" placeholder="CIE" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal2">
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

                            <div class="horizontal-group-simple" style="margin: 1rem 2rem 0 2rem;">
                                <div class="cuadros-ingreso-normal7">
                                    <div class="grupo-input text-center">
                                        <input id="txtDiagDescripcion2" type="text" class="form-control select-medidas" placeholder="Descripción" value="" onfocus="setInputActivo(2)"  oninput="BuscarCodigosCIE()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboCodigos2"></ul>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <div class="grupo-input text-center">
                                        <input id="txtDiagCIE2" type="text" class="form-control" placeholder="CIE" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal2">
                                    <fieldset class="grupo-input text-center">
                                        <select id="txtDiagnositicoSelect2" class="form-control">
                                            <option value="" selected>- Seleccionar -</option>
                                            <option value="PRE">Presuntivo</option>
                                            <option value="DEF">Definitivo</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>

                            <div class="horizontal-group-simple" style="margin: 1rem 2rem 1rem 2rem;">
                                <div class="cuadros-ingreso-normal7">
                                    <div class="grupo-input text-center">
                                        <input id="txtDiagDescripcion3" type="text" class="form-control select-medidas" placeholder="Descripción" value="" onfocus="setInputActivo(3)"  oninput="BuscarCodigosCIE()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboCodigos3"></ul>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <div class="grupo-input text-center">
                                        <input id="txtDiagCIE3" type="text" class="form-control" placeholder="CIE" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal2">
                                    <fieldset class="grupo-input text-center">
                                        <select id="txtDiagnositicoSelect3" class="form-control">
                                            <option value="" selected>- Seleccionar -</option>
                                            <option value="PRE">Presuntivo</option>
                                            <option value="DEF">Definitivo</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>

                            <div class="horizontal-group-simple" style="margin: 1rem 2rem 0 2rem;">
                                <div class="cuadros-ingreso-normal7">
                                    <div class="grupo-input text-center">
                                        <input id="txtDiagDescripcion4" type="text" class="form-control select-medidas" placeholder="Descripción" value="" onfocus="setInputActivo(4)" oninput="BuscarCodigosCIE()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboCodigos4"></ul>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <div class="grupo-input text-center">
                                        <input id="txtDiagCIE4" type="text" class="form-control" placeholder="CIE" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal2">
                                    <fieldset class="grupo-input text-center">
                                        <select id="txtDiagnositicoSelect4" class="form-control">
                                            <option value="" selected>- Seleccionar -</option>
                                            <option value="PRE">Presuntivo</option>
                                            <option value="DEF">Definitivo</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>

                            <div class="horizontal-group-simple" style="margin: 1rem 2rem 0 2rem;">
                                <div class="cuadros-ingreso-normal7">
                                    <div class="grupo-input text-center">
                                        <input id="txtDiagDescripcion5" type="text" class="form-control select-medidas" placeholder="Descripción" value="" onfocus="setInputActivo(5)" oninput="BuscarCodigosCIE()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboCodigos5"></ul>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <div class="grupo-input text-center">
                                        <input id="txtDiagCIE5" type="text" class="form-control" placeholder="CIE" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal2">
                                    <fieldset class="grupo-input text-center">
                                        <select id="txtDiagnositicoSelect5" class="form-control">
                                            <option value="" selected>- Seleccionar -</option>
                                            <option value="PRE">Presuntivo</option>
                                            <option value="DEF">Definitivo</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>

                            <div class="horizontal-group-simple" style="margin: 1rem 2rem 2rem 2rem;">
                                <div class="cuadros-ingreso-normal7">
                                    <div class="grupo-input text-center">
                                        <input id="txtDiagDescripcion6" type="text" class="form-control select-medidas" placeholder="Descripción" value="" onfocus="setInputActivo(6)" oninput="BuscarCodigosCIE()">
                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboCodigos6"></ul>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal3">
                                    <div class="grupo-input text-center">
                                        <input id="txtDiagCIE6" type="text" class="form-control" placeholder="CIE" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal2">
                                    <fieldset class="grupo-input text-center">
                                        <select id="txtDiagnositicoSelect6" class="form-control">
                                            <option value="" selected>- Seleccionar -</option>
                                            <option value="PRE">Presuntivo</option>
                                            <option value="DEF">Definitivo</option>
                                        </select>
                                    </fieldset>
                                </div>
                            </div>


                        </div>


                        <!-- Cuadro de APTITUD MÉDICA PARA EL TRABAJO " -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">APTITUD MÉDICA PARA EL TRABAJO</h4>
                                </div>
                            </div>

                            <div class="horizontal-group-simple" style="margin: 2rem 3rem;">

                                <div class="col-sm-4" style="padding-right: 1rem;">
                                    <fieldset class="grupo-input text-center">
                                        <label for="disabledTextInput">APTITUD</label>
                                            <select id="txtAptitudSelect" class="form-control">
                                                <option value="" selected>- Seleccionar -</option>
                                                <option value="apto">APTO </option>
                                                <option value="aptoObservacion">APTO CON OBSERVACIÓN</option>      
                                                <option value="aptoLimitacion">APTO CON LIMITACIONES </option>   
                                                <option value="noApto">NO APTO</option>   
                                            </select>
                                     </fieldset>
                                </div>
                                <div class="col-sm-8">
                                    <div class="col-sm-12" id="descObservacion">
                                        <div class="grupo-input text-center" >
                                            <label for="disabledTextInput">Observación</label>
                                            <input id="txtDescObservacion" type="text" class="form-control select-temas" placeholder="Descripción" value="">
                                        </div>
                                    </div>     
                                    <div class="col-sm-12" id="descLimitacion" style="margin-top:1rem;">
                                        <div class="grupo-input text-center" >
                                            <label for="disabledTextInput">Limitación</label>
                                            <input id="txtDescLimitacion" type="text" class="form-control select-principal" placeholder="Descripción" value="">
                                        </div>
                                    </div> 

                                </div> 
                           </div>

                            <div class="form-group" style="margin: 0 2rem;">
                                <label for="disabledTextInput" class="col-form-label">OBSERVACIONES</label>
                                <textarea id="txtObsAptitud" rows="4" style="width: 100%; resize: vertical;" value="" placeholder="De acuerdo a la valoración médica efectuada colocar la observación."></textarea>
                            </div>

                        </div>


                        <!-- Cuadro de APTITUD MÉDICA PARA EL TRABAJO " -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">RECOMENDACIONES Y/O TRATAMIENTO</h4>
                                </div>
                            </div>
                            <div class="form-group" style="margin: 0 2rem;">
                                <label for="disabledTextInput" class="col-form-label">Descripción</label>
                                <textarea id="txtRecomendacion" rows="4" style="width: 100%; resize: vertical;" value="" placeholder="De acuerdo a la valoración médica efectuada colocar las recomendaciones y/o tratamiento farmacológico y no farmacológico."></textarea>
                            </div>
                        </div>


                        <!-- Cuadro de APTITUD MÉDICA PARA EL TRABAJO " -->
                        <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0;">
                                <div>
                                    <h4 class="well-title">RETIRO (evaluación)</h4>
                                </div>
                            </div>

                            
                            <div class="horizontal-group-simple" style="margin:2rem 2.5rem 0 2.5rem;">                                    
                                <div class="col-sm-10 input-group" style="">
                                    <label for="disabledTextInput" class="col-form-label">SE REALIZÓ LA EVALUACIÓN?</label>
                                    <select id="EvaluacionRetiroSelect" class="col-sm-6 form-control">
                                         <option style="background-color:gainsboro;" value="" selected>Si/No</option>
                                         <option value="si">SI</option>
                                         <option value="no">NO</option>
                                    </select>
                                </div>
                            </div>
  
                            <div class="horizontal-group-simple" style="margin:2rem 2.5rem 0 2.5rem;">                                    
                                    
                                <div class="col-sm-10 input-group" style="">
                                    <label for="disabledTextInput" class="col-form-label">LA CONDICIÓN DE SALUD ESTA RELACIONADA CON EL TRABAJO</label>
                                    <select id="CondicionRetiroSelect" class="col-sm-6 form-control">
                                         <option style="background-color:gainsboro;" value="" selected>Si/No</option>
                                         <option value="si">SI</option>
                                         <option value="no">NO</option>
                                    </select>
                                </div>
                            </div>


                            <div class="form-group" style="margin: 0 2rem;">
                                <label for="disabledTextInput" class="col-form-label">Observación</label>
                                <textarea id="txtObsRetiro" rows="4" style="width: 100%; resize: vertical;" value="" placeholder="Ingrese la valoración"></textarea>
                            </div>

                        </div>



                        <div class="horizontal-group-start" id="btnCarga" style="display:none; margin-top:4rem; margin-bottom:3rem;">
                            <div class="col-sm-4" style="color:whitesmoke; display:flex; justify-content:space-between; padding-left:0; padding-right:0;">
                                <div class="col-sm-6 text-center input-group horizontal-group-start">       
                                    <label class="control-label">fecha: </label>
                                    <input type="date" style="margin-left:0.5rem;" class="form-control" id="fechaFormulario" value="">
                                </div>
                                <div class="col-sm-5 input-group horizontal-group-end" style="padding-right:1rem;">  
                                    <label class="control-label">hora: </label>
                                    <input type="text" style="margin-left:0.5rem;" class="form-control" id="horaFormulario" placeholder="00:00" value="">
                                </div>
                            </div>                    

                            <!--div class="" id="btnCarga" style="display:none; justify-content:center;"-->
                                <button class="btn btn-info col-sm-6" type="button" id="btnCargarDatosPersonales" onclick="GuardarHistoria()" style="width: 25%; margin-left:5rem;">Cargar Datos</button>
                            <!--/div-->        
                
                            <!-- Boton flotante de regreso-->
                            <div><a id="btnRegresar" class="text-center chat-button"></a></div>
                        
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
