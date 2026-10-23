using CapaEntidad;
using System;
using System.Collections.Generic;
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

        // ---------- Procesos entre areas (Entrega 2) ----------

        /// <summary>Se avisa al responsable salvo que sea quien inicio el proceso (o no tenga responsable).</summary>
        public static bool DebeAvisarPaso(EntAgendaProceso p, EntAgendaProcesoPaso paso)
        {
            if (p == null || paso == null) { return false; }
            string resp = NegAgendaEquipo.Normalizar(paso.CodResponsable);
            if (resp == "") { return false; }
            return !NegAgendaEquipo.Codigos.Equals(resp, NegAgendaEquipo.Normalizar(p.CodIniciadoPor));
        }

        public static string AsuntoPaso(EntAgendaProceso p, EntAgendaProcesoPaso paso)
        {
            return "Te toca un paso: " + UnaLinea(paso == null ? "" : paso.Titulo)
                + " \u2014 " + UnaLinea(p == null ? "" : p.Nombre);
        }

        public static string CuerpoPaso(EntAgendaProceso p, EntAgendaProcesoPaso paso, string nombreResponsable,
            string nombreIniciador, DateTime fechaEntrega, string urlSitio)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#222\">");
            sb.Append("<p>Hola ").Append(H(nombreResponsable)).Append(",</p>");
            sb.Append("<p>Te toca un paso del proceso <b>").Append(H(p.Nombre)).Append("</b>, iniciado por <b>")
              .Append(H(nombreIniciador)).Append("</b>:</p>");
            sb.Append("<p style=\"font-size:16px\"><b>Paso ").Append(paso.Orden).Append(": ").Append(H(paso.Titulo)).Append("</b></p>");
            if (!string.IsNullOrWhiteSpace(paso.Area))
            {
                sb.Append("<p>&Aacute;rea: <b>").Append(H(paso.Area)).Append("</b></p>");
            }
            sb.Append("<p>Fecha de entrega: <b>").Append(H(fechaEntrega.ToString("dddd d 'de' MMMM 'de' yyyy", Es))).Append("</b></p>");
            if (!string.IsNullOrWhiteSpace(paso.Entregables))
            {
                sb.Append("<p>Entregables:</p><p>").Append(H(paso.Entregables).Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br>")).Append("</p>");
            }
            AgregarBoton(sb, urlSitio, "Ver en mi agenda");
            AgregarPie(sb);
            return sb.ToString();
        }

        public static string AsuntoTerminado(EntAgendaProceso p)
        {
            return "Proceso terminado: " + UnaLinea(p == null ? "" : p.Nombre);
        }

        public static string CuerpoTerminado(EntAgendaProceso p, IDictionary<string, string> nombres, string urlSitio)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#222\">");
            sb.Append("<p>El proceso <b>").Append(H(p.Nombre)).Append("</b> que iniciaste termin&oacute;: todos sus pasos est&aacute;n completos.</p>");
            sb.Append("<table style=\"border-collapse:collapse;font-size:13px\">");
            sb.Append("<tr style=\"background:#1F3864;color:#ffffff\"><th style=\"padding:6px 10px;text-align:left\">#</th>")
              .Append("<th style=\"padding:6px 10px;text-align:left\">Paso</th>")
              .Append("<th style=\"padding:6px 10px;text-align:left\">Responsable</th>")
              .Append("<th style=\"padding:6px 10px;text-align:left\">Completado</th></tr>");
            foreach (EntAgendaProcesoPaso s in (p.Pasos ?? new List<EntAgendaProcesoPaso>()).OrderBy(x => x.Orden))
            {
                string cod = NegAgendaEquipo.Normalizar(s.CodResponsable);
                string nombre = cod;
                if (nombres != null)
                {
                    foreach (KeyValuePair<string, string> kv in nombres)
                    {
                        if (NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(kv.Key), cod)) { nombre = kv.Value; break; }
                    }
                }
                string fecha = s.FechaCompletado.HasValue ? s.FechaCompletado.Value.ToString("d 'de' MMMM 'de' yyyy", Es) : "";
                sb.Append("<tr><td style=\"padding:6px 10px;border-bottom:1px solid #ddd\">").Append(s.Orden).Append("</td>")
                  .Append("<td style=\"padding:6px 10px;border-bottom:1px solid #ddd\">").Append(H(s.Titulo)).Append("</td>")
                  .Append("<td style=\"padding:6px 10px;border-bottom:1px solid #ddd\">").Append(H(nombre)).Append("</td>")
                  .Append("<td style=\"padding:6px 10px;border-bottom:1px solid #ddd\">").Append(H(fecha)).Append("</td></tr>");
            }
            sb.Append("</table>");
            AgregarBoton(sb, urlSitio, "Ver en mi agenda");
            AgregarPie(sb);
            return sb.ToString();
        }

        private static string UnaLinea(string s)
        {
            return System.Text.RegularExpressions.Regex.Replace(s ?? "", @"[\r\n\t]+", " ").Trim();
        }

        private static void AgregarBoton(StringBuilder sb, string urlSitio, string texto)
        {
            string enlace = Enlace(urlSitio);
            if (enlace != "")
            {
                sb.Append("<p><a href=\"").Append(H(enlace))
                  .Append("\" style=\"display:inline-block;padding:10px 22px;background:#1F3864;color:#ffffff;text-decoration:none;font-weight:bold\">").Append(texto).Append("</a></p>");
            }
        }

        private static void AgregarPie(StringBuilder sb)
        {
            sb.Append("<p style=\"font-size:11px;color:#777\">Correo autom&aacute;tico del Sistema de Tareas. No responder.</p>");
            sb.Append("</div>");
        }

        private static string H(string s)
        {
            return WebUtility.HtmlEncode(s ?? "");
        }
    }
}
