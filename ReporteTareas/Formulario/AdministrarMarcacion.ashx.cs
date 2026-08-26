using CapaEntidad;
using CapaNegocio;
using CorreoHelper;
using SeguridadAppHelper;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetMarcacion
{
    /// <summary>
    /// Handler de la marcación de entrada/salida.
    /// La identidad sale SIEMPRE de la sesión cifrada, nunca del request en claro.
    /// Acción: RegistrarMarcacion.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarMarcacion : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();

                JavaScriptSerializer i = new JavaScriptSerializer();
                dynamic parametros = i.Deserialize(inputJson.ToString(), typeof(object));

                var parameters = parametros[0]["parameters"];
                var Action = parametros[0]["action"];
                bool existAction = false;

                if (Action == "RegistrarMarcacion")
                {
                    existAction = true;
                    responseAction.Append(RegistrarMarcacion(parameters));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }

            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        private string RegistrarMarcacion(dynamic campos)
        {
            try
            {
                int accion = 0;
                try { accion = Convert.ToInt32(campos["Accion"]); }
                catch { accion = 0; }

                if (accion != 1 && accion != 2)
                {
                    return responseMessage("0", "Acción no válida.", "warning");
                }

                SeguridadHelper seguridad = new SeguridadHelper();
                string codigoUsuario = seguridad.Desencripta(campos["session"]);

                EntUsuario usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(codigoUsuario);
                if (usuario == null || usuario.Id_Usuario <= 0)
                {
                    return responseMessage("0", "No se pudo identificar al usuario. Vuelva a iniciar sesión.", "danger");
                }

                EntMarcacion resultado = NegMarcacion.RegistrarMarcacion(
                    Convert.ToDecimal(usuario.Id_Usuario), accion);

                if (resultado.Respuestas == 1)
                {
                    // Los datos se capturan ANTES de encolar: en el hilo del ThreadPool
                    // no existe HttpContext.Current y nada puede leerse de la sesión ahí.
                    string correoDestino = (usuario.E_Mail ?? string.Empty).Trim();
                    string nombreUsuario = (usuario.Nom_Usuario ?? string.Empty).Trim();
                    int accionCorreo = accion;
                    DateTime fechaHoraCorreo = resultado.FechaHora;
                    long idProcesoCorreo = resultado.IdProceso;
                    decimal idUsuarioCorreo = Convert.ToDecimal(usuario.Id_Usuario);

                    // Aviso que se agrega al mensaje de éxito cuando se sabe, ya mismo, que
                    // el correo no va a llegar. La marcación sí quedó registrada: se le dice
                    // para que no quede esperando un correo que nunca va a llegar.
                    string avisoCorreo = string.Empty;

                    if (correoDestino == string.Empty)
                    {
                        // Sin correo cargado no hay nada que enviar, pero SI queda constancia:
                        // este era el caso que antes desaparecía en silencio.
                        NegMarcacion.RegistrarLogCorreo(idProcesoCorreo, idUsuarioCorreo, accionCorreo,
                            string.Empty, "SIN_CORREO",
                            "El usuario no tiene E_Mail registrado en R_Usuarios.");

                        avisoCorreo = " No se le envió la notificación por correo porque usted no tiene una " +
                                      "dirección registrada. Comuníquese con el administrador del sistema.";
                    }
                    else if (!CorreoTieneFormatoValido(correoDestino))
                    {
                        // El formato se valida aquí y no en el hilo del envío para poder
                        // avisarle en la misma pantalla, en vez de fallar en silencio.
                        NegMarcacion.RegistrarLogCorreo(idProcesoCorreo, idUsuarioCorreo, accionCorreo,
                            correoDestino, "FALLIDO",
                            "La direccion registrada en R_Usuarios no tiene un formato valido.");

                        avisoCorreo = " No se le envió la notificación por correo porque su dirección registrada (" +
                                      correoDestino + ") no es válida. Comuníquese con el administrador del sistema.";
                    }
                    else
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(delegate
                        {
                            // try/catch TOTAL y obligatorio: una excepción sin capturar en un
                            // hilo del ThreadPool tumba el worker process de ASP.NET.
                            try
                            {
                                EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                                bool enviado = envioCorreo.EnvioCorreoMarcacion(correoDestino, nombreUsuario, accionCorreo, fechaHoraCorreo);

                                NegMarcacion.RegistrarLogCorreo(idProcesoCorreo, idUsuarioCorreo, accionCorreo,
                                    correoDestino,
                                    enviado ? "ENVIADO" : "FALLIDO",
                                    enviado ? string.Empty : (envioCorreo.ErrorProceso ?? "Error no informado por el helper."));
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    NegMarcacion.RegistrarLogCorreo(idProcesoCorreo, idUsuarioCorreo, accionCorreo,
                                        correoDestino, "FALLIDO", ex.Message);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        });
                    }

                    return responseMessage("1", resultado.Mensaje + avisoCorreo, "success");
                }

                return responseMessage("0", resultado.Mensaje, "warning");
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al registrar la marcación. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Valida la dirección con el MISMO parser que usa el envío
        /// (MailAddressCollection, que además parte por comas), para que la pantalla
        /// no prometa un correo que después va a fallar al armar el destinatario.
        /// </summary>
        private static bool CorreoTieneFormatoValido(string correo)
        {
            try
            {
                System.Net.Mail.MailAddressCollection destinatarios = new System.Net.Mail.MailAddressCollection();

                foreach (string parte in (correo ?? string.Empty).Split(new Char[] { ';' }))
                {
                    if (parte.Trim() != string.Empty)
                    {
                        destinatarios.Add(parte.Trim());
                    }
                }

                return destinatarios.Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string responseMessage(string estado, string mensaje, string tipoMensaje, string resultado = "")
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta.estado = estado;
            respuesta.mensaje = mensaje;
            respuesta.tipoMensaje = tipoMensaje;
            respuesta.resultado = resultado;

            return ToJson(respuesta);
        }

        private static string ToJson(object obj)
        {
            if (obj == null) return string.Empty;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return serializer.Serialize(obj);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
