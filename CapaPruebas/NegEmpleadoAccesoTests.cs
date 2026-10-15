using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    [TestClass]
    public class NegEmpleadoAccesoTests
    {
        [TestMethod]
        public void ConsultaLista_PideListaYVeLista_TodosLosEmpleados()
        {
            string cod; int tipo;
            Assert.IsTrue(NegEmpleadoAcceso.ConsultaLista(true, false, true, "jperez", out cod, out tipo));
            Assert.AreEqual("", cod);
            Assert.AreEqual(NegEmpleadoAcceso.TipoTodos, tipo);
        }

        [TestMethod]
        public void ConsultaLista_PideListaSinVerLista_SoloSuFicha()
        {
            string cod; int tipo;
            Assert.IsTrue(NegEmpleadoAcceso.ConsultaLista(false, true, true, "jperez", out cod, out tipo));
            Assert.AreEqual("jperez", cod);
            Assert.AreEqual(NegEmpleadoAcceso.TipoPropia, tipo);
        }

        [TestMethod]
        public void ConsultaLista_NoPideLista_SoloSuFichaAunqueVeaLista()
        {
            string cod; int tipo;
            Assert.IsTrue(NegEmpleadoAcceso.ConsultaLista(true, true, false, "jperez", out cod, out tipo));
            Assert.AreEqual("jperez", cod);
            Assert.AreEqual(NegEmpleadoAcceso.TipoPropia, tipo);
        }

        [TestMethod]
        public void ConsultaLista_SinNingunaPantalla_Niega()
        {
            string cod; int tipo;
            Assert.IsFalse(NegEmpleadoAcceso.ConsultaLista(false, false, true, "jperez", out cod, out tipo));
        }

        [TestMethod]
        public void ConsultaLista_FichaPropiaSinUsuario_Niega()
        {
            string cod; int tipo;
            Assert.IsFalse(NegEmpleadoAcceso.ConsultaLista(false, true, false, "  ", out cod, out tipo));
            Assert.IsFalse(NegEmpleadoAcceso.ConsultaLista(true, true, false, null, out cod, out tipo));
        }

        [TestMethod]
        public void TipoPropia_NoEsElQueDevuelveTodos()
        {
            Assert.AreEqual(0, NegEmpleadoAcceso.TipoTodos);
            Assert.AreNotEqual(NegEmpleadoAcceso.TipoTodos, NegEmpleadoAcceso.TipoPropia);
        }
    }
}
