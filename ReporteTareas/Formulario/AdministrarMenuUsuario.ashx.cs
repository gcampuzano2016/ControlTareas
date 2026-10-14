using CapaEntidad;
using CapaNegocio;
using ReporteTareas.clases;
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

            // Endpoint de control de accesos: sin sesión válida y sin permiso a la pantalla no se ejecuta ninguna acción.
            if (!Permisos.Autenticado(context))
            {
                responseAction.Append(responseMessage("0", "Su sesión ha expirado. Vuelva a iniciar sesión.", "danger"));
            }
            else if (!Permisos.PuedeEntrar(context, Pantallas.ParametrizacionMenuUsuario))
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

                if (Action == "ListaAccionesUsuario")
                {
                    existAction = true;
                    responseAction.Append(ListaAccionesUsuario(parameters));
                }

                if (Action == "GuardarPermisosUsuario")
                {
                    existAction = true;
                    responseAction.Append(GuardarPermisosUsuario(context, parameters));
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
                // Un arreglo vacío es válido (quita todos los extras); lo que NO es
                // válido es que falte la clave, o que algún elemento no sea un entero
                // positivo. En ninguno de esos dos casos se debe guardar nada: un
                // elemento inválido no puede vaciar en silencio la lista ya acumulada.
                object extras;
                try
                {
                    extras = campos["extras"];
                }
                catch
                {
                    extras = null;
                }

                if (extras == null)
                {
                    return responseMessage("0", "Falta el listado de módulos extra.", "warning");
                }

                // Debe ser un arreglo de verdad. Un string también implementa IEnumerable,
                // así que sin esta comprobación {"extras":""} se iteraría como cero
                // elementos y borraría en silencio todos los extras del usuario, y
                // {"extras":"12"} activaría los módulos 1 y 2 carácter por carácter.
                System.Collections.IEnumerable listaExtras = extras as System.Collections.IEnumerable;
                if (listaExtras == null || extras is string)
                {
                    return responseMessage("0", "El listado de módulos extra no es válido.", "warning");
                }

                List<string> ids = new List<string>();
                foreach (var v in listaExtras)
                {
                    int id;
                    if (!int.TryParse(Convert.ToString(v), out id) || id <= 0)
                    {
                        return responseMessage("0", "El listado de módulos extra contiene un valor inválido.", "warning");
                    }

                    ids.Add(id.ToString());
                }

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

        private string ListaAccionesUsuario(dynamic campos)
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

                return ToJson(NegMenuUsuario.ListarAccionesUsuario(codUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar las acciones. " + ex.Message, "danger");
            }
        }

        private string GuardarPermisosUsuario(HttpContext context, dynamic campos)
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

                if (!TieneClave(campos as object, "extras") || !TieneClave(campos as object, "acciones"))
                {
                    return responseMessage("0", "Faltan datos para guardar los permisos.", "warning");
                }

                string extrasCsv;
                if (!TryArmarCsv(campos as object, "extras", out extrasCsv))
                {
                    return responseMessage("0", "El listado de módulos contiene un valor inválido.", "warning");
                }

                string accionesCsv;
                if (!TryArmarCsv(campos as object, "acciones", out accionesCsv))
                {
                    return responseMessage("0", "El listado de acciones contiene un valor inválido.", "warning");
                }

                // Quien asigna sale de la sesión, nunca del cliente.
                string usuarioRegistro = "SISTEMA";
                if (context.Session != null && context.Session["Cod_Usuario"] != null)
                {
                    usuarioRegistro = context.Session["Cod_Usuario"].ToString();
                }

                EntRespuesta respuesta = NegMenuUsuario.GuardarPermisosUsuario(codUsuario, extrasCsv, accionesCsv, usuarioRegistro);
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar los permisos del usuario. " + ex.Message, "danger");
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
