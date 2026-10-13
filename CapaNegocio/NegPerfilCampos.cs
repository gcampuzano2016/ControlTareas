using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

            switch (ValidarFormatoTelefono(contacto.Telefono))
            {
                case ResultadoFormatoTelefono.CaracterInvalido:
                    return "El teléfono solo puede tener números, espacios, guiones, paréntesis, signo más y puntos.";
                case ResultadoFormatoTelefono.MuyPocosDigitos:
                    return "El teléfono debe tener al menos 7 dígitos.";
                case ResultadoFormatoTelefono.MuyMuchosDigitos:
                    return "El teléfono no puede tener más de 15 dígitos.";
                default:
                    return "";
            }
        }

        /// <summary>Lo que puede pasar al contar los digitos de un telefono en texto libre.</summary>
        private enum ResultadoFormatoTelefono
        {
            Valido,
            CaracterInvalido,
            MuyPocosDigitos,
            MuyMuchosDigitos
        }

        /// <summary>
        /// Cuenta los digitos ASCII de un telefono en texto libre y clasifica el
        /// resultado. Comparte esta logica ValidarEmergencia y ValidarContacto
        /// -antes vivia solo en ValidarEmergencia y quedaba fuera de alcance de
        /// ValidarContacto- para que las dos reglas de telefono no se
        /// desincronicen si alguna cambia despues.
        ///
        /// Se cuentan digitos ASCII, no caracteres: "099 123-4567" es un
        /// telefono perfectamente valido. Solo se aceptan separadores que la
        /// gente usa de verdad: espacio, guion, parentesis, signo +, punto.
        /// char.IsDigit aceptaria digitos Unicode (arabigo-indicos,
        /// devanagari); aqui se exige ASCII explicito.
        /// </summary>
        private static ResultadoFormatoTelefono ValidarFormatoTelefono(string telefono)
        {
            int digitos = 0;
            foreach (char c in telefono)
            {
                if (c >= '0' && c <= '9')
                {
                    digitos++;
                }
                else if (c == ' ' || c == '-' || c == '(' || c == ')' || c == '+' || c == '.')
                {
                    /* Nada: son caracteres permitidos. */
                }
                else
                {
                    return ResultadoFormatoTelefono.CaracterInvalido;
                }
            }

            if (digitos < DigitosMinimosTelefono) { return ResultadoFormatoTelefono.MuyPocosDigitos; }
            if (digitos > DigitosMaximosTelefono) { return ResultadoFormatoTelefono.MuyMuchosDigitos; }

            return ResultadoFormatoTelefono.Valido;
        }

        /* Las cuatro opciones que ofrece el combo de estado civil en la pantalla.
           No es una tabla de catalogo porque no vale la pena una tabla de cinco
           filas fijas para esto. */
        private static readonly string[] EstadosCivilesValidos =
        {
            "Soltero/a", "Casado/a", "Unión de hecho", "Divorciado/a", "Viudo/a"
        };

        /// <summary>
        /// Cadena vacia si el contacto personal sirve; si no, el mensaje para el
        /// usuario.
        ///
        /// GuardarContacto es la unica de las doce escrituras del modulo que no
        /// llamaba a un Validar*: un POST directo podia escribir cualquier texto
        /// en Empleados.EstadoCivil, un campo que mantiene Talento Humano y lee
        /// el modulo medico. Los otros tres campos son de la lista blanca de
        /// LeerContacto, pero esa lista solo filtra CLAVES, no valores.
        /// </summary>
        public static string ValidarContacto(EntPerfilContacto contacto)
        {
            if (contacto == null)
            {
                return "No se recibió el contacto.";
            }

            /* Los cuatro campos son opcionales: la persona puede guardar solo el
               que quiera y dejar el resto tal como estaba. */
            if (!string.IsNullOrWhiteSpace(contacto.EstadoCivil))
            {
                bool valido = false;
                foreach (string opcion in EstadosCivilesValidos)
                {
                    if (opcion == contacto.EstadoCivil) { valido = true; break; }
                }

                if (!valido)
                {
                    return "El estado civil no es válido.";
                }
            }

            if (!string.IsNullOrWhiteSpace(contacto.TelefonoPersonal)
                && ValidarFormatoTelefono(contacto.TelefonoPersonal) != ResultadoFormatoTelefono.Valido)
            {
                return "El teléfono debe tener entre 7 y 15 dígitos.";
            }

            if (!string.IsNullOrWhiteSpace(contacto.CorreoPersonal))
            {
                string correo = contacto.CorreoPersonal;
                int arroba = correo.IndexOf('@');

                /* "algo antes" exige arroba > 0; "algo despues" exige que no sea
                   el ultimo caracter; el punto se busca desde despues de la
                   arroba, no en el correo completo, porque un punto solo en la
                   parte local ("nombre.apellido@dominio") no cuenta. */
                bool tieneAlgoAntes  = arroba > 0;
                bool tieneAlgoDespues = arroba >= 0 && arroba < correo.Length - 1;
                bool tienePuntoDespues = arroba >= 0 && correo.IndexOf('.', arroba + 1) > arroba;

                if (!tieneAlgoAntes || !tieneAlgoDespues || !tienePuntoDespues)
                {
                    return "El correo personal no es válido.";
                }
            }

            /* Direccion no se valida mas alla de lo que ya hace la lista blanca
               de LeerContacto: es texto libre y no hay forma razonable de
               distinguir una direccion valida de una que no lo es. */

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

            /* AnioDesde solo admite hasta el anio actual, mientras que AnioHasta
               (mas abajo) admite hasta el anio proximo. La asimetria es
               deliberada y no un descuido: un empleo que ya empezo no puede
               tener fecha de inicio en el futuro, pero un contrato a plazo fijo
               si tiene una fecha de fin conocida de antemano, igual que
               AnioGraduacion en ValidarEstudio. */
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

        /// <summary>
        /// Los unicos tipos de imagen que se aceptan. Lista blanca y no negra:
        /// un SVG puede traer JavaScript adentro, y con una lista negra la
        /// pregunta pasa a ser "de que nos acordamos de prohibir".
        /// </summary>
        private static readonly string[] TiposDeFotoValidos = { "image/jpeg", "image/png" };

        /// <summary>
        /// Longitud maxima del base64 de la foto. 500 000 caracteres son unos
        /// 366 KB de imagen: el navegador manda alrededor de 25 KB -reduce a
        /// 256x256 antes de subir- asi que esto es un techo, no un limite de uso.
        /// Se mide sobre el texto y antes de decodificar para no gastar memoria
        /// decodificando lo que se va a rechazar.
        /// </summary>
        private const int LargoMaximoFoto = 500000;

        /// <summary>
        /// Valida la foto que llega del navegador. Cadena vacia si esta bien.
        ///
        /// Va en el servidor y no solo en el navegador porque AdministrarPerfil.ashx
        /// es alcanzable por HTTP directo: lo que el canvas del navegador garantiza
        /// -que sea un JPEG de 256x256- no lo garantiza nadie para un POST hecho a
        /// mano.
        /// </summary>
        public static string ValidarFoto(EntPerfilFoto foto)
        {
            if (foto == null) { return "No se recibió la foto."; }

            string tipo = (foto.Tipo ?? "").Trim().ToLowerInvariant();
            bool tipoValido = false;

            for (int i = 0; i < TiposDeFotoValidos.Length; i++)
            {
                if (TiposDeFotoValidos[i] == tipo) { tipoValido = true; }
            }

            if (!tipoValido)
            {
                return "La foto debe ser una imagen JPG o PNG.";
            }

            string base64 = (foto.Base64 ?? "").Trim();

            if (base64 == "")
            {
                return "No se recibió el contenido de la foto.";
            }

            /* El navegador manda solo el payload. Si llega el data URI completo,
               guardarlo dejaria el prefijo dentro del base64 y la imagen no se
               veria nunca sin que nada avise. */
            if (base64.StartsWith("data:"))
            {
                return "El formato de la foto no es el esperado.";
            }

            if (base64.Length > LargoMaximoFoto)
            {
                return "La foto es demasiado grande. Use una imagen más liviana.";
            }

            /* Que decodifique es la prueba de que es base64 de verdad. Sin esto,
               cualquier texto quedaria guardado como si fuera una imagen y el
               fallo aparecería recien en el navegador de la persona. */
            try
            {
                Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                return "El formato de la foto no es el esperado.";
            }

            return "";
        }

        /// <summary>
        /// De que puede colgar un documento. Los mismos dos valores que espera
        /// Sp_RTA_PerfilGuardarDocumento; si algun dia se agrega un tercero, van
        /// juntos o el procedimiento lo rechazara con -1 sin explicar por que.
        /// </summary>
        private static readonly string[] OrigenesDeDocumentoValidos = { "CERTIFICACION", "CARGAFAMILIAR" };

        /// <summary>
        /// Extensiones aceptadas. Lista BLANCA, y tiene que seguir siendolo: un
        /// .aspx o un .ashx dentro de una carpeta del sitio es codigo que el
        /// servidor ejecuta. Con una lista negra la pregunta pasa a ser de que
        /// nos acordamos de prohibir, y basta olvidar una para entregar el
        /// servidor.
        /// </summary>
        private static readonly string[] ExtensionesDeDocumentoValidas = { "pdf", "jpg", "jpeg", "png" };

        /// <summary>5 MB. Web.config permite 200, pero un respaldo escaneado no los necesita.</summary>
        private const long TamanoMaximoDocumento = 5242880;

        /// <summary>
        /// Valida un documento de respaldo antes de guardarlo. Cadena vacia si
        /// esta bien.
        ///
        /// Lo que esta funcion NO valida, porque no puede: que el IdOrigen sea de
        /// quien sube el archivo. Eso lo comprueba Sp_RTA_PerfilGuardarDocumento
        /// contra la base y devuelve -1 si no lo es.
        /// </summary>
        public static string ValidarDocumento(string origen, int idOrigen, string nombreArchivo, long tamanoBytes)
        {
            string origenNormalizado = (origen ?? "").Trim().ToUpperInvariant();
            bool origenValido = false;

            for (int i = 0; i < OrigenesDeDocumentoValidos.Length; i++)
            {
                if (OrigenesDeDocumentoValidos[i] == origenNormalizado) { origenValido = true; }
            }

            if (!origenValido)
            {
                return "No se pudo determinar a qué registro corresponde el documento.";
            }

            if (idOrigen <= 0)
            {
                return "No se pudo determinar a qué registro corresponde el documento.";
            }

            string nombre = (nombreArchivo ?? "").Trim();

            if (nombre == "")
            {
                return "No se recibió el archivo.";
            }

            /* GetExtension devuelve la ULTIMA, que es la que decide como trata el
               servidor al archivo: "titulo.pdf.aspx" es un .aspx. */
            string extension = Path.GetExtension(nombre).TrimStart('.').ToLowerInvariant();
            bool extensionValida = false;

            for (int i = 0; i < ExtensionesDeDocumentoValidas.Length; i++)
            {
                if (ExtensionesDeDocumentoValidas[i] == extension) { extensionValida = true; }
            }

            if (!extensionValida)
            {
                return "El documento debe ser un PDF o una imagen JPG o PNG.";
            }

            if (tamanoBytes <= 0)
            {
                return "El archivo está vacío.";
            }

            if (tamanoBytes > TamanoMaximoDocumento)
            {
                return "El documento no puede pesar más de 5 MB.";
            }

            return "";
        }

        /// <summary>
        /// Cuantos caracteres exige el buscador de personal antes de traer nada.
        ///
        /// No es un capricho de interfaz: sin minimo, un filtro vacio devuelve a
        /// todo el personal activo de una sola vez, en una tabla sin paginar y en
        /// un sistema donde DataTables no carga. Dos caracteres bastan para que
        /// quien busca sepa a quien busca.
        /// </summary>
        public const int MinimoFiltroPersonal = 2;

        /// <summary>
        /// Valida el filtro del buscador de personal. Cadena vacia si sirve.
        ///
        /// Recorta antes de medir: sin eso, tres espacios pasan la comprobacion de
        /// largo y el procedimiento recibe un filtro que no filtra nada.
        /// </summary>
        public static string ValidarFiltroPersonal(string filtro)
        {
            string limpio = (filtro ?? "").Trim();

            if (limpio.Length < MinimoFiltroPersonal)
            {
                return "Escriba al menos " + MinimoFiltroPersonal + " caracteres para buscar.";
            }

            return "";
        }

        /* ------------------------------------------- datos personales ------ */

        /* Los anchos son los de la columna MAS ANGOSTA de las dos tablas donde
           cae cada campo, no los de la mas ancha: un valor que entra en
           Empleados.Nombre nvarchar(350) pero no en R_Usuarios.Nom_Usuario
           varchar(100) no deja el guardado a medias, lo hace fallar entero con
           "String or binary data would be truncated". Medidos contra produccion
           el 2026-09-17; estan campo por campo en la seccion 2 del diseno. */
        private const int LargoMaximoNombre = 100;
        private const int LargoMaximoCedula = 32;
        private const int LargoMaximoCargo  = 128;
        private const int LargoMaximoArea   = 128;
        private const int LargoMaximoCiudad = 150;
        private const int LargoMaximoCorreo = 100;

        /// <summary>
        /// Arma los datos personales editables a partir del payload, leyendo
        /// solo las ocho claves que este codigo esta escrito para leer.
        ///
        /// La lista blanca es la forma de este codigo, no un arreglo que se
        /// recorra: una clave como "estado" o "rolUsuario" no se cuela porque no
        /// hay linea que la pida ni propiedad que la reciba. El horario no esta
        /// a proposito: es de solo lectura y tiene su propio modulo.
        /// </summary>
        public static EntPerfilDatosPersonales LeerDatosPersonales(IDictionary<string, object> campos)
        {
            return new EntPerfilDatosPersonales
            {
                Nombre             = Texto(campos, "nombre"),
                Cedula             = Texto(campos, "cedula"),
                FechaNacimiento    = Texto(campos, "fechaNacimiento"),
                Cargo              = Texto(campos, "cargo"),
                Area               = Texto(campos, "area"),
                Ciudad             = Texto(campos, "ciudad"),
                CodJefeInmediato   = Texto(campos, "codJefeInmediato"),
                CorreoNotificacion = Texto(campos, "correoNotificacion")
            };
        }

        /// <summary>
        /// Cadena vacia si los datos personales sirven; si no, el mensaje para el
        /// usuario.
        ///
        /// Recibe la cabecera GUARDADA porque un campo solo se valida cuando
        /// cambio. No es una comodidad: hay tres personas en produccion cuya
        /// cedula no pasa el digito verificador -una de once digitos y dos con el
        /// tercer digito en 9-, y validar siempre dejaria a esas tres sin poder
        /// guardar NINGUN campo, trabadas por un valor que nadie estaba tocando.
        /// Lo mismo vale para el correo.
        ///
        /// codUsuario es de quien es el perfil, y hace falta para la unica regla
        /// que no se puede leer de los campos: que nadie sea su propio jefe.
        /// </summary>
        public static string ValidarDatosPersonales(EntPerfilDatosPersonales enviado,
                                                    EntPerfilCabecera actual,
                                                    string codUsuario)
        {
            if (enviado == null) { return "No se recibieron los datos personales."; }

            /* Sin cabecera no hay contra que comparar, y validarlo todo seria
               justo lo que traba a las tres personas de arriba. Se rechaza en vez
               de adivinar. */
            if (actual == null) { return "No se pudieron leer los datos actuales de la persona."; }

            /* R_Usuarios.Nom_Usuario es NOT NULL: es el unico de los ocho que no
               se puede dejar en blanco, y conviene decirlo con este mensaje y no
               dejar que salga el de SQL Server. */
            string nombre = (enviado.Nombre ?? "").Trim();
            if (nombre == "") { return "El nombre es obligatorio."; }
            if (nombre.Length > LargoMaximoNombre)
            {
                return "El nombre no puede pasar de " + LargoMaximoNombre + " caracteres.";
            }

            string cedula = (enviado.Cedula ?? "").Trim();
            if (cedula.Length > LargoMaximoCedula)
            {
                return "La cédula no puede pasar de " + LargoMaximoCedula + " caracteres.";
            }
            if (Cambio(cedula, actual.Cedula) && cedula != "" && !NegPerfilCedula.EsValida(cedula))
            {
                return "La cédula no es válida: revise el número.";
            }

            string fecha = (enviado.FechaNacimiento ?? "").Trim();
            if (fecha != "")
            {
                /* Mismo formato explicito y misma InvariantCulture que
                   EdadDesdeTexto, y por la misma razon: en la base hay 129 fechas
                   dd/MM/yyyy y 80 tienen el dia por encima de 12. Dejar que lo
                   adivine la cultura del servidor rompe esas 80 sin dar error. */
                DateTime nacimiento;
                bool valida = DateTime.TryParseExact(fecha, FormatoFecha,
                                                     CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out nacimiento);
                if (!valida)
                {
                    return "La fecha de nacimiento tiene que estar en formato dd/mm/aaaa.";
                }
                if (nacimiento.Date > DateTime.Today)
                {
                    return "La fecha de nacimiento no puede ser futura.";
                }
            }

            if ((enviado.Cargo ?? "").Trim().Length > LargoMaximoCargo)
            {
                return "El cargo no puede pasar de " + LargoMaximoCargo + " caracteres.";
            }

            if ((enviado.Area ?? "").Trim().Length > LargoMaximoArea)
            {
                return "El área no puede pasar de " + LargoMaximoArea + " caracteres.";
            }

            if ((enviado.Ciudad ?? "").Trim().Length > LargoMaximoCiudad)
            {
                return "La ciudad no puede pasar de " + LargoMaximoCiudad + " caracteres.";
            }

            /* Que el jefe exista y este activo lo comprueba el procedimiento, que
               es el unico que puede mirar la base. Aca va lo que no necesita
               mirarla: que no sea uno mismo. Se compara ignorando mayusculas
               porque Cod_Usuario no distingue mayusculas en la base y "usr001"
               dejaria a esa persona sin aprobador igual que "USR001". */
            string jefe = (enviado.CodJefeInmediato ?? "").Trim();
            if (jefe != "" && string.Equals(jefe, (codUsuario ?? "").Trim(),
                                            StringComparison.OrdinalIgnoreCase))
            {
                return "Una persona no puede ser su propio jefe inmediato.";
            }

            string correo = (enviado.CorreoNotificacion ?? "").Trim();
            if (correo.Length > LargoMaximoCorreo)
            {
                return "El correo no puede pasar de " + LargoMaximoCorreo + " caracteres.";
            }
            if (Cambio(correo, actual.CorreoNotificacion) && correo != ""
                && !CorreoBienFormado(correo))
            {
                return "El correo de notificación no es válido.";
            }

            return "";
        }

        /// <summary>
        /// true si el valor enviado difiere del guardado. Compara recortado y
        /// tratando null como cadena vacia; distingue mayusculas, porque
        /// cambiarle la capitalizacion a un correo SI es un cambio.
        /// </summary>
        private static bool Cambio(string enviado, string guardado)
        {
            return (enviado ?? "").Trim() != (guardado ?? "").Trim();
        }

        /// <summary>
        /// Misma comprobacion que ValidarContacto le hace al correo personal:
        /// algo antes de la arroba, algo despues, y un punto despues de la
        /// arroba. El punto se busca desde despues de la arroba y no en el correo
        /// completo, porque un punto solo en la parte local -"nombre.apellido@x"-
        /// no alcanza.
        /// </summary>
        private static bool CorreoBienFormado(string correo)
        {
            int arroba = correo.IndexOf('@');

            bool tieneAlgoAntes    = arroba > 0;
            bool tieneAlgoDespues  = arroba >= 0 && arroba < correo.Length - 1;
            bool tienePuntoDespues = arroba >= 0 && correo.IndexOf('.', arroba + 1) > arroba;

            return tieneAlgoAntes && tieneAlgoDespues && tienePuntoDespues;
        }
    }
}
