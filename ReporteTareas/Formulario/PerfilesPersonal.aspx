<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="PerfilesPersonal.aspx.cs" Inherits="ReporteTareas.Formulario.PerfilesPersonal" ResponseEncoding="utf-8" %>
<%@ Register Src="~/Controles/PerfilFichas.ascx" TagPrefix="rta" TagName="PerfilFichas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        /* Apaga la carga inicial de miPerfil.js. Aqui no hay a quien mostrar
           hasta que Talento Humano elija a una persona: sin esto se cargaria el
           perfil de quien mira dentro de las fichas de otro. En MiPerfil.aspx
           esta bandera no existe y aquella pagina carga como siempre. */
        var PERFIL_SIN_CARGA_INICIAL = true;

        /* Enciende el formulario de edicion de "Datos personales". Es de ALCANCE,
           no de seguridad: quien puede editar lo decide NegPerfilAcceso en el
           servidor, y el handler lo vuelve a comprobar en cada llamada. Esto solo
           evita que "Mi perfil" cambie para las 228 personas que la usan. */
        var PERFIL_DATOS_EDITABLES = true;
    </script>
    <script src="../js/miPerfil.js?v=12" type="text/javascript"></script>
    <script src="../js/perfilesPersonal.js?v=3" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%-- Un solo page-wrapper para TODA la pantalla: el buscador, la tabla y las
         fichas. Es el id que despeja los 250px del menu lateral, y por ser id
         solo puede haber uno. Si el buscador quedara fuera, se dibuja debajo
         del menu - que es exactamente lo que pasaba antes de este envoltorio. --%>
    <div id="page-wrapper" style="padding: 0px">
    <div class="row">
        <div class="col-lg-12" style="padding: 20px">
            <div class="card card-primary">
                <div class="card-header" style="text-align: center">
                    <h3>Perfiles del personal</h3>
                </div>
                <div class="card-body" style="padding: 15px">

                    <div class="row">
                        <div class="col-md-6">
                            <div class="input-group">
                                <input type="text" class="form-control" id="txtBuscarPersonal"
                                       maxlength="100" placeholder="Nombre, código o cargo" />
                                <span class="input-group-btn">
                                    <button type="button" id="btnBuscarPersonal" class="btn btn-primary">
                                        <i class="fa fa-search"></i> Buscar
                                    </button>
                                </span>
                            </div>
                            <p class="text-muted" style="margin-top: 6px; font-size: 12px">
                                Escriba al menos 2 caracteres. No aparece el personal inactivo,
                                ni quienes tienen el código de usuario repetido.
                            </p>
                        </div>
                    </div>

                    <div class="table-responsive" style="margin-top: 10px">
                        <table class="table table-hover table-condensed">
                            <thead>
                                <tr>
                                    <th>Nombre</th>
                                    <th>Código</th>
                                    <th>Cargo</th>
                                    <th>Área</th>
                                    <th style="width: 60px"></th>
                                </tr>
                            </thead>
                            <tbody id="cuerpoPersonal"></tbody>
                        </table>
                    </div>

                </div>
            </div>
        </div>
    </div>

    <%-- Historial de certificados laborales de todo el personal. Lo pide el
         handler solo con el permiso de esta pantalla. Arranca plegado: el uso
         principal de la pantalla sigue siendo buscar a una persona. --%>
    <div class="row">
        <div class="col-lg-12" style="padding: 0 20px 20px 20px">
            <div class="panel panel-default" style="margin: 0">
                <div class="panel-heading">
                    <a href="javascript:void(0)" id="lnkHistorialCertificados">
                        <i class="fa fa-file-text-o"></i> Historial de certificados laborales
                        <i class="fa fa-chevron-down" id="icoHistorialCertificados"></i>
                    </a>
                    <span class="text-muted pull-right" style="font-size: 12px" id="resumenCertificados"></span>
                </div>
                <div class="panel-body" id="cuerpoHistorialCertificados" style="display: none">
                    <div class="row" id="kpisCertificados" style="margin-bottom: 12px"></div>

                    <div class="row">
                        <div class="form-group col-md-4">
                            <input type="search" class="form-control" id="fCertTexto" placeholder="Buscar por nombre o cédula…" />
                        </div>
                        <div class="form-group col-md-3">
                            <select class="form-control" id="fCertEmpresa">
                                <option value="">Todas las empresas</option>
                                <option value="DOS">COMPUEQUIP DOS</option>
                                <option value="AGILITY">AGILITY</option>
                            </select>
                        </div>
                        <div class="form-group col-md-3">
                            <select class="form-control" id="fCertTipo">
                                <option value="">Con y sin salario</option>
                                <option value="CON">Con salario</option>
                                <option value="SIN">Sin salario</option>
                            </select>
                        </div>
                        <div class="form-group col-md-2">
                            <button type="button" class="btn btn-default btn-block" id="btnCertCsv">
                                <i class="fa fa-download"></i> Exportar CSV
                            </button>
                        </div>
                    </div>

                    <div class="table-responsive">
                        <table class="table table-hover table-condensed">
                            <thead>
                                <tr>
                                    <th style="width: 130px">Fecha y hora</th>
                                    <th>Colaborador</th>
                                    <th>Empresa</th>
                                    <th>Tipo</th>
                                    <th>Firma</th>
                                    <th style="width: 70px"></th>
                                </tr>
                            </thead>
                            <tbody id="cuerpoCertificadosTodos"></tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div id="panelFichas" style="display: none">
        <div class="alert alert-info" style="margin: 0 20px">
            Está viendo el perfil de <b id="personaElegida">–</b>.
            Los cambios que guarde quedan registrados a su nombre.
        </div>
        <rta:PerfilFichas runat="server" ID="fichas" />
    </div>
    </div>
</asp:Content>
