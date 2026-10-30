using CapaDato;
using CapaEntidad;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CapaNegocio
{
    /// <summary>
    /// Aviso de compra de vacaciones para Nomina, en PDF (decision 7 de GTH,
    /// 2026-10-30): Nomina tiene que pagar los dias que la persona vendio.
    ///
    /// El valor es referencial, con la misma formula que la pantalla de nomina
    /// (dias x sueldo / DIAS_MES) y el sueldo vigente a la fecha de la compra.
    /// Lo confirma Nomina; el PDF lo dice.
    /// </summary>
    public static class NegVacCompra
    {
        /// <summary>Null si la persona no existe o la compra no es suya.</summary>
        public static EntVacCompraAviso Aviso(int idColaborador, int idMovimiento, DateTime hoy, string usuario)
        {
            EntVacAjustes a = NegVacAjustes.Cargar(idColaborador, hoy);
            if (a == null || a.Ficha.Saldo == null) { return null; }
            return Armar(a, idMovimiento, DaoVacSaldos.Sueldos(), DateTime.Now, usuario);
        }

        /// <summary>Sin base: lo que Aviso hace despues de leer. Null si la compra no esta.</summary>
        public static EntVacCompraAviso Armar(EntVacAjustes a, int idMovimiento, EntVacSueldos sueldos, DateTime emitido, string usuario)
        {
            EntVacMovimiento compra = a.Ficha.Movimientos.FirstOrDefault(m => m.IdMovimiento == idMovimiento && m.Tipo == NegVacAjustes.TipoCompra);
            if (compra == null) { return null; }

            EntVacCompraDocumento documento;
            a.Documentos.TryGetValue(idMovimiento, out documento);

            EntVacCompraAviso r = new EntVacCompraAviso
            {
                Colaborador = a.Ficha.Colaborador,
                Compra = compra,
                Saldo = a.Ficha.Saldo,
                Registro = a.Bitacora.FirstOrDefault(b => b.Tipo == "COMPRA" && b.Accion == "REGISTRA" && b.IdMovimiento == idMovimiento),
                Documento = documento,
                DiasMes = sueldos.DiasMes,
                Emitido = emitido,
                Usuario = usuario ?? ""
            };

            string fuente;
            decimal sueldo = NegVacNomina.SueldoVigente(r.Colaborador.IdColaborador, compra.Desde ?? emitido.Date, sueldos, out fuente);
            if (sueldo > 0m)
            {
                r.Sueldo = sueldo;
                r.FuenteSueldo = fuente;
                r.Valor = NegVacNomina.Valor(compra.Dias ?? 0m, sueldo, sueldos.DiasMes);
            }
            return r;
        }

        /// <summary>"" si se puede emitir; si no, por que no.</summary>
        public static string PuedeEmitir(EntVacCompraAviso a)
        {
            if (a == null) { return "La compra no existe."; }
            if (a.Compra.Estado == "DADA BAJA") { return "La compra está anulada: no corresponde avisar a Nómina."; }
            return "";
        }

        /// <summary>
        /// Hoja A4, mismo membrete y mismas reglas de HtmlRenderer que el estado
        /// individual (NegVacEstado.Html): un texto simple por celda.
        /// </summary>
        public static string Html(EntVacCompraAviso a, string membreteDos, string logoAgility)
        {
            if (a == null) { return ""; }
            EntVacColaborador c = a.Colaborador;
            EntVacMovimiento m = a.Compra;
            decimal dias = m.Dias ?? 0m;
            bool agility = NegCertificadoLaboral.Empresa(c.Empresa) == NegCertificadoLaboral.EmpresaAgility;

            StringBuilder h = new StringBuilder();
            h.Append("<html><head><meta charset=\"utf-8\" /></head>");
            h.Append("<body style=\"margin:0; font-family: Arial, Helvetica, sans-serif; font-size:9pt; color:#000\">");

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

            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-top:6px\"><tr>")
             .Append("<td style=\"font-size:14pt; font-weight:bold\">COMPRA DE VACACIONES</td>")
             .Append("<td style=\"text-align:right; padding-right:6px; font-size:10pt\">Registro N.º ").Append(m.IdMovimiento.ToString("000000", CultureInfo.InvariantCulture)).Append("</td>")
             .Append("</tr><tr><td colspan=\"2\" style=\"padding-bottom:8px\">Aviso a Nómina para el pago · ")
             .Append(NegCertificadoLaboral.FechaLarga(a.Emitido)).Append("</td></tr></table>");

            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-bottom:8px\">");
            NegVacEstado.Par(h, "Colaborador", c.Nombre, "Cédula", c.Cedula);
            NegVacEstado.Par(h, "Empresa", c.Empresa, "Área", c.Area);
            NegVacEstado.Par(h, "Cargo", c.Cargo, "Fecha de ingreso", NegVacEstado.Fecha(c.FechaIngreso));
            h.Append("</table>");

            /* Casillas */
            string[] titulos = { "Días comprados", "Sueldo mensual", "Valor por día", "VALOR A PAGAR (referencial)" };
            string[] valores =
            {
                NegVacEstado.Num(dias, 2),
                a.Sueldo.HasValue ? Dinero(a.Sueldo.Value) : "—",
                a.Sueldo.HasValue ? Dinero(Math.Round(a.Sueldo.Value / a.DiasMes, 2, MidpointRounding.AwayFromZero)) : "—",
                a.Valor.HasValue ? Dinero(a.Valor.Value) : "—"
            };
            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-bottom:6px\"><tr>");
            for (int i = 0; i < titulos.Length; i++)
            {
                h.Append("<td style=\"width:138px; border:1px solid #999; border-bottom:0; padding:4px 4px 0 4px; text-align:center; vertical-align:top; font-size:7.5pt")
                 .Append(i == 3 ? "; background:#e8eef7" : "").Append("\">").Append(NegVacEstado.Escapar(titulos[i])).Append("</td>");
            }
            h.Append("</tr><tr>");
            for (int i = 0; i < valores.Length; i++)
            {
                h.Append("<td style=\"width:138px; border:1px solid #999; border-top:0; padding:2px 4px 4px 4px; text-align:center; font-size:13pt; font-weight:bold")
                 .Append(i == 3 ? "; background:#e8eef7" : "").Append("\">").Append(NegVacEstado.Escapar(valores[i])).Append("</td>");
            }
            h.Append("</tr></table>");

            decimal despues = a.Saldo.Saldo;
            decimal antes = m.Situacion == "DESCUENTA" ? despues + m.DiasDescuenta : despues;
            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-bottom:8px\">");
            NegVacEstado.Par(h, "Fecha de la compra", m.Desde.HasValue ? NegVacEstado.Fecha(m.Desde.Value) : "", "Saldo al " + NegVacEstado.Fecha(a.Saldo.FechaCalculo), Dias(despues) + " (ya descontada)");
            NegVacEstado.Par(h, "Saldo sin la compra", Dias(antes), "Documento firmado", a.Documento != null ? a.Documento.NombreArchivo : "Sin documento adjunto");
            if (a.Registro != null)
            {
                NegVacEstado.Par(h, "Registrada por", a.Registro.Usuario, "Registrada el", a.Registro.Fecha.ToString("dd'/'MM'/'yyyy HH:mm", CultureInfo.InvariantCulture));
            }
            h.Append("</table>");

            h.Append("<div style=\"font-weight:bold; margin-bottom:2px\">Motivo</div>");
            h.Append("<div style=\"border:1px solid #999; background:#f5f5f5; padding:6px; margin-bottom:6px; text-align:justify; line-height:1.35\">")
             .Append(NegVacEstado.Escapar(m.Observacion)).Append("</div>");

            string nota = a.Valor.HasValue
                ? "El valor es referencial: días comprados × sueldo mensual ÷ " + NegVacEstado.Num(a.DiasMes, 0) +
                  ", con el sueldo vigente a la fecha de la compra. Nómina confirma el valor a pagar."
                : "No hay sueldo registrado para esta persona en el sistema: Nómina debe calcular el valor a pagar.";
            h.Append("<div style=\"font-size:8pt; color:#444; text-align:justify\">").Append(NegVacEstado.Escapar(nota)).Append("</div>");

            h.Append("<table style=\"width:555px; border-collapse:collapse; margin-top:60px; margin-bottom:18px\">")
             .Append("<tr><td style=\"width:30%; border-top:1px solid #000; padding-left:3px\">Talento Humano y Cultura</td><td style=\"width:5%\"></td>")
             .Append("<td style=\"width:30%; border-top:1px solid #000; padding-left:3px\">Nómina (recibido)</td><td style=\"width:5%\"></td>")
             .Append("<td style=\"width:30%; border-top:1px solid #000; padding-left:3px\">Colaborador</td></tr>")
             .Append("<tr><td></td><td></td><td></td><td></td><td style=\"font-weight:bold; padding-left:3px\">").Append(NegVacEstado.Escapar(c.Nombre)).Append("</td></tr>")
             .Append("<tr><td></td><td></td><td></td><td></td><td style=\"padding-left:3px\">C.I. ").Append(NegVacEstado.Escapar(c.Cedula)).Append("</td></tr>")
             .Append("</table>");

            h.Append("<div style=\"font-size:7.5pt; color:#666; margin-top:10px\">Registro ").Append(m.IdMovimiento.ToString("000000", CultureInfo.InvariantCulture))
             .Append(" · emitido el ").Append(NegVacEstado.Fecha(a.Emitido)).Append(" a las ").Append(a.Emitido.ToString("HH:mm", CultureInfo.InvariantCulture))
             .Append(" por ").Append(NegVacEstado.Escapar(a.Usuario)).Append(" · ReporteTareas</div>");

            h.Append("</body></html>");
            return h.ToString();
        }

        /// <summary>"$1.250,00", como la pantalla de nomina.</summary>
        public static string Dinero(decimal monto)
        {
            return "$" + NegVacEstado.Num(monto, 2);
        }

        private static string Dias(decimal d)
        {
            return NegVacEstado.Num(d, 2) + (d == 1m ? " día" : " días");
        }
    }
}
