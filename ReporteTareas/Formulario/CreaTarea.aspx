<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="CreaTarea.aspx.cs" Inherits="ReporteTareas.Formulario.CreaTarea" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

        <div id="page-wrapper" style="padding: 0px">


                <div class="row">
                    <div class="col-lg-12" style="padding: 20px">

                            <div class="panel-body">

                                <div class="row">
                                    <h3>Creación de Tareas Administrativas</h3>
                                </div>
                            </div>
          
                    </div>
                </div>



            <asp:Panel ID="Panel2" runat="server" Visible="true" Enabled="true">


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

                                                    <div class="col-lg-4">
                                                        <h6><asp:Label ID="lbl_Os" Text="Categoría de tarea interna DOS" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:DropDownList ID="dbl_OS" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True" Enabled="true"></asp:DropDownList>
                                                        <%--<asp:TextBox ID="txt_Os" runat="server" cssClass="form-control tam-text-box" Enabled="true" TextMode="Number" />--%>
                                                    </div>

                                                    <div class="col-lg-4">
                                                        <h6><asp:Label ID="lbl_Cliente" Text="Nom. Cliente" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_Cliente" runat="server" cssClass="form-control tam-text-box" Enabled="true"/>
                                                    </div>


                                                    <div class="col-lg-4">
                                                        <h6><asp:Label ID="lbl_FchRegistro" Text="Fecha Registro" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_FchRegistro" runat="server" cssClass="form-control tam-text-box" Enabled="true" />
                                                    </div>


                                                </div>

                                            </div>


                                            <div class="row">
                                                <div class="col-lg-12">

                                                    <%--<div class="col-lg-1">
                                                        </div>--%>


                                                    <div class="col-lg-6">

                                                        <h6><asp:Label ID="lbl_SubCategoria" Text="Sub-Categoria de la tarea" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:DropDownList ID="dbl_SubCategoria" runat="server" CssClass="form-control tam-txtbox-combo" AutoPostBack="True" Enabled="true"></asp:DropDownList>

                                                    </div>

                                                  <%--  <div class="col-lg-6">
                                                        <h6><asp:Label ID="lbl_Categoria" Text="Categoria de la tarea" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_Categoria"  runat="server" cssClass="form-control tam-text-box" Enabled="true" />
                                                    </div>--%>


                                                    <%--<div class="col-lg-1">
                                                        </div>--%>
                                                </div>

                                            </div>

                                            <div class="row">
                                                <div class="col-lg-12">


                                                    <div class="col-lg-6">
                                                        <h6><asp:Label ID="lbl_DetTarea" Text="Detalle de la Tarea" runat="server" CssClass="text-center"></asp:Label></h6>
                                                        <asp:TextBox ID="txt_DetCamEstado" runat="server" cssClass="form-control tam-text-box" Height="60px" TextMode="MultiLine" MaxLength="100" Enabled="true" />
                                                    </div>
                                                    <div class="col-lg-1">
                                                        </div>
                                                </div>

                                                    <div class="col-lg-6">

                                                    </div>

                                                </div>
                                            </div>


                                    <div class="row">
                                        <div class="col-lg-12">
                                            <br />
                                        </div>
                                    </div>

                                            <div class="row">
                                                                
                                                <div class="col-lg-12 col-md-12 col-sm-12">
                                                    <div class="col-lg-4">
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <%--<asp:Button ID="btn_Guardar" runat="server" Text="Guardar" OnClick="btn_Guardar_Click" Enabled="false" />--%>
                                                        <asp:Button ID="btn_Guardar" runat="server" Text="Guardar" Enabled="true" OnClick="btn_Guardar_Click" ControlStyle-CssClass ="btn btn-primary" />
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





                <%--///////////////////////////////////////////////////////////////////////////////////--%>

            </asp:Panel>

        </div>





</asp:Content>
