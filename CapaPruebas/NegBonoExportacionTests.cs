using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CapaPruebas
{
    /// <summary>Datos inventados: el repositorio es publico.</summary>
    [TestClass]
    public class NegBonoExportacionTests
    {
        private const int FilaTitulo = 1, FilaEncabezado = 2, PrimeraFila = 3;

        private static EntBonoTipo Tipo(int id, string nombre, string familia, bool porSemana)
        {
            EntBonoTipo t = new EntBonoTipo { IdTipo = id, Nombre = nombre, Familia = familia, PorSemana = porSemana, Activo = true };
            t.Conceptos["DOS"] = "C-DOS";
            t.Conceptos["AGILITY"] = "C-AGI";
            return t;
        }

        private static Dictionary<int, EntBonoTipo> Catalogo()
        {
            return new Dictionary<int, EntBonoTipo>
            {
                { 1, Tipo(1, "Standby Cisco UIO", "Standby", true) },
                { 2, Tipo(2, "Standby GPF", "Standby", true) },
                { 3, Tipo(3, "Contrato Nestlé", "Contrato", false) },
                { 4, Tipo(4, "Cumplimiento", "Otro", false) }
            };
        }

        private static EntBono Bono(string nombre, string empresa, int idTipo, decimal valor, string ident = "1", params DateTime[] lunes)
        {
            EntBono b = new EntBono { Nombre = nombre, Empresa = empresa, IdTipo = idTipo, Valor = valor, Identificacion = ident, Estado = BonoEstados.Aprobado };
            b.Lunes.AddRange(lunes);
            return b;
        }

        private static EntBonoPeriodo Periodo(string estado)
        {
            return new EntBonoPeriodo { Anio = 2026, Mes = 9, Estado = estado };
        }

        private static ExcelWorksheet Leer(byte[] bytes, out ExcelPackage paquete)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            paquete = new ExcelPackage(new MemoryStream(bytes));
            return paquete.Workbook.Worksheets.First();
        }

        private static string Texto(ExcelWorksheet h, int fila, int col)
        {
            object v = h.Cells[fila, col].Value;
            return v == null ? "" : Convert.ToString(v);
        }

        [TestMethod]
        public void Observacion_PorSemana()
        {
            EntBono b = Bono("A", "DOS", 1, 40m, "1", new DateTime(2026, 8, 3), new DateTime(2026, 8, 10));
            Assert.AreEqual("2 semanas Standby Cisco UIO", NegBonoExportacion.Observacion(b, Catalogo()[1]));
        }

        [TestMethod]
        public void Observacion_UnaSemana_Singular()
        {
            EntBono b = Bono("A", "DOS", 2, 40m, "1", new DateTime(2026, 8, 3));
            Assert.AreEqual("1 semana Standby GPF", NegBonoExportacion.Observacion(b, Catalogo()[2]));
        }

        [TestMethod]
        public void Observacion_ConNotaConTildes()
        {
            EntBono b = Bono("A", "DOS", 3, 100m);
            b.Observacion = "cubrió a Andrés";
            Assert.AreEqual("Contrato Nestlé · cubrió a Andrés", NegBonoExportacion.Observacion(b, Catalogo()[3]));
        }

        [TestMethod]
        public void Construir_Columnas()
        {
            byte[] bytes = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Cerrado), new List<EntBono> { Bono("A", "DOS", 4, 10m) }, Catalogo());
            ExcelPackage p;
            ExcelWorksheet h = Leer(bytes, out p);
            using (p)
            {
                string[] esperado = { "EMPRESA", "NOMBRES APELLIDOS", "ID SAP", "VALOR", "OBSERVACION", "FECHAS", "Lugar", "CONCEPTO SAP" };
                for (int i = 0; i < esperado.Length; i++)
                    Assert.AreEqual(esperado[i], Texto(h, FilaEncabezado, i + 1));
                Assert.AreEqual("", Texto(h, FilaEncabezado, 9));
            }
        }

        [TestMethod]
        public void Construir_IdSapComoTexto_ConservaCeros()
        {
            byte[] bytes = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Cerrado), new List<EntBono> { Bono("A", "AGILITY", 4, 10m, "0912345678") }, Catalogo());
            ExcelPackage p;
            ExcelWorksheet h = Leer(bytes, out p);
            using (p)
            {
                Assert.IsInstanceOfType(h.Cells[PrimeraFila, 3].Value, typeof(string));
                Assert.AreEqual("0912345678", h.Cells[PrimeraFila, 3].Value);
            }
        }

        [TestMethod]
        public void Construir_OrdenEmpresaFamiliaNombre()
        {
            List<EntBono> lista = new List<EntBono>
            {
                Bono("Zeta", "AGILITY", 1, 1m),
                Bono("Mario", "DOS", 4, 1m),
                Bono("Beto", "DOS", 3, 1m),
                Bono("Zoe", "DOS", 1, 1m),
                Bono("Ana", "DOS", 1, 1m),
                Bono("Carlos", "DOS", 3, 1m)
            };
            byte[] bytes = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Cerrado), lista, Catalogo());
            ExcelPackage p;
            ExcelWorksheet h = Leer(bytes, out p);
            using (p)
            {
                string[] esperado = { "Ana", "Zoe", "Beto", "Carlos", "Mario", "Zeta" };
                for (int i = 0; i < esperado.Length; i++)
                    Assert.AreEqual(esperado[i], Texto(h, PrimeraFila + i, 2));
                Assert.AreEqual("DOS", Texto(h, PrimeraFila, 1));
                Assert.AreEqual("AGILITY", Texto(h, PrimeraFila + 5, 1));
            }
        }

        [TestMethod]
        public void Ordenar_NombresConTildeEnEspanol()
        {
            // Ordinal pondria "Ángel" y "Óscar" despues de "Zoe".
            List<EntBono> lista = new List<EntBono>
            {
                Bono("Zoe", "DOS", 1, 1m),
                Bono("Óscar", "DOS", 1, 1m),
                Bono("beto", "DOS", 1, 1m),
                Bono("Ángel", "DOS", 1, 1m),
                Bono("Andrés", "DOS", 1, 1m)
            };
            string[] orden = NegBonoExportacion.Ordenar(lista, Catalogo()).Select(b => b.Nombre).ToArray();
            CollectionAssert.AreEqual(new[] { "Andrés", "Ángel", "beto", "Óscar", "Zoe" }, orden);
        }

        [TestMethod]
        public void Construir_FilaTotalSinSubtotales()
        {
            List<EntBono> lista = new List<EntBono> { Bono("A", "DOS", 4, 10.50m), Bono("B", "DOS", 3, 20m), Bono("C", "AGILITY", 1, 5.25m) };
            byte[] bytes = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Cerrado), lista, Catalogo());
            ExcelPackage p;
            ExcelWorksheet h = Leer(bytes, out p);
            using (p)
            {
                int ultima = h.Dimension.End.Row;
                Assert.AreEqual(PrimeraFila + 3, ultima);
                Assert.AreEqual("TOTAL", Texto(h, ultima, 2));
                Assert.AreEqual(35.75m, Convert.ToDecimal(h.Cells[ultima, 4].Value));
                for (int f = PrimeraFila; f < ultima; f++)
                    Assert.AreNotEqual("TOTAL", Texto(h, f, 2), "no debe haber otra fila de suma");
            }
        }

        [TestMethod]
        public void Construir_MesAbierto_TituloProvisional()
        {
            byte[] abierto = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Abierto), new List<EntBono>(), Catalogo());
            byte[] cerrado = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Cerrado), new List<EntBono>(), Catalogo());
            ExcelPackage p1, p2;
            ExcelWorksheet h1 = Leer(abierto, out p1);
            ExcelWorksheet h2 = Leer(cerrado, out p2);
            using (p1) using (p2)
            {
                Assert.AreEqual("Base de ingresos - Septiembre 2026 - PROVISIONAL", Texto(h1, FilaTitulo, 1));
                Assert.AreEqual("Base de ingresos - Septiembre 2026", Texto(h2, FilaTitulo, 1));
            }
        }

        [TestMethod]
        public void Construir_MesCerrado_UsaCopiasNoCatalogo()
        {
            EntBono b = Bono("A", "DOS", 4, 10m);
            b.NombreTipo = "Nombre viejo";
            b.CodigoConcepto = "VIEJO";
            List<EntBono> lista = new List<EntBono> { b };

            byte[] cerrado = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Cerrado), lista, Catalogo());
            byte[] abierto = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Abierto), lista, Catalogo());
            ExcelPackage p1, p2;
            ExcelWorksheet hc = Leer(cerrado, out p1);
            ExcelWorksheet ha = Leer(abierto, out p2);
            using (p1) using (p2)
            {
                Assert.AreEqual("Nombre viejo", Texto(hc, PrimeraFila, 5));
                Assert.AreEqual("VIEJO", Texto(hc, PrimeraFila, 8));
                Assert.AreEqual("Cumplimiento", Texto(ha, PrimeraFila, 5));
                Assert.AreEqual("C-DOS", Texto(ha, PrimeraFila, 8));
            }
        }

        [TestMethod]
        public void Construir_MesAbierto_SinConcepto_CeldaVacia()
        {
            Dictionary<int, EntBonoTipo> cat = Catalogo();
            cat[4].Conceptos.Remove("AGILITY");
            byte[] bytes = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Abierto), new List<EntBono> { Bono("A", "AGILITY", 4, 10m) }, cat);
            ExcelPackage p;
            ExcelWorksheet h = Leer(bytes, out p);
            using (p)
                Assert.AreEqual("", Texto(h, PrimeraFila, 8));
        }

        [TestMethod]
        public void Construir_MesCerrado_SemanasDelBono_SinDependerDelCatalogo()
        {
            Dictionary<int, EntBonoTipo> cat = Catalogo();
            cat[2].PorSemana = false;
            EntBono sinTipo = Bono("A", "DOS", 99, 40m, "1", new DateTime(2026, 8, 3), new DateTime(2026, 8, 10));
            sinTipo.NombreTipo = "Standby X";
            EntBono tipoCambiado = Bono("B", "DOS", 2, 40m, "1", new DateTime(2026, 8, 3));
            tipoCambiado.NombreTipo = "Standby GPF";
            byte[] bytes = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Cerrado), new List<EntBono> { sinTipo, tipoCambiado }, cat);
            ExcelPackage p;
            ExcelWorksheet h = Leer(bytes, out p);
            using (p)
            {
                // Standby (tipo 2) va antes que el tipo faltante (familia desconocida, al final).
                Assert.AreEqual("B", Texto(h, PrimeraFila, 2));
                Assert.AreEqual("1 semana Standby GPF", Texto(h, PrimeraFila, 5));
                Assert.AreEqual("Del 3 al 9 de agosto", Texto(h, PrimeraFila, 6));
                Assert.AreEqual("2 semanas Standby X", Texto(h, PrimeraFila + 1, 5));
                Assert.AreEqual("Del 3 al 16 de agosto", Texto(h, PrimeraFila + 1, 6));
            }
        }

        [TestMethod]
        public void Construir_Fechas_YLugar()
        {
            EntBono b = Bono("A", "DOS", 1, 40m, "1", new DateTime(2026, 8, 3), new DateTime(2026, 8, 10));
            b.Lugar = "Quito & Guayaquil";
            byte[] bytes = NegBonoExportacion.Construir(Periodo(BonoPeriodoEstados.Cerrado), new List<EntBono> { b }, Catalogo());
            ExcelPackage p;
            ExcelWorksheet h = Leer(bytes, out p);
            using (p)
            {
                Assert.AreEqual("Del 3 al 16 de agosto", Texto(h, PrimeraFila, 6));
                Assert.AreEqual("Quito & Guayaquil", Texto(h, PrimeraFila, 7));
            }
        }
    }
}
