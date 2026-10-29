<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="MisVacaciones.aspx.cs" Inherits="ReporteTareas.Formulario.MisVacaciones" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mv-tarjeta { border: 1px solid #ddd; border-radius: 4px; padding: 10px; text-align: center; margin-bottom: 10px; border-top-width: 4px; min-height: 92px; }
        .mv-tarjeta .mv-titulo { font-weight: bold; }
        .mv-tarjeta .mv-valor { font-size: 22px; font-weight: bold; }
        .mv-saldo { border-top-color: #2F6DB5; background: #f4f8fd; }
        .mv-disp { border-top-color: #27AE60; }
        .mv-prop { border-top-color: #8E44AD; }
        .mv-usado { border-top-color: #555; }
        .mv-vencer { border-top-color: #E07B24; }
        .mv-tabla { font-size: 12px; }
        .mv-tabla td, .mv-tabla th { white-space: nowrap; padding: 5px 8px !important; }
        .mv-tabla td.mv-texto { white-space: normal; min-width: 180px; }
        .mv-num { text-align: right; }
        .mv-neg { color: #C0392B; font-weight: bold; }
        .mv-tenue td { color: #999 !important; }
        .mv-oculto { display: none; }
        .mv-dato { margin-bottom: 4px; }
        .mv-dato b { display: inline-block; min-width: 140px; }
    </style>
    <script src="../js/misVacaciones.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Mis vacaciones</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="mvError" class="alert alert-warning mv-oculto"></div>
            <div id="mvCargando" class="text-muted">Consultando su saldo...</div>

            <div id="mvContenido" class="mv-oculto">
                <div class="panel panel-default">
                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-6" id="mvPersona"></div>
                            <div class="col-md-6" id="mvAniversario"></div>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-lg-3 col-sm-6"><div class="mv-tarjeta mv-saldo"><div class="mv-titulo">Saldo a la fecha</div><div class="mv-valor" id="indSaldo">-</div><div id="indFecha"></div></div></div>
                    <div class="col-lg-2 col-sm-6"><div class="mv-tarjeta mv-disp"><div class="mv-titulo">Disponible</div><div class="mv-valor" id="indDisponible">-</div><div>periodos cumplidos</div></div></div>
                    <div class="col-lg-2 col-sm-4"><div class="mv-tarjeta mv-prop"><div class="mv-titulo">Proporcional</div><div class="mv-valor" id="indProporcional">-</div><div>periodo en curso</div></div></div>
                    <div class="col-lg-2 col-sm-4"><div class="mv-tarjeta mv-usado"><div class="mv-titulo">Usado en curso</div><div class="mv-valor" id="indUsado">-</div><div>del periodo actual</div></div></div>
                    <div class="col-lg-3 col-sm-4"><div class="mv-tarjeta mv-vencer"><div class="mv-titulo">Por vencer</div><div class="mv-valor" id="indPorVencer">-</div><div id="indVence">en los próximos 90 días</div></div></div>
                </div>

                <div class="alert alert-info" id="mvMensaje"></div>

                <div class="panel panel-default">
                    <div class="panel-heading">Mis periodos</div>
                    <div class="panel-body" style="overflow-x: auto">
                        <table id="tblPeriodos" class="table table-condensed table-bordered mv-tabla">
                            <thead><tr><th>Periodo</th><th>Desde</th><th>Hasta</th><th class="mv-num">Ganados</th><th class="mv-num">Tomados</th><th class="mv-num">Vencidos</th><th class="mv-num">Saldo</th><th>Vence el</th><th>Estado</th></tr></thead>
                            <tbody></tbody>
                        </table>
                        <p class="text-muted" style="margin: 0">Los días se descuentan siempre del periodo más antiguo con saldo. Los periodos van de aniversario a aniversario de su fecha de ingreso.</p>
                    </div>
                </div>

                <div class="panel panel-default">
                    <div class="panel-heading">Mis solicitudes que descuentan</div>
                    <div class="panel-body" style="overflow-x: auto">
                        <table id="tblSolicitudes" class="table table-condensed table-bordered mv-tabla">
                            <thead><tr><th>Desde</th><th>Hasta</th><th>Tipo</th><th>Estado</th><th class="mv-num">Pedido</th><th class="mv-num">Fin de semana</th><th class="mv-num">Descuenta</th><th>Periodo</th><th>Observación</th></tr></thead>
                            <tbody></tbody>
                        </table>
                        <p class="text-muted" id="mvOtras" style="margin: 0"></p>
                    </div>
                </div>

                <div class="panel panel-default">
                    <div class="panel-heading"><a data-toggle="collapse" href="#mvCriterios">Cómo se calcula</a></div>
                    <div id="mvCriterios" class="panel-collapse collapse"><div class="panel-body">
                        <ul>
                            <li><b>Días por año:</b> 15 por año cumplido, y uno más por cada año después del quinto, hasta 15 adicionales.</li>
                            <li><b>Saldo</b> = disponible de los periodos vigentes + proporcional del periodo en curso − lo usado en el periodo en curso. Si queda negativo es un anticipo.</li>
                            <li><b>Vencimiento:</b> cada periodo vence tres años después de terminar; lo que no se goce hasta entonces se pierde.</li>
                            <li><b>Fin de semana:</b> una solicitud que termina en viernes carga 2 días (salvo Beneficio DOS), y cada 5 días sueltos en el mismo año de servicio cargan 2 más; máximo 2 fines de semana por periodo.</li>
                            <li><b>Permisos por horas</b> = (horas + minutos/60) ÷ 8. Las solicitudes por aprobar ya descuentan.</li>
                            <li>Si algo no le cuadra, escriba a Talento Humano.</li>
                        </ul>
                    </div></div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
