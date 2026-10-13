using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Un perfil de horario del catalogo (R_HorarioLaboral).
    /// Si EsPropio = 1 el horario pertenece a una sola persona y no se ofrece
    /// en el combo de asignacion: se edita desde la pantalla de asignacion.
    /// </summary>
    public class EntHorarioLaboral
    {
        public Int32 IdHorarioLaboral { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public Int32 EsPredeterminado { get; set; }
        public Int32 Activo { get; set; }
        public Int32 EsPropio { get; set; }
        public string CodigoDueno { get; set; }
        public string NombreDueno { get; set; }
        /// <summary>Los dias laborables agrupados por franja: "LUN,MAR,MIE,JUE,VIE 08:30-17:30".</summary>
        public string Resumen { get; set; }
        public Int32 UsuariosAsignados { get; set; }
    }

    /// <summary>
    /// Un dia de la semana dentro de un perfil (R_HorarioLaboralDetalle).
    /// Las horas viajan como texto "HH:mm" porque es lo que come un input type=time.
    /// </summary>
    public class EntHorarioLaboralDetalle
    {
        public Int32 DiaSemana { get; set; }
        public string NombreDia { get; set; }
        public Int32 EsLaborable { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
    }
}
