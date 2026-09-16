using CapaEntidad;
using CapaNegocio;
using ReporteTareas.clases;
using System;
using System.IO;
using System.Web;

namespace JsonJQueryNetPerfil
{
    /// <summary>
    /// Las descargas del perfil. Handler aparte de AdministrarPerfil.ashx porque
    /// aquel responde JSON a peticiones POST y esto es un GET que escribe bytes:
    /// meterlos juntos obligaria a que uno de los dos rompiera su propio contrato.
    ///
    /// IRequiresSessionState no es decorativo: sin el, context.Session es null en
    /// un IHttpHandler y no habria identidad con la que comprobar nada.
    ///
    /// La identidad sale SIEMPRE de la sesion. El unico parametro que se acepta de
    /// la query string es que documento se quiere; de quien es lo dice la base.
    /// </summary>
    public class DescargarPerfil : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.");
                return;
            }

            string codUsuario = CodUsuarioSesion(context);

            if (codUsuario == "")
            {
                NoDisponible(context, "No se pudo identificar al usuario de la sesión.");
                return;
            }

            if (context.Request.QueryString["cv"] == "1")
            {
                EntregarCv(context, codUsuario);
                return;
            }

            int idDocumento;
            if (!int.TryParse(context.Request.QueryString["doc"], out idDocumento) || idDocumento <= 0)
            {
                NoDisponible(context, "No se indicó qué documento descargar.");
                return;
            }

            EntregarDocumento(context, codUsuario, idDocumento);
        }

        /// <summary>
        /// Entrega un documento de respaldo.
        ///
        /// ObtenerDocumento devuelve null cuando el documento no es de esta
        /// persona, no existe, ya fue quitado, o su codigo de usuario esta
        /// repetido. Los cuatro casos dan el mismo mensaje a proposito: uno
        /// distinto para "existe pero no es suyo" le confirmaria a quien esta
        /// probando numeros que acerto con uno.
        /// </summary>
        private void EntregarDocumento(HttpContext context, string codUsuario, int idDocumento)
        {
            EntPerfilDocumento doc = NegPerfil.ObtenerDocumento(codUsuario, idDocumento);

            if (doc == null)
            {
                NoDisponible(context, "No se encontró ese documento.");
                return;
            }

            string rutaFisica = context.Server.MapPath(doc.Ruta) + doc.NombreArchivoCodigo;

            if (!File.Exists(rutaFisica))
            {
                NoDisponible(context, "El archivo ya no está disponible en el servidor.");
                return;
            }

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = TipoDeContenido(doc.NombreArchivoCodigo);
            context.Response.AddHeader("Content-Disposition",
                                       "attachment;filename=\"" + NombreSeguro(doc.NombreArchivo) + "\"");
            context.Response.TransmitFile(rutaFisica);
            context.Response.End();
        }

        /// <summary>
        /// Genera y entrega el CV de quien lo pide.
        ///
        /// SOLO el propio. No hay parametro que diga de quien es el CV, y no es un
        /// descuido: una jefatura consulta el perfil de su equipo, no se descarga
        /// sus hojas de vida.
        /// </summary>
        private void EntregarCv(HttpContext context, string codUsuario)
        {
            EntPerfilCompleto perfil = NegPerfil.CargarPerfil(codUsuario);

            if (!perfil.PerfilEncontrado)
            {
                NoDisponible(context, "No pudimos identificar su perfil de forma única. " +
                                      "Escriba a Talento Humano para que corrijan su código de usuario.");
                return;
            }

            byte[] pdf = PdfHojaVida.Generar(NegPerfilCv.Construir(perfil));

            /* El codigo de usuario va al nombre del archivo y de ahi a una
               cabecera HTTP: se deja solo lo alfanumerico. */
            string nombre = "CV_" + SoloAlfanumerico(codUsuario) + "_" +
                            DateTime.Now.ToString("yyyyMMdd_HHmmss",
                                System.Globalization.CultureInfo.InvariantCulture) + ".pdf";

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/pdf";
            context.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + nombre + "\"");
            context.Response.BinaryWrite(pdf);
            context.Response.End();
        }

        private static string SoloAlfanumerico(string texto)
        {
            System.Text.StringBuilder limpio = new System.Text.StringBuilder();

            foreach (char c in texto ?? "")
            {
                if (char.IsLetterOrDigit(c)) { limpio.Append(c); }
            }

            return limpio.Length == 0 ? "perfil" : limpio.ToString();
        }

        /// <summary>
        /// El tipo segun la extension. La lista es la misma que acepta
        /// NegPerfilCampos.ValidarDocumento; cualquier otra cosa sale como binario
        /// generico, que el navegador descarga en vez de interpretar.
        /// </summary>
        private static string TipoDeContenido(string nombreArchivo)
        {
            string extension = Path.GetExtension(nombreArchivo ?? "").TrimStart('.').ToLowerInvariant();

            if (extension == "pdf") { return "application/pdf"; }
            if (extension == "png") { return "image/png"; }
            if (extension == "jpg" || extension == "jpeg") { return "image/jpeg"; }

            return "application/octet-stream";
        }

        /// <summary>
        /// El nombre del archivo lo puso el usuario y aca va dentro de una cabecera
        /// HTTP. Un salto de linea ahi le agrega cabeceras a la respuesta; unas
        /// comillas cierran el valor antes de tiempo. Se quitan los tres caracteres
        /// y, si no queda nada, se usa un nombre generico.
        /// </summary>
        private static string NombreSeguro(string nombreArchivo)
        {
            string limpio = (nombreArchivo ?? "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("\"", "")
                .Trim();

            return limpio == "" ? "documento" : limpio;
        }

        /// <summary>De quien es esta descarga. Sale de la sesion, nunca del cliente.</summary>
        private string CodUsuarioSesion(HttpContext context)
        {
            if (context.Session != null && context.Session["Cod_Usuario"] != null)
            {
                return context.Session["Cod_Usuario"].ToString().Trim();
            }
            return "";
        }

        /// <summary>
        /// Lo que ve quien pide algo que no puede tener. Texto plano y no JSON: el
        /// navegador llega aqui por un enlace, no por una llamada de JavaScript, y
        /// un JSON en pantalla no le dice nada a nadie.
        /// </summary>
        private void NoDisponible(HttpContext context, string mensaje)
        {
            context.Response.Clear();
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(mensaje);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
