namespace CapaEntidad
{
    /// <summary>
    /// Lo que sale del calculo de una fila.
    ///
    /// Los tres valores hora llevan seis decimales a proposito: son
    /// intermedios y no se redondean durante la cadena. Solo los totales se
    /// redondean, y a dos decimales.
    /// </summary>
    public class EntHeResultado
    {
        public int Divisor { get; set; }
        public decimal ValorHoraOrdinaria { get; set; }
        public decimal ValorHora50 { get; set; }
        public decimal ValorHora100 { get; set; }
        public decimal Total50 { get; set; }
        public decimal Total100 { get; set; }
        public decimal TotalHoras { get; set; }
        public decimal TotalHE { get; set; }

        /// <summary>
        /// El salario o el divisor son cero y la hora ordinaria salio en cero.
        /// La fila se muestra en rojo y bloquea el cierre del periodo: no es un
        /// error de programa, es un dato que alguien tiene que arreglar.
        /// </summary>
        public bool TieneAdvertencia { get; set; }
    }
}
