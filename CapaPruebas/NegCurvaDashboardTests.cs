using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CapaPruebas
{
    /// <summary>Agregados del dashboard (seccion 7.11). Datos inventados.</summary>
    [TestClass]
    public class NegCurvaDashboardTests
    {
        [TestMethod]
        public void SinDatos_PorcentajesNull()
        {
            var r = NegCurvaDashboard.Agregar(new EntCurvaDashboardInsumos());
            Assert.IsNull(r.AprobacionPct);
            Assert.IsNull(r.CumplimientoPct);
            Assert.AreEqual(0, r.Casos.Count);
        }

        [TestMethod]
        public void Aprobacion_SobreTodasLasDecisiones()
        {
            var d = new EntCurvaDashboardInsumos();
            d.Decisiones.Add(new KeyValuePair<string, string>("aprobado", "TI"));
            d.Decisiones.Add(new KeyValuePair<string, string>("aprobado", null));
            d.Decisiones.Add(new KeyValuePair<string, string>("rechazado", "TI"));
            var r = NegCurvaDashboard.Agregar(d);
            Assert.AreEqual(67, r.AprobacionPct);
            Assert.AreEqual(1, r.DecisionesPorArea["Sin área"]["aprobado"]);
        }

        [TestMethod]
        public void Compromisos_SinSeguimientoEsSinRevisar()
        {
            var d = new EntCurvaDashboardInsumos();
            d.Compromisos.Add(new EntCurvaDashCompromiso { Id = 1, Categoria = "A", UltimoEstado = "realizado" });
            d.Compromisos.Add(new EntCurvaDashCompromiso { Id = 2, Categoria = "A" });
            d.Compromisos.Add(new EntCurvaDashCompromiso { Id = 3, Categoria = null, UltimoEstado = "pendiente" });
            d.Compromisos.Add(new EntCurvaDashCompromiso { Id = 4, Categoria = "B", UltimoEstado = "realizado" });
            var r = NegCurvaDashboard.Agregar(d);
            Assert.AreEqual(1, r.CompromisosPorEstado["sin_revisar"]);
            Assert.AreEqual(50, r.CumplimientoPct);
            Assert.AreEqual("A", r.TemasRecurrentes[0].Clave);
            Assert.AreEqual(2, r.TemasRecurrentes[0].N);
            Assert.IsTrue(r.TemasRecurrentes.Any(t => t.Clave == "Sin categoría"));
        }

        [TestMethod]
        public void Competencias_PromedioDeFilasPorEtapa()
        {
            var d = new EntCurvaDashboardInsumos();
            d.Competencias.Add(new KeyValuePair<string, string>("365", "{\"A\":5,\"B\":4}"));
            d.Competencias.Add(new KeyValuePair<string, string>("90", "{\"A\":3,\"B\":4}"));
            d.Competencias.Add(new KeyValuePair<string, string>("90", "{\"A\":2}"));
            var r = NegCurvaDashboard.Agregar(d);
            Assert.AreEqual("90", r.EvolucionCompetencias[0].Etapa);
            Assert.AreEqual(2.75, r.EvolucionCompetencias[0].Promedio);   // (3.5 + 2) / 2
            Assert.AreEqual(2, r.EvolucionCompetencias[0].Evaluaciones);
            Assert.AreEqual(4.5, r.EvolucionCompetencias[1].Promedio);
        }

        [TestMethod]
        public void Casos_SoloEnCursoOExtendidoYSeguimientoPrimero()
        {
            var d = new EntCurvaDashboardInsumos();
            d.Procesos.Add(new EntCurvaDashProceso { Id = 1, EmpleadoId = 10, Estado = "en_curso", NombreCompleto = "A" });
            d.Procesos.Add(new EntCurvaDashProceso { Id = 2, EmpleadoId = 20, Estado = "extendido", NombreCompleto = "B" });
            d.Procesos.Add(new EntCurvaDashProceso { Id = 3, EmpleadoId = 30, Estado = "aprobado", NombreCompleto = "C" });
            Agregar(d, 100, 1, "[\"x\",\"y\",\"z\",\"w\",\"v\"]", "y");     // 75 -> atencion
            Agregar(d, 200, 2, "[\"x\",\"y\"]", "y");                      // 0 -> seguimiento
            d.EvaluacionesEnCurso.Add(new EntCurvaInsumoEvaluacion { EvaluacionId = 300, Rol = "colaborador", EtapaCodigo = "30" });
            d.ProcesoDeEvaluacion[300] = 3;
            var r = NegCurvaDashboard.Agregar(d);
            Assert.AreEqual(2, r.Casos.Count);
            Assert.AreEqual("B", r.Casos[0].NombreCompleto);
            Assert.AreEqual("seguimiento", r.Casos[0].Nivel);
            Assert.AreEqual("atencion", r.Casos[1].Nivel);
        }

        private static void Agregar(EntCurvaDashboardInsumos d, int idEval, int idProceso, string opciones, string valor)
        {
            var ev = new EntCurvaInsumoEvaluacion { EvaluacionId = idEval, Rol = "colaborador", EtapaCodigo = "30" };
            ev.Respuestas.Add(new EntCurvaRespuesta { TipoRespuesta = "escala", OpcionesJson = opciones, ValorRespuesta = valor });
            d.EvaluacionesEnCurso.Add(ev);
            d.ProcesoDeEvaluacion[idEval] = idProceso;
        }
    }
}
