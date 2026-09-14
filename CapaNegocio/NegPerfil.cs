using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    /// <summary>Fachada del modulo de perfil, igual que el resto de la capa.</summary>
    public class NegPerfil
    {
        /// <summary>
        /// Lee el perfil y le pone la edad.
        ///
        /// La edad se calcula aca y no en el Dao porque NegPerfilCampos vive en
        /// esta capa y CapaDato no puede referenciarla sin cerrar un ciclo. No es
        /// un rodeo: leer y calcular son dos cosas distintas y esta es la capa que
        /// calcula.
        ///
        /// Tampoco se calcula en SQL, que seria el otro lugar tentador: alli el
        /// formato dd/MM/yyyy depende de acertarle al estilo 103 y no hay forma de
        /// probarlo. Aca esta cubierto por NegPerfilCamposTests.
        /// </summary>
        public static EntPerfilCompleto CargarPerfil(string codUsuario)
        {
            EntPerfilCompleto perfil = DaoPerfil.CargarPerfil(codUsuario);

            perfil.Cabecera.Edad =
                NegPerfilCampos.EdadDesdeTexto(perfil.Cabecera.FechaNacTexto);

            return perfil;
        }
    }
}
