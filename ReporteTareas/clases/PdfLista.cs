using System;
using System.Collections.Generic;
using System.Linq;

using System.Web;
using PdfSharp;
using PdfSharp.Pdf;
using System.IO;
using TheArtOfDev.HtmlRenderer.PdfSharp;

using CapaEntidad;
using CapaNegocio;

namespace ReporteTareas.clases
{
    public class PdfLista
    {
        #region CrearPDF
        public bool CrearPDF(string contenidohtml, int codigoSolicitud)
        {
            bool Resultado = false;
            string fecha;
            try
            {
                fecha = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss");
                VerErrores("fecha: " + fecha.ToString(), "Log", "Detalle");
                string folderPath = "";
                // Carpeta donde se creará el archivo
                folderPath = HttpContext.Current.Server.MapPath("~/descargas/");
                VerErrores("folderPath: " + folderPath.ToString(), "Log", "Detalle");
                string directory = folderPath;
                VerErrores("directory: " + directory.ToString(), "Log", "Detalle");
                // Nombre del PDF
                string filename = fecha.Replace(":", "_").Replace(" ", "_") + ".pdf";
                string htmlString = contenidohtml;
                try
                {
                    PdfDocument pdfDocument = PdfGenerator.GeneratePdf(htmlString, PageSize.A4);
                    pdfDocument.Save(directory + filename);

                    Console.WriteLine("PDF Succesfully created");
                    VerErrores("PDF Succesfully created", "Log", "Detalle");
                    EntSolicitud registro = new EntSolicitud();
                    registro.IdVacaciones = codigoSolicitud;
                    registro.Ruta_Archivo = directory;
                    registro.Descripcion_Archivo = filename;
                    int result = NegSolicitud.RTA_ActualizarRutaRide(registro);

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