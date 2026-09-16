using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// El calculo de horas extras. Clase pura: no consulta la base, no sabe de
    /// pantallas, y por eso se puede probar entera sin levantar nada.
    ///
    /// Todo va en decimal y nunca en double: con double, 0.1 + 0.2 no es 0.3 y
    /// un centavo de diferencia por fila se convierte en dolares al cierre.
    /// </summary>
    public static class NegHorasExtras
    {
        /// <summary>
        /// Decimales con los que se GUARDAN los valores hora. No es la
        /// precision con la que se calculan: la cadena corre en decimal a plena
        /// precision y esto solo recorta lo que se muestra y se persiste.
        /// </summary>
        private const int DecimalesValorHora = 6;

        /// <summary>
        /// El sueldo que rige a una fecha: el mas reciente cuya vigencia no sea
        /// posterior al corte. Cero si no hay ninguno, que el calculo traduce a
        /// hora ordinaria cero y fila marcada.
        ///
        /// A igual fecha de vigencia, gana el ajuste sobre el rol (documento
        /// funcional 2.2): el ajuste sustituye al rol. La comparacion es
        /// case-insensitive con trim para que variaciones de formato en la
        /// plantilla que mantiene RRHH no cambien la decision de pago.
        ///
        /// Si el desempate es entre dos filas del mismo origen, gana la ultima
        /// de la lista: es estable y no hay criterio de negocio que aplicar.
        /// </summary>
        public static decimal SalarioVigente(List<EntHeSalario> historial, DateTime corte)
        {
            if (historial == null) { return 0m; }

            decimal vigente = 0m;
            DateTime mejor = DateTime.MinValue;
            string origenGanador = "";

            foreach (EntHeSalario s in historial)
            {
                if (s == null) { continue; }
                if (s.FechaVigenciaDesde > corte) { continue; }

                /* Si es una fecha mas reciente, gana sin mirar origen. */
                if (s.FechaVigenciaDesde > mejor)
                {
                    mejor = s.FechaVigenciaDesde;
                    vigente = s.Monto;
                    origenGanador = s.Origen ?? "";
                }
                /* Si es la misma fecha, solo gana si su origen es superior al
                   del actual ganador (ajuste > rol) o si son del mismo origen
                   y esta al final de la lista (estable). */
                else if (s.FechaVigenciaDesde == mejor)
                {
                    string origenActual = (s.Origen ?? "").Trim().ToUpperInvariant();
                    string origenActualGanador = origenGanador.Trim().ToUpperInvariant();
                    bool esAjuste = origenActual == "AJUSTE";
                    bool eraAjuste = origenActualGanador == "AJUSTE";

                    /* A igual fecha: ajuste manda sobre rol. Si son del mismo
                       origen, el ultimo de la lista (esta iteracion) gana. */
                    if (esAjuste && !eraAjuste)
                    {
                        vigente = s.Monto;
                        origenGanador = s.Origen ?? "";
                    }
                    else if (!esAjuste && !eraAjuste)
                    {
                        /* Dos roles en la misma fecha: el ultimo gana. */
                        vigente = s.Monto;
                        origenGanador = s.Origen ?? "";
                    }
                    else if (esAjuste && eraAjuste)
                    {
                        /* Dos ajustes en la misma fecha: el ultimo gana. */
                        vigente = s.Monto;
                        origenGanador = s.Origen ?? "";
                    }
                    /* esAjuste=false && eraAjuste=true: el rol no gana sobre
                       el ajuste, se mantiene el vigente. */
                }
            }

            return vigente;
        }

        /// <summary>
        /// El divisor mensual de horas.
        ///
        /// Si hay divisor manual, manda. Si no, para jornada completa son las
        /// horas mensuales del parametro y para cualquier otra, horas por dia
        /// por dias del mes. Para 8 h/dia las dos ramas dan 240; la condicion se
        /// conserva para que el origen de ese 240 sea explicito y auditable.
        /// </summary>
        private static int Divisor(EntHeInsumo i, EntHeParametros p)
        {
            if (i.DivisorManual.HasValue && i.DivisorManual.Value > 0)
            {
                return i.DivisorManual.Value;
            }

            if (i.JornadaHorasDia == 8) { return p.HorasMesJornadaCompleta; }

            return i.JornadaHorasDia * p.DiasMes;
        }

        /// <summary>Redondeo de pago: alejandose del cero, que es como se paga.</summary>
        private static decimal Monto(decimal v, EntHeParametros p)
        {
            return Math.Round(v, p.DecimalesMonto, MidpointRounding.AwayFromZero);
        }

        /// <summary>Calcula una fila. No lanza nunca: un dato malo se marca, no revienta.</summary>
        public static EntHeResultado Calcular(EntHeInsumo insumo, EntHeParametros parametros)
        {
            EntHeResultado r = new EntHeResultado();

            if (insumo == null || parametros == null)
            {
                r.TieneAdvertencia = true;
                return r;
            }

            r.Divisor = Divisor(insumo, parametros);

            /* Equivalente al IFERROR del Excel. Sin esto, un divisor en cero
               lanzaria DivideByZeroException a media nomina. Horas negativas
               no son credito: son dato malo que bloquea el cierre de periodo
               como salario cero o divisor cero. La pantalla no deberia
               permitirlas, pero el calculo no confia en la pantalla. */
            if (r.Divisor <= 0 || insumo.SalarioBaseVigente <= 0m ||
                insumo.Horas50 < 0m || insumo.Horas100 < 0m)
            {
                r.TieneAdvertencia = true;
                r.ValorHoraOrdinaria = 0m;
                r.ValorHora50 = 0m;
                r.ValorHora100 = 0m;
                r.TotalHoras = 0m;
                return r;
            }

            /* La cadena corre SIN redondear, a plena precision de decimal. El
               documento funcional lo dice con todas las letras: los valores
               intermedios "se almacenan con 6 decimales; no se redondean
               durante la cadena". Son dos cosas distintas y confundirlas
               cambia el resultado.

               Con salario 329 y divisor 120: sin redondear, la hora al 50% es
               4.1125 exactos. Redondeando la hora ordinaria a 6 decimales
               primero -2.741667- da 4.1125005, que a seis decimales es
               4.112501. Un centavo por aqui, otro por alla. */
            decimal horaOrdinaria = insumo.SalarioBaseVigente / r.Divisor;
            decimal hora50  = horaOrdinaria * parametros.Factor50;
            decimal hora100 = horaOrdinaria * parametros.Factor100;

            /* Estos tres SI se recortan, pero solo para mostrarlos y guardarlos:
               los totales de abajo se calculan con los valores sin recortar. */
            r.ValorHoraOrdinaria = Math.Round(horaOrdinaria, DecimalesValorHora, MidpointRounding.AwayFromZero);
            r.ValorHora50        = Math.Round(hora50,        DecimalesValorHora, MidpointRounding.AwayFromZero);
            r.ValorHora100       = Math.Round(hora100,       DecimalesValorHora, MidpointRounding.AwayFromZero);

            r.TotalHoras = insumo.Horas50 + insumo.Horas100;

            /* Quien no aplica no cobra, aunque lleguen horas. La pantalla no
               deberia permitir cargarlas, pero el calculo no confia en la
               pantalla. */
            if (!insumo.AplicaHE)
            {
                r.Total50 = 0m;
                r.Total100 = 0m;
                r.TotalHE = 0m;
                return r;
            }

            /* Del valor SIN recortar, no de r.ValorHora50. Esa es la diferencia
               entre "no se redondea durante la cadena" y redondear dos veces. */
            r.Total50  = Monto(insumo.Horas50  * hora50,  parametros);
            r.Total100 = Monto(insumo.Horas100 * hora100, parametros);
            r.TotalHE  = r.Total50 + r.Total100;

            return r;
        }

        /// <summary>
        /// El total del periodo: la suma de los totales YA redondeados.
        ///
        /// No es lo mismo que redondear la suma, y la diferencia se ve: asi el
        /// gran total cuadra con lo que cualquiera obtiene sumando a mano la
        /// columna de la pantalla. Si no cuadrara, la primera reaccion de quien
        /// revisa la nomina seria desconfiar del sistema entero.
        /// </summary>
        public static decimal TotalDelPeriodo(List<decimal> totalesPorFila)
        {
            if (totalesPorFila == null) { return 0m; }

            decimal suma = 0m;
            foreach (decimal t in totalesPorFila) { suma += t; }

            return suma;
        }
    }
}
