using System;

namespace CapaEntidad
{
    /// <summary>El proceso de onboarding de un colaborador (CA_procesos_onboarding).</summary>
    public class EntCurvaProceso
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public string Estado { get; set; }
        public string ResumenEjecutivoAnual { get; set; }
    }

    /// <summary>Una evaluacion (etapa + rol) con los datos de su etapa.</summary>
    public class EntCurvaEvaluacion
    {
        public int Id { get; set; }
        public int ProcesoId { get; set; }
        public int EtapaId { get; set; }
        public int EmpleadoId { get; set; }
        public string EvaluadorRol { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaObjetivo { get; set; }
        /// <summary>Se fija al completar y NO cambia al editar.</summary>
        public DateTime? FechaCompletada { get; set; }
        public string Conclusion { get; set; }
        public string ResumenEjecutivo { get; set; }
        public string PlanAccionGth { get; set; }
        public string Token { get; set; }
        public string EtapaCodigo { get; set; }
        public string EtapaNombre { get; set; }
        public int EtapaOrden { get; set; }

        public bool EstaCompletada
        {
            get { return Estado == CurvaConstantes.EvalCompletada; }
        }
    }

    /// <summary>Una respuesta guardada, con los datos de su pregunta.</summary>
    public class EntCurvaRespuesta
    {
        public int Id { get; set; }
        public int EvaluacionId { get; set; }
        public int PreguntaId { get; set; }
        /// <summary>Texto crudo si era un string; JSON en cualquier otro caso (seccion 10.3).</summary>
        public string ValorRespuesta { get; set; }
        public string TextoPregunta { get; set; }
        public string TipoRespuesta { get; set; }
        public string Seccion { get; set; }
        public int Orden { get; set; }
        public string OpcionesJson { get; set; }
    }

    /// <summary>Decision formal del periodo de prueba (etapa 90, jefe).</summary>
    public class EntCurvaDecision
    {
        public int Id { get; set; }
        public int ProcesoId { get; set; }
        public int EvaluacionId { get; set; }
        public string Decision { get; set; }
        public DateTime? NuevaFechaFin { get; set; }
        public string Comentarios { get; set; }
        public DateTime FechaDecision { get; set; }
    }
}
