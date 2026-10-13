using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegPerfilInicio
    {
        public static List<EntPerfilInicio> ListarPerfilInicio()
        {
            return DaoPerfilInicio.ListarPerfilInicio();
        }

        public static EntRespuesta GuardarPerfilInicio(int idPerfil, string href, int idTipo, string usuarioRegistro)
        {
            return DaoPerfilInicio.GuardarPerfilInicio(idPerfil, href, idTipo, usuarioRegistro);
        }

        public static EntPerfilInicio ObtenerPerfilInicio(int idPerfil)
        {
            return DaoPerfilInicio.ObtenerPerfilInicio(idPerfil);
        }

        public static List<EntMenuDos> ListarPaginasMenu()
        {
            return DaoPerfilInicio.ListarPaginasMenu();
        }
    }
}
