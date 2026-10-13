using CapaEntidad;

namespace CapaNegocio
{
    /// <summary>
    /// Decide como queda registrado en la bitacora el intento de avisar por
    /// correo sobre una solicitud.
    ///
    /// Esta separado del envio y no toca base ni SMTP a proposito: es la unica
    /// parte con decisiones, y asi se puede probar sola. El resto (armar el
    /// cuerpo, hablar con Office 365, insertar el renglon) no decide nada.
    ///
    /// La distincion que importa es SIN_CORREO contra FALLIDO. La primera la
    /// arregla Talento Humano cargando la direccion; la segunda es del servidor
    /// de correo. Confundirlas fue lo que hizo que el caso de septiembre de 2026
    /// se investigara a mano durante horas.
    /// </summary>
    public class NegCorreoSolicitud
    {
        public const string Enviado = "ENVIADO";
        public const string Fallido = "FALLIDO";
        public const string SinCorreo = "SIN_CORREO";

        /* Los limites de las columnas de RegistroCorreoSolicitud. Se recorta aca
           ademas de en el SP: un VARCHAR(400) que recibe 1.200 caracteres se
           trunca sin avisar, y preferimos que el recorte sea una decision.

           El asunto no se recorta aca porque no es dato de nadie: sale de
           literales del codigo ("Solicitud de Permiso"). De ese se encarga el
           LEFT() del procedimiento. */
        private const int LargoDestinatario = 400;
        private const int LargoMensaje = 500;

        private const string SinDireccionCargada =
            "No hay una direccion de correo registrada para el destinatario.";
        private const string SinDetalleDelError =
            "El envio fallo y el helper de correo no informo el motivo.";

        public static EntLogCorreoSolicitud Clasificar(long idVacaciones, string tipoAviso,
                                                       string destinatarios, string asunto,
                                                       bool envioExitoso, string errorProceso)
        {
            string destinatario = Normalizar(destinatarios);

            string resultado;
            if (envioExitoso) { resultado = Enviado; }
            else if (destinatario.Length == 0) { resultado = SinCorreo; }
            else { resultado = Fallido; }

            return new EntLogCorreoSolicitud
            {
                IdVacaciones = idVacaciones,
                TipoAviso = tipoAviso,
                Destinatario = Recortar(destinatario, LargoDestinatario),
                Asunto = asunto,
                Resultado = resultado,
                Mensaje = Recortar(Explicacion(resultado, errorProceso), LargoMensaje)
            };
        }

        private static string Explicacion(string resultado, string errorProceso)
        {
            if (resultado == Enviado) { return ""; }
            if (!string.IsNullOrEmpty(errorProceso)) { return errorProceso; }

            return resultado == SinCorreo ? SinDireccionCargada : SinDetalleDelError;
        }

        /// <summary>
        /// Deja las direcciones separadas por punto y coma, sin espacios ni
        /// huecos. Un destinatario que queda vacio despues de esto es lo que
        /// convierte el renglon en SIN_CORREO.
        /// </summary>
        private static string Normalizar(string destinatarios)
        {
            return string.Join(";", NegDestinatariosCorreo.Separar(destinatarios));
        }

        private static string Recortar(string valor, int largo)
        {
            if (valor == null) { return ""; }

            return valor.Length <= largo ? valor : valor.Substring(0, largo);
        }
    }
}
