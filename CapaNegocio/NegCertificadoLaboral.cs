using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

namespace CapaNegocio
{
    /// <summary>
    /// El certificado laboral: arma la foto de los datos y su HTML.
    ///
    /// El HTML vive aqui -y no junto al PDF- por lo mismo que NegPerfilCv:
    /// CapaPruebas solo referencia CapaEntidad y CapaNegocio. La misma cadena
    /// sirve para la vista previa en pantalla y para el PDF, asi que lo que la
    /// persona ve antes de generar es lo que se imprime.
    ///
    /// La redaccion es la de las plantillas Word de Talento Humano. Va con
    /// tablas y estilos en linea porque HtmlRenderer ignora en silencio lo que
    /// no entiende (posiciones absolutas, flexbox).
    /// </summary>
    public static class NegCertificadoLaboral
    {
        public const string TipoSin = "SIN";
        public const string TipoCon = "CON";
        public const string EmpresaDos = "DOS";
        public const string EmpresaAgility = "AGILITY";

        private static readonly CultureInfo Es = new CultureInfo("es-EC");

        private static readonly string[] Meses =
        {
            "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio",
            "agosto", "septiembre", "octubre", "noviembre", "diciembre"
        };

        /* ------------------------------------------------------- datos --- */

        /// <summary>
        /// Lo que va en el certificado, en vivo, o un mensaje para la persona si
        /// no se puede emitir. Nunca las dos cosas.
        /// </summary>
        public static EntCertificadoLaboral Preparar(string codUsuario, string tipo, string autor,
                                                     DateTime ahora, out string error)
        {
            return Armar(DaoCertificadoLaboral.Datos(codUsuario), tipo, autor, ahora, out error);
        }

        /// <summary>
        /// La parte de Preparar que no toca la base: separada para poder probarla.
        /// </summary>
        public static EntCertificadoLaboral Armar(EntCertificadoDatos d, string tipo, string autor,
                                                  DateTime ahora, out string error)
        {
            error = "";
            string t = (tipo ?? "").Trim().ToUpperInvariant();

            if (t != TipoSin && t != TipoCon)
            {
                error = "Elija si el certificado va con o sin salario.";
                return null;
            }

            if (d == null || !d.Encontrado)
            {
                error = "No pudimos identificar su perfil de forma única. Escriba a Talento Humano para que corrijan su código de usuario.";
                return null;
            }

            /* Sin el maestro de GTH no hay fecha de ingreso, y un certificado
               laboral sin ella no dice nada. Mejor no emitirlo que emitirlo con
               un hueco. */
            if (!d.EnMaestro || !d.FechaIngreso.HasValue)
            {
                error = "Talento Humano todavía no tiene registrada su fecha de ingreso. Escríbales para que lo actualicen y vuelva a intentarlo.";
                return null;
            }

            string empresa = Empresa(d.Empresa);
            if (empresa == "")
            {
                error = "Talento Humano no tiene registrada la empresa en la que usted trabaja. Escríbales para que lo actualicen.";
                return null;
            }

            if (string.IsNullOrWhiteSpace(d.Cargo))
            {
                error = "Talento Humano no tiene registrado su cargo. Escríbales para que lo actualicen.";
                return null;
            }

            decimal? salario = null;
            if (t == TipoCon)
            {
                decimal vigente = NegHorasExtras.SalarioVigente(d.Salarios, ahora.Date);
                if (vigente <= 0m)
                {
                    error = "No hay un sueldo registrado para usted, así que solo se puede emitir el certificado sin salario. Si necesita el otro, escriba a Talento Humano.";
                    return null;
                }
                salario = vigente;
            }

            return new EntCertificadoLaboral
            {
                CodUsuario     = d.CodUsuario,
                Tipo           = t,
                Empresa        = empresa,
                Nombre         = (d.Nombre ?? "").Trim().ToUpper(Es),
                Cedula         = (d.Cedula ?? "").Trim(),
                Cargo          = d.Cargo.Trim().ToUpper(Es),
                FechaIngreso   = d.FechaIngreso,
                Salario        = salario,
                Ciudad         = (d.Ciudad ?? "").Trim(),
                FechaSolicitud = ahora,
                UsuSolicita    = autor ?? ""
            };
        }

