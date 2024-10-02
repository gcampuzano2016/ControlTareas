using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntPedido
    {
        public Int32 IdSucursal { get; set; }
        public Int32 IdPedido { get; set; }
        public Int32 IdClasificacion { get; set; }
        public Int32 IdCliente { get; set; }
        public Int32 IdVendedor { get; set; }
        public Int32 IdGerenteProducto { get; set; }
        public string FECHA { get; set; }
        public int PEDIDO { get; set; }
        public string CLIENTE { get; set; }
        public string SEGMENTACION { get; set; }
        public string CLASIFICACION { get; set; }
        public string DETALLE { get; set; }
        public double VALOR { get; set; }
        public double RENTABILIDAD { get; set; }
        public double MARGEN { get; set; }
        public string ESTADO { get; set; }
        public string N_FACTURA { get; set; }
        public string FECHA_FACTURACION { get; set; }
        public string FECHA_ESTIMADA_DE_FACTURACION { get; set; }
        public string OBSERVACION { get; set; }
        public string VENDEDOR { get; set; }
        public string GERENTE_DE_PRODUCTO { get; set; }
        public string ORDEN_DE_COMPRA { get; set; }
        public string ORDEN_DE_SERVICIOS_INTERNA { get; set; }
        public string ORDEN_DE_SERVICIOS_EXTERNA { get; set; }
        public string ORDEN_DE_SERVICIOS_DE_FINANZAS { get; set; }
        public string SUCURSAL { get; set; }
        public int Tipo { get; set; }
        public int Anio { get; set; }
        public int meses { get; set; }
        public int idFecha { get; set; }
        public int ChekRenovacion { get; set; }
        public string FechaInicioR { get; set; }
        public string FechaFinalR { get; set; }


    }
}
