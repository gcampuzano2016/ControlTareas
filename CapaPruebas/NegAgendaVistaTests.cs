using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegAgendaVistaTests
    {
        private static readonly DateTime L = NegAgendaActividadTests.Lunes12;
        private static readonly DateTime Hoy = NegAgendaActividadTests.Jueves15;

        private static EntAgendaActividad A(string resp, string asig, DateTime entrega, params bool[] e)
        {
            return NegAgendaActividadTests.A(resp, asig, entrega, e);
        }

        private static List<EntAgendaPersona> ConSinArea()
        {
            List<EntAgendaPersona> l = NegAgendaEquipoTests.Organigrama();
            l.Add(NegAgendaEquipoTests.P("C5", "J2", 9, "Sin área", true));
            return l;
        }

        [TestMethod]
        public void Iniciales_DosPrimerasPalabras()
        {
            Assert.AreEqual("PH", NegAgendaVista.Iniciales("PINOS HERNANDEZ CARLOS"));
            Assert.AreEqual("A", NegAgendaVista.Iniciales("  ana "));
            Assert.AreEqual("?", NegAgendaVista.Iniciales(""));
            Assert.AreEqual("?", NegAgendaVista.Iniciales(null));
        }

        [TestMethod]
        public void Contar_SemanaYAtrasadasDeCualquierFecha()
        {
            EntAgendaActividad anulada = A("C1", "C1", L.AddDays(2));
            anulada.Anulada = true;
            List<EntAgendaActividad> acts = new List<EntAgendaActividad>
            {
                A("C1", "C1", L, true, true),          // en la semana, completa
                A("C1", "J1", L.AddDays(1), false),    // en la semana, atrasada
                A("C1", "C1", L.AddDays(4)),           // en la semana, pendiente
                A("C1", "C1", L.AddDays(7)),           // semana siguiente: no cuenta
                A("C1", "C1", L.AddDays(-7)),          // semana anterior, atrasada
                anulada                                 // no cuenta para nada
            };
            EntAgendaContador c = NegAgendaVista.Contar(acts, L, Hoy);
            Assert.AreEqual(3, c.Actividades);
            Assert.AreEqual(1, c.Completas);
            Assert.AreEqual(2, c.Atrasadas);
            Assert.AreEqual(3, c.EntregablesTotal);
            Assert.AreEqual(2, c.EntregablesHechos);
            Assert.AreEqual(67, c.Porcentaje);
        }

        [TestMethod]
        public void Contar_SinEntregables_PorcentajeNull()
        {
            Assert.IsNull(NegAgendaVista.Contar(new[] { A("C1", "C1", L) }, L, Hoy).Porcentaje);
            Assert.AreEqual(0, NegAgendaVista.Contar(null, L, Hoy).Actividades);
        }

        [TestMethod]
        public void Sumar_RecalculaElPorcentajeConLosTotales()
        {
            EntAgendaContador s = NegAgendaVista.Sumar(new[]
            {
                new EntAgendaContador { Actividades = 1, Completas = 1, Atrasadas = 0, EntregablesTotal = 1, EntregablesHechos = 1 },
                new EntAgendaContador { Actividades = 2, Completas = 0, Atrasadas = 1, EntregablesTotal = 3, EntregablesHechos = 0 }
            });
            Assert.AreEqual(3, s.Actividades);
            Assert.AreEqual(1, s.Completas);
            Assert.AreEqual(1, s.Atrasadas);
            Assert.AreEqual(25, s.Porcentaje);
        }

        [TestMethod]
        public void Visibles_SegunLaCadena()
        {
            CollectionAssert.AreEquivalent(new[] { "C1" }, NegAgendaVista.Visibles("C1", false, ConSinArea()).Select(p => p.CodUsuario).ToList());
            CollectionAssert.AreEquivalent(new[] { "J1", "C1", "C2" }, NegAgendaVista.Visibles("J1", false, ConSinArea()).Select(p => p.CodUsuario).ToList());
            Assert.AreEqual(8, NegAgendaVista.Visibles("C1", true, ConSinArea()).Count);
        }

        [TestMethod]
        public void Equipo_UnJefeVeSoloLasAreasDeSuGente()
        {
            List<EntAgendaTarjetaArea> t = NegAgendaVista.Equipo("J1", false, ConSinArea(), new List<EntAgendaActividad>(), L, Hoy);
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual("SERVICIOS", t[0].Area);
            CollectionAssert.AreEquivalent(new[] { "J1", "C1", "C2" }, t[0].Personas.Select(p => p.CodUsuario).ToList());
        }

        [TestMethod]
        public void Equipo_VerTodos_SinAreaAlFinal_ElRestoPorNombre()
        {
            List<EntAgendaTarjetaArea> t = NegAgendaVista.Equipo("G", true, ConSinArea(), new List<EntAgendaActividad>(), L, Hoy);
            CollectionAssert.AreEqual(new[] { "COMERCIAL", "GERENCIA", "SERVICIOS", "Sin área" }, t.Select(x => x.Area).ToList());
            Assert.IsTrue(t[3].SinArea);
        }

        [TestMethod]
        public void Equipo_ContadorDelAreaSumaASusPersonas()
        {
            List<EntAgendaActividad> acts = new List<EntAgendaActividad>
            {
                A("C1", "J1", L.AddDays(1)),            // atrasada
                A("C2", "C2", L.AddDays(3), true),      // completa
                A("C3", "C3", L.AddDays(3))             // otra area
            };
            EntAgendaTarjetaArea servicios = NegAgendaVista.Equipo("G", true, ConSinArea(), acts, L, Hoy).Single(x => x.Area == "SERVICIOS");
            Assert.AreEqual(2, servicios.Contador.Actividades);
            Assert.AreEqual(1, servicios.Contador.Atrasadas);
            Assert.AreEqual(1, servicios.Personas.Single(p => p.CodUsuario == "C1").Contador.Atrasadas);
            Assert.AreEqual("PC", servicios.Personas.Single(p => p.CodUsuario == "C1").Iniciales);
        }

        [TestMethod]
        public void Persona_NoVisible_Null()
        {
            Assert.IsNull(NegAgendaVista.Persona("C1", false, ConSinArea(), new List<EntAgendaActividad>(), "C2", L, Hoy));
            Assert.IsNull(NegAgendaVista.Persona("J1", false, ConSinArea(), new List<EntAgendaActividad>(), "NO-EXISTE", L, Hoy));
        }

        [TestMethod]
        public void Persona_PropiaYDeUnJefe()
        {
            EntAgendaVistaPersona propia = NegAgendaVista.Persona("C1", false, ConSinArea(), new List<EntAgendaActividad>(), "c1", L, Hoy);
            Assert.IsTrue(propia.EsPropia);
            Assert.IsTrue(propia.PuedeAsignar);
            Assert.AreEqual("C1", propia.CodUsuario);

            EntAgendaVistaPersona deJefe = NegAgendaVista.Persona("J1", false, ConSinArea(), new List<EntAgendaActividad>(), "C1", L, Hoy);
            Assert.IsFalse(deJefe.EsPropia);
            Assert.IsTrue(deJefe.PuedeAsignar);
        }

        [TestMethod]
        public void Persona_CincoDiasYHoyMarcado()
        {
            EntAgendaVistaPersona v = NegAgendaVista.Persona("C1", false, ConSinArea(), new List<EntAgendaActividad>(), "C1", L, Hoy);
            CollectionAssert.AreEqual(new[] { "2026-10-12", "2026-10-13", "2026-10-14", "2026-10-15", "2026-10-16" }, v.Dias.Select(d => d.Fecha).ToList());
            Assert.AreEqual("lunes", v.Dias[0].Nombre);
            Assert.AreEqual("jueves", v.Dias[3].Nombre);
            CollectionAssert.AreEqual(new[] { false, false, false, true, false }, v.Dias.Select(d => d.EsHoy).ToList());
        }

        [TestMethod]
        public void Persona_ActividadAsignada_VistaPorElResponsable()
        {
            EntAgendaActividad a = A("C1", "J1", L.AddDays(2), false);
            EntAgendaVistaPersona v = NegAgendaVista.Persona("C1", false, ConSinArea(), new List<EntAgendaActividad> { a }, "C1", L, Hoy);
            EntAgendaItem it = v.Dias[2].Actividades.Single();
            Assert.AreEqual(a.IdActividad, it.IdActividad);
            Assert.AreEqual("PERSONA J1", it.AsignadoPor);
            Assert.IsFalse(it.Propia);
            Assert.IsTrue(it.PuedeMarcar);
            Assert.IsFalse(it.PuedeEditar);
            Assert.IsTrue(it.Atrasada);
            Assert.AreEqual(1, it.Entregables.Count);
        }

        [TestMethod]
        public void Persona_AtrasadasDeSemanasAnteriores_EnLaFranja_SinAnuladasNiDeOtros()
        {
            EntAgendaActividad anulada = A("C1", "C1", L.AddDays(-3));
            anulada.Anulada = true;
            List<EntAgendaActividad> acts = new List<EntAgendaActividad>
            {
                A("C1", "C1", L.AddDays(-10)),
                A("C1", "C1", L.AddDays(-3), true),   // completa: no
                anulada,                              // anulada: no
                A("C2", "C2", L.AddDays(-3))          // de otro: no
            };
            EntAgendaVistaPersona v = NegAgendaVista.Persona("C1", false, ConSinArea(), acts, "C1", L, Hoy);
            Assert.AreEqual(1, v.Atrasadas.Count);
            Assert.AreEqual("2026-10-02", v.Atrasadas[0].FechaEntrega);
            Assert.AreEqual(0, v.Dias.Sum(d => d.Actividades.Count));
        }
    }
}
