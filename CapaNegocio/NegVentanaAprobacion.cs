using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegVentanaAprobacion
    {
        public static List<EntVentanaAprobacionJefe> ListarJefes(string filtro)
        {
            return DaoVentanaAprobacion.ListarJefes(filtro);
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
