using CapaEntidad;
using CapaNegocio;
using SeguridadAppHelper;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetUsuarios
{
    /// <summary>
    /// Handler de la pantalla "Administración de usuarios".
    /// Acciones: BuscarUsuarios, GuardarUsuario, RestablecerPassword, VerBitacora.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarUsuarios : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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

                if (Action == "BuscarUsuarios")
                {
                    existAction = true;
                    responseAction.Append(BuscarUsuarios(parameters));
                }

                if (Action == "GuardarUsuario")
                {
                    existAction = true;
                    responseAction.Append(GuardarUsuario(context, parameters));
                }

                if (Action == "RestablecerPassword")
                {
                    existAction = true;
                    responseAction.Append(RestablecerPassword(context, parameters));
                }

                if (Action == "VerBitacora")
                {
                    existAction = true;
                    responseAction.Append(VerBitacora(parameters));
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
                string filtro = Texto(campos, "filtro");
                return ToJson(NegUsuarioAdmin.ListarUsuarios(filtro));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al buscar usuarios. " + ex.Message, "danger");
            }
        }

        private string VerBitacora(dynamic campos)
        {
            try
            {
                decimal idUsuario = Numero(campos, "idUsuario");
                if (idUsuario <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                return ToJson(NegUsuarioAdmin.ListarBitacora(idUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar el historial. " + ex.Message, "danger");
            }
        }

        private string GuardarUsuario(HttpContext context, dynamic campos)
        {
            try
            {
                decimal idUsuario = Numero(campos, "idUsuario");
                if (idUsuario <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                EntUsuarioAdmin u = new EntUsuarioAdmin()
                {
                    Id_Usuario = idUsuario,
                    Nom_Usuario = Texto(campos, "nombre"),
                    E_Mail = Texto(campos, "correo"),
                    Cedula = Texto(campos, "cedula"),
                    Departamento = Texto(campos, "departamento"),
                    Empresa = Texto(campos, "empresa"),
                    Cod_Sap = Texto(campos, "codSap"),
                    Cod_Jefe_Inm = Texto(campos, "jefe"),
                    MailCodJefeInm = Texto(campos, "correoJefe")
                };

                string error = Validar(u);
                if (error != null)
                {
                    return responseMessage("0", error, "warning");
                }

                EntRespuesta respuesta = NegUsuarioAdmin.ActualizarUsuario(u, UsuarioSesion(context));
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar el usuario. " + ex.Message, "danger");
            }
        }

        private string RestablecerPassword(HttpContext context, dynamic campos)
        {
            try
            {
                decimal idUsuario = Numero(campos, "idUsuario");
                if (idUsuario <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                string clave = Texto(campos, "clave");
                if (clave.Length < 6)
                {
                    return responseMessage("0", "La contraseña debe tener al menos 6 caracteres.", "warning");
                }

                // El hash se calcula aquí, con la misma clase que usa el login.
                SeguridadHelper seguridad = new SeguridadHelper();
                string hash = seguridad.GetMd5Hash(clave);

                EntRespuesta respuesta = NegUsuarioAdmin.RestablecerPassword(idUsuario, hash, UsuarioSesion(context));
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al restablecer la contraseña. " + ex.Message, "danger");
            }
        }

        /// <summary>Validaciones de servidor. Devuelve null si todo está bien.</summary>
        private string Validar(EntUsuarioAdmin u)
        {
            if (u.Nom_Usuario.Length == 0) { return "El nombre del usuario es obligatorio."; }
            if (u.Nom_Usuario.Length > 100) { return "El nombre no puede superar los 100 caracteres."; }
            if (u.E_Mail.Length > 100) { return "El correo no puede superar los 100 caracteres."; }
            if (u.Cedula.Length > 32) { return "La cédula no puede superar los 32 caracteres."; }
            if (u.Departamento.Length > 128) { return "El departamento no puede superar los 128 caracteres."; }
            if (u.Empresa.Length > 50) { return "La empresa no puede superar los 50 caracteres."; }
            if (u.Cod_Sap.Length > 50) { return "El código SAP no puede superar los 50 caracteres."; }
            if (u.Cod_Jefe_Inm.Length > 100) { return "El jefe inmediato no puede superar los 100 caracteres."; }
            if (u.MailCodJefeInm.Length > 100) { return "El correo del jefe no puede superar los 100 caracteres."; }
            if (!CorreoValido(u.E_Mail)) { return "El correo no tiene un formato válido."; }
            if (!CorreoValido(u.MailCodJefeInm)) { return "El correo del jefe no tiene un formato válido."; }
            return null;
        }

        /// <summary>Vacío es válido: el correo es opcional.</summary>
        private bool CorreoValido(string correo)
        {
            if (string.IsNullOrEmpty(correo)) { return true; }
            return Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        /// <summary>Quién hace el cambio sale de la sesión, nunca del cliente.</summary>
        private string UsuarioSesion(HttpContext context)
        {
            if (context.Session != null && context.Session["Cod_Usuario"] != null)
            {
                return context.Session["Cod_Usuario"].ToString();
            }
            return "SISTEMA";
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

        private decimal Numero(dynamic campos, string clave)
        {
            try
            {
                decimal valor;
                if (!decimal.TryParse(Convert.ToString(campos[clave]), out valor)) { return 0; }
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
