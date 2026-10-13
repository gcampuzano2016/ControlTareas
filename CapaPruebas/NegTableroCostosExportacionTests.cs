using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace CapaPruebas
{
    /// <summary>
    /// El archivo se arma en memoria: se construye, se vuelve a abrir con EPPlus
    /// y se leen las celdas. Datos inventados: el repositorio es publico.
    /// </summary>
    [TestClass]
    public class NegTableroCostosExportacionTests
    {
        /// <summary>Dos actividades de una persona a 5 $/h: Normal 2 h ($10) y Extra 50% 1 h ($7,50).</summary>
        private static EntTableroCostos Tablero()
        {
            EntTcDatos d = new EntTcDatos();
            d.Parametros["Factor50"] = 1.5m;
            d.Parametros["Factor100"] = 2m;
            d.Parametros["DiasMes"] = 30m;
            d.Tarifas.Add(new EntTcTarifa { CodUsuario = "C1", Salario = 1200m, JornadaHorasDia = 8, AplicaHE = true });
            d.Actividades.Add(new EntTcActividad
            {
                IdDetalle = 1, FechaInicio = new DateTime(2026, 3, 5, 9, 0, 0), FechaFin = new DateTime(2026, 3, 5, 11, 0, 0),
                Tiempo = "02:00:00", CodResponsable = "C1", NomResponsable = "PERSONA DE PRUEBA",
                Empresa = "EMPRESA DE PRUEBA", Contrato = "CONTRATO DE PRUEBA", Estado = "Asignado"
            });
            d.Actividades.Add(new EntTcActividad
            {
                IdDetalle = 2, FechaInicio = new DateTime(2026, 3, 6, 19, 0, 0),
                Tiempo = "01:00:00", CodResponsable = "C1", NomResponsable = "PERSONA DE PRUEBA",
                Empresa = "EMPRESA DE PRUEBA", Contrato = "CONTRATO DE PRUEBA", Estado = "En progreso",
                TipoHE = 1, EstadoHE = 2
            });
            return NegTableroCostos.Armar(NegTableroCostos.Clasificar(d), new EntTcFiltro());
        }

        private static ExcelPackage Abrir(byte[] archivo)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return new ExcelPackage(new MemoryStream(archivo));
        }

        private static int Columna(string nombre)
        {
            return Array.IndexOf(NegTableroCostosExportacion.ColumnasDetalle, nombre) + 1;
        }

        [TestMethod]
        public void Construir_TieneHojaResumenYDetalle()
        {
            using (ExcelPackage p = Abrir(NegTableroCostosExportacion.Construir(Tablero(), "filtros")))
            {
                Assert.IsNotNull(p.Workbook.Worksheets[NegTableroCostosExportacion.HojaResumen]);
                Assert.IsNotNull(p.Workbook.Worksheets[NegTableroCostosExportacion.HojaDetalle]);
            }
        }

        [TestMethod]
        public void Detalle_CabeceraYUnaFilaPorActividad()
        {
            using (ExcelPackage p = Abrir(NegTableroCostosExportacion.Construir(Tablero(), "filtros")))
            {
                ExcelWorksheet ws = p.Workbook.Worksheets[NegTableroCostosExportacion.HojaDetalle];
                for (int c = 0; c < NegTableroCostosExportacion.ColumnasDetalle.Length; c++)
                {
                    Assert.AreEqual(NegTableroCostosExportacion.ColumnasDetalle[c], ws.Cells[1, c + 1].Text);
                }
                Assert.AreEqual(3, ws.Dimension.End.Row);
            }
        }

        [TestMethod]
        public void Detalle_HorasFactorYValorSonNumeros()
        {
            using (ExcelPackage p = Abrir(NegTableroCostosExportacion.Construir(Tablero(), "filtros")))
            {
                ExcelWorksheet ws = p.Workbook.Worksheets[NegTableroCostosExportacion.HojaDetalle];
                Assert.AreEqual(2m, Convert.ToDecimal(ws.Cells[2, Columna("Horas")].Value, CultureInfo.InvariantCulture));
                Assert.AreEqual(10m, Convert.ToDecimal(ws.Cells[2, Columna("Valor $")].Value, CultureInfo.InvariantCulture));
                Assert.AreEqual(1.5m, Convert.ToDecimal(ws.Cells[3, Columna("Factor")].Value, CultureInfo.InvariantCulture));
                Assert.AreEqual(7.5m, Convert.ToDecimal(ws.Cells[3, Columna("Valor $")].Value, CultureInfo.InvariantCulture));
                Assert.AreEqual("Extra", ws.Cells[3, Columna("Tipo Hora")].Text);
                Assert.AreEqual("Aprobado", ws.Cells[3, Columna("Estado HE")].Text);
            }
        }

        [TestMethod]
        public void Resumen_LlevaLosFiltrosYLasOnceFilas()
        {
            using (ExcelPackage p = Abrir(NegTableroCostosExportacion.Construir(Tablero(), "Desde 01/03/2026 · Hasta 31/03/2026")))
            {
                ExcelWorksheet ws = p.Workbook.Worksheets[NegTableroCostosExportacion.HojaResumen];
                Assert.AreEqual("Desde 01/03/2026 · Hasta 31/03/2026", ws.Cells[2, 1].Text);

                int primera = NegTableroCostosExportacion.FilaCabeceraResumen + 1;
                Assert.AreEqual("Normal", ws.Cells[primera, 1].Text);
                Assert.AreEqual("Total al 100% (Extra + Traslado)", ws.Cells[primera + 10, 1].Text);
                Assert.AreEqual(17.5m, Convert.ToDecimal(ws.Cells[primera + 8, 4].Value, CultureInfo.InvariantCulture));
            }
        }

        [TestMethod]
        public void Construir_SinActividades_NoFalla()
        {
            EntTableroCostos vacio = NegTableroCostos.Armar(new EntTcClasificado(), new EntTcFiltro());

            using (ExcelPackage p = Abrir(NegTableroCostosExportacion.Construir(vacio, "")))
            {
                ExcelWorksheet ws = p.Workbook.Worksheets[NegTableroCostosExportacion.HojaDetalle];
                Assert.AreEqual(NegTableroCostosExportacion.ColumnasDetalle[0], ws.Cells[1, 1].Text);
            }
        }

        [TestMethod]
        public void NombreDeArchivo_ConCulturaEspanola_UsaDigitosFijos()
        {
            CultureInfo antes = Thread.CurrentThread.CurrentCulture;
            try
            {
                // th-TH usa el calendario budista (anio + 543): sin InvariantCulture el nombre saldria 25682...
                Thread.CurrentThread.CurrentCulture = new CultureInfo("th-TH");
                Assert.AreEqual("TableroCostos_20250101_20261231.xlsx",
                    NegTableroCostosExportacion.NombreDeArchivo(new DateTime(2025, 1, 1), new DateTime(2026, 12, 31)));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = antes;
            }
        }
    }
}
