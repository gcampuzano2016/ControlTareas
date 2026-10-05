using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    /// <summary>
    /// Formato de valor_respuesta (seccion 10.3) y normalizacion del payload
    /// del formulario. Datos inventados.
    /// </summary>
    [TestClass]
    public class NegCurvaFormularioTests
    {
        private static EntCurvaPregunta P(int id, string tipo, string opciones = null, string texto = "Pregunta")
        {
            return new EntCurvaPregunta { Id = id, TipoRespuesta = tipo, OpcionesJson = opciones, TextoPregunta = texto };
        }

        [TestMethod]
        public void Serializar_StringCrudoYResto_Json()
        {
            Assert.AreEqual("Fácil", NegCurvaValorRespuesta.Serializar("Fácil"));
            Assert.AreEqual("[\"a\",\"b\"]", NegCurvaValorRespuesta.Serializar(new List<string> { "a", "b" }));
        }

        [TestMethod]
        public void ComoTexto_TextoPlanoNumeroYFecha()
        {
            Assert.AreEqual("Sí", NegCurvaValorRespuesta.ComoTexto("Sí"));
            Assert.AreEqual("123", NegCurvaValorRespuesta.ComoTexto("123"));
            Assert.AreEqual("2026-01-01", NegCurvaValorRespuesta.ComoTexto("\"2026-01-01\""));
            Assert.AreEqual("[nota] algo", NegCurvaValorRespuesta.ComoTexto("[nota] algo"));
            Assert.IsNull(NegCurvaValorRespuesta.ComoTexto(null));
        }

        [TestMethod]
        public void Compromisos_NombresHistoricosYFechaIso()
        {
            var json = NegCurvaValorRespuesta.SerializarCompromisos(new[]
            {
                new EntCurvaCompromisoFila { Descripcion = "Curso", Categoria = "Otro", FechaLimite = new DateTime(2026, 12, 1) }
            });
            Assert.AreEqual("[{\"descripcion\":\"Curso\",\"categoria\":\"Otro\",\"responsable\":null,\"fecha_limite\":\"2026-12-01\"}]", json);
            var leido = NegCurvaValorRespuesta.ComoCompromisos(json);
            Assert.AreEqual(new DateTime(2026, 12, 1), leido[0].FechaLimite);
        }

        [TestMethod]
        public void Otro_PatronYValor()
        {
            Assert.IsTrue(NegCurvaFormulario.EsOtro("Otros (especificar)"));
            Assert.IsTrue(NegCurvaFormulario.EsOtro("Otro"));
            Assert.IsFalse(NegCurvaFormulario.EsOtro("Otorgar"));
            Assert.AreEqual("Otro: horarios", NegCurvaFormulario.ValorConOtro("Otro", " horarios "));
            Assert.AreEqual("Otro", NegCurvaFormulario.ValorConOtro("Otro", "  "));
            Assert.AreEqual("A", NegCurvaFormulario.ValorConOtro("A", "texto"));
        }

        [TestMethod]
        public void Normalizar_ValidaOpcionesYAceptaOtro()
        {
            var ps = new List<EntCurvaPregunta> { P(1, "escala", "[\"Fácil\",\"Otro\"]"), P(2, "opcion_multiple", "[\"A\",\"Otros\"]") };
            var g = NegCurvaFormulario.Normalizar(ps, "{\"respuestas\":{\"1\":\"Otro: x\",\"2\":[\"A\",\"Otros: y\"],\"99\":\"ignorada\"}}");
            Assert.AreEqual("Otro: x", g.Valores[1]);
            Assert.AreEqual("[\"A\",\"Otros: y\"]", g.Valores[2]);
            Assert.IsFalse(g.Valores.ContainsKey(99));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Normalizar_RechazaOpcionInexistente()
        {
            NegCurvaFormulario.Normalizar(new List<EntCurvaPregunta> { P(1, "escala", "[\"Fácil\"]") }, "{\"respuestas\":{\"1\":\"fácil\"}}");
        }

        [TestMethod]
        public void Normalizar_SinRespuestaNoSeGuardaYTextoSeRecorta()
        {
            var ps = new List<EntCurvaPregunta> { P(1, "escala", "[\"A\",\"B\"]"), P(2, "texto_abierto"), P(3, "texto_abierto") };
            var g = NegCurvaFormulario.Normalizar(ps, "{\"respuestas\":{\"2\":\"  hola  \",\"3\":\"   \"}}");
            Assert.AreEqual(1, g.Valores.Count);
            Assert.AreEqual("hola", g.Valores[2]);
        }

        [TestMethod]
        public void Normalizar_CompromisosDescartaVaciasYResuelveOtro()
        {
            var ps = new List<EntCurvaPregunta> { P(5, "compromisos") };
            var g = NegCurvaFormulario.Normalizar(ps,
                "{\"respuestas\":{\"5\":[{\"descripcion\":\" Curso \",\"categoria\":\"Otro\",\"categoria_otro\":\"Idiomas\",\"fecha_limite\":\"\"}," +
                "{\"descripcion\":\"\",\"categoria\":\"Otro\"},{\"descripcion\":\"B\",\"categoria\":\"Otro\"}]}}");
            Assert.AreEqual(2, g.Compromisos.Count);
            Assert.AreEqual("Curso", g.Compromisos[0].Descripcion);
            Assert.AreEqual("Idiomas", g.Compromisos[0].Categoria);
            Assert.IsNull(g.Compromisos[0].FechaLimite);
            Assert.AreEqual("Otro", g.Compromisos[1].Categoria);
        }

        [TestMethod]
        public void Normalizar_CompromisosVaciosSiguenReemplazando()
        {
            var g = NegCurvaFormulario.Normalizar(new List<EntCurvaPregunta> { P(5, "compromisos") }, "{\"respuestas\":{}}");
            Assert.IsNotNull(g.Compromisos);
            Assert.AreEqual(0, g.Compromisos.Count);
        }

        [TestMethod]
        public void Normalizar_DecisionFechaSoloSiExtendido()
        {
            var ps = new List<EntCurvaPregunta> { P(7, "decision_periodo_prueba", "[\"aprobado\",\"extendido\",\"rechazado\"]") };
            var g = NegCurvaFormulario.Normalizar(ps, "{\"respuestas\":{\"7\":{\"decision\":\"aprobado\",\"nueva_fecha_fin\":\"2026-11-30\"}}}");
            Assert.AreEqual("aprobado", g.Decision.Decision);
            Assert.IsNull(g.Decision.NuevaFechaFin);
            g = NegCurvaFormulario.Normalizar(ps, "{\"respuestas\":{\"7\":{\"decision\":\"extendido\",\"nueva_fecha_fin\":\"2026-11-30\",\"comentarios\":\"x\"}}}");
            Assert.AreEqual(new DateTime(2026, 11, 30), g.Decision.NuevaFechaFin);
            Assert.AreEqual("{\"decision\":\"extendido\",\"nueva_fecha_fin\":\"2026-11-30\",\"comentarios\":\"x\"}", g.Valores[7]);
        }

        [TestMethod]
        public void Normalizar_CompetenciasSoloLasDelCatalogoYEntre1y5()
        {
            var ps = new List<EntCurvaPregunta> { P(8, "competencias_multi", "[\"Calidad\",\"Proactividad\"]") };
            var g = NegCurvaFormulario.Normalizar(ps, "{\"respuestas\":{\"8\":{\"Calidad\":\"4\",\"Inventada\":5}}}");
            Assert.AreEqual("{\"Calidad\":4}", g.Valores[8]);
        }

        [TestMethod]
        public void Normalizar_SeguimientosSoloConEstadoValido()
        {
            var g = NegCurvaFormulario.Normalizar(new List<EntCurvaPregunta>(),
                "{\"seguimientos\":[{\"compromiso_id\":3,\"estado\":\"realizado\",\"justificacion\":\" \"},{\"compromiso_id\":4,\"estado\":\"\"},{\"compromiso_id\":0,\"estado\":\"pendiente\"}]}");
            Assert.AreEqual(1, g.Seguimientos.Count);
            Assert.AreEqual(3, g.Seguimientos[0].CompromisoId);
            Assert.IsNull(g.Seguimientos[0].Justificacion);
        }

        [TestMethod]
        public void Conclusion_PrimeraPreguntaDeTextoQueDiceConclusion()
        {
            var ps = new List<EntCurvaPregunta>
            {
                P(1, "texto_abierto", texto: "Comentarios"),
                P(2, "texto_abierto", texto: "Conclusión y recomendación general"),
                P(3, "texto_abierto", texto: "Conclusión")
            };
            var g = NegCurvaFormulario.Normalizar(ps, "{\"respuestas\":{\"1\":\"a\",\"2\":\"la buena\",\"3\":\"otra\"}}");
            Assert.AreEqual("la buena", g.Conclusion);
            g = NegCurvaFormulario.Normalizar(ps, "{\"respuestas\":{\"3\":\"otra\"}}");
            Assert.IsNull(g.Conclusion);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Normalizar_JsonRoto()
        {
            NegCurvaFormulario.Normalizar(new List<EntCurvaPregunta>(), "{no es json");
        }
    }
}
