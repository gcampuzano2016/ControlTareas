using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegUsuarioHorario
    {
        public static List<EntCombo> ListarPerfiles()
        {
            return DaoUsuarioHorario.ListarPerfiles();
        }

        public static List<EntUsuarioHorario> ListarUsuarios(string filtro)
        {
            return DaoUsuarioHorario.ListarUsuarios(filtro);
        }

        public static EntRespuesta AsignarHorario(string idResponsable, int idHorarioLaboral, string fechaDesde, string usuarioRegistro)
        {
            return DaoUsuarioHorario.AsignarHorario(idResponsable, idHorarioLaboral, fechaDesde, usuarioRegistro);
        }
    }
}
