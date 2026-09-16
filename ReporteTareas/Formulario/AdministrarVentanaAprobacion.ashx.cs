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
    /// Acciones: ListaJefes, GuardarVentana, ExcluirJefe.
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

                if (Action == "ExcluirJefe")
                {
                    existAction = true;
                    responseAction.Append(ExcluirJefe(parameters));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }

            context.Response.ContentType = "application/json";
            // La app corre en windows-1252; forzamos UTF-8 en los bytes para que
            // coincidan con el charset declarado y las tildes/ñ lleguen intactas.
            context.Response.ContentEncoding = Encoding.UTF8;
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

                bool incluirInactivos = false;
                try { incluirInactivos = Convert.ToBoolean(campos["incluirInactivos"]); }
                catch { incluirInactivos = false; }

                return ToJson(NegVentanaAprobacion.ListarJefes(filtro, incluirInactivos));
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

        private string ExcluirJefe(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();

            try
            {
                string mailJefe = Convert.ToString(campos["mailJefe"]).Trim();

                bool excluir = false;
                try { excluir = Convert.ToBoolean(campos["excluir"]); }
                catch { excluir = false; }

                string usuarioRegistro = "";
                try { usuarioRegistro = Convert.ToString(campos["usuarioRegistro"]).Trim(); }
                catch { usuarioRegistro = ""; }

                if (string.IsNullOrWhiteSpace(mailJefe))
                {
                    return responseMessage("0", "Debe seleccionar un jefe.", "warning");
                }

                respuesta = NegVentanaAprobacion.ExcluirJefe(mailJefe, excluir, usuarioRegistro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al actualizar el estado del jefe. " + ex.Message, "danger");
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
