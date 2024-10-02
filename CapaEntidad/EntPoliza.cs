using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
   public class EntPoliza
    {
        public Int32 conteoArchivosAdjuntos { get; set; }
        public Int32 IdPoliza { get; set; }
        public Int32 IdPedido { get; set; }
        public string NumFactura { get; set; }
        public string NumPoliza { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string Monto { get; set; }
        public string TipoPoliza { get; set; }
        public string UsuarioRegistro { get; set; }
        public int Tipo { get; set; }
        public string OC { get; set; }
        public string OS { get; set; }
        public string ANEXO { get; set; }
        public string BENEFICIARIO { get; set; }
        public string EMISION { get; set; }
        public string VALOR { get; set; }
        public string PEDIDO { get; set; }
        public string OBJETO { get; set; }
        public string Proceso { get; set; }
        public string EstadoPoliza { get; set; }

    }
}
