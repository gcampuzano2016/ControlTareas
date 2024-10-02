using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegSolicitud
    {

        public static EntRespuesta RTA_ActualizarSolicitud(EntSolicitud objEntSolicitud)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoSolicitud.RTA_ActualizarSolicitud(objEntSolicitud);

            return respuesta;
        }
        public static EntRespuesta RTA_InsertaNuevaSolicitud(EntSolicitud objEntSolicitud)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoSolicitud.RTA_InsertaNuevaSolicitud(objEntSolicitud);

            return respuesta;
        }
        public static List<EntSolicitud> ConsultaSp_RTAListaSolicitud(int Tipo, string Cod_Jefe_Inm, int Pagina, string EstadoSolicitud, string FchIni, string FchFin, int TipoSolicitud,string usuario)
        {
            return DaoSolicitud.ConsultaSp_RTAListaSolicitud( Tipo,  Cod_Jefe_Inm,  Pagina,  EstadoSolicitud, FchIni, FchFin, TipoSolicitud, usuario);
        }

        public static EntRespuesta ConsultaSp_RTAListaSolicitudAct(int Tipo, string Cod_Jefe_Inm, int Pagina, string EstadoSolicitud, string FchIni, string FchFin, int TipoSolicitud)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoSolicitud.ConsultaSp_RTAListaSolicitudAct(Tipo, Cod_Jefe_Inm, Pagina, EstadoSolicitud, FchIni, FchFin, TipoSolicitud);

            return respuesta;
        }

        public static EntSolicitud ConsultaSp_RTANotificarSolicitud(int Tipo, int IdVacaciones)
        {
            return DaoSolicitud.ConsultaSp_RTANotificarSolicitud(Tipo, IdVacaciones);
        }
        public static EntRespuesta ConsultaSp_RTAListaSolicitudDescargar(int Tipo, string Cod_Jefe_Inm, int Pagina, string EstadoSolicitud, string FchIni, string FchFin, int TipoSolicitud)
        {
            return DaoSolicitud.ConsultaSp_RTAListaSolicitudDescargar(Tipo, Cod_Jefe_Inm, Pagina, EstadoSolicitud, FchIni, FchFin, TipoSolicitud);
        }

        public static int RTA_ActualizarRutaRide(EntSolicitud objEntSolicitud)
        {
            var respuesta = 0;

            respuesta = DaoSolicitud.RTA_ActualizarRutaRide(objEntSolicitud);

            return respuesta;

        }

        public static List<EntArchivoTarea> ListaArchivosSolicitudes(Int32 IdTarea)
        {
            return DaoSolicitud.ListaArchivosSolicitudes(IdTarea);
        }

    }
}
