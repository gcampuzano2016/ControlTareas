using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegForeCast
    {
        public static EntRespuesta RTAInsertaNuevaForeCast(EntForeCast objEntForeCast)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoForeCast.RTAInsertaNuevaForeCast(objEntForeCast);
            return respuesta;
        }

        public static EntRespuesta RTAAgregarPrioriodadForeCast(EntForeCast objEntForeCast)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoForeCast.RTAAgregarPrioriodadForeCast(objEntForeCast);
            return respuesta;
        }

        public static EntRespuesta RTAInsertaNuevaForeCastGD(EntForeCast objEntForeCast)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoForeCast.RTAInsertaNuevaForeCastGD(objEntForeCast);
            return respuesta;
        }

        public static List<EntForeCast> ConsultaSp_RTAListaForeCast(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, string IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio,string cierrenegocio, int IdPrioProyecto,int ProyecEstrategico, string TipoProyecto)
        {
            return DaoForeCast.ConsultaSp_RTAListaForeCast(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, IdMarca, sucursal, SegmentodeMercado, StrIdPrioridad, Idusuario, idFecha, Anio, cierrenegocio, IdPrioProyecto, ProyecEstrategico, TipoProyecto);
        }

        public static List<EntForeCast> ConsultaSp_RTAListaForeCastGD(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, int IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio, string cierrenegocio)
        {
            return DaoForeCast.ConsultaSp_RTAListaForeCastGD(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, IdMarca, sucursal, SegmentodeMercado, StrIdPrioridad, Idusuario, idFecha, Anio, cierrenegocio);
        }

        public static EntRespuesta ConsultaSp_RTAListaForeCastDescargar(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, string IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio, string cierrenegocio, int IdPrioProyecto, int ProyecEstrategico,string TipoProyecto)
        {
            return DaoForeCast.ConsultaSp_RTAListaForeCastDescargar(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, IdMarca, sucursal, SegmentodeMercado, StrIdPrioridad, Idusuario, idFecha, Anio,cierrenegocio, IdPrioProyecto, ProyecEstrategico, TipoProyecto);
        }

        public static EntRespuesta ConsultaSp_RTAListaForeCastDescargarPersonalizado(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, string IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio, string cierrenegocio, int IdPrioProyecto, int ProyecEstrategico)
        {
            return DaoForeCast.ConsultaSp_RTAListaForeCastDescargarPersonalizado(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, IdMarca, sucursal, SegmentodeMercado, StrIdPrioridad, Idusuario, idFecha, Anio, cierrenegocio, IdPrioProyecto, ProyecEstrategico);
        }

        public static EntRespuesta ConsultaSp_RTAListaForeCastDescargarGerencia(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, string IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio, string cierrenegocio, int IdPrioProyecto, int ProyecEstrategico)
        {
            return DaoForeCast.ConsultaSp_RTAListaForeCastDescargarGerencia(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, IdMarca, sucursal, SegmentodeMercado, StrIdPrioridad, Idusuario, idFecha, Anio, cierrenegocio, IdPrioProyecto, ProyecEstrategico);
        }

        public static EntRespuesta ConsultaSp_RTAListaForeCastGDDescargar(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, int IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio, string cierrenegocio)
        {
            return DaoForeCast.ConsultaSp_RTAListaForeCastGDDescargar(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, IdMarca, sucursal, SegmentodeMercado, StrIdPrioridad, Idusuario, idFecha, Anio, cierrenegocio);
        }

        public static List<EntForeCast> ConsultaSp_RTAConsultarForeCast(int IdForeCast, int tipo)
        {
            return DaoForeCast.ConsultaSp_RTAConsultarForeCast(IdForeCast, tipo);
        }

        public static List<EntForeCast> ConsultaSp_RTAConsultarForeCastGD(int IdForeCast, int tipo)
        {
            return DaoForeCast.ConsultaSp_RTAConsultarForeCastGD(IdForeCast, tipo);
        }

        public static List<EntForeCast> ConsultaSp_RTAValidarClienteForeCast(string Descripcion, int tipo)
        {
            return DaoForeCast.ConsultaSp_RTAValidarClienteForeCast(Descripcion, tipo);
        }

    }
}
