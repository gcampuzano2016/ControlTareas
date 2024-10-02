using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegContrato
    {

        public static EntRespuesta ConsultaSp_RTAListaContratosDescargar(string FchIni, string FchFin, string busqueda, int IdCliente, int IdGerenteCuenta, int IdGestorResponsable, string sucursal, string estado, string area)
        {
            return DaoContrato.ConsultaSp_RTAListaContratosDescargar(FchIni, FchFin, busqueda, IdCliente, IdGerenteCuenta, IdGestorResponsable, sucursal, estado,area);
        }
        public static List<EntContrato> ConsultaSp_RTAListaContratos(string FchIni, string FchFin, string busqueda, int IdCliente, int IdGerenteCuenta, int IdGestorResponsable, string sucursal, string estado, string area,int IdClasificacion)
        {
            return DaoContrato.ConsultaSp_RTAListaContratos(FchIni, FchFin, busqueda, IdCliente, IdGerenteCuenta, IdGestorResponsable, sucursal, estado,area, IdClasificacion);
        }
        public static List<EntContrato> ConsultaSp_RTAConsultarContratos(int IdServicio, string orden, int tipo)
        {
            return DaoContrato.ConsultaSp_RTAConsultarContratos(IdServicio, orden, tipo);
        }
        public static EntRespuesta RTA_InsertaNuevoContrato(EntContrato objEntContrato)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoContrato.RTA_InsertaNuevoContrato(objEntContrato);

            return respuesta;
        }

        public static EntRespuesta RTA_ActualizarNuevoContrato(EntContrato objEntContrato)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoContrato.RTA_ActualizarNuevoContrato(objEntContrato);

            return respuesta;
        }

        public static EntRespuesta RTA_EliminarNuevoContrato(EntContrato objEntContrato)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoContrato.RTA_EliminarNuevoContrato(objEntContrato);

            return respuesta;
        }
        public static List<EntCombo> RTAListaComboContrato(Int32 Tipo, string Cod_Usuario, Int32 IdSucursal,Int32 IdCliente, string IdSucursalGerente)
        {
            return DaoContrato.RTAListaComboContrato(Tipo, Cod_Usuario, IdSucursal, IdCliente, IdSucursalGerente);
        }
    }
}