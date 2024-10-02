using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace CapaEntidad
{
    public class EntContrato
    {
        public Int32 IdServicio { get; set; }
        public Int32 IdClasificacion { get; set; }
        public Int32 IdCliente { get; set; }
        public Int32 IdGerenteCuenta { get; set; }
        public Int32 IdGestorResponsable { get; set; }
        public Int32 conteoArchivosAdjuntos { get; set; }
        public string FECHA { get; set; }
        public int PEDIDO { get; set; }
        public string CLIENTE { get; set; }
        public string REFERENCIA_CLIENTE { get; set; }
        public string AREA { get; set; }
        public string GESTOR_RESPONSABLE { get; set; }
        public string ORDEN { get; set; }
        public string CLASIFICACION { get; set; }
        public string DESCRIPCION_DE_SERVICIO { get; set; }
        public string SLA_CONTRATO { get; set; }
        public string HORAS_CONTRATADAS { get; set; }
        public string HORAS_ENTREGADAS { get; set; }
        public string HORAS_DISPONIBLES { get; set; }
        public string COSTO_PLAN { get; set; }
        public string COSTO_REAL { get; set; }
        public string SALDO_DE_COSTOS { get; set; }
        public string ESTATUS  { get; set; }
        public string FECHA_ESTIMADA_DE_CIERRE { get; set; }
        public string FECHA_CIERRE { get; set; }
        public string MANTENIMIENTO { get; set; }
        public string MANT_ENTREGADOS { get; set; }
        public string OBSERVACIONES { get; set; }
        public string CONTACTO_CLIENTE { get; set; }
        public string GERENTE_DE_CUENTA { get; set; }
        public string SUCURSAL { get; set; }
        public string TiempoRespuesta { get; set; }
        public string TiempoSolucion { get; set; }
        public string TiempoDiagnostico { get; set; }
        public string Usuario { get; set; }
        public string MAIL { get; set; }

    }
}