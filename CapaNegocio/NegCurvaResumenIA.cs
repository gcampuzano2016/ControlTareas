using CapaEntidad;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    /// <summary>
    /// Resumenes ejecutivos con IA (especificacion, seccion 13), por HTTP
    /// directo contra la Messages API de Anthropic: el SDK oficial de C# no
    /// apunta a .NET Framework 4.6.1.
    ///
    /// NUNCA lanza: completar o editar una evaluacion no puede fallar por la
    /// IA. Sin la variable de entorno ANTHROPIC_API_KEY devuelve el aviso de la
    /// seccion 13.4, y asi queda el modulo hasta que TI/Legal aprueben una
    /// cuenta corporativa (seccion 13.5: los datos de RRHH no salen a una nube
    /// de terceros sin cuenta administrada). La clave no va en web.config.
    ///
    /// Parametros en appsettings.config (opcionales): CA.IA.Modelo,
    /// CA.IA.Effort, CA.IA.TimeoutSegundos.
    ///
    /// TLS 1.2 se fuerza en Global.asax (Application_Start): .NET 4.6.1 puede
    /// no negociarlo por si solo.
    /// </summary>
    public static class NegCurvaResumenIA
    {
        public const string SinApiKey = "Resumen ejecutivo no disponible: configura la variable de entorno ANTHROPIC_API_KEY para activar la generación automática con IA.";
        public const string ErrorIndividual = "No se pudo generar el resumen ejecutivo automáticamente";
        public const string ErrorAnual = "No se pudo generar el resumen ejecutivo anual automáticamente";
        public const string ErrorRango = "No se pudo generar el resumen ejecutivo del rango automáticamente";

        private static readonly HttpClient Http = CrearHttp();   // una sola instancia: por peticion agota sockets

        private static HttpClient CrearHttp()
        {
            var h = new HttpClient { BaseAddress = new Uri("https://api.anthropic.com/"), Timeout = TimeSpan.FromSeconds(Config("CA.IA.TimeoutSegundos", 120)) };
            h.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            // fallbacks "default": si el modelo rechaza por politica, la API reintenta con el modelo recomendado.
            h.DefaultRequestHeaders.Add("anthropic-beta", "server-side-fallback-2026-07-01");
            return h;
        }

        public static bool Activa
        {
            get { return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")); }
        }

        /// <summary>El texto del modelo, o un texto de aviso/error. Nunca lanza.</summary>
        public static async Task<string> GenerarAsync(string system, string usuario, string mensajeError)
        {
            string apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            if (string.IsNullOrEmpty(apiKey)) return SinApiKey;
            try
            {
                var cuerpo = new
                {
                    model = ConfigTexto("CA.IA.Modelo", "claude-opus-5-5"),
                    max_tokens = 16000,
                    thinking = new { type = "adaptive" },
                    output_config = new { effort = ConfigTexto("CA.IA.Effort", "medium") },
                    fallbacks = "default",
                    system = system,
                    messages = new[] { new { role = "user", content = usuario } }
                };
                using (var req = new HttpRequestMessage(HttpMethod.Post, "v1/messages"))
                {
                    req.Content = new StringContent(JsonConvert.SerializeObject(cuerpo), Encoding.UTF8, "application/json");
                    req.Headers.Add("x-api-key", apiKey);
                    using (var resp = await Http.SendAsync(req).ConfigureAwait(false))
                    {
                        string json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (!resp.IsSuccessStatusCode) return mensajeError + " (HTTP " + (int)resp.StatusCode + ").";
                        var obj = JObject.Parse(json);
                        if ((string)obj["stop_reason"] == "refusal") return mensajeError + " (solicitud rechazada por el modelo).";
                        var bloques = obj["content"] as JArray;
                        var texto = bloques == null ? null : bloques.FirstOrDefault(b => (string)b["type"] == "text");
                        var t = texto != null ? ((string)texto["text"] ?? "").Trim() : "";
                        return t.Length > 0 ? t : null;
                    }
                }
            }
            catch (Exception ex) { return mensajeError + " (" + ex.Message + ")."; }
        }

        /* ============================================================ prompts */

        private const string Lema = "Eres una persona experta senior en Talento Humano y Desarrollo Organizacional de DOS, una empresa de tecnología cuyo lema es \"tecnología con propósito\". ";

        public const string SistemaIndividual = Lema +
"Redactas el resumen ejecutivo de una evaluación de curva de aprendizaje de un colaborador, a partir de las respuestas registradas en el formulario.\n\n" +
"Instrucciones de tono y forma:\n" +
"- Tono humano, cálido pero profesional y ejecutivo: como lo escribiría una persona experta en RRHH para que un gerente entienda la situación del colaborador en menos de un minuto.\n" +
"- 2 a 3 párrafos cortos, sin viñetas, sin encabezados, sin markdown.\n" +
"- Menciona fortalezas y focos de atención concretos basados en las respuestas reales, evita generalidades vacías.\n" +
"- Si hay una decisión de período de prueba, refléjala con claridad y con la sensibilidad adecuada.\n" +
"- Nunca inventes datos que no estén en las respuestas proporcionadas.\n" +
"- Responde directamente con el resumen ejecutivo, sin preámbulos ni frases como \"Aquí tienes\" o \"Resumen:\".";

        public const string SistemaAnual = Lema +
"Redactas el resumen ejecutivo del INFORME ANUAL CONSOLIDADO de la curva de aprendizaje de un colaborador, a partir de todas las evaluaciones registradas en las etapas de su primer año (30, 60, 90, 180 y 365 días), para presentar a gerencia.\n\n" +
"Instrucciones de tono y forma:\n" +
"- Tono humano, cálido pero profesional y ejecutivo: como lo escribiría una persona experta en RRHH para que gerencia entienda en menos de dos minutos qué se evaluó, cómo evolucionó el colaborador y qué sigue.\n" +
"- 3 a 4 párrafos cortos, sin viñetas, sin encabezados, sin markdown.\n" +
"- Primer párrafo: qué se evaluó a lo largo del año (menciona las dimensiones/categorías reales evaluadas, no generalidades vacías).\n" +
"- Segundo párrafo: tendencia de desempeño según la autoevaluación del colaborador y la evaluación de competencias del jefe (mejora, estancamiento o retroceso), y la decisión de período de prueba si existe, con la sensibilidad adecuada.\n" +
"- Párrafo(s) final(es): pasos a seguir o recomendaciones concretas para gerencia. Si ya existen planes de acción de GTH registrados por etapa, dales continuidad y resúmelos en vez de contradecirlos o repetirlos textualmente.\n" +
"- Nunca inventes datos que no estén en la información proporcionada.\n" +
"- Responde directamente con el resumen ejecutivo, sin preámbulos ni frases como \"Aquí tienes\" o \"Resumen:\".";

        public const string SistemaRango = Lema +
"Redactas el resumen ejecutivo de un INFORME CONSOLIDADO POR RANGO de la curva de aprendizaje de un colaborador, a partir de las evaluaciones registradas en un tramo específico de su proceso de onboarding (no necesariamente el año completo), para presentar a gerencia.\n\n" +
"Instrucciones de tono y forma:\n" +
"- Tono humano, cálido pero profesional y ejecutivo: como lo escribiría una persona experta en RRHH para que gerencia entienda en menos de dos minutos qué se evaluó en ese tramo y qué sigue.\n" +
"- 3 a 4 párrafos cortos, sin viñetas, sin encabezados, sin markdown.\n" +
"- Primer párrafo: qué etapas y dimensiones se evaluaron en este tramo específico (menciónalas por nombre, no generalidades vacías). No asumas que el proceso de onboarding ya terminó si el tramo no llega hasta el día 365.\n" +
"- Segundo párrafo: tendencia de desempeño en ese tramo según la autoevaluación del colaborador y la evaluación de competencias del jefe (mejora, estancamiento o retroceso), y la decisión de período de prueba si existe, con la sensibilidad adecuada.\n" +
"- Párrafo(s) final(es): pasos a seguir o recomendaciones concretas para gerencia sobre lo que sigue después de este tramo. Si ya existen planes de acción de GTH registrados por etapa, dales continuidad y resúmelos en vez de contradecirlos o repetirlos textualmente.\n" +
"- Nunca inventes datos que no estén en la información proporcionada.\n" +
"- Responde directamente con el resumen ejecutivo, sin preámbulos ni frases como \"Aquí tienes\" o \"Resumen:\".";

        /// <summary>Mensaje de usuario del resumen de UNA evaluacion (seccion 13.3).</summary>
        public static string PromptIndividual(EntCurvaExpediente x, EntCurvaEvaluacion ev)
        {
            var sb = new StringBuilder();
            sb.Append("Colaborador: ").Append(x.Empleado.NombreCompleto)
              .Append(" — ").Append(Vacio(x.Empleado.Cargo) ? "cargo no especificado" : x.Empleado.Cargo)
              .Append(" — ").Append(Vacio(x.Empleado.Area) ? "área no especificada" : x.Empleado.Area).Append('\n');
            sb.Append("Etapa evaluada: ").Append(ev.EtapaCodigo).Append(" días\n");
            sb.Append("Evaluación respondida por: ").Append(ev.EvaluadorRol == CurvaConstantes.RolColaborador ? "el propio colaborador" : "su jefe/evaluador").Append('\n');
            var dec = x.Decisiones.FirstOrDefault(d => d.EvaluacionId == ev.Id);
            if (dec != null) sb.Append(LineaDecision(dec)).Append('\n');
            sb.Append("\nRespuestas del formulario:\n");
            foreach (var r in RespuestasLegibles(x, ev.Id))
                sb.Append("- ").Append(r.TextoPregunta).Append("\n  Respuesta: ").Append(NegCurvaValorRespuesta.Formatear(r.TipoRespuesta, r.ValorRespuesta)).Append('\n');
            sb.Append("\nCompromisos acordados en esta evaluación:\n");
            var comps = x.Compromisos.Where(c => c.EvaluacionId == ev.Id).ToList();
            if (comps.Count == 0) sb.Append("Ninguno\n");
            foreach (var c in comps)
                sb.Append("- ").Append(c.Descripcion).Append(Vacio(c.Categoria) ? "" : " (" + c.Categoria + ")").Append('\n');
            sb.Append("\nRedacta el resumen ejecutivo de esta evaluación siguiendo las instrucciones del sistema.");
            return sb.ToString();
        }

        /// <summary>
        /// Mensaje de usuario del resumen anual o por rango (misma plantilla;
        /// en rango solo las evaluaciones y puntos del tramo).
        /// </summary>
        public static string PromptConsolidado(EntCurvaExpediente x, ICollection<string> etapas)
        {
            Func<string, bool> enAlcance = e => etapas == null || etapas.Contains(e);
            var sb = new StringBuilder();
            sb.Append("Colaborador: ").Append(x.Empleado.NombreCompleto).Append(" — ").Append(x.Empleado.Cargo ?? "").Append(" — ").Append(x.Empleado.Area ?? "").Append('\n');
            sb.Append("Fecha de ingreso: ").Append(x.Proceso.FechaInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("Estado actual del proceso: ").Append(x.Proceso.Estado).Append("\n\n");
            sb.Append("Evaluaciones completadas en el año:\n\n");

            var completadas = x.Evaluaciones.Where(e => e.EstaCompletada && enAlcance(e.EtapaCodigo)).ToList();
            if (completadas.Count == 0) sb.Append("Ninguna evaluación completada todavía.\n");
            var bloques = new List<string>();
            foreach (var ev in completadas)
            {
                var b = new StringBuilder();
                b.Append("Etapa ").Append(ev.EtapaCodigo).Append(" días — ").Append(ev.EvaluadorRol == CurvaConstantes.RolColaborador ? "Colaborador" : "Jefe/Evaluador").Append(":\n");
                foreach (var r in RespuestasLegibles(x, ev.Id))
                    b.Append("  - ").Append(r.TextoPregunta).Append(": ").Append(NegCurvaValorRespuesta.Formatear(r.TipoRespuesta, r.ValorRespuesta)).Append('\n');
                var dec = x.Decisiones.FirstOrDefault(d => d.EvaluacionId == ev.Id);
                if (dec != null) b.Append("  ").Append(LineaDecision(dec)).Append('\n');
                if (!Vacio(ev.PlanAccionGth)) b.Append("  Plan de acción GTH ya registrado: ").Append(ev.PlanAccionGth).Append('\n');
                bloques.Add(b.ToString());
            }
            sb.Append(string.Join("\n", bloques));
            sb.Append('\n');
            sb.Append("Índice de progreso colaborador por etapa: ").Append(Puntos(x.Serie.Colaborador, enAlcance)).Append('\n');
            sb.Append("Índice de progreso jefe por etapa: ").Append(Puntos(x.Serie.Jefe, enAlcance)).Append("\n\n");
            sb.Append("Redacta el resumen ejecutivo del informe anual siguiendo las instrucciones del sistema.");
            return sb.ToString();
        }

        private static IEnumerable<EntCurvaRespuesta> RespuestasLegibles(EntCurvaExpediente x, int evaluacionId)
        {
            return x.Respuestas.Where(r => r.EvaluacionId == evaluacionId
                                           && r.TipoRespuesta != CurvaConstantes.TipoCompromisos
                                           && r.TipoRespuesta != CurvaConstantes.TipoDecision)
                               .OrderBy(r => r.Orden);
        }

        private static string LineaDecision(EntCurvaDecision d)
        {
            return "Decisión formal de período de prueba: " + (d.Decision ?? "").ToUpperInvariant() + (Vacio(d.Comentarios) ? "" : " — " + d.Comentarios);
        }

        private static string Puntos(IEnumerable<EntCurvaPunto> puntos, Func<string, bool> enAlcance)
        {
            return string.Join(", ", puntos.Where(p => enAlcance(p.Etapa)).Select(p => p.Etapa + "d=" + p.Indice + "/100"));
        }

        private static bool Vacio(string s) { return string.IsNullOrWhiteSpace(s); }

        private static int Config(string clave, int porDefecto)
        {
            int v;
            return int.TryParse(ConfigurationManager.AppSettings[clave], out v) && v > 0 ? v : porDefecto;
        }

        private static string ConfigTexto(string clave, string porDefecto)
        {
            var v = ConfigurationManager.AppSettings[clave];
            return string.IsNullOrWhiteSpace(v) ? porDefecto : v.Trim();
        }
    }
}
