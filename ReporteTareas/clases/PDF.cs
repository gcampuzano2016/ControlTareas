using CapaEntidad;
using CapaNegocio;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Pechkin;
using Pechkin.Synchronized;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;

namespace PDF
{
    public class PDFs
    {
        #region EnvioCorreoEncuesta
        public bool EnvioCorreoEncuesta(string contenidohtml, int codigoSolicitud)
        {
            VerErrores("Ingreso Generar Pdf: " + codigoSolicitud.ToString(), "Log", "Detalle");
            bool Resultado = false;
            string fecha;
            fecha = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss");
            VerErrores("fecha: " + fecha.ToString(), "Log", "Detalle");
            string folderPath = "";
            try
            {

                // Create global configuration object
                GlobalConfig gc = new GlobalConfig();
                VerErrores("Paso 1: " + "Paso 1", "Log", "Detalle");
                // Set it up using fluent notation
                //gc.SetMargins(new Margins(50, 100, 0, 0))
                //    .SetDocumentTitle("Request")
                //    .SetPaperSize(PaperKind.A4);
                VerErrores("Paso 2: " + "Paso 2", "Log", "Detalle");
                // Create converter
                IPechkin pechkin = new SynchronizedPechkin(gc);
                VerErrores("Paso 3: " + "Paso 3", "Log", "Detalle");
                // Create document configuration object
                ObjectConfig oc = new ObjectConfig();
                VerErrores("Paso 4: " + "Paso 4", "Log", "Detalle");

                try
                {
                    byte[] pdfBuffer = null;
                    pdfBuffer = pechkin.Convert(contenidohtml);
                    VerErrores("Paso 5: " + "Paso 5", "Log", "Detalle");
                    // PDF simple de cadena

                    //pdfBuffer = new SimplePechkin(new GlobalConfig()).Convert(contenidohtml);

                    // Carpeta donde se crear� el archivo
                    folderPath = HttpContext.Current.Server.MapPath("~/descargas/");
                    VerErrores("folderPath: " + folderPath.ToString(), "Log", "Detalle");
                    string directory = folderPath;
                    VerErrores("directory: " + directory.ToString(), "Log", "Detalle");
                    // Nombre del PDF
                    string filename = fecha.Replace(":", "_").Replace(" ", "_") + ".pdf";
                    VerErrores("filename: " + filename.ToString(), "Log", "Detalle");
                    VerErrores("directory: " + directory, "Log", "Detalle");
                    VerErrores("directory: " + filename, "Log", "Detalle");
                    if (ByteArrayToFile(directory + filename, pdfBuffer))
                    {
                        Console.WriteLine("PDF Succesfully created");
                        VerErrores("PDF Succesfully created", "Log", "Detalle");
                        EntSolicitud registro = new EntSolicitud();
                        registro.IdVacaciones = codigoSolicitud;
                        registro.Ruta_Archivo = directory;
                        registro.Descripcion_Archivo = filename;
                        int result = NegSolicitud.RTA_ActualizarRutaRide(registro);

                    }
                    else
                    {
                        Console.WriteLine("Cannot create PDF");
                        VerErrores("Cannot create PDF", "Log", "Detalle");
                    }

                }
                catch (Exception ex)
                {
                    VerErrores("ex-1: " + ex.Message.ToString(), "Log", "Detalle");
                }
            }
            catch (Exception ex)
            {
                VerErrores("ex-2: " + ex.Message.ToString(), "Log", "Detalle");
            }

            return Resultado;
        }

        #endregion

        #region ByteArrayToFile
        public bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
        {
            try
            {
                // Abrir archivo para leer
                FileStream _FileStream = new FileStream(_FileName, FileMode.Create, FileAccess.Write);
                // Escribe un bloque de bytes en esta secuencia utilizando datos de una matriz de bytes.
                _FileStream.Write(_ByteArray, 0, _ByteArray.Length);

                // Cerrar secuencia de archivos
                _FileStream.Close();
                //_FileStream.Flush();
                return true;
            }
            catch (Exception _Exception)
            {
                //Console.WriteLine("Excepci�n detectada en el proceso al intentar guardar: {0}", _Exception.ToString());
                VerErrores("Exception: " + _Exception.ToString(), "Log", "Detalle");
            }

            return false;
        }
        #endregion

