using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// La regla que impide que alguien se cambie el perfil a si mismo.
    ///
    /// Es la unica parte de este cambio que se puede probar sin base ni sesion,
    /// y es tambien la que mas caro sale si falla: sin ella el ultimo Super
    /// Admin puede quitarse el perfil y dejar el sistema sin nadie que
    /// administre, sin forma de deshacerlo desde la aplicacion.
    /// </summary>
    [TestClass]
    public class NegUsuarioAdminTests
    {
        [TestMethod]
        public void EsSuPropioUsuario_ConElMismoId_DevuelveTrue()
        {
            Assert.IsTrue(NegUsuarioAdmin.EsSuPropioUsuario(12m, 12));
        }

        [TestMethod]
        public void EsSuPropioUsuario_ConOtroId_DevuelveFalse()
        {
            Assert.IsFalse(NegUsuarioAdmin.EsSuPropioUsuario(12m, 13));
        }

        /// <summary>
        /// Session["Id_Usuario"] es un object y, segun por donde se haya
        /// guardado, llega como cadena o como numero. Si la comparacion fuera
        /// de cadenas, "12" contra 12 daria distinto y la regla no protegeria
        /// nada justo en el caso normal.
        /// </summary>
        [TestMethod]
        public void EsSuPropioUsuario_ConElIdDeSesionComoCadena_DevuelveTrue()
        {
            Assert.IsTrue(NegUsuarioAdmin.EsSuPropioUsuario(12m, "12"));
        }

        [TestMethod]
        public void EsSuPropioUsuario_ConDecimalesEquivalentes_DevuelveTrue()
        {
            Assert.IsTrue(NegUsuarioAdmin.EsSuPropioUsuario(12m, "12.0"));
        }

        /// <summary>
        /// Sesion sin Id_Usuario: no se puede afirmar que sea la misma persona,
        /// asi que devuelve false y deja que decida la comprobacion del
        /// procedimiento, que compara por Cod_Usuario. Devolver true aqui
        /// bloquearia todos los cambios; es la comprobacion de mas abajo la que
        /// tiene que seguir siendo la ultima palabra.
        /// </summary>
        [TestMethod]
        public void EsSuPropioUsuario_ConSesionNula_DevuelveFalse()
        {
            Assert.IsFalse(NegUsuarioAdmin.EsSuPropioUsuario(12m, null));
        }

        [TestMethod]
        public void EsSuPropioUsuario_ConBasuraEnLaSesion_DevuelveFalse()
        {
            Assert.IsFalse(NegUsuarioAdmin.EsSuPropioUsuario(12m, "no es un numero"));
        }

        [TestMethod]
        public void EsSuPropioUsuario_ConCadenaVacia_DevuelveFalse()
        {
            Assert.IsFalse(NegUsuarioAdmin.EsSuPropioUsuario(12m, ""));
        }

        [TestMethod]
        public void EsSuPropioUsuario_ConCero_NoConfundeConSesionAusente()
        {
            /* Un Id_Usuario 0 no existe en la base, pero si llegara, comparar
               0 con 0 tiene que dar true y no colarse por el mismo camino que
               una sesion vacia. */
            Assert.IsTrue(NegUsuarioAdmin.EsSuPropioUsuario(0m, 0));
            Assert.IsFalse(NegUsuarioAdmin.EsSuPropioUsuario(0m, null));
        }
    }
}
