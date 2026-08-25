namespace CapaEntidad
{
    /// <summary>
    /// Los campos que CB-GAP-POL-01 agrega al permiso: el tipo, el tratamiento
    /// del excedente y, cuando corresponde, el detalle de teletrabajo.
    ///
    /// Vive aparte de EntSolicitud porque no se guarda con el alta: el alta pasa
    /// por Sp_RTAInsertaNuevaSolicitud, que hace también la actualización, la
    /// aprobación y el rechazo, y no conviene ampliarla.
    /// </summary>
    public class EntDetallePermiso
    {
        public long IdVacaciones { get; set; }

        /// <summary>PERSONAL, MEDICO, FAMILIAR, CALAMIDAD, TELETRABAJO u OTRO.</summary>
        public string TipoPermiso { get; set; }

        /// <summary>VACACIONES, RECUPERACION o SIN_REMUNERACION.</summary>
        public string TratamientoExcedente { get; set; }

        /// <summary>
        /// La actividad en texto libre. Para lo registrado antes de agosto de 2026
        /// es el único dato que hay: esas solicitudes no tienen TipoPermiso y no
        /// se migraron. La pantalla muestra el que exista.
        /// </summary>
        public string Actividad { get; set; }

        public bool EsTeletrabajo { get; set; }

        /// <summary>COMPLETA o HORAS.</summary>
        public string Modalidad { get; set; }
        public string HoraDesde { get; set; }
        public string HoraHasta { get; set; }
        public string Lugar { get; set; }
        public string MediosContacto { get; set; }
        public string MotivoGeneral { get; set; }
        public string Actividades { get; set; }
        public string Entregables { get; set; }
        public bool ConfirmaConectividad { get; set; }
    }
}
