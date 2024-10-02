using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class EntRebates
    {
        public string FECHA { get; set; }
        public string ID_TRANSACCION { get; set; }
        public string PROGRAMA { get; set; }
        public string TIPO_DE_INGRESO { get; set; }
        public string PROCESO { get; set; }
        public string MARCA { get; set; }
        public string DESCRIPCION { get; set; }
        public string VALOR { get; set; }
        public string Q_FABRICANTE { get; set; }
        public string TIPO_DE_PAGO { get; set; }
        public string ESTADO { get; set; }
        public string ID_TRANSACCION1 { get; set; }
        public string ID_PAGO { get; set; }
        public string FECHA_PAGO { get; set; }
        public string FECHA_ESTIMADA_PAGO { get; set; }
        public string OBSERVACIONES { get; set; }
        public string RESPONSABLE { get; set; }
        public int Tipo { get; set; }
        public Int32 IdRebates { get; set; }
        public Int32 IdTipoIngreso { get; set; }
        public Int32 IdMarca { get; set; }
        public Int32 IdPago { get; set; }

        //Banco
        public Int32 IdBanco { get; set; }
        public string DescripcionB { get; set; }
        public string ValorB { get; set; }
        public string FechaIngreso { get; set; }

        //AñoFiscal
        public Int32 IdAnioFiscal { get; set; }
        public string DescripcionP { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFinal { get; set; }
        public string FechaInicioDOS { get; set; }
        public string FechaFinalDOS { get; set; }
        public Int32 Estado { get; set; }

        public Int32 conteoArchivosAdjuntos { get; set; }

    }
}
