using CapaEntidad;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Agregados del dashboard (especificacion, seccion 7.11) sobre los
    /// insumos que lee Sp_RTA_CurvaDashboard. Clase pura. Los porcentajes sin
    /// denominador quedan en null: la pantalla los muestra como "—", nunca "0%".
    /// </summary>
    public static class NegCurvaDashboard
    {
        public const string SinArea = "Sin área";
        public const string SinCategoria = "Sin categoría";

        public static EntCurvaDashboard Agregar(EntCurvaDashboardInsumos d)
        {
            var r = new EntCurvaDashboard { TotalColaboradores = d.TotalColaboradores };

            r.ProcesosPorEstado = d.Procesos.GroupBy(p => p.Estado).ToDictionary(g => g.Key, g => g.Count());
            r.ProcesosEnCurso = d.Procesos.Count(p => p.Estado == CurvaConstantes.ProcesoEnCurso);

            // Periodo de prueba: sobre TODAS las decisiones registradas.
            r.DecisionesTotal = d.Decisiones.Count;
            r.DecisionesPorTipo = CurvaConstantes.Decisiones.ToDictionary(x => x, x => d.Decisiones.Count(k => k.Key == x));
            r.AprobacionPct = r.DecisionesTotal == 0 ? (int?)null
                : NegCurvaCurva.RedondearJs(100.0 * r.DecisionesPorTipo[CurvaConstantes.ProcesoAprobado] / r.DecisionesTotal);
            r.DecisionesPorArea = d.Decisiones
                .GroupBy(k => Area(k.Value))
                .ToDictionary(g => g.Key, g => g.GroupBy(k => k.Key).ToDictionary(x => x.Key, x => x.Count()));

            // Compromisos: el estado de cada uno es el de su ultimo seguimiento, o sin_revisar.
            var estados = new[] { CurvaConstantes.SegRealizado, CurvaConstantes.SegEnProgreso, CurvaConstantes.SegPendiente,
                                  CurvaConstantes.SegNoRealizado, CurvaConstantes.SegSinRevisar };
            r.CompromisosTotal = d.Compromisos.Count;
            r.CompromisosPorEstado = estados.ToDictionary(e => e, e => d.Compromisos.Count(c => EstadoFinal(c) == e));
            r.CumplimientoPct = r.CompromisosTotal == 0 ? (int?)null
                : NegCurvaCurva.RedondearJs(100.0 * r.CompromisosPorEstado[CurvaConstantes.SegRealizado] / r.CompromisosTotal);
            r.CompromisosPorArea = d.Compromisos
                .GroupBy(c => Area(c.Area))
                .ToDictionary(g => g.Key, g => g.GroupBy(EstadoFinal).ToDictionary(x => x.Key, x => x.Count()));

            // Temas recurrentes: top 8 por categoria. OrderByDescending es
            // estable, asi que a igual conteo queda el orden de aparicion.
            r.TemasRecurrentes = d.Compromisos
                .GroupBy(c => string.IsNullOrWhiteSpace(c.Categoria) ? SinCategoria : c.Categoria)
                .Select(g => new EntCurvaConteo { Clave = g.Key, N = g.Count() })
                .OrderByDescending(x => x.N).Take(8).ToList();

            // Competencias: promedio de cada fila, luego promedio por etapa a 2 decimales.
            r.EvolucionCompetencias = d.Competencias
                .Select(k => new { Etapa = k.Key, Valores = NegCurvaValorRespuesta.ComoCompetencias(k.Value).Values.ToList() })
                .Where(x => x.Valores.Count > 0)
                .GroupBy(x => x.Etapa)
                .OrderBy(g => NegCurvaCurva.OrdenEtapa(g.Key))
                .Select(g => new EntCurvaCompetenciaEtapa
                {
                    Etapa = g.Key,
                    Promedio = NegCurvaCurva.RedondearJs(g.Average(x => x.Valores.Average()) * 100) / 100.0,
                    Evaluaciones = g.Count()
                }).ToList();

            r.Proximas = d.Proximas;

            // Casos con alerta: procesos en curso o extendidos cuya alerta general
            // sea atencion o seguimiento; seguimiento primero.
            var porProceso = d.EvaluacionesEnCurso
                .GroupBy(e => d.ProcesoDeEvaluacion[e.EvaluacionId])
                .ToDictionary(g => g.Key, g => g.ToList());
            var casos = new List<EntCurvaCaso>();
            foreach (var p in d.Procesos.Where(p => p.Estado == CurvaConstantes.ProcesoEnCurso || p.Estado == CurvaConstantes.ProcesoExtendido))
            {
                List<EntCurvaInsumoEvaluacion> evs;
                if (!porProceso.TryGetValue(p.Id, out evs)) continue;
                var alerta = NegCurvaCurva.CalcularAlerta(NegCurvaCurva.Calcular(evs));
                if (alerta.Nivel == CurvaConstantes.NivelAtencion || alerta.Nivel == CurvaConstantes.NivelSeguimiento)
                    casos.Add(new EntCurvaCaso { EmpleadoId = p.EmpleadoId, NombreCompleto = p.NombreCompleto, Area = p.Area, Nivel = alerta.Nivel, Motivo = alerta.Motivo });
            }
            r.Casos = casos.OrderBy(c => c.Nivel == CurvaConstantes.NivelSeguimiento ? 0 : 1).ToList();

            return r;
        }

        private static string EstadoFinal(EntCurvaDashCompromiso c)
        {
            return string.IsNullOrEmpty(c.UltimoEstado) ? CurvaConstantes.SegSinRevisar : c.UltimoEstado;
        }

        private static string Area(string area)
        {
            return string.IsNullOrWhiteSpace(area) ? SinArea : area;
        }
    }
}