        /// <summary>
        /// "DOS" o "AGILITY", o vacio si el maestro trae otra cosa. Se mira el
        /// comienzo y no el texto entero: el maestro de horas extras ya tiene
        /// "AGILITY - SERVICIOS PROFESIONALES" y "DOS S.A.".
        /// </summary>
        public static string Empresa(string valor)
        {
            string v = (valor ?? "").Trim().ToUpperInvariant();
            if (v.StartsWith(EmpresaAgility)) { return EmpresaAgility; }
            if (v == EmpresaDos || v.StartsWith(EmpresaDos + " ")) { return EmpresaDos; }
            return "";
        }

        public static int Registrar(EntCertificadoLaboral c, string ip)
        {
            return DaoCertificadoLaboral.Registrar(c, ip);
        }

        public static List<EntCertificadoLaboral> Listar(string codUsuario)
        {
            return DaoCertificadoLaboral.Listar(codUsuario);
        }

        public static EntCertificadoLaboral Obtener(int idCertificado, string codUsuario)
        {
            return DaoCertificadoLaboral.Obtener(idCertificado, codUsuario);
        }

        /* --------------------------------------------------- formatos --- */

        /// <summary>"13 de octubre de 2026".</summary>
        public static string FechaLarga(DateTime fecha)
        {
            return fecha.Day + " de " + Meses[fecha.Month - 1] + " de " + fecha.Year;
        }

        /// <summary>"$1.250,00 USD", como las plantillas de Talento Humano.</summary>
        public static string Dinero(decimal monto)
        {
            NumberFormatInfo f = new NumberFormatInfo
            {
                NumberGroupSeparator = ".",
                NumberDecimalSeparator = ",",
                NumberDecimalDigits = 2
            };
            return "$" + monto.ToString("N2", f) + " USD";
        }

        /// <summary>
        /// El lugar del encabezado. Quito va como "Quito, D.M." -asi esta en la
        /// plantilla- y es tambien lo que se usa si no hay ciudad: es la matriz.
        /// </summary>
        public static string Lugar(string ciudad)
        {
            string c = (ciudad ?? "").Trim();
            if (c == "" || c.Equals("QUITO", StringComparison.OrdinalIgnoreCase)) { return "Quito, D.M."; }
            return Es.TextInfo.ToTitleCase(c.ToLower(Es));
        }

        /* ------------------------------------------------------- html --- */

        private class Firmante
        {
            public string RazonSocial;
            public string Nombre;
            public string Cargo;
            public string Correo;
        }

        private static Firmante FirmanteDe(string empresa)
        {
            if (empresa == EmpresaAgility)
            {
                return new Firmante
                {
                    RazonSocial = "AGILITY SERVICES IT",
                    Nombre = "CARLOS PINOS",
                    Cargo = "Gerente General",
                    Correo = ""
                };
            }

            return new Firmante
            {
                RazonSocial = "COMPUEQUIP DOS S.A.",
                Nombre = "ANDRADE SALCEDO MARTHA CAROLINA",
                Cargo = "Jefe de Talento Humano y Cultura",
                Correo = "candrade@dos.com.ec"
            };
        }

        /// <summary>Quien firma, en corto, para las tablas del historial.</summary>
        public static string FirmaCorta(string empresa)
        {
            return empresa == EmpresaAgility ? "Gerencia General" : "Jefatura TH y Cultura";
        }

