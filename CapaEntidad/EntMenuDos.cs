using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntMenuDos
    {

        public Int32 Id_Menu { get; set; }

        public Int32 Id_Perfil { get; set; }
        public String Href { get; set; }
        public String Titulo { get; set; }
        public Int32 Id_MenuPadre { get; set; }
        public String Class_Icon { get; set; }
        public Int32 Estado { get; set; }


    }
}
