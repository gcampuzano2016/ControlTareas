using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Aritmetica de dias habiles (lunes a viernes que no son feriado).
    /// Clase pura: el conjunto de feriados lo pasa quien llama.
    /// </summary>
    public static class NegAgendaDiasHabiles
    {
        public const int PlazoMinimo = 1;
        public const int PlazoMaximo = 60;

        public static bool EsHabil(DateTime d, ISet<DateTime> feriados)
        {
            DateTime dia = d.Date;
            if (dia.DayOfWeek == DayOfWeek.Saturday || dia.DayOfWeek == DayOfWeek.Sunday) return false;
            return feriados == null || !feriados.Contains(dia);
        }

        /// <summary>
        /// El conteo empieza el dia siguiente a "desde". Devuelve el dia habil
        /// numero "dias" (1..60) a partir de ahi, sin hora.
        /// </summary>
        public static DateTime SumarDiasHabiles(DateTime desde, int dias, ISet<DateTime> feriados)
        {
            if (dias < PlazoMinimo || dias > PlazoMaximo)
                throw new ArgumentOutOfRangeException("dias", "El plazo va de 1 a 60 dias habiles.");
            DateTime d = desde.Date;
            int contados = 0;
            while (contados < dias)
            {
                d = d.AddDays(1);
                if (EsHabil(d, feriados)) contados++;
            }
            return d;
        }
    }
}
