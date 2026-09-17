using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// La regla de la que depende que nadie escriba en el perfil de otro.
    ///
    /// Vive en CapaNegocio y no en el proyecto web por esto mismo: CapaPruebas
    /// solo referencia CapaEntidad y CapaNegocio. Una regla escrita dentro del
    /// handler no tendria ni una sola prueba.
    ///
    /// Los codigos de estas pruebas son inventados. No hay datos reales aca.
    /// </summary>
    [TestClass]
    public class NegPerfilAccesoTests
    {
        private const string Yo    = "USR001";
        private const string Otro  = "USR002";
        private const string Rrhh  = "14";
        private const string Admin = "18";
        private const string Comun = "7";

        /* ------------------------------------------------ sin codigo pedido --- */

        /// <summary>
        /// El caso de MiPerfil.aspx, que es el 99% del trafico: esa pantalla no
        /// manda codUsuario nunca. Si esta prueba falla, la pantalla que ya esta
        /// en produccion deja de funcionar.
        /// </summary>
        [TestMethod]
        public void Objetivo_SinCodigoPedido_DevuelveElDeLaSesion()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Comun, "");

            Assert.IsTrue(r.Permitido, "Sin codigo pedido siempre se permite: es el propio perfil");
            Assert.AreEqual(Yo, r.CodUsuario);
        }

        [TestMethod]
        public void Objetivo_CodigoPedidoNulo_DevuelveElDeLaSesion()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Comun, null);

            Assert.IsTrue(r.Permitido);
            Assert.AreEqual(Yo, r.CodUsuario);
        }

        /* --------------------------------------------------------- permitido --- */

        [TestMethod]
        public void Objetivo_PerfilTalentoHumano_PuedePedirUnoAjeno()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Rrhh, Otro);

            Assert.IsTrue(r.Permitido);
            Assert.AreEqual(Otro, r.CodUsuario);
        }

        [TestMethod]
        public void Objetivo_PerfilSuperAdmin_PuedePedirUnoAjeno()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Admin, Otro);

            Assert.IsTrue(r.Permitido);
            Assert.AreEqual(Otro, r.CodUsuario);
        }

        /// <summary>
        /// Pedir el propio codigo no es pedir uno ajeno. Sin este caso, un cliente
        /// que mandara su propio codigo -por simetria, por copiar y pegar- se
        /// llevaria un rechazo incomprensible.
        /// </summary>
        [TestMethod]
        public void Objetivo_UsuarioComunPideElSuyo_SePermite()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Comun, Yo);

            Assert.IsTrue(r.Permitido);
            Assert.AreEqual(Yo, r.CodUsuario);
        }

        /// <summary>
        /// Todo el modulo compara recortando -Cod_Jefe_Inm trae relleno y
        /// Cod_Usuario tambien-. Si esta comparacion fuera cruda, un codigo propio
        /// con un espacio delante se leeria como ajeno y el usuario recibiria un
        /// rechazo por editar su propio perfil.
        /// </summary>
        [TestMethod]
        public void Objetivo_CodigoPedidoConRelleno_SeComparaRecortado()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Comun, "  " + Yo + "  ");

            Assert.IsTrue(r.Permitido);
            Assert.AreEqual(Yo, r.CodUsuario, "Ademas de permitirlo, devuelve el codigo ya recortado");
        }

        /* ---------------------------------------------------------- rechazos --- */

        /// <summary>
        /// El caso que motiva toda la clase: alguien sin perfil de Talento Humano
        /// llamando al handler por HTTP directo con el codigo de otra persona.
        /// </summary>
        [TestMethod]
        public void Objetivo_UsuarioComunPideUnoAjeno_SeRechaza()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, Comun, Otro);

            Assert.IsFalse(r.Permitido);
            Assert.AreEqual("", r.CodUsuario, "Un rechazo no puede devolver ningun codigo utilizable");
            Assert.AreNotEqual("", r.Mensaje, "El rechazo trae su propio mensaje: no lo inventa quien llama");
        }

        /// <summary>
        /// Cierra por defecto. Una sesion sin Id_Perfil, o con uno que no es un
        /// numero, no es una sesion de Talento Humano: es una sesion de la que no
        /// sabemos nada.
        /// </summary>
        [TestMethod]
        public void Objetivo_PerfilNoNumerico_SeRechazaElAjeno()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, "no-es-un-numero", Otro);

            Assert.IsFalse(r.Permitido);
        }

        [TestMethod]
        public void Objetivo_PerfilAusente_SeRechazaElAjeno()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo(Yo, null, Otro);

            Assert.IsFalse(r.Permitido);
        }

        /// <summary>
        /// Sin sesion no hay ni perfil propio que devolver. Es el caso que hoy
        /// cubre el "No se pudo identificar al usuario de la sesion" de cada
        /// accion del handler, y que a partir de ahora vive aca.
        /// </summary>
        [TestMethod]
        public void Objetivo_SesionSinCodigo_SeRechaza()
        {
            EntPerfilObjetivo r = NegPerfilAcceso.Objetivo("", Comun, "");

            Assert.IsFalse(r.Permitido);
            Assert.AreNotEqual("", r.Mensaje);
        }

        /* ------------------------------------------------------------ EsRRHH --- */

        [TestMethod]
        public void EsRRHH_CatorceYDieciocho_SonLosDosUnicos()
        {
            Assert.IsTrue(NegPerfilAcceso.EsRRHH("14"));
            Assert.IsTrue(NegPerfilAcceso.EsRRHH("18"));
            Assert.IsFalse(NegPerfilAcceso.EsRRHH("7"));
            Assert.IsFalse(NegPerfilAcceso.EsRRHH("1"));
            Assert.IsFalse(NegPerfilAcceso.EsRRHH(""));
            Assert.IsFalse(NegPerfilAcceso.EsRRHH(null));
        }
    }
}
