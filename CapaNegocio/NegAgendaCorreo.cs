using CapaEntidad;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace CapaNegocio
{
    /// <summary>
    /// El aviso por correo de la agenda. Clase pura: arma el texto; el envio y
    /// la bitacora los hace el handler.
    ///
    /// Solo se avisa cuando alguien asigna una actividad a OTRA persona. Una
    /// actividad propia, un cambio de fecha o una anulacion no envian nada.
    /// Todo lo que viene de la base o de la persona se escapa: el titulo lo
    /// escribe un usuario.
    /// </summary>
    public static class NegAgendaCorreo
    {
        private static readonly CultureInfo Es = new CultureInfo("es-EC");

        public static bool DebeAvisar(EntAgendaActividad a)
        {
            return a != null && !NegAgendaActividad.EsPropia(a);
        }

        public static string Asunto(EntAgendaActividad a)
        {
            string t = a == null ? "" : (a.Titulo ?? "");
            t = System.Text.RegularExpressions.Regex.Replace(t, @"[\r\n]+", " ").Trim();
            return "Nueva actividad asignada: " + t;
        }

        /// <summary>URL_SITE + /Formulario/Agenda.aspx; vacio si no hay URL configurada.</summary>
        public static string Enlace(string urlSitio)
        {
            if (string.IsNullOrWhiteSpace(urlSitio)) { return ""; }
            return urlSitio.Trim().TrimEnd('/') + "/Formulario/Agenda.aspx";
        }

        public static string Cuerpo(EntAgendaActividad a, string nombreResponsable, string nombreAsignador, string urlSitio)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#222\">");
            sb.Append("<p>Hola ").Append(H(nombreResponsable)).Append(",</p>");
            sb.Append("<p><b>").Append(H(nombreAsignador)).Append("</b> te asign&oacute; una actividad:</p>");
            sb.Append("<p style=\"font-size:16px\"><b>").Append(H(a.Titulo)).Append("</b></p>");
            sb.Append("<p>Fecha de entrega: <b>").Append(H(a.FechaEntrega.ToString("dddd d 'de' MMMM 'de' yyyy", Es))).Append("</b></p>");

            if (a.Entregables != null && a.Entregables.Count > 0)
            {
                sb.Append("<p>Entregables:</p><ul>");
                foreach (EntAgendaEntregable e in a.Entregables.OrderBy(x => x.Orden))
                {
                    sb.Append("<li>").Append(H(e.Descripcion)).Append("</li>");
                }
                sb.Append("</ul>");
            }

            string enlace = Enlace(urlSitio);
            if (enlace != "")
            {
                sb.Append("<p><a href=\"").Append(H(enlace))
                  .Append("\" style=\"display:inline-block;padding:10px 22px;background:#1F3864;color:#ffffff;text-decoration:none;font-weight:bold\">Ver en mi agenda</a></p>");
            }
            sb.Append("<p style=\"font-size:11px;color:#777\">Correo autom&aacute;tico del Sistema de Tareas. No responder.</p>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string H(string s)
        {
            return WebUtility.HtmlEncode(s ?? "");
        }
    }
}
