namespace CapaEntidad
{
    /// <summary>
    /// Un archivo de respaldo. Cuelga de una certificacion o de una carga
    /// familiar, y de cual lo dicen Origen e IdOrigen: una sola tabla para los
    /// dos casos, con el mismo codigo para subir, listar y quitar.
    /// </summary>
    public class EntPerfilDocumento
    {
        public int IdDocumento { get; set; }

        /// <summary>"CERTIFICACION" o "CARGAFAMILIAR". En mayusculas, siempre.</summary>
        public string Origen { get; set; } = "";

        /// <summary>IdCertificacion o IdCargaFam, segun Origen.</summary>
        public int IdOrigen { get; set; }

        /// <summary>El nombre con el que la persona subio el archivo. Se muestra.</summary>
        public string NombreArchivo { get; set; } = "";

        /// <summary>
        /// El nombre con el que quedo en el disco, que lo arma el servidor.
        ///
        /// Al leer el perfil se deja VACIA a proposito: esa lista viaja al
        /// navegador y el nombre en disco no tiene nada que hacer alli. La
        /// descarga la pide aparte, con DaoPerfil.ObtenerDocumento.
        /// </summary>
        public string NombreArchivoCodigo { get; set; } = "";

        /// <summary>
        /// Ruta de la aplicacion, "~/descargas/perfil/", no la fisica: asi el
        /// dato sigue sirviendo si el sitio cambia de carpeta o de servidor.
        /// Se deja vacia al leer el perfil, por lo mismo que NombreArchivoCodigo.
        /// </summary>
        public string Ruta { get; set; } = "";
    }
}
