using System;

namespace CapaEntidad
{
    /// <summary>
    /// Lo que Nomina pide guardar sobre un segmento desde la pantalla. Todavia no
    /// es una decision: NegHeDecisiones.Validar lo revisa contra los candidatos
    /// resueltos antes de mandarlo a Sp_RTA_HeGuardarDecisiones.
    /// </summary>
    public class EntHeCambioDecision
    {
        public long IdRegDetTareas { get; set; }
        public DateTime SegmentoInicio { get; set; }
        public bool Incluir { get; set; }

        /// <summary>1 = 50%, 2 = 100%.</summary>
        public int TipoHora { get; set; }

        /// <summary>null = el segmento completo. Solo sirve para reducir, nunca para subir.</summary>
        public int? MinutosIncluidos { get; set; }

        public string Motivo { get; set; } = "";
    }
}
