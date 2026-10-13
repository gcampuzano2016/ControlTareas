using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System;
using System.IO;

namespace CapaPruebas
{
    /// <summary>
    /// El archivo se arma en memoria, asi que se puede probar entero sin tocar
    /// disco ni base: se construye, se vuelve a abrir con EPPlus y se leen las
    /// celdas.
    ///
    /// Muchas de estas pruebas recorren toda la fila en vez de apuntar a una
    /// columna fija, a proposito: el orden exacto de las 23 columnas no es
    /// parte del contrato (solo lo son las constantes publicas de
    /// NegHeExportacion), asi que una prueba que dependiera de un numero de
    /// columna que no sea publico se romperia con un reacomodo inocente.
    ///
    /// Los datos de estas pruebas son inventados a proposito. El repositorio es
    /// publico y un fixture con una cedula o un sueldo real seria una fuga.
    /// </summary>
    [TestClass]
    public class NegHeExportacionTests
    {
        private static EntHePantalla PantallaDeEjemplo(string estado)
        {
            EntHePantalla p = new EntHePantalla();
            p.Periodo = new EntHePeriodo();
            p.Periodo.IdPeriodo = 7;
            p.Periodo.FechaInicio = new DateTime(2026, 8, 17);
            p.Periodo.FechaFin = new DateTime(2026, 9, 15);
            p.Periodo.EstadoPeriodo = estado;

            EntHeFila f = new EntHeFila();
            f.IdEmpleado = 4242;
            f.CedulaSnapshot = "0000000000";
            f.NombreSnapshot = "PERSONA DE PRUEBA";
            f.EmpresaSnapshot = "EMPRESA DE PRUEBA";
            f.CargoSnapshot = "CARGO DE PRUEBA";
            f.JornadaHorasDiaSnapshot = 8;
            f.SalarioBaseSnapshot = 1200m;
            f.AplicaHESnapshot = true;
            f.Divisor = 240;
            f.ValorHoraOrdinaria = 5m;
            f.ValorHora50 = 7.5m;
            f.ValorHora100 = 10m;
            f.Horas50 = 10m;
            f.Horas100 = 2m;
            f.Total50 = 75m;
            f.Total100 = 20m;
            f.TotalHoras = 12m;
            f.TotalHE = 95m;
            f.Observacion = "";
            p.Filas.Add(f);

            p.TotalHoras50 = 10m;
            p.TotalHoras100 = 2m;
            p.TotalPago50 = 75m;
            p.TotalPago100 = 20m;
            p.TotalHoras = 12m;
            p.TotalPagar = 95m;
            return p;
        }

        /// <summary>
        /// Persona a la que le apagaron la elegibilidad (AplicaHESnapshot=false)
        /// pero que conserva horas guardadas de antes -abrir un periodo
        /// preserva las horas anteriores aunque el maestro haya cambiado desde
        /// entonces-. TieneAdvertencia se deja en false a proposito: si la
        /// exportacion dependiera solo de ese campo, este caso pasaria
        /// desapercibido.
        /// </summary>
        private static EntHePantalla PantallaConFilaQueNoAplicaPeroTieneHoras()
        {
            EntHePantalla p = new EntHePantalla();
            p.Periodo = new EntHePeriodo();
            p.Periodo.FechaInicio = new DateTime(2026, 8, 17);
            p.Periodo.FechaFin = new DateTime(2026, 9, 15);
            p.Periodo.EstadoPeriodo = "Cerrado";

            EntHeFila f = new EntHeFila();
            f.IdEmpleado = 4343;
            f.CedulaSnapshot = "0000000001";
            f.NombreSnapshot = "PERSONA SIN ELEGIBILIDAD";
            f.EmpresaSnapshot = "EMPRESA DE PRUEBA";
            f.CargoSnapshot = "CARGO DE PRUEBA";
            f.JornadaHorasDiaSnapshot = 8;
            f.SalarioBaseSnapshot = 1200m;
            f.AplicaHESnapshot = false;
            f.MotivoNoAplica = "";
            f.Divisor = 240;
            f.ValorHoraOrdinaria = 5m;
            f.ValorHora50 = 7.5m;
            f.ValorHora100 = 10m;
            f.Horas50 = 8m;
            f.Horas100 = 0m;
            f.Total50 = 0m;
            f.Total100 = 0m;
            f.TotalHoras = 8m;
            f.TotalHE = 0m;
            f.TieneAdvertencia = false;
            f.Observacion = "";
            p.Filas.Add(f);

            p.TotalHoras50 = 8m;
            p.TotalHoras100 = 0m;
            p.TotalPago50 = 0m;
            p.TotalPago100 = 0m;
            p.TotalHoras = 8m;
            p.TotalPagar = 0m;
            return p;
        }

