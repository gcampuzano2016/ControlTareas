<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="InfoDepMedico.aspx.cs" Inherits="ReporteTareas.Formulario.InfoDepMedico" %>
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
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>

    <script src="https://cdn.jsdelivr.net/npm/jquery@3.6.0.min.js"></script>    
    <script src="https://cdn.jsdelivr.net/npm/popper.js@1.16.1/dist/umd/popper.min.js" integrity="sha384-9/reFTGAW83EW2RDu2S0VKaIzap3H66lZH81PoYlFhbGU+6BZp6G7niu735Sk7lN" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/js/bootstrap.min.js" integrity="sha384-+sLIOodYLS7CIrQpBjl+C7nPvqq+FbNUBDunl/OZv93DB7Ln/533i8e/mZXLi/P+" crossorigin="anonymous"></script>
    

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <script src="https://cdn.jsdelivr.net/jquery/latest/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/momentjs/latest/moment.min.js"></script>

    <!-- SweetAlert2  -->    
    <script src="../bower_components/sweetalert/js/sweetalert2.all.min.js"></script> 
    <link href="../bower_components/sweetalert/css/sweetalert2.min.css" rel="stylesheet" /> 

    <script src="../js/InfoDepMedico.js?v=7"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content; ">
        <div class="col-lg-12" style="background-color: #0D2538;padding-bottom: 1.5rem;padding-top: 0.5rem;">

            <div class="row" style="background-color: #0D2538; margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs">

                    <ul class="breadcrumb" style="margin-bottom:10px;">
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
                            <%--<asp:TextBox ID="hiddenCedulaField" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />--%>
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
<%--                        <li data-target="#myCarousel" data-slide-to="3"></li>
                        <li data-target="#myCarousel" data-slide-to="4"></li>--%>
                    </ol>
                    
                    <!-- Imagenes del carrusel  -->
                    <div class="carousel-inner" style="height:190px;">
                        <%--<div class="item active">
                            <img src="../carrusel/imagenes/slogan2.png">
                            <div class="carousel-caption">
                            </div>
                        </div>--%>

                        <div class="item active">
                            <img src="../carrusel/imagenes/medicina6 gth.jpg">
                            <div class="carousel-caption">
                            </div>
                        </div>

                        <div class="item ">
                            <img src="../carrusel/imagenes/medicina1 gth.jpg">
                            <div class="carousel-caption">
                            </div>
                        </div>

                        <%--<div class="item ">
                            <img src="../carrusel/imagenes/medicina5 gth.jpg">
                            <div class="carousel-caption">
                            </div>
                        </div>--%>

                        <div class="item ">
                            <img src="../carrusel/imagenes/medicina4 gth.jpg">
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

                <%--<div class="input-group col-sm-4" style="display:none; margin: 0 auto; padding-top: 1rem; padding-bottom: 1rem;">
                    <input type="text" id="txtEmpleado" class="form-control" placeholder="Buscar paciente">                    
                    <div class="input-group-btn">
                        <button class="btn btn-default" type="button" id="btnBuscarDatosPersonales" oninput="BuscarCodigosCIE()"><i class="glyphicon glyphicon-search"></i></button>
                    </div>
                </div>--%>
                

                <div class="tabs" id="pestanias" style="background-color: #0D2538; padding-top: 1rem; display:block;">
                    <ul class="nav nav-tabs">
                        <li class="nav-item">
                            <a class="nav-link tab-seleccionada" id="pestania1" onclick="seleccionarPestania('pestania1'); cargarHistorialAtenciones();">HISTORIAL</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" id="pestania2" onclick="seleccionarPestania('pestania2')">DASHBOARDS</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" id="pestania3" onclick="seleccionarPestania('pestania3')">BUSCAR RESULTADOS</a>
                        </li>
                    </ul>
                </div>

                <div id="pestaniaAtencionesMedicas" style="display: block; padding: 0 0 0.2rem 0;">  
                    <!-- Cuadro de Actividades " -->
                    <div class="well" style="margin-bottom: 10px; margin-top: 0; background: #eaeff6;">
                        <div class="well-header" style="margin-top: 0;">
                            <div class="header-med">
                                <h2 class="">Atenciones Médicas</h2>
                                <p>A continuacion se muestra el historial de atenciones médicas realizadas</p>
                            </div>
                        </div>

                        <!-- Contenedor donde se crearán las tarjetas dinámicamente -->
                        <div id="contenedorTarjetas"></div>
                                            

                    </div>
                </div>
                                

            </asp:Panel>

        </div>        
    </div>
</asp:Content>
