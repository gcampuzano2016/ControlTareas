namespace CapaEntidad
{
    /// <summary>Un empleo anterior.</summary>
    public class EntPerfilExperiencia
    {
        public int IdExperiencia { get; set; }
        public string Empresa { get; set; } = "";
        public string Cargo { get; set; } = "";
        public int? AnioDesde { get; set; }

        /// <summary>Nulo significa "hasta hoy", no "no se sabe".</summary>
        public int? AnioHasta { get; set; }

        public string Funciones { get; set; } = "";
    }
}
