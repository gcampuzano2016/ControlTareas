<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="Agenda.aspx.cs" Inherits="ReporteTareas.Formulario.Agenda" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .ag-toolbar { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; margin: 0 0 12px; }
        .ag-buscar { flex: 1; min-width: 220px; }
        .ag-semana { display: flex; align-items: center; gap: 6px; }
        #agSemLabel { min-width: 210px; text-align: center; font-weight: 600; }
        .ag-areas { display: grid; grid-template-columns: repeat(auto-fill, minmax(230px, 1fr)); gap: 14px; }
        .ag-area { background: #fff; border: 1px solid #ddd; border-top: 4px solid #888; border-radius: 6px; padding: 14px; cursor: pointer; }
        .ag-area:hover { box-shadow: 0 4px 12px rgba(0,0,0,.12); }
        .ag-area-nombre { font-weight: 700; font-size: 14px; }
        .ag-area-cuenta, .ag-meta { font-size: 12px; color: #666; }
        .ag-avatares { display: flex; margin: 10px 0; }
        .ag-avatar { width: 30px; height: 30px; border-radius: 50%; background: #888; color: #fff; display: inline-flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; border: 2px solid #fff; margin-right: -6px; flex-shrink: 0; }
        .ag-mas { background: #e5e5e5; color: #555; }
        .ag-cifras { display: flex; gap: 12px; font-size: 12px; color: #555; flex-wrap: wrap; }
        .ag-rojo { color: #C62828; }
        .ag-personas { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 12px; }
        .ag-persona { background: #fff; border: 1px solid #ddd; border-radius: 6px; padding: 12px; cursor: pointer; }
        .ag-persona:hover { border-color: #2F6DB5; }
        .ag-persona-top { display: flex; gap: 14px; align-items: center; margin-bottom: 8px; }
        .ag-nombre { font-weight: 600; }
        .ag-chip { display: inline-block; color: #fff; font-size: 11px; padding: 1px 8px; border-radius: 10px; background: #888; }
        .ag-persona-cab { display: flex; align-items: center; gap: 14px; margin-bottom: 12px; flex-wrap: wrap; }
        .ag-persona-cab #agNueva { margin-left: auto; }
        .ag-franja { border: 1px solid #F3B4B4; background: #FDECEC; border-radius: 6px; padding: 10px; margin-bottom: 12px; }
        .ag-franja-titulo { font-weight: 700; color: #C62828; margin-bottom: 6px; }
        .ag-franja-lista { display: flex; flex-wrap: wrap; gap: 8px; }
        .ag-franja-lista .ag-act { width: 220px; }
        .ag-dias { display: grid; grid-template-columns: repeat(5, minmax(180px, 1fr)); gap: 10px; overflow-x: auto; }
        .ag-dia { background: #f7f7f7; border-radius: 6px; min-height: 320px; display: flex; flex-direction: column; }
        .ag-dia-cab { display: flex; justify-content: space-between; padding: 8px 10px; border-bottom: 2px solid #2F6DB5; }
        .ag-dia-cab.ag-hoy { border-bottom-color: #2E7D32; background: #EAF5EA; }
        .ag-dia-nombre { font-weight: 700; text-transform: uppercase; font-size: 12px; }
        .ag-dia-fecha { font-size: 11px; color: #666; }
        .ag-dia-cuerpo { padding: 8px; display: flex; flex-direction: column; gap: 6px; }
        .ag-vacio-dia, .ag-vacio { font-size: 12px; color: #888; text-align: center; padding: 12px; }
        .ag-act { background: #fff; border: 1px solid #ddd; border-radius: 6px; padding: 8px; cursor: pointer; font-size: 12px; }
        .ag-act.ag-completa { background: #EAF5EA; border-color: #9FD89F; }
        .ag-act.ag-atrasada { border-color: #E57373; border-left: 4px solid #C62828; }
        .ag-tit { font-weight: 600; margin-bottom: 4px; }
        .ag-progreso { display: flex; align-items: center; gap: 6px; font-size: 10px; color: #666; }
        .ag-barra { flex: 1; height: 4px; background: #e1dfdd; border-radius: 3px; overflow: hidden; }
        .ag-barra div { height: 100%; background: #2E7D32; }
        .ag-modal { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,.35); display: none; align-items: center; justify-content: center; z-index: 1050; padding: 16px; }
        .ag-modal.ag-abierto { display: flex; }
        .ag-modal-caja { background: #fff; border-radius: 6px; width: 100%; max-width: 520px; padding: 18px; max-height: 90vh; overflow-y: auto; }
        .ag-modal-ancha { max-width: 760px; }
        .ag-modal-cab { display: flex; align-items: center; gap: 10px; margin-bottom: 12px; }
        .ag-modal-cab button { margin-left: auto; }
        .ag-modal-titulo { font-size: 16px; font-weight: 700; }
        .ag-punto { width: 12px; height: 12px; border-radius: 50%; display: inline-block; }
        .ag-entregable { display: block; font-weight: normal; margin: 4px 0; }
        .ag-botones { display: flex; justify-content: flex-end; gap: 8px; margin-top: 14px; flex-wrap: wrap; }
    </style>
    <script src="../js/agenda.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Equipo y agenda</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div id="agError" class="alert alert-danger" style="display: none"></div>
            <div id="agAviso" class="alert" style="display: none"></div>

            <div class="ag-toolbar">
                <input type="text" id="agBuscar" class="form-control ag-buscar" placeholder="Buscar colaborador por nombre..." />
                <div class="ag-semana">
                    <button type="button" id="agSemAnt" class="btn btn-default">&lsaquo;</button>
                    <span id="agSemLabel"></span>
                    <button type="button" id="agSemSig" class="btn btn-default">&rsaquo;</button>
                    <button type="button" id="agSemHoy" class="btn btn-default">Hoy</button>
                </div>
                <button type="button" id="agMiAgenda" class="btn btn-default">Mi agenda</button>
                <button type="button" id="agAsignar" class="btn btn-primary">+ Asignar actividad</button>
            </div>

            <div id="vistaEquipo" style="display: none">
                <div id="agAreas" class="ag-areas"></div>
            </div>

            <div id="vistaPersona" style="display: none">
                <div class="ag-persona-cab">
                    <button type="button" id="agVolver" class="btn btn-default">&lsaquo; Volver al equipo</button>
                    <span id="agPerIniciales" class="ag-avatar"></span>
                    <div>
                        <div id="agPerNombre" class="ag-nombre"></div>
                        <span id="agPerArea" class="ag-chip"></span>
                    </div>
                    <button type="button" id="agNueva" class="btn btn-primary">+ Nueva actividad</button>
                </div>
                <div id="agAtrasadas"></div>
                <div id="agDias" class="ag-dias"></div>
            </div>
        </div>
    </div>

    <div id="agModalArea" class="ag-modal">
        <div class="ag-modal-caja ag-modal-ancha">
            <div class="ag-modal-cab">
                <span id="agModalAreaPunto" class="ag-punto"></span>
                <span id="agModalAreaTitulo" class="ag-modal-titulo"></span>
                <button type="button" class="btn btn-default btn-sm" data-cerrar="1">Cerrar</button>
            </div>
            <div id="agModalAreaPersonas" class="ag-personas"></div>
        </div>
    </div>

    <div id="agModalDetalle" class="ag-modal">
        <div class="ag-modal-caja">
            <div class="ag-modal-cab">
                <span id="agDetTitulo" class="ag-modal-titulo"></span>
                <button type="button" class="btn btn-default btn-sm" data-cerrar="1">Cerrar</button>
            </div>
            <div id="agDetMeta" class="ag-meta"></div>
            <div id="agDetError" class="alert alert-danger" style="display: none; margin-top: 8px"></div>
            <div id="agDetEntregables" style="margin-top: 10px"></div>
            <div id="agDetBotones" class="ag-botones"></div>
        </div>
    </div>

    <div id="agModalForm" class="ag-modal">
        <div class="ag-modal-caja">
            <div class="ag-modal-cab">
                <span id="agFormTitulo" class="ag-modal-titulo"></span>
                <button type="button" class="btn btn-default btn-sm" data-cerrar="1">Cerrar</button>
            </div>
            <div id="agFormError" class="alert alert-danger" style="display: none"></div>
            <div class="form-group">
                <label for="agFormTexto">¿Qué hay que hacer? *</label>
                <input type="text" id="agFormTexto" class="form-control" maxlength="200" />
            </div>
            <div class="form-group">
                <label for="agFormPersona">Asignar a *</label>
                <select id="agFormPersona" class="form-control"></select>
            </div>
            <div class="form-group">
                <label for="agFormFecha">Fecha de entrega *</label>
                <input type="date" id="agFormFecha" class="form-control" />
            </div>
            <div class="form-group">
                <label for="agFormEntregables">Entregables (uno por línea, opcional)</label>
                <textarea id="agFormEntregables" class="form-control" rows="4"></textarea>
            </div>
            <div class="ag-botones">
                <button type="button" class="btn btn-default" data-cerrar="1">Cancelar</button>
                <button type="button" id="agFormGuardar" class="btn btn-primary">Guardar</button>
            </div>
        </div>
    </div>
</asp:Content>
