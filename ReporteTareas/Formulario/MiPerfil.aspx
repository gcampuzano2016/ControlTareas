<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="MiPerfil.aspx.cs" Inherits="ReporteTareas.Formulario.MiPerfil" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/miPerfil.js?v=2" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Mi perfil</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="row" style="padding: 0 15px">

            <!-- ----------------------------------------------- barra lateral -->
            <div class="col-lg-3">
                <div class="panel panel-default">
                    <div class="panel-body text-center">
                        <div id="perfilAvatar"
                             style="width: 96px; height: 96px; margin: 0 auto 12px; border-radius: 50%;
                                    background: #750202; color: #fff; font-size: 34px; line-height: 96px;">–</div>
                        <h4 id="perfilNombre" style="margin: 0 0 4px">–</h4>
                        <p id="perfilCargo" class="text-muted" style="margin: 0 0 10px">–</p>
                        <span id="perfilArea" class="label label-primary">–</span>
                        <span id="perfilCiudad" class="label label-default">–</span>
                        <hr />
                        <p style="margin: 0"><b id="perfilEdad">–</b><br /><small class="text-muted">Edad</small></p>
                    </div>
                </div>

                <!-- Aviso para los 119 usuarios sin ficha de empleado enlazada. -->
                <div id="perfilSinFicha" class="alert alert-warning" style="display: none; font-size: 12px">
                    <i class="fa fa-info-circle"></i>
                    Algunos datos administrados por Talento Humano todavía no están
                    asociados a tu usuario. Puedes usar el resto del perfil con normalidad.
                </div>

                <!-- Caso distinto y mas grave: el codigo de usuario esta repetido en
                     R_Usuarios y no se puede saber cual de las dos personas eres. El
                     procedimiento devuelve cero filas a proposito, antes que arriesgarse
                     a mostrarte los datos de otra persona. Afecta a 4 usuarios activos. -->
                <div id="perfilNoIdentificado" class="alert alert-danger" style="display: none; font-size: 12px">
                    <i class="fa fa-exclamation-triangle"></i>
                    No pudimos identificar tu perfil de forma única: tu código de usuario
                    está repetido en el sistema. Escribe a Talento Humano para que lo corrijan.
                </div>
            </div>

            <!-- --------------------------------------------------- contenido -->
            <div class="col-lg-9">
                <ul class="nav nav-tabs" role="tablist">
                    <li class="active"><a href="#tabPersonal" data-toggle="tab"><i class="fa fa-lock"></i> Datos personales</a></li>
                    <li><a href="#tabContacto" data-toggle="tab"><i class="fa fa-envelope"></i> Contacto y domicilio</a></li>
                    <li><a href="#tabEmergencia" data-toggle="tab"><i class="fa fa-ambulance"></i> Emergencia</a></li>
                </ul>

                <div class="tab-content" style="padding-top: 15px">

                    <div class="tab-pane active" id="tabPersonal">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Información personal
                                <span class="label label-default pull-right">
                                    <i class="fa fa-lock"></i> Gestionado por Talento Humano
                                </span>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="form-group col-lg-6"><label>Nombres completos</label>
                                        <p class="form-control-static" id="dpNombre">–</p></div>
                                    <div class="form-group col-lg-6"><label>Número de cédula</label>
                                        <p class="form-control-static" id="dpCedula">–</p></div>
                                    <div class="form-group col-lg-6"><label>Fecha de nacimiento</label>
                                        <p class="form-control-static" id="dpFnac">–</p></div>
                                    <div class="form-group col-lg-6"><label>Cargo</label>
                                        <p class="form-control-static" id="dpCargo">–</p></div>
                                    <div class="form-group col-lg-6"><label>Área</label>
                                        <p class="form-control-static" id="dpArea">–</p></div>
                                    <div class="form-group col-lg-6"><label>Jefe inmediato</label>
                                        <p class="form-control-static" id="dpJefe">–</p></div>
                                    <div class="form-group col-lg-6"><label>Ciudad</label>
                                        <p class="form-control-static" id="dpCiudad">–</p></div>
                                    <div class="form-group col-lg-6"><label>Correo de notificación</label>
                                        <p class="form-control-static" id="dpCorreo">–</p></div>
                                    <div class="form-group col-lg-6"><label>Horario</label>
                                        <p class="form-control-static" id="dpHorario">–</p></div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tab-pane" id="tabContacto">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Contacto y domicilio
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="form-group col-lg-12"><label>Dirección de domicilio</label>
                                        <input type="text" class="form-control" id="inDireccion" maxlength="400" /></div>
                                    <div class="form-group col-lg-6"><label>Correo personal</label>
                                        <input type="email" class="form-control" id="inCorreoPersonal" maxlength="150" /></div>
                                    <div class="form-group col-lg-6"><label>Número de teléfono</label>
                                        <input type="tel" class="form-control" id="inTelefonoPersonal" maxlength="50" /></div>
                                    <div class="form-group col-lg-6"><label>Estado civil</label>
                                        <select class="form-control" id="inEstadoCivil">
                                            <option value="">Seleccione…</option>
                                            <option>Soltero/a</option><option>Casado/a</option>
                                            <option>Unión de hecho</option><option>Divorciado/a</option>
                                            <option>Viudo/a</option>
                                        </select></div>
                                </div>
                                <button type="button" class="btn btn-primary" onclick="GuardarContacto()">
                                    <i class="fa fa-save"></i> Guardar cambios
                                </button>
                            </div>
                        </div>
                    </div>
                    <div class="tab-pane" id="tabEmergencia"></div>

                </div>
            </div>
        </div>
    </div>
</asp:Content>
