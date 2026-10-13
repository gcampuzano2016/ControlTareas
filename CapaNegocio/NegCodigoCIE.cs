using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

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
