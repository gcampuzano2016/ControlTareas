<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="AprobarHorasExtras.aspx.cs" Inherits="ReporteTareas.Formulario.AprobarHorasExtras" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">


                <div class="row">
                    <div class="col-lg-12">

                            <div class="panel-body">

                                <div class="row">
                                    <h1>Solicitar Aprobación de Horas Extras</h1>
                                </div>
                            </div>
          
                    </div>
                </div>



        <asp:Panel ID="Panel1" runat="server" Visible="true" Enabled="true">
            <div class="row">
                <div class="col-lg-12">
                    <div class="panel panel-default">
                        <div class="panel-body">
                            <div class="row">
                                <div class="col-lg-12 col-md-12 col-sm-12 col-xs-12">

                                    <%--<asp:GridView ID="dgv_Detalle" runat="server" CssClass="table-responsive table-striped table-bordered table-hover" AutoGenerateColumns="False" OnSelectedIndexChanged="dgv_Detalle_SelectedIndexChanged" >--%>
                                    <asp:GridView ID="dgv_Tareas" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None" CssClass="table-responsive table-striped table-bordered table-hover" AutoGenerateColumns="False" Width="100%" OnSelectedIndexChanged="dgv_Tareas_SelectedIndexChanged" OnRowCommand="dgv_Tareas_RowCommand" OnRowEditing="dgv_Tareas_RowEditing">
                                        <%--<asp:GridView ID="dgv_Tareas" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None"  class="table table-condensed table-bordered table-hover" AutoGenerateColumns="False" OnSelectedIndexChanged="dgv_Tareas_SelectedIndexChanged" Width="100%">--%>
                                        <Columns>
                                            <asp:CommandField ButtonType="Button"  SelectText=" " ShowInsertButton="True" NewText="Solicitar Aprobar H. Extras" ControlStyle-CssClass ="btn btn-primary" />
                                            <asp:CommandField ButtonType="Button"  SelectText=" " ShowEditButton="True" EditText="Revisar Tarea"  ControlStyle-CssClass ="btn btn-primary" />
                                            <asp:BoundField DataField="Id_RegTareas" HeaderText="ID." Visible="true" />
                                            <asp:BoundField DataField="Num_OrdenServicio" HeaderText="N° OS" Visible="true" />
                                            <asp:BoundField DataField="Id_CompAranda" HeaderText="N° Ticket" Visible="false" />
                                            <asp:BoundField DataField="Fch_Registro" HeaderText="Fch. Registro" Visible="false" />
                                            <asp:BoundField DataField="Fch_EstAtencion" HeaderText="Fch. Est. Atencion" Visible="false" />
                                            <asp:BoundField DataField="Fch_EstSolucion" HeaderText="Fch. Est. Solucion" Visible="false" />
                                            <asp:BoundField DataField="Id_Responsable" HeaderText="Id. Responsable" Visible="false" />
                                            <asp:BoundField DataField="Nom_Responsable" HeaderText="Nom. Responsable" Visible="false" />
                                            <asp:BoundField DataField="Nom_Empresa" HeaderText="Nom. Cliente" Visible="true" />
                                            <asp:BoundField DataField="Nom_SlaAranda" HeaderText="Tipo SLA" Visible="false" />
                                            <asp:BoundField DataField="Det_Tarea" HeaderText="Det. Tarea" Visible="true" />
                                            <asp:BoundField DataField="Estado" HeaderText="Estado Tarea" Visible="true" />
                                            <asp:BoundField DataField="Estado2" HeaderText="Estado Horas Extra" />
                                            <asp:BoundField DataField="Id_RegTareasHorasExtras" HeaderText="Horas Extras" Visible="False" />
                                            <asp:BoundField DataField="ResponsableAprobacion" HeaderText="Responsable de Autorizar" />
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="Panel3" runat="server" Visible="false" Enabled="false">
            <div class="row">
                <div class="col-lg-12">
                    <div class="panel panel-default">
                        <div class="panel-body">
                            <div class="row">
                                <div class="col-lg-12 col-md-12 col-sm-12 col-xs-12">
                                    <div class="row">
                                        <div class="col-lg-12">
                                            <div class="row">
                                                <div class="row">
                                                    <div class="col-lg-12">
                                                        <div class="col-lg-3">
                                                            <h6>
                                                                <asp:Label ID="lbl_CambioEstado" Text="Cambio de Estado" runat="server" CssClass="text-center"></asp:Label></h6>
                                                            <asp:DropDownList ID="dbl_Estado" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True" OnSelectedIndexChanged="dbl_Estado_SelectedIndexChanged1"></asp:DropDownList>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-12">
                                                        <div class="col-lg-3">
                                                            <h6>
                                                                <asp:Label ID="lbl_DetCamEstado" Text="Comentario de cambio de estado " runat="server" CssClass="text-center"></asp:Label></h6>
                                                            <asp:DropDownList ID="dbl_DetEstado" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True" Enabled="false"></asp:DropDownList>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-12">
                                                        <div class="col-lg-4">
                                                        </div>
                                                        <div class="col-lg-4">
                                                            <asp:Button ID="btn_Guardar" runat="server" Text="Guardar" OnClick="btn_Guardar_Click" Enabled="false" />
                                                            <asp:Button ID="btn_Guardar0" runat="server" OnClick="btn_Guardar0_Click" Text="Regresar" Visible="False" />
                                                        </div>
                                                        <div class="col-lg-4">
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-lg-12">
                                                        <br />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="Panel2" runat="server" Visible="false" Enabled="false">
            <%--/////////////////////////////////////////////--%>
            <div class="row">
                <div class="col-lg-12">
                    <div class="panel panel-default">
                        <div class="panel-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <div class="row">
                                        <div class="row">
                                            <div class="col-lg-12">

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lbl_Os" Text="N° Orden de Servicio" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txt_Os" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lbl_Ticket" Text="Cod. Ticket" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txt_Ticket" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lbl_FchRegistro" Text="Fch. Registro" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txt_FchRegistro" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lbl_Estado" Text="Estado" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txt_Estado" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-lg-12">

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lbl_Cliente" Text="Nom. Cliente" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txt_Cliente" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lbl_NomResponsable" Text="Nom. Responsable" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txt_NomResponsable" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lbl_FchTenInicio" Text="Fch. Inicio" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txt_FchTenInicio" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>
                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lbl_FchTenFin" Text="Fch. Fin" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txt_FchTenFin" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-lg-12">
                                                <div class="col-lg-3">
                                                    <asp:TextBox ID="txt_IdRegistro" runat="server" CssClass="form-control tam-text-box" Enabled="true" Visible="false" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-lg-12">
                                                <br />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <%--///////////////////////////////////////////////////////////////////////////////////--%>
        </asp:Panel>
    </div>
</asp:Content>
