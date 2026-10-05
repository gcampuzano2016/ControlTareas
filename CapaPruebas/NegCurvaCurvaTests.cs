using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    /// <summary>
    /// Indice, alerta y semaforo de la curva (especificacion, seccion 7.6).
    /// Datos inventados: el repositorio es publico.
    /// </summary>
    [TestClass]
    public class NegCurvaCurvaTests
    {
        private const string Cinco = "[\"Excelente\",\"Bueno\",\"Regular\",\"Malo\",\"Muy malo\"]";
        private const string Cuatro = "[\"Excelente\",\"Buena\",\"Regular\",\"Preocupante\"]";

        private static EntCurvaRespuesta R(string tipo, string opciones, string valor)
        {
            return new EntCurvaRespuesta { TipoRespuesta = tipo, OpcionesJson = opciones, ValorRespuesta = valor };
        }

        private static EntCurvaInsumoEvaluacion Ev(string rol, string etapa, params EntCurvaRespuesta[] rs)
        {
            var e = new EntCurvaInsumoEvaluacion { Rol = rol, EtapaCodigo = etapa };
            e.Respuestas.AddRange(rs);
            return e;
        }

        [TestMethod]
        public void RedondearJs_MedioSubeComoEnJavaScript()
        {
            Assert.AreEqual(73, NegCurvaCurva.RedondearJs(72.5));
            Assert.AreEqual(1, NegCurvaCurva.RedondearJs(0.5));
            Assert.AreEqual(72, NegCurvaCurva.RedondearJs(72.49));
        }

        [TestMethod]
        public void Nivel_Umbrales70y85()
        {
            Assert.AreEqual("seguimiento", NegCurvaCurva.Nivel(69));
            Assert.AreEqual("atencion", NegCurvaCurva.Nivel(70));
            Assert.AreEqual("atencion", NegCurvaCurva.Nivel(84));
            Assert.AreEqual("ok", NegCurvaCurva.Nivel(85));
        }

        [TestMethod]
        public void Colaborador_PrimeraOpcionEs100UltimaEs0()
        {
            Assert.AreEqual(100, NegCurvaCurva.IndiceEvaluacion(Ev("colaborador", "30", R("escala", Cinco, "Excelente"))));
            Assert.AreEqual(0, NegCurvaCurva.IndiceEvaluacion(Ev("colaborador", "30", R("escala", Cinco, "Muy malo"))));
            // Bueno = 0.75, Regular = 0.5 -> 62.5 -> 63 (no 62 bancario)
            Assert.AreEqual(63, NegCurvaCurva.IndiceEvaluacion(Ev("colaborador", "30", R("escala", Cinco, "Bueno"), R("escala", Cinco, "Regular"))));
        }

        [TestMethod]
        public void Colaborador_SeguimientosCuentan()
        {
            var e = Ev("colaborador", "60", R("escala", Cinco, "Excelente"));
            e.EstadosSeguimiento.Add("no_realizado");
            e.EstadosSeguimiento.Add("en_progreso");
            // (1 + 0 + 0.5) / 3 = 0.5
            Assert.AreEqual(50, NegCurvaCurva.IndiceEvaluacion(e));
        }

        [TestMethod]
        public void OtroConTexto_SiNo_YMultiple_NoCuentan()
        {
            var e = Ev("colaborador", "30",
                R("escala", "[\"A\",\"Otros\"]", "Otros: algo"),
                R("si_no", "[\"Sí\",\"No\"]", "No"),
                R("opcion_multiple", "[\"A\",\"B\"]", "[\"B\"]"),
                R("texto_abierto", null, "hola"));
            Assert.IsNull(NegCurvaCurva.IndiceEvaluacion(e));
        }

        [TestMethod]
        public void ValorGuardadoComoJsonString_SeLeeIgual()
        {
            Assert.AreEqual(100, NegCurvaCurva.IndiceEvaluacion(Ev("colaborador", "30", R("escala", Cinco, "\"Excelente\""))));
        }

        [TestMethod]
        public void Jefe_EscalaPorCincoMasCompetencias()
        {
            // posicion Buena = 2/3 -> x5 = 3.333; competencias 4 y 2 -> promedio (3.333+4+2)/3 = 3.111 /5 = 62.2 -> 62
            var e = Ev("jefe", "90",
                R("escala", Cuatro, "Buena"),
                R("competencias_multi", "[\"A\",\"B\"]", "{\"A\":4,\"B\":2}"));
            Assert.AreEqual(62, NegCurvaCurva.IndiceEvaluacion(e));
        }

        [TestMethod]
        public void Jefe_TodoEnUltimaOpcionDaCero()
        {
            var e = Ev("jefe", "30", R("escala", Cuatro, "Preocupante"), R("escala", Cuatro, "Preocupante"));
            Assert.AreEqual(0, NegCurvaCurva.IndiceEvaluacion(e));
        }

        [TestMethod]
        public void Calcular_OrdenaPorEtapaYOmiteSinValores()
        {
            var s = NegCurvaCurva.Calcular(new[]
            {
                Ev("colaborador", "90", R("escala", Cinco, "Excelente")),
                Ev("colaborador", "30", R("escala", Cinco, "Malo")),
                Ev("colaborador", "60", R("texto_abierto", null, "x")),
                Ev("jefe", "30", R("escala", Cuatro, "Excelente"))
            });
            CollectionAssert.AreEqual(new[] { "30", "90" }, s.Colaborador.Select(p => p.Etapa).ToArray());
            Assert.AreEqual(1, s.Jefe.Count);
        }

        [TestMethod]
        public void Alerta_SinPuntosEsNullNoOk()
        {
            var a = NegCurvaCurva.CalcularAlerta(new EntCurvaSerie());
            Assert.IsNull(a.Nivel);
        }

        [TestMethod]
        public void Alerta_TomaElUltimoPuntoYGanaSeguimiento()
        {
            var s = new EntCurvaSerie();
            s.Colaborador.Add(new EntCurvaPunto { Etapa = "30", Indice = 10 });
            s.Colaborador.Add(new EntCurvaPunto { Etapa = "60", Indice = 75 });
            s.Jefe.Add(new EntCurvaPunto { Etapa = "30", Indice = 13 });
            var a = NegCurvaCurva.CalcularAlerta(s);
            Assert.AreEqual("seguimiento", a.Nivel);
            Assert.AreEqual("Jefe calificó 13/100 en etapa 30", a.Motivo);
        }

        [TestMethod]
        public void Alerta_AtencionListaTodosLosBajos()
        {
            var s = new EntCurvaSerie();
            s.Colaborador.Add(new EntCurvaPunto { Etapa = "60", Indice = 75 });
            s.Jefe.Add(new EntCurvaPunto { Etapa = "30", Indice = 80 });
            var a = NegCurvaCurva.CalcularAlerta(s);
            Assert.AreEqual("atencion", a.Nivel);
            Assert.AreEqual("Colaborador calificó 75/100 en etapa 60; Jefe calificó 80/100 en etapa 30", a.Motivo);
        }

        [TestMethod]
        public void Alerta_TodoAltoEsOkSinMotivo()
        {
            var s = new EntCurvaSerie();
            s.Colaborador.Add(new EntCurvaPunto { Etapa = "30", Indice = 90 });
            var a = NegCurvaCurva.CalcularAlerta(s);
            Assert.AreEqual("ok", a.Nivel);
            Assert.IsNull(a.Motivo);
        }

        [TestMethod]
        public void AlertaPorEtapa_ElCombinadoEsElPeor()
        {
            var s = new EntCurvaSerie();
            s.Colaborador.Add(new EntCurvaPunto { Etapa = "30", Indice = 90 });
            s.Jefe.Add(new EntCurvaPunto { Etapa = "30", Indice = 50 });
            s.Colaborador.Add(new EntCurvaPunto { Etapa = "60", Indice = 72 });
            var l = NegCurvaCurva.AlertaPorEtapa(s);
            Assert.AreEqual(2, l.Count);
            Assert.AreEqual("seguimiento", l[0].Nivel);
            Assert.AreEqual("ok", l[0].Colaborador.Nivel);
            Assert.AreEqual("atencion", l[1].Nivel);
            Assert.IsNull(l[1].Jefe);
        }

        [TestMethod]
        public void CompetenciasJefe_ClavesFijas90y365()
        {
            var d = NegCurvaCurva.CompetenciasJefe(new[] { Ev("jefe", "90", R("competencias_multi", null, "{\"A\":3}")) });
            Assert.AreEqual(3, d["90"]["A"]);
            Assert.IsTrue(d.ContainsKey("365"));
            Assert.IsNull(d["365"]);
        }
    }
}
