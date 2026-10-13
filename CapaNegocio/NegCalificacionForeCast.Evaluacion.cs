using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaNegocio
{
    public static partial class NegCalificacionForeCast
    {
        public const int AlertaSinActividad = 1;
        public const int AlertaCierreVencido = 2;
        public const int AlertaMontoSinDefinir = 3;
        public const int AlertaSinPropuesta = 4;
        public const int AlertaSinDecisor = 5;
        public const int AlertaCierreCambiado = 6;
        public const int AlertaCalificacionPendiente = 7;
        public const int AlertaInconsistente = 8;

        private const string FormatoFecha = "dd/MM/yyyy";

        /// <summary>
        /// Llena el resultado de UNA oportunidad. El orden importa: las alertas
        /// usan Autoridad y UltimaActividad, y la accion usa las alertas.
        /// </summary>
        public static void Evaluar(EntCalificacionForeCast o, IList<EntTasaExito> tasas, DateTime hoy)
        {
            if (o == null) { throw new ArgumentNullException(nameof(o)); }

            EntCalificacionRespuesta c = o.Calificacion;
            o.SinCalificar = c == null;
            o.Presupuesto = c == null ? 0 : c.Presupuesto;
            o.Autoridad = c == null ? 0 : c.Autoridad;
            o.Necesidad = c == null ? 0 : c.Necesidad;

            // o.FechaCierre nunca trae el centinela 1900-01-01: el SP y el DAO ya lo
            // convierten a NULL antes de llegar aqui, asi que no hace falta repetir
            // la regla en esta capa.
            o.Tiempo = PuntosTiempo(o.FechaCierre, hoy);
            o.UltimaActividad = UltimaActividad(o.FechaModificacion, c == null ? (DateTime?)null : c.FechaRegistro);
            o.Actividad = PuntosActividad(o.UltimaActividad, hoy);
            o.Encaje = PuntosEncaje(o.SegmentodeMercado, o.TipoEmpresa, o.GanadosCliente, tasas);

            o.Puntaje = o.Presupuesto + o.Autoridad + o.Necesidad + o.Tiempo + o.Actividad + o.Encaje;
            o.Estado = Clasificar(o.Puntaje);
            o.MontoPonderado = MontoPonderado(o.PVPEstimado, o.Porcentaje);

            o.Alertas = Alertas(o, hoy);
            o.Faltantes = Faltantes(o);
            o.AccionSugerida = Accion(o);

            o.FechaCierreTexto = Texto(o.FechaCierre);
            o.UltimaActividadTexto = Texto(o.UltimaActividad);
            o.FechaCalificacionTexto = c == null ? "" : Texto(c.FechaRegistro);
            o.FechaAccionTexto = c == null ? "" : Texto(c.FechaAccion);
        }

        public static bool TieneAlerta(EntCalificacionForeCast o, int codigo)
        {
            if (o == null || o.Alertas == null) { return false; }
            foreach (EntAlertaCalificacion a in o.Alertas)
            {
                if (a.Codigo == codigo) { return true; }
            }
            return false;
        }

        private static List<EntAlertaCalificacion> Alertas(EntCalificacionForeCast o, DateTime hoy)
        {
            List<EntAlertaCalificacion> r = new List<EntAlertaCalificacion>();
            bool avanzada = EsAvanzada(o.Prioridad, o.Porcentaje);

            if (!o.UltimaActividad.HasValue)
            {
                r.Add(Alerta(AlertaSinActividad, "Sin actividad registrada"));
            }
            else
            {
                int dias = (hoy.Date - o.UltimaActividad.Value.Date).Days;
                if (dias > 30) { r.Add(Alerta(AlertaSinActividad, "Sin actividad hace " + dias + " días")); }
            }

            if (o.FechaCierre.HasValue && o.FechaCierre.Value.Date < hoy.Date)
            {
                r.Add(Alerta(AlertaCierreVencido, "Fecha de cierre vencida (" + Texto(o.FechaCierre) + ")"));
            }

            if (avanzada && (!o.PVPEstimado.HasValue || o.PVPEstimado.Value == 0m))
            {
                r.Add(Alerta(AlertaMontoSinDefinir, "Monto en cero o sin definir en etapa avanzada"));
            }

            if (avanzada && (o.Calificacion == null || !o.Calificacion.PropuestaEnviada))
            {
                r.Add(Alerta(AlertaSinPropuesta, "Etapa avanzada sin propuesta enviada"));
            }

            if (o.ContactosCuenta <= 1 && o.Autoridad < 20)
            {
                r.Add(Alerta(AlertaSinDecisor, "Un solo contacto en la cuenta y sin decisor identificado"));
            }

            if (o.FechasCierreDistintas >= 3)
            {
                r.Add(Alerta(AlertaCierreCambiado, o.FechasCierreDistintas + " fechas de cierre distintas en el historial"));
            }

            if (o.Calificacion == null)
            {
                r.Add(Alerta(AlertaCalificacionPendiente, "Sin calificar"));
            }
            else
            {
                int dias = (hoy.Date - o.Calificacion.FechaRegistro.Date).Days;
                if (dias > 30) { r.Add(Alerta(AlertaCalificacionPendiente, "Calificación desactualizada (hace " + dias + " días)")); }
            }

            if (o.IdNegocio.HasValue && o.IdNegocio.Value == IdNegocioPerdida)
            {
                r.Add(Alerta(AlertaInconsistente, "Sigue abierta pero el código de cierre indica perdida o descartada"));
            }

            return r;
        }

        private static List<string> Faltantes(EntCalificacionForeCast o)
        {
            List<string> r = new List<string>();
            if (Vacio(o.Contacto) && Vacio(o.LiderProyectoCliente)) { r.Add("Contacto"); }
            if (!o.FechaCierre.HasValue) { r.Add("Fecha de cierre"); }
            if (!o.UtilidadBrutaEstimada.HasValue || o.UtilidadBrutaEstimada.Value == 0m) { r.Add("Utilidad bruta estimada"); }
            if (o.Calificacion == null || Vacio(o.Calificacion.DecisorCargo)) { r.Add("Decisor con cargo"); }
            if (o.Calificacion == null) { r.Add("Calificación"); }
            return r;
        }

        /// <summary>La primera que aplique, en el orden de la especificacion.</summary>
        private static string Accion(EntCalificacionForeCast o)
        {
            if (TieneAlerta(o, AlertaInconsistente)) { return "Cerrar o corregir la etapa: el código indica perdida o descartada"; }
            if (TieneAlerta(o, AlertaCierreVencido)) { return "Actualizar la fecha de cierre con el cliente"; }
            if (o.Calificacion == null) { return "Calificar la oportunidad"; }
            if (TieneAlerta(o, AlertaSinPropuesta)) { return "Enviar la propuesta"; }

            /* Los tres tienen el mismo maximo (20), asi que "en proporcion a su
               maximo" es comparar los puntos. En empate gana el primero. */
            int minimo = Math.Min(o.Presupuesto, Math.Min(o.Autoridad, o.Necesidad));
            if (minimo < 20)
            {
                if (o.Presupuesto == minimo) { return "Confirmar el presupuesto con el cliente"; }
                if (o.Autoridad == minimo) { return "Identificar y reunirse con el decisor"; }
                return "Validar la necesidad y la fecha del requerimiento";
            }

            if (TieneAlerta(o, AlertaSinActividad)) { return "Retomar contacto con el cliente"; }
            return "Avanzar a la siguiente etapa";
        }

        private static EntAlertaCalificacion Alerta(int codigo, string texto)
        {
            return new EntAlertaCalificacion { Codigo = codigo, Texto = texto };
        }

        private static bool Vacio(string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        /// <summary>dd/MM/yyyy con cultura invariante: el servidor corre en es-EC.</summary>
        private static string Texto(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString(FormatoFecha, CultureInfo.InvariantCulture) : "";
        }
    }
}
