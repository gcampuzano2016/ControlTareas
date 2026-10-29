using CapaEntidad;
using CapaNegocio;
using ReporteTareas.clases;
using System;
using System.Globalization;
using System.Text;
using System.Web;

namespace JsonJQueryNetVacSaldos
{
    /// <summary>
    /// Estado individual de vacaciones en PDF, para imprimir y firmar.
    /// GET ?id=IdColaborador&amp;fecha=yyyy-MM-dd (fecha vacia = hoy).
    ///
    /// Mismo permiso que la pantalla de saldos (VacacionesSaldos.aspx). Cada
    /// PDF queda registrado en Vac_EstadoEmitido con los numeros que lleva
    /// impresos, y el folio va en la hoja: asi el papel firmado se puede
    /// contrastar con el sistema aunque despues cambie el saldo.
    /// </summary>
    public class DescargarVacEstado : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (!Permisos.Autenticado(context))
            {
                NoDisponible(context, 401, "Su sesión expiró. Vuelva a iniciar sesión.");
                return;
            }
            if (!AdministrarVacSaldos.Autorizado(context))
            {
                NoDisponible(context, 403, "No tiene permisos para esta pantalla.");
                return;
            }

            int id;
            if (!int.TryParse(context.Request.QueryString["id"], NumberStyles.None, CultureInfo.InvariantCulture, out id) || id <= 0)
            {
                NoDisponible(context, 400, "Colaborador no válido.");
                return;
            }

            string texto = (context.Request.QueryString["fecha"] ?? "").Trim();
            DateTime fecha = DateTime.Today;
            if (texto != "" && !DateTime.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                NoDisponible(context, 400, "Indique una fecha de cálculo válida.");
                return;
            }
            string error = NegVacSaldos.ValidarFecha(fecha, DateTime.Today);
            if (error != "")
            {
                NoDisponible(context, 400, error);
                return;
            }

            byte[] pdf;
            string nombre;
            try
            {
                string usuario = Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture).Trim();
                EntVacEstado e = NegVacEstado.Emitir(fecha, id, usuario, context.Request.UserHostAddress);
                if (e == null)
                {
                    NoDisponible(context, 404, "El colaborador no existe.");
                    return;
                }

                string html = NegVacEstado.Html(e, CertificadoImagenes.MembreteDos(context), CertificadoImagenes.LogoAgility(context));
                pdf = PdfHojaVida.Generar(html);
                nombre = "Estado_Vacaciones_" + SoloAlfanumerico(e.Ficha.Colaborador.Cedula) + "_"
                       + e.Ficha.Saldo.FechaCalculo.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                       + "_folio" + e.Folio.ToString(CultureInfo.InvariantCulture) + ".pdf";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                NoDisponible(context, 500, "No se pudo generar el estado de vacaciones. Intente nuevamente.");
                return;
            }

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/pdf";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + nombre + "\"");
            context.Response.BinaryWrite(pdf);
            context.Response.End();
        }

        private static void NoDisponible(HttpContext context, int codigo, string mensaje)
        {
            context.Response.Clear();
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.StatusCode = codigo;
            context.Response.ContentType = "text/plain";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Write(mensaje);
        }

        private static string SoloAlfanumerico(string texto)
        {
            StringBuilder limpio = new StringBuilder();
            foreach (char c in texto ?? "")
            {
                if (char.IsLetterOrDigit(c)) { limpio.Append(c); }
            }
            return limpio.Length == 0 ? "colaborador" : limpio.ToString();
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
