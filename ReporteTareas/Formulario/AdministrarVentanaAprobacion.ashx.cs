using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetVentanaAprobacion
{
    /// <summary>
    /// Handler de la pantalla "Parametrización de ventana de aprobación por jefe".
    /// Acciones: ListaJefes, GuardarVentana.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarVentanaAprobacion : IHttpHandler
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

                if (Action == "ListaJefes")
                {
                    existAction = true;
                    responseAction.Append(ListaJefes(parameters));
                }

                if (Action == "GuardarVentana")
                {
                    existAction = true;
                    responseAction.Append(GuardarVentana(parameters));
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

        private string ListaJefes(dynamic campos)
        {
            try
            {
                string filtro = "";
                try { filtro = Convert.ToString(campos["filtro"]); }
                catch { filtro = ""; }

                return ToJson(NegVentanaAprobacion.ListarJefes(filtro));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los jefes. " + ex.Message, "danger");
            }
        }

        private string GuardarVentana(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();

            try
            {
                string mailJefe = Convert.ToString(campos["mailJefe"]).Trim();
                string fechaDesde = Convert.ToString(campos["fechaDesde"]).Trim();
                string fechaHasta = Convert.ToString(campos["fechaHasta"]).Trim();

                string usuarioRegistro = "";
                try { usuarioRegistro = Convert.ToString(campos["usuarioRegistro"]).Trim(); }
                catch { usuarioRegistro = ""; }

                if (string.IsNullOrWhiteSpace(mailJefe))
                {
                    return responseMessage("0", "Debe seleccionar un jefe.", "warning");
                }
                if (string.IsNullOrWhiteSpace(fechaDesde) || string.IsNullOrWhiteSpace(fechaHasta))
                {
                    return responseMessage("0", "Debe indicar la fecha desde y la fecha hasta.", "warning");
                }

                respuesta = NegVentanaAprobacion.GuardarVentana(mailJefe, fechaDesde, fechaHasta, usuarioRegistro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar la ventana. " + ex.Message, "danger");
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
        /// Serializa a JSON escapando no-ASCII (tildes, ñ) como \uXXXX,
        /// mismo criterio que AdministrarHorarioUsuario.ashx.cs.
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
