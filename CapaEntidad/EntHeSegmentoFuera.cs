using System;

namespace CapaEntidad
{
    /// <summary>
    /// Un pedazo de un tramo registrado que cae fuera del horario de la persona,
    /// tal como lo devuelve Sp_RTA_HeFueraDeHorario. Todavia no esta depurado:
    /// dos segmentos pueden cubrir el mismo minuto, o uno puede estar ya cubierto
    /// por un tramo pagado. Eso lo resuelve NegHeFueraDeHorario.
    /// </summary>
    public class EntHeSegmentoFuera
    {
        public long IdEmpleado { get; set; }
        public long IdRegDetTareas { get; set; }

        /// <summary>El tramo tal como lo registro la persona.</summary>
        public DateTime TramoInicio { get; set; }
        public DateTime TramoFin { get; set; }

        /// <summary>La parte de ese tramo que la funcion de horarios marca como 50% o 100%.</summary>
        public DateTime SegmentoInicio { get; set; }
        public DateTime SegmentoFin { get; set; }

        /// <summary>1 = 50%, 2 = 100%, segun el horario (no segun lo que se eligio al registrar).</summary>
        public int TipoHora { get; set; }

        /// <summary>Det_Horas_Extras_Estado: 0 sin solicitar, 1 solicitado, 2 aprobado, 3 rechazado.</summary>
        public int EstadoTramo { get; set; }

        /// <summary>Det_Horas_Extras_Tipo: 0 sin tipo, 1 = 50%, 2 = 100%. Decide si un aprobado esta pagado.</summary>
        public int TipoRegistrado { get; set; }

        public string Actividad { get; set; } = "";
    }
}
