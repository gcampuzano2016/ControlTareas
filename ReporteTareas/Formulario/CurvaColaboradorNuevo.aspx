<%@ Page Title="Nuevo colaborador" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CurvaColaboradorNuevo.aspx.cs" Inherits="ReporteTareas.Formulario.CurvaColaboradorNuevo" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../css/curvaAprendizaje.css?v=1" rel="stylesheet" />
    <script src="../js/curvaComun.js?v=1" type="text/javascript"></script>
    <script src="../js/curvaNuevo.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="ca-pagina">
            <div class="ca-encabezado">
                <p class="ca-kicker">RRHH · DOS</p>
                <h1>Nuevo colaborador</h1>
                <p class="ca-sub">Al registrarlo se abre su proceso de onboarding y se generan las evaluaciones de los 30 días.</p>
            </div>

            <div class="ca-panel" style="max-width: 760px">
                <h2>Vincular con su ficha del sistema (opcional)</h2>
                <p class="ca-nota">Si la persona ya está en «Datos Empleado», búscala para copiar sus datos y dejarla enlazada. Si todavía no tiene ficha, llena los campos a mano.</p>
                <input type="text" id="caBuscarFicha" class="ca-buscador" style="width: 100%" placeholder="Escribe al menos 3 letras del nombre…" autocomplete="off" />
                <div id="caResultadosFicha" style="margin-top: 8px"></div>
                <p id="caFichaVinculada" class="ca-nota" style="display: none"></p>
            </div>

            <div class="ca-panel" style="max-width: 760px">
                <h2>Datos del colaborador</h2>
                <div class="row">
                    <div class="form-group col-sm-12">
                        <label for="caNombre">Nombre completo *</label>
                        <input type="text" id="caNombre" class="form-control" maxlength="200" />
                    </div>
                    <div class="form-group col-sm-6">
                        <label for="caCargo">Cargo</label>
                        <input type="text" id="caCargo" class="form-control" maxlength="200" />
                    </div>
                    <div class="form-group col-sm-6">
                        <label for="caArea">Área o departamento</label>
                        <input type="text" id="caArea" class="form-control" maxlength="150" />
                    </div>
                    <div class="form-group col-sm-6">
                        <label for="caEmpresa">Empresa</label>
                        <input type="text" id="caEmpresa" class="form-control" maxlength="150" value="DOS" />
                    </div>
                    <div class="form-group col-sm-6">
                        <label for="caUnidad">Unidad de negocio</label>
                        <input type="text" id="caUnidad" class="form-control" maxlength="150" />
                    </div>
                    <div class="form-group col-sm-6">
                        <label for="caIngreso">Fecha de ingreso *</label>
                        <input type="date" id="caIngreso" class="form-control" />
                    </div>
                </div>
                <input type="hidden" id="caIdEmpleado" value="" />
                <div class="ca-guardar">
                    <button type="button" id="caCrear" class="ca-btn ca-btn-primario">Iniciar curva de aprendizaje</button>
                    <a class="ca-btn ca-btn-outline" href="CurvaColaboradores.aspx">Cancelar</a>
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
