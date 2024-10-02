using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntEmpleado
    {
        public Int32 IdEmpleado { get; set; }
        public String Cedula { get; set; }
        public String Nombre { get; set; }
        public String Sociedad { get; set; }
        public String Ciudad { get; set; }
        public String AreaTrabajo { get; set; }
        public String Direccion { get; set; }
        public String Telefono { get; set; }
        public String Sexo { get; set; }
        public string Fecha_Nacimiento { get; set; }
        public String Provincia { get; set; }
        public string Fec_Modificacion { get; set; }
        public string Usu_Modificacion { get; set; }
        public string Ip_Modificacion { get; set; }
        public String Estado { get; set; }

        public String EstadoCivil { get; set; }
        public String PuestoTrabajo { get; set; }
        public String Correo { get; set; }

        public Int32 Notificacion { get; set; }
    }
}
