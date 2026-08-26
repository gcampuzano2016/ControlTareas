using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegHsCln
    {
        public static EntRespuesta Sp_InsertarActualizarHsCln(EntHsCln objHsCln)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoHsCln.Consulta_Sp_InsUpd_HsCln(objHsCln);
            return respuesta;
        }

        public static EntHsCln Consulta_Sp_DatosPerVulnerable(int op, string CI)
        {
            return DaoHsCln.Consulta_Sp_DatosPerVulnerable(op, CI);
        }

        public static List<EntHsCln> ConsultaLista_Sp_DatosPerVulnerable(int op, string CI)
        {
            return DaoHsCln.ConsultaLista_Sp_DatosPerVulnerable(op, CI);
        }

        public static List<EntHsCln> ConsultaSp_RTAConsultarTodasHsClnPorCI(string CI, int op, string fecha_ini, string fecha_fin)
        {
            return DaoHsCln.ConsultaSp_RTAListaHsCln(CI, op, fecha_ini, fecha_fin);
        }
    }
}