        #region DocumentoDeSolicitud
        /// <summary>
        /// Arma el documento de una solicitud con el formato de los modelos de
        /// Talento Humano y lo deja en ~/descargas/, registrado contra la solicitud.
        /// Devuelve el nombre del archivo, o cadena vacía si falló.
        ///
        /// Vive acá y no en el handler porque hay dos momentos que lo necesitan:
        /// cuando se crea la solicitud —la copia que acompaña al correo— y cuando
        /// alguien descarga el documento firmado desde la lista. Antes cada uno
        /// generaba su propio PDF por caminos distintos, y el de la creación salía
        /// convertido de la plantilla del correo: dos documentos que no se parecían
        /// en nada para la misma solicitud.
        ///
        /// Se puede llamar en cualquier estado. Las firmas que todavía no existen
        /// salen como "Pendiente", que es lo correcto para una copia recién creada.
        /// </summary>
        public string DocumentoDeSolicitud(long idVacaciones)
        {
            string temporales = "";

            try
            {
                /* El cargador de solicitudes recibe int, aunque la columna sea bigint.
                   Convert lanza si no cabe, que es lo que se quiere: mejor un error
                   visible que un id truncado en silencio. */
                EntSolicitud solicitud =
                    NegSolicitud.ConsultaSp_RTANotificarSolicitud(0, Convert.ToInt32(idVacaciones));

                if (solicitud == null || solicitud.IdVacaciones == 0)
                {
                    VerErrores("DocumentoDeSolicitud: no existe la solicitud " + idVacaciones, "Log", "Detalle");
                    return "";
                }

                List<EntFirmaSolicitud> firmas = NegFirmaSolicitud.Listar(idVacaciones);
                string folio = NegFirmaSolicitud.Folio(idVacaciones);

                /* Los trazos se escriben a disco porque wkhtmltopdf de esta versión
                   no resuelve base64 embebido. Se borran al terminar: el original
                   vive en la base. */
                temporales = HttpContext.Current.Server.MapPath("~/descargas/tmp_" + idVacaciones + "_" +
                                                                DateTime.Now.ToString("yyyyMMddHHmmssfff")) + "\\";
                Directory.CreateDirectory(temporales);

                foreach (EntFirmaSolicitud f in firmas)
                {
                    if (string.IsNullOrEmpty(f.TrazoBase64)) { continue; }
                    string ruta = temporales + f.Rol + "_" + f.Secuencia + ".png";
                    File.WriteAllBytes(ruta, Convert.FromBase64String(f.TrazoBase64));
                    f.RutaTrazo = ruta;
                }

                /* El logo institucional, el mismo de los correos. Se usa este y no
                   Img/logo_dos.png porque ese último no está declarado en el
                   proyecto: existe en la máquina de desarrollo pero no viaja en la
                   publicación, así que el PDF habría salido sin logo en el servidor
                   y sin que nadie se enterara. Si algún día se declara, se toma. */
                string rutaLogo = HttpContext.Current.Server.MapPath("~/Img/imagesCorreo/logo_dos_textoGris.png");
                if (!File.Exists(rutaLogo))
                {
                    rutaLogo = HttpContext.Current.Server.MapPath("~/Img/logo_dos.png");
                }
                if (!File.Exists(rutaLogo)) { rutaLogo = ""; }

                /* El detalle del permiso: tipo, teletrabajo, saldo mensual y plan de
                   recuperación. En vacaciones no aplica y viene null, que es lo que
                   la plantilla espera. */
                EntDetallePermiso detalle = null;
                if (solicitud.IdTipoSolicitud == 1)
                {
                    detalle = NegDetallePermiso.Obtener(idVacaciones);
                }

                /* Los períodos de los que salen los días. Solo en vacaciones. */
                string periodos = "";
                if (solicitud.IdTipoSolicitud != 1)
                {
                    periodos = NegVacaciones.PeriodosConSaldo(solicitud.CodSap);
                }

                string html = HtmlSolicitud.Construir(solicitud, firmas, folio, rutaLogo, detalle, periodos);

                string archivo = GenerarPdfSolicitud(html, folio);
                if (string.IsNullOrEmpty(archivo)) { return ""; }

                /* Se deja registrado contra la solicitud para poder volver a
                   descargarlo desde el historial. */
                EntSolicitud registro = new EntSolicitud()
                {
                    IdVacaciones = idVacaciones,
                    Ruta_Archivo = "descargas/",
                    Descripcion_Archivo = archivo
                };
                NegSolicitud.RTA_ActualizarRutaRide(registro);

                return archivo;
            }
            catch (Exception ex)
            {
                VerErrores("DocumentoDeSolicitud: " + ex.Message, "Log", "Detalle");
                return "";
            }
            finally
            {
                /* Si esto falla no importa: son archivos de render, no el registro. */
                try
                {
                    if (!string.IsNullOrEmpty(temporales) && Directory.Exists(temporales))
                    {
                        Directory.Delete(temporales, true);
                    }
                }
                catch { }
            }
        }
        #endregion

