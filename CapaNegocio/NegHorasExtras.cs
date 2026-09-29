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

            /* Decision por tramo (tarea 3): si vienen minutos exactos, mandan
               sobre las horas del insumo. minutos/60 SIN redondear -la misma
               regla de "no se redondea durante la cadena" que ya rige para
               horaOrdinaria y hora50/hora100 mas abajo-. Cada tramo decide
               por su cuenta: si solo llegan minutos de un tramo, el otro
               sigue tomando su hora del insumo tal cual, sin pasar por
               minutos. Si ninguno de los dos llega, horas50/horas100 quedan
               identicos a insumo.Horas50/Horas100 y el resto del metodo se
               comporta exactamente igual que antes de esta tarea. */
            bool usaMinutos = insumo.Minutos50.HasValue || insumo.Minutos100.HasValue;
            decimal horas50 = insumo.Minutos50.HasValue ? insumo.Minutos50.Value / 60m : insumo.Horas50;
            decimal horas100 = insumo.Minutos100.HasValue ? insumo.Minutos100.Value / 60m : insumo.Horas100;

            /* Horas negativas no son credito: son dato malo que bloquea el
               cierre de periodo. Se valida primero, antes de cualquier otro
               error, porque TotalHoras debe ser cero si hay horas negativas
               incluso si hay otros problemas (salario cero, divisor cero).
               Un numero negativo en pantalla es lo peor que puede pasar. */
            if (horas50 < 0m || horas100 < 0m)
            {
                r.TieneAdvertencia = true;
                r.ValorHoraOrdinaria = 0m;
                r.ValorHora50 = 0m;
                r.ValorHora100 = 0m;
                r.TotalHoras = 0m;
                return r;
            }

            /* Equivalente al IFERROR del Excel. Sin esto, un divisor en cero
               lanzaria DivideByZeroException a media nomina. */
            if (r.Divisor <= 0 || insumo.SalarioBaseVigente <= 0m)
            {
                r.TieneAdvertencia = true;
                r.ValorHoraOrdinaria = 0m;
                r.ValorHora50 = 0m;
                r.ValorHora100 = 0m;

                /* Misma regla que el flujo principal (ver mas abajo): con
                   minutos, TotalHoras se redondea a 2 decimales porque es lo
                   que se muestra. Round 1 de revision: esta rama se habia
                   quedado sin redondear, inconsistente con el caso normal. */
                r.TotalHoras = usaMinutos
                    ? Math.Round(horas50 + horas100, 2, MidpointRounding.AwayFromZero)
                    : horas50 + horas100;
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

            /* TotalHoras SI se redondea cuando hay minutos: es lo que se
               muestra en la columna de horas, y 100 minutos no se ven en
               pantalla como "1.6666666666666666666666666667" horas. Sin
               minutos, sigue sin redondear -exactamente como antes de esta
               tarea-, porque insumo.Horas50/Horas100 ya vienen con los
               decimales que alguien digito o que trajo la siembra. */
            r.TotalHoras = usaMinutos
                ? Math.Round(horas50 + horas100, 2, MidpointRounding.AwayFromZero)
                : horas50 + horas100;

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
               entre "no se redondea durante la cadena" y redondear dos veces.
               horas50/horas100 ya son minutos/60 sin redondear cuando hay
               minutos (o insumo.Horas50/Horas100 tal cual cuando no los hay). */
            decimal exacto50 = horas50 * hora50;
            decimal exacto100 = horas100 * hora100;

            if (usaMinutos)
            {
                /* Ruling del controlador tras la validacion de aceptacion: el
                   analisis manual de Nomina redondea UNA sola vez por
                   persona, no un tramo y despues el otro. Sumar dos
                   redondeos separados puede quedar un centavo lejos del
                   analisis manual -ver Calcular_ConMinutos_RedondeaUnaVezPorPersona,
                   265.83 contra los 265.84 que darian dos redondeos-, y ese
                   centavo es justo lo que Nomina va a comparar a mano.

                   Por eso TotalHE sale de redondear la suma EXACTA una sola
                   vez, Total50 sale de redondear su propio exacto -es el
                   numero que se muestra en esa columna-, y Total100 se
                   obtiene por diferencia (TotalHE - Total50) en vez de
                   redondear su propio exacto: asi las dos columnas de la
                   pantalla siempre suman, centavo a centavo, el total ya
                   redondeado, sin un tercer numero suelto que no cuadre. */
                r.TotalHE = Monto(exacto50 + exacto100, parametros);
                r.Total50 = Monto(exacto50, parametros);
                r.Total100 = r.TotalHE - r.Total50;
            }
            else
            {
                /* Sin minutos, exactamente como antes de esta correccion: cada
                   tramo redondea su propio exacto y TotalHE es la suma de los
                   dos ya redondeados. */
                r.Total50 = Monto(exacto50, parametros);
                r.Total100 = Monto(exacto100, parametros);
                r.TotalHE = r.Total50 + r.Total100;
            }

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
