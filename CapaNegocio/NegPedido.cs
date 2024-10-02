using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegPedido
    {

        public static EntRespuesta RTA_ActualizarFechaPedido(EntPedido objEntPedido)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoPedido.RTA_ActualizarFechaPedido(objEntPedido);

            return respuesta;
        }
        public static EntRespuesta RTA_InsertaNuevoPedido(EntPedido objEntPedido)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoPedido.RTA_InsertaNuevoPedido(objEntPedido);

            return respuesta;
        }

        public static List<EntPedido> ConsultaSp_RTAListaPedido(string FchIni, string FchFin, string busqueda, int IdCliente, int IdGerenteCuenta, int IdGestorProducto, string sucursal, string estado, int IdClasificacion, int Anio, string meses,int idFecha,int IdRenovacion)
        {
            return DaoPedido.ConsultaSp_RTAListaPedido(FchIni, FchFin, busqueda, IdCliente, IdGerenteCuenta, IdGestorProducto, sucursal, estado, IdClasificacion, Anio, meses, idFecha, IdRenovacion);
        }

        public static EntRespuesta ConsultaSp_RTAListaPedidoDescargar(string FchIni, string FchFin, string busqueda, int IdCliente, int IdGerenteCuenta, int IdGestorProducto, string sucursal, string estado, int IdClasificacion, int Anio, string meses,int idFecha,int IdRenovacion)
        {
            return DaoPedido.ConsultaSp_RTAListaPedidoDescargar(FchIni, FchFin, busqueda, IdCliente, IdGerenteCuenta, IdGestorProducto, sucursal, estado, IdClasificacion, Anio, meses, idFecha, IdRenovacion);
        }

        public static List<EntPedido> ConsultaSp_RTAConsultarPedido(int IdPedido, string orden, int tipo)
        {
            return DaoPedido.ConsultaSp_RTAConsultarPedido(IdPedido, orden, tipo);
        }

        public static EntRespuesta RTA_InsertaNuevoClientes(EntPedido objEntPedido)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoPedido.RTA_InsertaNuevoClientes(objEntPedido);

            return respuesta;
        }

        public static EntRespuesta RTA_InsertaNuevoClientesGD(EntPedido objEntPedido)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoPedido.RTA_InsertaNuevoClientesGD(objEntPedido);

            return respuesta;
        }

    }
}
