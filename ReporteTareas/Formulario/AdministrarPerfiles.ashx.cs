using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetPerfiles
{
    /// <summary>
    /// Handler de la pantalla "Administración de perfiles".
    /// Acciones: ListarPerfiles, GuardarPerfil, EliminarPerfil.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarPerfiles : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            // Sin sesión no se ejecuta nada: es una pantalla de administración.
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

                var parameters = parametros[0]["parameters"];
                var Action = parametros[0]["action"];
                bool existAction = false;

                if (Action == "ListarPerfiles")
                {
                    existAction = true;
                    responseAction.Append(ListarPerfiles(parameters));
                }

                if (Action == "GuardarPerfil")
                {
                    existAction = true;
                    responseAction.Append(GuardarPerfil(parameters));
                }

                if (Action == "EliminarPerfil")
                {
                    existAction = true;
                    responseAction.Append(EliminarPerfil(parameters));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }
            else
            {
                responseAction.Append(responseMessage("0", "Solicitud no válida.", "danger"));
            }

            context.Response.ContentType = "application/json";
            context.Response.Write(responseAction.ToString());
        }

        private string ListarPerfiles(dynamic campos)
        {
            try
            {
                return ToJson(NegPerfiles.ListarPerfilesAdmin(Texto(campos, "filtro")));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al listar los perfiles. " + ex.Message, "danger");
            }
        }

        private string GuardarPerfil(dynamic campos)
        {
            try
            {
                string nombre = Texto(campos, "nombrePerfil");
                if (nombre == string.Empty)
                {
                    return responseMessage("0", "Debe escribir el nombre del perfil.", "warning");
                }

                EntPerfiles perfil = new EntPerfiles();
                perfil.IdPerfil = Entero(campos, "idPerfil");
                perfil.NombrePerfil = nombre;
                perfil.Estado = Entero(campos, "estado");
                perfil.Fecha = DateTime.Now;

                // Id 0 significa alta; cualquier otro, modificación.
                if (perfil.IdPerfil == 0)
                {
                    // @Codigo no se fija acá: sin poder consultar Sp_RTAInsertaNuevoPerfil
                    // (no hay acceso a la base desde este entorno), no se sabe si el
                    // procedimiento lo usa con algún significado. Ver task-4-report.md.
                    return ToJson(NegPerfiles.RTAInsertarNuevoPerfil(perfil));
                }

                return ToJson(NegPerfiles.Sp_RTActualizarPerfil(perfil));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar el perfil. " + ex.Message, "danger");
            }
        }

        private string EliminarPerfil(dynamic campos)
        {
            try
            {
                int idPerfil = Entero(campos, "idPerfil");
                if (idPerfil <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un perfil.", "warning");
                }

                // Quien decide si se puede borrar es el SP. Acá no se repite la
                // validación: repetirla invita a que las dos versiones se separen.
                return ToJson(NegPerfiles.EliminarPerfil(idPerfil));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar el perfil. " + ex.Message, "danger");
            }
        }

        private string Texto(dynamic campos, string clave)
        {
            try
            {
                object valor = campos[clave];
                if (valor == null) { return string.Empty; }
                return Convert.ToString(valor).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private int Entero(dynamic campos, string clave)
        {
            try
            {
                int valor;
                if (!int.TryParse(Convert.ToString(campos[clave]), out valor)) { return 0; }
                return valor;
            }
            catch
            {
                return 0;
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
