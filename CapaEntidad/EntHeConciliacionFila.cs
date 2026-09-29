using System;

namespace CapaEntidad
{
    /// <summary>
    /// Una fila de la conciliacion: por persona del periodo, que parte de su tiempo
    /// candidato se paga y que parte no, en minutos y en USD. La arma
    /// NegHeDecisiones.Conciliar.
    ///
    /// Existe para que Nomina pueda cuadrar el sistema contra su analisis manual:
    /// el total a pagar solo dice cuanto, y esto dice de donde sale (lo aprobado,
    /// lo que se incluyo sin estar aprobado) y cuanto quedo afuera.
    ///
    /// Los USD de cada columna usan el valor hora de la fila de la persona y se
    /// redondean UNA vez sobre la suma, igual que el pago. TotalAPagar no se
    /// recalcula aqui: es el TotalHE de la fila, lo que de verdad se paga.
    /// </summary>
    public class EntHeConciliacionFila
    {
        public long IdEmpleado { get; set; }
        public string Nombre { get; set; } = "";

        /// <summary>Tramos aprobados por la jefatura que se pagan en este periodo.</summary>
        public int MinutosAprobadoIncluido { get; set; }

        /// <summary>Tiempo sin solicitar, pendiente o rechazado que Nomina decidio pagar.</summary>
        public int MinutosNoAprobadoIncluido { get; set; }

        /// <summary>
        /// Tiempo candidato del periodo que no se paga: no incluido, no bloqueado por
        /// otro periodo y que ningun otro segmento incluido ya cubre.
        /// </summary>
        public int MinutosExcluido { get; set; }

        public decimal UsdAprobadoIncluido { get; set; }
        public decimal UsdNoAprobadoIncluido { get; set; }
        public decimal UsdExcluido { get; set; }

        /// <summary>El TotalHE de la fila.</summary>
        public decimal TotalAPagar { get; set; }

        public decimal HorasAprobadoIncluido { get { return Horas(MinutosAprobadoIncluido); } }
        public decimal HorasNoAprobadoIncluido { get { return Horas(MinutosNoAprobadoIncluido); } }
        public decimal HorasExcluido { get { return Horas(MinutosExcluido); } }

        /// <summary>
        /// Lo que costaria pagar tambien lo excluido: la columna «Total incluyendo no
        /// incluido». Suma dos montos ya redondeados, como el resto del tablero.
        /// </summary>
        public decimal TotalConExcluido { get { return TotalAPagar + UsdExcluido; } }

        private static decimal Horas(int minutos)
        {
            return Math.Round(minutos / 60m, 2, MidpointRounding.AwayFromZero);
        }
    }
}
