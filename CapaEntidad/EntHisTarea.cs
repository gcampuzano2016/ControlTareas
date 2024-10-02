using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntHisTarea
    {
        public Int32 Id_RegDetTareas { get; set; }
        public Int32 Id_RegTareas { get; set; }
        public string Det_Num_OrdenServicio { get; set; }
        public string Det_Id_CompAranda { get; set; }
        public string Det_Fch_RegDetalleIni { get; set; }
        public string Det_Fch_RegDetalleFin { get; set; }
        public string Det_EstadoIni { get; set; }
        public string Det_EstadoFin { get; set; }
        public string Det_Nom_Empresa { get; set; }
        public string Det_Det_Tarea { get; set; }
        public string Det_Estado { get; set; }
        public string Det_Motivo_Cambio_Estado { get; set; }
    }
}
