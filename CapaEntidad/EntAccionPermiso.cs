namespace CapaEntidad
{
    /// <summary>Una accion del catalogo RTA_Accion con su estado para un perfil o un usuario.</summary>
    public class EntAccionPermiso
    {
        public int Id_Accion { get; set; }
        public string Clave { get; set; } = "";
        public int Id_Menu { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int Activo { get; set; }
        public int ActivoPerfil { get; set; }
        public int ActivoUsuario { get; set; }
    }
}
