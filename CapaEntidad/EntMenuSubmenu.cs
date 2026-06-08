using System;

namespace CapaEntidad
{
    public class EntMenuSubmenu
    {

        public Int32 Id { get; set; }
        public string Orden_Menu { get; set; }
        public Int32 Id_Menu { get; set; }
        public Int32 Id_MenuPadre { get; set; }
        public Int32 Orden_Menu_Final { get; set; }
    }
}
