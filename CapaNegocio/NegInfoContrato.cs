using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegInfoContrato
    {
        public static List<EntInfoContrato> Sp_RTA_ConsultarPermisoContratos()
        {
            return DaoInfoContrato.Consulta_Sp_RTAConsultarpermisoContratos();
        }

        public static EntRespuesta Sp_InsertarActualizarContrato(EntInfoContrato objInfoContrato)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoInfoContrato.Consulta_Sp_InsertarActualizarContrato(objInfoContrato);
            return respuesta;
        }

        public static EntInfoContrato Sp_RTAConsultarPermisoContratoNum(String num, String cod)
        {
            return DaoInfoContrato.Consulta_Sp_RTAConsultarPermisoContratoNum(num, cod);
        }

        public static EntInfoContrato Sp_RTAConsultarContratoNum(String num, int op)
        {
            return DaoInfoContrato.Consulta_Sp_RTAConsultarContratoNum(num, op);
        }

        public static List<EntInfoContrato> Sp_RTA_ConsultarContratos(String num)
        {
            return DaoInfoContrato.Consulta_Sp_RTAConsultarContratos(num);
        }

        public static List<EntOrdenServicio> Sp_RTAConsultarOSnumPedido(float num)
        {
            return DaoInfoContrato.Consulta_Sp_ConsultarOrdenServicio(num);
        }
    }
}
