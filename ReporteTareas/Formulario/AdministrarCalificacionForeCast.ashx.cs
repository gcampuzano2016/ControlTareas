using CapaEntidad;
using CapaNegocio;
using SeguridadAppHelper;
using System;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetCalificacionForeCast
{
    /// <summary>
    /// Handler de la pantalla "Calificacion ForeCast".
    /// Acciones: ListaCalificacionForeCast, GuardarCalificacionForeCast.
    ///
    /// El usuario sale SIEMPRE de "session" (Cod_Usuario encriptado) y nunca de
    /// un parametro en claro: es lo que decide la visibilidad en Sp_RTAListaForeCast.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarCalificacionForeCast : IHttpHandler
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

                if (Action == "ListaCalificacionForeCast")
                {
                    existAction = true;
                    responseAction.Append(Lista(parameters));
                }

                if (Action == "GuardarCalificacionForeCast")
                {
                    existAction = true;
                    responseAction.Append(Guardar(parameters));
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

        private string Lista(dynamic campos)
        {
            try
            {
                string idUsuario = Usuario(campos);
                if (string.IsNullOrWhiteSpace(idUsuario))
                {
                    return responseMessage("0", "Sesión no válida. Vuelva a iniciar sesión.", "warning");
                }

                EntFiltroCalificacion filtro = new EntFiltroCalificacion
                {
                    GerentedeCuenta = Campo(campos, "gerente"),
                    Sucursal = Campo(campos, "sucursal"),
                    Prioridad = Campo(campos, "etapa"),
                    Estado = Campo(campos, "estado"),
                    Marca = Campo(campos, "marca")
                };

                EntPantallaCalificacion pantalla = NegCalificacionForeCast.Cargar(idUsuario, filtro);
                return ToJson(new EntRespuesta { estado = "1", resultado = pantalla, mensaje = "", tipoMensaje = "success" });
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar la calificación. " + ex.Message, "danger");
            }
        }

        private string Guardar(dynamic campos)
        {
            try
            {
                string idUsuario = Usuario(campos);
                if (string.IsNullOrWhiteSpace(idUsuario))
                {
                    return responseMessage("0", "Sesión no válida. Vuelva a iniciar sesión.", "warning");
                }

                int presupuesto, autoridad, necesidad;
                long idForeCast;
                // "|" (no "||"): fuerza a evaluar los 4 TryParse siempre. Con "||" el
                // compilador no puede probar que presupuesto/autoridad/necesidad quedan
                // asignados en la rama en que el if es falso (CS0165) en una cadena de
                // mas de dos TryParse encadenados con "out".
                if (!long.TryParse(Campo(campos, "idForeCast"), out idForeCast)
                    | !int.TryParse(Campo(campos, "presupuesto"), out presupuesto)
                    | !int.TryParse(Campo(campos, "autoridad"), out autoridad)
                    | !int.TryParse(Campo(campos, "necesidad"), out necesidad))
                {
                    return responseMessage("0", "Responda Presupuesto, Autoridad y Necesidad.", "warning");
                }

                DateTime fecha;
                DateTime? fechaAccion = null;
                if (DateTime.TryParseExact(Campo(campos, "fechaAccion"), "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                {
                    fechaAccion = fecha;
                }

                EntCalificacionRespuesta r = new EntCalificacionRespuesta
                {
                    IdForeCast = idForeCast,
                    Presupuesto = presupuesto,
                    Autoridad = autoridad,
                    Necesidad = necesidad,
                    DecisorCargo = Campo(campos, "decisorCargo"),
                    PropuestaEnviada = Campo(campos, "propuestaEnviada") == "1",
                    AccionComprometida = Campo(campos, "accionComprometida"),
                    FechaAccion = fechaAccion
                };

                return ToJson(NegCalificacionForeCast.Guardar(r, idUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar la calificación. " + ex.Message, "danger");
            }
        }

        private static string Usuario(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            return seguridad.Desencripta(Campo(campos, "session"));
        }

        private static string Campo(dynamic campos, string nombre)
        {
            try
            {
                object v = campos[nombre];
                return v == null ? "" : Convert.ToString(v).Trim();
            }
            catch
            {
                return "";
            }
        }

        private string responseMessage(string estado, string mensaje, string tipoMensaje, string resultado = "")
        {
            return ToJson(new EntRespuesta { estado = estado, mensaje = mensaje, tipoMensaje = tipoMensaje, resultado = resultado });
        }

        /// <summary>
        /// JavaScriptSerializer escapa las tildes como \uXXXX: evita el problema
        /// de codificacion del helper compartido (Encoding.Default).
        /// </summary>
        private static string ToJson(object obj)
        {
            if (obj == null) { return string.Empty; }
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
