using CapaEntidad;
using OfficeOpenXml;
using System;
using System.Globalization;

namespace CapaNegocio
{
    /// <summary>
    /// Arma el archivo xlsx de un periodo de horas extras para que Nomina se lo
    /// pueda llevar. Todo ocurre en memoria: no hay plantilla en disco, porque
    /// no existe ninguna para este modulo y crear una agregaria un archivo de
    /// despliegue que puede faltar sin dar ningun error (ya paso con
    /// descargas/perfil/web.config). La hoja se construye desde cero.
    /// </summary>
    public static class NegHeExportacion
    {
        private const string NombreHoja = "Horas Extras";

        /// <summary>Fila donde va el titulo con el periodo y su estado.</summary>
        public const int FilaDeEstado = 1;

        /// <summary>Fila donde arranca el detalle, un colaborador por fila.</summary>
        public const int PrimeraFilaDeDatos = 5;

        /// <summary>Columna del total a pagar por horas extras de cada fila.</summary>
        public const int ColumnaTotalHE = 17;

        private const int FilaDeCierre = 2;
        private const int FilaDeEncabezados = 4;
        private const int UltimaColumna = 18;

        private const string FormatoMoneda = "#,##0.00";
        private const string FormatoHoras = "#,##0.00";
        private const string FormatoEntero = "0";

        // El orden aqui define el orden de las columnas 1..18 en la hoja.
        private static readonly string[] Encabezados = new string[]
        {
            "Cedula",
            "Nombre",
            "Empresa",
            "Cargo",
            "Jornada (h/dia)",
            "Salario Base",
            "Aplica HE",
            "Divisor",
            "Valor Hora Ordinaria",
            "Valor Hora 50%",
            "Valor Hora 100%",
            "Horas 50%",
            "Horas 100%",
            "Total Pago 50%",
            "Total Pago 100%",
            "Total Horas",
            "Total HE",
            "Observacion"
        };

        /// <summary>
        /// Nombre de archivo con el codigo de documento controlado que usan los
        /// demas exportadores del sistema, mas el periodo y la hora de armado.
        /// </summary>
        public static string NombreDeArchivo(EntHePeriodo periodo)
        {
            DateTime ahora = DateTime.Now;
            return "F-CS-001 Reporte de Horas Extras"
                + periodo.Anio.ToString(CultureInfo.InvariantCulture)
                + periodo.Mes.ToString("00", CultureInfo.InvariantCulture)
                + "_" + ahora.ToString("HHmmss", CultureInfo.InvariantCulture)
                + ".xlsx";
        }

        /// <summary>
        /// Arma el libro completo y lo devuelve como bytes. Nunca toca disco:
        /// GetAsByteArray, nunca SaveAs(FileInfo).
        /// </summary>
        public static byte[] Construir(EntHePantalla pantalla)
        {
            // EPPlus exige fijar el contexto de licencia antes del primer uso.
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage paquete = new ExcelPackage())
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets.Add(NombreHoja);

                EscribirTitulo(hoja, pantalla.Periodo);
                EscribirCierre(hoja, pantalla.Periodo);
                EscribirEncabezados(hoja);
                int filaTotales = EscribirFilas(hoja, pantalla);
                EscribirTotales(hoja, filaTotales, pantalla);
                AjustarColumnas(hoja);

