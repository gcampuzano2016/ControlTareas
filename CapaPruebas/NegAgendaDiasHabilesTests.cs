using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CapaPruebas
{
    [TestClass]
    public class NegAgendaDiasHabilesTests
    {
        private static DateTime D(int m, int d) { return new DateTime(2026, m, d); }
        private static ISet<DateTime> F(params DateTime[] f) { return new HashSet<DateTime>(f); }

        [TestMethod] public void Sumar_UnDiaDesdeMiercoles_Jueves() => Assert.AreEqual(D(10,22), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,21), 1, F()));
        [TestMethod] public void Sumar_UnDiaDesdeViernes_Lunes() => Assert.AreEqual(D(10,26), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,23), 1, F()));
        [TestMethod] public void Sumar_SaltaFeriado() => Assert.AreEqual(D(10,13), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,8), 2, F(D(10,9))));
        [TestMethod] public void Sumar_FeriadoEnLunesTrasViernes_Martes() => Assert.AreEqual(D(10,27), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,23), 1, F(D(10,26))));
        [TestMethod] public void Sumar_CruzaAnio() => Assert.AreEqual(new DateTime(2027,1,1), NegAgendaDiasHabiles.SumarDiasHabiles(D(12,31), 1, F()));
        [TestMethod] public void Sumar_CruzaAnioConFeriado() => Assert.AreEqual(new DateTime(2027,1,4), NegAgendaDiasHabiles.SumarDiasHabiles(D(12,31), 1, F(new DateTime(2027,1,1))));
        [TestMethod] public void Sumar_CincoDias_UnaSemana() => Assert.AreEqual(D(10,28), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,21), 5, F()));
        [TestMethod] public void Sumar_DesdeSabado_CuentaDesdeLunes() => Assert.AreEqual(D(10,26), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,24), 1, F()));
        [TestMethod] public void Sumar_IgnoraHora() => Assert.AreEqual(D(10,22), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,21).AddHours(23), 1, F()));
        [TestMethod] [ExpectedException(typeof(ArgumentOutOfRangeException))] public void Sumar_PlazoCero_Error() => NegAgendaDiasHabiles.SumarDiasHabiles(D(10,21), 0, F());
    }
}
