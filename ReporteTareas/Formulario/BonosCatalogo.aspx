<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="BonosCatalogo.aspx.cs" Inherits="ReporteTareas.Formulario.BonosCatalogo" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .bc-tabla td { vertical-align: middle !important; }
        .bc-num { text-align: right; }
    </style>
    <script src="../js/bonosCatalogo.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Catálogo de bonos</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="bcError" class="alert alert-danger" style="display: none"></div>
            <div id="bcAviso" class="alert" style="display: none"></div>

            <div class="panel panel-default">
                <div class="panel-heading">Tipos de bono</div>
                <div class="panel-body">
                    <table class="table table-condensed table-striped bc-tabla">
                        <thead>
                            <tr>
                                <th>Nombre</th><th>Familia</th><th class="bc-num">Sugerido</th><th>Por semana</th>
                                <th>Activo</th><th>Concepto DOS</th><th>Concepto AGILITY</th><th></th>
                            </tr>
                        </thead>
                        <tbody id="bcTipos"></tbody>
                    </table>
                    <button type="button" id="bcNuevo" class="btn btn-primary">+ Nuevo tipo</button>
                </div>
            </div>

            <div class="panel panel-default" id="bcPanelForm" style="display: none">
                <div class="panel-heading" id="bcTituloForm">Tipo de bono</div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-md-4 form-group">
                            <label for="bcNombre">Nombre</label>
                            <input type="text" id="bcNombre" class="form-control" maxlength="100" />
                        </div>
                        <div class="col-md-3 form-group">
                            <label for="bcFamilia">Familia</label>
                            <select id="bcFamilia" class="form-control">
                                <option value="Standby">Standby</option>
                                <option value="Contrato">Contrato</option>
                                <option value="Otro">Otro</option>
                            </select>
                        </div>
                        <div class="col-md-3 form-group">
                            <label for="bcSugerido">Valor sugerido (vacío = ninguno)</label>
                            <input type="text" id="bcSugerido" class="form-control" placeholder="0.00" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-3 form-group">
                            <label for="bcConceptoDOS">Concepto SAP DOS</label>
                            <input type="text" id="bcConceptoDOS" class="form-control" maxlength="10" style="text-transform: uppercase" />
                        </div>
                        <div class="col-md-3 form-group">
                            <label for="bcConceptoAGILITY">Concepto SAP AGILITY</label>
                            <input type="text" id="bcConceptoAGILITY" class="form-control" maxlength="10" style="text-transform: uppercase" />
                        </div>
                        <div class="col-md-3 form-group">
                            <div class="checkbox"><label><input type="checkbox" id="bcPorSemana" /> Se paga por semana</label></div>
                            <div class="checkbox"><label><input type="checkbox" id="bcActivo" checked="checked" /> Activo</label></div>
                        </div>
                    </div>
                    <button type="button" id="bcGuardar" class="btn btn-primary">Guardar</button>
                    <button type="button" id="bcCancelar" class="btn btn-default">Cancelar</button>
                    <p class="text-muted" style="margin-top: 10px">Un tipo con bonos no se borra: se desactiva.</p>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
