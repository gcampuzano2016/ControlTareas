<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CalificacionForeCast.aspx.cs" Inherits="ReporteTareas.Formulario.CalificacionForeCast" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .cf-tarjeta { border: 1px solid #ddd; border-radius: 4px; padding: 10px; text-align: center; margin-bottom: 10px; }
        .cf-tarjeta .cf-valor { font-size: 22px; font-weight: bold; }
        .cf-estado { padding: 2px 8px; border-radius: 10px; font-weight: bold; white-space: nowrap; }
        .cf-Caliente { background: #f8cbad; }
        .cf-Tibia { background: #ffe699; }
        .cf-Fria { background: #bdd7ee; }
        .cf-sin { font-size: 11px; color: #a94442; display: block; }
        .cf-detalle td { vertical-align: top; }
    </style>
    <script src="../js/CalificacionForeCast.js?v=2" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Calificación de oportunidades</h3>
                    </div>
                </div>
            </div>
        </div>

        <div style="display: none">
            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">Filtros</div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-2"><label>Gerente de cuenta</label><select id="cboGerente" class="form-control"></select></div>
                        <div class="form-group col-lg-2"><label>Sucursal</label><select id="cboSucursal" class="form-control"></select></div>
                        <div class="form-group col-lg-2"><label>Etapa</label><select id="cboEtapa" class="form-control"></select></div>
                        <div class="form-group col-lg-2"><label>Estado</label>
                            <select id="cboEstado" class="form-control">
                                <option value="">Todos</option>
                                <option value="Caliente">Caliente</option>
                                <option value="Tibia">Tibia</option>
                                <option value="Fría">Fría</option>
                                <option value="Sin calificar">Sin calificar</option>
                            </select>
                        </div>
                        <div class="form-group col-lg-2"><label>Marca</label><select id="cboMarca" class="form-control"></select></div>
                        <div class="form-group col-lg-2" style="padding-top: 25px">
                            <button type="button" class="btn btn-primary" onclick="Cargar()">Buscar</button>
                            <button type="button" class="btn btn-success" onclick="Descargar()"><i class="fa fa-file-excel-o"></i> Excel</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div class="row">
                <div class="col-lg-2"><div class="cf-tarjeta"><div>Abiertas</div><div class="cf-valor" id="indTotal">-</div></div></div>
                <div class="col-lg-2"><div class="cf-tarjeta"><div>Monto total</div><div class="cf-valor" id="indMonto">-</div></div></div>
                <div class="col-lg-2"><div class="cf-tarjeta"><div>Monto ponderado</div><div class="cf-valor" id="indPonderado">-</div></div></div>
                <div class="col-lg-4"><div class="cf-tarjeta"><div>Caliente · Tibia · Fría</div><div class="cf-valor" id="indEstados">-</div></div></div>
                <div class="col-lg-2"><div class="cf-tarjeta"><div>Sin calificar</div><div class="cf-valor" id="indSinCalificar">-</div></div></div>
            </div>
            <div class="row">
                <div class="col-lg-6">
                    <div class="panel panel-success"><div class="panel-heading">Top 5 de la semana</div><div class="panel-body"><ol id="lstTop5"></ol></div></div>
                </div>
                <div class="col-lg-6">
                    <div class="panel panel-danger"><div class="panel-heading">En riesgo</div><div class="panel-body"><ul id="lstRiesgo"></ul></div></div>
                </div>
            </div>
            <div class="panel panel-default">
                <div class="panel-heading">Oportunidades (clic en una fila para ver el desglose)</div>
                <div class="panel-body" style="overflow-x: auto">
                    <table id="tblCalificacion" class="table table-striped table-bordered" style="width: 100%">
                        <thead>
                            <tr><th>Cliente</th><th>Oportunidad</th><th>Etapa</th><th>Monto</th><th>Puntaje</th><th>Estado</th><th>Alertas</th><th></th></tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="modalCalificar" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">Calificar: <span id="lblOportunidad"></span></h4>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="hidIdForeCast" />
                    <div class="form-group"><label>¿En qué estado está el presupuesto del cliente?</label>
                        <select id="cboPresupuesto" class="form-control">
                            <option value="">Seleccione…</option>
                            <option value="20">Aprobado o asignado (partida, PAC u OC)</option>
                            <option value="12">En trámite (en POA/PAC, pendiente de aprobación)</option>
                            <option value="5">Solo lo estimamos nosotros</option>
                            <option value="0">No tiene / no sabemos</option>
                        </select>
                    </div>
                    <div class="form-group"><label>¿Con quién estamos hablando?</label>
                        <select id="cboAutoridad" class="form-control">
                            <option value="">Seleccione…</option>
                            <option value="20">Decisor final (gerente, director, rectorado con firma)</option>
                            <option value="12">Influenciador con acceso al decisor</option>
                            <option value="5">Solo usuario técnico</option>
                            <option value="0">Sin contacto</option>
                        </select>
                    </div>
                    <div class="form-group"><label>¿Qué tan clara es la necesidad?</label>
                        <select id="cboNecesidad" class="form-control">
                            <option value="">Seleccione…</option>
                            <option value="20">Crítica y con fecha (vence soporte, EOSL, proceso o TDR en curso)</option>
                            <option value="14">Clara y reconocida por el cliente</option>
                            <option value="6">Interés exploratorio</option>
                            <option value="0">No definida</option>
                        </select>
                    </div>
                    <div class="form-group"><label>Nombre y cargo del decisor</label>
                        <input type="text" id="txtDecisor" class="form-control" maxlength="150" /></div>
                    <div class="checkbox"><label><input type="checkbox" id="chkPropuesta" /> Propuesta enviada</label></div>
                    <div class="form-group"><label>Próxima acción comprometida</label>
                        <input type="text" id="txtAccion" class="form-control" maxlength="300" /></div>
                    <div class="form-group"><label>Fecha de la acción</label>
                        <input type="date" id="txtFechaAccion" class="form-control" /></div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                    <button type="button" class="btn btn-primary" id="btnGuardar" onclick="Guardar()">Guardar</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
