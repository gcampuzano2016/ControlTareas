using CapaEntidad;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// La tabla de la pantalla en xlsx, armada en memoria, como NegHeExportacion.
    /// Montos y puntajes van como numeros, no como texto, para que se puedan sumar.
    /// </summary>
    public static class NegCalificacionExportacion
    {
        public const string NombreHoja = "Calificación";

        private static readonly string[] Encabezados =
        {
            "ID", "Cliente", "Oportunidad", "Etapa", "Gerente de cuenta", "Monto (USD)", "Monto ponderado (USD)",
            "Cierre estimado", "Presupuesto /20", "Autoridad /20", "Necesidad /20", "Tiempo /15", "Actividad /15",
            "Encaje /10", "Puntaje", "Estado", "Alertas", "Datos faltantes", "Acción sugerida", "Acción comprometida"
        };

        public static string NombreDeArchivo(DateTime hoy)
        {
            return "Calificacion_ForeCast_" + hoy.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".xlsx";
        }

        public static byte[] Construir(EntPantallaCalificacion pantalla)
        {
            if (pantalla == null) { throw new ArgumentNullException(nameof(pantalla)); }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage paquete = new ExcelPackage())
            {
                ExcelWorksheet h = paquete.Workbook.Worksheets.Add(NombreHoja);

                for (int c = 0; c < Encabezados.Length; c++)
                {
                    h.Cells[1, c + 1].Value = Encabezados[c];
                    h.Cells[1, c + 1].Style.Font.Bold = true;
                }

                int fila = 2;
                foreach (EntCalificacionForeCast o in pantalla.Oportunidades ?? new List<EntCalificacionForeCast>())
                {
                    h.Cells[fila, 1].Value = o.IdForeCast;
                    h.Cells[fila, 2].Value = o.Cliente;
                    h.Cells[fila, 3].Value = o.DetalleProyecto;
                    h.Cells[fila, 4].Value = o.Prioridad;
                    h.Cells[fila, 5].Value = o.GerentedeCuenta;
                    h.Cells[fila, 6].Value = o.PVPEstimado ?? 0m;
                    h.Cells[fila, 7].Value = o.MontoPonderado;
                    h.Cells[fila, 8].Value = o.FechaCierreTexto;
                    h.Cells[fila, 9].Value = o.Presupuesto;
                    h.Cells[fila, 10].Value = o.Autoridad;
                    h.Cells[fila, 11].Value = o.Necesidad;
                    h.Cells[fila, 12].Value = o.Tiempo;
                    h.Cells[fila, 13].Value = o.Actividad;
                    h.Cells[fila, 14].Value = o.Encaje;
                    h.Cells[fila, 15].Value = o.Puntaje;
                    h.Cells[fila, 16].Value = o.SinCalificar ? o.Estado + " (" + NegCalificacionForeCast.SinCalificarTexto + ")" : o.Estado;
                    h.Cells[fila, 17].Value = string.Join("; ", (o.Alertas ?? new List<EntAlertaCalificacion>()).Select(a => a.Texto));
                    h.Cells[fila, 18].Value = string.Join(", ", o.Faltantes ?? new List<string>());
                    h.Cells[fila, 19].Value = o.AccionSugerida;
                    h.Cells[fila, 20].Value = o.Calificacion == null ? "" : o.Calificacion.AccionComprometida;
                    fila++;
                }

                h.Cells[2, 6, Math.Max(fila - 1, 2), 7].Style.Numberformat.Format = "#,##0";
                h.View.FreezePanes(2, 3);
                h.Cells[1, 1, Math.Max(fila - 1, 1), Encabezados.Length].AutoFilter = true;
                h.Cells[h.Dimension.Address].AutoFitColumns(8, 60);

                return paquete.GetAsByteArray();
            }
        }
    }
}