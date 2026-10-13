using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegMenuPerfil
    {
        public static List<EntPerfil> ListarPerfiles()
        {
            return DaoMenuPerfil.ListarPerfiles();
        }

        public static List<EntMenuPerfil> ListarMenuPerfil(int idPerfil)
        {
            return DaoMenuPerfil.ListarMenuPerfil(idPerfil);
        }

        public static EntRespuesta GuardarMenuPerfil(int idPerfil, string activosCsv)
        {
            return DaoMenuPerfil.GuardarMenuPerfil(idPerfil, activosCsv);
        }
    }
}
