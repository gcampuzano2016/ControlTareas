using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaPruebas
{
    /// <summary>
    /// La aritmetica de minutos del tiempo fuera de horario. Los datos son
    /// inventados: el repositorio es publico.
    ///
    /// Cada caso describe una situacion real que salio en la validacion del
    /// periodo 16-08 / 15-09-2026, con otras personas y otras horas.
    /// </summary>
    [TestClass]
    public class NegHeFueraDeHorarioTests
    {
        private const long Persona = 101;
        private const long OtraPersona = 202;

        private static DateTime H(string texto)
        {
            return DateTime.ParseExact(texto, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }

        private static EntHeSegmentoFuera Seg(long idTramo, long empleado, string inicio, string fin,
                                             int tipoHora, int estado, int tipoRegistrado = 0)
        {
            return new EntHeSegmentoFuera
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

        private static EntHeIntervaloPagado Pagado(long empleado, string inicio, string fin)
        {
            return new EntHeIntervaloPagado { IdEmpleado = empleado, Inicio = H(inicio), Fin = H(fin) };
        }

        private static EntHeFueraDeHorario Calcular(List<EntHeSegmentoFuera> s, List<EntHeIntervaloPagado> p = null)
        {
            return NegHeFueraDeHorario.Calcular(s, p ?? new List<EntHeIntervaloPagado>());
        }

        [TestMethod]
        public void SegmentoSolo_SumaCompleto()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 08:00", "2026-08-18 08:30", 1, 0)
            });

            Assert.IsTrue(r.Calculado);
            Assert.AreEqual(30, r.DePersona(Persona).MinutosSinAprobar50);
            Assert.AreEqual(1, r.Detalle.Count);
            Assert.AreEqual("08:00 - 08:30", r.Detalle[0].FueraTexto);
            Assert.AreEqual("Sin solicitar", r.Detalle[0].EstadoTexto);
        }

        [TestMethod]
        public void PagadoQueCubreTodo_NoSuma()
        {
            // El caso de un tramo pagado de 08:00 a 08:30 dentro de uno normal de
            // 08:00 a 17:30: el segmento fuera de horario del normal ya esta pagado.
            EntHeFueraDeHorario r = Calcular(
                new List<EntHeSegmentoFuera> { Seg(1, Persona, "2026-09-14 08:00", "2026-09-14 08:30", 1, 0) },
                new List<EntHeIntervaloPagado> { Pagado(Persona, "2026-09-14 08:00", "2026-09-14 08:30") });

            Assert.IsNull(r.DePersona(Persona), "Una persona sin nada pendiente no deberia aparecer.");
            Assert.AreEqual(0, r.Detalle.Count);
        }

        [TestMethod]
        public void PagadoParcial_DescuentaSoloLaSuperposicion()
        {
            EntHeFueraDeHorario r = Calcular(
                new List<EntHeSegmentoFuera> { Seg(1, Persona, "2026-08-18 17:30", "2026-08-18 18:30", 1, 0) },
                new List<EntHeIntervaloPagado> { Pagado(Persona, "2026-08-18 18:00", "2026-08-18 19:00") });

            Assert.AreEqual(30, r.DePersona(Persona).MinutosSinAprobar50);
            Assert.AreEqual(1, r.Detalle.Count);
            Assert.AreEqual("17:30 - 18:00", r.Detalle[0].FueraTexto);
            Assert.AreEqual("17:30 - 18:30", r.Detalle[0].TramoTexto, "El tramo registrado se muestra entero.");
        }

        [TestMethod]
        public void PagadoEnMedio_ParteElSegmentoEnDos()
        {
            EntHeFueraDeHorario r = Calcular(
                new List<EntHeSegmentoFuera> { Seg(1, Persona, "2026-08-18 17:30", "2026-08-18 19:30", 1, 0) },
                new List<EntHeIntervaloPagado> { Pagado(Persona, "2026-08-18 18:00", "2026-08-18 19:00") });

            Assert.AreEqual(60, r.DePersona(Persona).MinutosSinAprobar50);
            Assert.AreEqual(2, r.Detalle.Count);
            Assert.AreEqual("17:30 - 18:00", r.Detalle[0].FueraTexto);
            Assert.AreEqual("19:00 - 19:30", r.Detalle[1].FueraTexto);
        }

        [TestMethod]
        public void DosTramosSobreElMismoMinuto_CuentanUnaVez()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 08:00", "2026-08-18 08:30", 1, 0),
                Seg(2, Persona, "2026-08-18 08:00", "2026-08-18 08:30", 1, 1)
            });

            Assert.AreEqual(30, r.DePersona(Persona).MinutosSinAprobar50);
            Assert.AreEqual(1, r.Detalle.Count);
        }

        [TestMethod]
        public void SinAprobarGanaARechazado()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 08:15", "2026-08-18 08:45", 1, 3),
                Seg(2, Persona, "2026-08-18 08:00", "2026-08-18 08:30", 1, 0)
            });

            EntHeFueraDeHorarioPersona p = r.DePersona(Persona);
            Assert.AreEqual(30, p.MinutosSinAprobar50);
            Assert.AreEqual(15, p.MinutosRechazado50, "Del rechazado solo queda 08:30-08:45.");
        }

        [TestMethod]
        public void CienGanaACincuentaEnLaMismaCategoria()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 17:30", "2026-08-18 18:30", 1, 0),
                Seg(2, Persona, "2026-08-18 18:00", "2026-08-18 18:30", 2, 0)
            });

            EntHeFueraDeHorarioPersona p = r.DePersona(Persona);
            Assert.AreEqual(30, p.MinutosSinAprobar100);
            Assert.AreEqual(30, p.MinutosSinAprobar50);
        }

        [TestMethod]
        public void SegmentoDeTramoPagado_SeIgnora()
        {
            // Defensa: el SQL no deberia mandarlo, pero si llega no puede contar como pendiente.
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 18:00", "2026-08-18 19:00", 1, 2, tipoRegistrado: 1)
            });

            Assert.IsNull(r.DePersona(Persona));
        }

        [TestMethod]
        public void AprobadoSinTipo_CuentaComoSinAprobar()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 18:00", "2026-08-18 19:00", 1, 2, tipoRegistrado: 0)
            });

            Assert.AreEqual(60, r.DePersona(Persona).MinutosSinAprobar50);
            Assert.AreEqual("Aprobado sin tipo", r.Detalle[0].EstadoTexto);
            Assert.AreEqual(NegHeFueraDeHorario.CategoriaSinAprobar, r.Detalle[0].Categoria);
        }

        [TestMethod]
        public void Rechazado_SeSeparaPorTipo()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-09-05 11:30", "2026-09-05 12:25", 2, 3)
            });

            EntHeFueraDeHorarioPersona p = r.DePersona(Persona);
            Assert.AreEqual(55, p.MinutosRechazado100);
            Assert.AreEqual(0, p.MinutosSinAprobar50 + p.MinutosSinAprobar100);
            Assert.AreEqual("Rechazado", r.Detalle[0].EstadoTexto);
            Assert.AreEqual(1, r.PersonasConRechazado);
            Assert.AreEqual(0, r.PersonasConSinAprobar);
        }

        [TestMethod]
        public void Horas_SeRedondeanUnaSolaVez()
        {
            // Tres pedazos de 20 min: 60 min = 1,00 h. Redondeando cada uno (0,33 x 3) saldria 0,99.
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 07:00", "2026-08-18 07:20", 1, 0),
                Seg(2, Persona, "2026-08-19 07:00", "2026-08-19 07:20", 1, 0),
                Seg(3, Persona, "2026-08-20 07:00", "2026-08-20 07:20", 1, 0)
            });

            Assert.AreEqual(1.00m, r.DePersona(Persona).HorasSinAprobar);
            Assert.AreEqual(1.00m, r.TotalHorasSinAprobar);
        }

        [TestMethod]
        public void SinSegmentos_Calculado_YVacio()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>());

            Assert.IsTrue(r.Calculado, "Vacio es un resultado valido, no un error.");
            Assert.AreEqual(0, r.Personas.Count);
            Assert.AreEqual(0m, r.TotalHorasSinAprobar);
        }

        [TestMethod]
        public void NulosSeTratanComoListasVacias()
        {
            EntHeFueraDeHorario r = NegHeFueraDeHorario.Calcular(null, null);

            Assert.IsTrue(r.Calculado);
            Assert.AreEqual(0, r.Personas.Count);
        }

        [TestMethod]
        public void PagadoDeOtraPersona_NoDescuenta()
        {
            EntHeFueraDeHorario r = Calcular(
                new List<EntHeSegmentoFuera> { Seg(1, Persona, "2026-08-18 08:00", "2026-08-18 08:30", 1, 0) },
                new List<EntHeIntervaloPagado> { Pagado(OtraPersona, "2026-08-18 08:00", "2026-08-18 08:30") });

            Assert.AreEqual(30, r.DePersona(Persona).MinutosSinAprobar50);
        }

        [TestMethod]
        public void SegmentoVacioOAlReves_SeIgnora()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 08:30", "2026-08-18 08:30", 1, 0),
                Seg(2, Persona, "2026-08-18 09:00", "2026-08-18 08:00", 1, 0)
            });

            Assert.IsNull(r.DePersona(Persona));
        }

        [TestMethod]
        public void Detalle_OrdenadoPorEmpleadoYHora()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(3, OtraPersona, "2026-08-18 07:00", "2026-08-18 07:30", 1, 0),
                Seg(2, Persona, "2026-08-19 07:00", "2026-08-19 07:30", 1, 0),
                Seg(1, Persona, "2026-08-18 18:00", "2026-08-18 18:30", 1, 3)
            });

            Assert.AreEqual(3, r.Detalle.Count);
            Assert.AreEqual(1L, r.Detalle[0].IdRegDetTareas);
            Assert.AreEqual(2L, r.Detalle[1].IdRegDetTareas);
            Assert.AreEqual(3L, r.Detalle[2].IdRegDetTareas);
            Assert.AreEqual(2, r.Personas.Count);
            Assert.AreEqual(Persona, r.Personas[0].IdEmpleado);
        }

        // ------------------------------------------- personas fuera del modulo --

        private static EntHeFueraDelModuloPersona PersonaModulo(long id, string nombre)
        {
            return new EntHeFueraDelModuloPersona { IdEmpleado = id, Nombre = nombre, Cargo = "CARGO DE PRUEBA" };
        }

        private static EntHeIntervaloPagado PagadoTipo(long empleado, string inicio, string fin, int tipo)
        {
            return new EntHeIntervaloPagado { IdEmpleado = empleado, Inicio = H(inicio), Fin = H(fin), TipoRegistrado = tipo };
        }

        [TestMethod]
        public void FueraDelModulo_AprobadasAlCien_SeSuman()
        {
            // El caso de alguien con horas ya aprobadas por su jefe que no esta en el modulo.
            List<EntHeFueraDelModuloPersona> r = NegHeFueraDeHorario.ResumirFueraDelModulo(
                new List<EntHeFueraDelModuloPersona> { PersonaModulo(Persona, "PERSONA A") },
                new List<EntHeSegmentoFuera>(),
                new List<EntHeIntervaloPagado>
                {
                    PagadoTipo(Persona, "2026-08-22 22:00", "2026-08-22 23:59", 2),
                    PagadoTipo(Persona, "2026-08-23 00:00", "2026-08-23 08:30", 2)
                });

            Assert.AreEqual(1, r.Count);
            Assert.AreEqual(119 + 510, r[0].MinutosAprobados100);
            Assert.AreEqual(0, r[0].MinutosAprobados50);
            Assert.AreEqual("CARGO DE PRUEBA", r[0].Cargo);
        }

        [TestMethod]
        public void FueraDelModulo_AprobadasSuperpuestas_CuentanUnaVez()
        {
            List<EntHeFueraDelModuloPersona> r = NegHeFueraDeHorario.ResumirFueraDelModulo(
                new List<EntHeFueraDelModuloPersona> { PersonaModulo(Persona, "PERSONA A") },
                null,
                new List<EntHeIntervaloPagado>
                {
                    PagadoTipo(Persona, "2026-08-18 18:00", "2026-08-18 19:00", 1),
                    PagadoTipo(Persona, "2026-08-18 18:30", "2026-08-18 19:30", 1)
                });

            Assert.AreEqual(90, r[0].MinutosAprobados50);
        }

        [TestMethod]
        public void FueraDelModulo_AprobadoCienGanaACincuenta()
        {
            List<EntHeFueraDelModuloPersona> r = NegHeFueraDeHorario.ResumirFueraDelModulo(
                new List<EntHeFueraDelModuloPersona> { PersonaModulo(Persona, "PERSONA A") },
                null,
                new List<EntHeIntervaloPagado>
                {
                    PagadoTipo(Persona, "2026-08-18 18:00", "2026-08-18 19:00", 1),
                    PagadoTipo(Persona, "2026-08-18 18:30", "2026-08-18 19:00", 2)
                });

            Assert.AreEqual(30, r[0].MinutosAprobados50);
            Assert.AreEqual(30, r[0].MinutosAprobados100);
        }

        [TestMethod]
        public void FueraDelModulo_SinAprobarYRechazado()
        {
            List<EntHeFueraDelModuloPersona> r = NegHeFueraDeHorario.ResumirFueraDelModulo(
                new List<EntHeFueraDelModuloPersona> { PersonaModulo(Persona, "PERSONA A") },
                new List<EntHeSegmentoFuera>
                {
                    Seg(1, Persona, "2026-08-17 07:30", "2026-08-17 08:30", 1, 0),
                    Seg(2, Persona, "2026-08-17 05:28", "2026-08-17 05:42", 2, 0),
                    Seg(3, Persona, "2026-08-18 18:00", "2026-08-18 18:20", 1, 3)
                },
                null);

            Assert.AreEqual(60, r[0].MinutosSinAprobar50);
            Assert.AreEqual(14, r[0].MinutosSinAprobar100);
            Assert.AreEqual(20, r[0].MinutosRechazado50);
            Assert.AreEqual(1.23m, r[0].HorasSinAprobar);
            StringAssert.Contains(r[0].Motivo, "faltan sus datos de RRHH");
        }

        [TestMethod]
        public void FueraDelModulo_PersonaSinNada_NoAparece()
        {
            List<EntHeFueraDelModuloPersona> r = NegHeFueraDeHorario.ResumirFueraDelModulo(
                new List<EntHeFueraDelModuloPersona> { PersonaModulo(Persona, "PERSONA A") }, null, null);

            Assert.AreEqual(0, r.Count);
        }

        [TestMethod]
        public void FueraDelModulo_NulosYOrdenPorNombre()
        {
            Assert.AreEqual(0, NegHeFueraDeHorario.ResumirFueraDelModulo(null, null, null).Count);

            List<EntHeFueraDelModuloPersona> r = NegHeFueraDeHorario.ResumirFueraDelModulo(
                new List<EntHeFueraDelModuloPersona> { PersonaModulo(Persona, "ZETA"), PersonaModulo(OtraPersona, "ALFA") },
                null,
                new List<EntHeIntervaloPagado>
                {
                    PagadoTipo(Persona, "2026-08-18 18:00", "2026-08-18 19:00", 1),
                    PagadoTipo(OtraPersona, "2026-08-18 18:00", "2026-08-18 19:00", 1)
                });

            Assert.AreEqual("ALFA", r[0].Nombre);
            Assert.AreEqual("ZETA", r[1].Nombre);
        }
    }
}
