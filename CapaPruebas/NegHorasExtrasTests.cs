using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CapaPruebas
{
    /// <summary>
    /// El contrato del calculo de horas extras.
    ///
    /// Los ocho primeros casos son los criterios de aceptacion del documento
    /// funcional, con cifras verificables contra el Excel que este modulo
    /// reemplaza. Los dos ultimos no estan en ese documento y son los que
    /// fallan en silencio: el momento del redondeo y como se suma el total.
    /// Un error en cualquiera de los diez no lanza excepcion: paga mal.
    /// </summary>
    [TestClass]
    public class NegHorasExtrasTests
    {
        private static EntHeParametros Par()
        {
            return new EntHeParametros
            {
                DiasMes = 30,
                HorasMesJornadaCompleta = 240,
                Factor50 = 1.50m,
                Factor100 = 2.00m,
                TopeDiario50 = 4,
                TopeSemanal50 = 12,
                DecimalesMonto = 2
            };
        }

        private static EntHeInsumo Insumo(decimal salario, int jornada, decimal h50, decimal h100,
                                          bool aplica = true, int? divisorManual = null)
        {
            return new EntHeInsumo
            {
                SalarioBaseVigente = salario,
                JornadaHorasDia = jornada,
                DivisorManual = divisorManual,
                AplicaHE = aplica,
                Horas50 = h50,
                Horas100 = h100
            };
        }

        /* --------------------------------- criterios de aceptacion ------ */

        [TestMethod]
        public void Caso1_Salario1200_Jornada8_Diez_Horas_Al50()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 8, 10m, 0m), Par());

            Assert.AreEqual(240, r.Divisor);
            Assert.AreEqual(5.00m, r.ValorHoraOrdinaria);
            Assert.AreEqual(7.50m, r.ValorHora50);
            Assert.AreEqual(75.00m, r.Total50);
        }

        [TestMethod]
        public void Caso2_Salario1200_Jornada8_Diez_Horas_Al100()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 8, 0m, 10m), Par());

            Assert.AreEqual(10.00m, r.ValorHora100);
            Assert.AreEqual(100.00m, r.Total100);
        }

        [TestMethod]
        public void Caso3_Salario600_Ocho_Al50_Y_Cuatro_Al100()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(600m, 8, 8m, 4m), Par());

            Assert.AreEqual(2.50m, r.ValorHoraOrdinaria);
            Assert.AreEqual(30.00m, r.Total50);
            Assert.AreEqual(20.00m, r.Total100);
            Assert.AreEqual(50.00m, r.TotalHE);
        }

        /// <summary>
        /// Media jornada. Es el caso de las 8 personas que trabajan 4 h/dia, y
        /// el unico donde el divisor no es 240. 329/120 = 2.741666... y el
        /// total sale 24.675, que redondeado a dos decimales alejandose del
        /// cero da 24.68 y no 24.67.
        /// </summary>
        [TestMethod]
        public void Caso4_MediaJornada_Divisor120()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(329m, 4, 6m, 0m), Par());

            Assert.AreEqual(120, r.Divisor);
            Assert.AreEqual(2.741667m, r.ValorHoraOrdinaria);
            Assert.AreEqual(4.112500m, r.ValorHora50);
            Assert.AreEqual(24.68m, r.Total50);
        }

        [TestMethod]
        public void Caso5_SinHoras_TodoEnCero_SinError()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(2500m, 8, 0m, 0m), Par());

            Assert.AreEqual(0.00m, r.Total50);
            Assert.AreEqual(0.00m, r.Total100);
            Assert.AreEqual(0.00m, r.TotalHE);
        }

        /// <summary>
        /// La pantalla no deberia dejar cargar horas a quien no aplica, pero el
        /// calculo no confia en la pantalla: con AplicaHE en false los totales
        /// son cero aunque lleguen horas.
        /// </summary>
        [TestMethod]
        public void Caso6_NoAplica_ConHorasCargadas_PagaCero()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 8, 10m, 0m, aplica: false), Par());

            Assert.AreEqual(0.00m, r.Total50);
            Assert.AreEqual(0.00m, r.Total100);
            Assert.AreEqual(0.00m, r.TotalHE);
        }

        /// <summary>
        /// Equivalente al IFERROR del Excel: en vez de reventar, la hora vale
        /// cero y la fila queda marcada para que alguien la mire.
        /// </summary>
        [TestMethod]
        public void Caso7_SalarioCero_HoraOrdinariaCero()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(0m, 8, 10m, 0m), Par());

            Assert.AreEqual(0m, r.ValorHoraOrdinaria);
            Assert.AreEqual(0.00m, r.TotalHE);
            Assert.IsTrue(r.TieneAdvertencia);
        }

        [TestMethod]
        public void Caso7b_DivisorCero_NoLanzaYMarcaAdvertencia()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 0, 10m, 0m), Par());

            Assert.AreEqual(0m, r.ValorHoraOrdinaria);
            Assert.AreEqual(0.00m, r.TotalHE);
            Assert.IsTrue(r.TieneAdvertencia);
        }

        /// <summary>
        /// El ajuste salarial manda sobre el sueldo del rol. En el Excel esto
        /// era una columna llamada con el nombre de un mes; aqui es un
        /// historial con fecha de vigencia y gana el mas reciente que no sea
        /// posterior al corte.
        /// </summary>
        [TestMethod]
        public void Caso8_ConAjusteSalarial_UsaElAjuste()
        {
            List<EntHeSalario> historial = new List<EntHeSalario>
            {
                new EntHeSalario { Monto = 1200m, FechaVigenciaDesde = new DateTime(2026, 1, 1), Origen = "Rol" },
                new EntHeSalario { Monto = 1500m, FechaVigenciaDesde = new DateTime(2026, 9, 1), Origen = "Ajuste" }
            };

            Assert.AreEqual(1500m, NegHorasExtras.SalarioVigente(historial, new DateTime(2026, 9, 30)));
        }

        [TestMethod]
        public void Caso8b_AjustePosteriorAlCorte_NoSeUsa()
        {
            List<EntHeSalario> historial = new List<EntHeSalario>
            {
                new EntHeSalario { Monto = 1200m, FechaVigenciaDesde = new DateTime(2026, 1, 1), Origen = "Rol" },
                new EntHeSalario { Monto = 1500m, FechaVigenciaDesde = new DateTime(2026, 10, 1), Origen = "Ajuste" }
            };

            Assert.AreEqual(1200m, NegHorasExtras.SalarioVigente(historial, new DateTime(2026, 9, 30)));
        }

        /* ------------------- los dos que el documento funcional no tiene -- */

        /// <summary>
        /// El redondeo va SOLO al final. Si alguien redondea el valor hora
        /// durante la cadena, este caso cambia: 100/240 = 0.416666..., por 1.5
        /// da 0.625 exacto, y por 7 horas da 4.375 -> 4.38. Redondeando el
        /// valor hora a 2 decimales primero (0.42 * 1.5 = 0.63) daria 4.41.
        /// La diferencia es de centavos por fila y de dolares por nomina.
        /// </summary>
        [TestMethod]
        public void Redondeo_SoloAlFinal_NoDuranteLaCadena()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(100m, 8, 7m, 0m), Par());

            Assert.AreEqual(4.38m, r.Total50);
        }

        /// <summary>
        /// El total del periodo es la suma de los totales YA redondeados, no el
        /// redondeo de la suma. Asi el gran total cuadra con lo que se ve en
        /// pantalla sumado a mano. Tres filas de 0.125 redondean a 0.13 cada
        /// una y suman 0.39; sumar primero daria 0.375 -> 0.38.
        /// </summary>
        [TestMethod]
        public void TotalDelPeriodo_EsLaSumaDeLosRedondeados()
        {
            List<decimal> totalesPorFila = new List<decimal>();

            for (int i = 0; i < 3; i++)
            {
                totalesPorFila.Add(NegHorasExtras.Calcular(Insumo(20m, 8, 1m, 0m), Par()).Total50);
            }

            Assert.AreEqual(0.13m, totalesPorFila[0]);
            Assert.AreEqual(0.39m, NegHorasExtras.TotalDelPeriodo(totalesPorFila));
        }
    }
}
