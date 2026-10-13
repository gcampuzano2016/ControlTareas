<%@ Page Title="Curva de aprendizaje" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CurvaColaboradores.aspx.cs" Inherits="ReporteTareas.Formulario.CurvaColaboradores" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../css/curvaAprendizaje.css?v=1" rel="stylesheet" />
    <script src="../js/curvaComun.js?v=1" type="text/javascript"></script>
    <script src="../js/curvaColaboradores.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="ca-pagina">
            <div class="ca-encabezado">
                <p class="ca-kicker">RRHH · DOS</p>
                <h1>Colaboradores</h1>
                <p class="ca-sub">Seguimiento de onboarding y curva de aprendizaje del primer año</p>
                <div class="ca-chips" id="caChips" style="display: none"></div>
            </div>

            <div class="ca-panel">
                <div class="ca-toolbar">
                    <input type="text" id="caBuscar" class="ca-buscador" placeholder="Buscar por nombre…" autocomplete="off" />
                    <button type="button" class="ca-filtro activo" data-filtro="">Todos</button>
                    <button type="button" class="ca-filtro" data-filtro="activo">Activos</button>
                    <button type="button" class="ca-filtro" data-filtro="inactivo">Inactivos</button>
                    <a class="ca-btn ca-btn-primario" href="CurvaColaboradorNuevo.aspx">+ Nuevo colaborador</a>
                </div>
                <div class="ca-lista-cab">
                    <span>Colaborador</span><span>Área</span><span>Ingreso</span><span>Estado</span><span>Proceso</span><span></span>
                </div>
                <div id="caLista"><div class="ca-vacio">Cargando…</div></div>
                <div id="caSinResultados" class="ca-vacio" style="display: none">No se encontraron colaboradores con ese filtro.</div>
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
