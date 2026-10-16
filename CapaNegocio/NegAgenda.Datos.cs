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

        public static void MarcarEntregable(int idEntregable, bool hecho, string usuario)
        {
            DaoAgenda.MarcarEntregable(idEntregable, hecho, usuario);
        }

        public static void MarcarCompleta(int idActividad, bool completa, string usuario)
        {
            DaoAgenda.MarcarCompleta(idActividad, completa, usuario);
        }

        public static void Anular(int idActividad, string usuario)
        {
            DaoAgenda.Anular(idActividad, usuario);
        }

        public static void RegistrarCorreo(int idActividad, string destinatario, string asunto, bool enviado, string error)
        {
            DaoAgenda.RegistrarCorreo(idActividad, destinatario, asunto, enviado, error);
        }
    }
}
