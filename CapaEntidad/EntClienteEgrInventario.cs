using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntClienteEgrInventario
    {
        public long IdCliente { get; set; }
        public string Nombres { get; set; }
        public string FechaIngreso { get; set; }
        public int Estado { get; set; }

        public long IdEncabezado { get; set; }
        public string Cod_Usuario { get; set; }
        public string FechaRegistro { get; set; }
        public double CantidadComprada { get; set; }
        public double SubTotal { get; set; }
        public double Iva { get; set; }
        public double Total { get; set; }
        public string NombreArchivo { get; set; }
        public string CodigoProceso { get; set; }
        public string Observacion { get; set; }
        public int EstadoEnc { get; set; }
    }

}
