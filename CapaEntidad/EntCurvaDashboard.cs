using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>Lo que lee Sp_RTA_CurvaDashboard, sin agregar.</summary>
    public class EntCurvaDashboardInsumos
    {
        public int TotalColaboradores { get; set; }
        public List<EntCurvaDashProceso> Procesos { get; set; }
        /// <summary>(decision, area)</summary>
        public List<KeyValuePair<string, string>> Decisiones { get; set; }
        public List<EntCurvaDashCompromiso> Compromisos { get; set; }
        /// <summary>(etapa, valor_respuesta crudo)</summary>
        public List<KeyValuePair<string, string>> Competencias { get; set; }
        public List<EntCurvaProxima> Proximas { get; set; }
        /// <summary>Evaluaciones completadas de procesos en curso o extendidos, con sus respuestas y seguimientos.</summary>
        public List<EntCurvaInsumoEvaluacion> EvaluacionesEnCurso { get; set; }
        /// <summary>evaluacion_id -&gt; proceso_id de EvaluacionesEnCurso.</summary>
        public Dictionary<int, int> ProcesoDeEvaluacion { get; set; }

        public EntCurvaDashboardInsumos()
        {
            Procesos = new List<EntCurvaDashProceso>();
            Decisiones = new List<KeyValuePair<string, string>>();
            Compromisos = new List<EntCurvaDashCompromiso>();
            Competencias = new List<KeyValuePair<string, string>>();
            Proximas = new List<EntCurvaProxima>();
            EvaluacionesEnCurso = new List<EntCurvaInsumoEvaluacion>();
            ProcesoDeEvaluacion = new Dictionary<int, int>();
        }
    }

    public class EntCurvaDashProceso
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public string Estado { get; set; }
        public string NombreCompleto { get; set; }
        public string Area { get; set; }
    }

    public class EntCurvaDashCompromiso
    {
        public int Id { get; set; }
        public string Categoria { get; set; }
        public string Area { get; set; }
        public string UltimoEstado { get; set; }
    }

    public class EntCurvaProxima
    {
        public int EvaluacionId { get; set; }
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; }
        public string Area { get; set; }
        public string EtapaCodigo { get; set; }
        public string EtapaNombre { get; set; }
        public string Rol { get; set; }
        public DateTime? FechaObjetivo { get; set; }
        public bool Vencida { get; set; }
    }

    public class EntCurvaConteo
    {
        public string Clave { get; set; }
        public int N { get; set; }
    }

    public class EntCurvaCompetenciaEtapa
    {
        public string Etapa { get; set; }
        public double Promedio { get; set; }
        public int Evaluaciones { get; set; }
    }

    public class EntCurvaCaso
    {
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; }
        public string Area { get; set; }
        public string Nivel { get; set; }
        public string Motivo { get; set; }
    }

    /// <summary>El dashboard ya agregado (seccion 7.11). Los porcentajes null se muestran como "—".</summary>
    public class EntCurvaDashboard
    {
        public int TotalColaboradores { get; set; }
        public int ProcesosEnCurso { get; set; }
        public Dictionary<string, int> ProcesosPorEstado { get; set; }

        public int DecisionesTotal { get; set; }
        public Dictionary<string, int> DecisionesPorTipo { get; set; }
        public int? AprobacionPct { get; set; }
        public Dictionary<string, Dictionary<string, int>> DecisionesPorArea { get; set; }

        public int CompromisosTotal { get; set; }
        public Dictionary<string, int> CompromisosPorEstado { get; set; }
        public int? CumplimientoPct { get; set; }
        public Dictionary<string, Dictionary<string, int>> CompromisosPorArea { get; set; }

        public List<EntCurvaConteo> TemasRecurrentes { get; set; }
        public List<EntCurvaCompetenciaEtapa> EvolucionCompetencias { get; set; }
        public List<EntCurvaProxima> Proximas { get; set; }
        public List<EntCurvaCaso> Casos { get; set; }
    }
}
