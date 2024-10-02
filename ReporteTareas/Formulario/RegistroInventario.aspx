<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="RegistroInventario.aspx.cs" Inherits="ReporteTareas.Formulario.RegistroInventario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/Producto.js?v=17"></script>
    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicer.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>
    <link href="../dist/css/Montos.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="panel-body">
            <!-- Nav tabs -->
            <ul class="nav nav-pills">
                <li class="active" id="tab1"><a href="#home-pills" data-toggle="tab">Ingreso de Inventario</a>
                </li>
                <li id="tab2"><a href="#profile-pills" data-toggle="tab">Reporte Kardex</a>
                </li>
                <li id="tab3"><a href="#profile-reporte" data-toggle="tab">Inventario Total</a>
                </li>
            </ul>
            <!-- Tab panes -->
            <div class="tab-content">
                <div class="tab-pane fade in active" id="home-pills">
                    <div class="col-lg-12" style="padding: 0px">
                        <div class="row">
                            <div class="breadcrumbs ace-save-state" id="breadcrumbs">
                                <ul class="breadcrumb">
                                    <li>
                                        <i class="ace-icon fa fa-home home-icon"></i>
                                        <a href="#">Inventario</a>
                                    </li>
                                    <li>
                                        <a href="#">Registro Inventario</a>
                                    </li>
                                </ul>
                            </div>
                        </div>
                        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
                            <div class="col-lg-12" style="padding: 0px">
                                <div class="panel panel-default">
                                    <div>
                                        <div style="display: none">
                                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                            <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                            <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                            <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-lg-12" style="display: block" id="editBoton">
                                <div class="col-xs-12">
                                    <div class="text-left">
                                        <button type="button" title="Nuevo producto" class="btn btn-success btn-sm" onclick="VerRegistroProductos()">
                                            <i class="fa fa-plus-circle margin-right"></i>Nuevo producto
                                        </button>
                                        <button type="button" title="Descargar producto" class="btn btn-primary btn-sm" onclick="DescargarProductos()">
                                            <i class="fa fa-plus-circle margin-right"></i>Descargar producto
                                        </button>
                                    </div>
                                </div>
                            </div>
                            <div class="col-lg-12" style="display: block" id="editMenu">
                                <div class="col-xs-12">
                                    <div class="box box-primary">
                                        <div class="box-header">
                                            <h3 class="box-title">Lista de Productos</h3>
                                        </div>
                                        <div class="box-body">
                                            <table id="tbl_Productos" class="table table-bordered table-hover text-center">
                                                <thead>
                                                    <tr>
                                                        <th>ID</th>
                                                        <th>Cantidad</th>
                                                        <th>Descripcion</th>
                                                        <th>Precio</th>
                                                        <th>Código Principal</th>
                                                        <th>Código Aux/SAP</th>
                                                        <th>N° de Serie</th>
                                                        <th>Ubicación</th>
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
                            </div>
                            <div class="col-lg-12" style="display: none" id="editProductos">
                                <div class="form-horizontal">
                                    <div class="col-lg-12">
                                        <div class="col-md-12">
                                            <div class="form-group">
                                                <label>Descripción</label>
                                            </div>
                                            <div class="form-group">
                                                <input type="text" class="form-control" id="txtDescripcion">
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-12">
                                        <div class="col-md-3">
                                            <div class="form-group">
                                                <label>Marca</label>
                                            </div>
                                            <div class="form-group">
                                                <select id="cboMarca" class="form-control">
                                                </select>
                                            </div>
                                        </div>
                                        <div class="col-md-2">
                                            <div class="form-group">
                                                <label>Código SAP</label>
                                            </div>
                                            <div class="form-group">
                                                <div class="input-group m-bot15">
                                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                                    <input type="text" class="form-control" id="txtCodigoSap">
                                                </div>
                                            </div>
                                        </div>
                                        <div class="form-group col-lg-1">
                                            <button type="button" title="Buscar Producto" class="btn btn-lg btn-success"><i class="ace-icon glyphicon-plus  bigger-110 icon-only" aria-hidden="true" onclick="BuscarReferenciaIngresoInventario()"></i></button>
                                        </div>
                                        <div class="col-md-3">
                                            <div class="form-group">
                                                <label>Num. Parte</label>
                                            </div>
                                            <div class="form-group">
                                                <div class="input-group m-bot15">
                                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                                    <input type="text" class="form-control" id="txtNumParte">
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-md-3">
                                            <div class="form-group">
                                                <label>Cantidad</label>
                                            </div>
                                            <div class="form-group">
                                                <div class="input-group m-bot15">
                                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                                    <input type="text" class="form-control" id="txtCantidad">
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-12">
                                        <div class="col-md-3">
                                            <div class="form-group">
                                                <label>Ubicacion</label>
                                            </div>
                                            <div class="form-group">
                                                <div class="input-group m-bot15">
                                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                                    <input type="text" class="form-control" id="txtUbicacion">
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-md-3">
                                            <div class="form-group">
                                                <label>Almacen</label>
                                            </div>
                                            <div class="form-group">
                                                <div class="input-group m-bot15">
                                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                                    <input type="text" class="form-control" id="txtAlmacen">
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-md-3">
                                            <div class="form-group">
                                                <label>Num. Serie</label>
                                            </div>
                                            <div class="form-group">
                                                <div class="input-group m-bot15">
                                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                                    <input type="text" class="form-control" id="txtNumSerie">
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-md-3">
                                            <div class="form-group">
                                                <label>Precio</label>
                                            </div>
                                            <div class="form-group">
                                                <div class="input-group m-bot15">
                                                    <span class="input-group-addon btn-white"><i class="fa fa-thumb-tack"></i></span>
                                                    <input type="text" class="form-control" id="txtPrecioUnitario">
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-xs-12">
                                        <br>
                                        <div class="text-left">
                                            <button type="button" title="lista producto" class="btn btn-success btn-sm" onclick="VerListaProductos()">
                                                <i class="fa fa-plus-circle margin-right"></i>Lista producto
                                            </button>
                                            <button type="button" title="producto" class="btn btn-primary btn-sm" id="btnGProducto" onclick="GuardarProducto()">Guardar</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
                <div class="tab-pane fade" id="profile-pills">
                    <div class="panel panel-default">
                        <div class="panel-body">
                            <div class="row">
                                <div class="form-group col-lg-12" style="text-align: center">
                                    <label>CONTROL KARDEX MÉTODO PROMEDIO PONDERADO</label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="form-group col-lg-4">
                                    <label>ARTICULO</label>
                                    <input readonly="readonly" type="text" class="form-control" id="txtArticulo" placeholder="ARTICULO">
                                </div>
                                <div class="form-group col-lg-3">
                                    <label>REFERENCIA</label>
                                    <input type="text" class="form-control" id="txtReferencia" placeholder="REFERENCIA">
                                </div>
                                <div class="form-group col-lg-1">
                                    <button type="button" title="Buscar Referencia" class="btn btn-sm btn-success"><i class="ace-icon glyphicon-plus  bigger-110 icon-only" aria-hidden="true" onclick="BuscarReferencia()"></i></button>
                                </div>
                                <div class="form-group col-lg-4">
                                    <label>LOCALIZACION</label>
                                    <input readonly="readonly" type="text" class="form-control" id="txtLocalizacion" placeholder="LOCALIZACION">
                                </div>
                            </div>
                            <div class="row">
                                <div class="form-group col-lg-4">
                                    <label>N° PARTE</label>
                                    <input readonly="readonly" type="text" class="form-control" id="txtNumPartes" placeholder="N° PARTE">
                                </div>
                                <div class="form-group col-lg-4">
                                    <label>UNIDAD DE MEDIDA</label>
                                    <input readonly="readonly" type="text" class="form-control" id="txtUnidadMedida" placeholder="UNIDAD DE MEDIDA">
                                </div>
                                <div class="form-group col-lg-4">
                                    <label>EXISTENCIA A LA FECHA:</label>
                                    <input readonly="readonly" type="text" class="form-control" id="txtExistencia" placeholder="EXISTENCIA A LA FECHA">
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-xs-12">
                                    <div class="text-left">
                                        <button type="button" title="Descargar reporte" class="btn btn-primary btn-sm" onclick="DescargarReporteKardex()">
                                            <i class="fa fa-plus-circle margin-right"></i>Descargar reporte
                                        </button>
                                    </div>
                                </div>
                            </div>
                            <div class="col-lg-12">
                                <div class="col-xs-12">
                                    <div class="box box-primary">
                                        <div class="box-header">
                                            <h3 class="box-title">Detalle de Kardex</h3>
                                        </div>
                                        <div class="box-body">
                                            <table id="tbl_Kardex" class="table table-bordered table-hover text-center">
                                                <thead>
                                                    <tr>
                                                        <th>ID</th>
                                                        <th>Fecha</th>
                                                        <th>Descripcion</th>
                                                        <th>Cant. EN.</th>
                                                        <th>C. Unit EN.</th>
                                                        <th>Cost. Total EN.</th>
                                                        <th>Cant. SA.</th>
                                                        <th>C. Unit SA.</th>
                                                        <th>Cost. Total SA.</th>
                                                        <th>Cant. SD.</th>
                                                        <th>C. Unit SD.</th>
                                                        <th>Cost. Total SD.</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <!-- DATA POR MEDIO DE AJAX-->
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="tab-pane fade" id="profile-reporte">
                    <div class="panel panel-default">
                        <div class="panel-body">
                            <div class="col-lg-12">
                                <div class="col-xs-12">
                                    <div class="text-left">
                                        <button type="button" title="Descargar producto" class="btn btn-primary btn-sm" onclick="DescargarInventario()">
                                            <i class="fa fa-plus-circle margin-right"></i>Descargar Inventario
                                        </button>
                                        <button type="button" title="Refresh producto" class="btn btn-success btn-sm" onclick="RefreshProducto()">
                                            <i class="fa fa-plus-circle margin-right"></i>Refresh producto
                                        </button>
                                    </div>
                                </div>
                            </div>
                            <div class="col-lg-12">
                                <div class="col-xs-12">
                                    <div class="box box-primary">
                                        <div class="box-header">
                                            <h3 class="box-title">Lista de Inventario Total</h3>
                                        </div>
                                        <div class="box-body">
                                            <table id="tbl_InventarioTotal" class="table table-bordered table-hover text-center">
                                                <thead>
                                                    <tr>
                                                        <th>ID</th>
                                                        <th>Código Aux/SAP</th>
                                                        <th>Descripcion</th>
                                                        <th>Cantidad</th>
                                                        <th>Ubicación</th>
                                                        <th>Precio</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <!-- DATA POR MEDIO DE AJAX-->
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

        </div>
    </div>
</asp:Content>
