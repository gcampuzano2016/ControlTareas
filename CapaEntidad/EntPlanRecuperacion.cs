using System;

namespace CapaEntidad
{
    /// <summary>
    /// El plan con el que un colaborador propone recuperar las horas de un
    /// permiso, y su cierre posterior por parte del jefe.
    /// </summary>
    public class EntPlanRecuperacion
    {
        public long IdVacaciones { get; set; }
        public string Colaborador { get; set; }
        public string Cedula { get; set; }
        public string JefeInmediato { get; set; }

        public DateTime FechaPermiso { get; set; }
        public string Horas { get; set; }

        public DateTime FechaPropuesta { get; set; }
        public string HorarioPropuesto { get; set; }
        public string Actividades { get; set; }
        public string Entregables { get; set; }

        /// <summary>Calculada por el procedimiento a partir de la fecha del permiso.</summary>
        public DateTime FechaMaximaCierre { get; set; }

        /// <summary>Días transcurridos desde el vencimiento. Negativo si aún no vence.</summary>
        public int DiasVencido { get; set; }
    }
}
