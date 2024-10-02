using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntMensajeForeCast
    {
        public int IdMensaje { get; set; }
        public int IdForeCast { get; set; }
        public string Usuario { get; set; }
        public string Mensaje { get; set; }
        public int Tipo { get; set; }
    }
}
