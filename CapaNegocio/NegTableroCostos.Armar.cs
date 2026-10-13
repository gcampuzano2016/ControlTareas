using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Filtros y secciones del tablero, con las mismas reglas que las formulas
    /// del Excel:
    ///   - Mes, Responsable, Empresa y Contrato filtran todo.
    ///   - Recargo y Estado HE filtran SOLO Extra y Traslado.
    ///   - La tabla por mes ignora el filtro de Mes; el top 15 ignora el de
    ///     Responsable; la tabla por anio aplica todos.
    /// </summary>
    public static partial class NegTableroCostos
    {
        public const int MesesMaximos = 24;
        public const int TopMaximo = 15;

        private static readonly string[] EstadosExtra = { "Aprobado", "Solicitado", "Rechazado", "Sin solicitud" };
        private static readonly string[] EstadosTraslado = { "Aprobado", "Solicitado", "Rechazado", "Sin solicitud", "No aplica" };

        /// <summary>Orden de las listas: alfabetico en espanol, sin distinguir mayusculas (la Ñ va despues de la N).</summary>
        private static readonly StringComparer OrdenTexto = StringComparer.Create(CultureInfo.GetCultureInfo("es-ES"), true);

        [Flags]
        private enum Ignorar
        {
            Nada = 0,
            Mes = 1,
            Responsable = 2,
            Empresa = 4,
            Contrato = 8
        }

        public static EntTableroCostos Armar(EntTcClasificado datos, EntTcFiltro filtro)
        {
            if (datos == null) { datos = new EntTcClasificado(); }
            if (filtro == null) { filtro = new EntTcFiltro(); }

            List<EntTcFila> todas = datos.Filas;
            List<EntTcFila> filtradas = todas.Where(r => Pasa(r, filtro, Ignorar.Nada)).ToList();
            List<EntTcFila> sinMes = todas.Where(r => Pasa(r, filtro, Ignorar.Mes)).ToList();
            List<EntTcFila> sinResponsable = todas.Where(r => Pasa(r, filtro, Ignorar.Responsable)).ToList();

            EntTableroCostos t = new EntTableroCostos();
            t.ParametrosPorDefecto = datos.ParametrosPorDefecto;

            t.Indicadores.Normal = Sumar(filtradas.Where(r => r.Tipo == Normal));
            t.Indicadores.Extra = Sumar(filtradas.Where(r => r.Tipo == Extra));
            t.Indicadores.Traslado = Sumar(filtradas.Where(r => r.Tipo == Traslado));
            t.Indicadores.Total = Sumar(filtradas);

            t.Resumen = ArmarResumen(filtradas);
            t.ExtrasPorEstado = ArmarExtrasPorEstado(filtradas);
            t.TrasladosPorEstado = ArmarTrasladosPorEstado(filtradas);

            foreach (int anio in todas.Select(r => r.Anio).Distinct().OrderBy(a => a))
            {
                t.PorAnio.Add(FilaPeriodo(anio.ToString(CultureInfo.InvariantCulture), filtradas.Where(r => r.Anio == anio)));
            }
            t.PorAnio.Add(FilaPeriodo("Total", filtradas));

            foreach (string periodo in PeriodosCronologicos(todas))
            {
                t.PorMes.Add(FilaPeriodo(periodo, sinMes.Where(r => r.Periodo == periodo)));
            }
            t.PorMes.Add(FilaPeriodo("Total", sinMes));

            t.TopNormal = Top(sinResponsable, Normal);
            t.TopExtra = Top(sinResponsable, Extra);
            t.TopTraslado = Top(sinResponsable, Traslado);

            t.Detalle = filtradas.OrderBy(r => r.FechaInicio).ThenBy(r => r.IdDetalle).ToList();

            t.SinSalario = filtradas.Where(r => r.SinSalario)
                                    .Select(r => r.Responsable)
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .OrderBy(n => n, OrdenTexto)
                                    .ToList();

            /* Listas dependientes: cada una ofrece los valores que tienen
               actividades con los DEMAS filtros elegidos. Si el valor elegido no
               esta, se avisa y se agrega igual, para que el combo lo siga
               mostrando en vez de saltar a "Todos" en silencio. */
            List<string> avisos = new List<string>();

            t.Meses = PeriodosCronologicos(todas.Where(r => Pasa(r, filtro, Ignorar.Mes)));
            Revisar(t.Meses, filtro.Mes, "El mes elegido no tiene actividades con los demás filtros. ", avisos);

            t.Responsables = Valores(todas, filtro, Ignorar.Responsable, r => r.Responsable);
            Revisar(t.Responsables, filtro.Responsable, "El responsable elegido no tiene actividades con los demás filtros. ", avisos);

            t.Empresas = Valores(todas, filtro, Ignorar.Empresa, r => r.Empresa);
            Revisar(t.Empresas, filtro.Empresa, "La empresa elegida no tiene actividades con los demás filtros. ", avisos);

            t.Contratos = Valores(todas, filtro, Ignorar.Contrato, r => r.Contrato);
            Revisar(t.Contratos, filtro.Contrato, "El contrato elegido no tiene actividades con los demás filtros. ", avisos);

            bool hayFiltro = Elegido(filtro.Mes) || Elegido(filtro.Responsable) || Elegido(filtro.Empresa)
                          || Elegido(filtro.Contrato) || Elegido(filtro.Recargo) || Elegido(filtro.EstadoHE);
            if (avisos.Count == 0 && hayFiltro && filtradas.Count == 0)
            {
                avisos.Add("Ninguna actividad cumple los filtros elegidos. ");
            }
            t.Aviso = avisos.Count == 0 ? "" : "⚠ " + string.Concat(avisos) + "Elija otro valor o Todos.";

            return t;
        }

        /// <summary>"" si el rango es valido; si no, el mensaje para el usuario.</summary>
        public static string ValidarRango(DateTime desde, DateTime hasta)
        {
            if (hasta.Date < desde.Date)
            {
                return "La fecha Hasta no puede ser anterior a Desde.";
            }
            if (hasta.Date >= desde.Date.AddMonths(MesesMaximos))
            {
                return "El rango no puede superar " + MesesMaximos.ToString(CultureInfo.InvariantCulture) + " meses.";
            }
            return "";
        }

        /// <summary>Texto de los filtros para la auditoria y la cabecera del Excel. Formato fijo, sin depender de la cultura.</summary>
        public static string DescribirFiltro(DateTime desde, DateTime hasta, EntTcFiltro filtro)
        {
            EntTcFiltro f = filtro ?? new EntTcFiltro();
            return "Desde " + desde.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture)
                 + " · Hasta " + hasta.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture)
                 + " · Mes: " + Mostrar(f.Mes)
                 + " · Responsable: " + Mostrar(f.Responsable)
                 + " · Empresa: " + Mostrar(f.Empresa)
                 + " · Contrato: " + Mostrar(f.Contrato)
                 + " · Recargo: " + Mostrar(f.Recargo)
                 + " · Estado HE: " + Mostrar(f.EstadoHE);
        }

        // ----------------------------------------------------- filtros

        private static bool Pasa(EntTcFila r, EntTcFiltro f, Ignorar ignorar)
        {
            if ((ignorar & Ignorar.Mes) == 0 && Elegido(f.Mes) && !Igual(r.Periodo, f.Mes)) { return false; }
            if ((ignorar & Ignorar.Responsable) == 0 && Elegido(f.Responsable) && !Igual(r.Responsable, f.Responsable)) { return false; }
            if ((ignorar & Ignorar.Empresa) == 0 && Elegido(f.Empresa) && !Igual(r.Empresa, f.Empresa)) { return false; }
            if ((ignorar & Ignorar.Contrato) == 0 && Elegido(f.Contrato) && !Igual(r.Contrato, f.Contrato)) { return false; }

            if (r.Tipo != Normal)
            {
                if (Elegido(f.Recargo) && !Igual(r.Recargo, f.Recargo)) { return false; }
                if (Elegido(f.EstadoHE) && !Igual(r.EstadoHE, f.EstadoHE)) { return false; }
            }
            return true;
        }

        private static bool Elegido(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor)
                && !string.Equals(valor.Trim(), Todos, StringComparison.OrdinalIgnoreCase);
        }

        private static bool Igual(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string Mostrar(string valor)
        {
            return Elegido(valor) ? valor.Trim() : Todos;
        }

        // ---------------------------------------------------- secciones

        private static EntTcCelda Sumar(IEnumerable<EntTcFila> filas)
        {
            EntTcCelda c = new EntTcCelda();
            foreach (EntTcFila r in filas)
            {
                c.Horas += r.Horas;
                c.Valor += r.Valor;
                c.Actividades++;
            }
            return c;
        }

        private static EntTcCelda Mas(params EntTcCelda[] celdas)
        {
            EntTcCelda c = new EntTcCelda();
            foreach (EntTcCelda x in celdas)
            {
                c.Horas += x.Horas;
                c.Valor += x.Valor;
                c.Actividades += x.Actividades;
            }
            return c;
        }

        private static List<EntTcResumenFila> ArmarResumen(List<EntTcFila> filas)
        {
            Func<string, EntTcCelda> cat = nombre => Sumar(filas.Where(r => Categoria(r) == nombre));

            EntTcCelda normal = cat(CatNormal);
            EntTcCelda extra50 = cat(CatExtra50);
            EntTcCelda extra100 = cat(CatExtra100);
            EntTcCelda trasladoSin = cat(CatTrasladoSin);
            EntTcCelda traslado50 = cat(CatTraslado50);
            EntTcCelda traslado100 = cat(CatTraslado100);
            EntTcCelda subExtra = Mas(extra50, extra100);
            EntTcCelda subTraslado = Mas(trasladoSin, traslado50, traslado100);
            EntTcCelda total = Mas(normal, subExtra, subTraslado);

            return new List<EntTcResumenFila>
            {
                FilaResumen(CatNormal, normal, total, false),
                FilaResumen(CatExtra50, extra50, total, false),
                FilaResumen(CatExtra100, extra100, total, false),
                FilaResumen("Subtotal Horas Extras", subExtra, total, true),
                FilaResumen(CatTrasladoSin, trasladoSin, total, false),
                FilaResumen(CatTraslado50, traslado50, total, false),
                FilaResumen(CatTraslado100, traslado100, total, false),
                FilaResumen("Subtotal Traslado", subTraslado, total, true),
                FilaResumen("Total", total, total, true),
                FilaResumen("Total al 50% (Extra + Traslado)", Mas(extra50, traslado50), total, true),
                FilaResumen("Total al 100% (Extra + Traslado)", Mas(extra100, traslado100), total, true)
            };
        }

        private static EntTcResumenFila FilaResumen(string concepto, EntTcCelda c, EntTcCelda total, bool subtotal)
        {
            return new EntTcResumenFila
            {
                Concepto = concepto,
                EsSubtotal = subtotal,
                Horas = c.Horas,
                PctHoras = Dividir(c.Horas, total.Horas),
                Valor = c.Valor,
                PctValor = Dividir(c.Valor, total.Valor),
                Actividades = c.Actividades,
                ValorPorHora = Dividir(c.Valor, c.Horas)
            };
        }

        private static decimal Dividir(decimal a, decimal b)
        {
            return b == 0m ? 0m : a / b;
        }

        private static List<EntTcEstadoExtra> ArmarExtrasPorEstado(List<EntTcFila> filas)
        {
            List<EntTcFila> extras = filas.Where(r => r.Tipo == Extra).ToList();
            List<EntTcEstadoExtra> resultado = EstadosExtra
                .Select(e => FilaEstadoExtra(e, extras.Where(r => r.EstadoHE == e)))
                .ToList();
            resultado.Add(FilaEstadoExtra("Total", extras));
            return resultado;
        }

        private static EntTcEstadoExtra FilaEstadoExtra(string estado, IEnumerable<EntTcFila> filas)
        {
            EntTcEstadoExtra e = new EntTcEstadoExtra { Estado = estado };
            foreach (EntTcFila r in filas)
            {
                if (r.Recargo == Recargo50) { e.Horas50 += r.Horas; e.Valor50 += r.Valor; }
                if (r.Recargo == Recargo100) { e.Horas100 += r.Horas; e.Valor100 += r.Valor; }
            }
            e.Total = e.Valor50 + e.Valor100;
            return e;
        }

        private static List<EntTcEstadoTraslado> ArmarTrasladosPorEstado(List<EntTcFila> filas)
        {
            List<EntTcFila> traslados = filas.Where(r => r.Tipo == Traslado).ToList();
            List<EntTcEstadoTraslado> resultado = EstadosTraslado
                .Select(e => FilaEstadoTraslado(e, traslados.Where(r => r.EstadoHE == e)))
                .ToList();
            resultado.Add(FilaEstadoTraslado("Total", traslados));
            return resultado;
        }

        private static EntTcEstadoTraslado FilaEstadoTraslado(string estado, IEnumerable<EntTcFila> filas)
        {
            EntTcEstadoTraslado e = new EntTcEstadoTraslado { Estado = estado };
            foreach (EntTcFila r in filas)
            {
                if (r.Recargo == SinRecargo) { e.HorasSinRecargo += r.Horas; }
                if (r.Recargo == Recargo50) { e.Horas50 += r.Horas; }
                if (r.Recargo == Recargo100) { e.Horas100 += r.Horas; }
                e.TotalHoras += r.Horas;
                e.TotalValor += r.Valor;
            }
            return e;
        }

        private static EntTcPeriodo FilaPeriodo(string etiqueta, IEnumerable<EntTcFila> filas)
        {
            EntTcPeriodo p = new EntTcPeriodo { Periodo = etiqueta };
            foreach (EntTcFila r in filas)
            {
                switch (Categoria(r))
                {
                    case CatNormal: p.HNormal += r.Horas; p.VNormal += r.Valor; break;
                    case CatExtra50: p.HExtra50 += r.Horas; p.VExtra50 += r.Valor; break;
                    case CatExtra100: p.HExtra100 += r.Horas; p.VExtra100 += r.Valor; break;
                    case CatTrasladoSin: p.HTrasladoSin += r.Horas; p.VTrasladoSin += r.Valor; break;
                    case CatTraslado50: p.HTraslado50 += r.Horas; p.VTraslado50 += r.Valor; break;
                    case CatTraslado100: p.HTraslado100 += r.Horas; p.VTraslado100 += r.Valor; break;
                }
                p.HTotal += r.Horas;
                p.VTotal += r.Valor;
            }
            return p;
        }

        private static List<EntTcTop> Top(IEnumerable<EntTcFila> filas, string tipo)
        {
            return filas.Where(r => r.Tipo == tipo)
                .GroupBy(r => r.Responsable)
                .Select(g => new EntTcTop
                {
                    Responsable = g.Key,
                    Horas = g.Sum(r => r.Horas),
                    HorasSinRecargo = g.Where(r => r.Recargo == SinRecargo).Sum(r => r.Horas),
                    Horas50 = g.Where(r => r.Recargo == Recargo50).Sum(r => r.Horas),
                    Horas100 = g.Where(r => r.Recargo == Recargo100).Sum(r => r.Horas),
                    Valor = g.Sum(r => r.Valor)
                })
                .OrderByDescending(x => x.Valor)
                .ThenByDescending(x => x.Horas)
                .ThenBy(x => x.Responsable, StringComparer.Ordinal)
                .Take(TopMaximo)
                .ToList();
        }

        // ------------------------------------------------------- listas

        private static List<string> PeriodosCronologicos(IEnumerable<EntTcFila> filas)
        {
            return filas.GroupBy(r => new { r.Anio, r.Mes, r.Periodo })
                        .OrderBy(g => g.Key.Anio)
                        .ThenBy(g => g.Key.Mes)
                        .Select(g => g.Key.Periodo)
                        .ToList();
        }

        private static List<string> Valores(List<EntTcFila> todas, EntTcFiltro f, Ignorar ignorar, Func<EntTcFila, string> campo)
        {
            return todas.Where(r => Pasa(r, f, ignorar))
                        .Select(campo)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(v => v, OrdenTexto)
                        .ToList();
        }

        private static void Revisar(List<string> lista, string elegido, string texto, List<string> avisos)
        {
            if (!Elegido(elegido) || lista.Contains(elegido.Trim(), StringComparer.OrdinalIgnoreCase)) { return; }
            avisos.Add(texto);
            lista.Add(elegido.Trim());
        }
    }
}
