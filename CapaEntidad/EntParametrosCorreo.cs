using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace CapaEntidad
{
    public class EntParametrosCorreo
    {

        public string smtpAddress { get; set; }
        public string emailFrom { get; set; }
        public string emailFromName { get; set; }
        
        public string password { get; set; }
        public Int32 portNumber { get; set; }
        public bool enableSSL { get; set; }
        public string subject { get; set; }

    }
}
