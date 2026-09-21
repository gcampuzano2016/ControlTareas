using CapaEntidad;
using CapaNegocio;
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
    /// Acciones: BuscarUsuarios, ListarDepartamentos, GuardarUsuario, VerBitacora.
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

                if (Action == "ListarDepartamentos")
                {
                    existAction = true;
                    responseAction.Append(ListarDepartamentos());
                }

                if (Action == "ListarPerfiles")
                {
                    existAction = true;
                    responseAction.Append(ListarPerfiles(context));
                }

                if (Action == "CambiarPerfilUsuario")
                {
                    existAction = true;
                    responseAction.Append(CambiarPerfilUsuario(context, parameters));
                }

                if (Action == "GuardarUsuario")
                {
                    existAction = true;
                    responseAction.Append(GuardarUsuario(context, parameters));
                }

                if (Action == "CambiarEstadoUsuario")
                {
                    existAction = true;
                    responseAction.Append(CambiarEstadoUsuario(context, parameters));
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

        private string ListarDepartamentos()
        {
            try
            {
                return ToJson(NegUsuarioAdmin.ListarDepartamentos());
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los departamentos. " + ex.Message, "danger");
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

        /// <summary>Las diez claves que debe traer todo GuardarUsuario, aunque su valor venga vacío.</summary>
        private static readonly string[] ClavesUsuario = { "nombre", "correo", "cedula", "departamento", "empresa", "cargo", "codSap", "jefe", "correoJefe", "telefonosEmergencia" };

        private string GuardarUsuario(HttpContext context, dynamic campos)
        {
            try
            {
                decimal idUsuario = Numero(campos, "idUsuario");
                if (idUsuario <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                foreach (string clave in ClavesUsuario)
                {
                    if (!Existe(campos, clave))
                    {
                        return responseMessage("0", "Faltan datos del usuario: no se recibió el campo '" + clave + "'.", "warning");
                    }
                }

                EntUsuarioAdmin u = new EntUsuarioAdmin()
                {
                    Id_Usuario = idUsuario,
                    Nom_Usuario = Texto(campos, "nombre"),
                    E_Mail = Texto(campos, "correo"),
                    Cedula = Texto(campos, "cedula"),
                    Departamento = Texto(campos, "departamento"),
                    Empresa = Texto(campos, "empresa"),
                    Cargo = Texto(campos, "cargo"),
                    Cod_Sap = Texto(campos, "codSap"),
                    Cod_Jefe_Inm = Texto(campos, "jefe"),
                    MailCodJefeInm = Texto(campos, "correoJefe"),
                    TelefonosEmergencia = Texto(campos, "telefonosEmergencia")
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

        /// <summary>
        /// Inactiva o activa al usuario en los selectores del sistema.
        /// La clave 'inactivar' debe venir explícita: sin ella no se asume nada,
        /// porque el valor por defecto decidiría por su cuenta si alguien queda fuera.
        /// </summary>
        private string CambiarEstadoUsuario(HttpContext context, dynamic campos)
        {
            try
            {
                decimal idUsuario = Numero(campos, "idUsuario");
                if (idUsuario <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                if (!Existe(campos, "inactivar"))
                {
                    return responseMessage("0", "Falta indicar si se debe activar o inactivar.", "warning");
                }

                bool inactivar;
                if (!bool.TryParse(Texto(campos, "inactivar"), out inactivar))
                {
                    return responseMessage("0", "El valor de activar o inactivar no es válido.", "warning");
                }

                EntRespuesta respuesta = NegUsuarioAdmin.CambiarEstado(idUsuario, inactivar, UsuarioSesion(context));
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al cambiar el estado del usuario. " + ex.Message, "danger");
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
            if (u.Cargo.Length > 128) { return "El cargo no puede superar los 128 caracteres."; }
            if (u.Cod_Sap.Length > 50) { return "El código SAP no puede superar los 50 caracteres."; }
            if (u.Cod_Jefe_Inm.Length > 100) { return "El jefe inmediato no puede superar los 100 caracteres."; }
            if (u.MailCodJefeInm.Length > 100) { return "El correo del jefe no puede superar los 100 caracteres."; }
            if (u.TelefonosEmergencia.Length > 100) { return "Los teléfonos de emergencia no pueden superar los 100 caracteres."; }
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

        /// <summary>
        /// Solo Super Admin (18) puede cambiar el perfil de un usuario.
        ///
        /// Esta es la UNICA comprobacion de perfil del handler, y no es un
        /// descuido que las demas acciones no la tengan: lo que aquellas
        /// escriben son once campos de contacto, y el perfil es el que da
        /// acceso a todo. Sin esta lista, cualquiera de los usuarios con sesion
        /// podria ponerse el 18 con una peticion directa a este handler.
        /// </summary>
        private static readonly int[] PerfilesQueCambianPerfil = { 18 };

        private static bool EsSuperAdmin(HttpContext context)
        {
            int idPerfil;
            if (!int.TryParse(Convert.ToString(context.Session["Id_Perfil"]), out idPerfil)) { return false; }
            return Array.IndexOf(PerfilesQueCambianPerfil, idPerfil) >= 0;
        }

        /// <summary>
        /// Los perfiles que se pueden elegir, para el desplegable.
        ///
        /// Sale de Perfiles -via Sp_RTA_ListarPerfiles- y no de R_Perfil: en
        /// esta base conviven los dos catalogos y no dicen lo mismo. Manda
        /// Perfiles porque es contra el que une Sp_RTA_ListarUsuariosAdmin, o
        /// sea el nombre que esta misma pantalla ya muestra en la grilla; usar
        /// el otro haria que la columna y el desplegable se contradigan.
        ///
        /// Detras de la misma guarda que el cambio: a quien no puede cambiar
        /// perfiles no le hace falta el catalogo.
        /// </summary>
        private string ListarPerfiles(HttpContext context)
        {
            if (!EsSuperAdmin(context))
            {
                return responseMessage("0", "Su perfil no puede cambiar el perfil de otros usuarios.", "warning");
            }

            try
            {
                return ToJson(NegMenuPerfil.ListarPerfiles());
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al cargar los perfiles. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Cambia el perfil de un usuario ya creado.
        ///
        /// Tres barreras, y ninguna sobra: el perfil 18 aqui, que nadie se
        /// cambie a si mismo -aqui, donde vive la sesion, y otra vez en el
        /// procedimiento, que es llamable desde SSMS-, y que el perfil elegido
        /// exista, que solo la base puede saber.
        /// </summary>
        private string CambiarPerfilUsuario(HttpContext context, dynamic campos)
        {
            if (!EsSuperAdmin(context))
            {
                return responseMessage("0", "Su perfil no puede cambiar el perfil de otros usuarios.", "warning");
            }

            try
            {
                decimal idUsuario = Numero(campos, "idUsuario");
                if (idUsuario <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                decimal idPerfil = Numero(campos, "idPerfil");
                if (idPerfil <= 0)
                {
                    return responseMessage("0", "Debe elegir el perfil nuevo.", "warning");
                }

                /* Se corta aqui y no solo en el procedimiento para dar el mensaje
                   correcto: alla la comparacion es por Cod_Usuario y aqui por
                   Id_Usuario, que es lo que la pantalla manda. Las dos tienen que
                   estar; esta evita el viaje y aquella cubre a quien llame al
                   procedimiento por fuera de la aplicacion. */
                if (NegUsuarioAdmin.EsSuPropioUsuario(idUsuario, context.Session["Id_Usuario"]))
                {
                    return responseMessage("0",
                        "No puede cambiar su propio perfil. Pídaselo a otro administrador.", "warning");
                }

                EntRespuesta respuesta = NegUsuarioAdmin.CambiarPerfil(
                    idUsuario, Convert.ToInt64(idPerfil), UsuarioSesion(context));

                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al cambiar el perfil. " + ex.Message, "danger");
            }
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

        /// <summary>
        /// Distingue "la clave no vino en el payload" de "vino con valor vacío".
        /// Texto() no puede: colapsa ambos casos en "". Un valor vacío es válido
        /// (borra el campo a propósito); una clave ausente no lo es.
        /// </summary>
        private bool Existe(dynamic campos, string clave)
        {
            var diccionario = campos as System.Collections.Generic.IDictionary<string, object>;
            if (diccionario == null) { return false; }
            return diccionario.ContainsKey(clave);
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
