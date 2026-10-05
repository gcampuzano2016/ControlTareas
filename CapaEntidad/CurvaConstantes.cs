using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Vocabulario del modulo Curva de Aprendizaje (especificacion, seccion 6).
    /// Los valores son los mismos textos que guarda la base -y que guardaba el
    /// sistema original en SQLite-: no traducirlos ni cambiarles el casing, la
    /// migracion copia las columnas sin transformar.
    /// </summary>
    public static class CurvaConstantes
    {
        public const string RolColaborador = "colaborador";
        public const string RolJefe = "jefe";

        public const string EvalPendiente = "pendiente";
        public const string EvalCompletada = "completada";

        public const string ProcesoEnCurso = "en_curso";
        public const string ProcesoAprobado = "aprobado";
        public const string ProcesoRechazado = "rechazado";
        public const string ProcesoExtendido = "extendido";
        public const string ProcesoCompletado = "completado";

        public const string TipoEscala = "escala";
        public const string TipoOpcionUnica = "opcion_unica";
        public const string TipoOpcionMultiple = "opcion_multiple";
        public const string TipoTextoAbierto = "texto_abierto";
        public const string TipoSiNo = "si_no";
        public const string TipoDecision = "decision_periodo_prueba";
        public const string TipoCompromisos = "compromisos";
        public const string TipoCompetencias = "competencias_multi";

        public const string SegRealizado = "realizado";
        public const string SegEnProgreso = "en_progreso";
        public const string SegPendiente = "pendiente";
        public const string SegNoRealizado = "no_realizado";
        public const string SegSinRevisar = "sin_revisar";

        /// <summary>null (sin datos) es distinto de ok: ver NegCurvaCurva.CalcularAlerta.</summary>
        public const string NivelOk = "ok";
        public const string NivelAtencion = "atencion";
        public const string NivelSeguimiento = "seguimiento";

        /// <summary>indice &lt; 70 -&gt; seguimiento; 70-84 -&gt; atencion; &gt;= 85 -&gt; ok.</summary>
        public const int UmbralSeguimiento = 70;
        public const int UmbralAtencion = 85;

        public const string EtapaFinal = "365";

        public static readonly string[] EtapasOrden = { "30", "60", "90", "180", "365" };

        /// <summary>Decision de negocio: el jefe evalua solo en 30, 90 y 365.</summary>
        public static readonly Dictionary<string, string[]> RolesPorEtapa = new Dictionary<string, string[]>
        {
            { "30",  new[] { RolColaborador, RolJefe } },
            { "60",  new[] { RolColaborador } },
            { "90",  new[] { RolColaborador, RolJefe } },
            { "180", new[] { RolColaborador } },
            { "365", new[] { RolColaborador, RolJefe } }
        };

        public static readonly Dictionary<string, double> ScoreSeguimiento = new Dictionary<string, double>
        {
            { SegRealizado, 1.0 }, { SegEnProgreso, 0.5 }, { SegPendiente, 0.25 }, { SegNoRealizado, 0.0 }
        };

        public static readonly string[] EstadosSeguimiento = { SegRealizado, SegEnProgreso, SegPendiente, SegNoRealizado };

        public static readonly string[] Decisiones = { ProcesoAprobado, ProcesoExtendido, ProcesoRechazado };

        public static readonly string[] CategoriasCompromiso =
        {
            "Capacitación técnica", "Habilidades blandas / Comunicación", "Liderazgo y gestión de equipos",
            "Procesos internos / Herramientas", "Mentoría / Acompañamiento", "Otro"
        };
    }
}
