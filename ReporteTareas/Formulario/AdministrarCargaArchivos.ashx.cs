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
using System.IO;

namespace JsonJQueryNetAdministrarCargaArchivos
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarCargaArchivos : IHttpHandler
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

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }


            }
           if (context.Request.ContentType.Contains(""))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();
                List<RespuestaJson> collectionJson = inputJson.DeserializarJsonTo<List<RespuestaJson>>();
                string IdUsuarioSession = "";
                string folderPath = "";
                string fileName = "";
                string mensajeRespuesta = ""; 


                JavaScriptSerializer i = new JavaScriptSerializer();
                SeguridadHelper seguridad = new SeguridadHelper();

                if (context.Request.Files.Count > 0) {

                    folderPath = context.Server.MapPath("~/descargas/");

                    for (int elemento = 0; elemento < context.Request.Files.Count; elemento++)
                    {
                        HttpPostedFile postedFile = context.Request.Files.Get(elemento);
                        fileName = Path.GetFileName(postedFile.FileName);
                        postedFile.SaveAs(folderPath + fileName);
                    }

                    if (context.Request.Files.Count > 1)
                    {
                        mensajeRespuesta = "Archivos cargados con éxito.";
                    }
                    else
                    {
                        mensajeRespuesta = "Archivo cargado con éxito.";
                    }

                    responseAction.Append(responseMessage("1", mensajeRespuesta, "success"));
                    string session = context.Request.Form.Get("session");
                    IdUsuarioSession = seguridad.Desencripta(session.ToString());

                }
                else
                {
                    responseAction.Append(responseMessage("0", "Debe seleccionar un archivo.", "danger"));
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
            string correoJefeInmediato = "";
            List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
            EntItemValor campoCorreo = new EntItemValor();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";

            try
            {
                registroOriginal = NegTareas.RTA_ConsultaDetalleTareaRTA(Convert.ToInt32(campos["frmTxtCodigo"]));

                IdUsuarioSession = seguridad.Desencripta(campos["session"]);
                usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);

                correoJefeInmediato = NegUsuario.RTA_CorreoJefeInmediato(IdUsuarioSession);

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

                respuesta = NegTareas.RTA_ActualizaDetalleTarea(registro, IdUsuarioSession);
                bool respuestaEnvioCorreo = false;

                if (respuesta.estado == "1" && ((registro.Det_Horas_Extras_Tipo == 1 || registro.Det_Horas_Extras_Tipo == 2) && registro.Det_Horas_Extras_Estado == 0))
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
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = registroOriginal.Det_Num_OrdenServicio });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Cliente:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = registroOriginal.Det_Nom_Empresa });

                    if(registro.Det_Horas_Extras_Tipo == 1)
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


                    //correoJefeInmediato = "arosero@dos.com.ec";

                    respuestaEnvioCorreo = envioCorreo.EnvioCorreo(correoJefeInmediato, "Autorización de Horas Extras", envioCorreo.EstructuraContenidoCorreo(), listaCamposCorreo);

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

                correoJefeInmediato = NegUsuario.RTA_CorreoJefeInmediato(IdUsuarioSession);

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

                registro.Det_EstadoIni = objCatalogoEstado.IdExterno;
                registro.Det_EstadoFin = objCatalogoEstado.IdExterno;
                registro.Det_Nom_Empresa = objTarea.Nom_Empresa;
                registro.Det_Det_Tarea = campos["frmTxtTareaDetalle"];
                registro.Det_Estado = objTarea.EstadoTarea;
                registro.Det_Motivo_Cambio_Estado = objTarea.EstadoTarea;
                registro.Det_Observaciones = campos["frmTxtObservaciones"];
                registro.Det_Horas_Extras_Tipo = Convert.ToInt32(campos["frmcmbHorasExtras"]);
                registro.Id_Responsable = objTarea.Id_Responsable;

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
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Cliente:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = registroOriginal.Det_Nom_Empresa });

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


                    //correoJefeInmediato = "arosero@dos.com.ec";

                    respuestaEnvioCorreo = envioCorreo.EnvioCorreo(correoJefeInmediato, "Autorización de Horas Extras", envioCorreo.EstructuraContenidoCorreo(), listaCamposCorreo);

                    if (!respuestaEnvioCorreo)
                    {
                        return responseMessage("0", "Se guardo correctamente los datos, pero no se pudo enviar el correo con la solicitud de horas extras.", "warning");
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
