<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="InformacionContratos.aspx.cs" Inherits="ReporteTareas.Formulario.InformacionContratos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    
    <script src="../js/InformacionContratos.js?v=32"></script> 
    <link href="../dist/css/InfoContratos.css" rel="stylesheet" />   
    <link href="../dist/css/styleInventario.css" rel="stylesheet" />  
    <%--<link href="://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/css/all.min.css" rel="stylesheet">--%>
    <!-- Tipo de letra de Google  -->
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;700&display=swap" rel="stylesheet">


    <!-- jQuery (la versión completa en lugar de la slim) -->
    <script src="https://cdn.jsdelivr.net/npm/jquery@3.6.0.min.js"></script>    
    <script src="https://cdn.jsdelivr.net/npm/popper.js@1.16.1/dist/umd/popper.min.js" integrity="sha384-9/reFTGAW83EW2RDu2S0VKaIzap3H66lZH81PoYlFhbGU+6BZp6G7niu735Sk7lN" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/js/bootstrap.min.js" integrity="sha384-+sLIOodYLS7CIrQpBjl+C7nPvqq+FbNUBDunl/OZv93DB7Ln/533i8e/mZXLi/P+" crossorigin="anonymous"></script>
    

    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <script src="https://cdn.jsdelivr.net/jquery/latest/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/momentjs/latest/moment.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/daterangepicker/daterangepicker.min.js"></script>
    <script>
        $('#my-calendar').daterangepicker({
            "singleDatePicker": true,
            "showDropdowns": true,
            locale: { format: 'DD/MM/YYYY' },
        });
    </script>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <link href="../bower_components/sweetalert/css/animate.css" rel="stylesheet" />
    <link href="../bower_components/sweetalert/css/sweetalert2.min.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert2.all.min.js"></script>
        

    <script src="../bower_components/popper/popper.min.js"></script>
     
    <!-- El script de la librería PDF-->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jspdf-autotable/3.5.29/jspdf.plugin.autotable.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js"></script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">        

    <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content;">
        <div class="col-lg-12" style="background-color: #0D2538; padding:0.5rem 1.2rem 1.2rem 1.2rem;">

            <div class="row" style="background-color: #0D2538; margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs" style="margin-top:1rem;">

                    <ul class="breadcrumb" style="margin-bottom:1.5rem;">
                        <li style="display:flex; justify-content:space-between;">
                            <a href="#"><b>Ingreso de información de contrato</b></a>
                            <a href="#"><i class="glyphicon glyphicon-log-out"></i></a>
                        </li>
                    </ul>
                </div>
            </div>

            <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
                <div class="panel-default">
                     <div>
                          <div style="display: none">
                                 <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                 <asp:TextBox ID="txtCodUnico" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
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
                            <div class="carousel-inner" style="height:150px;">
                                <div class="item active">
                                    <img src="../carrusel/imagenes/imgContratos1.jpg">
                                    <div class="carousel-caption">
                                    </div>
                                </div>

                                <div class="item ">
                                    <img src="../carrusel/imagenes/imgContratos2.jpg">
                                    <div class="carousel-caption">
                                    </div>
                                </div>

                                <div class="item ">
                                    <img src="../carrusel/imagenes/imgContratos3.jpg">
                                    <div class="carousel-caption">
                                    </div>
                                </div>

                                <div class="item ">
                                    <img src="../carrusel/imagenes/imgContratos4.jpg">
                                    <div class="carousel-caption">
                                    </div>
                                </div>

                                <div class="item ">
                                    <img src="../carrusel/imagenes/imgContratos5.jpg">
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

                     <!-- ******************************************************************** -->
                     <!--                    Menu de Seleccion principal                       -->
                     <!-- ******************************************************************** -->    
                    <div id="menuActionProyectos" style="display: block; margin: 1.2rem 0 0 0;">
                    <h1>Gestiona tus proyectos</h1>
                    <h4 class="subtitle">Elige una opción para comenzar</h4>
                    <section class="card-container">

                        <!-- Card izquierda -->
                        <article class="card" style="background-image: url('../carrusel/imagenes/vista2.jpg');">
                            <!-- Texto visible siempre -->
                            <h2 class="card-title">Crear/Editar Proyecto</h2>
            
                            <!-- Overlay que aparece solo en hover -->
                            <div class="card-overlay">
                                <h4>Ingresa a este espacio para crear un nuevo proyecto o editar uno existente.</h4>
                                <button id="btnCreateEditProy1" onclick="VerFormCrear()">Seleccionar</button>
                            </div>
                        </article>

                        <!-- Card derecha -->
                        <article class="card" style="background-image: url('../carrusel/imagenes/mirar2.jpg');" >
                            <!-- Texto visible siempre -->
                            <h2 class="card-title">Consultar Documentos</h2>
            
                            <!-- Overlay que aparece solo en hover -->
                            <div class="card-overlay">
                                <h4>Ingresa a este espacio para consultar los archivos cargados a un proyecto.</h4>
                                <button id="btnSearchDocs" onclick="VerConsultaDocs()">Consultar</button>
                            </div>
                        </article>
                    </section>
                </div>
                                

                    <div class="generalesInfoContratos" id="FormularioPrincipal" style="display:none;">

                            <!-- Cuadro de seleccion NUEVO o BUSCAR -->
                            <div id="pestaniaPrincipal" class="pagina" style="display: block; padding: 0.5rem 0 0.2rem 0;">        
                                <!-- Cuadro Datos del Contrato  background-color: rgba(169, 14, 14, 0.50);-->
                                <div class="well fondo-azul" style="text-align:center; margin:10px 0 0 0; padding-top: 1rem; border:none;">
                                    <div class="well-header" style="margin-top: 0;">
                                        <h2 class="well-title" style="color:aliceblue;">Menu Proyectos</h2>
                                    </div>
                                    <!-- Botones agregados -->
                                    <div style="margin-top: 20px;">
                                        <button id="boton1" type="button" class="btn btn-primary" onclick="manejarBoton(1,'','')">Nuevo</button>
