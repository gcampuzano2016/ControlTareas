using System;
using System.Collections.Generic;
using System.Globalization;
using CapaEntidad;

namespace CapaNegocio
{
    /// <summary>
    /// Traduce lo que llega de afuera -texto guardado en la base, payload del
    /// navegador- a algo en lo que se pueda confiar.
    ///
    /// Esta clase no toca la base de datos ni HttpContext a proposito: es lo
    /// unico del modulo que se puede probar sin levantar nada, y es justamente
    /// donde estan los errores que no dan la cara.
    /// </summary>
    public static class NegPerfilCampos
    {
        /* Empleados.Fecha_nacimiento es nvarchar(50) y guarda dd/MM/yyyy. El
           formato va explicito y con InvariantCulture: si se deja que lo adivine
           la cultura del servidor, las fechas con dia mayor que 12 fallan y las
           demas se interpretan al reves sin avisar. */
        private const string FormatoFecha = "dd/MM/yyyy";

        /// <summary>
        /// Edad en anios cumplidos, o null si el texto esta vacio o no es una
        /// fecha valida. Null significa "no se sabe", y la pantalla muestra un
        /// guion; nunca un cero, que se leeria como un dato real.
        /// </summary>
        public static int? EdadDesdeTexto(string fechaTexto)
        {
            if (string.IsNullOrWhiteSpace(fechaTexto)) { return null; }

            DateTime nacimiento;
            bool valida = DateTime.TryParseExact(fechaTexto.Trim(),
                                                 FormatoFecha,
                                                 CultureInfo.InvariantCulture,
                                                 DateTimeStyles.None,
                                                 out nacimiento);
            if (!valida) { return null; }

            DateTime hoy = DateTime.Today;

            // Una fecha de nacimiento futura no es una edad negativa: es un
            // dato mal tipeado. No se acota el extremo inferior a proposito:
            // una persona de 90 anios es perfectamente plausible.
            if (nacimiento.Date > hoy) { return null; }

            int anios = hoy.Year - nacimiento.Year;

            // Todavia no cumple anios este anio.
            if (nacimiento.Date > hoy.AddYears(-anios)) { anios--; }

            return anios;
        }

        /* La lista blanca no es un arreglo que se recorra: es la forma de este
           codigo. LeerContacto pide exactamente cuatro claves, y
           EntPerfilContacto tiene exactamente cuatro propiedades. Un cargo o
           una cedula en el payload no se cuelan porque no hay linea que los
           pida ni propiedad que los reciba.

           Agregar un campo editable son dos cambios deliberados -la propiedad
           y la llamada- y la prueba que fija el conteo de propiedades obliga a
           justificarlo. */

        /// <summary>
        /// Arma el contacto editable a partir del payload, leyendo solo las
        /// cuatro claves que el codigo esta escrito para leer.
        /// </summary>
        public static EntPerfilContacto LeerContacto(IDictionary<string, object> campos)
        {
            return new EntPerfilContacto
            {
                CorreoPersonal   = Texto(campos, "correoPersonal"),
                TelefonoPersonal = Texto(campos, "telefonoPersonal"),
                Direccion        = Texto(campos, "direccion"),
                EstadoCivil      = Texto(campos, "estadoCivil")
            };
        }

        /// <summary>
        /// El valor de una clave, recortado. Cadena vacia si la clave no vino
        /// o si vino nula.
        /// </summary>
        private static string Texto(IDictionary<string, object> campos, string clave)
        {
            if (campos == null) { return ""; }

            object valor;
            if (!campos.TryGetValue(clave, out valor) || valor == null) { return ""; }

            return valor.ToString().Trim();
        }

        /// <summary>Minimo de digitos de un telefono utilizable en Ecuador.</summary>
        private const int DigitosMinimosTelefono = 7;

        /// <summary>
        /// Cadena vacia si el contacto sirve; si no, el mensaje para el usuario.
        ///
        /// Se valida aca y no solo en el navegador porque el handler es
        /// alcanzable por HTTP directo: una validacion que solo vive en el
        /// cliente no es una validacion.
        /// </summary>
        public static string ValidarEmergencia(EntPerfilEmergencia contacto)
        {
            if (contacto == null)
            {
                return "No se recibió el contacto de emergencia.";
            }

            if (string.IsNullOrWhiteSpace(contacto.Nombre))
            {
                return "Escriba el nombre del contacto de emergencia.";
            }

            if (string.IsNullOrWhiteSpace(contacto.Parentesco))
            {
                return "Indique el parentesco del contacto de emergencia.";
            }

            if (string.IsNullOrWhiteSpace(contacto.Telefono))
            {
                return "Escriba el teléfono del contacto de emergencia.";
            }

            /* Se cuentan digitos, no caracteres: "099 123-4567" es un telefono
               perfectamente valido y la gente lo escribe asi. */
            int digitos = 0;
            foreach (char c in contacto.Telefono)
            {
                if (char.IsDigit(c)) { digitos++; }
            }

            if (digitos < DigitosMinimosTelefono)
            {
                return "El teléfono debe tener al menos 7 dígitos.";
            }

            return "";
        }
    }
}
