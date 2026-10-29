using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CapaPruebas
{
    [TestClass]
    public class NegVacMiasTests
    {
        [TestMethod]
        public void Aviso_UnRegistro_NoAvisa()
        {
            Assert.AreEqual("", NegVacMias.Aviso(new List<int> { 7 }));
        }

        [TestMethod]
        public void Aviso_SinRegistro_PideEscribirATalentoHumano()
        {
            Assert.AreEqual(NegVacMias.AvisoSinRegistro, NegVacMias.Aviso(new List<int>()));
            Assert.AreEqual(NegVacMias.AvisoSinRegistro, NegVacMias.Aviso(null));
        }

        [TestMethod]
        public void Aviso_VariosRegistros_NoEligeNinguno()
        {
            Assert.AreEqual(NegVacMias.AvisoAmbiguo, NegVacMias.Aviso(new List<int> { 7, 9 }));
        }
    }
}
