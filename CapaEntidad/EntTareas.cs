using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace CapaEntidad
{
    public class EntTareas
    {
        public string HORAS_CONTRATADAS { get; set; }
        public string HORAS_ENTREGADAS { get; set; }
        public string HORAS_DISPONIBLES { get; set; }
        public Int32 Id_RegTareasHorasExtras { get; set; }
        public Int32 Id_RegTareas { get; set; }
        public int idServicio { get; set; }

        public int Tipo { get; set; }
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
        public string Estado2 { get; set; }
        public string ResponsableAprobacion { get; set; }
        public string MailResponsableAprobacion { get; set; }
        public string MailAprobacionTitulo { get; set; }
        public Int32 TipoAprobacion { get; set; }
        public Int32 IdEstado { get; set; }
        public Int32 IdEstadoAprobacion { get; set; }
        public Int32 IdEstadoTarea { get; set; }
        public string EstadoTarea { get; set; }
        public string CatalogoTareaSap { get; set; }
        public Int32 IdProyecto { get; set; }
        public Int32 Cod_CatalogoTareaSap { get; set; }
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
        public Int32 TareaEnEjecucion { get; set; }
        public Int32 conteoArchivosAdjuntos { get; set; }

        public Int32 IdCategoria { get; set; }

        public string DetCategoria { get; set; }
        public string Valor { get; set; }

        public string Det_Fch_RegDetalleIni { get; set; }
        public string Det_Fch_RegDetalleFin { get; set; }

        public string Det_Tiempo { get; set; }

        public string TipoHoras { get; set; }

        public string Tipos { get; set; }
        public string FechaRegistro { get; set; }

        public string EstadoAprobacion { get; set; }
        public string Proceso { get; set; }
    }
}
