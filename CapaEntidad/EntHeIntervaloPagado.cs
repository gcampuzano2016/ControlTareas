using System;

namespace CapaEntidad
{
    /// <summary>
    /// Un tramo ya pagado (aprobado con tipo 50% o 100%). Se usa entero, no solo
    /// su parte fuera de horario: un minuto que ya se paga no puede aparecer
    /// ademas como pendiente.
    /// </summary>
    public class EntHeIntervaloPagado
    {
        public long IdEmpleado { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }

        /// <summary>
        /// 1 = 50%, 2 = 100%. Solo lo usa el resumen de personas fuera del modulo,
        /// que tiene que separar lo aprobado por tipo; para descontar da igual.
        /// </summary>
        public int TipoRegistrado { get; set; }
    }
}
