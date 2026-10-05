using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>Un punto de la curva: indice 0-100 de un rol en una etapa.</summary>
    public class EntCurvaPunto
    {
        public string Etapa { get; set; }
        public int Indice { get; set; }
    }

    /// <summary>La curva de un proceso: una serie por rol, en el orden 30..365.</summary>
    public class EntCurvaSerie
    {
        public List<EntCurvaPunto> Colaborador { get; set; }
        public List<EntCurvaPunto> Jefe { get; set; }

        public EntCurvaSerie()
        {
            Colaborador = new List<EntCurvaPunto>();
            Jefe = new List<EntCurvaPunto>();
        }
    }

    /// <summary>Alerta general. Nivel null = sin datos todavia (distinto de "ok").</summary>
    public class EntCurvaAlerta
    {
        public string Nivel { get; set; }
        public string Motivo { get; set; }
    }

    public class EntCurvaNivelRol
    {
        public int Indice { get; set; }
        public string Nivel { get; set; }
    }

    /// <summary>Semaforo de una etapa: cada rol por separado y el peor de los dos.</summary>
    public class EntCurvaAlertaEtapa
    {
        public string Etapa { get; set; }
        public EntCurvaNivelRol Colaborador { get; set; }
        public EntCurvaNivelRol Jefe { get; set; }
        public string Nivel { get; set; }
    }

    /// <summary>
    /// Lo minimo que necesita el calculo de la curva de una evaluacion
    /// completada: su rol, su etapa, sus respuestas y los seguimientos que se
    /// registraron en ella.
    /// </summary>
    public class EntCurvaInsumoEvaluacion
    {
        public int EvaluacionId { get; set; }
        public string Rol { get; set; }
        public string EtapaCodigo { get; set; }
        public List<EntCurvaRespuesta> Respuestas { get; set; }
        public List<string> EstadosSeguimiento { get; set; }

        public EntCurvaInsumoEvaluacion()
        {
            Respuestas = new List<EntCurvaRespuesta>();
            EstadosSeguimiento = new List<string>();
        }
    }
}
