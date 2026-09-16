namespace CapaEntidad
{
    /// <summary>Lo que entra al calculo de una fila. Ya resuelto: aqui no se consulta nada.</summary>
    public class EntHeInsumo
    {
        public decimal SalarioBaseVigente { get; set; }
        public int JornadaHorasDia { get; set; }

        /// <summary>
        /// Si tiene valor, manda sobre la formula. Nace nulo para todos: el
        /// divisor de jornada parcial esta pendiente de validar con Legal y
        /// afecta a 8 personas reales.
        /// </summary>
        public int? DivisorManual { get; set; }

        public bool AplicaHE { get; set; }
        public decimal Horas50 { get; set; }
        public decimal Horas100 { get; set; }
    }
}
