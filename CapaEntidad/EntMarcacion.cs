using System;

namespace CapaEntidad
{
    public class EntMarcacion
    {
        public int Accion { get; set; }
        public int Respuestas { get; set; }
        public string Mensaje { get; set; }
        public DateTime FechaHora { get; set; }
        public long IdProceso { get; set; }
    }
}
