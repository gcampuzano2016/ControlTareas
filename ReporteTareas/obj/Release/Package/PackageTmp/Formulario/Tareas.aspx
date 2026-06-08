<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="Tareas.aspx.cs" Inherits="ReporteTareas.Formulario.Tareas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
        <div class="row">
            <div class="col-lg-12">
                <div class="panel-body">
                    <div class="row">
                        <h1>Listado de Tareas</h1>
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
                                    <asp:GridView ID="dgv_Tareas" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None" CssClass="table-responsive table-striped table-bordered table-hover" AutoGenerateColumns="False" OnSelectedIndexChanged="dgv_Tareas_SelectedIndexChanged" Width="100%">
                                        <%--<asp:GridView ID="dgv_Tareas" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None"  class="table table-condensed table-bordered table-hover" AutoGenerateColumns="False" OnSelectedIndexChanged="dgv_Tareas_SelectedIndexChanged" Width="100%">--%>
                                        <Columns>
                                            <asp:CommandField ButtonType="Image" SelectImageUrl="~/Img/modificar.png" SelectText=" " ShowSelectButton="True"  />
                                            <asp:BoundField DataField="Id_RegTareas" HeaderText="ID." Visible="true" />
                                            <asp:BoundField DataField="Id_CompAranda" HeaderText="ID. ARANDA" />
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
                                            <asp:TemplateField HeaderText="EstadoTarea" SortExpression="">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" Text='<%# Bind("EstadoTarea") %>' ID="labelEstadoTarea" Style="padding: 5px; display: block;"></asp:Label>
                                                </ItemTemplate>
                                                <ControlStyle CssClass="alert alert-info text-center" />
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="Categoria" HeaderText="Categoria" Visible="true" />
                                            <asp:BoundField DataField="SubCategoria" HeaderText="Sub-Categoria" Visible="true" />
                                            <asp:BoundField DataField="EstadoApro" HeaderText="Estado de Aprobacion" Visible="true" />
                                            <asp:BoundField DataField="Estado" HeaderText="Estado ARANDA" Visible="true" />

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
                                                        <div class="col-lg-6">
                                                            <h6>
                                                                <asp:Label ID="lbl_CambioEstado" Text="Cambio de Estado" runat="server" CssClass="text-center"></asp:Label></h6>
                                                                <asp:DropDownList ID="dbl_Estado" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True" OnSelectedIndexChanged="dbl_Estado_SelectedIndexChanged1"></asp:DropDownList>
                                                        </div>
                                                        <div class="col-lg-6">
                                                            <h6>
                                                                <asp:Label ID="LabelTareaActual" Text="Estado Actual:" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        
                                                                <asp:Label ID="LabelTareaActualText" Text="" runat="server" CssClass="alert alert-warning text-center" Style="padding: 5px;"></asp:Label>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-12">
                                                        <div class="col-lg-5">
                                                            <h6>
                                                                <asp:Label ID="lbl_Comentario" Text="Detalle de la actividad " runat="server" CssClass="text-center" Visible="False"></asp:Label></h6>
                                                            <asp:TextBox ID="txt_Comentario" runat="server" CssClass="form-control tam-text-box" Enabled="False" TextMode="MultiLine" Visible="False" />
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-12" style="text-align: center;">
                                                            <asp:Button ID="btn_Guardar" runat="server" Text="Guardar" OnClick="btn_Guardar_Click" Enabled="False"  CssClass="btn btn-primary" Visible="False" />
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
                                                    <%--<asp:DropDownList ID="dbl_Estado" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True" OnTextChanged="dbl_TipoFactu_TextChanged"></asp:DropDownList>
                                                        <asp:DropDownList ID="dbl_Estado" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True"></asp:DropDownList>--%>
                                                    <asp:TextBox ID="txt_Estado" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>

                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-lg-12">

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lbl_Cliente" Text="Compañia" runat="server" CssClass="text-center"></asp:Label></h6>
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
                                        <div class="row" runat="server" id="Parte0" visible="true">
                                            <div class="col-lg-12">

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblCliente" Text="Nom. Cliente" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtcliente" runat="server" CssClass="form-control tam-text-box" Enabled="false" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblCategoria" Text="Categria" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtCategoria" runat="server" CssClass="form-control tam-text-box" Enabled="false" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblServicio" Text="Servicio" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtServicio" runat="server" CssClass="form-control tam-text-box" Enabled="false" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblGrupo" Text="Grupo" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtGrupo" runat="server" CssClass="form-control tam-text-box" Enabled="false" />
                                                </div>

                                            </div>
                                        </div>
                                        <div class="row" runat="server" id="Parte2" visible="true">
                                            <div class="col-lg-12">

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblEstadoAranda" Text="Estado de Aranda" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtEstadoAranda" runat="server" CssClass="form-control tam-text-box" Enabled="false" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblImpacto" Text="Impacto" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtImpacto" runat="server" CssClass="form-control tam-text-box" Enabled="false" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblUrgencia" Text="Urgencia" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtUrgencia" runat="server" CssClass="form-control tam-text-box" Enabled="false" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblPrieoridad" Text="Prioridad" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtPrioridad" runat="server" CssClass="form-control tam-text-box" Enabled="false" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row" runat="server" id="Parte1" visible="true">
                                            <div class="col-lg-12">
                                                <div class="col-lg-6">
                                                    <h6>
                                                        <asp:Label ID="lblDescripcionCaso" Text="Descripción" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtDescripcionCaso" runat="server" CssClass="form-control tam-text-box" Enabled="false" TextMode="MultiLine" />
                                                </div>
                                                <div class="col-lg-6">
                                                    <h6>
                                                        <asp:Label ID="lblProyecto" Text="Proyecto" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtProyecto" runat="server" CssClass="form-control tam-text-box" Enabled="false" TextMode="MultiLine" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row" runat="server" id="Parte3" visible="true">
                                            <div class="col-lg-12">
                                                <div class="col-lg-12">
                                                    <h6>
                                                        <asp:Label ID="lblDetalleCaso" Text="Detalle" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtDetalleCaso" runat="server" CssClass="form-control tam-text-box" Enabled="false" TextMode="MultiLine" Rows="10" />
                                                </div>

                                            </div>
                                        </div>
                                        <%--                                        <div class="row">
                                            <div class="col-lg-12">

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblApertura" Text="Apertura" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtApertura" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblAtencionReal" Text="Atencion Real" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtAtencionReal" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblAtencionEstimada" Text="Atencion Estimada" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtAtencionEstimada" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>

                                                <div class="col-lg-3">
                                                    <h6>
                                                        <asp:Label ID="lblExpira" Text="Expira" runat="server" CssClass="text-center"></asp:Label></h6>
                                                    <asp:TextBox ID="txtExpira" runat="server" CssClass="form-control tam-text-box" Enabled="true" />
                                                </div>

                                            </div>
                                        </div>--%>
                                        <div class="row" runat="server" id="Parte4" visible="true">
                                            <div class="col-lg-12">

                                                <div class="col-lg-3">
                                                    <asp:TextBox ID="txt_IdRegistro" runat="server" CssClass="form-control tam-text-box" Enabled="true" Visible="false" />
                                                </div>


                                            </div>
                                        </div>
                                        <%--                                            <div class="panel panel-default">
                                                <div class="panel-body">
                                                    <div class="col-lg-12">

                                                        <div class="col-lg-3">
                                                            <h6><asp:Label ID="lbl_Os" Text="N° Orden de Servicio" runat="server" CssClass="text-center"></asp:Label></h6>
                                                            <asp:TextBox ID="txt_Os" runat="server" cssClass="form-control tam-text-box" Enabled="true"/>
                                            
                                                        </div>
                                                                        
                                                        <div class="col-lg-3">
                                                                <h6><asp:Label ID="lbl_Cliente" Text="Nom. Cliente" runat="server" CssClass="text-center"></asp:Label></h6>
                                                                <asp:TextBox ID="txt_Cliente" runat="server" cssClass="form-control tam-text-box"/>
                                                        </div>

                                                        <div class="col-lg-3">
                                                                <h6><asp:Label ID="Label1" Text="Localidad" runat="server" CssClass="text-center"></asp:Label></h6>
                                                                <asp:TextBox ID="TextBox1" runat="server" cssClass="form-control tam-text-box"/>
                                                        </div>

                                                        <div class="col-lg-3">
                                                                <h6><asp:Label ID="Label2" Text="Localidad" runat="server" CssClass="text-center"></asp:Label></h6>
                                                                <asp:TextBox ID="TextBox2" runat="server" cssClass="form-control tam-text-box"/>
                                                        </div>

                                                    </div>
                                                </div>
                                            </div>--%>
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



<%--<asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True"></asp:DropDownList>--%>

