using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
   public class NegPoliza
    {
        public static EntRespuesta RTAInsertaNuevaPoliza(EntPoliza objEntPoliza)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoPoliza.RTAInsertaNuevaPoliza(objEntPoliza);

            return respuesta;
        }

        public static EntRespuesta RTAInsertaNuevaPolizas(EntPoliza objEntPoliza)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoPoliza.RTAInsertaNuevaPolizas(objEntPoliza);

            return respuesta;
        }

        public static List<EntPoliza> ConsultaSp_RTAConsultarPoliza(int IdPedido, int tipo)
        {
            return DaoPoliza.ConsultaSp_RTAConsultarPoliza(IdPedido, tipo);
        }

        public static List<EntPoliza> ConsultaSp_RTAListaPoliza(string buscar, string FchIni, string FchFin, int IdPedido)
        {
            return DaoPoliza.ConsultaSp_RTAListaPoliza(buscar, FchIni, FchFin, IdPedido);
        }

        public static List<EntPoliza> ConsultaSp_RTAListaPolizas(string buscar, string FchIni, string FchFin, int IdPedido, string BENEFICIARIO, string Proceso, int idFecha)
        {
            return DaoPoliza.ConsultaSp_RTAListaPolizas(buscar, FchIni, FchFin, IdPedido, BENEFICIARIO, Proceso, idFecha);
        }

        public static EntRespuesta ConsultaSp_RTAListaPolizasDescargar(string buscar, string FchIni, string FchFin, int IdPedido, string BENEFICIARIO, string Proceso, int idFecha)
        {
            return DaoPoliza.ConsultaSp_RTAListaPolizasDescargar(buscar, FchIni, FchFin, IdPedido, BENEFICIARIO, Proceso, idFecha);
        }

    }
}