        /// <summary>
        /// El certificado completo. Las imagenes llegan como data URI porque esta
        /// capa no sabe donde estan los archivos del sitio; con cadena vacia se
        /// omite la imagen y el resto sale igual.
        ///
        /// Las medidas estan en puntos de una hoja A4 (595 x 842) generada sin
        /// margen: el membrete de DOS va de borde a borde.
        /// </summary>
        public static string Html(EntCertificadoLaboral c, string membreteDos, string logoAgility)
        {
            if (c == null) { return ""; }

            Firmante f = FirmanteDe(c.Empresa);
            bool dos = c.Empresa != EmpresaAgility;
            string nombre = Escapar(c.Nombre);

            StringBuilder h = new StringBuilder();
            h.Append("<html><head><meta charset=\"utf-8\" /></head>");
            h.Append("<body style=\"margin:0; font-family: Arial, Helvetica, sans-serif; font-size:11pt; color:#000\">");

            /* Cabecera: el membrete de DOS ocupa todo el ancho; el logo de Agility
               va chico a la derecha. */
            if (dos)
            {
                if (membreteDos != "")
                {
                    h.Append("<img src=\"").Append(membreteDos).Append("\" style=\"width:595px; height:127px\" />");
                }
                else
                {
                    h.Append("<div style=\"height:127px\"></div>");
                }
            }
            else
            {
                h.Append("<table style=\"width:595px; border-collapse:collapse\"><tr>");
                h.Append("<td style=\"height:150px; padding:33px 58px 0 0; text-align:right; vertical-align:top\">");
                if (logoAgility != "")
                {
                    h.Append("<img src=\"").Append(logoAgility).Append("\" style=\"width:87px; height:95px\" />");
                }
                h.Append("</td></tr></table>");
            }

            h.Append("<div style=\"padding:").Append(dos ? "14" : "4").Append("px 84px 0 84px; line-height:1.4\">");

            h.Append("<p style=\"margin:0 0 40px 0; line-height:16pt\">")
             .Append(Escapar(Lugar(c.Ciudad))).Append(", ").Append(FechaLarga(c.FechaSolicitud))
             .Append("</p>");

            h.Append("<p style=\"margin:0 0 26px 0; text-align:center; font-weight:bold; letter-spacing:6px\">C E R T I F I C A D O</p>");

            h.Append("<p style=\"margin:0 0 22px 0; font-weight:bold\">A quien corresponda. -</p>");

            h.Append("<p style=\"margin:0 0 15px 0; text-align:justify; line-height:16pt\">")
             .Append(Escapar(f.RazonSocial))
             .Append(" extiende el presente certificado a el(la) señor(a) <b>").Append(nombre)
             /* La coma va dentro del <b>: fuera de el, HtmlRenderer la separa con
                un espacio ("0916559735 ,"). */
             .Append("</b> con Cédula de Ciudadanía No. <b>").Append(Escapar(c.Cedula))
             .Append(",</b> quien ingresó a laborar a la empresa en calidad de <b>").Append(Escapar(c.Cargo))
             .Append("</b> desde el ").Append(c.FechaIngreso.HasValue ? FechaLarga(c.FechaIngreso.Value) : "")
             .Append(" hasta la actualidad.");

            if (c.Tipo == TipoCon && c.Salario.HasValue)
            {
                h.Append(" Percibiendo ingresos mensuales de ").Append(Dinero(c.Salario.Value))
                 .Append(", conforme a su banda salarial.");
            }
            h.Append("</p>");

            h.Append("<p style=\"margin:0 0 15px 0; text-align:justify; line-height:16pt\">Durante este tiempo ha demostrado alto sentido de responsabilidad, compromiso y calidad profesional en cada una de las actividades encomendadas.</p>");

            h.Append("<p style=\"margin:0 0 15px 0; text-align:justify; line-height:16pt\">El(la) señor(a) <b>").Append(nombre)
             .Append("</b> puede hacer uso del presente certificado en beneficio de sus intereses.</p>");

            h.Append("<p style=\"margin:0\">Atentamente,</p>");

            /* El espacio de la firma: se firma a mano sobre la linea. */
            h.Append("<div style=\"height:70px\"></div>");
            h.Append("<div style=\"width:190px; border-top:1px solid #000; margin-bottom:4px\"></div>");

            /* Una linea por div: con <br /> despues de un </b>, HtmlRenderer deja
               una linea en blanco de mas. */
            h.Append("<div style=\"line-height:1.3\">").Append(Escapar(f.Nombre)).Append("</div>");
            h.Append("<div style=\"line-height:1.3; font-weight:bold\">").Append(Escapar(f.Cargo)).Append("</div>");
            if (f.Correo != "")
            {
                h.Append("<div style=\"line-height:1.3\"><b>Correo:</b> ").Append(Escapar(f.Correo)).Append("</div>");
            }
            h.Append("<div style=\"line-height:1.3; font-weight:bold\">").Append(Escapar(f.RazonSocial)).Append("</div>");

            h.Append("</div>");

            if (dos)
            {
                /* El pie de pagina va en el flujo, no pegado al borde: HtmlRenderer
                   no posiciona en absoluto. El espaciador lo deja cerca del final
                   de la hoja con el texto de largo normal. */
                h.Append("<div style=\"height:").Append(c.Tipo == TipoCon ? "120" : "150").Append("px\"></div>");
                h.Append("<p style=\"margin:0; text-align:center; font-size:8.5pt; line-height:1.35\">")
                 .Append("Dirección: Av. Occidental Oe6-201 y José Miguel Carrión, Diagonal al Diario El Hoy<br />")
                 .Append("Teléfono: +593 98 616 9313<br />www.dos.com.ec</p>");
            }

            h.Append("</body></html>");
            return h.ToString();
        }

        private static string Escapar(string texto)
        {
            return WebUtility.HtmlEncode(texto ?? "");
        }
    }
}
