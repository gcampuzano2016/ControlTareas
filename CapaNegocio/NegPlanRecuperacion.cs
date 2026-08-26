using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegPlanRecuperacion
    {
        public static EntRespuesta Guardar(EntPlanRecuperacion plan)
        {
            return DaoPlanRecuperacion.Guardar(plan);
        }

        public static EntRespuesta Confirmar(long idVacaciones, bool seRecupero, string observacion, string codUsuario)
        {
            return DaoPlanRecuperacion.Confirmar(idVacaciones, seRecupero, observacion, codUsuario);
        }

        public static List<EntPlanRecuperacion> ListarPendientes()
        {
            return DaoPlanRecuperacion.ListarPendientes();
        }
    }
}
