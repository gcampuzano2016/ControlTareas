using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;


namespace CapaNegocio
{
    public class NegCosteoServicio
    {

        public static EntRespuesta RTAInsertaNuevoCosteo(EntCosteoServicio objEntCosteoServicio)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoCosteoServicio.RTAInsertaNuevoCosteo(objEntCosteoServicio);
            return respuesta;
        }

        public static List<EntCosteoServicio> ConsultaSp_RTAListaCosteServicios(string FchIni, string FchFin, int IdGerenteCuenta, string sucursal, string ResponsableDimen, int idFecha)
        {
            return DaoCosteoServicio.ConsultaSp_RTAListaCosteServicios(FchIni, FchFin, IdGerenteCuenta, sucursal, ResponsableDimen, idFecha);
        }

        public static EntRespuesta ConsultaSp_RTAListaCosteServiciosDescargar(string FchIni, string FchFin, int IdGerenteCuenta, string sucursal, string ResponsableDimen, int idFecha)
        {
            return DaoCosteoServicio.ConsultaSp_RTAListaCosteServiciosDescargar(FchIni, FchFin, IdGerenteCuenta, sucursal, ResponsableDimen, idFecha);
        }

        public static List<EntCosteoServicio> ConsultaSp_RTAConsultarCosteServicios(int IdForeCast, int tipo)
        {
            return DaoCosteoServicio.ConsultaSp_RTAConsultarCosteServicios(IdForeCast, tipo);
        }

    }
}
