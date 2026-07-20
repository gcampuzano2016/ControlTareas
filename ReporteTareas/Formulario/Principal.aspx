<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="Principal.aspx.cs" Inherits="ReporteTareas.Formulario.Principal" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
        <div class="row">
            <div class="col-lg-12">
                <div class="panel panel-default">
                    <div class="panel-body">
                    <div class="row">
                        <h1 style="color: var(--dos-primary-600); font-weight: 700">Bienvenidos al Sistema de Gesti&oacute;n Interno</h1>
                        <p class="text-muted">Este es el Sistema de Gesti&oacute;n Interno de DOS.</p>
                    </div>
                    <div id="myCarousel" class="carousel slide" data-ride="carousel">
                        <!-- Target para los slide lde las imagenes  -->
                        <ol class="carousel-indicators">
                            <li data-target="#myCarousel" data-slide-to="0" class="active"></li>
                            <li data-target="#myCarousel" data-slide-to="1"></li>
                            <li data-target="#myCarousel" data-slide-to="2"></li>
                            <li data-target="#myCarousel" data-slide-to="3"></li>
                            <li data-target="#myCarousel" data-slide-to="4"></li>
                            <li data-target="#myCarousel" data-slide-to="5"></li>
                        </ol>
                        <!-- Imagenes del carrusel  -->
                        <div class="carousel-inner">
                            <div class="item active">
                                <img src="../carrusel/imagenes/slogan2.png">
                                <div class="carousel-caption">
                                </div>
                            </div>
                            <div class="item ">
                                <img src="../carrusel/imagenes/img2.2.png">
                                <div class="carousel-caption">
                                </div>
                            </div>
                            <div class="item ">
                                <img src="../carrusel/imagenes/img3.2.png">
                                <div class="carousel-caption">
                                </div>
                            </div>
                            <div class="item ">
                                <img src="../carrusel/imagenes/img5.2.png">
                                <div class="carousel-caption">
                                </div>
                            </div>
                            <div class="item ">
                                <img src="../carrusel/imagenes/img6VF2.png">
                                <div class="carousel-caption">
                                </div>
                            </div>
                            <div class="item ">
                                <img src="../carrusel/imagenes/imagenFin.png">
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
                    <div class="text-center">
                        <h2 style="font-weight: 700; margin: 24px 0">30 años como la empresa de tecnología líder del Ecuador</h2>
                        <p class="text-left">Hemos crecido como aliados de nuestros clientes apoyando su gestión empresarial con talento humano altamente calificado, brindando servicios de asesoría, data center, cloud computing, almacenamiento, mantenimiento preventivo y correctivo, redes empresariales y mucho m&aacute;s. M&aacute;s de tres d&eacute;cadas nos respaldan como la empresa de tecnolog&iacute;a m&aacute;s importante del Ecuador. Somos representantes autorizados de las mejores marcas del mundo: Microsoft, HP y HPE, Cisco, Oracle, Xerox, Red Hat, F5, Veeam, Simplivity, VMware, Micro Focus y Cylance.</p>
                        <video controls muted width="80%" style="max-width: 100%; height: auto">
                            <source src="../carrusel/imagenes/VideoDOS.mp4" type="video/mp4">
                        </video>
                    </div>
                </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
