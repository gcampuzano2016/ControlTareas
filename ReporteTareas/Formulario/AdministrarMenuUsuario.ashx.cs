using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetMenuUsuario
{
    /// <summary>
    /// Handler de la pantalla "Parametrización de módulos por usuario".
    /// Acciones: BuscarUsuarios, ListaMenuUsuario, GuardarMenuUsuario.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarMenuUsuario : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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

                if (Action == "BuscarUsuarios")
                {
                    existAction = true;
                    responseAction.Append(BuscarUsuarios(parameters));
                }

                if (Action == "ListaMenuUsuario")
                {
                    existAction = true;
                    responseAction.Append(ListaMenuUsuario(parameters));
                }

                if (Action == "GuardarMenuUsuario")
                {
                    existAction = true;
                    responseAction.Append(GuardarMenuUsuario(context, parameters));
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

        private string BuscarUsuarios(dynamic campos)
        {
            try
            {
                string filtro = "";
                try { filtro = Convert.ToString(campos["filtro"]); }
                catch { filtro = ""; }

                return ToJson(NegMenuUsuario.ListarUsuarios(filtro));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al buscar usuarios. " + ex.Message, "danger");
            }
        }

        private string ListaMenuUsuario(dynamic campos)
        {
            try
            {
                string codUsuario = "";
                try { codUsuario = Convert.ToString(campos["codUsuario"]); }
                catch { codUsuario = ""; }

                if (string.IsNullOrEmpty(codUsuario))
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                return ToJson(NegMenuUsuario.ListarMenuUsuario(codUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los módulos. " + ex.Message, "danger");
            }
        }

        private string GuardarMenuUsuario(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = "";
                try { codUsuario = Convert.ToString(campos["codUsuario"]); }
                catch { codUsuario = ""; }

                if (string.IsNullOrEmpty(codUsuario))
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                // 'extras' es un arreglo de ids (enteros). Se arma un CSV validado.
                List<string> ids = new List<string>();
                try
                {
                    var extras = campos["extras"];
                    if (extras != null)
                    {
                        foreach (var v in extras)
                        {
                            int id = Convert.ToInt32(v);
                            if (id > 0) { ids.Add(id.ToString()); }
                        }
                    }
                }
                catch { ids = new List<string>(); }

                string csv = string.Join(",", ids);

                // Quién asigna sale de la sesión, nunca del cliente.
                string usuarioRegistro = "SISTEMA";
                if (context.Session != null && context.Session["Cod_Usuario"] != null)
                {
                    usuarioRegistro = context.Session["Cod_Usuario"].ToString();
                }

                EntRespuesta respuesta = NegMenuUsuario.GuardarMenuUsuario(codUsuario, csv, usuarioRegistro);
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar los módulos del usuario. " + ex.Message, "danger");
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
