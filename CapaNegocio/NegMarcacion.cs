using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    public class NegMarcacion
    {
        public static EntMarcacion RegistrarMarcacion(decimal idUsuario, int accion)
        {
            return DaoMarcacion.RegistrarMarcacion(idUsuario, accion);
        }
    }
}
