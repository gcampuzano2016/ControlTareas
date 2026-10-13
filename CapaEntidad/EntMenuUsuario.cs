namespace CapaEntidad
{
    public class EntMenuUsuario
    {
        public int Id_Menu { get; set; }
        public int Id_MenuPadre { get; set; }
        public string Titulo { get; set; }
        public string Class_Icon { get; set; }
        public int ActivoPerfil { get; set; }
        public int ActivoUsuario { get; set; }
    }
}
