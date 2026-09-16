<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionPerfiles.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionPerfiles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionPerfiles.js?v=1" type="text/javascript"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="panel-body">
                    <div class="row">
                        <h3>Administración de Perfiles</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">Buscar</div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-6">
                            <label for="txtBuscar">Nombre del perfil</label>
                            <input type="text" class="form-control" id="txtBuscar" placeholder="Escriba parte del nombre" />
                        </div>
                    </div>
                    <div id="divMensajes"></div>
                </div>
                <div class="panel-footer" style="text-align: center">
                    <button id="btnBuscar" onclick="BuscarPerfiles()" type="button" class="btn btn-default">Consultar</button>
                    <button id="btnNuevo" onclick="NuevoPerfil()" type="button" class="btn btn-primary">Nuevo Perfil</button>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h4>Listado de Perfiles</h4>
                </div>
                <div class="panel-body" style="padding: 0">
                    <div class="dataTables_wrapper">
                        <div id="datosTablaPerfiles"></div>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" id="panelDetalle" style="padding: 0px; display: none">
            <div class="panel panel-default">
                <div class="panel-heading">Datos del perfil</div>
                <div class="panel-body">
                    <input type="hidden" id="hdnIdPerfil" value="0" />
                    <div class="row">
                        <div class="form-group col-lg-6">
                            <label for="txtNombrePerfil">Nombre</label>
                            <input type="text" class="form-control" id="txtNombrePerfil" maxlength="100" />
                        </div>
                        <div class="form-group col-lg-3">
                            <label for="cmbEstado">Estado</label>
                            <select id="cmbEstado" class="form-control">
                                <option value="1">Activo</option>
                                <option value="0">Inactivo</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="panel-footer" style="text-align: center">
                    <button id="btnGuardar" onclick="GuardarPerfil()" type="button" class="btn btn-primary">Guardar</button>
                    <button id="btnCancelar" onclick="CancelarPerfil()" type="button" class="btn btn-default">Cancelar</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
