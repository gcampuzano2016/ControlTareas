namespace CapaEntidad
{
    /// <summary>A quien llamar. Una fila por contacto; puede haber varios.</summary>
    public class EntPerfilEmergencia
    {
        public int IdContacto { get; set; }
        public string Nombre { get; set; }
        public string Parentesco { get; set; }
        public string Telefono { get; set; }
    }
}
