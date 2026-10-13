namespace CapaEntidad
{
    /// <summary>
    /// Los ocho campos de la pestana "Datos personales" que Talento Humano
    /// puede editar. El horario no esta: sale de R_UsuarioHorarioLaboral, tiene
    /// su propio modulo y hay 5 personas con mas de una asignacion activa, algo
    /// que un solo campo no puede representar.
    ///
    /// Todas arrancan en "" y no en null, igual que EntPerfilCabecera: asi un
    /// payload al que le falte una clave no deja propiedades nulas que revienten
    /// en el primer .Trim().
    ///
    /// CodJefeInmediato guarda un Cod_Usuario, no un nombre. La pantalla muestra
    /// el nombre y envia el codigo; ver la seccion 3 del diseno.
    /// </summary>
    public class EntPerfilDatosPersonales
    {
        public string Nombre { get; set; } = "";
        public string Cedula { get; set; } = "";

        /// <summary>Texto dd/MM/yyyy, igual que Empleados.Fecha_nacimiento.</summary>
        public string FechaNacimiento { get; set; } = "";

        public string Cargo { get; set; } = "";
        public string Area { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string CodJefeInmediato { get; set; } = "";
        public string CorreoNotificacion { get; set; } = "";
    }
}
