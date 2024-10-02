using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntOrdenServicio
    {
        public Int64 IdServicio { get; set; }
        public Int64 IdClasificacion { get; set; }
        public Int64 IdCliente { get; set; }
        public Int64 IdGerenteCuenta { get; set; }
        public Int64 IdGestorResponsable { get; set; }
        public string TiempoRespuesta { get; set; }
        public string TiempoSolucion { get; set; }
        public string TiempoDiagnostico { get; set; }
        public DateTime FECHA { get; set; }
        public float PEDIDO { get; set; }
        public string CLIENTE { get; set; }
        public string REFERENCIA_CLIENTE { get; set; }
        public string AREA { get; set; }
        public string GESTOR_RESPONSABLE { get; set; }
        public string ORDEN { get; set; }
        public string CLASIFICACION { get; set; }
        public string DESCRIPCION_DE_SERVICIO { get; set; }
        public string SLA_CONTRATO { get; set; }
        public float HORAS_CONTRATADAS { get; set; }
        public float HORAS_ENTREGADAS { get; set; }
        public float HORAS_DISPONIBLES { get; set; }
        public float COSTO_PLAN { get; set; }
        public float COSTO_REAL { get; set; }
        public float SALDO_DE_COSTOS { get; set; }
        public string ESTATUS { get; set; }
        public DateTime FECHA_ESTIMADA_DE_CIERRE { get; set; }
        public DateTime FECHA_CIERRE { get; set; }
        public string MANTENIMIENTO { get; set; }
        public string MANT_ENTREGADOS { get; set; }
        public string OBSERVACIONES { get; set; }
        public string CONTACTO_CLIENTE { get; set; }
        public string GERENTE_DE_CUENTA { get; set; }
        public string SUCURSAL { get; set; }
        public Int32 Estado { get; set; }
        public float COSTO_PLAN1 { get; set; }
        public float COSTO_REAL1 { get; set; }
        public float SALDO_DE_COSTOS1 { get; set; }
        public float COSTO_PLAN_OR { get; set; }
        public float COSTO_REAL_OR { get; set; }
        public float SALDO_DE_COSTOS_OR { get; set; }
        public Int32 CambiarEstado { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string Usuario { get; set; }
    }
}
