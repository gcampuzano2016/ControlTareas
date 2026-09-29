using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// La decision de Nomina, segmento por segmento, sin base de datos ni pantalla.
    /// Recibe los tres conjuntos de Sp_RTA_HeTramosCandidatos y responde tres
    /// preguntas: que queda incluido en este periodo (Resolver), cuantos minutos al
    /// 50% y al 100% paga cada persona (Minutos), y si lo que se quiere guardar es
    /// valido (Validar).
    ///
    /// Por que aqui y no en el SQL: el procedimiento que guarda aplica las mismas
    /// reglas como ultima barrera (codigos -2, -3 y -4), pero la pantalla tiene que
    /// mostrar el bloqueo y el motivo del rechazo ANTES de guardar, y la union por
    /// minuto solo tiene pruebas si vive en C#. Si las reglas cambian, cambian en los
    /// dos lados: Resolver es el espejo de la regla -2 de Sp_RTA_HeGuardarDecisiones.
    /// </summary>
    public static class NegHeDecisiones
    {
        /// <summary>
        /// Completa cada candidato con lo que vale en el periodo idPeriodo:
        ///   1. la decision vigente de ESTE periodo, si la hay; si no, el valor por
        ///      omision, que incluye solo un aprobado cuyo dueno es este periodo
        ///      (asi el pago de hoy no cambia hasta que Nomina marque algo);
        ///   2. Bloqueado si OTRO periodo lo tiene incluido: por su decision
        ///      vigente, o porque es su dueno, el tramo es aprobado y alli nadie lo
        ///      excluyo;
        ///   3. un bloqueado nunca queda incluido, aunque la decision de este periodo
        ///      diga lo contrario: esa decision pudo guardarse antes de que el otro
        ///      periodo lo incluyera, y pagarlo en los dos seria el pago doble.
        ///
        /// Un periodo 'Anulado' no cuenta para nada: no es dueno y sus decisiones no
        /// bloquean, porque no paga. Devuelve copias: la entrada no se modifica, asi
        /// que se puede resolver la misma lista para otro periodo.
        /// </summary>
        public static List<EntHeCandidato> Resolver(int idPeriodo, IList<EntHeCandidato> candidatos,
                                                    IList<EntHeDecision> decisiones, IList<EntHeRangoPeriodo> periodos)
        {
            List<EntHeCandidato> resultado = new List<EntHeCandidato>();
            if (candidatos == null) { return resultado; }

            List<EntHeRangoPeriodo> todos = (periodos ?? new List<EntHeRangoPeriodo>())
                .Where(p => p != null)
                .OrderBy(p => p.FechaInicio)
                .ThenBy(p => p.IdPeriodo)
                .ToList();
            HashSet<int> anulados = new HashSet<int>(todos.Where(p => p.EsAnulado).Select(p => p.IdPeriodo));
            List<EntHeRangoPeriodo> validos = todos.Where(p => !p.EsAnulado).ToList();

            // La vigente de cada (periodo, tramo, segmento): la de IdDecision mas alto.
            // El procedimiento ya la filtra, pero no conviene depender del orden ni de
            // que siempre venga una sola.
            Dictionary<Tuple<long, DateTime>, List<EntHeDecision>> vigentesPorSegmento =
                (decisiones ?? new List<EntHeDecision>())
                .Where(d => d != null)
                .GroupBy(d => new { d.IdPeriodo, d.IdRegDetTareas, d.SegmentoInicio })
                .Select(g => g.OrderByDescending(d => d.IdDecision).First())
                .GroupBy(d => Clave(d.IdRegDetTareas, d.SegmentoInicio))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (EntHeCandidato original in candidatos)
            {
                if (original == null) { continue; }
                EntHeCandidato c = Copiar(original);

                List<EntHeDecision> vigentes;
                if (!vigentesPorSegmento.TryGetValue(Clave(c.IdRegDetTareas, c.SegmentoInicio), out vigentes))
                {
                    vigentes = new List<EntHeDecision>();
                }

                List<EntHeRangoPeriodo> duenos = validos.Where(p => p.Contiene(c.TramoInicio)).ToList();

                // 1. Decision de este periodo, o valor por omision.
                EntHeDecision propia = vigentes.FirstOrDefault(d => d.IdPeriodo == idPeriodo);
                if (propia != null)
                {
                    c.Incluir = propia.Incluir;
                    c.TipoElegido = propia.TipoHora;
                    c.MinutosIncluidos = propia.MinutosIncluidos;
                    c.Motivo = propia.Motivo ?? "";
                    c.DecididoPor = propia.Usuario ?? "";
                    c.DecididoEl = propia.Fecha;
                }
                else
                {
                    c.Incluir = c.EsAprobado && duenos.Any(p => p.IdPeriodo == idPeriodo);
                    c.TipoElegido = c.TipoHora;
                    c.MinutosIncluidos = null;
                    c.Motivo = "";
                    c.DecididoPor = "";
                    c.DecididoEl = null;
                }

                // 2. El pago doble. (a) otro periodo no anulado lo incluyo por decision.
                string bloqueadoPor = null;
                EntHeDecision incluidaAfuera = vigentes
                    .Where(d => d.IdPeriodo != idPeriodo && d.Incluir && !anulados.Contains(d.IdPeriodo))
                    .OrderBy(d => d.IdDecision)
                    .FirstOrDefault();
                if (incluidaAfuera != null)
                {
                    bloqueadoPor = Leyenda(incluidaAfuera.IdPeriodo, todos);
                }

                // (b) otro periodo es su dueno y lo paga por omision: solo un
                //     aprobado, y solo si alli nadie lo excluyo.
                if (bloqueadoPor == null && c.EsAprobado)
                {
                    EntHeRangoPeriodo duenoAjeno = duenos.FirstOrDefault(p =>
                        p.IdPeriodo != idPeriodo
                        && !vigentes.Any(d => d.IdPeriodo == p.IdPeriodo && !d.Incluir));
                    if (duenoAjeno != null)
                    {
                        bloqueadoPor = Leyenda(duenoAjeno.IdPeriodo, todos);
                    }
                }

                // 3. Un bloqueado nunca queda incluido aqui.
                c.Bloqueado = bloqueadoPor != null;
                c.BloqueadoPor = bloqueadoPor ?? "";
                if (c.Bloqueado) { c.Incluir = false; }

                resultado.Add(c);
            }

            return resultado;
        }

        /// <summary>
        /// Minutos a pagar por persona: [0] al 50% y [1] al 100%. Solo cuentan los
        /// segmentos incluidos, con el tipo que eligio Nomina (TipoElegido).
        ///
        /// Se une por minuto, como NegHeFueraDeHorario: dos segmentos que se pisan
        /// pagan ese minuto una sola vez y, si uno es 50% y otro 100%, gana el 100%.
        /// De un segmento reducido se toman los PRIMEROS N minutos: es la regla de la
        /// especificacion (la reduccion sigue pendiente de confirmar con Nomina).
        /// </summary>
        public static Dictionary<long, int[]> Minutos(IList<EntHeCandidato> resueltos)
        {
            Dictionary<long, int[]> resultado = new Dictionary<long, int[]>();
            if (resueltos == null) { return resultado; }

            var porPersona = resueltos
                .Where(c => c != null && c.Incluir && c.MinutosEfectivos > 0)
                .GroupBy(c => c.IdEmpleado);

            foreach (var grupo in porPersona)
            {
                HashSet<long> cincuenta = new HashSet<long>();
                HashSet<long> cien = new HashSet<long>();

                foreach (EntHeCandidato c in grupo)
                {
                    HashSet<long> destino = c.TipoElegido == 2 ? cien : cincuenta;
                    long desde = EntHeCandidato.Minuto(c.SegmentoInicio);
                    long hasta = desde + c.MinutosEfectivos;
                    for (long m = desde; m < hasta; m++) { destino.Add(m); }
                }

                // Un minuto que esta en los dos se paga al 100%.
                cincuenta.ExceptWith(cien);
                resultado[grupo.Key] = new int[] { cincuenta.Count, cien.Count };
            }

            return resultado;
        }

        /// <summary>
        /// null si el lote se puede guardar; si no, el primer error, en palabras para
        /// Nomina. Revisa en el mismo orden que Sp_RTA_HeGuardarDecisiones (-4, -3,
        /// -2), sobre el lote entero en cada paso, para que la pantalla y el
        /// procedimiento rechacen por la misma razon.
        ///
        /// Los minutos se validan contra el largo exacto del SEGMENTO; el
        /// procedimiento solo puede validar contra el fin del tramo, asi que esta es
        /// la validacion fina y aquella el respaldo.
        /// </summary>
        public static string Validar(IList<EntHeCandidato> resueltos, long idEmpleado, IList<EntHeCambioDecision> cambios)
        {
            if (cambios == null || cambios.Count(x => x != null) == 0)
            {
                return "No hay decisiones que guardar.";
            }

            List<EntHeCandidato> dePersona = (resueltos ?? new List<EntHeCandidato>())
                .Where(c => c != null && c.IdEmpleado == idEmpleado)
                .ToList();
            List<EntHeCambioDecision> lote = cambios.Where(x => x != null).ToList();
            List<Tuple<EntHeCambioDecision, EntHeCandidato>> pares = new List<Tuple<EntHeCambioDecision, EntHeCandidato>>();

            // -4: cada cambio tiene que ser un segmento candidato de ESTA persona. Se
            // compara la clave completa: otro SegmentoInicio sobre el mismo tramo
            // esquivaria la regla del pago doble.
            foreach (EntHeCambioDecision x in lote)
            {
                EntHeCandidato c = dePersona.FirstOrDefault(k => k.IdRegDetTareas == x.IdRegDetTareas
                                                              && k.SegmentoInicio == x.SegmentoInicio);
                if (c == null)
                {
                    return "El tramo " + x.IdRegDetTareas + " (" + x.SegmentoInicio.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
                         + ") no es un tramo candidato de esta persona en este periodo. Vuelva a abrir el detalle.";
                }
                pares.Add(Tuple.Create(x, c));
            }

            // -3: datos invalidos.
            if (lote.GroupBy(x => Clave(x.IdRegDetTareas, x.SegmentoInicio)).Any(g => g.Count() > 1))
            {
                return "El mismo tramo aparece dos veces en las decisiones.";
            }

            foreach (var par in pares)
            {
                EntHeCambioDecision x = par.Item1;
                EntHeCandidato c = par.Item2;
                string donde = "En el tramo " + c.SegmentoTexto + ": ";
                bool conMotivo = !string.IsNullOrWhiteSpace(x.Motivo);

                if (x.TipoHora != 1 && x.TipoHora != 2)
                {
                    return donde + "el tipo tiene que ser 50% o 100%.";
                }
                if (x.MinutosIncluidos.HasValue
                    && (x.MinutosIncluidos.Value < 1 || x.MinutosIncluidos.Value > c.MinutosSegmento))
                {
                    return donde + "los minutos tienen que estar entre 1 y " + c.MinutosSegmento
                         + ". Solo se pueden reducir, nunca subir.";
                }
                if (!conMotivo && x.MinutosIncluidos.HasValue)
                {
                    return donde + "falta el motivo: es obligatorio cuando se reducen los minutos.";
                }
                if (!conMotivo && x.Incluir && !c.EsAprobado)
                {
                    return donde + "falta el motivo: es obligatorio para incluir un tramo que no esta aprobado.";
                }
                if (conMotivo && x.Motivo.Trim().Length > 400)
                {
                    return donde + "el motivo no puede pasar de 400 caracteres.";
                }
            }

            // -2: el pago doble.
            foreach (var par in pares)
            {
                if (par.Item1.Incluir && par.Item2.Bloqueado)
                {
                    return "En el tramo " + par.Item2.SegmentoTexto + ": no se puede incluir, ya esta "
                         + par.Item2.BloqueadoPor + ".";
                }
            }

            return null;
        }

        private static Tuple<long, DateTime> Clave(long idRegDetTareas, DateTime segmentoInicio)
        {
            return Tuple.Create(idRegDetTareas, segmentoInicio);
        }

        /// <summary>
        /// La leyenda del bloqueo con el rango del otro periodo. Una decision de un
        /// periodo que no vino en la lista (no deberia pasar: el procedimiento los
        /// devuelve todos) bloquea igual, porque el procedimiento tambien la cuenta;
        /// solo que se nombra por su numero.
        /// </summary>
        private static string Leyenda(int idPeriodo, IList<EntHeRangoPeriodo> periodos)
        {
            EntHeRangoPeriodo p = periodos.FirstOrDefault(x => x.IdPeriodo == idPeriodo);
            return p != null
                ? "incluido en el periodo " + p.RangoTexto
                : "incluido en el periodo " + idPeriodo;
        }

        private static EntHeCandidato Copiar(EntHeCandidato o)
        {
            return new EntHeCandidato
            {
                IdEmpleado = o.IdEmpleado,
                IdRegDetTareas = o.IdRegDetTareas,
                TramoInicio = o.TramoInicio,
                TramoFin = o.TramoFin,
                SegmentoInicio = o.SegmentoInicio,
                SegmentoFin = o.SegmentoFin,
                TipoHora = o.TipoHora,
                EstadoTramo = o.EstadoTramo,
                TipoRegistrado = o.TipoRegistrado,
                Actividad = o.Actividad ?? "",
                DentroDelPeriodo = o.DentroDelPeriodo
            };
        }
    }
}
