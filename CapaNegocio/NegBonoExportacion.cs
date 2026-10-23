using CapaEntidad;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>Excel de bonos para Nomina: una hoja, una fila por bono aprobado y una fila final de total.</summary>
    public static class NegBonoExportacion
    {
        private static readonly string[] Encabezados =
            { "EMPRESA", "NOMBRES APELLIDOS", "ID SAP", "VALOR", "OBSERVACION", "FECHAS", "Lugar", "CONCEPTO SAP" };

        private const string FormatoMoneda = "#,##0.00";

        /// <summary>"2 semanas Standby X" / "1 semana Standby X" si es por semana; si no, el nombre; mas " · nota".</summary>
        public static string Observacion(EntBono b, EntBonoTipo t)
        {
            string nombre = t == null ? "" : t.Nombre ?? "";
            string texto = nombre;
            if (t != null && t.PorSemana)
            {
                int n = b.Lunes == null ? 0 : b.Lunes.Count;
                texto = n + (n == 1 ? " semana " : " semanas ") + nombre;
            }
            if (!string.IsNullOrWhiteSpace(b.Observacion))
                texto += " · " + b.Observacion.Trim();
            return texto;
        }

        /// <summary>Empresa (DOS, AGILITY), familia (Standby, Contrato, Otro) y nombre.</summary>
        public static List<EntBono> Ordenar(IEnumerable<EntBono> bonos, IDictionary<int, EntBonoTipo> tipos)
        {
            return bonos
                .OrderBy(b => RangoEmpresa(b.Empresa))
                .ThenBy(b => RangoFamilia(Familia(b, tipos)))
                .ThenBy(b => b.Nombre ?? "", StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static byte[] Construir(EntBonoPeriodo p, IList<EntBono> aprobados, IDictionary<int, EntBonoTipo> tipos)
        {
            bool abierto = p.Estado == BonoPeriodoEstados.Abierto;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage paquete = new ExcelPackage())
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets.Add("Bonos");

                string titulo = "Base de ingresos - " + NombreMes(p.Mes) + " " + p.Anio;
                if (abierto) titulo += " - PROVISIONAL";
                hoja.Cells[1, 1].Value = titulo;
                hoja.Cells[1, 1].Style.Font.Bold = true;

                for (int c = 0; c < Encabezados.Length; c++)
                {
                    hoja.Cells[2, c + 1].Value = Encabezados[c];
                    hoja.Cells[2, c + 1].Style.Font.Bold = true;
                }

                int fila = 3;
                decimal total = 0m;
                foreach (EntBono b in Ordenar(aprobados ?? new List<EntBono>(), tipos))
                {
                    EntBonoTipo t = Tipo(b, tipos);
                    string nombreTipo, concepto;
                    if (abierto)
                    {
                        nombreTipo = t == null ? "" : t.Nombre ?? "";
                        string codigo = null;
                        if (t != null && t.Conceptos != null)
                            t.Conceptos.TryGetValue(NegBonoReglas.EmpresaNormalizada(b.Empresa), out codigo);
                        concepto = codigo ?? "";
                    }
                    else
                    {
                        nombreTipo = b.NombreTipo ?? "";
                        concepto = b.CodigoConcepto ?? "";
                    }

                    bool porSemana = t != null && t.PorSemana;
                    EntBonoTipo paraTexto = new EntBonoTipo { Nombre = nombreTipo, PorSemana = porSemana };

                    hoja.Cells[fila, 1].Value = b.Empresa;
                    hoja.Cells[fila, 2].Value = b.Nombre;
                    hoja.Cells[fila, 3].Style.Numberformat.Format = "@";
                    hoja.Cells[fila, 3].Value = b.Identificacion ?? "";
                    hoja.Cells[fila, 4].Value = b.Valor;
                    hoja.Cells[fila, 4].Style.Numberformat.Format = FormatoMoneda;
                    hoja.Cells[fila, 5].Value = Observacion(b, paraTexto);
                    hoja.Cells[fila, 6].Value = porSemana ? NegBonoFechas.TextoFechas(b.Lunes) : "";
                    hoja.Cells[fila, 7].Value = b.Lugar ?? "";
                    hoja.Cells[fila, 8].Style.Numberformat.Format = "@";
                    hoja.Cells[fila, 8].Value = concepto;
                    total += b.Valor;
                    fila++;
                }

                hoja.Cells[fila, 2].Value = "TOTAL";
                hoja.Cells[fila, 4].Value = total;
                hoja.Cells[fila, 4].Style.Numberformat.Format = FormatoMoneda;
                hoja.Cells[fila, 2].Style.Font.Bold = true;
                hoja.Cells[fila, 4].Style.Font.Bold = true;

                hoja.Cells[2, 1, fila, Encabezados.Length].AutoFitColumns();
                return paquete.GetAsByteArray();
            }
        }

        private static EntBonoTipo Tipo(EntBono b, IDictionary<int, EntBonoTipo> tipos)
        {
            EntBonoTipo t;
            return tipos != null && tipos.TryGetValue(b.IdTipo, out t) ? t : null;
        }

        private static string Familia(EntBono b, IDictionary<int, EntBonoTipo> tipos)
        {
            EntBonoTipo t = Tipo(b, tipos);
            return t == null ? "" : t.Familia ?? "";
        }

        private static int RangoEmpresa(string empresa)
        {
            string e = NegBonoReglas.EmpresaNormalizada(empresa);
            return e == "DOS" ? 0 : e == "AGILITY" ? 1 : 2;
        }

        private static int RangoFamilia(string familia)
        {
            return familia == "Standby" ? 0 : familia == "Contrato" ? 1 : familia == "Otro" ? 2 : 3;
        }

        private static string NombreMes(int mes)
        {
            string m = new CultureInfo("es-EC").DateTimeFormat.GetMonthName(mes);
            return m.Length == 0 ? "" : char.ToUpper(m[0], CultureInfo.InvariantCulture) + m.Substring(1);
        }
    }
}