                return paquete.GetAsByteArray();
            }
        }

        private static void EscribirTitulo(ExcelWorksheet hoja, EntHePeriodo periodo)
        {
            bool cerrado = string.Equals(periodo.EstadoPeriodo, "Cerrado", StringComparison.OrdinalIgnoreCase);

            string titulo = string.Format(
                CultureInfo.InvariantCulture,
                "Reporte de Horas Extras - Periodo {0:00}/{1} - {2}{3}",
                periodo.Mes,
                periodo.Anio,
                periodo.EstadoPeriodo,
                cerrado ? "" : " (PROVISIONAL)");

            using (ExcelRange celda = hoja.Cells[FilaDeEstado, 1, FilaDeEstado, UltimaColumna])
            {
                celda.Merge = true;
            }

            hoja.Cells[FilaDeEstado, 1].Value = titulo;
            hoja.Cells[FilaDeEstado, 1].Style.Font.Bold = true;
            hoja.Cells[FilaDeEstado, 1].Style.Font.Size = 12;
        }

        private static void EscribirCierre(ExcelWorksheet hoja, EntHePeriodo periodo)
        {
            bool cerrado = string.Equals(periodo.EstadoPeriodo, "Cerrado", StringComparison.OrdinalIgnoreCase);
            string texto = "";

            if (cerrado)
            {
                bool tieneUsuario = !string.IsNullOrEmpty(periodo.UsuarioCierre);
                bool tieneFecha = periodo.FechaCierre.HasValue;

                if (tieneUsuario && tieneFecha)
                {
                    texto = string.Format(
                        CultureInfo.InvariantCulture,
                        "Cerrado por {0} el {1:dd/MM/yyyy HH:mm}",
                        periodo.UsuarioCierre,
                        periodo.FechaCierre.Value);
                }
                else if (tieneUsuario)
                {
                    texto = "Cerrado por " + periodo.UsuarioCierre;
                }
                else if (tieneFecha)
                {
                    texto = string.Format(
                        CultureInfo.InvariantCulture,
                        "Cerrado el {0:dd/MM/yyyy HH:mm}",
                        periodo.FechaCierre.Value);
                }
            }

            using (ExcelRange celda = hoja.Cells[FilaDeCierre, 1, FilaDeCierre, UltimaColumna])
            {
                celda.Merge = true;
            }

            hoja.Cells[FilaDeCierre, 1].Value = texto;
            hoja.Cells[FilaDeCierre, 1].Style.Font.Italic = true;
        }

        private static void EscribirEncabezados(ExcelWorksheet hoja)
        {
            for (int i = 0; i < Encabezados.Length; i++)
            {
                ExcelRange celda = hoja.Cells[FilaDeEncabezados, i + 1];
                celda.Value = Encabezados[i];
                celda.Style.Font.Bold = true;
            }
        }

        /// <summary>Escribe una fila por colaborador y devuelve la fila donde debe ir la de totales.</summary>
        private static int EscribirFilas(ExcelWorksheet hoja, EntHePantalla pantalla)
        {
            int fila = PrimeraFilaDeDatos;

            foreach (EntHeFila f in pantalla.Filas)
            {
                hoja.Cells[fila, 1].Value = f.CedulaSnapshot;
                hoja.Cells[fila, 2].Value = f.NombreSnapshot;
                hoja.Cells[fila, 3].Value = f.EmpresaSnapshot;
                hoja.Cells[fila, 4].Value = f.CargoSnapshot;

                hoja.Cells[fila, 5].Value = f.JornadaHorasDiaSnapshot;
                hoja.Cells[fila, 5].Style.Numberformat.Format = FormatoEntero;

                hoja.Cells[fila, 6].Value = f.SalarioBaseSnapshot;
                hoja.Cells[fila, 6].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 7].Value = f.AplicaHESnapshot ? "Si" : "No";

                hoja.Cells[fila, 8].Value = f.Divisor;
                hoja.Cells[fila, 8].Style.Numberformat.Format = FormatoEntero;

                hoja.Cells[fila, 9].Value = f.ValorHoraOrdinaria;
                hoja.Cells[fila, 9].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 10].Value = f.ValorHora50;
                hoja.Cells[fila, 10].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 11].Value = f.ValorHora100;
                hoja.Cells[fila, 11].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 12].Value = f.Horas50;
                hoja.Cells[fila, 12].Style.Numberformat.Format = FormatoHoras;

                hoja.Cells[fila, 13].Value = f.Horas100;
                hoja.Cells[fila, 13].Style.Numberformat.Format = FormatoHoras;

                hoja.Cells[fila, 14].Value = f.Total50;
                hoja.Cells[fila, 14].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 15].Value = f.Total100;
                hoja.Cells[fila, 15].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 16].Value = f.TotalHoras;
                hoja.Cells[fila, 16].Style.Numberformat.Format = FormatoHoras;

                // ColumnaTotalHE: el dinero va como decimal, nunca como texto ya
                // formateado. El formato visual es cosa aparte (Numberformat).
                hoja.Cells[fila, ColumnaTotalHE].Value = f.TotalHE;
                hoja.Cells[fila, ColumnaTotalHE].Style.Numberformat.Format = FormatoMoneda;

                hoja.Cells[fila, 18].Value = f.Observacion;

                fila++;
            }

            return fila;
        }

        private static void EscribirTotales(ExcelWorksheet hoja, int fila, EntHePantalla pantalla)
        {
            using (ExcelRange etiqueta = hoja.Cells[fila, 1, fila, 11])
            {
                etiqueta.Merge = true;
            }

            hoja.Cells[fila, 1].Value = "TOTALES";
            hoja.Cells[fila, 1].Style.Font.Bold = true;

            hoja.Cells[fila, 12].Value = pantalla.TotalHoras50;
            hoja.Cells[fila, 12].Style.Numberformat.Format = FormatoHoras;

            hoja.Cells[fila, 13].Value = pantalla.TotalHoras100;
            hoja.Cells[fila, 13].Style.Numberformat.Format = FormatoHoras;

            hoja.Cells[fila, 14].Value = pantalla.TotalPago50;
            hoja.Cells[fila, 14].Style.Numberformat.Format = FormatoMoneda;

            hoja.Cells[fila, 15].Value = pantalla.TotalPago100;
            hoja.Cells[fila, 15].Style.Numberformat.Format = FormatoMoneda;

            hoja.Cells[fila, 16].Value = pantalla.TotalHoras;
            hoja.Cells[fila, 16].Style.Numberformat.Format = FormatoHoras;

            hoja.Cells[fila, ColumnaTotalHE].Value = pantalla.TotalPagar;
            hoja.Cells[fila, ColumnaTotalHE].Style.Numberformat.Format = FormatoMoneda;

            for (int c = 1; c <= UltimaColumna; c++)
            {
                hoja.Cells[fila, c].Style.Font.Bold = true;
            }
        }

        private static void AjustarColumnas(ExcelWorksheet hoja)
        {
            for (int c = 1; c <= UltimaColumna; c++)
            {
                hoja.Column(c).Width = 16;
            }

            hoja.Column(2).Width = 28;
            hoja.Column(3).Width = 22;
            hoja.Column(4).Width = 22;
            hoja.Column(18).Width = 30;
        }
    }
}
