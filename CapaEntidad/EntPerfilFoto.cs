namespace CapaEntidad
{
    /// <summary>
    /// La foto de perfil. Base64 y tipo son el camino de ida -lo que el
    /// navegador manda al guardar-; DataUri es el de vuelta -lo que el
    /// navegador pone en el src de una imagen-. Mismo molde que la firma
    /// guardada, DaoFirmaUsuario.
    /// </summary>
    public class EntPerfilFoto
    {
        /// <summary>Solo el payload, sin el prefijo "data:...;base64,". De ida.</summary>
        public string Base64 { get; set; } = "";

        /// <summary>"image/jpeg" o "image/png". De ida.</summary>
        public string Tipo { get; set; } = "";

        /// <summary>
        /// data:&lt;tipo&gt;;base64,&lt;payload&gt;, listo para el src de una imagen.
        /// Cadena vacia si la persona no tiene foto.
        ///
        /// Al leer se llena SOLO esta y no Base64 ni Tipo: las tres juntas
        /// duplicarian el tamanio del JSON del perfil por nada, y una foto de
        /// 256x256 ya son unos 25 KB de base64.
        /// </summary>
        public string DataUri { get; set; } = "";
    }
}
