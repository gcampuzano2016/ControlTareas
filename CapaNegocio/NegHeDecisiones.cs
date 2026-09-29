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
    /// dos lados: RechazoAlIncluir es el espejo de la regla -2 de Sp_RTA_HeGuardarDecisiones.
    /// </summary>
    public static class NegHeDecisiones
    {
        /// <summary>
        /// Completa cada candidato con lo que vale en el periodo idPeriodo.
        ///
        /// El pago doble se decide por TRAMO (Id_RegDetTareas), no por clave de
        /// segmento: la clave de un tramo cambia cuando cambia su estado (sin
        /// aprobar son sus segmentos fuera de horario; aprobado es el tramo
        /// completo), y una regla por clave dejaba que dos periodos pagaran el mismo
        /// tramo con claves distintas, o que se bloquearan entre si y ninguno lo
        /// pagara. Cada tramo tiene a lo sumo UN titular (ver Titular), y:
        ///   1. si el titular existe y es otro periodo, todos los segmentos del
        ///      tramo quedan Bloqueado aqui, con la leyenda del titular, y nunca
        ///      incluidos, diga lo que diga la decision de este periodo;
        ///   2. si no, un segmento queda incluido si la decision vigente de ESTE
        ///      periodo lo incluye o, sin decision propia, si este periodo es el
        ///      titular por omision (el dueno de un aprobado): asi el pago de hoy no
        ///      cambia hasta que Nomina marque algo.
        ///
        /// Ademas llena IncluirRechazadoPor con el periodo que impediria GUARDAR
        /// este tramo incluido aqui (ver RechazoAlIncluir): es lo que mira la
        /// regla de guardado (-2 de Sp_RTA_HeGuardarDecisiones, y Validar aqui).
        ///
        /// Un periodo 'Anulado' no cuenta para nada: no es dueno y sus decisiones no
        /// retienen, porque no paga. Devuelve copias: la entrada no se modifica, asi
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
            HashSet<int> cerrados = new HashSet<int>(todos.Where(p => p.EsCerrado).Select(p => p.IdPeriodo));
            List<EntHeRangoPeriodo> validos = todos.Where(p => !p.EsAnulado).ToList();

            // La vigente de cada (periodo, tramo, segmento): la de IdDecision mas
            // alto. El procedimiento ya la filtra, pero no conviene depender del
            // orden ni de que siempre venga una sola. Las de un periodo anulado se
            // descartan aqui mismo: no pagan, asi que no retienen ni excluyen.
            Dictionary<long, List<EntHeDecision>> vigentesPorTramo =
                (decisiones ?? new List<EntHeDecision>())
                .Where(d => d != null && !anulados.Contains(d.IdPeriodo))
                .GroupBy(d => new { d.IdPeriodo, d.IdRegDetTareas, d.SegmentoInicio })
                .Select(g => g.OrderByDescending(d => d.IdDecision).First())
                .GroupBy(d => d.IdRegDetTareas)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Los segmentos ACTUALES de cada tramo: las claves que hoy devuelve el
            // procedimiento. Una decision sobre otra clave es de cuando el tramo
            // tenia otro estado.
            Dictionary<long, HashSet<DateTime>> segmentosPorTramo = candidatos
                .Where(c => c != null)
                .GroupBy(c => c.IdRegDetTareas)
                .ToDictionary(g => g.Key, g => new HashSet<DateTime>(g.Select(c => c.SegmentoInicio)));

            // El titular se calcula una vez por tramo, no por segmento.
            Dictionary<long, int?> titular = new Dictionary<long, int?>();
            Dictionary<long, int?> rechazo = new Dictionary<long, int?>();

            foreach (EntHeCandidato original in candidatos)
            {
                if (original == null) { continue; }
                EntHeCandidato c = Copiar(original);

                List<EntHeDecision> vigentes;
                if (!vigentesPorTramo.TryGetValue(c.IdRegDetTareas, out vigentes))
                {
                    vigentes = new List<EntHeDecision>();
                }

                if (!titular.ContainsKey(c.IdRegDetTareas))
                {
                    HashSet<DateTime> actuales = segmentosPorTramo[c.IdRegDetTareas];
                    titular[c.IdRegDetTareas] = Titular(c, vigentes, actuales, cerrados, validos);
                    rechazo[c.IdRegDetTareas] = RechazoAlIncluir(c, vigentes, actuales, cerrados, validos, idPeriodo);
                }
                int? elTitular = titular[c.IdRegDetTareas];
                int? elRechazo = rechazo[c.IdRegDetTareas];

                // Decision de este periodo sobre ESTE segmento, o valor por omision.
                EntHeDecision propia = vigentes.FirstOrDefault(d => d.IdPeriodo == idPeriodo
                                                                 && d.SegmentoInicio == c.SegmentoInicio);
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
                    // Por omision solo paga el titular por omision: el dueno de un
                    // aprobado (sin decision propia sobre el tramo completo, que es
                    // este mismo segmento, no puede haberlo excluido).
                    c.Incluir = c.EsAprobado && Duenio(c, validos) == idPeriodo;
                    c.TipoElegido = c.TipoHora;
                    c.MinutosIncluidos = null;
                    c.Motivo = "";
                    c.DecididoPor = "";
                    c.DecididoEl = null;
                }

                // Otro titular: bloqueado, y un bloqueado nunca queda incluido aqui.
                // La decision de este periodo pudo guardarse antes de que el otro lo
                // retuviera, y pagarlo en los dos seria el pago doble.
                c.Bloqueado = elTitular.HasValue && elTitular.Value != idPeriodo;
                c.BloqueadoPor = c.Bloqueado ? Leyenda(elTitular.Value, todos) : "";
                if (c.Bloqueado) { c.Incluir = false; }

                c.IncluirRechazadoPor = elRechazo.HasValue
                    ? Leyenda(elRechazo.Value, todos)
                    : "";

                resultado.Add(c);
            }

            return resultado;
        }

        /// <summary>
        /// El periodo que retiene el tramo de c (a lo sumo uno), o null. Decide que
        /// se paga y que se ve bloqueado; la regla de GUARDAR (el bloque -2 de
        /// Sp_RTA_HeGuardarDecisiones) es RechazoAlIncluir:
        ///
        ///   1. Titular EXPLICITO: un periodo cuya decision vigente sobre ALGUN
        ///      segmento del tramo dice Incluir = 1, siempre que esa clave sea un
        ///      segmento ACTUAL del tramo o que el periodo este 'Cerrado'. Un
        ///      periodo abierto con una clave vieja no retiene: en su proximo
        ///      recalculo esa clave no esta entre los candidatos y no la paga. Uno
        ///      cerrado si: ya la pago. Si hubiera varios (no deberia: la regla de
        ///      guardado lo impide), manda la decision de IdDecision mas bajo, la
        ///      que llego primero.
        ///   2. Si no, titular POR OMISION: el dueno (el periodo cuyo rango contiene
        ///      el dia del tramo) si el tramo esta aprobado y alli nadie excluyo el
        ///      tramo completo.
        ///   3. Si no, nadie: cualquiera lo puede incluir.
        ///
        /// Limitacion conocida (documentada en DESPLIEGUE.md 14): si el estado de
        /// aprobacion de un tramo cambia DESPUES de que su dueno cerro pagandolo por
        /// omision, aqui ya no se sabe que se pago -no hay una foto del pago por
        /// tramo- y otro periodo podria incluirlo. El remedio es reabrir el periodo
        /// cerrado y recalcularlo.
        /// </summary>
        private static int? Titular(EntHeCandidato c, List<EntHeDecision> vigentes, HashSet<DateTime> segmentosActuales,
                                    HashSet<int> cerrados, List<EntHeRangoPeriodo> validos)
        {
            EntHeDecision explicita = Explicitas(vigentes, segmentosActuales, cerrados).FirstOrDefault();
            if (explicita != null) { return explicita.IdPeriodo; }

            return DuenioQuePagaPorOmision(c, vigentes, validos);
        }

        /// <summary>
        /// El periodo que impide GUARDAR Incluir = 1 sobre el tramo de c en
        /// idPeriodo, o null si se puede. Espejo exacto del bloque -2 de
        /// Sp_RTA_HeGuardarDecisiones. Se rechaza si:
        ///   (a) OTRO periodo lo retiene explicitamente (Incluir = 1 vigente en una
        ///       clave actual, o en cualquier clave si esta Cerrado). Siempre,
        ///       aunque este periodo tambien lo tenga incluido: si no, un guardado
        ///       podria dar vuelta el titular en silencio segun el IdDecision;
        ///   (b) el tramo esta aprobado, su dueno es OTRO periodo que no lo
        ///       excluyo, y este periodo NO lo tiene ya incluido (antes del lote)
        ///       en una clave actual. La primera inclusion sobre un tramo que el
        ///       dueno paga por omision se rechaza; pero quien ya lo retiene por
        ///       decision propia (trajo un borde antes de que existiera su dueno)
        ///       puede volver a guardarlo incluido para cambiarle el tipo o los
        ///       minutos. Si solo pudiera excluirlo, y el dueno ya hubiera cerrado,
        ///       nadie lo pagaria.
        /// </summary>
        private static int? RechazoAlIncluir(EntHeCandidato c, List<EntHeDecision> vigentes, HashSet<DateTime> segmentosActuales,
                                             HashSet<int> cerrados, List<EntHeRangoPeriodo> validos, int idPeriodo)
        {
            EntHeDecision otra = Explicitas(vigentes, segmentosActuales, cerrados)
                .FirstOrDefault(d => d.IdPeriodo != idPeriodo);
            if (otra != null) { return otra.IdPeriodo; }

            int? duenio = DuenioQuePagaPorOmision(c, vigentes, validos);
            if (!duenio.HasValue || duenio.Value == idPeriodo) { return null; }

            bool yaLoRetiene = vigentes.Any(d => d.IdPeriodo == idPeriodo && d.Incluir
                                              && segmentosActuales.Contains(d.SegmentoInicio));
            return yaLoRetiene ? (int?)null : duenio;
        }

        /// <summary>
        /// Las decisiones que retienen el tramo explicitamente, de la mas antigua
        /// (IdDecision mas bajo, la que manda) a la mas nueva: Incluir = 1 vigente
        /// sobre una clave ACTUAL del tramo, o sobre cualquier clave si su periodo
        /// esta 'Cerrado'. Un periodo abierto con una clave vieja no retiene: en su
        /// proximo recalculo esa clave no esta entre los candidatos y no la paga.
        /// Uno cerrado si: ya la pago.
        /// </summary>
        private static IEnumerable<EntHeDecision> Explicitas(List<EntHeDecision> vigentes, HashSet<DateTime> segmentosActuales,
                                                             HashSet<int> cerrados)
        {
            return vigentes
                .Where(d => d.Incluir && (segmentosActuales.Contains(d.SegmentoInicio) || cerrados.Contains(d.IdPeriodo)))
                .OrderBy(d => d.IdDecision);
        }

        /// <summary>
        /// El dueno de un tramo aprobado si lo paga por omision (alli nadie excluyo
        /// el tramo completo), o null.
        /// </summary>
        private static int? DuenioQuePagaPorOmision(EntHeCandidato c, List<EntHeDecision> vigentes, List<EntHeRangoPeriodo> validos)
        {
            if (!c.EsAprobado) { return null; }

            int? duenio = Duenio(c, validos);
            if (!duenio.HasValue) { return null; }

            bool excluidoEnSuDuenio = vigentes.Any(d => d.IdPeriodo == duenio.Value
                                                     && d.SegmentoInicio == c.TramoInicio
                                                     && !d.Incluir);
            return excluidoEnSuDuenio ? (int?)null : duenio;
        }

        /// <summary>
        /// El dueno del tramo: el primer periodo no anulado (por FechaInicio y luego
        /// IdPeriodo, igual que el procedimiento) cuyo rango contiene el dia en que
        /// empieza el tramo. null si el dia no es de ningun periodo.
        /// </summary>
        private static int? Duenio(EntHeCandidato c, List<EntHeRangoPeriodo> validos)
        {
            EntHeRangoPeriodo p = validos.FirstOrDefault(x => x.Contiene(c.TramoInicio));
            return p == null ? (int?)null : p.IdPeriodo;
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

            // -2: el pago doble. Se rechaza incluir si IncluirRechazadoPor nombra
            // un periodo (ver RechazoAlIncluir): otro titular explicito, o un
            // dueno que ya lo paga por omision y este periodo no lo retenia.
            // Bloqueado implica eso mismo; se mira tambien por si el candidato no
            // vino de Resolver.
            foreach (var par in pares)
            {
                EntHeCandidato c = par.Item2;
                if (!par.Item1.Incluir) { continue; }
                string retenidoPor = c.Bloqueado ? c.BloqueadoPor : c.IncluirRechazadoPor;
                if (c.Bloqueado || !string.IsNullOrEmpty(c.IncluirRechazadoPor))
                {
                    return "En el tramo " + c.SegmentoTexto + ": no se puede incluir, ya esta "
                         + retenidoPor + ".";
                }
            }

            return null;
        }

        /// <summary>
        /// Pone en cada candidato su ValorUsd: los minutos que muestra la ventana
        /// (MinutosMostrados) por el valor hora de la fila de la persona segun el
        /// tipo elegido, redondeado a centavos. Sin fila no hay valor hora, y el
        /// valor queda en 0: no se inventa un monto para quien no esta en el periodo.
        ///
        /// Modifica los candidatos que recibe: se llama sobre la salida de Resolver,
        /// que ya son copias.
        /// </summary>
        public static void Valorar(IList<EntHeCandidato> resueltos, IList<EntHeFila> filas)
        {
            if (resueltos == null) { return; }
            Dictionary<long, EntHeFila> porPersona = PorPersona(filas);

            foreach (EntHeCandidato c in resueltos)
            {
                if (c == null) { continue; }
                EntHeFila fila;
                if (!porPersona.TryGetValue(c.IdEmpleado, out fila)) { c.ValorUsd = 0m; continue; }

                decimal valorHora = ValorHora(fila, c.TipoElegido);
                c.ValorUsd = Math.Round(c.MinutosMostrados * valorHora / 60m, 2, MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>
        /// La conciliacion por persona: una fila por cada fila del periodo, en el
        /// mismo orden, con lo incluido aprobado, lo incluido no aprobado, lo
        /// excluido y el total a pagar.
        ///
        /// Todo se cuenta por minuto, igual que Minutos: un minuto incluido cuenta
        /// una vez, al 100% si algun segmento incluido lo tiene al 100%, y como
        /// "aprobado" si algun segmento incluido que lo cubre es un tramo aprobado.
        /// Asi lo aprobado mas lo no aprobado incluido es exactamente lo que paga la
        /// fila.
        ///
        /// Excluido es lo que se podria pagar y no se paga:
        ///   - un segmento sin incluir, o lo que sobra de uno reducido;
        ///   - nunca un bloqueado: ese lo paga otro periodo;
        ///   - nunca un minuto que ya paga otro segmento incluido;
        ///   - de los 7 dias de borde, solo lo que sobra de un segmento que Nomina
        ///     trajo; un borde sin incluir es tiempo del periodo vecino, y contarlo
        ///     aqui lo haria aparecer como "no incluido" en los dos periodos.
        ///
        /// Los USD de cada columna se redondean UNA vez sobre la persona, no por
        /// segmento: redondear cada pedazo y sumar descuadra centavos.
        /// </summary>
        public static List<EntHeConciliacionFila> Conciliar(IList<EntHeCandidato> resueltos, IList<EntHeFila> filas)
        {
            List<EntHeConciliacionFila> resultado = new List<EntHeConciliacionFila>();
            if (filas == null) { return resultado; }

            Dictionary<long, List<EntHeCandidato>> piezasPorPersona = (resueltos ?? new List<EntHeCandidato>())
                .Where(c => c != null)
                .GroupBy(c => c.IdEmpleado)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (EntHeFila fila in filas)
            {
                if (fila == null) { continue; }

                EntHeConciliacionFila r = new EntHeConciliacionFila();
                r.IdEmpleado = fila.IdEmpleado;
                r.Nombre = fila.NombreSnapshot ?? "";
                r.TotalAPagar = fila.TotalHE;

                List<EntHeCandidato> piezas;
                if (piezasPorPersona.TryGetValue(fila.IdEmpleado, out piezas))
                {
                    Conciliar(r, piezas, fila);
                }

                resultado.Add(r);
            }

            return resultado;
        }

        /// <summary>Cuenta los minutos de una persona y los pasa a USD con el valor hora de su fila.</summary>
        private static void Conciliar(EntHeConciliacionFila r, List<EntHeCandidato> piezas, EntHeFila fila)
        {
            // Minuto incluido -> [tipo, 1 si es aprobado]. Gana el 100% y gana el aprobado.
            Dictionary<long, int[]> incluidos = new Dictionary<long, int[]>();
            foreach (EntHeCandidato c in piezas.Where(k => k.Incluir && k.MinutosEfectivos > 0))
            {
                long desde = EntHeCandidato.Minuto(c.SegmentoInicio);
                long hasta = desde + c.MinutosEfectivos;
                for (long m = desde; m < hasta; m++)
                {
                    int[] v;
                    if (!incluidos.TryGetValue(m, out v)) { v = new int[] { 0, 0 }; incluidos[m] = v; }
                    if (c.TipoElegido == 2) { v[0] = 2; } else if (v[0] == 0) { v[0] = 1; }
                    if (c.EsAprobado) { v[1] = 1; }
                }
            }

            // Minuto excluido -> tipo. Lo que ya se paga no entra.
            Dictionary<long, int> excluidos = new Dictionary<long, int>();
            foreach (EntHeCandidato c in piezas.Where(k => !k.Bloqueado && (k.DentroDelPeriodo || k.Incluir)))
            {
                long inicio = EntHeCandidato.Minuto(c.SegmentoInicio);
                long desde = inicio + c.MinutosEfectivos;
                long hasta = inicio + c.MinutosSegmento;
                for (long m = desde; m < hasta; m++)
                {
                    if (incluidos.ContainsKey(m)) { continue; }
                    int tipo;
                    excluidos.TryGetValue(m, out tipo);
                    if (c.TipoElegido == 2 || tipo == 0) { excluidos[m] = c.TipoElegido == 2 ? 2 : 1; }
                }
            }

            int aprobado50 = 0, aprobado100 = 0, noAprobado50 = 0, noAprobado100 = 0, excluido50 = 0, excluido100 = 0;
            foreach (int[] v in incluidos.Values)
            {
                if (v[1] == 1) { if (v[0] == 2) { aprobado100++; } else { aprobado50++; } }
                else { if (v[0] == 2) { noAprobado100++; } else { noAprobado50++; } }
            }
            foreach (int tipo in excluidos.Values)
            {
                if (tipo == 2) { excluido100++; } else { excluido50++; }
            }

            r.MinutosAprobadoIncluido = aprobado50 + aprobado100;
            r.MinutosNoAprobadoIncluido = noAprobado50 + noAprobado100;
            r.MinutosExcluido = excluido50 + excluido100;
            r.UsdAprobadoIncluido = Usd(aprobado50, aprobado100, fila);

            /* Si el TotalHE guardado es lo que pagan HOY los minutos incluidos. Se
               recalcula con la misma regla que el pago: minutos exactos por tipo,
               con UN SOLO redondeo sobre la suma de los dos tipos -no un
               redondeo por tipo que despues se suma- (ver
               NegHorasExtras.Calcular, donde TotalHE = Monto(exacto50 +
               exacto100)). Si no coincide, la fila quedo vieja: se decidio o
               aprobo algo y el periodo no se recalculo (por ejemplo, fallo el
               recalculo despues de guardar decisiones).

               Antes redondeaba por tipo y sumaba los dos redondeos: eso era la
               regla vieja del pago, y marcaba Desactualizada a cualquier fila
               cuyo redondeo por tipo cayera un centavo distinto del redondeo
               unico, aunque el pago real fuera identico al TotalHE guardado.
               Usd() ya hace ese redondeo unico cuando se le pasan los dos tipos
               juntos -asi la llaman UsdAprobadoIncluido y UsdExcluido, mas
               abajo-, asi que aqui basta con no ser la excepcion. */
            int incluido50 = aprobado50 + noAprobado50;
            int incluido100 = aprobado100 + noAprobado100;
            decimal pagoHoy = Usd(incluido50, incluido100, fila);
            r.Desactualizada = pagoHoy != r.TotalAPagar;

            /* Al dia: lo no aprobado incluido se obtiene por diferencia y no
               redondeando su propio grupo. El pago redondea UNA sola vez por
               persona -TotalHE = Monto(exacto50 + exacto100); Total100 sale por
               diferencia (TotalHE - Total50), no de redondear su propio exacto,
               ver NegHorasExtras.Calcular- y aqui se agrupa por aprobado / no
               aprobado: redondear cada grupo por su lado puede separarse 0,01 o
               0,02 del TotalHE, y la conciliacion existe para cuadrar al
               centavo.

               Desactualizada: la diferencia contra un TotalHE viejo podria dar
               negativo, asi que se valora el grupo por su cuenta y la fila queda
               marcada; TotalAPagar sigue siendo el TotalHE, que es lo que se
               pagaria hoy. No se recorta a cero en silencio: la marca es la que
               avisa que hay que recalcular. */
            r.UsdNoAprobadoIncluido = r.Desactualizada
                ? Usd(noAprobado50, noAprobado100, fila)
                : r.TotalAPagar - r.UsdAprobadoIncluido;
            r.UsdExcluido = Usd(excluido50, excluido100, fila);
        }

        private static decimal Usd(int minutos50, int minutos100, EntHeFila fila)
        {
            decimal monto = (minutos50 * ValorHora(fila, 1) + minutos100 * ValorHora(fila, 2)) / 60m;
            return Math.Round(monto, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Lanza si la lectura de Sp_RTA_HeTramosCandidatos llego incompleta: sin el
        /// periodo pedido en el tercer conjunto no hay dueno, Resolver no incluye
        /// nada por omision y la siembra pagaria 0 a todos sin un solo error. El DAO
        /// tolera conjuntos que faltan (no lanza), asi que el corte tiene que estar
        /// aqui, antes de que alguien siembre con eso.
        /// </summary>
        public static void ValidarLectura(int idPeriodo, IList<EntHeRangoPeriodo> periodos)
        {
            if (periodos == null || !periodos.Any(p => p != null && p.IdPeriodo == idPeriodo))
            {
                throw new InvalidOperationException(
                    "La lectura de los tramos del período " + idPeriodo + " llegó incompleta (falta el período).");
            }
        }

        /// <summary>
        /// El valor hora con el que se valora un minuto. 0 si la persona no aplica a
        /// horas extras: su pago es 0 aunque la fila tenga valor hora calculado, y
        /// mostrarle USD seria anunciar un pago que no existe.
        /// </summary>
        private static decimal ValorHora(EntHeFila fila, int tipo)
        {
            if (fila == null || !fila.AplicaHESnapshot) { return 0m; }
            return tipo == 2 ? fila.ValorHora100 : fila.ValorHora50;
        }

        private static Dictionary<long, EntHeFila> PorPersona(IList<EntHeFila> filas)
        {
            Dictionary<long, EntHeFila> d = new Dictionary<long, EntHeFila>();
            if (filas == null) { return d; }
            foreach (EntHeFila f in filas)
            {
                if (f != null && !d.ContainsKey(f.IdEmpleado)) { d[f.IdEmpleado] = f; }
            }
            return d;
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
