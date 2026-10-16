using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    [TestClass]
    public class NegAgendaCorreoTests
    {
        private static EntAgendaActividad Asignada()
        {
            EntAgendaActividad a = NegAgendaActividadTests.A("C1", "J1", NegAgendaActividadTests.Jueves15, false, false);
            a.Titulo = "Preparar informe mensual";
            a.Entregables[0].Descripcion = "Borrador";
            a.Entregables[1].Descripcion = "Versión final";
            return a;
        }

        [TestMethod]
        public void DebeAvisar_SoloSiLaAsignoOtraPersona()
        {
            Assert.IsTrue(NegAgendaCorreo.DebeAvisar(Asignada()));
            Assert.IsFalse(NegAgendaCorreo.DebeAvisar(NegAgendaActividadTests.A("C1", "C1", NegAgendaActividadTests.Jueves15)));
            Assert.IsFalse(NegAgendaCorreo.DebeAvisar(null));
        }

        [TestMethod]
        public void Asunto_LlevaElTitulo()
        {
            Assert.AreEqual("Nueva actividad asignada: Preparar informe mensual", NegAgendaCorreo.Asunto(Asignada()));
        }

        [TestMethod]
        public void Asunto_QuitaSaltosDeLinea()
        {
            EntAgendaActividad a = Asignada();
            a.Titulo = "Linea uno\r\nLinea dos";
            Assert.AreEqual("Nueva actividad asignada: Linea uno Linea dos", NegAgendaCorreo.Asunto(a));
        }

        [TestMethod]
        public void Cuerpo_TituloFechaEntregablesYQuienAsigno()
        {
            string c = NegAgendaCorreo.Cuerpo(Asignada(), "Ana Perez", "Luis Mora", "https://portal.ejemplo/RTareas/");
            StringAssert.Contains(c, "Preparar informe mensual");
            StringAssert.Contains(c, "jueves 15 de octubre de 2026");
            StringAssert.Contains(c, "Borrador");
            StringAssert.Contains(c, "Luis Mora");
            StringAssert.Contains(c, "Ana Perez");
            StringAssert.Contains(c, "https://portal.ejemplo/RTareas/Formulario/Agenda.aspx");
            StringAssert.Contains(c, "Ver en mi agenda");
        }

        [TestMethod]
        public void Cuerpo_SinUrlDelSitio_SinBoton()
        {
            string c = NegAgendaCorreo.Cuerpo(Asignada(), "Ana", "Luis", "  ");
            Assert.IsFalse(c.Contains("Ver en mi agenda"));
        }

        [TestMethod]
        public void Cuerpo_EscapaHtmlYTildes()
        {
            EntAgendaActividad a = Asignada();
            a.Titulo = "<script>alert(1)</script>";
            string c = NegAgendaCorreo.Cuerpo(a, "Verónica", "Luis", "");
            Assert.IsFalse(c.Contains("<script>"));
            StringAssert.Contains(c, "&lt;script&gt;");
            StringAssert.Contains(c, "Ver&#243;nica");
        }

        [TestMethod]
        public void Enlace_QuitaLaBarraFinal()
        {
            Assert.AreEqual("https://x/RTareas/Formulario/Agenda.aspx", NegAgendaCorreo.Enlace("https://x/RTareas//"));
            Assert.AreEqual("", NegAgendaCorreo.Enlace(null));
        }
    }
}
