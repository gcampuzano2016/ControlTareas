namespace CapaEntidad
{
    /// <summary>
    /// Un renglon de la bitacora de correos de una solicitud: a quien se le
    /// intento avisar, de que, y como salio.
    /// </summary>
    public class EntLogCorreoSolicitud
    {
        public long IdVacaciones { get; set; }
        public string TipoAviso { get; set; }
        public string Destinatario { get; set; }
        public string Asunto { get; set; }
        public string Resultado { get; set; }
        public string Mensaje { get; set; }
    }
}
