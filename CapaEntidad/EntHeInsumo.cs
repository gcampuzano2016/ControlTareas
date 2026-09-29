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

        /// <summary>
        /// Minutos exactos por tramo (decision por tramo, tarea 3). Si
        /// tienen valor, mandan sobre Horas50/Horas100 para los totales en
        /// dinero -sin redondear minutos/60 antes de multiplicar-, y tambien
        /// para TotalHoras, que si se redondea a 2 decimales porque es lo que
        /// se muestra. Si son null, el calculo se comporta exactamente igual
        /// que antes de esta tarea.
        /// </summary>
        public int? Minutos50 { get; set; }
        public int? Minutos100 { get; set; }
    }
}
