using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>Reglas puras del bono: gente a cargo, identificacion, valor, doble pago, permisos y cierre.</summary>
    public static class NegBonoReglas
    {
        private static readonly CultureInfo Cultura = new CultureInfo("es-EC");
        private const string SinMesAbierto = "No hay un mes de bonos abierto.";

        /// <summary>Trim + mayusculas; null -> "".</summary>
        public static string Normalizar(string cod)
        {
            return (cod ?? "").Trim().ToUpperInvariant();
        }

        /// <summary>Reportes directos de <paramref name="yo"/>, sin el mismo, ordenados por nombre.</summary>
        public static List<EntBonoPersona> MiGente(IEnumerable<EntBonoPersona> activas, string yo)
        {
            string miCod = Normalizar(yo);
            if (activas == null || miCod.Length == 0) return new List<EntBonoPersona>();
            return activas
                .Where(p => Normalizar(p.CodJefe) == miCod && Normalizar(p.CodUsuario) != miCod)
                .OrderBy(p => p.Nombre ?? "", StringComparer.Create(Cultura, true))
                .ToList();
        }

        /// <summary>"DOS" o "AGILITY"; "" si no es valida.</summary>
        public static string EmpresaNormalizada(string empresa)
        {
            string e = (empresa ?? "").Trim().ToUpperInvariant();
            return e == "DOS" || e == "AGILITY" ? e : "";
        }

        /// <summary>Las sucursales del combo Lugar, escritas como en los bonos migrados.</summary>
        public static readonly string[] Lugares = { "Guayaquil", "Quito", "Cuenca", "Ambato" };

        /// <summary>La sucursal escrita como en <see cref="Lugares"/>; "" si viene vacio; null si no es una sucursal.</summary>
        public static string LugarNormalizado(string lugar)
        {
            string l = (lugar ?? "").Trim();
            if (l.Length == 0) return "";
            return Lugares.FirstOrDefault(s => string.Equals(s, l, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Codigo SAP (DOS) o cedula de 10 digitos (AGILITY). Vacio y <paramref name="error"/> si falta el dato.</summary>
        public static string Identificacion(EntBonoPersona p, out string error)
        {
            error = null;
            string empresa = EmpresaNormalizada(p.Empresa);
            if (empresa == "DOS")
            {
                string sap = (p.CodSap ?? "").Trim();
                if (sap.Length == 0)
                {
                    error = "Falta el código SAP de " + p.Nombre + " en Administración de Usuarios.";
                    return "";
                }
                return sap;
            }
            if (empresa == "AGILITY")
            {
                string cedula = new string((p.Cedula ?? "").Where(char.IsDigit).ToArray());
                if (cedula.Length == 0)
                {
                    error = "Falta la cédula de " + p.Nombre + " en Administración de Usuarios.";
                    return "";
                }
                return cedula.PadLeft(10, '0');
            }
            error = "La empresa de " + p.Nombre + " no es DOS ni AGILITY: corríjala en Administración de Usuarios.";
            return "";
        }

        /// <summary>Por semana: tarifa x semanas; si no, el sugerido del tipo. Null si el tipo no tiene sugerido.</summary>
        public static decimal? Sugerido(EntBonoTipo t, int semanas)
        {
            if (t == null || !t.ValorSugerido.HasValue) return null;
            return t.PorSemana ? t.ValorSugerido.Value * semanas : t.ValorSugerido.Value;
        }

        /// <summary>null = valido; si no, el mensaje. Valor &gt; 0; si difiere del sugerido, el motivo es obligatorio.</summary>
        public static string ValidarValor(EntBonoTipo t, int semanas, decimal valor, string motivo)
        {
            if (valor <= 0m) return "El valor debe ser mayor que cero.";
            decimal? sugerido = Sugerido(t, semanas);
            if (sugerido.HasValue && valor != sugerido.Value && string.IsNullOrWhiteSpace(motivo))
                return "El valor es distinto del sugerido ($" + sugerido.Value.ToString("0.00", Cultura) + "): escriba el motivo del ajuste.";
            return null;
        }

        /// <summary>Lunes de <paramref name="b"/> ya tomados por otro bono no rechazado de la misma persona y tipo, en cualquier periodo.</summary>
        public static List<DateTime> ChocaDoblePago(EntBono b, IEnumerable<EntBono> otros)
        {
            if (b == null || otros == null || b.Lunes == null) return new List<DateTime>();
            string persona = Normalizar(b.CodUsuario);
            HashSet<DateTime> tomados = new HashSet<DateTime>(
                otros.Where(o => o.IdBono != b.IdBono
                              && o.IdTipo == b.IdTipo
                              && o.Estado != BonoEstados.Rechazado
                              && Normalizar(o.CodUsuario) == persona
                              && o.Lunes != null)
                     .SelectMany(o => o.Lunes.Select(l => l.Date)));
            return b.Lunes.Select(l => l.Date).Distinct().Where(tomados.Contains).OrderBy(l => l).ToList();
        }

        /// <summary>Texto para el usuario: "semana del 4 de agosto, semana del 11 de agosto".</summary>
        public static string MensajeDoblePago(IEnumerable<DateTime> lunes)
        {
            IEnumerable<string> semanas = (lunes ?? new DateTime[0])
                .Select(l => l.Date).Distinct().OrderBy(l => l)
                .Select(l => "semana del " + l.Day + " de " + Cultura.DateTimeFormat.GetMonthName(l.Month).ToLower(Cultura));
            return "Ya hay otro bono de este tipo para esta persona en la " + string.Join(", ", semanas) + ".";
        }

        /// <summary>Aviso (pide confirmar, no bloquea) si el tipo no semanal ya existe para la persona en el periodo.</summary>
        public static string AvisoRepetido(EntBono b, EntBonoTipo t, IEnumerable<EntBono> delPeriodo)
        {
            if (b == null || t == null || t.PorSemana || delPeriodo == null) return null;
            string persona = Normalizar(b.CodUsuario);
            bool repetido = delPeriodo.Any(o => o.IdBono != b.IdBono
                                             && o.IdTipo == b.IdTipo
                                             && o.IdPeriodo == b.IdPeriodo
                                             && o.Estado != BonoEstados.Rechazado
                                             && Normalizar(o.CodUsuario) == persona);
            return repetido
                ? "Esta persona ya tiene otro bono «" + t.Nombre + "» este mes. ¿Desea registrarlo de todos modos?"
                : null;
        }

        private static bool EstaAbierto(EntBonoPeriodo p)
        {
            return p != null && p.Estado == BonoPeriodoEstados.Abierto;
        }

        private static bool EsMiGente(ISet<string> miGente, string cod)
        {
            string c = Normalizar(cod);
            return miGente != null && miGente.Any(g => Normalizar(g) == c);
        }

        public static string PuedeCrear(EntBonoPeriodo abierto, ISet<string> miGente, string cod)
        {
            if (!EstaAbierto(abierto)) return SinMesAbierto;
            if (!EsMiGente(miGente, cod)) return "Esa persona no está entre su gente a cargo.";
            return null;
        }

        public static string PuedeEditar(EntBono b, EntBonoPeriodo p, string yo, ISet<string> miGente)
        {
            if (!EstaAbierto(p)) return SinMesAbierto;
            if (b.IdPeriodo.HasValue && b.IdPeriodo.Value != p.IdPeriodo) return "El bono no pertenece al mes abierto.";
            if (Normalizar(b.CodJefe) != Normalizar(yo) || Normalizar(yo).Length == 0 || !EsMiGente(miGente, b.CodUsuario))
                return "Este bono no es de una persona a su cargo.";
            if (b.Estado != BonoEstados.Borrador && b.Estado != BonoEstados.Rechazado)
                return "Solo se pueden editar bonos en borrador o rechazados.";
            return null;
        }

        public static string PuedeEnviar(EntBono b, EntBonoPeriodo p, string yo, ISet<string> miGente)
        {
            if (!EstaAbierto(p)) return SinMesAbierto;
            if (b.IdPeriodo.HasValue && b.IdPeriodo.Value != p.IdPeriodo) return "El bono no pertenece al mes abierto.";
            if (Normalizar(b.CodJefe) != Normalizar(yo) || Normalizar(yo).Length == 0 || !EsMiGente(miGente, b.CodUsuario))
                return "Este bono no es de una persona a su cargo.";
            if (b.Estado != BonoEstados.Borrador)
                return "Solo se pueden enviar bonos en borrador.";
            return null;
        }

        /// <summary>
        /// Borrar es solo de quien lo propuso (CodJefe), aunque la persona ya no
        /// este a su cargo: un borrador huerfano bloquea sus semanas y nadie mas
        /// lo veria. Editar y enviar siguen exigiendo que sea de mi gente.
        /// </summary>
        public static string PuedeBorrar(EntBono b, EntBonoPeriodo p, string yo)
        {
            if (!EstaAbierto(p)) return SinMesAbierto;
            if (b.IdPeriodo.HasValue && b.IdPeriodo.Value != p.IdPeriodo) return "El bono no pertenece al mes abierto.";
            if (Normalizar(yo).Length == 0 || Normalizar(b.CodJefe) != Normalizar(yo))
                return "Este bono no lo propuso usted.";
            if (b.Estado != BonoEstados.Borrador && b.Estado != BonoEstados.Rechazado)
                return "Solo se pueden borrar bonos en borrador o rechazados.";
            return null;
        }

        /// <summary>
        /// accion = Aprobar | Ajustar | Rechazar. Aprobar solo desde Enviado; Ajustar,
        /// desde Enviado o Aprobado; Rechazar, tambien desde Borrador (libera las
        /// semanas de un borrador que su jefe ya no puede tocar).
        /// </summary>
        public static string PuedeRevisar(EntBono b, EntBonoPeriodo p, string accion)
        {
            if (!EstaAbierto(p)) return SinMesAbierto;
            switch (accion)
            {
                case "Aprobar":
                    return b.Estado == BonoEstados.Enviado ? null : "Solo se pueden aprobar bonos enviados.";
                case "Ajustar":
                    return b.Estado == BonoEstados.Enviado || b.Estado == BonoEstados.Aprobado
                        ? null
                        : "Solo se pueden ajustar bonos enviados o aprobados.";
                case "Rechazar":
                    return b.Estado == BonoEstados.Enviado || b.Estado == BonoEstados.Aprobado || b.Estado == BonoEstados.Borrador
                        ? null
                        : "Solo se pueden rechazar bonos en borrador, enviados o aprobados.";
                default:
                    return "Acción no válida.";
            }
        }

        /// <summary>Editar un rechazado lo devuelve a Borrador; los demas conservan su estado.</summary>
        public static string EstadoAlEditar(string estadoActual)
        {
            return estadoActual == BonoEstados.Rechazado ? BonoEstados.Borrador : estadoActual;
        }

        /// <summary>Lo que se ve antes de cerrar: lo que queda fuera, tipos x empresa sin concepto SAP y el total aprobado.</summary>
        public static EntBonoCierre Cierre(IEnumerable<EntBono> delPeriodo, IEnumerable<EntBonoTipo> tipos)
        {
            List<EntBono> todos = (delPeriodo ?? new EntBono[0]).ToList();
            Dictionary<int, EntBonoTipo> catalogo = (tipos ?? new EntBonoTipo[0]).GroupBy(t => t.IdTipo).ToDictionary(g => g.Key, g => g.First());
            List<EntBono> aprobados = todos.Where(b => b.Estado == BonoEstados.Aprobado).ToList();

            EntBonoCierre c = new EntBonoCierre();
            c.QuedanFuera = todos.Where(b => b.Estado != BonoEstados.Aprobado).ToList();
            c.Total = aprobados.Sum(b => b.Valor);

            HashSet<string> vistos = new HashSet<string>();
            foreach (EntBono b in aprobados)
            {
                EntBonoTipo t;
                catalogo.TryGetValue(b.IdTipo, out t);
                string empresa = EmpresaNormalizada(b.Empresa);
                string codigo = null;
                if (t != null && t.Conceptos != null && empresa.Length > 0) t.Conceptos.TryGetValue(empresa, out codigo);
                if (!string.IsNullOrWhiteSpace(codigo)) continue;

                string nombre = t != null ? t.Nombre : (string.IsNullOrEmpty(b.NombreTipo) ? "Tipo " + b.IdTipo : b.NombreTipo);
                string etiqueta = nombre + " (" + (empresa.Length > 0 ? empresa : (b.Empresa ?? "").Trim().ToUpperInvariant()) + ")";
                if (vistos.Add(etiqueta)) c.SinConcepto.Add(etiqueta);
            }
            c.PuedeCerrar = c.SinConcepto.Count == 0;
            return c;
        }

        /// <summary>Historico: TotalHistorico; si no, la suma de lo aprobado.</summary>
        public static decimal Total(EntBonoPeriodo p, IEnumerable<EntBono> delPeriodo)
        {
            if (p != null && p.Estado == BonoPeriodoEstados.Historico) return p.TotalHistorico ?? 0m;
            return (delPeriodo ?? new EntBono[0]).Where(b => b.Estado == BonoEstados.Aprobado).Sum(b => b.Valor);
        }

        /// <summary>Fraccion contra el mes anterior; null si no hay anterior o es cero.</summary>
        public static decimal? Variacion(decimal actual, decimal? anterior)
        {
            if (!anterior.HasValue || anterior.Value == 0m) return null;
            return (actual - anterior.Value) / anterior.Value;
        }
    }
}
