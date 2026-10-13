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

        public static List<string> ListarDepartamentos()
        {
            return DaoUsuarioAdmin.ListarDepartamentos();
        }

        public static List<EntUsuarioBitacora> ListarBitacora(decimal idUsuario)
        {
            return DaoUsuarioAdmin.ListarBitacora(idUsuario);
        }

        public static EntRespuesta ActualizarUsuario(EntUsuarioAdmin u, string usuarioRegistro)
        {
            return DaoUsuarioAdmin.ActualizarUsuario(u, usuarioRegistro);
        }

        public static EntRespuesta CambiarEstado(decimal idUsuario, bool inactivar, string usuarioRegistro)
        {
            return DaoUsuarioAdmin.CambiarEstado(idUsuario, inactivar, usuarioRegistro);
        }
        /// <summary>
        /// Cambia el perfil de un usuario ya creado. Quien puede hacerlo lo
        /// decide el handler -es el unico que conoce la sesion-; esta fachada
        /// no comprueba perfiles, igual que el resto de la capa.
        /// </summary>
        public static EntRespuesta CambiarPerfil(decimal idUsuario, long idPerfil, string usuarioRegistro)
        {
            return DaoUsuarioAdmin.CambiarPerfil(idUsuario, idPerfil, usuarioRegistro);
        }

        /// <summary>
        /// true si el usuario al que se le va a cambiar el perfil es el mismo
        /// que esta pidiendo el cambio.
        ///
        /// Funcion pura y publica para poder probarla sin sesion ni base. La
        /// regla que protege: nadie cambia su propio perfil. Sin ella, el
        /// ultimo Super Admin puede quitarse el perfil y dejar el sistema sin
        /// nadie que administre, y no hay forma de deshacerlo desde la
        /// aplicacion.
        ///
        /// Compara numeros y no cadenas: el Id_Usuario de la sesion llega como
        /// object y puede venir como "12" o como 12 segun por donde se haya
        /// guardado. Cualquier cosa que no sea un numero da false y deja que la
        /// comprobacion del procedimiento sea la que decida.
        /// </summary>
        public static bool EsSuPropioUsuario(decimal idObjetivo, object idSesion)
        {
            if (idSesion == null) { return false; }

            decimal idDeLaSesion;
            if (!decimal.TryParse(System.Convert.ToString(idSesion), out idDeLaSesion)) { return false; }

            return idDeLaSesion == idObjetivo;
        }
    }
}
