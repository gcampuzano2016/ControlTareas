using System;
using CapaDato;
using CapaEntidad;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static EntInfoContrato Sp_RTAConsultarContratoNum(String num)
        {
            return DaoInfoContrato.Consulta_Sp_RTAConsultarContratoNum(num);
        }
        public static List<EntOrdenServicio> Sp_RTAConsultarOSnumPedido(float num)
        {
            return DaoInfoContrato.Consulta_Sp_ConsultarOrdenServicio(num);
        }
    }
}
