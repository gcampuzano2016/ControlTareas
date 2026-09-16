using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetMenuPerfil
{
    /// <summary>
    /// Handler de la pantalla "Parametrización de menús por perfil".
    /// Acciones: ListaPerfiles, ListaMenuPerfil, GuardarMenuPerfil.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarMenuPerfil : IHttpHandler
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

                if (Action == "ListaPerfiles")
                {
                    existAction = true;
                    responseAction.Append(ListaPerfiles(parameters));
                }

                if (Action == "ListaMenuPerfil")
                {
                    existAction = true;
                    responseAction.Append(ListaMenuPerfil(parameters));
                }

                if (Action == "GuardarMenuPerfil")
                {
                    existAction = true;
                    responseAction.Append(GuardarMenuPerfil(parameters));
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

        private string ListaPerfiles(dynamic campos)
        {
            try
            {
                return ToJson(NegMenuPerfil.ListarPerfiles());
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los perfiles. " + ex.Message, "danger");
            }
        }

        private string ListaMenuPerfil(dynamic campos)
        {
            try
            {
                int idPerfil = 0;
                try { idPerfil = Convert.ToInt32(campos["idPerfil"]); }
                catch { idPerfil = 0; }

                if (idPerfil <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un perfil.", "warning");
                }

                return ToJson(NegMenuPerfil.ListarMenuPerfil(idPerfil));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los menús. " + ex.Message, "danger");
            }
        }

        private string GuardarMenuPerfil(dynamic campos)
        {
            try
            {
                int idPerfil = 0;
                try { idPerfil = Convert.ToInt32(campos["idPerfil"]); }
                catch { idPerfil = 0; }

                if (idPerfil <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un perfil.", "warning");
                }

                // 'activos' es un arreglo de ids (enteros). Se arma un CSV validado.
                List<string> ids = new List<string>();
                try
                {
                    var activos = campos["activos"];
                    if (activos != null)
                    {
                        foreach (var v in activos)
                        {
                            int id = Convert.ToInt32(v);
                            if (id > 0) { ids.Add(id.ToString()); }
                        }
                    }
                }
                catch { ids = new List<string>(); }

                string csv = string.Join(",", ids);

                EntRespuesta respuesta = NegMenuPerfil.GuardarMenuPerfil(idPerfil, csv);
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar los menús del perfil. " + ex.Message, "danger");
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
