namespace CapaEntidad
{
    /// <summary>
    /// Una fila de la grilla: el snapshot congelado del colaborador, las dos
    /// columnas que se digitan, y lo que el calculo derivo de ambos.
    ///
    /// El snapshot es deliberado: un periodo cerrado no debe cambiar porque
    /// alguien edito el maestro despues. Por eso la fila guarda su propia copia
    /// del nombre, el cargo, la jornada y el sueldo, y no los vuelve a consultar.
    /// </summary>
    public class EntHeFila
    {
        public int IdDetalle { get; set; }
        public long IdEmpleado { get; set; }

        public string CedulaSnapshot { get; set; }
        public string NombreSnapshot { get; set; }
        public string EmpresaSnapshot { get; set; }
        public string CargoSnapshot { get; set; }
        public int JornadaHorasDiaSnapshot { get; set; }
        public decimal SalarioBaseSnapshot { get; set; }
        public bool AplicaHESnapshot { get; set; }

        /// <summary>
        /// El divisor manual del maestro, si lo hay. No se persiste en
        /// HE_Detalle: ya quedo aplicado dentro de Divisor. Viaja solo desde
        /// los insumos hasta el calculo.
        /// </summary>
        public int? DivisorManual { get; set; }

        public int Divisor { get; set; }
        public decimal ValorHoraOrdinaria { get; set; }
        public decimal ValorHora50 { get; set; }
        public decimal ValorHora100 { get; set; }

        public decimal Horas50 { get; set; }
        public decimal Horas100 { get; set; }

        public decimal Total50 { get; set; }
        public decimal Total100 { get; set; }
        public decimal TotalHoras { get; set; }
        public decimal TotalHE { get; set; }

        public string Observacion { get; set; }

        /// <summary>
        /// Por que esta persona no aplica, o "EnRevisionSalarial" para alguien
        /// que SI aplica pero tiene el sueldo en revision. La columna de la base
        /// se llama MotivoNoAplica y el nombre enganna: hay dos personas que
        /// aplican horas extras y llevan texto aqui. La pantalla los distingue
        /// con AplicaHESnapshot.
        /// </summary>
        public string MotivoNoAplica { get; set; }

        /// <summary>El calculo marco esta fila como dato que alguien debe revisar.</summary>
        public bool TieneAdvertencia { get; set; }
    }
}
