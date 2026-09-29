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
                                          bool aplica = true, int? divisorManual = null,
                                          int? minutos50 = null, int? minutos100 = null)
        {
            return new EntHeInsumo
            {
                SalarioBaseVigente = salario,
                JornadaHorasDia = jornada,
                DivisorManual = divisorManual,
                AplicaHE = aplica,
                Horas50 = h50,
                Horas100 = h100,
                Minutos50 = minutos50,
                Minutos100 = minutos100
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
            Assert.AreEqual(10m, r.TotalHoras);
        }

        [TestMethod]
        public void Caso7b_DivisorCero_NoLanzaYMarcaAdvertencia()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 0, 10m, 0m), Par());

            Assert.AreEqual(0m, r.ValorHoraOrdinaria);
            Assert.AreEqual(0.00m, r.TotalHE);
            Assert.IsTrue(r.TieneAdvertencia);
            Assert.AreEqual(10m, r.TotalHoras);
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

        /// <summary>
        /// A igual fecha de vigencia, el ajuste manda sobre el rol (documento
        /// funcional 2.2). El ajuste es el que sustituye al rol. El criterio es
        /// por origen, no por posicion en la lista, porque RRHH puede mantener
        /// la plantilla en cualquier orden.
        ///
        /// Este caso pone el rol DESPUES del ajuste en la lista: si el codigo
        /// ganador fuera "el ultimo", fallaria. Es decir, la prueba falla con
        /// el codigo viejo y pasa con el nuevo.
        /// </summary>
        [TestMethod]
        public void Caso8c_AjusteYRolMismaFecha_ElAjusteGanaPorOrigen()
        {
            List<EntHeSalario> historial = new List<EntHeSalario>
            {
                new EntHeSalario { Monto = 1500m, FechaVigenciaDesde = new DateTime(2026, 9, 1), Origen = "Ajuste" },
                new EntHeSalario { Monto = 1200m, FechaVigenciaDesde = new DateTime(2026, 9, 1), Origen = "Rol" }
            };

            Assert.AreEqual(1500m, NegHorasExtras.SalarioVigente(historial, new DateTime(2026, 9, 30)));
        }

        /// <summary>
        /// La comparacion de origen es case-insensitive con trim, porque la
        /// plantilla que mantiene RRHH puede tener variaciones de formato.
        /// </summary>
        [TestMethod]
        public void Caso8d_AjusteConMayusculasYEspacios_SeReconoc()
        {
            List<EntHeSalario> historial = new List<EntHeSalario>
            {
                new EntHeSalario { Monto = 1500m, FechaVigenciaDesde = new DateTime(2026, 9, 1), Origen = "  AJUSTE  " },
                new EntHeSalario { Monto = 1200m, FechaVigenciaDesde = new DateTime(2026, 9, 1), Origen = "Rol" }
            };

            Assert.AreEqual(1500m, NegHorasExtras.SalarioVigente(historial, new DateTime(2026, 9, 30)));
        }

        /// <summary>
        /// Caso8c y esta juntas fijan que el ganador es el mismo
        /// independientemente del orden de la lista. En produccion, el orden
        /// no lo decide RRHH sino el DAO de la fase 2 via un ORDER BY en SQL
        /// Server, que a fecha igual es arbitrario (el indice es por IdEmpleado
        /// y FechaVigenciaDesde DESC, no hay tercera columna). Por eso el
        /// desempate no puede depender del orden.
        ///
        /// Este caso pone el rol ANTES del ajuste, al contrario de Caso8c.
        /// Ambas deben devolver el ajuste (1500).
        /// </summary>
        [TestMethod]
        public void Caso8e_AjusteYRolMismaFecha_IndependienteDelOrden()
        {
            List<EntHeSalario> historial = new List<EntHeSalario>
            {
                new EntHeSalario { Monto = 1200m, FechaVigenciaDesde = new DateTime(2026, 9, 1), Origen = "Rol" },
                new EntHeSalario { Monto = 1500m, FechaVigenciaDesde = new DateTime(2026, 9, 1), Origen = "Ajuste" }
            };

            Assert.AreEqual(1500m, NegHorasExtras.SalarioVigente(historial, new DateTime(2026, 9, 30)));
        }

        /// <summary>
        /// Horas negativas son dato malo: bloquean el cierre como salario
        /// cero o divisor cero, no son un credito silencioso en la nomina.
        /// </summary>
        [TestMethod]
        public void Caso9_HorasNegativas50_MarcarAdvertenciaYPagaCero()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 8, -5m, 0m), Par());

            Assert.AreEqual(0.00m, r.Total50);
            Assert.AreEqual(0.00m, r.Total100);
            Assert.AreEqual(0.00m, r.TotalHE);
            Assert.IsTrue(r.TieneAdvertencia);
        }

        [TestMethod]
        public void Caso9b_HorasNegativas100_MarcarAdvertenciaYPagaCero()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 8, 0m, -3m), Par());

            Assert.AreEqual(0.00m, r.Total50);
            Assert.AreEqual(0.00m, r.Total100);
            Assert.AreEqual(0.00m, r.TotalHE);
            Assert.IsTrue(r.TieneAdvertencia);
        }

        /// <summary>
        /// Con horas negativas, el total de horas debe ser cero para que no se
        /// cuele un numero negativo en una columna que alguien suma en pantalla.
        /// La advertencia es la senial; un cero con aviso es mas limpio. (Con
        /// salario cero o divisor cero, las horas se muestran porque son dato
        /// valido: lo que falta es el sueldo de la persona.)
        /// </summary>
        [TestMethod]
        public void Caso9c_HorasNegativas_TotalHorasCero()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(1200m, 8, -5m, 0m), Par());

            Assert.AreEqual(0.00m, r.TotalHoras);
            Assert.IsTrue(r.TieneAdvertencia);
        }

        /// <summary>
        /// El invariante critico: horas negativas siempre resultan en TotalHoras
        /// cero, incluso si hay otros errores (salario cero, divisor cero).
        /// Un numero negativo en una columna que se suma en pantalla es peor
        /// que cualquier otra cosa. Esta prueba cubre la combinacion de dos
        /// condiciones de error, que ninguna prueba anterior activaba.
        /// </summary>
        [TestMethod]
        public void Caso9d_SalarioCeroYHorasNegativas_TotalHorasCero()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(0m, 8, -5m, 0m), Par());

            Assert.AreEqual(0.00m, r.TotalHoras);
            Assert.IsTrue(r.TieneAdvertencia);
        }

        /* ------------------------------- pago por minutos (tarea 3) ------ */

        /// <summary>
        /// Cuando llegan minutos, la cadena de multiplicacion usa
        /// minutos/60 SIN redondear -igual que hoy hace con Horas100-. Con
        /// sueldo 1850, jornada 8 (divisor 240) y factor 100% en 2.0:
        /// horaOrdinaria = 1850/240 = 7.708333... ; hora100 = 15.416666...
        /// 660 minutos son 11 horas exactas, y 11 * 15.416666... = 169.58333...,
        /// que redondeado alejandose del cero a 2 decimales da 169.58.
        ///
        /// El nombre dice "no redondea antes de multiplicar": si alguien
        /// redondeara horaOrdinaria u hora100 a 6 decimales antes de esta
        /// multiplicacion, el resultado cambiaria de centavos, igual que en
        /// Redondeo_SoloAlFinal_NoDuranteLaCadena.
        /// </summary>
        [TestMethod]
        public void Calcular_ConMinutos_NoRedondeaAntesDeMultiplicar()
        {
            EntHeResultado r = NegHorasExtras.Calcular(
                Insumo(1850m, 8, 0m, 0m, minutos100: 660), Par());

            Assert.AreEqual(169.58m, r.Total100);
        }

        /// <summary>
        /// TotalHoras SI se redondea a 2 decimales cuando vienen minutos,
        /// aunque los totales en dinero se calculen sin redondear. 100
        /// minutos al 50% son 1.6666...horas, que en la columna de horas se
        /// muestran como 1.67 -no 1.6666...-.
        /// </summary>
        [TestMethod]
        public void Calcular_ConMinutos_HorasMostradasRedondeadas()
        {
            EntHeResultado r = NegHorasExtras.Calcular(
                Insumo(1200m, 8, 0m, 0m, minutos50: 100), Par());

            Assert.AreEqual(1.67m, r.TotalHoras);
        }

        /// <summary>
        /// Sin minutos (el caso de hoy, y el de toda fila que todavia no usa
        /// decision por tramo), el comportamiento es identico al de antes de
        /// esta tarea: mismos totales que Caso3, con TotalHoras SIN redondear
        /// -12m exacto, que aqui coincide con 12.00 porque no hay fraccion-.
        /// </summary>
        [TestMethod]
        public void Calcular_SinMinutos_IgualQueAntes()
        {
            EntHeResultado r = NegHorasExtras.Calcular(Insumo(600m, 8, 8m, 4m), Par());

            Assert.AreEqual(2.50m, r.ValorHoraOrdinaria);
            Assert.AreEqual(30.00m, r.Total50);
            Assert.AreEqual(20.00m, r.Total100);
            Assert.AreEqual(50.00m, r.TotalHE);
            Assert.AreEqual(12m, r.TotalHoras);
        }

        /// <summary>
        /// Minutos negativos son el mismo dato malo que horas negativas hoy:
        /// -300 minutos son -5 horas, y el bloqueo (advertencia, totales en
        /// cero) tiene que dispararse igual que en Caso9.
        /// </summary>
        [TestMethod]
        public void Calcular_ConMinutosNegativos_Advertencia()
        {
            EntHeResultado r = NegHorasExtras.Calcular(
                Insumo(1200m, 8, 0m, 0m, minutos50: -300), Par());

            Assert.AreEqual(0.00m, r.Total50);
            Assert.AreEqual(0.00m, r.Total100);
            Assert.AreEqual(0.00m, r.TotalHE);
            Assert.AreEqual(0.00m, r.TotalHoras);
            Assert.IsTrue(r.TieneAdvertencia);
        }

        /// <summary>
        /// Round 1 de revision: la salida temprana de salario/divisor en cero
        /// tambien tiene que redondear TotalHoras a 2 decimales cuando hay
        /// minutos, igual que el flujo principal -antes de esta correccion
        /// se quedaba sin redondear en esta rama, inconsistente con
        /// Calcular_ConMinutos_HorasMostradasRedondeadas-.
        ///
        /// 100 minutos al 50% + 70 minutos al 100% = 170/60 = 2.8333...
        /// horas, que redondeado alejandose del cero a 2 decimales da 2.83.
        /// Con salario 0 la fila queda en advertencia y sin pago, pero las
        /// horas si se muestran -son dato valido, lo que falta es el sueldo-.
        /// </summary>
        [TestMethod]
        public void Calcular_ConMinutos_SalarioCero_TotalHorasRedondeado()
        {
            EntHeResultado r = NegHorasExtras.Calcular(
                Insumo(0m, 8, 0m, 0m, minutos50: 100, minutos100: 70), Par());

            Assert.IsTrue(r.TieneAdvertencia);
            Assert.AreEqual(0.00m, r.Total50);
            Assert.AreEqual(0.00m, r.Total100);
            Assert.AreEqual(2.83m, r.TotalHoras);
        }

        /// <summary>
        /// Ambos tramos con minutos a la vez, sueldo normal. Con sueldo 1200,
        /// jornada 8 (divisor 240): horaOrdinaria = 5.00, hora50 = 7.50,
        /// hora100 = 10.00.
        ///
        /// 100 minutos al 50% = 100/60 horas; multiplicado por 7.50 cancela
        /// el 60 exacto: (100*7.5)/60 = 750/60 = 12.50 -sin redondeo
        /// intermedio, ver Calcular_ConMinutos_NoRedondeaAntesDeMultiplicar-.
        ///
        /// 70 minutos al 100% = 70/60 horas; por 10.00 da 700/60 = 11.6666...
        /// que redondeado alejandose del cero a 2 decimales da 11.67.
        ///
        /// TotalHE = 12.50 + 11.67 = 24.17. TotalHoras = redondeo(170/60, 2)
        /// = 2.83, igual que en el caso de salario cero de arriba: las horas
        /// digitadas no cambian aunque el sueldo si.
        /// </summary>
        [TestMethod]
        public void Calcular_ConMinutos_AmbosTramos()
        {
            EntHeResultado r = NegHorasExtras.Calcular(
                Insumo(1200m, 8, 0m, 0m, minutos50: 100, minutos100: 70), Par());

            Assert.AreEqual(12.50m, r.Total50);
            Assert.AreEqual(11.67m, r.Total100);
            Assert.AreEqual(24.17m, r.TotalHE);
            Assert.AreEqual(2.83m, r.TotalHoras);
        }

        /// <summary>
        /// Ruling del controlador tras la validacion de aceptacion: el analisis
        /// manual de Nomina redondea UNA sola vez por persona, no una vez por
        /// tramo. Con sueldo 1500, jornada 8 (divisor 240): horaOrdinaria = 6.25,
        /// hora50 = 9.375, hora100 = 12.5.
        ///
        /// 60 minutos al 50% son 1 hora exacta: exacto50 = 1 * 9.375 = 9.375.
        /// 1231 minutos al 100% son 20.51666... horas: exacto100 =
        /// 20.51666... * 12.5 = 256.458333...
        ///
        /// Sumando los EXACTOS antes de redondear: 9.375 + 256.458333... =
        /// 265.833333..., que redondeado UNA vez da 265.83.
        ///
        /// Si se redondeara cada tramo por separado y se sumara -la regla
        /// vieja-: Total50 = Monto(9.375) = 9.38, Total100 =
        /// Monto(256.458333...) = 256.46 (el tercer decimal es 8, sube), y la
        /// suma daria 265.84 -un centavo de mas, y distinto del analisis
        /// manual-. Por eso Total100 no se calcula redondeando su propio
        /// exacto: sale por diferencia (TotalHE - Total50), para que las dos
        /// columnas sumen siempre exactamente el total ya redondeado.
        /// </summary>
        [TestMethod]
        public void Calcular_ConMinutos_RedondeaUnaVezPorPersona()
        {
            EntHeResultado r = NegHorasExtras.Calcular(
                Insumo(1500m, 8, 0m, 0m, minutos50: 60, minutos100: 1231), Par());

            Assert.AreEqual(265.83m, r.TotalHE, "No 265.84: ese seria redondear cada tramo por separado.");
            Assert.AreEqual(9.38m, r.Total50);
            Assert.AreEqual(256.45m, r.Total100);
        }

        /// <summary>
        /// Total50 + Total100 tiene que dar EXACTAMENTE TotalHE, siempre, con
        /// minutos que no sean multiplos limpios de 60 en ninguno de los dos
        /// tramos -es la garantia que motiva la regla de "un solo redondeo por
        /// persona": las columnas que ve Nomina en pantalla tienen que sumar el
        /// total que se paga, centavo a centavo, sin que quede una diferencia
        /// de redondeo escondida en ninguna fila.
        /// </summary>
        [TestMethod]
        public void Calcular_ConMinutos_TotalesSumanTotalHE()
        {
            EntHeResultado r1 = NegHorasExtras.Calcular(
                Insumo(1500m, 8, 0m, 0m, minutos50: 60, minutos100: 1231), Par());
            Assert.AreEqual(r1.TotalHE, r1.Total50 + r1.Total100);

            EntHeResultado r2 = NegHorasExtras.Calcular(
                Insumo(973m, 4, 0m, 0m, minutos50: 37, minutos100: 193), Par());
            Assert.AreEqual(r2.TotalHE, r2.Total50 + r2.Total100);
        }

        /// <summary>
        /// Round de revision de Task 3b: Total100 sale por diferencia
        /// (TotalHE - Total50), no de redondear su propio exacto -ver el
        /// comentario en Calcular-. Esta prueba fija a proposito el caso mas
        /// exigente para esa resta: que Total50 se lleve el maximo posible de
        /// centavos (un exacto justo en el limite de redondeo) mientras el
        /// otro tramo aporta poco.
        ///
        /// Sueldo 1200, jornada 8 (divisor 240): horaOrdinaria = 5.00,
        /// hora50 = 7.50, hora100 = 10.00.
        ///
        /// 1 minuto al 50% = 1/60 horas; exacto50 = (1/60)*7.50 = 0.125
        /// -EXACTO en el limite de redondeo: el tercer decimal es 5, y
        /// alejandose del cero sube-. Total50 = Monto(0.125) = 0.13: se
        /// llevo un centavo mas del que su propio exacto tenia.
        ///
        /// 1 minuto al 100% = 1/60 horas; exacto100 = (1/60)*10.00 =
        /// 0.16666..., deliberadamente chico frente a ese centavo de mas.
        /// Suma exacta = 0.29166... -> TotalHE = 0.29. Total100 = 0.29 - 0.13
        /// = 0.16: no negativo, y las dos columnas siguen sumando el total.
        ///
        /// Matematicamente esto no puede fallar -Monto() es monotona no
        /// decreciente, asi que Total50 nunca puede superar a TotalHE-, pero
        /// es la clase de prueba barata que deja el invariante escrito en vez
        /// de confiar en el argumento a mano.
        /// </summary>
        [TestMethod]
        public void Calcular_ConMinutos_Total100NuncaNegativo()
        {
            EntHeResultado r = NegHorasExtras.Calcular(
                Insumo(1200m, 8, 0m, 0m, minutos50: 1, minutos100: 1), Par());

            Assert.IsTrue(r.Total100 >= 0m,
                "Total50 redondeado hacia arriba no puede llevarse mas centavos de los que hay en TotalHE.");
            Assert.AreEqual(r.TotalHE, r.Total50 + r.Total100);
            Assert.AreEqual(0.13m, r.Total50);
            Assert.AreEqual(0.16m, r.Total100);
            Assert.AreEqual(0.29m, r.TotalHE);
        }
    }
}
