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

        private static ExcelWorksheet Abrir(byte[] bytes)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage paquete = new ExcelPackage(new MemoryStream(bytes));
            return paquete.Workbook.Worksheets[0];
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
    }
}
