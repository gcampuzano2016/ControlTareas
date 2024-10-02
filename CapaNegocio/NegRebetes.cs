using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegRebates
    {

        public static EntRespuesta RTA_InsertaNuevoRebates(EntRebates objEntRebates)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoRebates.RTA_InsertaNuevoRebates(objEntRebates);

            return respuesta;
        }

        public static List<EntRebates> ConsultaSp_RTAListaRebetes(string FchIni, string FchFin, int IdTipoIngreso, int IdMarca, int IdPago, string estado, string  Anio, string meses, int idFecha)
        {
            return DaoRebates.ConsultaSp_RTAListaRebetes(FchIni, FchFin, IdTipoIngreso, IdMarca, IdPago, estado, Anio, meses, idFecha);
        }

        public static EntRespuesta ConsultaSp_RTAListaRebetesDescargar(string FchIni, string FchFin, int IdTipoIngreso, int IdMarca, int IdPago, string estado, string Anio, string meses, int idFecha)
        {
            return DaoRebates.ConsultaSp_RTAListaRebetesDescargar(FchIni, FchFin, IdTipoIngreso, IdMarca, IdPago, estado, Anio, meses, idFecha);
        }
        public static List<EntRebates> ConsultaSp_RTAConsultarPeriodoFiscal(int IdMarca, string FchIni, string FchFin,int IdAnioFiscal, int tipo)
        {
            return DaoRebates.ConsultaSp_RTAConsultarPeriodoFiscal(IdMarca, FchIni, FchFin, IdAnioFiscal, tipo);
        }

        public static List<EntRebates> ConsultaSp_RTAConsultarRebetes(int IdRebates, string orden, int tipo)
        {
            return DaoRebates.ConsultaSp_RTAConsultarRebetes(IdRebates, orden, tipo);
        }

        //BANCOS
        public static EntRespuesta RTA_InsertaNuevoBanco(EntRebates objEntRebates)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoRebates.RTA_InsertaNuevoBanco(objEntRebates);

            return respuesta;
        }

        //BANCOS

        //PERIODO FISCAL
        public static EntRespuesta RTA_InsertaNuevoPeriodoFiscal(EntRebates objEntRebates)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoRebates.RTA_InsertaNuevoPeriodoFiscal(objEntRebates);

            return respuesta;
        }
        //PERIODO FISCAL

    }
}
