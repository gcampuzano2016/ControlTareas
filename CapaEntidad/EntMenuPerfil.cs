namespace CapaEntidad
{
    public class EntMenuPerfil
    {
        public int Id_Menu { get; set; }
        public int Id_MenuPadre { get; set; }
        public string Titulo { get; set; }
        public string Class_Icon { get; set; }
        public int Activo { get; set; }
    }
}
