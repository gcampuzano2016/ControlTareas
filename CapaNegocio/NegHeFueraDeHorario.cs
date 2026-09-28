using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Cuenta el tiempo fuera de horario que no se va a pagar, sin base de datos
    /// ni pantalla. Recibe lo que devuelve Sp_RTA_HeFueraDeHorario y decide, minuto
    /// a minuto, en que categoria cae cada uno.
    ///
    /// Por que minuto a minuto y no restando intervalos: los tramos se registran
    /// con precision de minuto (DATEDIFF(MINUTE) en todo el modulo), un periodo son
    /// unos 45.000 minutos por persona como mucho, y un conjunto de minutos
    /// ocupados hace imposible contar dos veces el mismo, que es justo el error que
    /// esta clase existe para evitar. Una resta de intervalos escrita a mano tiene
    /// mas casos borde que pruebas.
    ///
    /// Prioridad de un minuto disputado: pagado > sin aprobar > rechazado, y dentro
    /// de una misma categoria 100% antes que 50%.
    /// </summary>
    public static class NegHeFueraDeHorario
    {
        public const string CategoriaSinAprobar = "SinAprobar";
        public const string CategoriaRechazado = "Rechazado";

        /// <summary>Cualquier fecha anterior a los datos sirve de origen para numerar minutos.</summary>
        private static readonly DateTime Origen = new DateTime(2000, 1, 1);

        public static EntHeFueraDeHorario Calcular(IList<EntHeSegmentoFuera> segmentos, IList<EntHeIntervaloPagado> pagados)
        {
            EntHeFueraDeHorario resultado = new EntHeFueraDeHorario();
            resultado.Calculado = true;

            IList<EntHeSegmentoFuera> todos = segmentos ?? new List<EntHeSegmentoFuera>();
            IList<EntHeIntervaloPagado> yaPagados = pagados ?? new List<EntHeIntervaloPagado>();

            var porPersona = todos
                .Where(s => !EstaPagado(s)
                            && (s.TipoHora == 1 || s.TipoHora == 2)
                            && s.SegmentoFin > s.SegmentoInicio)
                .GroupBy(s => s.IdEmpleado)
                .OrderBy(g => g.Key);

            foreach (var grupo in porPersona)
            {
                HashSet<long> ocupados = new HashSet<long>();

                foreach (EntHeIntervaloPagado p in yaPagados.Where(x => x.IdEmpleado == grupo.Key))
                {
                    for (long m = Minuto(p.Inicio); m < Minuto(p.Fin); m++) { ocupados.Add(m); }
                }

                EntHeFueraDeHorarioPersona persona = new EntHeFueraDeHorarioPersona();
                persona.IdEmpleado = grupo.Key;

                var enOrden = grupo
                    .OrderBy(s => Categoria(s) == CategoriaSinAprobar ? 0 : 1)
                    .ThenByDescending(s => s.TipoHora)
                    .ThenBy(s => s.SegmentoInicio)
                    .ThenBy(s => s.IdRegDetTareas);

                foreach (EntHeSegmentoFuera s in enOrden)
                {
                    long fin = Minuto(s.SegmentoFin);
                    long desde = -1;

                    // Recorre hasta fin INCLUSIVE para cerrar el ultimo pedazo abierto.
                    for (long m = Minuto(s.SegmentoInicio); m <= fin; m++)
                    {
                        bool libre = m < fin && ocupados.Add(m);

                        if (libre && desde < 0) { desde = m; }
                        if (!libre && desde >= 0)
                        {
                            Sumar(resultado, persona, s, desde, m);
                            desde = -1;
                        }
                    }
                }

                if (persona.MinutosSinAprobar50 + persona.MinutosSinAprobar100
                    + persona.MinutosRechazado50 + persona.MinutosRechazado100 > 0)
                {
                    resultado.Personas.Add(persona);
                }
            }

            resultado.Detalle = resultado.Detalle
                .OrderBy(d => d.IdEmpleado)
                .ThenBy(d => d.Inicio)
                .ThenBy(d => d.IdRegDetTareas)
                .ToList();

            return resultado;
        }

        /// <summary>Aprobado con tipo 50% o 100%: ya esta en lo que se paga.</summary>
        private static bool EstaPagado(EntHeSegmentoFuera s)
        {
            return s.EstadoTramo == 2 && (s.TipoRegistrado == 1 || s.TipoRegistrado == 2);
        }

        private static string Categoria(EntHeSegmentoFuera s)
        {
            return s.EstadoTramo == 3 ? CategoriaRechazado : CategoriaSinAprobar;
        }

        private static string EstadoTexto(EntHeSegmentoFuera s)
        {
            switch (s.EstadoTramo)
            {
                case 1: return "Solicitado";
                case 2: return "Aprobado sin tipo";
                case 3: return "Rechazado";
                default: return "Sin solicitar";
            }
        }

        /// <summary>Numero de minuto, truncado: coincide con DATEDIFF(MINUTE) del SQL.</summary>
        private static long Minuto(DateTime t)
        {
            return (long)Math.Floor((t - Origen).TotalMinutes);
        }

        private static void Sumar(EntHeFueraDeHorario resultado, EntHeFueraDeHorarioPersona persona,
                                  EntHeSegmentoFuera s, long desde, long hasta)
        {
            int minutos = (int)(hasta - desde);
            string categoria = Categoria(s);
            bool cien = s.TipoHora == 2;

            if (categoria == CategoriaRechazado)
            {
                if (cien) { persona.MinutosRechazado100 += minutos; } else { persona.MinutosRechazado50 += minutos; }
            }
            else
            {
                if (cien) { persona.MinutosSinAprobar100 += minutos; } else { persona.MinutosSinAprobar50 += minutos; }
            }

            EntHeFueraDeHorarioDetalle d = new EntHeFueraDeHorarioDetalle();
            d.IdEmpleado = s.IdEmpleado;
            d.IdRegDetTareas = s.IdRegDetTareas;
            d.TramoInicio = s.TramoInicio;
            d.TramoFin = s.TramoFin;
            d.Inicio = Origen.AddMinutes(desde);
            d.Fin = Origen.AddMinutes(hasta);
            d.Minutos = minutos;
            d.TipoHora = s.TipoHora;
            d.Categoria = categoria;
            d.EstadoTexto = EstadoTexto(s);
            d.Actividad = s.Actividad ?? "";
            resultado.Detalle.Add(d);
        }
    }
}
