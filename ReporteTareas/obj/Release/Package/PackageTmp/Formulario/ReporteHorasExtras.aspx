<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ReporteHorasExtras.aspx.cs" Inherits="ReporteTareas.Formulario.ReporteHorasExtras1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
        <div class="row">
            <div class="col-lg-12">

                <div class="panel-body">

                    <div class="row">
                        <h1>Reporte Horas Extras</h1>
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

                                <div class="col-lg-3 col-md-3 col-sm-3 col-xs-3">
                                    <div class="row">
                                        <div class="col-lg-12">
                                            <div class="row">
                                                <div class="col-lg-12">
                                                    <h6>
                                                        <asp:Label ID="lbl_FchIni" Text="Fecha Inicio" runat="server" CssClass="text-center"></asp:Label></h6>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-10 col-md-10 col-sm-10 col-xs-10" style="padding-left: 15px; padding-right: 1px;">
                                                    <asp:TextBox ID="TexfechaInicio" runat="server" CssClass="form-control tam-text-box" type="text"></asp:TextBox>
                                                </div>

                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="col-lg-3 col-md-3 col-sm-3 col-xs-3">
                                    <div class="row">
                                        <div class="col-lg-12">
                                            <div class="row">
                                                <div class="col-lg-12">
                                                    <h6>
                                                        <asp:Label ID="lbl_FchFin" Text="Fecha Fin" runat="server" CssClass="text-center"></asp:Label></h6>
                                                </div>
                                            </div>

                                            <div class="row">
                                                
                                                <div class="col-lg-10 col-md-10 col-sm-10 col-xs-10" style="padding-left: 15px; padding-right: 1px;">
                                                    <asp:TextBox ID="TexfechaFin" runat="server" CssClass="form-control tam-text-box" type="text"></asp:TextBox>
                                                </div>

                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="col-lg-3 col-md-3 col-sm-3 col-xs-3" runat="server" id="Combo" visible="false">
                                    <div class="row">
                                        <div class="col-lg-12">
                                            <h6>
                                                <asp:Label ID="Label1" Text="Empleados" runat="server" CssClass="text-center"></asp:Label></h6>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <asp:DropDownList ID="CboUsuarios" runat="server"></asp:DropDownList>
                                    </div>
                                </div>

                            </div>

                            <div class="row">
                                <div class="row" style="padding-top: 10px; padding-top: 10px;">
                                    <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align:center">
                                        <asp:Button ID="btn_consulta" runat="server" Text="Consultar" OnClick="btn_consulta_Click"  CssClass="btn btn-primary"/>
                                    </div>
                                    <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align:center">
                                        <asp:Button ID="btn_Descarga" runat="server" Text="Descargar" OnClick="btn_Descarga_Click"  CssClass="btn btn-primary" Visible="False"/>
                                    </div>
                                </div>

                            </div>
                            <div class="row">
                                <div class="row" style="padding-top: 10px; padding-top: 10px;">
                                    <div id="divMensajes" class="">
                                    </div>
                                </div>
                            </div>

                            <div class="row">
                                <br />
                            </div>
                            <div class="row">
                                <div class="col-lg-12 col-md-12 col-sm-12 col-xs-12">
                                    <asp:GridView ID="dgv_Tareas" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None" CssClass="table-responsive table-striped table-bordered table-hover" AutoGenerateColumns="False" Width="100%">
                                        <Columns>
                                            <asp:BoundField DataField="Nom_Cliente" HeaderText="Nom. Cliente" Visible="true" />
                                            <asp:BoundField DataField="Num_OrdenServicio" HeaderText="N° OS" Visible="true" />
                                            <asp:BoundField DataField="Id_Responsable" HeaderText="Id. Responsable" Visible="true" />
                                            <asp:BoundField DataField="Det_Tarea" HeaderText="Descripción Tarea" Visible="true" />
                                            <asp:BoundField DataField="Det_Fecha" HeaderText="Fch. Tarea" Visible="true" />
                                            <asp:BoundField DataField="Det_Fch_RegDetalleIni" HeaderText="H. Inicio" Visible="true" />
                                            <asp:BoundField DataField="Det_Fch_RegDetalleFin" HeaderText="H. Fin" Visible="true" />
                                            <asp:BoundField DataField="Det_Tiempo" HeaderText="Tiempo" Visible="true" />
                                            <asp:BoundField DataField="Det_Det_Tarea" HeaderText="Detalle Tarea" Visible="true" />
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

        <script>
            var divMensajesFinal = '';
            if(typeof divMensajesContenido_0 === "undefined"){
                var divMensajesContenido_0 = '';
            }
            if(typeof divMensajesContenido_1 === "undefined"){
                var divMensajesContenido_1 = '';
            }
            if(typeof divMensajesContenido_2 === "undefined"){
                var divMensajesContenido_2 = '';
            }
            if(typeof divMensajesContenido_3 === "undefined"){
                var divMensajesContenido_3 = '';
            }
            if(typeof divMensajesContenido_4 === "undefined"){
                var divMensajesContenido_4 = '';
            }
            var divMensajesContenido = [divMensajesContenido_0, divMensajesContenido_1, divMensajesContenido_2, divMensajesContenido_3, divMensajesContenido_4];
            divMensajesContenido.forEach(function(element) {
                divMensajesFinal = divMensajesFinal.concat(element);
            });
            document.getElementById("divMensajes").innerHTML = divMensajesFinal;
        </script>
    <script>
         $( function() {
        var dateFormat = "dd/mm/yy";

      from = $( "#ContentPlaceHolder1_TexfechaInicio" ).datepicker(
              {
                    dateFormat: dateFormat,
                    dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
                    dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
                    firstDay: 1,
                    gotoCurrent: true,
                    monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]

                })
        .on( "change", function() {
            to.datepicker("option", "minDate", getDate(this));
            $("#ContentPlaceHolder1_btn_Descarga").hide();
        }),
        to = $("#ContentPlaceHolder1_TexfechaFin").datepicker(
              {
                    dateFormat: dateFormat,
                    dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
                    dayNamesMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
                    firstDay: 1,
                    gotoCurrent: true,
                    monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Deciembre"]

                })
      .on( "change", function() {
            from.datepicker("option", "maxDate", getDate(this));
          $("#ContentPlaceHolder1_btn_Descarga").hide();
                     });

            $("#ContentPlaceHolder1_TexfechaInicio").datepicker('setDate', 'today');
            $("#ContentPlaceHolder1_TexfechaFin").datepicker('setDate', 'today');
 
    function getDate( element ) {
      var date;
      try {
        date = $.datepicker.parseDate( dateFormat, element.value );
      } catch( error ) {
        date = null;
      }
 
      return date;
    }
  } );


    </script>


        </asp:Panel>
    </div>
</asp:Content>
