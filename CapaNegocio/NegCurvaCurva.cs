using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Curva de aprendizaje, alerta general y semaforo por etapa
    /// (especificacion, seccion 7.6). Clase pura: recibe las evaluaciones ya
    /// leidas, no persiste nada. Se calcula al vuelo cada vez.
    ///
    /// Tiene que dar EXACTAMENTE lo mismo que el sistema original (prueba 19:
    /// regresion de indices con colaboradores migrados), de ahi RedondearJs.
    /// </summary>
    public static class NegCurvaCurva
    {
        /// <summary>
        /// Math.round de JavaScript: .5 hacia arriba. Math.Round de C# usa
        /// redondeo bancario (72.5 -&gt; 72) y daria otros indices y otros
        /// semaforos que el sistema original.
        /// </summary>
        public static int RedondearJs(double x)
        {
            return (int)Math.Floor(x + 0.5);
        }

        /// <summary>&lt; 70 seguimiento, 70-84 atencion, &gt;= 85 ok.</summary>
        public static string Nivel(int indice)
        {
            if (indice < CurvaConstantes.UmbralSeguimiento) return CurvaConstantes.NivelSeguimiento;
            if (indice < CurvaConstantes.UmbralAtencion) return CurvaConstantes.NivelAtencion;
            return CurvaConstantes.NivelOk;
        }

        /// <summary>
        /// Indice 0-100 de UNA evaluacion completada, o null si no tiene
        /// ningun valor que lo alimente.
        ///   A: posicion de escala/opcion_unica (primera opcion = 1, ultima = 0).
        ///   B: seguimientos registrados en esa evaluacion (1 / .5 / .25 / 0).
        ///   C: competencias 1-5 (solo jefe; la primera respuesta de ese tipo).
        /// Colaborador: promedio(A u B) x 100. Jefe: promedio(A x5 u C u B x5) / 5 x 100.
        /// </summary>
        public static int? IndiceEvaluacion(EntCurvaInsumoEvaluacion ev)
        {
            var posiciones = new List<double>();
            foreach (var r in ev.Respuestas.Where(x => x.TipoRespuesta == CurvaConstantes.TipoEscala || x.TipoRespuesta == CurvaConstantes.TipoOpcionUnica))
            {
                var opciones = NegCurvaValorRespuesta.Opciones(r.OpcionesJson);
                if (opciones == null || opciones.Count < 2) continue;
                string valor = NegCurvaValorRespuesta.ComoTexto(r.ValorRespuesta);
                int idx = opciones.FindIndex(o => string.Equals(o, valor, StringComparison.Ordinal));
                if (idx >= 0) posiciones.Add(1.0 - (double)idx / (opciones.Count - 1));
            }

            var scoresComp = ev.EstadosSeguimiento
                .Where(s => s != null && CurvaConstantes.ScoreSeguimiento.ContainsKey(s))
                .Select(s => CurvaConstantes.ScoreSeguimiento[s]).ToList();

            if (ev.Rol == CurvaConstantes.RolColaborador)
            {
                var valores = posiciones.Concat(scoresComp).ToList();
                if (valores.Count == 0) return null;
                return RedondearJs(valores.Average() * 100);
            }

            var scores = posiciones.Select(p => p * 5).ToList();
            var comp = ev.Respuestas.FirstOrDefault(x => x.TipoRespuesta == CurvaConstantes.TipoCompetencias);
            if (comp != null) scores.AddRange(NegCurvaValorRespuesta.ComoCompetencias(comp.ValorRespuesta).Values.Select(v => (double)v));
            scores.AddRange(scoresComp.Select(s => s * 5));
            if (scores.Count == 0) return null;
            return RedondearJs(scores.Average() / 5 * 100);
        }

        /// <summary>La curva de un proceso a partir de sus evaluaciones COMPLETADAS.</summary>
        public static EntCurvaSerie Calcular(IEnumerable<EntCurvaInsumoEvaluacion> completadas)
        {
            var serie = new EntCurvaSerie();
            foreach (var ev in completadas)
            {
                int? indice = IndiceEvaluacion(ev);
                if (!indice.HasValue) continue;
                var punto = new EntCurvaPunto { Etapa = ev.EtapaCodigo, Indice = indice.Value };
                if (ev.Rol == CurvaConstantes.RolColaborador) serie.Colaborador.Add(punto); else serie.Jefe.Add(punto);
            }
            serie.Colaborador = serie.Colaborador.OrderBy(p => OrdenEtapa(p.Etapa)).ToList();
            serie.Jefe = serie.Jefe.OrderBy(p => OrdenEtapa(p.Etapa)).ToList();
            return serie;
        }

        /// <summary>
        /// Alerta general: el ULTIMO punto de cada rol. Sin puntos -&gt; Nivel null
        /// ("sin datos todavia", que NO es ok). Si alguno esta en seguimiento,
        /// gana seguimiento y solo se listan esos en el motivo.
        /// </summary>
        public static EntCurvaAlerta CalcularAlerta(EntCurvaSerie serie)
        {
            var ultimos = new List<KeyValuePair<string, EntCurvaPunto>>();
            if (serie.Colaborador.Count > 0) ultimos.Add(new KeyValuePair<string, EntCurvaPunto>("Colaborador", serie.Colaborador.Last()));
            if (serie.Jefe.Count > 0) ultimos.Add(new KeyValuePair<string, EntCurvaPunto>("Jefe", serie.Jefe.Last()));
            if (ultimos.Count == 0) return new EntCurvaAlerta { Nivel = null, Motivo = null };

            var bajos = ultimos.Where(u => Nivel(u.Value.Indice) != CurvaConstantes.NivelOk).ToList();
            if (bajos.Count == 0) return new EntCurvaAlerta { Nivel = CurvaConstantes.NivelOk, Motivo = null };

            var graves = bajos.Where(u => Nivel(u.Value.Indice) == CurvaConstantes.NivelSeguimiento).ToList();
            var listados = graves.Count > 0 ? graves : bajos;
            return new EntCurvaAlerta
            {
                Nivel = graves.Count > 0 ? CurvaConstantes.NivelSeguimiento : CurvaConstantes.NivelAtencion,
                Motivo = string.Join("; ", listados.Select(u => u.Key + " calificó " + u.Value.Indice + "/100 en etapa " + u.Value.Etapa))
            };
        }

        /// <summary>
        /// Semaforo de cada etapa con al menos un punto, en orden. Existe para
        /// que una etapa anterior mala siga visible aunque la ultima este en verde.
        /// </summary>
        public static List<EntCurvaAlertaEtapa> AlertaPorEtapa(EntCurvaSerie serie)
        {
            var lista = new List<EntCurvaAlertaEtapa>();
            foreach (var etapa in CurvaConstantes.EtapasOrden)
            {
                var c = serie.Colaborador.FirstOrDefault(p => p.Etapa == etapa);
                var j = serie.Jefe.FirstOrDefault(p => p.Etapa == etapa);
                if (c == null && j == null) continue;
                var item = new EntCurvaAlertaEtapa
                {
                    Etapa = etapa,
                    Colaborador = c == null ? null : new EntCurvaNivelRol { Indice = c.Indice, Nivel = Nivel(c.Indice) },
                    Jefe = j == null ? null : new EntCurvaNivelRol { Indice = j.Indice, Nivel = Nivel(j.Indice) }
                };
                item.Nivel = Peor(item.Colaborador == null ? null : item.Colaborador.Nivel, item.Jefe == null ? null : item.Jefe.Nivel);
                lista.Add(item);
            }
            return lista;
        }

        /// <summary>El indice de un rol en una etapa, o null.</summary>
        public static int? IndiceDe(EntCurvaSerie serie, string etapa, string rol)
        {
            var lista = rol == CurvaConstantes.RolJefe ? serie.Jefe : serie.Colaborador;
            var p = lista.FirstOrDefault(x => x.Etapa == etapa);
            return p == null ? (int?)null : p.Indice;
        }

        /// <summary>
        /// Competencias del jefe por etapa: claves fijas "90" y "365"; valor =
        /// la respuesta competencias_multi de la evaluacion del jefe completada
        /// de esa etapa, o null.
        /// </summary>
        public static Dictionary<string, Dictionary<string, int>> CompetenciasJefe(IEnumerable<EntCurvaInsumoEvaluacion> completadas)
        {
            var dict = new Dictionary<string, Dictionary<string, int>> { { "90", null }, { "365", null } };
            foreach (var ev in completadas.Where(e => e.Rol == CurvaConstantes.RolJefe && dict.ContainsKey(e.EtapaCodigo)))
            {
                var r = ev.Respuestas.FirstOrDefault(x => x.TipoRespuesta == CurvaConstantes.TipoCompetencias);
                if (r != null) dict[ev.EtapaCodigo] = NegCurvaValorRespuesta.ComoCompetencias(r.ValorRespuesta);
            }
            return dict;
        }

        public static int OrdenEtapa(string codigo)
        {
            int i = Array.IndexOf(CurvaConstantes.EtapasOrden, codigo);
            return i < 0 ? int.MaxValue : i;
        }

        private static string Peor(string a, string b)
        {
            Func<string, int> peso = n => n == CurvaConstantes.NivelSeguimiento ? 2 : n == CurvaConstantes.NivelAtencion ? 1 : n == CurvaConstantes.NivelOk ? 0 : -1;
            return peso(a) >= peso(b) ? a : b;
        }
    }
}
