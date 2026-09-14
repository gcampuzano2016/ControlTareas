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

        /* Las unicas cuatro claves que se leen del payload. Todo lo demas que
           venga se descarta sin avisar: no es un error del cliente, es que no
           le corresponde. Cargo, area, cedula, fecha de nacimiento y puesto los
           administra RRHH y no tienen entrada por aca. */
        private static readonly string[] CamposPermitidos =
        {
            "correoPersonal", "telefonoPersonal", "direccion", "estadoCivil"
        };

        /// <summary>
        /// Arma el contacto editable a partir del payload, leyendo solo las
        /// claves permitidas.
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
        /// El valor de una clave permitida, recortado. Cadena vacia si la clave
        /// no vino, si vino nula, o si no esta en la lista blanca.
        /// </summary>
        private static string Texto(IDictionary<string, object> campos, string clave)
        {
            if (campos == null) { return ""; }
            if (Array.IndexOf(CamposPermitidos, clave) < 0) { return ""; }

            object valor;
            if (!campos.TryGetValue(clave, out valor) || valor == null) { return ""; }

            return valor.ToString().Trim();
        }
    }
}
