using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CapaPruebas
{
    /// <summary>
    /// Las reglas de completar datos y registrar ajustes de sueldo desde la
    /// pantalla de Horas Extras. Solo lo que no toca la base: leer el monto que
    /// escribe una persona, validar, y el mensaje despues de guardar.
    /// </summary>
    [TestClass]
    public class NegHeMaestroPantallaTests
    {
        private static decimal Monto(string texto)
        {
            decimal m;
            Assert.IsTrue(NegHeMaestroPantalla.LeerMonto(texto, out m), "Deberia aceptar «" + texto + "».");
            return m;
        }

        private static void Rechaza(string texto)
        {
            decimal m;
            Assert.IsFalse(NegHeMaestroPantalla.LeerMonto(texto, out m), "Deberia rechazar «" + texto + "».");
        }

        [TestMethod]
        public void LeerMonto_EnteroYDecimalesConPuntoOComa()
        {
            Assert.AreEqual(1850m, Monto("1850"));
            Assert.AreEqual(1850.5m, Monto("1850.5"));
            Assert.AreEqual(1850.5m, Monto("1850,5"));
            Assert.AreEqual(1850.55m, Monto(" 1850,55 "));
            Assert.AreEqual(1850m, Monto("$1850"));
        }

        [TestMethod]
        public void LeerMonto_ConSeparadorDeMiles()
        {
            Assert.AreEqual(1850.50m, Monto("1.850,50"));
            Assert.AreEqual(1850.50m, Monto("1,850.50"));
            Assert.AreEqual(1250000m, Monto("1.250.000"));
        }

        [TestMethod]
        public void LeerMonto_NuncaMultiplicaPorCien()
        {
            // El error que no puede pasar: leer "1.850,50" como 185050.
            Assert.AreNotEqual(185050m, Monto("1.850,50"));
        }

        [TestMethod]
        public void LeerMonto_AmbiguoSeRechaza()
        {
            // "1.850" puede ser mil ochocientos cincuenta o uno coma ochenta y cinco.
            Rechaza("1.850");
            Rechaza("1,850");
        }

        [TestMethod]
        public void LeerMonto_VacioCeroNegativoOTextoSeRechaza()
        {
            Rechaza("");
            Rechaza(null);
            Rechaza("0");
            Rechaza("-100");
            Rechaza("abc");
            Rechaza("1850,555");
        }

        [TestMethod]
        public void ValidarColaborador_DatosCompletos_SinError()
        {
            Assert.IsNull(NegHeMaestroPantalla.ValidarColaborador("DOS S.A.", 8, 1850m, new DateTime(2026, 9, 1)));
        }

        [TestMethod]
        public void ValidarColaborador_CadaFalta_DiceCual()
        {
            StringAssert.Contains(NegHeMaestroPantalla.ValidarColaborador("", 8, 1850m, new DateTime(2026, 9, 1)), "empresa");
            StringAssert.Contains(NegHeMaestroPantalla.ValidarColaborador("DOS S.A.", 0, 1850m, new DateTime(2026, 9, 1)), "jornada");
            StringAssert.Contains(NegHeMaestroPantalla.ValidarColaborador("DOS S.A.", 13, 1850m, new DateTime(2026, 9, 1)), "jornada");
            StringAssert.Contains(NegHeMaestroPantalla.ValidarColaborador("DOS S.A.", 8, 0m, new DateTime(2026, 9, 1)), "sueldo");
            StringAssert.Contains(NegHeMaestroPantalla.ValidarColaborador("DOS S.A.", 8, 1850m, DateTime.MinValue), "fecha");
        }

        [TestMethod]
        public void ValidarAjuste_MontoYFecha()
        {
            Assert.IsNull(NegHeMaestroPantalla.ValidarAjuste(1650m, new DateTime(2026, 9, 1)));
            StringAssert.Contains(NegHeMaestroPantalla.ValidarAjuste(0m, new DateTime(2026, 9, 1)), "sueldo");
            StringAssert.Contains(NegHeMaestroPantalla.ValidarAjuste(1650m, DateTime.MinValue), "fecha");
        }

        [TestMethod]
        public void MensajeTrasGuardar_PeriodoAbierto_DiceQueSeRecalculo()
        {
            string m = NegHeMaestroPantalla.MensajeTrasGuardar(true, true, new DateTime(2026, 9, 1), new DateTime(2026, 9, 15));
            StringAssert.Contains(m, "recalcul");
        }

        [TestMethod]
        public void MensajeTrasGuardar_PeriodoCerrado()
        {
            string m = NegHeMaestroPantalla.MensajeTrasGuardar(true, false, new DateTime(2026, 9, 1), new DateTime(2026, 9, 15));
            StringAssert.Contains(m, "cerrado");
            Assert.IsFalse(m.Contains("recalculó"), "Un periodo cerrado no se recalcula.");
        }

        [TestMethod]
        public void MensajeTrasGuardar_VigenciaPosterior()
        {
            string m = NegHeMaestroPantalla.MensajeTrasGuardar(true, true, new DateTime(2026, 10, 1), new DateTime(2026, 9, 15));
            StringAssert.Contains(m, "no aplica");
        }

        [TestMethod]
        public void Mensajes_DeLosCodigosDelProcedimiento()
        {
            StringAssert.Contains(NegHeMaestroPantalla.MensajeColaborador(-1), "activo");
            StringAssert.Contains(NegHeMaestroPantalla.MensajeColaborador(-2), "ajuste");
            StringAssert.Contains(NegHeMaestroPantalla.MensajeAjuste(-1), "módulo");
            StringAssert.Contains(NegHeMaestroPantalla.MensajeAjuste(-2), "misma fecha");
        }
    }
}
