using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    [TestClass]
    public class NegAgendaActividadTests
    {
        internal static readonly DateTime Lunes12 = new DateTime(2026, 10, 12);
        internal static readonly DateTime Jueves15 = new DateTime(2026, 10, 15);

        private static int _id;

        internal static EntAgendaActividad A(string responsable, string asignadoPor, DateTime entrega, params bool[] entregables)
        {
            EntAgendaActividad a = new EntAgendaActividad
            {
                IdActividad = ++_id,
                Titulo = "ACTIVIDAD " + _id,
                CodResponsable = responsable,
                CodAsignadoPor = asignadoPor,
                FechaEntrega = entrega
            };
            int i = 0;
            foreach (bool hecho in entregables)
            {
                i++;
                a.Entregables.Add(new EntAgendaEntregable
                {
                    IdEntregable = a.IdActividad * 100 + i,
                    IdActividad = a.IdActividad,
                    Descripcion = "ENTREGABLE " + i,
                    Orden = i,
                    Hecho = hecho
                });
            }
            return a;
        }

        private static HashSet<string> CadenaDe(string yo) { return NegAgendaEquipo.Cadena(NegAgendaEquipoTests.Organigrama(), yo); }

        // ---------------------------------------------- estado

        [TestMethod]
        public void Completa_ConEntregables_SoloSiTodosEstanHechos()
        {
            Assert.IsTrue(NegAgendaActividad.Completa(A("C1", "C1", Jueves15, true, true)));
            Assert.IsFalse(NegAgendaActividad.Completa(A("C1", "C1", Jueves15, true, false)));
        }

        [TestMethod]
        public void Completa_SinEntregables_UsaLaMarcaManual()
        {
            EntAgendaActividad a = A("C1", "C1", Jueves15);
            Assert.IsFalse(NegAgendaActividad.Completa(a));
            a.CompletadaManual = true;
            Assert.IsTrue(NegAgendaActividad.Completa(a));
        }

        [TestMethod]
        public void Completa_ConEntregables_IgnoraLaMarcaManual()
        {
            EntAgendaActividad a = A("C1", "C1", Jueves15, false);
            a.CompletadaManual = true;
            Assert.IsFalse(NegAgendaActividad.Completa(a));
        }

        [TestMethod]
        public void Atrasada_VenceHoy_No()
        {
            Assert.IsFalse(NegAgendaActividad.Atrasada(A("C1", "C1", Jueves15), Jueves15.AddHours(18)));
        }

        [TestMethod]
        public void Atrasada_VencioAyer_Si()
        {
            Assert.IsTrue(NegAgendaActividad.Atrasada(A("C1", "C1", Jueves15.AddDays(-1)), Jueves15));
        }

        [TestMethod]
        public void Atrasada_CompletaOAnulada_No()
        {
            Assert.IsFalse(NegAgendaActividad.Atrasada(A("C1", "C1", Lunes12, true), Jueves15));
            EntAgendaActividad anulada = A("C1", "C1", Lunes12);
            anulada.Anulada = true;
            Assert.IsFalse(NegAgendaActividad.Atrasada(anulada, Jueves15));
        }

        [TestMethod]
        public void EsPropia_IgnoraMayusculasYEspacios()
        {
            Assert.IsTrue(NegAgendaActividad.EsPropia(A("C1", " c1 ", Jueves15)));
            Assert.IsFalse(NegAgendaActividad.EsPropia(A("C1", "J1", Jueves15)));
        }

        // ---------------------------------------------- permisos

        [TestMethod]
        public void PuedeEditar_Asignada_QuienAsignoYSuCadenaSi_ElResponsableNo()
        {
            EntAgendaActividad a = A("C1", "J1", Jueves15);
            Assert.IsTrue(NegAgendaActividad.PuedeEditar("J1", a, CadenaDe("J1"), false));
            Assert.IsTrue(NegAgendaActividad.PuedeEditar("G", a, CadenaDe("G"), false));
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("C1", a, CadenaDe("C1"), false));
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("C2", a, CadenaDe("C2"), false));
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("J2", a, CadenaDe("J2"), false));
            Assert.IsTrue(NegAgendaActividad.PuedeEditar("OTRO", a, new HashSet<string>(), true));
        }

        [TestMethod]
        public void PuedeEditar_Propia_ElResponsableYSuJefe()
        {
            EntAgendaActividad a = A("C1", "C1", Jueves15);
            Assert.IsTrue(NegAgendaActividad.PuedeEditar("C1", a, CadenaDe("C1"), false));
            Assert.IsTrue(NegAgendaActividad.PuedeEditar("J1", a, CadenaDe("J1"), false));
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("C2", a, CadenaDe("C2"), false));
        }

        [TestMethod]
        public void PuedeEditar_QuienAsignoPeroYaNoLaVe_No()
        {
            /* J2 se la asigno a C1 cuando C1 estaba en su cadena; hoy C1 depende de J1. */
            EntAgendaActividad a = A("C1", "J2", Jueves15);
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("J2", a, CadenaDe("J2"), false));
        }

        [TestMethod]
        public void PuedeEditar_Anulada_Nadie()
        {
            EntAgendaActividad a = A("C1", "J1", Jueves15);
            a.Anulada = true;
            Assert.IsFalse(NegAgendaActividad.PuedeEditar("J1", a, CadenaDe("J1"), true));
        }

        [TestMethod]
        public void PuedeMarcar_SoloElResponsable_YNoSiEstaAnulada()
        {
            EntAgendaActividad a = A("C1", "J1", Jueves15, false);
            Assert.IsTrue(NegAgendaActividad.PuedeMarcar("c1", a));
            Assert.IsFalse(NegAgendaActividad.PuedeMarcar("J1", a));
            a.Anulada = true;
            Assert.IsFalse(NegAgendaActividad.PuedeMarcar("C1", a));
        }

        // ---------------------------------------------- fechas

        [TestMethod]
        public void ProximoDiaHabil_SaltaElFinDeSemana()
        {
            Assert.AreEqual(new DateTime(2026, 10, 13), NegAgendaActividad.ProximoDiaHabil(Lunes12));
            Assert.AreEqual(new DateTime(2026, 10, 16), NegAgendaActividad.ProximoDiaHabil(Jueves15));
            Assert.AreEqual(new DateTime(2026, 10, 19), NegAgendaActividad.ProximoDiaHabil(new DateTime(2026, 10, 16)));
            Assert.AreEqual(new DateTime(2026, 10, 19), NegAgendaActividad.ProximoDiaHabil(new DateTime(2026, 10, 17)));
            Assert.AreEqual(new DateTime(2026, 10, 19), NegAgendaActividad.ProximoDiaHabil(new DateTime(2026, 10, 18, 23, 0, 0)));
        }

        [TestMethod]
        public void Lunes_DeCualquierDiaDeLaSemana()
        {
            Assert.AreEqual(Lunes12, NegAgendaActividad.Lunes(Lunes12));
            Assert.AreEqual(Lunes12, NegAgendaActividad.Lunes(Jueves15.AddHours(10)));
            Assert.AreEqual(Lunes12, NegAgendaActividad.Lunes(new DateTime(2026, 10, 18)));
        }

        // ---------------------------------------------- validacion

        private static List<EntAgendaEntregable> E(int cuantos, int largo = 10)
        {
            return Enumerable.Range(1, cuantos).Select(i => new EntAgendaEntregable { Descripcion = new string('x', largo), Orden = i }).ToList();
        }

        [TestMethod]
        public void Validar_DatosCorrectos_Null()
        {
            Assert.IsNull(NegAgendaActividad.Validar("Preparar informe", Jueves15, E(2)));
            Assert.IsNull(NegAgendaActividad.Validar("Preparar informe", Jueves15, null));
        }

        [TestMethod]
        public void Validar_Titulo()
        {
            Assert.AreEqual("Escriba qué hay que hacer.", NegAgendaActividad.Validar("   ", Jueves15, null));
            Assert.AreEqual("El título admite hasta 200 caracteres.", NegAgendaActividad.Validar(new string('a', 201), Jueves15, null));
            Assert.IsNull(NegAgendaActividad.Validar(new string('a', 200), Jueves15, null));
        }

        [TestMethod]
        public void Validar_Fecha()
        {
            Assert.AreEqual("Elija la fecha de entrega.", NegAgendaActividad.Validar("T", null, null));
            Assert.AreEqual("La fecha de entrega debe ser de lunes a viernes.", NegAgendaActividad.Validar("T", new DateTime(2026, 10, 17), null));
            Assert.AreEqual("La fecha de entrega debe ser de lunes a viernes.", NegAgendaActividad.Validar("T", new DateTime(2026, 10, 18), null));
        }

        [TestMethod]
        public void Validar_Entregables()
        {
            Assert.AreEqual("Una actividad admite hasta 20 entregables.", NegAgendaActividad.Validar("T", Jueves15, E(21)));
            Assert.AreEqual("Cada entregable admite hasta 200 caracteres.", NegAgendaActividad.Validar("T", Jueves15, E(1, 201)));
        }

        [TestMethod]
        public void Limpiar_RecortaQuitaVaciosYNumera()
        {
            List<EntAgendaEntregable> r = NegAgendaActividad.Limpiar(new[]
            {
                new EntAgendaEntregable { IdEntregable = 7, Descripcion = "  Borrador " },
                new EntAgendaEntregable { IdEntregable = 0, Descripcion = "   " },
                null,
                new EntAgendaEntregable { IdEntregable = 0, Descripcion = "Final" }
            });
            Assert.AreEqual(2, r.Count);
            Assert.AreEqual("Borrador", r[0].Descripcion);
            Assert.AreEqual(7, r[0].IdEntregable);
            Assert.AreEqual(1, r[0].Orden);
            Assert.AreEqual("Final", r[1].Descripcion);
            Assert.AreEqual(2, r[1].Orden);
            Assert.AreEqual(0, NegAgendaActividad.Limpiar(null).Count);
        }
    }
}
