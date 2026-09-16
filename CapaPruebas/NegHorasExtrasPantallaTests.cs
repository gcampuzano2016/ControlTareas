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

            NegHorasExtrasPantalla.AplicarCalculo(f, 1200m, 0m, Parametros());

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

            NegHorasExtrasPantalla.AplicarCalculo(f, 1200m, 0m, Parametros());

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

            NegHorasExtrasPantalla.AplicarCalculo(f, 0m, 0m, Parametros());

            Assert.IsTrue(f.TieneAdvertencia);
            Assert.AreEqual(0m, f.TotalHE);
            Assert.AreEqual(10m, f.TotalHoras, "las horas cargadas son validas: lo que falta es el sueldo");
        }

        /// <summary>
        /// Si no llega un sueldo vigente pero se pasa uno congelado -el
        /// snapshot de un guardado anterior-, no se pisa con cero. Cubre tanto
        /// a quien ya no esta activo en el maestro como a quien esta activo
        /// pero no tiene ningun sueldo vigente a la fecha de corte: para
        /// AplicarCalculo los dos llegan igual, con salarioDelMaestro en 0.
        /// </summary>
        [TestMethod]
        public void AplicarCalculo_SinSueldoVigente_ConCongeladoDisponible_UsaElCongelado()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.AplicaHESnapshot = true;
            f.Horas50 = 10m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 0m, 1200m, Parametros());

            Assert.AreEqual(1200m, f.SalarioBaseSnapshot);
            Assert.IsFalse(f.TieneAdvertencia);
            Assert.AreEqual(75.00m, f.Total50);
        }

        /// <summary>
        /// Si el maestro SI da un sueldo valido, ese gana aunque haya uno
        /// congelado distinto -un sueldo corregido tiene que aplicarse, no
        /// quedarse pegado al anterior-.
        /// </summary>
        [TestMethod]
        public void AplicarCalculo_ConSueldoDelMaestro_GanaSobreElCongelado()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.AplicaHESnapshot = true;
            f.Horas50 = 10m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 1500m, 1200m, Parametros());

            Assert.AreEqual(1500m, f.SalarioBaseSnapshot);
        }

        /// <summary>
        /// Si ni el maestro ni lo congelado tienen un sueldo, es el caso
        /// legitimo de alguien que nunca tuvo sueldo cargado: sigue la rama
        /// normal, con advertencia y sin pagar.
        /// </summary>
        [TestMethod]
        public void AplicarCalculo_SinMaestroYSinCongelado_EsElCasoLegitimoSinSueldo()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.AplicaHESnapshot = true;
            f.Horas50 = 10m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 0m, 0m, Parametros());

            Assert.AreEqual(0m, f.SalarioBaseSnapshot);
            Assert.IsTrue(f.TieneAdvertencia);
        }

        [TestMethod]
        public void AplicarCalculo_NoAplicaHE_ConHoras_PagaCero()
        {
            EntHeFila f = new EntHeFila();
            f.JornadaHorasDiaSnapshot = 8;
            f.AplicaHESnapshot = false;
            f.Horas50 = 10m;

            NegHorasExtrasPantalla.AplicarCalculo(f, 1200m, 0m, Parametros());

            Assert.AreEqual(0m, f.TotalHE);
        }

        /// <summary>
        /// El sueldo congelado a usar al reabrir un periodo: el de la fila que
        /// ya existia, o cero si es la primera vez que el colaborador aparece.
        /// </summary>
        [TestMethod]
        public void SalarioCongeladoAlReabrir_ConFilaExistente_DevuelveSuSnapshot()
        {
            EntHeFila anterior = new EntHeFila();
            anterior.SalarioBaseSnapshot = 1200m;

            Assert.AreEqual(1200m, NegHorasExtrasPantalla.SalarioCongeladoAlReabrir(anterior));
        }

        [TestMethod]
        public void SalarioCongeladoAlReabrir_SinFilaExistente_DaCero()
        {
            Assert.AreEqual(0m, NegHorasExtrasPantalla.SalarioCongeladoAlReabrir(null));
        }

        /// <summary>
        /// Fija exactamente el defecto de la tercera ronda: reabrir un periodo
        /// no puede borrarle el sueldo a quien ya no tiene uno vigente en el
        /// maestro. Compone las dos piezas puras que AbrirPeriodo usa por fila
        /// -SalarioCongeladoAlReabrir y AplicarCalculo- sin tocar el DAO: una
        /// fila fresca como la que devuelve LeerInsumos (SalarioBaseSnapshot en
        /// 0 por omision) mas la fila que ya existia en el periodo.
        /// </summary>
        [TestMethod]
        public void Reapertura_SinSueldoVigenteEnElMaestro_NoPierdeElSueldoCongelado()
        {
            EntHeFila filaExistente = new EntHeFila();
            filaExistente.SalarioBaseSnapshot = 1200m;

            EntHeFila filaFresca = new EntHeFila();
            filaFresca.JornadaHorasDiaSnapshot = 8;
            filaFresca.AplicaHESnapshot = true;
            filaFresca.Horas50 = 10m;

            decimal salarioCongelado = NegHorasExtrasPantalla.SalarioCongeladoAlReabrir(filaExistente);

            NegHorasExtrasPantalla.AplicarCalculo(filaFresca, 0m, salarioCongelado, Parametros());

            Assert.AreEqual(1200m, filaFresca.SalarioBaseSnapshot);
            Assert.IsFalse(filaFresca.TieneAdvertencia);
            Assert.AreEqual(75.00m, filaFresca.Total50);
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

        /// <summary>Una fila con todos los campos que Sp_RTA_HeGuardarFila persiste.</summary>
        private static EntHeFila FilaCompleta()
        {
            EntHeFila f = new EntHeFila();
            f.CedulaSnapshot = "0102030405";
            f.NombreSnapshot = "Persona de Prueba";
            f.EmpresaSnapshot = "Empresa";
            f.CargoSnapshot = "Cargo";
            f.JornadaHorasDiaSnapshot = 8;
            f.SalarioBaseSnapshot = 1200m;
            f.AplicaHESnapshot = true;
            f.Divisor = 240;
            f.ValorHoraOrdinaria = 5m;
            f.ValorHora50 = 7.5m;
            f.ValorHora100 = 10m;
            f.Horas50 = 10m;
            f.Horas100 = 0m;
            f.Total50 = 75.00m;
            f.Total100 = 0m;
            f.TotalHoras = 10m;
            f.TotalHE = 75.00m;
            f.Observacion = "";
            return f;
        }

        [TestMethod]
        public void FilaSinCambios_ConLosMismosValores_DaTrue()
        {
            Assert.IsTrue(NegHorasExtrasPantalla.FilaSinCambios(FilaCompleta(), FilaCompleta()));
        }

        /// <summary>
        /// 1.50 y 1.5 son el mismo numero. Si la comparacion fuera por
        /// representacion en vez de por valor, esta fila se reescribiria sin
        /// que nada haya cambiado de verdad.
        /// </summary>
        [TestMethod]
        public void FilaSinCambios_ConDecimalesEquivalentesEnDistintaRepresentacion_DaTrue()
        {
            EntHeFila nueva = FilaCompleta();
            nueva.ValorHora50 = 7.50m;

            EntHeFila anterior = FilaCompleta();
            anterior.ValorHora50 = 7.5m;

            Assert.IsTrue(NegHorasExtrasPantalla.FilaSinCambios(nueva, anterior));
        }

        /// <summary>
        /// La base nunca devuelve null para Observacion -Texto() en el DAO
        /// siempre da ""-, pero una fila armada en memoria si podria llegar
        /// sin inicializar. Sin este trato, la primera reapertura reescribiria
        /// las filas igual y el arreglo no serviria de nada.
        /// </summary>
        [TestMethod]
        public void FilaSinCambios_ConObservacionNullYVacia_DaTrue()
        {
            EntHeFila nueva = FilaCompleta();
            nueva.Observacion = "";

            EntHeFila anterior = FilaCompleta();
            anterior.Observacion = null;

            Assert.IsTrue(NegHorasExtrasPantalla.FilaSinCambios(nueva, anterior));
        }

        [TestMethod]
        public void FilaSinCambios_ConUnTotalDistinto_DaFalse()
        {
            EntHeFila nueva = FilaCompleta();
            nueva.Total50 = 80.00m;

            Assert.IsFalse(NegHorasExtrasPantalla.FilaSinCambios(nueva, FilaCompleta()));
        }

        [TestMethod]
        public void FilaSinCambios_ConUnaObservacionDistinta_DaFalse()
        {
            EntHeFila nueva = FilaCompleta();
            nueva.Observacion = "cambio real";

            Assert.IsFalse(NegHorasExtrasPantalla.FilaSinCambios(nueva, FilaCompleta()));
        }

        /// <summary>
        /// Sin fila anterior -la primera vez que el colaborador aparece en el
        /// periodo- siempre se escribe: no hay nada contra que comparar.
        /// </summary>
        [TestMethod]
        public void FilaSinCambios_SinFilaAnterior_DaFalse()
        {
            Assert.IsFalse(NegHorasExtrasPantalla.FilaSinCambios(FilaCompleta(), null));
        }
    }
}
