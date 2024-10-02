using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegMantenimiento
    {

        public static EntRespuesta RTA_ConsultaReporteServiciosDescarga(int Anio, String Gestor, String Sucursal, String Estado, String Area, String GerenteCuenta)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoMantenimiento.RTA_ConsultaReporteServiciosDescarga(Anio, Gestor, Sucursal, Estado, Area, GerenteCuenta);

            return respuesta;
        }

        public static EntRespuesta Sp_RTA_ConsultarOrdenServicioDescarga(int Tipo, String ORDEN)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoMantenimiento.Sp_RTA_ConsultarOrdenServicioDescarga(Tipo, ORDEN);

            return respuesta;
        }
        public static EntRespuesta RTA_InsertaNuevoMantenimiento(EntMantenimiento objEntMantenimiento)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoMantenimiento.RTA_InsertaNuevoMantenimiento(objEntMantenimiento);

            return respuesta;
        }

        public static List<EntMantenimiento> ConsultSp_RTAConsultaMantenimientoContrato(string FchIni, string FchFin, string orden, int tipo, int IdRequerimiento)
        {
            return DaoMantenimiento.ConsultSp_RTAConsultaMantenimientoContrato(FchIni, FchFin, orden, tipo, IdRequerimiento);
        }

        public static List<EntMantenimiento> ConsultSp_RTAListaMantenimientoContrato(string FchIni, string FchFin, string orden, int IdCliente, string Clasificacion, int tipo)
        {
            return DaoMantenimiento.ConsultSp_RTAListaMantenimientoContrato(FchIni, FchFin, orden, IdCliente, Clasificacion, tipo);
        }

        public static EntRespuesta ConsultSp_RTAListaMantenimientoContratoDescargar(string FchIni, string FchFin, string orden, int IdCliente, string Clasificacion, int tipo)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoMantenimiento.ConsultSp_RTAListaMantenimientoContratoDescargar(FchIni, FchFin, orden, IdCliente, Clasificacion, tipo);

            return respuesta;
        }


        //METODO 1

        public static List<EntMantenimiento> Sp_RTA_ConsultarOrdenServicio(int Tipo, String ORDEN)
        {
            return DaoMantenimiento.Sp_RTA_ConsultarOrdenServicio(Tipo, ORDEN);
        }

        //METODO 2
        public static List<EntCombo> RTAListaComboContrato(Int32 Tipo, string Cod_Usuario, Int32 IdSucursal, Int32 IdCliente, string IdSucursalGerente)
        {
            return DaoMantenimiento.RTAListaComboContrato(Tipo, Cod_Usuario, IdSucursal, IdCliente, IdSucursalGerente);
        }

        //METODO 3

        public static List<EntMantenimiento> RTA_ConsultaReporteServicios(int Anio, String Gestor, String Sucursal, String Estado, String Area)
        {
            return DaoMantenimiento.RTA_ConsultaReporteServicios(Anio, Gestor, Sucursal, Estado, Area);
        }


    }
}
