using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntArchivoTarea
    {

        public Int32 Id_ArchivosTarea { get; set; }
        public string Nombre_Archivo { get; set; }
        public string Extension_Archivo { get; set; }
        public string Codigo_Archivo { get; set; }
        public string Nombre_ArchivoCodigo { get; set; }
        public string Ruta_Archivo { get; set; }
        public string Descripcion_Archivo { get; set; }
        public Int32 Id_RegDetTareas { get; set; }
        public Int32 Id_RegTareas { get; set; }
        public Int32 Orden_Archivo { get; set; }
        public string Icon_Nombre { get; set; }
        public Int32 Estado { get; set; }
        public string Fec_Modificacion { get; set; }
        public Int32 Usu_Modificacion { get; set; }
        public string Ip_Modificacion { get; set; }
        public int idServicio { get; set; }

    }
}
