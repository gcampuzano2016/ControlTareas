<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionAreasAgenda.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionAreasAgenda" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .pa-tabla td { vertical-align: middle !important; }
        .pa-tabla input[type=color] { width: 48px; height: 30px; padding: 0; border: 1px solid #ccc; }
    </style>
    <script src="../js/parametrizacionAreasAgenda.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Áreas de la agenda</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="paError" class="alert alert-danger" style="display: none"></div>
            <div id="paAviso" class="alert" style="display: none"></div>

            <div class="panel panel-default">
                <div class="panel-heading">Áreas</div>
                <div class="panel-body">
                    <table class="table table-condensed pa-tabla">
                        <thead><tr><th>Nombre</th><th>Color</th><th>Activa</th><th>Personas</th><th></th></tr></thead>
                        <tbody id="paAreas"></tbody>
                    </table>
                    <button type="button" id="paNueva" class="btn btn-primary">+ Nueva área</button>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading">Personas</div>
                <div class="panel-body">
                    <div class="row" style="margin-bottom: 10px">
                        <div class="col-lg-4"><input type="text" id="paBuscar" class="form-control" placeholder="Buscar por nombre..." /></div>
                        <div class="col-lg-3"><select id="paFiltro" class="form-control"></select></div>
                    </div>
                    <table class="table table-condensed pa-tabla">
                        <thead><tr><th>Persona</th><th>Área</th></tr></thead>
                        <tbody id="paPersonas"></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
