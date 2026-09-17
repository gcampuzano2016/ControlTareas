<%@ Control Language="C#" AutoEventWireup="true" ClassName="PerfilFichas" %>

<%--
    Las fichas del perfil: datos personales, contacto, emergencia, formacion,
    experiencia, cargas familiares y equipo.

    Vive aca y no dentro de una pagina porque lo usan DOS pantallas: MiPerfil.aspx
    -el perfil propio- y PerfilesPersonal.aspx -el de otra persona, para Talento
    Humano-. Duplicar 498 lineas de marcado era garantizar que al primer arreglo
    las dos se separaran sin que nadie se enterara.

    Los id de aqui adentro son los que busca miPerfil.js. No renombrar ninguno.

    La pestana Equipo esta en el control pero PerfilesPersonal.aspx no la dibuja:
    ListaEquipo toma al jefe de la SESION, asi que alli mostraria el equipo de
    quien mira con el nombre de otro en la cabecera.
--%>
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
                        <!-- Dos representaciones excluyentes: la foto si la hay,
                             y si no las iniciales, que es lo que la fase 1 dejo. -->
                        <img id="perfilFoto" alt="Foto de perfil"
                             style="display: none; width: 96px; height: 96px; margin: 0 auto 12px;
                                    border-radius: 50%; object-fit: cover" />
                        <div id="perfilAvatar"
                             style="width: 96px; height: 96px; margin: 0 auto 12px; border-radius: 50%;
                                    background: #750202; color: #fff; font-size: 34px; line-height: 96px;">–</div>
                        <h4 id="perfilNombre" style="margin: 0 0 4px">–</h4>
                        <p id="perfilCargo" class="text-muted" style="margin: 0 0 10px">–</p>

                        <!-- El input va oculto y lo dispara el boton: el control de
                             archivo nativo no se puede estilar y desentona. -->
                        <input type="file" id="inFoto" accept="image/jpeg,image/png" style="display: none" />
                        <p style="margin: 0 0 10px">
                            <button type="button" class="btn btn-default btn-xs" onclick="ElegirFoto()">
                                <i class="fa fa-camera"></i> Cambiar foto
                            </button>
                            <button type="button" id="btnQuitarFoto" class="btn btn-default btn-xs"
                                    style="display: none" onclick="QuitarFoto()">
                                <i class="fa fa-trash"></i> Quitar
                            </button>
                        </p>

                        <span id="perfilArea" class="label label-primary">–</span>
                        <span id="perfilCiudad" class="label label-default">–</span>
                        <hr />
                        <p style="margin: 0"><b id="perfilEdad">–</b><br /><small class="text-muted">Edad</small></p>
                        <hr />
                        <!-- Enlace y no llamada de JavaScript: la respuesta es un
                             archivo, no JSON, y un <a> con target es lo que el
                             navegador ya sabe manejar. -->
                        <a id="lnkHojaVida" href="DescargarPerfil.ashx?cv=1" target="_blank"
                           class="btn btn-primary btn-block btn-sm">
                            <i class="fa fa-file-pdf-o"></i> <span id="txtHojaVida">Descargar mi hoja de vida</span>
                        </a>
                    </div>
                </div>

                <!-- Aviso para los 119 usuarios sin ficha de empleado enlazada. -->
                <div id="perfilSinFicha" class="alert alert-warning" style="display: none; font-size: 12px">
                    <i class="fa fa-info-circle"></i>
                    Algunos datos administrados por Talento Humano todavía no están
                    asociados a su usuario. Puede usar el resto del perfil con normalidad.
                </div>

                <!-- Caso distinto y mas grave: el codigo de usuario esta repetido en
                     R_Usuarios y no se puede saber cual de las dos personas eres. El
                     procedimiento devuelve cero filas a proposito, antes que arriesgarse
                     a mostrarte los datos de otra persona. Afecta a 4 usuarios activos. -->
                <div id="perfilNoIdentificado" class="alert alert-danger" style="display: none; font-size: 12px">
                    <i class="fa fa-exclamation-triangle"></i>
                    No pudimos identificar su perfil de forma única: su código de usuario
                    está repetido en el sistema. Escriba a Talento Humano para que lo corrijan.
                </div>
            </div>

            <!-- --------------------------------------------------- contenido -->
            <div class="col-lg-9">
                <ul class="nav nav-tabs" role="tablist">
                    <li class="active"><a href="#tabPersonal" data-toggle="tab"><i class="fa fa-lock"></i> Datos personales</a></li>
                    <li><a href="#tabContacto" data-toggle="tab"><i class="fa fa-envelope"></i> Contacto y domicilio</a></li>
                    <li><a href="#tabEmergencia" data-toggle="tab"><i class="fa fa-ambulance"></i> Emergencia</a></li>
                    <li><a href="#tabFormacion" data-toggle="tab"><i class="fa fa-graduation-cap"></i> Formación</a></li>
                    <li><a href="#tabExperiencia" data-toggle="tab"><i class="fa fa-briefcase"></i> Experiencia</a></li>
                    <li><a href="#tabCargas" data-toggle="tab"><i class="fa fa-users"></i> Cargas familiares</a></li>
                    <!-- Aparece sola: se muestra desde el JavaScript si la
                         cabecera dice EsJefe. No la enciende ningun perfil ni
                         ninguna fila de menu, asi que no hay una lista de jefes
                         que mantener. Son 22 personas hoy. -->
                    <li id="liTabEquipo" style="display: none">
                        <a href="#tabEquipo" data-toggle="tab"><i class="fa fa-sitemap"></i> Mi equipo</a>
                    </li>
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
                                        </select>
                                        <p class="text-muted" id="notaEstadoCivilSinFicha" style="display:none;">
                                            Este dato lo administra Talento Humano y todavía no hay una ficha asociada a su usuario.
                                        </p>
                                        <p class="text-muted" id="notaEstadoCivilValorSinCalzar" style="display:none;"></p></div>
                                </div>
                                <button type="button" class="btn btn-primary" onclick="GuardarContacto()">
                                    <i class="fa fa-save"></i> Guardar cambios
                                </button>
                            </div>
                        </div>
                    </div>
                    <div class="tab-pane" id="tabEmergencia">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Contactos de emergencia
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">
                                    A quién debemos llamar si le ocurre algo. Puede registrar más de uno.
                                </p>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover">
                                        <thead>
                                            <tr><th>Nombre</th><th>Parentesco</th><th>Teléfono</th><th style="width:90px">Quitar</th></tr>
                                        </thead>
                                        <tbody id="cuerpoEmergencia"></tbody>
                                    </table>
                                </div>
                                <hr />
                                <div class="row">
                                    <div class="form-group col-lg-4"><label>Nombre completo</label>
                                        <input type="text" class="form-control" id="emNombre" maxlength="150" /></div>
                                    <div class="form-group col-lg-3"><label>Parentesco</label>
                                        <input type="text" class="form-control" id="emParentesco" maxlength="50" /></div>
                                    <div class="form-group col-lg-3"><label>Teléfono</label>
                                        <input type="tel" class="form-control" id="emTelefono" maxlength="50" /></div>
                                    <div class="form-group col-lg-2" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary btn-block" onclick="AgregarEmergencia()">
                                            <i class="fa fa-plus"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tab-pane" id="tabFormacion">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Estudios
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">Formación académica formal.</p>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover">
                                        <thead>
                                            <tr><th>Nivel</th><th>Institución</th><th>Título</th><th style="width:80px">Año</th><th style="width:70px">Quitar</th></tr>
                                        </thead>
                                        <tbody id="cuerpoEstudios"></tbody>
                                    </table>
                                </div>
                                <hr />
                                <div class="row">
                                    <div class="form-group col-lg-3"><label>Nivel</label>
                                        <select class="form-control" id="esNivel">
                                            <option value="">Seleccione…</option>
                                            <option>Bachillerato</option><option>Tercer nivel</option>
                                            <option>Cuarto nivel (Maestría)</option><option>Doctorado</option>
                                        </select></div>
                                    <div class="form-group col-lg-3"><label>Institución</label>
                                        <input type="text" class="form-control" id="esInstitucion" maxlength="200" /></div>
                                    <div class="form-group col-lg-3"><label>Título obtenido</label>
                                        <input type="text" class="form-control" id="esTitulo" maxlength="200" /></div>
                                    <div class="form-group col-lg-1"><label>Año</label>
                                        <input type="number" class="form-control" id="esAnio" min="1940" max="2100" /></div>
                                    <div class="form-group col-lg-2" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary btn-block" onclick="AgregarEstudio()">
                                            <i class="fa fa-plus"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Certificaciones
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">Cursos y certificaciones.</p>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover">
                                        <thead>
                                            <tr><th>Nombre</th><th>Entidad emisora</th><th style="width:120px">Obtenida</th><th style="width:170px">Respaldo</th><th style="width:70px">Quitar</th></tr>
                                        </thead>
                                        <tbody id="cuerpoCertificaciones"></tbody>
                                    </table>
                                </div>
                                <hr />
                                <div class="row">
                                    <div class="form-group col-lg-4"><label>Nombre de la certificación</label>
                                        <input type="text" class="form-control" id="ceNombre" maxlength="200" /></div>
                                    <div class="form-group col-lg-3"><label>Entidad emisora</label>
                                        <input type="text" class="form-control" id="ceEntidad" maxlength="200" /></div>
                                    <div class="form-group col-lg-3"><label>Fecha de obtención</label>
                                        <input type="month" class="form-control" id="ceFecha" /></div>
                                    <div class="form-group col-lg-2" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary btn-block" onclick="AgregarCertificacion()">
                                            <i class="fa fa-plus"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tab-pane" id="tabExperiencia">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Experiencia laboral
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">
                                    Deje el año de fin vacío si sigue trabajando ahí.
                                </p>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover">
                                        <thead>
                                            <tr><th>Empresa</th><th>Cargo</th><th style="width:110px">Período</th><th>Funciones</th><th style="width:70px">Quitar</th></tr>
                                        </thead>
                                        <tbody id="cuerpoExperiencia"></tbody>
                                    </table>
                                </div>
                                <hr />
                                <div class="row">
                                    <div class="form-group col-lg-3"><label>Empresa</label>
                                        <input type="text" class="form-control" id="exEmpresa" maxlength="200" /></div>
                                    <div class="form-group col-lg-3"><label>Cargo</label>
                                        <input type="text" class="form-control" id="exCargo" maxlength="200" /></div>
                                    <div class="form-group col-lg-2"><label>Desde (año)</label>
                                        <input type="number" class="form-control" id="exDesde" min="1940" max="2100" /></div>
                                    <div class="form-group col-lg-2"><label>Hasta (año)</label>
                                        <input type="number" class="form-control" id="exHasta" min="1940" max="2100" placeholder="Actual" /></div>
                                    <div class="form-group col-lg-2" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary btn-block" onclick="AgregarExperiencia()">
                                            <i class="fa fa-plus"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-12"><label>Funciones principales</label>
                                        <textarea class="form-control" id="exFunciones" rows="2"></textarea></div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tab-pane" id="tabCargas">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Cargas familiares
                                <span class="label label-info pull-right"><i class="fa fa-pencil"></i> Editable</span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">
                                    Las personas que dependen económicamente de usted.
                                </p>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover">
                                        <thead>
                                            <tr><th>Nombre</th><th style="width:130px">Parentesco</th><th style="width:140px">Fecha de nacimiento</th><th style="width:170px">Respaldo</th><th style="width:70px">Quitar</th></tr>
                                        </thead>
                                        <tbody id="cuerpoCargas"></tbody>
                                    </table>
                                </div>
                                <hr />
                                <div class="row">
                                    <div class="form-group col-lg-5"><label>Nombre completo</label>
                                        <input type="text" class="form-control" id="cfNombre" maxlength="150" /></div>
                                    <div class="form-group col-lg-3"><label>Parentesco</label>
                                        <select class="form-control" id="cfParentesco">
                                            <option value="">Seleccione…</option>
                                            <option>Hijo/a</option><option>Cónyuge</option>
                                            <option>Padre</option><option>Madre</option><option>Otro</option>
                                        </select></div>
                                    <div class="form-group col-lg-2"><label>Fecha de nacimiento</label>
                                        <input type="date" class="form-control" id="cfFecha" /></div>
                                    <div class="form-group col-lg-2" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary btn-block" onclick="AgregarCargaFamiliar()">
                                            <i class="fa fa-plus"></i> Agregar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tab-pane" id="tabEquipo">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Mi equipo
                                <span class="label label-default pull-right">
                                    <i class="fa fa-lock"></i> Solo consulta
                                </span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">
                                    Las personas que le reportan directamente. Puede consultar su
                                    información laboral y de emergencia; los datos personales y las
                                    cargas familiares no se muestran.
                                </p>

                                <div class="row">
                                    <div class="form-group col-lg-6">
                                        <label>Buscar (nombre, código o cargo):</label>
                                        <input type="text" class="form-control" id="txtBuscarEquipo"
                                               placeholder="Escriba para filtrar…"
                                               onkeypress="if(event.keyCode==13){BuscarEquipo();return false;}" />
                                    </div>
                                    <div class="form-group col-lg-6" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary" onclick="BuscarEquipo()">
                                            <i class="fa fa-search"></i> Buscar
                                        </button>
                                        <button type="button" class="btn btn-default" onclick="LimpiarBusquedaEquipo()">
                                            Mostrar todos
                                        </button>
                                    </div>
                                </div>

                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover">
                                        <thead>
                                            <tr><th>Nombre</th><th>Cargo</th><th>Área</th><th>Ciudad</th><th style="width:70px">Ver</th></tr>
                                        </thead>
                                        <tbody id="cuerpoEquipo"></tbody>
                                    </table>
                                </div>
                            </div>
                        </div>

                        <!-- Se muestra al elegir a alguien de la lista. -->
                        <div class="panel panel-default" id="panelSubordinado" style="display: none">
                            <div class="panel-heading">
                                <span id="subNombre">–</span>
                                <button type="button" class="close" onclick="$('#panelSubordinado').hide()">&times;</button>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-lg-3 text-center">
                                        <img id="subFoto" alt="Foto"
                                             style="display: none; width: 96px; height: 96px; margin: 0 auto 10px;
                                                    border-radius: 50%; object-fit: cover" />
                                        <div id="subAvatar"
                                             style="width: 96px; height: 96px; margin: 0 auto 10px; border-radius: 50%;
                                                    background: #750202; color: #fff; font-size: 34px; line-height: 96px;">–</div>
                                    </div>
                                    <div class="col-lg-9">
                                        <div class="row">
                                            <div class="form-group col-lg-6"><label>Cargo</label>
                                                <p class="form-control-static" id="subCargo">–</p></div>
                                            <div class="form-group col-lg-6"><label>Área</label>
                                                <p class="form-control-static" id="subArea">–</p></div>
                                            <div class="form-group col-lg-6"><label>Ciudad</label>
                                                <p class="form-control-static" id="subCiudad">–</p></div>
                                            <div class="form-group col-lg-6"><label>Correo de notificación</label>
                                                <p class="form-control-static" id="subCorreo">–</p></div>
                                            <div class="form-group col-lg-6"><label>Jefe inmediato</label>
                                                <p class="form-control-static" id="subJefe">–</p></div>
                                            <div class="form-group col-lg-6"><label>Horario</label>
                                                <p class="form-control-static" id="subHorario">–</p></div>
                                        </div>
                                    </div>
                                </div>

                                <hr />
                                <h5><i class="fa fa-ambulance"></i> Contactos de emergencia</h5>
                                <div class="table-responsive">
                                    <table class="table table-bordered">
                                        <thead><tr><th>Nombre</th><th>Parentesco</th><th>Teléfono</th></tr></thead>
                                        <tbody id="cuerpoSubEmergencia"></tbody>
                                    </table>
                                </div>

                                <h5><i class="fa fa-graduation-cap"></i> Formación</h5>
                                <div class="table-responsive">
                                    <table class="table table-bordered">
                                        <thead><tr><th>Título</th><th>Institución</th><th>Nivel</th><th style="width:70px">Año</th></tr></thead>
                                        <tbody id="cuerpoSubEstudios"></tbody>
                                    </table>
                                </div>

                                <h5><i class="fa fa-certificate"></i> Certificaciones</h5>
                                <div class="table-responsive">
                                    <table class="table table-bordered">
                                        <thead><tr><th>Nombre</th><th>Entidad</th><th style="width:110px">Obtenida</th></tr></thead>
                                        <tbody id="cuerpoSubCertificaciones"></tbody>
                                    </table>
                                </div>

                                <h5><i class="fa fa-briefcase"></i> Experiencia</h5>
                                <div class="table-responsive">
                                    <table class="table table-bordered">
                                        <thead><tr><th>Empresa</th><th>Cargo</th><th style="width:110px">Período</th><th>Funciones</th></tr></thead>
                                        <tbody id="cuerpoSubExperiencia"></tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>
            </div>
        </div>

        <!-- Un solo control de archivo para toda la pantalla. Antes de abrirlo se
             le cuelga con .data() a que fila pertenece: un input por fila serian
             tantos como respaldos pueda tener la persona, creados y destruidos en
             cada repintado. -->
        <input type="file" id="inDocumento" accept=".pdf,.jpg,.jpeg,.png" style="display: none" />

        <!-- Modal Informativo -->
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
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->
    </div>
