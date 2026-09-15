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

        /// <summary>Maximo de digitos segun la norma E.164 para numeros telefonicos internacionales.</summary>
        private const int DigitosMaximosTelefono = 15;

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

            /* Se cuentan digitos ASCII, no caracteres: "099 123-4567" es un
               telefono perfectamente valido. Solo se aceptan separadores que la
               gente usa de verdad: espacio, guion, parentesis, signo +, punto. */
            int digitos = 0;
            foreach (char c in contacto.Telefono)
            {
                /* char.IsDigit aceptaria digitos Unicode (arabigo-indicos,
                   devanagari). Aqui se exige ASCII explicito. */
                if (c >= '0' && c <= '9')
                {
                    digitos++;
                }
                /* Los separadores permitidos. */
                else if (c == ' ' || c == '-' || c == '(' || c == ')' || c == '+' || c == '.')
                {
                    /* Nada: son caracteres permitidos. */
                }
                else
                {
                    /* Cualquier otro caracter rechaza el telefono. */
                    return "El teléfono solo puede tener números, espacios, guiones, paréntesis, signo más y puntos.";
                }
            }

            if (digitos < DigitosMinimosTelefono)
            {
                return "El teléfono debe tener al menos 7 dígitos.";
            }

            if (digitos > DigitosMaximosTelefono)
            {
                return "El teléfono no puede tener más de 15 dígitos.";
            }

            return "";
        }

        /* Rango plausible para un anio academico o laboral. El piso es el mismo
           1940 que usa el reporte de excepciones del script para marcar fechas de
           nacimiento absurdas. El techo es el anio proximo, no el actual: alguien
           que se gradua en diciembre registra su titulo en enero. */
        private const int AnioMinimoPlausible = 1940;

        /// <summary>Cadena vacia si el estudio sirve; si no, el mensaje para el usuario.</summary>
        public static string ValidarEstudio(EntPerfilEstudio estudio)
        {
            if (estudio == null) { return "No se recibió el estudio."; }

            if (string.IsNullOrWhiteSpace(estudio.Nivel))
            {
                return "Seleccione el nivel de estudio.";
            }

            if (string.IsNullOrWhiteSpace(estudio.Institucion))
            {
                return "Escriba la institución donde estudió.";
            }

            if (string.IsNullOrWhiteSpace(estudio.Titulo))
            {
                return "Escriba el título obtenido.";
            }

            /* El anio es opcional: "no lo recuerdo" es una respuesta legitima y no
               debe impedir que registre el estudio. Solo se valida si vino. */
            if (estudio.AnioGraduacion.HasValue)
            {
                if (estudio.AnioGraduacion.Value > DateTime.Today.Year + 1)
                {
                    return "El año de graduación no puede ser posterior al próximo año.";
                }

                if (estudio.AnioGraduacion.Value < AnioMinimoPlausible)
                {
                    return "El año de graduación no parece correcto.";
                }
            }

            return "";
        }

        /// <summary>Cadena vacia si la certificacion sirve; si no, el mensaje.</summary>
        public static string ValidarCertificacion(EntPerfilCertificacion cert)
        {
            if (cert == null) { return "No se recibió la certificación."; }

            if (string.IsNullOrWhiteSpace(cert.Nombre))
            {
                return "Escriba el nombre de la certificación.";
            }

            if (string.IsNullOrWhiteSpace(cert.Entidad))
            {
                return "Escriba la entidad que la emitió.";
            }

            /* La fecha es opcional. Si vino, tiene que ser "yyyy-MM" -lo que produce
               un input type="month"- y con formato explicito, por la misma razon que
               la fecha de nacimiento: sin el, la cultura del servidor decide y en
               produccion no es la misma que aqui. */
            if (!string.IsNullOrWhiteSpace(cert.FechaObtencion))
            {
                DateTime obtenida;
                bool valida = DateTime.TryParseExact(cert.FechaObtencion.Trim(),
                                                     "yyyy-MM",
                                                     CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None,
                                                     out obtenida);
                if (!valida)
                {
                    return "La fecha de obtención no es válida.";
                }

                /* Se compara contra el primer dia del mes siguiente: una certificacion
                   obtenida "este mes" es valida aunque el dia 1 ya haya pasado. */
                DateTime inicioMesSiguiente = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1);
                if (obtenida >= inicioMesSiguiente)
                {
                    return "La fecha de obtención no puede estar en el futuro.";
                }
            }

            return "";
        }

        /// <summary>Cadena vacia si la experiencia sirve; si no, el mensaje.</summary>
        public static string ValidarExperiencia(EntPerfilExperiencia exp)
        {
            if (exp == null) { return "No se recibió la experiencia laboral."; }

            if (string.IsNullOrWhiteSpace(exp.Empresa))
            {
                return "Escriba el nombre de la empresa.";
            }

            if (string.IsNullOrWhiteSpace(exp.Cargo))
            {
                return "Escriba el cargo que ocupó.";
            }

            /* Aqui el anio de inicio SI es obligatorio, al reves que en los estudios:
               el CV ordena la experiencia por fecha, y una fila sin anio no tiene
               donde colocarse. */
            if (!exp.AnioDesde.HasValue)
            {
                return "Indique el año en que empezó.";
            }

            if (exp.AnioDesde.Value > DateTime.Today.Year)
            {
                return "El año en que empezó no puede estar en el futuro.";
            }

            if (exp.AnioDesde.Value < AnioMinimoPlausible)
            {
                return "El año en que empezó no parece correcto.";
            }

            /* AnioHasta nulo significa "sigo ahi", no "no se sabe". Por eso no se
               exige, pero si vino tiene que ser coherente. */
            if (exp.AnioHasta.HasValue)
            {
                if (exp.AnioHasta.Value < exp.AnioDesde.Value)
                {
                    return "El año en que terminó no puede ser anterior al año en que empezó.";
                }

                if (exp.AnioHasta.Value > DateTime.Today.Year + 1)
                {
                    return "El año en que terminó no puede estar en el futuro.";
                }
            }

            return "";
        }

        /// <summary>Cadena vacia si la carga familiar sirve; si no, el mensaje.</summary>
        public static string ValidarCargaFamiliar(EntPerfilCargaFamiliar carga)
        {
            if (carga == null) { return "No se recibió la carga familiar."; }

            if (string.IsNullOrWhiteSpace(carga.Nombre))
            {
                return "Escriba el nombre completo de la carga familiar.";
            }

            if (string.IsNullOrWhiteSpace(carga.Parentesco))
            {
                return "Seleccione el parentesco.";
            }

            /* Aqui la fecha SI es obligatoria, al reves que en las certificaciones:
               Talento Humano usa la edad para saber si la carga sigue siendolo, y
               sin fecha ese calculo no existe. */
            if (string.IsNullOrWhiteSpace(carga.FechaNacimiento))
            {
                return "Indique la fecha de nacimiento.";
            }

            DateTime nacimiento;
            bool fechaValida = DateTime.TryParseExact(carga.FechaNacimiento.Trim(),
                                                      "yyyy-MM-dd",
                                                      CultureInfo.InvariantCulture,
                                                      DateTimeStyles.None,
                                                      out nacimiento);
            if (!fechaValida)
            {
                return "La fecha de nacimiento no es válida.";
            }

            if (nacimiento.Date > DateTime.Today)
            {
                return "La fecha de nacimiento no puede estar en el futuro.";
            }

            return "";
        }
    }
}
