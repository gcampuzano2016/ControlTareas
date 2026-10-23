using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>Reglas de las semanas marcables de un bono (lunes a domingo).</summary>
    public static class NegBonoFechas
    {
        public static readonly DateTime PrimerLunes = new DateTime(2026, 8, 3);

        private static readonly CultureInfo Cultura = new CultureInfo("es-EC");

        public static bool EsLunes(DateTime fecha)
        {
            return fecha.DayOfWeek == DayOfWeek.Monday;
        }

        /// <summary>La semana ya terminada: su domingo es anterior a hoy.</summary>
        public static bool Terminada(DateTime lunes, DateTime hoy)
        {
            return lunes.Date.AddDays(6) < hoy.Date;
        }

        /// <summary>null = valido; si no, el mensaje para el usuario.</summary>
        public static string ValidarSemanas(IList<DateTime> lunes, bool porSemana, DateTime hoy)
        {
            int cuantas = lunes == null ? 0 : lunes.Count;
            if (!porSemana)
                return cuantas > 0 ? "Este tipo de bono no lleva semanas." : null;
            if (cuantas == 0)
                return "Marque al menos una semana.";

            HashSet<DateTime> vistas = new HashSet<DateTime>();
            foreach (DateTime l in lunes)
            {
                if (!EsLunes(l)) return "Las semanas empiezan en lunes.";
                if (l.Date < PrimerLunes) return "No se pueden marcar semanas anteriores al 3 de agosto de 2026.";
                if (!Terminada(l, hoy)) return "Solo se pueden marcar semanas ya terminadas.";
                if (!vistas.Add(l.Date)) return "Hay una semana marcada dos veces.";
            }
            return null;
        }

        /// <summary>Texto para la columna FECHAS: "Del 3 al 9 de agosto y del 14 al 20 de septiembre".</summary>
        public static string TextoFechas(IEnumerable<DateTime> lunes)
        {
            if (lunes == null) return "";
            List<DateTime> orden = lunes.Select(l => l.Date).Distinct().OrderBy(l => l).ToList();
            if (orden.Count == 0) return "";

            List<DateTime[]> tramos = new List<DateTime[]>();
            DateTime inicio = orden[0], ultimo = orden[0];
            for (int i = 1; i < orden.Count; i++)
            {
                if ((orden[i] - ultimo).Days == 7) { ultimo = orden[i]; continue; }
                tramos.Add(new[] { inicio, ultimo.AddDays(6) });
                inicio = ultimo = orden[i];
            }
            tramos.Add(new[] { inicio, ultimo.AddDays(6) });

            List<string> partes = new List<string>();
            for (int i = 0; i < tramos.Count; i++)
            {
                DateTime a = tramos[i][0], b = tramos[i][1];
                bool mesInicio = a.Month != b.Month;
                bool mesFin = i == tramos.Count - 1 || tramos[i + 1][0].Month != b.Month;
                partes.Add(a.Day + (mesInicio ? " de " + Mes(a) : "") + " al " + b.Day + (mesFin ? " de " + Mes(b) : ""));
            }

            if (partes.Count == 1) return "Del " + partes[0];
            return "Del " + string.Join(", del ", partes.Take(partes.Count - 1)) + " y del " + partes[partes.Count - 1];
        }

        private static string Mes(DateTime f)
        {
            return Cultura.DateTimeFormat.GetMonthName(f.Month).ToLower(Cultura);
        }
    }
}
