namespace CapaEntidad
{
    /// <summary>
    /// Lo unico que el colaborador puede cambiar de su propio contacto.
    ///
    /// Que esta clase tenga exactamente cuatro propiedades no es casualidad: es
    /// la lista blanca. Un campo que no esta aca no se puede guardar, porque no
    /// hay donde ponerlo.
    /// </summary>
    public class EntPerfilContacto
    {
        /* En "" por defecto y no en null, por la misma razon que
           EntPerfilCabecera: el objeto por defecto debe verse igual que el
           camino normal cuando no hay fila que leer. */
        public string CorreoPersonal { get; set; } = "";
        public string TelefonoPersonal { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string EstadoCivil { get; set; } = "";
    }
}
