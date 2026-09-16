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
    /// columna fija, a proposito: el orden exacto de las 22 columnas no es
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
            p.Periodo.Anio = 2026;
            p.Periodo.Mes = 9;
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
            p.Periodo.Anio = 2026;
            p.Periodo.Mes = 9;
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
        public void NombreDeArchivo_LlevaElPeriodoYLaExtension()
        {
            EntHePeriodo p = new EntHePeriodo();
            p.Anio = 2026;
            p.Mes = 9;

            string nombre = NegHeExportacion.NombreDeArchivo(p);

            StringAssert.Contains(nombre, "2026");
            StringAssert.Contains(nombre, "09");
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
            Assert.IsTrue(FilaContiene(hoja, fila, "09/2026"), "No se encontro una columna con el periodo repetido en la fila.");
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
                Assert.AreEqual(2, paquete.Workbook.Worksheets.Count, "El archivo deberia tener dos hojas: detalle y pagos.");

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
    }
}
