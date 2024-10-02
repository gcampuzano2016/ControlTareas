using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntParametrosConfiguracion
    {

        public Int32 Id_ParametrosConfiguracion { get; set; }
        public string Nombre { get; set; }
        public string Valor { get; set; }
        public Int32 Estado { get; set; }
        public string Fec_Modificacion { get; set; }
        public Int32 Usu_Modificacion { get; set; }

    }
}
