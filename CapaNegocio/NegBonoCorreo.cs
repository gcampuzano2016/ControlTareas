using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CapaNegocio
{
    /// <summary>
    /// Correos del modulo de bonos. Clase pura: arma destinatarios, asuntos y
    /// cuerpos; el envio y la bitacora los hace NegBono. Todo valor que viene
    /// de la base o de una persona se escapa.
    /// </summary>
    public static class NegBonoCorreo
    {
        private static readonly CultureInfo Es = new CultureInfo("es-EC");
        private const int PerfilTalentoHumano = 14;

        /// <summary>Jefes con bonos en los 3 ultimos periodos Cerrado (Historico no cuenta) o con pendientes. Codigos normalizados y ordenados.</summary>
        public static List<string> JefesApertura(IList<EntBonoPeriodo> periodos, IList<EntBono> bonos, IList<EntBono> pendientes)
        {
            HashSet<int> ultimos = new HashSet<int>(
                (periodos ?? new List<EntBonoPeriodo>())
                    .Where(p => p != null && p.Estado == BonoPeriodoEstados.Cerrado)
                    .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
                    .Take(3)
                    .Select(p => p.IdPeriodo));

            IEnumerable<string> deBonos = (bonos ?? new List<EntBono>())
                .Where(b => b != null && b.IdPeriodo.HasValue && ultimos.Contains(b.IdPeriodo.Value))
                .Select(b => NegBonoReglas.Normalizar(b.CodJefe));
            IEnumerable<string> dePendientes = (pendientes ?? new List<EntBono>())
                .Where(b => b != null)
                .Select(b => NegBonoReglas.Normalizar(b.CodJefe));

            return deBonos.Concat(dePendientes)
                .Where(c => c.Length > 0)
                .Distinct()
                .OrderBy(c => c, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>Usuarios con perfil 14 y correo, sin repetidos, ordenados.</summary>
        public static List<string> DestinatariosEnvio(IEnumerable<EntBonoPersona> personas)
        {
            return (personas ?? new List<EntBonoPersona>())
                .Where(p => p != null && p.IdPerfil == PerfilTalentoHumano && !string.IsNullOrWhiteSpace(p.Correo))
                .Select(p => p.Correo.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // Separadores fijos ("$1.234,50"): el es-EC del servidor no es confiable para esto.
        private static readonly NumberFormatInfo Moneda = new NumberFormatInfo
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = ".",
            NumberGroupSizes = new[] { 3 }
        };

        private static string Dinero(decimal v)
        {
            return "$" + v.ToString("N2", Moneda);
        }

        private static string Mes(int anio, int mes)
        {
            return Es.DateTimeFormat.GetMonthName(mes).ToLower(Es) + " " + anio.ToString(CultureInfo.InvariantCulture);
        }

        public static string AsuntoApertura(int anio, int mes)
        {
            return "Bonos " + Mes(anio, mes) + ": período abierto";
        }

        public static string CuerpoApertura(int anio, int mes, string url)
        {
            StringBuilder sb = Abrir();
            sb.Append("<p>Hola,</p>");
            sb.Append("<p>Ya est&aacute; abierto el per&iacute;odo de bonos de <b>").Append(H(Mes(anio, mes))).Append("</b>. ");
            sb.Append("Proponga los bonos de su gente y env&iacute;elos a Talento Humano.</p>");
            return Cerrar(sb, url, "Proponer bonos");
        }

        private static string Linea(string s)
        {
            return System.Text.RegularExpressions.Regex.Replace(s ?? "", @"[\r\n]+", " ").Trim();
        }

        public static string AsuntoEnvio(string jefe, int cantidad, decimal total, string url)
        {
            return "Bonos: " + Linea(jefe) + " envió " + cantidad.ToString(CultureInfo.InvariantCulture) + " bono(s) para revisar";
        }

        public static string CuerpoEnvio(string jefe, int cantidad, decimal total, string url)
        {
            StringBuilder sb = Abrir();
            sb.Append("<p><b>").Append(H(jefe)).Append("</b> envi&oacute; bonos para su revisi&oacute;n.</p>");
            sb.Append("<p>Cantidad: <b>").Append(cantidad.ToString(CultureInfo.InvariantCulture)).Append("</b><br/>");
            sb.Append("Total: <b>").Append(H(Dinero(total))).Append("</b></p>");
            return Cerrar(sb, url, "Revisar bonos");
        }

        public static string AsuntoRechazo(EntBono b, string url)
        {
            return "Bonos: se rechazó un bono de " + Linea(b == null ? "" : b.Nombre);
        }

        public static string CuerpoRechazo(EntBono b, string url)
        {
            b = b ?? new EntBono();
            StringBuilder sb = Abrir();
            sb.Append("<p>Talento Humano rechaz&oacute; un bono que usted propuso:</p>");
            sb.Append("<p>Persona: <b>").Append(H(b.Nombre)).Append("</b><br/>");
            sb.Append("Tipo: <b>").Append(H(b.NombreTipo)).Append("</b><br/>");
            sb.Append("Valor: <b>").Append(H(Dinero(b.Valor))).Append("</b><br/>");
            sb.Append("Motivo: <b>").Append(H(b.MotivoRechazo)).Append("</b></p>");
            return Cerrar(sb, url, "Corregir en Proponer bonos");
        }

        private static StringBuilder Abrir()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#222\">");
            return sb;
        }

        private static string Cerrar(StringBuilder sb, string url, string texto)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                sb.Append("<p><a href=\"").Append(H(url.Trim()))
                  .Append("\" style=\"display:inline-block;padding:10px 22px;background:#1F3864;color:#ffffff;text-decoration:none;font-weight:bold\">")
                  .Append(H(texto)).Append("</a></p>");
            }
            sb.Append("<p style=\"font-size:11px;color:#777\">Correo autom&aacute;tico del Sistema de Tareas. No responder.</p>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string H(string s)
        {
            // WebUtility.HtmlEncode vuelve entidades numericas las letras con tilde
            // (&#209;); aqui solo se escapa lo que rompe el HTML y las tildes quedan.
            return (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                            .Replace("\"", "&quot;").Replace("'", "&#39;");
        }
    }
}
