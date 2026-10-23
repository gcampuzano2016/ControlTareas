using CapaEntidad;
using CapaNegocio;
using System;
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

        private static EntAgendaProceso Proc()
        {
            return new EntAgendaProceso { IdProceso = 1, Nombre = "Alta de proveedor", CodIniciadoPor = "INI1" };
        }

        private static EntAgendaProcesoPaso Paso(string resp)
        {
            return new EntAgendaProcesoPaso { Orden = 2, Titulo = "Revisar contrato", Area = "Legal", CodResponsable = resp, Entregables = "Contrato firmado" };
        }

        [TestMethod]
        public void DebeAvisarPaso_IniciadorEsResponsable_No()
        {
            Assert.IsFalse(NegAgendaCorreo.DebeAvisarPaso(Proc(), Paso(" ini1 ")));
        }

        [TestMethod]
        public void DebeAvisarPaso_OtroResponsable_Si()
        {
            Assert.IsTrue(NegAgendaCorreo.DebeAvisarPaso(Proc(), Paso("RESP9")));
            Assert.IsFalse(NegAgendaCorreo.DebeAvisarPaso(Proc(), Paso("  ")));
            Assert.IsFalse(NegAgendaCorreo.DebeAvisarPaso(null, Paso("RESP9")));
        }

        [TestMethod]
        public void AsuntoPaso_Formato()
        {
            Assert.AreEqual("Te toca un paso: Revisar contrato \u2014 Alta de proveedor", NegAgendaCorreo.AsuntoPaso(Proc(), Paso("RESP9")));
        }

        [TestMethod]
        public void AsuntoPaso_SinSaltosDeLinea()
        {
            EntAgendaProcesoPaso paso = Paso("RESP9");
            paso.Titulo = "Revisar\r\ncontrato\tfinal";
            EntAgendaProceso p = Proc();
            p.Nombre = "Alta\ny baja";
            string a = NegAgendaCorreo.AsuntoPaso(p, paso);
            Assert.AreEqual("Te toca un paso: Revisar contrato final \u2014 Alta y baja", a);
            Assert.IsFalse(a.Contains("\r") || a.Contains("\n") || a.Contains("\t"));
        }

        [TestMethod]
        public void CuerpoPaso_EscapaHtmlYTildes()
        {
            EntAgendaProcesoPaso paso = Paso("RESP9");
            paso.Titulo = "<b>Revisi\u00f3n</b>";
            paso.Entregables = "<script>x</script>";
            string c = NegAgendaCorreo.CuerpoPaso(Proc(), paso, "Ver\u00f3nica", "<i>Ana</i>", new DateTime(2026, 10, 30), "https://x/RTareas");
            StringAssert.Contains(c, "&lt;b&gt;Revisi&#243;n&lt;/b&gt;");
            StringAssert.Contains(c, "&lt;script&gt;");
            StringAssert.Contains(c, "&lt;i&gt;Ana&lt;/i&gt;");
            StringAssert.Contains(c, "Ver&#243;nica");
            Assert.IsFalse(c.Contains("<script>"));
        }

        [TestMethod]
        public void CuerpoPaso_TraeFechaYEnlace()
        {
            string c = NegAgendaCorreo.CuerpoPaso(Proc(), Paso("RESP9"), "Luis", "Ana", new DateTime(2026, 10, 30), "https://x/RTareas/");
            StringAssert.Contains(c, "30 de octubre de 2026");
            StringAssert.Contains(c, "https://x/RTareas/Formulario/Agenda.aspx");
            StringAssert.Contains(c, "Alta de proveedor");
            string sin = NegAgendaCorreo.CuerpoPaso(Proc(), Paso("RESP9"), "Luis", "Ana", new DateTime(2026, 10, 30), "");
            Assert.IsFalse(sin.Contains("href"));
        }

        [TestMethod]
        public void AsuntoTerminado_Formato()
        {
            EntAgendaProceso p = Proc();
            p.Nombre = "Alta\r\nde proveedor";
            Assert.AreEqual("Proceso terminado: Alta de proveedor", NegAgendaCorreo.AsuntoTerminado(p));
        }

        [TestMethod]
        public void CuerpoTerminado_ListaTodosLosPasos()
        {
            EntAgendaProceso p = Proc();
            p.Pasos.Add(new EntAgendaProcesoPaso { Orden = 1, Titulo = "Pedir <datos>", CodResponsable = "A1", FechaCompletado = new DateTime(2026, 10, 27) });
            p.Pasos.Add(new EntAgendaProcesoPaso { Orden = 2, Titulo = "Firmar", CodResponsable = "B2", FechaCompletado = new DateTime(2026, 10, 29) });
            p.Pasos.Add(new EntAgendaProcesoPaso { Orden = 3, Titulo = "Archivar", CodResponsable = "C3" });
            var nombres = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            { { "a1", "Ana" }, { "B2", "Beto" } };
            string c = NegAgendaCorreo.CuerpoTerminado(p, nombres, "https://x/RTareas");
            StringAssert.Contains(c, "Pedir &lt;datos&gt;");
            StringAssert.Contains(c, "Firmar");
            StringAssert.Contains(c, "Archivar");
            StringAssert.Contains(c, "Ana");
            StringAssert.Contains(c, "Beto");
            StringAssert.Contains(c, "C3");
            StringAssert.Contains(c, "27 de octubre de 2026");
            StringAssert.Contains(c, "29 de octubre de 2026");
            StringAssert.Contains(c, "Alta de proveedor");
        }
    }
}
