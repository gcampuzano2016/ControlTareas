<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="VacacionesSaldos.aspx.cs" Inherits="ReporteTareas.Formulario.VacacionesSaldos" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .vs-tarjeta { border: 1px solid #ddd; border-radius: 4px; padding: 10px; text-align: center; margin-bottom: 10px; border-top-width: 4px; min-height: 100px; }
        .vs-tarjeta .vs-titulo { font-weight: bold; }
        .vs-tarjeta .vs-valor { font-size: 20px; font-weight: bold; }
        .vs-total { border-top-color: #2F6DB5; }
        .vs-vencer { border-top-color: #E07B24; }
        .vs-anticipo { border-top-color: #C0392B; }
        .vs-acumula { border-top-color: #8E44AD; }
        .vs-vencido { border-top-color: #555; }
        .vs-tabla { font-size: 12px; }
        .vs-tabla td, .vs-tabla th { white-space: nowrap; padding: 5px 8px !important; }
        .vs-tabla td.vs-texto { white-space: normal; }
        .vs-num { text-align: right; }
        .vs-tabla th.vs-orden { cursor: pointer; }
        .vs-tabla th.vs-orden:after { content: ' \2195'; color: #bbb; }
        .vs-tabla th.vs-asc:after { content: ' \25B2'; color: #333; }
        .vs-tabla th.vs-desc:after { content: ' \25BC'; color: #333; }
        #tblSaldos tbody tr { cursor: pointer; }
        #tblSaldos tbody tr.vs-elegida td { background: #fcf8e3; }
        .vs-neg { color: #C0392B; font-weight: bold; }
        .vs-etiqueta { display: inline-block; padding: 2px 6px; border-radius: 3px; font-size: 11px; font-weight: bold; color: #fff; }
        .vs-e-ANTICIPO { background: #C0392B; }
        .vs-e-POR-VENCER { background: #E07B24; }
        .vs-e-ACUMULANDO { background: #8E44AD; }
        .vs-e-AL-DIA { background: #27AE60; }
        .vs-e-otro { background: #7f8c8d; }
        .vs-tenue td { color: #999 !important; }
        .vs-oculto { display: none; }
        .vs-dato { margin-bottom: 6px; }
        .vs-dato b { display: inline-block; min-width: 150px; }
    </style>
    <script src="../js/vacacionesSaldos.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Saldos de vacaciones</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">Filtros</div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-2"><label>Fecha de cálculo</label><input type="date" id="txtFecha" class="form-control" /></div>
                        <div class="form-group col-lg-2"><label>Colaboradores</label>
                            <select id="cboColaborador" class="form-control">
                                <option value="ACTIVO">Activos</option>
                                <option value="INACTIVO">Inactivos</option>
                                <option value="">Todos</option>
                            </select>
                        </div>
                        <div class="form-group col-lg-2"><label>Estado del saldo</label>
                            <select id="cboEstado" class="form-control">
                                <option value="">Todos</option>
                                <option value="ANTICIPO">Anticipo</option>
                                <option value="POR VENCER">Por vencer</option>
                                <option value="ACUMULANDO">Acumulando</option>
                                <option value="AL DIA">Al día</option>
                            </select>
                        </div>
                        <div class="form-group col-lg-2"><label>Área</label><select id="cboArea" class="form-control"><option value="">Todas</option></select></div>
                        <div class="form-group col-lg-2"><label>Buscar</label><input type="text" id="txtBuscar" class="form-control" placeholder="Nombre o cédula" /></div>
                        <div class="form-group col-lg-2" style="padding-top: 25px">
                            <button type="button" class="btn btn-primary" id="btnConsultar">Consultar</button>
                            <button type="button" class="btn btn-success" id="btnDescargar" title="Descarga lo que está en pantalla"><i class="fa fa-file-excel-o"></i> CSV</button>
                        </div>
                    </div>
                    <div id="vsError" class="alert alert-danger vs-oculto"></div>
                    <div id="vsAvisos" class="alert alert-warning vs-oculto"></div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <p class="text-muted">Las tarjetas suman solo los colaboradores <b>activos</b>, como el DASHBOARD del Excel de GTH. Saldos calculados al <b id="lblFecha">-</b>.</p>
            <div class="row">
                <div class="col-lg-2 col-sm-4"><div class="vs-tarjeta vs-total"><div class="vs-titulo">Saldo total</div><div class="vs-valor" id="indSaldo">-</div><div id="indActivos">-</div></div></div>
                <div class="col-lg-3 col-sm-4"><div class="vs-tarjeta vs-vencer"><div class="vs-titulo">Por vencer</div><div class="vs-valor" id="indPorVencer">-</div><div id="indPorVencerP">-</div></div></div>
                <div class="col-lg-2 col-sm-4"><div class="vs-tarjeta vs-anticipo"><div class="vs-titulo">Anticipos</div><div class="vs-valor" id="indAnticipo">-</div><div id="indAnticipoP">-</div></div></div>
                <div class="col-lg-2 col-sm-6"><div class="vs-tarjeta vs-acumula"><div class="vs-titulo">Acumulando</div><div class="vs-valor" id="indAcumula">-</div><div>más de un año de días</div></div></div>
                <div class="col-lg-3 col-sm-6"><div class="vs-tarjeta vs-vencido"><div class="vs-titulo">Días vencidos</div><div class="vs-valor" id="indVencidos">-</div><div>perdidos sin gozar</div></div></div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading">Colaboradores <small id="lblConteo"></small> <small class="text-muted">— clic en una fila para ver sus periodos y solicitudes</small></div>
                <div class="panel-body" style="overflow-x: auto">
                    <table id="tblSaldos" class="table table-condensed table-bordered table-hover vs-tabla">
                        <thead>
                            <tr>
                                <th class="vs-orden" data-campo="Nombre">Nombre</th>
                                <th class="vs-orden" data-campo="Cedula">Cédula</th>
                                <th class="vs-orden" data-campo="Area">Área</th>
                                <th class="vs-orden" data-campo="ingreso">Ingreso</th>
                                <th class="vs-orden vs-num" data-campo="cumplidos">Años</th>
                                <th class="vs-orden vs-num" data-campo="diasPorAnio">Días/año</th>
                                <th class="vs-orden vs-num" data-campo="disponible">Disponible</th>
                                <th class="vs-orden vs-num" data-campo="proporcional">Proporcional</th>
                                <th class="vs-orden vs-num" data-campo="usadoEnCurso">Usado en curso</th>
                                <th class="vs-orden vs-num" data-campo="saldo">Saldo</th>
                                <th class="vs-orden vs-num" data-campo="porVencer">Por vencer</th>
                                <th class="vs-orden" data-campo="proximoVencimiento">Vence</th>
                                <th class="vs-orden" data-campo="estadoSaldo">Estado</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                        <tfoot></tfoot>
                    </table>
                </div>
            </div>

            <div id="vsFicha" class="panel panel-primary vs-oculto">
                <div class="panel-heading">
                    <button type="button" class="close" id="btnCerrarFicha" aria-label="Cerrar">&times;</button>
                    <span id="fichaTitulo"></span>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-lg-6" id="fichaPersona"></div>
                        <div class="col-lg-6" id="fichaSaldo"></div>
                    </div>
                    <h4>Periodos</h4>
                    <div style="overflow-x: auto">
                        <table id="tblPeriodos" class="table table-condensed table-bordered vs-tabla">
                            <thead><tr><th>N.º</th><th>Periodo</th><th>Desde</th><th>Hasta</th><th class="vs-num">Ganados</th><th class="vs-num">Tomados</th><th class="vs-num">Vencidos</th><th class="vs-num">Saldo</th><th>Vence</th><th>Estado</th></tr></thead>
                            <tbody></tbody>
                        </table>
                    </div>
                    <h4>Solicitudes <small>(las tenues no descuentan)</small></h4>
                    <div style="overflow-x: auto">
                        <table id="tblMovimientos" class="table table-condensed table-bordered vs-tabla">
                            <thead><tr><th>Tipo</th><th>Estado</th><th>Desde</th><th>Hasta</th><th class="vs-num">Pedido</th><th class="vs-num">Año serv.</th><th class="vs-num">Días</th><th class="vs-num">Fin de semana</th><th class="vs-num">Descuenta</th><th>Situación</th><th>Observación</th></tr></thead>
                            <tbody></tbody>
                        </table>
                    </div>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading"><a data-toggle="collapse" href="#vsCriterios">Cómo se calcula</a></div>
                <div id="vsCriterios" class="panel-collapse collapse"><div class="panel-body">
                    <ul>
                        <li><b>Días por año:</b> 15 por año cumplido, y uno más por cada año después del quinto, hasta 15 adicionales.</li>
                        <li><b>Periodos:</b> van de aniversario a aniversario y se consumen del más antiguo al más nuevo. El periodo <i>n</i> vence en el aniversario <i>n</i>+3; 90 días antes sale <b>Por vencer</b>.</li>
                        <li><b>Saldo</b> = disponible de los periodos vigentes + proporcional del periodo en curso − lo usado en el periodo en curso. Si queda negativo es <b>Anticipo</b>.</li>
                        <li><b>Fin de semana:</b> una solicitud que termina en viernes carga 2 días (salvo Beneficio DOS), y cada 5 días sueltos en el mismo año de servicio cargan 2 más; máximo 2 fines de semana por periodo.</li>
                        <li><b>Permisos por horas</b> = (horas + minutos/60) ÷ 8. Lo dado de baja no descuenta; lo «por aprobar» y «revisar» sí.</li>
                        <li><b>Acumulando:</b> tiene disponible más de lo que gana en un año.</li>
                        <li>Los parámetros están en la tabla <code>Vac_Parametro</code>; el cálculo, en <code>Sp_RTA_VacCalcular</code>.</li>
                    </ul>
                </div></div>
            </div>
        </div>
    </div>
</asp:Content>
