using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntDetalleInventario
    {
        public long IdDetalle { get; set; }
        public long IdEncabezado { get; set; }
        public double Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double PrecioTotal { get; set; }

        public long IdInventario { get; set; }
        public long IdMarca { get; set; }
        public string CodigoSAP { get; set; }
        public string NumParte { get; set; }
        public string Descripcion { get; set; }
        public double CantidadInv { get; set; }
        public string Ubicacion { get; set; }
        public string Almacen { get; set; }
        public string NumSerie { get; set; }
        public double PrecioUnitarioInv { get; set; }
        public double PrecioTotalInv { get; set; }
        public string FechaIngreso { get; set; }
        public string Usuario { get; set; }
        public int Estado { get; set; }
    }

}
