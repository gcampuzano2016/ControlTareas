using System;

namespace CapaEntidad
{
    /// <summary>
    /// El permiso mensual de 3 horas de un colaborador en un mes.
    ///
    /// Se trabaja en minutos y no en horas decimales porque los permisos se
    /// registran como HH:MM: 1h30 son 90 minutos, y como 1.5 se presta a que
    /// alguien lo lea como 1 hora 50.
    /// </summary>
    public class EntSaldoPermisoMensual
    {
        public int MinutosAsignados { get; set; }
        public int MinutosUsados { get; set; }
        public int MinutosDisponibles { get; set; }

        /// <summary>Último día del mes al que pertenece esta bolsa.</summary>
        public DateTime VigenteHasta { get; set; }

        /// <summary>
        /// El texto ya redactado, tal como debe verse en pantalla y en el PDF.
        /// Viene del procedimiento para no repetir la redacción en dos lenguajes.
        /// </summary>
        public string Mensaje { get; set; }

        public bool Agotado
        {
            get { return MinutosDisponibles <= 0; }
        }
    }
}
