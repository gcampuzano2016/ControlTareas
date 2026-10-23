<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="BonosProponer.aspx.cs" Inherits="ReporteTareas.Formulario.BonosProponer" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .bp-tabla td { vertical-align: middle !important; }
        .bp-num { text-align: right; }
        .bp-semanas { max-height: 220px; overflow-y: auto; border: 1px solid #ddd; border-radius: 4px; padding: 6px 10px; }
        .bp-semanas label { display: block; font-weight: normal; margin: 2px 0; }
        .bp-semanas label.bp-tomada { text-decoration: line-through; color: #999; }
        .bp-rechazo { display: block; color: #a94442; font-size: 12px; margin-top: 3px; }
        .bp-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,.45); z-index: 2000; display: none; }
        .bp-dialogo { background: #fff; max-width: 460px; margin: 15vh auto 0; padding: 20px; border-radius: 4px; }
    </style>
    <script src="../js/bonosProponer.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Proponer bonos</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="bpError" class="alert alert-danger" style="display: none"></div>
            <div id="bpAviso" class="alert" style="display: none"></div>
            <div id="bpSinGente" class="alert alert-info" style="display: none"></div>

            <div id="bpContenido" style="display: none">
                <div id="bpBarra" class="alert alert-info"></div>

                <div class="panel panel-default">
                    <div class="panel-heading">Mis bonos</div>
                    <div class="panel-body">
                        <table class="table table-condensed table-striped bp-tabla">
                            <thead>
                                <tr>
                                    <th>Persona</th><th>Tipo de bono</th><th>Semanas</th><th class="bp-num">Valor</th><th>Estado</th><th></th>
                                </tr>
                            </thead>
                            <tbody id="bpBonos"></tbody>
                        </table>
                        <button type="button" id="bpNuevo" class="btn btn-default">+ Agregar bono</button>
                        <button type="button" id="bpEnviar" class="btn btn-primary">Enviar a Talento Humano</button>
                    </div>
                </div>

                <div class="panel panel-default" id="bpPanelForm" style="display: none">
                    <div class="panel-heading" id="bpTituloForm">Bono</div>
                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-4 form-group">
                                <label for="bpPersona">Persona</label>
                                <select id="bpPersona" class="form-control"></select>
                                <span id="bpAvisoId" class="text-warning" style="display: none"></span>
                            </div>
                            <div class="col-md-4 form-group">
                                <label for="bpTipo">Tipo de bono</label>
                                <select id="bpTipo" class="form-control"></select>
                            </div>
                            <div class="col-md-4 form-group">
                                <label for="bpLugar">Lugar</label>
                                <input type="text" id="bpLugar" class="form-control" maxlength="100" />
                            </div>
                        </div>
                        <div class="row" id="bpFilaSemanas" style="display: none">
                            <div class="col-md-8 form-group">
                                <label>Semanas (lunes a domingo)</label>
                                <div id="bpSemanas" class="bp-semanas"></div>
                                <span class="help-block">Tachada = ya tomada por otro bono de esta persona y tipo.</span>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-3 form-group">
                                <label for="bpValor">Valor</label>
                                <input type="text" id="bpValor" class="form-control" placeholder="0.00" />
                                <span class="help-block" id="bpSugerido"></span>
                            </div>
                            <div class="col-md-9 form-group">
                                <label for="bpMotivo">Motivo del ajuste <span id="bpMotivoReq" class="text-danger" style="display: none">(obligatorio: el valor difiere del sugerido)</span></label>
                                <input type="text" id="bpMotivo" class="form-control" maxlength="300" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12 form-group">
                                <label for="bpObservacion">Observación</label>
                                <input type="text" id="bpObservacion" class="form-control" maxlength="300" />
                            </div>
                        </div>
                        <button type="button" id="bpGuardar" class="btn btn-primary">Guardar</button>
                        <button type="button" id="bpCancelar" class="btn btn-default">Cancelar</button>
                    </div>
                </div>
            </div>
        </div>

        <div id="bpOverlay" class="bp-overlay">
            <div class="bp-dialogo">
                <p id="bpDialogoTexto"></p>
                <div class="text-right">
                    <button type="button" id="bpDialogoNo" class="btn btn-default">Cancelar</button>
                    <button type="button" id="bpDialogoSi" class="btn btn-primary">Guardar de todos modos</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
