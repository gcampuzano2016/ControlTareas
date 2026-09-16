using System;

namespace CapaEntidad
{
    /// <summary>
    /// Una fila del historial de sueldos. En el Excel esto era una columna
    /// llamada con el nombre de un mes; aqui cada monto tiene su fecha de
    /// vigencia y el calculo toma el vigente al corte del periodo.
    /// </summary>
    public class EntHeSalario
    {
        public decimal Monto { get; set; }
        public DateTime FechaVigenciaDesde { get; set; }

        /// <summary>"Rol" o "Ajuste". Informativo: el que manda es el mas reciente, venga de donde venga.</summary>
        public string Origen { get; set; } = "";
    }
}
