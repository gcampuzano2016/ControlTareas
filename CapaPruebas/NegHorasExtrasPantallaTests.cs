using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CapaPruebas
{
    /// <summary>
    /// Pruebas de la orquestacion que NO necesitan base de datos. Lo que habla
    /// con el DAO no se prueba aqui; lo que decide numeros, si.
    /// </summary>
    [TestClass]
    public class NegHorasExtrasPantallaTests
    {
        private static EntHeParametros Parametros()
        {
            EntHeParametros p = new EntHeParametros();
            p.DiasMes = 30;
            p.HorasMesJornadaCompleta = 240;
            p.Factor50 = 1.5m;
            p.Factor100 = 2m;
            p.DecimalesMonto = 2;
            return p;
        }

        [TestMethod]
        public void UltimoDiaDelMes_Septiembre2026_Da30()
        {
            Assert.AreEqual(new DateTime(2026, 9, 30), NegHorasExtrasPantalla.UltimoDiaDelMes(2026, 9));
        }

        [TestMethod]
        public void UltimoDiaDelMes_FebreroBisiesto_Da29()
        {
            Assert.AreEqual(new DateTime(2028, 2, 29), NegHorasExtrasPantalla.UltimoDiaDelMes(2028, 2));
        }

        /// <summary>
        /// El corte tiene que ser el ULTIMO dia del mes y no el primero. Con el
        /// primero, un ajuste que entra en vigencia el dia 1 del periodo se
        /// tomaria, pero uno que entra el dia 15 quedaria fuera y la persona
        /// cobraria el mes entero al sueldo viejo.
        /// </summary>
        [TestMethod]
        public void AplicarCalculo_UsaElSalarioVigenteAlCorte()
        {
            List<EntHeSalario> historial = new List<EntHeSalario>
            {
                new EntHeSalario { Monto = 1200m, FechaVigenciaDesde = new DateTime(2026, 1, 1), Origen = "Rol" },
                new EntHeSalario { Monto = 1500m, FechaVigenciaDesde = new DateTime(2026, 9, 15), Origen = "Ajuste" }
            };

            decimal alCorte = NegHorasExtras.SalarioVigente(historial, NegHorasExtrasPantalla.UltimoDiaDelMes(2026, 9));

            Assert.AreEqual(1500m, alCorte);
        }

        [TestMethod]
        public void AplicarCalculo_JornadaCompleta_LlenaLaFila()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.AplicaHESnapshot = true;
            f.Horas50 = 10m;
            f.Horas100 = 0m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 1200m, Parametros());

            Assert.AreEqual(1200m, f.SalarioBaseSnapshot);
            Assert.AreEqual(240, f.Divisor);
            Assert.AreEqual(5m, f.ValorHoraOrdinaria);
            Assert.AreEqual(7.5m, f.ValorHora50);
            Assert.AreEqual(75.00m, f.Total50);
            Assert.AreEqual(75.00m, f.TotalHE);
            Assert.AreEqual(10m, f.TotalHoras);
            Assert.IsFalse(f.TieneAdvertencia);
        }

        /// <summary>
        /// El divisor manual manda sobre la jornada. Es la unica columna del
        /// maestro que cambia el valor de la hora, asi que si se perdiera por
        /// el camino la persona cobraria distinto sin que nada avise.
        /// </summary>
        [TestMethod]
        public void AplicarCalculo_ConDivisorManual_MandaSobreLaJornada()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.DivisorManual = 120;
            f.AplicaHESnapshot = true;
            f.Horas50 = 10m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 1200m, Parametros());

            Assert.AreEqual(120, f.Divisor);
            Assert.AreEqual(10m, f.ValorHoraOrdinaria);
            Assert.AreEqual(150.00m, f.Total50);
        }

        [TestMethod]
        public void AplicarCalculo_SinSalario_MarcaAdvertenciaYNoPaga()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.AplicaHESnapshot = true;
            f.Horas50 = 10m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 0m, Parametros());

            Assert.IsTrue(f.TieneAdvertencia);
            Assert.AreEqual(0m, f.TotalHE);
            Assert.AreEqual(10m, f.TotalHoras, "las horas cargadas son validas: lo que falta es el sueldo");
        }

        [TestMethod]
        public void AplicarCalculo_NoAplicaHE_ConHoras_PagaCero()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.AplicaHESnapshot = false;
            f.Horas50 = 10m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 1200m, Parametros());

            Assert.AreEqual(0m, f.TotalHE);
        }

        /// <summary>
        /// Esta prueba NO distingue "sumar los redondeados" de "redondear la
        /// suma": Total50 = 0.13m ya llega a dos decimales, y sumar tres veces
        /// 0.13 da 0.39 con cualquiera de las dos formas. Esa distincion no se
        /// puede dar por esta via porque Total50 siempre llega redondeado
        /// desde Calcular; un fixture que la forzara estaria probando un caso
        /// imposible.
        ///
        /// Lo que si atrapa: que el total en dinero salga del campo correcto.
        /// Horas50 vale 1m y Total50 vale 0.13m -a proposito distintos-, asi
        /// que si TotalPago50 se armara sumando Horas50 en vez de Total50 (o
        /// viceversa con TotalHoras50) el resultado seria 3 en vez de 0.39, o
        /// 0.39 en vez de 3, y la prueba lo marcaria.
        /// </summary>
        [TestMethod]
        public void SumarTotales_SumaLosTotalesYaRedondeados()
        {
            EntHePantalla p = new EntHePantalla();
            for (int i = 0; i < 3; i++)
            {
                EntHeFila f = new EntHeFila();
                f.Horas50 = 1m;
                f.Total50 = 0.13m;
                f.TotalHE = 0.13m;
                f.TotalHoras = 1m;
                p.Filas.Add(f);
            }

            NegHorasExtrasPantalla.SumarTotales(p);

            Assert.AreEqual(0.39m, p.TotalPago50);
            Assert.AreEqual(0.39m, p.TotalPagar);
            Assert.AreEqual(3m, p.TotalHoras50);
            Assert.AreEqual(3m, p.TotalHoras);
        }

        [TestMethod]
        public void SumarTotales_SinFilas_DaCeroYNoLanza()
        {
            EntHePantalla p = new EntHePantalla();

            NegHorasExtrasPantalla.SumarTotales(p);

            Assert.AreEqual(0m, p.TotalPagar);
        }

        /// <summary>
        /// Tope de horas por celda del lado del servidor. El documento
        /// funcional lo declara como validacion de cliente, pero el cliente no
        /// es de fiar: un dedazo de 10000 horas tiene que rechazarse aqui, no
        /// pagarse.
        /// </summary>
        [TestMethod]
        public void ExcedeTopePorCelda_Con201_DaTrue()
        {
            Assert.IsTrue(NegHorasExtrasPantalla.ExcedeTopePorCelda(201m));
        }

        [TestMethod]
        public void ExcedeTopePorCelda_Con200_DaFalse()
        {
            Assert.IsFalse(NegHorasExtrasPantalla.ExcedeTopePorCelda(200m));
        }

        [TestMethod]
        public void ExcedeTopePorCelda_ConDiezMil_DaTrue()
        {
            Assert.IsTrue(NegHorasExtrasPantalla.ExcedeTopePorCelda(10000m));
        }
    }
}
