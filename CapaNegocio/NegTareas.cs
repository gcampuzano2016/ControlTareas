using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegTareas
    {


        public static List<EntTareas> ListaTareas(string Idusuario)
        {
            return DaoTareas.ListaTareas(Idusuario);
        }

        public static List<EntArchivoTarea> ListaArchivosTareas(Int32 IdTarea, Int32 idServicio)
        {
            return DaoTareas.ListaArchivosTareas(IdTarea, idServicio);
        }

        public static List<EntArchivoTarea> ListaArchivosContrato(Int32 IdTarea)
        {
            return DaoTareas.ListaArchivosContrato(IdTarea);
        }

        public static EntTareas RTA_ConsultaTareaRTA(Int32 IdTarea)
        {
            EntTareas respuesta = new EntTareas();

            respuesta = DaoTareas.RTA_ConsultaTareaRTA(IdTarea);

            return respuesta;
        }

        public static EntTareas RTAConsultaNumeroOrden(string Num_OrdenServicio)
        {
            EntTareas respuesta = new EntTareas();

            respuesta = DaoTareas.RTAConsultaNumeroOrden(Num_OrdenServicio);

            return respuesta;
        }

        public static EntDetalleTarea RTA_ConsultaDetalleTareaRTA(Int32 IdDetalleTarea)
        {
            EntDetalleTarea respuesta = new EntDetalleTarea();

            respuesta = DaoTareas.RTA_ConsultaDetalleTareaRTA(IdDetalleTarea);

            return respuesta;
        }

        public static int RTA_IngresaHisTarea(EntHisTarea objHisTarea)
        {
            var respuesta = 0;
            respuesta = DaoTareas.RTA_IngresaHisTarea(objHisTarea);

            return respuesta;
        }

        public static int RTA_ActualizaHisTarea(EntHisTarea objHisTarea)
        {
            var respuesta = 0;
            respuesta = DaoTareas.RTA_ActualizaHisTarea(objHisTarea);

            return respuesta;
        }

        public static EntRespuesta RTA_ActualizaEstadoTarea(EntTareas objTarea)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_ActualizaEstadoTarea(objTarea);

            return respuesta;
        }

        public static EntRespuesta RTA_ActualizarRequerimiento(EntTareas objTarea)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_ActualizarRequerimiento(objTarea);

            return respuesta;
        }

        public static EntRespuesta RTA_ActualizaDetalleTarea(EntDetalleTarea objDetalleTarea, string IdUsuarioSession, string IpCliente)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_ActualizaDetalleTarea(objDetalleTarea, IdUsuarioSession, IpCliente);

            return respuesta;
        }

 
        public static EntRespuesta RTA_BorrarDetalleTarea(Int32 idRegDetTareas)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_BorrarDetalleTarea(idRegDetTareas);

            return respuesta;
        }


        public static EntRespuesta RTA_CambiarEstadoDetalleTarea(Int32 idRegDetTareas)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_CambiarEstadoDetalleTarea(idRegDetTareas);

            return respuesta;
        }


        public static EntRespuesta BorrarArchivosTarea(Int32 IdArchivo)
        {
            EntRespuesta respuesta = new EntRespuesta(); 

             respuesta = DaoTareas.RTA_BorrarArchivosTarea(IdArchivo);

            return respuesta;
        }

        public static EntRespuesta BorrarArchivosContrato(Int32 IdArchivo)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_BorrarArchivosContrato(IdArchivo);

            return respuesta;
        }

        public static EntRespuesta MaximaOrdenArchivosTarea(Int32 IdTarea, Int32 idServicio)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTAMaximaOrdenArchivosTarea(IdTarea, idServicio);

            return respuesta;
        }


        public static EntRespuesta RTAActualizarEstadoHorasExtras(int Id_RegDetTareas, int Det_HorasExtrasEstado)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTAActualizarEstadoHorasExtras(Id_RegDetTareas, Det_HorasExtrasEstado);

            return respuesta;
        }

        public static EntRespuesta RTA_InsertaDetalleTarea(EntDetalleTarea objDetalleTarea)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_InsertaDetalleTarea(objDetalleTarea);

            return respuesta;
        }

        public static EntRespuesta RTA_InsertaDetalleTareaActualizar(EntDetalleTarea objDetalleTarea)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_InsertaDetalleTareaActualizar(objDetalleTarea);

            return respuesta;
        }


        public static EntRespuesta RTAInsertaArchivoTarea(EntArchivoTarea registro)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTAInsertaArchivoTarea(registro);

            return respuesta;
        }

        public static EntRespuesta RTA_ConsultaCatalogo(Int32 IdCatalogo)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_ConsultaCatalogo(IdCatalogo);

            return respuesta;
        }

        public static int RTA_ActuaProgHisTarea(String IdUsuario)
        {
            var respuesta = 0;
            respuesta = DaoTareas.RTA_ActuaProgHisTarea(IdUsuario);

            return respuesta;
        }

        public static int RTA_CreaTareas(EntTareas objTarea)
        {
            var respuesta = 0;
            respuesta = DaoTareas.RTA_CreaTareas(objTarea);

            return respuesta;

        }


        public static EntRespuesta RTA_CreaTareas2(EntTareas objTarea)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_CreaTareas2(objTarea);

            return respuesta;

        }


        public static List<EntTareas> ListaTareasFechas(string Idusuario, string FchIni, string FchFin, int Tipo)
        {
            return DaoTareas.ListaTareasFechas(Idusuario, FchIni, FchFin, Tipo);
        }

        public static List<EntTareas> ConsultaHorasExtrasPorAutorizar(string FchIni, string FchFin, string IdResponsable, int Det_Horas_Extras_Estado, string IdUsuarioJefe)
        {
            return DaoTareas.ConsultaHorasExtrasPorAutorizar(FchIni, FchFin, IdResponsable, Det_Horas_Extras_Estado, IdUsuarioJefe);
        }

        public static List<EntTareas> ConsultaTareasGeneradas(string FchIni, string FchFin, int Tipo, int Id_RegTareas,string registro, string busqueda)
        {
            return DaoTareas.ConsultaTareasGeneradas(FchIni, FchFin, Tipo, Id_RegTareas, registro, busqueda);
        }

        public static EntRespuesta ConsultaTareasGeneradasDescargar(string FchIni, string FchFin, int Tipo, int Id_RegTareas, string registro, string busqueda)
        {
            return DaoTareas.ConsultaTareasGeneradasDescargar(FchIni, FchFin, Tipo, Id_RegTareas, registro, busqueda);
        }


        public static List<EntTareas> ConsultaTareasGeneradasContrato(string FchIni, string FchFin, int Tipo, int Id_RegTareas, string registro, string numeroOS,int idCliente,int idProceso)
        {
            return DaoTareas.ConsultaTareasGeneradasContrato(FchIni, FchFin, Tipo, Id_RegTareas, registro, numeroOS, idCliente, idProceso);
        }

        public static List<EntTareas> ConsultaHorasContratadas(string FchIni, string FchFin, int Tipo, int Id_RegTareas, string registro, string numeroOS, int idCliente, int idProceso)
        {
            return DaoTareas.ConsultaHorasContratadas(FchIni, FchFin, Tipo, Id_RegTareas, registro, numeroOS, idCliente, idProceso);
        }

        public static EntRespuesta ConsultaTareasGeneradasContratoDescargar(string FchIni, string FchFin, int Tipo, int Id_RegTareas, string registro, string numeroOS,int idCliente, int idProceso)
        {
            return DaoTareas.ConsultaTareasGeneradasContratoDescargar(FchIni, FchFin, Tipo, Id_RegTareas, registro, numeroOS, idCliente, idProceso);
        }


        public static EntRespuesta ObtenerReporteGPFGeneradasDescargar(string FchIni, string FchFin,int idtipo)
        {
            return DaoTareas.ObtenerReporteGPFGeneradasDescargar(FchIni, FchFin, idtipo);
        }
        public static EntRespuesta ConsultaHorasExtrasPorAutorizarDescargar(string FchIni, string FchFin, string IdResponsable, int Det_Horas_Extras_Estado, string IdUsuarioJefe)
        {
            return DaoTareas.ConsultaHorasExtrasPorAutorizarDescargar(FchIni, FchFin, IdResponsable, Det_Horas_Extras_Estado, IdUsuarioJefe);
        }

   
        public static List<EntDetTarea> ListaDetTareas(string Idusuario, string FchIni, string FchFin, string Det_Nom_Empresa, string Det_Num_OrdenServicio, int IdTipoGasto,int idTareaAprobada,int idTareaAprobadaQA,int idFecha, int Tipo)
        {
            return DaoTareas.ListaDetTareas(Idusuario, FchIni, FchFin, Det_Nom_Empresa, Det_Num_OrdenServicio, IdTipoGasto, idTareaAprobada, idTareaAprobadaQA, idFecha, Tipo);
        }

        public static List<EntDetTarea> ListaDetTareasRechazadas(string Idusuario, string FchIni, string FchFin, string Det_Nom_Empresa, string Det_Num_OrdenServicio, int IdTipoGasto, int idTareaAprobada, int idTareaAprobadaQA, int idFecha, int Tipo)
        {
            return DaoTareas.ListaDetTareasRechazadas(Idusuario, FchIni, FchFin, Det_Nom_Empresa, Det_Num_OrdenServicio, IdTipoGasto, idTareaAprobada, idTareaAprobadaQA, idFecha, Tipo);
        }

        public static EntRespuesta ListaHorasRecursosPorJefatura(string Idusuario, string FchIni, string FchFin, int Tipo, int estado)
        {
            return DaoTareas.ListaHorasRecursosPorJefatura(Idusuario, FchIni, FchFin, Tipo,estado);
        }

        public static EntRespuesta ListaDetalleTareasDescarga(string Idusuario, string FchIni, string FchFin, string Det_Nom_Empresa, string Det_Num_OrdenServicio, int IdTipoGasto, int idTareaAprobada, int idTareaAprobadaQA, int idFecha, int Tipo)
        {
            return DaoTareas.ListaDetalleTareasDescarga(Idusuario, FchIni, FchFin, Det_Nom_Empresa, Det_Num_OrdenServicio, IdTipoGasto, idTareaAprobada, idTareaAprobadaQA, idFecha, Tipo);
        }


        public static List<EntKpi> ListaKpiTareas(string Idusuario, string FchIni, string FchFin, int Tipo)
        {
            return DaoTareas.ListaKpiTareas(Idusuario, FchIni, FchFin, Tipo);
        }

        public static List<EntDetTarea> Sp_RTAConsultaUsuarioPorJefe(string Idusuario)
        {
            return DaoTareas.Sp_RTAConsultaUsuarioPorJefe(Idusuario);
        }

        public static int RTA_CreaTareasHorasExtras(Int32 Id_RegTareas)
        {
            var respuesta = 0;
            respuesta = DaoTareas.RTA_CreaTareasHorasExtras(Id_RegTareas);

            return respuesta;
        }

        
        public static EntRespuesta RTA_CambioEstadoAprobacionTareaIndividual(string IdUsuarioJefe, string fechaDesde, string fechaHasta, string idUsuarioResponsable, string IdUsuarioSession, string ipCliente, Int32 estadoAprobacion, Int32 idDetalleTarea)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_CambioEstadoAprobacionTareaIndividual(IdUsuarioJefe, fechaDesde, fechaHasta, idUsuarioResponsable, IdUsuarioSession, ipCliente, estadoAprobacion, idDetalleTarea);

            return respuesta;
        }

        public static EntRespuesta RTA_CambioEstadoAprobacionTarea(string IdUsuarioJefe, string fechaDesde, string fechaHasta, string idUsuarioResponsable, string IdUsuarioSession, string ipCliente, Int32 estadoAprobacion)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_CambioEstadoAprobacionTarea(IdUsuarioJefe, fechaDesde, fechaHasta, idUsuarioResponsable, IdUsuarioSession, ipCliente, estadoAprobacion);

            return respuesta;
        }
        public static EntRespuesta RTA_CambioEstadoAprobacionTareaRevisor(string IdUsuarioJefe, string fechaDesde, string fechaHasta, string idUsuarioResponsable, string IdUsuarioSession, string ipCliente, Int32 estadoAprobacion)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoTareas.RTA_CambioEstadoAprobacionTareaRevisor(IdUsuarioJefe, fechaDesde, fechaHasta, idUsuarioResponsable, IdUsuarioSession, ipCliente, estadoAprobacion);

            return respuesta;
        }

        public static List<EntTareas> ListaTareasHorasExtras(string Idusuario)
        {
            return DaoTareas.ListaTareasHorasExtras(Idusuario);
        }

        public static int RTAActuaAprobacionHorasExtras(EntTareas objEntTareas)
        {
            return DaoTareas.RTAActuaAprobacionHorasExtras(objEntTareas);
        }

        public static List<EntTareas> RTAConsultaTareaHorasExtras()
        {
            return DaoTareas.RTAConsultaTareaHorasExtras();
        }

        public static EntTareas RTA_ConsultaTareaHorasExtrasRTA(Int32 IdTarea)
        {
            EntTareas respuesta = new EntTareas();
            respuesta = DaoTareas.RTA_ConsultaTareaHorasExtrasRTA(IdTarea);

            return respuesta;
        }

        public static int RTAActualizaHisTareaHorasExtras(EntHisTarea objHisTarea)
        {
            var respuesta = 0;
            respuesta = DaoTareas.RTAActualizaHisTareaHorasExtras(objHisTarea);

            return respuesta;
        }

        public static List<EntDetTarea> ListaDetTareasHorasExtras(string Idusuario, string FchIni, string FchFin, int Tipo)
        {
            return DaoTareas.ListaDetTareasHorasExtras(Idusuario, FchIni, FchFin, Tipo);
        }

        public static DataSet RTAConsultaCatalogo( Int32 IdEstado)
        {
            return DaoTareas.RTAConsultaCatalogo(IdEstado);
        }

        public static DataSet RTAListaCatalogoSecuencia(Int32 IdEstado)
        {
            return DaoTareas.RTAListaCatalogoSecuencia(IdEstado);
        }

        public static List<EntCombo> ListaEstadosCombo(Int32 idEstado, Int32 IdProyecto)
        {
            return DaoTareas.RTAListaEstadosCombo(idEstado, IdProyecto);
        }

        
        public static List<EntCombo> ListaCatalogoCombo(Int32 idTipoCatalogo)
        {
            return DaoTareas.RTAListaCatalogoCombo(idTipoCatalogo);
        }

        public static List<EntCombo> ListaCatalogoTodosCombo(Int32 idTipoCatalogo)
        {
            return DaoTareas.RTAListaCatalogoTodosCombo(idTipoCatalogo);
        }

        public static List<EntCombo> ListaCatalogoFechaCombo(Int32 idTipoCatalogo)
        {
            return DaoTareas.RTAListaCatalogoFechaCombo(idTipoCatalogo);
        }

        public static List<EntCombo> RTAListaCatalogoAprobacionCombo(Int32 idTipoCatalogo)
        {
            return DaoTareas.RTAListaCatalogoAprobacionCombo(idTipoCatalogo);
        }

        public static DataSet RTAConsultaCatalogoPorPadre(Int32 IdPadre, Int32 IdItem)
        {
            return DaoTareas.RTAConsultaCatalogoPorPadre(IdPadre, IdItem);
        }



    }
}
