using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    public class NegDetallePermiso
    {
        public static EntRespuesta Guardar(EntDetallePermiso detalle)
        {
            return DaoDetallePermiso.Guardar(detalle);
        }

        public static EntDetallePermiso Obtener(long idVacaciones)
        {
            return DaoDetallePermiso.Obtener(idVacaciones);
        }

        public static EntSaldoPermisoMensual SaldoMensual(string codUsuario, System.DateTime fecha, long idVacaciones)
        {
            return DaoDetallePermiso.SaldoMensual(codUsuario, fecha, idVacaciones);
        }
    }
}
