<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="SolicitarVacacionesPermiso.aspx.cs" Inherits="ReporteTareas.Formulario.SolicitarVacacionesPermiso" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js"></script>

    <script src="../js/padFirma.js?v=2" type="text/javascript"></script>
    <script src="../js/feriadosVacaciones.js?v=1" type="text/javascript"></script>
    <script src="../js/Convenio.js?v=40" type="text/javascript"></script>

    <script src="../js/moment.min.js" type="text/javascript"></script>
    <script src="../js/moment-with-locales.min.js" type="text/javascript"></script>
    <script src="../js/bootstrap-datetimepicker.js" type="text/javascript"></script>
    <script src="../js/jquery.blockUI.js" type="text/javascript"></script>
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Solicitud de Vacaciones y Permisos</h3>
                    </div>
                </div>
            </div>
        </div>
        <asp:Panel ID="Panel5" runat="server" Visible="true" Enabled="true">
            <div class="col-lg-12" style="padding: 0px">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Lista de solicitud
						<div style="display: none">
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                            <asp:TextBox ID="txtIdTipo" runat="server" CssClass="form-control tam-text-box" Enabled="False" Visible="true" />
                        </div>
                    </div>
                    <div class="panel-body">
                        <!-- Nav tabs -->
                        <ul class="nav nav-pills">
                            <!--<li class="active" id="tab1"><a href="#home-pills" data-toggle="tab">Ingreso de Solicitud</a>
                            </li>-->
                        </ul>
                        <!-- Tab panes -->
                        <div class="tab-content">
                            <div class="tab-pane fade in active" id="home-pills">
                                <div class="panel panel-default">
                                    <div class="panel-body">
                                        <div class="row">
                                            <div id="RegistroVacaciones" style="display: none">
                                                <div class="modal-header">
                                                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios1()">&times;</button>
                                                    <h4 class="modal-title" id="fomTitleLabel">Convenio de vacaciones</h4>
                                                </div>
                                                <div class="modal-body">
                                                    <div class="panel panel-default">
                                                        <div class="panel-body">
                                                            <div class="col-lg-12">
                                                                <div class="form-group col-lg-6">
                                                                    <label>Fecha:</label>
                                                                    <input type="text" class="form-control" id="txtfechaV">
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Cédula:</label>
                                                                    <input type="text" class="form-control" id="txtCedulaV" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Nombre del Colaborador:</label>
                                                                    <input type="text" class="form-control" id="txtNombreColaboradorV" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Departamento:</label>
                                                                    <input type="text" class="form-control" id="txtDepartamentoV" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-12">
                                                                    <label>Jefe inmediato:</label>
                                                                    <input type="text" class="form-control" id="txtJefaAreaV" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-12">
                                                                    <label>Reemplazo:</label>
                                                                    <select id="txtReemplazo" class="js-example-basic-multiple" name="mercado[]" multiple="multiple" onchange="CargarTodoEmpleados()">
                                                                    </select>
                                                                    <!--<input type="text" class="form-control" id="txtReemplazo">-->
                                                                </div>
                                                                <div class="form-group col-lg-12">
                                                                    <div class="col-lg-3">
                                                                        <label>Fecha Desde:</label>
                                                                        <input type="text" class="form-control" id="frmTxtHoraDesdeV" onchange="DiasVacaciones()">
                                                                        <p class="help-block" id="frmTxtHoraDesdeMsg"></p>
                                                                    </div>
                                                                    <div class="col-lg-3">
                                                                        <label>Fecha Hasta:</label>
                                                                        <input type="text" class="form-control" id="frmTxtHoraHastaV" onchange="DiasVacaciones()">
                                                                        <p class="help-block" id="frmTxtHoraHastaMsg"></p>
                                                                    </div>
                                                                    <div class="col-lg-3">
                                                                        <label>Dias:</label>
                                                                        <input type="text" class="form-control" id="frmTxtTiempoDiasV" value="1" disabled>
                                                                        <!-- Los feriados ya no se digitan: salen de la tabla Feriado. Se
                                                                             muestran acá cuando hay, para que el número de días se
                                                                             entienda sin tener que preguntar. -->
                                                                        <p class="help-block" id="frmTxtFeriadosMsg"></p>
                                                                        <input type="hidden" id="frmTxtTiempoF" value="0">
                                                                    </div>
                                                                    <div class="col-lg-3">
                                                                        <label>Regresa a trabajar:</label>
                                                                        <input type="text" class="form-control" id="frmTxtRegresaTrabajar" disabled>
                                                                        <p class="help-block">Día siguiente a Fecha Hasta</p>
                                                                    </div>
                                                                </div>
                                                                <div class="col-lg-12">
                                                                    <label style="color: red;" id="Idlista1">---</label>
                                                                    <label style="color: red;" id="Idlista2">---</label>
                                                                </div>
                                                                <!-- Aviso de saldo insuficiente. No bloquea el envío: informa. -->
                                                                <div class="col-lg-12">
                                                                    <div id="IdAvisoSaldo" class="alert alert-warning" style="display: none; margin-bottom: 10px;"></div>
                                                                </div>
                                                                <div class="form-group col-lg-3">
                                                                    <label>Días de vacaciones dispobibles</label>
                                                                </div>
                                                                <div class="form-group col-lg-12">
                                                                    <!-- /.panel-heading -->
                                                                    <div class="panel-body" style="height: 200px; overflow-y: auto; overflow-x: auto;">
                                                                        <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                                            <div class="row">
                                                                                <div class="col-sm-12" id='datosTablaPrincipal2' style="padding: 0px">
                                                                                </div>
                                                                                <!-- /.table-responsive -->
                                                                                <!-- /.panel-body -->
                                                                            </div>
                                                                            <!-- /.panel -->
                                                                        </div>
                                                                        <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align: center">
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <!-- Firma del colaborador. Nombre, cargo y cedula los toma el
                                                                     sistema de la sesion: no se digitan. -->
                                                                <div class="form-group col-lg-6">
                                                                    <label>Su firma: <span style="color:#a94442">*</span></label>
                                                                    <div id="divFirmaColaborador"></div>
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <div class="" style="text-align: center">
                                                                    <button id="btnGuardar1" onclick="GuardarSolicitud1()" type="button" class="btn btn-primary">Solicitar</button>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div id="RegistroPermisos" style="display: none">
                                                <div class="modal-header">
                                                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true" onclick="CancelarCambios2()">&times;</button>
                                                    <h4 class="modal-title" id="fomTitleLabel">Convenio de permisos en horas laborales</h4>
                                                </div>
                                                <div class="modal-body">
                                                    <div class="panel panel-default">
                                                        <div class="panel-body">
                                                            <div class="col-lg-12">
                                                                <div class="form-group col-lg-6">
                                                                    <label>Cédula:</label>
                                                                    <input type="text" class="form-control" id="txtCedulaP" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Nombre del Colaborador:</label>
                                                                    <input type="text" class="form-control" id="txtNombreColaboradorP" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Departamento:</label>
                                                                    <input type="text" class="form-control" id="txtDepartamentoP" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-6">
                                                                    <label>Jefe de Area:</label>
                                                                    <input type="text" class="form-control" id="txtJefaAreaP" disabled>
                                                                </div>
                                                                <div class="form-group col-lg-12">
                                                                    <div class="form-group col-lg-3">
                                                                        <label>Tipo de Permiso</label>
                                                                        <select id="txtActividadPC" class="form-control" onchange="AgregaActividad()">
                                                                            <option value="PERSONAL">Personal</option>
                                                                            <option value="MEDICO">Médico</option>
                                                                            <option value="FAMILIAR">Familiar</option>
                                                                            <option value="CALAMIDAD">Calamidad</option>
                                                                            <option value="TELETRABAJO">Teletrabajo</option>
                                                                            <option value="OTRO">Otro</option>
                                                                        </select>
                                                                        <p class="help-block"></p>
                                                                    </div>
                                                                    <div class="form-group col-lg-9" style="display: none" id="IdOtrasActividad">
                                                                        <label>Actividad a realizar:</label>
                                                                        <input type="text" class="form-control" id="txtActividadP">
                                                                    </div>
                                                                    <!-- Respaldo adjunto. Decide si se pide archivo. No aparece en
                                                                         Médico, donde el adjunto es obligatorio, ni en Teletrabajo,
                                                                         que no usa el campo genérico. -->
                                                                    <div class="form-group col-lg-3" style="display: none" id="IdRespaldo">
                                                                        <label>Respaldo adjunto</label>
                                                                        <select id="cboRespaldo" class="form-control" onchange="CambiaRespaldoAdjunto()">
                                                                            <option value="NO_APLICA">No aplica</option>
                                                                            <option value="SI">Sí</option>
                                                                            <option value="NO">No</option>
                                                                        </select>
                                                                    </div>
                                                                    <!-- Rama de Teletrabajo. Aparece solo con ese tipo. -->
                                                                    <div class="col-lg-12" style="display: none" id="IdTeletrabajo">
                                                                        <div class="panel panel-default" style="margin-top:10px">
                                                                            <div class="panel-heading">Teletrabajo</div>
                                                                            <div class="panel-body">
                                                                                <!-- La fecha vive acá cuando el permiso es teletrabajo, y no arriba con
                                                                                     las horas: abierto este panel, aquel campo queda demasiado lejos para
                                                                                     leerse como la fecha del teletrabajo.
                                                                                
                                                                                     Es la misma fecha, no una segunda. Lo que se escribe acá se copia a
                                                                                     txtfechaP, que sigue siendo el valor que viaja al guardar y del que
                                                                                     dependen el saldo mensual, el plazo de recuperación y el PDF. -->
                                                                                <div class="form-group col-lg-4">
                                                                                    <label>Fecha del teletrabajo <span style="color:#a94442">*</span></label>
                                                                                    <input type="text" class="form-control" id="txtTTFecha" onchange="CambiaFechaTeletrabajo()">
                                                                                    <p class="help-block">El día en que trabaja fuera de la oficina.</p>
                                                                                </div>
                                                                                <div class="form-group col-lg-4">
                                                                                    <label>Modalidad</label>
                                                                                    <select id="cboModalidadTT" class="form-control" onchange="CambiaModalidadTeletrabajo()">
                                                                                        <option value="COMPLETA">Jornada completa</option>
                                                                                        <option value="HORAS">Por horas</option>
                                                                                    </select>
                                                                                </div>
                                                                                <!-- Solo con modalidad por horas. Despues de esa hora la
                                                                                     persona se presenta de forma presencial. -->
                                                                                <div class="form-group col-lg-4" style="display: none" id="IdTTHoras">
                                                                                    <label>Hora Desde / Hasta</label>
                                                                                    <div class="row">
                                                                                        <div class="col-lg-6">
                                                                                            <input type="text" class="form-control" id="txtTTHoraDesde" placeholder="08:00" maxlength="5">
                                                                                        </div>
                                                                                        <div class="col-lg-6">
                                                                                            <input type="text" class="form-control" id="txtTTHoraHasta" placeholder="12:00" maxlength="5">
                                                                                        </div>
                                                                                    </div>
                                                                                    <p class="help-block">Después de esa hora se presenta de forma presencial.</p>
                                                                                </div>
                                                                                <div class="form-group col-lg-4">
                                                                                    <label>Lugar desde donde trabajará</label>
                                                                                    <input type="text" class="form-control" id="txtTTLugar" maxlength="200" placeholder="Domicilio — Quito">
                                                                                </div>
                                                                                <div class="form-group col-lg-6">
                                                                                    <label>Medios de contacto</label>
                                                                                    <input type="text" class="form-control" id="txtTTMedios" maxlength="200" placeholder="Teléfono / Teams">
                                                                                </div>
                                                                                <div class="form-group col-lg-6">
                                                                                    <label>Motivo general</label>
                                                                                    <input type="text" class="form-control" id="txtTTMotivo" maxlength="500">
                                                                                </div>
                                                                                <div class="form-group col-lg-6">
                                                                                    <label>Actividades a ejecutar</label>
                                                                                    <textarea class="form-control" id="txtTTActividades" rows="2" maxlength="1000"></textarea>
                                                                                </div>
                                                                                <div class="form-group col-lg-6">
                                                                                    <label>Entregables esperados</label>
                                                                                    <textarea class="form-control" id="txtTTEntregables" rows="2" maxlength="1000"></textarea>
                                                                                </div>
                                                                                <div class="form-group col-lg-12">
                                                                                    <label>
                                                                                        <input type="checkbox" id="chkTTConectividad">
                                                                                        Confirmo conectividad y confidencialidad de la información
                                                                                    </label>
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                    <div class="form-group col-lg-9" style="display: block" id="IdOtrasActividadCargar">
                                                                        <form role="form">
                                                                            <div class="col-lg-8">
                                                                                <label>Adjuntar Archivos:</label>
                                                                                <input type="file" id="archivosAdjuntos" multiple />
                                                                                <p class="help-block"></p>
                                                                                <progress id="fileProgress" style="display: none"></progress>
                                                                                <hr />
                                                                                <span id="lblMessage" style="color: Green"></span>
                                                                            </div>
                                                                            <div class="col-lg-4" style="display: none" id="IdCargarArchivo">
                                                                                <br>
                                                                                <input type="button" id="btnCargarArchivosAdjuntos" value="Cargar Archivos" class="btn btn-info" />
                                                                                <p class="help-block"></p>
                                                                            </div>

                                                                            <div class="col-lg-12" id="divArchivosAdjuntosAnteriores">
                                                                            </div>
                                                                            <div class="col-lg-12" id="divArchivosAdjuntos">
                                                                            </div>
                                                                            <div class="col-lg-12">
                                                                                <p class="help-block" id="messageNotify" style="display: none;"></p>
                                                                            </div>
                                                                        </form>
                                                                    </div>
                                                                </div>
                                                                <!-- La fecha del permiso vive acá, junto a las horas, y no arriba entre
                                                                     los datos autocompletados: ahí se leía como la fecha del documento.
                                                                     Es el día de la ausencia, y de él depende de qué mes sale el saldo
                                                                     del permiso mensual. Por eso al cambiarla se vuelve a consultar. -->
                                                                <div class="col-lg-3" id="IdFechaPermiso">
                                                                    <label>Fecha del permiso:</label>
                                                                    <input type="text" class="form-control" id="txtfechaP" onchange="ConsultarSaldoMensual(); MostrarPlazoRecuperacion()">
                                                                    <p class="help-block">El día en que se ausenta.</p>
                                                                </div>
                                                                <div class="col-lg-3" id="IdHoraDesdeP">
                                                                    <label>Hora Desde:</label>
                                                                    <input type="text" class="form-control" id="frmTxtHoraDesdeP">
                                                                    <p class="help-block" id="frmTxtHoraDesdePMsg"></p>
                                                                </div>
                                                                <div class="col-lg-3" id="IdHoraHastaP">
                                                                    <label>Hora Hasta:</label>
                                                                    <input type="text" class="form-control" id="frmTxtHoraHastaP">
                                                                    <p class="help-block" id="frmTxtHoraHastaPMsg"></p>
                                                                </div>
                                                                <div class="col-lg-3" id="IdTiempoP">
                                                                    <label>Tiempo:</label>
                                                                    <input type="text" class="form-control" id="frmTxtTiempoP" disabled>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                                <!-- Permiso mensual de 3 horas. El saldo se consulta al abrir el
                                                                     formulario y cada vez que cambia la fecha: la bolsa es del mes
                                                                     en que la persona se ausenta, no del mes en que lo pide.

                                                                     No aplica a Médico, Calamidad ni Teletrabajo. -->
                                                                <div class="form-group col-lg-6" id="IdPermisoMensual" style="display: none">
                                                                    <label>
                                                                        <input type="checkbox" id="chkPermisoMensual" onchange="CambiaPermisoMensual()">
                                                                        ¿Usa permiso mensual de 3 horas?
                                                                    </label>
                                                                    <p class="help-block" id="msgPermisoMensual">Consultando su saldo del mes...</p>
                                                                </div>
                                                                <!-- El tratamiento del excedente solo tiene sentido si hay excedente:
                                                                     se oculta cuando el permiso entra completo en la bolsa mensual. -->
                                                                <div class="form-group col-lg-6" id="IdExcedente" style="display: none">
                                                                    <label>Tratamiento del excedente</label>
                                                                    <select id="cboExcedente" class="form-control" onchange="CambiaTratamientoExcedente()">
                                                                        <option value="">-- Seleccione --</option>
                                                                        <option value="VACACIONES">Vacaciones</option>
                                                                        <option value="RECUPERACION">Recuperación</option>
                                                                        <option value="SIN_REMUNERACION">Permiso sin remuneración</option>
                                                                    </select>
                                                                    <p class="help-block" id="msgExcedente"></p>
                                                                    <!-- Las casillas viejas siguen existiendo, ocultas: el guardado y
                                                                         la columna CargoVacaciones se alimentan de ellas, y así el
                                                                         histórico conserva el mismo significado. Las mantiene
                                                                         sincronizadas CambiaTratamientoExcedente(). -->
                                                                    <input type="checkbox" id="IdSI" style="display: none">
                                                                    <input type="checkbox" id="IdNO" style="display: none">
                                                                </div>
                                                                <!-- Plan de recuperacion. Aparece solo cuando el excedente se trata
                                                                     asi. Reemplaza a un bloque anterior que estaba muerto: era una
                                                                     tabla de N filas copiada de un widget de formas de pago, sin los
                                                                     campos que pide la especificacion, y nunca se mostraba. -->
                                                                <div class="col-lg-12" style="display: none" id="IdRecuperacion">
                                                                    <div class="panel panel-default" style="margin-top:10px">
                                                                        <div class="panel-heading">Plan de recuperación</div>
                                                                        <div class="panel-body">
                                                                            <div class="form-group col-lg-4">
                                                                                <label>Fecha propuesta <span style="color:#a94442">*</span></label>
                                                                                <input type="text" class="form-control" id="txtRecFecha" placeholder="dd/mm/aaaa">
                                                                            </div>
                                                                            <div class="form-group col-lg-4">
                                                                                <label>Horario propuesto</label>
                                                                                <input type="text" class="form-control" id="txtRecHorario" maxlength="100" placeholder="08:00 a 10:00">
                                                                            </div>
                                                                            <div class="form-group col-lg-4">
                                                                                <label>Fecha máxima de cierre</label>
                                                                                <input type="text" class="form-control" id="txtRecMaxima" disabled>
                                                                                <p class="help-block">30 días desde el permiso.</p>
                                                                            </div>
                                                                            <div class="form-group col-lg-6">
                                                                                <label>Actividades a ejecutar</label>
                                                                                <textarea class="form-control" id="txtRecActividades" rows="2" maxlength="1000"></textarea>
                                                                            </div>
                                                                            <div class="form-group col-lg-6">
                                                                                <label>Entregables esperados</label>
                                                                                <textarea class="form-control" id="txtRecEntregables" rows="2" maxlength="1000"></textarea>
                                                                            </div>
                                                                            <div class="form-group col-lg-12">
                                                                                <p class="help-block">Pasada la fecha máxima, su jefe inmediato confirma si el tiempo se recuperó.</p>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                                <div class="col-lg-12">
                                                                    <label>Observaciones:</label>
                                                                    <textarea class="form-control" rows="2" cols="50" id="frmTxtObservacionesP">
																	</textarea>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <!-- Firma del colaborador, igual que en vacaciones. Nombre, cargo y
                                                                     cédula los toma el sistema de la sesión: no se digitan. -->
                                                                <div class="form-group col-lg-6">
                                                                    <label>Su firma: <span style="color:#a94442">*</span></label>
                                                                    <div id="divFirmaColaboradorP"></div>
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <div class="" style="text-align: center">
                                                                    <button id="btnGuardar2" onclick="GuardarSolicitud2()" type="button" class="btn btn-primary">Solicitar</button>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div id="ListaSolicitud" style="display: block">
                                                <div class="col-lg-12" style="padding: 0px">
                                                    <div class="panel panel-default">
                                                        <!--<div class="panel-heading">
														Registro de Solicitud
														</div>-->
                                                        <div class="panel-body">
                                                            <div class="row">
                                                                <div class="panel panel-default">
                                                                    <div class="panel-heading">
                                                                        <label>Tipo Solicitud</label>
                                                                    </div>
                                                                </div>
                                                                <div class="form-group col-lg-3">
                                                                    <select id="cboSolicitud" class="form-control" onchange="BuscarSolicitud()">
                                                                    </select>
                                                                    <p class="help-block"></p>
                                                                </div>
                                                                <!--
																<div class="form-group col-lg-3" id="idGerente" runat="server">
																	<label>Colaborador</label>
																	<input type="text" class="form-control" id="txtEmpledos">
																	<p class="help-block"></p>
																</div>																
																<div class="form-group col-lg-3">
																<button id="btnConsulta" onclick="BtnPermiso()" type="button" class="btn btn-primary">Permiso</button>
																<button id="btnDescarga" onclick="BtnVacaciones()" type="button" class="btn btn-primary">Vacaciones</button>
																</div>-->
                                                            </div>
                                                            <div class="row">
                                                                <div class="panel panel-default">
                                                                    <div class="panel-heading">
                                                                        <label>Consulta de la solicitud</label>
                                                                    </div>
                                                                </div>
                                                                <div class="form-group col-lg-2">
                                                                    <label>Fecha desde:</label>
                                                                    <input type="text" class="form-control" id="txtFechaConsulta1">
                                                                    <p class="help-block"></p>
                                                                </div>
                                                                <div class="form-group col-lg-2">
                                                                    <label>Fecha Hasta:</label>
                                                                    <input type="text" class="form-control" id="txtFechaConsulta2">
                                                                    <p class="help-block"></p>
                                                                </div>
                                                                <div class="form-group col-lg-2">
                                                                    <label>Filtrar sin fecha</label>
                                                                    <input class="form-check-input" type="checkbox" value="" id="idFiltros">
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div class="panel-footer">
                                                            <div class="" style="text-align: center">
                                                                <button id="btnConsulta" onclick="BtnConsulta()" type="button" class="btn btn-primary">Consultar</button>
                                                                <button id="btnDescarga" onclick="BtnDescarga()" type="button" class="btn btn-primary">Descargar XLS</button>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-12" style="padding: 0px">
                                                        <div class="panel panel-default">
                                                            <div class="panel-heading">
                                                                <div>
                                                                    <h4 class="" id="listPrincipalTitleLabel">Detalle de Solicitud</h4>
                                                                </div>
                                                            </div>
                                                            <!-- /.panel-heading -->
                                                            <div class="panel-body" style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                                                <div id="table-datosTablaPrincipal" class="dataTables_wrapper form-inline dt-bootstrap no-footer">
                                                                    <div class="row">
                                                                        <div class="col-sm-12" id='datosTablaPrincipal' style="padding: 0px">
                                                                        </div>
                                                                        <!-- /.table-responsive -->
                                                                        <!-- /.panel-body -->
                                                                    </div>
                                                                    <!-- /.panel -->
                                                                </div>
                                                                <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4" style="text-align: center">
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
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Modal -->
        <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-labelledby="modalMensajeInformativoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeInformativoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="myModalLabel">Informativo</h4>
                    </div>
                    <div class="modal-body" id="MensajeInformativo">
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <!-- Modal -->
        <div class="modal fade" id="modalMensajeConfirmacion" tabindex="-1" role="dialog" aria-labelledby="modalMensajeConfirmacionLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeConfirmacionTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="myModalConfirmacionLabel">Confirmación</h4>
                    </div>
                    <div class="modal-body" id="modalMensajeConfirmacioMensaje">
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" id="txtCodigoItem" disabled style="visibility: hidden">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="EliminarArchivo()" class="btn btn-primary" id="btnBorrarArchivo" hidden>Continuar Borrado</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <!-- Modal -->
        <div class="modal fade" id="modalCargarProceso" tabindex="-1" role="dialog" aria-labelledby="modalCargarProcesoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalCargarProcesoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="mymodalCargarProcesoLabel">Anular Solicitud</h4>
                    </div>
                    <div class="modal-body" id="modalCargarProcesoMensaje">
                        <div class="form-group col-lg-12">
                            </br>
							<label>Estado</label>
                            <select id="cboEstado2" class="form-control">
                            </select>
                        </div>
                        <div class="form-group col-lg-12">
                            <label>Motivo:</label>
                            <textarea class="form-control" id="txtDescripcionReq2" name="txtDescripcionReq" rows="2" cols="50"></textarea>
                            <p class="help-block"></p>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <input type="text" class="form-control" id="txtCodigoItem" disabled style="visibility: hidden">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" onclick="ActualizarProceso()" class="btn btn-primary" id="btnBorrarArchivo" hidden>Guardar</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </div>
</asp:Content>
