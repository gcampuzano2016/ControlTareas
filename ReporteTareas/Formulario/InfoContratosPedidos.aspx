<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="InfoContratosPedidos.aspx.cs" Inherits="ReporteTareas.Formulario.InfoContratosPedidos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

     
    <link href="../dist/css/InfoContratos.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>
    
    <script src="../js/RegistroPedido.js?v=13"></script>

    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>

    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>
            
    <script src="../bower_components/popper/popper.min.js"></script>    

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    InformacionContratosPedidos.aspx.cs

    <div id="page-wrapper" style="padding: 0; background-color: #9DA8AD; height: max-content;">
        <div class="col-lg-12" style="background-color: #0D2538;padding-bottom: 1.5rem;padding-top: 0.5rem;">

            <div class="row" style=" margin-left: 0; margin-right: 0;">
                <div class="titulo-pag" id="breadcrumbs">

                    <ul class="breadcrumb">
                        <li style="display:flex; justify-content:space-between;">
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

                    <div class="generalesInfoContratos" >
                        !-- Contenido de la pestaña 3" Registro de pedidos  background-color: rgba(255, 208, 140, 0.8);" -->
                            <div id="pestaniaRegistro" class="pagina oculto" style="display: none; padding: 0.5rem 0 0.2rem 0;">    
                                <div class="well fondo-azul" style="margin:10px 0 0 0; padding-top: 1rem; border:none;">

                                    <div class="well-header" style="margin-top: 0;">
                                        <h3 class="well-title" style="color:aliceblue;">Registro pedidos</h3>
                                    </div>  
                            
                                    <div class="panel-body">
                                        <!-- Nav tabs -->
                                        <ul class="nav nav-pills" style="background-color:gainsboro;">
                                            <li class="active" id="tab1"><a href="#home-pills" data-toggle="tab">Registro de Pedido</a>
                                            </li>
                                            <li id="tab2"><a href="#profile-pills" data-toggle="tab">Listas de Pedidos</a>
                                            </li>
                                            <li id="tab3"><a href="#fecha-pills" data-toggle="tab">Actualizar Fecha de Pedidos</a>
                                            </li>
                                            <li id="tab4"><a href="#pedido-pills" data-toggl="tab">Actualización de Pedidos</a>
                                            </li>
                                            <li id="tab5"><a href="#polizas-pills" data-toggle="tab">Lista de Polizas</a>
                                            </li>
                                        </ul>

                                        <div style="text-align:center;padding:0 0;"> 
                                            <h3><a style="color:cornflowerblue; text-decoration:none;" href="#"><br />Quito, Ecuador</a></h3> 
                                            <iframe src="https://www.zeitverschiebung.net/clock-widget-iframe-v2?language=es&size=medium&timezone=America%2FGuayaquil" width="100%" height="115" frameborder="0" style="pointer-events: none;"></iframe>
                                        </div>

                                        <!-- Tab panes -->
                                        <div class="tab-content">
                            
                                            <div class="tab-pane fade in active " id="home-pills">
                                
                                                <div class="horizontal-group-around" style="margin-top:2rem;">
                                    
                                                    <!--div-- class="form-group col-lg-2">
                                                        <label>Fecha Registro</label>
                                                        <input type="text" class="form-control" id="txtFechaDesde" placeholder="Fecha Registro">
                                                        <p class="help-block"></p>
                                                    </!--div-->

                                                    <div class="col-sm-2 input-group" >   
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Fecha Registro: </label>
                                                        <input type="text" class="form-control" id="txtFechaDesde" >
                                                        <p class="help-block"></p>
                                                    </div>


                                                    <!--div class="form-group col-lg-3">
                                                        <label>Pedido</label>
                                                        <input type="text" class="form-control" id="txtPedido" placeholder="Pedido" oninput="BuscarPedido($('#txtPedido').val())">
                                                        <p class="help-block"></p>
                                                    </--div-->

                                                    <div class="col-sm-3 input-group" >   
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Pedido: </label>
                                                        <input type="text" class="form-control" id="txtPedido" placeholder="Pedido" oninput="BuscarPedido($('#txtPedido').val())">
                                                    </div>


                                                    <!--div class="form-group col-lg-3">
                                                        <label>Cliente</label>
                                                        <input type="text" class="form-control" id="cboCliente" placeholder="Cliente" oninput="BuscarCliente()">
                                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes">
                                                        </ul>
                                                        <p class="help-block"></p>
                                                    </!--div-->

                                                    <div class="col-sm-3 input-group" >   
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Cliente: </label>
                                                        <input type="text" class="form-control" id="cboCliente" placeholder="Cliente" oninput="BuscarCliente()">
                                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes">
                                                        </ul>
                                                    </div>

                                                    <div class="form-group col-lg-1">
                                                        <label></label>
                                                        <button class="btn btn-lg btn-success" onclick="BtnCrearCliente()" type="button">
                                                            <i class="ace-icon glyphicon-plus  bigger-110 icon-only"></i>
                                                        </button>
                                                    </div>

                                                    <!--div-- class="form-group col-lg-3">
                                                        <label>Segmentación</label>
                                                        <select id="cboSegmentacion" class="form-control">
                                                            <option value="SELECCIONAR">SELECCIONAR</option>
                                                            <option value="PUBLICO">PUBLICO</option>
                                                            <option value="PRIVADO">PRIVADO</option>
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </!--div-->

                                                    <div class="col-sm-2 input-group" >   
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Segmentación: </label>
                                                        <select id="cboSegmentacion" class="form-control">
                                                            <option value="SELECCIONAR">SELECCIONAR</option>
                                                            <option value="PUBLICO">PUBLICO</option>
                                                            <option value="PRIVADO">PRIVADO</option>
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>

                                                </div>

                                                <div class="horizontal-group-around" style="margin-top:2rem;">
                                                    <!--div-- class="form-group col-lg-2">
                                                        <label>Clasificación</label>
                                                        <input type="text" class="form-control" id="cboClasificacion" placeholder="Clasificacion" oninput="BuscarClasificacion()">
                                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboClasificacion">
                                                        </ul>
                                                        <p class="help-block"></p>
                                                    </!--div-->

                                                    <div class="col-sm-2 input-group" >   
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Clasificación: </label>
                                                        <input type="text" class="form-control" id="cboClasificacion" placeholder="Clasificacion" oninput="BuscarClasificacion()">
                                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 57px; left: 15px;" id="comboClasificacion">
                                                        </ul>
                                                        <p class="help-block"></p>
                                                    </div>

                                                    <div class="form-group col-lg-1">
                                                        <label></label>
                                                        <button class="btn btn-lg btn-success" onclick="BtnCrearClasificacion()" type="button">
                                                            <i class="ace-icon glyphicon-plus  bigger-110 icon-only"></i>
                                                        </button>
                                                    </div>
                                                    <!--div-- class="form-group col-lg-2">
                                                        <label>Valor</label>
                                                        <input type="text" class="form-control" id="txtValor" placeholder="Valor" oninput="CalcularMargen($('#txtMargen').val(),$('#txtValor').val())">
                                                        <p class="help-block"></p>
                                                    </!--div-->

                                                    <div class="col-sm-2 input-group" >   
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Valor: </label>
                                                        <input type="text" class="form-control" id="txtValor" placeholder="Valor" oninput="CalcularMargen($('#txtMargen').val(),$('#txtValor').val())">
                                                        <p class="help-block"></p>
                                                    </div>

                                                    <!--div-- class="form-group col-lg-2">
                                                        <label>Rentabilidad</label>
                                                        <input type="text" class="form-control" id="txtRentabilidad" placeholder="Rentabilidad">
                                                        <p class="help-block"></p>
                                                    </!--div-->

                                                    <div class="col-sm-2 input-group" >   
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Rentabilidad: </label>
                                                        <input type="text" class="form-control" id="txtRentabilidad" placeholder="Rentabilidad">
                                                        <p class="help-block"></p>
                                                    </div>

                                                    <!--div-- class="form-group col-lg-1">
                                                        <label>Margen</label>
                                                        <input type="text" class="form-control" id="txtMargen" placeholder="Margen" oninput="CalcularMargen($('#txtMargen').val(),$('#txtValor').val())">
                                                        <p class="help-block"></p>
                                                    </!--div-->

                                                    <div class="col-sm-2 input-group" >   
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Margen: </label>
                                                        <input type="text" class="form-control" id="txtMargen" placeholder="Margen" oninput="CalcularMargen($('#txtMargen').val(),$('#txtValor').val())">
                                                        <p class="help-block"></p>
                                                    </div>

                                                    <!--div class="form-group col-lg-4">
                                                        <label>Estado</label>
                                                        <select id="cboEstado" class="form-control" onchange="BuscarEstado()">
                                                            <option value="SELECCIONAR">SELECCIONAR</option>
                                                            <option value="FACTURADO">FACTURADO</option>
                                                            <option value="POR FACTURAR">POR FACTURAR</option>
                                                            <option value="MOTIVO DE RECHAZO">MOTIVO DE RECHAZO</option>
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </!--div-->

                                                    <div class="col-sm-2 input-group" >   
                                                        <label class="control-label ingresosTitulos" style="color:antiquewhite;"><i class="glyphicon glyphicon-leaf"></i> Estado: </label>
                                                        <select id="cboEstado" class="form-control" onchange="BuscarEstado()">
                                                            <option value="SELECCIONAR">SELECCIONAR</option>
                                                            <option value="FACTURADO">FACTURADO</option>
                                                            <option value="POR FACTURAR">POR FACTURAR</option>
                                                            <option value="MOTIVO DE RECHAZO">MOTIVO DE RECHAZO</option>
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>

                                                </div>

                                                <div class="horizontal-group-around" style="justify-content:start;">
                                                    <div class="form-group col-lg-3">
                                                        <label>Suscripciones y soporte</label>
                                                        <input class="form-check-input" type="checkbox" value="" id="idRenovacion" onchange="DesactivarCheck()">
                                                    </div>
                                                    <div class="form-group col-lg-2" id="fecha1" style="display: none">
                                                        <label>Fecha Desde</label>
                                                        <input type="text" class="form-control" id="txtFechaDesdeRenova" placeholder="Fecha Desde">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2" id="fecha2" style="display: none">
                                                        <label>Fecha Hasta</label>
                                                        <input type="text" class="form-control" id="txtFechaHastaRenova" placeholder="Fecha Hasta">
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>

                                                <div class="horizontal-group-around">
                                                    <div class="form-group col-lg-12">
                                                        <label>Detalle</label>
                                                        <textarea class="form-control" id="txtDetalle" name="txtDetalle" rows="4" cols="50"></textarea>
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>

                                                <div class="horizontal-group-around">
                                                    <div class="form-group col-lg-2">
                                                        <label>N Factura</label>
                                                        <input type="text" class="form-control" id="txtNFactura" placeholder="Número factura">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Fecha Facturacion</label>
                                                        <input type="text" class="form-control" id="txtFechaFacturacion" placeholder="Fecha Facturacion">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Fecha Estimada de Facturacion</label>
                                                        <input type="text" class="form-control" id="txtFechaEstimadaFacturacion" placeholder="Fecha Estimada de Facturacion">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Sucursal</label>
                                                        <select id="cboSucursal" class="form-control" onchange="BuscarSucursal()">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>

                                                <div class="horizontal-group-around">
                                                    <div class="form-group col-lg-12">
                                                        <label>Observación</label>
                                                        <textarea class="form-control" id="txtObservacion" name="txtObservacion" rows="4" cols="50"></textarea>
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>

                                                <div class="horizontal-group-around">
                                                    <div class="form-group col-lg-2">
                                                        <label>Vendedor</label>
                                                        <select id="cboVendedor" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Gerente Producto</label>
                                                        <select id="cboGerente" class="form-control">
                                                        </select>
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-2">
                                                        <label>Orden Compra</label>
                                                        <input type="text" class="form-control" id="txtOrdenCompra" placeholder="Orden Compra">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Orden Servicio Interno</label>
                                                        <input type="text" class="form-control" id="txtOrdenServicioInterno" placeholder="Orden Servicio Interno">
                                                        <p class="help-block"></p>
                                                    </div>
                                                    <div class="form-group col-lg-3">
                                                        <label>Orden Servicio Externo</label>
                                                        <input type="text" class="form-control" id="txtOrdenServicioExterno" placeholder="Orden Servicio Externo">
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>

                                                <div class="horizontal-group-around" style="justify-content:start;">
                                                    <div class="form-group col-lg-3">
                                                        <label>Orden Servicio Fianzas</label>
                                                        <input type="text" class="form-control" id="txtOrdenServicioFinanza" placeholder="Orden Servicio Fianzas">
                                                        <p class="help-block"></p>
                                                    </div>
                                                </div>

                                                <div class="" style="background-color:rgba(15, 15, 15, 0.7); padding:1rem;">
                                                    <div class="" style="text-align: right;" >
                                                        <button id="btnGuardar" onclick="GuardarNuevoPedido()" type="button" class="btn btn-primary">Guardar</button>
                                                        <!--<button id="btnDuplicar" onclick="DuplicarPedido()" type="button" class="btn btn-primary">Duplicar</button>-->
                                                        <button id="btnActualizar" onclick="ActualizarPedido()" type="button" class="btn btn-primary">Actualizar</button>
                                                        <button id="btnEliminar" onclick="EliminarPedido()" type="button" class="btn btn-primary">Eliminar</button>
                                                    </div>
                                                </div>
                                
                                                <div class="row">
                                                    <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                                                </div>
                                            </div>
                            
                                            <div class="tab-pane fade" id="profile-pills">
                                                <div class="row" id="ListaPedido" style="display: block;">
                                                        <div class="col-lg-12" style="padding: 0px; background-color: rgba(15, 15, 15, 0.4); margin-bottom:2rem;">
                                                        
                                                                <div style="margin-left:3rem; margin-top: 2rem; margin-bottom: 2rem;">
                                                                    <h3 class="" style="color:cadetblue;">Filtros</h3>
                                                                </div>
                                                            <hr style="width:95%;"/>

                                                                    <div class="horizontal-group-contrato">
                                                                        <div class="form-group col-lg-3">
                                                                            <label>Tipo Fecha</label>
                                                                            <select id="cboFecha" class="form-control" >
                                                                                <option value="1">Fecha Registro</option>
                                                                                <option value="2">Fecha Estimación</option>
                                                                                <option value="3">Fecha Facturación</option>
                                                                            </select>
                                                                            <p class="help-block"></p>
                                                                        </div>
                                                                        <div class="form-group col-lg-2">
                                                                            <label>Años</label>
                                                                            <select id="cboAnio" class="form-control">
                                                                            </select>
                                                                            <p class="help-block"></p>
                                                                        </div>
                                                                        <div class="form-group col-lg-5" >
                                                                            <label>Meses</label>
                                                                            <%--<select id="cboMeses" class="form-control">
														                    </select>--%>
                                                                            <select id="cboMeses"  class="form-control js-example-basic-multiple2" name="meses[]" multiple="multiple"  style="background-color:rgba(15, 15, 15, 0); width: 100%; color:black;">
                                                                            </select>
                                                                            <p class="help-block"></p>
                                                                        </div>
                                                                    </div>
                                                                    <div class="horizontal-group-contrato">
                                                                        <div class="form-group col-lg-3">
                                                                            <label>Fecha desde:</label>
                                                                            <input type="text" class="form-control" id="txtFechaConsulta1">
                                                                            <p class="help-block"></p>
                                                                        </div>
                                                                        <div class="form-group col-lg-3">
                                                                            <label>Fecha Hasta:</label>
                                                                            <input type="text" class="form-control" id="txtFechaConsulta2">
                                                                            <p class="help-block"></p>
                                                                        </div>
                                                                        <div class="form-group col-lg-3">
                                                                            <label>Filtrar sin fecha</label>
                                                                            <input class="form-check-input" type="checkbox" value="" id="idFiltros">
                                                                        </div>
                                                                        <div class="form-group col-lg-4">
                                                                            <label>Gerente Producto</label>
                                                                            <select id="cboGerente2" class="form-control">
                                                                            </select>
                                                                            <p class="help-block"></p>
                                                                        </div>
                                                                        <div class="form-group col-lg-4">
                                                                            <label>Clasificación</label>
                                                                            <select id="cboClasificacion2" class="form-control">
                                                                            </select>
                                                                            <p class="help-block"></p>
                                                                        </div>
                                                                    </div>
                                                                    <div class="horizontal-group-contrato">
                                                                        <div class="form-group col-lg-4">
                                                                            <label>Sucursal</label>
                                                                            <select id="cboSucursal2" class="js-example-basic-multiple" name="sucursal[]" multiple="multiple" onchange="BuscarSucursal2()" style=" background-color:rgba(15, 15, 15, 0); width: 100%; color:black;">
                                                                            </select>
                                                                            <p class="help-block"></p>
                                                                        </div>
                                                                        <div class="form-group col-lg-2">
                                                                            <label>Estado</label>
                                                                            <select id="cboEstado2" class="form-control">
                                                                                <option value="SELECCIONAR">SELECCIONAR</option>
                                                                                <option value="FACTURADO">FACTURADO</option>
                                                                                <option value="POR FACTURAR">POR FACTURAR</option>
                                                                            </select>
                                                                        </div>
                                                                        <div class="form-group col-lg-3">
                                                                            <label>Cliente</label>
                                                                            <input type="text" class="form-control" id="cboCliente2" placeholder="Cliente" oninput="BuscarCliente2()">
                                                                            <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes2">
                                                                            </ul>
                                                                            <p class="help-block"></p>
                                                                        </div>
                                                                        <div class="form-group col-lg-3">
                                                                            <label>Gerente Cuenta</label>
                                                                            <select id="cboVendedor2" class="form-control">
                                                                            </select>
                                                                            <p class="help-block"></p>
                                                                        </div>
                                                                    </div>
                                                                    <div class="horizontal-group-contrato">
                                                                        <div class="form-group col-lg-6">
                                                                            <label>Pedido</label>
                                                                            <input type="text" class="form-control" id="txtNumeroOrden2">
                                                                        </div>
                                                                        <div class="form-group col-lg-2">
                                                                            <label>Renovación</label>
                                                                            <input class="form-check-input" type="checkbox" value="" id="idRenovacion2">
                                                                        </div>
                                                                    </div>

                                                                <div class="" style="background-color:rgba(15, 15, 15, 0.7); padding:1rem;">
                                                                    <div class="" style="text-align: center">
                                                                        <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-primary">Consultar</button>
                                                                        <button id="btnDescarga" onclick="BtnDescarga()" type="button" class="btn btn-primary">Descargar XLS</button>
                                                                        <button id="btnVisualizar" onclick="BtnVisualizar()" type="button" class="btn btn-primary">Visualizar</button>
                                                                    </div>
                                                                </div>
                                                        </div>
                                                        <div class="col-lg-12" style="padding: 0px">
                                                            <div class="panel panel-default">
                                                                <div class="panel-heading">
                                                                    <div>
                                                                        <h4 class="" id="listPrincipalTitleLabel">Lista de Pedidos</h4>
                                                                    </div>
                                                                </div>
                                                                <!-- /.panel-heading -->
                                                                <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                                                    <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                                        <div class="row">

                                                                            <div class="col-sm-12" id='datosTablaPrincipal' style="padding: 0px">
                                                                            </div>
                                                                            <!-- /.table-responsive -->
                                                                            <!-- /.panel-body -->
                                                                        </div>
                                                                        <!-- /.panel -->
                                                                    </div>
                                                                    <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align: center">
                                                                    </div>
                                                                </div>

                                                            </div>
                                                         </div>
                                                </div>
                                                <div class="row" id="RegistroPoliza" style="display: none">
                                                    <asp:Panel ID="Panel3" runat="server" Visible="true" Enabled="true">
                                                        <div id="formActualizaTarea" class="panel panel-default col-lg-12">
                                                            <!-- Modal -->
                                                            <div class="" id="formModalActualizacion" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
                                                                <div class="modal-dialog">
                                                                    <div class="modal-content">
                                                                        <div class="modal-header">
                                                                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios()">&times;</button>
                                                                            <h4 class="modal-title" id="fomTitleLabel">Registrar Nueva Poliza</h4>
                                                                        </div>
                                                                        <div class="modal-body">
                                                                            <div class="form-group col-lg-3">
                                                                                <label>Num Factura:</label>
                                                                                <input type="text" class="form-control" id="txtNumFactura">
                                                                            </div>
                                                                            <div class="form-group col-lg-3" style="display: none">
                                                                                <label>Valor Factura:</label>
                                                                                <input type="text" class="form-control" id="txtValorFactura">
                                                                            </div>
                                                                            <div class="form-group col-lg-3" style="display: none">
                                                                                <label>OC:</label>
                                                                                <input type="text" class="form-control" id="txtOC">
                                                                            </div>
                                                                            <div class="form-group col-lg-3">
                                                                                <label>OS:</label>
                                                                                <input disabled type="text" class="form-control" id="txtOS">
                                                                            </div>
                                                                            <div class="form-group col-lg-3">
                                                                                <label>Pedido:</label>
                                                                                <input disabled type="text" class="form-control" id="txtPedidoPoliza">
                                                                            </div>
                                                                            <div class="form-group col-lg-3">
                                                                                <label>Num Poliza:</label>
                                                                                <input type="text" class="form-control" id="txtPoliza">
                                                                            </div>
                                                                            <div class="form-group col-lg-3">
                                                                                <label>Anexo:</label>
                                                                                <input type="text" class="form-control" id="txtAnexo">
                                                                            </div>
                                                                            <div class="form-group col-lg-3">
                                                                                <label>Monto:</label>
                                                                                <input type="text" class="form-control" id="txtValorMonto">
                                                                            </div>
                                                                            <div class="form-group col-lg-4">
                                                                                <label>Estado</label>
                                                                                <select id="cboEstadoPoliza2" class="form-control">
                                                                                    <option value="SELECCIONAR">SELECCIONAR</option>
                                                                                    <option value="ACTIVO">ACTIVO</option>
                                                                                    <option value="POR VENCER">POR VENCER</option>
                                                                                    <option value="VENCIDA">VENCIDA</option>
                                                                                    <option value="FIN CONTRATO">FIN CONTRATO</option>
                                                                                </select>
                                                                                <p class="help-block"></p>
                                                                            </div>
                                                                            <div class="form-group col-lg-12">
                                                                                <label>Beneficiario:</label>
                                                                                <input disabled type="text" class="form-control" id="txtBeneficiario">
                                                                            </div>
                                                                            <div class="form-group col-lg-4" style="display: none">
                                                                                <label>Fecha emision:</label>
                                                                                <input type="text" class="form-control" id="txtFechaEmisionPo">
                                                                            </div>
                                                                            <div class="form-group col-lg-4">
                                                                                <label>Fecha inicio:</label>
                                                                                <input type="text" class="form-control" id="txtFechaInicioPo">
                                                                            </div>
                                                                            <div class="form-group col-lg-4">
                                                                                <label>Fecha Vencimiento:</label>
                                                                                <input type="text" class="form-control" id="txtFechFinalPo">
                                                                            </div>
                                                                            <div class="form-group col-lg-12">
                                                                                <label>Objetivo:</label>
                                                                                <textarea class="form-control" id="txtObjetivo" name="txtObjetivo" rows="4" cols="50"></textarea>
                                                                            </div>
                                                                            <div class="form-group col-lg-6">
                                                                                <label>Tipo Poliza</label>
                                                                                <select id="cboPoliza" class="form-control">
                                                                                    <option value="SELECCIONAR">SELECCIONAR</option>
                                                                                    <option value="FIEL CUMPLIMIENTO">FIEL CUMPLIMIENTO</option>
                                                                                    <option value=" BUEN USO DE ANTICIPO">BUEN USO DE ANTICIPO</option>
                                                                                </select>
                                                                                <p class="help-block"></p>
                                                                            </div>
                                                                            <div class="row">
                                                                                <div class="form-group col-lg-8">
                                                                                    <form role="form">
                                                                                        <div class="col-lg-8">
                                                                                            <label>Adjuntar Archivos:</label>
                                                                                            <input type="file" id="archivosAdjuntos" multiple />
                                                                                            <p class="help-block"></p>
                                                                                            <progress id="fileProgress" style="display: none"></progress>
                                                                                            <hr />
                                                                                            <span id="lblMessage" style="color: Green"></span>
                                                                                        </div>
                                                                                        <div class="col-lg-4">
                                                                                            <br>
                                                                                            <input type="button" id="btnCargarArchivosAdjuntos" value="Cargar Archivos" class="btn btn-info" />
                                                                                            <p class="help-block"></p>
                                                                                        </div>
                                                                                        <div class="col-lg-12" id="divArchivosAdjuntosAnteriores">
                                                                                        </div>
                                                                                        <div class="col-lg-12" id="divArchivosAdjuntos">
                                                                                        </div>
                                                                                        <div class="col-lg-12">
                                                                                            <p class="help-block" id="messageNotify" style="display: none;"></p>
                                                                                        </div>
                                                                                    </form>
                                                                                </div>
                                                                            </div>
                                                                            <div class="row">
                                                                                <div class="form-group col-lg-12" style="text-align: right">
                                                                                    <div class="col-lg-12" style="text-align: right">
                                                                                        <button type="button" class="btn btn-danger" onclick="CancelarCambios()">Close</button>
                                                                                        <button type="button" id="btnAceptarAprobar2" onclick="GuardarPoliza()" class="btn btn-primary">Guardar</button>
                                                                                        <button type="button" id="btnAceptarAprobar3" onclick="BtnConsultaPolizaLista()" class="btn btn-primary">Lista Poliza</button>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </asp:Panel>
                                                </div>
                                            </div>
                                    
                                            <div class="tab-pane fade" id="fecha-pills">
                                                <asp:Panel ID="Panel7" runat="server" Visible="true" Enabled="true">
                                                    <div class="col-lg-12" style="padding: 0px; background-color: rgba(15, 15, 15, 0.3); margin-bottom:2rem;">
                                                        <div class="">
                                                    
                                                            <div style="margin-left:3rem; margin-top: 2rem; margin-bottom: 2rem;">
                                                                <h3 class="" style="color:cadetblue;">Filtros</h3>
                                                            </div>
                                                            <hr style="width:95%;"/>

                                                            <div class="panel-body">
                                                                <div class="horizontal-group-around">
                                                                    <div class="form-group col-lg-3">
                                                                        <label>Tipo Fecha</label>
                                                                        <select id="cboFecha1" class="form-control">
                                                                            <option value="1">Fecha Registro</option>
                                                                            <option value="2">Fecha Estimación</option>
                                                                            <option value="3">Fecha Facturación</option>
                                                                        </select>
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                    <div class="form-group col-lg-2">
                                                                        <label>Años</label>
                                                                        <select id="cboAnio1" class="form-control">
                                                                        </select>
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                    <div class="form-group col-lg-2">
                                                                        <label>Mes Actual</label>
                                                                        <select id="cboMeses1" class="form-control">
                                                                        </select>
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                    <div class="form-group col-lg-3">
                                                                        <label>Fecha Registro</label>
                                                                        <input type="text" class="form-control" id="txtFechaNueva" placeholder="Fecha ">
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div class="" style="background-color:rgba(15, 15, 15, 0.6); padding:1rem;">
                                                                <div class="" style="text-align: center">
                                                                    <button id="btnGuardarFecha" onclick="BtnActualizarFecha()" type="button" class="btn btn-primary">Guardar</button>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </asp:Panel>
                                            </div>

                                            <div class="tab-pane fade" id="pedido-pills">
                                                <asp:Panel ID="Panel8" runat="server" Visible="true" Enabled="true">
                                                    <div class="col-lg-12" style="padding: 0px; background-color: rgba(15, 15, 15, 0.3); margin-bottom:2rem;">
                                                        <div class="">
                                                    
                                                            <div style="margin-left:3rem; margin-top: 2rem; margin-bottom: 2rem;">
                                                                <h3 class="" style="color:cadetblue;">Filtros</h3>
                                                            </div>
                                                            <hr style="width:95%;"/>

                                                            <div class="panel-body">
                                                                <div class="row">
                                                                    <div class="form-group col-lg-3">
                                                                        <label>Pedido</label>
                                                                        <input type="text" class="form-control" id="txtPedido2" placeholder="Pedido" oninput="BuscarPedido2($('#txtPedido2').val())">
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                    <div class="form-group col-lg-2">
                                                                        <label>Valor</label>
                                                                        <input type="text" class="form-control" id="txtValor2" placeholder="Valor" oninput="CalcularMargen2($('#txtMargen2').val(),$('#txtValor2').val())">
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                    <div class="form-group col-lg-2">
                                                                        <label>Rentabilidad</label>
                                                                        <input type="text" class="form-control" id="txtRentabilidad2" placeholder="Rentabilidad">
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                    <div class="form-group col-lg-2">
                                                                        <label>Margen</label>
                                                                        <input type="text" class="form-control" id="txtMargen2" placeholder="Margen" oninput="CalcularMargen2($('#txtMargen2').val(),$('#txtValor2').val())">
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                    <div class="form-group col-lg-3">
                                                                        <label>Cliente</label>
                                                                        <!--<select id="cboCliente2" class="form-control">
                                                                        </select>-->
                                                                        <input type="text" class="form-control" id="cboCliente3" placeholder="Cliente" oninput="BuscarCliente3()">
                                                                        <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes3">
                                                                        </ul>
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                </div>
                                                                <div class="row">
                                                                    <div class="form-group col-lg-3">
                                                                        <label>Gerente Producto</label>
                                                                        <select id="cboGerente3" class="form-control">
                                                                        </select>
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                    <div class="form-group col-lg-3">
                                                                        <label>Gerente Cuenta</label>
                                                                        <select id="cboVendedor3" class="form-control">
                                                                        </select>
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div class="" style="background-color:rgba(15, 15, 15, 0.6); padding:1rem;">
                                                                <div class="" style="text-align: center">
                                                                    <button id="btnGuardarFecha2" onclick="ActualizarPedidoMargen()" type="button" class="btn btn-primary">Actualizar</button>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </asp:Panel>
                                            </div>

                                            <div class="tab-pane fade" id="polizas-pills" >
                                                <div class="well" style="padding: 2rem 0 0 0; background-color: rgba(15, 15, 15, 0.3); margin-bottom:2rem; border:none;">

                                                    <div class="horizontal-group-contrato">
                                                        <div class="form-group col-lg-4">
                                                            <label>Tipo Fecha</label>
                                                            <select id="cboFechaPoliza" class="form-control">
                                                                <option value="1">Fecha Inicio</option>
                                                                <option value="2">Fecha Vencimiento</option>
                                                            </select>
                                                            <p class="help-block"></p>
                                                        </div>
                                                        <div class="form-group col-lg-4">
                                                            <label>Fecha desde:</label>
                                                            <input type="text" class="form-control" id="txtFechaBusInicioPoliza">
                                                            <p class="help-block"></p>
                                                        </div>
                                                        <div class="form-group col-lg-4">
                                                            <label>Fecha Hasta:</label>
                                                            <input type="text" class="form-control" id="txtFechaBusFinalPoliza">
                                                            <p class="help-block"></p>
                                                        </div>
                                                        <div class="form-group col-lg-3">
                                                            <label>Filtrar sin fecha</label>
                                                            <input class="form-check-input" type="checkbox" value="" id="idFiltrosPoliza">
                                                        </div>
                                                    </div>
                                                    <div class="horizontal-group-contrato">
                                                        <div class="form-group col-lg-3">
                                                            <label>Factura/Poliza/Pedido</label>
                                                            <input type="text" class="form-control" id="txtBuscarProcesoPoliza" placeholder="Buscar">
                                                            <p class="help-block"></p>
                                                        </div>
                                                        <div class="form-group col-lg-3">
                                                            <label>Estado</label>
                                                            <select id="cboEstadoPoliza" class="form-control">
                                                                <option value="SELECCIONAR">SELECCIONAR</option>
                                                                <option value="ACTIVO">ACTIVO</option>
                                                                <option value="POR VENCER">POR VENCER</option>
                                                                <option value="VENCIDA">VENCIDA</option>
                                                                <option value="FIN CONTRATO">FIN CONTRATO</option>
                                                            </select>
                                                            <p class="help-block"></p>
                                                        </div>
                                                        <div class="form-group col-lg-3">
                                                            <label>Cliente</label>
                                                            <input type="text" class="form-control" id="cboCliente4" placeholder="Cliente" oninput="BuscarCliente4()">
                                                            <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes4">
                                                            </ul>
                                                            <p class="help-block"></p>
                                                        </div>
                                                    </div>
                                                    <div class="row">
                                                        <div id="divMensajes" class="col-md-6 col-md-offset-3" style=""></div>
                                                    </div>
                                                    <div class="" style="text-align: center; background-color:rgba(15, 15, 15, 0.6); padding:1rem;">
                                                        <button id="btnConsulta" onclick="BtnConsultaPoliza()" type="button" class="btn btn-primary">Consultar</button>
                                                        <button id="btnDescargar" onclick="BtnDescargaPoliza()" type="button" class="btn btn-primary">Descargar</button>
                                                    </div>
                                                </div>
                                        
                                                <div class="row" id="ListaRegistroPoliza" style="display: block">                                                    
                                                    <div class="col-lg-12" style="padding: 0px">
                                                        <div class="panel panel-default">
                                                            <div class="panel-heading">
                                                                <div>
                                                                    <div style="float: right">
                                                                    </div>
                                                                    <h4 class="" id="listPrincipalTitleLabel">Polizas Registradas</h4>
                                                                </div>
                                                            </div>
                                                            <!-- /.panel-heading -->
                                                            <div class="panel-body" style="height: 450px; overflow-y: auto; overflow-x: auto; display: block;" id="Pagina1">
                                                                <div id="table-datosTablaPrincipal1" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                                    <div class="row">
                                                                        <div class="col-sm-12" id='datosTablaPrincipalPolizas'>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                                <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align: center">
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="row" id="EnvioMail" style="display: none">
                                                    <asp:Panel ID="PanelEnvioMail" runat="server" Visible="true" Enabled="true">
                                                        <div id="formEnvioMail" class="panel panel-default col-lg-12">
                                                            <!-- Modal -->
                                                            <div class="" id="formModalEnvioMail" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
                                                                <div class="modal-dialog">
                                                                    <div class="modal-content">
                                                                        <div class="modal-header">
                                                                            <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarEnvioMail()">&times;</button>
                                                                            <h4 class="modal-title" id="fomTitleLabel">Enviar mail poliza</h4>
                                                                        </div>
                                                                        <div class="modal-body">
                                                                            <!-- Form -->
                                                                            <div class="panel panel-default">
                                                                                <div class="panel-body">
                                                                                    <div class="col-lg-12">
                                                                                        <form role="form">
                                                                                            <div class="row">
                                                                                                <div class="form-group col-lg-4">
                                                                                                    <label>Num Factura:</label>
                                                                                                    <input disabled type="text" class="form-control" id="txtNumFacturaEM">
                                                                                                </div>
                                                                                                <div class="form-group col-lg-3">
                                                                                                    <label>Pedido:</label>
                                                                                                    <input disabled type="text" class="form-control" id="txtPedidoPolizaEM">
                                                                                                </div>
                                                                                                <div class="form-group col-lg-3">
                                                                                                    <label>OS:</label>
                                                                                                    <input disabled type="text" class="form-control" id="txtOSEM">
                                                                                                </div>
                                                                                                <div class="form-group col-lg-12">
                                                                                                    <label>Beneficiario:</label>
                                                                                                    <input disabled type="text" class="form-control" id="txtBeneficiarioEM">
                                                                                                </div>
                                                                                                <div class="form-group col-lg-12">
                                                                                                    <label>Enviar Email Destinatario:</label>
                                                                                                    <input type="text" class="form-control" id="txtEmailEnvio">
                                                                                                </div>
                                                                                                <div class="form-group col-lg-12">
                                                                                                    <label>Detalle Mail:</label>
                                                                                                    <textarea class="form-control" id="txtDetalleMail" name="txtDetalleMail" rows="4" cols="50"></textarea>
                                                                                                </div>
                                                                                            </div>
                                                                                            <div class="row">
                                                                                                <div class="form-group col-lg-12" style="text-align: right">
                                                                                                    <div class="col-lg-12" style="text-align: right">
                                                                                                        <button type="button" class="btn btn-danger" onclick="CancelarEnvioMail()">Close</button>
                                                                                                        <button type="button" id="btnAceptarAprobar2" onclick="GuardarEnvioCorreo()" class="btn btn-primary">Enviar Email</button>
                                                                                                    </div>
                                                                                                </div>
                                                                                            </div>
                                                                                        </form>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </asp:Panel>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    </div>
                                </div>
                        
                    
                    
                            <!-- fechas -->     
                            <div id="pestaniaCargaInfo" class="pagina oculto" style="display:none;">                       


                                    <div class="well" style="background-color:rgba(109,109,109,0); border:none;">
                                         <!-- Paginador -->
                                        <div id="btn-paginas2" style="display:block; margin-top:4rem; margin-bottom:4rem;">                             
                                             <ul class="pager">
                                                <li><a href="#" type="button" style="background-color:rgba(123, 170, 245, 0.5); color:honeydew" class="ingresosTitulos btn" onclick="irAPaginaAnterior(2)"> Información Contrato</a></li>
                                                <li><a href="#" type="button" style="background-color:rgba(125, 125, 125, 0.7); color:honeydew" class="ingresosTitulos btn btn-danger" onclick="irAPaginaSiguiente(2)" disabled>Siguiente</a></li>
                                             </ul>
                                         </div>
                                        <div class="datos-gerentes" id="btn-cargaContrato2" style="display: none; flex-direction: column; align-items: center;">          
                                            <button class="btn btn-info col-sm-6" type="button" id="" onclick="GuardarContrato(document.getElementById('txtNumContrato').value)" style=" margin: 4rem;">Guardar Datos</button>                            
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
                                <h4 class="modal-title">CARGA ARCHIVOS</h4><span id="txtnombreVentana"></span><span id="txtnombreArchivo" style="display:none;"></span>
                            </div>
                            <div class="modal-body">                             
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
                                    <input type="button" id="" value="Cargar Archivos" class="btn btn-info" onclick="confirmacionCargaArchivo(document.getElementById('txtnombreArchivo').textContent)"/>
                                     <p class="help-block"></p>
                                </formview>
                            </div>    
                        </div>
                    </div>
                </div>       
                
                <!-- Modal -->
                <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-labelledby="modalMensajeInformativoLabel" aria-hidden="true">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header" style="background: #fcf8e3" id="modalMensajeInformativoTipo">
                                <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                                <h4 class="modal-title" id="myModalLabel">Informativo</h4>
                            </div>
                            <div class="modal-body" id="MensajeInformativo">
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                            </div>
                        </div>
                        <!-- /.modal-content -->
                    </div>
                    <!-- /.modal-dialog -->
                </div>
                <!-- /.modal -->


                <!-- Modal -->
                <div class="modal fade" id="modalMensajeAprobar" tabindex="-1" role="dialog" aria-labelledby="modalMensajeAprobarLabel" aria-hidden="true">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header" style="background: #d9edf7" id="modalMensajeAprobarTipo">
                                <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                                <h4 class="modal-title" id="modalMensajeAprobarLabelTitulo">Registro Cliente</h4>
                            </div>
                            <div class="modal-body" id="divMensajeAprobar">
                                <div class="form-group col-lg-12">
                                    <label>Descripción:</label>
                                    <!--<input type="text" class="form-control" id="txtObservacionReq">-->
                                    <textarea class="form-control" id="txtObservacionReq" name="txtObservacionReq" rows="4" cols="50"></textarea>
                                    <p class="help-block"></p>
                                </div>
                            </div>
                            <div class="modal-footer">
                                <input type="text" class="form-control" style="display: none;" id="txtCodigo">
                                <button type="button" class="btn btn-danger" data-dismiss="modal">Close</button>
                                <button type="button" id="btnAceptarAprobar" onclick="GuardarCliente()" class="btn btn-primary">Guardar</button>
                            </div>
                        </div>
                        <!-- /.modal-content -->
                    </div>
                    <!-- /.modal-dialog -->
                </div>

                <!-- Modal -->
                <div class="modal fade" id="modalMensajeConfirmacion" tabindex="-1" role="dialog" aria-labelledby="modalMensajeConfirmacionLabel" aria-hidden="true">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header" style="background: #fcf8e3" id="modalMensajeConfirmacionTipo">
                                <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                                <h4 class="modal-title" id="myModalConfirmacionLabel">Confirmación</h4>
                            </div>
                            <div class="modal-body" id="modalMensajeConfirmacioMensaje">
                            </div>
                            <div class="modal-footer">
                                <input type="text" class="form-control" id="txtCodigoItem" disabled style="visibility: hidden">
                                <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                                <button type="button" onclick="EliminarArchivo()" class="btn btn-primary" id="btnBorrarArchivo" hidden>Continuar Borrado</button>
                            </div>
                        </div>
                        <!-- /.modal-content -->
                    </div>
                    <!-- /.modal-dialog -->
                </div>
                       
    
</asp:Content>