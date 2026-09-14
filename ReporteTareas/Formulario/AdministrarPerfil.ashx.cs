using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetPerfil
{
    /// <summary>
    /// Handler de la pantalla "Mi perfil".
    ///
    /// IRequiresSessionState no es decorativo: sin el, context.Session es null
    /// en un IHttpHandler y no habria de donde sacar la identidad.
    ///
    /// La estructura viene de AdministrarHorarioUsuario.ashx, pero NO su manejo
    /// de identidad. Aquel recibe codUsuario del cliente; aca eso seria que
    /// cualquiera lea y sobrescriba el perfil de cualquiera. Se sigue el patron
    /// de AdministrarUsuarios.ashx: la identidad sale de la sesion, siempre.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarPerfil : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                responseAction.Append(responseMessage("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger"));
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();

                JavaScriptSerializer i = new JavaScriptSerializer();
                dynamic parametros = i.Deserialize(inputJson.ToString(), typeof(object));

                var Action = parametros[0]["action"];
                bool existAction = false;

                if (Action == "CargarPerfil")
                {
                    existAction = true;
                    responseAction.Append(CargarPerfil(context));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }

            context.Response.ContentType = "application/json";
            // La app corre en windows-1252; se fuerza UTF-8 en los bytes para que
            // coincidan con el charset declarado y las tildes lleguen intactas.
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        /// <summary>
        /// El perfil de quien esta conectado. No recibe parametros a proposito:
        /// no hay nada que el cliente pueda decir sobre de quien es este perfil.
        /// </summary>
        private string CargarPerfil(HttpContext context)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);

                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                return ToJson(NegPerfil.CargarPerfil(codUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar el perfil. " + ex.Message, "danger");
            }
        }

        /// <summary>De quien es este perfil. Sale de la sesion, nunca del cliente.</summary>
        private string CodUsuarioSesion(HttpContext context)
        {
            if (context.Session != null && context.Session["Cod_Usuario"] != null)
            {
                return context.Session["Cod_Usuario"].ToString().Trim();
            }
            return "";
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

        /// <summary>
        /// Serializa escapando lo no ASCII como \uXXXX. El helper compartido usa
        /// Encoding.Default y rompe las tildes.
        /// </summary>
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
