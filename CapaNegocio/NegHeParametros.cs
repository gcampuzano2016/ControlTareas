using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Los parametros de calculo, leidos de HE_Parametro.
    ///
    /// No se incrustan en el codigo porque son configurables por diseno: hay un
    /// permiso previsto para editarlos. Si una clave faltara, se usa el valor con
    /// el que se cargo la base, que es lo que el modulo ha usado siempre.
    ///
    /// La tabla esta versionada por fecha de vigencia, asi que "los parametros"
    /// no existen sin una fecha: siempre hay que decir A QUE FECHA. Por eso
    /// Vigentes exige el corte y no hay ninguna version sin argumento.
    /// </summary>
    public static class NegHeParametros
    {
        /// <summary>
        /// Los parametros que regian en la fecha de corte -el ultimo dia del
        /// periodo que se esta calculando, la misma fecha con la que se resuelve
        /// el sueldo-.
        /// </summary>
        public static EntHeParametros Vigentes(DateTime corte)
        {
            List<EntHeParametroFila> historial = DaoHorasExtras.LeerHistorialParametros(corte);

            EntHeParametros p = new EntHeParametros();
            p.DiasMes = (int)ValorAlCorte(historial, "DiasMes", corte, 30m);
            p.HorasMesJornadaCompleta = (int)ValorAlCorte(historial, "HorasMesJornadaCompleta", corte, 240m);
            p.Factor50 = ValorAlCorte(historial, "Factor50", corte, 1.5m);
            p.Factor100 = ValorAlCorte(historial, "Factor100", corte, 2m);
            p.TopeDiario50 = (int)ValorAlCorte(historial, "TopeDiario50", corte, 4m);
            p.TopeSemanal50 = (int)ValorAlCorte(historial, "TopeSemanal50", corte, 12m);
            p.DecimalesMonto = (int)ValorAlCorte(historial, "DecimalesMonto", corte, 2m);
            return p;
        }

        /// <summary>
        /// El valor de una clave a una fecha: la version mas reciente que ya habia
        /// empezado y que no estaba cerrada antes de esa fecha.
        ///
        /// La fecha que importa es la del PERIODO, no la de hoy. Un periodo de agosto
        /// se calcula con lo que regia en agosto, aunque se reabra en noviembre --
        /// si no, reabrir para meter una aprobacion tardia cambiaria de paso el
        /// importe de las otras 61 personas.
        /// </summary>
        public static decimal ValorAlCorte(List<EntHeParametroFila> historial, string clave,
                                           DateTime corte, decimal porOmision)
        {
            if (historial == null) { return porOmision; }

            bool hayGanadora = false;
            decimal ganador = porOmision;
            DateTime desdeGanador = DateTime.MinValue;

            foreach (EntHeParametroFila f in historial)
            {
                if (f == null) { continue; }

                string suClave = f.Clave == null ? "" : f.Clave.Trim();
                if (!string.Equals(suClave, clave, StringComparison.OrdinalIgnoreCase)) { continue; }

                /* Todavia no habia empezado al corte. */
                if (f.FechaVigenciaDesde.Date > corte.Date) { continue; }

                /* Ya estaba cerrada antes del corte. El dia mismo del cierre
                   todavia rige: FechaVigenciaHasta es inclusiva, igual que
                   FechaVigenciaDesde. */
                if (f.FechaVigenciaHasta.HasValue && f.FechaVigenciaHasta.Value.Date < corte.Date) { continue; }

                /* Gana la que empezo mas tarde; a igualdad, la ultima de la
                   lista -el >= y no el >-, para que la decision no dependa de
                   en que orden devolvio las filas la base. */
                if (hayGanadora && f.FechaVigenciaDesde.Date < desdeGanador) { continue; }

                hayGanadora = true;
                ganador = f.Valor;
                desdeGanador = f.FechaVigenciaDesde.Date;
            }

            return hayGanadora ? ganador : porOmision;
        }
    }
}
