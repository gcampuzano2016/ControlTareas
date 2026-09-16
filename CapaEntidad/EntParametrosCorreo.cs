using System;

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
