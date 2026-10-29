using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace CapaNegocio
{
    /// <summary>
    /// Estado individual de vacaciones para firmar: la hoja ESTADO INDIVIDUAL
    /// del Excel de GTH, en PDF.
    ///
    /// El HTML vive aqui -y no junto al PDF- por lo mismo que el certificado
    /// laboral: CapaPruebas solo ve CapaEntidad y CapaNegocio. Va con tablas y
    /// estilos en linea porque HtmlRenderer ignora lo que no entiende.
    ///
    /// Los numeros son los de Sp_RTA_VacCalcular; lo unico que se calcula aqui
    /// es a que periodo se carga cada solicitud, con la misma regla que la
    /// columna O de la hoja REGISTRO.
    /// </summary>
    public static class NegVacEstado
    {
        /* Coma decimal y punto de miles, como la pantalla. No se usa la cultura
           es-EC: en el servidor de Windows trae punto decimal. */
        private static readonly NumberFormatInfo Es = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };

        /// <summary>Genera el folio y deja registrado lo que se imprime.</summary>
        public static EntVacEstado Emitir(DateTime fechaCalculo, int idColaborador, string usuario, string ip)
        {
            EntVacFicha f = DaoVacSaldos.Ficha(fechaCalculo, idColaborador);
            if (f == null || f.Saldo == null) { return null; }

            int folio = DaoVacSaldos.RegistrarEstado(idColaborador, f.Saldo, usuario, ip);
            return new EntVacEstado { Folio = folio, Emitido = DateTime.Now, Usuario = usuario, Ficha = f };
        }

        // ------------------------------------------------------- reglas

        /// <summary>
        /// Periodo al que se carga cada solicitud que descuenta (REGISTRO!O):
        /// se suma lo descontado antes -mas el ajuste del saldo de corte- y se
        /// toma el ultimo periodo cuyo "inicio de absorcion" no lo supera. Ese
        /// inicio es lo consumido por los periodos anteriores
        /// (CALCULO_PERIODOS!S), no lo ganado: un periodo que vencio con saldo
        /// deja de absorber, y lo que sigue cae en el siguiente.
        /// Si ese periodo esta EN CURSO o es FUTURO, es un anticipo.
        /// Las que no descuentan no aparecen en el resultado.
        /// </summary>
        public static Dictionary<int, string> PeriodoDeCadaSolicitud(EntVacFicha f)
        {
            Dictionary<int, string> r = new Dictionary<int, string>();
            if (f == null || f.Periodos.Count == 0) { return r; }

            List<EntVacPeriodo> periodos = f.Periodos.OrderBy(p => p.N).ToList();
            decimal previo = f.Saldo == null ? 0m : f.Saldo.AjusteCorte;

            /* El motor ya las entrega en orden (desde, fila del Excel). */
            foreach (EntVacMovimiento m in f.Movimientos)
            {
                if (m.Situacion != "DESCUENTA" || m.DiasDescuenta == 0m) { continue; }

                decimal antes = 0m;
                EntVacPeriodo elegido = null;
                foreach (EntVacPeriodo p in periodos)
                {
                    if (Math.Round(previo, 6) < Math.Round(antes, 6)) { break; }
                    elegido = p;
                    antes += p.DiasTomados;
                }

                r[m.IdMovimiento] = elegido == null
                    ? "REVISAR"
                    : elegido.Periodo + (elegido.Estado == "EN CURSO" || elegido.Estado == "FUTURO" ? " (anticipo)" : "");
                previo += m.DiasDescuenta;
            }
            return r;
        }

        /// <summary>"7 años 6 meses", en años y meses calendario cumplidos.</summary>
        public static string Antiguedad(DateTime ingreso, DateTime fecha)
        {
            if (fecha < ingreso) { return "0 años 0 meses"; }
            int meses = (fecha.Year - ingreso.Year) * 12 + fecha.Month - ingreso.Month;
            if (fecha.Day < ingreso.Day) { meses--; }
            int anios = meses / 12;
            return anios + (anios == 1 ? " año " : " años ") + (meses % 12) + (meses % 12 == 1 ? " mes" : " meses");
        }

        /// <summary>El parrafo que explica el saldo, en el tono de la hoja del Excel.</summary>
        public static string Mensaje(EntVacSaldo s)
        {
            if (s == null) { return ""; }

            StringBuilder t = new StringBuilder();
            t.Append("A la fecha tiene ").Append(Dias(s.Disponible)).Append(" disponibles de periodos ya cumplidos");
            if (s.UsadoEnCurso > 0m)
            {
                t.Append(", de los cuales ya usó ").Append(Dias(s.UsadoEnCurso)).Append(" a cuenta del periodo actual");
            }
            t.Append(". En el periodo en curso lleva acumulados ").Append(Dias(s.Proporcional)).Append(" proporcionales, ");
            if (s.Saldo < 0m)
            {
                t.Append("por lo que tiene un anticipo de ").Append(Dias(-s.Saldo))
                 .Append(", que se descontará de los días que vaya ganando.");
            }
            else
            {
                t.Append("por lo que su saldo a la fecha es de ").Append(Dias(s.Saldo)).Append(".");
            }
            if (s.PorVencer > 0m && s.ProximoVencimiento.HasValue)
            {
                t.Append(" Atención: ").Append(Dias(s.PorVencer)).Append(" vencen el ")
                 .Append(Fecha(s.ProximoVencimiento.Value)).Append("; conviene planificarlos antes de esa fecha.");
            }
            t.Append(" Cada periodo vence en la fecha indicada en el detalle; lo que no se goce hasta entonces se pierde.");
            return t.ToString();
        }

        // ------------------------------------------------------- html

        /// <summary>
        /// El estado completo. Las imagenes llegan como data URI (las mismas del
        /// certificado laboral); con cadena vacia se omiten. Hoja A4 con margen
        /// de 20 puntos: el contenido mide 555 de ancho y puede ocupar varias hojas.
        /// </summary>
        public static string Html(EntVacEstado e, string membreteDos, string logoAgility)
        {
            if (e == null || e.Ficha == null || e.Ficha.Saldo == null) { return ""; }

            EntVacFicha f = e.Ficha;
            EntVacColaborador c = f.Colaborador;
            EntVacSaldo s = f.Saldo;
            bool agility = NegCertificadoLaboral.Empresa(c.Empresa) == NegCertificadoLaboral.EmpresaAgility;

            StringBuilder h = new StringBuilder();
            h.Append("<html><head><meta charset=\"utf-8\" /></head>");
            h.Append("<body style=\"margin:0; font-family: Arial, Helvetica, sans-serif; font-size:9pt; color:#000\">");

            /* Cabecera */
            if (!agility && membreteDos != "")
            {
                h.Append("<img src=\"").Append(membreteDos).Append("\" style=\"width:555px; height:118px\" />");
            }
            else if (agility && logoAgility != "")
            {
                h.Append("<table style=\"width:555px; border-collapse:collapse\"><tr><td style=\"text-align:right\">")
                 .Append("<img src=\"").Append(logoAgility).Append("\" style=\"width:60px; height:66px\" />")
                 .Append("</td></tr></table>");
            }

            /* Regla de HtmlRenderer (version del sitio): dentro de una celda solo se
               dibuja bien UN texto simple. Un <b>, un segundo <div> o un <br /> en la
               misma celda se pierden sin aviso; por eso la negrita va en el estilo de
               la celda y cada linea es su propia fila. Y una celda en negrita alineada
               a la derecha pierde el ultimo caracter: el folio va sin negrita. */
            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-top:6px\"><tr>")
             .Append("<td style=\"font-size:14pt; font-weight:bold\">ESTADO DE VACACIONES</td>")
             .Append("<td style=\"text-align:right; padding-right:6px; font-size:10pt\">Folio N.º ").Append(e.Folio.ToString("000000", CultureInfo.InvariantCulture)).Append("</td>")
             .Append("</tr><tr><td colspan=\"2\" style=\"padding-bottom:8px\">Calculado al ").Append(NegCertificadoLaboral.FechaLarga(s.FechaCalculo))
             .Append(s.FechaCalculo.Date != f.FechaCalculo.Date ? " (fecha de salida)" : "")
             .Append("</td></tr></table>");

            /* Datos */
            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-bottom:8px\">");
            Par(h, "Colaborador", c.Nombre, "Cédula", c.Cedula);
            Par(h, "Empresa", c.Empresa, "Área", c.Area);
            Par(h, "Cargo", c.Cargo, "Estado", c.Estado + (c.FechaSalida.HasValue ? ", salió el " + Fecha(c.FechaSalida.Value) : ""));
            Par(h, "Fecha de ingreso", Fecha(c.FechaIngreso), "Antigüedad", Antiguedad(c.FechaIngreso, s.FechaCalculo));
            Par(h, "Días por año hoy", Num(s.DiasPorAnio, 0), "Próximo aniversario", Fecha(s.ProximoAniversario));
            if (c.FechaCorte.HasValue && c.SaldoCorte.HasValue)
            {
                Par(h, "Saldo de corte", Num(c.SaldoCorte.Value, 2) + " días al " + Fecha(c.FechaCorte.Value), "", "");
            }
            h.Append("</table>");

            /* Casillas */
            string[] titulos = { "Disponible (periodos cumplidos)", "Proporcional acumulado", "Usado del periodo en curso", "SALDO A LA FECHA", "Por vencer (próx. 90 días)" };
            decimal[] valores = { s.Disponible, s.Proporcional, s.UsadoEnCurso, s.Saldo, s.PorVencer };
            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-bottom:4px\"><tr>");
            for (int i = 0; i < titulos.Length; i++)
            {
                h.Append("<td style=\"width:111px; border:1px solid #999; border-bottom:0; padding:4px 4px 0 4px; text-align:center; vertical-align:top; font-size:7.5pt")
                 .Append(i == 3 ? "; background:#e8eef7" : "").Append("\">").Append(Escapar(titulos[i])).Append("</td>");
            }
            h.Append("</tr><tr>");
            for (int i = 0; i < valores.Length; i++)
            {
                h.Append("<td style=\"width:111px; border:1px solid #999; border-top:0; padding:2px 4px 4px 4px; text-align:center; font-size:13pt; font-weight:bold")
                 .Append(i == 3 ? "; background:#e8eef7" : "").Append("\">").Append(Escapar(Num(valores[i], 2))).Append("</td>");
            }
            h.Append("</tr></table>");
            if (s.ProximoVencimiento.HasValue)
            {
                h.Append("<div style=\"margin-bottom:6px\">Próximo vencimiento: ").Append(Fecha(s.ProximoVencimiento.Value)).Append("</div>");
            }

            h.Append("<div style=\"border:1px solid #999; background:#f5f5f5; padding:6px; margin-bottom:6px; text-align:justify; line-height:1.35\">")
             .Append(Escapar(Mensaje(s))).Append("</div>");

            /* Firmas: en la primera hoja, antes del detalle, para que la firma no
               quede en una hoja suelta cuando el detalle es largo. */
            h.Append("<div style=\"font-size:8pt; color:#444\">El detalle por periodo y de solicitudes que sigue forma parte de este estado.</div>");
            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-top:55px; margin-bottom:18px\">")
             .Append("<tr><td style=\"width:40%; border-top:1px solid #000; padding-left:3px\">Colaborador</td><td style=\"width:20%\"></td>")
             .Append("<td style=\"width:40%; border-top:1px solid #000\">Talento Humano y Cultura</td></tr>")
             .Append("<tr><td style=\"font-weight:bold; padding-left:3px\">").Append(Escapar(c.Nombre)).Append("</td><td></td><td></td></tr>")
             .Append("<tr><td style=\"padding-left:3px\">C.I. ").Append(Escapar(c.Cedula)).Append("</td><td></td><td></td></tr>")
             .Append("</table>");

            /* Periodos */
            h.Append("<div style=\"font-weight:bold; font-size:10pt; margin-bottom:3px\">DETALLE POR PERIODO</div>");
            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-bottom:4px\">");
            Cabecera(h, "Periodo", "Desde", "Hasta", "Días ganados", "Días tomados", "Días vencidos", "Saldo", "Vence el", "Estado");
            foreach (EntVacPeriodo p in f.Periodos.OrderBy(x => x.N))
            {
                Fila(h, new[] { p.Periodo, Fecha(p.Inicio), Fecha(p.Fin), Num(p.DiasGanados, 2), Num(p.DiasTomados, 2),
                                Num(p.DiasVencidos, 2), Num(p.Saldo, 2), Fecha(p.Vence), p.Estado },
                     new[] { false, false, false, true, true, true, true, false, false });
            }
            h.Append("</table>");
            h.Append("<div style=\"font-size:8pt; color:#444; margin-bottom:6px; text-align:justify\">Nota: los días se descuentan siempre del periodo más antiguo con saldo. ")
             .Append("Los periodos cuentan de aniversario a aniversario de la fecha de ingreso. Si existe un saldo real conciliado (fecha de corte), ")
             .Append("los periodos anteriores aparecen como CERRADO AL CORTE.</div>");

            /* Solicitudes */
            Dictionary<int, string> periodoDe = PeriodoDeCadaSolicitud(f);
            List<EntVacMovimiento> descuentan = f.Movimientos.Where(m => periodoDe.ContainsKey(m.IdMovimiento)).ToList();
            int otras = f.Movimientos.Count - descuentan.Count;

            h.Append("<div style=\"font-weight:bold; font-size:10pt; margin-bottom:3px\">DETALLE DE SOLICITUDES QUE DESCUENTAN</div>");
            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-bottom:4px\">");
            Cabecera(h, "Desde", "Hasta", "Tipo", "Estado", "Días", "Horas", "Días descontados", "Periodo", "Situación");
            if (descuentan.Count == 0)
            {
                h.Append("<tr><td colspan=\"9\" style=\"border:1px solid #999; padding:3px\">Sin solicitudes que descuenten.</td></tr>");
            }
            foreach (EntVacMovimiento m in descuentan)
            {
                bool horas = m.Tipo == "PERMISO POR HORAS";
                Fila(h, new[]
                {
                    m.Desde.HasValue ? Fecha(m.Desde.Value) : "",
                    m.Hasta.HasValue ? Fecha(m.Hasta.Value) : "",
                    m.Tipo + (m.BeneficioDos ? " (Beneficio DOS)" : ""),
                    m.Estado,
                    horas || !m.Dias.HasValue ? "" : Num(m.Dias.Value, 2),
                    horas ? Num(m.Horas ?? 0m, 0) + "h " + Num(m.Minutos ?? 0m, 0) + "m" : "",
                    Num(m.DiasDescuenta, 2),
                    periodoDe[m.IdMovimiento],
                    m.DiasFinDeSemana > 0m ? "DESCUENTA (+" + Num(m.DiasFinDeSemana, 0) + " fin de semana)" : "DESCUENTA"
                }, new[] { false, false, false, false, true, true, true, false, false });
            }
            h.Append("</table>");
            if (otras > 0)
            {
                h.Append("<div style=\"font-size:8pt; color:#444\">No se listan ").Append(otras)
                 .Append(otras == 1 ? " solicitud registrada que no descuenta" : " solicitudes registradas que no descuentan")
                 .Append(" (dadas de baja, sin fecha, o anteriores al saldo de corte o al reingreso).</div>");
            }

            h.Append("<div style=\"font-size:7.5pt; color:#666; margin-top:10px\">Folio ").Append(e.Folio.ToString("000000", CultureInfo.InvariantCulture))
             .Append(" · emitido el ").Append(Fecha(e.Emitido)).Append(" a las ").Append(e.Emitido.ToString("HH:mm", CultureInfo.InvariantCulture))
             .Append(" por ").Append(Escapar(e.Usuario)).Append(" · ReporteTareas</div>");

            h.Append("</body></html>");
            return h.ToString();
        }

        // ------------------------------------------------------- utilidades

        private static void Par(StringBuilder h, string e1, string v1, string e2, string v2)
        {
            h.Append("<tr>")
             .Append("<td style=\"width:105px; padding:2px 4px; font-weight:bold; vertical-align:top\">").Append(Escapar(e1)).Append("</td>")
             .Append("<td style=\"width:185px; padding:2px 4px; vertical-align:top\">").Append(Escapar(v1)).Append("</td>")
             .Append("<td style=\"width:105px; padding:2px 4px; font-weight:bold; vertical-align:top\">").Append(Escapar(e2)).Append("</td>")
             .Append("<td style=\"padding:2px 4px; vertical-align:top\">").Append(Escapar(v2)).Append("</td>")
             .Append("</tr>");
        }

        private static void Cabecera(StringBuilder h, params string[] titulos)
        {
            h.Append("<tr>");
            foreach (string t in titulos)
            {
                h.Append("<td style=\"border:1px solid #999; background:#e6e6e6; padding:2px 3px; font-size:7.5pt; font-weight:bold\">")
                 .Append(Escapar(t)).Append("</td>");
            }
            h.Append("</tr>");
        }

        private static void Fila(StringBuilder h, string[] valores, bool[] numericos)
        {
            h.Append("<tr>");
            for (int i = 0; i < valores.Length; i++)
            {
                h.Append("<td style=\"border:1px solid #999; padding:2px 3px; font-size:7.5pt")
                 .Append(numericos[i] ? "; text-align:right" : "").Append("\">")
                 .Append(Escapar(valores[i])).Append("</td>");
            }
            h.Append("</tr>");
        }

        private static string Dias(decimal d)
        {
            return Num(d, 2) + (d == 1m ? " día" : " días");
        }

        public static string Num(decimal d, int decimales)
        {
            return Math.Round(d, decimales, MidpointRounding.AwayFromZero).ToString("N" + decimales, Es);
        }

        private static string Fecha(DateTime d)
        {
            return d.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
        }

        private static string Escapar(string texto)
        {
            return WebUtility.HtmlEncode(texto ?? "");
        }
    }
}