        #region GenerarPdfSolicitud
        /// <summary>
        /// Convierte HTML a PDF y lo deja en ~/descargas/, devolviendo el nombre
        /// del archivo generado, o cadena vacía si falló.
        ///
        /// Existe en vez de reusar EnvioCorreoEncuesta, que hace casi lo mismo pero
        /// tiene tres problemas para este uso:
        ///
        ///   - Nunca devuelve true. Su variable de resultado se declara en false y
        ///     no se toca, así que quien la llama no puede distinguir un PDF creado
        ///     de uno que no se creó.
        ///   - Arma el nombre del archivo con "hh:mm:ss", que es hora de 12 sin
        ///     AM/PM. Dos documentos generados a la 01:00 y a las 13:00 del mismo
        ///     día se sobrescriben, y dos en el mismo segundo también.
        ///   - Se llama "EnvioCorreoEncuesta" y no envía correos ni tiene que ver
        ///     con encuestas.
        ///
        /// No se toca esa función porque hay tres pantallas colgando de ella.
        /// </summary>
        /// <param name="contenidoHtml">El documento ya armado.</param>
        /// <param name="prefijo">Prefijo del nombre del archivo, normalmente el folio.</param>
        public string GenerarPdfSolicitud(string contenidoHtml, string prefijo)
        {
            if (string.IsNullOrEmpty(contenidoHtml)) { return ""; }

            try
            {
                byte[] pdf = new SynchronizedPechkin(new GlobalConfig()).Convert(contenidoHtml);

                if (pdf == null || pdf.Length == 0)
                {
                    VerErrores("GenerarPdfSolicitud: Pechkin devolvio vacio", "Log", "Detalle");
                    return "";
                }

                string carpeta = HttpContext.Current.Server.MapPath("~/descargas/");
                if (!Directory.Exists(carpeta)) { Directory.CreateDirectory(carpeta); }

                /* Nombre único: folio más marca de tiempo hasta el milisegundo en
                   24 horas. Regenerar el mismo documento crea un archivo nuevo en
                   vez de pisar el anterior, que es lo que se quiere para algo que
                   lleva firmas. */
                string nombre = LimpiarNombre(prefijo) + "_" +
                                DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".pdf";

                if (!ByteArrayToFile(carpeta + nombre, pdf))
                {
                    VerErrores("GenerarPdfSolicitud: no se pudo escribir " + nombre, "Log", "Detalle");
                    return "";
                }

                return nombre;
            }
            catch (Exception ex)
            {
                VerErrores("GenerarPdfSolicitud: " + ex.Message, "Log", "Detalle");
                return "";
            }
        }

        /// <summary>Deja el prefijo apto para nombre de archivo.</summary>
        private string LimpiarNombre(string texto)
        {
            if (string.IsNullOrEmpty(texto)) { return "solicitud"; }

            StringBuilder limpio = new StringBuilder();
            foreach (char c in texto)
            {
                limpio.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_');
            }
            return limpio.ToString();
        }
        #endregion

        #region GenerarCodigoQR
        public string GenerarCodigoQR(string detalle)
        {
            string rutaQr = "";
            try
            {

                string fecha;
                fecha = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss");
                string folderPath = "";
                folderPath = HttpContext.Current.Server.MapPath("~/descargas/");
                string directory = folderPath;
                // Nombre del PDF
                string filename = fecha.Replace(":", "_").Replace(" ", "_") + ".png";

                rutaQr = directory + filename;

                var qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
                var qrCode = qrEncoder.Encode(detalle);

                var renderer = new GraphicsRenderer(new FixedModuleSize(5, QuietZoneModules.Two), Brushes.Black, Brushes.White);
                using (var stream = new FileStream(directory + filename, FileMode.Create))
                    renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);

                return rutaQr;

            }
            catch (Exception ex)
            {
                VerErrores("ex-QR: " + ex.Message.ToString(), "Log", "Detalle");
                //Console.WriteLine("Excepci�n detectada en el proceso al intentar guardar: {0}", _Exception.ToString());
                //VerErrores("Exception.ToString(): " + ex.ToString(), "Log", "Detalle");
            }
            return rutaQr;
        }
        #endregion

        #region VerErrores
        public void VerErrores(string valor, string Carpeta, string rucEmpresa)
        {
            try
            {
                string fecha;
                fecha = DateTime.Now.ToString("dd-MM-yyyy");//DateTime.Now.ToShortDateString().Replace("/", "-");
                if (!Directory.Exists(@"C:\\" + rucEmpresa + "\\" + Carpeta + "\\" + fecha))
                {
                    Directory.CreateDirectory(@"C:\\" + rucEmpresa + "\\" + Carpeta + "\\" + fecha);
                }

                string path = @"C:\\" + rucEmpresa + "\\" + Carpeta + "\\" + fecha + "\\log.txt";
                TextWriter tw = new StreamWriter(path, true);
                tw.WriteLine("A fecha de : " + DateTime.Now.ToString() + ": " + valor);
                tw.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", "Exception: " + ex.Message);
            }
        }
        #endregion

    }
}