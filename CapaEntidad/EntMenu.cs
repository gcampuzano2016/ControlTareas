using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace CapaEntidad
{
    public class EntMenu
    {

        public Int32 IdMenu { get; set; }
        public Int32 IdMenuPadre { get; set; }
        public Int32 EsOpcionDeMenu { get; set; }
        public string Href { get; set; }
        public string ClassOpcion { get; set; }
        public string ClassIcon { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public Int32 EsOpcionPublica { get; set; }
        public Int32 OrdenOpcion { get; set; }
    }
}
