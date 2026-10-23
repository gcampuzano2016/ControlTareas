using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CapaPruebas
{
    [TestClass]
    public class NegBonoFechasTests
    {
        static DateTime D(int m, int d) => new DateTime(2026, m, d);
        static readonly DateTime Hoy = D(10, 22); // jueves

        [TestMethod] public void Terminada_DomingoAyerOAntes_Si() => Assert.IsTrue(NegBonoFechas.Terminada(D(10, 12), Hoy));
        [TestMethod] public void Terminada_SemanaEnCurso_No() => Assert.IsFalse(NegBonoFechas.Terminada(D(10, 19), Hoy));
        [TestMethod] public void Terminada_DomingoEsHoy_No() => Assert.IsFalse(NegBonoFechas.Terminada(D(10, 12), D(10, 18)));
        [TestMethod] public void Validar_NoLunes() => StringAssert.Contains(NegBonoFechas.ValidarSemanas(new[] { D(10, 13) }, true, Hoy), "lunes");
        [TestMethod] public void Validar_AntesDelPrimerLunes() => StringAssert.Contains(NegBonoFechas.ValidarSemanas(new[] { D(7, 27) }, true, Hoy), "3 de agosto");
        [TestMethod] public void Validar_PrimerLunesExacto_Vale() => Assert.IsNull(NegBonoFechas.ValidarSemanas(new[] { D(8, 3) }, true, Hoy));
        [TestMethod] public void Validar_PorSemanaSinSemanas() => Assert.IsNotNull(NegBonoFechas.ValidarSemanas(new DateTime[0], true, Hoy));
        [TestMethod] public void Validar_NoPorSemanaConSemanas() => Assert.IsNotNull(NegBonoFechas.ValidarSemanas(new[] { D(10, 5) }, false, Hoy));
        [TestMethod] public void Validar_Repetida() => Assert.IsNotNull(NegBonoFechas.ValidarSemanas(new[] { D(10, 5), D(10, 5) }, true, Hoy));

        // Los tres primeros son los ejemplos del spec 6, que son lunes de 2025.
        static DateTime D25(int m, int d) => new DateTime(2025, m, d);
        [TestMethod] public void Texto_DosConsecutivasMismoMes() => Assert.AreEqual("Del 18 al 31 de agosto", NegBonoFechas.TextoFechas(new[] { D25(8, 25), D25(8, 18) }));
        [TestMethod] public void Texto_CruzaMes() => Assert.AreEqual("Del 28 de julio al 3 de agosto", NegBonoFechas.TextoFechas(new[] { D25(7, 28) }));
        [TestMethod] public void Texto_SalteadasMismoMes() => Assert.AreEqual("Del 4 al 10 y del 25 al 31 de agosto", NegBonoFechas.TextoFechas(new[] { D25(8, 4), D25(8, 25) }));
        [TestMethod] public void Texto_TresTramos() => Assert.AreEqual("Del 3 al 9, del 17 al 23 y del 31 de agosto al 6 de septiembre", NegBonoFechas.TextoFechas(new[] { D(8, 3), D(8, 17), D(8, 31) }));
        [TestMethod] public void Texto_DosMesesSeparados() => Assert.AreEqual("Del 3 al 9 de agosto y del 14 al 20 de septiembre", NegBonoFechas.TextoFechas(new[] { D(8, 3), D(9, 14) }));
        [TestMethod] public void Texto_Vacio() => Assert.AreEqual("", NegBonoFechas.TextoFechas(new DateTime[0]));
    }
}
