using System;
using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using PruebaEpplus;
using System.Diagnostics;
using System.Data;

namespace CapaNegocio
{
    public class NegEmpleado
    {
        public static EntRespuesta Sp_InsertarActualizarEmpleado(EntEmpleado objEmpleado)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoEmpleado.Consulta_Sp_InsertarActualizarEmpleado(objEmpleado);
            return respuesta;
        }

        /*public static EntRespuesta Sp_RTActualizarPerfil(EntEmpleado objEmpleado)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoEmpleados.Sp_RTActualizarPerfil(objEmpleado);
            return respuesta;
        }*/

        public static List<EntEmpleado> Sp_RTAConsultarListaEmpleados(string Cod_Usuario, int tipo)
        {            
            return DaoEmpleado.Consulta_Sp_RTAConsultarListaEmpleados(Cod_Usuario, tipo);
        }

        public static EntRespuesta Consulta_Sp_RTAConsultarListaEmpleadosDescargar()
        {
            return DaoEmpleado.Consulta_Sp_RTAConsultarListaEmpleadosDescargar();
        }


        public static EntEmpleado Sp_RTA_ConsultarEmpleadoPorId(String id)
        {            
            return DaoEmpleado.Consulta_Sp_RTAConsultarEmpleadoPorId(id);
        }
        

        public static List<EntEmpleado> Sp_RTA_ConsultarEmpleadoPorCedula(String id)
        {
            return DaoEmpleado.Consulta_Sp_RTAConsultarEmpleadoPorCedula(id);
        }

        public static EntRespuesta Consulta_Sp_RTACambiarEstadoEmpleado(int id, string estado)
        {
            return DaoEmpleado.RTA_EliminarEmpleado(id,estado);
        }


    }
}
