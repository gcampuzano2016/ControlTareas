using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Separa la cadena de destinatarios que usa toda la aplicacion: direcciones
    /// unidas con punto y coma.
    ///
    /// Estaba escrito dos veces, en EnviarCorreo y en EnviarCorreoPermiso. En
    /// julio de 2026 se arreglo una sola copia, y la otra siguio reventando con
    /// la cadena nula y pasando espacios en blanco a mail.To.Add. Una sola copia,
    /// probada, es lo que evita que vuelvan a divergir.
    ///
    /// No valida el formato a proposito: de eso se encarga MailAddress al
    /// armar el correo, y su excepcion es la que termina en FALLIDO.
    /// </summary>
    public class NegDestinatariosCorreo
    {
        public static string[] Separar(string destinatarios)
        {
            List<string> direcciones = new List<string>();

            if (string.IsNullOrEmpty(destinatarios)) { return direcciones.ToArray(); }

            foreach (string parte in destinatarios.Split(';'))
            {
                string direccion = parte.Trim();
                if (direccion.Length > 0) { direcciones.Add(direccion); }
            }

            return direcciones.ToArray();
        }
    }
}
