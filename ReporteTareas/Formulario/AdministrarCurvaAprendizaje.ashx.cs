using CapaEntidad;
using CapaNegocio;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace JsonJQueryNetCurvaAprendizaje
{
    /// <summary>
    /// Handler de las pantallas de Curva de Aprendizaje (CurvaColaboradores,
    /// CurvaColaboradorNuevo, CurvaColaboradorFicha, CurvaEvaluacion,
    /// CurvaDashboard).
    ///
    /// Asincrono (HttpTaskAsyncHandler) porque completar, editar y guardar un
    /// plan de accion llaman a la IA, que puede tardar; asi no retiene un hilo
    /// del pool mientras espera. IRequiresSessionState no es decorativo: sin
    /// el, context.Session es null.
    ///
    /// El perfil sale SIEMPRE de la sesion, nunca del cliente: el menu solo
    /// controla que se VEA la pantalla, no que se pueda LLAMAR al handler.
    ///
    /// Los mensajes de error que llegan al usuario son los de las reglas de
    /// negocio (CurvaReglaException). Un error tecnico se responde con un
    /// texto generico: el texto crudo de SQL Server no sale de aqui.
    /// </summary>
    public class AdministrarCurvaAprendizaje : HttpTaskAsyncHandler, System.Web.SessionState.IRequiresSessionState
    {
        /// <summary>
        /// Talento Humano (14) y Super Admin (18). La misma lista esta en
        /// docs/sql/2026-10-05-curva-aprendizaje-menu.sql: si cambia una,
        /// cambiar la otra. internal: las paginas y DescargarCurvaAprendizaje.ashx
        /// la reusan en vez de declarar la suya.
        /// </summary>
        internal static readonly int[] PerfilesAutorizados = { 14, 18 };

        internal static readonly JsonSerializerSettings Json = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-dd",
            NullValueHandling = NullValueHandling.Include
        };

        internal static bool Autorizado(HttpContext context)
        {
            if (context.Session == null || context.Session["UserLogin"] == null) return false;
            int idPerfil;
            return int.TryParse(Convert.ToString(context.Session["Id_Perfil"]), out idPerfil) && PerfilesAutorizados.Contains(idPerfil);
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            // Web.config escribe en windows-1252; sin esto cada tilde llega rota.
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "application/json";
            context.Response.Charset = "utf-8";

            string salida;
            if (context.Session == null || context.Session["UserLogin"] == null)
                salida = Mensaje("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger");
            else if (!Autorizado(context))
                salida = Mensaje("0", "No tiene permisos para esta pantalla.", "danger");
            else if (context.Request.ContentType == null || !context.Request.ContentType.Contains("json"))
                salida = Mensaje("0", "Petición no válida.", "danger");
            else
                salida = await Despachar(context).ConfigureAwait(true);

            context.Response.Write(salida);
        }

        private static async Task<string> Despachar(HttpContext context)
        {
            string accion = null;
            try
            {
                string cuerpo;
                using (var lector = new System.IO.StreamReader(context.Request.InputStream)) cuerpo = lector.ReadToEnd();
                var item = JArray.Parse(cuerpo).First as JObject;
                accion = (string)item["action"];
                var p = item["parameters"] as JObject ?? new JObject();

                switch (accion)
                {
                    case "listar":
                        {
                            var lista = NegCurvaOnboarding.ListarColaboradores();
                            return Ok(new
                            {
                                colaboradores = lista,
                                total = lista.Count,
                                activos = lista.Count(x => x.Estado == "activo"),
                                enCurso = lista.Count(x => x.ProcesoEstado == CurvaConstantes.ProcesoEnCurso)
                            });
                        }
                    case "buscarEmpleadoSistema":
                        return Ok(NegCurvaOnboarding.BuscarEmpleadoSistema((string)p["texto"]));
                    case "crearColaborador":
                        {
                            var fecha = NegCurvaValorRespuesta.Fecha((string)p["fecha_ingreso"]);
                            long idEmp;
                            var e = new EntCurvaEmpleado
                            {
                                NombreCompleto = (string)p["nombre_completo"],
                                Cargo = (string)p["cargo"],
                                Area = (string)p["area"],
                                Empresa = (string)p["empresa"],
                                UnidadNegocio = (string)p["unidad_negocio"],
                                FechaIngreso = fecha ?? default(DateTime),
                                IdEmpleado = long.TryParse((string)p["id_empleado"], out idEmp) && idEmp > 0 ? idEmp : (long?)null
                            };
                            int id = NegCurvaOnboarding.CrearColaborador(e, Usuario(context));
                            return Ok(new { id = id }, "Colaborador registrado. Se generaron las evaluaciones de la etapa de 30 días.");
                        }
                    case "ficha":
                        return Ok(NegCurvaOnboarding.Ficha(Entero(p["id"])));
                    case "cambiarEstado":
                        NegCurvaOnboarding.CambiarEstado(Entero(p["id"]), (string)p["estado"]);
                        return Ok(null, "Estado actualizado.");
                    case "eliminarColaborador":
                        NegCurvaOnboarding.EliminarColaborador(Entero(p["id"]));
                        return Ok(null, "Colaborador eliminado con todo su historial.");
                    case "cargarEvaluacion":
                        return Ok(NegCurvaOnboarding.CargarFormulario(Entero(p["id"]), false));
                    case "completar":
                        {
                            int emp = await NegCurvaOnboarding.CompletarAsync(Entero(p["id"]), (string)p["payload"]).ConfigureAwait(true);
                            return Ok(new { empleadoId = emp }, "Evaluación completada.");
                        }
                    case "editar":
                        {
                            int emp = await NegCurvaOnboarding.EditarAsync(Entero(p["id"]), (string)p["payload"]).ConfigureAwait(true);
                            return Ok(new { empleadoId = emp }, "Cambios guardados.");
                        }
                    case "eliminarEvaluacion":
                        NegCurvaOnboarding.EliminarEvaluacion(Entero(p["id"]));
                        return Ok(null, "Evaluación eliminada.");
                    case "guardarPlan":
                        await NegCurvaOnboarding.GuardarPlanAccionAsync(Entero(p["id"]), (string)p["plan"]).ConfigureAwait(true);
                        return Ok(null, "Plan de acción guardado.");
                    case "generarEnlace":
                        {
                            var token = NegCurvaOnboarding.GenerarEnlace(Entero(p["id"]));
                            return Ok(new { url = UrlRemota(context, token) });
                        }
                    case "dashboard":
                        return Ok(NegCurvaOnboarding.Dashboard());
                    default:
                        return Mensaje("0", "La acción solicitada no existe.", "danger");
                }
            }
            catch (CurvaReglaException ex)
            {
                return Mensaje("0", ex.Message, "warning");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("AdministrarCurvaAprendizaje[" + accion + "]: " + ex);
                return Mensaje("0", "Ocurrió un error al procesar la solicitud. Intente nuevamente.", "danger");
            }
        }

        /// <summary>
        /// El enlace de la evaluacion remota. CA.UrlBasePublica (appsettings.config)
        /// manda si esta configurada; si no, el mismo host y aplicacion de esta peticion.
        /// </summary>
        internal static string UrlRemota(HttpContext context, string token)
        {
            const string pagina = "/Formulario/CurvaEvaluacionRemota.aspx?token=";
            var basePublica = ConfigurationManager.AppSettings["CA.UrlBasePublica"];
            if (!string.IsNullOrWhiteSpace(basePublica)) return basePublica.TrimEnd('/') + pagina + token;
            return context.Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~" + pagina) + token;
        }

        private static string Usuario(HttpContext context)
        {
            return Convert.ToString(context.Session["Cod_Usuario"]).Trim();
        }

        private static int Entero(JToken t)
        {
            int v;
            if (t == null || !int.TryParse(t.ToString(), out v) || v <= 0) throw new CurvaReglaException("Parámetro no válido.");
            return v;
        }

        internal static string Ok(object resultado, string mensaje = null)
        {
            var r = new EntRespuesta { estado = "1", resultado = resultado, mensaje = mensaje, tipoMensaje = "success" };
            return JsonConvert.SerializeObject(r, Json);
        }

        internal static string Mensaje(string estado, string mensaje, string tipo)
        {
            var r = new EntRespuesta { estado = estado, mensaje = mensaje, tipoMensaje = tipo };
            return JsonConvert.SerializeObject(r, Json);
        }

        public override bool IsReusable { get { return false; } }
    }
}
