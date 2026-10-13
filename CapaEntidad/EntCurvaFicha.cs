using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Todo lo de un proceso, leido de una vez: lo usan la ficha, los
    /// informes PDF y los prompts de IA. Las evaluaciones traen el token: este
    /// objeto NUNCA se entrega entero a la pagina remota.
    /// </summary>
    public class EntCurvaExpediente
    {
        public EntCurvaEmpleado Empleado { get; set; }
        public EntCurvaProceso Proceso { get; set; }
        public List<EntCurvaEvaluacion> Evaluaciones { get; set; }
        public List<EntCurvaRespuesta> Respuestas { get; set; }
        public List<EntCurvaCompromiso> Compromisos { get; set; }
        public List<EntCurvaSeguimiento> Seguimientos { get; set; }
        public List<EntCurvaDecision> Decisiones { get; set; }
        /// <summary>Insumos de las evaluaciones completadas (la curva solo usa esas).</summary>
        public List<EntCurvaInsumoEvaluacion> Completadas { get; set; }
        public EntCurvaSerie Serie { get; set; }
    }

    /// <summary>Una tarjeta de la ficha: una evaluacion con su avance.</summary>
    public class EntCurvaTarjeta
    {
        public int Id { get; set; }
        public string EtapaCodigo { get; set; }
        public string EtapaNombre { get; set; }
        public string Rol { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaObjetivo { get; set; }
        public DateTime? FechaCompletada { get; set; }
        public int? Indice { get; set; }
        /// <summary>Nivel del indice de ESTA tarjeta; null si no hay indice.</summary>
        public string Nivel { get; set; }
        /// <summary>Jefe con nivel distinto de ok: aviso visual "Requiere plan de accion de GTH".</summary>
        public bool AvisoPlanAccion { get; set; }
        /// <summary>La caja de plan de accion solo se muestra en nivel seguimiento.</summary>
        public bool MostrarPlanAccion { get; set; }
        public string PlanAccionGth { get; set; }
        public string Conclusion { get; set; }
        public bool TieneEnlace { get; set; }
    }

    /// <summary>La ficha del colaborador (pantalla CurvaColaboradorFicha.aspx).</summary>
    public class EntCurvaFicha
    {
        public EntCurvaEmpleado Empleado { get; set; }
        public EntCurvaProceso Proceso { get; set; }
        public List<EntCurvaTarjeta> Evaluaciones { get; set; }
        public List<EntCurvaCompromiso> Compromisos { get; set; }
        public EntCurvaSerie Curva { get; set; }
        public EntCurvaAlerta Alerta { get; set; }
        public List<EntCurvaAlertaEtapa> AlertaPorEtapa { get; set; }
        public Dictionary<string, Dictionary<string, int>> Competencias { get; set; }
        public List<EntCurvaEtapa> Etapas { get; set; }
    }

    /// <summary>
    /// Lo que necesita el formulario para pintarse. En la pagina remota se
    /// arma con lo minimo (seccion 11.3): nombre y cargo, etapa, rol,
    /// preguntas y compromisos abiertos. Nada de curva, alertas, resumen,
    /// plan, conclusion ni tokens.
    /// </summary>
    public class EntCurvaFormularioDatos
    {
        public int EvaluacionId { get; set; }
        public int EmpleadoId { get; set; }
        public string EmpleadoNombre { get; set; }
        public string EmpleadoCargo { get; set; }
        public string EtapaCodigo { get; set; }
        public string EtapaNombre { get; set; }
        public string Rol { get; set; }
        public string Estado { get; set; }
        public List<EntCurvaPreguntaFormulario> Preguntas { get; set; }
        /// <summary>id de pregunta -&gt; valor ya decodificado (texto, lista u objeto).</summary>
        public Dictionary<int, object> Respuestas { get; set; }
        public List<EntCurvaAbierto> Abiertos { get; set; }
        public string[] Categorias { get; set; }
    }

    public class EntCurvaPreguntaFormulario
    {
        public int Id { get; set; }
        public string Seccion { get; set; }
        public string Texto { get; set; }
        public string Tipo { get; set; }
        public List<string> Opciones { get; set; }
    }

    /// <summary>Compromiso abierto tal como lo ve el formulario (sin datos de seguimiento ajenos).</summary>
    public class EntCurvaAbierto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public string Responsable { get; set; }
        public string EtapaCodigo { get; set; }
        public DateTime? FechaLimite { get; set; }
        public string EstadoActual { get; set; }
        public string JustificacionActual { get; set; }
    }
}
