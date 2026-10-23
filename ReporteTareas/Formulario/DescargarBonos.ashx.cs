using CapaNegocio;
using ReporteTareas.clases;
using System;
using System.Globalization;
using System.Web;
using System.Web.SessionState;

namespace JsonJQueryNetBonos
{
    /// <summary>
    /// Excel de bonos para Nomina: GET DescargarBonos.ashx?idPeriodo=N.
    /// Handler aparte de AdministrarBonos.ashx (aquel responde JSON, esto escribe
    /// bytes). El archivo lleva cedulas y montos por persona: el permiso se
    /// comprueba AQUI (pantalla BonosReporte) y no solo en el boton.
    /// La bitacora del mes la maneja NegBono; aqui no se anota nada mas.
    /// </summary>
    public class DescargarBonos : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (!Permisos.Autenticado(context))
            {
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.", 401);
                return;
            }
            if (!AdministrarBonos.Interno(context) || !Permisos.PuedeEntrar(context, Pantallas.BonosReporte))
            {
                NoDisponible(context, "Su perfil no puede descargar este archivo.", 403);
                return;
            }

            int idPeriodo;
            if (!int.TryParse(context.Request.QueryString["idPeriodo"], NumberStyles.Integer, CultureInfo.InvariantCulture, out idPeriodo) || idPeriodo <= 0)
            {
                NoDisponible(context, "No se indicó qué mes descargar.", 400);
                return;
            }

            byte[] archivo;
            string nombre;
            /* Todo lo que toca la base va en este try; el End() de la respuesta
               buena queda FUERA a proposito (lanza ThreadAbortException). */
            try
            {
                archivo = NegBono.Excel(idPeriodo, out nombre);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                NoDisponible(context, "No se pudo generar el archivo. Intente nuevamente.", 500);
                return;
            }

            if (archivo == null)
            {
                NoDisponible(context, "No se encontró ese mes, o es un mes histórico sin archivo.", 404);
                return;
            }

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + nombre + "\"");
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.BinaryWrite(archivo);
            context.Response.End();
        }

        /// <summary>
        /// Texto plano, no una pagina de error. ContentEncoding fijo: Web.config
        /// declara windows-1252 y sin esto las tildes llegan rotas.
        /// </summary>
        private static void NoDisponible(HttpContext context, string mensaje, int statusCode)
        {
            context.Response.Clear();
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(mensaje);
        }

        public bool IsReusable { get { return false; } }
    }
}
