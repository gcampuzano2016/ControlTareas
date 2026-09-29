using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Guardar las decisiones de Nomina sobre los tramos de UNA persona, desde la
    /// ventana de detalle. Orquesta: carga, valida, arma el lote, lo manda a la base
    /// y deja el periodo recalculado.
    ///
    /// Aparte de NegHeDecisiones por la misma razon que NegHorasExtrasPantalla vive
    /// aparte de NegHorasExtras: aquella es pura y se prueba sin base; esta habla con
    /// el DAO.
    ///
    /// NO comprueba perfiles: eso es de la capa web, que es la unica que conoce la
    /// sesion (solo 14 y 18 deciden).
    /// </summary>
    public static class NegHeDecisionesPantalla
    {
        public const string TextoPeriodoCerrado = "El período está cerrado: no admite decisiones.";

        public static EntRespuesta Guardar(int idPeriodo, long idEmpleado, IList<EntHeCambioDecision> cambios,
                                           string usuario, string ip)
        {
            EntHePeriodo periodo = DaoHorasExtras.CargarPeriodo(idPeriodo).Periodo;
            if (periodo == null) { return Rechazo("Ese período no existe."); }

            /* El procedimiento tambien lo rechaza (-1), pero decirlo antes ahorra
               leer los candidatos para nada, y el texto es el mismo. */
            if (!periodo.EstaAbierto) { return Rechazo(TextoPeriodoCerrado); }

            /* Se valida contra los candidatos resueltos AHORA, no contra lo que tenia
               la pantalla: si otro periodo incluyo el segmento mientras la ventana
               estaba abierta, el bloqueo tiene que verse aqui. */
            List<EntHeCandidato> candidatos;
            List<EntHeDecision> decisiones;
            List<EntHeRangoPeriodo> periodos;
            DaoHorasExtras.LeerCandidatos(idPeriodo, out candidatos, out decisiones, out periodos);
            NegHeDecisiones.ValidarLectura(idPeriodo, periodos);
            List<EntHeCandidato> resueltos = NegHeDecisiones.Resolver(idPeriodo, candidatos, decisiones, periodos);

            string error = NegHeDecisiones.Validar(resueltos, idEmpleado, cambios);
            if (error != null) { return Rechazo(error); }

            int codigo = DaoHorasExtras.GuardarDecisiones(idPeriodo, ArmarXml(cambios), usuario, ip);
            if (codigo != 0) { return Rechazo(MensajeDecisiones(codigo)); }

            /* Lo guardado YA esta en la base. Si el recalculo falla, la respuesta
               sigue diciendo "guardadas" con un aviso (lo arma CombinarTrasGuardar):
               si dijera error, Nomina reintentaria y duplicaria las decisiones.
               La vigencia es el inicio del periodo para que CombinarTrasGuardar no
               agregue el aviso de "el sueldo rige despues del fin", que aqui no
               aplica: no se toco ningun sueldo. */
            EntRespuesta refresco;
            try
            {
                refresco = NegHorasExtrasPantalla.AbrirPeriodo(periodo.FechaInicio, periodo.FechaFin, usuario, ip);
            }
            catch (Exception)
            {
                refresco = null;
            }

            return NegHeMaestroPantalla.CombinarTrasGuardar(MensajeDecisiones(0), periodo, periodo.FechaInicio, refresco);
        }

        /// <summary>
        /// El lote para Sp_RTA_HeGuardarDecisiones, un &lt;d&gt; por cambio dentro de
        /// &lt;ds&gt;. Con XElement y no concatenando texto: un motivo con &lt;, &amp;
        /// o comillas tiene que llegar escapado, y concatenar lo romperia.
        ///
        /// s va en el formato de EntHeCandidato.ClaveSegmento (estilo 126 del SQL) y
        /// con cultura invariante; m vacio significa el segmento completo.
        /// </summary>
        public static string ArmarXml(IList<EntHeCambioDecision> cambios)
        {
            XElement raiz = new XElement("ds");
            if (cambios != null)
            {
                foreach (EntHeCambioDecision x in cambios)
                {
                    if (x == null) { continue; }
                    raiz.Add(new XElement("d",
                        new XAttribute("t", x.IdRegDetTareas.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("s", x.SegmentoInicio.ToString(EntHeCandidato.FormatoClave, CultureInfo.InvariantCulture)),
                        new XAttribute("i", x.Incluir ? "1" : "0"),
                        new XAttribute("h", x.TipoHora.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("m", x.MinutosIncluidos.HasValue
                                            ? x.MinutosIncluidos.Value.ToString(CultureInfo.InvariantCulture) : ""),
                        new XAttribute("r", (x.Motivo ?? "").Trim())));
                }
            }
            return raiz.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Traduce la respuesta de Sp_RTA_HeGuardarDecisiones. Normalmente Validar ya
        /// rechazo antes con un texto mas preciso; estos mensajes cubren lo que solo
        /// la base puede ver (otro guardado que se adelanto, un cierre a mitad).
        /// </summary>
        public static string MensajeDecisiones(int codigo)
        {
            switch (codigo)
            {
                case 0: return "Decisiones guardadas.";
                case -1: return "El período ya no está abierto: no admite decisiones.";
                case -2: return "Algún tramo ya está incluido en otro período: incluirlo aquí sería pagarlo dos veces. "
                              + "Vuelva a abrir el detalle para ver cuál.";
                case -3: return "Hay datos inválidos en las decisiones (tipo, minutos o motivo). Revise y vuelva a guardar.";
                case -4: return "Algún tramo ya no es candidato de este período. Vuelva a abrir el detalle.";
                default: return "No se pudieron guardar las decisiones.";
            }
        }

        private static EntRespuesta Rechazo(string mensaje)
        {
            EntRespuesta r = new EntRespuesta();
            r.estado = "0";
            r.mensaje = mensaje;
            r.tipoMensaje = "warning";
            return r;
        }
    }
}
