namespace CapaEntidad
{
    /// <summary>Una certificacion o curso con entidad emisora.</summary>
    public class EntPerfilCertificacion
    {
        public int IdCertificacion { get; set; }
        public string Nombre { get; set; } = "";
        public string Entidad { get; set; } = "";

        /// <summary>
        /// Texto "yyyy-MM", que es lo que produce un input type="month". Se deja
        /// como texto hasta la capa de datos por la misma razon que la fecha de
        /// nacimiento: la conversion vive en un solo sitio y con formato explicito.
        /// </summary>
        public string FechaObtencion { get; set; } = "";
    }
}
