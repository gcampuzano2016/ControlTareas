using System;

namespace CapaEntidad
{
    /// <summary>
    /// Una decision vigente de Nomina sobre un segmento, en un periodo, tal como la
    /// devuelve el segundo conjunto de Sp_RTA_HeTramosCandidatos (una fila de
    /// HE_TramoDecision).
    ///
    /// La tabla es solo INSERT: cambiar una decision es insertar otra, y manda la
    /// de IdDecision mas alto de cada (IdPeriodo, IdRegDetTareas, SegmentoInicio).
    /// El procedimiento ya filtra la vigente, pero NegHeDecisiones vuelve a
    /// elegirla por IdDecision para no depender de eso.
    /// </summary>
    public class EntHeDecision
    {
        public int IdDecision { get; set; }
        public int IdPeriodo { get; set; }
        public long IdRegDetTareas { get; set; }
        public DateTime SegmentoInicio { get; set; }

        /// <summary>true se paga en ese periodo, false no.</summary>
        public bool Incluir { get; set; }

        /// <summary>1 = 50%, 2 = 100%: el tipo que eligio Nomina, que puede no ser el del horario.</summary>
        public int TipoHora { get; set; }

        /// <summary>null = el segmento completo; si no, se pagan los primeros N minutos.</summary>
        public int? MinutosIncluidos { get; set; }

        public string Motivo { get; set; } = "";
        public string Usuario { get; set; } = "";
        public DateTime? Fecha { get; set; }
    }
}
