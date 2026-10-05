using CapaEntidad;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CapaNegocio
{
    /// <summary>
    /// Convierte lo que envia el formulario (JSON armado por curvaFormulario.js)
    /// en lo que se guarda, validando cada valor contra SU pregunta. Clase pura:
    /// sin base de datos.
    ///
    /// Forma del payload:
    ///   { "respuestas":   { "&lt;idPregunta&gt;": valor, ... },
    ///     "seguimientos": [ { "compromiso_id": n, "estado": "...", "justificacion": "..." } ] }
    ///
    /// Las claves que no corresponden a preguntas de la evaluacion se ignoran.
    /// Las preguntas sin respuesta no se guardan (igual que el sistema
    /// original: no hay obligatoriedad).
    /// </summary>
    public static class NegCurvaFormulario
    {
        /// <summary>Opcion "Otro", "Otros", "Otros (especificar)" (seccion 10.3).</summary>
        private static readonly Regex ReOtro = new Regex(@"^otros?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex ReConclusion = new Regex("conclusi", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool EsOtro(string opcion)
        {
            return opcion != null && ReOtro.IsMatch(opcion.Trim());
        }

        /// <summary>"Otro: texto", o solo la opcion si el texto viene vacio.</summary>
        public static string ValorConOtro(string opcion, string textoOtro)
        {
            var t = (textoOtro ?? "").Trim();
            return EsOtro(opcion) && t.Length > 0 ? opcion + ": " + t : opcion;
        }

        /// <summary>
        /// true si el valor es una opcion exacta, o "&lt;opcion Otro&gt;: texto".
        /// Comparacion ordinal: nada de cultura ni Trim (seccion 17).
        /// </summary>
        public static bool ValorValido(string valor, List<string> opciones)
        {
            if (valor == null || opciones == null) return false;
            foreach (var o in opciones)
            {
                if (string.Equals(valor, o, StringComparison.Ordinal)) return true;
                if (EsOtro(o) && valor.StartsWith(o + ": ", StringComparison.Ordinal)) return true;
            }
            return false;
        }

        /// <summary>
        /// La respuesta (si es texto) a la PRIMERA pregunta de texto abierto
        /// cuyo enunciado diga "conclusi..."; null si no hay.
        /// </summary>
        public static string CalcularConclusion(IEnumerable<EntCurvaPregunta> preguntas, IDictionary<int, string> valores)
        {
            var p = preguntas.FirstOrDefault(x => x.TipoRespuesta == CurvaConstantes.TipoTextoAbierto
                                                  && ReConclusion.IsMatch(x.TextoPregunta ?? ""));
            if (p == null) return null;
            string v;
            if (!valores.TryGetValue(p.Id, out v) || v == null) return null;
            return NegCurvaValorRespuesta.ComoTexto(v);
        }

        /// <summary>
        /// Normaliza el payload contra las preguntas de la evaluacion. Lanza
        /// ArgumentException con un texto para el usuario si el JSON no se
        /// puede leer o un valor no corresponde a su pregunta.
        /// </summary>
        public static EntCurvaGuardado Normalizar(List<EntCurvaPregunta> preguntas, string payloadJson)
        {
            JObject raiz;
            try { raiz = JObject.Parse(string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson); }
            catch (Exception) { throw new ArgumentException("El formulario enviado no es válido."); }

            var g = new EntCurvaGuardado();
            var respuestas = raiz["respuestas"] as JObject ?? new JObject();

            foreach (var p in preguntas)
            {
                if (p.Opciones == null) p.Opciones = NegCurvaValorRespuesta.Opciones(p.OpcionesJson);
                JToken v = respuestas[p.Id.ToString(CultureInfo.InvariantCulture)];

                switch (p.TipoRespuesta)
                {
                    case CurvaConstantes.TipoEscala:
                    case CurvaConstantes.TipoOpcionUnica:
                    case CurvaConstantes.TipoSiNo:
                        {
                            var s = Texto(v);
                            if (string.IsNullOrEmpty(s)) break;
                            if (!ValorValido(s, p.Opciones)) throw Invalido(p);
                            g.Valores[p.Id] = s;
                            break;
                        }
                    case CurvaConstantes.TipoOpcionMultiple:
                        {
                            var lista = (v as JArray ?? new JArray()).Select(Texto).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                            if (lista.Count == 0) break;
                            if (lista.Any(x => !ValorValido(x, p.Opciones))) throw Invalido(p);
                            g.Valores[p.Id] = NegCurvaValorRespuesta.Serializar(lista);
                            break;
                        }
                    case CurvaConstantes.TipoTextoAbierto:
                        {
                            var s = (Texto(v) ?? "").Trim();
                            if (s.Length > 0) g.Valores[p.Id] = s;
                            break;
                        }
                    case CurvaConstantes.TipoCompetencias:
                        {
                            var o = v as JObject;
                            if (o == null) break;
                            var dict = new Dictionary<string, int>();
                            foreach (var comp in p.Opciones ?? new List<string>())
                            {
                                JToken n = o[comp];
                                int nivel;
                                if (n == null || n.Type == JTokenType.Null) continue;
                                if (!int.TryParse(Texto(n), NumberStyles.Integer, CultureInfo.InvariantCulture, out nivel) || nivel < 1 || nivel > 5)
                                    throw Invalido(p);
                                dict[comp] = nivel;
                            }
                            if (dict.Count > 0) g.Valores[p.Id] = NegCurvaValorRespuesta.Serializar(dict);
                            break;
                        }
                    case CurvaConstantes.TipoCompromisos:
                        {
                            /* Si la pregunta existe en el formulario, la lista se
                               reemplaza siempre, aunque venga vacia (seccion 18,
                               punto 3): por eso Compromisos deja de ser null. */
                            var filas = new List<EntCurvaCompromisoFila>();
                            foreach (var o in (v as JArray ?? new JArray()).OfType<JObject>())
                            {
                                var desc = (Texto(o["descripcion"]) ?? "").Trim();
                                if (desc.Length == 0) continue;
                                var cat = (Texto(o["categoria"]) ?? "").Trim();
                                if (cat == "Otro")
                                {
                                    var otro = (Texto(o["categoria_otro"]) ?? "").Trim();
                                    cat = otro.Length > 0 ? otro : "Otro";
                                }
                                var resp = (Texto(o["responsable"]) ?? "").Trim();
                                filas.Add(new EntCurvaCompromisoFila
                                {
                                    Descripcion = Recortar(desc, 1000),
                                    Categoria = cat.Length > 0 ? Recortar(cat, 200) : null,
                                    Responsable = resp.Length > 0 ? Recortar(resp, 200) : null,
                                    FechaLimite = NegCurvaValorRespuesta.Fecha(Texto(o["fecha_limite"]))
                                });
                            }
                            g.Compromisos = filas;
                            g.Valores[p.Id] = NegCurvaValorRespuesta.SerializarCompromisos(filas);
                            break;
                        }
                    case CurvaConstantes.TipoDecision:
                        {
                            var o = v as JObject;
                            if (o == null) break;
                            var dec = (Texto(o["decision"]) ?? "").Trim();
                            if (dec.Length == 0) break;
                            if (!CurvaConstantes.Decisiones.Contains(dec)) throw Invalido(p);
                            var com = (Texto(o["comentarios"]) ?? "").Trim();
                            var d = new EntCurvaDecisionInput
                            {
                                Decision = dec,
                                NuevaFechaFin = dec == CurvaConstantes.ProcesoExtendido ? NegCurvaValorRespuesta.Fecha(Texto(o["nueva_fecha_fin"])) : null,
                                Comentarios = com.Length > 0 ? com : null
                            };
                            g.Decision = d;
                            g.Valores[p.Id] = NegCurvaValorRespuesta.SerializarDecision(d);
                            break;
                        }
                }
            }

            foreach (var o in (raiz["seguimientos"] as JArray ?? new JArray()).OfType<JObject>())
            {
                int id;
                var estado = (Texto(o["estado"]) ?? "").Trim();
                if (!int.TryParse(Texto(o["compromiso_id"]), NumberStyles.Integer, CultureInfo.InvariantCulture, out id) || id <= 0) continue;
                if (!CurvaConstantes.EstadosSeguimiento.Contains(estado)) continue;
                var j = (Texto(o["justificacion"]) ?? "").Trim();
                g.Seguimientos.Add(new EntCurvaSeguimientoInput { CompromisoId = id, Estado = estado, Justificacion = j.Length > 0 ? Recortar(j, 1000) : null });
            }

            g.Conclusion = CalcularConclusion(preguntas, g.Valores);
            return g;
        }

        private static ArgumentException Invalido(EntCurvaPregunta p)
        {
            return new ArgumentException("La respuesta a «" + p.TextoPregunta + "» no es una opción válida.");
        }

        private static string Texto(JToken t)
        {
            if (t == null || t.Type == JTokenType.Null) return null;
            return t.Type == JTokenType.String ? (string)t : t.ToString(Newtonsoft.Json.Formatting.None);
        }

        private static string Recortar(string s, int max)
        {
            return s.Length > max ? s.Substring(0, max) : s;
        }
    }
}
