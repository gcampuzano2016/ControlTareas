using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CapaPruebas
{
    [TestClass]
    public class NegBonoCorreoTests
    {
        private static EntBonoPeriodo P(int id, int anio, int mes, string estado)
        {
            return new EntBonoPeriodo { IdPeriodo = id, Anio = anio, Mes = mes, Estado = estado };
        }

        private static EntBono B(int periodo, string jefe, string estado = BonoEstados.Aprobado)
        {
            return new EntBono { IdBono = periodo * 100 + (jefe ?? "").Length, IdPeriodo = periodo, CodJefe = jefe, Estado = estado };
        }

        private static List<EntBonoPeriodo> Periodos()
        {
            return new List<EntBonoPeriodo>
            {
                P(1, 2026, 9, BonoPeriodoEstados.Historico),
                P(2, 2026, 10, BonoPeriodoEstados.Cerrado),
                P(3, 2026, 11, BonoPeriodoEstados.Cerrado),
                P(4, 2026, 12, BonoPeriodoEstados.Cerrado),
                P(5, 2027, 1, BonoPeriodoEstados.Cerrado),
                P(6, 2027, 2, BonoPeriodoEstados.Abierto),
            };
        }

        [TestMethod]
        public void JefesApertura_UltimosTresCerrados()
        {
            // Cerrados: 10, 11, 12 (2026) y 1 (2027). Los tres ultimos: 11, 12, 1. El de octubre queda fuera.
            List<EntBono> bonos = new List<EntBono> { B(2, "VIEJO"), B(3, "A1"), B(4, "B2"), B(5, "C3") };
            List<string> r = NegBonoCorreo.JefesApertura(Periodos(), bonos, new List<EntBono>());
            CollectionAssert.AreEqual(new List<string> { "A1", "B2", "C3" }, r);
        }

        [TestMethod]
        public void JefesApertura_IgnoraHistoricos()
        {
            List<EntBono> bonos = new List<EntBono> { B(1, "HIST"), B(5, "C3") };
            List<string> r = NegBonoCorreo.JefesApertura(Periodos(), bonos, new List<EntBono>());
            CollectionAssert.AreEqual(new List<string> { "C3" }, r);
        }

        [TestMethod]
        public void JefesApertura_ConPendientes_Entra()
        {
            List<EntBono> pend = new List<EntBono> { new EntBono { CodJefe = " pend " } };
            List<string> r = NegBonoCorreo.JefesApertura(Periodos(), new List<EntBono> { B(5, "C3") }, pend);
            CollectionAssert.AreEqual(new List<string> { "C3", "PEND" }, r);
        }

        [TestMethod]
        public void JefesApertura_SinRepetidos()
        {
            List<EntBono> bonos = new List<EntBono> { B(3, "a1"), B(4, "A1 "), B(5, "A1"), B(5, "") };
            List<EntBono> pend = new List<EntBono> { new EntBono { CodJefe = "A1" } };
            List<string> r = NegBonoCorreo.JefesApertura(Periodos(), bonos, pend);
            CollectionAssert.AreEqual(new List<string> { "A1" }, r);
        }

        [TestMethod]
        public void AsuntoApertura_MesEnEspanol()
        {
            Assert.AreEqual("Bonos noviembre 2026: período abierto", NegBonoCorreo.AsuntoApertura(2026, 11));
            Assert.AreEqual("Bonos enero 2027: período abierto", NegBonoCorreo.AsuntoApertura(2027, 1));
        }

        [TestMethod]
        public void DestinatariosEnvio_SoloPerfil14ConCorreo()
        {
            List<EntBonoPersona> gente = new List<EntBonoPersona>
            {
                new EntBonoPersona { CodUsuario = "1", IdPerfil = 14, Correo = " th@dos.com.ec " },
                new EntBonoPersona { CodUsuario = "2", IdPerfil = 14, Correo = "TH@dos.com.ec" },
                new EntBonoPersona { CodUsuario = "3", IdPerfil = 14, Correo = "  " },
                new EntBonoPersona { CodUsuario = "4", IdPerfil = 14, Correo = null },
                new EntBonoPersona { CodUsuario = "5", IdPerfil = 2, Correo = "otro@dos.com.ec" },
                new EntBonoPersona { CodUsuario = "6", IdPerfil = 14, Correo = "ana@dos.com.ec" },
            };
            List<string> r = NegBonoCorreo.DestinatariosEnvio(gente);
            CollectionAssert.AreEqual(new List<string> { "ana@dos.com.ec", "th@dos.com.ec" }, r);
        }

        [TestMethod]
        public void Cuerpo_EscapaHtmlYTildes()
        {
            string raro = "<b>Ñandú & Cía</b>";
            EntBono b = new EntBono { Nombre = raro, NombreTipo = raro, Valor = 10m, MotivoRechazo = raro };
            string rechazo = NegBonoCorreo.CuerpoRechazo(b, "http://x/y?a=1&b=2");
            string envio = NegBonoCorreo.CuerpoEnvio(raro, 2, 1234.5m, "http://x");
            foreach (string cuerpo in new[] { rechazo, envio })
            {
                Assert.IsFalse(cuerpo.Contains("<b>Ñandú"), "sin escapar");
                Assert.IsTrue(cuerpo.Contains("&lt;b&gt;Ñandú &amp; Cía&lt;/b&gt;"), "debe salir escapado y con tildes");
            }
            Assert.IsTrue(rechazo.Contains("a=1&amp;b=2"));
            Assert.IsTrue(envio.Contains("$1.234,50"));
        }

        [TestMethod]
        public void Asuntos_EnvioYRechazo()
        {
            Assert.AreEqual("Bonos: Ana envió 3 bono(s) para revisar", NegBonoCorreo.AsuntoEnvio("Ana", 3, 10m, "u"));
            Assert.AreEqual("Bonos: se rechazó un bono de Luis", NegBonoCorreo.AsuntoRechazo(new EntBono { Nombre = "Luis" }, "u"));
        }

        [TestMethod]
        public void CuerpoApertura_Trae_Mes_Y_Enlace()
        {
            string c = NegBonoCorreo.CuerpoApertura(2026, 11, "http://x/BonosProponer.aspx");
            Assert.IsTrue(c.Contains("noviembre 2026"));
            Assert.IsTrue(c.Contains("href=\"http://x/BonosProponer.aspx\""));
        }
    }
}
