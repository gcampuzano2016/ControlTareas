using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Reglas de una actividad de la agenda. Clase pura.
    ///
    /// Estado (se calcula, no se guarda):
    /// - Completa: con entregables, todos hechos; sin entregables, la marca manual.
    /// - Atrasada: no completa, no anulada y FechaEntrega anterior a hoy. Vencer
    ///   hoy no es estar atrasada. "hoy" lo pone el servidor.
    ///
    /// Permisos:
    /// - Editar (titulo, fecha, entregables) y anular: hay que ver al
    ///   responsable, y ademas ser quien la asigno o poder asignarle hoy. Para
    ///   una actividad propia, quien la asigno es el mismo responsable. Por eso
    ///   el responsable NO mueve la fecha de lo que le asigno otro.
    /// - Marcar entregables y "completa": solo el responsable.
    /// - Nada de esto sobre una anulada.
    /// </summary>
    public static class NegAgendaActividad
    {
        public const int LargoTexto = 200;
        public const int MaxEntregables = 20;

        private static bool Igual(string a, string b)
        {
            return NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(a), NegAgendaEquipo.Normalizar(b));
        }

        public static bool EsPropia(EntAgendaActividad a)
        {
            return a != null && Igual(a.CodAsignadoPor, a.CodResponsable);
        }

        public static bool Completa(EntAgendaActividad a)
        {
            if (a == null) { return false; }
            if (a.Entregables != null && a.Entregables.Count > 0) { return a.Entregables.All(e => e.Hecho); }
            return a.CompletadaManual;
        }

        public static bool Atrasada(EntAgendaActividad a, DateTime hoy)
        {
            return a != null && !a.Anulada && !Completa(a) && a.FechaEntrega.Date < hoy.Date;
        }

        public static bool PuedeEditar(string yo, EntAgendaActividad a, ISet<string> cadena, bool verTodos)
        {
            if (a == null || a.Anulada) { return false; }
            if (!NegAgendaEquipo.PuedeVer(yo, a.CodResponsable, cadena, verTodos)) { return false; }
            return Igual(yo, a.CodAsignadoPor)
                || NegAgendaEquipo.PuedeAsignarA(yo, a.CodResponsable, cadena, verTodos);
        }

        public static bool PuedeMarcar(string yo, EntAgendaActividad a)
        {
            return a != null && !a.Anulada && Igual(yo, a.CodResponsable);
        }

        public static bool EsDiaHabil(DateTime d)
        {
            return d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>El siguiente dia de lunes a viernes despues de hoy (un viernes propone el lunes).</summary>
        public static DateTime ProximoDiaHabil(DateTime hoy)
        {
            DateTime d = hoy.Date.AddDays(1);
            while (!EsDiaHabil(d)) { d = d.AddDays(1); }
            return d;
        }

        public static DateTime Lunes(DateTime d)
        {
            int desdeLunes = ((int)d.DayOfWeek + 6) % 7;
            return d.Date.AddDays(-desdeLunes);
        }

        /// <summary>Recorta, quita los vacios y numera el orden 1..n. Conserva los IdEntregable.</summary>
        public static List<EntAgendaEntregable> Limpiar(IEnumerable<EntAgendaEntregable> entregables)
        {
            List<EntAgendaEntregable> r = new List<EntAgendaEntregable>();
            if (entregables == null) { return r; }
            foreach (EntAgendaEntregable e in entregables)
            {
                string texto = e == null ? "" : (e.Descripcion ?? "").Trim();
                if (texto == "") { continue; }
                r.Add(new EntAgendaEntregable
                {
                    IdEntregable = e.IdEntregable,
                    IdActividad = e.IdActividad,
                    Descripcion = texto,
                    Orden = r.Count + 1,
                    Hecho = e.Hecho
                });
            }
            return r;
        }

        /// <summary>null si todo esta bien; si no, el mensaje para la persona.</summary>
        public static string Validar(string titulo, DateTime? fecha, IList<EntAgendaEntregable> entregables)
        {
            string t = (titulo ?? "").Trim();
            if (t == "") { return "Escriba qué hay que hacer."; }
            if (t.Length > LargoTexto) { return "El título admite hasta 200 caracteres."; }
            if (fecha == null) { return "Elija la fecha de entrega."; }
            if (!EsDiaHabil(fecha.Value)) { return "La fecha de entrega debe ser de lunes a viernes."; }
            if (entregables != null)
            {
                if (entregables.Count > MaxEntregables) { return "Una actividad admite hasta 20 entregables."; }
                if (entregables.Any(e => e != null && (e.Descripcion ?? "").Trim().Length > LargoTexto))
                {
                    return "Cada entregable admite hasta 200 caracteres.";
                }
            }
            return null;
        }
    }
}
