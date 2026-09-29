using System;
using System.Globalization;

namespace CapaEntidad
{
    /// <summary>
    /// Un periodo de horas extras tal como lo devuelve el tercer conjunto de
    /// Sp_RTA_HeTramosCandidatos. Solo lo necesario para saber de que periodo es
    /// dueno un tramo y si ese periodo cuenta para el pago doble.
    /// </summary>
    public class EntHeRangoPeriodo
    {
        public int IdPeriodo { get; set; }

        /// <summary>HE_Periodo.FechaInicio y FechaFin son DATE: ambos dias entran en el periodo.</summary>
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        /// <summary>'Abierto', 'Cerrado' o 'Anulado'.</summary>
        public string EstadoPeriodo { get; set; } = "";

        /// <summary>
        /// Un periodo anulado no paga: no es dueno de nada y sus decisiones no
        /// bloquean a nadie. Es la misma regla que aplica Sp_RTA_HeGuardarDecisiones.
        /// </summary>
        public bool EsAnulado
        {
            get { return string.Equals((EstadoPeriodo ?? "").Trim(), "Anulado", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Un periodo cerrado ya pago: su Incluir = 1 retiene el tramo aunque la
        /// clave del segmento ya no sea la de hoy (ver NegHeDecisiones.Titular).
        /// </summary>
        public bool EsCerrado
        {
            get { return string.Equals((EstadoPeriodo ?? "").Trim(), "Cerrado", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>Si el dia de la fecha cae dentro de [FechaInicio, FechaFin], sin mirar la hora.</summary>
        public bool Contiene(DateTime fecha)
        {
            return fecha.Date >= FechaInicio.Date && fecha.Date <= FechaFin.Date;
        }

        /// <summary>El rango como lo lee Nomina: dd/MM/yyyy - dd/MM/yyyy.</summary>
        public string RangoTexto
        {
            get { return FechaInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + " - " + FechaFin.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture); }
        }
    }
}
