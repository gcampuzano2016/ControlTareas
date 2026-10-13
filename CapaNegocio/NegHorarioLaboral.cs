using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegHorarioLaboral
    {
        public static List<EntHorarioLaboral> ListarHorarios(string filtro, bool incluirInactivos, bool incluirPropios)
        {
            return DaoHorarioLaboral.ListarHorarios(filtro, incluirInactivos, incluirPropios);
        }

        public static List<EntHorarioLaboralDetalle> ObtenerHorario(int idHorarioLaboral)
        {
            return DaoHorarioLaboral.ObtenerHorario(idHorarioLaboral);
        }

        public static EntRespuesta GuardarHorario(EntHorarioLaboral horario,
                                                  List<EntHorarioLaboralDetalle> dias,
                                                  string usuarioRegistro,
                                                  bool confirmaSobrescribir)
        {
            return DaoHorarioLaboral.GuardarHorario(horario, dias, usuarioRegistro, confirmaSobrescribir);
        }

        public static EntRespuesta CambiarEstadoHorario(int idHorarioLaboral, bool activo, string usuarioRegistro)
        {
            return DaoHorarioLaboral.CambiarEstadoHorario(idHorarioLaboral, activo, usuarioRegistro);
        }

        public static EntRespuesta GuardarHorarioPropio(string idResponsable,
                                                        List<EntHorarioLaboralDetalle> dias,
                                                        string fechaDesde,
                                                        string usuarioRegistro)
        {
            return DaoHorarioLaboral.GuardarHorarioPropio(idResponsable, dias, fechaDesde, usuarioRegistro);
        }
    }
}
