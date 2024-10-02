using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegMensajeForeCast
    {
        public static EntRespuesta RTAInsertaNuevaMensajeForeCast(EntMensajeForeCast objEntForeCast)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoMensajeForeCast.RTAInsertaNuevaMensajeForeCast(objEntForeCast);
            return respuesta;
        }

        public static List<EntMensajeForeCast> ConsultaSp_RTAMostrarMensajeForeCast(int IdForeCast)
        {
            return DaoMensajeForeCast.ConsultaSp_RTAMostrarMensajeForeCast(IdForeCast);
        }

    }
}
