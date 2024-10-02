<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="HistoriaInmunizaciones.aspx.cs" Inherits="ReporteTareas.Formulario.HistoriaInmunizaciones" %>
<asp:Content ID="HistoriaInmunizacionesContent" ContentPlaceHolderID="head" runat="server">
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
    <script src="../js/HistoriaImunizaciones.js?v="></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    HistoriaInmunizaciones.aspx.cs
    
    <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content;">
        <div class="col-lg-12" style="background-color: #0D2538;padding-bottom: 1.5rem;padding-top: 0.5rem;">

            <div class="row" style="background-color: #0D2538; margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs">

                    <ul class="breadcrumb">
                        <li>
                            <a href="#"><b>Registro de inmunizaciones para salud en el trabajo</b></a>
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

                <!-- No se muestra pero es importante para cargar los datos del empleado -->
                <div class="input-group col-sm-4" style="display:none; margin: 0 auto; padding-top: 1rem; padding-bottom: 1rem;">
                    <input type="text" id="txtEmpleado" class="form-control" placeholder="Buscar paciente">
                    <div class="input-group-btn">
                        <button class="btn btn-default" type="button" id="btnBuscarDatosPersonales" onclick="BuscarEmpleado()"><i class="glyphicon glyphicon-search"></i></button>
                    </div>
                </div>
                
                    <!-- Contenido de la pag -->
                    <div id="pestaniaConsulta" style="display: block; padding: 1rem 1rem 0.2rem 1rem; background-color: #9DA8AD;">
                    
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
                                                        <span class="input-group-addon" style="width: 25px;"><i class="glyphicon glyphicon-user"></i>  Nombre:</span>
                                                        <input id="txtNombre" type="text" class="form-control" name="nombre" placeholder="Nombre" oninput="convertirAMayusculas(this)" readonly>
                                                    </div>
                                                </div>
                                                <div class="horizontal">
                                                    <div class="input-group col-sm-6">
                                                        <span class="input-group-addon input-basic3"><i class="glyphicon glyphicon-credit-card"></i>  Cédula:</span>
                                                        <input id="txtCedula" type="text" class="form-control" name="cedula" placeholder="Cedula" readonly>
                                                    </div>
                                                </div>
                                                <div class="horizontal">  
                                                    <div class="input-group col-sm-6">
                                                        <span class="input-group-addon input-basic3"><i class="glyphicon glyphicon-leaf"></i>  Estado civil:</span>
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
                                                        <span class="input-group-addon input-basic3"><i class="glyphicon glyphicon-home"></i>  Sociedad:</span>
                                                        <input id="txtSociedad" type="text" class="form-control" name="sociedad" placeholder="Sociedad" readonly>
                                                    </div>    
                                                    <div class="input-group col-sm-5" style="margin-left: 6rem;">
                                                        <span class="input-group-addon input-medium"><i class="glyphicon glyphicon-link"></i>  Area de trabajo:</span>
                                                        <input id="txtAreaTrabajo" type="text" class="form-control" name="areatrabajo" placeholder="Area de trabajo" readonly>
                                                    </div>
                                                </div>
                                                <div class="horizontal-group">
                                                    <div class="input-group col-sm-6">
                                                        <span class="input-group-addon input-medium"><i class="glyphicon glyphicon-lock"></i>  Puesto de trabajo:</span>
                                                        <input id="txtPuestoTrabajo" type="text" class="form-control" name="puestotrabajo" placeholder="Puesto de trabajo" value="" readonly>
                                                    </div>                                                
                                                </div>
                                                <!-- ... otros campos ... -->
                                            </div>

                                            <div class="col-sm-3" style="text-align: center;">
                                                <div id="imagenDiv" class="imagen-div"></div>
                                                <!--img id="imgEmpleado" src="../carrusel/imagenes/usuarios.png" alt="Imagen de Empleado" height="200" width="200"-->
                                                <!--button type="button" class="btn btn-danger" style="margin-top: 1rem;"><i class="glyphicon glyphicon-user"></i>  Contacto de emergencia</button-->
                                            </div>

                                        </formview>
                                    </div>

                                </div>

                            </div>

                        <!--    Cuadros de INMUNIZACIONES   -->
                        <div class="well color-well4" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                            <div class="well-header" style="margin-top: 0; margin-bottom:4rem; color:white;">
                                <div>
                                    <h3 class="well-title">INMUNIZACIONES</h3>
                                </div>
                            </div>
                            <div class="well color-opaco" style="margin:0 0.5rem;">

                                <div class="well-header text-center" style="margin-top: 1rem; margin-bottom:2rem;">
                                    <div>
                                        <h4 class="well-title">TÉTANOS - DIFTERIA</h4>
                                    </div>
                                </div>                               

                                <div class="horizontal-group-simple" >
                                    <!-- TITULOS -->
                                    <div class="">
                                        <div class="grupo-input text-center">
                                            <label for="disabledTextInput" class="control-label" style="font-size:90%;">Dosis</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal3">
                                        <div class="grupo-input text-center">
                                            <label for="disabledTextInput" class="control-label" style="font-size:90%;">FECHA</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center">
                                            <label for="disabledTextInput" class="control-label" style="font-size:90%;">LOTE</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <fieldset class="input-group text-center">
                                            <label for="disabledTextInput" class="control-label" style="font-size:90%;">ESQUEMA COMPLETO</label>

                                        </fieldset>
                                    </div>                                
                                    <div class="cuadros-ingreso-normal3">
                                        <fieldset class="input-group text-center">
                                            <label for="disabledTextInput" class="control-label" style="font-size:90%;">NOMBRES COMPLETOS</label>
                                        </fieldset>
                                    </div>  
                                    <div class="cuadros-ingreso-normal4">
                                        <fieldset class="input-group text-center">
                                            <label for="disabledTextInput" class="control-label" style="font-size:90%;">ESTABLECIMIENTO</label>
                                        </fieldset>
                                    </div>
                                    <div class="cuadros-ingreso-normal5">
                                        <fieldset class="input-group text-center">
                                            <label for="disabledTextInput" class="control-linput-azulont-size:90%;">OBSERVACIONES</label>
                                        </fieldset>
                                    </div>
                                </div> <!-- fin titulos -->

                                <div class="horizontal-group-start"> <!-- fila 1 -->                                
                                    <!-- cuadros 1 -->
                                    <div class="">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <label id="txtTetanosDosis1"class="form-control select-dorado">1°</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input type="date" class="form-control input-azul" id="fechaTetanos1" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input id="txtTetanosLote1" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <select id="SelectTetanosEsquema1" class="form-control input-medium select-principal">
                                                <option value="" selected>Si/No</option>                                           
                                                <option value="SI">Si</option>
                                                <option value="">No</option>
                                            </select>
                                        </fieldset>
                                    </div>                                
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <input id="txtTetanosNombre1" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                    </div>  
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <input id="txtTetanosEstablecimiento1" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                        </fieldset>
                                    </div>
                                    <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                        <input id="txtTetanosObs1" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                        </fieldset>
                                    </div>
                                </div>

                                <div class="horizontal-group-start"> <!-- otros fila 2 -->                                
                                    <!-- cuadros 2 -->
                                    <div class="">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <label id="txtTetanosDosis2"class="form-control select-dorado">2°</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input type="date" class="form-control input-azul" id="fechaTetanos2" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input id="txtTetanosLote2" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <select id="SelectTetanosEsquema2" class="form-control input-medium select-principal">
                                                <option value="" selected>Si/No</option>                                           
                                                <option value="SI">Si</option>
                                                <option value="">No</option>
                                            </select>
                                        </fieldset>
                                    </div>                                
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <input id="txtTetanosNombre2" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                    </div>  
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <input id="txtTetanosEstablecimiento2" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                        </fieldset>
                                    </div>
                                    <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                        <input id="txtTetanosObs2" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                        </fieldset>
                                    </div>
                                </div>

                                <div class="horizontal-group-start"> <!-- otros fila 3 -->                                
                                    <!-- cuadros 3 -->
                                    <div class="">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <label id="txtTetanosDosis3"class="form-control select-dorado">3°</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input type="date" class="form-control input-azul" id="fechaTetanos3" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input id="txtTetanosLote3" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <select id="SelectTetanosEsquema3" class="form-control input-medium select-principal">
                                                <option value="" selected>Si/No</option>                                           
                                                <option value="SI">Si</option>
                                                <option value="">No</option>
                                            </select>
                                        </fieldset>
                                    </div>                                
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <input id="txtTetanosNombre3" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                    </div>  
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <input id="txtTetanosEstablecimiento3" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                        </fieldset>
                                    </div>
                                    <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                        <input id="txtTetanosObs3" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                        </fieldset>
                                    </div>
                                </div>

                                <div class="horizontal-group-start"> <!-- otros fila 4 -->                                
                                    <!-- cuadros 4 -->
                                    <div class="">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <label id="txtTetanosDosis4"class="form-control select-dorado">4°</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input type="date" class="form-control input-azul" id="fechaTetanos4" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input id="txtTetanosLote4" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <select id="SelectTetanosEsquema4" class="form-control input-medium select-principal">
                                                <option value="" selected>Si/No</option>                                           
                                                <option value="SI">Si</option>
                                                <option value="">No</option>
                                            </select>
                                        </fieldset>
                                    </div>                                
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <input id="txtTetanosNombre4" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                    </div>  
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <input id="txtTetanosEstablecimiento4" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                        </fieldset>
                                    </div>
                                    <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                        <input id="txtTetanosObs4" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                        </fieldset>
                                    </div>
                                </div>

                                <div class="horizontal-group-start"> <!-- otros fila 5 -->                                
                                    <!-- cuadros 5 -->
                                    <div class="">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <label id="txtTetanosDosis5"class="form-control select-dorado">5°</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input type="date" class="form-control input-azul" id="fechaTetanos5" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input id="txtTetanosLote5" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <select id="SelectTetanosEsquema5" class="form-control input-medium select-principal">
                                                <option value="" selected>Si/No</option>                                           
                                                <option value="SI">Si</option>
                                                <option value="">No</option>
                                            </select>
                                        </fieldset>
                                    </div>                                
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <input id="txtTetanosNombre5" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                    </div>  
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <input id="txtTetanosEstablecimiento5" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                        </fieldset>
                                    </div>
                                    <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                        <input id="txtTetanosObs5" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                        </fieldset>
                                    </div>                                    
                                </div>
                                <hr />
                                <!--/div> <!-- fin well tetanos -->

                            <!--div class="well color-well2" style="margin:2rem 0.5rem;"-->

                            <div class="well-header text-center" style="margin-top: 4rem; margin-bottom:0;">
                                 <div>
                                     <h4 class="well-title">HEPATITIS A</h4>
                                 </div>
                            </div>


                            <div class="horizontal-group-start"> <!-- fila 1 -->                                
                                <!-- cuadros 1 -->
                                <div class="">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <label id="txtHepatitisADosis1"class="form-control select-dorado">1°</label>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input type="date" class="form-control input-azul" id="fechaHepatitisA1" value="NA">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input id="txtHepatitisALote1" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="SelectHepatitisAEsquema1" class="form-control input-medium select-principal">                                            <option value="" selected>Si/No</option>                                           
                                            <option value="SI">Si</option>
                                            <option value="">No</option>
                                        </select>
                                    </fieldset>
                                </div>                                
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <input id="txtHepatitisANombre1" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                </div>  
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <input id="txtHepatitisAEstablecimiento1" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                    <input id="txtHepatitisAObs1" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                    </fieldset>
                                </div>
                            </div>

                            <div class="horizontal-group-start"> <!-- fila 2 -->                                
                                <!-- cuadros 2 -->
                                <div class="">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <label id="txtHepatitisADosis2"class="form-control select-dorado">2°</label>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input type="date" class="form-control input-azul" id="fechaHepatitisA2" value="NA">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input id="txtHepatitisALote2" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="SelectHepatitisAEsquema2" class="form-control input-medium select-principal">                                            <option value="" selected>Si/No</option>                                           
                                            <option value="SI">Si</option>
                                            <option value="">No</option>
                                        </select>
                                    </fieldset>
                                </div>                                
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <input id="txtHepatitisANombre2" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                </div>  
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <input id="txtHepatitisAEstablecimiento2" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                    <input id="txtHepatitisAObs2" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                    </fieldset>
                                </div>
                            </div>

                            <div class="horizontal-group-start"> <!-- fila 3 -->                                
                                <!-- cuadros 3 -->
                                <div class="">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <label id="txtHepatitisADosis3"class="form-control select-dorado">3°</label>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input type="date" class="form-control input-azul" id="fechaHepatitisA3" value="NA">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input id="txtHepatitisALote3" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="SelectHepatitisAEsquema3" class="form-control input-medium select-principal">                                            <option value="" selected>Si/No</option>                                           
                                            <option value="SI">Si</option>
                                            <option value="">No</option>
                                        </select>
                                    </fieldset>
                                </div>                                
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <input id="txtHepatitisANombre3" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                </div>  
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <input id="txtHepatitisAEstablecimiento3" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                    <input id="txtHepatitisAObs3" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                    </fieldset>
                                </div>
                                
                            </div>
                            <hr />
                            <!--/div><!-- fin well hepatitis A -->

                            <!--div class="well color-well4" style="margin:2rem 0.5rem;"-->

                            <div class="well-header text-center" style="margin-top: 4rem; margin-bottom:0;">
                                <div>                                    
                                    <h4 class="well-title">HEPATITIS B</h4>
                                </div>
                            </div>


                            <div class="horizontal-group-start"> <!-- fila 1 -->                                
                                <!-- cuadros 1 -->
                                <div class="">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <label id="txtHepatitisBDosis1"class="form-control select-dorado">1°</label>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input type="date" class="form-control input-azul" id="fechaHepatitisB1" value="NA">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input id="txtHepatitisBLote1" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="SelectHepatitisBEsquema1" class="form-control input-medium select-principal">                                            <option value="" selected>Si/No</option>                                           
                                            <option value="SI">Si</option>
                                            <option value="">No</option>
                                        </select>
                                    </fieldset>
                                </div>                                
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <input id="txtHepatitisBNombre1" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                </div>  
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <input id="txtHepatitisBEstablecimiento1" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                    <input id="txtHepatitisBObs1" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                    </fieldset>
                                </div>
                            </div>

                            <div class="horizontal-group-start"> <!-- fila 2 -->                                
                                <!-- cuadros 2 -->
                                <div class="">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <label id="txtHepatitisBDosis2"class="form-control select-dorado">2°</label>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input type="date" class="form-control input-azul" id="fechaHepatitisB2" value="NA">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input id="txtHepatitisBLote2" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="SelectHepatitisBEsquema2" class="form-control input-medium select-principal">                                            <option value="" selected>Si/No</option>                                           
                                            <option value="SI">Si</option>
                                            <option value="">No</option>
                                        </select>
                                    </fieldset>
                                </div>                                
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <input id="txtHepatitisBNombre2" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                </div>  
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <input id="txtHepatitisBEstablecimiento2" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                    <input id="txtHepatitisBObs2" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                    </fieldset>
                                </div>
                            </div>

                            <div class="horizontal-group-start"> <!-- fila 3 -->                                
                                <!-- cuadros 3 -->
                                <div class="">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <label id="txtHepatitisBDosis3"class="form-control select-dorado">3°</label>
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input type="date" class="form-control input-azul" id="fechaHepatitisB3" value="NA">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal">
                                    <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                        <input id="txtHepatitisBLote3" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                    </div>
                                </div>
                                <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <select id="SelectHepatitisBEsquema3" class="form-control input-medium select-principal">                                            <option value="" selected>Si/No</option>                                           
                                            <option value="SI">Si</option>
                                            <option value="">No</option>
                                        </select>
                                    </fieldset>
                                </div>                                
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <input id="txtHepatitisBNombre3" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                </div>  
                                <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                        <input id="txtHepatitisBEstablecimiento3" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                    </fieldset>
                                </div>
                                <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                    <fieldset class="input-group text-center">
                                    <input id="txtHepatitisBObs3" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                    </fieldset>
                                </div>                                
                            </div>
                            <hr />
                                <!-- fin well hepatitis B -->

                                <!--div class="well color-well4" style="margin:2rem 0.5rem;"-->

                                <div class="well-header text-center" style="margin-top: 4rem; margin-bottom:0;">
                                    <div>
                                        <h4 class="well-title">INFLUENZA ESTACIONARIA</h4>
                                    </div>
                                </div>

                                <div class="horizontal-group-start"> <!-- fila 1 -->                                
                                    <!-- cuadros 1 -->
                                    <div class="">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <label id="txtInfluenzaDosis1"class="form-control select-dorado">U</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input type="date" class="form-control input-azul" id="fechaInfluenza1" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input id="txtInfluenzaLote1" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <select id="SelectInfluenzaEsquema1" class="form-control input-medium select-principal">                                               <option value="" selected>Si/No</option>                                           
                                                <option value="SI">Si</option>
                                                <option value="">No</option>
                                            </select>
                                        </fieldset>
                                    </div>                                
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <input id="txtInfluenzaNombre1" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                    </div>  
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <input id="txtInfluenzaEstablecimiento1" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                        </fieldset>
                                    </div>
                                    <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                        <input id="txtInfluenzaObs1" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                        </fieldset>
                                    </div>                                    
                                </div>
                                <hr />
                                <!-- fin well INFLUENZA ESTACIONARIA -->

                                <!--div class="well color-well4" style="margin:2rem 0.5rem;"-->

                                <div class="well-header text-center" style="margin-top: 4rem; margin-bottom:0;">
                                    <div>
                                        <h4 class="well-title">FIEBRE AMARILLA</h4>
                                    </div>
                                </div>


                                <div class="horizontal-group-start"> <!-- fila 1 -->                                
                                    <!-- cuadros 1 -->
                                    <div class="">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <label id="txtFiebreDosis1"class="form-control select-dorado">U</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input type="date" class="form-control input-azul" id="fechaFiebre1" value="" />
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input id="txtFiebreLote1" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <select id="SelectFiebreEsquema1" class="form-control input-medium select-principal">                                            <option value="" selected>Si/No</option>                                           
                                                <option value="SI">Si</option>
                                                <option value="">No</option>
                                            </select>
                                        </fieldset>
                                    </div>                                
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <input id="txtFiebreNombre1" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                    </div>  
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <input id="txtFiebreEstablecimiento1" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                        </fieldset>
                                    </div>
                                    <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                        <input id="txtFiebreObs1" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                        </fieldset>
                                    </div>
                                </div>
                                <hr />
                                <!-- fin well FIEBRE AMARILLA -->

                                <!--div class="well color-well4" style="margin:2rem 0.5rem;"-->

                                <div class="well-header text-center" style="margin-top: 4rem; margin-bottom:0;">
                                    <div>
                                        <h4 class="well-title">SARAMPIÓN - RUBÉOLA</h4>
                                    </div>
                                </div>

                                <div class="horizontal-group-start"> <!-- fila 1 -->                                
                                    <!-- cuadros 1 -->
                                    <div class="">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <label id="txtSarampionDosis1"class="form-control select-dorado">1°</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input type="date" class="form-control input-azul" id="fechaSarampion1" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input id="txtSarampionLote1" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <select id="SelectSarampionEsquema1" class="form-control input-medium select-principal">                                               <option value="" selected>Si/No</option>                                           
                                                <option value="SI">Si</option>
                                                <option value="">No</option>
                                            </select>
                                        </fieldset>
                                    </div>                                
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <input id="txtSarampionNombre1" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                    </div>  
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <input id="txtSarampionEstablecimiento1" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                        </fieldset>
                                    </div>
                                    <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                        <input id="txtSarampionObs1" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                        </fieldset>
                                    </div>
                                </div>

                                <div class="horizontal-group-start"> <!-- fila 2 -->                                
                                    <!-- cuadros 2 -->
                                    <div class="">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <label id="txtSarampionDosis2"class="form-control select-dorado">2°</label>
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input type="date" class="form-control input-azul" id="fechaSarampion2" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal">
                                        <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                            <input id="txtSarampionLote2" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                        </div>
                                    </div>
                                    <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <select id="SelectSarampionEsquema2" class="form-control input-medium select-principal">
                                                <option value="" selected>Si/No</option>                                           
                                                <option value="SI">Si</option>
                                                <option value="">No</option>
                                            </select>
                                        </fieldset>
                                    </div>                                
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <input id="txtSarampionNombre2" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                    </div>  
                                    <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                            <input id="txtSarampionEstablecimiento2" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                        </fieldset>
                                    </div>
                                    <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                        <fieldset class="input-group text-center">
                                        <input id="txtSarampionObs2" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                        </fieldset>
                                    </div>
                                </div><!-- fin well SARAMPION -->

                            </div><!--    FIN de WELL interno   -->

                            <div class="well color-opaco" style="margin:2rem 0.5rem;">

                                <div id="inmunizacionContainer">
                                    <div class="horizontal-group" style="margin-top: 1rem; margin-bottom:2rem; justify-content:center;">
                                        <div class="col-sm-6">
                                            <input id="txtInmunizacsionNueva" type="text" class="text-center form-control input-nombre" placeholder="NOMBRE NUEVA INMUNIZACIÓN" value="" style="font-weight: bold; font-size: 1.5rem;">
                                        </div>
                                    </div>   

                                    <div class="horizontal-group-simple" >
                                        <!-- TITULOS -->
                                        <div class="">
                                            <div class="grupo-input text-center">
                                                <label for="disabledTextInput" class="control-label" style="font-size:90%;">Dosis</label>
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal3">
                                            <div class="grupo-input text-center">
                                                <label for="disabledTextInput" class="control-label" style="font-size:90%;">FECHA</label>
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center">
                                                <label for="disabledTextInput" class="control-label" style="font-size:90%;">LOTE</label>
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <fieldset class="input-group text-center">
                                                <label for="disabledTextInput" class="control-label" style="font-size:90%;">ESQUEMA COMPLETO</label>

                                            </fieldset>
                                        </div>                                
                                        <div class="cuadros-ingreso-normal3">
                                            <fieldset class="input-group text-center">
                                                <label for="disabledTextInput" class="control-label" style="font-size:90%;">NOMBRES COMPLETOS</label>
                                            </fieldset>
                                        </div>  
                                        <div class="cuadros-ingreso-normal4">
                                            <fieldset class="input-group text-center">
                                                <label for="disabledTextInput" class="control-label" style="font-size:90%;">ESTABLECIMIENTO</label>
                                            </fieldset>
                                        </div>
                                        <div class="cuadros-ingreso-normal5">
                                            <fieldset class="input-group text-center">
                                                <label for="disabledTextInput" class="control-linput-azulont-size:90%;">OBSERVACIONES</label>
                                            </fieldset>
                                        </div>
                                    </div> <!-- fin titulos -->

                                    <div class="horizontal-group-start"> <!-- fila 1 -->                                
                                        <!-- cuadros 1 -->
                                        <div class="">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <label id="txtNuevaDosis1"class="form-control select-dorado">1°</label>
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <input type="date" class="form-control input-azul" id="fechaNuevo1" value="">
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <input id="txtNuevoLote1" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                                <select id="SelectNuevoEsquema1" class="form-control input-medium select-principal">
                                                    <option value="" selected>Si/No</option>                                           
                                                    <option value="SI">Si</option>
                                                    <option value="">No</option>
                                                </select>
                                            </fieldset>
                                        </div>                                
                                        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                            <input id="txtNuevoNombre1" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">
                                        </div>  
                                        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                                <input id="txtNuevoEstablecimiento1" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                            </fieldset>
                                        </div>
                                        <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                            <input id="txtNuevoObs1" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                            </fieldset>
                                        </div>
                                    </div>

                                    <div class="horizontal-group-start"> <!-- otros fila 2 -->                                
                                        <!-- cuadros 2 -->
                                        <div class="">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <label id="txtNuevaDosis2"class="form-control select-dorado">2°</label>
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <input type="date" class="form-control input-azul" id="fechaNuevo2" value="">
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <input id="txtNuevoLote2" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                                <select id="SelectNuevoEsquema2" class="form-control input-medium select-principal">
                                                    <option value="" selected>Si/No</option>                                           
                                                    <option value="SI">Si</option>
                                                    <option value="">No</option>
                                                </select>
                                            </fieldset>
                                        </div>                                
                                        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                            <input id="txtNuevoNombre2" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                        </div>  
                                        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                                <input id="txtNuevoEstablecimiento2" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                            </fieldset>
                                        </div>
                                        <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                            <input id="txtNuevoObs2" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                            </fieldset>
                                        </div>
                                    </div>

                                    <div class="horizontal-group-start"> <!-- otros fila 3 -->                                
                                        <!-- cuadros 3 -->
                                        <div class="">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <label id="txtNuevaDosis3"class="form-control select-dorado">3°</label>
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <input type="date" class="form-control input-azul" id="fechaNuevo3" value="">
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <input id="txtNuevoLote3" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                                <select id="SelectNuevoEsquema3" class="form-control input-medium select-principal">
                                                    <option value="" selected>Si/No</option>                                           
                                                    <option value="SI">Si</option>
                                                    <option value="">No</option>
                                                </select>
                                            </fieldset>
                                        </div>                                
                                        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                            <input id="txtNuevoNombre3" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                        </div>  
                                        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                                <input id="txtNuevoEstablecimiento3" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                            </fieldset>
                                        </div>
                                        <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                            <input id="txtNuevoObs3" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                            </fieldset>
                                        </div>
                                    </div>

                                    <div class="horizontal-group-start"> <!-- otros fila 4 -->                                
                                        <!-- cuadros 4 -->
                                        <div class="">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <label id="txtNuevaDosis4"class="form-control select-dorado">4°</label>
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <input type="date" class="form-control input-azul" id="fechaNuevo4" value="">
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <input id="txtNuevoLote4" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                                <select id="SelectNuevoEsquema4" class="form-control input-medium select-principal">
                                                    <option value="" selected>Si/No</option>                                           
                                                    <option value="SI">Si</option>
                                                    <option value="">No</option>
                                                </select>
                                            </fieldset>
                                        </div>                                
                                        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                            <input id="txtNuevoNombre4" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                        </div>  
                                        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                                <input id="txtNuevoEstablecimiento4" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                            </fieldset>
                                        </div>
                                        <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                            <input id="txtNuevoObs4" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                            </fieldset>
                                        </div>
                                    </div>

                                    <div class="horizontal-group-start"> <!-- otros fila 5 -->                                
                                        <!-- cuadros 5 -->
                                        <div class="">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <label id="txtNuevaDosis5"class="form-control select-dorado">5°</label>
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <input type="date" class="form-control input-azul" id="fechaNuevo5" value="">
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal">
                                            <div class="grupo-input text-center" style="margin-bottom: 10px;">
                                                <input id="txtNuevoLote5" type="text" class="form-control select-temas" placeholder="Lote" value="">
                                            </div>
                                        </div>
                                        <div class="cuadros-ingreso-normal" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                                <select id="SelectNuevoEsquema5" class="form-control input-medium select-principal">
                                                    <option value="" selected>Si/No</option>                                           
                                                    <option value="SI">Si</option>
                                                    <option value="">No</option>
                                                </select>
                                            </fieldset>
                                        </div>                                
                                        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                            <input id="txtNuevoNombre5" type="text" class="form-control input-nombre" placeholder="NOMBRES" value="">

                                        </div>  
                                        <div class="cuadros-ingreso-normal4" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                                <input id="txtNuevoEstablecimiento5" type="text" class="form-control select-temas" placeholder="ESTABLECIMIENTO DE SALUD" value="">
                                            </fieldset>
                                        </div>
                                        <div class="cuadros-ingreso-normal5" style="margin-bottom: 10px;">
                                            <fieldset class="input-group text-center">
                                            <input id="txtNuevoObs5" type="text" class="form-control input-azul" placeholder="OBSERVACION" value="">

                                            </fieldset>
                                        </div>                                    
                                    </div>
                                    <hr />
                                    <!--/div> <!-- fin well Nueva Inmunizacion -->

                                </div>
                                <div class="row" id="btnInm" style="justify-content:end;">
                                    <button class="btn btn-danger col-sm-6" type="button" id="btnNuevaInmunizacion" onclick="agregarInmunizacion()" style="width: 25%; margin: 1rem;">+ Agregar Inmunización</button>
                                </div>

                            </div>

                        </div><!--    FIN de INMUNIZACIONES   -->

                        </div>

                        <div class="" id="btnCarga" style="display:none; justify-content:center;">
                            <button class="btn btn-info col-sm-6" type="button" id="btnCargarDatosInm" onclick="GuardarInmunizaciones()" style="width: 25%; margin: 3rem;">Cargar Datos</button>
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



    