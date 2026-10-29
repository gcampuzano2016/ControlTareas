using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CapaPruebas
{
    [TestClass]
    public class NegVacLiquidacionTests
    {
        static EntVacFicha Ficha(decimal saldo, params EntVacMovimiento[] movs)
            => new EntVacFicha
            {
                FechaCalculo = new DateTime(2026, 10, 28),
                Colaborador = new EntVacColaborador { IdColaborador = 1, Nombre = "X", Estado = "ACTIVO", FechaIngreso = new DateTime(2024, 9, 6) },
                Saldo = new EntVacSaldo
                {
                    IdColaborador = 1, FechaCalculo = new DateTime(2026, 10, 28), Saldo = saldo, Disponible = 12m, Proporcional = 2.14m,
                    InicioPeriodo = new DateTime(2026, 9, 6), ProximoAniversario = new DateTime(2027, 9, 6), DiasPorAnio = 15m
                },
                Periodos = new List<EntVacPeriodo>(),
                Movimientos = new List<EntVacMovimiento>(movs)
            };

        static EntVacSueldos Sueldo(decimal monto)
            => new EntVacSueldos
            {
                Vacaciones = new Dictionary<int, List<EntHeSalario>> { { 1, new List<EntHeSalario> { new EntHeSalario { Monto = monto, FechaVigenciaDesde = new DateTime(1900, 1, 1) } } } },
                DiasMes = 30m
            };

        [TestMethod]
        public void Antiguedad_ComoElDatedifDelExcel()
        {
            /* El ejemplo de la hoja LIQUIDACION. */
            Assert.AreEqual("2 años, 1 meses, 22 días", NegVacLiquidacion.Antiguedad(new DateTime(2024, 9, 6), new DateTime(2026, 10, 28)));
            Assert.AreEqual("0 años, 11 meses, 29 días", NegVacLiquidacion.Antiguedad(new DateTime(2025, 10, 29), new DateTime(2026, 10, 28)));
            Assert.AreEqual("0 años, 0 meses, 0 días", NegVacLiquidacion.Antiguedad(new DateTime(2026, 11, 1), new DateTime(2026, 10, 28)));
        }

        [TestMethod]
        public void FechaPorDefecto_SalidaRegistradaOHoy()
        {
            var hoy = new DateTime(2026, 10, 29);
            Assert.AreEqual(new DateTime(2026, 10, 2), NegVacLiquidacion.FechaPorDefecto(new EntVacColaborador { FechaSalida = new DateTime(2026, 10, 2) }, hoy));
            Assert.AreEqual(hoy, NegVacLiquidacion.FechaPorDefecto(new EntVacColaborador(), hoy));
            Assert.AreEqual(hoy, NegVacLiquidacion.FechaPorDefecto(null, hoy));
        }

        [TestMethod]
        public void ValorArt71_GanadoSobre360PorDias()
        {
            Assert.AreEqual(565.60m, NegVacLiquidacion.ValorArt71(14400m, 14.14m));
        }

        [TestMethod]
        public void ValidarGanado_NoAceptaNegativo()
        {
            Assert.AreEqual("", NegVacLiquidacion.ValidarGanado(null));
            Assert.AreEqual("", NegVacLiquidacion.ValidarGanado(0m));
            Assert.AreNotEqual("", NegVacLiquidacion.ValidarGanado(-1m));
        }

        [TestMethod]
        public void Armar_ValorReferencialArt71YLoPosterior()
        {
            var f = Ficha(14.14m,
                new EntVacMovimiento { IdMovimiento = 1, Situacion = "POSTERIOR" },
                new EntVacMovimiento { IdMovimiento = 2, Situacion = "ANULADO" });

            var l = NegVacLiquidacion.Armar(f, new DateTime(2026, 10, 28), Sueldo(1200m), 14400m);

            Assert.AreEqual(52, l.DiasTranscurridos);
            Assert.AreEqual(365, l.DiasDelPeriodo);
            Assert.AreEqual(1200m, l.Sueldo);
            Assert.AreEqual(565.60m, l.ValorReferencial, "14,14 x 1200 / 30");
            Assert.AreEqual(565.60m, l.ValorArt71);
            Assert.AreEqual(1, l.Posteriores);
            Assert.AreEqual("2 años, 1 meses, 22 días", l.Antiguedad);
        }

        [TestMethod]
        public void Armar_SinGanadoNoCalculaArt71YSinSueldoNoValoriza()
        {
            var l = NegVacLiquidacion.Armar(Ficha(5m), new DateTime(2026, 10, 28), new EntVacSueldos(), null);
            Assert.IsNull(l.ValorArt71);
            Assert.IsNull(l.Sueldo);
            Assert.IsNull(l.ValorReferencial);
        }
    }
}
