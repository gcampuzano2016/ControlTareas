using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegVacEstadoTests
    {
        static EntVacPeriodo P(int n, decimal ganados, decimal tomados, string estado = "GOZADO")
            => new EntVacPeriodo
            {
                N = n, Periodo = (2019 + n) + "-" + (2020 + n), DiasGanados = ganados, DiasTomados = tomados, Estado = estado,
                Inicio = new DateTime(2019 + n, 4, 2), Fin = new DateTime(2020 + n, 4, 1), Vence = new DateTime(2023 + n, 4, 2)
            };

        static EntVacMovimiento M(int id, decimal descuenta, string situacion = "DESCUENTA")
            => new EntVacMovimiento
            {
                IdMovimiento = id, Tipo = "VACACIONES", Estado = "APROBADO", Situacion = situacion, DiasDescuenta = descuenta,
                Dias = descuenta, Desde = new DateTime(2024, 1, id), Hasta = new DateTime(2024, 1, id)
            };

        static EntVacFicha Ficha(decimal ajuste, EntVacPeriodo[] periodos, params EntVacMovimiento[] movs)
            => new EntVacFicha
            {
                FechaCalculo = new DateTime(2026, 10, 28),
                Colaborador = new EntVacColaborador
                {
                    IdColaborador = 1, Nombre = "PEREZ & <ANA>", Cedula = "0000000000", Empresa = "DOS", Estado = "ACTIVO",
                    FechaIngreso = new DateTime(2019, 4, 2)
                },
                Saldo = new EntVacSaldo
                {
                    IdColaborador = 1, AjusteCorte = ajuste, FechaCalculo = new DateTime(2026, 10, 28), DiasPorAnio = 18,
                    Disponible = 1.25m, Proporcional = 10.41m, UsadoEnCurso = 0m, Saldo = 11.66m, Estado = "AL DIA",
                    ProximoAniversario = new DateTime(2027, 4, 2)
                },
                Periodos = periodos.ToList(),
                Movimientos = movs.ToList()
            };

        // ---------- PeriodoDeCadaSolicitud ----------
        [TestMethod]
        public void Periodo_LlenaDelMasAntiguoAlSiguiente()
        {
            var f = Ficha(0m, new[] { P(1, 15, 15), P(2, 15, 3, "VIGENTE") }, M(1, 10), M(2, 5), M(3, 3));
            var r = NegVacEstado.PeriodoDeCadaSolicitud(f);
            Assert.AreEqual("2020-2021", r[1]);
            Assert.AreEqual("2020-2021", r[2]);
            Assert.AreEqual("2021-2022", r[3], "con 15 previos ya empieza el periodo 2: el limite se incluye");
        }

        [TestMethod]
        public void Periodo_ElVencidoConSaldoDejaDeAbsorber()
        {
            /* El periodo 1 vencio habiendo absorbido solo 10 de 15: el limite es 10, no 15. */
            var f = Ficha(0m, new[] { P(1, 15, 10, "VENCIDO"), P(2, 15, 4, "VIGENTE") }, M(1, 10), M(2, 4));
            var r = NegVacEstado.PeriodoDeCadaSolicitud(f);
            Assert.AreEqual("2020-2021", r[1]);
            Assert.AreEqual("2021-2022", r[2]);
        }

        [TestMethod]
        public void Periodo_ElAjusteDeCorteCuentaComoYaTomado()
        {
            var f = Ficha(16m, new[] { P(1, 15, 15, "CERRADO AL CORTE"), P(2, 15, 3, "VIGENTE") }, M(1, 2));
            Assert.AreEqual("2021-2022", NegVacEstado.PeriodoDeCadaSolicitud(f)[1]);
        }

        [TestMethod]
        public void Periodo_EnCursoOFuturoEsAnticipo()
        {
            var f = Ficha(0m, new[] { P(1, 15, 15), P(2, 15, 2, "EN CURSO") }, M(1, 15), M(2, 2));
            Assert.AreEqual("2021-2022 (anticipo)", NegVacEstado.PeriodoDeCadaSolicitud(f)[2]);
        }

        [TestMethod]
        public void Periodo_LimiteConFraccionesNoSeCorre()
        {
            /* El caso real que fallaba con 4 decimales: 530,13125 previos contra un limite de 530,13125. */
            var f = Ficha(510m, new[] { P(1, 510, 510, "CERRADO AL CORTE"), P(2, 30, 20.13125m, "VENCIDO"), P(3, 30, 1, "VENCIDO") },
                          M(1, 19.95m), M(2, 0.18125m), M(3, 1m));
            var r = NegVacEstado.PeriodoDeCadaSolicitud(f);
            Assert.AreEqual("2021-2022", r[2]);
            Assert.AreEqual("2022-2023", r[3]);
        }

        [TestMethod]
        public void Periodo_LoQueNoDescuentaNoAparece()
        {
            var f = Ficha(0m, new[] { P(1, 15, 2, "VIGENTE") }, M(1, 2), M(2, 0m, "ANULADO"), M(3, 0m, "ANTES DEL CORTE"));
            var r = NegVacEstado.PeriodoDeCadaSolicitud(f);
            CollectionAssert.AreEqual(new[] { 1 }, r.Keys.ToArray());
        }

        // ---------- Antiguedad ----------
        [TestMethod]
        public void Antiguedad_AniosYMesesCumplidos()
        {
            Assert.AreEqual("7 años 6 meses", NegVacEstado.Antiguedad(new DateTime(2019, 4, 2), new DateTime(2026, 10, 30)));
            Assert.AreEqual("7 años 5 meses", NegVacEstado.Antiguedad(new DateTime(2019, 4, 2), new DateTime(2026, 10, 1)));
            Assert.AreEqual("1 año 1 mes", NegVacEstado.Antiguedad(new DateTime(2025, 1, 15), new DateTime(2026, 2, 15)));
            Assert.AreEqual("0 años 0 meses", NegVacEstado.Antiguedad(new DateTime(2026, 1, 1), new DateTime(2025, 1, 1)));
        }

        // ---------- Mensaje ----------
        [TestMethod]
        public void Mensaje_SaldoPositivo()
        {
            var s = new EntVacSaldo { Disponible = 1.42m, Proporcional = 10.41m, Saldo = 11.83m };
            string m = NegVacEstado.Mensaje(s);
            StringAssert.Contains(m, "1,42 días disponibles");
            StringAssert.Contains(m, "su saldo a la fecha es de 11,83 días.");
            Assert.IsFalse(m.Contains("anticipo"));
        }

        [TestMethod]
        public void Mensaje_AnticipoYPorVencer()
        {
            var s = new EntVacSaldo
            {
                Disponible = 0m, Proporcional = 2.22m, UsadoEnCurso = 6m, Saldo = -3.78m,
                PorVencer = 1m, ProximoVencimiento = new DateTime(2026, 12, 1)
            };
            string m = NegVacEstado.Mensaje(s);
            StringAssert.Contains(m, "ya usó 6,00 días");
            StringAssert.Contains(m, "anticipo de 3,78 días");
            StringAssert.Contains(m, "1,00 día vencen el 01/12/2026");
        }

        // ---------- Html ----------
        [TestMethod]
        public void Html_LlevaFolioDatosYEscapa()
        {
            var f = Ficha(0m, new[] { P(1, 15, 2, "VIGENTE") }, M(1, 2), M(2, 0m, "ANULADO"));
            var e = new EntVacEstado { Folio = 42, Emitido = new DateTime(2026, 10, 29, 9, 5, 0), Usuario = "gth01", Ficha = f };

            string h = NegVacEstado.Html(e, "", "");

            StringAssert.Contains(h, "Folio N.º 000042");
            StringAssert.Contains(h, "PEREZ &amp; &lt;ANA&gt;");
            Assert.IsFalse(h.Contains("<ANA>"));
            StringAssert.Contains(h, "28 de octubre de 2026");
            StringAssert.Contains(h, "DETALLE POR PERIODO");
            StringAssert.Contains(h, "Talento Humano y Cultura");
            StringAssert.Contains(h, "No se listan 1 solicitud registrada que no descuenta");
            StringAssert.Contains(h, "por gth01");
        }

        [TestMethod]
        public void Html_InactivoSeCalculaASuSalida()
        {
            var f = Ficha(0m, new[] { P(1, 15, 0, "VIGENTE") });
            f.Saldo.FechaCalculo = new DateTime(2026, 10, 2);
            f.Colaborador.Estado = "INACTIVO";
            f.Colaborador.FechaSalida = new DateTime(2026, 10, 2);
            string h = NegVacEstado.Html(new EntVacEstado { Folio = 1, Emitido = DateTime.Now, Usuario = "x", Ficha = f }, "", "");
            string texto = System.Net.WebUtility.HtmlDecode(h);
            StringAssert.Contains(texto, "2 de octubre de 2026 (fecha de salida)");
            StringAssert.Contains(texto, "salió el 02/10/2026");
        }

        [TestMethod]
        public void Html_SinFichaDevuelveVacio()
        {
            Assert.AreEqual("", NegVacEstado.Html(null, "", ""));
            Assert.AreEqual("", NegVacEstado.Html(new EntVacEstado(), "", ""));
        }
    }
}
