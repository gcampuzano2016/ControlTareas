<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="BonosRevisar.aspx.cs" Inherits="ReporteTareas.Formulario.BonosRevisar" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .br-tabla td { vertical-align: middle !important; }
        .br-num { text-align: right; }
        .br-nota { display: block; font-size: 12px; margin-top: 3px; color: #777; }
        .br-rechazo { display: block; color: #a94442; font-size: 12px; margin-top: 3px; }
        .br-resumen .br-cifra { font-size: 22px; font-weight: bold; display: block; }
        .br-resumen .col-xs-6, .br-resumen .col-md-3 { text-align: center; margin-bottom: 8px; }
        .br-semanas { max-height: 220px; overflow-y: auto; border: 1px solid #ddd; border-radius: 4px; padding: 6px 10px; }
        .br-semanas label { display: block; font-weight: normal; margin: 2px 0; }
        .br-semanas label.br-tomada { text-decoration: line-through; color: #999; }
        .br-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,.45); z-index: 2000; display: none; overflow-y: auto; }
        .br-dialogo { background: #fff; max-width: 560px; margin: 10vh auto 30px; padding: 20px; border-radius: 4px; }
        .br-lista { max-height: 260px; overflow-y: auto; margin: 6px 0 10px; }
    </style>
    <script src="../js/bonosRevisar.js?v=3" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Revisar y aprobar bonos</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="brError" class="alert alert-danger" style="display: none"></div>
            <div id="brAviso" class="alert" style="display: none"></div>

            <div id="brContenido" style="display: none">
                <div id="brBarra" class="alert alert-info"></div>

                <div class="panel panel-default br-resumen" id="brPanelResumen">
                    <div class="panel-body">
                        <div class="row">
                            <div class="col-xs-6 col-md-3"><span class="br-cifra" id="brResEnviados">0</span>Enviados</div>
                            <div class="col-xs-6 col-md-3"><span class="br-cifra" id="brResAprobados">0</span>Aprobados</div>
                            <div class="col-xs-6 col-md-3"><span class="br-cifra" id="brResRechazados">0</span>Rechazados</div>
                            <div class="col-xs-6 col-md-3"><span class="br-cifra" id="brResTotal">$0.00</span>Total aprobado</div>
                        </div>
                    </div>
                </div>

                <div class="panel panel-default">
                    <div class="panel-heading">Bonos del período</div>
                    <div class="panel-body">
                        <div class="row" style="margin-bottom: 10px">
                            <div class="col-md-3 form-group">
                                <label for="brFiltroEstado">Estado</label>
                                <select id="brFiltroEstado" class="form-control">
                                    <option value="">Todos (sin borradores)</option>
                                    <option value="Enviado">Enviado</option>
                                    <option value="Aprobado">Aprobado</option>
                                    <option value="Rechazado">Rechazado</option>
                                    <option value="Borrador">Borrador (sin enviar)</option>
                                </select>
                            </div>
                            <div class="col-md-4 form-group">
                                <label for="brFiltroJefe">Jefe</label>
                                <select id="brFiltroJefe" class="form-control"></select>
                            </div>
                        </div>
                        <div class="table-responsive">
                            <table class="table table-condensed table-striped br-tabla">
                                <thead>
                                    <tr>
                                        <th style="width: 28px"><input type="checkbox" id="brSelTodos" title="Marcar los enviados visibles" /></th>
                                        <th>Jefe</th><th>Persona</th><th>Tipo de bono</th><th>Semanas</th>
                                        <th class="br-num">Sugerido</th><th class="br-num">Valor</th><th>Estado</th><th></th>
                                    </tr>
                                </thead>
                                <tbody id="brBonos"></tbody>
                            </table>
                        </div>
                        <button type="button" id="brAprobarSel" class="btn btn-success">Aprobar seleccionados</button>
                        <button type="button" id="brDirecto" class="btn btn-default">Registrar bono directo</button>
                        <button type="button" id="brAbrirMes" class="btn btn-default" style="display: none">Abrir mes</button>
                        <button type="button" id="brCerrarMes" class="btn btn-primary">Cerrar el mes…</button>
                    </div>
                </div>

                <div class="panel panel-default" id="brPanelForm" style="display: none">
                    <div class="panel-heading">Registrar bono directo (queda Aprobado)</div>
                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-4 form-group">
                                <label for="brPersona">Persona</label>
                                <select id="brPersona" class="form-control"></select>
                            </div>
                            <div class="col-md-4 form-group">
                                <label for="brTipo">Tipo de bono</label>
                                <select id="brTipo" class="form-control"></select>
                            </div>
                            <div class="col-md-4 form-group">
                                <label for="brLugar">Lugar</label>
                                <select id="brLugar" class="form-control">
                                    <option value="">-- Seleccione --</option>
                                    <option value="Guayaquil">GUAYAQUIL</option>
                                    <option value="Quito">QUITO</option>
                                    <option value="Cuenca">CUENCA</option>
                                    <option value="Ambato">AMBATO</option>
                                </select>
                            </div>
                        </div>
                        <div class="row" id="brFilaSemanas" style="display: none">
                            <div class="col-md-8 form-group">
                                <label>Semanas (lunes a domingo)</label>
                                <div id="brSemanas" class="br-semanas"></div>
                                <span class="help-block">Tachada = ya tomada por un bono de esta persona y tipo.</span>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-2 form-group">
                                <label for="brValor">Valor por semana</label>
                                <input type="text" id="brValor" class="form-control" placeholder="0.00" />
                                <span class="help-block" id="brSugerido"></span>
                            </div>
                            <div class="col-md-2 form-group">
                                <label for="brNumSemanas">N.º de semanas</label>
                                <input type="number" id="brNumSemanas" class="form-control" min="1" max="52" step="1" />
                            </div>
                            <div class="col-md-2 form-group">
                                <label for="brTotal">Total</label>
                                <input type="text" id="brTotal" class="form-control" readonly="readonly" tabindex="-1" />
                            </div>
                            <div class="col-md-6 form-group">
                                <label for="brMotivo">Motivo del ajuste <span id="brMotivoReq" class="text-danger" style="display: none">(obligatorio: el valor difiere del sugerido)</span></label>
                                <input type="text" id="brMotivo" class="form-control" maxlength="300" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12 form-group">
                                <label for="brObservacion">Observación</label>
                                <input type="text" id="brObservacion" class="form-control" maxlength="300" />
                            </div>
                        </div>
                        <button type="button" id="brGuardar" class="btn btn-primary">Registrar</button>
                        <button type="button" id="brCancelar" class="btn btn-default">Cancelar</button>
                    </div>
                </div>
            </div>
        </div>

        <%-- Diálogo único: lo llenan Confirmar repetido, Ajustar, Rechazar y Cierre. --%>
        <div id="brOverlay" class="br-overlay">
            <div class="br-dialogo">
                <h4 id="brDialogoTitulo" style="margin-top: 0"></h4>
                <p id="brDialogoTexto"></p>
                <div id="brDialogoCampos">
                    <div class="form-group" id="brGrupoValor">
                        <label for="brDialogoValor">Nuevo valor</label>
                        <input type="text" id="brDialogoValor" class="form-control" placeholder="0.00" />
                    </div>
                    <div class="form-group" id="brGrupoMotivo">
                        <label for="brDialogoMotivo">Motivo <span class="text-danger">(obligatorio)</span></label>
                        <input type="text" id="brDialogoMotivo" class="form-control" maxlength="300" />
                    </div>
                </div>
                <div id="brDialogoLista" class="br-lista"></div>
                <div id="brDialogoError" class="alert alert-danger" style="display: none"></div>
                <div class="text-right">
                    <button type="button" id="brDialogoNo" class="btn btn-default">Cancelar</button>
                    <button type="button" id="brDialogoSi" class="btn btn-primary">Aceptar</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
