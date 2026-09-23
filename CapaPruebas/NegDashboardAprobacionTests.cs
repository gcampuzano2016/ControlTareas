using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace CapaPruebas
{
    /// <summary>
    /// Lo unico del dashboard que se puede probar sin base: las conversiones
    /// que ocurren entre el dato agregado y lo que se dibuja.
    ///
    /// Desde la ronda de correcciones estas conversiones son las que de verdad
    /// corren en produccion: NegDashboardAprobacion.Cargar las aplica sobre la
    /// entidad y el navegador ya no convierte nada. Antes habia una segunda
    /// copia en dashboardAprobacion.js, esta suite probaba la de C# y la
    /// pantalla usaba la de JavaScript, que para 75 minutos daba 1,3 donde
    /// esta da 1,2.
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

        /// <summary>
        /// El punto medio: 75 minutos son 1,25 horas exactas. Math.Round de .NET
        /// redondea al par y da 1,2; la copia que vivia en JavaScript redondeaba
        /// medio hacia arriba y daba 1,3. Ahora hay una sola regla, y esta
        /// prueba la fija: si alguien vuelve a escribirla en otro lado, tiene
        /// que dar esto.
        /// </summary>
        [TestMethod]
        public void HorasDecimales_EnElPuntoMedio_RedondeaAlPar()
        {
            Assert.AreEqual(1.2m, NegDashboardAprobacion.HorasDecimales(75));
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

        /// <summary>
        /// El promedio llega con un decimal desde que el procedimiento dejo de
        /// truncar. Se escribe con coma y no con punto: lo lee una persona en
        /// espaniol. Y no lo decide la cultura del hilo: el separador se pone a
        /// mano para que el texto sea el mismo desde la web -donde Web.config
        /// fija es-ES- y desde aca o desde cualquier otro llamador.
        /// </summary>
        [TestMethod]
        public void TextoDemora_ConDecimal_UsaComaYNoPunto()
        {
            Assert.AreEqual("1,9 días", NegDashboardAprobacion.TextoDemora(1.9m));
        }

        /// <summary>
        /// Sin nada pendiente, la tarjeta no puede decir "hoy": el cero viene del
        /// ISNULL del procedimiento, no de que algo espere desde esta manana.
        /// </summary>
        [TestMethod]
        public void TextoMasViejo_SinPendientes_NoDiceHoy()
        {
            Assert.AreEqual("sin pendientes", NegDashboardAprobacion.TextoMasViejo(0m, 0));
        }

        /// <summary>
        /// Con pendientes y cero dias si corresponde "hoy": lo mas viejo sin
        /// aprobar se cargo en el dia.
        /// </summary>
        [TestMethod]
        public void TextoMasViejo_ConPendientesYCeroDias_DiceHoy()
        {
            Assert.AreEqual("hoy", NegDashboardAprobacion.TextoMasViejo(0m, 4));
        }

        [TestMethod]
        public void TextoMasViejo_ConPendientes_DiceLosDias()
        {
            Assert.AreEqual("49 días", NegDashboardAprobacion.TextoMasViejo(49m, 4));
        }

        /// <summary>
        /// 1,04 no es igual a 1, pero "0.#" lo imprime como "1": sin redondear
        /// antes de decidir, el texto sale "1 dias". El procedimiento ya devuelve
        /// el promedio redondeado, asi que hoy no pasa; se prueba porque el
        /// metodo es publico y no puede depender de quien lo llame.
        /// </summary>
        [TestMethod]
        public void TextoDemora_ConDecimalQueRedondeaAUno_DiceSingular()
        {
            Assert.AreEqual("1 día", NegDashboardAprobacion.TextoDemora(1.04m));
        }

        /// <summary>
        /// El espejo del anterior por abajo: 0,04 se imprime "0" y diria
        /// "0 dias", que no es ni correcto ni lo que el resto del tablero usa
        /// para "sin demora".
        /// </summary>
        [TestMethod]
        public void TextoDemora_ConDecimalQueRedondeaACero_DiceHoy()
        {
            Assert.AreEqual("hoy", NegDashboardAprobacion.TextoDemora(0.04m));
        }

        [TestMethod]
        public void TextoDemora_DeUnoConDecimalEnCero_NoDicePluralRaro()
        {
            Assert.AreEqual("1 día", NegDashboardAprobacion.TextoDemora(1.0m));
        }

        /* ---------------------------------------- texto demora promedio ---- */

        /// <summary>
        /// Sin ninguna aprobada con fecha no hay promedio. El procedimiento
        /// devuelve cero y cero se escribe "hoy": la tarjeta mostraria el mejor
        /// resultado posible justo en el peor caso de calidad del dato.
        /// </summary>
        [TestMethod]
        public void TextoDemoraPromedio_SinAprobadasConFecha_NoDiceHoy()
        {
            Assert.AreEqual("sin datos", NegDashboardAprobacion.TextoDemoraPromedio(0m, 0));
        }

        [TestMethod]
        public void TextoDemoraPromedio_ConAprobadasConFecha_DiceElPromedio()
        {
            Assert.AreEqual("2,5 días", NegDashboardAprobacion.TextoDemoraPromedio(2.5m, 40));
        }

        /// <summary>
        /// Con aprobadas con fecha y promedio cero, "hoy" SI es la respuesta
        /// correcta: se aprobo el mismo dia. Lo que distingue un caso del otro
        /// es AprobadasConFecha, no el promedio.
        /// </summary>
        [TestMethod]
        public void TextoDemoraPromedio_ConAprobadasYPromedioCero_DiceHoy()
        {
            Assert.AreEqual("hoy", NegDashboardAprobacion.TextoDemoraPromedio(0m, 12));
        }

        /* ---------------------------------------- solo aprobadas ---- */

        [TestMethod]
        public void SoloAprobadas_PasaLoAprobadoComoMinutos()
        {
            var lista = new List<EntDashboardEmpresa>
            {
                new EntDashboardEmpresa { Empresa = "A", Minutos = 100, MinutosAprobados = 60 }
            };

            var r = NegDashboardAprobacion.SoloAprobadas(lista);

            Assert.AreEqual(1, r.Count);
            Assert.AreEqual("A", r[0].Empresa);
            Assert.AreEqual(60, r[0].Minutos);
        }

        [TestMethod]
        public void SoloAprobadas_NoModificaLaListaOriginal()
        {
            var original = new EntDashboardEmpresa { Empresa = "A", Minutos = 100, MinutosAprobados = 60 };
            var lista = new List<EntDashboardEmpresa> { original };

            NegDashboardAprobacion.SoloAprobadas(lista);

            Assert.AreEqual(100, original.Minutos, "la entidad original viaja al JSON: no se pisa");
        }

        [TestMethod]
        public void SoloAprobadas_ConListaNula_DevuelveVacia()
        {
            var r = NegDashboardAprobacion.SoloAprobadas(null);

            Assert.IsNotNull(r);
            Assert.AreEqual(0, r.Count);
        }

        /* ------------------------------------------------------ pivote ----- */

        private static List<EntDashboardPersonaEmpresa> Cruce(params string[] datos)
        {
            /* Cada terna es persona, empresa, minutos. */
            var lista = new List<EntDashboardPersonaEmpresa>();
            for (int i = 0; i < datos.Length; i += 3)
            {
                lista.Add(new EntDashboardPersonaEmpresa
                {
                    Id_Responsable = datos[i],
                    Nombre = datos[i],
                    Empresa = datos[i + 1],
                    Minutos = int.Parse(datos[i + 2], CultureInfo.InvariantCulture)
                });
            }
            return lista;
        }

        [TestMethod]
        public void Pivote_ColocaCadaValorEnSuCelda()
        {
            var t = NegDashboardAprobacion.Pivote(
                Cruce("ANA", "A", "60", "ANA", "B", "30", "LUIS", "A", "120"), 8);

            Assert.AreEqual(2, t.Columnas.Count);
            Assert.AreEqual("A", t.Columnas[0], "la columna con mas minutos va primero");
            Assert.AreEqual("B", t.Columnas[1]);

            Assert.AreEqual("LUIS", t.Filas[0].Nombre, "las filas van por total descendente");
            Assert.AreEqual(120, t.Filas[0].Minutos[0]);
            Assert.AreEqual(0, t.Filas[0].Minutos[1], "celda sin dato es cero, no un hueco");

            Assert.AreEqual("ANA", t.Filas[1].Nombre);
            Assert.AreEqual(60, t.Filas[1].Minutos[0]);
            Assert.AreEqual(30, t.Filas[1].Minutos[1]);
        }

        [TestMethod]
        public void Pivote_TotalesPorFilaYPorColumnaCuadran()
        {
            var t = NegDashboardAprobacion.Pivote(
                Cruce("ANA", "A", "60", "ANA", "B", "30", "LUIS", "A", "120"), 8);

            Assert.AreEqual(120, t.Filas[0].MinutosTotal);
            Assert.AreEqual(90, t.Filas[1].MinutosTotal);

            Assert.AreEqual(180, t.Totales.Minutos[0], "columna A");
            Assert.AreEqual(30, t.Totales.Minutos[1], "columna B");
            Assert.AreEqual(210, t.Totales.MinutosTotal);
        }

        [TestMethod]
        public void Pivote_ConMasClientesQueColumnas_ElSobranteVaAOtras()
        {
            var t = NegDashboardAprobacion.Pivote(
                Cruce("ANA", "A", "100", "ANA", "B", "50", "ANA", "C", "20", "ANA", "D", "5"), 2);

            Assert.AreEqual(3, t.Columnas.Count);
            Assert.AreEqual("Otras", t.Columnas[2]);
            Assert.AreEqual(25, t.Filas[0].Minutos[2], "20 + 5");
            Assert.AreEqual(175, t.Filas[0].MinutosTotal, "el total general no cambia por recortar");
        }

        [TestMethod]
        public void Pivote_EmpresaVaciaSeAgrupaBajoLaEtiquetaDelSql()
        {
            var t = NegDashboardAprobacion.Pivote(
                Cruce("ANA", "", "40", "ANA", "A", "10"), 8);

            Assert.IsTrue(t.Columnas.Contains("(sin empresa)"));
            Assert.AreEqual(50, t.Filas[0].MinutosTotal, "no se descarta: seguiria sin cuadrar con la tarjeta");
        }

        [TestMethod]
        public void Pivote_ConvierteAHorasDespuesDeSumar()
        {
            /* Tres tramos de 5 minutos son 15 minutos: 0,25 h, que HorasDecimales
               redondea al par y deja en 0,2. Convertir cada tramo antes de sumar daria
               0,1 + 0,1 + 0,1 = 0,3. Por eso la conversion va al final. */
            var t = NegDashboardAprobacion.Pivote(
                Cruce("ANA", "A", "5", "ANA", "A", "5", "ANA", "A", "5"), 8);

            Assert.AreEqual(15, t.Filas[0].Minutos[0]);
            Assert.AreEqual(0.2m, t.Filas[0].Horas[0]);
            Assert.AreEqual(0.2m, t.Filas[0].HorasTotal);
        }

        [TestMethod]
        public void Pivote_ConListaNula_DevuelveTablaVacia()
        {
            var t = NegDashboardAprobacion.Pivote(null, 8);

            Assert.IsNotNull(t);
            Assert.AreEqual(0, t.Columnas.Count);
            Assert.AreEqual(0, t.Filas.Count);
        }
    }
}
