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
    /// Lo que se descarga de una compra de vacaciones, desde Ajustes:
    ///   GET ?id=IdColaborador&amp;mov=IdMovimiento&amp;que=aviso      aviso a Nomina en PDF
    ///   GET ?id=IdColaborador&amp;mov=IdMovimiento&amp;que=documento  el documento firmado que subio GTH
    ///
    /// Mismo permiso que la pantalla de ajustes. El documento vive en la base
    /// (Vac_CompraDocumento) y se busca por colaborador Y compra: un mov de otra
    /// persona no devuelve nada.
    /// </summary>
    public class DescargarVacCompra : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (!Permisos.Autenticado(context))
            {
                NoDisponible(context, 401, "Su sesión expiró. Vuelva a iniciar sesión.");
                return;
            }
            if (!Permisos.PuedeEntrar(context, Pantallas.VacacionesAjustes))
            {
                NoDisponible(context, 403, "No tiene permisos para esta pantalla.");
                return;
            }

            int id, mov;
            if (!int.TryParse(context.Request.QueryString["id"], NumberStyles.None, CultureInfo.InvariantCulture, out id) || id <= 0
                || !int.TryParse(context.Request.QueryString["mov"], NumberStyles.None, CultureInfo.InvariantCulture, out mov) || mov <= 0)
            {
                NoDisponible(context, 400, "Compra no válida.");
                return;
            }
            string que = (context.Request.QueryString["que"] ?? "").Trim();

            byte[] bytes;
            string nombre, tipo;
            try
            {
                if (que == "documento")
                {
                    EntVacArchivo d = NegVacAjustes.Documento(id, mov);
                    if (d == null)
                    {
                        NoDisponible(context, 404, "La compra no tiene documento adjunto.");
                        return;
                    }
                    bytes = d.Contenido;
                    tipo = d.TipoContenido;
                    nombre = "Compra_" + mov.ToString(CultureInfo.InvariantCulture) + "_" + d.NombreArchivo;
                }
                else if (que == "aviso")
                {
                    string usuario = Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture).Trim();
                    EntVacCompraAviso a = NegVacCompra.Aviso(id, mov, DateTime.Today, usuario);
                    string error = NegVacCompra.PuedeEmitir(a);
                    if (error != "")
                    {
                        NoDisponible(context, a == null ? 404 : 409, error);
                        return;
                    }
                    string html = NegVacCompra.Html(a, CertificadoImagenes.MembreteDos(context), CertificadoImagenes.LogoAgility(context));
                    bytes = PdfHojaVida.Generar(html);
                    tipo = "application/pdf";
                    nombre = "Compra_Vacaciones_" + SoloAlfanumerico(a.Colaborador.Cedula) + "_registro"
                           + mov.ToString(CultureInfo.InvariantCulture) + ".pdf";
                }
                else
                {
                    NoDisponible(context, 400, "Indique qué descargar.");
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                NoDisponible(context, 500, "No se pudo generar el archivo. Intente nuevamente.");
                return;
            }

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = tipo;
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + NombreSeguro(nombre) + "\"");
            context.Response.BinaryWrite(bytes);
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

        /// <summary>Sin comillas ni caracteres de control: va dentro de una cabecera.</summary>
        private static string NombreSeguro(string nombre)
        {
            StringBuilder s = new StringBuilder();
            foreach (char c in nombre ?? "")
            {
                if (char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-') { s.Append(c < 128 ? c : '_'); }
                else { s.Append('_'); }
            }
            return s.Length == 0 ? "documento" : s.ToString();
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
