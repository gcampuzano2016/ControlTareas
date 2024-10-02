<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="EgresoInventario.aspx.cs" Inherits="ReporteTareas.Formulario.EgresoInventario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/Inventario.js?v=33"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>
    <link href="../dist/css/Montos.css" rel="stylesheet" />
    <link href="../dist/css/styleInventario.css" rel="stylesheet" />
        <!-- El script de la librería PDF-->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.9.2/html2pdf.bundle.min.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="col-lg-12" style="padding: 0px">
            <div class="row">
                <div class="breadcrumbs ace-save-state" id="breadcrumbs">
                    <ul class="breadcrumb">
                        <li>
                            <i class="ace-icon fa fa-home home-icon"></i>
                            <a href="#">Inventario</a>
                        </li>
                        <li>
                            <a href="#">Egreso Inventario</a>
                        </li>
                    </ul>
                </div>
            </div>
            <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
                <div class="col-lg-12" style="padding: 0px">
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <div style="display: none">
                                <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            </div>
                        </div>
                        <div class="panel-body">
                            <!-- Nav tabs -->
                            <ul class="nav nav-pills">
                                <li class="active" id="tab1"><a href="#home-pills" data-toggle="tab">Egreso</a>
                                </li>
                                <li id="tab2"><a href="#profile-pills" data-toggle="tab">Listas de Egreso</a>
                                </li>
                            </ul>
                            <!-- Tab panes -->
                            <div class="tab-content">
                                <div class="tab-pane fade in active" id="home-pills">
                                    <div class="panel panel-default">
                                        <div class="row">
                                            <div class="col-md-12 col-sm-12">
                                                <div class="form-group col-lg-1">
                                                    <button id="btnNuevo" onclick="NuevoEgreso()" type="button" class="btn btn-info">Nuevo</button>
                                                </div>
                                                <div class="form-group col-lg-1">
                                                    <button id="btnGuardar" onclick="GuardarNuevoEgreso()" type="button" class="btn btn-success">Guardar</button>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-md-12 col-sm-12">
                                                <div class="form-group col-lg-2">
                                                    <label>Fecha Actual</label>
                                                    <input type="text" class="form-control" id="txtFechaActual" placeholder="Fecha Actual">
                                                </div>
                                                <div class="form-group col-lg-2">
                                                    <label>Num Pedido</label>
                                                    <input type="text" class="form-control" id="txtCodigoProceso" placeholder="Num Pedido" value="0000" oninput="ConsultarPedido()">
                                                </div>
                                                <div class="form-group col-lg-8">
                                                    <label>Cliente</label>
                                                    <input type="text" class="form-control" id="cboCliente2" placeholder="Cliente" oninput="BuscarCliente2()" readonly>
                                                    <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes2">
                                                    </ul>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-md-12 col-sm-12">
                                            <div class="form-group col-lg-12">
                                                <label>Observación</label>
                                                <textarea class="form-control" id="txtObservacion" name="txtObservacion" rows="2" cols="50"></textarea>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-md-12 col-sm-12">
                                                <div class="col-xs-12">
                                                    <section class="panel">
                                                        <header class="panel-heading tab-bg-dark-navy-blue ">
                                                            <ul class="nav nav-tabs">
                                                                <li class="">
                                                                    <a data-toggle="tab" aria-expanded="false"><i class="fa fa-list"></i>&nbsp;&nbsp;Detalles del egreso</a>
                                                                </li>
                                                            </ul>
                                                        </header>
                                                        <div class="panel-body">
                                                            <div class="tab-content">
                                                                <div class="tab-pane adv-table active" id="detallesFactura">
                                                                    <div class="col-xs-12 col-sm-12 col-md-12 col-lg-12 padding-1">
                                                                        <button type="button" title="Buscar producto o servicio" class="btn btn-primary btn-sm" onclick="BuscarProducto()">
                                                                            <i class="fa fa-search margin-right"></i>Buscar producto
                                                                        </button>
                                                                    </div>
                                                                    <br>
                                                                    <hr>
                                                                    <table id="tbl_Detalle" class="table table-items">
                                                                        <thead>
                                                                            <tr>
                                                                                <th width="5%" class="text-center" data-override="cantidad">Cantidad</th>
                                                                                <th width="70%" class="text-center">Detalle</th>
                                                                                <th width="10%" class="text-center" data-override="precio">Precio ($)</th>
                                                                                <th width="10%" class="text-center">Total</th>
                                                                                <th width="5%" class="text-center"></th>
                                                                            </tr>
                                                                        </thead>
                                                                        <tbody>
                                                                        </tbody>
                                                                    </table>
                                                                </div>

                                                            </div>
                                                        </div>
                                                    </section>
                                                </div>
                                                <div class="col-xs-12 col-sm-12 col-md-12 col-lg-12">
                                                    <hr>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-xs-12 col-sm-12 col-md-7 col-lg-7">
                                            </div>
                                            <div class="col-xs-12 col-sm-12 col-md-5 col-lg-4">
                                                <ul class="unstyled amounts">
                                                    <li class="grand-total" style="text-align: right">Valor total: $<span id="valorTotal">0.00</span></li>
                                                </ul>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="tab-pane fade" id="profile-pills">
                                    <div class="panel panel-default">
                                        <div class="row">
                                            <div class="col-md-12 col-sm-12">
                                                <div class="form-group col-lg-2">
                                                    <label>Fecha Actual</label>
                                                    <input type="text" class="form-control" id="txtFechaActual2" placeholder="Fecha Inicio">
                                                </div>
                                                <div class="form-group col-lg-2">
                                                    <label>Fecha Actual</label>
                                                    <input type="text" class="form-control" id="txtFechaActual3" placeholder="Fecha Final">
                                                </div>
                                                <div class="form-group col-lg-8">
                                                    <label>Cliente</label>
                                                    <input type="text" class="form-control" id="cboCliente3" placeholder="Cliente" oninput="BuscarCliente3()">
                                                    <ul class="typeahead dropdown-menu" role="listbox" style="top: 65px; left: 10px;" id="comboClientes3">
                                                    </ul>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-xs-12">
                                            <div class="box box-primary">
                                                <div class="box-header">
                                                    <h3 class="box-title">Lista de egresos</h3>
                                                </div>
                                                <div class="box-body">
                                                    <table id="tbl_Egreso" class="table table-bordered table-hover text-center">
                                                        <thead>
                                                            <tr>
                                                                <th>ID</th>
                                                                <th>Fecha</th>
                                                                <th>Cliente</th>
                                                                <th>Total</th>
                                                                <th>Num. Pedido</th>
                                                                <th>Acciones</th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                            <!-- DATA POR MEDIO DE AJAX-->
                                                        </tbody>
                                                    </table>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-xs-12">
                                            <div class="" style="text-align: center">
                                                <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-primary">Consultar</button>
                                                <button id="btnDescarga" onclick="BtnDescarga()" type="button" class="btn btn-primary">Descargar XLS</button>
                                            </div>
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

    <div class="modal fade" id="imodalProductos" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title" id="myModalLabelListaProductos">Buscar producto</h4>
                </div>
                <div class="box box-primary">
                    <div class="box-body table-responsive">
                        <div class="col-lg-12">
                            <div class="col-md-4 col-sm-5 inv-label">Descripción: </div>
                            <div class="col-md-8 col-sm-7">
                                <input style="text-transform: uppercase" type="text" class="form-control" id="txtDescripcionProducto" placeholder="buscar producto por descripción" oninput="BuscarProductoLike()">
                            </div>
                        </div>
                        <div class="col-lg-12">
                            <table id="tbl_Productos" class="table table-striped table-bordered table-hover dataTable no-footer dtr-inline">
                                <thead>
                                    <th>ID</th>
                                    <th>Codigo SAP</th>
                                    <th>Descripcion</th>
                                    <th>Precio</th>
                                    <th>Stock</th>
                                    <th>Ubicación</th>
                                    <th>Num. Serie</th>
                                    <th>Acciones</th>
                                </thead>
                            </table>
                        </div>
                    </div>
                </div>
                <div class="modal-footer" style="display: none">
                    <div class="form-group">
                        <button type="button" class="btn btn-white btn-primary" id="btnProducto" onclick="NuevoProducto()" hidden>Nuevo Producto</button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal -->
    <!-- Modal de Visualizacion de PDF -->
    <div id="ModalEgresoInventarioPDF" class="modal fade" role="dialog">
        <div class="modal-dialog modal-lg"> <!-- Agrega la clase 'modal-lg' para que el modal sea grande -->
            <div class="modal-content">
                <div class="modal-header bg-info">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">View</h4>
                </div>
                <div class="modal-body modal-body-custom">
                    <div class="container" style="margin-top:3rem; margin-bottom: 1rem; padding:0;">

                        <div class="row-flex" style="margin: 0 0 1rem 0;"><!-- Cuadros 1-->            
                            
                            <div class="div1-mitad" style="max-width:50%; padding: 0; margin: 0 0.5rem 0 0;">

                                <div class="image-container .div1-mitad" style=" display: grid; justify-items: center; align-items: center; padding: 0; ">
                                    <img src="../carrusel/imagenes/logo_dos_textoGris.png" alt="Descripción de la imagen">
                                </div>
                               
                                <div class="columna text-container" style="padding-left: 0.2rem;">
                                    <div class="letra-bold">
                                        <span class="">COMPUTADORES Y EQUIPOS COMPUEQUIP DOS S.A.</span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="letra-bold" style="width: 25%;">Dirección:</span> <span id="txtDireccion">AV. MARISCAL SUCRE OE6-201 Y JOSE MIGUEL CARRION</span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="letra-bold" style="width: 25%;">Matríz: </span> <span id="txtMatriz">MIGUEL CARRION, QUITO - Ecuador</span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="letra-bold" style="width: 25%;">Sucursal:</span> <span id="txtSucursal">QUITO</span>
                                    </div>
                    
                                    <div class="doble-columna">
                                        <span class="" style="width: 50%;">Contribuyente especial Nro:</span> <span id="txtContribuyenteEsp">143</span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="" style="width: 50%;">OBLIGADO A LLEVAR CONTABILIDAD:</span> <span id="txtObLLevarCont">Si</span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="" style="width: 50%;">Fecha Inicio Transporte:</span> <span id="txtFechaIniTrans"></span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="" style="width: 50%;">Fecha Fin Transporte:</span> <span id="txtFechaFinTrans"></span>
                                    </div>
                                </div>                
                           </div>

                            <div class="columna div2 letra-bold" style="max-width:50%; padding-left: 0.2rem; margin: 0 0 0 0.5rem;">
                                <div class="centered-element">
                                    <span class="">RUC:  1790885186001</span>
                                </div> 
                                <div class="centered-element">
                                    <span class="">ACTA DE ENTREGA - REMISION:</span>
                                </div>
                                <br />
                                <div class="">
                                    <span>No. LOGISTICA-ACTA</span>
                                </div>
                                <div class="">
                                    <span>NÚMERO DE AUTORIZACIÓN</span>
                                </div>    
                                <br />
                                <div class="doble-columna">                                       
                                    <span style="width: 65%;">FECHA Y HORA DE AUTORIZACIÓN:</span> <span id="txtFechaAuto"></span>
                                </div>
                                <div class="">
                                    <span>AMBIENTE:</span>
                                </div>
                                <div class="">
                                    <span>EMISIÓN:</span>
                                </div>
                           </div>            
                        </div>

                        <div class="row" style="margin: 0 0 1rem 0;"> <!-- Cuadros 2-->
                            <div class="columna div1" style="padding-left: 0.2rem;">
                                <div class="doble-columna">
                                     <span style="width: 25%;">RUC/CI (Transportista)</span><span ID="TxtRuc">17908851860001</span> 
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Razón Social / Nombres y apellidos:</span><span ID="TxtRazonSoc">COMPUTADORES Y EQUIPOS COMPUEQUIP DOS S.A.</span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Placa:</span><span ID="TxtPlaca">0</span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Punto de Partida:</span><span ID="TxtPuntoPar">AV. OCCIDENTAL OE6-201 Y JOSE MIGUEL CARRION</span>          
                                </div>
                            </div>
                     </div>

                        <div class="row" style="margin: 0 0 1rem 0;"> <!-- Cuadros 3-->
                            <div class="columna div1" style="padding-left: 0.2rem;">
                                <div class="doble-columna">
                                    <span style="width: 25%;">Comprobante de venta:</span><span id="TxtComprobanteVnt">143654</span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Número de autorización:</span><span id="TxtNumAutorizacion">000654</span>
                                </div>
                                <div class="doble-columna">
                                    <div class="doble-columna" style="width: 62%;">
                                        <span style="width: 45%;">Motivo Traslado:</span><span id="TxtMotivoTras">Compra directa</span>
                                    </div>                    
                                    <div class="flex-half doble-columna">
                                        <span style="width: 30%;">Fecha de Emisión:</span><span id="TxtFechaEmision"></span>
                                    </div>
                                </div>  
                                <div class="doble-columna">   
                                    <br />
                                    <div class="doble-columna" style="width: 62%;">                                            
                                        <span style="width: 45%;">Destino (Punto de llegada):</span><span id="TxtDestino">MARISCAL SUCRE OE6-201 Y JOSE MIGUEL</span>
                                    </div>
                                    <div class="flex-half doble-columna">                                        
                                        <span style="width: 30%;">CIUDAD:</span><span id="TxtCiudad3">QUITO</span>
                                    </div>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">RUC / CI (Destinatario):</span><span id="TxtRucDes">172383743</span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Razón Social / Nombres y apellidos:</span><span id="TxtRazonSocNomApe"></span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Documento Aduanero:</span><span id="TxtDocAduanero"></span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Código Establecimiento Destino:</span><span id="TxtCodEstablecimientoDes"></span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Ruta:</span><span id="TxtRuta3"></span>
                                </div>
                            </div>
                        </div>

                        <div class="row" style="margin: 0 0 1rem 0;"> <!-- Cuadros 4-->
                            <table id="tbl_ProductosInv" class="columna div1 table table-sm" style="margin-bottom:0;">
                                <thead>
                                    <tr style="text-align:center;">
                                        <th style="width:10%;" class="titulosTabla">Cantidad</th>
                                        <th style="width:36%;" class="titulosTabla">Descripción</th>
                                        <th style="width:18%;" class="titulosTabla">Código Principal</th>
                                        <th style="width:18%;" class="titulosTabla">Código Auxiliar</th>
                                        <th style="width:18%;" class="titulosTabla">N° de Serie</th>
                                    </tr>
                                </thead>
                                <tbody style="text-align:center;">

                                </tbody>
                            </table>
                        </div>

                        <div class="row-flex" style="margin: 0 0 1rem 0;"> <!-- Cuadros 5-->
                            <div class="columna col-80" style="padding-left: 0.2rem;">
                                <div>
                                    <span>Pedido de Venta:</span>
                                </div>
                                <div>
                                    <span>Email:</span>
                                </div>
                                <div>
                                    <span>Atención:</span>
                                </div>
                                <div>
                                    <span>Observaciones:</span>
                                </div>
                            </div>            
                        </div>

                        <div class="row-flex letra-bold" style="margin: 0 0 2rem 0;"> <!-- Cuadros 6-->
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
                    <button class="btn btn-info col-sm-3" type="button"onclick="generatePDF()">Descargar</button>
                </div>
                <div class="modal-footer" style="margin-top:2rem;">                
                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                </div>
                <!-- /.modal-content -->
            </div>
        <!-- /.modal-dialog -->    
       </div>
    </div>    <!-- Modal -->
    <!-- Modal de Visualizacion de PDF -->
    <div id="ModalEgresoInventarioPDF" class="modal fade" role="dialog">
        <div class="modal-dialog modal-lg"> <!-- Agrega la clase 'modal-lg' para que el modal sea grande -->
            <div class="modal-content">
                <div class="modal-header bg-info">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">View</h4>
                </div>
                <div class="modal-body modal-body-custom">
                    <div class="container" style="margin-top:3rem; margin-bottom: 1rem; padding:0;">

                        <div class="row-flex" style="margin: 0 0 1rem 0;"><!-- Cuadros 1-->            
                            
                            <div class="div1-mitad" style="max-width:50%; padding: 0; margin: 0 0.5rem 0 0;">

                                <div class="image-container .div1-mitad" style=" display: grid; justify-items: center; align-items: center; padding: 0; ">
                                    <img src="../carrusel/imagenes/logo_dos_textoGris.png" alt="Descripción de la imagen">
                                </div>
                               
                                <div class="columna text-container" style="padding-left: 0.2rem;">
                                    <div class="letra-bold">
                                        <span class="">COMPUTADORES Y EQUIPOS COMPUEQUIP DOS S.A.</span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="letra-bold" style="width: 25%;">Dirección:</span> <span id="txtDireccion">AV. MARISCAL SUCRE OE6-201 Y JOSE MIGUEL CARRION</span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="letra-bold" style="width: 25%;">Matríz: </span> <span id="txtMatriz">MIGUEL CARRION, QUITO - Ecuador</span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="letra-bold" style="width: 25%;">Sucursal:</span> <span id="txtSucursal">QUITO</span>
                                    </div>
                    
                                    <div class="doble-columna">
                                        <span class="" style="width: 50%;">Contribuyente especial Nro:</span> <span id="txtContribuyenteEsp">143</span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="" style="width: 50%;">OBLIGADO A LLEVAR CONTABILIDAD:</span> <span id="txtObLLevarCont">Si</span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="" style="width: 50%;">Fecha Inicio Transporte:</span> <span id="txtFechaIniTrans"></span>
                                    </div>
                                    <div class="doble-columna">
                                        <span class="" style="width: 50%;">Fecha Fin Transporte:</span> <span id="txtFechaFinTrans"></span>
                                    </div>
                                </div>                
                           </div>

                            <div class="columna div2 letra-bold" style="max-width:50%; padding-left: 0.2rem; margin: 0 0 0 0.5rem;">
                                <div class="centered-element">
                                    <span class="">RUC:  1790885186001</span>
                                </div> 
                                <div class="centered-element">
                                    <span class="">ACTA DE ENTREGA - REMISION:</span>
                                </div>
                                <br />
                                <div class="">
                                    <span>No. LOGISTICA-ACTA</span>
                                </div>
                                <div class="">
                                    <span>NÚMERO DE AUTORIZACIÓN</span>
                                </div>    
                                <br />
                                <div class="doble-columna">                                       
                                    <span style="width: 65%;">FECHA Y HORA DE AUTORIZACIÓN:</span> <span id="txtFechaAuto"></span>
                                </div>
                                <div class="">
                                    <span>AMBIENTE:</span>
                                </div>
                                <div class="">
                                    <span>EMISIÓN:</span>
                                </div>
                           </div>            
                        </div>

                        <div class="row" style="margin: 0 0 1rem 0;"> <!-- Cuadros 2-->
                            <div class="columna div1" style="padding-left: 0.2rem;">
                                <div class="doble-columna">
                                     <span style="width: 25%;">RUC/CI (Transportista)</span><span ID="TxtRuc">17908851860001</span> 
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Razón Social / Nombres y apellidos:</span><span ID="TxtRazonSoc">COMPUTADORES Y EQUIPOS COMPUEQUIP DOS S.A.</span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Placa:</span><span ID="TxtPlaca">0</span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Punto de Partida:</span><span ID="TxtPuntoPar">AV. OCCIDENTAL OE6-201 Y JOSE MIGUEL CARRION</span>          
                                </div>
                            </div>
                     </div>

                        <div class="row" style="margin: 0 0 1rem 0;"> <!-- Cuadros 3-->
                            <div class="columna div1" style="padding-left: 0.2rem;">
                                <div class="doble-columna">
                                    <span style="width: 25%;">Comprobante de venta:</span><span id="TxtComprobanteVnt">143654</span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Número de autorización:</span><span id="TxtNumAutorizacion">000654</span>
                                </div>
                                <div class="doble-columna">
                                    <div class="doble-columna" style="width: 62%;">
                                        <span style="width: 45%;">Motivo Traslado:</span><span id="TxtMotivoTras">Compra directa</span>
                                    </div>                    
                                    <div class="flex-half doble-columna">
                                        <span style="width: 30%;">Fecha de Emisión:</span><span id="TxtFechaEmision"></span>
                                    </div>
                                </div>  
                                <div class="doble-columna">   
                                    <br />
                                    <div class="doble-columna" style="width: 62%;">                                            
                                        <span style="width: 45%;">Destino (Punto de llegada):</span><span id="TxtDestino">MARISCAL SUCRE OE6-201 Y JOSE MIGUEL</span>
                                    </div>
                                    <div class="flex-half doble-columna">                                        
                                        <span style="width: 30%;">CIUDAD:</span><span id="TxtCiudad3">QUITO</span>
                                    </div>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">RUC / CI (Destinatario):</span><span id="TxtRucDes">172383743</span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Razón Social / Nombres y apellidos:</span><span id="TxtRazonSocNomApe"></span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Documento Aduanero:</span><span id="TxtDocAduanero"></span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Código Establecimiento Destino:</span><span id="TxtCodEstablecimientoDes"></span>
                                </div>
                                <div class="doble-columna">
                                    <span style="width: 25%;">Ruta:</span><span id="TxtRuta3"></span>
                                </div>
                            </div>
                        </div>

                        <div class="row" style="margin: 0 0 1rem 0;"> <!-- Cuadros 4-->
                            <table id="tbl_ProductosInv" class="columna div1 table table-sm" style="margin-bottom:0;">
                                <thead>
                                    <tr style="text-align:center;">
                                        <th style="width:10%;" class="titulosTabla">Cantidad</th>
                                        <th style="width:36%;" class="titulosTabla">Descripción</th>
                                        <th style="width:18%;" class="titulosTabla">Código Principal</th>
                                        <th style="width:18%;" class="titulosTabla">Código Auxiliar</th>
                                        <th style="width:18%;" class="titulosTabla">N° de Serie</th>
                                    </tr>
                                </thead>
                                <tbody style="text-align:center;">

                                </tbody>
                            </table>
                        </div>

                        <div class="row-flex" style="margin: 0 0 1rem 0;"> <!-- Cuadros 5-->
                            <div class="columna col-80" style="padding-left: 0.2rem;">
                                <div>
                                    <span>Pedido de Venta:</span>
                                </div>
                                <div>
                                    <span>Email:</span>
                                </div>
                                <div>
                                    <span>Atención:</span>
                                </div>
                                <div>
                                    <span>Observaciones:</span>
                                </div>
                            </div>            
                        </div>

                        <div class="row-flex letra-bold" style="margin: 0 0 2rem 0;"> <!-- Cuadros 6-->
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
                    <button class="btn btn-info col-sm-3" type="button"onclick="generatePDF()">Descargar</button>
                </div>
                <div class="modal-footer" style="margin-top:2rem;">                
                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                </div>
                <!-- /.modal-content -->
            </div>
        <!-- /.modal-dialog -->    
       </div>
    </div>

</asp:Content>
