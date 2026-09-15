namespace CapaEntidad
{
    /// <summary>
    /// La cabecera de un subordinado, tal como la ve su jefatura.
    ///
    /// Es deliberadamente mas pobre que EntPerfilCabecera y NO debe converger
    /// con ella. Le faltan, porque la matriz de permisos del diseno se los
    /// niega a la jefatura: Cedula, FechaNacTexto y Edad. No es que no se
    /// llenen: es que no existen, y por eso no se pueden enviar por descuido.
    ///
    /// EntPerfilEquipoTests comprueba esto en cada compilacion. Si vas a
    /// agregar una propiedad aqui, esa prueba te va a parar: es lo que tiene
    /// que pasar. Cambia el diseno primero.
    /// </summary>
    public class EntPerfilEquipoCabecera
    {
        public string CodUsuario { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Area { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string CorreoNotificacion { get; set; } = "";
        public string JefeInmediato { get; set; } = "";

        /// <summary>Nombre del horario vigente, o vacio si no tiene ninguno.</summary>
        public string Horario { get; set; } = "";
    }
}