<%--                                        <button id="boton2" type="button" class="btn btn-secondary" onclick="manejarBoton('', 2)">Buscar y Editar</button>--%>
                                        <button type="button" class="btn btn-secondary" onclick="MostrarModalConsulta()">
                                            Buscar y Editar
                                        </button>
                                    </div>
                                </div>
                            </div>

                            <!-- Contenido de la PARTE 1 "Datos del Contrato" -->
                            <div id="pestaniaRoja" class="pagina" style="display: none; padding: 0.5rem 0 0.2rem 0;">                         
                                                        
                                <!-- Cuadro Datos del Contrato  background-color: rgba(169, 14, 14, 0.50);-->
                                <div class="well fondo-dorado" style="text-align:center;  margin:10px 0 0 0; padding-top: 1rem; border:none;">

                                    <div class="well-header" style="margin-top: 0; margin-bottom:2rem;">
                                        <h2 class="well-title" style="color:black;">Información del Contrato</h2>
                                    </div>

                                    <div style="width:30%; display:inline-block; padding:0 0; background-color:#0e0f0fab; border-radius:8px;"> 
                                        <h3 style="margin-top:0;"><a style="color:cornflowerblue; text-decoration:none; text-align:center;" href="https://www.zeitverschiebung.net/es/city/3652462"><br />Quito, Ecuador</a></h3> 
                                        <iframe src="https://www.zeitverschiebung.net/clock-widget-iframe-v2?language=es&size=medium&timezone=America%2FGuayaquil" width="100%" height="115" frameborder="0" seamless></iframe> 
                                    </div>

                                    <div class="horizontal-cuadriculado-flow">
                                         <div class="horizontal-group-contrato">
                                             <div class="input-group col-sm-9" style=" margin-top:4rem; margin-bottom:0;">
                                                  <span class="input-azulobscuro input-group-addon ingresosTitulos" style="width: 18.5rem; color:aliceblue;"><i class="glyphicon glyphicon-user"></i>  Cliente:</span>
                                                  <input id="txtCliente" type="text" class="form-control" placeholder="Cliente" oninput="convertirAMayusculas(this),BuscarCliente2()" name="cliente">                                                  <i class="glyphicon glyphicon-asterisk asterisk" aria-hidden="true"></i>
                                                  <ul class="typeahead dropdown-menu" role="listbox" style="left: 25%; cursor:pointer;" id="comboClientes2">
                                                  </ul>
                                                  <p class="help-block"></p>                                                 
                                             </div>                                                
                                            <!--div class="input-group col-sm-12">
                                                <textarea id="txtClienteObs" rows="3" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </!--div-->                                      
                                        </div>  
                                    </div>
                                                                        

                                    <div class="horizontal-cuadriculado-flow">

                                        <div class="horizontal-group-contrato">
                                            <div class="" style="width:100%; margin-right:3rem;"> 
                                                 <div class="input-group" style=" margin-bottom:0.5rem; margin-top:4rem;">                                              
                                                     <span class="select-dorado input-group-addon ingresosTitulos" style="color:#333;"><i class="glyphicon glyphicon-list-alt"></i>  Número de Contrato:</span>
                                                     <input id="txtNumContrato" type="text" class="form-control" placeholder="Núm Contrato" oninput="convertirAMayusculas(this)" value="">
                                                     <i class="glyphicon glyphicon-asterisk asterisk" aria-hidden="true"></i>
                                                 </div>                                                 
                                             </div>

                                            <div class="" style="width:70%;"> 
                                                 <div class="input-group col-sm-12" style=" margin-bottom:0.5rem; margin-top:4rem;">                                              
                                                     <span class="input-group-addon ingresosTitulos" style="width:18rem; background-color:#06313f; color:aliceblue;"><i class="glyphicon glyphicon-hourglass"></i>  Pedido:</span>
                                                     <input id="txtConPedido" type="text" class="form-control" placeholder="Num Pedido" oninput="convertirAMayusculas(this)" value="" >                                                     
                                                 </div>
                                             </div>                                             
                                        </div>
                                        <div class="horizontal-group-contrato">
                                            <div class="input-group col-sm-12">
                                                <textarea id="txtNumContratoObs" rows="3" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>
                                        </div>
                                        
                                    </div>
                            
                                    <div class="horizontal-cuadriculado-flow">

                                        <div class="horizontal-group-contrato">
                                            
                                             <div class="" style="width:100%; margin-right:3rem;">
                                                 <div class="input-group" style=" margin-bottom:0.5rem; margin-top:4rem;">                                              
                                                     <span class="select-dorado input-group-addon ingresosTitulos" style="color:#333;"><i class="glyphicon glyphicon-usd"></i>  Valor total del Contrato:</span>
                                                     <div class="" style="display:flex;">
                                                        <input id="txtValorContrato" name="txtValorContrato" type="text" class="form-control" placeholder="$" oninput="calcularPorcentaje();formatearValor(this)" required>
                                                        <i class="glyphicon glyphicon-asterisk asterisk" aria-hidden="true"></i>
                                                        <span style="font-size:20px; font-weight:700; color:black; margin-left:0.5rem; margin-right:0.5rem; padding-bottom:0;">,</span><!-- Input para los decimales -->
                                                        <input id="txtDecimales" type="text" class="form-control" placeholder="00" maxlength="2" style="width:50%;">   
                                                     </div>
                                                 </div>                                                 
                                             </div>

                                            <div class="" style="width:70%;">
                                                 <div class="input-group col-sm-12" style=" margin-bottom:0.5rem; margin-top:4rem;">                                              
                                                     <span class="input-group-addon ingresosTitulos" style="width:18rem; background-color:#06313f; color:aliceblue;"><i class="glyphicon glyphicon-flash"></i>  Margen:</span>
                                                    <input id="txtConMargen" type="number" class="form-control" placeholder="" value="" step="0.01">
                                                 </div>
                                             </div>
                                        </div>

                                        <div class="horizontal-group-contrato">
                                            <div class="input-group col-sm-12">
                                                 <textarea id="txtValorContratoObs" rows="3" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>
                                        </div>
                                 
                                    </div>

                                    <!-- fechas -->     
                                <div id="pestaniaFechas" class="pagina" style="margin:2rem; border:none;">
                                    <hr style="width:85%; height:2px; background-color:#0a61a8;" /> 
                                   <div class="horizontal-group-around" style="margin-top:1rem;">   
                                        <div class="col-sm-3 text-center input-group centrado" >   
                                            <label class="control-label ingresosTitulos"><i class="glyphicon glyphicon-leaf" style="margin-right:0.5rem;"></i> Fecha de suscripción contrato: </label>
                                            <input type="date" class="form-control text-center" id="fechaSuscripContrato" >
                                        </div>
                                     <div class="col-sm-3 text-center input-group centrado">   
                                            <label class="control-label ingresosTitulos"><i class="glyphicon glyphicon-leaf" style="margin-right:0.5rem;"></i> Fecha de notificación anticipo: </label>
                                            <input type="date" class="form-control text-center" id="fechaNotifAnticipo">
                                        </div>
                                    </div>

                                    <div class="horizontal-group-around" style="margin-top:3rem;">
                                        <div class="col-sm-4 text-center input-group centrado" style="">   
                                            <label class="control-label ingresosTitulos"><i class="glyphicon glyphicon-leaf" style="margin-right:0.5rem;"></i> Fecha inicio activación garantía fabricante:</label>
                                            <input type="date" class="form-control text-center" style="width:75%;" id="fechaIniActivacion">
                                        </div>
                                        <div class="col-sm-4 text-center input-group centrado" style="">   
                                            <label class="control-label ingresosTitulos"><i class="glyphicon glyphicon-leaf" style="margin-right:0.5rem;"></i> Fecha fin activación garantía fabricante:</label>
                                            <input type="date" class="form-control text-center" style="width:75%;" id="fechaFinActivacion" value="">
                                        </div>
                                    </div>

                                    <%--<div class="horizontal-group-around" style="margin-top:4rem;">                            
                                        <div class="input-group col-sm-6">
                                            <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 50%;"><i class="glyphicon glyphicon-user"></i> Plazo activación garantía:</span>
                                            <input id="txtPlazoActGarantia" type="text" class="form-control" placeholder="Plazo" oninput="convertirAMayusculas(this)" value="">
                                        </div>
                                    </div>      
                        
                                    <div class="horizontal-group-around" style="margin-top:4rem;margin-bottom:2rem;">                            
                                        <div class="input-group col-sm-6">
                                            <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 50%"><i class="glyphicon glyphicon-user"></i> Plazo activación licenciamiento:</span>
                                            <input id="txtPlazoActLicencia" type="text" class="form-control" placeholder="Plazo" oninput="convertirAMayusculas(this)" value="">
                                        </div>
                                    </div>  
                        
                                    <div class="horizontal-group-around" style="margin-top:4rem; margin-bottom:4rem;">
                                        <div class="input-group col-sm-6">
                                            <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 50%"><i class="glyphicon glyphicon-user"></i> Duración vigencia tecnológica:</span>
                                            <input id="txtDuracionVigTec" type="text" class="form-control" placeholder="Ingrese la duración de vigencia" oninput="convertirAMayusculas(this)" value="">
                                        </div>
                                    </div>--%>

                                    <%--<div class="horizontal-group-around" style="margin-top:4rem; margin-bottom:3rem;">                            
                                        <div class="input-group horizontal-group col-sm-4" style="margin-top:0; margin-bottom:0; padding-bottom:0; padding-top:0">
                                            <label class="ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Entrega de Licencias temporales: </label>
                                            <div class="horizontal-group" style="margin-top:0; margin-bottom:0; padding-bottom:0; padding-top:0">
                                                <div class="btn-group" role="group">
                                                    <button type="button" id="siLicenciaTemporales" class="btn btn-custom" onclick="toggleButtonColor('siLicenciaTemporales')">Sí</button>
                                                    <button type="button" id="noLicenciaTemporales" class="btn btn-custom" onclick="toggleButtonColor('noLicenciaTemporales')">No</button>
                                                </div>
                                            </div> 
                                        </div>
                                    </div>--%>
                                    <hr style="width:85%; height:2px; background-color:#0a61a8;" />                                                   
                         
                                </div>  
                                    

                                    <div class="well well-toggle box" onclick="toggleContent(this)" id="" style="background-color:#02505C; padding:14px; margin-top:2rem; margin-bottom:3rem; border:none;">
                                        <label class="control-label ingresosTitulos label-principal" style="color:white;"><i class="glyphicon glyphicon-tag" style="margin-right:0.5rem;"></i> Objeto: </label>                                        
                                         <div class="contenedor-horizontal contenido-oculto" onclick="event.stopPropagation()" style="padding-top:0;">                                          
                                             <div class="input-group col-sm-12" style=" margin-bottom:1rem; margin-top:1rem;">
                                                 <span class="input-azulmedio input-group-addon input-basic3 ingresosTitulos" style="width:16%;"><i class="glyphicon glyphicon-tag"></i>  Objeto:</span>
                                                 <textarea id="txtObjeto" rows="6" type="text" class="form-control" placeholder="Objeto" required></textarea>
                                                 <i class="glyphicon glyphicon-asterisk asterisk" aria-hidden="true"></i>
                                             </div>
                                             <div class="input-group col-sm-12">
                                                 <textarea id="txtObjetoObs" rows="3" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                             </div>

                                         </div>
                                    </div>


                                    <div class="well well-toggle box" onclick="toggleContent(this)" id="" style="background-color:#4A8699; padding:14px; margin-bottom:3rem; border:none;">
                                        <label class="control-label label-principal"><i class="glyphicon glyphicon-fire" style="margin-right:0.5rem;"></i> Servicios DOS: </label>
                                        <div class="contenedor-horizontal contenido-oculto" onclick="event.stopPropagation()" style="padding-top:0;">                                           
                                            <div class="horizontal-group-simple">
                                                <label class="control-label" style="color:#01397ef2;"><i class="glyphicon glyphicon-fire" style="margin-right:0.5rem;"></i> Servicios DOS: </label> 
                                                <%--<div class="form-group horizontal-group-simple" style="text-align:right; margin-top:0; margin-bottom:0;">
                                                    <label class="control-label ingresosTitulos" style="color:#01397ef2; margin-right:2rem;"><i class="glyphicon glyphicon-question-sign"></i> Necesita cargar un archivo? </label>
                                                    <div class="btn-group" role="group">
                                                        <button type="button" id="siServiciosDOS" class="btn btn-custom" onclick="toggleButtonColor('siServiciosDOS')">Sí</button>
                                                        <button type="button" id="noServiciosDOS" class="btn btn-custom" onclick="toggleButtonColor('noServiciosDOS')">No</button>
                                                    </div>
                                                </div>--%>
                                            </div>
                                            <div class="input-group col-sm-12">
                                                <textarea id="txtServiciosDOSObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Ingrese Servicios DOS" value=""></textarea>
                                            </div>
                                        </div>
                                    </div>


                                    <div class="well well-toggle box" onclick="toggleContent(this)" style="background-color:#369f98; padding:14px; margin-bottom:3rem; border:none;">
                                        <label class="control-label ingresosTitulos label-principal"style="color:white;"><i class="glyphicon glyphicon-leaf" style="margin-right:0.5rem;"></i> Servicios Externos: </label>
                                        <div class="contenedor-horizontal contenido-oculto" onclick="event.stopPropagation()" style="padding-top:0; margin-bottom:2rem;">                                            
                                            <div class="horizontal-group-simple">
                                                <label class="control-label ingresosTitulos" style="color:#01397ef2;"><i class="glyphicon glyphicon-leaf" style="margin-right:0.5rem;"></i> Servicios Externos: </label>                                                 
                                            </div>
                                             <div class="contenedor-horizontal">    
                                                <div style="margin-bottom:1rem;">    
                                                    <table id="dynamicTableServiciosExt" class="dynamic-tableOtra" style="background-color:rgba(50, 55, 50, 0.4);">
                                                        <thead>
                                                            <tr style="background-color:black; color:darkturquoise;">
                                                                <th style="width:70%;">Descripción</th>                                                                
                                                                <th style="width:20%;">Valor ($)</th>
                                                                <th style="width:10%;">Acciones</th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                        </tbody>                                                
                                                    </table>    
                                                </div>
                                                 <div class="horizontal-group-simple" style=" margin-bottom:0;">
                                                    <button id="addRowButtonServExt" class="dynamic-button"  style="margin-bottom:0;">+ Agregar servicio</button>
                                                     <p id="sumaValueSerExt" style="margin-right:17%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                                </div>                                 
                                             </div>
                                            <div class="contenedor-horizontal input-group col-sm-12" style="margin-top:1rem;">
                                                <textarea id="txtServiciosExternosObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Detalle" value=""></textarea>
                                            </div>
                                        </div>
                                    </div>


                                    <div class="well well-toggle box" onclick="toggleContent(this)" style="background-color:#5f7c8f; padding:14px; margin-bottom:3rem; border:none;">
                                        <label class="control-label ingresosTitulos label-principal" style="color:white;"><i class="glyphicon glyphicon-signal" style="margin-right:0.5rem;"></i> Costeo: </label>
                                        <div class="contenedor-horizontal contenido-oculto" onclick="event.stopPropagation()" style="padding-top:0; margin-bottom:2rem;">
                                            
                                            <div class="horizontal-group-simple">
                                                <div class="input-group col-sm-6" style=" margin-bottom:0.5rem; margin-top:1rem;">
                                                     <span class="input-azulmedio input-group-addon input-basic3 ingresosTitulos"><i class="glyphicon glyphicon-signal" style="margin-right:0.5rem;"></i>  Costeo:</span>
                                                     <%--<input id="txtAlcance" type="text" class="form-control" placeholder="Monto" oninput="convertirAMayusculas(this); formatearValorGeneral(this);" value="">--%>                                                    
                                                 </div>                                                
                                             </div>
                                             <div class="contenedor-horizontal">    
                                                <div style="margin-bottom:0;">    
                                                    <table id="dynamicTableCosteo" class="dynamic-tableOtra" style="background-color:rgba(15, 15, 15, 0.15);">
                                                        <thead>
                                                            <tr style="background-color:black; color:darkturquoise;">
                                                                <th style="width:15%;">OS</th>
                                                                <th style="width:60%;">Detalle</th>
                                                                <th style="width:15%;">Valor ($)</th>
                                                                <th style="width:10%;">Eliminar</th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                        </tbody>                                                
                                                    </table>    
                                                </div>
                                                 <div class="horizontal-group-simple" style="margin-top:0;">
                                                    <button id="addRowButtonCosteo" class="dynamic-button"  style="margin-bottom:0;">+ Agregar linea</button>
                                                     <p id="sumaValueCosteo" style="margin-right:17%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                                </div>  
                                             </div>    
                                            <div class="contenedor-horizontal input-group col-sm-12" style="margin-top:1rem;">
                                                <textarea id="txtAlcanceObjetoObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Detalle"></textarea>
                                            </div>
                                        </div>                                        
                                    </div>


                                    <div class="well well-toggle box" onclick="toggleContent(this)" style="background-color:#02505C; padding:14px; margin-bottom:3rem; border:none;">
                                        <label class="control-label ingresosTitulos label-principal" style="color:white;"><i class="glyphicon glyphicon-inbox" style="margin-right:0.5rem;"></i> Hardware </label>
                                        <div class="contenedor-horizontal contenido-oculto" onclick="event.stopPropagation()" style="padding-top:0;">                                            
                                            <div class="horizontal-group-simple">
                                                <label class="control-label ingresosTitulos" style="color:#01397ef2; margin-left:1rem; margin-bottom:1rem;"><i class="glyphicon glyphicon-inbox" style="margin-right:0.5rem;"></i> Hardware: </label>  
                                            </div>
                                             <div class="contenedor-horizontal">    
                                                <div style="margin-bottom:0.2rem;">    
                                                    <table id="dynamicTableHwd" class="dynamic-tableOtra" style="background-color:rgba(50, 50, 50, 0.2);">
                                                        <thead>
                                                            <tr style="background-color:black; color:darkturquoise;">
                                                                <th style="width:15%;">Cantidad</th>
                                                                <th style="width:50%;">Detalle</th>
                                                                <th style="width:25%;">Valor ($)</th>
                                                                <th style="width:10%;">Eliminar</th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                        </tbody>                                                
                                                    </table>    
                                                </div>
                                                 <div class="horizontal-group-simple" style="">
                                                    <button id="addRowButtonHwd" class="dynamic-button"  style="margin-bottom:0;">+ Agregar Hardware</button>
                                                     <p id="sumaValueSerHwd" style="margin-right:17%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                                </div>                                 
                                             </div>
                                            <div class="contenedor-horizontal input-group col-sm-12" style="margin-top:1rem;">
                                                <textarea id="txtHardwareObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>
                                        </div>
                                    </div>


                                    <div class="well well-toggle box" onclick="toggleContent(this)" style="background-color:#4A8699; padding:14px; margin-bottom:3rem; border:none;">
                                        <label class="control-label ingresosTitulos label-principal" style="color:white;"><i class="glyphicon glyphicon-tags" style="margin-right:0.5rem;"></i> Licencias </label>
                                        <div class="contenedor-horizontal contenido-oculto" onclick="event.stopPropagation()" style="padding-top:0; ">                                            
                                            <div class="horizontal-group-simple">
                                                <label class="control-label ingresosTitulos" style="color:#01397ef2;margin-left:1rem; margin-bottom:1rem;"><i class="glyphicon glyphicon-tags" style="margin-right:0.5rem;"></i> Licencias: </label>                                                  
                                            </div>
                                             <div class="contenedor-horizontal">    
                                                <div style="margin-bottom:0.2rem;">    
                                                    <table id="dynamicTableLicencias" class="dynamic-tableOtra" style="background-color:rgba(50, 55, 50, 0.2);">
                                                        <thead>
                                                            <tr style="background-color:black; color:darkturquoise;">
                                                                <th style="width:10%;">Cantidad</th>
                                                                <th style="width:60%;">Detalle</th>
                                                                <th style="width:20%;">Valor ($)</th>
                                                                <th style="width:10%;">Eliminar</th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                        </tbody>                                                
                                                    </table>    
                                                </div>
                                                 <div class="horizontal-group-simple" style="">
                                                    <button id="addRowButtonLicencias" class="dynamic-button"  style="margin-bottom:0;">+ Agregar licencia</button>
                                                     <p id="sumaValueSerLic" style="margin-right:17%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                                </div>                                 
                                             </div>
                                            <div class="contenedor-horizontal input-group col-sm-12" style="margin-top:1rem;">
                                                <textarea id="txtLicenciasObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>
                                        </div>
                                    </div>


                                    <div class="well well-toggle box" onclick="toggleContent(this)" style="background-color:#369f98; padding:14px; margin-bottom:3rem; border:none;">
                                        <label class="control-label ingresosTitulos label-principal"style="color:white;"><i class="glyphicon glyphicon-shopping-cart" style="margin-right:0.5rem;"></i> Servicios de fabricante </label>
                                        <div class="contenedor-horizontal contenido-oculto" onclick="event.stopPropagation()" style="padding-top:0;">                                            
                                            
                                            <div class="horizontal-group-simple">
                                                <label class="control-label ingresosTitulos" style="color:#01397ef2; margin-bottom:1rem;"><i class="glyphicon glyphicon-shopping-cart" style="margin-right:0.5rem;"></i> Servicios de fabricante: </label>  
                                                     <!--input id="txtAlcance" type="text" class="form-control" placeholder="Alcance" oninput="convertirAMayusculas(this)" value=""-->                                                    
                                            </div>
                                             <div class="contenedor-horizontal">    
                                                <div style="margin-bottom:0;">    
                                                    <table id="dynamicTableServFab" class="dynamic-tableOtra" style="background-color:rgba(50, 55, 60, 0.25);">
                                                        <thead>
                                                            <tr style="background-color:black; color:darkturquoise;">
                                                                <th style="width:10%;">Cantidad</th>
                                                                <th style="width:60%;">Descripción</th>
                                                                <th style="width:20%;">Valor ($)</th>
                                                                <th style="width:10%;">Eliminar</th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                        </tbody>                                                
                                                    </table>    
                                                </div>
                                                 <div class="horizontal-group-simple" style="margin-top:0;">
                                                    <button id="addRowButtonServFab" class="dynamic-button"  style="margin-bottom:0;">+ Agregar linea</button>
                                                     <p id="sumaValueServFab" style="margin-right:17%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                                </div>  
                                             </div>    
                                            <div class="contenedor-horizontal input-group col-sm-12" style="margin-top:1rem;">
                                                <textarea id="txtServFabObs" rows="3" style="width: 100%; resize: vertical;" placeholder="Detalle"></textarea>
                                            </div>
                                        </div>                                        
                                    </div>                                   
                                    

                                    <div class="well well-toggle box" onclick="toggleContent(this)" style="background-color:#5f7c8f; padding:14px; margin-bottom:3rem; border:none;">
                                        <label class="control-label ingresosTitulos label-principal" style="color:white;"><i class="glyphicon glyphicon-briefcase" style="margin-right:0.5rem;"></i> Póliza / Garantía bancaria </label>
                                        <div class="contenedor-horizontal contenido-oculto" onclick="event.stopPropagation()" style="padding-top:0;">                                            
                                            <div class="horizontal-group-simple">
                                                <div class="input-group col-sm-6" style=" margin-bottom:0.5rem; margin-top:1rem;">
                                                     <span class="input-azulmedio input-group-addon input-basic3 ingresosTitulos"><i class="glyphicon glyphicon-briefcase" style="margin-right:0.5rem;"></i> Póliza / Garantía bancaria:</span>
                                                     <%--<input id="txtPolizaMonto" type="text" class="form-control" placeholder="Monto" oninput="convertirAMayusculas(this)" value="">--%>                                                                 
                                                </div>    
                                                <div class="" style="text-align: center">
                                                    <button id="btnConsulta" onclick="BtnConsultaPoliza()" type="button" class="btn btn-primary">Consultar</button>
                                                </div>
                                                <div class="horizontal-group-simple" style="margin-top:0; justify-content:end;">
                                                     <button id="addRowButtonPoliza" class="dynamic-button"  style="margin-bottom:0;">+ Agregar linea</button>
                                                </div> 
                                            </div>
                                             <div class="contenedor-horizontal">    
                                                <div style="margin-bottom:0.2rem;">    
                                                    <table id="dynamicTablePoliza" class="dynamic-tableOtra" style="background-color:rgba(50, 55, 60, 0.25);">
                                                        <thead>
                                                            <tr style="background-color:black; color:darkturquoise;">
                                                                <th style="width:37%;">Detalle</th>
                                                                <th style="width:17%;">Tipo</th>
                                                                <th style="width:12%; font-size:12px;">Fecha Emision</th>
                                                                <th style="width:12%; font-size:12px;">Fecha Caducidad</th>
                                                                <th style="width:14%;">Valor ($)</th>
                                                                <th style="width:5%;">❌</th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                        </tbody>                                                
                                                    </table>    
                                                </div>      
                                                 <div class="horizontal-group-simple" style="margin-top:0; justify-content:end;">
                                                     <p id="sumaValuePoliza" style="margin-right:10%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                                </div>  
                                             </div>    
                                            <div class="contenedor-horizontal input-group col-sm-12" style="margin-top:1.5rem; border:none;">
                                                <textarea id="txtPolizasObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>
                                        </div>                                        
                                    </div>

                            
                                    <!--div class="horizontal-cuadriculado-flow">
                                        <div class="contenedor-horizontal marg-2">  
                                            <div class="horizontal-group" style=" margin-bottom:0; margin-top:0; padding-bottom:0;">

                                                <div class="input-group horizontal-group col-sm-5" style=" margin-bottom:0; margin-top:0; padding-bottom:0;">
                                                    <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Términos de referencia - TDR's: </label>
                                                    <div class="form-group horizontal-group">
                                                        <div class="btn-group" role="group">
                                                            <button type="button" id="siTDR" class="btn btn-custom" onclick="toggleButtonColor('siTDR')">Sí</button>
                                                            <button type="button" id="noTDR" class="btn btn-custom" onclick="toggleButtonColor('noTDR')">No</button>  
                                                        </div>
                                                    </div> 
                                                </div>   
                                            </div>

                                            <div class="input-group col-sm-12" style=" margin-top:-1.5rem; padding-top:0;">
                                                <textarea id="txtTerminosObs" rows="6" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>
                                        </div>  
                                    </!--div-->

                                    <div class="well well-toggle box" onclick="toggleContent(this)" style="background-color:#02505C; padding:14px;">
                                        <label class="control-label ingresosTitulos label-principal" style="color:white;"><i class="glyphicon glyphicon-shopping-cart" style="margin-right:0.5rem;"></i> Formas de pago </label>
                                        
                                        <div class="contenedor-horizontal contenido-oculto" onclick="event.stopPropagation()" style="padding-top:0;">                                            
                                             <div class="horizontal-group-simple">                                             
                                                 <div class="input-group col-sm-5" style="margin-top:0;">
                                                     <span class="input-azulmedio input-group-addon ingresosTitulos"><i class="glyphicon glyphicon-credit-card"></i>  Formas de pago:</span>                                                 
                                                 </div>
                                                 <%--<div class="col-sm-6" style="margin-top:0; padding:0;">
                                                    <input id="txtOtraFormaPago" type="text" class="form-control select-temas" placeholder="Resumen de Formas de pago" oninput="convertirAMayusculas(this)" value="">
                                                 </div>--%>
                                             </div>

                                            <div class="horizontal-group-simple" style=" margin-bottom:-2rem;">
                                                <p id="remainingValue" style="margin-left:45%; color:black;">Valor restante: $0.00</p>
                                                <button id="addRowButton" class="dynamic-button"  style="margin-bottom:0;">+ Agregar forma de pago</button>
                                            </div>

                                            <div style="margin-bottom:1rem;">    
                                                <table id="dynamicTableFormas" class="dynamic-table" style="background-color:rgba(50, 55, 60, 0.4);">
                                                    <thead>
                                                        <tr style="background-color:black; color:darkturquoise;">
                                                            <th style="width:45%;">Detalle</th>                                                        
                                                            <th style="width:15%;">Porcentaje (%)</th>
                                                            <th style="width:20%;">Valor ($)</th>
                                                            <th style="width:20%;">Fecha estimada pago</th>
                                                            <th style="width:10%;">Eliminar</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                    </tbody>                                                
                                                 </table>    
                                            </div>
                                 
                                             <div class="input-group col-sm-12">
                                                 <textarea id="txtFormaPagoObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Ingrese una observacion sobre la forma de pago de ser necesario (opcional)"></textarea>
                                             </div>

                                        </div>

                                     </div>
                                                                        
                                </div>

                                

                                <!--    CUADRO PARA CARGAR LOS ARCHIVOS    -->
                                <div class="well contenedor-horizontal" style=" margin:2rem 0 0 0; padding-bottom:2rem; background-color:rgba(235, 235, 235, 0.938); border:none;">                                            
                                   <h3>Lista de Archivos</h3>
                                    <div class="containerFiles input-group col-sm-11">
                                        <div class="title-list">
                                            <button class="title-button" onclick="toggleTitle(event, this)">Contrato</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Objeto</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Servicios DOS</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Servicios Externos</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Costeo</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Hardware</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Licencias</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Servicios de fabricante</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Poliza/Garantía bancaria</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Formas de pago</button>                                            
                                            <button class="title-button" onclick="toggleTitle(event, this)">Términos de referencia - TDR's</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Acta preguntas y respuestas</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Acta de adjudicación</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Acta de negociación</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">BoM Solución</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Nombre de Mayorista - Acuerdos</button>
                                            <button class="title-button" onclick="toggleTitle(event, this)">Garantías y Licencias Técnicas</button>
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

                                <!-- Tabla de busqueda -->
                                 <div class="well" id="tblaArchivos" style="display:none; margin:2rem 0 0 0; padding-top: 1rem; padding-bottom:3rem; background-color:rgba(235, 235, 235, 0.938); border:none;">
                                    <div class="">
                                        <h3 class="box-title ">Lista de archivos</h3>
                                        <div class="">
                                            <table id="tbl_Archivos" class="dynamic-table table-hover table-bordered sm-12" 
                                                   style="width:100%; border-radius: 6px; overflow: hidden; background: rgb(0 0 0 / 80%); color: white;">
                                                <thead class="text-white text-center" style="background: linear-gradient(to right, #0353cd, #5900c9);">
                                                    <tr>
                                                        <th style="width: 85%;">Nombre</th>
                                                        <th style="width: 15%;">Acciones</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <!-- DATA POR MEDIO DE AJAX -->
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>




                                <!-- Cuadro Datos de contacto -->
                                <div class="well" style=" margin:2rem 0 0 0; padding-top: 1rem; padding-bottom:3rem; background-color:rgba(235, 235, 235, 0.938); border:none;">

                                        <div class="well-header" style="margin-top: 0;">
                                            <div>
                                                <h4 class="well-title" style="text-align:left;">Datos contacto</h4>
                                            </div>
                                        </div>
                                        <div class="contenedor-horizontal">
                                     
                                             <div class="horizontal-group-special">
                                                <div class="input-group col-sm-12">
                                                     <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 20%; text-align:left;"><i class="glyphicon glyphicon-user"></i>  Nombre:</span>
                                                    <input id="txtNomContacto" type="text" class="form-control" name="Nombre" placeholder="Nombre" oninput="convertirAMayusculas(this)" value="">
                                                 </div>
                                            </div>
                                             <div class="horizontal-group-special" style="margin-top: 1rem;">
                                                <div class="input-group col-sm-12">
                                                     <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 20%; text-align:left;"><i class="glyphicon glyphicon-phone-alt"></i>  Teléfono:</span>
                                                    <input id="txtTelefono" type="text" class="form-control" name="telefono" placeholder="Teléfono" value="">
                                                 </div>
                                            </div>
                                            <div class="horizontal-group-special" style="margin-top: 1rem;">
                                                <div class="input-group col-sm-12">
                                                     <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 20%; text-align:left;"><i class="glyphicon glyphicon-globe"></i>  Dirección:</span>
                                                     <input id="txtDireccion" type="text" class="form-control" name="direccion" placeholder="Dirección" value="">
                                                 </div>
                                            </div>
                                            <div class="horizontal-group-special" style="margin-top: 1rem;">
                                                <div class="input-group col-sm-12">
                                                     <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 20%; text-align:left;"><i class="glyphicon glyphicon-envelope"></i>  Correo:</span>
                                                     <input id="txtCorreo" type="text" class="form-control" name="correo" placeholder="Correo" value="">
                                                 </div>
                                            </div>

                                        </div>  
                                    </div>

                                <!-- Cuadro para ingreso de Correos -->
                                <div class="well" style=" margin:2rem 0 0 0;padding:1rem 10rem 2rem 10rem; background-color:rgb(30 71 105 / 40%); border:none;">

                                        <div class="well-header" style="margin-top: 0; color:white;">
                                            <div>
                                                <h4 class="well-title" style="text-align:left;">Ingrese el correo de las personas que recibirán la notificación </h4>
                                            </div>
                                        </div>
                                        <div class="contenedor-horizontal">
                                                                                                                             
                                            <div style="margin-top: 1rem;">
                                                <!-- Contenedor para los correos -->
                                                <div id="contenedorCorreos">
                                                    <div class="input-group col-sm-12 correo-item" style="margin-top: 0.5rem;">
                                                        <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 20%; text-align:left;">
                                                            <i class="glyphicon glyphicon-envelope"></i> Correo:
                                                        </span>
                                                        <input id="txtCorreoNotificacion1" type="text" class="form-control" name="correo[]" placeholder="Correo">
                                                        <span class="input-group-addon" style="cursor: pointer;" title="Eliminar" onclick="this.parentElement.remove();">
                                                            <i class="glyphicon glyphicon-trash text-danger"></i>
                                                        </span>
                                                    </div>
                                                </div>

                                                <!-- Botón con función en línea -->
                                                <div class="horizontal-group-simple" style="margin-top:1rem; margin-bottom:0;">
                                                    <button id="btnCorreos" class="dynamic-button" type="button" style="margin-bottom:0;" onclick="agregarNuevoInputCorreo()">Agregar correo</button>
                                                </div>
                                            </div>


                                        </div>  
                                 </div>

                                <div style="border:none;">
                                    <!-- Paginador -->
                                    <div class="col-sm-12" id="btn-cargaContrato1" style="display: flex; justify-content:center; margin-top:2rem;">      
                                        <div class="col-sm-6" style="margin: 3rem;">
                                            <button class="btn btn-info " type="button" id="" onclick="GuardarContrato(document.getElementById('txtConPedido').value)" style="width:100%; padding:0.7rem 0; font-size: larger; font-style: italic; color: currentColor; font-weight: bold;">Guardar Contrato</button>                                                                    
                                        </div>
                                        <div class="col-sm-6" id="btn-mostrarPDF" style="display:none; margin: 3rem;">          
                                            <button class="btn btn-danger" type="button" id="btnMostrarPdf" onclick="MostrarPDF(document.getElementById('txtNumContrato').value)" style=" width:100%; padding:0.7rem 0; font-size: larger; font-style: italic; font-weight: bold;">Visualizar PDF</button>                            
                                        </div>
                                    </div>
                                    <div id="btn-paginas1" style="display:none; margin-top:3rem; margin-bottom:1rem;">                             
                                         <ul class="pager">
                                            <li><a href="#" type="button" style="background-color:rgba(125, 125, 125, 0.7); color:honeydew" class="ingresosTitulos btn btn-danger" onclick="irAPaginaAnterior(1)" disabled> Regresar</a></li>
                                            <li><a href="#" type="button" style="background-color:rgba(123, 170, 245, 0.5); color:honeydew" class="ingresosTitulos btn" onclick="irAPaginaSiguiente(1)" >Siguiente</a></li>
                                         </ul>
                                     </div>                                    
                                </div>

                                                          
                                <!-- Cuadro de Calculos  " -->
                                <div class="well" style="display:none; background-color: #d8e4e9; margin-top: 0; padding-top: 1rem;">
                                    <div class="well-header" style="margin-top: 0;">
                                        <h4 class="well-title">CALCULOS DATOS</h4>
                                    </div>

                                    <div class="row">

                                        <div class="well well-sm" style="background-color: #d8e4e9; margin: 1rem 2rem; padding-top: 1rem;">

                                            <div class="well" style="margin-top: 0; padding-top: 1rem;">
                                                <div class="well-header" style="margin-top: 0;">
                                                    <div>
                                                        <p class="well-title">Información de contrato</p>
                                                    </div>
                                                </div>
                                                <div class="row" style="margin-left: 2rem;">

                                                    <div class="input-group col-sm-6" style="margin-bottom: 10px;">
                                                        <span class="input-group-addon" style="width: 40%;"><i class="glyphicon glyphicon-user"></i>Porcentaje multa diaria:</span>
                                                        <input id="txtPorcentajeMulta" oninput="calcularMulta()" type="number" class="form-control" placeholder="%" value="">
                                                    </div>
                                                    <div class="input-group col-sm-6" style="margin-bottom: 10px;">
                                                        <span class="input-group-addon" style="width: 40%;"><i class="glyphicon glyphicon-user"></i>Resultado Valor total:</span>
                                                        <input id="txtResultadoCinco" type="text" class="form-control" value="">
                                                    </div>

                                                    <div class="col-sm-9 horizontal-group form-group row">
                                                        <label for="disabledTextInput" class="col-sm-6 col-form-label">MONTO MULTA DIARIA: </label>
                                                        <label id="txtMontoMulta" style="width: 100%;">Monto multa diaria</label>
                                                    </div>
                                                </div>

                                            </div>

                                        </div>
                                    </div>
                                </div>                                                                     
                        
                            <!-- Contenido de la pestaña 2"Archivos del Contrato  background-color: rgba(255, 208, 140, 0.8);" -->
                            <div id="pestaniaCafe" class="pagina oculto" style="display: none; padding: 0.5rem 0 0.2rem 0;">    
                                <div class="well fondo-dorado" style="margin:10px 0 0 0; padding-top: 1rem; border:none;">

                                    <div class="well-header" style="margin-top: 0;">
                                        <h3 class="well-title" style="color:aliceblue;">Información de contrato</h3>
                                    </div>                            
                                                                        

                                    <div class="horizontal-cuadriculado-flow text-center">
                                            <div class="contenedor-horizontal marg-2">  
                                                <div class="horizontal-group" style=" margin-bottom:0; margin-top:0; padding-bottom:0;">
                                                    <div class="input-group horizontal-group col-sm-5"  style=" margin-bottom:0; margin-top:1rem; padding-bottom:0;">
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Acta preguntas y respuestas: </label>
                                                        <div class="form-group horizontal-group">
                                                            <div class="btn-group" role="group">
                                                                <button type="button" id="siPreguntas" class="btn btn-custom" onclick="toggleButtonColor('siPreguntas')">Sí</button>
                                                                <button type="button" id="noPreguntas" class="btn btn-custom" onclick="toggleButtonColor('noPreguntas')">No</button>  
                                                            </div>
                                                        </div> 
                                                    </div>
                                        
                                                    <!--button id="btnActaPreguntas" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnActaPreguntas','Acta preguntas y respuestas:')"><i class="glyphicon glyphicon-link"></i> Cargar archivo</!--button-->
                                                </div>     
                                                <div class="input-group col-sm-12" style=" margin-top:-1.5rem; padding-top:0;">
                                                    <textarea id="txtActaPreguntasObs" rows="3" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                                </div>
                                            </div>  
                                        </div>

                                    <div class="horizontal-cuadriculado-flow text-center">
                                            <div class="contenedor-horizontal marg-2">  
                                                <div class="horizontal-group" style=" margin-bottom:0; margin-top:0; padding-bottom:0;">
                                                    <div class="input-group horizontal-group col-sm-5"  style=" margin-bottom:0; margin-top:1rem; padding-bottom:0;">
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Acta de adjudicación: </label>
                                                        <div class="form-group horizontal-group">
                                                            <div class="btn-group" role="group">
                                                                <button type="button" id="siAdj" class="btn btn-custom" onclick="toggleButtonColor('siAdj')">Sí</button>
                                                                <button type="button" id="noAdj" class="btn btn-custom" onclick="toggleButtonColor('noAdj')">No</button>  
                                                            </div>
                                                        </div> 
                                                    </div>
                                        
                                                    <!--button-- id="btnActaAdjudicacion" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnActaAdjudicacion','Acta de adjudicación:')"><i class="glyphicon glyphicon-link"></i> Cargar archivo</!--button-->
                                                </div>     
                                                <div class="input-group col-sm-12" style=" margin-top:-1.5rem; padding-top:0;">
                                                    <textarea id="txtActaAdjObs" rows="3" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                                </div>
                                            </div>  
                                        </div>

                                    <div class="horizontal-cuadriculado-flow text-center">
                                            <div class="contenedor-horizontal marg-2">  
                                                <div class="horizontal-group" style=" margin-bottom:0; margin-top:0; padding-bottom:0;">
                                                    <div class="input-group horizontal-group col-sm-5"  style=" margin-bottom:0; margin-top:1rem; padding-bottom:0;">
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Acta de negociación: </label>
                                                        <div class="form-group horizontal-group">
                                                            <div class="btn-group" role="group">
                                                                <button type="button" id="siNeg" class="btn btn-custom" onclick="toggleButtonColor('siNeg')">Sí</button>
                                                                <button type="button" id="noNeg" class="btn btn-custom" onclick="toggleButtonColor('noNeg')">No</button>  
                                                            </div>
                                                        </div> 
                                                    </div>
                                        
                                                    <!--button-- id="btnActaNegociacion" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnActaNegociacion','Acta de negociación:')"><i class="glyphicon glyphicon-link"></i> Cargar archivo</!--button-->
                                                </div>     
                                                <div class="input-group col-sm-12" style=" margin-top:-1.5rem; padding-top:0;">
                                                    <textarea id="txtActaNegObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                                </div>
                                            </div>  
                                        </div>

                                    <!-- Cuadro Datos de otros -->
                                    <div class="well datos-gerentes" style=" margin:3rem 1.7rem 0 1.7rem; padding-top: 1rem; padding-bottom:3rem; background-color:rgba(22, 25, 25, 0.60); border:none; ">

                                        <div class="well-header" style="margin-top: 0;">
                                            <div>
                                                <h4 class="well-title" style="color:aliceblue; text-align:left;">Datos otros</h4>
                                            </div>
                                        </div>

                                        <div class="horizontal-cuadriculado-flow">
                                             <div class="contenedor-horizontal">

                                                 <div class="horizontal-group">
                                                    <div class="input-group col-sm-6" style=" margin-bottom:0.5rem; margin-top:2rem;">
                                                         <span class="input-azulmedio input-group-addon ingresosTitulos"><i class="glyphicon glyphicon-hdd"></i>  BoM Solución:</span>
                                                         <input id="txtBomSolucion" type="text" class="form-control" placeholder="Ingrese BoM Solución" oninput="convertirAMayusculas(this)" value="">
                                                    </div>
                                                    <!--button-- id="btnBomSolucion" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnBomSolucion','BoM Solucion:')"><i class="glyphicon glyphicon-link"></i> Cargar archivo</button-->
                                                 </div>
                                                 <div class="input-group col-sm-12">
                                                     <textarea id="txtBomSolucionObs" rows="2" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                                 </div>
                                             </div> 
                                        </div>

                                         <div class="contenedor-horizontal">
                                             <div class="input-group col-sm-8" style=" margin-bottom:0.5rem; margin-top:3rem;">
                                                 <span class="input-azulmedio input-group-addon ingresosTitulos"><i class="glyphicon glyphicon-briefcase"></i>  Nombre de Mayorista - Acuerdos:</span>
                                                 <input id="txtAcuMayoristas" type="text" class="form-control" placeholder="Ingrese aquí el nombre de Mayorista" oninput="convertirAMayusculas(this)" value="">
                                             </div>
                                             <div class="input-group col-sm-12">
                                                 <textarea id="txtAcuMayoristasObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Acuerdos"></textarea>
                                             </div>
                                         </div> 

                                         <div class="contenedor-horizontal">
                                             <div class="input-group col-sm-8" style=" margin-bottom:0.5rem; margin-top:3rem;">
                                                 <span class="input-azulmedio input-group-addon input-basic3 ingresosTitulos"><i class="glyphicon glyphicon-tent"></i> Nombre de Fabricante - Acuerdos:</span>
                                                 <input id="txtAcuFabricantes" type="text" class="form-control" placeholder="Ingrese aquí el nombre de Fabricantes" oninput="convertirAMayusculas(this)" value="">
                                             </div>
                                             <div class="input-group col-sm-12">
                                                 <textarea id="txtAcuFabricantesObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Acuerdos"></textarea>
                                             </div>
                                         </div> 
                             

                                    </div>


                                     <div class="horizontal-cuadriculado-flow text-center">
                                        <div class="contenedor-horizontal marg-2">  
                                            <div class="horizontal-group" style=" margin-bottom:0; margin-top:0; padding-bottom:0;">
                                                <div class="input-group horizontal-group col-sm-5"  style=" margin-bottom:0; margin-top:1rem; padding-bottom:0;">
                                                    <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Garantías FIN: </label>
                                                    <div class="form-group horizontal-group">
                                                        <div class="btn-group" role="group">
                                                            <button type="button" id="siGarantiasFIN" class="btn btn-custom" onclick="toggleButtonColor('siGarantiasFIN')">Sí</button>
                                                            <button type="button" id="noGarantiasFIN" class="btn btn-custom" onclick="toggleButtonColor('noGarantiasFIN')">No</button>  
                                                        </div>
                                                    </div> 
                                                </div>    
                                                <!--button-- id="btnGarantiasFIN" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnGarantiasFIN','Garantías FIN:')"><i class="glyphicon glyphicon-link"></i> Cargar archivo</!--button-->                                               
                                            </div>     
                                            <div class="input-group col-sm-12" style=" margin-top:-1.5rem; padding-top:0;">
                                                <textarea id="txtGarFinObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>
                                        </div>  
                                     </div>

                                     <div class="horizontal-cuadriculado-flow text-center">
                                        <div class="contenedor-horizontal marg-2">  
                                            <div class="horizontal-group" style=" margin-bottom:0; margin-top:0; padding-bottom:0;">
                                                <div class="input-group horizontal-group col-sm-5"  style=" margin-bottom:0; margin-top:1rem; padding-bottom:0;">
                                                    <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Garantías y Licencias TEC: </label>
                                                    <div class="form-group horizontal-group">
                                                        <div class="btn-group" role="group">
                                                            <button type="button" id="siGarantiasTEC" class="btn btn-custom" onclick="toggleButtonColor('siGarantiasTEC')">Sí</button>
                                                            <button type="button" id="noGarantiasTEC" class="btn btn-custom" onclick="toggleButtonColor('noGarantiasTEC')">No</button>  
                                                        </div>
                                                    </div> 
                                                </div>                                        
                                                <!--button-- id="btnGarantiasTEC" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnGarantiasTEC','Garantías y Licencias TEC:')"><i class="glyphicon glyphicon-link"></i> Cargar archivo</!--button-->
                                            </div>     
                                            <div class="input-group col-sm-12" style=" margin-top:-1.5rem; padding-top:0;">
                                                <textarea id="txtGarTECObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>
                                        </div>  
                                     </div>

                                     <div class="horizontal-cuadriculado-flow text-center marg-2" style="margin-top:3rem;">

                                        <div class="input-group horizontal-group col-sm-10" style="display:inline-flex;">
                                            <span class="select-dorado ingresosTitulos" style="width:48%; padding:7px; color:#333;"><i class="glyphicon glyphicon-credit-card"></i> Número de Pedido:</span>
                                            <input id="txtGenPedidos" type="text" class="form-control" style="text-align:center;" placeholder="Ingrese aquí XXXXXX" oninput="convertirAMayusculas(this)" value="">
                                            <button class="btn btn-default" type="button" id="btnBuscarDatosPersonales" onclick="buscarOrdenesServicio()"><i class="glyphicon glyphicon-search"></i></button>
                                        </div>
                                        <div class="input-group col-sm-10" style="display:inline-flex;">
                                            <div style="width:45%; background-color:#7B7D7E; display: inline-table; align-items: center;">
                                                <span class="input-group-addon ingresosTitulos" style="background-color:black; color:aliceblue; justify-self:center; "><i class="glyphicon glyphicon-credit-card"></i> Ordenes Servicio:</span>
                                            </div>                                         
                                            <div id="listaOrdenes" style="position: relative; width:100%; height:100%;">
                                                <div id="listaOrdenesContent" style="position: relative; z-index: 1; background-color: rgba(0, 0, 0, 0.6); padding: 10px; width: 100%; color:aliceblue; border:solid; border-color: darkgoldenrod; border-width:thin;"></div> <!-- Establecí la posición como absolute y agregué top y left -->
                                            </div>
                                        </div>
                                        <div class="input-group col-sm-10" style="display:inline-flex;">
                                            <textarea id="txtGenPedidosObs" rows="2" style="width: 100%; resize: vertical; margin-bottom:50px;" placeholder="Detalle / Observaciones"></textarea>
                                        </div>

                                    </div>                           

                                 </div>


                            </div>

                            <!-- Contenido de la pestaña 3" Registro de pedidos  background-color: rgba(255, 208, 140, 0.8);" -->
                            <div id="pestaniaRegistro" class="pagina oculto" style="display: none; padding: 0.5rem 0 0.2rem 0;">    
                                <div class="well fondo-azul" style="margin:10px 0 0 0; padding-top: 1rem; border:none;">

                                    <div class="well-header" style="margin-top: 0;">
                                        <h3 class="well-title" style="color:aliceblue;">Registro pedidos</h3>
                                    </div>         
                                </div>
                            </div>                            
                                        
                            <div id="pestaniaCargaInfo" class="pagina oculto" style="display:none;">                      

                                    <div class="well" style="background-color:rgba(109,109,109,0); border:none;">
                                         <!-- Paginador -->
                                        <div id="btn-paginas2" style="display:block; margin-top:4rem; margin-bottom:4rem;">                             
                                             <ul class="pager">
                                                <li><a href="#" type="button" style="background-color:rgba(123, 170, 245, 0.5); color:honeydew" class="ingresosTitulos btn" onclick="irAPaginaAnterior(2)"> Regresar</a></li>
                                                <li><a href="#" type="button" style="background-color:rgba(125, 125, 125, 0.7); color:honeydew" class="ingresosTitulos btn btn-danger" onclick="irAPaginaSiguiente(2)" disabled>Siguiente</a></li>
                                             </ul>
                                         </div>
                                        <div class="datos-gerentes" id="btn-cargaContrato2" style="display: none; flex-direction: column; align-items: center;">          
                                            <button class="btn btn-info col-sm-6" type="button" id="" onclick="GuardarContrato(document.getElementById('txtNumContrato').value)" style=" margin: 4rem;">Guardar Datos</button>                            
                                        </div> 
                                        <div class="datos-gerentes" id="btn-cargaPDF" style="display:flex; flex-direction: column; align-items: center;">          
                                            <button class="btn btn-danger col-sm-6" type="button" id="" onclick="MostrarPDF(document.getElementById('txtNumContrato').value)" style=" margin: 4rem;">PDF</button>                            
                                        </div>

                                    </div>

                            </div>                                      

                       </div> <!-- FIN ingreso InfoContratos -->                                                                                 
                      
                </div> <!-- FIN Incluye los perfiles y usuarios iniciales -->


                    <!-- Tabla de consulta de Documentos Cargados -->
                    <div class="well" id="tablaDocsCargados" style="display:none; margin:2rem 0 0 0; padding-top: 1rem; padding-bottom:3rem; background-color:rgba(235, 235, 235, 0.938); border:none;">
                        <div class="">
                             <h3 class="box-title ">Lista de archivos</h3>
                             <div class="">
                                 <table id="tbl_Docs" class="dynamic-table table-hover table-bordered sm-12" 
                                        style="width:100%; border-radius: 6px; overflow: hidden; background: rgb(0 0 0 / 80%); color: white;">
                                        <thead class="text-white text-center" style="background: linear-gradient(to right, #0353cd, #5900c9);">
                                              <tr>
                                                  <th style="width: 85%;">Nombre</th>
                                                  <th style="width: 15%;">Descargar</th>
                                              </tr>
                                        </thead>
                                        <tbody>
                                            <!-- DATA POR MEDIO DE AJAX -->
                                        </tbody>
                                 </table>
                             </div>
                         </div>
                     </div>



                <!----------------------------------------------------->
                <!--        Modal para mostrar en la pagina          -->
                <!----------------------------------------------------->
                <!-- Modal para cargar un archivo -->
                <div id="msgCargarArchivos" class="modal fade" role="dialog">
                    <div class="modal-dialog">
                        <!-- Modal content-->
                        <div class="modal-content">
                            <div class="modal-header bg-info">
                                <button type="button" class="close" data-dismiss="modal">&times;</button>
                                <h4 class="modal-title">CARGA ARCHIVOS</h4><span id="txtnombreVentana"></span><span id="txtnombreArchivo" style="display:none;"></span>
                            </div>
                            <div class="modal-body">                             
                                 <formview class="horizontal-group-around" id="formCargarArchivo" enctype="multipart/form-data">
                                    <label for="myfile" style="width:30%;">Seleccionar archivo:</label>
                                    <input type="file" id="archivosAdjuntos" style="width:70%;" multiple>
                                    <br><br>                                     
                                </formview>
                                <formview>
                                    <div style="width:100%;">
                                        <progress id="fileProgress" style="display: none; width:100%;"></progress>
                                    </div>                                     
                                    <div>
                                        <span id="lblMessage" style="color: Green"></span>
                                    </div>
                                     
                                     <br>
                                     <!--button type="button" id="btnCargarArchivosAdjuntos" style="text-align: end;" >Cargar</!--button-->
                                    <input type="button" id="" value="Cargar Archivos" class="btn btn-info" onclick="confirmacionCargaArchivo(document.getElementById('txtnombreArchivo').textContent)"/>
                                        <p class="help-block"></p>
                                </formview>
                            </div>    
                        </div>
                    </div>
                </div>       
                
                
                <!-- Modal de Visualizacion de PDF -->
                <div id="ModalEgresoInventarioPDF" class="ModalPDF modal fade" role="dialog">
                    <div class="modal-dialog modal-lg"> <!-- Agrega la clase 'modal-lg' para que el modal sea grande -->
                        <div class="modal-content">
                            <div class="modal-header bg-info">
                                <button type="button" class="close" data-dismiss="modal">&times;</button>
                                <h4 class="modal-title">View</h4>
                            </div>
                            <div class="modal-body modal-body-custom">
                                <div class="container" style="margin-top:0; margin-bottom: 1rem; padding:0;">

                                    <div class="row-flex" style="margin: 0 0 1rem 0;"><!-- Cuadros 1-->            
                            
                                        <div class="div1-mitad" style="max-width:50%; padding: 0; margin: 0 0 0 0;">

                                            <div class="image-container" style=" display: grid; justify-items: center; align-items: center; padding: 0; ">
                                                <img src="../carrusel/imagenes/logo_dos_textoGris.png" style="width: 30%;" alt="logo DOS">
                                            </div>
                                        </div>
                                    </div>

                                    <div class="row" style="margin: 0 0 1rem 0;">               
                                        <div class="columna div1" style="padding-left: 0.2rem; padding-bottom:0.5rem;">                                                
                                            <div class="centered-element letra-bold" style="margin-bottom:1rem;">                                                  
                                                <span class="">COMPUTADORES Y EQUIPOS COMPUEQUIP DOS S.A.</span>                                                
                                            </div>                                                
                                            <div class="doble-columna">                                                    
                                                <span class="letra-bold" style="width: 25%;">Dirección:</span> <span id="">AV. MARISCAL SUCRE OE6-201 Y JOSE MIGUEL CARRION</span>                                                
                                            </div>                                                
                                            <div class="doble-columna">                                                   
                                                <span class="letra-bold" style="width: 25%;">Matríz: </span> <span id="">MIGUEL CARRION, QUITO - Ecuador</span>                                                
                                            </div>                                               
                                            <div class="doble-columna">                                                    
                                                <span class="letra-bold" style="width: 25%;">Sucursal:</span> <span id="">QUITO</span>                                                
                                            </div>                                               
                                            <div class="doble-columna">                                                    
                                                <span class="letra-bold" style="width: 25%;">RUC:</span> <span id="">17908851860011</span>                                                
                                            </div>
                                       </div>                                               
                                    </div>
                                
                                    <div class="row" id="informacionPDF" style="margin: 0 0 1rem 0;"> <!-- Cuadros 3-->
                                        <div class="columna div1" style="padding-left: 0.2rem; padding-bottom:0.5rem;">
                                            <div class="centered-element">
                                                <span class="letra-bold">SERVICIOS DOS</span>
                                            </div> 
                                            <div class="centered-element" style="margin-bottom:1rem;">
                                                <span class="letra-bold">INFORMACIÓN - CONTRATO:</span>
                                            </div>

                                            <div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                <span style="width: 25%;">CLIENTE:</span><span id="txtPDFCliente">-- -- --</span>
                                            </div>      
                                            
                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:1rem 1rem 0.2rem 0.5rem;">
                                                <span style="width: 25%;">NÚMERO DE CONTRATO:</span><span id="txtPDFNumeroContrato">-- -- --</span>
                                            </div>
                                            <div class="doble-columna" style="padding:1rem 1rem 0.2rem 0.5rem;">
                                                <span style="width: 25%;">NÚMERO DE PEDIDO:</span><span id="txtPDFNumeroPedido">-- -- --</span>
                                            </div>                                             
                                            <div class="doble-columna">
                                                <span style="width: 27%; padding-left:1rem;">- Observacion:</span><span id="txtPDFNumeroContratoOBS">-- -- --</span>
                                            </div>

                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:1rem 1rem 0.2rem 0.5rem;">
                                                <span style="width: 25%;">VALOR TOTAL DEL CONTRATO:</span><span id="txtPDFValorTotalContrato">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 27%; padding-left:1rem;">- Observacion:</span><span id="txtPDFValorTotalContratoOBS">-- -- --</span>
                                            </div>
                                            <div class="doble-columna" style="padding:1rem 0 0.2rem 0.5rem;">   
                                                <br />
                                                <div class="doble-columna" style="width: 50%;">                                            
                                                    <span style="width: 50%;">Porcentaje de Rentabilidad:</span><span id="txtPDFRentabilidadPorcentaje">--- ---</span>
                                                </div>
                                                <div class="flex-half doble-columna">                                        
                                                    <span style="width: 50%;">Valor de Rentabilidad:</span><span id="txtPDFRentabilidadValor">--- ---</span>
                                                </div>
                                            </div>

                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:1rem 0 0.2rem 0.5rem;">   
                                                <br />
                                                <div class="doble-columna" style="width: 50%;">                                            
                                                    <span style="width: 50%;">Fecha de suscripción contrato:</span><span id="txtPDFFechaSuscripcionContrato">DD/MM/YYYY</span>
                                                </div>
                                                <div class="flex-half doble-columna">                                        
                                                    <span style="width: 50%;">Fecha de notificación anticipo:</span><span id="txtPDFFechaNotificacionAnticipo">DD/MM/YYYY</span>
                                                </div>
                                            </div>
                                            <div class="doble-columna" style="padding:0 0 0.2rem 0.5rem;">      
                                                <br />
                                                <div class="doble-columna" style="width: 50%;">                                            
                                                    <span style="width: 50%;">Fecha inicio activación garantía fab:</span><span id="txtPDFFechaInicioGarantiaFabricante">DD/MM/YYYY</span>
                                                </div>
                                                <div class="flex-half doble-columna">                                        
                                                    <span style="width: 50%;">Fecha fin activación garantía fab:</span><span id="txtPDFFechaFinGarantiaFabricante">DD/MM/YYYY</span>
                                                </div>
                                            </div>
                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:2rem 1rem 0.2rem 0.5rem;">
                                                <span style="width: 20%;">Objeto:</span><span style="width: 80%;" id="txtPDFObjeto">-- -- --</span>
                                            </div>
                                            <div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                <span style="width: 22%; padding-left:1rem;">- Observacion:</span><span style="width: 80%;" id="txtPDFObjetoOBS">-- -- --</span>
                                            </div>

                                            <hr class="linea">
                                            <div class="doble-columna" style=" padding: 2rem 1rem 0.2rem 0.5rem;">
                                                <span style="width:20%;">Costeo:</span>
                                                <table id="tbl_pdfCosteo" class="columna div1 table table-sm" 
                                                    style="margin-bottom:0; flex: 1; border: 2px solid #5a5959; width: 100%; table-layout: fixed; border-collapse:separate;">
                                                    <thead>
                                                        <tr style="text-align: center; background-color: #e4e4e48f;">
                                                            <th style="width: 15%; padding:0 2px; white-space: nowrap; text-align:center;">OS</th>
                                                            <th style="width: 70%; padding:0 2px; white-space: nowrap; text-align:center;">Detalle</th>
                                                            <th style="width: 15%; padding:0 2px; white-space: nowrap; text-align:center;">Valor ($)</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody style="text-align: center;">
                                                        <!-- Aquí se generarán las filas dinámicamente -->
                                                    </tbody>
                                                </table>                                                
                                            </div>                                            
                                            <div class="horizontal-group-simple" style="margin-top:0;justify-content:end;">
                                                 <p id="sumaValueCosteoPDF" style="margin-right:5%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                            </div> 
                                            <div class="doble-columna">
                                                <span style="width: 22%; padding-left:1rem;">- Observacion:</span><span style="width: 78%;" id="txtPDFAlcanceOBS">-- -- --</span>
                                            </div>                                                                                       
                                                                                        

                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:2rem 1rem 0.2rem 0.5rem;">
                                                <%--<span style="width: 20%;">Hardware:</span><span style="width: 80%;" id="txtPDFHardware">-- -- --</span>--%>
                                                <span style="width: 20%;">Hardware:</span>
                                                <table id="tbl_pdfHardware" class="columna div1 table table-sm" 
                                                    style="margin-bottom:0; flex: 1; border: 2px solid #5a5959; width: 100%; table-layout: fixed; border-collapse:separate; page-break-inside: avoid !important;break-inside: avoid !important;">
                                                    <thead>
                                                        <tr style="text-align: center; background-color: #e4e4e48f; page-break-inside: avoid !important; break-inside: avoid !important;">
                                                            <th style="width: 10%; padding:0 2px; white-space: nowrap; text-align:center;">Cantidad</th>
                                                            <th style="width: 70%; padding:0 2px; white-space: nowrap; text-align:center;">Detalle</th>
                                                            <th style="width: 20%; padding:0 2px; white-space: nowrap; text-align:center;">Valor ($)</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody style="text-align: center;">
                                                        <!-- Aquí se generarán las filas dinámicamente -->
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="horizontal-group-simple" style="margin-top:0;justify-content:end;">
                                                 <p id="sumaValueSerHwdPDF" style="margin-right:5%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                            </div> 
                                            <div class="doble-columna" style="padding:0 0.5rem 0 0.5rem;">
                                                <span style="width: 22%; padding-left:1rem;">- Observacion:</span><span style="width: 78%;" id="txtPDFHardwareOBS">-- -- --</span>
                                            </div>

                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:2rem 1rem 0.2rem 0.5rem;">
                                                <%--<span style="width: 20%;">Licencias:</span><span style="width: 80%;" id="txtPDFLicencias">-- -- --</span>--%>
                                                <span style="width: 20%;">Licencias:</span>
                                                <table id="tbl_pdfLicencias" class="columna div1 table table-sm" 
                                                    style="margin-bottom:0; flex: 1; border: 2px solid #5a5959; width: 100%; table-layout: fixed; border-collapse:separate;">
                                                    <thead>
                                                        <tr style="text-align: center; background-color: #e4e4e48f;">
                                                            <th style="width: 10%; padding:0 2px; white-space: nowrap; text-align:center;" class="titulosTabla">Cantidad</th>
                                                            <th style="width: 70%; padding:0 2px; white-space: nowrap; text-align:center;" class="titulosTabla">Detalle</th>
                                                            <th style="width: 20%; padding:0 2px; white-space: nowrap; text-align:center;">Valor ($)</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody style="text-align: center;">
                                                        <!-- Aquí se generarán las filas dinámicamente -->
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="horizontal-group-simple" style="margin-top:0;justify-content:end;">
                                                 <p id="sumaValueSerLicPDF" style="margin-right:5%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                            </div> 
                                            <div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                <span style="width: 22%; padding-left:1rem;">- Observacion:</span><span style="width: 78%;" id="txtPDFLicenciasOBS">-- -- --</span>
                                            </div>

                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:2rem 1rem 0.2rem 0.5rem;">
                                                <span style="width: 20%;">Servicios Externos:</span>
                                                <table id="tbl_pdfServExt" class="columna div1 table table-sm" 
                                                    style="margin-bottom:0; flex: 1; border: 2px solid #5a5959; width: 100%; table-layout: fixed; border-collapse:separate;">
                                                    <thead>
                                                        <tr style="text-align: center; background-color: #e4e4e48f;">
                                                            <th style="width: 10%; padding:0 2px; white-space: nowrap; text-align:center;">#</th>
                                                            <th style="width: 70%; padding:0 2px; white-space: nowrap; text-align:center;">Detalle</th>
                                                            <th style="width: 20%; padding:0 2px; white-space: nowrap; text-align:center;">Valor ($)</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody style="text-align: center;">
                                                        <!-- Aquí se generarán las filas dinámicamente -->
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="horizontal-group-simple" style="margin-top:0;justify-content:end;">
                                                 <p id="sumaValueSerExtPDF" style="margin-right:5%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                            </div> 
                                            <div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                <span style="width: 22%; padding-left:1rem;">- Observacion:</span><span style="width: 80%;" id="txtPDFServiciosExternosOBS">-- -- --</span>
                                            </div>

                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:2rem 1rem 0.2rem 0.5rem;">
                                                <%--<span style="width: 20%;">Servicios de fabricante:</span><span style="width: 80%;" id="txtPDFServiciosFabricante">-- -- --</span>--%>
                                                <span style="width: 20%;">Servicios de fabricante:</span>
                                                <table id="tbl_pdfServiciosFab" class="columna div1 table table-sm" 
                                                    style="margin-bottom:0; flex: 1; border: 2px solid #5a5959; width: 100%; table-layout: fixed; border-collapse:separate;">
                                                    <thead>
                                                        <tr style="text-align: center; background-color: #e4e4e48f;">
                                                            <th style="width: 10%; padding:0 2px; white-space: nowrap; text-align:center;">Cantidad</th>
                                                            <th style="width: 70%; padding:0 2px; white-space: nowrap; text-align:center;">Detalle</th>
                                                            <th style="width: 20%; padding:0 2px; white-space: nowrap; text-align:center;">Valor ($)</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody style="text-align: center;">
                                                        <!-- Aquí se generarán las filas dinámicamente -->
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="horizontal-group-simple" style="margin-top:0;justify-content:end;">
                                                 <p id="sumaValueServFabPDF" style="margin-right:5%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                            </div> 
                                            <div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                <span style="width: 22%; padding-left:1rem;">- Observacion:</span><span style="width: 78%;" id="txtPDFServiciosFabricanteOBS">-- -- --</span>
                                            </div>                                            

                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:2rem 1rem 0.2rem 0.5rem;">
                                                <%--<span style="width: 25%;">Polizas:</span><span id="txtPDFPolizas">-- -- --</span>--%>
                                                <span style="width: 20%;">Polizas:</span>
                                                <table id="tbl_pdfPolizas" class="columna div1 table table-sm" 
                                                    style="margin-bottom:0; flex: 1; border: 2px solid #5a5959; width: 100%; table-layout: fixed; border-collapse:separate;">
                                                    <thead>
                                                        <tr style="text-align: center; background-color: #e4e4e48f;">                   
                                                            <th style="width: 35%; padding:0 2px; white-space: nowrap; text-align:center;">Detalle</th>
                                                            <th style="width: 25%; padding:0 2px; white-space: nowrap; text-align:center;">Tipo</th>                                                            
                                                            <th style="width: 15%; padding:0 2px; white-space: nowrap; text-align:center;">Fecha emisión</th>
                                                            <th style="width: 15%; padding:0 2px; white-space: nowrap; text-align:center;">Fecha caducidad</th>
                                                            <th style="width: 15%; padding:0 2px; white-space: nowrap; text-align:center;">Valor ($)</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody style="text-align: center;">
                                                        <!-- Aquí se generarán las filas dinámicamente -->
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="horizontal-group-simple" style="margin-top:0;justify-content:end;">
                                                 <p id="sumaValuePolizaPDF" style="margin-right:5%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                            </div> 
                                            <div class="doble-columna">
                                                <span style="width: 22%; padding-left:1rem;">- Observacion:</span><span style="width: 78%;" id="txtPDFPolizasOBS">-- -- --</span>
                                            </div>

                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:2rem 1rem 0.2rem 0.5rem;">
                                                <span style="width: 20%;">Servicios DOS:</span><span style="width: 80%;" id="txtPDFServiciosDOSOBS">-- -- --</span>
                                            </div>
                                            <%--<div class="doble-columna" style="padding:0.2rem 0.5rem 0 0.5rem;">
                                                <span style="width: 20%; padding-left:1rem;">- Observaciones:</span><span style="width: 80%;" id="txtPDFServiciosDOSOBS">-- -- --</span>
                                            </div>--%>


                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:2rem 1rem 0.2rem 0.5rem;">
                                                <%--<span style="width: 25%;">Polizas:</span><span id="txtPDFPolizas">-- -- --</span>--%>
                                                <span style="width: 20%;">Formas de Pago:</span>
                                                <table id="tbl_FormasPago" class="columna div1 table table-sm" 
                                                    style="margin-bottom:0; flex: 1; border: 2px solid #5a5959; width: 100%; table-layout: fixed; border-collapse:separate;">
                                                    <thead>
                                                        <tr style="text-align:center; background-color: #e4e4e48f;">
                                                            <th style="width:5%; padding:0 2px; white-space: nowrap; text-align:center;">#</th>
                                                            <th style="width:40%; padding:0 2px; white-space: nowrap; text-align:center;">Descripción</th>
                                                            <th style="width:15%; padding:0 2px; white-space: nowrap; text-align:center;">Porcentaje %</th>
                                                            <th style="width:15%; padding:0 2px; white-space: nowrap; text-align:center;">Valor ($)</th>
                                                            <th style="width:22%; padding:0 2px; white-space: nowrap; text-align:center;">Fecha estimada pago</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody style="text-align:center;">

                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="horizontal-group-simple" style="margin-top:0;justify-content:end;">
                                                 <p id="remainingValuePDF" style="margin-right:20%; color:black; font-weight:bold;">Valor total: $0.00</p>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 22%;padding-left:1rem;">- Observacion:</span><span style="width: 78%;" id="txtPDFFormasPagoOBS">-- -- --</span>
                                            </div>

                                            <hr class="linea">
                                            <div class="doble-columna" style="padding:2rem 1rem 0.2rem 0.5rem;">
                                                <span style="width: 20%;">Archivos cargados:</span>
                                                <div id="listaArchivos"></div>
                                            </div>
                                            <div class="doble-columna" style="padding:1rem 1rem 0.2rem 0.5rem;">
                                                <span style="width: 20%;">Temas contenidos en los archivo cargados:</span>
                                                <div id="listaArchivos2"></div>
                                            </div>
                                            

                                           <%-- <hr class="linea">
                                            <div class="doble-columna">
                                                <span style="width: 25%;">Términos de referencia - TDR's:</span><span id="txtPDFTerminosReferencia">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;"></span><span id="txtPDFTerminosReferenciaOBS">-- -- --</span>
                                            </div>
                                            <hr class="linea">--%>
                                            <!--div class="doble-columna">
                                                <div class="doble-columna" style="width: 62%;">
                                                    <span style="width: 45%;">Formas de pago:</span><span id="txtPDFFormasPago">-- -- --</span>
                                                </div>                    
                                                <div class="flex-half doble-columna">
                                                    <span style="width: 30%;">Otra forma de pago:</span><span id="txtPDFOtraFormaPago">-- -- --</span>
                                                </div>
                                            </!--div-->  

                                            <%--<div class="doble-columna">
                                                <span style="width: 25%;">Acta de preguntas y respuestas:</span><span id="txtPDFActaPreguntasRespuestas">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;"></span><span id="txtPDFActaPreguntasRespuestasOBS">-- -- --</span>
                                            </div>
                                            <hr class="linea">
                                            <div class="doble-columna">
                                                <span style="width: 25%;">Acta de adjudicación:</span><span id="txtPDFActaAdjudicacion">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;"></span><span id="txtPDFActaAdjudicacionOBS">-- -- --</span>
                                            </div>
                                            <hr class="linea">
                                            <div class="doble-columna">
                                                <span style="width: 25%;">Acta de negociación:</span><span id="txtPDFActaNegociacion">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;"></span><span id="txtPDFActaNegociacionOBS">-- -- --</span>
                                            </div>
                                            <hr class="linea">
                                            <div class="doble-columna">
                                                <span style="width: 25%;">BoM Solución:</span><span id="txtPDFBoMSolucion">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;"></span><span id="txtPDFBoMSolucionOBS">-- -- --</span>
                                            </div>
                                            <hr class="linea">
                                            <div class="doble-columna">
                                                <span style="width: 25%;">Acuerdos Mayoristas:</span><span id="txtPDFAcuerdosMayoristas">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;"></span><span id="txtPDFAcuerdosMayoristasOBS">-- -- --</span>
                                            </div>
                                            <hr class="linea">
                                            <div class="doble-columna">
                                                <span style="width: 25%;">Acuerdos Fabricantes:</span><span id="txtPDFAcuerdosFabricantes">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;"></span><span id="txtPDFAcuerdosFabricantesOBS">-- -- --</span>
                                            </div>
                                            <hr class="linea">
                                            <div class="doble-columna">
                                                <span style="width: 25%;">Garantías FIN:</span><span id="txtPDFGarantiasFIN">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;"></span><span id="txtPDFGarantiasFINOBS">-- -- --</span>
                                            </div>
                                            <hr class="linea">
                                            <div class="doble-columna">
                                                <span style="width: 25%;">Garantías y Licencias TEC:</span><span id="txtPDFGarantiasLicenciasTEC">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;"></span><span id="txtPDFGarantiasLicenciasTECOBS">-- -- --</span>
                                            </div>
                                            <hr class="linea">
                                            <div class="doble-columna">
                                                <span style="width: 25%;">Generación de Pedidos:</span><span id="txtPDFGeneracionPedidos">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;">Ordenes Servicio:</span><span id="txtPDFOrdenesServicio">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 25%;"></span><span id="txtPDFGeneracionPedidosOBS">-- -- --</span>
                                            </div>--%>                                            
                                            

                                        </div>
                                    </div>

                                   <%-- <div class="row" style="margin: 0 0 1rem 0;"> <!-- Cuadros 4-->
                                        <div class="centered-element" style="padding-bottom:0.5rem; width:100%;">
                                            <span class="letra-bold">FORMAS DE PAGO:</span>
                                        </div> 
                                        <table id="tbl_FormasPago" class="columna div1 table table-sm" style="margin-bottom:0;">
                                            <thead>
                                                <tr style="text-align:center;">
                                                    <th style="width:10%;" class="titulosTabla">#</th>
                                                    <th style="width:36%;" class="titulosTabla">Descripción</th>
                                                    <th style="width:15%;" class="titulosTabla">%</th>
                                                    <th style="width:18%;" class="titulosTabla">Valor</th>
                                                    <th style="width:23%;" class="titulosTabla">Fecha estimada pago</th>
                                                </tr>
                                            </thead>
                                            <tbody style="text-align:center;">

                                            </tbody>
                                        </table>
                                    </div>--%>

                                    <div class="row-flex" style="margin: 0 0 1rem 0;"> <!-- Cuadros 5-->
                                        <div class="columna div1 col-80" style="padding-left: 0.2rem; padding-bottom:0.5rem;">
                                            <div class="centered-element" style="padding-bottom:0.5rem;">
                                                <span class="letra-bold">DATOS DE CONTACTO:</span>
                                            </div> 
                                            <div class="doble-columna">
                                                <span style="width: 15%; padding-right:0.5rem;">Nombre:</span><span id="txtPDFNombre">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 15%; padding-right:0.5rem;">Teléfono:</span><span id="txtPDFTelefono">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 15%; padding-right:0.5rem;">Dirección:</span><span id="txtPDFDireccion">-- -- --</span>
                                            </div>
                                            <div class="doble-columna">
                                                <span style="width: 15%; padding-right:0.5rem;">Correo:</span><span id="txtPDFCorreo">-- -- --</span>
                                            </div>
                                        </div>            
                                    </div>
                                          
                        
                                </div>            
                            </div>            
                
                            <div class="container" style="margin: 1rem auto 0 33%">
                                <%--<button class="btn btn-info col-sm-3" type="button"onclick="generatePDF(), descargarModalComoPDF('ModalEgresoInventarioPDF')">Descargar</button>--%>
                                <button class="btn btn-info col-sm-3" type="button"onclick="descargarModalComoPDF('ModalEgresoInventarioPDF')">Descargar</button>
                            </div>
                            <div class="modal-footer" style="margin-top:2rem;">                
                                <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                            </div>
                            <!-- /.modal-content -->
                        </div>
                    <!-- /.modal-dialog -->    
                   </div>
                </div>

                

                <!-- Modal de Visualizacion de Consulta por tipos -->
                <div id="ModalConsulta" class="modal fade"  role="dialog">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header bg-info">                            
                                <h4 class="modal-title">
                                    <span class="badge" style="background-color:aliceblue; color:#025ba3; margin-right:1rem;">
                                        <i class="fa fa-question"></i>
                                    </span> 
                                    Seleccione el tipo de consulta a utilizar
                                </h4>                     
                                <button type="button" class="close" data-dismiss="modal">x</button>
                            </div>
                            <div class="modal-body" style="display:flex; justify-content:space-evenly; margin-top:1rem;">
                                <div class="" id="modal_opciones">
                                    <label class="form-label">Tipo de búsqueda</label>
                                    <div class="form-check" style="margin-left:1rem;">
                                        <input class="form-check-input" type="radio" name="tipoBusqueda" id="OpContrato" value="contrato">
                                        <label class="form-check-label" for="contrato">Por contrato</label>
                                    </div>
                                    <div class="form-check" style="margin-left:1rem;">
                                        <input class="form-check-input" type="radio" name="tipoBusqueda" id="OpPedido" value="pedido">
                                        <label class="form-check-label" for="pedido">Por pedido</label>
                                    </div>
                                    <div class="form-check" style="margin-left:1rem;">
                                        <input class="form-check-input" type="radio" name="tipoBusqueda" id="OpCliente" value="cliente">
                                        <label class="form-check-label" for="cliente">Por cliente</label>
                                    </div>
                                </div>

                                <div class="col-sm-6" id="modal_input" style="display:none; margin-top:2rem;">
                                    <input type="text" class="form-control" id="numero" placeholder="Ingrese el número">
                                </div>

                                <div class="col-sm-10" id="modal_table" style="display:none; margin-top:1rem;">
                                    <div class="input-group col-sm-12" style="margin-bottom:0; font-size:10px;">
                                        <span class="input-azulobscuro input-group-addon ingresosTitulos" style="width: 20%; color:aliceblue;"><i class="glyphicon glyphicon-user"></i>  Cliente:</span>
                                        <input id="txtCliente3" type="text" class="form-control" placeholder="Cliente" oninput="convertirAMayusculas(this),BuscarCliente3()" name="cliente"><i class="glyphicon glyphicon-asterisk asterisk" aria-hidden="true"></i>
                                        <ul class="typeahead dropdown-menu" role="listbox" style="left: 25%; cursor:pointer;" id="comboClientes3">
                                        </ul>
                                        <div class="input-group-btn">
                                            <button class="btn btn-default" type="button" id="" onclick="BuscarEstadoNotificacion(document.getElementById('txtCliente3').value, '', 4)"><i class="glyphicon glyphicon-search"></i></button>
                                        </div>                                     
                                    </div>   
                                    <div id="tabla_clientes_consulta" style="display:none;">
                                        <table id="tbl_ProyectosConsulta" class=" dynamic-table table-hover table-bordered sm-12" style="width:100%; overflow: hidden; margin-top:1rem;">
                                            <thead  class="text-center bg-info">
                                                <tr>
                                                    <th style="width:45%; text-align:center;">Cliente</th>
                                                    <th style="width:25%; text-align:center;"># Contrato</th>
                                                    <th style="width:25%; text-align:center;"># Pedido</th>
                                                    <th style="width:5%; text-align:center;">Acción</th>
                                                </tr>
                                            </thead>
                                            <tbody>                                           
                                            </tbody>
                                        </table>
                                    </div>                                    
                                </div>
                            </div>                        
                            <div class="modal-footer" style="margin-top:0;">
                                 <button type="button" class="btn btn-primary" id="buscarBtnModal">Buscar</button>
                            </div>
                        </div>
                    </div>
                </div>


                <!-- Modal de Visualizacion de Consulta Documentos -->
                <div id="ModalConsultaDocs" class="modal fade"  role="dialog">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header bg-info" style="display:flex; justify-content: space-between">                            
                                <h4 class="modal-title">
                                    <span class="badge" style="background-color:aliceblue; color:#025ba3; margin-right:1rem;">
                                        <i class="fa fa-question"></i>
                                    </span> 
                                    Ingrese el número de Proyecto a buscar
                                </h4>                     
                                <button type="button" class="close" data-dismiss="modal">X</button>
                            </div>
                            <div class="modal-body" style="display:flex; justify-content:space-evenly; margin-top:1rem;">                                
                                <div class="col-sm-6" style="margin-top:2rem;">
                                    <input type="text" class="form-control" id="numeroProyecto" placeholder="Ingrese el número de pedído">
                                </div>                               
                            </div>                        
                            <div class="modal-footer" style="margin-top:0;">
                                 <button type="button" class="btn btn-primary" id="consultarDocsBtnModal">Buscar</button>
                            </div>
                        </div>
                    </div>
                </div>
                

            </asp:Panel>


        </div> <!-- FIN de pagina sin bordes solo ajusta el tamaño -->

   </div> <!-- FIN de pagina incluye titulo de la pagina -->
    
</asp:Content>

<%--<asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True"></asp:DropDownList>--%>
