using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Las reglas de docs/superpowers/specs/2026-09-28-calificacion-forecast-design.md.
    /// Todo lo que decide un puntaje, una alerta o una accion vive aca y en
    /// ningun otro lado: ni el procedimiento ni el JavaScript calculan nada.
    ///
    /// Ninguna funcion lee el reloj: la fecha de hoy entra como parametro, para
    /// que las pruebas fijen el dia y los limites (60 dias, 61 dias) no se
    /// muevan solos.
    /// </summary>
    public static partial class NegCalificacionForeCast
    {
        public const string Caliente = "Caliente";
        public const string Tibia = "Tibia";
        public const string Fria = "Fría";
        public const string SinCalificarTexto = "Sin calificar";

        /// <summary>PrmPorcentaje: IdNegocio 10 = "Perdida o Descartada" (verificado 2026-09-28).</summary>
        public const int IdNegocioPerdida = 10;

        /// <summary>Con menos cierres que esto un segmento no tiene evidencia y suma el neutro.</summary>
        public const int MinimoCierresSegmento = 10;

        public static int PuntosTiempo(DateTime? cierre, DateTime hoy)
        {
            if (!cierre.HasValue) { return 0; }
            int dias = (cierre.Value.Date - hoy.Date).Days;
            if (dias < 0) { return 3; }
            if (dias <= 60) { return 15; }
            if (dias <= 120) { return 12; }
            if (dias <= 240) { return 8; }
            return 5;
        }

        /// <summary>Calificar tambien es actividad: gana la fecha mas reciente.</summary>
        public static DateTime? UltimaActividad(DateTime? modificacion, DateTime? calificacion)
        {
            if (!modificacion.HasValue) { return calificacion; }
            if (!calificacion.HasValue) { return modificacion; }
            return modificacion.Value >= calificacion.Value ? modificacion : calificacion;
        }

        public static int PuntosActividad(DateTime? ultima, DateTime hoy)
        {
            if (!ultima.HasValue) { return 0; }
            int dias = (hoy.Date - ultima.Value.Date).Days;
            if (dias <= 14) { return 15; }
            if (dias <= 30) { return 10; }
            if (dias <= 60) { return 5; }
            return 0;
        }

        public static int PuntosEncaje(string segmento, string tipoEmpresa, int ganadosCliente, IList<EntTasaExito> tasas)
        {
            if (tasas == null) { tasas = new List<EntTasaExito>(); }

            int puntos = PuntosSegmento(segmento, tasas) + PuntosTipo(tipoEmpresa, tasas);
            if (ganadosCliente >= 3) { puntos += 3; }
            else if (ganadosCliente >= 1) { puntos += 2; }

            return Math.Min(puntos, 10);
        }

        private static int PuntosSegmento(string segmento, IList<EntTasaExito> tasas)
        {
            EntTasaExito t = Buscar(tasas, "SEGMENTO", segmento);
            if (t == null || t.Ganados + t.Perdidos < MinimoCierresSegmento) { return 3; }

            decimal tasa = (decimal)t.Ganados / (t.Ganados + t.Perdidos);
            if (tasa >= 0.60m) { return 5; }
            if (tasa >= 0.50m) { return 3; }
            return 1;
        }

        /// <summary>2 si es el tipo de empresa con mejor tasa de exito; 1 en cualquier otro caso.</summary>
        private static int PuntosTipo(string tipoEmpresa, IList<EntTasaExito> tasas)
        {
            EntTasaExito mejor = null;
            decimal mejorTasa = -1m;

            foreach (EntTasaExito t in tasas)
            {
                if (!Igual(t.Dimension, "TIPO") || t.Ganados + t.Perdidos == 0) { continue; }
                decimal tasa = (decimal)t.Ganados / (t.Ganados + t.Perdidos);
                if (tasa > mejorTasa) { mejorTasa = tasa; mejor = t; }
            }

            return mejor != null && Igual(mejor.Valor, tipoEmpresa) ? 2 : 1;
        }

        private static EntTasaExito Buscar(IList<EntTasaExito> tasas, string dimension, string valor)
        {
            foreach (EntTasaExito t in tasas)
            {
                if (Igual(t.Dimension, dimension) && Igual(t.Valor, valor)) { return t; }
            }
            return null;
        }

        /// <summary>Comparacion sin espacios alrededor ni mayusculas. Null solo es igual a null o vacio.</summary>
        internal static bool Igual(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public static string Clasificar(int puntaje)
        {
            if (puntaje >= 75) { return Caliente; }
            if (puntaje >= 50) { return Tibia; }
            return Fria;
        }

        /// <summary>PVPEstimado x PrmPorcentaje.Porcentaje / 100, redondeado a centavos.</summary>
        public static decimal MontoPonderado(decimal? pvp, decimal? porcentaje)
        {
            if (!pvp.HasValue || !porcentaje.HasValue) { return 0m; }
            return Math.Round(pvp.Value * porcentaje.Value / 100m, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>UPSIDE o FORECAST, o "Propuesta entregada" (40 %) en adelante.</summary>
        public static bool EsAvanzada(string prioridad, decimal? porcentaje)
        {
            if (Igual(prioridad, "UPSIDE") || Igual(prioridad, "FORECAST")) { return true; }
            return porcentaje.HasValue && porcentaje.Value >= 40m;
        }
    }
}
