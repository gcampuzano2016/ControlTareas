using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntEgresoInventario
    {
        public Int32 IdEncabezado { get; set; }
        public Int32 IdCliente { get; set; }
        public string Cod_Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string StrFechaRegistro { get; set; }
        public double  CantidadComprada { get; set; }
        public double SubTotal { get; set; }
        public double Iva { get; set; }
        public double Total { get; set; }
        public string Observacion { get; set; }
        public Int32 IdDetalle { get; set; }
        public Int32 IdInventario { get; set; }
        public double Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double PrecioTotal { get; set; }

        public int Estado { get; set; }
        public int Tipo { get; set; }

        public string NombreArchivo { get; set; }
        public string CodigoProceso { get; set; }
        public string Nombres { get; set; }

        public string CLIENTE { get; set; }
    }
}
