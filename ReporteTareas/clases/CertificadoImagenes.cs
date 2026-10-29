using System;
using System.IO;
using System.Web;

namespace ReporteTareas.clases
{
    /// <summary>
    /// Los membretes del certificado laboral, como data URI.
    ///
    /// Van como data URI y no como ruta porque el mismo HTML lo consumen dos
    /// lectores: el navegador (vista previa) y HtmlRenderer (PDF), y HtmlRenderer
    /// no resuelve rutas del sitio. Se leen una vez por proceso: son 20 KB.
    ///
    /// Si un archivo falta se devuelve cadena vacia y el certificado sale sin esa
    /// imagen en vez de fallar: la redaccion es lo que vale, el logo no.
    /// </summary>
    internal static class CertificadoImagenes
    {
        private static string _membreteDos;
        private static string _logoAgility;

        internal static string MembreteDos(HttpContext context)
        {
            if (_membreteDos == null)
            {
                _membreteDos = DataUri(context, "~/Img/certificado/membrete-dos.png", "image/png");
            }
            return _membreteDos;
        }

        internal static string LogoAgility(HttpContext context)
        {
            if (_logoAgility == null)
            {
                _logoAgility = DataUri(context, "~/Img/certificado/logo-agility.jpg", "image/jpeg");
            }
            return _logoAgility;
        }

        private static string DataUri(HttpContext context, string ruta, string tipo)
        {
            string fisica = context.Server.MapPath(ruta);
            if (!File.Exists(fisica)) { return ""; }

            return "data:" + tipo + ";base64," + Convert.ToBase64String(File.ReadAllBytes(fisica));
        }
    }
}
