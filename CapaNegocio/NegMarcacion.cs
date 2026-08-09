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

        public static void RegistrarLogCorreo(long idProceso, decimal idUsuario, int accion,
                                              string destinatario, string resultado, string mensaje)
        {
            DaoMarcacion.RegistrarLogCorreo(idProceso, idUsuario, accion, destinatario, resultado, mensaje);
        }
    }
}
