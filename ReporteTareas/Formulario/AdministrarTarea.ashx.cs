using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using JSONHelper;
using System.Text;
using CapaEntidad;
using CapaNegocio;
using SeguridadAppHelper;
using System.Web.Script.Serialization;
using System.Globalization;
using CorreoHelper;
using ReporteTareas.ConsultarTicket2;
using ReporteTareas.ConsultarIncidente2;
using System.IO;
using System.Data;

namespace JsonJQueryNetAdministrarTarea
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarTarea : IHttpHandler
    {
        
        public void ProcessRequest(HttpContext context)
        {
            dynamic parametros;
            StringBuilder responseAction = new StringBuilder();

            if (context.Request.ContentType.Contains("json"))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();
                List<RespuestaJson> collectionJson = inputJson.DeserializarJsonTo<List<RespuestaJson>>();

                JavaScriptSerializer i = new JavaScriptSerializer();
                parametros = i.Deserialize(inputJson.ToString(), typeof(object));

                var parameters = parametros[0]["parameters"];
                parameters["Ip_Cliente"] = context.Request.UserHostAddress;

                var Action = parametros[0]["action"];
                Boolean existAction = false;

                if (Action == "ActualizarDetalleTarea")
                {
                    existAction = true;
                    responseAction.Append(ActualizarDetalleTarea(parameters));
                }

                if (Action == "GuardarDetalleTarea")
                {
                    existAction = true;
                    responseAction.Append(GuardarDetalleTarea(parameters));
                }

                if(Action == "EnviarEmailPoliza")
                {
                    existAction = true;
                    responseAction.Append(EnviarEmailPolizas(parameters));
                }

                if (Action == "EnviarEmailPermiso")
                {
                    existAction = true;
                    responseAction.Append(EnviarEmailPermiso(parameters));
                }

                if (Action == "BorrarDetalleTarea")
                {
                    existAction = true;
                    responseAction.Append(BorrarDetalleTarea(parameters));
                }

                if (Action == "CambiarEstadoDetalleTarea")
                {
                    existAction = true;
                    responseAction.Append(CambiarEstadoDetalleTarea(parameters));
                }

                if (Action == "GuardarCambioEstado")
                {
                    existAction = true;
                    responseAction.Append(GuardarDetalleTareaCambioEstado(parameters));
                }
                
                if (Action == "AprobarTarea")
                {
                    existAction = true;
                    responseAction.Append(AprobarTareas(parameters));
                }

                if (Action == "AprobarTareaIndividual")
                {
                    existAction = true;
                    responseAction.Append(AprobarTareasIndividual(parameters));
                }

                if (Action == "AnularAprobacionTareas")
                {
                    existAction = true;
                    responseAction.Append(AnularAprobacionTareas(parameters));
                }

                if (Action == "AnularHorasExtras")
                {
                    existAction = true;
                    responseAction.Append(AnularHorasExtra(parameters));
                }

                if (Action == "AprobarHorasExtras")
                {
                    existAction = true;
                    responseAction.Append(AprobarHorasExtra(parameters));
                }


                if (Action == "AprobarTareaRevisor")
                {
                    existAction = true;
                    responseAction.Append(AprobarTareaRevisor(parameters));
                }
                
                if (Action == "AnularAprobacionTareasRevisor")
                {
                    existAction = true;
                    responseAction.Append(AnularAprobacionTareasRevisor(parameters));
                }


                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }


            }

 

            context.Response.ContentType = "application/json";
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        public string ActualizarDetalleTarea(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntRespuesta respuestaActualizaHorasExtras = new EntRespuesta(); 
            EntDetalleTarea registro = new EntDetalleTarea();
            EntDetalleTarea registroOriginal = new EntDetalleTarea();
            EntTareas objTarea = new EntTareas();
            string correoJefeInmediato = "";
            string correoUsuario = "";
            List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
            EntItemValor campoCorreo = new EntItemValor();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            string IpCliente = ""; 


            try
            {
                registroOriginal = NegTareas.RTA_ConsultaDetalleTareaRTA(Convert.ToInt32(campos["frmTxtCodigo"]));

                // Validación de estado de tarea para permitir actualización.
                if (registroOriginal.Det_Aprobacion_Tarea_Estado == 2)
                {
                    return responseMessage("0", "La Tarea <b>No puede ser modificada debido a que ya se encuentra en estado APROBADA</b>.", "warning", "");
                }

                // Consulta datos de tarea principal
                if (Convert.ToInt32(campos["frmTxtCodigoTarea"]) != registroOriginal.Id_RegTareas)
                {
                    registro.Id_RegTareas = Convert.ToInt32(campos["frmTxtCodigoTarea"]);
                    objTarea = NegTareas.RTA_ConsultaTareaRTA(registro.Id_RegTareas);
                }
                else
                {
                    objTarea = NegTareas.RTA_ConsultaTareaRTA(registroOriginal.Id_RegTareas);
                }

                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
                IpCliente = campos["Ip_Cliente"];

                correoJefeInmediato = NegUsuario.RTA_CorreoJefeInmediato(IdUsuarioSession);
                correoUsuario = NegUsuario.RTA_CorreoUsuario(IdUsuarioSession);

                registro.Det_Num_OrdenServicio = objTarea.Num_OrdenServicio;
                registro.Det_Id_CompAranda = objTarea.Id_CompAranda;
                registro.Det_Nom_Empresa = objTarea.Nom_Empresa;
                registro.Id_RegTareas = objTarea.Id_RegTareas;

                registro.Cod_CatalogoTareaSap = Convert.ToInt32(campos["frmCmbTipoActividad"]); 
                registro.Id_RegDetTareas = Convert.ToInt32(campos["frmTxtCodigo"]);
                registro.Det_Det_Tarea = campos["frmTxtTareaDetalle"];
                string fechaInicio = campos["frmTxtFecha"] + " " + campos["frmTxtHoraDesde"];
                string fechaFin = campos["frmTxtFecha"] + " " + campos["frmTxtHoraHasta"];
                DateTime dtFechaInicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                DateTime dtFechaFin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                registro.Det_Fch_RegDetalleIni = dtFechaInicio.ToString("yyyy-MM-dd HH:mm:ss");
                registro.Det_Fch_RegDetalleFin = dtFechaFin.ToString("yyyy-MM-dd HH:mm:ss");
                registro.Det_Tiempo = campos["frmTxtTiempo"];
                registro.Det_Observaciones = campos["frmTxtObservaciones"];

                registro.Det_Horas_Extras_Tipo = Convert.ToInt32(campos["frmcmbHorasExtras"]);

                registro.IdTipoGasto = Convert.ToInt32(campos["frmCmbTipoGasto"]);
                registro.Det_Num_OrdenServicio = Convert.ToString(campos["frmTxtOrdenServicio"]).Trim();

                respuesta = NegTareas.RTA_ActualizaDetalleTarea(registro, IdUsuarioSession, IpCliente);
                bool respuestaEnvioCorreo = false;
                bool respuestaEnvioCorreoUsuario = false;

                if (respuesta.estado == "1" && ((registro.Det_Horas_Extras_Tipo == 1 || registro.Det_Horas_Extras_Tipo == 2) && (registroOriginal.Det_Horas_Extras_Estado == 0 || registroOriginal.Det_Horas_Extras_Tipo != registro.Det_Horas_Extras_Tipo)))
                {
                    EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();

                    // Se encripta los parametros de aprobación y rechazo para enviar en la url de aprobación.
                    string valorEncritar1 = registroOriginal.Id_RegDetTareas.ToString() + ";" + registroOriginal.Id_RegTareas.ToString() + ";" + "D" + ";" + registroOriginal.Det_Horas_Extras_Tipo.ToString();
                    string valorEncritar2 = registroOriginal.Id_RegDetTareas.ToString() + ";" + registroOriginal.Id_RegTareas.ToString() + ";" + "R" + ";" + registroOriginal.Det_Horas_Extras_Tipo.ToString();
                    string valorIncritado1 = envioCorreo.Encrypt(valorEncritar1, "3m1l10100", "3m1l10100");
                    string valorIncritado2 = envioCorreo.Encrypt(valorEncritar2, "3m1l10100", "3m1l10100");
                    // Se trae el parametro de la URL del sitio de aprobaciones.
                    string urlSiteAprobacion = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE_APROBACIONES");
                    // Se estrcutura las URL de aprobación y rechazo de las horas extras
                    string urlAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado1;
                    string urlRechazarAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado2;
                    // Llenado de listado de parámetros a ser reemplazados en el contenido del correo electrónico.
                    listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "SOLICITUD DE HORAS EXTRAS" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Orden de Servicio:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = registro.Det_Num_OrdenServicio });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta11", Valor = "Cod. ARANDA:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto11", Valor = registro.Det_Id_CompAranda });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Empresa:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = registro.Det_Nom_Empresa });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta21", Valor = "Cliente:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto21", Valor = objTarea.NombreCliente });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta22", Valor = "Fecha:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto22", Valor = campos["frmTxtFecha"] });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta23", Valor = "Hora Inicio:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto23", Valor = campos["frmTxtHoraDesde"] });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta24", Valor = "Hora Fin:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto24", Valor = campos["frmTxtHoraHasta"] });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta25", Valor = "Tiempo:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto25", Valor = campos["frmTxtTiempo"] });

                    if (registro.Det_Horas_Extras_Tipo == 1)
                    {
                        listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Tipo de Horas Extra:" });
                        listaCamposCorreo.Add(new EntItemValor() { Item = "texto3", Valor = "50%" });
                    }
                    if (registro.Det_Horas_Extras_Tipo == 2)
                    {
                        listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Tipo de Horas Extra:" });
                        listaCamposCorreo.Add(new EntItemValor() { Item = "texto3", Valor = "100%" });
                    }

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta4", Valor = "Solicitante:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto4", Valor = usuario.Nom_Usuario });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta7", Valor = "Actividad Principal:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto7", Valor = objTarea.Det_Tarea });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaDescripcion", Valor = "Descripción de la Tarea:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "textoDescripcion", Valor = registroOriginal.Det_Det_Tarea.ToString() });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton1", Valor = "Aprobar" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton1", Valor = urlAprobacion });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton2", Valor = "Rechazar" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton2", Valor = urlRechazarAprobacion });


                    //correoJefeInmediato = "arosero@dos.com.ec";

                    respuestaEnvioCorreo = envioCorreo.EnvioCorreo(correoJefeInmediato, "Autorización de Horas Extras", envioCorreo.EstructuraContenidoCorreo(), listaCamposCorreo);
                    respuestaEnvioCorreoUsuario = envioCorreo.EnvioCorreo(correoUsuario, "Solicitud de Autorización de Horas Extras", envioCorreo.EstructuraContenidoCorreoUsuario(), listaCamposCorreo);

                    if (!respuestaEnvioCorreo)
                    {
                        return responseMessage("0", "Se guardo correctamente los datos, pero no se pudo enviar el correo con la solicitud de horas extras.", "warning");
                    }
                    else
                    {
                        // Se envia el codigo de la solicitud y el valor 1-Solicitud Horas Extras Enviada
                        respuestaActualizaHorasExtras = NegTareas.RTAActualizarEstadoHorasExtras(registro.Id_RegDetTareas, 1);
                        if(respuestaActualizaHorasExtras.estado == "0")
                        {
                            return responseMessage("0", "Se guardo correctamente los datos y se envía correo con solicitud, pero no se pudo cambiar el estado de la solicitud.", "warning");
                        }

                    }
                    
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al actualizar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string GuardarDetalleTarea(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntRespuesta respuestaConsulta = new EntRespuesta();
            EntRespuesta respuestaActualizaHorasExtras = new EntRespuesta();
            EntDetalleTarea registro = new EntDetalleTarea();
            EntTareas objTarea = new EntTareas();
            EntCatalogo objCatalogoEstado = new EntCatalogo();
            EntDetalleTarea registroOriginal = new EntDetalleTarea();
            string correoJefeInmediato = "";
            List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
            EntItemValor campoCorreo = new EntItemValor();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";

            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                registro.Id_RegTareas = Convert.ToInt32(campos["frmTxtCodigoTarea"]);
                // Consulto la tarea principal para poblar datos del detalle de la tarea nueva
                objTarea = NegTareas.RTA_ConsultaTareaRTA(registro.Id_RegTareas);

                registro.Det_Num_OrdenServicio = objTarea.Num_OrdenServicio;
                registro.Det_Id_CompAranda = objTarea.Id_CompAranda;

                string fechaInicio = campos["frmTxtFecha"] + " " + campos["frmTxtHoraDesde"];
                string fechaFin = campos["frmTxtFecha"] + " " + campos["frmTxtHoraHasta"];
                DateTime dtFechaInicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                DateTime dtFechaFin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                registro.Det_Fch_RegDetalleIni = dtFechaInicio.ToString("yyyy-MM-dd HH:mm:ss");
                registro.Det_Fch_RegDetalleFin = dtFechaFin.ToString("yyyy-MM-dd HH:mm:ss");
                registro.Det_Tiempo = campos["frmTxtTiempo"];

                respuestaConsulta = NegTareas.RTA_ConsultaCatalogo(objTarea.IdEstado);
                objCatalogoEstado = respuestaConsulta.resultado;

                registro.Cod_CatalogoTareaSap = Convert.ToInt32(campos["frmCmbTipoActividad"]);
                registro.Det_EstadoIni = objCatalogoEstado.IdExterno;
                registro.Det_EstadoFin = objCatalogoEstado.IdExterno;
                registro.Det_Nom_Empresa = objTarea.Nom_Empresa;
                registro.Det_Det_Tarea = campos["frmTxtTareaDetalle"];
                registro.Det_Estado = objTarea.EstadoTarea;
                registro.IdDet_EstadoIni = objCatalogoEstado.IdCatalogo;
                registro.Det_Motivo_Cambio_Estado = objTarea.EstadoTarea;
                registro.Det_Observaciones = campos["frmTxtObservaciones"];
                registro.Det_Horas_Extras_Tipo = Convert.ToInt32(campos["frmcmbHorasExtras"]);
                registro.Id_Responsable = objTarea.Id_Responsable;
                registro.IdTipoGasto = Convert.ToInt32(campos["frmCmbTipoGasto"]);
                registro.Det_Num_OrdenServicio = Convert.ToString(campos["frmTxtOrdenServicio"]).Trim();
                // Validación de poder crear la tarea en la fecha indicada
                int diaSemanaMasMenosFecha = 0;
                string diasBloqueoEditarTarea = "";
                string fechaInicial = registro.Det_Fch_RegDetalleIni.ToString().Substring(0, 10).ToString();

                DateTime dtFechaInicioComparar = DateTime.ParseExact(fechaInicial, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime dtFechaMinima = DateTime.Today;

                diasBloqueoEditarTarea = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("DIAS_BLOQUEO_EDITAR_TAREA");

                diaSemanaMasMenosFecha = Convert.ToInt32(diasBloqueoEditarTarea);
                dtFechaMinima = dtFechaMinima.AddDays(diaSemanaMasMenosFecha);
                if (dtFechaMinima.Date > dtFechaInicioComparar.Date)
                {
                    return responseMessage("0", "La Nueva Tarea <b>No puede ser creada antes del " + dtFechaMinima.Date.ToString().Substring(0, 10).ToString() + "</b>.", "warning", "");
                }

                respuesta = NegTareas.RTA_InsertaDetalleTarea(registro);

                bool respuestaEnvioCorreo = false;

                if (respuesta.estado == "1" && ((registro.Det_Horas_Extras_Tipo == 1 || registro.Det_Horas_Extras_Tipo == 2) && registro.Det_Horas_Extras_Estado == 0))
                {
                    EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                    registroOriginal = NegTareas.RTA_ConsultaDetalleTareaRTA(Convert.ToInt32(respuesta.resultado));

                    // Se encripta los parametros de aprobación y rechazo para enviar en la url de aprobación.
                    string valorEncritar1 = registroOriginal.Id_RegDetTareas.ToString() + ";" + registroOriginal.Id_RegTareas.ToString() + ";" + "D" + ";" + registroOriginal.Det_Horas_Extras_Tipo.ToString();
                    string valorEncritar2 = registroOriginal.Id_RegDetTareas.ToString() + ";" + registroOriginal.Id_RegTareas.ToString() + ";" + "R" + ";" + registroOriginal.Det_Horas_Extras_Tipo.ToString();
                    string valorIncritado1 = envioCorreo.Encrypt(valorEncritar1, "3m1l10100", "3m1l10100");
                    string valorIncritado2 = envioCorreo.Encrypt(valorEncritar2, "3m1l10100", "3m1l10100");
                    // Se trae el parametro de la URL del sitio de aprobaciones.
                    string urlSiteAprobacion = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE_APROBACIONES");
                    // Se estrcutura las URL de aprobación y rechazo de las horas extras
                    string urlAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado1;
                    string urlRechazarAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado2;
                    // Llenado de listado de parámetros a ser reemplazados en el contenido del correo electrónico.
                    listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "SOLICITUD DE HORAS EXTRAS" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Orden de Servicio:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = registroOriginal.Det_Num_OrdenServicio });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Empresa:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = registroOriginal.Det_Nom_Empresa });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta21", Valor = "Cliente:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto21", Valor = objTarea.NombreCliente });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta22", Valor = "Fecha:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto22", Valor = campos["frmTxtFecha"] });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta23", Valor = "Hora Inicio:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto23", Valor = campos["frmTxtHoraDesde"] });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta24", Valor = "Hora Fin:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto24", Valor = campos["frmTxtHoraHasta"] });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta25", Valor = "Tiempo:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto25", Valor = campos["frmTxtTiempo"] });

                    if (registro.Det_Horas_Extras_Tipo == 1)
                    {
                        listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Tipo de Horas Extra:" });
                        listaCamposCorreo.Add(new EntItemValor() { Item = "texto3", Valor = "50%" });
                    }
                    if (registro.Det_Horas_Extras_Tipo == 2)
                    {
                        listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Tipo de Horas Extra:" });
                        listaCamposCorreo.Add(new EntItemValor() { Item = "texto3", Valor = "100%" });
                    }

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta4", Valor = "Solicitante:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto4", Valor = usuario.Nom_Usuario });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaDescripcion", Valor = "Descripción de la Tarea:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "textoDescripcion", Valor = registroOriginal.Det_Det_Tarea.ToString() });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton1", Valor = "Aprobar" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton1", Valor = urlAprobacion });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton2", Valor = "Rechazar" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton2", Valor = urlRechazarAprobacion });


                    correoJefeInmediato = NegUsuario.RTA_CorreoJefeInmediato(IdUsuarioSession);
                    //correoJefeInmediato = "arosero@dos.com.ec";

                    respuestaEnvioCorreo = envioCorreo.EnvioCorreo(correoJefeInmediato, "Autorización de Horas Extras", envioCorreo.EstructuraContenidoCorreo(), listaCamposCorreo);

                    if (!respuestaEnvioCorreo)
                    {
                        return responseMessage("0", "Se guardo correctamente los datos, pero no se pudo enviar el correo con la solicitud de horas extras.", "warning");

                        // Se envia el codigo de la solicitud y el valor 1-Solicitud Horas Extras Enviada
                        respuestaActualizaHorasExtras = NegTareas.RTAActualizarEstadoHorasExtras(registroOriginal.Id_RegDetTareas, 1);
                        if (respuestaActualizaHorasExtras.estado == "0")
                        {
                            return responseMessage("0", "Se guardo correctamente los datos y se envía correo con solicitud, pero no se pudo cambiar el estado de la solicitud.", "warning");
                        }

                    }
                    else
                    {
                        // Se envia el codigo de la solicitud y el valor 1-Solicitud Horas Extras Enviada
                        respuestaActualizaHorasExtras = NegTareas.RTAActualizarEstadoHorasExtras(registroOriginal.Id_RegDetTareas, 1);
                        if (respuestaActualizaHorasExtras.estado == "0")
                        {
                            return responseMessage("0", "Se guardo correctamente los datos y se envía correo con solicitud, pero no se pudo cambiar el estado de la solicitud.", "warning");
                        }

                    }

                }
                else if (respuesta.estado == "-1")
                {
                    return respuesta.SerializaToJson();
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string EnviarEmailPolizas(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            Int32 idpoliza = 0;
            string numfactura = "";
            string os = "";
            string beneficiario = "";
            string pedidoPoliza = "";
            string correo = "";
            string detalle = "";
            string detalleBody = "";
            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
                idpoliza = Convert.ToInt32(campos["idpoliza"]);
                numfactura = campos["numfactura"];
                os = campos["os"];
                beneficiario = campos["beneficiario"];
                pedidoPoliza = campos["pedidoPoliza"];
                correo = campos["correo"];
                detalle = campos["detalle"];
                detalleBody = os + "↨" + beneficiario + "↨" + pedidoPoliza + "↨" + detalle;
                bool respuestaEnvioCorreo = false;
                EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();

                respuestaEnvioCorreo = envioCorreo.EnvioCorreoPoliza(correo, "Registro de Polizas", detalleBody, detalleBody, idpoliza);
                if (respuestaEnvioCorreo)
                {
                    respuesta.estado = "1";
                    respuesta.mensaje = "Correo enviado exitosamente.";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    respuesta.estado = "1";
                    respuesta.mensaje = "el correo no se fue por favor validar.";
                    respuesta.tipoMensaje = "danger";
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();
        }

        public string EnviarEmailPermiso(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            string correo = "";
            string detalle = "";
            string detalleBody = "";
            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
                correo = campos["correo"];
                detalle = campos["detalle"]+"↨" + usuario.Nom_Usuario;
                detalleBody = detalle;
                bool respuestaEnvioCorreo = false;
                EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();

                respuestaEnvioCorreo = envioCorreo.EnvioCorreoPermiso(correo, "Solicitud de Certificado", detalleBody, detalleBody);
                if (respuestaEnvioCorreo)
                {
                    respuesta.estado = "1";
                    respuesta.mensaje = "Correo enviado exitosamente.";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    respuesta.estado = "1";
                    respuesta.mensaje = "el correo no se fue por favor validar.";
                    respuesta.tipoMensaje = "danger";
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();
        }

        public string GuardarDetalleTareaCambioEstado(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntRespuesta respuestaCreacionDetalle = new EntRespuesta();
            EntRespuesta respuestaCreacionPrincipal = new EntRespuesta();
            EntRespuesta respuestaCambioAranda = new EntRespuesta();
            EntRespuesta respuestaConsulta = new EntRespuesta();
            EntDetalleTarea registro = new EntDetalleTarea();
            EntTareas objTarea = new EntTareas();
            EntCatalogo objCatalogoEstado = new EntCatalogo();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            Int32 cmbEstadoSeleccionado = 0;
            string detalleTarea = "";
            string observacionTarea = "";
            string ipCliente = "";
            string cmbValorSeleccionado = "";
            Int32 idTareaPrincipal = 0;
            Int32 IdRegistroUnico = 0;
            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                idTareaPrincipal = Convert.ToInt32(campos["frmTxtCodigo"]);
                detalleTarea = campos["frmTxtTareaDetalle"];
                observacionTarea = campos["frmTxtObservaciones"];

                string selectEstadoSeleccionado = campos["selectEstadoSeleccionado"];

                cmbValorSeleccionado = campos["cmbEstadoSeleccionado"];
                string[] cmbEstado = cmbValorSeleccionado.Split(Convert.ToChar("-"));
                cmbEstadoSeleccionado = Convert.ToInt32(cmbEstado[0]);
                ipCliente = campos["Ip_Cliente"];

                respuestaConsulta = NegTareas.RTA_ConsultaCatalogo(cmbEstadoSeleccionado);
                objCatalogoEstado = respuestaConsulta.resultado;

                // Se consulta la tarea principal 
                objTarea = NegTareas.RTA_ConsultaTareaRTA(idTareaPrincipal);


                // Cambio de estado de la tarea principal
                objTarea.Estado = objCatalogoEstado.IdExterno;
                objTarea.IdEstadoTarea = objCatalogoEstado.IdCatalogo;
                objTarea.EstadoTarea = objCatalogoEstado.Descripcion;
                objTarea.Det_Id_Usuario_Modificacion = IdUsuarioSession;
                objTarea.Det_Ip_Modificacion = ipCliente;

                //actualizar observacion
                objTarea.EstadoTarea = detalleTarea +":  " + observacionTarea;
                objTarea.Tipo = 1;
                objTarea.Id_RegTareas = idTareaPrincipal;
                NegTareas.RTA_ActualizarRequerimiento(objTarea);
                //actualizar observacion

                //respuestaCreacionPrincipal = NegTareas.RTA_ActualizaEstadoTarea(objTarea); //revisar

                // Creación de nueva tarea detalle en caso de que sea requerido por el estado seleccionado.

                if (objCatalogoEstado.RequiereCrearTareaDetalle == 1 || objCatalogoEstado.RequiereCrearTareaDetalle == 0)
                //if (objCatalogoEstado.RequiereCrearTareaDetalle == 3 || objCatalogoEstado.RequiereCrearTareaDetalle == 4)
                {
                    //
                    // Se crea la tarea detalle con los datos de la tarea principal
                    //
                    registro.Id_RegTareas = objTarea.Id_RegTareas;
                    IdRegistroUnico = objTarea.Id_RegTareas;
                    registro.Det_Num_OrdenServicio = objTarea.Num_OrdenServicio;
                    registro.Det_Id_CompAranda = objTarea.Id_CompAranda;

                    // La fecha de la tarea es la fecha del día actual
                    DateTime dtFechaInicio = DateTime.Now;
                    registro.Det_Fch_RegDetalleIni = dtFechaInicio.ToString("yyyy-MM-dd HH:mm:ss");
                    registro.Det_Fch_RegDetalleFin = dtFechaInicio.ToString("yyyy-MM-dd HH:mm:ss");

                    registro.Det_EstadoIni = objCatalogoEstado.IdExterno;
                    registro.Det_EstadoFin = "";
                    registro.Det_Nom_Empresa = objTarea.Nom_Empresa;
                    registro.Det_Det_Tarea = detalleTarea;
                    registro.Det_Estado = objCatalogoEstado.Descripcion;
                    registro.Det_Motivo_Cambio_Estado = "";
                    registro.Id_Responsable = objTarea.Id_Responsable;
                    registro.IdDet_EstadoIni = objCatalogoEstado.IdCatalogo;
                    registro.Det_Observaciones = observacionTarea;
                    registro.Det_Tiempo = "";
                    //registro.Det_Horas_Extras_Tipo = Convert.ToInt32(campos["frmcmbHorasExtras"]);
                    registro.Det_Fecha_Creacion = dtFechaInicio.ToString("yyyy-MM-dd HH:mm:ss");
                    registro.Det_Id_Usuario_Creacion = IdUsuarioSession;
                    registro.Det_Ip_Creacion = ipCliente;

                    //respuestaCreacionDetalle = NegTareas.RTA_InsertaDetalleTareaActualizar(registro);
                }

                // Cambiar estado en Aranda
                respuestaCambioAranda.mensaje = "";
                string activarActualizacionAranda = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("ACTIVAR_ACTUALIZACION_ARANDA");
                if (Convert.ToBoolean(activarActualizacionAranda))
                {
                    bool resultadoWebService = false;
                    string[] idAranda = objTarea.Id_CompAranda.Split(Convert.ToChar("-"));

                    if (idAranda[0].ToString() == "IM")
                    {
                        // Cuando se trata de un Incidente
                        IncidentSoapClient incidente = new IncidentSoapClient();
                        IncidentDescription getObjectIncidentResponse = new IncidentDescription();
                        Incident serviceIncident = new Incident();

                        if (getObjectIncidentResponse != null)
                        {
                            getObjectIncidentResponse = incidente.GetObject(Convert.ToInt32(idAranda[1]));

                            serviceIncident.Id = Convert.ToInt32(idAranda[1]);
                            //serviceIncident.StatusId = Convert.ToInt32(objCatalogoEstado.IdAranda);
                            serviceIncident.StatusId = Convert.ToInt32(cmbValorSeleccionado);
                            serviceIncident.Description = getObjectIncidentResponse.Description;
                            if (objCatalogoEstado.RequiereTextoSolucion == 1)
                            {
                                serviceIncident.Commentary = detalleTarea;
                            }
                            serviceIncident.ModifierId = Convert.ToInt32(IdUsuarioSession);

                            resultadoWebService = incidente.Update(serviceIncident);

                            //resultadoWebService = false;
                        }
                        if (resultadoWebService)
                        {
                            //Enviar encuesta al cliente
                            if (selectEstadoSeleccionado == "Solucionado")
                            {
                                bool respuestaEnvioCorreo = false;
                                EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                                //respuestaEnvioCorreo = envioCorreo.EnvioCorreoEncuesta("guillermoarma@hotmail.com", "Encuesta de satisfacción al cliente", "", IdRegistroUnico);
                                //IdRegistroUnico
                            }

                            respuestaCambioAranda.mensaje = "Actualización IM de Aranda Exitosa.";
                        }
                        else
                        {
                            respuestaCambioAranda.mensaje = "Actualización IM de Aranda Fallida, debe cambiar estado manualmente en ARANDA.";
                        }

                    }
                    else if (idAranda[0].ToString() == "RF")
                    {
                        // Cuando se trata de un Servicio
                        ServiceCallSoapClient service = new ServiceCallSoapClient();
                        ServiceCallDescription getObjectResponse = new ServiceCallDescription();
                        ServiceCall serviceCall = new ServiceCall();

                        if (getObjectResponse != null)
                        {
                            getObjectResponse = service.GetObject(Convert.ToInt32(idAranda[1]));

                            serviceCall.Id = Convert.ToInt32(idAranda[1]);
                            //serviceCall.StatusId = Convert.ToInt32(objCatalogoEstado.IdAranda);
                            serviceCall.StatusId = Convert.ToInt32(cmbValorSeleccionado);
                            serviceCall.Description = getObjectResponse.Description;
                            if (objCatalogoEstado.RequiereTextoSolucion == 1)
                            {
                                serviceCall.Commentary = detalleTarea;
                            }

                            serviceCall.ModifierId = Convert.ToInt32(IdUsuarioSession);

                            resultadoWebService = service.Update(serviceCall);
                        }

                        if (resultadoWebService)
                        {
                            //Enviar encuesta al cliente
                            if (selectEstadoSeleccionado == "Solucionado")
                            {
                                bool respuestaEnvioCorreo = false;
                                EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                                //respuestaEnvioCorreo = envioCorreo.EnvioCorreoEncuesta("guillermoarma@hotmail.com", "Encuesta de satisfacción al cliente", "", IdRegistroUnico);
                                //IdRegistroUnico
                            }

                            respuestaCambioAranda.mensaje = "Actualización RF de Aranda Exitosa.";
                        }
                        else
                        {
                            respuestaCambioAranda.mensaje = "Actualización RF de Aranda Fallida, debe cambiar estado manualmente en ARANDA.";
                        }

                    }

                }

                //
                // Verificación de acciones realizadas y estructuración de respuesta de toda la acción.
                //

                if (respuestaCreacionPrincipal.estado == "1")
                {
                    respuesta.mensaje = "Se realizo el cambio de estado.";
                    respuesta.estado = "1";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    respuesta.mensaje = respuestaCreacionPrincipal.mensaje;
                    respuesta.estado = "0";
                    respuesta.tipoMensaje = "danger";
                }

                if (objCatalogoEstado.RequiereCrearTareaDetalle == 1 || objCatalogoEstado.RequiereCrearTareaDetalle == 0)
                {
                    if (respuestaCreacionDetalle.estado == "1")
                    {
                        respuesta.mensaje = respuesta.mensaje + " Se crea una nueva actividad.";
                    }
                    else
                    {
                        respuesta.mensaje = respuesta.mensaje + "; " + respuestaCreacionDetalle.mensaje;
                        respuesta.estado = "0";
                        respuesta.tipoMensaje = "danger";
                    }
                }

                respuesta.mensaje = respuesta.mensaje + "; " + respuestaCambioAranda.mensaje;

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string BorrarDetalleTarea(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            EntDetalleTarea registroOriginal = new EntDetalleTarea();
            string IdUsuarioSession = "";
            Int32 idDetTarea = 0;

            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
                idDetTarea = Convert.ToInt32(campos["frmTxtCodigo"]);


                // Validación de estado de tarea para permitir actualización.
                registroOriginal = NegTareas.RTA_ConsultaDetalleTareaRTA(Convert.ToInt32(campos["frmTxtCodigo"]));

                if (registroOriginal.Det_Aprobacion_Tarea_Estado == 2)
                {
                    return responseMessage("0", "La Tarea <b>No puede ser borrada debido a que ya se encuentra en estado APROBADA</b>.", "warning", "");
                }

                respuesta = NegTareas.RTA_BorrarDetalleTarea(idDetTarea);

                if (respuesta.estado == "1")
                {
                    return respuesta.SerializaToJson();
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al tratar de borrar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string CambiarEstadoDetalleTarea(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            EntDetalleTarea registroOriginal = new EntDetalleTarea();
            string IdUsuarioSession = "";
            Int32 idDetTarea = 0;

            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
                idDetTarea = Convert.ToInt32(campos["frmTxtCodigo"]);


                // Validación de estado de tarea para permitir actualización.
                registroOriginal = NegTareas.RTA_ConsultaDetalleTareaRTA(Convert.ToInt32(campos["frmTxtCodigo"]));

                if (registroOriginal.Det_Aprobacion_Tarea_Estado == 2)
                {
                    return responseMessage("0", "La Tarea <b>No puede ser Cambiado debido a que ya se encuentra en estado APROBADA</b>.", "warning", "");
                }

                respuesta = NegTareas.RTA_CambiarEstadoDetalleTarea(idDetTarea);

                if (respuesta.estado == "1")
                {
                    return respuesta.SerializaToJson();
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al tratar de borrar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string AprobarTareas(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntRespuesta respuestaAprobacion = new EntRespuesta();
            EntCatalogo objCatalogoEstado = new EntCatalogo();
            EntUsuario usuario = new EntUsuario();

            string IdUsuarioSession = "";
            string ipCliente = campos["Ip_Cliente"];
            string idUsuarioResponsable = campos["txtCodigo"].ToString();
            string fechaDesde = campos["txtFechaDesde"].ToString();
            string fechaHasta = campos["txtFechaHasta"].ToString();
            
            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                Int32 estadoAprobacion = 2;

                // Actualización de tareas con estado de aprobación. 
                respuestaAprobacion = NegTareas.RTA_CambioEstadoAprobacionTarea(IdUsuarioSession, fechaDesde, fechaHasta, idUsuarioResponsable, IdUsuarioSession, ipCliente, estadoAprobacion);

                //
                // Verificación de acciones realizadas y estructuración de respuesta de toda la acción.
                //
                if (respuestaAprobacion.estado == "1")
                {
                    respuesta.mensaje = "Aprobación de Tareas Exitosa.";
                    respuesta.estado = "1";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    if(respuesta.mensaje == null)
                    {
                        respuesta.mensaje = "No se realizó la Aprobación de Tareas. Si las tareas seleccionadas ya estan aprobadas puede recibir este mensaje.";
                    }
                    respuesta.mensaje = respuesta.mensaje;
                    respuesta.estado = "0";
                    respuesta.tipoMensaje = "danger";
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string AprobarTareasIndividual(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntRespuesta respuestaAprobacion = new EntRespuesta();
            EntCatalogo objCatalogoEstado = new EntCatalogo();
            EntUsuario usuario = new EntUsuario();

            string IdUsuarioSession = "";
            string ipCliente = campos["Ip_Cliente"];
            string idUsuarioResponsable = campos["txtCodigo"].ToString();
            string fechaDesde = campos["txtFechaDesde"].ToString();
            string fechaHasta = campos["txtFechaHasta"].ToString();
            Int32 idDetalleTarea = Convert.ToInt32(campos["idDetalleTarea"].ToString());
            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                Int32 estadoAprobacion = 2;

                // Actualización de tareas con estado de aprobación. 
                respuestaAprobacion = NegTareas.RTA_CambioEstadoAprobacionTareaIndividual(IdUsuarioSession, fechaDesde, fechaHasta, idUsuarioResponsable, IdUsuarioSession, ipCliente, estadoAprobacion, idDetalleTarea);

                //
                // Verificación de acciones realizadas y estructuración de respuesta de toda la acción.
                //
                if (respuestaAprobacion.estado == "1")
                {
                    respuesta.mensaje = "Aprobación de Tareas Exitosa.";
                    respuesta.estado = "1";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    if (respuesta.mensaje == null)
                    {
                        respuesta.mensaje = "No se realizó la Aprobación de Tareas. Si las tareas seleccionadas ya estan aprobadas puede recibir este mensaje.";
                    }
                    respuesta.mensaje = respuesta.mensaje;
                    respuesta.estado = "0";
                    respuesta.tipoMensaje = "danger";
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string AnularAprobacionTareas(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntRespuesta respuestaAprobacion = new EntRespuesta();
            EntCatalogo objCatalogoEstado = new EntCatalogo();
            EntUsuario usuario = new EntUsuario();

            string IdUsuarioSession = "";
            string ipCliente = campos["Ip_Cliente"];
            string idUsuarioResponsable = campos["txtCodigo"].ToString();
            string fechaDesde = campos["txtFechaDesde"].ToString();
            string fechaHasta = campos["txtFechaHasta"].ToString();

            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                Int32 estadoPendienteAprobacion = 1;

                // Actualización de tareas con estado de aprobación. 
                respuestaAprobacion = NegTareas.RTA_CambioEstadoAprobacionTarea(IdUsuarioSession, fechaDesde, fechaHasta, idUsuarioResponsable, IdUsuarioSession, ipCliente, estadoPendienteAprobacion);

                //
                // Verificación de acciones realizadas y estructuración de respuesta de toda la acción.
                //
                if (respuestaAprobacion.estado == "1")
                {
                    respuesta.mensaje = "Se han Anulado las aprobaciones de las Tareas.";
                    respuesta.estado = "1";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    if (respuesta.mensaje == null)
                    {
                        respuesta.mensaje = "No se realizó la anulación de la Aprobación de Tareas. Si las tareas seleccionadas no estan en estado aprobado puede recibir este mensaje.";
                    }
                    respuesta.mensaje = respuesta.mensaje;
                    respuesta.estado = "0";
                    respuesta.tipoMensaje = "danger";
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string AprobarHorasExtra(dynamic campos)
        {

            EntRespuesta respuestaActualizaHorasExtras = new EntRespuesta();
            EntRespuesta respuesta = new EntRespuesta();


            int id = Convert.ToInt32(campos["id"].ToString());
            int codigo = Convert.ToInt32(campos["codigo"].ToString());

            try
            {
                // Actualización de tareas con estado de aprobación. 
                respuestaActualizaHorasExtras = NegTareas.RTAActualizarEstadoHorasExtras(id, codigo);
                //
                // Verificación de acciones realizadas y estructuración de respuesta de toda la acción.
                //
                if (respuestaActualizaHorasExtras.estado == "1")
                {
                    respuesta.mensaje = "Aprobación de Horas Extras Exitosa.";
                    respuesta.estado = "1";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    if (respuesta.mensaje == null)
                    {
                        respuesta.mensaje = "No se realizó la Aprobación de las Horas Extras. Si las tareas seleccionadas ya estan aprobadas puede recibir este mensaje.";
                    }
                    respuesta.mensaje = respuesta.mensaje;
                    respuesta.estado = "0";
                    respuesta.tipoMensaje = "danger";
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string AnularHorasExtra(dynamic campos)
        {

            EntRespuesta respuestaActualizaHorasExtras = new EntRespuesta();
            EntRespuesta respuesta = new EntRespuesta();


            int id = Convert.ToInt32(campos["id"].ToString());
            int codigo = Convert.ToInt32(campos["codigo"].ToString());

            try
            {
                // Actualización de tareas con estado de aprobación. 
                respuestaActualizaHorasExtras = NegTareas.RTAActualizarEstadoHorasExtras(id, codigo);

                //
                // Verificación de acciones realizadas y estructuración de respuesta de toda la acción.
                //
                if (respuestaActualizaHorasExtras.estado == "1")
                {
                    respuesta.mensaje = "Rechazada las Horas Extras Exitosa.";
                    respuesta.estado = "1";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    if (respuesta.mensaje == null)
                    {
                        respuesta.mensaje = "No se realizó la Aprobación de las Horas Extras. Si las tareas seleccionadas ya estan aprobadas puede recibir este mensaje.";
                    }
                    respuesta.mensaje = respuesta.mensaje;
                    respuesta.estado = "0";
                    respuesta.tipoMensaje = "danger";
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string AprobarTareaRevisor(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntRespuesta respuestaAprobacion = new EntRespuesta();
            EntCatalogo objCatalogoEstado = new EntCatalogo();
            EntUsuario usuario = new EntUsuario();

            string IdUsuarioSession = "";
            string ipCliente = campos["Ip_Cliente"];
            string idUsuarioResponsable = campos["txtCodigo"].ToString();
            string fechaDesde = campos["txtFechaDesde"].ToString();
            string fechaHasta = campos["txtFechaHasta"].ToString();

            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                Int32 estadoAprobacion = 2;

                // Actualización de tareas con estado de aprobación. 
                respuestaAprobacion = NegTareas.RTA_CambioEstadoAprobacionTareaRevisor(IdUsuarioSession, fechaDesde, fechaHasta, idUsuarioResponsable, IdUsuarioSession, ipCliente, estadoAprobacion);

                //
                // Verificación de acciones realizadas y estructuración de respuesta de toda la acción.
                //
                if (respuestaAprobacion.estado == "1")
                {
                    respuesta.mensaje = "Aprobación de Tareas Exitosa.";
                    respuesta.estado = "1";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    if (respuesta.mensaje == null)
                    {
                        respuesta.mensaje = "No se realizó la Aprobación de Tareas. Si las tareas seleccionadas ya estan aprobadas puede recibir este mensaje.";
                    }
                    respuesta.mensaje = respuesta.mensaje;
                    respuesta.estado = "0";
                    respuesta.tipoMensaje = "danger";
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }

        public string AnularAprobacionTareasRevisor(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntRespuesta respuestaAprobacion = new EntRespuesta();
            EntCatalogo objCatalogoEstado = new EntCatalogo();
            EntUsuario usuario = new EntUsuario();

            string IdUsuarioSession = "";
            string ipCliente = campos["Ip_Cliente"];
            string idUsuarioResponsable = campos["txtCodigo"].ToString();
            string fechaDesde = campos["txtFechaDesde"].ToString();
            string fechaHasta = campos["txtFechaHasta"].ToString();

            try
            {
                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                Int32 estadoPendienteAprobacion = 1;

                // Actualización de tareas con estado de aprobación. 
                respuestaAprobacion = NegTareas.RTA_CambioEstadoAprobacionTareaRevisor(IdUsuarioSession, fechaDesde, fechaHasta, idUsuarioResponsable, IdUsuarioSession, ipCliente, estadoPendienteAprobacion);

                //
                // Verificación de acciones realizadas y estructuración de respuesta de toda la acción.
                //
                if (respuestaAprobacion.estado == "1")
                {
                    respuesta.mensaje = "Se han Anulado las aprobaciones de las Tareas.";
                    respuesta.estado = "1";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    if (respuesta.mensaje == null)
                    {
                        respuesta.mensaje = "No se realizó la anulación de la Aprobación de Tareas. Si las tareas seleccionadas no estan en estado aprobado puede recibir este mensaje.";
                    }
                    respuesta.mensaje = respuesta.mensaje;
                    respuesta.estado = "0";
                    respuesta.tipoMensaje = "danger";
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger");
            }

            return respuesta.SerializaToJson();

        }


        // Función para armar respuesta que se envía al ajax
        public string responseMessage(string estado, string mensaje, string tipoMensaje, string resultado = "")
        {

            EntRespuesta respuesta = new EntRespuesta();
            respuesta.estado = estado;
            respuesta.mensaje = mensaje;
            respuesta.tipoMensaje = tipoMensaje;
            respuesta.resultado = resultado;

            return respuesta.SerializaToJson().ToString();

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
