using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegAgendaAreasTests
    {
        [TestMethod]
        public void Validar_NombreYColor()
        {
            Assert.IsNull(NegAgendaAreas.Validar("Hybrid Cloud", "#0078D4"));
            Assert.IsNull(NegAgendaAreas.Validar("Hybrid Cloud", "#a1b2c3"));
            Assert.AreEqual("Escriba el nombre del área.", NegAgendaAreas.Validar("  ", "#0078D4"));
            Assert.AreEqual("El nombre admite hasta 100 caracteres.", NegAgendaAreas.Validar(new string('a', 101), "#0078D4"));
            Assert.AreEqual("Elija un color válido.", NegAgendaAreas.Validar("Compras", "rojo"));
            Assert.AreEqual("Elija un color válido.", NegAgendaAreas.Validar("Compras", "#12345G"));
            Assert.AreEqual("Elija un color válido.", NegAgendaAreas.Validar("Compras", null));
        }

        [TestMethod]
        public void MensajeResultado_CodigosDelProcedimiento()
        {
            Assert.AreEqual("Ya existe un área con ese nombre.", NegAgendaAreas.MensajeResultado(-1));
            Assert.AreEqual("«Sin área» no se puede renombrar ni desactivar.", NegAgendaAreas.MensajeResultado(-2));
            Assert.AreEqual("El área no existe.", NegAgendaAreas.MensajeResultado(-3));
            Assert.AreEqual("No se pudo guardar el área.", NegAgendaAreas.MensajeResultado(0));
            Assert.IsNull(NegAgendaAreas.MensajeResultado(12));
        }

        [TestMethod]
        public void ConPersonas_CuentaPorArea()
        {
            List<EntAgendaArea> areas = new List<EntAgendaArea>
            {
                new EntAgendaArea { IdArea = 1, Nombre = "A" },
                new EntAgendaArea { IdArea = 2, Nombre = "B" },
                new EntAgendaArea { IdArea = 9, Nombre = "Sin área", EsSinArea = true }
            };
            List<EntAgendaPersona> personas = new List<EntAgendaPersona>
            {
                NegAgendaEquipoTests.P("X", "G", 1), NegAgendaEquipoTests.P("Y", "G", 1), NegAgendaEquipoTests.P("Z", "G", 9, "Sin área", true)
            };
            List<EntAgendaArea> r = NegAgendaAreas.ConPersonas(areas, personas);
            CollectionAssert.AreEqual(new[] { 2, 0, 1 }, r.Select(a => a.Personas).ToList());
        }
    }
}
