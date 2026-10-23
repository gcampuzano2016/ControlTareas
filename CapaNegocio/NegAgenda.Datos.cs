using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Unico archivo de la agenda que toca la base. Las reglas (NegAgendaEquipo,
    /// NegAgendaActividad, NegAgendaVista, NegAgendaCorreo, NegAgendaAreas) son
    /// puras. Todo lanza si la base falla: quien llama decide que responder.
    /// </summary>
    public static partial class NegAgenda
    {
        public static List<EntAgendaPersona> PersonasActivas()
        {
            return DaoAgenda.PersonasActivas();
        }

        public static List<EntAgendaArea> Areas()
        {
            return DaoAgenda.Areas();
        }

        public static int GuardarArea(int idArea, string nombre, string color, bool activa, string usuario)
        {
            return DaoAgenda.GuardarArea(idArea, nombre, color, activa, usuario);
        }

        public static void AsignarArea(string codUsuario, int idArea, string usuario)
        {
            DaoAgenda.AsignarArea(codUsuario, idArea, usuario);
        }

        public static List<EntAgendaActividad> Actividades(DateTime desde, DateTime hasta, DateTime hoy)
        {
            return DaoAgenda.Actividades(desde, hasta, hoy);
        }

        public static EntAgendaActividad Actividad(int idActividad)
        {
            return DaoAgenda.Actividad(idActividad);
        }

        public static int GuardarActividad(int idActividad, string titulo, string codResponsable, string codAsignadoPor,
            DateTime fechaEntrega, IList<EntAgendaEntregable> entregables, string usuario)
        {
            return DaoAgenda.GuardarActividad(idActividad, titulo, codResponsable, codAsignadoPor, fechaEntrega, entregables, usuario);
        }

        public static EntAgendaAvance MarcarEntregable(int idEntregable, bool hecho, string usuario, DateTime? fechaSiguiente = null)
        {
            return DaoAgenda.MarcarEntregable(idEntregable, hecho, usuario, fechaSiguiente);
        }

        public static EntAgendaAvance MarcarCompleta(int idActividad, bool completa, string usuario, DateTime? fechaSiguiente = null)
        {
            return DaoAgenda.MarcarCompleta(idActividad, completa, usuario, fechaSiguiente);
        }

        public static void Anular(int idActividad, string usuario)
        {
            DaoAgenda.Anular(idActividad, usuario);
        }

        public static void RegistrarCorreo(int? idActividad, string destinatario, string asunto, bool enviado, string error, int? idProceso = null)
        {
            DaoAgenda.RegistrarCorreo(idActividad, destinatario, asunto, enviado, error, idProceso);
        }

        public static HashSet<DateTime> Feriados()
        {
            return DaoAgenda.Feriados();
        }

        public static List<EntAgendaPlantilla> Plantillas()
        {
            return DaoAgenda.Plantillas();
        }

        public static int GuardarPlantilla(EntAgendaPlantilla t, string usuario)
        {
            return DaoAgenda.GuardarPlantilla(t, usuario);
        }

        public static List<EntAgendaProceso> Procesos()
        {
            return DaoAgenda.Procesos();
        }

        public static int IniciarProceso(string nombre, int idPlantilla, IList<EntAgendaProcesoPaso> pasos,
            DateTime fechaPrimero, string usuario, out int? idPasoActivado)
        {
            return DaoAgenda.IniciarProceso(nombre, idPlantilla, pasos, fechaPrimero, usuario, out idPasoActivado);
        }

        public static int ReasignarPaso(int idPaso, string estadoEsperado, string codResponsable, string usuario)
        {
            return DaoAgenda.ReasignarPaso(idPaso, estadoEsperado, codResponsable, usuario);
        }

        public static int CambiarPasoPendiente(EntAgendaProcesoPaso paso, string usuario)
        {
            return DaoAgenda.CambiarPasoPendiente(paso, usuario);
        }

        public static int CambiarFechaPaso(int idPaso, DateTime fecha, string usuario)
        {
            return DaoAgenda.CambiarFechaPaso(idPaso, fecha, usuario);
        }

        public static int CancelarProceso(int idProceso, string motivo, string usuario)
        {
            return DaoAgenda.CancelarProceso(idProceso, motivo, usuario);
        }
    }
}
