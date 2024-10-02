using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntCargaFamiliar
    {
        public Int32 IdCargaFam { get; set; }
        public Int32 IdEmpleado { get; set; }
        public String Parentesco { get; set; }
        public String Nombre { get; set; }
        public string fecha_nacimiento { get; set; }
        public string Fec_Modificacion { get; set; }
        public string Usu_Modificacion { get; set; }
        public string Ip_Modificacion { get; set; }

        public Int32 Operacion { get; set; }

        public String Estado { get; set; }
    }
}
