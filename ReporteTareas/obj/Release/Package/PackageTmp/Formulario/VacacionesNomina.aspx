<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="VacacionesNomina.aspx.cs" Inherits="ReporteTareas.Formulario.VacacionesNomina" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .vn-tarjeta { border: 1px solid #ddd; border-radius: 4px; padding: 10px; text-align: center; margin-bottom: 10px; border-top-width: 4px; min-height: 100px; }
        .vn-tarjeta .vn-titulo { font-weight: bold; }
        .vn-tarjeta .vn-valor { font-size: 20px; font-weight: bold; }
        .vn-dias { border-top-color: #2F6DB5; }
        .vn-plata { border-top-color: #27AE60; }
        .vn-anticipo { border-top-color: #C0392B; }
        .vn-falta { border-top-color: #E07B24; }
        .vn-tabla { font-size: 12px; }
        .vn-tabla td, .vn-tabla th { white-space: nowrap; padding: 5px 8px !important; }
        .vn-num { text-align: right; }
        .vn-neg { color: #C0392B; font-weight: bold; }
        .vn-tabla th.vn-orden { cursor: pointer; }
        .vn-tabla th.vn-orden:after { content: ' \2195'; color: #bbb; }
        .vn-tabla th.vn-asc:after { content: ' \25B2'; color: #333; }
        .vn-tabla th.vn-desc:after { content: ' \25BC'; color: #333; }
        .vn-falta-sueldo td { background: #fff7ec; }
        .vn-oculto { display: none; }
    </style>
    <script src="../js/vacacionesNomina.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Nómina de vacaciones</h3>
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
                        <div class="form-group col-lg-2"><label>Empresa</label><select id="cboEmpresa" class="form-control"><option value="">Todas</option></select></div>
                        <div class="form-group col-lg-2"><label>Sueldo</label>
                            <select id="cboSueldo" class="form-control">
                                <option value="">Todos</option>
                                <option value="FALTA">Falta sueldo</option>
                                <option value="ANTICIPO">Con anticipo</option>
                            </select>
                        </div>
                        <div class="form-group col-lg-2"><label>Buscar</label><input type="text" id="txtBuscar" class="form-control" placeholder="Nombre o cédula" /></div>
                        <div class="form-group col-lg-2" style="padding-top: 25px">
                            <button type="button" class="btn btn-primary" id="btnConsultar">Consultar</button>
                            <button type="button" class="btn btn-success" id="btnDescargar" title="Descarga lo que está en pantalla"><i class="fa fa-file-excel-o"></i> CSV</button>
                        </div>
                    </div>
                    <div id="vnError" class="alert alert-danger vn-oculto"></div>
                    <div id="vnAvisos" class="alert alert-warning vn-oculto"></div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <p class="text-muted">Las tarjetas y el resumen por empresa suman solo los colaboradores <b>activos</b>, como la hoja NOMINA del Excel de GTH. Calculado al <b id="lblFecha">-</b>; los inactivos, a su fecha de salida.</p>
            <div class="row">
                <div class="col-lg-3 col-sm-6"><div class="vn-tarjeta vn-dias"><div class="vn-titulo">Días a provisionar</div><div class="vn-valor" id="indDias">-</div><div id="indActivos">-</div></div></div>
                <div class="col-lg-3 col-sm-6"><div class="vn-tarjeta vn-plata"><div class="vn-titulo">Provisión ($)</div><div class="vn-valor" id="indValor">-</div><div>días × sueldo ÷ 30</div></div></div>
                <div class="col-lg-3 col-sm-6"><div class="vn-tarjeta vn-anticipo"><div class="vn-titulo">Anticipos ($)</div><div class="vn-valor" id="indAnticipos">-</div><div>ya incluido en la provisión</div></div></div>
                <div class="col-lg-3 col-sm-6"><div class="vn-tarjeta vn-falta"><div class="vn-titulo">Sin sueldo</div><div class="vn-valor" id="indSinSueldo">-</div><div>sus días no se valorizan</div></div></div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading">Por empresa (activos)</div>
                <div class="panel-body" style="overflow-x: auto">
                    <table id="tblEmpresas" class="table table-condensed table-bordered vn-tabla" style="width: auto">
                        <thead><tr><th>Empresa</th><th class="vn-num">Personas</th><th class="vn-num">Días</th><th class="vn-num">Valor ($)</th><th class="vn-num">Sin sueldo</th></tr></thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading">Colaboradores <small id="lblConteo"></small></div>
                <div class="panel-body" style="overflow-x: auto">
                    <table id="tblNomina" class="table table-condensed table-bordered vn-tabla">
                        <thead>
                            <tr>
                                <th class="vn-orden" data-campo="Nombre">Colaborador</th>
                                <th class="vn-orden" data-campo="Cedula">Cédula</th>
                                <th class="vn-orden" data-campo="Empresa">Empresa</th>
                                <th class="vn-orden" data-campo="ingreso">Ingreso</th>
                                <th class="vn-orden" data-campo="fechaSaldo">Calculado al</th>
                                <th class="vn-orden vn-num" data-campo="Sueldo">Sueldo</th>
                                <th class="vn-orden vn-num" data-campo="ValorDia">Valor día</th>
                                <th class="vn-orden vn-num" data-campo="disponible">Cumplidos</th>
                                <th class="vn-orden vn-num" data-campo="proporcional">Proporcional</th>
                                <th class="vn-orden vn-num" data-campo="usadoEnCurso">Usado</th>
                                <th class="vn-orden vn-num" data-campo="dias">Días a liquidar</th>
                                <th class="vn-orden vn-num" data-campo="Valor">Valor ($)</th>
                                <th class="vn-orden" data-campo="FuenteSueldo">Fuente del sueldo</th>
                                <th>Observación</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                        <tfoot></tfoot>
                    </table>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading"><a data-toggle="collapse" href="#vnCriterios">Cómo se calcula</a></div>
                <div id="vnCriterios" class="panel-collapse collapse"><div class="panel-body">
                    <ul>
                        <li><b>Días a liquidar</b> = el saldo de vacaciones (el mismo de «Saldos de vacaciones»): disponible + proporcional − usado en el periodo en curso.</li>
                        <li><b>Valor</b> = días × sueldo ÷ 30, redondeado al centavo. Si el saldo es negativo (anticipo), el valor también lo es.</li>
                        <li><b>Sueldo:</b> el vigente a la fecha de cálculo en el maestro de horas extras (<code>HE_Salario</code>); si la persona no está ahí, el cargado desde el Excel de GTH.</li>
                        <li>Es <b>referencial</b>: en la liquidación la ley (Art. 71 CT) paga la veinticuatroava parte de lo percibido en el periodo; si hubo horas extra o comisiones, validar con el cálculo de nómina.</li>
                    </ul>
                </div></div>
            </div>
        </div>
    </div>
</asp:Content>
