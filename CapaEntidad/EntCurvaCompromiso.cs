using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>Un compromiso acordado en una evaluacion, con su etapa de origen.</summary>
    public class EntCurvaCompromiso
    {
        public int Id { get; set; }
        public int EvaluacionId { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public string Responsable { get; set; }
        public DateTime? FechaLimite { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EtapaCodigo { get; set; }
        public string EtapaNombre { get; set; }
        public string CreadoPorRol { get; set; }
        public int NSeguimientos { get; set; }
        /// <summary>Estado del seguimiento de mayor id; null = sin revisar.</summary>
        public string UltimoEstado { get; set; }
        /// <summary>Solo en compromisos abiertos: lo que se marco en la evaluacion que se esta editando.</summary>
        public string EstadoActual { get; set; }
        public string JustificacionActual { get; set; }
    }

    /// <summary>Un seguimiento registrado (CA_compromiso_seguimientos).</summary>
    public class EntCurvaSeguimiento
    {
        public int Id { get; set; }
        public int CompromisoId { get; set; }
        public int EvaluacionId { get; set; }
        public string Estado { get; set; }
        public string Justificacion { get; set; }
        public DateTime FechaRevision { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
    }

    /// <summary>Una fila del editor de compromisos, ya normalizada.</summary>
    public class EntCurvaCompromisoFila
    {
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public string Responsable { get; set; }
        public DateTime? FechaLimite { get; set; }
    }

    public class EntCurvaSeguimientoInput
    {
        public int CompromisoId { get; set; }
        public string Estado { get; set; }
        public string Justificacion { get; set; }
    }

    public class EntCurvaDecisionInput
    {
        public string Decision { get; set; }
        public DateTime? NuevaFechaFin { get; set; }
        public string Comentarios { get; set; }
    }

    /// <summary>
    /// Lo que resulta de normalizar el payload del formulario contra las
    /// preguntas de la evaluacion: el valor a guardar por pregunta y los
    /// efectos (compromisos, decision, seguimientos).
    /// </summary>
    public class EntCurvaGuardado
    {
        /// <summary>Clave = id de pregunta; valor = texto a guardar en valor_respuesta.</summary>
        public Dictionary<int, string> Valores { get; set; }
        /// <summary>null = el formulario no traia pregunta de compromisos.</summary>
        public List<EntCurvaCompromisoFila> Compromisos { get; set; }
        public EntCurvaDecisionInput Decision { get; set; }
        public List<EntCurvaSeguimientoInput> Seguimientos { get; set; }
        /// <summary>Respuesta a la primera pregunta de texto abierto que diga "Conclusion..."; null si no hay.</summary>
        public string Conclusion { get; set; }

        public EntCurvaGuardado()
        {
            Valores = new Dictionary<int, string>();
            Seguimientos = new List<EntCurvaSeguimientoInput>();
        }
    }
}
