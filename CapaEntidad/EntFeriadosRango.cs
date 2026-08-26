namespace CapaEntidad
{
    /// <summary>
    /// Feriados que caen dentro del rango de una solicitud de vacaciones.
    /// </summary>
    public class EntFeriadosRango
    {
        /// <summary>Feriados activos entre la fecha desde y la fecha hasta, ambas incluidas.</summary>
        public int Feriados { get; set; }

        /// <summary>
        /// Años del rango de los que no hay ningún feriado cargado, separados por coma.
        /// Vacío cuando están todos. La tabla Feriado se mantiene a mano, así que un año
        /// sin cargar devuelve 0 feriados: sin esta advertencia el colaborador perdería
        /// días sin que nadie se entere.
        /// </summary>
        public string AniosSinCargar { get; set; }
    }
}
