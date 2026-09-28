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
    }
}
