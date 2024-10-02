<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CreaTarea.aspx.cs" Inherits="ReporteTareas.Formulario.CreaTarea" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

        <div id="page-wrapper">



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
                                                        <h6><asp:Label ID="lbl_Os" Text="N° Orden de Servicio" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_Os" runat="server" cssClass="form-control tam-text-box" Enabled="true"/>
                                                    </div>
                                                                
                                                    <div class="col-lg-3">
                                                        <h6><asp:Label ID="lbl_Ticket" Text="Cod. Ticket" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_Ticket" runat="server" cssClass="form-control tam-text-box" Enabled="true"/>
                                                    </div>

                                                    <div class="col-lg-3">
                                                        <h6><asp:Label ID="lbl_FchRegistro" Text="Fch. Registro" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_FchRegistro" runat="server" cssClass="form-control tam-text-box" Enabled="true" />
                                                    </div>

                                                    <div class="col-lg-3">
                                                        <h6><asp:Label ID="lbl_Estado" Text="Estado" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <%--<asp:DropDownList ID="dbl_Estado" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True" OnTextChanged="dbl_TipoFactu_TextChanged"></asp:DropDownList>
                                                        <asp:DropDownList ID="dbl_Estado" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True"></asp:DropDownList>--%>
                                                        <asp:TextBox ID="txt_Estado" runat="server" cssClass="form-control tam-text-box" Enabled="true"/>
                                                    </div>

                                                </div>

                                            </div>

                                            <div class="row">
                                                <div class="col-lg-12">

                                                    <div class="col-lg-3">
                                                        <h6><asp:Label ID="lbl_Cliente" Text="Nom. Cliente" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_Cliente" runat="server" cssClass="form-control tam-text-box" Enabled="true"/>
                                                    </div>
                                                                
                                                    <div class="col-lg-3">
                                                        <h6><asp:Label ID="lbl_NomResponsable" Text="Nom. Responsable" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_NomResponsable" runat="server" cssClass="form-control tam-text-box" Enabled="true"/>
                                                    </div>

                                                    <div class="col-lg-3">
                                                        <h6><asp:Label ID="lbl_FchTenInicio" Text="Fch. Inicio" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_FchTenInicio" runat="server" cssClass="form-control tam-text-box" Enabled="true"/>
                                                    </div>

                                                    <div class="col-lg-3">
                                                        <h6><asp:Label ID="lbl_FchTenFin" Text="Fch. Fin" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_FchTenFin" runat="server" cssClass="form-control tam-text-box" Enabled="true"/>
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
