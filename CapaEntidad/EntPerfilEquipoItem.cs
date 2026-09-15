namespace CapaEntidad
{
    /// <summary>
    /// Una fila de la lista del equipo de una jefatura. Lo justo para
    /// reconocer a alguien y decidir si abrir su ficha.
    /// </summary>
    public class EntPerfilEquipoItem
    {
        public string CodUsuario { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Area { get; set; } = "";
        public string Ciudad { get; set; } = "";
    }
}
