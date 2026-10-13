using System;

namespace CapaEntidad
{
    public class EntActividad
    {
        public Int64 IdProceso { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinal { get; set; }
        public DateTime FechaLimite { get; set; }
        public int Estado { get; set; }
        public string Observacion { get; set; }
        public int Tipo { get; set; }
    }
}
