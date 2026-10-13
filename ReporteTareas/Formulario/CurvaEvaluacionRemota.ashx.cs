using CapaNegocio;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Web;

namespace JsonJQueryNetCurvaAprendizaje
{
    /// <summary>
    /// Handler ANONIMO de la evaluacion remota (CurvaEvaluacionRemota.aspx).
    /// La unica credencial es el token de 48 hex del enlace.
    ///
    /// Regla de oro (especificacion, seccion 11.3): de aqui solo sale nombre
    /// y cargo de la persona evaluada, etapa, rol, preguntas y compromisos
    /// abiertos del proceso. Nunca curva, alertas, resumen, plan de accion,
    /// conclusion, tokens ni respuestas guardadas. Eso lo garantiza
    /// NegCurvaOnboarding.CargarRemota (paraRemota = true), no esta clase:
    /// aqui no se arma ningun dato propio.
    ///
    /// Sin IRequiresSessionState a proposito: esta pagina no tiene sesion y no
    /// la necesita.
    /// </summary>
    public class CurvaEvaluacionRemota : HttpTaskAsyncHandler
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "application/json";
            context.Response.Charset = "utf-8";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            string salida;
            try
            {
                if (context.Request.ContentType == null || !context.Request.ContentType.Contains("json"))
                    throw new CurvaReglaException("Petición no válida.");
                string cuerpo;
                using (var lector = new System.IO.StreamReader(context.Request.InputStream)) cuerpo = lector.ReadToEnd();
                var item = JArray.Parse(cuerpo).First as JObject;
                var accion = (string)item["action"];
                var p = item["parameters"] as JObject ?? new JObject();
                var token = ((string)p["token"] ?? "").Trim().ToLowerInvariant();

                if (token.Length == 0)
                    salida = AdministrarCurvaAprendizaje.Ok(new { situacion = "incompleto" });
                else if (accion == "cargar")
                {
                    var datos = NegCurvaOnboarding.CargarRemota(token);
                    if (datos == null) salida = AdministrarCurvaAprendizaje.Ok(new { situacion = "invalido" });
                    else if (datos.Estado == CapaEntidad.CurvaConstantes.EvalCompletada) salida = AdministrarCurvaAprendizaje.Ok(new { situacion = "completada" });
                    else salida = AdministrarCurvaAprendizaje.Ok(new { situacion = "pendiente", formulario = datos });
                }
                else if (accion == "enviar")
                {
                    await NegCurvaOnboarding.CompletarRemotaAsync(token, (string)p["payload"]).ConfigureAwait(true);
                    salida = AdministrarCurvaAprendizaje.Ok(new { situacion = "enviada" });
                }
                else salida = AdministrarCurvaAprendizaje.Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (CurvaReglaException ex)
            {
                salida = AdministrarCurvaAprendizaje.Mensaje("0", ex.Message, "warning");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("CurvaEvaluacionRemota: " + ex);
                salida = AdministrarCurvaAprendizaje.Mensaje("0", "Ocurrió un problema al procesar la evaluación. Intenta nuevamente.", "danger");
            }
            context.Response.Write(salida);
        }

        public override bool IsReusable { get { return false; } }
    }
}
