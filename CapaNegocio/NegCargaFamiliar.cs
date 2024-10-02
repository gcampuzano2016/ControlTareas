using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class NegCargaFamiliar
    {
        public static EntRespuesta Sp_InsertarActualizarCargaFam(EntCargaFamiliar objCargaFam)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoCargaFamiliar.RTAInsertarActualizarCargaFam(objCargaFam);
            return respuesta;
        }

        public static List<EntCargaFamiliar> Sp_RTAConsultarListaCargasFam()
        {
            return DaoCargaFamiliar.Consulta_Sp_RTAConsultarListaCargasFam();
        }

        public static List<EntCargaFamiliar> Sp_RTAConsultarCargaFamPorId(int idEmpleado)
        {
            return DaoCargaFamiliar.ConsultaSp_RTAConsultarCargaFamPorId(idEmpleado);
        }

        public static EntRespuesta Consulta_Sp_RTACambiarEstadoCargaFam(string Nombre)
        {
            return DaoCargaFamiliar.RTA_EliminarCargaFamiliar(Nombre);
        }
    }
}
