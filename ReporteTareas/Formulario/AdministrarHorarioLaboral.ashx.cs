using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetHorarioLaboral
{
    /// <summary>
    /// Handler de la pantalla "Catalogo de horarios".
    /// Acciones: ListaHorarios, ObtenerHorario, GuardarHorario, CambiarEstadoHorario,
    ///           GuardarHorarioPropio.
    /// La asignacion de un perfil a un usuario sigue en AdministrarHorarioUsuario.ashx.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarHorarioLaboral : IHttpHandler
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

                if (Action == "ListaHorarios")
                {
                    existAction = true;
                    responseAction.Append(ListaHorarios(parameters));
                }

                if (Action == "ObtenerHorario")
                {
                    existAction = true;
                    responseAction.Append(ObtenerHorario(parameters));
                }

                if (Action == "GuardarHorario")
                {
                    existAction = true;
                    responseAction.Append(GuardarHorario(parameters));
                }

                if (Action == "CambiarEstadoHorario")
                {
                    existAction = true;
                    responseAction.Append(CambiarEstadoHorario(parameters));
                }

                if (Action == "GuardarHorarioPropio")
                {
                    existAction = true;
                    responseAction.Append(GuardarHorarioPropio(parameters));
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

        private string ListaHorarios(dynamic campos)
        {
            try
            {
                string filtro = LeerTexto(campos, "filtro");
                bool incluirInactivos = LeerBandera(campos, "incluirInactivos");
                bool incluirPropios = LeerBandera(campos, "incluirPropios");

                return ToJson(NegHorarioLaboral.ListarHorarios(filtro, incluirInactivos, incluirPropios));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los horarios. " + ex.Message, "danger");
            }
        }

        private string ObtenerHorario(dynamic campos)
        {
            try
            {
                int idHorario = LeerEntero(campos, "idHorario");

                return ToJson(NegHorarioLaboral.ObtenerHorario(idHorario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar el detalle del horario. " + ex.Message, "danger");
            }
        }

        private string GuardarHorario(dynamic campos)
        {
            try
            {
                EntHorarioLaboral horario = new EntHorarioLaboral()
                {
                    IdHorarioLaboral = LeerEntero(campos, "idHorario"),
                    Codigo = LeerTexto(campos, "codigo"),
                    Nombre = LeerTexto(campos, "nombre"),
                    EsPredeterminado = LeerBandera(campos, "esPredeterminado") ? 1 : 0,
                    Activo = LeerBandera(campos, "activo") ? 1 : 0
                };

                List<EntHorarioLaboralDetalle> dias = LeerDias(campos);

                if (dias.Count == 0)
                {
                    return responseMessage("0", "Debe indicar el horario de los siete días de la semana.", "warning");
                }

                EntRespuesta respuesta = NegHorarioLaboral.GuardarHorario(
                    horario,
                    dias,
                    LeerTexto(campos, "usuarioRegistro"),
                    LeerBandera(campos, "confirmaSobrescribir"));

                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar el horario. " + ex.Message, "danger");
            }
        }

        private string CambiarEstadoHorario(dynamic campos)
        {
            try
            {
                int idHorario = LeerEntero(campos, "idHorario");

                if (idHorario <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un horario.", "warning");
                }

                EntRespuesta respuesta = NegHorarioLaboral.CambiarEstadoHorario(
                    idHorario,
                    LeerBandera(campos, "activo"),
                    LeerTexto(campos, "usuarioRegistro"));

                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al cambiar el estado del horario. " + ex.Message, "danger");
            }
        }

        private string GuardarHorarioPropio(dynamic campos)
        {
            try
            {
                string codUsuario = LeerTexto(campos, "codUsuario");

                if (string.IsNullOrWhiteSpace(codUsuario))
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                List<EntHorarioLaboralDetalle> dias = LeerDias(campos);

                if (dias.Count == 0)
                {
                    return responseMessage("0", "Debe indicar el horario de los siete días de la semana.", "warning");
                }

                EntRespuesta respuesta = NegHorarioLaboral.GuardarHorarioPropio(
                    codUsuario,
                    dias,
                    LeerTexto(campos, "fechaDesde"),
                    LeerTexto(campos, "usuarioRegistro"));

                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar el horario propio. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Los dias llegan como arreglo dentro de parameters["dias"]:
        /// [{ ds: 1, lab: 1, ini: "08:30", fin: "17:30" }, ...]
        /// </summary>
        private static List<EntHorarioLaboralDetalle> LeerDias(dynamic campos)
        {
            List<EntHorarioLaboralDetalle> dias = new List<EntHorarioLaboralDetalle>();

            try
            {
                object[] crudos = campos["dias"] as object[];

                if (crudos == null) return dias;

                foreach (object crudo in crudos)
                {
                    var dia = (System.Collections.Generic.IDictionary<string, object>)crudo;

                    dias.Add(new EntHorarioLaboralDetalle()
                    {
                        DiaSemana = Convert.ToInt32(dia["ds"]),
                        EsLaborable = Convert.ToInt32(dia["lab"]),
                        HoraInicio = Convert.ToString(dia["ini"]),
                        HoraFin = Convert.ToString(dia["fin"])
                    });
                }
            }
            catch (Exception)
            {
                return new List<EntHorarioLaboralDetalle>();
            }

            return dias;
        }

        private static string LeerTexto(dynamic campos, string nombre)
        {
            try { return Convert.ToString(campos[nombre]).Trim(); }
            catch { return ""; }
        }

        private static int LeerEntero(dynamic campos, string nombre)
        {
            try
            {
                int valor;
                return int.TryParse(Convert.ToString(campos[nombre]), out valor) ? valor : 0;
            }
            catch { return 0; }
        }

        private static bool LeerBandera(dynamic campos, string nombre)
        {
            try
            {
                string valor = Convert.ToString(campos[nombre]).Trim();
                return valor == "1" || valor.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
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
        /// Serializa a JSON escapando los caracteres no ASCII (tildes, ñ) como \uXXXX,
        /// evitando el problema de codificación del helper compartido (Encoding.Default).
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
