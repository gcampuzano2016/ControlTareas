using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegUsuarioAdmin
    {
        public static List<EntUsuarioAdmin> ListarUsuarios(string filtro)
        {
            return DaoUsuarioAdmin.ListarUsuarios(filtro);
        }

        public static List<EntUsuarioBitacora> ListarBitacora(decimal idUsuario)
        {
            return DaoUsuarioAdmin.ListarBitacora(idUsuario);
        }

        public static EntRespuesta ActualizarUsuario(EntUsuarioAdmin u, string usuarioRegistro)
        {
            return DaoUsuarioAdmin.ActualizarUsuario(u, usuarioRegistro);
        }

        public static EntRespuesta RestablecerPassword(decimal idUsuario, string hashMd5, string usuarioRegistro)
        {
            return DaoUsuarioAdmin.RestablecerPassword(idUsuario, hashMd5, usuarioRegistro);
        }
    }
}
