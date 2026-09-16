using CapaEntidad;
using System.Text;

namespace CapaNegocio
{
    /// <summary>
    /// Arma el HTML del CV a partir del perfil ya cargado.
    ///
    /// Vive en esta capa y no junto al generador de PDF por dos razones. La
    /// primera es que CapaPruebas solo referencia CapaEntidad y CapaNegocio: aqui
    /// se puede probar, alla no. La segunda es que asi esta clase no sabe nada de
    /// PdfSharp, y quien lea el HTML que produce no tiene que saber de PDF.
    ///
    /// El marcado va con tablas y estilos en linea, como HtmlSolicitud, y no con
    /// flexbox ni grid: HtmlRenderer entiende un subconjunto chico de CSS y lo
    /// que no entiende lo ignora en silencio, sin error y sin aviso.
    /// </summary>
    public static class NegPerfilCv
    {
        /// <summary>
        /// El CV completo, o cadena vacia si no hay de quien hacerlo.
        /// </summary>
        public static string Construir(EntPerfilCompleto perfil)
        {
            if (perfil == null || !perfil.PerfilEncontrado) { return ""; }

            StringBuilder html = new StringBuilder();

            html.Append("<html><head><meta charset=\"utf-8\" /></head>");
            html.Append("<body style=\"font-family: Arial, sans-serif; font-size: 11pt; color: #222\">");

            Encabezado(html, perfil);
            Datos(html, perfil);
            Estudios(html, perfil);
            Certificaciones(html, perfil);
            Experiencia(html, perfil);

            /* Los contactos de emergencia y las cargas familiares NO van. Un CV se
               entrega a terceros, y el telefono de la madre de alguien no tiene
               por que viajar con el. */

            html.Append("</body></html>");

            return html.ToString();
        }

        private static void Encabezado(StringBuilder html, EntPerfilCompleto perfil)
        {
            html.Append("<div style=\"border-bottom: 2px solid #750202; padding-bottom: 8px; margin-bottom: 14px\">");
            html.Append("<div style=\"font-size: 18pt; font-weight: bold\">");
            html.Append(Escapar(perfil.Cabecera.NombreCompleto));
            html.Append("</div>");

            html.Append("<div style=\"color: #666\">");
            html.Append(Escapar(perfil.Cabecera.Cargo));

            if (perfil.Cabecera.Area != "")
            {
                html.Append(" &middot; ");
                html.Append(Escapar(perfil.Cabecera.Area));
            }

            html.Append("</div></div>");
        }

        /// <summary>
        /// Los datos de contacto, en dos columnas. Se omite cada linea que no
        /// tenga dato en vez de imprimir la etiqueta con un guion: el CV de los
        /// 119 sin ficha de empleado saldria lleno de guiones.
        /// </summary>
        private static void Datos(StringBuilder html, EntPerfilCompleto perfil)
        {
            html.Append("<table style=\"width: 100%; font-size: 10pt; margin-bottom: 16px\">");

            Dato(html, "Cédula", perfil.Cabecera.Cedula);
            Dato(html, "Fecha de nacimiento", perfil.Cabecera.FechaNacTexto);
            Dato(html, "Ciudad", perfil.Cabecera.Ciudad);
            Dato(html, "Correo", perfil.Contacto.CorreoPersonal != ""
                                     ? perfil.Contacto.CorreoPersonal
                                     : perfil.Cabecera.CorreoNotificacion);
            Dato(html, "Teléfono", perfil.Contacto.TelefonoPersonal);
            Dato(html, "Domicilio", perfil.Contacto.Direccion);

            html.Append("</table>");
        }

        private static void Dato(StringBuilder html, string etiqueta, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) { return; }

            html.Append("<tr><td style=\"width: 160px; color: #666; padding: 2px 0\">");
            html.Append(Escapar(etiqueta));
            html.Append("</td><td style=\"padding: 2px 0\">");
            html.Append(Escapar(valor));
            html.Append("</td></tr>");
        }

