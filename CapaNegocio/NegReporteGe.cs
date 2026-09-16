using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegReporteGe
    {

        public static List<EntReporteGe> ConsultaSp_RTAConsultarForeCast(int Anio, int tipo)
        {
            return DaoReporteGe.ConsultaSp_RTA_GenerarReporteGDOS(Anio, tipo);
        }

        public static EntRespuesta ConsultaSp_RTAListaForeCastDescargar(int Anio, int tipo)
        {
            return DaoReporteGe.ConsultaSp_RTA_GenerarReporteGDOSDescargar(Anio, tipo);
        }
    }
}
