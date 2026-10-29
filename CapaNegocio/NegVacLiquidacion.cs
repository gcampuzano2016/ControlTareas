using CapaDato;
using CapaEntidad;
using System;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Liquidacion de vacaciones por periodo, para salidas: la hoja
    /// LIQUIDACION del Excel de GTH.
    ///
    /// Los dias los calcula el motor con @Liquidacion = 1: exactamente a la
    /// fecha de salida y solo con lo tomado hasta ella. El sueldo, el mismo de
    /// la nomina (HE_Salario o Vac_Sueldo) vigente a esa fecha. El valor del
    /// Art. 71 solo se calcula si GTH ingresa lo ganado en los ultimos 12
    /// meses.
    /// </summary>
    public static class NegVacLiquidacion
    {
        /// <summary>Todo el maestro, para elegir a quien liquidar.</summary>
        public static System.Collections.Generic.List<EntVacColaborador> Colaboradores()
        {
            return DaoVacSaldos.ListaColaboradores();
        }

        /// <summary>Si no se indica fecha: la de salida registrada; si no tiene, hoy.</summary>
        public static DateTime FechaPorDefecto(EntVacColaborador c, DateTime hoy)
        {
            return c != null && c.FechaSalida.HasValue ? c.FechaSalida.Value.Date : hoy.Date;
        }

        public static string ValidarGanado(decimal? ganado)
        {
            if (ganado.HasValue && ganado.Value < 0m) { return "Lo ganado en los últimos 12 meses no puede ser negativo."; }
            return "";
        }

        /// <summary>Null si la persona no existe.</summary>
        public static EntVacLiquidacion Calcular(int idColaborador, DateTime? fecha, decimal? ganadoUltimoAnio, DateTime hoy)
        {
            DateTime salida = fecha ?? FechaPorDefecto(Colaboradores().FirstOrDefault(c => c.IdColaborador == idColaborador), hoy);
            EntVacFicha f = DaoVacSaldos.Ficha(salida, idColaborador, true);
            if (f == null || f.Saldo == null) { return null; }
            return Armar(f, salida, DaoVacSaldos.Sueldos(), ganadoUltimoAnio);
        }

        public static EntVacLiquidacion Armar(EntVacFicha f, DateTime salida, EntVacSueldos sueldos, decimal? ganadoUltimoAnio)
        {
            EntVacSaldo s = f.Saldo;
            EntVacLiquidacion l = new EntVacLiquidacion
            {
                Ficha = f,
                FechaSalida = salida.Date,
                Antiguedad = Antiguedad(f.Colaborador.FechaIngreso, salida),
                DiasTranscurridos = Math.Max(0, (salida.Date - s.InicioPeriodo.Date).Days),
                DiasDelPeriodo = (s.ProximoAniversario.Date - s.InicioPeriodo.Date).Days,
                PeriodoDe = NegVacEstado.PeriodoDeCadaSolicitud(f),
                Posteriores = f.Movimientos.Count(m => m.Situacion == "POSTERIOR"),
                GanadoUltimoAnio = ganadoUltimoAnio
            };

            EntVacNominaFila n = NegVacNomina.Fila(new EntVacFila { Colaborador = f.Colaborador, Saldo = s }, sueldos);
            l.Sueldo = n.Sueldo;
            l.FuenteSueldo = n.FuenteSueldo;
            l.ValorReferencial = n.Valor;

            if (ganadoUltimoAnio.HasValue && ganadoUltimoAnio.Value > 0m)
            {
                l.ValorArt71 = ValorArt71(ganadoUltimoAnio.Value, s.Saldo);
            }
            return l;
        }

        /// <summary>LIQUIDACION!I20: ROUND(ganado / 360 x dias, 2).</summary>
        public static decimal ValorArt71(decimal ganadoUltimoAnio, decimal dias)
        {
            return Math.Round(ganadoUltimoAnio * dias / 360m, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// "2 años, 1 meses, 22 días": DATEDIF "y", "ym" y "md" del Excel. Los
        /// dias se cuentan desde el mismo dia del mes anterior a la salida.
        /// </summary>
        public static string Antiguedad(DateTime ingreso, DateTime salida)
        {
            if (salida < ingreso) { return "0 años, 0 meses, 0 días"; }
            int meses = (salida.Year - ingreso.Year) * 12 + salida.Month - ingreso.Month;
            if (salida.Day < ingreso.Day) { meses--; }
            DateTime base_ = ingreso.AddMonths(meses);
            int dias = (salida.Date - base_.Date).Days;
            return (meses / 12) + " años, " + (meses % 12) + " meses, " + dias + " días";
        }
    }
}
