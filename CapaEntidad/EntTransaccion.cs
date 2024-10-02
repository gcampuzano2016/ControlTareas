using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntTransaccion
    {
        public Int32 IdTransaccion { get; set; }
        public int IdModulo { get; set; }
        public string NombrePresenta { get; set; }
        public string NombrePantalla { get; set; }
        public string Descripcion { get; set; }
        public System.DateTime FecCrea { get; set; }
        public Int32 UserCrea { get; set; }
        public System.DateTime FecModifica { get; set; }
        public Int32 UserModifica { get; set; }
        public string Estado { get; set; }
    }
}
