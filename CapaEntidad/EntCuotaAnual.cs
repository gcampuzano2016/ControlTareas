using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntCuotaAnual
    {
        public Int32 IdCuotaAnual { get; set; }
        public Int32 IdProceso { get; set; }
        public Int32 IdMetas { get; set; }
        public string  Fecha { get; set; }
        public string CuotaAnual { get; set; }
        public Int32 Tipo { get; set; }
        public string Nombres { get; set; }
        public Int32 ID { get; set; }
        public string MetaFacturacion { get; set; }
        public string MetaMargenBruto { get; set; }
        public string Estado { get; set; }
        public string Anio { get; set; }
        public Int32 IdGerencial { get; set; }
        public Int32 IdEstado { get; set; }
        public Int32 IdSucursal { get; set; }
    }
}
