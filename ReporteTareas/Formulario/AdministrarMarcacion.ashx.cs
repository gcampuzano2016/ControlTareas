using CapaEntidad;
using CapaNegocio;
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
                    return responseMessage("1", resultado.Mensaje, "success");
                }

                return responseMessage("0", resultado.Mensaje, "warning");
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al registrar la marcación. " + ex.Message, "danger");
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
