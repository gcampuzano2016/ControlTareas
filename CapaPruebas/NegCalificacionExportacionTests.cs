using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace CapaPruebas
{
    [TestClass]
    public class NegCalificacionExportacionTests
    {
        private static EntPantallaCalificacion Pantalla()
        {
            EntCalificacionForeCast o = new EntCalificacionForeCast
            {
                IdForeCast = 9, Cliente = "Cliente A", DetalleProyecto = "Proyecto A", Prioridad = "PIPELINE",
                GerentedeCuenta = "G1", PVPEstimado = 1234.5m, MontoPonderado = 493.8m,
                Presupuesto = 12, Autoridad = 5, Necesidad = 14, Tiempo = 15, Actividad = 10, Encaje = 7,
                Puntaje = 63, Estado = "Tibia", FechaCierreTexto = "30/10/2026",
                Alertas = new List<EntAlertaCalificacion> { new EntAlertaCalificacion { Codigo = 1, Texto = "Uno" },
                                                           new EntAlertaCalificacion { Codigo = 2, Texto = "Dos" } },
                Faltantes = new List<string> { "Contacto" },
                AccionSugerida = "Enviar la propuesta"
            };
            return new EntPantallaCalificacion { Oportunidades = new List<EntCalificacionForeCast> { o }, Total = 1 };
        }

        [TestMethod]
        public void Construir_EncabezadoFilaYNumeros()
        {
            byte[] archivo = NegCalificacionExportacion.Construir(Pantalla());

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage p = new ExcelPackage(new MemoryStream(archivo)))
            {
                ExcelWorksheet h = p.Workbook.Worksheets["Calificación"];
                Assert.IsNotNull(h);
                Assert.AreEqual("ID", h.Cells[1, 1].Text);
                Assert.AreEqual("Puntaje", h.Cells[1, 15].Text);
                Assert.AreEqual(9L, Convert.ToInt64(h.Cells[2, 1].Value));
                Assert.AreEqual("Cliente A", h.Cells[2, 2].Text);
                Assert.AreEqual(1234.5m, Convert.ToDecimal(h.Cells[2, 6].Value));   // numero, no texto
                Assert.AreEqual(63, Convert.ToInt32(h.Cells[2, 15].Value));
                Assert.AreEqual("Uno; Dos", h.Cells[2, 17].Text);
                Assert.IsNull(h.Cells[3, 1].Value);
            }
        }

        [TestMethod]
        public void Construir_Vacio_SoloEncabezado()
        {
            byte[] archivo = NegCalificacionExportacion.Construir(new EntPantallaCalificacion());
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage p = new ExcelPackage(new MemoryStream(archivo)))
            {
                Assert.IsNull(p.Workbook.Worksheets["Calificación"].Cells[2, 1].Value);
            }
        }

        [TestMethod]
        public void NombreDeArchivo()
        {
            Assert.AreEqual("Calificacion_ForeCast_20260928.xlsx",
                NegCalificacionExportacion.NombreDeArchivo(new DateTime(2026, 9, 28)));
        }
    }
}