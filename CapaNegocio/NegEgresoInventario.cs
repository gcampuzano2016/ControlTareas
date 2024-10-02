using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegEgresoInventario
    {
        public static EntRespuesta RTAInsertaNuevoEgresoInventario(EntEgresoInventario objEgresoInventario)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoEgresoInventario.RTAInsertaNuevoEgresoInventario(objEgresoInventario);
            return respuesta;
        }

        public static List<EntEgresoInventario> ConsultaRTAListaEgresos(int IdCliente, string FchIni, string FchFin)
        {
            return DaoEgresoInventario.ConsultaRTAListaEgresos(IdCliente, FchIni, FchFin);
        }

        public static EntRespuesta ConsultaRTAListaEgresosDescargar(int IdCliente, string FchIni, string FchFin)
        {
            return DaoEgresoInventario.ConsultaRTAListaEgresosDescargar(IdCliente, FchIni, FchFin);
        }

        public static List<EntEgresoInventario> RTA_ConsultaClientePedidoLike(int tipo, string Descripcion)
        {
            return DaoEgresoInventario.RTA_ConsultaClientePedidoLike(tipo, Descripcion);
        }

        public static List<EntClienteEgrInventario> ConsultaRTAListaClienteInventario(int IdEncabezado)
        {
            return DaoEgresoInventario.ConsultaRTAListaClienteInventario(IdEncabezado);
        }

        public static List<EntDetalleInventario> ConsultaRTAListaDetallesInv(int IdEncabezado)
        {
            return DaoEgresoInventario.ConsultaRTAListaDetallesInv(IdEncabezado);
        }
    }
}
