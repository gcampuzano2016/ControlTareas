<%@ Page Title="Ficha del colaborador" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CurvaColaboradorFicha.aspx.cs" Inherits="ReporteTareas.Formulario.CurvaColaboradorFicha" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../css/curvaAprendizaje.css?v=1" rel="stylesheet" />
    <script src="../js/chart.umd.js" type="text/javascript"></script>
    <script src="../js/curvaComun.js?v=1" type="text/javascript"></script>
    <script src="../js/curvaFicha.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="ca-pagina">
            <p><a href="CurvaColaboradores.aspx">← Colaboradores</a></p>
            <div id="caFicha"><div class="ca-vacio">Cargando…</div></div>
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