        private static void Estudios(StringBuilder html, EntPerfilCompleto perfil)
        {
            if (perfil.Estudios.Count == 0) { return; }

            Titulo(html, "Formación académica");
            html.Append("<table style=\"width: 100%; font-size: 10pt; margin-bottom: 16px\">");

            foreach (EntPerfilEstudio e in perfil.Estudios)
            {
                html.Append("<tr><td style=\"padding: 3px 0\"><b>");
                html.Append(Escapar(e.Titulo));
                html.Append("</b><br /><span style=\"color: #666\">");
                html.Append(Escapar(e.Institucion));

                if (e.Nivel != "")
                {
                    html.Append(" &middot; ");
                    html.Append(Escapar(e.Nivel));
                }

                html.Append("</span></td><td style=\"width: 70px; text-align: right; color: #666\">");
                html.Append(e.AnioGraduacion.HasValue ? e.AnioGraduacion.Value.ToString() : "");
                html.Append("</td></tr>");
            }

            html.Append("</table>");
        }

        private static void Certificaciones(StringBuilder html, EntPerfilCompleto perfil)
        {
            if (perfil.Certificaciones.Count == 0) { return; }

            Titulo(html, "Certificaciones");
            html.Append("<table style=\"width: 100%; font-size: 10pt; margin-bottom: 16px\">");

            foreach (EntPerfilCertificacion c in perfil.Certificaciones)
            {
                html.Append("<tr><td style=\"padding: 3px 0\"><b>");
                html.Append(Escapar(c.Nombre));
                html.Append("</b><br /><span style=\"color: #666\">");
                html.Append(Escapar(c.Entidad));
                html.Append("</span></td><td style=\"width: 70px; text-align: right; color: #666\">");
                html.Append(Escapar(c.FechaObtencion));
                html.Append("</td></tr>");
            }

            html.Append("</table>");
        }

        private static void Experiencia(StringBuilder html, EntPerfilCompleto perfil)
        {
            if (perfil.Experiencia.Count == 0) { return; }

            Titulo(html, "Experiencia laboral");
            html.Append("<table style=\"width: 100%; font-size: 10pt; margin-bottom: 16px\">");

            foreach (EntPerfilExperiencia x in perfil.Experiencia)
            {
                html.Append("<tr><td style=\"padding: 3px 0\"><b>");
                html.Append(Escapar(x.Cargo));
                html.Append("</b><br /><span style=\"color: #666\">");
                html.Append(Escapar(x.Empresa));
                html.Append("</span>");

                if (x.Funciones != "")
                {
                    html.Append("<br /><span style=\"font-size: 9pt\">");
                    html.Append(Escapar(x.Funciones));
                    html.Append("</span>");
                }

                html.Append("</td><td style=\"width: 110px; text-align: right; color: #666\">");
                html.Append(Escapar(Periodo(x.AnioDesde, x.AnioHasta)));
                html.Append("</td></tr>");
            }

            html.Append("</table>");
        }

        /// <summary>
        /// "2018 - 2021", "2018 - Actual" cuando sigue ahi, o vacio si no se sabe
        /// cuando empezo. Sin anio de fin, un "2018 - " a secas se lee como un
        /// dato cortado.
        /// </summary>
        private static string Periodo(int? desde, int? hasta)
        {
            if (!desde.HasValue) { return ""; }

            return desde.Value.ToString() + " - " +
                   (hasta.HasValue ? hasta.Value.ToString() : "Actual");
        }

        private static void Titulo(StringBuilder html, string texto)
        {
            html.Append("<div style=\"font-size: 12pt; font-weight: bold; color: #750202; ");
            html.Append("border-bottom: 1px solid #ddd; margin-bottom: 6px\">");
            html.Append(Escapar(texto));
            html.Append("</div>");
        }

        /// <summary>
        /// Escapa el texto que va al HTML.
        ///
        /// Se escribe a mano en vez de usar HttpUtility.HtmlEncode porque
        /// CapaNegocio no referencia System.Web y no vale la pena que empiece a
        /// hacerlo por una funcion de cinco reemplazos. Los cinco caracteres son
        /// los que importan: el ampersand va primero, o volveria a escapar lo que
        /// escaparon los demas.
        /// </summary>
        private static string Escapar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) { return ""; }

            return texto
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }
    }
}
