using CapaDato;

namespace CapaNegocio
{
    public class NegFirmaUsuario
    {
        /// <summary>
        /// La firma guardada de una persona como data URI, o cadena vacia.
        /// </summary>
        public static string Obtener(string codUsuario)
        {
            return DaoFirmaUsuario.Obtener(codUsuario);
        }
    }
}
