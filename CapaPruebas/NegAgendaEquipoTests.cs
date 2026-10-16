using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    /// <summary>Datos inventados a proposito: el repositorio es publico.</summary>
    [TestClass]
    public class NegAgendaEquipoTests
    {
        internal static EntAgendaPersona P(string cod, string jefe, int idArea = 1, string area = "AREA A",
            bool sinArea = false, string nombre = null, string correo = "")
        {
            return new EntAgendaPersona
            {
                CodUsuario = cod,
                CodJefe = jefe,
                Nombre = nombre ?? ("PERSONA " + cod),
                Correo = correo,
                IdArea = idArea,
                Area = area,
                Color = "#0078D4",
                SinArea = sinArea
            };
        }

        /// <summary>G (cima) -> J1 -> C1, C2 ;  G -> J2 -> C3 -> C4.</summary>
        internal static List<EntAgendaPersona> Organigrama()
        {
            return new List<EntAgendaPersona>
            {
                P("G", "0", 1, "GERENCIA"),
                P("J1", "G", 2, "SERVICIOS"), P("C1", "J1", 2, "SERVICIOS"), P("C2", "J1", 2, "SERVICIOS"),
                P("J2", "G", 3, "COMERCIAL"), P("C3", "J2", 3, "COMERCIAL"), P("C4", "C3", 3, "COMERCIAL")
            };
        }

        private static List<string> Lista(HashSet<string> c) { return c.OrderBy(x => x).ToList(); }

        [TestMethod]
        public void Cadena_ReportesDirectos()
        {
            CollectionAssert.AreEquivalent(new[] { "C1", "C2" }, Lista(NegAgendaEquipo.Cadena(Organigrama(), "J1")));
        }

        [TestMethod]
        public void Cadena_VariosNiveles_TodaLaRamaHaciaAbajo()
        {
            CollectionAssert.AreEquivalent(new[] { "C3", "C4" }, Lista(NegAgendaEquipo.Cadena(Organigrama(), "J2")));
            CollectionAssert.AreEquivalent(new[] { "J1", "C1", "C2", "J2", "C3", "C4" }, Lista(NegAgendaEquipo.Cadena(Organigrama(), "G")));
        }

        [TestMethod]
        public void Cadena_SinGenteACargo_Vacia()
        {
            Assert.AreEqual(0, NegAgendaEquipo.Cadena(Organigrama(), "C4").Count);
        }

        [TestMethod]
        public void Cadena_Ciclo_NoSeCuelgaNiSeIncluyeASiMismo()
        {
            List<EntAgendaPersona> l = new List<EntAgendaPersona> { P("A", "B"), P("B", "A"), P("C", "B") };
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, Lista(NegAgendaEquipo.Cadena(l, "A")));
        }

        [TestMethod]
        public void Cadena_JefeDeSiMismo_SeIgnora()
        {
            List<EntAgendaPersona> l = new List<EntAgendaPersona> { P("A", "A"), P("B", "A") };
            CollectionAssert.AreEquivalent(new[] { "B" }, Lista(NegAgendaEquipo.Cadena(l, "A")));
        }

        [TestMethod]
        public void Cadena_JefeFueraDeLosActivos_NadieMasLaVe()
        {
            List<EntAgendaPersona> l = Organigrama();
            l.Add(P("X", "INACTIVO"));
            l.Add(P("Y", "X"));
            CollectionAssert.AreEquivalent(new[] { "Y" }, Lista(NegAgendaEquipo.Cadena(l, "X")));
            Assert.IsFalse(NegAgendaEquipo.Cadena(l, "G").Contains("X"));
        }

        [TestMethod]
        public void Cadena_IgnoraMayusculasYEspacios()
        {
            List<EntAgendaPersona> l = new List<EntAgendaPersona> { P("J1", "G"), P(" c1 ", "j1 ") };
            Assert.IsTrue(NegAgendaEquipo.Cadena(l, "J1").Contains("C1"));
        }

        [TestMethod]
        public void Cadena_PersonaRepetida_NoDuplica()
        {
            List<EntAgendaPersona> l = new List<EntAgendaPersona> { P("C1", "J1"), P("C1", "J1") };
            Assert.AreEqual(1, NegAgendaEquipo.Cadena(l, "J1").Count);
        }

        [TestMethod]
        public void Cadena_RaizVaciaONula_Vacia()
        {
            Assert.AreEqual(0, NegAgendaEquipo.Cadena(Organigrama(), "").Count);
            Assert.AreEqual(0, NegAgendaEquipo.Cadena(Organigrama(), null).Count);
            Assert.AreEqual(0, NegAgendaEquipo.Cadena(null, "G").Count);
        }

        [TestMethod]
        public void PuedeVer_ASiMismo_ASuCadena_NoAOtraRama()
        {
            HashSet<string> cadena = NegAgendaEquipo.Cadena(Organigrama(), "J1");
            Assert.IsTrue(NegAgendaEquipo.PuedeVer("J1", "J1", cadena, false));
            Assert.IsTrue(NegAgendaEquipo.PuedeVer("J1", "c1", cadena, false));
            Assert.IsFalse(NegAgendaEquipo.PuedeVer("J1", "C3", cadena, false));
            Assert.IsFalse(NegAgendaEquipo.PuedeVer("J1", "G", cadena, false));
        }

        [TestMethod]
        public void PuedeVer_VerTodos_ACualquiera_PeroNoAVacio()
        {
            Assert.IsTrue(NegAgendaEquipo.PuedeVer("C1", "C3", new HashSet<string>(), true));
            Assert.IsFalse(NegAgendaEquipo.PuedeVer("C1", " ", new HashSet<string>(), true));
        }

        [TestMethod]
        public void PuedeAsignarA_NuncaASiMismo()
        {
            Assert.IsFalse(NegAgendaEquipo.PuedeAsignarA("G", "G", NegAgendaEquipo.Cadena(Organigrama(), "G"), true));
        }

        [TestMethod]
        public void PuedeAsignarA_SuCadena_OVerTodos()
        {
            HashSet<string> cadena = NegAgendaEquipo.Cadena(Organigrama(), "J2");
            Assert.IsTrue(NegAgendaEquipo.PuedeAsignarA("J2", "C4", cadena, false));
            Assert.IsFalse(NegAgendaEquipo.PuedeAsignarA("J2", "C1", cadena, false));
            Assert.IsTrue(NegAgendaEquipo.PuedeAsignarA("J2", "C1", cadena, true));
        }
    }
}
