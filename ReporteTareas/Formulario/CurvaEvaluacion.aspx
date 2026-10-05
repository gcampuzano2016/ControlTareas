<%@ Page Title="Evaluación" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CurvaEvaluacion.aspx.cs" Inherits="ReporteTareas.Formulario.CurvaEvaluacion" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../css/curvaAprendizaje.css?v=1" rel="stylesheet" />
    <script src="../js/curvaComun.js?v=1" type="text/javascript"></script>
    <script src="../js/curvaFormulario.js?v=1" type="text/javascript"></script>
    <script src="../js/curvaEvaluacion.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="ca-pagina">
            <p><a id="caVolver" href="CurvaColaboradores.aspx">← Volver a la ficha</a></p>
            <div class="ca-encabezado" id="caEncabezado" style="display: none">
                <p class="ca-kicker" id="caKicker"></p>
                <h1 id="caTitulo"></h1>
                <p class="ca-sub" id="caSubtitulo"></p>
                <p class="ca-sub" id="caAvisoEdicion" style="display: none">Esta evaluación ya estaba completada — los cambios se guardan sobre las respuestas existentes.</p>
            </div>
            <div id="caFormulario"><div class="ca-vacio">Cargando…</div></div>
            <div class="ca-guardar" id="caBotonera" style="display: none">
                <button type="button" id="caGuardar" class="ca-btn ca-btn-primario">Guardar y completar evaluación</button>
                <a class="ca-btn ca-btn-outline" id="caCancelar" href="CurvaColaboradores.aspx">Cancelar</a>
            </div>
            <p class="ca-nota" id="caNotaIA" style="display: none; margin-top: 8px">Al guardar se genera el resumen ejecutivo; puede tardar unos segundos.</p>
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
