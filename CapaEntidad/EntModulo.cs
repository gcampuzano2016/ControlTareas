using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntModulo
    {
        public int IdModulo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public System.DateTime FecCrea { get; set; }
        public Int32 UserCrea { get; set; }
        public System.DateTime FecModifica { get; set; }
        public Int32 UserModifica { get; set; }
        public string Grafica { get; set; }
        public string Estado { get; set; }

    }
}
