namespace CapaEntidad
{
    /// <summary>
    /// El resultado de preguntar "sobre que perfil se esta actuando".
    ///
    /// No es un simple string porque hay tres desenlaces y no dos: el propio
    /// perfil, el de otra persona con permiso para ello, y el rechazo. Un string
    /// vacio para el rechazo obligaria a cada sitio a inventarse el mensaje, y
    /// entonces el mensaje dependeria del sitio en vez de la regla.
    /// </summary>
    public class EntPerfilObjetivo
    {
        public bool Permitido { get; set; }

        /// <summary>El Cod_Usuario del perfil sobre el que se actua. Vacio si no se permite.</summary>
        public string CodUsuario { get; set; }

        /// <summary>Que decirle al usuario cuando no se permite. Vacio si se permite.</summary>
        public string Mensaje { get; set; }
    }
}
