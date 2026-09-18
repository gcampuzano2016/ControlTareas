using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// El clasificador del resultado de los correos de una solicitud.
    ///
    /// Existe porque hasta ahora un correo que no salia no dejaba ningun rastro:
    /// en ObtenerListaTareas.ashx.cs el resultado del envio se asignaba a una
    /// variable que nadie leia. Distinguir SIN_CORREO de FALLIDO es lo que
    /// permite separar "la persona no tiene direccion cargada" de "lo intentamos
    /// y el servidor lo rechazo", que se arreglan de formas distintas.
    ///
    /// Las direcciones mal formadas de estas pruebas son las tres reales que
    /// aparecieron al auditar R_Usuarios, con el apellido cambiado.
    /// </summary>
    [TestClass]
    public class NegCorreoSolicitudTests
    {
        private const long UnaSolicitud = 22942;
        private const string UnTipoAviso = "SOLICITUD_JEFE";
        private const string UnAsunto = "Solicitud de Permiso";
        private const string UnaDireccion = "mmejia@dos.com.ec";

        private static EntLogCorreoSolicitud Clasificar(string destinatarios,
                                                        bool envioExitoso,
                                                        string errorProceso)
        {
            return NegCorreoSolicitud.Clasificar(UnaSolicitud, UnTipoAviso, destinatarios,
                                                 UnAsunto, envioExitoso, errorProceso);
        }

        [TestMethod]
        public void Clasificar_ConEnvioExitoso_DevuelveEnviado()
        {
            EntLogCorreoSolicitud log = Clasificar(UnaDireccion, true, "");

            Assert.AreEqual("ENVIADO", log.Resultado);
        }

        [TestMethod]
        public void Clasificar_ConDestinatarioNulo_DevuelveSinCorreo()
        {
            EntLogCorreoSolicitud log = Clasificar(null, false, "");

            Assert.AreEqual("SIN_CORREO", log.Resultado);
        }

        [TestMethod]
        public void Clasificar_ConDestinatarioVacio_DevuelveSinCorreo()
        {
            EntLogCorreoSolicitud log = Clasificar("", false, "");

            Assert.AreEqual("SIN_CORREO", log.Resultado);
        }

        [TestMethod]
        public void Clasificar_ConDestinatarioDeSoloSeparadores_DevuelveSinCorreo()
        {
            // Es lo que deja Sp_RTACorreoJefeInmediato cuando MailCodJefeInm
            // esta en blanco y el llamador concatena varias direcciones.
            EntLogCorreoSolicitud log = Clasificar("  ;  ; ", false, "");

            Assert.AreEqual("SIN_CORREO", log.Resultado);
        }

        [TestMethod]
        public void Clasificar_ConFalloYDestinatarioValido_DevuelveFallido()
        {
            EntLogCorreoSolicitud log = Clasificar(UnaDireccion, false, "Tiempo de espera agotado.");

            Assert.AreEqual("FALLIDO", log.Resultado);
        }

        [TestMethod]
        public void Clasificar_ConDireccionMalFormada_DevuelveFallidoYNoSinCorreo()
        {
            // Habia una direccion asi en R_Usuarios: coma en vez de punto.
            // Hay texto, asi que se intento enviar: es FALLIDO, no SIN_CORREO.
            EntLogCorreoSolicitud log = Clasificar("aperez@dos.com,ec", false,
                                                   "The specified string is not in the form required for an e-mail address.");

            Assert.AreEqual("FALLIDO", log.Resultado);
        }

        [TestMethod]
        public void Clasificar_ConFallo_GuardaElMensajeDelHelper()
        {
            EntLogCorreoSolicitud log = Clasificar(UnaDireccion, false, "Mailbox unavailable.");

            Assert.AreEqual("Mailbox unavailable.", log.Mensaje);
        }

        [TestMethod]
        public void Clasificar_ConFalloSinMensaje_GuardaUnMensajeQueNoEsVacio()
        {
            // Sin esto la bitacora diria FALLIDO y nada mas, que es justo lo que
            // no sirve cuando hay que reconstruir un caso meses despues.
            EntLogCorreoSolicitud log = Clasificar(UnaDireccion, false, null);

            Assert.IsFalse(string.IsNullOrEmpty(log.Mensaje));
        }

        [TestMethod]
        public void Clasificar_ConEnvioExitoso_NoGuardaMensaje()
        {
            EntLogCorreoSolicitud log = Clasificar(UnaDireccion, true, "residuo de un intento anterior");

            Assert.AreEqual("", log.Mensaje);
        }

        [TestMethod]
        public void Clasificar_LimpiaLosSeparadoresSobrantesDelDestinatario()
        {
            EntLogCorreoSolicitud log = Clasificar(" uno@dos.com.ec ; ; dos@dos.com.ec ", true, "");

            Assert.AreEqual("uno@dos.com.ec;dos@dos.com.ec", log.Destinatario);
        }

        [TestMethod]
        public void Clasificar_ConDestinatarioNulo_GuardaCadenaVaciaYNoNulo()
        {
            // La columna admite NULL, pero un NULL obliga a escribir ISNULL en
            // cada consulta de diagnostico. Se guarda vacio.
            EntLogCorreoSolicitud log = Clasificar(null, false, "");

            Assert.AreEqual("", log.Destinatario);
        }

        [TestMethod]
        public void Clasificar_ConDestinatariosMasLargosQueLaColumna_LosTrunca()
        {
            string[] direcciones = new string[40];
            for (int i = 0; i < direcciones.Length; i++)
            {
                direcciones[i] = "unadireccionlarga@dos.com.ec";
            }

            EntLogCorreoSolicitud log = Clasificar(string.Join(";", direcciones), true, "");

            Assert.AreEqual(400, log.Destinatario.Length);
        }

        [TestMethod]
        public void Clasificar_ConMensajeMasLargoQueLaColumna_LoTrunca()
        {
            EntLogCorreoSolicitud log = Clasificar(UnaDireccion, false, new string('x', 900));

            Assert.AreEqual(500, log.Mensaje.Length);
        }

        [TestMethod]
        public void Clasificar_CopiaLaSolicitudElTipoDeAvisoYElAsunto()
        {
            EntLogCorreoSolicitud log = Clasificar(UnaDireccion, true, "");

            Assert.AreEqual(UnaSolicitud, log.IdVacaciones);
            Assert.AreEqual(UnTipoAviso, log.TipoAviso);
            Assert.AreEqual(UnAsunto, log.Asunto);
        }
    }
}
