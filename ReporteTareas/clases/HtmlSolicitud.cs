using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF
{
    /// <summary>
    /// Arma el HTML del documento de una solicitud, siguiendo los modelos que
    /// entregó Talento Humano: VACACIONES, PERMISOS, TELETRABAJO y RECUPERACIÓN.
    ///
    /// Los cuatro modelos son el mismo documento con distinta sección de detalle,
    /// no cuatro documentos. Comparten el sello de la cabecera, el bloque de datos
    /// del colaborador, el pie con el código de verificación y las tres firmas.
    /// Lo único que cambia es qué sección va en el medio, y en recuperación además
    /// un cuarto recuadro de firma para el cierre.
    ///
    /// Por eso acá no hay cuatro plantillas sino una, con las secciones que
    /// correspondan: un teletrabajo por horas lleva las horas Y el detalle de
    /// teletrabajo, aunque el modelo muestre cada cosa por separado.
    ///
    /// Dos restricciones que explican por qué el marcado se ve anticuado:
    ///
    /// 1. La conversión la hace Pechkin 0.5.8, que envuelve una versión vieja de
    ///    wkhtmltopdf. No entiende flexbox ni grid, así que la maquetación va con
    ///    tablas y estilos en línea. Cambiarlo a CSS moderno rompe el PDF sin dar
    ///    ningún error: sale la página desarmada.
    ///
    /// 2. Las imágenes van por ruta de archivo, no como data URI. Es el mismo
    ///    camino que ya usa GenerarCodigoQR, que devuelve una ruta justamente
    ///    porque esa versión no resuelve base64 embebido de forma confiable.
    /// </summary>
    public class HtmlSolicitud
    {
        /* Los colores del modelo. El azul es el de los rótulos y los títulos; el
           rojo va solo en "CONTROL DE GESTIÓN", dentro del sello. */
        private const string Azul = "#1F3864";
        private const string Rojo = "#C00000";
        private const string Punteado = "#C9C9C9";
        private const string Regla = "#D0D0D0";
        private const string Gris = "#8A8A8A";
        private const string Texto = "#333333";

        /// <summary>
        /// El HTML completo del documento.
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud.</param>
        /// <param name="firmas">Firmas ya registradas, con RutaTrazo resuelta.</param>
        /// <param name="folio">Folio y código de verificación.</param>
        /// <param name="rutaLogo">Ruta en disco del logo. Si está vacía, se omite.</param>
        /// <param name="detalle">Detalle del permiso. Puede venir null en vacaciones.</param>
        /// <param name="periodos">Períodos con saldo. Solo aplica a vacaciones.</param>
        public static string Construir(EntSolicitud solicitud, List<EntFirmaSolicitud> firmas,
                                       string folio, string rutaLogo, EntDetallePermiso detalle,
                                       string periodos)
        {
            return Construir(solicitud, firmas, folio, rutaLogo, detalle, periodos, "");
        }

        /// <summary>
        /// Igual, con un bloque propio antes del pie.
        ///
        /// Lo usa el correo al jefe, que lleva el mismo documento que el colaborador
        /// mas los botones de aprobar y rechazar. El documento en si no cambia: los
        /// botones se agregan al final, despues de las firmas.
        /// </summary>
        public static string Construir(EntSolicitud solicitud, List<EntFirmaSolicitud> firmas,
                                       string folio, string rutaLogo, EntDetallePermiso detalle,
                                       string periodos, string htmlAcciones)
        {
            bool esVacaciones = solicitud.IdTipoSolicitud != 1;
            bool hayRecuperacion = detalle != null && detalle.TieneRecuperacion;

            StringBuilder h = new StringBuilder();

            h.Append("<!DOCTYPE html><html><head><meta charset='utf-8' /></head>");
            /* El margen del documento va acá y no en la configuración de Pechkin:
               así el resultado no depende de qué margen traiga por defecto esa
               versión, que no está documentado en ninguna parte. */
            h.Append("<body style=\"font-family:'Segoe UI',Arial,Helvetica,sans-serif; font-size:12px; color:")
             .Append(Texto).Append("; margin:0; padding:10px 26px 0\">");

            Sello(h);

            h.Append("<div style='margin-top:18px; font-size:15px; font-weight:bold; color:")
             .Append(Azul).Append("'>")
             .Append(esVacaciones ? "SOLICITUD DE VACACIONES" : "SOLICITUD DE PERMISO")
             .Append("</div>");

            h.Append("<div style='border-top:1px solid ").Append(Regla).Append("; margin-top:8px'></div>");

            /* El cuerpo ocupa tres cuartos del ancho y el logo va en la columna de
               la derecha, a la altura de los primeros campos, como en el modelo. */
            h.Append("<table width='100%' cellpadding='0' cellspacing='0' style='margin-top:6px'><tr>");
            h.Append("<td width='76%' valign='top'>");

            h.Append("<table width='100%' cellpadding='0' cellspacing='0'>");
            Fila(h, "Fecha", SoloFecha(solicitud.FechaRegistro));
            Fila(h, "Cédula", solicitud.Cedula);
            Fila(h, "Colaborador", solicitud.Colaborador);

            if (esVacaciones)
            {
                Fila(h, "Jefe Inmediato", solicitud.JefeInmediato);
                Fila(h, "Reemplazo", solicitud.Remplazo);
            }
            else
            {
                Fila(h, "Departamento", solicitud.Departamento);
                Fila(h, "Jefe Inmediato", solicitud.JefeInmediato);
                Fila(h, "Tipo de Permiso", TipoDelPermiso(solicitud, detalle));
            }
            h.Append("</table>");

            if (esVacaciones)
            {
                SeccionVacaciones(h, solicitud, periodos);
            }
            else
            {
                SeccionHoras(h, solicitud, detalle);

                if (detalle != null && detalle.EsTeletrabajo)
                {
                    SeccionTeletrabajo(h, detalle);
                }

                if (hayRecuperacion)
                {
                    SeccionRecuperacion(h, solicitud, detalle);
                }
            }

            if (!string.IsNullOrEmpty(solicitud.Observacion))
            {
                /* Texto corrido: una observación no es un par etiqueta/valor, y
                   ponerle un rótulo inventado quedaba raro en el documento. */
                Seccion(h, "Observaciones");
                h.Append("<div style='padding:4px 0 6px; border-bottom:1px dotted ")
                 .Append(Punteado).Append("'>")
                 .Append(Escapar(solicitud.Observacion)).Append("</div>");
            }

            h.Append("</td>");

            if (!string.IsNullOrEmpty(rutaLogo))
            {
                h.Append("<td width='24%' valign='top' align='right' style='padding-left:14px'>");
                h.Append("<img src='").Append(RutaComoUri(rutaLogo)).Append("' style='max-width:105px' />");
                h.Append("</td>");
            }

            h.Append("</tr></table>");

            h.Append(BloqueFirmas(firmas, hayRecuperacion));

            if (!string.IsNullOrEmpty(htmlAcciones)) { h.Append(htmlAcciones); }

            h.Append("<div style='margin-top:34px; text-align:center; font-family:\"Courier New\",monospace; font-size:9px; color:#9A9A9A'>");
            h.Append("Documento generado automáticamente &middot; código de verificación: ").Append(Escapar(folio));
            h.Append("</div>");

            h.Append("</body></html>");
            return h.ToString();
        }

        /// <summary>
        /// El sello de la cabecera: la C en un círculo y el nombre del sistema, con
        /// "CONTROL DE GESTIÓN" en rojo.
        ///
        /// El círculo se dibuja con CSS y no con una imagen porque no existe ese
        /// recurso en el proyecto. Se declara el border-radius con prefijo y sin él:
        /// si la versión de wkhtmltopdf no lo entiende, sale un cuadro con la C
        /// adentro, que se sigue leyendo.
        /// </summary>
        private static void Sello(StringBuilder h)
        {
            h.Append("<div style='text-align:center; padding-top:6px'>");
            h.Append("<div style='display:inline-block; width:30px; height:30px; line-height:28px; ")
             .Append("border:1.5px solid ").Append(Azul).Append("; ")
             .Append("-webkit-border-radius:15px; border-radius:15px; ")
             .Append("font-family:Georgia,'Times New Roman',serif; font-size:17px; color:").Append(Azul)
             .Append("; text-align:center'>C</div>");

            h.Append("<div style='margin-top:6px; font-size:11px; font-weight:bold; color:").Append(Azul).Append("'>");
            h.Append("SISTEMA DE <span style='color:").Append(Rojo).Append("'>CONTROL DE GESTIÓN</span> INTERNO");
            h.Append("</div>");
            h.Append("</div>");
        }

        private static void SeccionVacaciones(StringBuilder h, EntSolicitud solicitud, string periodos)
        {
            Seccion(h, "Detalle de Vacaciones");
            h.Append("<table width='100%' cellpadding='0' cellspacing='0'>");

            FilaSiHay(h, "Período", periodos);
            Fila(h, "Fechas", SoloFecha(solicitud.FechaDesde) + " al " + SoloFecha(solicitud.FechaHasta));
            Fila(h, "Días tomados", Dias(solicitud.TotalDias));

            if (solicitud.Feriado > 0)
            {
                /* Se dice explícitamente que los feriados no se cobraron: es la
                   explicación de por qué los días no cuadran con las fechas. */
                Fila(h, "Feriados no descontados", Dias(solicitud.Feriado));
            }

            Fila(h, "Saldo restante", Dias(solicitud.SaldoDias));
            Fila(h, "Regresa a trabajar", DiaSiguiente(SoloFecha(solicitud.FechaHasta)));

            h.Append("</table>");
        }

        private static void SeccionHoras(StringBuilder h, EntSolicitud solicitud, EntDetallePermiso detalle)
        {
            string desde = FechaYHora(solicitud.FechaDesde);
            string hasta = FechaYHora(solicitud.FechaHasta);

            /* Un teletrabajo de jornada completa no tiene horas que mostrar. Sin
               esto saldría la sección con tres renglones vacíos. */
            bool hayHoras = !string.IsNullOrEmpty(desde)
                            || !string.IsNullOrEmpty(hasta)
                            || !string.IsNullOrEmpty(solicitud.Horas);

            bool hayMensual = detalle != null && detalle.UsaPermisoMensual;

            if (!hayHoras && !hayMensual) { return; }

            Seccion(h, "Horas de Permiso");
            h.Append("<table width='100%' cellpadding='0' cellspacing='0'>");

            FilaSiHay(h, "Desde", desde);
            FilaSiHay(h, "Hasta", hasta);
            FilaSiHay(h, "Total de Horas", solicitud.Horas);

            if (hayMensual)
            {
                /* Solo el saldo, no la bolsa entera. Antes iba además un renglón
                   "¿Usa permiso mensual de 3h?: Sí", y en el documento leído de
                   corrido las tres horas se confundían con lo que a la persona le
                   quedaba. Que use la bolsa ya se deduce de que este renglón esté.

                   El texto es el congelado al pedir el permiso, no el saldo de hoy:
                   la especificación pide constancia de cuánto quedaba entonces. */
                FilaSiHay(h, "Saldo del permiso mensual", detalle.SaldoMensualTexto);
            }

            if (detalle != null)
            {
                FilaSiHay(h, "Tratamiento del excedente", RotuloExcedente(detalle.TratamientoExcedente));
                FilaSiHay(h, "Respaldo adjunto", RotuloRespaldo(detalle.RespaldoAdjunto));
            }

            h.Append("</table>");
        }

        /// <summary>
        /// El detalle de teletrabajo.
        ///
        /// El modelo lista Modalidad, Lugar y Motivo general. Se agregan medios de
        /// contacto, actividades y entregables porque son los campos por los que
        /// existe la política: sin ellos el documento no deja constancia de a qué
        /// se comprometió la persona. Si Talento Humano los quiere fuera, se sacan
        /// de acá y nada más.
        /// </summary>
        private static void SeccionTeletrabajo(StringBuilder h, EntDetallePermiso detalle)
        {
            Seccion(h, "Detalle de Teletrabajo");
            h.Append("<table width='100%' cellpadding='0' cellspacing='0'>");

            string modalidad = detalle.Modalidad == "HORAS"
                ? "Por horas · " + detalle.HoraDesde + " a " + detalle.HoraHasta + " (luego presencial)"
                : "Jornada completa";

            Fila(h, "Modalidad", modalidad);
            FilaSiHay(h, "Lugar", detalle.Lugar);
            FilaSiHay(h, "Motivo general", detalle.MotivoGeneral);
            FilaSiHay(h, "Medios de contacto", detalle.MediosContacto);
            FilaSiHay(h, "Actividades a ejecutar", detalle.Actividades);
            FilaSiHay(h, "Entregables esperados", detalle.Entregables);

            h.Append("</table>");
        }

        private static void SeccionRecuperacion(StringBuilder h, EntSolicitud solicitud, EntDetallePermiso detalle)
        {
            Seccion(h, "Recuperación de Jornada");
            h.Append("<table width='100%' cellpadding='0' cellspacing='0'>");

            string ausencia = SoloFecha(solicitud.FechaRegistro);
            if (!string.IsNullOrEmpty(solicitud.Horas))
            {
                ausencia = ausencia + " · " + solicitud.Horas;
            }
            FilaSiHay(h, "Fecha y tiempo de ausencia", ausencia);

            FilaSiHay(h, "Horas a recuperar", HorasARecuperar(solicitud, detalle));

            FilaSiHay(h, "Fecha propuesta de recuperación", detalle.RecFechaPropuesta);
            FilaSiHay(h, "Horario propuesto", detalle.RecHorario);
            FilaSiHay(h, "Actividades a ejecutar", detalle.RecActividades);
            FilaSiHay(h, "Entregables esperados", detalle.RecEntregables);
            FilaSiHay(h, "Fecha máxima de cierre", detalle.RecFechaMaxima);

            h.Append("</table>");
        }

        /// <summary>
        /// El tipo de permiso tal como va en el documento.
        ///
        /// Para lo registrado antes de agosto de 2026 no hay tipo: esas solicitudes
        /// solo llevan la actividad en texto libre, que es todo lo que se guardó.
        /// En "Otro" se muestran los dos, porque el tipo por sí solo no dice nada.
        /// </summary>
        private static string TipoDelPermiso(EntSolicitud solicitud, EntDetallePermiso detalle)
        {
            if (detalle == null || string.IsNullOrEmpty(detalle.TipoPermiso))
            {
                return solicitud.Actividad;
            }

            string rotulo = RotuloTipo(detalle.TipoPermiso);

            if (detalle.TipoPermiso == "OTRO" && !string.IsNullOrEmpty(solicitud.Actividad))
            {
                return rotulo + " · " + solicitud.Actividad;
            }

            return rotulo;
        }

        private static string RotuloTipo(string tipo)
        {
            switch (tipo)
            {
                case "PERSONAL": return "Personal";
                case "MEDICO": return "Médico";
                case "FAMILIAR": return "Familiar";
                case "CALAMIDAD": return "Calamidad";
                case "TELETRABAJO": return "Teletrabajo";
                case "OTRO": return "Otro";
                default: return tipo;
            }
        }

        private static string RotuloRespaldo(string valor)
        {
            switch (valor)
            {
                case "SI": return "Sí";
                case "NO": return "No";
                case "NO_APLICA": return "No aplica";
                default: return "";
            }
        }

        private static string RotuloExcedente(string valor)
        {
            switch (valor)
            {
                case "VACACIONES": return "Vacaciones";
                case "RECUPERACION": return "Recuperación";
                case "SIN_REMUNERACION": return "Permiso sin remuneración";
                default: return "";
            }
        }

        /// <summary>
        /// Las firmas: tres columnas sin recuadro, como en el modelo, y un cuarto
        /// bloque abajo para el cierre cuando hay plan de recuperación.
        ///
        /// En recuperación la primera columna se llama "Solicitud colaborador" y no
        /// "Firma colaborador": así lo distingue el modelo, porque en ese documento
        /// hay dos momentos de firma del jefe y conviene que se lea cuál es cuál.
        /// </summary>
        private static string BloqueFirmas(List<EntFirmaSolicitud> firmas, bool hayRecuperacion)
        {
            string[] roles = { "COLABORADOR", "JEFE", "GTH" };
            string[] rotulos =
            {
                hayRecuperacion ? "SOLICITUD COLABORADOR" : "FIRMA COLABORADOR",
                "APROBACIÓN JEFE INMEDIATO",
                "VALIDACIÓN TALENTO HUMANO"
            };

            StringBuilder h = new StringBuilder();

            h.Append("<div style='border-top:1px solid ").Append(Regla).Append("; margin-top:26px'></div>");
            h.Append("<div style='margin-top:10px; color:").Append(Gris)
             .Append("; font-size:10px; font-weight:bold; letter-spacing:1px'>FIRMAS</div>");

            h.Append("<table width='100%' cellpadding='0' cellspacing='0' style='margin-top:8px'><tr>");
            for (int i = 0; i < roles.Length; i++)
            {
                h.Append("<td width='33%' valign='top' style='padding-right:12px'>");
                Recuadro(h, rotulos[i], Buscar(firmas, roles[i], 1));
                h.Append("</td>");
            }
            h.Append("</tr></table>");

            if (hayRecuperacion)
            {
                h.Append("<table width='100%' cellpadding='0' cellspacing='0' style='margin-top:22px'><tr>");
                h.Append("<td width='33%' valign='top' style='padding-right:12px'>");
                Recuadro(h, "CIERRE JEFE INMEDIATO", Buscar(firmas, "JEFE", 2));
                h.Append("</td><td></td><td></td>");
                h.Append("</tr></table>");
            }

            return h.ToString();
        }

        /// <summary>Un espacio de firma: rótulo, trazo y quién firmó.</summary>
        private static void Recuadro(StringBuilder h, string rotulo, EntFirmaSolicitud firma)
        {
            h.Append("<div style='color:").Append(Azul)
             .Append("; font-size:10px; font-weight:bold; letter-spacing:0.4px'>")
             .Append(Escapar(rotulo)).Append("</div>");

            /* El trazo se incrusta tal cual, no se regenera. */
            h.Append("<div style='height:62px; padding-top:4px'>");
            if (firma != null && !string.IsNullOrEmpty(firma.RutaTrazo))
            {
                h.Append("<img src='").Append(RutaComoUri(firma.RutaTrazo))
                 .Append("' style='max-height:58px; max-width:96%' />");
            }
            h.Append("</div>");

            h.Append("<div style='border-top:1px solid ").Append(firma != null ? "#555555" : Punteado).Append("'></div>");

            if (firma == null)
            {
                h.Append("<div style='padding-top:4px; color:#9A9A9A; font-size:10px'>Pendiente</div>");
                return;
            }

            h.Append("<div style='padding-top:4px; font-weight:bold; font-size:11px'>")
             .Append(Escapar(firma.Nombre)).Append("</div>");

            if (!string.IsNullOrEmpty(firma.Cargo))
            {
                h.Append("<div style='color:#555555; font-size:10px'>").Append(Escapar(firma.Cargo)).Append("</div>");
            }
            if (!string.IsNullOrEmpty(firma.Cedula))
            {
                h.Append("<div style='color:").Append(Gris).Append("; font-size:10px'>C.I. ")
                 .Append(Escapar(firma.Cedula)).Append("</div>");
            }

            h.Append("<div style='color:").Append(Gris).Append("; font-size:10px; margin-top:2px'>")
             .Append(firma.FechaFirma.ToString("dd/MM/yyyy HH:mm")).Append("</div>");

            if (firma.Decision == "RECHAZADO")
            {
                h.Append("<div style='color:").Append(Rojo)
                 .Append("; font-weight:bold; font-size:10px; margin-top:3px'>RECHAZADO</div>");
            }
            if (!string.IsNullOrEmpty(firma.Comentario))
            {
                h.Append("<div style='color:#555555; font-size:10px; margin-top:3px; font-style:italic'>")
                 .Append(Escapar(firma.Comentario)).Append("</div>");
            }
        }

        private static EntFirmaSolicitud Buscar(List<EntFirmaSolicitud> firmas, string rol, int secuencia)
        {
            if (firmas == null) { return null; }
            foreach (EntFirmaSolicitud f in firmas)
            {
                if (f.Rol == rol && f.Secuencia == secuencia) { return f; }
            }
            return null;
        }

        /// <summary>El título de una sección del detalle.</summary>
        private static void Seccion(StringBuilder h, string titulo)
        {
            h.Append("<div style='margin-top:24px; margin-bottom:2px; font-weight:bold; color:")
             .Append(Azul).Append("'>").Append(Escapar(titulo)).Append("</div>");
        }

        /// <summary>
        /// Un renglón del documento: rótulo en azul y valor sobre una línea de
        /// puntos que llega hasta el borde, como en el modelo.
        /// </summary>
        private static void Fila(StringBuilder h, string etiqueta, string valor)
        {
            h.Append("<tr><td style='padding:7px 0 4px; border-bottom:1px dotted ").Append(Punteado).Append("'>");
            h.Append("<span style='color:").Append(Azul).Append("; font-weight:bold'>")
             .Append(Escapar(etiqueta)).Append(":</span> ");
            h.Append("<span>").Append(Escapar(valor)).Append("</span>");
            h.Append("</td></tr>");
        }

        private static void FilaSiHay(StringBuilder h, string etiqueta, string valor)
        {
            if (string.IsNullOrEmpty(valor)) { return; }
            Fila(h, etiqueta, valor);
        }

        /// <summary>
        /// Cuánto queda por recuperar: el permiso menos lo que cubrió la bolsa
        /// mensual. Cadena vacía cuando no se puede afirmar.
        ///
        /// Quien pide 4 horas y usa las 3 de la bolsa recupera 1. Sin la bolsa de
        /// por medio, el excedente es el permiso entero.
        ///
        /// Lo registrado antes del 4 de septiembre de 2026 no guardó los minutos de
        /// la bolsa, solo el texto que se le mostró a la persona. Ahí el renglón se
        /// omite, como se venía haciendo: en un documento firmado es preferible que
        /// falte un dato a que aparezca uno inventado.
        /// </summary>
        private static string HorasARecuperar(EntSolicitud solicitud, EntDetallePermiso detalle)
        {
            if (!detalle.UsaPermisoMensual) { return solicitud.Horas ?? ""; }

            if (!detalle.SaldoMensualMinutos.HasValue) { return ""; }

            int permiso = Minutos(solicitud.Horas);
            if (permiso <= 0) { return ""; }

            /* Si la bolsa lo cubrió entero no hay nada que recuperar, y el renglón
               no tiene por qué salir diciendo cero. */
            int excedente = permiso - detalle.SaldoMensualMinutos.Value;
            if (excedente <= 0) { return ""; }

            return Duracion(excedente);
        }

        /// <summary>Los minutos de un "HH:mm". Cero si no se entiende.</summary>
        private static int Minutos(string texto)
        {
            if (string.IsNullOrEmpty(texto)) { return 0; }

            string[] partes = texto.Split(new char[] { (char)58 });
            if (partes.Length != 2) { return 0; }

            int horas, minutos;
            if (!int.TryParse(partes[0], out horas)) { return 0; }
            if (!int.TryParse(partes[1], out minutos)) { return 0; }

            return horas * 60 + minutos;
        }

        /// <summary>
        /// Unos minutos en palabras. Las horas justas se dicen como horas —"1 hora",
        /// "2 horas"— porque es como lo dice la gente; el resto va en el formato
        /// corto que ya usa el saldo mensual, "1h30".
        /// </summary>
        private static string Duracion(int minutos)
        {
            if (minutos <= 0) { return ""; }

            if (minutos % 60 == 0)
            {
                int horas = minutos / 60;
                return horas == 1 ? "1 hora" : horas.ToString() + " horas";
            }

            if (minutos < 60) { return minutos.ToString() + " minutos"; }

            return (minutos / 60).ToString() + "h" + (minutos % 60).ToString("00");
        }

        private static string Dias(double valor)
        {
            return valor == 1 ? "1 día" : valor.ToString("0.##") + " días";
        }

        /// <summary>
        /// La parte de fecha de un texto que puede traer fecha y hora. Si no se
        /// entiende se devuelve tal cual: en un documento es preferible el dato
        /// crudo a un renglón vacío.
        /// </summary>
        private static string SoloFecha(string texto)
        {
            DateTime d;
            if (DateTime.TryParse(texto, out d)) { return d.ToString("dd/MM/yyyy"); }
            return texto ?? "";
        }

        /// <summary>
        /// Fecha y hora de un permiso, como "18/09/2026 15:30".
        ///
        /// Aca se usaba SoloHora, que devolvia "15:30" y descartaba el dia. El
        /// procedimiento Sp_RTANotificarSolicitud ya entrega "dd/MM/yyyy HH:mm:ss"
        /// para los permisos -la version que devolvia solo la hora quedo comentada
        /// dentro del propio procedimiento-, asi que el dia SI estaba llegando y el
        /// documento lo tiraba. El resultado era que quien leia el PDF veia la hora
        /// del permiso y, como unica fecha, la de creacion de la solicitud en la
        /// cabecera: no habia forma de saber que dia se ausentaba la persona.
        ///
        /// Sin hora devuelve solo la fecha, no cadena vacia: son los permisos de
        /// jornada completa, y ahi el dia es justamente lo unico que hay que decir.
        ///
        /// El 01/01/1900 es el centinela que escribe DaoSolicitud cuando la fecha
        /// viene vacia; se trata como ausencia para que no salga un renglon con una
        /// fecha inventada.
        /// </summary>
        private static string FechaYHora(string texto)
        {
            DateTime d;
            if (!DateTime.TryParse(texto, out d)) { return texto ?? ""; }
            if (d.Year <= 1900) { return ""; }

            return (d.Hour == 0 && d.Minute == 0)
                ? d.ToString("dd/MM/yyyy")
                : d.ToString("dd/MM/yyyy HH:mm");
        }

        /// <summary>Día siguiente a una fecha dd/MM/yyyy. Cadena vacía si no se entiende.</summary>
        private static string DiaSiguiente(string fecha)
        {
            DateTime d;
            if (!DateTime.TryParseExact(fecha, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out d))
            {
                return "";
            }
            return d.AddDays(1).ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Ruta de disco como URI de archivo. wkhtmltopdf lee las imágenes del
        /// sistema de archivos; una ruta de Windows sin este prefijo la ignora en
        /// silencio y el recuadro sale vacío.
        ///
        /// Una dirección web se devuelve tal cual. El mismo documento se usa como
        /// cuerpo del correo, y ahí file:/// no sirve: apunta al disco del servidor,
        /// que quien recibe el mensaje no tiene.
        /// </summary>
        private static string RutaComoUri(string ruta)
        {
            if (ruta.StartsWith("http://") || ruta.StartsWith("https://")) { return ruta; }

            return "file:///" + ruta.Replace("\\", "/");
        }

        private static string Escapar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) { return ""; }
            return texto.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
