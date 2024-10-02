using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class NegCodigoCIE
    {
        public static List<EntCodigoCIE> Sp_RTA_ConsultarCodigoCIE(String id)
        {
            return DaoCodigoCIE.Consulta_Sp_RTAConsultarCodigoCIE(id);
        }
    }
}
