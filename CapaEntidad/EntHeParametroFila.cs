using System;

namespace CapaEntidad
{
    /// <summary>
    /// Una fila del historial de HE_Parametro: una clave con su valor y el
    /// rango de fechas en que ese valor rigio.
    ///
    /// El DAO devuelve el historial completo y NO resuelve cual rige; la
    /// eleccion la hace NegHeParametros.ValorAlCorte contra la fecha de fin
    /// del periodo que se esta calculando, no contra la fecha de hoy. Es la
    /// misma forma que ya tiene EntHeSalario, y por la misma razon: reabrir
    /// un periodo de agosto en noviembre no puede recalcularlo con los
    /// factores de noviembre.
    /// </summary>
    public class EntHeParametroFila
    {
        /// <summary>
        /// La llave de la fila. Queda en 0 cuando la lee
        /// DaoHorasExtras.LeerHistorialParametros, y eso es correcto: el
        /// calculo elige por Clave y FechaVigenciaDesde, y no tiene por que
        /// referirse a una fila concreta. La llena el DAO de la pantalla de
        /// administracion, que es quien la usa para identificar que version
        /// se esta editando.
        /// </summary>
        public int IdParametro { get; set; }

        /// <summary>
        /// Se inicializa en "" a proposito: una columna sin mapear en el
        /// SELECT dejaria la clave en null y ValorAlCorte descartaria la fila
        /// en silencio, cayendo al valor por omision sin ningun aviso.
        /// </summary>
        public string Clave { get; set; } = "";

        public decimal Valor { get; set; }

        /// <summary>
        /// Desde cuando rige, inclusive.
        /// </summary>
        public DateTime FechaVigenciaDesde { get; set; }

        /// <summary>
        /// Hasta cuando rigio. NULL es "sigue abierta".
        /// </summary>
        public DateTime? FechaVigenciaHasta { get; set; }

        public string Usu_Modificacion { get; set; } = "";
        public DateTime? Fec_Modificacion { get; set; }
    }
}
