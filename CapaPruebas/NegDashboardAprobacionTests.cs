using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace CapaPruebas
{
    /// <summary>
    /// Lo unico del dashboard que se puede probar sin base: las tres
    /// conversiones que ocurren entre el dato agregado y lo que se dibuja.
    /// </summary>
    [TestClass]
    public class NegDashboardAprobacionTests
    {
        private CultureInfo _culturaOriginal;

        /// <summary>
        /// Cultura hostil a proposito: en es-ES el separador decimal es la coma.
        /// Si alguien construye el numero con ToString() sin InvariantCulture,
        /// al JSON le llega "7,5" y Chart.js lo lee como texto, no como numero:
        /// la barra sale en cero sin un solo error.
        /// </summary>
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

        /* ------------------------------------------- horas decimales ------- */

        [TestMethod]
        public void HorasDecimales_DeUnaHoraExacta_DaUno()
        {
            Assert.AreEqual(1.0m, NegDashboardAprobacion.HorasDecimales(60));
        }

        [TestMethod]
        public void HorasDecimales_DeMediaHora_DaMedio()
        {
            Assert.AreEqual(0.5m, NegDashboardAprobacion.HorasDecimales(30));
        }

        [TestMethod]
        public void HorasDecimales_RedondeaAUnDecimal()
        {
            // 100 minutos son 1,666... horas
            Assert.AreEqual(1.7m, NegDashboardAprobacion.HorasDecimales(100));
        }

        [TestMethod]
        public void HorasDecimales_DeCero_DaCero()
        {
            Assert.AreEqual(0m, NegDashboardAprobacion.HorasDecimales(0));
        }

        /// <summary>
        /// Minutos negativos no deberian existir, pero si la base devolviera uno
        /// -una fila con Det_Tiempo raro que pase el filtro- la grafica tiene que
        /// mostrar cero y no una barra hacia abajo que nadie sabe leer.
        /// </summary>
        [TestMethod]
        public void HorasDecimales_DeNegativo_DaCero()
        {
            Assert.AreEqual(0m, NegDashboardAprobacion.HorasDecimales(-30));
        }

        /* ----------------------------------------------- top con otras ----- */

        private static List<EntDashboardEmpresa> Empresas(params int[] minutos)
        {
            var lista = new List<EntDashboardEmpresa>();
            for (int i = 0; i < minutos.Length; i++)
            {
                lista.Add(new EntDashboardEmpresa
                {
                    Empresa = "Empresa " + (i + 1),
                    Minutos = minutos[i]
                });
            }
            return lista;
        }

        [TestMethod]
        public void TopConOtras_ConMenosQueElTope_DevuelveTodoSinAgregarOtras()
        {
            var r = NegDashboardAprobacion.TopConOtras(Empresas(50, 40, 30), 10);

            Assert.AreEqual(3, r.Count);
            Assert.IsFalse(r.Exists(e => e.Empresa == "Otras"));
        }

        [TestMethod]
        public void TopConOtras_ConExactamenteElTope_NoAgregaOtras()
        {
            var r = NegDashboardAprobacion.TopConOtras(Empresas(10, 9, 8), 3);

            Assert.AreEqual(3, r.Count);
            Assert.IsFalse(r.Exists(e => e.Empresa == "Otras"));
        }

        [TestMethod]
        public void TopConOtras_ConMasQueElTope_AgrupaElRestoEnOtras()
        {
            var r = NegDashboardAprobacion.TopConOtras(Empresas(10, 9, 8, 5, 3), 3);

            Assert.AreEqual(4, r.Count);
            Assert.AreEqual("Otras", r[3].Empresa);
            Assert.AreEqual(8, r[3].Minutos);   // 5 + 3
        }

        /// <summary>
        /// El orden manda: si la lista llega desordenada, el top tiene que ser el
        /// de MAS minutos, no los primeros que vinieron.
        /// </summary>
        [TestMethod]
        public void TopConOtras_OrdenaPorMinutosAntesDeCortar()
        {
            var lista = new List<EntDashboardEmpresa>
            {
                new EntDashboardEmpresa { Empresa = "Chica", Minutos = 5 },
                new EntDashboardEmpresa { Empresa = "Grande", Minutos = 100 },
                new EntDashboardEmpresa { Empresa = "Media", Minutos = 50 }
            };

            var r = NegDashboardAprobacion.TopConOtras(lista, 2);

            Assert.AreEqual("Grande", r[0].Empresa);
            Assert.AreEqual("Media", r[1].Empresa);
            Assert.AreEqual("Otras", r[2].Empresa);
            Assert.AreEqual(5, r[2].Minutos);
        }

        [TestMethod]
        public void TopConOtras_ConListaVacia_DevuelveVacia()
        {
            var r = NegDashboardAprobacion.TopConOtras(new List<EntDashboardEmpresa>(), 10);

            Assert.AreEqual(0, r.Count);
        }

        [TestMethod]
        public void TopConOtras_ConNulo_DevuelveVacia()
        {
            var r = NegDashboardAprobacion.TopConOtras(null, 10);

            Assert.AreEqual(0, r.Count);
        }

        /// <summary>
        /// Con tope cero o negativo no se puede armar un top: se devuelve todo
        /// sin agrupar, que es menos sorprendente que devolver una sola porcion
        /// llamada "Otras" con el total.
        /// </summary>
        [TestMethod]
        public void TopConOtras_ConTopeCero_DevuelveLaListaSinAgrupar()
        {
            var r = NegDashboardAprobacion.TopConOtras(Empresas(10, 9), 0);

            Assert.AreEqual(2, r.Count);
            Assert.IsFalse(r.Exists(e => e.Empresa == "Otras"));
        }

        /* ------------------------------------------------ texto demora ----- */

        [TestMethod]
        public void TextoDemora_DeCero_DiceHoy()
        {
            Assert.AreEqual("hoy", NegDashboardAprobacion.TextoDemora(0));
        }

        [TestMethod]
        public void TextoDemora_DeUno_NoDicePluralRaro()
        {
            Assert.AreEqual("1 día", NegDashboardAprobacion.TextoDemora(1));
        }

        [TestMethod]
        public void TextoDemora_DeVarios_DicePlural()
        {
            Assert.AreEqual("12 días", NegDashboardAprobacion.TextoDemora(12));
        }

        [TestMethod]
        public void TextoDemora_DeNegativo_DiceHoy()
        {
            /* Una aprobacion con fecha anterior al registro da dias negativos.
               Existe en datos viejos y no es un error que valga la pena gritar:
               se lee como "sin demora". */
            Assert.AreEqual("hoy", NegDashboardAprobacion.TextoDemora(-3));
        }
    }
}