        private static ExcelWorksheet Abrir(byte[] bytes)
        {
            return AbrirPaquete(bytes).Workbook.Worksheets[0];
        }

        private static ExcelPackage AbrirPaquete(byte[] bytes)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return new ExcelPackage(new MemoryStream(bytes));
        }

        /// <summary>Busca un valor exacto en cualquier columna de una fila. Sirve para no atarse a un numero de columna que no es publico.</summary>
        private static bool FilaContiene(ExcelWorksheet hoja, int fila, string valorBuscado)
        {
            for (int c = 1; c <= 30; c++)
            {
                object valor = hoja.Cells[fila, c].Value;
                if (valor != null && Convert.ToString(valor) == valorBuscado)
                {
                    return true;
                }
            }

            return false;
        }

        [TestMethod]
        public void Construir_DevuelveUnXlsxQueSePuedeAbrir()
        {
            byte[] bytes = NegHeExportacion.Construir(PantallaDeEjemplo("Cerrado"));

            Assert.IsTrue(bytes.Length > 0);
            Assert.IsNotNull(Abrir(bytes));
        }

        /// <summary>
        /// El dinero tiene que salir como NUMERO, no como texto. Si saliera
        /// como texto, quien reciba el archivo no puede sumarlo ni pasarlo a
        /// su proceso sin convertirlo a mano, y una columna de 62 filas
        /// convertida a mano es justo donde se cuelan los errores.
        /// </summary>
        [TestMethod]
        public void Construir_ElDineroEsNumeroNoTexto()
        {
            byte[] bytes = NegHeExportacion.Construir(PantallaDeEjemplo("Cerrado"));
            ExcelWorksheet hoja = Abrir(bytes);

            object total = hoja.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaTotalHE].Value;

