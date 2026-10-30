using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CapaPruebas
{
    [TestClass]
    public class NegVacAjustesTests
    {
        static readonly DateTime Hoy = new DateTime(2026, 10, 29);

        static EntVacColaborador Persona(DateTime? corte = null, decimal? saldoCorte = null, DateTime? salida = null)
            => new EntVacColaborador
            {
                IdColaborador = 1, Nombre = "X", Estado = salida.HasValue ? "INACTIVO" : "ACTIVO",
                FechaIngreso = new DateTime(2022, 3, 2), FechaSalida = salida, FechaCorte = corte, SaldoCorte = saldoCorte
            };

        [TestMethod]
        public void ValidarMotivo_ExigeUnMotivoDeVerdad()
        {
            Assert.AreNotEqual("", NegVacAjustes.ValidarMotivo(null));
            Assert.AreNotEqual("", NegVacAjustes.ValidarMotivo("   corto   "));
            Assert.AreEqual("", NegVacAjustes.ValidarMotivo("Acta firmada el 28/10"));
            Assert.AreNotEqual("", NegVacAjustes.ValidarMotivo(new string('x', NegVacAjustes.MotivoMaximo + 1)));
        }

        [TestMethod]
        public void ValidarCorte_FechaYSaldoVanJuntos()
        {
            Assert.AreNotEqual("", NegVacAjustes.ValidarCorte(Persona(), new DateTime(2025, 1, 1), null, Hoy));
            Assert.AreNotEqual("", NegVacAjustes.ValidarCorte(Persona(), null, 5m, Hoy));
            Assert.AreEqual("", NegVacAjustes.ValidarCorte(Persona(), new DateTime(2025, 1, 1), 12.79m, Hoy));
        }

        [TestMethod]
        public void ValidarCorte_QuitarSoloSiTiene()
        {
            Assert.AreNotEqual("", NegVacAjustes.ValidarCorte(Persona(), null, null, Hoy));
            Assert.AreEqual("", NegVacAjustes.ValidarCorte(Persona(new DateTime(2023, 2, 3), 12.79m), null, null, Hoy));
        }

        [TestMethod]
        public void ValidarCorte_FechaDentroDeLaRelacion()
        {
            Assert.AreNotEqual("", NegVacAjustes.ValidarCorte(Persona(), new DateTime(2022, 3, 1), 1m, Hoy));
            Assert.AreEqual("", NegVacAjustes.ValidarCorte(Persona(), new DateTime(2022, 3, 2), 0m, Hoy));
            Assert.AreNotEqual("", NegVacAjustes.ValidarCorte(Persona(), Hoy.AddDays(1), 1m, Hoy));
            Assert.AreNotEqual("", NegVacAjustes.ValidarCorte(Persona(salida: new DateTime(2026, 6, 30)), new DateTime(2026, 7, 1), 1m, Hoy));
        }

        [TestMethod]
        public void ValidarCorte_SaldoRazonable()
        {
            DateTime f = new DateTime(2025, 1, 1);
            Assert.AreNotEqual("", NegVacAjustes.ValidarCorte(Persona(), f, -0.01m, Hoy));
            Assert.AreNotEqual("", NegVacAjustes.ValidarCorte(Persona(), f, NegVacAjustes.SaldoCorteMaximo + 1m, Hoy));
            Assert.AreNotEqual("", NegVacAjustes.ValidarCorte(Persona(), f, 1.255m, Hoy));
        }

        [TestMethod]
        public void ValidarCorte_SinCambiosNoSeGuarda()
        {
            var p = Persona(new DateTime(2023, 2, 3), 12.79m);
            Assert.AreNotEqual("", NegVacAjustes.ValidarCorte(p, new DateTime(2023, 2, 3), 12.79m, Hoy));
            Assert.AreEqual("", NegVacAjustes.ValidarCorte(p, new DateTime(2023, 2, 3), 12.80m, Hoy));
        }

        [TestMethod]
        public void ValidarCompra_Fechas()
        {
            Assert.AreNotEqual("", NegVacAjustes.ValidarCompra(Persona(), new DateTime(2022, 3, 1), 1m, 30m, Hoy));
            Assert.AreNotEqual("", NegVacAjustes.ValidarCompra(Persona(), Hoy.AddDays(1), 1m, 30m, Hoy));
            Assert.AreNotEqual("", NegVacAjustes.ValidarCompra(Persona(salida: new DateTime(2026, 6, 30)), new DateTime(2026, 7, 1), 1m, 30m, Hoy));
            /* Antes del corte el motor no la descontaria. */
            Assert.AreNotEqual("", NegVacAjustes.ValidarCompra(Persona(new DateTime(2025, 12, 22), 10m), new DateTime(2025, 12, 21), 1m, 30m, Hoy));
            Assert.AreEqual("", NegVacAjustes.ValidarCompra(Persona(new DateTime(2025, 12, 22), 10m), new DateTime(2025, 12, 22), 1m, 30m, Hoy));
            Assert.AreEqual("", NegVacAjustes.ValidarCompra(Persona(), Hoy, 1m, 30m, Hoy));
        }

        [TestMethod]
        public void ValidarCompra_DiasDentroDelSaldo()
        {
            Assert.AreNotEqual("", NegVacAjustes.ValidarCompra(Persona(), Hoy, 0m, 30m, Hoy));
            Assert.AreNotEqual("", NegVacAjustes.ValidarCompra(Persona(), Hoy, 1.005m, 30m, Hoy));
            Assert.AreEqual("", NegVacAjustes.ValidarCompra(Persona(), Hoy, 21m, 21m, Hoy));
            StringAssert.Contains(NegVacAjustes.ValidarCompra(Persona(), Hoy, 21.5m, 21m, Hoy), "21,00");
        }

        [TestMethod]
        public void Mensaje_TraduceLosResultados()
        {
            Assert.AreEqual("", NegVacAjustes.Mensaje("OK"));
            foreach (string r in new[] { "NO EXISTE", "CAMBIADO", "SIN CAMBIOS", "NO ES COMPRA", "YA ANULADA", "", null, "OTRO" })
            {
                Assert.AreNotEqual("", NegVacAjustes.Mensaje(r), r ?? "null");
            }
        }

        [TestMethod]
        public void Compras_SoloCompras_LaUltimaPrimero()
        {
            var f = new EntVacFicha
            {
                Movimientos = new List<EntVacMovimiento>
                {
                    new EntVacMovimiento { IdMovimiento = 1, Tipo = NegVacAjustes.TipoCompra, Desde = new DateTime(2025, 8, 31) },
                    new EntVacMovimiento { IdMovimiento = 2, Tipo = "VACACIONES", Desde = new DateTime(2026, 1, 5) },
                    new EntVacMovimiento { IdMovimiento = 3, Tipo = NegVacAjustes.TipoCompra, Desde = new DateTime(2026, 8, 31), Estado = "DADA BAJA" }
                }
            };
            CollectionAssert.AreEqual(new[] { 3, 1 }, NegVacAjustes.Compras(f).ConvertAll(m => m.IdMovimiento));
        }

        static EntVacArchivo Archivo(string nombre, params byte[] inicio)
        {
            byte[] b = new byte[Math.Max(inicio.Length, 16)];
            Array.Copy(inicio, b, inicio.Length);
            return new EntVacArchivo { NombreArchivo = nombre, Contenido = b };
        }

        [TestMethod]
        public void ValidarDocumento_ExigeElDocumento()
        {
            Assert.AreNotEqual("", NegVacAjustes.ValidarDocumento(null));
            Assert.AreNotEqual("", NegVacAjustes.ValidarDocumento(new EntVacArchivo { NombreArchivo = "acta.pdf", Contenido = new byte[0] }));
        }

        [TestMethod]
        public void ValidarDocumento_MiraElContenidoYNoSoloElNombre()
        {
            var pdf = Archivo("acta.pdf", 0x25, 0x50, 0x44, 0x46);
            Assert.AreEqual("", NegVacAjustes.ValidarDocumento(pdf));
            Assert.AreEqual("application/pdf", pdf.TipoContenido);
            Assert.AreEqual("", NegVacAjustes.ValidarDocumento(Archivo("foto.JPG", 0xFF, 0xD8, 0xFF)));
            Assert.AreEqual("", NegVacAjustes.ValidarDocumento(Archivo("foto.png", 0x89, 0x50, 0x4E, 0x47)));

            Assert.AreNotEqual("", NegVacAjustes.ValidarDocumento(Archivo("acta.pdf", 0x4D, 0x5A)), "un .exe renombrado");
            Assert.AreNotEqual("", NegVacAjustes.ValidarDocumento(Archivo("acta.docx", 0x50, 0x4B)), "otro formato");
            Assert.AreNotEqual("", NegVacAjustes.ValidarDocumento(Archivo("acta.pdf.aspx", 0x25, 0x50, 0x44, 0x46)), "vale la ultima extension");
        }

        [TestMethod]
        public void ValidarDocumento_TopeDe5MB_YNombreSinRuta()
        {
            var grande = new EntVacArchivo { NombreArchivo = "acta.pdf", Contenido = new byte[NegVacAjustes.DocumentoMaximo + 1] };
            grande.Contenido[0] = 0x25; grande.Contenido[1] = 0x50; grande.Contenido[2] = 0x44; grande.Contenido[3] = 0x46;
            Assert.AreNotEqual("", NegVacAjustes.ValidarDocumento(grande));

            var conRuta = Archivo(@"C:\Users\gth\Escritorio\acta.pdf", 0x25, 0x50, 0x44, 0x46);
            Assert.AreEqual("", NegVacAjustes.ValidarDocumento(conRuta));
            Assert.AreEqual("acta.pdf", conRuta.NombreArchivo);
        }
    }
}
