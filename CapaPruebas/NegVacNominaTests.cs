using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegVacNominaTests
    {
        static readonly DateTime Hoy = new DateTime(2026, 10, 28);

        static EntVacColaborador C(int id, string empresa = "DOS", string estado = "ACTIVO")
            => new EntVacColaborador { IdColaborador = id, Nombre = "Persona " + id, Empresa = empresa, Estado = estado, FechaIngreso = new DateTime(2020, 1, 1) };

        static EntVacSaldo S(int id, decimal saldo, DateTime? calculo = null)
            => new EntVacSaldo { IdColaborador = id, Saldo = saldo, FechaCalculo = calculo ?? Hoy, Estado = saldo < 0 ? "ANTICIPO" : "AL DIA" };

        static EntHeSalario Sal(decimal monto, int anio, int mes = 1, string origen = "Rol")
            => new EntHeSalario { Monto = monto, FechaVigenciaDesde = new DateTime(anio, mes, 1), Origen = origen };

        static EntVacSueldos Sueldos(Dictionary<int, List<EntHeSalario>> he = null, Dictionary<int, List<EntHeSalario>> vac = null)
            => new EntVacSueldos
            {
                HorasExtras = he ?? new Dictionary<int, List<EntHeSalario>>(),
                Vacaciones = vac ?? new Dictionary<int, List<EntHeSalario>>(),
                DiasMes = 30m
            };

        /* Pares colaborador, saldo: sin tuplas, por la version de C# del proyecto. */
        static EntVacDatos Datos(params object[] pares)
        {
            var d = new EntVacDatos();
            for (int i = 0; i < pares.Length; i += 2)
            {
                d.Colaboradores.Add((EntVacColaborador)pares[i]);
                d.Saldos.Add((EntVacSaldo)pares[i + 1]);
            }
            return d;
        }

        // ---------- Valor ----------
        [TestMethod]
        public void Valor_DiasPorSueldoSobreTreinta()
        {
            Assert.AreEqual(473.20m, NegVacNomina.Valor(11.83m, 1200m, 30m));
            Assert.AreEqual(-151.20m, NegVacNomina.Valor(-3.78m, 1200m, 30m));
        }

        [TestMethod]
        public void Valor_RedondeaElMedioCentavoHaciaArriba()
        {
            /* 0,01 x 15 / 30 = 0,005 */
            Assert.AreEqual(0.01m, NegVacNomina.Valor(0.01m, 15m, 30m));
        }

        // ---------- Fila ----------
        [TestMethod]
        public void Fila_HorasExtrasMandaSobreVacaciones()
        {
            var s = Sueldos(new Dictionary<int, List<EntHeSalario>> { { 1, new List<EntHeSalario> { Sal(1500m, 2024) } } },
                            new Dictionary<int, List<EntHeSalario>> { { 1, new List<EntHeSalario> { Sal(900m, 1900) } } });
            var r = NegVacNomina.Fila(new EntVacFila { Colaborador = C(1), Saldo = S(1, 10m) }, s);
            Assert.AreEqual(1500m, r.Sueldo);
            Assert.AreEqual(NegVacNomina.FuenteHorasExtras, r.FuenteSueldo);
            Assert.AreEqual(500.00m, r.Valor);
        }

        [TestMethod]
        public void Fila_SinHorasExtrasUsaElDeVacaciones()
        {
            var s = Sueldos(vac: new Dictionary<int, List<EntHeSalario>> { { 1, new List<EntHeSalario> { Sal(900m, 1900) } } });
            var r = NegVacNomina.Fila(new EntVacFila { Colaborador = C(1), Saldo = S(1, 10m) }, s);
            Assert.AreEqual(900m, r.Sueldo);
            Assert.AreEqual(NegVacNomina.FuenteVacaciones, r.FuenteSueldo);
            Assert.AreEqual(30.00m, r.ValorDia);
            Assert.AreEqual(300.00m, r.Valor);
        }

        [TestMethod]
        public void Fila_InactivoUsaElSueldoVigenteASuSalida()
        {
            var he = new Dictionary<int, List<EntHeSalario>> { { 1, new List<EntHeSalario> { Sal(1000m, 2024), Sal(1300m, 2026, 6) } } };
            var r = NegVacNomina.Fila(new EntVacFila { Colaborador = C(1, estado: "INACTIVO"), Saldo = S(1, 3m, new DateTime(2026, 3, 31)) }, Sueldos(he));
            Assert.AreEqual(1000m, r.Sueldo, "el aumento de junio es posterior a la salida");
        }

        [TestMethod]
        public void Fila_SinSueldoNoTieneValorYLoDice()
        {
            var r = NegVacNomina.Fila(new EntVacFila { Colaborador = C(1), Saldo = S(1, 10m) }, Sueldos());
            Assert.IsNull(r.Sueldo);
            Assert.IsNull(r.Valor);
            Assert.AreEqual("", r.FuenteSueldo);
            Assert.AreEqual(NegVacNomina.ObsSinSueldo, r.Observacion);
        }

        [TestMethod]
        public void Fila_AnticipoDaValorNegativoYAviso()
        {
            var s = Sueldos(vac: new Dictionary<int, List<EntHeSalario>> { { 1, new List<EntHeSalario> { Sal(1200m, 1900) } } });
            var r = NegVacNomina.Fila(new EntVacFila { Colaborador = C(1), Saldo = S(1, -3.78m) }, s);
            Assert.AreEqual(-151.20m, r.Valor);
            Assert.AreEqual(NegVacNomina.ObsAnticipo, r.Observacion);
        }

        // ---------- Armar ----------
        [TestMethod]
        public void Armar_TotalesSoloDeActivosYPorEmpresa()
        {
            var vac = new Dictionary<int, List<EntHeSalario>>
            {
                { 1, new List<EntHeSalario> { Sal(1200m, 1900) } },
                { 2, new List<EntHeSalario> { Sal(600m, 1900) } },
                { 3, new List<EntHeSalario> { Sal(3000m, 1900) } }
            };
            var d = Datos(C(1, "DOS"), S(1, 10m), C(2, "AGILITY"), S(2, -2m), C(3, "DOS", "INACTIVO"), S(3, 50m), C(4, "DOS"), S(4, 5m));

            var n = NegVacNomina.Armar(d, Sueldos(vac: vac), Hoy);

            Assert.AreEqual(4, n.Filas.Count);
            Assert.AreEqual(3, n.Activos);
            Assert.AreEqual(13m, n.TotalDias);
            Assert.AreEqual(400.00m - 40.00m, n.TotalValor, "el inactivo no suma y el sin sueldo no tiene valor");
            Assert.AreEqual(1, n.SinSueldo);
            Assert.AreEqual(-40.00m, n.ValorAnticipos);

            CollectionAssert.AreEqual(new[] { "AGILITY", "DOS" }, n.PorEmpresa.Select(e => e.Empresa).ToArray());
            var dos = n.PorEmpresa.Single(e => e.Empresa == "DOS");
            Assert.AreEqual(2, dos.Personas);
            Assert.AreEqual(15m, dos.Dias);
            Assert.AreEqual(400.00m, dos.Valor);
            Assert.AreEqual(1, dos.SinSueldo);
        }
    }
}
