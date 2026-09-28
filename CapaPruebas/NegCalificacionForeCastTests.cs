using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace CapaPruebas
{
    /// <summary>
    /// Las reglas de la calificacion del Forecast. La fecha de hoy entra como
    /// parametro: ninguna prueba depende del reloj.
    /// </summary>
    [TestClass]
    public class NegCalificacionForeCastTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 9, 28);
        private CultureInfo _culturaOriginal;

        /// <summary>Cultura hostil: coma decimal y meses en espanol.</summary>
        [TestInitialize]
        public void FijarCulturaHostil()
        {
            _culturaOriginal = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
        }

        [TestCleanup]
        public void RestaurarCultura()
        {
            Thread.CurrentThread.CurrentCulture = _culturaOriginal;
        }

        /* ---------------- Tiempo ---------------- */

        [TestMethod]
        public void Tiempo_SinFecha_Cero()
        {
            Assert.AreEqual(0, NegCalificacionForeCast.PuntosTiempo(null, Hoy));
        }

        [TestMethod]
        public void Tiempo_Tramos()
        {
            Assert.AreEqual(3,  NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(-1), Hoy));
            Assert.AreEqual(15, NegCalificacionForeCast.PuntosTiempo(Hoy, Hoy));
            Assert.AreEqual(15, NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(60), Hoy));
            Assert.AreEqual(12, NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(61), Hoy));
            Assert.AreEqual(12, NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(120), Hoy));
            Assert.AreEqual(8,  NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(121), Hoy));
            Assert.AreEqual(8,  NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(240), Hoy));
            Assert.AreEqual(5,  NegCalificacionForeCast.PuntosTiempo(Hoy.AddDays(241), Hoy));
        }

        [TestMethod]
        public void Tiempo_IgnoraLaHora()
        {
            // Cierre hoy a las 23:59 sigue siendo "hoy", no "manana".
            Assert.AreEqual(15, NegCalificacionForeCast.PuntosTiempo(Hoy.AddHours(23.9), Hoy.AddHours(8)));
        }

        /* ---------------- Actividad ---------------- */

        [TestMethod]
        public void UltimaActividad_TomaLaMasReciente()
        {
            DateTime a = Hoy.AddDays(-40), b = Hoy.AddDays(-3);
            Assert.AreEqual(b, NegCalificacionForeCast.UltimaActividad(a, b));
            Assert.AreEqual(b, NegCalificacionForeCast.UltimaActividad(b, a));
            Assert.AreEqual(a, NegCalificacionForeCast.UltimaActividad(a, null));
            Assert.AreEqual(b, NegCalificacionForeCast.UltimaActividad(null, b));
            Assert.IsNull(NegCalificacionForeCast.UltimaActividad(null, null));
        }

        [TestMethod]
        public void Actividad_Tramos()
        {
            Assert.AreEqual(0,  NegCalificacionForeCast.PuntosActividad(null, Hoy));
            Assert.AreEqual(15, NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-14), Hoy));
            Assert.AreEqual(10, NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-15), Hoy));
            Assert.AreEqual(10, NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-30), Hoy));
            Assert.AreEqual(5,  NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-31), Hoy));
            Assert.AreEqual(5,  NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-60), Hoy));
            Assert.AreEqual(0,  NegCalificacionForeCast.PuntosActividad(Hoy.AddDays(-61), Hoy));
        }

        /* ---------------- Encaje ---------------- */

        private static List<EntTasaExito> Tasas()
        {
            return new List<EntTasaExito>
            {
                new EntTasaExito { Dimension = "SEGMENTO", Valor = "Alto",    Ganados = 60, Perdidos = 40 },  // 60 %
                new EntTasaExito { Dimension = "SEGMENTO", Valor = "Medio",   Ganados = 50, Perdidos = 50 },  // 50 %
                new EntTasaExito { Dimension = "SEGMENTO", Valor = "Bajo",    Ganados = 49, Perdidos = 51 },  // 49 %
                new EntTasaExito { Dimension = "SEGMENTO", Valor = "Chico",   Ganados = 9,  Perdidos = 0  },  // < 10 cierres
                new EntTasaExito { Dimension = "TIPO",     Valor = "PRIVADO", Ganados = 62, Perdidos = 38 },
                new EntTasaExito { Dimension = "TIPO",     Valor = "PUBLICO", Ganados = 55, Perdidos = 45 }
            };
        }

        [TestMethod]
        public void Encaje_Segmento()
        {
            Assert.AreEqual(5 + 1, NegCalificacionForeCast.PuntosEncaje("Alto",  "PUBLICO", 0, Tasas()));
            Assert.AreEqual(3 + 1, NegCalificacionForeCast.PuntosEncaje("Medio", "PUBLICO", 0, Tasas()));
            Assert.AreEqual(1 + 1, NegCalificacionForeCast.PuntosEncaje("Bajo",  "PUBLICO", 0, Tasas()));
            Assert.AreEqual(3 + 1, NegCalificacionForeCast.PuntosEncaje("Chico", "PUBLICO", 0, Tasas()));
            Assert.AreEqual(3 + 1, NegCalificacionForeCast.PuntosEncaje("Desconocido", "PUBLICO", 0, Tasas()));
        }

        [TestMethod]
        public void Encaje_TipoConMejorTasaSumaDos()
        {
            Assert.AreEqual(1 + 2, NegCalificacionForeCast.PuntosEncaje("Bajo", "PRIVADO", 0, Tasas()));
        }

        [TestMethod]
        public void Encaje_VentasPreviasYTope()
        {
            Assert.AreEqual(1 + 1 + 2, NegCalificacionForeCast.PuntosEncaje("Bajo", "PUBLICO", 1, Tasas()));
            Assert.AreEqual(1 + 1 + 3, NegCalificacionForeCast.PuntosEncaje("Bajo", "PUBLICO", 3, Tasas()));
            Assert.AreEqual(10,        NegCalificacionForeCast.PuntosEncaje("Alto", "PRIVADO", 9, Tasas()));  // 5+2+3
        }

        [TestMethod]
        public void Encaje_NoDependeDeEspaciosNiMayusculas()
        {
            Assert.AreEqual(5 + 2, NegCalificacionForeCast.PuntosEncaje("  alto ", " privado", 0, Tasas()));
        }

        [TestMethod]
        public void Encaje_SinTasas_NoFalla()
        {
            Assert.AreEqual(3 + 1, NegCalificacionForeCast.PuntosEncaje("Alto", "PRIVADO", 0, null));
            Assert.AreEqual(3 + 1, NegCalificacionForeCast.PuntosEncaje(null, null, 0, new List<EntTasaExito>()));
        }

        /* ---------------- Clasificacion, ponderado, etapa ---------------- */

        [TestMethod]
        public void Clasificar_Limites()
        {
            Assert.AreEqual("Fría",     NegCalificacionForeCast.Clasificar(49));
            Assert.AreEqual("Tibia",    NegCalificacionForeCast.Clasificar(50));
            Assert.AreEqual("Tibia",    NegCalificacionForeCast.Clasificar(74));
            Assert.AreEqual("Caliente", NegCalificacionForeCast.Clasificar(75));
        }

        [TestMethod]
        public void Ponderado()
        {
            Assert.AreEqual(40000m, NegCalificacionForeCast.MontoPonderado(100000m, 40m));
            Assert.AreEqual(0m,     NegCalificacionForeCast.MontoPonderado(100000m, null));
            Assert.AreEqual(0m,     NegCalificacionForeCast.MontoPonderado(100000m, 0m));
            Assert.AreEqual(0m,     NegCalificacionForeCast.MontoPonderado(null, 40m));
            Assert.AreEqual(333.33m, NegCalificacionForeCast.MontoPonderado(3333.33m, 10m));
        }

        [TestMethod]
        public void EsAvanzada()
        {
            Assert.IsTrue(NegCalificacionForeCast.EsAvanzada("UPSIDE", 0m));
            Assert.IsTrue(NegCalificacionForeCast.EsAvanzada("FORECAST", null));
            Assert.IsTrue(NegCalificacionForeCast.EsAvanzada("LEAD", 40m));
            Assert.IsFalse(NegCalificacionForeCast.EsAvanzada("PIPELINE", 20m));
            Assert.IsFalse(NegCalificacionForeCast.EsAvanzada(null, null));
        }

        /* ---------------- Evaluar ---------------- */

        /// <summary>
        /// Una oportunidad "sana": calificada hoy con todo en maximo, cierre en 30
        /// dias, modificada hoy, con contacto y monto. No dispara ninguna alerta.
        /// </summary>
        private static EntCalificacionForeCast Sana()
        {
            return new EntCalificacionForeCast
            {
                IdForeCast = 1,
                IdCliente = 1,
                Cliente = "Cliente A",
                Prioridad = "PIPELINE",
                SegmentodeMercado = "Alto",
                TipoEmpresa = "PRIVADO",
                PVPEstimado = 100000m,
                UtilidadBrutaEstimada = 20000m,
                IdNegocio = 5,
                Porcentaje = 40m,
                FechaCierre = Hoy.AddDays(30),
                FechaModificacion = Hoy,
                Contacto = "Contacto A",
                ContactosCuenta = 2,
                FechasCierreDistintas = 1,
                GanadosCliente = 3,
                Calificacion = new EntCalificacionRespuesta
                {
                    IdForeCast = 1, Presupuesto = 20, Autoridad = 20, Necesidad = 20,
                    DecisorCargo = "Decisor, Gerente", PropuestaEnviada = true,
                    AccionComprometida = "Reunion de cierre", FechaAccion = Hoy.AddDays(3),
                    Usuario = "u1", FechaRegistro = Hoy
                }
            };
        }

        private static EntCalificacionForeCast Evaluada(EntCalificacionForeCast o)
        {
            NegCalificacionForeCast.Evaluar(o, Tasas(), Hoy);
            return o;
        }

        [TestMethod]
        public void Evaluar_Sana_PuntajeCompletoYSinAlertas()
        {
            EntCalificacionForeCast o = Evaluada(Sana());

            Assert.AreEqual(100, o.Puntaje);           // 20+20+20+15+15+10
            Assert.AreEqual("Caliente", o.Estado);
            Assert.IsFalse(o.SinCalificar);
            Assert.AreEqual(40000m, o.MontoPonderado);
            Assert.AreEqual(0, o.Alertas.Count);
            Assert.AreEqual(0, o.Faltantes.Count);
            Assert.AreEqual("Avanzar a la siguiente etapa", o.AccionSugerida);
        }

        [TestMethod]
        public void Evaluar_FechasEnTextoInvariante()
        {
            EntCalificacionForeCast o = Evaluada(Sana());
            Assert.AreEqual("28/10/2026", o.FechaCierreTexto);
            Assert.AreEqual("28/09/2026", o.UltimaActividadTexto);
            Assert.AreEqual("28/09/2026", o.FechaCalificacionTexto);
            Assert.AreEqual("01/10/2026", o.FechaAccionTexto);
        }

        [TestMethod]
        public void Evaluar_SinCalificar_CeroEnLosTresYMarcada()
        {
            EntCalificacionForeCast o = Sana();
            o.Calificacion = null;
            Evaluada(o);

            Assert.IsTrue(o.SinCalificar);
            Assert.AreEqual(0, o.Presupuesto + o.Autoridad + o.Necesidad);
            Assert.AreEqual(40, o.Puntaje);            // 15+15+10
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(o, NegCalificacionForeCast.AlertaCalificacionPendiente));
            CollectionAssert.Contains(o.Faltantes, "Calificación");
            CollectionAssert.Contains(o.Faltantes, "Decisor con cargo");
            Assert.AreEqual("Calificar la oportunidad", o.AccionSugerida);
        }

        [TestMethod]
        public void Alerta1_SinActividad()
        {
            EntCalificacionForeCast o = Sana();
            o.FechaModificacion = Hoy.AddDays(-31);
            o.Calificacion.FechaRegistro = Hoy.AddDays(-31);
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(Evaluada(o), NegCalificacionForeCast.AlertaSinActividad));

            EntCalificacionForeCast p = Sana();
            p.FechaModificacion = Hoy.AddDays(-30);
            p.Calificacion.FechaRegistro = Hoy.AddDays(-30);
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaSinActividad));
        }

        [TestMethod]
        public void Alerta2_CierreVencido()
        {
            EntCalificacionForeCast o = Sana();
            o.FechaCierre = Hoy.AddDays(-1);
            Evaluada(o);
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(o, NegCalificacionForeCast.AlertaCierreVencido));
            Assert.AreEqual("Actualizar la fecha de cierre con el cliente", o.AccionSugerida);

            EntCalificacionForeCast p = Sana();
            p.FechaCierre = Hoy;
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaCierreVencido));
        }

        [TestMethod]
        public void Alerta3_MontoSinDefinirEnAvanzada()
        {
            EntCalificacionForeCast o = Sana();
            o.PVPEstimado = 0m;
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(Evaluada(o), NegCalificacionForeCast.AlertaMontoSinDefinir));

            EntCalificacionForeCast p = Sana();
            p.PVPEstimado = null; p.Porcentaje = 20m;   // no avanzada
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaMontoSinDefinir));
        }

        [TestMethod]
        public void Alerta4_AvanzadaSinPropuesta()
        {
            EntCalificacionForeCast o = Sana();
            o.Calificacion.PropuestaEnviada = false;
            Evaluada(o);
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(o, NegCalificacionForeCast.AlertaSinPropuesta));
            Assert.AreEqual("Enviar la propuesta", o.AccionSugerida);

            EntCalificacionForeCast p = Sana();
            p.Calificacion.PropuestaEnviada = false; p.Porcentaje = 20m;  // no avanzada
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaSinPropuesta));
        }

        [TestMethod]
        public void Alerta5_SinDecisor()
        {
            EntCalificacionForeCast o = Sana();
            o.ContactosCuenta = 1; o.Calificacion.Autoridad = 12;
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(Evaluada(o), NegCalificacionForeCast.AlertaSinDecisor));

            EntCalificacionForeCast p = Sana();
            p.ContactosCuenta = 1;   // Autoridad 20: hay decisor
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaSinDecisor));
        }

        [TestMethod]
        public void Alerta6_CierreCambiado()
        {
            EntCalificacionForeCast o = Sana();
            o.FechasCierreDistintas = 3;
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(Evaluada(o), NegCalificacionForeCast.AlertaCierreCambiado));

            EntCalificacionForeCast p = Sana();
            p.FechasCierreDistintas = 2;
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaCierreCambiado));
        }

        [TestMethod]
        public void Alerta7_CalificacionDesactualizada()
        {
            EntCalificacionForeCast o = Sana();
            o.Calificacion.FechaRegistro = Hoy.AddDays(-31);
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(Evaluada(o), NegCalificacionForeCast.AlertaCalificacionPendiente));

            EntCalificacionForeCast p = Sana();
            p.Calificacion.FechaRegistro = Hoy.AddDays(-30);
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(Evaluada(p), NegCalificacionForeCast.AlertaCalificacionPendiente));
        }

        [TestMethod]
        public void Alerta8_Inconsistente_TienePrioridadEnLaAccion()
        {
            EntCalificacionForeCast o = Sana();
            o.IdNegocio = NegCalificacionForeCast.IdNegocioPerdida;
            o.FechaCierre = Hoy.AddDays(-5);          // tambien vencida
            Evaluada(o);
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(o, NegCalificacionForeCast.AlertaInconsistente));
            Assert.AreEqual("Cerrar o corregir la etapa: el código indica perdida o descartada", o.AccionSugerida);
        }

        [TestMethod]
        public void Accion_CriterioMasDebil_EmpateGanaPresupuesto()
        {
            EntCalificacionForeCast o = Sana();
            o.Calificacion.Presupuesto = 12; o.Calificacion.Autoridad = 12; o.Calificacion.Necesidad = 14;
            o.ContactosCuenta = 3;
            Assert.AreEqual("Confirmar el presupuesto con el cliente", Evaluada(o).AccionSugerida);

            EntCalificacionForeCast p = Sana();
            p.Calificacion.Autoridad = 5; p.ContactosCuenta = 3;
            Assert.AreEqual("Identificar y reunirse con el decisor", Evaluada(p).AccionSugerida);

            EntCalificacionForeCast q = Sana();
            q.Calificacion.Necesidad = 6;
            Assert.AreEqual("Validar la necesidad y la fecha del requerimiento", Evaluada(q).AccionSugerida);
        }

        [TestMethod]
        public void Accion_SinActividad_CuandoLoDemasEstaBien()
        {
            EntCalificacionForeCast p = Sana();
            p.FechaModificacion = Hoy.AddDays(-40);
            p.Calificacion.FechaRegistro = Hoy.AddDays(-31);
            Evaluada(p);
            // Tiene las alertas 1 y 7. La 7 no esta en la precedencia de la accion
            // y los tres criterios estan en 20: gana "sin actividad".
            Assert.IsTrue(NegCalificacionForeCast.TieneAlerta(p, NegCalificacionForeCast.AlertaSinActividad));
            Assert.AreEqual("Retomar contacto con el cliente", p.AccionSugerida);
        }

        [TestMethod]
        public void UltimaActividad_LaCalificacionCuentaComoActividad()
        {
            EntCalificacionForeCast o = Sana();
            o.FechaModificacion = Hoy.AddDays(-40);
            o.Calificacion.FechaRegistro = Hoy.AddDays(-29);
            Evaluada(o);
            Assert.IsFalse(NegCalificacionForeCast.TieneAlerta(o, NegCalificacionForeCast.AlertaSinActividad));
            Assert.AreEqual(10, o.Actividad);
        }

        [TestMethod]
        public void Faltantes_SinContactoSinFechaSinUtilidad_NoFalla()
        {
            EntCalificacionForeCast o = Sana();
            o.Contacto = " "; o.LiderProyectoCliente = null;
            o.FechaCierre = null; o.UtilidadBrutaEstimada = null;
            o.PVPEstimado = null; o.Porcentaje = null;
            Evaluada(o);

            CollectionAssert.Contains(o.Faltantes, "Contacto");
            CollectionAssert.Contains(o.Faltantes, "Fecha de cierre");
            CollectionAssert.Contains(o.Faltantes, "Utilidad bruta estimada");
            Assert.AreEqual(0, o.Tiempo);
            Assert.AreEqual(0m, o.MontoPonderado);
            Assert.AreEqual("", o.FechaCierreTexto);
        }

        /* ---------------- Armar ---------------- */

        private static EntDatosCalificacion Datos()
        {
            EntCalificacionForeCast caliente = Sana();                 // 100, pond 40.000

            EntCalificacionForeCast tibiaGrande = Sana();
            tibiaGrande.IdForeCast = 2; tibiaGrande.PVPEstimado = 500000m;          // pond 200.000
            tibiaGrande.Calificacion.Presupuesto = 5; tibiaGrande.Calificacion.Autoridad = 5;
            tibiaGrande.Calificacion.Necesidad = 6;                                   // 16+15+15+10 = 56
            tibiaGrande.Sucursal = "SUR"; tibiaGrande.GerentedeCuenta = "G2";

            EntCalificacionForeCast fria = Sana();
            fria.IdForeCast = 3; fria.Calificacion = null;
            fria.FechaModificacion = Hoy.AddDays(-90);                                // alerta 1
            fria.FechaCierre = Hoy.AddDays(-10);                                      // alerta 2

            EntCalificacionForeCast friaSinRiesgo = Sana();
            friaSinRiesgo.IdForeCast = 4; friaSinRiesgo.Calificacion = null;          // 40, sin alertas 1/2/8

            caliente.Sucursal = fria.Sucursal = friaSinRiesgo.Sucursal = "NORTE";
            caliente.GerentedeCuenta = fria.GerentedeCuenta = friaSinRiesgo.GerentedeCuenta = "G1";

            return new EntDatosCalificacion
            {
                Oportunidades = new List<EntCalificacionForeCast> { fria, friaSinRiesgo, tibiaGrande, caliente },
                Tasas = Tasas()
            };
        }

        [TestMethod]
        public void Armar_OrdenaIndicadoresTopYRiesgo()
        {
            EntPantallaCalificacion p = NegCalificacionForeCast.Armar(Datos(), new EntFiltroCalificacion(), Hoy);

            CollectionAssert.AreEqual(new long[] { 1, 2, 4, 3 }, IdsDe(p));   // por puntaje desc
            Assert.AreEqual(4, p.Total);
            Assert.AreEqual(1, p.Calientes);
            Assert.AreEqual(1, p.Tibias);
            Assert.AreEqual(2, p.Frias);
            Assert.AreEqual(2, p.SinCalificar);
            Assert.AreEqual(50.0m, p.PorcentajeSinCalificar);
            Assert.AreEqual(800000m, p.MontoTotal);
            CollectionAssert.AreEqual(new long[] { 2, 1 }, p.Top5);            // por ponderado desc
            CollectionAssert.AreEqual(new long[] { 3 }, p.EnRiesgo);
            CollectionAssert.AreEqual(new[] { "G1", "G2" }, p.Gerentes);
        }

        [TestMethod]
        public void Armar_Filtros_NoCambianLosCombos()
        {
            EntPantallaCalificacion p = NegCalificacionForeCast.Armar(
                Datos(), new EntFiltroCalificacion { Sucursal = " sur " }, Hoy);
            CollectionAssert.AreEqual(new long[] { 2 }, IdsDe(p));
            CollectionAssert.AreEqual(new[] { "NORTE", "SUR" }, p.Sucursales);

            EntPantallaCalificacion q = NegCalificacionForeCast.Armar(
                Datos(), new EntFiltroCalificacion { Estado = "Sin calificar" }, Hoy);
            CollectionAssert.AreEqual(new long[] { 4, 3 }, IdsDe(q));

            EntPantallaCalificacion r = NegCalificacionForeCast.Armar(
                Datos(), new EntFiltroCalificacion { Estado = "Fría" }, Hoy);
            Assert.AreEqual(2, r.Total);
        }

        [TestMethod]
        public void Armar_Vacio_NoDivideEntreCero()
        {
            EntPantallaCalificacion p = NegCalificacionForeCast.Armar(new EntDatosCalificacion(), null, Hoy);
            Assert.AreEqual(0, p.Total);
            Assert.AreEqual(0m, p.PorcentajeSinCalificar);
        }

        [TestMethod]
        public void Json_ConPuntoDecimalEnCulturaEsES()
        {
            EntPantallaCalificacion p = new EntPantallaCalificacion { MontoPonderado = 1234.5m, PorcentajeSinCalificar = 33.3m };
            string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(p);
            StringAssert.Contains(json, "\"MontoPonderado\":1234.5");
            StringAssert.Contains(json, "\"PorcentajeSinCalificar\":33.3");
            Assert.IsFalse(json.Contains("1234,5"));
        }

        private static long[] IdsDe(EntPantallaCalificacion p)
        {
            List<long> ids = new List<long>();
            foreach (EntCalificacionForeCast o in p.Oportunidades) { ids.Add(o.IdForeCast); }
            return ids.ToArray();
        }

        /* ---------------- Validacion ---------------- */

        private static EntCalificacionRespuesta Respuesta()
        {
            return new EntCalificacionRespuesta { IdForeCast = 7, Presupuesto = 12, Autoridad = 5, Necesidad = 14 };
        }

        [TestMethod]
        public void ValidarRespuesta_AceptaSoloLosPuntosDelModal()
        {
            Assert.AreEqual("", NegCalificacionForeCast.ValidarRespuesta(Respuesta()));

            EntCalificacionRespuesta a = Respuesta(); a.Presupuesto = 7;
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(a));
            EntCalificacionRespuesta b = Respuesta(); b.Necesidad = 12;   // 12 es de Presupuesto, no de Necesidad
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(b));
            EntCalificacionRespuesta c = Respuesta(); c.IdForeCast = 0;
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(c));
            EntCalificacionRespuesta d = Respuesta(); d.DecisorCargo = new string('x', 151);
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(d));
            EntCalificacionRespuesta e = Respuesta(); e.AccionComprometida = new string('x', 301);
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(e));
            Assert.AreNotEqual("", NegCalificacionForeCast.ValidarRespuesta(null));
        }

        [TestMethod]
        public void ValidarGuardado_RechazaUnaOportunidadQueElUsuarioNoVe()
        {
            Assert.AreEqual("", NegCalificacionForeCast.ValidarGuardado(Respuesta(), new long[] { 5, 7 }));
            Assert.AreEqual("No tiene acceso a esa oportunidad.",
                NegCalificacionForeCast.ValidarGuardado(Respuesta(), new long[] { 5, 6 }));
        }
    }
}
