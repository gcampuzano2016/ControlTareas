﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ReporteFacturacion.aspx.cs" Inherits="ReporteTareas.Formulario.ReporteFacturacion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

            <div id="page-wrapper">



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
                                                <div class="col-lg-1">
                                                    </div>
                                                <div class="col-lg-11">
                                                    
                                                    <iframe width="800" height="600" src="https://app.powerbi.com/view?r=eyJrIjoiZDNhZTRkOGItZTMzMS00MDllLWEyNDYtOGExZjM1NzNhNTJlIiwidCI6IjgwODc0OTA5LTMzMjUtNDEzMS1iNmFiLTdiMDVjZDEwNmJhZSIsImMiOjR9&pageName=ReportSection" frameborder="0" allowFullScreen="true"></iframe>
                                                                                                        
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