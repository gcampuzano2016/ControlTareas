using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>Una etapa del proceso (30, 60, 90, 180, 365). Orden define la secuencia.</summary>
    public class EntCurvaEtapa
    {
        public int Id { get; set; }
        /// <summary>Texto, no numero: '30', '60', ...</summary>
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int DiasOffset { get; set; }
        public int Orden { get; set; }
    }

    /// <summary>Una pregunta del catalogo, tal como sale de CA_preguntas.</summary>
    public class EntCurvaPregunta
    {
        public int Id { get; set; }
        public int EtapaId { get; set; }
        public string RolEvaluador { get; set; }
        public string Seccion { get; set; }
        public string TextoPregunta { get; set; }
        public string TipoRespuesta { get; set; }
        /// <summary>Arreglo JSON de textos, o null. Se parsea en CapaNegocio.</summary>
        public string OpcionesJson { get; set; }
        /// <summary>Lo llena CapaNegocio a partir de OpcionesJson.</summary>
        public List<string> Opciones { get; set; }
        public int Orden { get; set; }
        public bool Activa { get; set; }
    }
}
