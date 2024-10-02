using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntTareaPrincipal
    {
        public Int32 Id_RegTareas { get; set; }
        public string Num_OrdenServicio { get; set; }
        public string Id_CompAranda { get; set; }
        public string Fch_Registro { get; set; }
        public string Fch_EstAtencion { get; set; }
        public string Fch_EstSolucion { get; set; }
        public string Id_Responsable { get; set; }
        public string Nom_Responsable { get; set; }
        public string Nom_Empresa { get; set; }
        public string Nom_SlaAranda { get; set; }
        public string Det_Tarea { get; set; }
        public string Estado { get; set; }
        public string Categoria { get; set; }
        public string SubCategoria { get; set; }
        public string EstadoApro { get; set; }
        public string FechaAprobacion { get; set; }
        public Int32 IdEstado { get; set; }
        public Int32 IdProyecto { get; set; }
        public string IdEstadoAprobacion { get; set; }
        public Int32 Cod_CatalogoTareaSap { get; set; }
        public Int32 IdEstadoTarea { get; set; }
        public string EstadoTarea { get; set; }
        public string OrdenOperacion { get; set; }
        public Int32 StateId { get; set; }
        public string StateName { get; set; }
        public string NombreCliente { get; set; }
        public string Det_Fecha_Creacion { get; set; }
        public string Det_Id_Usuario_Creacion { get; set; }
        public string Det_Ip_Creacion { get; set; }
        public string Det_Fecha_Modificacion { get; set; }
        public string Det_Id_Usuario_Modificacion { get; set; }
        public string Det_Ip_Modificacion { get; set; }
        public Int32 Det_Estado_Logico_Registro { get; set; }

    }
}
