using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Arma lo que dibujan las pantallas de la agenda. Clase pura: recibe las
    /// personas activas y las actividades ya cargadas, y decide con
    /// NegAgendaEquipo y NegAgendaActividad. El handler no decide nada.
    /// </summary>
    public static class NegAgendaVista
    {
        private static readonly CultureInfo Es = new CultureInfo("es-EC");

        public static string Iniciales(string nombre)
        {
            string[] partes = (nombre ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string r = "";
            foreach (string p in partes)
            {
                r += char.ToUpperInvariant(p[0]);
                if (r.Length == 2) { break; }
            }
            return r == "" ? "?" : r;
        }

        /// <summary>
        /// Actividades, completas y entregables: las que vencen de lunes a viernes
        /// de esa semana. Atrasadas: de cualquier fecha. Las anuladas no cuentan.
        /// </summary>
        public static EntAgendaContador Contar(IEnumerable<EntAgendaActividad> actividades, DateTime lunes, DateTime hoy)
        {
            EntAgendaContador c = new EntAgendaContador();
            DateTime desde = lunes.Date;
            DateTime hasta = lunes.Date.AddDays(4);
            foreach (EntAgendaActividad a in actividades ?? Enumerable.Empty<EntAgendaActividad>())
            {
                if (a == null || a.Anulada) { continue; }
                if (NegAgendaActividad.Atrasada(a, hoy)) { c.Atrasadas++; }
                if (a.FechaEntrega.Date < desde || a.FechaEntrega.Date > hasta) { continue; }
                c.Actividades++;
                if (NegAgendaActividad.Completa(a)) { c.Completas++; }
                c.EntregablesTotal += a.Entregables.Count;
                c.EntregablesHechos += a.Entregables.Count(e => e.Hecho);
            }
            c.Porcentaje = Porcentaje(c);
            return c;
        }

        public static EntAgendaContador Sumar(IEnumerable<EntAgendaContador> contadores)
        {
            EntAgendaContador s = new EntAgendaContador();
            foreach (EntAgendaContador c in contadores ?? Enumerable.Empty<EntAgendaContador>())
            {
                s.Actividades += c.Actividades;
                s.Completas += c.Completas;
                s.Atrasadas += c.Atrasadas;
                s.EntregablesTotal += c.EntregablesTotal;
                s.EntregablesHechos += c.EntregablesHechos;
            }
            s.Porcentaje = Porcentaje(s);
            return s;
        }

        private static int? Porcentaje(EntAgendaContador c)
        {
            if (c.EntregablesTotal == 0) { return null; }
            return (int)Math.Round(c.EntregablesHechos * 100.0 / c.EntregablesTotal, MidpointRounding.AwayFromZero);
        }

        /// <summary>Las personas que yo puedo ver, ordenadas por nombre. Me incluye.</summary>
        public static List<EntAgendaPersona> Visibles(string yo, bool verTodos, IList<EntAgendaPersona> personas)
        {
            if (personas == null) { return new List<EntAgendaPersona>(); }
            HashSet<string> cadena = NegAgendaEquipo.Cadena(personas, yo);
            return personas
                .Where(p => p != null && NegAgendaEquipo.PuedeVer(yo, p.CodUsuario, cadena, verTodos))
                .OrderBy(p => p.Nombre, StringComparer.Create(Es, true))
                .ToList();
        }

        public static List<EntAgendaTarjetaArea> Equipo(string yo, bool verTodos, IList<EntAgendaPersona> personas,
            IList<EntAgendaActividad> actividades, DateTime lunes, DateTime hoy)
        {
            ILookup<string, EntAgendaActividad> porPersona = (actividades ?? new List<EntAgendaActividad>())
                .ToLookup(a => NegAgendaEquipo.Normalizar(a.CodResponsable), NegAgendaEquipo.Codigos);

            return Visibles(yo, verTodos, personas)
                .GroupBy(p => p.IdArea)
                .Select(g =>
                {
                    EntAgendaPersona primera = g.First();
                    List<EntAgendaTarjetaPersona> gente = g.Select(p => new EntAgendaTarjetaPersona
                    {
                        CodUsuario = p.CodUsuario,
                        Nombre = p.Nombre,
                        Iniciales = Iniciales(p.Nombre),
                        Contador = Contar(porPersona[NegAgendaEquipo.Normalizar(p.CodUsuario)], lunes, hoy)
                    }).ToList();
                    return new EntAgendaTarjetaArea
                    {
                        IdArea = primera.IdArea,
                        Area = primera.Area,
                        Color = primera.Color,
                        SinArea = primera.SinArea,
                        Personas = gente,
                        Contador = Sumar(gente.Select(x => x.Contador))
                    };
                })
                .OrderBy(t => t.SinArea)
                .ThenBy(t => t.Area, StringComparer.Create(Es, true))
                .ToList();
        }

        /// <summary>null si yo no puedo ver a esa persona o no esta activa.</summary>
        public static EntAgendaVistaPersona Persona(string yo, bool verTodos, IList<EntAgendaPersona> personas,
            IList<EntAgendaActividad> actividades, string cod, DateTime lunes, DateTime hoy)
        {
            if (personas == null) { return null; }
            HashSet<string> cadena = NegAgendaEquipo.Cadena(personas, yo);
            string buscado = NegAgendaEquipo.Normalizar(cod);
            EntAgendaPersona p = personas.FirstOrDefault(x => x != null && NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(x.CodUsuario), buscado));
            if (p == null || !NegAgendaEquipo.PuedeVer(yo, p.CodUsuario, cadena, verTodos)) { return null; }

            Dictionary<string, string> nombres = personas
                .Where(x => x != null)
                .GroupBy(x => NegAgendaEquipo.Normalizar(x.CodUsuario), NegAgendaEquipo.Codigos)
                .ToDictionary(g => g.Key, g => g.First().Nombre, NegAgendaEquipo.Codigos);

            List<EntAgendaActividad> suyas = (actividades ?? new List<EntAgendaActividad>())
                .Where(a => a != null && !a.Anulada && NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(a.CodResponsable), NegAgendaEquipo.Normalizar(p.CodUsuario)))
                .OrderBy(a => a.FechaEntrega)
                .ThenBy(a => a.IdActividad)
                .ToList();

            bool esPropia = NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(yo), NegAgendaEquipo.Normalizar(p.CodUsuario));
            EntAgendaVistaPersona v = new EntAgendaVistaPersona
            {
                CodUsuario = p.CodUsuario,
                Nombre = p.Nombre,
                Iniciales = Iniciales(p.Nombre),
                Area = p.Area,
                Color = p.Color,
                EsPropia = esPropia,
                PuedeAsignar = esPropia || NegAgendaEquipo.PuedeAsignarA(yo, p.CodUsuario, cadena, verTodos)
            };

            Func<EntAgendaActividad, EntAgendaItem> item = a => Item(yo, verTodos, cadena, nombres, a, hoy);
            v.Atrasadas = suyas.Where(a => NegAgendaActividad.Atrasada(a, hoy)).Select(item).ToList();
            for (int i = 0; i < 5; i++)
            {
                DateTime dia = lunes.Date.AddDays(i);
                v.Dias.Add(new EntAgendaDia
                {
                    Fecha = dia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Nombre = Es.DateTimeFormat.GetDayName(dia.DayOfWeek),
                    EsHoy = dia == hoy.Date,
                    Actividades = suyas.Where(a => a.FechaEntrega.Date == dia).Select(item).ToList()
                });
            }
            return v;
        }

        private static EntAgendaItem Item(string yo, bool verTodos, ISet<string> cadena, Dictionary<string, string> nombres,
            EntAgendaActividad a, DateTime hoy)
        {
            bool propia = NegAgendaActividad.EsPropia(a);
            string asignador = "";
            if (!propia)
            {
                string cod = NegAgendaEquipo.Normalizar(a.CodAsignadoPor);
                if (!nombres.TryGetValue(cod, out asignador)) { asignador = cod; }
            }
            return new EntAgendaItem
            {
                IdActividad = a.IdActividad,
                Titulo = a.Titulo,
                FechaEntrega = a.FechaEntrega.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                AsignadoPor = asignador,
                Propia = propia,
                Completa = NegAgendaActividad.Completa(a),
                Atrasada = NegAgendaActividad.Atrasada(a, hoy),
                PuedeEditar = NegAgendaActividad.PuedeEditar(yo, a, cadena, verTodos),
                PuedeMarcar = NegAgendaActividad.PuedeMarcar(yo, a),
                Entregables = a.Entregables
                    .OrderBy(e => e.Orden)
                    .ThenBy(e => e.IdEntregable)
                    .Select(e => new EntAgendaItemEntregable { IdEntregable = e.IdEntregable, Descripcion = e.Descripcion, Hecho = e.Hecho })
                    .ToList()
            };
        }
    }
}
