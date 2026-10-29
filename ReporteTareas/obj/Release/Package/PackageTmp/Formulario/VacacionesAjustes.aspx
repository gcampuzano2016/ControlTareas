<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="VacacionesAjustes.aspx.cs" Inherits="ReporteTareas.Formulario.VacacionesAjustes" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .va-tabla { font-size: 12px; }
        .va-tabla td, .va-tabla th { padding: 5px 8px !important; vertical-align: top !important; }
        .va-tabla td.va-nowrap, .va-tabla th { white-space: nowrap; }
        .va-num { text-align: right; }
        .va-neg { color: #C0392B; font-weight: bold; }
        .va-anulada td { color: #999; text-decoration: line-through; }
        .va-anulada td.va-acciones { text-decoration: none; }
        .va-oculto { display: none; }
        .va-dato { margin-bottom: 4px; }
        .va-dato b { display: inline-block; min-width: 150px; }
        .va-simulacion { margin: 10px 0 0 0; }
        .va-motivo { min-height: 60px; }
    </style>
    <script src="../js/vacacionesAjustes.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Ajustes de vacaciones</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">Saldo de corte y compra de vacaciones. Cada cambio queda en la bitácora con su motivo.</div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-3"><label>Buscar</label><input type="text" id="txtBuscar" class="form-control" placeholder="Nombre o cédula" /></div>
                        <div class="form-group col-lg-5"><label>Colaborador</label><select id="cboColaborador" class="form-control"><option value="">Cargando...</option></select></div>
                    </div>
                    <div id="vaError" class="alert alert-warning va-oculto" style="margin-bottom: 0"></div>
                    <div id="vaOk" class="alert alert-success va-oculto" style="margin-bottom: 0"></div>
                </div>
            </div>
        </div>

        <div class="col-lg-12 va-oculto" id="vaResultado" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-body">
                    <div class="row">
                        <div class="col-md-6" id="vaPersona"></div>
                        <div class="col-md-6" id="vaSaldo"></div>
                    </div>
                    <div id="vaEmitidos" class="alert alert-info va-oculto" style="margin: 10px 0 0 0"></div>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading">Saldo de corte</div>
                <div class="panel-body">
                    <p class="text-muted">El saldo de corte es un saldo conciliado y firmado: el cálculo parte de él e ignora todo lo anterior a su fecha.</p>
                    <div class="row">
                        <div class="form-group col-lg-2"><label>Fecha de corte</label><input type="date" id="txtCorteFecha" class="form-control" /></div>
                        <div class="form-group col-lg-2"><label>Saldo a esa fecha (días)</label><input type="text" id="txtCorteSaldo" class="form-control" placeholder="0,00" /></div>
                        <div class="form-group col-lg-8"><label>Motivo</label><textarea id="txtCorteMotivo" class="form-control va-motivo" maxlength="1000" placeholder="Por qué se pone o cambia el corte (acta, conciliación, firma...)"></textarea></div>
                    </div>
                    <button type="button" class="btn btn-default" id="btnCorteSimular">Ver cómo queda</button>
                    <button type="button" class="btn btn-primary" id="btnCorteGuardar">Guardar corte</button>
                    <button type="button" class="btn btn-danger" id="btnCorteQuitar">Quitar corte</button>
                    <div id="vaSimulacion" class="alert alert-info va-simulacion va-oculto"></div>
                    <div id="vaQuitarConfirmar" class="alert alert-danger va-simulacion va-oculto">
                        <span id="vaQuitarTexto"></span>
                        <button type="button" class="btn btn-danger btn-sm" id="btnQuitarSi">Sí, quitar el corte</button>
                        <button type="button" class="btn btn-default btn-sm" id="btnQuitarNo">Cancelar</button>
                    </div>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading">Compras de vacaciones</div>
                <div class="panel-body">
                    <p class="text-muted">La compra descuenta sus días del saldo, sin cargo de fin de semana. No se borra: se anula, y queda en la bitácora.</p>
                    <div class="row">
                        <div class="form-group col-lg-2"><label>Fecha</label><input type="date" id="txtCompraFecha" class="form-control" /></div>
                        <div class="form-group col-lg-2"><label>Días comprados</label><input type="text" id="txtCompraDias" class="form-control" placeholder="0,00" /></div>
                        <div class="form-group col-lg-8"><label>Motivo</label><textarea id="txtCompraMotivo" class="form-control va-motivo" maxlength="1000" placeholder="Quién la aprobó y en qué rol de pagos se pagó"></textarea></div>
                    </div>
                    <button type="button" class="btn btn-primary" id="btnCompraRegistrar">Registrar compra</button>
                    <span class="text-muted" id="vaCompraPrevia" style="margin-left: 10px"></span>

                    <div style="overflow-x: auto; margin-top: 15px">
                        <table id="tblCompras" class="table table-condensed table-bordered va-tabla">
                            <thead><tr><th>Fecha</th><th class="va-num">Días</th><th>Estado</th><th>Origen</th><th>Motivo / observación</th><th></th></tr></thead>
                            <tbody></tbody>
                        </table>
                    </div>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading">Bitácora</div>
                <div class="panel-body" style="overflow-x: auto">
                    <table id="tblBitacora" class="table table-condensed table-bordered va-tabla">
                        <thead><tr><th>Cuándo</th><th>Usuario</th><th>Qué</th><th>Antes</th><th>Después</th><th>Motivo</th></tr></thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
