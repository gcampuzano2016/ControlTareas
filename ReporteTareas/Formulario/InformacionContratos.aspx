<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="InformacionContratos.aspx.cs" Inherits="ReporteTareas.Formulario.InformacionContratos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="../dist/css/InfoContratos.css" rel="stylesheet" />
    <link href="../dist/css/styleInventario.css" rel="stylesheet" />

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

    <script src="../js/InformacionContratos.js?v=16"></script>

    <script src="../bower_components/popper/popper.min.js"></script>

    <!-- El script de la librería PDF-->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.9.2/html2pdf.bundle.min.js"></script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    InformacionContratos.aspx.cs

    <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content;">
        <div class="col-lg-12" style="background-color: #0D2538; padding-bottom: 1.5rem; padding-top: 0.5rem;">

            <div class="row" style="margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs">

                    <ul class="breadcrumb">
                        <li style="display: flex; justify-content: space-between;">
                            <a href="#"><b>Ingreso de información de contrato</b></a>
                            <a href="#"><i class="glyphicon glyphicon-log-out"></i></a>
                        </li>
                    </ul>
                </div>
            </div>

            <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
                <div class="panel-default">
                    <div class="panel-heading">
                        <div style="display: none">
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtCodUnico" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtPerfil" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
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


                    <div class="generalesInfoContratos">

                        <div id="pestaniaPrincipal" class="pagina" style="display: block; padding: 0.5rem 0 0.2rem 0;">
                            <!-- Cuadro Datos del Contrato  background-color: rgba(169, 14, 14, 0.50);-->
                            <div class="well fondo-azul" style="text-align: center; margin: 10px 0 0 0; padding-top: 1rem; border: none;">
                                <div class="well-header" style="margin-top: 0;">
                                    <h2 class="well-title" style="color: aliceblue;">Menu Proyectos</h2>
                                </div>
                                <!-- Botones agregados -->
                                <div style="margin-top: 20px;">
                                    <button type="button"  id="boton1" class="btn btn-primary" onclick="manejarBoton('', 1)">Nuevo</button>
                                    <button type ="button" id="boton2" class="btn btn-secondary" onclick="manejarBoton('', 2)">Buscar y Editar</button>
                                </div>
                            </div>
                        </div>



                        <!-- Contenido de la pestaña 1"Datos del Contrato" -->
                        <div id="pestaniaRoja" class="pagina" style="display: none; padding: 0.5rem 0 0.2rem 0;">

                            <!-- Cuadro Datos del Contrato  background-color: rgba(169, 14, 14, 0.50);-->
                            <div class="well fondo-rojo" style="text-align: center; margin: 10px 0 0 0; padding-top: 1rem; border: none;">

                                <div class="well-header" style="margin-top: 0;">
                                    <h2 class="well-title" style="color: aliceblue;">Información del Contrato</h2>
                                </div>

                                <%--<div style="text-align:center;padding:0 0;"> 
                                        <h3><a style="color:cornflowerblue; text-decoration:none;" href="https://www.zeitverschiebung.net/es/city/3652462"><br />Quito, Ecuador</a></h3> 
                                        <iframe src="https://www.zeitverschiebung.net/clock-widget-iframe-v2?language=es&size=medium&timezone=America%2FGuayaquil" width="100%" height="115" frameborder="0" seamless></iframe> 
                                    </div>--%>

                                <div class="horizontal-cuadriculado-flow">
                                    <div class="horizontal-group-contrato">
                                        <div class="input-group col-sm-9" style="margin-top: 4rem; margin-bottom: 0.5rem;">
                                            <span class="input-azulobscuro input-group-addon ingresosTitulos" style="width: 18.5rem; color: aliceblue;"><i class="glyphicon glyphicon-user"></i>Cliente:</span>
                                            <input id="txtCliente" type="text" class="form-control" placeholder="Cliente" oninput="convertirAMayusculas(this),BuscarCliente2()">
                                            <i class="glyphicon glyphicon-asterisk asterisk" aria-hidden="true"></i>
                                            <ul class="typeahead dropdown-menu" role="listbox" style="left: 25%; cursor: pointer;" id="comboClientes2">
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
                                        <div class="" style="width: 90%;">

                                            <div class="input-group" style="margin-bottom: 0.5rem; margin-top: 4rem;">
                                                <span class="input-group-addon ingresosTitulos" style="background-color: #7B7D7E; color: aliceblue;"><i class="glyphicon glyphicon-list-alt"></i>Número de Contrato:</span>
                                                <input id="txtNumContrato" type="text" class="form-control" placeholder="Núm Contrato" oninput="convertirAMayusculas(this)">
                                                <i class="glyphicon glyphicon-asterisk asterisk" aria-hidden="true"></i>
                                            </div>
                                            <div class="input-group col-sm-12">
                                                <textarea id="txtNumContratoObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>

                                        </div>
                                        <div class="" style="width: 90%;">

                                            <div class="input-group" style="margin-bottom: 0.5rem; margin-top: 4rem;">
                                                <span class="select-dorado input-group-addon ingresosTitulos" style="color: #333;"><i class="glyphicon glyphicon-usd"></i>Valor total del Contrato:</span>
                                                <div class="" style="display: flex;">
                                                    <input id="txtValorContrato" type="text" class="form-control" placeholder="$" oninput="calcularPorcentaje();formatearValor(this)">
                                                    <i class="glyphicon glyphicon-asterisk asterisk" aria-hidden="true"></i>
                                                    <span style="font-size: 20px; font-weight: 700; color: aliceblue; margin-left: 0.5rem; margin-right: 0.5rem; padding-bottom: 0;">,</span>
                                                    <!-- Input para los decimales -->
                                                    <input id="txtDecimales" type="text" class="form-control" placeholder="00" maxlength="2" style="width: 40%;">
                                                </div>
                                            </div>
                                            <div class="input-group col-sm-12">
                                                <textarea id="txtValorContratoObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>

                                        </div>
                                    </div>

                                </div>

                                <!-- Cuadro Datos de contacto -->
                                <div class="well" style="margin: 3rem 1.7rem 0 1.7rem; padding-top: 1rem; padding-bottom: 3rem; background-color: rgba(50, 55, 60, 0.6); border: none;">

                                    <div class="well-header" style="margin-top: 0;">
                                        <div>
                                            <h4 class="well-title" style="color: aliceblue; text-align: left;">Datos contacto</h4>
                                        </div>
                                    </div>
                                    <div class="contenedor-horizontal">

                                        <div class="horizontal-group-special">
                                            <div class="input-group col-sm-12">
                                                <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 20%; text-align: left;"><i class="glyphicon glyphicon-user"></i>Nombre:</span>
                                                <input id="txtNomContacto" type="text" class="form-control" name="Nombre" placeholder="Nombre" oninput="convertirAMayusculas(this)" value="">
                                            </div>
                                        </div>
                                        <div class="horizontal-group-special" style="margin-top: 1rem;">
                                            <div class="input-group col-sm-12">
                                                <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 20%; text-align: left;"><i class="glyphicon glyphicon-phone-alt"></i>Teléfono:</span>
                                                <input id="txtTelefono" type="text" class="form-control" name="telefono" placeholder="Teléfono" value="">
                                            </div>
                                        </div>
                                        <div class="horizontal-group-special" style="margin-top: 1rem;">
                                            <div class="input-group col-sm-12">
                                                <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 20%; text-align: left;"><i class="glyphicon glyphicon-globe"></i>Dirección:</span>
                                                <input id="txtDireccion" type="text" class="form-control" name="direccion" placeholder="Dirección" value="">
                                            </div>
                                        </div>
                                        <div class="horizontal-group-special" style="margin-top: 1rem;">
                                            <div class="input-group col-sm-12">
                                                <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 20%; text-align: left;"><i class="glyphicon glyphicon-envelope"></i>Correo:</span>
                                                <input id="txtCorreo" type="text" class="form-control" name="correo" placeholder="Correo" value="">
                                            </div>
                                        </div>

                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow">
                                    <div class="contenedor-horizontal marg-2">

                                        <div class="input-group col-sm-8" style="margin-bottom: 0.5rem; margin-top: 4rem;">
                                            <span class="input-azulmedio input-group-addon input-basic3 ingresosTitulos" style="width: 26%;"><i class="glyphicon glyphicon-tag"></i>Objeto:</span>
                                            <input id="txtObjeto" type="text" class="form-control" placeholder="Objeto" oninput="convertirAMayusculas(this)" value="">
                                            <i class="glyphicon glyphicon-asterisk asterisk" aria-hidden="true"></i>
                                        </div>
                                        <div class="input-group col-sm-12">
                                            <textarea id="txtObjetoObs" rows="3" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>

                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow">
                                    <div class="contenedor-horizontal marg-2">

                                        <div class="horizontal-group">
                                            <div class="input-group horizontal-group col-sm-4" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">
                                                <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Servicios DOS: </label>
                                                <div class="form-group horizontal-group">
                                                    <div class="btn-group" role="group">
                                                        <button type="button" id="siServiciosDOS" class="btn btn-custom" onclick="toggleButtonColor('siServiciosDOS')">Sí</button>
                                                        <button type="button" id="noServiciosDOS" class="btn btn-custom" onclick="toggleButtonColor('noServiciosDOS')">No</button>
                                                    </div>
                                                </div>
                                            </div>
                                            <button id="btnServDOS" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnServDOS','Servicios DOS:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                        </div>
                                        <div class="input-group col-sm-12" style="margin-top: -1.5rem; padding-top: 0;">
                                            <textarea id="txtServiciosDOSObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>

                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow">
                                    <div class="contenedor-horizontal marg-2">

                                        <div class="horizontal-group">
                                            <div class="input-group horizontal-group col-sm-4" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">
                                                <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Servicios Externos: </label>
                                                <div class="form-group horizontal-group">
                                                    <div class="btn-group" role="group">
                                                        <button type="button" id="siServiciosExt" class="btn btn-custom" onclick="toggleButtonColor('siServiciosExt')">Sí</button>
                                                        <button type="button" id="noServiciosExt" class="btn btn-custom" onclick="toggleButtonColor('noServiciosExt')">No</button>
                                                    </div>
                                                </div>
                                            </div>
                                            <button id="btnServExternos" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnServExternos','Servicios Externos:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                        </div>
                                        <div class="input-group col-sm-12" style="margin-top: -1.5rem; padding-top: 0;">
                                            <textarea id="txtServiciosExternosObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>

                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow">
                                    <div class="contenedor-horizontal marg-2">

                                        <div class="horizontal-group">
                                            <div class="input-group col-sm-8" style="margin-bottom: 0.5rem; margin-top: 3rem;">
                                                <span class="input-azulmedio input-group-addon input-basic3 ingresosTitulos"><i class="glyphicon glyphicon-align-left"></i>Costeo:</span>
                                                <input id="txtAlcance" type="text" class="form-control" placeholder="Alcance" oninput="convertirAMayusculas(this)" value="">
                                            </div>
                                            <button id="btnAlcance" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnAlcance','Alcance resumen - componentes:')">
                                                <i class="glyphicon glyphicon-link"></i>Cargar archivo
                                            </button>
                                        </div>
                                        <div class="input-group col-sm-12">
                                            <textarea id="txtAlcanceObjetoObs" rows="8" style="width: 100%; resize: vertical;" placeholder="Detalle"></textarea>
                                        </div>

                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow">
                                    <div class="contenedor-horizontal marg-2">
                                        <div class="input-group horizontal-group col-sm-4" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">
                                            <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Hardware: </label>
                                            <div class="form-group horizontal-group">
                                                <div class="btn-group" role="group">
                                                    <button type="button" id="siHardware" class="btn btn-custom" onclick="toggleButtonColor('siHardware')">Sí</button>
                                                    <button type="button" id="noHardware" class="btn btn-custom" onclick="toggleButtonColor('noHardware')">No</button>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="input-group col-sm-12" style="margin-top: -1.5rem; padding-top: 0;">
                                            <textarea id="txtHardwareObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>
                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow">
                                    <div class="contenedor-horizontal marg-2">

                                        <div class="horizontal-group">
                                            <div class="input-group col-sm-6" style="margin-bottom: 0.5rem; margin-top: 3rem;">
                                                <span class="input-azulmedio input-group-addon input-basic3 ingresosTitulos" style="width: 26%;"><i class="glyphicon glyphicon-ok-circle"></i>Licencias:</span>
                                                <input id="txtLicencias" type="text" class="form-control" placeholder="Licencias" oninput="convertirAMayusculas(this)" value="">
                                            </div>
                                            <!--button id="btnLicencias" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnLicencias','Licencias:')"><i class="glyphicon glyphicon-link"></i> Cargar archivo</!--button-->
                                        </div>
                                        <div class="input-group col-sm-12">
                                            <textarea id="txtLicenciasObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>

                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow">
                                    <div class="contenedor-horizontal marg-2">

                                        <div class="horizontal-group">
                                            <div class="input-group col-sm-6" style="margin-bottom: 0.5rem; margin-top: 3rem;">
                                                <span class="input-azulmedio input-group-addon input-basic3 ingresosTitulos"><i class="glyphicon glyphicon-random"></i>Servicios de fabricante:</span>
                                                <input id="txtServiciosFab" type="text" class="form-control" placeholder="Servicios de fabricante" oninput="convertirAMayusculas(this)" value="">
                                            </div>
                                            <button id="btnServFabricante" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnServFabricante','Servicios de fabricante:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                        </div>
                                        <div class="input-group col-sm-12">
                                            <textarea id="txtServiciosFabObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>

                                    </div>
                                </div>


                                <div class="horizontal-cuadriculado-flow">
                                    <div class="contenedor-horizontal marg-2">
                                        <div class="horizontal-group">
                                            <div class="input-group horizontal-group col-sm-4" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">
                                                <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Polizas: </label>
                                                <div class="form-group horizontal-group">
                                                    <div class="btn-group" role="group">
                                                        <button type="button" id="siPolizas" class="btn btn-custom" onclick="toggleButtonColor('siPolizas')">Sí</button>
                                                        <button type="button" id="noPolizas" class="btn btn-custom" onclick="toggleButtonColor('noPolizas')">No</button>
                                                    </div>
                                                </div>
                                            </div>
                                            <button id="btnPolizas" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnPolizas','Polizas:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                        </div>
                                        <div class="input-group col-sm-12" style="margin-top: -1.5rem; padding-top: 0;">
                                            <textarea id="txtPolizasObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>
                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow">
                                    <div class="contenedor-horizontal marg-2">
                                        <div class="horizontal-group" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">

                                            <div class="input-group horizontal-group col-sm-5" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">
                                                <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Términos de referencia - TDR's: </label>
                                                <div class="form-group horizontal-group">
                                                    <div class="btn-group" role="group">
                                                        <button type="button" id="siTDR" class="btn btn-custom" onclick="toggleButtonColor('siTDR')">Sí</button>
                                                        <button type="button" id="noTDR" class="btn btn-custom" onclick="toggleButtonColor('noTDR')">No</button>
                                                    </div>
                                                </div>
                                            </div>
                                            <button id="btnTerminosTDR" type="button" class="btn btn-primary ingresosTitulos" onclick="MensajeCargaArchivo('btnActaPreguntas','Términos de referencia - TDR´s:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                        </div>

                                        <div class="input-group col-sm-12" style="margin-top: -1.5rem; padding-top: 0;">
                                            <textarea id="txtTerminosObs" rows="6" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>
                                    </div>
                                </div>

                            </div>


                            <!-- Cuadro de Calculos  " -->
                            <div class="well" style="display: none; background-color: #d8e4e9; margin-top: 0; padding-top: 1rem;">
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



                            <div class="well" style="background-color: rgba(109,109,109,0); border: none;">
                                <!-- Paginador -->
                                <div class="datos-gerentes" id="btn-cargaContrato1" style="display: flex; flex-direction: column; align-items: center;">
                                    <button class="btn btn-info col-sm-6" type="button" id="" onclick="GuardarContrato(document.getElementById('txtNumContrato').value)" style="margin: 4rem 4rem 3rem 4rem;">Guardar Datos</button>
                                </div>
                                <div id="btn-paginas1" style="display: none; margin-top: 3rem; margin-bottom: 1rem;">
                                    <ul class="pager">
                                        <li><a href="#" type="button" style="background-color: rgba(125, 125, 125, 0.7); color: honeydew" class="ingresosTitulos btn btn-danger" onclick="irAPaginaAnterior(1)" disabled>Regresar</a></li>
                                        <li><a href="#" type="button" style="background-color: rgba(123, 170, 245, 0.5); color: honeydew" class="ingresosTitulos btn" onclick="irAPaginaSiguiente(1)">Siguiente</a></li>
                                    </ul>
                                </div>
                            </div>

                        </div>



                        <!-- Contenido de la pestaña 2"Archivos del Contrato  background-color: rgba(255, 208, 140, 0.8);" -->
                        <div id="pestaniaCafe" class="pagina oculto" style="display: none; padding: 0.5rem 0 0.2rem 0;">
                            <div class="well fondo-rojo" style="margin: 10px 0 0 0; padding-top: 1rem; border: none;">

                                <div class="well-header" style="margin-top: 0;">
                                    <h3 class="well-title" style="color: aliceblue;">Información de contrato</h3>
                                </div>

                                <div class="contenedor-horizontal marg-2">
                                    <div class="horizontal-group" style="margin-bottom: 1rem; margin-top: 0; padding-bottom: 0;">
                                        <div class="input-group col-sm-5" style="margin-top: 3rem;">
                                            <span class="input-azulmedio input-group-addon ingresosTitulos"><i class="glyphicon glyphicon-credit-card"></i>Formas de pago:</span>
                                        </div>
                                        <div class="col-sm-6" style="margin-top: 3rem; padding: 0;">
                                            <input id="txtOtraFormaPago" type="text" class="form-control select-temas" placeholder="Resumen de Formas de pago" oninput="convertirAMayusculas(this)" value="">
                                        </div>
                                    </div>
                                    <div class="horizontal-group-simple" style="margin-bottom: -2rem;">
                                        <p id="remainingValue" style="margin-left: 10%; color: aliceblue;">Valor restante: $0.00</p>
                                        <button id="addRowButton" class="dynamic-button" style="margin-bottom: 0;">+ Agregar forma de pago</button>
                                    </div>
                                    <div style="margin-bottom: 1rem;">
                                        <table id="dynamicTable" class="dynamic-table" style="background-color: rgba(50, 55, 60, 0.4); border: dotted;">
                                            <thead>
                                                <tr style="background-color: black; color: darkturquoise;">
                                                    <th style="width: 45%;">Descripción</th>
                                                    <th style="width: 20%;">Porcentaje (%)</th>
                                                    <th style="width: 25%;">Valor ($)</th>
                                                    <th style="width: 10%;">Acción</th>
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

                                <div class="horizontal-cuadriculado-flow text-center">
                                    <div class="contenedor-horizontal marg-2">
                                        <div class="horizontal-group" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">
                                            <div class="input-group horizontal-group col-sm-5" style="margin-bottom: 0; margin-top: 1rem; padding-bottom: 0;">
                                                <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Acta preguntas y respuestas: </label>
                                                <div class="form-group horizontal-group">
                                                    <div class="btn-group" role="group">
                                                        <button type="button" id="siPreguntas" class="btn btn-custom" onclick="toggleButtonColor('siPreguntas')">Sí</button>
                                                        <button type="button" id="noPreguntas" class="btn btn-custom" onclick="toggleButtonColor('noPreguntas')">No</button>
                                                    </div>
                                                </div>
                                            </div>

                                            <button id="btnActaPreguntas" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnActaPreguntas','Acta preguntas y respuestas:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                        </div>
                                        <div class="input-group col-sm-12" style="margin-top: -1.5rem; padding-top: 0;">
                                            <textarea id="txtActaPreguntasObs" rows="3" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>
                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow text-center">
                                    <div class="contenedor-horizontal marg-2">
                                        <div class="horizontal-group" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">
                                            <div class="input-group horizontal-group col-sm-5" style="margin-bottom: 0; margin-top: 1rem; padding-bottom: 0;">
                                                <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Acta de adjudicación: </label>
                                                <div class="form-group horizontal-group">
                                                    <div class="btn-group" role="group">
                                                        <button type="button" id="siAdj" class="btn btn-custom" onclick="toggleButtonColor('siAdj')">Sí</button>
                                                        <button type="button" id="noAdj" class="btn btn-custom" onclick="toggleButtonColor('noAdj')">No</button>
                                                    </div>
                                                </div>
                                            </div>

                                            <button id="btnActaAdjudicacion" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnActaAdjudicacion','Acta de adjudicación:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                        </div>
                                        <div class="input-group col-sm-12" style="margin-top: -1.5rem; padding-top: 0;">
                                            <textarea id="txtActaAdjObs" rows="3" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>
                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow text-center">
                                    <div class="contenedor-horizontal marg-2">
                                        <div class="horizontal-group" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">
                                            <div class="input-group horizontal-group col-sm-5" style="margin-bottom: 0; margin-top: 1rem; padding-bottom: 0;">
                                                <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Acta de negociación: </label>
                                                <div class="form-group horizontal-group">
                                                    <div class="btn-group" role="group">
                                                        <button type="button" id="siNeg" class="btn btn-custom" onclick="toggleButtonColor('siNeg')">Sí</button>
                                                        <button type="button" id="noNeg" class="btn btn-custom" onclick="toggleButtonColor('noNeg')">No</button>
                                                    </div>
                                                </div>
                                            </div>

                                            <button id="btnActaNegociacion" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnActaNegociacion','Acta de negociación:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                        </div>
                                        <div class="input-group col-sm-12" style="margin-top: -1.5rem; padding-top: 0;">
                                            <textarea id="txtActaNegObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>
                                    </div>
                                </div>

                                <!-- Cuadro Datos de otros -->
                                <div class="well datos-gerentes" style="margin: 3rem 1.7rem 0 1.7rem; padding-top: 1rem; padding-bottom: 3rem; background-color: rgba(22, 25, 25, 0.60); border: none;">

                                    <div class="well-header" style="margin-top: 0;">
                                        <div>
                                            <h4 class="well-title" style="color: aliceblue; text-align: left;">Datos otros</h4>
                                        </div>
                                    </div>

                                    <div class="horizontal-cuadriculado-flow">
                                        <div class="contenedor-horizontal">

                                            <div class="horizontal-group">
                                                <div class="input-group col-sm-6" style="margin-bottom: 0.5rem; margin-top: 2rem;">
                                                    <span class="input-azulmedio input-group-addon ingresosTitulos"><i class="glyphicon glyphicon-hdd"></i>BoM Solución:</span>
                                                    <input id="txtBomSolucion" type="text" class="form-control" placeholder="Ingrese BoM Solución" oninput="convertirAMayusculas(this)" value="">
                                                </div>
                                                <button id="btnBomSolucion" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnBomSolucion','BoM Solucion:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                            </div>
                                            <div class="input-group col-sm-12">
                                                <textarea id="txtBomSolucionObs" rows="2" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="contenedor-horizontal">
                                        <div class="input-group col-sm-8" style="margin-bottom: 0.5rem; margin-top: 3rem;">
                                            <span class="input-azulmedio input-group-addon ingresosTitulos"><i class="glyphicon glyphicon-briefcase"></i>Nombre de Mayorista - Acuerdos:</span>
                                            <input id="txtAcuMayoristas" type="text" class="form-control" placeholder="Ingrese aquí el nombre de Mayorista" oninput="convertirAMayusculas(this)" value="">
                                        </div>
                                        <div class="input-group col-sm-12">
                                            <textarea id="txtAcuMayoristasObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Acuerdos"></textarea>
                                        </div>
                                    </div>

                                    <div class="contenedor-horizontal">
                                        <div class="input-group col-sm-8" style="margin-bottom: 0.5rem; margin-top: 3rem;">
                                            <span class="input-azulmedio input-group-addon input-basic3 ingresosTitulos"><i class="glyphicon glyphicon-tent"></i>Nombre de Fabricante - Acuerdos:</span>
                                            <input id="txtAcuFabricantes" type="text" class="form-control" placeholder="Ingrese aquí el nombre de Fabricantes" oninput="convertirAMayusculas(this)" value="">
                                        </div>
                                        <div class="input-group col-sm-12">
                                            <textarea id="txtAcuFabricantesObs" rows="4" style="width: 100%; resize: vertical;" placeholder="Acuerdos"></textarea>
                                        </div>
                                    </div>


                                </div>


                                <div class="horizontal-cuadriculado-flow text-center">
                                    <div class="contenedor-horizontal marg-2">
                                        <div class="horizontal-group" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">
                                            <div class="input-group horizontal-group col-sm-5" style="margin-bottom: 0; margin-top: 1rem; padding-bottom: 0;">
                                                <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Garantías FIN: </label>
                                                <div class="form-group horizontal-group">
                                                    <div class="btn-group" role="group">
                                                        <button type="button" id="siGarantiasFIN" class="btn btn-custom" onclick="toggleButtonColor('siGarantiasFIN')">Sí</button>
                                                        <button type="button" id="noGarantiasFIN" class="btn btn-custom" onclick="toggleButtonColor('noGarantiasFIN')">No</button>
                                                    </div>
                                                </div>
                                            </div>
                                            <button id="btnGarantiasFIN" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnGarantiasFIN','Garantías FIN:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                        </div>
                                        <div class="input-group col-sm-12" style="margin-top: -1.5rem; padding-top: 0;">
                                            <textarea id="txtGarFinObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>
                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow text-center">
                                    <div class="contenedor-horizontal marg-2">
                                        <div class="horizontal-group" style="margin-bottom: 0; margin-top: 0; padding-bottom: 0;">
                                            <div class="input-group horizontal-group col-sm-5" style="margin-bottom: 0; margin-top: 1rem; padding-bottom: 0;">
                                                <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Garantías y Licencias TEC: </label>
                                                <div class="form-group horizontal-group">
                                                    <div class="btn-group" role="group">
                                                        <button type="button" id="siGarantiasTEC" class="btn btn-custom" onclick="toggleButtonColor('siGarantiasTEC')">Sí</button>
                                                        <button type="button" id="noGarantiasTEC" class="btn btn-custom" onclick="toggleButtonColor('noGarantiasTEC')">No</button>
                                                    </div>
                                                </div>
                                            </div>
                                            <button id="btnGarantiasTEC" type="button" class="btn btn-primary" onclick="MensajeCargaArchivo('btnGarantiasTEC','Garantías y Licencias TEC:')"><i class="glyphicon glyphicon-link"></i>Cargar archivo</button>
                                        </div>
                                        <div class="input-group col-sm-12" style="margin-top: -1.5rem; padding-top: 0;">
                                            <textarea id="txtGarTECObs" rows="5" style="width: 100%; resize: vertical;" placeholder="Observaciones"></textarea>
                                        </div>
                                    </div>
                                </div>

                                <div class="horizontal-cuadriculado-flow text-center marg-2" style="margin-top: 3rem;">

                                    <div class="input-group horizontal-group col-sm-10" style="display: inline-flex;">
                                        <span class="select-dorado ingresosTitulos" style="width: 45%; padding: 7px; color: #333;"><i class="glyphicon glyphicon-credit-card"></i>Número de Pedido:</span>
                                        <input id="txtGenPedidos" type="text" class="form-control" style="text-align: center;" placeholder="Ingrese aquí XXXXXX" oninput="convertirAMayusculas(this)" value="">
                                        <div class="input-group-btn">
                                            <button class="btn btn-default" type="button" id="btnBuscarDatosPersonales" onclick="buscarOrdenesServicio()"><i class="glyphicon glyphicon-search"></i></button>
                                        </div>
                                    </div>
                                    <div class="input-group col-sm-10" style="display: inline-flex;">
                                        <div style="width: 45%; background-color: #7B7D7E; display: inline-table; align-items: center;">
                                            <span class="input-group-addon ingresosTitulos" style="background-color: black; color: aliceblue; justify-self: center;"><i class="glyphicon glyphicon-credit-card"></i>Ordenes Servicio:</span>
                                        </div>
                                        <div id="listaOrdenes" style="position: relative; width: 100%; height: 100%;">
                                            <div id="listaOrdenesContent" style="position: relative; z-index: 1; background-color: rgba(0, 0, 0, 0.6); padding: 10px; width: 100%; color: aliceblue; border: solid; border-color: darkgoldenrod; border-width: thin;"></div>
                                            <!-- Establecí la posición como absolute y agregué top y left -->
                                        </div>
                                    </div>
                                    <div class="input-group col-sm-10" style="display: inline-flex;">
                                        <textarea id="txtGenPedidosObs" rows="2" style="width: 100%; resize: vertical; margin-bottom: 50px;" placeholder="Detalle / Observaciones"></textarea>
                                    </div>

                                </div>

                            </div>

                            <!-- fechas -->
                            <div id="pestaniaFechas" class="pagina" style="display: block;">
                                <div class="horizontal-group-around" style="margin-top: 4rem;">
                                    <div class="col-sm-3 text-center input-group">
                                        <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Fecha de suscripción contrato: </label>
                                        <input type="date" class="form-control" id="fechaSuscripContrato">
                                    </div>
                                    <div class="col-sm-3 text-center input-group">
                                        <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Fecha de notificación anticipo: </label>
                                        <input type="date" class="form-control" id="fechaNotifAnticipo">
                                    </div>
                                </div>

                                <div class="horizontal-group-around" style="margin-top: 4rem;">
                                    <div class="col-sm-3 text-center input-group">
                                        <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Fecha inicio activación garantía fabricante:</label>
                                        <input type="date" class="form-control" id="fechaIniActivacion">
                                    </div>
                                    <div class="col-sm-3 text-center input-group">
                                        <label class="control-label ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Fecha fin activación garantía fabricante:</label>
                                        <input type="date" class="form-control" id="fechaFinActivacion" value="">
                                    </div>
                                </div>

                                <div class="horizontal-group-around" style="margin-top: 4rem;">
                                    <div class="input-group col-sm-6">
                                        <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 50%;"><i class="glyphicon glyphicon-user"></i>Plazo activación garantía:</span>
                                        <input id="txtPlazoActGarantia" type="text" class="form-control" placeholder="Plazo" oninput="convertirAMayusculas(this)" value="">
                                    </div>
                                </div>

                                <div class="horizontal-group-around" style="margin-top: 4rem; margin-bottom: 2rem;">
                                    <div class="input-group col-sm-6">
                                        <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 50%"><i class="glyphicon glyphicon-user"></i>Plazo activación licenciamiento:</span>
                                        <input id="txtPlazoActLicencia" type="text" class="form-control" placeholder="Plazo" oninput="convertirAMayusculas(this)" value="">
                                    </div>
                                </div>

                                <div class="horizontal-group-around" style="margin-top: 4rem; margin-bottom: 4rem;">
                                    <div class="input-group col-sm-6">
                                        <span class="input-azulmedio input-group-addon ingresosTitulos" style="width: 50%"><i class="glyphicon glyphicon-user"></i>Duración vigencia tecnológica:</span>
                                        <input id="txtDuracionVigTec" type="text" class="form-control" placeholder="Ingrese la duración de vigencia" oninput="convertirAMayusculas(this)" value="">
                                    </div>
                                </div>

                                <div class="horizontal-group-around" style="margin-top: 4rem; margin-bottom: 3rem;">
                                    <div class="input-group horizontal-group col-sm-4" style="margin-top: 0; margin-bottom: 0; padding-bottom: 0; padding-top: 0">
                                        <label class="ingresosTitulos" style="color: antiquewhite;"><i class="glyphicon glyphicon-leaf"></i>Entrega de Licencias temporales: </label>
                                        <div class="horizontal-group" style="margin-top: 0; margin-bottom: 0; padding-bottom: 0; padding-top: 0">
                                            <div class="btn-group" role="group">
                                                <button type="button" id="siLicenciaTemporales" class="btn btn-custom" onclick="toggleButtonColor('siLicenciaTemporales')">Sí</button>
                                                <button type="button" id="noLicenciaTemporales" class="btn btn-custom" onclick="toggleButtonColor('noLicenciaTemporales')">No</button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <hr style="width: 80%;" />

                            </div>

                        </div>



                        <!-- Contenido de la pestaña 3" Registro de pedidos  background-color: rgba(255, 208, 140, 0.8);" -->
                        <div id="pestaniaRegistro" class="pagina oculto" style="display: none; padding: 0.5rem 0 0.2rem 0;">
                            <div class="well fondo-azul" style="margin: 10px 0 0 0; padding-top: 1rem; border: none;">

                                <div class="well-header" style="margin-top: 0;">
                                    <h3 class="well-title" style="color: aliceblue;">Registro pedidos</h3>
                                </div>


                            </div>
                        </div>



                        <!-- fechas -->
                        <div id="pestaniaCargaInfo" class="pagina oculto" style="display: none;">


                            <div class="well" style="background-color: rgba(109,109,109,0); border: none;">
                                <!-- Paginador -->
                                <div id="btn-paginas2" style="display: block; margin-top: 4rem; margin-bottom: 4rem;">
                                    <ul class="pager">
                                        <li><a href="#" type="button" style="background-color: rgba(123, 170, 245, 0.5); color: honeydew" class="ingresosTitulos btn" onclick="irAPaginaAnterior(2)">Regresar</a></li>
                                        <li><a href="#" type="button" style="background-color: rgba(125, 125, 125, 0.7); color: honeydew" class="ingresosTitulos btn btn-danger" onclick="irAPaginaSiguiente(2)" disabled>Siguiente</a></li>
                                    </ul>
                                </div>
                                <div class="datos-gerentes" id="btn-cargaContrato2" style="display: none; flex-direction: column; align-items: center;">
                                    <button class="btn btn-info col-sm-6" type="button" id="" onclick="GuardarContrato(document.getElementById('txtNumContrato').value)" style="margin: 4rem;">Guardar Datos</button>
                                </div>
                                <div class="datos-gerentes" id="btn-cargaPDF" style="display: flex; flex-direction: column; align-items: center;">
                                    <button class="btn btn-danger col-sm-6" type="button" id="" onclick="MostrarPDF()" style="margin: 4rem;">PDF</button>
                                </div>

                            </div>

                        </div>

                    </div>

                </div>
            </asp:Panel>

            <!------------------------------------------->
            <!--        Modal cargarArchivos           -->
            <!------------------------------------------->

            <div id="msgCargarArchivos" class="modal fade" role="dialog">
                <div class="modal-dialog">
                    <!-- Modal content-->
                    <div class="modal-content">
                        <div class="modal-header bg-info">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title">CARGA ARCHIVOS</h4>
                            <span id="txtnombreVentana"></span><span id="txtnombreArchivo" style="display: none;"></span>
                        </div>
                        <div class="modal-body">
                            <formview class="horizontal-group-around" id="formCargarArchivo" enctype="multipart/form-data">
                                <label for="myfile" style="width: 30%;">Seleccionar archivo:</label>
                                <input type="file" id="archivosAdjuntos" style="width: 70%;" multiple>
                                <br>
                                <br>
                            </formview>
                            <formview>
                                <div style="width: 100%;">
                                    <progress id="fileProgress" style="display: none; width: 100%;"></progress>
                                </div>
                                <div>
                                    <span id="lblMessage" style="color: Green"></span>
                                </div>

                                <br>
                                <!--button type="button" id="btnCargarArchivosAdjuntos" style="text-align: end;" >Cargar</!--button-->
                                <input type="button" id="" value="Cargar Archivos" class="btn btn-info" onclick="confirmacionCargaArchivo(document.getElementById('txtnombreArchivo').textContent)" />
                                <p class="help-block"></p>
                            </formview>
                        </div>
                    </div>
                </div>
            </div>



            <!-- Modal -->
            <!-- Modal de Visualizacion de PDF -->
            <div id="ModalEgresoInventarioPDF" class="ModalPDF modal fade" role="dialog">
                <div class="modal-dialog modal-lg">
                    <!-- Agrega la clase 'modal-lg' para que el modal sea grande -->
                    <div class="modal-content">
                        <div class="modal-header bg-info">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title">View</h4>
                        </div>
                        <div class="modal-body modal-body-custom">
                            <div class="container" style="margin-top: 3rem; margin-bottom: 1rem; padding: 0;">

                                <div class="row-flex" style="margin: 0 0 1rem 0;">
                                    <!-- Cuadros 1-->

                                    <div class="div1-mitad" style="max-width: 50%; padding: 0; margin: 0 0.5rem 0 0;">

                                        <div class="image-container .div1-mitad" style="display: grid; justify-items: center; align-items: center; padding: 0;">
                                            <img src="../carrusel/imagenes/logo_dos_textoGris.png" alt="Descripción de la imagen">
                                        </div>
                                    </div>
                                </div>

                                <div class="row" style="margin: 0 0 1rem 0;">
                                    <div class="columna div1" style="padding-left: 0.2rem; padding-bottom: 0.5rem;">
                                        <div class="centered-element letra-bold" style="margin-bottom: 1rem;">
                                            <span class="">COMPUTADORES Y EQUIPOS COMPUEQUIP DOS S.A.</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span class="letra-bold" style="width: 25%;">Dirección:</span> <span id="txtPDFDireccionDos">AV. MARISCAL SUCRE OE6-201 Y JOSE MIGUEL CARRION</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span class="letra-bold" style="width: 25%;">Matríz: </span><span id="txtPDFMatriz">MIGUEL CARRION, QUITO - Ecuador</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span class="letra-bold" style="width: 25%;">Sucursal:</span> <span id="txtPDFSucursal">QUITO</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span class="" style="width: 25%;">Contribuyente especial Nro:</span> <span id="txtContribuyenteEsp">143</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span class="" style="width: 25%;">OBLIGADO A LLEVAR CONTABILIDAD:</span> <span id="txtObLLevarCont">Si</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span class="" style="width: 25%;">Fecha Inicio Contrato:</span> <span id="txtFechaIniTrans">DD/MM/YYYY</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span class="" style="width: 25%;">Fecha Fin Contrato:</span> <span id="txtFechaFinTrans">DD/MM/YYYY</span>
                                        </div>
                                    </div>
                                </div>

                                <div class="row" style="margin: 0 0 1rem 0;">
                                    <!-- Cuadros 3-->
                                    <div class="columna div1" style="padding-left: 0.2rem; padding-bottom: 0.5rem;">
                                        <div class="centered-element">
                                            <span class="letra-bold">SERVICIOS DOS</span>
                                        </div>
                                        <div class="centered-element" style="margin-bottom: 1rem;">
                                            <span class="letra-bold">INFORMACIÓN - CONTRATO:</span>
                                        </div>

                                        <div class="doble-columna">
                                            <span style="width: 25%;">CLIENTE:</span><span id="txtPDFCliente">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">NÚMERO DE CONTRATO:</span><span id="txtPDFNumeroContrato">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">VALOR TOTAL DEL CONTRATO:</span><span id="txtPDFValorTotalContrato">-- -- --</span>
                                        </div>

                                        <div class="doble-columna">
                                            <span style="width: 25%;">Objeto:</span><span id="txtPDFObjeto">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Servicios DOS:</span><span id="txtPDFServiciosDOS">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Servicios Externos:</span><span id="txtPDFServiciosExternos">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Alcance resumen - componentes:</span><span id="txtPDFAlcance">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Hardware:</span><span id="txtPDFHardware">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Licencias:</span><span id="txtPDFLicencias">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Servicios de fabricante:</span><span id="txtPDFServiciosFabricante">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Polizas:</span><span id="txtPDFPolizas">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Términos de referencia - TDR's:</span><span id="txtPDFTerminosReferencia">-- -- --</span>
                                        </div>

                                        <div class="doble-columna">
                                            <div class="doble-columna" style="width: 62%;">
                                                <span style="width: 45%;">Formas de pago:</span><span id="txtPDFFormasPago">-- -- --</span>
                                            </div>
                                            <div class="flex-half doble-columna">
                                                <span style="width: 30%;">Otra forma de pago:</span><span id="txtPDFOtraFormaPago">-- -- --</span>
                                            </div>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Acta ed preguntas y respuestas:</span><span id="txtPDFActaPreguntasRespuestas">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Acta de adjudicación:</span><span id="txtPDFActaAdjudicacion">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Acta de negociación:</span><span id="txtPDFActaNegociacion">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">BoM Solución:</span><span id="txtPDFBoMSolucion">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Acuerdos Mayoristas:</span><span id="txtPDFAcuerdosMayoristas">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Acuerdos Fabricantes:</span><span id="txtPDFAcuerdosFabricantes">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Garantías FIN:</span><span id="txtPDFGarantiasFIN">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Garantías y Licencias TEC:</span><span id="txtPDFGarantiasLicenciasTEC">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 25%;">Generación de Pedidos y Ordenes Servicio:</span><span id="txtPDFGeneracionPedidos">-- -- --</span>
                                        </div>

                                        <div class="doble-columna">
                                            <br />
                                            <div class="doble-columna" style="width: 50%;">
                                                <span style="width: 50%;">Fecha de suscripción contrato:</span><span id="txtPDFFechaSuscripcionContrato">DD/MM/YYYY</span>
                                            </div>
                                            <div class="flex-half doble-columna">
                                                <span style="width: 50%;">Fecha de notificación anticipo:</span><span id="txtPDFFechaNotificacionAnticipo">DD/MM/YYYY</span>
                                            </div>
                                        </div>
                                        <div class="doble-columna">
                                            <br />
                                            <div class="doble-columna" style="width: 50%;">
                                                <span style="width: 50%;">Fecha inicio activación garantía fabricante:</span><span id="txtPDFFechaInicioGarantiaFabricante">DD/MM/YYYY</span>
                                            </div>
                                            <div class="flex-half doble-columna">
                                                <span style="width: 50%;">Fecha fin activación garantía fabricante:</span><span id="txtPDFFechaFinGarantiaFabricante">DD/MM/YYYY</span>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="row" style="margin: 0 0 1rem 0;">
                                    <!-- Cuadros 4-->
                                    <div class="centered-element" style="padding-bottom: 0.5rem; width: 100%;">
                                        <span class="letra-bold">FORMAS DE PAGO:</span>
                                    </div>
                                    <table id="tbl_FormasPago" class="columna div1 table table-sm" style="margin-bottom: 0;">
                                        <thead>
                                            <tr style="text-align: center;">
                                                <th style="width: 10%;" class="titulosTabla">#</th>
                                                <th style="width: 36%;" class="titulosTabla">Descripción</th>
                                                <th style="width: 18%;" class="titulosTabla">%</th>
                                                <th style="width: 18%;" class="titulosTabla">Valor</th>
                                            </tr>
                                        </thead>
                                        <tbody style="text-align: center;">
                                        </tbody>
                                    </table>
                                </div>

                                <div class="row-flex" style="margin: 0 0 1rem 0;">
                                    <!-- Cuadros 5-->
                                    <div class="columna div1 col-80" style="padding-left: 0.2rem; padding-bottom: 0.5rem;">
                                        <div class="centered-element" style="padding-bottom: 0.5rem;">
                                            <span class="letra-bold">DATOS DE CONTACTO:</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 15%; padding-right: 0.5rem;">Nombre:</span><span id="txtPDFNombre">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 15%; padding-right: 0.5rem;">Teléfono:</span><span id="txtPDFTelefono">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 15%; padding-right: 0.5rem;">Dirección:</span><span id="txtPDFDireccion">-- -- --</span>
                                        </div>
                                        <div class="doble-columna">
                                            <span style="width: 15%; padding-right: 0.5rem;">Correo:</span><span id="txtPDFCorreo">-- -- --</span>
                                        </div>
                                    </div>
                                </div>



                                <div class="row-flex letra-bold" style="margin: 0 0 2rem 0;">
                                    <!-- Cuadros 6-->
                                    <div class="columna col-80 " style="padding-left: 0.2rem;">
                                        <div style="margin-top: 5px;">
                                            <span>RECIBÍ CONFORME:</span>
                                        </div>
                                        <div style="margin-top: 5px;">
                                            <span>CEDULA DE IDENTIDAD:</span>
                                        </div>
                                        <div style="margin-top: 5px;">
                                            <span>FECHA:</span>
                                        </div>
                                        <div style="margin-top: 5px;">
                                            <span>CARGO:</span>
                                        </div>
                                        <div style="margin-top: 5px;">
                                            <span>FIRMA:</span>
                                        </div>
                                    </div>
                                </div>

                            </div>
                        </div>

                        <div class="container" style="margin: 1rem auto 0 33%">
                            <button class="btn btn-info col-sm-3" type="button" onclick="generatePDF()">Descargar</button>
                        </div>
                        <div class="modal-footer" style="margin-top: 2rem;">
                            <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                        </div>
                        <!-- /.modal-content -->
                    </div>
                    <!-- /.modal-dialog -->
                </div>
            </div>

        </div>
    </div>

</asp:Content>

<%--<asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True"></asp:DropDownList>--%>