            Assert.IsInstanceOfType(total, typeof(double), "el total salio como " + total.GetType().Name);
            Assert.AreEqual(95d, (double)total, 0.001);
        }

        /// <summary>
        /// El valor hora se calcula y se guarda con 6 decimales (DECIMAL(18,6)
        /// en la base). Si el archivo lo mostrara con dos, Nomina no podria
        /// reproducir horas x tarifa = total: veria 5,00 x 10 y esperaria
        /// 50,00 donde la fila dice 50,04.
        /// </summary>
        [TestMethod]
        public void Construir_ElValorHoraSeMuestraConSeisDecimales()
        {
            byte[] bytes = NegHeExportacion.Construir(PantallaDeEjemplo("Cerrado"));
            ExcelWorksheet hoja = Abrir(bytes);

            string formato = hoja.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaValorHoraOrdinaria]
                .Style.Numberformat.Format;

            StringAssert.Contains(formato, "0.000000");
        }

        /// <summary>
        /// Un periodo abierto son cifras provisionales, y el archivo tiene que
        /// decirlo: si no, un borrador descargado el dia 20 es indistinguible
        /// del definitivo y alguien puede pagar con el.
        /// </summary>
        [TestMethod]
        public void Construir_PeriodoAbierto_LoDiceEnElArchivo()
        {
            ExcelWorksheet abierto = Abrir(NegHeExportacion.Construir(PantallaDeEjemplo("Abierto")));
            ExcelWorksheet cerrado = Abrir(NegHeExportacion.Construir(PantallaDeEjemplo("Cerrado")));

            string textoAbierto = Convert.ToString(abierto.Cells[NegHeExportacion.FilaDeEstado, 1].Value);
            string textoCerrado = Convert.ToString(cerrado.Cells[NegHeExportacion.FilaDeEstado, 1].Value);

            StringAssert.Contains(textoAbierto.ToUpperInvariant(), "PROVISIONAL");
            Assert.IsFalse(textoCerrado.ToUpperInvariant().Contains("PROVISIONAL"));
        }

        /// <summary>
        /// Un periodo anulado no es un borrador por firmar -es justo lo
        /// contrario: esto no se paga nunca-. Decirle PROVISIONAL es la
        /// confusion que esa palabra existe para evitar, invertida.
        /// </summary>
        [TestMethod]
        public void Construir_PeriodoAnulado_NoDicePROVISIONAL()
        {
            ExcelWorksheet hoja = Abrir(NegHeExportacion.Construir(PantallaDeEjemplo("Anulado")));

            string texto = Convert.ToString(hoja.Cells[NegHeExportacion.FilaDeEstado, 1].Value);

            StringAssert.Contains(texto, "Anulado");
            Assert.IsFalse(texto.ToUpperInvariant().Contains("PROVISIONAL"), "Un periodo anulado no debe decir PROVISIONAL.");
        }

        [TestMethod]
        public void NombreDeArchivo_LlevaElRangoYLaExtension()
        {
            EntHePeriodo p = new EntHePeriodo();
            p.FechaInicio = new DateTime(2026, 8, 17);
            p.FechaFin = new DateTime(2026, 9, 15);

            string nombre = NegHeExportacion.NombreDeArchivo(p);

            /* El rango completo, no el mes: dos periodos del mismo mes -dos
               quincenas- tienen que dar nombres distintos. */
            StringAssert.Contains(nombre, "20260817_20260915");
            StringAssert.EndsWith(nombre, ".xlsx");
        }

        /// <summary>
        /// Un periodo recien abierto sin nadie dentro no debe reventar la
        /// descarga: devuelve un archivo con sus encabezados y sin filas.
        /// </summary>
        [TestMethod]
        public void Construir_SinFilas_NoLanzaYDevuelveArchivo()
        {
            EntHePantalla vacia = PantallaDeEjemplo("Abierto");
            vacia.Filas.Clear();

            byte[] bytes = NegHeExportacion.Construir(vacia);

            Assert.IsTrue(bytes.Length > 0);
        }

        /// <summary>
        /// NegHorasExtrasPantalla.CargarPantalla devuelve Periodo nulo como
        /// resultado legitimo cuando el periodo no existe. Si eso llegara aqui
        /// sin que quien llama lo haya comprobado antes, el error debe decir
        /// que falta, no ser una NullReferenceException muda.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Construir_PeriodoNulo_LanzaExcepcionClara()
        {
            EntHePantalla pantalla = new EntHePantalla();
            pantalla.Filas.Add(new EntHeFila());

            NegHeExportacion.Construir(pantalla);
        }

        /// <summary>
        /// Los encabezados los lee una persona: llevan tildes. Se busca el
        /// texto exacto con tilde en la fila de encabezados -no una columna
        /// fija, que no es parte del contrato publico-.
        /// </summary>
        [TestMethod]
        public void Construir_LosEncabezadosLlevanTildes()
        {
            ExcelWorksheet hoja = Abrir(NegHeExportacion.Construir(PantallaDeEjemplo("Cerrado")));
            int filaDeEncabezados = NegHeExportacion.PrimeraFilaDeDatos - 1;

            Assert.IsTrue(
                FilaContiene(hoja, filaDeEncabezados, "Cédula"),
                "No se encontro el encabezado 'Cédula' con tilde.");
        }

        /// <summary>
        /// Caso real y alcanzable: a alguien le apagan AplicaHE entre un cierre
        /// y una reapertura, pero sus horas cargadas antes se conservan. El
        /// calculo ya dice "quien no aplica no cobra, aunque lleguen horas",
        /// y sin marcar la fila, 8 horas a 7,50 mostrando un total en 0,00 es
        /// indistinguible de un error de calculo para quien abre el archivo.
        /// </summary>
        [TestMethod]
        public void Construir_FilaQueNoAplicaConHoras_QuedaMarcadaConAdvertencia()
        {
            ExcelWorksheet hoja = Abrir(NegHeExportacion.Construir(PantallaConFilaQueNoAplicaPeroTieneHoras()));

            Assert.IsTrue(
                FilaContiene(hoja, NegHeExportacion.PrimeraFilaDeDatos, "Sí"),
                "La fila que no aplica pero tiene horas deberia llevar alguna marca de advertencia en 'Sí'.");
        }

        /// <summary>
        /// La cedula sola no basta para cruzar contra el maestro -hay
        /// candidatos repetidos-, asi que IdEmpleado tiene que viajar. El
        /// periodo va repetido en cada fila para que sobreviva si Nomina pega
        /// varios meses en una sola hoja consolidada.
        /// </summary>
        [TestMethod]
        public void Construir_ExportaIdEmpleadoYPeriodoPorFila()
        {
            ExcelWorksheet hoja = Abrir(NegHeExportacion.Construir(PantallaDeEjemplo("Cerrado")));
            int fila = NegHeExportacion.PrimeraFilaDeDatos;

            Assert.IsTrue(FilaContiene(hoja, fila, "4242"), "No se encontro el IdEmpleado en ninguna columna.");
            Assert.IsTrue(FilaContiene(hoja, fila, "17/08/2026 - 15/09/2026"), "No se encontro una columna con el periodo repetido en la fila.");
        }

        /// <summary>
        /// Nomina tiene que poder distinguir lo que sembro el sistema desde
        /// las tareas aprobadas de lo que corrigio una persona: sin esa
        /// columna, las dos cosas se ven igual en el archivo.
        /// </summary>
        [TestMethod]
        public void Construir_ExportaElOrigenDeLasHoras()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Cerrado");
            pantalla.Filas[0].HorasOrigen = "Manual";

            ExcelWorksheet hoja = Abrir(NegHeExportacion.Construir(pantalla));

            Assert.IsTrue(
                FilaContiene(hoja, NegHeExportacion.PrimeraFilaDeDatos - 1, "Origen de las Horas"),
                "Falta el encabezado de la columna de origen.");
            Assert.IsTrue(
                FilaContiene(hoja, NegHeExportacion.PrimeraFilaDeDatos, "Manual"),
                "No se encontro el origen de las horas en ninguna columna de la fila.");
        }

        /// <summary>
        /// El proceso de pago consume una lista de pagos, no un padron: una
        /// segunda hoja con solo quien cobra evita la conversacion de "esto no
        /// es lo que necesito".
        /// </summary>
        [TestMethod]
        public void Construir_TieneHojaDePagosConSoloFilasConTotalPositivo()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Cerrado");

            EntHeFila sinPago = new EntHeFila();
            sinPago.IdEmpleado = 5151;
            sinPago.CedulaSnapshot = "0000000002";
            sinPago.NombreSnapshot = "PERSONA SIN PAGO";
            sinPago.EmpresaSnapshot = "EMPRESA DE PRUEBA";
            sinPago.CargoSnapshot = "CARGO DE PRUEBA";
            sinPago.JornadaHorasDiaSnapshot = 8;
            sinPago.SalarioBaseSnapshot = 0m;
            sinPago.AplicaHESnapshot = true;
            sinPago.Divisor = 240;
            sinPago.TotalHE = 0m;
            sinPago.Observacion = "";
            pantalla.Filas.Add(sinPago);

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla)))
            {
                Assert.AreEqual(6, paquete.Workbook.Worksheets.Count, "El archivo deberia tener seis hojas: detalle, pagos, fuera de horario, fuera del modulo, conciliacion y decisiones.");

                ExcelWorksheet hojaPagos = paquete.Workbook.Worksheets[1];
                Assert.IsTrue(
                    FilaContiene(hojaPagos, NegHeExportacion.PrimeraFilaDeDatos, "PERSONA DE PRUEBA"),
                    "La hoja de pagos deberia incluir a quien si cobra.");
                Assert.IsFalse(
                    FilaContiene(hojaPagos, NegHeExportacion.PrimeraFilaDeDatos, "PERSONA SIN PAGO")
                        || FilaContiene(hojaPagos, NegHeExportacion.PrimeraFilaDeDatos + 1, "PERSONA SIN PAGO"),
                    "La hoja de pagos no deberia incluir filas con total en cero.");
            }
        }

        private static EntHeFueraDeHorario FueraDeEjemplo(long idEmpleado)
        {
            EntHeFueraDeHorario f = new EntHeFueraDeHorario();
            f.Calculado = true;
            f.Personas.Add(new EntHeFueraDeHorarioPersona
            {
                IdEmpleado = idEmpleado,
                MinutosSinAprobar50 = 60,
                MinutosRechazado100 = 30
            });
            f.Detalle.Add(new EntHeFueraDeHorarioDetalle
            {
                IdEmpleado = idEmpleado,
                IdRegDetTareas = 555,
                TramoInicio = new DateTime(2026, 8, 18, 8, 0, 0),
                TramoFin = new DateTime(2026, 8, 18, 9, 0, 0),
                Inicio = new DateTime(2026, 8, 18, 8, 0, 0),
                Fin = new DateTime(2026, 8, 18, 9, 0, 0),
                Minutos = 60,
                TipoHora = 1,
                Categoria = NegHeFueraDeHorario.CategoriaSinAprobar,
                EstadoTexto = "Sin solicitar",
                Actividad = "ACTIVIDAD DE PRUEBA"
            });

            // Una fila de conciliacion y un candidato para la misma persona: las
            // pruebas de las hojas "Conciliación" y "Decisiones" parten de aqui y
            // agregan lo que les falta (otra persona, Desactualizada, etc).
            f.Conciliacion.Add(new EntHeConciliacionFila
            {
                IdEmpleado = idEmpleado,
                MinutosAprobadoIncluido = 60,
                MinutosNoAprobadoIncluido = 30,
                MinutosExcluido = 15,
                UsdAprobadoIncluido = 10m,
                UsdNoAprobadoIncluido = 5m,
                UsdExcluido = 2.5m,
                TotalAPagar = 15m
            });
            f.Candidatos.Add(new EntHeCandidato
            {
                IdEmpleado = idEmpleado,
                IdRegDetTareas = 555,
                TramoInicio = new DateTime(2026, 8, 18, 8, 0, 0),
                TramoFin = new DateTime(2026, 8, 18, 9, 0, 0),
                SegmentoInicio = new DateTime(2026, 8, 18, 8, 0, 0),
                SegmentoFin = new DateTime(2026, 8, 18, 9, 0, 0),
                TipoHora = 1,
                EstadoTramo = 0,
                TipoRegistrado = 0,
                Actividad = "ACTIVIDAD DE PRUEBA",
                DentroDelPeriodo = true,
                Incluir = true,
                TipoElegido = 1,
                ValorUsd = 7.5m
            });
            return f;
        }

        [TestMethod]
        public void Construir_ConFueraDeHorario_ColumnasSoloEnElDetalle()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, FueraDeEjemplo(4242))))
            {
                ExcelWorksheet detalle = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaDetalle];
                ExcelWorksheet pagos = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaPagos];

                Assert.AreEqual("Fuera de horario sin aprobar (h)",
                    Convert.ToString(detalle.Cells[6, NegHeExportacion.ColumnaFueraSinAprobar].Value));
                Assert.AreEqual(1.00m,
                    Convert.ToDecimal(detalle.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaFueraSinAprobar].Value));
                Assert.AreEqual(0.50m,
                    Convert.ToDecimal(detalle.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaFueraRechazado].Value));

                Assert.IsNull(pagos.Cells[6, NegHeExportacion.ColumnaFueraSinAprobar].Value,
                    "La hoja de pagos no lleva las columnas informativas: podrian sumarse al rol por error.");
            }
        }

        [TestMethod]
        public void Construir_ConFueraDeHorario_HojaConElDetalle()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, FueraDeEjemplo(4242))))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDeHorario];
                Assert.IsNotNull(hoja);

                int fila = NegHeExportacion.FueraPrimeraFilaDeDatos;
                Assert.AreEqual("PERSONA DE PRUEBA", Convert.ToString(hoja.Cells[fila, 2].Value));
                Assert.AreEqual("08:00 - 09:00", Convert.ToString(hoja.Cells[fila, 5].Value));
                Assert.AreEqual("Sin solicitar", Convert.ToString(hoja.Cells[fila, 9].Value));
                Assert.AreEqual(555L, Convert.ToInt64(hoja.Cells[fila, 11].Value));
            }
        }

        [TestMethod]
        public void Construir_SinCalculo_HojaConMensaje()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla)))
            {
                Assert.AreEqual(6, paquete.Workbook.Worksheets.Count);

                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDeHorario];
                StringAssert.StartsWith(
                    Convert.ToString(hoja.Cells[NegHeExportacion.FueraPrimeraFilaDeDatos, 1].Value),
                    "No se pudo calcular el tiempo fuera de horario");

                ExcelWorksheet detalle = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaDetalle];
                Assert.IsNull(detalle.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaFueraSinAprobar].Value,
                    "Sin calculo las columnas quedan vacias, no en cero: cero diria que no hay nada.");
            }
        }

        [TestMethod]
        public void Construir_CalculoConError_DiceElMotivo()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");
            EntHeFueraDeHorario fallido = new EntHeFueraDeHorario { Calculado = false, Error = "timeout" };

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, fallido)))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDeHorario];
                StringAssert.Contains(Convert.ToString(hoja.Cells[NegHeExportacion.FueraPrimeraFilaDeDatos, 1].Value), "timeout");
            }
        }

        [TestMethod]
        public void Construir_PersonaFueraDeLaGrilla_NoFalla()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, FueraDeEjemplo(9999))))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDeHorario];
                Assert.AreEqual(9999L, Convert.ToInt64(hoja.Cells[NegHeExportacion.FueraPrimeraFilaDeDatos, 1].Value));
                Assert.AreEqual("", Convert.ToString(hoja.Cells[NegHeExportacion.FueraPrimeraFilaDeDatos, 2].Value ?? ""));

                ExcelWorksheet detalle = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaDetalle];
                Assert.AreEqual(0m,
                    Convert.ToDecimal(detalle.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaFueraSinAprobar].Value),
                    "La persona de la grilla no tiene nada fuera de horario.");
            }
        }

        [TestMethod]
        public void Construir_TotalesDelDetalle_SoloSumanLaGrilla()
        {
            // Alguien activo en el maestro pero no sembrado en el periodo: aparece en
            // la tercera hoja, pero el TOTAL de la columna tiene que cuadrar con la
            // suma de la columna, o nadie puede conciliarlo.
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, FueraDeEjemplo(9999))))
            {
                ExcelWorksheet detalle = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaDetalle];

                int filaTotales = -1;
                for (int f = NegHeExportacion.PrimeraFilaDeDatos; f <= NegHeExportacion.PrimeraFilaDeDatos + 5; f++)
                {
                    if (Convert.ToString(detalle.Cells[f, 1].Value) == "TOTALES") { filaTotales = f; }
                }

                Assert.IsTrue(filaTotales > 0, "No se encontro la fila TOTALES.");
                Assert.AreEqual(0m, Convert.ToDecimal(detalle.Cells[filaTotales, NegHeExportacion.ColumnaFueraSinAprobar].Value));
                Assert.AreEqual(0m, Convert.ToDecimal(detalle.Cells[filaTotales, NegHeExportacion.ColumnaFueraRechazado].Value));
            }
        }

        [TestMethod]
        public void Construir_FueraDelModulo_HojaConLaPersona()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");
            EntHeFueraDeHorario fuera = FueraDeEjemplo(4242);
            fuera.FueraDelModulo.Add(new EntHeFueraDelModuloPersona
            {
                IdEmpleado = 777,
                Nombre = "PERSONA FUERA DEL MODULO",
                Cargo = "CARGO DE PRUEBA",
                MinutosAprobados100 = 660,
                MinutosSinAprobar50 = 30
            });

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, fuera)))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDelModulo];
                Assert.IsNotNull(hoja);

                int fila = NegHeExportacion.FueraPrimeraFilaDeDatos;
                Assert.AreEqual(777L, Convert.ToInt64(hoja.Cells[fila, 1].Value));
                Assert.AreEqual("PERSONA FUERA DEL MODULO", Convert.ToString(hoja.Cells[fila, 2].Value));
                Assert.AreEqual(11.00m, Convert.ToDecimal(hoja.Cells[fila, 5].Value), "Aprobadas 100%.");
                Assert.AreEqual(0.50m, Convert.ToDecimal(hoja.Cells[fila, 6].Value), "Sin aprobar.");
                StringAssert.Contains(Convert.ToString(hoja.Cells[fila, 8].Value), "faltan sus datos de RRHH");
            }
        }

        [TestMethod]
        public void Construir_FueraDelModuloVacio_LoDice()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, FueraDeEjemplo(4242))))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDelModulo];
                StringAssert.StartsWith(Convert.ToString(hoja.Cells[NegHeExportacion.FueraPrimeraFilaDeDatos, 1].Value),
                                        "No hay personas");
            }
        }

        [TestMethod]
        public void Construir_FueraDelModuloSinCalculo_DiceQueNoSePudo()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla)))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDelModulo];
                StringAssert.StartsWith(Convert.ToString(hoja.Cells[NegHeExportacion.FueraPrimeraFilaDeDatos, 1].Value),
                                        "No se pudo calcular");
            }
        }

        /// <summary>
        /// El total de la fila TOTALES no es una suma nueva inventada por la hoja:
        /// tiene que coincidir con la suma de TotalAPagar (el TotalHE real de cada
        /// persona) que ya trae la conciliacion, aunque haya mas de una fila.
        /// </summary>
        [TestMethod]
        public void Construir_Conciliacion_TotalIgualSumaDeTotalHE()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");
            EntHeFueraDeHorario fuera = FueraDeEjemplo(4242);
            fuera.Conciliacion.Add(new EntHeConciliacionFila
            {
                IdEmpleado = 9999,
                Nombre = "OTRA PERSONA",
                MinutosAprobadoIncluido = 120,
                UsdAprobadoIncluido = 20m,
                TotalAPagar = 20m
            });

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, fuera)))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaConciliacion];

                decimal sumaEsperada = 0m;
                foreach (EntHeConciliacionFila c in fuera.Conciliacion) { sumaEsperada += c.TotalAPagar; }

                int filaTotales = -1;
                for (int f = NegHeExportacion.ConciliacionPrimeraFilaDeDatos; f <= NegHeExportacion.ConciliacionPrimeraFilaDeDatos + 10; f++)
                {
                    if (Convert.ToString(hoja.Cells[f, 1].Value) == "TOTALES") { filaTotales = f; }
                }

                Assert.IsTrue(filaTotales > 0, "No se encontro la fila TOTALES.");
                Assert.AreEqual(sumaEsperada, Convert.ToDecimal(hoja.Cells[filaTotales, 9].Value),
                    "El TOTALES de 'Total a pagar (USD)' deberia ser la suma de TotalAPagar de cada fila.");
            }
        }

        /// <summary>
        /// Una fila cuyo TotalHE no coincide con lo que pagan sus minutos incluidos
        /// hoy queda marcada en la columna Estado, y el aviso aparece debajo del
        /// titulo: Nomina no debe usar este archivo sin recalcular antes.
        /// </summary>
        [TestMethod]
        public void Construir_Conciliacion_MarcaDesactualizada()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");
            EntHeFueraDeHorario fuera = FueraDeEjemplo(4242);
            fuera.Conciliacion[0].Desactualizada = true;

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, fuera)))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaConciliacion];

                string estado = Convert.ToString(hoja.Cells[NegHeExportacion.ConciliacionPrimeraFilaDeDatos, 11].Value);
                StringAssert.Contains(estado, "Desactualizada");
                StringAssert.Contains(estado, "Traer aprobaciones nuevas");

                // El aviso tiene que nombrar el boton real de la pantalla -«Traer
                // aprobaciones nuevas»-, no un «Recalcular» que no existe como boton
                // (ver ReporteTareas/Formulario/HorasExtras.aspx, btnRecalcular).
                string aviso = Convert.ToString(hoja.Cells[3, 1].Value ?? "");
                StringAssert.Contains(aviso, "1", "El aviso deberia decir cuantas filas estan desactualizadas.");
                StringAssert.Contains(aviso, "Traer aprobaciones nuevas",
                    "El aviso deberia nombrar el boton real de la pantalla.");
            }
        }

        /// <summary>
        /// Con el periodo cerrado el boton «Traer aprobaciones nuevas» no aparece:
        /// el aviso y la columna Estado dicen que hay que reabrirlo, igual que la
        /// pantalla (AccionParaActualizar en horasExtras.js).
        /// </summary>
        [TestMethod]
        public void Construir_Conciliacion_DesactualizadaEnCerrado_PideReabrir()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Cerrado");
            EntHeFueraDeHorario fuera = FueraDeEjemplo(4243);
            fuera.Conciliacion[0].Desactualizada = true;

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, fuera)))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaConciliacion];

                string aviso = Convert.ToString(hoja.Cells[3, 1].Value ?? "");
                StringAssert.Contains(aviso, "reabra el período para actualizarlo");
                Assert.IsFalse(aviso.Contains("Traer aprobaciones nuevas"), "Con el periodo cerrado ese boton no existe.");

                string estado = Convert.ToString(hoja.Cells[NegHeExportacion.ConciliacionPrimeraFilaDeDatos, 11].Value);
                StringAssert.Contains(estado, "Desactualizada");
                StringAssert.Contains(estado, "reabra el período");
            }
        }

        /// <summary>
        /// Cada segmento candidato -aprobado o no, incluido o no- es una fila propia
        /// en "Decisiones", ordenada por nombre y luego por fecha. Quien no esta en
        /// la grilla (nombres.ContainsKey en false) sale con el nombre vacio, no
        /// falla.
        /// </summary>
        [TestMethod]
        public void Construir_Decisiones_UnaFilaPorCandidato()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");
            EntHeFueraDeHorario fuera = FueraDeEjemplo(4242);
            fuera.Candidatos.Add(new EntHeCandidato
            {
                IdEmpleado = 9999,
                IdRegDetTareas = 777,
                TramoInicio = new DateTime(2026, 8, 20, 7, 0, 0),
                TramoFin = new DateTime(2026, 8, 20, 8, 0, 0),
                SegmentoInicio = new DateTime(2026, 8, 20, 7, 0, 0),
                SegmentoFin = new DateTime(2026, 8, 20, 8, 0, 0),
                TipoHora = 2,
                EstadoTramo = 3,
                TipoRegistrado = 0,
                DentroDelPeriodo = true,
                Incluir = false,
                TipoElegido = 2,
                ValorUsd = 0m
            });

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, fuera)))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaDecisiones];

                // Orden por nombre: 9999 no esta en la grilla ("" ordena antes que
                // "PERSONA DE PRUEBA"), asi que va primero.
                Assert.AreEqual(9999L, Convert.ToInt64(hoja.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos, 1].Value));
                Assert.AreEqual("", Convert.ToString(hoja.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos, 2].Value ?? ""));
                Assert.AreEqual("Rechazado", Convert.ToString(hoja.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos, 6].Value));
                Assert.AreEqual("No", Convert.ToString(hoja.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos, 8].Value));
                Assert.AreEqual(777L, Convert.ToInt64(hoja.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos, 17].Value));

                Assert.AreEqual(4242L, Convert.ToInt64(hoja.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos + 1, 1].Value));
                Assert.AreEqual("PERSONA DE PRUEBA", Convert.ToString(hoja.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos + 1, 2].Value));
                Assert.AreEqual("Sí", Convert.ToString(hoja.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos + 1, 8].Value));
                Assert.AreEqual("50%", Convert.ToString(hoja.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos + 1, 9].Value));
            }
        }

        /// <summary>
        /// Sin calculo (fuera null o Calculado=false) las dos hojas nuevas existen
        /// igual, con el mismo patron de mensaje que las demas hojas informativas.
        /// </summary>
        [TestMethod]
        public void Construir_SinCalculo_ConciliacionYDecisionesConMensaje()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla)))
            {
                Assert.AreEqual(6, paquete.Workbook.Worksheets.Count);

                ExcelWorksheet conciliacion = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaConciliacion];
                StringAssert.StartsWith(
                    Convert.ToString(conciliacion.Cells[NegHeExportacion.ConciliacionPrimeraFilaDeDatos, 1].Value),
                    "No se pudo calcular la conciliación");

                ExcelWorksheet decisiones = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaDecisiones];
                StringAssert.StartsWith(
                    Convert.ToString(decisiones.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos, 1].Value),
                    "No se pudo calcular las decisiones");
            }
        }

        /// <summary>
        /// Calculado = true pero sin nadie que conciliar ni ningun candidato: un
        /// periodo recien abierto sin nadie sembrado. Las dos hojas tienen que
        /// existir con sus encabezados, la fila TOTALES en cero -no el mensaje de
        /// "no se pudo calcular", que es para Calculado = false-, y sin lanzar.
        /// </summary>
        [TestMethod]
        public void Construir_CalculadoSinConciliacionNiCandidatos_HojasEnCeroSinExcepcion()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");
            EntHeFueraDeHorario fuera = new EntHeFueraDeHorario { Calculado = true };

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, fuera)))
            {
                ExcelWorksheet conciliacion = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaConciliacion];
                Assert.IsNotNull(conciliacion);

                int filaTotales = -1;
                for (int f = NegHeExportacion.ConciliacionPrimeraFilaDeDatos; f <= NegHeExportacion.ConciliacionPrimeraFilaDeDatos + 5; f++)
                {
                    if (Convert.ToString(conciliacion.Cells[f, 1].Value) == "TOTALES") { filaTotales = f; }
                }
                Assert.IsTrue(filaTotales > 0, "No se encontro la fila TOTALES.");
                Assert.AreEqual(0m, Convert.ToDecimal(conciliacion.Cells[filaTotales, 9].Value ?? 0m), "Total a pagar en cero.");
                Assert.AreEqual(0m, Convert.ToDecimal(conciliacion.Cells[filaTotales, 10].Value ?? 0m), "Total incluyendo no incluido en cero.");

                ExcelWorksheet decisiones = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaDecisiones];
                Assert.IsNotNull(decisiones);
                Assert.IsNull(decisiones.Cells[NegHeExportacion.DecisionesPrimeraFilaDeDatos, 1].Value,
                    "Sin candidatos no deberia haber ninguna fila de datos.");
            }
        }
    }
}
