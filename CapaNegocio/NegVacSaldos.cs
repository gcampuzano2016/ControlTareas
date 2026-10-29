using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Pantalla de saldos de vacaciones (GTH). Los dias los calcula
    /// Sp_RTA_VacCalcular; aqui solo se une el saldo con la persona y se suman
    /// las tarjetas. Las tarjetas cuentan solo ACTIVOS, igual que el DASHBOARD
    /// del Excel de GTH, para que el total se pueda cuadrar contra el.
    /// </summary>
    public static class NegVacSaldos
    {
        /// <summary>Mas atras no hay datos cargados; mas adelante el calculo ya no dice nada util.</summary>
        public static readonly DateTime FechaMinima = new DateTime(2005, 1, 1);
        public const int AniosHaciaAdelante = 3;

        public static string ValidarFecha(DateTime fecha, DateTime hoy)
        {
            if (fecha.Date < FechaMinima) { return "La fecha de cálculo no puede ser anterior al " + FechaMinima.ToString("dd/MM/yyyy") + "."; }
            if (fecha.Date > hoy.Date.AddYears(AniosHaciaAdelante)) { return "La fecha de cálculo no puede pasar de " + AniosHaciaAdelante + " años desde hoy."; }
            return "";
        }

        public static EntVacTablero Cargar(DateTime fechaCalculo)
        {
            return Armar(DaoVacSaldos.Tablero(fechaCalculo), fechaCalculo);
        }

        /// <summary>Null si la persona no existe.</summary>
        public static EntVacFicha Ficha(DateTime fechaCalculo, int idColaborador)
        {
            return DaoVacSaldos.Ficha(fechaCalculo, idColaborador);
        }

        public static EntVacTablero Armar(EntVacDatos datos, DateTime fechaCalculo)
        {
            EntVacTablero t = new EntVacTablero { FechaCalculo = fechaCalculo.Date };

            Dictionary<int, EntVacSaldo> saldos = new Dictionary<int, EntVacSaldo>();
            foreach (EntVacSaldo s in datos.Saldos) { saldos[s.IdColaborador] = s; }

            HashSet<int> vistos = new HashSet<int>();
            foreach (EntVacColaborador c in datos.Colaboradores.OrderBy(x => x.Nombre, StringComparer.CurrentCultureIgnoreCase))
            {
                vistos.Add(c.IdColaborador);
                EntVacSaldo s;
                if (!saldos.TryGetValue(c.IdColaborador, out s))
                {
                    t.Avisos.Add("El motor no devolvió saldo para " + c.Nombre + ".");
                    continue;
                }
                t.Filas.Add(new EntVacFila { Colaborador = c, Saldo = s });
            }
            foreach (EntVacSaldo s in datos.Saldos.Where(x => !vistos.Contains(x.IdColaborador)))
            {
                t.Avisos.Add("Hay un saldo sin colaborador (Id " + s.IdColaborador + ").");
            }

            t.Resumen = Resumir(t.Filas);
            return t;
        }

        public static EntVacResumen Resumir(IEnumerable<EntVacFila> filas)
        {
            EntVacResumen r = new EntVacResumen();
            foreach (EntVacFila f in filas)
            {
                if (!EsActivo(f.Colaborador))
                {
                    r.Inactivos++;
                    continue;
                }
                EntVacSaldo s = f.Saldo;
                r.Activos++;
                r.SaldoTotal += s.Saldo;
                r.DiasVencidos += s.Vencidos;
                if (s.PorVencer > 0)
                {
                    r.PersonasPorVencer++;
                    r.DiasPorVencer += s.PorVencer;
                }
                if (s.Estado == "ANTICIPO")
                {
                    r.PersonasAnticipo++;
                    r.DiasAnticipo += s.Saldo;
                }
                if (s.Estado == "ACUMULANDO") { r.PersonasAcumulando++; }
            }
            return r;
        }

        public static bool EsActivo(EntVacColaborador c)
        {
            return c != null && string.Equals(c.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase);
        }
    }
}
