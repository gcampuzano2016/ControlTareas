using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaPruebas
{
    /// <summary>
    /// La decision por tramo: el valor por omision, la decision vigente, el
    /// periodo dueno y el pago doble, los minutos que entran en el pago y la
    /// validacion de lo que Nomina pide guardar. Los datos son inventados: el
    /// repositorio es publico.
    ///
    /// Periodos de las pruebas (inventados, con la forma de los reales):
    ///   3 = 16/08/2026 - 15/09/2026, abierto (el que se esta decidiendo)
    ///   4 = 15/07/2026 - 13/08/2026, abierto (el anterior)
    /// El 14 y el 15-08 no son de ningun periodo, igual que hoy en produccion.
    /// </summary>
    [TestClass]
    public class NegHeDecisionesTests
    {
        private const long Persona = 101;
        private const long OtraPersona = 202;
        private const int Este = 3;
        private const int Anterior = 4;

        private static DateTime H(string texto)
        {
            return DateTime.ParseExact(texto, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }

        private static DateTime D(string texto)
        {
            return DateTime.ParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static EntHeRangoPeriodo Periodo(int id, string inicio, string fin, string estado = "Abierto")
        {
            return new EntHeRangoPeriodo { IdPeriodo = id, FechaInicio = D(inicio), FechaFin = D(fin), EstadoPeriodo = estado };
        }

        private static List<EntHeRangoPeriodo> Periodos()
        {
            return new List<EntHeRangoPeriodo>
            {
                Periodo(Anterior, "2026-07-15", "2026-08-13"),
                Periodo(Este, "2026-08-16", "2026-09-15")
            };
        }

        /// <summary>Un tramo aprobado: un solo segmento, el tramo completo, con el tipo aprobado.</summary>
        private static EntHeCandidato Aprobado(long idTramo, string inicio, string fin, int tipo, long empleado = Persona)
        {
            return Cand(idTramo, inicio, fin, tipo, 2, tipo, empleado);
        }

        private static EntHeCandidato Cand(long idTramo, string inicio, string fin, int tipoHora, int estado,
                                           int tipoRegistrado = 0, long empleado = Persona)
        {
            return new EntHeCandidato
            {
                IdEmpleado = empleado,
                IdRegDetTareas = idTramo,
                TramoInicio = H(inicio),
                TramoFin = H(fin),
                SegmentoInicio = H(inicio),
                SegmentoFin = H(fin),
                TipoHora = tipoHora,
                EstadoTramo = estado,
                TipoRegistrado = tipoRegistrado,
                Actividad = "ACTIVIDAD " + idTramo
            };
        }

        private static EntHeDecision Dec(int idDecision, int idPeriodo, EntHeCandidato c, bool incluir,
                                         int tipo = 1, int? minutos = null, string motivo = "")
        {
            return new EntHeDecision
            {
                IdDecision = idDecision,
                IdPeriodo = idPeriodo,
                IdRegDetTareas = c.IdRegDetTareas,
                SegmentoInicio = c.SegmentoInicio,
                Incluir = incluir,
                TipoHora = tipo,
                MinutosIncluidos = minutos,
                Motivo = motivo,
                Usuario = "usuario" + idDecision,
                Fecha = H("2026-09-20 10:00")
            };
        }

        private static EntHeCandidato Resolver1(EntHeCandidato c, params EntHeDecision[] decisiones)
        {
            List<EntHeCandidato> r = NegHeDecisiones.Resolver(Este, new List<EntHeCandidato> { c },
                                                              decisiones.ToList(), Periodos());
            Assert.AreEqual(1, r.Count);
            return r[0];
        }

        private static EntHeCambioDecision Cambio(EntHeCandidato c, bool incluir, int tipo, int? minutos = null, string motivo = "")
        {
            return new EntHeCambioDecision
            {
                IdRegDetTareas = c.IdRegDetTareas,
                SegmentoInicio = c.SegmentoInicio,
                Incluir = incluir,
                TipoHora = tipo,
                MinutosIncluidos = minutos,
                Motivo = motivo
            };
        }

        private static string Validar(EntHeCandidato c, params EntHeCambioDecision[] cambios)
        {
            return NegHeDecisiones.Validar(new List<EntHeCandidato> { c }, Persona, cambios.ToList());
        }

        // ================================================= Resolver =========

        [TestMethod]
        public void Resolver_AprobadoDentro_IncluidoPorOmision()
        {
            EntHeCandidato r = Resolver1(Aprobado(1, "2026-08-20 18:00", "2026-08-20 20:00", 1));

            Assert.IsTrue(r.Incluir);
            Assert.AreEqual(1, r.TipoElegido);
            Assert.IsFalse(r.Bloqueado);
            Assert.AreEqual("", r.BloqueadoPor);
            Assert.AreEqual("Aprobado", r.Grupo);
            Assert.AreEqual(120, r.MinutosSegmento);
            Assert.AreEqual(120, r.MinutosEfectivos);
            Assert.IsNull(r.MinutosIncluidos);
        }

        [TestMethod]
        public void Resolver_NoAprobado_ExcluidoPorOmision()
        {
            // Cada estado no aprobado cae en su grupo y queda afuera, con el tipo
            // que da el horario, hasta que Nomina diga otra cosa.
            var casos = new[]
            {
                new { Estado = 0, TipoReg = 0, Grupo = "Sin solicitar" },
                new { Estado = 1, TipoReg = 1, Grupo = "Pendiente" },
                new { Estado = 2, TipoReg = 0, Grupo = "Aprobado sin tipo" },
                new { Estado = 3, TipoReg = 2, Grupo = "Rechazado" }
            };

            foreach (var caso in casos)
            {
                EntHeCandidato r = Resolver1(Cand(2, "2026-08-22 19:00", "2026-08-22 21:00", 2, caso.Estado, caso.TipoReg));

                Assert.IsFalse(r.Incluir, caso.Grupo);
                Assert.AreEqual(2, r.TipoElegido, caso.Grupo);
                Assert.AreEqual(caso.Grupo, r.Grupo);
                Assert.IsFalse(r.EsAprobado, caso.Grupo);
                Assert.AreEqual(0, r.MinutosEfectivos, caso.Grupo);
            }
        }

        [TestMethod]
        public void Resolver_AprobadoFueraDelPeriodoSinDuenio_ExcluidoPorOmision()
        {
            // Aprobado, pero del 14-08: no es de ningun periodo, asi que nadie lo
            // paga por omision.
            EntHeCandidato r = Resolver1(Aprobado(3, "2026-08-14 18:00", "2026-08-14 19:00", 1));

            Assert.IsFalse(r.Incluir);
            Assert.IsFalse(r.Bloqueado);
        }

        [TestMethod]
        public void Resolver_DecisionVigenteGana()
        {
            EntHeCandidato c = Cand(4, "2026-08-25 19:00", "2026-08-25 21:00", 1, 0);

            // La de IdDecision mas alto manda aunque venga primero en la lista.
            EntHeCandidato r = Resolver1(c,
                Dec(12, Este, c, true, 2, 90, "vigente"),
                Dec(10, Este, c, false, 1, null, "vieja"));

            Assert.IsTrue(r.Incluir);
            Assert.AreEqual(2, r.TipoElegido);
            Assert.AreEqual(90, r.MinutosIncluidos);
            Assert.AreEqual(90, r.MinutosEfectivos);
            Assert.AreEqual("vigente", r.Motivo);
            Assert.AreEqual("usuario12", r.DecididoPor);
            Assert.AreEqual(H("2026-09-20 10:00"), r.DecididoEl);
        }

        [TestMethod]
        public void Resolver_DecisionDeEstePeriodo_ExcluyeUnAprobado()
        {
            EntHeCandidato c = Aprobado(5, "2026-08-20 18:00", "2026-08-20 20:00", 1);

            EntHeCandidato r = Resolver1(c, Dec(20, Este, c, false, 1, null, "no corresponde"));

            Assert.IsFalse(r.Incluir);
            Assert.AreEqual(0, r.MinutosEfectivos);
        }

        [TestMethod]
        public void Resolver_AprobadoDeOtroPeriodo_Bloqueado()
        {
            // Aprobado el 10-08: su dueno es el periodo anterior, que lo paga por
            // omision. Incluirlo aqui seria pagarlo dos veces, aunque haya una
            // decision de este periodo que lo incluya.
            EntHeCandidato c = Aprobado(6, "2026-08-10 18:00", "2026-08-10 20:00", 1);

            EntHeCandidato sinDecision = Resolver1(c);
            Assert.IsTrue(sinDecision.Bloqueado);
            Assert.IsFalse(sinDecision.Incluir);
            Assert.AreEqual("incluido en el periodo 15/07/2026 - 13/08/2026", sinDecision.BloqueadoPor);

            EntHeCandidato conDecision = Resolver1(c, Dec(30, Este, c, true, 1));
            Assert.IsTrue(conDecision.Bloqueado);
            Assert.IsFalse(conDecision.Incluir, "Un bloqueado nunca queda incluido.");
            Assert.AreEqual(0, conDecision.MinutosEfectivos);
        }

        [TestMethod]
        public void Resolver_ExcluidoEnSuDuenio_SePuedeIncluirEnOtro()
        {
            EntHeCandidato c = Aprobado(7, "2026-08-10 18:00", "2026-08-10 20:00", 1);
            EntHeDecision excluidoAlla = Dec(40, Anterior, c, false, 1, null, "se paga en el siguiente");

            EntHeCandidato sinDecisionAqui = Resolver1(c, excluidoAlla);
            Assert.IsFalse(sinDecisionAqui.Bloqueado);
            Assert.IsFalse(sinDecisionAqui.Incluir, "No es dueno: por omision no lo incluye.");

            EntHeCandidato incluidoAqui = Resolver1(c, excluidoAlla, Dec(41, Este, c, true, 1));
            Assert.IsFalse(incluidoAqui.Bloqueado);
            Assert.IsTrue(incluidoAqui.Incluir);
            Assert.AreEqual(120, incluidoAqui.MinutosEfectivos);
        }

        [TestMethod]
        public void Resolver_IncluidoEnOtroPeriodoPorDecision_Bloqueado()
        {
            // Sin solicitar, del 14-08 (sin dueno): el periodo anterior lo incluyo
            // por decision, asi que aqui queda bloqueado.
            EntHeCandidato c = Cand(8, "2026-08-14 19:00", "2026-08-14 20:30", 1, 0);

            EntHeCandidato r = Resolver1(c, Dec(50, Anterior, c, true, 1, null, "lo trajo el anterior"),
                                            Dec(51, Este, c, true, 1, null, "tambien aqui"));

            Assert.IsTrue(r.Bloqueado);
            Assert.IsFalse(r.Incluir);
            Assert.AreEqual("incluido en el periodo 15/07/2026 - 13/08/2026", r.BloqueadoPor);
        }

        [TestMethod]
        public void Resolver_OtroPeriodoLoIncluyoYDespuesLoExcluyo_NoBloquea()
        {
            // Solo cuenta la decision vigente del otro periodo, no su historial.
            EntHeCandidato c = Cand(9, "2026-08-14 19:00", "2026-08-14 20:30", 1, 0);

            EntHeCandidato r = Resolver1(c, Dec(60, Anterior, c, true, 1, null, "primero si"),
                                            Dec(61, Anterior, c, false, 1, null, "despues no"));

            Assert.IsFalse(r.Bloqueado);
        }

        [TestMethod]
        public void Resolver_SinDuenio_SePuedeIncluir()
        {
            // El 15-08 no es de ningun periodo: nadie lo incluye por omision y
            // nadie lo bloquea.
            EntHeCandidato c = Aprobado(10, "2026-08-15 08:00", "2026-08-15 12:00", 2);

            EntHeCandidato porOmision = Resolver1(c);
            Assert.IsFalse(porOmision.Incluir);
            Assert.IsFalse(porOmision.Bloqueado);

            EntHeCandidato incluido = Resolver1(c, Dec(70, Este, c, true, 2));
            Assert.IsTrue(incluido.Incluir);
            Assert.IsFalse(incluido.Bloqueado);
            Assert.AreEqual(240, incluido.MinutosEfectivos);
        }

        [TestMethod]
        public void Resolver_PeriodoAnulado_NoEsDuenioNiBloquea()
        {
            // Un periodo anulado no paga: no es dueno del aprobado del 10-08 y su
            // Incluir = 1 sobre el otro tramo no bloquea a nadie.
            List<EntHeRangoPeriodo> periodos = new List<EntHeRangoPeriodo>
            {
                Periodo(9, "2026-07-15", "2026-08-13", "Anulado"),
                Periodo(Este, "2026-08-16", "2026-09-15")
            };
            EntHeCandidato aprobado = Aprobado(11, "2026-08-10 18:00", "2026-08-10 20:00", 1);
            EntHeCandidato sinSolicitar = Cand(12, "2026-08-14 19:00", "2026-08-14 20:00", 1, 0);

            List<EntHeCandidato> r = NegHeDecisiones.Resolver(Este,
                new List<EntHeCandidato> { aprobado, sinSolicitar },
                new List<EntHeDecision>
                {
                    Dec(80, 9, sinSolicitar, true, 1, null, "anulado"),
                    Dec(81, Este, aprobado, true, 1),
                    Dec(82, Este, sinSolicitar, true, 1, null, "se paga aqui")
                },
                periodos);

            Assert.IsFalse(r[0].Bloqueado);
            Assert.IsTrue(r[0].Incluir);
            Assert.IsFalse(r[1].Bloqueado);
            Assert.IsTrue(r[1].Incluir);
        }

        [TestMethod]
        public void Resolver_NoModificaLaEntrada()
        {
            EntHeCandidato c = Aprobado(13, "2026-08-20 18:00", "2026-08-20 20:00", 1);

            List<EntHeCandidato> r = NegHeDecisiones.Resolver(Este, new List<EntHeCandidato> { c },
                                                              new List<EntHeDecision>(), Periodos());

            Assert.IsTrue(r[0].Incluir);
            Assert.IsFalse(c.Incluir, "El candidato de entrada no se toca.");
            Assert.AreNotSame(c, r[0]);
        }

        // ================================================= Minutos ==========

        private static EntHeCandidato Incluido(long idTramo, string inicio, string fin, int tipo,
                                               long empleado = Persona, int? minutos = null)
        {
            EntHeCandidato c = Cand(idTramo, inicio, fin, tipo, 0, 0, empleado);
            c.Incluir = true;
            c.TipoElegido = tipo;
            c.MinutosIncluidos = minutos;
            return c;
        }

        [TestMethod]
        public void Minutos_UnionYCienGana()
        {
            // 18:00-20:00 al 50% y 19:00-21:00 al 100%: la hora compartida cuenta
            // una vez, al 100%. El excluido no suma y la otra persona va aparte.
            EntHeCandidato excluido = Cand(23, "2026-08-20 21:00", "2026-08-20 22:00", 1, 0);
            excluido.TipoElegido = 1;

            Dictionary<long, int[]> m = NegHeDecisiones.Minutos(new List<EntHeCandidato>
            {
                Incluido(21, "2026-08-20 18:00", "2026-08-20 20:00", 1),
                Incluido(22, "2026-08-20 19:00", "2026-08-20 21:00", 2),
                excluido,
                Incluido(24, "2026-08-20 19:00", "2026-08-20 20:00", 1, OtraPersona)
            });

            Assert.AreEqual(60, m[Persona][0]);
            Assert.AreEqual(120, m[Persona][1]);
            Assert.AreEqual(60, m[OtraPersona][0]);
            Assert.AreEqual(0, m[OtraPersona][1]);
        }

        [TestMethod]
        public void Minutos_MismoTipoSuperpuesto_CuentaUnaVez()
        {
            Dictionary<long, int[]> m = NegHeDecisiones.Minutos(new List<EntHeCandidato>
            {
                Incluido(25, "2026-08-21 18:00", "2026-08-21 19:00", 1),
                Incluido(26, "2026-08-21 18:30", "2026-08-21 19:30", 1)
            });

            Assert.AreEqual(90, m[Persona][0]);
            Assert.AreEqual(0, m[Persona][1]);
        }

        [TestMethod]
        public void Minutos_Reducidos_PrimerosN()
        {
            // 18:00-20:00 reducido a 30 minutos: entra 18:00-18:30, que se une con
            // 18:15-18:45. Si se tomaran los ultimos 30 (19:30-20:00) darian 60.
            Dictionary<long, int[]> m = NegHeDecisiones.Minutos(new List<EntHeCandidato>
            {
                Incluido(27, "2026-08-22 18:00", "2026-08-22 20:00", 1, Persona, 30),
                Incluido(28, "2026-08-22 18:15", "2026-08-22 18:45", 1)
            });

            Assert.AreEqual(45, m[Persona][0]);
        }

        [TestMethod]
        public void Minutos_TipoCambiado()
        {
            // El horario dice 50%, Nomina eligio 100%: manda lo elegido.
            EntHeCandidato c = Incluido(29, "2026-08-23 18:00", "2026-08-23 19:04", 1);
            c.TipoElegido = 2;

            Dictionary<long, int[]> m = NegHeDecisiones.Minutos(new List<EntHeCandidato> { c });

            Assert.AreEqual(0, m[Persona][0]);
            Assert.AreEqual(64, m[Persona][1]);
        }

        [TestMethod]
        public void Minutos_SegmentoBloqueadoNoSuma()
        {
            // Aprobado del periodo anterior con una decision de este periodo que lo
            // incluye: Resolver lo bloquea, y lo bloqueado no entra en el pago.
            EntHeCandidato c = Aprobado(30, "2026-08-10 18:00", "2026-08-10 20:00", 1);
            List<EntHeCandidato> r = NegHeDecisiones.Resolver(Este, new List<EntHeCandidato> { c },
                new List<EntHeDecision> { Dec(90, Este, c, true, 1) }, Periodos());

            Dictionary<long, int[]> m = NegHeDecisiones.Minutos(r);

            int total = m.ContainsKey(Persona) ? m[Persona][0] + m[Persona][1] : 0;
            Assert.AreEqual(0, total);
        }

        [TestMethod]
        public void Minutos_SegundosSeTruncanAlMinuto()
        {
            // Igual que DATEDIFF(MINUTE): 18:00:40 a 18:30:10 son 30 minutos.
            EntHeCandidato c = Incluido(31, "2026-08-24 18:00", "2026-08-24 18:30", 1);
            c.SegmentoInicio = c.SegmentoInicio.AddSeconds(40);
            c.SegmentoFin = c.SegmentoFin.AddSeconds(10);

            Dictionary<long, int[]> m = NegHeDecisiones.Minutos(new List<EntHeCandidato> { c });

            Assert.AreEqual(30, c.MinutosSegmento);
            Assert.AreEqual(30, m[Persona][0]);
        }

        // ================================================= Validar ==========

        [TestMethod]
        public void Validar_CambioValido_DevuelveNull()
        {
            EntHeCandidato c = Aprobado(40, "2026-08-20 18:00", "2026-08-20 20:00", 1);

            Assert.IsNull(Validar(c, Cambio(c, true, 1)));
            Assert.IsNull(Validar(c, Cambio(c, false, 2)));
        }

        [TestMethod]
        public void Validar_MinutosFueraDeRango()
        {
            EntHeCandidato c = Aprobado(41, "2026-08-20 18:00", "2026-08-20 20:00", 1);

            Assert.IsNotNull(Validar(c, Cambio(c, true, 1, 0, "reducido")));
            Assert.IsNotNull(Validar(c, Cambio(c, true, 1, -5, "reducido")));
            Assert.IsNotNull(Validar(c, Cambio(c, true, 1, 121, "subir no se puede")));
            Assert.IsNull(Validar(c, Cambio(c, true, 1, 120, "completo")));
            Assert.IsNull(Validar(c, Cambio(c, true, 1, 1, "un minuto")));
        }

        [TestMethod]
        public void Validar_MotivoObligatorio()
        {
            EntHeCandidato aprobado = Aprobado(42, "2026-08-20 18:00", "2026-08-20 20:00", 1);
            EntHeCandidato sinSolicitar = Cand(43, "2026-08-21 19:00", "2026-08-21 20:00", 1, 0);

            // Reducir minutos exige motivo, aun en un aprobado.
            Assert.IsNotNull(Validar(aprobado, Cambio(aprobado, true, 1, 60, "")));
            Assert.IsNotNull(Validar(aprobado, Cambio(aprobado, true, 1, 60, "   ")));
            // Incluir algo no aprobado exige motivo.
            Assert.IsNotNull(Validar(sinSolicitar, Cambio(sinSolicitar, true, 1)));
            Assert.IsNull(Validar(sinSolicitar, Cambio(sinSolicitar, true, 1, null, "lo pidio la jefatura")));
            // Excluir algo no aprobado, o incluir un aprobado completo, no.
            Assert.IsNull(Validar(sinSolicitar, Cambio(sinSolicitar, false, 1)));
            Assert.IsNull(Validar(aprobado, Cambio(aprobado, true, 1)));
        }

        [TestMethod]
        public void Validar_Bloqueado()
        {
            EntHeCandidato c = Aprobado(44, "2026-08-10 18:00", "2026-08-10 20:00", 1);
            c.Bloqueado = true;
            c.BloqueadoPor = "incluido en el periodo 15/07/2026 - 13/08/2026";

            string error = Validar(c, Cambio(c, true, 1));

            Assert.IsNotNull(error);
            StringAssert.Contains(error, "incluido en el periodo 15/07/2026 - 13/08/2026");
            // Dejarlo afuera si se puede.
            Assert.IsNull(Validar(c, Cambio(c, false, 1)));
        }

        [TestMethod]
        public void Validar_TipoInvalido()
        {
            EntHeCandidato c = Aprobado(45, "2026-08-20 18:00", "2026-08-20 20:00", 1);

            Assert.IsNotNull(Validar(c, Cambio(c, true, 0)));
            Assert.IsNotNull(Validar(c, Cambio(c, true, 3)));
            Assert.IsNotNull(Validar(c, Cambio(c, false, 0)));
        }

        [TestMethod]
        public void Validar_CambioDeOtraPersona()
        {
            EntHeCandidato c = Aprobado(46, "2026-08-20 18:00", "2026-08-20 20:00", 1);

            // El segmento existe, pero es de otra persona que la que se esta editando.
            Assert.IsNotNull(NegHeDecisiones.Validar(new List<EntHeCandidato> { c }, OtraPersona,
                             new List<EntHeCambioDecision> { Cambio(c, true, 1) }));

            // Un SegmentoInicio que no es el del candidato tampoco es candidato.
            EntHeCambioDecision otroInicio = Cambio(c, true, 1);
            otroInicio.SegmentoInicio = c.SegmentoInicio.AddMinutes(30);
            Assert.IsNotNull(Validar(c, otroInicio));
        }

        [TestMethod]
        public void Validar_ListaVaciaORepetido()
        {
            EntHeCandidato c = Aprobado(47, "2026-08-20 18:00", "2026-08-20 20:00", 1);

            Assert.IsNotNull(Validar(c));
            Assert.IsNotNull(NegHeDecisiones.Validar(new List<EntHeCandidato> { c }, Persona, null));
            Assert.IsNotNull(Validar(c, Cambio(c, true, 1), Cambio(c, false, 1)));
        }

        [TestMethod]
        public void Validar_DevuelveElPrimerError()
        {
            EntHeCandidato a = Aprobado(48, "2026-08-20 18:00", "2026-08-20 20:00", 1);
            EntHeCandidato b = Cand(49, "2026-08-21 19:00", "2026-08-21 20:00", 1, 0);

            string error = NegHeDecisiones.Validar(new List<EntHeCandidato> { a, b }, Persona,
                new List<EntHeCambioDecision> { Cambio(a, true, 1), Cambio(b, true, 7) });

            Assert.IsNotNull(error);
            StringAssert.Contains(error, "21/08/2026 19:00");
        }

        // ================================================= Conciliar ========

        /// <summary>
        /// Un segmento ya resuelto, dentro del periodo salvo que se diga otra cosa.
        /// estado 2 con tipo registrado = aprobado; cualquier otro, no aprobado.
        /// </summary>
        private static EntHeCandidato Pieza(long idTramo, string inicio, string fin, int tipo, int estado, bool incluir,
                                            long empleado = Persona, bool dentro = true, bool bloqueado = false)
        {
            EntHeCandidato c = Cand(idTramo, inicio, fin, tipo, estado, estado == 2 ? tipo : 0, empleado);
            c.Incluir = incluir;
            c.TipoElegido = tipo;
            c.DentroDelPeriodo = dentro;
            c.Bloqueado = bloqueado;
            return c;
        }

        /// <summary>Valor hora inventado: 7,50 al 50% y 10,00 al 100%.</summary>
        private static EntHeFila Fila(long empleado, string nombre, decimal totalHE = 0m,
                                      decimal valor50 = 7.5m, decimal valor100 = 10m)
        {
            return new EntHeFila
            {
                IdEmpleado = empleado,
                NombreSnapshot = nombre,
                ValorHora50 = valor50,
                ValorHora100 = valor100,
                TotalHE = totalHE
            };
        }

        private static EntHeConciliacionFila Conciliar1(EntHeFila fila, params EntHeCandidato[] resueltos)
        {
            List<EntHeConciliacionFila> r = NegHeDecisiones.Conciliar(resueltos.ToList(), new List<EntHeFila> { fila });
            Assert.AreEqual(1, r.Count);
            return r[0];
        }

        [TestMethod]
        public void Conciliar_SeparaAprobadoNoAprobadoYExcluido()
        {
            EntHeConciliacionFila r = Conciliar1(Fila(Persona, "PERSONA A"),
                Pieza(61, "2026-08-20 18:00", "2026-08-20 20:00", 1, 2, true),   // aprobado incluido: 120 al 50%
                Pieza(62, "2026-08-21 19:00", "2026-08-21 19:30", 2, 0, true),   // no aprobado incluido: 30 al 100%
                Pieza(63, "2026-08-22 20:00", "2026-08-22 21:30", 1, 3, false),  // excluido: 90 al 50%
                Pieza(64, "2026-08-23 20:00", "2026-08-23 21:00", 1, 0, false, bloqueado: true));

            Assert.AreEqual(120, r.MinutosAprobadoIncluido);
            Assert.AreEqual(30, r.MinutosNoAprobadoIncluido);
            Assert.AreEqual(90, r.MinutosExcluido, "Lo bloqueado lo paga otro periodo: no es excluido aqui.");
            Assert.AreEqual(15.00m, r.UsdAprobadoIncluido);
            Assert.AreEqual(5.00m, r.UsdNoAprobadoIncluido);
            Assert.AreEqual(11.25m, r.UsdExcluido);
        }

        [TestMethod]
        public void Conciliar_TotalAPagarEsTotalHE()
        {
            // El total a pagar es el de la fila, no una suma nueva: la conciliacion
            // explica el pago, no lo recalcula.
            List<EntHeConciliacionFila> r = NegHeDecisiones.Conciliar(
                new List<EntHeCandidato> { Pieza(65, "2026-08-20 18:00", "2026-08-20 20:00", 1, 2, true) },
                new List<EntHeFila> { Fila(Persona, "PERSONA A", 123.45m), Fila(OtraPersona, "PERSONA B", 7.01m) });

            Assert.AreEqual(2, r.Count);
            Assert.AreEqual(Persona, r[0].IdEmpleado);
            Assert.AreEqual("PERSONA A", r[0].Nombre);
            Assert.AreEqual(123.45m, r[0].TotalAPagar);
            Assert.AreEqual(OtraPersona, r[1].IdEmpleado);
            Assert.AreEqual(7.01m, r[1].TotalAPagar);
        }

        [TestMethod]
        public void Conciliar_PersonaSinCandidatos_Ceros()
        {
            EntHeConciliacionFila r = Conciliar1(Fila(OtraPersona, "PERSONA B", 0m),
                Pieza(66, "2026-08-20 18:00", "2026-08-20 20:00", 1, 2, true));

            Assert.AreEqual(OtraPersona, r.IdEmpleado);
            Assert.AreEqual(0, r.MinutosAprobadoIncluido);
            Assert.AreEqual(0, r.MinutosNoAprobadoIncluido);
            Assert.AreEqual(0, r.MinutosExcluido);
            Assert.AreEqual(0m, r.UsdAprobadoIncluido);
            Assert.AreEqual(0m, r.UsdNoAprobadoIncluido);
            Assert.AreEqual(0m, r.UsdExcluido);
            Assert.AreEqual(0m, r.TotalAPagar);
        }

        [TestMethod]
        public void Conciliar_MinutoIncluidoNoCuentaComoExcluido()
        {
            // 18:00-20:00 incluido; 19:00-21:00 excluido al 100%: solo 20:00-21:00
            // queda afuera. La hora compartida ya se paga.
            EntHeConciliacionFila r = Conciliar1(Fila(Persona, "PERSONA A"),
                Pieza(67, "2026-08-20 18:00", "2026-08-20 20:00", 1, 2, true),
                Pieza(68, "2026-08-20 19:00", "2026-08-20 21:00", 2, 0, false));

            Assert.AreEqual(120, r.MinutosAprobadoIncluido);
            Assert.AreEqual(60, r.MinutosExcluido);
            Assert.AreEqual(10.00m, r.UsdExcluido);
        }

        [TestMethod]
        public void Conciliar_IncluidoReducido_ElRestoEsExcluido()
        {
            // Se pagan los primeros 30 de 60 minutos: los otros 30 no se pagan, y
            // eso es exactamente lo que Nomina decidio dejar afuera.
            EntHeCandidato reducido = Pieza(69, "2026-08-20 19:00", "2026-08-20 20:00", 1, 0, true);
            reducido.MinutosIncluidos = 30;

            EntHeConciliacionFila r = Conciliar1(Fila(Persona, "PERSONA A"), reducido);

            Assert.AreEqual(30, r.MinutosNoAprobadoIncluido);
            Assert.AreEqual(30, r.MinutosExcluido);
        }

        [TestMethod]
        public void Conciliar_FueraDelPeriodoSinIncluir_NoCuentaComoExcluido()
        {
            // Los 7 dias de borde son del periodo vecino: si no se incluyen aqui, su
            // tiempo no es "no incluido" de este periodo, o se contaria en los dos.
            // Incluido, en cambio, si suma.
            EntHeConciliacionFila r = Conciliar1(Fila(Persona, "PERSONA A"),
                Pieza(70, "2026-09-16 19:00", "2026-09-16 20:00", 1, 0, false, dentro: false),
                Pieza(71, "2026-09-17 19:00", "2026-09-17 20:00", 1, 0, true, dentro: false));

            Assert.AreEqual(0, r.MinutosExcluido);
            Assert.AreEqual(60, r.MinutosNoAprobadoIncluido);
        }

        [TestMethod]
        public void Conciliar_UsdSeRedondeaUnaVezPorPersona()
        {
            // 10 minutos a 7,53 = 1,255 cada uno. Redondeando cada pieza: 1,26 + 1,26
            // = 2,52; redondeando una vez la suma: 2,51. Manda el segundo.
            EntHeConciliacionFila r = Conciliar1(Fila(Persona, "PERSONA A", 0m, 7.53m, 10m),
                Pieza(72, "2026-08-20 19:00", "2026-08-20 19:10", 1, 0, false),
                Pieza(73, "2026-08-21 19:00", "2026-08-21 19:10", 1, 0, false));

            Assert.AreEqual(20, r.MinutosExcluido);
            Assert.AreEqual(2.51m, r.UsdExcluido);
        }

        [TestMethod]
        public void Conciliar_CienGanaAlCincuentaEnElMismoMinuto()
        {
            EntHeConciliacionFila r = Conciliar1(Fila(Persona, "PERSONA A"),
                Pieza(74, "2026-08-20 18:00", "2026-08-20 20:00", 1, 0, true),
                Pieza(75, "2026-08-20 19:00", "2026-08-20 20:00", 2, 0, true));

            Assert.AreEqual(120, r.MinutosNoAprobadoIncluido);
            Assert.AreEqual(17.50m, r.UsdNoAprobadoIncluido, "60 al 50% (7,50) + 60 al 100% (10,00).");
        }

        [TestMethod]
        public void Conciliar_ListasNulas_SinFilas()
        {
            Assert.AreEqual(0, NegHeDecisiones.Conciliar(null, null).Count);
            Assert.AreEqual(1, NegHeDecisiones.Conciliar(null, new List<EntHeFila> { Fila(Persona, "PERSONA A") }).Count);
        }

        // ================================================= Valorar ==========

        [TestMethod]
        public void Valorar_IncluidoUsaMinutosEfectivos_ExcluidoUsaElSegmento()
        {
            EntHeCandidato reducido = Pieza(76, "2026-08-20 19:00", "2026-08-20 20:00", 2, 0, true);
            reducido.MinutosIncluidos = 45;
            EntHeCandidato excluido = Pieza(77, "2026-08-21 19:00", "2026-08-21 20:30", 1, 0, false);
            EntHeCandidato ajeno = Pieza(78, "2026-08-21 19:00", "2026-08-21 20:00", 1, 0, false, OtraPersona);

            NegHeDecisiones.Valorar(new List<EntHeCandidato> { reducido, excluido, ajeno },
                                    new List<EntHeFila> { Fila(Persona, "PERSONA A") });

            Assert.AreEqual(7.50m, reducido.ValorUsd, "45 minutos al 100% (10,00).");
            Assert.AreEqual(11.25m, excluido.ValorUsd, "90 minutos al 50% (7,50).");
            Assert.AreEqual(0m, ajeno.ValorUsd, "Sin fila no hay valor hora.");
        }

        // ================================================= Textos ===========

        [TestMethod]
        public void Candidato_ClaveSegmentoYTextos()
        {
            EntHeCandidato c = Cand(79, "2026-08-20 18:00", "2026-08-20 21:15", 1, 0);
            c.SegmentoInicio = H("2026-08-20 19:05");

            Assert.AreEqual("2026-08-20T19:05:00", c.ClaveSegmento);
            Assert.AreEqual("20/08/2026", c.Fecha);
            Assert.AreEqual("18:00 - 21:15", c.TramoTexto);
            Assert.AreEqual("19:05 - 21:15", c.ParteTexto);
        }

        // ================================================= Pantalla =========

        [TestMethod]
        public void MensajeDecisiones_TraduceCadaCodigo()
        {
            StringAssert.Contains(NegHeDecisionesPantalla.MensajeDecisiones(0), "guardadas");
            StringAssert.Contains(NegHeDecisionesPantalla.MensajeDecisiones(-1), "abierto");
            StringAssert.Contains(NegHeDecisionesPantalla.MensajeDecisiones(-2), "otro per");
            StringAssert.Contains(NegHeDecisionesPantalla.MensajeDecisiones(-3), "inválid");
            StringAssert.Contains(NegHeDecisionesPantalla.MensajeDecisiones(-4), "candidato");
            StringAssert.Contains(NegHeDecisionesPantalla.MensajeDecisiones(-99), "No se pudieron guardar");
        }

        [TestMethod]
        public void ArmarXml_UnElementoPorCambio()
        {
            string xml = NegHeDecisionesPantalla.ArmarXml(new List<EntHeCambioDecision>
            {
                new EntHeCambioDecision { IdRegDetTareas = 80, SegmentoInicio = H("2026-08-20 19:05"), Incluir = true,
                                          TipoHora = 2, MinutosIncluidos = 45, Motivo = "  a <b> & \"c\"  " },
                new EntHeCambioDecision { IdRegDetTareas = 81, SegmentoInicio = H("2026-08-21 18:00"), Incluir = false,
                                          TipoHora = 1, MinutosIncluidos = null, Motivo = null }
            });

            System.Xml.Linq.XElement raiz = System.Xml.Linq.XElement.Parse(xml);
            Assert.AreEqual("ds", raiz.Name.LocalName);
            List<System.Xml.Linq.XElement> d = raiz.Elements("d").ToList();
            Assert.AreEqual(2, d.Count);

            Assert.AreEqual("80", (string)d[0].Attribute("t"));
            Assert.AreEqual("2026-08-20T19:05:00", (string)d[0].Attribute("s"));
            Assert.AreEqual("1", (string)d[0].Attribute("i"));
            Assert.AreEqual("2", (string)d[0].Attribute("h"));
            Assert.AreEqual("45", (string)d[0].Attribute("m"));
            Assert.AreEqual("a <b> & \"c\"", (string)d[0].Attribute("r"));

            Assert.AreEqual("0", (string)d[1].Attribute("i"));
            Assert.AreEqual("1", (string)d[1].Attribute("h"));
            Assert.AreEqual("", (string)d[1].Attribute("m"), "Sin minutos va vacio: el segmento completo.");
            Assert.AreEqual("", (string)d[1].Attribute("r"));
        }
    }
}
