using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Pechkin;

using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;

using CapaEntidad;
using CapaNegocio;

using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using Pechkin.Synchronized;

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

                    // Carpeta donde se creará el archivo
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
                //Console.WriteLine("Excepción detectada en el proceso al intentar guardar: {0}", _Exception.ToString());
                VerErrores("Exception: "+ _Exception.ToString(), "Log", "Detalle");
            }

            return false;
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
                //Console.WriteLine("Excepción detectada en el proceso al intentar guardar: {0}", _Exception.ToString());
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