using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Todo lo que la pantalla necesita de una sola ida: el periodo, sus filas
    /// y los seis totales del tablero.
    /// </summary>
    public class EntHePantalla
    {
        public EntHePeriodo Periodo { get; set; }
        public List<EntHeFila> Filas { get; set; }

        public decimal TotalHoras50 { get; set; }
        public decimal TotalHoras100 { get; set; }
        public decimal TotalPago50 { get; set; }
        public decimal TotalPago100 { get; set; }
        public decimal TotalHoras { get; set; }
        public decimal TotalPagar { get; set; }

        public EntHePantalla()
        {
            Filas = new List<EntHeFila>();
        }
    }
}
