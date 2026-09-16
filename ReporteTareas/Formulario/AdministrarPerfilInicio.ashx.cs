using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetPerfilInicio
{
    /// <summary>
    /// Handler de la pantalla "Parametrización de página de inicio por perfil".
    /// Acciones: ListaPerfilInicio, ListaPaginas, GuardarPerfilInicio.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarPerfilInicio : IHttpHandler
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

                if (Action == "ListaPerfilInicio")
                {
                    existAction = true;
                    responseAction.Append(ListaPerfilInicio(parameters));
                }

                if (Action == "ListaPaginas")
                {
                    existAction = true;
                    responseAction.Append(ListaPaginas(parameters));
                }

                if (Action == "GuardarPerfilInicio")
                {
                    existAction = true;
                    responseAction.Append(GuardarPerfilInicio(parameters));
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

        private string ListaPerfilInicio(dynamic campos)
        {
            try
            {
                return ToJson(NegPerfilInicio.ListarPerfilInicio());
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los perfiles. " + ex.Message, "danger");
            }
        }

        private string ListaPaginas(dynamic campos)
        {
            try
            {
                return ToJson(NegPerfilInicio.ListarPaginasMenu());
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar las páginas. " + ex.Message, "danger");
            }
        }

        private string GuardarPerfilInicio(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();

            try
            {
                int idPerfil = 0;
                try { idPerfil = Convert.ToInt32(campos["idPerfil"]); }
                catch { idPerfil = 0; }

                string href = "";
                try { href = Convert.ToString(campos["href"]).Trim(); }
                catch { href = ""; }

                int idTipo = 0;
                try { idTipo = Convert.ToInt32(campos["idTipo"]); }
                catch { idTipo = 0; }

                string usuarioRegistro = "";
                try { usuarioRegistro = Convert.ToString(campos["usuarioRegistro"]).Trim(); }
                catch { usuarioRegistro = ""; }

                if (idPerfil <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un perfil.", "warning");
                }
                if (string.IsNullOrWhiteSpace(href))
                {
                    return responseMessage("0", "Debe seleccionar la página de inicio.", "warning");
                }

                respuesta = NegPerfilInicio.GuardarPerfilInicio(idPerfil, href, idTipo, usuarioRegistro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar la configuración. " + ex.Message, "danger");
            }

            return ToJson(respuesta);
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
