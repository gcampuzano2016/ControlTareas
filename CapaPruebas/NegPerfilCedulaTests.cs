using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// El digito verificador de la cedula ecuatoriana. Una cedula mal escrita
    /// que pase por buena rompe el enlace entre R_Usuarios y Empleados, que es
    /// por donde el modulo de horas extras identifica a la gente.
    ///
    /// Las cedulas de estas pruebas son INVENTADAS y se construyeron con el
    /// algoritmo de referencia para que den exactamente el digito que la prueba
    /// afirma. El repositorio es publico: no se usa ninguna cedula real.
    /// </summary>
    [TestClass]
    public class NegPerfilCedulaTests
    {
        /* Prefijo 171003406 + su verificador, que es 5. Provincia 17
           (Pichincha), tercer digito 1: persona natural. */
        private const string CedulaValida = "1710034065";

        [TestMethod]
        public void EsValida_ConDigitoCorrecto_DevuelveTrue()
        {
            Assert.IsTrue(NegPerfilCedula.EsValida(CedulaValida));
        }

        [TestMethod]
        public void EsValida_ConElUltimoDigitoCambiado_DevuelveFalse()
        {
            // La misma de arriba con el verificador corrido en uno.
            string rota = CedulaValida.Substring(0, 9)
                        + ((CedulaValida[9] - '0' + 1) % 10).ToString();

            Assert.IsFalse(NegPerfilCedula.EsValida(rota));
        }

        [TestMethod]
        public void EsValida_ConNueveDigitos_DevuelveFalse()
        {
            Assert.IsFalse(NegPerfilCedula.EsValida("171003406"));
        }

        [TestMethod]
        public void EsValida_ConOnceDigitos_DevuelveFalse()
        {
            // Hay una asi en produccion hoy. Ver la seccion 5 del diseno.
            Assert.IsFalse(NegPerfilCedula.EsValida(CedulaValida + "1"));
        }

        [TestMethod]
        public void EsValida_ConLetras_DevuelveFalse()
        {
            Assert.IsFalse(NegPerfilCedula.EsValida("17100A4065"));
        }

        [TestMethod]
        public void EsValida_ConProvinciaCero_DevuelveFalse()
        {
            // Prefijo 001003406 + su verificador, que es 4. Provincia 00, que
            // no existe: el digito verificador esta bien y aun asi no sirve.
            Assert.IsFalse(NegPerfilCedula.EsValida("0010034064"));
        }

        [TestMethod]
        public void EsValida_ConProvinciaMayorQue24YDistintaDe30_DevuelveFalse()
        {
            // Prefijo 251003406 + su verificador, que es 5. Provincia 25.
            Assert.IsFalse(NegPerfilCedula.EsValida("2510034065"));
        }

        [TestMethod]
        public void EsValida_ConProvincia30_DevuelveTrue()
        {
            // 30 es el codigo de los ecuatorianos nacidos en el exterior.
            // Prefijo 301003406 + su verificador, que es 8.
            Assert.IsTrue(NegPerfilCedula.EsValida("3010034068"));
        }

        [TestMethod]
        public void EsValida_ConTercerDigitoSeisOMas_DevuelveFalse()
        {
            // Prefijo 176003406 + su verificador, que es 4. Tercer digito 6:
            // no es una persona natural. Hay dos casos asi en produccion hoy,
            // con el tercer digito en 9.
            Assert.IsFalse(NegPerfilCedula.EsValida("1760034064"));
        }

        [TestMethod]
        public void EsValida_ConEspaciosAlrededor_LosIgnora()
        {
            Assert.IsTrue(NegPerfilCedula.EsValida("  " + CedulaValida + "  "));
        }

        [TestMethod]
        public void EsValida_ConNulo_DevuelveFalse()
        {
            Assert.IsFalse(NegPerfilCedula.EsValida(null));
        }

        [TestMethod]
        public void EsValida_ConVacio_DevuelveFalse()
        {
            // Vacio no es valido: quien decide si una cedula vacia se acepta es
            // NegPerfilCampos.ValidarDatosPersonales, no esta funcion.
            Assert.IsFalse(NegPerfilCedula.EsValida("   "));
        }
    }
}
