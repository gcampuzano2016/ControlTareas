using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Nomina y provision de vacaciones: la hoja NOMINA del Excel de GTH.
    /// Dias a liquidar = el saldo del motor; valor = dias x (sueldo / DIAS_MES),
    /// redondeado al centavo. Para un inactivo todo es a su fecha de salida.
    ///
    /// El sueldo: el vigente de HE_Salario a la fecha de calculo, con la misma
    /// regla que horas extras y el certificado (NegHorasExtras.SalarioVigente);
    /// si la persona no esta ahi, el de Vac_Sueldo. Ver
    /// docs/sql/2026-10-29-vacaciones-nomina.sql.
    ///
    /// Es referencial, como dice el propio Excel: en la liquidacion la ley
    /// (Art. 71 CT) paga la veinticuatroava parte de lo percibido en el
    /// periodo, y con horas extras o comisiones hay que validar con nomina.
    /// </summary>
    public static class NegVacNomina
    {
        public const string FuenteHorasExtras = "HE_Salario";
        public const string FuenteVacaciones = "Excel de GTH";
        public const string ObsSinSueldo = "Falta sueldo";
        public const string ObsAnticipo = "Días tomados por anticipado: evaluar descuento";

        public static EntVacNomina Cargar(DateTime fechaCalculo)
        {
            return Armar(DaoVacSaldos.Tablero(fechaCalculo), DaoVacSaldos.Sueldos(), fechaCalculo);
        }

        public static EntVacNomina Armar(EntVacDatos datos, EntVacSueldos sueldos, DateTime fechaCalculo)
        {
            EntVacTablero t = NegVacSaldos.Armar(datos, fechaCalculo);
            EntVacNomina n = new EntVacNomina { FechaCalculo = fechaCalculo.Date, Avisos = t.Avisos };
            Dictionary<string, EntVacNominaEmpresa> empresas = new Dictionary<string, EntVacNominaEmpresa>();

            foreach (EntVacFila f in t.Filas)
            {
                EntVacNominaFila fila = Fila(f, sueldos);
                n.Filas.Add(fila);

                if (!NegVacSaldos.EsActivo(f.Colaborador)) { continue; }

                string clave = string.IsNullOrWhiteSpace(f.Colaborador.Empresa) ? "(sin empresa)" : f.Colaborador.Empresa.Trim().ToUpperInvariant();
                EntVacNominaEmpresa e;
                if (!empresas.TryGetValue(clave, out e)) { e = new EntVacNominaEmpresa { Empresa = clave }; empresas[clave] = e; }

                n.Activos++;
                e.Personas++;
                n.TotalDias += f.Saldo.Saldo;
                e.Dias += f.Saldo.Saldo;
                if (fila.Valor.HasValue)
                {
                    n.TotalValor += fila.Valor.Value;
                    e.Valor += fila.Valor.Value;
                    if (fila.Valor.Value < 0m) { n.ValorAnticipos += fila.Valor.Value; }
                }
                else
                {
                    n.SinSueldo++;
                    e.SinSueldo++;
                }
            }

            n.PorEmpresa = empresas.Values.OrderBy(x => x.Empresa, StringComparer.Ordinal).ToList();
            return n;
        }

        /// <summary>Sueldo vigente, fuente, valor diario, valor y observacion de una persona.</summary>
        public static EntVacNominaFila Fila(EntVacFila f, EntVacSueldos sueldos)
        {
            EntVacNominaFila r = new EntVacNominaFila { Colaborador = f.Colaborador, Saldo = f.Saldo };
            DateTime corte = f.Saldo.FechaCalculo == DateTime.MinValue ? DateTime.Today : f.Saldo.FechaCalculo;
            int id = f.Colaborador.IdColaborador;

            List<EntHeSalario> historial;
            decimal sueldo = 0m;
            if (sueldos.HorasExtras.TryGetValue(id, out historial))
            {
                sueldo = NegHorasExtras.SalarioVigente(historial, corte);
                if (sueldo > 0m) { r.FuenteSueldo = FuenteHorasExtras; }
            }
            if (sueldo <= 0m && sueldos.Vacaciones.TryGetValue(id, out historial))
            {
                sueldo = NegHorasExtras.SalarioVigente(historial, corte);
                if (sueldo > 0m) { r.FuenteSueldo = FuenteVacaciones; }
            }

            if (sueldo > 0m)
            {
                r.Sueldo = sueldo;
                r.ValorDia = Math.Round(sueldo / sueldos.DiasMes, 2, MidpointRounding.AwayFromZero);
                r.Valor = Valor(f.Saldo.Saldo, sueldo, sueldos.DiasMes);
            }

            if (!r.Sueldo.HasValue) { r.Observacion = ObsSinSueldo; }
            else if (f.Saldo.Saldo < 0m) { r.Observacion = ObsAnticipo; }
            return r;
        }

        /// <summary>
        /// NOMINA!O: ROUND(dias x sueldo / DIAS_MES, 2). El valor diario NO se
        /// redondea antes de multiplicar, como en el Excel.
        /// </summary>
        public static decimal Valor(decimal dias, decimal sueldo, decimal diasMes)
        {
            return Math.Round(dias * sueldo / diasMes, 2, MidpointRounding.AwayFromZero);
        }
    }
}
