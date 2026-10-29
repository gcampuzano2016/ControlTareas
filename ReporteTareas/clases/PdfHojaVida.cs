using PdfSharp;
using PdfSharp.Pdf;
using System.IO;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace ReporteTareas.clases
{
    /// <summary>
    /// HTML a PDF, y nada mas. Lo unico de todo el modulo de perfil que sabe de
    /// PdfSharp.
    ///
    /// Se usa PdfSharp + HtmlRenderer y no Pechkin -clases/PDF.cs- porque aquel
    /// es puramente gestionado: sin DLL nativa, sin el problema de 32 contra 64
    /// bits, y sin los fallos intermitentes que obligaron a poner un bucle de
    /// reintentos alrededor del otro. Es el mismo camino que ya usa
    /// clases/PdfLista.cs.
    ///
    /// A cambio, HtmlRenderer entiende un subconjunto chico de CSS: por eso el
    /// HTML que llega aqui va con tablas y estilos en linea.
    /// </summary>
    public static class PdfHojaVida
    {
        /// <summary>
        /// El PDF en memoria. No se escribe en disco a proposito: un CV lleva la
        /// cedula, la fecha de nacimiento y el domicilio de una persona, y un
        /// archivo en una carpeta servida por IIS es un archivo que alguien mas
        /// puede pedir. Ademas no hay nada que limpiar despues.
        /// </summary>
        public static byte[] Generar(string html)
        {
            return Generar(html, 20);
        }

        /// <summary>
        /// Con el margen explicito. El certificado laboral va con 0: su membrete
        /// va de borde a borde y los margenes los pone su propio HTML. 20 es el
        /// valor por omision de HtmlRenderer, el que siempre uso el CV.
        /// </summary>
        public static byte[] Generar(string html, int margen)
        {
            using (MemoryStream memoria = new MemoryStream())
            {
                PdfDocument documento = PdfGenerator.GeneratePdf(html, PageSize.A4, margen);

                /* false: que no cierre el stream, que lo cierra el using y de el
                   todavia hay que sacar los bytes. */
                documento.Save(memoria, false);

                return memoria.ToArray();
            }
        }
    }
}
