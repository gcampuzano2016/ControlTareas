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
    <script src="../js/miPerfil.js?v=10" type="text/javascript"></script>
    <script src="../js/perfilesPersonal.js?v=2" type="text/javascript"></script>
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

    <div id="panelFichas" style="display: none">
        <div class="alert alert-info" style="margin: 0 20px">
            Está viendo el perfil de <b id="personaElegida">–</b>.
            Los cambios que guarde quedan registrados a su nombre.
        </div>
        <rta:PerfilFichas runat="server" ID="fichas" />
    </div>
    </div>
</asp:Content>
