using CapaEntidad;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Formato de CA_respuestas.valor_respuesta (especificacion, seccion 10.3),
    /// identico al del sistema original para que los datos migrados se lean
    /// igual: un string se guarda CRUDO; cualquier otro valor, en JSON. Al leer
    /// se intenta parsear JSON y, si falla, se usa el texto tal cual.
    ///
    /// Los nombres de propiedad de compromisos y decision (descripcion,
    /// categoria, responsable, fecha_limite, decision, nueva_fecha_fin,
    /// comentarios) y el formato de fecha yyyy-MM-dd son los historicos: no
    /// cambiarlos.
    /// </summary>
    public static class NegCurvaValorRespuesta
    {
        private const string FormatoFecha = "yyyy-MM-dd";

        /// <summary>string -&gt; tal cual; cualquier otra cosa -&gt; JSON.</summary>
        public static string Serializar(object valor)
        {
            var s = valor as string;
            return s ?? JsonConvert.SerializeObject(valor, Formatting.None);
        }

        public static string SerializarCompromisos(IEnumerable<EntCurvaCompromisoFila> filas)
        {
            var lista = filas.Select(f => new Dictionary<string, object>
            {
                { "descripcion", f.Descripcion },
                { "categoria", f.Categoria },
                { "responsable", f.Responsable },
                { "fecha_limite", f.FechaLimite.HasValue ? f.FechaLimite.Value.ToString(FormatoFecha, CultureInfo.InvariantCulture) : null }
            }).ToList();
            return Serializar(lista);
        }

        public static string SerializarDecision(EntCurvaDecisionInput d)
        {
            return Serializar(new Dictionary<string, object>
            {
                { "decision", d.Decision },
                { "nueva_fecha_fin", d.NuevaFechaFin.HasValue ? d.NuevaFechaFin.Value.ToString(FormatoFecha, CultureInfo.InvariantCulture) : null },
                { "comentarios", d.Comentarios }
            });
        }

        /// <summary>
        /// JSON tolerante. Sin conversion de fechas: "2026-01-01" debe seguir
        /// siendo texto, como en JavaScript.
        /// </summary>
        public static JToken Parsear(string crudo)
        {
            if (crudo == null) return null;
            try
            {
                using (var lector = new JsonTextReader(new StringReader(crudo)) { DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Double })
                {
                    var t = JToken.ReadFrom(lector);
                    if (lector.Read()) return new JValue(crudo);   // texto sobrante: no era JSON
                    return t;
                }
            }
            catch (JsonException) { return new JValue(crudo); }
        }

        /// <summary>
        /// El texto de una respuesta de opcion. Un valor como "123" o "true"
        /// parsea a numero/booleano (igual que en JS); en ese caso se devuelve
        /// el crudo.
        /// </summary>
        public static string ComoTexto(string crudo)
        {
            var t = Parsear(crudo);
            return t != null && t.Type == JTokenType.String ? (string)t : crudo;
        }

        /// <summary>Arreglo de textos (opcion multiple); un texto suelto da una lista de uno.</summary>
        public static List<string> ComoLista(string crudo)
        {
            var t = Parsear(crudo);
            if (t == null) return new List<string>();
            if (t.Type == JTokenType.Array)
                return t.Children().Where(x => x.Type != JTokenType.Null).Select(x => x.Type == JTokenType.String ? (string)x : x.ToString(Formatting.None)).ToList();
            var s = ComoTexto(crudo);
            return string.IsNullOrEmpty(s) ? new List<string>() : new List<string> { s };
        }

        /// <summary>{competencia: 1-5}. Vacio si no se puede leer; descarta valores no numericos.</summary>
        public static Dictionary<string, int> ComoCompetencias(string crudo)
        {
            var dict = new Dictionary<string, int>();
            var o = Parsear(crudo) as JObject;
            if (o == null) return dict;
            foreach (var p in o.Properties())
            {
                double v;
                if (p.Value.Type == JTokenType.Integer || p.Value.Type == JTokenType.Float) v = p.Value.Value<double>();
                else if (p.Value.Type == JTokenType.String && double.TryParse((string)p.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) { }
                else continue;
                dict[p.Name] = (int)v;
            }
            return dict;
        }

        public static List<EntCurvaCompromisoFila> ComoCompromisos(string crudo)
        {
            var lista = new List<EntCurvaCompromisoFila>();
            var a = Parsear(crudo) as JArray;
            if (a == null) return lista;
            foreach (var o in a.OfType<JObject>())
            {
                lista.Add(new EntCurvaCompromisoFila
                {
                    Descripcion = TextoDe(o, "descripcion"),
                    Categoria = TextoDe(o, "categoria"),
                    Responsable = TextoDe(o, "responsable"),
                    FechaLimite = Fecha(TextoDe(o, "fecha_limite"))
                });
            }
            return lista;
        }

        public static EntCurvaDecisionInput ComoDecision(string crudo)
        {
            var o = Parsear(crudo) as JObject;
            if (o == null) return null;
            return new EntCurvaDecisionInput
            {
                Decision = TextoDe(o, "decision"),
                NuevaFechaFin = Fecha(TextoDe(o, "nueva_fecha_fin")),
                Comentarios = TextoDe(o, "comentarios")
            };
        }

        /// <summary>El arreglo JSON de opciones de una pregunta, o null.</summary>
        public static List<string> Opciones(string opcionesJson)
        {
            if (string.IsNullOrEmpty(opcionesJson)) return null;
            var a = Parsear(opcionesJson) as JArray;
            if (a == null) return null;
            return a.Select(x => x.Type == JTokenType.String ? (string)x : x.ToString(Formatting.None)).ToList();
        }

        /// <summary>
        /// Una respuesta como texto legible (prompts de IA, PDF): opcion
        /// multiple unida con ", ", competencias como "X: 4/5, ...", el resto
        /// como texto.
        /// </summary>
        public static string Formatear(string tipo, string crudo)
        {
            if (crudo == null) return "";
            if (tipo == CurvaConstantes.TipoOpcionMultiple) return string.Join(", ", ComoLista(crudo));
            if (tipo == CurvaConstantes.TipoCompetencias)
                return string.Join(", ", ComoCompetencias(crudo).Select(kv => kv.Key + ": " + kv.Value + "/5"));
            return ComoTexto(crudo);
        }

        /// <summary>yyyy-MM-dd estricto (lo que envia un input type=date); null si no.</summary>
        public static DateTime? Fecha(string texto)
        {
            DateTime d;
            if (string.IsNullOrWhiteSpace(texto)) return null;
            var t = texto.Trim();
            if (t.Length > 10) t = t.Substring(0, 10);
            return DateTime.TryParseExact(t, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d : (DateTime?)null;
        }

        private static string TextoDe(JObject o, string clave)
        {
            JToken t;
            if (!o.TryGetValue(clave, out t) || t.Type == JTokenType.Null) return null;
            return t.Type == JTokenType.String ? (string)t : t.ToString(Formatting.None);
        }
    }
}
