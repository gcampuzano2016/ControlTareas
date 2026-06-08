using CapaEntidad;
using CapaNegocio;
using JSONHelper;
using SeguridadAppHelper;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetHorarioUsuario
{
    /// <summary>
    /// Handler de la pantalla "Parametrizacion de horario por usuario".
    /// Acciones: ListaPerfilesHorario, ListaUsuariosHorario, AsignarHorarioUsuario.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarHorarioUsuario : IHttpHandler
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

                if (Action == "ListaPerfilesHorario")
                {
                    existAction = true;
                    responseAction.Append(ListaPerfilesHorario());
                }

                if (Action == "ListaUsuariosHorario")
                {
                    existAction = true;
                    responseAction.Append(ListaUsuariosHorario(parameters));
                }

                if (Action == "AsignarHorarioUsuario")
                {
                    existAction = true;
                    responseAction.Append(AsignarHorarioUsuario(parameters));
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

        private string ListaPerfilesHorario()
        {
            try
            {
                return ToJson(NegUsuarioHorario.ListarPerfiles());
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los perfiles de horario. " + ex.Message, "danger");
            }
        }

        private string ListaUsuariosHorario(dynamic campos)
        {
            try
            {
                string filtro = "";
                try { filtro = Convert.ToString(campos["filtro"]); }
                catch { filtro = ""; }

                return ToJson(NegUsuarioHorario.ListarUsuarios(filtro));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los usuarios. " + ex.Message, "danger");
            }
        }

        private string AsignarHorarioUsuario(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();

            try
            {
                SeguridadHelper seguridad = new SeguridadHelper();

                string codUsuario = Convert.ToString(campos["codUsuario"]).Trim();
                int idHorario = Convert.ToInt32(campos["idHorario"]);
                string fechaDesde = Convert.ToString(campos["fechaDesde"]).Trim();

                string usuarioRegistro = "";
                try { usuarioRegistro = Convert.ToString(campos["usuarioRegistro"]).Trim(); }
                catch { usuarioRegistro = ""; }

                if (string.IsNullOrWhiteSpace(codUsuario))
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                respuesta = NegUsuarioHorario.AsignarHorario(codUsuario, idHorario, fechaDesde, usuarioRegistro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al asignar el horario. " + ex.Message, "danger");
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

        /// <summary>
        /// Serializa a JSON escapando los caracteres no ASCII (tildes, ñ) como \uXXXX,
        /// evitando el problema de codificación del helper compartido (Encoding.Default).
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
