using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegInfoAdicionalHisClinica
    {
        public static EntRespuesta Sp_InsUpdInfoAdicional(EntInfoAdicionalHisClinica objinfoAdicional, int op)
        {
            EntRespuesta respuesta = new EntRespuesta();
            if (op == 1)
            {
                respuesta = DaoInfoAdicionalHisClinica.Consulta_Sp_InsUpdInfoAdicional1(objinfoAdicional);
            }
            else if (op == 2)
            {
                respuesta = DaoInfoAdicionalHisClinica.Consulta_Sp_InsUpdInfoAdicional2(objinfoAdicional);
            }

            return respuesta;
        }


        public static List<EntInfoAdicionalHisClinica> Sp_RTA_ConsultarInfoPorId(String id)
        {
            return DaoInfoAdicionalHisClinica.Consulta_Sp_RTAConsultarInfoPorId(id);
        }


        public static EntRespuesta Sp_InsUpdInfoRelevante(List<EntDatosMed> objinfoRelevante, int op)
        {
            EntRespuesta respuesta = new EntRespuesta();
            if (op == 1)
            {
                respuesta = DaoInfoAdicionalHisClinica.Consulta_Sp_InsUpdInfoRelevante(objinfoRelevante);
            }
            else if (op == 2)
            {
                respuesta = DaoInfoAdicionalHisClinica.Consulta_Sp_InsUpdInfoRelevante(objinfoRelevante);
            }

            return respuesta;
        }

    }
}
