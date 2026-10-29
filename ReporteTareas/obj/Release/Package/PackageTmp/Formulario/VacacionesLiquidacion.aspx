<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="VacacionesLiquidacion.aspx.cs" Inherits="ReporteTareas.Formulario.VacacionesLiquidacion" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .vl-tarjeta { border: 1px solid #ddd; border-radius: 4px; padding: 10px; text-align: center; margin-bottom: 10px; border-top-width: 4px; min-height: 96px; }
        .vl-tarjeta .vl-titulo { font-weight: bold; }
        .vl-tarjeta .vl-valor { font-size: 20px; font-weight: bold; }
        .vl-cumplidos { border-top-color: #27AE60; }
        .vl-prop { border-top-color: #8E44AD; }
        .vl-usado { border-top-color: #555; }
        .vl-dias { border-top-color: #2F6DB5; background: #f4f8fd; }
        .vl-plata { border-top-color: #E07B24; }
        .vl-tabla { font-size: 12px; }
        .vl-tabla td, .vl-tabla th { white-space: nowrap; padding: 5px 8px !important; }
        .vl-num { text-align: right; }
        .vl-neg { color: #C0392B; font-weight: bold; }
        .vl-oculto { display: none; }
        .vl-dato { margin-bottom: 4px; }
        .vl-dato b { display: inline-block; min-width: 150px; }
    </style>
    <script src="../js/vacacionesLiquidacion.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Liquidación de vacaciones</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">Para salidas: elija a la persona y la fecha de salida. Solo cuenta lo tomado hasta esa fecha.</div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-2"><label>Buscar</label><input type="text" id="txtBuscar" class="form-control" placeholder="Nombre o cédula" /></div>
                        <div class="form-group col-lg-4"><label>Colaborador</label><select id="cboColaborador" class="form-control"><option value="">Cargando...</option></select></div>
                        <div class="form-group col-lg-2"><label>Fecha de salida</label><input type="date" id="txtFecha" class="form-control" /></div>
                        <div class="form-group col-lg-2"><label>Ganado últimos 12 meses <small>(opcional, Art. 71)</small></label><input type="text" id="txtGanado" class="form-control" placeholder="0,00" /></div>
                        <div class="form-group col-lg-2" style="padding-top: 25px"><button type="button" class="btn btn-primary" id="btnCalcular">Calcular</button></div>
                    </div>
                    <p class="text-muted" style="margin: 0">Si no cambia la fecha, se usa la de salida registrada; si no tiene, la de hoy.</p>
                    <div id="vlError" class="alert alert-warning vl-oculto" style="margin-top: 10px"></div>
                </div>
            </div>
        </div>

        <div class="col-lg-12 vl-oculto" id="vlResultado">
            <div class="panel panel-default">
                <div class="panel-body">
                    <div class="row">
                        <div class="col-md-6" id="vlPersona"></div>
                        <div class="col-md-6" id="vlSalida"></div>
                    </div>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading">Periodo en curso (proporcional)</div>
                <div class="panel-body">
                    <table class="table table-condensed table-bordered vl-tabla" style="width: auto; margin-bottom: 6px">
                        <thead><tr><th>Inicio del periodo</th><th>Próximo aniversario</th><th class="vl-num">Días transcurridos</th><th class="vl-num">Días por año</th><th class="vl-num">Proporcional ganado</th></tr></thead>
                        <tbody id="vlEnCurso"></tbody>
                    </table>
                    <p class="text-muted" id="vlFormula" style="margin: 0"></p>
                </div>
            </div>

            <h4>Resumen para liquidar</h4>
            <div class="row">
                <div class="col-lg-2 col-sm-4"><div class="vl-tarjeta vl-cumplidos"><div class="vl-titulo">Periodos cumplidos</div><div class="vl-valor" id="indDisponible">-</div><div>saldo disponible</div></div></div>
                <div class="col-lg-2 col-sm-4"><div class="vl-tarjeta vl-prop"><div class="vl-titulo">Proporcional</div><div class="vl-valor" id="indProporcional">-</div><div>periodo en curso</div></div></div>
                <div class="col-lg-2 col-sm-4"><div class="vl-tarjeta vl-usado"><div class="vl-titulo">Usado en curso</div><div class="vl-valor" id="indUsado">-</div><div>del periodo actual</div></div></div>
                <div class="col-lg-2 col-sm-6"><div class="vl-tarjeta vl-dias"><div class="vl-titulo">DÍAS A LIQUIDAR</div><div class="vl-valor" id="indDias">-</div><div id="indDiasNota">&nbsp;</div></div></div>
                <div class="col-lg-2 col-sm-6"><div class="vl-tarjeta vl-plata"><div class="vl-titulo">Valor referencial</div><div class="vl-valor" id="indValor">-</div><div>sueldo ÷ 30</div></div></div>
                <div class="col-lg-2 col-sm-6"><div class="vl-tarjeta vl-plata"><div class="vl-titulo">Valor Art. 71</div><div class="vl-valor" id="indArt71">-</div><div>ganado ÷ 360</div></div></div>
            </div>
            <div class="alert alert-info" id="vlPosteriores" style="display: none"></div>

            <div class="panel panel-default">
                <div class="panel-heading">Detalle por periodo</div>
                <div class="panel-body" style="overflow-x: auto">
                    <table id="tblPeriodos" class="table table-condensed table-bordered vl-tabla">
                        <thead><tr><th>Periodo</th><th>Inicio</th><th>Fin</th><th class="vl-num">Días por año</th><th class="vl-num">Tomado</th><th class="vl-num">Saldo</th><th class="vl-num">Vencido</th><th>Vence el</th><th>Estado</th></tr></thead>
                        <tbody></tbody>
                    </table>
                    <p class="text-muted" id="vlCorte" style="margin: 0"></p>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading">Solicitudes consideradas (hasta la fecha de salida)</div>
                <div class="panel-body" style="overflow-x: auto">
                    <table id="tblSolicitudes" class="table table-condensed table-bordered vl-tabla">
                        <thead><tr><th>Desde</th><th>Hasta</th><th>Tipo</th><th>Estado</th><th class="vl-num">Pedido</th><th class="vl-num">Descuenta</th><th>Periodo</th></tr></thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>

            <p class="text-muted">Art. 71 CT: el valor de las vacaciones es 1/24 de lo ganado en el último año (equivale a lo ganado ÷ 360 por cada día). El valor referencial usa sueldo ÷ 30. Si hubo horas extra o comisiones, validar con el cálculo de nómina.</p>
        </div>
    </div>
</asp:Content>
