<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="TableroCostos.aspx.cs" Inherits="ReporteTareas.Formulario.TableroCostos" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .tc-tarjeta { border: 1px solid #ddd; border-radius: 4px; padding: 10px; text-align: center; margin-bottom: 10px; border-top-width: 4px; }
        .tc-tarjeta .tc-titulo { font-weight: bold; }
        .tc-tarjeta .tc-horas { font-size: 20px; font-weight: bold; }
        .tc-normal { border-top-color: #2F6DB5; }
        .tc-extra { border-top-color: #E07B24; }
        .tc-traslado { border-top-color: #1F9E89; }
        .tc-total { border-top-color: #555; }
        .tc-subtotal td { font-weight: bold; background: #f5f5f5; }
        .tc-grafico { position: relative; height: 300px; }
        .tc-grafico-alto { position: relative; height: 420px; }
        .tc-tabla { font-size: 12px; }
        .tc-tabla td.text-right, .tc-tabla th { white-space: nowrap; }
        .tc-aviso { display: none; }
    </style>
    <script src="../js/chart.umd.js" type="text/javascript"></script>
    <script src="../js/tableroCostos.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Tablero de Costos – Tareas Aranda</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">Filtros</div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-2"><label>Desde</label><input type="date" id="txtDesde" class="form-control" /></div>
                        <div class="form-group col-lg-2"><label>Hasta</label><input type="date" id="txtHasta" class="form-control" /></div>
                        <div class="form-group col-lg-2"><label>Mes</label><select id="cboMes" class="form-control"><option value="Todos">Todos</option></select></div>
                        <div class="form-group col-lg-3"><label>Responsable</label><select id="cboResponsable" class="form-control"><option value="Todos">Todos</option></select></div>
                        <div class="form-group col-lg-3"><label>Empresa</label><select id="cboEmpresa" class="form-control"><option value="Todos">Todos</option></select></div>
                    </div>
                    <div class="row">
                        <div class="form-group col-lg-4"><label>Contrato</label><select id="cboContrato" class="form-control"><option value="Todos">Todos</option></select></div>
                        <div class="form-group col-lg-2"><label>Recargo <small>(solo Extras y Traslado)</small></label>
                            <select id="cboRecargo" class="form-control">
                                <option value="Todos">Todos</option>
                                <option value="Sin recargo">Sin recargo</option>
                                <option value="50%">50%</option>
                                <option value="100%">100%</option>
                            </select>
                        </div>
                        <div class="form-group col-lg-2"><label>Estado HE <small>(solo Extras y Traslado)</small></label>
                            <select id="cboEstadoHE" class="form-control">
                                <option value="Todos">Todos</option>
                                <option value="Aprobado">Aprobado</option>
                                <option value="Solicitado">Solicitado</option>
                                <option value="Rechazado">Rechazado</option>
                                <option value="Sin solicitud">Sin solicitud</option>
                                <option value="No aplica">No aplica</option>
                            </select>
                        </div>
                        <div class="form-group col-lg-4" style="padding-top: 25px">
                            <button type="button" class="btn btn-primary" id="btnConsultar">Consultar</button>
                            <button type="button" class="btn btn-success" id="btnExcel"><i class="fa fa-file-excel-o"></i> Exportar a Excel</button>
                        </div>
                    </div>
                    <div id="tcError" class="alert alert-danger tc-aviso"></div>
                    <div id="tcRangoAviso" class="alert alert-info tc-aviso"></div>
                    <div id="tcAviso" class="alert alert-warning tc-aviso"></div>
                    <div id="tcVacio" class="alert alert-info tc-aviso"></div>
                    <div id="tcParametros" class="alert alert-warning tc-aviso"></div>
                    <div id="tcSinSalario" class="alert alert-warning tc-aviso"></div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div class="row">
                <div class="col-lg-3"><div class="tc-tarjeta tc-normal"><div class="tc-titulo">Horas Normales</div><div class="tc-horas" id="indNormalHoras">-</div><div id="indNormalValor">-</div><div id="indNormalAct">-</div></div></div>
                <div class="col-lg-3"><div class="tc-tarjeta tc-extra"><div class="tc-titulo">Horas Extras (50% + 100%)</div><div class="tc-horas" id="indExtraHoras">-</div><div id="indExtraValor">-</div><div id="indExtraAct">-</div></div></div>
                <div class="col-lg-3"><div class="tc-tarjeta tc-traslado"><div class="tc-titulo">Traslado (sin recargo + 50% + 100%)</div><div class="tc-horas" id="indTrasladoHoras">-</div><div id="indTrasladoValor">-</div><div id="indTrasladoAct">-</div></div></div>
                <div class="col-lg-3"><div class="tc-tarjeta tc-total"><div class="tc-titulo">Total</div><div class="tc-horas" id="indTotalHoras">-</div><div id="indTotalValor">-</div><div id="indTotalAct">-</div></div></div>
            </div>

            <div class="row">
                <div class="col-lg-6">
                    <div class="panel panel-default"><div class="panel-heading">Resumen por tipo y recargo</div>
                        <div class="panel-body" style="overflow-x: auto"><table id="tblResumen" class="table table-condensed table-bordered tc-tabla"></table></div></div>
                </div>
                <div class="col-lg-6">
                    <div class="panel panel-default"><div class="panel-heading">Horas extra por estado y recargo</div>
                        <div class="panel-body" style="overflow-x: auto"><table id="tblExtras" class="table table-condensed table-bordered tc-tabla"></table></div></div>
                    <div class="panel panel-default"><div class="panel-heading">Traslado por estado y recargo</div>
                        <div class="panel-body" style="overflow-x: auto"><table id="tblTraslados" class="table table-condensed table-bordered tc-tabla"></table></div></div>
                </div>
            </div>

            <div class="row">
                <div class="col-lg-6"><div class="panel panel-default"><div class="panel-heading">Distribución de horas por tipo y recargo</div><div class="panel-body"><div class="tc-grafico"><canvas id="grfDonaHoras"></canvas></div></div></div></div>
                <div class="col-lg-6"><div class="panel panel-default"><div class="panel-heading">Distribución del valor ($) por tipo y recargo</div><div class="panel-body"><div class="tc-grafico"><canvas id="grfDonaValor"></canvas></div></div></div></div>
            </div>

            <div class="panel panel-default"><div class="panel-heading">Horas y valor ($) por año, tipo y recargo</div>
                <div class="panel-body" style="overflow-x: auto"><table id="tblAnio" class="table table-condensed table-bordered tc-tabla"></table></div></div>

            <div class="panel panel-default"><div class="panel-heading">Horas y valor ($) por mes, tipo y recargo <small>(no aplica el filtro de Mes)</small></div>
                <div class="panel-body" style="overflow-x: auto"><table id="tblMes" class="table table-condensed table-bordered tc-tabla"></table></div></div>

            <div class="row">
                <div class="col-lg-6"><div class="panel panel-default"><div class="panel-heading">Horas por mes, tipo y recargo</div><div class="panel-body"><div class="tc-grafico"><canvas id="grfMesHoras"></canvas></div></div></div></div>
                <div class="col-lg-6"><div class="panel panel-default"><div class="panel-heading">Valor ($) por mes, tipo y recargo</div><div class="panel-body"><div class="tc-grafico"><canvas id="grfMesValor"></canvas></div></div></div></div>
            </div>

            <h4>Top 15 responsables por tipo de hora <small>(por valor $; no aplica el filtro de Responsable)</small></h4>
            <div class="row">
                <div class="col-lg-4"><div class="panel panel-default"><div class="panel-heading">Normal</div><div class="panel-body">
                    <div class="tc-grafico-alto"><canvas id="grfTopNormal"></canvas></div>
                    <table id="tblTopNormal" class="table table-condensed table-bordered tc-tabla"></table></div></div></div>
                <div class="col-lg-4"><div class="panel panel-default"><div class="panel-heading">Extra</div><div class="panel-body">
                    <div class="tc-grafico-alto"><canvas id="grfTopExtra"></canvas></div>
                    <table id="tblTopExtra" class="table table-condensed table-bordered tc-tabla"></table></div></div></div>
                <div class="col-lg-4"><div class="panel panel-default"><div class="panel-heading">Traslado</div><div class="panel-body">
                    <div class="tc-grafico-alto"><canvas id="grfTopTraslado"></canvas></div>
                    <table id="tblTopTraslado" class="table table-condensed table-bordered tc-tabla"></table></div></div></div>
            </div>

            <div class="panel panel-default"><div class="panel-heading">Detalle de actividades</div>
                <div class="panel-body" style="overflow-x: auto">
                    <table id="tblDetalle" class="table table-condensed table-striped table-bordered tc-tabla">
                        <thead><tr><th>Inicio</th><th>Responsable</th><th>Empresa</th><th>Contrato</th><th>Ticket</th><th>Tarea</th><th>Estado</th><th>Tipo</th><th>Recargo</th><th>Estado HE</th><th>Horas</th><th>Valor $</th></tr></thead>
                        <tbody></tbody>
                    </table>
                    <button type="button" class="btn btn-default btn-sm" id="btnAnterior">Anterior</button>
                    <span id="lblPagina" style="margin: 0 10px"></span>
                    <button type="button" class="btn btn-default btn-sm" id="btnSiguiente">Siguiente</button>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading"><a data-toggle="collapse" href="#tcCriterios">Criterios del tablero</a></div>
                <div id="tcCriterios" class="panel-collapse collapse"><div class="panel-body">
                    <ul>
                        <li><b>Traslado:</b> el estado empieza con «Movilización», o <code>IdDet_EstadoIni</code> es 14, 28 o 62. Tiene prioridad sobre Extra.</li>
                        <li><b>Extra:</b> <code>Det_Horas_Extras_Tipo</code> = 1 (50%) o 2 (100%). Estado HE: 1 Solicitado, 2 Aprobado, 3 Rechazado; otro valor, Sin solicitud.</li>
                        <li><b>Normal:</b> el resto de actividades, en cualquier estado (también las pausas).</li>
                        <li><b>Recargo y Estado HE</b> filtran solo Extras y Traslado. Los traslados sin recargo tienen estado «No aplica».</li>
                        <li><b>Valor hora</b> = salario mensual ÷ (jornada en horas por día × días por mes). <b>Valor $</b> = horas × valor hora × factor.</li>
                        <li><b>Factor:</b> 1,5 (50%) y 2 (100%) para extras y traslados con recargo; 1 para el resto o si el colaborador no aplica horas extra.</li>
                        <li><b>Salario:</b> el vigente más reciente en Horas Extras, aplicado a todo el período. Quien no tiene salario se valora en $0.</li>
                    </ul>
                </div></div>
            </div>
        </div>
    </div>
</asp:Content>
