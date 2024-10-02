using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntSolicitud
    {
        public Int64 IdVacaciones { get; set; }
        public Int64 IdTipoSolicitud { get; set; }
        public string FechaRegistro { get; set; }
        public int CodSap { get; set; }
        public string Cedula { get; set; }
        public string Colaborador { get; set; }
        public string Departamento { get; set; }
        public string JefeInmediato { get; set; }
        public string Remplazo { get; set; }
        public string FechaDesde { get; set; }
        public string FechaHasta { get; set; }
        public double TotalDias { get; set; }
        public double Feriado { get; set; }
        public double SaldoDias { get; set; }
        public double CargoVacaciones { get; set; }
        public string StrCargoVacaciones { get; set; }
        public string Horas { get; set; }
        public string Actividad { get; set; }
        public string Observacion { get; set; }
        public string EstadoSolicitud { get; set; }
        public string FechaAprobacion { get; set; }
        public string FechaRechazo { get; set; }
        public string UsuarioAprobo { get; set; }
        public string UsuarioRechazo { get; set; }
        public string Cod_Usuario { get; set; }
        public int Tipo { get; set; }
        public string Descripcion { get; set; }
        public string Ruta_Archivo { get; set; }
        public string Descripcion_Archivo { get; set; }

        public string MotivoAnulacion { get; set; }
        public int conteoArchivosAdjuntos { get; set; }

    }
}
