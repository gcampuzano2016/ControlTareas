using System;

namespace CapaEntidad
{
    /// <summary>La cabecera de un periodo mensual de horas extras.</summary>
    public class EntHePeriodo
    {
        public int IdPeriodo { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Descripcion { get; set; }

        /// <summary>"Abierto", "Cerrado" o "Anulado".</summary>
        public string EstadoPeriodo { get; set; }

        public DateTime? FechaCierre { get; set; }
        public string UsuarioCierre { get; set; }

        /// <summary>
        /// Solo un periodo Abierto admite edicion. Se compara sin distinguir
        /// mayusculas porque el valor viaja como texto y un dia alguien lo
        /// escribira a mano en la base.
        /// </summary>
        public bool EstaAbierto
        {
            get { return string.Equals(EstadoPeriodo, "Abierto", StringComparison.OrdinalIgnoreCase); }
        }
    }
}
