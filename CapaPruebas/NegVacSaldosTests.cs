using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegVacSaldosTests
    {
        static readonly DateTime Hoy = new DateTime(2026, 10, 28);

        static EntVacColaborador C(int id, string nombre, string estado = "ACTIVO")
            => new EntVacColaborador { IdColaborador = id, Nombre = nombre, Estado = estado, FechaIngreso = new DateTime(2020, 1, 1) };

        static EntVacSaldo S(int id, decimal saldo, string estado = "AL DIA", decimal porVencer = 0, decimal vencidos = 0)
            => new EntVacSaldo { IdColaborador = id, Saldo = saldo, Estado = estado, PorVencer = porVencer, Vencidos = vencidos };

        // ---------- ValidarFecha ----------
        [TestMethod]
        public void ValidarFecha_HoyYRangoValido()
        {
            Assert.AreEqual("", NegVacSaldos.ValidarFecha(Hoy, Hoy));
            Assert.AreEqual("", NegVacSaldos.ValidarFecha(new DateTime(2005, 1, 1), Hoy));
            Assert.AreEqual("", NegVacSaldos.ValidarFecha(Hoy.AddYears(3), Hoy));
        }

        [TestMethod]
        public void ValidarFecha_FueraDeRango()
        {
            Assert.AreNotEqual("", NegVacSaldos.ValidarFecha(new DateTime(2004, 12, 31), Hoy));
            Assert.AreNotEqual("", NegVacSaldos.ValidarFecha(Hoy.AddYears(3).AddDays(1), Hoy));
        }

        // ---------- Armar ----------
        [TestMethod]
        public void Armar_UneYOrdenaPorNombre()
        {
            var d = new EntVacDatos();
            d.Colaboradores.AddRange(new[] { C(1, "Zoila"), C(2, "álvaro"), C(3, "Bruno") });
            d.Saldos.AddRange(new[] { S(1, 10), S(2, 20), S(3, 30) });

            var t = NegVacSaldos.Armar(d, Hoy);

            CollectionAssert.AreEqual(new[] { "álvaro", "Bruno", "Zoila" }, t.Filas.Select(f => f.Colaborador.Nombre).ToArray());
            Assert.AreEqual(20m, t.Filas[0].Saldo.Saldo);
            Assert.AreEqual(0, t.Avisos.Count);
            Assert.AreEqual(Hoy, t.FechaCalculo);
        }

        [TestMethod]
        public void Armar_AvisaLoQueNoCasa()
        {
            var d = new EntVacDatos();
            d.Colaboradores.AddRange(new[] { C(1, "Ana"), C(2, "Beto") });
            d.Saldos.AddRange(new[] { S(1, 10), S(9, 5) });

            var t = NegVacSaldos.Armar(d, Hoy);

            Assert.AreEqual(1, t.Filas.Count);
            Assert.AreEqual(2, t.Avisos.Count);
            Assert.IsTrue(t.Avisos.Any(a => a.Contains("Beto")));
            Assert.IsTrue(t.Avisos.Any(a => a.Contains("9")));
        }

        // ---------- Resumir ----------
        [TestMethod]
        public void Resumir_SoloActivosEnLosTotales()
        {
            var d = new EntVacDatos();
            d.Colaboradores.AddRange(new[] { C(1, "A"), C(2, "B"), C(3, "C", "INACTIVO") });
            d.Saldos.AddRange(new[] { S(1, 10.5m), S(2, 4.25m), S(3, 100m) });

            var r = NegVacSaldos.Armar(d, Hoy).Resumen;

            Assert.AreEqual(2, r.Activos);
            Assert.AreEqual(1, r.Inactivos);
            Assert.AreEqual(14.75m, r.SaldoTotal);
        }

        [TestMethod]
        public void Resumir_AnticipoRestaYCuentaAparte()
        {
            var d = new EntVacDatos();
            d.Colaboradores.AddRange(new[] { C(1, "A"), C(2, "B") });
            d.Saldos.AddRange(new[] { S(1, 20m), S(2, -3.78m, "ANTICIPO") });

            var r = NegVacSaldos.Armar(d, Hoy).Resumen;

            Assert.AreEqual(16.22m, r.SaldoTotal);
            Assert.AreEqual(1, r.PersonasAnticipo);
            Assert.AreEqual(-3.78m, r.DiasAnticipo);
        }

        [TestMethod]
        public void Resumir_PorVencerVencidosYAcumulando()
        {
            var d = new EntVacDatos();
            d.Colaboradores.AddRange(new[] { C(1, "A"), C(2, "B"), C(3, "C") });
            d.Saldos.AddRange(new[]
            {
                S(1, 40m, "POR VENCER", porVencer: 7.5m, vencidos: 2m),
                S(2, 35m, "ACUMULANDO"),
                S(3, 5m, "AL DIA", vencidos: 1.25m)
            });

            var r = NegVacSaldos.Armar(d, Hoy).Resumen;

            Assert.AreEqual(1, r.PersonasPorVencer);
            Assert.AreEqual(7.5m, r.DiasPorVencer);
            Assert.AreEqual(1, r.PersonasAcumulando);
            Assert.AreEqual(3.25m, r.DiasVencidos);
        }

        [TestMethod]
        public void EsActivo_IgnoraMayusculasYNulo()
        {
            Assert.IsTrue(NegVacSaldos.EsActivo(C(1, "A", "activo")));
            Assert.IsFalse(NegVacSaldos.EsActivo(C(1, "A", "INACTIVO")));
            Assert.IsFalse(NegVacSaldos.EsActivo(null));
        }
    }
}
