<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="Procesos.aspx.cs" Inherits="ReporteTareas.Formulario.Procesos" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .pr-barra { margin-bottom: 15px; }
        .pr-barra label { margin-right: 6px; font-weight: normal; }
        .pr-barra select { display: inline-block; width: auto; margin-right: 14px; }
        .pr-tarjeta { border: 1px solid #ddd; border-radius: 4px; padding: 12px; margin-bottom: 14px; background: #fff; }
        .pr-cab { display: flex; flex-wrap: wrap; align-items: baseline; gap: 10px; margin-bottom: 4px; }
        .pr-nombre { font-size: 16px; font-weight: 700; }
        .pr-estado { font-size: 12px; padding: 1px 8px; border-radius: 10px; background: #eee; color: #444; }
        .pr-estado-EnCurso { background: #d9edf7; color: #31708f; }
        .pr-estado-Terminado { background: #dff0d8; color: #3c763d; }
        .pr-estado-Cancelado { background: #eee; color: #777; text-decoration: line-through; }
        .pr-meta { color: #777; font-size: 12px; margin-bottom: 8px; }
        .pr-pasos { display: flex; flex-wrap: wrap; gap: 8px; align-items: stretch; }
        .pr-chip { border: 1px solid #ccc; border-left-width: 5px; border-radius: 4px; padding: 6px 9px; min-width: 190px; max-width: 260px; background: #f3f3f3; color: #666; font-size: 12px; }
        .pr-chip .pr-t { font-weight: 700; display: block; }
        .pr-chip .pr-a { display: block; }
        .pr-punto { display: inline-block; width: 10px; height: 10px; border-radius: 50%; border: 1px solid #999; margin-right: 5px; vertical-align: middle; }
        .pr-chip-Completo { background: #dff0d8; border-color: #8cc17d; color: #2f6b2a; }
        .pr-chip-EnCurso { background: #d9edf7; border-color: #31708f; color: #1f4f6b; box-shadow: 0 0 0 2px rgba(49,112,143,.25); }
        .pr-chip-Pendiente { background: #f3f3f3; color: #777; }
        .pr-chip-Cancelado { background: #f3f3f3; color: #999; text-decoration: line-through; }
        .pr-chip-Atrasado { background: #f2dede; border-color: #a94442; color: #a94442; box-shadow: none; }
        .pr-inactivo { color: #c9302c; font-weight: 700; display: block; }
        .pr-acc { margin-top: 5px; text-decoration: none; }
        .pr-acc .btn { margin: 0 3px 3px 0; }
        .pr-motivo { margin-top: 8px; color: #777; font-size: 12px; }
        .pr-modal { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,.35); display: none; align-items: center; justify-content: center; z-index: 1050; padding: 16px; }
        .pr-modal.pr-abierto { display: flex; }
        .pr-caja { background: #fff; border-radius: 6px; width: 100%; max-width: 520px; padding: 18px; max-height: 90vh; overflow-y: auto; }
        .pr-caja.pr-ancha { max-width: 820px; }
        .pr-caja-titulo { font-size: 16px; font-weight: 700; margin-bottom: 12px; }
        .pr-caja-pie { margin-top: 14px; text-align: right; }
        .pr-fila { border: 1px solid #ddd; border-radius: 4px; padding: 10px; margin-bottom: 10px; background: #fafafa; }
        .pr-fila textarea { resize: vertical; }
        .pr-fila-cab { font-weight: bold; margin-bottom: 8px; }
    </style>
    <script src="../js/procesos.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Procesos</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="prError" class="alert alert-danger" style="display: none"></div>
            <div id="prAviso" class="alert" style="display: none"></div>

            <div class="pr-barra">
                <label for="prFiltroEstado">Estado</label>
                <select id="prFiltroEstado" class="form-control input-sm">
                    <option value="EnCurso" selected="selected">En curso</option>
                    <option value="Terminado">Terminados</option>
                    <option value="Cancelado">Cancelados</option>
                    <option value="">Todos</option>
                </select>
                <label for="prFiltroArea">Área</label>
                <select id="prFiltroArea" class="form-control input-sm">
                    <option value="0">Todas</option>
                </select>
                <button type="button" id="prIniciar" class="btn btn-primary btn-sm" style="display: none">+ Iniciar un proceso</button>
            </div>

            <div id="prLista"></div>
        </div>
    </div>

    <div id="prModal" class="pr-modal">
        <div class="pr-caja" id="prCaja">
            <div class="pr-caja-titulo" id="prModalTitulo"></div>
            <div id="prModalError" class="alert alert-danger" style="display: none"></div>
            <div id="prModalCuerpo"></div>
            <div class="pr-caja-pie">
                <button type="button" id="prModalCancelar" class="btn btn-default">Volver</button>
                <button type="button" id="prModalOk" class="btn btn-primary">Aceptar</button>
            </div>
        </div>
    </div>
</asp:Content>
