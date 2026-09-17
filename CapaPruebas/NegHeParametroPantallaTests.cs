using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// Pruebas de la orquestacion de la pantalla de parametros que NO
    /// necesitan base de datos: la traduccion de codigos y las reglas sobre
    /// las siete claves conocidas. Lo que habla con el DAO -Listar, Guardar-
    /// no se prueba aqui, igual que en NegHorasExtrasPantallaTests.
    /// </summary>
    [TestClass]
    public class NegHeParametroPantallaTests
    {
        /// <summary>
        /// Hay SEIS codigos de rechazo, no cuatro: -1 a -4 mas -5
        /// -DecimalesMonto no puede pasar de 2, hay un CHECK en la tabla-, y
        /// 0 de exito. Sin traducir el -5, quien escriba 4 decimales recibe
        /// en la cara el error crudo de la restriccion de SQL Server.
        /// </summary>
        [TestMethod]
        public void MensajeDeGuardado_TraduceCadaCodigoAAlgoAccionable()
        {
            StringAssert.Contains(NegHeParametroPantalla.MensajeDeGuardado(-1), "no existe");
            StringAssert.Contains(NegHeParametroPantalla.MensajeDeGuardado(-2), "mayor que cero");
            StringAssert.Contains(NegHeParametroPantalla.MensajeDeGuardado(-3), "misma fecha");
            StringAssert.Contains(NegHeParametroPantalla.MensajeDeGuardado(-4), "posterior");
            StringAssert.Contains(NegHeParametroPantalla.MensajeDeGuardado(-5), "2 decimales");
            StringAssert.Contains(NegHeParametroPantalla.MensajeDeGuardado(0), "guardado");
        }

        [TestMethod]
        public void EsClaveConocida_RechazaLoQueNoEstaEnLaLista()
        {
            Assert.IsTrue(NegHeParametroPantalla.EsClaveConocida("Factor50"));
            Assert.IsFalse(NegHeParametroPantalla.EsClaveConocida("FactorInventado"));
        }

        [TestMethod]
        public void EsClaveConocida_NoDependeDeMayusculas()
        {
            Assert.IsTrue(NegHeParametroPantalla.EsClaveConocida("factor50"));
        }

        [TestMethod]
        public void EsParametroActivo_LosTopesNoLoEstan()
        {
            /* TopeDiario50 y TopeSemanal50 estan cargados y no los usa ningun
               calculo: RRHH no definio a que umbral mensual se traducen. La
               pantalla los muestra, pero marcados, para que nadie crea que
               hacen algo. */
            Assert.IsTrue(NegHeParametroPantalla.EsParametroActivo("Factor50"));
            Assert.IsFalse(NegHeParametroPantalla.EsParametroActivo("TopeDiario50"));
            Assert.IsFalse(NegHeParametroPantalla.EsParametroActivo("TopeSemanal50"));
        }

        [TestMethod]
        public void EsParametroActivo_LoQueNoEstaEnLaListaNoEstaActivo()
        {
            Assert.IsFalse(NegHeParametroPantalla.EsParametroActivo("FactorInventado"));
        }
    }
}
