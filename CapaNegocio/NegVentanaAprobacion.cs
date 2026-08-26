using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegVentanaAprobacion
    {
        public static List<EntVentanaAprobacionJefe> ListarJefes(string filtro, bool incluirInactivos)
        {
            return DaoVentanaAprobacion.ListarJefes(filtro, incluirInactivos);
        }

        public static EntRespuesta ExcluirJefe(string mailJefe, bool excluir, string usuarioRegistro)
        {
            return DaoVentanaAprobacion.ExcluirJefe(mailJefe, excluir, usuarioRegistro);
        }

        public static EntRespuesta GuardarVentana(string mailJefe, string fechaDesde, string fechaHasta, string usuarioRegistro)
        {
            return DaoVentanaAprobacion.GuardarVentana(mailJefe, fechaDesde, fechaHasta, usuarioRegistro);
        }

        public static EntRespuesta ValidarVentana(string mailJefe, DateTime fecha)
        {
            return DaoVentanaAprobacion.ValidarVentana(mailJefe, fecha);
        }
    }
}
