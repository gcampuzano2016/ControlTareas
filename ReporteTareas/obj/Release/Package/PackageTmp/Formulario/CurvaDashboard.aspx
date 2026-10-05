<%@ Page Title="Dashboard curva de aprendizaje" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CurvaDashboard.aspx.cs" Inherits="ReporteTareas.Formulario.CurvaDashboard" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../css/curvaAprendizaje.css?v=1" rel="stylesheet" />
    <script src="../js/chart.umd.js" type="text/javascript"></script>
    <script src="../js/curvaComun.js?v=1" type="text/javascript"></script>
    <script src="../js/curvaDashboard.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="ca-pagina">
            <div class="ca-encabezado">
                <p class="ca-kicker">RRHH · DOS</p>
                <h1>Dashboard</h1>
                <p class="ca-sub">Vista agregada del programa de curva de aprendizaje</p>
            </div>

            <div class="ca-kpis">
                <div class="ca-kpi"><span>Colaboradores</span><b id="kpiColab">—</b></div>
                <div class="ca-kpi"><span>Aprobación período de prueba</span><b id="kpiAprob">—</b></div>
                <div class="ca-kpi"><span>Compromisos realizados</span><b id="kpiComp">—</b></div>
                <div class="ca-kpi"><span>Procesos en curso</span><b id="kpiCurso">—</b></div>
            </div>

            <div class="ca-panel">
                <h2>Casos con alerta</h2>
                <div id="caCasos"><div class="ca-vacio">Cargando…</div></div>
            </div>

            <div class="ca-dos-columnas">
                <div class="ca-panel">
                    <h2>Decisiones de período de prueba</h2>
                    <div class="ca-grafico"><canvas id="grafDecisiones"></canvas></div>
                    <p class="ca-vacio" id="sinDecisiones" style="display: none">Todavía no hay decisiones registradas.</p>
                </div>
                <div class="ca-panel">
                    <h2>Cumplimiento de compromisos</h2>
                    <div class="ca-grafico"><canvas id="grafCompromisos"></canvas></div>
                </div>
                <div class="ca-panel ca-ancho">
                    <h2>Evolución promedio de competencias (evaluación del jefe)</h2>
                    <div class="ca-grafico"><canvas id="grafCompetencias"></canvas></div>
                    <p class="ca-vacio" id="sinCompetencias" style="display: none">Aún no hay suficientes evaluaciones de jefe en 90 días o año para mostrar una tendencia.</p>
                </div>
                <div class="ca-panel">
                    <h2>Temas de capacitación más recurrentes</h2>
                    <div id="caTemas"></div>
                </div>
                <div class="ca-panel">
                    <h2>Próximas evaluaciones</h2>
                    <div id="caProximas"></div>
                </div>
            </div>
        </div>

        <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeInformativoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title">Informativo</h4>
                    </div>
                    <div class="modal-body" id="MensajeInformativo"></div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
