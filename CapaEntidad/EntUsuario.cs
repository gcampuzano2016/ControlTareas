using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace CapaEntidad
{
    public class EntUsuario
    {
        public Int32 Id_Perfil { get; set; }
        public Int32 Id_Usuario { get; set; }
        public Int32 IdCliente { get; set; }
        public string Cod_Usuario { get; set; }
        public string Nom_Usuario { get; set; }
        public string Log_Usuario { get; set; }
        public string Pass_Usuario { get; set; }
        public Int32 Cod_Perfil { get; set; }
        public string E_Mail { get; set; }
        public Int32 Rol_Usuario { get; set; }
        public string Fec_Creacion { get; set; }
        public string Usuario_Estado { get; set; }
        public string Cod_Jefe_Inm { get; set; }
        public string MailCodJefeInm { get; set; }

        public string ID { get; set; }

        public string NOMBRE { get; set; }

        public string Cedula { get; set; }
        public string Departamento { get; set; }
        public string JefeInmediato { get; set; }
        public string CodigoReset { get; set; }

    }
}
