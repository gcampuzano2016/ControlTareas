namespace CapaEntidad
{
    /// <summary>Un estudio formal del colaborador.</summary>
    public class EntPerfilEstudio
    {
        public int IdEstudio { get; set; }
        public string Nivel { get; set; } = "";
        public string Institucion { get; set; } = "";
        public string Titulo { get; set; } = "";

        /// <summary>Nulo mientras no se sepa; nunca cero, que se leeria como dato real.</summary>
        public int? AnioGraduacion { get; set; }
    }
}
