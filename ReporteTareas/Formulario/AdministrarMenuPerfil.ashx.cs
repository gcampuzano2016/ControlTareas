using CapaEntidad;
using CapaNegocio;
using ReporteTareas.clases;
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
    public class AdministrarMenuPerfil : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            if (!Permisos.Autenticado(context))
            {
                responseAction.Append(responseMessage("0", "Su sesión ha expirado. Vuelva a iniciar sesión.", "danger"));
            }
            else if (!Permisos.PuedeEntrar(context, Pantallas.ParametrizacionMenuPerfil))
            {
                responseAction.Append(responseMessage("0", "No tiene permisos para esta pantalla.", "danger"));
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
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

                if (Action == "ListaAccionesPerfil")
                {
                    existAction = true;
                    responseAction.Append(ListaAccionesPerfil(parameters));
                }

                if (Action == "GuardarPermisosPerfil")
                {
                    existAction = true;
                    responseAction.Append(GuardarPermisosPerfil(context, parameters));
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

        private string ListaAccionesPerfil(dynamic campos)
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

                return ToJson(NegMenuPerfil.ListarAccionesPerfil(idPerfil));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar las acciones. " + ex.Message, "danger");
            }
        }

        private string GuardarPermisosPerfil(HttpContext context, dynamic campos)
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

                if (!TieneClave(campos as object, "activos") || !TieneClave(campos as object, "acciones"))
                {
                    return responseMessage("0", "Faltan datos para guardar los permisos.", "warning");
                }

                string menusCsv;
                if (!TryArmarCsv(campos as object, "activos", out menusCsv))
                {
                    return responseMessage("0", "El listado de módulos contiene un valor inválido.", "warning");
                }

                string accionesCsv;
                if (!TryArmarCsv(campos as object, "acciones", out accionesCsv))
                {
                    return responseMessage("0", "El listado de acciones contiene un valor inválido.", "warning");
                }

                // Quien guarda sale de la sesion, nunca del cliente.
                string usuarioRegistro = "SISTEMA";
                if (context.Session != null && context.Session["Cod_Usuario"] != null)
                {
                    usuarioRegistro = context.Session["Cod_Usuario"].ToString();
                }

                EntRespuesta respuesta = NegMenuPerfil.GuardarPermisosPerfil(idPerfil, menusCsv, accionesCsv, usuarioRegistro);
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar los permisos del perfil. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Indica si la clave llego con un valor (una lista vacia explicita si cuenta).
        /// </summary>
        private static bool TieneClave(object campos, string clave)
        {
            IDictionary<string, object> diccionario = campos as IDictionary<string, object>;
            object valor;
            return diccionario != null && diccionario.TryGetValue(clave, out valor) && valor != null;
        }

        /// <summary>
        /// Arma un CSV de enteros positivos con la lista que llega bajo la clave. Una clave que
        /// no llega es una lista vacia; un valor que no es arreglo, o con un elemento que no es
        /// entero positivo, es invalido y no se guarda nada.
        /// </summary>
        private static bool TryArmarCsv(object campos, string clave, out string csv)
        {
            csv = string.Empty;

            IDictionary<string, object> diccionario = campos as IDictionary<string, object>;
            object valor;
            if (diccionario == null || !diccionario.TryGetValue(clave, out valor) || valor == null)
            {
                return true;
            }

            System.Collections.IEnumerable lista = valor as System.Collections.IEnumerable;
            if (lista == null || valor is string)
            {
                return false;
            }

            List<string> ids = new List<string>();
            foreach (var v in lista)
            {
                int id;
                if (!int.TryParse(Convert.ToString(v), out id) || id <= 0)
                {
                    return false;
                }

                ids.Add(id.ToString());
            }

            csv = string.Join(",", ids);
            return true;
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
