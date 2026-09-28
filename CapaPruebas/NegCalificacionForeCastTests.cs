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
    }
}
