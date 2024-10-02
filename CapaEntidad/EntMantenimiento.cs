using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntMantenimiento
    {
        public Int32 IdRequerimiento { get; set; }
        public Int32 IdServicio { get; set; }

        public Int32 Tipo { get; set; }
        public string Orden { get; set; }
        public string OrdenServicio { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFinal { get; set; }
        public double Valor { get; set; }
        public string Descripcion { get; set; }

        public string Observacion { get; set; }
        public string Clasificacion { get; set; }
        public int Estado { get; set; }
        public string FechIngreso { get; set; }

        public string CLIENTE { get; set; }

        public Int32 conteoArchivosAdjuntos { get; set; }

        public string FECHA { get; set; }
        public Int32 PEDIDO { get; set; }
        public string AREA { get; set; }
        public string SUCURSAL { get; set; }
        public string GESTOR_RESPONSABLE { get; set; }
        public string ESTATUS { get; set; }
        public float HORAS_CONTRATADAS { get; set; }
        public float HORAS_ENTREGADAS { get; set; }
        public float HORAS_DISPONIBLES { get; set; }
        public String ORDEN { get; set; }
        public Int32 ID_TAREA { get; set; }
        public String ID_COMPRA { get; set; }
        public String NOM_RESPONSABLE { get; set; }
        public String NOM_EMPRESA { get; set; }
        public String DETALLE { get; set; }
        public String DETALLE_TAREA { get; set; }
        public String FECHA_CREACION { get; set; }
        public String FECHA_INICIO { get; set; }
        public String FECHA_FIN { get; set; }
        public String TIEMPO { get; set; }
        public String OBSERVACIONES { get; set; }

    }
}
