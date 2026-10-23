<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionPlantillasProcesos.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionPlantillasProcesos" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .pp-tabla td { vertical-align: middle !important; }
        .pp-chip { display: inline-block; width: 12px; height: 12px; border-radius: 50%; margin-right: 6px; vertical-align: middle; border: 1px solid #999; }
        .pp-paso { border: 1px solid #ddd; border-radius: 4px; padding: 10px; margin-bottom: 10px; background: #fafafa; }
        .pp-paso textarea { resize: vertical; }
        .pp-paso-cab { font-weight: bold; margin-bottom: 8px; }
    </style>
    <script src="../js/parametrizacionPlantillasProcesos.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Plantillas de procesos</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="ppError" class="alert alert-danger" style="display: none"></div>
            <div id="ppAviso" class="alert" style="display: none"></div>

            <div class="panel panel-default" id="ppListaPanel">
                <div class="panel-heading">Plantillas</div>
                <div class="panel-body">
                    <table class="table table-condensed pp-tabla">
                        <thead><tr><th>Nombre</th><th>Pasos</th><th>Activa</th><th>Con procesos</th><th></th></tr></thead>
                        <tbody id="ppLista"></tbody>
                    </table>
                    <button type="button" id="ppNueva" class="btn btn-primary">+ Nueva plantilla</button>
                </div>
            </div>

            <div class="panel panel-default" id="ppEditor" style="display: none">
                <div class="panel-heading" id="ppEditorTitulo">Plantilla</div>
                <div class="panel-body">
                    <div id="ppNotaProcesos" class="alert alert-info" style="display: none">
                        Esta plantilla ya tiene procesos. Puede editarla: los procesos en curso conservan su propia copia de los pasos y no cambian.
                    </div>
                    <div class="row">
                        <div class="col-lg-6 form-group">
                            <label for="ppNombre">Nombre</label>
                            <input type="text" id="ppNombre" class="form-control" maxlength="100" />
                        </div>
                        <div class="col-lg-2 form-group">
                            <label for="ppActiva">Activa</label><br />
                            <input type="checkbox" id="ppActiva" />
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="ppDescripcion">Descripción</label>
                        <textarea id="ppDescripcion" class="form-control" rows="2" maxlength="500"></textarea>
                    </div>

                    <h4>Pasos</h4>
                    <div id="ppPasos"></div>
                    <button type="button" id="ppAgregar" class="btn btn-default btn-sm">+ Agregar paso</button>
                    <hr />
                    <button type="button" id="ppGuardar" class="btn btn-primary">Guardar plantilla</button>
                    <button type="button" id="ppCancelar" class="btn btn-default">Cancelar</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
