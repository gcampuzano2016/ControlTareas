using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDF
{
    /// <summary>
    /// Arma el HTML de una solicitud aprobada, tal como lo quiere ver Talento
    /// Humano en el PDF descargable (CB-GAP-POL-01, seccion 08).
    ///
    /// Dos restricciones que explican por que el marcado se ve anticuado:
    ///
    /// 1. La conversion la hace Pechkin 0.5.8, que envuelve una version vieja de
    ///    wkhtmltopdf. No entiende flexbox ni grid, asi que la maquetacion va con
    ///    tablas y estilos en linea. Cambiarlo a CSS moderno rompe el PDF sin dar
    ///    ningun error: sale la pagina desarmada.
    ///
    /// 2. Las imagenes van por ruta de archivo, no como data URI. Es el mismo
    ///    camino que ya usa GenerarCodigoQR, que devuelve una ruta justamente
    ///    porque esa version no resuelve base64 embebido de forma confiable.
    /// </summary>
    public class HtmlSolicitud
    {
        private const string Empresa = "COMPUEQUIP DOS S.A.";

        /// <summary>
        /// El HTML completo del documento.
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud.</param>
        /// <param name="firmas">Firmas ya registradas, con RutaTrazo resuelta.</param>
        /// <param name="folio">Folio y codigo de verificacion.</param>
        /// <param name="rutaLogo">Ruta en disco del logo. Si esta vacia, se omite.</param>
        /// <param name="detalle">Detalle del permiso. Puede venir null en vacaciones.</param>
        public static string Construir(EntSolicitud solicitud, List<EntFirmaSolicitud> firmas,
                                       string folio, string rutaLogo, EntDetallePermiso detalle)
        {
            bool esVacaciones = solicitud.IdTipoSolicitud != 1;
            string titulo = esVacaciones ? "Solicitud de vacaciones" : "Convenio de permisos en horas laborales";
            string tipo = esVacaciones ? "Vacaciones" : "Permiso";

            StringBuilder h = new StringBuilder();

            h.Append("<!DOCTYPE html><html><head><meta charset='utf-8' /></head>");
            h.Append("<body style=\"font-family:Arial,Helvetica,sans-serif; font-size:12px; color:#222; margin:0\">");

            /* Cabecera: datos de la empresa a la izquierda, logo a la derecha. */
            h.Append("<table width='100%' cellpadding='0' cellspacing='0' style='border-bottom:2px solid #222; padding-bottom:8px'><tr>");
            h.Append("<td valign='top'>");
            h.Append("<div style='font-size:15px; font-weight:bold'>").Append(Escapar(Empresa)).Append("</div>");
            h.Append("<div style='color:#666; margin-top:2px'>").Append(Escapar(folio)).Append(" &middot; ").Append(Escapar(tipo)).Append("</div>");
            h.Append("<div style='font-size:16px; font-weight:bold; margin-top:6px'>").Append(Escapar(titulo)).Append("</div>");
            h.Append("</td>");
            if (!string.IsNullOrEmpty(rutaLogo))
            {
                h.Append("<td valign='top' align='right' width='150'>");
                h.Append("<img src='").Append(RutaComoUri(rutaLogo)).Append("' style='max-height:52px' />");
                h.Append("</td>");
            }
            h.Append("</tr></table>");

            /* Cuerpo: pares etiqueta / valor. */
            h.Append("<table width='100%' cellpadding='5' cellspacing='0' style='margin-top:14px; border-collapse:collapse'>");
            Fila(h, "Colaborador", solicitud.Colaborador);
            Fila(h, "Cédula", solicitud.Cedula);
            Fila(h, "Departamento", solicitud.Departamento);
            Fila(h, esVacaciones ? "Jefe inmediato" : "Jefe de Área", solicitud.JefeInmediato);

            if (esVacaciones)
            {
                Fila(h, "Reemplazo", string.IsNullOrEmpty(solicitud.Remplazo) ? "—" : solicitud.Remplazo);
                Fila(h, "Fechas", solicitud.FechaDesde + " – " + solicitud.FechaHasta);
                Fila(h, "Días tomados", Dias(solicitud.TotalDias));
                if (solicitud.Feriado > 0)
                {
                    /* Se dice explicitamente que los feriados no se cobraron: es la
                       explicacion de por que los dias no cuadran con las fechas. */
                    Fila(h, "Feriados no descontados", Dias(solicitud.Feriado));
                }
                Fila(h, "Saldo restante", Dias(solicitud.SaldoDias));
                Fila(h, "Regresa a trabajar", DiaSiguiente(solicitud.FechaHasta));
            }
            else
            {
                FilasDelPermiso(h, solicitud, detalle);
            }

            if (!string.IsNullOrEmpty(solicitud.Observacion))
            {
                Fila(h, "Observaciones", solicitud.Observacion);
            }
            h.Append("</table>");

            h.Append(BloqueFirmas(firmas));

            h.Append("<div style='margin-top:26px; padding-top:8px; border-top:1px solid #ccc; color:#666; font-size:10px'>");
            h.Append("Documento generado automáticamente &middot; código de verificación: ").Append(Escapar(folio));
            h.Append("</div>");

            h.Append("</body></html>");
            return h.ToString();
        }

        /// <summary>
        /// Las filas propias de un permiso.
        ///
        /// Cada una aparece solo si tiene valor: un permiso personal de dos horas
        /// no tiene por qué salir con ocho campos vacíos de teletrabajo. Por eso
        /// hay tantos condicionales en vez de una lista fija.
        ///
        /// El detalle puede venir null para lo registrado antes de agosto de 2026:
        /// esas solicitudes no tienen tipo de permiso y solo llevan la actividad
        /// en texto libre, que es todo lo que se guardó en su momento.
        /// </summary>
        private static void FilasDelPermiso(StringBuilder h, EntSolicitud solicitud, EntDetallePermiso detalle)
        {
            bool hayDetalle = detalle != null && !string.IsNullOrEmpty(detalle.TipoPermiso);

            if (hayDetalle)
            {
                Fila(h, "Tipo de Permiso", RotuloTipo(detalle.TipoPermiso));

                /* La actividad en texto libre solo aporta cuando es "Otro": para
                   los demás tipos repetiría la etiqueta de arriba. */
                if (detalle.TipoPermiso == "OTRO" && !string.IsNullOrEmpty(solicitud.Actividad))
                {
                    Fila(h, "Actividad", solicitud.Actividad);
                }
            }
            else
            {
                Fila(h, "Actividad", solicitud.Actividad);
            }

            Fila(h, "Fecha", solicitud.FechaDesde);
            Fila(h, "Horas", solicitud.Horas);

            if (detalle == null) { return; }

            if (detalle.EsTeletrabajo)
            {
                string modalidad = detalle.Modalidad == "HORAS"
                    ? "Por horas · " + detalle.HoraDesde + " – " + detalle.HoraHasta + " (luego presencial)"
                    : "Jornada completa";

                Fila(h, "Modalidad", modalidad);
                FilaSiHay(h, "Lugar", detalle.Lugar);
                FilaSiHay(h, "Medios de contacto", detalle.MediosContacto);
                FilaSiHay(h, "Motivo general", detalle.MotivoGeneral);
                FilaSiHay(h, "Actividades a ejecutar", detalle.Actividades);
                FilaSiHay(h, "Entregables esperados", detalle.Entregables);
            }

            if (detalle.UsaPermisoMensual)
            {
                Fila(h, "¿Usa permiso mensual de 3h?", "Sí");
                /* El texto congelado al pedir el permiso, no el saldo de hoy: la
                   especificación pide constancia de cuánto quedaba en ese momento. */
                FilaSiHay(h, "Saldo del permiso mensual", detalle.SaldoMensualTexto);
            }

            FilaSiHay(h, "Tratamiento del excedente", RotuloExcedente(detalle.TratamientoExcedente));

            if (detalle.TieneRecuperacion)
            {
                FilaSiHay(h, "Recuperación propuesta", detalle.RecFechaPropuesta);
                FilaSiHay(h, "Horario de recuperación", detalle.RecHorario);
                FilaSiHay(h, "Actividades de recuperación", detalle.RecActividades);
                FilaSiHay(h, "Entregables de recuperación", detalle.RecEntregables);
                FilaSiHay(h, "Fecha máxima de cierre", detalle.RecFechaMaxima);
            }
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

        private static void FilaSiHay(StringBuilder h, string etiqueta, string valor)
        {
            if (string.IsNullOrEmpty(valor)) { return; }
            Fila(h, etiqueta, valor);
        }

        /// <summary>Los tres recuadros de firma, en el orden del flujo.</summary>
        private static string BloqueFirmas(List<EntFirmaSolicitud> firmas)
        {
            string[] roles = { "COLABORADOR", "JEFE", "GTH" };
            string[] rotulos = { "Firma colaborador", "Aprobación jefe inmediato", "Validación Talento Humano" };

            StringBuilder h = new StringBuilder();
            h.Append("<table width='100%' cellpadding='8' cellspacing='0' style='margin-top:28px; border-collapse:collapse'><tr>");

            for (int i = 0; i < roles.Length; i++)
            {
                EntFirmaSolicitud firma = Buscar(firmas, roles[i]);

                h.Append("<td width='33%' valign='top' align='center' style='border:1px solid #ccc'>");
                h.Append("<div style='color:#666; font-size:10px; text-transform:uppercase'>").Append(Escapar(rotulos[i])).Append("</div>");

                /* El trazo se incrusta tal cual, no se regenera. */
                if (firma != null && !string.IsNullOrEmpty(firma.RutaTrazo))
                {
                    h.Append("<div style='height:70px; padding:4px 0'>");
                    h.Append("<img src='").Append(RutaComoUri(firma.RutaTrazo)).Append("' style='max-height:66px; max-width:95%' />");
                    h.Append("</div>");
                }
                else
                {
                    h.Append("<div style='height:70px'></div>");
                }

                if (firma != null)
                {
                    h.Append("<div style='border-top:1px solid #222; padding-top:4px; font-weight:bold'>").Append(Escapar(firma.Nombre)).Append("</div>");
                    if (!string.IsNullOrEmpty(firma.Cargo))
                    {
                        h.Append("<div style='color:#444'>").Append(Escapar(firma.Cargo)).Append("</div>");
                    }
                    if (!string.IsNullOrEmpty(firma.Cedula))
                    {
                        h.Append("<div style='color:#666; font-size:10px'>C.I. ").Append(Escapar(firma.Cedula)).Append("</div>");
                    }
                    h.Append("<div style='color:#666; font-size:10px; margin-top:3px'>")
                     .Append(firma.FechaFirma.ToString("dd/MM/yyyy HH:mm")).Append("</div>");

                    if (firma.Decision == "RECHAZADO")
                    {
                        h.Append("<div style='color:#a33a32; font-weight:bold; font-size:10px; margin-top:3px'>RECHAZADO</div>");
                    }
                    if (!string.IsNullOrEmpty(firma.Comentario))
                    {
                        h.Append("<div style='color:#444; font-size:10px; margin-top:3px; font-style:italic'>")
                         .Append(Escapar(firma.Comentario)).Append("</div>");
                    }
                }
                else
                {
                    h.Append("<div style='border-top:1px solid #ccc; padding-top:4px; color:#999'>Pendiente</div>");
                }

                h.Append("</td>");
            }

            h.Append("</tr></table>");
            return h.ToString();
        }

        private static EntFirmaSolicitud Buscar(List<EntFirmaSolicitud> firmas, string rol)
        {
            if (firmas == null) { return null; }
            foreach (EntFirmaSolicitud f in firmas)
            {
                /* Secuencia 1: la reconfirmacion del jefe se muestra aparte cuando
                   llegue Permisos con recuperacion. */
                if (f.Rol == rol && f.Secuencia == 1) { return f; }
            }
            return null;
        }

        private static void Fila(StringBuilder h, string etiqueta, string valor)
        {
            h.Append("<tr>");
            h.Append("<td width='190' style='border-bottom:1px solid #eee; color:#666'>").Append(Escapar(etiqueta)).Append("</td>");
            h.Append("<td style='border-bottom:1px solid #eee; font-weight:bold'>").Append(Escapar(valor)).Append("</td>");
            h.Append("</tr>");
        }

        private static string Dias(double valor)
        {
            return valor == 1 ? "1 día" : valor.ToString("0.##") + " días";
        }

        /// <summary>Dia siguiente a una fecha dd/MM/yyyy. Cadena vacia si no se entiende.</summary>
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
        /// Ruta de disco como URI de archivo. wkhtmltopdf lee las imagenes del
        /// sistema de archivos; una ruta de Windows sin este prefijo la ignora en
        /// silencio y el recuadro sale vacio.
        /// </summary>
        private static string RutaComoUri(string ruta)
        {
            return "file:///" + ruta.Replace("\\", "/");
        }

        private static string Escapar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) { return ""; }
            return texto.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
