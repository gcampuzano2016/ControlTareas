using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntCosteoServicio
    {
        public int IdCosteo { get; set; }
        public Int32 conteoArchivosAdjuntos { get; set; }
        public string  Ticket { get; set; }
        public string FechaSolicitud { get; set; }
        public string FechaActual { get; set; }
        public int IdVendedor { get; set; }
        public string  Vendedor { get; set; }
        public string Sucursal { get; set; }
        public string  Sector { get; set; }
        public int IdCliente { get; set; }
        public string Cliente { get; set; }
        public string Concepto { get; set; }
        public string  UnidadNegocio { get; set; }
        public string ResponsableDimen { get; set; }
        public string TipoServicio { get; set; }
        public string PlazoEntrega { get; set; }
        public string EstadoServicio { get; set; }
        public string FechaEntregaEsp { get; set; }
        public string FechaEntregaAlc { get; set; }
        public string Revision { get; set; }
        public decimal Costo { get; set; }
        public string Observacion { get; set; }
        public string Usuario { get; set; }
        public int Tipo { get; set; }
        public int IdSucursal { get; set; }
    }
}
