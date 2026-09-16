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

        /// <summary>
        /// Los factores de recargo vigentes (HE_Parametro), tal cual los uso el
        /// servidor para calcular esta misma pantalla. Viajan aqui para que el
        /// recalculo local de la grilla los tome de la respuesta en vez de
        /// llevar su propia copia: sin esto, el dia que HE_Parametro cambiara
        /// un factor, el cliente seguiria mostrando el numero viejo en las 64
        /// filas a la vez, sin que nada lo avisara.
        /// </summary>
        public decimal Factor50 { get; set; }
        public decimal Factor100 { get; set; }

        public EntHePantalla()
        {
            Filas = new List<EntHeFila>();
        }
    }
}
