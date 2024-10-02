<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="HistoriaClinicaArchivosRRHH.aspx.cs" Inherits="ReporteTareas.Formulario.HistoriaClinicaArchivosRRHH" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/HistoriaClinicaArchivosRRHH.js?v=1"></script>

    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>


    <link href="../dist/css/depMedico.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

     HistoriaClinicaArchivosRRHH.aspx.cs
    
    <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content;">
        <div class="col-lg-12" style="background-color: #0D2538;padding-bottom: 1.5rem;padding-top: 0.5rem;">

            <div class="row" style="background-color: #0D2538; margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs">

                    <ul class="breadcrumb">
                        <li>
                            <a href="#"><b>Archivos y Documentos del Departamento Médico</b></a>
                        </li>
                    </ul>
                </div>
            </div>

            <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
                 <div class="panel-default">
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

                <!-- ******************************************************************** -->
                <!--                            PAGINA                                    -->
                <!-- ******************************************************************** -->

                <div id="myCarousel" class="carousel slide" data-ride="carousel">
                    <!-- Target para los slide lde las imagenes  -->
                    <ol class="carousel-indicators">
                        <li data-target="#myCarousel" data-slide-to="0" class="active"></li>
                        <li data-target="#myCarousel" data-slide-to="1"></li>

                    </ol>

                    <!-- Imagenes del carrusel  -->
                    <div class="carousel-inner">
                        <div class="item active">
                            <img src="../carrusel/imagenes/documentosMedicos1.jpg">
                            <div class="carousel-caption">
                            </div>
                        </div>
                        <div class="item ">
                            <img src="../carrusel/imagenes/documentosMedicos2.jpg">
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

                <!-- Contenido de "Historia Ocupacional" -->
                <div style="padding: 1rem 1rem 0.2rem 1rem; margin-top: 1rem; background-color: #9DA8AD;">     
                    <!--  Cuadro Datos personales  -->
                    <div class="well" style="margin-bottom: 10px; margin-top: 0; padding-top: 1rem;">
                        <div class="well-header" style="margin-top: 0;">
                            <div>
                                 <h4 class="well-title">Actualizar información de todos los empleados</h4>
                            </div>
                        </div>                                 
                        <div class="horizontal-group">                            
                            <button type="button" title="Cargar Documentos" class="btn btn-info btn-sm " onclick="MensajeCargaArchivo()">
                                   <i class="fa fa-files-o" style="margin-right:5px;"></i>Cargar Documento
                            </button>
                        </div>
                    </div>   

                </div>                            

                <!------------------------------------------->
                <!--        Modal cargarArchivos           -->
                <!------------------------------------------->

                <!-- Mensaje Modal -->
                <div id="msgCargarArchivos" class="modal fade" role="dialog">
                    <div class="modal-dialog">
                        <!-- Modal content-->
                        <div class="modal-content">
                            <div class="modal-header bg-info">
                                <button type="button" class="close" data-dismiss="modal">&times;</button>
                                <h4 class="modal-title">CARGA ARCHIVOS</h4>
                            </div>
                            <div class="modal-body">
                                <p>(Ejemplo:"NombreArchivo"_info_empleados.xlsx)</p>                                
                                 <formview class="horizontal-group" id="formCargarArchivo" enctype="multipart/form-data">
                                    <label for="myfile" style="width:30%;">Seleccionar archivo:</label>
                                    <input type="file" id="archivosAdjuntos" style="width:70%;" multiple><br><br>                                     
                                </formview>
                                <formview>
                                     <progress id="fileProgress" style="display: none; width:100%;"></progress>
                                     <hr />
                                     <span id="lblMessage" style="color: Green"></span>
                                     <br>
                                     <!--button type="button" id="btnCargarArchivosAdjuntos" style="text-align: end;" >Cargar</!--button-->
                                     <input type="button" id="btnCargarArchivosAdjuntos" value="Cargar Archivos" class="btn btn-info" />
                                     <p class="help-block"></p>
                                </formview>
                            </div>    
                        </div>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </div>
</asp:Content>
