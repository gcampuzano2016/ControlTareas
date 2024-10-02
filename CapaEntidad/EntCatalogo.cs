using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace CapaEntidad
{
    public class EntCatalogo
    {

        public Int32 IdCatalogo { get; set; }
        public Int32 IdTipoCatalogo { get; set; }
        public string Descripcion { get; set; }
        public string DescripcionLarga { get; set; }
        public string IdExterno { get; set; }
        public string IdAranda { get; set; }
        public Int32 IdProyecto { get; set; }
        public Int32 IdCatalogoPadre { get; set; }
        public Int32 RequiereAprobacion { get; set; }
        public Int32 RequiereCrearTareaDetalle { get; set; }
        public Int32 EsEstadoFinal { get; set; }
        public Int32 RequiereTextoSolucion { get; set; }

    }
}
