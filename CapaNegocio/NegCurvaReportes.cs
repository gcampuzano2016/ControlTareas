using CapaEntidad;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CapaNegocio
{
    /// <summary>
    /// Informes PDF de Curva de Aprendizaje (especificacion, seccion 12), con
    /// PDFsharp 1.32 (ya estaba en la solucion; MIT). Se arman en memoria y
    /// nunca tocan disco.
    ///
    /// Colores: SOLO la paleta oficial de DOS (seccion 15); los pasteles son
    /// para la web. A4, margen 40pt, marca de agua del logo en cada pagina y
    /// pie "DOS · Tecnologia con proposito — tipo    n/N".
    ///
    /// La barra de posicion de las respuestas de escala se llena mas cuanto
    /// MEJOR es la opcion (1 - indice/(n-1)): el sistema original lo hacia al
    /// reves (seccion 18, punto 5).
    /// </summary>
    public static class NegCurvaReportes
    {
        /* ------------------------------------------------- paleta oficial */
        private static readonly XColor Negro = XColor.FromArgb(0, 0, 0);
        private static readonly XColor GrisOscuro = XColor.FromArgb(0x22, 0x1e, 0x20);
        private static readonly XColor Gris = XColor.FromArgb(0x77, 0x78, 0x7b);
        private static readonly XColor RojoOscuro = XColor.FromArgb(0x8f, 0x18, 0x1b);
        private static readonly XColor Rojo = XColor.FromArgb(0xe2, 0x00, 0x00);
        private static readonly XColor Blanco = XColor.FromArgb(0xff, 0xff, 0xff);
        private static readonly XColor Petroleo = XColor.FromArgb(0x0f, 0x45, 0x60);
        private static readonly XColor VerdeAzulado = XColor.FromArgb(0x02, 0x66, 0x66);
        private static readonly XColor Amarillo = XColor.FromArgb(0xf9, 0xd7, 0x10);
        private static readonly XColor Purpura = XColor.FromArgb(0x6d, 0x33, 0xa0);
        private static readonly XColor Naranja = XColor.FromArgb(0xf7, 0x90, 0x2b);

        private static readonly Dictionary<string, string> EstadosProceso = new Dictionary<string, string>
        {
            { "en_curso", "En curso" }, { "aprobado", "Aprobado" }, { "rechazado", "Rechazado" },
            { "extendido", "Extendido" }, { "completado", "Completado" }
        };
        private static readonly Dictionary<string, string> NivelLargo = new Dictionary<string, string>
        {
            { "ok", "En buen curso" }, { "atencion", "Dar seguimiento" }, { "seguimiento", "Alerta: plan de acción" }
        };
        private static readonly Dictionary<string, string> NivelCorto = new Dictionary<string, string>
        {
            { "ok", "En buen curso" }, { "atencion", "Atención" }, { "seguimiento", "Seguimiento" }
        };

        /* ================================================ nombres de archivo */

        private static string Limpio(string s)
        {
            return Regex.Replace(s ?? "", "[^A-Za-z0-9]", "-");
        }

        public static string NombreIndividual(EntCurvaExpediente x, EntCurvaEvaluacion ev)
        {
            return "Curva-Aprendizaje-" + Limpio(x.Empleado.NombreCompleto) + "-" + Limpio(ev.EtapaCodigo) + "d-" + Limpio(ev.EvaluadorRol) + ".pdf";
        }

        public static string NombreAnual(EntCurvaExpediente x)
        {
            return "Curva-Aprendizaje-Anual-" + Limpio(x.Empleado.NombreCompleto) + ".pdf";
        }

        public static string NombreRango(EntCurvaExpediente x, string desde, string hasta)
        {
            return "Curva-Aprendizaje-" + Limpio(x.Empleado.NombreCompleto) + "-" + Limpio(desde) + "a" + Limpio(hasta) + "d.pdf";
        }

        /// <summary>Las etapas entre desde y hasta (inclusive), o null si el rango no es valido.</summary>
        public static List<string> EtapasDelRango(string desde, string hasta)
        {
            int i = Array.IndexOf(CurvaConstantes.EtapasOrden, desde), j = Array.IndexOf(CurvaConstantes.EtapasOrden, hasta);
            if (i < 0 || j < 0 || i > j) return null;
            return CurvaConstantes.EtapasOrden.Skip(i).Take(j - i + 1).ToList();
        }

        /* ================================================== informe individual */

        public static byte[] Individual(EntCurvaExpediente x, EntCurvaEvaluacion ev, string rutaLogo)
        {
            var l = new Lienzo(rutaLogo, "Informe individual");
            l.Encabezado("INFORME CURVA DE APRENDIZAJE",
                "Etapa " + ev.EtapaCodigo + " días · " + (ev.EvaluadorRol == CurvaConstantes.RolJefe ? "Evaluación del Jefe / Evaluador" : "Autoevaluación del Colaborador"), x.Empleado);

            var nuevos = x.Compromisos.Where(c => c.EvaluacionId == ev.Id).ToList();
            var revisados = x.Seguimientos.Where(s => s.EvaluacionId == ev.Id).ToList();
            l.Kpis(new[]
            {
                new[] { "Etapa evaluada", ev.EtapaCodigo + " días" },
                new[] { "Fecha de cierre", Fecha(ev.FechaCompletada) },
                new[] { "Compromisos nuevos", nuevos.Count.ToString(CultureInfo.InvariantCulture) },
                new[] { "Compromisos revisados", revisados.Count.ToString(CultureInfo.InvariantCulture) }
            });

            if (!string.IsNullOrWhiteSpace(ev.ResumenEjecutivo)) l.Caja("RESUMEN EJECUTIVO", ev.ResumenEjecutivo, Petroleo);

            var dec = x.Decisiones.FirstOrDefault(d => d.EvaluacionId == ev.Id);
            if (dec != null)
            {
                var texto = (dec.NuevaFechaFin.HasValue ? "Nueva fecha límite: " + Fecha(dec.NuevaFechaFin) + "\n" : "") + (dec.Comentarios ?? "");
                l.Caja("DECISIÓN DE PERÍODO DE PRUEBA: " + (dec.Decision ?? "").ToUpperInvariant(), texto.Trim().Length > 0 ? texto.Trim() : "Sin comentarios.",
                       dec.Decision == "rechazado" ? RojoOscuro : dec.Decision == "extendido" ? Purpura : VerdeAzulado);
            }

            string seccion = null;
            foreach (var r in x.Respuestas.Where(r => r.EvaluacionId == ev.Id && r.TipoRespuesta != CurvaConstantes.TipoCompromisos && r.TipoRespuesta != CurvaConstantes.TipoDecision).OrderBy(r => r.Orden))
            {
                if (r.Seccion != seccion) { seccion = r.Seccion; l.Seccion(seccion ?? ""); }
                l.Pregunta(r.TextoPregunta);
                switch (r.TipoRespuesta)
                {
                    case CurvaConstantes.TipoOpcionMultiple:
                        foreach (var v in NegCurvaValorRespuesta.ComoLista(r.ValorRespuesta)) l.Vineta(v);
                        break;
                    case CurvaConstantes.TipoCompetencias:
                        l.BarrasCompetencias(NegCurvaValorRespuesta.ComoCompetencias(r.ValorRespuesta), Petroleo);
                        break;
                    case CurvaConstantes.TipoTextoAbierto:
                        var t = NegCurvaValorRespuesta.ComoTexto(r.ValorRespuesta);
                        l.Parrafo(string.IsNullOrWhiteSpace(t) ? "—" : t, l.Texto, GrisOscuro);
                        break;
                    default:
                        var valor = NegCurvaValorRespuesta.ComoTexto(r.ValorRespuesta);
                        l.Parrafo(valor, l.Negrita, GrisOscuro);
                        var ops = NegCurvaValorRespuesta.Opciones(r.OpcionesJson);
                        int idx = ops == null ? -1 : ops.FindIndex(o => string.Equals(o, valor, StringComparison.Ordinal));
                        if (ops != null && ops.Count >= 2 && idx >= 0) l.Barra(1.0 - (double)idx / (ops.Count - 1), VerdeAzulado);
                        break;
                }
                l.Espacio(6);
            }

            if (nuevos.Count > 0)
            {
                l.Seccion("Compromisos y Acciones Acordadas");
                foreach (var c in nuevos) l.Compromiso(c);
            }

            if (revisados.Count > 0)
            {
                l.Seccion("Seguimiento de Compromisos Anteriores");
                foreach (var s in revisados) l.Seguimiento(s.Descripcion, s.Estado, s.Justificacion);
            }

            if (!string.IsNullOrWhiteSpace(ev.Conclusion)) { l.Seccion("Conclusión"); l.Parrafo(ev.Conclusion, l.Texto, GrisOscuro); }
            if (!string.IsNullOrWhiteSpace(ev.PlanAccionGth)) { l.Seccion("Plan de Acción GTH"); l.Parrafo(ev.PlanAccionGth, l.Texto, GrisOscuro); }

            return l.Terminar();
        }

        /* =================================================== informe consolidado */

        /// <summary>Anual (etapas = null) o por rango (etapas del tramo).</summary>
        public static byte[] Consolidado(EntCurvaExpediente x, List<string> etapas, string resumen, string rutaLogo)
        {
            bool rango = etapas != null;
            Func<string, bool> enAlcance = e => !rango || etapas.Contains(e);
            var l = new Lienzo(rutaLogo, rango ? "Informe consolidado por rango" : "Informe anual consolidado");
            l.Encabezado(rango ? "INFORME CONSOLIDADO POR RANGO" : "INFORME ANUAL CONSOLIDADO",
                rango ? "Curva de Aprendizaje — Etapas " + etapas.First() + " a " + etapas.Last() + " días" : "Curva de Aprendizaje — Resumen del primer año",
                x.Empleado);

            var evals = x.Evaluaciones.Where(e => enAlcance(e.EtapaCodigo)).ToList();
            var serie = new EntCurvaSerie
            {
                Colaborador = x.Serie.Colaborador.Where(p => enAlcance(p.Etapa)).ToList(),
                Jefe = x.Serie.Jefe.Where(p => enAlcance(p.Etapa)).ToList()
            };
            var alerta = NegCurvaCurva.CalcularAlerta(serie);
            var idsEval = new HashSet<int>(evals.Select(e => e.Id));
            var compromisos = x.Compromisos.Where(c => idsEval.Contains(c.EvaluacionId)).ToList();

            string estado;
            l.Kpis(new[]
            {
                new[] { "Evaluaciones completadas", evals.Count(e => e.EstaCompletada) + "/" + evals.Count },
                new[] { "Estado del proceso", EstadosProceso.TryGetValue(x.Proceso.Estado ?? "", out estado) ? estado : x.Proceso.Estado },
                new[] { "Compromisos acordados", compromisos.Count.ToString(CultureInfo.InvariantCulture) },
                new[] { "Alerta de seguimiento", alerta.Nivel == null ? "Sin datos" : NivelCorto[alerta.Nivel] }
            });

            if (!string.IsNullOrWhiteSpace(resumen)) l.Caja("RESUMEN EJECUTIVO", resumen, Purpura);

            if (serie.Colaborador.Count > 0 || serie.Jefe.Count > 0)
            {
                l.Seccion("Curva de Aprendizaje");
                l.GraficoCurva(serie, rango ? etapas : CurvaConstantes.EtapasOrden.ToList());
            }

            l.Seccion("Línea de tiempo");
            foreach (var ev in evals)
            {
                int? indice = ev.EstaCompletada ? NegCurvaCurva.IndiceDe(x.Serie, ev.EtapaCodigo, ev.EvaluadorRol) : null;
                string nivel = indice.HasValue ? NegCurvaCurva.Nivel(indice.Value) : null;
                var lineas = new List<string>();
                lineas.Add(ev.EstaCompletada ? "Completada el " + Fecha(ev.FechaCompletada) : "Pendiente");
                if (nivel != null) lineas.Add("Estado: " + NivelLargo[nivel] + " (" + indice.Value + "/100)");
                l.Hito("Etapa " + ev.EtapaCodigo + " días — " + (ev.EvaluadorRol == CurvaConstantes.RolJefe ? "Jefe / Evaluador" : "Colaborador"),
                       lineas, ev.EstaCompletada,
                       ev.EvaluadorRol == CurvaConstantes.RolJefe && nivel != null && nivel != CurvaConstantes.NivelOk ? "Requiere plan de acción inmediato de GTH" : null,
                       ev.Conclusion, ev.PlanAccionGth);
            }

            var comp = NegCurvaCurva.CompetenciasJefe(x.Completadas);
            foreach (var et in new[] { "90", "365" })
            {
                if (!enAlcance(et) || comp[et] == null || comp[et].Count == 0) continue;
                l.Seccion("Competencias evaluadas por el jefe — " + et + " días");
                l.BarrasCompetencias(comp[et], et == "90" ? Petroleo : Naranja);
            }

            if (compromisos.Count > 0)
            {
                l.Seccion(rango ? "Compromisos del tramo" : "Compromisos del año");
                foreach (var c in compromisos) l.Seguimiento(c.Descripcion + "  (etapa " + c.EtapaCodigo + " días)", c.UltimoEstado ?? CurvaConstantes.SegSinRevisar, c.Categoria);
            }

            return l.Terminar();
        }

        private static string Fecha(DateTime? d)
        {
            return d.HasValue ? d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "—";
        }

        private static XColor ColorEstado(string estado)
        {
            switch (estado)
            {
                case "realizado": return VerdeAzulado;
                case "en_progreso": return Amarillo;
                case "pendiente": return Gris;
                case "no_realizado": return RojoOscuro;
                default: return Gris;
            }
        }

        private static string TextoEstado(string estado)
        {
            switch (estado)
            {
                case "realizado": return "REALIZADO";
                case "en_progreso": return "EN PROGRESO";
                case "pendiente": return "PENDIENTE";
                case "no_realizado": return "NO REALIZADO";
                default: return "SIN REVISAR";
            }
        }

        /* ======================================================== el lienzo */

        /// <summary>
        /// Un cursor vertical sobre paginas A4: cada metodo dibuja un bloque y
        /// baja el cursor, abriendo una pagina nueva cuando no cabe.
        /// </summary>
        private sealed class Lienzo
        {
            private const double M = 40, Pie = 50;
            private readonly PdfDocument doc = new PdfDocument();
            private readonly string tipo;
            private readonly XImage marca;
            private readonly XImage logo;
            private PdfPage pagina;
            private XGraphics g;
            private double y;
            private double Ancho { get { return pagina.Width.Point - 2 * M; } }
            private double Limite { get { return pagina.Height.Point - Pie; } }

            public readonly XFont Texto, Negrita, Chica, Titulo, Subtitulo, Kpi, SeccionF;

            public Lienzo(string rutaLogo, string tipoInforme)
            {
                tipo = tipoInforme;
                var op = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
                Texto = new XFont("Arial", 9.5, XFontStyle.Regular, op);
                Negrita = new XFont("Arial", 9.5, XFontStyle.Bold, op);
                Chica = new XFont("Arial", 8, XFontStyle.Regular, op);
                Titulo = new XFont("Arial", 17, XFontStyle.Bold, op);
                Subtitulo = new XFont("Arial", 10.5, XFontStyle.Regular, op);
                Kpi = new XFont("Arial", 13, XFontStyle.Bold, op);
                SeccionF = new XFont("Arial", 10.5, XFontStyle.Bold, op);
                if (!string.IsNullOrEmpty(rutaLogo) && File.Exists(rutaLogo))
                {
                    logo = XImage.FromGdiPlusImage(Image.FromFile(rutaLogo));
                    marca = XImage.FromGdiPlusImage(Desvanecido(rutaLogo, 0.06f));
                }
                NuevaPagina();
            }

            /// <summary>Una copia del logo casi transparente sobre blanco, para la marca de agua.</summary>
            private static Image Desvanecido(string ruta, float alfa)
            {
                using (var original = Image.FromFile(ruta))
                {
                    var bmp = new Bitmap(original.Width, original.Height);
                    using (var gr = Graphics.FromImage(bmp))
                    using (var atr = new ImageAttributes())
                    {
                        gr.Clear(Color.White);
                        var matriz = new ColorMatrix { Matrix33 = alfa };
                        atr.SetColorMatrix(matriz);
                        gr.DrawImage(original, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, atr);
                    }
                    return bmp;
                }
            }

            private void NuevaPagina()
            {
                if (g != null) g.Dispose();
                pagina = doc.AddPage();
                pagina.Size = PageSize.A4;
                g = XGraphics.FromPdfPage(pagina);
                if (marca != null)
                {
                    double w = 360, h = w * marca.PixelHeight / marca.PixelWidth;
                    g.DrawImage(marca, (pagina.Width.Point - w) / 2, (pagina.Height.Point - h) / 2, w, h);
                }
                y = M;
            }

            private void Asegurar(double alto)
            {
                if (y + alto > Limite) NuevaPagina();
            }

            public void Espacio(double alto) { y += alto; }

            /* ------------------------------------------------- texto basico */

            private List<string> Partir(string texto, XFont f, double ancho)
            {
                var lineas = new List<string>();
                foreach (var parrafo in (texto ?? "").Replace("\r\n", "\n").Split('\n'))
                {
                    var actual = "";
                    foreach (var palabra in parrafo.Split(' '))
                    {
                        var prueba = actual.Length == 0 ? palabra : actual + " " + palabra;
                        if (g.MeasureString(prueba, f).Width <= ancho || actual.Length == 0) actual = prueba;
                        else { lineas.Add(actual); actual = palabra; }
                    }
                    lineas.Add(actual);
                }
                return lineas;
            }

            private double Alto(XFont f) { return f.Size * 1.35; }

            public void Parrafo(string texto, XFont f, XColor color, double sangria = 0)
            {
                var pincel = new XSolidBrush(color);
                foreach (var linea in Partir(texto, f, Ancho - sangria))
                {
                    Asegurar(Alto(f));
                    g.DrawString(linea, f, pincel, new XRect(M + sangria, y, Ancho - sangria, Alto(f)), XStringFormats.TopLeft);
                    y += Alto(f);
                }
            }

            public void Encabezado(string titulo, string subtitulo, EntCurvaEmpleado e)
            {
                g.DrawRectangle(new XSolidBrush(GrisOscuro), M, y, Ancho, 64);
                g.DrawRectangle(new XSolidBrush(Rojo), M, y, 6, 64);
                g.DrawString(titulo, Titulo, XBrushes.White, new XRect(M + 18, y + 12, Ancho - 90, 22), XStringFormats.TopLeft);
                g.DrawString(subtitulo, Subtitulo, XBrushes.White, new XRect(M + 18, y + 38, Ancho - 90, 16), XStringFormats.TopLeft);
                if (logo != null) g.DrawImage(logo, M + Ancho - 62, y + 8, 48, 48.0 * logo.PixelHeight / logo.PixelWidth);
                y += 76;
                var datos = new List<string> { e.NombreCompleto };
                if (!string.IsNullOrWhiteSpace(e.Cargo)) datos.Add("Cargo: " + e.Cargo);
                if (!string.IsNullOrWhiteSpace(e.Area)) datos.Add("Área: " + e.Area);
                if (!string.IsNullOrWhiteSpace(e.Empresa)) datos.Add("Empresa: " + e.Empresa);
                if (!string.IsNullOrWhiteSpace(e.UnidadNegocio)) datos.Add("Unidad de negocio: " + e.UnidadNegocio);
                if (!string.IsNullOrWhiteSpace(e.JefeNombre)) datos.Add("Jefe: " + e.JefeNombre);
                Parrafo(datos[0], SeccionF, Negro);
                if (datos.Count > 1) Parrafo(string.Join("   ·   ", datos.Skip(1)), Texto, Gris);
                y += 10;
            }

            public void Kpis(string[][] kpis)
            {
                double gap = 8, w = (Ancho - gap * (kpis.Length - 1)) / kpis.Length, h = 50;
                Asegurar(h + 10);
                var colores = new[] { Naranja, Petroleo, VerdeAzulado, Purpura };
                for (int i = 0; i < kpis.Length; i++)
                {
                    double x = M + i * (w + gap);
                    g.DrawRectangle(new XSolidBrush(GrisOscuro), x, y, w, h);
                    g.DrawRectangle(new XSolidBrush(colores[i % 4]), x, y, w, 3);
                    // La etiqueta se achica hasta caber en su tarjeta: "EVALUACIONES COMPLETADAS" no entra a 8pt.
                    var etiqueta = kpis[i][0].ToUpperInvariant();
                    var fuente = Chica;
                    while (g.MeasureString(etiqueta, fuente).Width > w - 16 && fuente.Size > 5.5)
                        fuente = new XFont(fuente.Name, fuente.Size - 0.5, fuente.Style, fuente.PdfOptions);
                    g.DrawString(etiqueta, fuente, new XSolidBrush(XColor.FromArgb(0xc4, 0xbe, 0xb8)), new XRect(x + 8, y + 8, w - 16, 12), XStringFormats.TopLeft);
                    g.DrawString(kpis[i][1] ?? "—", Kpi, XBrushes.White, new XRect(x + 8, y + 24, w - 16, 18), XStringFormats.TopLeft);
                }
                y += h + 14;
            }

            public void Caja(string titulo, string texto, XColor fondo)
            {
                var lineas = Partir(texto, Texto, Ancho - 24);
                double alto = 30 + lineas.Count * Alto(Texto) + 8;
                if (alto > Limite - M) { Seccion(titulo); Parrafo(texto, Texto, GrisOscuro); y += 8; return; }   // no cabe en una pagina
                Asegurar(alto);
                g.DrawRectangle(new XSolidBrush(fondo), M, y, Ancho, alto);
                g.DrawString(titulo, Negrita, XBrushes.White, new XRect(M + 12, y + 10, Ancho - 24, 14), XStringFormats.TopLeft);
                double yy = y + 28;
                foreach (var linea in lineas)
                {
                    g.DrawString(linea, Texto, XBrushes.White, new XRect(M + 12, yy, Ancho - 24, Alto(Texto)), XStringFormats.TopLeft);
                    yy += Alto(Texto);
                }
                y += alto + 12;
            }

            public void Seccion(string titulo)
            {
                Asegurar(40);
                y += 6;
                g.DrawString((titulo ?? "").ToUpperInvariant(), SeccionF, new XSolidBrush(Petroleo), new XRect(M, y, Ancho, 14), XStringFormats.TopLeft);
                y += 16;
                g.DrawLine(new XPen(Rojo, 1.2), M, y, M + Ancho, y);
                y += 6;
            }

            public void Pregunta(string texto)
            {
                Asegurar(Alto(Negrita) * 2);
                Parrafo(texto, Negrita, Negro);
            }

            public void Vineta(string texto)
            {
                Asegurar(Alto(Texto));
                g.DrawEllipse(new XSolidBrush(Petroleo), M + 4, y + 4, 4, 4);
                Parrafo(texto, Texto, GrisOscuro, 14);
            }

            public void Barra(double posicion, XColor color)
            {
                Asegurar(10);
                double w = Math.Min(220, Ancho);
                g.DrawRectangle(new XSolidBrush(XColor.FromArgb(0xe5, 0xe3, 0xe2)), M, y + 2, w, 5);
                g.DrawRectangle(new XSolidBrush(color), M, y + 2, w * Math.Max(0, Math.Min(1, posicion)), 5);
                y += 10;
            }

            public void BarrasCompetencias(Dictionary<string, int> comp, XColor color)
            {
                foreach (var kv in comp)
                {
                    Asegurar(16);
                    g.DrawString(kv.Key, Texto, new XSolidBrush(GrisOscuro), new XRect(M, y, 170, 14), XStringFormats.TopLeft);
                    double w = Ancho - 220;
                    g.DrawRectangle(new XSolidBrush(XColor.FromArgb(0xe5, 0xe3, 0xe2)), M + 175, y + 3, w, 8);
                    g.DrawRectangle(new XSolidBrush(color), M + 175, y + 3, w * Math.Max(0, Math.Min(5, kv.Value)) / 5.0, 8);
                    g.DrawString(kv.Value + "/5", Negrita, new XSolidBrush(GrisOscuro), new XRect(M + 180 + w, y, 40, 14), XStringFormats.TopLeft);
                    y += 16;
                }
            }

            public void Compromiso(EntCurvaCompromiso c)
            {
                Parrafo("• " + c.Descripcion, Negrita, GrisOscuro);
                var meta = new List<string>();
                if (!string.IsNullOrWhiteSpace(c.Categoria)) meta.Add(c.Categoria);
                if (!string.IsNullOrWhiteSpace(c.Responsable)) meta.Add("Responsable: " + c.Responsable);
                if (c.FechaLimite.HasValue) meta.Add("Límite: " + Fecha(c.FechaLimite));
                if (meta.Count > 0) Parrafo(string.Join(" · ", meta), Chica, Gris, 10);
                y += 4;
            }

            public void Seguimiento(string descripcion, string estado, string nota)
            {
                Asegurar(Alto(Negrita) * 2);
                g.DrawEllipse(new XSolidBrush(ColorEstado(estado)), M, y + 3, 7, 7);
                g.DrawString(TextoEstado(estado), Negrita, new XSolidBrush(ColorEstado(estado) == Amarillo ? GrisOscuro : ColorEstado(estado)), new XRect(M + 12, y, 100, 14), XStringFormats.TopLeft);
                y += Alto(Negrita);
                Parrafo(descripcion, Texto, GrisOscuro, 12);
                if (!string.IsNullOrWhiteSpace(nota)) Parrafo(nota, Chica, Gris, 12);
                y += 4;
            }

            public void Hito(string titulo, List<string> lineas, bool completada, string aviso, string conclusion, string plan)
            {
                Asegurar(Alto(Negrita) * 3);
                g.DrawEllipse(new XSolidBrush(completada ? VerdeAzulado : Gris), M, y + 2, 9, 9);
                Parrafo(titulo, Negrita, Negro, 16);
                foreach (var linea in lineas) Parrafo(linea, Texto, GrisOscuro, 16);
                if (aviso != null) Parrafo(aviso, Negrita, RojoOscuro, 16);
                if (!string.IsNullOrWhiteSpace(conclusion)) Parrafo("Conclusión: " + conclusion, Texto, GrisOscuro, 16);
                if (!string.IsNullOrWhiteSpace(plan)) Parrafo("PLAN DE ACCIÓN GTH: " + plan, Texto, Purpura, 16);
                y += 6;
            }

            public void GraficoCurva(EntCurvaSerie serie, List<string> etapas)
            {
                double alto = 170, izq = M + 30, ancho = Ancho - 40;
                Asegurar(alto + 40);
                var lapiz = new XPen(XColor.FromArgb(0xe5, 0xe3, 0xe2), 0.6);
                for (int v = 0; v <= 100; v += 25)
                {
                    double yy = y + alto - alto * v / 100.0;
                    g.DrawLine(lapiz, izq, yy, izq + ancho, yy);
                    g.DrawString(v.ToString(CultureInfo.InvariantCulture), Chica, new XSolidBrush(Gris), new XRect(M, yy - 5, 24, 10), XStringFormats.TopLeft);
                }
                Func<int, double> xDe = i => etapas.Count == 1 ? izq + ancho / 2 : izq + ancho * i / (etapas.Count - 1);
                for (int i = 0; i < etapas.Count; i++)
                    g.DrawString(etapas[i] + " días", Chica, new XSolidBrush(Gris), new XRect(xDe(i) - 25, y + alto + 4, 50, 10), XStringFormats.TopCenter);

                Action<List<EntCurvaPunto>, XColor> linea = (puntos, color) =>
                {
                    var pts = new List<XPoint>();
                    for (int i = 0; i < etapas.Count; i++)
                    {
                        var p = puntos.FirstOrDefault(q => q.Etapa == etapas[i]);
                        if (p != null) pts.Add(new XPoint(xDe(i), y + alto - alto * p.Indice / 100.0));
                    }
                    if (pts.Count > 1) g.DrawLines(new XPen(color, 2), pts.ToArray());
                    foreach (var pt in pts) g.DrawEllipse(new XSolidBrush(color), pt.X - 3, pt.Y - 3, 6, 6);
                };
                linea(serie.Colaborador, Petroleo);
                linea(serie.Jefe, Naranja);
                y += alto + 18;
                double lx = izq;
                if (serie.Colaborador.Count > 0) { g.DrawRectangle(new XSolidBrush(Petroleo), lx, y + 2, 10, 6); g.DrawString("Colaborador", Chica, new XSolidBrush(GrisOscuro), new XRect(lx + 14, y, 80, 10), XStringFormats.TopLeft); lx += 100; }
                if (serie.Jefe.Count > 0) { g.DrawRectangle(new XSolidBrush(Naranja), lx, y + 2, 10, 6); g.DrawString("Jefe", Chica, new XSolidBrush(GrisOscuro), new XRect(lx + 14, y, 80, 10), XStringFormats.TopLeft); }
                y += 18;
            }

            /// <summary>Cierra el documento: pie con "n/N" en cada pagina.</summary>
            public byte[] Terminar()
            {
                g.Dispose();
                int total = doc.PageCount;
                for (int i = 0; i < total; i++)
                {
                    var p = doc.Pages[i];
                    using (var gp = XGraphics.FromPdfPage(p, XGraphicsPdfPageOptions.Append))
                    {
                        var texto = "DOS · Tecnología con propósito — " + tipo + "    " + (i + 1) + "/" + total;
                        gp.DrawString(texto, Chica, new XSolidBrush(Gris), new XRect(M, p.Height.Point - 30, p.Width.Point - 2 * M, 12), XStringFormats.TopCenter);
                    }
                }
                using (var ms = new MemoryStream())
                {
                    doc.Save(ms, false);
                    return ms.ToArray();
                }
            }
        }
    }
}
