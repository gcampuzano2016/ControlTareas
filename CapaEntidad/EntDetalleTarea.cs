using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntDetalleTarea
    {
        public Int32 Id_RegDetTareas { get; set; }
        public Int32 Id_RegTareas { get; set; }
        public string Det_Num_OrdenServicio { get; set; }
        public string Det_Id_CompAranda { get; set; }
        public string Det_Fch_RegDetalleIni { get; set; }
        public string Det_Fch_RegDetalleFin { get; set; }
        public string Det_EstadoIni { get; set; }
        public string Det_EstadoFin { get; set; }
        public string Det_Tiempo { get; set; }
        public string Det_Nom_Empresa { get; set; }
        public string Det_Det_Tarea { get; set; }
        public string Det_Tarea { get; set; }
        public string Det_Estado { get; set; }
        public string Id_Responsable { get; set; }
        public string Det_Det_TareaFin { get; set; }
        public string Det_Motivo_Cambio_Estado { get; set; }
        public Int32 IdDet_EstadoFin { get; set; }
        public Int32 IdDet_EstadoIni { get; set; }
        public string Det_Observaciones { get; set; }
        public Int32 Det_Horas_Extras_Estado { get; set; }
        public Int32 Det_Horas_Extras_Tipo { get; set; }
        public string Det_Horas_Extras_Descripcion { get; set; }
        public Int32 Det_Horas_Extras_Envio_Correo { get; set; }
        public string UsuarioResponsable { get; set; }

        public string Det_Horas_Extras_Fecha_Solicitud { get; set; }
        public string Det_Horas_Extras_Fecha_Aprobacion { get; set; }
        public Int32 Det_Aprobacion_Tarea_Estado { get; set; }
        public string Det_Horas_Extras_Estado_Descripcion { get; set; }
        public string Det_Fecha_Creacion { get; set; }
        public string Det_Id_Usuario_Creacion { get; set; }
        public string Det_Ip_Creacion { get; set; }
        public string Det_Fecha_Modificacion { get; set; }
        public string Det_Id_Usuario_Modificacion { get; set; }
        public string Det_Ip_Modificacion { get; set; }
        public string Det_Estado_Logico_Registro { get; set; }
        public Int32 Cod_CatalogoTareaSap { get; set; }
        public string Det_Aprobacion_Tarea_Estado_Descripcion { get; set; }
        public string Det_Fecha_Aprobacion_Tarea { get; set; }
        public Int32 Det_Aprobacion_Tarea_Estado_QA { get; set; }
        public string Det_Aprobacion_Tarea_Estado_QA_Descripcion { get; set; }
        public string Det_Fecha_Aprobacion_Tarea_QA { get; set; }

        public Int32 IdTipoGasto { get; set; }

    }
}
