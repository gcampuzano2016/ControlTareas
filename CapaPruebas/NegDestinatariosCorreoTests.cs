using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// La lista de direcciones a las que se le va a escribir, separada de la
    /// mecanica de enviar.
    ///
    /// Existe porque este parseo estaba escrito dos veces, en EnviarCorreo y en
    /// EnviarCorreoPermiso, y en julio de 2026 se arreglo una sola de las dos
    /// copias. La que quedo sin arreglar reventaba con la direccion nula y
    /// pasaba cadenas en blanco a mail.To.Add. Con una sola copia probada, las
    /// dos se comportan igual.
    /// </summary>
    [TestClass]
    public class NegDestinatariosCorreoTests
    {
        [TestMethod]
        public void Separar_ConNulo_NoDevuelveNingunaDireccion()
        {
            // El caso que tiraba NullReferenceException.
            Assert.AreEqual(0, NegDestinatariosCorreo.Separar(null).Length);
        }

        [TestMethod]
        public void Separar_ConCadenaVacia_NoDevuelveNingunaDireccion()
        {
            Assert.AreEqual(0, NegDestinatariosCorreo.Separar("").Length);
        }

        [TestMethod]
        public void Separar_ConSoloSeparadoresYEspacios_NoDevuelveNingunaDireccion()
        {
            // Sin Trim() esto entregaba "  " a mail.To.Add y lanzaba FormatException.
            Assert.AreEqual(0, NegDestinatariosCorreo.Separar("  ;  ; ").Length);
        }

        [TestMethod]
        public void Separar_ConUnaDireccion_DevuelveEsaDireccion()
        {
            string[] direcciones = NegDestinatariosCorreo.Separar("mmejia@dos.com.ec");

            CollectionAssert.AreEqual(new[] { "mmejia@dos.com.ec" }, direcciones);
        }

        [TestMethod]
        public void Separar_ConEspaciosAlrededor_LosSaca()
        {
            string[] direcciones = NegDestinatariosCorreo.Separar("  mmejia@dos.com.ec  ");

            CollectionAssert.AreEqual(new[] { "mmejia@dos.com.ec" }, direcciones);
        }

        [TestMethod]
        public void Separar_ConVariasDirecciones_LasDevuelveEnOrdenYSinEspacios()
        {
            string[] direcciones = NegDestinatariosCorreo.Separar(" uno@dos.com.ec ; dos@dos.com.ec ");

            CollectionAssert.AreEqual(new[] { "uno@dos.com.ec", "dos@dos.com.ec" }, direcciones);
        }

        [TestMethod]
        public void Separar_ConSeparadoresDeMas_IgnoraLosHuecos()
        {
            string[] direcciones = NegDestinatariosCorreo.Separar("uno@dos.com.ec;;dos@dos.com.ec;");

            CollectionAssert.AreEqual(new[] { "uno@dos.com.ec", "dos@dos.com.ec" }, direcciones);
        }

        [TestMethod]
        public void Separar_ConDireccionMalFormada_LaDevuelveIgual()
        {
            // No es tarea de esta pieza validar el formato: de eso se encarga
            // MailAddress, y su excepcion es la que se convierte en FALLIDO.
            string[] direcciones = NegDestinatariosCorreo.Separar("aperez@dos.com,ec");

            CollectionAssert.AreEqual(new[] { "aperez@dos.com,ec" }, direcciones);
        }
    }
}
