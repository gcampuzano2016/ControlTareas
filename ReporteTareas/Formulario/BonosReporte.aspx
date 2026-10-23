<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="BonosReporte.aspx.cs" Inherits="ReporteTareas.Formulario.BonosReporte" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .br-tabla td { vertical-align: middle !important; }
        .br-num { text-align: right; }
    </style>
    <script src="../js/bonosReporte.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Reporte mensual de bonos</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="brError" class="alert alert-danger" style="display: none"></div>

            <div class="panel panel-default">
                <div class="panel-heading">Meses</div>
                <div class="panel-body">
                    <table class="table table-condensed table-striped br-tabla">
                        <thead>
                            <tr>
                                <th>Año</th><th>Mes</th><th>Estado</th>
                                <th class="br-num">Total</th><th class="br-num">Variación</th><th></th>
                            </tr>
                        </thead>
                        <tbody id="brMeses"></tbody>
                    </table>
                    <p class="text-muted">La variación compara con el mes anterior. Los meses históricos no tienen Excel.</p>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
