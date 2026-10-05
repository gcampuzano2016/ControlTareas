using CapaNegocio;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace JsonJQueryNetCurvaAprendizaje
{
    /// <summary>
    /// Informes PDF de Curva de Aprendizaje (especificacion, seccion 12):
    ///   ?tipo=evaluacion&amp;id=N                       informe individual
    ///   ?tipo=anual&amp;procesoId=N                     anual consolidado
    ///   ?tipo=rango&amp;procesoId=N&amp;desde=30&amp;hasta=90   consolidado por rango
    ///
    /// Asincrono: el anual genera su resumen con IA la primera vez (y lo
    /// guarda); el de rango lo genera en CADA descarga y no lo guarda.
    /// Misma barrera de sesion y perfil que AdministrarCurvaAprendizaje.
    /// </summary>
    public class DescargarCurvaAprendizaje : HttpTaskAsyncHandler, System.Web.SessionState.IRequiresSessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            if (context.Session == null || context.Session["UserLogin"] == null) { Error(context, 401, "Su sesión expiró. Vuelva a iniciar sesión."); return; }
            if (!AdministrarCurvaAprendizaje.Autorizado(context)) { Error(context, 403, "No tiene permisos para esta pantalla."); return; }

            byte[] pdf;
            string nombre;
            try
            {
                string logo = context.Server.MapPath("~/Img/logo_dos.png");
                string tipo = context.Request.QueryString["tipo"];
                if (tipo == "evaluacion")
                {
                    int id;
                    if (!int.TryParse(context.Request.QueryString["id"], out id)) { Error(context, 400, "Evaluación no encontrada"); return; }
                    var ev = BuscarEvaluacion(id);
                    if (ev == null) { Error(context, 404, "Evaluación no encontrada"); return; }
                    if (!ev.EstaCompletada) { Error(context, 400, "La evaluación aún no se ha completado"); return; }
                    var x = NegCurvaOnboarding.Expediente(ev.ProcesoId);
                    ev = x.Evaluaciones.First(e => e.Id == id);
                    pdf = NegCurvaReportes.Individual(x, ev, logo);
                    nombre = NegCurvaReportes.NombreIndividual(x, ev);
                }
                else if (tipo == "anual" || tipo == "rango")
                {
                    int procesoId;
                    var x = int.TryParse(context.Request.QueryString["procesoId"], out procesoId) ? NegCurvaOnboarding.Expediente(procesoId) : null;
                    if (x == null) { Error(context, 404, "Proceso no encontrado"); return; }
                    if (tipo == "anual")
                    {
                        string resumen = x.Proceso.ResumenEjecutivoAnual;
                        if (string.IsNullOrWhiteSpace(resumen)) resumen = await NegCurvaOnboarding.RegenerarResumenAnualAsync(procesoId).ConfigureAwait(true);
                        pdf = NegCurvaReportes.Consolidado(x, null, resumen, logo);
                        nombre = NegCurvaReportes.NombreAnual(x);
                    }
                    else
                    {
                        string desde = context.Request.QueryString["desde"], hasta = context.Request.QueryString["hasta"];
                        var etapas = NegCurvaReportes.EtapasDelRango(desde, hasta);
                        if (etapas == null) { Error(context, 400, "Debes indicar etapas \"desde\" y \"hasta\" válidas (30, 60, 90, 180, 365)"); return; }
                        string resumen = x.Evaluaciones.Any(e => e.EstaCompletada && etapas.Contains(e.EtapaCodigo))
                            ? await NegCurvaOnboarding.ResumenRangoAsync(x, etapas).ConfigureAwait(true) : null;
                        pdf = NegCurvaReportes.Consolidado(x, etapas, resumen, logo);
                        nombre = NegCurvaReportes.NombreRango(x, desde, hasta);
                    }
                }
                else { Error(context, 400, "Tipo de informe no válido."); return; }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("DescargarCurvaAprendizaje: " + ex);
                Error(context, 500, "No se pudo generar el informe. Intente nuevamente.");
                return;
            }

            context.Response.Clear();
            context.Response.ContentType = "application/pdf";
            context.Response.AddHeader("Content-Disposition", "inline; filename=\"" + nombre + "\"");
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.BinaryWrite(pdf);
        }

        private static CapaEntidad.EntCurvaEvaluacion BuscarEvaluacion(int id)
        {
            try { return NegCurvaOnboarding.CargarEvaluacionSimple(id); }
            catch (CurvaReglaException) { return null; }
        }

        private static void Error(HttpContext context, int codigo, string mensaje)
        {
            context.Response.Clear();
            context.Response.StatusCode = codigo;
            context.Response.ContentType = "text/plain";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(mensaje);
        }

        public override bool IsReusable { get { return false; } }
    }
}
