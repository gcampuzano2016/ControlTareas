using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class NegInfoFactProv
    {
        public static List<EntInfoFacturaProv> Sp_RTAConsultarListaProveedores(string fecIni, string fecFin, string txtfactura, string txtcliente, int tipo)
        {
            switch (tipo)
            {
                case 0:
                    return DaoInfoFactProv.Consulta_Sp_RTAConsultarListaProveedores(fecIni, fecFin, txtfactura, txtcliente,tipo);
                case 2:
                    return DaoInfoFactProv.Consulta_Sp_RTAConsultarListaProveedoresRes(txtfactura, txtcliente);
                case 1:
                    return DaoInfoFactProv.Consulta_Sp_RTAConsultarListaProveedoresSum(txtfactura, txtcliente,tipo);
                default:
                    throw new ArgumentException("Invalid tipo parameter", nameof(tipo));
            }
        }       

    }
}
