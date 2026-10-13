using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegMenuUsuario
    {
        public static List<EntUsuarioMenuBusqueda> ListarUsuarios(string filtro)
        {
            return DaoMenuUsuario.ListarUsuarios(filtro);
        }

        public static List<EntMenuUsuario> ListarMenuUsuario(string codUsuario)
        {
            return DaoMenuUsuario.ListarMenuUsuario(codUsuario);
        }

        public static EntRespuesta GuardarMenuUsuario(string codUsuario, string extrasCsv, string usuarioRegistro)
        {
            return DaoMenuUsuario.GuardarMenuUsuario(codUsuario, extrasCsv, usuarioRegistro);
        }
    }
}
