using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CapaPruebas
{
    /// <summary>
    /// Las tres funciones que pueden fallar sin dar la cara. Un error aca no
    /// lanza excepcion ni pinta rojo en ningun lado: la edad sale mal, el campo
    /// bloqueado se guarda, el contacto de emergencia queda sin telefono.
    /// </summary>
    [TestClass]
    public class NegPerfilCamposTests
    {
        /* ---------------------------------------------------------- edad ---- */

        /// <summary>
        /// El caso que motiva toda esta funcion. En la base hay 129 fechas en
        /// formato dd/MM/yyyy y 80 de ellas tienen el dia por encima de 12. Si
        /// alguien las interpreta como mm/dd/yyyy, esas 80 fallan y las otras 49
        /// salen bien: el error se ve como "a algunos no les carga la edad".
        /// </summary>
        [TestMethod]
        public void EdadDesdeTexto_DiaMayorQueDoce_NoLoConfundeConElMes()
        {
            int? edad = NegPerfilCampos.EdadDesdeTexto("25/12/1990");

            Assert.IsNotNull(edad, "25/12/1990 es una fecha valida en dd/MM/yyyy");
            Assert.AreEqual(AniosDesde(new DateTime(1990, 12, 25)), edad.Value);
        }

        [TestMethod]
        public void EdadDesdeTexto_DiaMenorQueDoce_LeeElDiaPrimero()
        {
            // 03/07 es 3 de julio, no 7 de marzo. Sin formato explicito esta
            // fecha "funciona" en los dos sentidos y da edades distintas.
            int? edad = NegPerfilCampos.EdadDesdeTexto("03/07/1985");

            Assert.AreEqual(AniosDesde(new DateTime(1985, 7, 3)), edad.Value);
        }

        [TestMethod]
        public void EdadDesdeTexto_CumpleAunNoCumplido_RestaUnAnio()
        {
            DateTime manana = DateTime.Today.AddDays(1);
            string texto = manana.AddYears(-30).ToString("dd/MM/yyyy");

            Assert.AreEqual(29, NegPerfilCampos.EdadDesdeTexto(texto).Value,
                            "si el cumpleanios es manana todavia tiene 29");
        }

        [TestMethod]
        public void EdadDesdeTexto_CumpleHoy_CuentaElAnio()
        {
            string texto = DateTime.Today.AddYears(-30).ToString("dd/MM/yyyy");

            Assert.AreEqual(30, NegPerfilCampos.EdadDesdeTexto(texto).Value);
        }

        [TestMethod]
        public void EdadDesdeTexto_VacioOBasura_DevuelveNull()
        {
            // 4 empleados tienen la fecha vacia. No es un error: es que no se
            // cargo. La pantalla muestra un guion, no un cero ni una excepcion.
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto(null));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto(""));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto("   "));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto("no es fecha"));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto("31/02/1990"));
        }

        [TestMethod]
        public void EdadDesdeTexto_ConEspacios_LosIgnora()
        {
            Assert.IsNotNull(NegPerfilCampos.EdadDesdeTexto("  14/03/1996  "));
        }

        private static int AniosDesde(DateTime nacimiento)
        {
            DateTime hoy = DateTime.Today;
            int anios = hoy.Year - nacimiento.Year;
            if (nacimiento.Date > hoy.AddYears(-anios)) { anios--; }
            return anios;
        }
    }
}
