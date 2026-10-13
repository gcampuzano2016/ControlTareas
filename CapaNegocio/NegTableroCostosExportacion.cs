using CapaEntidad;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaNegocio
{
    /// <summary>
    /// El Excel del Tablero de Costos: el resumen por tipo y recargo, y el
    /// detalle de las actividades filtradas, con las columnas de la hoja "Datos"
    /// del Excel original. Se arma en memoria y nunca se escribe a disco, como
    /// NegHeExportacion: lleva valores calculados con salarios.
    /// </summary>
    public static class NegTableroCostosExportacion
    {
        public const string HojaResumen = "Resumen";
        public const string HojaDetalle = "Detalle";
        public const int FilaCabeceraResumen = 4;

        public static readonly string[] ColumnasDetalle =
        {
            "Id Detalle", "Fecha Inicio", "Fecha Fin", "Año", "Mes", "Responsable", "Empresa", "Contrato",
            "Ticket Aranda", "Orden Servicio", "Tarea", "Estado", "Tipo Hora", "Recargo HE", "Estado HE",
            "Tiempo", "Horas", "Factor", "Valor Hora", "Valor $", "Periodo"
        };

        private static readonly string[] ColumnasResumen =
            { "Tipo / Recargo", "Horas", "% horas", "Valor $", "% valor", "Actividades", "$ por hora" };

        public static string NombreDeArchivo(DateTime desde, DateTime hasta)
        {
            return "TableroCostos_"
                 + desde.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "_"
                 + hasta.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".xlsx";
        }

        public static byte[] Construir(EntTableroCostos tablero, string descripcionFiltros)
        {
            if (tablero == null) { throw new ArgumentNullException("tablero"); }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage paquete = new ExcelPackage())
            {
                EscribirResumen(paquete.Workbook.Worksheets.Add(HojaResumen), tablero, descripcionFiltros ?? "");
                EscribirDetalle(paquete.Workbook.Worksheets.Add(HojaDetalle), tablero.Detalle);
                return paquete.GetAsByteArray();
            }
        }

        private static void EscribirResumen(ExcelWorksheet ws, EntTableroCostos t, string filtros)
        {
            ws.Cells[1, 1].Value = "Tablero de Costos – Tareas Aranda";
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;
            ws.Cells[2, 1].Value = filtros;

            Cabecera(ws, FilaCabeceraResumen, ColumnasResumen);

            int fila = FilaCabeceraResumen + 1;
            foreach (EntTcResumenFila r in t.Resumen)
            {
                ws.Cells[fila, 1].Value = r.Concepto;
                ws.Cells[fila, 2].Value = r.Horas;
                ws.Cells[fila, 3].Value = r.PctHoras;
                ws.Cells[fila, 4].Value = r.Valor;
                ws.Cells[fila, 5].Value = r.PctValor;
                ws.Cells[fila, 6].Value = r.Actividades;
                ws.Cells[fila, 7].Value = r.ValorPorHora;
                if (r.EsSubtotal) { ws.Cells[fila, 1, fila, ColumnasResumen.Length].Style.Font.Bold = true; }
                fila++;
            }

            int primera = FilaCabeceraResumen + 1, ultima = fila - 1;
            if (ultima >= primera)
            {
                ws.Cells[primera, 2, ultima, 2].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[primera, 3, ultima, 3].Style.Numberformat.Format = "0.0%";
                ws.Cells[primera, 4, ultima, 4].Style.Numberformat.Format = "\"$\"#,##0.00";
                ws.Cells[primera, 5, ultima, 5].Style.Numberformat.Format = "0.0%";
                ws.Cells[primera, 6, ultima, 6].Style.Numberformat.Format = "#,##0";
                ws.Cells[primera, 7, ultima, 7].Style.Numberformat.Format = "\"$\"#,##0.00";
            }
            ws.Column(1).Width = 38;
            for (int c = 2; c <= ColumnasResumen.Length; c++) { ws.Column(c).Width = 14; }
        }

        private static void EscribirDetalle(ExcelWorksheet ws, List<EntTcFila> filas)
        {
            Cabecera(ws, 1, ColumnasDetalle);

            int fila = 2;
            foreach (EntTcFila r in filas)
            {
                object[] v =
                {
                    r.IdDetalle, r.FechaInicio, r.FechaFin, r.Anio, r.Mes, r.Responsable, r.Empresa, r.Contrato,
                    r.Ticket, r.OrdenServicio, r.Tarea, r.Estado, r.Tipo, r.Recargo, r.EstadoHE,
                    r.Tiempo, r.Horas, r.Factor, r.ValorHora, r.Valor, r.Periodo
                };
                for (int c = 0; c < v.Length; c++) { ws.Cells[fila, c + 1].Value = v[c]; }
                fila++;
            }

            int ultima = fila - 1;
            if (ultima >= 2)
            {
                ws.Cells[2, 2, ultima, 3].Style.Numberformat.Format = "dd/mm/yyyy hh:mm";
                ws.Cells[2, 17, ultima, 17].Style.Numberformat.Format = "0.0000";
                ws.Cells[2, 18, ultima, 18].Style.Numberformat.Format = "0.00";
                ws.Cells[2, 19, ultima, 19].Style.Numberformat.Format = "\"$\"#,##0.0000";
                ws.Cells[2, 20, ultima, 20].Style.Numberformat.Format = "\"$\"#,##0.00";
                ws.Cells[1, 1, ultima, ColumnasDetalle.Length].AutoFilter = true;
            }
            ws.View.FreezePanes(2, 1);

            int[] anchos = { 11, 17, 17, 7, 6, 32, 26, 30, 17, 15, 45, 20, 11, 11, 13, 10, 9, 8, 11, 12, 10 };
            for (int c = 0; c < anchos.Length; c++) { ws.Column(c + 1).Width = anchos[c]; }
        }

        private static void Cabecera(ExcelWorksheet ws, int fila, string[] textos)
        {
            for (int i = 0; i < textos.Length; i++) { ws.Cells[fila, i + 1].Value = textos[i]; }

            ExcelRange r = ws.Cells[fila, 1, fila, textos.Length];
            r.Style.Font.Bold = true;
            r.Style.Font.Color.SetColor(255, 255, 255, 255);
            r.Style.Fill.PatternType = ExcelFillStyle.Solid;
            r.Style.Fill.BackgroundColor.SetColor(255, 0x1F, 0x3A, 0x5F);
        }
    }
}
